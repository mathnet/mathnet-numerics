// (c) Microsoft Corporation 2005-2009. 

#light

///  Generate the hash/compare functions we add to user-defined types by default.
module internal Microsoft.FSharp.Compiler.Augment 
open Internal.Utilities
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.PrettyNaming
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Env

let mk_IComparable_CompareTo_slotsig g = 
    TSlotSig("CompareTo",g.mk_IComparable_ty, [],[], [[TSlotParam(Some("obj"),g.obj_ty,false,false,false,[])]],Some g.int_ty)
    
let mk_IStructuralComparable_CompareTo_slotsig g =
    TSlotSig("CompareTo",g.mk_IStructuralComparable_ty,[],[],[[TSlotParam(None,(mk_tuple_ty [g.obj_ty ; g.mk_IComparer_ty]),false,false,false,[])]], Some g.int_ty)
    
let mk_IStructuralEquatable_Equals_slotsig g =
    TSlotSig("Equals",g.mk_IStructuralEquatable_ty,[],[],[[TSlotParam(None,(mk_tuple_ty [g.obj_ty ; g.mk_IEqualityComparer_ty]),false,false,false,[])]], Some g.bool_ty)

let mk_IStructuralEquatable_GetHashCode_slotsig g =
    TSlotSig("GetHashCode",g.mk_IStructuralEquatable_ty,[],[],[[TSlotParam(None,g.mk_IEqualityComparer_ty,false,false,false,[])]], Some g.int_ty)
 
let mk_GetHashCode_slotsig g = 
    TSlotSig("GetHashCode", g.obj_ty, [],[], [],Some  g.int_ty)

let mk_Equals_slotsig g = 
    TSlotSig("Equals", g.obj_ty, [],[], [[TSlotParam(Some("obj"),g.obj_ty,false,false,false,[])]],Some  g.bool_ty)


let mspec_Object_GetType ilg = IL.mk_nongeneric_instance_mspec_in_nongeneric_boxed_tref(ilg.tref_Object,"GetType",[],ilg.typ_Type)
let mspec_Object_ToString ilg = IL.mk_nongeneric_instance_mspec_in_nongeneric_boxed_tref(ilg.tref_Object,"ToString",[],ilg.typ_String)
let mk_call_Object_GetType_GetString g m e1 = 
  mk_asm ([ IL.mk_normal_callvirt(mspec_Object_ToString g.ilg)   ], [], 
             [ mk_asm ([ IL.mk_normal_callvirt(mspec_Object_GetType g.ilg) ], [], [ e1; ], [ g.int_ty ], m) ], [ g.string_ty ], m)


//-------------------------------------------------------------------------
// Helpers associated with code-generation of comparison/hash augmentations
//------------------------------------------------------------------------- 

let mk_this_typ           g ty = if is_struct_typ g ty then mk_byref_typ g ty else ty 

let mk_compare_obj_typ    g ty = (mk_this_typ g ty) --> (g.obj_ty --> g.int_ty)
let mk_compare_typ        g ty = (mk_this_typ g ty) --> (ty --> g.int_ty)
let mk_compare_withc_typ  g ty = (mk_this_typ g ty) --> ((mk_tuple_ty [g.obj_ty ; g.mk_IComparer_ty]) --> g.int_ty)

let mk_equals_obj_typ     g ty = (mk_this_typ g ty) --> (g.obj_ty --> g.bool_ty)
let mk_equals_typ         g ty = (mk_this_typ g ty) --> (ty --> g.bool_ty)
let mk_equals_withc_typ   g ty = (mk_this_typ g ty) --> ((mk_tuple_ty [g.obj_ty ; g.mk_IEqualityComparer_ty]) --> g.bool_ty)

let mk_hash_withc_typ     g ty = (mk_this_typ g ty) --> (g.mk_IEqualityComparer_ty --> g.int_ty)

//-------------------------------------------------------------------------
// Polymorphic comparison
//------------------------------------------------------------------------- 

let mk_rel_binop g op m e1 e2 = mk_asm ([ IL.I_arith op  ],[],  [e1; e2],[g.bool_ty],m)
let mk_clt g m e1 e2 = mk_rel_binop g IL.AI_clt m e1 e2 
let mk_cgt g m e1 e2 = mk_rel_binop g IL.AI_cgt m e1 e2

//-------------------------------------------------------------------------
// REVIEW: make this a .constrained call, not a virtual call.
//------------------------------------------------------------------------- 

// for creating and using FSharpComparer objects and for creating and using 
// IStructuralComparable objects (Eg, Calling CompareTo(obj o, IComparer comp))
let icomparer_iltref g = g.tcref_System_Collections_IComparer.CompiledRepresentationForTyrepNamed
let icomparer_ilt g = mk_boxed_typ (icomparer_iltref g) []
let istructuralcomparable_iltref g = g.tcref_System_IStructuralComparable.CompiledRepresentationForTyrepNamed

let iequalitycomparer_iltref g = (g.tcref_System_Collections_IEqualityComparer).CompiledRepresentationForTyrepNamed
let iequalitycomparer_ilt g = mk_boxed_typ (iequalitycomparer_iltref g) []
let istructuralequatable_iltref g = (g.tcref_System_IStructuralEquatable).CompiledRepresentationForTyrepNamed
let langprim_iltref g = (g.tcref_LanguagePrimitives).CompiledRepresentationForTyrepNamed
let langprim_ilt g = mk_boxed_typ (langprim_iltref g) []

let mspec_getComparer g = mk_static_nongeneric_mspec_in_typ (langprim_ilt g, "FSharpComparer",[],icomparer_ilt g)
let mk_call_GetComparer g m =
    mk_asm([IL.mk_normal_call(mspec_getComparer g)], [], [], [g.mk_IComparer_ty], m)

let mk_thisv g m ty = mk_local m "this" (mk_this_typ g ty)  

let mk_shl g m acce n = mk_asm([ IL.mk_ldc_i32 n; IL.I_arith IL.AI_shl ],[],[acce],[g.int_ty],m)
let mk_shr g m acce n = mk_asm([ IL.mk_ldc_i32 n; IL.I_arith IL.AI_shr ],[],[acce],[g.int_ty],m)
let mk_add g m e1 e2 = mk_asm([ IL.I_arith IL.AI_add ],[],[e1;e2],[g.int_ty],m)
                   
let add_to_hash_acc g m e accv acce =
    mk_val_set m accv (mk_add g m (mk_int g m 0x9e3779b9) 
                          (mk_add g m e 
                             (mk_add g m (mk_shl g m acce 6) (mk_shr g m acce 2))))

     
let mk_combine_all_hash_generators g m exprs accv acce =
    List.fold_left
        (fun tm e -> mk_compgen_seq m (add_to_hash_acc g m e accv acce) tm)
        acce
        exprs

