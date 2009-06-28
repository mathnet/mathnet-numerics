
#light

module internal Microsoft.FSharp.Compiler.PostTypecheckSemanticChecks

open System.Collections.Generic
open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.IL
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
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.Typrelns
open Microsoft.FSharp.Compiler.Infos

//--------------------------------------------------------------------------
// TestHooks - for dumping range to support source transforms
//--------------------------------------------------------------------------

let testFlagMemberBody = ref false
let testHookMemberBody membInfo expr =
    if !testFlagMemberBody then
        let m = range_of_expr expr in
        printf "TestMemberBody,%A,%s,%d,%d,%d,%d\n"
          (membInfo.MemberFlags.MemberKind)          
          (file_of_range m)
          (start_line_of_range m)
          (start_col_of_range m)
          (end_line_of_range m)
          (end_col_of_range m)

//--------------------------------------------------------------------------
// NOTES: byref safety checks
//--------------------------------------------------------------------------

(*
  The .NET runtime has safety requirements on the use of byrefs.
  These include:
    A1: No generic type/method can be instantiated with byref types (meaning contains byref type).
    A2: No object field may be byref typed.

  In F# TAST level, byref types can be introduced/consumed at:
    B1: lambda ... (v:byref<a>) ...         -- binding sites for values.
    B2: &m                                  -- address of operator, where m is local mutable or reference cell.
    B3: ms.M()                              -- method calls on mutable structs.
    B4: *br                                 -- dereference byref
    B5: br <- x                             -- assign byref
    B6: expr@[byrefType]                    -- any type instantiation could introduce byref types.
    B7: asm                                 -- TExpr_asm forms that create/consume byrefs.
        a) I_ldfld <byref> expr
        b) I_stfld <byref>
        c) others TBD... work in progress.

  Closures imply objects.
  Closures are either:
    a) explicit lambda expressions.
    b) functions partially applied below their known arity.
 
  Checks:
    C1: check no instantiation can contain byref types.
    C2: check type declarations to ensure no object field will have byref type.
    C3: check no explicit lambda expressions capture any free byref typed expression.    
    C4: check byref type expr occur only as:
        C4.a) arg to functions occuring within their known arity.
        C4.b) arg to IL method calls, e.g. arising from calls to instance methods on mutable structs.
        C4.c) arg to property getter on mutable struct (record field projection)
        C4.d) rhs of byref typed binding (aliasing).
              Note [1] aliasing should not effect safety. The restrictions on RHS byref will also apply to alias.
              Note [2] aliasing happens in the generated hash/compare code.
    C5: when is a byref-typed-binding acceptable?    
        a) if it will be a method local, ok.
        b) if it will be a top-level value stored as a field, then no. [These should have arity info].

  Check commentary:
    The C4 checks ensure byref expressions are only passed directly as method arguments (or aliased).
    The C3 check ensures byref expressions are never captured, e.g. passed as direct method arg under a capturing thunk.
    The C2 checks no type can store byrefs (C4 ensures F# code would never actually store them).
    The C1 checks no generic type could be instanced to store byrefs.
*)

//--------------------------------------------------------------------------
// NOTES: rethrow safety checks
//--------------------------------------------------------------------------
 
(* "rethrow may only occur with-in the body of a catch handler".
   -- Section 4.23. Part III. CLI Instruction Set. ECMA Draft 2002.
   
   1. rethrow() calls are converted to TOp_rethrow in the type checker.
   2. any remaining rethrow val_refs will be first class uses. These are trapped.
   3. The freevars track free TOp_rethrow (they are bound (cleared) at try-catch handlers).
   4. An outermost expression is not contained in a try-catch handler.
      These may not have unbound rethrows.      
      Outermost expressions occur at:
      * module bindings.
      * attribute arguments.
      * Any more? What about fields of a static class?            
   5. A lambda body (from lambda-expression or method binding) will not occur under a try-catch handler.
      These may not have unbound rethrows.
   6. All other constructs are assumed to generate IL code sequences.
      For correctness, this claim needs to be justified.
      
      Q:  Do any post check rewrite passes factor expressions out to other functions?      
      A1. The optimiser may introduce auxillary functions, e.g. by splitting out match-branches.
          This should not be done if the refactored body contains an unbound rethrow.
      A2. TLR? Are any expression factored out into functions?
      
   Informal justification:
   If a rethrow occurs, then it is minimally contained by either:
     a) a try-catch - accepted.
     b) a lambda expression - rejected.
     c) none of the above - rejected as when checking outmost expressions.
*)



//--------------------------------------------------------------------------
// check environment
//--------------------------------------------------------------------------

type env = 
    { boundTypars: Typar list; 
      /// "module remap info", i.e. hiding information down the signature chain, used to compute what's hidden by a signature
      mrmi: (Remap * SignatureHidingInfo) list; 
      /// Constructor limited - are we in the prelude of a constructor, prior to object initialization
      limited: bool;
      /// Are we in a quotation?
      quote : bool;  } 

let BindTypar env tyv = { env with boundTypars= tyv::env.boundTypars } 

let BindTypars env (tps:Typar list) = 
    if isNil tps then env else
    // Here we mutate to provide better names for generalized type parameters 
    let nms = PrettyTypes.PrettyTyparNames (fun _ -> true) (env.boundTypars |> List.map (fun tp -> tp.Name) ) tps
    (tps,nms) ||> List.iter2 (fun tp nm -> 
            if PrettyTypes.NeedsPrettyTyparName tp  then 
                tp.Data.typar_id <- ident (nm,tp.Range));      
    List.fold BindTypar env tps 

type cenv = 
    { mutable boundVals: Set<stamp>; 
      mutable potentialUnboundUsesOfVals: Map<stamp,range>; 
      g: TcGlobals; 
      amap: Import.ImportMap; 
      /// For reading metadata
      infoReader: InfoReader;
      internalsVisibleToPaths : CompilationPath list;
      denv: DisplayEnv; 
      viewCcu : ccu;
      reportErrors: bool;
      canContainEntryPoint : bool;
      // outputs
      mutable usesQuotations : bool
      mutable entryPointGiven:bool  }

let BindVal cenv (v:Val) = 
    //printfn "binding %s..." v.DisplayName
    cenv.boundVals <- cenv.boundVals.Add(v.Stamp)
let BindVals cenv vs = List.iter (BindVal cenv) vs

//--------------------------------------------------------------------------
// check for byref types
//--------------------------------------------------------------------------

let exists_ty pred typ = 
    let res = ref false 
    let visitType ty = if pred ty then res := true 
    let visitTypar tp = () 
    let visitTraitSolution tp = () 
    typ |> IterType (visitType,visitTypar,visitTraitSolution);
    !res

let is_byref_like_ty g ty = 
    match try_tcref_of_stripped_typ g ty with 
    | None -> false
    | Some tcref -> 
        tcref_eq g g.byref_tcr tcref ||
        tcref_eq g g.system_TypedReference_tcref tcref ||
        tcref_eq g g.system_ArgIterator_tcref tcref ||
        tcref_eq g g.system_RuntimeArgumentHandle_tcref tcref

let contains_byref_ty cenv typ = exists_ty (is_byref_like_ty cenv.g) typ
let contains_void_ty  cenv typ = exists_ty (is_void_typ cenv.g) typ


