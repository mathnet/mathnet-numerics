#light

/// This namespace contains FSharp.PowerPack extensions for FSharp.Build.dll. MSBuild tasks for the FsYacc and FsLex tools.
namespace Microsoft.FSharp.Build
type FscCustomBuildEventArgs = class
                                 inherit Microsoft.Build.Framework.CustomBuildEventArgs
                                 new : commandLine:string -> FscCustomBuildEventArgs
                                 member CommandLine : string
                               end
    
type Fsc = class
             inherit Microsoft.Build.Utilities.ToolTask
             new : unit -> Fsc
             override GenerateCommandLineCommands : unit -> System.String
             override GenerateFullPathToTool : unit -> System.String
             override ToolName : System.String
             override StandardErrorEncoding : System.Text.Encoding
             override StandardOutputEncoding : System.Text.Encoding

             member internal InternalGenerateCommandLineCommands : unit -> System.String
             member BaseAddress : string with get,set
             member CodePage : string with get,set
             member DebugSymbols : bool with get,set
             member DebugType : string with get,set
             member DefineConstants : Microsoft.Build.Framework.ITaskItem [] with get,set
             member DisabledWarnings : string [] with get,set
             member DocumentationFile : string with get,set
             member GenerateInterfaceFile : string with get,set
             member KeyFile : string with get,set
             member NoFramework : bool with get,set
             member NoWarn : string with get,set
             member Optimize : bool with get,set
             member Tailcalls : bool with get,set
             member OtherFlags : string with get,set
             member OutputAssembly : string with get,set
             member PdbFile : string with get,set
             member Platform : string with get,set
             member VersionFile : string with get,set
             member References : Microsoft.Build.Framework.ITaskItem [] with get,set
             member ReferencePath : string with get,set
             member Resources : Microsoft.Build.Framework.ITaskItem [] with get,set
             member Sources : Microsoft.Build.Framework.ITaskItem [] with get,set
             member TargetType : string with get,set
#if FX_ATLEAST_35
#else
             member ToolExe : string with get,set
#endif             
             member ToolPath : string with get,set
             member TreatWarningsAsErrors : bool with get,set
             member Utf8Output : bool with get,set
             member VisualStudioStyleErrors : bool with get,set
             member WarningLevel : string with get,set
             member WarningsAsErrors : string with get,set
             member Win32ResourceFile : string with get,set
             member Win32ManifestFile : string with get,set
           end
