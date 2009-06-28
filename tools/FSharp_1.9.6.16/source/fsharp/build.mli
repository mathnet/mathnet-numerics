
// (c) Microsoft Corporation. All rights reserved 
#light

/// Loading initial context, reporting errors etc.
module (* internal *) Microsoft.FSharp.Compiler.Build

open System.Text
open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 
open Microsoft.FSharp.Compiler 

module Tc = Microsoft.FSharp.Compiler.TypeChecker

open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Infos
open Microsoft.FSharp.Compiler.MSBuildResolver
open Tc
open Microsoft.FSharp.Compiler.Env

#if DEBUG
val internal showAssertForUnexpectedException : bool ref
#endif

/// Signature file suffixes
val public sigSuffixes : string list

/// Implementation file suffixes
val public implSuffixes : string list

/// Script file suffixes
val public scriptSuffixes : string list

val public IsScript : string -> bool

val public IsInvalidPath : string -> bool

/// File suffixes where #light is the default
val internal lightSyntaxDefaultExtensions : string list

//----------------------------------------------------------------------------
// Parsing inputs
//--------------------------------------------------------------------------
  
val public QualFileNameOfUniquePath : range * string list -> Ast.QualifiedNameOfFile

val public PrependPathToInput : Ast.ident list -> Ast.input -> Ast.input

val internal ParseInput : (UnicodeLexing.Lexbuf -> Parser.token) * ErrorLogger * UnicodeLexing.Lexbuf * string option * string * canContainEntryPoint: bool -> Ast.input

//----------------------------------------------------------------------------
// Errors
//--------------------------------------------------------------------------

type ErrorStyle = 
    | DefaultErrors 
    | EmacsErrors 
    | TestErrors 
    | VSErrors
    

val internal RangeOfError : exn -> range option
val internal SplitRelatedErrors : exn -> exn * exn list
val public OutputException : StringBuilder -> exn -> bool -> unit
val public SanitizeFileName : filename:string -> implicitIncludeDir:string -> string
val public OutputErrorOrWarning : implicitIncludeDir:string * showFullPaths: bool * flattenErrors: bool * errorStyle: ErrorStyle *  warning:bool -> StringBuilder -> exn -> unit
val public OutputErrorOrWarningContext : prefix:string -> fileLineFunction:(string -> int -> string) -> StringBuilder -> exn -> unit


//----------------------------------------------------------------------------
// Options and configuration
//--------------------------------------------------------------------------

// For command-line options that can be suffixed with +/-
type public OptionSwitch =
    | On
    | Off

/// The spec value describes the action of the argument,
/// and whether it expects a following parameter.
type OptionSpec = 
    | OptionClear of bool ref
    | OptionFloat of (float -> unit)
    | OptionInt of (int -> unit)
    | OptionSwitch of (OptionSwitch -> unit)
    | OptionIntList of (int -> unit)
    | OptionIntListSwitch of (int -> OptionSwitch -> unit)
    | OptionRest of (string -> unit)
    | OptionSet of bool ref
    | OptionString of (string -> unit)
    | OptionStringList of (string -> unit)
    | OptionStringListSwitch of (string -> OptionSwitch -> unit)
    | OptionUnit of (unit -> unit)
    | OptionHelp of (CompilerOptionBlock list -> unit)                      // like OptionUnit, but given the "options"
    | OptionGeneral of (string list -> bool) * (string list -> string list) // Applies? * (ApplyReturningResidualArgs)
and  CompilerOption      = CompilerOption of string * string * OptionSpec * Option<exn> * string list
and  CompilerOptionBlock = PublicOptions  of string * CompilerOption list | PrivateOptions of CompilerOption list

val printCompilerOptionBlocks : CompilerOptionBlock list -> unit  // for printing usage
val dumpCompilerOptionBlocks  : CompilerOptionBlock list -> unit  // for QA
val filterCompilerOptionBlock : (CompilerOption -> bool) -> CompilerOptionBlock -> CompilerOptionBlock

exception public AssemblyNotResolved of (*originalName*) string * range
exception public FileNameNotResolved of (*filename*) string * (*description of searched locations*) string * range
exception public DeprecatedCommandLineOption of string * string * range
exception HashLoadedSourceHasIssues of (*warnings*) exn list * (*errors*) exn list * range
exception HashLoadedScriptConsideredSource of range  

