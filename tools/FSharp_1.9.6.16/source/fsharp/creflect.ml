// (c) Microsoft Corporation. All rights reserved

#light

module internal Microsoft.FSharp.Compiler.Creflect

open Internal.Utilities
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library


open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Sreflect
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.PrettyNaming
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Typrelns
open Range

#if DEBUG
let verboseCReflect = try (System.Environment.GetEnvironmentVariable("VERBOSE_CREFLECT") <> null) with _ -> false
#else
let verboseCReflect = false
#endif

let mscorlibName = ""
let mkVoidTy = mkNamedTy({ tcName = "System.Void"; tcAssembly=mscorlibName},[])

type cenv = 
    { g: Env.TcGlobals; 
      amap: Import.ImportMap;
      scope: ccu; 
      typeSplices: ResizeArray<Tast.Typar> ;
      exprSplices: ResizeArray<Tast.expr> }

let mk_cenv (g,amap,scope) = 
    { g = g; scope=scope; amap=amap; typeSplices = new ResizeArray<_>() ; exprSplices = new ResizeArray<_>() } 

type env = 
    { //Map from Val to binding index
      vs: ValMap<int>; 
      nvs: int;
      //Map from typar stamps to binding index
      tyvs: I64map.t<int>;
      // Map for values bound by the 
      //     'let v = isinst e in .... if nonnull v then ...v .... ' 
      // construct arising out the compilation of pattern matching. We decode these back to the form
      //     'if istype v then ...unbox v .... ' 
      isinstVals: ValMap<Tast.typ * Tast.expr> 
      substVals: ValMap<Tast.expr> }

let empty_env = 
    { vs=vspec_map_empty(); 
      nvs=0;
      tyvs = I64map.empty ();
      isinstVals = vspec_map_empty() 
      substVals = vspec_map_empty() }

let BindTypar env (v:Typar) = 
    let idx = env.tyvs.Count
    { env with tyvs = I64map.add v.Stamp idx env.tyvs }

let BindTypars env vs = 
    List.fold BindTypar env vs // fold_left because indexes are left-to-right 

let BindFormalTypars env vs = BindTypars { env with tyvs=I64map.empty()} vs 

let BindVal env v = 
    let idx = env.nvs
    { env with vs = vspec_map_add v idx env.vs; nvs = env.nvs + 1 }

let BindIsInstVal env v (ty,e) = 
    { env with isinstVals = vspec_map_add v (ty,e) env.isinstVals }

let BindSubstVal env v e = 
    { env with substVals = vspec_map_add v e env.substVals }


let BindVals env vs = List.fold BindVal env vs // fold left-to-right because indexes are left-to-right 
let BindFlatVals env vs = FlatList.fold BindVal env vs // fold left-to-right because indexes are left-to-right 

exception InvalidQuotedTerm of exn
exception IgnoringPartOfQuotedTermWarning of string * Range.range

let wfail e = raise (InvalidQuotedTerm(e))

// Used to remove TExpr_link for inner expressions in pattern matches
let (|InnerExprPat|) expr = strip_expr expr 

let (|ModuleValueOrMemberUse|_|) cenv expr = 
    let rec loop expr args = 
        match strip_expr expr with 
        | TExpr_app((InnerExprPat(TExpr_val(vref,vFlags,_) as f)),fty,tyargs,actualArgs,m)  when vref.IsMemberOrModuleBinding ->
            Some(vref,vFlags,f,fty,tyargs,actualArgs@args)
        | TExpr_app(f,fty,[],actualArgs,m)  ->
            loop f (actualArgs @ args)
        | (TExpr_val(vref,vFlags,m) as f) when (vref.ActualParent <> ParentNone) -> 
            let fty = type_of_expr cenv.g f
            Some(vref,vFlags,f,fty,[],args)
        | _ -> 
            None
    loop expr []
             

