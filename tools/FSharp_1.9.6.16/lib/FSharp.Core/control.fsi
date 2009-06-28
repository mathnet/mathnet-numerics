//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

/// This namespace contains types for derived control constructs including 
/// on-demand and asynchronous computations.
namespace Microsoft.FSharp.Control

    #nowarn "44" // This construct is deprecated. This method will be removed in the next version of F# and may no longer be used. Consider using Async.RunWithContinuations

    open System
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Control
    open Microsoft.FSharp.Collections

#if FX_ATLEAST_40
    open System.Threading.Tasks
#endif

#if FX_NO_OPERATION_CANCELLED

    type OperationCanceledException =
        inherit System.Exception
        new : System.String -> OperationCanceledException
#endif

#if DONT_INCLUDE_DEPRECATED
    type
#else
    [<Sealed>]
    [<AutoSerializable(false)>]
    [<Obsolete("This type will be removed in the next version of F# and should no longer be used. Consider using Async.RunWithContinuations")>]
    type AsyncFuture<'T> =
          interface System.IDisposable 
          interface System.IAsyncResult
          /// Wait for the completion of the operation and get its result
          member Value : 'T


    /// A handle to a capability to cancel a set of asynchronous computations.
    and 
#endif

       [<Sealed>]
       AsyncGroup =
          /// Generate a new asynchronous group
          new : unit -> AsyncGroup
          /// Raise the cancellation condition for this group of computations
          member TriggerCancel : ?message:string -> unit
                    
          /// Run the asynchronous computation and await its result.
          ///
          /// If an exception occurs in the asynchronous computation then an exception is re-raised by this
          /// function.
          member RunSynchronously : computation:Async<'T> * ?timeout : int -> 'T

          /// Start the asynchronous computation as a work item. Do not await its result.
          member Start : computation:Async<unit> -> unit
          
#if FX_ATLEAST_40
          /// Creates a <c>System.Threading.Tasks.Task</c> that executes asynchronous computation.
          /// The task is not started.
          member CreateTask : computation:Async<'T> * ?taskCreationOptions : TaskCreationOptions -> Task<'T>
                    
          /// Starts a <c>System.Threading.Tasks.Task</c> that executes asynchronous computation.
          member StartAsTask : computation:Async<'T> * ?taskCreationOptions : TaskCreationOptions -> Task<'T>
          
          /// Creates an asynchronous computation which starts given computation as <c>System.Threading.Tasks.Task</c>
          member StartChildAsTask : computation:Async<'T> * ?taskCreationOptions : TaskCreationOptions -> Async<Task<'T>>
#endif 

#if DONT_INCLUDE_DEPRECATED
#else
          [<Obsolete("This method was renamed. It is now called RunSynchronously")>]
          member Run : computation:Async<'T> * ?timeout : int -> 'T

          [<Obsolete("This method was renamed. It is now called Start")>]
          member Spawn : computation:Async<unit> -> unit
          
          [<Obsolete("This method will be removed in the next version of F# and may no longer be used. Consider using Async.RunWithContinuations",true)>]
          member SpawnFuture : computation:Async<'T> -> AsyncFuture<'T>
