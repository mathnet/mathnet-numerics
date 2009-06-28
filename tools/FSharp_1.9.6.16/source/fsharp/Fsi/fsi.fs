// (c) Microsoft Corporation. All rights reserved *)

#light

module internal Microsoft.FSharp.Compiler.Interactive.Shell

[<assembly: System.Runtime.InteropServices.ComVisible(false)>]
[<assembly: System.CLSCompliant(true)>]  
do()

open Internal.Utilities

// #nowarn "64" // fsi.fs(1505,41): error FS0064: This construct causes code to be less generic than indicated by the type annotations. The type variable 'a has been constrained to be type ''a'.

module Ilsupp = Microsoft.FSharp.Compiler.AbstractIL.Internal.Support
module Ilmorph = Microsoft.FSharp.Compiler.AbstractIL.Morphs 
module Ilprint = Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter
module Ilreflect = Microsoft.FSharp.Compiler.AbstractIL.RuntimeWriter
module Tc = Microsoft.FSharp.Compiler.TypeChecker

open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler.AbstractIL.RuntimeWriter 

open Internal.Utilities.StructuredFormat

open System
open System.Diagnostics
open System.Runtime.InteropServices
open System.IO
open System.Text
open System.Threading
open System.Reflection
open System.Windows.Forms

open Microsoft.FSharp.Compiler.Interactive.Settings

open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Fscopts
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.Ilxgen
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.TypeChecker
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Opt
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Build
open Microsoft.FSharp.Compiler.Lexhelp
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.PostTypecheckSemanticChecks

//----------------------------------------------------------------------------
// Hardbinding dependencies should we NGEN fsi.exe
//----------------------------------------------------------------------------

open System.Runtime.CompilerServices
[<Dependency("FSharp.Compiler",LoadHint.Always)>] do ()    
[<Dependency("FSharp.Core",LoadHint.Always)>] do ()

let callStaticMethod (ty:Type) name args =
    ty.InvokeMember(name, (BindingFlags.InvokeMethod ||| BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic), null, null, Array.of_list args,Globalization.CultureInfo.InvariantCulture)

let callGenericStaticMethod (ty:Type) name tyargs args =
    let m = ty.GetMethod(name,(BindingFlags.InvokeMethod ||| BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic)) 
    let m = m.MakeGenericMethod(Array.of_list tyargs) 
    m.Invoke(null,Array.of_list args)

//----------------------------------------------------------------------------
// Timing support
//----------------------------------------------------------------------------

let product = "Microsoft F# Interactive"
let stdinMockFilename = "stdin"

[<AutoSerializable(false)>]
type TimeReporter() =
    let stopwatch = new System.Diagnostics.Stopwatch()
    let ptime = System.Diagnostics.Process.GetCurrentProcess()
    let numGC = System.GC.MaxGeneration
    member tr.TimeOp(f) =
        let startTotal = ptime.TotalProcessorTime
        let startGC = [| for i in 0 .. numGC -> System.GC.CollectionCount(i) |]
        stopwatch.Reset()
        stopwatch.Start()
        let res = f ()
        stopwatch.Stop()
        let total = ptime.TotalProcessorTime - startTotal
        let spanGC = [ for i in 0 .. numGC-> System.GC.CollectionCount(i) - startGC.[i] ]
        let elapsed = stopwatch.Elapsed in
        printfn "Real: %02d:%02d:%02d.%03d, CPU: %02d:%02d:%02d.%03d, GC %s" 
            elapsed.Hours elapsed.Minutes elapsed.Seconds elapsed.Milliseconds 
            total.Hours total.Minutes total.Seconds total.Milliseconds
            (String.concat ", " (List.mapi (sprintf "gen%d: %d") spanGC))
        res

    member tr.TimeOpIf flag f = if flag then tr.TimeOp(f) else f ()

let timeReporter = TimeReporter()


//----------------------------------------------------------------------------
// Console coloring
//----------------------------------------------------------------------------

// Testing shows "console coloring" is broken on some Mono configurations (e.g. Mono 2.4 Suse LiveCD).
// To support fsi usage, the console coloring is switched off by default on Mono.
do  if runningOnMono then enableConsoleColoring <- false 


//----------------------------------------------------------------------------
// value saving
//----------------------------------------------------------------------------

let saverPath  = ["Microsoft";"FSharp";"Compiler";"Interactive";"Internals";"SaveIt"]

//----------------------------------------------------------------------------
// value printing
//----------------------------------------------------------------------------

module ValuePrinting = 

    type PrintMode = PrintExpr | PrintDecl
    let printVal printMode (opts:FormatOptions) (x:obj) (ty:System.Type) = 
        // We do a dynamic invoke of any_to_layout with the right System.Type parameter for the static type of the saved value.
        // In principle this helps any_to_layout do the right thing as it descends through terms. In practice it means
        // it at least does the right thing for top level 'null' list and option values (but not for nested ones).
        //
        // The static type was saved into the location used by Internals.GetSavedItType when Internals.SaveIt was called.
        // Internals.SaveIt has type ('a -> unit), and fetches the System.Type for 'a by using a typeof<'a> call.
        // The funny thing here is that you might think that the driver (this file) knows more about the static types
        // than the compiled code does. But it doesn't! In particular, it's not that easy to get a System.Type value based on the
        // static type information we do have: we have no direct way to bind a F# TAST type or even an AbstractIL type to 
        // a System.Type value (I guess that functionality should be in ilreflect.fs).
        //
        // This will be more significant when we print values other then 'it'
        //
        try 
            let ass = typeof<Internal.Utilities.StructuredFormat.Layout>.Assembly
            let displayModule = ass.GetType("Internal.Utilities.StructuredFormat.Display")
            match printMode with
              | PrintDecl ->
                  // When printing rhs of fsi declarations, use "fsi_any_to_layout".
                  // This will suppress some less informative values, by returning an empty layout. [fix 4343].
                  (Internal.Utilities.StructuredFormat.Display.fsi_any_to_layout |> ignore); // if you adjust this then adjust the dynamic reference too            
                  callGenericStaticMethod displayModule "fsi_any_to_layout" [ty] [box opts; box x] |> unbox<Internal.Utilities.StructuredFormat.Layout>
              | PrintExpr -> 
                  (Internal.Utilities.StructuredFormat.Display.any_to_layout |> ignore); // if you adjust this then adjust the dynamic reference too            
                  callGenericStaticMethod displayModule "any_to_layout" [ty] [box opts; box x] |> unbox<Internal.Utilities.StructuredFormat.Layout>             
        with 
        | :? ThreadAbortException -> Layout.wordL ""
        | e ->
#if DEBUG
          Printf.printf "\n\nprintVal: x = %+A and ty=%s\n" x (ty.FullName)
#endif
          Printf.printf "\n\nException raised during pretty printing.\nPlease report this so it can be fixed.\nTrace: %s\n" (e.ToString()); 
          Layout.wordL ""
            
    let invokeDeclLayout tcGlobals emEnv ilxGenEnv (v:Val) =
        // Bug 2581 requests to print declared values (rather than just expressions).
        // This code supports it by providing a lookup from v to a concrete (System.Object,System.Type).
        // This (obj,objTy) pair can then be fed to the fsi value printer.
        // Note: The value may be (null:Object).
        // Note: A System.Type allows the value printer guide printing of nulls, e.g. as None or [].
        //-------
        // Ilxgen knows what the v:Val was converted to w.r.t. AbsIL datastructures.
        // Ilreflect knows what the AbsIL was generated to.
        // Combining these allows for obtaining the (obj,objTy) by reflection where possible.
        // This assumes the v:Val was given appropriate storage, e.g. StaticField.
        if Microsoft.FSharp.Compiler.Interactive.Internals.GetFsiShowDeclarationValues() then 
            // Adjust "opts" for printing for "declared-values":
            // - No sequences, because they may have effects or time cost.
            // - No properties, since they may have unexpected effects.
            // - Limit strings to roughly one line, since huge strings (e.g. 1 million chars without \n are slow in vfsi).
            // - Limit PrintSize which is a count on nodes.
            let declaredValueReductionFactor = 10 (* reduce PrintSize for declared values, e.g. see less of large terms *)
            let opts   = Microsoft.FSharp.Compiler.Interactive.Internals.GetFsiPrintOptions()
            let opts   = {opts with ShowProperties  = false} (* properties off, motivated by Form props *)
            let opts   = {opts with ShowIEnumerable = false} (* seq off, motivated by db query concerns *)
            let opts   = {opts with StringLimit = max 0 (opts.PrintWidth-4)} (* 4 allows for an indent of 2 and 2 quotes (rough) *)
            let opts   = {opts with PrintSize = opts.PrintSize / declaredValueReductionFactor } (* print less *)
            let res    = try  Ilxgen.lookupGeneratedValue (Ilreflect.lookupFieldRef  emEnv >> Option.get,
                                                           Ilreflect.lookupMethodRef emEnv >> Option.get,
                                                           Ilreflect.lookupTypeRef   emEnv >> Option.get,
                                                           Ilreflect.lookupType      emEnv) tcGlobals ilxGenEnv v
                         with e -> assert(false);
#if DEBUG
                                   dprintf "\nlookGenerateVal: failed on v=%+A v.Name=%s\n" v v.MangledName
#endif
                                   None (* lookup may fail *)
            match res with
              | None             -> None
              | Some (obj,objTy) -> let lay : layout = printVal PrintDecl opts obj objTy
                                    if isEmptyL lay then None else Some lay (* suppress empty layout *)
                                    
        else
            None
    
    let invokeExprPrinter denv vref = 
        let opts        = Microsoft.FSharp.Compiler.Interactive.Internals.GetFsiPrintOptions()
        let savedIt     = Microsoft.FSharp.Compiler.Interactive.Internals.GetSavedIt()
        let savedItType = Microsoft.FSharp.Compiler.Interactive.Internals.GetSavedItType()
        let rhsL  = printVal PrintExpr opts savedIt savedItType
        let fullL = if isEmptyL rhsL then
                      Tastops.NicePrint.valL denv vref (* the rhs was suppressed by the printer, so no value to print *)
                    else
                      (Tastops.NicePrint.valL denv vref ++ wordL "=") --- rhsL
        // let fullL = wordL " " $$ fullL // indent by 2 chars??
        Internal.Utilities.StructuredFormat.Display.output_layout opts stdout fullL;  
        stdout.WriteLine()
    


//----------------------------------------------------------------------------
// Reporting - syphon input text
//----------------------------------------------------------------------------


type Syphon() = 
    let syphonText = new StringBuilder()
#if DEBUG
    let syphonDump() =
        let text = syphonText.ToString()
        let lines = text.Split(Array.of_list [ '\n' ])  
        Array.iteri (fun i (s:string) -> dprintf "history %2d : %s\n" i s) lines
#endif

    member x.Reset () = ignore (syphonText.Remove(0,syphonText.Length))

    member x.Add (str:string) = // syphonDump();
        ignore (syphonText.Append(str))  (* ; printf "syphon: %s\n" str *)

    member x.GetLine filename i =
        if filename<> stdinMockFilename then "" else
        let text = syphonText.ToString()
        // In Visual Studio, when sending a block of text, it  prefixes  with '# <line> "filename"\n'
        // and postfixes with '# 1 "stdin"\n'. To first, get errors filename context,
        // and second to get them back into stdin context (no position stack...).
        // To find an error line, trim upto the last stdinReset string the syphoned text.
        //printf "PrePrune:-->%s<--\n\n" text;
        let rec prune (text:string) =
          let stdinReset = "# 1 \"stdin\"\n"
          let idx = text.IndexOf(stdinReset,StringComparison.Ordinal)
          if idx <> -1 then
            prune (text.Substring(idx + stdinReset.Length))
          else
            text
       
        let text = prune text
        //printf "PostPrune:-->%s<--\n\n" text;
        let lines = text.Split(Array.of_list [ '\n' ])
        if 0 < i && i <= lines.Length then lines.[i-1] else ""