let rec ConvExpr cenv (env : env) (expr: Tast.expr) : Sreflect.ExprData = 
    // Eliminate subsumption coercions for functions. This must be done post-typechecking because we need
    // complete inference types.
    let expr = NormalizeAndAdjustPossibleSubsumptionExprs cenv.g expr

    // Remove TExpr_ref nodes
    let expr = strip_expr expr 

    // Recognize F# object model calls 
    // Recognize applications of module functions. 
    match expr with 
    // Detect expression tree exprSplices
    | TExpr_app(InnerExprPat(TExpr_val(vf,_,_)),_,_,[x0],m) 
           when cenv.g.vref_eq vf cenv.g.splice_expr_vref || cenv.g.vref_eq vf cenv.g.splice_raw_expr_vref -> 
        let idx = cenv.exprSplices.Count
        let ty = type_of_expr cenv.g expr
        match (free_in_expr CollectTyparsAndLocalsNoCaching x0).FreeLocals |> Seq.first (fun v -> if vspec_map_mem v env.vs then Some(v) else None) with 
        | Some v -> errorR(Error(sprintf "The variable '%s' is bound in a quotation but is used as part of a spliced expression. This is not permitted since it may escape its scope" v.DisplayName,v.Range))
        | None -> ()
        cenv.exprSplices.Add(x0);
        mkHole(ConvType cenv env m ty,idx)

    | ModuleValueOrMemberUse cenv (vref,vFlags,f,fty,tyargs,curriedArgs) ->
        let m = range_of_expr expr 

        let (numEnclTypeArgs,_,isNewObj,isSuperInit,isSelfInit,takesInstanceArg,isPropGet,isPropSet) = 
            GetMemberCallInfo cenv.g (vref,vFlags)

        let isMember,tps,curriedArgInfos,retTy = 

            match vref.MemberInfo with 
            | Some _ when not vref.IsExtensionMember -> 
                // This is an application of a member method
                // We only count one argument block for these.
                let tps,curriedArgInfos,retTy,_ = GetTypeOfIntrinsicMemberInCompiledForm cenv.g vref 
                true,tps,curriedArgInfos,retTy
            | _ -> 
                // This is an application of a module value or extension member
                let arities = arity_of_val (deref_val vref) 
                let tps,curriedArgInfos,retTy,_ = GetTopValTypeInCompiledForm cenv.g arities vref.Type m
                false,tps,curriedArgInfos,retTy

        // Compute the object arguments as they appear in a compiled call
        // Strip off the object argument, if any. The curriedArgInfos are already adjusted to compiled member form
        let objArgs,curriedArgs = 
            match takesInstanceArg,curriedArgs with 
            | false,curriedArgs -> [],curriedArgs
            | true,(objArg::curriedArgs) -> [objArg],curriedArgs
            | true,[] -> wfail(InternalError("warning: unexpected missing object argument when generating quotation for call to F# object member "^vref.MangledName,m)) 

        if verboseCReflect then 
            dprintfn "vref.DisplayName = %A,  #objArgs = %A, #curriedArgs = %A" vref.DisplayName objArgs.Length curriedArgs.Length

        // Check to see if there aren't enough arguments or if there is a tuple-arity mismatch
        // If so, adjust and try again
        if curriedArgs.Length < curriedArgInfos.Length ||
           ((fst(List.chop curriedArgInfos.Length curriedArgs),curriedArgInfos) ||> List.exists2 (fun arg argInfo -> 
                       (argInfo.Length > (try_dest_tuple arg).Length))) then

            if verboseCReflect then 
                dprintfn "vref.DisplayName = %A was under applied" vref.DisplayName 
            // Too few arguments or incorrect tupling? Convert to a lambda and beta-reduce the 
            // partially applied arguments to 'let' bindings 
            let topValInfo = 
               match vref.TopValInfo with 
               | None -> error(InternalError("no arity information found for F# value "^vref.MangledName,vref.Range))
               | Some a -> a 

            let expr,exprty = AdjustValForExpectedArity cenv.g m vref vFlags topValInfo 
            ConvExpr cenv env (MakeApplicationAndBetaReduce cenv.g (expr,exprty,[tyargs],curriedArgs,m)) 
        else
        
            // Too many arguments? Chop 
            let curriedArgs,laterArgs = List.chop curriedArgInfos.Length curriedArgs 

            let callR = 
                // We now have the right number of arguments, w.r.t. currying and tupling.
                // Next work out what kind of object model call and build an object model call node. 
                
                // detuple the args
                let untupledCurriedArgs = 
                    (curriedArgs,curriedArgInfos) ||> List.map2 (fun arg curriedArgInfos -> 
                        let numUntupledArgs = curriedArgInfos.Length 
                        (if numUntupledArgs = 0 then [] 
                         elif numUntupledArgs = 1 then [arg] 
                         else try_dest_tuple arg))

                if verboseCReflect then 
                    dprintfn "vref.DisplayName  = %A , after unit adjust, #untupledCurriedArgs = %A, #curriedArgInfos = %d" vref.DisplayName  (List.map List.length untupledCurriedArgs) curriedArgInfos.Length

                if isMember then 
                    // This is an application of a member method
                    // We only count one argument block for these.
                    let callArgs = (objArgs::untupledCurriedArgs) |> List.concat

                    let parentTyconR = ConvTyconRef cenv env vref.MemberActualParent m 
                    let isNewObj = (isNewObj || isSuperInit || isSelfInit) 
                    // The signature types are w.r.t. to the formal context 
                    let envinner = BindFormalTypars env tps 
                    let argTys =  curriedArgInfos |> List.concat |> List.map fst
                    let methArgTypesR = ConvTypes cenv envinner m argTys 
                    let methRetTypeR = ConvReturnType cenv envinner m retTy
                    let methName = vref.CompiledName 
                    let numGenericArgs = tyargs.Length-numEnclTypeArgs  
                    ConvObjectModelCall cenv env m (isPropGet,isPropSet,isNewObj,parentTyconR,methArgTypesR,methRetTypeR,methName,tyargs,numGenericArgs,callArgs)
                else
                    // This is an application of the module value. 
                    ConvModuleValueApp cenv env m vref tyargs untupledCurriedArgs

                
            List.fold (fun fR arg -> mkApp (fR,ConvExpr cenv env arg)) callR laterArgs


    // Blast type application nodes and expression application nodes apart so values are left with just their type arguments 
    | TExpr_app(f,fty,(_ :: _ as tyargs),(_ :: _ as args),m) -> 
      let rfty = reduce_forall_typ cenv.g  fty tyargs
      ConvExpr cenv env (prim_mk_app (prim_mk_app (f,fty) tyargs [] m, rfty) [] args m) 

    // Uses of possibly-polymorphic values 
    | TExpr_app(InnerExprPat(TExpr_val(vref,vFlags,m)),fty,tyargs,[],m2) -> 
      ConvValRef cenv env m vref tyargs

    // Simple applications 
    | TExpr_app(f,fty,tyargs,args,m) -> 
        if nonNil tyargs then wfail(Error("Quotations may not contain uses of generic expressions",m));
        List.fold (fun fR arg -> mkApp (fR,ConvExpr cenv env arg)) (ConvExpr cenv env f) args
    
    // REVIEW: what is the quotation view of literals accessing enumerations? Currently they show up as integers. 
    | TExpr_const(c,m,ty) -> 
        ConvConst cenv env m c ty

    | TExpr_val(vref,vFlags,m) -> 

        ConvValRef cenv env m vref []

    | TExpr_let(bind,body,m,_) -> 

        // The binding may be a compiler-generated binding that gets removed in the quotation presentation
        match ConvLetBind cenv env bind with 
        | None, env -> ConvExpr cenv env body
        | Some(bindR),env -> mkLet(bindR,ConvExpr cenv env body)
        
    | TExpr_letrec(binds,body,m,_) -> 
         let vs = vars_of_Bindings binds
         let vsR = vs |> FlatList.map (ConvVal cenv env) 
         let env = BindFlatVals env vs
         let bodyR = ConvExpr cenv env body 
         let bindsR = FlatList.zip vsR (FlatList.map (rhs_of_bind >> ConvExpr cenv env) binds)
         mkLetRec(FlatList.to_list bindsR,bodyR)

    | TExpr_lambda(_,_,vs,b,_,_,_) -> 
        let v,b = multi_lambda_to_tupled_lambda vs b 
        let vR = ConvVal cenv env v 
        let bR  = ConvExpr cenv (BindVal env v) b 
        mkLambda(vR, bR)

    | TExpr_quote(ast,_,_,_) -> 
        mkQuote(ConvExpr cenv empty_env ast)

    | TExpr_tlambda (_,_,_,m,_,_) -> 
        wfail(Error("Quotations may not contain function declarations that are inferred or declared to be generic. Consider adding some type constraints to make this a valid quoted expression",m))

    | TExpr_match (spBind,m,dtree,tgs,m2,retTy,_) ->
        let typR = ConvType cenv env m retTy 
        ConvDecisionTree cenv env tgs typR dtree 
           
    | TExpr_seq (x0,x1,NormalSeq,_,m)  -> mkSequential(ConvExpr cenv env x0, ConvExpr cenv env x1)
    | TExpr_obj (_,typ,_,_,[TObjExprMethod(TSlotSig(_,ctyp, _,_,_,_),tps,[tmvs],e,_) as tmethod],_,m,_) when is_delegate_typ cenv.g typ -> 
         let f = mk_lambdas m tps tmvs (e,GetFSharpViewOfReturnType cenv.g (rty_of_tmethod cenv.g tmethod))
         let fR = ConvExpr cenv env f 
         let tyargR = ConvType cenv env m ctyp 
         mkDelegate(tyargR, fR)

    | TExpr_static_optimization (tcs,csx,x,m)               -> ConvExpr cenv env x
    | TExpr_tchoose _  -> ConvExpr cenv env (Typrelns.choose_typar_solutions_for_tchoose cenv.g cenv.amap expr)
    | TExpr_seq  (x0,x1,ThenDoSeq,_,m)                        -> mkSequential(ConvExpr cenv env x0, ConvExpr cenv env x1)
    | TExpr_obj (n,typ,basev,basecall,overrides,iimpls,m,_)      -> wfail(Error( "Quotations may not contain object expressions",m))

    | TExpr_op(op,tyargs,args,m) -> 
        match op,tyargs,args with 
        | TOp_ucase ucref,_,_ -> 
            let mkR = ConvUnionCaseRef cenv env ucref m
            let typ = rty_of_uctyp ucref tyargs
            let tyargsR = ConvTypes cenv env m tyargs
            let argsR = ConvExprs cenv env args
            mkSum(mkR,tyargsR,argsR)
        | TOp_tuple,tyargs,_ -> 
            let tyR = ConvType cenv env m (mk_tupled_ty cenv.g tyargs)
            let argsR = ConvExprs cenv env args
            mkTuple(tyR,argsR)
        | TOp_recd (_,tcref),_,_  -> 
            let rgtypR = ConvTyconRef cenv env tcref m
            let tyargsR = ConvTypes cenv env m tyargs
            let typ = mk_tyapp_ty tcref tyargs
            let argsR = ConvExprs cenv env args
            mkRecdMk(rgtypR,tyargsR,argsR)
        | TOp_ucase_field_get (ucref,n),tyargs,[e] -> 
            let tyargsR = ConvTypes cenv env m tyargs
            let tcR,s = ConvUnionCaseRef cenv env ucref m
            let projR = (tcR,s,n)
            mkSumFieldGet( projR, tyargsR,ConvExpr cenv env e)

        | TOp_field_get_addr(rfref),tyargs,_ -> 
            wfail(Error( "Quotations may not contain subexpressions that take the address of a field",m)) 

        | TOp_rfield_get(rfref),tyargs,[] -> 
            wfail(Error( "Quotations may not contain subexpressions that fetch static fields",m)) 

        | TOp_rfield_get(rfref),tyargs,args ->
            let tyargsR = ConvTypes cenv env m tyargs
            let argsR = ConvExprs cenv env args
            let ((parentTyconR,fldName) as projR) = ConvRecdFieldRef cenv env rfref m
            if rfref.TyconRef.IsRecordTycon then
                mkRecdGet(projR,tyargsR,argsR)
            else
                let fspec = rfref.RecdField 
                let tcref = rfref.TyconRef
                if use_genuine_field tcref.Deref fspec then
                    mkFieldGet(projR,tyargsR, argsR)
                else
                    let envinner = BindFormalTypars env (tcref.TyparsNoRange)
                    let propRetTypeR = ConvType cenv envinner m fspec.FormalType
                    mkPropGet( (parentTyconR, fldName,propRetTypeR,[]),tyargsR, argsR)
            

        | TOp_tuple_field_get(n),tyargs,[e] -> 
            let tyR = ConvType cenv env m (mk_tupled_ty cenv.g tyargs)
            mkTupleGet(tyR, n, ConvExpr cenv env e)

        | TOp_asm(([ I_ldfld(_,_,fspec) ] 
                    | [ I_ldsfld (_,fspec) ] 
                    | [ I_ldsfld (_,fspec); I_arith AI_nop ]),_),enclTypeArgs,args  -> 
            let tyargsR = ConvTypes cenv env m enclTypeArgs
            let parentTyconR = ConvIlTypeRef cenv env fspec.EnclosingTypeRef
            let objArg = 
                match args with 
                | obj::rest -> ConvLValueExpr cenv env obj :: rest
                | e -> e
            mkFieldGet( (parentTyconR, fspec.Name),tyargsR, ConvExprs cenv env args)

        | TOp_asm([ I_stfld(_,_,fspec) | I_stsfld (_,fspec) ],_),enclTypeArgs,args  -> 
            let tyargsR = ConvTypes cenv env m enclTypeArgs
            let parentTyconR = ConvIlTypeRef cenv env fspec.EnclosingTypeRef
            let objArg = 
                match args with 
                | obj::rest -> ConvLValueExpr cenv env obj :: rest
                | e -> e
            mkFieldSet( (parentTyconR, fspec.Name),tyargsR, ConvExprs cenv env args)

        | TOp_asm([ I_arith AI_ceq ],_),_,[arg1;arg2]  -> 
            let ty = type_of_expr cenv.g arg1
            let eq = mk_call_generic_equality_outer cenv.g m ty arg1 arg2
            ConvExpr cenv env eq

        | TOp_asm([ I_throw ],_),_,[arg1]  -> 
            let raiseExpr = mk_call_raise cenv.g m (type_of_expr cenv.g expr) arg1 
            ConvExpr cenv env raiseExpr

        | TOp_asm(il,_),_,_                         -> 
            wfail(Error( sprintf "Quotations may not contain inline assembly code (%+A)" il,m))
        | TOp_exnconstr tcref,_,args              -> 
            let rgtypR = ConvTyconRef cenv env tcref m
            let typ = mk_tyapp_ty tcref []
            let parentTyconR = ConvTyconRef cenv env tcref m  
            let argtys = tcref |> rfields_of_ecref  |> List.map (fun rfld -> rfld.FormalType) 
            let methArgTypesR = ConvTypes cenv env m argtys
            let argsR = ConvExprs cenv env args
            mkCtorCall( { ctorParent   = parentTyconR; 
                          ctorArgTypes = methArgTypesR },
                        [], argsR)

        | TOp_rfield_set rfref, tinst,args     -> 
            let argsR = ConvExprs cenv env args
            let tyargsR = ConvTypes cenv env m tyargs
            let ((parentTyconR,fldName) as projR) = ConvRecdFieldRef cenv env rfref m
            if rfref.TyconRef.IsRecordTycon then
                mkRecdSet(projR,tyargsR,argsR)
            else
                let fspec = rfref.RecdField 
                let tcref = rfref.TyconRef
                let parentTyconR = ConvTyconRef cenv env tcref m
                if use_genuine_field tcref.Deref fspec then
                    mkFieldSet( projR,tyargsR, argsR)
                else
                    let envinner = BindFormalTypars env (tcref.TyparsNoRange)
                    let propRetTypeR = ConvType cenv envinner m fspec.FormalType
                    mkPropSet( (parentTyconR, fldName,propRetTypeR,[]),tyargsR, argsR)

        | TOp_exnconstr_field_get(tcref,i),[],args   -> 
            let exnc = strip_eqns_from_ecref tcref
            let fspec = List.nth (exnc.TrueInstanceFieldsAsList) i
            let parentTyconR = ConvTyconRef cenv env tcref m  
            let propRetTypeR = ConvType cenv env m fspec.FormalType
            let callArgsR = ConvExprs cenv env args
            mkPropGet( (parentTyconR, fspec.Name,propRetTypeR,[]),[], callArgsR)


        | TOp_coerce,[tgtTy;srcTy],[x]  -> 
            let xR = ConvExpr cenv env x
            if type_equiv cenv.g tgtTy srcTy then 
                xR
            else
                mkCoerce(ConvType cenv env m tgtTy,xR)
        | TOp_rethrow,[toTy],[]         -> mk_rethrow_library_call cenv.g toTy m |> ConvExpr cenv env (* rebuild rethrow<T>() and Convert *)
        | TOp_lval_op(LGetAddr,vref),[],[] -> mkAddressOf(ConvExpr cenv env (TExpr_val(vref,NormalValUse,m)))
        | TOp_lval_op(LByrefSet,vref),[],[e] -> mkAddressSet(ConvExpr cenv env (TExpr_val(vref,NormalValUse,m)), ConvExpr cenv env e)
        | TOp_lval_op(LSet,vref),[],[e] -> mkAddressSet( mkAddressOf(ConvExpr cenv env (TExpr_val(vref,NormalValUse,m))), ConvExpr cenv env e)
        | TOp_lval_op(LByrefGet,vref),[],[] -> ConvExpr cenv env (TExpr_val(vref,NormalValUse,m))
        | TOp_array,[ty],xa -> mkNewArray(ConvType cenv env m ty,ConvExprs cenv env xa)                            

        | TOp_while _,[],[TExpr_lambda(_,_,[_],test,_,_,_);TExpr_lambda(_,_,[_],body,_,_,_)]  -> 
              mkWhileLoop(ConvExpr cenv env test, ConvExpr cenv env body)

        | TOp_for(_,dir),[],[TExpr_lambda(_,_,[_],lim0,_,_,_);TExpr_lambda(_,_,[_],lim1,_,_,_);body]  -> 
            match dir with 
            | FSharpForLoopUp -> mkForLoop(ConvExpr cenv env lim0,ConvExpr cenv env lim1, ConvExpr cenv env body)
            | _ -> wfail(Error( "Quotations may not contain descending for loops",m))

        | TOp_ilcall((virt,protect,valu,isNewObj,isSuperInit,isProp,_,boxthis,mref),
                      enclTypeArgs,methTypeArgs,tys),[],callArgs -> 
             let parentTyconR = ConvIlTypeRef cenv env mref.EnclosingTypeRef
             let isNewObj = (isNewObj || (isSuperInit = CtorValUsedAsSuperInit) || (isSuperInit = CtorValUsedAsSelfInit))
             let methArgTypesR = List.map (ConvIlType cenv env m) mref.ArgTypes
             let methRetTypeR = ConvIlType cenv env m mref.ReturnType
             let methName = mref.Name
             let isPropGet = isProp && methName.StartsWith("get_",System.StringComparison.Ordinal)
             let isPropSet = isProp && methName.StartsWith("set_",System.StringComparison.Ordinal)
             let tyargs = (enclTypeArgs@methTypeArgs)
             ConvObjectModelCall cenv env m (isPropGet,isPropSet,isNewObj,parentTyconR,methArgTypesR,methRetTypeR,methName,tyargs,methTypeArgs.Length,callArgs)

        | TOp_try_finally _,[resty],[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[_],e2,_,_,_)] -> 
            mkTryFinally(ConvExpr cenv env e1,ConvExpr cenv env e2)

        | TOp_try_catch _,[resty],[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[vf],ef,_,_,_); TExpr_lambda(_,_,[vh],eh,_,_,_)] -> 
            let vfR = ConvVal cenv env vf
            let envf = BindVal env vf
            let vhR = ConvVal cenv env vh
            let envh = BindVal env vh
            mkTryWith(ConvExpr cenv env e1,vfR,ConvExpr cenv envf ef,vhR,ConvExpr cenv envh eh)

        | TOp_bytes _,_,_ -> 
            wfail(Error( "Quotations may not contain literal byte arrays",m))

        | TOp_ucase_proof _,_,[e]       -> ConvExpr cenv env e  // Note: we erase the union case proof conversions when converting to quotations
        | TOp_ucase_tag_get tycr,tinst,[cx]       -> wfail(Error( "Quotations may not contain subexpressions that fetch union case indexes",m))
        | TOp_ucase_field_set (c,i),tinst,[cx;x]  -> wfail(Error( "Quotations may not contain subexpressions that set union case fields",m))
        | TOp_exnconstr_field_set(tcref,i),[],[ex;x] -> wfail(Error( "Quotations may not contain subexpressions that set fields in exception values",m))
        | TOp_get_ref_lval,_,_                     -> wfail(Error( "Quotations may not contain subexpressions that require byref pointers",m))
        | TOp_trait_call (ss),_,_                  -> wfail(Error( "Quotations may not contain subexpressions that call trait members",m))
        | _ -> 
            wfail(InternalError( "Unexpected expression shape",m))

    | _ -> 
        wfail(InternalError(sprintf "unhandled construct in AST: %A" expr,range_of_expr expr))

