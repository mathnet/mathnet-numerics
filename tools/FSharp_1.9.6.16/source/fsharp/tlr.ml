// (c) Microsoft Corporation. All rights reserved
#light

module internal Microsoft.FSharp.Compiler.Tlr 

open Internal.Utilities
open Internal.Utilities.Pervasives
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
open Microsoft.FSharp.Compiler.Detuple.GlobalUsageAnalysis
open Microsoft.FSharp.Compiler.Lib

let verboseTLR = false

/// Turns on explicit lifting of TLR constants to toplevel
/// e.g. use true if want the TLR constants to be initialised once.
///
/// NOTE: liftTLR is incomplete and disabled
///       Approach is to filter Top* let binds whilst "under lambdas",
///       and wrap them around that expr ASAP (when get to TopLevel position).
///       However, for arity assigned public vals (not TLR at moment),
///       assumptions that their RHS are lambdas get broken since the
///       lambda can be wrapped with bindings...
let liftTLR    = ref false

//-------------------------------------------------------------------------
// library helpers
//-------------------------------------------------------------------------

let internalError str = dprintf "Error: %s\n" str;raise (Failure str)  

module Zmap = 
    let force k   mp (str,soK) = try Zmap.find k mp with e -> dprintf "forceM': %s %s\n" str (soK k); raise e

let forceM' k   mp (str,soK) = try Zmap.find k mp with e -> dprintf "forceM: %s %s\n" str (soK k); raise e

//-------------------------------------------------------------------------
// misc
//-------------------------------------------------------------------------

/// tree, used to store dec sequence 
type 'a tree =
    | TreeNode of 'a tree list
    | LeafNode of 'a

let fringeTR tr =
    let rec collect tr acc =
        match tr with
        | TreeNode subts -> List.foldBack collect subts acc
        | LeafNode x     -> x::acc
   
    collect tr []

let emptyTR = TreeNode[]


//-------------------------------------------------------------------------
// misc
//-------------------------------------------------------------------------

/// Collapse reclinks on app and combine apps if possible 
/// recursive ids are inside reclinks and maybe be type instanced with a TExpr_app 

// CLEANUP NOTE: mk_appl ensures applications are kept in a collapsed 
// and combined form, so this function should not be needed 
let DestApp (f,fty,tys,args,m) =
    match strip_expr f with
    | TExpr_app (f2,fty2,tys2,[]     ,m2) -> (f2,fty2,tys2 @ tys,args,m)
    | TExpr_app (f2,fty2,tys2,argtys2,m2) -> (f,fty,tys,args,m) (* has args, so not combine ty args *)
    | f                                   -> (f,fty,tys,args,m)

let soTyparSet tps = showL (commaListL (List.map TyparL (Zset.elements tps)))    
let soTyp t = (DebugPrint.showType t)    
let soLength xs = string (List.length xs)

// CLEANUP NOTE: don't like the look of this function - this distinction 
// should never be needed 
let isDelayedRepr (f:Val) e = 
    let tps,vss,b,rty = dest_top_lambda (e,f.Type)
    List.length vss>0


// REVIEW: these should just be replaced by direct calls to mk_local, mk_compgen_local etc. 
// REVIEW: However these set an arity whereas the others don't 
let mkLocalNameTypeArity compgen m name ty topValInfo =
    NewVal(ident(name,m),ty,Immutable,compgen,topValInfo,None,taccessPublic,ValNotInRecScope,None,NormalVal,[],OptionalInline,emptyXmlDoc,false,false,false,false,None,ParentNone)

let mkLocal m name ty = mkLocalNameTypeArity true m name ty None

//-------------------------------------------------------------------------
// definitions: TLR, arity, arity-met, arity-short
//-------------------------------------------------------------------------

(* DEFN: An f is TLR with arity wf if
         (a) it's repr is "LAM tps. lam x1...xN. body" and have N<=wf (i.e. have enough args)
         (b) it has no free tps
         (c) for g:freevars(repr), both
             (1) g is TLR with arity wg, and
             (2) g occurs in arity-met occurance.
         (d) if N=0, then further require that body be a TLR-constant.

   Conditions (a-c) are required if f is to have a static method/field represenation.
   Condition (d) chooses which constants can be lifted. (no effects, non-trivial).

   DEFN: An arity-met occurance of g is a g application with enough args supplied,
         ie. (g tps args) where wg <= |args|.

   DEFN: An arity-short occurance does not have enough args.

   DEFN: A TLR-constant:
         - can have constructors (tuples, datatype, records, exn).
         - should be non-trivial (says, causes allocation).
         - if calls are allowed, they must be effect free (since eval point is moving).
*)


//-------------------------------------------------------------------------
// OVERVIEW
//-------------------------------------------------------------------------

(* Overview of passes (over term) and steps (not over term):

   pass1 - decide which f will be TLR and determine their arity.
   pass2 - what closures are needed? Finds etps(f) and envReq(f) for TLR f.
           Depends on the arity choice, so must follow pass1.
   step3 - choose env packing, create fHats.
   pass4 - rewrite term fixing up definitions and callsites.
           Depends on closure and env packing, so must follow pass2 (and step 3).
   pass5 - copy_expr call to topexpr to ensure all bound ids are unique.
           For complexity reasons, better to re-recurse over expr once.
   pass6 - sanity check, confirm that all TLR marked bindings meet DEFN.

*)   

//-------------------------------------------------------------------------
// pass1: GetValsBoundUnderMustInline (see comment further below)
//-------------------------------------------------------------------------

let GetValsBoundUnderMustInline ccu xinfo =
    let accRejectFrom (v:Val) repr rejectS =
      if v.InlineInfo = PseudoValue then
        Zset.union (GetValsBoundInExpr repr) rejectS
      else rejectS
    let rejectS = Zset.empty val_spec_order
    let rejectS = Zmap.fold accRejectFrom xinfo.xinfo_eqns rejectS
    rejectS

//-------------------------------------------------------------------------
// pass1: isTLRConstant
//-------------------------------------------------------------------------

(*
let rec trivialExpr x =
  match x with
  | TExpr_val _                       -> true
  | TExpr_op(TOp_ucase (_),tyargs,[],_)    -> true
  | TExpr_const _                     -> true
  | TExpr_app((f0,f0ty),tyargsl,[],m) -> not (is_tyfunc_vref_expr f0) && trivialExpr f0
  | _                                 -> false

let rec const_expr x =
  (* For now: constructions around constants
   * Later, can also refer to *PRIOR* TLR constants (i.e. CSE) - need declaration order.
   *)
  match x with
  | TExpr_const _                    -> true
  | TExpr_op(TOp_tuple, tys,args,m)        -> const_exprs args
  | TExpr_op(TOp_ucase cr,tinst,args,m)   -> const_exprs args && not (tcref_alloc_observable (fst cr))
  | TExpr_op(TOp_recd   (ctor,tcr),tinst,args,m)  -> ctor = None && const_exprs args && not (tcref_alloc_observable tcr)
(*| TExpr_lambda _ | TExpr_tlambda _ -> true -- these should be ok *)
(*| could also allow calls to functions which have no known effect *)
  | _                                -> false
and const_exprs es = List.forall const_expr es

let isTLRConstant x = const_expr x && not (trivialExpr x)
*)

//-------------------------------------------------------------------------
// pass1: IsRefusedTLR
//-------------------------------------------------------------------------
       
let IsRefusedTLR g (f:Val) =  
    let mutableVal = f.IsMutable
    // things marked NeverInline are special 
    let dllImportStubOrOtherNeverInline = (f.InlineInfo = NeverInline)
    // Cannot have static fields of byref type 
    let byrefVal = is_byref_typ g f.Type
    // Special values are instance methods etc. on .NET types.  For now leave these alone 
    let specialVal = isSome(f.MemberInfo)
    let alreadyChosen = f.TopValInfo.IsSome
    let refuseTest = alreadyChosen || mutableVal || byrefVal || specialVal || dllImportStubOrOtherNeverInline
    refuseTest