//----------------------------------------------------------------------------
// Error reporting
//----------------------------------------------------------------------------

 
let ignoreAllErrors f = try f() with _ -> ()

let PrintError (tcConfig:TcConfigBuilder,syphon:Syphon,isWarn,err) = 
    ignoreAllErrors(fun () -> 
        DoWithErrorColor isWarn  (fun () ->
            stderr.WriteLine();
            writeViaBufferWithEnvironmentNewLines stderr (OutputErrorOrWarningContext "  " syphon.GetLine) err; 
            writeViaBufferWithEnvironmentNewLines stderr (OutputErrorOrWarning (tcConfig.implicitIncludeDir,tcConfig.showFullPaths,tcConfig.flatErrors,tcConfig.errorStyle,false))  err;
            stderr.WriteLine()))


/// This ErrorLogger reports all warnings, but raises StopProcessing on first error or early exit
type ErrorLoggerThatStopsOnFirstError(tcConfigB:TcConfigBuilder,syphon:Syphon) = 
    let mutable errors = 0 
    member x.SetError() = 
        errors <- 1
    member x.ErrorSink(err) = 
        PrintError(tcConfigB,syphon,false,err)
        errors <- errors + 1;
        if tcConfigB.abortOnError then exit 1 (* non-zero exit code *)
        // STOP ON FIRST ERROR (AVOIDS PARSER ERROR RECOVERY)
        raise StopProcessing 
    
    member x.CheckForNoErrors() = (errors = 0)
    member x.ResetErrorCount() = (errors <- 0)
    member x.ErrorCount = errors 
    
    
    interface ErrorLogger with
        member public x.WarnSink(err) = 
            DoWithErrorColor true (fun () -> 
                if ReportWarningAsError tcConfigB.globalWarnLevel tcConfigB.specificWarnOff tcConfigB.specificWarnAsError tcConfigB.globalWarnAsError err then 
                    x.ErrorSink err 
                else if ReportWarning tcConfigB.globalWarnLevel tcConfigB.specificWarnOff err then 
                    stderr.WriteLine();
                    writeViaBufferWithEnvironmentNewLines stderr (OutputErrorOrWarningContext "  " syphon.GetLine) err; 
                    writeViaBufferWithEnvironmentNewLines stderr (OutputErrorOrWarning (tcConfigB.implicitIncludeDir,tcConfigB.showFullPaths,tcConfigB.flatErrors,tcConfigB.errorStyle,true)) err;
                    stderr.WriteLine())
        member public x.ErrorSink(err) = x.ErrorSink(err)
        member public x.ErrorCount =  errors

    /// A helper function to check if its time to abort
    member x.AbortOnError() = 
        if errors > 0 
        then (eprintf "stopped due to error\n"; stderr.Flush(); raise StopProcessing) 
        else ()


//----------------------------------------------------------------------------
// cmd line - option state
//----------------------------------------------------------------------------

let dirname (s:string) = 
    if s = "" then "."
    else 
        match Path.GetDirectoryName(s) with 
        | null -> if Path.IsPathRooted(s) then s else "."
        | res -> if res = "" then "." else res

let defaultFSharpBinariesDir = System.AppDomain.CurrentDomain.BaseDirectory

//----------------------------------------------------------------------------
// tcConfig - build the initial config
//----------------------------------------------------------------------------

let tcConfigB = Build.TcConfigBuilder.CreateNew(defaultFSharpBinariesDir, 
                                                true, // long running: optimizeForMemory 
                                                Directory.GetCurrentDirectory())
let tcConfigP = TcConfigProvider.BasedOnMutableBuilder(tcConfigB)
do tcConfigB.resolutionEnvironment <- MSBuildResolver.RuntimeLike // See Bug 3608
do tcConfigB.product <- product
do tcConfigB.useFsiAuxLib <- true
do tcConfigB.includes <- (// BUG 890: #light does not start new block on RHS of <- assignment
                          let progFiles = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
                          let windows   = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.System),"..")
                          let rec path = function [] -> "" | [x] -> x | x::y::ys -> path (Path.Combine(x,y)::ys)
                          tcConfigB.includes)
// Preset: --optimize+ -g --tailcalls+ (see 4505)
do SetOptimizeSwitch tcConfigB On
do SetDebugSwitch    tcConfigB (Some "pdbonly") On
do SetTailcallSwitch On    


//----------------------------------------------------------------------------
// cmd line - state for options
//----------------------------------------------------------------------------

let readline = ref (not runningOnMono)
let gui        = ref true // override via "--gui", on by default
#if DEBUG
let showILCode = ref false // show modul il code 
#endif
let showTypes  = ref true  // show types after each interaction?
let saveEmittedCode = ref false
let fsiServerName = ref ""
let interact = ref true
let IsInteractiveServer() = !fsiServerName<>""  
let explicitArgs = ref []
let recordExplicitArg arg = explicitArgs := !explicitArgs @ [arg]

let fsiServerInputCodePage = ref None
let fsiServerOutputCodePage = ref None

// internal options  
let probeToSeeIfConsoleWorks         = ref true (* Retail always on *)
let peekAheadOnConsoleToPermitTyping = ref true (* Retail always on. REVIEW: cause of obscure GUI blocked until keypress bug? *)

// Additional fsi options are list below.
// In the "--help", these options can be printed either before (fsiUsagePrefix) or after (fsiUsageSuffix) the core options.

let displayHelpFsi tcConfigB (blocks:CompilerOptionBlock list) =
    DisplayBannerText tcConfigB;
    printfn ""
    printfn "Usage: fsi.exe <options> [script.fsx [<arguments>]]" // additional in fsi.exe 
    printCompilerOptionBlocks blocks
    exit 0

// option tags
let tagString      = "<string>"
let tagFile        = "<file>"
let tagNone        = ""
  
/// These options preceed the FsiCoreCompilerOptions in the help blocks
let fsiUsagePrefix inputFiles tcConfigB =
  [PublicOptions("- INPUT FILES -",
    [CompilerOption("use",tagFile, OptionString (fun s -> inputFiles := !inputFiles @ [(s,true)]), None,
                             [ "Use the given file on startup as initial input"]);
     CompilerOption("load",tagFile, OptionString (fun s -> inputFiles := !inputFiles @ [(s,false)]), None,
                             [ "#load the given file on startup"]);
    ]);
   PublicOptions("- CODE GENERATION -"     ,[]);
   PublicOptions("- ERRORS AND WARNINGS -" ,[]);
   PublicOptions("- LANGUAGE -"            ,[]);
   PublicOptions("- MISCELLANEOUS -"       ,[]);
   PublicOptions("- ADVANCED -"            ,[]);
   PrivateOptions(
    [(* Make internal fsi-server* options. Do not print in the help. They are used by VFSI. *)
     CompilerOption("fsi-server","", OptionString (fun s -> fsiServerName := s), None,
                             [ "FSI server mode on given named channel"]);
     CompilerOption("fsi-server-input-codepage","",OptionInt (fun n -> fsiServerInputCodePage := Some(n)), None,
                             [ " Set the input codepage for the console"]); 
     CompilerOption("fsi-server-output-codepage","",OptionInt (fun n -> fsiServerOutputCodePage := Some(n)), None,
                             [ " Set the output codepage for the console"]); 
     CompilerOption("fsi-server-no-unicode","", OptionUnit (fun () -> fsiServerOutputCodePage := None;  fsiServerInputCodePage := None), None,
                             [ "Do not set the codepages for the console"]);
     (* We do not want to print the "script.fsx arg2..." as part of the options *)     
     CompilerOption("script.fsx arg1 arg2 ...","",
                             OptionGeneral((fun args -> List.length args>0 && IsScript args.[0]),
                                           (fun args -> let scriptFile = args.[0]
                                                        let scriptArgs = List.tl args
                                                        inputFiles := !inputFiles @ [(scriptFile,true)]   (* record script.fsx for evaluation *)
                                                        List.iter recordExplicitArg scriptArgs            (* record rest of line as explicit arguments *)
                                                        tcConfigB.noFeedback <- true                      (* "quiet", no banners responses etc *)
                                                        interact := false                                 (* --exec, exit after eval *)
                                                        [] (* no arguments passed on, all consumed here *)
                                           )),None,
                             [ "Run script.fsx with the follow command line arguments: arg1 arg2 ..."]);
    ]);
   PrivateOptions(
    [(* Deprecated 2009.02.17. --no-gui and --gui is replaced by --gui[+/-] *)
     CompilerOption("no-gui","", OptionUnit(fun () -> warning(DeprecatedCommandLineOption("--no-gui", "Use '--gui-' instead", rangeCmdArgs));
                                                      gui := false), None,
                             [ "" ]);
     (* Deprecated 2009.02.17. "--no-debug-info" is the same as --debug- *)
     CompilerOption("no-debug-info","", OptionUnit (fun n -> warning(DeprecatedCommandLineOption("--no-debug-info", "Use '--debug-' instead", rangeCmdArgs));
                                                             tcConfigB.debuginfo <- false),None,
                             [ "Turn off generation of debug information for"
                               "dynamic code"]);
     (* Deprecated 2009.02.17. --no-readline replaced by --readline-, soon be replaced by --tabcomplete-, or removed altogether *)
     CompilerOption("no-readline",tagNone, OptionUnit (function () -> warning(DeprecatedCommandLineOption("--no-readline", "Use '--readline-' instead", rangeCmdArgs));
                                                                      readline := false),None,
                             [ ]);
     (* Private options, related to diagnostics around console probing *)
     CompilerOption("probeconsole","", OptionSwitch (fun flag -> probeToSeeIfConsoleWorks := flag=On), None,
                             [ "Probe to see if System.Console looks functional"]);
     CompilerOption("peekahead","", OptionSwitch (fun flag -> peekAheadOnConsoleToPermitTyping := flag=On), None,
                             [ "Probe to see if System.Console looks functional"]);
    ])
  ]

/// These options follow the FsiCoreCompilerOptions in the help blocks
let fsiUsageSuffix tcConfigB =
  [PublicOptions("- INPUT FILES -",
    [CompilerOption("--","", OptionRest recordExplicitArg, None,
                             [ "Treat remaining arguments as command line";
                               "arguments, accessed using fsi.CommandLineArgs"]);
    ]);
   PublicOptions("- MISCELLANEOUS -",    
    [   CompilerOption("help", tagNone,                      
                             OptionHelp (fun blocks -> displayHelpFsi tcConfigB blocks),None,
                             [ "Display this usage message (Short form: -?)" ])
    ]);
   PrivateOptions(
    [   CompilerOption("?"        , tagNone, OptionHelp (fun blocks -> displayHelpFsi tcConfigB blocks), None, [ "Short form of --help" ]);
        CompilerOption("help"     , tagNone, OptionHelp (fun blocks -> displayHelpFsi tcConfigB blocks), None, [ "Short form of --help" ]);
        CompilerOption("full-help", tagNone, OptionHelp (fun blocks -> displayHelpFsi tcConfigB blocks), None, [ "Short form of --help" ]);
    ]);
   PublicOptions("- ADVANCED -",
    [CompilerOption("exec","", OptionClear interact, None,
                             [ "Exit fsi after loading the files or running the"
                               ".fsx script given on the command line"]);
     CompilerOption("gui", tagNone, OptionSwitch (fun flag -> gui := (flag = On)),None,
                             [ "Execute interactions on a Windows Forms event";
                               "loop (on by default)"
                             ]);
     CompilerOption("quiet","", OptionUnit (fun () -> tcConfigB.noFeedback <- true), None,
                             [ "Suppress fsi writing to stdout"]);     
     (* Renamed --readline and --no-readline to --tabcompletion:+|- *)
     CompilerOption("readline",tagNone, OptionSwitch (function flag -> readline := (flag = On)),None,
                             [ "Support TAB completion in console (on by default)" ]);
    ]);
  ]
   
