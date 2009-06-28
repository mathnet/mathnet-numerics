// (c) Microsoft Corporation 2005-2009.
namespace Microsoft.FSharp.Control

    open System
    open Microsoft.FSharp.Control
    
    /// Represents the reified result of an asynchronous computation
    type AsyncResult<'T>  =
        | AsyncOk of 'T
        | AsyncException of exn
        | AsyncCanceled of OperationCanceledException

        /// Return an asynchronous computation that, when run, either returns a value, raises an exception 
        /// of cancels according to the value of the asynchronous result.
        static member Commit : AsyncResult<'T> -> Async<'T>

    [<Sealed>]
    /// A helper type to store a single result from an asynchronous computation and asynchronously
    /// access its result.
    /// 
    /// When using .NET 4.0 you can often use Task&lt;'T&gt; instead of this type
    type AsyncResultCell<'T> =
        /// Record the result in the AsyncResultCell.
        /// Subsequent sets of the result are ignored. This can happen, 
        /// e.g. for a race between a cancellation and a success.
        member RegisterResult:AsyncResult<'T> -> unit

        /// Wait for the result and commit it
        member AsyncResult : Async<'T>
        /// Create a new result cell
        new : unit -> AsyncResultCell<'T>
        
    [<AutoOpen>]
    /// Extensions to the F# Microsoft.FSharp.Control.Async module
    module AsyncExtensions =


#if FX_NO_CREATE_DELEGATE
#else
        type Microsoft.FSharp.Control.Async<'T> with 
            /// Return an asynchronous computation that waits for a single invocation of a .NET event by
            /// internally adding a handler to the event. Once the computation completes or is cancelled, the 
            /// handler is removed from the event.
            ///
            /// The computation will respond to cancellation while waiting for the completion
            /// of the operation. If 'cancelAction' is specified, cancellation causes the given 
            /// function to be executed. The computation then continues to wait for the event.
            /// 
            /// If 'cancelAction' is not specified, then cancellation causes the computation
            /// to stop immediately, and if the event is subsequently raised it may
            /// be ignored.
            static member AwaitEvent: IEvent<'Delegate,'T> * ?cancelAction : (unit -> unit) -> Async<'T> when 'Delegate : delegate<'T,unit> and 'Delegate :> System.Delegate 

#endif

        type Microsoft.FSharp.Control.Async<'T> with 
            /// Return three functions that can be used to implement the .NET Asynchronous 
            /// Programming Model (APM) for a given asynchronous computation.
            /// 
            /// The functions should normally be published as members with prefix 'Begin',
            /// 'End' and 'Cancel', and can be used within a type definition as follows:
            /// <c>
            ///   let beginAction,endAction,cancelAction = Async.AsBeginEnd computation
            ///   member x.BeginSomeOperation(callback,state) = beginAction(callback,state)
            ///   member x.EndSomeOperation(iar) = endAction(iar)
            ///   member x.CancelSomeOperation(iar) = cancelAction(iar)
            /// </c>
            ///
            /// The resulting API will be familiar to programmers in other .NET languages and 
            /// is a useful way to publish asynchronous computations in .NET components.
            static member AsBeginEnd : computation:Async<'T> -> 
                                         // The 'Begin' member
                                         (System.AsyncCallback * obj -> System.IAsyncResult) * 
                                         // The 'End' member
                                         (System.IAsyncResult -> 'T) * 
                                         // The 'Cancel' member
                                         (System.IAsyncResult -> unit)


    [<AutoOpen>]
    module StreamReaderExtensions =
        open System.IO

        type System.IO.StreamReader with 
            member AsyncReadToEnd: unit -> Async<string>
            [<System.Obsolete("Please use AsyncReadToEnd instead")>]
            member ReadToEndAsync : unit -> Async<string>

    [<AutoOpen>]
    module FileExtensions =
        open System.IO
        type System.IO.File with 
            /// Return an asynchronous computation that will open the file via a fresh blocking I/O thread.
            static member AsyncOpenText: path:string -> Async<StreamReader>

            /// Return an asynchronous computation that will open the file via a fresh blocking I/O thread.
            static member AsyncOpenRead: path:string -> Async<FileStream>

            /// Return an asynchronous computation that will open the file via a fresh blocking I/O thread.
            static member AsyncOpenWrite: path:string -> Async<FileStream>

            /// Return an asynchronous computation that will open the file via a fresh blocking I/O thread.
            static member AsyncAppendText: path:string -> Async<StreamWriter>

            /// Return an asynchronous computation that will open the file via a fresh blocking I/O thread.
            /// Pass 'options=FileOptions.Asynchronous' to enable further asynchronous read/write operations
            /// on the FileStream.
#if FX_NO_FILE_OPTIONS
            static member AsyncOpen: path:string * mode:FileMode * ?access: FileAccess * ?share: FileShare * ?bufferSize: int -> Async<FileStream>
#else
            static member AsyncOpen: path:string * mode:FileMode * ?access: FileAccess * ?share: FileShare * ?bufferSize: int * ?options: FileOptions -> Async<FileStream>
#endif

            [<System.Obsolete("Please use AsyncOpenText instead")>]
            static member OpenTextAsync: path:string -> Async<StreamReader>
            [<System.Obsolete("Please use AsyncOpenRead instead")>]
            static member OpenReadAsync: path:string -> Async<FileStream>
            [<System.Obsolete("Please use AsyncOpenWrite instead")>]
            static member OpenWriteAsync: path:string -> Async<FileStream>
            [<System.Obsolete("Please use AppendTextAsAsync instead")>]
            static member AppendTextAsync: path:string -> Async<StreamWriter>
            [<System.Obsolete("Please use AsyncOpen instead")>]
#if FX_NO_FILE_OPTIONS
            static member OpenAsync: path:string * mode:FileMode * ?access: FileAccess * ?share: FileShare * ?bufferSize: int -> Async<FileStream>
#else
            static member OpenAsync: path:string * mode:FileMode * ?access: FileAccess * ?share: FileShare * ?bufferSize: int * ?options: FileOptions -> Async<FileStream>
#endif

#if FX_NO_WEB_REQUESTS
#else
    [<AutoOpen>]
    module WebRequestExtensions =
        type System.Net.WebRequest with 
            /// Return an asynchronous computation that, when run, will wait for a response to the given WebRequest.
            member AsyncGetResponse : unit -> Async<System.Net.WebResponse>
            [<System.Obsolete("Please use AsyncGetResponse instead")>]
            member GetResponseAsync : unit -> Async<System.Net.WebResponse>
#endif
    
#if FX_NO_WEB_CLIENT
#else
    [<AutoOpen>]
    module WebClientExtensions =
        type System.Net.WebClient with
            member AsyncDownloadString : address:System.Uri -> Async<string>
#endif