type AssemblyReference = 
    | AssemblyReference of range * string 
    member Range : range
    member Text : string

type AssemblyResolution = {
    /// The original reference to the assembly.
    originalReference : AssemblyReference
    /// Path to the resolvedFile
    resolvedPath : string    
    /// Search path used to find this spot.
    resolvedFrom : ResolvedFrom
    /// Long fusion name of the assembly
    fusionName : string 
    /// Version of the assembly like 4.0.0.0
    fusionVersion : string 
    /// Name of the redist, if any, that the assembly was found in.
    redist : string 
    /// Whether or not this is an installed system assembly (for example, System.dll)
    sysdir : bool
    }
    
type target = 
    | WinExe 
    | ConsoleExe 
    | Dll 
    | Module
    member IsExe : bool
    
type ResolveLibFileMode = 
    | Speculative 
    | ReportErrors

type VersionFlag = 
    | VersionString of string
    | VersionFile of string
    | VersionNone
    member GetVersionInfo : (*implicitIncludeDir:*)string -> ILVersionInfo
    member GetVersionString : (*implicitIncludeDir:*)string -> string
         
     
type public TcConfigBuilder =
    { mutable mscorlibAssemblyName: string;
      mutable autoResolveOpenDirectivesToDlls: bool;
      mutable noFeedback: bool;
      mutable implicitIncludeDir: string;
      mutable openBinariesInMemory: bool;
      mutable openDebugInformationForLaterStaticLinking: bool;
      defaultFSharpBinariesDir: string;
      mutable compilingFslib: bool;
      mutable useIncrementalBuilder: bool;
      mutable includes: string list;
      mutable implicitOpens: string list;
      mutable useFsiAuxLib: bool;
      mutable framework: bool;
      mutable resolutionEnvironment : ResolutionEnvironment
      mutable implicitlyResolveAssemblies : bool
      /// Set if the user has explicitly turned indentation-aware syntax on/off
      mutable light: bool option;
      mutable conditionalCompilationDefines: string list;
      /// Sources added into the build with #load
      mutable loadedSources: (range * string) list;
      
      mutable referencedDLLs: AssemblyReference  list;
      optimizeForMemory: bool;
      mutable inputCodePage: int option;
      mutable embedResources : string list;
      mutable globalWarnAsError: bool;
      mutable globalWarnLevel: int;
      mutable specificWarnOff: int list; 
      mutable specificWarnAsError: int list 
      mutable mlCompatibility:bool;
      mutable checkOverflow:bool;
      mutable showReferenceResolutions:bool;
      mutable outputFile : string option;
      mutable resolutionFrameworkRegistryBase : string;
      mutable resolutionAssemblyFoldersSuffix : string; 
      mutable resolutionAssemblyFoldersConditions : string;          
      mutable platform : ILPlatform option
      mutable useMonoResolution : bool
      mutable target : target
      mutable debuginfo : bool
      mutable debugSymbolFile : string option
      mutable typeCheckOnly : bool
      mutable parseOnly : bool
      mutable simulateException : string option
      mutable printAst : bool
      mutable tokenizeOnly : bool
      mutable testInteractionParser : bool
      mutable reportNumDecls : bool
      mutable printSignature : bool
      mutable printSignatureFile : string
      mutable xmlDocOutputFile : string option
      mutable generateHtmlDocs : bool
      mutable htmlDocDirectory : string option
      mutable htmlDocCssFile : string option
      mutable htmlDocNamespaceFile : string option
      mutable htmlDocAppendFlag : bool
      mutable htmlDocLocalLinks : bool  
      mutable stats : bool
      mutable generateFilterBlocks : bool 
      mutable signer : string option
      mutable container : string option
      mutable delaysign : bool
      mutable version : VersionFlag 
      mutable standalone : bool
      mutable extraStaticLinkRoots : string list 
      mutable noSignatureData : bool
      mutable onlyEssentialOptimizationData : bool
      mutable useOptimizationDataFile : bool
      mutable jitTracking : bool
      mutable ignoreSymbolStoreSequencePoints : bool
      mutable internConstantStrings : bool
      mutable generateConfigFile : bool
      mutable extraOptimizationIterations : int
      mutable win32res : string 
      mutable win32manifest : string
      mutable includewin32manifest : bool
      mutable linkResources : string list
      mutable showFullPaths : bool
      mutable errorStyle : ErrorStyle
      mutable utf8output : bool
      mutable flatErrors : bool
      mutable maxErrors : int
      mutable abortOnError : bool
      mutable baseAddress : int32 option
 #if DEBUG
      mutable writeGeneratedILFiles : bool (* write il files? *)  
      mutable showOptimizationData : bool
#endif
      mutable showTerms     : bool 
      mutable writeTermsToFiles : bool 
      mutable doDetuple     : bool 
      mutable doTLR         : bool 
      mutable optsOn        : bool 
      mutable optSettings   : Opt.OptimizationSettings 
      mutable product : string
      mutable showBanner  : bool
      mutable showTimes : bool
      mutable pause : bool }
    static member CreateNew : string * bool * string  -> TcConfigBuilder
    member DecideNames : string list -> (*outfile*)string * (*pdbfile*)string option * (*assemblyName*)string 
    member TurnWarningOff : range * string -> unit
    member AddIncludePath : range * string -> unit
    member AddReferencedAssemblyByPath : range * string -> unit
    member AddLoadedSource : range * string -> unit
    member AddEmbeddedResource : string -> unit
    
    static member SplitCommandLineResourceInfo : string -> string * string * ILResourceAccess


    