//----------------------------------------------------------------------------
// printfs - user, error
//----------------------------------------------------------------------------

let nullOut = new StreamWriter(Stream.Null) :> TextWriter
let kfprintf f (os:TextWriter) fmt = Printf.ktwprintf f os fmt
let fprintfnn (os: TextWriter) fmt  = kfprintf (fun _ -> os.WriteLine(); os.WriteLine()) os fmt   
/// uprintf to write usual responses to stdout (suppressed by --quiet), with various pre/post newlines
let uprintf    fmt = fprintf   (if tcConfigB.noFeedback then nullOut else System.Console.Out) fmt 
let uprintfn   fmt = fprintfn  (if tcConfigB.noFeedback then nullOut else System.Console.Out) fmt
let uprintfnn  fmt = fprintfnn (if tcConfigB.noFeedback then nullOut else System.Console.Out) fmt
let uprintnf   fmt = uprintfn ""; uprintf   fmt
let uprintnfn  fmt = uprintfn ""; uprintfn  fmt
let uprintnfnn fmt = uprintfn ""; uprintfnn fmt
  
/// eprintf to write errors to stderr (not suppressable (yet))
let eprintf fmt = eprintf fmt

 

//----------------------------------------------------------------------------
// Run "f x" on GUI thread, resorting to "dflt" if failure
//----------------------------------------------------------------------------

let RunCodeOnWinFormsMainThread (mainForm : Control) f = 
    if !progress then dprintf "RunCodeOnWinFormsMainThread: entry...\n";                  

    // Workaround: Mono's Control.Invoke returns a null result.  Hence avoid the problem by 
    // transferring the resulting state using a mutable location.
    // Note ownership of this mutable location gets temporarily transferred between threads,
    // but it is not concurrently read/written because Invoke is blocking.
    let mainFormInvokeResultHolder = ref None

    // Actually, Mono's Control.Invoke isn't even blocking (or wasn't on 1.1.15)!  So use a signal to indicate completion.
    // Indeed, we should probably do this anyway with a timeout so we can report progress from 
    // the GUI thread.
    use doneSignal = new AutoResetEvent(false)

    if !progress then dprintf "RunCodeOnWinFormsMainThread: invoking...\n";                  
    // BLOCKING: This blocks the stdin-reader thread until the
    // form invocation has completed.  NOTE: does not block on Mono
    mainForm.Invoke(new MethodInvoker(fun () -> 
                               try 
                                  mainFormInvokeResultHolder := Some(f ());
                               finally 
                                  doneSignal.Set() |> ignore)) |> ignore;
    let handles = Array.of_list [ (doneSignal :> WaitHandle) ] 
    if !progress then dprintf "RunCodeOnWinFormsMainThread: Waiting for completion signal....\n";
    while not (doneSignal.WaitOne(new TimeSpan(0,0,1),true)) do 
        if !progress then dprintf "."; stdout.Flush()

    if !progress then dprintf "RunCodeOnWinFormsMainThread: Got completion signal, res = %b\n" (Option.is_some !mainFormInvokeResultHolder);
    !mainFormInvokeResultHolder |> Option.get


//----------------------------------------------------------------------------
// Reporting - warnings, errors
//----------------------------------------------------------------------------

let syphon = new Syphon()
let errorLogger = ErrorLoggerThatStopsOnFirstError(tcConfigB,syphon)

let _ = InstallGlobalErrorLogger(fun _ -> errorLogger)


//----------------------------------------------------------------------------
// cmd line - parse options and process inputs
//----------------------------------------------------------------------------

let argv = System.Environment.GetCommandLineArgs()
/// Process command line, flags and collect filenames 
/// The ParseCompilerOptions function calls imperative function to process "real" args 
/// Rather than start processing, just collect names, then process them. 
let sourceFiles = 
    let inputFilesAcc   = ref ([] : (string * bool) list) 
    let collect name = 
        let fsx = Build.IsScript name
        inputFilesAcc := !inputFilesAcc @ [(name,fsx)] (* O(n^2), but n small... *)
    (try 
        let fsiCompilerOptions = fsiUsagePrefix inputFilesAcc tcConfigB @ GetCoreFsiCompilerOptions tcConfigB @ fsiUsageSuffix tcConfigB
        let abbrevArgs = abbrevFlagSet tcConfigB false
        ParseCompilerOptions collect fsiCompilerOptions (List.tl (PostProcessCompilerArgs abbrevArgs argv))
     with e ->
         stopProcessingRecovery e range0; exit 1);
    !inputFilesAcc

do 
    if tcConfigB.utf8output then
        let prev = System.Console.OutputEncoding
        System.Console.OutputEncoding <- System.Text.Encoding.UTF8
        System.AppDomain.CurrentDomain.ProcessExit.Add(fun _ -> System.Console.OutputEncoding <- prev)

do 
    let firstArg = 
        match sourceFiles with 
        | [] -> argv.[0] 
        | _  -> fst (List.hd (List.rev sourceFiles) )
    let args = Array.of_list (firstArg :: !explicitArgs) 
    fsi.CommandLineArgs <- args


//----------------------------------------------------------------------------
// Banner
//----------------------------------------------------------------------------

let prompt = if IsInteractiveServer() then "SERVER-PROMPT>\n" else "> "
let banner() =
    uprintnfn "Microsoft F# Interactive, (c) Microsoft Corporation, All Rights Reserved";
    uprintfnn "F# Version %s, compiling for .NET Framework Version %s" Ilxconfig.version (Ilsupp.clrVersion());
    uprintfn  "Please send bug reports to fsbugs@microsoft.com";
    uprintfn  "For help type #help;;"
 
let help() =
    uprintfn  ""
    uprintfnn "  F# Interactive directives:";
    uprintfn  "    #r \"file.dll\";;        reference (dynamically load) the given DLL.";
    uprintfn  "    #I \"path\";;            add the given search path for referenced DLLs.";
#if SUPPORT_USE
    uprintfn  "    #use \"file.fs\";;       use the given file, as if typed in.";
#endif
    uprintfn  "    #load \"file.fs\" ...;;  load the given file(s) as if compiled and referenced.";
    uprintfn  "    #time [\"on\"|\"off\"\"];;  toggle timing on/off.";
    uprintfn  "    #help;;                display help."
    uprintfn  "    #quit;;                exit."; (* last thing you want to do, last thing in the list - stands out more *)
    uprintfn  "";
    uprintfnn "  F# Interactive command line options:"
    uprintfn  "      See 'fsi --help' for options";
    uprintfn  "";
    if IsInteractiveServer() then 
        uprintfnn "  Visual Studio key bindings:";
        uprintfn  "      Up/Down   = cycle history"; 
        uprintfn  "      CTRL-DOT  = interrupt session";  (* CTRL-DOT in Visual Studio *)
        uprintfn  "      ALT-ENTER = send selected source text to FSI session (adds ;;)";
        uprintfn  ""
    uprintfn "  Please send bug reports to fsbugs@microsoft.com"; (* final message. Ask for bugs and *feedback* ? *)
    uprintfn "";


//----------------------------------------------------------------------------
// TabletPC warning, in case of imminent system hang!
//----------------------------------------------------------------------------

/// Determining Whether a PC is a Tablet PC 
/// http://msdn2.microsoft.com/en-us/library/ms700675(VS.85).aspx
/// http://blogs.msdn.com/swick/archive/2007/11/04/tabletpc-development-gotchas-part3-semantics-of-getsystemmetrics-sm-tabletpc-a.aspx
[<DllImport("user32.dll", EntryPoint=("GetSystemMetrics"))>]
extern bool GetSystemMetrics(int);
let WarningForTabletPC() =
    try 
      let SM_TABLETPC = 86
      if GetSystemMetrics(SM_TABLETPC) then
        uprintfn "NOTE:"
        uprintfn "NOTE: For Tablet PC (Input Service) users:"
        uprintfn "NOTE:   If the 'Tablet PC Input Service' is running, the system might hang."
        uprintfn "NOTE:   See the XP hotfix http://support.microsoft.com/kb/925271" (* intended no full stop, keep URL clear *)
        uprintfn "NOTE:   If this occurs, please report to fsbugs@microsoft.com"
    with e ->
        () // maybe Mono.
      
 
/// Set the input/output encoding. The use of a thread is due to a known bug on 
/// on Vista where calls to System.Console.InputEncoding can block the process.
let SetServerCodePages() =     
    match !fsiServerInputCodePage, !fsiServerOutputCodePage with 
    | None,None -> ()
    | inputCodePageOpt,outputCodePageOpt -> 
        let successful = ref false 
        Async.Spawn (async { do match inputCodePageOpt with 
                                | None -> () 
                                | Some(n:int) ->
                                      let encoding = System.Text.Encoding.GetEncoding(n) in 
                                      // Note this modifies the real honest-to-goodness settings for the current shell.
                                      // and the modifiations hang around even after the process has exited.
                                      System.Console.InputEncoding <- encoding
                             do match outputCodePageOpt with 
                                | None -> () 
                                | Some(n:int) -> 
                                      let encoding = System.Text.Encoding.GetEncoding(n) in 
                                      // Note this modifies the real honest-to-goodness settings for the current shell.
                                      // and the modifiations hang around even after the process has exited.
                                      System.Console.OutputEncoding <- encoding
                             do successful := true  });
        for pause in [10;50;100;1000;2000;10000] do 
            if not !successful then 
                System.Threading.Thread.Sleep(pause);
        if not !successful then 
            System.Windows.Forms.MessageBox.Show("A problem occurred starting the F# Interactive process. This may be due to a known problem with background process console support for Unicode-enabled applications on some Windows systems. Try selecting Tools->Options->F# Interactive for Visual Studio and enter '--fsi-server-no-unicode'") |> ignore


//----------------------------------------------------------------------------
// Prompt printing
//----------------------------------------------------------------------------

// A prompt gets "printed ahead" at start up. Tells users to start type while initialisation completes.
// A prompt can be skipped by "silent directives", e.g. ones sent to FSI by VS.
let mutable dropPrompt = 0
let promptPrint()      = if dropPrompt = 0 then uprintf "%s" prompt else dropPrompt <- dropPrompt - 1
let promptPrintAhead() = dropPrompt <- dropPrompt + 1; uprintf "%s" prompt
let promptSkipNext()   = dropPrompt <- dropPrompt + 1    


//----------------------------------------------------------------------------
// Startup...
//----------------------------------------------------------------------------
                
do if tcConfigB.showBanner then banner()
do if not runningOnMono then WarningForTabletPC()
do (try SetServerCodePages() with e -> warning(e))
do uprintfn ""
do if isNil  sourceFiles then promptPrintAhead()       (* When no source files to load, print ahead prompt here *)