#endif

          /// Start the asynchronous computation as a work item. 
          /// Return a handle to the computation as an AsyncFuture.
          member RunWithContinuations: continuation:('T -> unit) * exceptionContinuation:(exn -> unit) * cancellationContinuation:(OperationCanceledException -> unit) * computation:Async<'T> -> unit
    
    /// An asynchronous computation, which, when run, will eventually produce a value 
    /// of the given type, or else raise an exception. The value and/or exception is not returned 
    /// to the caller immediately, but is rather passed to a success continuation, exception continuation 
    /// or cancellation continuation. 
    ///
    /// Asynchronous computations are normally specified using the F# 'workflow' syntax for building 
    /// computations.
    ///
    /// When run, asynchronous computations can normally be thought of as running run in one of
    /// two modes: 'work item mode' or 'waiting mode'.
    ///
    ///    - 'work item mode' indicates that the computation is executing as a work item,
    ///      e.g. in the .NET Thread Pool via ThreadPool.QueueUserWorkItem, or is running
    ///      a brief event-response action on the GUI thread. 
    ///
    ///    - 'waiting mode' indicates the computations a waiting for asynchronous I/O completions,
    ///      typically suspended as thunks using ThreadPool.RegisterWaitForSingleObject. 
    ///
    /// Asynchronous computations running as 'work items' should not generally perform blocking
    /// operations, e.g. long running synchronous loops. However, some asynchronous 
    /// computations may, out of necessity, need to execute blocking I/O operations: 
    /// these should be run on new threads or a user-managed pool of threads specifically 
    /// dedicated to resolving blocking conditions. For example, System.IO.OpenFile is, by 
    /// design, a blocking operation. However frequently it is important to code as 
    /// if this is asynchronous. This can be done by executing Async.SwitchToNewThread
    /// as part of the workflow.
    ///
    /// When run, asynchronous computations belong to an AsyncGroup. This can usually be specified 
    /// when the async computation is started. The only action on an AsyncGroup is to raise a cancellation 
    /// condition for the AsyncGroup. Async values check the cancellation condition for their AsyncGroup 
    /// regularly, though synchronous computations within an asynchronous computation will not automatically 
    /// check this condition. This gives a user-level cooperative cancellation protocol.
    and 
       [<Sealed>]
       [<StructuralEquality(false); StructuralComparison(false)>]
       Async<'T>

    /// This static class holds members for creating and manipulating asynchronous computations
    and 
       [<Sealed>]
       Async =

        /// Run the asynchronous computation and await its result.
        ///
        /// If an exception occurs in the asynchronous computation then an exception is re-raised by this
        /// function.
        ///
        /// Run as part of the default AsyncGroup
        static member RunSynchronously : computation:Async<'T> * ?timeout : int -> 'T
        
#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This method was renamed. It is now called RunSynchronously")>]
        static member Run : computation:Async<'T> * ?timeout : int -> 'T
        
#if FX_NO_SYNC_CONTEXT
#else
        [<Obsolete("This method will be removed in a future version of F#")>]
        static member SpawnThenPostBack : computation:Async<'T> * postBack:('T -> unit) -> unit
#endif
#endif

        /// Start the asynchronous computation in the thread pool. Do not await its result.
        ///
        /// Run as part of the default AsyncGroup
        static member Start : computation:Async<unit> -> unit

#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This method was renamed. It is now called Start")>]
        static member Spawn : computation:Async<unit> -> unit

        [<Obsolete("This method will be removed in the next version of F# and may no longer be used. Consider using Async.RunWithContinuations",true)>]
        static member SpawnFuture : computation:Async<'T> -> AsyncFuture<'T>
#endif

#if FX_ATLEAST_40
        /// Creates a <c>System.Threading.Tasks.Task</c> that executes asynchronous computation.
        /// The task is not started.
        static member CreateTask : computation:Async<'T> * ?taskCreationOptions:TaskCreationOptions -> Task<'T>
        
        /// Starts a <c>System.Threading.Tasks.Task</c> that executes asynchronous computation.
        static member StartAsTask : computation:Async<'T> * ?taskCreationOptions:TaskCreationOptions -> Task<'T>
        
        /// Creates an asynchronous computation which starts given computation as <c>System.Threading.Tasks.Task</c>
        static member StartChildAsTask : computation:Async<'T> * ?taskCreationOptions:TaskCreationOptions -> Async<Task<'T>>