//--------------------------------------------------------------------------
// check captures under lambdas
//--------------------------------------------------------------------------
  
/// This is the definition of what can/can't be free in a lambda expression. This is checked at lambdas OR TBind(v,e) nodes OR TObjExprMethod nodes. 
/// For TBind(v,e) nodes we may know an 'arity' which gives as a larger set of legitimate syntactic arguments for a lambda. 
/// For TObjExprMethod(v,e) nodes we always know the legitimate syntactic arguments. 
let CheckEscapes cenv allowProtected syntacticArgs body =
    if cenv.reportErrors then 
        let cant_be_free v = 
           // First, if v is a syntactic argument, then it can be free since it was passed in. 
           // The following can not be free: 
           //   a) "Local" mutables, being mutables such that: 
           //         i)  the mutable has no arity (since arity implies top-level storage, top level mutables...) 
           //             Note: "this" arguments to instance members on mutable structs are mutable arguments. 
           //   b) BaseVal can never escape. 
           //   c) Byref typed values can never escape. 

           // These checks must correspond to the tests governing the error messages below. 
           let passedIn = ListSet.mem vspec_eq v syntacticArgs 
           if passedIn then
               false
           else
               (v.IsMutable && v.TopValInfo.IsNone) ||
               (v.BaseOrThisInfo = BaseVal  && not passedIn) ||
               (is_byref_like_ty cenv.g v.Type)

        let frees = free_in_expr CollectLocals body
        let fvs   = frees.FreeLocals 
        if not allowProtected && frees.UsesMethodLocalConstructs  then
            errorR(Error("A protected member is called or 'base' is being used. This is only allowed in the direct implementation of members since they could escape their object scope",range_of_expr body))
        elif Zset.exists cant_be_free fvs then 
            let v =  List.find cant_be_free (Zset.elements fvs) 
            (* byref error before mutable error (byrefs are mutable...). *)
            if (is_byref_like_ty cenv.g v.Type) then
                // Inner functions are not guaranteed to compile to method with a predictable arity (number of arguments). 
                // As such, partial applications involving byref arguments could lead to closures containing byrefs. 
                // For safety, such functions are assumed to have no known arity, and so can not accept byrefs. 
                errorR(Error("The byref-typed variable '"^v.DisplayName^"' is used in an invalid way. Byrefs may not be captured by closures or passed to inner functions",range_of_expr body))
            elif v.IsMutable then 
                errorR(Error("The mutable variable '"^v.DisplayName^"' is used in an invalid way. Mutable variables may not be captured by closures. Consider eliminating this use of mutation or using a heap-allocated mutable reference cell via 'ref' and '!'",range_of_expr body))
            elif v.BaseOrThisInfo = BaseVal then
                errorR(Error("The 'base' keyword is used in an invalid way. Base calls may not be used in closures. Consider using a private member to make base calls",range_of_expr body))
            else
                errorR(InternalError("The variable '"^v.DisplayName^"' is used in an invalid way",range_of_expr body)) (* <- should be dead code, unless governing tests change *)


//--------------------------------------------------------------------------
// check type access
//--------------------------------------------------------------------------

let access_internals_visible_to_as_internal thisCompPath internalsVisibleToPaths access =
  // Each internalsVisibleToPath is a compPath for the internals of some assembly.
  // Replace those by the compPath for the internals of this assembly.
  // This makes those internals visible here, but still internal. Bug://3737
  List.fold
    (fun access internalsVisibleToPath -> access_subst_paths (thisCompPath,internalsVisibleToPath) access)
    access internalsVisibleToPaths

let CheckTypeForAccess (cenv:cenv) objName valAcc m ty =
    if cenv.reportErrors then 
        let visitType ty = 
            match try_tcref_of_stripped_typ cenv.g ty with 
            | None -> ()
            | Some tcref ->
                let thisCompPath = cpath_of_ccu cenv.viewCcu
                let tyconAcc = tcref.Accessibility |> access_internals_visible_to_as_internal thisCompPath cenv.internalsVisibleToPaths
                if IsLessAccessible tyconAcc valAcc then
                    errorR(Error(Printf.sprintf "The type '%s' is less accessible than the value, member or type '%s' it is used in" tcref.DisplayName (objName()),m)) 
        let visitTypar tp = () 
        let visitTraitSolution tp = () 
        ty |> IterType (visitType,visitTypar,visitTraitSolution)

//--------------------------------------------------------------------------
// check type instantiations
//--------------------------------------------------------------------------

/// Check types occuring in the TAST.
let CheckType permitByrefs (cenv:cenv) (env:env) m ty =
    if cenv.reportErrors then 
        let checkByrefs = not permitByrefs
        let containsVoid = ref false 
        let containsByref = ref false 
        let visitType ty = 
            if checkByrefs && is_byref_like_ty cenv.g ty then containsByref := true 
            if is_void_typ cenv.g ty then containsVoid := true 
        let visitTypar tp = () 
        let visitTraitSolution info = 
            match info with 
            | FSMethSln(_,vref,_) -> 
               //printfn "considering %s..." vref.DisplayName
               if vref_in_this_assembly cenv.g.compilingFslib vref && not (cenv.boundVals.Contains(vref.Stamp)) then 
                   //printfn "recording %s..." vref.DisplayName
                   cenv.potentialUnboundUsesOfVals <- cenv.potentialUnboundUsesOfVals.Add(vref.Stamp,m)
            | _ -> ()
        ty |> IterType (visitType,visitTypar,visitTraitSolution);

        if !containsVoid then 
            errorR(Error("'System.Void' can only be used as 'typeof<System.Void>' in F#",m))
        if !containsByref then 
            errorR(Error("A type instantiation involves a byref type. This is not permitted by the .NET runtime",m))

/// Check types occuring in TAST (like CheckType) and additionally reject any byrefs.
/// The additional byref checks are to catch "byref instantiations" - one place were byref are not permitted.  
let CheckTypeNoByrefs (cenv:cenv) env m ty = CheckType false cenv env m ty
let CheckTypePermitByrefs (cenv:cenv) env m ty = CheckType true cenv env m ty

let CheckTypeInstNoByrefs (cenv:cenv) env m tyargs =
    tyargs |> List.iter (CheckTypeNoByrefs cenv env m)

let CheckTypeInstPermitByrefs (cenv:cenv) env m tyargs =
    tyargs |> List.iter (CheckType true cenv env m)

//--------------------------------------------------------------------------
// check exprs etc
//--------------------------------------------------------------------------
  
type context = 
    /// Tuple of contexts allowing byref typed expr 
    | KnownArityTuple of int    
    /// Context allows for byref typed expr 
    | DirectArg                 
    /// General (byref type expr not allowed) 
    | GeneralContext            

let mkKnownArity n = if n=1 then DirectArg else KnownArityTuple n

let argAritiesOfVal (vref:ValRef) = 
    match vref.TopValInfo with
    | Some topValInfo -> List.map mkKnownArity topValInfo.AritiesOfArgs
    | None -> []  