//-------------------------------------------------------------------------
// Build comparison functions for union, record and exception types.
//------------------------------------------------------------------------- 

let mk_thisv_thatv g m ty =
    let thisv,thise = mk_thisv g m ty
    let thatv,thate = mk_local m "obj" (mk_this_typ g ty)   
    thisv,thatv,thise,thate

let bind_thatv g m ty thatv expr = 
    if is_struct_typ g ty then 
      let thatv2,_ = mk_mut_compgen_local m "obj" ty 
      thatv2,mk_compgen_let m thatv (mk_val_addr m (mk_local_vref thatv2)) expr
    else thatv,expr 

let mk_thatcast g m ty =
    if is_struct_typ g ty then
        mk_mut_compgen_local m "thatCast" (mk_byref_typ g ty)     
    else
        mk_compgen_local m "thatCast" ty
        
let bind_thatcast g m ty thatcastv thatv thate expr =
    if is_struct_typ g ty then
        mk_compgen_let m thatcastv (mk_val_addr m (mk_local_vref thatv))  expr
    else
        mk_compgen_let m thatcastv thate expr

let mk_compare_test_conjs g m exprs =
    match exprs with 
    | [] -> mk_zero g m
    | [h] -> h
    | l -> 
        let a,b = List.frontAndBack l 
        (a,b) ||> List.foldBack (fun e acc -> 
            let nv,ne = mk_local m "n" g.int_ty
            mk_compgen_let m nv e
              (mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.int_ty
                 (mk_clt g m ne (mk_zero g m))
                 ne
                 (mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.int_ty 
                    (mk_cgt g m ne (mk_zero g m))
                    ne
                    acc)))

let mk_equals_test_conjs g m exprs =
    match exprs with 
    | [] -> mk_one g m
    | [h] -> h
    | l -> 
        let a,b = List.frontAndBack l 
        List.foldBack (fun e acc -> mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.bool_ty e acc (mk_false g m)) a b

/// note: 'x == y' does not imply 'x = y' for NaN 
let mk_physical_equality_equals_test g m tycon thise thate expr = expr

let minimal_type g (tcref:TyconRef) = 
    if tcref.Deref.IsExceptionDecl then [], g.exn_ty 
    else generalize_tcref tcref
    
/// Build the comparison implementation for a record type 
let mk_recd_compare g tcref (tycon:Tycon) = 
    let m = tycon.Range 
    let fields = tycon.AllInstanceFieldsAsList 
    let tinst,ty = minimal_type g tcref
    let thisv,thatv,thise,thate = mk_thisv_thatv g m ty 
    let compe = mk_call_GetComparer g m
    let mk_test (fspec:RecdField) = 
        let fty = fspec.FormalType 
        let fref = rfref_of_rfield tcref fspec 
        let m = fref.Range 
        mk_call_generic_comparison_withc_outer g m fty
          compe
          (mk_recd_field_get_via_expra(thise, fref, tinst, m))
          (mk_recd_field_get_via_expra(thate, fref, tinst, m)) 
    let expr = mk_compare_test_conjs g m (List.map mk_test fields) 
    let thatv,expr = bind_thatv g m ty thatv expr
    thisv,thatv, expr


/// Build the comparison implementation for a record type when parameterized by a comparer
let mk_recd_compare_withc g tcref (tycon:Tycon) (thisv,thise) (_,thate) compe = 
    let m = tycon.Range 
    let fields = tycon.AllInstanceFieldsAsList
    let tinst,ty = minimal_type g tcref
    let tcv,tce = mk_compgen_local m "tempCast" ty    // let tcv = thate
    let thatcastv,thatcaste = mk_thatcast g m ty      // let thatcastv = &tcv, if a struct
    
    let mk_test (fspec:RecdField) = 
        let fty = fspec.FormalType 
        let fref = rfref_of_rfield tcref fspec 
        let m = fref.Range 
        let e1 = mk_recd_field_get_via_expra(thise, fref, tinst, m)
        let e2 = mk_recd_field_get_via_expra(thatcaste, fref, tinst, m)

        mk_call_generic_comparison_withc_outer g m fty
          compe
          (mk_recd_field_get_via_expra(thise, fref, tinst, m))
          (mk_recd_field_get_via_expra(thatcaste, fref, tinst, m))
    let expr = mk_compare_test_conjs g m (List.map mk_test fields) 

    let expr = bind_thatcast g m ty thatcastv tcv tce expr
    // will be optimized away if not necessary
    let expr = mk_compgen_let m tcv thate expr
    expr    


/// Build the equality implementation wrapper for a record type 
let mk_recd_equality g tcref (tycon:Tycon) = 
    let m = tycon.Range
    let fields = tycon.AllInstanceFieldsAsList 
    let tinst,ty = minimal_type g tcref
    let thisv,thatv,thise,thate = mk_thisv_thatv g m ty 
    let mk_test (fspec:RecdField) = 
        let fty = fspec.FormalType 
        let fref = rfref_of_rfield tcref fspec 
        let m = fref.Range 
        mk_call_generic_equality_outer g m fty
          (mk_recd_field_get_via_expra(thise, fref, tinst, m))
          (mk_recd_field_get_via_expra(thate, fref, tinst, m)) 
    let expr = mk_equals_test_conjs g m (List.map mk_test fields) 
    let expr = mk_physical_equality_equals_test g m tycon thise thate expr
    let thatv,expr = bind_thatv g m ty thatv expr
    thisv,thatv,expr
    
/// Build the equality implementation for a record type when parameterized by a comparer
let mk_recd_equality_withc g tcref (tycon:Tycon) (thisv,thise) (thatv,thate) compe =
    let m = tycon.Range
    let fields = tycon.AllInstanceFieldsAsList
    let tinst,ty = minimal_type g tcref
    let thatcastv,thatcaste = mk_thatcast g m ty
    let tcv,tce = mk_compgen_local m "tempCast" ty  
    
    let mk_test (fspec:RecdField) =
        let fty = fspec.FormalType
        let fref = rfref_of_rfield tcref fspec
        let m = fref.Range
        let e1 = mk_recd_field_get_via_expra(thise, fref, tinst, m)
        let e2 = mk_recd_field_get_via_expra(thatcaste, fref, tinst, m)
        
        mk_call_generic_equality_withc_outer g m fty
            compe
            (mk_recd_field_get_via_expra(thise, fref, tinst, m))
            (mk_recd_field_get_via_expra(thatcaste, fref, tinst, m))
    let expr = mk_equals_test_conjs g m (List.map mk_test fields)
    let expr = mk_physical_equality_equals_test g m tycon thise thatcaste expr
    let expr = bind_thatcast g m ty thatcastv tcv tce expr
    // will be optimized away if not necessary
    let expr = mk_compgen_let m tcv thate expr
    thisv,thatv,expr
        