and ConvLetBind cenv env (bind : Binding) = 
    match bind.Expr with 
    // Map for values bound by the 
    //     'let v = isinst e in .... if nonnull v then ...v .... ' 
    // construct arising out the compilation of pattern matching. We decode these back to the form
    //     'if istype e then ...unbox e .... ' 
    // It's bit annoying that pattern matching does this tranformation. Like all premature optimization we pay a 
    // cost here to undo it.
    | TExpr_op(TOp_asm([ I_isinst _ ],_),[ty],[e],m) -> 
        None, BindIsInstVal env bind.Var (ty,e)
    
    // Remove let <compilerGeneratedVar> = <var> from quotation tree
    | TExpr_val _ when bind.Var.IsCompilerGenerated -> 
        None, BindSubstVal env bind.Var bind.Expr

    // Remove let unionCase = ... from quotation tree
    | TExpr_op(TOp_ucase_proof _,_,[e],m) -> 
        None, BindSubstVal env bind.Var e
    | _ ->
        let v = bind.Var
        let vR = ConvVal cenv env v 
        let rhsR = ConvExpr cenv env bind.Expr
        let envinner = BindVal env v
        Some(vR,rhsR),envinner

and ConvLValueExpr cenv env (expr: Tast.expr) : Tast.expr = 
    match expr with 
    | TExpr_op(op,tyargs,args,m) -> 
        match op with
        | TOp_lval_op(LGetAddr,vref) -> TExpr_val(vref,NormalValUse,m)
        | TOp_field_get_addr(rfref) -> TExpr_op(TOp_rfield_get(rfref),tyargs,args,m)
        | TOp_asm([ I_ldflda(fspec) ],rtys)  -> 
            let args = 
                match args with 
                | obj :: rest -> ConvLValueExpr cenv env obj  :: rest
                | [] -> []
            TExpr_op(TOp_asm([ mk_normal_ldfld(fspec) ],rtys),tyargs,args,m)
        | TOp_asm([ I_ldsflda(fspec) ],rtys)  -> 
            TExpr_op(TOp_asm([ mk_normal_ldsfld(fspec) ],rtys),tyargs,[],m)
