// (c) Microsoft Corporation. All rights reserved

#light

module (* internal *) Microsoft.FSharp.Compiler.ConstraintSolver

//-------------------------------------------------------------------------
// Incremental type inference constraint solving.  
//
// Primary constraints are:
//   - type equations        ty1 = ty2
//   - subtype inequations   ty1 :> ty2
//   - trait constraints     tyname : (static member op_Addition : 'a * 'b -> 'c)
//
// Plus some other constraints inherited from .NET generics.
// 
// The constraints are immediately processed into a normal form, in particular
//   - type equations on inference parameters:   'tp = ty
//   - type inequations on inference parameters: 'tp :> ty
//   - other constraints on inference paramaters
//
// The state of the inference engine is kept in imperative mutations to inference
// type variables.
//
// The use of the normal form allows the state of the inference engine to 
// be queried for type-directed name resolution, type-directed overload 
// resolution and when generating warning messages.
//
// The inference engine can be used in 'undo' mode to implement
// can-unify predicates used in method overload resolution and trait constraint
// satisfaction.
//
//------------------------------------------------------------------------- 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Outcome
open Microsoft.FSharp.Compiler.Infos
open Microsoft.FSharp.Compiler.Infos.AccessibilityLogic
open Microsoft.FSharp.Compiler.Infos.AttributeChecking
open Microsoft.FSharp.Compiler.Typrelns
open Microsoft.FSharp.Compiler.PrettyNaming

(*-------------------------------------------------------------------------
!* Generate type variables and record them in within the scope of the
 * compilation environment, which currently corresponds to the scope
 * of the constraint resolution carried out by type checking.
 *------------------------------------------------------------------------- *)

   
let new_tv_uniq = let i = ref 0 in fun () -> incr i; !i

let compgen_id = text_to_id0 unassignedTyparName

let new_compgen_inference_var (kind,rigid,staticReq,dynamicReq,error) = 
    NewTypar(kind,rigid,Typar(compgen_id,staticReq,true),error,dynamicReq,[]) 
    
let anon_id m = mksyn_id m unassignedTyparName
let new_anon_inference_var (kind,m,rigid,var,dyn) = 
    NewTypar (kind,rigid,Typar(anon_id m,var,true),false,dyn,[])
    
let new_named_inference_measurevar (m,rigid,var,id) = 
    NewTypar(KindMeasure,rigid,Typar(id,var,false),false,NoDynamicReq,[]) 

let new_inference_measurevar () = new_compgen_inference_var (KindMeasure,TyparFlexible,NoStaticReq,NoDynamicReq,false)


let new_error_tyvar () = new_compgen_inference_var (KindType,TyparFlexible,NoStaticReq,NoDynamicReq,true)
let new_error_measurevar () = new_compgen_inference_var (KindMeasure,TyparFlexible,NoStaticReq,NoDynamicReq,true)
let new_inference_typ () = mk_typar_ty (NewTypar (KindType,TyparFlexible,Typar(compgen_id,NoStaticReq,true),false,NoDynamicReq,[]))
let new_error_typ () = mk_typar_ty (new_error_tyvar ())
let new_error_measure () = MeasureVar (new_error_measurevar ())

let new_inference_typs l = l |> List.map (fun _ -> new_inference_typ ()) 

// QUERY: should 'rigid' ever really be 'true'? We set this when we know 
// we are going to have to generalize a typar, e.g. when implementing a 
// abstract generic method slot. But we later check the generalization 
// condition anyway, so we could get away with a non-rigid typar. This 
// would sort of be cleaner, though give errors later. 
let freshen_and_fixup_typars m rigid fctps tinst tpsorig = 
    let copy_tyvar (tp:Typar) =  new_compgen_inference_var (tp.Kind,rigid,tp.StaticReq,(if rigid=TyparRigid then DynamicReq else NoDynamicReq),false)
    let tps = tpsorig |> List.map copy_tyvar 
    let renaming,tinst = FixupNewTypars m fctps tinst tpsorig tps
    tps,renaming,tinst

let new_tinst m tpsorig = freshen_and_fixup_typars m TyparFlexible [] [] tpsorig 
let new_minst m fctps tinst tpsorig = freshen_and_fixup_typars m TyparFlexible fctps tinst tpsorig 

let freshen_tps m tpsorig = 
    let _,renaming,tptys = new_tinst m tpsorig
    tptys

let FreshenMethInfo m (minfo:MethInfo) =
    let _,renaming,tptys = new_minst m (FormalTyparsOfEnclosingTypeOfMethInfo m minfo) minfo.ActualTypeInst minfo.FormalMethodTypars
    tptys


(*-------------------------------------------------------------------------
!* Unification of types: solve/record equality constraints
 * Subsumption of types: solve/record subtyping constraints
 *------------------------------------------------------------------------- *)

exception ConstraintSolverTupleDiffLengths of DisplayEnv * Tast.typ list * Tast.typ list * range  * range 
exception ConstraintSolverInfiniteTypes of DisplayEnv * Tast.typ * Tast.typ * range * range
exception ConstraintSolverTypesNotInEqualityRelation of DisplayEnv * Tast.typ * Tast.typ * range  * range 
exception ConstraintSolverTypesNotInSubsumptionRelation of DisplayEnv * Tast.typ * Tast.typ * range  * range 
exception ConstraintSolverMissingConstraint of DisplayEnv * Tast.Typar * Tast.TyparConstraint * range  * range 
exception ConstraintSolverError of string * range * range
exception ConstraintSolverRelatedInformation of string option * range * exn 

exception ErrorFromApplyingDefault of Env.TcGlobals * DisplayEnv * Tast.Typar * Tast.typ * error * range
exception ErrorFromAddingTypeEquation of Env.TcGlobals * DisplayEnv * Tast.typ * Tast.typ * error * range
exception ErrorsFromAddingSubsumptionConstraint of Env.TcGlobals * DisplayEnv * Tast.typ * Tast.typ * error * range
exception ErrorFromAddingConstraint of  DisplayEnv * error * range

exception UnresolvedOverloading of DisplayEnv * error list * error list * error list * string * range
exception PossibleOverload of DisplayEnv * string * range
//exception PossibleBestOverload of DisplayEnv * string * range

let GetPossibleOverloads  amap m denv (calledMethGroup:CalledMeth<_> list) = 
    calledMethGroup |> List.map (fun cmeth -> PossibleOverload(denv,string_of_minfo amap m denv cmeth.Method,m))

(*
let GetPossibleBestOverloads  amap m denv (calledMethGroup:CalledMeth<_> list) = 
    calledMethGroup |> List.map (fun cmeth -> PossibleBestOverload(denv,string_of_minfo amap m denv cmeth.Method,m))
*)

type ConstraintSolverState = 
    { 
      css_g: Env.TcGlobals;
      css_amap: Import.ImportMap; 
      css_InfoReader : InfoReader;
      /// This table stores all trait constraints, indexed by free type variable.
      /// That is, there will be one entry in this table for each free type variable in 
      /// each outstanding trait constraint. Constraints are removed from the table and resolved 
      /// each time a solution to an index variable is found. 
      mutable css_cxs:  Hashtbl.t<stamp, (Tast.TraitConstraintInfo * range)>;
    }


type ConstraintSolverEnv = 
    { 
      cs_css: ConstraintSolverState;
      cs_m: range;
      cs_aenv: TypeEquivEnv;
      cs_denv : DisplayEnv
    }
    member c.InfoReader = c.cs_css.css_InfoReader
    member c.g = c.cs_css.css_g
    member c.amap = c.cs_css.css_amap
    
let MakeConstraintSolverEnv css m denv = 
    { cs_css=css;
      cs_m=m;
      cs_aenv=tyeq_env_empty; 
      cs_denv = denv }


(*-------------------------------------------------------------------------
!* Occurs check
 *------------------------------------------------------------------------- *)

/// Check whether a type variable OccursCheck in the r.h.s. of a type, e.g. to catch
/// infinite equations such as 
///    'a = list<'a>
let rec OccursCheck g un ty = 
    match strip_tpeqns_and_tcabbrevs g ty with 

    | TType_ucase(_,l)
    | TType_app (_,l) 
    | TType_tuple l -> List.exists (OccursCheck g un) l

    | TType_fun (d,r) -> OccursCheck g un d || OccursCheck g un r

    | TType_var r   ->  typar_ref_eq un r 

    | TType_forall (tp,tau) -> OccursCheck g un tau
    | _ -> false 


(*-------------------------------------------------------------------------
!* Predicates on types
 *------------------------------------------------------------------------- *)

let IsSignedIntegralType  g ty =
    type_equiv_aux EraseMeasures g g.sbyte_ty ty || 
    type_equiv_aux EraseMeasures g g.int16_ty ty || 
    type_equiv_aux EraseMeasures g g.int32_ty ty || 
    type_equiv_aux EraseMeasures g g.nativeint_ty ty || 
    type_equiv_aux EraseMeasures g g.int64_ty ty 

let IsUnsignedIntegralType  g ty =
    type_equiv_aux EraseMeasures g g.byte_ty ty || 
    type_equiv_aux EraseMeasures g g.uint16_ty ty || 
    type_equiv_aux EraseMeasures g g.uint32_ty ty || 
    type_equiv_aux EraseMeasures g g.unativeint_ty ty || 
    type_equiv_aux EraseMeasures g g.uint64_ty ty 

let rec IsIntegralOrIntegralEnumType g ty =
    IsSignedIntegralType g ty || 
    IsUnsignedIntegralType g ty || 
    (is_enum_typ g ty && IsIntegralOrIntegralEnumType g (GetUnderlyingTypeOfEnumType g ty))
    
let rec IsIntegralType g ty =
    IsSignedIntegralType g ty || 
    IsUnsignedIntegralType g ty 
    
let IsStringType g ty =
    type_equiv g g.string_ty ty 
let IsCharType g ty =
    type_equiv g g.char_ty ty 

/// float or float32 or float<_> or float32<_> 
let IsFpType g ty =
    type_equiv_aux EraseMeasures g g.float_ty ty || 
    type_equiv_aux EraseMeasures g g.float32_ty ty 

/// decimal or decimal<_>
let IsDecimalType g ty = 
    type_equiv_aux EraseMeasures g g.decimal_ty ty 

let IsNonDecimalNumericOrIntegralEnumType g ty = IsIntegralOrIntegralEnumType g ty || IsFpType g ty
let IsNumericOrIntegralEnumType g ty = IsNonDecimalNumericOrIntegralEnumType g ty || IsDecimalType g ty
let IsNonDecimalNumericType g ty = IsIntegralType g ty || IsFpType g ty
let IsNumericType g ty = IsNonDecimalNumericType g ty || IsDecimalType g ty

// Get measure of type, float<_> or float32<_> or decimal<_> but not float=float<1> or float32=float32<1> or decimal=decimal<1> 
let GetMeasureOfType g ty =
    if is_stripped_tyapp_typ g ty then
      let tcref,tinst = dest_stripped_tyapp_typ g ty
      match tinst with
      | [tyarg] ->  
        match strip_tpeqns_and_tcabbrevs g tyarg with  
        | TType_measure ms -> 
          if measure_equiv g ms MeasureOne then None else Some (tcref,ms)
        | _ -> None
      | _ -> None
    else None

let IsArrayTypeWithIndexer g ty = is_any_array_typ g ty

let IsArrayTypeWithSlice g n ty = 
    is_any_array_typ g ty && (rank_of_any_array_typ g ty = n)

let IsArrayKindMismatch g tc1 l1 tc2 l2 = 
    tcref_eq g tc1 g.il_arr1_tcr &&
    tcref_eq g g.array_tcr tc2 && 
    List.length l1 = 1 && 
    List.length l2 = 1 && 
    type_equiv g (List.hd l1) (List.hd l2)  

type TraitConstraintSolution = 
    | TTraitUnsolved
    | TTraitBuiltIn
    | TTraitSolved of MethInfo * tinst

let BakedInTraitConstraintNames = 
    [ "op_Division" ; "op_Multiply"; "op_Addition" 
      "op_Subtraction"; "op_Modulus"; 
      "get_Zero"; "get_One";
      "DivideByInt";"get_Item"; "set_Item";
      "op_BitwiseAnd"; "op_BitwiseOr"; "op_ExclusiveOr"; "op_LeftShift";
      "op_RightShift"; "op_UnaryPlus"; "op_UnaryNegation"; "get_Sign"; "op_LogicalNot"
      "op_OnesComplement"; "Abs"; "Sqrt"; "Sin"; "Cos"; "Tan";
      "Sinh";  "Cosh"; "Tanh"; "Atan"; "Acos"; "Asin"; "Exp"; "Ceiling"; "Floor"; "Round"; "Log10"; "Log"; "Sqrt";
      "Truncate"; "ToChar"; "ToByte"; "ToSByte"; "ToInt16"; "ToUInt16"; "ToInt32"; "ToUInt32"; "ToInt64"; "ToUInt64"; "ToSingle"; "ToDouble";
      "ToDecimal"; "ToUIntPtr"; "ToIntPtr"; "Pow"; "Atan2" ]
    
//-------------------------------------------------------------------------
// Run the constraint solver with undo (used during method overload resolution)

type trace = Trace of (unit -> unit) list ref

type OptionalTrace = 
    | NoTrace
    | WithTrace of trace
    
let newTrace () =  Trace (ref [])
let undoTrace (Trace trace) =   List.iter (fun a -> a ()) !trace
let saveOnPreviousTrace (Trace trace1) (Trace trace2) = trace1 := !trace1 @ !trace2

let isNoTrace = function NoTrace -> true | WithTrace _ -> false


let CollectThenUndo f = 
    let trace = newTrace()
    let res = f trace
    undoTrace trace; 
    res

let CheckThenUndo f = CollectThenUndo f |> CheckNoErrorsAndGetWarnings 

let FilterEachThenUndo f meths = 
    meths |> List.choose (fun calledMeth -> 
        match CheckThenUndo (fun trace -> f trace calledMeth) with 
        | None -> None 
        | Some warns -> Some (calledMeth,warns.Length))


//-------------------------------------------------------------------------
// Solve

exception NonRigidTypar of DisplayEnv * string option * range * Tast.typ * Tast.typ * range
exception LocallyAbortOperationThatLosesAbbrevs 
let localAbortD = ErrorD LocallyAbortOperationThatLosesAbbrevs

/// Ensure that vs is ordered so that an element with minimum sized exponent
/// is at the head of the list. Also, if possible, this element should have rigidity TyparFlexible 
let FindMinimumMeasureExponent vs =
    let rec findmin vs = 
        match vs with
        | [] -> vs
        | (v:Typar,e)::vs ->
           match findmin vs with
            | [] -> [(v,e)]
            | (v',e')::vs' ->
                if abs e < abs e' || (abs e = abs e' && (v.Rigidity = TyparFlexible))
                then (v, e) :: vs
                else (v',e') :: (v,e) :: vs' 
    findmin vs
  
