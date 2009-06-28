// (c) Microsoft Corporation. All rights reserved
#light

module (* internal *) Microsoft.FSharp.Compiler.Detuple 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 
open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.PrettyNaming
open Microsoft.FSharp.Compiler.Lib

//
// This pass has one aim.
// - to eliminate tuples allocated at call sites (due to uncurried style)
//
// After PASS, 
//   Private, non-top-level functions fOrig which had explicit tuples at all callsites,
//   have been replaced by transformedVal taking the individual tuple fields,
//   subject to the type of the fOrig formal permitting the split.
// 
// The decisions are based on call site analysis 
//
//----------
// TUPLE COLLAPSE SIMPLIFIED.
//
// The aim of the optimization pass implemented in this module
// is to eliminate (redundant) tuple allocs arising due to calls.
// These typically arise from code written in uncurried form.
//
// Note that "top-level" functions and methods are automatically detupled in F#,
// by choice of representation. So this only applies to inner functions, and even
// then only to those not given "TLR" representation through lambda-lifting.
//
// Q: When is a tuple allocation at callsite redundant?
// A1: If the function called only wants the fields of the tuple.
// A2: If all call sites allocate a tuple argument,
//     then can factor that tuple creation into the function,
//     and hope the optimiser will eliminate it if possible.
//     e.g. if only the fields are required.
//
// The COLLAPSE transform is based on answer A2...
//
//   [[ let rec fOrig p = ... fOrig (a,b) ...
//      fOrig (x,y) ]]
//   ->
//      let rec transformedVal p1 p2 = let p = p1,p2
//                        ... (transformedVal a b) ...
//     
//      transformedVal x y
//
// Q: What about cases where some calls to fOrig provide just a tuple?
// A: If fOrig requires the original tuple argument, then this transform
//    would insert a tuple allocation inside fOrig, where none was before...
//
//----------
// IMPLEMENTATION OVERVIEW.
//
// 1. Require call-pattern info about callsites of each function, e.g.
//
//      [ (_,_) ; (_,(_,_,_)) ; _ ]
//      [ (_,_) ; (_,_)       ]
//      [ (_,_) ]
//
//    Detailing the number of arguments applied and their explicit tuple structure.
//
//    ASIDE: Efficiency note.
//           The rw pass does not change the call-pattern info,
//           so call-pattern info can be collected for all ids in pre-pass.
//
// 2. Given the above, can *CHOOSE* a call-pattern for the transformed function.
//    Informally,
//      Collapse any tuple structure if it is known at ALL call sites.
//    Formally,
//      - n = max List.length of call-pattern args.
//      - extend call patterns to List.length n with _ (no tuple info known)
//      - component-wise intersect argument tuple-structures over call patterns.
//      - gives least known call-pattern of List.length n.
//      - can trim to minimum non-trivual List.length.
//
//    [Used to] have INVARIANT on this chosen call pattern:
//
//      Have: For each argi with non-trivial tuple-structure,
//            at every call have an explicit tuple argument,
//            with (at least) that structure.
//            ----
//            Note, missing args in partial application will always
//            have trivial tuple structure in chosen call-pattern.
//
//    [PS: now defn arg projection info can override call site info]
//
// 2b.Choosing CallPattern also needs to check type of formals for the function.
//    If function is not expecting a tuple (accoring to types) do not split them.
//
// 3. Given CallPattern for selected fOrig,
//    (a) Can choose replacement formals, ybi where needed. (b, bar, means vector of formals).
//
//     cpi                | xi    | ybi
//    --------------------|-------|----------
//     UnknownTS          | xi    | SameArg xi
//     TupleTS []         | []    | SameArg []     // unit case, special case for now.
//     TupleTS ts1...tsN  | xi    | NewArgs (List.collect createFringeFormals [ts1..tsN])
//
//    (b) Can define transformedVal replacement function id.
//
// 4. Fixup defn bindings.
//
//    [[DEFN: fOrig  = LAM tps. lam x1 ...xp xq...xN. body ]]
//    ->
//           transformedVal = LAM tps. lam [[FORMALS: yb1...ybp]] xq...xN. [[REBINDS x1,yb1 ... xp,ybp]] [[FIX: body]]
//
//    [[FORMAL: SameArg xi]] -> xi
//    [[FORMAL: NewArgs vs]] -> [ [v1] ... [vN] ]                // list up individual args for TExpr_lambda
//
//    [[REBIND: xi , SameArg xi]] -> // no binding needed
//    [[REBIND: [u], NewArgs vs]] -> u = "rebuildTuple(cpi,vs)"
//    [[REBIND: us , NewArgs vs]] -> "rebuildTuple(cpi,vs)" then bind us to BuildProjections. // for TExpr_lambda
//
//    rebuildTuple - create tuple based on vs fringe according to cpi tuple structure.
//
//    Note, fixup body...
//
// 5. Fixup callsites.
//
//    [[FIXCALL: APP fOrig tps args]] -> when fOrig is transformed, APP fOrig tps [[collapse args wrt cpf]]
//                                   otherwise, unchanged,  APP fOrig tps args.
//
// 6. Overview.
//    - pre-pass to find callPatterns.
//    - choose CallPattern (tuple allocs on all callsites)
//    - create replacement formals and transformedVal where needed.
//    - rw pass over expr - fixing defns and applications as required.
//    - sanity checks and done.