(*
        | TOp_asm(([ I_ldelema(ro,shape,tyarg) ] ),_),_,_  -> 
            mkArrayGet( (parentTyconR, fspec.Name),tyargsR, args)
*)
        | _ -> expr
    | _ -> expr
    

and ConvObjectModelCall cenv env m (isPropGet,isPropSet,isNewObj,parentTyconR,methArgTypesR,methRetTypeR,methName,tyargs,numGenericArgs,callArgs) =

    let tyargsR = ConvTypes cenv env m tyargs
    let callArgs = 
        match callArgs with 
        // Strip of the get-address operation for structs
        | obj::rest -> ConvLValueExpr cenv env obj :: rest
        | _ -> callArgs
    let callArgsR = ConvExprs cenv env callArgs

    if isPropGet || isPropSet then 
        let propName = ChopPropertyName methName
        if isPropGet then 
            mkPropGet( (parentTyconR, propName,methRetTypeR,methArgTypesR),tyargsR, callArgsR)
        else 
            let args,rty = List.frontAndBack methArgTypesR
            mkPropSet( (parentTyconR, propName,rty,args),tyargsR, callArgsR)

    elif isNewObj then 
        mkCtorCall( { ctorParent   = parentTyconR; 
                      ctorArgTypes = methArgTypesR },
                    tyargsR, callArgsR)

    else 
        mkMethodCall( { methParent   = parentTyconR; 
                        methArgTypes = methArgTypesR;
                        methRetType  = methRetTypeR;
                        methName     = methName;
                        numGenericArgs=numGenericArgs },
                      tyargsR, callArgsR)