[<Sealed>]
// Immutable TcConfig
type public TcConfig =
    member mscorlibAssemblyName: string;
    member autoResolveOpenDirectivesToDlls: bool;
    member noFeedback: bool;
    member implicitIncludeDir: string;
    member openBinariesInMemory: bool;
    member openDebugInformationForLaterStaticLinking: bool;
    member fsharpBinariesDir: string;
    member compilingFslib: bool;
    member useIncrementalBuilder: bool;
    member includes: string list;
    member implicitOpens: string list;
    member useFsiAuxLib: bool;
    member framework: bool;
    member implicitlyResolveAssemblies : bool
    /// Set if the user has explicitly turned indentation-aware syntax on/off
    member light: bool option;
    member conditionalCompilationDefines: string list;
    /// Sources added into the build with #load
    member loadedSources: (range * string) list;
    
    member referencedDLLs: AssemblyReference list;
    member optimizeForMemory: bool;
    member inputCodePage: int option;
    member embedResources : string list;
    member globalWarnAsError: bool;
    member globalWarnLevel: int;
    member specificWarnOff: int list; 
    member specificWarnAsError: int list 
    member mlCompatibility:bool;
    member checkOverflow:bool;
    member showReferenceResolutions:bool;
    member outputFile : string option;
    member resolutionFrameworkRegistryBase : string;
    member resolutionAssemblyFoldersSuffix : string; 
    member resolutionAssemblyFoldersConditions : string;          
    member platform : ILPlatform option
    member useMonoResolution : bool
    member target : target
    member debuginfo : bool
    member debugSymbolFile : string option
    member typeCheckOnly : bool
    member parseOnly : bool
    member simulateException : string option
    member printAst : bool
    member tokenizeOnly : bool
    member testInteractionParser : bool
    member reportNumDecls : bool
    member printSignature : bool
    member printSignatureFile : string
    member xmlDocOutputFile : string option
    member generateHtmlDocs : bool
    member htmlDocDirectory : string option
    member htmlDocCssFile : string option
    member htmlDocNamespaceFile : string option
    member htmlDocAppendFlag : bool
    member htmlDocLocalLinks : bool  
    member stats : bool
    member generateFilterBlocks : bool 
    member signer : string option
    member container : string option
    member delaysign : bool
    member version : VersionFlag 
    member standalone : bool
    member extraStaticLinkRoots : string list 
    member noSignatureData : bool
    member onlyEssentialOptimizationData : bool
    member useOptimizationDataFile : bool
    member jitTracking : bool
    member ignoreSymbolStoreSequencePoints : bool
    member internConstantStrings : bool
    member generateConfigFile : bool
    member extraOptimizationIterations : int
    member win32res : string 
    member win32manifest : string
    member includewin32manifest : bool
    member linkResources : string list
    member showFullPaths : bool
    member errorStyle : ErrorStyle
    member utf8output : bool
    member flatErrors : bool

    member maxErrors : int
    member baseAddress : int32 option