let SubstMeasure (r:Typar) ms = 
    if r.Rigidity = TyparRigid then error(InternalError("SubstMeasure: rigid",r.Range)); 
    if r.Kind = KindType then error(InternalError("SubstMeasure: kind=type",r.Range));

    let tp = r.Data
    match tp.typar_solution with
    | None -> tp.typar_solution <- Some (TType_measure ms)
    | Some _ -> error(InternalError("already solved",r.Range));

let rec TransactStaticReq (csenv:ConstraintSolverEnv) trace (tpr:Typar) req = 
    let m = csenv.cs_m
    if (tpr.Rigidity = TyparRigid && tpr.StaticReq <> req) then 
        ErrorD(ConstraintSolverError("The declared type parameter '"^tpr.Name^" cannot be used here since the type parameter cannot be resolved at compile time",m,m)) 
    else
        let tpdata = tpr.Data
        let orig = tpr.StaticReq
        match trace with 
        | NoTrace -> () 
        | WithTrace (Trace actions) -> actions := (fun () -> set_static_req_of_tpdata tpdata orig) :: !actions
        set_static_req_of_tpdata tpdata req;
        CompleteD

and SolveTypStaticReqTypar (csenv:ConstraintSolverEnv) trace req (tpr:Typar) =
    let orig = tpr.StaticReq
    let req2 = JoinTyparStaticReq req orig
    if orig <> req2 then TransactStaticReq csenv trace tpr req2 else CompleteD

and SolveTypStaticReq (csenv:ConstraintSolverEnv) trace req ty =
    match req with 
    | NoStaticReq -> CompleteD
    | HeadTypeStaticReq -> 
        (* requires that a type constructor be known at compile time *)
        match strip_tpeqns ty with
        | TType_measure ms ->
          let vs = ListMeasureVarOccsWithNonZeroExponents ms
          IterateD (fun ((tpr:Typar),_) -> SolveTypStaticReqTypar csenv trace req tpr) vs

        | _ -> 
          if (is_anypar_typ csenv.g ty) then 
            let tpr = dest_anypar_typ csenv.g ty
            SolveTypStaticReqTypar csenv trace req tpr
          else CompleteD
      
let rec TransactDynamicReq (csenv:ConstraintSolverEnv) trace (tpr:Typar) req = 
    let m = csenv.cs_m
    let tpdata = tpr.Data
    let orig = tpr.DynamicReq
    match trace with 
    | NoTrace -> () 
    | WithTrace (Trace actions) -> actions := (fun () -> set_dynamic_req_of_tpdata tpdata orig) :: !actions
    set_dynamic_req_of_tpdata tpdata req;
    CompleteD

and SolveTypDynamicReq (csenv:ConstraintSolverEnv) trace req ty =
    match req with 
    | NoDynamicReq -> CompleteD
    | DynamicReq -> 
        if (is_anypar_typ csenv.g ty) then 
            let tpr = dest_anypar_typ csenv.g ty
            if tpr.DynamicReq <> DynamicReq then TransactDynamicReq csenv trace tpr DynamicReq else CompleteD
        else CompleteD

let SubstMeasureWarnIfRigid (csenv:ConstraintSolverEnv) trace (v:Typar) ms =
    if v.Rigidity = TyparWarnIfNotRigid && not (is_anypar_typ csenv.g (TType_measure ms)) then         
      // NOTE: we grab the name eagerly to make sure the type variable prints as a type variable 
      let tpnmOpt = if v.IsCompilerGenerated then None else Some v.Name 
      SolveTypStaticReq csenv trace v.StaticReq (TType_measure ms) ++ (fun () -> 
      SubstMeasure v ms;
      WarnD(NonRigidTypar(csenv.cs_denv,tpnmOpt,v.Range,TType_measure (MeasureVar v), TType_measure ms,csenv.cs_m)))
    else 
      // Propagate static requirements from 'tp' to 'ty' 
      SolveTypStaticReq csenv trace v.StaticReq (TType_measure ms) ++ (fun () -> 
      SubstMeasure v ms;
      if v.Rigidity = TyparAnon && measure_equiv csenv.g ms MeasureOne then 
        WarnD(Error("This code is less generic than indicated by its annotations. A unit-of-measure specified using '_' has been determined to be '1', i.e. dimensionless. Consider making the code generic, or removing the use of '_'",v.Range))
      else CompleteD)

/// The division operator in Caml/F# rounds towards zero. For our purposes,
/// we want to round towards negative infinity. 
let DivRoundDown x y = 
    let signx=if x<0 then -1 else 1
    let signy=if y<0 then -1 else 1
  
    if signx=signy then x / y
    else (x-y+signy) / y

/// Imperatively unify the unit-of-measure expression ms against 1.
/// This is a gcd-like algorithm that proceeds as follows:
/// 1. Express ms in the form 'u1^x1 * ... * 'un^xn * c1^y1 * ... * cm^ym
///    where 'u1,...,'un are non-rigid measure variables, c1,...,cm are measure identifiers or rigid measure variables,
///    x1,...,xn and y1,...,yn are non-zero exponents with |x1| <= |xi| for all i.
/// 2. (a) If m=n=0 then we're done (we're unifying 1 against 1)
///    (b) If m=0 but n<>0 then fail (we're unifying a variable-free expression against 1)
///    (c) If xi is divisible by |x1| for all i, and yj is divisible by |x1| for all j, then 
///        immediately solve the constraint with the substitution 
///          'u1 := 'u2^(-x2/x1) * ... * 'un^(-xn/x1) * c1^(-y1/x1) * ... * cm^(-ym/x1)
///    (d) Otherwise, if m=1, fail (example: unifying 'u^2 * kg^3)
///    (e) Otherwise, make the substitution
///          'u1 := 'u * 'u2^(-x2/x1) * ... * 'un^(-xn/x1) * c1^(-y1/x1) * ... * cm^(-ym/x1)
///        where 'u is a fresh measure variable, and iterate.