/// Build the comparison implementation for an exception definition 
let mk_exnconstr_compare g exnref (exnc:Tycon) = 
    let m = exnc.Range 
    let thatv,thate = mk_local m "obj" g.exn_ty  
    let thisv,thise = mk_thisv g m g.exn_ty  
    let compe = mk_call_GetComparer g m
    let mk_test i (rfield:RecdField) =    
        let fty = rfield.FormalType
        mk_call_generic_comparison_withc_outer g m fty
          compe
          (mk_exnconstr_field_get(thise, exnref, i, m))
          (mk_exnconstr_field_get(thate, exnref, i, m)) 
    let expr = mk_compare_test_conjs g m (List.mapi mk_test (exnc.AllInstanceFieldsAsList)) 
    let expr =
        let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m )
        let dtree = 
          TDSwitch(thate,
                   [ mk_case(TTest_isinst(g.exn_ty,mk_tyapp_ty exnref []),
                             mbuilder.AddResultTarget(expr,SuppressSequencePointAtTarget)) ],
                   // OK, this is gross - we are comparing types by comparing strings.  We should be able to do this another way.
                   Some(mbuilder.AddResultTarget 
                            (mk_call_string_compare g m 
                               (mk_call_Object_GetType_GetString g m thise) 
                               (mk_call_Object_GetType_GetString g m thate),
                             SuppressSequencePointAtTarget)),
                   m)
        mbuilder.Close(dtree,m,g.int_ty)
    thisv,thatv,expr

    
/// Build the comparison implementation for an exception definition when parameterized by a comparer
let mk_exnconstr_compare_withc g exnref (exnc:Tycon) (thisv,thise) (thatv,thate) compe = 
    let m = exnc.Range
    let thatcastv,thatcaste = mk_thatcast g m g.exn_ty
    let mk_test i (rfield:RecdField) = 
        let fty = rfield.FormalType
        let e1 = mk_exnconstr_field_get(thise, exnref, i, m)
        let e2 = mk_exnconstr_field_get(thatcaste, exnref, i, m)

        mk_call_generic_comparison_withc_outer g m fty
          compe
          (mk_exnconstr_field_get(thise, exnref, i, m))
          (mk_exnconstr_field_get(thatcaste, exnref, i, m))
    let expr = mk_compare_test_conjs g m (List.mapi mk_test (exnc.AllInstanceFieldsAsList)) 
    let expr =
        let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m )
        let dtree = 
          TDSwitch(thatcaste,
                   [ mk_case(TTest_isinst(g.exn_ty,mk_tyapp_ty exnref []),
                             mbuilder.AddResultTarget(expr,SuppressSequencePointAtTarget)) ],
                   // OK, this is gross - we are comparing types by comparing strings.  We should be able to do this another way.
                   Some(mbuilder.AddResultTarget 
                            (mk_call_string_compare g m 
                               (mk_call_Object_GetType_GetString g m thise) 
                               (mk_call_Object_GetType_GetString g m thatcaste),
                             SuppressSequencePointAtTarget)),
                   m)
        mbuilder.Close(dtree,m,g.int_ty)
    let expr = bind_thatcast g m g.exn_ty thatcastv thatv thate expr
    expr


/// Build the equality implementation for an exception definition
let mk_exnconstr_equality g exnref (exnc:Tycon) = 
    let m = exnc.Range 
    let thatv,thate = mk_local m "obj" g.exn_ty  
    let thisv,thise = mk_thisv g m g.exn_ty  
    let mk_test i (rfield:RecdField) = 
        let fty = rfield.FormalType
        mk_call_generic_equality_outer g m fty
          (mk_exnconstr_field_get(thise, exnref, i, m))
          (mk_exnconstr_field_get(thate, exnref, i, m)) 
    let expr = mk_equals_test_conjs g m (List.mapi mk_test (exnc.AllInstanceFieldsAsList)) 
    let existential_tested =
        let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m ) 
        let dtree = 
          TDSwitch(thate,
                   [ mk_case(TTest_isinst(g.exn_ty,mk_tyapp_ty exnref []),
                            mbuilder.AddResultTarget(expr,SuppressSequencePointAtTarget)) ],
                   Some(mbuilder.AddResultTarget(mk_false g m,SuppressSequencePointAtTarget)),
                   m)
        mbuilder.Close(dtree,m,g.bool_ty)
    let eqTested = mk_physical_equality_equals_test g m exnc thise thate existential_tested
    thisv,thatv, eqTested
    
    
/// Build the equality implementation for an exception definition when parameterized by a comparer
let mk_exnconstr_equality_withc g exnref (exnc:Tycon) (thisv,thise) (thatv,thate) compe = 
    let m = exnc.Range
    let thatcastv,thatcaste =  mk_thatcast g m g.exn_ty
    let mk_test i (rfield:RecdField) = 
        let fty = rfield.FormalType
        let e1 = mk_exnconstr_field_get(thise, exnref, i, m)
        let e2 = mk_exnconstr_field_get(thatcaste, exnref, i, m)
        mk_call_generic_equality_withc_outer g m fty
          compe
          (mk_exnconstr_field_get(thise, exnref, i, m))
          (mk_exnconstr_field_get(thatcaste, exnref, i, m))
    let expr = mk_equals_test_conjs g m (List.mapi mk_test (exnc.AllInstanceFieldsAsList)) 
    let existential_tested =
        let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m ) 
        let dtree = 
          TDSwitch(thatcaste,
                   [ mk_case(TTest_isinst(g.exn_ty,mk_tyapp_ty exnref []),
                            mbuilder.AddResultTarget(expr,SuppressSequencePointAtTarget)) ],
                   Some(mbuilder.AddResultTarget(mk_false g m,SuppressSequencePointAtTarget)),
                   m)
        mbuilder.Close(dtree,m,g.bool_ty)
    let eqTested = mk_physical_equality_equals_test g m exnc thise thatcaste existential_tested
    let eqTested = bind_thatcast g m g.exn_ty thatcastv thatv thate eqTested
    thisv,thatv, eqTested