// Note:  ids can occur in several ways in expr at this point in compiler.
//      val id                                        - freely
//      app (val id) tys args                         - applied to tys/args (if no args, then free occurance)
//      app (reclink (val id)) tys args               - applied (recursive case)
//      app (reclink (app (val id) tys' []) tys args  - applied (recursive type instanced case)
// So, taking care counting callpatterns.
//
// Note: now considering defn projection requirements in decision.
//       no longer can assume that all call sites have explicit tuples if collapsing.
//       in these new cases, take care to have let binding sequence (eval order...)
 

// Merge a tyapp node and and app node.
let (|TyappAndApp|_|) e = 
    match e with 
    | TExpr_app (f,fty,tys,args,m)       -> 
        match strip_expr f with
        | TExpr_app(f2,fty2,tys2,[],m2) -> Some(f2,fty2,tys2 @ tys,args,m2)
        | TExpr_app(f2,fty2,tys2,_,_)   -> Some(f,fty,tys,args,m) (* has args, so not combine ty args *)
        | f                             -> Some(f,fty,tys,args,m)
    | _ -> None
(*-------------------------------------------------------------------------
 *INDEX: GetValsBoundInExpr
 *-------------------------------------------------------------------------*)

module GlobalUsageAnalysis = begin
    let bindAccBounds vals (isInDTree,v) =  Zset.add v vals

    let GetValsBoundInExpr expr =
       let folder = {ExprFolder0 with valBindingSiteIntercept = bindAccBounds}
       let z0 = Zset.empty val_spec_order
       let z  = FoldExpr folder z0 expr
       z


    (*-------------------------------------------------------------------------
     *INDEX: xinfo - state and ops
     *-------------------------------------------------------------------------*)

    type accessor = PTup of int * typ list

    type xinfo =
      (* Expr information.
       * For each v,
       *   (a) log it's usage site context = accessors // APP type-inst args
       *       where first accessor in list applies first to the v/app.
       *   (b) log it's binding site representation.
       *------
       * Future, could generalise to be graph representation of expr. (partly there).
       * This type used to be called "usage".
       *)
       { xinfo_uses     : Zmap.map<Val,(accessor list * typ list * expr list) list>; (* v -> context / APP inst args *)
         xinfo_eqns     : Zmap.map<Val,expr>;                                        (* v -> binding repr *)
         xinfo_dtree    : Zset.set<Val>;                                              (* bound in a decision tree? *)
         xinfo_mubinds  : Zmap.map<Val,bool * FlatVals>;                        (* v -> v list * recursive? -- the others in the mutual binding *)
         xinfo_toplevel : Zset.set<Val>;
         xinfo_top      : bool
       }

    let z0 =
       { xinfo_uses     = Zmap.empty val_spec_order;
         xinfo_eqns     = Zmap.empty val_spec_order;
         xinfo_mubinds  = Zmap.empty val_spec_order;
         xinfo_dtree    = Zset.empty val_spec_order;
         xinfo_toplevel = Zset.empty val_spec_order;
         xinfo_top      = true
       }

    // Note: this routine is called very frequently
    let logUse (f:Val) tup z =
       {z with xinfo_uses = 
            match Zmap.tryfind f z.xinfo_uses with
            | Some sites -> Zmap.add f (tup::sites) z.xinfo_uses
            | None    -> Zmap.add f [tup] z.xinfo_uses }

    let logBinding z (isInDTree,v) =
        let z = if isInDTree then {z with xinfo_dtree = Zset.add v z.xinfo_dtree} else z
        let z = if z.xinfo_top then {z with xinfo_toplevel = Zset.add v z.xinfo_toplevel} else z
        z
        

    let logNonRecBinding z (bind:Binding) =
        (* log mubind v -> vs *)
        let v = var_of_bind bind
        let vs = FlatList.one v
        {z with xinfo_mubinds = Zmap.add v (false,vs) z.xinfo_mubinds;
                xinfo_eqns = Zmap.add v bind.Expr z.xinfo_eqns } 

    let logRecBindings z binds =
        (* log mubind v -> vs *)
        let vs = vars_of_Bindings binds
        {z with xinfo_mubinds = (z.xinfo_mubinds,vs) ||> FlatList.fold (fun mubinds v -> Zmap.add v (true,vs) mubinds);
                xinfo_eqns    = (z.xinfo_eqns,binds) ||> FlatList.fold (fun eqns bind -> Zmap.add bind.Var bind.Expr eqns)  } 

    let foldUnderLambda f z x =
        let saved = z.xinfo_top
        let z = {z with xinfo_top=false}
        let z = f z x
        let z = {z with xinfo_top=saved}
        z