#endif
    
        /// Run an asynchronous computation, initially as a work item.
        ///
        /// Run as part of the default AsyncGroup
        static member RunWithContinuations: continuation:('T -> unit) * exceptionContinuation:(exn -> unit) * cancellationContinuation:(OperationCanceledException -> unit) * computation:Async<'T> -> unit

        //---------- Exceptions

        /// Get the default group for executing asynchronous computations
        static member Catch : computation:Async<'T> -> Async<Choice<'T,exn>>

        /// Specify an asynchronous computation that, when run, executes <c>computation</c>,
        /// If <c>p</c> is effectively cancelled before its termination then
        /// the process <c>f exn</c> is executed. 
        static member TryCancelled : computation:Async<'T> * compensation:(OperationCanceledException -> unit) -> Async<'T>

        //---------- Cancellation checking and performing actions on cancellation.

#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This method will be removed in a future version of F#")>]
        static member CancelCheck : unit -> Async<unit>
#endif

        /// Generate a scoped, cooperative cancellation handler for use within an asynchronous workflow. 
        ///
        /// <c>async { use! holder = Async.OnCancel f ... }</c> generates an asynchronous computation where, 
        /// if a cancellation happens any time during the execution of the asynchronous computation in the scope of 'holder',
        /// then action 'f' is executed on the thread that is performing the cancellation. You can use
        /// this to arrange for your own computation to be asynchronously notified that a cancellation has occurred, e.g.
        /// by setting a flag, or deregistering a pending I/O action.
        static member OnCancel : compensation: (string -> unit) -> Async<System.IDisposable>

        /// Raise the cancellation condition for the most recent set of Async computations started without any specific AsyncGroup.
        /// Replace the global group with a new global group for any asynchronous computations created after this point without 
        /// any specific AsyncGroup.
        static member CancelDefaultGroup :  ?message:string -> unit 

        /// Get the default group for executing asynchronous computations
        static member DefaultGroup :  AsyncGroup

        //---------- Parallelism

        /// Start a child computation within an asynchronous workflow. 
        /// This allows multiple asynchronous computations to be executed simultaneously.
        /// 
        /// This method should normally be used as the immediate 
        /// right-hand-side of a 'let!' binding in an F# asynchronous workflow, i.e.,
        /// 
        ///        async { ...
        ///                let! completor1 = childComputation1 |&gt; Async.StartChild  
        ///                let! completor2 = childComputation2 |&gt; Async.StartChild  
        ///                ... 
        ///                let! result1 = completor1 
        ///                let! result2 = completor2 
        ///                ... }
        /// 
        /// When used in this way, each use of <c>StartChild</c> starts an instance of <c>childComputation</c> 
        /// and returns a completor object representing a computation to wait for the completion of the operation.
        /// When executed, the completor awaits the completion of <c>childComputation</c>. 
        static member StartChild : computation:Async<'T> * ?millisecondsTimeout : int -> Async<Async<'T>>
        

#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This function is now obsolete. Use <c>Async.StartChild(computation) |&gt; Async.Ignore</c> or <c>let! _ = Async.StartChild(computation)</c>. This indicates that results and errors from the child computation are ignored")>]
        static member SpawnChild : computation:Async<unit> -> Async<unit>

        [<Obsolete("This method will be removed in a future version of F#")>]
        static member Generate : itemCount:int * generator: (int -> Async<'T>) * ?groupCount: int -> Async<'T array>
#endif
        
        /// Specify an asynchronous computation that, when run, executes all the given asynchronous computations, initially
        /// queueing each as work items and using a fork/join pattern. If any raise an exception then the 
        /// overall computation will raise the first detected exception, and attempt to cancel the others.
        /// All the sub-computations belong to an AsyncGroup that is a subsidiary of the AsyncGroup of the outer computations.
        static member Parallel : computationList:seq<Async<'T>> -> Async<'T array>

        /// Specify an asynchronous computation that, when run, executes the two asynchronous computations, starting each in the thread pool.
        /// If any raise an exception then the overall computation will raise an exception, and attempt to cancel the others.
        /// All the sub-computations belong to an AsyncGroup that is a subsidiary of the AsyncGroup of the outer computations.
        static member Parallel2 : computation1:Async<'T1> * computation2:Async<'T2> -> Async<'T1 * 'T2>

        /// Specify an asynchronous computation that, when run, executese the three asynchronous computations, starting each in the thread pool.
        /// If any raise an exception then the overall computation will raise an exception, and attempt to cancel the others.
        /// All the sub-computations belong to an AsyncGroup that is a subsidiary of the AsyncGroup of the outer computations.
        static member Parallel3 : computation1:Async<'T1> * computation2:Async<'T2> * computation3:Async<'T3> -> Async<'T1 * 'T2 * 'T3>

        //---------- Thread Control
        
        /// Specify an asynchronous computation that, when run, creates a new thread and runs
        /// its continutation in that thread
        static member SwitchToNewThread : unit -> Async<unit> 
        
        /// Specify an asynchronous computation that, when run, queues a CPU-intensive work in the thread pool item that runs
        /// its continutation.
        static member SwitchToThreadPool :  unit -> Async<unit> 

#if FX_NO_SYNC_CONTEXT
#else
        /// Specify an asynchronous computation that, when run, runs
        /// its continuation using syncContext.Post. If syncContext is null 
        /// then the asynchronous computation is equivalent to SwitchToThreadPool().
        static member SwitchToGuiThread :  syncContext:System.Threading.SynchronizationContext -> Async<unit> 
#endif

                    
        //---------- Building new primitive Async values

#if DONT_INCLUDE_DEPRECATED
#else
        [<OverloadID("Primitive")>]
        [<Obsolete("This overload will be removed in a future version of F#. Please use the overload where the callback takes three continuations, for success, exception and cancellation",true)>]
        static member Primitive : callback:(('T -> unit) * (exn -> unit) -> unit) -> Async<'T>
#endif

        /// Specify an asynchronous computation that, when run, executes the given callback. 
        /// The callback must eventually call either the continuation,
        /// the exception continuation or the cancel exception.
        [<OverloadID("Primitive_with_ccont")>]
        static member Primitive : callback:(('T -> unit) * (exn -> unit) * (OperationCanceledException -> unit) -> unit) -> Async<'T>

        /// Specify an asynchronous computation in terms of a Begin/End pair of actions in 
        /// the style used in .NET APIs where the overall operation is not qualified by any arguments. For example, 
        ///     <c>Async.BuildPrimitive(ws.BeginGetWeather,ws.EndGetWeather)</c>
        /// When the computation is run, the 'Begin' half of the operation is executed, and
        /// an asynchronous computation is returned that, when run, awaits the completion 
        /// of the computation and fetches its overall result using the 'End' operation.
        [<OverloadID("BuildPrimitve_zero_arg")>]
        static member BuildPrimitive : beginAction:(System.AsyncCallback * obj -> System.IAsyncResult) * endAction:(System.IAsyncResult -> 'T) -> Async<'T>

        /// Specify an asynchronous computation in terms of a Begin/End pair of actions in 
        /// the style used in .NET APIs where the
        /// overall operation is qualified by one argument. For example, 
        ///     <c>Async.BuildPrimitive(place,ws.BeginGetWeather,ws.EndGetWeather)</c>
        /// When the computation is run, the 'Begin' half of the operation is executed, and
        /// an asynchronous computation is returned that, when run, awaits the completion 
        /// of the computation and fetches its overall result using the 'End' operation.
        [<OverloadID("BuildPrimitve_one_arg")>]
        static member BuildPrimitive : arg:'Arg1 * beginAction:('Arg1 * System.AsyncCallback * obj -> System.IAsyncResult) * endAction:(System.IAsyncResult -> 'T) -> Async<'T>

        /// Specify an asynchronous computation in terms of a Begin/End pair of actions in 
        /// the style used in .NET APIs where the
        /// overall operation is qualified by two arguments. For example, 
        ///     <c>Async.BuildPrimitive(arg1,arg2,ws.BeginGetWeather,ws.EndGetWeather)</c>
        /// When the computation is run, the 'Begin' half of the operation is executed, and
        /// an asynchronous computation is returned that, when run, awaits the completion 
        /// of the computation and fetches its overall result using the 'End' operation.
        [<OverloadID("BuildPrimitve_two_arg")>]
        static member BuildPrimitive : arg1:'Arg1 * arg2:'Arg2 * beginAction:('Arg1 * 'Arg2 * System.AsyncCallback * obj -> System.IAsyncResult) * endAction:(System.IAsyncResult -> 'T) -> Async<'T>

        /// Specify an asynchronous computation in terms of a Begin/End pair of actions in 
        /// the style used in .NET APIs where the overall operation is qualified by three arguments. For example, 
        ///     <c>Async.BuildPrimitive(arg1,arg2,arg3,ws.BeginGetWeather,ws.EndGetWeather)</c>
        /// When the computation is run, the 'Begin' half of the operation is executed, and
        /// an asynchronous computation is returned that, when run, awaits the completion 
        /// of the computation and fetches its overall result using the 'End' operation.
        [<OverloadID("BuildPrimitve_three_arg")>]
        static member BuildPrimitive : arg1:'Arg1 * arg2:'Arg2 * arg3:'Arg3 * beginAction:('Arg1 * 'Arg2 * 'Arg3 * System.AsyncCallback * obj -> System.IAsyncResult) * endAction:(System.IAsyncResult -> 'T) -> Async<'T>

        /// Specify an asynchronous computation that, when run, runs 'p', ignoring the result and
        /// returning the result '()'.
        static member Ignore : computation: Async<'T> -> Async<unit>


    [<Sealed>]
    /// The type of the 'async' operator, used to build workflows for asynchronous computations.
    type AsyncBuilder =
        /// Specify an asynchronous computation that, when run, enumerates the sequence 'seq'
        /// on demand and runs 'f' for each element.
        member For: sequence:seq<'T> * body:('T -> Async<unit>) -> Async<unit>

        /// Specify an asynchronous computation that, when run, just returns '()'
        member Zero : unit -> Async<unit> 

        /// Specify an asynchronous computation that, when run, first runs 'p1' and then runs 'p2', returning the result of 'p2'.
        member Combine : computation1:Async<unit> * computation2:Async<'T> -> Async<'T>

        /// Specify an asynchronous computation that, when run, runs 'p' repeatedly 
        /// until 'gd()' becomes false.
        member While : guard:(unit -> bool) * computation:Async<unit> -> Async<unit>

        /// Specify an asynchronous computation that, when run, returns the result 'v'
        member Return : value:'T -> Async<'T>

        /// Specify an asynchronous computation that, when run, runs 'f()'
        member Delay : generator:(unit -> Async<'T>) -> Async<'T>

        /// Specify an asynchronous computation that, when run, runs 'f(resource)'. 
        /// The action 'resource.Dispose()' is executed as this computation yields its result
        /// or if the asynchronous computation exits by an exception or by cancellation.
        member Using: resource:'T * binder:('T -> Async<'U>) -> Async<'U> when 'T :> System.IDisposable

        /// Specify an asynchronous computation that, when run, runs 'p', and when 
        /// 'p' generates a result 'T', runs 'f res'.
        member Bind: computation: Async<'T> * binder: ('T -> Async<'U>) -> Async<'U>
        
        /// Specify an asynchronous computation that, when run, runs 'p'. The action 'f' is executed 
        /// after 'p' completes, whether 'p' exits normally or by an exception. If 'f' raises an exception itself
        /// the original exception is discarded and the new exception becomes the overall result of the computation.
        member TryFinally : computation:Async<'T> * compensation:(unit -> unit) -> Async<'T>

        /// Specify an asynchronous computation that, when run, runs 'p' and returns its result.
        /// If an exception happens then 'f(exn)' is called and the resulting computation executed instead.
        member TryWith : computation:Async<'T> * catchHandler:(exn -> Async<'T>) -> Async<'T>

        /// Generate an object used to build asynchronous computations using F# computation expressions. The value
        /// 'async' is a pre-defined instance of this type.
        internal new : unit -> AsyncBuilder


    [<AutoOpen>]
    /// A module of extension members that provide asynchronous operations for some basic .NET types related to concurrency and I/O.
    module CommonExtensions =
        open System.IO
        
        type System.IO.Stream with 
            
            /// Return an asynchronous computation that will read from the stream into the given buffer. 
            member AsyncRead : buffer:byte[] * ?offset:int * ?count:int -> Async<int>
            
            /// Return an asynchronous computation that will read the given number of bytes from the stream 
            member AsyncRead : count:int -> Async<byte[]>
            
            /// Return an asynchronous computation that will write the given bytes to the stream 
            member AsyncWrite : buffer:byte[] * ?offset:int * ?count:int -> Async<unit>

        type System.Threading.WaitHandle with 
            /// Return an asynchronous computation that will wait on the given WaitHandle. This is scheduled
            /// as a wait-item in the .NET thread pool using RegisterWaitForSingleObject, 
            /// meaning that the operation will not block any .NET threads for 
            /// the duration of the wait.
            member AsyncWaitOne: ?millisecondsTimeout:int -> Async<bool>

        type System.Threading.Thread with 
            /// Return an asynchronous computation that will sleep for the given time. This is scheduled
            /// using a System.Threading.Timer object, meaning that the operation will not block any threads
            /// for the duration of the wait.
            static member AsyncSleep : millisecondsDueTime:int -> Async<unit>
            
#if FX_ATLEAST_40
        type System.Threading.Tasks.Task<'T> with
            member AsyncValue : Async<'T>
#endif            


    [<Sealed>]
    /// A handle to a capability to reply to a PostAndReply message
    type AsyncReplyChannel<'Reply> =
        /// Send a reply to a PostAndReply message
        member Reply : value:'Reply -> unit
        
    /// A MailboxProcessor is a message-processing agent defined using an asynchronous workflow. 
    /// The agent encapsulates a message queue that supports multiple-writers and the single reader agent.
    /// Writers send messages to the agent by using the Post, PostAndReply or AsyncPostAndReply methods.
    ///
    /// The reader agent is specified when creating the MailboxProcessor. The 
    /// agent is usually an asychronous workflow that waits for messages
    /// by using the Receive or TryReceive methods. 
    /// A MailboxProcessor may also scan through all available messages by using the 
    /// Scan or TryScan method. The encapsulated message queue only 
    /// supports a single active reader, thus at most one concurrent call to Receive, TryReceive,
    /// Scan and/or TryScan may be active at any one time.

    [<Sealed>]
    [<AutoSerializable(false)>]    
    type MailboxProcessor<'Msg> =

        /// Create an instance of a MailboxProcessor. The asynchronous computation executed by the
        /// processor is the one returned by the 'initial' function. This function is not executed until
        /// 'Start' is called.
        new :  initial:(MailboxProcessor<'Msg> -> Async<unit>) * ?asyncGroup: AsyncGroup -> MailboxProcessor<'Msg>

        /// Create and start an instance of a MailboxProcessor. The asynchronous computation executed by the
        /// processor is the one returned by the 'initial' function. 
        static member Start  :  initial:(MailboxProcessor<'Msg> -> Async<unit>) * ?asyncGroup: AsyncGroup -> MailboxProcessor<'Msg>

        /// Post a message to the message queue of the MailboxProcessor, asynchronously
        member Post : message:'Msg -> unit

        /// Post a message to the message queue of the MailboxProcessor and await a reply on the channel synchronously.
        /// The message is produced by a single call to the first function which must build a message
        /// containing the reply channel. The receiving MailboxProcessor must process this message and
        /// invoke the Reply method on the reply channel precisly once.
        member PostAndReply : buildMessage:(AsyncReplyChannel<'Reply> -> 'Msg) * ?timeout : int -> 'Reply

        /// Post a message to the message queue of the MailboxProcessor and await a reply on the channel asynchronously.
        /// The message is produced by a single call to the first function which must build a message
        /// containing the reply channel. The receiving MailboxProcessor must process this message and
        /// invoke the Reply method on the reply channel precisly once.
        member AsyncPostAndReply : buildMessage:(AsyncReplyChannel<'Reply> -> 'Msg) * ?timeout : int -> Async<'Reply>

        /// Like PostAndReply, but return None if no reply within the timeout period. 
        member TryPostAndReply : buildMessage:(AsyncReplyChannel<'Reply> -> 'Msg) * ?timeout : int -> 'Reply option

        /// Like AsyncPostAndReply, but return None if no reply within the timeout period. 
        member AsyncTryPostAndReply : buildMessage:(AsyncReplyChannel<'Reply> -> 'Msg) * ?timeout : int -> Async<'Reply option>

        /// Return an asynchronous computation which will 
        /// consume the first message in arrival order. No thread
        /// is blocked while waiting for further messages. Raise a TimeoutException
        /// if the timeout is exceeded.
        member Receive : ?timeout:int -> Async<'Msg>

        /// Return an asynchronous computation which will 
        /// consume the first message in arrival order. No thread
        /// is blocked while waiting for further messages. Return None
        /// if the timeout is exceeded.
        member TryReceive : ?timeout:int -> Async<'Msg option>
        
        /// Return an asynchronous computation which will 
        /// look through messages in arrival order until 'scanner' returns a Some value. No thread
        /// is blocked while waiting for further messages. Raise a TimeoutException
        /// if the timeout is exceeded.
        member Scan : scanner:('Msg -> (Async<'T>) option) * ?timeout:int -> Async<'T>

        /// Return an asynchronous computation which will 
        /// look through messages in arrival order until 'scanner' returns a Some value. No thread
        /// is blocked while waiting for further messages. Return None
        /// if the timeout is exceeded.
        member TryScan : scanner:('Msg -> (Async<'T>) option) * ?timeout:int -> Async<'T option>

#if DEBUG
        member UnsafeMessageQueueContents : obj 
#endif

        /// Start the MailboxProcessor
        member Start : unit -> unit

        /// Raise a timeout exception if a message not received in this amount of time. Default infinite.
        member DefaultTimeout : int with get, set

        interface System.IDisposable