//----------------------------------------------------------------------------
// Startup processing
//----------------------------------------------------------------------------

let consoleLooksFunctional() =
    if not !probeToSeeIfConsoleWorks then true else
    try
        // Probe to see if the console looks functional on this version of .NET
        let _ = System.Console.KeyAvailable 
        let _ = System.Console.ForegroundColor
        let _ = System.Console.CursorLeft <- System.Console.CursorLeft
        true
    with e -> 
        (* warning(Failure("Note: there was a problem setting up custom readline console support. Consider starting fsi.exe with the --no-readline option")); *)
        false

let consoleOpt =
    // The "console.fs" code does a limitted form of "TAB-completion".
    // Currently, it turns on if it looks like we have a console.
    //
    // UNDER REVIEW:
    //    Choose suitable defaults and allow user override via /switch?
    //    e.g. In VFSI, never use console code. [IsInteractiveServer() should be condition, not check].
    //    e.g. On Mono, if still problematic, then default to off?
    //    Once defaults and user switch considered, if exceptions thrown, report them and fallback??
    if !readline && consoleLooksFunctional() then
        Some(new Microsoft.FSharp.Compiler.Interactive.ReadLineConsole(fun (s1,s2) -> Seq.empty))
    else
        None

// When VFSI is running, there should be no "console", and in particular the console.fs readline code should not to run.
do  if IsInteractiveServer() then assert(consoleOpt = None)

/// This threading event gets set after the first-line-reader has finished its work
let consoleReaderStartupDone = new ManualResetEvent(false)

/// When using a key-reading console this holds the first line after it is read
let consoleFirstLine : string option ref = ref None

/// Peek on the standard input so that the user can type into it from a console window.
do if !interact then
     if !peekAheadOnConsoleToPermitTyping then 
      (new Thread(fun () -> 
          match consoleOpt with 
          | Some console when !readline && not (IsInteractiveServer()) ->
              if isNil(sourceFiles) then (
                  if !progress then dprintf "first-line-reader-thread reading first line...\n";
                  consoleFirstLine := Some(console.ReadLine()); 
                  if !progress then dprintf "first-line-reader-thread got first line = %s...\n" (any_to_string !consoleFirstLine);
              );
              consoleReaderStartupDone.Set() |> ignore 
              if !progress then dprintf "first-line-reader-thread has set signal and exited.\n" ;
          | _ -> 
              ignore(Console.In.Peek());
              consoleReaderStartupDone.Set() |> ignore 
        )).Start()
     else
       consoleReaderStartupDone.Set() |> ignore

/// FSI does a "startup" interaction to automatically page all the libary information.
/// This is mainly information for the typechecker environment.
/// Printing a prompt first means it should happen while the first line is being entered,
/// so effectively the background.

//----------------------------------------------------------------------------
// parsing - ParseInteraction
//----------------------------------------------------------------------------

let ParseInteraction (tokenizer:Lexfilter.LexFilter) =   
    let lastToken = ref Parser.ELSE (* Bug 1935: any token <> SEMICOLON_SEMICOLON will do for initial value *)
    try 
        if !progress then dprintf "In ParseInteraction...\n";

        let input = 
            Lexhelp.reusingLexbufForParsing (tokenizer.lexbuf,None) (fun () -> 
                let lexer = tokenizer.lexer
                let lexbuf = tokenizer.lexbuf
                let lexer = fun lexbuf -> (let tok = lexer lexbuf 
                                           lastToken := tok;
                                           tok)                        
                Parser.interaction lexer lexbuf)
        Some input
    with e ->
        // Bug 1935. On error, consume tokens until to ;; or EOF.
        // Caveat: Unless the error parse ended on ;; - so check the lastToken returned by the lexer function.
        // Caveat: What if this was a look-ahead? That's fine! Since we need to skip to the ;; anyway.     
        if !lastToken <> Parser.SEMICOLON_SEMICOLON then
            let lexer  = tokenizer.lexer  
            let lexbuf = tokenizer.lexbuf 
            let mutable tok = Parser.ELSE (* <-- any token <> SEMICOLON_SEMICOLON will do *)
            while tok <> Parser.SEMICOLON_SEMICOLON && not lexbuf.IsPastEndOfStream do
                tok <- lexer lexbuf            

        stopProcessingRecovery e range0;    
        None

//----------------------------------------------------------------------------
// tcImports, typechecker globals etc.
//----------------------------------------------------------------------------

let tcGlobals,tcImports =  
  try 
      TcImports.BuildTcImports(tcConfigP) 
  with e -> 
      stopProcessingRecovery e range0; exit 1

let ilGlobals  = tcGlobals.ilg

//----------------------------------------------------------------------------
// global objects
//----------------------------------------------------------------------------

let niceNameGen = NiceNameGenerator() 
// Share intern'd strings across all lexing/parsing
let lexResourceManager = new Lexhelp.LexResourceManager() 
let rangeStdin = rangeN stdinMockFilename 0

//----------------------------------------------------------------------------
// Final linking
//----------------------------------------------------------------------------

/// Add attributes 
let CreateModuleFragment assemblyName codegenResults =
    if !progress then dprintf "Creating main module...\n";
    let mainModule = mk_simple_mainmod assemblyName (fsharpModuleName tcConfigB.target assemblyName) (tcConfigB.target = Dll) (mk_tdefs codegenResults.ilTypeDefs) None None 0x0
    { mainModule 
      with modulManifest = 
            (let man = mainModule.ManifestOfAssembly
             Some { man with  manifestCustomAttrs = mk_custom_attrs codegenResults.ilAssemAttrs }); }


//----------------------------------------------------------------------------
// Compute assembly names
//----------------------------------------------------------------------------

#if FX_ATLEAST_40 // REVIEW: bug 3383
let generateDebugInfo = false
#else
let generateDebugInfo = tcConfigB.debuginfo
#endif

let outfile,pdbfile,assemblyName = "TMPFSCI.exe",None,"FSI-ASSEMBLY"

let assemblyBuilder,moduleBuilder = Ilreflect.mkDynamicAssemblyAndModule assemblyName generateDebugInfo

let writer = moduleBuilder.GetSymWriter()


//----------------------------------------------------------------------------
// InteractionState
//----------------------------------------------------------------------------

[<AutoSerializable(false)>]
type InteractionState =
    { optEnv    : Opt.IncrementalOptimizationEnv;
      emEnv     : Ilreflect.emEnv;
      tcGlobals : Env.TcGlobals;
      tcState   : Build.tcState; 
      ilxGenEnv : Ilxgen.ilxGenEnv;
      timing    : bool;
    }


let tcLockObject = box 7 // any new object will do
let tcLock thunk =
  lock tcLockObject (fun _ -> thunk())
      
//----------------------------------------------------------------------------
// ParseOneInputFile
//----------------------------------------------------------------------------

let ProcessInputs i istate inputs showTypes isIncrementalFragment prefixPath =
    let optEnv    = istate.optEnv
    let emEnv     = istate.emEnv
    let tcState   = istate.tcState
    let ilxGenEnv = istate.ilxGenEnv
    let tcConfig = TcConfig.Create(tcConfigB,validate=false)

    // typecheck 
    let tcState,topCustomAttrs,declaredImpls,tcEnvAtEndOfLastInput =

        tcLock (fun _ -> TypecheckClosedInputSet(errorLogger.CheckForNoErrors,tcConfig,tcImports,tcGlobals, Some prefixPath,tcState,inputs))
          

    // Logging/debugging
    if tcConfig.printAst then
        let (TAssembly(declaredImpls)) = declaredImpls
        for input in declaredImpls do 
            dprintf "AST:\n%+A\n" input

    errorLogger.AbortOnError();
     
    let importMap = tcImports.GetImportMap()

    (*moved printing of response to after the effects are executed because values may now be shown *)

    // optimize: note we collect the incremental optimization environment 
    let optimizedImpls, optData, optEnv = 
        ApplyAllOptimizations (tcConfig,tcGlobals,outfile,importMap,isIncrementalFragment,optEnv,tcState.Ccu,declaredImpls)
    errorLogger.AbortOnError();
        
    // codegen: note we collect the incremental optimization environment 
    let fragName = text_of_lid prefixPath 
    let codegenResults = GenerateIlxCode(true,runningOnMono,tcGlobals,tcConfig,importMap,topCustomAttrs,optimizedImpls,tcState.Ccu,fragName,ilxGenEnv)
    errorLogger.AbortOnError();
    //if assemAttrs <> [] or modulAttrs <> [] then warning(Failure("Assembly attributes are ignored by by F# Interactive"));

    // Each fragment is like a small separately compiled extension to a single source file. 
    // The incremental extension to the environment is dictated by the "signature" of the values as they come out 
    // of the type checker. Hence we add the declaredImpls (unoptimized) to the environment, rather than the 
    // optimizedImpls. 
    let ilxGenEnv = Ilxgen.AddIncrementalLocalAssmblyFragmentToIlxGenEnv isIncrementalFragment tcGlobals tcState.Ccu fragName ilxGenEnv declaredImpls in

    ReportTime tcConfig "TAST -> ILX";
    errorLogger.AbortOnError();
        
    (* step *)    
    ReportTime tcConfig "Linking";
    let ilxMainModule = CreateModuleFragment assemblyName codegenResults

    errorLogger.AbortOnError();
        
    (* step *)
    ReportTime tcConfig "ILX -> IL"; 
    let (ilxMainModule : ILModuleDef) = Ilxerase.ConvModule ilGlobals ilxMainModule
    errorLogger.AbortOnError();   
          
    ReportTime tcConfig "Assembly refs Normalised"; 
    let mainmod3 = Ilmorph.module_scoref2scoref_memoized (NormalizeAssemblyRefs tcImports) ilxMainModule
    errorLogger.AbortOnError();

#if DEBUG
    if !showILCode then 
        uprintnfn "--------------------";
        Ilprint.output_module stdout mainmod3;
        uprintnfn "--------------------"