and ConvModuleValueApp cenv env m (vref:ValRef) tyargs (args: Tast.expr list list) =
    match vref.ActualParent with 
    | ParentNone -> failwith "ConvModuleValueApp"
    | Parent(tcref) -> 
        let isProperty = IsCompiledAsStaticValue cenv.g vref.Deref
        let tcrefR = ConvTyconRef cenv env tcref m 
        let tyargsR = ConvTypes cenv env m tyargs 
        let nm = vref.UniqueCompiledName
        let argsR = List.map (ConvExprs cenv env) args
        mkModuleValueApp(tcrefR,nm,isProperty,tyargsR,argsR)

and ConvExprs cenv env args =
    List.map (ConvExpr cenv env) args 

and ConvValRef cenv env m (vref:ValRef) tyargs =
    let v = deref_val vref
    if vspec_map_mem v env.isinstVals then 
        let (ty,e) = vspec_map_find v env.isinstVals
        ConvExpr cenv env (mk_call_unbox cenv.g m ty e)
    elif vspec_map_mem v env.substVals then 
        let e = vspec_map_find v env.substVals
        ConvExpr cenv env e
    elif vspec_map_mem v env.vs then 
        if nonNil tyargs then wfail(InternalError("ignoring generic application of local quoted variable",m));
        mkVar(vspec_map_find v env.vs)
    else 
        let vty = v.Type
        match v.ActualParent with 
        | ParentNone -> 
              // References to local values are embedded by value
              let idx = cenv.exprSplices.Count 
              cenv.exprSplices.Add(mk_call_lift_value cenv.g m vty (expr_for_vref m vref));
              mkHole(ConvType cenv env m vty,idx)
        | Parent _ -> 
              ConvModuleValueApp cenv env m vref tyargs []