#if DEBUG
    let dumpXInfo z =
        let soAccessor (PTup (n,ts)) = "#" ^ string n
        let dumpSite v (accessors,inst,args) =
            dprintf "- use %s%s %s %s\n"
              (showL (valL v))
              (match inst with
                [] -> ""
              | _  -> "@[" ^ showL (commaListL (List.map typeL inst)) ^ "]")
              (showL (spaceListL (List.map ExprL args)))
              (match accessors with
                [] -> ""
              | _  -> "|> " ^ String.concat " " (List.map soAccessor accessors))
        let dumpUse v sites = List.iter (dumpSite v) sites
        let dumpTop (v:Val) = dprintf "- toplevel: %s\n" v.MangledName
        if false then
         ( dprintf "usage:\n";
           Zmap.iter dumpUse z.xinfo_uses;
           Zset.iter dumpTop z.xinfo_toplevel
          )
        else
         ()
#endif


    (*-------------------------------------------------------------------------
     *INDEX: xinfo - FoldExpr, foldBind collectors
     *-------------------------------------------------------------------------*)

    let UsageFolders g =
      // Fold expr, intercepts selected exprs.
      //   "val v"        - count []     callpattern of v
      //   "app (f,args)" - count <args> callpattern of f
      //---
      // On intercepted nodes, must continue exprF fold over any subexpressions, e.g. args.
      //------
      // Also, noting top-level bindings,
      // so must cancel top-level "foldUnderLambda" whenever step under loop/lambda:
      //   - lambdas
      //   - try_catch + try_finally
      //   - for body
      //   - match targets
      //   - tmethods
      let foldLocalVal f z vref = 
          if vref_in_this_assembly g.compilingFslib vref then f z (deref_val vref)
          else z
      let exprUsageIntercept exprF z expr =
          let exprsF z xs = List.fold exprF z xs
          let rec recognise context expr = 
            match expr with
             | TExpr_val (v,_,m)                  -> 
                 (* YES: count free occurance *)
                 let z = foldLocalVal (fun z v -> logUse v (context,[],[]) z) z v
                 Some z
             | TyappAndApp(f,fty,tys,args,m)       -> 
                 match f with
                  | TExpr_val (fOrig,_,_) ->
                    // app where function is val 
                    // YES: count instance/app (app when have term args), and then
                    //      collect from args (have intercepted this node) 
                    let collect z f = logUse f (context,tys,args) z
                    let z = foldLocalVal collect z fOrig
                    let z = List.fold exprF z args
                    Some z
                  | _ ->
                     (* NO: app but function is not val *)
                     None
             | TExpr_op(TOp_tuple_field_get (n),ts,[x],m)   -> 
                 let context = PTup (n,ts) :: context
                 recognise context x
                 
             // lambdas end top-level status 
             | TExpr_lambda(id,basevopt,vs,body,m,rty,_)   -> 
                 let z = foldUnderLambda exprF z body
                 Some z
             | TExpr_tlambda(id,tps,body,m,rty,_) -> 
                 let z = foldUnderLambda exprF z body
                 Some z
             | _  -> 
                 None // NO: no intercept 
          
          let context = []
          recognise context expr

      let targetIntercept exprF z = function TTarget(argvs,body,_) -> Some (foldUnderLambda exprF z body)
      let tmethodIntercept exprF z = function TObjExprMethod(_,_,_,e,m) -> Some (foldUnderLambda exprF z e)
      
      {ExprFolder0 with
         exprIntercept    = exprUsageIntercept; 
         nonRecBindingsIntercept         = logNonRecBinding;
         recBindingsIntercept         = logRecBindings;
         valBindingSiteIntercept          = logBinding;
         targetIntercept  = targetIntercept;
         tmethodIntercept = tmethodIntercept;
      }


    (*-------------------------------------------------------------------------
     *INDEX: xinfo - entry point
     *-------------------------------------------------------------------------*)

    let GetUsageInfoOfImplFile g expr =
        let folder = UsageFolders g
        let z = FoldImplFile folder z0 expr
        z

end


open GlobalUsageAnalysis

(*-------------------------------------------------------------------------
 *INDEX: misc
 *-------------------------------------------------------------------------*)
  
let internalError str = raise(Failure(str))

let mkLocalVal m name ty topValInfo =
    let compgen    = false in (* REVIEW: review: should this be true? *)
    NewVal(mksyn_id m name,ty,Immutable,compgen,topValInfo,None,taccessPublic,ValNotInRecScope,None,NormalVal,[],OptionalInline,emptyXmlDoc,false,false,false,false,None,ParentNone) 

let dprintTerm header expr =
  if false then
    let str = Layout.showL (Layout.squashTo 192 (ImplFileL expr)) in  (* improve cxty! *)
    dprintf "\n\n\n%s:\n%s\n" header str
  else
    ()


(*-------------------------------------------------------------------------
 *INDEX: TupleStructure = tuple structure
 *-------------------------------------------------------------------------*)

type TupleStructure = (* tuple structure *)
    | UnknownTS
    | TupleTS   of TupleStructure list

let rec TopValInfoForTS = function
    | UnknownTS  -> [TopValInfo.unnamedTopArg]
    | TupleTS ts -> ts |> List.collect TopValInfoForTS 