let rec argAritiesOfFunExpr x =
    match x with 
    | TExpr_val (vref,_,_)         -> argAritiesOfVal     vref      (* recognise val *)
    | TExpr_link eref              -> argAritiesOfFunExpr !eref     (* step through reclink  *)
    | TExpr_app(f,fty,tyargs,[],m) -> argAritiesOfFunExpr f         (* step through instantiations *)
    | TExpr_op(TOp_coerce,_,[f],_) -> argAritiesOfFunExpr f         (* step through subsumption coercions *)
    | _                            -> []
    
let CheckNoRethrow cenv (body:expr) = 
    if cenv.reportErrors then
        if (free_in_expr CollectLocals body).UsesUnboundRethrow then
            errorR(Error("This expression contains a call to rethrow. Rethrow may only occur directly in a handler of a try-with. Rethrow may not be factored to a separate method or delayed under a lambda expression",range_of_expr body))

let is_splice g v = g.vref_eq v g.splice_expr_vref || g.vref_eq v g.splice_raw_expr_vref 

let rec CheckExpr   (cenv:cenv) (env:env) expr = CheckExprInContext cenv env expr GeneralContext
and CheckVal (cenv:cenv) (env:env) v m context = 
    if cenv.reportErrors then 
          if is_splice cenv.g v && not env.quote then errorR(Error("Expression-splicing operators may only be used within quotations",m));
          if is_splice cenv.g v then errorR(Error("First-class uses of the expression-splicing operator are not permitted",m));
          if cenv.g.vref_eq v cenv.g.addrof_vref then errorR(Error("First-class uses of the address-of operators are not permitted",m));
          if cenv.g.vref_eq v cenv.g.addrof2_vref then errorR(Error("First-class uses of the address-of operators are not permitted",m));
          if cenv.g.vref_eq v cenv.g.rethrow_vref then errorR(Error("First-class uses of the rethrow function is not permitted",m));          
          if is_byref_like_ty cenv.g v.Type then 
              // byref typed val can only occur in permitting contexts 
              if context <> DirectArg then errorR(Error("The byref typed value '" ^ v.DisplayName ^ "' may not be used at this point",m))
    CheckTypePermitByrefs cenv env m v.Type
and CheckExprInContext (cenv:cenv) (env:env) expr (context:context) =    
    // dprintf "CheckExpr: %s\n" (showL(ExprL expr));
    let expr = strip_expr expr in
    match expr with
    | TExpr_seq (e1,e2,dir,_,_) -> 
        CheckExpr cenv env e1; 
        CheckExpr cenv (if dir=ThenDoSeq then {env with limited=false} else env) e2
    | TExpr_let (bind,body,m,_) ->  
        CheckBinding cenv env bind ; 
        BindVal cenv (var_of_bind bind)
        CheckExpr cenv env body
    | TExpr_const (c,m,ty) -> 
        CheckTypePermitByrefs cenv env m ty 
            
    | TExpr_val (v,vFlags,m) -> 
          if cenv.reportErrors then 
              if v.BaseOrThisInfo = BaseVal then 
                errorR(Error("'base' values may only be used to make direct calls to the base implementations of overridden members",m))            
          
          CheckVal cenv env v m context
          
    | TExpr_quote(ast,savedConv,m,ty) -> 
          CheckExpr cenv {env with quote=true} ast;
          if cenv.reportErrors then 
              cenv.usesQuotations <- true
              try 
                  let conv = Creflect.ConvExprPublic (cenv.g, cenv.amap, cenv.viewCcu) Creflect.empty_env ast  
                  match !savedConv with 
                  | None -> savedConv:= Some conv
                  | Some _ -> ()
              with Creflect.InvalidQuotedTerm e -> 
                  errorRecovery e m
                
          CheckTypeNoByrefs cenv env m ty

    | TExpr_obj (_,typ,basev,superInitCall,overrides,iimpls,m,_) -> 
          CheckExpr cenv env superInitCall;
          CheckMethods cenv env basev overrides ;
          CheckInterfaceImpls cenv env basev iimpls;
          CheckTypePermitByrefs cenv env m typ

    // Allow base calls to F# methods
    | TExpr_app((TExpr_val(v,vFlags,_) as f),fty,tyargs,(TExpr_val(baseVal,_,_)::rest),m) 
          when vFlags = VSlotDirectCall && baseVal.BaseOrThisInfo = BaseVal ->
        // dprintfn "GOT BASE VAL USE"
        CheckVal cenv env v m GeneralContext
        CheckVal cenv env baseVal m GeneralContext
        CheckTypePermitByrefs cenv env m fty;
        CheckTypeInstPermitByrefs cenv env m tyargs;
        CheckExprsInContext cenv env rest (argAritiesOfFunExpr f)

    // Allow base calls to IL methods
    | TExpr_op (TOp_ilcall ((virt,protect,valu,newobj,superInit,prop,isDllImport,boxthis,mref),enclTypeArgs,methTypeArgs,tys),tyargs,(TExpr_val(baseVal,_,_)::rest),m) 
          when not virt && baseVal.BaseOrThisInfo = BaseVal ->
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckTypeInstNoByrefs cenv env m enclTypeArgs;
        CheckTypeInstNoByrefs cenv env m methTypeArgs;
        CheckTypeInstNoByrefs cenv env m tys;
        CheckVal cenv env baseVal m GeneralContext
        CheckExprDirectArgs cenv env rest

    | TExpr_op (c,tyargs,args,m) ->
          CheckExprOp cenv env (c,tyargs,args,m) context

    // Allow 'typeof<System.Void>' calls as a special case, the only accepted use of System.Void! 
    | TExpr_app(TExpr_val(vref,_,_),_,[ty],[],m) when (is_typeof_vref cenv.g vref || is_typedefof_vref cenv.g vref ) && is_void_typ cenv.g ty ->
          () // typeof<System.Void> allowed. Special case. No further checks.

    // Allow '%expr' in quotations
    | TExpr_app(TExpr_val(vref,_,_),_,tinst,[arg],m) when is_splice cenv.g vref && env.quote ->
          CheckTypeInstPermitByrefs cenv env m tinst;
          CheckExpr cenv env arg


    | TExpr_app(f,fty,tyargs,argsl,m) ->
        // dprintfn "NO BASE VAL USE"
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckTypePermitByrefs cenv env m fty;
        CheckTypeInstPermitByrefs cenv env m tyargs;
        CheckExpr cenv env f;
        CheckExprsInContext cenv env argsl (argAritiesOfFunExpr f)

    (* REVIEW: fold the next two cases together *)
    | TExpr_lambda(lambda_id,basevopt,argvs,body,m,rty,_) -> 
        let topValInfo = TopValInfo ([],[argvs |> List.map (fun _ -> TopValInfo.unnamedTopArg1)],TopValInfo.unnamedRetVal) in 
        Option.iter (BindVal cenv) basevopt
        BindVals cenv argvs
        List.iter (type_of_val >> CheckTypePermitByrefs cenv env m) argvs;
        CheckTypePermitByrefs cenv env m rty;
        let ty = mk_multi_lambda_ty m argvs rty in 
        CheckLambdas None cenv env false topValInfo expr m ty

    | TExpr_tlambda(lambda_id,tps,body,m,rty,_)  -> 
        let topValInfo = TopValInfo (TopValInfo.InferTyparInfo tps,[],TopValInfo.unnamedRetVal) in
        CheckTypePermitByrefs cenv env m rty; 
        let ty = try_mk_forall_ty tps rty in 
        CheckLambdas None cenv env false topValInfo expr m ty

    | TExpr_tchoose(tps,e1,m)  -> 
        CheckExpr cenv env e1 

    | TExpr_match(_,_,dtree,targets,m,ty,_) -> 
        CheckTypeNoByrefs cenv env m ty;
        CheckDecisionTree cenv env dtree;
        CheckDecisionTreeTargets cenv env m ty targets;
    | TExpr_letrec (binds,e,m,_) ->  
        BindVals cenv (List.map var_of_bind binds) 
        CheckBindings cenv env binds;
        CheckExpr cenv env e
    | TExpr_static_optimization (constraints,e2,e3,m) -> 
        CheckExpr cenv env e2;
        CheckExpr cenv env e3;
        constraints |> List.iter (fun (TTyconEqualsTycon(ty1,ty2)) -> 
            CheckTypeNoByrefs cenv env m ty1;
            CheckTypeNoByrefs cenv env m ty2)
    | TExpr_link eref -> 
        failwith "Unexpected reclink"