#endif

    ReportTime tcConfig "Reflection.Emit";
    let emEnv,execs = Ilreflect.emitModuleFragment ilGlobals emEnv assemblyBuilder moduleBuilder mainmod3 generateDebugInfo
    if !saveEmittedCode then assemblyBuilder.Save(assemblyName ^ ".dll");
    errorLogger.AbortOnError();

    // Explicitly register the resources with the Sreflect module 
    // We would save them as resources into the dynamic assembly but there is missing 
    // functionality System.Reflection for dynamic modules that means they can't be read back out 
    //printf "#resources = %d\n" (length resources);
    for bytes in codegenResults.quotationResourceBytes do 
        Microsoft.FSharp.Quotations.Expr.RegisterReflectedDefinitions (assemblyBuilder, fragName, bytes);
        

    ReportTime tcConfig "Run Bindings";
    timeReporter.TimeOpIf istate.timing (fun () -> 
      execs |> List.iter (fun exec -> 
        match exec() with 
        | Some(e) -> 
            eprintf "%s\n" (e.ToString()); 
            errorLogger.SetError()
            errorLogger.AbortOnError(); 
        | None -> ())) ;

    errorLogger.AbortOnError();

    // Echo the decls (reach inside wrapping)
    // This code occurs AFTER the execution of the declarations.
    // So stored values will have been initialised, modified etc.
    if showTypes && not tcConfig.noFeedback then  
        let denv = tcState.TcEnvFromImpls.DisplayEnv
        let denv = if isIncrementalFragment then
                     // Extend denv with a (Val -> layout option) function for printing of val bindings.
                     {denv with generatedValueLayout = ValuePrinting.invokeDeclLayout istate.tcGlobals emEnv ilxGenEnv}
                   else
                     denv (* with #load items, the vals in the inferred signature do not tied up with those generated. Disabling printing. *)
        // open the path for the fragment we just compiled 
        let denv = denv_add_open_path (path_of_lid prefixPath) denv 

        let (TAssembly(declaredImpls)) = declaredImpls
        for (TImplFile(qname,_,mexpr)) in declaredImpls do
            let responseL = NicePrint.InferredSigOfModuleExprL false denv mexpr 
            if not (Layout.isEmptyL responseL) then      
                uprintfn "";
                // There are two copies of the layout sqashTo code (one in compiler, one in library).
                // The library one converts Leaf objects to strings on the fly.
                // The compiler one expects Lead objects to be strings already.
                let opts = Microsoft.FSharp.Compiler.Interactive.Internals.GetFsiPrintOptions()
                let responseL = Internal.Utilities.StructuredFormat.Display.squash_layout opts responseL
                Layout.renderL (Layout.channelR stdout) responseL |> ignore
                uprintfnn ""

    let istate = {istate with  optEnv    = optEnv;
                               emEnv     = emEnv;
                               ilxGenEnv = ilxGenEnv;
                               tcState   = tcState  }
    istate,tcEnvAtEndOfLastInput


//----------------------------------------------------------------------------
// EvalDefns, EvalExpr
//----------------------------------------------------------------------------

let mkId      str  = mksyn_id rangeStdin str 
let modIdN    i = mkId (DynamicModulePrefix ^ sprintf "%04d" i) // shows exn traces, make clear and fixed width 
let newModPathN  i = [modIdN i]
let genIntI = let i = ref 0 in fun () -> incr i; !i 

let EvalInputsFromLoadedFiles istate inputs =
    let i = genIntI()
    let prefix = newModPathN i 
    // Ensure the path includes the qualifying name 
    let inputs = inputs |> List.map (PrependPathToInput prefix) 
    let istate,_ = ProcessInputs i istate inputs true false prefix 
    istate

let EvalDefns istate showTypes defs =
    let ml       = false
    let filename = stdinMockFilename
    let i = genIntI()
    let prefix = newModPathN i
    let prefixPath = path_of_lid prefix
    let impl = ModuleOrNamespaceImpl(prefix,true, (* true indicates module, false namespace *) defs,emptyPreXmlDoc,[],None,rangeStdin)
    let input = ImplFileInput(ImplFile(filename,true, QualFileNameOfUniquePath (rangeStdin,prefixPath),[],[],[impl],true (* canContainEntryPoint *) ))
    let istate,tcEnvAtEndOfLastInput = ProcessInputs i istate [input] showTypes true prefix
    let tcState = istate.tcState 
    { istate with tcState = tcState.NextStateAfterIncrementalFragment(tcEnvAtEndOfLastInput) }
 
let EvalExpr istate expr =
    let m = rangeStdin
    let tcConfig = TcConfig.Create(tcConfigB,validate=false)
    let itName = "it" 
    let itID  = mksyn_id m itName
    let itExp = Expr_id_get itID
    let exprA = expr
    let exprB = Expr_app(ExprAtomicFlag.NonAtomic, Expr_lid_get(false, List.map mkId saverPath,m), itExp,m)
    let mkBind pat expr = Binding(None,DoBinding,false,false,[],emptyPreXmlDoc,SynInfo.emptyValSynData,pat,BindingRhs([],None,expr),m,NoSequencePointAtInvisibleBinding)
    let bindingA = mkBind (mksyn_pat_var None itID) exprA (* let it = <expr> *)
    let bindingB = mkBind (Pat_wild m)         exprB (* let _  = <istate.viewer> it *)
    let defA = Def_let (false, [bindingA], m)
    let defB = Def_let (false, [bindingB], m)
    let istate = EvalDefns istate false [defA;defB] 
    // Snarf the type for 'it' via the binding
    match istate.tcState.TcEnvFromImpls |> Tc.items_of_tenv |> NameMap.find itName with 
    | Nameres.Item_val vref -> 
         if not tcConfig.noFeedback then 
             ValuePrinting.invokeExprPrinter istate.tcState.TcEnvFromImpls.DisplayEnv (deref_val vref)
    | _ -> ()
    istate

let EvalRequireDll istate m path = 
    if IsInvalidPath(path) then
        error(Error(Printf.sprintf "'%s' is not a valid assembly name" path,m))
    // Check the file can be resolved before calling requireDLLReference 
    let _ = tcImports.ResolveLibFile(AssemblyReference(m,path),ResolveLibFileMode.ReportErrors)
    tcConfigB.AddReferencedAssemblyByPath(m,path);
    let tcState = istate.tcState 
    let tcEnv,(dllinfos,ccuinfos) = RequireDLL tcImports tcState.TcEnvFromImpls m path 
    let optEnv = List.fold AddExternalCcuToOpimizationEnv istate.optEnv ccuinfos
    let ilxGenEnv = AddExternalCcusToIlxGenEnv tcGlobals istate.ilxGenEnv (ccuinfos |> List.map (fun ccuinfo -> ccuinfo.FSharpViewOfMetadata)) 
    dllinfos,
    { istate with tcState = tcState.NextStateAfterIncrementalFragment(tcEnv);
                  optEnv = optEnv;
                  ilxGenEnv = ilxGenEnv }

let WithImplicitHome dir f = 
    let old = tcConfigB.implicitIncludeDir 
    tcConfigB.implicitIncludeDir <- dir;
    try f() 
    finally tcConfigB.implicitIncludeDir <- old
  
let ProcessMetaCommandsFromInputAsInteractiveCommands m istate sourceFile inp =
    let inp =
        match inp with
        | SigFileInput(_) ->  inp                   
        | ImplFileInput(ImplFile(filename,isScript,qualName,scopedPragmas,hashDirectives,impls,canContainEntryPoint)) ->
            if isScript then
              warning(Build.HashLoadedScriptConsideredSource(m))
              ImplFileInput(ImplFile(filename,false,qualName,scopedPragmas,hashDirectives,impls,canContainEntryPoint))
            else
              inp
    // REVIEW:2988 move loading printing to here...
    WithImplicitHome
       (dirname sourceFile) 
       (fun () ->
           ProcessMetaCommandsFromInput 
               ((fun st (m,nm) -> tcConfigB.TurnWarningOff(m,nm); st),
                (fun st (m,nm) -> snd (EvalRequireDll st m nm)),
                (fun st (m,nm) -> ()))  
               tcConfigB 
               inp 
               istate)
  
let EvalLoadFiles istate m sourceFiles =
    let tcConfig = TcConfig.Create(tcConfigB,validate=false)
    match sourceFiles with 
    | [] -> istate
    | _ -> 
      // use source file as if it were a module
      // REVIEW:2988 move loading printing to here...
      let sourceFiles = sourceFiles |> List.map (fun nm -> tcConfig.ResolveSourceFile(m,nm))
      // Intent "[Loading %s]\n" (String.concat "\n     and " sourceFiles)
      uprintf "[Loading "
      sourceFiles |> List.iteri (fun i sourceFile -> if i=0 then (uprintf "%s" sourceFile)
                                                            else (uprintnf "     and %s" sourceFile))
      uprintfn "]"
      let inputs = 
          sourceFiles 
          |> List.map (fun file -> Fscopts.ParseOneInputFile(tcConfig,lexResourceManager,["INTERACTIVE"],file,true,errorLogger))  
      errorLogger.AbortOnError();
      if List.exists (function None -> true | _ -> false) inputs then failwith "parse error";
      let inputs = List.map Option.get inputs 
      let istate = List.fold_left2 (ProcessMetaCommandsFromInputAsInteractiveCommands m) istate sourceFiles inputs
      EvalInputsFromLoadedFiles istate inputs  

//----------------------------------------------------------------------------
// FsiIntellisense - v1 - identifier completion - namedItemInEnvL
//----------------------------------------------------------------------------

let CompletionsForPartialLID istate (prefix:string) =
    let lid,stem =
        if prefix.IndexOf(".",StringComparison.Ordinal) >= 0 then
            let parts = prefix.Split(Array.of_list ['.'])
            let n = parts.Length
            Array.sub parts 0 (n-1) |> Array.to_list,parts.[n-1]
        else
            [],prefix   
    let tcState = istate.tcState (* folded through now? *)
    let amap = tcImports.GetImportMap()
    let infoReader = new Infos.InfoReader(tcGlobals,amap)
    let ncenv = new Nameres.NameResolver(tcGlobals,amap,infoReader,Nameres.FakeInstantiationGenerator)
    // Note: for the accessor domain we should use (AccessRightsOfEnv tcState.TcEnvFromImpls)
    let ad = Infos.AccessibleFromSomeFSharpCode
    let nItems = Nameres.ResolvePartialLongIdent ncenv (Tc.nenv_of_tenv tcState.TcEnvFromImpls) rangeStdin ad lid false
    let names  = nItems |> List.map (Nameres.DisplayNameOfItem tcGlobals) 
    let names  = names |> List.filter (fun (name:string) -> name.StartsWith(stem,StringComparison.Ordinal)) 
    names

//----------------------------------------------------------------------------
// FsiIntellisense (posible feature for v2) - GetDeclarations
//----------------------------------------------------------------------------

let FsiGetDeclarations istate (text:string) (names:string[]) =
    try
      let tcConfig = TcConfig.Create(tcConfigB,validate=false)
      Microsoft.FSharp.Compiler.SourceCodeServices.FsiIntelisense.getDeclarations
        (tcConfig,
         tcGlobals,
         tcImports,
         istate.tcState) 
        text 
        names
    with
      e ->
        System.Windows.Forms.MessageBox.Show("FsiGetDeclarations: throws:\n" ^ e.ToString()) |> ignore;
        [| |]


//----------------------------------------------------------------------------
// ctrl-c handling
//----------------------------------------------------------------------------

module ControlC = 

    type ControlEventHandler = delegate of int -> bool

    [<DllImport("kernel32.dll")>]
    extern bool SetConsoleCtrlHandler(ControlEventHandler callback,bool add)

    // One strange case: when a TAE happens a strange thing 
    // occurs the next read from stdin always returns
    // 0 bytes, i.e. the channel will look as if it has been closed.  So we check
    // for this condition explicitly.  We also recreate the lexbuf whenever CtrlC kicks.
    type StdinState = StdinEOFPermittedBecauseCtrlCRecentlyPressed | StdinNormal
    let stdinState = ref StdinNormal
    let CTRL_C = 0 
    let mainThread = Thread.CurrentThread 

    type SignalProcessorState =  
        | CtrlCCanRaiseException 
        | CtrlCIgnored 

    type KillerThreadRequest =  
        | ThreadAbortRequest 
        | NoRequest 
        | ExitRequest 
        | PrintInterruptRequest

    let ctrlcState = ref CtrlCIgnored
    let killThreadRequest = ref NoRequest
    let ctrlEventHandlers = ref [] : ControlEventHandler list ref
    let ctrlEventActions  = ref [] : (unit -> unit) list ref

    // Currently used only on Mono/Posix
    type PosixSignalProcessor() = 
        let mutable reinstate = (fun () -> ())
        member x.Reinstate with set(f) = reinstate <- f
        member x.Invoke(n:int) = 
             // we run this code once with n = -1 to make sure it is JITted before execution begins
             // since we are not allowed to JIT a signal handler.  THis also ensures the "Invoke"
             // method is not eliminated by dead-code elimination
             if n >= 0 then 
                 reinstate();
                 stdinState := StdinEOFPermittedBecauseCtrlCRecentlyPressed;
                 killThreadRequest := if (!ctrlcState = CtrlCCanRaiseException) then ThreadAbortRequest else PrintInterruptRequest
       
    // REVIEW: streamline all this code to use the same code on Windows and Posix.   
    let InstallCtrlCHandler((threadToKill:Thread),(pauseMilliseconds:int)) = 
        if !progress then dprintf "installing CtrlC handler\n";
        // WINDOWS TECHNIQUE: .NET has more safe points, and you can do more when a safe point. 
        // Hence we actually start up the killer thread within the handler. 
        try 
            let raiseCtrlC() = 
                Printf.eprintf "\n- Interrupt\n";  
                stdinState := StdinEOFPermittedBecauseCtrlCRecentlyPressed;
                if (!ctrlcState = CtrlCCanRaiseException) then 
                    killThreadRequest := ThreadAbortRequest;
                    let killerThread = 
                        new Thread(new ThreadStart(fun () ->
                            // sleep long enough to allow ControlEventHandler handler on main thread to return 
                            // Also sleep to give computations a bit of time to terminate 
                            Thread.Sleep(pauseMilliseconds);
                            if (!killThreadRequest = ThreadAbortRequest) then 
                                if !progress then uprintnfn "- Aborting main thread...";  
                                killThreadRequest := NoRequest;
                                threadToKill.Abort();
                            ()),Name="ControlCAbortThread") 
                    killerThread.IsBackground <- true;
                    killerThread.Start() 
        
            let ctrlEventHandler = new ControlEventHandler(fun i ->  if i = CTRL_C then (raiseCtrlC(); true) else false ) 
            ctrlEventHandlers := ctrlEventHandler :: !ctrlEventHandlers;
            ctrlEventActions  := raiseCtrlC       :: !ctrlEventActions;
            let resultOK = SetConsoleCtrlHandler(ctrlEventHandler,true)
            //Uncomment if failure is to be reported.
            //if not resultOK then
            //  uprintfn "SetConsoleCtrlHandler: failed to install handler for ctrl-C"
            false // don't exit via kill thread
        with e -> 
            if !progress then eprintf "Failed to install ctrl-c handler using Windows technique - trying to install one using Unix signal handling...\n";
            // UNIX TECHNIQUE: We start up a killer thread, and it watches the mutable reference location.    
            // We can't have a dependency on Mono DLLs (indeed we don't even have them!)
            // So SOFT BIND the following code:
            // Mono.Unix.Native.Stdlib.signal(Mono.Unix.Native.Signum.SIGINT,new Mono.Unix.Native.SignalHandler(fun n -> PosixSignalProcessor.Invoke(n))) |> ignore;
            match (try Choice1Of2(Assembly.Load("Mono.Posix, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756")) with e -> Choice2Of2 e) with 
            | Choice1Of2(monoPosix) -> 
              try
                if !progress then eprintf "loading type Mono.Unix.Native.Stdlib...\n";
                let monoUnixStdlib = monoPosix.GetType("Mono.Unix.Native.Stdlib") 
                if !progress then eprintf "loading type Mono.Unix.Native.SignalHandler...\n";
                let monoUnixSignalHandler = monoPosix.GetType("Mono.Unix.Native.SignalHandler") 
                if !progress then eprintf "creating delegate...\n";
                let target = new PosixSignalProcessor() 
                target.Invoke(-1);
                let monoHandler = System.Delegate.CreateDelegate(monoUnixSignalHandler,target,"Invoke") 
                if !progress then eprintf "registering signal handler...\n";
                let monoSignalNumber = System.Enum.Parse(monoPosix.GetType("Mono.Unix.Native.Signum"),"SIGINT")
                let register () = callStaticMethod monoUnixStdlib "signal" [ monoSignalNumber; box monoHandler ]  |> ignore 
                target.Reinstate <- register;
                register();
                let killerThread = 
                    new Thread(new ThreadStart(fun () ->
                        while true do 
                            //Printf.eprintf "\n- kill thread loop...\n"; stderr.Flush();  
                            Thread.Sleep(pauseMilliseconds*2);
                            match !killThreadRequest with 
                            | PrintInterruptRequest -> 
                                Printf.eprintf "\n- Interrupt\n"; stderr.Flush();  
                                killThreadRequest := NoRequest;
                            | ThreadAbortRequest -> 
                                Printf.eprintf "\n- Interrupt\n"; stderr.Flush();  
                                if !progress then uprintnfn "- Aborting main thread...";
                                killThreadRequest := NoRequest;
                                threadToKill.Abort()
                            | ExitRequest -> 
                                // Mono has some wierd behaviour where it blocks on exit
                                // once CtrlC has ever been pressed.  Who knows why?  Perhaps something
                                // to do with having a signal handler installed, but it only happens _after_
                                // at least one CtrLC has been pressed.  Maybe raising a ThreadAbort causes
                                // exiting to have problems.
                                //
                                // Anyway, we make "#q" work this case by setting ExitRequest and brutally calling
                                // the process-wide 'exit'
                                Printf.eprintf "\n- Exit...\n"; stderr.Flush();  
                                callStaticMethod monoUnixStdlib "exit" [ box 0 ] |> ignore
                            | _ ->  ()
                        done),Name="ControlCAbortAlternativeThread") 
                killerThread.IsBackground <- true;
                killerThread.Start();
                true // exit via kill thread to workaround block-on-exit bugs with Mono once a CtrlC has been pressed
              with e -> 
                eprintf "Failed to install ctrl-c handler. Ctrl-C handling will not be available (sorry). Error was:\n\t%s" e.Message 
                false
            | Choice2Of2 e ->
              eprintf "Failed to install ctrl-c handler - Ctrl-C handling will not be available (sorry). Error was:\n\t%s" e.Message 
              false  

open ControlC

//----------------------------------------------------------------------------
// assembly finder
//----------------------------------------------------------------------------

module MagicAssemblyResolution = 

    // FxCop identifies Assembly.LoadFrom.
    [<CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId="System.Reflection.Assembly.LoadFrom")>]
    let assemblyLoadFrom (path:string) = Assembly.LoadFrom(path)

    do  AppDomain.CurrentDomain.add_AssemblyResolve(new ResolveEventHandler(fun _ args -> 
       try 
           // Grab the name of the assembly
           let tcConfig = TcConfig.Create(tcConfigB,validate=false)
           let assemName = args.Name.Split([| ',' |]).[0]
           if !progress then uprintfn "ATTEMPT MAGIC LOAD ON ASSEMBLY, assemName = %s" assemName;
           
           // Special case: Mono Windows Forms attempts to load an assembly called something like "Windows.Forms.resources"
           // We can't resolve this, so don't try.
           // REVIEW: Suggest 4481, delete this special case.
           if assemName.EndsWith(".resources",StringComparison.OrdinalIgnoreCase) or (runningOnMono && assemName = "UIAutomationWinforms") then null else

           // Special case: Is this the global unique dynamic assembly for FSI code? In this case just
           // return the dynamic assembly itself.       
           if assemblyName = assemName then (assemblyBuilder :> Reflection.Assembly) else

           // Otherwise continue
           let fileName1 = (assemName + ".dll") 
           let fileName2 = (assemName + ".exe") 
           let overallSearchResult =           
               // OK, try to resolve as a .dll
               let searchResult = tcImports.TryResolveLibFile (AssemblyReference(rangeStdin,fileName1),ResolveLibFileMode.Speculative)

               match searchResult with
               | OkResult _ -> searchResult
               | _ -> 

               // OK, try to resolve as a .exe
               let searchResult = tcImports.TryResolveLibFile (AssemblyReference(rangeStdin,fileName2),ResolveLibFileMode.Speculative)

               match searchResult with
               | OkResult _ -> searchResult
               | _ -> 

               if !progress then uprintfn "ATTEMPT LOAD, fileName1 = %s" fileName1;
               /// Take a look through the files quoted, perhaps with explicit paths
               let searchResult = 
                   (tcConfig.referencedDLLs 
                        |> List.tryPick (fun assemblyReference -> 
                         if !progress then uprintfn "ATTEMPT MAGIC LOAD ON FILE, referencedDLL = %s" assemblyReference.Text;
                         if System.String.Compare(System.IO.Path.GetFileName assemblyReference.Text, fileName1,StringComparison.OrdinalIgnoreCase) = 0 then
                             Some(tcImports.TryResolveLibFile(assemblyReference,ResolveLibFileMode.Speculative))
                         else None ))

               match searchResult with
               | Some (OkResult _ as res) -> res
               | _ -> 

               // OK, assembly resolution has failed, at least according to the F# rules. We now give
               // other AssemblyResolve handlers a chance to run. 
               // This is a specific request from customers who customize the 
               // AssemblyResolve mechanism to do magic things like going to a distributed company file store
               // to pick up DLLs. This is also one of the reasons why the TryResolveLibFile paths can't 
               // report errors or warnings: we don't want spurious errors and warnings coming out before everyon
               // has had a chance to resolve an assembly.
               //
               // If all other AssemblyResolve also fail then we want to report a "nice" exception. But how do we know if
               // they failed? We just add a handler to the end of the AssemblyResolve chain, and if it
               // ever gets executed we know they failed.
               //
               // This is also a fix for bug 1171.
               let rec failingResolveHandler = 
                    new ResolveEventHandler(fun _ _ -> 

                        // OK, the failingResolveHandler is now running now so remove it from the list to prevent it
                        // ever running again
                        (try AppDomain.CurrentDomain.remove_AssemblyResolve(failingResolveHandler) with _ -> ());

                        // Now commit the warnings and errors by re-resolving. If the file suddenly exists in the milliseconds
                        // in between well, then we succeed
                        tcImports.ResolveLibFile(AssemblyReference(rangeStdin,fileName1),ResolveLibFileMode.ReportErrors).resolvedPath |> assemblyLoadFrom)

               AppDomain.CurrentDomain.add_AssemblyResolve(failingResolveHandler);
               ErrorResult([],Failure "no resolution")
                       
           match overallSearchResult with 
           | ErrorResult _ -> null
           | OkResult _ -> 
               let res = CommitOperationResult overallSearchResult
               if assemName <> "Mono.Posix" then uprintfn "Binding session to '%s'..." res.resolvedPath;
               assemblyLoadFrom(res.resolvedPath)
               
       with e -> 
           stopProcessingRecovery e range0; 
           null));

      