let rec andTS ts tsB =
    match ts,tsB with
    |         _   ,UnknownTS    -> UnknownTS
    | UnknownTS   ,_            -> UnknownTS
    | TupleTS ss  ,TupleTS ssB  -> if List.length ss <> List.length ssB then UnknownTS (* different tuple instances *)
                                                                        else TupleTS (List.map2 andTS ss ssB)

let checkTS = function
    | TupleTS []   -> internalError "exprTS: Tuple[]  not expected. (units not done that way)."
    | TupleTS [ts] -> internalError "exprTS: Tuple[x] not expected. (singleton tuples should not exist."
    | ts           -> ts   
          
let rec uncheckedExprTS = function (* explicit tuple-structure in expr *)
    | TExpr_op(TOp_tuple,tys,args,m) -> TupleTS (List.map uncheckedExprTS args)
    | _                        -> UnknownTS

let rec uncheckedTypeTS g ty =
    if is_tuple_typ g ty then 
        let tys = dest_tuple_typ g ty 
        TupleTS (List.map (uncheckedTypeTS g) tys)
    else 
        UnknownTS

let exprTS exprs = exprs |> uncheckedExprTS |> checkTS
let typeTS g tys = tys |> uncheckedTypeTS g |> checkTS

let rebuildTS g m ts vs =
    let rec rebuild vs ts = 
      match vs,ts with
      | []   ,UnknownTS   -> internalError "rebuildTS: not enough fringe to build tuple"
      | v::vs,UnknownTS   -> vs,(expr_for_val m v,v.Type)
      | vs   ,TupleTS tss -> let vs,xtys = List.fmap rebuild vs tss
                             let xs,tys  = List.unzip xtys
                             let x  = mk_tupled g m xs tys
                             let ty = mk_tupled_ty g tys
                             vs,(x,ty)
   
    let vs,(x,ty) = rebuild vs ts
    if List.length vs<>0 then internalError "rebuildTS: had move fringe vars than fringe. REPORT BUG" else ();
    x

(* naive string concats, just for testing *)

/// CallPattern is tuple-structure for each argument position.
/// - callsites have a CallPattern (possibly instancing fOrig at tuple types...).
/// - the definition lambdas may imply a one-level CallPattern
/// - the definition formal projection info suggests a CallPattern
type CallPattern =
    TupleStructure list (* equality/ordering ok on this type *)
      
let callPatternOrder = (compare : CallPattern -> CallPattern -> int)
let argsCP exprs = List.map exprTS exprs
let noArgsCP = []
let isTrivialCP xs = (isNil xs)

#if DEBUG
let rec soTS = function (UnknownTS) -> "_" | TupleTS ss -> "(" ^ String.concat "," (List.map soTS ss) ^ ")"
let soCP tss = String.concat ";" (List.map soTS tss)
#endif

let rec minimalCP callPattern =
    match callPattern with 
    | []                -> []
    | UnknownTS::tss    -> 
        match minimalCP tss with
        | []  -> []              (* drop trailing UnknownTS *)
        | tss -> UnknownTS::tss (* non triv tss tail *)
    | (TupleTS ts)::tss -> TupleTS ts :: minimalCP tss

/// INTERSECTION.
/// Combines a list of callpatterns into one common callpattern.
let commonCP callPatterns =
    let rec andCPs cpA cpB =
      match cpA,cpB with
      | []       ,[]        -> []
      | tsA::tsAs,tsB::tsBs -> andTS tsA tsB :: andCPs tsAs tsBs
      | tsA::tsAs,[]        -> [] (* now trim to shortest - UnknownTS     :: andCPs tsAs []   *)
      | []       ,tsB::tsBs -> [] (* now trim to shortest - UnknownTS     :: andCPs []   tsBs *)
   
    List.reduce_left andCPs callPatterns

let siteCP (accessors,inst,args) = argsCP args
let sitesCPs sites = List.map siteCP sites

(*-------------------------------------------------------------------------
 *INDEX: transform
 *-------------------------------------------------------------------------*)

type TransformedFormal =
  // Indicates that
  //    - the actual arg in this position is unchanged
  //    - also menas that we keep the original formal arg
  | SameArg                          

  // Indictes 
  //    - the new formals for the transform
  //    - expr is tuple of the formals
  | NewArgs of Val list * expr  

/// Info needed to convert f to curried form.
/// - yb1..ybp - replacement formal choices for x1...xp.
/// - transformedVal       - replaces f.
type Transform =
   { transformCallPattern : CallPattern;
     transformedFormals   : TransformedFormal list; (* REVIEW: could push these to fixup binding site *)
     transformedVal         : Val;
   }


(*-------------------------------------------------------------------------
 *INDEX: transform - mkTransform - decided, create necessary stuff
 *-------------------------------------------------------------------------*)

