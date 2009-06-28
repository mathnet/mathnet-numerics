// (c) Microsoft Corporation. All rights reserved

#light

/// Primary relations on types and signatures (with the exception of
/// the constraint solving engine and method overload resolution)
module (* internal *) Microsoft.FSharp.Compiler.Typrelns

open Internal.Utilities
open Internal.Utilities.Pervasives
open System.Text

open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.AbstractIL.IL (* Abstract IL  *)
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Infos
open Microsoft.FSharp.Compiler.PrettyNaming
open Microsoft.FSharp.Compiler.Infos.AccessibilityLogic

//-------------------------------------------------------------------------
// a :> b without coercion based on finalized (no type variable) types
//------------------------------------------------------------------------- 


// QUERY: This relation is barely used in the implementation and is 
// not part of the language specification. It is in general only used for 
// optimizations and warnings and to omit upcast coercions in the TAST 
let rec type_definitely_subsumes_type_no_coercion ndeep g amap m ty1 ty2 = 
  if ndeep > 100 then error(InternalError("recursive class hierarchy (detected in type_definitely_subsumes_type_no_coercion), ty1 = "^(DebugPrint.showType ty1),m));
  if ty1 === ty2 then true 
  // QUERY : quadratic
  elif type_equiv g ty1 ty2 then true
  else
    let ty1 = strip_tpeqns_and_tcabbrevs g ty1
    let ty2 = strip_tpeqns_and_tcabbrevs g ty2
    match ty1,ty2 with 
    | TType_app (tc1,l1)  ,TType_app (tc2,l2) when tcref_eq g tc1 tc2  ->  
        List.lengthsEqAndForall2 (type_equiv g) l1 l2
    | TType_ucase (tc1,l1)  ,TType_ucase (tc2,l2) when g.ucref_eq tc1 tc2  ->  
        List.lengthsEqAndForall2 (type_equiv g) l1 l2
    | TType_tuple l1    ,TType_tuple l2     -> 
        List.lengthsEqAndForall2 (type_equiv g) l1 l2 
    | TType_fun (d1,r1)  ,TType_fun (d2,r2)   -> 
        type_equiv g d1 d2 && type_equiv g r1 r2
    | TType_measure measure1, TType_measure measure2 ->
        measure_equiv g measure1 measure2
    | _ ->  
        (type_equiv g ty1 g.obj_ty && is_ref_typ g ty2) || (* F# reference types are subtypes of type 'obj' *)
        (is_stripped_tyapp_typ g ty2 &&
         is_ref_typ g ty2 && 
         let tcref,tinst = dest_stripped_tyapp_typ g ty2

         (match SuperTypeOfType g amap m ty2 with 
         | None -> false
         | Some ty -> type_definitely_subsumes_type_no_coercion (ndeep+1) g amap m ty1 ty) ||

         (is_interface_typ g ty1 &&
          ty2 |> ImplementsOfType g amap m 
              |> List.exists (type_definitely_subsumes_type_no_coercion (ndeep+1) g amap m ty1)))



type canCoerce = CanCoerce | NoCoerce

/// The feasible coercion relation. Part of the language spec.

let rec type_feasibly_subsumes_type ndeep g amap m ty1 canCoerce ty2 = 
    if ndeep > 100 then error(InternalError("recursive class hierarchy (detected in type_feasibly_subsumes_type), ty1 = "^(DebugPrint.showType ty1),m));
    let ty1 = strip_tpeqns_and_tcabbrevs g ty1
    let ty2 = strip_tpeqns_and_tcabbrevs g ty2
    match ty1,ty2 with 
    | TType_var r , _  | _, TType_var r -> true

    | TType_app (tc1,l1)  ,TType_app (tc2,l2) when tcref_eq g tc1 tc2  ->  
        List.lengthsEqAndForall2 (type_feasibly_equiv ndeep g amap m) l1 l2
    | TType_tuple l1    ,TType_tuple l2     -> 
        List.lengthsEqAndForall2 (type_feasibly_equiv ndeep g amap m) l1 l2 
    | TType_fun (d1,r1)  ,TType_fun (d2,r2)   -> 
        (type_feasibly_equiv ndeep g amap m) d1 d2 && (type_feasibly_equiv ndeep g amap m) r1 r2
    | TType_measure ms1, TType_measure ms2 ->
        measure_equiv g ms1 ms2
    | _ -> 
        (* F# reference types are subtypes of type 'obj' *) 
        (is_obj_typ g ty1 && (canCoerce = CanCoerce || is_ref_typ g ty2)) 
        ||
        (is_stripped_tyapp_typ g ty2 &&
         (canCoerce = CanCoerce || is_ref_typ g ty2) && 
         let tcref,tinst = dest_stripped_tyapp_typ g ty2
         begin match SuperTypeOfType g amap m ty2 with 
         | None -> false
         | Some ty -> type_feasibly_subsumes_type (ndeep+1) g amap m ty1 NoCoerce ty
         end or
         ty2 |> ImplementsOfType g amap m 
             |> List.exists (type_feasibly_subsumes_type (ndeep+1) g amap m ty1 NoCoerce))
                   
and type_feasibly_equiv ndeep g amap m ty1 ty2 = 

    if ndeep > 100 then error(InternalError("recursive class hierarchy (detected in type_feasibly_subsumes_type), ty1 = "^(DebugPrint.showType ty1),m));
    let ty1 = strip_tpeqns_and_tcabbrevs g ty1
    let ty2 = strip_tpeqns_and_tcabbrevs g ty2
    match ty1,ty2 with 
    | TType_var r , _  | _, TType_var r -> true
    | TType_app (tc1,l1)  ,TType_app (tc2,l2) when tcref_eq g tc1 tc2  ->  
        List.lengthsEqAndForall2 (type_feasibly_equiv ndeep g amap m) l1 l2
    | TType_tuple l1    ,TType_tuple l2     -> 
        List.lengthsEqAndForall2 (type_feasibly_equiv ndeep g amap m) l1 l2 
    | TType_fun (d1,r1)  ,TType_fun (d2,r2)   -> 
        (type_feasibly_equiv ndeep g amap m) d1 d2 && (type_feasibly_equiv ndeep g amap m) r1 r2
    | TType_measure ms1, TType_measure ms2 ->
        measure_equiv g ms1 ms2
    | _ -> 
        false


/// Choose solutions for TExpr_tchoose type "hidden" variables introduced
/// by letrec nodes. Also used by the pattern match compiler to choose type
/// variables when compiling patterns at generalized bindings.
///     e.g. let ([],x) = ([],[])
/// Here x gets a generalized type "list<'a>".
let choose_typar_solution_and_range g amap (tp:Typar) =
    let m = tp.Range
    if verbose then dprintf "choose_typar_solution, arbitrary: tp = %s\n" (Layout.showL (TyparsL [tp]));
    let max,m = 
         List.fold (fun (maxSoFar,_) tpc -> 
             let join m x = 
               if type_feasibly_subsumes_type 0 g amap m x CanCoerce maxSoFar then maxSoFar
               elif type_feasibly_subsumes_type 0 g amap m maxSoFar CanCoerce x then x
               else 
                   errorR(Error(Printf.sprintf "The implicit instantiation of a generic construct at or near this point could not be resolved because it could resolve to multiple unrelated types, e.g. '%s' and '%s'. Consider using type annotations to resolve the ambiguity" (DebugPrint.showType x) (DebugPrint.showType maxSoFar),m)); maxSoFar
             (* Don't continue if an error occurred and we set the value eagerly *)
             if tpref_is_solved tp then maxSoFar,m else
             match tpc with 
             | TTyparCoercesToType(x,m) -> 
                 join m x,m
             | TTyparMayResolveMemberConstraint(TTrait(_,nm,_,_,_,_),m) -> 
                 errorR(Error("Could not resolve the ambiguity inherent in the use of the overloaded operator '"^DemangleOperatorName nm^"' at or near this program point. Consider using type annotations to resolve the ambiguity",m));
                 maxSoFar,m
             | TTyparSimpleChoice(_,m) -> 
                 errorR(Error("Could not resolve the ambiguity inherent in the use of a 'printf'-style format string",m));
                 maxSoFar,m
             | TTyparSupportsNull m -> 
                 maxSoFar,m
             | TTyparIsEnum(_,m) -> 
                 errorR(Error("Could not resolve the ambiguity in the use of a generic construct with an 'enum' constraint at or near this position",m));
                 maxSoFar,m
             | TTyparIsDelegate(_,_,m) -> 
                 errorR(Error("Could not resolve the ambiguity in the use of a generic construct with a 'delegate' constraint at or near this position",m));
                 maxSoFar,m
             | TTyparIsNotNullableValueType m -> 
                 join m g.int_ty,m
             | TTyparRequiresDefaultConstructor m -> 
                 (* errorR(Error("Could not resolve the ambiguity inherent in the use of a generic construct at or near this program point. Consider using type annotations to resolve the ambiguity",m)); *)
                 maxSoFar,m
             | TTyparIsReferenceType m -> 
                 maxSoFar,m
             | TTyparDefaultsToType(priority,ty,m) -> 
                 maxSoFar,m)
           ((match tp.Kind with KindType -> g.obj_ty | KindMeasure -> TType_measure MeasureOne),m)
           tp.Constraints
    max,m

let choose_typar_solution g amap tp = 
    let ty,m = choose_typar_solution_and_range g amap tp
    if tp.Rigidity = TyparAnon && type_equiv g ty (TType_measure MeasureOne)
    then warning(Error("This code is less generic than indicated by its annotations. A unit-of-measure specified using '_' has been determined to be '1', i.e. dimensionless. Consider making the code generic, or removing the use of '_'",tp.Range));
    ty

let choose_typar_solutions_for_tchoose g amap e = 
    match e with 
    | TExpr_tchoose(tps,e1,m)  -> 
    
        /// Only make choices for variables that are actually used in the expression 
        let ftvs = (free_in_expr CollectTyparsNoCaching e1).FreeTyvars.FreeTypars
        let tps = tps |> List.filter (Zset.mem_of ftvs)
        
        let tpenv = mk_typar_inst tps (List.map (choose_typar_solution g amap) tps)
        inst_expr g tpenv e1
    | _ -> e
                 

/// Break apart lambdas. Needs choose_typar_solutions_for_tchoose because it's used in
/// PostTypecheckSemanticChecks before we've eliminated these nodes.
let try_dest_top_lambda_upto g amap (TopValInfo (tpNames,_,_) as tvd) (e,ty) =
    let rec strip_lambda_upto n (e,ty) = 
        match e with 
        | TExpr_lambda (_,None,v,b,_,retTy,_) when n > 0 -> 
            let (vs',b',retTy') = strip_lambda_upto (n-1) (b,retTy)
            (v :: vs', b', retTy') 
        | _ -> ([],e,ty)

    let rec start_strip_lambda_upto n (e,ty) = 
        match e with 
        | TExpr_lambda (_,basevopt,v,b,_,retTy,_) when n > 0 -> 
            let (vs',b',retTy') = strip_lambda_upto (n-1) (b,retTy)
            (basevopt, (v :: vs'), b', retTy') 
        | TExpr_tchoose (tps,b,_) -> 
            start_strip_lambda_upto n (choose_typar_solutions_for_tchoose g amap e, ty)
        | _ -> (None,[],e,ty)

    let n = tvd.NumCurriedArgs
    let tps,taue,tauty = 
        match e with 
        | TExpr_tlambda (_,tps,b,_,retTy,_) when nonNil tpNames -> tps,b,retTy 
        | _ -> [],e,ty
    let basevopt,vsl,body,retTy = start_strip_lambda_upto n (taue,tauty)
    if vsl.Length <> n then 
        None 
    else
        Some (tps,basevopt,vsl,body,retTy)

let dest_top_lambda_upto g amap topValInfo (e,ty) = 
    match try_dest_top_lambda_upto g amap topValInfo (e,ty) with 
    | None -> error(Error("Invalid value", range_of_expr e));
    | Some res -> res
    
let IteratedAdjustArityOfLambdaBody g arities vsl body  =
      (arities, vsl, ([],body)) |||> List.foldBack2 (fun arities vs (allvs,body) -> 
          let vs,body = AdjustArityOfLambdaBody g arities vs body
          vs :: allvs, body)

/// Do AdjustArityOfLambdaBody for a series of  
/// iterated lambdas, producing one method.  
/// The required iterated function arity (List.length topValInfo) must be identical 
/// to the iterated function arity of the input lambda (List.length vsl) 
let IteratedAdjustArityOfLambda g amap topValInfo e =
    let tps,basevopt,vsl,body,bodyty = dest_top_lambda_upto g amap topValInfo (e, type_of_expr g e)
    let arities = topValInfo.AritiesOfArgs
    if List.length arities <> List.length vsl then (
      errorR(InternalError(sprintf "IteratedAdjustArityOfLambda, List.length arities = %d, List.length vsl = %d" (List.length arities) (List.length vsl), range_of_expr body))
    );
    let vsl,body = IteratedAdjustArityOfLambdaBody g arities vsl body
    tps,basevopt,vsl,body,bodyty


exception RequiredButNotSpecified of DisplayEnv * Tast.ModuleOrNamespaceRef * string * (StringBuilder -> unit) * range
exception ValueNotContained       of DisplayEnv * Tast.ModuleOrNamespaceRef * Val * Val * string
exception ConstrNotContained      of DisplayEnv * UnionCase * UnionCase * string
exception ExnconstrNotContained   of DisplayEnv * Tycon * Tycon * string
exception FieldNotContained       of DisplayEnv * RecdField * RecdField * string 
exception InterfaceNotRevealed    of DisplayEnv * Tast.typ * range


/// Containment relation for module types
module SignatureConformance = begin

    let rec internal CheckTypars g denv m aenv (atps:typars) (ftps:typars) = 
        if atps.Length <> ftps.Length then  (errorR (Error("The signature and implementation are not compatible because the respective type parameter counts differ",m)); false)
        else 
          let aenv = bind_tyeq_env_typars atps ftps aenv
          (atps,ftps) ||> List.forall2 (fun atp ftp -> 
              let m = ftp.Range
              if atp.StaticReq <> ftp.StaticReq then 
                  errorR (Error("The signature and implementation are not compatible because the type parameter in the class/signature has a different compile-time requirement to the one in the member/implementation", m));          
              
              // Adjust the actual type parameter name to look look like the signature
              atp.Data.typar_id <- mksyn_id atp.Range ftp.Id.idText     
              // Mark it as "not compiler generated", now that we've got a good name for it
              set_compgen_of_tpdata atp.Data false 

              atp.Constraints |> List.forall (fun atpc -> 
                  match atpc with 
                  // defaults can be dropped in the signature 
                  | TTyparDefaultsToType(_,acty,_) -> true
                  | _ -> 
                      if not (List.exists  (typarConstraints_aequiv g aenv atpc) ftp.Constraints)
                      then (errorR(Error("The signature and implementation are not compatible because the declaration of the type parameter '"^ftp.Name^"' requires a constraint of the form "^Layout.showL(NicePrint.constraintL denv (atp,atpc)),m)); false)
                      else  true) &&
              ftp.Constraints |> List.forall (fun ftpc -> 
                  match ftpc with 
                  // defaults can be present in the signature and not in the implementation 
                  | TTyparDefaultsToType(_,acty,_) -> true
                  | _ -> 
                      if not (List.exists  (fun atpc -> typarConstraints_aequiv g aenv atpc ftpc) atp.Constraints)
                      then (errorR(Error("The signature and implementation are not compatible because the type parameter '"^ftp.Name^"' has a constraint of the form "^Layout.showL(NicePrint.constraintL denv (ftp,ftpc))^" but the implementation does not. Either remove this constraint from the signature or add it to the implementation",m)); false)
                      else  true))

(*
    and private CheckAttribs g amap denv aenv (actualAttribs:Attribs) (formalAttribs:Attribs) =
        let allActualAttribs = new ResizeArray<_>()
        
        let mutable remaining = List.map (fun x -> (x,true)) actualAttribs @  List.map (fun x -> (x,false)) formalAttribs 
        while nonNil remaining do 
            let (Attrib(tcref,_,_,_,_),isActual) = List.hd remaining
            let sameTypeActual, rest =  remaining |> List.partition (fun (Attrib(tcref2,_,_,_,_),isActual2) -> isActual2 && tcref_aequiv g aenv tcref tcref2) |> pair_map (List.map fst) id
            let sameTypeFormal, rest =  rest |> List.partition (fun (Attrib(tcref2,_,_,_,_),isActual2) -> not isActual2 && tcref_aequiv g aenv tcref tcref2) |> pair_map (List.map fst) id
            let sameActual, sameTypeActual =  sameTypeActual |> List.partition (fun (Attrib(_,kind2,exprs2,namedArgs2,_)) -> true) 
            let sameFormal, sameFormalActual =  sameTypeFormal  |> List.partition (fun (Attrib(_,kind2,exprs2,namedArgs2,_)) -> true) 
            match sameFormal.Length, sameActual.Length with 
            | n,m when n = m -> for x in sameActual do allActualAttribs.Add (sameActual)
            | 1,m when n = m -> for x in sameActual do allActualAttribs.Add (sameActual)
            remaining <- rest
            
        true
*)

    and private CheckTypeDef g amap denv aenv (atc:Tycon) (ftc:Tycon) =
        let m = atc.Range
        let err s =  Error("The " ^ atc.TypeOrMeasureKind.ToString() ^ " definitions in the signature and implementation are not compatible because "^s,m)
        if atc.MangledName <> ftc.MangledName then  (errorR (err "the names differ"); false) else
        CheckExnInfo g m denv  (fun s -> ExnconstrNotContained(denv,atc,ftc,s)) aenv atc.ExceptionInfo ftc.ExceptionInfo &&
        let atps = atc.Typars(m)
        let ftps = ftc.Typars(m)
        if List.length atps <> List.length ftps then  (errorR (err("the respective type parameter counts differ")); false)
        elif IsLessAccessible atc.Accessibility ftc.Accessibility then (errorR(err "the accessibility specified in the signature is more than that specified in the implementation"); false)
        else 
          let aenv = bind_tyeq_env_typars atps ftps aenv
          let aintfs = List.map p13 (List.filter (fun (_,compgen,_) -> not compgen) atc.TypeContents.tcaug_implements)
          let fintfs = List.map p13 (List.filter (fun (_,compgen,_) -> not compgen) ftc.TypeContents.tcaug_implements)
          let aintfs = ListSet.setify (type_equiv g) (List.collect (AllSuperTypesOfType g amap m) aintfs)
          let fintfs = ListSet.setify (type_equiv g) (List.collect (AllSuperTypesOfType g amap m) fintfs)
        
          let unimpl = ListSet.subtract (fun fity aity -> type_aequiv g aenv aity fity) fintfs aintfs
          (unimpl |> List.forall (fun ity -> errorR (err ("the signature requires that the type supports the interface "^NicePrint.pretty_string_of_typ denv ity^" but the interface has not been implemented")); false)) &&
          let hidden = ListSet.subtract (type_aequiv g aenv) aintfs fintfs
          hidden |> List.iter (fun ity -> (if atc.IsFSharpInterfaceTycon then error else warning) (InterfaceNotRevealed(denv,ity,atc.Range)));
          let aNull = IsUnionTypeWithNullAsTrueValue g atc
          let fNull = IsUnionTypeWithNullAsTrueValue g ftc
          if aNull && not fNull then 
            errorR(err("the implementation says this type may use nulls as a representation but the signature does not"))
          elif fNull && not aNull then 
            errorR(err("the signature says this type may use nulls as a representation but the implementation does not"));
          let aSealed = is_sealed_typ g (snd (generalize_tcref (mk_local_tcref atc)))
          let fSealed = is_sealed_typ g (snd (generalize_tcref (mk_local_tcref ftc)))
          if  aSealed && not fSealed  then 
            errorR(err("the implementation type is sealed but the signature implies it is not. Consider adding the [<Sealed>] attribute to the signature"));
          if  not aSealed && fSealed  then 
            errorR(err("the implementation type is not sealed but signature implies is. Consider adding the [<Sealed>] attribute to the implementation"));
          let aPartial = is_partially_implemented_tycon atc
          let fPartial = is_partially_implemented_tycon ftc
          if aPartial && not fPartial then 
            errorR(err("the implementation is an abstract class but the signature is not. Consider adding the [<AbstractClass>] attribute to the signature"));
          if not aPartial && fPartial then 
            errorR(err("the signature is an abstract class but the implementation is not. Consider adding the [<AbstractClass>] attribute to the implementation"));
          if not (type_aequiv g aenv (super_of_tycon g atc) (super_of_tycon g ftc)) then 
              errorR (err("the types have different base types"));
          CheckTypars g denv m aenv atps ftps &&
          CheckTypeRepr g denv err aenv atc.TypeReprInfo ftc.TypeReprInfo &&
          CheckTypeAbbrev g denv err aenv atc.TypeOrMeasureKind ftc.TypeOrMeasureKind atc.TypeAbbrev ftc.TypeAbbrev 
        
    and private CheckValInfo err (id:ident) aarity farity = 
        match aarity,farity with 
        | _,None -> true
        | None, Some _ -> err("An arity was not inferred for this value")
        | Some (TopValInfo (tpNames1,_,_) as info1), Some (TopValInfo (tpNames2,_,_) as info2) ->
            let ntps = tpNames1.Length
            let mtps = tpNames2.Length
            let n = info1.AritiesOfArgs
            let m = info2.AritiesOfArgs
            if ntps = mtps && m.Length <= n.Length && List.forall2 (fun x y -> x <= y) m (fst (List.chop m.Length n)) then true
            elif ntps <> mtps then
              err("The number of generic parameters in the signature and implementation differ (the signature declares "^string mtps^" but the implementation declares "^string ntps) 
            elif info1.KindsOfTypars <> info2.KindsOfTypars then
              err("The generic parameters in the signature and implementation have different kinds. Perhaps there is a missing [<Measure>] attribute")
            else 
              err("The arities in the signature and implementation differ. The signature specifies that '"^id.idText^"' is function definition or lambda expression accepting at least "^string (List.length m)^" argument(s), but the implementation is a computed function value. To declare that a computed function value is a permitted implementation simply parenthesize its type in the signature, e.g.\n\tval "^id.idText^": int -> (int -> int)\ninstead of\n\tval "^id.idText^": int -> int -> int")

    and private CheckVal g amap denv implModRef aenv (implVal:Val) (sigVal:Val) =

        // Propagate defn location information from implementation to signature . 
        sigVal.Data.val_defn_range <- implVal.DefinitionRange;

        if verbose then  dprintf "checking value %s, %d, %d\n" implVal.MangledName implVal.Stamp sigVal.Stamp;
        let mk_err denv s = ValueNotContained(denv,implModRef,implVal,sigVal,s)
        let err denv s = errorR(mk_err denv s); false
        let m = implVal.Range
        if implVal.IsMutable <> sigVal.IsMutable then (err denv "The mutability attributes differ")
        elif implVal.MangledName <> sigVal.MangledName then (err denv "The names differ")
        elif IsLessAccessible implVal.Accessibility sigVal.Accessibility then (err denv "The accessibility specified in the signature is more than that specified in the implementation")
        elif implVal.MustInline <> sigVal.MustInline then (err denv "The inline flags differ")
        elif implVal.LiteralValue <> sigVal.LiteralValue then (err denv "The literal constant values and/or attributes differ")
        elif implVal.IsTypeFunction <> sigVal.IsTypeFunction then (err denv "One is a type function and the other is not. The signature requires explicit type parameters if they are present in the implementation")
        else 
            let atps,atau = implVal.TypeScheme
            let ftps,ftau = sigVal.TypeScheme
            if atps.Length <> ftps.Length then (err {denv with showTyparBinding=true} "The respective type parameter counts differ") else
            let aenv = bind_tyeq_env_typars atps ftps aenv
            CheckTypars g denv m aenv atps ftps &&
            let res = 
              if not (type_aequiv g aenv atau ftau) then err denv "The types differ" 
              elif not (CheckValInfo (err denv) implVal.Id implVal.TopValInfo sigVal.TopValInfo) then false
              elif not (implVal.IsExtensionMember = sigVal.IsExtensionMember) then err denv "One is an extension member and the other is not"
              elif not (CheckMemberDatasConform g (err denv) (implVal.Attribs, implVal,implVal.MemberInfo) (sigVal.Attribs,sigVal,sigVal.MemberInfo)) then false
              else true

            // Propagate information signature to implementation

            // Update the arity of the value to reflect the constraint of the signature 
            // This ensures that the compiled form of the value matches the signature rather than 
            // the implementation. This also propagates argument names from signature to implementation
            implVal.Data.val_top_repr_info <- sigVal.Data.val_top_repr_info;
            res

    and private CheckExnInfo g m denv err aenv arepr frepr =
        match arepr,frepr with 
        | TExnAsmRepr _, TExnFresh _ -> 
            (errorR (err "a .NET exception mapping is being hidden by a signature. The exception mapping must be visible to other modules"); false)
        | TExnAsmRepr tcr1, TExnAsmRepr tcr2  -> 
            if tcr1 <> tcr2 then  (errorR (err "the .NET representations differ"); false) else true
        | TExnAbbrevRepr _, TExnFresh _ -> 
            (errorR (err "the exception abbreviation is being hidden by the signature. The abbreviation must be visible to other .NET languages. Consider making the abbreviation visible in the signature"); false)
        | TExnAbbrevRepr ecr1, TExnAbbrevRepr ecr2 -> 
            if not (tcref_aequiv g aenv ecr1 ecr2) then 
              (errorR (err "the exception abbreviations in the signature and implementation differ"); false)
            else true
        | TExnFresh r1, TExnFresh  r2-> CheckRecordFields g denv err aenv r1 r2
        | TExnNone,TExnNone -> true
        | _ -> 
            (errorR (err "the exception declrations differ"); false)

    and private CheckUnionCase g denv aenv c1 c2 =
        let err msg = errorR(ConstrNotContained(denv,c1,c2,msg));false
        if c1.ucase_id.idText <> c2.ucase_id.idText then  err "The names differ"
        elif c1.RecdFields.Length <> c2.RecdFields.Length then err "The respective number of data fields differ"
        elif not (List.forall2 (CheckField g denv aenv) c1.RecdFields c2.RecdFields) then err "The types of the fields differ"
        elif IsLessAccessible c1.Accessibility c2.Accessibility then err "the accessibility specified in the signature is more than that specified in the implementation"
        else true

    and private CheckField g denv aenv f1 f2 =
        let err msg = errorR(FieldNotContained(denv,f1,f2,msg)); false
        if f1.rfield_id.idText <> f2.rfield_id.idText then err "The names differ"
        elif IsLessAccessible f1.Accessibility f2.Accessibility then err "the accessibility specified in the signature is more than that specified in the implementation"
        elif f1.IsStatic <> f2.IsStatic then err "The 'static' modifiers differ"
        elif f1.IsMutable <> f2.IsMutable then err "The 'mutable' modifiers differ"
        elif f1.LiteralValue <> f2.LiteralValue then err "The 'literal' modifiers differ"
        elif not (type_aequiv g aenv f1.FormalType f2.FormalType) then err "The types differ"
        else true

    and private CheckMemberDatasConform g err  (implAttrs,implVal,implMemberInfo) (sigAttrs, sigVal,sigMemberInfo)  =
        match implMemberInfo,sigMemberInfo with 
        | None,None -> true
        | Some avspr, Some fvspr -> 
            if not (avspr.CompiledName = fvspr.CompiledName) then 
              err("The .NET member names differ")
            elif not (avspr.MemberFlags.OverloadQualifier = fvspr.MemberFlags.OverloadQualifier) then 
              err("The overload resolution identifier attributes differ")
            elif not (avspr.MemberFlags.MemberIsInstance = fvspr.MemberFlags.MemberIsInstance) then 
              err("One is static and the other isn't")
            elif not (avspr.MemberFlags.MemberIsVirtual = fvspr.MemberFlags.MemberIsVirtual) then 
              err("One is virtual and the other isn't")
            elif not (avspr.MemberFlags.MemberIsDispatchSlot = fvspr.MemberFlags.MemberIsDispatchSlot) then 
              err("One is abstract and the other isn't")
           (* I've weakened this check: *)
           (*     classes have non-final CompareTo/Hash methods *)
           (*     abstract have non-final CompareTo/Hash methods *)
           (*     records  have final CompareTo/Hash methods *)
           (*     unions  have final CompareTo/Hash methods *)
           (* Therefore it is OK for the signaure to say 'non-final' when the implementation says 'final' *)
            elif not avspr.MemberFlags.MemberIsFinal && fvspr.MemberFlags.MemberIsFinal then 
              err("One is final and the other isn't")
            elif not (avspr.MemberFlags.MemberIsOverrideOrExplicitImpl = fvspr.MemberFlags.MemberIsOverrideOrExplicitImpl) then 
              err("One is marked as an override and the other isn't")
            elif not (avspr.MemberFlags.MemberKind = fvspr.MemberFlags.MemberKind) then 
              err("One is a constructor/property and the other is not")
            else  
               let finstance = ValSpecIsCompiledAsInstance g sigVal
               let ainstance = ValSpecIsCompiledAsInstance g implVal
               if  finstance && not ainstance then 
                  err "The compiled representation of this method is as a static member but the signature indicates its compiled representation is as an instance member"
               elif not finstance && ainstance then 
                  err "The compiled representation of this method is as an instance member, but the signature indicates its compiled representation is as a static member"
               else true

        | _ -> false

    and CheckRecordFields g denv err aenv (afields:TyconRecdFields) (ffields:TyconRecdFields) =
        let afields = afields.TrueFieldsAsList
        let ffields = ffields.TrueFieldsAsList
        let m1 = afields |> NameMap.of_keyed_list (fun rfld -> rfld.Name)
        let m2 = ffields |> NameMap.of_keyed_list (fun rfld -> rfld.Name)
        NameMap.suball2 (fun s _ -> errorR(err ("the field "^s^" was required by the signature but was not specified by the implementation")); false) (CheckField g denv aenv)  m1 m2 &&
        NameMap.suball2 (fun s _ -> errorR(err ("the field "^s^" was present in the implementation but not in the signature")); false) (fun x y -> CheckField g denv aenv y x)  m2 m1 &&
        (* This check is required because constructors etc. are externally visible *)
        (* and thus compiled representations do pick up dependencies on the field order  *)
        (if List.forall2 (fun f1 f2 -> CheckField g denv aenv f1 f2)  afields ffields
         then true
         else (errorR(err ("the order of the fields is different in the signature and implementation")); false))

    and CheckVirtualSlots g denv err aenv avslots fvslots =
        let m1 = NameMap.of_keyed_list (fun (v:ValRef) -> v.MangledName) avslots
        let m2 = NameMap.of_keyed_list (fun (v:ValRef) -> v.MangledName) fvslots
        NameMap.suball2 (fun s vref -> errorR(err ("the abstract member '"^ Layout.showL(NicePrint.valL denv (deref_val vref)) ^"' was required by the signature but was not specified by the implementation")); false) (fun x y -> true)  m1 m2 &&
        NameMap.suball2 (fun s vref -> errorR(err ("the abstract member '"^ Layout.showL(NicePrint.valL denv (deref_val vref)) ^"' was present in the implementation but not in the signature")); false) (fun x y -> true)  m2 m1

    and CheckClassFields isStruct g denv err aenv (afields:TyconRecdFields) (ffields:TyconRecdFields) =
        let afields = afields.TrueFieldsAsList
        let ffields = ffields.TrueFieldsAsList
        let m1 = afields |> NameMap.of_keyed_list (fun rfld -> rfld.Name) 
        let m2 = ffields |> NameMap.of_keyed_list (fun rfld -> rfld.Name) 
        NameMap.suball2 (fun s _ -> errorR(err ("the field "^s^" was required by the signature but was not specified by the implementation")); false) (CheckField g denv aenv)  m1 m2 &&
        (if isStruct then 
            NameMap.suball2 (fun s _ -> warning(err ("the field "^s^" was present in the implementation but not in the signature. Struct types must now reveal their fields in the signature for the type, though the fields may still be labelled 'private' or 'internal'")); false) (fun x y -> CheckField g denv aenv y x)  m2 m1 
         else
            true)
        

    and CheckTypeRepr g denv err aenv arepr frepr =
        let reportNiceError k s1 s2 = 
          let aset = Nameset.of_list s1
          let fset = Nameset.of_list s2
          match Zset.elements (Zset.diff aset fset) with 
          | [] -> 
              match Zset.elements (Zset.diff fset aset) with             
              | [] -> (errorR (err ("the number of "^k^"s differ")); false)
              | l -> (errorR (err ("the signature defines the "^k^" '"^String.concat ";" l^"' but the implementation does not (or does, but not in the same order)")); false)
          | l -> (errorR (err ("the implementation defines the "^k^" '"^String.concat ";" l^"' but the signature does not (or does, but not in the same order)")); false)

        match arepr,frepr with 
        | Some (TRecdRepr _ | TFiniteUnionRepr _ | TILObjModelRepr _ ), None  -> true
        | Some (TFsObjModelRepr r), None  -> 
            if r.fsobjmodel_kind = TTyconStruct or r.fsobjmodel_kind = TTyconEnum then 
              (errorR (err "the implementation defines a struct but the signature defines a type with a hidden representation"); false)
            else true
        | Some (TAsmRepr _), None -> 
            (errorR (err "a .NET type representation is being hidden by a signature"); false)
        | Some (TMeasureableRepr _), None -> 
            (errorR (err "a type representation is being hidden by a signature"); false)
        | Some (TFiniteUnionRepr r1), Some (TFiniteUnionRepr r2) -> 
            let ucases1 = r1.UnionCasesAsList
            let ucases2 = r2.UnionCasesAsList
            if ucases1.Length <> ucases2.Length then
              let names l = List.map (fun c -> c.ucase_id.idText) l
              reportNiceError "union case" (names ucases1) (names ucases2) 
            else List.forall2 (CheckUnionCase g denv aenv) ucases1 ucases2
        | Some (TRecdRepr afields), Some (TRecdRepr ffields) -> 
            CheckRecordFields g denv err aenv afields ffields
        | Some (TFsObjModelRepr r1), Some (TFsObjModelRepr r2) -> 
            if not (match r1.fsobjmodel_kind,r2.fsobjmodel_kind with 
                     | TTyconClass,TTyconClass -> true
                     | TTyconInterface,TTyconInterface -> true
                     | TTyconStruct,TTyconStruct -> true
                     | TTyconEnum, TTyconEnum -> true
                     | TTyconDelegate (TSlotSig(nm1,typ1,ctps1,mtps1,ps1, rty1)), 
                       TTyconDelegate (TSlotSig(nm2,typ2,ctps2,mtps2,ps2, rty2)) -> 
                         (type_aequiv g aenv typ1 typ2) &&
                         (ctps1.Length = ctps2.Length) &&
                         (let aenv = bind_tyeq_env_typars ctps1 ctps2 aenv
                          (typar_decls_aequiv g aenv ctps1 ctps2) &&
                          (mtps1.Length = mtps2.Length) &&
                          (let aenv = bind_tyeq_env_typars mtps1 mtps2 aenv
                           (typar_decls_aequiv g aenv mtps1 mtps2) &&
                           ((ps1,ps2) ||> List.lengthsEqAndForall2 (List.lengthsEqAndForall2 (fun p1 p2 -> type_aequiv g aenv p1.Type p2.Type))) &&
                           (return_types_aequiv g aenv rty1 rty2)))
                     | _,_ -> false) then 
              (errorR (err "the types are of different kinds"); false)
            else 
              CheckClassFields (r1.fsobjmodel_kind = TTyconStruct) g denv err aenv r1.fsobjmodel_rfields r2.fsobjmodel_rfields &&
              CheckVirtualSlots g denv err aenv r1.fsobjmodel_vslots r2.fsobjmodel_vslots
        | Some (TAsmRepr tcr1),  Some (TAsmRepr tcr2) -> 
            if tcr1 <> tcr2 then  (errorR (err "the IL representations differ"); false) else true
        | Some (TMeasureableRepr ty1),  Some (TMeasureableRepr ty2) -> 
            if type_aequiv g aenv ty1 ty2 then true else (errorR (err "the representations differ"); false)
        | Some _, Some _ -> (errorR (err "the representations differ"); false)
        | None, Some _ -> (errorR (err "the representations differ"); false)
        | None, None -> true

    and CheckTypeAbbrev g denv err aenv kind1 kind2 abbrev1 abbrev2 =
        if kind1 <> kind2 then (errorR (err ("the signature declares a " ^ kind2.ToString() ^ " while the implementation declares a " ^ kind1.ToString())); false)
        else
          match abbrev1,abbrev2 with 
          | Some ty1, Some ty2 -> if not (type_aequiv g aenv ty1 ty2) then (errorR (err ("the abbreviations differ: " ^ Layout.showL (typeL ty1) ^ " vs " ^  Layout.showL (typeL ty2))); false) else true
          | None,None -> true
          | Some _, None -> (errorR (err ("an abbreviation is being hidden by a signature. The abbreviation must be visible to other .NET languages. Consider making the abbreviation visible in the signature")); false)
          | None, Some _ -> (errorR (err "the signature has an abbreviation while the implementation does not"); false)

    and CheckModuleOrNamespaceContents m g amap denv aenv (implModRef:ModuleOrNamespaceRef) (signModType:ModuleOrNamespaceType) = 
        let implModType = implModRef.ModuleOrNamespaceType
        (if implModType.ModuleOrNamespaceKind <> signModType.ModuleOrNamespaceKind then errorR(Error("The namespace or module attributes differ between signature and implementation",m)));
        NameMap.suball2 
            (fun s fx -> errorR(RequiredButNotSpecified(denv,implModRef,"type",(fun os -> Printf.bprintf os "%s" s),m)); false) 
            (CheckTypeDef g amap denv aenv)  
            implModType.TypesByMangledName 
            signModType.TypesByMangledName &&

        (implModType.ModulesAndNamespacesByDemangledName, signModType.ModulesAndNamespacesByDemangledName ) 
          ||> NameMap.suball2 
               (fun s fx -> errorR(RequiredButNotSpecified(denv,implModRef,(if fx.IsModule then "module" else "namespace"),(fun os -> Printf.bprintf os "%s" s),m)); false) 
               (fun x1 (x2:ModuleOrNamespace) -> CheckModuleOrNamespace g amap denv aenv (mk_local_modref x1) x2.ModuleOrNamespaceType)  &&

        NameMap.suball2 
            (fun s (fx:Val) -> 
                errorR(RequiredButNotSpecified(denv,implModRef,"value",(fun os -> 
                   (* In the case of missing members show the full required enclosing type and signature *)
                   if fx.IsMember then 
                       Printf.bprintf os "%a" (NicePrint.output_qualified_val_spec denv) fx
                   else
                       Printf.bprintf os "%s" fx.DisplayName),m)); false)
            (CheckVal g amap denv implModRef aenv) 
            implModType.AllValuesAndMembers 
            signModType.AllValuesAndMembers

    and CheckModuleOrNamespace g amap denv aenv (implModRef:ModuleOrNamespaceRef) signModType = 
        CheckModuleOrNamespaceContents implModRef.Range g amap denv aenv implModRef signModType


    /// Check the names add up between a signature and its implementation. We check this first.
    let rec CheckNamesOfModuleOrNamespaceContents denv (implModRef:ModuleOrNamespaceRef) (signModType:ModuleOrNamespaceType) = 
        let m = implModRef.Range 
        let implModType = implModRef.ModuleOrNamespaceType
        NameMap.suball2 
            (fun s fx -> errorR(RequiredButNotSpecified(denv,implModRef,"type",(fun os -> Printf.bprintf os "%s" s),m)); false) 
            (fun _ _ -> true)  
            implModType.TypesByMangledName 
            signModType.TypesByMangledName &&

        (implModType.ModulesAndNamespacesByDemangledName, signModType.ModulesAndNamespacesByDemangledName ) 
          ||> NameMap.suball2 
                (fun s fx -> errorR(RequiredButNotSpecified(denv,implModRef,(if fx.IsModule then "module" else "namespace"),(fun os -> Printf.bprintf os "%s" s),m)); false) 
                (fun x1 (x2:ModuleOrNamespace) -> CheckNamesOfModuleOrNamespace denv (mk_local_modref x1) x2.ModuleOrNamespaceType)  &&

        NameMap.suball2 
            (fun s (fx:Val) -> 
                errorR(RequiredButNotSpecified(denv,implModRef,"value",(fun os -> 
                   (* In the case of missing members show the full required enclosing type and signature *)
                   if isSome (fx.MemberInfo) then 
                       Printf.bprintf os "%a" (NicePrint.output_qualified_val_spec denv) fx
                   else
                       Printf.bprintf os "%s" fx.DisplayName),m)); false)
            (fun _ _ -> true) 
            implModType.AllValuesAndMembers 
            signModType.AllValuesAndMembers


    and CheckNamesOfModuleOrNamespace denv (implModRef:ModuleOrNamespaceRef) signModType = 
        CheckNamesOfModuleOrNamespaceContents denv implModRef signModType

end

//-------------------------------------------------------------------------
// Completeness of classes
//------------------------------------------------------------------------- 

type OverrideCanImplement = 
    | CanImplementAnyInterfaceSlot
    | CanImplementAnyClassHierarchySlot
    | CanImplementAnySlot
    | CanImplementNoSlots
    
type OverrideInfo = 
    | Override of OverrideCanImplement * ident * (typars * TyparInst) * Tast.typ list list * Tast.typ option * (*isFakeEventProperty:*)bool
    member x.IsFakeEventProperty = let (Override(_,_,_,_,_,b)) = x in b
    member x.LogicalName = let (Override(_,id,_,_,_,_)) = x in id.idText
    member x.Range = let (Override(_,id,_,_,_,_)) = x in id.idRange

type AvailPriorPropertySlotImpl = AvailPriorPropertySlotImpl of PropInfo

type SlotImplSet = SlotImplSet of MethInfo list * NameMultiMap<MethInfo> * OverrideInfo list * PropInfo list

exception TypeIsImplicitlyAbstract of range
exception OverrideDoesntOverride of DisplayEnv * OverrideInfo * MethInfo option * TcGlobals * Import.ImportMap * range



module DispatchSlotChecking =
    /// The overall information about a method implementation in a class or obeject expression 

    let PrintOverrideToBuffer denv os (Override(_,id,(mtps,memberToParentInst),argTys,retTy,_)) = 
       let denv = { denv with showTyparBinding = true }
       let retTy = (retTy  |> GetFSharpViewOfReturnType denv.g)
       let argInfos = 
           match argTys with 
           | [] -> [[(denv.g.unit_ty,TopValInfo.unnamedTopArg1)]]
           | _ -> argTys |> List.mapSquared (fun ty -> (ty, TopValInfo.unnamedTopArg1)) 
       Layout.bufferL os (NicePrint.memberSigL denv (memberToParentInst,id.idText,mtps, argInfos, retTy))

    let PrintMethInfoSigToBuffer g amap m denv os minfo =
        let denv = { denv with showTyparBinding = true }
        let argTys,retTy,fmtps,ttpinst = CompiledSigOfMeth g amap m minfo
        let retTy = (retTy  |> GetFSharpViewOfReturnType g)
        let argInfos = argTys |> List.mapSquared (fun ty -> (ty, TopValInfo.unnamedTopArg1))
        let nm = minfo.LogicalName
        Layout.bufferL os (NicePrint.memberSigL denv (ttpinst,nm,fmtps, argInfos, retTy))

    let FormatOverride denv d = bufs (fun buf -> PrintOverrideToBuffer denv buf d)
    let FormatMethInfoSig g amap m denv d = bufs (fun buf -> PrintMethInfoSigToBuffer g amap m denv buf d)

    let GetInheritedMemberOverrideInfo g amap m parentType (minfo:MethInfo) = 
        let nm = minfo.LogicalName
        let argTys,retTy,fmtps,ttpinst = CompiledSigOfMeth g amap m minfo

        let isFakeEventProperty = minfo.IsFSharpEventProperty
        Override(parentType,mksyn_id m nm, (fmtps,ttpinst),argTys,retTy,isFakeEventProperty)

    let GetTypeMemberOverrideInfo g reqdTy (overrideBy:ValRef) = 
        let _,argInfos,retTy,_ = GetTypeOfMemberInMemberForm g overrideBy
        let nm = overrideBy.MemberInfo.Value.LogicalName

        let argTys = argInfos |> List.mapSquared fst
        
        let memberMethodTypars,memberToParentInst,argTys,retTy = 
            match PartitionValRefTypars g overrideBy with
            | Some(_,_,memberMethodTypars,memberToParentInst,tinst) -> 
                let argTys = argTys |> List.mapSquared (InstType memberToParentInst) 
                let retTy = retTy |> Option.map (InstType memberToParentInst) 
                memberMethodTypars, memberToParentInst,argTys, retTy
            | None -> 
                error(Error("this method is over-constrained in its type parameters",overrideBy.Range))
        let implKind = 
            if MemberIsExplicitImpl g overrideBy.MemberInfo.Value then 
                
                let belongsToReqdTy = 
                    match overrideBy.MemberInfo.Value.ImplementedSlotSigs with
                    | [] -> false
                    | ss :: _ -> is_interface_typ g ss.ImplementedType && type_equiv g reqdTy ss.ImplementedType
                if belongsToReqdTy then 
                    CanImplementAnyInterfaceSlot
                else
                    CanImplementNoSlots
            else if MemberRefIsDispatchSlot overrideBy then 
                CanImplementNoSlots
                // abstract slots can only implement interface slots
                //CanImplementAnyInterfaceSlot  <<----- Change to this to enable implicit interface implementation
            
            else 
                CanImplementAnyClassHierarchySlot
                //CanImplementAnySlot  <<----- Change to this to enable implicit interface implementation

        let isFakeEventProperty = overrideBy.IsFSharpEventProperty(g)
        Override(implKind,mksyn_id overrideBy.Range nm, (memberMethodTypars,memberToParentInst),argTys,retTy,isFakeEventProperty)

    let GetObjectExprOverrideInfo g amap (implty,id:ident,memberFlags,ty,arityInfo,expr) = 
        if verbose then  dprintf "--> GetObjectExprOverrideInfo\n";
        // Dissect the type
        let tps,argInfos,retTy,_ = GetMemberTypeInMemberForm g memberFlags arityInfo ty id.idRange
        // Drop 'this'
        let argTys = argInfos |> List.mapSquared fst
        // Dissect the implementation
        let _,basevopt,vsl,body,_ = dest_top_lambda_upto g amap arityInfo (expr,ty)
        match vsl with 
        | [thisv]::vs -> 
            // Check for empty variable list from a () arg
            let vs = if vs.Length = 1 && argInfos.IsEmpty then [] else vs
            let implKind = 
                if is_interface_typ g implty then 
                    CanImplementAnyInterfaceSlot 
                else 
                    CanImplementAnyClassHierarchySlot
                    //CanImplementAnySlot  <<----- Change to this to enable implicit interface implementation
            let overrideByInfo = Override(implKind,id,(tps,[]),argTys,retTy,false )
            overrideByInfo,(basevopt,thisv,vs,body)
        | _ -> 
            error(InternalError("Unexpected shape for object expression override",id.idRange))
          
    let is_name_match (dispatchSlot:MethInfo) (overrideBy: OverrideInfo) = 
        (overrideBy.LogicalName = dispatchSlot.LogicalName)
          
    let is_impl_match g amap m (dispatchSlot:MethInfo) (Override(implKind,_,_,_,_,_)) = 
        // If the override is listed as only relevant to one type, and we're matching it against an abstract slot of an interface type,
        // then check that interface type is the right type.
        (match implKind with 
         | CanImplementNoSlots -> false
         | CanImplementAnySlot -> true 
         | CanImplementAnyClassHierarchySlot -> not (is_interface_typ g dispatchSlot.EnclosingType)
         //| CanImplementSpecificInterfaceSlot parentTy -> is_interface_typ g dispatchSlot.EnclosingType && type_equiv g parentTy dispatchSlot.EnclosingType 
         | CanImplementAnyInterfaceSlot -> is_interface_typ g dispatchSlot.EnclosingType)

    let is_typar_kind_match g amap m (dispatchSlot:MethInfo) (Override(_,_,(mtps,_),_,_,_) as overrideBy) = 
        let vargtys,_,fvmtps,_ = CompiledSigOfMeth g amap m dispatchSlot 
        List.lengthsEqAndForall2 (fun (tp1:Typar) (tp2:Typar) -> tp1.Kind = tp2.Kind) mtps fvmtps
        
    let is_partial_match g amap m (dispatchSlot:MethInfo) (Override(implKind,_,(mtps,_),argTys,retTy,_) as overrideBy) = 
        is_name_match dispatchSlot overrideBy &&
        let vargtys,_,fvmtps,_ = CompiledSigOfMeth g amap m dispatchSlot 
        mtps.Length = fvmtps.Length &&
        is_typar_kind_match g amap m dispatchSlot overrideBy && 
        argTys.Length = vargtys.Length &&
        is_impl_match g amap m dispatchSlot overrideBy  
          
    let reverse_renaming g tinst = 
        tinst |> List.map (fun (tp,ty) -> (dest_typar_typ g ty, mk_typar_ty tp))

    let compose_inst inst1 inst2 = 
        inst1 |> List.map (map2'2 (InstType inst2)) 
     
    let is_exact_match g amap m dispatchSlot (Override(_,id,(mtps,mtpinst),argTys,retTy,_) as overrideBy) =
        is_partial_match g amap m dispatchSlot overrideBy &&
        let vargtys,vrty,fvmtps,ttpinst = CompiledSigOfMeth g amap m dispatchSlot

        (* Compare the types. CompiledSigOfMeth, GetObjectExprOverrideInfo and GetTypeMemberOverrideInfo have already *)
        (* applied all relevant substitutions except the renamings from fvtmps <-> mtps *)

        let aenv = 
           tyeq_env_empty  
           |> bind_tyeq_env_typars fvmtps mtps

        List.forall2 (List.lengthsEqAndForall2 (type_aequiv g aenv)) vargtys argTys &&
        return_types_aequiv g aenv vrty retTy &&
        
        (* Comparing the method typars and their constraints is much trickier since the substitutions have not been applied 
           to the constraints of these babies. This is partly because constraints are directly attached to typars so it's 
           difficult to apply substitutions to them unless we separate them off at some point, which we don't as yet.        

           Given   C<ctps>
                   D<dtps>
                   dispatchSlot :   C<ctys[dtps]>.M<fvmtps[ctps]>(...)
                   overrideBy:  parent: D<dtys[dtps]>  value: !<ttps> <mtps[ttps]>(...) 
                   
               where X[dtps] indicates that X may involve free type variables dtps
               
               we have 
                   ttpinst maps  ctps --> ctys[dtps] 
                   mtpinst maps  ttps --> dtps
               
               compare fvtmps[ctps] and mtps[ttps] by 
                  fvtmps[ctps]  @ ttpinst     -- gives fvtmps[dtps]
                  fvtmps[dtps] @ rev(mtpinst) -- gives fvtmps[ttps]
                  
               Now fvtmps[ttps] and mtpinst[ttps] are comparable, i.e. have sontraints w.r.t. the same set of type variables 
                   
          i.e.  Compose the substitutions ttpinst and rev(mtpinst) *)

              
        (* Compose the substitutions *)
        
        let ttpinst = 
            (* check we can reverse - in some error recovery situations we can't *)
            if mtpinst |> List.exists (snd >> is_typar_typ g >> not) then ttpinst 
            else compose_inst ttpinst (reverse_renaming g mtpinst)

        (* Compare under the composed substitutions *)
        let aenv = bind_tyeq_env_tpinst ttpinst tyeq_env_empty
        
        typar_decls_aequiv g aenv fvmtps mtps

    /// 6a. check all interface and abstract methods are implemented 
          
    let CheckDispatchSlotsAreImplemented (denv,g,amap,m,
                                          isOverallTyAbstract,
                                          reqdTy,
                                          dispatchSlots:MethInfo list,
                                          availPriorOverridesKeyed:OverrideInfo list,
                                          overrides:OverrideInfo list) = 

        let isReqdTyInterface = is_interface_typ g reqdTy 
        let showMissingMethodsAndRaiseErrors = (isReqdTyInterface || not isOverallTyAbstract)
        let res = ref true
        let fail exn = (res := false ; if showMissingMethodsAndRaiseErrors then errorR exn)
        
        // Index the availPriorOverrides and overrides by name
        let availPriorOverridesKeyed = availPriorOverridesKeyed |> NameMultiMap.initBy (fun ov -> ov.LogicalName)
        let overridesKeyed = overrides |> NameMultiMap.initBy (fun ov -> ov.LogicalName)
        
        dispatchSlots |> List.iter (fun dispatchSlot -> 
          
            match NameMultiMap.find dispatchSlot.LogicalName overridesKeyed |> List.filter (is_exact_match g amap m dispatchSlot)  with
            | [h] -> 
                ()
            | [] -> 
                if not (NameMultiMap.find  dispatchSlot.LogicalName availPriorOverridesKeyed |> List.exists (is_exact_match g amap m dispatchSlot)) then 
                    (* error reporting path *)
                    let vargtys,vrty,fvmtps,_ = CompiledSigOfMeth g amap m dispatchSlot
                    let noimpl() = fail(Error("No implementation was given for '"^string_of_minfo amap m denv dispatchSlot^"'"^
                                                (if isReqdTyInterface then ". Note that all interface members must be implemented and listed under an appropriate 'interface' declaration, e.g. 'interface ... with member ...'"
                                                 else ""),m))
                    match  overrides |> List.filter (is_partial_match g amap m dispatchSlot)  with 
                    | [] -> 
                        match overrides |> List.filter (fun overrideBy -> is_name_match dispatchSlot overrideBy && 
                                                                            is_impl_match g amap m dispatchSlot overrideBy)  with 
                        | [] -> 
                            noimpl()
                        | [ Override(_,_,(mtps,_),argTys,_,_) as overrideBy ] -> 
                            let explanation = 
                                if argTys.Length <> vargtys.Length then "does not have the correct number of arguments"
                                elif mtps.Length <> fvmtps.Length then "does not have the correct number of method type parameters"
                                elif not (is_typar_kind_match g amap m dispatchSlot overrideBy) then  "does not have the correct kinds of generic parameters"
                                else "can not be used to implement '" ^ string_of_minfo amap m denv dispatchSlot ^ "'"
                            fail(Error("The member '"^FormatOverride denv overrideBy^"' " ^ explanation ^ ". The required signature is '"^FormatMethInfoSig g amap m denv dispatchSlot^"'",overrideBy.Range))
                        | overrideBy :: _ -> 
                            errorR(Error("No implementations of '"^FormatMethInfoSig g amap m denv dispatchSlot^"' had the correct number of arguments and type parameters. The required signature is '"^FormatMethInfoSig g amap m denv dispatchSlot^"'",overrideBy.Range))

                    | [ overrideBy ] -> 

                        match dispatchSlots  |> List.filter (fun dispatchSlot -> is_exact_match g amap m dispatchSlot overrideBy) with 
                        | [] -> 
                            // Error will be reported below in CheckOverridesAreAllUsedOnce 
                            ()
                        | _ -> 
                            noimpl()
                        
                    | _ -> 
                        fail(Error("The override for '"^FormatMethInfoSig g amap m denv dispatchSlot^"' was ambiguous",m))
            | _ -> fail(Error("More than one override implements '"^FormatMethInfoSig g amap m denv dispatchSlot^"'",m)));
        !res

    /// 6b. check all implementations implement some virtual method 
    let CheckOverridesAreAllUsedOnce denv g amap (m,reqdTy,dispatchSlotsKeyed,overrides) = 
        // Index the virtuals by name
        
        overrides |> List.iter  (fun (Override(_,_,_,argTys,retTy,_) as overrideBy) -> 
          if not overrideBy.IsFakeEventProperty then
            let m = overrideBy.Range
            let relevantVirts = NameMultiMap.find overrideBy.LogicalName dispatchSlotsKeyed
            // For the purposes of the "how many abstract slots can this override implement" relation,
            // we are only interested in interface slots. That is, a single override is allowed
            // to implement two distinct class-hierarchy abstract slots
            //
            // So here we merge all
            let mergeClassHierarchyAbstractSlots (dispatchSlots:MethInfo list) = 
                dispatchSlots |> ListSet.setify (fun v1 v2 -> not (is_interface_typ g v1.EnclosingType) && 
                                                              not (is_interface_typ g v2.EnclosingType) &&
                                                              MethInfosEquivByNameAndSig EraseNone g amap m v1 v2) 
            
            match relevantVirts  
                     |> List.filter (fun dispatchSlot -> is_exact_match g amap m dispatchSlot overrideBy) 
                     |> mergeClassHierarchyAbstractSlots with
            | [] -> 
                match relevantVirts 
                        |> List.filter (fun dispatchSlot -> is_partial_match g amap m dispatchSlot overrideBy) 
                        |> mergeClassHierarchyAbstractSlots with 
                | [dispatchSlot] -> 
                    errorR(OverrideDoesntOverride(denv,overrideBy,Some(dispatchSlot),g,amap,m))
                | _ -> 
                    match relevantVirts 
                            |> List.filter (fun dispatchSlot -> is_name_match dispatchSlot overrideBy) 
                            |> mergeClassHierarchyAbstractSlots with 
                    | [dispatchSlot] -> 
                        errorR(OverrideDoesntOverride(denv,overrideBy,Some(dispatchSlot),g,amap,m))
                    | _ -> 
                        errorR(OverrideDoesntOverride(denv,overrideBy,None,g,amap,m))
            | [dispatchSlot] -> 
                if dispatchSlot.IsFinal && not (type_equiv g reqdTy dispatchSlot.EnclosingType) then 
                    errorR(Error("The method '"^string_of_minfo amap m denv dispatchSlot^"' is sealed and may not be overridden",m))
            | h1 :: h2 :: _ -> 
                errorR(Error(Printf.sprintf "The override '%s' implements more than one abstract slot, e.g. '%s' and '%s'" (FormatOverride denv overrideBy) (string_of_minfo amap m denv h1) (string_of_minfo amap m denv h2),m)))

    //-------------------------------------------------------------------------

    /// Get the slots of a type that can or must be implemented. This depends
    /// partly on the full set of interface types that are being implemented
    /// simultaneously, e.g.
    ///    { new C with  interface I2 = ... interface I3 = ... }
    /// allReqdTys = {C;I2;I3}
    ///
    /// allReqdTys can include one class/record/union type. 
    let GetSlotImplSets (infoReader:InfoReader) denv isObjExpr allReqdTys = 

        let g = infoReader.g
        let amap = infoReader.amap
        // For each implemented type, get a list containing the transitive closure of
        // interface types implied by the type. This includes the implemented type itself if the implemented type
        // is an interface type.
        let intfSets = 
            allReqdTys |> List.mapi (fun i (reqdTy,m) -> 
                let interfaces = AllSuperTypesOfType g amap m reqdTy |> List.filter (is_interface_typ g)
                let impliedTys = (if is_interface_typ g reqdTy then interfaces else reqdTy :: interfaces)
                (i, reqdTy, impliedTys,m))

        // For each implemented type, reduce its list of implied interfaces by subtracting out those implied 
        // by another implemented interface type.
        //
        // REVIEW: Note complexity O(ity*jty)
        let reqdTyInfos = 
            intfSets |> List.map (fun (i,reqdTy,impliedTys,m) -> 
                let reduced = 
                    (impliedTys,intfSets) ||> List.fold (fun acc (j,jty,impliedTys2,m) -> 
                         if i <> j && type_feasibly_subsumes_type 0 g amap m jty CanCoerce reqdTy 
                         then ListSet.subtract (type_feasibly_equiv 0 g amap m) acc impliedTys2
                         else acc ) 
                (i, reqdTy, m, reduced))

        // Check that, for each implemented type, at least one implemented type is implied. This is enough to capture
        // duplicates.
        for (i, reqdTy, m, impliedTys) in reqdTyInfos do
            if is_interface_typ g reqdTy && isNil impliedTys then 
                errorR(Error("Duplicate or redundant interface",m));

        // Check that no interface type is implied twice
        //
        // REVIEW: Note complexity O(reqdTy*reqdTy)
        for (i, reqdTy, im, impliedTys) in reqdTyInfos do
            for (j,reqdTy2,jm,impliedTys2) in reqdTyInfos do
                if i > j then  
                    let overlap = ListSet.intersect (type_feasibly_equiv 0 g amap im) impliedTys impliedTys2
                    overlap |> List.iter (fun overlappingTy -> 
                        if nonNil(GetImmediateIntrinsicMethInfosOfType (None,AccessibleFromSomewhere) g amap im overlappingTy |> List.filter MethInfo.IsVirtual) then                                
                            errorR(Error("The interface "^NicePrint.pretty_string_of_typ denv (List.hd overlap)^" is included in multiple explicitly implemented interface types. Add an explicit implementation of this interface",im)));

        // Get the SlotImplSet for each implemented type
        // This contains the list of required members and the list of available members
        [ for (_,reqdTy,im,impliedTys) in reqdTyInfos do

            // Build a table of the implied interface types, for quicker lookup
            let isImpliedInterfaceTable = 
                impliedTys 
                |> List.filter (is_interface_typ g) 
                |> List.map (fun ty -> tcref_of_stripped_typ g ty, ()) 
                |> tcref_map_of_list 
            
            // Is a member an abstract slot of one of the implied interface types?
            let isImpliedInterfaceType ty =
                isImpliedInterfaceTable |> tcref_map_mem (tcref_of_stripped_typ g ty) &&
                impliedTys |> List.exists (type_feasibly_equiv 0 g amap im ty)

            let isSlotImpl (minfo:MethInfo) = 
                not minfo.IsAbstract && minfo.IsVirtual 

            // Compute the abstract slots that require implementations
            let dispatchSlots = 
                [ if is_interface_typ g reqdTy then 
                      for impliedTy in impliedTys  do
                          yield! GetImmediateIntrinsicMethInfosOfType (None,AccessibleFromSomewhere) g amap im impliedTy 
                  else
                      
                      // In the normal case, the requirements for a class are precisely all the abstract slots up the whole hierarchy.
                      // So here we get and yield all of those.
                      for minfo in reqdTy |> GetIntrinsicMethInfosOfType infoReader (None,AccessibleFromSomewhere) IgnoreOverrides im do
                         if minfo.IsDispatchSlot then 
                             yield minfo ]
                
                
            // Compute the methods that are available to implement abstract slots from the base class
            //
            // THis is used in CheckDispatchSlotsAreImplemented when we think a dispatch slot may not
            // have been implemented. 
            let availPriorOverridesKeyed : OverrideInfo list = 
                if is_interface_typ g reqdTy then 
                    []
                else 
                    [ // Get any class hierarchy methods on this type 
                      //
                      // NOTE: This is may not 100% correct. What we have below may be an over-approximation that will get too many methods 
                      // and will not correctly relating them to the slots they implement. For example, 
                      // we may get an override from a base class and believe it implements a fresh, new abstract
                      // slot in a subclass. We may have to move to a model where the availPriorOverridesKeyed is computed as a mapping
                      // rather than as a set of "available overrides".
                      for minfos in infoReader.GetRawIntrinsicMethodSetsOfType(None,AccessibleFromSomewhere,im,reqdTy) do
                        for minfo in minfos do
                          if not minfo.IsAbstract then 
                              yield GetInheritedMemberOverrideInfo g amap im CanImplementAnyClassHierarchySlot minfo   ]
                     
            // We also collect up the properties. This is used for abstract slot inference when overriding properties
            let isRelevantRequiredProperty (x:PropInfo) = 
                (x.IsVirtualProperty && not (is_interface_typ g reqdTy)) ||
                isImpliedInterfaceType x.EnclosingType
                
            let reqdProperties = 
                GetIntrinsicPropInfosOfType infoReader (None,AccessibleFromSomewhere) IgnoreOverrides im reqdTy 
                |> List.filter isRelevantRequiredProperty
                
            let dispatchSlotsKeyed = dispatchSlots |> NameMultiMap.initBy (fun v -> v.LogicalName) 
            yield SlotImplSet(dispatchSlots, dispatchSlotsKeyed, availPriorOverridesKeyed, reqdProperties) ]


    let CheckImplementationRelationAtEndOfInferenceScope (infoReader:InfoReader,denv,tycon:Tycon,isImplementation) =

        let g = infoReader.g
        let amap = infoReader.amap
        let m = tycon.Range

        let tcaug = tycon.TypeContents
        let interfaces = tcaug.tcaug_implements
        
        let interfaces = interfaces |> List.map (fun (ity,compgen,m) -> (ity,m))

        let _,overallTy = generalize_tcref (mk_local_tcref tycon)

        let allReqdTys = (overallTy,tycon.Range) :: interfaces 

        // Get all the members that are immediately part of this type
        // Include the auto-generated members
        let allImmediateMembers = 
            (NameMultiMap.range tcaug.tcaug_adhoc @
             (match tcaug.tcaug_compare with None -> [] | Some(a,b) -> [a;b]) @
             (match tcaug.tcaug_compare_withc with None -> [] | Some(a) -> [a]) @
             (match tcaug.tcaug_equals with None -> [] | Some(a,b) -> [a;b]) @
             (match tcaug.tcaug_hash_and_equals_withc with None -> [] | Some(a,b) -> [a;b])) 

        // Get all the members we have to implement, organized by each type we explicitly implement
        let slotImplSets = GetSlotImplSets infoReader denv false allReqdTys

        let allImpls = List.zip allReqdTys slotImplSets


        // Find the methods relevant to implementing the abstract slots listed under the reqdType being checked.
        let allImmediateMembersThatMightImplementDispatchSlots = 
            allImmediateMembers 
            |> List.filter (fun overrideBy -> overrideBy.IsInstanceMember && not (MemberRefIsAbstract overrideBy))

        let mustOverrideSomething reqdTy (overrideBy:ValRef) =
           let memberInfo = overrideBy.MemberInfo.Value
           not (overrideBy.IsFSharpEventProperty(g)) &&
           memberInfo.MemberFlags.MemberIsOverrideOrExplicitImpl && 
    
           match memberInfo.ImplementedSlotSigs with 
           | [] -> 
                // Are we looking at the implementation of the class hierarchy? If so include all the override members
                not (is_interface_typ g reqdTy)
           | ss -> 
                 ss |> List.forall (fun ss -> 
                     let ty = ss.ImplementedType
                     if is_interface_typ g ty then 
                         // Is this a method impl listed under the reqdTy?
                         type_equiv g ty reqdTy
                     else
                         not (is_interface_typ g reqdTy) )
        

        // We check all the abstracts related to the class hierarchy and then check each interface implementation
        for ((reqdTy,m),slotImplSet) in allImpls do
            let (SlotImplSet(dispatchSlots, dispatchSlotsKeyed, availPriorOverridesKeyed,_)) = slotImplSet
            try 

                // Now extract the information about each overriding method relevant to this SLotImplSet
                let allImmediateMembersThatMightImplementDispatchSlots = 
                    allImmediateMembersThatMightImplementDispatchSlots
                    |> List.map (fun overrideBy -> overrideBy,GetTypeMemberOverrideInfo g reqdTy overrideBy)
                
                // Now check the implementation
                // We don't give missing method errors for abstract classes 
                
                if isImplementation && not (is_interface_typ g overallTy) then 
                    let overrides = allImmediateMembersThatMightImplementDispatchSlots |> List.map snd 
                    let allCorrect = CheckDispatchSlotsAreImplemented (denv,g,amap,m,tcaug.tcaug_abstract,reqdTy,dispatchSlots,availPriorOverridesKeyed,overrides)
                    
                    // Tell the user to mark the thing abstract if it was missing implementations
                    if not allCorrect && not tcaug.tcaug_abstract && not (is_interface_typ g reqdTy) then 
                        errorR(TypeIsImplicitlyAbstract(m));
                    
                    
                    let overridesToCheck = 
                        allImmediateMembersThatMightImplementDispatchSlots 
                           |> List.filter (fst >> mustOverrideSomething reqdTy)
                           |> List.map snd
                    CheckOverridesAreAllUsedOnce denv g amap (m,reqdTy,dispatchSlotsKeyed,overridesToCheck);

            with e -> errorRecovery e m; ()

        // Now record the full slotsigs of the abstract members implemented by each override.
        // This is used to generate IL MethodImpls in the code generator.
        allImmediateMembersThatMightImplementDispatchSlots |> List.iter (fun overrideBy -> 

            let isFakeEventProperty = overrideBy.IsFSharpEventProperty(g)
            if not isFakeEventProperty then 
                
                let overriden = 
                    [ for ((reqdTy,m),(SlotImplSet(dispatchSlots,dispatchSlotsKeyed,_,_))) in allImpls do
                          let overrideByInfo = GetTypeMemberOverrideInfo g reqdTy overrideBy
                          let overridenForThisSlotImplSet = 
                              [ for dispatchSlot in NameMultiMap.find overrideByInfo.LogicalName dispatchSlotsKeyed do 
                                    if is_exact_match g amap m dispatchSlot overrideByInfo then 
                                        // Get the slotsig of the overriden method 
                                        let slotsig = SlotSigOfMethodInfo amap m dispatchSlot

                                        // The slotsig from the overriden method is in terms of the type parameters on the parent type of the overriding method,
                                        // Modify map the slotsig so it is in terms of the type parameters for the overriding method 
                                        let slotsig = ReparentSlotSigToUseMethodTypars g amap m overrideBy slotsig
                     
                                        // Record the slotsig via mutation 
                                        yield slotsig ] 
                          //if mustOverrideSomething reqdTy overrideBy then 
                          //    assert nonNil overridenForThisSlotImplSet
                          yield! overridenForThisSlotImplSet ]
                overrideBy.MemberInfo.Value.ImplementedSlotSigs <- overriden);

//-------------------------------------------------------------------------
// Sets of methods involved in overload resolution and trait constraint
// satisfaction.
//------------------------------------------------------------------------- 

/// In the following, 'a gets instantiated to: 
///   1. the expression being supplied for an argument 
///   2. "unit", when simply checking for the existence of an overload that satisfies 
///      a signature, or when finding the corresponding witness. 
/// Note the parametricity helps ensure that overload resolution doesn't depend on the 
/// expression on the callside (though it is in some circumstances allowed 
/// to depend on some type information inferred syntactically from that 
/// expression, e.g. a lambda expression may be converted to a delegate as 
/// an adhoc conversion. 
///
/// The bool indicates if named using a '?' 
type CallerArg<'a> = 
    | CallerArg of Tast.typ * range * bool * 'a  
    member x.Type = (let (CallerArg(ty,_,_,_)) = x in ty)
    
/// CalledArg(pos,isParamArray,optArgInfo,isOutArg,nmOpt,argType)
type CalledArg = 
    | CalledArg of (int * int) * bool (* isParamArray *) * OptionalArgInfo * bool (* isOutArg *) * string option * Tast.typ 
    member x.Type = (let (CalledArg(_,_,_,_,_,ty)) = x in ty)
    member x.Position = (let (CalledArg(i,_,_,_,_,_)) = x in i)

type AssignedCalledArg<'a> = 
    | AssignedCalledArg of ident option * CalledArg * CallerArg<'a>
    member x.CalledArg = (let (AssignedCalledArg(_,calledArg,_)) = x in calledArg)
    member x.Position = x.CalledArg.Position

type AssignedItemSetterTarget = 
    | AssignedPropSetter of PropInfo * MethInfo * Tast.tinst   (* the MethInfo is a non-indexer setter property *)
    | AssignedIlFieldSetter of ILFieldInfo 
    | AssignedRecdFieldSetter of RecdFieldInfo 

type AssignedItemSetter<'a> = AssignedItemSetter of ident * AssignedItemSetterTarget * CallerArg<'a> 

type CallerNamedArg<'a> = 
    | CallerNamedArg of ident * CallerArg<'a>  
    member x.Ident = (let (CallerNamedArg(id,carg)) = x in id)
    member x.Name = x.Ident.idText

type CalledMethArgSet<'a> = 
    | CalledMethArgSet of 
        // The called arguments corresponding to "unnamed" arguments
        CalledArg list * 
        // Any unnamed caller arguments not otherwise assigned 
        CallerArg<'a> list * 
        // The called "ParamArray" argument, if any
        CalledArg option * 
        // Any unnamed caller arguments assigned to a "param array" argument
        CallerArg<'a> list * 
        // named args
        AssignedCalledArg<'a> list 
    member x.UnnamedCalledArgs      = match x with (CalledMethArgSet(unnamedCalledArgs,_,_,_,_)) -> unnamedCalledArgs 
    member x.UnnamedCallerArgs      = match x with (CalledMethArgSet(_,unnamedCallerArgs,_,_,_)) -> unnamedCallerArgs 
    member x.ParamArrayCalledArgOpt = match x with (CalledMethArgSet(_,_,paramArrayCalledArgOpt,_,_)) -> paramArrayCalledArgOpt 
    member x.ParamArrayCallerArgs   = match x with (CalledMethArgSet(_,_,_,paramArrayCallerArgs,_)) -> paramArrayCallerArgs 
    member x.AssignedNamedArgs           = match x with (CalledMethArgSet(_,_,_,_,namedArgs)) -> namedArgs
    member x.NumUnnamedCallerArgs = x.UnnamedCallerArgs.Length
    member x.NumAssignedNamedArgs = x.AssignedNamedArgs.Length
    member x.NumUnnamedCalledArgs = x.UnnamedCalledArgs.Length


// CLEANUP: make this a record or class
type CalledMeth<'a> = 
    | CalledMeth of 
        // the method we're attempting to call 
        MethInfo * 
        // the instantiation of the method we're attempting to call 
        Tast.tinst * 
        // the formal instantiation of the method we're attempting to call 
        Tast.tinst * 
        // The types of the actual object arguments, if any
        Tast.typ list * 

        // The argument analysis for each set of curried arguments
        CalledMethArgSet<'a> list *

        // return type
        Tast.typ * 
        // named property setters
        AssignedItemSetter<'a> list * 
        // the property related to the method we're attempting to call, if any  
        PropInfo option * 
        // unassigned args
        CallerNamedArg<'a> list * 
        // args assigned to specifiy values for attribute fields and properties (these are not necessarily "property sets")
        CallerNamedArg<'a> list  * 
        // unnamed called optional args: pass defaults for these
        CalledArg list *
        // unnamed called out args: return these as part of the return tuple
        CalledArg list

    member x.Method                 = match x with (CalledMeth(minfo,_,_,_,_,_,_,_,_,_,_,_)) -> minfo
    static member GetMethod (x:CalledMeth<'a>) = x.Method

    member x.CalledTyArgs           = match x with (CalledMeth(_,minst,_,_,_,_,_,_,_,_,_,_)) -> minst
    member x.CallerTyArgs           = match x with (CalledMeth(_,_,userTypeArgs,_,_,_,_,_,_,_,_,_)) -> userTypeArgs
    member x.CallerObjArgTys        = match x with (CalledMeth(_,_,_,callerObjArgTys,_,_,_,_,_,_,_,_)) -> callerObjArgTys
    member x.ArgSets                = match x with (CalledMeth(_,_,_,_,argSets,_,_,_,_,_,_,_)) -> argSets
    member x.NumArgSets             = x.ArgSets.Length

    member x.AssignedProps          = match x with (CalledMeth(_,_,_,_,_,_,namedProps,_,_,_,_,_)) -> namedProps
    member x.AssociatedPropertyInfo = match x with (CalledMeth(_,_,_,_,_,_,_,x,_,_,_,_)) -> x
    member x.UnassignedNamedArgs    = match x with (CalledMeth(_,_,_,_,_,_,_,_,unassignedNamedItems,_,_,_)) -> unassignedNamedItems
    member x.AttributeAssignedNamedArgs = match x with (CalledMeth(_,_,_,_,_,_,_,_,_,x,_,_)) -> x
    member x.HasOptArgs             = match x with (CalledMeth(_,_,_,_,_,_,_,_,_,_,unnamedCalledOptArgs,_)) -> nonNil unnamedCalledOptArgs
    member x.HasOutArgs             = match x with (CalledMeth(_,_,_,_,_,_,_,_,_,_,_,unnamedCalledOutArgs)) -> nonNil unnamedCalledOutArgs
    member x.UsesParamArrayConversion = 
        x.ArgSets |> List.exists (fun argSet -> argSet.ParamArrayCalledArgOpt.IsSome)
    member x.ParamArrayCalledArgOpt = 
        x.ArgSets |> List.tryPick (fun argSet -> argSet.ParamArrayCalledArgOpt)
    member x.ParamArrayCallerArgs = 
        x.ArgSets |> List.tryPick (fun argSet -> if isSome argSet.ParamArrayCalledArgOpt then Some argSet.ParamArrayCallerArgs else None )
    member x.ParamArrayElementType(g) = 
        assert (x.UsesParamArrayConversion)
        x.ParamArrayCalledArgOpt.Value.Type |> dest_il_arr1_typ g 
    member x.NumAssignedProps = x.AssignedProps.Length
    member x.CalledObjArgTys(amap,m) = ObjTypesOfMethInfo amap m x.Method x.CalledTyArgs
    member x.NumCalledTyArgs = x.CalledTyArgs.Length
    member x.NumCallerTyArgs = x.CallerTyArgs.Length 

    member x.AssignsAllNamedArgs = isNil x.UnassignedNamedArgs

    member x.HasCorrectArity =
      (x.NumCalledTyArgs = x.NumCallerTyArgs)  &&
      x.ArgSets |> List.forall (fun argSet -> argSet.NumUnnamedCalledArgs = argSet.NumUnnamedCallerArgs) 

    member x.HasCorrectGenericArity =
      (x.NumCalledTyArgs = x.NumCallerTyArgs)  

    member x.IsAccessible(amap,m,ad) = 
        IsMethInfoAccessible amap m ad x.Method 

    member x.HasCorrectObjArgs(amap,m,ad) = 
        x.CalledObjArgTys(amap,m).Length = x.CallerObjArgTys.Length 

    member x.IsCandidate(g,amap,m,ad) =
        x.IsAccessible(amap,m,ad) &&
        x.HasCorrectArity && 
        x.HasCorrectObjArgs(amap,m,ad) &&
        x.AssignsAllNamedArgs
    member x.AllUnnamedCalledArgs = x.ArgSets |> List.collect (fun x -> x.UnnamedCalledArgs)
    member x.TotalNumUnnamedCalledArgs = x.ArgSets |> List.sumBy (fun x -> x.NumUnnamedCalledArgs)
    member x.TotalNumUnnamedCallerArgs = x.ArgSets |> List.sumBy (fun x -> x.NumUnnamedCallerArgs)
    member x.TotalNumAssignedNamedArgs = x.ArgSets |> List.sumBy (fun x -> x.NumAssignedNamedArgs)

let MakeCalledArgs amap m minfo minst =
    // Mark up the arguments with their position, so we can sort them back into order later 
    let paramDatas = ParamDatasOfMethInfo amap m minfo minst
    paramDatas |> List.mapiSquared (fun i j (ParamData(isParamArrayArg,isOutArg,optArgInfo,nmOpt,typeOfCalledArg))  -> 
        let isOptArg = optArgInfo <> NotOptional
        CalledArg((i,j),isParamArrayArg,optArgInfo,isOutArg,nmOpt,typeOfCalledArg))  

let MakeCalledMeth 
      (infoReader:InfoReader,
       checkingAttributeCall, 
       freshenMethInfo,// a function to help generate fresh type variables the property setters methods in generic classes 
       m, 
       ad,                // the access domain of the place where the call is taking place
       minfo,             // the method we're attempting to call 
       minst,             // the instantiation of the method we're attempting to call 
       uminst,            // the formal instantiation of the method we're attempting to call 
       pinfoOpt,          // the property related to the method we're attempting to call, if any  
       objArgs,           // the types of the actual object argument, if any 
       callerArgs: (CallerArg<_> list * CallerNamedArg<_> list) list,     // the data about any arguments supplied by the caller 
       allowParamArgs:bool)   // do we allow the use of a param args method in its "expanded" form?
    =
    let g = infoReader.g
    let amap = infoReader.amap
    let methodRetTy = FSharpReturnTyOfMeth amap m minfo minst

    if verbose then dprintf "--> methodRetTy = %s\n" (Layout.showL (typeL methodRetTy));
    if verbose then dprintf "--> minfo.Type = %s\n" (Layout.showL (typeL minfo.EnclosingType));

    let fullCalledArgs = MakeCalledArgs amap m minfo minst
    assert (callerArgs.Length = fullCalledArgs.Length)
 
    let argSetInfos = 
        (callerArgs, fullCalledArgs) ||> List.map2 (fun (unnamedCallerArgs,namedCallerArgs) fullCalledArgs -> 
            // Find the arguments not given by name 
            let unnamedCalledArgs = 
                fullCalledArgs |> List.filter (function 
                    | (CalledArg(_,_,_,_,Some nm,_)) -> 
                        namedCallerArgs |> List.forall (fun (CallerNamedArg(nm2,e)) -> nm <> nm2.idText)   
                    | _ -> true)

            // See if any of them are 'out' arguments being returned as part of a return tuple 
            let unnamedCalledArgs, unnamedCalledOptArgs, unnamedCalledOutArgs = 
                let nUnnamedCallerArgs = unnamedCallerArgs.Length
                if nUnnamedCallerArgs < unnamedCalledArgs.Length then
                    let unnamedCalledArgsTrimmed,unnamedCalledOptOrOutArgs = List.chop nUnnamedCallerArgs unnamedCalledArgs
                    
                    // Check if all optional/out arguments are byref-out args
                    if unnamedCalledOptOrOutArgs |> List.forall (fun (CalledArg(i,_,_,isOutArg,_,typeOfCalledArg)) -> isOutArg && is_byref_typ g typeOfCalledArg) then 
                        let unnamedCalledOutArgs = unnamedCalledOptOrOutArgs |> List.map (fun (CalledArg(i,isParamArrayArg,optArgInfo,isOutArg,nmOpt,typeOfCalledArg)) -> (CalledArg(i,isParamArrayArg,optArgInfo,isOutArg,nmOpt,typeOfCalledArg)))
                        unnamedCalledArgsTrimmed,[],unnamedCalledOutArgs
                    // Check if all optional/out arguments are optional args
                    elif unnamedCalledOptOrOutArgs |> List.forall (fun (CalledArg(i,_,optArgInfo,isOutArg,_,typeOfCalledArg)) -> optArgInfo <> NotOptional) then 
                        let unnamedCalledOptArgs = unnamedCalledOptOrOutArgs
                        unnamedCalledArgsTrimmed,unnamedCalledOptArgs,[]
                    // Otherwise drop them on the floor
                    else
                        unnamedCalledArgs,[],[]
                else 
                    unnamedCalledArgs,[],[]

            let (unnamedCallerArgs,paramArrayCallerArgs),unnamedCalledArgs,paramArrayCalledArgOpt = 
                let minArgs = unnamedCalledArgs.Length - 1
                let supportsParamArgs = 
                    allowParamArgs && 
                    minArgs >= 0 && 
                    unnamedCalledArgs |> List.last |> (fun (CalledArg(_,isParamArray,_,_,_,ty)) -> isParamArray && is_il_arr1_typ g ty)

                if supportsParamArgs  && unnamedCallerArgs.Length >= minArgs then
                    let a,b = List.frontAndBack unnamedCalledArgs
                    List.chop minArgs unnamedCallerArgs, a, Some(b)
                else
                    (unnamedCallerArgs, []),unnamedCalledArgs, None
            //dprintfn "Calling %s: paramArrayCallerArgs = %d, paramArrayCalledArgOpt = %d" minfo.LogicalName paramArrayCallerArgs.Length (Option.length paramArrayCalledArgOpt)

            let assignedNamedArgs = fullCalledArgs |> List.choose (function CalledArg(_,_,_,_,Some nm,_) as arg -> List.tryPick (fun (CallerNamedArg(nm2,arg2)) -> if nm = nm2.idText then Some (AssignedCalledArg(Some(nm2),arg,arg2)) else None)  namedCallerArgs | _ -> None)
            let unassignedNamedItem = namedCallerArgs |> List.filter (fun (CallerNamedArg(nm,e)) -> List.forall (function CalledArg(_,_,_,_,Some nm2,_) -> nm.idText <> nm2 | _ -> true) fullCalledArgs)

            let attributeAssignedNamedItems,unassignedNamedItem = 
                if checkingAttributeCall then 
                    // the assignment of names to properties is substantially for attribute specifications 
                    // permits bindings of names to non-mutable fields and properties, so we do that using the old 
                    // reliable code for this later on. 
                    unassignedNamedItem,[]
                 else 
                    [],unassignedNamedItem

            let assignedNamedProps,unassignedNamedItem = 
                let returnedObjTy = if minfo.IsConstructor then minfo.EnclosingType else methodRetTy
                unassignedNamedItem |> List.splitChoose (fun (CallerNamedArg(id,e) as arg) -> 
                    let nm = id.idText
                    let pinfos = GetIntrinsicPropInfoSetsOfType infoReader (Some(nm),ad) IgnoreOverrides id.idRange returnedObjTy
                    let pinfos = pinfos |> ExcludeHiddenOfPropInfos g amap m 
                    match pinfos with 
                    | [pinfo] when pinfo.HasSetter && not pinfo.IsIndexer -> 
                        let pminfo = pinfo.SetterMethod
                        let pminst = freshenMethInfo m pminfo
                        Choice1Of2(AssignedItemSetter(id,AssignedPropSetter(pinfo,pminfo, pminst), e))
                    | _ ->
                        match infoReader.GetILFieldInfosOfType(Some(nm),ad,m,returnedObjTy) with
                        | finfo :: _ -> 
                            Choice1Of2(AssignedItemSetter(id,AssignedIlFieldSetter(finfo), e))
                        | _ ->              
                          match infoReader.TryFindRecdFieldInfoOfType(nm,m,returnedObjTy) with
                          | Some rfinfo -> 
                              Choice1Of2(AssignedItemSetter(id,AssignedRecdFieldSetter(rfinfo), e))
                          | None -> 
                              Choice2Of2(arg))

            let names = namedCallerArgs |> List.map (function CallerNamedArg(nm,_) -> nm.idText) 

            if (List.noRepeats String.order names).Length <> namedCallerArgs.Length then
                errorR(Error("a named argument has been assigned more than one value",m));
                
            if verbose then dprintf "#fullCalledArgs = %d, #unnamedCalledArgs = %d, #assignedNamedArgs = %d, #residueNamedArgs = %d, #attributeAssignedNamedItems = %d\n"
                                        fullCalledArgs.Length unnamedCalledArgs.Length assignedNamedArgs.Length unassignedNamedItem.Length attributeAssignedNamedItems.Length;
            let argSet = CalledMethArgSet(unnamedCalledArgs,unnamedCallerArgs,paramArrayCalledArgOpt,paramArrayCallerArgs,assignedNamedArgs)
            (argSet,assignedNamedProps,unassignedNamedItem,attributeAssignedNamedItems,unnamedCalledOptArgs,unnamedCalledOutArgs))

    let argSets                     = argSetInfos |> List.map     (fun (x,_,_,_,_,_) -> x)
    let assignedNamedProps          = argSetInfos |> List.collect (fun (_,x,_,_,_,_) -> x)
    let unassignedNamedItems        = argSetInfos |> List.collect (fun (_,_,x,_,_,_) -> x)
    let attributeAssignedNamedItems = argSetInfos |> List.collect (fun (_,_,_,x,_,_) -> x)
    let unnamedCalledOptArgs        = argSetInfos |> List.collect (fun (_,_,_,_,x,_) -> x)
    let unnamedCalledOutArgs        = argSetInfos |> List.collect (fun (_,_,_,_,_,x) -> x)
    CalledMeth(minfo,minst,uminst,objArgs,argSets,methodRetTy,assignedNamedProps,pinfoOpt,unassignedNamedItems,attributeAssignedNamedItems,unnamedCalledOptArgs,unnamedCalledOutArgs)
    
let NamesOfCalledArgs calledArgs = 
    calledArgs |> List.choose (fun (CalledArg(_,_,_,_,nmOpt,_)) -> nmOpt) 


let showAccessDomain ad =
    match ad with 
    | AccessibleFromEverywhere -> "public" 
    | AccessibleFrom(_,_) -> "accessible"
    | AccessibleFromSomeFSharpCode -> "public, protected or internal" 
    | AccessibleFromSomewhere -> ""




/// "Type Completion" inference and a few other checks at the end of the
/// inference scope
let FinalTypeDefinitionChecksAtEndOfInferenceScope (infoReader:InfoReader) isImplementation denv (tycon:Tycon) =

    let g = infoReader.g
    let amap = infoReader.amap
    let m = tycon.Range

    let tcaug = tycon.TypeContents
    tcaug.tcaug_closed <- true
  
    // Note you only have to explicitly implement 'System.IComparable' to customize structural comparison AND equality on F# types 
    if isImplementation &&
       isNone tcaug.tcaug_compare &&
        tcaug_has_interface g tcaug g.mk_IComparable_ty && 
        not (tcaug_has_override g tcaug "Equals" [g.obj_ty]) && 
        not tycon.IsFSharpInterfaceTycon
     then
        (* Warn when we're doing this for class types *)
        if Augment.TyconIsAugmentedWithEquals g tycon then
            warning(Error("The type '"^tycon.DisplayName^"' implements 'System.IComparable'. Consider also adding an explicit override for 'Object.Equals'",tycon.Range))
        else
            warning(Error("The type '"^tycon.DisplayName^"' implements 'System.IComparable' explicitly but provides no corresponding override for 'Object.Equals'. An implementation of 'Object.Equals' has been automatically provided, implemented via 'System.IComparable'. Consider implementing the override 'Object.Equals' explicitly",tycon.Range))

    // Check some conditions about generic comparison and hashing. We can only check this condition after we've done the augmentation 
    if isImplementation then
        Augment.CheckAugmentationAttribs g tycon;

        let tcaug = tycon.TypeContents
        let m = tycon.Range
        let hasExplicitObjectGetHashCode = tcaug_has_override g tcaug "GetHashCode" []
        let hasExplicitObjectEqualsOverride = tcaug_has_override g tcaug "Equals" [g.obj_ty]

        if (isSome tcaug.tcaug_hash_and_equals_withc || isSome tcaug.tcaug_compare || isSome tcaug.tcaug_compare_withc) && 
           (hasExplicitObjectGetHashCode || hasExplicitObjectEqualsOverride) then 
            errorR(Error("The struct, record or union type '"^tycon.DisplayName^"' has an explicit implementation of 'Object.GetHashCode' or 'Object.Equals'. You must apply the '[<StructuralEquality(false)>]' attribute to the type",m)); 

        if not hasExplicitObjectEqualsOverride && hasExplicitObjectGetHashCode then 
            warning(Error("The struct, record or union type '"^tycon.DisplayName^"' has an explicit implementation of 'Object.GetHashCode'. Consider implementing a matching override for 'Object.Equals(obj)'",m)); 

        if hasExplicitObjectEqualsOverride && not hasExplicitObjectGetHashCode then 
            warning(Error("The struct, record or union type '"^tycon.DisplayName^"' has an explicit implementation of 'Object.Equals'. Consider implementing a matching override for 'Object.GetHashCode()'",m)); 


        // remember these values to ensure we don't generate these methods during codegen 
        set_tcaug_hasObjectGetHashCode tcaug hasExplicitObjectGetHashCode;

        if not tycon.IsHiddenReprTycon
           && not tycon.IsTypeAbbrev
           && not tycon.IsMeasureableReprTycon
           && not tycon.IsAsmReprTycon
           && not tycon.IsFSharpInterfaceTycon
           && not tycon.IsFSharpDelegateTycon then 

            DispatchSlotChecking.CheckImplementationRelationAtEndOfInferenceScope (infoReader,denv,tycon,isImplementation) 
    
/// "Single Feasible Type" inference
/// Look for the unique supertype of ty2 for which ty2 :> ty1 might feasibly hold
/// REVIEW: eliminate this use of type_feasibly_subsumes_type
/// We should be able to look for identical head types.
let FindUniqueFeasibleSupertype g amap m ty1 ty2 =  
    if not (is_stripped_tyapp_typ g ty2) then None else
    let tcref,tinst = dest_stripped_tyapp_typ g ty2
    let supertypes = Option.to_list (SuperTypeOfType g amap m ty2) @ (ImplementsOfType g amap m ty2)
    supertypes |> List.tryfind (type_feasibly_subsumes_type 0 g amap m ty1 NoCoerce) 
    


/// Get the methods relevant to deterimining if a uniquely-identified-override exists based on the syntactic information 
/// at the member signature prior to type inference. This is used to pre-assign type information if it does 
let GetAbstractMethInfosForSynMethodDecl (infoReader:InfoReader,ad,memberName:ident,bindm,typToSearchForAbstractMembers,valSynData) =
    let g = infoReader.g
    let amap = infoReader.amap
    let minfos = 
        match typToSearchForAbstractMembers with 
        | _,Some(SlotImplSet(_, dispatchSlotsKeyed,_,_)) -> 
            NameMultiMap.find  memberName.idText dispatchSlotsKeyed
        | ty, None -> 
            GetIntrinsicMethInfosOfType infoReader (Some(memberName.idText),ad) IgnoreOverrides bindm ty
    let dispatchSlots = minfos |> List.filter (fun minfo -> minfo.IsDispatchSlot)
    let topValSynArities = SynInfo.AritiesOfArgs valSynData
    let topValSynArities = if topValSynArities.Length > 0 then topValSynArities.Tail else topValSynArities
    let dispatchSlotsArityMatch = dispatchSlots |> List.filter (fun minfo -> minfo.NumArgs = topValSynArities) 
    dispatchSlots,dispatchSlotsArityMatch 

/// Get the proeprties relevant to deterimining if a uniquely-identified-override exists based on the syntactic information 
/// at the member signature prior to type inference. This is used to pre-assign type information if it does 
let GetAbstractPropInfosForSynPropertyDecl (infoReader:InfoReader,ad,memberName:ident,bindm,typToSearchForAbstractMembers,k,valSynData) = 
    let pinfos = 
        match typToSearchForAbstractMembers with 
        | _,Some(SlotImplSet(_,_,_,reqdProps)) -> 
            reqdProps |> List.filter (fun pinfo -> pinfo.PropertyName = memberName.idText) 
        | ty, None -> 
            GetIntrinsicPropInfosOfType infoReader (Some(memberName.idText),ad) IgnoreOverrides bindm ty
        
    let dispatchSlots = pinfos |> List.filter (fun pinfo -> pinfo.IsVirtualProperty)
    dispatchSlots

let HaveSameHeadType g ty1 ty2 = 
    is_stripped_tyapp_typ g ty1 && is_stripped_tyapp_typ g ty2 &&
    tcref_eq g (tcref_of_stripped_typ g ty1) (tcref_of_stripped_typ g ty2)

let ExistsSameHeadTypeInHierarchy g amap m typeToSearchFrom typeWithToLookFor = 
     ExistsInEntireHierarchyOfType (HaveSameHeadType g typeWithToLookFor)  g amap m typeToSearchFrom
  