//----------------------------------------------------------------------------
// showInfo
//----------------------------------------------------------------------------

// REVIEW: add to RETAIL build? This could be useful diagnostics. Renamed?
#if DEBUG
/// Dump internal state information for diagnostics and regression tests.  
let showInfo (istate:InteractionState) =
    PrintOptionInfo tcConfigB
#endif


//----------------------------------------------------------------------------
// interactive loop
//----------------------------------------------------------------------------

type stepStatus = CtrlC | EndOfFile | Completed | CompletedWithReportedError

let InteractiveCatch f istate = 
    try
        (* reset error count *)
        errorLogger.ResetErrorCount();  
        f istate
    with  e ->
        stopProcessingRecovery e range0;
        istate,CompletedWithReportedError

//----------------------------------------------------------------------------
// Process one parsed interaction.  This runs on the GUI thread.
// It might be simpler if it ran on the parser thread.
//----------------------------------------------------------------------------

// #light is the default for FSI
let interactiveInputLightSyntaxStatus = 
    LightSyntaxStatus (tcConfigB.light <> Some(false), false (* no warnings *))

let ChangeDirectory (path:string) m =
    let tcConfig = TcConfig.Create(tcConfigB,validate=false)
    let path = tcConfig.MakePathAbsolute path 
    if Directory.Exists(path) then 
        tcConfigB.implicitIncludeDir <- path
    else
        error(Error(sprintf "Directory '%s' doesn't exist" path,m))