/// Build the comparison implementation for a union type
let mk_union_compare g tcref (tycon:Tycon) = 
    let m = tycon.Range 
    let ucases = tycon.UnionCasesAsList 
    let tinst,ty = minimal_type g tcref
    let thisv,thise = mk_local m "this" ty  
    let thatv,thate = mk_local m "obj" ty  
    let thistagv,thistage = mk_compgen_local m "thisTag" g.int_ty  
    let thattagv,thattage = mk_compgen_local m "thatTag" g.int_ty 
    let compe = mk_call_GetComparer g m

    let expr1 = 
        let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m ) 
        let mk_constr_case ucase =
            let cref = ucref_of_ucase tcref ucase 
            let m = cref.Range 
            let thisucv,thisucve = mk_compgen_local m "thisCast" (mk_proven_ucase_typ cref tinst)
            let thatucv,thatucve = mk_compgen_local m "thatCast" (mk_proven_ucase_typ cref tinst)
            let mk_test j (argty:RecdField) = 
              mk_call_generic_comparison_withc_outer g m argty.FormalType
                compe
                (mk_ucase_field_get_proven(thisucve, cref, tinst, j, m))
                (mk_ucase_field_get_proven(thatucve, cref, tinst, j, m)) 
            let rfields = ucase.RecdFields 
            if isNil rfields then None else
            Some (mk_case(TTest_unionconstr(cref,tinst),
                          mbuilder.AddResultTarget
                             (mk_compgen_let m thisucv (mk_ucase_proof(thise,cref,tinst,m))
                                 (mk_compgen_let m thatucv (mk_ucase_proof(thate,cref,tinst,m))
                                     (mk_compare_test_conjs g m (List.mapi mk_test rfields))),
                              SuppressSequencePointAtTarget)))
        
        let nullary,nonNullary = List.partition isNone (List.map mk_constr_case ucases)  
        if isNil nonNullary then mk_zero g m else 
        let dtree = 
            TDSwitch(thise,
                     (nonNullary |> List.map (function (Some c) -> c | None -> failwith "mk_union_compare")), 
                     (if isNil nullary then None 
                      else Some (mbuilder.AddResultTarget(mk_zero g m,SuppressSequencePointAtTarget))),
                     m) 
        mbuilder.Close(dtree,m,g.int_ty)

    let getTags = 
        if ucases.Length = 1 then expr1 else
        let tagsEqTested = 
            mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.int_ty  
              (mk_ceq g m thistage thattage)
              expr1
              (mk_asm ([ IL.I_arith IL.AI_sub  ],[],  [thistage; thattage],[g.int_ty],m))in 
        mk_compgen_let m thistagv
          (mk_ucase_tag_get (thise,tcref,tinst,m))
          (mk_compgen_let m thattagv
               (mk_ucase_tag_get (thate,tcref,tinst,m))
               tagsEqTested) 

    let nullTestedThat = mk_nonnull_cond g m g.int_ty thate getTags (mk_one g m) 
    let nullTestedThis = mk_nonnull_cond g m g.int_ty thise nullTestedThat (mk_minus_one g m)
    thisv,thatv, nullTestedThis


/// Build the comparison implementation for a union type when parameterized by a comparer
let mk_union_compare_withc g tcref (tycon:Tycon) (thisv,thise) (thatv,thate) compe = 
    let m = tycon.Range 
    let ucases = tycon.UnionCasesAsList
    let tinst,ty = minimal_type g tcref
    let thistagv,thistage = mk_compgen_local m "thisTag" g.int_ty  
    let thattagv,thattage = mk_compgen_local m "thatTag" g.int_ty  
    let thatcastv,thatcaste = mk_thatcast g m ty

    let expr1 = 
        let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m ) 
        let mk_constr_case ucase =
            let cref = ucref_of_ucase tcref ucase 
            let m = cref.Range 
            let thisucv,thisucve = mk_compgen_local m "thisCastu" (mk_proven_ucase_typ cref tinst)
            let thatucv,thatucve = mk_compgen_local m "thatCastu" (mk_proven_ucase_typ cref tinst)
            let mk_test j (argty:RecdField) = 
              let e1 = mk_ucase_field_get_proven(thisucve, cref, tinst, j, m)
              let e2 = mk_ucase_field_get_proven(thatucve, cref, tinst, j, m)
              mk_call_generic_comparison_withc_outer g m argty.FormalType
                compe
                (mk_ucase_field_get_proven(thisucve, cref, tinst, j, m))
                (mk_ucase_field_get_proven(thatucve, cref, tinst, j, m))
            let rfields = ucase.RecdFields 
            if isNil rfields then None else
            Some (mk_case(TTest_unionconstr(cref,tinst),
                          mbuilder.AddResultTarget
                             (mk_compgen_let m thisucv (mk_ucase_proof(thise,cref,tinst,m))
                                 (mk_compgen_let m thatucv (mk_ucase_proof(thatcaste,cref,tinst,m))
                                     (mk_compare_test_conjs g m (List.mapi mk_test rfields))),
                              SuppressSequencePointAtTarget)))
        
        let nullary,nonNullary = List.partition isNone (List.map mk_constr_case ucases)  
        if isNil nonNullary then mk_zero g m else 
        let dtree = 
            TDSwitch(thise,
                     (nonNullary |> List.map (function (Some c) -> c | None -> failwith "mk_union_compare")), 
                     (if isNil nullary then None 
                      else Some (mbuilder.AddResultTarget(mk_zero g m,SuppressSequencePointAtTarget))),
                     m) 
        mbuilder.Close(dtree,m,g.int_ty)

    let getTags = 
        if ucases.Length = 1 then expr1 else
        let tagsEqTested = 
            mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.int_ty  
              (mk_ceq g m thistage thattage)
              expr1
              (mk_asm ([ IL.I_arith IL.AI_sub  ],[],  [thistage; thattage],[g.int_ty],m))in 
        mk_compgen_let m thistagv
          (mk_ucase_tag_get (thise,tcref,tinst,m))
          (mk_compgen_let m thattagv
               (mk_ucase_tag_get (thatcaste,tcref,tinst,m))
               tagsEqTested) 

    let nullTestedThat = mk_nonnull_cond g m g.int_ty thatcaste getTags (mk_one g m) 
    let nullTestedThis = mk_nonnull_cond g m g.int_ty thise nullTestedThat (mk_minus_one g m)
    let eqTested = bind_thatcast g m ty thatcastv thatv thate nullTestedThis
    eqTested
    
    
