
#light

namespace Microsoft.FSharp.Compiler.Interactive

type IEventLoop =
    /// Enter the run loop on the main thread of the event loop.
    /// Call this from the GUI thread.
    ///
    /// A return of 'true' indicates 'I was replaced mid-flight and you can try to restart a different event loop'
    /// A return of 'false' indicates a normal exit, e.g. a call to System.Windows.Forms.Application.Exit()
    abstract Run : unit -> bool  
    /// Call this from a worker thread. Runs the operation synchronously.
    abstract Invoke : (unit -> 'a) -> 'a 
    /// Call this from a worker thread. Notify the event loop that 
    /// it should exit the 'Run' loop on its main thread with a return value 
    /// of 'true' indicating an attempt to restart.  Ignored if not running.
    abstract ScheduleRestart : unit -> unit
    
[<Sealed>]
type InteractiveSession =
    member FloatingPointFormat: string with get,set
    member FormatProvider: System.IFormatProvider  with get,set
    member PrintWidth : int  with get,set
    member PrintDepth : int  with get,set
    member PrintLength : int  with get,set
    member PrintSize : int  with get,set      
    member ShowProperties : bool  with get,set
    member ShowIEnumerable: bool  with get,set
    member ShowDeclarationValues: bool  with get,set      
    member AddPrinter: ('a -> string) -> unit
    member AddPrintTransformer: ('a -> obj) -> unit
    
    /// The command line arguments after ignoring the arguments relevant to the interactive
    /// environment and replacing the first argument with the name of the last script file,
    /// if any. Thus 'fsi.exe test1.fs test2.fs -- hello goodbye' will give arguments
    /// 'test2.fs', 'hello', 'goodbye'.  This value will normally be different to those
    /// returned by System.Environment.GetCommandLineArgs and Sys.argv.
    member CommandLineArgs : string [] with get,set
    
    [<System.Obsolete("This property will be removed in a future version of F#")>]
    member EventLoop: IEventLoop with get,set
    
    [<System.Obsolete("This method will be removed in a future version of F#")>]
    member ReportThreadException: exn -> unit
    
    [<System.Obsolete("This method will be removed in a future version of F#")>]
    member ThreadException: IEvent<exn>
    

module Settings = 

  val fsi : InteractiveSession

/// Hooks (internal use only, may change without notice).
module Internals = 
    val GetFsiPrintOptions : unit -> Internal.Utilities.StructuredFormat.FormatOptions 
    val SaveIt : 'a -> unit  
    val GetSavedIt : unit -> obj
    val GetSavedItType : unit -> System.Type
    val GetFsiShowDeclarationValues : unit -> bool
(*    val openPaths : unit -> string[] *)

