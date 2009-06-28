// (c) Microsoft Corporation. All rights reserved

#light

/// An "expr -> expr" pass that eta-expands under-applied values of 
/// known arity to lambda expressions and beta-var-reduces to bind 
/// any known arguments.  The results are later optimized by the peephole 
/// optimizer in opt.ml.
module internal Microsoft.FSharp.Compiler.Lowertop 

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Infos
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.PrettyNaming


let InterceptExpr g cont expr =
    if verbose then dprintf "lower_expr@%a\n" output_range (range_of_expr expr);

    match expr with
    | TExpr_val(vref,flags,m) -> 
        begin match vref.TopValInfo with 
        | Some(arity) -> Some (fst (AdjustValForExpectedArity g m vref flags arity))
        | None -> None
        end
    
    // App (Val v,tys,args) 
    | TExpr_app((TExpr_val (vref,flags,mv) as f0),f0ty,tyargsl,argsl,m) -> 
        // Only transform if necessary, i.e. there are not enough arguments 
        match vref.TopValInfo with
        | Some(topValInfo) ->
            let argsl = List.map cont argsl 
            let f0 = 
                if List.length topValInfo.AritiesOfArgs > List.length argsl 
                then fst(AdjustValForExpectedArity g m vref flags topValInfo) 
                else f0 

            Some (MakeApplicationAndBetaReduce g (f0,f0ty,[tyargsl],argsl,m))
        | None -> None

    | TExpr_app(f0,f0ty,tyargsl,argsl,m) -> 
        Some (MakeApplicationAndBetaReduce g (f0,f0ty, [tyargsl],argsl,m) )

    | _ -> None

let LowerImplFile g ass = 
    RewriteImplFile {pre_intercept = Some(InterceptExpr g);
                      post_transform= (fun _ -> None);
                      under_quotations=false } ass


//----------------------------

let mk_lambda_notype g m uv e = 
    mk_lambda m uv (e,type_of_expr g e) 

let mk_unit_delay_lambda g m e =
    let uv,ue = mk_compgen_local m "unitVar" g.unit_ty
    mk_lambda_notype g m uv e 

let callNonOverloadedMethod g amap m methName ty args =
    match TryFindMethInfo (InfoReader(g,amap)) m AccessibleFromSomeFSharpCode methName ty  with 
    | [] -> error(InternalError("No method called '"^methName^"' was found",m));
    | ILMeth(g,ilMethInfo) :: _  -> mk_il_minfo_call g amap m false ilMethInfo NormalValUse [] false args |> fst
    | _  -> error(InternalError("The method called '"^methName^"' resolved to a non-IL type",m))
                

type LoweredSeqFirstPhaseResult = 
   { /// The code to run in the second phase, to rebuild the expressions, once all code labels and their mapping to program counters have been determined
     /// 'nextVar' is the argument variable for the GenerateNext method that represents the byref argument that holds the "goto" destination for a tailcalling sequence expression
     phase2 : ((* pc: *) ValRef * (* current: *) ValRef * (* nextVar: *) ValRef * Map<ILCodeLabel,int> -> expr * expr * expr)
     /// The labels allocated for one portion of the sequence expression
     labels : int list 
     
     /// The state variables allocated for one portion of the sequence expression (i.e. the local let-bound variables which become state variables)
     stateVars: ValRef list }

let free_in v e = Zset.mem v (free_in_expr CollectTyparsAndLocals e).FreeLocals