let mkTransform g (f:Val) m tps x1Ntys rty (callPattern,tyfringes: (typ list * Val list) list) =
    // Create formal choices for x1...xp under callPattern  
    let transformedFormals = 
        (callPattern,tyfringes) ||>  List.map2 (fun cpi (tyfringe,vs) -> 
            match cpi with
            | UnknownTS  -> SameArg
            | TupleTS [] -> SameArg  
            | TupleTS ts -> 
                // Try to keep the same names for the arguments if possible
                let vs = 
                    if List.length vs = List.length tyfringe then 
                        vs |> List.map (fun v -> mk_compgen_local v.Range v.MangledName v.Type |> fst)
                    else
                        let baseName = match vs with [v] -> v.MangledName | _ -> "arg"
                        let baseRange = match vs with [v] -> v.Range | _ -> m
                        tyfringe |> List.mapi (fun i ty -> 
                            let name = baseName ^ string i
                            mk_compgen_local baseRange name ty |> fst)
                        
                NewArgs (vs,rebuildTS g m cpi vs))
       
    // Create transformedVal replacement for f 
    // Mark the arity of the value 
    let topValInfo = 
        match f.TopValInfo with 
        | None -> None 
        | _ -> Some(TopValInfo (TopValInfo.InferTyparInfo tps,List.collect TopValInfoForTS callPattern,TopValInfo.unnamedRetVal))
    (* type(transformedVal) tyfringes types replace initial arg types of f *)
    let tys1r = List.collect fst tyfringes  (* types for collapsed initial r args *)
    let tysrN = List.drop tyfringes.Length x1Ntys    (* types for remaining args *)
    let argtys = tys1r @ tysrN
    let fCty  = mk_lambda_ty tps argtys rty                  
    let transformedVal  = mkLocalVal f.Range (globalNng.FreshCompilerGeneratedName (f.MangledName,f.Range)) fCty topValInfo
  (*dprintf "mkTransform: f=%s\n"         (showL (valL f));
    dprintf "mkTransform: tps=%s\n"       (showL (commaListL (List.map TyparL tps)));
    dprintf "mkTransform: callPattern=%s\n"        (soCP callPattern);
    dprintf "mkTransform: tyfringes=%s\n" (showL (commaListL (List.map (fun fr -> tupleL (List.map typeL fr)) tyfringes)));
    dprintf "mkTransform: tys1r=%s\n"     (showL (commaListL (List.map typeL tys1r)));
    dprintf "mkTransform: tysrN=%s\n"     (showL (commaListL (List.map typeL tysrN)));
    dprintf "mkTransform: rty  =%s\n"     ((DebugPrint.showType rty));
  *)   
    { transformCallPattern = callPattern;
      transformedFormals      = transformedFormals;
      transformedVal         = transformedVal;
    }

#if DEBUG
open Microsoft.FSharp.Compiler.Layout    
let dumpTransform trans =
    let argTS = function
      | 1 -> UnknownTS
      | n -> TupleTS (List.repeat n UnknownTS)
   
    dprintf " - cp   : %s\n - transformedVal   : %s\n\n"
      (soCP trans.transformCallPattern)
      (showL (valL trans.transformedVal))
#endif

(*-------------------------------------------------------------------------
 *INDEX: transform - vTransforms - support
 *-------------------------------------------------------------------------*)

let zipCallPatternArgTys m g (callPattern : TupleStructure list) (vss : Val list list) =
    let rec zipTSTyp ts typ =
        // match a tuple-structure and type, yields:
        //  (a) (restricted) tuple-structure, and
        //  (b) type fringe for each arg position.
        match ts with
        | TupleTS tss when is_tuple_typ g typ ->
            let tys = dest_tuple_typ g typ 
            let tss,tyfringe = zipTSListTypList tss tys
            TupleTS tss,tyfringe
        | _ -> 
            UnknownTS,[typ] (* trim back CallPattern, function more general *)
    and zipTSListTypList tss tys =
        let tstys = List.map2 zipTSTyp tss tys  // assumes tss tys same length 
        let tss  = List.map fst tstys         
        let tys = List.collect snd tstys       // link fringes 
        tss,tys
    
    let vss = List.take callPattern.Length vss    // drop excessive tys if callPattern shorter 
    let tstys = List.map2 (fun ts vs -> let ts,tyfringe = zipTSTyp ts (type_of_lambda_arg m vs) in ts,(tyfringe,vs)) callPattern vss
    List.unzip tstys   

(*-------------------------------------------------------------------------
 *INDEX: transform - vTransforms - defnSuggestedCP
 *-------------------------------------------------------------------------*)

/// v = LAM tps. lam vs1:ty1 ... vsN:tyN. body.
/// The types suggest a tuple structure CallPattern.
/// The BuildProjections of the vsi trim this down,
/// since do not want to take as components any tuple that is required (projected to).
let decideFormalSuggestedCP g z tys vss =

    let rec trimTsByAccess accessors ts =
        match ts,accessors with
        | UnknownTS ,_                       -> UnknownTS
        | TupleTS tss,[]                     -> UnknownTS (* trim it, require the val at this point *)
        | TupleTS tss,PTup (i,ty)::accessors -> 
            let tss = List.mapNth i (trimTsByAccess accessors) tss
            TupleTS tss

    let trimTsByVal z ts v =
        match Zmap.tryfind v z.xinfo_uses with
        | None       -> UnknownTS (* formal has no usage info, it is unused *)
        | Some sites -> 
            let trim ts (accessors,inst,args) = trimTsByAccess accessors ts
            List.fold trim ts sites

    let trimTsByFormal z ts vss = 
        match vss with 
        | [v]  -> trimTsByVal z ts v
        | vs   -> 
            let tss = match ts with TupleTS tss -> tss | _ -> internalError "trimByFormal: ts must be tuple?? PLEASE REPORT\n"
            let tss = List.map2 (trimTsByVal z) tss vs
            TupleTS tss

    let tss = List.map (typeTS g) tys (* most general TS according to type *)
    let tss = List.map2 (trimTsByFormal z) tss vss
    tss