let rec execInteraction exitViaKillThread (tcConfig:TcConfig) istate (action:interaction) =
    istate |> InteractiveCatch (fun istate -> 
        // REVIEW: unify this processing of meta-commands with the two techniques used to process
        // the meta-commands from files.
        match action with 
        | IDefns ([  ],_) ->
            istate,Completed
        | IDefns ([  Def_expr(_,expr,m)],_) ->
            EvalExpr  istate expr,Completed           
        | IDefns (defs,m) -> 
            EvalDefns istate true defs,Completed
#if SUPPORT_USE
        | IHash (HashDirective("use",[sourceFile],m),_) ->
            MainThreadProcessInteractiveFile exitViaKillThread istate (sourceFile,m)
#endif
        | IHash (HashDirective("load",sourceFiles,m),_) -> 
            EvalLoadFiles istate m sourceFiles,Completed
        | IHash (HashDirective(("reference" | "r"),[path],m),_) -> 
            let dllinfos,istate = EvalRequireDll istate m path 
            dllinfos |> List.iter (fun dllinfo -> uprintnfnn "--> Referenced '%s'" dllinfo.FileName);
            istate,Completed
        | IHash (HashDirective("I",[path],m),_) -> 
            tcConfigB.AddIncludePath (m,path); 
            uprintnfnn "--> Added '%s' to library include path" (tcConfig.MakePathAbsolute path);
            istate,Completed
        | IHash (HashDirective("cd",[path],m),_) ->
            ChangeDirectory path m;
            istate,Completed
        | IHash (HashDirective("silentCd",[path],m),_) ->
            ChangeDirectory path m;
            promptSkipNext(); (* "silent" directive *)
            istate,Completed                  
        | IHash (HashDirective("time",[],m),_) -> 
            uprintnfnn "--> Timing now %s" (if istate.timing then "off" else "on");
            {istate with timing = not (istate.timing)},Completed
        | IHash (HashDirective("time",[("on" | "off") as v],m),_) -> 
            uprintnfnn "--> Timing now %s" (if v = "on" then "on" else "off");
            {istate with timing = (v = "on")},Completed
        | IHash (HashDirective("nowarn",[d],m),_) -> 
            tcConfigB.TurnWarningOff(m,d);
            istate,Completed
        | IHash (HashDirective("terms",[],m),_) -> 
            tcConfigB.showTerms <- not tcConfig.showTerms; istate,Completed
        | IHash (HashDirective("types",[],m),_) -> 
            showTypes := not (!showTypes); istate,Completed
#if DEBUG
        | IHash (HashDirective("ilcode",[],m),_) -> 
            showILCode := not (!showILCode); istate,Completed
        | IHash (HashDirective("info",[],m),_) -> 
            showInfo istate; istate,Completed         
#endif
        | IHash (HashDirective("savedll",[],m),_) -> 
            saveEmittedCode := true; istate,Completed
        | IHash (HashDirective("nosavedll",[],m),_) -> 
            saveEmittedCode := true; istate,Completed                 
        | IHash (HashDirective(("q" | "quit"),[],m),_) -> 
            if exitViaKillThread then 
                killThreadRequest := ExitRequest;
                Thread.Sleep(1000)
            exit 0;                
        | IHash (HashDirective("help",[],m),_) ->
            help();
            istate,Completed
        | IHash (HashDirective(c,arg,m),_) -> 
            uprintfn "Invalid directive '#%s %s'" c (String.concat " " arg);  // REVIEW: uprintnfnn - like other directives above
            istate,Completed  (* REVIEW: cont = CompletedWithReportedError *)
    )

and execInteractions exitViaKillThread tcConfig istate (action:interaction option) =
    // #directive comes through with other definitions as a Def_hash.
    // Split these out for individual processing.
    let action,nextAction = 
        match action with
        | None                                      -> None  ,None
        | Some (IHash (hash,m))                     -> action,None
        | Some (IDefns ([],m))                      -> None  ,None
        | Some (IDefns (Def_hash(hash,mh)::defs,m)) -> Some (IHash(hash,mh)),Some (IDefns(defs,m))
        | Some (IDefns (defs,m))                    -> let isDefHash = function Def_hash(_,_) -> true | _ -> false
                                                       let defsA = Seq.takeWhile (isDefHash >> not) defs |> Seq.to_list
                                                       let defsB = Seq.skipWhile (isDefHash >> not) defs |> Seq.to_list
                                                       Some (IDefns(defsA,m)),Some (IDefns(defsB,m))
    match action with
      | None -> assert(nextAction = None); istate,Completed
      | Some action ->
          let istate,cont = execInteraction exitViaKillThread tcConfig istate action
          match cont with
            | Completed                  -> execInteractions exitViaKillThread tcConfig istate nextAction
            | CompletedWithReportedError -> istate,CompletedWithReportedError  (* drop nextAction on error *)
            | EndOfFile                  -> istate,Completed                   (* drop nextAction on EOF *)
            | CtrlC                      -> istate,CtrlC                       (* drop nextAction on CtrlC *)

and MainThreadProcessParsedInteraction exitViaKillThread action istate = 
    let tcConfig = TcConfig.Create(tcConfigB,validate=false)
    try 
        if !progress then dprintf "In MainThreadProcessParsedInteraction...\n";                  
        ctrlcState := CtrlCCanRaiseException;
        let res = execInteractions exitViaKillThread tcConfig istate action
        killThreadRequest := NoRequest;
        ctrlcState := CtrlCIgnored;
        res
    with
    | :? ThreadAbortException ->
       killThreadRequest := NoRequest;
       ctrlcState := CtrlCIgnored;
       (try Thread.ResetAbort() with _ -> ());
       (istate,CtrlC)
    |  e ->
       killThreadRequest := NoRequest;
       ctrlcState := CtrlCIgnored;
       stopProcessingRecovery e range0;
       istate,CompletedWithReportedError

//----------------------------------------------------------------------------
// Parse then process one parsed interaction.  This initially runs on the parser
// thread, then calls runCodeOnMainThread to run on the GUI thread. 
// 'ProcessAndRunOneInteractionFromLexbuf' calls the runCodeOnMainThread when it has completed 
// parsing and needs to typecheck and execute a definition.  Type-checking and execution 
// happens on the GUI thread.
//----------------------------------------------------------------------------

and ProcessAndRunOneInteractionFromLexbuf exitViaKillThread runCodeOnMainThread istate (tokenizer:Lexfilter.LexFilter) =
    let tcConfig = TcConfig.Create(tcConfigB,validate=false)
    let lexbuf = tokenizer.lexbuf
    if lexbuf.IsPastEndOfStream then 
        istate,(if !stdinState = StdinEOFPermittedBecauseCtrlCRecentlyPressed then (stdinState := StdinNormal; CtrlC) 
                else EndOfFile)
    else 
        promptPrint();
        istate |> InteractiveCatch (fun istate -> 
            // BLOCKING POINT
            // When FSI.EXE is waiting for input from the console the 
            // parser thread is blocked somewhere deep this call. *)
            if !progress then dprintf "entering ParseInteraction...\n";
            let action  = ParseInteraction tokenizer
            if !progress then dprintf "returned from ParseInteraction...\n";
            // After we've unblocked and got something to run we switch 
            // over to the run-thread (e.g. the GUI thread) 
            if !progress then dprintf "calling runCodeOnMainThread...\n";
            let res = runCodeOnMainThread (MainThreadProcessParsedInteraction exitViaKillThread action) istate 
            if !progress then dprintf "Just called runCodeOnMainThread, res = %O...\n" res;
            res)
    
and MainThreadProcessInteractiveFile exitViaKillThread istate (sourceFile,m) =
    let tcConfig = TcConfig.Create(tcConfigB,validate=false)
    // Resolve the filename to an absolute filename
    let sourceFile = tcConfig.ResolveSourceFile(m,sourceFile) 
    // During the processing of the file, further filenames are 
    // resolved relative to the home directory of the loaded file.
    WithImplicitHome (dirname sourceFile)  (fun () ->
        // use source file containing maybe several ;-interaction blocks 
            let stream,reader,lexbuf = UnicodeLexing.UnicodeFileAsLexbuf(sourceFile,tcConfig.inputCodePage)  in
            use stream = stream in
            use reader = reader in
            Lexhelp.resetLexbufPos sourceFile lexbuf;
            let skip = true 
            let defines = "INTERACTIVE"::tcConfig.conditionalCompilationDefines
            let lexargs = mkLexargs ((fun () -> tcConfig.implicitIncludeDir),sourceFile,defines, interactiveInputLightSyntaxStatus, lexResourceManager, ref [], errorLogger) in 
            let tokenizer = Lexfilter.create false interactiveInputLightSyntaxStatus (Lexer.token lexargs skip) lexbuf
            let rec run istate =
                let istate,cont = ProcessAndRunOneInteractionFromLexbuf exitViaKillThread (fun f istate -> f istate) istate tokenizer
                if cont = Completed then run istate else istate,cont 
            let istate,cont = run istate 
            match cont with
            | Completed -> failwith "MainThreadProcessInteractiveFile: Completed expected to have relooped"
            | CompletedWithReportedError -> istate,CompletedWithReportedError
            | EndOfFile -> istate,Completed (* here file-EOF is normal, continue required *)
            | CtrlC     -> istate,CtrlC
      )


let rec evalInteractiveFiles istate exitViaKillThread rangeStdin sourceFiles =
  match sourceFiles with
    | [] -> istate
    | sourceFile :: sourceFiles ->
        // Catch errors on a per-file basis, so results/bindings from pre-error files can be kept.
        let istate,cont = InteractiveCatch (fun istate -> MainThreadProcessInteractiveFile exitViaKillThread istate (sourceFile,rangeStdin)) istate
        match cont with
          | Completed                  -> evalInteractiveFiles istate exitViaKillThread rangeStdin sourceFiles  
          | CompletedWithReportedError -> istate (* do not process any more files *)             
          | CtrlC                      -> istate (* do not process any more files *)
          | EndOfFile                  -> assert(false); istate (* This is unexpected. EndOfFile is replaced by Completed in the called function *)

//----------------------------------------------------------------------------
// GUI runCodeOnMainThread
//----------------------------------------------------------------------------

//type InteractionStateConverter = delegate of InteractionState -> InteractionState * stepStatus

///Use a dummy to access protected member
type DummyForm() = 
    inherit Form() 
    member x.DoCreateHandle() = x.CreateHandle() 

//----------------------------------------------------------------------------
// initial state and welcome
//----------------------------------------------------------------------------

let initialInteractiveState =
    let tcConfig = TcConfig.Create(tcConfigB,validate=false)
    let optEnv0 = InitialOptimizationEnv tcImports
    let emEnv = Ilreflect.emEnv0
    let tcEnv = GetInitialTypecheckerEnv None rangeStdin tcConfig tcImports tcGlobals 
    let ccuName = assemblyName 

    let tcState = TypecheckInitialState (rangeStdin,ccuName,tcConfig,tcGlobals,niceNameGen,tcEnv)

    let ilxgenEnv0 = IlxgenEnvInit(tcConfig,tcImports,tcGlobals,tcState.Ccu )
    {optEnv    = optEnv0;
     emEnv     = emEnv;
     tcGlobals = tcGlobals;
     tcState   = tcState;
     ilxGenEnv = ilxgenEnv0;
     timing    = false;
    } 