and ConvUnionCaseRef cenv env (ucref:UnionCaseRef) m =
    let ucgtypR = ConvTyconRef cenv env ucref.TyconRef m
    let nm = 
        if cenv.g.ucref_eq ucref cenv.g.cons_ucref then "Cons"
        elif cenv.g.ucref_eq ucref cenv.g.nil_ucref then "Empty"
        else ucref.CaseName 
    (ucgtypR,nm) 

and ConvRecdFieldRef cenv env (rfref:RecdFieldRef) m =
    let typR = ConvTyconRef cenv env rfref.TyconRef m
    (typR,rfref.FieldName)

and ConvVal cenv env (v:Val) = 
    let tyR = ConvType cenv env v.Range v.Type
    freshVar (v.UniqueCompiledName, tyR)

and ConvTyparRef cenv env m (tp:Typar) = 
    if I64map.mem tp.Stamp env.tyvs then 
        I64map.find tp.Stamp env.tyvs 
    else
        match ResizeArray.tryfind_index (typar_ref_eq tp) cenv.typeSplices with
        | Some idx -> idx
        | None  ->
            let idx = cenv.typeSplices.Count 
            cenv.typeSplices.Add(tp);
            idx

and FilterMeasureTyargs tys = 
    tys |> List.filter (fun ty -> match ty with TType_measure _ -> false | _ -> true) 