(*-------------------------------------------------------------------------
 *INDEX: transform - decideTransform
 *-------------------------------------------------------------------------*)

let decideTransform g z v callPatterns (m,tps,vss:Val list list,rty) (* tys are types of outer args *) =
    let tys = List.map (type_of_lambda_arg m) vss       (* arg types *)
    (* NOTE: 'a in arg types may have been instanced at different tuples... *)
    (*       commonCP has to handle those cases. *)
    let callPattern           = commonCP callPatterns                   // common CallPattern 
    let callPattern           = List.take vss.Length callPattern            // restricted to max nArgs 
    (* NOW: get formal callPattern by defn usage of formals *)
    let formalCallPattern     = decideFormalSuggestedCP g z tys vss 
    let callPattern           = List.take callPattern.Length formalCallPattern
    // zip with information about known args 
    let callPattern,tyfringes = zipCallPatternArgTys m g callPattern vss
    // drop trivial tail AND 
    let callPattern           = minimalCP callPattern                     
    // shorten tyfringes (zippable) 
    let tyfringes    = List.take callPattern.Length tyfringes       
  (*dprintf "decideTransform: for v=%s\n" (showL (valL v));
    List.iter (fun cp -> dprintf "- site cp    = %s\n" (soCP cp)) callPatterns;
    dprintf "- common  cp = %s\n" (soCP cp);     
    dprintf "- front   cp = %s\n" (soCP cp);
    dprintf "- arg tys    = %s\n" (showL (commaListL (List.map typeL tys)));  
    dprintf "- formalCallPattern        = %s\n" (soCP formalCallPattern);  
    dprintf "- front formalCallPattern  = %s\n" (soCP cp);
    dprintf "- zipped  cp = %s\n" (soCP cp);
    dprintf "- tyfringes  = %s\n" (showL (commaListL (List.map (List.length >> intL) tyfringes)));  
    dprintf "- minimal cp = %s\n\n" (soCP cp);
  *)   
    if isTrivialCP callPattern then
        None (* no transform *)
    else
        Some (v,mkTransform g v m tps tys rty (callPattern,tyfringes))


(*-------------------------------------------------------------------------
 *INDEX: transform - DetermineTransforms
 *-------------------------------------------------------------------------*)
      
// Public f could be used beyond assembly.
// For now, suppressing any transforms on these.
// Later, could transform f and fix up local calls and provide an f wrapper for beyond. 
let eligibleVal g (v:Val) =
    let dllImportStubOrOtherNeverInline = (v.InlineInfo = NeverInline)
    let mutableVal = v.IsMutable
    let byrefVal = is_byref_typ g v.Type
    not dllImportStubOrOtherNeverInline &&
    not byrefVal &&
    not mutableVal &&
    not v.IsMemberOrModuleBinding && //  .IsCompiledAsTopLevel &&
    not v.IsCompiledAsTopLevel 

let DetermineTransforms g (z : GlobalUsageAnalysis.xinfo) =
   let selectTransform f sites =
     if not (eligibleVal g f) then None else
     (* consider f, if it has top-level lambda (meaning has term args) *)
     match Zmap.tryfind f z.xinfo_eqns with
     | None   -> None (* no binding site, so no transform *)
     | Some e -> 
        let tps,vss,b,rty = dest_top_lambda (e,f.Type)
        match List.concat vss with
        | []      -> None (* defn has no term args *)
        | arg1::_ -> (* consider f *)
          let m   = arg1.Range                       (* mark of first arg, mostly for error reporting *)
          let callPatterns = sitesCPs sites                   (* callPatterns from sites *)
          decideTransform g z f callPatterns (m,tps,vss,rty) (* make transform (if required) *)
  
   let vtransforms = Zmap.chooseL selectTransform z.xinfo_uses
   let vtransforms = Zmap.of_list val_spec_order vtransforms
   vtransforms

#if DEBUG
let dumpVTransform v tr =
    dprintf "Transform for %s\n" (showL (valL v));
    dumpTransform tr;
    stdout.Flush()
#endif


(*-------------------------------------------------------------------------
 *INDEX: pass - penv - env of pass
 *-------------------------------------------------------------------------*)

type penv =
   { transforms : Zmap.map<Val,Transform>; (* planned transforms *)
     ccu        : ccu;
     g          : Env.TcGlobals;
   }

let HasTransfrom penv f = Zmap.tryfind f penv.transforms

(*-------------------------------------------------------------------------
 *INDEX: pass - app fixup - CollapseArgs
 *-------------------------------------------------------------------------*)

(* CollapseArgs:
   - the args may not be tuples (decision made on defn projection).
   - need to factor any side-effecting args out into a let binding sequence.
   - also factor BuildProjections, so they share common tmps.
*)

type env = {eg : TcGlobals;
            prefix : string;
            m      : Range.range; }
let suffixE env s = {env with prefix = env.prefix ^ s}
let rangeE  env m = {env with m = m}

let push  b  bs = b::bs
let pushL xs bs = xs@bs

let newLocal  env   ty = mk_compgen_local env.m env.prefix ty
let newLocalN env i ty = mk_compgen_local env.m (env.prefix ^ string i) ty

let noEffectExpr env bindings x =
    match x with
    | TExpr_val (v,_,m) -> bindings,x
    | x                 -> 
        let tmp,xtmp = newLocal env (type_of_expr env.eg x)
        let bind = mk_compgen_bind tmp x
        push bind bindings,xtmp

// Given 'e', build 
//     let v1 = e#1
//     let v2 = e#N
let BuildProjections env bindings x xtys =

    let binds,vixs = 
        xtys 
        |> List.mapi (fun i xty ->
            let vi,vix = newLocalN env i xty
            let bind = mk_bind NoSequencePointAtInvisibleBinding vi (mk_tuple_field_get (x,xtys,i,env.m))
            bind,vix)
        |> List.unzip

    // Why are we reversing here? Because we end up reversing once more later
    let bindings = pushL (List.rev binds) bindings
    bindings,vixs

let rec CollapseArg env bindings ts x =
    let m = range_of_expr x
    let env = rangeE env m
    match ts,x with
    | UnknownTS  ,x -> 
        let bindings,vx = noEffectExpr env bindings x
        bindings,[vx]
    | TupleTS tss,TExpr_op(TOp_tuple,xtys,xs,m) -> 
        let env = suffixE env "'"
        CollapseArgs env bindings 1 tss xs
    | TupleTS tss,x                      -> 
        // project components 
        let bindings,x = noEffectExpr env bindings x
        let env  = suffixE env "_p" 
        let xty = type_of_expr env.eg x
        let xtys = dest_tuple_typ env.eg xty
        let bindings,xs = BuildProjections env bindings x xtys
        CollapseArg env bindings (TupleTS tss) (mk_tupled env.eg m xs xtys)

and CollapseArgs env bindings n (callPattern as tss) args =
    match callPattern,args with
    | []     ,args        -> bindings,args
    | ts::tss,arg::args -> 
        let env1 = suffixE env (string n)
        let bindings,xty  = CollapseArg  env1 bindings ts    arg     
        let bindings,xtys = CollapseArgs env  bindings (n+1) tss args
        bindings,xty @ xtys
    | ts::tss,[]            -> 
        internalError "CollapseArgs: CallPattern longer than callsite args. REPORT BUG"


//-------------------------------------------------------------------------
// pass - app fixup
//-------------------------------------------------------------------------

// REVIEW: use mk_let etc. 
let nestedLet = List.foldBack (fun b acc -> mk_let_bind (range_of_expr acc) b acc) 

let FixupApp (penv:penv) (fx,fty,tys,args,m) =

    // Is it a val app, where the val has a transform? 
    match fx with
    | TExpr_val (vref,_,m) -> 
        let f = deref_val vref
        match HasTransfrom penv f with
        | Some trans -> 
            // fix it 
            let callPattern       = trans.transformCallPattern 
            let transformedVal       = trans.transformedVal         
            let fCty     = transformedVal.Type
            let fCx      = expr_for_val m transformedVal
            (* [[f tps args ]] -> transformedVal tps [[COLLAPSED: args]] *)
            let env      = {prefix = "arg";m = m;eg=penv.g}
            let bindings = []
            let bindings,args = CollapseArgs env bindings 0 callPattern args
            let bindings = List.rev bindings
            nestedLet bindings (TExpr_app (fCx,fCty,tys,args,m))
        | None       -> 
            TExpr_app (fx,fty,tys,args,m) (* no change, f untransformed val *)
    | _ -> 
        TExpr_app (fx,fty,tys,args,m)                      (* no change, f is expr *)


//-------------------------------------------------------------------------
//INDEX: pass - mubinds - translation support
//-------------------------------------------------------------------------

let TransFormal ybi xi =
    match ybi with
    | SameArg        -> [xi]                         // one arg   - where arg=vpsecs 
    | NewArgs (vs,x) -> vs |> List.map List.singleton // many args 

let TransRebind ybi xi =
    match xi,ybi with
    | xi ,SameArg        -> []                    (* no rebinding, reused original formal *)
    | [u],NewArgs (vs,x) -> [mk_compgen_bind u x]
    | us ,NewArgs (vs,x) -> List.map2 mk_compgen_bind us (try_dest_tuple x)


//-------------------------------------------------------------------------
//INDEX: pass - mubinds
//-------------------------------------------------------------------------

// Foreach (f,repr) where
//   If f has trans, then
//   repr = LAM tps. lam x1...xN . body
//
//   transformedVal, yb1...ybp in trans.
//
// New binding:
//
//   transformedVal = LAM tps. lam [[FORMALS: yb1 ... ybp]] xq...xN = let [[REBINDS: x1,yb1 ...]]
//                                                        body
//
// Does not fix calls/defns in binding rhs, that is done by caller.
//

let pass_bind penv (TBind(fOrig,repr,letSeqPtOpt) as bind) =
     let m = fOrig.Range
     match HasTransfrom penv fOrig with
     | None ->
         // fOrig no transform 
         bind
     | Some trans ->
         // fOrig has transform 
         let tps,vss,body,rty = dest_top_lambda (repr,fOrig.Type) in (* expectation *)
         // transformedVal is curried version of fOrig 
         let transformedVal    = trans.transformedVal
         // fCBody - parts - formals 
         let transformedFormals = trans.transformedFormals 
         let p     = transformedFormals.Length
         if (vss.Length < p) then internalError "pass_binds: |vss|<p - detuple pass" else (); (* ASSERTION *)
         let xqNs  = List.drop p vss  
         let x1ps  = List.take p vss  
         let y1Ps  = List.concat (List.map2 TransFormal transformedFormals x1ps)
         let formals = y1Ps @ xqNs
         // fCBody - parts 
         let rebinds = List.concat (List.map2 TransRebind transformedFormals x1ps)
         // fCBody - rebuild 
         // fCBody = TLambda tps. Lam formals. let rebinds in body 
         let rbody,rt  = mk_lets_bind            m rebinds body,rty   
         let bind      = mk_multi_lambda_bind transformedVal letSeqPtOpt m tps formals (rbody,rt)
         // result 
         bind

let pass_binds penv binds = binds |> FlatList.map (pass_bind penv) 

(*-------------------------------------------------------------------------
 *INDEX: pass - pass_bind_rhs
 *
 * At bindings (letrec/let),
 *   0. run pass of bodies first.
 *   1. transform bindings (as required),
 *      yields new bindings and fixup data for callsites.
 *   2. required to fixup any recursive calls in the bodies (beware O(n^2) cost)
 *   3. run pass over following code.
 *-------------------------------------------------------------------------*)

let pass_bind_rhs penv conv (TBind (v,repr,letSeqPtOpt)) = TBind(v,conv repr,letSeqPtOpt)
let pre_intercept_expr (penv:penv) conv expr =
  match expr with
  | TExpr_letrec (binds,e,m,_) ->
     let binds = FlatList.map (pass_bind_rhs penv conv) binds
     let binds = pass_binds penv binds
     Some (mk_letrec_binds m binds (conv e))
  | TExpr_let (bind,e,m,_) ->  
     let bind = pass_bind_rhs penv conv bind
     let bind = pass_bind penv bind
     Some (mk_let_bind m bind (conv e))
  | TyappAndApp(f,fty,tys,args,m) ->
     (* match app, and fixup if needed *)
     let args = List.map conv args
     let f = conv f
     Some (FixupApp penv (f,fty,tys,args,m) )
  | _ -> None
  

let PostTransformExpr (penv:penv) expr =
    match expr with
    | TExpr_letrec (binds,e,m,_) ->
        let binds = pass_binds penv binds
        Some (mk_letrec_binds m binds e)
    | TExpr_let (bind,e,m,_) ->  
        let bind = pass_bind penv bind
        Some (mk_let_bind m bind e)
    | TyappAndApp(f,fty,tys,args,m) ->
        // match app, and fixup if needed 
        Some (FixupApp penv (f,fty,tys,args,m) )
    | _ -> None
  

let pass_ImplFile penv ass = 
    ass |> RewriteImplFile {pre_intercept =None (* Some (pre_intercept_expr penv) *);
                            post_transform= PostTransformExpr penv (* (fun _ -> None)  *);
                            under_quotations=false } 


(*-------------------------------------------------------------------------
 *INDEX: entry point
 *-------------------------------------------------------------------------*)

let DetupleImplFile ccu g expr =
   (* collect expr info - wanting usage contexts and bindings *)
   let (z : xinfo) = GetUsageInfoOfImplFile g expr
   (* For each Val, decide Some "transform", or None if not changing *)
   let vtrans = DetermineTransforms g z

#if DEBUG
   // Diagnostics - summary of planned transforms 
   if verbose then dprintf "note: detuple - %d functions transformed\n" (List.length (Zmap.keys vtrans));
   if verbose then Zmap.iter dumpVTransform vtrans;
#endif

   (* Pass over term, rewriting bindings and fixing up call sites, under penv *)
   let penv = {g=g; transforms = vtrans; ccu = ccu}
   if verbose then dprintTerm "DetupleAssembly before:" expr;
   if verbose then dprintf   "DetupleAssembly: pass\n";
   let z = () in (* z=state, relic, to be removed *)
   let expr = pass_ImplFile penv expr
   if verbose then dprintTerm "DetupleAssembly after:" expr;
   if verbose then dprintf   "DetupleAssembly: done\n";
   expr