#if DEBUG
    member writeGeneratedILFiles : bool (* write il files? *)  
    member showOptimizationData : bool
#endif
    member showTerms     : bool 
    member writeTermsToFiles : bool 
    member doDetuple     : bool 
    member doTLR         : bool 
    member optSettings   : Opt.OptimizationSettings 
    member optsOn        : bool 
    member product : string
    member showBanner  : bool
    member showTimes : bool
    member pause : bool 


    member ComputeLightSyntaxInitialStatus : string -> bool
    member ComputeSyntaxFlagRequired : string -> bool
    member ClrRoot : string list
    
    /// Get the loaded sources that exist and issue a warning for the ones that don't
    member GetAvailableLoadedSources : unit -> (range*string) list
    
    member ComputeCanContainEntryPoint : sourceFiles:string list -> bool list 

    /// File system query based on TcConfig settings
    member ResolveSourceFile : range * string -> string
    /// File system query based on TcConfig settings
    member MakePathAbsolute : string -> string
    static member Create : TcConfigBuilder * validate: bool -> TcConfig
    


//----------------------------------------------------------------------------
// Tables of referenced DLLs 
//--------------------------------------------------------------------------

type public ImportedBinary = 
    { FileName: string;
      IsFSharpBinary: bool;
      RawMetadata: ILModuleDef;
      ILAssemblyRefs : ILAssemblyRef list;
      ILScopeRef: ILScopeRef ;}

type public ImportedAssembly = 
    { ILScopeRef: ILScopeRef;
      FSharpViewOfMetadata: Tast.ccu;
      AssemblyAutoOpenAttributes: string list;
      AssemblyInternalsVisibleToAttributes: string list;
      FSharpOptimizationData : Lazy<Option<Opt.LazyModuleInfo>> }

type UnresolvedReference = UnresolvedReference of string * AssemblyReference list

[<Sealed>] 
type TcAssemblyResolutions = 
    member GetAssemblyResolutions : unit -> AssemblyResolution list

    static member SplitNonFoundationalResolutions  : TcConfig -> AssemblyResolution list * AssemblyResolution list * UnresolvedReference list
    static member BuildFromPriorResolutions     : TcConfig * AssemblyResolution list -> TcAssemblyResolutions 
    

[<Sealed>]
type TcConfigProvider = 
    static member Constant : TcConfig -> TcConfigProvider
    static member BasedOnMutableBuilder : TcConfigBuilder -> TcConfigProvider

[<Sealed>] 
type TcImports =
    interface System.IDisposable
    //new : TcImports option -> TcImports
    member SetBase : TcImports -> unit
    member GetCcuInfos : unit -> ImportedAssembly list
    member GetCcusInDeclOrder : unit -> ccu list
    member FindDllInfo : range * string -> ImportedBinary
    member FindCcuFromAssemblyRef : range * ILAssemblyRef -> Tast.CcuResolutionResult
    member AssemblyLoader : Import.AssemblyLoader 
    member GetImportMap : unit -> Import.ImportMap

    /// File system query based on TcConfig settings
    member TryResolveLibFile : AssemblyReference * ResolveLibFileMode -> OperationResult<AssemblyResolution>
    /// File system query based on TcConfig settings
    member ResolveLibFile : AssemblyReference * ResolveLibFileMode -> AssemblyResolution

    static member BuildFrameworkTcImports      : TcConfigProvider * AssemblyResolution list -> TcGlobals * TcImports
    static member BuildNonFrameworkTcImports   : TcConfigProvider * TcGlobals * TcImports * AssemblyResolution list -> TcImports
    static member BuildTcImports               : TcConfigProvider -> TcGlobals * TcImports

//----------------------------------------------------------------------------
// Special resources in DLLs
//--------------------------------------------------------------------------

