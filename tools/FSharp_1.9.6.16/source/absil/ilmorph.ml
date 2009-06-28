// (c) Microsoft Corporation. All rights reserved
#light

module Microsoft.FSharp.Compiler.AbstractIL.Morphs 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
module Illib = Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 
module Ildiag = Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 
open Ildiag
open Il
open Ilx
open Illib

type 'a morph = 'a -> 'a

type EnclosingTypeDefs = ILTypeDef list * ILTypeDef

let checking = false 
let notlazy v = Lazy.CreateFromValue v

let mdef_code2code f md  =
  let code = 
    match dest_mbody md.mdBody with 
    | MethodBody_il il-> il 
    | _ -> failwith "mdef_code2code - method not IL"  
  let code' = MethodBody_il {code with ilCode = f code.ilCode} 
  {md with mdBody=  mk_mbody code'}  

let code_block2block f (c:ILCode) = check_code (f c)

let bblock_instr2instr f bb = 
    let instrs = bb.bblockInstrs 
    let len = Array.length instrs 
    let res = Array.zeroCreate len 
    for i = 0 to len - 1 do 
        res.[i] <- f instrs.[i]
    {bb with bblockInstrs=res}

// This is quite performance critical 
let nonNil x = match x with [] -> false | _ -> true
let bblock_instr2instrs f bb = 
  let instrs = bb.bblockInstrs 
  let codebuf = ref (Array.zeroCreate (Array.length instrs)) 
  let codebuf_size = ref 0 
  for i = 0 to Array.length instrs - 1 do 
    let instr = instrs.[i] 
    let instrs = f instr 
    let curr = ref instrs 
    while nonNil !curr do
      match !curr with 
        (instr'::t) ->  
          let sz = !codebuf_size 
          let old_buf_size = Array.length !codebuf 
          let new_size = sz + 1 
          if new_size > old_buf_size then begin
            let old = !codebuf 
            let new' = Array.zeroCreate (max new_size (old_buf_size * 4)) 
            Array.blit old 0 new' 0 sz;
            codebuf := new';
          end;
          (!codebuf).[sz] <- instr';
          incr codebuf_size;
          curr := t;
      | [] -> ()
    done;
  done;
  {bb with bblockInstrs = Array.sub !codebuf 0 !codebuf_size}

(* Map each instruction in a basic block to a more complicated block that *)
(* may involve internal branching, but which will still have one entry *)
(* label and one exit label. This is used, for example, when macro-expanding *)
(* complicated high-level ILX instructions. *)
(* The morphing function is told the name of the input and output labels *)
(* that must be used for the generated block. *)
(* Optimize the case where an instruction gets mapped to a *)
(* straightline sequence of instructions by allowing the morphing *)
(* function to return a special result for this case. *)
(* *)
(* Let [i] be the instruction being morphed.  If [i] is a control-flow *)
(* then instruction then [f] must return either a control-flow terminated *)
(* sequence of instructions or a block both of which must targets the same labels *)
(* (or a subset of the labels) targeted in [i].  If [i] *)
(* is not a if not a control-flow instruction then [f] *)
(* must return a block targeting the given output label. *)
let commit_acc_bblock sofar = 
    let sofar = List.rev sofar (* fragments pushed in reverse *)
    let nres = 
      let len = ref 0 (* 1: make room for final branch instruction *)
      List.iter (fun l -> len := !len + List.length l) sofar;
      !len 
    let res = Array.create nres I_ret 
    let count = ref 0 
    sofar |> List.iterSquared (fun i -> res.[!count] <- i; incr count) ;
    assert(!count = nres);
    res

let rec bblock_loop f bb curr_bblock_inplab curri_inplab curri_outlab sofar instrs = 
    match instrs with 
    | (i::rest) -> 
        let res = f curri_inplab curri_outlab i 
        begin match res with 
          (* First possibility: return a sequence of instructions.  No *)
          (* addresses get consumed. *)
        | Choice1Of2 is' -> 
            bblock_loop f bb curr_bblock_inplab curri_inplab curri_outlab (is' :: sofar) rest
        | Choice2Of2 middle_bblock ->
          let before_bblock = 
            let instrs = commit_acc_bblock ([I_br curri_inplab] :: sofar) 
            mk_bblock {bblockLabel=curr_bblock_inplab;bblockInstrs=instrs} 
          if checking && unique_entry_of_code middle_bblock <> curri_inplab then 
            dprintn ("*** warning when transforming bblock "^string_of_code_label bb.bblockLabel^": bblock2code_instr2code: input label of returned block does not match the expected label while converting an instruction to a block.");
          let after_blocks = 
            match rest with 
            | [] -> 
    (*
              if checking && List.mem curri_outlab (exits_of_code middle_bblock) then 
                dprintn ("*** warning when transforming bblock "^string_of_code_label bb.bblockLabel^": bblock2code_instr2code: output label of transformed control flow instruction should not use the label provided for non-control-flow instructions.");
    *)
              [] (* the bblock has already been transformed *)
            | _ -> 
    (*
              if checking && unique_exit_of_code middle_bblock <> curri_outlab then 
                dprintn ("*** warning when transforming bblock "^string_of_code_label bb.bblockLabel^": bblock2code_instr2code: output label of returned block does not match the expected label while converting an instruction to a block.");
    *)
              let new_curri_inlab = generate_code_label () 
              let new_curri_outlab = generate_code_label () 
              [ bblock_loop f bb curri_outlab new_curri_inlab new_curri_outlab [] rest ]
           
          check_code 
                  (mk_group_block 
                     ( curri_inplab :: (match rest with [] -> [] | _ -> [ curri_outlab ]),
                      before_bblock ::  middle_bblock :: after_blocks))
        end
    | [] -> 
       let instrs = commit_acc_bblock sofar 
       mk_bblock {bblockLabel=curr_bblock_inplab;bblockInstrs=instrs} 

let bblock2code_instr2code
    (f:ILCodeLabel -> ILCodeLabel -> ILInstr -> Choice<ILInstr list, ILCode> ) 
    bb = 
  bblock_loop f bb
    bb.bblockLabel 
    (generate_code_label ()) 
    (generate_code_label ()) [] (Array.to_list bb.bblockInstrs)

let rec block_bblock2code_typ2typ ((fbb,fty) as f) x =
  match x with
  | ILBasicBlock bblock -> fbb bblock
  | GroupBlock (locs,l) -> GroupBlock(locs,List.map (code_bblock2code_typ2typ f) l)
  | TryBlock (tryb,seh) ->
      TryBlock (code_bblock2code_typ2typ f tryb,
                begin match seh with 
                | FaultBlock b -> FaultBlock (code_bblock2code_typ2typ f  b)
                | FinallyBlock b -> FinallyBlock (code_bblock2code_typ2typ f  b)
                | FilterCatchBlock clsl -> 
                    FilterCatchBlock 
                      (List.map (fun (flt,ctch) -> 
                        (match flt with 
                          CodeFilter fltcode -> CodeFilter (code_bblock2code_typ2typ f fltcode)
                        | TypeFilter ty -> TypeFilter (fty ty)), 
                        code_bblock2code_typ2typ f ctch) clsl)
                end)
  | RestrictBlock (ls,c) -> RestrictBlock (ls,code_bblock2code_typ2typ f c)

and code_bblock2code_typ2typ f (c:ILCode) = check_code (block_bblock2code_typ2typ f c)
let topcode_bblock2code_typ2typ f (c:ILCode) = code_bblock2code_typ2typ f c

let rec block_bblock2code f x =
  match x with
  | ILBasicBlock bblock -> f bblock
  | GroupBlock (locs,l) -> GroupBlock(locs,List.map (code_bblock2code f) l)
  | TryBlock (tryb,seh) ->
      TryBlock (code_bblock2code f tryb,
                begin match seh with 
                | FaultBlock b -> FaultBlock (code_bblock2code f  b)
                | FinallyBlock b -> FinallyBlock (code_bblock2code f  b)
                | FilterCatchBlock clsl -> 
                    FilterCatchBlock 
                      (List.map (fun (flt,ctch) -> 
                        (match flt with 
                         |CodeFilter fltcode -> CodeFilter (code_bblock2code f fltcode)
                         | TypeFilter ty -> flt), 
                        code_bblock2code f ctch) clsl)
                end)
  | RestrictBlock (ls,c) -> RestrictBlock (ls,code_bblock2code f c)

and code_bblock2code f (c:ILCode) = check_code (block_bblock2code f c)
let topcode_bblock2code f (c:ILCode) = code_bblock2code f c

(* --------------------------------------------------------------------
 * Standard morphisms - mapping types etc.
 * -------------------------------------------------------------------- *)

let rec typ_tref2tref f x  = 
  match x with 
  | Type_ptr t -> Type_ptr (typ_tref2tref f t)
  | Type_fptr x -> 
      Type_fptr
        { callsigCallconv=x.callsigCallconv;
          callsigArgs=List.map (typ_tref2tref f) x.callsigArgs;
          callsigReturn=typ_tref2tref f x.callsigReturn}
  | Type_byref t -> Type_byref (typ_tref2tref f t)
  | Type_boxed cr -> Type_boxed (tspec_tref2tref f cr)
  | Type_value ir -> Type_value (tspec_tref2tref f ir)
  | Type_array (s,ty) -> Type_array (s,typ_tref2tref f ty)
  | Type_tyvar v ->  Type_tyvar v 
  | x -> x
and tspec_tref2tref f (x:ILTypeSpec) = 
  ILTypeSpec.Create(f x.TypeRef, List.map (typ_tref2tref f) x.GenericArgs)


let rec typ_scoref2scoref_tyvar2typ ((fscope,ftyvar) as fs)x  = 
  match x with 
  | Type_ptr t -> Type_ptr (typ_scoref2scoref_tyvar2typ fs t)
  | Type_fptr t -> Type_fptr (callsig_scoref2scoref_tyvar2typ fs t)
  | Type_byref t -> Type_byref (typ_scoref2scoref_tyvar2typ fs t)
  | Type_boxed cr -> Type_boxed (tspec_scoref2scoref_tyvar2typ fs cr)
  | Type_value ir -> Type_value (tspec_scoref2scoref_tyvar2typ fs ir)
  | Type_array (s,ty) -> Type_array (s,typ_scoref2scoref_tyvar2typ fs ty)
  | Type_tyvar v ->  ftyvar v
  | x -> x
and tspec_scoref2scoref_tyvar2typ fs (x:ILTypeSpec) = 
  ILTypeSpec.Create(tref_scoref2scoref (fst fs) x.TypeRef,typs_scoref2scoref_tyvar2typ fs x.GenericArgs)
and callsig_scoref2scoref_tyvar2typ f x = 
  { callsigCallconv=x.callsigCallconv;
    callsigArgs=List.map (typ_scoref2scoref_tyvar2typ f) x.callsigArgs;
    callsigReturn=typ_scoref2scoref_tyvar2typ f x.callsigReturn}
and typs_scoref2scoref_tyvar2typ f i = List.map (typ_scoref2scoref_tyvar2typ f) i
and gparams_scoref2scoref_tyvar2typ f i = List.map (gparam_scoref2scoref_tyvar2typ f) i
and gparam_scoref2scoref_tyvar2typ f i = i
and tref_scoref2scoref fscope (x:ILTypeRef) = 
  ILTypeRef.Create(scope=fscope x.Scope, enclosing=x.Enclosing, name = x.Name)


let callsig_typ2typ f x = 
  { callsigCallconv=x.callsigCallconv;
    callsigArgs=List.map f x.callsigArgs;
    callsigReturn=f x.callsigReturn}

let gparam_typ2typ f gf = {gf with gpConstraints=List.map f gf.gpConstraints}
let gparams_typ2typ f gfs = List.map (gparam_typ2typ f) gfs
let typs_typ2typ f  x = List.map f x
let mref_typ2typ f (x:ILMethodRef) = 
  ILMethodRef.Create(enclosingTypeRef=tref_of_typ (f (Type_boxed (mk_nongeneric_tspec x.EnclosingTypeRef))),
                     callingConv=x.CallingConv,
                     name=x.Name,
                     genericArity=x.GenericArity,
                     argTypes= List.map f x.ArgTypes,
                     returnType= f x.ReturnType)


type formal_scope_ctxt =  Choice<ILMethodSpec, ILFieldSpec, IlxUnionSpec>

let mspec_typ2typ (((factualty : ILType -> ILType) , (fformalty: formal_scope_ctxt -> ILType -> ILType)) as fs) x = 
  let x1,x2,x3 = dest_mspec x 
  mk_mref_mspec_in_typ(mref_typ2typ (fformalty (Choice1Of3 x)) x1,
                       factualty x2, 
                       typs_typ2typ factualty  x3)

let fref_typ2typ f x = 
  { x with frefParent = tref_of_typ (f (Type_boxed (mk_nongeneric_tspec x.frefParent)));
           frefType= f x.frefType }
let fspec_typ2typ ((factualty,(fformalty : formal_scope_ctxt -> ILType -> ILType)) as fs) x = 
  { fspecFieldRef=fref_typ2typ (fformalty (Choice2Of3 x)) x.fspecFieldRef;
    fspecEnclosingType= factualty x.fspecEnclosingType }

let cattr_typ2typ f c =
  { c with customMethod = mspec_typ2typ (f, (fun _ -> f)) c.customMethod }

let cattrs_typ2typ f cs =
  mk_custom_attrs (List.map (cattr_typ2typ f) (dest_custom_attrs cs))

let fdef_typ2typ ftype fd = {fd with fdType=ftype fd.fdType; 
                                     fdCustomAttrs=cattrs_typ2typ ftype fd.fdCustomAttrs}

let alts_typ2typ f alts = 
  Array.map (fun alt -> { alt with altFields = Array.map (fdef_typ2typ f)  alt.altFields;
                                   altCustomAttrs = cattrs_typ2typ f alt.altCustomAttrs }) alts

let curef_typ2typ f (IlxUnionRef(s,alts,nullPermitted)) =
  IlxUnionRef(s,alts_typ2typ f alts,nullPermitted)

let local_typ2typ f l = {l with localType = f l.localType}
let freevar_typ2typ f l = {l with fvType = f l.fvType}
let varargs_typ2typ f varargs = Option.map (List.map f) varargs
(* REVIEW: convert varargs *)
let instr_typ2typ ((factualty,fformalty) as fs) i = 
  let factualty = factualty (Some i) 
  let conv_fspec fr = fspec_typ2typ (factualty,fformalty (Some i)) fr 
  let conv_mspec mr = mspec_typ2typ (factualty,fformalty (Some i)) mr 
  match i with 
  | I_calli (a,mref,varargs) ->  I_calli (a,callsig_typ2typ (factualty) mref,varargs_typ2typ factualty varargs)
  | I_call (a,mr,varargs) ->  I_call (a,conv_mspec mr,varargs_typ2typ factualty varargs)
  | I_callvirt (a,mr,varargs) ->   I_callvirt (a,conv_mspec mr,varargs_typ2typ factualty varargs)
  | I_callconstraint (a,ty,mr,varargs) ->   I_callconstraint (a,factualty ty,conv_mspec mr,varargs_typ2typ factualty varargs)
  | I_newobj (mr,varargs) ->  I_newobj (conv_mspec mr,varargs_typ2typ factualty varargs)
  | I_ldftn mr ->  I_ldftn (conv_mspec mr)
  | I_ldvirtftn mr ->  I_ldvirtftn (conv_mspec mr)
  | I_other e when is_ilx_ext_instr e -> 
      begin match (dest_ilx_ext_instr e) with 
      | (EI_ldftn_then_call (mr1,(a,mr2,varargs))) -> 
          mk_IlxInstr (EI_ldftn_then_call (conv_mspec mr1,(a, conv_mspec mr2, varargs_typ2typ factualty varargs)))
      | (EI_ld_instance_ftn_then_newobj (mr1,callsig,(mr2,varargs2))) -> 
          mk_IlxInstr (EI_ld_instance_ftn_then_newobj 
                          (conv_mspec mr1,
                           callsig_typ2typ (fformalty (Some i) (Choice1Of3 mr2)) callsig,
                           (conv_mspec mr2, varargs_typ2typ factualty varargs2)))
      | _ -> i
      end
  | I_ldfld (a,b,fr) ->  I_ldfld (a,b,conv_fspec fr)
  | I_ldsfld (a,fr) ->  I_ldsfld (a,conv_fspec fr)
  | I_ldsflda (fr) ->  I_ldsflda (conv_fspec fr)
  | I_ldflda fr ->  I_ldflda (conv_fspec fr)
  | I_stfld (a,b,fr) -> I_stfld (a,b,conv_fspec fr)
  | I_stsfld (a,fr) -> I_stsfld (a,conv_fspec fr)
  | I_castclass typ -> I_castclass (factualty typ)
  | I_isinst typ -> I_isinst (factualty typ)
  | I_initobj typ -> I_initobj (factualty typ)
  | I_cpobj typ -> I_cpobj (factualty typ)
  | I_stobj (al,vol,typ) -> I_stobj (al,vol,factualty typ)
  | I_ldobj (al,vol,typ) -> I_ldobj (al,vol,factualty typ)
  | I_box typ -> I_box (factualty typ)
  | I_unbox typ -> I_unbox (factualty typ)
  | I_unbox_any typ -> I_unbox_any (factualty typ)
  | I_ldelem_any (shape,typ) ->  I_ldelem_any (shape,factualty typ)
  | I_stelem_any (shape,typ) ->  I_stelem_any (shape,factualty typ)
  | I_newarr (shape,typ) ->  I_newarr (shape,factualty typ)
  | I_ldelema (ro,shape,typ) ->  I_ldelema (ro,shape,factualty typ)
  | I_sizeof typ ->  I_sizeof (factualty typ)
  | I_ldtoken tok -> 
      begin match tok with 
        Token_type typ ->   I_ldtoken (Token_type (factualty typ))
      | Token_method mr -> I_ldtoken (Token_method (conv_mspec mr))
      | Token_field fr -> I_ldtoken (Token_field (conv_fspec fr))
      end
  | x -> x

let return_typ2typ f (r:ILReturnValue) = {r with returnType=f r.Type; returnCustomAttrs=cattrs_typ2typ f r.returnCustomAttrs}
let param_typ2typ f p = {p with paramType=f p.paramType; paramCustomAttrs=cattrs_typ2typ f p.paramCustomAttrs}

let mdefs_mdef2mdef f (m:ILMethodDefs) = mk_mdefs (List.map f (dest_mdefs m))
let fdefs_fdef2fdef f (m:ILFieldDefs) = mk_fdefs (List.map f (dest_fdefs m))

(* use this when the conversion produces just one type... *)
let tdefs_tdef2tdef f m = mk_tdefs (List.map f (dest_tdefs m))

let tdefs_tdef2tdefs f (m:ILTypeDefs) = 
  mk_tdefs (List.foldBack (fun x y -> f x @ y)(dest_tdefs m) [])

let module_tdefs2tdefs typesf m = 
    {m with modulTypeDefs=typesf m.modulTypeDefs}

let locals_typ2typ f ls = List.map (local_typ2typ f) ls
let freevars_typ2typ f ls = List.map (freevar_typ2typ f) ls

let ilmbody_bblock2code_typ2typ_maxstack2maxstack fs il = 
  let (finstr,ftype,fmaxstack) = fs 
  {il with ilCode=topcode_bblock2code_typ2typ (finstr,ftype) il.ilCode;
           ilLocals = locals_typ2typ ftype il.ilLocals;
           ilMaxStack = fmaxstack il.ilMaxStack }

let mbody_details_ilmbody2ilmbody (filmbody) x = 
  match x with
  | MethodBody_il il -> MethodBody_il (filmbody il)
  | x -> x

let mbody_ilmbody2ilmbody (filmbody) x = mk_mbody (mbody_details_ilmbody2ilmbody filmbody (dest_mbody x))

let ospec_typ2typ f (OverridesSpec(mref,ty)) =OverridesSpec(mref_typ2typ f mref, f ty)

let mdef_typ2typ_ilmbody2ilmbody fs md  = 
  let (ftype,filmbody) = fs 
  let ftype' = ftype (Some md) 
  let body' = mbody_ilmbody2ilmbody (filmbody (Some md))  md.mdBody 
  {md with 
    mdGenericParams=gparams_typ2typ ftype' md.mdGenericParams;
    mdBody= body';
    mdParams = List.map (param_typ2typ ftype') md.mdParams;
    mdReturn = return_typ2typ ftype' md.mdReturn;
    mdCustomAttrs=cattrs_typ2typ ftype' md.mdCustomAttrs }

let fdefs_typ2typ f x = fdefs_fdef2fdef (fdef_typ2typ f) x

let mdefs_typ2typ_ilmbody2ilmbody fs x = mdefs_mdef2mdef (mdef_typ2typ_ilmbody2ilmbody fs) x

let cuinfo_typ2typ  ftype cud = 
  { cud with cudAlternatives = alts_typ2typ ftype cud.cudAlternatives; } 


let cloinfo_typ2typ_ilmbody2ilmbody fs clo = 
  let (ftype,filmbody) = fs 
  let c' = filmbody None (Lazy.force clo.cloCode) 
  { clo with cloFreeVars = freevars_typ2typ ftype clo.cloFreeVars;
             cloCode=notlazy c' }

let cloinfo_ilmbody2ilmbody f clo = 
  let c' = f (Lazy.force clo.cloCode) 
  { clo with cloCode=notlazy c' }

let mimpl_typ2typ f e =
  { e with
      mimplOverrides = ospec_typ2typ f e.mimplOverrides;
      mimplOverrideBy = mspec_typ2typ (f,(fun _ -> f)) e.mimplOverrideBy; }

let edef_typ2typ f e =
  { e with
      eventType = Option.map f e.eventType;
      eventAddOn = mref_typ2typ f e.eventAddOn;
      eventRemoveOn = mref_typ2typ f e.eventRemoveOn;
      eventFire = Option.map (mref_typ2typ f) e.eventFire;
      eventOther = List.map (mref_typ2typ f) e.eventOther;
      eventCustomAttrs = cattrs_typ2typ f e.eventCustomAttrs }

let pdef_typ2typ f p =
  { p with
      propSet = Option.map (mref_typ2typ f) p.propSet;
      propGet = Option.map (mref_typ2typ f) p.propGet;
      propType = f p.propType;
      propArgs = List.map f p.propArgs;
      propCustomAttrs = cattrs_typ2typ f p.propCustomAttrs }

let pdefs_typ2typ f pdefs = 
  mk_properties (List.map (pdef_typ2typ f) (dest_pdefs pdefs))
let edefs_typ2typ f edefs = 
  mk_events (List.map (edef_typ2typ f) (dest_edefs edefs))

let mimpls_typ2typ f mimpls = 
  mk_mimpls (List.map (mimpl_typ2typ f) (dest_mimpls mimpls))

let rec tdef_typ2typ_ilmbody2ilmbody_mdefs2mdefs enc fs td = 
   let (ftype,filmbody,fmdefs) = fs 
   let ftype' = ftype (Some (enc,td)) None 
   let mdefs' = fmdefs (enc,td) td.tdMethodDefs 
   let fdefs' = fdefs_typ2typ ftype' td.tdFieldDefs 
   {td with tdImplements= List.map ftype' td.tdImplements;
            tdGenericParams= gparams_typ2typ ftype' td.tdGenericParams; 
            tdExtends = Option.map ftype' td.tdExtends;
            tdMethodDefs=mdefs';
            tdNested=tdefs_typ2typ_ilmbody2ilmbody_mdefs2mdefs (enc@[td]) fs td.tdNested;
            tdFieldDefs=fdefs';
            tdMethodImpls = mimpls_typ2typ ftype' td.MethodImpls;
            tdEvents = edefs_typ2typ ftype' td.Events; 
            tdProperties = pdefs_typ2typ ftype' td.Properties;
            tdCustomAttrs = cattrs_typ2typ ftype' td.CustomAttrs;
            tdKind =
               begin match td.tdKind with
               | TypeDef_other e when is_ilx_ext_type_def_kind e -> 
                   begin match dest_ilx_ext_type_def_kind e with 
                   | ETypeDef_closure i -> mk_IlxTypeDefKind (ETypeDef_closure (cloinfo_typ2typ_ilmbody2ilmbody (ftype',filmbody (enc,td)) i))
                   | ETypeDef_classunion i -> mk_IlxTypeDefKind (ETypeDef_classunion (cuinfo_typ2typ  ftype' i))
                   end
               | _ -> td.tdKind
               end 
  }

and tdefs_typ2typ_ilmbody2ilmbody_mdefs2mdefs enc fs tdefs = 
  tdefs_tdef2tdef (tdef_typ2typ_ilmbody2ilmbody_mdefs2mdefs enc fs) tdefs

(* --------------------------------------------------------------------
 * Derived versions of the above, e.g. with defaults added
 * -------------------------------------------------------------------- *)

let manifest_typ2typ f m =
  { m with
      manifestCustomAttrs = cattrs_typ2typ f m.manifestCustomAttrs }

let module_typ2typ_ilmbody2ilmbody_mdefs2mdefs 
    ((ftype: ILModuleDef -> (ILTypeDef list * ILTypeDef) option -> ILMethodDef option -> ILType -> ILType),
     (filmbody: ILModuleDef -> ILTypeDef list * ILTypeDef -> ILMethodDef option -> ILMethodBody -> ILMethodBody),
     fmdefs) m = 
  let ftdefs = 
    tdefs_typ2typ_ilmbody2ilmbody_mdefs2mdefs []
      (ftype m,
       filmbody m,
       fmdefs m) 
  { m with modulTypeDefs=ftdefs m.modulTypeDefs;
           modulCustomAttrs=cattrs_typ2typ (ftype m None None) m.modulCustomAttrs;
           modulManifest=Option.map (manifest_typ2typ (ftype m None None)) m.modulManifest  }
    
let module_bblock2code_typ2typ_maxstack2maxstack fs x = 
  let (fbblock,ftype,fmaxstack) = fs 
  let filmbody mod_ctxt tdef_ctxt mdef_ctxt =
    ilmbody_bblock2code_typ2typ_maxstack2maxstack 
      (fbblock mod_ctxt tdef_ctxt mdef_ctxt, 
       ftype mod_ctxt (Some tdef_ctxt) mdef_ctxt,
       fmaxstack mod_ctxt tdef_ctxt mdef_ctxt) 
  let fmdefs mod_ctxt tdef_ctxt = 
    mdefs_typ2typ_ilmbody2ilmbody 
      (ftype mod_ctxt (Some tdef_ctxt),
       filmbody mod_ctxt tdef_ctxt) 
  module_typ2typ_ilmbody2ilmbody_mdefs2mdefs 
    (ftype,
     filmbody,
     fmdefs)
    x 

let module_bblock2code f x = 
  module_bblock2code_typ2typ_maxstack2maxstack 
    (f, 
     (fun mod_ctxt tdef_ctxt mdef_ctxt x -> x), 
     (fun mod_ctxt tdef_ctxt mdef_ctxt x -> x)) x
let module_bblock2code_typ2typ (f1,f2) x = 
  module_bblock2code_typ2typ_maxstack2maxstack 
    (f1, 
     f2, 
     (fun mod_ctxt tdef_ctxt mdef_ctxt x -> x)) x
let module_bblock2code_maxstack2maxstack (f1,f2) x = 
  module_bblock2code_typ2typ_maxstack2maxstack 
    (f1,
     (fun mod_ctxt tdef_ctxt mdef_ctxt x -> x),
     f2) 
    x
let module_instr2instr_typ2typ (f1,f2) x = 
  module_bblock2code_typ2typ 
    ((fun mod_ctxt tdef_ctxt mdef_ctxt  i -> mk_bblock (bblock_instr2instr (f1 mod_ctxt tdef_ctxt mdef_ctxt) i)), 
     f2)
    x

let topcode_instr2instrs f x = 
  topcode_bblock2code 
    (fun i -> mk_bblock (bblock_instr2instrs f i))
    x
let topcode_instr2code f x = 
  topcode_bblock2code (bblock2code_instr2code f) x

let module_typ2typ ftype y = 
    let finstr mod_ctxt tdef_ctxt mdef_ctxt =
        let fty = ftype mod_ctxt (Some tdef_ctxt) mdef_ctxt 
        instr_typ2typ ((fun instr_ctxt -> fty), (fun instr_ctxt formal_ctxt -> fty)) 
    module_instr2instr_typ2typ (finstr,ftype) y


let rec tdef_tdef2tdef f enc td = 
    let td' = f enc td  
    {td' with tdNested = all_tdefs_tdef2tdef f (enc@[td]) td'.tdNested }
and all_tdefs_tdef2tdef f enc tds = mk_tdefs (List.map (tdef_tdef2tdef f enc) (dest_tdefs tds))

let module_tdef2tdef f m = module_tdefs2tdefs (all_tdefs_tdef2tdef f []) m

let module_tref2tref_memoized f modul = 
  let fty = memoize (typ_tref2tref f)
  module_typ2typ (fun _ _ _ ty -> fty ty) modul

let module_scoref2scoref_memoized f modul = 
  module_tref2tref_memoized (tref_scoref2scoref f) modul