//----------------------------------------------------------------------------
// interactive state ref - most recent istate 
//----------------------------------------------------------------------------

let istateRef = ref initialInteractiveState
  
// Update the console completion function now we've got an initial type checking state.
// This means completion doesn't work until the initial type checking state has finished loading - fair enough!
begin
    match consoleOpt with 
    | Some console when !readline -> 
        console.SetCompletion(fun (s1,s2) -> 
                                CompletionsForPartialLID !istateRef 
                                  (match s1 with 
                                   | Some s -> s + "." + s2 
                                   | None -> s2) 
                                |> Seq.of_list)
    | _ -> 
      ()
end

//----------------------------------------------------------------------------
// Reading stdin 
//----------------------------------------------------------------------------

let LexbufFromLineReader readf : UnicodeLexing.Lexbuf = 
    UnicodeLexing.FunctionAsLexbuf 
      (fun (buf: char[], start, len) -> 
        //dprintf "Calling ReadLine\n";
        let inputOption = try Some(readf()) with :? EndOfStreamException -> None
        inputOption |> Option.iter (fun t -> syphon.Add (t + "\n"));
        match inputOption with 
        |  Some(null) | None -> 
             if !progress then dprintf "End of file from TextReader.ReadLine\n";
             0
        | Some (input:string) ->
            let input  = input + "\n" 
            let ninput = input.Length 
            if ninput > len then eprintf "Warning: line too long, ignoring some characters\n";
            let ntrimmed = min len ninput 
            for i = 0 to ntrimmed-1 do
                buf.[i+start] <- input.[i]
            ntrimmed
    )

     
let TrySetUnhandledExceptionMode() =  
    let i = ref 0 // stop inlining 
    try 
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException) 
      incr i;incr i;incr i;incr i;incr i;incr i;
    with _ -> 
      decr i;decr i;decr i;decr i;()

//----------------------------------------------------------------------------
// Reading stdin as a lex stream
//----------------------------------------------------------------------------

let removeZeroCharsFromString (str:string) = (* bug://4466 *)
    if str<>null && str.Contains("\000") then
      System.String(str |> Seq.filter (fun c -> c<>'\000') |> Seq.to_array)
    else
      str

let MkStdinLexer () =
    let lexbuf = 
        match consoleOpt with 
        | Some console when !readline && not (IsInteractiveServer()) -> 
            LexbufFromLineReader (fun () -> match !consoleFirstLine with Some l -> (consoleFirstLine := None; l) | None -> console.ReadLine()) 
        | _ -> 
            LexbufFromLineReader (fun () -> System.Console.In.ReadLine() |> removeZeroCharsFromString)
            //lexbufFromTextReader Encoding.UTF8 System.Console.In
    Lexhelp.resetLexbufPos stdinMockFilename lexbuf;
    syphon.Reset();
    let lexargs = mkLexargs ((fun () -> tcConfigB.implicitIncludeDir),stdinMockFilename,"INTERACTIVE"::tcConfigB.conditionalCompilationDefines,interactiveInputLightSyntaxStatus,lexResourceManager, ref[], errorLogger) in 
    let skip = true  (* don't report whitespace from lexer *)
    (* A single hardWhite tokenizer must be shared for the entire *)
    (* use of this lexbuf. *)
    let tokenizer = Lexfilter.create false interactiveInputLightSyntaxStatus (Lexer.token lexargs skip) lexbuf
    tokenizer

//do Console.Out.Encoding <- Encoding.UTF8
//do Console.Error.Encoding <- Encoding.UTF8

//----------------------------------------------------------------------------
// main()
//----------------------------------------------------------------------------
 
let main () = 
  if !progress then dprintf "fsi : main()\n";
  let initial exitViaKillThread istate = 
      let istate = 
          let rec consume istate sourceFiles =
              match sourceFiles with
              | [] -> istate
              | (_,fsx1) :: _ -> 
                  let sourceFiles,rest = List.takeUntil (fun (_,fsx2) -> fsx1 <> fsx2) sourceFiles 
                  let sourceFiles = List.map fst sourceFiles 
                  let istate = 
                      if fsx1 
                      then evalInteractiveFiles istate exitViaKillThread rangeStdin sourceFiles
                      else istate |> InteractiveCatch (fun istate -> EvalLoadFiles istate rangeStdin sourceFiles, Completed) |> fst in 
                  consume istate rest in
           consume istate sourceFiles
      if nonNil(sourceFiles) then promptPrintAhead(); (* Seems required. I expected this could be deleted. Why not? *)
      istate 

  let istate = initialInteractiveState
  if !interact then 
      // page in the type check env 
      let istate = istate |> InteractiveCatch (fun istate ->  EvalDefns istate true [],Completed) |> fst
      if !progress then dprintf "MAIN: installed CtrlC handler!\n";
      let exitViaKillThread = InstallCtrlCHandler(mainThread,(if !gui then 400 else 100)) 
      if !progress then dprintf "MAIN: got initial state, creating form\n";
      if !gui then 
          do (try Application.EnableVisualStyles() with _ -> ())
          Application.add_ThreadException(new ThreadExceptionEventHandler(fun _ args -> fsi.ReportThreadException(args.Exception)));
          if not runningOnMono then (try TrySetUnhandledExceptionMode() with _ -> ());

          // This is the event loop for winforms
          let evLoop = 
              let mainForm = new DummyForm() 
              mainForm.DoCreateHandle();
              // Set the default thread exception handler
              fsi.ThreadException.Add(fun exn -> RunCodeOnWinFormsMainThread mainForm (fun () -> PrintError(tcConfigB,syphon,true,exn)));
              let restart = ref false
              { new Microsoft.FSharp.Compiler.Interactive.IEventLoop with
                   member x.Run() =  
                       restart := false;
                       if !progress then dprintf "MAIN: Calling Application.Run...\n";
                       Application.Run()
                       if !progress then dprintf "MAIN: Returned from Application.Run...\n";
                       !restart
                   member x.Invoke(f) : 'a =   RunCodeOnWinFormsMainThread mainForm f  

                   member x.ScheduleRestart()  =   restart := true; Application.Exit() } 
          fsi.EventLoop <- evLoop;
                                      
      let istate = initial exitViaKillThread istate
      if !progress then dprintf "creating stdinReaderThread\n";
      let tokenizer = ref (MkStdinLexer())

      let stdinReaderThread = 
        istateRef := istate;
        let cont = ref Completed 
        new Thread(new ThreadStart(fun () ->
            try 
               try 
                  if !progress then dprintf "READER: stdin thread started...\n";

                  // Delay until we've peeked the input or read the entire first line
                  WaitHandle.WaitAll([| (consoleReaderStartupDone :> WaitHandle) |]) |> ignore;
                  
                  if !progress then dprintf "READER: stdin thread got first line...\n";

                  // The main stdin loop, running on the stdinReaderThread.
                  // 
                  // The function 'ProcessAndRunOneInteractionFromLexbuf' is blocking: it reads stdin 
                  // until one or more real chunks of input have been received. 
                  //
                  // We run the actual computations for each action on the main GUI thread by using
                  // mainForm.Invoke to pipe a message back through the form's main event loop. (The message 
                  // is a delegate to execute on the main Thread)
                  //
                  while (!cont = CompletedWithReportedError or !cont = Completed or !cont = CtrlC) do
                      if (!cont = CtrlC) then 
                          tokenizer := MkStdinLexer();
                      let istate',cont' = 
                          let runCodeOnMainThread f istate = 
                              try fsi.EventLoop.Invoke (fun () -> f istate) 
                              with _ -> (istate,Completed)
                              
                          ProcessAndRunOneInteractionFromLexbuf exitViaKillThread runCodeOnMainThread !istateRef !tokenizer   
                      istateRef := istate'; 
                      cont := cont';
                      if !progress then dprintf "READER: cont = %O\n" !cont;
                  done ;
                  if !progress then dprintf "\n- READER: Exiting stdinReaderThread\n";  
                with e -> stopProcessingRecovery e range0;
            finally 
                (if !progress then dprintf "\n- READER: Exiting process because of failure/exit on  stdinReaderThread\n";  
                 exit 1)
        ),Name="StdinReaderThread")
      // stdinReaderThread.IsBackground <- true; 
      if !progress then dprintf "MAIN: starting stdin thread...\n";
      stdinReaderThread.Start();


      let rec runLoop() = 
          if !progress then dprintf "GUI thread runLoop\n";
          let restart = 
              try 
                // BLOCKING POINT: The GUI Thread spends most (all) of its time this event loop
                if !progress then dprintf "MAIN:  entering event loop...\n";
                fsi.EventLoop.Run()
              with
              |  :? ThreadAbortException ->
                // If this TAE handler kicks it's almost certainly too late to save the
                // state of the process - the state of the message loop may have been corrupted 
                uprintnfn "- Unexpected ThreadAbortException (Ctrl-C) during event handling: Trying to restart...";  
                (try Thread.ResetAbort() with _ -> ());
                true
                // Try again, just case we can restart
              | e -> 
                stopProcessingRecovery e range0;
                true
                // Try again, just case we can restart
          if !progress then dprintf "MAIN:  exited event loop...\n";
          if restart then runLoop() 

      runLoop();
      ()
  else // not interact
      let istate = initial false istate
      exit (min errorLogger.ErrorCount 1)

//----------------------------------------------------------------------------
// Server mode:
//----------------------------------------------------------------------------

let SpawnThread name f =
    let th = new Thread(new ThreadStart(f),Name=name)
    th.IsBackground <- true;
    th.Start()

let SpawnInteractiveServer() =   
    //Printf.printf "Spawning fsi server on channel '%s'" !fsiServerName;
    SpawnThread "ServerThread" (fun () ->
             let server =
                 {new Server.Shared.FSharpInteractiveServer() with
                    member this.Interrupt() = //printf "FSI-SERVER: received CTRL-C request...\n";
                        try !ctrlEventActions |> List.iter (fun act -> act())
                        with e -> assert(false); ()    (* final sanity check! - catch all exns - but not expected *)
                    member this.Completions(prefix) = 
                        try CompletionsForPartialLID !istateRef prefix  |> List.to_array
                        with e -> assert(false); [| |] (* final sanity check! - catch all exns - but not expected*)
                    member this.GetDeclarations(text,names) = 
                        try tcLock (fun () -> FsiGetDeclarations !istateRef text names)
                        with e -> assert(false); [| |] (* final sanity check! - catch all exns - but not expected *)
                 }

             Server.Shared.FSharpInteractiveServer.StartServer(!fsiServerName,server))
  
do if not runningOnMono && IsInteractiveServer() then SpawnInteractiveServer() 
   
//----------------------------------------------------------------------------
// STAThread
// Mark the main thread as STAThread since it is a GUI thread
//----------------------------------------------------------------------------

[<STAThread()>]    
do
#if DEBUG  
    try
#endif    
      main()
#if DEBUG    
    with e -> printf "Exception by fsi.exe:\n%+A\n" e
#endif

//----------------------------------------------------------------------------
// Misc
// The Ctrl-C exception handler that we've passed to native code has
// to be explicitly kept alive.
//----------------------------------------------------------------------------


[<CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")>]
do GC.KeepAlive(ctrlEventHandlers)

