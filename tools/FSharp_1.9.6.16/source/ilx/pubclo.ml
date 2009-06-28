// (c) Microsoft Corporation. All rights reserved

#light

module Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Pubclo

open Internal.Utilities

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler.AbstractIL.Morphs 
open Microsoft.FSharp.Compiler.PrettyNaming

module Ildiag = Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types 
module Ilprint = Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 
module Illib = Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

open Ildiag
open Illib
open Il
open Ilx
open Ilprint
open Msilxlib 
open Ilxsettings

let add_mdef_generated_attrs_to_tdef ilg tdef = 
    { tdef with tdMethodDefs = tdef.tdMethodDefs |> dest_mdefs |> List.map (fun md -> md |> add_mdef_generated_attrs ilg) |> mk_mdefs }

// -------------------------------------------------------------------- 
// Erase closures and function types
// by compiling down to code pointers, classes etc.
// -------------------------------------------------------------------- 

let notlazy v = Lazy.CreateFromValue v
let logging = false 
let _ = if logging then dprintn "*** warning: Clo2_erase.logging is on"

let rec strip_upto n test dest x =
    if n = 0 then ([],x) else 
    if test x then 
        let l,r = dest x
        let ls,res = strip_upto (n-1) test dest r
        (l::ls),res
    else ([],x)

(* -------------------------------------------------------------------- 
 * Flags.  These need to match the various classes etc. in the 
 * ILX standard library, and the parts 
 * of the makefile that select the right standard library for a given
 * combination of flags.
 *
 * Beyond this, the translation inserts classes or value classes for 
 * the closure environment.  
 * -------------------------------------------------------------------- *)

let erase_method_pointers = ref true

type tyfunc_implementation = 
  | TyFuncCallVirt

let tyfunc_implementation = ref TyFuncCallVirt 