/// Analyze a TAST expression to detect the elaborated form of a sequence expression.
/// Then compile it to a state machine represented as a TAST containing goto, return and label nodes. 
/// The returned state machine will also contain references to state variables (from internal 'let' bindings),
/// a program counter (pc) that records the current state, and a current generated value (current).
/// All these variables are then represented as fields in a hosting closure object along with any additional
/// free variables of the sequence expression.
///
/// The analysis is done in two phases. The first phase determines the state variables and state labels (as Abstract IL code labels).
/// We then allocate an integer pc for each state label and proceed with the second phase, which builds two related state machine
/// expressions: one for 'MoveNext' and one for 'Dispose'.
let LowerSeqExpr g amap overallExpr = 
    /// Detect a 'yield x' within a 'seq { ... }'
    let (|SeqYield|_|) expr = 
        match expr with
        | TExpr_app(TExpr_val (vref,_,_),f0ty,tyargsl,[arg],m) when g.vref_eq vref g.seq_singleton_vref ->  
            Some (arg,m)
        | _ -> 
            None
    
    /// Detect a 'expr; expr' within a 'seq { ... }'
    let (|SeqAppend|_|) expr = 
        match expr with
        | TExpr_app(TExpr_val (vref,_,_),f0ty,tyargsl,[arg1;arg2],m) when g.vref_eq vref g.seq_append_vref ->  
            Some (arg1,arg2,m)
        | _ -> 
            None
    
    /// Detect a 'while gd do expr' within a 'seq { ... }'
    let (|SeqWhile|_|) expr = 
        match expr with
        | TExpr_app(TExpr_val (vref,_,_),f0ty,tyargsl,[TExpr_lambda(_,_,[dummyv],gd,_,_,_);arg2],m) 
             when g.vref_eq vref g.seq_generated_vref && 
                  not (free_in dummyv gd) ->  
            Some (gd,arg2,m)
        | _ -> 
            None
    
    let (|SeqTryFinally|_|) expr = 
        match expr with
        | TExpr_app(TExpr_val (vref,_,_),f0ty,tyargsl,[arg1;TExpr_lambda(_,_,[dummyv],compensation,_,_,_)],m) 
            when g.vref_eq vref g.seq_finally_vref && 
                 not (free_in dummyv compensation) ->  
            Some (arg1,compensation,m)
        | _ -> 
            None
    
    let (|SeqUsing|_|) expr = 
        match expr with
        | TExpr_app(TExpr_val (vref,_,_),f0ty,[_;_;elemTy],[resource;TExpr_lambda(_,_,[v],body,_,_,_)],m) 
            when g.vref_eq vref g.seq_using_vref ->  
            Some (resource,v,body,elemTy,m)
        | _ -> 
            None
    
    let (|SeqFor|_|) expr = 
        match expr with
        // Nested for loops are represented by calls to Seq.collect
        | TExpr_app(TExpr_val (vref,_,_),f0ty,[inpElemTy;enumty2;genElemTy],[TExpr_lambda(_,_,[v],body,_,_,_); inp],m) when g.vref_eq vref g.seq_map_concat_vref ->  Some (inp,v,body,genElemTy,m)
        // "for x in e -> e2" is converted to a call to Seq.map by the F# type checker. This could be removed.
        | TExpr_app(TExpr_val (vref,_,_),f0ty,[inpElemTy;genElemTy],[TExpr_lambda(_,_,[v],body,_,_,_); inp],m) when g.vref_eq vref g.seq_map_vref ->  Some (inp,v,mk_call_seq_singleton g m genElemTy body,genElemTy,m)
        | _ -> None
    
    let (|SeqDelay|_|) expr = 
        match expr with
        | TExpr_app(TExpr_val (vref,_,_),f0ty,[elemTy],[TExpr_lambda(_,_,[v],e,_,_,_)],m) when g.vref_eq vref g.seq_delay_vref && not (free_in v e) ->  Some (e,elemTy)
        | _ -> None
    
    let (|SeqEmpty|_|) expr = 
        match expr with
        | TExpr_app(TExpr_val (vref,_,_),f0ty,tyargsl,[],m) when g.vref_eq vref g.seq_empty_vref ->  Some (m)
        | _ -> None
    
    let (|Seq|_|) expr = 
        match expr with
        // use 'seq { ... }' as an indicator
        | TExpr_app(TExpr_val (vref,_,_),f0ty,[elemTy],[e],m) when g.vref_eq vref g.seq_vref ->  Some (e,elemTy) 
        // use 'Seq.delay (fun () -> ...)' as an indicator for sequence expressions in FSharp.Core.dll
        | SeqDelay(e,elemTy) when g.compilingFslib -> Some(e,elemTy)
        | _ -> None

    let allocLabel() =  new_stamp()
        
    let rec Lower  
                 isWholeExpr 
                 isTailCall // is this sequence in tailcall position?
                 noDisposeContinuationLabel // represents the label for the code where there is effectively nothig to do to dispose the iterator for the current state
                 currentDisposeContinuationLabel // represents the label for the code we have to run to dispose the iterator given the current state
                 expr = 

        match expr with 
        | SeqYield(e,m) -> 
            // printfn "found Seq.singleton"
                 //this.pc <- NEXT; 
                 //curr <- e; 
                 //return true;  
                 //NEXT:
            let label = IL.generate_code_label()
            Some { phase2 = (fun (pcv,currv,nextv,pcMap) ->                 
                        let generate = 
                            mk_compgen_seq m 
                                (mk_val_set m pcv (mk_int32 g m pcMap.[label]))
                                (mk_seq SequencePointsAtSeq m 
                                    (mk_val_set m currv e)
                                    (mk_compgen_seq m 
                                        (TExpr_op(TOp_return,[],[mk_one g m],m))
                                        (TExpr_op(TOp_label label,[],[],m))))
                        let dispose = 
                            mk_compgen_seq m 
                                (TExpr_op(TOp_label label,[],[],m))
                                (TExpr_op(TOp_goto currentDisposeContinuationLabel,[],[],m))
                        let checkDispose = 
                            mk_compgen_seq m 
                                (TExpr_op(TOp_label label,[],[],m))
                                (TExpr_op(TOp_return,[],[mk_bool g m (not (noDisposeContinuationLabel = currentDisposeContinuationLabel))],m))
                        generate,dispose,checkDispose);
                   labels=[label];
                   stateVars=[] }

        | SeqDelay(e,elemTy) -> 
            // printfn "found Seq.delay"
            Lower isWholeExpr isTailCall noDisposeContinuationLabel currentDisposeContinuationLabel e // note, using 'isWholeExpr' here prevents 'seq { yield! e }' and 'seq { 0 .. 1000 }' from being compiled
        | SeqAppend(e1,e2,m) -> 
            // printfn "found Seq.append"
            match Lower false false noDisposeContinuationLabel currentDisposeContinuationLabel e1, 
                  Lower false isTailCall noDisposeContinuationLabel currentDisposeContinuationLabel e2 with 
            | Some res1, Some res2 -> 
                Some { phase2 = (fun ctxt -> 
                            let generate1,dispose1,checkDispose1 = res1.phase2 ctxt
                            let generate2,dispose2,checkDispose2 = res2.phase2 ctxt
                            let generate = mk_compgen_seq m generate1 generate2
                            // Order shouldn't matter here, since disposals actions are linked together by goto's  (each ends in a goto).
                            // However leaving as is for now.
                            let dispose = mk_compgen_seq m dispose2 dispose1 
                            let checkDispose = mk_compgen_seq m checkDispose2 checkDispose1
                            generate,dispose,checkDispose);
                       labels= res1.labels @ res2.labels;
                       stateVars = res1.stateVars @ res2.stateVars }
            | _ -> 
                None
        | SeqWhile(e1,e2,m) -> 
            // printfn "found Seq.while"
            match Lower false false noDisposeContinuationLabel currentDisposeContinuationLabel e2 with 
            | Some res2  -> 
                Some { phase2 = (fun ctxt -> 
                            let generate2,dispose2,checkDispose2 = res2.phase2 ctxt
                            let generate = mk_while g (SequencePointAtWhileLoop(range_of_expr e1),e1,generate2,m)
                            let dispose = dispose2
                            let checkDispose = checkDispose2
                            generate,dispose,checkDispose);
                       labels = res2.labels;
                       stateVars = res2.stateVars }
            | _ -> 
                None
        | SeqUsing(resource,v,body,elemTy,m) -> 
            // printfn "found Seq.using"
            Lower false isTailCall noDisposeContinuationLabel currentDisposeContinuationLabel (mk_let (SequencePointAtBinding(range_of_expr body)) m v resource (mk_call_seq_finally g m elemTy body (mk_unit_delay_lambda g m (mk_call_dispose g m v.Type (expr_for_val m v))))) 
        | SeqFor(inp,v,body,genElemTy,m) -> 
            // printfn "found Seq.for"
            let inpElemTy = v.Type
            let inpEnumTy = mk_IEnumerator_ty g inpElemTy
            let enumv, enume = mk_compgen_local m "enum" inpEnumTy
            // [[ use enum = inp.GetEnumerator()
            //    while enum.MoveNext() do
            //       let v = enum.Current 
            //       body ]]            
            Lower false isTailCall noDisposeContinuationLabel currentDisposeContinuationLabel
                 (mk_call_seq_using g m inpEnumTy genElemTy (callNonOverloadedMethod g amap m "GetEnumerator" (mk_seq_ty g inpElemTy) [inp]) 
                     (mk_lambda_notype g m enumv 
                       (mk_call_seq_generated g m genElemTy (mk_unit_delay_lambda g m (callNonOverloadedMethod g amap m "MoveNext" inpEnumTy [enume]))
                          (mk_invisible_let m v (callNonOverloadedMethod g amap m "get_Current" inpEnumTy [enume])
                              body))))
        | SeqTryFinally(e1,compensation,m) -> 
            // printfn "found Seq.try/finally"
            let innerDisposeContinuationLabel = IL.generate_code_label()
            match Lower false false noDisposeContinuationLabel innerDisposeContinuationLabel e1 with 
            | Some res1  -> 
                Some { phase2 = (fun ((pcv,currv,_,pcMap) as ctxt) -> 
                            let generate1,dispose1,checkDispose1 = res1.phase2 ctxt
                            let generate = 
                                // copy the compensation expression - one copy for the success continuation and one for the exception
                                let compensation = copy_expr g CloneAllAndMarkExprValsAsCompilerGenerated compensation
                                mk_compgen_seq m 
                                    // set the PC to the inner finally, so that if an exception happens we run the right finally 
                                    (mk_compgen_seq m 
                                        (mk_val_set m pcv (mk_int32 g m pcMap.[innerDisposeContinuationLabel]))
                                        generate1 )
                                    // set the PC past the try/finally before trying to run it, to make sure we only run it once
                                    (mk_compgen_seq m 
                                        (TExpr_op(TOp_label innerDisposeContinuationLabel,[],[],m))
                                        (mk_compgen_seq m 
                                            (mk_val_set m pcv (mk_int32 g m pcMap.[currentDisposeContinuationLabel]))
                                            compensation))
                            let dispose = 
                                // generate inner try/finallys, then outer try/finallys
                                mk_compgen_seq m 
                                    dispose1 
                                    // set the PC past the try/finally before trying to run it, to make sure we only run it once
                                    (mk_compgen_seq m 
                                        (TExpr_op(TOp_label innerDisposeContinuationLabel,[],[],m))
                                        (mk_compgen_seq m 
                                            (mk_val_set m pcv (mk_int32 g m pcMap.[currentDisposeContinuationLabel]))
                                            (mk_compgen_seq m 
                                                compensation
                                                (TExpr_op(TOp_goto currentDisposeContinuationLabel,[],[],m)))))
                            let checkDispose = 
                                mk_compgen_seq m 
                                    checkDispose1 
                                    (mk_compgen_seq m 
                                        (TExpr_op(TOp_label innerDisposeContinuationLabel,[],[],m))
                                        (TExpr_op(TOp_return,[],[mk_true g m (* yes, we must dispose!!! *) ],m)))

                            generate,dispose,checkDispose);
                       labels = innerDisposeContinuationLabel :: res1.labels;
                       stateVars = res1.stateVars }
            | _ -> 
                None
        | SeqEmpty(m) -> 
            // printfn "found Seq.empty"
            Some { phase2 = (fun _ -> 
                            let generate = mk_unit g  m
                            let dispose = TExpr_op(TOp_goto currentDisposeContinuationLabel,[],[],m)
                            let checkDispose = TExpr_op(TOp_goto currentDisposeContinuationLabel,[],[],m)
                            generate,dispose,checkDispose);
                   labels = []
                   stateVars = [] }
        | TExpr_seq(x1,x2,NormalSeq,ty,m) -> 
            match Lower false isTailCall noDisposeContinuationLabel currentDisposeContinuationLabel x2 with 
            | Some res2-> 
                // printfn "found sequential execution"
                Some { phase2 = (fun ctxt -> 
                            let generate2,dispose2,checkDispose2 = res2.phase2 ctxt
                            let generate = TExpr_seq(x1,generate2,NormalSeq,ty,m)
                            let dispose = dispose2
                            let checkDispose = checkDispose2
                            generate,dispose,checkDispose);
                       labels = res2.labels
                       stateVars = res2.stateVars }
            | None -> None
        | TExpr_let(bind,e2,m,_) -> 
            match Lower false isTailCall noDisposeContinuationLabel currentDisposeContinuationLabel e2 with 
            | Some res2 ->
                if bind.Var.IsCompiledAsTopLevel then 
                    // printfn "found top level let  "
                    Some { phase2 = (fun ctxt -> 
                                let generate2,dispose2,checkDispose2 = res2.phase2 ctxt
                                let generate = mk_let_bind m bind generate2
                                let dispose = dispose2
                                let checkDispose = checkDispose2
                                generate,dispose, checkDispose);
                           labels=res2.labels;
                           stateVars = res2.stateVars }
                else
                    // printfn "found state variable %s" bind.Var.DisplayName
                    let (TBind(v,e,sp)) = bind
                    let sp,spm = 
                        match sp with 
                        | SequencePointAtBinding m -> SequencePointsAtSeq,m 
                        | _ -> SuppressSequencePointOnExprOfSequential,range_of_expr e
                    let vref = mk_local_vref v
                    Some { phase2 = (fun ctxt -> 
                                let generate2,dispose2,checkDispose2 = res2.phase2 ctxt
                                let generate = 
                                    mk_compgen_seq m 
                                        (mk_seq sp m 
                                            (mk_val_set spm vref e) 
                                            generate2) 
                                        // zero out the current value to free up its memory
                                        (mk_val_set m vref (mk_ilzero (m,vref.Type)))  
                                let dispose = dispose2
                                let checkDispose = checkDispose2
                                generate,dispose,checkDispose);
                           labels=res2.labels
                           stateVars = vref::res2.stateVars }
            | None -> 
                None

        | TExpr_match (spBind,exprm,pt,targets,m,ty,_) when targets |> Array.forall (fun (TTarget(vs,e,spTarget)) -> isNil vs) ->
            // lower all the targets. abandon if any fail to lower
            let tgl = targets |> Array.map (fun (TTarget(vs,e,spTarget)) -> Lower false isTailCall noDisposeContinuationLabel currentDisposeContinuationLabel e) |> Array.to_list
            // LIMITATION: non-trivial pattern matches involving or-patterns or active patterns where bindings can't be 
            // transferred to the r.h.s. are not yet compiled. 
            if tgl |> List.forall isSome then 
                let tgl = List.map Option.get tgl
                let labs = tgl |> List.collect (fun res -> res.labels)
                let stateVars = tgl |> List.collect (fun res -> res.stateVars)
                Some { phase2 = (fun ctxt -> 
                            let gtgs,disposals,checkDisposes = 
                                (Array.to_list targets,tgl) 
                                  ||> List.map2 (fun (TTarget(vs,_,spTarget)) res -> 
                                        let generate,dispose,checkDispose = res.phase2 ctxt
                                        let gtg = TTarget(vs,generate,spTarget)
                                        gtg,dispose,checkDispose)
                                  |> List.unzip3  
                            let generate = prim_mk_match (spBind,exprm,pt,Array.of_list gtgs,m,ty)
                            let dispose = if isNil disposals then mk_unit g m else List.reduce (mk_compgen_seq m) disposals
                            let checkDispose = if isNil checkDisposes then mk_false g m else List.reduce (mk_compgen_seq m) checkDisposes
                            generate,dispose,checkDispose);
                       labels=labs;
                       stateVars = stateVars }
            else
                None

        // yield! e ---> (for x in e -> x)
        //
        // Design choice: we compile 'yield! e' as 'for x in e do yield x'. 
        //
        // Note, however, this leads to a loss of tailcalls: the case not 
        // handled correctly yet is sequence expressions that use yield! in the last position
        // This can give rise to infinite iterator chains when implemented by the naive expansion to 
        // “for x in e yield e”. For example consider this:
        //
        // let rec rwalk x = {  yield x; 
        //                      yield! rwalk (x + rand()) }
        //
        // This is the moral equivalent of a tailcall optimization. These also don’t compile well 
        // in the C# compilation model

        | arbitrarySeqExpr -> 
            let m = range_of_expr arbitrarySeqExpr
            if isWholeExpr then 
                // printfn "FAILED - not worth compiling an unrecognized immediate yield! %s " (string_of_range m)
                None
            else
                let conforms_to_seq g ty = is_stripped_tyapp_typ g ty && tcref_eq g (tcref_of_stripped_typ g ty) g.tcref_System_Collections_Generic_IEnumerable
                match SearchEntireHierarchyOfType (conforms_to_seq g) g amap m (type_of_expr g arbitrarySeqExpr) with
                | None -> 
                    // printfn "FAILED - yield! did not yield a sequence! %s" (string_of_range m)
                    None
                | Some ty -> 
                    // printfn "found yield!"
                    let inpElemTy = List.hd (tinst_of_stripped_typ g ty)
                    if isTailCall then 
                             //this.pc <- NEXT; 
                             //nextEnumerator <- e; 
                             //return 2;  
                             //NEXT:
                        let label = IL.generate_code_label()
                        Some { phase2 = (fun (pcv,currv,nextv,pcMap) ->                 
                                    let generate = 
                                        mk_compgen_seq m 
                                            (mk_val_set m pcv (mk_int32 g m pcMap.[label]))
                                            (mk_seq SequencePointsAtSeq m 
                                                (mk_lval_set m nextv arbitrarySeqExpr)
                                                (mk_compgen_seq m 
                                                    (TExpr_op(TOp_return,[],[mk_two g m],m))
                                                    (TExpr_op(TOp_label label,[],[],m))))
                                    let dispose = 
                                        mk_compgen_seq m 
                                            (TExpr_op(TOp_label label,[],[],m))
                                            (TExpr_op(TOp_goto currentDisposeContinuationLabel,[],[],m))
                                    let checkDispose = 
                                        mk_compgen_seq m 
                                            (TExpr_op(TOp_label label,[],[],m))
                                            (TExpr_op(TOp_return,[],[mk_false g m],m))
                                    generate,dispose,checkDispose);
                               labels=[label];
                               stateVars=[] }
                    else
                        let v,ve = mk_compgen_local m "v" inpElemTy
                        Lower false isTailCall noDisposeContinuationLabel currentDisposeContinuationLabel (mk_call_seq_map_concat g m inpElemTy inpElemTy (mk_lambda_notype g m v (mk_call_seq_singleton g m inpElemTy ve)) arbitrarySeqExpr)
            

    match overallExpr with 
    | Seq(e,ty) -> 
        // printfn "found seq { ... } or Seq.delay (fun () -> ...) in FSharp.Core.dll"
        let m = range_of_expr e
        let initLabel = IL.generate_code_label()
        let noDisposeContinuationLabel = IL.generate_code_label()
        match Lower true true noDisposeContinuationLabel noDisposeContinuationLabel e with 
        | Some res ->
            let labs = res.labels
            let stateVars = res.stateVars 
            // printfn "successfully lowered, found %d state variables and %d labels!" stateVars.Length labs.Length
            let pcv,pce = mk_mut_compgen_local m "pc" g.int32_ty
            let currv,curre = mk_mut_compgen_local m "current" ty
            let nextv,nexte = mk_mut_compgen_local m "next" (mk_byref_typ g (mk_seq_ty g ty))
            let nextvref = mk_local_vref nextv
            let pcvref = mk_local_vref pcv
            let currvref = mk_local_vref currv
            let pcs = labs |> List.mapi (fun i _ -> i + 2)
            let pcDone = labs.Length + 2
            let pcInit = 1
            let pc2lab  = Map.of_list ((pcInit,initLabel) :: (pcDone,noDisposeContinuationLabel) :: List.zip pcs labs)
            let lab2pc = Map.of_list ((initLabel,pcInit) :: (noDisposeContinuationLabel,pcDone) :: List.zip labs pcs)
            let stateMachineExpr,disposalExpr, checkDisposeExpr = res.phase2 (pcvref,currvref,nextvref,lab2pc)
            // Add on the final 'return false' to indicate the iteration is complete
            let stateMachineExpr = 
                mk_compgen_seq m 
                    stateMachineExpr 
                    (mk_compgen_seq m 
                        // set the pc to "finished"
                        (mk_val_set m pcvref (mk_int32 g m pcDone))
                        (mk_compgen_seq m 
                            (TExpr_op(TOp_label noDisposeContinuationLabel,[],[],m))
                            (mk_compgen_seq m 
                                // zero out the current value to free up its memory
                                (mk_val_set m currvref (mk_ilzero (m,currvref.Type)))
                                (TExpr_op(TOp_return,[],[mk_zero g m],m)))))
            let disposalExpr = 
                mk_compgen_seq m 
                    disposalExpr 
                    (mk_compgen_seq m 
                        (TExpr_op(TOp_label noDisposeContinuationLabel,[],[],m))
                        (mk_compgen_seq m 
                            // set the pc to "finished"
                            (mk_val_set m pcvref (mk_int32 g m pcDone))
                            // zero out the current value to free up its memory
                            (mk_val_set m currvref (mk_ilzero (m,currvref.Type)))))
            let checkDisposeExpr = 
                mk_compgen_seq m 
                    checkDisposeExpr 
                    (mk_compgen_seq m 
                        (TExpr_op(TOp_label noDisposeContinuationLabel,[],[],m))
                        (TExpr_op(TOp_return,[],[mk_false g m],m)))
                            
            let addJumpTable isDisposal expr = 
                let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,m )
                let mkGotoLabelTarget lab = mbuilder.AddResultTarget(TExpr_op(TOp_goto lab,[],[],m),SuppressSequencePointAtTarget)
                let dtree = 
                  TDSwitch(pce,
                           [ 
                             // no disposal action for the initial state (pc = 1)
                             if isDisposal then 
                                 yield mk_case(TTest_const(TConst_int32 pcInit),mkGotoLabelTarget noDisposeContinuationLabel) 
                             for pc in pcs do 
                                 yield mk_case(TTest_const(TConst_int32 pc),mkGotoLabelTarget pc2lab.[pc])
                             yield mk_case(TTest_const(TConst_int32 pcDone),mkGotoLabelTarget noDisposeContinuationLabel) ],
                           Some(mkGotoLabelTarget pc2lab.[pcInit]),
                           m)
                           
                let table = mbuilder.Close(dtree,m,g.int_ty)
                mk_compgen_seq m table (mk_compgen_seq m (TExpr_op(TOp_label initLabel,[],[],m)) expr)

            let stateMachineExprWithJumpTable = addJumpTable false stateMachineExpr
            let disposalExprWithJumpTable = addJumpTable true disposalExpr  
            let checkDisposeExprWithJumpTable = addJumpTable true checkDisposeExpr  
            // all done, no return the results
            Some (nextvref, pcvref,currvref,stateVars,stateMachineExprWithJumpTable,disposalExprWithJumpTable,checkDisposeExprWithJumpTable,ty,m)

        | None -> 
            // printfn "FAILED: no compilation found! %s" (string_of_range m)
            None
    | _ -> None
    
    