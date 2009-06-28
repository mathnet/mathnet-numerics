// (c) Microsoft Corporation 2005-2009. 

#light

/// Derived expression manipulation and construction functions.
module (* internal *) Microsoft.FSharp.Compiler.Tastops 

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX 
open Microsoft.FSharp.Compiler 

module Illib = Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
module Ilprint = Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 

open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.PrettyNaming

//---------------------------------------------------------------------------
// Standard orderings, e.g. for order set/map keys
//---------------------------------------------------------------------------

let val_spec_order (v1:Val) (v2:Val) = compare v1.Stamp v2.Stamp
let tycon_spec_order  (tc1:Tycon) (tc2:Tycon) = compare tc1.Stamp tc2.Stamp 
let rfref_order  (RFRef(tcref1,nm1)) (RFRef(tcref2,nm2)) = 
    let c = tycon_spec_order (deref_tycon tcref1) (deref_tycon tcref2) 
    if c <> 0 then c else 
    compare nm1 nm2

let ucref_order  (UCRef(tcref1,nm1)) (UCRef(tcref2,nm2)) = 
    let c = tycon_spec_order (deref_tycon tcref1) (deref_tycon tcref2) 
    if c <> 0 then c else 
    compare nm1 nm2

//---------------------------------------------------------------------------
// Make some common types
//---------------------------------------------------------------------------

let mk_fun_ty d r = TType_fun (d,r)
let (-->) d r = mk_fun_ty d r
let mk_forall_ty d r = TType_forall (d,r)
let try_mk_forall_ty d r = if isNil d then r else mk_forall_ty d r
let (+->) d r = try_mk_forall_ty d r
let mk_tuple_ty l = TType_tuple l
let mk_iterated_fun_ty dl r = List.foldBack (-->) dl r

let fake_mk_tupled_ty m tys = 
    match tys with 
    | [] -> error(InternalError("fake_mk_tupled_ty",m))
    | [h] -> h 
    | _ -> mk_tuple_ty tys

let type_of_lambda_arg m vs = fake_mk_tupled_ty m (types_of_vals vs)
let mk_multi_lambda_ty m vs rty = mk_fun_ty (type_of_lambda_arg m vs) rty 
let mk_lambda_ty tps tys rty = try_mk_forall_ty tps (mk_iterated_fun_ty tys rty)

/// When compiling FSharp.Core.dll we have to deal with the non-local references into
/// the library arising from env.ml. Part of this means that we have to be able to resolve these
/// references. This function artificially forces the existence of a module or namespace at a 
/// particular point in order to do this.
let ensure_fslib_has_submodul_at (ccu:ccu) path (CompPath(_,cpath)) xml =
    let scoref = ccu.ILScopeRef 
    let rec loop prior_cpath (path:ident list) cpath (modul:ModuleOrNamespace) =
        let mtype = modul.ModuleOrNamespaceType 
        match path,cpath with 
        | (hpath::tpath),((_,mkind)::tcpath)  -> 
            let modName = hpath.idText 
            if not (Map.mem modName mtype.AllEntities) then 
                let smodul = NewModuleOrNamespace (Some(CompPath(scoref,prior_cpath))) taccessPublic hpath xml [] (notlazy (empty_mtype mkind))
                mtype.AddModuleOrNamespaceByMutation(smodul);
            let modul = Map.find modName mtype.AllEntities 
            loop (prior_cpath@[(modName,Namespace)]) tpath tcpath modul 

        | _ -> () 

    loop [] path cpath ccu.Contents


//---------------------------------------------------------------------------
// Primitive destructors
//---------------------------------------------------------------------------

/// Look through the TExpr_link nodes arising from type inference
let rec strip_expr e = 
    match e with 
    | TExpr_link eref -> strip_expr !eref
    | _ -> e    

let discrim_of_case (TCase(d,_)) = d
let dest_of_case (TCase(_,d)) = d
let mk_case (a,b) = TCase(a,b)

let is_tuple e = match e with TExpr_op(TOp_tuple,_,_,_) -> true | _ -> false
let try_dest_tuple e = match e with TExpr_op(TOp_tuple,_,es,_) -> es | _ -> [e]

//---------------------------------------------------------------------------
// Debug info for expressions
//---------------------------------------------------------------------------

let rec range_of_expr x =
  match x with
  | TExpr_val (_,_,m) | TExpr_op (_,_,_,m)   | TExpr_const (_,m,_) | TExpr_quote (_,_,m,_)
  | TExpr_obj (_,_,_,_,_,_,m,_) | TExpr_app(_,_,_,_,m) | TExpr_seq (_,_,_,_,m) 
  | TExpr_static_optimization (_,_,_,m) | TExpr_lambda (_,_,_,_,m,_,_) 
  | TExpr_tlambda (_,_,_,m,_,_)| TExpr_tchoose (_,_,m) | TExpr_letrec (_,_,m,_) | TExpr_let (_,_,m,_) | TExpr_match (_,_,_,_,m,_,_)
    -> m
  | TExpr_link(eref) -> range_of_expr !eref

//---------------------------------------------------------------------------
// Build nodes in decision graphs
//---------------------------------------------------------------------------


let prim_mk_match(spBind,exprm,tree,targets,matchm,ty) = TExpr_match (spBind,exprm,tree,targets,matchm,ty,SkipFreeVarsCache())

type MatchBuilder(spBind,inpRange: Range.range) = 

    let targets = new ResizeArray<_>(10) 
    member x.AddTarget(tg) = 
        let n = targets.Count 
        targets.Add(tg);
        n

    member x.AddResultTarget(e,spTarget) = TDSuccess(FlatList.empty, x.AddTarget(TTarget(FlatList.empty,e,spTarget)))

    member x.CloseTargets() = targets |> ResizeArray.to_list

    member x.Close(dtree,m,ty) = prim_mk_match  (spBind,inpRange,dtree,ResizeArray.to_array targets,m,ty)

let mk_bool_switch m g t e = TDSwitch(g,[TCase(TTest_const(TConst_bool(true)),t)],Some e,m)

let mk_cond spBind spTarget m ty e1 e2 e3 = 
    let mbuilder = new MatchBuilder(spBind,m)
    let dtree = mk_bool_switch m e1 (mbuilder.AddResultTarget(e2,spTarget)) (mbuilder.AddResultTarget(e3,spTarget)) 
    mbuilder.Close(dtree,m,ty)

//---------------------------------------------------------------------------
// These make local/non-local references to values according to whether
// the item is globally stable ("published") or not.
//---------------------------------------------------------------------------

let mk_local_vref   (v:Val) = VRef_private v
let mk_local_modref (v:ModuleOrNamespace) = ERef_private v
let mk_local_tcref  (v:Tycon) = ERef_private v
let mk_local_ecref  (v:Tycon) = ERef_private v

let mk_nonlocal_ccu_top_tcref ccu (x:Tycon) = mk_nonlocal_tcref_preresolved x (nlpath_of_ccu ccu) x.MangledName

let mk_vref_in_modref  (cref:TyconRef) (x:Val) : ValRef = 
    match cref with 
    | ERef_private _ -> mk_local_vref x
    | ERef_nonlocal nlr ->
        let (NLPath(ccu,p)) = nlr.nlr_nlpath 
        mk_nonlocal_vref_preresolved x (NLPath(ccu, Array.append p [| nlr.nlr_item |])) x.MangledName

let MakeNestedTcref (cref:TyconRef) (x:Entity) : TyconRef  = 
    match cref with 
    | ERef_private _ -> mk_local_tcref x
    | ERef_nonlocal nlr ->
        let (NLPath(ccu,p)) = nlr.nlr_nlpath 
        mk_nonlocal_tcref_preresolved x (NLPath(ccu, Array.append p [| nlr.nlr_item |])) x.MangledName

let mk_rfref_in_tcref (x:ModuleOrNamespaceRef) tycon (rf:ident) : RecdFieldRef = mk_rfref (MakeNestedTcref x tycon) rf.idText 

//---------------------------------------------------------------------------
// Primitive constructors
//---------------------------------------------------------------------------

let expr_for_vref m vref =  TExpr_val(vref,NormalValUse,m)
let expr_for_val m v =  expr_for_vref m (mk_local_vref v)
let gen_mk_local m s ty mut compgen =
    let thisv = NewVal(ident(s,m),ty,mut,compgen,None,None,taccessPublic,ValNotInRecScope,None,NormalVal,[],OptionalInline,emptyXmlDoc,false,false,false,false,None,ParentNone) 
    thisv,expr_for_val m thisv

let mk_local         m s ty = gen_mk_local m s ty Immutable false
let mk_compgen_local m s ty = gen_mk_local m s ty Immutable true
let mk_mut_compgen_local m s ty = gen_mk_local m s ty Mutable true


(* Type gives return type.  For type-lambdas this is the formal return type. *)
let mk_multi_lambda m vs (b,rty) = TExpr_lambda (new_uniq(), None,vs,b,m, rty, SkipFreeVarsCache())
let mk_basev_multi_lambda m basevopt vs (b,rty) = TExpr_lambda (new_uniq(), basevopt,vs,b,m, rty, SkipFreeVarsCache())
let mk_lambda m v (b,rty) = mk_multi_lambda m [v] (b,rty)
let mk_tlambda m vs (b,tau_ty) = match vs with [] -> b | _ -> TExpr_tlambda (new_uniq(), vs,b,m,tau_ty, SkipFreeVarsCache())
let mk_tchoose m vs b = match vs with [] -> b | _ -> TExpr_tchoose (vs,b,m)

let mk_obj_expr (ty,basev,basecall,overrides,iimpls,m) = 
    TExpr_obj (new_uniq(),ty,basev,basecall,overrides,iimpls,m,SkipFreeVarsCache()) 

let mk_lambdas m tps (vs:Val list) (b,rty) = 
  mk_tlambda m tps (List.foldBack (fun v (e,ty) -> mk_lambda m v (e,ty), v.Type --> ty) vs (b,rty))
let mk_multi_lambdas_core m vsl (b,rty) = 
  List.foldBack (fun v (e,ty) -> mk_multi_lambda m v (e,ty), type_of_lambda_arg m v --> ty) vsl (b,rty)
let mk_multi_lambdas m tps vsl (b,rty) = 
  mk_tlambda m tps (mk_multi_lambdas_core m vsl (b,rty) )

let mk_basev_multi_lambdas_core m basevopt vsl (b,rty) = 
    match basevopt with
    | None -> mk_multi_lambdas_core m vsl (b,rty)
    | _ -> 
        match vsl with 
        | [] -> error(InternalError("mk_basev_multi_lambdas_core: can't attach a basev to a non-lambda expression",m))
        | h::t -> 
            let b,rty = mk_multi_lambdas_core m t (b,rty)
            (mk_basev_multi_lambda m basevopt h (b,rty), (type_of_lambda_arg m h --> rty))
        
let mk_basev_multi_lambdas m tps basevopt vsl (b,rty) = 
  mk_tlambda m tps (mk_basev_multi_lambdas_core m basevopt vsl (b,rty) )

let mk_multi_lambda_bind v letSeqPtOpt m  tps vsl (b,rty) = 
    TBind(v,mk_multi_lambdas m tps vsl (b,rty),letSeqPtOpt)

let mk_bind seqPtOpt v e = TBind(v,e,seqPtOpt)
let mk_compgen_bind v e = TBind(v,e,NoSequencePointAtStickyBinding)

/// Make bindings that are compiler generated (though the variables may not be - e.g. they may be lambda arguments in a beta reduction)
let mk_compgen_binds vs es = 
    if List.length vs <> List.length es then failwith "mk_compgen_binds: invalid argument";
    List.map2 mk_compgen_bind vs es |> FlatList.of_list

(* n.b. type gives type of body *)
let mk_let_bind m bind body = TExpr_let(bind,body, m, NewFreeVarsCache())
let mk_lets_bind m binds body = List.foldBack (mk_let_bind m) binds body 
let mk_lets_from_Bindings m binds body = FlatList.foldBack (mk_let_bind m) binds body 
let mk_let seqPtOpt m v x body = mk_let_bind m (mk_bind seqPtOpt v x) body
let mk_compgen_let m v x body = mk_let_bind m (mk_compgen_bind v x) body

let mk_invisible_bind v e = TBind(v,e,NoSequencePointAtInvisibleBinding)
let mk_invisible_let m v x body = mk_let_bind m (mk_invisible_bind v x) body
let mk_invisible_binds vs es = 
    if List.length vs <> List.length es then failwith "mk_invisible_binds: invalid argument";
    List.map2 mk_invisible_bind vs es

let mk_invisible_FlatBindings vs es = 
    if FlatList.length vs <> FlatList.length es then failwith "mk_invisible_FlatBindings: invalid argument";
    FlatList.map2 mk_invisible_bind vs es

let mk_invisible_lets m vs xs body = mk_lets_bind m (mk_invisible_binds vs xs) body
let mk_invisible_lets_from_Bindings m vs xs body = mk_lets_from_Bindings m (mk_invisible_FlatBindings vs xs) body

let mk_letrec_binds m binds body = if FlatList.isEmpty binds then body else TExpr_letrec(binds,body, m, NewFreeVarsCache())
let mk_letrec_binds_typed m binds (body,ty) =  mk_letrec_binds m binds body, ty



//-------------------------------------------------------------------------
// Type schemes...
//-------------------------------------------------------------------------

 
type TypeScheme   = 
    TypeScheme of 
        typars (* the truly generalized type parameters *)
      * typars (* free choice type parameters from a recursive block where this value only generalizes a subsest of the overall set of type parameters generalized *)
      * typ    (* the 'tau' type forming the body of the generalized type *)
  
let mk_poly_bind_rhs m typeScheme bodyExpr = 
    let (TypeScheme(generalizedTypars,freeChoiceTypars,tauType)) = typeScheme
    mk_tlambda m generalizedTypars (mk_tchoose m freeChoiceTypars bodyExpr, tauType)

let is_being_generalized tp typeScheme = 
    let (TypeScheme(generalizedTypars,_,_)) = typeScheme
    ListSet.mem tpspec_eq tp generalizedTypars

//-------------------------------------------------------------------------
// Build conditional expressions...
//------------------------------------------------------------------------- 

let mk_lazy_and g m e1 e2 = mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.bool_ty e1 e2 (TExpr_const(TConst_bool false,m,g.bool_ty))
let mk_lazy_or g m e1 e2 = mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.bool_ty e1 (TExpr_const(TConst_bool true,m,g.bool_ty)) e2

let mk_byref_typ g ty = TType_app (g.byref_tcr, [ty])
let mk_multi_dim_array_typ g n ty = 
  if n = 1 then TType_app (g.il_arr1_tcr, [ty]) 
  elif n = 2 then TType_app (g.il_arr2_tcr, [ty]) 
  elif n = 3 then TType_app (g.il_arr3_tcr, [ty]) 
  elif n = 4 then TType_app (g.il_arr4_tcr, [ty]) 
  else failwith "F# supports a maxiumum .NET array dimension of 4"

let mk_unit_typ g = g.unit_ty 
let mk_nativeint_typ g = g.nativeint_ty

let mk_coerce(e,to_ty,m,from_ty)                     = TExpr_op(TOp_coerce,[to_ty;from_ty],[e],m)

let mk_asm(code,tinst,args,rettys,m)                 = TExpr_op(TOp_asm(code,rettys),tinst,args,m)
let mk_ucase(uc,tinst,args,m)                        = TExpr_op(TOp_ucase uc,tinst,args,m)
let mk_exnconstr(uc,args,m)                          = TExpr_op(TOp_exnconstr uc,[],args,m)
let mk_tuple_field_get(e,tinst,i,m)                  = TExpr_op(TOp_tuple_field_get(i), tinst, [e],m)

let mk_recd_field_get_via_expra(e,fref,tinst,m)      = TExpr_op(TOp_rfield_get(fref), tinst, [e],m)
let mk_recd_field_get_addr_via_expra(e,fref,tinst,m) = TExpr_op(TOp_field_get_addr(fref), tinst, [e],m)

let mk_static_rfield_get_addr(fref,tinst,m)          = TExpr_op(TOp_field_get_addr(fref), tinst, [],m)
let mk_static_rfield_get(fref,tinst,m)               = TExpr_op(TOp_rfield_get(fref), tinst, [],m)
let mk_static_rfield_set(fref,tinst,e,m)             = TExpr_op(TOp_rfield_set(fref), tinst, [e],m)

let mk_recd_field_set_via_expra(e1,fref,tinst,e2,m)  = TExpr_op(TOp_rfield_set(fref), tinst, [e1;e2],m)

let mk_ucase_tag_get(e1,cref,tinst,m)                = TExpr_op(TOp_ucase_tag_get(cref), tinst, [e1],m)
let mk_ucase_proof(e1,cref,tinst,m)            = TExpr_op(TOp_ucase_proof(cref), tinst, [e1],m)

/// Build a 'get' expression for something we've already determined to be a particular union case, and where the
/// input expression has 'TType_ucase', which is an F# compiler internal "type"
let mk_ucase_field_get_proven(e1,cref,tinst,j,m)   = TExpr_op(TOp_ucase_field_get(cref,j), tinst, [e1],m)

/// Build a 'get' expression for something we've already determined to be a particular union case, but where 
/// the static type of the input is not yet proven to be that particular union case. This requires a type
/// cast to 'prove' the condition.
let mk_ucase_field_get_unproven(e1,cref,tinst,j,m)  = mk_ucase_field_get_proven(mk_ucase_proof(e1,cref,tinst,m),cref,tinst,j,m)

let mk_ucase_field_set(e1,cref,tinst,j,e2,m)         = TExpr_op(TOp_ucase_field_set(cref,j), tinst, [e1;e2],m)

let mk_exnconstr_field_get(e1,ecref,j,m)             = TExpr_op(TOp_exnconstr_field_get(ecref,j), [],[e1],m)
let mk_exnconstr_field_set(e1,ecref,j,e2,m)          = TExpr_op(TOp_exnconstr_field_set(ecref,j), [],[e1;e2],m)

let mk_dummy_lambda g (e,ety) = 
    let m = (range_of_expr e) 
    mk_lambda m (fst (mk_compgen_local m "unitVar" g.unit_ty)) (e,ety)
                           
let mk_while       g (spWhile,e1,e2,m)             = 
    TExpr_op(TOp_while spWhile,[]  ,[mk_dummy_lambda g (e1,g.bool_ty);mk_dummy_lambda g (e2,g.unit_ty)],m)

let mk_for         g (spFor,v,e1,dir,e2,e3,m)    = 
    TExpr_op(TOp_for (spFor,dir)    ,[]  ,[mk_dummy_lambda g (e1,g.int_ty) ;mk_dummy_lambda g (e2,g.int_ty);mk_lambda (range_of_expr e3) v (e3,g.unit_ty)],m)

let mk_try_catch   g (e1,vf,ef,vh,eh,m,ty,spTry,spWith) = 
    TExpr_op(TOp_try_catch(spTry,spWith),[ty],[mk_dummy_lambda g (e1,ty);mk_lambda (range_of_expr ef) vf (ef,ty);mk_lambda (range_of_expr eh) vh (eh,ty)],m)

let mk_try_finally g (e1,e2,m,ty,spTry,spFinally)          = 
    TExpr_op(TOp_try_finally(spTry,spFinally),[ty],[mk_dummy_lambda g (e1,ty);mk_dummy_lambda g (e2,g.unit_ty)],m)

let mk_ilzero (m,ty) = TExpr_const(TConst_zero,m,ty) 


let rec split_after_acc n l1 l2 = if n <= 0 then List.rev l1,l2 else split_after_acc (n-1) ((List.hd l2):: l1) (List.tl l2) 
let split_after n l = split_after_acc n [] l


//--------------------------------------------------------------------------
// Maps tracking extra information for values
//--------------------------------------------------------------------------

[<Struct>]
type ValMap<'a> = 
    val imap: I64map.t<'a>
    new (imap) = {imap=imap}
     
// REVIEW: convert these to OO
let vspec_map_find    (v: Val) (m:ValMap<_>) = I64map.find v.Stamp m.imap
let vspec_map_tryfind (v: Val) (m:ValMap<_>) = I64map.tryfind v.Stamp m.imap
let vspec_map_mem     (v: Val) (m:ValMap<_>) = I64map.mem v.Stamp m.imap
let vspec_map_add     (v: Val) x (m:ValMap<_>) = ValMap (I64map.add v.Stamp x m.imap)
let vspec_map_remove  (v: Val) (m:ValMap<_>) = ValMap (I64map.remove v.Stamp m.imap)
let vspec_map_empty () = ValMap (I64map.empty ())
let vspec_map_is_empty (m:ValMap<_>) = m.imap.IsEmpty
let vspec_map_of_list vs = List.foldBack (fun (x,y) acc -> vspec_map_add x y acc) vs (vspec_map_empty()) 

type ValHash<'a> = VSpecHash of System.Collections.Generic.Dictionary<stamp,'a>
let vspec_hash_find    (VSpecHash t) (v:Val) = t.[v.Stamp]
let vspec_hash_tryfind  (VSpecHash t) (v:Val) = let i = v.Stamp in if t.ContainsKey(i) then Some(t.[i]) else None
let vspec_hash_mem      (VSpecHash t) (v:Val) = let i = v.Stamp in t.ContainsKey(i)
let vspec_hash_add      (VSpecHash t) (v:Val) x = let i = v.Stamp in t.[i] <- x
let vspec_hash_remove   (VSpecHash t) (v:Val) = let i = v.Stamp in t.Remove(i) |> ignore
let vspec_hash_create()  =  VSpecHash (new System.Collections.Generic.Dictionary<_,_>(11))

type ValMultiMap<'a> = ValMap<'a list> 
let vspec_mmap_find v (m: ValMultiMap<'a>) = if vspec_map_mem v m then vspec_map_find v m else []
let vspec_mmap_add v x (m: ValMultiMap<'a>) = vspec_map_add v (x :: vspec_mmap_find v m) m
let vspec_mmap_empty () : ValMultiMap<'a> = vspec_map_empty()

type TyparMap<'a> = TPMap of I64map.t<'a>
let tpmap_find (v: Typar) (TPMap m) = I64map.find v.Stamp m
let tpmap_mem (v: Typar) (TPMap m) = I64map.mem v.Stamp m
let tpmap_add (v: Typar) x (TPMap m) = TPMap (I64map.add v.Stamp x m)
let tpmap_empty () = TPMap (I64map.empty ())

type TcrefMap<'a> = TCRefMap of I64map.t<'a>
let tcref_map_find    (v: TyconRef) (TCRefMap m) = I64map.find v.Stamp m
let tcref_map_tryfind (v: TyconRef) (TCRefMap m) = I64map.tryfind v.Stamp m
let tcref_map_mem     (v: TyconRef) (TCRefMap m) = I64map.mem v.Stamp m
let tcref_map_add     (v: TyconRef) x (TCRefMap m) = TCRefMap (I64map.add v.Stamp x m)
let tcref_map_empty  () = TCRefMap (I64map.empty ())
let tcref_map_is_empty (TCRefMap m) = Zmap.is_empty m
let tcref_map_of_list vs = List.foldBack (fun (x,y) acc -> tcref_map_add x y acc) vs (tcref_map_empty()) 

type TcrefMultiMap<'a> = 'a list TcrefMap
let tcref_mmap_find v (m: TcrefMultiMap<'a>) = if tcref_map_mem v m then tcref_map_find v m else []
let tcref_mmap_add v x (m: TcrefMultiMap<'a>) = tcref_map_add v (x :: tcref_mmap_find v m) m
let tcref_mmap_empty () : TcrefMultiMap<'a> = tcref_map_empty()

//--------------------------------------------------------------------------
// Substitute for type variables and remap type constructors 
//--------------------------------------------------------------------------

type TyparInst = (Typar * typ) list
type tpenv = TyparInst

type TyconRefRemap = TyconRef TcrefMap
type ValRemap = ValMap<ValRef>

let empty_tpenv = ([] : tpenv)
let empty_tpinst = ([] : TyparInst)

type Remap =
    { tpinst : TyparInst;
      vspec_remap: ValRemap;
      tcref_remap : TyconRefRemap }

type tyenv = Remap
      
let empty_tcref_remap : TyconRefRemap = tcref_map_empty()
let empty_vref_remap : ValRemap = vspec_map_empty()
let empty_remap = { tpinst = empty_tpinst; tcref_remap =empty_tcref_remap;vspec_remap =empty_vref_remap }
let empty_tyenv = empty_remap

//--------------------------------------------------------------------------
// renamings
//--------------------------------------------------------------------------

let tmenv_add_tcref_remap tcref1 tcref2 tmenv = 
    {tmenv with tcref_remap=tcref_map_add tcref1 tcref2 tmenv.tcref_remap}

let remap_vref tmenv vref = 
    match vspec_map_tryfind (deref_val vref) tmenv.vspec_remap with 
    | None -> vref 
    | Some res -> res

let remap_is_empty tyenv = 
    isNil tyenv.tpinst && 
    tcref_map_is_empty tyenv.tcref_remap && 
    vspec_map_is_empty tyenv.vspec_remap 

let inst_tpref tpinst ty tp  =
    if ListAssoc.containsKey typar_ref_eq tp tpinst then ListAssoc.find typar_ref_eq tp tpinst 
    else ty    (* avoid re-allocation of TType_app node in the common case *)

let inst_tpref_unit tpinst measure (tp:Typar)  =
   if tp.Kind = KindType then failwith "inst_tpref_unit: kind=Type";
   if ListAssoc.containsKey typar_ref_eq tp tpinst then 
       match ListAssoc.find typar_ref_eq tp tpinst with 
       | TType_measure measure -> measure
       | _ ->  failwith "inst_tpref_unit incorrect kind";
   else measure

let remap_tcref tcmap tcr  =
    match tcref_map_tryfind tcr tcmap with 
    | Some tcr ->  tcr
    | None -> tcr

let remap_ucref tcmap (UCRef(tcref,nm)) = UCRef(remap_tcref tcmap tcref,nm)
let remap_rfref tcmap (RFRef(tcref,nm)) = RFRef(remap_tcref tcmap tcref,nm)

let mk_typar_inst (typars: typars) tyargs =  
#if CHECKED
    if List.length typars <> List.length tyargs then
      failwith ("mk_typar_inst: invalid type" ^ (sprintf " %d <> %d" (List.length typars) (List.length tyargs)));
#endif
    (List.zip typars tyargs : TyparInst)

let generalize_typar tp = mk_typar_ty tp
let generalize_typars tps = List.map generalize_typar tps

let rec remap_typeA (tyenv : tyenv) (ty:typ) =
  let ty = strip_tpeqns ty
  match ty with
  | TType_var tp as ty       -> inst_tpref tyenv.tpinst ty tp
  | TType_app (tcr,tinst) as ty -> 
      match tcref_map_tryfind tcr tyenv.tcref_remap with 
      | Some tcr' ->  TType_app (tcr',remap_typesA tyenv tinst)
      | None -> 
          match tinst with 
          | [] -> ty  (* optimization to avoid re-allocation of TType_app node in the common case *)
          | _ -> 
              (* avoid reallocation on idempotent *)
              let tinst' = remap_typesA tyenv tinst
              if tinst === tinst' then ty else 
              TType_app (tcr,tinst')

  | TType_ucase (UCRef(tcr,n),tinst) as ty -> 
      match tcref_map_tryfind tcr tyenv.tcref_remap with 
      | Some tcr' ->  TType_ucase (UCRef(tcr',n),remap_typesA tyenv tinst)
      | None -> TType_ucase (UCRef(tcr,n),remap_typesA tyenv tinst)

  | TType_tuple l  as ty -> 
      let l' = remap_typesA tyenv l
      if l === l' then ty else  
      TType_tuple (l')
  | TType_fun (d,r) as ty      -> 
      let d' = remap_typeA tyenv d
      let r' = remap_typeA tyenv r
      if d === d' && r === r' then ty else
      TType_fun (d', r')
  | TType_forall (tps,ty) -> 
      let tps',tyenv = copy_remap_and_bind_typars tyenv tps
      TType_forall (tps', remap_typeA tyenv ty)
  | TType_modul_bindings          -> ty
  | TType_measure measure -> 
      TType_measure (remap_measureA tyenv measure)

and remap_measureA tyenv measure =
    match measure with
    | MeasureOne -> measure
    | MeasureCon tcr ->
        match tcref_map_tryfind tcr tyenv.tcref_remap with 
        | Some tcr ->  MeasureCon tcr
        | None -> measure
    | MeasureProd(u1,u2) -> MeasureProd(remap_measureA tyenv u1, remap_measureA tyenv u2)
    | MeasureInv u -> MeasureInv(remap_measureA tyenv u)
    | MeasureVar tp as measure -> 
      match tp.Solution with
       | None -> 
          if ListAssoc.containsKey typar_ref_eq tp tyenv.tpinst then 
              match ListAssoc.find typar_ref_eq tp tyenv.tpinst with 
              | TType_measure measure -> measure
              | _ -> failwith "remap_measureA: incorrect kinds"
          else measure
       | Some (TType_measure measure) -> remap_measureA tyenv measure
       | Some ty -> failwithf "incorrect kinds: %A" ty
and remap_typesA tyenv types = List.mapq (remap_typeA tyenv) types
and remap_typar_constraintsA tyenv cs =
   cs |>  List.choose (fun x -> 
         match x with 
         | TTyparCoercesToType(ty,m) -> 
             Some(TTyparCoercesToType (remap_typeA tyenv ty,m))
         | TTyparMayResolveMemberConstraint(traitInfo,m) -> 
             Some(TTyparMayResolveMemberConstraint (remap_traitA tyenv traitInfo,m))
         | TTyparDefaultsToType(priority,ty,m) -> Some(TTyparDefaultsToType(priority,remap_typeA tyenv ty,m))
         | TTyparIsEnum(uty,m) -> 
             Some(TTyparIsEnum(remap_typeA tyenv uty,m))
         | TTyparIsDelegate(uty1,uty2,m) -> 
             Some(TTyparIsDelegate(remap_typeA tyenv uty1,remap_typeA tyenv uty2,m))
         | TTyparSimpleChoice(tys,m) -> Some(TTyparSimpleChoice(remap_typesA tyenv tys,m))
         | TTyparSupportsNull _ | TTyparIsNotNullableValueType _ 
         | TTyparIsReferenceType _ | TTyparRequiresDefaultConstructor _ -> Some(x))

and remap_traitA tyenv (TTrait(typs,nm,mf,argtys,rty,slnCell)) =
    let slnCell = 
        match !slnCell with 
        | None -> None
        | Some sln -> 
            let sln = 
                match sln with 
                | ILMethSln(typ,extOpt,mref,minst) ->
                     ILMethSln(remap_typeA tyenv typ,extOpt,mref,remap_typesA tyenv minst)  
                | FSMethSln(typ, vref,minst) ->
                     FSMethSln(remap_typeA tyenv typ, remap_vref tyenv vref,remap_typesA tyenv minst)  
                | BuiltInSln -> 
                     BuiltInSln
            Some sln
    // Note: we reallocate a new solution cell on every traversal of a trait constraint
    // This feels incorrect for trait constraints that are quantified: it seems we should have 
    // formal binders for trait constraints when they are quantified, just as
    // we have formal binders for type variables.
    //
    // The danger here is that a solution for one syntactic occurrence of a trait constraint won't
    // be propagated to other, "linked" solutions. However trait constraints don't appear in any algebrra
    // in the same way as types
    TTrait(remap_typesA tyenv typs,nm,mf,remap_typesA tyenv argtys, Option.map (remap_typeA tyenv) rty,ref slnCell)


and bind_typars tps tyargs tpinst =   
    match tps with 
    | [] -> tpinst 
    | _ -> List.map2 (fun tp tyarg -> (tp,tyarg)) tps tyargs @ tpinst 

(* This version is used to remap most type parameters, e.g. ones bound at tycons, vals, records *)
(* See notes below on remap_type_full for why we have a function that accepts remap_attribs as an argument *)
and copy_remap_and_bind_typars_full remap_attrib tyenv tps =
    match tps with 
    | [] -> tps,tyenv 
    | _ -> 
      let tps' = CopyTypars tps
      let tyenv = { tyenv with tpinst = bind_typars tps (generalize_typars tps') tyenv.tpinst } 
      (tps,tps') ||> List.iter2 (fun tporig tp -> 
         fixup_typar_constraints tp (remap_typar_constraintsA tyenv  tporig.Constraints);
         tp.Data.typar_attribs  <- tporig.Data.typar_attribs |> List.map remap_attrib) ;
      tps',tyenv

(* copies bound typars, extends tpinst *)
and copy_remap_and_bind_typars tyenv tps =
    copy_remap_and_bind_typars_full (fun _ -> failwith "Unexpected attribute in first-class Type_forall") tyenv tps

let remap_type  tyenv x =
    if remap_is_empty tyenv then x else
    remap_typeA tyenv x

let remap_types tyenv x = 
    if remap_is_empty tyenv then x else 
    remap_typesA tyenv x

/// Use this one for any type that may be a forall type where the type variables may contain attributes 
/// Logically speaking this is mtuually recursive with remap_attrib defined much later in this file, 
/// because types may contain forall types that contain attributes, which need to be remapped. 
/// We currently break the recursion by passing in remap_attrib as a function parameter. 
/// Use this one for any type that may be a forall type where the type variables may contain attributes 
let remap_type_full remap_attrib tyenv ty =
    if remap_is_empty tyenv then ty else 
    match strip_tpeqns ty with
    | TType_forall(tps,tau) -> 
        let tps',tyenvinner = copy_remap_and_bind_typars_full remap_attrib tyenv tps
        TType_forall(tps',remap_type tyenvinner tau)
    | _ -> 
        try remap_type tyenv ty
        with e -> failwith "error in remap_type_full"

let remap_param tyenv (TSlotParam(nm,typ,fl1,fl2,fl3,attribs) as x) = 
    if remap_is_empty tyenv then x else 
    TSlotParam(nm,remap_typeA tyenv typ,fl1,fl2,fl3,attribs) 

let remap_slotsig remap_attrib tyenv (TSlotSig(nm,typ, ctps,methTypars,paraml, rty) as x) =
    if remap_is_empty tyenv then x else 
    let typ' = remap_typeA tyenv typ
    let ctps',tyenvinner = copy_remap_and_bind_typars_full remap_attrib tyenv ctps
    let methTypars',tyenvinner = copy_remap_and_bind_typars_full remap_attrib tyenvinner methTypars
    TSlotSig(nm,typ', ctps',methTypars',List.mapSquared (remap_param tyenvinner) paraml,Option.map (remap_typeA tyenvinner) rty) 

let mk_inst_tyenv tpinst = { tcref_remap= empty_tcref_remap; tpinst=tpinst; vspec_remap=empty_vref_remap }

(* entry points for "typar -> typ" instantiation *)
let InstType              tpinst x = if isNil tpinst then x else remap_typeA  (mk_inst_tyenv tpinst) x
let inst_types             tpinst x = if isNil tpinst then x else remap_typesA (mk_inst_tyenv tpinst) x
let inst_trait             tpinst x = if isNil tpinst then x else remap_traitA (mk_inst_tyenv tpinst) x
let inst_typar_constraints tpinst x = if isNil tpinst then x else remap_typar_constraintsA (mk_inst_tyenv tpinst) x
let inst_slotsig tpinst ss = remap_slotsig (fun _ -> failwith "Unexpected attribute in first-class Type_forall") (mk_inst_tyenv tpinst) ss
let copy_slotsig ss = remap_slotsig (fun _ -> failwith "Unexpected attribute in first-class Type_forall") empty_remap ss

let mk_typar_to_typar_renaming tpsorig tps = 
    let tinst = generalize_typars tps
    mk_typar_inst tpsorig tinst,tinst

let mk_tycon_inst (tycon:Tycon) tinst = mk_typar_inst tycon.TyparsNoRange tinst
let mk_tcref_inst tcref tinst = mk_tycon_inst (deref_tycon tcref) tinst

//---------------------------------------------------------------------------
// Remove inference equations and abbreviations from units
//---------------------------------------------------------------------------

let reduce_tcref_abbrev_measure (tcref:TyconRef) = 
    let abbrev = tcref.TypeAbbrev
    match abbrev with 
    | Some (TType_measure abbrev_measure) -> abbrev_measure 
    | _ -> invalid_arg "reduce_tcref_abbrev_measure: no abbreviation or incorrect kind"

let rec strip_tpeqns_and_tcabbrevsA_measure canShortcut measure = 
    match strip_upeqnsA canShortcut measure with 
    | MeasureCon tcref when tcref.IsTypeAbbrev  ->  
        strip_tpeqns_and_tcabbrevsA_measure canShortcut (reduce_tcref_abbrev_measure tcref) 
    | m -> m

let strip_tpeqns_and_tcabbrevs_measure m = strip_tpeqns_and_tcabbrevsA_measure false m

let tcref_eq g tcref1 tcref2 = prim_tcref_eq g.compilingFslib g.fslibCcu tcref1 tcref2
let tycon_eq (tycon1:Entity) (tycon2:Entity) = (tycon1.Stamp = tycon2.Stamp)

//---------------------------------------------------------------------------
// Basic unit stuff
//---------------------------------------------------------------------------


/// What is the contribution of unit-of-measure constant ucref to unit-of-measure expression measure? 
let rec MeasureConExponent g abbrev ucref unt =
    match (if abbrev then strip_tpeqns_and_tcabbrevs_measure unt else strip_upeqns unt) with
    | MeasureCon ucref' -> if tcref_eq g ucref' ucref then 1 else 0
    | MeasureInv unt' -> -(MeasureConExponent g abbrev ucref unt')
    | MeasureProd(unt1,unt2) -> MeasureConExponent g abbrev ucref unt1 + MeasureConExponent g abbrev ucref unt2
    | _ -> 0

/// What is the contribution of unit-of-measure constant ucref to unit-of-measure expression measure
/// after remapping tycons? 
let rec MeasureConExponentAfterRemapping g r ucref unt =
    match strip_tpeqns_and_tcabbrevs_measure unt with
    | MeasureCon ucref' -> if tcref_eq g (r ucref') ucref then 1 else 0
    | MeasureInv unt' -> -(MeasureConExponentAfterRemapping g r ucref unt')
    | MeasureProd(unt1,unt2) -> MeasureConExponentAfterRemapping g r ucref unt1 + MeasureConExponentAfterRemapping g r ucref unt2
    | _ -> 0

/// What is the contribution of unit-of-measure variable tp to unit-of-measure expression unt? 
let rec MeasureVarExponent tp unt =
    match strip_tpeqns_and_tcabbrevs_measure unt with
    | MeasureVar tp' -> if typar_ref_eq tp tp' then 1 else 0
    | MeasureInv unt' -> -(MeasureVarExponent tp unt')
    | MeasureProd(unt1,unt2) -> MeasureVarExponent tp unt1 + MeasureVarExponent tp unt2
    | _ -> 0

/// List the *literal* occurrences of unit variables in a unit expression, without repeats  
let ListMeasureVarOccs unt =
    let rec gather acc unt =  
        match strip_tpeqns_and_tcabbrevs_measure unt with
          MeasureVar tp -> if List.exists (typar_ref_eq tp) acc then acc else tp::acc
        | MeasureProd(unt1,unt2) -> gather (gather acc unt1) unt2
        | MeasureInv unt' -> gather acc unt'
        | _ -> acc   
    gather [] unt

/// List the *observable* occurrences of unit variables in a unit expression, without repeats, paired with their non-zero exponents
let ListMeasureVarOccsWithNonZeroExponents untexpr =
    let rec gather acc unt =  
        match strip_tpeqns_and_tcabbrevs_measure unt with
          MeasureVar tp -> if List.exists (fun (tp', _) -> typar_ref_eq tp tp') acc then acc 
                           else let e = MeasureVarExponent tp untexpr in if e=0 then acc else (tp,e)::acc
        | MeasureProd(unt1,unt2) -> gather (gather acc unt1) unt2
        | MeasureInv unt' -> gather acc unt'
        | _ -> acc   
    gather [] untexpr

/// List the *observable* occurrences of unit constants in a unit expression, without repeats, paired with their non-zero exponents
let ListMeasureConOccsWithNonZeroExponents g eraseAbbrevs untexpr =
    let rec gather acc unt =  
        match (if eraseAbbrevs then strip_tpeqns_and_tcabbrevs_measure unt else strip_upeqns unt) with
        | MeasureCon c -> if List.exists (fun (c', _) -> tcref_eq g c c') acc then acc 
                          else let e = MeasureConExponent g eraseAbbrevs c untexpr in if e=0 then acc else (c,e)::acc
        | MeasureProd(unt1,unt2) -> gather (gather acc unt1) unt2
        | MeasureInv unt' -> gather acc unt'
        | _ -> acc  
    gather [] untexpr

/// List the *literal* occurrences of unit constants in a unit expression, without repeats,
/// and after applying a remapping function r to tycons
let ListMeasureConOccsAfterRemapping g r unt =
    let rec gather acc unt =  
        match (strip_tpeqns_and_tcabbrevs_measure unt) with
        | MeasureCon c -> if List.exists (tcref_eq g (r c)) acc then acc else r c::acc
        | MeasureProd(unt1,unt2) -> gather (gather acc unt1) unt2
        | MeasureInv unt' -> gather acc unt'
        | _ -> acc
   
    gather [] unt

/// Construct a measure expression representing the n'th power of a measure
let rec MeasurePower u n = 
    if n=0 then MeasureOne
    elif n=1 then u
    elif n<0 then MeasureInv (MeasurePower u (-n))
    else MeasureProd(u,MeasurePower u (n-1))

/// Construct a measure expression representing the product of a list of measures
let ProdMeasures ms = List.foldBack (fun m1 m2 -> MeasureProd (m1,m2)) ms MeasureOne

let is_dimensionless g tyarg =
    match strip_tpeqns tyarg with
    | TType_measure unt ->
      isNil (ListMeasureVarOccsWithNonZeroExponents unt) && 
      isNil (ListMeasureConOccsWithNonZeroExponents g true unt)
    | _ -> false

//--------------------------------------------------------------------------
// Tuple compilation (types)
//------------------------------------------------------------------------ 

let maxTuple = 8
let goodTupleFields = maxTuple-1

let is_tuple_tcref g tcref =
    match tcref with
    | x when g.tuple1_tcr = x || g.tuple2_tcr = x || g.tuple3_tcr = x || g.tuple4_tcr = x || g.tuple5_tcr = x || g.tuple6_tcr = x || g.tuple7_tcr = x || g.tuple8_tcr = x -> true
    | _ -> false

let compiled_tuple_tcref g tys = 
    let n = List.length tys 
    if   n = 1 then g.tuple1_tcr
    elif n = 2 then g.tuple2_tcr
    elif n = 3 then g.tuple3_tcr
    elif n = 4 then g.tuple4_tcr
    elif n = 5 then g.tuple5_tcr
    elif n = 6 then g.tuple6_tcr
    elif n = 7 then g.tuple7_tcr
    elif n = 8 then g.tuple8_tcr
    else failwithf "compiled_tuple_tcref, n = %d" n

let rec compiled_tuple_ty g tys = 
    let n = List.length tys 
    if n < maxTuple then TType_app (compiled_tuple_tcref g tys, tys)
    else 
        let tysA,tysB = split_after goodTupleFields tys
        TType_app (g.tuple8_tcr, tysA@[compiled_tuple_ty g tysB])

//---------------------------------------------------------------------------
// Remove inference equations and abbreviations from types 
//---------------------------------------------------------------------------


let apply_tycon_abbrev abbrev_ty tycon tyargs = 
    if isNil tyargs then abbrev_ty 
    else InstType (mk_tycon_inst tycon tyargs) abbrev_ty

let reduce_tycon_abbrev (tycon:Tycon) tyargs = 
    let abbrev = tycon.TypeAbbrev
    match abbrev with 
    | None -> invalidArg "tycon" "this type definition is not an abbreviation";
    | Some abbrev_ty -> 
        apply_tycon_abbrev abbrev_ty tycon tyargs

let reduce_tcref_abbrev (tcref:TyconRef) tyargs = 
    reduce_tycon_abbrev tcref.Deref tyargs

let reduce_tycon_measureable (tycon:Tycon) tyargs = 
    let repr = tycon.TypeReprInfo
    match repr with 
    | Some (TMeasureableRepr ty) -> 
        if isNil tyargs then ty else InstType (mk_tycon_inst tycon tyargs) ty
    | _ -> invalidArg "tc" "this type definition is not a refinement"

let reduce_tcref_measureable (tcref:TyconRef) tyargs = 
    reduce_tycon_measureable tcref.Deref tyargs

let rec strip_tpeqns_and_tcabbrevsA g canShortcut ty = 
    let ty = strip_tpeqnsA canShortcut ty 
    match ty with 
    | TType_app (tcref,tinst) -> 
        let tycon = tcref.Deref
        match tycon.TypeAbbrev with 
        | Some abbrev_ty -> 
            strip_tpeqns_and_tcabbrevsA g canShortcut (apply_tycon_abbrev abbrev_ty tycon tinst)
        | None -> 
            if tycon.IsMeasureableReprTycon && List.forall (is_dimensionless g) tinst then
                strip_tpeqns_and_tcabbrevsA g canShortcut (reduce_tycon_measureable tycon tinst)
            else ty
    | ty -> ty

let strip_tpeqns_and_tcabbrevs g ty = strip_tpeqns_and_tcabbrevsA g false ty

/// This erases outermost occurences of inference equations, type abbreviations and measureable types (float<_>).
/// It also optionally erases all "compilation representations", i.e. function and
/// tuple types, and also "nativeptr<'T> --> System.IntPtr"
let rec strip_tpeqns_and_tcabbrevs_and_erase eraseFuncAndTuple g ty =
    let ty = strip_tpeqns_and_tcabbrevs g ty
    match ty with
    | TType_app (tcref,args) -> 
        let tycon = tcref.Deref
        if tycon.IsMeasureableReprTycon  then
            strip_tpeqns_and_tcabbrevs_and_erase eraseFuncAndTuple g (reduce_tycon_measureable tycon args)
        elif tcref_eq g tcref g.nativeptr_tcr && eraseFuncAndTuple then 
            strip_tpeqns_and_tcabbrevs_and_erase eraseFuncAndTuple g g.nativeint_ty
        else
            ty
    | TType_fun(a,b) when eraseFuncAndTuple -> TType_app(g.fastFunc_tcr,[ a; b]) 
    | TType_tuple(l) when eraseFuncAndTuple -> compiled_tuple_ty g l
    | ty -> ty

let strip_tpeqns_and_tcabbrevs_and_measureable g ty =
   strip_tpeqns_and_tcabbrevs_and_erase false g ty
       
type Erasure = EraseAll | EraseMeasures | EraseNone

let  strip_tpeqns_and_tcabbrevs_wrt_erasure erasureFlag g ty = 
    match erasureFlag with 
    | EraseAll -> strip_tpeqns_and_tcabbrevs_and_erase true g ty
    | EraseMeasures -> strip_tpeqns_and_tcabbrevs_and_erase false g ty
    | _ -> strip_tpeqns_and_tcabbrevs g ty

    
let rec strip_eqns_from_ecref (eref:TyconRef) = 
    let exnc = deref_tycon eref
    match exnc.ExceptionInfo with
    | TExnAbbrevRepr eref -> strip_eqns_from_ecref eref
    | _ -> exnc

let dest_unpar_measure g unt =
    let vs = ListMeasureVarOccsWithNonZeroExponents unt
    let cs = ListMeasureConOccsWithNonZeroExponents g true unt
    match vs, cs with
    | [(v,1)], [] -> v
    | _, _ -> failwith "dest_unpar_measure: not a unit-of-measure parameter"

let is_unpar_measure g unt =
    let vs = ListMeasureVarOccsWithNonZeroExponents unt
    let cs = ListMeasureConOccsWithNonZeroExponents g true unt
 
    match vs, cs with
    | [(_,1)], [] -> true
    | _,   _ -> false


let prim_dest_forall_typ g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_forall (tyvs,tau) -> (tyvs,tau) | _ -> failwith "prim_dest_forall_typ: not a forall type")
let dest_fun_typ      g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_fun (tyv,tau) -> (tyv,tau) | _ -> failwith "dest_fun_typ: not a function type")
let dest_tuple_typ    g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_tuple l -> l | _ -> failwith "dest_tuple_typ: not a tuple type")
let dest_typar_typ    g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_var v -> v | _ -> failwith "dest_typar_typ: not a typar type")
let dest_anypar_typ   g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_var v -> v | TType_measure unt -> dest_unpar_measure g unt | _ -> failwith "dest_anypar_typ: not a typar or unpar type")
let dest_measure_typ  g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_measure m -> m | _ -> failwith "dest_measure_typ: not a unit-of-measure type")
let is_fun_typ        g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_fun _ -> true | _ -> false)
let is_forall_typ     g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_forall _ -> true | _ -> false)
let is_tuple_typ      g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_tuple _ -> true | _ -> false)
let is_union_typ      g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app(tcr,_) -> tcr.IsUnionTycon | _ -> false)
let is_repr_hidden_typ   g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app(tcr,_) -> tcr.IsHiddenReprTycon | _ -> false)
let is_fsobjmodel_typ g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app(tcr,_) -> tcr.IsFSharpObjectModelTycon | _ -> false)
let is_recd_typ       g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app(tcr,_) -> tcr.IsRecordTycon | _ -> false)
let is_typar_typ      g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_var _ -> true | _ -> false)
let is_anypar_typ     g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_var _ -> true | TType_measure unt -> is_unpar_measure g unt | _ -> false)
let is_measure_typ    g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_measure _ -> true | _ -> false)

// WARNING: If you increase this you must make the corresponding types in FSharp.Core.dll structs
#if TUPLE_STRUXT
let highestTupleStructType = 2
let is_tuple_struct_typ g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_tuple l -> l.Length <= highestTupleStructType | _ -> false)
#else
let is_tuple_struct_typ (g:TcGlobals) (ty:typ) = false
#endif


let is_proven_ucase_typ ty = match ty with TType_ucase _ -> true | _ -> false

let mk_tyapp_ty tcref tyargs = TType_app(tcref,tyargs)
let mk_proven_ucase_typ ucref tyargs = TType_ucase(ucref,tyargs)
let is_stripped_tyapp_typ   g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app _ -> true | _ -> false) 
let dest_stripped_tyapp_typ g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app(tcref,tinst) -> tcref,tinst | _ -> failwith "dest_stripped_tyapp_typ") 
let tcref_of_stripped_typ   g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app(tcref,_) -> tcref | _ -> failwith "tcref_of_stripped_typ") 
let try_tcref_of_stripped_typ   g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app(tcref,_) -> Some tcref | _ -> None) 
let tinst_of_stripped_typ   g ty = ty |> strip_tpeqns_and_tcabbrevs g |> (function TType_app(_,tinst) -> tinst | _ -> []) 
let tycon_of_stripped_typ   g ty = deref_tycon (tcref_of_stripped_typ g ty)

let mk_inst_for_stripped_typ g typ = 
    if is_stripped_tyapp_typ g typ then 
      let tcref,tinst = dest_stripped_tyapp_typ g typ
      mk_tcref_inst tcref tinst
    else []

let domain_of_fun_typ g ty = fst(dest_fun_typ g ty)
let range_of_fun_typ  g ty = snd(dest_fun_typ g ty)

let contains_measures_typ g ty = 
    let rec contains ty =
        match strip_tpeqns_and_tcabbrevs g ty with 
        | TType_tuple l -> List.exists contains l
        | TType_app (_,tinst) -> List.exists contains tinst
        | TType_ucase (_,tinst) -> List.exists contains tinst
        | TType_fun (d,r) -> contains d || contains r
        | TType_var r -> match r.Kind with KindMeasure -> true | _ -> false
        | TType_forall (tps,r) -> contains r
        | TType_modul_bindings -> failwith "contains_measures_typ: naked struct"
        | TType_measure unt -> true
    contains ty

//---------------------------------------------------------------------------
// Type information about records, constructors etc.
//---------------------------------------------------------------------------
 
let typ_of_rfield inst (fspec:RecdField)  = InstType inst fspec.FormalType

let typs_of_rfields inst rfields = List.map (typ_of_rfield inst) rfields

let typs_of_instance_rfields_of_tcref inst (tcref:TyconRef) = tcref.AllInstanceFieldsAsList |>  typs_of_rfields inst 

let rfield_tables_of_ucref (x:UnionCaseRef) = x.UnionCase.ucase_rfields 
let rfields_of_ucref x = (rfield_tables_of_ucref x).AllFieldsAsList
let rfield_of_ucref_by_idx x n = (rfield_tables_of_ucref x).FieldByIndex n

let rty_of_ucref (x:UnionCaseRef) = x.UnionCase.ucase_rty
let typs_of_ucref_rfields inst x = typs_of_rfields inst (rfields_of_ucref x)

let typ_of_ucref_rfield_by_idx (x:UnionCaseRef) tinst j = 
    let tcref = x.TyconRef
    let inst = mk_tcref_inst tcref tinst
    typ_of_rfield inst (rfield_of_ucref_by_idx x j)

let rty_of_uctyp (x:UnionCaseRef) tinst = 
    let tcref = x.TyconRef
    let inst = mk_tcref_inst tcref tinst
    InstType inst (rty_of_ucref x)

let rfields_of_ecref x = (strip_eqns_from_ecref x).TrueInstanceFieldsAsList
let rfield_of_ecref_by_idx x n = (strip_eqns_from_ecref x).GetFieldByIndex n

let typs_of_ecref_rfields x = typs_of_rfields [] (rfields_of_ecref x)
let typ_of_ecref_rfield x j = typ_of_rfield [] (rfield_of_ecref_by_idx x j)

(* REVIEW: these could be faster, e.g. by storing the index in the NameMap *)
let ucref_index (UCRef(tcref,id)) = try tcref.UnionCasesArray |> Array.find_index (fun ucspec -> ucspec.DisplayName = id) with Not_found -> error(InternalError(Printf.sprintf "constructor %s not found in type %s" id tcref.MangledName, tcref.Range))
let rfref_index (RFRef(tcref,id)) = try tcref.AllFieldsArray |> Array.find_index (fun rfspec -> rfspec.Name = id)  with Not_found -> error(InternalError(Printf.sprintf "field %s not found in type %s" id tcref.MangledName, tcref.Range))

let ucrefs_of_tcref          (tcref:TyconRef) = tcref.UnionCasesAsList         |> List.map (ucref_of_ucase tcref) 
let instance_rfrefs_of_tcref (tcref:TyconRef) = tcref.TrueInstanceFieldsAsList |> List.map (rfref_of_rfield tcref) 
let all_rfrefs_of_tcref      (tcref:TyconRef) = tcref.AllFieldsAsList          |> List.map (rfref_of_rfield tcref) 

let actual_typ_of_rfield tycon tinst (fspec:RecdField) = 
    InstType (mk_tycon_inst tycon tinst) fspec.FormalType

let actual_rtyp_of_rfref (fref:RecdFieldRef) tinst = 
    actual_typ_of_rfield fref.Tycon tinst fref.RecdField

let formal_typ_of_tcref g (tcref:TyconRef) = 
    TType_app(tcref,List.map mk_typar_ty tcref.TyparsNoRange)

let enclosing_formal_typ_of_val g (v:Val) = formal_typ_of_tcref g v.MemberApparentParent
    
//---------------------------------------------------------------------------
// Apply type functions to types
//---------------------------------------------------------------------------

let NormalizeDeclaredTyparsForEquiRecursiveInference g tps = 
    match tps with 
    | [] -> []
    | tps -> 
        tps |> List.map (fun tp -> 
          let ty =  mk_typar_ty tp
          if is_anypar_typ g ty then dest_anypar_typ g ty else tp)

let dest_forall_typ g ty = 
    let tps,tau = prim_dest_forall_typ g ty 
    // tps may be have been equated to other tps in equi-recursive type inference 
    // and unit type inference. Normalize them here 
    let tps = NormalizeDeclaredTyparsForEquiRecursiveInference g tps
    tps,tau

let try_dest_forall_typ g ty = 
    if is_forall_typ g ty then dest_forall_typ g ty else ([],ty) 


let rec strip_fun_typ g ty = 
    if is_fun_typ g ty then 
        let (d,r) = dest_fun_typ g ty 
        let more,rty = strip_fun_typ g r 
        d::more, rty
    else [],ty

let reduce_forall_typ g ty tyargs = 
    let tps,tau = dest_forall_typ g ty
    InstType (mk_typar_inst tps tyargs) tau

let reduce_iterated_fun_ty g ty args = 
    List.fold (fun ty _ -> 
        if not (is_fun_typ g ty) then failwith "reduce_iterated_fun_ty";
        snd (dest_fun_typ g ty)) ty args

let apply_types g functy (tyargs,argtys) = 
    let after_tyapp_ty = if is_forall_typ g functy then reduce_forall_typ g functy tyargs else functy
    reduce_iterated_fun_ty g after_tyapp_ty argtys

let formal_apply_types g functy (tyargs,args) = 
    reduce_iterated_fun_ty g
      (if isNil tyargs then functy else snd (dest_forall_typ g functy))
      args

let rec strip_fun_typ_upto g n ty = 
    assert (n >= 0);
    if n > 0 && is_fun_typ g ty then 
        let (d,r) = dest_fun_typ g ty
        let more,rty = strip_fun_typ_upto g (n-1) r in d::more, rty
    else [],ty

        
let try_dest_tuple_typ g ty = 
    if is_tuple_typ g ty then dest_tuple_typ g ty else [ty]

type UncurriedArgInfos = (typ * TopArgInfo) list 
type CurriedArgInfos = (typ * TopArgInfo) list list

(* A 'tau' type is one with its type paramaeters stripped off *)
let GetTopTauTypeInFSharpForm g (curriedArgInfos: TopArgInfo list list) tau m =
    let argtys,rty = strip_fun_typ_upto g curriedArgInfos.Length tau
    if curriedArgInfos.Length <> argtys.Length then 
        error(Error("Invalid member signature encountered because of an earlier error",m))
    let argtysl = 
        (curriedArgInfos,argtys) ||> List.map2 (fun argInfos argty -> 
            match argInfos with 
            | [] -> [ (g.unit_ty, TopValInfo.unnamedTopArg1) ]
            | [argInfo] -> [ (argty, argInfo) ]
            | _ -> List.zip (dest_tuple_typ g argty) argInfos) 
    argtysl,rty

let dest_top_forall_type g (TopValInfo (ntps,argInfos,retInfo) as topValInfo) ty =
    let tps,tau = (if isNil ntps then [],ty else try_dest_forall_typ g ty)
#if CHECKED
    if tps.Length <> kinds.Length then failwith (sprintf "dest_top_forall_type: internal error, #tps = %d, #ntps = %d" (List.length tps) ntps);
#endif
    (* tps may be have been equated to other tps in equi-recursive type inference. Normalize them here *)
    let tps = NormalizeDeclaredTyparsForEquiRecursiveInference g tps
    tps,tau

let GetTopValTypeInFSharpForm g (TopValInfo(_,argInfos,retInfo) as topValInfo) ty m =
    let tps,tau = dest_top_forall_type g topValInfo ty
    let argtysl,rty = GetTopTauTypeInFSharpForm g argInfos tau m
    tps,argtysl,rty,retInfo

let IsCompiledAsStaticValue g (v:Val) = 
    (isSome v.TopValInfo &&
     match GetTopValTypeInFSharpForm g v.TopValInfo.Value v.Type v.Range with 
     | [],[], _,_ when not v.IsMember -> true
     | _ -> false) 

//-------------------------------------------------------------------------
// Multi-dimensional array types...
//-------------------------------------------------------------------------

let is_il_arr_tcref g tcr = 
    tcref_eq g tcr g.il_arr1_tcr || 
    tcref_eq g tcr g.il_arr2_tcr || 
    tcref_eq g tcr g.il_arr3_tcr || 
    tcref_eq g tcr g.il_arr4_tcr 

let rank_of_il_arr_tcref g tcr = 
    if tcref_eq g tcr g.il_arr1_tcr then 1
    elif tcref_eq g tcr g.il_arr2_tcr then 2
    elif tcref_eq g tcr g.il_arr3_tcr then 3
    elif tcref_eq g tcr g.il_arr4_tcr then 4
    else failwith "rank_of_il_arr_tcref: unsupported array rank"

//-------------------------------------------------------------------------
// Misc functions on F# types
//------------------------------------------------------------------------- 

let get_array_element_typ (g:TcGlobals) ty =
    let tcr,tinst = dest_stripped_tyapp_typ g ty
    match tinst with 
    | [ty] -> ty
    | _ -> failwith "get_array_element_typ";

let is_il_arr_typ g ty =
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> is_il_arr_tcref g tcref

let is_il_arr1_typ g ty =
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref_eq g tcref g.il_arr1_tcr 

let dest_il_arr1_typ g ty = get_array_element_typ g ty

let is_compat_array_typ g ty = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref_eq g g.array_tcr tcref

let is_unit_typ g ty = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref_eq g g.unit_tcr_canon tcref

let is_obj_typ g ty = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref_eq g g.system_Object_tcref tcref

let is_void_typ (g:TcGlobals) ty = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref_eq g g.system_Void_tcref tcref

let is_il_named_typ g ty = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref.IsILTycon


let is_il_class_typ g ty = 
    (is_il_named_typ g ty &&
     let tcref,tinst = dest_stripped_tyapp_typ g ty
     (tcref.ILTyconRawMetadata.tdKind = TypeDef_class))

let is_il_interface_typ g ty = 
    (is_il_named_typ g ty && 
     let tcr,tinst = dest_stripped_tyapp_typ g ty
     (tcr.ILTyconRawMetadata.tdKind = TypeDef_interface))

let is_il_ref_typ g ty = 
    (is_il_named_typ g ty && 
     let tcref,tinst = dest_stripped_tyapp_typ g ty
     not (is_value_or_enum_tdef tcref.ILTyconRawMetadata)) ||
    is_il_arr_typ g ty


let is_il_enum_tycon (tycon:Tycon) = 
    (tycon.IsILTycon && tycon.ILTyconRawMetadata.IsEnum)

let is_il_interface_tycon (tycon:Tycon) = 
    (tycon.IsILTycon && (tycon.ILTyconRawMetadata.tdKind = TypeDef_interface))

let is_il_delegate_tcref (tcref:TyconRef) = 
    if tcref.IsILTycon then 
        match tcref.ILTyconRawMetadata.tdKind with
        | TypeDef_delegate -> true
        | _ -> false
    else false

let is_any_array_typ g ty =  is_il_arr_typ g ty || is_compat_array_typ g ty
let dest_any_array_typ g ty = get_array_element_typ g ty
let rank_of_any_array_typ g ty = 
    if is_il_arr_typ g ty then 
        rank_of_il_arr_tcref g (tcref_of_stripped_typ g ty)
    else 1

let is_fsobjmodel_ref_typ g ty = 
    is_fsobjmodel_typ g ty && 
    let tcr,tinst = dest_stripped_tyapp_typ g ty
    match tcr.FSharpObjectModelTypeInfo.fsobjmodel_kind with 
    | TTyconClass | TTyconInterface   | TTyconDelegate _ -> true
    | TTyconStruct | TTyconEnum -> false

let is_tycon_kind_struct k = 
    match k with 
    | TTyconClass | TTyconInterface   | TTyconDelegate _ -> false
    | TTyconStruct | TTyconEnum -> true

let is_tycon_kind_enum k = 
    match k with 
    | TTyconStruct | TTyconClass | TTyconInterface   | TTyconDelegate _  -> false
    | TTyconEnum -> true

let is_fsobjmodel_class_tycon (x:Tycon) = 
    x.IsFSharpObjectModelTycon &&
    match x.FSharpObjectModelTypeInfo.fsobjmodel_kind with TTyconClass -> true | _ -> false

let is_fsobjmodel_class_typ     g ty = is_stripped_tyapp_typ g ty && is_fsobjmodel_class_tycon (tycon_of_stripped_typ g ty)
let is_fsobjmodel_struct_typ    g ty = is_stripped_tyapp_typ g ty && (tycon_of_stripped_typ g ty).IsFSharpStructTycon
let is_fsobjmodel_interface_typ g ty = is_stripped_tyapp_typ g ty && (tycon_of_stripped_typ g ty).IsFSharpInterfaceTycon
let is_fsobjmodel_delegate_typ  g ty = is_stripped_tyapp_typ g ty && (tycon_of_stripped_typ g ty).IsFSharpDelegateTycon

let is_delegate_typ g ty = 
    is_fsobjmodel_delegate_typ g ty ||
     match try_tcref_of_stripped_typ g ty with 
     | Some tcref -> is_il_delegate_tcref tcref
     | _ -> false

let is_interface_typ g ty = 
    is_il_interface_typ g ty || 
    is_fsobjmodel_interface_typ g ty

let is_class_typ g ty = 
    is_il_class_typ g ty || 
    is_fsobjmodel_class_typ g ty

let is_ref_typ g ty = 
    is_union_typ g ty || 
    is_compat_array_typ g ty ||
    (is_tuple_typ g ty && not (is_tuple_struct_typ g ty)) || 
    is_recd_typ g ty || 
    is_il_ref_typ g ty ||
    is_fun_typ g ty || 
    is_repr_hidden_typ g ty || 
    is_fsobjmodel_ref_typ g ty || 
    is_unit_typ g ty

let is_struct_typ g ty = 
    (is_stripped_tyapp_typ g ty && (tycon_of_stripped_typ g ty).IsStructTycon) || is_tuple_struct_typ g ty

let is_enum_tycon x = 
    is_il_enum_tycon x || x.IsFSharpEnumTycon

let is_interface_tycon x = 
    is_il_interface_tycon x || x.IsFSharpInterfaceTycon

let is_enum_tcref tcref = is_enum_tycon (deref_tycon tcref)
let is_struct_tcref (tcref:TyconRef) = tcref.IsStructTycon
let is_interface_tcref tcref = is_interface_tycon (deref_tycon tcref)

let is_enum_typ g ty = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> is_enum_tcref tcref

let actual_rty_of_slotsig parentTyInst methTyInst (TSlotSig(_,_,parentFormalTypars,methFormalTypars,_,formalRetTy)) = 
    let methTyInst = mk_typar_inst methFormalTypars methTyInst
    let parentTyInst = mk_typar_inst parentFormalTypars parentTyInst
    Option.map (InstType (parentTyInst @ methTyInst)) formalRetTy

let slotsig_has_void_rty (TSlotSig(_,_,_,_,_,formalRetTy)) = 
    isNone formalRetTy 

let rty_of_tmethod g (TObjExprMethod((TSlotSig(_,parentTy,_,_,_,_) as ss),methFormalTypars,_,_,m)) =
    let tinst = tinst_of_stripped_typ g parentTy
    let methTyInst = generalize_typars methFormalTypars
    actual_rty_of_slotsig tinst methTyInst ss

/// Is the type 'abstract' in C#-speak
let is_partially_implemented_tycon (tycon:Tycon) = 
    if tycon.IsFSharpObjectModelTycon then 
      not tycon.IsFSharpDelegateTycon && 
      tycon.TypeContents.tcaug_abstract 
    else 
      (tycon.IsILTycon && tycon.ILTyconRawMetadata.IsAbstract)

//---------------------------------------------------------------------------
// Find all type variables in a type, apart from those that have had 
// an equation assigned by type inference.
//---------------------------------------------------------------------------

let empty_free_locvals = Zset.empty val_spec_order
let union_free_locvals s1 s2 = 
    if s1 == empty_free_locvals then s2
    elif s2 == empty_free_locvals then s1
    else Zset.union s1 s2

let empty_free_rfields = Zset.empty rfref_order
let union_free_rfields s1 s2 = 
    if s1 == empty_free_rfields then s2
    elif s2 == empty_free_rfields then s1
    else Zset.union s1 s2

let empty_free_ucases = Zset.empty ucref_order
let union_free_ucases s1 s2 = 
    if s1 == empty_free_ucases then s2
    elif s2 == empty_free_ucases then s1
    else Zset.union s1 s2

let empty_free_loctycons = Zset.empty tycon_spec_order
let union_free_loctycons s1 s2 = 
    if s1 == empty_free_loctycons then s2
    elif s2 == empty_free_loctycons then s1
    else Zset.union s1 s2

let typar_spec_order (v1:Typar) (v2:Typar) = compare v1.Stamp v2.Stamp (* type instanced *) 

let empty_free_loctypars = Zset.empty typar_spec_order
let union_free_loctypars s1 s2 = 
    if s1 == empty_free_loctypars then s2
    elif s2 == empty_free_loctypars then s1
    else Zset.union s1 s2

let empty_free_tyvars =  
    { FreeTycons=empty_free_loctycons; 
      /// The summary of values used as trait solutions
      FreeTraitSolutions=empty_free_locvals;
      FreeTypars=empty_free_loctypars}

let union_free_tyvars fvs1 fvs2 = 
    if fvs1 == empty_free_tyvars then fvs2 else 
    if fvs2 == empty_free_tyvars then fvs1 else
    { FreeTycons           = union_free_loctycons fvs1.FreeTycons fvs2.FreeTycons;
      FreeTraitSolutions   = union_free_locvals fvs1.FreeTraitSolutions fvs2.FreeTraitSolutions;
      FreeTypars           = union_free_loctypars fvs1.FreeTypars fvs2.FreeTypars }

type FreeVarOptions = 
    { canCache: bool;
      collectInTypes: bool
      includeLocalTycons: bool;
      includeTypars: bool; 
      includeLocalTyconReprs: bool;
      includeRecdFields : bool; 
      includeUnionCases : bool;
      includeLocals : bool }
      
let CollectAllNoCaching = 
        { canCache=false;
          collectInTypes=true;
          includeLocalTycons=true;
          includeLocalTyconReprs=true;
          includeRecdFields =true; 
          includeUnionCases=true;
          includeTypars=true; 
          includeLocals=true }

let CollectTyparsNoCaching = 
        { canCache=false;
          collectInTypes=true;
          includeLocalTycons=false;
          includeTypars=true; 
          includeLocalTyconReprs=false;
          includeRecdFields =false; 
          includeUnionCases=false;
          includeLocals=false }

let CollectLocalsNoCaching = 
        { canCache=false;
          collectInTypes=false;
          includeLocalTycons=false;
          includeTypars=false; 
          includeLocalTyconReprs=false;
          includeRecdFields =false; 
          includeUnionCases=false;
          includeLocals=true }

let CollectTyparsAndLocalsNoCaching = 
        { canCache=false;
          collectInTypes=true;
          includeLocalTycons=false;
          includeLocalTyconReprs=false;
          includeRecdFields =false; 
          includeUnionCases=false;
          includeTypars=true; 
          includeLocals=true }

let CollectAll =
        { canCache=false; 
          collectInTypes=true;
          includeLocalTycons=true;
          includeLocalTyconReprs=true;
          includeRecdFields =true; 
          includeUnionCases=true;
          includeTypars=true; 
          includeLocals=true }
    
let CollectTyparsAndLocals = // CollectAll
        { canCache=true; // only cache for this one
          collectInTypes=true;
          includeTypars=true; 
          includeLocals=true;
          includeLocalTycons=false;
          includeLocalTyconReprs=false;
          includeRecdFields =false; 
          includeUnionCases=false; }


let CollectTypars = CollectTyparsAndLocals
(*
        { canCache=false; 
          collectInTypes=true;
          includeTypars=true; 
          includeLocals=false; 
          includeLocalTycons=false;
          includeLocalTyconReprs=false;
          includeRecdFields =false; 
          includeUnionCases=false;}
*)

let CollectLocals = CollectTyparsAndLocals
(*
        { canCache=false; 
          collectInTypes=false;
          includeLocalTycons=false;
          includeLocalTyconReprs=false;
          includeRecdFields =false; 
          includeUnionCases=false;
          includeTypars=false; 
          includeLocals=true }
*)


let acc_free_loctycon opts x acc = 
    if not opts.includeLocalTycons then acc else
    if Zset.mem x acc.FreeTycons then acc else 
    {acc with FreeTycons = Zset.add x acc.FreeTycons } 

let acc_free_tycon opts (tcr:TyconRef) acc = 
    if not opts.includeLocalTycons then acc else
    match tcr.IsLocalRef with 
    | true -> acc_free_loctycon opts tcr.PrivateTarget acc
    | _ -> acc

let rec bound_typars opts tps acc = 
    // Bound type vars form a recursively-referential set due to constraints, e.g.  A : I<B>, B : I<A> 
    // So collect up free vars in all constraints first, then bind all variables 
    let acc = List.foldBack (fun (tp:Typar) acc -> acc_free_in_typar_constraints opts tp.Constraints acc) tps acc
    List.foldBack (fun tp acc -> {acc with FreeTypars = Zset.remove tp acc.FreeTypars}) tps acc

and acc_free_in_typar_constraints opts cxs acc =
    List.foldBack (acc_free_in_typar_constraint opts) cxs acc

and acc_free_in_typar_constraint opts tpc acc =
    match tpc with 
    | TTyparCoercesToType(typ,m) -> acc_free_in_type opts typ acc
    | TTyparMayResolveMemberConstraint (traitInfo,_) -> acc_free_in_trait opts traitInfo acc
    | TTyparDefaultsToType(_,rty,_) -> acc_free_in_type opts rty acc
    | TTyparSimpleChoice(tys,_) -> acc_free_in_types opts tys acc
    | TTyparIsEnum(uty,m) -> acc_free_in_type opts uty acc
    | TTyparIsDelegate(aty,bty,m) -> acc_free_in_type opts aty (acc_free_in_type opts bty acc)
    | TTyparSupportsNull _ | TTyparIsNotNullableValueType _ | TTyparIsReferenceType _ 
    | TTyparRequiresDefaultConstructor _ -> acc

and acc_free_in_trait opts (TTrait(typs,_,_,argtys,rty,sln)) acc = 
    Option.fold_right (acc_free_in_trait_sln opts) sln.Value
       (acc_free_in_types opts typs 
         (acc_free_in_types opts argtys 
           (Option.fold_right (acc_free_in_type opts) rty acc)))

and acc_free_in_trait_sln opts sln acc = 
    match sln with 
    | ILMethSln(typ,extOpt,mref,minst) ->
         acc_free_in_type opts typ 
            (acc_free_in_types opts minst acc)
    | FSMethSln(typ, vref,minst) ->
         acc_free_in_type opts typ 
            (acc_free_trait_sln_vref opts vref  
               (acc_free_in_types opts minst acc))
    | BuiltInSln -> acc

and acc_free_trait_sln_locval opts v fvs =
    if Zset.mem v fvs.FreeTraitSolutions then fvs 
    else 
        let fvs =  acc_free_in_val opts v fvs
        {fvs with FreeTraitSolutions=Zset.add v fvs.FreeTraitSolutions}
and acc_free_trait_sln_vref opts (vref:ValRef) fvs = 
    match vref.IsLocalRef with 
    | true -> acc_free_trait_sln_locval opts vref.PrivateTarget fvs
    // non-local values do not contain free variables 
    | _ -> fvs

and acc_free_tpref opts (tp:Typar) acc = 
    if not opts.includeTypars then acc else
    if Zset.mem tp acc.FreeTypars then acc
    else 
      acc_free_in_typar_constraints opts tp.Constraints
        {acc with FreeTypars=Zset.add tp acc.FreeTypars}

and acc_free_in_type opts ty acc  = 
    match strip_tpeqns ty with 
    | TType_tuple l -> acc_free_in_types opts l acc
    | TType_app (tc,tinst) -> 
        let acc = acc_free_tycon opts tc  acc
        match tinst with 
        | [] -> acc  // optimization to avoid unneeded call
        | _ -> acc_free_in_types opts tinst acc
    | TType_ucase (UCRef(tc,_),tinst) -> acc_free_in_types opts tinst (acc_free_tycon opts tc  acc)
    | TType_fun (d,r) -> acc_free_in_type opts d (acc_free_in_type opts r acc)
    | TType_var r -> acc_free_tpref opts r acc
    | TType_forall (tps,r) -> union_free_tyvars (bound_typars opts tps (free_in_type opts r)) acc
    | TType_modul_bindings -> failwith "acc_free_in_type opts: naked struct"
    | TType_measure unt -> acc_free_in_unit opts unt acc
and acc_free_in_unit opts unt acc = List.foldBack (fun (tp,_) acc -> acc_free_tpref opts tp acc) (ListMeasureVarOccsWithNonZeroExponents unt) acc
and acc_free_in_types opts tys acc = 
    match tys with 
    | [] -> acc
    | h :: t -> acc_free_in_type opts h (acc_free_in_types opts t acc)
and free_in_type opts ty = acc_free_in_type opts ty empty_free_tyvars

and acc_free_in_val opts (v:Val) acc = acc_free_in_type opts v.Data.val_type acc

let free_in_types opts tys = acc_free_in_types opts tys empty_free_tyvars
let free_in_val opts v = acc_free_in_val opts v empty_free_tyvars
let free_in_typar_constraints opts v = acc_free_in_typar_constraints opts v empty_free_tyvars
let acc_free_tprefs opts tps acc = List.foldBack (acc_free_tpref opts) tps acc
        

//--------------------------------------------------------------------------
// Free in type, left-to-right order preserved. This is used to determine the
// order of type variables for top-level definitions based on their signature,
// so be careful not to change the order.  We accumulate in reverse
// order.
//--------------------------------------------------------------------------

let empty_free_typars_lr = []
let union_free_typars_lr fvs1 fvs2 = ListSet.unionFavourRight typar_ref_eq fvs1 fvs2

let rec bound_typars_lr g cxFlag thruFlag acc tps = 
    (* Bound type vars form a recursively-referential set due to constraints, e.g.  A : I<B>, B : I<A> *)
    (* So collect up free vars in all constraints first, then bind all variables *)
    let acc = List.fold (fun acc (tp:Typar) -> acc_free_in_typar_constraints_lr g cxFlag thruFlag acc tp.Constraints) tps acc
    List.foldBack (ListSet.remove typar_ref_eq) tps acc

and acc_free_in_typar_constraints_lr g cxFlag thruFlag acc cxs =
    List.fold (acc_free_in_typar_constraint_lr g cxFlag thruFlag) acc cxs 

and acc_free_in_typar_constraint_lr g cxFlag thruFlag acc tpc =
    match tpc with 
    | TTyparCoercesToType(typ,m) -> acc_free_in_type_lr g cxFlag thruFlag acc typ 
    | TTyparMayResolveMemberConstraint (traitInfo,_) -> acc_free_in_trait_lr g cxFlag thruFlag acc traitInfo 
    | TTyparDefaultsToType(_,rty,_) -> acc_free_in_type_lr g cxFlag thruFlag acc rty 
    | TTyparSimpleChoice(tys,_) -> acc_free_in_types_lr g cxFlag thruFlag acc tys 
    | TTyparIsEnum(uty,m) -> acc_free_in_type_lr g cxFlag thruFlag acc uty
    | TTyparIsDelegate(aty,bty,m) -> acc_free_in_type_lr g cxFlag thruFlag (acc_free_in_type_lr g cxFlag thruFlag acc aty) bty  
    | TTyparSupportsNull _ | TTyparIsNotNullableValueType _ | TTyparIsReferenceType _ 
    | TTyparRequiresDefaultConstructor _ -> acc

and acc_free_in_trait_lr g cxFlag thruFlag acc (TTrait(typs,_,_,argtys,rty,_))  = 
    let acc = acc_free_in_types_lr g cxFlag thruFlag acc typs
    let acc = acc_free_in_types_lr g cxFlag thruFlag acc argtys
    let acc = Option.fold_left (acc_free_in_type_lr g cxFlag thruFlag) acc rty
    acc

and acc_free_tpref_lr g cxFlag thruFlag acc (tp:Typar) = 
    if ListSet.mem typar_ref_eq tp acc 
    then acc
    else 
        let acc = (ListSet.insert typar_ref_eq tp acc)
        if cxFlag then 
            acc_free_in_typar_constraints_lr g cxFlag thruFlag acc tp.Constraints
        else 
            acc

and acc_free_in_type_lr g cxFlag thruFlag acc ty  = 
    if verbose then  dprintf "--> acc_free_in_type_lr \n";
    match (if thruFlag then strip_tpeqns_and_tcabbrevs g ty else strip_tpeqns ty) with 
    | TType_tuple l -> acc_free_in_types_lr g cxFlag thruFlag acc l 
    | TType_app (_,tinst) -> acc_free_in_types_lr g cxFlag thruFlag acc tinst 
    | TType_ucase (_,tinst) -> acc_free_in_types_lr g cxFlag thruFlag acc tinst 
    | TType_fun (d,r) -> acc_free_in_type_lr g cxFlag thruFlag (acc_free_in_type_lr g cxFlag thruFlag acc d ) r
    | TType_var r -> acc_free_tpref_lr g cxFlag thruFlag acc r 
    | TType_forall (tps,r) -> union_free_typars_lr (bound_typars_lr g cxFlag thruFlag tps (acc_free_in_type_lr g cxFlag thruFlag empty_free_typars_lr r)) acc
    | TType_modul_bindings -> failwith "acc_free_in_type_lr: naked struct"
    | TType_measure unt -> List.foldBack (fun (tp,_) acc -> acc_free_tpref_lr g cxFlag thruFlag acc tp) (ListMeasureVarOccsWithNonZeroExponents unt) acc

and acc_free_in_types_lr g cxFlag thruFlag acc tys = 
    match tys with 
    | [] -> acc
    | h :: t -> acc_free_in_types_lr g cxFlag thruFlag (acc_free_in_type_lr g cxFlag thruFlag acc h) t
    
let free_in_type_lr g thruFlag ty = acc_free_in_type_lr g true thruFlag empty_free_typars_lr ty |> List.rev
let free_in_types_lr g thruFlag ty = acc_free_in_types_lr g true thruFlag empty_free_typars_lr ty |> List.rev
let free_in_types_lr_no_cxs g ty = acc_free_in_types_lr g false true empty_free_typars_lr ty |> List.rev

let var_of_bind (b:Binding) = b.Var
let rhs_of_bind (b:Binding) = b.Expr
let vars_of_Bindings (binds:Bindings) = binds |> FlatList.map (fun b -> b.Var)
let vars_of_binds (binds:Binding list) = binds |> List.map (fun (b:Binding) -> b.Var)

let bind_order (v1:Binding) (v2:Binding) = val_spec_order v1.Var v2.Var

//---------------------------------------------------------------------------
// Equivalence of types up to alpha-equivalence 
//---------------------------------------------------------------------------

type TypeEquivEnv = 
    { ae_typars: TyparMap<typ>;
      ae_tcrefs: TyconRefRemap}

let tyeq_env_empty = { ae_typars=tpmap_empty(); ae_tcrefs=empty_tcref_remap }

let bind_tyeq_env_types tps1 tys2 aenv =
    {aenv with ae_typars=List.fold_right2 tpmap_add tps1 tys2 aenv.ae_typars}

let bind_tyeq_env_typars tps1 tps2 aenv =
    bind_tyeq_env_types tps1 (List.map mk_typar_ty tps2) aenv

let bind_tyeq_env_tpinst tpinst aenv =
    let tps,tys = List.unzip tpinst
    bind_tyeq_env_types tps tys aenv

let mk_tyeq_env tps1 tps2 = bind_tyeq_env_typars tps1 tps2 tyeq_env_empty

let rec traits_aequiv_aux erasureFlag g aenv (TTrait(typs1,nm,mf1,argtys,rty,_)) (TTrait(typs2,nm2,mf2,argtys2,rty2,_)) =
   ListSet.equals (type_aequiv_aux erasureFlag g aenv) typs1 typs2 &&
   mf1 = mf2 && 
   return_types_aequiv_aux erasureFlag g aenv rty rty2 && 
   List.lengthsEqAndForall2 (type_aequiv_aux erasureFlag g aenv) argtys argtys2 &&
   nm = nm2

and return_types_aequiv_aux erasureFlag g aenv rty rty2 =
    match rty,rty2 with  
    | None,None -> true
    | Some t1,Some t2 -> type_aequiv_aux erasureFlag g aenv t1 t2
    | _ -> false

    
and typarConstraints_aequiv_aux erasureFlag g aenv tpc1 tpc2 =
    match tpc1,tpc2 with
    | TTyparCoercesToType(acty,_),
      TTyparCoercesToType(fcty,_) -> 
        type_aequiv_aux erasureFlag g aenv acty fcty

    | TTyparMayResolveMemberConstraint(trait1,_),
      TTyparMayResolveMemberConstraint(trait2,_) -> 
        traits_aequiv_aux erasureFlag g aenv trait1 trait2 

    | TTyparDefaultsToType(_,acty,_),
      TTyparDefaultsToType(_,fcty,_) -> 
        type_aequiv_aux erasureFlag g aenv acty fcty

    | TTyparIsEnum(uty1,_),TTyparIsEnum(uty2,_) -> 
        type_aequiv_aux erasureFlag g aenv uty1 uty2

    | TTyparIsDelegate(aty1,bty1,_),TTyparIsDelegate(aty2,bty2,_) -> 
        type_aequiv_aux erasureFlag g aenv aty1 aty2 && 
        type_aequiv_aux erasureFlag g aenv bty1 bty2 

    | TTyparSimpleChoice (tys1,_),TTyparSimpleChoice(tys2,_) -> 
        ListSet.equals (type_aequiv_aux erasureFlag g aenv) tys1 tys2

    | TTyparSupportsNull _               ,TTyparSupportsNull _ 
    | TTyparIsNotNullableValueType _    ,TTyparIsNotNullableValueType _
    | TTyparIsReferenceType _           ,TTyparIsReferenceType _ 
    | TTyparRequiresDefaultConstructor _, TTyparRequiresDefaultConstructor _ -> true
    | _ -> false

and typarConstraintSets_aequiv_aux erasureFlag g aenv (tp1:Typar) (tp2:Typar) = 
    tp1.StaticReq = tp2.StaticReq &&
    ListSet.equals (typarConstraints_aequiv_aux erasureFlag g aenv) tp1.Constraints tp2.Constraints

and typar_decls_aequiv_aux erasureFlag g aenv tps1 tps2 = 
    List.length tps1 = List.length tps2 &&
    let aenv = bind_tyeq_env_typars tps1 tps2 aenv
    List.for_all2 (typarConstraintSets_aequiv_aux erasureFlag g aenv) tps1 tps2

and tcref_aequiv g aenv tc1 tc2 = 
    tcref_eq g tc1 tc2 || 
    (tcref_map_mem tc1 aenv.ae_tcrefs && tcref_eq g (tcref_map_find tc1 aenv.ae_tcrefs) tc2)

and type_aequiv_aux erasureFlag g aenv ty1 ty2 = 
    if verbose then  dprintf "--> type_aequiv...\n";
    let ty1 = strip_tpeqns_and_tcabbrevs_wrt_erasure erasureFlag g ty1 
    let ty2 = strip_tpeqns_and_tcabbrevs_wrt_erasure erasureFlag g ty2
    match ty1, ty2 with
    | TType_forall(tps1,rty1), TType_forall(tps2,rty2) -> 
        typar_decls_aequiv_aux erasureFlag g aenv tps1 tps2 && type_aequiv_aux erasureFlag g (bind_tyeq_env_typars tps1 tps2 aenv) rty1 rty2
    | TType_var tp1, TType_var tp2 when typar_ref_eq tp1 tp2 -> 
        true
    | TType_var tp1, _ when tpmap_mem tp1 aenv.ae_typars -> 
        type_equiv_aux erasureFlag g (tpmap_find tp1 aenv.ae_typars) ty2
    | TType_app (tc1,b1)  ,TType_app (tc2,b2) -> 
        tcref_aequiv g aenv tc1 tc2 &&
        types_aequiv_aux erasureFlag g aenv b1 b2
    | TType_ucase (UCRef(tc1,n1),b1)  ,TType_ucase (UCRef(tc2,n2),b2) -> 
        n1=n2 &&
        tcref_aequiv g aenv tc1 tc2 &&
        types_aequiv_aux erasureFlag g aenv b1 b2
    | TType_tuple l1,TType_tuple l2 -> 
        types_aequiv_aux erasureFlag g aenv l1 l2
    | TType_fun (dtys1,rty1),TType_fun (dtys2,rty2) -> 
        type_aequiv_aux erasureFlag g aenv dtys1 dtys2 && type_aequiv_aux erasureFlag g aenv rty1 rty2
    | TType_measure m1, TType_measure m2 -> 
        match erasureFlag with EraseNone -> measure_aequiv g aenv m1 m2 | _ -> true 
    | _ -> false

and measure_aequiv g aenv un1 un2 =
    let vars1 = ListMeasureVarOccs un1
    let trans tp1 = if tpmap_mem tp1 aenv.ae_typars then dest_anypar_typ g (tpmap_find tp1 aenv.ae_typars) else tp1
    let remap_tcref tc = if tcref_map_mem tc aenv.ae_tcrefs then tcref_map_find tc aenv.ae_tcrefs else tc
    let vars1' = List.map trans vars1
    let vars2 = ListSet.subtract typar_ref_eq (ListMeasureVarOccs un2) vars1'
    let cons1 = ListMeasureConOccsAfterRemapping g remap_tcref un1
    let cons2 = ListMeasureConOccsAfterRemapping g remap_tcref un2 
 
    List.forall (fun v -> MeasureVarExponent v un1 = MeasureVarExponent (trans v) un2) vars1 &&
    List.forall (fun v -> MeasureVarExponent v un1 = MeasureVarExponent v un2) vars2 &&
    List.forall (fun c -> MeasureConExponentAfterRemapping g remap_tcref c un1 = MeasureConExponentAfterRemapping g remap_tcref c un2) (cons1@cons2)  

and types_aequiv_aux erasureFlag g aenv l1 l2 = List.lengthsEqAndForall2 (type_aequiv_aux erasureFlag g aenv) l1 l2
and type_equiv_aux erasureFlag g ty1 ty2 =  type_aequiv_aux erasureFlag g tyeq_env_empty ty1 ty2

let type_aequiv g aenv ty1 ty2 = type_aequiv_aux EraseNone g aenv ty1 ty2
let type_equiv g ty1 ty2 = type_equiv_aux EraseNone g ty1 ty2
let traits_aequiv g aenv t1 t2 = traits_aequiv_aux EraseNone g aenv t1 t2
let typarConstraints_aequiv g aenv c1 c2 = typarConstraints_aequiv_aux EraseNone g aenv c1 c2
let typar_decls_aequiv g aenv d1 d2 = typar_decls_aequiv_aux EraseNone g aenv d1 d2
let return_types_aequiv g aenv t1 t2 = return_types_aequiv_aux EraseNone g aenv t1 t2
let measure_equiv g m1 m2 = measure_aequiv g tyeq_env_empty m1 m2

//--------------------------------------------------------------------------
// Values representing member functions on F# types
//--------------------------------------------------------------------------

let GetNumObjArgsOfMember memberFlags = 
    if memberFlags.MemberIsInstance then 1 else 0

let GetNumObjArgsOfVal (v:Val) = 
    match v.MemberInfo with 
    | Some membInfo -> GetNumObjArgsOfMember membInfo.MemberFlags
    | None -> 0

let GetNumObjArgsOfValRef (vref:ValRef) =  GetNumObjArgsOfVal vref.Deref

// Pull apart the type for an F# value that represents an object model method. Do not strip off a 'unit' argument.
// Review: Should GetMemberTypeInFSharpForm have any other direct callers? 
let GetMemberTypeInFSharpForm g memberFlags arities ty m = 
    let tps,argInfos,rty,retInfo = GetTopValTypeInFSharpForm g arities ty m
    let numObjArgs = GetNumObjArgsOfMember memberFlags

    let argInfos = 
        if numObjArgs = 1 then 
            match argInfos with
            | [] -> 
                errorR(InternalError("value does not have a valid member type",m)); 
                argInfos
            | h::t -> t
        else argInfos
    tps,argInfos,rty,retInfo

// Check that an F# value represents an object model method. 
// It will also always have an arity (inferred from syntax). 
let check_member_val membInfo arity m =
    match membInfo, arity with 
    | None,_ -> error(InternalError("check_member_val - no membInfo" , m))
    | _,None -> error(InternalError("check_member_val - no arity", m))
    | Some membInfo,Some arity ->  (membInfo,arity)

let check_member_vref (vref:ValRef) =
    check_member_val vref.MemberInfo vref.TopValInfo vref.Range
     
let GetTopValTypeInCompiledForm g topValInfo typ m =
    let tps,paramArgInfos,rty,retInfo = GetTopValTypeInFSharpForm g topValInfo typ m
    // Eliminate lone single unit arguments
    let paramArgInfos = 
        match paramArgInfos, topValInfo.ArgInfos with 
        // static member and module value unit argument elimination
        | [[(argType,_)]] ,[[]] -> 
            //assert is_unit_typ g argType 
            [[]]
        // instance member unit argument elimination
        | [objInfo;[(argType,_)]] ,[[objArg];[]] -> 
            //assert is_unit_typ g argType 
            [objInfo; []]
        | _ -> 
            paramArgInfos
    let rty = (if is_unit_typ g rty then None else Some rty)
    (tps,paramArgInfos,rty,retInfo)
     
// Pull apart the type for an F# value that represents an object model method
// and see the "member" form for the type, i.e. 
// detect methods with no arguments by (effectively) looking for single argument type of 'unit'. 
// The analysis is driven of the inferred arity information for the value.
//
// This is used not only for the compiled form - it's also used for all type checking and object model
// logic such as determining if abstract methods have been implemented or not, and how
// many arguments the method takes etc.
let GetMemberTypeInMemberForm g memberFlags topValInfo typ m =
    let tps,paramArgInfos,rty,retInfo = GetMemberTypeInFSharpForm g memberFlags topValInfo typ m
    // Eliminate lone single unit arguments
    let paramArgInfos = 
        match paramArgInfos, topValInfo.ArgInfos with 
        // static member and module value unit argument elimination
        | [[(argType,_)]] ,[[]] -> 
            assert is_unit_typ g argType 
            [[]]
        // instance member unit argument elimination
        | [[(argType,_)]] ,[[objArg];[]] -> 
            assert is_unit_typ g argType 
            [[]]
        | _ -> 
            paramArgInfos
    let rty = (if is_unit_typ g rty then None else Some rty)
    (tps,paramArgInfos,rty,retInfo)

let GetTypeOfMemberInMemberForm g (vref:ValRef) =
    //assert (not vref.IsExtensionMember)
    let membInfo,topValInfo = check_member_vref vref
    GetMemberTypeInMemberForm g membInfo.MemberFlags topValInfo vref.Type vref.Range

let GetTypeOfMemberInFSharpForm g (vref:ValRef) =
    let membInfo,topValInfo = check_member_vref vref
    GetMemberTypeInFSharpForm g membInfo.MemberFlags topValInfo vref.Type vref.Range

let GetReturnTypeOMemberInMemberForm g (vref:ValRef) =
    let membInfo,topValInfo = check_member_vref vref
    let _,_,rty,_ = GetMemberTypeInMemberForm g membInfo.MemberFlags topValInfo vref.Type vref.Range
    rty
  
/// Match up the type variables on an member value with the type 
/// variables on the apparent enclosing type
let PartitionValTypars g (v:Val)  = 
     match v.TopValInfo with 
     | None -> error(InternalError("PartitionValTypars: not a top value", v.Range))
     | Some arities -> 
         let fullTypars,_ = dest_top_forall_type g arities v.Type 
         let parent = v.MemberApparentParent
         let parentTypars = parent.TyparsNoRange
         let nparentTypars = parentTypars.Length
         if nparentTypars <= fullTypars.Length then 
             let memberParentTypars,memberMethodTypars = List.chop nparentTypars fullTypars
             let memberToParentInst,tinst = mk_typar_to_typar_renaming memberParentTypars parentTypars
             Some(parentTypars,memberParentTypars,memberMethodTypars,memberToParentInst,tinst)
         else None

let PartitionValRefTypars g vref = PartitionValTypars g (deref_val vref) 

/// Get the arguments for an F# value that represents an object model method 
let ArgInfosOfMemberVal g (v:Val) = 
    let membInfo,topValInfo = check_member_val v.MemberInfo v.TopValInfo v.Range
    let _,arginfos,_,_ = GetMemberTypeInMemberForm g membInfo.MemberFlags topValInfo v.Type v.Range
    arginfos

let ArgInfosOfMember g vref = 
    ArgInfosOfMemberVal g (deref_val vref)

let GetFSharpViewOfReturnType g retTy =
    match retTy with 
    | None -> g.unit_ty
    | Some retTy ->  retTy


/// Get the property "type" (getter return type) for an F# value that represents a getter or setter
/// of an object model property.
let ReturnTypeOfPropertyVal g (v:Val) = 
    let membInfo,topValInfo = check_member_val v.MemberInfo v.TopValInfo v.Range
    match membInfo.MemberFlags.MemberKind with 
    | MemberKindPropertySet ->
        let _,arginfos,_,_ = GetMemberTypeInMemberForm g membInfo.MemberFlags topValInfo v.Type v.Range
        if not arginfos.IsEmpty && not arginfos.Head.IsEmpty then
            arginfos.Head |> List.last |> fst 
        else
            error(Error("this value does not have a valid property setter type", v.Range));
    | MemberKindPropertyGet ->
        let _,_,rty,_ = GetMemberTypeInMemberForm g membInfo.MemberFlags topValInfo v.Type v.Range
        GetFSharpViewOfReturnType g rty
    | _ -> error(InternalError("vtyp_of_prop_vref",v.Range))


/// Get the property arguments for an F# value that represents a getter or setter
/// of an object model property.
let ArgInfosOfPropertyVal g (v:Val) = 
    let membInfo,topValInfo = check_member_val v.MemberInfo v.TopValInfo v.Range
    match membInfo.MemberFlags.MemberKind with 
    | MemberKindPropertyGet ->
        ArgInfosOfMemberVal g v |> List.concat
    | MemberKindPropertySet ->
        let _,arginfos,_,_ = GetMemberTypeInMemberForm g membInfo.MemberFlags topValInfo v.Type v.Range
        if not arginfos.IsEmpty && not arginfos.Head.IsEmpty then
            arginfos.Head |> List.frontAndBack |> fst 
        else
            error(Error("this value does not have a valid property setter type", v.Range));
    | _ -> 
        error(InternalError("ArgInfosOfPropertyVal",v.Range))

//---------------------------------------------------------------------------
// Generalize type constructors to types
//---------------------------------------------------------------------------

let generalize_tcref_tinst (tc:TyconRef) =  generalize_typars tc.TyparsNoRange
let generalize_tcref tc = 
    let tinst = generalize_tcref_tinst tc
    tinst,TType_app(tc, tinst)

let isTTyparSupportsStaticMethod = function TTyparMayResolveMemberConstraint _ -> true | _ -> false
let isTTyparCoercesToType = function TTyparCoercesToType _ -> true | _ -> false

//--------------------------------------------------------------------------
// Print Signatures/Types - prelude
//-------------------------------------------------------------------------- 

let prefix_of_static_req s =
    match s with 
    | NoStaticReq -> "'"
    | HeadTypeStaticReq -> " ^"

let prefix_of_rigid (typar:Typar) =   
  if (typar.Rigidity <> TyparRigid) then "_" else ""

//---------------------------------------------------------------------------
// Prettify: PrettyTyparNames/PrettifyTypes - make typar names human friendly
//---------------------------------------------------------------------------

module PrettyTypes = begin

    let NewPrettyTypar (tp:Typar) nm = 
        NewTypar (tp.Kind, tp.Rigidity,Typar(ident(nm, tp.Range),tp.StaticReq,false),false,DynamicReq,[])

    let NewPrettyTypars renaming tps names = 
        let niceTypars = List.map2 NewPrettyTypar tps names
        let renaming = renaming @ mk_typar_inst tps (generalize_typars niceTypars)
        (tps,niceTypars) ||> List.iter2 (fun tp tpnice -> fixup_typar_constraints tpnice (inst_typar_constraints renaming tp.Constraints)) ;
        niceTypars, renaming

    // We choose names for type parameters from 'a'..'t'
    // We choose names for unit-of-measure from 'u'..'z'
    // If we run off the end of these ranges, we use 'aX' for positive integer X or 'uX' for positive integer X
    // Finally, we skip any names already in use
    let NeedsPrettyTyparName (tp:Typar) = tp.IsCompilerGenerated && (tp.Data.typar_id.idText = unassignedTyparName)
    let PrettyTyparNames pred alreadyInUse tps = 
        let rec choose (tps:Typar list) (typeIndex, measureIndex) acc = 
            match tps with
            | [] -> List.rev acc
            | tp::tps ->
            

                // Use a particular name, possibly after incrementing indexes
                let useThisName (nm, typeIndex, measureIndex) = 
                    choose tps (typeIndex, measureIndex) (nm::acc)

                // Give up, try again with incremented indexes
                let tryAgain (typeIndex, measureIndex) = 
                    choose (tp::tps) (typeIndex, measureIndex) acc

                let tryName (nm, typeIndex, measureIndex) f = 
                    if List.mem nm alreadyInUse then 
                        f()
                    else
                        useThisName (nm, typeIndex, measureIndex)

                if pred tp then 
                    if NeedsPrettyTyparName tp then 
                        let (typeIndex, measureIndex, baseName, letters, i) = 
                          match tp.Kind with 
                          | KindType -> (typeIndex+1,measureIndex,'a',20,typeIndex) 
                          | KindMeasure -> (typeIndex,measureIndex+1,'u',6,measureIndex)
                        let nm = 
                           if i < letters then String.make 1 (char(int baseName + i)) 
                           else String.make 1 baseName ^ string (i-letters+1)
                        tryName (nm, typeIndex, measureIndex)  (fun () -> 
                            tryAgain (typeIndex, measureIndex))

                    else
                        tryName (tp.Name, typeIndex, measureIndex) (fun () -> 
                            // Use the next index and append it to the natural name
                            let (typeIndex, measureIndex, nm) = 
                              match tp.Kind with 
                              | KindType -> (typeIndex+1,measureIndex,tp.Name+ string typeIndex) 
                              | KindMeasure -> (typeIndex,measureIndex+1,tp.Name+ string measureIndex)
                            tryName (nm,typeIndex, measureIndex) (fun () -> 
                                tryAgain (typeIndex, measureIndex)))
                else
                    useThisName (tp.Name,typeIndex, measureIndex)

                          
        choose tps (0,0) []

    let PrettifyTypes g foldTys mapTys tys = 
        let ftps = (foldTys (acc_free_in_type_lr g true false) empty_free_typars_lr tys)
        (* let ftps = (foldTys (fun x acc -> acc_free_in_type_lr false acc x) tys empty_free_typars_lr) in   *)
        let ftps = List.rev ftps
        (* ftps |> List.iter (fun tp -> dprintf "free typar: %d\n" (stamp_of_typar tp)); *)
        let rec computeKeep (keep:typars) change (tps:typars) = 
            match tps with 
            | [] -> List.rev keep, List.rev change 
            | tp :: rest -> 
                if not (NeedsPrettyTyparName tp) && (not (keep |> List.exists (fun tp2 -> tp.Name = tp2.Name)))  then
                    computeKeep (tp :: keep) change rest
                else 
                    computeKeep keep (tp :: change) rest
        let keep,change = computeKeep [] [] ftps
        
        (* change |> List.iter (fun tp -> dprintf "change typar: %s %s %d\n" tp.Name (tp.DisplayName) (stamp_of_typar tp));  *)
        (* keep |> List.iter (fun tp -> dprintf "keep typar: %s %s %d\n" tp.Name (tp.DisplayName) (stamp_of_typar tp));  *)
        let alreadyInUse = keep |> List.map (fun x -> x.Name)
        let names = PrettyTyparNames (fun x -> List.memq x change) alreadyInUse ftps

        let niceTypars, renaming = NewPrettyTypars [] ftps names 
        let prettyTypars = mapTys (InstType renaming) tys
        (* niceTypars |> List.iter (fun tp -> dprintf "nice typar: %d\n" (stamp_of_typar tp)); *) 
        let tpconstraints  = niceTypars |> List.collect (fun tpnice -> List.map (fun tpc -> tpnice,tpc) tpnice.Constraints)

        renaming,
        prettyTypars,
        tpconstraints

    let PrettifyTypes1   g x = PrettifyTypes g (fun f -> f) (fun f -> f) x
    let PrettifyTypes2   g x = PrettifyTypes g (fun f -> foldl'2 (f,f)) (fun f -> map'2 (f,f)) x
    let PrettifyTypesN   g x = PrettifyTypes g List.fold List.map   x
    let PrettifyTypesN1  g x = PrettifyTypes g (fun f -> foldl'2 (List.fold (foldl1'2  f), f)) (fun f -> map'2 (List.map (map1'2  f),f)) x
    let PrettifyTypesNN1 g x = PrettifyTypes g (fun f -> foldl'3 (List.fold f, List.fold (foldl1'2 f),f)) (fun f -> map'3 (List.map f, List.map (map1'2  f), f)) x
    let PrettifyTypesNM1 g x = PrettifyTypes g (fun f -> foldl'3 (List.fold f, List.fold (List.fold (foldl1'2 f)),f)) (fun f -> map'3 (List.map f, List.mapSquared (map1'2  f), f)) x

end


 
module SimplifyTypes = begin

    (* CAREFUL! This function does NOT walk constraints *)
    let rec FoldType f z typ =
        let typ = strip_tpeqns typ 
        let z = f z typ
        match typ with
        | TType_forall (tps,body) -> FoldType f z body
        | TType_app (_,tinst) -> List.fold (FoldType f) z tinst
        | TType_ucase (_,tinst) -> List.fold (FoldType f) z tinst
        | TType_tuple typs        -> List.fold (FoldType f) z typs
        | TType_fun (s,t)         -> FoldType f (FoldType f z s) t
        | TType_var tp            -> z
        | TType_modul_bindings            -> z
        | TType_measure unt          -> z

    let incM x m =
        if Zmap.mem x m then Zmap.add x (1 + Zmap.find x m) m
        else Zmap.add x 1 m

    let AccTyparCounts z typ =
        (* Walk type to determine typars and their counts (for pprinting decisions) *)
        FoldType (fun z typ -> match typ with | TType_var tp when tp.Rigidity = TyparRigid  -> incM tp z | _ -> z) z typ

    let emptyTyparCounts = Zmap.empty typar_spec_order

    (* print multiple fragments of the same type using consistent naming and formatting *)
    let AccTyparCountsMulti acc l = List.fold AccTyparCounts acc l

    type TypeSimplificationInfo =
        { singletons         : Typar Zset.set;
          inplaceConstraints :  Zmap.map<Typar,typ>;
          postfixConstraints : (Typar * TyparConstraint) list; }
          
    let typeSimplificationInfo0 = 
        { singletons         = Zset.empty typar_spec_order;
          inplaceConstraints = Zmap.empty typar_spec_order;
          postfixConstraints = [] }

    let CategorizeConstraints simplify m cxs =
        let singletons = if simplify then Zmap.chooseL (fun tp n -> if n=1 then Some tp else None) m else []
        let singletons = Zset.addList singletons (Zset.empty typar_spec_order)
        // Here, singletons are typars that occur once in the type.
        // However, they may also occur in a type constraint.
        // If they do, they are really multiple occurance - so we should remove them.
        let constraintTypars = (free_in_typar_constraints CollectTyparsNoCaching (List.map snd cxs)).FreeTypars
        let usedInTypeConstraint typar = Zset.mem typar constraintTypars
        let singletons = singletons |> Zset.filter (usedInTypeConstraint >> not) 
        (* Here, singletons should really be used once *)
        let inplace,postfix =
          cxs |> List.partition (fun (tp,tpc) -> 
            simplify &&
            isTTyparCoercesToType tpc && 
            Zset.mem tp singletons && 
            tp.Constraints.Length = 1)
        let inplace = inplace |> List.map (function (tp,TTyparCoercesToType(ty,m)) -> tp,ty | _ -> failwith "not isTTyparCoercesToType")
        
        { singletons         = singletons;
          inplaceConstraints = Zmap.of_list typar_spec_order inplace;
          postfixConstraints = postfix;
        }
    let CollectInfo simplify tys cxs = 
        CategorizeConstraints simplify (AccTyparCountsMulti emptyTyparCounts tys) cxs 
        
end

let rec IterType ((fStripped,fTypars,fTraitSolution) as f) typ =
    // We iterate the _solved_ constraints as well, to pick up any record of trait constraint solutions
    // This means we walk _all_ the constraints _everywhere_ in a type, including
    // those attached to _solved_ type variables. This is used by PostTypecheckSemanticChecks to detect uses of
    // values as solutions to trait constraints and determine if inference has caused the value to escape its scope.
    // The only record of these solutions is in the _solved_ constraints of types.
    // In an ideal world we would, instead, record the solutions to these constraints as "witness variables" in expressions,
    // rather than solely in types.
    match typ with 
    | TType_var tp  when tp.Solution.IsSome  -> 
        tp.Constraints |> List.iter (fun cx -> 
            match cx with 
            | TTyparMayResolveMemberConstraint((TTrait(typs,_,_,argtys,rty,soln)),m) -> 
                 Option.iter fTraitSolution !soln
            | _ -> ())
    | _ -> ()
    
    let typ = strip_tpeqns typ 
    fStripped typ;
    match typ with
    | TType_forall (tps,body) -> 
        IterType f body;           
        tps |> List.iter fTypars;
        tps |> List.iter (fun tp -> tp.Constraints |> List.iter (IterTypeConstraint f))

    | TType_measure unt          -> ()
    | TType_app (_,tinst) -> IterTypes f tinst
    | TType_ucase (_,tinst) -> IterTypes f tinst
    | TType_tuple typs        -> IterTypes f typs
    | TType_fun (s,t)         -> IterType f s; IterType f t
    | TType_var tp            -> fTypars tp
    | TType_modul_bindings            -> ()
and IterTypes f tys = List.iter (IterType f) tys
and IterTypeConstraint ((fStripped,fTypars,fTraitSolution) as f) x =
     match x with 
     | TTyparCoercesToType(ty,m) -> IterType f ty
     | TTyparMayResolveMemberConstraint(traitInfo,m) -> IterTraitInfo f traitInfo
     | TTyparDefaultsToType(priority,ty,m) -> IterType f ty
     | TTyparSimpleChoice(tys,m) -> IterTypes f tys
     | TTyparIsEnum(uty,m) -> IterType f uty
     | TTyparIsDelegate(aty,bty,m) -> IterType f aty; IterType f bty
     | TTyparSupportsNull _ | TTyparIsNotNullableValueType _ 
     | TTyparIsReferenceType _ | TTyparRequiresDefaultConstructor _ -> ()
and IterTraitInfo ((_,_,fTraitSolution) as f) (TTrait(typs,_,_,argtys,rty,soln))  = 
    IterTypes f typs; 
    IterTypes f argtys; 
    Option.iter (IterType f) rty;
    Option.iter fTraitSolution !soln

//--------------------------------------------------------------------------
// Print Signatures/Types
//-------------------------------------------------------------------------- 

type DisplayEnv = 
  { html: bool;
    htmlHideRedundantKeywords: bool;
    htmlAssemMap: string NameMap; (* where can the docs for f# assemblies be found? *)
    openTopPaths: (string list) list; 
    showObsoleteMembers: bool; 
    showTyparBinding: bool; 
    showImperativeTyparAnnotations: bool;
    suppressInlineKeyword: bool;
    showMemberContainers:bool;
    shortConstraints:bool;
    showAttributes:bool;
    showOverrides:bool;
    showConstraintTyparAnnotations: bool;
    abbreviateAdditionalConstraints: bool;
    showTyparDefaultConstraints : bool;
    g: TcGlobals;
    contextAccessibility: Accessibility;
    generatedValueLayout:(Val -> layout option);    
    }

  member x.Normalize() = 
      { x with 
           openTopPaths = 
              x.openTopPaths 
                |> List.sortWith (fun p1 p2 -> -(compare p1 p2)) 
      }

let empty_denv tcGlobals = 
  { html=false;
    htmlHideRedundantKeywords=false;
    htmlAssemMap=NameMap.empty;
    openTopPaths=[]; 
    showObsoleteMembers=true;
    showTyparBinding = false;
    showImperativeTyparAnnotations=false;
    suppressInlineKeyword=false;
    showMemberContainers=false;
    showAttributes=false;
    showOverrides=true;
    showConstraintTyparAnnotations=true;
    abbreviateAdditionalConstraints=false;
    showTyparDefaultConstraints=false;
    shortConstraints=false;
    g=tcGlobals;
    contextAccessibility = taccessPublic;
    generatedValueLayout = (fun v -> None);        
  }.Normalize()


let denv_add_open_path path denv = 
    { denv with openTopPaths= path :: denv.openTopPaths}.Normalize()

let denv_add_open_modref modref denv = 
    let path = demangled_path_of_cpath (full_cpath_of_modul (deref_modul modref))
    denv_add_open_path path denv 

let denv_scope_access access denv =     
    { denv with contextAccessibility = combineAccess denv.contextAccessibility access }

let full_name_of_nlpath (NLPath(ccu,p) : NonLocalPath) =  text_of_arr_path p 
let (+.+) s1 s2 = (if s1 = "" then s2 else s1^"."^s2)

let full_name_of_parent_of_item_ref ppF (|Ref_private|Ref_nonlocal|) tcref = 
    match tcref with 
    | Ref_private x -> 
         (match ppF x with 
          | None -> None
          | Some (PubPath([| |],nm)) -> None
          | Some (PubPath(p,nm)) -> Some(text_of_path (Array.to_list p)))
    | Ref_nonlocal nlr -> 
        match nlr.nlr_nlpath with 
        | (NLPath(ccu,[| |])) -> None 
        | _ -> Some (full_name_of_nlpath nlr.nlr_nlpath)
  

let full_display_text_of_item_ref nmF ppF (|Ref_private|Ref_nonlocal|) xref = 
    match full_name_of_parent_of_item_ref ppF (|Ref_private|Ref_nonlocal|) xref  with 
    | None -> nmF xref 
    | Some pathText -> pathText +.+ nmF xref
  
let full_display_text_of_parent_of_modref r = full_name_of_parent_of_item_ref pubpath_of_tycon (|ERef_private|ERef_nonlocal|) r

let full_display_text_of_modref r = full_display_text_of_item_ref demangled_name_of_modref pubpath_of_tycon (|ERef_private|ERef_nonlocal|) r
let full_display_text_of_vref   r = full_display_text_of_item_ref (fun (tc:ValRef)   -> tc.DisplayName) pubpath_of_val   (|VRef_private|VRef_nonlocal|) r
let full_display_text_of_tcref  r = full_display_text_of_item_ref (fun (tc:TyconRef) -> tc.DisplayNameWithUnderscoreTypars) pubpath_of_tycon (|ERef_private|ERef_nonlocal|) r
let full_display_text_of_ecref  r = full_display_text_of_item_ref (fun (tc:TyconRef) -> tc.DisplayNameWithUnderscoreTypars) pubpath_of_tycon (|ERef_private|ERef_nonlocal|) r

let full_display_text_of_ucref (ucref:UnionCaseRef) = full_display_text_of_tcref ucref.TyconRef +.+ ucref.CaseName
let full_display_text_of_rfref (rfref:RecdFieldRef) = full_display_text_of_tcref rfref.TyconRef +.+ rfref.FieldName

// This is a broken version of 'full_display_text_of_item_ref' that uses a mangled name in the Ref_nonlocal case.
// It is only used by the FSI generation code, which uses a really unstable and subtle technique to propagate information from
// the pretty printing code that lays out signautre files to the consuming code in the Language Service.
let full_mangled_name_of_item_ref_DO_NOT_USE nmF ppF (|Ref_private|Ref_nonlocal|) xref = 
    match xref with 
    | Ref_private x -> 
        match full_name_of_parent_of_item_ref ppF (|Ref_private|Ref_nonlocal|) xref  with 
        | None -> nmF xref 
        | Some pathText -> pathText +.+ nmF xref
    | Ref_nonlocal nlr -> 
        let nm = nlr.nlr_item //   <<----- BAD BAD BAD - this is a mangled name. The FSI generation code expects exactly this name to be used for non-local references
        match full_name_of_parent_of_item_ref ppF (|Ref_private|Ref_nonlocal|) xref  with 
        | None -> nm 
        | Some pathText -> pathText +.+ nm

let approx_full_mangled_name_of_modref r = full_mangled_name_of_item_ref_DO_NOT_USE demangled_name_of_modref pubpath_of_tycon (|ERef_private|ERef_nonlocal|) r
let approx_full_mangled_name_of_vref   r = full_mangled_name_of_item_ref_DO_NOT_USE (fun (tc:ValRef)   -> tc.MangledName) pubpath_of_val   (|VRef_private|VRef_nonlocal|) r
let approx_full_mangled_name_of_tcref  r = full_mangled_name_of_item_ref_DO_NOT_USE (fun (tc:TyconRef) -> tc.DisplayName) pubpath_of_tycon (|ERef_private|ERef_nonlocal|) r
let approx_full_mangled_name_of_ecref  r = full_mangled_name_of_item_ref_DO_NOT_USE (fun (tc:TyconRef) -> tc.DisplayName) pubpath_of_tycon (|ERef_private|ERef_nonlocal|) r
let approx_full_mangled_name_of_ucref (ucref:UnionCaseRef) = approx_full_mangled_name_of_tcref ucref.TyconRef +.+ ucref.CaseName
let approx_full_mangled_name_of_rfref (rfref:RecdFieldRef) = approx_full_mangled_name_of_tcref rfref.TyconRef +.+ rfref.FieldName

let full_mangled_path_to_tcref (tcref:TyconRef) = 
    match tcref with 
    | ERef_private ltc -> (match tcref.PublicPath with None -> [| |] | Some (PubPath(p,nm)) -> p)
    | ERef_nonlocal nlr -> path_of_nlpath (nlpath_of_nlref nlr)
  
let qualified_mangled_name_of_tcref tcref nm = 
    String.concat "-" (Array.to_list (full_mangled_path_to_tcref tcref) @ [ tcref.MangledName ^ "-" ^ nm ])

let rec firstEq p1 p2 = 
    match p1 with
    | [] -> true 
    | h1::t1 -> 
        match p2 with 
        | h2::t2 -> h1 = h2 && firstEq t1 t2
        | _ -> false 

let rec firstRem p1 p2 = 
   match p1 with [] -> p2 | h1::t1 -> firstRem t1 (List.tl p2)

let trim_path_by_denv denv path =
    let findOpenedNamespace opened_path = 
        if  firstEq opened_path path then 
          let t2 = firstRem opened_path path
          if t2 <> [] then Some(text_of_path t2^".")
          else Some("")
        else None
    match List.tryPick findOpenedNamespace denv.openTopPaths with
    | Some(s) -> s
    | None ->  if isNil path then "" else text_of_path path ^ "."


let adhoc_of_tycon (tycon:Tycon) = 
    NameMultiMap.range tycon.TypeContents.tcaug_adhoc 
    |> List.filter (fun v -> not v.IsCompilerGenerated)

let super_of_tycon g (tycon:Tycon) = 
    match tycon.TypeContents.tcaug_super with 
    | None -> g.obj_ty 
    | Some ty -> ty 

let implements_of_tycon (g:TcGlobals) (tycon:Tycon) = 
    tycon.TypeContents.tcaug_implements |> List.map (fun (x,_,_) -> x)

//----------------------------------------------------------------------------
// Detect attributes
//----------------------------------------------------------------------------

// AbsIL view of attributes (we read these from .NET binaries) 
let is_il_attrib (tref:ILTypeRef) attr = 
    (attr.customMethod.EnclosingType.TypeSpec.Name = tref.Name) &&
    (attr.customMethod.EnclosingType.TypeSpec.Enclosing = tref.Enclosing)

// REVIEW: consider supporting querying on Abstract IL custom attributes.
// These linear iterations cost us a fair bit when there are lots of attributes
// on imported types. However this is fairly rare and can also be solved by caching the
// results of attribute lookups in the TAST
let ILThingHasILAttrib tref attrs = List.exists (is_il_attrib tref) (dest_custom_attrs attrs)

let ILThingDecodeILAttrib g tref attrs = 
    attrs |> dest_custom_attrs |> List.tryPick(fun x -> if is_il_attrib tref x then Some(decode_il_attrib_data g.ilg x)  else None)

// This one is done by name to ensure the compiler doesn't take a dependency on dereferencing a type that only exists in .NET 3.5
let ILThingHasExtensionAttribute attrs = 
    attrs |> dest_custom_attrs |> List.exists (fun attr -> 
        attr.customMethod.EnclosingType.TypeSpec.Name = "System.Runtime.CompilerServices.ExtensionAttribute")
    
(* F# view of attributes (these get converted to AbsIL attributes in ilxgen) *)
let IsMatchingAttrib g (AttribInfo(_,tcref)) (Attrib(tcref2,_,_,_,_)) = tcref_eq g tcref  tcref2
let HasAttrib g tref attrs = List.exists (IsMatchingAttrib g tref) attrs
let fsthing_find_attrib g tref attrs = List.find (IsMatchingAttrib g tref) attrs
let TryFindAttrib g tref attrs = List.tryfind (IsMatchingAttrib g tref) attrs

let (|SpecificAttribNamedArg|_|) nm = function (AttribNamedArg(nm2,_,_,AttribExpr(_,v))) when nm = nm2 -> Some v | _ -> None

let (|AttribInt32Arg|_|) = function AttribExpr(_,TExpr_const (TConst_int32(n),m,_)) -> Some(n) | _ -> None
let (|AttribBoolArg|_|) = function AttribExpr(_,TExpr_const (TConst_bool(n),m,_)) -> Some(n) | _ -> None
let (|AttribStringArg|_|) = function AttribExpr(_,TExpr_const (TConst_string(n),m,_)) -> Some(n) | _ -> None

let TryFindBoolAttrib g nm attrs = 
    match TryFindAttrib g nm attrs with
    | Some(Attrib(_,_,[ ],_,_)) -> Some(true)
    | Some(Attrib(_,_,[ AttribBoolArg(b) ],_,_)) -> Some(b)
    | _ -> None

let TryFindUnitAttrib g nm attrs = 
    match TryFindAttrib g nm attrs with
    | Some(Attrib(_,_,[ ],_,_)) -> Some()
    | _ -> None

let TryFindInt32Attrib g nm attrs = 
    match TryFindAttrib g nm attrs with
    | Some(Attrib(_,_,[ AttribInt32Arg(b) ],_,_)) -> Some b
    | _ -> None
    
let ILThingHasAttrib (AttribInfo (atref,_)) attrs = 
    ILThingHasILAttrib atref attrs

let TyconRefTryBindAttrib g (AttribInfo (atref,_) as args) (tcref:TyconRef) f1 f2 = 
    if tcref.IsILTycon then 
        match ILThingDecodeILAttrib g atref tcref.ILTyconRawMetadata.tdCustomAttrs with 
        | Some attr -> f1 attr
        | _ -> None
    else 
        match TryFindAttrib g args tcref.Attribs with 
        | Some attr -> f2 attr
        | _ -> None
      
let TyconRefHasAttrib g args tcref = 
    TyconRefTryBindAttrib g args tcref (fun _ -> Some()) (fun _ -> Some()) |> isSome 

/// Detect if a type is definitely known to be non-serializable
let is_definitely_not_serializable g typ =
    if is_il_arr_typ g typ || is_any_array_typ g typ then
      true
    else
      match try_tcref_of_stripped_typ g typ with 
      | None -> false
      | Some tcref -> 
         if tcref.IsILTycon then 
             not tcref.ILTyconRawMetadata.tdSerializable
         else 
             (TryFindBoolAttrib g g.attrib_AutoSerializableAttribute tcref.Attribs = Some(false))

//-------------------------------------------------------------------------
// List and reference types...
//------------------------------------------------------------------------- 

let is_byref_typ g ty     = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref_eq g g.byref_tcr tcref

let dest_byref_typ g ty   = if is_byref_typ g ty then List.hd (tinst_of_stripped_typ g ty) else failwith "dest_byref_typ: not a byref type"

let is_refcell_ty g ty   = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref_eq g g.refcell_tcr tcref

let dest_refcell_ty g ty = if is_refcell_ty g ty then List.hd (tinst_of_stripped_typ g ty) else failwith "dest_refcell_ty: not a ref type"
let mk_refcell_ty  g ty = TType_app(g.refcell_tcr_nice,[ty])

let mk_lazy_ty g ty = TType_app(g.lazy_tcr_canon,[ty])

let mk_format_ty g aty bty cty dty ety = TType_app(g.format_tcr, [aty;bty;cty;dty; ety])

let mk_option_ty g ty = TType_app (g.option_tcr_nice, [ty])
let mk_list_ty g ty = TType_app (g.list_tcr_nice, [ty])
let is_arity1_ty g tcr ty = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> tcref_eq g tcr tcref

let is_option_ty g ty = is_arity1_ty g g.option_tcr ty

let try_dest_option_ty g ty = 
    match tinst_of_stripped_typ g ty with 
    | [ty1]  when is_option_ty g ty  -> Some(ty1)
    | _ -> None

let dest_option_ty g ty = 
    match try_dest_option_ty g ty with 
    | Some(ty) -> ty
    | None -> failwith "dest_option_ty: not an option type"

let mk_none_ucref g = mk_ucref g.option_tcr "None"
let mk_some_ucref g = mk_ucref g.option_tcr "Some"

let vref_is_dispatch_slot (vref:ValRef) = 
    match vref.MemberInfo with 
    | Some membInfo -> membInfo.MemberFlags.MemberIsDispatchSlot 
    | None -> false

let (|BitwiseOr|_|) g expr = 
    match expr with 
    | TExpr_app(TExpr_val(vref,_,_),_,_,[arg1;arg2],_) when g.vref_eq vref g.bitwise_or_vref  ->
        Some(arg1,arg2)
    // Special workaround, only used when compiling FSharp.Core.dll. Uses of 'a ||| b' occur before the '|||' bitwise or operator
    // is defined. These get through type checking because enums implicitly support the '|||' operator through
    // the automatic resolution of undefined operators (see tc.ml, Item_implicit_op). This then compiles as an 
    // application of a lambda to two arguments. We recognize this pattern here
    | TExpr_app(TExpr_lambda _,_,_,[arg1;arg2],_) when g.compilingFslib  -> 
        Some(arg1,arg2)
    | _ -> None

let is_typeof_vref g vref = 
    g.vref_eq vref g.typeof_vref 
    // There is an internal version of typeof defined in prim-types.fs that needs to be detected
    || (g.compilingFslib && vref.CompiledName = "typeof") 

let is_typedefof_vref g vref = 
    g.vref_eq vref g.typedefof_vref 
    // There is an internal version of typeof defined in prim-types.fs that needs to be detected
    || (g.compilingFslib && vref.CompiledName = "typedefof") 

//--------------------------------------------------------------------------
// Print Signatures/Types 
//-------------------------------------------------------------------------- 

module NicePrint = begin
    open Microsoft.FSharp.Compiler.Layout
    open Microsoft.FSharp.Compiler.PrettyNaming
    open PrettyTypes

    /// Paths are used in FSI generation and Abstract IL metadata printing
    ///
    /// paths in innermost-to-outermost order (e.g., `Foo.Bar.Baz` = ["Baz" ; "Bar" ; "Foo"])
    type Path = 
        | Path of string list 
        | NoPath 

        static member Empty = Path []

        member path.Add ident = 
            match path with
            | NoPath    -> NoPath
            | Path xs -> Path (ident :: xs)

    let fullySplitTypeRef (tref:ILTypeRef) = 
        (List.collect IL.split_namespace (tref.Enclosing @ [IL.ungenericize_tname tref.Name])) 

    let commentL l = wordL "(*" ++ l ++ wordL "*)"
    let comment str = str |> wordL |> commentL

    let layoutsL (ls : layout list) : layout =
        match ls with
        | []      -> emptyL
        | [x]     -> x
        | x :: xs -> List.fold ($$) x xs 

    /// Place the mangled identifier's name at the 'end' of the path; this will make the fully-qualified name later
    ///
    /// REVIEW: when FSI generation for assemblies containing quoted identifiers is
    /// implemented, a decision will have to be taken here whether to strip "``" from the 
    /// beginning / end of these identifiers; this depends on how the island parser is made to 
    /// get "funny" identifiers; the same for operators
    let annotateWithPath ident path mangledIdent = 
        let mkPair s = (s, "")
        let node = wordL ident
        match path with
        | NoPath      -> node
        | Path path -> Internal.Utilities.StructuredFormat.Layout.Attr ("goto:path", mangledIdent :: path |> List.rev |> List.map mkPair , node)

    // shortcut to annote elements with a path extended by the identifier.
    let (&==) ident path = annotateWithPath ident path ident 


    module IlPrint = begin

        open Microsoft.FSharp.Compiler.AbstractIL.IL
        
        let private adjustIlNameInternal (path : Path)(n : string) : string list =

            // REVIEW: don't hardwire this table like this
            let demangleFSharpBaseTypes n =
                match n with
                | "System.Void"    -> "unit"
                | "System.Object"  -> "obj"
                | "System.String"  -> "string"
                | "System.Single"  -> "float32"
                | "System.Double"  -> "float"
                | "System.Decimal"  -> "decimal"
                | "System.Char"    -> "char"
                | "System.Int16"   -> "int16"
                | "System.Int32"   -> "int" 
                | "System.Int64"   -> "int64" 
                | "System.UInt16"  -> "uint16" 
                | "System.UInt32"  -> "uint32" 
                | "System.UInt64"  -> "uint64" 
                | "System.Boolean" -> "bool"
                | _                -> n

            let dropCurrentNamespace n    = 
                match path with
                | NoPath    -> n
                | Path xs -> 
                    match n |> List.rev with
                    | (y :: _) as ys when ys = xs -> [ y ] // special case for (recursive) references to the type we're currently defining
                    | y :: ys when ys = xs        -> [ y ]
                    | _                           -> n
            n |> demangleFSharpBaseTypes |> SplitNamesForFsiGenerationPath |> ChopUnshowableInFsiGenerationPath |> dropCurrentNamespace |> List.map Lexhelp.Keywords.QuoteIdentifierIfNeeded

        /// fix up a name coming from IL metadata by:
        /// - translating well-known base types (e.g., System.Int32 -> int)
        /// - chopping off numeric (num-of-params) suffixes
        /// - hiding the current namespace (e.g., if we're in A.B.C.D and we're
        ///   showing A.B.C.D.E, we can abbreviate this to E)
        /// - quote "funny" names (keywords, otherwise invalid identifiers)
        let private adjustIlName (path : Path)(n : string) : string =
            n |> adjustIlNameInternal path |> JoinNamesForFsiGenerationPath

        /// this fixes up a name just like adjustIlName but also handles F#
        /// operators
        let private adjustIlMethodName (path : Path)(n : string) : string =
          let specialdemangleOperatorName s =
              if IsMangledOpName s
              then DemangleOperatorName s
              else s
          n |> adjustIlNameInternal path |> List.map specialdemangleOperatorName |> JoinNamesForFsiGenerationPath



        // our good Haskell friend
        let rec intersperse x lst =
            match lst with 
            | []      -> []
            | [y]     -> [y]
            | y :: ys -> y :: x :: intersperse x ys

        let private arrayShapeL (ILArrayShape sh : ILArrayShape) : layout = 
            leftL "[" $$ wordL (sh |> List.tl |> List.map (fun _ -> ",") |> String.concat "") $$ rightL "]" // drop off one "," so that a n-dimensional array has n - 1 ","'s

        let private  paramsList (ps : ILGenericParameterDefs) = 
            ps |> List.map (fun x -> "'" + x.Name |> wordL) 

        let private paramsL (ps : layout list) : layout = 
            match ps with
            | [] -> emptyL
            | _  -> 
                let body = ps |> intersperse (sepL ",") |> layoutsL
                sepL "<" $$ body $$ rightL ">"

        let private pruneParms (name : string)(parms : layout list) =
            let numParms = let rightMost = name |> SplitNamesForFsiGenerationPath |> List.rev |> List.hd
                           try rightMost |> int // can't find a way to see the number of generic parameters for *this* class (the GenericParams also include type variables for enclosing classes); this will have to do
                               with _ -> 0 // looks like it's non-generic
            parms |> List.rev |> List.take numParms |> List.rev
                             
        let rec ilILTypeL (denv : DisplayEnv)(path : Path)(parms : layout list)(typ : ILType) : layout =
            match typ with
            | Type_void               -> wordL "unit" // These are type-theoretically totally different type-theoretically `void` is Fin 0 and `unit` is Fin (S 0) ... but, this looks like as close as we can get.
            | Type_array (sh, t)      -> ilILTypeL denv path parms t $$ arrayShapeL sh
            | Type_value t
            | Type_boxed t            -> (adjustIlName path t.Name |> wordL) $$ (t.GenericArgs |> List.map (ilILTypeL denv path parms) |> paramsL)
            | Type_ptr t
            | Type_byref t            -> ilILTypeL denv path parms t
            | Type_fptr t             -> ilILCallingSignatureL denv path parms None t
            | Type_tyvar n            -> List.nth parms <| int n
            | Type_modified (_, _, t) -> ilILTypeL denv path parms t // "Just recurse through them to the contained ILType"--Don

        /// Layout a function's type signature. We need a special case for
        /// constructors (Their return types are reported as `void`, but this is
        /// incorrect; so if we're dealing with a constructor we require that the
        /// return type be passed along as the `cons` parameter.)
        and ilILCallingSignatureL (denv : DisplayEnv)(path : Path)(parms : layout list)(cons : string option)(signatur : ILCallingSignature) : layout =
            let args = signatur.ArgTypes |> List.map (ilILTypeL denv path parms) |> intersperse (wordL "*")
            let res  = 
                match cons with
                | Some className -> (adjustIlName path className |> wordL) $$ (pruneParms className parms |> paramsL) // special case for constructor return-type (viz., the class itself)
                | None           -> signatur.ReturnType |> ilILTypeL denv path parms
            match args with
            | []      -> wordL "unit" $$ wordL "->" $$ res
            | [x]     -> x $$ wordL "->" $$ res
            | x :: xs -> (List.fold ($$) x xs) $$ wordL "->" $$ res

        /// Layout a method's signature. In the case that we've a constructor, we
        /// pull off the class name from the `path`; naturally, it's the
        /// most-deeply-nested element.

        let private ilMethodDefL (denv : DisplayEnv)(path : Path)(parms : layout list)(className : string)(m : ILMethodDef) : layout =
            let myParms         = m.GenericParams |> paramsList
            let parms           = parms @ myParms
            let name            = adjustIlMethodName path m.Name
            let (nameL, isCons) = 
                match () with
                | _ when m.IsConstructor -> ("new" &== path,                                                              Some className) // we need the unadjusted name here to be able to grab the number of generic parameters
                | _ when m.IsStatic      -> (wordL "static" $$ wordL "member" $$ (name &== path) $$ (myParms |> paramsL), None)
                | _                      -> (wordL "member" $$ (name &== path) $$ (myParms |> paramsL),                   None)
            let signaturL       = callsig_of_mdef m |> ilILCallingSignatureL denv path parms isCons
            nameL $$ wordL ":" $$ signaturL

        let private ilFieldDefL (denv : DisplayEnv)(path : Path)(parms : layout list)(f : ILFieldDef) : layout =
            let staticL =  if f.IsStatic then wordL "static" else emptyL
            let name    = adjustIlName path f.Name
            let nameL   = name &== path
            let typL    = ilILTypeL denv path parms f.Type
            staticL $$ wordL "val" $$ nameL $$ wordL ":" $$ typL            
       
        let private ilPropertyDefL (denv : DisplayEnv)(path : Path)(parms : layout list)(p : ILPropertyDef) : layout =
            let staticL =  if p.propCallconv =  CC_static then wordL "static" else emptyL
            let name    = adjustIlName path p.Name
            let nameL   = name &== path
            
            let getterTypeL (getterRef:ILMethodRef) =
                match getterRef.ArgTypes with
                |   [] -> ilILTypeL denv path parms getterRef.ReturnType
                |   _ -> ilILCallingSignatureL denv path parms None getterRef.CallingSignature
                
            let setterTypeL (setterRef:ILMethodRef) =
                let argTypes = setterRef.ArgTypes
                if isNil argTypes then
                    emptyL // shouldn't happen
                else
                    let frontArgs, lastArg = List.frontAndBack argTypes
                    let argsL = frontArgs |> List.map (ilILTypeL denv path parms) |> intersperse (wordL "*") |> List.fold ($$) emptyL
                    argsL $$ wordL "->" $$ (ilILTypeL denv path parms lastArg)
            
            let typL = 
                match p.GetMethod, p.SetMethod with
                |   None, None -> ilILTypeL denv path parms p.Type // shouldn't happen
                |   Some getterRef, _ -> getterTypeL getterRef
                |   None, Some setterRef -> setterTypeL setterRef
                
            let specGetSetL =
                match p.GetMethod, p.SetMethod with
                |   None,None 
                |   Some _, None -> emptyL
                |   None, Some _ -> wordL "with" $$ wordL "deb  set"
                |   Some _, Some _ -> wordL "with" $$ wordL "get," $$ wordL "set"
            staticL $$ wordL "member" $$ nameL $$ wordL ":" $$ typL $$ specGetSetL

        let ilEnumFieldDefL (denv : DisplayEnv) (path : Path) (f : ILFieldDef) : layout =
            let res   = 
                match f.fdInit with
                | Some init -> 
                    match init with
                    | FieldInit_bool x   -> 
                        if x
                        then Some "true"
                        else Some "false"
                    | FieldInit_char c   -> Some ("'" + (char c).ToString () + "'")
                    | FieldInit_int16 x  -> Some ((x |> int32 |> string) + "s")
                    | FieldInit_int32 x  -> Some (x |> string)
                    | FieldInit_int64 x  -> Some ((x |> string) + "L")
                    | FieldInit_uint16 x -> Some ((x |> int32 |> string) + "us")
                    | FieldInit_uint32 x -> Some ((x |> int64 |> string) + "u")
                    | FieldInit_uint64 x -> Some ((x |> int64 |> string) + "UL")
                    | FieldInit_single d -> 
                        let s = d.ToString ("g12", System.Globalization.CultureInfo.InvariantCulture)
                        let s = 
                            if String.for_all (fun c -> System.Char.IsDigit c || c = '-')  s 
                            then s + ".0" 
                            else s
                        Some (s + "f")
                    | FieldInit_double d -> 
                         let s = d.ToString ("g12", System.Globalization.CultureInfo.InvariantCulture)
                         if String.for_all (fun c -> System.Char.IsDigit c || c = '-')  s 
                         then Some (s + ".0")
                         else Some s
                    | _   -> None
                | None      -> None
            let initL = match res with
                        | None   -> wordL "=" $$ (comment "value unavailable")
                        | Some s -> wordL "=" $$ wordL s

            let name  = adjustIlName path f.Name
            let nameL = name &== path
            wordL "|" $$ nameL $$ initL

        // filtering methods for hiding things we oughtn't show
        let private isStaticProperty    (p : ILPropertyDef)      = match p.GetMethod,p.SetMethod with
                                                                   | Some getter,_    -> getter.CallingSignature.CallingConv.IsStatic
                                                                   | None,Some setter -> setter.CallingSignature.CallingConv.IsStatic
                                                                   | None,None        -> true
        let private isPublicMethod      (m : ILMethodDef) : bool = m.Access = MemAccess_public
        let private isPublicCtor        (m : ILMethodDef) : bool = m.Access = MemAccess_public && m.IsConstructor
        let private isNotSpecialName    (m : ILMethodDef) : bool = not m.mdSpecialName
        let private isPublicField       (f : ILFieldDef)  : bool = f.Access = MemAccess_public
        let private isPublicClass       (c : ILTypeDef)   : bool =
            match c.Access with
            | TypeAccess_public
            | TypeAccess_nested MemAccess_public -> true
            | _                                  -> false
        let private isShowEnumField (f : ILFieldDef) : bool = f.Name <> "value__" // this appears to be the hard-coded underlying storage field
        let private isShowBase      (n : layout)   : bool = 
            let noShow = [ "System.Object" ; "System.ValueType" ; "obj" ] // hide certain 'obvious' base classes
            noShow |> List.map wordL |> List.exists ((=) n) |> not

        let rec ilTypeDefL (denv : DisplayEnv)(path : Path)(t : ILTypeDef) : layout =
            let name        = t.Name |> SplitNamesForFsiGenerationPath |> List.rev |> List.hd // dump the qualifiers
            let path'       = path // assumption: path's already been augmented when handling the LHS
            let parms       = t.GenericParams |> paramsList

            let baseNameL b = 
                match b with
                | Some b -> let baseName = ilILTypeL denv path parms b
                            if isShowBase baseName
                               then [ wordL "inherit" $$ baseName ]
                               else []
                | None   -> []

            let fieldsL parms = dest_fdefs >> List.filter isPublicField   >> List.map (ilFieldDefL denv path' parms)
            let enumsL        = dest_fdefs >> List.filter isShowEnumField >> List.map (ilEnumFieldDefL denv path')
            let typesL        = dest_tdefs >> List.filter isPublicClass   >> List.map (ilNestedClassDefL denv path')

            let renderL pre body post = 
                match pre with
                | Some pre -> 
                    match body with
                    | [] -> emptyL // empty type
                    | _  -> (pre @@-- aboveListL body) @@ post
                | None     -> aboveListL body

            let classL (t : ILTypeDef) typeWord = 
                let pre    = Some (wordL typeWord)
                let baseT  = baseNameL t.Extends
                let memberBlockLs (fieldDefs:ILFieldDefs,methodDefs:ILMethodDefs,propertyDefs:PropertyDefs) =
                    let ctors  =
                        methodDefs |> dest_mdefs |> 
                            List.filter isPublicCtor (*|> List.filter isNotSpecialName*) |> 
                            List.map (ilMethodDefL denv path' parms t.Name)
                    let fields = fieldsL parms fieldDefs
                    let nProps = propertyDefs |> dest_pdefs |> List.map (fun pd -> pd.Name,ilPropertyDefL denv path' parms pd)
                    let nMeths = 
                        methodDefs |> dest_mdefs |> 
                            List.filter isPublicMethod |> List.filter isNotSpecialName |> 
                            List.map (fun md -> md.Name,ilMethodDefL denv path' parms t.Name md)
                    let members = (nProps @ nMeths) |> List.sortBy fst |> List.map snd (* (properties and members) are sorted by name *)
                    ctors @ fields @ members
                let bodyStatic   = memberBlockLs (t.Fields     |> dest_fdefs |> List.filter (fun fd -> fd.IsStatic)                |> mk_fdefs,
                                                  t.Methods    |> dest_mdefs |> List.filter (fun md -> md.IsStatic)                |> mk_mdefs,
                                                  t.Properties |> dest_pdefs |> List.filter (fun pd -> isStaticProperty pd)        |> mk_properties)
                let bodyInstance = memberBlockLs (t.Fields     |> dest_fdefs |> List.filter (fun fd -> fd.IsStatic         |> not) |> mk_fdefs,
                                                  t.Methods    |> dest_mdefs |> List.filter (fun md -> md.IsStatic         |> not) |> mk_mdefs,
                                                  t.Properties |> dest_pdefs |> List.filter (fun pd -> isStaticProperty pd |> not) |> mk_properties)
                let body = bodyInstance @ bodyStatic (* instance "member" before "static member" *)
                let types  = typesL t.NestedTypes
                let post   = wordL "end"
                renderL pre (baseT @ body @ types) post

            let delegateL (t : ILTypeDef) = 
                let rhs = 
                    match t.Methods |> dest_mdefs |> List.filter (fun m -> m.Name = "Invoke") with // the delegate delegates to the type of `Invoke`
                    | m :: _ -> ilILCallingSignatureL denv path parms None (callsig_of_mdef m)
                    | _      -> comment "`Invoke` method could not be found"
                wordL "delegate" $$ wordL "of" $$ rhs

            match t.tdKind with
            | TypeDef_class     -> classL t "class"
            | TypeDef_valuetype -> classL t "struct"
            | TypeDef_interface -> classL t "interface"
            | TypeDef_enum      -> renderL None (enumsL t.Fields) emptyL
            | TypeDef_delegate  -> delegateL t
            | TypeDef_other _   -> comment "cannot show type"
          
        and ilNestedClassDefL (denv : DisplayEnv)(path : Path)(t : ILTypeDef) : layout =
            let name     = adjustIlName path t.Name
            let nameL    = name &== path
            let parms    = t.GenericParams |> paramsList
            let paramsL  = pruneParms t.Name parms |> paramsL
            let path'    = path.Add name
            let pre      = wordL "type" $$ nameL $$ paramsL
            let body     = ilTypeDefL denv path' t
            if body = emptyL
               then pre
               else (pre $$ wordL "=") @@-- body

    end

    (* Note: We need nice printing of constants in order to print literals and attributes *)
    let constL c =
        let str = 
            match c with
            | TConst_bool x        -> if x then "true" else "false"
            | TConst_sbyte x        -> (x               |> string)^"y"
            | TConst_byte x       -> (x               |> string)^"uy"
            | TConst_int16 x       -> (x               |> string)^"s"
            | TConst_uint16 x      -> (x               |> string)^"us"
            | TConst_int32 x       -> (x               |> string)
            | TConst_uint32 x      -> (x               |> string)^"u"
            | TConst_int64 x       -> (x               |> string)^"L"
            | TConst_uint64 x      -> (x               |> string)^"UL"
            | TConst_nativeint x   -> (x               |> string)^"n"
            | TConst_unativeint x  -> (x               |> string)^"un"
            | TConst_float32 d      -> 
                (let s = d.ToString("g12",System.Globalization.CultureInfo.InvariantCulture)
                 if String.for_all (fun c -> System.Char.IsDigit(c) || c = '-')  s 
                 then s + ".0" 
                 else s) + "f"
            | TConst_float d      -> 
                let s = d.ToString("g12",System.Globalization.CultureInfo.InvariantCulture)
                if String.for_all (fun c -> System.Char.IsDigit(c) || c = '-')  s 
                then s + ".0" 
                else s
            | TConst_char c        -> "'" ^ c.ToString() ^ "'" 
            | TConst_string bs     -> "\"" ^ bs ^ "\"" 
            | TConst_unit          -> "()" 
            | TConst_decimal bs    -> string bs ^ "M" 
            | TConst_zero       -> "default"
        wordL str

    let bracketIfL x lyt = if x then bracketL lyt else lyt
    let hlinkL (url:string) l = linkL url l
    let squareAngleL x = leftL "[<" $$ x $$ rightL ">]"
    let angleL denv x = 
        if denv.html then 
            sepL "&lt;" $$ x $$ rightL "&gt;"  
        else
            sepL "<" $$ x $$ rightL ">"  
    let braceL x = leftL "{" $$ x $$ rightL "}"  
    let boolL = function true -> wordL "true" | false -> wordL "false"

    let accessibilityL (denv:DisplayEnv) accessibility itemL =   
        let isInternalCompPath x = 
            match x with 
            | CompPath(ScopeRef_local,[]) -> true 
            | _ -> false
        let (|Public|Internal|Private|) (TAccess p) = 
            match p with 
            | [] -> Public 
            | _ when List.forall isInternalCompPath p  -> Internal 
            | _ -> Private
        match denv.contextAccessibility,accessibility with
        | Public,Internal  -> wordL "internal" ++ itemL    // print modifier, since more specific than context
        | Public,Private   -> wordL "private" ++ itemL     // print modifier, since more specific than context
        | Internal,Private -> wordL "private" ++ itemL     // print modifier, since more specific than context
        | _ -> itemL

    let trefL denv tref =
        let path = fullySplitTypeRef tref
        let p2,n = List.frontAndBack path
        if denv.html then
            hlinkL (text_of_path path ^ ".html") (wordL n)
        else 
            leftL (trim_path_by_denv denv p2) $$ wordL n
        
    /// Layout a reference to a type or value, perhaps emitting a HTML hyperlink *)
    let tcrefL isExn denv (tcref:TyconRef) = 
        let nm = tcref.MangledName
        let isExn = isExn && tcref.IsExceptionDecl
        let demangled = DemangleGenericTypeName nm
        let demangled = if isExn then DemangleExceptionName demangled else demangled
        let tyconTextL = wordL demangled
        let path = demangled_path_of_cpath tcref.CompilationPath
        let arity = tcref.TyparsNoRange.Length
        let arity_suffix = if arity=0 then "" else "-" ^ string_of_int arity
        match tcref with 
        | ERef_private _ -> 
            if denv.html then 
                let nm = (text_of_path (path@["type_" ^ String.underscoreLowercase demangled ^ arity_suffix])) in    (* text must tie up with fsc.ml *)
                hlinkL (sprintf "%s.html" nm)  tyconTextL
            else
                let pathText = trim_path_by_denv denv path
                (if pathText = "" then tyconTextL else leftL pathText $$ tyconTextL)
        | ERef_nonlocal nlref -> 
            if denv.html then 
                let href = 
                    let ccu = ccu_of_nlref nlref
                    match ccu.AssemblyName with 
                    | "mscorlib" | "System" | "System.Windows.Forms" 
                    | "System.Xml" | "System.Drawing" | "System.Data" -> 
                        (* cross link to the MSDN 2.0 documentation.  Generic types don't seem to have stable names :-( *)
                        if demangled = nm then 
                          Some (sprintf "http://msdn2.microsoft.com/en-us/library/%s.aspx" (text_of_path (path@[nm])))
                        else None
                    | _ -> 
                        if ccu.IsFSharp then 
                            let nm = text_of_path (path@["type_" ^ String.underscoreLowercase demangled ^ arity_suffix]) in     (* text must tie up with fsc.ml *)
                            match denv.htmlAssemMap.TryFind(ccu.AssemblyName) with 
                            | Some root -> Some (sprintf "%s/%s.html" root nm)  
                            (* otherwise assume it is installed parallel to this tree *)
                            | None -> Some (sprintf "../%s/%s.html" ccu.AssemblyName nm)  
                        else
                            None
                           
                match href with 
                | Some href -> hlinkL href tyconTextL
                | None      -> tyconTextL
            else
              let pathText = trim_path_by_denv denv path
              (if pathText = "" then tyconTextL else leftL pathText $$ tyconTextL)

    /// Layout the flags of a member *)
    let memFlagsL hide memFlags = 
        let stat = if hide || memFlags.MemberIsInstance || (memFlags.MemberKind = MemberKindConstructor) then emptyL else wordL "static"
        let stat = if not memFlags.MemberIsDispatchSlot && memFlags.MemberIsVirtual then stat ++ wordL "virtual" 
                   elif not hide && memFlags.MemberIsDispatchSlot then stat ++ wordL "abstract" 
                   elif memFlags.MemberIsOverrideOrExplicitImpl then stat ++ wordL "override" 
                   else stat
        let stat = 
        
            if memFlags.MemberIsOverrideOrExplicitImpl then stat 
            else  
              match memFlags.MemberKind with 
              | MemberKindClassConstructor  
              | MemberKindConstructor 
              | MemberKindPropertyGetSet -> stat
              | MemberKindMember 
              | MemberKindPropertyGet 
              | MemberKindPropertySet -> stat ++ wordL "member"

        (* let stat = if memFlags.MemberIsFinal then stat ++ wordL "final" else stat in  *)
        stat

    /// Layout a single attibute arg, following the cases of 'gen_attr_arg' in ilxgen.ml *)
    /// This is the subset of expressions we display in the NicePrint pretty printer *)
    /// See also dataExprL - there is overlap between these that should be removed *)
    let rec attribArgL denv arg = 
        match arg with 
        | TExpr_const(c,_,ty) -> 
            if is_enum_typ denv.g ty then 
                wordL "enum" $$ angleL denv (typeL denv ty) $$ bracketL (constL c)
            else
                constL c

        | TExpr_op(TOp_array,[elemTy],args,m) ->
            leftL "[|" $$ semiListL (List.map (attribArgL denv) args) $$ rightL "|]"

        (* Detect 'typeof<ty>' calls *)
        | TExpr_app(TExpr_val(vref,_,_),_,[ty],[],m) when is_typeof_vref denv.g vref  ->
            leftL "typeof<" $$ typeL denv ty $$ rightL ">"

        (* Detect 'typedefof<ty>' calls *)
        | TExpr_app(TExpr_val(vref,_,_),_,[ty],[],m) when is_typedefof_vref denv.g vref  ->
            leftL "typedefof<" $$ typeL denv ty $$ rightL ">"

        | TExpr_op(TOp_coerce,[tgTy;_],[arg2],_) ->
            leftL "(" $$ attribArgL denv arg2 $$ wordL ":>" $$ typeL denv tgTy $$ rightL ")"

        | BitwiseOr denv.g (arg1,arg2) ->
            attribArgL denv arg1 $$ wordL "|||" $$ attribArgL denv arg2

        (* Detect explicit enum values *)
        | TExpr_app(TExpr_val(vref,_,_),_,_,[arg1],_) when denv.g.vref_eq vref denv.g.enum_vref  ->
            wordL "enum" ++ bracketL (attribArgL denv arg1)


        | _ -> wordL "(* unsupported attribute argument *)"

    /// Layout arguments of an attribute 'arg1, ..., argN' 
    and attribArgsL denv args = 
        sepListL (rightL ",") (List.map (fun (AttribExpr(e1,_)) -> attribArgL denv e1) args)

    /// Layout an attribute 'Type(arg1, ..., argN)' 
    //
    // REVIEW: we are ignoring "props" here
    and attribL denv (Attrib(tcref,k,args,props,m)) = 
        let argsL = bracketL (attribArgsL denv args)
        match k with 
        | (ILAttrib(mref)) -> 
            let trimmedName = 
                let name =  mref.EnclosingTypeRef.Name
                match String.tryDropSuffix name "Attribute" with 
                | Some shortName -> shortName
                | None -> name
            let tref = mref.EnclosingTypeRef
            let tref = ILTypeRef.Create(scope= tref.Scope, enclosing=tref.Enclosing, name=trimmedName)
            trefL denv tref ++ argsL

        | (FSAttrib(vref)) -> 
            (* REVIEW: this is not trimming "Attribute" *)
            let rty = GetReturnTypeOMemberInMemberForm denv.g vref
            let rty = GetFSharpViewOfReturnType denv.g rty
            let tcref = tcref_of_stripped_typ denv.g rty
            tcrefL false denv tcref ++ argsL


    /// Layout '[<attribs>]' above another block 
    and attribsL denv kind attrs restL = 
        
        if denv.showAttributes then
            (* Don't display DllImport attributes in generated signatures and/or html *)
            let attrs = if denv.html then attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_OverloadIDAttribute >> not) else attrs
            let attrs = if denv.html then attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_ClassAttribute >> not) else attrs
            let attrs = if denv.html then attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_StructAttribute >> not) else attrs
            let attrs = if denv.html then attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_InterfaceAttribute >> not) else attrs
            let attrs = attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_DllImportAttribute >> not)
            let attrs = attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_ContextStaticAttribute >> not)
            let attrs = attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_ThreadStaticAttribute >> not)
            let attrs = attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_EntryPointAttribute >> not)
            let attrs = attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_MarshalAsAttribute >> not)
            let attrs = attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_ReflectedDefinitionAttribute >> not)
            let attrs = attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_StructLayoutAttribute >> not)
            let attrs = attrs |> List.filter (IsMatchingAttrib denv.g denv.g.attrib_AutoSerializableAttribute >> not)
            match attrs with
            | [] -> restL 
            | _  -> squareAngleL (sepListL (rightL ";") (List.map (attribL denv) attrs)) @@ 
                    restL
        else 
        match kind with 
        | KindType -> restL
        | KindMeasure -> squareAngleL (wordL "Measure") @@ restL

    and typarAttribsL denv kind attrs restL =         
            match attrs, kind with
            | [], KindType -> restL 
            | _, _  -> squareAngleL (sepListL (rightL ";") ((match kind with KindType -> [] | KindMeasure -> [wordL "Measure"]) @ List.map (attribL denv) attrs)) $$ restL

    (* NOTE: The primed' functions take an "env" - determines inplace printing of typar and constraints *)
    (* NOTE: "denv" is the DisplayEnv - "env" is the TypeSimplificationInfo *)      

    /// Layout type parameters, taking TypeSimplificationInfo into account  *
    and typarDeclsL' denv env nmL prefix (typars:typars) =
        let tpcs = typars |> List.collect (fun tp -> List.map (fun tpc -> tp,tpc) tp.Constraints) 
        match typars,tpcs with 
        | [],[]  -> 
            nmL

        | [h],[] when not prefix -> 
            typarL' denv env h --- nmL

        | _ -> 
            let tpcsL = constraintsL denv env tpcs
            let coreL = sepListL (sepL ",") (List.map (typarL' denv env) typars)
            (if prefix or nonNil(tpcs) then nmL $$ angleL denv (coreL --- tpcsL) else bracketL coreL --- nmL)

    /// Layout a single type parameter declaration, taking TypeSimplificationInfo into account  *)
    /// There are several printing-cases for a typar:
    ///
    ///  'a              - is multiple  occurance.
    ///  _               - singleton occurance, an underscore prefered over 'b. (OCAML accepts but does not print)
    ///  #Type           - inplace coercion constraint and singleton.
    ///  ('a :> Type)    - inplace coercion constraint not singleton.
    ///  ('a.opM : S->T) - inplace operator constraint.
    ///
    and typarL' denv env (typar:Typar) =
        let varL =
          wordL (sprintf "%s%s%s"
                   (if denv.showConstraintTyparAnnotations then prefix_of_static_req typar.StaticReq else "'")
                   (if denv.showImperativeTyparAnnotations then prefix_of_rigid typar else "")
                   typar.DisplayName)
        let varL = typarAttribsL denv typar.Kind typar.Attribs varL

        match Zmap.tryfind typar env.SimplifyTypes.inplaceConstraints with
        | Some (typarConstrTyp) ->
            if Zset.mem typar env.SimplifyTypes.singletons then
                leftL "#" $$ typarSubtypeConstraintL denv env typarConstrTyp
            else
                (varL $$ sepL ":>" $$ typarSubtypeConstraintL denv env typarConstrTyp) |> bracketL

        | _ -> varL

      
    /// Layout type parameter constraints, taking TypeSimplificationInfo into account  *)
    and constraintsL denv env cxs = 
        
        
        // Internally member constraints get attached to each type variable in their support. 
        // This means we get too many constraints being printed. 
        // So we normalize the constraints to eliminate duplicate member constraints 
        let cxs = 
            cxs  
            |> ListSet.setify (fun (_,cx1) (_,cx2) ->
                     match cx1,cx2 with 
                     | TTyparMayResolveMemberConstraint(traitInfo1,_),
                       TTyparMayResolveMemberConstraint(traitInfo2,_) -> traits_aequiv denv.g tyeq_env_empty traitInfo1 traitInfo2
                     | _ -> false)
                     
        let cxsL = List.collect (constraintL' denv env) cxs
        match cxsL with 
        | [] -> emptyL 
        | _ -> 
            if denv.abbreviateAdditionalConstraints then 
                wordL "when <constraints>"
            elif denv.shortConstraints then 
                leftL "(" $$ wordL "requires" $$ sepListL (wordL "and") cxsL $$ rightL ")"
            else
                wordL "when" $$ sepListL (wordL "and") cxsL

    /// Layout constraints, taking TypeSimplificationInfo into account  *)
    and constraintL' denv env (tp,tpc) =
        match tpc with 
        | TTyparCoercesToType(tpct,m) -> 
            [typarL' denv env tp $$ wordL ":>" --- typarSubtypeConstraintL denv env tpct]
        | TTyparMayResolveMemberConstraint(traitInfo,_) ->
            [traitL denv env traitInfo]
        | TTyparDefaultsToType(_,ty,m) ->
           if denv.showTyparDefaultConstraints then [wordL "default" $$ typarL' denv env tp $$ wordL " :" $$ typeL' denv env ty]
           else []
        | TTyparIsEnum(ty,m) ->
            if denv.shortConstraints then 
               [wordL "enum"]
            else
               [typarL' denv env tp $$ wordL ":" $$ tyappL denv env (wordL "enum") 2 true [ty]]
        | TTyparIsDelegate(aty,bty,m) ->
            if denv.shortConstraints then 
               [wordL "delegate"]
            else
               [typarL' denv env tp $$ wordL ":" $$ tyappL denv env (wordL "delegate") 2 true [aty;bty]]
        | TTyparSupportsNull(m) ->
           [typarL' denv env tp $$ wordL ":" $$ wordL "null" ]
        | TTyparIsNotNullableValueType(m) ->
            if denv.shortConstraints then 
               [wordL "value type"]
            else
               [typarL' denv env tp $$ wordL ":" $$ wordL "struct" ]
        | TTyparIsReferenceType(m) ->
            if denv.shortConstraints then 
               [wordL "reference type"]
            else
               [typarL' denv env tp $$ wordL ":" $$ wordL "not struct" ]
        | TTyparSimpleChoice(tys,m) ->
           [typarL' denv env tp $$ wordL ":" $$ bracketL (sepListL (sepL "|") (List.map (typeL' denv env) tys)) ]
        | TTyparRequiresDefaultConstructor(m) -> 
            if denv.shortConstraints then 
               [wordL "default constructor"]
            else
               [typarL' denv env tp $$ wordL ":" $$ bracketL (wordL "new : unit -> " $$ (typarL' denv env tp))]

    /// Layout a subtype constraint *)
    and typarSubtypeConstraintL denv env ty = typeL' denv env ty

    and traitL denv env (TTrait(tys,nm,memFlags,argtys,rty,_)) =
        let nm = DemangleOperatorName nm
        if denv.shortConstraints then 
            wordL ("member "^nm)
        else
            let rty = GetFSharpViewOfReturnType denv.g rty
            let stat = memFlagsL denv.htmlHideRedundantKeywords memFlags
            let tys = ListSet.setify (type_equiv denv.g) tys
            let tysL = 
                match tys with 
                | [ty] -> typeL' denv env ty 
                | tys -> bracketL (typesWithPrecL denv env 2 (wordL "or") tys)
            tysL $$ wordL ":"  ---  
                bracketL (stat ++ wordL nm $$ wordL ":" ---
                        ((typesWithPrecL denv env 2 (wordL "*") argtys --- wordL "->") --- (typeL' denv env rty)))


    /// Layout a unit expression *)
    and auxMeasureL' denv env prec unt =
        let sortVars (vs:(Typar * int) list) = vs |> List.sortBy (fun (v,_) -> v.DisplayName) 
        let sortCons (cs:(TyconRef * int) list) = cs |> List.sortBy (fun (c,_) -> c.DisplayName) 
        let negvs,posvs = ListMeasureVarOccsWithNonZeroExponents              unt |> sortVars |> List.partition (fun (_,e) -> e<0)
        let negcs,poscs = ListMeasureConOccsWithNonZeroExponents denv.g false unt |> sortCons |> List.partition (fun (_,e) -> e<0)
        let unparL (uv:Typar) = wordL ("'" ^  uv.DisplayName)
        let unconL tc = tcrefL false denv tc
        let prefix = spaceListL  (List.map (fun (v,e) -> if e=1  then unparL v else unparL v -- wordL (Printf.sprintf "^ %d" e)) posvs @
                                  List.map (fun (c,e) -> if e=1  then unconL c else unconL c -- wordL (Printf.sprintf "^ %d" e)) poscs)
        let postfix = spaceListL (List.map (fun (v,e) -> if e= -1 then unparL v else unparL v -- wordL (Printf.sprintf "^ %d" (-e))) negvs @
                                  List.map (fun (c,e) -> if e= -1 then unconL c else unconL c -- wordL (Printf.sprintf "^ %d" (-e))) negcs)
        match (negvs,negcs) with 
        | [],[] -> (match posvs,poscs with [],[] -> wordL "1" | _ -> prefix)
        | _ -> prefix $$ sepL "/" $$ (if List.length negvs + List.length negcs > 1 then sepL "(" $$ postfix $$ sepL ")" else postfix)

    /// Layout type arguments, either NAME<ty,...,ty> or (ty,...,ty) NAME *)
    and tyappL denv env tcL prec prefix args =
        if prefix  then 
            match args with
            | [] -> tcL
            | [arg] -> tcL $$ sepL (if denv.html then "&lt;" else "<") $$ (typeWithPrecL denv env 4 arg) $$ rightL (if denv.html then "&gt;" else ">")
            | args -> bracketIfL (prec <= 1) (tcL $$ angleL denv (typesWithPrecL denv env 2 (sepL ",") args))
        else
            match args with
            | []    -> tcL
            | [arg] ->  typeWithPrecL denv env 2 arg $$ tcL
            | args  -> bracketIfL (prec <= 1) (bracketL (typesWithPrecL denv env 2 (sepL ",") args) --- tcL)

    /// Layout a type, taking precedence into account to insert brackets where needed *)
    and typeWithPrecL denv env prec typ =

        match strip_tpeqns typ with 

        (* Layout a type application *)
        | TType_app (tc,args) when tc.IsMeasureableReprTycon && List.forall (is_dimensionless denv.g) args ->
          typeWithPrecL denv env prec (reduce_tcref_measureable tc args)

        | TType_app (tc,args) -> 
          tyappL denv env (tcrefL false denv tc) prec tc.IsPrefixDisplay args 

        | TType_ucase (UCRef(tc,_),args) -> 
          tyappL denv env (tcrefL false denv tc) prec tc.IsPrefixDisplay args 


        (* Layout a tuple type *)
        | TType_tuple t ->
            bracketIfL (prec <= 2) (typesWithPrecL denv env 2 (wordL "*") t)

        (* Layout a first-class generic type. *)
        | TType_forall (tps,tau) ->
            let tauL = typeWithPrecL denv env prec tau
            match tps with 
            | []  -> tauL
            | [h] -> typarL' denv env h $$ rightL "." --- tauL
            | (h::t) -> spaceListL (List.map (typarL' denv env) (h::t)) $$ rightL "." --- tauL

        (* Layout a function type. *)
        | TType_fun (dty,rty) ->
            let rec loop soFarL ty = 
              match strip_tpeqns ty with 
              | TType_fun (dty,rty) -> loop (soFarL --- (typeWithPrecL denv env 4 dty $$ wordL "->")) rty
              | rty -> soFarL --- typeWithPrecL denv env 5 rty
            bracketIfL (prec <= 4) (loop emptyL typ)

        (* Layout a type variable . *)
        | TType_var r ->
            typarL' denv env r

        | TType_modul_bindings -> wordL "<struct>"
        | TType_measure unt -> auxMeasureL' denv env 4 unt

    /// Layout a list of types, separated with the given separator, either '*' or ',' *)
    and typesWithPrecL denv env prec sep typl = 
        sepListL sep (List.map (typeWithPrecL denv env prec) typl)

    and measureL' denv env unt = auxMeasureL' denv env 5 unt

    /// Layout a single type, taking TypeSimplificationInfo into account *)
    and typeL' denv env typ = 
        typeWithPrecL denv env 5 typ

    and typeL denv typ  = 
        typeL' denv SimplifyTypes.typeSimplificationInfo0 typ

    /// Layout a single type used as the type of a member or value 
    let topTypeL denv env argInfos rty cxs =
        if denv.html && List.exists (snd >> isTTyparSupportsStaticMethod) cxs then
            wordL "overloaded"
        else
            // Parenthesize the return type to match the topValInfo 
            let rtyL  = typeWithPrecL denv env 4 rty
            let cxsL = constraintsL denv env cxs
            match argInfos with
            | [] -> rtyL --- cxsL
            | _  -> 

               // Format each argument, including its name and type 
               let argL (ty,TopArgInfo(argAttribs,idOpt)) = 
                   
                   // Detect an optional argument 
                   let isOptionalArg = HasAttrib denv.g denv.g.attrib_OptionalArgumentAttribute argAttribs
                   match idOpt, isOptionalArg, try_dest_option_ty denv.g ty with 
                   // Layout an optional argument 
                   | Some(id), true, Some(ty) -> 
                       leftL ("?"^id.idText) $$ sepL ":" $$ typeWithPrecL denv env 2 ty 
                   // Layout an unnamed argument 
                   | None, _,_ -> 
                       typeWithPrecL denv env 2 ty
                   // Layout a named argument 
                   | Some id,_,_ -> 
                        leftL id.idText $$ sepL ":" $$ typeWithPrecL denv env 2 ty

               let allArgsL = 
                   argInfos 
                   |> List.mapSquared argL 
                   |> List.map (sepListL (wordL "*"))
                   |> List.map (fun x -> (x $$ wordL "->")) 
               (List.foldBack (---) allArgsL rtyL) --- cxsL

    let typarDeclsL denv nmL prefix typars = 
        typarDeclsL' denv SimplifyTypes.typeSimplificationInfo0 nmL prefix typars 

    let measureL denv unt  = 
        measureL' denv SimplifyTypes.typeSimplificationInfo0 unt

    let constraintL denv typars = 
        match constraintL' denv SimplifyTypes.typeSimplificationInfo0 typars  with 
        | h::_ -> h 
        | [] -> emptyL

    let typesAndConstraintsL denv taus =
        let _,ptaus,cxs = PrettifyTypesN denv.g taus
        let env = SimplifyTypes.CollectInfo true ptaus cxs
        List.map (typeL' denv env) ptaus,constraintsL denv env env.SimplifyTypes.postfixConstraints

    let topPrettifiedTypesAndConstraintsL denv argInfos tau cxs = 
        let env = SimplifyTypes.CollectInfo true (tau:: List.collect (List.map fst) argInfos) cxs
        topTypeL denv env argInfos tau env.SimplifyTypes.postfixConstraints

    let topTypAndConstraintsL denv argInfos tau = 
        let _,(argInfos,tau),cxs = PrettifyTypesN1 denv.g (argInfos,tau)
        topPrettifiedTypesAndConstraintsL denv [argInfos] tau cxs

    let memberTypeAndConstraintsL denv argInfos retTy parentTyparTys = 
        let _,(parentTyparTys,argInfos,retTy),cxs = PrettifyTypesNM1 denv.g (parentTyparTys,argInfos,retTy)
        (* Filter out the parent typars, which don't get shown in the member signature *)
        let cxs = cxs |> List.filter (fun (tp,_) -> not (List.exists (dest_typar_typ denv.g >> typar_ref_eq tp) parentTyparTys)) 
        topPrettifiedTypesAndConstraintsL denv argInfos retTy cxs

    (* Layout: type spec - class, datatype, record, abbrev *)

    let memberTypeCoreL denv memberToParentInst (methTypars:typars,argInfos,retTy) = 
       let niceMethodTypars, allTyparInst = 
           let methTyparNames = methTypars |> List.mapi (fun i tp -> if (NeedsPrettyTyparName tp) then sprintf "a%d" (List.length memberToParentInst + i) else tp.Name)
           NewPrettyTypars memberToParentInst methTypars methTyparNames

       let retTy = InstType allTyparInst retTy
       let argInfos = argInfos |> List.map (fun infos -> if isNil infos then [(denv.g.unit_ty,TopValInfo.unnamedTopArg1)] else infos |> List.map (map1'2 (InstType allTyparInst))) 

       (* Also format dummy types corresponding to any type variables on the container to make sure they *)
       (* aren't chosen as names for displayed variables. *)
       let memberParentTypars = List.map fst memberToParentInst
       let parentTyparTys = List.map (mk_typar_ty >> InstType allTyparInst) memberParentTypars

       niceMethodTypars,memberTypeAndConstraintsL denv argInfos retTy parentTyparTys

    let memberTypeL denv v tps argInfos retTy = 
        match PartitionValTypars denv.g v with
        | Some(_,memberParentTypars,memberMethodTypars,memberToParentInst,_) ->  
           memberTypeCoreL denv memberToParentInst (memberMethodTypars, argInfos, retTy)
        | None -> 
           [],topTypAndConstraintsL denv (List.concat argInfos) retTy 

    let memberSigL denv  (memberToParentInst,nm,methTypars,argInfos,retTy) = 
        let niceMethodTypars,tauL = memberTypeCoreL denv memberToParentInst (methTypars, argInfos, retTy)
        let nameL = 
            let nameL = wordL (DemangleOperatorName nm)
            let nameL = if denv.showTyparBinding then typarDeclsL denv nameL true niceMethodTypars else nameL
            nameL
        nameL $$ wordL ":" $$ tauL


    let memberLP denv (path : Path) (v:Val) = 
        let membInfo = the v.MemberInfo
        let topValInfo = the v.TopValInfo
        let id = v.Id
        let ty = v.Type
        let stat = memFlagsL denv.htmlHideRedundantKeywords membInfo.MemberFlags
        let tps,argInfos,rty,_ = GetMemberTypeInFSharpForm denv.g membInfo.MemberFlags topValInfo ty v.Range
        let mkNameL niceMethodTypars name =       
            let name  = DemangleOperatorName name
            let nameL = if denv.showMemberContainers then tcrefL false denv (tcref_of_stripped_typ denv.g (enclosing_formal_typ_of_val denv.g v)) $$ sepL "." $$ (name &== path) else name &== path
            let nameL = if denv.showTyparBinding then typarDeclsL denv nameL true niceMethodTypars else nameL
            let nameL = accessibilityL denv v.Accessibility nameL
            nameL

        match membInfo.MemberFlags.MemberKind with 
        | MemberKindMember -> 
            let niceMethodTypars,tauL = memberTypeL denv v tps argInfos rty
            let nameL = mkNameL niceMethodTypars membInfo.CompiledName
            stat --- (nameL $$ wordL ":" $$ tauL)
        | MemberKindClassConstructor  
        | MemberKindConstructor -> 
            let niceMethodTypars,tauL = memberTypeL denv v tps argInfos rty
            let newL = accessibilityL denv v.Accessibility (wordL "new")
            stat ++ newL $$ wordL ":" $$ tauL
        | MemberKindPropertyGetSet -> stat
        | MemberKindPropertyGet -> 
            if isNil argInfos then 
                // use error recovery because intellisense on an incomplete file will show this
                errorR(Error("invalid form for a property getter. At least one '()' argument is required when using the explicit syntax",id.idRange));
                stat --- wordL membInfo.PropertyName --- wordL "with get"
            else
                let argInfos = 
                    match argInfos with 
                    | [[(ty,_)]] when is_unit_typ denv.g ty -> []
                    | _ -> argInfos

                let niceMethodTypars,tauL = memberTypeL denv v tps argInfos rty
                let nameL = mkNameL niceMethodTypars membInfo.PropertyName
                stat --- (nameL $$ wordL ":" $$ (if isNil argInfos then tauL else tauL --- wordL "with get"))
        | MemberKindPropertySet -> 
            if argInfos.Length <> 1 || isNil argInfos.Head then 
                // use error recovery because intellisense on an incomplete file will show this
                errorR(Error("invalid form for a property setter. At least one argument is required",id.idRange));
                stat --- wordL membInfo.PropertyName --- wordL "with set"
            else 
                let argInfos,valueInfo = List.frontAndBack argInfos.Head
                let niceMethodTypars,tauL = memberTypeL denv v tps (if isNil argInfos then [] else [argInfos]) (fst valueInfo)
                let nameL = mkNameL niceMethodTypars membInfo.PropertyName
                stat --- (nameL $$ wordL ":" $$ (tauL --- wordL "with set"))

    let nonMemberValSpecLP denv (path : Path) (tps,v:Val,tau,cxs) =
        let env = SimplifyTypes.CollectInfo true [tau] cxs
        let cxs = env.SimplifyTypes.postfixConstraints
        let argInfos,rty = GetTopTauTypeInFSharpForm denv.g (arity_of_val v).ArgInfos tau v.Range
        (* Drop the names from value arguments when printing them *)
        let argInfos = argInfos |> List.mapSquared (fun (ty,info) -> ty,TopValInfo.unnamedTopArg1)
        let nameL = v.DisplayName &== path
        let nameL = accessibilityL denv v.Accessibility nameL
        let nameL = if v.IsMutable then wordL "mutable" ++ nameL else nameL
        let nameL = 
            if not denv.html && v.MustInline && not denv.suppressInlineKeyword then 
                wordL "inline" ++ nameL 
            else 
                nameL

        let isOverGeneric = List.length (Zset.elements (free_in_type CollectTyparsNoCaching tau).FreeTypars) < List.length tps (* Bug: 1143 *)
        let isTyFunction  = v.IsTypeFunction     (* Bug: 1143, and innerpoly tests *)
        let typarBindingsL = 
            if isTyFunction || isOverGeneric || denv.showTyparBinding then 
                typarDeclsL denv nameL true tps 
            else nameL
        let valAndTypeL = (wordL "val"  $$ typarBindingsL --- wordL ":") --- topTypeL denv env argInfos rty cxs
        match denv.generatedValueLayout v with
          | None      -> valAndTypeL
          | Some rhsL -> (valAndTypeL ++ wordL "=") --- rhsL

    let valLP denv (path : Path) (v:Val) =
        let vL = 
            match v.MemberInfo with 
            | None -> 
                let tps,tau = v.TypeScheme

                // adjust the type in case this is the 'this' pointer stored in a reference cell
                let tau = if v.BaseOrThisInfo = CtorThisVal && is_refcell_ty denv.g tau then dest_refcell_ty denv.g tau else tau

                let tprenaming,ptau,cxs = PrettyTypes.PrettifyTypes1 denv.g tau
                let ptps = 
                    tps  
                       |> generalize_typars 
                       // Badly formed code may instantiate rigid declared typars to types, e.g. see bug
                       // Hence we double check here that the thing is really a type variable
                       |> List.map (InstType tprenaming)
                       |> List.filter (is_anypar_typ denv.g) 
                       |> List.map (dest_anypar_typ denv.g)
                nonMemberValSpecLP denv path (ptps,v,ptau,cxs)
            | Some _ -> 
                memberLP denv path v
        attribsL denv KindType v.Attribs vL

    let valL denv (v:Val) = valLP denv Path.NoPath v

    let extensionMembersLP denv (path:Path) (vs:Val list) =
      let extensionMemberLP (v:Val) =
        let tycon = v.MemberApparentParent.Deref
        let nameL = wordL tycon.DisplayName
        let nameL = accessibilityL denv tycon.Accessibility nameL // "type-accessibility"
        let tps =
          match PartitionValTypars denv.g v with
            | Some(_,memberParentTypars,memberMethodTypars,_,_) -> memberParentTypars
            | None -> []          
        let lhsL = wordL "type" $$ typarDeclsL denv nameL tycon.IsPrefixDisplay tps
        (lhsL $$ wordL "with") @@-- (valLP denv path v)
      aboveListL (List.map extensionMemberLP vs)

    let ucaseArgTypesL denv argtys = 
        sepListL (wordL "*") (List.map (typeWithPrecL denv SimplifyTypes.typeSimplificationInfo0 2) argtys)

    let ucaseLP denv (path : Path) prefixL ucase =
        let nmL = (DemangleOperatorName ucase.ucase_id.idText) &== path
        //let nmL = accessibilityL denv ucase.Accessibility nmL
        match ucase.RecdFields |> List.map (fun rfld -> rfld.FormalType) with
        | []     -> (prefixL $$ nmL)
        | argtys -> (prefixL $$ nmL $$ wordL "of") --- ucaseArgTypesL denv argtys

    let ucaseL denv prefixL ucase = ucaseLP denv Path.Empty prefixL ucase

    let ucasesLP denv (path : Path) ucases =
        let prefixL = wordL "|" // See bug://2964 - always prefix in case preceeded by accessibility modifier
        List.map (ucaseLP denv path prefixL) ucases
        
    let rfspecLP addAccess denv (path : Path) (fld:RecdField) =
        let lhs = fld.Name &== path
        let lhs = (if addAccess then accessibilityL denv fld.Accessibility lhs else lhs)
        let lhs = if fld.IsMutable then wordL "mutable" --- lhs else lhs
        (lhs $$ rightL ":") --- typeL denv fld.FormalType

    let rfspecL addAccess denv fld = rfspecLP addAccess denv Path.Empty fld

    /// When to force a break? "type tyname = <HERE> repn"
    /// When repn is class or datatype constructors (not single one).
    let breakTypeDefnEqn repr =
        match repr with 
        | TFsObjModelRepr _ -> true
        | TFiniteUnionRepr r    -> (List.length (r.funion_ucases.UnionCasesAsList) > 1) 
        | TRecdRepr _ -> true
        | TAsmRepr _ 
        | TILObjModelRepr _  -> false
        | TMeasureableRepr _ -> false

    let tyconLP denv (path : Path) typewordL (tycon:Tycon) =
        let prefix = tycon.IsPrefixDisplay
        let name   = tycon.DisplayName
        let nameL  = name &== path
        let nameL = accessibilityL denv tycon.Accessibility nameL 
        let denv = denv_scope_access tycon.Accessibility denv
        let path  = path.Add name // the full names of constructors and , e.g., have their type name as part of the path
        let lhsL =
            let tps = tycon.TyparsNoRange
            let tpsL = typarDeclsL denv nameL prefix tps
            typewordL $$ tpsL
        let memberImplementLs,memberCtorLs,memberInstanceLs,memberStaticLs = 
            let tcaug = tycon.TypeContents
            let adhoc = 
                adhoc_of_tycon tycon  
                |> List.filter (vref_is_dispatch_slot >> not) 
                |> List.filter (fun v -> 
                                    match v.MemberInfo.Value.ImplementedSlotSigs with 
                                    | TSlotSig(_,oty,_,_,_,_) :: _ -> 
                                        // Don't print overrides in HTML docs
                                        denv.showOverrides && 
                                        // Don't print individual methods forming interface implementations - these are currently never exported 
                                        not (is_interface_typ denv.g oty)
                                    | [] -> true)
                |> List.filter (fun v -> denv.showObsoleteMembers || not (HasAttrib denv.g denv.g.attrib_SystemObsolete v.Attribs))
            (* sort *)            
            let sortKey (v:ValRef) = (not v.IsConstructor,    (* constructors before others *)
                                      v.Id.idText)            (* sort by name *)
            let adhoc = adhoc |> List.sortBy sortKey
            let iimpls = 
                match tycon.TypeReprInfo with 
                | Some (TFsObjModelRepr r) when r.fsobjmodel_kind = TTyconInterface -> []
                | _ -> tcaug.tcaug_implements
            let iimpls = iimpls |> List.filter (fun (ty,compgen,m) -> not compgen)
            (* if TTyconInterface, the iimpls should be printed as inheritted interfaces *)
            let iimplsLs = iimpls |> List.map (fun (ty,compgen,m) -> wordL "interface" --- typeL denv ty)
            let adhocCtorsLs    = adhoc |> List.filter (fun v -> v.IsConstructor)                               |> List.map (fun vref -> valLP denv path (deref_val vref))
            let adhocInstanceLs = adhoc |> List.filter (fun v -> not v.IsConstructor && v.IsInstanceMember)     |> List.map (fun vref -> valLP denv path (deref_val vref))
            let adhocStaticLs   = adhoc |> List.filter (fun v -> not v.IsConstructor && not v.IsInstanceMember) |> List.map (fun vref -> valLP denv path (deref_val vref))
            iimplsLs,adhocCtorsLs,adhocInstanceLs,adhocStaticLs
        let memberLs = memberImplementLs @ memberCtorLs @ memberInstanceLs @ memberStaticLs
        let addMembersAsWithEnd reprL = 
            if isNil memberLs then reprL
            else reprL @@ (wordL "with" @@-- aboveListL memberLs) @@ wordL "end"

        let reprL = 
            match tycon.TypeReprInfo with 
            | Some repr -> 
                let brk  = nonNil memberLs || breakTypeDefnEqn repr
                let rhsL =                     
                    let addReprAccessL l = accessibilityL denv tycon.TypeReprAccessibility l 
                    let denv = denv_scope_access tycon.TypeReprAccessibility denv  
                    match repr with 
                    | TRecdRepr flds ->
                        let recdFieldRefL fld = rfspecLP false denv path fld $$ rightL ";"
                        let recdL = tycon.TrueFieldsAsList |> List.map recdFieldRefL |> aboveListL |> braceL
                        addMembersAsWithEnd (addReprAccessL recdL)
                        
                    | TFsObjModelRepr r -> 
                        match r.fsobjmodel_kind with 
                        | TTyconDelegate (TSlotSig(nm,typ, _,_,paraml, rty)) ->
                            let rty = GetFSharpViewOfReturnType denv.g rty
                            wordL "delegate of" --- topTypeL denv SimplifyTypes.typeSimplificationInfo0 (paraml |> List.mapSquared (fun sp -> (sp.Type, TopValInfo.unnamedTopArg1))) rty []
                        | _ ->
                            match r.fsobjmodel_kind with
                            | TTyconEnum -> 
                                tycon.TrueFieldsAsList
                                |> List.map (fun f -> 
                                                  match f.LiteralValue with 
                                                  | None -> emptyL
                                                  | Some c -> wordL "| " $$ (f.Name &== path) $$ wordL " = " $$ constL c)
                                |> aboveListL
                            | _ -> 
                                let start = 
                                    match r.fsobjmodel_kind with
                                    | TTyconClass -> "class" 
                                    | TTyconInterface -> "interface" 
                                    | TTyconStruct -> "struct" 
                                    | TTyconEnum -> "enum" 
                                    | _ -> failwith "???"
                                let inherits = 
                                   match r.fsobjmodel_kind, tycon.TypeContents.tcaug_super with
                                   | TTyconClass,Some super -> [wordL  "inherit" $$ (typeL denv super)] 
                                   | TTyconInterface,_ -> 
                                     let tcaug = tycon.TypeContents
                                     tcaug.tcaug_implements 
                                       |> List.filter (fun (ity,compgen,_) -> not compgen)
                                       |> List.map (fun (ity,compgen,_) -> wordL  "inherit" $$ (typeL denv ity))
                                   | _ -> []
                                let vsprs = 
                                    adhoc_of_tycon tycon 
                                    |> List.filter (fun v -> isNil (the(v.MemberInfo)).ImplementedSlotSigs) 
                                    |> List.filter vref_is_dispatch_slot 
                                    |> List.map (fun vref -> valLP denv path (deref_val vref))
                                let staticValsLs  = 
                                    tycon.TrueFieldsAsList
                                    |> List.filter (fun f -> f.IsStatic)
                                    |> List.map (fun f -> wordL "static" $$ wordL "val" $$ rfspecLP true denv path f)
                                let instanceValsLs  = 
                                    tycon.TrueFieldsAsList
                                    |> List.filter (fun f -> not f.IsStatic)
                                    |> List.map (fun f -> wordL "val" $$ rfspecLP true denv path f)
                                let alldecls = inherits @ memberImplementLs @ memberCtorLs @ instanceValsLs @ vsprs @ memberInstanceLs @ staticValsLs @ memberStaticLs
                                let emptyMeasure = match tycon.TypeOrMeasureKind with KindMeasure -> isNil alldecls | _ -> false
                                if emptyMeasure then emptyL else (wordL start @@-- aboveListL alldecls) @@ wordL "end"
                    | TFiniteUnionRepr ucases        -> 
                        let ucasesL = tycon.UnionCasesAsList |> ucasesLP denv path |> aboveListL
                        addMembersAsWithEnd (addReprAccessL ucasesL)
                    | TAsmRepr s                      -> 
                        wordL "(# \"<Common IL Type Omitted>\" #)"
                    | TMeasureableRepr ty                 ->
                        typeL denv ty
                 
                    | TILObjModelRepr (scoref,enc,td) -> IlPrint.ilTypeDefL denv path td

                let brk  = match tycon.TypeReprInfo with
                           | Some (TILObjModelRepr _) -> true
                           | _                        -> brk
                if rhsL = emptyL
                   then lhsL
                   else (* only unions and records can have accessibility on their representation *)
                        if brk
                           then (lhsL $$ wordL "=") @@-- rhsL 
                           else (lhsL $$ wordL "=") ---  rhsL

            | None   -> 
                match tycon.TypeAbbrev with
                | None   -> 
                    addMembersAsWithEnd lhsL 
                | Some a -> 
                    (lhsL $$ wordL "=") --- (typeL denv a)
        attribsL denv tycon.TypeOrMeasureKind tycon.Attribs reprL

    let tyconL denv typewordL tycon = tyconLP denv Path.NoPath typewordL tycon

    let prettyTypeL denv typ = 
        let tprenaming,typ,cxs = PrettyTypes.PrettifyTypes1 denv.g typ
        let env = SimplifyTypes.CollectInfo true [typ] cxs
        let cxsL = constraintsL denv env env.SimplifyTypes.postfixConstraints
        typeWithPrecL denv env 2 typ  --- cxsL

    (* Layout: exception spec *)
      
    let exnDefnReprL denv repr =
        match repr with 
        | TExnAbbrevRepr ecref -> wordL "=" --- tcrefL true denv ecref
        | TExnAsmRepr tref     -> wordL "=" --- wordL "(# ... #)"
        | TExnNone             -> emptyL
        | TExnFresh r          -> 
            match r.TrueFieldsAsList with
            | []  -> emptyL
            | r -> wordL "of" --- ucaseArgTypesL denv (r |> List.map (fun rfld -> rfld.FormalType))

    let exnDefnLP denv (path : Path) (exnc:Entity) =
        let nm = exnc.DemangledExceptionName
        let nmL = annotateWithPath nm path (mangle_exception_name nm)
        let nmL = accessibilityL denv exnc.TypeReprAccessibility nmL
        let exnL = wordL "exception" $$ nmL // need to tack on the Exception at the right of the name for goto definition
        exnL $$ exnDefnReprL denv exnc.ExceptionInfo

    let exnDefnL denv exnc = exnDefnLP denv Path.NoPath exnc

    (* Layout: module spec *)

    let tycon_specsLP denv (path : Path) (tycons:Tycon list) =
        match tycons with 
        | [] -> emptyL
        | [h] when h.IsExceptionDecl -> exnDefnLP denv path h
        | h :: t -> 
            let x  = tyconLP denv path (wordL "type") h
            let xs = List.map (tyconLP denv path (wordL "and")) t
            aboveListL (x::xs)

    /// Layout the inferred signature of a compilation unit
    let InferredSigOfModuleExprL showHeader denv expr =

        let rec isConcreteNamespace x = 
            match x with 
            | TMDefRec(tycons,binds,mbinds,m) -> 
                nonNil tycons || not (FlatList.isEmpty binds) || (mbinds |> List.exists (fun (TMBind(x,_)) -> not x.IsNamespace))
            | TMDefLet(bind,m)  -> true
            | TMDefDo(e,m)  -> true
            | TMDefs(defs) -> defs |> List.exists isConcreteNamespace 
            | TMAbstract(TMTyped(_,def,_)) -> isConcreteNamespace def

        let rec imexprLP denv (path : Path) (TMTyped(mty,def,m)) = imdefLP denv path def

        and imexprL denv (TMTyped(mty,def,m)) = imexprLP denv Path.Empty (TMTyped(mty,def,m))

        and imdefsLP denv (path : Path) x = aboveListL (x |> List.map (imdefLP denv path))

        and imdefLP denv (path : Path) x = 
            let filterVal    (v:Val) = not v.IsCompilerGenerated && isNone v.MemberInfo
            let filterExtMem (v:Val) = v.IsExtensionMember
            match x with 
            | TMDefRec(tycons,binds,mbinds,m) -> 
                 tycon_specsLP denv path tycons @@ 
                 (binds |> FlatList.to_list |> vars_of_binds |> List.filter filterExtMem |> extensionMembersLP denv path) @@
                 (binds |> FlatList.to_list |> vars_of_binds |> List.filter filterVal    |> List.map (valLP denv path)   |> aboveListL) @@
                 (mbinds |> List.map (imbindLP denv path) |> aboveListL)
            | TMDefLet(bind,m) -> ([bind] |> vars_of_binds |> List.filter filterVal    |> List.map (valLP denv path) |> aboveListL)
            | TMDefs(defs) -> imdefsLP denv path defs
            | TMDefDo _  -> emptyL
            | TMAbstract(mexpr) -> imexprLP denv path mexpr
        and imbindLP denv (path : Path) (TMBind(mspec, def)) = 
            let nm = demangled_name_of_modul mspec
            let innerPath = (full_cpath_of_modul mspec).AccessPath
            let outerPath = mspec.CompilationPath.AccessPath
            let k = mspec.ModuleOrNamespaceType.ModuleOrNamespaceKind

            let denv = denv_add_open_path (List.map fst innerPath) denv
            if mspec.IsNamespace then  
                let basic = imdefLP denv path def
                // Check if this namespace contains anything interesting
                if isConcreteNamespace def then 
                    // This is a container namespace. We print the header when we get to the first concrete module.
                    let headerL = wordL ("namespace " ^ (String.concat "." (innerPath |> List.map fst)))
                    headerL @@-- basic
                else
                    // This is a namespace that only contains namespaces. Skipt the header
                    basic
            else
                // This is a module 
                let nmL   = accessibilityL denv mspec.Accessibility (nm &== path)
                let denv  = denv_scope_access mspec.Accessibility denv
                let basic = imdefLP denv path def
                // Check if its an outer module or a nested module
                if (outerPath |> List.forall (fun (_,istype) -> istype = Namespace) ) then 
                    // OK, this is an outer module
                    if showHeader then 
                        // OK, we're not in F# Interactive
                        // Check if this is an outer module with no namespace
                        if isNil outerPath then 
                            // If so print a "module" declaration
                            (wordL "module" $$ nmL) @@ basic
                        else 
                            // Otherwise this is an outer module contained immediately in a namespace
                            // We already printed the namespace declaration earlier.  So just print the 
                            // module now.
                            ((wordL "module" $$ nmL $$ wordL "=" $$ wordL "begin") @@-- basic) @@ wordL "end"
                    else
                        // OK, wer'e in F# Interactive, presumably the implicit module for each interaction.
                        basic
                else
                    // OK, this is a nested module
                    ((wordL "module" $$ nmL $$ wordL "=" $$ wordL "begin") @@-- basic) @@ wordL "end"
        imexprL denv expr

    type DeclSpec = 
        | DVal of Val 
        | DTycon of Tycon 
        | DException of Tycon 
        | DModul of ModuleOrNamespace

    let rangeOfDeclSpec = function
        | DVal   v -> v.Range
        | DTycon t -> t.Range
        | DException t -> t.Range
        | DModul m -> m.Range

    /// modul - provides (valspec)* - and also types, exns and submodules.
    /// Each defines a decl block on a given range.
    /// Can sort on the ranges to recover the original declaration order.
    let rec ModuleOrNamespaceTypeLP (topLevel : bool)(denv : DisplayEnv)(path : Path)(mtype : ModuleOrNamespaceType) =
        (* REVIEW: consider a better way to keep decls in order. *)
        let decl_specs : DeclSpec list =
            List.concat
              [mtype.AllValuesAndMembers |> NameMap.range |> List.filter (fun v -> not v.IsCompilerGenerated && v.MemberInfo = None) |> List.map DVal;
               mtype.TypeDefinitions |> List.map DTycon;
               mtype.ExceptionDefinitions |> List.map DException;
               mtype.ModuleAndNamespaceDefinitions |> List.map DModul;
              ]
       
        let decl_specs = List.sortWith (orderOn rangeOfDeclSpec range_ord) decl_specs
        let decl_specL =
          function // only show namespaces / modules at the top level; this is because we've no global namespace
          | DVal  vspec      when not topLevel -> valLP                     denv path vspec
          | DTycon tycon     when not topLevel -> tyconLP                   denv path (wordL "type") tycon
          | DException tycon when not topLevel -> exnDefnLP                 denv path tycon 
          | DModul mspec                       -> ModuleOrNamespaceLP false denv path mspec
          | _                                  -> emptyL // this catches non-namespace / modules at the top-level

        aboveListL (List.map decl_specL decl_specs)

    and ModuleOrNamespaceLP (topLevel : bool)(denv : DisplayEnv)(path : Path)(mspec : ModuleOrNamespace) = 
        let istype = mspec.ModuleOrNamespaceType.ModuleOrNamespaceKind
        let nm     = demangled_name_of_modul mspec
        let denv   = denv_add_open_modref (mk_local_modref mspec) denv
        let nmL    = accessibilityL denv mspec.Accessibility (nm &== path)
        let denv   = denv_scope_access mspec.Accessibility denv       
        let path   = path.Add nm // tack on the current module to be used in calls to linearise all subterms
        let body   = ModuleOrNamespaceTypeLP topLevel denv path  mspec.ModuleOrNamespaceType
        if istype = Namespace
           then (wordL "namespace" $$ nmL) @@-- body
           else (wordL "module" $$ nmL $$ wordL "= begin") @@-- body @@ wordL "end"

    let ModuleOrNamespaceTypeL (denv : DisplayEnv)(mtype : ModuleOrNamespaceType) = ModuleOrNamespaceTypeLP false denv Path.Empty mtype
    let ModuleOrNamespaceL (denv : DisplayEnv)(mspec : ModuleOrNamespace) = ModuleOrNamespaceLP false denv Path.Empty mspec
    let AssemblyL denv (mspec : ModuleOrNamespace) = ModuleOrNamespaceTypeLP true denv Path.Empty mspec.ModuleOrNamespaceType // we seem to get the *assembly* name as an outer module, this strips this off

    //--------------------------------------------------------------------------
    // Nice printing of a subset of expressions, e.g. for refutations in pattern matching
    //--------------------------------------------------------------------------

    let rec dataExprL denv expr = dataExprWrapL denv false expr
    and atomL denv expr = dataExprWrapL denv true  expr (* true means bracket if needed to be atomic expr *)

    and dataExprWrapL denv isAtomic expr =
        let wrap = bracketIfL isAtomic in (* wrap iff require atomic expr *)
        match expr with
        | TExpr_const (c,m,ty)                          -> 
            if is_enum_typ denv.g ty then 
                wordL "enum" $$ angleL denv (typeL denv ty) $$ bracketL (constL c)
            else
                constL c

        | TExpr_val (v,flags,m)                         -> wordL (v.DisplayName)
        | TExpr_link rX                                 -> dataExprWrapL denv isAtomic (!rX)
        | TExpr_op(TOp_ucase(c),tyargs,args,m)        -> 
            if denv.g.ucref_eq c denv.g.nil_ucref then wordL "[]"
            elif denv.g.ucref_eq c denv.g.cons_ucref then 
                let rec strip = function (TExpr_op(TOp_ucase(c),tyargs,[h;t],m)) -> h::strip t | _ -> []
                listL (dataExprL denv) (strip expr)
            elif isNil(args) then 
                wordL c.CaseName 
            else 
                (wordL c.CaseName ++ bracketL (commaListL (dataExprsL denv args)))
            
        | TExpr_op(TOp_exnconstr(c),_,args,m)           ->  (wordL c.MangledName ++ bracketL (commaListL (dataExprsL denv args)))
        | TExpr_op(TOp_tuple,tys,xs,m)                  -> tupleL (dataExprsL denv xs)
        | TExpr_op(TOp_recd (ctor,tc),tinst,xs,m)       -> let fields = tc.TrueInstanceFieldsAsList
                                                           let lay fs x = (wordL fs.rfield_id.idText $$ sepL "=") --- (dataExprL denv x)
                                                           leftL "{" $$ semiListL (List.map2 lay fields xs) $$ rightL "}" 
        | TExpr_op(TOp_array,[ty],xs,m)                 -> leftL "[|" $$ semiListL (dataExprsL denv xs) $$ rightL "|]"
        | _ -> wordL "?"
    and dataExprsL denv xs = List.map (dataExprL denv) xs

    //--------------------------------------------------------------------------
    // Print Signatures/Types - ouput functions - old style
    //-------------------------------------------------------------------------- 
        
    (* A few old-style o/p functions are used, e.g. in tc.ml *)    
    let output_tref                 denv os x    = trefL denv x                |> bufferL os
    let output_tcref                denv os x    = x |> tcrefL false denv      |> bufferL os    
    let output_val_spec             denv os x    = x |> valL denv              |> bufferL os
    let output_typ                  denv os x    = x |> typeL denv             |> bufferL os  
    let output_exnc                 denv os x    = x |> exnDefnL denv          |> bufferL os
    let output_typar_constraints    denv os x    = x |> constraintsL denv SimplifyTypes.typeSimplificationInfo0  |> bufferL os
    let string_of_typar_constraints denv x       = x |> constraintsL denv SimplifyTypes.typeSimplificationInfo0  |> showL
    let output_rfield               denv os x    = x |> rfspecL false denv |> bufferL os
    let output_tycon                denv os x    = tyconL denv (wordL "type") x      |> bufferL os
    let output_ucase                denv os x    = ucaseL denv (wordL "|") x |> bufferL os
    let output_typars               denv nm os tps  = (typarDeclsL denv  (wordL nm) true tps)  |> bufferL os
    let output_typar_constraint     denv os tpc  = output_typar_constraints denv os [tpc]
    let string_of_typar_constraint  denv tpc     = string_of_typar_constraints denv [tpc]
    let string_of_typ               denv    typ  = typeL denv typ                                 |> showL
    let pretty_string_of_typ        denv    typ  = prettyTypeL denv typ                          |> showL
    let pretty_string_of_unit       denv    unt  = measureL denv unt                          |> showL
    let string_of_rfield            denv x    = x |> rfspecL false denv |> showL
    let string_of_tycon             denv x    = tyconL denv (wordL "type") x      |> showL
    let string_of_ucase             denv x    = ucaseL denv (wordL "|") x |> showL
    let string_of_typars            denv nm tps  = (typarDeclsL denv  (wordL nm) true tps)  |> showL
    let string_of_exnc              denv x    = x |> exnDefnL denv          |> showL
    let string_of_val_spec          denv x    = x |> valL denv              |> showL
    (* Print members with a qualification showing the type they are contained in *)
    let output_qualified_val_spec denv os v = output_val_spec { denv with showMemberContainers=true; } os v
    let string_of_qualified_val_spec denv v = string_of_val_spec { denv with showMemberContainers=true; } v

end

//--------------------------------------------------------------------------
// DEBUG layout
//---------------------------------------------------------------------------

module DebugPrint = begin
    open Microsoft.FSharp.Compiler.Layout
    open PrettyTypes
    let layout_ranges = ref false  

    let intL (n:int)          = wordL (string n )
    let int64L (n:int64)          = wordL (string n )

    let namemapL xL xmap = NameMap.foldRange (fun x z -> z @@ xL x) xmap emptyL

    let bracketIfL x lyt = if x then bracketL lyt else lyt

    let lvalopL x = 
        match x with 
        | LGetAddr  -> wordL "LGetAddr"
        | LByrefGet -> wordL "LByrefGet"
        | LSet      -> wordL "LSet"
        | LByrefSet -> wordL "LByrefSet"

    let angleBracketL l = leftL "<" $$ l $$ rightL ">"
    let angleBracketListL l = angleBracketL (sepListL (sepL ",") l)


    let memFlagsL hide memFlags = 
        let stat = if hide || memFlags.MemberIsInstance || (memFlags.MemberKind = MemberKindConstructor) then emptyL else wordL "static"
        let stat = if not memFlags.MemberIsDispatchSlot && memFlags.MemberIsVirtual then stat ++ wordL "virtual" 
                   elif not hide && memFlags.MemberIsDispatchSlot then stat ++ wordL "abstract" 
                   elif memFlags.MemberIsOverrideOrExplicitImpl then stat ++ wordL "override" 
                   else stat
        (* let stat = if memFlags.MemberIsFinal then stat ++ wordL "final" else stat in  *)
        stat

    let stampL n w = if !verboseStamps then w $$ sepL "#" $$ int64L n else w

    let tcrefL (tc:TyconRef) = wordL tc.DisplayName |> stampL tc.Stamp


    let rec auxTypeL env typ = auxTypeWrapL env false typ

    and auxTypeAtomL env typ = auxTypeWrapL env true  typ

    and auxTyparsL env tcL prefix tinst = 
       match tinst with 
       | [] -> tcL
       | [t] -> 
         let tL = auxTypeAtomL env t
         if prefix then        tcL $$ angleBracketL tL 
         else            tL $$ tcL 
       | _ -> 
         let tinstL = List.map (auxTypeL env) tinst
         if prefix then                   
             tcL $$ angleBracketListL tinstL
         else  
             tupleL tinstL $$ tcL
            
    and auxTypeWrapL env isAtomic typ = 
        let wrap x = NicePrint.bracketIfL isAtomic x in (* wrap iff require atomic expr *)
        match strip_tpeqns typ with
        | TType_forall (typars,rty) -> 
           (leftL "!" $$ typarDeclsL typars --- auxTypeL env rty) |> wrap
        | TType_ucase (UCRef(tcref,_),tinst)  
        | TType_app (tcref,tinst)   -> 
           let prefix = tcref.IsPrefixDisplay
           let tcL = tcrefL tcref
           auxTyparsL env tcL prefix tinst
        | TType_tuple typs          -> sepListL (wordL "*") (List.map (auxTypeAtomL env) typs) |> wrap
        | TType_fun (f,x)           -> ((auxTypeAtomL env f $$ wordL "->") --- auxTypeL env x) |> wrap
        | TType_var typar           -> auxTyparWrapL env isAtomic typar 
        | TType_modul_bindings              -> wordL "structT"
        | TType_measure unt -> 
#if DEBUG
          leftL "{" $$
          (match !global_g with
           | None -> wordL "<no global g>"
           | Some g -> 
             let sortVars (vs:(Typar * int) list) = vs |> List.sortBy (fun (v,_) -> v.DisplayName) 
             let sortCons (cs:(TyconRef * int) list) = cs |> List.sortBy (fun (c,_) -> c.DisplayName) 
             let negvs,posvs = ListMeasureVarOccsWithNonZeroExponents         unt |> sortVars |> List.partition (fun (_,e) -> e<0)
             let negcs,poscs = ListMeasureConOccsWithNonZeroExponents g false unt |> sortCons |> List.partition (fun (_,e) -> e<0)
             let unparL (uv:Typar) = wordL ("'" ^  uv.DisplayName)
             let unconL tc = tcrefL tc
             let prefix = spaceListL  (List.map (fun (v,e) -> if e=1  then unparL v else unparL v -- wordL (Printf.sprintf "^ %d" e)) posvs @
                                       List.map (fun (c,e) -> if e=1  then unconL c else unconL c -- wordL (Printf.sprintf "^ %d" e)) poscs)
             let postfix = spaceListL (List.map (fun (v,e) -> if e= -1 then unparL v else unparL v -- wordL (Printf.sprintf "^ %d" (-e))) negvs @
                                       List.map (fun (c,e) -> if e= -1 then unconL c else unconL c -- wordL (Printf.sprintf "^ %d" (-e))) negcs)
             match (negvs,negcs) with 
             | [],[] -> prefix 
             | _ -> prefix $$ sepL "/" $$ postfix) $$
          rightL "}"
#else
          wordL "<measure>"
#endif

    and auxTyparWrapL env isAtomic (typar:Typar) =
          let wrap x = NicePrint.bracketIfL isAtomic x in (* wrap iff require atomic expr *)  
          (* There are several cases for pprinting of typar.
           * 
           *   'a              - is multiple  occurance.
           *   #Type           - inplace coercion constraint and singleton
           *   ('a :> Type)    - inplace coercion constraint not singleton
           *   ('a.opM : S->T) - inplace operator constraint
           *)
          let tpL =
            wordL (prefix_of_static_req typar.StaticReq
                   ^ prefix_of_rigid typar
                   ^ typar.DisplayName)
          let varL = tpL |> stampL typar.Stamp 

          match Zmap.tryfind typar env.SimplifyTypes.inplaceConstraints with
          | Some (typarConstrTyp) ->
              if Zset.mem typar env.SimplifyTypes.singletons then
                leftL "#" $$ auxTyparConstraintTypL env typarConstrTyp
              else
                (varL $$ sepL ":>" $$ auxTyparConstraintTypL env typarConstrTyp) |> wrap
          | _ -> varL

    and auxTypar2L     env typar = auxTyparWrapL env false typar

    and auxTyparAtomL env typar = auxTyparWrapL env true  typar

    and auxTyparConstraintTypL env ty = auxTypeL env ty

    and auxTraitL env (TTrait(tys,nm,memFlags,argtys,rty,_)) =
#if DEBUG
        match !global_g with
        | None -> wordL "<no global g>"
        | Some g -> 
            let rty = GetFSharpViewOfReturnType g rty
            let stat = memFlagsL false memFlags
            let argsL = sepListL (wordL "*") (List.map (auxTypeAtomL env) argtys)
            let resL  = auxTypeL env rty
            let methodTypeL = (argsL $$ wordL "->") ++ resL
            bracketL (stat ++ bracketL (sepListL (wordL "or") (List.map (auxTypeAtomL env) tys)) ++ wordL "member" --- (wordL nm $$ wordL ":" -- methodTypeL))
#else
        wordL "trait"
#endif

    and auxTyparConstraintL env (tp,tpc) = 
        match tpc with
        | TTyparCoercesToType(typarConstrTyp,m) ->
            auxTypar2L env tp $$ wordL ":>" --- auxTyparConstraintTypL env typarConstrTyp
        | TTyparMayResolveMemberConstraint(traitInfo,_) ->
            auxTypar2L env tp $$ wordL ":"  --- auxTraitL env traitInfo
        | TTyparDefaultsToType(_,ty,m) ->
            wordL "default" $$ auxTypar2L env tp $$ wordL ":" $$ auxTypeL env ty
        | TTyparIsEnum(ty,m) ->
            auxTypar2L env tp $$ wordL ":" $$ auxTyparsL env (wordL "enum") true [ty]
        | TTyparIsDelegate(aty,bty,m) ->
            auxTypar2L env tp $$ wordL ":" $$ auxTyparsL env (wordL "delegate") true [aty; bty]
        | TTyparSupportsNull(m) ->
            auxTypar2L env tp $$ wordL ":" $$ wordL "null"
        | TTyparIsNotNullableValueType(m) ->
            auxTypar2L env tp $$ wordL ":" $$ wordL "struct"
        | TTyparIsReferenceType(m) ->
            auxTypar2L env tp $$ wordL ":" $$ wordL "not struct"
        | TTyparSimpleChoice(tys,m) ->
            auxTypar2L env tp $$ wordL ":" $$ bracketL (sepListL (sepL "|") (List.map (auxTypeL env) tys))
        | TTyparRequiresDefaultConstructor(m) ->
            auxTypar2L env tp $$ wordL ":" $$ bracketL (wordL "new : unit -> " $$ (auxTypar2L env tp))

    and auxTyparConstraintsL env x = 
        match x with 
        | []   -> emptyL
        | cxs -> wordL "when" --- aboveListL (List.map (auxTyparConstraintL env) cxs)    

    and TyparL     tp = auxTypar2L     SimplifyTypes.typeSimplificationInfo0 tp 
    and typarAtomL tp = auxTyparAtomL SimplifyTypes.typeSimplificationInfo0 tp

    and typeAtomL tau =
        let tau,cxs = tau,[]
        let env = SimplifyTypes.CollectInfo false [tau] cxs
        match env.SimplifyTypes.postfixConstraints with
        | [] -> auxTypeAtomL env tau
        | _ -> bracketL (auxTypeL env tau --- auxTyparConstraintsL env env.SimplifyTypes.postfixConstraints)
          
    and typeL tau =
        let tau,cxs = tau,[]
        let env = SimplifyTypes.CollectInfo false [tau] cxs
        match env.SimplifyTypes.postfixConstraints with
        | [] -> auxTypeL env tau 
        | _ -> (auxTypeL env tau --- auxTyparConstraintsL env env.SimplifyTypes.postfixConstraints) 

    and TyparDeclL tp =
        let tau,cxs = mk_typar_ty tp,(List.map (fun x -> (tp,x)) tp.Constraints)
        let env = SimplifyTypes.CollectInfo false [tau] cxs
        match env.SimplifyTypes.postfixConstraints with
        | [] -> auxTypeL env tau 
        | _ -> (auxTypeL env tau --- auxTyparConstraintsL env env.SimplifyTypes.postfixConstraints) 
    and typarDeclsL tps = angleBracketListL (List.map TyparDeclL       tps) 

    //--------------------------------------------------------------------------
    // DEBUG layout - types
    //--------------------------------------------------------------------------
      
    let rangeL m = wordL (string_of_range m)

    let instL tyL tys =
        match tys with
        | []  -> emptyL
        | tys -> sepL "@[" $$ commaListL (List.map tyL tys) $$ rightL "]"

    let ValRefL  (vr:ValRef)  = 
        wordL vr.MangledName |> stampL vr.Stamp 

    let attribL (Attrib(_,k,args,props,m)) = 
        leftL "[<" $$ 
        (match k with 
         | ILAttrib (ilmeth) -> wordL ilmeth.Name
         | FSAttrib (vref)   -> ValRefL vref) $$
        rightL ">]"
    
    let attribsL attribs = aboveListL (List.map attribL attribs)

    let arityInfoL (TopValInfo (tpNames,_,_) as tvd) = 
        let ns = tvd.AritiesOfArgs in 
        leftL "arity<" $$ intL tpNames.Length $$ sepL ">[" $$ commaListL (List.map intL ns) $$ rightL "]"


    let valL (vspec:Val) =
        let vsL = wordL (DecompileOpName vspec.MangledName) |> stampL vspec.Stamp
        let vsL = if not !verboseStamps then vsL else vsL $$ rightL (if isSome(vspec.PublicPath) then "+" else "-")
        let vsL = vsL -- attribsL (vspec.Attribs)
        vsL

    let TypeOfvalL      (v:Val) =
        (valL v
          $$ (if  v.MustInline then wordL "inline " else emptyL) 
          $$ (if v.IsMutable then wordL "mutable " else emptyL)
          $$ wordL ":") -- typeL v.Type


    let tslotparamL(TSlotParam(nmOpt, typ, inFlag, outFlag, optFlag,attribs)) =
        (optionL wordL nmOpt) $$ wordL ":" $$ typeL typ $$ (if inFlag then wordL "[in]" else emptyL)  $$ (if outFlag then wordL "[out]" else emptyL)  $$ (if inFlag then wordL "[opt]" else emptyL)
    

    let SlotSigL (TSlotSig(nm,typ,tps1,tps2,pms,rty)) =
#if DEBUG
        match !global_g with
        | None -> wordL "<no global g>"
        | Some g -> 
            let rty = GetFSharpViewOfReturnType g rty
            (wordL "slot" --- (wordL nm) $$ wordL "@" $$ typeL typ) --
              (wordL "LAM" --- spaceListL (List.map TyparL       tps1) $$ rightL ".") ---
              (wordL "LAM" --- spaceListL (List.map TyparL       tps2) $$ rightL ".") ---
              (commaListL (List.map (List.map tslotparamL >> tupleL) pms)) $$ (wordL "-> ") --- (typeL rty) 
#else
        wordL "slotsig"
#endif

    let rec MemberL (membInfo:ValMemberInfo) = 
        (aboveListL [ wordL "vspr_il_name! = " $$ wordL membInfo.CompiledName ;
                      wordL "membInfo-slotsig! = " $$ listL SlotSigL membInfo.ImplementedSlotSigs ]) 
    and vspecAtBindL  v = 
        let vL = valL v  in
        let mutL = (if v.IsMutable then wordL "mutable" ++ vL else vL)
        mutL  --- (aboveListL (List.concat [[wordL ":" $$ typeL v.Type];
                                            (match v.MemberInfo with None -> [] | Some mem_info   -> [wordL "!" $$ MemberL mem_info]);
                                            (match v.TopValInfo with None -> [] | Some arity_info -> [wordL "#" $$ arityInfoL arity_info])]))

    let UnionCaseRefL (ucr:UnionCaseRef) = wordL ucr.CaseName
    let recdFieldRefL (rfref:RecdFieldRef) = wordL rfref.FieldName

    //--------------------------------------------------------------------------
    // DEBUG layout - bind, expr, dtree etc.
    //--------------------------------------------------------------------------

    let identL (id:ident) = wordL id.idText  

    let rec tyconL (tycon:Tycon) =
        if tycon.IsModuleOrNamespace then EntityL tycon else 
        
        let lhsL = wordL (match tycon.TypeOrMeasureKind with KindMeasure -> "[<Measure>] type" | KindType -> "type") $$ wordL tycon.DisplayName $$ typarDeclsL tycon.TyparsNoRange
        let lhsL = lhsL --- attribsL tycon.Attribs
        let memberLs = 
            let tcaug = tycon.TypeContents
            let adhoc = adhoc_of_tycon tycon  |> List.filter (vref_is_dispatch_slot >> not)
            (* Don't print individual methods forming interface implementations - these are currently never exported *)
            let adhoc = adhoc |> List.filter (fun v -> isNil (the(v.MemberInfo)).ImplementedSlotSigs)
            let iimpls = 
                match tycon.TypeReprInfo with 
                | Some (TFsObjModelRepr r) when r.fsobjmodel_kind = TTyconInterface -> []
                | _ -> tcaug.tcaug_implements
            let iimpls = iimpls |> List.filter (fun (ty,compgen,m) -> not compgen)
            (* if TTyconInterface, the iimpls should be printed as inheritted interfaces *)
            if (isNil adhoc && isNil iimpls) 
            then emptyL 
            else 
                let iimplsLs = iimpls |> List.map (fun (ty,compgen,m) -> wordL "interface" --- typeL ty)
                let adhocLs  = adhoc  |> List.map (fun vref -> vspecAtBindL  (deref_val vref))
                (wordL "with" @@-- aboveListL (iimplsLs @ adhocLs)) @@ wordL "end"

        let ucaseArgTypesL argtys = sepListL (wordL "*") (List.map typeL argtys)

        let ucaseL prefixL ucase =
            let nmL = wordL (DemangleOperatorName ucase.ucase_id.idText)
            match ucase.RecdFields |> List.map (fun rfld -> rfld.FormalType) with
            | []     -> (prefixL $$ nmL)
            | argtys -> (prefixL $$ nmL $$ wordL "of") --- ucaseArgTypesL argtys

        let ucasesL ucases =
            let prefixL = if List.length ucases > 1 then wordL "|" else emptyL
            List.map (ucaseL prefixL) ucases
            
        let rfspecL (fld:RecdField) =
            let lhs = wordL fld.Name
            let lhs = if fld.IsMutable then wordL "mutable" --- lhs else lhs
            (lhs $$ rightL ":") --- typeL fld.FormalType

        let tyconReprL (repr,tycon:Tycon) = 
            match repr with 
            | TRecdRepr _ ->
                tycon.TrueFieldsAsList |> List.map (fun fld -> rfspecL fld $$ rightL ";") |> aboveListL  
            | TFsObjModelRepr r -> 
                match r.fsobjmodel_kind with 
                | TTyconDelegate (TSlotSig(nm,typ, _,_,paraml, rty)) ->
                    wordL "delegate ..."
                | _ ->
                    let start = 
                        match r.fsobjmodel_kind with
                        | TTyconClass -> "class" 
                        | TTyconInterface -> "interface" 
                        | TTyconStruct -> "struct" 
                        | TTyconEnum -> "enum" 
                        | _ -> failwith "???"
                    let inherits = 
                       match r.fsobjmodel_kind, tycon.TypeContents.tcaug_super with
                       | TTyconClass,Some super -> [wordL  "inherit" $$ (typeL super)] 
                       | TTyconInterface,_ -> 
                         let tcaug = tycon.TypeContents
                         tcaug.tcaug_implements 
                           |> List.filter (fun (ity,compgen,_) -> not compgen)
                           |> List.map (fun (ity,compgen,_) -> wordL  "inherit" $$ (typeL ity))
                       | _ -> []
                    let vsprs = adhoc_of_tycon tycon |> List.filter vref_is_dispatch_slot |> List.map (fun vref -> vspecAtBindL (deref_val vref))
                    let vals  = tycon.TrueFieldsAsList |> List.map (fun f -> (if f.IsStatic then wordL "static" else emptyL) $$ wordL "val" $$ rfspecL f)
                    let alldecls = inherits @ vsprs @ vals
                    let emptyMeasure = match tycon.TypeOrMeasureKind with KindMeasure -> isNil alldecls | _ -> false
                    if emptyMeasure then emptyL else (wordL start @@-- aboveListL alldecls) @@ wordL "end"
            | TFiniteUnionRepr ucases        -> tycon.UnionCasesAsList |> ucasesL |> aboveListL 
            | TAsmRepr s                      -> wordL "(# ... #)"
            | TMeasureableRepr ty             -> typeL ty
            | TILObjModelRepr (_,_,td) -> wordL td.tdName
        let reprL = 
            match tycon.TypeReprInfo with 
            | Some a -> let rhsL = tyconReprL (a,tycon) @@ memberLs
                        (lhsL $$ wordL "=") @@-- rhsL
            | None   -> match tycon.TypeAbbrev with
                              | None   -> lhsL @@-- memberLs
                              | Some a -> (lhsL $$ wordL "=") --- (typeL a @@ memberLs)
        reprL

        
    //--------------------------------------------------------------------------
    // layout - bind, expr, dtree etc.
    //--------------------------------------------------------------------------

    and BindingL (TBind(v,repr,_)) =
        vspecAtBindL v --- (wordL "=" $$ ExprL repr)

    and ExprL expr = exprWrapL false expr
    and atomL expr = exprWrapL true  expr (* true means bracket if needed to be atomic expr *)

    and letRecL binds bodyL = 
        let eqnsL = 
            binds 
               |>  FlatList.to_list 
               |> List.mapHeadTail (fun bind -> wordL "rec" $$ BindingL bind $$ wordL "in")
                              (fun bind -> wordL "and" $$ BindingL bind $$ wordL "in") 
        (aboveListL eqnsL @@ bodyL) 

    and letL bind bodyL = 
        let eqnL = wordL "let" $$ BindingL bind $$ wordL "in"
        (eqnL @@ bodyL) 
                                                               
    and exprWrapL isAtomic expr =
        let wrap = bracketIfL isAtomic in (* wrap iff require atomic expr *)
        let lay =
            match expr with
            | TExpr_const (c,m,ty)                          -> NicePrint.constL c
            | TExpr_val (v,flags,m)                         -> let xL = valL (deref_val v) in
                                                               let xL =
                                                                   if not !verboseStamps then xL else
                                                                   let tag = 
                                                                     match v with
                                                                     | VRef_private _    -> ""
                                                                     | VRef_nonlocal _ -> "!!" in
                                                                   xL $$ rightL tag in
                                                               let xL =
                                                                   match flags with
                                                                     | CtorValUsedAsSelfInit    -> xL $$ rightL "<selfinit>"
                                                                     | CtorValUsedAsSuperInit -> xL $$ rightL "<superinit>"
                                                                     | VSlotDirectCall -> xL $$ rightL "<vdirect>"
                                                                     | NormalValUse -> xL in
                                                               xL
            | TExpr_seq (x0,x1,flag,_,m)                    -> (let flag = 
                                                                  match flag with
                                                                  | NormalSeq   -> "; (*Seq*)"
                                                                  | ThenDoSeq   -> "; (*ThenDo*)" in
                                                                ((ExprL x0 $$ rightL flag) @@ ExprL x1) |> wrap)
            | TExpr_lambda(lambda_id ,basevopt,argvs,body,m,rty,_)  -> let formalsL = spaceListL (List.map vspecAtBindL argvs) in
                                                                       let bindingL = match basevopt with
                                                                                      | None       -> wordL "lam" $$ formalsL $$ rightL "."
                                                                                      | Some basev -> wordL "lam" $$ (leftL "base=" $$ vspecAtBindL basev) --- formalsL $$ rightL "." in
                                                                       (bindingL ++ ExprL body) |> wrap
            | TExpr_tlambda(lambda_id,argtyvs,body,m,rty,_) -> ((wordL "LAM"    $$ spaceListL (List.map TyparL       argtyvs) $$ rightL ".") ++ ExprL body) |> wrap
            | TExpr_tchoose(argtyvs,body,m)                 -> ((wordL "CHOOSE" $$ spaceListL (List.map TyparL       argtyvs) $$ rightL ".") ++ ExprL body) |> wrap
            | TExpr_app (f,fty,tys,argtys,m)              -> 
                let flayout = atomL f
                appL flayout tys argtys |> wrap
            | TExpr_letrec (binds,body,m,_)                 -> letRecL binds (ExprL body) |> wrap
            | TExpr_let    (bind,body,m,_)                 -> letL bind (ExprL body) |> wrap
            | TExpr_link rX                                 -> (wordL "RecLink" --- atomL (!rX)) |> wrap
            | TExpr_match (spBind,exprm,dtree,targets,m,ty,_)      -> leftL "[" $$ (DecisionTreeL dtree @@ aboveListL (List.mapi targetL (targets |> Array.to_list)) $$ rightL "]")
            | TExpr_op(TOp_ucase (c),tyargs,args,m)                -> ((UnionCaseRefL c (*$$ (instL typeL tyargs)*)) ++ spaceListL (List.map atomL args)) |> wrap
            | TExpr_op(TOp_exnconstr (ecref),_,args,m)                -> wordL ecref.DemangledExceptionName $$ bracketL (commaListL (List.map atomL args))
            | TExpr_op(TOp_tuple,tys,xs,m)                          -> tupleL (List.map ExprL xs)
            | TExpr_op(TOp_recd (ctor,tc),tinst,xs,m)               -> 
                let fields = tc.TrueInstanceFieldsAsList
                let lay fs x = (wordL fs.rfield_id.idText $$ sepL "=") --- (ExprL x)
                let ctorL = 
                    match ctor with
                    | RecdExpr             -> emptyL
                    | RecdExprIsObjInit-> wordL "(new)"
                leftL "{" $$ semiListL (List.map2 lay fields xs) $$ rightL "}" $$ ctorL
            | TExpr_op(TOp_rfield_set (rf),tinst,[rx;x],m)        -> (atomL rx --- wordL ".") $$ (recdFieldRefL rf $$ wordL "<-" --- ExprL x)
            | TExpr_op(TOp_rfield_set (rf),tinst,[x],m)        -> (recdFieldRefL rf $$ wordL "<-" --- ExprL x)
            | TExpr_op(TOp_rfield_get (rf),tinst,[rx],m)          -> (atomL rx $$ rightL ".#" $$ recdFieldRefL rf)
            | TExpr_op(TOp_rfield_get (rf),tinst,[],m)          -> (recdFieldRefL rf)
            | TExpr_op(TOp_field_get_addr (rf),tinst,[rx],m)     -> leftL "&" $$ bracketL (atomL rx $$ rightL ".!" $$ recdFieldRefL rf)
            | TExpr_op(TOp_field_get_addr (rf),tinst,[],m)     -> leftL "&" $$ (recdFieldRefL rf)
            | TExpr_op(TOp_ucase_tag_get (tycr),tinst,[x],m)         -> wordL ("#" ^ tycr.MangledName ^ ".tag") $$ atomL x
            | TExpr_op(TOp_ucase_proof (c),tinst,[x],m)    -> wordL ("#" ^ c.CaseName^ ".cast") $$ atomL x
            | TExpr_op(TOp_ucase_field_get (c,i),tinst,[x],m)        -> wordL ("#" ^ c.CaseName ^ "." ^ string i) --- atomL x
            | TExpr_op(TOp_ucase_field_set (c,i),tinst,[x;y],m)      -> ((atomL x --- (rightL ("#" ^ c.CaseName ^ "." ^ string i))) $$ wordL ":=") --- ExprL y
            | TExpr_op(TOp_tuple_field_get (i),tys,[x],m)             -> wordL ("#" ^ string i) --- atomL x
            | TExpr_op(TOp_coerce,[typ;typ2],[x],m)                   -> atomL x --- (wordL ":>" $$ typeL typ) (* check: or is it typ2? *)
            | TExpr_op(TOp_rethrow,[typ],[],m)                        -> wordL "Rethrow!"
            | TExpr_op(TOp_asm (a,tys),tyargs,args,m)      -> 
                let instrs = a |> List.map (sprintf "%+A" >> wordL) |> spaceListL // %+A has + since instrs are from an "internal" type  
                let instrs = leftL "(#" $$ instrs $$ rightL "#)"
                (appL instrs tyargs args ---
                    wordL ":" $$ spaceListL (List.map typeAtomL tys)) |> wrap
            | TExpr_op(TOp_lval_op (lvop,vr),_,args,m)       -> (lvalopL lvop $$ ValRefL vr --- bracketL (commaListL (List.map atomL args))) |> wrap
            | TExpr_op(TOp_ilcall ((virt,protect,valu,newobj,superInit,prop,isDllImport,boxthis,mref),tinst,minst,tys),tyargs,args,m) ->
                let meth = mref.Name
                wordL "ILCall" $$ aboveListL [wordL "meth  " --- wordL meth;
                                              wordL "tinst " --- listL typeL tinst;
                                              wordL "minst " --- listL typeL minst;
                                              wordL "tyargs" --- listL typeL tyargs;
                                              wordL "args  " --- listL ExprL args] |> wrap
            | TExpr_op(TOp_array,[ty],xs,m)                 -> leftL "[|" $$ commaListL (List.map ExprL xs) $$ rightL "|]"
            | TExpr_op(TOp_while _,[],[x1;x2],m)              -> wordL "while" $$ ExprL x1 $$ wordL "do" $$ ExprL x2 $$ rightL "}"
            | TExpr_op(TOp_for _,[],[x1;x2;x3],m)           -> wordL "for" $$ aboveListL [(ExprL x1 $$ wordL "to" $$ ExprL x2 $$ wordL "do"); ExprL x3 ] $$ rightL "done"
            | TExpr_op(TOp_try_catch _,[_],[x1;x2],m)         -> wordL "try" $$ ExprL x1 $$ wordL "with" $$ ExprL x2 $$ rightL "}"
            | TExpr_op(TOp_try_finally _,[_],[x1;x2],m)       -> wordL "try" $$ ExprL x1 $$ wordL "finally" $$ ExprL x2 $$ rightL "}"
            | TExpr_op(TOp_bytes _,_ ,_ ,m)                 -> wordL "bytes++"       
            | TExpr_op(TOp_uint16s _,_ ,_ ,m)                 -> wordL "uint16++"       
            | TExpr_op(TOp_get_ref_lval,tyargs,args,m)      -> wordL "GetRefLVal..."
            | TExpr_op(TOp_trait_call _,tyargs,args,m)      -> wordL "traitcall..."
            | TExpr_op(TOp_exnconstr_field_get _,tyargs,args,m) -> wordL "TOp_exnconstr_field_get..."
            | TExpr_op(TOp_exnconstr_field_set _,tyargs,args,m) -> wordL "TOp_exnconstr_field_set..."
            | TExpr_op(TOp_try_finally _,tyargs,args,m) -> wordL "TOp_try_finally..."
            | TExpr_op(TOp_try_catch  _,tyargs,args,m) -> wordL "TOp_try_catch..."
            | TExpr_op(_,tys,args,m)                        -> wordL "TExpr_op ..." $$ bracketL (commaListL (List.map atomL args)) (* REVIEW *)
            | TExpr_quote (a,_,m,_)                       -> leftL "<@" $$ atomL a $$ rightL "@>"
            | TExpr_obj (n,typ,basev,ccall,
                           overrides,iimpls,_,_)              -> 
                let ccallL (vu,mr,tinst,args) = appL (wordL "ccall") tinst args
                wordL "OBJ:" $$ aboveListL [typeL typ;
                                            ExprL ccall;
                                            optionL vspecAtBindL basev;
                                            aboveListL (List.map overrideL overrides);
                                            aboveListL (List.map iimplL iimpls)]

            | TExpr_static_optimization (tcs,csx,x,m)       -> 
                let tconstraintL = function TTyconEqualsTycon (s,t) -> (typeL s $$ wordL "=") --- typeL t
                (wordL "opt" @@- (ExprL x)) @@--
                   (wordL "|" $$ ExprL csx --- (wordL "when..." (* --- sepListL (wordL "and") (List.map tconstraintL tcs) *) ))
           
        (* For tracking ranges through expr rewrites *)
        if !layout_ranges 
        then leftL "{" $$ (rangeL (range_of_expr expr) $$ rightL ":") ++ lay $$ rightL "}"
        else lay

    and AssemblyL (TAssembly(implFiles)) = 
        aboveListL (List.map ImplFileL implFiles)
    
    and appL flayout tys args =
        let z = flayout
        let z = z $$ instL typeL tys
        let z = z --- sepL "`" --- (spaceListL (List.map atomL args))
        z
       
    and ImplFileL (TImplFile(qnm,_,e)) =
        aboveListL [(wordL "top implementation ") @@-- mexprL e]

    and mexprL x =
        match x with 
        (* | TMTyped(mtyp,rest,_) -> aboveListL [wordL "CONSTRAIN" @@-- mexprL rest @@- (wordL ":"  @@-  EntityTypeL mtyp)] *)
        | TMTyped(mtyp,defs,m) -> mdefL defs  @@- (wordL ":"  @@-  EntityTypeL mtyp)
    and mdefsL defs = wordL "Module Defs" @@-- aboveListL(List.map mdefL defs)
    and mdefL x = 
        match x with 
        | TMDefRec(tycons ,binds,mbinds,m) ->  aboveListL ((tycons |> List.map tyconL) @ [letRecL binds emptyL] @ List.map mbindL mbinds)
        | TMDefLet(bind,m) -> letL bind emptyL
        | TMDefDo(e,m) -> ExprL e
        | TMDefs(defs) -> mdefsL defs; 
        | TMAbstract(mexpr) -> mexprL mexpr
    and mbindL (TMBind(mspec, rhs)) =
        (wordL (if mspec.IsNamespace then "namespace" else "module") $$ (wordL (demangled_name_of_modul mspec) |> stampL mspec.Stamp)) @@-- mdefL rhs 

    and EntityTypeL (mtyp:ModuleOrNamespaceType) =
        aboveListL [namemapL TypeOfvalL mtyp.AllValuesAndMembers;
                    namemapL tyconL  mtyp.AllEntities;]    

    and EntityL (ms:ModuleOrNamespace) =
        let header = wordL "module" $$ (wordL  (demangled_name_of_modul ms) |> stampL ms.Stamp) $$ wordL ":"
        let footer = wordL "end"
        let body = EntityTypeL ms.ModuleOrNamespaceType
        (header @@-- body) @@ footer

    and ccuL     (ccu:ccu) = EntityL ccu.Contents

    and DecisionTreeL x = 
        match x with 
        | TDBind (bind,body)            -> let bind = wordL "let" $$ BindingL bind $$ wordL "in" in (bind @@ DecisionTreeL body) 
        | TDSuccess (args,n)            -> wordL "Success" $$ leftL "T" $$ intL n $$ tupleL (args |> FlatList.to_list |> List.map ExprL)
        | TDSwitch (test,dcases,dflt,r) -> (wordL "Switch" --- ExprL test) @@--
                                            (aboveListL (List.map dcaseL dcases) @@
                                             match dflt with
                                               None       -> emptyL
                                             | Some dtree -> wordL "dflt:" --- DecisionTreeL dtree)

    and dcaseL (TCase (test,dtree)) = (dtestL test $$ wordL "//") --- DecisionTreeL dtree

    and dtestL x = 
        match x with 
        |  (TTest_unionconstr (c,tinst)) -> wordL "is" $$ UnionCaseRefL c $$ instL typeL tinst
        |  (TTest_array_length (n,ty)) -> wordL "length" $$ intL n $$ typeL ty
        |  (TTest_const       c        ) -> wordL "is" $$ NicePrint.constL c
        |  (TTest_isnull               ) -> wordL "isnull"
        |  (TTest_isinst (_,typ)           ) -> wordL "isinst" $$ typeL typ
        |  (TTest_query (exp,_,_,idx,_)) -> wordL "query" $$ ExprL exp
            
    and targetL i (TTarget (argvs,body,_)) = leftL "T" $$ intL i $$ tupleL (FlatValsL argvs) $$ rightL ":" --- ExprL body
    and FlatValsL vs = vs |> FlatList.to_list |> List.map valL

    and tmethodL (TObjExprMethod(TSlotSig(nm,_,_,_,_,_),tps,vs,e,m)) =
        (wordL "TObjExprMethod" --- (wordL nm) $$ wordL "=") --
          (wordL "METH-LAM" --- angleBracketListL (List.map TyparL       tps) $$ rightL ".") ---
          (wordL "meth-lam" --- tupleL (List.map (List.map vspecAtBindL >> tupleL) vs)  $$ rightL ".") ---
          (atomL e) 
    and overrideL tmeth     = wordL "with" $$ tmethodL tmeth 
    and iimplL (typ,tmeths) = wordL "impl" $$ aboveListL (typeL typ :: List.map tmethodL tmeths) 

    let showType x = Layout.showL (typeL x)
    let showExpr x = Layout.showL (ExprL x)

end


let ValRefL    x = DebugPrint.ValRefL x
let UnionCaseRefL   x = DebugPrint.UnionCaseRefL x
let intL     x = DebugPrint.intL x
let valL   x = DebugPrint.valL x
let TyparL   x = DebugPrint.TyparL x
let TyparDeclL   x = DebugPrint.TyparDeclL x
let TyparsL   x = DebugPrint.typarDeclsL x
let typeL    x = DebugPrint.typeL x
let SlotSigL x = DebugPrint.SlotSigL x
let EntityTypeL   x = DebugPrint.EntityTypeL x
let EntityL   x = DebugPrint.EntityL x
let TypeOfvalL x = DebugPrint.TypeOfvalL x
let MemberL    x = DebugPrint.MemberL x
let BindingL    x = DebugPrint. BindingL x
let ExprL    x = DebugPrint.ExprL x
let DecisionTreeL    x = DebugPrint.DecisionTreeL x
let tyconL    x = DebugPrint.tyconL x
let ImplFileL  x = DebugPrint.ImplFileL x
let AssemblyL  x = DebugPrint.AssemblyL x
let vspecAtBindL x = DebugPrint.vspecAtBindL x
let recdFieldRefL x = DebugPrint.recdFieldRefL x
let traitL x = DebugPrint.auxTraitL SimplifyTypes.typeSimplificationInfo0 x

//--------------------------------------------------------------------------
// 
//--------------------------------------------------------------------------

let mtyp_of_mexpr (TMTyped(mtyp,_,_)) = mtyp

let wrap_modul_as_mtyp_in_namespace x = NewModuleOrNamespaceType Namespace [ x ] []

let wrap_mtyp_as_mspec id cpath mtyp = 
    NewModuleOrNamespace (Some cpath)  taccessPublic  id  emptyXmlDoc  [] (notlazy mtyp)
let wrap_modul_in_namespace id (mspec:ModuleOrNamespace) = 
    wrap_mtyp_as_mspec id (parent_cpath mspec.CompilationPath)  (wrap_modul_as_mtyp_in_namespace mspec)
let wrap_mbind_in_namespace (id :ident) (TMBind(mspec,defs))  = 
    let cpath = mspec.CompilationPath
    let parentModuleSpec = NewModuleOrNamespace (Some (parent_cpath cpath)) taccessPublic id emptyXmlDoc [] (notlazy (empty_mtype Namespace))
    TMBind(parentModuleSpec, TMDefRec ([],FlatList.empty,[TMBind(mspec,defs)],id.idRange))

let SigTypeOfImplFile (TImplFile(_,_,mexpr)) = mtyp_of_mexpr mexpr 

//--------------------------------------------------------------------------
// Data structures representing what gets hidden and what gets remapped (i.e. renamed or alpha-converted)
// when a module signature is applied to a module.
//--------------------------------------------------------------------------

type SignatureRepackageInfo = 
    { mrpiVals  : (ValRef * ValRef) list;
      mrpiTycons: (TyconRef * TyconRef) list  }
    
type SignatureHidingInfo = 
    { mhiTycons     : Zset.t<Tycon>; 
      mhiTyconReprs : Zset.t<Tycon>;  
      mhiVals       : Zset.t<Val>; 
      mhiRecdFields : Zset.t<RecdFieldRef>; 
      mhiUnionCases : Zset.t<UnionCaseRef> }

let union_mhi x y = 
    { mhiTycons       = Zset.union x.mhiTycons y.mhiTycons;
      mhiTyconReprs   = Zset.union x.mhiTyconReprs y.mhiTyconReprs;
      mhiVals         = Zset.union x.mhiVals y.mhiVals;
      mhiRecdFields   = Zset.union x.mhiRecdFields y.mhiRecdFields;
      mhiUnionCases   = Zset.union x.mhiUnionCases y.mhiUnionCases; }

let empty_mhi = 
    { mhiTycons      = Zset.empty tycon_spec_order; 
      mhiTyconReprs  = Zset.empty tycon_spec_order;  
      mhiVals        = Zset.empty val_spec_order; 
      mhiRecdFields  = Zset.empty rfref_order; 
      mhiUnionCases  = Zset.empty ucref_order }

let empty_mrpi = { mrpiVals = []; mrpiTycons= [] } 

let mk_repackage_remapping mrpi = 
    { vspec_remap = vspec_map_of_list (List.map (map1'2 deref_val) mrpi.mrpiVals );
      tpinst = empty_tpinst; 
      tcref_remap = tcref_map_of_list mrpi.mrpiTycons }

//--------------------------------------------------------------------------
// Compute instances of the above for mty -> mty
//--------------------------------------------------------------------------

let acc_entity_remap (msigty:ModuleOrNamespaceType) (tycon:Tycon) (mrpi,mhi) =
    let sigtyconOpt = (NameMap.tryfind tycon.MangledName msigty.AllEntities)
    match sigtyconOpt with 
    | None -> 
        // The type constructor is not present in the signature. Hence it is hidden. 
        let mhi = { mhi with mhiTycons = Zset.add tycon mhi.mhiTycons }
        (mrpi,mhi) 
    | Some sigtycon  -> 
        // The type constructor is in the signature. Hence record the repackage entry 
        let sigtcref = mk_local_tcref sigtycon
        let tcref = mk_local_tcref tycon
        let mrpi = { mrpi with mrpiTycons = ((tcref, sigtcref) :: mrpi.mrpiTycons) }
        (* OK, now look for hidden things *)
        let mhi = 
            if isSome tycon.TypeReprInfo && isNone sigtycon.TypeReprInfo then 
                (* The type representation is absent in the signature, hence it is hidden *)
                { mhi with mhiTyconReprs = Zset.add tycon mhi.mhiTyconReprs } 
            else 
                (* The type representation is present in the signature. *)
                (* Find the fields that have been hidden or which were non-public anyway. *)
                mhi 
                |> Array.fold_right  (fun (rfield:RecdField) mhi ->
                            match sigtycon.GetFieldByName(rfield.Name) with 
                            | Some _  -> 
                                (* The field is in the signature. Hence it is not hidden. *)
                                mhi
                            | _ -> 
                                (* The field is not in the signature. Hence it is regarded as hidden. *)
                                let rfref = rfref_of_rfield tcref rfield
                                { mhi with mhiRecdFields =  Zset.add rfref mhi.mhiRecdFields })
                        tycon.AllFieldsArray
                |> List.foldBack  (fun (ucase:UnionCase) mhi ->
                            match sigtycon.GetUnionCaseByName ucase.DisplayName with 
                            | Some _  -> 
                                (* The constructor is in the signature. Hence it is not hidden. *)
                                mhi
                            | _ -> 
                                (* The constructor is not in the signature. Hence it is regarded as hidden. *)
                                let ucref = ucref_of_ucase tcref ucase
                                { mhi with mhiUnionCases =  Zset.add ucref mhi.mhiUnionCases })
                        (tycon.UnionCasesAsList)  
        (mrpi,mhi) 

let acc_sub_entity_remap (msigty:ModuleOrNamespaceType) (tycon:Tycon) (mrpi,mhi) =
    let sigtyconOpt = (NameMap.tryfind tycon.MangledName msigty.AllEntities)
    match sigtyconOpt with 
    | None -> 
        // The type constructor is not present in the signature. Hence it is hidden. 
        let mhi = { mhi with mhiTycons = Zset.add tycon mhi.mhiTycons }
        (mrpi,mhi) 
    | Some sigtycon  -> 
        // The type constructor is in the signature. Hence record the repackage entry 
        let sigtcref = mk_local_tcref sigtycon
        let tcref = mk_local_tcref tycon
        let mrpi = { mrpi with mrpiTycons = ((tcref, sigtcref) :: mrpi.mrpiTycons) }
        (mrpi,mhi) 

let acc_val_remap (msigty:ModuleOrNamespaceType) (vspec:Val) (mrpi,mhi) =
    let sigValOpt = (NameMap.tryfind vspec.MangledName msigty.AllValuesAndMembers)
    let vref = mk_local_vref vspec
    match sigValOpt with 
    | None -> 
        if verbose then dprintf "acc_val_remap, hide = %s#%d\n" vspec.MangledName vspec.Stamp; (* showL(valL vspec)); *)
        let mhi = { mhi with mhiVals = Zset.add vspec mhi.mhiVals }
        (mrpi,mhi) 
    | Some sigVal  -> 
        (* The value is in the signature. Add the repackage entry. *)
        if !verboseStamps then dprintf "acc_val_remap, remap value %s#%d --> %s#%d\n" vspec.MangledName vspec.Stamp sigVal.MangledName sigVal.Stamp; 
      
        let mrpi = { mrpi with mrpiVals = (vref,mk_local_vref sigVal) :: mrpi.mrpiVals }
        (mrpi,mhi) 

let get_submodsigty nm (msigty:ModuleOrNamespaceType) = 
    match NameMap.tryfind nm msigty.AllEntities with 
    | None -> empty_mtype FSharpModule 
    | Some sigsubmodul -> sigsubmodul.ModuleOrNamespaceType

let rec acc_mty_remap (mty:ModuleOrNamespaceType) (msigty:ModuleOrNamespaceType) acc = 
    let acc = List.foldBack (fun (submodul:ModuleOrNamespace) acc -> acc_mty_remap submodul.ModuleOrNamespaceType (get_submodsigty submodul.MangledName msigty) acc) mty.ModuleAndNamespaceDefinitions acc
    let acc = NameMap.foldRange (acc_entity_remap msigty) mty.AllEntities acc
    let acc = NameMap.foldRange (acc_val_remap msigty) mty.AllValuesAndMembers acc
    acc 

let mk_mtyp_to_mtyp_remapping mty msigty = 
    (* dprintf "mk_mtyp_to_mtyp_remapping,\nmty = %s\nmmsigty=%s\n" (showL(EntityTypeL mty)) (showL(EntityTypeL msigty)); *)
    acc_mty_remap mty msigty (empty_mrpi, empty_mhi) 

//--------------------------------------------------------------------------
// Compute instances of the above for mexpr -> mty
//--------------------------------------------------------------------------

/// At TMDefRec nodes abstract (virtual) vslots are effectively binders, even 
/// though they are tucked away inside the tycon. This helper function extracts the
/// virtual slots to aid with finding this babies.
let vslot_vals_of_tycons (tycons:Tycon list) =  
  tycons 
  |> List.collect (fun tycon -> if tycon.IsFSharpObjectModelTycon then tycon.FSharpObjectModelTypeInfo.fsobjmodel_vslots else []) 
  |> List.map deref_val

let rec acc_mdef_remap msigty x acc = 
    match x with 
    | TMDefRec(tycons,binds,mbinds,m) -> 
         (*  Abstract (virtual) vslots in the tycons at TMDefRec nodes are binders. They also need to be added to the remapping. *)
         let vslotvs = vslot_vals_of_tycons tycons
         List.foldBack (acc_entity_remap msigty) tycons 
            (List.foldBack (acc_val_remap msigty) vslotvs 
                (FlatList.foldBack (var_of_bind >> acc_val_remap msigty) binds 
                    (List.foldBack (acc_mbind_remap msigty) mbinds acc)))
    | TMDefLet(bind,m)  -> acc_val_remap msigty bind.Var acc
    | TMDefDo(e,m)  -> acc
    | TMDefs(defs) -> acc_mdefs_remap msigty defs acc
    | TMAbstract(mexpr) -> acc_mty_remap (mtyp_of_mexpr mexpr) msigty acc
and acc_mbind_remap msigty (TMBind(mspec, def)) acc =
    acc_sub_entity_remap msigty mspec (acc_mdef_remap (get_submodsigty mspec.MangledName msigty) def acc)

and acc_mdefs_remap msigty mdefs acc = List.foldBack (acc_mdef_remap msigty) mdefs acc

let mk_mdef_to_mtyp_remapping mdef msigty =  
    if verbose then dprintf "mk_mdef_to_mtyp_remapping,\nmdefs = %s\nmsigty=%s\n" (showL(DebugPrint.mdefL mdef)) (showL(EntityTypeL msigty));
    acc_mdef_remap msigty mdef (empty_mrpi, empty_mhi) 

//--------------------------------------------------------------------------
// Compute instances of the above for the assembly boundary
//--------------------------------------------------------------------------

let acc_tycon_assembly_boundary_mhi (tycon:Tycon) mhi =
    if not (can_access_from_everywhere tycon.Accessibility) then 
        // The type constructor is not public, hence hidden at the assembly boundary. 
        { mhi with mhiTycons = Zset.add tycon mhi.mhiTycons } 
    elif not (can_access_from_everywhere tycon.TypeReprAccessibility) then 
        { mhi with mhiTyconReprs = Zset.add tycon mhi.mhiTyconReprs } 
    else 
        mhi 
        |> Array.fold_right  
               (fun (rfield:RecdField) mhi ->
                   if not (can_access_from_everywhere rfield.Accessibility) then 
                       let tcref = mk_local_tcref tycon
                       let rfref = rfref_of_rfield tcref rfield
                       { mhi with mhiRecdFields = Zset.add rfref mhi.mhiRecdFields } 
                   else mhi)
               tycon.AllFieldsArray  
        |> List.foldBack  
               (fun (ucase:UnionCase) mhi ->
                   if not (can_access_from_everywhere ucase.Accessibility) then 
                       let tcref = mk_local_tcref tycon
                       let ucref = ucref_of_ucase tcref ucase
                       { mhi with mhiUnionCases = Zset.add ucref mhi.mhiUnionCases } 
                   else mhi)
               (tycon.UnionCasesAsList)   

let acc_val_assembly_boundary_mhi (vspec:Val) mhi =
    if not (can_access_from_everywhere vspec.Accessibility) then 
        // The value is not public, hence hidden at the assembly boundary. 
        { mhi with mhiVals = Zset.add vspec mhi.mhiVals } 
    else 
        mhi

let rec acc_mty_assembly_boundary_mhi mty acc = 
    let acc = List.foldBack (fun (submodul:ModuleOrNamespace) acc -> acc_mty_assembly_boundary_mhi submodul.ModuleOrNamespaceType acc) mty.ModuleAndNamespaceDefinitions acc
    let acc = NameMap.foldRange acc_tycon_assembly_boundary_mhi mty.AllEntities acc
    let acc = NameMap.foldRange acc_val_assembly_boundary_mhi mty.AllValuesAndMembers acc
    acc 

let mk_assembly_boundary_mhi mty = 
(*     dprintf "mk_mtyp_to_mtyp_remapping,\nmty = %s\nmmsigty=%s\n" (showL(EntityTypeL mty)) (showL(EntityTypeL msigty)); *)
    acc_mty_assembly_boundary_mhi mty empty_mhi

(*--------------------------------------------------------------------------
!* Compute instances of the above for mexpr -> mty
 *------------------------------------------------------------------------ *)

let IsHidden setF accessF remapF debugF = 
    let rec check mrmi x = 
        if verbose then dprintf "IsHidden %s ??\n" (showL (debugF x));
            (* Internal/private? *)
        not (can_access_from_everywhere (accessF x)) || 
        (match mrmi with 
         | [] -> false (* Ah! we escaped to freedom! *)
         | (rpi,mhi) :: rest -> 
            (* Explicitly hidden? *)
            Zset.mem x (setF mhi) or 
            (* Recurse... *)
            check rest (remapF rpi x))
    fun mrmi x -> 
        let res = check mrmi x
        if verbose then dprintf "IsHidden, #mrmi = %d, %s = %b\n" mrmi.Length (showL (debugF x)) res;
        res
        
let IsHiddenTycon     mrmi x = IsHidden (fun mhi -> mhi.mhiTycons)     (fun tc -> tc.Accessibility)        (fun rpi x ->  deref_tycon (remap_tcref rpi.tcref_remap (mk_local_tcref x))) tyconL mrmi x 
let IsHiddenTyconRepr mrmi x = IsHidden (fun mhi -> mhi.mhiTyconReprs) (fun v -> v.TypeReprAccessibility)  (fun rpi x ->  deref_tycon (remap_tcref rpi.tcref_remap (mk_local_tcref x))) tyconL mrmi x 
let IsHiddenVal       mrmi x = IsHidden (fun mhi -> mhi.mhiVals)       (fun v -> v.Accessibility)          (fun rpi x ->  deref_val (remap_vref rpi (mk_local_vref x))) valL mrmi x 
let IsHiddenRecdField mrmi x = IsHidden (fun mhi -> mhi.mhiRecdFields) (fun rfref -> rfref.RecdField.Accessibility) (fun rpi x ->  remap_rfref rpi.tcref_remap x) recdFieldRefL mrmi x 


(*--------------------------------------------------------------------------
!* Generic operations on module types
 *------------------------------------------------------------------------ *)

let fold_vals_and_tycons_of_mtyp ft fv = 
    let rec go mty acc = 
        let acc = NameMap.foldRange (fun (mspec:ModuleOrNamespace) acc -> go mspec.ModuleOrNamespaceType acc) mty.ModulesAndNamespacesByDemangledName acc
        let acc = NameMap.foldRange ft mty.AllEntities acc
        let acc = NameMap.foldRange fv mty.AllValuesAndMembers acc
        acc
    go 

let all_vals_of_mtyp m = fold_vals_and_tycons_of_mtyp (fun ft acc -> acc) (fun v acc -> v :: acc) m []
let all_tycons_of_mtyp m = fold_vals_and_tycons_of_mtyp (fun ft acc -> ft :: acc) (fun v acc -> acc) m []

//---------------------------------------------------------------------------
// Free variables in terms.  Are all constructs public accessible?
//---------------------------------------------------------------------------
 
let is_public_vspec (lv:Val)           = (lv.Accessibility = taccessPublic)
let is_public_ucref (ucr:UnionCaseRef) = (ucr.UnionCase.Accessibility = taccessPublic)
let is_public_rfref (rfr:RecdFieldRef) = (rfr.RecdField.Accessibility = taccessPublic)
let is_public_tycon (tcr:Tycon)        = (tcr.Accessibility = taccessPublic)

let freevars_all_public fvs = 
    // Are any non-public items used in the expr (which corresponded to the fvs)?
    // Recall, taccess occurs in:
    //      EntityData     has     entity_tycon_repr_accessibility and entity_accessiblity
    //      UnionCase    has     ucase_access
    //      RecdField      has     rfield_access
    //      ValData       has     val_access
    // The freevars and FreeTyvars collect local constructs.
    // Here, we test that all those constructs are public.
    //
    // CODEREVIEW:
    // What about non-local vals. This fix assumes non-local vals must be public. OK?
    Zset.for_all is_public_vspec fvs.FreeLocals  &&
    Zset.for_all is_public_ucref fvs.FreeUnionCases &&
    Zset.for_all is_public_rfref fvs.FreeRecdFields  &&
    Zset.for_all is_public_tycon fvs.FreeTyvars.FreeTycons

let free_tyvars_all_public tyvars = 
    Zset.for_all is_public_tycon tyvars.FreeTycons

(*---------------------------------------------------------------------------
!* Free variables in terms.  All binders are distinct.
 *------------------------------------------------------------------------- *)

let empty_freevars =  
  { UsesMethodLocalConstructs=false;
    UsesUnboundRethrow=false;
    FreeLocalTyconReprs=empty_free_loctycons; 
    FreeLocals=empty_free_locvals; 
    FreeTyvars=empty_free_tyvars;
    FreeRecdFields = empty_free_rfields;
    FreeUnionCases = empty_free_ucases}

let union_freevars fvs1 fvs2 = 
  if fvs1 == empty_freevars then fvs2 else 
  if fvs2 == empty_freevars then fvs1 else
  { FreeLocals                    = union_free_locvals fvs1.FreeLocals fvs2.FreeLocals;
    FreeTyvars                    = union_free_tyvars fvs1.FreeTyvars fvs2.FreeTyvars;    
    UsesMethodLocalConstructs     = fvs1.UsesMethodLocalConstructs || fvs2.UsesMethodLocalConstructs;
    UsesUnboundRethrow            = fvs1.UsesUnboundRethrow || fvs2.UsesUnboundRethrow;
    FreeLocalTyconReprs           = union_free_loctycons fvs1.FreeLocalTyconReprs fvs2.FreeLocalTyconReprs; 
    FreeRecdFields                = union_free_rfields fvs1.FreeRecdFields fvs2.FreeRecdFields; 
    FreeUnionCases                = union_free_ucases fvs1.FreeUnionCases fvs2.FreeUnionCases; }

let inline tyvars (opts:FreeVarOptions) f v acc =
    if not opts.collectInTypes then acc else
    let ftyvs = acc.FreeTyvars
    let ftyvs' = f v ftyvs
    if ftyvs == ftyvs' then acc else 
    { acc with FreeTyvars = ftyvs' }

#if FREEVARS_IN_TYPES_ANALYSIS
type CheckCachability<'key,'acc>(name,f: FreeVarOptions -> 'key -> 'acc -> bool * 'acc) =
    let dict = System.Collections.Generic.Dictionary<'key,int>(HashIdentity.Reference)
    let idem = System.Collections.Generic.Dictionary<'key,int>(HashIdentity.Reference)
    let closed = System.Collections.Generic.Dictionary<'key,int>(HashIdentity.Reference)
    let mutable saved = 0
    do System.AppDomain.CurrentDomain.ProcessExit.Add(fun _ ->
        let hist = dict |> Seq.group_by (fun (KeyValue(k,v)) -> v) |> Seq.map (fun (n,els) -> (n,Seq.length els)) |> Seq.sort_by (fun (n,_) -> n)
        let total = hist |> Seq.sum_by (fun (nhits,nels) -> nels)
        let totalHits = hist |> Seq.sum_by (fun (nhits,nels) -> nhits * nels)
        printfn "*** %s saved %d hits (%g%%) ***" name saved (float  saved / float (saved + totalHits) * 100.0)
        printfn "*** %s had %d hits total, possible saving %d ***" name totalHits (totalHits - total)
        //for (nhits,nels) in hist do 
        //    printfn "%s, %g%% els for %g%% hits had %d hits" name (float nels / float total * 100.0) (float (nels * nhits) / float totalHits * 100.0) nhits

        let hist = idem |> Seq.group_by (fun (KeyValue(k,v)) -> v) |> Seq.map (fun (n,els) -> (n,Seq.length els)) |> Seq.sort_by (fun (n,_) -> n)
        let total = hist |> Seq.sum_by (fun (nhits,nels) -> nels)
        let totalHits = hist |> Seq.sum_by (fun (nhits,nels) -> nhits * nels)
        printfn "*** %s had %d idempotent hits total, possible saving %d ***" name totalHits (totalHits - total)
        //for (nhits,nels) in hist do 
        //    printfn "%s, %g%% els for %g%% hits had %d idempotent hits" name (float nels / float total * 100.0) (float (nels * nhits) / float totalHits * 100.0) nhits

        let hist = closed |> Seq.group_by (fun (KeyValue(k,v)) -> v) |> Seq.map (fun (n,els) -> (n,Seq.length els)) |> Seq.sort_by (fun (n,_) -> n)
        let total = hist |> Seq.sum_by (fun (nhits,nels) -> nels)
        let totalHits = hist |> Seq.sum_by (fun (nhits,nels) -> nhits * nels)
        printfn "*** %s had %d closed hits total, possible saving %d ***" name totalHits (totalHits - total)
       )
        
    member cache.Apply(opts,key,acc) = 
        if not opts.collectInTypes then 
            saved <- saved + 1
            acc 
        else
            let cls,res = f opts  key acc
            if opts.canCache then 
                if dict.ContainsKey key then 
                    dict.[key] <- dict.[key] + 1
                else
                    dict.[key] <- 1
                if res === acc then
                    if idem.ContainsKey key then 
                        idem.[key] <- idem.[key] + 1
                    else
                        idem.[key] <- 1
                if cls then
                    if closed.ContainsKey key then 
                        closed.[key] <- closed.[key] + 1
                    else
                        closed.[key] <- 1
            res
            

    //member cache.OnExit() = 

let acc_freevars_in_type_cache =  CheckCachability("acc_freevars_in_type", (fun opts ty fvs -> (free_in_type opts ty === empty_free_tyvars), tyvars opts (acc_free_in_type opts) ty fvs))
let acc_freevars_in_val_cache =  CheckCachability("acc_freevars_in_val", (fun opts  v fvs ->  (free_in_val opts v === empty_free_tyvars), tyvars opts (acc_free_in_val opts) v fvs))
let acc_freevars_in_types_cache =  CheckCachability("acc_freevars_in_types", (fun opts  tys fvs -> (free_in_types opts tys === empty_free_tyvars), tyvars opts (acc_free_in_types opts) tys fvs))
let acc_freevars_in_tycon_cache =  CheckCachability("acc_freevars_in_tycon", (fun opts  tys fvs -> false,tyvars opts (acc_free_tycon opts) tys fvs))

let acc_freevars_in_type opts ty fvs = acc_freevars_in_type_cache.Apply(opts,ty,fvs)
let acc_freevars_in_types opts tys fvs = 
    if isNil tys then fvs else acc_freevars_in_types_cache.Apply(opts,tys,fvs)
let acc_freevars_in_tycon opts (tcr:TyconRef) acc = 
    match tcr.IsLocalRef with 
    | true -> acc_freevars_in_tycon_cache.Apply(opts,tcr,acc)
    | _ -> acc
let acc_freevars_in_val opts v fvs = acc_freevars_in_val_cache.Apply(opts,v,fvs)
#else

let acc_freevars_in_type opts ty acc = tyvars opts (acc_free_in_type opts) ty acc
let acc_freevars_in_types opts tys acc = if isNil tys then acc else tyvars opts (acc_free_in_types opts) tys acc
let acc_freevars_in_tycon opts tcref acc = tyvars opts (acc_free_tycon opts) tcref acc
let acc_freevars_in_val opts v acc = tyvars opts (acc_free_in_val opts) v acc
#endif
    
let acc_freevars_in_trait_sln opts tys acc = tyvars opts (acc_free_in_trait_sln opts) tys acc 

let bound_locval opts v fvs =
    if not opts.includeLocals then fvs else
    let fvs = acc_freevars_in_val opts v fvs
    if not (Zset.mem v fvs.FreeLocals) then fvs
    else {fvs with FreeLocals= Zset.remove v fvs.FreeLocals} 

let bound_protect fvs =
    if fvs.UsesMethodLocalConstructs then {fvs with UsesMethodLocalConstructs = false} else fvs

let acc_uses_function_local_constructs flg fvs = 
    if flg && not fvs.UsesMethodLocalConstructs then {fvs with UsesMethodLocalConstructs = true} 
    else fvs 

let bound_rethrow fvs =
    if fvs.UsesUnboundRethrow then {fvs with UsesUnboundRethrow = false} else fvs  

let acc_uses_rethrow flg fvs = 
    if flg && not fvs.UsesUnboundRethrow then {fvs with UsesUnboundRethrow = true} 
    else fvs 

let bound_locvals opts vs fvs = List.foldBack (bound_locval opts) vs fvs

let bind_lhs opts (bind:Binding) fvs = bound_locval opts bind.Var fvs

let FreeVarsCacheCompute opts cache f = if opts.canCache then cached cache f else f()

let rec acc_rhs opts (TBind(_,repr,_)) acc = acc_free_in_expr opts repr acc
          
and acc_free_in_switch_cases opts csl dflt (acc:FreeVars) =
    Option.fold_right (acc_free_in_dtree opts) dflt (List.foldBack (acc_free_in_switch_case opts) csl acc)
 
and acc_free_in_switch_case opts (TCase(discrim,dtree)) acc = 
    acc_free_in_dtree opts dtree (acc_free_in_discrim opts discrim acc)

and acc_free_in_discrim (opts:FreeVarOptions) discrim acc = 
    match discrim with 
    | TTest_unionconstr(ucref,tinst) -> acc_free_ucref opts ucref (acc_freevars_in_types opts tinst acc)
    | TTest_array_length(_,ty) -> acc_freevars_in_type opts ty acc
    | TTest_const _
    | TTest_isnull -> acc
    | TTest_isinst (srcty,tgty) -> acc_freevars_in_type opts srcty (acc_freevars_in_type opts tgty acc)
    | TTest_query (exp, tys, vref, idx, apinfo) -> acc_free_in_expr opts exp (acc_freevars_in_types opts tys (Option.fold_right (acc_free_vref opts) vref acc))

and acc_free_in_dtree opts x (acc : FreeVars) =
    match x with 
    | TDSwitch(e1,csl,dflt,_) -> acc_free_in_expr opts e1 (acc_free_in_switch_cases opts csl dflt acc)
    | TDSuccess (es,_) -> acc_free_in_FlatExprs opts es acc
    | TDBind (bind,body) -> union_freevars (bind_lhs opts bind (acc_rhs opts bind (free_in_dtree opts body))) acc
  
and acc_free_locval opts v fvs =
    if not opts.includeLocals then fvs else
    if Zset.mem v fvs.FreeLocals then fvs 
    else 
        let fvs = acc_freevars_in_val opts v fvs
        {fvs with FreeLocals=Zset.add v fvs.FreeLocals}
  
and acc_loctycon_repr opts b fvs = 
    if not opts.includeLocalTyconReprs then fvs else
    if Zset.mem b fvs.FreeLocalTyconReprs  then fvs
    else { fvs with FreeLocalTyconReprs = Zset.add b fvs.FreeLocalTyconReprs } 

and acc_used_tycon_repr opts (tc:Tycon) fvs = 
    if isSome tc.TypeReprInfo
    then acc_loctycon_repr opts tc fvs
    else fvs

and acc_free_ucref opts cr fvs =   
    if not opts.includeUnionCases then fvs else
    if Zset.mem cr fvs.FreeUnionCases then fvs 
    else
        let fvs = fvs |> acc_used_tycon_repr opts cr.Tycon
        let fvs = fvs |> acc_freevars_in_tycon opts cr.TyconRef
        { fvs with FreeUnionCases = Zset.add cr fvs.FreeUnionCases } 

and acc_free_rfref opts fr fvs = 
    if not opts.includeRecdFields then fvs else
    if Zset.mem fr fvs.FreeRecdFields then fvs 
    else 
        let fvs = fvs |> acc_used_tycon_repr opts fr.Tycon
        let fvs = fvs |> acc_freevars_in_tycon opts fr.TyconRef 
        { fvs with FreeRecdFields = Zset.add fr fvs.FreeRecdFields } 
  
and acc_free_ecref exnc fvs = fvs (* Note: this exnc (TyconRef) should be collected the surround types, e.g. tinst of TExpr_op *)
and acc_free_vref opts (vref:ValRef) fvs = 
    match vref.IsLocalRef with 
    | true -> acc_free_locval opts vref.PrivateTarget fvs
    // non-local values do not contain free variables 
    | _ -> fvs

and acc_free_in_method opts (TObjExprMethod(slotsig,tps,tmvs,e,m)) acc =
    acc_free_in_slotsig opts slotsig
     (union_freevars (tyvars opts (bound_typars opts) tps (List.foldBack (bound_locvals opts) tmvs (free_in_expr opts  e))) acc)

and acc_free_in_methods opts methods acc = 
    List.foldBack (acc_free_in_method opts) methods acc

and acc_free_in_iimpl opts (ty,overrides) acc = 
    acc_freevars_in_type opts ty (acc_free_in_methods opts overrides acc)

and acc_free_in_expr (opts:FreeVarOptions) x acc = 
  match x with
  | TExpr_let(_) -> acc_free_in_expr_linear opts x acc (fun e -> e)
  | _ -> acc_free_in_expr_nonlinear opts x acc
      
and acc_free_in_expr_linear (opts:FreeVarOptions) x acc contf =   
    (* for nested let-bindings, we need to continue after the whole let-binding is processed *)
    match x with
    | TExpr_let (bind,e,_,cache) -> 
        let contf = contf << (fun free ->
          union_freevars (FreeVarsCacheCompute opts cache (fun () -> bind_lhs opts bind (acc_rhs opts bind free))) acc )
        acc_free_in_expr_linear opts e empty_freevars contf
    | _ -> 
      // No longer linear expr
      acc_free_in_expr opts x acc |> contf
    
and acc_free_in_expr_nonlinear opts x acc =
    match x with
    (* BINDING CONSTRUCTS *)
    | TExpr_lambda (_,basev,vs,b,_,rty,cache)  -> 
        union_freevars (SkipCacheCompute cache (fun () -> Option.fold_right (bound_locval opts) basev (bound_locvals opts vs (acc_freevars_in_type opts rty (free_in_expr opts b))))) acc
    | TExpr_tlambda (_,vs,b,_,rty, cache) ->
        union_freevars (SkipCacheCompute cache (fun () -> tyvars opts (bound_typars opts) vs (acc_freevars_in_type opts rty (free_in_expr opts b)))) acc
    | TExpr_tchoose (vs,b,_) ->
        union_freevars (tyvars opts (bound_typars opts) vs (free_in_expr opts b)) acc
    | TExpr_letrec (binds,e,_,cache) ->
        union_freevars (FreeVarsCacheCompute opts cache (fun () -> FlatList.foldBack (bind_lhs opts) binds (FlatList.foldBack (acc_rhs opts) binds (free_in_expr opts e)))) acc
    | TExpr_let (bind,e,_,cache) -> 
        failwith "unreachable - linear expr"
    | TExpr_obj (_,typ,basev,basecall,overrides,iimpls,m,cache)   ->  
        union_freevars (SkipCacheCompute cache (fun () -> 
            bound_protect
              (Option.fold_right (bound_locval opts) basev
                (acc_freevars_in_type opts typ
                   (acc_free_in_expr opts basecall
                      (acc_free_in_methods opts overrides 
                         (List.foldBack (acc_free_in_iimpl opts) iimpls empty_freevars))))))) acc  
    (* NON-BINDING CONSTRUCTS *)                      
    | TExpr_const _ -> acc
    | TExpr_val (lvr,flags,_) ->  
        acc_uses_function_local_constructs (flags <> NormalValUse) (acc_free_vref opts lvr acc)
    | TExpr_quote (ast,{contents=Some(argTypes,argExprs,data)},_,ty) ->  
        acc_free_in_expr opts ast 
            (acc_free_in_exprs opts argExprs
               (acc_freevars_in_types opts argTypes
                  (acc_freevars_in_type opts ty acc))) 
    | TExpr_quote (ast,{contents=None},_,ty) ->  
        acc_free_in_expr opts ast (acc_freevars_in_type opts ty acc)
    | TExpr_app(f0,f0ty,tyargs,args,_) -> 
        acc_freevars_in_type opts f0ty
          (acc_free_in_expr opts f0
             (acc_freevars_in_types opts tyargs
                (acc_free_in_exprs opts args acc)))
    | TExpr_link(eref) -> acc_free_in_expr opts !eref acc
    | TExpr_seq (e1,e2,_,_,_) -> 
        let acc = acc_free_in_expr opts e1 acc
        // tail-call - this is required because we should be able to handle (((e1; e2); e3); e4; .... ))
        acc_free_in_expr opts e2 acc 

    | TExpr_static_optimization (_,e2,e3,m) -> acc_free_in_expr opts e2 (acc_free_in_expr opts e3 acc)
    | TExpr_match (_,_,dtree,targets,_,_,cache) -> 
        union_freevars 
          (SkipCacheCompute cache (fun () -> acc_free_in_targets opts targets empty_freevars)) 
          (acc_free_in_dtree opts dtree acc)
            
    //| TExpr_op (TOp_try_catch,tinst,[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[_],e2,_,_,_); TExpr_lambda(_,_,[_],e3,_,_,_)],_) ->
    | TExpr_op (TOp_try_catch _,tinst,[e1;e2;e3],m) ->
        union_freevars 
          (acc_freevars_in_types opts tinst
            (acc_free_in_exprs opts [e1;e2] acc))
          (bound_rethrow (acc_free_in_expr opts e3 empty_freevars))

    | TExpr_op (op,tinst,args,_) -> 
         let acc = acc_free_in_op opts op acc
         let acc = acc_freevars_in_types opts tinst acc
         acc_free_in_exprs opts args acc

and acc_free_in_op opts op acc =
    match op with

    // Things containing no references
    | TOp_bytes _ 
    | TOp_uint16s _ 
    | TOp_try_catch _ 
    | TOp_try_finally _ 
    | TOp_for _ 
    | TOp_coerce 
    | TOp_get_ref_lval 
    | TOp_tuple 
    | TOp_array 
    | TOp_while _
    | TOp_goto _ | TOp_label _ | TOp_return 
    | TOp_tuple_field_get _ -> acc

    | TOp_ucase_tag_get tr -> acc_used_tycon_repr opts (deref_tycon tr) acc
    
    // Things containing just a union case reference
    | TOp_ucase_proof cr 
    | TOp_ucase cr 
    | TOp_ucase_field_get (cr,_) 
    | TOp_ucase_field_set (cr,_) -> acc_free_ucref opts cr acc

    // Things containing just an exception reference
    | TOp_exnconstr ecr 
    | TOp_exnconstr_field_get (ecr,_) 
    | TOp_exnconstr_field_set (ecr,_)  -> acc_free_ecref ecr acc

    | TOp_rfield_get fr 
    | TOp_field_get_addr fr 
    | TOp_rfield_set fr -> acc_free_rfref opts fr acc

    | TOp_recd (kind,tcr) -> 
        let acc = acc_uses_function_local_constructs (kind = RecdExprIsObjInit) acc
        (acc_used_tycon_repr opts (deref_tycon tcr) (tyvars opts (acc_free_tycon opts) tcr acc)) 

    | TOp_asm (_,tys) ->  acc_freevars_in_types opts tys acc
    | TOp_rethrow -> acc_uses_rethrow true acc

    | TOp_trait_call(TTrait(tys,nm,_,argtys,rty,sln)) -> 
        Option.fold_right (acc_freevars_in_trait_sln opts) sln.Value
           (acc_freevars_in_types opts tys 
             (acc_freevars_in_types opts argtys 
               (Option.fold_right (acc_freevars_in_type opts) rty acc)))

    | TOp_lval_op (_,lvr) -> 
        acc_free_vref opts lvr acc

    | TOp_ilcall ((virt,protect,valu,newobj,superInit,prop,isDllImport,boxthis,mref),enclTypeArgs,methTypeArgs,tys) ->
       acc_freevars_in_types opts enclTypeArgs 
         (acc_freevars_in_types opts methTypeArgs  
           (acc_freevars_in_types opts tys 
             (acc_uses_function_local_constructs protect acc)))

and acc_free_in_targets opts targets acc = 
    Array.fold_right (fun (TTarget(vs,e,_)) acc -> FlatList.foldBack (bound_locval opts) vs (acc_free_in_expr opts e acc)) targets acc

and acc_free_in_FlatExprs opts (es:FlatExprs) acc = FlatList.foldBack (acc_free_in_expr opts) es acc

and acc_free_in_exprs opts (es: Exprs) acc = 
    match es with 
    | [] -> acc 
    | h::t -> 
        let acc = acc_free_in_expr opts h acc
        // tailcall - e.g. Cons(x,Cons(x2,.......Cons(x1000000,Nil))) and [| x1; .... ; x1000000 |]
        acc_free_in_exprs opts t acc

and acc_free_in_slotsig opts (TSlotSig(_,typ,_,_,_,_)) acc = acc_freevars_in_type opts typ acc
 
and free_in_dtree opts e = acc_free_in_dtree opts e empty_freevars
and free_in_expr opts e = acc_free_in_expr opts e empty_freevars

(* Note: these are only an approximation - they are currently used only by the optimizer  *)
let rec acc_free_in_mdef opts x acc = 
    match x with 
    | TMDefRec(tycons,binds,mbinds,m) -> FlatList.foldBack (acc_rhs opts) binds  (List.foldBack (acc_free_in_mbind opts) mbinds acc)
    | TMDefLet(bind,m)  -> acc_rhs opts bind  acc
    | TMDefDo(e,m)  -> acc_free_in_expr opts e acc
    | TMDefs(defs) -> acc_free_in_mdefs opts defs acc
    | TMAbstract(TMTyped(mtyp,mdef,_)) -> acc_free_in_mdef opts mdef acc (* not really right, but sufficient for how this is used in optimization *)
and acc_free_in_mbind opts (TMBind(_, def)) acc = acc_free_in_mdef opts def acc
and acc_free_in_mdefs opts x acc = 
    List.foldBack (acc_free_in_mdef opts) x acc

(* NOTE: we don't yet need to ask for free variables in module expressions *)

let free_in_rhs opts bind = acc_rhs opts bind empty_freevars
let free_in_mdef opts mdef = acc_free_in_mdef opts mdef empty_freevars

(*---------------------------------------------------------------------------
!* Destruct - rarely needed
 *------------------------------------------------------------------------- *)

let rec strip_lambda (e,ty) = 
    match e with 
    | TExpr_lambda (_,basevopt,v,b,_,rty,_) -> 
        if isSome basevopt then errorR(InternalError("skipping basevopt", range_of_expr e));
        let (vs',b',rty') = strip_lambda (b,rty)
        (v :: vs', b', rty') 
    | _ -> ([],e,ty)

let dest_top_lambda (e,ty) =
    let tps,taue,tauty = match e with TExpr_tlambda (_,tps,b,_,rty,_) -> tps,b,rty | _ -> [],e,ty
    let vs,body,rty = strip_lambda (taue,tauty)
    tps,vs,body,rty

// This is used to infer arities of expressions 
// i.e. base the chosen arity on the syntactic expression shape and type of arguments 
let InferArityOfExpr g ty partialArgAttribsL retAttribs e = 
    let rec strip_lambda_notypes e = 
        match e with 
        | TExpr_lambda (_,_,vs,b,_,_,_) -> 
            let (vs',b') = strip_lambda_notypes b
            (vs :: vs', b') 
        | TExpr_tchoose (tps,b,_) -> strip_lambda_notypes b 
        | _ -> ([],e)

    let dest_top_lambda_notypes e =
        let tps,taue = match e with TExpr_tlambda (_,tps,b,_,_,_) -> tps,b | _ -> [],e
        let vs,body = strip_lambda_notypes taue
        tps,vs,body

    let tps,vsl,body = dest_top_lambda_notypes e
    let fun_arity = vsl.Length
    let dtys,rty =  strip_fun_typ_upto g fun_arity (snd (try_dest_forall_typ g ty))
    let partialArgAttribsL = Array.of_list partialArgAttribsL
    assert (List.length vsl = List.length dtys)
        
    let curriedArgInfos =
        (List.zip vsl dtys) |> List.mapi (fun i (vs,ty) -> 
            let partialAttribs = if i < partialArgAttribsL.Length then partialArgAttribsL.[i] else []
            let tys = if (i = 0 && is_unit_typ g ty) then [] else try_dest_tuple_typ g ty
            let ids = 
                if vs.Length = tys.Length then  vs |> List.map (fun v -> Some v.Id)
                else tys |> List.map (fun _ -> None)
            let attribs = 
                if partialAttribs.Length = tys.Length then  partialAttribs 
                else tys |> List.map (fun _ -> [])
            (ids,attribs) ||> List.map2 (fun id attribs -> TopArgInfo(attribs,id)))
    let retInfo = TopArgInfo(retAttribs,None)
    TopValInfo (TopValInfo.InferTyparInfo tps, curriedArgInfos, retInfo)

let InferArityOfExprBinding g (v:Val) e = 
    match v.TopValInfo with
    | Some info -> info
    | None -> InferArityOfExpr g v.Type [] [] e

let chosen_arity_of_bind (TBind(v,repr,_)) = v.TopValInfo

//-------------------------------------------------------------------------
// Check if constraints are satisfied that allow us to use more optimized
// implementations
//------------------------------------------------------------------------- 

let GetUnderlyingTypeOfEnumType g typ = 
   assert(is_enum_typ g typ);
   let tycon = deref_tycon (tcref_of_stripped_typ g typ)
   if is_il_enum_tycon tycon then 
       let tdef = tycon.ILTyconRawMetadata
       let info = info_for_enum (tdef.tdName,tdef.tdFieldDefs)
       let il_ty = typ_of_enum_info info
       match il_ty.TypeSpec.Name with 
       | "System.Byte" -> g.byte_ty
       | "System.SByte" -> g.sbyte_ty
       | "System.Int16" -> g.int16_ty
       | "System.Int32" -> g.int32_ty
       | "System.Int64" -> g.int64_ty
       | "System.UInt16" -> g.uint16_ty
       | "System.UInt32" -> g.uint32_ty
       | "System.UInt64" -> g.uint64_ty
       | "System.Single" -> g.float32_ty
       | "System.Double" -> g.float_ty
       | "System.Char" -> g.char_ty
       | "System.Boolean" -> g.bool_ty
       | _ -> g.int32_ty
   else 
       match tycon.GetFieldByName "value__" with 
       | Some rf -> rf.FormalType
       | None ->  error(InternalError("no 'value__' field found for enumeration type "^tycon.MangledName,tycon.Range))


(* CLEANUP NOTE: this absolutely awful. Get rid of this nonsense mutation. *)
let set_val_has_no_arity (f:Val) = 
    if verbose then  dprintf "clearing topValInfo on %s\n" f.MangledName; 
    f.Data.val_top_repr_info <- None; f


(*--------------------------------------------------------------------------
!* Resolve static optimization constraints
 *------------------------------------------------------------------------ *)

let norm_enum_typ g ty = (if is_enum_typ g ty then GetUnderlyingTypeOfEnumType g ty else ty) 

// -1 equals "no", 0 is "unknown", 1 is "yes"
let decide_static_optimization_constraint g (TTyconEqualsTycon (a,b)) =
     let a = norm_enum_typ g (strip_tpeqns_and_tcabbrevs_and_measureable g a)
     let b = norm_enum_typ g (strip_tpeqns_and_tcabbrevs_and_measureable g b)
     // Both types must be nominal for a definite result
     match try_tcref_of_stripped_typ g a with 
     | Some tcref1 -> 
         match try_tcref_of_stripped_typ g b with 
         | Some tcref2 -> if tcref_eq g tcref1 tcref2 then 1 else -1
         | None -> 0
     | None -> 0
    
let rec DecideStaticOptimizations g cs = 
    match cs with 
    | [] -> 1
    | h::t -> 
        let d = decide_static_optimization_constraint g h 
        if d = -1 then -1 elif d = 1 then DecideStaticOptimizations g t else 0

let mk_static_optimization_expr g (cs,e1,e2,m) = 
    let d = DecideStaticOptimizations g cs in 
    if d = -1 then e2
    elif d = 1 then e1
    else TExpr_static_optimization(cs,e1,e2,m)

//--------------------------------------------------------------------------
// Copy expressions, including new names for locally bound values.
// Used to inline expressions.
//--------------------------------------------------------------------------


type ValCopyFlag = 
    | CloneAll
    | CloneAllAndMarkExprValsAsCompilerGenerated
    | OnlyCloneExprVals
    
let mark_as_compgen compgen d = 
    let compgen = 
        match compgen with 
        | CloneAllAndMarkExprValsAsCompilerGenerated -> true
        | _ -> false
    { d with val_flags= ValFlags.encode_compgen_of_vflags (ValFlags.is_compgen_of_vflags d.val_flags || compgen) d.val_flags }

let bind_locval (v:Val) (v':Val) tmenv = 
    { tmenv with vspec_remap=vspec_map_add v (mk_local_vref v') tmenv.vspec_remap}

let bind_LocalVals vs vs' tmenv = 
    { tmenv with vspec_remap=List.fold_right2 (fun v v' acc -> vspec_map_add v (mk_local_vref v') acc) vs vs' tmenv.vspec_remap}

let bind_LocalFlatVals vs vs' tmenv = 
    { tmenv with vspec_remap=FlatList.foldBack2 (fun v v' acc -> vspec_map_add v (mk_local_vref v') acc) vs vs' tmenv.vspec_remap}

let bind_tycon (tc:Tycon) (tc':Tycon) tyenv = 
    { tyenv with tcref_remap=tcref_map_add (mk_local_tcref tc) (mk_local_tcref tc') tyenv.tcref_remap }

let bind_tycons tcs tcs' tyenv =  
    { tyenv with tcref_remap= List.fold_right2 (fun tc tc' acc -> tcref_map_add (mk_local_tcref tc) (mk_local_tcref tc') acc) tcs tcs' tyenv.tcref_remap }

let remap_attrib_kind  tmenv k =  
    match k with 
    | ILAttrib _ as x -> x
    | FSAttrib vref -> FSAttrib(remap_vref tmenv vref)

let tmenv_copy_remap_and_bind_typars remap_attrib tmenv tps = 
    let tps',tyenvinner = copy_remap_and_bind_typars_full remap_attrib tmenv tps
    let tmenvinner = tyenvinner 
    tps',tmenvinner

let rec remap_attrib g tmenv (Attrib (tcref,kind, args, props,m)) = 
    Attrib(remap_tcref tmenv.tcref_remap tcref,
           remap_attrib_kind tmenv kind, 
           args |> List.map (remap_attrib_expr g tmenv), 
           props |> List.map (fun (AttribNamedArg(nm,ty,flg,expr)) -> AttribNamedArg(nm,remap_type tmenv ty, flg, remap_attrib_expr g tmenv expr)),
           m)

and remap_attrib_expr g tmenv (AttribExpr(e1,e2)) = 
    AttribExpr(remap_expr g CloneAll tmenv e1, remap_expr g CloneAll tmenv e2)
    
and remap_attribs g tmenv xs =  List.map (remap_attrib g tmenv) xs

and remap_possible_forall_typ g tmenv ty = remap_type_full (remap_attrib g tmenv) tmenv ty

and remap_arg_data g tmenv (TopArgInfo(attribs,nm)) =
    TopArgInfo(remap_attribs g tmenv attribs,nm)

and remap_top_val_info g tmenv (TopValInfo(tpNames,arginfosl,retInfo)) =
    TopValInfo(tpNames,List.mapSquared (remap_arg_data g tmenv) arginfosl, remap_arg_data g tmenv retInfo)

and remap_val_data g tmenv d =
    if !verboseStamps then dprintf "remap val data #%d\n" d.val_stamp;
    let ty = d.val_type
    let topValInfo = d.val_top_repr_info
    let ty' = ty |> remap_possible_forall_typ g tmenv
    { d with 
        val_type    = ty';
        val_actual_parent = d.val_actual_parent |> remap_parent_ref tmenv;
        val_top_repr_info = d.val_top_repr_info |> Option.map (remap_top_val_info g tmenv);
        val_member_info   = d.val_member_info |> Option.map (remap_member_info g d.val_defn_range topValInfo ty ty' tmenv);
        val_attribs       = d.val_attribs       |> remap_attribs g tmenv }

and remap_parent_ref tyenv p =
    match p with 
    | ParentNone -> ParentNone
    | Parent x -> Parent (x |> remap_tcref tyenv.tcref_remap)

and map_immediate_vals_and_tycons_of_modtyp ft fv (x:ModuleOrNamespaceType) = 
    let vals = x.AllValuesAndMembers      |> NameMap.map fv
    let tycons = x.AllEntities |> NameMap.map ft
    new ModuleOrNamespaceType(x.ModuleOrNamespaceKind, vals, tycons)
    
and copy_and_remap_val g compgen tmenv (v:Val) = 
    match compgen with 
    | OnlyCloneExprVals when v.IsMemberOrModuleBinding -> v
    | _ ->  v |> NewModifiedVal (remap_val_data g tmenv >> mark_as_compgen compgen)

and fixup_val_attribs g tmenv (v1:Val) (v2:Val) =
    let attrs = v1.Attribs
    if isNil attrs then () else
    v2.Data.val_attribs <- attrs |> remap_attribs g tmenv
    
and copy_and_remap_and_bind_vals g compgen tmenv vs = 
    let vs' = vs |> List.map (copy_and_remap_val g compgen tmenv)
    let tmenvinner = bind_LocalVals vs vs' tmenv
    // Fixup attributes now we've built the full List.map of value renamings (attributes contain value references) *)
    List.iter2 (fixup_val_attribs g tmenvinner) vs vs';    
    vs', tmenvinner

and copy_and_remap_and_bind_FlatVals g compgen tmenv vs = 
    let vs' = vs |> FlatList.map (copy_and_remap_val g compgen tmenv)
    let tmenvinner = bind_LocalFlatVals vs vs' tmenv
    (* Fixup attributes now we've built the full List.map of value renamings (attributes contain value references) *)
    FlatList.iter2 (fixup_val_attribs g tmenvinner) vs vs';    
    vs', tmenvinner

and copy_and_remap_and_bind_val g compgen tmenv v = 
    let v' = v |> copy_and_remap_val g compgen tmenv
    let tmenvinner = bind_locval v v' tmenv
    (* Fixup attributes now we've built the full List.map of value renamings (attributes contain value references) *)
    fixup_val_attribs g tmenvinner v v';    
    v', tmenvinner
    
and remap_expr g (compgen:ValCopyFlag) (tmenv:Remap) x =
    match x with
    // Binding constructs - see also dtrees below 
    | TExpr_lambda (_,basevopt,vs,b,m,rty,_)  -> 
        let basevopt, tmenv =  Option.mapfold (copy_and_remap_and_bind_val g compgen) tmenv basevopt
        let vs,tmenv = copy_and_remap_and_bind_vals g compgen tmenv vs
        let b = remap_expr g compgen tmenv b
        let rty = remap_type tmenv rty
        TExpr_lambda (new_uniq(), basevopt,vs,b,m, rty, SkipFreeVarsCache ())
    | TExpr_tlambda (_,tps,b,m,rty,_) ->
        let tps',tmenvinner = tmenv_copy_remap_and_bind_typars (remap_attrib g tmenv) tmenv tps
        mk_tlambda m tps' (remap_expr g compgen tmenvinner b,remap_type tmenvinner rty)
    | TExpr_tchoose (tps,b,m) ->
        let tps',tmenvinner = tmenv_copy_remap_and_bind_typars (remap_attrib g tmenv) tmenv tps
        TExpr_tchoose(tps',remap_expr g compgen tmenvinner b,m)
    | TExpr_letrec (binds,e,m,_) ->  
        let binds',tmenvinner = copy_and_remap_and_bind_bindings g compgen tmenv binds 
        TExpr_letrec (binds',remap_expr g compgen tmenvinner e,m,NewFreeVarsCache())
    | TExpr_let _ -> remap_linear_expr g compgen tmenv x (fun x -> x)
    | TExpr_match (spBind,exprm,pt,targets,m,ty,_) ->
        prim_mk_match (spBind,exprm,remap_dtree g compgen tmenv pt,
                     targets |> Array.map (fun (TTarget(vs,e,spTarget)) ->
                       let vs',tmenvinner = copy_and_remap_and_bind_FlatVals g compgen tmenv vs 
                       TTarget(vs', remap_expr g compgen tmenvinner e,spTarget)),
                     m,remap_type tmenv ty)
    (* Other constructs *)
    | TExpr_val (vr,isSuperInit,m) -> 
        let vr' = remap_vref tmenv vr 
        if vr == vr' then x 
        else TExpr_val (vr',isSuperInit,m)
    | TExpr_quote (a,{contents=Some(argTypes,argExprs,data)},m,ty) ->  
        TExpr_quote (remap_expr g compgen tmenv a,{contents=Some(remap_typesA tmenv argTypes,remap_exprs g compgen tmenv  argExprs,data)},m,remap_type tmenv ty)
    | TExpr_quote (a,{contents=None},m,ty) ->  
        TExpr_quote (remap_expr g compgen tmenv a,{contents=None},m,remap_type tmenv ty)
    | TExpr_obj (_,typ,basev,basecall,overrides,iimpls,m,_) -> 
        let basev',tmenvinner = Option.mapfold (copy_and_remap_and_bind_val g compgen) tmenv basev 
        mk_obj_expr(remap_type tmenv typ,basev',
                    remap_expr g compgen tmenv basecall,
                    List.map (remap_method g compgen tmenvinner) overrides,
                    List.map (remap_iimpl g compgen tmenvinner) iimpls,m) 
    | TExpr_op(op,tinst,args,m) -> 
        let op' = remap_op tmenv op 
        let tinst' = remap_types tmenv tinst 
        let args' = remap_exprs g compgen tmenv args 
        if op == op' && tinst == tinst' && args == args' then x 
        else TExpr_op (op',tinst',args',m)
    | TExpr_app(e1,e1ty,tyargs,args,m) -> 
        let e1' = remap_expr g compgen tmenv e1 
        let e1ty' = remap_possible_forall_typ g tmenv e1ty 
        let tyargs' = remap_types tmenv tyargs 
        let args' = remap_exprs g compgen tmenv args 
        if e1 == e1' && e1ty == e1ty' && tyargs == tyargs' && args == args' then x 
        else TExpr_app(e1',e1ty',tyargs',args',m)
    | TExpr_link(eref) -> 
        remap_expr g compgen tmenv !eref
    | TExpr_seq (e1,e2,dir,spSeq,m)  -> 
        let e1' = remap_expr g compgen tmenv e1 
        let e2' = remap_expr g compgen tmenv e2 
        if e1 == e1' && e2 == e2' then x 
        else TExpr_seq (e1',e2',dir,spSeq,m)
    | TExpr_static_optimization (cs,e2,e3,m) -> 
       (* note that type instantiation typically resolve the static constraints here *)
       mk_static_optimization_expr g (List.map (remap_constraint tmenv) cs,
                                      remap_expr g compgen tmenv e2,
                                      remap_expr g compgen tmenv e3,m)

    | TExpr_const (c,m,ty) -> 
        let ty' = remap_type tmenv ty 
        if ty == ty' then x else TExpr_const (c,m,ty')

and remap_linear_expr g compgen tmenv e contf =
    match e with 
    | TExpr_let (bind,e,m,_) ->  
      let bind',tmenvinner = copy_and_remap_and_bind_binding g compgen tmenv bind
      remap_linear_expr g compgen tmenvinner e (contf << mk_let_bind m bind')
    | _ -> contf (remap_expr g compgen tmenv e) 
and remap_constraint tyenv c = 
    match c with 
    | TTyconEqualsTycon(ty1,ty2) -> TTyconEqualsTycon(remap_type tyenv ty1, remap_type tyenv ty2)

and remap_op tmenv op = 
    match op with 
    | TOp_recd (ctor,tcr)           -> TOp_recd(ctor,remap_tcref tmenv.tcref_remap tcr)
    | TOp_ucase_tag_get tcr         -> TOp_ucase_tag_get(remap_tcref tmenv.tcref_remap tcr)
    | TOp_ucase(ucref)              -> TOp_ucase(remap_ucref tmenv.tcref_remap ucref)
    | TOp_ucase_proof(ucref)  -> TOp_ucase_proof(remap_ucref tmenv.tcref_remap ucref)
    | TOp_exnconstr(ec)             -> TOp_exnconstr(remap_tcref tmenv.tcref_remap ec)
    | TOp_exnconstr_field_get(ec,n) -> TOp_exnconstr_field_get(remap_tcref tmenv.tcref_remap ec,n)
    | TOp_exnconstr_field_set(ec,n) -> TOp_exnconstr_field_set(remap_tcref tmenv.tcref_remap ec,n)
    | TOp_rfield_set(rfref)         -> TOp_rfield_set(remap_rfref tmenv.tcref_remap rfref)
    | TOp_rfield_get(rfref)         -> TOp_rfield_get(remap_rfref tmenv.tcref_remap rfref)
    | TOp_field_get_addr(rfref)     -> TOp_field_get_addr(remap_rfref tmenv.tcref_remap rfref)
    | TOp_ucase_field_get(ucref,n)  -> TOp_ucase_field_get(remap_ucref tmenv.tcref_remap ucref,n)
    | TOp_ucase_field_set(ucref,n)  -> TOp_ucase_field_set(remap_ucref tmenv.tcref_remap ucref,n)
    | TOp_asm (instrs,tys)          -> TOp_asm (instrs,remap_types tmenv tys)
    | TOp_trait_call(traitInfo)     -> TOp_trait_call(remap_traitA tmenv traitInfo)
    | TOp_lval_op (kind,lvr)        -> TOp_lval_op (kind,remap_vref tmenv lvr)
    | TOp_ilcall ((virt,protect,valu,newobj,superInit,prop,isDllImport,boxthis,mref),enclTypeArgs,methTypeArgs,tys) -> 
       TOp_ilcall ((virt,protect,valu,newobj,superInit,prop,isDllImport,Option.map (fun (a,b) -> (remap_type tmenv a, remap_type tmenv b)) boxthis,mref),
                      remap_types tmenv enclTypeArgs,remap_types tmenv methTypeArgs,remap_types tmenv tys)
    | _ ->  op
    

and remap_exprs g compgen tmenv es = List.mapq (remap_expr g compgen tmenv) es
and remap_FlatExprs g compgen tmenv es = FlatList.mapq (remap_expr g compgen tmenv) es

and remap_dtree g compgen tmenv x =
    match x with 
    | TDSwitch(e1,csl,dflt,m) -> 
        TDSwitch(remap_expr g compgen tmenv e1,
                List.map (fun (TCase(test,y)) -> 
                  let test' = 
                    match test with 
                    | TTest_unionconstr (uc,tinst)   -> TTest_unionconstr(remap_ucref tmenv.tcref_remap uc,remap_types tmenv tinst)
                    | TTest_array_length (n,ty)      -> TTest_array_length(n,remap_type tmenv ty)
                    | TTest_const c                  -> test
                    | TTest_isinst (srcty,tgty)      -> TTest_isinst (remap_type tmenv srcty,remap_type tmenv tgty) 
                    | TTest_isnull                   -> TTest_isnull 
                    | TTest_query _ -> failwith "TTest_query should only be used during pattern match compilation"
                  TCase(test',remap_dtree g compgen tmenv y)) csl, 
                Option.map (remap_dtree g compgen tmenv) dflt,
                m)
    | TDSuccess (es,n) -> 
        TDSuccess (remap_FlatExprs g compgen tmenv es,n)
    | TDBind (bind,rest) -> 
        let bind',tmenvinner = copy_and_remap_and_bind_binding g compgen tmenv bind
        TDBind (bind',remap_dtree g compgen tmenvinner rest)
        
and copy_and_remap_and_bind_binding g compgen tmenv (bind:Binding) =
    let v = bind.Var
    let v', tmenv = copy_and_remap_and_bind_val g compgen tmenv v
    remap_and_rename_bind g compgen tmenv bind v' , tmenv

and copy_and_remap_and_bind_bindings g compgen tmenv binds = 
    let vs', tmenvinner = copy_and_remap_and_bind_FlatVals g compgen tmenv (vars_of_Bindings binds)
    remap_and_rename_binds g compgen tmenvinner binds vs',tmenvinner

and remap_and_rename_binds g compgen tmenvinner binds vs' = FlatList.map2 (remap_and_rename_bind g compgen tmenvinner) binds vs'
and remap_and_rename_bind g compgen tmenvinner (TBind(_,repr,letSeqPtOpt)) v' = TBind(v', remap_expr g compgen tmenvinner repr,letSeqPtOpt)

and remap_method g compgen tmenv (TObjExprMethod(slotsig,tps,vs,e,m))  =
    let slotsig' = remap_slotsig (remap_attrib g tmenv) tmenv slotsig
    let tps',tmenvinner = tmenv_copy_remap_and_bind_typars (remap_attrib g tmenv) tmenv tps
    let vs', tmenvinner2 = List.mapfold (copy_and_remap_and_bind_vals g compgen) tmenvinner vs
    let e' = remap_expr g compgen tmenvinner2 e
    TObjExprMethod(slotsig',tps',vs',e',m)

and remap_iimpl g compgen tmenv (ty,overrides)  =
    (remap_type tmenv ty, List.map (remap_method g compgen tmenv) overrides)

and remap_rfield g tmenv x = 
    { x with 
          rfield_type     = x.rfield_type     |> remap_possible_forall_typ g tmenv;
          rfield_pattribs = x.rfield_pattribs |> remap_attribs g tmenv;
          rfield_fattribs = x.rfield_fattribs |> remap_attribs g tmenv; } 
and remap_rfields g tmenv (x:TyconRecdFields) = x.AllFieldsAsList |> List.map (remap_rfield g tmenv) |> MakeRecdFieldsTable 

and remap_ucase g tmenv x = 
    { x with 
          ucase_rfields = x.ucase_rfields |> remap_rfields g tmenv;
          ucase_rty     = x.ucase_rty     |> remap_type tmenv;
          ucase_attribs = x.ucase_attribs |> remap_attribs g tmenv; } 
and remap_funion g tmenv (x:TyconUnionData) = x.UnionCasesAsList |> List.map (remap_ucase g tmenv)|> MakeUnionCases 

and remap_fsobjmodel g tmenv x = 
    { x with 
          fsobjmodel_kind = 
             (match x.fsobjmodel_kind with 
              | TTyconDelegate slotsig -> TTyconDelegate (remap_slotsig (remap_attrib g tmenv) tmenv slotsig)
              | TTyconClass | TTyconInterface | TTyconStruct | TTyconEnum -> x.fsobjmodel_kind);
          fsobjmodel_vslots  = x.fsobjmodel_vslots  |> List.map (remap_vref tmenv);
          fsobjmodel_rfields = x.fsobjmodel_rfields |> remap_rfields g tmenv } 


and remap_tycon_repr g tmenv repr = 
    match repr with 
    | TFsObjModelRepr    x -> TFsObjModelRepr (remap_fsobjmodel g tmenv x)
    | TRecdRepr          x -> TRecdRepr (remap_rfields g tmenv x)
    | TFiniteUnionRepr   x -> TFiniteUnionRepr (remap_funion g tmenv x)
    | TILObjModelRepr    _ -> failwith "cannot remap IL type definitions"
    | TAsmRepr           x -> repr
    | TMeasureableRepr   x -> TMeasureableRepr (remap_type tmenv x)

and remap_tcaug tmenv x = 
    { x with 
          tcaug_equals           = x.tcaug_equals            |> Option.map (pair_map (remap_vref tmenv) (remap_vref tmenv));
          tcaug_compare       = x.tcaug_compare        |> Option.map (pair_map (remap_vref tmenv) (remap_vref tmenv));
          tcaug_compare_withc = x.tcaug_compare_withc  |> Option.map(remap_vref tmenv);
          tcaug_hash_and_equals_withc  = x.tcaug_hash_and_equals_withc   |> Option.map (pair_map (remap_vref tmenv) (remap_vref tmenv));
          tcaug_adhoc            = x.tcaug_adhoc             |> NameMap.map (List.map (remap_vref tmenv));
          tcaug_super            = x.tcaug_super             |> Option.map (remap_type tmenv);
          tcaug_implements       = x.tcaug_implements        |> List.map (map1'3 (remap_type tmenv)) } 

and remap_tycon_exnc_info g tmenv inp =
    match inp with 
    | TExnAbbrevRepr x -> TExnAbbrevRepr (remap_tcref tmenv.tcref_remap x)
    | TExnFresh      x -> TExnFresh (remap_rfields g tmenv x)
    | TExnAsmRepr  _ | TExnNone -> inp 

and remap_member_info g m topValInfo ty ty' tmenv x = 
    // The slotsig in the ImplementedSlotSigs is w.r.t. the type variables in the value's type. 
    // REVIEW: this is a bit gross. It would be nice if the slotsig was standalone 
    assert (isSome topValInfo);
    let tpsorig,_,_,_ = GetMemberTypeInFSharpForm g x.MemberFlags (the(topValInfo)) ty m
    let tps,_,_,_ = GetMemberTypeInFSharpForm g x.MemberFlags (the(topValInfo)) ty' m
    let renaming,_ = mk_typar_to_typar_renaming tpsorig tps 
    let tmenv = { tmenv with tpinst = tmenv.tpinst @ renaming } 
    { x with 
        ApparentParent    = x.ApparentParent    |>  remap_tcref tmenv.tcref_remap ;
        ImplementedSlotSigs = x.ImplementedSlotSigs |> List.map (remap_slotsig (remap_attrib g tmenv) tmenv); 
    } 

and copy_remap_and_bind_mtyp g compgen tmenv mty = 
    let tycons = all_tycons_of_mtyp mty
    let vs = all_vals_of_mtyp mty
    let _,_,tmenvinner = copy_and_remap_and_bind_tycons_and_vals g compgen tmenv tycons vs
    remap_mtyp g compgen tmenvinner mty, tmenvinner

and remap_mtyp g compgen tmenv mty = 
    map_immediate_vals_and_tycons_of_modtyp (rename_tycon tmenv) (rename_val tmenv) mty 

and rename_tycon tyenv x = 
    let tcref = 
        try 
            let res = tcref_map_find (mk_local_tcref x) tyenv.tcref_remap 
            res
        with Not_found -> 
            errorR(InternalError("couldn't remap internal tycon "^showL(tyconL x),x.Range)); 
            mk_local_tcref x 
    deref_tycon  tcref

and rename_val tmenv x = 
    match vspec_map_tryfind x tmenv.vspec_remap with 
    | Some v -> deref_val v
    | None -> x

and copy_tycon compgen (tycon:Tycon) = 
    match compgen with 
    | OnlyCloneExprVals -> tycon
    | _ ->  NewClonedTycon tycon

/// This operates over a whole nested collection of tycons and vals simultaneously *)
and copy_and_remap_and_bind_tycons_and_vals g compgen tmenv tycons vs = 
    let tycons' = tycons |> List.map (copy_tycon compgen)

    let tmenvinner = bind_tycons tycons tycons' tmenv
    
    (* Values need to be copied and renamed. *)
    let vs',tmenvinner = copy_and_remap_and_bind_vals g compgen tmenvinner vs
    if !verboseStamps then 
        for tycon in tycons do 
            dprintf "copy_and_remap_and_bind_tycons_and_vals: tycon %s#%d\n" tycon.MangledName tycon.Stamp;
        for v in vs do 
            dprintf "copy_and_remap_and_bind_tycons_and_vals: val %s#%d\n" v.MangledName v.Stamp;

    (* "if a type constructor is hidden then all its inner values and inner type constructors must also be hidden" *)
    (* Hence we can just lookup the inner tycon/value mappings in the tables. *)

    let lookup_val (v:Val) = 
        let vref = 
            try  
               let res = vspec_map_find v tmenvinner.vspec_remap
               if !verboseStamps then 
                   dprintf "remaped internal value %s#%d --> %s#%d\n" v.MangledName v.Stamp (deref_val res).MangledName (deref_val res).Stamp;
               res 
            with Not_found -> 
                errorR(InternalError(sprintf "couldn't remap internal value '%s'" v.MangledName,v.Range));
                mk_local_vref v
        deref_val vref
        
    let lookup_tycon tycon = 
        let tcref = 
            try 
                let res = tcref_map_find (mk_local_tcref tycon) tmenvinner.tcref_remap 
                if !verboseStamps then 
                    dprintf "remaped internal tycon %s#%d --> %s#%d\n" tycon.MangledName tycon.Stamp (deref_tycon res).MangledName (deref_tycon res).Stamp;
                res
            with Not_found -> 
                errorR(InternalError("couldn't remap internal tycon "^showL(tyconL tycon),tycon.Range));
                mk_local_tcref tycon
        deref_tycon  tcref
             
    (tycons,tycons') ||> List.iter2 (fun tc tc' -> 
        let tcd = tc.Data
        let tcd' = tc'.Data
        let tps',tmenvinner2 = tmenv_copy_remap_and_bind_typars (remap_attrib g tmenvinner) tmenvinner (tcd.entity_typars.Force(tcd.entity_range))
        tcd'.entity_typars         <- LazyWithContext.NotLazy tps';
        tcd'.entity_attribs        <- tcd.entity_attribs |> remap_attribs g tmenvinner2;
        tcd'.entity_tycon_repr           <- tcd.entity_tycon_repr    |> Option.map (remap_tycon_repr g tmenvinner2);
        tcd'.entity_tycon_abbrev         <- tcd.entity_tycon_abbrev  |> Option.map (remap_type tmenvinner2) ;
        tcd'.entity_tycon_tcaug          <- tcd.entity_tycon_tcaug   |> remap_tcaug tmenvinner2 ;
        tcd'.entity_modul_contents <- notlazy (tcd.entity_modul_contents 
                                              |> Lazy.force 
                                              |> map_immediate_vals_and_tycons_of_modtyp lookup_tycon lookup_val);
        tcd'.entity_exn_info      <- tcd.entity_exn_info      |> remap_tycon_exnc_info g tmenvinner2) ;
    tycons',vs', tmenvinner


and all_tycons_of_mdef mdef =
    match mdef with 
    | TMDefRec(tycons,binds,mbinds,m) -> 
        tycons @ List.collect (fun (TMBind(mspec, def)) -> mspec :: all_tycons_of_mdef def) mbinds
    | TMDefLet _           -> []
    | TMDefDo _            -> []
    | TMDefs(defs)      -> List.collect all_tycons_of_mdef defs
    | TMAbstract(TMTyped(mty,_,_)) -> all_tycons_of_mtyp mty
and all_vals_of_mdef mdef = 
    match mdef with 
    | TMDefRec(tycons,binds,mbinds,m) -> 
       vslot_vals_of_tycons tycons @
       (binds |> vars_of_Bindings |> FlatList.to_list) @ 
       List.collect (fun (TMBind(mspec, def)) -> all_vals_of_mdef def) mbinds
    | TMDefLet(bind,m)            -> [bind.Var]
    | TMDefDo _            -> []
    | TMDefs(defs)      -> List.collect all_vals_of_mdef defs
    | TMAbstract(TMTyped(mty,_,_)) -> all_vals_of_mtyp mty

and remap_and_bind_mexpr g compgen tmenv (TMTyped(mty,mdef,m)) =
    let mdef = copy_and_remap_mdef g compgen tmenv mdef
    let mty,tmenv = copy_remap_and_bind_mtyp g compgen tmenv mty
    TMTyped(mty,mdef,m), tmenv

and remap_mexpr g compgen tmenv (TMTyped(mty,mdef,m)) =
    let mdef = copy_and_remap_mdef g compgen tmenv mdef 
    let mty = remap_mtyp g compgen tmenv mty 
    TMTyped(mty,mdef,m)

and copy_and_remap_mdef g compgen tmenv mdef =
    let tycons = all_tycons_of_mdef mdef 
    let vs = all_vals_of_mdef mdef 
    let _,_,tmenvinner = copy_and_remap_and_bind_tycons_and_vals g compgen tmenv tycons vs
    remap_and_rename_mdef g compgen tmenvinner mdef

and remap_and_rename_mdefs g compgen tmenv x = 
    List.map (remap_and_rename_mdef g compgen tmenv) x 

and remap_and_rename_mdef g compgen tmenv mdef =
    match mdef with 
    | TMDefRec(tycons,binds,mbinds,m) -> 
        (* Abstract (virtual) vslots in the tycons at TMDefRec nodes are binders. They also need to be copied and renamed. *)
        let tycons = tycons |> List.map (rename_tycon tmenv)
        let binds = remap_and_rename_binds g compgen tmenv binds (binds |> FlatList.map (var_of_bind >> rename_val tmenv))
        let mbinds = mbinds |> List.map (remap_and_rename_mbind g compgen tmenv)
        TMDefRec(tycons,binds,mbinds,m)
    | TMDefLet(bind,m)            ->
        let v = bind.Var
        let bind = remap_and_rename_bind g compgen tmenv bind (rename_val tmenv v)
        TMDefLet(bind, m)
    | TMDefDo(e,m)            ->
        let e = remap_expr g compgen tmenv e
        TMDefDo(e, m)
    | TMDefs(defs)      -> 
        let defs = remap_and_rename_mdefs g compgen tmenv defs
        TMDefs(defs)
    | TMAbstract(mexpr) -> 
        let mexpr = remap_mexpr g compgen tmenv mexpr
        TMAbstract(mexpr)

and remap_and_rename_mbind g compgen tmenv (TMBind(mspec, def)) =
    let mspec = rename_tycon tmenv mspec
    let def = remap_and_rename_mdef g compgen tmenv def
    TMBind(mspec, def)

and remap_ImplFile g compgen tmenv mv = 
    map_acc_TImplFile (remap_and_bind_mexpr g compgen) tmenv mv

and remap_assembly g compgen tmenv (TAssembly(mvs)) = 
    let mvs,z = List.mapfold (remap_ImplFile g compgen) tmenv mvs
    TAssembly(mvs),z

let empty_expr_remap = empty_remap

let copy_mtyp     g compgen mtyp = copy_remap_and_bind_mtyp g compgen empty_expr_remap mtyp |> fst
let copy_val      g compgen v    = copy_and_remap_and_bind_val g compgen empty_expr_remap v |> fst
let copy_expr     g compgen e    = remap_expr g compgen empty_expr_remap e    
let copy_ImplFile g compgen e    = remap_ImplFile g compgen empty_expr_remap e |> fst
let copy_assembly g compgen e    = remap_assembly g compgen empty_expr_remap e |> fst

let inst_expr g tpinst e = remap_expr g CloneAll (mk_inst_tyenv tpinst) e

//--------------------------------------------------------------------------
// Replace Marks - adjust debugging marks when a lambda gets
// eliminated (i.e. an expression gets inlined)
//--------------------------------------------------------------------------

let rec RemarkExpr m x =
    match x with
    | TExpr_lambda (uniq,basevopt,vs,b,_,rty,fvs)  -> TExpr_lambda (uniq,basevopt,vs,RemarkExpr m b,m,rty,fvs)  
    | TExpr_tlambda (uniq,tps,b,_,rty,fvs) -> TExpr_tlambda (uniq,tps,RemarkExpr m b,m,rty,fvs)
    | TExpr_tchoose (tps,b,_) -> TExpr_tchoose (tps,RemarkExpr m b,m)
    | TExpr_letrec (binds,e,_,fvs) ->  TExpr_letrec (RemarkBinds m binds,RemarkExpr m e,m,fvs)
    | TExpr_let (bind,e,_,fvs) -> TExpr_let (RemarkBind m bind,RemarkExpr m e,m,fvs)
    | TExpr_match (_,_,pt,targets,_,ty,_) -> prim_mk_match (NoSequencePointAtInvisibleBinding,m,RemarkDecisionTree m pt, Array.map (fun (TTarget(vs,e,_)) ->TTarget(vs, RemarkExpr m e,SuppressSequencePointAtTarget)) targets,m,ty)
    | TExpr_val (x,isSuperInit,_) -> TExpr_val (x,isSuperInit,m)
    | TExpr_quote (a,conv,_,ty) ->  TExpr_quote (RemarkExpr m a,conv,m,ty)
    | TExpr_obj (n,typ,basev,basecall,overrides,iimpls,_,fvs) -> 
        TExpr_obj (n,typ,basev,RemarkExpr m basecall,
                     List.map (RemarkMethod m) overrides,
                     List.map (RemarkInterfaceImpl m) iimpls,m,fvs)
    | TExpr_op (op,tinst,args,_) -> 
        let op = 
            match op with 
            | TOp_try_finally(_,_) -> TOp_try_finally(NoSequencePointAtTry,NoSequencePointAtFinally)
            | TOp_try_catch(_,_) -> TOp_try_catch(NoSequencePointAtTry,NoSequencePointAtWith)
            | _ -> op
            
        TExpr_op (op,tinst,RemarkExprs m args,m)
    | TExpr_link (eref) -> RemarkExpr m !eref
    | TExpr_app(e1,e1ty,tyargs,args,_) -> TExpr_app(RemarkExpr m e1,e1ty,tyargs,RemarkExprs m args,m)
    | TExpr_seq (e1,e2,dir,_,_)  -> TExpr_seq (RemarkExpr m e1,RemarkExpr m e2,dir,SuppressSequencePointOnExprOfSequential,m)
    | TExpr_static_optimization (eqns,e2,e3,_) -> TExpr_static_optimization (eqns,RemarkExpr m e2,RemarkExpr m e3,m)
    | TExpr_const (c,_,ty) -> TExpr_const (c,m,ty)
  
and RemarkMethod m (TObjExprMethod(slotsig,tps,vs,e,_)) = 
  TObjExprMethod(slotsig,tps,vs,RemarkExpr m e,m)
and RemarkInterfaceImpl m (ty,overrides) = 
  (ty, List.map (RemarkMethod m) overrides)
and RemarkExprs m es = es |> List.map (RemarkExpr m) 
and RemarkFlatExprs m es = es |> FlatList.map (RemarkExpr m) 
and RemarkDecisionTree m x =
  match x with 
  | TDSwitch(e1,csl,dflt,_) -> TDSwitch(RemarkExpr m e1, List.map (fun (TCase(test,y)) -> TCase(test,RemarkDecisionTree m y)) csl, Option.map (RemarkDecisionTree m) dflt,m)
  | TDSuccess (es,n) -> TDSuccess (RemarkFlatExprs m es,n)
  | TDBind (bind,rest) -> TDBind(RemarkBind m bind,RemarkDecisionTree m rest)
and RemarkBinds m binds = FlatList.map (RemarkBind m) binds
and RemarkBind m (TBind(v,repr,_)) = TBind(v, RemarkExpr m repr,NoSequencePointAtStickyBinding)


//--------------------------------------------------------------------------
// Reference semantics?
//--------------------------------------------------------------------------

let rfield_alloc_observable (f:RecdField) = not f.IsStatic && f.IsMutable
let ucase_alloc_observable uc = uc.ucase_rfields.rfields_by_index |> Array.exists rfield_alloc_observable
let ucref_alloc_observable (uc:UnionCaseRef) = uc.UnionCase |> ucase_alloc_observable
  
let tycon_alloc_observable (tycon:Tycon) =
  if tycon.IsRecordTycon or tycon.IsStructTycon then 
    tycon.AllFieldsArray |> Array.exists rfield_alloc_observable
  elif tycon.IsUnionTycon then 
    tycon.UnionCasesArray |> Array.exists ucase_alloc_observable
  else false

let tcref_alloc_observable tcr = tycon_alloc_observable (deref_tycon tcr)
  
// Although from the pure F# perspective exception values cannot be changed, the .NET 
// implementation of exception objects attaches a whole bunch of stack information to 
// each raised object.  Hence we treat exception objects as if they have identity 
let ecref_alloc_observable (ecref:TyconRef) = true 

// Some of the implementations of library functions on lists use mutation on the tail 
// of the cons cell. These cells are always private, i.e. not accessible by any other 
// code until the construction of the entire return list has been completed. 
// However, within the implementation code reads of the tail cell must in theory be treated 
// with caution.  Hence we are conservative and within fslib we don't treat list 
// reads as if they were pure. 
let ucref_rfield_mutable g (ucref:UnionCaseRef) n = 
    (g.compilingFslib && tcref_eq g ucref.TyconRef g.list_tcr_canon && n = 1) ||
    (rfield_of_ucref_by_idx ucref n).IsMutable
  
let ecref_rfield_mutable ecref n = 
    if n < 0 || n >= List.length (rfields_of_ecref ecref) then errorR(InternalError(sprintf "ecref_rfield_mutable, exnc = %s, n = %d" ecref.DemangledExceptionName n,ecref.Range));
    (rfield_of_ecref_by_idx ecref n).IsMutable

let use_genuine_field tycon (f:RecdField) = 
    isSome f.LiteralValue || is_enum_tycon tycon || f.rfield_secret || (f.rfield_mutable && not tycon.IsRecordTycon)

let gen_field_name tycon f = 
    if use_genuine_field tycon f then f.rfield_id.idText
    else CompilerGeneratedName f.rfield_id.idText 

//-------------------------------------------------------------------------
// Helpers for building code contained in the initial environment
//------------------------------------------------------------------------- 

let mk_expr_ty g ty =  TType_app(g.expr_tcr,[ty])
let mk_raw_expr_ty g =  TType_app(g.raw_expr_tcr,[])

// flag to specify use of System.Tuple and System.Threading.LazyInit
let mutable use_40_System_Types = true 

let mk_tupled_ty g tys = 
    match tys with 
    | [] -> g.unit_ty 
    | [h] -> h
    | _ -> mk_tuple_ty tys

let mk_tupled_vars_ty g vs = 
    mk_tupled_ty g (types_of_vals vs)

let mk_meth_ty g argtys rty = mk_iterated_fun_ty (List.map (mk_tupled_ty g) argtys) rty 
let mk_nativeptr_typ g ty = TType_app (g.nativeptr_tcr, [ty])
let mk_array_typ g ty = TType_app (g.array_tcr, [ty])
let mk_bytearray_ty g = mk_array_typ g g.byte_ty


//--------------------------------------------------------------------------
// type_of_expr
//--------------------------------------------------------------------------
 
let rec type_of_expr g e = 
    match e with 
    | TExpr_app(f,fty,tyargs,args,m) -> apply_types g fty (tyargs,args)
    | TExpr_obj (_,ty,_,_,_,_,_,_)  
    | TExpr_match (_,_,_,_,_,ty,_) 
    | TExpr_quote(_,_,_,ty) 
    | TExpr_const(_,_,ty)              -> (ty)
    | TExpr_val(vref,isSuperInit,_)  -> vref.Type
    | TExpr_seq(a,b,k,_,_) -> type_of_expr g (match k with NormalSeq  -> b | ThenDoSeq -> a)
    | TExpr_lambda(_,basevopt,vs,_,_,rty,_) -> (mk_tupled_vars_ty g vs --> rty)
    | TExpr_tlambda(_,tyvs,_,_,rty,_) -> (tyvs +-> rty)
    | TExpr_let(_,e,_,_) 
    | TExpr_tchoose(_,e,_)
    | TExpr_link { contents=e}
    | TExpr_static_optimization (_,_,e,_) 
    | TExpr_letrec(_,e,_,_) -> type_of_expr g e
    | TExpr_op(op,tinst,args,m) -> 
        match op with 
        | TOp_coerce -> (match tinst with [to_ty;from_ty] -> to_ty | _ -> failwith "bad TOp_coerce node")
        | (TOp_ilcall (_,_,_,rtys) | TOp_asm(_,rtys)) -> (match rtys with [h] -> h | _ -> g.unit_ty)
        | TOp_ucase uc -> rty_of_uctyp uc tinst
        | TOp_ucase_proof uc -> mk_proven_ucase_typ uc tinst  
        | TOp_recd (_,tcref) -> mk_tyapp_ty tcref tinst
        | TOp_exnconstr uc -> g.exn_ty
        | TOp_bytes _ -> mk_bytearray_ty g
        | TOp_uint16s _ -> mk_array_typ g g.uint16_ty
        | TOp_tuple_field_get(i) -> List.nth tinst i
        | TOp_tuple -> mk_tuple_ty tinst
        | (TOp_for _ | TOp_while _) -> g.unit_ty
        | TOp_array -> (match tinst with [ty] -> mk_array_typ g ty | _ -> failwith "bad TOp_array node")
        | (TOp_try_catch _ | TOp_try_finally _) -> (match tinst with [ty] ->  ty | _ -> failwith "bad TOp_try node")
        | TOp_field_get_addr(fref) -> mk_byref_typ g (actual_rtyp_of_rfref fref tinst)
        | TOp_rfield_get(fref) -> actual_rtyp_of_rfref fref tinst
        | (TOp_rfield_set _ | TOp_ucase_field_set _ | TOp_exnconstr_field_set _ | TOp_lval_op ((LSet | LByrefSet),_)) ->g.unit_ty
        | TOp_ucase_tag_get(cref) -> g.int_ty
        | TOp_ucase_field_get(cref,j) -> typ_of_ucref_rfield_by_idx cref tinst j
        | TOp_exnconstr_field_get(ecref,j) -> typ_of_ecref_rfield ecref j
        | TOp_lval_op (LByrefGet, v) -> dest_byref_typ g v.Type
        | TOp_lval_op (LGetAddr, v) -> mk_byref_typ g v.Type
        | TOp_get_ref_lval -> (match tinst with [ty] -> mk_byref_typ g ty | _ -> failwith "bad TOp_get_ref_lval node")      
        | TOp_trait_call (TTrait(_,_,_,_,ty,_)) -> GetFSharpViewOfReturnType g ty
        | TOp_rethrow -> (match tinst with [rtn_ty] -> rtn_ty | _ -> failwith "bad TOp_rethrow node")
        | TOp_goto _ | TOp_label _ | TOp_return -> 
            //assert false; 
            //errorR(InternalError("unexpected goto/label/return in type_of_expr",m)); 
            // It doesn't matter what type we return here. THis is only used in free variable analysis in the code generator
            g.unit_ty

//--------------------------------------------------------------------------
// Make applications
//---------------------------------------------------------------------------

let prim_mk_app (f,fty) tyargs argsl m = 
  TExpr_app(f,fty,tyargs,argsl,m)

let rec mk_expr_appl_aux g f fty argsl m =
  if verbose then  dprintf "--- mk_expr_appl_aux, fty = %s\n" ((DebugPrint.showType fty));
  match argsl with 
  | arg :: rest ->
      match f with 
      (* Try to zip the term application with others *)
      | TExpr_app(f',fty',tyargs,pargs,m2) 
          (* Only do this when the formal return type of the function type is another function type *)
          when is_fun_typ g (formal_apply_types g fty' (tyargs,pargs)) ->
            if verbose then  dprintf "--- mk_expr_appl_aux, List.zip, fty' = %s\n" ((DebugPrint.showType fty'));
            let pargs' = pargs@[arg]
            let f'' = prim_mk_app (f',fty') tyargs pargs' (union_ranges m2 m)
            let fty'' = apply_types g fty' (tyargs,pargs') 
            if verbose then  dprintf "--- mk_expr_appl_aux, combined, continue, fty'' = %s\n" ((DebugPrint.showType fty''));
            mk_expr_appl_aux g f'' fty'' rest m
      | _ -> 
          if not (is_fun_typ g fty) then error(InternalError("expected a function type",m));
          let _,rfty = dest_fun_typ g fty
          mk_expr_appl_aux g (prim_mk_app (f,fty) [] [arg] m) rfty rest m

  | [] -> (f,fty)

let rec mk_appl_aux g f fty tyargsl argsl m =
  match tyargsl with 
  | tyargs :: rest -> 
      begin match tyargs with 
      | [] -> mk_appl_aux g f fty rest argsl m
      | _ -> 
        let arfty = reduce_forall_typ g fty tyargs
        mk_appl_aux g (prim_mk_app (f,fty) tyargs [] m) arfty rest argsl m
      end
  | [] -> mk_expr_appl_aux g f fty argsl m
      
let mk_appl g ((f,fty),tyargsl,argl,m) = fst (mk_appl_aux g f fty tyargsl argl m)
let mk_tyapp m (f,fty) tyargs = match tyargs with [] -> f | _ -> prim_mk_app (f,fty) tyargs [] m 

let mk_val_set m v e = TExpr_op(TOp_lval_op (LSet, v), [], [e], m)             (*   localv <- e      *)
let mk_lval_set m v e = TExpr_op(TOp_lval_op (LByrefSet, v), [], [e], m)       (*  *localv_ptr = e   *)
let mk_lval_get m v = TExpr_op(TOp_lval_op (LByrefGet, v), [], [], m)          (* *localv_ptr        *)
let mk_val_addr m v = TExpr_op(TOp_lval_op (LGetAddr, v), [], [], m)           (* &localv            *)

(*--------------------------------------------------------------------------
!* Decision tree reduction
 *------------------------------------------------------------------------ *)

let rec acc_targets_of_dtree tree acc =
  match tree with 
  | TDSwitch (_,edges,dflt,_) -> List.foldBack (dest_of_case >> acc_targets_of_dtree) edges (Option.fold_right acc_targets_of_dtree dflt acc)
  | TDSuccess (_,i) -> ListSet.insert (=) i acc
  | TDBind (_,rest) -> acc_targets_of_dtree rest acc

let rec map_acc_tips_of_dtree f tree =
    match tree with 
    | TDSwitch (e,edges,dflt,m) -> TDSwitch (e,List.map (map_acc_tips_of_edge f) edges,Option.map (map_acc_tips_of_dtree f) dflt,m)
    | TDSuccess (es,i) -> f es i  
    | TDBind (bind,rest) -> TDBind(bind,map_acc_tips_of_dtree f rest)
and map_acc_tips_of_edge f (TCase(x,t)) = 
    TCase(x,map_acc_tips_of_dtree f t)

let map_targets_of_dtree f tree = map_acc_tips_of_dtree (fun es i -> TDSuccess(es, f i)) tree

(* Dead target elimination *)
let eliminate_dead_targets_from_match tree targets =
    let used = acc_targets_of_dtree tree [] |> Array.of_list
    if used.Length < Array.length targets then
        Array.sortInPlace used;
        let nused = Array.length used
        let ntargets = Array.length targets
        let tree' = 
            let remap = Array.create ntargets (-1)
            Array.iteri (fun i tgn -> remap.[tgn] <- i) used;
            map_targets_of_dtree (fun tgn -> if remap.[tgn] = -1 then failwith "eliminate_dead_targets_from_match: failure while eliminating unused targets"; 
                                             remap.[tgn]) tree
        let targets' = Array.map (Array.get targets) used
        tree',targets'
    else 
        tree,targets
    


let rec target_of_success_dtree tree =
    match tree with 
    | TDSwitch _ -> None
    | TDSuccess (_,i) -> Some i
    | TDBind(b,t) -> target_of_success_dtree t

/// Check a decision tree only has bindings that immediately cover a 'Success'
let rec dtree_has_non_trivial_bindings tree =
    match tree with 
    | TDSwitch (_,edges,dflt,_) -> List.exists (dest_of_case >> dtree_has_non_trivial_bindings) edges || (Option.exists dtree_has_non_trivial_bindings dflt)
    | TDSuccess _ -> false
    | TDBind (_,t) -> isNone (target_of_success_dtree t)

// If a target has assignments and can only be reached through one 
// branch (i.e. is "linear"), then transfer the assignments to the r.h.s. to be a "let". 
let fold_linear_binding_targets_of_match tree targets =

    // Don't do this when there are any bindings in the tree except where those bindings immediately cover a success node
    // since the variables would be extruded from their scope. 
    if dtree_has_non_trivial_bindings tree then 
        tree,targets 

    else

        // Build a map showing how each target might be reached
        let rec acc_tips_of_dtree accBinds tree  acc =
            match tree with 
            | TDSwitch (_,edges,dflt,_) -> 
                assert (isNil accBinds)
                List.foldBack (acc_tips_of_edge accBinds) edges (Option.fold_right (acc_tips_of_dtree accBinds) dflt acc)
            | TDSuccess (es,i) -> 
                Map.add i ((List.rev accBinds,es) :: Map.tryFindMulti i acc) acc
            | TDBind (bind,rest) -> 
                acc_tips_of_dtree (bind::accBinds) rest acc

        and acc_tips_of_edge accBinds (TCase(_,x)) acc = acc_tips_of_dtree accBinds x acc

        // Compute the targets that can only be reached one way
        let linearTips = 
            acc_tips_of_dtree [] tree Map.empty 
            |> Map.filter (fun k v -> match v with [_] -> true | _ -> false)

        if linearTips.IsEmpty then 

            tree,targets

        else
            
            /// rebuild the decision tree, replacing 'bind-then-success' decision trees by TDSuccess nodes that just go to the target
            let rec rebuildDecisionTree tree =
                
                // Check if this is a bind-then-success tree
                match target_of_success_dtree tree with
                | Some i when linearTips.ContainsKey i -> TDSuccess(FlatList.empty,i)
                | _ -> 
                    match tree with 
                    | TDSwitch (e,edges,dflt,m) -> TDSwitch (e,List.map rebuildDecisionTreeEdge edges,Option.map rebuildDecisionTree dflt,m)
                    | TDSuccess _ -> tree
                    | TDBind _ -> tree

            and rebuildDecisionTreeEdge (TCase(x,t)) =  
                TCase(x,rebuildDecisionTree t)

            let tree' =  rebuildDecisionTree tree

            /// rebuild the targets , replacing linear targets by ones that include all the 'let' bindings from the source
            let targets' = 
                targets |> Array.mapi (fun i (TTarget(vs,e,spTarget) as tg) -> 
                    match Map.tryfind i linearTips with 
                    | Some ((binds,es) :: _) -> 
                        let m = (range_of_expr e)
                        TTarget(FlatList.empty,mk_lets_bind m binds (mk_invisible_lets_from_Bindings m vs es e),spTarget)
                    | _ -> tg )
     
            tree',targets'

// Simplify a little as we go, including dead target elimination 
let rec simplify_trivial_match spBind exprm matchm ty tree targets  = 
    match tree with 
    | TDSuccess(es,n) -> 
        if n >= Array.length targets then failwith "simplify_trivial_match: target out of range";
        let (TTarget(vs,rhs,spTarget)) = targets.[n]
        if vs.Length <> es.Length then failwith ("simplify_trivial_match: invalid argument, n = "^string n^", List.length targets = "^string (Array.length targets));
        mk_invisible_lets_from_Bindings (range_of_expr rhs) vs es rhs
    | _ -> 
        prim_mk_match (spBind,exprm,tree,targets,matchm,ty)
 
(* Simplify a little as we go, including dead target elimination *)
let mk_and_optimize_match spBind exprm matchm ty tree targets  = 
    let targets = Array.of_list targets
    match tree with 
    | TDSuccess _ -> 
        simplify_trivial_match spBind exprm matchm ty tree targets
    | _ -> 
        let tree,targets = eliminate_dead_targets_from_match tree targets
        let tree,targets = fold_linear_binding_targets_of_match tree targets
        simplify_trivial_match spBind exprm matchm ty tree targets


(*-------------------------------------------------------------------------
!* mk_expra_of_expr
 *------------------------------------------------------------------------- *)

type mutates = DefinitelyMutates | PossiblyMutates | NeverMutates
exception DefensiveCopyWarning of string * range 

let type_immutable g ty =
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> 
      not (tcref_alloc_observable tcref) ||
      tcref_eq g tcref g.decimal_tcr ||
      tcref_eq g tcref g.date_tcr

let MustTakeAddressOfVal (v:ValRef) = v.IsMutable 

let CanTakeAddressOfVal g (v:ValRef) mut =
    // We can take the address of values of struct type if all instances of the type are
    // known to be immutable. 
    // We only do this for true locals because we can't necessarily take adddresses
    // across assemblies.
    // Note: type_immutable should imply PossiblyMutates or NeverMutates
    not (mut = DefinitelyMutates) &&  
    not v.IsMemberOrModuleBinding &&
    type_immutable g v.Type

let MustTakeAddressOfRecdField (rfref: RecdFieldRef) = rfref.RecdField.IsMutable
let CanTakeAddressOfRecdField g (rfref: RecdFieldRef) mut tinst =
       (not (mut = DefinitelyMutates) && 
        // We only do this if the field is defined in this assembly because we can't take adddresses across assemblies fro immutable fields
        tcref_in_this_assembly g.compilingFslib rfref.TyconRef &&
        type_immutable g (actual_rtyp_of_rfref rfref tinst))


let rec mk_expra_of_expr g valu mut e m =
    if not valu then (fun x -> x),e else
    match e with 
    (* LVALUE: "x" where "x" is byref *)
    | TExpr_op(TOp_lval_op (LByrefGet, v), _,[], m) -> 
        (fun x -> x), expr_for_vref m v
    (* LVALUE: "x" where "x" is mutable local *)
    | TExpr_val(v, _,m) when MustTakeAddressOfVal v || CanTakeAddressOfVal g v mut ->
        (fun x -> x), mk_val_addr m v
    (* LVALUE: "x" where "e.x" is mutable record field. "e" may be an lvalue *)
    | TExpr_op(TOp_rfield_get rfref, tinst,[e],m) when MustTakeAddressOfRecdField rfref || CanTakeAddressOfRecdField g rfref mut tinst ->
        let exprty = type_of_expr g e
        let wrap,expra = mk_expra_of_expr g (is_struct_typ g exprty) mut e m
        wrap, mk_recd_field_get_addr_via_expra(expra,rfref,tinst,m)

    (* LVALUE: "x" where "e.x" is a .NET static field. *)
    | TExpr_op(TOp_asm ([IL.I_ldsfld(vol,fspec)],[ty2]), tinst,[],m) -> 
        (fun x -> x),TExpr_op(TOp_asm ([IL.I_ldsflda(fspec)],[mk_byref_typ g ty2]), tinst,[],m)

    (* LVALUE: "x" where "e.x" is a .NET instance field. "e" may be an lvalue *)
    | TExpr_op(TOp_asm ([IL.I_ldfld(align,vol,fspec)],[ty2]), tinst,[e],m) 
       -> 
        let exprty = type_of_expr g e
        let wrap,expra = mk_expra_of_expr g (is_struct_typ g exprty) mut e m
        wrap,TExpr_op(TOp_asm ([IL.I_ldflda(fspec)],[mk_byref_typ g ty2]), tinst,[expra],m)

    (* LVALUE: "x" where "x" is mutable static field. *)
    | TExpr_op(TOp_rfield_get rfref, tinst,[],m) when MustTakeAddressOfRecdField rfref || CanTakeAddressOfRecdField g rfref mut tinst ->
        (fun x -> x), mk_static_rfield_get_addr(rfref,tinst,m)

    // LVALUE:  "e.[n]" where e is an array of structs 
    | TExpr_app(TExpr_val(vf,_,_),_,[elemTy],[aexpr;nexpr],_) 
         when (g.vref_eq vf g.arr1_lookup_vref) -> 
        
        let shape = Rank1ArrayShape
        (fun x -> x), TExpr_op(TOp_asm ([IL.I_ldelema(NormalAddress,shape,IL.mk_tyvar_ty 0us)],[mk_byref_typ g elemTy]), [elemTy],[aexpr;nexpr],m)

    // LVALUE:  "e.[n1,n2]", "e.[n1,n2,n3]", "e.[n1,n2,n3,n4]" where e is an array of structs 
    | TExpr_app(TExpr_val(vf,_,_),_,[elemTy],[aexpr;TExpr_op(TOp_tuple,_,args,_)],_) 
         when (g.vref_eq vf g.arr2_lookup_vref || g.vref_eq vf g.arr3_lookup_vref || g.vref_eq vf g.arr4_lookup_vref) -> 
        
        let shape = ILArrayShape(Array.to_list (Array.create args.Length (None,None))) 

        (fun x -> x), TExpr_op(TOp_asm ([IL.I_ldelema(NormalAddress,shape,IL.mk_tyvar_ty 0us)],[mk_byref_typ g elemTy]), [elemTy],(aexpr::args),m)

    | TExpr_val(v, _,m) when mut = DefinitelyMutates
       -> 
        if is_byref_typ g v.Type then error(Error("Unexpected use of a byref-typed variable",m));
        error(Error("A value must be local and mutable in order to mutate the contents of a value type, e.g. 'let mutable x = ...'",m));
         
    | _ -> 
        begin match mut with 
        | NeverMutates -> ()
        | DefinitelyMutates -> 
          errorR(Error("Invalid mutation of a constant expression. Consider copying the expression to a mutable local, e.g. 'let mutable x = ...'",m));
        | PossiblyMutates -> 
          warning(DefensiveCopyWarning("The value has been copied to ensure the original is not mutated by this operation",m));
        end;
        let tmp,tmpe = mk_mut_compgen_local m "copyOfStruct" (type_of_expr g e)
        (fun rest -> mk_compgen_let m tmp e rest), (mk_val_addr m (mk_local_vref tmp))        

let mk_recd_field_get g (e,fref:RecdFieldRef,tinst,finst,m) = 
    let ftyp = actual_rtyp_of_rfref fref tinst
    let wrap,e' = mk_expra_of_expr g fref.Tycon.IsStructTycon NeverMutates e m 
    wrap (mk_tyapp m (mk_recd_field_get_via_expra(e',fref,tinst,m), ftyp) finst)

let mk_recd_field_set g (e,fref:RecdFieldRef,tinst,e2,m) = 
    let wrap,e' = mk_expra_of_expr g fref.Tycon.IsStructTycon DefinitelyMutates e m 
    wrap (mk_recd_field_set_via_expra(e',fref,tinst,e2,m))

(*---------------------------------------------------------------------------
!* Compute fixups for letrec's.
 *
 * Generate an assignment expression that will fixup the recursion 
 * amongst the vals on the r.h.s. of a letrec.  The returned expressions 
 * include disorderly constructs such as expressions/statements 
 * to set closure environments and non-mutable fields. These are only ever 
 * generated by the backend code-generator when processing a "letrec"
 * construct.
 *
 * [self] is the top level value that is being fixed
 * [expr_to_fix] is the r.h.s. expression
 * [rvs] is the set of recursive vals being bound. 
 * [acc] accumulates the expression right-to-left. 
 *
 * Traversal of the r.h.s. term must happen back-to-front to get the
 * uniq's for the lambdas correct in the very rare case where the same lambda
 * somehow appears twice on the right.
 *------------------------------------------------------------------------- *)

let rec iter_letrec_fixups g (selfv : Val option) rvs ((access : expr),set) expr_to_fix  = 
  let expr_to_fix =  strip_expr expr_to_fix
  match expr_to_fix with 
  | TExpr_const _ -> ()
  | TExpr_op(TOp_tuple,argtys,args,m) ->
      args |> List.iteri (fun n -> 
          iter_letrec_fixups g None rvs 
            (mk_tuple_field_get(access,argtys,n,m), 
            (fun e -> 
              (* NICE: it would be better to do this check in the type checker *)
              errorR(Error("Recursively defined values may not appear directly as part of the construction of a tuple value within a recursive binding.",m));
              e)))

  | TExpr_op(TOp_ucase (c),tinst,args,m) ->
      args |> List.iteri (fun n -> 
          iter_letrec_fixups g None rvs 
            (mk_ucase_field_get_unproven(access,c,tinst,n,m), 
             (fun e -> 
               (* NICE: it would be better to do this check in the type checker *)
               let tcref = c.TyconRef
               errorR(Error("Recursive values may not appear directly as a construction of the type '"^tcref.MangledName^"' within a recursive binding. This feature has been removed from the F# language. Consider using a record instead",m));
               mk_ucase_field_set(access,c,tinst,n,e,m))))

  | TExpr_op(TOp_recd (_,tcref),tinst,args,m) -> 
      (instance_rfrefs_of_tcref tcref, args) ||> List.iter2 (fun fref arg -> 
          let fspec = fref.RecdField
          iter_letrec_fixups g None rvs 
            (mk_recd_field_get_via_expra(access,fref,tinst,m), 
             (fun e -> 
               (* NICE: it would be better to do this check in the type checker *)
               if not fspec.IsMutable && not (tcref_in_this_assembly g.compilingFslib tcref) then
                 errorR(Error("Recursive values may not be directly assigned to the non-mutable field '"^fspec.rfield_id.idText^"' of the type '"^tcref.MangledName^"' within a recursive binding. Consider using a mutable field instead",m));
               mk_recd_field_set g (access,fref,tinst,e,m))) arg )
  | TExpr_val (_,_,m) 
  | TExpr_lambda (_,_,_,_,m,_,_)  
  | TExpr_obj (_,_,_,_,_,_,m,_)  
  | TExpr_tchoose (_,_,m)  
  | TExpr_tlambda (_,_,_,m,_,_)  -> 
      rvs selfv access set expr_to_fix
  | e -> ()




(*--------------------------------------------------------------------------
!* computations on constraints
 *------------------------------------------------------------------------*)
 
let JoinTyparStaticReq r1 r2 = 
  match r1,r2 with
  | NoStaticReq,r | r,NoStaticReq -> r 
  | HeadTypeStaticReq,r | r,HeadTypeStaticReq -> r
  


(*-------------------------------------------------------------------------
!* ExprFolder - fold steps
 *-------------------------------------------------------------------------*)

type ExprFolder<'a> = {exprIntercept    : ('a -> expr -> 'a) -> 'a -> expr  -> 'a option;   (* intercept? *)
                      (* hook. this bool is 'bound in dtree' *)
                      valBindingSiteIntercept          : 'a -> bool * Val  -> 'a;                     
                      (* hook.  these values are always bound to these expressions. bool is 'recursively' *)
                      nonRecBindingsIntercept         : 'a -> Binding -> 'a;         
                      recBindingsIntercept         : 'a -> Bindings -> 'a;         
                      dtreeAcc         : 'a -> DecisionTree -> 'a;                     (* hook *)
                      targetIntercept  : ('a -> expr -> 'a) -> 'a -> DecisionTreeTarget  -> 'a option; (* intercept? *)
                      tmethodIntercept : ('a -> expr -> 'a) -> 'a -> ObjExprMethod -> 'a option; (* intercept? *)
                     }

let ExprFolder0 =
  { exprIntercept    = (fun exprF z x -> None);
    valBindingSiteIntercept          = (fun z b  -> z);
    nonRecBindingsIntercept         = (fun z bs -> z);
    recBindingsIntercept         = (fun z bs -> z);
    dtreeAcc         = (fun z dt -> z);
    targetIntercept  = (fun exprF z x -> None);
    tmethodIntercept = (fun exprF z x -> None);
  }


(*-------------------------------------------------------------------------
!* FoldExpr
 *-------------------------------------------------------------------------*)

let mkFolders (folders : _ ExprFolder) =
  (******
   * Adapted from usage info folding.
   * Collecting from exprs at moment.
   * To collect ids etc some additional folding needed, over formals etc.
   ******)
  let {exprIntercept    = exprIntercept; 
       valBindingSiteIntercept          = valBindingSiteIntercept;
       nonRecBindingsIntercept         = nonRecBindingsIntercept;
       recBindingsIntercept         = recBindingsIntercept;
       dtreeAcc         = dtreeAcc;
       targetIntercept  = targetIntercept;
       tmethodIntercept = tmethodIntercept} = folders
  let rec exprsF z xs = List.fold exprF z xs
  and flatExprsF z xs = FlatList.fold exprF z xs
  and exprF z x =
    match exprIntercept exprF z x with (* fold this node, then recurse *)
    | Some z -> z (* intercepted *)
    | None ->     (* structurally recurse *)
        match x with
        | TExpr_const (c,m,ty)                                     -> z
        | TExpr_val (v,isSuperInit,m)                               -> z
        | TExpr_op (c,tyargs,args,m)                               -> exprsF z args
        | TExpr_seq (x0,x1,dir,_,m)                                -> exprsF z [x0;x1]
        | TExpr_lambda(lambda_id ,basevopt,argvs,body,m,rty,_)     -> exprF  z body
        | TExpr_tlambda(lambda_id,argtyvs,body,m,rty,_)            -> exprF  z body
        | TExpr_tchoose(_,body,m)                                  -> exprF  z body
        | TExpr_app (f,fty,tys,argtys,m)                           -> 
            let z = exprF z f
            let z = exprsF z argtys
            z
        | TExpr_letrec (binds,body,m,_)                            -> 
            let z = valBindsF false z binds
            let z = exprF z body
            z
        | TExpr_let    (bind,body,m,_)                             -> 
            let z = valBindF false z bind
            let z = exprF z body
            z
        | TExpr_link rX                                            -> exprF z (!rX)
        | TExpr_match (spBind,exprm,dtree,targets,m,ty,_)                 -> 
            let z = dtreeF z dtree
            let z = Array.fold_left targetF z targets
            z
        | TExpr_quote(e,{contents=Some(argTypes,argExprs,_)},m,_)  -> exprsF z argExprs
        | TExpr_quote(e,{contents=None},m,_)                       -> z
        | TExpr_obj (n,typ,basev,basecall,overrides,iimpls,m,_)    -> 
            let z = exprF z basecall
            let z = List.fold tmethodF z overrides
            let z = List.fold (foldOn snd (List.fold tmethodF)) z iimpls
            z
        | TExpr_static_optimization (tcs,csx,x,m)                   -> exprsF z [csx;x]
  and valBindF dtree z bind =
    let z = nonRecBindingsIntercept z bind
    bindF dtree z bind 
  and valBindsF dtree z binds =
    let z = recBindingsIntercept z binds
    FlatList.fold (bindF dtree) z binds 

  and bindF dtree z (bind:Binding) =
    let z = valBindingSiteIntercept z (dtree,bind.Var)
    exprF z bind.Expr

  and dtreeF z dtree =
    let z = dtreeAcc z dtree
    match dtree with
    | TDBind (bind,rest)            -> 
        let z = valBindF true z bind
        dtreeF z rest
    | TDSuccess (args,n)            -> flatExprsF z args
    | TDSwitch (test,dcases,dflt,r) -> 
        let z = exprF z test
        let z = List.fold dcaseF z dcases
        let z = Option.fold dtreeF z dflt
        z

  and dcaseF z = function
      TCase (test,dtree)   -> dtreeF z dtree (* not collecting from test *)

  and targetF z x =
    (match targetIntercept exprF z x with 
         Some z -> z (* intercepted *)
       | None ->     (* structurally recurse *)
           let (TTarget (argvs,body,_)) = x
           exprF z body)
            
  and tmethodF z x =
    (match tmethodIntercept exprF z x with 
         Some z -> z (* intercepted *)
       | None ->     (* structurally recurse *)
           let (TObjExprMethod(_,_,_,e,_)) = x
           exprF z e)

  and mexprF z x =
      match x with 
      | TMTyped(mtyp,def,m) -> mdefF z def
  and mdefF z x = 
      match x with
      | TMDefRec(tycons,binds,mbinds,m) -> 
          (* REVIEW: also iterate the abstract slot vspecs hidden in the _vslots field in the tycons *)
          let z = valBindsF false z binds
          let z = List.fold mbindF z mbinds
          z
      | TMDefLet(bind,m) -> valBindF false z bind
      | TMDefDo(e,m) -> exprF z e
      | TMDefs(defs) -> List.fold mdefF z defs 
      | TMAbstract(x) -> mexprF z x
  and mbindF z (TMBind(nm, def)) = mdefF z def

  and implF z x = foldTImplFile mexprF z x
  and implsF z (TAssembly(x)) = List.fold implF z x

 
  exprF, implF,implsF

let FoldExpr     folders = let exprF,implF,implsF = mkFolders folders in exprF
let FoldImplFile folders = let exprF,implF,implsF = mkFolders folders in implF

    
(*-------------------------------------------------------------------------
!* ExprStats
 *-------------------------------------------------------------------------*)

let ExprStats x =
  let count = ref 0
  let folders = {ExprFolder0 with exprIntercept = (fun exprF z x -> (count := !count + 1; None))}
  let () = FoldExpr folders () x
  string !count ^ " TExpr nodes"

    
(*-------------------------------------------------------------------------
!* 
 *------------------------------------------------------------------------- *)

let mk_string g m n = TExpr_const(TConst_string n,m,g.string_ty)
let mk_int64 g m n = TExpr_const(TConst_int64 n,m,g.int64_ty)
let mk_bool g m b = TExpr_const(TConst_bool b,m,g.bool_ty)
let mk_byte g m b = TExpr_const(TConst_byte b,m,g.byte_ty)
let mk_uint16 g m b = TExpr_const(TConst_uint16 b,m,g.uint16_ty)
let mk_true g m = mk_bool g m true
let mk_false g m = mk_bool g m false
let mk_unit g m = TExpr_const(TConst_unit,m,g.unit_ty)
let mk_int32 g m n =  TExpr_const(TConst_int32 n,m,g.int32_ty)
let mk_int g m n =  mk_int32 g m (n)
let mk_zero g m =  mk_int g m 0
let mk_one g m =  mk_int g m 1
let mk_two g m =  mk_int g m 2
let mk_minus_one g  m =  mk_int g m (-1)

let dest_int32 = function TExpr_const(TConst_int32 n,m,ty) -> Some n | _ -> None

let is_fslib_IDelegateEvent_ty g ty     = is_stripped_tyapp_typ g ty && tcref_eq g g.fslib_IDelegateEvent_tcr (tcref_of_stripped_typ g ty)
let dest_fslib_IDelegateEvent_ty g ty   = 
  if is_fslib_IDelegateEvent_ty g ty then 
    match tinst_of_stripped_typ g ty with 
    | [ty1] -> ty1
    | _ -> failwith "dest_fslib_IDelegateEvent_ty: internal error"
  else failwith "dest_fslib_IDelegateEvent_ty: not an IDelegateEvent type"
let mk_fslib_IEvent2_ty g ty1 ty2 = TType_app (g.fslib_IEvent2_tcr, [ty1;ty2])

let mk_refcell_contents_rfref g  = mk_rfref g.refcell_tcr "contents"

let typed_expr_for_val m (v:Val) = expr_for_val m v,v.Type

(*-------------------------------------------------------------------------
 * Tuples...
 *------------------------------------------------------------------------- *)
 
let mk_tupled g m es tys = 
    match es with 
    | [] -> mk_unit g m 
    | [e] -> e
    | _ -> TExpr_op(TOp_tuple,tys,es,m)

let mk_tupled_notypes g m args = mk_tupled g m args (List.map (type_of_expr g) args)

let mk_tupled_vars g m vs = mk_tupled g m (List.map (expr_for_val m) vs) (types_of_vals vs)

(*--------------------------------------------------------------------------
!* Permutations
 *------------------------------------------------------------------------*)
    
let inverse_perm (sigma:int array) =
  let n = Array.length sigma
  let inv_sigma = Array.create n (-1)
  for i = 0 to n-1 do
    let sigma_i = sigma.[i]
    (* assert( inv_sigma.[sigma_i] = -1 ); *)
    inv_sigma.[sigma_i] <- i
  done;
  inv_sigma
  
let permute (sigma:int array) (data:'a array) = 
  let n = Array.length sigma
  let inv_sigma = inverse_perm sigma
  Array.init n (fun i -> data.[inv_sigma.[i]])
  

(*--------------------------------------------------------------------------
!* Permute expressions
 *------------------------------------------------------------------------*)
    
let rec existsR a b pred = if a<=b then pred a || existsR (a+1) b pred else false
let mapi_acc_list f z xs =
  let rec fmapi f i z = function
    | []    -> z,[]
    | x::xs -> let z,x  = f i z x
               let z,xs = fmapi f (i+1) z xs
               z,x::xs
 
  fmapi f 0 z xs

/// Given expr = xi = [| x0; ... xN |]
/// Given sigma a permutation to apply to the xi.
/// Return (bindings',expr') such that:
///   (a) xi are permutated under sigma, xi -> position sigma(i).
///------
/// Motivation:
///   opt.ml    - put record field assignments in order under known effect information
///   ilxgen.ml - put record field assignments in order if necessary (no optimisations)
///               under unknown-effect information.
let permuteExpr (sigma:int array) (expr:expr array) (typ:typ array) (names:string array) =
    let inv_sigma = inverse_perm sigma
    let liftPosition i =
        (* In english, lift out xi if      
             * LC2: xi goes to position that will be preceded by
             *       (an expr with an effect that originally followed xi).
             *)
        (let i' = sigma.[i]
         existsR 0 (i' - 1) (fun j' ->
                                   let j = inv_sigma.[j']
                                   j > i))
   
    let rewrite i rbinds xi =
        if liftPosition i then
            let tmpv,tmpe = mk_compgen_local (range_of_expr xi) names.[i] typ.[i]
            let bind = mk_compgen_bind tmpv xi
            bind :: rbinds,tmpe
        else
            rbinds,xi
 
    let xis = Array.to_list expr
    let rbinds,xis = mapi_acc_list rewrite [] xis
    let binds = List.rev rbinds
    let expr  = permute sigma (Array.of_list xis)
    binds,expr
    
let permuteExprList (sigma:int array) (expr:expr list) (typ:typ list)  (names:string list) =
    let binds,expr = permuteExpr sigma (Array.of_list expr) (Array.of_list typ)  (Array.of_list names)
    binds,Array.to_list expr
  

(*-------------------------------------------------------------------------
 * Build lazy expressions...
 *------------------------------------------------------------------------- *)

let mk_seq spSeq m e1 e2 = TExpr_seq(e1,e2,NormalSeq,spSeq,m)
let mk_compgen_seq m e1 e2 = mk_seq SuppressSequencePointOnExprOfSequential m e1 e2
let rec mk_seqs spSeq g m es = 
    match es with 
    | [e] -> e 
    | e::es -> mk_seq spSeq m e (mk_seqs spSeq g m es) 
    | [] -> mk_unit g m

/// Evaluate the expressions in the original order, but build a record with the results in field order 
/// Note some fields may be static. If this were not the case we could just use 
///     let sigma       = Array.map rfref_index ()  
/// However the presence of static fields means rfref_index may index into a non-compact set of instance field indexes. 
/// We still need to sort by index. 
let mk_recd g (lnk,tcref,tinst,rfrefs,args,m) =  
    (* Remove any abbreviations *)
    let tcref,tinst = dest_stripped_tyapp_typ g (mk_tyapp_ty tcref tinst)
    
    let rfrefs_array = Array.of_list (List.mapi (fun i x -> (i,x)) rfrefs)
    Array.sortInPlaceBy (snd >> rfref_index) rfrefs_array;
    let sigma = Array.create (Array.length rfrefs_array) (-1)
    Array.iteri (fun j (i,_) -> 
        if sigma.[i] <> -1 then error(InternalError("bad permutation",m));
        sigma.[i] <- j)  rfrefs_array;
    
    let argTyps     = List.map (fun rfref  -> actual_rtyp_of_rfref rfref tinst) rfrefs
    let names       = rfrefs |> List.map (fun rfref -> rfref.FieldName)
    let binds,args  = permuteExprList sigma args argTyps names
    mk_lets_bind m binds (TExpr_op(TOp_recd(lnk,tcref),tinst,args,m))
  
let mk_ldarg0 m ty = mk_asm( [ ldarg_0 ],[],[],[ty],m) 

let mk_refcell     g m ty e = mk_recd g (RecdExpr,g.refcell_tcr,[ty],[mk_refcell_contents_rfref g],[e],m)
let mk_refcell_get g m ty e = mk_recd_field_get g (e,mk_refcell_contents_rfref g,[ty],[],m)
let mk_refcell_set g m ty e1 e2 = mk_recd_field_set g (e1,mk_refcell_contents_rfref g,[ty],e2,m)

(*-------------------------------------------------------------------------
 * List builders
 *------------------------------------------------------------------------- *)
 
let mk_nil g m ty = mk_ucase (g.nil_ucref,[ty],[],m)
let mk_cons g ty h t = mk_ucase (g.cons_ucref,[ty],[h;t],union_ranges (range_of_expr h) (range_of_expr t))

let mk_compgen_local_and_invisible_bind g nm m e = 
    let locv,loce = mk_compgen_local m nm (type_of_expr g e)
    locv,loce,mk_invisible_bind locv e 

(*----------------------------------------------------------------------------
 * Make some fragments of code
 *--------------------------------------------------------------------------*)

let box = IL.I_box (IL.mk_tyvar_ty 0us)
let isinst = IL.I_isinst (IL.mk_tyvar_ty 0us)
let unbox = IL.I_unbox_any (IL.mk_tyvar_ty 0us)
let mk_unbox ty e m = mk_asm ([ unbox ], [ty],[e], [ ty ], m)
let mk_isinst ty e m = mk_asm ([ isinst ], [ty],[e], [ ty ], m)

let mspec_Object_GetHashCode     ilg = IL.mk_nongeneric_instance_mspec_in_nongeneric_boxed_tref(ilg.tref_Object,"GetHashCode",[],ilg.typ_int32)
let mspec_Type_GetTypeFromHandle ilg = IL.mk_static_nongeneric_mspec_in_nongeneric_boxed_tref(ilg.tref_Type,"GetTypeFromHandle",[ilg.typ_RuntimeTypeHandle],ilg.typ_Type)
let fspec_Missing_Value  ilg = IL.mk_fspec_in_nongeneric_boxed_tref(ilg.tref_Missing,"Value",ilg.typ_Missing)


let typed_expr_for_val_info m (Intrinsic(mvr,nm,ty) as i) =
  let e = vref_for_val_info i
  expr_for_vref m e,ty

let mk_call_get_generic_comparer g m = mk_appl g (typed_expr_for_val_info m g.get_generic_comparer_info,       [], [ mk_unit g m ],  m)
let mk_call_get_generic_equality_comparer g m = mk_appl g (typed_expr_for_val_info m g.get_generic_equality_comparer_info,       [], [ mk_unit g m ],  m)
let mk_call_unbox                g m ty e1    = mk_appl g (typed_expr_for_val_info m g.unbox_info,       [[ty]], [ e1 ],  m)
let mk_call_unbox_fast           g m ty e1    = mk_appl g (typed_expr_for_val_info m g.unbox_fast_info,  [[ty]], [ e1 ],  m)
let mk_call_istype               g m ty e1    = mk_appl g (typed_expr_for_val_info m g.istype_info,      [[ty]], [ e1 ],  m)
let mk_call_istype_fast          g m ty e1    = mk_appl g (typed_expr_for_val_info m g.istype_fast_info, [[ty]], [ e1 ],  m)
let mk_call_typeof               g m ty       = mk_appl g (typed_expr_for_val_info m g.typeof_info,      [[ty]], [ ],  m)

let mk_call_dispose               g m ty e1    = mk_appl g (typed_expr_for_val_info m g.dispose_info,       [[ty]], [ e1 ],  m)
let mk_call_seq                   g m ty e1    = mk_appl g (typed_expr_for_val_info m g.seq_info,           [[ty]], [ e1 ],  m)
let mk_call_create_instance       g m ty       = mk_appl g (typed_expr_for_val_info m g.create_instance_info, [[ty]], [  (mk_unit g m) ],  m)

let mk_call_generic_comparison_outer       g m ty e1 e2      = mk_appl g (typed_expr_for_val_info m g.generic_comparison_outer_info,       [[ty]], [  e1;e2 ],  m)
let mk_call_generic_comparison_withc_outer g m ty comp e1 e2 = mk_appl g (typed_expr_for_val_info m g.generic_comparison_withc_outer_info, [[ty]], [  comp;e1;e2 ],  m)
let mk_call_generic_equality_outer        g m ty e1 e2      = mk_appl g (typed_expr_for_val_info m g.generic_equality_outer_info,        [[ty]], [  e1;e2 ],  m)
let mk_call_generic_equality_withc_outer  g m ty comp e1 e2 = mk_appl g (typed_expr_for_val_info m g.generic_equality_withc_outer_info,  [[ty]], [comp;e1;e2], m)
//let mk_call_generic_hash_outer          g m ty e1         = mk_appl g (typed_expr_for_val_info m g.generic_hash_outer_info,          [[ty]], [e1], m)
let mk_call_generic_hash_withc_outer    g m ty comp e1    = mk_appl g (typed_expr_for_val_info m g.generic_hash_withc_outer_info,    [[ty]], [comp;e1], m)

let mk_call_array_get             g m ty e1 e2 = mk_appl g (typed_expr_for_val_info m g.array_get_info, [[ty]], [ e1 ; e2 ],  m)
let mk_call_new_decimal g m (e1,e2,e3,e4,e5)       = mk_appl g (typed_expr_for_val_info m g.new_decimal_info, [], [ e1;e2;e3;e4;e5 ],  m)

let mk_call_string_compare g m e1 e2 = mk_call_generic_comparison_outer g m g.string_ty e1 e2 

let mk_call_new_format                g m aty bty cty dty ety e1    = mk_appl g (typed_expr_for_val_info m g.new_format_info, [[aty;bty;cty;dty;ety]], [ e1 ],  m)
let mk_call_raise g m aty e1    = mk_appl g (typed_expr_for_val_info m g.raise_info, [[aty]], [ e1 ],  m)

let TryEliminateDesugaredConstants g m c = 
    match c with 

    | TConst_decimal d -> 
        match System.Decimal.GetBits(d) with 
        | [| lo;med;hi; signExp |] -> 
            let scale = (min (((signExp &&& 0xFF0000) >>> 16) &&& 0xFF) 28) |> byte
            let isNegative = (signExp &&& 0x80000000) <> 0
        
            Some(mk_call_new_decimal g m (mk_int g m lo,mk_int g m med,mk_int g m hi,mk_bool g m isNegative,mk_byte g m scale) )
        | _ -> failwith "unreachable"
        
    | _ -> 
        None

let mk_seq_ty g ty = mk_tyapp_ty g.seq_tcr [ty] 
let mk_IEnumerator_ty g ty = mk_tyapp_ty g.tcref_System_Collections_Generic_IEnumerator [ty] 

let mk_call_seq_map_concat g m alphaTy betaTy arg1 arg2 = 
    let enumty2 = try range_of_fun_typ g (type_of_expr g arg1) with _ -> (* defensive programming *) (mk_seq_ty g betaTy)
    mk_appl g (typed_expr_for_val_info m g.seq_map_concat_info, [[alphaTy;enumty2;betaTy]], [ arg1; arg2 ],  m) 
                  
let mk_call_seq_using g m resourceTy elemTy arg1 arg2 = 
    (* We're intantiating val using : 'a -> ('a -> 'sb) -> seq<'b> when 'sb :> seq<'b> and 'a :> IDisposable *)
    (* We set 'sb -> range(typeof(arg2)) *)
    let enumty = try range_of_fun_typ g (type_of_expr g arg2) with _ -> (* defensive programming *) (mk_seq_ty g elemTy)
    mk_appl g (typed_expr_for_val_info m g.seq_using_info, [[resourceTy;enumty;elemTy]], [ arg1; arg2 ],  m) 
                  
let mk_call_seq_delay g m elemTy arg1 = 
    mk_appl g (typed_expr_for_val_info m g.seq_delay_info, [[elemTy]], [ arg1 ],  m) 
                  
let mk_call_seq_append g m elemTy arg1 arg2 = 
    mk_appl g (typed_expr_for_val_info m g.seq_append_info, [[elemTy]], [ arg1; arg2 ],  m) 

let mk_call_seq_generated g m elemTy arg1 arg2 = 
    mk_appl g (typed_expr_for_val_info m g.seq_generated_info, [[elemTy]], [ arg1; arg2 ],  m) 
                       
let mk_call_seq_finally g m elemTy arg1 arg2 = 
    mk_appl g (typed_expr_for_val_info m g.seq_finally_info, [[elemTy]], [ arg1; arg2 ],  m) 
                       
let mk_call_seq_of_functions g m ty1 ty2 arg1 arg2 arg3 = 
    mk_appl g (typed_expr_for_val_info m g.seq_of_functions_info, [[ty1;ty2]], [ arg1; arg2; arg3  ],  m) 
                  
let mk_call_seq_to_array g m elemTy arg1 =  
    mk_appl g (typed_expr_for_val_info m g.seq_to_array_info, [[elemTy]], [ arg1 ],  m) 
                  
let mk_call_seq_to_list g m elemTy arg1 = 
    mk_appl g (typed_expr_for_val_info m g.seq_to_list_info, [[elemTy]], [ arg1 ],  m) 
                  
let mk_call_seq_map g m inpElemTy genElemTy arg1 arg2 = 
    mk_appl g (typed_expr_for_val_info m g.seq_map_info, [[inpElemTy;genElemTy]], [ arg1; arg2 ],  m) 
                  
let mk_call_seq_singleton g m ty1 arg1 = 
    mk_appl g (typed_expr_for_val_info m g.seq_singleton_info, [[ty1]], [ arg1 ],  m) 
                  
let mk_call_seq_empty g m ty1 = 
    mk_appl g (typed_expr_for_val_info m g.seq_empty_info, [[ty1]], [ ],  m) 
                 
let mk_call_unpickle_quotation g m e1 e2 e3 e4 = 
    let args = [ e1; e2; e3; e4 ]
    mk_appl g (typed_expr_for_val_info m g.unpickle_quoted_info, [], [ mk_tupled_notypes g m args ],  m)

let mk_call_cast_quotation g m ty e1 = 
    mk_appl g (typed_expr_for_val_info m g.cast_quotation_info, [[ty]], [ e1 ],  m)

let mk_call_lift_value g m ty e1 = 
    mk_appl g (typed_expr_for_val_info m g.lift_value_info , [[ty]], [ e1],  m)

let mk_lazy_delayed g m ty f = mk_appl g (typed_expr_for_val_info m g.lazy_create_info, [[ty]], [ f ],  m) 
let mk_lazy_force g m ty e = mk_appl g (typed_expr_for_val_info m g.lazy_force_info, [[ty]], [ e; mk_unit g m ],  m) 


let query_asm e = 
    match strip_expr e with 
    |  TExpr_op(TOp_asm (instrs,_),[],args,_) ->Some(instrs,args)
    | _ -> None 

let dest_incr e = 
    match query_asm e with 
    | Some([ IL.I_arith IL.AI_add ],[TExpr_const(TConst_int32 1,_,_) ;arg2]) -> Some(arg2)
    | _ -> None 

// Note: We plan to get rid of all IL generation in the typechecker and pattern match
// compiler, or else train the quotation generator to understand the generated IL. 
// Hence each of the following are marked with places where they are generated.

// Generated by the optimizer and the encoding of 'for' loops     
let mk_decr g m e = mk_asm([ IL.I_arith IL.AI_sub  ],[],[e; mk_one g m],[g.int_ty],m)
let mk_incr g m e = mk_asm([ IL.I_arith IL.AI_add  ],[],[mk_one g m; e],[g.int_ty],m)

// Generated by the pattern match compiler and the optimizer for
//    1. array patterns
//    2. optimizations associated with getting 'for' loops into the shape expected by the JIT.
// 
// NOTE: The conv.i4 assumes that int_ty is int32. Note: ldlen returns native UNSIGNED int 
//
// REVIEW: quotation processing doesn't yet understand this, giving a problem with quoting
// constructors of array pattern matching
let mk_ldlen g m arre = mk_asm ([ IL.I_ldlen; IL.I_arith (IL.AI_conv IL.DT_I4) ],[],[ arre ], [ g.int_ty ], m)

// This is generated in equality/compare/hash augmentations and in the pattern match compiler.
// It is understood by the quotation processor and turned into "Equality" nodes.
let mk_ceq g m e1 e2 = mk_asm ([ IL.I_arith IL.AI_ceq  ],[],  [e1; e2],[g.bool_ty],m)

// This is generated in the initialization of the "ctorv" field in the typechecker's compilation of
// an implicit class construction.
let mk_null m ty = TExpr_const(TConst_zero, m,ty)

(*----------------------------------------------------------------------------
 * rethrow
 *--------------------------------------------------------------------------*)

(* throw, rethrow *)
let mk_throw m ty e = mk_asm ([ IL.I_throw ],[], [e],[ty],m)
let dest_throw = function
    | TExpr_op(TOp_asm([IL.I_throw],[ty2]),[],[e],m) -> Some (m,ty2,e)
    | _ -> None
let is_throw x = isSome (dest_throw x)

// rethrow - parsed as library call - internally represented as op form.
let mk_rethrow_library_call g ty m = let ve,vt = typed_expr_for_val_info m g.rethrow_info in TExpr_app(ve,vt,[ty],[mk_unit g m],m)
let mk_rethrow m returnTy = TExpr_op(TOp_rethrow,[returnTy],[],m) (* could suppress unitArg *)

(*----------------------------------------------------------------------------
 * CompilationMappingAttribute, SourceConstructFlags
 *--------------------------------------------------------------------------*)

let tref_CompilationMappingAttr() = mk_tref(Msilxlib.scoref (), lib_MFCore_name ^ ".CompilationMappingAttribute")
let tref_SourceConstructFlags ()=  mk_tref(Msilxlib.scoref (), lib_MFCore_name ^ ".SourceConstructFlags")
let IsCompilationMappingAttr cattr = is_il_attrib (tref_CompilationMappingAttr ()) cattr
let mk_CompilationMappingAttrPrim g k nums = 
    mk_custom_attribute g.ilg (tref_CompilationMappingAttr(), 
                               ((mk_nongeneric_value_typ (tref_SourceConstructFlags())) :: (nums |> List.map (fun _ -> g.ilg.typ_Int32))),
                               ((k :: nums) |> List.map (fun n -> CustomElem_int32(n))),
                               [])
let mk_CompilationMappingAttr g kind = mk_CompilationMappingAttrPrim g kind []
let mk_CompilationMappingAttrWithSeqNum g kind seqNum = mk_CompilationMappingAttrPrim g kind [seqNum]
let mk_CompilationMappingAttrWithVariantNumAndSeqNum g kind varNum seqNum = mk_CompilationMappingAttrPrim g kind [varNum;seqNum]

(*----------------------------------------------------------------------------
 * FSharpInterfaceDataVersionAttribute
 *--------------------------------------------------------------------------*)

let tref_SignatureDataVersionAttr () = 
    mk_tref(Msilxlib.scoref (), lib_MFCore_name ^ ".FSharpInterfaceDataVersionAttribute")

let mk_SignatureDataVersionAttr g ((v1,v2,v3,v4) : ILVersionInfo)  = 
    mk_custom_attribute g.ilg
        (tref_SignatureDataVersionAttr(), 
         [g.ilg.typ_Int32;g.ilg.typ_Int32;g.ilg.typ_Int32],
         [CustomElem_int32 (int32 v1);
          CustomElem_int32 (int32 v2) ; 
          CustomElem_int32 (int32 v3)],[])

let tref_AutoOpenAttr () = 
    mk_tref(Msilxlib.scoref (), lib_MFCore_name ^ ".AutoOpenAttribute")

let IsSignatureDataVersionAttr cattr = is_il_attrib (tref_SignatureDataVersionAttr ()) cattr
let TryFindAutoOpenAttr cattr = 
    if is_il_attrib (tref_AutoOpenAttr ()) cattr then 
        (* ok to use ecmaILGlobals here since we're querying metadata, not making it *)
        match decode_il_attrib_data IL.ecmaILGlobals cattr with 
        |  [CustomElem_string s],_ -> s
        |  [],_ -> None
        | _ -> 
            warning(Failure("Unexpected decode of AutoOpenAttribute")); 
            None
    else
        None
        
let tref_InternalsVisibleToAttr () = 
    mk_tref (ecma_mscorlib_scoref,"System.Runtime.CompilerServices.InternalsVisibleToAttribute")    

let TryFindInternalsVisibleToAttr cattr = 
    if is_il_attrib (tref_InternalsVisibleToAttr ()) cattr then 
        (* ok to use ecmaILGlobals here since we're querying metadata, not making it *)
        match decode_il_attrib_data IL.ecmaILGlobals cattr with 
        |  [CustomElem_string s],_ -> s
        |  [],_ -> None
        | _ -> 
            warning(Failure("Unexpected decode of InternalsVisibleToAttribute")); 
            None
    else
        None

let IsMatchingSignatureDataVersionAttr  ((v1,v2,v3,v4) : ILVersionInfo)  cattr = 
    IsSignatureDataVersionAttr cattr &&
    (* ok to use ecmaILGlobals here since we're querying metadata, not making it *)
    match decode_il_attrib_data IL.ecmaILGlobals cattr with 
    |  [CustomElem_int32 u1; CustomElem_int32 u2;CustomElem_int32 u3 ],_ -> 
          // add this specific case to reject the CTP format, which also shares prefix 1.9.6
          let fullName = cattr.customMethod.EnclosingType.TypeSpec.FullName
          if fullName.Contains("Version=1.9.6.3,") || fullName.Contains("Version=1.9.6.2,") || fullName.Contains("Version=1.9.6.0,") then false
          else (v1 = uint16 u1) && (v2 = uint16 u2) && (v3 = uint16 u3)
    | _ -> warning(Failure("Unexpected decode of InterfaceDataVersionAttribute")); false

let mk_CompilerGeneratedAttr g n = mk_custom_attribute g.ilg (tref_CompilationMappingAttr(), [mk_nongeneric_value_typ (tref_SourceConstructFlags())],[CustomElem_int32(n)],[])

(* match inp with :? ty as v -> e2[v] | _ -> e3 *)
let mk_isinst_cond g m tgty vinpe v e2 e3 = 
    // No sequence point for this simple expression form
    let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m)
    let tg2 = TDSuccess(FlatList.one (mk_call_unbox g m tgty vinpe), mbuilder.AddTarget(TTarget(FlatList.one v,e2,SuppressSequencePointAtTarget)))
    let tg3 = mbuilder.AddResultTarget(e3,SuppressSequencePointAtTarget)
    let dtree = TDSwitch(vinpe,[TCase(TTest_isinst(type_of_expr g vinpe,tgty),tg2)],Some tg3,m)
    mbuilder.Close(dtree,m,type_of_expr g e2)


(*--------------------------------------------------------------------------
!* tupled lambda --> method/function with a given topValInfo specification.
 *
 * AdjustArityOfLambdaBody: "(vs,body)" represents a lambda "fun (vs) ->  body".  The
 * aim is to produce a "static method" represented by a pair
 * "(mvs, body)" where mvs has the List.length "arity".
 *------------------------------------------------------------------------ *)


let untupled_to_tupled vs =
    let untupledTys = types_of_vals vs
    let m = (List.hd vs).Range
    let tupledv,tuplede = mk_compgen_local m "tupledArg" (mk_tuple_ty untupledTys)
    let untupling_es =  List.mapi (fun i ty ->  mk_tuple_field_get(tuplede,untupledTys,i,m)) untupledTys
    tupledv, mk_invisible_lets m vs untupling_es 
    
// The required tupled-arity (arity) can either be 1 
// or N, and likewise for the tuple-arity of the input lambda, i.e. either 1 or N 
// where the N's will be identical. 
let AdjustArityOfLambdaBody g arity (vs:Val list) body = 
    let nvs = vs.Length
    if not (nvs = arity || nvs = 1 || arity = 1) then failwith ("lengths don't add up");
    if arity = 0 then 
        vs,body
    elif nvs = arity then 
        vs,body
    elif nvs = 1 then
        let v = vs.Head
        let untupledTys = dest_tuple_typ g v.Type
        if  (untupledTys.Length <> arity) then failwith "length untupledTys <> arity";
        let dummyvs,dummyes = 
            untupledTys 
            |> List.mapi (fun i ty -> mk_compgen_local v.Range (v.MangledName ^"_"^string i) ty) 
            |> List.unzip 
        let body = mk_invisible_let v.Range v (mk_tupled g v.Range dummyes untupledTys) body
        dummyvs,body
    else 
        let tupledv, untupler =  untupled_to_tupled vs
        [tupledv],untupler body

let multi_lambda_to_tupled_lambda vs body = 
    match vs with 
    | [] -> failwith "multi_lambda_to_tupled_lambda: expected some argments"
    | [v] -> v,body 
    | vs -> 
        let tupledv, untupler =  untupled_to_tupled vs
        tupledv, untupler body 
      

//--------------------------------------------------------------------------
// Beta reduction via let-bindings. Reduce immediate apps. of lambdas to let bindings. 
// Includes binding the immediate application of generic
// functions. Input type is the type of the function.  Makes use of the invariant
// that any two expressions have distinct local variables (because we explicitly copy
// expressions).
//------------------------------------------------------------------------ 

let rec MakeApplicationAndBetaReduceAux g (f,fty,tyargsl,argsl,m) =
  (* let verbose = true in *)
  match f with 
  | TExpr_let(bind,body,mlet,_) ->
      // Lift bindings out, i.e. (let x = e in f) y --> let x = e in f y 
      // This increases the scope of 'x', which I don't like as it mucks with debugging 
      // scopes of variables, but this is an important optimization, especially when the '|>' 
      // notation is used a lot. 
      (* REVIEW: only apply this when beta-reduction really occurs *)
      if verbose then dprintf "--- MakeApplicationAndBetaReduceAux, reducing under let\n";
      mk_let_bind mlet bind (MakeApplicationAndBetaReduceAux g (body,fty,tyargsl,argsl,m))
  | _ -> 
  match tyargsl,argsl with 
  | [] :: rest,_ -> 
     MakeApplicationAndBetaReduceAux g (f,fty,rest,argsl,m)

  | tyargs :: rest,_ -> 
      (* Bind type parameters by immediate substitution *)
      match f with 
      | TExpr_tlambda(_, tyvs,body,_,bodyty,_) when tyvs.Length = List.length tyargs -> 
          let tpenv = bind_typars tyvs tyargs empty_tpenv
          let body = RemarkExpr m (inst_expr g tpenv body)
          let bodyty' = InstType tpenv bodyty
          MakeApplicationAndBetaReduceAux g (body,bodyty', rest,argsl,m) 

      | _ -> 
          let f,fty = mk_appl_aux g f fty [tyargs] [] m
          MakeApplicationAndBetaReduceAux g (f,fty, rest,argsl,m)

  | [], arg :: rest ->
      (* Bind term parameters by "let" explicit substitutions *)
      match f with 
      | TExpr_lambda(_,None,argvs,body,_,bodyty,_) -> 
          let argv,body = multi_lambda_to_tupled_lambda argvs body
          mk_compgen_let m argv arg (MakeApplicationAndBetaReduceAux g (body, bodyty, [],rest,m))
      | _ -> 
          let f,fty = mk_expr_appl_aux g f fty [arg] m
          MakeApplicationAndBetaReduceAux g (f,fty, [], rest,m)

  | [],[] -> 
      f
      
let MakeApplicationAndBetaReduce g (f,fty,tyargsl,argl,m) = 
  MakeApplicationAndBetaReduceAux g (f,fty,tyargsl,argl,m)

//---------------------------------------------------------------------------
// Adjust for expected usage
// Convert a use of a value to saturate to the given arity.
//--------------------------------------------------------------------------- 

let MakeArgsForTopArgs g m argtysl tpenv =
    argtysl |> List.mapi (fun i argtys -> 
        let n = List.length argtys
        argtys |> List.mapi (fun j (argty,TopArgInfo(_,nm)) -> 
            let ty = InstType tpenv argty
            let nm = 
               match nm with 
               | None -> CompilerGeneratedName ("arg"^ string i^ string j)
               | Some id -> id.idText
            fst (mk_compgen_local m nm ty)))

let AdjustValForExpectedArity g m (vref:ValRef) flags topValInfo =

    let tps,argtysl,rty,_ = GetTopValTypeInFSharpForm g topValInfo vref.Type m
    let tps' = CopyTypars tps
    let tyargs' = List.map mk_typar_ty tps'
    let tpenv = bind_typars tps tyargs' empty_tpenv
    let rty' = InstType tpenv rty
    let vsl = MakeArgsForTopArgs g m argtysl tpenv
    let call = MakeApplicationAndBetaReduce g (TExpr_val(vref,flags,m),vref.Type,[tyargs'],(List.map (mk_tupled_vars g m) vsl),m)
    let tauexpr,tauty = 
        List.foldBack 
            (fun vs (e,ty) -> mk_multi_lambda m vs (e, ty), (mk_tupled_vars_ty g vs --> ty))
            vsl
            (call, rty')
    // Build a type-lambda expression for the toplevel value if needed... 
    mk_tlambda m tps' (tauexpr,tauty),tps' +-> tauty


//---------------------------------------------------------------------------
// 


let IsSubsumptionExpr g expr =
    match expr with 
    | TExpr_op(TOp_coerce,[inputTy;actualTy],[_],m) ->
        is_fun_typ g actualTy && is_fun_typ g inputTy   
    | _ -> 
        false

let strip_tupled_fun_typ g ty = 
    let argTys,retTy = strip_fun_typ g ty
    let curriedArgTys = argTys |> List.map (try_dest_tuple_typ g)
    curriedArgTys, retTy

let (|ExprValWithPossibleTypeInst|_|) expr =
    match expr with 
    | TExpr_app(TExpr_val(vref,flags,m),fty,tyargs,[],_)  ->
        Some(vref,flags,tyargs,m)
    | TExpr_val(vref,flags,m) ->
        Some(vref,flags,[],m)
    | _ -> 
        None

let mk_coerce_if_needed g tgtTy srcTy expr =
    //if type_definitely_subsumes_type_no_coercion 0 g cenv.amap m tgtTy srcTy then 
    if type_equiv g tgtTy srcTy then 
        expr
    else 
        mk_coerce(expr,tgtTy,range_of_expr expr,srcTy)

let mk_compgen_let_in m nm ty e f = 
    let v,ve = mk_compgen_local m nm ty
    mk_compgen_let m v e (f (v,ve))

/// Take a node representing a coercion from one function type to another, e.g.
///    A -> A * A -> int 
/// to 
///    B -> B * A -> int 
/// and return an expression of the correct type that doesn't use a coercion type. For example
/// return   
///    (fun b1 b2 -> E (b1 :> A) (b2 :> A))
///
///    - Use good names for the closure arguments if available
///    - Create lambda variables if needed, or use the supplied arguments if available.
///
/// Return the new expression and any unused suffix of supplied arguments
///
/// If E is a value with TopInfo then use the arity to help create a better closure.
/// In particular we can create a closure like this:
///    (fun b1 b2 -> E (b1 :> A) (b2 :> A))
/// rather than 
///    (fun b1 -> let clo = E (b1 :> A) in (fun b2 -> clo (b2 :> A)))
/// The latter closures are needed to carefully preserve side effect order
///
/// Note that the results of this translation are visible to quotations

let AdjustPossibleSubsumptionExpr g (expr:expr) (suppliedArgs: expr list) : (expr* expr list) option =

    match expr with 
    | TExpr_op(TOp_coerce,[inputTy;actualTy],[exprWithActualTy],m) when 
        is_fun_typ g actualTy && is_fun_typ g inputTy  ->
        
        if type_equiv g actualTy inputTy then 
            Some(exprWithActualTy, suppliedArgs)
        else
            
            let curriedActualArgTys,retTy = strip_tupled_fun_typ g actualTy

            let curriedInputTys,_ = strip_fun_typ g inputTy

            assert (curriedActualArgTys.Length = curriedInputTys.Length)

            let argTys = (curriedInputTys,curriedActualArgTys) ||> List.mapi2 (fun i x y -> (i,x,y))


            // Use the nice names for a function of known arity and name. Note that 'nice' here also 
            // carries a semantic meaning. For a function with top-info,
            //   let f (x:A) (y:A) (z:A) = ...
            // we know there are no side effects on the application of 'f' to 1,2 args. This greatly simplifies
            // the closure built for 
            //   f b1 b2 
            // and indeed for 
            //   f b1 b2 b3
            // we don't build any closure at all, and just return
            //   f (b1 :> A) (b2 :> A) (b3 :> A)
            
            let curriedNiceNames = 
                match strip_expr exprWithActualTy with 
                | ExprValWithPossibleTypeInst(vref,flags,tyargs,m) when vref.TopValInfo.IsSome -> 

                    let tps,argtysl,rty,_ = GetTopValTypeInFSharpForm g vref.TopValInfo.Value vref.Type (range_of_expr expr)
                    argtysl |> List.mapi (fun i argtys -> 
                        let n = List.length argtys
                        argtys |> List.mapi (fun j (argty,TopArgInfo(_,nm)) -> 
                             match nm with 
                             | None -> CompilerGeneratedName ("arg" ^ string i ^string j)
                             | Some id -> id.idText))
                | _ -> 
                    []

            assert (curriedActualArgTys.Length >= curriedNiceNames.Length)

            let argTysWithNiceNames,argTysWithoutNiceNames =
                List.chop curriedNiceNames.Length argTys

            /// Only consume 'suppliedArgs' up to at most the number of nice arguments
            let suppliedArgs, droppedSuppliedArgs = 
                List.chop (min suppliedArgs.Length curriedNiceNames.Length) suppliedArgs

            /// THe relevant range for any expressions and applications includes the arguments 
            let appm = List.fold (fun m e -> union_ranges m (range_of_expr e)) m suppliedArgs

            // See if we have 'enough' suppliedArgs. If not, we have to build some lambdas, and,
            // we have to 'let' bind all arguments that we consume, e.g.
            //   Seq.take (effect;4) : int list -> int list
            // is a classic case. Here we generate
            //   let tmp = (effect;4) in 
            //   (fun v -> Seq.take tmp (v :> seq<_>))
            let buildingLambdas = (suppliedArgs.Length <> curriedNiceNames.Length)
            //printfn "buildingLambdas = %A" buildingLambdas
            //printfn "suppliedArgs.Length = %d" suppliedArgs.Length 

            /// Given a tuple of argument variables that has a tuple type that satisfies the input argument types,
            /// coerce it to a tuple that satisfies the matching coerced argument type(s).
            let CoerceDetupled (argTys: typ list) (detupledArgs:expr list) (actualTys: typ list) =
                assert (actualTys.Length = argTys.Length)
                assert (actualTys.Length = detupledArgs.Length)
                // Inject the coercions into the user-supplied explicit tuple
                let argm = List.reduce_left union_ranges (List.map range_of_expr detupledArgs) 
                mk_tupled g argm (List.map3 (mk_coerce_if_needed g) actualTys argTys detupledArgs) actualTys

            /// Given an argument variable of tuple type that has been evaluated and stored in the 
            /// given variable, where the tuple type that satisfies the input argument types,
            /// coerce it to a tuple that satisfies the matching coerced argument type(s).
            let CoerceBoundTuple tupleVar argTys (actualTys : typ list) =
                assert (actualTys.Length > 1)
            
                mk_tupled g appm 
                   ((actualTys,argTys) ||> List.mapi2 (fun i actualTy dummyTy ->  
                       let argExprElement = mk_tuple_field_get(tupleVar,argTys,i,appm)
                       mk_coerce_if_needed  g actualTy dummyTy argExprElement))
                   actualTys

            /// Given an argument that has a tuple type that satisfies the input argument types,
            /// coerce it to a tuple that satisfies the matching coerced argument type. Try to detuple the argument if possible.
            let CoerceTupled niceNames (argExpr:expr) (actualTys:typ list) =
                let argExprTy = (type_of_expr g argExpr)

                let argTys  = 
                    match actualTys with 
                    | [_] -> 
                        [type_of_expr g argExpr]
                    | _ -> 
                        try_dest_tuple_typ g argExprTy 
                
                assert (actualTys.Length = argTys.Length)
                let nm = match niceNames with [nm] -> nm | _ -> "arg"
                if buildingLambdas then 
                    // Evaluate the user-supplied tuple-valued argument expression, inject the coercions and build an explicit tuple
                    // Assign the argument to make sure it is only run once
                    //     f ~~> : B -> int
                    //     f ~~> : (B * B) -> int
                    //
                    //  for 
                    //     let f a = 1
                    //     let f (a,a) = 1
                    let v,ve = mk_compgen_local appm nm argExprTy
                    let binderBuilder = (fun tm -> mk_compgen_let appm v argExpr tm)
                    let expr = 
                        match actualTys,argTys with
                        | [actualTy],[argTy] -> mk_coerce_if_needed  g actualTy argTy ve 
                        | _ -> CoerceBoundTuple ve argTys actualTys

                    binderBuilder,expr
                else                
                    if type_equiv g (mk_tupled_ty g actualTys) argExprTy then 
                        (fun tm -> tm), argExpr
                    else
                    
                        let detupledArgs,argTys  = 
                            match actualTys with 
                            | [actualType] -> 
                                [argExpr],[type_of_expr g argExpr]
                            | _ -> 
                                try_dest_tuple argExpr,try_dest_tuple_typ g argExprTy 

                        // OK, the tuples match, or there is no de-tupling,
                        //     f x
                        //     f (x,y)
                        //
                        //  for 
                        //     let f (x,y) = 1
                        // and we're not building lambdas, just coerce the arguments in place
                        if detupledArgs.Length =  actualTys.Length then 
                            (fun tm -> tm), CoerceDetupled argTys detupledArgs actualTys
                        else 
                            // In this case there is a tuple mismatch.
                            //     f p
                            //
                            //
                            //  for 
                            //     let f (x,y) = 1
                            // Assign the argument to make sure it is only run once
                            let v,ve = mk_compgen_local appm nm argExprTy
                            let binderBuilder = (fun tm -> mk_compgen_let appm v argExpr tm)
                            let expr = CoerceBoundTuple ve argTys actualTys
                            binderBuilder,expr
                        

            // This variable is really a dummy to make the code below more regular. 
            // In the i = N - 1 cases we skip the introduction of the 'let' for
            // this variable.
            let resVar,resVarAsExpr = mk_compgen_local appm "result" retTy
            let N = argTys.Length
            let (cloVar,exprForOtherArgs,exprForOtherArgsTy) = 
                List.foldBack 
                    (fun (i,inpArgTy,actualArgTys) (cloVar:Val,res,resTy) -> 

                        let inpArgTys = 
                            match actualArgTys with 
                            | [_] -> [inpArgTy]
                            | _ -> dest_tuple_typ g inpArgTy

                        assert (inpArgTys.Length = actualArgTys.Length)
                        
                        let inpsAsVars,inpsAsExprs = inpArgTys |> List.mapi (fun j ty -> mk_compgen_local appm ("arg"^string i^string j) ty)  |> List.unzip
                        let inpsAsActualArg = CoerceDetupled inpArgTys inpsAsExprs actualArgTys
                        let inpCloVarType = (mk_fun_ty (mk_tupled_ty g actualArgTys) cloVar.Type)
                        let newResTy = mk_fun_ty inpArgTy resTy
                        let inpCloVar,inpCloVarAsExpr = mk_compgen_local appm ("clo"^string i) inpCloVarType
                        let newRes = 
                            // For the final arg we can skip introducing the dummy variable
                            if i = N - 1 then 
                                mk_multi_lambda appm inpsAsVars 
                                    (mk_appl g ((inpCloVarAsExpr,inpCloVarType),[],[inpsAsActualArg],appm),resTy)
                            else
                                mk_multi_lambda appm inpsAsVars 
                                    (mk_invisible_let appm cloVar 
                                       (mk_appl g ((inpCloVarAsExpr,inpCloVarType),[],[inpsAsActualArg],appm)) 
                                       res, 
                                     resTy)
                            
                        inpCloVar,newRes,newResTy)
                    argTysWithoutNiceNames
                    (resVar,resVarAsExpr,retTy)

            
            // Mark the up as Some/None
            let suppliedArgs = List.map Some suppliedArgs @ List.of_array (Array.create (curriedNiceNames.Length - suppliedArgs.Length) None)

            assert (suppliedArgs.Length = curriedNiceNames.Length)

            let exprForAllArgs = 

                if isNil argTysWithNiceNames then 
                    mk_invisible_let appm cloVar exprWithActualTy exprForOtherArgs
                else
                    let lambdaBuilders,binderBuilders,inpsAsArgs = 
                    
                        (argTysWithNiceNames,curriedNiceNames,suppliedArgs) |||> List.map3 (fun (i,inpArgTy,actualArgTys) niceNames suppliedArg -> 

                                let inpArgTys = 
                                    match actualArgTys with 
                                    | [_] -> [inpArgTy]
                                    | _ -> dest_tuple_typ g inpArgTy


                                /// Note: there might not be enough nice names, and they might not match in arity
                                let niceNames = 
                                    match niceNames with 
                                    | nms when nms.Length = inpArgTys.Length -> nms
                                    | [nm] -> inpArgTys |> List.mapi (fun i _ -> (nm^string i))
                                    | nms -> nms
                                match suppliedArg with 
                                | Some arg -> 
                                    let binderBuilder,inpsAsActualArg = CoerceTupled niceNames arg actualArgTys
                                    let lambdaBuilder = (fun tm -> tm)
                                    lambdaBuilder, binderBuilder,inpsAsActualArg
                                | None -> 
                                    let inpsAsVars,inpsAsExprs = (niceNames,inpArgTys)  ||> List.map2 (fun nm ty -> mk_compgen_local appm nm ty)  |> List.unzip
                                    let inpsAsActualArg = CoerceDetupled inpArgTys inpsAsExprs actualArgTys
                                    let lambdaBuilder = (fun tm -> mk_multi_lambda appm inpsAsVars (tm, type_of_expr g tm))
                                    let binderBuilder = (fun tm -> tm)
                                    lambdaBuilder,binderBuilder,inpsAsActualArg)
                        |> List.unzip3
                    
                    // If no trailing args then we can skip introducing the dummy variable
                    // This corresponds to 
                    //    let f (x:A) = 1      
                    //
                    //   f ~~> type B -> int
                    //
                    // giving
                    //   (fun b -> f (b :> A))
                    // rather than 
                    //   (fun b -> let clo = f (b :> A) in clo)   
                    let exprApp = 
                        if argTysWithoutNiceNames.Length = 0 then 
                            mk_appl g ((exprWithActualTy,actualTy),[],inpsAsArgs,appm)
                        else
                            mk_invisible_let appm 
                                    cloVar (mk_appl g ((exprWithActualTy,actualTy),[],inpsAsArgs,appm)) 
                                    exprForOtherArgs

                    List.foldBack (fun f acc -> f acc) binderBuilders 
                        (List.foldBack (fun f acc -> f acc) lambdaBuilders exprApp)

            Some(exprForAllArgs,droppedSuppliedArgs)
    | _ -> 
        None
  
/// Find and make all subsumption eliminations 
let NormalizeAndAdjustPossibleSubsumptionExprs g inputExpr = 
    let expr,args = 
        // AdjustPossibleSubsumptionExpr can take into account an application
        match strip_expr inputExpr with 
        | TExpr_app(f,fty,[],args,m)  ->
             f,args

        | expr -> 
            inputExpr,[]
    
    match AdjustPossibleSubsumptionExpr g expr args with 
    | None -> 
        inputExpr
    | Some (expr',[]) -> 
        expr'
    | Some (expr',args') -> 
        //printfn "adjusted...." 
        TExpr_app(expr',type_of_expr g expr',[],args',range_of_expr inputExpr)  
             
  
//---------------------------------------------------------------------------
// LinearizeTopMatch - when only one non-failing target, make linear.  The full
// complexity of this is only used for spectacularly rare bindings such as 
//    type ('a,'b) either = This of 'a | That of 'b
//    let this_f1 = This (fun x -> x)
//    let This fA | That fA = this_f1
// 
// Here a polymorphic top level binding "fA" is _computed_ by a pattern match!!!
// The TAST coming out of type checking must, however, define fA as a type function,
// since it is marked with an arity that indicates it's r.h.s. is a type function]
// without side effects and so can be compiled as a generic method (for example).

// polymorphic things bound in complex matches at top level require eta expansion of the 
// type function to ensure the r.h.s. of the binding is indeed a type function 
let tlambda_eta g m tps (tm,ty) = 
  if isNil tps then tm else mk_tlambda m tps (mk_appl g ((tm,ty),[(List.map mk_typar_ty tps)],[],m),ty)

let AdjustValToTopVal (tmp:Val) parent valData =
        tmp.Data.val_top_repr_info <- Some valData ;  
        tmp.Data.val_actual_parent <- parent;  
        set_is_topbind_of_vflags tmp.Data true

/// For match with only one non-failing target T0, the other targets, T1... failing (say, raise exception).
///   tree, T0(v0,..,vN) => rhs ; T1() => fail ; ...
/// Convert it to bind T0's variables, then continue with T0's rhs:
///   let tmp = switch tree, TO(fv0,...,fvN) => Tup (fv0,...,fvN) ; T1() => fail; ...
///   let v1  = #1 tmp in ...
///   and vN  = #N tmp
///   rhs
/// Motivation:
/// - For top-level let bindings with possibly failing matches,
///   this makes clear that subsequent bindings (if reached) are top-level ones.
let LinearizeTopMatchAux g parent  (spBind,m,tree,targets,m2,ty) =
    let targetsL = Array.to_list targets
    (* items* package up 0,1,more items *)
    let itemsProj tys i x = 
        match tys with 
        | []  -> failwith "itemsProj: no items?"
        | [t] -> x (* no projection needed *)
        | tys -> TExpr_op(TOp_tuple_field_get(i),tys,[x],m)
    let isThrowingTarget = function TTarget(_,x,_) -> is_throw x
    if 1 + List.count isThrowingTarget targetsL = targetsL.Length then
        (* Have failing targets and ONE successful one, so linearize *)
        let (TTarget (vs,rhs,spTarget)) = the (List.tryfind (isThrowingTarget >> not) targetsL)
        (* note - old code here used copy value to generate locals - this was not right *)
        let fvs      = vs |> FlatList.map (fun v -> fst(mk_local v.Range v.MangledName v.Type)) |> FlatList.to_list  (* fresh *)
        let vtys     = vs |> List.map (fun v -> v.Type) 
        let tmpTy    = mk_tupled_vars_ty g vs
        let tmp,tmpe = mk_compgen_local m "matchResultHolder" tmpTy

        AdjustValToTopVal tmp parent TopValInfo.emptyValData;  

        let newTg    = TTarget (fvs,mk_tupled_vars g m fvs,spTarget)
        let fixup (TTarget (tvs,tx,spTarget)) = 
           match dest_throw tx with
           | Some (m,ty,e) -> let tx = mk_throw m tmpTy e
                              TTarget(tvs,tx,spTarget) (* Throwing targets, recast it's "return type" *)
           | None          -> newTg       (* Non-throwing target,  replaced [new/old] *)
       
        let targets  = Array.map fixup targets
        let binds    = 
            vs |> FlatList.mapi (fun i v -> 
                let ty = v.Type
                let rhs =  tlambda_eta g m  v.Typars (itemsProj vtys i tmpe, ty)
                (* update the arity of the value *)
                v.Data.val_top_repr_info <- Some (InferArityOfExpr g ty [] [] rhs);  
                mk_invisible_bind v rhs)  in (* vi = proj tmp *)
        mk_compgen_let m
          tmp (prim_mk_match (spBind,m,tree,targets,m2,tmpTy)) (* note, probably retyped match, but note, result still has same type *)
          (mk_lets_from_Bindings m binds rhs)                             
    else
        (* no change *)
        prim_mk_match (spBind,m,tree,targets,m2,ty)

let LinearizeTopMatch g parent = function
  | TExpr_match (spBind,m,tree,targets,m2,ty,cache) -> LinearizeTopMatchAux g parent (spBind,m,tree,targets,m2,ty)
  | x -> x


(*---------------------------------------------------------------------------
 * XmlDoc signatures
 *------------------------------------------------------------------------- *)


let commaEncs strs  = String.concat "," strs
let angleEnc  str   = "{" ^ str ^ "}" 
let ticks_and_argcount_text_of_tcref (tcref:TyconRef) = 
     let nm = tcref.MangledName
     text_of_path (Array.to_list (full_mangled_path_to_tcref tcref) @ [nm]) 

let typarEnc g gtps typar = 
    let idx = 
        try ListSet.findIndex typar_ref_eq  typar gtps
        with Not_found -> warning(InternalError("Typar not found during XmlDoc generation",typar.Range)); 0
    "``"^string idx 

let rec typeEnc  g gtps ty = 
    if verbose then  dprintf "--> typeEnc";
    match (strip_tpeqns_and_tcabbrevs g ty) with 
    | TType_forall (typars,typ) -> 
        "Microsoft.FSharp.Core.TypeFunc"
    | _ when is_compat_array_typ g ty    -> 
        let tcref,tinst = dest_stripped_tyapp_typ g ty
        typeEnc g gtps (List.hd tinst)^ "[]"
    | _ when is_il_arr_typ g ty   -> 
        let tcref,tinst = dest_stripped_tyapp_typ g ty
        typeEnc g gtps (List.hd tinst)^ tcref.MangledName
    | TType_ucase (UCRef(tcref,_),tinst)   
    | TType_app (tcref,tinst)   -> 
        ticks_and_argcount_text_of_tcref tcref ^ tyargsEnc g gtps tinst
    | TType_tuple typs          -> 
        sprintf "Microsoft.FSharp.Core.Tuple`%d%s" typs.Length (tyargsEnc g gtps typs)
    | TType_fun (f,x)           -> 
        "Microsoft.FSharp.Core.FastFunc`2" ^ tyargsEnc g gtps [f;x]
    | TType_var typar           -> 
        typarEnc g gtps typar
    | TType_modul_bindings  -> 
        "System.Object"
    | TType_measure unt -> "?"

and tyargsEnc  g gtps args = 
     if isNil args then ""
     else angleEnc (commaEncs (List.map (typeEnc g gtps) args)) 

let XmlDocArgsEnc g gtps argTs =
  if isNil argTs then "" 
  else "(" ^ String.concat "," (List.map (typeEnc g gtps) argTs) ^ ")"

let XmlDocSigOfVal g path (v:Val) =
  let tps,methTypars,argInfos,prefix,path,name = 

    // CLEANUP: this is one of several code paths that treat module values and members 
    // seperately when really it would be cleaner to make sure GetTopValTypeInFSharpForm, GetMemberTypeInFSharpForm etc.
    // were lined up so code paths like this could be uniform
    
    match v.MemberInfo with 
    | Some membInfo when not v.IsExtensionMember -> 
        (* Methods, Properties etc. *)
        let tps,argInfos,rtnT,_ = GetMemberTypeInMemberForm g membInfo.MemberFlags (the v.TopValInfo) v.Type v.Range
        let prefix,name = 
          match membInfo.MemberFlags.MemberKind with 
          | MemberKindClassConstructor 
          | MemberKindConstructor 
          | MemberKindMember -> "M:", v.CompiledName
          | MemberKindPropertyGetSet 
          | MemberKindPropertySet
          | MemberKindPropertyGet -> "P:",membInfo.PropertyName
        let path = 
          path^"."^ v.MemberActualParent.MangledName
        let methTypars = 
          match PartitionValTypars g v with
          | Some(_,_,memberMethodTypars,_,_) ->  memberMethodTypars
          | None -> tps
        tps,methTypars,argInfos,prefix,path,name
    | _ ->
        // Regular F# values and extension members 
        let w = arity_of_val v
        let tps,argInfos,_,_ = GetTopValTypeInCompiledForm g w v.Type v.Range
        let name = v.CompiledName
        let prefix =
          if  w.NumCurriedArgs = 0 && isNil tps then "P:"
          else "M:"
        tps,tps,argInfos,prefix,path,name
  let argTs = argInfos |> List.concat |> List.map fst
  let args = XmlDocArgsEnc g tps argTs
  let arity = List.length methTypars in (* C# XML doc adds ``<arity> to *generic* member names *)
  let genArity = if arity=0 then "" else Printf.sprintf "``%d" arity
  prefix ^ path ^ "." ^ name ^ genArity ^ args

let XmlDocSigOfTycon (g:TcGlobals) path (tc:Tycon) = "T:" ^ path ^ "." ^ tc.MangledName 
let XmlDocSigOfSubModul (g:TcGlobals) path = "T:" ^ path 


(*--------------------------------------------------------------------------
!* Some unions have null as representations 
 *------------------------------------------------------------------------*)


let enum_CompilationRepresentationAttribute_Static             = 0b0000000000000001
let enum_CompilationRepresentationAttribute_Instance           = 0b0000000000000010
let enum_CompilationRepresentationAttribute_StaticInstanceMask = 0b0000000000000011
let enum_CompilationRepresentationAttribute_ModuleSuffix       = 0b0000000000000100
let enum_CompilationRepresentationAttribute_PermitNull         = 0b0000000000001000

let TyconHasUseNullAsTrueValueAttribute g (tycon:Tycon) =
     match TryFindInt32Attrib  g g.attrib_CompilationRepresentationAttribute tycon.Attribs with
     | Some(flags) -> ((flags &&& enum_CompilationRepresentationAttribute_PermitNull) <> 0)
     | _ -> false 


(* WARNING: this must match optimizeAlternativeToNull in ilx/cu_erase.ml *) 
(* REVIEW: make this fully attribute controlled *)
let IsUnionTypeWithNullAsTrueValue (g:TcGlobals) (tycon:Tycon) =
  (tycon.IsUnionTycon && 
   let ucs = tycon.UnionCasesArray
   (Array.length ucs = 0 ||
     (TyconHasUseNullAsTrueValueAttribute g tycon &&
      ucs |> Array.existsOne (fun uc -> uc.IsNullary) &&
      ucs |> Array.exists (fun uc -> not uc.IsNullary))))

let TyconCompilesInstanceMembersAsStatic g tycon = IsUnionTypeWithNullAsTrueValue g tycon
let TcrefCompilesInstanceMembersAsStatic g tcref = TyconCompilesInstanceMembersAsStatic g (deref_tycon tcref)

let TypeNullNever g ty = 
    let underlyingTy = strip_tpeqns_and_tcabbrevs_and_measureable g ty
    (is_struct_typ g underlyingTy) ||
    (is_byref_typ g underlyingTy)

let TypeNullIsExtraValue g ty = 
    is_il_ref_typ g ty ||
    is_delegate_typ g ty //||
        //(not (TypeNullNever g ty) && 
         //is_stripped_tyapp_typ ty && 
         //HasAttrib  g g.attrib_PermitNullLiteralAttribute (deref_tycon (tcref_of_stripped_typ ty)).Attribs)

let TypeNullIsTrueValue g ty = 
    (is_stripped_tyapp_typ g ty && IsUnionTypeWithNullAsTrueValue g (deref_tycon (tcref_of_stripped_typ g ty)))  ||
    (is_unit_typ g ty)

let TypeNullNotLiked g ty = 
       not (TypeNullIsExtraValue g ty) 
    && not (TypeNullIsTrueValue g ty) 
    && not (TypeNullNever g ty) 

let TypeSatisfiesNullConstraint g ty = 
    TypeNullIsExtraValue g ty  

let rec TypeHasDefaultValue g ty = 
    TypeSatisfiesNullConstraint g ty  
    || (is_struct_typ g ty &&
        // Is it an F# struct type?
        (if is_fsobjmodel_struct_typ g ty then 
            let tcref,tinst = dest_stripped_tyapp_typ g ty 
            let flds = 
                tcref.TrueInstanceFieldsAsList
                  // We can ignore fields with the DefaultValue(false) attribute 
                  |> List.filter (fun fld -> not (TryFindBoolAttrib g g.attrib_DefaultValueAttribute fld.FieldAttribs = Some(false)))

            flds |> List.forall (typ_of_rfield (mk_tcref_inst tcref tinst) >> TypeHasDefaultValue g)
         elif is_tuple_struct_typ g ty then 
            dest_tuple_typ g ty |> List.forall (TypeHasDefaultValue g)
         else
            // All struct types defined in other .NET languages have a DefaultValue regardless of their
            // instantiation
            true))

(* Can we use the fast helper for the 'LanguagePrimitives.IntrinsicFunctions.TypeTestGeneric'? *)
let can_use_istype_fast g ty = 
     not (is_typar_typ g ty) && 
     not (TypeNullIsTrueValue g ty) && 
     not (TypeNullNever g ty)

(* Can we use the fast helper for the 'LanguagePrimitives.IntrinsicFunctions.UnboxGeneric'? *)
let can_use_unbox_fast g ty = 
     not (is_typar_typ g ty) && 
     not (TypeNullNotLiked g ty)
     
     
(*--------------------------------------------------------------------------
!* Nullness tests and pokes 
 *------------------------------------------------------------------------*)

// Null tests are generated by
//    1. The compilation of array patterns in the pattern match compiler
//    2. The compilation of string patterns in the pattern match compiler

let mk_nonnull_test g m e = mk_asm ([ IL.I_arith IL.AI_ldnull ; IL.I_arith IL.AI_cgt_un  ],[],  [e],[g.bool_ty],m)
let mk_nonnull_poke g m e = mk_asm ([ IL.I_arith IL.AI_dup ; IL.I_ldvirtftn (mspec_Object_GetHashCode g.ilg); IL.I_arith IL.AI_pop  ],[],  [e],[type_of_expr g e],m)
let mk_nonnull_cond g m ty e1 e2 e3 = mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m ty (mk_nonnull_test g m e1) e2 e3


let ModuleNameIsMangled g attrs =
    match TryFindInt32Attrib g g.attrib_CompilationRepresentationAttribute attrs with
    | Some(flags) -> ((flags &&& enum_CompilationRepresentationAttribute_ModuleSuffix) <> 0)
    | _ -> false 

let CompileAsEvent g attrs = HasAttrib g g.attrib_CLIEventAttribute attrs 


let MemberIsCompiledAsInstance g parent isExtensionMember membInfo attrs =
    // All extension members are compiled as static members
    if isExtensionMember then false
    // Anything implementing a dispatch slot is compiled as an instance member
    elif nonNil membInfo.ImplementedSlotSigs then true
    else 
        // Otherwise check attributes to see if there is an explicit instance or explicit static flag
        let explicitInstance,explicitStatic = 
            match TryFindInt32Attrib g g.attrib_CompilationRepresentationAttribute attrs with
            | Some(flags) -> 
              ((flags &&& enum_CompilationRepresentationAttribute_Instance) <> 0),
              ((flags &&& enum_CompilationRepresentationAttribute_Static) <> 0)
            | _ -> false,false
        explicitInstance ||
        (membInfo.MemberFlags.MemberIsInstance &&
         not explicitStatic &&
         not (TcrefCompilesInstanceMembersAsStatic g parent))


let is_sealed_typ g ty =
  let ty = strip_tpeqns_and_tcabbrevs_and_measureable g ty
  not (is_ref_typ g ty) ||
  is_any_array_typ g ty || 
  (if is_il_named_typ g ty then 
      let tcref,tinst = dest_stripped_tyapp_typ g ty
      let tdef = tcref.ILTyconRawMetadata
      tdef.tdSealed
   elif (is_fsobjmodel_interface_typ g ty || is_fsobjmodel_class_typ g ty) then 
      let tcref,tinst = dest_stripped_tyapp_typ g ty
      (TryFindBoolAttrib g g.attrib_SealedAttribute tcref.Attribs = Some(true))
   else true)
   
let IsComInteropType g ty =
    let tcr,_ = dest_stripped_tyapp_typ g ty
    TryFindBoolAttrib g g.attrib_ComImportAttribute tcr.Attribs = Some(true)
  
let ValSpecIsCompiledAsInstance g (v:Val) =
    match v.MemberInfo with 
    | Some(membInfo) -> 
        // Note it doesn't matter if we pass 'v.MemberActualParent' or 'v.MemberApparentParent' here. 
        // These only differ if the value is an extension member, and in that case MemberIsCompiledAsInstance always returns 
        // false anyway 
        MemberIsCompiledAsInstance g v.MemberApparentParent v.IsExtensionMember membInfo v.Attribs  
    |  _ -> false

let ValRefIsCompiledAsInstanceMember g vref = ValSpecIsCompiledAsInstance g (deref_val vref)


(*---------------------------------------------------------------------------
 * Crack information about an F# object model call
 *------------------------------------------------------------------------- *)

let GetMemberCallInfo g (vref:ValRef,vFlags) = 
    match vref.MemberInfo with 
    | Some(membInfo) when not vref.IsExtensionMember -> 
      let numEnclTypeArgs = vref.MemberApparentParent.TyparsNoRange.Length
      let virtualCall = 
        (membInfo.MemberFlags.MemberIsVirtual || 
         membInfo.MemberFlags.MemberIsOverrideOrExplicitImpl || 
         membInfo.MemberFlags.MemberIsDispatchSlot) && 
        not membInfo.MemberFlags.MemberIsFinal && 
        not (vFlags = VSlotDirectCall)
      let isNewObj    = (membInfo.MemberFlags.MemberKind = MemberKindConstructor) && (vFlags = NormalValUse)
      let isSuperInit = (membInfo.MemberFlags.MemberKind = MemberKindConstructor) && (vFlags = CtorValUsedAsSuperInit)
      let isSelfInit  = (membInfo.MemberFlags.MemberKind = MemberKindConstructor) && (vFlags = CtorValUsedAsSelfInit)
      let isCompiledAsInstance = ValRefIsCompiledAsInstanceMember g vref
      let takesInstanceArg = isCompiledAsInstance && not isNewObj
      let isPropGet = (membInfo.MemberFlags.MemberKind = MemberKindPropertyGet) && (membInfo.MemberFlags.MemberIsInstance = isCompiledAsInstance)
      let isPropSet = (membInfo.MemberFlags.MemberKind = MemberKindPropertySet) && (membInfo.MemberFlags.MemberIsInstance = isCompiledAsInstance)
      numEnclTypeArgs, virtualCall,isNewObj,isSuperInit,isSelfInit ,takesInstanceArg,isPropGet,isPropSet
    | _ -> 
      0,false,false,false,false,false,false,false

(*---------------------------------------------------------------------------
 * Active pattern name helpers
 *------------------------------------------------------------------------- *)

let core_display_name_of_vref (vref:ValRef) = vref.CoreDisplayName

let is_ap_name(nm) =
    // REVIEW: This may not be correct in all the cases. For example, is "|+|" valid AP name?
    let len = String.length nm
    String.contains nm '|' &&
      (String.index nm '|' = 0) &&
      len >= 3 &&
      (String.rindex nm '|' = len - 1)
    
let apinfo_of_vname (nm, m) = 
    let rec loop nm = 
        if String.contains nm '|' then 
           let n = String.index nm '|'
           String.sub nm 0 n :: loop (String.sub nm (n+1) (String.length nm - n - 1))
        else
           [nm]
    let nm = DecompileOpName nm
    let len = String.length nm
    if is_ap_name nm then 
          let res = loop (String.sub nm 1 (len - 2))
          let resH,resT = List.frontAndBack res
          Some(if resT = "_" then APInfo(false,resH,m) else APInfo(true,res,m))
        (* dprintf "apinfo_of_vname %s, res = %s\n" nm (String.concat ";" res);*)
    else None

let apinfo_of_vref (vref:ValRef) =  
    // This next line is an optimization to prevent calls to core_display_name_of_vref, which calls DemangleOperatorName
    if not (String.hasPrefix vref.MangledName "|" ) then None else 
    apinfo_of_vname (vref.CoreDisplayName, vref.Range)

let name_of_apref (APElemRef(_,vref,n)) =
    match apinfo_of_vref vref with
    | None -> error(InternalError("name_of_apref: not an active pattern name", vref.Range))
    | Some (APInfo(total,nms,_)) -> 
        if n < 0 || n >= List.length nms  then error(InternalError("name_of_apref: index out of range for active pattern refernce", vref.Range));
        List.nth nms n

let mk_choices_tcref g m n = 
     match n with 
     | 0 | 1 -> error(InternalError("mk_choices_tcref",m))
     | 2 -> g.choice2_tcr
     | 3 -> g.choice3_tcr
     | 4 -> g.choice4_tcr
     | 5 -> g.choice5_tcr
     | 6 -> g.choice6_tcr
     | 7 -> g.choice7_tcr
     | _ -> error(Error("active patterns may not return more than 7 possibilities",m))
let mk_choices_typ g m tinst = 
     match List.length tinst with 
     | 0 -> g.unit_ty
     | 1 -> List.hd tinst
     | _ -> mk_tyapp_ty (mk_choices_tcref g m (List.length tinst)) tinst

let mk_choices_ucref g m n i = 
     mk_ucref (mk_choices_tcref g m n) ("Choice"^string (i+1)^"Of"^string n)

let names_of_apinfo (APInfo(_,nms,_)) = nms
let total_of_apinfo (APInfo(total,_,_)) = total

let mk_apinfo_result_typ g m apinfo rtys = 
    let choicety = mk_choices_typ g m rtys
    if total_of_apinfo apinfo then choicety else mk_option_ty g choicety
    
let mk_apinfo_typ g m apinfo dty rtys = mk_fun_ty dty (mk_apinfo_result_typ g m apinfo rtys)
    

(*---------------------------------------------------------------------------
!* RewriteExpr: rewrite bottom up with interceptors 
 *-------------------------------------------------------------------------*)

type ExprRewritingEnv = 
    { pre_intercept: ((expr -> expr) -> expr -> expr option) option;
      post_transform: expr -> expr option;
      under_quotations: bool }    

let rec rewrite_bind env (TBind(v,e,letSeqPtOpt) as bind) = TBind(v,RewriteExpr env e,letSeqPtOpt) 

and rewrite_binds env binds = FlatList.map (rewrite_bind env) binds

and RewriteExpr env expr =
  match expr with 
  | TExpr_let _ 
  | TExpr_seq _ ->
      rewrite_linear_expr env expr (fun e -> e)
  | _ -> 
      let expr = 
         match pre_rewrite_expr env expr with 
         | Some expr -> expr
         | None -> rewrite_expr_structure env expr
      post_rewrite_expr env expr 

and pre_rewrite_expr env expr = 
     match env.pre_intercept  with 
     | Some f -> f (RewriteExpr env) expr
     | None -> None 
and post_rewrite_expr env expr = 
     match env.post_transform expr with 
     | None -> expr 
     | Some expr -> expr 

and rewrite_expr_structure env expr =  
  match expr with
  | TExpr_const _ 
  | TExpr_val _ -> expr
  | TExpr_app(f0,f0ty,tyargs,args,m) -> 
      let f0'   = RewriteExpr env f0
      let args' = rewrite_exprs env args
      if f0 == f0' && args == args' then expr
      else TExpr_app(f0',f0ty,tyargs,args',m)

  | TExpr_quote(ast,{contents=Some(argTypes,argExprs,data)},m,ty) -> 
      TExpr_quote((if env.under_quotations then RewriteExpr env ast else ast),{contents=Some(argTypes,rewrite_exprs env argExprs,data)},m,ty)
  | TExpr_quote(ast,{contents=None},m,ty) -> 
      TExpr_quote((if env.under_quotations then RewriteExpr env ast else ast),{contents=None},m,ty)
  | TExpr_obj (_,ty,basev,basecall,overrides,iimpls,m,_) -> 
      mk_obj_expr(ty,basev,RewriteExpr env basecall,List.map (rewrite_override env) overrides,
                  List.map (rewrite_iimpl env) iimpls,m)
  | TExpr_link eref -> 
      RewriteExpr env !eref
  | TExpr_op (c,tyargs,args,m) -> 
      let args' = rewrite_exprs env args
      if args == args' then expr 
      else TExpr_op(c,tyargs,args',m)
  | TExpr_lambda(lambda_id,basevopt,argvs,body,m,rty,_) -> 
      let body = RewriteExpr env body
      mk_basev_multi_lambda m basevopt argvs (body,rty)
  | TExpr_tlambda(lambda_id,argtyvs,body,m,rty,_) -> 
      let body = RewriteExpr env body
      mk_tlambda m argtyvs (body,rty)
  | TExpr_match(spBind,exprm,dtree,targets,m,ty,_) -> 
      let dtree' = rewrite_dtree env dtree
      let targets' = rewrite_targets env targets
      mk_and_optimize_match spBind exprm m ty dtree' targets'
  | TExpr_letrec (binds,e,m,_) ->
      let binds = rewrite_binds env binds
      let e' = RewriteExpr env e
      TExpr_letrec(binds,e',m,NewFreeVarsCache())
  | TExpr_let _ -> failwith "unreachable - linear let"
  | TExpr_seq _ -> failwith "unreachable - linear seq"
  | TExpr_static_optimization (constraints,e2,e3,m) ->
      let e2' = RewriteExpr env e2
      let e3' = RewriteExpr env e3
      TExpr_static_optimization(constraints,e2',e3',m)
  | TExpr_tchoose (a,b,m) -> 
      TExpr_tchoose(a,RewriteExpr env b,m)
and rewrite_linear_expr env expr contf =
    (* schedule a rewrite on the way back up by adding to the continuation *)
    let contf = contf << post_rewrite_expr env
    match pre_rewrite_expr env expr with 
    | Some expr -> contf expr  (* done - intercepted! *)
    | None -> 
        match expr with 
        | TExpr_let (bind,body,m,_) ->  
            let bind = rewrite_bind env bind
            rewrite_linear_expr env body (contf << (fun body' ->
                mk_let_bind m bind body'))
        | TExpr_seq  (e1,e2,dir,spSeq,m) ->
            let e1' = RewriteExpr env e1
            rewrite_linear_expr env e2 (contf << (fun e2' ->
                if e1 == e1' && e2 == e2' then expr 
                else TExpr_seq(e1',e2',dir,spSeq,m)))
        | _ -> 
            (* no longer linear *)
            contf (RewriteExpr env expr) 

and rewrite_exprs env exprs = List.mapq (RewriteExpr env) exprs
and rewrite_FlatExprs env exprs = FlatList.mapq (RewriteExpr env) exprs

and rewrite_dtree env x =
  match x with 
  | TDSuccess (es,n) -> 
      let es' = rewrite_FlatExprs env es
      if FlatList.physicalEquality es es' then x 
      else TDSuccess(es',n)
  | TDSwitch (e,cases,dflt,m) ->
      let e' = RewriteExpr env e
      let cases' = List.map (fun (TCase(discrim,e)) -> TCase(discrim,rewrite_dtree env e)) cases
      let dflt' = Option.map (rewrite_dtree env) dflt
      TDSwitch (e',cases',dflt',m)
  | TDBind (bind,body) ->
      let bind' = rewrite_bind env bind
      let body = rewrite_dtree env body
      TDBind (bind',body)
and rewrite_target env (TTarget(vs,e,spTarget)) = TTarget(vs,RewriteExpr env e,spTarget)
and rewrite_targets env targets = List.map (rewrite_target env) (Array.to_list targets)

and rewrite_override env (TObjExprMethod(slotsig,tps,vs,e,m)) =
  TObjExprMethod(slotsig,tps,vs,RewriteExpr env e,m)
and rewrite_iimpl env (ty,overrides) = 
  (ty, List.map (rewrite_override env) overrides)
    
and rewrite_mexpr env x = 
    match x with  
    (* | TMTyped(mty,e,m) -> TMTyped(mty,rewrite_mexpr env e,m) *)
    | TMTyped(mty,def,m) ->  TMTyped(mty,rewrite_mdef env def,m)
and rewrite_mdefs env x = List.map (rewrite_mdef env) x
    
and rewrite_mdef env x = 
    match x with 
    | TMDefRec(tycons,binds,mbinds,m) -> TMDefRec(tycons,rewrite_binds env binds,rewrite_mbinds env mbinds,m)
    | TMDefLet(bind,m)         -> TMDefLet(rewrite_bind env bind,m)
    | TMDefDo(e,m)             -> TMDefDo(RewriteExpr env e,m)
    | TMDefs(defs)             -> TMDefs(rewrite_mdefs env defs)
    | TMAbstract(mexpr)        -> TMAbstract(rewrite_mexpr env mexpr)
and rewrite_mbind env (TMBind(nm, rhs)) = TMBind(nm,rewrite_mdef env rhs)
and rewrite_mbinds env mbinds = List.map (rewrite_mbind env) mbinds

and RewriteImplFile env mv = mapTImplFile (rewrite_mexpr env) mv


let is_flag_enum_typ (g:TcGlobals) typ = 
   (is_enum_typ g typ (* && TyconRefHasAttrib g g.attrib_FlagsAttribute (tcref_of_stripped_typ typ) *) ) 

(*--------------------------------------------------------------------------
!* Build a mrpi that converts all "local" references to "public" things 
 * to be non local references.
 *------------------------------------------------------------------------ *)

let MakeExportRemapping viewedCcu = 

    let acc_entity_remap (tycon:Tycon) tmenv = 
        match tycon.PublicPath with 
        | Some pubpath -> 
            if !verboseStamps then dprintf "adding export remapping for tycon %s#%d\n" tycon.MangledName tycon.Stamp;
            let tcref = rescope_tycon_pubpath viewedCcu pubpath tycon
            tmenv_add_tcref_remap (mk_local_tcref tycon) tcref tmenv
        | None -> error(InternalError("Unexpected tycon without a pubpath when remapping assembly data",tycon.Range))

    let acc_val_remap (vspec:Val) tmenv = 
        match vspec.PublicPath with 
        | Some pubpath -> 
            if !verboseStamps then dprintf "adding export remapping for value %s#%d\n" vspec.MangledName vspec.Stamp;
            {tmenv with vspec_remap=vspec_map_add vspec (rescope_val_pubpath viewedCcu pubpath vspec) tmenv.vspec_remap}
        | None -> error(InternalError("Unexpected value without a pubpath when remapping assembly data",vspec.Range))

    fun (mspec:ModuleOrNamespace) -> 
        fold_vals_and_tycons_of_mtyp acc_entity_remap acc_val_remap mspec.ModuleOrNamespaceType empty_expr_remap

(*--------------------------------------------------------------------------
!* Apply a "local to nonlocal" renaming to a module type.  This can't use
 * remap_mspec since the remapping we want isn't to newly created nodes
 * but rather to remap to the nonlocal references. This is deliberately 
 * "breaking" the binding structure implicit in the module type, which is
 * the whole point - one things are rewritten to use non local references then
 * the elements can be copied at will, e.g. when inlining during optimization.
 *------------------------------------------------------------------------ *)


let rec remap_tycon_data_to_nonlocal g tmenv d = 
    let tps',tmenvinner = tmenv_copy_remap_and_bind_typars (remap_attrib g tmenv) tmenv (d.entity_typars.Force(d.entity_range))

    { d with 
          entity_typars         = LazyWithContext.NotLazy tps';
          entity_attribs        = d.entity_attribs        |> remap_attribs g tmenvinner;
          entity_tycon_repr           = d.entity_tycon_repr           |> Option.map (remap_tycon_repr g tmenvinner);
          entity_tycon_abbrev         = d.entity_tycon_abbrev         |> Option.map (remap_type tmenvinner) ;
          entity_tycon_tcaug          = d.entity_tycon_tcaug          |> remap_tcaug tmenvinner ;
          entity_modul_contents = 
              notlazy (d.entity_modul_contents 
                       |> Lazy.force 
                       |> map_immediate_vals_and_tycons_of_modtyp (remap_tycon_to_nonlocal g tmenv) 
                                                                  (remap_val_to_nonlocal g tmenv));
          entity_exn_info      = d.entity_exn_info      |> remap_tycon_exnc_info g tmenvinner}

and remap_tycon_to_nonlocal g tmenv x = 
    x |> NewModifiedTycon (remap_tycon_data_to_nonlocal g tmenv)  

and remap_val_to_nonlocal g  tmenv inp = 
    inp |> NewModifiedVal (remap_val_data g tmenv)

let ApplyExportRemappingToEntity g tmenv x = remap_tycon_to_nonlocal g tmenv x

(* Which constraints actually get compiled to .NET constraints? *)
let is_compiled_constraint cx = 
    match cx with 
      | TTyparIsNotNullableValueType _ 
      | TTyparIsReferenceType _
      | TTyparRequiresDefaultConstructor _
      | TTyparCoercesToType _ -> true
      | _ -> false
    
// Is a value a first-class polymorphic value with .NET constraints? 
// Used to turn off TLR and method splitting
let IsGenericValWithGenericContraints g (v:Val) = 
    is_forall_typ g v.Type && 
    v.Type |> dest_forall_typ g |> fst |> List.exists (fun tp -> List.exists is_compiled_constraint tp.Constraints)

(* Does a type support a given interface? *)
let tcaug_has_interface g tcaug ty = 
    List.exists (fun (x,_,_) -> type_equiv g ty x)  tcaug.tcaug_implements

(* Does a type have an override matching the given name and argument types? *)
(* Used to detet the presence of 'Equals' and 'GetHashCode' in type checking *)
let tcaug_has_override g tcaug nm argtys = 
    tcaug.tcaug_adhoc 
    |> NameMultiMap.find nm
    |> List.exists (fun vref -> 
                      match vref.MemberInfo with 
                      | None -> false 
                      | Some membInfo -> 
                                     let argInfos = ArgInfosOfMember g vref 
                                     argInfos.Length = 1 && 
                                     List.lengthsEqAndForall2 (type_equiv g) (List.map fst (List.hd argInfos)) argtys  &&  
                                     membInfo.MemberFlags.MemberIsOverrideOrExplicitImpl) 

let mk_fast_for_loop g (spLet,m,idv:Val,start,dir,finish,body) =
    let dir = if dir then FSharpForLoopUp else FSharpForLoopDown 
    //let startv,starte   = mk_compgen_local (range_of_expr start)  "loopStart" g.int_ty
    //let finishv,finishe = mk_compgen_local (range_of_expr finish) "loopEnd" g.int_ty
    //mk_let spLet (range_of_expr start)  startv  start 
      //(mk_compgen_let (range_of_expr finish) finishv finish 
    mk_for g (spLet,idv,start,dir,finish,body,m)

let rec EvalConstantExpr g x = 
    match x with 

    (* Detect standard constants *)
    | TExpr_const(c,m,_) -> 
        match c with 
        | TConst_bool _ 
        | TConst_int32 _ 
        | TConst_sbyte  _
        | TConst_int16  _
        | TConst_int32 _
        | TConst_int64 _  
        | TConst_byte  _
        | TConst_uint16  _
        | TConst_uint32  _
        | TConst_uint64  _
        | TConst_float _
        | TConst_float32 _
        | TConst_char _
        | TConst_zero _
        | TConst_string _  -> x
        | _ -> 
            errorR (Error ( "This constant may not be used as a custom attribute value",m)); 
            x

    | TExpr_app(TExpr_val(vref,_,_),_,[ty],[],m) when (is_typeof_vref g vref || is_typedefof_vref g vref) -> 
        x
    | TExpr_op(TOp_coerce,_,[arg],_) -> 
        EvalConstantExpr g arg
    | TExpr_app(TExpr_val(vref,_,_),_,_,[arg1],_) when g.vref_eq vref g.enum_vref  -> 
        EvalConstantExpr g arg1
    (* Detect bitwise or of attribute flags: one case of constant folding (a more general treatment is needed *)    
    | BitwiseOr g (arg1,arg2) ->
        match EvalConstantExpr g arg1, EvalConstantExpr g arg2 with 
        | TExpr_const(TConst_int32 x1,m,ty), TExpr_const(TConst_int32 x2,_,_) -> TExpr_const(TConst_int32 (x1 ||| x2),m,ty)
        | TExpr_const(TConst_sbyte x1,m,ty), TExpr_const(TConst_sbyte x2,_,_) -> TExpr_const(TConst_sbyte (x1 ||| x2),m,ty)
        | TExpr_const(TConst_int16 x1,m,ty), TExpr_const(TConst_int16 x2,_,_) -> TExpr_const(TConst_int16 (x1 ||| x2),m,ty)
        | TExpr_const(TConst_int64 x1,m,ty), TExpr_const(TConst_int64 x2,_,_) -> TExpr_const(TConst_int64 (x1 ||| x2),m,ty)
        | TExpr_const(TConst_byte x1,m,ty), TExpr_const(TConst_byte x2,_,_) -> TExpr_const(TConst_byte (x1 ||| x2),m,ty)
        | TExpr_const(TConst_uint16 x1,m,ty), TExpr_const(TConst_uint16 x2,_,_) -> TExpr_const(TConst_uint16 (x1 ||| x2),m,ty)
        | TExpr_const(TConst_uint32 x1,m,ty), TExpr_const(TConst_uint32 x2,_,_) -> TExpr_const(TConst_uint32 (x1 ||| x2),m,ty)
        | TExpr_const(TConst_uint64 x1,m,ty), TExpr_const(TConst_uint64 x2,_,_) -> TExpr_const(TConst_uint64 (x1 ||| x2),m,ty)
        | _ -> x
    | _ -> 
        errorR (Error ( "This is not a constant expression or valid custom attribute value",range_of_expr x)); 
        x


let EvalAttribArg g x = 
    match x with 
    | TExpr_op(TOp_array,[elemTy],args,m) -> 
        let args = args |> List.map (EvalConstantExpr g) 
        TExpr_op(TOp_array,[elemTy],args,m) 
    | _ -> 
        EvalConstantExpr g x

// Take into account the fact that some "instance" members are compiled as static
// members when usinging CompilationRepresentation.Static, or any non-virtual instance members
// in a type that supports "null" as a true value. This is all members
// where ValRefIsCompiledAsInstanceMember is false but membInfo.MemberFlags.MemberIsInstance 
// is true.
//
// This is the right abstraction for viewing member types, but the implementation
// below is a little ugly.
let GetTypeOfIntrinsicMemberInCompiledForm g (vref:ValRef) =
    assert (not vref.IsExtensionMember)
    let membInfo,topValInfo = check_member_vref vref
    let tps,argInfos,rty,retInfo = GetTypeOfMemberInMemberForm g vref
    let argInfos = 
        // Check if the thing is really an instance member compiled as a static member
        // If so, the object argument counts as a normal argument in the compiled form
        if membInfo.MemberFlags.MemberIsInstance && not (ValRefIsCompiledAsInstanceMember g vref) then 
            let _,origArgInfos,_,_ = GetTopValTypeInFSharpForm g topValInfo vref.Type vref.Range
            match origArgInfos with
            | [] -> 
                errorR(InternalError("value does not have a valid member type",vref.Range)); 
                argInfos
            | h::t -> h ::argInfos
        else argInfos
    tps,argInfos,rty,retInfo


//--------------------------------------------------------------------------
// Tuple compilation (expressions)
//------------------------------------------------------------------------ 


let rec compiled_mk_tuple g (argtys,args,m) = 
    let n = List.length argtys 
    if n <= 0 then failwith "compiled_mk_tuple"
    elif n < maxTuple then  (compiled_tuple_tcref g argtys, argtys, args, m)
    else
        let argtysA,argtysB = split_after goodTupleFields argtys
        let argsA,argsB = split_after (goodTupleFields) args
        let ty8, v8 = 
            match argtysB,argsB with 
            | [ty8],[arg8] -> 
                match ty8 with
                // if it's already been nested or ended, pass it through
                |  TType_app(tn, _)  when (is_tuple_tcref g tn) ->
                    ty8,arg8
                | _ ->
                    let ty8enc = TType_app(g.tuple1_tcr,[ty8])
                    let v8enc = TExpr_op(TOp_tuple,[ty8],[arg8],m) 
                    ty8enc,v8enc
            | _ -> 
                let a,b,c,d = compiled_mk_tuple g (argtysB, argsB, m)
                let ty8plus = TType_app(a,b)
                let v8plus = TExpr_op(TOp_tuple,b,c,d)
                ty8plus,v8plus
        let argtysAB = argtysA @ [ty8] 
        (compiled_tuple_tcref g argtysAB, argtysAB,argsA @ [v8],m)

let get_rfref_of_tcref(tcref,n) =
    let f = (deref_tycon tcref).GetFieldByIndex n 
    rfref_of_rfield tcref f 

let mspec_Tuple_ItemN (g : TcGlobals) typ n = IL.mk_nongeneric_instance_mspec_in_tref((tref_of_typ typ), AsObject, (if n < goodTupleFields then "get_Item"^(n+1).ToString() else "get_Rest"), [], (mk_tyvar_ty (uint16 n)), (inst_of_typ typ))
let mk_call_Tuple_ItemN g m n typ te retty =
    mk_asm([IL.mk_normal_call(mspec_Tuple_ItemN g typ n)],[],[te],[retty],m)
          
