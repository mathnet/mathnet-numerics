//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Control

    open System
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Control
    open Microsoft.FSharp.Collections

#if FX_NO_DELEGATE_DYNAMIC_METHOD // not available on CompactFramework 2.0
#else
    /// Event implementations for an arbitrary type of delegate
    type DelegateEvent<'Del when 'Del :> System.Delegate> = 
        /// Create an event object suitable for implementing an arbitrary type of delegate
        new : unit -> DelegateEvent<'Del>
        /// Trigger the event using the given parameters
        member Trigger : args:obj[] -> unit
        /// Publish the event as a first class event value
        member Publish : IDelegateEvent<'Del>

    /// Event implementations for a delegate types following the standard .NET Framework convention of a first 'sender' argument
    type Event<'Del,'Args when 'Del : delegate<'Args,unit> and 'Del :> System.Delegate > = 
        /// Create an event object suitable for delegate types following the standard .NET Framework convention of a first 'sender' argument
        new : unit -> Event<'Del,'Args>
        /// Trigger the event using the given sender object and parameters. The sender object may be <c>null</c>.
        member Trigger : sender:obj * args:'Args -> unit
        /// Publish the event as a first class event value
        member Publish : IEvent<'Del,'Args> 

#endif
    /// Event implementations for the IEvent&lt;_&gt; type
    type Event<'T> = 
        /// Create an event object suitable for implementing for the IEvent&lt;_&gt; type
        new : unit -> Event<'T>
        /// Trigger the event using the given parameters
        member Trigger : arg:'T -> unit
        /// Publish the event as a first class event value
        member Publish : IEvent<'T> 


    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    /// Basic operations on first class event objects.  
    module Event = 

        /// Create an IEvent with no initial listeners. Two items are returned: 
        /// a function to invoke (trigger) the event, and the event that clients 
        /// can plug listeners into.
        val create : unit -> ('T -> unit) * IEvent<'T>

        /// Fire the output event when either of the input events fire
        val merge: event1:IEvent<'del1,'T> -> event2:IEvent<'del2,'T> -> IEvent<'T>

        /// Return a new event that passes values transformed by the given function
        val map: mapping:('T -> 'U) -> sourceEvent:IEvent<'Del,'T> -> IEvent<'U>

        /// Return a new event that listens to the original event and triggers the resulting
        /// event only when the argument to the event passes the given function
        val filter: predicate:('T -> bool) -> sourceEvent:IEvent<'Del,'T> -> IEvent<'T>

        /// Return a new event that listens to the original event and triggers the 
        /// first resulting event if the application of the predicate to the event arguments
        /// returned true, and the second event if it returned false
        val partition: predicate:('T -> bool) -> sourceEvent:IEvent<'Del,'T> -> (IEvent<'T> * IEvent<'T>)

        /// Return a new event that listens to the original event and triggers the 
        /// first resulting event if the application of the function to the event arguments
        /// returned a Choice1Of2, and the second event if it returns a Choice2Of2
        val split: splitter:('T -> Choice<'U1,'U2>) -> sourceEvent:IEvent<'Del,'T> -> (IEvent<'U1> * IEvent<'U2>)

        /// Return a new event which fires on a selection of messages from the original event.
        /// The selection function takes an original message to an optional new message.
        val choose: chooser:('T -> 'U option) -> sourceEvent:IEvent<'Del,'T> -> IEvent<'U>

        /// Return a new event consisting of the results of applying the given accumulating function
        /// to successive values triggered on the input event.  An item of internal state
        /// records the current value of the state parameter.  The internal state is not locked during the
        /// execution of the accumulation function, so care should be taken that the 
        /// input IEvent not triggered by multiple threads simultaneously.
        val scan: collector:('U -> 'T -> 'U) -> state:'U -> sourceEvent:IEvent<'Del,'T> -> IEvent<'U> 

        /// Run the given function each time the given event is triggered.
        val listen : callback:('T -> unit) -> sourceEvent:IEvent<'Del,'T> -> unit

        /// Return a new event that triggers on the second and subsequent triggerings of the input event.
        /// The Nth triggering of the input event passes the arguments from the N-1th and Nth triggering as
        /// a pair. The argument passed to the N-1th triggering is held in hidden internal state until the 
        /// Nth triggering occurs.
        ///
        /// You should ensure that the contents of the values being sent down the event are
        /// not mutable. Note that many EventArgs types are mutable, e.g. MouseEventArgs, and
        /// each firing of an event using this argument type may reuse the same physical 
        /// argument obejct with different values. In this case you should extract the necessary
        /// information from the argument before using this combinator.    
        val pairwise: sourceEvent:IEvent<'Del,'T> -> IEvent<'T * 'T>

