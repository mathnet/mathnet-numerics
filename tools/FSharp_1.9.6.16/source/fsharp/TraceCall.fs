#light
namespace Internal.Utilities.Debug

open Internal.Utilities
open Internal.Utilities.Pervasives
open System
open System.IO
open System.Threading
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
open System.Diagnostics
open System.Runtime.InteropServices


module TraceInterop = 
    type MessageBeepType =
        | Default = -1
        | Ok = 0x00000000
        | Error = 0x00000010
        | Question = 0x00000020
        | Warning = 0x00000030
        | Information = 0x00000040

    [<DllImport("user32.dll", SetLastError=true)>]
    let MessageBeep(mbt:MessageBeepType):bool=failwith "" 

[<AbstractClass>]
[<Sealed>]
type Trace private() =
#if DEBUG
    static let mutable alreadyWarnedThatLoggingIsOn = false
#endif
    static let mutable log = ""
    static let TMinusZero = DateTime.Now
    static let indent = 0
    static let mutable out = Console.Out
    [<ThreadStatic>] [<DefaultValue>] static val mutable private indent:int    
    [<ThreadStatic>] [<DefaultValue>] static val mutable private threadName:string

    /// Set to the semicolon-delimited names of the logging classes to be reported.
    /// Use * to mean all.
    static member Log 
        with get() = log
        and set(value) = log<-value

    /// Output destination.
    static member Out 
        with get() = out
        and set(value:TextWriter) = out<-value

    /// True if the given logging class should be logged.
    static member ShouldLog(loggingClass) =
        let result = Trace.Log = "*" || Trace.Log.Contains(loggingClass^";") || Trace.Log.EndsWith(loggingClass,StringComparison.Ordinal)
#if DEBUG
        if result && not(alreadyWarnedThatLoggingIsOn) then
            alreadyWarnedThatLoggingIsOn <- true
            System.Windows.Forms.MessageBox.Show(sprintf "Tracing is on (ShouldLog(%s) is true)\r\nTrace.Log is \"%s\"" loggingClass Trace.Log) |> ignore
#endif
        result

    /// Description of the current thread.     
    static member private CurrentThreadInfo() =
        if String.IsNullOrEmpty(Trace.threadName) then sprintf "(id=%d)" Thread.CurrentThread.ManagedThreadId
        else sprintf "(id=%d,name=%s)" Thread.CurrentThread.ManagedThreadId Trace.threadName
        
    /// Report the elapsed time since start
    static member private ElapsedTime(start) = 
        let elapsed : TimeSpan = (DateTime.Now-start)
        sprintf "%A ms" elapsed.TotalMilliseconds
        
    /// Get a string with spaces for indention.
    static member private IndentSpaces() = new string(' ', Trace.indent)
        
    /// Log a message.
    static member private LogMessage(msg:string) =
        Trace.Out.Write(sprintf "%s%s" (Trace.IndentSpaces()) msg) 
        Trace.Out.Flush()
        if Trace.Out<>Console.Out then 
            // Always log to console.
            Console.Out.Write(sprintf "%s%s" (Trace.IndentSpaces()) msg) 
        
    /// Name the current thread.
    static member private NameCurrentThread(threadName) =
        match threadName with 
        | Some(threadName)->
            let current = Trace.threadName
            if String.IsNullOrEmpty(current) then Trace.threadName <- threadName
            else if not(current.Contains(threadName)) then Trace.threadName <- current^","^threadName
        | None -> ()

    /// Base implementation of the call function
    static member private CallImpl(loggingClass,functionName,descriptionFunc,threadName:string option) : IDisposable = 
        #if DEBUG
        if Trace.ShouldLog(loggingClass) then 
            Trace.NameCurrentThread(threadName)
            
            let threadInfo = Trace.CurrentThreadInfo()
            let indent = Trace.IndentSpaces()
            let description = try descriptionFunc() with e->"No description because of exception"
            let start = DateTime.Now
            
#if DEBUG_WITH_TIME_AND_THREAD_INFO
            Trace.LogMessage(sprintf "Entering %s(%s) %s t-plus %fms %s\n"
                                functionName
                                loggingClass
                                threadInfo
                                (start-TMinusZero).TotalMilliseconds
                                description)
#else
            Trace.LogMessage(sprintf "Entering %s(%s) %s\n"
                                functionName
                                loggingClass
                                description)
#endif
            Trace.indent<-Trace.indent+1
    
            {new IDisposable with
                member d.Dispose() = 
                    Trace.indent<-Trace.indent-1
#if DEBUG_WITH_TIME_AND_THREAD_INFO
                    Trace.LogMessage(sprintf "Exitting %s %s %s\n" 
                                       functionName 
                                       threadInfo
                                       (Trace.ElapsedTime(start)))}
#else
                    Trace.LogMessage(sprintf "Exitting %s\n" 
                                       functionName)}
#endif
        else 
            null : IDisposable  
        #else
            null : IDisposable  
        #endif                                       
                
    /// Log a method as its called.
    static member Call(loggingClass,functionName,descriptionFunc) = Trace.CallImpl(loggingClass,functionName,descriptionFunc,None)
    /// Log a method as its called. Expected always to be called on the same thread which will be named 'threadName'
    static member CallByThreadNamed(loggingClass,functionName,threadName,descriptionFunc) = Trace.CallImpl(loggingClass,functionName,descriptionFunc,Some(threadName))
    /// Log a message by logging class.
    static member PrintLine(loggingClass, messageFunc) = 
    #if DEBUG
        if Trace.ShouldLog(loggingClass) then 
            let message = try messageFunc() with _-> "    No message because of exception.\n"
            Trace.LogMessage(sprintf "    %s\n" message)
    #else
        ()
    #endif            

    /// Log a message by logging class.
    static member Print(loggingClass, messageFunc) = 
    #if DEBUG
        if Trace.ShouldLog(loggingClass) then 
            let message = try messageFunc() with _-> "No message because of exception.\n"
            Trace.LogMessage(message)
    #else
        ()
    #endif
            
    /// Make a beep when the given loggingClass is matched.
    static member private BeepHelper(loggingClass,beeptype) = 
    #if DEBUG
        if Trace.ShouldLog(loggingClass) then 
            TraceInterop.MessageBeep(beeptype) |> ignore
    #else
        ()
    #endif                    
        
    /// Make the "OK" sound when the given loggingClass is matched.
    static member BeepOk(loggingClass) = Trace.BeepHelper(loggingClass,TraceInterop.MessageBeepType.Ok)
            
    /// Make the "Error" sound when the given loggingClass is matched. 
    static member BeepError(loggingClass) = Trace.BeepHelper(loggingClass,TraceInterop.MessageBeepType.Error)
        
    /// Make the default sound when the given loggingClass is matched. 
    static member Beep(loggingClass) = Trace.BeepHelper(loggingClass,TraceInterop.MessageBeepType.Default)
            
            
