//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Control

    #nowarn "44" // This construct is deprecated. This method will be removed in the next version of F# and may no longer be used. Consider using Async.RunWithContinuations
    #nowarn "67" // This type test or downcast will always hold

    open System
    open System.Diagnostics
    open System.Diagnostics.CodeAnalysis
    open System.Threading
    open System.IO
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Control
    open Microsoft.FSharp.Collections

#if FX_ATLEAST_40
    open System.Threading.Tasks
#endif

#if FX_NO_OPERATION_CANCELLED
    type OperationCanceledException(s : System.String) =
        inherit System.Exception(s)
#endif

    /// We use our own internal implementation of queues to avoid a dependency on System.dll
    type Queue<'T>() =  //: IEnumerable<T>, ICollection, IEnumerable
    
        let mutable array = [| |]
        let mutable head = 0
        let mutable size = 0
        let mutable tail = 0

        let SetCapacity(capacity) =
            let destinationArray = Array.zeroCreate(capacity);
            if (size > 0) then 
                if (head < tail) then 
                    System.Array.Copy(array, head, destinationArray, 0, size);
                else
                    System.Array.Copy(array, head, destinationArray, 0, array.Length - head);
                    System.Array.Copy(array, 0, destinationArray, array.Length - head, tail);
            array <- destinationArray;
            head <- 0;
            tail <- if (size = capacity) then 0 else size;

        member x.Dequeue() =
            if (size = 0) then
                failwith "Dequeue"
            let local = array.[head];
            array.[head] <- Unchecked.defaultof<'T>
            head <- (head + 1) % array.Length;
            size <- size - 1;
            local

        member this.Enqueue(item) =
            if (size = array.Length) then 
                let capacity = int ((int64 array.Length * 200L) / 100L);
                let capacity = max capacity (array.Length + 4)
                SetCapacity(capacity);
            array.[tail] <- item;
            tail <- (tail + 1) % array.Length;
            size <- size + 1

        member x.Count = size

 
    
    [<Sealed>]
    type Unexpected =
        /// cases that should never occur except due to bad code in this file
        static member Fail msg =
            //System.Diagnostics.Debug.Assert(false, msg)
            raise <| new InvalidOperationException(msg)

    [<Sealed>]
    type AsyncGroup() = 
        let mutable cancellationFlag = false
        let mutable cancellationMessage = None
        let cancelEvent = new Event<_>()
        member x.TriggerCancel(?message:string) =
            let message = defaultArg message "an exception happened on another parallel process"
            //Console.WriteLine(" ** TriggerCancel on group {0}", x.GetHashCode())
            cancellationMessage <- Some message
            cancellationFlag <- true; cancelEvent.Trigger(message)
                    
        member x.Cancel = cancelEvent.Publish

        [<SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>] // inlined
        member x.Test() = cancellationFlag

        [<SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>] // inlined
        member x.GetMessage() = cancellationMessage

        member x.CreateSubGroup() =
            let innerAsyncGroup = new AsyncGroup()
            let handler = Handler(fun _ msg -> innerAsyncGroup.TriggerCancel(msg))
            //Console.WriteLine(" ** AddHandler for subgroup {0} to group {1}", innerAsyncGroup.GetHashCode(), x.GetHashCode); 
            x.Cancel.AddHandler(handler);
            innerAsyncGroup, (fun () -> x.Cancel.RemoveHandler(handler))
    // Some versions of F# don't always take tailcalls to functions returning 'unit' because this
    // is represented as type 'void' in the underlying IL.
    // Hence we don't use the 'unit' return type here, and instead invent our own type.
    type FakeUnitValue =
        | FakeUnit


    type cont<'T> = ('T -> FakeUnitValue)
    type econt = (exn -> FakeUnitValue)
    type ccont = (OperationCanceledException -> FakeUnitValue)

    [<StructuralComparison(false); StructuralEquality(false)>]
    type AsyncParams<'T> =
        { group : AsyncGroup;
          cont : cont<'T>;
          econt : econt;
          ccont : ccont;
          blocked : System.Threading.Thread; 
        }
        
    [<StructuralComparison(false); StructuralEquality(false)>]
    type Async<'T> =
        P of (AsyncParams<'T> -> FakeUnitValue)

    module AsyncBuilderImpl =
        // To consider: augment with more exception traceability information
        // To consider: add the ability to suspend running ps in debug mode
        // To consider: add the ability to trace running ps in debug mode
        open System
        open System.Threading
        open System.IO

        let fake () = FakeUnit
        let unfake FakeUnit = ()
        let ignoreFake _ = FakeUnit


        let defaultAsyncGroup = ref (AsyncGroup())

        type Result<'T> =
            | Error of exn
            | Ok of 'T

        // Apply f to x and call either the continuation or exception continuation depending what happens
        let inline protect econt f x (cont : 'T -> FakeUnitValue) : FakeUnitValue =
            // This is deliberately written in a completely allocation-free style
            let mutable res = Unchecked.defaultof<_>
            let mutable exn = null
            try 
                res <- f x
            with 
                // Note: using a :? catch keeps FxCop happy
                | :? System.Exception as e -> 
                    exn <- e
            match exn with 
            | null -> 
                // NOTE: this must be a tailcall
                cont res
            | exn -> 
                // NOTE: this must be a tailcall
                econt exn

        type TailCallCount = 
#if FX_NO_THREAD_STATIC // no thread static on .NET CF 2.0. The hijack implementation is still a reasonable approximation without this - hijacks are just aken more often
#else
            [<System.ThreadStatic>]
#endif
            [<DefaultValue>]
            static val mutable count : int 

        let tailcallLim = 300 

            //if sizeof<nativeint> = 8 then
                // Hijack every so often
            //else
            //    f()

        // Reify exceptional results as exceptions
        let commit res =
            match res with
            | Ok res -> res
            | Error exn -> raise exn

        // Reify exceptional results as exceptions
        let commitWithPossibleTimeout res =
            match res with
            | None -> raise (System.TimeoutException())
            | Some res -> commit res

        let cancelT (args:AsyncParams<_>) =
            let msg = args.group.GetMessage() |> Option.get
            args.ccont (new OperationCanceledException(msg))
                   

        // Generate async computation which calls its continuation with the given result
        let resultA x = 
            P (fun args -> 
                if args.group.Test() then
                    cancelT args
                else
                    args.cont x)

        // Apply the underlying implementation of an async computation to its inputs
        let inline invokeA (P pf) args  = pf args

        // The primitive bind operation. Generate a process that runs the first process, takes
        // its result, applies f and then runs the new process produced. Hijack if necessary and 
        // run 'f' with exception protection
        let bindA p1 f  =
            P (fun args ->
                if args.group.Test() then
                    cancelT args
                else

                    let bind() =
                        let cont a = protect args.econt f a (fun p2 -> invokeA p2 args)
                        invokeA p1 { cont=cont;ccont=args.ccont;econt=args.econt;blocked=args.blocked;group=args.group }

                    // .NET 2.0 JIT 64 doesn't always take tailcalls correctly. Hence hijack the continuation onto a new work item
                    // every so often by counting the number of tailcalls we make from each particular O/S thread.
                    // Note this also gives more interleaving of I/O.
                    TailCallCount.count <- TailCallCount.count + 1
                    if TailCallCount.count >= tailcallLim then
                        TailCallCount.count <- 0
#if FX_NO_SYNC_CONTEXT
                        //if not (ExecutionContext.IsFlowSuppressed()) then ExecutionContext.SuppressFlow() |> ignore
                        if ThreadPool.QueueUserWorkItem(fun _ ->  bind() |> unfake) then
                            FakeUnit
                        else
                            bind()
#else
                        let ctxt = System.Threading.SynchronizationContext.Current 
                        if (match ctxt with null -> true | _ -> false)
                           || (match args.blocked with 
                               | null -> false 
                               | t -> t = System.Threading.Thread.CurrentThread)  then 

                            //if not (ExecutionContext.IsFlowSuppressed()) then ExecutionContext.SuppressFlow() |> ignore
                            if ThreadPool.QueueUserWorkItem(fun _ ->  bind() |> unfake) then
                                FakeUnit
                            else
                                bind()
                        else 
                            // Reschedule in the same synchronization context if one is available
                            ctxt.Post(SendOrPostCallback(fun _ ->  bind() |> unfake),null)
                            FakeUnit
#endif
                    else
                        // NOTE: this must be a tailcall
                        bind())



        // callA = "bindA (return x) f", though no hijack check
        let callA f x =
            P (fun args ->
                if args.group.Test() then
                    cancelT args
                else
                    protect args.econt f x (fun p2 -> invokeA p2 args))

        // delayPrim = "bindA (return ()) f", though no hijack check
        let delayA f = callA f ()

        // Call p but augment the normal, exception and cancel continuations with a call to finallyFunction.
        // If the finallyFunction raises an exception then call the original exception continuation
        // with the new exception. If exception is raised after a cancellation, exception is ignored
        // and cancel continuation is called.
        let tryFinallyA finallyFunction p  =
            P (fun args ->
                if args.group.Test() then
                    cancelT args
                else
                    // The new continuation runs the finallyFunction and resumes the old continuation
                    // If an exception is thrown we continue with the previous exception continuation.
                    let cont b     = protect args.econt finallyFunction () (fun () -> args.cont b)
                    // The new exception continuation runs the finallyFunction and then runs the previous exception continuation.
                    // If an exception is thrown we continue with the previous exception continuation.
                    let econt exn  = protect args.econt finallyFunction () (fun () -> args.econt exn)
                    // The cancellation continuation runs the finallyFunction and then runs the previous cancellation continuation.
                    // If an exception is thrown we continue with the previous cancellation continuation (the exception is lost)
                    let ccont cexn = protect (fun _ -> args.ccont cexn) finallyFunction () (fun () -> args.ccont cexn)
                    invokeA p { args with cont = cont; econt = econt; ccont = ccont } )

        let protectedPrimitive f =
            P (fun args ->
                if args.group.Test() then
                    cancelT args
                else
                    try f args
                    with exn -> args.econt exn)

        // Re-rout the exception continuation to call to catchFunction. This generates
        // a new process. If catchFunction or the new process fail
        // then call the original exception continuation with the failure.
        let tryWithPrimA catchFunction p =
            P (fun args ->
                if args.group.Test() then
                    cancelT args
                else 
                    let econt exn = invokeA (callA catchFunction exn) args
                    invokeA p { args with econt = econt })

        let tryWithA    f p = tryWithPrimA f p
        let doneA           = resultA()
        let fakeUnitA       = resultA FakeUnit
        let usingA (r:'T :> IDisposable) f =  tryFinallyA (fun () -> r.Dispose()) (callA f r)
        let raiseA exn = P (fun args -> args.econt (exn :> Exception))
        let ignoreA p = bindA p (fun _ -> doneA)

        let catchA p =
            P (fun args ->
                invokeA p { group = args.group;
                            blocked = args.blocked;
                            cont = (Ok >> args.cont); 
                            ccont = args.ccont; 
                            econt = (Error >> args.cont) })

        let whenCancelledA finallyFunction p =
            P (fun args ->
                invokeA p { args with ccont = (fun exn -> finallyFunction(exn); args.ccont(exn)) })

        let primitiveA f = 
            protectedPrimitive (fun args -> f (args.cont,args.econt, args.ccont))

        // Start on the current thread. Only Async.Run uses this.
        let startA blocked asyncGroup cont econt ccont p =
            invokeA p { group = asyncGroup; cont = cont; econt = econt; ccont = ccont; blocked = blocked }
            |> unfake

        // Start as a work item in the thread pool
        let queueWorkItem asyncGroup cont econt ccont p =
            let args = { group = asyncGroup; cont = cont; econt = econt; ccont = ccont; blocked = null }
            //if not (ExecutionContext.IsFlowSuppressed()) then ExecutionContext.SuppressFlow() |> ignore
            if not (ThreadPool.QueueUserWorkItem(fun _ -> invokeA p args |> unfake)) then
                Unexpected.Fail "failed to queue user work item"

        let getAsyncGroup()  =
            P (fun args -> args.cont args.group)

        let rec whileA gd prog =
            if gd() then bindA prog (fun () -> whileA gd prog) else doneA

    open AsyncBuilderImpl
    
    [<Sealed>]
    type AsyncBuilder() =
        member b.Zero() = doneA
        member b.Delay(f) = delayA(f)
        member b.Return(x) = resultA(x)
        member b.Bind(p1,p2) = bindA p1 p2
        member b.Using(g,p) = usingA g p
        member b.While(gd,prog) = whileA gd prog
        member b.For(e:seq<_>,prog) =
            usingA (e.GetEnumerator()) (fun ie ->
                whileA
                    (fun () -> ie.MoveNext())
                    (delayA(fun () -> prog ie.Current)))

        member b.Combine(p1,p2) = bindA p1 (fun () -> p2)
        member b.TryFinally(p,cf) = tryFinallyA cf p
        member b.TryWith(p,cf) = tryWithA cf p

    module AsyncImpl = 
        let async = AsyncBuilder()

#if FX_NO_SYNC_CONTEXT
#else
        let post (syncContext : SynchronizationContext) trigger args =
                match syncContext with
                | null -> ()
                | ctxt -> ctxt.Post(SendOrPostCallback(fun _ -> trigger args),state=null)
#endif


        let boxed(a:Async<'b>) : Async<obj> = async { let! res = a in return box res }

        // A utility type to provide a synchronization point between an asynchronous computation and callers waiting
        // on the result of that computation.
        [<Sealed>]        
        [<AutoSerializable(false)>]        
        type ResultCell<'T>() =
            // Use the reference cell as a lock
            let mutable result = None
            // The WaitHandle event for the result. Only created if needed, and set to None when disposed.
            let mutable resEvent = null
            let mutable disposed = false
            // All writers of result are protected by lock on syncRoot.
            // Note: don't use 'resEvent' as a lock - it is option-valued. Option values use 'null' as a representation.
            let syncRoot = new Object()

            member x.GetWaitHandle() =
                lock syncRoot (fun () -> 
                    if disposed then 
                        raise (System.ObjectDisposedException("ResultCell"));
                    match resEvent with 
                    | null ->
                        // Start in signalled state if a result is already present.
                        let ev = new ManualResetEvent(result.IsSome)
                        resEvent <- ev
                        (ev :> WaitHandle)
                    | ev -> 
                        (ev :> WaitHandle))

            member x.Close() =
                lock syncRoot (fun () ->
                    if not disposed then 
                        disposed <- true;
                        match resEvent with
                        | null -> ()
                        | ev -> 
                            ev.Close(); 
                            resEvent <- null)

            interface IDisposable with
                member x.Dispose() = x.Close() // ; System.GC.SuppressFinalize(x)


            member x.GrabResult() =
                match result with
                | Some res -> res
                | None -> Unexpected.Fail "Unexpected no result"


            // Record the result in the ResultCell.
            member x.RegisterResult (res:'T) =
                lock syncRoot (fun () ->
                    // In this case the ResultCell has already been disposed, e.g. due to a timeout.
                    // The result is dropped on the floor.
                    if disposed then 
                        ()
                    else
                        result <- Some res;
                        // If the resEvent exists then set it. If not we can skip setting it altogether and it won't be
                        // created
                        match resEvent with
                        | null -> 
                            ()
                        | ev ->
                            // If the resEvent exists then set it. If not we can skip setting it altogether and it won't be
                            // created.
                            ev.Set()  |> ignore)

            /// Wait until a result is available in the ResultCell. Should be called at most once.

            member x.ResultAvailable = result.IsSome

            member x.TryWaitForResult (?timeout) : 'T option =
                // Check if a result is available.
                match result with
                | Some res as r ->
                    r
                | None ->
                    // Force the creation of the WaitHandle
                    let resHandle = x.GetWaitHandle()
                    // Check again. While we were in GetWaitHandle, a call to RegisterResult may have set result then skipped the
                    // Set because the resHandle wasn't forced.
                    match result with
                    | Some res as r ->
                        r
                    | None ->
                        // OK, let's really wait for the Set signal. This may block.
                        let timeout = defaultArg timeout Threading.Timeout.Infinite 
#if FX_NO_EXIT_CONTEXT_FLAGS
                        let ok = resHandle.WaitOne(millisecondsTimeout= timeout) 
#else
                        let ok = resHandle.WaitOne(millisecondsTimeout= timeout,exitContext=true) 
#endif
                        if ok then
                            // Now the result really must be available
                            result
                        else
                            // timed out
                            None

    open AsyncImpl
    

    type TaskResult<'T>  =
    |   TaskOk of 'T
    |   TaskExn of exn
    |   TaskCancelled of string

    type AsyncGroup with
        /// Run the asynchronous workflow and wait for its result.
        member g.RunSynchronously (p,?timeout) =
            let innerAsyncGroup, disposeFct = g.CreateSubGroup()
            use resultCell = new ResultCell<Result<_>>()
            let blocked = System.Threading.Thread.CurrentThread
            startA
                blocked
                innerAsyncGroup
                (fun res ->
                    // Note the ResultCell may have been disposed if the operation
                    // timed out. In this case RegisterResult drops the result on the floor.
                    resultCell.RegisterResult(Ok(res));
                    FakeUnit)
                (fun exn -> resultCell.RegisterResult(Error(exn)); FakeUnit)
                (fun exn -> resultCell.RegisterResult(Error(exn :> Exception)); FakeUnit)
                p
            let res = resultCell.TryWaitForResult(?timeout = timeout) in
            match res with
            | None ->
                innerAsyncGroup.TriggerCancel();
                disposeFct()
                raise (System.TimeoutException())
            | Some res ->
                disposeFct()
                commit res

        member g.Start (computation) =
            queueWorkItem
                  g
                  (fun () -> FakeUnit)   // nothing to do on success
                  (fun _ -> FakeUnit)   // ignore exception in child
                  (fun _ -> FakeUnit)    // ignore cancellation in child
                  computation

        member g.RunWithContinuations(cont,econt,ccont,a:Async<'T>) : unit =
            let (P(f)) = a 
            let args = { cont = (cont >> fake); econt = (econt >> fake); ccont = (ccont >> fake); blocked = null; group = g }
            f args |> unfake
#if FX_ATLEAST_40            
        member g.CreateTask (computation,?taskCreationOptions) =
            let taskCreationOptions = defaultArg taskCreationOptions TaskCreationOptions.None
            let task = ref Unchecked.defaultof<Task<_>>
            let action () =
                use resultCell = new ResultCell<TaskResult<_>>()
                startA
                    []
                    g
                    (fun r -> resultCell.RegisterResult(TaskOk r) |> fake)
                    (fun exn -> resultCell.RegisterResult(TaskExn exn) |> fake)
                    (fun exn -> 
                        resultCell.RegisterResult(TaskCancelled exn.Message); 
                        (!task).Cancel() // wire group cancelation to task cancelation
                        FakeUnit)
                    computation
                let res = resultCell.TryWaitForResult()
                match res with 
                |   None -> 
                        // None case shouldn't happen - there is not timeout in TryWaitForResult
                        g.TriggerCancel()
                        raise (System.TimeoutException())
                |   Some r -> 
                        (match r with
                        |   TaskOk r -> r
                        |   TaskCancelled _ -> 
                                (!task).AcknowledgeCancellation()
                                Unchecked.defaultof<_>
                        |   TaskExn exn -> raise exn)
            task := new Task<_>(Func<_>(action), taskCreationOptions)
            
            // Wire task cancelation to group cancelation
            let onTaskCanceled canceledTask =
                if not(g.Test()) then                    
                    g.TriggerCancel()
            
            let cancelationTask = (!task).ContinueWith(Action<Task<_>>(onTaskCanceled), TaskContinuationOptions.OnlyOnCanceled)             
            !task
            
        member g.StartAsTask (computation,?taskCreationOptions) =
            let t = g.CreateTask(computation,?taskCreationOptions=taskCreationOptions)
            t.Start()
            t
#endif
            
#if DONT_INCLUDE_DEPRECATED
#else
        member g.Spawn computation =
            g.Start computation
        
        member g.Run (computation,?timeout) =
            g.RunSynchronously(computation,?timeout=timeout)
#endif

    [<Sealed>]
    type Async =

        static member CancelCheck () = doneA

        [<OverloadID("Primitive")>]
        static member Primitive (f : ('T -> unit) * (exn -> unit) -> unit): Async<'T> =
            primitiveA (fun (cont:cont<_>,econt:econt,_:ccont) ->
                f (cont >> unfake, econt >> unfake) |> fake)

        [<OverloadID("Primitive_with_ccont")>]
        static member Primitive (f : ('T -> unit) * (exn -> unit) * (OperationCanceledException -> unit) -> unit) : Async<'T> =
            primitiveA (fun (cont:cont<_>,econt:econt,ccont:ccont) ->
                f (cont >> unfake, econt >> unfake,ccont >> unfake) |> fake)

        static member CancelDefaultGroup(?message) =
            let asyncGroup = !defaultAsyncGroup
            asyncGroup.TriggerCancel(?message=message)
            defaultAsyncGroup := new AsyncGroup()
            
        static member DefaultGroup = !defaultAsyncGroup

        static member Catch (p: Async<'T>) =
            P (fun args ->
                invokeA p { group = args.group; 
                            cont = (Choice1Of2 >> args.cont); 
                            blocked=args.blocked; 
                            econt=(Choice2Of2 >> args.cont);
                            ccont=args.ccont})

        static member RunSynchronously (p: Async<'T>,?timeout) =
            (!defaultAsyncGroup).RunSynchronously(p, ?timeout=timeout)

        static member Start (computation) =
            (!defaultAsyncGroup).Start computation

#if FX_ATLEAST_40
        static member CreateTask (computation,?taskCreationOptions)=
            (!defaultAsyncGroup).CreateTask(computation,?taskCreationOptions=taskCreationOptions)
            
        static member StartAsTask (computation,?taskCreationOptions)=
            (!defaultAsyncGroup).StartAsTask(computation,?taskCreationOptions=taskCreationOptions)

#endif

#if DONT_INCLUDE_DEPRECATED
#else
        static member Run (p: Async<'T>,?timeout) = 
            Async.RunSynchronously(p,?timeout = timeout)

        static member Spawn (computation) = 
            Async.Start computation
#endif

#if FX_ATLEAST_40
    type AsyncGroup with
        member g.StartChildAsTask (computation,?taskCreationOptions) =
            async { return g.StartAsTask(computation, ?taskCreationOptions=taskCreationOptions) }

    type Async with
        static member StartChildAsTask (computation,?taskCreationOptions) =
            (!defaultAsyncGroup).StartChildAsTask(computation, ?taskCreationOptions=taskCreationOptions)

#endif 
    type Async with
        static member Parallel (l: seq<Async<'T>>) =
            protectedPrimitive (fun args ->
                let tasks = Seq.to_array l
                if tasks.Length = 0 then args.cont [| |] else
                let count = ref tasks.Length
                let firstExn = ref None
                let results = Array.zeroCreate tasks.Length
                // Attept to cancel the individual operations if an exception happens on any the other threads
                let innerAsyncGroup, disposeFct = args.group.CreateSubGroup()
                let finishTask(remaining) = 
                    if (remaining=0) then 
                        disposeFct(); 
                        match (!firstExn) with 
                        | None -> args.cont results
                        | Some (Choice1Of2 exn) -> args.econt exn
                        | Some (Choice2Of2 cexn) -> args.ccont cexn
                    else
                        FakeUnit

                let recordSuccess i res = 
                    results.[i] <- res;
                    let remaining = lock count (fun () -> decr count; !count)
                    finishTask(remaining) 

                let recordFailure exn = 
                    let remaining = 
                        lock count (fun () -> 
                            decr count; 
                            match !firstExn with 
                            | None -> firstExn := Some exn  // save the cancellation as the first exception
                            | _ -> ()
                            !count) 
                    if (remaining>0) then 
                        innerAsyncGroup.TriggerCancel() 
                    finishTask(remaining)
                
                tasks |> Array.iteri (fun i p ->
                    queueWorkItem
                        innerAsyncGroup
                        // on success, record the result
                        (fun res -> recordSuccess i res)
                        // on exception...
                        (fun exn -> recordFailure (Choice1Of2 exn))
                        // on cancellation...
                        (fun cexn -> recordFailure (Choice2Of2 cexn))
                        p);
                FakeUnit)

    type Async with

        static member Parallel2 (a:Async<'T1>,b:Async<'T2>) =
            async { let! res = Async.Parallel [boxed a; boxed b]
                    return (unbox<'T1>(res.[0]), unbox<'T2>(res.[1])) }

        static member Parallel3 (a:Async<'T1>,b:Async<'T2>,c:Async<'T3>) =
            async { let! res = Async.Parallel [boxed a; boxed b; boxed c]
                    return (unbox<'T1>(res.[0]), unbox<'T2>(res.[1]), unbox<'T3>(res.[2])) }

        static member RunWithContinuations(cont,econt,ccont,a:Async<'T>) : unit =
            (!defaultAsyncGroup).RunWithContinuations(cont,econt,ccont,a)

#if FX_NO_REGISTERED_WAIT_HANDLES
        static member WaitOne(waitHandle:WaitHandle,?millisecondsTimeout:int) =
            protectedPrimitive(fun args ->
                let millisecondsTimeout = defaultArg millisecondsTimeout Threading.Timeout.Infinite
                if millisecondsTimeout = 0 then 
                    let ok = waitHandle.WaitOne(0,exitContext=false)
                    args.cont ok
                else
                    // The .NET Compact Framework doesn't support RegisterWaitForSingleObject
                    // Hence we unblock the condition byusing a new thread pool work item.
                    // This is sub-optimal.
                    if ThreadPool.QueueUserWorkItem(fun _ -> 
                              let ok = waitHandle.WaitOne(millisecondsTimeout,exitContext=false)
                              args.cont ok |> ignore) then 
                        FakeUnit
                    else
                        // If we couldn't queue in the thread pool, try a new thread
                        (new Thread(ThreadStart(fun () -> 
                              let ok = waitHandle.WaitOne(millisecondsTimeout,exitContext=false)
                              args.cont ok |> ignore),IsBackground=true)).Start(); 
                        FakeUnit)

#else
        static member WaitOne(waitHandle:WaitHandle,?millisecondsTimeout:int) =
            protectedPrimitive(fun args ->
                let millisecondsTimeout = defaultArg millisecondsTimeout Threading.Timeout.Infinite
                if millisecondsTimeout = 0 then 
#if FX_NO_EXIT_CONTEXT_FLAGS
                    let ok = waitHandle.WaitOne(0)
#else
                    let ok = waitHandle.WaitOne(0,exitContext=false)
#endif
                    args.cont ok
                else

                    let rwh = ref (None : RegisteredWaitHandle option)

                    let cancelHandler =
                        Handler(fun obj (msg:string) ->
                            match !rwh with
                            | None -> ()
                            | Some rwh ->
                                // If we successfully Unregister the operation then
                                // the callback below will never get called. So we call the cancel
                                // continuation directly in a new work item.
                                let succeeded = rwh.Unregister(waitHandle)
                                if succeeded then
                                    Async.Start (async { do (args.ccont (new OperationCanceledException(msg)) |> unfake) }))

                    rwh := Some(ThreadPool.RegisterWaitForSingleObject
                                  (waitObject=waitHandle,
                                   callBack=WaitOrTimerCallback(fun _ timeOut ->
                                                rwh := None;
                                                args.group.Cancel.RemoveHandler(cancelHandler);
                                                args.cont (not timeOut) |> ignore),
                                   state=null,
                                   millisecondsTimeOutInterval=millisecondsTimeout,
                                   executeOnlyOnce=true));

                    args.group.Cancel.AddHandler(cancelHandler);
                    FakeUnit)
#endif

        [<OverloadID("BuildPrimitve_zero_arg")>]
        static member WaitIAsyncResult(iar: IAsyncResult,endFunc): Async<'T> =
            async { if iar.CompletedSynchronously then 
                        return endFunc iar
                    else
                        let! ok = Async.WaitOne(iar.AsyncWaitHandle) 
                        return endFunc iar }

        [<OverloadID("BuildPrimitve_zero_arg")>]
        static member BuildPrimitive(beginFunc,endFunc): Async<'T> =
            async { let iar : IAsyncResult = beginFunc((null : System.AsyncCallback),(null:obj))
                    return! Async.WaitIAsyncResult(iar,endFunc) }

        [<OverloadID("BuildPrimitve_one_arg")>]
        static member BuildPrimitive(arg1,beginFunc,endFunc): Async<'T> =
            async { let iar : IAsyncResult = beginFunc(arg1,(null : System.AsyncCallback),(null:obj))
                    return! Async.WaitIAsyncResult(iar,endFunc) }

        [<OverloadID("BuildPrimitve_two_arg")>]
        static member BuildPrimitive(arg1,arg2,beginFunc,endFunc): Async<'T> =
            async { let iar : IAsyncResult = beginFunc(arg1,arg2,(null : System.AsyncCallback),(null:obj))
                    return! Async.WaitIAsyncResult(iar,endFunc) }

        [<OverloadID("BuildPrimitve_three_arg")>]
        static member BuildPrimitive(arg1,arg2,arg3,beginFunc,endFunc): Async<'T> =
            async { let iar : IAsyncResult = beginFunc(arg1,arg2,arg3,(null : System.AsyncCallback),(null:obj))
                    return! Async.WaitIAsyncResult(iar,endFunc) }

    type Async with
#if DONT_INCLUDE_DEPRECATED
#else
        static member Generate (n,f, ?numChunks) : Async<'T array> =
            async { let procs = defaultArg numChunks System.Environment.ProcessorCount
                    let resArray = Array.zeroCreate n
                    let! res = Async.Parallel
                                  [ for pid in 0 .. procs-1 ->
                                        async { for i in 0 .. (n/procs+(if n%procs > pid then 1 else 0)) - 1 do
                                                    let elem = (i + pid * (n/procs) + min (n%procs) pid)
                                                    let! res = f elem
                                                    do resArray.[elem] <- res;  } ]
                    return resArray }
#endif

        static member Ignore (p: Async<'T>) = bindA p (fun _ -> doneA)
        static member SwitchToNewThread() =
            protectedPrimitive(fun args ->
                  (new Thread(ThreadStart(fun () -> args.cont () |> ignore),IsBackground=true)).Start(); FakeUnit)

        static member SwitchToThreadPool() =
            protectedPrimitive(fun args ->
                    //if not (ExecutionContext.IsFlowSuppressed()) then ExecutionContext.SuppressFlow() |> ignore
                    if not (ThreadPool.QueueUserWorkItem(fun _ -> args.cont () |> ignore)) then
                        args.econt (Failure "SwitchToThreadPool: failed to queue user work item")
                    else
                        FakeUnit)

    type Async with

        static member StartChild (p:Async<'T>,?millisecondsTimeout) =
            async { let resultCell = new ResultCell<_>()
                    let! asyncGroup = getAsyncGroup()
                    do queueWorkItem asyncGroup
                                      (fun res -> resultCell.RegisterResult (Ok res); FakeUnit)   
                                      (fun err -> resultCell.RegisterResult (Error err); FakeUnit)   
                                      (fun err -> resultCell.RegisterResult (Error err); FakeUnit)    
                                      p
                                               
                    return async { try 
                                     if resultCell.ResultAvailable then 
                                       return commit (resultCell.GrabResult())
                                     else
                                       let! ok = Async.WaitOne(resultCell.GetWaitHandle(),?millisecondsTimeout=millisecondsTimeout) 
                                       return commitWithPossibleTimeout (if ok then Some(resultCell.GrabResult()) else None) 
                                   finally 
                                     resultCell.Close() } }

#if DONT_INCLUDE_DEPRECATED
#else
        static member SpawnChild p = 
            async { let! asyncGroup = getAsyncGroup()
                    do queueWorkItem asyncGroup
                                      (fun () -> FakeUnit)   // nothing to do on success
                                      (fun _ -> FakeUnit)   // ignore exception in child
                                      (fun _ -> FakeUnit)    // ignore cancellation in child
                                      p }
#endif                                      

#if FX_NO_SYNC_CONTEXT
#else
        static member SpawnThenPostBack (computation: Async<'T>,postBack) =
            let syncContext = SynchronizationContext.Current
            match syncContext with
            | null -> raise <| new System.InvalidOperationException("The System.Threading.SynchronizationContext.Current of the calling thread is null")
            | _ ->
                Async.Start (async { let! res = catchA(computation) in
                                        do post syncContext (fun () -> postBack (commit res)) () })

        static member SwitchToGuiThread syncContext =
            async { match syncContext with 
                    | null -> 
                        // no synchronization context, just switch to the thread pool
                        do! Async.SwitchToThreadPool()
                    | ctxt -> 
                        // post the continuation to the synchronization context
                        do! Async.Primitive (fun (cont,econt) -> post ctxt (fun () -> cont()) ()) }
#endif

        static member OnCancel action =
            async { let h = new Handler<_>(fun sender x -> try action x with _ -> ())
                    let! asyncGroup = getAsyncGroup ()
                    do asyncGroup.Cancel.AddHandler(h);
                    return { new IDisposable with
                                 member x.Dispose() = asyncGroup.Cancel.RemoveHandler(h) } }

        static member TryCancelled (p: Async<'T>,f) = 
            whenCancelledA f p

#if FX_ATLEAST_40            
        static member WaitTask (task:Task<'T>) : Async<'T> =
            protectedPrimitive(fun args ->
                let continuation (completedTask : Task<_>) =
                    if completedTask.IsCanceled then
                        args.group.TriggerCancel()
                        args.ccont (new OperationCanceledException())
                    elif completedTask.IsFaulted then
                        args.econt completedTask.Exception
                    else
                        args.cont completedTask.Result
            
                task.ContinueWith(Action<Task<'T>>(continuation >> unfake), TaskContinuationOptions.None) |> ignore |> fake
            )
#endif

    module CommonExtensions =
        open AsyncBuilderImpl

        type System.IO.Stream with

            member stream.AsyncRead(buffer: byte[],?offset,?count) =
                let offset = defaultArg offset 0
                let count  = defaultArg count buffer.Length
                Async.BuildPrimitive (buffer,offset,count,stream.BeginRead,stream.EndRead)

            member stream.AsyncRead(count) =
                async { let buffer = Array.zeroCreate count
                        let i = ref 0
                        while !i < count do
                            let! n = stream.AsyncRead(buffer,!i,count - !i)
                            i := !i + n
                            if n = 0 then 
                                raise(System.IO.EndOfStreamException("failed to read enough bytes from stream"))
                        return buffer }
            
            member stream.AsyncWrite(buffer:byte[], ?offset:int, ?count:int) =
                let offset = defaultArg offset 0
                let count  = defaultArg count buffer.Length
                Async.BuildPrimitive (buffer,offset,count,stream.BeginWrite,stream.EndWrite)
                
        type System.Threading.WaitHandle with
            member waitHandle.AsyncWaitOne(?millisecondsTimeout:int) =
                Async.WaitOne(waitHandle,?millisecondsTimeout=millisecondsTimeout) 

        type System.Threading.Thread with

            static member AsyncSleep(dueTime) =
               Async.Primitive (fun (cont,econt) ->
                   let timer = ref None
                   timer := Some (new Timer((fun _ ->
                                                 // Try to Dispose of the TImer.
                                                 // Note: there is a race here: the System.Threading.Timer time very occasionally
                                                 // calls the callback _before_ the timer object has been recorded anywhere. This makes it difficult to dispose the
                                                 // timer in this situation. In this case we just let the timer be collected by finalization.
                                                (match !timer with
                                                 | None -> ()
                                                 | Some t -> t.Dispose());
                                                // Now we're done, so call the continuation
                                                cont() |> ignore),
                                            null, dueTime=dueTime, period = -1)))

#if FX_ATLEAST_40                                                    
        type System.Threading.Tasks.Task<'T> with
            member this.AsyncValue 
                with get () =
                    Async.WaitTask(this)
#endif

    open CommonExtensions

#if DONT_INCLUDE_DEPRECATED
#else
    [<Sealed>]
    [<AutoSerializable(false)>]        
    [<Obsolete("This type will be removed in the next version of F# and should no longer be used. Consider using Async.RunWithContinuations")>]
    type AsyncFuture<'T>(resultCell:ResultCell<Result<'T>>) = 
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
        member x.Value = commitWithPossibleTimeout (resultCell.TryWaitForResult())

        interface IAsyncResult with
            member iar.IsCompleted= resultCell.ResultAvailable
            member iar.AsyncState=null
            member iar.CompletedSynchronously=false
            member iar.AsyncWaitHandle = resultCell.GetWaitHandle() 
        interface IDisposable with
            member x.Dispose() = resultCell.Close()


    type AsyncGroup with
        [<Obsolete("This method will be removed in the next version of F# and may no longer be used. Consider using Async.RunWithContinuations")>]
        member g.SpawnFuture p  = 
            let resultCell = new ResultCell<_>()

            // OK, queueWorkItem the async computation and when it's done register completion
            queueWorkItem
                g
                // on success, record the result
                (fun res -> resultCell.RegisterResult (Ok res); FakeUnit)
                // on exception...
                (fun exn -> resultCell.RegisterResult (Error(exn)); FakeUnit)
                // on cancellation...
                (fun exn -> resultCell.RegisterResult (Error(exn :> Exception)); FakeUnit)
                p
            new AsyncFuture<_>(resultCell)

    type Async with
            
        [<Obsolete("This method will be removed in the next version of F# and may no longer be used. Consider using Async.RunWithContinuations")>]
        static member SpawnFuture (p: Async<'T>) = 
            (!defaultAsyncGroup).SpawnFuture p

#endif
    
    [<Sealed>]
    [<AutoSerializable(false)>]        
    type Mailbox<'Msg>() =  
        let mutable inbox_store  = null 
        let mutable arrivals = new Queue<'Msg>()
        let syncRoot = arrivals

        // Control elements indicating the state of the reader. When the reader is "blocked" at an 
        // asynchronous receive, either 
        //     -- "cont" is non-null and the reader is "activated" by re-scheduling cont in the thread pool; or
        //     -- "pulse" is non-null and the reader is "activated" by setting this event
        let mutable cont : (bool -> FakeUnitValue) = Unchecked.defaultof<_>  
        let mutable pulse : AutoResetEvent = null
        let ensurePulse() = 
            match pulse with 
            | null -> 
                pulse <- new AutoResetEvent(false);
            | ev -> 
                ()
            pulse
                
        let waitOneNoTimeout = 
            P (fun args -> 
                match box cont with 
                | null -> 
                    let descheduled = 
                        // An arrival may have happened while we're preparing to deschedule
                        lock syncRoot (fun () -> 
                            if arrivals.Count = 0 then 
                                // OK, no arrival so deschedule
                                cont <- args.cont;
                                true
                            else
                                false)
                    if descheduled then 
                        FakeUnit 
                    else 
                        // If we didn't deschedule then run the continuation immediately
                        args.cont true
                | _ -> 
                    failwith "multiple waiting reader continuations for mailbox")

        let waitOne(timeout) = 
            if timeout < 0  then 
                waitOneNoTimeout
            else 
                ensurePulse().AsyncWaitOne(millisecondsTimeout=timeout)

        member x.inbox = 
            match inbox_store with 
            | null -> inbox_store <- new System.Collections.Generic.List<'Msg>(1) // ResizeArray
            | _ -> () 
            inbox_store

        member x.scanArrivalsUnsafe(f) =
            if arrivals.Count = 0 then None
            else let msg = arrivals.Dequeue()
                 match f msg with
                 | None -> 
                     x.inbox.Add(msg); 
                     x.scanArrivalsUnsafe(f)
                 | res -> res
        // Lock the arrivals queue while we scan that
        member x.scanArrivals(f) = lock syncRoot (fun () -> x.scanArrivalsUnsafe(f))

        member x.scanInbox(f,n) =
            match inbox_store with
            | null -> None
            | inbox ->
                if n >= inbox.Count
                then None
                else
                    let msg = inbox.[n]
                    match f msg with
                    | None -> x.scanInbox (f,n+1)
                    | res -> inbox.RemoveAt(n); res

        member x.receiveFromArrivalsUnsafe() =
            if arrivals.Count = 0 then None
            else Some(arrivals.Dequeue())

        member x.receiveFromArrivals() = 
            lock syncRoot (fun () -> x.receiveFromArrivalsUnsafe())

        member x.receiveFromInbox() =
            match inbox_store with
            | null -> None
            | inbox ->
                if inbox.Count = 0
                then None
                else
                    let x = inbox.[0]
                    inbox.RemoveAt(0);
                    Some(x)

        member x.Post(msg) =
            lock syncRoot (fun () ->
                arrivals.Enqueue(msg);
                // This is called when we enqueue a message, within a lock
                // We cooperatively unblock any waiting reader. If there is no waiting
                // reader we just leave the message in the incoming queue
                match box cont with
                | null -> 
                    match pulse with 
                    | null -> 
                        () // no one waiting, leaving the message in the queue is sufficient
                    | ev -> 
                        // someone is waiting on the wait handle
                        ev.Set() |> ignore
                | _ -> 
                    let c = cont
                    cont <- Unchecked.defaultof<_>
                    //if not (ExecutionContext.IsFlowSuppressed()) then ExecutionContext.SuppressFlow() |> ignore
                    ThreadPool.QueueUserWorkItem(fun _ ->  c true |> unfake) |> ignore)

        member x.TryScan ((f: 'Msg -> (Async<'T>) option), timeout) : Async<'T option> =
            let rec scan() =
                async { match x.scanArrivals(f) with
                        | None -> // Deschedule and wait for a message. When it comes, rescan the arrivals
                                  let! ok = waitOne(timeout)
                                  if ok then return! scan() else return None
                        | Some resP -> let! res = resP
                                       return Some(res) }
            // Look in the inbox first
            async { match x.scanInbox(f,0) with
                    | None  -> return! scan()
                    | Some resP -> let! res = resP
                                   return Some(res) }

        member x.Scan((f: 'Msg -> (Async<'T>) option), timeout) =
            async { let! resOpt = x.TryScan(f,timeout)
                    match resOpt with
                    | None -> return raise(TimeoutException("Mailbox.Scan timed out"))
                    | Some res -> return res }


        member x.TryReceive(timeout) =
            let rec processFirstArrival() =
                async { match x.receiveFromArrivals() with
                        | None -> 
                            // Wait until we have been notified about a message. When that happens, rescan the arrivals
                            let! ok = waitOne(timeout)
                            if ok then return! processFirstArrival()
                            else return None
                        | res -> return res }
            // look in the inbox first
            async { match x.receiveFromInbox() with
                    | None -> return! processFirstArrival()
                    | res -> return res }

        member x.Receive(timeout) =

            let rec processFirstArrival() =
                async { match x.receiveFromArrivals() with
                        | None -> 
                            // Wait until we have been notified about a message. When that happens, rescan the arrivals
                            let! ok = waitOne(timeout)
                            if ok then return! processFirstArrival()
                            else return raise(TimeoutException("Mailbox.Receive timed out"))
                        | Some res -> return res }
            // look in the inbox first
            async { match x.receiveFromInbox() with
                    | None -> return! processFirstArrival() 
                    | Some res -> return res }

        interface System.IDisposable with
            member x.Dispose() =
                if pulse <> null then (pulse :> IDisposable).Dispose()

#if DEBUG
        member x.UnsafeContents =
            (x.inbox,arrivals,pulse,cont) |> box
#endif


    [<Sealed>]
    type AsyncReplyChannel<'Reply>(replyf : 'Reply -> unit) =
        member x.Reply(reply) = replyf(reply)

    [<Sealed>]
    [<AutoSerializable(false)>]
    type MailboxProcessor<'Msg>(initialState,?asyncGroup) =
        let asyncGroup = defaultArg asyncGroup !defaultAsyncGroup
        let mailbox = new Mailbox<'Msg>()
        let mutable defaultTimeout = Threading.Timeout.Infinite
        let mutable started = false

        member x.DefaultTimeout with get() = defaultTimeout and set(v) = defaultTimeout <- v
#if DEBUG
        member x.UnsafeMessageQueueContents = mailbox.UnsafeContents
#endif
        member x.Start() =
            if started then
                raise (new InvalidOperationException("the mailbox has already been started"))
            else
                started <- true
                asyncGroup.Start(computation=initialState(x))
        member x.Post(msg) = mailbox.Post(msg)

        member x.TryPostAndReply(msgf : (_ -> 'Msg), ?timeout) : 'Reply option = 
            let timeout = defaultArg timeout defaultTimeout
            use resultCell = new ResultCell<_>()
            let msg = msgf (AsyncReplyChannel<_>(fun reply ->
                                    // Note the ResultCell may have been disposed if the operation
                                    // timed out. In this case RegisterResult drops the result on the floor.
                                    resultCell.RegisterResult(reply)))
            mailbox.Post(msg)
            resultCell.TryWaitForResult(timeout=timeout) 

        member x.PostAndReply(msgf, ?timeout) : 'Reply = 
            match x.TryPostAndReply(msgf,?timeout=timeout) with
            | None ->  raise (TimeoutException("PostAndReply timed out"))
            | Some res -> res

        member x.AsyncTryPostAndReply(msgf, ?timeout) : Async<'Reply option> = 
            let timeout = defaultArg timeout defaultTimeout
            let resultCell = new ResultCell<_>()
            let msg = msgf (AsyncReplyChannel<_>(fun reply ->
                                    // Note the ResultCell may have been disposed if the operation
                                    // timed out. In this case RegisterResult drops the result on the floor.
                                    resultCell.RegisterResult(reply)))
            mailbox.Post(msg)
            async { use disposeCell = resultCell
                    let! ok =  resultCell.GetWaitHandle().AsyncWaitOne(millisecondsTimeout=timeout)
                    let res = (if ok then Some(resultCell.GrabResult()) else None)
                    return res }
                    
        member x.AsyncPostAndReply(msgf, ?timeout) =
            let timeout = defaultArg timeout defaultTimeout
            let asyncReply = x.AsyncTryPostAndReply(msgf,timeout=timeout) 
            async { let! res = asyncReply
                    match res with 
                    | None ->  return! raise (TimeoutException("AsyncPostAndReply timed out"))
                    | Some res -> return res }
                    
        member x.Receive(?timeout)    = mailbox.Receive(timeout=defaultArg timeout defaultTimeout)
        member x.TryReceive(?timeout) = mailbox.TryReceive(timeout=defaultArg timeout defaultTimeout)
        member x.Scan(f: 'Msg -> (Async<'T>) option,?timeout)     = mailbox.Scan(f,timeout=defaultArg timeout defaultTimeout)
        member x.TryScan(f: 'Msg -> (Async<'T>) option,?timeout)  = mailbox.TryScan(f,timeout=defaultArg timeout defaultTimeout)

        interface System.IDisposable with
            member x.Dispose() = (mailbox :> IDisposable).Dispose()

        static member Start(initialState,?asyncGroup) = 
            let mb = new MailboxProcessor<'Msg>(initialState,?asyncGroup=asyncGroup)
            mb.Start();
            mb