/// Build the equality implementation for a union type
let mk_union_equality g tcref (tycon:Tycon) = 
    let m = tycon.Range 
    let ucases = tycon.UnionCasesAsList 
    let tinst,ty = minimal_type g tcref
    let thisv,thise = mk_local m "this" ty  
    let thatv,thate = mk_local m "obj" ty  
    let thistagv,thistage = mk_compgen_local m "thisTag" g.int_ty  
    let thattagv,thattage = mk_compgen_local m "thatTag" g.int_ty  

    let expr1 = 
        let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m ) 
        let mk_constr_case ucase =
            let cref = ucref_of_ucase tcref ucase 
            let m = cref.Range 
            let thisucv,thisucve = mk_compgen_local m "thisCast" (mk_proven_ucase_typ cref tinst)
            let thatucv,thatucve = mk_compgen_local m "thatCast" (mk_proven_ucase_typ cref tinst)
            let mk_test j (argty:RecdField) = 
                mk_call_generic_equality_outer g m argty.FormalType
                  (mk_ucase_field_get_proven(thisucve, cref, tinst, j, m))
                  (mk_ucase_field_get_proven(thatucve, cref, tinst, j, m)) 
            let rfields = ucase.RecdFields
            if isNil rfields then None else
            Some (mk_case(TTest_unionconstr(cref,tinst),
                          mbuilder.AddResultTarget
                              (mk_compgen_let m thisucv (mk_ucase_proof(thise,cref,tinst,m))
                                 (mk_compgen_let m thatucv (mk_ucase_proof(thate,cref,tinst,m))
                                     (mk_equals_test_conjs g m (List.mapi mk_test rfields))),
                               SuppressSequencePointAtTarget)))
        
        let nullary,nonNullary = List.partition isNone (List.map mk_constr_case ucases)  
        if isNil nonNullary then mk_true g m else 
        let dtree = 
            TDSwitch(thise,List.map (function (Some c) -> c | None -> failwith "mk_union_equality") nonNullary, 
                    (if isNil nullary then None else Some (mbuilder.AddResultTarget(mk_true g m,SuppressSequencePointAtTarget))),
                    m) 
        mbuilder.Close(dtree,m,g.bool_ty)
        
    let getTags = 
        if ucases.Length = 1 then expr1 else
        let tagsEqTested = 
          mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.bool_ty  
            (mk_ceq g m thistage thattage)
            expr1
            (mk_false g m)

        mk_compgen_let m thistagv
          (mk_ucase_tag_get (thise,tcref,tinst,m))
          (mk_compgen_let m thattagv
               (mk_ucase_tag_get (thate,tcref,tinst,m))
               tagsEqTested) 
    let nullTestedThat = mk_nonnull_cond g m g.bool_ty thate getTags (mk_false g m) 
    let nullTestedThis = mk_nonnull_cond g m g.bool_ty thise nullTestedThat (mk_false g m)
    let eqTested = 
        if is_unit_typ g ty then mk_true g m else
        mk_physical_equality_equals_test g m tycon thise thate nullTestedThis
    thisv,thatv, eqTested


/// Build the equality implementation for a union type when parameterized by a comparer
let mk_union_equality_withc g tcref (tycon:Tycon) (thisv,thise) (thatv,thate) compe =
    let m = tycon.Range 
    let ucases = tycon.UnionCasesAsList
    let tinst,ty = minimal_type g tcref
    let thistagv,thistage = mk_compgen_local m "thisTag" g.int_ty  
    let thattagv,thattage = mk_compgen_local m "thatTag" g.int_ty  
    let thatcastv,thatcaste = mk_thatcast g m ty

    let expr1 = 
        let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m ) 
        let mk_constr_case ucase =
            let cref = ucref_of_ucase tcref ucase 
            let m = cref.Range 
            let thisucv,thisucve = mk_compgen_local m "thisCastu" (mk_proven_ucase_typ cref tinst)
            let thatucv,thatucve = mk_compgen_local m "thatCastu" (mk_proven_ucase_typ cref tinst)
            let mk_test j (argty:RecdField) = 
              let e1 = mk_ucase_field_get_proven(thisucve, cref, tinst, j, m)
              let e2 = mk_ucase_field_get_proven(thatucve, cref, tinst, j, m)
              mk_call_generic_equality_withc_outer g m argty.FormalType
                compe
                (mk_ucase_field_get_proven(thisucve, cref, tinst, j, m))
                (mk_ucase_field_get_proven(thatucve, cref, tinst, j, m))
            let rfields = ucase.RecdFields
            if isNil rfields then None else
            Some (mk_case(TTest_unionconstr(cref,tinst),
                          mbuilder.AddResultTarget
                              (mk_compgen_let m thisucv (mk_ucase_proof(thise,cref,tinst,m))
                                 (mk_compgen_let m thatucv (mk_ucase_proof(thatcaste,cref,tinst,m))
                                     (mk_equals_test_conjs g m (List.mapi mk_test rfields))),
                               SuppressSequencePointAtTarget)))
        
        let nullary,nonNullary = List.partition isNone (List.map mk_constr_case ucases)  
        if isNil nonNullary then mk_true g m else 
        let dtree = 
            TDSwitch(thise,List.map (function (Some c) -> c | None -> failwith "mk_union_equality") nonNullary, 
                    (if isNil nullary then None else Some (mbuilder.AddResultTarget(mk_true g m,SuppressSequencePointAtTarget))),
                    m) 
        mbuilder.Close(dtree,m,g.bool_ty)
        
    let getTags = 
        if ucases.Length = 1 then expr1 else
        let tagsEqTested = 
          mk_cond NoSequencePointAtStickyBinding SuppressSequencePointAtTarget m g.bool_ty  
            (mk_ceq g m thistage thattage)
            expr1
            (mk_false g m)

        mk_compgen_let m thistagv
          (mk_ucase_tag_get (thise,tcref,tinst,m))
          (mk_compgen_let m thattagv
               (mk_ucase_tag_get (thatcaste,tcref,tinst,m))
               tagsEqTested) 
    let nullTestedThat = mk_nonnull_cond g m g.bool_ty thatcaste getTags (mk_false g m) 
    let nullTestedThis = mk_nonnull_cond g m g.bool_ty thise nullTestedThat (mk_false g m)
    let eqTested = 
        if is_unit_typ g ty then mk_true g m else
        mk_physical_equality_equals_test g m tycon thise thatcaste nullTestedThis
    let eqTested = bind_thatcast g m ty thatcastv thatv thate eqTested
    thisv,thatv, eqTested

//-------------------------------------------------------------------------
// Build hashing functions for union, record and exception types.
// Hashing functions must respect the "=" and comparison operators.
//------------------------------------------------------------------------- 

/// Structural hash implementation for record types when parameterized by a comparer 
let mk_recd_hash_withc g tcref (tycon:Tycon) compe = 
    let m = tycon.Range 
    let fields = tycon.AllInstanceFieldsAsList
    let tinst,ty = minimal_type g tcref
    let thisv,thise = mk_thisv g m ty
    let mk_field_hash (fspec:RecdField) = 
        let fty = fspec.FormalType
        let fref = rfref_of_rfield tcref fspec 
        let m = fref.Range 
        let e = mk_recd_field_get_via_expra(thise, fref, tinst, m)
        
        mk_call_generic_hash_withc_outer g m fty compe e
            
    let accv,acce = mk_mut_compgen_local m "i" g.int_ty                  
    let stmt = mk_combine_all_hash_generators g m (List.map mk_field_hash fields) (mk_local_vref accv) acce
    let expr = mk_compgen_let m accv (mk_zero g m) stmt 
    thisv,expr

