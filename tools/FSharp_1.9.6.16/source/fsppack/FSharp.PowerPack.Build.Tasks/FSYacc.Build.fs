// (c) Microsoft Corporation 2005-2009.

namespace Microsoft.FSharp.Build

open System
open Microsoft.Build.Framework
open Microsoft.Build.Utilities
open Internal.Utilities

(**************************************
MSBuild task for fsyacc

options:
        -o <string>: Name the output file.
        -v: Produce a listing file.
        --module <string>: Define the F# module name to host the generated parser.
        --open <string>: Add the given module to the list of those to open in both the generated signature and implementation.
        --ml-compatibility: Support the use of the global state from the 'Parsing' module in MLLib.
        --tokens: Simply tokenize the specification file itself.
        --codepage <int>: Assume input lexer specification file is encoded with the given codepage.
        --help: display this list of options
        -help: display this list of options

NOT SUPPORTED:
    -v        
    --tokens
    --open
    --module

**************************************)

type FsYacc() = 
    inherit ToolTask()

    let mutable inputFile  : string = null
    let mutable outputFile : string = null
    
    let mutable codePage   : string = null
    let mutable otherFlags : string = null
    let mutable mlCompat   = false
    
    let mutable _open  : string = null
    let mutable _module  : string = null

    let mutable toolPath : string = 
        match FSharpEnvironment.FSharpRunningBinFolder with
        | Some(s) -> s
        | None -> ""
#if FX_ATLEAST_35
#else
    let mutable toolExe : string = "fsyacc.exe"
#endif

    // [<Required>]
    member this.InputFile
        with get ()  = inputFile
        and  set (x) = inputFile <- x
    
    [<Microsoft.Build.Framework.Output>]
    member this.OutputFile
        with get ()  = outputFile
        and  set (x) = outputFile <- x
    
    member this.OtherFlags
        with get() = otherFlags
        and set(s) = otherFlags <- s

    // --codepage <int>: Assume input lexer specification file is encoded with the given codepage.
    member this.CodePage
        with get ()  = codePage
        and  set (x) = codePage <- x
    
    // --ml-compatibility: Support the use of the global state from the 'Parsing' module in MLLib.
    member this.MLCompatibility
        with get ()  = mlCompat
        and  set (x) = mlCompat <- x
        
    // --open
    member this.Open
        with get ()  = _open
        and  set (x) = _open <- x       

   // --module
    member this.Module
        with get ()  = _module
        and  set (x) = _module <- x             

    // For targeting other versions of fslex.exe, such as "\LKG\" or "\Prototype\"
    member this.ToolPath
        with get ()  = toolPath
        and  set (s) = toolPath <- s
        
#if FX_ATLEAST_35
#else
    // Name of the .exe to call
    member this.ToolExe
        with get ()  = toolExe
        and  set (s) = toolExe <- s        
#endif

    // ToolTask methods
    override this.ToolName = "fsyacc.exe"
    
    override this.GenerateFullPathToTool() = 
        System.IO.Path.Combine(toolPath, this.ToolExe)
        
    override this.GenerateCommandLineCommands() =
    
        let builder = new CommandLineBuilder()
        
        // CodePage
        builder.AppendSwitchIfNotNull("--codepage ", codePage)
        
        // ML Compatibility
        if mlCompat then builder.AppendSwitch("--ml-compatibility")

        // Open
        builder.AppendSwitchIfNotNull("--open ", _open)

        // Module
        builder.AppendSwitchIfNotNull("--module ", _module)

        // OutputFile
        builder.AppendSwitchIfNotNull("-o ", outputFile)

        // OtherFlags - must be second-to-last
        builder.AppendSwitchUnquotedIfNotNull("", otherFlags)

        builder.AppendSwitchIfNotNull(" ", inputFile)
        
        let args = builder.ToString()

        // when doing simple unit tests using API, no BuildEnginer/Logger is attached
        if this.BuildEngine <> null then
            let eventArgs = { new CustomBuildEventArgs(message=args,helpKeyword="",senderName="") with member x.Equals(y) = false }
            this.BuildEngine.LogCustomEvent(eventArgs)
        
        args
    
    // Expose this to internal components (for unit testing)
    member internal this.InternalGenerateCommandLineCommands() =
        this.GenerateCommandLineCommands()