#if CLOSURES_VIA_POINTERS
let max_free_vars = ref 2 (* This turns out better than 4... Makes sense for generic code (fewer classes/RTTs), but can't think why for nongenericmorphic code. *)
let min_free_vars = ref 0
#endif

let dest_tyabsstruct = function Lambdas_forall(l,r) -> (l,r) | _ -> failwith "no"
let is_tyabsstruct   = function Lambdas_forall(l,r) -> true | _ -> false
let is_tyfunc_app    = function Apps_tyapp (b,c) ->true | _ -> false

let strip_tyabsstruct_upto n lambdas = strip_upto n is_tyabsstruct dest_tyabsstruct lambdas

(* -------------------------------------------------------------------- 
 * Three very important tables!!
 * -------------------------------------------------------------------- *)

let support_multilam() = true

(* Supported indirect calling conventions: *)
(* 0 *)
(* 0_1 *)
(* 0_1_1 *)
(* 0_1_1_1 *)
(* 1 *)
(* 1_1 *)
(* 1_1_1 *)
(* 1_1_1_1 *)
(* 1_1_1_1_1 *)
(* plus type applications - up to 7 in one step *)
(* Nb. later code currently takes advantage of the fact that term *)
(* and type applications are never mixed in a single step. *)
let strip_supported_indirect_call apps =
    match apps with 
    | Apps_app(x,Apps_app(y,Apps_app(z,Apps_app(w,Apps_app(v,rest))))) when support_multilam() -> [],[x;y;z;w;v],rest
    | Apps_app(x,Apps_app(y,Apps_app(z,Apps_app(w,rest))))             when support_multilam() -> [],[x;y;z;w],rest
    | Apps_app(x,Apps_app(y,Apps_app(z,rest)))                         when support_multilam() -> [],[x;y;z],rest
    | Apps_app(x,Apps_app(y,rest))                                     when support_multilam() -> [],[x;y],rest
    | Apps_app(x,rest) -> [],[x],rest
    | Apps_tyapp _  -> 
        let max_tyapps =  1
        let tys,rest =  strip_upto max_tyapps is_tyfunc_app dest_tyfunc_app apps
        tys,[],rest
    | rest -> [],[],rest

(* Supported conventions for baking closures: *)
(* 0 *)
(* 1 *)
(* 1_1 *)
(* 1_1_1 *)
(* 1_1_1_1 *)
(* 1_1_1_1_1 *)
(* plus type applications - up to 7 in one step *)
(* Nb. later code currently takes advantage of the fact that term *)
(* and type applications are never mixed in a single step. *)
let strip_supported_abstraction lambdas =
    match lambdas with 
    | Lambdas_lambda(x,Lambdas_lambda(y,Lambdas_lambda(z,Lambdas_lambda(w,Lambdas_lambda(v,rest))))) when support_multilam() -> [],[x;y;z;w;v],rest
    | Lambdas_lambda(x,Lambdas_lambda(y,Lambdas_lambda(z,Lambdas_lambda(w,rest)))) when support_multilam() -> [],[x;y;z;w],rest
    | Lambdas_lambda(x,Lambdas_lambda(y,Lambdas_lambda(z,rest)))                     when support_multilam() -> [],[x;y;z],rest
    | Lambdas_lambda(x,Lambdas_lambda(y,rest))                                         when support_multilam() -> [],[x;y],rest
    | Lambdas_lambda(x,rest) -> [],[x],rest
    | Lambdas_forall _ -> 
        let max_tyapps =  1
        let tys,rest = strip_tyabsstruct_upto max_tyapps lambdas
        tys,[],rest
    | rest -> [],[],rest

(* This must correspond to strip_supported_abstraction *)
let is_supported_direct_call = function 
  | Apps_app (_,Apps_done _)                                          when support_multilam() -> true
  | Apps_app (_,Apps_app (_, Apps_done _))                            when support_multilam() -> true
  | Apps_app (_,Apps_app (_,Apps_app (_, Apps_done _)))               when support_multilam()  -> true
  | Apps_app (_,Apps_app (_,Apps_app (_, Apps_app (_, Apps_done _)))) when support_multilam() -> true
  | Apps_tyapp _ -> false
  | _ -> false

(* -------------------------------------------------------------------- 
 * Prelude for function types.  Only use System.Func for now, prepare
 * for more refined types later.
 * -------------------------------------------------------------------- *)

let funcbase () = "FastFunc"
    
let tref_TyFunc () =  (mk_tref (Msilxlib.scoref (), Msilxlib.ilxNamespace () ^ ".TypeFunc"))
let tref_TyFuncStatics () =  (mk_tref (Msilxlib.scoref (), Msilxlib.ilxNamespace () ^ ".TyFuncStatics"))
let tspec_TyFunc () = mk_nongeneric_tspec (tref_TyFunc ())
let mk_typ_TyFunc () = Type_boxed (tspec_TyFunc())

let mk_tref_Func1 () = (mk_tref (Msilxlib.scoref (),Msilxlib.ilxNamespace () ^ "." ^ funcbase () ^ "`2"))
let mk_tref_Func1Statics () =  (mk_tref (Msilxlib.scoref (),Msilxlib.ilxNamespace () ^ ".FuncStatics"))

let mk_tref_Func(n) = 
    if n = 1 then mk_tref_Func1()
    else mk_nested_tref (Msilxlib.scoref (),
                         [Msilxlib.ilxNamespace () ^ ".OptimizedClosures"],
                         "FastFunc" ^ string n ^ "`"^ string (n + 1))

type  cenv = 
    { ilg:ILGlobals;
      tref_Func: ILTypeRef array;
      tref_TyFunc: ILTypeRef;
      typ_TyFunc: ILType;
      tref_Func1Statics: ILTypeRef }
  
let new_cenv(ilg) =
    { ilg=ilg;
      tref_Func= Array.init 10 (fun i -> mk_tref_Func(i+1));
      tref_TyFunc=tref_TyFunc();
      typ_TyFunc=mk_typ_TyFunc();
      tref_Func1Statics=mk_tref_Func1Statics() }

let tref_Func1 cenv = cenv.tref_Func.[0]

let typ_TyFunc cenv (x:ILGenericParameterDef) (y:ILType) = cenv.typ_TyFunc
  
let tspec_Func1 cenv dty rty = mk_tspec (cenv.tref_Func.[0],[dty;rty])
let typ_Func1 cenv dty rty = Type_boxed (tspec_Func1 cenv dty rty)
let typl_Func cenv dtys rty = List.foldBack (typ_Func1 cenv) dtys rty

let tref_Func cenv n = if n <= 10 then cenv.tref_Func.[n-1] else mk_tref_Func(n)   
let tspec_Func cenv dtys rty = mk_tspec (tref_Func cenv (List.length dtys),dtys @ [rty])
let typ_Func cenv dtys rty = Type_boxed (tspec_Func cenv dtys rty)

let rec type_of_apps cenv apps =
    match apps with 
    | Apps_tyapp (gactual,rest) -> cenv.typ_TyFunc
    | Apps_app (dty,rest) -> typ_Func1 cenv dty (type_of_apps cenv rest)
    | Apps_done rty -> rty

let rec typ_of_lambdas cenv lam = 
    match lam with 
    | Lambdas_return rty -> rty
    | Lambdas_lambda(d,r) -> typ_Func1 cenv d.Type (typ_of_lambdas cenv r)
    | Lambdas_forall(b,rest) -> cenv.typ_TyFunc

type spill_cases =
  | UnderOrNoSpill of IlxClosureFreeVar list * int
#if CLOSURES_VIA_POINTERS
  | OverSpill of IlxClosureFreeVar list * ILBoxity * IlxClosureFreeVar list
#endif


#if CLOSURES_VIA_POINTERS
(* -------------------------------------------------------------------- 
 * Spilling (VirtEntriesPtrCode only)
 * -------------------------------------------------------------------- *)

let mk_fvbundle_name nm = nm^"_FVspill"
let fvbundle_tref_of_tref (tref:ILTypeRef) = ILTypeRef.Create(Scope=tref.Scope, Enclosing=tref.Enclosing, Name=mk_fvbundle_name tref.Name)
let fvbundle_tref_of_clospec clospec = fvbundle_tref_of_tref (tref_of_clospec clospec)
    
let tspec_fvbundle fvtref inst = mk_tspec (fvtref,inst)
let typ_fvbundle boxed fvtref inst =  mk_typ boxed (tspec_fvbundle fvtref inst) 

let spill_cases fields =
    if !call_implementation = VirtEntriesVirtCode then UnderOrNoSpill (fields,0) else
    if List.length fields < !min_free_vars then 
        UnderOrNoSpill (fields, (!min_free_vars - List.length fields))
    else if List.length fields <= !max_free_vars then 
        UnderOrNoSpill (fields, 0)
    else 
        let a,b = chop (!max_free_vars - 1) fields
        OverSpill (a, AsObject,b)
          
let tspec_fvbundle_clospec clospec = tspec_fvbundle( fvbundle_tref_of_clospec clospec) (inst_of_clospec clospec)
let typ_fvbundle_clospec clospec = Type_boxed (tspec_fvbundle_clospec clospec)
#else
let spill_cases fields = UnderOrNoSpill (fields,0) 
#endif

(* -------------------------------------------------------------------- 
 * From closure refs to type refs (VirtEntriesVirtCode only)
 * -------------------------------------------------------------------- *)

let callvirt_tspec_Thunk rty = 
    let tref  = mk_tref(Msilxlib.scoref (),Msilxlib.ilxNamespace () ^ ".Thunk")
    mk_tspec(tref,[rty])
    
#if CLOSURES_VIA_POINTERS
(* -------------------------------------------------------------------- 
 * From closure refs to type refs (VirtEntriesPtrCode only)
 * -------------------------------------------------------------------- *)
  
let clotempl_fvbundle_fields cenv fvtref inst fields =
    if !call_implementation = VirtEntriesVirtCode then failwith "clotempl_fvbundle_fields cenv: call_implementation = VirtEntriesVirtCode";
    match spill_cases fields with 
    | UnderOrNoSpill (flds,n) -> List.map typ_of_freevar flds @ replicate n cenv.ilg.typ_Object
    | OverSpill (a,boxed,b) -> List.map typ_of_freevar a @ [ typ_fvbundle boxed fvtref inst ]
        
let clotempl_tspec_Func1Clo (dtys,rty,fields) = 
    let tref  = mk_tref(Msilxlib.scoref (),Msilxlib.ilxNamespace () ^ ".Func"^string (List.length dtys)^"Clo"^string (List.length fields))
    mk_tspec(tref,(dtys@(rty::fields)))

(* type reference for the function value itself *)
let clospec_to_clotempl_tspec cenv clospec = 
    match formal_lambdas_of_clospec clospec with 
    | Lambdas_forall _ | Lambdas_return _ -> clospec_to_class_tspec clospec
    | Lambdas_lambda _ -> 
        let tyargsl,argtys,r = strip_supported_abstraction (formal_lambdas_of_clospec clospec)
        assert (tyargsl = []);
        let d = argtys
        let d' = typs_of_params d
        let r' = typ_of_lambdas cenv r
        let f' = 
            let f = actual_freevars_of_clospec clospec
            let i = inst_of_clospec clospec
            clotempl_fvbundle_fields cenv (fvbundle_tref_of_clospec clospec) i f
        clotempl_tspec_Func1Clo (d',r',f')

let general_clotempl_type_of_clospec cenv clospec = 
    Type_boxed (clospec_to_clotempl_tspec cenv (generalize_cloref (gparams_of_inst (inst_of_clospec clospec)) (cloref_of_clospec clospec)))
#endif

(* type reference for the function value itself *)
let clospec_to_class_tspec clospec = mk_tspec(tref_of_clospec clospec,  inst_of_clospec clospec)

(* -------------------------------------------------------------------- 
 * Fields for free variables in closures
 * -------------------------------------------------------------------- *)

let fv_name fv = fv.fvName 

let clofld_field_name spill clospec n = 
#if CLOSURES_VIA_POINTERS
    if   spill 
      || (match !call_implementation with | VirtEntriesVirtCode  -> true | VirtEntriesPtrCode -> false) 
      || (match formal_lambdas_of_clospec clospec with | Lambdas_forall _  | Lambdas_return _ -> true | _ -> false) then 
#endif
      fv_name (List.nth (formal_freevars_of_clospec clospec) n)
#if CLOSURES_VIA_POINTERS
    else
      "_fv"^string (n+1)
#endif

let clospec_to_tspec cenv clospec =
    match formal_lambdas_of_clospec clospec with 
    | Lambdas_forall _ | Lambdas_return _ ->  clospec_to_class_tspec clospec
    | _ ->
#if CLOSURES_VIA_POINTERS
        match !call_implementation with 
        | VirtEntriesPtrCode -> clospec_to_clotempl_tspec cenv clospec
        | VirtEntriesVirtCode -> 
#endif
            clospec_to_class_tspec clospec

let mk_clofld_ref_clospec cenv clospec n =
    let tspec = clospec_to_tspec cenv clospec
    match formal_lambdas_of_clospec clospec with 
    | Lambdas_forall _  | Lambdas_return _ ->  
        let ty = formal_freevar_type_of_clospec clospec n
        mk_fspec_in_boxed_tspec (tspec,clofld_field_name false clospec n,ty)
    | Lambdas_lambda (d,_) ->
#if CLOSURES_VIA_POINTERS
        match !call_implementation with 
        | VirtEntriesPtrCode ->
            let tyargsl,argtys,r = strip_supported_abstraction (formal_lambdas_of_clospec clospec)
            assert (tyargsl = []);
            let d = argtys
            mk_fspec_in_boxed_tspec 
              (tspec,
               clofld_field_name false clospec n,
               Type_tyvar (uint16 (n+ (List.length d + 1))))
        | VirtEntriesVirtCode  -> 
#endif
            let ty = formal_freevar_type_of_clospec clospec n
            mk_fspec_in_boxed_tspec (tspec,clofld_field_name false clospec n,ty)

let mk_nonspill_clofld_ref_clospec cenv clospec n =
    mk_clofld_ref_clospec cenv clospec n

#if CLOSURES_VIA_POINTERS
let mk_spill_clofld_ref_clospec clospec nonspill_size field_num =
    let tspec = tspec_fvbundle_clospec clospec
    mk_fspec_in_boxed_tspec (tspec,clofld_field_name true clospec field_num,(nth (formal_freevars_of_clospec clospec) field_num).fvType)

let mk_fvbundle_fldref cenv clospec nonspill_size =
    mk_clofld_ref_clospec cenv clospec nonspill_size
#endif

(* -------------------------------------------------------------------- 
 * Method to call for a particular multi-application
 * -------------------------------------------------------------------- *)
    
let mspec_for_multi_value_app cenv (argtys',rty) =  
    let n = List.length argtys'
    let formal_argtys = List.mapi (fun i _ ->  Type_tyvar (uint16 i)) argtys'
    let formal_rty = Type_tyvar (uint16 n)
    let inst = argtys'@[rty]
    if List.length argtys' = 1  then 
      true, 
       (mk_nongeneric_instance_mspec_in_boxed_tspec
          (mk_tspec (tref_Func1 cenv,inst),"Invoke",formal_argtys, formal_rty))
    else 
       false, 
       (mk_static_mspec_in_boxed_tspec
          (tspec_Func1 cenv (List.hd inst) (List.hd (List.tl inst)),
           "InvokeFast",
           [typl_Func cenv formal_argtys formal_rty]@ (formal_argtys),
           formal_rty,
           List.tl (List.tl inst)))

let callblock_for_multi_value_app cenv do_tailcall (args',rty') inplab outlab =
    let callvirt,mr = mspec_for_multi_value_app cenv (args',rty')
    let instrs = [if callvirt then I_callvirt (do_tailcall,mr, None) else I_call (do_tailcall,mr, None)   ]
    if do_tailcall = Tailcall then nonbranching_instrs inplab instrs 
    else nonbranching_instrs_then_br inplab instrs outlab

let method_name_for_callclo_ptr nm = nm 

let mspec_for_callclo_virt cenv clospec = 
    let tyargsl,argtys,rstruct = strip_supported_abstraction (formal_lambdas_of_clospec clospec)
    if tyargsl <> [] then failwith "mspec_for_callclo_virt: internal error";
    let rty' = typ_of_lambdas cenv rstruct
    let argtys' = typs_of_params argtys
    let minst' = inst_of_clospec clospec
    (mk_instance_mspec_in_boxed_tspec(clospec_to_tspec cenv clospec,"Invoke",argtys',rty',minst'))

#if CLOSURES_VIA_POINTERS
let class_name_for_apply_meths = "<ClosureMethods>"

let mspec_for_callclo_ptr cenv clospec = 
    let tyargsl,argtys,rstruct = strip_supported_abstraction (formal_lambdas_of_clospec clospec)
    if tyargsl <> [] then failwith "mspec_for_callclo_ptr: internal error";
    let rty' = typ_of_lambdas cenv rstruct
    let argtys' = typs_of_params argtys
    let minst' = inst_of_clospec clospec
    let formal_clo_typ = general_clotempl_type_of_clospec cenv clospec
    mk_static_mspec_in_nongeneric_boxed_tref
      (mk_tref (ScopeRef_local, class_name_for_apply_meths ),
       method_name_for_callclo_ptr (tref_of_clospec clospec).Name,
       ([formal_clo_typ]@argtys'),rty',minst')
#endif

let mspec_for_callclo cenv clospec = 
#if CLOSURES_VIA_POINTERS
    match !call_implementation with 
    | VirtEntriesPtrCode -> mspec_for_callclo_ptr cenv clospec
    | VirtEntriesVirtCode -> 
#endif
        mspec_for_callclo_virt cenv clospec

(* -------------------------------------------------------------------- 
 * Translate instructions....
 * -------------------------------------------------------------------- *)

#if CLOSURES_VIA_POINTERS
let mk_callptr_baker_mspec cenv clospec actual_argtys' actual_rty' actual_fields' = 
    let nargs = List.length actual_argtys'
    let nfields = List.length actual_fields'
    let name_of_baker = "bake"^string nargs^"_"^string nfields
    let nfunc_tyvars = nargs+1
    let formal_field_tyvars = List.map (fun n -> Type_tyvar (uint16 (n+nfunc_tyvars-1))) (range 1 nfields)
    let formal_ret_tyvar = Type_tyvar (uint16 nargs)
    let args = List.map (fun n -> Type_tyvar (uint16 n)) (range 0 (nargs-1))
    let formal_rtyp_of_baker = typl_Func cenv args formal_ret_tyvar
    let formal_argtys_of_baker = 
        formal_field_tyvars@
        [ if !erase_method_pointers then cenv.ilg.typ_UIntPtr 
          else Type_fptr (mk_callsig(ILCallingConv.Static, [ formal_rtyp_of_baker  ] @ args,formal_ret_tyvar))]

    let minst = actual_argtys'@(actual_rty' :: actual_fields') 
    (* intern_mspec cenv.m *) 
     (mk_static_mspec_in_nongeneric_boxed_tref(cenv.tref_Func1Statics,name_of_baker,formal_argtys_of_baker,formal_rtyp_of_baker,minst))
#endif

let mk_ldfldg lda = (if lda then mk_normal_ldflda else mk_normal_ldfld)
#if CLOSURES_VIA_POINTERS
let mk_ldfld_spill boxed = (if boxed = AsObject then mk_normal_ldfld else mk_normal_ldflda)
#endif

let conv_stclofld cenv tmps clospec n =
    match formal_lambdas_of_clospec clospec with 
    | Lambdas_forall _ | Lambdas_return _ -> 
        [ mk_normal_stfld (mk_nonspill_clofld_ref_clospec cenv clospec n) ]
    | Lambdas_lambda _ -> 
        match spill_cases (actual_freevars_of_clospec clospec) with 
        | UnderOrNoSpill _ ->
            [ mk_normal_stfld (mk_nonspill_clofld_ref_clospec cenv clospec n) ]
#if CLOSURES_VIA_POINTERS
        | OverSpill (a,_,_) when n < List.length a -> 
            [ mk_normal_stfld (mk_nonspill_clofld_ref_clospec cenv clospec n) ]
        | OverSpill (a,boxed,b) -> 
            let stored_item_fspec' = mk_spill_clofld_ref_clospec clospec (List.length a) n
            let stored_item_typ = actual_typ_of_fspec stored_item_fspec'
            let env_fspec = mk_fvbundle_fldref cenv clospec (List.length a)
            let locn = alloc_tmp tmps (mk_local stored_item_typ)
            [ I_stloc locn;
              mk_ldfld_spill boxed env_fspec; 
              I_ldloc locn;
              mk_normal_stfld stored_item_fspec' ]
#endif

let conv_ldenv cenv lda clospec n = 
    match formal_lambdas_of_clospec clospec with 
    | Lambdas_forall _ | Lambdas_return _ -> 
        [ ldarg_0;
          mk_ldfldg lda (mk_nonspill_clofld_ref_clospec cenv clospec n) ]
    | Lambdas_lambda _ -> 
        match spill_cases (formal_freevars_of_clospec clospec) with 
        | UnderOrNoSpill (a,_) 
#if CLOSURES_VIA_POINTERS
        | OverSpill (a,_,_) 
#endif
         when n < List.length a  -> 
            [ ldarg_0;
              mk_ldfldg lda (mk_nonspill_clofld_ref_clospec cenv clospec n) ]
#if CLOSURES_VIA_POINTERS
        | OverSpill (a,boxed,b) ->
            [ (* load up a reference to the spill bundle *)
              ldarg_0; 
              mk_ldfld_spill boxed (mk_fvbundle_fldref cenv clospec (List.length a));
              (* load the field or field address from the spill bundle via the reference *)
              mk_ldfldg lda (mk_spill_clofld_ref_clospec clospec (List.length a) n) ]
#endif
          | UnderOrNoSpill _ -> failwith "Unexpected"

let rec conv_instr cenv tmps ((this_genparams,this_clo) as this_info) inplab outlab instr = 
    match instr with 
    |  (EI_ilzero typ) -> 
          let n = alloc_tmp tmps (mk_local  typ)
          Choice1Of2 [ I_ldloca n;  I_initobj typ; I_ldloc n ]
    | I_other e when is_ilx_ext_instr e -> 
        match (dest_ilx_ext_instr e) with 
        |  (EI_castclo clospec) -> Choice1Of2 [ I_castclass (Type_boxed (clospec_to_tspec cenv clospec)) ]
              
        |  (EI_isclo clospec)   -> Choice1Of2 [ I_isinst (Type_boxed (clospec_to_tspec cenv clospec)) ]
        |  (EI_newclo clospec) ->
            let cspec = clospec_to_tspec cenv clospec
            match formal_lambdas_of_clospec clospec with 
            | Lambdas_forall _ | Lambdas_return _ -> 
                let fields = formal_freevars_of_clospec clospec
                let maker_mspec = (* intern_mspec cenv.m *) (mk_ctor_mspec_for_boxed_tspec (cspec,List.map (typ_of_freevar) fields))
                Choice1Of2 [ I_newobj (maker_mspec,None) ]
            | Lambdas_lambda _ ->
#if CLOSURES_VIA_POINTERS
                match !call_implementation with
                | VirtEntriesPtrCode -> 
                    let code_mspec = mspec_for_callclo_ptr cenv clospec
                    
                    let formal_tyargsl,formal_argtys,formal_rstruct = strip_supported_abstraction (formal_lambdas_of_clospec clospec)
                    if formal_tyargsl <> [] then failwith "newclo: internal error - unexpected type abstraction";
                    let formal_rty' = typ_of_lambdas cenv formal_rstruct
                    let formal_argtys' = typs_of_params formal_argtys
                    
                    let actual_tyargsl,actual_argtys,actual_rstruct = strip_supported_abstraction (actual_lambdas_of_clospec clospec)
                    if actual_tyargsl <> [] then failwith "newclo: internal error - unexpected type abstraction";
                    let actual_rty' = typ_of_lambdas cenv actual_rstruct
                    let actual_argtys' = typs_of_params actual_argtys
                    
                    let formal_fields' = List.map (typ_of_freevar) (formal_freevars_of_clospec clospec)
                    let actual_fields' = actual_freevars_of_clospec clospec
                    
                    let inst' = inst_of_clospec clospec
                    let ld_env = 
                      match spill_cases actual_fields' with 
                      | UnderOrNoSpill (_,n) -> replicate n (I_arith AI_ldnull)
                      | OverSpill (_,boxed,b) -> 
                          match spill_cases (formal_freevars_of_clospec clospec) with 
                          | OverSpill (_,_,formal_b) -> 
                              [ mk_normal_newobj(mk_ctor_mspec(fvbundle_tref_of_clospec clospec,boxed,List.map (typ_of_freevar) formal_b,inst')) ]
                          |       _ -> failwith "internal error: ld_env"
                          end
                    let baker_mspec = 
                      let bundled_fields' = clotempl_fvbundle_fields cenv (fvbundle_tref_of_clospec clospec) inst' actual_fields'
                      mk_callptr_baker_mspec cenv clospec actual_argtys' actual_rty' bundled_fields'
                    
                    Choice1Of2 (ld_env @ [ (mk_IlxInstr (EI_ldftn_then_call (code_mspec, (Normalcall,baker_mspec,None)))) ])
                | VirtEntriesVirtCode -> 
#endif
                    let fields = formal_freevars_of_clospec clospec
                    let maker_mspec = (* intern_mspec cenv.m *) (mk_ctor_mspec_for_boxed_tspec (cspec,List.map (typ_of_freevar) fields))
                    Choice1Of2 [ I_newobj (maker_mspec,None) ]
              
        |  (EI_stclofld (clospec,n)) -> 
            Choice1Of2 (conv_stclofld cenv tmps clospec n)
              
        |  (EI_ldenv ( n)) -> 
            match this_clo with 
            | None -> failwith "ldenv in non-closure"
            | Some (_,clospec) -> Choice1Of2 (conv_ldenv cenv false clospec n)
            
        |  (EI_stenv ( n)) -> 
            match this_clo with 
            | None -> failwith "stenv in non-closure"
            | Some (_,clospec) -> Choice1Of2 (conv_stclofld cenv tmps clospec n)
            
        |  (EI_ldenva ( n)) -> 
            match this_clo with 
            | None -> failwith "ldenv in non-closure"
            | Some (_,clospec) -> Choice1Of2 (conv_ldenv cenv true clospec n)
            
        | i when (match i with  EI_callfunc _ -> true | EI_callclo _ -> true | _ -> false) ->
            (* "callfunc" and "callclo" instructions become a series of indirect *)
            (* calls or a single direct call.   *)
            let var_count = List.length this_genparams 
            let tl,apps,direct_clo = 
              match i with 
              | EI_callfunc (tl,apps) -> tl,apps,None
              | EI_callclo (tl,clospec,apps) when not (is_supported_direct_call apps) -> tl,apps,None 
              | EI_callclo (tl,clospec,apps) -> tl,apps,Some clospec 
              | _ -> failwith "Unexpected call instruction"
            match direct_clo with 
            | Some clospec ->  Choice1Of2 [I_call (tl, mspec_for_callclo cenv clospec,None) ]
            | None -> 
                
              (* Unwind the stack until the arguments given in the apps have *)
              (* all been popped off.  The apps given to this function is *)
              (* what remains after the first "strip" of suitable arguments for the *)
              (* first call. *)
              (* Loaders and storers are returned in groups.  Storers are used to pop *)
              (* the arguments off the stack that correspond to all the arguments in *)
              (* the apps, and the loaders are used to load them back on.  *)
                let rec unwind apps = 
                    match apps with 
                    | Apps_tyapp (actual,rest) -> 
                        let rest = inst_apps_aux var_count [actual] rest
                        let storers,loaders = unwind rest
                        [] :: storers, [] :: loaders 
                    | Apps_app (arg,rest) -> 
                        let storers, loaders =unwind rest
                        let args_storers,args_loaders = 
                              let locn = alloc_tmp tmps (mk_local arg)
                              [I_stloc locn], [I_ldloc locn]
                        args_storers :: storers, args_loaders :: loaders  
                    | Apps_done _ -> 
                        [],[]
                
                let rec compute_precall fst n rest (loaders: ILInstr list) = 
                    if fst then 
                      let storers,(loaders2 : ILInstr list list) =  unwind rest
                      (List.rev (List.concat storers) : ILInstr list) , ((List.concat loaders2) : ILInstr list)
                    else 
                      strip_upto n (function (x::y) -> true | _ -> false) (function (x::y) -> (x,y) | _ -> failwith "no!") loaders
                
                let rec build_app fst loaders apps inplab outlab =
                    // Strip off one valid indirect call.  [fst] indicates if this is the 
                    // first indirect call we're making. The code below makes use of the 
                    // fact that term and type applications are never currently mixed for 
                    // direct calls. 
                    match strip_supported_indirect_call apps with 
                    // Type applications: REVIEW: get rid of curried tyapps - just tuple them 
                    | tyargs,[],_ when tyargs <> [] ->
                        // strip again, instantiating as we go.  we could do this while we count. 
                        let (rev_inst_tyargs, rest') = 
                            List.fold 
                                (fun (rev_args_sofar,cs) _  -> 
                                  let actual,rest' = dest_tyfunc_app cs
                                  let rest'' = inst_apps_aux var_count [actual] rest'
                                  ((actual :: rev_args_sofar),rest''))
                                ([],apps)
                                tyargs
                        let inst_tyargs = List.rev rev_inst_tyargs
                        let precall,loaders' = compute_precall fst 0 rest' loaders
                        let more = (match rest' with Apps_done rty -> rty = Type_void | _ -> true)
                        let do_tailcall = and_tailness tl false
                        let instrs1 = 
                            precall @
                            [ I_callvirt (do_tailcall, 
                                          (* intern_mspec cenv.m *) (mk_instance_mspec_in_nongeneric_boxed_tref (cenv.tref_TyFunc,
                                                                                      "Specialize",
                                                                                      [],
                                                                                      cenv.ilg.typ_Object, 
                                                                                      inst_tyargs)), None) ]
                        let instrs1 =                        
                            (* TyFunc are represented as Specialize<_> methods returning an object.                            
                             * For value types, recover result via unbox and load.
                             * For reference types, recover via cast.
                             *)                            
                            let rtn_ty = type_of_apps cenv rest'
                            instrs1 @ [ I_unbox_any rtn_ty]
                        if do_tailcall = Tailcall then nonbranching_instrs inplab instrs1 
                        else 
                            let end_of_callblock = generate_code_label ()
                            let block1 = nonbranching_instrs_then_br inplab instrs1 end_of_callblock
                            let block2 = build_app false loaders' rest' end_of_callblock outlab
                            mk_group_block ([end_of_callblock],[ block1; block2 ])

                  (* Term applications *)
                    | [],args,rest when args <> [] -> 
                        let precall,loaders' = compute_precall fst (List.length args) rest loaders
                        let is_last = (match rest with Apps_done _ -> true | _ -> false)
                        let rty  = (type_of_apps cenv rest)
                        let args' = args
                        let rty' = rty
                        let do_tailcall = and_tailness tl is_last

                        let start_of_callblock = generate_code_label ()
                        let precall_block = nonbranching_instrs_then_br inplab precall start_of_callblock

                        if do_tailcall = Tailcall then 
                            let callblock =  callblock_for_multi_value_app cenv do_tailcall (args',rty') start_of_callblock outlab
                            mk_group_block ([start_of_callblock],[ precall_block; callblock ])
                        else
                            let end_of_callblock = generate_code_label ()
                            let callblock =  callblock_for_multi_value_app cenv do_tailcall (args',rty') start_of_callblock end_of_callblock
                            let rest_block = build_app false loaders' rest end_of_callblock outlab
                            mk_group_block ([start_of_callblock; end_of_callblock],[ precall_block; callblock; rest_block ])

                    | [],[],Apps_done rty -> 
                        (* "void" return values are allowed in function types *)
                        (* but are translated to empty value classes.  These *) 
                        (* values need to be popped. *)
                        nonbranching_instrs_then inplab ([]) (if tl = Tailcall then I_ret else I_br outlab)
                    | _ -> failwith "*** Error: internal error: unknown indirect calling convention returned by strip_supported_indirect_call"
                 
                Choice2Of2 (build_app true [] apps inplab outlab)
        | _ ->  Choice1Of2 [instr]  
          
    | _ ->  Choice1Of2 [instr] 

// Fix up I_ret instruction. Generalise to selected instr.
let conv_ret_instr returnInstrs inplab outlab instr = 
    match instr with 
    | I_ret -> Choice1Of2 returnInstrs
    | _     -> Choice1Of2 [instr]
        
let conv_ilmbody cenv (this_genparams,this_clo,box_return_ty) il = 
    let tmps = new_tmps (List.length il.ilLocals)
    let locals = il.ilLocals
    (* Add a local to keep the result value of a thunk while storing it *)
    (* into the result field and returning it. *)
    (* Record the local slot number in the environment passed in this_clo *)
    let this_clo, new_max = 
        match this_clo with 
        | Some clospec -> Some(List.length locals, clospec),il.ilMaxStack+2 (* for calls *)
        | None -> None,il.ilMaxStack
    let code' = topcode_instr2code (conv_instr cenv tmps (this_genparams,this_clo)) il.ilCode
    let code' = match box_return_ty with
                | None    -> code'
                | Some ty -> (* box before returning? e.g. in the case of a TyFunc returning a struct, which compiles to a Specialise<_> method returning an object *)
                             topcode_instr2code (conv_ret_instr [I_box ty;I_ret]) code'
    {il with ilMaxStack=new_max;  
             ilZeroInit=true;
             ilCode= code' ;
             ilLocals = locals @ get_tmps tmps }

let conv_mbody cenv (this_genparams,this_clo) = function
    | MethodBody_il il -> MethodBody_il (conv_ilmbody cenv (this_genparams,this_clo,None) il)
    | x -> x

let conv_mdef cenv (this_genparams,this_clo) md  =
    let b' = conv_mbody cenv ((this_genparams @ md.mdGenericParams) ,this_clo) (dest_mbody md.mdBody)
    {md with mdBody=mk_mbody b'}

(* -------------------------------------------------------------------- 
 * Build an apply method of the appropriate gparams.   
 * -------------------------------------------------------------------- *)

#if CLOSURES_VIA_POINTERS
let mk_callptr_mdef cenv this_clospec this_genparams this_name (pure_argtys,pure_rty) code = 
    mk_static_mdef
        (this_genparams,
         method_name_for_callclo_ptr this_name,
         MemAccess_public,
         (mk_named_param ("this",(Type_boxed (clospec_to_clotempl_tspec cenv this_clospec))) ::pure_argtys),
         mk_return pure_rty,
         conv_mbody cenv (this_genparams,Some(this_clospec)) (MethodBody_il code))
#endif

(* -------------------------------------------------------------------- 
 * Make fields for free variables of a type abstraction.
 *   REVIEW: change type abstractions to use other closure mechanisms.
 * -------------------------------------------------------------------- *)

let freevar_of_param p = 
    let nm = (match p.paramName with Some x -> x | None -> failwith "closure parameters must be given names")
    mk_freevar(nm, false,p.paramType)
let local_of_param p = mk_local(p.paramType)

let mk_genclass_clofld_specs cenv flds = 
    flds |> List.map (fun fv -> (fv.fvName,fv.fvType)) 

let mk_genclass_clofld_defs cenv flds = 
    flds 
    |> List.map (fun fv -> 
         let fdef = mk_instance_fdef (fv.fvName,fv.fvType,None,MemAccess_public)
         if fv.fvCompilerGenerated then 
             fdef |> add_fdef_never_attrs cenv.ilg
                  |> add_fdef_generated_attrs cenv.ilg
         else
             fdef)

(* -------------------------------------------------------------------- 
 * Convert a closure.  Split and chop if there are too many arguments,
 * otherwise build the appropriate kind of thing depending on whether
 * it's a type abstraction or a term abstraction.
 * -------------------------------------------------------------------- *)

let rec conv_clodef cenv mdefGen encl td clo = 
    if logging then dprintn ("//--- clo2_erase.ml, type "^  td.tdName);
    let new_tdefs,new_mdefs = 
      (* the following are shared between cases 1 && 2 *)
      let now_name = td.tdName
      let now_gparams = td.tdGenericParams
      let now_fields = clo.cloFreeVars
      let now_tref =  mk_nested_tref (ScopeRef_local, encl, now_name)
      let now_tspec = generalize_tref now_tref now_gparams
      let now_cloref = IlxClosureRef(now_tref,clo.cloStructure,now_fields)
      let now_clospec = generalize_cloref now_gparams now_cloref
      let tagClo = clo.cloSource
      let tagApp = (Lazy.force clo.cloCode).ilSource
      
      let tyargsl,tmargsl,later_struct = strip_supported_abstraction clo.cloStructure
      let later_access = td.tdAccess in (* (if td.tdAccess = TypeAccess_public then TypeAccess_nested MemAccess_public else TypeAccess_nested MemAccess_assembly) in*)

            (* Adjust all the argument and environment accesses *)
      let fix_code saved_args  = 
        let nenv = List.length now_fields
        let il = Lazy.force clo.cloCode
        let nlocs = List.length il.ilLocals
        let conv_instr_for_later instr =
          let fix_arg mk_env mk_arg n = 
            let rec find_matchin_arg l c = 
              match l with 
              | ((m,_)::t) -> 
                  if n = m then mk_env c
                  else find_matchin_arg t (c+1)
              | [] -> (mk_arg (n - List.length saved_args + 1))
            (find_matchin_arg saved_args 0)
          match instr with 
          | I_ldarg n -> 
              fix_arg 
                (fun x -> [ I_ldloc (uint16 (x+nlocs)) ]) 
                (fun x -> [ I_ldarg (uint16 x )])
                (int n)
          | I_starg n -> 
              fix_arg 
                (fun x -> [ I_stloc (uint16 (x+nlocs)) ]) 
                (fun x -> [ I_starg (uint16 x) ])
                (int n)
          | I_ldarga n ->  
              fix_arg 
                (fun x -> [ I_ldloca (uint16 (x+nlocs)) ]) 
                (fun x -> [ I_ldarga (uint16 x) ]) 
                (int n)
          | i ->  [i]
        let main_code = topcode_instr2instrs conv_instr_for_later il.ilCode
        let ldenv_code = List.concat (List.mapi (fun n _ -> [ (mk_IlxInstr (EI_ldenv (n + nenv))); I_stloc (uint16 (n+nlocs)) ]) saved_args)
        let code = prepend_instrs_to_code ldenv_code main_code
        
        {il with 
             ilCode=code;
             ilLocals=il.ilLocals @ List.map (snd >> local_of_param) saved_args; 
                          (* maxstack may increase by 1 due to environment loads *)
             ilMaxStack=il.ilMaxStack+1 }


      match tyargsl,tmargsl,later_struct with 
      // CASE 1 - Type abstraction 
      | (_ :: _), [],_ ->
          let added_genparams = tyargsl
          let now_rty = (typ_of_lambdas cenv later_struct)
          let purity = List.length tyargsl
          
        // CASE 1a. Split a type abstraction. 
        // Adjust all the argument and environment accesses 
        // Actually that special to do here in the type abstraction case 
        // nb. should combine the term and type abstraction cases for  
        // to allow for term and type variables to be mixed in a single 
        // application. 
          if (match later_struct with Lambdas_return _ -> false | _ -> true) then 
            
            let now_struct = List.foldBack (fun x y -> Lambdas_forall(x,y)) tyargsl (Lambdas_return now_rty)
            let later_name = now_name^"T"
            let later_tref = mk_nested_tref (ScopeRef_local,encl,later_name)
            let later_gparams = now_gparams @ added_genparams
            let self_ty = (typ_of_lambdas cenv now_struct)
            let later_fields =  now_fields @ [mk_freevar(CompilerGeneratedName ("self"^string now_fields.Length),true,self_ty)]
            let later_clospec = 
              let later_cloref = IlxClosureRef(later_tref,later_struct,later_fields)
              generalize_cloref later_gparams later_cloref
            
            let laterCode = fix_code [(0, mk_named_param (CompilerGeneratedName "self", self_ty))]
            let laterTypeDefs = 
              conv_clodef cenv mdefGen encl
                {td with tdGenericParams=later_gparams;
                          tdAccess=later_access;
                          tdName=later_name} 
                {clo with cloStructure=later_struct;
                          cloFreeVars=later_fields;
                          cloCode=notlazy laterCode}
            
            let laterTypeDefs = laterTypeDefs |>  List.map (add_mdef_generated_attrs_to_tdef cenv.ilg)

          // This is the code which will get called when then "now" 
          // arguments get applied. Convert it with the information 
          // that it is the code for a closure... 
            let nowCode = 
              mk_ilmbody
                (false,[],List.length now_fields + 1,
                 nonbranching_instrs_to_code
                   begin 
                 (* Load up the environment, including self... *)
                     List.mapi (fun n _ -> (mk_IlxInstr (EI_ldenv  n))) now_fields @
                     [ ldarg_0 ] @
                 (* Make the instance of the delegated closure && return it. *)
                 (* This passes the method type params. as class type params. *)
                     [ (mk_IlxInstr (EI_newclo later_clospec)) ] 
                   end,
                 tagApp)

            let nowTypeDefs = 
              conv_clodef cenv mdefGen encl
                td {clo with cloStructure=now_struct; 
                             cloCode=notlazy nowCode}
            nowTypeDefs @ laterTypeDefs, []
          else 
         (* CASE 1b. Build a type application. *)
         (* Currently the sole mbody defines a class and uses *)
         (* virtual methods. *)
              let box_return_ty = Some now_rty in (* box prior to all I_ret *)
              let now_apply_mdef =
                mk_generic_virtual_mdef
                  ("Specialize",
                   MemAccess_public,
                   added_genparams,  (* method is generic over added ILGenericParameterDefs *)
                   [],
                   mk_return(cenv.ilg.typ_Object),
                   MethodBody_il (conv_ilmbody cenv (now_gparams@added_genparams,Some now_clospec,box_return_ty)
                                    (Lazy.force clo.cloCode)))
              let ctor_mdef = 
                mk_storage_ctor 
                  (tagClo,
                   ([ ldarg_0] @
                     (match !tyfunc_implementation with 
                     | TyFuncCallVirt ->
                         [ mk_normal_call 
                             ((* intern_mspec cenv.m *) (mk_ctor_mspec_for_boxed_tspec (tspec_of_typ cenv.typ_TyFunc, []))) ])),
                   now_tspec,
                   mk_genclass_clofld_specs cenv now_fields,
                   MemAccess_assembly)
              let cloclass = 
                { tdName = now_name;
                  tdGenericParams= now_gparams;
                  tdAccess=td.tdAccess;
                  tdImplements = [];
                  tdAbstract = false;
                  tdNested = mk_tdefs [];
                  tdSealed = false;
                  tdSerializable=td.tdSerializable; 
                  tdComInterop=false;
                  tdSpecialName=false;
                  tdLayout=TypeLayout_auto;
                  tdEncoding=TypeEncoding_ansi;
                  tdInitSemantics=TypeInit_beforefield;
                  tdExtends= Some (cenv.typ_TyFunc);
                  tdMethodDefs= mk_mdefs ([ctor_mdef] @ [now_apply_mdef]); 
                  tdFieldDefs= mk_fdefs (mk_genclass_clofld_defs cenv now_fields);
                  tdCustomAttrs=mk_custom_attrs [];
                  tdMethodImpls=mk_mimpls [];
                  tdProperties=mk_properties [];
                  tdEvents=mk_events [];
                  
                  tdHasSecurity=false; 
                  tdSecurityDecls=mk_security_decls []; 
                  tdKind = TypeDef_class;}
              [ cloclass], []

    (* CASE 2 - Term Application *)
      |  [], (_ :: _ as now_params),_ ->
          let now_rty = typ_of_lambdas cenv later_struct
          let purity = List.length now_params
          
         (* CASE 2a - Too Many Term Arguments or Remaining Type arguments - Split the Closure Class in Two *)
          if (match later_struct with Lambdas_return _ -> false | _ -> true) then 
              let now_struct = List.foldBack (fun l r -> Lambdas_lambda(l,r)) now_params (Lambdas_return now_rty)
              let later_name = now_name^"D"
              let later_tref = mk_nested_tref (ScopeRef_local,encl,later_name)
              let later_gparams = now_gparams
            (* Number each argument left-to-right, adding one to account for the "this" pointer*)
              let self = mk_named_param (CompilerGeneratedName "self", Type_boxed (clospec_to_tspec cenv now_clospec))
              let now_params_nums = (0, self) :: List.mapi (fun i x -> i+1, x) now_params
              let saved_args =  now_params_nums
              let later_fields = now_fields@(List.map (fun (n,p) -> freevar_of_param p) saved_args)
              let later_cloref = IlxClosureRef(later_tref,later_struct,later_fields)
              let later_clospec = generalize_cloref later_gparams later_cloref
        (* This is the code which will first get called. *)
              let nowCode = 
                  mk_ilmbody
                    (false,[],List.length saved_args + List.length now_fields,
                     nonbranching_instrs_to_code
                       begin 
                   (* Load up the environment *)
                         List.mapi (fun n _ -> (mk_IlxInstr (EI_ldenv n))) now_fields @
                   (* Load up all the arguments (including self), which become free variables in the delegated closure *)
                         List.map (fun (n,ty) -> I_ldarg (uint16 n)) saved_args @
                   (* Make the instance of the delegated closure && return it. *)
                         [ (mk_IlxInstr (EI_newclo later_clospec)) ] 
                       end,
                     tagApp)
              let nowTypeDefs = 
                conv_clodef cenv mdefGen encl
                  td
                  {clo with cloStructure=now_struct;
                            cloCode=notlazy nowCode}
              let laterCode = fix_code saved_args
              let laterTypeDefs = 
                conv_clodef cenv mdefGen encl
                  {td with tdGenericParams=later_gparams;
                             tdAccess=later_access;
                             tdName=later_name} 
                  {clo with cloStructure=later_struct;
                        cloFreeVars=later_fields;
                        cloCode=notlazy laterCode}
              // add 'compiler generated' to all the methods in the 'now' classes
              let laterTypeDefs = laterTypeDefs |>  List.map (add_mdef_generated_attrs_to_tdef cenv.ilg)
              nowTypeDefs @ laterTypeDefs, []
                        
          else 
          (* CASE 2b - Build an Term Application Apply method *)
          (* CASE 2b1 - Use ldftn. *)
          (* Define a class to hold the free variables if the number *)
          (* exceeds the maximum.  All the variables get held in the class, which *)
          (* is a little perverse... *)
#if CLOSURES_VIA_POINTERS
            match !call_implementation with 
            | VirtEntriesPtrCode ->
                let spill_classes = 
                  match spill_cases now_fields with 
                  | UnderOrNoSpill (_,_) -> []
                  | OverSpill (a,boxed,spill_fields') -> 
                      let spill_tref = fvbundle_tref_of_clospec now_clospec
                      let spill_genparams = now_gparams
                      let spill_tspec = generalize_tref spill_tref spill_genparams
                      let ctor_mdef = mk_simple_storage_ctor(tagClo, Some(cenv.ilg.tspec_Object), spill_tspec, mk_genclass_clofld_specs cenv spill_fields', MemAccess_assembly)
                      [ { tdName = spill_tref.Name;
                          tdAccess= td.tdAccess;
                          tdGenericParams= now_gparams;
                          tdAbstract = false;
                          tdSealed = true;
                          tdNested = mk_tdefs []; tdImplements = [];
                          tdSerializable=td.tdSerializable; 
                          tdComInterop=false;
                          tdSpecialName=false;
                          tdLayout=TypeLayout_auto;
                          tdEncoding=TypeEncoding_ansi;
                          tdInitSemantics=TypeInit_beforefield;
                          tdExtends = Some(cenv.ilg.typ_Object);
                          tdMethodDefs = mk_mdefs [ctor_mdef];
                          tdFieldDefs = mk_genclass_clofld_defs cenv spill_fields';
                          tdCustomAttrs=mk_custom_attrs [];
                          tdMethodImpls=mk_mimpls [];
                          tdProperties=mk_properties [];
                          tdEvents=mk_events [];
                          tdHasSecurity=false; 
                          tdSecurityDecls=mk_security_decls []; 
                          tdKind = TypeDef_class; } ]
                spill_classes,
                [mk_callptr_mdef cenv now_clospec now_gparams now_name (now_params,now_rty) (Lazy.force clo.cloCode)] 
            | VirtEntriesVirtCode ->
#endif
                (* CASE 2b2. Build a term application as a virtual method. *)
                
                let now_env_parent_class = typ_Func cenv (typs_of_params now_params) now_rty 
                let cloclass = 
                    let now_apply_mdef =
                        mk_virtual_mdef
                          ("Invoke",MemAccess_public,
                           now_params, 
                           mk_return now_rty,
                           MethodBody_il (conv_ilmbody cenv (now_gparams,Some now_clospec,None)  (Lazy.force clo.cloCode)))
                    let ctor_mdef = 
                        mk_storage_ctor 
                           (tagClo,
                            (* how to call superclass constructor - should be added to pp_erase.ml? *)
                            [ ldarg_0; 
                              mk_normal_call ((* intern_mspec cenv.m *) (mk_ctor_mspec_for_boxed_tspec (tspec_of_typ now_env_parent_class,[]))) ],
                            now_tspec,
                            mk_genclass_clofld_specs cenv now_fields,
                            MemAccess_assembly)
                    { tdName = now_name;
                      tdGenericParams= now_gparams;
                      tdAccess = td.tdAccess;
                      tdImplements = [];
                      tdAbstract = false;
                      tdSealed = false;
                      tdSerializable=td.tdSerializable; 
                      tdComInterop=false;
                      tdSpecialName=false;
                      tdLayout=TypeLayout_auto;
                      tdEncoding=TypeEncoding_ansi;
                      tdInitSemantics=TypeInit_beforefield;
                      tdNested = mk_tdefs []; 
                      tdExtends= Some now_env_parent_class;
                      tdMethodDefs= mk_mdefs ([ctor_mdef] @ [now_apply_mdef]); 
                      tdFieldDefs= mk_fdefs (mk_genclass_clofld_defs cenv now_fields);
                      tdCustomAttrs=mk_custom_attrs [];
                      tdMethodImpls=mk_mimpls [];
                      tdProperties=mk_properties [];
                      tdEvents=mk_events [];
                      tdHasSecurity=false; 
                      tdSecurityDecls=mk_security_decls []; 
                      tdKind = TypeDef_class; } 
                [cloclass],[]
      |  [],[],Lambdas_return _ -> 
          (* No code is being declared: just bake a (mutable) environment *)
          let cloCode' = 
            match td.tdExtends with 
            | None ->  ilmbody_of_mdef (mk_nongeneric_nothing_ctor tagClo cenv.ilg.tref_Object []) 
            | Some  _ -> conv_ilmbody cenv (now_gparams,Some now_clospec,None)  (Lazy.force clo.cloCode)

          let ctor_mdef = 
            let flds = (mk_genclass_clofld_specs cenv now_fields)
            mk_ctor(MemAccess_public,
                    List.map mk_named_param flds,
                    mk_impl
                      (cloCode'.ilZeroInit,
                       cloCode'.ilLocals,
                       cloCode'.ilMaxStack,
                       prepend_instrs_to_code
                          (List.concat (List.mapi (fun n (nm,ty) -> 
                               [ ldarg_0;
                                 I_ldarg (uint16 (n+1));
                                 mk_normal_stfld (mk_fspec_in_boxed_tspec (now_tspec,nm,ty));
                               ])  flds))
                         cloCode'.ilCode,
                       tagClo))
          
          let cloclass = 
            { td with
                  tdImplements= td.tdImplements;
                  tdExtends= (match td.tdExtends with None -> Some cenv.ilg.typ_Object | Some x -> Some(x));
                  tdName = now_name;
                  tdGenericParams= now_gparams;
                  tdMethodDefs= mk_mdefs (ctor_mdef :: List.map (conv_mdef cenv ( now_gparams,Some now_clospec)) (dest_mdefs td.tdMethodDefs)); 
                  tdFieldDefs= mk_fdefs (mk_genclass_clofld_defs cenv now_fields @ dest_fdefs td.tdFieldDefs);
                  tdKind = TypeDef_class; } 
          [cloclass],[]
      | a,b,_ ->
          failwith ("Unexpected unsupported abstraction sequence, #tyabs = "^string (List.length a) ^ ", #tmabs = "^string (List.length b))
   
    mdefGen := !mdefGen@new_mdefs;
    new_tdefs

(* -------------------------------------------------------------------- 
 * Convert a class 
 * -------------------------------------------------------------------- *)

let rec conv_tdef cenv mdefGen encl td = 
  match td.tdKind with 
  | TypeDef_other e when is_ilx_ext_type_def_kind e && (match dest_ilx_ext_type_def_kind e with ETypeDef_closure _ -> true | _ -> false) -> 
      match dest_ilx_ext_type_def_kind e with 
      | ETypeDef_closure cloinfo -> conv_clodef cenv mdefGen encl td cloinfo
      | ETypeDef_classunion _ -> failwith "classunions should have been erased by this time"
  | _ -> 
      [ {td with 
             tdNested = conv_tdefs cenv mdefGen (encl@[td.tdName]) td.tdNested;
             tdMethodDefs=mdefs_mdef2mdef (conv_mdef cenv (td.tdGenericParams,None)) td.tdMethodDefs; } ]

and conv_tdefs cenv mdefGen encl tdefs = 
  tdefs_tdef2tdefs (conv_tdef cenv mdefGen encl) tdefs

let ConvModule ilg modul = 
  let cenv = new_cenv(ilg)
  let mdefGen = ref []
  let new_types = conv_tdefs cenv mdefGen [] modul.modulTypeDefs
  let new_meths = !mdefGen
#if CLOSURES_VIA_POINTERS
  let new_types = 
    if new_meths = [] then new_types else 
    let new_type_for_meths = 
       mk_simple_tdef 
         cenv.ilg
         (class_name_for_apply_meths,
          TypeAccess_private,
          mk_mdefs new_meths,
          mk_fdefs [],
          mk_properties [],
          mk_events [],
          mk_custom_attrs [])
      add_tdef new_type_for_meths new_types
#endif
  {modul with modulTypeDefs=new_types}

