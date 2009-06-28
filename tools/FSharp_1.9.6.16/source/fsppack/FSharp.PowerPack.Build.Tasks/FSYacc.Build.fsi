// (c) Microsoft Corporation 2005-2009.

namespace Microsoft.FSharp.Build

type FsYacc =
    inherit Microsoft.Build.Utilities.ToolTask

    new : unit -> FsYacc
    
    override GenerateCommandLineCommands : unit -> System.String
    override GenerateFullPathToTool : unit -> System.String
    override ToolName : System.String
    
    member internal InternalGenerateCommandLineCommands : unit -> System.String
    
    member InputFile  : string with set
    [<Microsoft.Build.Framework.Output>]
    member OutputFile : string with set
    member CodePage   : string with set
    member OtherFlags : string with set
    member MLCompatibility : bool with set
    member Open : string with set
    member Module   : string with set
    member ToolPath   : string with set
#if FX_ATLEAST_35
#else
    member ToolExe   : string with set    
#endif