/// Structural hash implementation for exception types when parameterized by a comparer
let mk_exnconstr_hash_withc g exnref (exnc:Tycon) compe = 
    let m = exnc.Range
    let thisv,thise = mk_thisv g m g.exn_ty
    
    let mk_hash i (rfield:RecdField) = 
        let fty = rfield.FormalType
        let e = mk_exnconstr_field_get(thise, exnref, i, m)
        
        mk_call_generic_hash_withc_outer g m fty compe e
       
    let accv,acce = mk_mut_compgen_local m "i" g.int_ty                  
    let stmt = mk_combine_all_hash_generators g m (List.mapi mk_hash (exnc.AllInstanceFieldsAsList)) (mk_local_vref accv) acce
    let expr = mk_compgen_let m accv (mk_zero g m) stmt 
    thisv,expr

/// Structural hash implementation for union types when parameterized by a comparer   
let mk_union_hash_withc g tcref (tycon:Tycon) compe =
    let m = tycon.Range
    let ucases = tycon.UnionCasesAsList
    let tinst,ty = minimal_type g tcref
    let thisv,thise = mk_thisv g m ty
    let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m ) 
    let accv,acce = mk_mut_compgen_local m "i" g.int_ty                  
    let mk_constr_case i ucase1 = 
      let c1ref = ucref_of_ucase tcref ucase1 
      let ucv,ucve = mk_compgen_local m "unionCase" (mk_proven_ucase_typ c1ref tinst)
      let m = c1ref.Range 
      let mk_hash j (rfield:RecdField) =  
        let fty = rfield.FormalType
        let e = mk_ucase_field_get_proven(ucve, c1ref, tinst, j, m)
        mk_call_generic_hash_withc_outer g m fty compe e
      mk_case(TTest_unionconstr(c1ref,tinst),
              mbuilder.AddResultTarget 
                (mk_compgen_let m ucv
                    (mk_ucase_proof(thise,c1ref,tinst,m))
                    (mk_compgen_seq m 
                          (mk_val_set m (mk_local_vref accv) (mk_int g m i)) 
                          (mk_combine_all_hash_generators g m (List.mapi mk_hash ucase1.RecdFields) (mk_local_vref accv) acce)),
                 SuppressSequencePointAtTarget))
    let dtree = TDSwitch(thise,List.mapi mk_constr_case ucases, None,m) 
    let stmt = mbuilder.Close(dtree,m,g.unit_ty)
    let expr = mk_compgen_let m accv (mk_zero g m) stmt 
    thisv,expr


//-------------------------------------------------------------------------
// The predicate that determines which types implement the 
// pre-baked IStructuralHash and IComparable semantics associated with F#
// types.  Note abstract types are not _known_ to implement these interfaces,
// though the interfaces may be discoverable via type tests.
//------------------------------------------------------------------------- 

let isNominalExnc (exnc:Tycon) = 
    match exnc.ExceptionInfo with 
    | TExnAbbrevRepr _ | TExnNone | TExnAsmRepr _ -> false
    | TExnFresh _ -> true

let isTrueFSharpStructTycon g (tycon: Tycon) = 
    (tycon.IsFSharpStructTycon  && not tycon.IsFSharpEnumTycon)

let canBeAugmented g (tycon:Tycon) = 
    tycon.IsUnionTycon ||
    tycon.IsRecordTycon ||
    (tycon.IsExceptionDecl && isNominalExnc tycon) ||
    isTrueFSharpStructTycon g tycon

let augmentation_attribs g (tycon:Tycon) = 
        canBeAugmented g tycon,
        TryFindBoolAttrib g g.attrib_ReferenceEqualityAttribute tycon.Attribs,
        TryFindBoolAttrib g g.attrib_StructuralEqualityAttribute tycon.Attribs,
        TryFindBoolAttrib g g.attrib_StructuralComparisonAttribute tycon.Attribs 

let CheckAugmentationAttribs g (tycon:Tycon)= 
    let m = tycon.Range
    let attribs = augmentation_attribs g tycon
    match attribs with 
    
    (* THESE ARE THE LEGITIMATE CASES *)

    | true, Some(true), None      , None ->
        if isTrueFSharpStructTycon g tycon then 
            errorR(Error("The 'ReferenceEquality' attribute may not be used on structs. Consider using the 'StructuralEquality' attribute instead, or implement an override for 'System.Object.Equals(obj)'", m))
        else ()

    (* [< >] *)
    | _,  None,      None, None
    (* [<ReferenceEquality(true)>] *)
    (* [<StructuralEquality(true); StructuralComparison(true)>] *)
    | true, None      , Some(true), Some(true) 
    (* [<StructuralEquality(false); StructuralComparison(false)>] *)
    | true, None      , Some(false), Some(false) 
    (* [<StructuralEquality(true); StructuralComparison(false)>] *)
    | true, None      , Some(true), Some(false) ->
        () 

    (* THESE ARE THE ERROR CASES *)

    (* [<ReferenceEquality(false); ...>] *)
    | _, Some(false), _      , _ ->
        errorR(Error("The 'ReferenceEquality' attribute may not be 'false'. Consider using the 'StructuralEquality' attribute instead", m))
    (* [<StructuralEquality(false); ...>] *)
    | _,           _, Some(false), None
    | _,           _, Some(false), Some(true) ->
        errorR(Error("The 'StructuralEquality' attribute may not be 'false' unles the 'StructuralComparison' attribute is also false", m))
    (* [<StructuralComparison(_)>] *)
    | true, None      , None      , Some(_) ->
        errorR(Error("The 'StructuralComparison' attribute must be used in conjunction with the 'StructuralEquality' attribute", m))
    (* [<StructuralEquality(_)>] *)
    | true, None      , Some(true), None ->
        errorR(Error("The 'StructuralEquality' attribute must be used in conjunction with the 'StructuralComparison' attribute", m))

    (* [<ReferenceEquality; StructuralEquality>] *)
    | true, Some(_)  , Some(_)    ,      _
    (* [<ReferenceEquality; StructuralComparison(_) >] *)
    | true, Some(_),             _, Some(_) -> 
        errorR(Error("A type may not have both the 'ReferenceEquality' and 'StructuralEquality' or 'StructuralComparison' attributes", m))

    (* non augmented type, [<ReferenceEquality; ... >] *)
    | false,  Some(_),           _, _
    (* non augmented type, [<StructuralEquality; ... >] *)
    | false,        _, Some(_)    , _
    (* non augmented type, [<StructuralComparison(_); ... >] *)
    | false,        _,      _     , Some(_) ->
        errorR(Error("Only record, union, exception and struct types may be augmented with the 'ReferenceEquality', 'StructuralEquality' and 'StructuralComparison' attributes", m))
    let tcaug = tycon.TypeContents
    
    let hasExplicitICompare = 
        isNone tcaug.tcaug_compare 
    let hasExplicitIStructuralCompare =
        isNone tcaug.tcaug_compare_withc
    let hasExplicitEquals = 
        isNone tcaug.tcaug_equals && tcaug_has_override g tcaug "Equals" [g.obj_ty]

    match attribs with 
    (* [<ReferenceEquality(true)>] *)
    | _, Some(true), _, _ when hasExplicitEquals -> 
        errorR(Error("A type with attribute 'ReferenceEquality' may not have an explicit implementation of 'Object.Equals(obj)'", m))
    | _,          _, Some(true), _ when hasExplicitEquals -> 
        errorR(Error("A type with attribute 'StructuralEquality' may not have an explicit implementation of 'Object.Equals(obj)'", m))
    // already caught the case where ReferenceEquality is true
    | _, None,        _,  Some(true) when hasExplicitICompare ->
        errorR(Error("A type with attribute 'StructuralComparison' may not have an explicit implementation of 'System.IComparable'", m))
    | _, Some(false), _,  Some(true) when hasExplicitICompare -> 
        errorR(Error("A type with attribute 'StructuralComparison' may not have an explicit implementation of 'System.IComparable'", m))
    | _ -> ()

