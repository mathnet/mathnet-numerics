//==========================================================================
// (c) Microsoft Corporation 2005-2009.  
//==========================================================================

#light
#nowarn "64" // fsiaux.fs(56,26): error FS0064: This construct causes code to be less generic than indicated by the type annotations. The type variable 'a has been constrained to be type ''a'

namespace Microsoft.FSharp.Compiler.Interactive

open System
open System.Diagnostics
open System.Threading
open Internal.Utilities.StructuredFormat
open Internal.Utilities.StructuredFormat.LayoutOps

[<assembly: System.Runtime.InteropServices.ComVisible(false)>]
[<assembly: System.CLSCompliant(true)>]  
do()

type IEventLoop =
    abstract Run : unit -> bool
    abstract Invoke : (unit -> 'a) -> 'a 
    abstract ScheduleRestart : unit -> unit
    
// An implementation of IEventLoop suitable for the command-line console
[<AutoSerializable(false)>]
type SimpleEventLoop() = 
    let runSignal = new AutoResetEvent(false)
    let exitSignal = new AutoResetEvent(false)
    let doneSignal = new AutoResetEvent(false)
    let queue = ref ([] : (unit -> obj) list)
    let result = ref (None : obj option)
    let setSignal(signal : AutoResetEvent) = while not (signal.Set()) do Thread.Sleep(1); done
    let waitSignal signal = WaitHandle.WaitAll([| (signal :> WaitHandle) |]) |> ignore
    let waitSignal2 signal1 signal2 = 
        WaitHandle.WaitAny([| (signal1 :> WaitHandle); (signal2 :> WaitHandle) |])
    let running = ref false
    let restart = ref false
    interface IEventLoop with 
         member x.Run() =  
             running := true;
             let rec run() = 
                 match waitSignal2 runSignal exitSignal with 
                 | 0 -> 
                     !queue |> List.iter (fun f -> result := try Some(f()) with _ -> None); 
                     setSignal doneSignal;
                     run()
                 | 1 -> 
                     running := false;
                     !restart
                 | _ -> run()
             run();
         member x.Invoke(f : unit -> 'a) : 'a  = 
             queue := [f >> box];
             setSignal runSignal;
             waitSignal doneSignal
             !result |> Option.get |> unbox
         member x.ScheduleRestart() = 
             // nb. very minor race condition here on running here, but totally 
             // unproblematic as ScheduleRestart and Exit are almost never called.
             if !running then 
                 restart := true; 
                 setSignal exitSignal
    interface System.IDisposable with 
         member x.Dispose() =
                     runSignal.Close();
                     exitSignal.Close();
                     doneSignal.Close();
                     


[<Sealed>]
type InteractiveSession() as self = 
    let mutable evLoop = (new SimpleEventLoop() :> IEventLoop)
    let mutable showIDictionary = true
    let mutable showDeclarationValues = true
    let mutable args = System.Environment.GetCommandLineArgs()
    let mutable opts = { FormatOptions.Default with ShowProperties=true; 
                                                   ShowIEnumerable=true; 
                                                   PrintWidth=78 } 
    // Add a default printer for dictionaries 
    let intercept (ienv: Internal.Utilities.StructuredFormat.IEnvironment) (obj:obj) = 
       match obj with 
       | null -> None 
       | :? System.Collections.IDictionary as ie ->
          let it = ie.GetEnumerator() 
          try 
              let itemLs = 
                  unfoldL // the function to layout each object in the unfold
                          (fun obj -> ienv.GetLayout obj) 
                          // the function to call at each step of the unfold
                          (fun () -> 
                              if it.MoveNext() then 
                                 Some((it.Key, it.Value),()) 
                              else None) () 
                          // the maximum length
                          (1+opts.PrintLength/3) 
              let makeListL itemLs =
                (leftL "[") $$
                sepListL (rightL ";") itemLs $$
                (rightL "]")
              Some(wordL "dict" --- makeListL itemLs)
          finally
             match it with 
             | :? System.IDisposable as d -> d.Dispose()
             | _ -> ()
             
       | _ -> None 

    let fireThreadExn, threadExn = Event.create<exn>()
    
    do self.PrintIntercepts <-  intercept :: self.PrintIntercepts
    
        
    member self.FloatingPointFormat 
       with get() = opts.FloatingPointFormat
       and set(x) = opts <-  { opts with FloatingPointFormat=x}
    member self.FormatProvider 
       with get() = opts.FormatProvider
       and set(x:System.IFormatProvider)= opts <-  { opts with FormatProvider=x}
    member self.PrintWidth  
       with get() = opts.PrintWidth
       and set(x) = opts <-  { opts with PrintWidth=x} 
    member self.PrintDepth  
       with get() = opts.PrintDepth
       and set(x) = opts <-  { opts with PrintDepth=x}
    member self.PrintLength  
       with get() = opts.PrintLength
       and set(x) = opts <-  { opts with PrintLength=x}
    member self.PrintSize  
       with get() = opts.PrintSize
       and set(x) = opts <-  { opts with PrintSize=x}     
    member self.ShowDeclarationValues
       with get() = showDeclarationValues
       and set(x) = showDeclarationValues <- x
    member self.ShowProperties  
       with get() = opts.ShowProperties
       and set(x) = opts <-  { opts with ShowProperties=x}
    member self.ShowIEnumerable 
       with get() = opts.ShowIEnumerable
       and set(x) = opts <-  { opts with ShowIEnumerable=x}
    member self.ShowIDictionary
       with get() = showIDictionary
       and set(x) = showIDictionary <-  x
    member self.PrintIntercepts
       with get() = opts.PrintIntercepts
       and set(x) = opts <- { opts with PrintIntercepts=x}
    member self.PrintOptions
       with get() = opts

    [<CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")>]
    member self.CommandLineArgs
       with get() = args 
       and set(x)  = args <- x
    member self.AddPrinter(f : 'a -> string) =
      let aty = typeof<'a> in 
      let intercept ienv (obj:obj) = 
         match obj with 
         | null -> None 
         | _ when (aty).IsAssignableFrom(obj.GetType())  -> 
            Some(Internal.Utilities.StructuredFormat.LayoutOps.wordL( f(unbox obj))) 
         | _ -> None in 
      opts <-  { opts with PrintIntercepts = (intercept :: opts.PrintIntercepts) }
    member self.EventLoop
       with get() = evLoop
       and set(x:IEventLoop)  = evLoop.ScheduleRestart(); evLoop <- x

    member self.AddPrintTransformer(f : 'a -> obj) =
      let aty = typeof<'a> in 
      let intercept (ienv:Internal.Utilities.StructuredFormat.IEnvironment)  (obj:obj) = 
         match obj with 
         | null -> None 
         | _ when (aty).IsAssignableFrom(obj.GetType())  -> 
            Some(ienv.GetLayout(f(unbox obj))) 
         | _ -> None in 
      opts <-  { opts with PrintIntercepts = (intercept :: opts.PrintIntercepts) }

    member x.ReportThreadException(exn:exn) = fireThreadExn(exn)

    member x.ThreadException = threadExn

[<assembly: CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Scope="member", Target="Microsoft.FSharp.Compiler.Interactive.InteractiveSession.#ThreadException")>]
do()
  
  
module Settings = 
    let fsi = new InteractiveSession()

    [<assembly: AutoOpen("Microsoft.FSharp.Compiler.Interactive.Settings")>]
    do()

module Internals = 
    open System
    open System.Reflection

    let savedIt = ref (typeof<int>,box 0)
    let SaveIt (x:'a) = (savedIt := (typeof<'a>, (box x)))
    let GetSavedIt () = snd !savedIt
    let GetSavedItType () = fst !savedIt
    let GetFsiPrintOptions () = Settings.fsi.PrintOptions
    let GetFsiShowDeclarationValues () = Settings.fsi.ShowDeclarationValues
