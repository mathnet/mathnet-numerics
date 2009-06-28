// (c) Microsoft Corporation 2005-2009. 
namespace Microsoft.FSharp.Control

    open System
    open Microsoft.FSharp.Control

    /// Represents the reified result of an asynchronous computation
    type AsyncResult<'T>  =
        |   AsyncOk of 'T
        |   AsyncException of exn
        |   AsyncCanceled of OperationCanceledException

        static member Commit(res:AsyncResult<'T>) = 
            Async.Primitive (fun (cont,econt,ccont) -> 
                   match res with 
                   | AsyncOk v -> cont v 
                   | AsyncException exn -> econt exn 
                   | AsyncCanceled exn -> ccont exn)

    /// When using .NET 4.0 you can replace this type by Task<'T>
    [<Sealed>]
    type AsyncResultCell<'T>() =
        let mutable result = None
        // The continuation for the result, if any
        let mutable savedCont = Unchecked.defaultof<_>

        // Record the result in the AsyncResultCell.
        // Ignore subsequent sets of the result. This can happen, e.g. for a race between 
        // a cancellation and a success.
        member x.RegisterResult (res:AsyncResult<'T>) =
            let grabbedCont = 
                lock x (fun () ->
                    if result.IsSome then  
                        Unchecked.defaultof<_>
                    else
                        result <- Some res;
                        savedCont)
            // Run the action outside the lock
            match box grabbedCont with 
            | null -> ()
            | _ -> savedCont res 

        /// Get the reified result 
        member private x.AsyncPrimitiveResult =
            Async.Primitive(fun (cont,_,_) -> 
                let grabbedResult = 
                    lock x (fun () ->
                        match result with
                        | Some res -> 
                            result
                        | None ->
                            // Otherwise save the continuation and call it in RegisterResult
                            savedCont <- cont
                            None)
                // Run the action outside the lock
                match grabbedResult with 
                | None -> ()
                | Some res -> cont res) 
                           

        /// Get the result and commit it
        member x.AsyncResult =
            async { let! res = x.AsyncPrimitiveResult
                    return! AsyncResult.Commit(res) }


    [<AutoOpen>]
    module AsyncExtensions =


#if FX_NO_CREATE_DELEGATE
#else
        type private Closure<'T>(f) =
          member x.Invoke(sender:obj,a:'T) : unit = f(a)

        type Async<'T> with 

            /// Return an asynchronous computation that waits for a single invocation of a .NET event by
            /// adding a handler to the event. Once the computation completes or is cancelled, the 
            /// handler is removed from the event.
            ///
            /// The computation will respond to cancellation while waiting for the completion
            /// of the operation, in which case later invocations of the event are ignored.
            static member AwaitEvent(event:IEvent<'Delegate,'T>, ?cancelAction : (unit -> unit)) : Async<'T> =
                async { let resultCell = new AsyncResultCell<'T>()

                        let cancelRemovalCell = ref (None : IDisposable option)

                        // Set up the handlers to listen to events and cancellation
                        let rec onCancel(msg) = 
                            // We've been cancelled. Call the given cancellation routine
                            match cancelAction with 
                            | None -> 
                                // We've been cancelled without a cancel action. Stop listening to events
                                event.RemoveHandler(del)
                                // Register the result. This may race with a sucessful result, but
                                // AsyncResultCell allows a race and throws away whichever comes last.
                                resultCell.RegisterResult(AsyncCanceled (OperationCanceledException msg))
                            | Some cancel -> 
                                // If we get an exception from a cooperative cancellation function
                                // we assume the operation has already completed.
                                try cancel() with _ -> () 
                        
                        and obj = 
                            new Closure<'T>(fun eventArgs ->
                                // Stop listening to events
                                event.RemoveHandler(del)
                                // The callback has been activated, so ensure cancellation is not possible beyond this point
                                match !cancelRemovalCell with 
                                | None -> ()  
                                | Some d -> d.Dispose() 
                                // Register the result. This may race with a cancellation result, but
                                // AsyncResultCell allows a race and throws away whichever comes last.
                                resultCell.RegisterResult(AsyncOk(eventArgs)))
                        and del = 
                            System.Delegate.CreateDelegate(typeof<'Delegate>, obj, "Invoke") :?> 'Delegate

                        // Activate cancellation
                        let! cancelRemoval = Async.OnCancel(onCancel)
                        cancelRemovalCell := Some cancelRemoval

                        
                        // Start listening to events
                        event.AddHandler(del)

                        // Return the async computation that allows us to await the result
                        return! resultCell.AsyncResult }

#endif

        type Async<'T> with 

            static member FromBeginEndViaCallback(beginAction,endAction,?cancelAction): Async<'T> =

                async { let resultCell = new AsyncResultCell<_>()

                        // Activate cancellation
                        let! cancelRemoval = 
                            Async.OnCancel(fun msg -> 
                                // Call the cancellation routine
                                match cancelAction with 
                                | None -> 
                                    // Register the result. This may race with a sucessful result, but
                                    // AsyncResultCell allows a race and throws away whichever comes last.
                                    resultCell.RegisterResult(AsyncCanceled (OperationCanceledException(msg)))
                                | Some cancel -> 
                                    // If we get an exception from a cooperative cancellation function
                                    // we assume the operation has already completed.
                                    try cancel() with _ -> () )

                        let callback = 
                            new System.AsyncCallback(fun iar -> 
                                // The callback has been activated, so ensure cancellation is not possible
                                // beyond this point
                                cancelRemoval.Dispose()
                                // Run the endAction and collect its result.
                                let res = 
                                    try AsyncOk(endAction iar) 
                                    with e -> AsyncException(e)
                                // Register the result. This may race with a cancellation result, but
                                // AsyncResultCell allows a race and throws away whichever comes last.
                                resultCell.RegisterResult(res))

                        // Get the async computation that allows us to await the result
                        let (_:IAsyncResult) = beginAction (callback,(null:obj)) 
                        return! resultCell.AsyncResult }

            static member FromBeginEndViaCallback(arg1,beginAction,endAction,?cancelAction): Async<'T> =
                Async.FromBeginEndViaCallback((fun (iar,state) -> beginAction(arg1,iar,state)), endAction, ?cancelAction=cancelAction)


            static member FromBeginEndViaCallback(arg1,arg2,beginAction,endAction,?cancelAction): Async<'T> =
                Async.FromBeginEndViaCallback((fun (iar,state) -> beginAction(arg1,arg2,iar,state)), endAction, ?cancelAction=cancelAction)

            static member FromBeginEndViaCallback(arg1,arg2,arg3,beginAction,endAction,?cancelAction): Async<'T> =
                Async.FromBeginEndViaCallback((fun (iar,state) -> beginAction(arg1,arg2,arg3,iar,state)), endAction, ?cancelAction=cancelAction)

        type AsyncIAsyncResult<'T>(ag:AsyncGroup, callback: System.AsyncCallback,state:obj) =
             let mutable completedSynchronously = true
             let mutable result = None
             let ev = new System.Threading.ManualResetEvent(false)
             member s.SetResult(v: AsyncResult<'T>) =  
                 result <- Some v; 
                 ev.Set() |> ignore; 
                 match callback with
                 | null -> ()
                 | d -> 
                     // The IASyncResult becomes observable here
                     d.Invoke (s :> System.IAsyncResult)
             member s.GetResult() = 
                 ev.WaitOne() |> ignore; 
                 match result.Value with 
                 | AsyncOk v -> v
                 | AsyncException err -> raise err
                 | AsyncCanceled err -> raise err
             member x.Close() = ev.Close()
             member x.CancelAsync() = ag.TriggerCancel()
             member x.CheckForNotSynchronous() = 
                 if result.IsNone then 
                     completedSynchronously <- false

             interface System.IAsyncResult with
                   member x.IsCompleted = result.IsSome
                   member x.CompletedSynchronously = completedSynchronously
                   member x.AsyncWaitHandle = ev :> System.Threading.WaitHandle
                   member x.AsyncState = state
                      

        type Async<'T> with 

            static member AsBeginEnd(computation:Async<'T>) : 
                                        (// The 'Begin' member
                                         (System.AsyncCallback * obj -> System.IAsyncResult) * 
                                         // The 'End' member
                                         (System.IAsyncResult -> 'T) * 
                                         // The 'Cancel' member
                                         (System.IAsyncResult -> unit)) =
  
               let beginAction(callback,state) = 
                   let ag = new AsyncGroup()
                   let aiar = new AsyncIAsyncResult<'T>(ag,callback,state)
                   let cont v = aiar.SetResult (AsyncOk v)
                   let econt v = aiar.SetResult (AsyncException v)
                   let ccont v = aiar.SetResult (AsyncCanceled v)
                   ag.RunWithContinuations(cont,econt,ccont,computation)
                   aiar.CheckForNotSynchronous()
                   (aiar :> IAsyncResult)
               let endAction(iar:IAsyncResult) =
                   match iar with 
                   | :? AsyncIAsyncResult<'T> as aiar ->
                       let res = aiar.GetResult()
                       aiar.Close ()
                       res
                   | _ -> 
                       invalidArg "iar" "mismatched IAsyncResult passed to an 'End' operation"
               let cancelAction(iar:IAsyncResult) =
                   match iar with 
                   | :? AsyncIAsyncResult<'T> as aiar ->
                       aiar.CancelAsync()
                   | _ -> 
                       invalidArg "iar" "mismatched IAsyncResult passed to a 'Cancel' operation"
               beginAction, endAction, cancelAction

    [<AutoOpen>]
    module FileExtensions =

        let UnblockViaNewThread f =
            async { do! Async.SwitchToNewThread ()
                    let res = f()
                    do! Async.SwitchToThreadPool ()
                    return res }


        type System.IO.File with
            static member AsyncOpenText(path)   = UnblockViaNewThread (fun () -> System.IO.File.OpenText(path))
            static member AsyncAppendText(path) = UnblockViaNewThread (fun () -> System.IO.File.AppendText(path))
            static member AsyncOpenRead(path)   = UnblockViaNewThread (fun () -> System.IO.File.OpenRead(path))
            static member AsyncOpenWrite(path)  = UnblockViaNewThread (fun () -> System.IO.File.OpenWrite(path))
#if FX_NO_FILE_OPTIONS
            static member AsyncOpen(path,mode,?access,?share,?bufferSize) =
#else
            static member AsyncOpen(path,mode,?access,?share,?bufferSize,?options) =
#endif
                let access = match access with Some v -> v | None -> System.IO.FileAccess.ReadWrite
                let share = match share with Some v -> v | None -> System.IO.FileShare.None
#if FX_NO_FILE_OPTIONS
#else
                let options = match options with Some v -> v | None -> System.IO.FileOptions.None
#endif
                let bufferSize = match bufferSize with Some v -> v | None -> 0x1000
                UnblockViaNewThread (fun () -> 
#if FX_NO_FILE_OPTIONS
                    new System.IO.FileStream(path,mode,access,share,bufferSize))
#else
                    new System.IO.FileStream(path,mode,access,share,bufferSize, options))
#endif

            static member OpenTextAsync(path)   = System.IO.File.AsyncOpenText(path)
            static member AppendTextAsync(path) = System.IO.File.AsyncAppendText(path)
            static member OpenReadAsync(path)   = System.IO.File.AsyncOpenRead(path)
            static member OpenWriteAsync(path)  = System.IO.File.AsyncOpenWrite(path)
#if FX_NO_FILE_OPTIONS
            static member OpenAsync(path,mode,?access,?share,?bufferSize) = 
                System.IO.File.AsyncOpen(path, mode, ?access=access, ?share=share,?bufferSize=bufferSize)
#else
            static member OpenAsync(path,mode,?access,?share,?bufferSize,?options) = 
                System.IO.File.AsyncOpen(path, mode, ?access=access, ?share=share,?bufferSize=bufferSize,?options=options)
#endif

    [<AutoOpen>]
    module StreamReaderExtensions =
        type System.IO.StreamReader with

            member s.AsyncReadToEnd () = FileExtensions.UnblockViaNewThread (fun () -> s.ReadToEnd())
            member s.ReadToEndAsync () = s.AsyncReadToEnd ()

#if FX_NO_WEB_REQUESTS
#else
    [<AutoOpen>]
    module WebRequestExtensions =
        open System
        open System.Net
        
        type System.Net.WebRequest with
            member req.AsyncGetResponse() : Async<WebResponse>= 
                
                async { try 
                            // Note that we specify req.Abort as the cancelAction. If successful, this will cause 
                            // a WebExceptionStatus.RequestCanceled to be raised from the web request.
                           return! Async.FromBeginEndViaCallback(beginAction=req.BeginGetResponse, 
                                                                 endAction = req.EndGetResponse, 
                                                                 cancelAction = req.Abort)
                        with 
                           | :? WebException as webExn 
                                   when webExn.Status = WebExceptionStatus.RequestCanceled -> 

                               return! AsyncResult.Commit(AsyncCanceled (OperationCanceledException webExn.Message)) }

            member req.GetResponseAsync() = req.AsyncGetResponse()
#endif
     
#if FX_NO_WEB_CLIENT
#else
    [<AutoOpen>]
    module WebClientExtensions =
        open System
        open System.Net
        open Microsoft.FSharp.Control
        
        type WebClient with
            member this.AsyncDownloadString (address:Uri) : Async<string> =
                async { let userToken = new obj()
                        let cancelAction() = this.CancelAsync()
                        
                        this.DownloadStringAsync(address, userToken)
                        /// Loop until we see a reply with the same token
                        let rec loop() = 
                            async { let! args = Async.AwaitEvent(this.DownloadStringCompleted,cancelAction=this.CancelAsync)
                                    if args.UserState <> userToken then 
                                        // This was not our request. Keep waiting
                                        return! loop()
                                    else
                                        let result = 
                                            if args.Cancelled then 
#if SILVERLIGHT
                                                AsyncCanceled (new OperationCanceledException("The operation was cancelled"))
#else
                                                AsyncCanceled (new OperationCanceledException())
#endif
                                            elif args.Error <> null then  AsyncException args.Error
                                            else  AsyncOk args.Result 
                                        return! AsyncResult.Commit(result) }
                        return! loop() }
#endif


//        type Microsoft.FSharp.Control.Async<'T> with 
//            /// Like Async.BuildPrimitive, but does not wait on a WaitHandle.
//            static member FromBeginEndViaCallback: beginAction:(System.AsyncCallback * obj -> System.IAsyncResult) * endAction:(System.IAsyncResult -> 'T) * ?cancelAction : (unit -> unit) -> Async<'T>
//            static member FromBeginEndViaCallback: arg1:'Arg1 * beginAction:('Arg1 * System.AsyncCallback * obj -> System.IAsyncResult) * endAction:(System.IAsyncResult -> 'T) * ?cancelAction : (unit -> unit) -> Async<'T>
//            static member FromBeginEndViaCallback: arg1:'Arg1 * arg2: 'Arg2 * beginAction:('Arg1 * 'Arg2 * System.AsyncCallback * obj -> System.IAsyncResult) * endAction:(System.IAsyncResult -> 'T) * ?cancelAction : (unit -> unit) -> Async<'T>
//            static member FromBeginEndViaCallback: arg1:'Arg1 * arg2: 'Arg2 * arg3:'Arg3 * beginAction:('Arg1 * 'Arg2 * 'Arg3 * System.AsyncCallback * obj -> System.IAsyncResult) * endAction:(System.IAsyncResult -> 'T) * ?cancelAction : (unit -> unit) -> Async<'T>
//            /// As for AwaitEvent, but the sender of the event is also returned.
//            static member AwaitEvent: IEvent<'Delegate,'T> * ?cancelAction : (unit -> unit) -> Async<obj * 'T> when 'Delegate : delegate<'T,unit> and 'Delegate :> System.Delegate 