let TyconIsAugmentedWithCompare g (tycon:Tycon) = 
    // This type gets defined in prim-types, before we can add attributes to F# type definitions
    let isUnit = g.compilingFslib && tycon.DisplayName = "Unit"
    not isUnit && 

    match augmentation_attribs g tycon with 
    (* [< >] *)
    | true, None, None      , None
    (* [<StructuralEquality(true); StructuralComparison(true)>] *)
    | true, None, Some(true), Some(true) -> true
    (* other cases *)
    | _ -> false

let TyconIsAugmentedWithEquals g (tycon:Tycon) = 
    // This type gets defined in prim-types, before we can add attributes to F# type definitions
    let isUnit = g.compilingFslib && tycon.DisplayName = "Unit"
    not isUnit && 

    match augmentation_attribs g tycon with 
    (* [< >] *)
    | true, None, None      , _
    (* [<StructuralEquality(true); _ >] *)
    (* [<StructuralEquality(true); StructuralComparison(true)>] *)
    | true, None, Some(true), _ -> true
    (* other cases *)
    | _ -> false

let TyconIsAugmentedWithHash g tycon = TyconIsAugmentedWithEquals g tycon
      
(*-------------------------------------------------------------------------
 * Make values that represent the implementations of the 
 * IComparable semantics associated with F# types.  
 *------------------------------------------------------------------------- *)

let slotImplMethod (final,ilnm,c,slotsig) = 
  { ImplementedSlotSigs=[slotsig];
    MemberFlags=
        { OverloadQualifier=None;
          MemberIsInstance=true; 
          MemberIsVirtual=false;
          MemberIsDispatchSlot=false;
          MemberIsFinal=final;
          MemberIsOverrideOrExplicitImpl=true;
          MemberKind=MemberKindMember};
    IsImplemented=false;
    CompiledName=ilnm; 
    ApparentParent=c} 

let nonVirtualMethod (ilnm,c) = 
  { ImplementedSlotSigs=[];
    MemberFlags={ OverloadQualifier=None;
                  MemberIsInstance=true; 
                  MemberIsVirtual=false;
                  MemberIsDispatchSlot=false;
                  MemberIsFinal=false;
                  MemberIsOverrideOrExplicitImpl=false;
                  MemberKind=MemberKindMember};
    IsImplemented=false;
    CompiledName=ilnm; 
    ApparentParent=c} 

let mk_vspec g (tcref:TyconRef) isStronglyTyped tmty cpath vis methn slotsig ilnm ty tuparg = 
    let m = tcref.Range 
    let tps = tcref.Typars(m)
    let id = mksyn_id m methn
    let final = is_union_typ g tmty or is_recd_typ g tmty or is_struct_typ g tmty 
    let membInfo = match slotsig with None -> nonVirtualMethod (ilnm,tcref) | Some(slotsig) -> slotImplMethod(final,ilnm,tcref,slotsig) 
    let inl = OptionalInline
    let args = if tuparg then [TopValInfo.unnamedTopArg; [TopValInfo.unnamedTopArg1;TopValInfo.unnamedTopArg1]] else [TopValInfo.unnamedTopArg;TopValInfo.unnamedTopArg]
    let topValInfo = Some (TopValInfo (TopValInfo.InferTyparInfo tps,args,TopValInfo.unnamedRetVal)) 
    NewVal (id,ty,Immutable,true,topValInfo,cpath,vis,ValNotInRecScope,Some(membInfo),NormalVal,[],inl,emptyXmlDoc,true,false,false,false,None,Parent(tcref)) 

let MakeValsForCompareAugmentation g (tcref:TyconRef) = 
    let _,tmty = minimal_type g tcref
    let tps = tcref.Typars(tcref.Range)
    let vis = tcref.TypeReprAccessibility
    mk_vspec g tcref false tmty tcref.CompilationPathOpt vis (tcref.MangledName^".CompareToOverride" ) (Some(mk_IComparable_CompareTo_slotsig g)) "CompareTo" (tps +-> (mk_compare_obj_typ g tmty)) false, 
    mk_vspec g tcref true tmty tcref.CompilationPathOpt vis (tcref.MangledName^".CompareTo" ) None "CompareTo" (tps +-> (mk_compare_typ g tmty)) false
    
let MakeValsForCompareWithComparerAugmentation g (tcref:TyconRef) =
    let _,tmty = minimal_type g tcref
    let tps = tcref.Typars(tcref.Range)
    let vis = tcref.TypeReprAccessibility
    mk_vspec g tcref false tmty tcref.CompilationPathOpt vis (tcref.MangledName^".StructuralCompareTo") (Some(mk_IStructuralComparable_CompareTo_slotsig g)) "CompareTo" (tps +-> (mk_compare_withc_typ g tmty)) true

let MakeValsForEqualsAugmentation g (tcref:TyconRef) = 
    let _,tmty = minimal_type g tcref
    let vis = tcref.TypeReprAccessibility
    let tps = tcref.Typars(tcref.Range)
    let obj_vspec = mk_vspec g tcref false tmty tcref.CompilationPathOpt vis (tcref.MangledName^".EqualsOverride" ) (Some(mk_Equals_slotsig g)) "Equals" (tps +-> (mk_equals_obj_typ g tmty)) false
    let vspec = mk_vspec g tcref true tmty tcref.CompilationPathOpt vis (tcref.MangledName^".Equals" ) None "Equals" (tps +-> (mk_equals_typ g tmty)) false
    obj_vspec,vspec
    
let MakeValsForEqualityWithComparerAugmentation g (tcref:TyconRef) =
    let _,tmty = minimal_type g tcref
    let vis = tcref.TypeReprAccessibility
    let tps = tcref.Typars(tcref.Range)
    let hsh = mk_vspec g tcref false tmty tcref.CompilationPathOpt vis (tcref.MangledName^".StructuralGetHashCode") (Some(mk_IStructuralEquatable_GetHashCode_slotsig g)) "GetHashCode" (tps +-> (mk_hash_withc_typ g tmty)) false
    let eq  = mk_vspec g tcref false tmty tcref.CompilationPathOpt vis (tcref.MangledName^".StructuralEquals") (Some(mk_IStructuralEquatable_Equals_slotsig g)) "Equals" (tps +-> (mk_equals_withc_typ g tmty)) true
    hsh,eq