val public  IsSignatureDataResource : ILResource -> bool
val public IsOptDataResource : ILResource -> bool
val public IsReflectedDefinitionsResource : ILResource -> bool

val public WriteSignatureData : TcConfig * TcGlobals * Tastops.Remap * ccu * string -> ILResource
val public WriteOptData :  TcGlobals -> string -> Tast.ccu * Opt.LazyModuleInfo -> ILResource

//----------------------------------------------------------------------------
// 
//--------------------------------------------------------------------------


val public GetNameOfScopeRef : ILScopeRef -> string
val public GetNameOfILModule : ILModuleDef -> string

val public GetFSharpCoreLibraryName : unit -> string
val public GetFSharpPowerPackLibraryName : unit -> string

//----------------------------------------------------------------------------
// Finding and requiring DLLs
//--------------------------------------------------------------------------

val public RequireDLL : TcImports -> tcEnv -> range -> string -> tcEnv * (ImportedBinary list * ImportedAssembly list)

//----------------------------------------------------------------------------
// Processing # commands
//--------------------------------------------------------------------------

val public ProcessMetaCommandsFromInput : 
              ('a -> range * string -> 'a) * 
              ('a -> range * string -> 'a) * 
              ('a -> range * string -> unit) -> TcConfigBuilder -> Ast.input -> 'a -> 'a


val public GetScopedPragmasForInput : Ast.input -> ScopedPragma list
val public GetErrorLoggerFilteringByScopedPragmas : checkFile:bool * ScopedPragma list * ErrorLogger  -> ErrorLogger

val public ApplyMetaCommandsFromInputToTcConfig : TcConfig -> Ast.input -> TcConfig
val public GetResolvedAssemblyInformation : TcConfig -> AssemblyResolution list

//----------------------------------------------------------------------------
// Loading the default library sets
//--------------------------------------------------------------------------
                
val public coreFramework : string list
val public extendedFramework : string list
val public scriptingFramework : string list

//----------------------------------------------------------------------------
// Type checking and querying the type checking state
//--------------------------------------------------------------------------

val public GetInitialTypecheckerEnv : string option -> range -> TcConfig -> TcImports -> TcGlobals -> tcEnv
                
type topRootedSigs = (QualifiedNameOfFile, Tast.ModuleOrNamespaceType) Zmap.t

[<Sealed>]
type tcState =
    member NiceNameGenerator : Ast.NiceNameGenerator
    member Ccu : Tast.ccu
    member TcEnvFromSignatures : tcEnv
    member NextStateAfterIncrementalFragment : tcEnv -> tcState
    member TcEnvFromImpls : tcEnv

val public TypecheckInitialState : 
    range * string * TcConfig * TcGlobals * Ast.NiceNameGenerator * tcEnv -> tcState

val public TypecheckOneInputEventually :
    (unit -> bool) -> TcConfig -> TcImports -> TcGlobals 
      -> Ast.LongIdent option -> tcState -> Ast.input  
           -> Eventually<(tcEnv * topAttribs * Tast.TypedImplFile list) * tcState>

val public TypecheckMultipleInputsFinish :
    (tcEnv * topAttribs * 'a list) list * tcState
        -> (tcEnv * topAttribs * 'a list) * tcState
    
val public TypecheckMultipleInputs :
    (unit -> bool) * TcConfig * TcImports * TcGlobals * Ast.LongIdent option * tcState * Ast.input list 
        -> (tcEnv * topAttribs * Tast.TypedImplFile list) * tcState
  
val public TypecheckClosedInputSetFinish :
    TypedImplFile list * tcState 
        -> tcState * TypedAssembly

val public TypecheckClosedInputSet :
    (unit -> bool) * TcConfig * TcImports * TcGlobals * Ast.LongIdent option * tcState * Ast.input  list 
        -> tcState * topAttribs * Tast.TypedAssembly * tcEnv


val public ParseCompilerOptions : (string -> unit) -> CompilerOptionBlock list -> string list -> unit
val public ReportWarning : int -> int list -> exn -> bool
val public ReportWarningAsError : int -> int list -> int list -> bool -> exn -> bool

val public highestInstalledNetFrameworkVersionMajorMinor : unit -> int * string
