// (c) Microsoft Corporation 2005-2009.

namespace Microsoft.FSharp.Build

open System
open Microsoft.Build.Framework
open Microsoft.Build.Utilities
open Internal.Utilities

(**************************************
MSBuild task for fslex

options:
        -o <string>: Name the output file.
        --codepage <int>: Assume input lexer specification file is encoded with the given codepage.
        --unicode: Produce a lexer for use with 16-bit unicode characters.
        --help: display this list of options
        -help: display this list of options
**************************************)

type FsLex() = 
    inherit ToolTask()

    let mutable inputFile  : string = null
    let mutable outputFile : string = null
    
    let mutable codePage   : string = null
    let mutable unicode   = false
    let mutable otherFlags   = ""

    let mutable toolPath : string = 
        match FSharpEnvironment.FSharpRunningBinFolder with
        | Some(s) -> s
        | None -> ""
#if FX_ATLEAST_35   
#else 
    let mutable toolExe : string = "fslex.exe"
#endif 

    // [<Required>]
    member this.InputFile
        with get ()  = inputFile
        and  set (x) = inputFile <- x
    
    [<Output>]
    member this.OutputFile
        with get ()  = outputFile
        and  set (x) = outputFile <- x
    
    // --codepage <int>: Assume input lexer specification file is encoded with the given codepage.
    member this.CodePage
        with get ()  = codePage
        and  set (x) = codePage <- x
    
    // --unicode: Produce a lexer for use with 16-bit unicode characters.
    member this.Unicode
        with get ()  = unicode
        and  set (x) = unicode <- x

    member this.OtherFlags
        with get() = otherFlags
        and set(s) = otherFlags <- s

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
    override this.ToolName = "fslex.exe"
    
    override this.GenerateFullPathToTool() = 
        System.IO.Path.Combine(toolPath, this.ToolExe)
        
    override this.GenerateCommandLineCommands() =
    
        let builder = new CommandLineBuilder()
        
        // CodePage
        builder.AppendSwitchIfNotNull("--codepage ", codePage)
        
        // Unicode
        if unicode then builder.AppendSwitch("--unicode")

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