let MakeBindingsForCompareAugmentation g (tycon:Tycon) = 
    let tcref = mk_local_tcref tycon 
    let m = tycon.Range
    let tps = tycon.Typars(tycon.Range)
    let mk_compare comparef =
        match tycon.TypeContents.tcaug_compare with 
        | None ->  []
        | Some (vref1,vref2) -> 
            let vspec1 = deref_val vref1
            let vspec2 = deref_val vref2
            (* this is the body of the override *)
            let rhs1 = 
              let tinst,ty = minimal_type g tcref
              
              let thisv,thise = mk_thisv g m ty  
              let thatobjv,thatobje = mk_local m "obj" g.obj_ty  
              let comparee = 
                  if is_unit_typ g ty then mk_zero g m else

                  mk_appl g ((expr_for_vref m vref2,vref2.Type), (if isNil tinst then [] else [tinst]), [thise;mk_coerce(thatobje,ty,m,g.obj_ty)], m)
              
              mk_lambdas m tps [thisv;thatobjv] (comparee,g.int_ty)  
            let rhs2 = 
              let thisv,thatv,comparee = comparef g tcref tycon 
              mk_lambdas m tps [thisv;thatv] (comparee,g.int_ty)  
            [ // This one must come first because it may be inlined into the second
              mk_compgen_bind vspec2 rhs2;
              mk_compgen_bind vspec1 rhs1; ] 
    if tycon.IsUnionTycon then mk_compare mk_union_compare 
    elif tycon.IsRecordTycon or tycon.IsStructTycon then mk_compare mk_recd_compare 
    elif tycon.IsExceptionDecl then mk_compare mk_exnconstr_compare 
    else []
    
let MakeBindingsForCompareWithComparerAugmentation g (tycon:Tycon) =
    let tcref = mk_local_tcref tycon
    let m = tycon.Range
    let tps = tycon.Typars(tycon.Range)
    let mk_compare comparef = 
        match tycon.TypeContents.tcaug_compare_withc with
        | None -> []
        | Some (vref) ->
            let vspec = deref_val vref
            let tinst,ty = minimal_type g tcref

            let compv,compe = mk_local m "comp" g.mk_IComparer_ty

            let thisv,thise = mk_thisv g m ty
            let thatobjv,thatobje = mk_local m "obj" g.obj_ty
            let thate = mk_coerce(thatobje,ty,m,g.obj_ty)

            let rhs =
                let comparee = comparef g tcref tycon (thisv,thise) (thatobjv,thate) compe
                let comparee = if is_unit_typ g ty then mk_zero g m else comparee
                mk_multi_lambdas m tps [[thisv];[thatobjv;compv]] (comparee,g.int_ty)
            [mk_compgen_bind vspec rhs]
    if tycon.IsUnionTycon then mk_compare mk_union_compare_withc
    elif tycon.IsRecordTycon or tycon.IsStructTycon then mk_compare mk_recd_compare_withc
    elif tycon.IsExceptionDecl then mk_compare mk_exnconstr_compare_withc
    else []    
    
let MakeBindingsForEqualityWithComparerAugmentation g (tycon:Tycon) =
    let tcref = mk_local_tcref tycon
    let m = tycon.Range
    let tps = tycon.Typars(tycon.Range)
    let mk_structural_equatable hashf equalsf =
        match tycon.TypeContents.tcaug_hash_and_equals_withc with
        | None -> []
        | Some (vref1, vref2) ->
            let vspec1 = deref_val vref1
            let vspec2 = deref_val vref2
            let tinst,ty = minimal_type g tcref
            let compv,compe = mk_local m "comp" g.mk_IEqualityComparer_ty
            
            // build the hash rhs
            let rhs_hash =
                let thisv,hashe = hashf g tcref tycon compe
                mk_lambdas m tps [thisv;compv] (hashe,g.int_ty)
                
            // build the equals rhs
            let rhs_equals =
                let thisv,thise = mk_thisv g m ty
                let thatv,thate = mk_local m "obj" g.obj_ty
                let thate = mk_coerce(thate,ty,m,g.obj_ty)
                let thisv,thatv,equalse = equalsf g tcref tycon (thisv,thise) (thatv,thate) compe
                mk_multi_lambdas m tps [[thisv];[thatv;compv]] (equalse,g.bool_ty)
                
            [(mk_compgen_bind vspec1 rhs_hash) ; (mk_compgen_bind vspec2 rhs_equals)] 
    if tycon.IsUnionTycon then mk_structural_equatable mk_union_hash_withc mk_union_equality_withc
    elif (tycon.IsRecordTycon || tycon.IsStructTycon) then mk_structural_equatable mk_recd_hash_withc mk_recd_equality_withc
    elif tycon.IsExceptionDecl then mk_structural_equatable mk_exnconstr_hash_withc mk_exnconstr_equality_withc
    else []

let MakeBindingsForEqualsAugmentation g (tycon:Tycon) = 
    let tcref = mk_local_tcref tycon 
    let m = tycon.Range 
    let tps = tycon.Typars(m)
    let mk_equals equalsf =
      match tycon.TypeContents.tcaug_equals with 
      | None ->  []
      | Some (vref1,vref2) -> 
          // this is the body of the override 
          let rhs1 = 
            let tinst,ty = minimal_type g tcref
            
            let thisv,thise = mk_thisv g m ty  
            let thatobjv,thatobje = mk_local m "obj" g.obj_ty  
            let equalse = 
                if is_unit_typ g ty then mk_true g m else

                let thatv,thate = mk_local m "that" ty  
                mk_isinst_cond g m ty thatobje thatv 
                    (mk_appl g ((expr_for_vref m vref2,vref2.Type), (if isNil tinst then [] else [tinst]), [thise;thate], m))
                    (mk_false g m)
            
            mk_lambdas m tps [thisv;thatobjv] (equalse,g.bool_ty)  
          // this is the body of the real strongly typed implementation 
          let rhs2 = 
              let thisv,thatv,equalse = equalsf g tcref tycon 
              mk_lambdas m tps [thisv;thatv] (equalse,g.bool_ty)  
            
          [ mk_compgen_bind (deref_val vref2) rhs2;
            mk_compgen_bind (deref_val vref1) rhs1;   ] 
    if tycon.IsExceptionDecl then mk_equals mk_exnconstr_equality 
    elif tycon.IsUnionTycon then mk_equals mk_union_equality 
    elif tycon.IsRecordTycon or tycon.IsStructTycon then mk_equals mk_recd_equality 
    else []