and ConvType cenv env m typ =
    match strip_tpeqns_and_tcabbrevs_and_measureable cenv.g typ with 
    | TType_app(tcref,[tyarg]) when is_il_arr_tcref cenv.g tcref -> 
        mkArrayTy(rank_of_il_arr_tcref cenv.g tcref,ConvType cenv env m tyarg)

    | TType_ucase(UCRef(tcref,_),tyargs) // Note: we erase union case 'types' when converting to quotations
    | TType_app(tcref,tyargs) -> 
        mkNamedTy(ConvTyconRef cenv env tcref m, ConvTypes cenv env m tyargs)

    | TType_fun(a,b)          -> mkFunTy(ConvType cenv env m a,ConvType cenv env m b)
    | TType_tuple(l)          -> ConvType cenv env m (compiled_tuple_ty cenv.g l)
    | TType_var(tp)           -> mkVarTy(ConvTyparRef cenv env m tp)
    | TType_forall(spec,ty)   -> wfail(Error("Inner generic functions are not permitted in quoted expressions. Consider adding some type constraints until this function is no longer generic",m))
    | _ -> wfail(InternalError ("Quotations may not contain this kind of type",m))

and ConvTypes cenv env m typs =
    List.map (ConvType cenv env m) (FilterMeasureTyargs typs)

and ConvConst cenv env m c ty =
    match TryEliminateDesugaredConstants cenv.g m c with 
    | Some e -> ConvExpr cenv env e
    | None ->
        match c with 
        | TConst_bool    i ->  mkBool i
        | TConst_sbyte    i ->  mkSByte i
        | TConst_byte   i ->  mkByte i
        | TConst_int16   i ->  mkInt16 i
        | TConst_uint16  i ->  mkUInt16 i
        | TConst_int32   i ->  mkInt32 i
        | TConst_uint32  i ->  mkUInt32 i
        | TConst_int64   i ->  mkInt64 i
        | TConst_uint64  i ->  mkUInt64 i
        | TConst_float   i ->  mkDouble(i)
        | TConst_float32 i ->  mkSingle(i)
        | TConst_string  s ->  mkString(s)
        | TConst_char    c ->  mkChar c
        | TConst_unit      ->  mkUnit()
        | TConst_zero     ->  mkNull(ConvType cenv env m ty)
        | _ -> wfail(Error ("Quotations may not contain this kind of constant",m))

and ConvDecisionTree cenv env tgs typR x = 
    match x with 
    | TDSwitch(e1,csl,dfltOpt,m) -> 
        let acc = 
            match dfltOpt with 
            | Some d -> ConvDecisionTree cenv env tgs typR d 
            | None -> wfail(Error( "Quotations may not contain this kind of pattern match",m))
        (csl,acc) ||> List.foldBack (fun (TCase(discrim,dtree)) acc -> 
              match discrim with 
              | TTest_unionconstr(ucref,tyargs) -> 
                  let e1R = ConvExpr cenv env e1
                  let ucR = ConvUnionCaseRef cenv env ucref m
                  let tyargsR = ConvTypes cenv env m tyargs
                  mkCond(mkSumTagTest(ucR,tyargsR,e1R),ConvDecisionTree cenv env tgs typR dtree,acc)
              | TTest_const(TConst_bool(true)) -> 
                  let e1R = ConvExpr cenv env e1
                  mkCond(e1R,ConvDecisionTree cenv env tgs typR dtree,acc)
              | TTest_const(TConst_bool(false)) -> 
                  let e1R = ConvExpr cenv env e1
                  mkCond(e1R,acc,ConvDecisionTree cenv env tgs typR dtree)
              | TTest_const(c) -> 
                  let ty = type_of_expr cenv.g e1
                  let eq = mk_call_generic_equality_outer cenv.g m ty e1 (TExpr_const(c,m,ty))
                  let eqR = ConvExpr cenv env eq 
                  mkCond(eqR,ConvDecisionTree cenv env tgs typR dtree,acc)
              | TTest_isnull -> 
                  match e1 with 
                  | TExpr_val(vref,_,_) when vspec_map_mem (deref_val vref) env.isinstVals ->
                      let (ty,e) = vspec_map_find (deref_val vref) env.isinstVals 
                      let tyR = ConvType cenv env m ty
                      let eR = ConvExpr cenv env e
                      mkCond(mkTypeTest(tyR,eR),ConvDecisionTree cenv env tgs typR dtree,acc)
                  | _ -> 
                      let ty = type_of_expr cenv.g e1
                      let eq = mk_call_generic_equality_outer cenv.g m ty e1 (TExpr_const(TConst_zero,m,ty))
                      let eqR = ConvExpr cenv env eq 
                      mkCond(eqR,ConvDecisionTree cenv env tgs typR dtree,acc)
              | TTest_isinst (srcty,tgty) -> 
                  let e1R = ConvExpr cenv env e1
                  mkCond(mkTypeTest(ConvType cenv env m tgty,e1R),ConvDecisionTree cenv env tgs typR dtree,acc)
              | TTest_query _ -> wfail(InternalError( "TTest_query test in quoted expression",m))
              | TTest_array_length _ -> wfail(Error( "Quotations may not contain array pattern matching",m))
             )
      | TDSuccess (args,n) -> 
          let (TTarget(vars,rhs,_)) = tgs.[n] 
          // TAST stores pattern bindings in reverse order for some reason
          // Reverse them here to give a good presentation to the user
          let args = List.rev args
          let vars = List.rev vars
          
          let varsR = vars |> FlatList.map (ConvVal cenv env) 
          let targetR = ConvExpr cenv (BindFlatVals env vars) rhs
          (varsR,args,targetR) |||> FlatList.foldBack2 (fun vR arg acc -> mkLet((vR,ConvExpr cenv env arg), acc) ) 
          
      | TDBind(bind,rest) -> 
          // The binding may be a compiler-generated binding that gets removed in the quotation presentation
          match ConvLetBind cenv env bind with 
          | None, env -> ConvDecisionTree cenv env tgs typR rest 
          | Some(bindR),env -> mkLet(bindR,ConvDecisionTree cenv env tgs typR rest)