let rec UnifyMeasureWithOne (csenv:ConstraintSolverEnv) trace ms = 
    if verbose then dprintf "  UnifyMeasureWithOne...%s\n" ("ms = " ^ Layout.showL(typeL (TType_measure ms)));
    let (rigidvars,nonrigidvars) = (ListMeasureVarOccsWithNonZeroExponents ms) |> List.partition (fun (v,_) -> v.Rigidity = TyparRigid) 
    let cons = ListMeasureConOccsWithNonZeroExponents csenv.g true ms
    match FindMinimumMeasureExponent nonrigidvars, rigidvars, cons with
    | [], [], [] -> CompleteD
    | [], _, _ -> localAbortD
    | (v,e)::vs, rigidvars, cs -> 
      let newms = ProdMeasures (List.map (fun (c,e') -> MeasurePower (MeasureCon c) (- (DivRoundDown e' e))) cs 
                              @ List.map (fun (v,e') -> MeasurePower (MeasureVar v) (- (DivRoundDown e' e))) (vs @ rigidvars))
      if cs |> List.forall (fun (_,e') -> e' % e = 0) && (vs@rigidvars) |> List.forall (fun (_,e') -> e' % e = 0) 
      then SubstMeasureWarnIfRigid csenv trace v newms
      elif isNil vs 
      then localAbortD
      else
        // New variable v' must inherit WarnIfNotRigid from v
        let v' = new_anon_inference_var (KindMeasure,v.Range,v.Rigidity,v.StaticReq,v.DynamicReq)
        SubstMeasure v (MeasureProd(MeasureVar v', newms));
        UnifyMeasureWithOne csenv trace ms

/// Imperatively unify unit-of-measure expression ms1 against ms2
let UnifyMeasures (csenv:ConstraintSolverEnv) trace ms1 ms2 = 
    if verbose then dprintf "UnifyMeasures...%s\n" ("ms1 = "^Layout.showL(typeL (TType_measure ms1))^", ms2 = "^Layout.showL(typeL (TType_measure ms2)));
    UnifyMeasureWithOne csenv trace (MeasureProd(ms1,MeasureInv ms2))


/// Simplify a unit-of-measure expression ms that forms part of a type scheme. 
/// We make substitutions for vars, which are the (remaining) bound variables
///   in the scheme that we wish to simplify. 
let SimplifyMeasure g vars ms =
    if verbose then dprintf ("SimplifyMeasure ms = %s generalizable = %s\n") (Layout.showL (typeL (TType_measure ms))) (String.concat "," (List.map (fun tp -> Layout.showL (typeL (mk_typar_ty tp))) vars));
    let rec simp vars = 
        match FindMinimumMeasureExponent (List.filter (fun (v,e) -> e<>0) (List.map (fun v -> (v, MeasureVarExponent v ms)) vars)) with
        | [] -> 
          (vars, None)

        | (v,e)::vs -> 
            if e < 0 then
                let v' = new_anon_inference_var (KindMeasure,v.Range,TyparFlexible,v.StaticReq,v.DynamicReq)
                let vars' = v' :: ListSet.remove typar_ref_eq v vars 
                SubstMeasure v (MeasureInv (MeasureVar v'));
                simp vars'
            else 
                let newv = if v.IsCompilerGenerated then new_anon_inference_var (KindMeasure,v.Range,TyparFlexible,v.StaticReq,v.DynamicReq)
                                                    else new_named_inference_measurevar (v.Range,TyparFlexible,v.StaticReq,v.Id)
                let remainingvars = ListSet.remove typar_ref_eq v vars
                let newms = (ProdMeasures (List.map (fun (c,e') -> MeasurePower (MeasureCon c) (- (DivRoundDown e' e))) (ListMeasureConOccsWithNonZeroExponents g false ms)
                                            @ List.map (fun (v',e') -> if typar_ref_eq v v' then MeasureVar newv else MeasurePower (MeasureVar v') (- (DivRoundDown e' e))) (ListMeasureVarOccsWithNonZeroExponents ms)));
                SubstMeasure v newms;
                match vs with 
                | [] -> (remainingvars, Some newv) 
                | _ -> simp (newv::remainingvars)
   
    simp vars

// Normalize a type ty that forms part of a unit-of-measure-polymorphic type scheme. 
//  Generalizable are the unit-of-measure variables that remain to be simplified. Generalized
// is a list of unit-of-measure variables that have already been generalized. 
let rec SimplifyMeasuresInType g resultFirst ((generalizable, generalized) as param) ty =
    if verbose then dprintf ("SimplifyMeasuresInType ty = %s generalizable = %s\n") (Layout.showL (typeL ty)) (String.concat "," (List.map (fun tp -> Layout.showL (typeL (mk_typar_ty tp))) generalizable));
    match strip_tpeqns ty with 
    | TType_ucase(_,l)
    | TType_app (_,l) 
    | TType_tuple l -> SimplifyMeasuresInTypes g param l

    | TType_fun (d,r) -> if resultFirst then SimplifyMeasuresInTypes g param [r;d] else SimplifyMeasuresInTypes g param [d;r]        
    | TType_var r   -> param
    | TType_forall (tp,tau) -> SimplifyMeasuresInType g resultFirst param tau
    | TType_measure measure -> 
        let (generalizable', newlygeneralized) = SimplifyMeasure g generalizable measure   
        if verbose then dprintf "newlygeneralized = %s\n" (match newlygeneralized with None -> "none" | Some tp -> Layout.showL (typeL (mk_typar_ty tp)));
        match newlygeneralized with
        | None -> (generalizable', generalized)
        | Some v -> (generalizable', v::generalized)
    | _ -> param

and SimplifyMeasuresInTypes g param tys = 
    match tys with
    | [] ->  param
    | ty::tys -> 
        let param' = SimplifyMeasuresInType g false param ty 
        SimplifyMeasuresInTypes g param' tys

// We normalize unit-of-measure-polymorphic type schemes as described in Kennedy's thesis. There  
// are three reasons for doing this:
//   (1) to present concise and consistent type schemes to the programmer
//   (2) so that we can compute equivalence of type schemes in signature matching
//   (3) in order to produce a list of type parameters ordered as they appear in the (normalized) scheme.
//
// Representing the normal form as a matrix, with a row for each variable,
// and a column for each unit-of-measure expression in the "skeleton" of the type. Entries are integer exponents.
//  
// ( 0...0  a1  as1    b1  bs1    c1  cs1    ...)
// ( 0...0  0   0...0  b2  bs2    c2  cs2    ...)
// ( 0...0  0   0...0  0   0...0  c3  cs3    ...)
//...
// ( 0...0  0   0...0  0   0...0  0   0...0  ...)
//
// The normal form is unique; what's more, it can be used to force a variable ordering 
// because the first occurrence of a variable in a type is in a unit-of-measure expression with no 
// other "new" variables (a1, b2, c3, above). 
//
// The corner entries a1, b2, c3 are all positive. Entries lying above them (b1, c1, c2, etc) are
// non-negative and smaller than the corresponding corner entry. Entries as1, bs1, bs2, etc are arbitrary.
// This is known as a *reduced row echelon* matrix or Hermite matrix.   
let SimplifyMeasuresInTypeScheme g resultFirst (generalizable:Typar list) ty =
    // Only bother if we're generalizing over at least one unit-of-measure variable 
    let uvars, vars = 
        generalizable |> List.partition (fun v -> v.Kind = KindMeasure && v.Rigidity <> TyparRigid) 
 
    match uvars with
    | [] -> generalizable
    | _::_ ->
    let (_, generalized) = SimplifyMeasuresInType g resultFirst (uvars, []) ty 
   
    vars @ List.rev generalized

let freshMeasure () = MeasureVar (new_inference_measurevar ())

let CheckWarnIfRigid (csenv:ConstraintSolverEnv) ty1 (r:Typar) ty =
    let g = csenv.g
    let denv = csenv.cs_denv
    if r.Rigidity = TyparWarnIfNotRigid && 
       (not (is_anypar_typ g ty) ||
        (let tp2 = dest_anypar_typ g ty 
         not tp2.IsCompilerGenerated &&
         (r.IsCompilerGenerated || 
          // exclude this warning for two identically named user-specified type parameters, e.g. from different mutually recursive functions or types
          r.DisplayName <> tp2.DisplayName )))  then 
        
        // NOTE: we grab the name eagerly to make sure the type variable prints as a type variable 
        let tpnmOpt = if r.IsCompilerGenerated then None else Some r.Name 
        WarnD(NonRigidTypar(denv,tpnmOpt,r.Range,ty1,ty,csenv.cs_m  )) 
    else 
        CompleteD

/// Return true if we would rather unify this variable v1 := v2 than vice versa
let PreferUnifyTypar (v1:Typar) (v2:Typar) =
    match v1.Rigidity,v2.Rigidity with 
    // Rigid > all
    | TyparRigid,_ -> false
    // Prefer to unify away WarnIfNotRigid in favour of Rigid
    | TyparWarnIfNotRigid,TyparRigid -> true
    | TyparWarnIfNotRigid,TyparWarnIfNotRigid -> true
    | TyparWarnIfNotRigid,TyparAnon -> false
    | TyparWarnIfNotRigid,TyparFlexible -> false
    // Prefer to unify away anonymous variables in favour of Rigid, WarnIfNotRigid 
    | TyparAnon,TyparRigid -> true
    | TyparAnon,TyparWarnIfNotRigid -> true
    | TyparAnon,TyparAnon -> true
    | TyparAnon,TyparFlexible -> false
    // Prefer to unify away Flexible in favour of Rigid, WarnIfNotRigid or Anon
    | TyparFlexible,TyparRigid -> true
    | TyparFlexible,TyparWarnIfNotRigid -> true
    | TyparFlexible,TyparAnon -> true
    | TyparFlexible,TyparFlexible -> 

      // Prefer to unify away compiler generated type vars
      match v1.IsCompilerGenerated, v2.IsCompilerGenerated with
      | true,false -> true
      | false,true -> false
      | _ -> 
         // Prefer to unify away non-error vars - gives better error recovery since we keep
         // error vars lying around, and can avoid giving errors about illegal polymorphism 
         // if they occur 
         match v1.IsFromError, v2.IsFromError with
         | true,false -> false
         | _ -> true


/// Add the constraint "ty1 = ty" to the constraint problem, where ty1 is a type variable. 
/// Propagate all effects of adding this constraint, e.g. to solve other variables 
let rec SolveTyparEqualsTyp (csenv:ConstraintSolverEnv) ndeep m2 trace ty1 ty =
    if verbose then dprintf "--> SolveTyparEqualsTyp...%s\n" ("ty1 = "^Layout.showL(typeL ty1)^", ty = "^Layout.showL(typeL ty));
    let m = csenv.cs_m
    let denv = csenv.cs_denv
    DepthCheck ndeep m  ++ (fun () -> 
    match ty1 with 
    | TType_var r  ->
      if r.Kind = KindMeasure then error(InternalError("SolveTyparEqualsTyp: unit",m));

      // The types may still be equivalent due to abbreviations, which we are trying not to eliminate 
      if type_equiv csenv.g ty1 ty then CompleteD else

      // The famous 'OccursCheck' check to catch things like 'a = list<'a> 
      if OccursCheck csenv.g r ty then ErrorD (ConstraintSolverInfiniteTypes(denv,ty1,ty,m,m2)) else

      // Note: warn _and_ continue! 
      CheckWarnIfRigid (csenv:ConstraintSolverEnv) ty1 r ty ++ (fun () ->

      // Record the solution before we solve the constraints, since 
      // We may need to make use of the equation when solving the constraints. 
      // Record a entry in the undo trace if one is provided 
      let tpdata = r.Data
      match trace with 
      | NoTrace -> () 
      | WithTrace (Trace actions) -> actions := (fun () -> tpdata.typar_solution <- None) :: !actions
      tpdata.typar_solution <- Some ty;
      
  (*   dprintf "setting typar %d to type %s at %a\n" r.Stamp ((DebugPrint.showType ty)) output_range m; *)

      (* Only solve constraints if this is not an error var *)
      if r.IsFromError then CompleteD else
      
      // Check to see if this type variable is relevant to any trait constraints. 
      // If so, re-solve the relevant constraints. 
      (if csenv.cs_css.css_cxs.ContainsKey r.Stamp then 
           RepeatWhileD (fun () -> SolveRelevantMemberConstraintsForTypar csenv ndeep false trace r)
       else 
           CompleteD) ++ (fun _ ->
      
      // Re-solve the other constraints associated with this type variable 
      solveTypMeetsTyparConstraints csenv ndeep m2 trace ty (r.DynamicReq,r.StaticReq,r.Constraints)))
      
    | TType_measure (MeasureVar r) ->
        if r.Kind = KindType then error(InternalError("SolveTyparEqualsTyp: kind=type for unit var",m));

        CheckWarnIfRigid (csenv:ConstraintSolverEnv) ty1 r ty ++ (fun () ->
  
        solveTypMeetsTyparConstraints csenv ndeep m2 trace ty (r.DynamicReq,r.StaticReq,r.Constraints) ++ (fun () ->
        match ty with 
        | TType_measure ms ->
            let tp = r.Data
            tp.typar_solution <- Some (TType_measure ms); CompleteD
        | _ -> failwith "SolveTyparEqualsTyp: unit-of-measure var unified with type"))

    | _ -> failwith "SolveTyparEqualsTyp")

/// Given a type 'ty' and a set of constraints on that type, solve those constraints. 
and solveTypMeetsTyparConstraints (csenv:ConstraintSolverEnv) ndeep m2 trace ty (dreq,sreq,cs) =
    let g = csenv.g
    let m = csenv.cs_m
    // Propagate dynamic requirements from 'tp' to 'ty'
    SolveTypDynamicReq csenv trace dreq ty ++ (fun () -> 
    // Propagate static requirements from 'tp' to 'ty' 
    SolveTypStaticReq csenv trace sreq ty ++ (fun () -> 
    
    // Solve constraints on 'tp' w.r.t. 'ty' 
    cs |> IterateD (function
      | TTyparDefaultsToType (priority,dty,m) -> 
          if not (is_typar_typ g ty) or type_equiv g ty dty then CompleteD else
          AddConstraint csenv ndeep m2 trace (dest_typar_typ g ty)  (TTyparDefaultsToType(priority,dty,m))
          
      | TTyparSupportsNull m2               -> SolveTypSupportsNull               csenv ndeep m2 trace ty
      | TTyparIsEnum(underlying, m2)        -> SolveTypIsEnum                     csenv ndeep m2 trace ty underlying
      | TTyparIsDelegate(aty,bty, m2)       -> SolveTypIsDelegate                 csenv ndeep m2 trace ty aty bty
      | TTyparIsNotNullableValueType m2     -> SolveTypIsNonNullableValueType     csenv ndeep m2 trace ty
      | TTyparIsReferenceType m2            -> SolveTypIsReferenceType            csenv ndeep m2 trace ty
      | TTyparRequiresDefaultConstructor m2 -> SolveTypRequiresDefaultConstructor csenv ndeep m2 trace ty
      | TTyparSimpleChoice(tys,m2)          -> SolveTypChoice                     csenv ndeep m2 trace ty tys
      | TTyparCoercesToType(ty2,m2) -> SolveTypSubsumesTypKeepAbbrevs csenv ndeep m2 trace ty2 ty
      | TTyparMayResolveMemberConstraint(traitInfo,m2) -> 
          SolveMemberConstraint csenv false ndeep m2 trace traitInfo ++ (fun _ -> CompleteD) 
    )))

        
/// Add the constraint "ty1 = ty2" to the constraint problem. 
/// Propagate all effects of adding this constraint, e.g. to solve type variables 
and solveTypEqualsTyp (csenv:ConstraintSolverEnv) ndeep m2 trace ty1 ty2 = 
    if verbose then  dprintf "solveTypEqualsTyp ndeep @ %a\n" output_range csenv.cs_m;
(*     dprintf "solveTypEqualsTyp ty1=%s ty2=%s\n" (showL (typeL ty1)) (showL (typeL ty2)); *)
    let ndeep = ndeep + 1
    let aenv = csenv.cs_aenv
    let g = csenv.g
    let m = csenv.cs_m
    let denv = csenv.cs_denv
    if ty1 === ty2 then CompleteD else
    let canShortcut = (isNoTrace trace)
    let sty1 = strip_tpeqns_and_tcabbrevsA csenv.g canShortcut ty1
    let sty2 = strip_tpeqns_and_tcabbrevsA csenv.g canShortcut ty2
(*      dprintf "sty1=%s sty2=%s\n" (showL (typeL sty1)) (showL (typeL sty2));  *)
    match sty1, sty2 with 
    // type vars inside forall-types may be alpha-equivalent 
    | TType_var tp1, TType_var tp2 when  typar_ref_eq tp1 tp2 || (tpmap_mem tp1 aenv.ae_typars && type_equiv g (tpmap_find tp1 aenv.ae_typars) ty2) -> CompleteD

    | TType_var tp1, TType_var tp2 when PreferUnifyTypar tp1 tp2 -> SolveTyparEqualsTyp csenv ndeep m2 trace sty1 ty2
    | TType_var tp1, TType_var tp2 when PreferUnifyTypar tp2 tp1 -> SolveTyparEqualsTyp csenv ndeep m2 trace sty2 ty1

    | TType_var r, _ when (r.Rigidity <> TyparRigid) -> SolveTyparEqualsTyp csenv ndeep m2 trace sty1 ty2
    | _, TType_var r when (r.Rigidity <> TyparRigid) -> SolveTyparEqualsTyp csenv ndeep m2 trace sty2 ty1

    // Catch float<_>=float<1>, float32<_>=float32<1> and decimal<_>=decimal<1> 
    | (_, TType_app (tc2,[ms])) when (tc2.IsMeasureableReprTycon && type_equiv csenv.g sty1 (reduce_tcref_measureable tc2 [ms]))
        -> solveTypEqualsTyp csenv ndeep m2 trace ms (TType_measure MeasureOne)
    | (TType_app (tc2,[ms]), _) when (tc2.IsMeasureableReprTycon && type_equiv csenv.g sty2 (reduce_tcref_measureable tc2 [ms]))
        -> solveTypEqualsTyp csenv ndeep m2 trace ms (TType_measure MeasureOne)

    | TType_app (tc1,l1)  ,TType_app (tc2,l2) when tcref_eq g tc1 tc2  -> SolveTypEqualsTypEqns csenv ndeep m2 trace l1 l2
    | TType_app (tc1,_)   ,TType_app (tc2,_)   ->  localAbortD
    | TType_tuple l1      ,TType_tuple l2      -> SolveTypEqualsTypEqns csenv ndeep m2 trace l1 l2
    | TType_fun (d1,r1)   ,TType_fun (d2,r2)   -> SolveFunTypEqn csenv ndeep m2 trace d1 d2 r1 r2
    | TType_measure ms1   ,TType_measure ms2   -> UnifyMeasures csenv trace ms1 ms2
    | TType_forall(tps1,rty1), TType_forall(tps2,rty2) -> 
        if (List.length tps1 <> List.length tps2) then localAbortD else
        let aenv = bind_tyeq_env_typars tps1 tps2 aenv
        let csenv = {csenv with cs_aenv = aenv }
        if not (typar_decls_aequiv g aenv tps1 tps2) then localAbortD else
        SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty1 rty2 

    | TType_ucase (uc1,l1)  ,TType_ucase (uc2,l2) when g.ucref_eq uc1 uc2  -> SolveTypEqualsTypEqns csenv ndeep m2 trace l1 l2


    | _  -> localAbortD

and SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ty1 ty2 = 
   let denv = csenv.cs_denv
   
   // Back out of back out of expansions of type abbreviations to give improved error messages. 
   TryD (fun () -> solveTypEqualsTyp csenv ndeep m2 trace ty1 ty2)
        (function LocallyAbortOperationThatLosesAbbrevs -> ErrorD(ConstraintSolverTypesNotInEqualityRelation(denv,ty1,ty2,csenv.cs_m,m2))
                | err -> ErrorD err)

and SolveTypEqualsTypEqns csenv ndeep m2 trace origl1 origl2 = 
   match origl1,origl2 with 
   | [],[] -> CompleteD 
   | _ -> 
       // We unwind Iterate2D by hand here for performance reasons.
       let rec loop l1 l2 = 
           match l1,l2 with 
           | [],[] -> CompleteD 
           | h1::t1, h2::t2 -> 
               SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace h1 h2 ++ (fun () -> loop t1 t2) 
           | _ -> 
               ErrorD(ConstraintSolverTupleDiffLengths(csenv.cs_denv,origl1,origl2,csenv.cs_m,m2)) 
       loop origl1 origl2

and SolveFunTypEqn csenv ndeep m2 trace d1 d2 r1 r2 = 
    SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace d1 d2 ++ (fun () -> 
    SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace r1 r2)

and SolveTypSubsumesTyp (csenv:ConstraintSolverEnv) ndeep m2 trace ty1 ty2 = 
    // 'a :> obj ---> <solved> 
    let ndeep = ndeep + 1
    let g = csenv.g
    let amap = csenv.amap
    let aenv = csenv.cs_aenv
    let denv = csenv.cs_denv
    let m = csenv.cs_m
    if is_obj_typ g ty1 then CompleteD else 
    let canShortcut = (isNoTrace trace)
    let sty1 = strip_tpeqns_and_tcabbrevsA csenv.g canShortcut ty1
    let sty2 = strip_tpeqns_and_tcabbrevsA csenv.g canShortcut ty2
    match sty1, sty2 with 
    | TType_var tp1, _ 
        when  tpmap_mem tp1 aenv.ae_typars -> 
        SolveTypSubsumesTyp csenv ndeep m2 trace (tpmap_find tp1 aenv.ae_typars) ty2 
        
    | TType_var r1, TType_var r2 when typar_ref_eq r1 r2 -> CompleteD
    | _, TType_var r (* when not (rigid_of_typar r) *) -> SolveTyparSubtypeOfType csenv ndeep m2 trace r ty1
    | TType_var r , _ (* | _, TType_var r *)  ->  SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ty1 ty2
    | TType_tuple l1    ,TType_tuple l2     -> SolveTypEqualsTypEqns csenv ndeep m2 trace l1 l2 (* nb. can unify since no variance *)
    | TType_fun (d1,r1)  ,TType_fun (d2,r2)   -> SolveFunTypEqn csenv ndeep m2 trace d1 d2 r1 r2 (* nb. can unify since no variance *)
    | TType_measure ms1, TType_measure ms2    -> UnifyMeasures csenv trace ms1 ms2

    // Enforce the identities float=float<1>, float32=float32<1> and decimal=decimal<1> 
    | (_, TType_app (tc2,[ms])) when (tc2.IsMeasureableReprTycon && type_equiv csenv.g sty1 (reduce_tcref_measureable tc2 [ms]))
        -> SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ms (TType_measure MeasureOne)
    | (TType_app (tc2,[ms]), _) when (tc2.IsMeasureableReprTycon && type_equiv csenv.g sty2 (reduce_tcref_measureable tc2 [ms]))
        -> SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ms (TType_measure MeasureOne)

    | TType_app (tc1,l1)  ,TType_app (tc2,l2) when tcref_eq g tc1 tc2  -> 
        SolveTypEqualsTypEqns csenv ndeep m2 trace l1 l2

    | TType_ucase (uc1,l1)  ,TType_ucase (uc2,l2) when g.ucref_eq uc1 uc2  -> 
        SolveTypEqualsTypEqns csenv ndeep m2 trace l1 l2

    | _ ->  
        // By now we know the type is not a variable type 

        // C :> obj ---> <solved> 
        if is_obj_typ g ty1 then CompleteD else

        // 'a[] :> IList<'b>   ---> 'a = 'b  
        // 'a[] :> ICollection<'b>   ---> 'a = 'b  
        // 'a[] :> IEnumerable<'b>   ---> 'a = 'b  
        // Note we don't support co-variance on array types nor 
        // the special .NET conversions for these types 
        if 
            (is_il_arr1_typ g ty2 &&  
             is_stripped_tyapp_typ g ty1 && 
             (let tcr1 = tcref_of_stripped_typ g ty1
              tcref_eq g tcr1 g.tcref_System_Collections_Generic_IList || 
              tcref_eq g tcr1 g.tcref_System_Collections_Generic_ICollection || 
              tcref_eq g tcr1 g.tcref_System_Collections_Generic_IEnumerable)) then

          let tcref,tinst = dest_stripped_tyapp_typ g ty1
          match tinst with 
          | [ty1arg] -> 
              let ty2arg = dest_il_arr1_typ g ty2
              SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ty1arg  ty2arg
          | _ -> error(InternalError("dest_il_arr1_typ",m));

        // D<inst> :> Head<_> --> C<inst'> :> Head<_> for the 
        // first interface or super-class C supported by D which 
        // may feasibly convert to Head. 

        else 
            match (FindUniqueFeasibleSupertype g amap m ty1 ty2) with 
            | None -> ErrorD(ConstraintSolverTypesNotInSubsumptionRelation(denv,ty1,ty2,m,m2))
            | Some t -> SolveTypSubsumesTyp csenv ndeep m2 trace ty1 t

and SolveTypSubsumesTypKeepAbbrevs csenv ndeep m2 trace ty1 ty2 = 
   let denv = csenv.cs_denv
   TryD (fun () -> SolveTypSubsumesTyp csenv ndeep m2 trace ty1 ty2)
        (function LocallyAbortOperationThatLosesAbbrevs -> ErrorD(ConstraintSolverTypesNotInSubsumptionRelation(denv,ty1,ty2,csenv.cs_m,m2))
                | err -> ErrorD err)

//-------------------------------------------------------------------------
// Solve and record non-equality constraints
//------------------------------------------------------------------------- 

      
and SolveTyparSubtypeOfType (csenv:ConstraintSolverEnv) ndeep m2 trace tp ty1 = 
    let g = csenv.g
    let m = csenv.cs_m
    if is_obj_typ g ty1 then CompleteD
    elif type_equiv g ty1 (mk_typar_ty tp) then CompleteD
    elif is_sealed_typ g ty1 then 
        SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace (mk_typar_ty tp) ty1
    else 
        AddConstraint csenv ndeep m2 trace tp  (TTyparCoercesToType(ty1,m))

and DepthCheck ndeep m = 
  if ndeep > 300 then ErrorD(Error("Type inference problem too complicated (maximum iteration depth reached). Consider adding further type annotations",m)) else CompleteD

// If this is a type that's parameterized on a unit-of-measure (expected to be numeric), unify its measure with 1
and SolveDimensionlessNumericType (csenv:ConstraintSolverEnv) ndeep m2 trace ty =
    match GetMeasureOfType csenv.g ty with
    | Some (tcref,ms2) ->
      SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ty (mk_tyapp_ty tcref [TType_measure MeasureOne])
    | None ->
      CompleteD

/// We do a bunch of fakery to pretend that primitive types have certain members. 
/// We pretend int and other types support a number of operators.  In the actual IL for mscorlib they 
/// don't, however the type-directed static optimization rules in the library code that makes use of this 
/// will deal with the problem. 
and SolveMemberConstraint (csenv:ConstraintSolverEnv) canon ndeep m2 trace (TTrait(tys,nm,memFlags,argtys,rty,sln)) :  OperationResult<bool> =
    //if sln.Value.IsSome then ResultD true else
    let g = csenv.g
    let m = csenv.cs_m
    let amap = csenv.amap
    let aenv = csenv.cs_aenv
    let denv = csenv.cs_denv
    DepthCheck ndeep m  ++ (fun () -> 

    if verbose then dprintf "-----------------------------\nResolve trait for %s\n" nm;

    // Remove duplicates from the set of types in the support 
    let tys = ListSet.setify (type_aequiv g aenv) tys
    // Rebuild the trait infor after removing duplicates 
    let traitInfo = (TTrait(tys,nm,memFlags,argtys,rty,sln))
    let rty = GetFSharpViewOfReturnType g rty    
    
    // Assert the object type if the constraint is for an instance member
    begin
        if memFlags.MemberIsInstance then 
            match tys, argtys with 
            | [ty], (h :: t) -> SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace h ty
            | _ -> ErrorD (ConstraintSolverError("Expected arguments to an instance member", m,m2))
        else CompleteD
    end ++ (fun () -> 
    
    // Trait calls are only supported on pseudo type (variables) 
    tys |> IterateD (SolveTypStaticReq csenv trace HeadTypeStaticReq)) ++ (fun () -> 
    
    let argtys = if memFlags.MemberIsInstance then List.tl argtys else argtys

    let minfos = GetRelevantMethodsForTrait csenv canon nm traitInfo

    if verbose then minfos |> List.iter (fun minfo -> dprintf "Possible overload: %s\n" (string_of_minfo amap m denv minfo));
        
    match minfos,tys,memFlags.MemberIsInstance,nm,argtys with 
      | _,_,false,("op_Division" | "op_Multiply"),[argty1;argty2]
          when 
               // This simulates the existence of 
               //    float * float -> float
               //    float32 * float32 -> float32
               //    float<'u> * float<'v> -> float<'u 'v>
               //    float32<'u> * float32<'v> -> float32<'u 'v>
               //    decimal<'u> * decimal<'v> -> decimal<'u 'v>
               //    decimal<'u> * decimal -> decimal<'u>
               //    float32<'u> * float32<'v> -> float32<'u 'v>
               //    int * int -> int
               //    int64 * int64 -> int64
               //
               // The rule is triggered by these sorts of inputs when canon=false
               //    float * float 
               //    float * float32 // will give error 
               //    decimal<m> * decimal<m>
               //    decimal<m> * decimal  <-- Note this one triggers even though "decimal" has some possibly-relevant methods
               //    float * Matrix // the rule doesn't trigger for this one since Matrix has overloads we can use and we prefer those instead
               //    float * Matrix // the rule doesn't trigger for this one since Matrix has overloads we can use and we prefer those instead
               //
               // The rule is triggered by these sorts of inputs when canon=true
               //    float * 'a 
               //    'a * float 
               //    decimal<'u> * 'a <---
                  (let checkRuleAppliesInPreferenceToMethods argty1 argty2 = 
                     // Check that at least one of the argument types is numeric
                     (IsNumericOrIntegralEnumType g argty1) && 
                     // Check that the support of type variables is empty. That is,
                     // if we're canonicalizing, then having one of the types nominal is sufficient.
                     // If not, then both must be nominal (i.e. not a type variable).
                     (canon || not (is_typar_typ g argty2)) &&
                     // This next condition checks that either 
                     //   - Neither type contributes any methods OR
                     //   - We have the special case "decimal<_> * decimal". In this case we have some 
                     //     possibly-relevant methods from "decimal" but we ignore them in this case.
                     (isNil minfos || (isSome (GetMeasureOfType g argty1) && IsDecimalType g argty2)) in

                   checkRuleAppliesInPreferenceToMethods argty1 argty2 || 
                   checkRuleAppliesInPreferenceToMethods argty2 argty1) ->
                   
          match GetMeasureOfType g argty1 with
          | Some (tcref,ms1) ->
            let ms2 = freshMeasure ()
            SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty2 (mk_tyapp_ty tcref [TType_measure ms2]) ++ (fun () ->
            SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty (mk_tyapp_ty tcref [TType_measure (MeasureProd(ms1,if nm = "op_Multiply" then ms2 else MeasureInv ms2))]) ++ (fun () ->
            ResultD TTraitBuiltIn))
          | _ ->
            match GetMeasureOfType g argty2 with
            | Some (tcref,ms2) ->
              let ms1 = freshMeasure ()
              SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty1 (mk_tyapp_ty tcref [TType_measure ms1]) ++ (fun () ->
              SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty (mk_tyapp_ty tcref [TType_measure (MeasureProd(ms1, if nm = "op_Multiply" then ms2 else MeasureInv ms2))]) ++ (fun () ->
              ResultD TTraitBuiltIn))
            | _ ->
              SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty2 argty1 ++ (fun () -> 
              SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty1 ++ (fun () -> 
              ResultD TTraitBuiltIn))

      | [],_,false,("op_Addition" | "op_Subtraction" | "op_Modulus"),[argty1;argty2] 
          when    (IsNumericOrIntegralEnumType g argty1 || (nm = "op_Addition" && (IsCharType g argty1 || IsStringType g argty1))) && (canon || not (is_typar_typ g argty2))
               || (IsNumericOrIntegralEnumType g argty2 || (nm = "op_Addition" && (IsCharType g argty1 || IsStringType g argty1))) && (canon || not (is_typar_typ g argty1)) ->
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty2 argty1 ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty1 ++ (fun () -> 
          ResultD TTraitBuiltIn))


      // We pretend for uniformity that the numeric types have a static property called Zero and One 
      // As with constants, only zero is polymorphic in its units
      | [],[ty],false,"get_Zero",[] 
          when IsNumericType g ty ->
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty ty ++ (fun () -> 
          ResultD TTraitBuiltIn)

      | [],[ty],false,"get_One",[] 
          when IsNumericType g ty || IsCharType g ty ->
          SolveDimensionlessNumericType csenv ndeep m2 trace ty ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty ty ++ (fun () -> 
          ResultD TTraitBuiltIn))

      | [],_,false,("DivideByInt"),[argty1;argty2] 
          when IsFpType g argty1 || IsDecimalType g argty1 ->
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty2 g.int_ty ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty1 ++ (fun () -> 
          ResultD TTraitBuiltIn))

      // We pretend for uniformity that the 'string' and 'array' types have an indexer property called 'Item' 
      | [], [ty],true,("get_Item"),[argty1] 
          when IsStringType g ty ->

          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty1 g.int_ty ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty g.char_ty ++ (fun () -> 
          ResultD TTraitBuiltIn))
          (* WarnD(OCamlCompatibility("An use of the operator 'expr.[idx]' involved a lookup on an object of indeterminate type. This is deprecated in F# unless OCaml-comaptibility is enabled. Consider adding further type constraints",m) *)

      | [], [ty],true,("get_Item"),argtys
          when IsArrayTypeWithIndexer g ty ->

          (if rank_of_any_array_typ g ty <> argtys.Length then ErrorD(ConstraintSolverError(sprintf "This indexer expects %d arguments but is here given %d" (rank_of_any_array_typ g ty) argtys.Length,m,m2)) else CompleteD) ++ (fun () ->
          (argtys |> IterateD (fun argty -> SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty g.int_ty)) ++ (fun () -> 
          let ety = dest_any_array_typ g ty
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty ety ++ (fun () -> 
          ResultD TTraitBuiltIn)))

      | [], [ty],true,("set_Item"),argtys
          when IsArrayTypeWithIndexer g ty ->
          
          (if rank_of_any_array_typ g ty <> argtys.Length - 1 then ErrorD(ConstraintSolverError(sprintf "This indexer expects %d arguments but is here given %d" (rank_of_any_array_typ g ty) (argtys.Length - 1),m,m2)) else CompleteD) ++ (fun () ->
          let argtys,ety = List.frontAndBack argtys
          (argtys |> IterateD (fun argty -> SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty g.int_ty)) ++ (fun () -> 
          let etys = dest_any_array_typ g ty
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ety etys ++ (fun () -> 
          ResultD TTraitBuiltIn)))

      | [],_,false,("op_BitwiseAnd" | "op_BitwiseOr" | "op_ExclusiveOr"),[argty1;argty2] 
          when    (IsIntegralOrIntegralEnumType g argty1 || (is_flag_enum_typ g argty1)) && (canon || not (is_typar_typ g argty2))
               || (IsIntegralOrIntegralEnumType g argty2 || (is_flag_enum_typ g argty2)) && (canon || not (is_typar_typ g argty1)) ->

          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty2 argty1 ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty1 ++ (fun () -> 
          SolveDimensionlessNumericType csenv ndeep m2 trace argty1 ++ (fun () -> 
          ResultD TTraitBuiltIn)));

      | [], _,false,("op_LeftShift" | "op_RightShift"),[argty1;argty2] 
          when    IsIntegralOrIntegralEnumType g argty1  ->

          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty2 g.int_ty ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty1 ++ (fun () -> 
          SolveDimensionlessNumericType csenv ndeep m2 trace argty1 ++ (fun () -> 
          ResultD TTraitBuiltIn)))

      | _,_,false,("op_UnaryPlus"),[argty] 
          when IsNumericOrIntegralEnumType g argty ->  

          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty ++ (fun () -> 
          ResultD TTraitBuiltIn)

      | _,_,false,("op_UnaryNegation"),[argty] 
          when IsSignedIntegralType g argty || IsFpType g argty || IsDecimalType g argty ->

          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty ++ (fun () -> 
          ResultD TTraitBuiltIn)

      | _,_,true,("get_Sign"),[] 
          when (let argty = tys.Head in IsSignedIntegralType g argty || IsFpType g argty || IsDecimalType g argty) ->

          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty g.int32_ty ++ (fun () -> 
          ResultD TTraitBuiltIn)

      | _,_,false,("op_LogicalNot" | "op_OnesComplement"),[argty] 
          when IsIntegralOrIntegralEnumType g argty  ->

          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty ++ (fun () -> 
          SolveDimensionlessNumericType csenv ndeep m2 trace argty ++ (fun () -> 
          ResultD TTraitBuiltIn))

      | _,_,false,("Abs"),[argty] 
          when IsSignedIntegralType g argty || IsFpType g argty || IsDecimalType g argty ->

          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty ++ (fun () -> 
          ResultD TTraitBuiltIn)

      | _,_,false,"Sqrt",[argty1] 
          when IsFpType g argty1 ->
          match GetMeasureOfType g argty1 with
            | Some (tcref, _) ->
              let ms1 = freshMeasure () 
              SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty1 (mk_tyapp_ty tcref [TType_measure (MeasureProd (ms1,ms1))]) ++ (fun () ->
              SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty (mk_tyapp_ty tcref [TType_measure ms1]) ++ (fun () ->
              ResultD TTraitBuiltIn))
            | None ->
              SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty1 ++ (fun () -> 
              ResultD TTraitBuiltIn)

      | _,_,false,("Sin" | "Cos" | "Tan" | "Sinh" | "Cosh" | "Tanh" | "Atan" | "Acos" | "Asin" | "Exp" | "Ceiling" | "Floor" | "Round" | "Truncate" | "Log10" | "Log" | "Sqrt"),[argty] 
          when IsFpType g argty ->

          SolveDimensionlessNumericType csenv ndeep m2 trace argty ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty ++ (fun () -> 
          ResultD TTraitBuiltIn))

      | _,_,false,("ToChar" | "ToByte" | "ToSByte" | "ToInt16" | "ToUInt16" | "ToInt32" | "ToUInt32" | "ToInt64" | "ToUInt64" | "ToSingle" | "ToDouble"),[argty] 
          when IsNonDecimalNumericOrIntegralEnumType g argty || IsStringType g argty || IsCharType g argty ->

          ResultD TTraitBuiltIn

      | _,_,false,("ToDecimal"),[argty] 
          when IsNumericOrIntegralEnumType g argty || IsStringType g argty ->

          ResultD TTraitBuiltIn

      | _,_,false,("ToUIntPtr" | "ToIntPtr"),[argty] 
          when IsNonDecimalNumericOrIntegralEnumType g argty || IsCharType g argty -> (* note: IntPtr and UIntPtr are different, they do not support .Parse() from string *)

          ResultD TTraitBuiltIn

      | [],_,false,"Pow",[argty1; argty2] 
          when IsFpType g argty1 ->
          
          SolveDimensionlessNumericType csenv ndeep m2 trace argty1 ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty2 argty1 ++ (fun () -> 
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty1 ++ (fun () -> 
          ResultD TTraitBuiltIn)))

      | _,_,false,("Atan2"),[argty1; argty2] 
          when IsFpType g argty1 ->
          SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace argty2 argty1 ++ (fun () ->
          match GetMeasureOfType g argty1 with
          | None -> SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty argty1
          | Some (tcref, _) -> SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty (mk_tyapp_ty tcref [TType_measure MeasureOne])) ++ (fun () ->
          ResultD TTraitBuiltIn)

      | _ -> 
          // OK, this is not solved by a built-in constraint.
          // Now look for real solutions

          match minfos,tys  with 
          | [],[ty] when not (is_typar_typ g ty) ->
              if tys |> List.exists (is_fun_typ g) then 
                  ErrorD (ConstraintSolverError("Expecting a type supporting the operator '"^DecompileOpName nm^"' but given a function type. You may be missing an argument to a function",m,m2)) 
              else
                  ErrorD (ConstraintSolverError("The type '"^(NicePrint.pretty_string_of_typ denv ty)^"' does not support any operators named '"^DecompileOpName nm^"'",m,m2))

          | _ -> 

              let dummyExpr = mk_unit g m
              let calledMethGroup = 
                  minfos |> List.map (fun minfo -> 
                      let callerArgs = argtys |> List.map (fun argty -> CallerArg(argty,m,false,dummyExpr))
                      let minst = FreshenMethInfo m minfo
                      let objtys = (ObjTypesOfMethInfo amap m minfo minst)
                      MakeCalledMeth(csenv.InfoReader,false, FreshenMethInfo, m,AccessibleFromEverywhere,minfo,minst,minst,None,objtys,[(callerArgs,[])],false))
              (* dprintf "    ---> calling ResolveOverloading, nm = %s, ty = '%s'\n" nm (Layout.showL (typeL ty)); *)

              let result,errors = 
                  CollectThenUndo (fun trace -> ResolveOverloading csenv (WithTrace(trace)) nm (0,0) AccessibleFromEverywhere calledMethGroup false (Some (rty,dummyExpr)))  

              if verbose then dprintf "    <--- called ResolveOverloading, ok? = %b\n" (isSome (CheckNoErrorsAndGetWarnings errors));
                  
              match result with 
              | Some (calledMeth:CalledMeth<_>) -> 
                  // OK, the constraint is solved. 
                  // Re-run without undo to commit the inference equations. Throw errors away 
                  let minfo = calledMeth.Method
                  if verbose then dprintf "    ---> constraint solved, calling ResolveOverloading a second time, without undo, minfo = %s\n" (string_of_minfo amap m denv minfo);
                  let _,errors = ResolveOverloading csenv trace nm (0,0) AccessibleFromEverywhere calledMethGroup false (Some (rty,dummyExpr))

                  errors ++ (fun () -> 
                      let isInstance = minfo.IsInstance
                      if isInstance <> memFlags.MemberIsInstance then 
                          ErrorD(ConstraintSolverError("The type '"^(NicePrint.pretty_string_of_typ denv minfo.EnclosingType)^"' has a method '"^DecompileOpName nm^"' (full name '"^nm^"'), but the method is"^(if isInstance then " not" else "")^" static",m,m2 ))
                      else 
                          CheckMethInfoAttributes g m minfo ++ (fun () -> 
                          ResultD (TTraitSolved (minfo,calledMeth.CalledTyArgs))))
                          
              | None ->
                      
                  let support =  GetSupportOfMemberConstraint csenv traitInfo
                  let frees =  GetFreeTyparsOfMemberConstraint csenv traitInfo

                  // If there's nothing left to learn then raise the errors 
                  (if (canon && isNil support) || isNil frees then errors  
                  // Otherwise re-record the trait waiting for canonicalization 
                   else AddMemberConstraint csenv ndeep m2 trace traitInfo support frees) ++ (fun () -> ResultD TTraitUnsolved)
    ) 
    ++ 
    (fun res -> RecordMemberConstraintSolution g m trace traitInfo res))


/// Record the solution to a member constraint in the mutable reference cell attached to 
/// each member constraint.
and RecordMemberConstraintSolution g m trace traitInfo res = 
        let transactSolution sln = 
            let prev = traitInfo.Solution 
            traitInfo.Solution <- Some sln
            match trace with 
            | NoTrace -> () 
            | WithTrace (Trace actions) -> actions := (fun () -> traitInfo.Solution <- prev) :: !actions
            
        match res with 
        | TTraitUnsolved -> 
            ResultD false

        | TTraitSolved (minfo,minst) -> 
            let sln = MemberConstraintSolutionOfMethInfo g m minfo minst
            TransactMemberConstraintSolution traitInfo trace sln;
            ResultD true

        | TTraitBuiltIn -> 
            TransactMemberConstraintSolution traitInfo trace BuiltInSln;
            ResultD true

/// Convert a MethInfo into the data we save in the TAST
and MemberConstraintSolutionOfMethInfo g m minfo minst = 
    match minfo with 
    | ILMeth(g,ILMethInfo(ILTypeInfo(tcref,tref,tinst,_),extOpt,mdef,typars)) ->
       let mref = IL.mk_mref_to_mdef (tref,mdef)
       ILMethSln(mk_tyapp_ty tcref tinst,extOpt,mref,minst)
    | FSMeth(g,typ,vref) ->  
       FSMethSln(typ, vref,minst)
    | MethInfo.DefaultStructCtor _ -> 
       error(InternalError("the default struct constructor was the unexpected solution to a trait constraint",m))

/// Write into the reference cell stored in the TAST and add to the undo trace if necessary
and TransactMemberConstraintSolution traitInfo trace sln  =
    let prev = traitInfo.Solution 
    traitInfo.Solution <- Some sln
    match trace with 
    | NoTrace -> () 
    | WithTrace (Trace actions) -> actions := (fun () -> traitInfo.Solution <- prev) :: !actions

/// Only consider overload resolution if canonicalizing or all the types are now nominal. 
/// That is, don't perform resolution if more nominal information may influence the set of available overloads 
and GetRelevantMethodsForTrait (csenv:ConstraintSolverEnv) canon nm (TTrait(tys,_,_,_,_,_) as traitInfo) =
    if canon || isNil (GetSupportOfMemberConstraint csenv traitInfo) then
        let m = csenv.cs_m
        let g = csenv.g
        let minfos = tys |> List.map (GetIntrinsicMethInfosOfType csenv.cs_css.css_InfoReader (Some(nm),AccessibleFromSomeFSharpCode) IgnoreOverrides m) 
        /// Merge the sets so we don't get the same minfo from each side 
        /// We merge based on whether minfos use identical metadata or not. 

        /// REVIEW: Consider the pathological cases where this may cause a loss of distinction 
        /// between potential overloads because a generic instantiation derived from the left hand type differs 
        /// to a generic instantiation for an operator based on the right hand type. 
        
        let minfos = List.fold (ListSet.unionFavourLeft (MethInfosUseIdenticalDefinitions g)) (List.hd minfos) (List.tl minfos)
        minfos
    else 
        []



/// The nominal support of the member constraint 
and GetSupportOfMemberConstraint (csenv:ConstraintSolverEnv) (TTrait(tys,_,_,_,_,_)) =
    tys |> List.choose (fun ty -> if is_typar_typ csenv.g ty then Some (dest_typar_typ csenv.g ty) else  None)
    
/// All the typars relevant to the member constraint *)
and GetFreeTyparsOfMemberConstraint (csenv:ConstraintSolverEnv) (TTrait(tys,nm,memFlags,argtys,rty,_)) =
    (free_in_types_lr_no_cxs csenv.g (tys@argtys@ Option.to_list rty))

/// Re-solve the global constraints involving any of the given type variables. 
/// Trait constraints can't always be solved using the pessimistic rules. As a result we only canonicalize 
/// them forcefully prior to generalization. 
and SolveRelevantMemberConstraints (csenv:ConstraintSolverEnv) ndeep canon trace tps =

    RepeatWhileD
        (fun () -> 
            tps |> AtLeastOneD (fun tp -> 
                /// Normalize the typar 
                let ty = mk_typar_ty tp
                if not (is_typar_typ csenv.g ty) then ResultD false else
                let tp = dest_typar_typ csenv.g ty
                SolveRelevantMemberConstraintsForTypar csenv ndeep canon trace tp))

and SolveRelevantMemberConstraintsForTypar (csenv:ConstraintSolverEnv) ndeep canon trace tp =
    let cxst = csenv.cs_css.css_cxs
    let tpn = tp.Stamp
    let cxs = Hashtbl.find_all  cxst tpn
    if isNil cxs then ResultD false else

    if verbose then dprintf "SolveRelevantMemberConstraintsForTypar #cxs = %d, m = %a\n" (List.length cxs) output_range csenv.cs_m;
    cxs |> List.iter (fun _ -> Hashtbl.remove cxst tpn);

    assert (isNil (Hashtbl.find_all cxst tpn));

    match trace with 
    | NoTrace -> () 
    | WithTrace (Trace actions) -> actions := (fun () -> List.iter (Hashtbl.add cxst tpn) cxs) :: !actions

    cxs |> AtLeastOneD (fun (traitInfo,m2) -> 
        let csenv = { csenv with cs_m = m2 }
        SolveMemberConstraint csenv canon (ndeep+1) m2 trace traitInfo)

and CanonicalizeRelevantMemberConstraints (csenv:ConstraintSolverEnv) ndeep trace tps =
    SolveRelevantMemberConstraints csenv ndeep true trace tps 

  
and AddMemberConstraint (csenv:ConstraintSolverEnv) ndeep m2 trace traitInfo support frees =
    let g = csenv.g
    let m = csenv.cs_m
    let aenv = csenv.cs_aenv
    let cxst = csenv.cs_css.css_cxs

    // Write the constraint into the global table. That is,
    // associate the constraint with each type variable in the free variables of the constraint.
    // This will mean the constraint gets resolved whenever one of these free variables gets solved.
    frees |> List.iter (fun tp -> 
        let tpn = tp.Stamp

        let cxs = Hashtbl.find_all  cxst tpn
        if verbose then dprintf "AddMemberConstraint: tpn = %d, #cxs = %d, m = %a\n" tpn (List.length cxs) output_range csenv.cs_m;
        if verbose && List.length cxs > 10 then 
            cxs |> List.iter (fun (cx,_) -> dprintf "     --> cx = %s, fvs = %s\n" (Layout.showL (traitL cx)) (Layout.showL (TyparsL (GetFreeTyparsOfMemberConstraint csenv cx))));

        // check the constraint is not already listed for this type variable
        if not (cxs |> List.exists (fun (traitInfo2,_) -> traits_aequiv g aenv traitInfo traitInfo2)) then 
            match trace with 
            | NoTrace -> () 
            | WithTrace (Trace actions) -> actions := (fun () -> Hashtbl.remove csenv.cs_css.css_cxs tpn) :: !actions
            Hashtbl.add csenv.cs_css.css_cxs tpn (traitInfo,m2)
    );

    // Associate the constraint with each type variable in the support, so if the type variable
    // gets generalized then this constraint is attached at the binding site.
    support |> IterateD (fun tp -> AddConstraint csenv ndeep m2 trace tp (TTyparMayResolveMemberConstraint(traitInfo,m2)))

    
/// Record a constraint on an inference type variable. 
and AddConstraint (csenv:ConstraintSolverEnv) ndeep m2 trace tp newConstraint  =
    let g = csenv.g
    let aenv = csenv.cs_aenv
    let amap = csenv.amap
    let denv = csenv.cs_denv
    let m = csenv.cs_m


    let consistent tpc1 tpc2 =
        match tpc1,tpc2 with           
        | (TTyparMayResolveMemberConstraint(TTrait(tys1,nm1,memFlags1,argtys1,rty1,_),_),
           TTyparMayResolveMemberConstraint(TTrait(tys2,nm2,memFlags2,argtys2,rty2,_),_))  
              when (memFlags1 = memFlags2 &&
                    nm1 = nm2 &&
                    argtys1.Length = argtys2.Length &&
                    List.lengthsEqAndForall2 (type_equiv g) tys1 tys2) -> 

                  let rty1 = GetFSharpViewOfReturnType g rty1
                  let rty2 = GetFSharpViewOfReturnType g rty2
                  Iterate2D (SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace) argtys1 argtys2 ++ (fun () -> 
                      SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace rty1 rty2 ++ (fun () -> 
                         if verbose then dprintf "\n-------------\nmerged constraint for %s, tp = %s\n---------\n" nm1 (Layout.showL (TyparDeclL tp));
                         CompleteD))
          
        | (TTyparCoercesToType(ty1,_), 
           TTyparCoercesToType(ty2,_)) -> 


              // Record at most one subtype constraint for each head type. 
              // That is, we forbid constraints by both I<string> and I<int>. 
              // This works because the types on the r.h.s. of subtype 
              // constraints are head-types and so any further inferences are equational. 
              let collect ty = 
                  let res = ref [] 
                  IterateEntireHierarchyOfType (fun x -> res := x :: !res) g amap m ty; 
                  List.rev !res
              let parents1 = collect ty1
              let parents2 = collect ty2
              parents1 |> IterateD (fun ty1Parent -> 
                 parents2 |> IterateD (fun ty2Parent ->  
                     if not (HaveSameHeadType g ty1Parent ty2Parent) then CompleteD else
                     SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ty1Parent ty2Parent))

        | (TTyparIsEnum (u1,_),
           TTyparIsEnum (u2,m2)) ->   
            SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace u1 u2
            
        | (TTyparIsDelegate (aty1,bty1,_),
           TTyparIsDelegate (aty2,bty2,m2)) ->   
            SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace aty1 aty2 ++ (fun () -> 
            SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace bty1 bty2)

        | TTyparIsNotNullableValueType _,TTyparIsReferenceType _     
        | TTyparIsReferenceType _,TTyparIsNotNullableValueType _   ->
            ErrorD (Error("The constraints 'struct' and 'not struct' are inconsistent",m))


        | TTyparSupportsNull _,TTyparSupportsNull _  
        | TTyparIsNotNullableValueType _,TTyparIsNotNullableValueType _     
        | TTyparIsReferenceType _,TTyparIsReferenceType _ 
        | TTyparRequiresDefaultConstructor _,TTyparRequiresDefaultConstructor _ 
        | TTyparSimpleChoice (_,_),TTyparSimpleChoice (_,_) -> 
            CompleteD
            
        | _ -> CompleteD

    // See when one constraint implies implies another. 
    // 'a :> ty1  implies 'a :> 'ty2 if the head type name of ty2 (say T2) OccursCheck anywhere in the heirarchy of ty1 
    // If it does occcur, e.g. at instantiation T2<inst2>, then the check above will have enforced that 
    // T2<inst2> = ty2 
    let implies tpc1 tpc2 = 
        match tpc1,tpc2 with           
        | TTyparMayResolveMemberConstraint(trait1,_),
          TTyparMayResolveMemberConstraint(trait2,_) -> 
            traits_aequiv g aenv trait1 trait2
        | TTyparCoercesToType(ty1,_),TTyparCoercesToType(ty2,_) -> 
              ExistsSameHeadTypeInHierarchy g amap m ty1 ty2
        | TTyparIsEnum(u1,_),TTyparIsEnum(u2,_) -> type_equiv g u1 u2
        | TTyparIsDelegate(aty1,bty1,_),TTyparIsDelegate(aty2,bty2,_) -> type_equiv g aty1 aty2 && type_equiv g bty1 bty2 
        | TTyparSupportsNull _,TTyparSupportsNull _  
        | TTyparIsNotNullableValueType _,TTyparIsNotNullableValueType _     
        | TTyparIsReferenceType _,TTyparIsReferenceType _ 
        | TTyparRequiresDefaultConstructor _,TTyparRequiresDefaultConstructor _ -> true
        | TTyparSimpleChoice (tys1,_),TTyparSimpleChoice (tys2,_) -> ListSet.isSubsetOf (type_equiv g) tys1 tys2
        | TTyparDefaultsToType (priority1,dty1,_), TTyparDefaultsToType (priority2,dty2,m) -> 
             (priority1 = priority2) && type_equiv g dty1 dty2
        | _ -> false

        
    
    // First ensure constraint conforms with existing constraints 
    // NOTE: QUADRATIC 
    let existingConstraints = tp.Constraints

    let allCxs = newConstraint :: List.rev existingConstraints
    begin 
        let rec enforceMutualConsistency i cxs = 
            match cxs with 
            | [] ->  CompleteD
            | cx :: rest -> IterateIdxD (fun j cx2 -> if i = j then CompleteD else consistent cx cx2) allCxs ++ (fun () -> enforceMutualConsistency (i+1) rest)

        enforceMutualConsistency 0 allCxs 
    end ++ (fun ()  ->
    
    let impliedByExistingConstraints = existingConstraints |> List.exists (fun tpc2 -> implies tpc2 newConstraint) 
    if verbose then dprintf "  impliedByExistingConstraints?  %b\n" impliedByExistingConstraints;
    
    if not impliedByExistingConstraints && (tp.Rigidity = TyparRigid) then 
            ErrorD (ConstraintSolverMissingConstraint(denv,tp,newConstraint,m,m2)) 

    elif impliedByExistingConstraints then 
        (if verbose && List.length existingConstraints > 10 then 
            dprintf "    (after implied) tp = %s\n" (Layout.showL (TyparDeclL tp));
         CompleteD)

    else
        let newConstraints = 
              // Eliminate any constraints where one constraint implies another 
              // Keep constraints in the left-to-right form according to the order they are asserted. 
              // NOTE: QUADRATIC 
              let rec eliminateRedundant cxs acc = 
                  match cxs with 
                  | [] ->  acc
                  | cx :: rest -> 
                      eliminateRedundant rest (if List.exists (fun cx2 -> implies cx2 cx) acc then acc else (cx::acc))
                  
              eliminateRedundant allCxs []
              

        // Write the constraint into the type variable 
        // Record a entry in the undo trace if one is provided 
        let d = tp.Data
        let orig = d.typar_constraints
        begin match trace with 
        | NoTrace -> () 
        | WithTrace (Trace actions) -> actions := (fun () -> d.typar_constraints <- orig) :: !actions
        end;
        d.typar_constraints <- newConstraints;

        if verbose then dprintf "#newConstraints = %d\n" (List.length newConstraints);
        if verbose && List.length newConstraints > 10 then 
            dprintf "\n----------------------\n    tp = %s\n" (Layout.showL (TyparDeclL tp));

        CompleteD)

and SolveTypSupportsNull (csenv:ConstraintSolverEnv) ndeep m2 trace ty =
    let g = csenv.g
    let m = csenv.cs_m
    let denv = csenv.cs_denv
    begin 
        if is_typar_typ g ty then AddConstraint csenv ndeep m2 trace (dest_typar_typ g ty) (TTyparSupportsNull(m))
        elif TypeSatisfiesNullConstraint g ty then CompleteD
        else ErrorD (ConstraintSolverError(sprintf "The type '%s' does not have 'null' as a proper value" (NicePrint.pretty_string_of_typ denv ty),m,m2))
    end 

and SolveTypIsEnum (csenv:ConstraintSolverEnv) ndeep m2 trace ty underlying =
    trackErrors {
        let g = csenv.g
        let m = csenv.cs_m
        let denv = csenv.cs_denv
        if is_typar_typ g ty then 
            return! AddConstraint csenv ndeep m2 trace (dest_typar_typ g ty) (TTyparIsEnum(underlying,m))
        elif is_enum_typ g ty then 
            do! SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace underlying (GetUnderlyingTypeOfEnumType g ty) 
            return! CompleteD
        else 
            return! ErrorD (ConstraintSolverError(sprintf "The type '%s' is not a .NET enum type" (NicePrint.pretty_string_of_typ denv ty),m,m2))
    }

and SolveTypIsDelegate (csenv:ConstraintSolverEnv) ndeep m2 trace ty aty bty =
    let g = csenv.g
    let m = csenv.cs_m
    let denv = csenv.cs_denv
    if is_typar_typ g ty then 
        AddConstraint csenv ndeep m2 trace (dest_typar_typ g ty) (TTyparIsDelegate(aty,bty,m))
    elif is_delegate_typ g ty then 
        match TryDestStandardDelegateTyp csenv.InfoReader m AccessibleFromSomewhere ty with 
        | Some (tupledArgTy,rty) ->
            SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace aty tupledArgTy ++ (fun () ->
            SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace bty rty ++ (fun () ->
            CompleteD))
        | None ->
            ErrorD (ConstraintSolverError(sprintf "The type '%s' has a non-standard delegate type" (NicePrint.pretty_string_of_typ denv ty),m,m2))
    else ErrorD (ConstraintSolverError(sprintf "The type '%s' is not a .NET delegate type" (NicePrint.pretty_string_of_typ denv ty),m,m2))

and SolveTypIsNonNullableValueType (csenv:ConstraintSolverEnv) ndeep m2 trace ty =
    let g = csenv.g
    let m = csenv.cs_m
    let denv = csenv.cs_denv
    if is_typar_typ g ty then 
        AddConstraint csenv ndeep m2 trace (dest_typar_typ g ty) (TTyparIsNotNullableValueType(m))
    else
        let underlyingTy = strip_tpeqns_and_tcabbrevs_and_measureable g ty
        if is_struct_typ g underlyingTy then
            (* IsValueType = IsValueType + NonNullable *)
            if   tcref_eq g g.system_Nullable_tcref (tcref_of_stripped_typ g underlyingTy) then
                ErrorD (ConstraintSolverError(sprintf "This type parameter may not be instantiated to 'Nullable'. This is a restriction imposed in order to ensure the meaning of 'null' in some .NET languages is not confusing when used in conjunction with 'Nullable' values",m,m))
            else
                CompleteD
        else
            ErrorD (ConstraintSolverError(sprintf "A generic construct requires that the type '%s' is a .NET or F# struct type" (NicePrint.pretty_string_of_typ denv ty),m,m2))

and SolveTypChoice (csenv:ConstraintSolverEnv) ndeep m2 trace ty tys =
    let g = csenv.g
    let m = csenv.cs_m
    let denv = csenv.cs_denv
    if is_typar_typ g ty then AddConstraint csenv ndeep m2 trace (dest_typar_typ g ty) (TTyparSimpleChoice(tys,m)) else 
    match strip_tpeqns_and_tcabbrevs g ty with
    | TType_app (tc2,[ms]) when tc2.IsMeasureableReprTycon ->
        SolveTypEqualsTypKeepAbbrevs csenv ndeep m2 trace ms (TType_measure MeasureOne) 
    | _ ->
        if List.exists (type_equiv g ty) tys then CompleteD
        else ErrorD (ConstraintSolverError(sprintf "The type '%s' is not compatible with any of the types %s, arising from the use of a printf-style format string" (NicePrint.pretty_string_of_typ denv ty) (String.concat "," (List.map (NicePrint.pretty_string_of_typ denv) tys)),m,m2))


and SolveTypIsReferenceType (csenv:ConstraintSolverEnv) ndeep m2 trace ty =
    let g = csenv.g
    let m = csenv.cs_m
    let denv = csenv.cs_denv
    if is_typar_typ g ty then AddConstraint csenv ndeep m2 trace (dest_typar_typ g ty)  (TTyparIsReferenceType(m))
    elif is_ref_typ g ty then CompleteD
    else ErrorD (ConstraintSolverError(sprintf "A generic construct requires that the type '%s' have reference semantics, but it does not, i.e. it is a struct" (NicePrint.pretty_string_of_typ denv ty),m,m))

and SolveTypRequiresDefaultConstructor (csenv:ConstraintSolverEnv) ndeep m2 trace ty =
    let g = csenv.g
    let amap = csenv.amap
    let m = csenv.cs_m
    let denv = csenv.cs_denv
    if is_typar_typ g ty then AddConstraint csenv ndeep m2 trace (dest_typar_typ g ty) (TTyparRequiresDefaultConstructor(m))
    elif is_struct_typ g ty && TypeHasDefaultValue g ty then 
         CompleteD
    elif
        GetIntrinsicConstructorInfosOfType csenv.InfoReader m ty 
        |> List.filter (IsMethInfoAccessible amap m AccessibleFromEverywhere)   
        |> List.exists minfo_is_nullary 
           then 
       if (is_stripped_tyapp_typ g ty && HasAttrib g g.attrib_AbstractClassAttribute (tcref_of_stripped_typ g ty).Attribs) then 
         ErrorD (ConstraintSolverError(sprintf "A generic construct requires that the type '%s' be non-abstract" (NicePrint.pretty_string_of_typ denv ty),m,m2))
       else
         CompleteD
    else 
         ErrorD (ConstraintSolverError(sprintf "A generic construct requires that the type '%s' have a public default constructor" (NicePrint.pretty_string_of_typ denv ty),m,m2))
     

// Parameterized compatibility relation between member signatures.  The real work
// is done by "equateTypes" and "subsumeTypes" and "subsumeArg"
and CanMemberSigsMatchUpToCheck 
      (csenv:ConstraintSolverEnv) 
      permitOptArgs // are we allowed to supply optional and/or "param" arguments?
      equateTypes   // used to equate the formal method instantiation with the actual method instantiation for a generic method
      subsumeTypes  // used to compare the "obj" type 
      (subsumeArg: CalledArg -> CallerArg<_> -> OperationResult<unit>)    // used to compare the arguments for compatibility
      reqdRetTyOpt 
      calledMeth : ImperativeOperationResult =

    let g    = csenv.g
    let amap = csenv.amap
    let m    = csenv.cs_m
    
    let (CalledMeth(minfo,
                    minst,
                    uminst,
                    callerObjArgTys,
                    argSets,
                    (*
                    unnamedCalledArgs,
                    unnamedCallerArgs,
                    paramArrayCalledArgOpt,
                    paramArrayCallerArgs,
                    assignedNamedArgs,
                    *)
                    methodRetTy,
                    assignedNamedProps,
                    pinfoOpt,
                    unassignedNamedItem,
                    attributeAssignedNamedItems,
                    unnamedCalledOptArgs,
                    unnamedCalledOutArgs)) = calledMeth

    // First equate the method instantiation (if any) with the method type parameters 
    if minst.Length <> uminst.Length then ErrorD(Error("type instantiation length mismatch",m)) else
    
    Iterate2D equateTypes minst uminst ++ (fun () -> 

    if not (permitOptArgs or isNil(unnamedCalledOptArgs)) then ErrorD(Error("optional arguments not permitted here",m)) else
    

    let calledObjArgTys = ObjTypesOfMethInfo amap m minfo minst
    
    // Check all the argument types. 

    if calledObjArgTys.Length <> callerObjArgTys.Length then 
        ErrorD(Error (minfo.LogicalName^" is not "^(if (calledObjArgTys.Length <> 0) then "a static" else "an instance")^" member",m))

    else
        Iterate2D subsumeTypes calledObjArgTys callerObjArgTys ++ (fun () -> 
        (calledMeth.ArgSets |> IterateD (fun argSet -> 
            if argSet.UnnamedCalledArgs.Length <> argSet.UnnamedCallerArgs.Length then ErrorD(Error("argument length mismatch",m)) else
            Iterate2D subsumeArg argSet.UnnamedCalledArgs argSet.UnnamedCallerArgs)) ++ (fun () -> 
        (calledMeth.ParamArrayCalledArgOpt |> OptionD (fun calledArg ->
            if is_il_arr1_typ g calledArg.Type then 
                let ety = dest_il_arr1_typ g calledArg.Type
                calledMeth.ParamArrayCallerArgs |> OptionD (IterateD (fun callerArg -> subsumeArg (CalledArg((0,0),false,NotOptional,false,None,ety)) callerArg))
            else
                CompleteD)
        
        ) ++ (fun () -> 
        (calledMeth.ArgSets |> IterateD (fun argSet -> 
            argSet.AssignedNamedArgs |> IterateD (fun (AssignedCalledArg(_,called,caller)) -> subsumeArg called caller)))  ++ (fun () -> 
        (assignedNamedProps |> IterateD (fun (AssignedItemSetter(_,item,caller)) -> 
            let name, calledArgTy = 
                match item with
                | AssignedPropSetter(pinfo,pminfo,pminst) -> 
                    let calledArgTy = List.hd (List.hd (ParamTypesOfMethInfo amap m pminfo pminst))
                    pminfo.LogicalName, calledArgTy

                | AssignedIlFieldSetter(finfo) ->
                    (* Get or set instance IL field *)
                    let calledArgTy = FieldTypeOfILFieldInfo amap m  finfo
                    finfo.FieldName, calledArgTy
                
                | AssignedRecdFieldSetter(rfinfo) ->
                    let calledArgTy = rfinfo.FieldType
                    rfinfo.Name, calledArgTy
            
            subsumeArg (CalledArg((-1,0),false, NotOptional,false,Some(name), calledArgTy)) caller) )) ++ (fun () -> 
        
        // If there is a conflict in the return type up to subsumption then reject the overload. 
        // This lets us use partial type information to resolve overloads such as op_Explicit 
        // Do not take into account return type information for constructors 
        // Take into account tupling up of unfilled out args 
        if minfo.IsConstructor then CompleteD else
        match reqdRetTyOpt with 
        | None -> CompleteD 
        | Some (reqdRetTy,e) -> 
            let methodRetTy = 
                if isNil unnamedCalledOutArgs then methodRetTy else 
                let outArgTys = List.map (fun (CalledArg(i,_,_,_,_,argty)) -> dest_byref_typ g argty) unnamedCalledOutArgs
                if is_unit_typ g methodRetTy then mk_tupled_ty g outArgTys
                else mk_tupled_ty g (methodRetTy :: outArgTys)
            subsumeArg (CalledArg((-1,0),false,NotOptional,false,None,reqdRetTy)) (CallerArg(methodRetTy,m,false,e))) ))))

//-------------------------------------------------------------------------
// Resolve IL overloading. 
// 
// This utilizes the type inference constraint solving engine in undo mode.
//------------------------------------------------------------------------- 


// F# supports two adhoc conversions at method callsites (note C# supports more, though ones 
// such as implicit conversions interact badly with type inference). 
// The first is the use of "(fun x y -> ...)" when  a delegate it expected. This is not part of 
// the ":>" coercion relationship or inference constraint problem as 
// such, but is a special rule applied only to method arguments. 
// 
// The function AdjustCalledArgType detects this case based on types and needs to know that the type being applied 
// is a function type. 
// 
// The other conversion supported is the two ways to pass a value where a byref is expxected. 
// The first (default) is to use a reference cell, and the interioer address is taken automatically 
// The second is an explicit use of the "address-of" operator "&e". Here we detect the second case, 
// and record the presence of the sytnax "&e" in the pre-inferred actual type for the method argument. 
// The function AdjustCalledArgType detects this and refuses to apply the default byref-to-ref transformation. 
//
// The function AdjustCalledArgType also adjusts for optional arguments. 
and AdjustCalledArgType (csenv:ConstraintSolverEnv) (CalledArg(_,_,optArgInfo,isOutArg,_,calledArgTy)) (CallerArg(callerArgTy,m,isOptCallerArg,_)) =
    (* If the called method argument is a byref type, then the caller may provide a byref or ref *)
    let g = csenv.g
    let amap = csenv.amap
    if is_byref_typ g calledArgTy then
        if is_byref_typ g callerArgTy then 
            calledArgTy
        else 
            mk_refcell_ty g (dest_byref_typ g calledArgTy)  
    else 
        // If the called method argument is a delegate type, then the caller may provide a function 
        let calledArgTy = 
            if is_delegate_typ g calledArgTy && is_fun_typ g callerArgTy then 
                let minfo,del_argtys,del_rty,fty = GetSigOfFunctionForDelegate csenv.InfoReader calledArgTy m  AccessibleFromSomeFSharpCode 
                let del_argtys = (if isNil del_argtys then [g.unit_ty] else del_argtys)
                if List.length (fst (strip_fun_typ g callerArgTy)) = List.length del_argtys
                then fty 
                else calledArgTy 
            else calledArgTy

        // Adjust the called argument type to take into account whether the caller's argument is M(?arg=Some(3)) or M(arg=1) 
        // If the called method argument is optional with type Option<T>, then the caller may provide a T, unless their argument is propogating-optional (i.e. isOptCallerArg) 
        let calledArgTy = 
            match optArgInfo with 
            | NotOptional                    -> calledArgTy
            | CalleeSide when not isOptCallerArg && is_option_ty g calledArgTy  -> dest_option_ty g calledArgTy
            | CalleeSide | CallerSide _ -> calledArgTy
        calledArgTy
        

and private FeasiblySubsumesOrConverts (csenv:ConstraintSolverEnv) calledArg (CallerArg(callerArgTy,m,_,_) as callerArg) =
    let calledArgTy = AdjustCalledArgType csenv calledArg callerArg
    if not (type_feasibly_subsumes_type 0 csenv.g csenv.amap m calledArgTy CanCoerce callerArgTy) then ErrorD(Error("The argument types don't match",m)) else
    CompleteD

and private DefinitelyEquiv (csenv:ConstraintSolverEnv) calledArg (CallerArg(callerArgTy,m,_,_) as callerArg) = 
    let calledArgTy = AdjustCalledArgType csenv calledArg callerArg
    if not (type_equiv csenv.g calledArgTy callerArgTy) then ErrorD(Error("The argument types don't match",m)) else
    CompleteD
  
// Assert a subtype constraint, and wrap an ErrorsFromAddingSubsumptionConstraint error around any failure 
// to allow us to report the outer types involved in the constraint 
and private SolveTypSubsumesTypWithReport (csenv:ConstraintSolverEnv) ndeep m trace ty1 ty2 = 
    TryD (fun () -> SolveTypSubsumesTypKeepAbbrevs csenv ndeep m trace ty1 ty2)
         (fun res -> ErrorD (ErrorsFromAddingSubsumptionConstraint(csenv.g,csenv.cs_denv,ty1,ty2,res,m)))

and private solveTypEqualsTypWithReport (csenv:ConstraintSolverEnv) ndeep  m trace ty1 ty2 = 
    TryD (fun () -> SolveTypEqualsTypKeepAbbrevs csenv ndeep m trace ty1 ty2)
         (fun res -> ErrorD (ErrorFromAddingTypeEquation(csenv.g,csenv.cs_denv,ty1,ty2,res,m)))
  
and ArgsMustSubsumeOrConvert 
        (csenv:ConstraintSolverEnv)
        trace
        (CalledArg(_,isParamArrayArg,_,_,_,calledArgTy) as calledArg) 
        (CallerArg(callerArgTy,m,_,_) as callerArg) = 
        
    let g = csenv.g
    let amap = csenv.amap
    let calledArgTy = AdjustCalledArgType csenv calledArg callerArg
    SolveTypSubsumesTypWithReport csenv 0 m trace calledArgTy callerArgTy ++ (fun () -> 

    if isParamArrayArg &&
        is_stripped_tyapp_typ g calledArgTy &&
        (let tcf,tinstf = dest_stripped_tyapp_typ g calledArgTy
        List.length tinstf  = 1 &&
        type_feasibly_equiv 0 g amap m (List.hd tinstf) callerArgTy)
    then 
        ErrorD(Error("This method expects a .NET 'params' parameter in this position. 'params' is a way of passing a variable number of arguments to a method in languages such as C#. Consider passing an array for this argument",m))
    else
        CompleteD)

and MustUnify csenv trace ty1 ty2 = 
    solveTypEqualsTypWithReport csenv 0 csenv.cs_m trace ty1 ty2

and MustUnifyInsideUndo csenv trace ty1 ty2 = 
    solveTypEqualsTypWithReport csenv 0 csenv.cs_m (WithTrace trace) ty1 ty2

and ArgsMustSubsumeOrConvertInsideUndo (csenv:ConstraintSolverEnv) trace calledArg (CallerArg(callerArgTy,m,_,_) as callerArg) = 
    let g = csenv.g
    let amap = csenv.amap
    let calledArgTy = AdjustCalledArgType csenv calledArg callerArg
    SolveTypSubsumesTypWithReport csenv 0 m (WithTrace trace) calledArgTy callerArgTy 

and TypesMustSubsumeOrConvertInsideUndo (csenv:ConstraintSolverEnv) trace m calledArgTy callerArgTy = 
    SolveTypSubsumesTypWithReport csenv 0 m trace calledArgTy callerArgTy 

and ArgsEquivInsideUndo (csenv:ConstraintSolverEnv) trace calledArg (CallerArg(callerArgTy,m,_,_) as callerArg) = 
    let g = csenv.g
    let amap = csenv.amap
    let calledArgTy = AdjustCalledArgType csenv calledArg callerArg
    if not (type_equiv csenv.g calledArgTy callerArgTy) then ErrorD(Error("The argument types don't match",m)) else
    CompleteD

and ReportNoCandidatesError (csenv:ConstraintSolverEnv) (nUnnamedCallerArgs,nNamedCallerArgs) methodName ad (calledMethGroup:CalledMeth<_> list) =

    let g    = csenv.g
    let amap = csenv.amap
    let m    = csenv.cs_m
    let denv = csenv.cs_denv

    match (calledMethGroup |> List.partition (CalledMeth.GetMethod >> IsMethInfoAccessible amap m ad)),
          (calledMethGroup |> List.partition (fun cmeth -> cmeth.HasCorrectObjArgs(amap,m,ad))),
          (calledMethGroup |> List.partition (fun cmeth -> cmeth.HasCorrectArity)),
          (calledMethGroup |> List.partition (fun cmeth -> cmeth.HasCorrectGenericArity)),
          (calledMethGroup  |> List.partition (fun cmeth -> cmeth.AssignsAllNamedArgs)) with

    // No version accessible 
    | ([],others),_,_,_,_ ->  
        ErrorD (Error ("The member or object constructor '"^methodName^"' is not "^showAccessDomain ad ^
                       (if nonNil others then ". Private members may only be accessed from within the declaring type. Protected members may only be accessed from an extending type and may not be accessed from inner lambda expressions" else ""), m))
    | _,([],(cmeth::_)),_,_,_ ->  
    
        // Check all the argument types. 

        ErrorD(Error (methodName^" is not "^(if (cmeth.CalledObjArgTys(amap,m).Length <> 0) then "a static" else "an instance")^" method",m))

    // One method, incorrect name/arg assignment 
    | _,_,_,_,([],[cmeth]) -> 
        let msg = 
            List.foldBack 
               (fun (CallerNamedArg(id,CallerArg(_,m,_,_))) acc -> "The member or object constructor '"^methodName^"' has no argument or settable return property '"^id.idText^"'. "^acc)
               cmeth.UnassignedNamedArgs
               ("The required signature is "^string_of_minfo amap m denv cmeth.Method)
        ErrorD (Error (msg,m))

    // One method, incorrect number of arguments provided by the user
    | _,_,([],[cmeth]),_,_ when not cmeth.HasCorrectArity ->  
        let minfo = cmeth.Method
        let nReqd = cmeth.TotalNumUnnamedCalledArgs
        let nReqdNamed = cmeth.TotalNumAssignedNamedArgs
        let nActual = cmeth.TotalNumUnnamedCallerArgs
        let nreqdTyArgs = cmeth.NumCalledTyArgs
        let nactualTyArgs = cmeth.NumCallerTyArgs
        if nActual <> nReqd then 
            if nReqdNamed > 0 or cmeth.NumAssignedProps > 0 then 
                if nReqd > nActual then 
                    let furtherText = if nActual = 0 then "" else " additional"
                    let nameText = 
                        if nReqd > nActual then 
                            let missingArgs = List.drop nReqd cmeth.AllUnnamedCalledArgs
                            match NamesOfCalledArgs missingArgs with 
                            | [] -> ""
                            | names -> ". Some names for missing arguments are "^String.concat ";" names
                        else ""
                    ErrorD (Error (sprintf "The member or object constructor '%s' requires %d%s argument(s). The required signature is '%s'%s" methodName (nReqd-nActual) furtherText (string_of_minfo amap m denv minfo) nameText, m))
                else 
                    ErrorD (Error (sprintf "The member or object constructor '%s' requires %d argument(s) but is here given %d unnamed and %d named argument(s). The required signature is '%s'" methodName (nReqd+nReqdNamed) nActual nReqdNamed (string_of_minfo amap m denv minfo), m))
            else
                ErrorD (Error (sprintf "The member or object constructor '%s' takes %d argument(s) but is here given %d. The required signature is '%s'" methodName nReqd nActual (string_of_minfo amap m denv minfo), m))
        else 
            ErrorD (Error (sprintf "The member or object constructor '%s' takes %d type argument(s) but is here given %d. The required signature is '%s'" methodName nreqdTyArgs nactualTyArgs (string_of_minfo amap m denv minfo), m))

    // One or more accessible, all the same arity, none correct 
    | ((cmeth :: cmeths2),_),_,_,_,_ when not cmeth.HasCorrectArity && cmeths2 |> List.forall (fun cmeth2 -> cmeth.TotalNumUnnamedCalledArgs = cmeth2.TotalNumUnnamedCalledArgs) -> 
        ErrorD (Error (sprintf "The member or object constructor '%s' taking %d arguments is not accessible from this code location. All accessible versions of method '%s' take %d arguments" methodName (cmeth.ArgSets |> List.sumBy (fun x -> x.NumUnnamedCalledArgs)) methodName cmeth.TotalNumUnnamedCalledArgs,m))

    // Many methods, all with incorrect number of generic arguments
    | _,_,_,([],(cmeth :: _)),_ -> 
        let msg = sprintf "Incorrect generic instantiation. No %s member named '%s' takes %d generic arguments" (showAccessDomain ad) methodName cmeth.NumCallerTyArgs
        ErrorD (Error (msg,m))
    // Many methods of different arities, all incorrect 
    | _,_,([],(cmeth :: _)),_,_ -> 
        let minfo = cmeth.Method
        ErrorD (Error (sprintf "The member or object constructor '%s' does not take %d argument(s). An overload was found taking %d arguments" methodName cmeth.TotalNumUnnamedCallerArgs (List.sum minfo.NumArgs),m))
    | _ -> 
        let msg = sprintf "No %s member or object constructor named '%s' takes %d arguments" (showAccessDomain ad) methodName (nUnnamedCallerArgs)
        let msg = 
            if nNamedCallerArgs = 0 then 
                msg 
            else 
                let s = calledMethGroup |> List.map (fun cmeth -> cmeth.UnassignedNamedArgs |> List.map (fun na -> na.Name)|> Set.of_list) |> Set.intersect_all
                if s.IsEmpty then 
                     msg + sprintf ". Note the call to this member also provides %d named arguments" nNamedCallerArgs
                else 
                    let sample = s.MinimumElement
                    msg + sprintf ". The named argument '%s' doesn't correspond to any argument or settable return property for any overload" sample
        ErrorD (Error (msg,m))


// Resolve the overloading of a method 
// This is used after analyzing the types of arguments 
and ResolveOverloading 
         (csenv:ConstraintSolverEnv) 
         trace           // The undo trace, if any
         methodName      // The name of the method being called, for error reporting
         callerArgCounts // How many named/unnamed args id the caller provide? 
         ad              // The access domain of the caller, e.g. a module, type etc. 
         calledMethGroup // The set of methods being called 
         permitOptArgs   // Can we supply optional arguments?
         reqdRetTyOpt    // The expected return type, if known 
     =
    let g = csenv.g
    let amap = csenv.amap
    let m    = csenv.cs_m
    let denv = csenv.cs_denv
    // See what candidates we have based on name and arity 
    let candidates = calledMethGroup |> List.filter (fun cmeth -> cmeth.IsCandidate(g,amap,m,ad))
    let calledMethOpt, errors = 

        match calledMethGroup,candidates with 
        | _,[calledMeth] -> 
            Some(calledMeth), CompleteD

        | [],_ -> 
            None, ErrorD (Error (sprintf "Method or object constructor '%s' not found" methodName,m))

        | _,[] -> 
            None, ReportNoCandidatesError csenv callerArgCounts methodName ad calledMethGroup
            
        | _,_ -> 

          // See what candidates we have based on current inferred type information and exact matches of argument types 
          // Return type deliberately not take into account 
          match candidates |> FilterEachThenUndo (fun newTrace calledMeth -> 
                     CanMemberSigsMatchUpToCheck 
                         csenv 
                         permitOptArgs 
                         (MustUnifyInsideUndo csenv newTrace) 
                         (TypesMustSubsumeOrConvertInsideUndo csenv (WithTrace newTrace) m)
                         (ArgsEquivInsideUndo csenv newTrace) 
                         None 
                         calledMeth) with
          | [(calledMeth,_)] -> 
              Some(calledMeth), CompleteD

          | _ -> 
            let applicable = candidates |> FilterEachThenUndo (fun newTrace candidate -> 
                               CanMemberSigsMatchUpToCheck 
                                   csenv 
                                   permitOptArgs
                                   (MustUnifyInsideUndo csenv newTrace) 
                                   (TypesMustSubsumeOrConvertInsideUndo csenv (WithTrace newTrace) m)
                                   (ArgsMustSubsumeOrConvertInsideUndo csenv newTrace) 
                                   reqdRetTyOpt 
                                   candidate) 
            match applicable with 
            | [] ->
                // OK, we failed. Collect up the errors from overload resolution and the possible overloads
                let errors = 
                    (candidates |> List.choose (fun calledMeth -> 
                            match CollectThenUndo (fun newTrace -> 
                                         CanMemberSigsMatchUpToCheck 
                                             csenv 
                                             permitOptArgs
                                             (MustUnifyInsideUndo csenv newTrace) 
                                             (TypesMustSubsumeOrConvertInsideUndo csenv (WithTrace newTrace) m)
                                             (ArgsMustSubsumeOrConvertInsideUndo csenv newTrace) 
                                             reqdRetTyOpt 
                                             calledMeth) with 
                            | OkResult _ -> None
                            | ErrorResult(_,exn) -> Some exn))
                                                
                let overloads = GetPossibleOverloads amap m denv calledMethGroup

                None,ErrorD (UnresolvedOverloading (denv,overloads,[],errors,"No overloads match for method "^methodName^". Possible matches are shown below (or in the Error List window)",m))
            | [(calledMeth,_)] -> 
                Some(calledMeth), CompleteD
            | applicableMeths -> 
                
                let better (candidate:CalledMeth<_>, candidateWarnCount) (other:CalledMeth<_>, otherWarnCount) = 
                    // prefer methods that don't give "this code is less generic" warnings
                    (candidateWarnCount = 0 || otherWarnCount > 0) &&
                    
                    // prefer methods that don't use param array arg, or with more precise param array arg
                    (not candidate.UsesParamArrayConversion ||
                     (other.UsesParamArrayConversion &&
                          type_feasibly_subsumes_type 0 csenv.g csenv.amap m (other.ParamArrayElementType(g)) CanCoerce (other.ParamArrayElementType(g)))) &&
                    
                    // prefer methods that don't use out args
                    (not candidate.HasOutArgs || other.HasOutArgs) &&

                    // prefer methods that don't use optional args
                    (not candidate.HasOptArgs || other.HasOptArgs) &&

                    // prefer non-generic methods 
                    (candidate.CalledTyArgs.IsEmpty || not other.CalledTyArgs.IsEmpty) &&
                    
                    // check regular args. The argument counts will only be different if one is using param args
                    (candidate.TotalNumUnnamedCalledArgs <> other.TotalNumUnnamedCalledArgs ||
                     // all args are at least as good
                     (candidate.AllUnnamedCalledArgs, other.AllUnnamedCalledArgs) ||> List.forall2 (fun (CalledArg(pos1,isParamArray1,optArgInfo1,isOutArg1,nmOpt1,argType1)) (CalledArg(pos2,isParamArray2,optArgInfo2,isOutArg2,nmOpt2,argType2)) -> 
                          type_feasibly_subsumes_type 0 csenv.g csenv.amap m argType2 CanCoerce argType1))

                let bestMethods =
                    applicableMeths |> List.choose (fun candidate -> 
                        if applicableMeths |> List.forall (fun other -> 
                             candidate === other || 
                             let res = better candidate other
                             //eprintfn "\n-------\nCandidate: %s\nOther: %s\nResult: %b\n" (string_of_minfo amap m denv candidate.Method) (string_of_minfo amap m denv other.Method) res
                             res) then 
                           Some(candidate)
                        else 
                           None) 
                match bestMethods with 
                | [(calledMeth,_)] -> Some(calledMeth), CompleteD
                | bestMethods -> 
                    let overloads = GetPossibleOverloads  amap m denv calledMethGroup
                    //let bestOverloads = GetPossibleBestOverloads  amap m denv bestMethods
                    
                    None, 
                    ErrorD (UnresolvedOverloading (denv,overloads,(* bestOverloads *) [] ,[],"The method '"^methodName^"' is overloaded. Possible matches are shown below (or in the Error List window)",m));
                            
    // If we've got a candidate solution: make the final checks - no undo here! 
    match calledMethOpt with 
    | Some(calledMeth) -> 
        calledMethOpt,
        errors ++ (fun () -> CanMemberSigsMatchUpToCheck 
                                 csenv 
                                 permitOptArgs
                                 (MustUnify csenv trace) 
                                 (TypesMustSubsumeOrConvertInsideUndo csenv trace m)// REVIEW: this should not be an "InsideUndo" operation
                                 (ArgsMustSubsumeOrConvert csenv trace) 
                                 reqdRetTyOpt 
                                 calledMeth)

    | None -> 
        None, errors        


/// This is used before analyzing the types of arguments in a single overload resolution
let UnifyUniqueOverloading (csenv:ConstraintSolverEnv) callerArgCounts methodName ad (calledMethGroup:CalledMeth<_> list) =
    let g = csenv.g
    let amap = csenv.amap
    let m    = csenv.cs_m
    let denv = csenv.cs_denv
    if verbose then  dprintf "--> UnifyUniqueOverloading@%a\n" output_range m;
    (* See what candidates we have based on name and arity *)
    let candidates = calledMethGroup |> List.filter (fun cmeth -> cmeth.IsCandidate(g,amap,m,ad)) 
    if verbose then  dprintf "in UnifyUniqueOverloading@%a\n" output_range m;
    match calledMethGroup,candidates with 
    | _,[calledMeth] -> 
        (* Only one candidate found - we thus know the types we expect of arguments *)
        CanMemberSigsMatchUpToCheck 
            csenv true 
            (MustUnify csenv NoTrace) 
            (TypesMustSubsumeOrConvertInsideUndo csenv NoTrace m)
            (ArgsMustSubsumeOrConvert csenv NoTrace) 
            None 
            calledMeth
        ++ (fun () -> ResultD(true))
        
    | [],_ -> 
        ErrorD (Error ("Method or object constructor '"^methodName^"' not found",m))
    | _,[] -> 
        ReportNoCandidatesError csenv callerArgCounts methodName ad calledMethGroup 
        ++ (fun () -> ResultD(false))
    | _ -> 
        ResultD(false)

let EliminateConstraintsForGeneralizedTypars csenv trace (generalizedTypars:typars) =
    // Resolve the global constraints where this type variable appears in the support of the constraint 
    generalizedTypars |> List.iter (fun tp -> 
        let tpn = tp.Stamp
        let cxst = csenv.cs_css.css_cxs
        let cxs = Hashtbl.find_all  cxst tpn
        if isNil cxs then () else
        if verbose then dprintf "EliminateConstraintsForGeneralizedTypars: #cxs = %d, m = %a\n" (List.length cxs) output_range csenv.cs_m;
        cxs |> List.iter (fun cx -> 
            Hashtbl.remove cxst tpn;
            match trace with 
            | NoTrace -> () 
            | WithTrace (Trace actions) -> actions := (fun () -> (Hashtbl.add csenv.cs_css.css_cxs tpn cx)) :: !actions)
    )


//-------------------------------------------------------------------------
// Main entry points to constraint solver (some backdoors are used for 
// some constructs)
//
// No error recovery here : we do that on a per-expression basis.
//------------------------------------------------------------------------- 

let AddCxTypeEqualsType denv css m ty1 ty2 = 
    solveTypEqualsTypWithReport (MakeConstraintSolverEnv css m denv) 0 m NoTrace ty1 ty2
    |> RaiseOperationResult

let UndoIfFailed f =
    let trace = newTrace()
    let res = 
        try f trace |> CheckNoErrorsAndGetWarnings
        with e -> None
    match res with 
    | None -> 
        // Don't report warnings if we failed
        undoTrace trace; false
    | Some warns -> 
        // Report warnings if we succeeded
        ReportWarnings warns; true

let AddCxTypeEqualsTypeUndoIfFailed denv css m ty1 ty2 =
    UndoIfFailed (fun trace -> SolveTypEqualsTypKeepAbbrevs (MakeConstraintSolverEnv css m denv) 0 m (WithTrace(trace)) ty1 ty2)

let AddCxTypeMustSubsumeTypeUndoIfFailed denv css m ty1 ty2 = 
    UndoIfFailed (fun trace -> SolveTypSubsumesTypKeepAbbrevs (MakeConstraintSolverEnv css m denv) 0 m (WithTrace(trace)) ty1 ty2)

let AddCxTypeMustSubsumeType denv css m trace ty1 ty2 = 
    SolveTypSubsumesTypWithReport (MakeConstraintSolverEnv css m denv) 0 m trace ty1 ty2
    |> RaiseOperationResult

let AddCxMethodConstraint denv css m trace traitInfo  =
    TryD (fun () -> SolveMemberConstraint (MakeConstraintSolverEnv css m denv) false 0 m trace traitInfo ++ (fun _ -> CompleteD))
         (fun res -> ErrorD (ErrorFromAddingConstraint(denv,res,m)))
    |> RaiseOperationResult

let AddCxTypeMustSupportNull denv css m trace ty =
    TryD (fun () -> SolveTypSupportsNull (MakeConstraintSolverEnv css m denv) 0 m trace ty)
         (fun res -> ErrorD (ErrorFromAddingConstraint(denv,res,m)))
    |> RaiseOperationResult

let AddCxTypeMustSupportDefaultCtor denv css m trace ty =
    TryD (fun () -> SolveTypRequiresDefaultConstructor (MakeConstraintSolverEnv css m denv) 0 m trace ty)
         (fun res -> ErrorD (ErrorFromAddingConstraint(denv,res,m)))
    |> RaiseOperationResult

let AddCxTypeIsReferenceType denv css m trace ty =
    TryD (fun () -> SolveTypIsReferenceType (MakeConstraintSolverEnv css m denv) 0 m trace ty)
         (fun res -> ErrorD (ErrorFromAddingConstraint(denv,res,m)))
    |> RaiseOperationResult

let AddCxTypeIsValueType denv css m trace ty =
    TryD (fun () -> SolveTypIsNonNullableValueType (MakeConstraintSolverEnv css m denv) 0 m trace ty)
         (fun res -> ErrorD (ErrorFromAddingConstraint(denv,res,m)))
    |> RaiseOperationResult

let AddCxTypeIsEnum denv css m trace ty underlying =
    TryD (fun () -> SolveTypIsEnum (MakeConstraintSolverEnv css m denv) 0 m trace ty underlying)
         (fun res -> ErrorD (ErrorFromAddingConstraint(denv,res,m)))
    |> RaiseOperationResult

let AddCxTypeIsDelegate denv css m trace ty aty bty =
    TryD (fun () -> SolveTypIsDelegate (MakeConstraintSolverEnv css m denv) 0 m trace ty aty bty)
         (fun res -> ErrorD (ErrorFromAddingConstraint(denv,res,m)))
    |> RaiseOperationResult

let CodegenWitnessThatTypSupportsTraitConstraint g amap m (traitInfo:TraitConstraintInfo) = 
    let css = {css_g=g;css_amap=amap;css_cxs=Hashtbl.create 10;css_InfoReader=new InfoReader(g,amap) }
    let csenv = MakeConstraintSolverEnv css m (empty_denv g)
    SolveMemberConstraint csenv true 0 m NoTrace traitInfo ++ (fun res -> 
       if res then 
          match traitInfo.Solution with 
          | None -> ResultD None
          | Some sln ->
              match sln with 
              | ILMethSln(typ,extOpt,mref,minst) ->
                   let tcref,tinst = dest_stripped_tyapp_typ g typ
                   let scoref,enc,tdef = tcref.ILTyconInfo
                   let mdef = IL.resolve_mref tdef mref
                   let tref = IL.tref_for_nested_tdef scoref (enc,tdef)
                   let mtps = Import.ImportIlTypars (fun () -> amap) m scoref tinst mdef.mdGenericParams
                   ResultD (Some (ILMeth(g,ILMethInfo(ILTypeInfo(tcref,tref,tinst,tdef),extOpt,mdef,mtps)),minst))
              | FSMethSln(typ, vref,minst) ->
                   ResultD (Some (FSMeth(g,typ,vref),minst))
              | BuiltInSln -> 
                   ResultD  None
       else 
           ResultD None) 

       //| TTraitUnsolved -> ResultD None //ErrorD(InternalError("unsolved trait constraint in codegen",m))
       //| TTraitBuiltIn -> ResultD  None //ErrorD(InternalError("trait constraint was resolved to F# library intrinsic in codegen",m))


let ChooseTyparSolutionAndSolve css denv tp =
    let g = css.css_g
    let amap = css.css_amap
    let max,m = choose_typar_solution_and_range g amap tp 
    let csenv = (MakeConstraintSolverEnv css m denv)
    TryD (fun () -> SolveTyparEqualsTyp csenv 0 m NoTrace (mk_typar_ty tp) max)
         (fun err -> ErrorD(ErrorFromApplyingDefault(g,denv,tp,max,err,m)))
    |> RaiseOperationResult