and CheckMethods cenv env basevopt l = List.iter (CheckMethod cenv env basevopt) l
and CheckMethod cenv env basevopt (TObjExprMethod(slotsig,tps,vs,e,m) as ObjExprMethod) = 
    let env = BindTypars env tps 
    let vs = List.concat vs
    CheckNoRethrow cenv e;
    CheckEscapes cenv true (match basevopt with Some x -> x:: vs | None -> vs) e;
    CheckExpr cenv env e

and CheckInterfaceImpls cenv env basevopt l = 
    l |> List.iter (CheckInterfaceImpl cenv env basevopt)
    
and CheckInterfaceImpl cenv env basevopt (ty,overrides) = 
    CheckMethods cenv env basevopt overrides 


and CheckExprOp cenv env (op,tyargs,args,m) context =
    let limitedCheck() = 
        if env.limited then errorR(Error("Object constructors may not directly use try/with and try/finally prior to the initialization of the object. This includes constructs such as 'for x in ...' that may elaborate to uses of these constructs. This is a limitation imposed by the .NET IL",m));
    List.iter (CheckTypePermitByrefs cenv env m) tyargs;
    (* Special cases *)
    match op,tyargs,args,context with 
    // Handle these as special cases since mutables are allowed inside their bodies 
    | TOp_while _,_,[TExpr_lambda(_,_,[_],e1,_,_,_);TExpr_lambda(_,_,[_],e2,_,_,_)],_  ->
        CheckTypeInstNoByrefs cenv env m tyargs; 
        CheckExprs cenv env [e1;e2]

    | TOp_try_finally _,[_],[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[_],e2,_,_,_)],_ ->
        CheckTypeInstNoByrefs cenv env m tyargs; 
        limitedCheck();
        CheckExprs cenv env [e1;e2]

    | TOp_for(_),_,[TExpr_lambda(_,_,[_],e1,_,_,_);TExpr_lambda(_,_,[_],e2,_,_,_);TExpr_lambda(_,_,[_],e3,_,_,_)],_  ->
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckExprs cenv env [e1;e2;e3]

    | TOp_try_catch _,[_],[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[_],e2,_,_,_); TExpr_lambda(_,_,[_],e3,_,_,_)],_ ->
        CheckTypeInstNoByrefs cenv env m tyargs;
        limitedCheck();
        CheckExprs cenv env [e1;e2;e3]

    | TOp_ilcall ((virt,protect,valu,newobj,superInit,prop,isDllImport,boxthis,mref),enclTypeArgs,methTypeArgs,tys),_,_,_ ->
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckTypeInstNoByrefs cenv env m enclTypeArgs;
        CheckTypeInstNoByrefs cenv env m methTypeArgs;
        CheckTypeInstNoByrefs cenv env m tys;
        CheckExprDirectArgs cenv env args  

    | TOp_tuple,_,_,KnownArityTuple nArity ->           (* tuple expression in known tuple context             *)
        if cenv.reportErrors then 
            if List.length args <> nArity then 
                errorR(InternalError("Tuple arity does not correspond to planned function argument arity",m));
        (* This tuple should not be generated. The known function arity means it just bundles arguments. *)
        CheckExprDirectArgs cenv env args 
    | TOp_lval_op(LGetAddr,v),_,_,arity -> 
        if arity = DirectArg then       
          CheckExprs cenv env args                   (* Address-of operator generates byref, and context permits this. *)
        else            
          if cenv.reportErrors  then 
              errorR(Error("The address of the variable '" ^ v.DisplayName ^"' may not be used at this point",m))            
    | TOp_rfield_get rf,_,[arg1],arity -> 
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckExprDirectArgs cenv env [arg1]          (* See mk_recd_field_get_via_expra -- byref arg1 when #args =1 *)
                                                        (* Property getters on mutable structs come through here. *)
    | TOp_rfield_set rf,_,[arg1;arg2],arity -> 
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckExprDirectArgs cenv env [arg1];         (* See mk_recd_field_set_via_expra -- byref arg1 when #args=2 *)
        CheckExprs            cenv env [arg2]          (* Property setters on mutable structs come through here (TBC). *)
    | TOp_coerce,[ty1;ty2],[x],arity ->
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckExprInContext cenv env x context
    | TOp_rethrow,[ty1],[],arity ->
        CheckTypeInstNoByrefs cenv env m tyargs        
    | TOp_field_get_addr rfref,tyargs,[],_ ->
        if context <> DirectArg && cenv.reportErrors  then
          errorR(Error("The address of the static field '"^rfref.FieldName^"' may not be used at this point",m));
        CheckTypeInstNoByrefs cenv env m tyargs
        (* NOTE: there are no arg exprs to check in this case *)
    | TOp_field_get_addr rfref,tyargs,[rx],_ ->
        if context <> DirectArg && cenv.reportErrors then
          errorR(Error("The address of the field '"^rfref.FieldName^"' may not be used at this point",m));
        (* This construct is used for &(rx.rfield) and &(rx->rfield). Relax to permit byref types for rx. [See Bug 1263]. *)
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckExprInContext cenv env rx DirectArg (* allow rx to be byref here *)
    | TOp_asm (instrs,tys),_,_,_  ->
        CheckTypeInstPermitByrefs cenv env m tys;
        CheckTypeInstNoByrefs cenv env m tyargs;
        begin
            match instrs,args with
            | [ I_stfld (alignment,vol,fspec) ],[lhs;rhs] ->
                CheckExprInContext cenv env lhs DirectArg; (* permit byref for lhs lvalue *)
                CheckExpr         cenv env rhs                
            | [ I_ldfld (alignment,vol,fspec) ],[lhs] ->
                CheckExprInContext cenv env lhs DirectArg  (* permit byref for lhs lvalue *)
            | [ I_ldflda (fspec) | I_ldsflda (fspec) ],[lhs] ->
                if context <> DirectArg && cenv.reportErrors then
                  errorR(Error("The address of the field '"^fspec.Name^"' may not be used at this point",m));
                CheckExprInContext cenv env lhs DirectArg  (* permit byref for lhs lvalue *)
            | [ I_ldelema _ ],[lhs] ->
                if context <> DirectArg && cenv.reportErrors then
                  errorR(Error("The address of an array element may not be used at this point",m));
                CheckExprInContext cenv env lhs DirectArg  (* permit byref for lhs lvalue *)
            | instrs ->
                CheckExprs cenv env args  
        end
    | (   TOp_tuple
        | TOp_ucase _
        | TOp_exnconstr _
        | TOp_array
        | TOp_bytes _
        | TOp_uint16s _
        | TOp_recd _
        | TOp_rfield_set _
        | TOp_ucase_tag_get _
        | TOp_ucase_proof _
        | TOp_ucase_field_get _
        | TOp_ucase_field_set _
        | TOp_exnconstr_field_get _
        | TOp_exnconstr_field_set _
        | TOp_tuple_field_get _
        | TOp_get_ref_lval 
        | TOp_trait_call _
        | _ (* catch all! *)
        ),_,_,_ ->    
        CheckTypeInstNoByrefs cenv env m tyargs;
        CheckExprs cenv env args 

and CheckLambdas memInfo cenv env inlined topValInfo e m ety =
    CheckTypePermitByrefs cenv env m ety;
    // The topValInfo here says we are _guaranteeing_ to compile a function value 
    // as a .NET method with precisely the corresponding argument counts. 
    match e with
    | TExpr_tchoose(tps,e1,m)  -> 
        let env = BindTypars env tps 
        CheckLambdas memInfo cenv env inlined topValInfo e1 m ety      

    | TExpr_lambda (lambda_id,_,_,_,m,_,_)  
    | TExpr_tlambda(lambda_id,_,_,m,_,_) ->

        let tps,basevopt,vsl,body,bodyty = dest_top_lambda_upto cenv.g cenv.amap topValInfo (e, ety) in
        let env = BindTypars env tps 
        let vspecs = (Option.to_list basevopt @ List.concat vsl) in 
        vspecs |> List.iter (CheckValSpec cenv env m);
        
        // Allow access to protected things within members
        match memInfo with 
        | None -> ()
        | Some membInfo -> 
            testHookMemberBody membInfo body;
        
        CheckEscapes cenv (isSome(memInfo)) vspecs body;
        CheckNoRethrow cenv body; (* no rethrow under lambda expression *)
        CheckExpr cenv env body;
        if not inlined && contains_byref_ty cenv bodyty && cenv.reportErrors then
            if List.length vsl = 0 then
                errorR(Error("The type of a first-class function may not contain byrefs",m))
            else
                errorR(Error("A method return type would contain byrefs which is not permitted",m))
    | _ -> 
        if not inlined && is_byref_like_ty cenv.g ety then
            CheckExprInContext cenv env e DirectArg           (* allow byref to occur as RHS of byref binding. *)
        else 
            CheckExpr cenv env e

and CheckExprsInContext cenv env exprs arities =
    let arities = Array.of_list arities 
    let argArity i = if i < Array.length arities then arities.[i] else GeneralContext 
    exprs |> List.iteri (fun i exp -> CheckExprInContext cenv env exp (argArity i)) 

and CheckExprs cenv env exprs = 
    exprs |> List.iter (CheckExpr cenv env) 

and CheckFlatExprs cenv env exprs = 
    exprs |> FlatList.iter (CheckExpr cenv env) 

and CheckExprDirectArgs cenv env exprs = 
    exprs |> List.iter (fun x -> CheckExprInContext cenv env x DirectArg) 

and CheckDecisionTreeTargets cenv env m ty targets = 
    targets |> Array.iter (CheckDecisionTreeTarget cenv env m ty) 

and CheckDecisionTreeTarget cenv env m ty (TTarget(vs,e,_)) = CheckExpr cenv env e;

and CheckDecisionTree cenv env x =
    match x with 
    | TDSuccess (es,n) -> CheckFlatExprs cenv env es;
    | TDBind(bind,rest) -> CheckBinding cenv env bind; CheckDecisionTree cenv env rest 
    | TDSwitch (e,cases,dflt,m) -> CheckDecisionTreeSwitch cenv env (e,cases,dflt,m)

and CheckDecisionTreeSwitch cenv env (e,cases,dflt,m) =
    CheckExpr cenv env e;
    List.iter (fun (TCase(discrim,e)) -> CheckDecisionTreeTest cenv env m discrim; CheckDecisionTree cenv env e) cases;
    Option.iter (CheckDecisionTree cenv env) dflt

and CheckDecisionTreeTest cenv env m discrim =
    match discrim with
    | TTest_unionconstr (ucref,tinst) -> CheckTypeInstPermitByrefs cenv env m tinst
    | TTest_array_length (n,typ)      -> CheckTypePermitByrefs cenv env m typ
    | TTest_const _                   -> ()
    | TTest_isnull                    -> ()
    | TTest_isinst (srcTyp,dstTyp)    -> (CheckTypePermitByrefs cenv env m srcTyp; CheckTypePermitByrefs cenv env m dstTyp)
    | TTest_query (exp,tys,_,_,_)     -> ()

and CheckAttrib cenv env (Attrib(_,k,args,props,m)) = 
    props |> List.iter (fun (AttribNamedArg(nm,ty,flg,expr)) -> CheckAttribExpr cenv env expr);
    args |> List.iter (CheckAttribExpr cenv env)

and CheckAttribExpr cenv env (AttribExpr(expr,vexpr)) = 
    CheckExpr cenv env expr;
    CheckExpr cenv env vexpr;
    CheckNoRethrow cenv expr; 
    CheckAttribValue cenv env vexpr

and CheckAttribValue cenv env expr = 
    match expr with 

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
        | TConst_string _  -> ()
        | _ -> 
            if cenv.reportErrors then 
                errorR (Error ( "This constant may not be used as a custom attribute value",m))

    | TExpr_op(TOp_array,[elemTy],args,m) -> 
        List.iter (CheckAttribValue cenv env) args
    | TExpr_app(TExpr_val(vref,_,_),_,[ty],[],m) when (is_typeof_vref cenv.g vref || is_typedefof_vref cenv.g vref) -> 
        ()
    | TExpr_op(TOp_coerce,_,[arg],_) -> 
        CheckAttribValue cenv env arg
    | TExpr_app(TExpr_val(vref,_,_),_,_,[arg1],_) when cenv.g.vref_eq vref cenv.g.enum_vref  -> 
        CheckAttribValue cenv env arg1
    (* Detect bitwise or of attribute flags: one case of constant folding (a more general treatment is needed *)    
    | BitwiseOr cenv.g (arg1,arg2) ->
        CheckAttribValue cenv env arg1;
        CheckAttribValue cenv env arg2
    | _ -> 
        if cenv.reportErrors then 
           errorR (Error ("invalid custom attribute value (not a constant or literal)",range_of_expr expr))
  
and CheckAttribs cenv env (attribs: Attribs) = 
    if isNil attribs then () else
    let tcrefs = [ for (Attrib(tcref,k,_,_,m)) in attribs -> (tcref,m) ]

    // Check for violations of allowMultiple = false
    let duplicates = 
        tcrefs 
        |> Seq.group_by (fun (tcref,m) -> tcref.Stamp) 
        |> Seq.map (fun (_,elems) -> List.last (List.of_seq elems), Seq.length elems) 
        |> Seq.filter (fun (_,count) -> count > 1) 
        |> Seq.map fst 
        |> Seq.to_list
        // Filter for allowMultiple = false
        |> List.filter (fun (tcref,m) -> 
                let (AttribInfo(tref,_)) = cenv.g.attrib_AttributeUsageAttribute
                let allowMultiple = 
                    TyconRefTryBindAttrib cenv.g cenv.g.attrib_AttributeUsageAttribute tcref 
                             (fun (_,named)             -> named |> List.tryPick (function ("AllowMultiple",_,_,CustomElem_bool res) -> Some res | _ -> None))
                             (fun (Attrib(_,_,_,named,_)) -> named |> List.tryPick (function AttribNamedArg("AllowMultiple",_,_,AttribBoolArg(res) ) -> Some res | _ -> None)) 
                
                (allowMultiple <> Some(true)))
    if cenv.reportErrors then 
       for (tcref,m) in duplicates do
          errorR(Error("The attribute type '"^tcref.DisplayName ^"' has 'AllowMultiple=false'. Multiple instances of this attribute may not be attached to a single language element",m))
    
    attribs |> List.iter (CheckAttrib cenv env) 

and CheckValInfo cenv env (TopValInfo(_,args,ret)) =
    args |> List.iterSquared (CheckArgInfo cenv env);
    ret |> CheckArgInfo cenv env;

and CheckArgInfo cenv env (TopArgInfo(attribs,_)) = 
    CheckAttribs cenv env attribs

and CheckValSpec cenv env m (v:Val) =
    v.Attribs |> CheckAttribs cenv env;
    v.TopValInfo |> Option.iter (CheckValInfo cenv env);
    v.Type |> CheckTypePermitByrefs cenv env m

and CheckBinding cenv env (TBind(v,e,_) as bind) =
    //printfn "visiting %s..." v.DisplayName
    match cenv.potentialUnboundUsesOfVals.TryFind(v.Stamp) with
    | None -> () 
    | Some m ->
         let nm = v.DisplayName
         errorR(Error(sprintf "The member '%s' is used in an invalid way. A use of '%s' has been inferred prior to its definition at or near '%s'. This is an invalid forward reference" nm nm (string_of_range m), v.Range))

    v.Type |> CheckTypePermitByrefs cenv env v.Range;
    v.Attribs |> CheckAttribs cenv env;
    v.TopValInfo |> Option.iter (CheckValInfo cenv env);
    if (v.IsMemberOrModuleBinding || v.IsMember) && not v.IsIncrClassGeneratedMember then 
        let access = 
           if IsHiddenVal env.mrmi v then 
               let (TAccess(l)) = v.Accessibility 
               // FSharp 1.0 bug 1908: Values hidden by signatures are implicitly at least 'internal'
               let scoref = v.MemberActualParent.CompilationPath.ILScopeRef
               (TAccess(CompPath(scoref,[])::l)) 
           else 
               v.Accessibility
        v.Type |> CheckTypeForAccess cenv (fun () -> NicePrint.string_of_qualified_val_spec cenv.denv v) access v.Range;
    
    let env = if v.IsConstructor && not v.IsIncrClassConstructor then { env with limited=true } else env

    if cenv.reportErrors  then 
        if is_byref_like_ty cenv.g v.Type && isSome (chosen_arity_of_bind bind) then    
            errorR(Error("A byref typed value would be stored here. Top-level let-bound byref values are not permitted",v.Range));

        // Check top-level let-bound values (arity=0 so not compiled not method) for byref types (not allowed) 
        match chosen_arity_of_bind bind with
          | Some info when info.HasNoArgs && contains_byref_ty cenv v.Type ->
                  errorR(Error("A byref typed value would be stored here. Top-level let-bound byref values are not permitted",v.Range))
          | _ -> ()

        if isSome v.PublicPath then 
            if HasAttrib cenv.g cenv.g.attrib_ReflectedDefinitionAttribute v.Attribs then
                cenv.usesQuotations <- true
                (* If we've already recorded a definition then skip this *)
                match v.ReflectedDefinition with 
                | None -> v.Data.val_defn <- Some e
                | Some _ -> ()
                // Run the conversion process over the reflected definition to report any errors in the
                // front end rather than the back end. We currenly re-run this during ilxgen.ml but there's
                // no real need for that except that it helps us to bundle all reflected definitions up into 
                // one blob for pickling to the binary format
                try
                    let ety = type_of_expr cenv.g e
                    let tps,taue,tauty = 
                      match e with 
                      | TExpr_tlambda (_,tps,b,_,_,_) -> tps,b,reduce_forall_typ cenv.g ety (List.map mk_typar_ty tps)
                      | _ -> [],e,ety
                    let env = Creflect.BindTypars Creflect.empty_env tps
                    let nng = NiceNameGenerator ()
                    let _,argExprs,_ = Creflect.ConvExprPublic (cenv.g,cenv.amap,cenv.viewCcu) env taue 
                    if nonNil(argExprs) then 
                        errorR(Error("[<ReflectedDefinition>] terms may not contain uses of the prefix splice operator '%'",v.Range));
                    let crenv = Creflect.mk_cenv (cenv.g,cenv.amap,cenv.viewCcu)
                    Creflect.ConvMethodBase crenv env v |> ignore
                with 
                  | Creflect.InvalidQuotedTerm e -> 
                          errorR(e)
            
    match v.MemberInfo with 
    | Some memberInfo when not v.IsIncrClassGeneratedMember -> 
        match memberInfo.MemberFlags.MemberKind with 
         
        | (MemberKindPropertySet | MemberKindPropertyGet)  ->
            // These routines raise errors for ill-formed properties
            v |> ReturnTypeOfPropertyVal cenv.g |> ignore
            v |> ArgInfosOfPropertyVal cenv.g |> ignore
        | _ -> 
            ()
        
    | _ -> ()
        
        
    let topValInfo  = match chosen_arity_of_bind bind with Some info -> info | _ -> TopValInfo.emptyValData in
    let inlined     = v.MustInline in
      (* certain inline functions are permitted to have byref return types, since they never compile to records. *)
      (* e.g. for the byref operator itself, &. *)
    CheckLambdas v.MemberInfo cenv env inlined topValInfo e v.Range v.Type;

and CheckBindings cenv env xs = FlatList.iter (CheckBinding cenv env) xs

// Top binds introduce expression, check they are rethrow free.
let CheckTopBinding cenv env (TBind(v,e,_) as bind) =
    let isExplicitEntryPoint = HasAttrib cenv.g cenv.g.attrib_EntryPointAttribute v.Attribs
    if isExplicitEntryPoint then 
        cenv.entryPointGiven <- true;
        if not cenv.canContainEntryPoint && cenv.reportErrors  then 
            errorR(Error("A function labelled with the 'EntryPointAttribute' attribute must be the last declaration in the last file in the compilation sequence",v.Range)) 
    CheckNoRethrow cenv e;
    CheckBinding cenv env bind

let CheckTopBindings cenv env binds = FlatList.iter (CheckTopBinding cenv env) binds

//--------------------------------------------------------------------------
// check tycons
//--------------------------------------------------------------------------
  
let CheckRecdField cenv env (tycon:Tycon) (rfield:RecdField) = 
    CheckTypeForAccess cenv (fun () -> rfield.Name) rfield.Accessibility rfield.Range rfield.FormalType;
    CheckTypePermitByrefs cenv env rfield.Range rfield.FormalType;
    CheckAttribs cenv env rfield.PropertyAttribs;
    CheckAttribs cenv env rfield.FieldAttribs;
    if contains_byref_ty cenv rfield.FormalType && cenv.reportErrors  then
      errorR(Error("A type would store a byref typed value. This is not permitted by the .NET runtime",tycon.Range))

let CheckTypeDefn cenv env (tycon:Tycon) =
    let m = tycon.Range in
    CheckAttribs cenv env tycon.Attribs;

    if cenv.reportErrors then begin
      if not tycon.IsTypeAbbrev then 
        let typ = (snd (generalize_tcref (mk_local_tcref tycon)))
        let allVirtualMethsInParent = 
            match SuperTypeOfType cenv.g cenv.amap m typ with 
            | Some super -> 
                GetIntrinsicMethInfosOfType cenv.infoReader (None,AccessibleFromSomewhere)  PreferOverrides m super
                |> List.filter (fun minfo -> minfo.IsVirtual)
            | None -> []

        let immediateMeths = GetImmediateIntrinsicMethInfosOfType (None,AccessibleFromSomewhere) cenv.g cenv.amap m typ
        let immediateProps = GetImmediateIntrinsicPropInfosOfType (None,AccessibleFromSomewhere) cenv.g cenv.amap m typ

        let getHash (hash:Dictionary<string,_>) nm = 
             if hash.ContainsKey(nm) then hash.[nm] else []

        let hashOfAllMeths = new Dictionary<string,_>()
        let hashOfAllProps = new Dictionary<string,_>()
        for minfo in immediateMeths do
            let nm = minfo.LogicalName
            let m = (match minfo.ArbitraryValRef with None -> m | Some vref -> vref.DefinitionRange)
            let others = getHash hashOfAllMeths nm
            // abstract/default pairs of duplicate methods are OK
            let IsAbstractDefaultPair (x:MethInfo) (y:MethInfo) = 
                x.IsDispatchSlot && y.IsDefiniteFSharpOverride
            let IsAbstractDefaultPair2 (minfo:MethInfo) (minfo2:MethInfo) = 
                IsAbstractDefaultPair minfo minfo2 || IsAbstractDefaultPair minfo2 minfo
            let checkForDup erasureFlag minfo2 =                         
                     
                not (IsAbstractDefaultPair2 minfo minfo2)
                && minfo.IsInstance = minfo2.IsInstance
                && MethInfosEquivByNameAndSig erasureFlag cenv.g cenv.amap m minfo minfo2

            if others |> List.exists (checkForDup EraseAll) then 
                let suffix = if others |> List.exists (checkForDup EraseNone) then "" else " once tuples, functions and/or units of measure are erased"
                errorR(Error(sprintf "Duplicate method. The method '%s' has the same name and signature as another method in this type%s" nm suffix,m))

            if minfo.NumArgs.Length > 1 && others |> List.exists (fun minfo2 -> not (IsAbstractDefaultPair2 minfo minfo2)) then 
                errorR(Error(sprintf "The method '%s' has curried arguments but has the same name as another method in this type. Methods with curried arguments may not be overloaded. Consider using a method taking tupled arguments." nm,m))

            if minfo.NumArgs.Length > 1 && ParamAttribsOfMethInfo cenv.amap m minfo |> List.existsSquared (fun (isParamArrayArg, isOutArg, optArgInfo) -> isParamArrayArg || isOutArg || optArgInfo <> NotOptional) then 
                errorR(Error(sprintf "Methods with curried arguments may not declare out arguments, 'ParamArray' arguments or optional arguments",m))


            hashOfAllMeths.[nm] <- minfo::others
        for pinfo in immediateProps do
            let nm = pinfo.PropertyName
            let m = (match pinfo.ArbitraryValRef with None -> m | Some vref -> vref.DefinitionRange)
            if hashOfAllMeths.ContainsKey(nm) then 
                errorR(Error(sprintf "Name clash. The property '%s' has the same name as a method in this type" nm,m))
            let others = getHash hashOfAllProps nm
            if pinfo.HasGetter && pinfo.HasSetter && (pinfo.GetterMethod.IsVirtual <>  pinfo.SetterMethod.IsVirtual) then 
                errorR(Error(sprintf "The property '%s' has getter and a setter that do not match. If one is abstract then the other must be as well" nm ,m))
            let checkForDup erasureFlag pinfo2 =                         
                  // abstract/default pairs of duplicate properties are OK
                 let IsAbstractDefaultPair (x:PropInfo) (y:PropInfo) = 
                     x.IsDispatchSlot && y.IsDefiniteFSharpOverride

                 not (IsAbstractDefaultPair pinfo pinfo2 || IsAbstractDefaultPair pinfo2 pinfo)
                 && pinfo.IsStatic = pinfo2.IsStatic
                 && PropInfosEquivByNameAndSig erasureFlag cenv.g cenv.amap m pinfo pinfo2 

            if others |> List.exists (checkForDup EraseAll) then
                let suffix = if others |> List.exists (checkForDup EraseNone) then "" else " once tuples, functions and/or units of measure are erased"
                errorR(Error(sprintf "Duplicate property. The property '%s' has the same name and signature as another property in this type%s" nm suffix,m))
            hashOfAllProps.[nm] <- pinfo::others

        if not (is_interface_typ cenv.g typ) then 
            let hashOfAllMethsInParent = new Dictionary<string,_>()
            for minfo in allVirtualMethsInParent do
                let nm = minfo.LogicalName
                let others = getHash hashOfAllMethsInParent nm
                hashOfAllMethsInParent.[nm] <- minfo::others
            for minfo in immediateMeths do
                if minfo.IsDispatchSlot then 
                    let nm = minfo.LogicalName
                    let m = (match minfo.ArbitraryValRef with None -> m | Some vref -> vref.DefinitionRange)
                    let parentMethsOfSameName = getHash hashOfAllMethsInParent nm 
                    let checkForDup erasureFlag minfo2 = MethInfosEquivByNameAndSig erasureFlag cenv.g cenv.amap m minfo minfo2
                    //if minfo.NumArgs.Length > 1 then 
                    //    warning(Error(sprintf "Abstract methods taking curried arguments Duplicate method. The method '%s' has curried arguments but has the same name as another method in this type. Methods with curried arguments may not be overloaded" nm,(match minfo.ArbitraryValRef with None -> m | Some vref -> vref.DefinitionRange)))
                    if parentMethsOfSameName |> List.exists (checkForDup EraseAll) then
                        let suffix = if parentMethsOfSameName |> List.exists (checkForDup EraseNone) then "" else " once tuples, functions and/or units of measure are erased"
                        errorR(Error(sprintf "Duplicate method. The abstract method '%s' has the same name and signature as an abstract method in an inherited type%s" nm suffix,m))

    end;
    
    // Considers TFsObjModelRepr, TRecdRepr and TFiniteUnionRepr. 
    // [Review] are all cases covered: TILObjModelRepr,TAsmRepr. [Yes - these are FSharp.Core.dll only]
    tycon.AllFieldsArray |> Array.iter (CheckRecdField cenv env tycon);
    vslot_vals_of_tycons [tycon] |> List.iter (type_of_val >> CheckTypePermitByrefs cenv env m); (* check vslots = abstract slots *)
    implements_of_tycon cenv.g tycon |> List.iter (CheckTypePermitByrefs cenv env m);                   (* check implemented interface types *)
    super_of_tycon cenv.g tycon |> CheckTypePermitByrefs cenv env m;                                    (* check super type *)
    if tycon.IsUnionTycon then                             (* This covers finite unions. *)
        tycon.UnionCasesAsList |> List.iter (fun uc ->
            CheckAttribs cenv env uc.Attribs; 
            uc.RecdFields |> List.iter (CheckRecdField cenv env tycon))

    let checkAccess ty = CheckTypeForAccess cenv (fun () -> tycon.DisplayNameWithUnderscoreTypars) tycon.Accessibility tycon.Range ty    
    vslot_vals_of_tycons [tycon] |> List.iter (type_of_val >> checkAccess); (* check vslots = abstract slots *)
    super_of_tycon cenv.g tycon |> checkAccess
    implements_of_tycon cenv.g tycon |> List.iter checkAccess
    if tycon.IsFSharpDelegateTycon then 
        match tycon.TypeReprInfo with 
        | Some (TFsObjModelRepr r) ->
            match r.fsobjmodel_kind with 
            | TTyconDelegate ss ->
                //ss.ClassTypars 
                //ss.MethodTypars 
                ss.FormalReturnType |> Option.iter checkAccess;
                ss.FormalParams |> List.iterSquared (fun (TSlotParam(_,ty,_,_,_,_)) -> checkAccess ty)
            | _ -> ()
        | _ -> ()


    let interfaces = AllSuperTypesOfType cenv.g cenv.amap tycon.Range (generalize_tcref (mk_local_tcref tycon) |> snd) |> List.filter (is_interface_typ cenv.g)

    if cenv.reportErrors then 
        if not tycon.IsTypeAbbrev then 
            let firstInterfaceWithMultipleGenericInstantiations = 
                interfaces |> List.tryPick (fun typ1 -> 
                    interfaces |> List.tryPick (fun typ2 -> 
                        if // same nominal type
                           tcref_eq cenv.g (tcref_of_stripped_typ cenv.g typ1) (tcref_of_stripped_typ cenv.g typ2) &&
                           // different instantiations
                           not (type_equiv_aux EraseAll cenv.g typ1 typ2) 
                        then Some (typ1,typ2)
                        else None))
        
            match firstInterfaceWithMultipleGenericInstantiations with 
            | None -> ()
            | Some (typ1,typ2) -> 
                 errorR(Error(sprintf "This type implements or inherits the same interface at different generic instantiations '%s' and '%s'. This is not permitted in this version of F#" (NicePrint.pretty_string_of_typ cenv.denv typ1) (NicePrint.pretty_string_of_typ cenv.denv typ2),tycon.Range))
        
        // Check struct fields. We check these late because we have to have first checked that the structs are
        // free of cycles
        if tycon.IsStructTycon then 
            tycon.AllInstanceFieldsAsList |> List.iter (fun f -> 
                (* Check if it's marked unsafe *)
                let zeroInitUnsafe = TryFindBoolAttrib cenv.g cenv.g.attrib_DefaultValueAttribute f.FieldAttribs
                if zeroInitUnsafe = Some(true) then
                   let ty' = snd(generalize_tcref (mk_local_tcref tycon))
                   if not (TypeHasDefaultValue cenv.g ty') then 
                       errorR(Error("The type of a field using the 'DefaultValue' attribute must admit default initialization, i.e. have 'null' as a proper value or be a struct type whose fields all admit default initialization. You can use 'DefaultValue(false)' to disable this check",m));
            )
        match tycon.TypeAbbrev with                          (* And type abbreviations *)
         | None     -> ()
         | Some typ -> 
             if contains_byref_ty cenv typ then
               errorR(Error("The type abbreviation contains byrefs. This is not permitted by F#",tycon.Range))

let CheckTypeDefns cenv env tycons = List.iter (CheckTypeDefn cenv env) tycons

//--------------------------------------------------------------------------
// check modules
//--------------------------------------------------------------------------

let rec CheckModuleExpr cenv env x = 
    match x with  
    | TMTyped(mty,def,m) -> 
       let (rpi,mhi) = mk_mdef_to_mtyp_remapping def mty
       let env = { env with mrmi = (mk_repackage_remapping rpi,mhi) :: env.mrmi }
       CheckDefnInModule cenv env def
    
and CheckDefnsInModule cenv env x = x |> List.iter (CheckDefnInModule cenv env)

and CheckNothingAfterEntryPoint cenv m =
    if cenv.entryPointGiven && cenv.reportErrors then 
        errorR(Error("A function labelled with the 'EntryPointAttribute' attribute must be the last declaration in the last file in the compilation sequence",m)) 

and CheckDefnInModule cenv env x = 
    match x with 
    | TMDefRec(tycons,binds,mspecs,m) -> 
        CheckNothingAfterEntryPoint cenv m
        BindVals cenv (List.map var_of_bind binds)
        CheckTypeDefns cenv env tycons; 
        CheckTopBindings cenv env binds;
        List.iter (CheckModuleSpec cenv env) mspecs
    | TMDefLet(bind,m)  -> 
        CheckNothingAfterEntryPoint cenv m
        CheckTopBinding cenv env bind 
        BindVal cenv (var_of_bind bind)
    | TMDefDo(e,m)  -> 
        CheckNothingAfterEntryPoint cenv m
        CheckNoRethrow cenv e;
        CheckExpr cenv env e
    | TMAbstract(def)  -> CheckModuleExpr cenv env def
    | TMDefs(defs) -> CheckDefnsInModule cenv env defs 
and CheckModuleSpec cenv env (TMBind(mspec, rhs)) = 
    CheckTypeDefn cenv env mspec;
    CheckDefnInModule cenv env rhs 

let CheckTopImpl (g,amap,reportErrors,infoReader,internalsVisibleToPaths,viewCcu,denv ,TImplFile(_,_,mexpr),extraAttribs,canContainEntryPoint) =
    let cenv = { g =g ; reportErrors=reportErrors; boundVals=Set.empty; potentialUnboundUsesOfVals=Map.empty; usesQuotations=false; infoReader=infoReader; internalsVisibleToPaths=internalsVisibleToPaths;amap=amap; denv=denv; viewCcu= viewCcu;canContainEntryPoint=canContainEntryPoint; entryPointGiven=false}
    let env = { mrmi=[]; quote=false; limited=false; boundTypars=[] } in
    CheckModuleExpr cenv env mexpr;
    CheckAttribs cenv env extraAttribs;
    if cenv.usesQuotations then 
        viewCcu.UsesQuotations <- true


