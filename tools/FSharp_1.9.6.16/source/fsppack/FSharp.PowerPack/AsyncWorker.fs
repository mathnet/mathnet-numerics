// (c) Microsoft Corporation 2005-2009. 
namespace Microsoft.FSharp.Control

    open System
    open System.Threading
    open System.IO
    open Microsoft.FSharp.Control
    open Microsoft.FSharp.Collections

#if FX_NO_SYNC_CONTEXT
#else
    type AsyncWorker<'T>(p,?asyncGroup) = 

        let syncContext = SynchronizationContext.Current
        do match syncContext with 
            | null -> failwith "Failed to capture the synchronization context of the calling thread. The System.Threading.SynchronizationContext.Current of the calling thread is null"
            | _ -> ()

        let post (syncContext : SynchronizationContext) trigger args =
                syncContext.Post(SendOrPostCallback(fun _ -> trigger args),state=null)

        // Trigger one of the following events when the iteration completes.
        let triggerCompleted,completed = Event.create<'T>()
        let triggerError    ,error     = Event.create()
        let triggerCanceled,canceled   = Event.create()
        let triggerProgress ,progress  = Event.create<int>()

        let asyncGroup = match asyncGroup with None -> new AsyncGroup() | Some ag -> ag

        let doWork() = 
            asyncGroup.RunWithContinuations( (fun res -> post syncContext triggerCompleted res),
                                             (fun exn -> post syncContext triggerError exn),
                                             (fun exn -> post syncContext triggerCanceled exn ),
                                             p)
                                

        member x.ReportProgress(progressPercentage) = 
            post syncContext triggerProgress progressPercentage
        
        member x.RunAsync()    = 
            ThreadPool.QueueUserWorkItem(fun args -> doWork())

        member x.CancelAsync(?message) = 
            asyncGroup.TriggerCancel(?message=message); 

        member x.ProgressChanged     = progress
        member x.Completed  = completed
        member x.Canceled   = canceled
        member x.Error      = error


#endif