// REVIEW: quotation references to items in the assembly being generated 
// are persisted as assembly-qualified-name strings. However this means 
// they are not correctly re-adjusted 
// when static-linking. We could consider persisting these references 
// by creating fake IL metadata (e.g. fields) that refer to the relevant assemblies, which then 
// get fixed-up automatically by IL metadata rewriting. 
and full_tname_of_tref (tr:ILTypeRef) = String.concat "+" (tr.Enclosing @ [tr.Name])

and ConvIlTypeRef cenv env (tr:ILTypeRef) = 
    let tname = full_tname_of_tref tr
    let scoref = tr.Scope
    let assref = 
        // We use the name "." as a special marker for the "local" assembly 
        match scoref with 
        | ScopeRef_local -> "."
        | _ -> scoref.QualifiedName 
    {tcName = tname; tcAssembly = assref}
  
and ConvIlType cenv env m ty = 
    match ty with 
    | Type_boxed tspec | Type_value tspec -> 
      mkNamedTy(ConvIlTypeRef cenv env tspec.TypeRef, 
                List.map (ConvIlType cenv env m) tspec.GenericArgs)
    | Type_array (shape,ty) -> 
      mkArrayTy(shape.Rank,ConvIlType cenv env m ty)
    | Type_tyvar idx -> mkVarTy(int idx)
    | Type_void -> mkVoidTy
    | Type_ptr _ 
    | Type_byref _ 
    | Type_modified _ 
    | Type_fptr _ ->  wfail(Error( "Quotations may not contain this kind of type (A)",m))
  
and ConvTyconRef cenv env (tcref:TyconRef) m = 
    let repr = tcref.CompiledRepresentation
    match repr with 
    | TyrepOpen asm -> 
        match asm with 
        | Type_boxed tspec | Type_value tspec -> 
            ConvIlTypeRef cenv env tspec.TypeRef
        | _ -> 
            wfail(Error( "Quotations may not contain this kind of type (B)",m))
    | TyrepNamed (tref,boxity) -> 
        if tcref.IsLocalRef then 
            (* We use the name "." as a special marker for the "local" assembly *)
            { tcName= full_tname_of_tref tref; tcAssembly="." }
        else 
            ConvIlTypeRef cenv env tref

and ConvReturnType cenv envinner m retTy =
    match retTy with 
    | None -> mkVoidTy 
    | Some ty -> ConvType cenv envinner m ty

let ConvExprPublic (g,amap,scope) env e = 
    let cenv = mk_cenv (g,amap,scope) 
    let astExpr = ConvExpr cenv env e 
    // Add the outer debug range attribute
    let astExpr = 
       let m = range_of_expr e
       let mk_tuple g m es = mk_tupled g m es (List.map (type_of_expr g) es)

       let rangeExpr = 
                mk_tuple cenv.g m 
                    [ mk_string cenv.g m (file_of_range m); 
                      mk_int cenv.g m (start_line_of_range m); 
                      mk_int cenv.g m (start_col_of_range m); 
                      mk_int cenv.g m (end_line_of_range m); 
                      mk_int cenv.g m (end_col_of_range m);  ] 
       let attrExpr = 
           mk_tuple cenv.g m 
              [ mk_string cenv.g m "DebugRange"; rangeExpr ]
       let attrExprR = ConvExpr cenv env attrExpr

       mkAttributedExpression(astExpr,attrExprR)
    cenv.typeSplices |> ResizeArray.to_list |> List.map mk_typar_ty, 
    cenv.exprSplices |> ResizeArray.to_list, 
    astExpr

let ConvMethodBase cenv env (v:Val) = 
        
    let m = v.Range 
    let methName = v.CompiledName
    let parentTyconR = ConvTyconRef cenv env v.MemberActualParent m 

    match v.MemberInfo with 
    | Some(vspr) when not v.IsExtensionMember -> 

        let vref = (mk_local_vref v) 
        let tps,argInfos,retTy,_ = GetTypeOfMemberInMemberForm cenv.g vref 
        let numEnclTypeArgs = vref.MemberApparentParent.TyparsNoRange.Length
        let argTys = argInfos |> List.concat |> List.map fst 

        let isNewObj = (vspr.MemberFlags.MemberKind = MemberKindConstructor)

        // The signature types are w.r.t. to the formal context 
        let envinner = BindFormalTypars env tps 
        let methArgTypesR = ConvTypes cenv envinner m argTys 
        let methRetTypeR = ConvReturnType cenv envinner m retTy

        let numGenericArgs = tps.Length-numEnclTypeArgs  

        if isNewObj then 
             Sreflect.MethodBaseData.Ctor 
                 { ctorParent   = parentTyconR; 
                   ctorArgTypes = methArgTypesR }
        else 
             Sreflect.MethodBaseData.Method 
                { methParent   = parentTyconR; 
                  methArgTypes = methArgTypesR;
                  methRetType  = methRetTypeR;
                  methName     = methName;
                  numGenericArgs=numGenericArgs }
    | _ ->

        Sreflect.MethodBaseData.ModuleDefn
            { Name = methName;
              Module = parentTyconR;
              IsProperty = IsCompiledAsStaticValue cenv.g v }