let IsMandatoryTopLevel (f:Val) = 
    let specialVal = isSome(f.MemberInfo)
    let isModulBinding = f.IsMemberOrModuleBinding
    specialVal || isModulBinding

let IsMandatoryNonTopLevel g (f:Val) =
    let byrefVal = is_byref_typ g f.Type
    byrefVal


//-------------------------------------------------------------------------
// pass1: decide which f are to be TLR? and if so, arity(f)
//-------------------------------------------------------------------------

module Pass1_DetermineTLRAndArities = 

    let GetMaxNumArgsAtUses xinfo f =
       match Zmap.tryfind f xinfo.xinfo_uses with
       | None       -> 0 (* no call sites *)
       | Some sites -> 
           sites |> List.map (fun (accessors,tinst,args) -> List.length args) |> List.max

    let SelectTLRVals g xinfo f e =
        if IsRefusedTLR g f then None 
        else if Zset.mem f xinfo.xinfo_dtree then None
        else
            (* Could the binding be TLR? with what arity? *)
            let atTopLevel = Zset.mem f xinfo.xinfo_toplevel
            let tps,vss,b,rty = dest_top_lambda (e,f.Type)
            let nFormals    = List.length vss
            let nMaxApplied = GetMaxNumArgsAtUses xinfo f  
            let arity       = Operators.min nFormals nMaxApplied
            if atTopLevel or arity<>0 or nonNil tps then Some (f,arity)
            else None

    /// Check if f involves any value recursion (so can skip those).
    /// ValRec considered: recursive && some f in mutual binding is not bound to a lambda
    let IsValueRecursionFree xinfo f =

        let hasDelayedRepr f = isDelayedRepr f (forceM' f xinfo.xinfo_eqns ("IsValueRecursionFree - hasDelayedRepr",name_of_val))
        let recursive,mudefs = forceM' f xinfo.xinfo_mubinds ("IsValueRecursionFree",name_of_val)
        not recursive || FlatList.forall hasDelayedRepr mudefs

    let DumpArity arityM =
        let dump f n = dprintf "tlr: arity %50s = %d\n" (showL (valL f)) n
        Zmap.iter dump arityM

    let DetermineTLRAndArities ccu g expr  =
       if verboseTLR then dprintf "DetermineTLRAndArities------\n";
       let xinfo = GetUsageInfoOfImplFile g expr
       let fArities = Zmap.chooseL (SelectTLRVals g xinfo) xinfo.xinfo_eqns
       let fArities = List.filter (fst >> IsValueRecursionFree xinfo) fArities
       // Do not TLR v if it is bound under a mustinline defn 
       // There is simply no point - the original value will be duplicated and TLR'd anyway 
       // However we could report warnings for such values as they lead to duplication 
       // which could be avoided by making inlineable versions of the TLR'd values 
       // also available. 
       let rejectS = GetValsBoundUnderMustInline ccu xinfo
       let fArities = List.filter (fun (v,_) -> not (Zset.mem v rejectS)) fArities
       (*-*)
       let tlrS   = Zset.of_list val_spec_order (List.map fst fArities)
       let topValS   = xinfo.xinfo_toplevel in                    (* genuinely top level *)
       let topValS   = Zset.filter (IsMandatoryNonTopLevel g >> not) topValS in    (* restrict *)
    (*
       let fUsedOnce = Zmap.List.choose (selectUsedOnce g xinfo) xinfo.xinfo_eqns
       let fUsedOnce = List.filter (isInRecursiveBinding xinfo >> not) fUsedOnce
       let fUsedOnce = List.filter (fun v -> not (Zset.mem v rejectS)) fUsedOnce
       let fUsedOnce = List.filter (fun v -> not (Zset.mem v tlrS)) fUsedOnce
       let fUsedOnce = List.filter (fun v -> not (Zset.mem v topValS)) fUsedOnce
       dprintf "#fUsedOnce = %d\n" (List.length fUsedOnce);
       fUsedOnce |> List.iter (fun v -> v.Data.val_mustinline <- PseudoValue);
    *)
       (* REPORT MISSED CASES *)
       begin 
         if verboseTLR then 
           let missed = Zset.diff  xinfo.xinfo_toplevel tlrS
           missed |> Zset.iter (fun v -> dprintf "TopLevel but not TLR = %s\n" v.MangledName) 
       end;
       (* REPORT OVER *)   
       let arityM = Zmap.of_list val_spec_order fArities
       if verboseTLR then DumpArity arityM;
       tlrS,topValS, arityM

     

(* NOTES:
   For constants,
     Want to fold in a declaration order,
     so can make decisions about TLR given TLR-knowledge about prior constants.
     Assuming ilxgen will fix up initialisations.
   So,
     xinfo to be extended to include some scoping representation.
     Maybe a telescope tree which can be walked over.
 *)

//-------------------------------------------------------------------------
// pass2: determine etps(f) and envreq(f) - notes
//-------------------------------------------------------------------------

/// What are the closing types/values for {f1,f2...} mutally defined?
///
//   Note: arity-met g-applications (g TLR) will translated as:
//           [[g @ tps ` args]] -> gHAT @ etps(g) tps ` env(g) args
//         so they require availability of closing types/values for g.
//
//   If g is free wrt f1,f2... then g's closure must be included.
//
//   Note: mutual definitions have a common closure.
//
//   For f1,f2,... = fBody1,fbody2... mutual bindings:
//
//   DEFN: The generators are the free-values of fBody1,fBody2...
//  
//   What are the closure equations?
//
//   etps(f1,f2..)    includes free-tps(f)
//   etps(f1,f2..)    includes etps(g) if fBody has arity-met g-occurance (g TLR).
//
//   envReq(f1,f2...) includes ReqSubEnv(g) if fBody has arity-met   g-occurance (g TLR)
//   envReq(f1,f2...) includes ReqVal(g)    if fBody has arity-short g-occurance (g TLR)
//   envReq(f1,f2...) includes ReqVal(g)    if fBody has g-occurance (g not TLR)
//
//   and only collect requirements if g is a generator (see next notes).
//
//   Note: "env-availability"
//     In the translated code, env(h) will be defined at the h definition point.
//     So, where-ever h could be called (recursive or not),
//     the env(h) will be available (in scope).
//   
//   Note (subtle): "sub-env-requirement-only-for-generators"
//     If have an arity-met call to h inside fBody, but h is not a freevar for f,
//     then h does not contribute env(h) to env(f), the closure for f.
//     It is true that env(h) will be required at the h call-site,
//     but the env(h) will be available there (by "env-availability"),
//     since h must be bound inside the fBody since h was not a freevar for f.
//     .
//     [note, f and h may mutally recurse and formals of f may be in env(h),
//      so env(f) may be properly inside env(h),
//      so better not have env(h) in env(f)!!!].


/// The subset of ids from a mutal binding that are chosen to be TLR.
/// They share a common env.
/// [Each fclass has an env, the fclass are the handles to envs.]
type fclass =
    FC of FlatVals

let fclass_order ccu = orderOn (fun (FC fs) -> fs) (FlatList.order val_spec_order)
let fclassPairs ((FC fs) as fc) = fs |> FlatList.map (fun f -> (f,fc)) 
let memFC (f:Val) (FC fs) = fs |> FlatList.exists (fun v -> v.Stamp = f.Stamp) 
let isEmptyFC (FC fs) = FlatList.isEmpty fs
let showFC (FC fs) = "+" + String.concat "+" (FlatList.map name_of_val fs)

/// It is required to make the TLR closed wrt it's freevars (the env generators).
/// For g a generator,
///   An arity-met g occurance contributes the env required for that g call.
///   Other occurances contribute the value g.
type envItem =
    | ReqSubEnv of Val
    | ReqVal    of Val

let envItem_order ccu =
    let rep = function
        ReqSubEnv v -> true ,v
      | ReqVal    v -> false,v
   
    orderOn rep (Pair.order (Bool.order,val_spec_order))

/// An env says what is needed to close the corresponding defn(s).
/// The etps   are the free etps of the defns, and those required by any direct TLR arity-met calls.
/// The envReq are the ids/subEnvs required from calls to freeVars.
type env =
   { etps   : Typar Zset.set;
     envReq : envItem    Zset.set;
     m      : Range.range; }
   
let env0 ccu m =
    {etps   = Zset.empty typar_spec_order;
     envReq = Zset.empty (envItem_order ccu);
     m      = m }

let extendEnv (typars,items) env = 
     {env with
            etps   = Zset.addList typars env.etps;
            envReq = Zset.addList items  env.envReq}

let envSubEnvs env =
    let select = function ReqSubEnv f -> Some f | ReqVal _ -> None
    List.choose select (Zset.elements env.envReq)

let envVals env =
    let select = function ReqSubEnv f -> None | ReqVal f -> Some f
    List.choose select (Zset.elements env.envReq)
     
(*--debug-stuff--*)

let showEnvItem = function
  | ReqSubEnv f -> "&" ^ f.MangledName
  | ReqVal    f -> f.MangledName

let soEnv env =
    (showL (commaListL (List.map TyparL (Zset.elements (env.etps))))) ^ "--" ^ 
    (String.concat "," (List.map showEnvItem (Zset.elements env.envReq)))


//-------------------------------------------------------------------------
// pass2: collector - state
//-------------------------------------------------------------------------

type generators = Val Zset.set

/// check a named function value applied to sufficient arguments 
let IsArityMet (vref:ValRef)  wf tys args = 
  (List.length tys = vref.Typars.Length) && (wf <= List.length args) 


module Pass2_DetermineTLREnvs = 


    // IMPLEMENTATION PLAN:
    //
    // fold over expr.
    //
    // - at an instance g,
    //   - (a) g arity-met,   LogRequiredFrom g - ReqSubEnv(g) -- direct call will require env(g) and etps(g)
    //   - (b) g arity-short, LogRequiredFrom g - ReqVal(g)    -- remains g call
    //   - (c) g non-TLR,     LogRequiredFrom g - ReqVal(g)    -- remains g
    //  where
    //   LogRequiredFrom g ... = logs info into (generators,env) if g in generators.
    //
    // - at some mu-bindings, f1,f2... = fBody1,fBody2,...
    //  "note generators, push (generators,env), fold-over bodies, pop, fold rest"
    //   
    //  - let fclass = ff1,... be the fi which are being made TLR.
    //  - required to find an env for these.
    //  - start a new envCollector:
    //      freetps = freetypars of (fBody1,fBody2,...)
    //      freevs  = freevars   of ..
    //      initialise:
    //        etps       = freetps
    //        envReq     = []      -- info collected from generator occurances in bindings
    //        generators = freevs
    //  - fold bodies, collecting info for generators.
    //  - pop and save env.
    //    - note: - etps(fclass) are only the freetps
    //            - they need to include etps(g) for each direct call to g (g a generator for fclass)
    //            - the etps(g) may not yet be known,
    //              e.g. if we are inside the definition of g and had recursively called it.
    //            - so need to FIX up the etps(-) function when collected info for all fclass.
    //  - fold rest (after binding)
    //
    // fix up etps(-) according to direct call dependencies.
    //


    /// This state collects:
    ///   envM          - fclass -> env
    ///   fclassM       - f      -> fclass
    ///   declist       - fclass list
    ///   recShortCallS - the f which are "recursively-called" in arity short instance.
    ///
    /// When walking expr, at each mutual binding site,
    /// push a (generator,env) collector frame on stack.
    /// If occurances in body are relevant (for a generator) then it's contribution is logged.
    ///
    /// recShortCalls to f will require a binding for f in terms of fHat within the fHatBody.
    type state =
        { stack         : (fclass * generators * env) list;
          envM          : Zmap.map<fclass,env>;
          fclassM       : Zmap.map<Val,fclass>;
          revDeclist    : fclass list;
          recShortCallS : Zset.set<Val>;
        }

    let state0 ccu =
        { stack         = [];
          envM          = Zmap.empty (fclass_order ccu);
          fclassM       = Zmap.empty val_spec_order;
          revDeclist    = [];
          recShortCallS = Zset.empty val_spec_order; }

    let soVSet fs = (showL (commaListL (List.map valL (Zset.elements fs))))

    /// PUSH = start collecting for fclass 
    let PushFrame ccu fclass (etps0,generators,m) state =
        if isEmptyFC fclass then state else
          ( (if verboseTLR then dprintf "PushFrame: %s\n - generators = %s\n" (showFC fclass) (soVSet generators));
            {state with
               revDeclist = fclass :: state.revDeclist;
               stack = (let env = extendEnv (etps0,[]) (env0 ccu m) in (fclass,generators,env)::state.stack); })

    /// POP & SAVE = end collecting for fclass and store 
    let SaveFrame     fclass state = 
        if isEmptyFC fclass then state else
        if verboseTLR then dprintf "SaveFrame: %s\n" (showFC fclass);
        match state.stack with
        | []                             -> internalError "trl: popFrame has empty stack"
        | (fclass,generators,env)::stack -> (* ASSERT: same fclass *)
            {state with
               stack      = stack;
               envM       = Zmap.add  fclass env   state.envM;
               fclassM    = FlatList.fold (fun mp (k,v) -> Zmap.add k v mp) state.fclassM (fclassPairs fclass) }

    /// Log requirements for g in the relevant stack frames 
    let LogRequiredFrom g items state =
        let logIntoFrame (fclass,generators,env) =
           let env = 
               if Zset.mem g generators then
                   // dprintf "          : logging for generators=%s\n" (soVSet generators); 
                   let typars = []
                   extendEnv (typars,items) env
               else env
         
           fclass,generators,env
       
        {state with stack = List.map logIntoFrame state.stack}

    let LogShortCall g state =
        let frameFor g (fclass,generators,env) = memFC g fclass
        if List.exists (frameFor g) state.stack then
          ((if verboseTLR then dprintf "shortCall:     rec: %s\n" g.MangledName);
           (* Have short call to g within it's (mutual) definition(s) *)
           {state with
               recShortCallS = Zset.add g state.recShortCallS})
        else
          ((if verboseTLR then dprintf "shortCall: not-rec: %s\n" g.MangledName);
           state)

    let getEnv f state =
        match Zmap.tryfind f state.fclassM with
        | Some fclass -> (* env(f) is known, f prior *)
                         ((if verboseTLR then dprintf "getEnv: fclass=%s\n" (showFC fclass));
                          let env = forceM' fclass state.envM ("getEnv",showFC)
                          Some env)
        | None        -> (* env(f) unknown, perhaps in body of it's defn *)
                         None



    let FreeInBindings bs = FlatList.fold (foldOn (free_in_rhs CollectTyparsAndLocals) union_freevars) empty_freevars bs

    /// Intercepts selected exprs.
    ///   "letrec f1,f2,... = fBody1,fBody2,... in rest" - 
    ///   "val v"                                        - free occurance
    ///   "app (f,tps,args)"                             - occurance
    ///
    /// On intercepted nodes, must exprF fold to collect from subexpressions.
    let ExprEnvIntercept ccu (tlrS,arityM) exprF z expr = 
         let accInstance z (fvref,tps,args) (* f known local *) = 
             let f = deref_val fvref
             match Zmap.tryfind f arityM with
             
             | Some wf -> 
                 // f is TLR with arity wf 
                 if IsArityMet fvref wf tps args then
                     // arity-met call to a TLR g 
                     LogRequiredFrom f [ReqSubEnv f] z                 
                 else
                     // arity-short instance 
                     let z = LogRequiredFrom f [ReqVal f] z          
                     // LogShortCall - logs recursive short calls 
                     let z = LogShortCall f z                   
                     z
             
             | None    -> 
                 // f is non-TLR 
                 LogRequiredFrom f [ReqVal f] z                    
        
         let accBinds m z (binds: Bindings) =
             let tlrBs,nonTlrBs = binds |> FlatList.partition (fun b -> Zset.mem b.Var tlrS) 
             // For bindings marked TLR, collect implied env 
             let fclass = FC (vars_of_Bindings tlrBs)
             // what determines env? 
             let frees      = FreeInBindings tlrBs
             let etps0      = frees.FreeTyvars.FreeTypars   |> Zset.elements      (* put in env *)
             // occurances contribute to env 
             let generators = (frees.FreeLocals |> Zset.elements) 
             // tlrBs are not generators for themselves 
             let generators = List.filter (fun g -> not (memFC g fclass)) generators
             let generators = Zset.of_list val_spec_order generators
             // collect into env over bodies 
             let z          = PushFrame ccu fclass (etps0,generators,m) z
             let z          = FlatList.fold (foldOn rhs_of_bind exprF) z tlrBs
             let z          = SaveFrame     fclass z
             (* for bindings not marked TRL, collect *)
             let z          = FlatList.fold (foldOn rhs_of_bind exprF) z nonTlrBs
             z
        
         match expr with
         | TExpr_val (v,_,m) -> 
             let z = accInstance z (v,[],[])
             Some z
         | TExpr_op (TOp_lval_op (_,v),tys,args,m) -> 
             let z = accInstance z (v,[],[])
             let z = List.fold exprF z args
             Some z
         | TExpr_app (f,fty,tys,args,m) -> 
             let f,fty,tys,args,m = DestApp (f,fty,tys,args,m)
             match f with
             | TExpr_val (f,_,_) ->
                  // // YES: APP vspec tps args - log 
                 let z = accInstance z (f,tys,args)
                 let z = List.fold exprF z args
                 Some z
             | _ ->
                 (* NO: app, but function is not val - no log *)
                 None
         | TExpr_letrec (binds,body,m,_) -> 
             let z = accBinds m z binds
             let z = exprF z body
             Some z
         | TExpr_let    (bind,body,m,_) -> 
             let z = accBinds m z (FlatList.one bind)
             let z = exprF z body
             Some z
         | _ -> None (* NO: no intercept *)
        

    /// Initially, etps(fclass) = freetps(bodies).
    /// For each direct call to a g, a generator for fclass,
    /// Required to include the etps(g) in etps(fclass).
    let CloseEnvETps fclassM envM =
        if verboseTLR then dprintf "CloseEnvETps------\n";
        let etpsFor envM f =
            let fc  = forceM' f  fclassM ("etpsFor",name_of_val)
            let env = forceM' fc envM    ("etpsFor",showFC)       
            env.etps
       
        let closeStep envM changed fc env =
            let directCallFs   = envSubEnvs env
            let directCallETps = List.map (etpsFor envM) directCallFs
            let etps0 = env.etps
            let etps  = List.fold Zset.union etps0 directCallETps
            let changed = changed || (not (Zset.equal etps0 etps))
            let env   = {env with etps = etps}
            if verboseTLR then 
                dprintf "closeStep: fc=%30s nSubs=%d etps0=%s etps=%s\n" (showFC fc) directCallFs.Length (soTyparSet etps0) (soTyparSet etps);
                directCallFs |> List.iter (fun f    -> dprintf "closeStep: dcall    f=%s\n" f.MangledName)          
                directCallFs |> List.iter (fun f    -> dprintf "closeStep: dcall   fc=%s\n" (showFC (Zmap.find f fclassM)))
                directCallETps |> List.iter (fun etps -> dprintf "closeStep: dcall etps=%s\n" (soTyparSet etps0)) 
            changed,env
       
        let rec fixpoint envM =
            let changed = false
            let changed,envM = Zmap.fmap (closeStep envM) changed envM
            if changed then
                fixpoint envM
            else
                envM
       
        fixpoint envM

    let DumpEnvM envM =
        let dump fc env = dprintf "CLASS=%s\n env=%s\n" (showFC fc) (soEnv env)
        Zmap.iter dump envM

    let DetermineTLREnvs ccu (tlrS,arityM) expr =
        if verboseTLR then dprintf "DetermineTLREnvs------\n";
        let folder = {ExprFolder0 with exprIntercept = ExprEnvIntercept ccu (tlrS,arityM)}
        let z = state0 ccu
        let z = FoldImplFile folder z expr
        (* project *)
        let envM          = z.envM
        let fclassM       = z.fclassM
        let declist       = List.rev z.revDeclist
        let recShortCallS = z.recShortCallS
        (* diagnostic dump *)
        (if verboseTLR then DumpEnvM envM);
        (* close the etps under the subEnv reln *)
        let envM    = CloseEnvETps fclassM envM
        (* filter out trivial fclass - with no TLR defns *)
        let envM    = Zmap.remove (FC FlatList.empty) envM
        (* restrict declist to those with envM bindings (the non-trivial ones) *)
        let declist = List.filter (Zmap.mem_of envM) declist
        (* diagnostic dump *)
        if verboseTLR then
             DumpEnvM envM;
             declist |> List.iter (fun fc -> dprintf "Declist: %s\n" (showFC fc)) 
             recShortCallS |> Zset.iter (fun f -> dprintf "RecShortCall: %s\n" f.MangledName) 

        envM,fclassM,declist,recShortCallS 

//-------------------------------------------------------------------------
// step3: envPack
//-------------------------------------------------------------------------

/// Each env is represented by some carrier values, the aenvs.
/// An env packing defines these, and the pack/unpack bindings.
/// The bindings are in terms of the fvs directly.
///
/// When defining a new TLR f definition,
///   the fvs   will become bound by the unpack bindings,
///   the aenvs will become bound by the new lam, and
///   the etps  will become bound by the new LAM.
/// For uniqueness of bound ids,
///   all these ids (Typar/Val) will need to be freshened up.
/// It is OK to break the uniqueness-of-bound-ids rule during the rw,
/// provided it is fixed up via a copy_expr call on the final expr.

type envPack =
    { /// The actual typars            
      ep_etps   : typars; 
      /// The actual env carrier values 
      ep_aenvs  : Val   list; 
      /// Sequentially define the aenvs in terms of the fvs   
      ep_pack   : Bindings;       
      /// Sequentially define the fvs   in terms of the aenvs 
      ep_unpack : Bindings;       
    }


//-------------------------------------------------------------------------
// step3: FlatEnvPacks
//-------------------------------------------------------------------------

exception AbortTLR of Range.range

/// A naive packing of environments.
/// Chooses to pass all env values as explicit args (no tupling).
/// Note, tupling would cause an allocation,
/// so, unless arg lists get very long, this flat packing will be preferable.

/// Given (fclass,env).
/// Have env = ReqVal vj, ReqSubEnv subEnvk -- ranging over j,k
/// Define vals(env) = {vj}|j union vals(subEnvk)|k -- trans closure of vals of env.
/// Define <vi,aenvi> for each vi in vals(env).
///  This is the cmap for the env.

///  etps     = env.etps
///  carriers = aenvi|i
///  pack     = TBIND(aenvi = vi)            for each (aenvi,vi) in cmap
///  unpack   = TBIND(vj = aenvFor(vj))      for each vj in reqvals(env).
///         and TBIND(asubEnvi = aenvFor(v)) for each (asubEnvi,v) in cmap(subEnvk) ranging over required subEnvk.
/// where
///   aenvFor(v) = aenvi where (v,aenvi) in cmap.
let FlatEnvPacks g ccu fclassM topValS declist envM =
   let fclassOf f = forceM' f fclassM ("fclassM",name_of_val)
   let packEnv carrierMaps (fc:fclass) =
       if verboseTLR then dprintf "\ntlr: packEnv fc=%s\n" (showFC fc);
       let env = forceM' fc envM ("packEnv",showFC)

       // carrierMaps = (fclass,(v,aenv)map)map 
       let carrierMapFor f = forceM' (fclassOf f) carrierMaps ("carrierMapFor",showFC)
       let valsSubEnvFor f = Zmap.keys (carrierMapFor f)

       // determine vals(env) - transclosure 
       let vals = envVals env @ List.collect valsSubEnvFor (envSubEnvs env) in // list, with repeats 
       let vals = List.noRepeats val_spec_order vals                                 // noRepeats 
       let vals = vals |> FlatList.of_list

       // Remove genuinely toplevel, need not close over 
       let vals = vals |> FlatList.filter (IsMandatoryTopLevel >> not) 
       let vals = vals |> FlatList.filter (Zset.mem_of topValS >> not) 
       
       // Carrier sets cannot include constrained polymorphic values. We can't just take such a value out, so for the moment 
       // we'll just abandon TLR altogether and give a warning about this condition. 
       (match vals |> FlatList.tryfind (IsGenericValWithGenericContraints g) with None -> () | Some v -> raise (AbortTLR v.Range));

       // build cmap for env 
       let cmapPairs = vals |> FlatList.mapi (fun i v -> (v,(mk_compgen_local env.m v.MangledName v.Type |> fst))) 
       let cmap      = Zmap.of_FlatList val_spec_order cmapPairs
       let aenvFor     v = forceM' v cmap ("aenvFor",name_of_val)
       let aenvExprFor v = expr_for_val env.m (aenvFor v)

       // build envPack 
       let etps   = env.etps
       let aenvs  = Zmap.values cmap
       let pack   = cmapPairs |> FlatList.map (fun (v,aenv) -> mk_invisible_bind aenv (expr_for_val env.m v))
       let unpack = 
           let unpackCarrier (v,aenv) = mk_invisible_bind (set_val_has_no_arity v) (expr_for_val env.m aenv)
           let unpackSubenv f = 
               let subCMap  = carrierMapFor f    
               let vaenvs   = Zmap.to_list subCMap
               vaenvs |> List.map (fun (subv,subaenv) -> mk_bind NoSequencePointAtInvisibleBinding subaenv (aenvExprFor subv))
           List.map unpackCarrier (Zmap.to_list cmap) @
           List.collect unpackSubenv (envSubEnvs env)
      
       // extend carrierMaps 
       let carrierMaps = Zmap.add fc cmap carrierMaps

       // dump 
       if verboseTLR then
           dprintf "tlr: packEnv envVals =%s\n" (showL (listL valL  (envVals env)));
           dprintf "tlr: packEnv envSubs =%s\n" (showL (listL valL  (envSubEnvs env)));
           dprintf "tlr: packEnv vals    =%s\n" (showL (listL valL (FlatList.to_list vals)));
           dprintf "tlr: packEnv aenvs   =%s\n" (showL (listL valL aenvs));
           dprintf "tlr: packEnv pack    =%s\n" (showL (listL BindingL  (FlatList.to_list pack)));
           dprintf "tlr: packEnv unpack  =%s\n" (showL (listL BindingL  unpack))

       // result 
       carrierMaps,
       (fc, { ep_etps   = Zset.elements etps;
              ep_aenvs  = aenvs;
              ep_pack   = pack;
              ep_unpack = FlatList.of_list unpack})
  
   let carriedMaps = Zmap.empty (fclass_order ccu)
   let carriedMaps,envPacks = List.fmap packEnv carriedMaps declist   (* List.fmap in dec order *)
   let envPacks = Zmap.of_list (fclass_order ccu) envPacks
   envPacks


(*-------------------------------------------------------------------------
 * step3: chooseEnvPacks
 *-------------------------------------------------------------------------*)

let DumpEnvPackM envPackM =
    let dump fc envPack =
        dprintf "envPack: fc     = %s\n" (showFC fc);
        dprintf "         etps   = %s\n" (showL (commaListL (List.map TyparL envPack.ep_etps)));
        dprintf "         aenvs  = %s\n" (showL (commaListL (List.map valL envPack.ep_aenvs)));
        dprintf "         pack   = %s\n" (showL (semiListL (FlatList.to_list (FlatList.map BindingL envPack.ep_pack))));
        dprintf "         unpack = %s\n" (showL (semiListL (FlatList.to_list (FlatList.map BindingL envPack.ep_unpack))));
        dprintf "\n"
  
    Zmap.iter dump envPackM

/// For each fclass, have an env.
/// Required to choose an envPack,
/// e.g. deciding whether to tuple up the environment or not.
/// e.g. deciding whether to use known values for required sub environments.
///
/// Scope for optimisating env packing here.
/// For now, pass all environments via arguments since aiming to eliminate allocations.
/// Later, package as tuples if arg lists get too long.
let ChooseEnvPackings g ccu fclassM topValS  declist envM =
    if verboseTLR then dprintf "ChooseEnvPackings------\n";
    let envPackM = FlatEnvPacks g ccu fclassM topValS  declist envM
    let envPackM : (fclass,envPack) Zmap.map = envPackM
    if verboseTLR then DumpEnvPackM envPackM;
    envPackM


//-------------------------------------------------------------------------
// step3: CreateFHatM
//-------------------------------------------------------------------------

/// arity info where nothing is untupled 
(* REVIEW: could do better here by preserving names *)
let MkSimpleArityInfo tps n = TopValInfo (TopValInfo.InferTyparInfo tps,List.replicate n TopValInfo.unnamedTopArg,TopValInfo.unnamedRetVal)

let CreateFHatM g ccu tlrS arityM fclassM envPackM = 
    if verboseTLR then dprintf "CreateFHatM------\n";
    let createFHat (f:Val) =
        let wf     = forceM' f arityM ("createFHat - wf",(fun v -> showL (valL v)))
        let fc     = forceM' f fclassM ("createFHat - fc",name_of_val)
        let envp   = forceM' fc envPackM ("CreateFHatM - envp",showFC)
        let name   = f.MangledName (* ^ "_TLR_" ^ string wf *)
        let m      = f.Range
        let tps,tau    = f.TypeScheme
        let argtys,res = strip_fun_typ g tau
        let newTps    = envp.ep_etps @ tps
        let fHatTy = 
            let newArgtys = List.map type_of_val envp.ep_aenvs @ argtys
            mk_lambda_ty newTps newArgtys res
        let fHatArity = MkSimpleArityInfo newTps (List.length envp.ep_aenvs + wf)
        let fHatName =  globalNng.FreshCompilerGeneratedName(name,m)

        let fHat = mkLocalNameTypeArity f.IsCompilerGenerated m fHatName fHatTy (Some fHatArity)
        (if verboseTLR then dprintf "new %50s : %s\n" fHat.MangledName ((DebugPrint.showType fHat.Type)));
        fHat
   
    let fs     = Zset.elements tlrS
    let ffHats = List.map (fun f -> f,createFHat f) fs
    let fHatM  = Zmap.of_list val_spec_order ffHats
    fHatM


//-------------------------------------------------------------------------
// pass4: rewrite - penv
//-------------------------------------------------------------------------

type penv =
   { ccu           : ccu;
     g             : Env.TcGlobals;
     tlrS          : Zset.set<Val> ;
     topValS       : Zset.set<Val> ;
     arityM        : Zmap.map<Val,int> ;
     fclassM       : Zmap.map<Val,fclass> ;
     recShortCallS : Zset.set<Val> ;
     envPackM      : Zmap.map<fclass,envPack>;
     /// The mapping from 'f' values to 'fHat' values
     fHatM         : Zmap.map<Val,Val> ;
   }


//-------------------------------------------------------------------------
// pass4: rwstate (z state)
//-------------------------------------------------------------------------

type IsRecursive   = IsRec | NotRec
type LiftedDeclaration  = IsRecursive * Bindings (* where bool=true if letrec *)    

/// This state is related to lifting to top-level (which is actually disabled right now)
/// This is to ensure the TLR constants get initialised once.
///
/// Top-level status ends when stepping inside a lambda, where a lambda is:
///   TExpr_tlambda, TExpr_lambda, TExpr_obj (and tmethods).
///   [... also, try_catch handlers, and switch targets...]
///
/// Top* repr bindings already at top-level do not need moving...
///   [and should not be, since they may lift over unmoved defns on which they depend].
/// Any TLR repr bindings under lambdas can be filtered out (and collected),
/// giving pre-declarations to insert before the outermost lambda expr.
type rwstate =
    { rws_mustinline: bool;
      /// counts level of enclosing "lambdas"  
      rws_innerLevel : int;        
      /// collected preDecs (fringe is in-order) 
      rws_preDecs    : tree<LiftedDeclaration>  
    }

let rws0 = {rws_mustinline=false;rws_innerLevel=0;rws_preDecs=emptyTR}

// move in/out of lambdas (or lambda containing construct) 
let EnterInner z = {z with rws_innerLevel = z.rws_innerLevel + 1}
let ExitInner  z = {z with rws_innerLevel = z.rws_innerLevel - 1}

let EnterMustInline b z f = 
    let orig = z.rws_mustinline
    let z',x = f (if b then {z with rws_mustinline = true } else z)
    {z' with rws_mustinline = orig },x

/// extract PreDecs (iff at top-level) 
let ExtractPreDecs z =
    // If level=0, so at top-level, then pop decs,
    // else keep until get back to a top-level point.
    if z.rws_innerLevel=0 then
      // at top-level, extract preDecs 
      let preDecs = fringeTR z.rws_preDecs
      {z with rws_preDecs=emptyTR}, preDecs
    else 
      // not yet top-level, keep decs 
      z,[]

/// pop and set preDecs  as "LiftedDeclaration tree" 
let PopPreDecs z     = {z with rws_preDecs=emptyTR},z.rws_preDecs
let SetPreDecs z pdt = {z with rws_preDecs=pdt}

/// collect Top* repr bindings - if needed... 
let LiftTopBinds isRec penv z binds =
    let isTopBind bind = isSome (chosen_arity_of_bind bind)
    let topBinds,otherBinds = FlatList.partition isTopBind binds
    let liftTheseBindings =
        !liftTLR &&             // lifting enabled 
        not z.rws_mustinline &&   // can't lift bindings in a mustinline context - they would become private an not inlined 
        z.rws_innerLevel>0 &&   // only collect Top* bindings when at inner levels (else will drop them!) 
        not (FlatList.isEmpty topBinds) // only collect topBinds if there are some! 
   
    if liftTheseBindings then
        let LiftedDeclaration = isRec,topBinds                                           // LiftedDeclaration Top* decs 
        let z = {z with rws_preDecs = TreeNode [z.rws_preDecs;LeafNode LiftedDeclaration]}   // logged at end 
        z,otherBinds
    else
        z,binds (* not "topBinds @ otherBinds" since that has changed order... *)
      
/// Wrap preDecs (in order) over an expr - use letrec/let as approp 
let MakePreDec  m (isRec,binds) expr = 
    if isRec=IsRec then 
        mk_letrec_binds m binds expr
    else 
        mk_lets_from_Bindings  m binds expr

let MakePreDecs m preDecs expr = List.foldBack (MakePreDec m) preDecs expr

let RecursivePreDecs pdsA pdsB =
    let pds = fringeTR (TreeNode[pdsA;pdsB])
    let decs = pds |> List.collect (fun (_,x) -> FlatList.to_list x)  |> FlatList.of_list
    LeafNode (IsRec,decs)


(*-------------------------------------------------------------------------
 * pass4: lowertop - convert_vterm_bind on TopLevel binds
 *-------------------------------------------------------------------------*)

let ConvertBind g (TBind(v,repr,_) as bind)  =
    match v.TopValInfo with 
    | None -> v.Data.val_top_repr_info <- Some (InferArityOfExprBinding g v repr )
    | Some _ -> ()
    
    bind

(*-------------------------------------------------------------------------
 * pass4: transBind (translate)
 *-------------------------------------------------------------------------*)

// Transform
//   let f<tps> vss = f_body[<f_freeTypars>,f_freeVars]
// To
//   let f<tps> vss = fHat<f_freeTypars> f_freeVars vss
//   let fHat<tps> f_freeVars vss = f_body[<f_freeTypars>,f_freeVars]
let TransTLRBindings penv (binds:Bindings) = 
    if FlatList.isEmpty binds then FlatList.empty,FlatList.empty else
    let fc   = FC (vars_of_Bindings binds)
    let envp = forceM' fc penv.envPackM ("TransTLRBindings",showFC)
    
    let fRebinding (TBind(fOrig,b,letSeqPtOpt)) =
        let m = fOrig.Range
        let tps,vss,b,rty = dest_top_lambda (b,fOrig.Type)
        let aenvExprs = envp.ep_aenvs |> List.map (expr_for_val m) 
        let vsExprs   = vss |> List.map (mk_tupled_vars penv.g m) 
        let fHat      = forceM' fOrig penv.fHatM ("fRebinding",name_of_val) 
        let w         = 0    
        (* REVIEW: is this mutation really, really necessary? *)
        (* Why are we applying TLR if the thing already has an arity? *)
        let fOrig = set_val_has_no_arity fOrig
        let fBind = 
           mk_multi_lambda_bind fOrig letSeqPtOpt m tps vss 
               (mk_appl penv.g 
                        (typed_expr_for_val m fHat,
                         [List.map mk_typar_ty (envp.ep_etps @ tps)],
                         aenvExprs @ vsExprs,m),rty)
        fBind                                

    let fHatNewBinding (shortRecBinds:Bindings) (TBind(f,b,letSeqPtOpt)) =
        let wf   = forceM' f penv.arityM ("fHatNewBinding - arityM",name_of_val)
        let fHat = forceM' f penv.fHatM  ("fHatNewBinding - fHatM",name_of_val)
        // Take off the variables
        let tps,vss,b,rty = dest_top_lambda (b,f.Type)
        // Don't take all the variables - only up to length wf
        let vssTake,vssDrop = List.chop wf vss
        // put the variables back on
        let b,rty = mk_multi_lambdas_core (range_of_expr b) vssDrop (b,rty)
        // fHat, args 
        let m = fHat.Range
        // Add the type variables to the front
        let fHat_tps  = envp.ep_etps @ tps
        // Add the 'aenv' and original taken variables to the front
        let fHat_args = List.map List.singleton envp.ep_aenvs @ vssTake
        let fHat_body = mk_lets_from_Bindings m envp.ep_unpack b        
        let fHat_body = mk_lets_from_Bindings m shortRecBinds  fHat_body  // bind "f" if have short recursive calls (somewhere) 
        // fHat binding, f rebinding 
        let wfHat      = List.length envp.ep_aenvs + wf
        let fHatBind   = mk_multi_lambda_bind fHat letSeqPtOpt m fHat_tps fHat_args (fHat_body,rty)
        fHatBind
    let rebinds = binds |> FlatList.map fRebinding 
    let shortRecBinds = rebinds |> FlatList.filter (fun b -> penv.recShortCallS.Contains(b.Var)) 
    let newBinds      = binds |> FlatList.map (fHatNewBinding shortRecBinds) 
    newBinds,rebinds

let GetAEnvBindings penv fc =
    match Zmap.tryfind fc penv.envPackM with
    | None      -> FlatList.empty           // no env for this mutual binding 
    | Some envp -> envp.ep_pack // environment pack bindings 

let TransBindings xisRec penv (binds:Bindings) =
    let tlrBs,nonTlrBs = binds |> FlatList.partition (fun b -> Zset.mem b.Var penv.tlrS) 
    let fclass = FC (vars_of_Bindings tlrBs)
    // Trans each TLR f binding into fHat and f rebind 
    let newTlrBinds,tlrRebinds = TransTLRBindings penv tlrBs
    let aenvBinds = GetAEnvBindings penv fclass
    // lower nonTlrBs if they are GTL 
    // QUERY: we repeat this logic in Lowertop.  Do we really need to do this here? 
    // QUERY: yes and no - if we don't, we have an unrealizable term, and many decisions must 
    // QUERY: correlate with Lowertop.  
    let forceTopBindToHaveArity (bind:Binding) = 
        if penv.topValS.Contains(bind.Var) then ConvertBind penv.g bind 
        else bind

    let nonTlrBs = nonTlrBs |> FlatList.map forceTopBindToHaveArity 
    let tlrRebinds = tlrRebinds |> FlatList.map forceTopBindToHaveArity 
    // assemble into replacement bindings 
    let bindAs,rebinds = 
        match xisRec with
        | IsRec  -> FlatList.to_list newTlrBinds @ FlatList.to_list tlrRebinds @ FlatList.to_list nonTlrBs @ FlatList.to_list aenvBinds,[]    (* note: aenv last, order matters in letrec! *)
        | NotRec -> FlatList.to_list aenvBinds @ FlatList.to_list newTlrBinds, FlatList.to_list tlrRebinds @ FlatList.to_list nonTlrBs (* note: aenv go first, they may be used *)
    FlatList.of_list bindAs, FlatList.of_list rebinds


(*-------------------------------------------------------------------------
 * pass4: TransApp (translate)
 *-------------------------------------------------------------------------*)

let TransApp penv (fx,fty,tys,args,m) =
    // Is it a val app, where the val f is TLR with arity wf? 
    // CLEANUP NOTE: should be using a mk_appl to make all applications 
    match fx with
    | TExpr_val (fvref,_,m) when 
            (Zset.mem (deref_val fvref) penv.tlrS) &&
            (let wf = forceM' (deref_val fvref) penv.arityM ("TransApp - wf",name_of_val)
             IsArityMet fvref wf tys args) ->

               let f = deref_val fvref
               (* replace by direct call to corresponding fHat (and additional closure args) *)
               let fc   = forceM' f  penv.fclassM ("TransApp - fc",name_of_val)in   
               let envp = forceM' fc penv.envPackM ("TransApp - envp",showFC)
               let fHat = forceM' f  penv.fHatM ("TransApp - fHat",name_of_val)in
               let tys  = (List.map mk_typar_ty envp.ep_etps) @ tys
               let aenvExprs = List.map (expr_for_val m) envp.ep_aenvs
               let args = aenvExprs @ args
               mk_appl penv.g (typed_expr_for_val m fHat,[tys],args,m) (* change, direct fHat call with closure (etps,aenvs) *)
    | _ -> 
        if isNil tys && isNil args then 
            fx 
        else TExpr_app (fx,fty,tys,args,m)
                          (* no change, f is expr *)

(*-------------------------------------------------------------------------
 * pass4: pass (over expr)
 *-------------------------------------------------------------------------*)

/// Must WrapPreDecs around every construct that could do EnterInner (which filters TLR decs).
/// i.e. let,letrec (bind may...), ilobj, lambda, tlambda.
let WrapPreDecs m pds x =
    MakePreDecs m pds x

/// At bindings, fixup any TLR bindings.
/// At applications, fixup calls  if they are arity-met instances of TLR.
/// At free vals,    fixup 0-call if it is an arity-met constant.
/// Other cases rewrite structurally.
let rec TransExpr (penv:penv) z expr =
    match expr with
    // Use TransLinearExpr with a rebuild-continuation for some forms to avoid stack overflows on large terms *)
    | TExpr_letrec _ | TExpr_let    _ -> 
         TransLinearExpr penv z expr (fun res -> res)

    // app - call sites may require z.
    //     - match the app (collapsing reclinks and type instances).
    //     - patch it.
    | TExpr_app (f,fty,tys,args,m) ->
       // pass over f,args subexprs 
       let z,f      = TransExpr penv z f
       let z,args = List.fmap (TransExpr penv) z args
       // match app, and fixup if needed 
       let f,fty,tys,args,m = DestApp (f,fty,tys,args,m)
       let expr = TransApp penv (f,fty,tys,args,m)
       z,expr

    | TExpr_val (v,_,m) ->
       // consider this a trivial app 
       let fx,fty = expr,v.Type
       let expr = TransApp penv (fx,fty,[],[],m)
       z,expr

    // reclink - suppress 
    | TExpr_link r ->
        TransExpr penv z (!r)

    // ilobj - has implicit lambda exprs and recursive/base references 
    | TExpr_obj (_,ty,basev,basecall,overrides,iimpls,m,_) ->
        let z,basecall  = TransExpr penv                            z basecall 
        let z,overrides = List.fmap (TransMethod penv)                  z overrides
        let z,iimpls    = List.fmap (fmap2'2 (List.fmap (TransMethod penv))) z iimpls   
        let expr = TExpr_obj(new_uniq(),ty,basev,basecall,overrides,iimpls,m,SkipFreeVarsCache())
        let z,pds = ExtractPreDecs z
        z,WrapPreDecs m pds expr (* if TopLevel, lift preDecs over the ilobj expr *)

    // lambda, tlambda - explicit lambda terms 
    | TExpr_lambda(_,basevopt,argvs,body,m,rty,_) ->
        let z = EnterInner z
        let z,body = TransExpr penv z body
        let z = ExitInner z
        let z,pds = ExtractPreDecs z
        z,WrapPreDecs m pds (mk_basev_multi_lambda m basevopt argvs (body,rty))

    | TExpr_tlambda(_,argtyvs,body,m,rty,_) ->
        let z = EnterInner z
        let z,body = TransExpr penv z body
        let z = ExitInner z
        let z,pds = ExtractPreDecs z
        z,WrapPreDecs m pds (mk_tlambda m argtyvs (body,rty))

    /// Lifting TLR out over constructs (disabled)
    /// Lift minimally to ensure the defn is not lifted up and over defns on which it depends (disabled)
    | TExpr_match(spBind,exprm,dtree,targets,m,ty,_) ->
        let targets = Array.to_list targets
        let z,dtree   = TransDecisionTree penv z dtree
        let z,targets = List.fmap (TransDecisionTreeTarget penv) z targets
        // TransDecisionTreeTarget wraps EnterInner/exitInnter, so need to collect any top decs 
        let z,pds = ExtractPreDecs z
        z,WrapPreDecs m pds (mk_and_optimize_match spBind exprm m ty dtree targets)

    // all others - below - rewrite structurally - so boiler plate code after this point... 
    | TExpr_const _ -> z,expr (* constant wrt Val *)
    | TExpr_quote (a,{contents=Some(argTypes,argExprs,data)},m,ty) -> 
        let z,argExprs = List.fmap (TransExpr penv) z argExprs
        z,TExpr_quote(a,{contents=Some(argTypes,argExprs,data)},m,ty)
    | TExpr_quote (a,{contents=None},m,ty) -> 
        z,TExpr_quote(a,{contents=None},m,ty)
    | TExpr_op (c,tyargs,args,m) -> 
        let z,args = List.fmap (TransExpr penv) z args
        z,TExpr_op(c,tyargs,args,m)
    | TExpr_seq (e1,e2,dir,spSeq,m) -> 
        let z,e1 = TransExpr penv z e1
        let z,e2 = TransExpr penv z e2
        z,TExpr_seq(e1,e2,dir,spSeq,m)
    | TExpr_static_optimization (constraints,e2,e3,m) ->
        let z,e2 = TransExpr penv z e2
        let z,e3 = TransExpr penv z e3
        z,TExpr_static_optimization(constraints,e2,e3,m)
    | TExpr_tchoose (_,_,m) -> error(Error("Unexpected TExpr_tchoose",m))

/// Walk over linear structured terms in tail-recursive loop, using a continuation 
/// to represent the rebuild-the-term stack 
and TransLinearExpr penv z expr contf =
    match expr with 
     // letrec - pass_recbinds does the work 
     | TExpr_letrec (binds,e,m,_) ->
         let z = EnterInner z
         // For letrec, preDecs from RHS must mutually recurse with those from the bindings 
         let z,pdsPrior    = PopPreDecs z
         let z,binds       = FlatList.fmap (TransBindingRhs penv) z binds
         let z,pdsRhs      = PopPreDecs z
         let binds,rebinds = TransBindings   IsRec penv binds
         let z,binds       = LiftTopBinds IsRec penv z   binds in  (* factor Top* repr binds *)
         let z,rebinds     = LiftTopBinds IsRec penv z rebinds
         let z,pdsBind     = PopPreDecs z
         let z             = SetPreDecs z (TreeNode [pdsPrior;RecursivePreDecs pdsBind pdsRhs])
         let z = ExitInner z
         let z,pds = ExtractPreDecs z
         TransLinearExpr penv z e (contf << (fun (z,e) -> 
             let e = mk_lets_from_Bindings m rebinds e
             z,WrapPreDecs m pds (TExpr_letrec (binds,e,m,NewFreeVarsCache()))))

     // let - can consider the mu-let bindings as mu-letrec bindings - so like as above 
     | TExpr_let    (bind,e,m,_) ->

         // For let, preDecs from RHS go before those of bindings, which is collection order 
         let z,bind       = TransBindingRhs penv z bind
         let binds,rebinds = TransBindings   NotRec penv (FlatList.one bind)
         // factor Top* repr binds 
         let z,binds       = LiftTopBinds NotRec penv z   binds  
         let z,rebinds     = LiftTopBinds NotRec penv z rebinds
         // any lifted PreDecs from binding, if so wrap them... 
         let z,pds = ExtractPreDecs z
         TransLinearExpr penv z e (contf << (fun (z,e) -> 
             let e = mk_lets_from_Bindings m rebinds e
             z,WrapPreDecs m pds (mk_lets_from_Bindings m binds e)))

     | _ -> 
        contf (TransExpr penv z expr)
  
and TransMethod penv z (TObjExprMethod(slotsig,tps,vs,e,m)) =
    let z = EnterInner z 
    let z,e = TransExpr penv z e
    let z = ExitInner z 
    z,TObjExprMethod(slotsig,tps,vs,e,m)

and TransBindingRhs penv z (TBind(v,e,letSeqPtOpt)) = 
    let mustInline = v.MustInline
    let z,e = EnterMustInline mustInline z (fun z -> TransExpr penv z e)
    z,TBind (v,e,letSeqPtOpt)

and TransDecisionTree penv z x =
   match x with 
   | TDSuccess (es,n) -> 
       let z,es = FlatList.fmap (TransExpr penv) z es
       z,TDSuccess(es,n)
   | TDBind (bind,rest) -> 
       let z,bind       = TransBindingRhs penv z bind
       let z,rest = TransDecisionTree penv z rest
       z,TDBind(bind,rest)
   | TDSwitch (e,cases,dflt,m) ->
       let z,e = TransExpr penv z e
       let TransDecisionTreeCase penv z (TCase (discrim,dtree)) =
           let z,dtree = TransDecisionTree penv z dtree
           z,TCase(discrim,dtree)
      
       let z,cases = List.fmap (TransDecisionTreeCase penv) z cases
       let z,dflt  = Option.fmap (TransDecisionTree penv)      z dflt
       z,TDSwitch (e,cases,dflt,m)

and TransDecisionTreeTarget penv z (TTarget(vs,e,spTarget)) =
    let z = EnterInner z 
    let z,e = TransExpr penv z e
    let z = ExitInner z
    z,TTarget(vs,e,spTarget)

and TransValBinding penv z bind = TransBindingRhs penv z bind 
and TransValBindings penv z binds = FlatList.fmap (TransValBinding penv) z  binds
and TransModuleExpr penv z x = 
    match x with  
    | TMTyped(mty,def,m) ->  
        let z,def = TransModuleDef penv z def
        z,TMTyped(mty,def,m)
    
and TransModuleDefs penv z x = List.fmap (TransModuleDef penv) z x
and TransModuleDef penv (z:rwstate) x = 
    match x with 
    | TMDefRec(tycons,binds,mbinds,m) -> 
        let z,binds = TransValBindings penv z binds
        let z,mbinds = TransModuleBindings penv z mbinds
        z,TMDefRec(tycons,binds,mbinds,m)
    | TMDefLet(bind,m)            -> 
        let z,bind = TransValBinding penv z bind
        z,TMDefLet(bind,m)
    | TMDefDo(e,m)            -> 
        let z,bind = TransExpr penv z e
        z,TMDefDo(e,m)
    | TMDefs(defs)   -> 
        let z,defs = TransModuleDefs penv z defs
        z,TMDefs(defs)
    | TMAbstract(mexpr) -> 
        let z,mexpr = TransModuleExpr penv z mexpr
        z,TMAbstract(mexpr)
and TransModuleBindings penv z binds = List.fmap (TransModuleBinding penv) z  binds
and TransModuleBinding penv z (TMBind(nm, rhs)) =
    let z,rhs = TransModuleDef penv z rhs
    z,TMBind(nm,rhs)

let TransImplFile penv z mv = fmapTImplFile (TransModuleExpr penv) z mv

let TransAssembly penv z (TAssembly(mvs)) = 
    let z,mvs = List.fmap (TransImplFile penv) z mvs 
    TAssembly(mvs)
(*-------------------------------------------------------------------------
 * pass5: copy_expr
 *-------------------------------------------------------------------------*)

let RecreateUniqueBounds g expr = 
    copy_ImplFile g OnlyCloneExprVals expr

(*-------------------------------------------------------------------------
 * entry point
 *-------------------------------------------------------------------------*)

let MakeTLRDecisions ccu g expr =
   try
      // pass1: choose the f to be TLR with arity(f) 
      let tlrS,topValS, arityM = Pass1_DetermineTLRAndArities.DetermineTLRAndArities ccu g expr

      // pass2: determine the typar/freevar closures, f->fclass and fclass declist 
      let envM,fclassM,declist,recShortCallS = Pass2_DetermineTLREnvs.DetermineTLREnvs ccu (tlrS,arityM) expr

      // pass3
      let envPackM = ChooseEnvPackings g ccu fclassM topValS  declist envM
      let fHatM    = CreateFHatM g ccu tlrS arityM fclassM envPackM

      // pass4: rewrite 
      if verboseTLR then dprintf "TransExpr(rw)------\n";
      let penv = {ccu=ccu; g=g; tlrS=tlrS; topValS=topValS; arityM=arityM; fclassM=fclassM; recShortCallS=recShortCallS; envPackM=envPackM; fHatM=fHatM}
      let z = rws0
      let _,expr = TransImplFile penv z expr

      // pass5: copy_expr to restore "each bound is unique" property 
      // aka, copy_expr 
      if verboseTLR then dprintf "copy_expr------\n";
      let expr = RecreateUniqueBounds g expr 
      if verboseTLR then dprintf "TLR-done------\n";

      // Summary:
      //   GTL = genuine top-level
      //   TLR = TopLevelRep = identified by this pass
      //   Note, some GTL are skipped until sort out the initial env...
      // if verboseTLR then dprintf "note: tlr = %d inner-TLR + %d GenuineTopLevel-TLR + %d GenuineTopLevel skipped TLR (public)\n"
      //  (lengthS (Zset.diff  tlrS topValS))
      //  (lengthS (Zset.inter topValS tlrS))
      //  (lengthS (Zset.diff  topValS tlrS))
      
      // DONE 
      expr
   with AbortTLR m -> 
       warning(Error("Note: Lambda-lifting optimizations have not been applied because of the use of this local constrained generic function as a first class value. Adding type constraints may resolve this condition",m));
       expr
