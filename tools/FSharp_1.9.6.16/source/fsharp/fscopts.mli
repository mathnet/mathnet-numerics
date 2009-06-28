// (c) Microsoft Corporation. All rights reserved

#light

module (* internal *) Microsoft.FSharp.Compiler.Fscopts

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Build
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Ilxgen
open Microsoft.FSharp.Compiler.Import
open Microsoft.FSharp.Compiler.Opt
open Microsoft.FSharp.Compiler.Env

val DisplayBannerText : TcConfigBuilder -> unit

//val GetCompilerOptions : TcConfigBuilder -> CompilerOption list -> CompilerOption list
val GetCoreFscCompilerOptions     : TcConfigBuilder -> CompilerOptionBlock list
val GetCoreFsiCompilerOptions     : TcConfigBuilder -> CompilerOptionBlock list
val GetCoreServiceCompilerOptions : TcConfigBuilder -> CompilerOptionBlock list

// Expose the "setters" for some user switches, to enable setting of defaults
val SetOptimizeSwitch : TcConfigBuilder -> OptionSwitch -> unit
val SetTailcallSwitch : OptionSwitch -> unit
val SetDebugSwitch    : TcConfigBuilder -> string option -> OptionSwitch -> unit
val PrintOptionInfo   : TcConfigBuilder -> unit

val fsharpModuleName : target -> string -> string

val ParseOneInputFile : TcConfig * Lexhelp.LexResourceManager * string list * string * canContainEntryPoint: bool * ErrorLogger -> input option

val InitialOptimizationEnv : TcImports -> IncrementalOptimizationEnv
val AddExternalCcuToOpimizationEnv : IncrementalOptimizationEnv -> ImportedAssembly -> IncrementalOptimizationEnv
val ApplyAllOptimizations : TcConfig * TcGlobals * string * ImportMap * bool * IncrementalOptimizationEnv * ccu * TypedAssembly -> TypedAssembly * Opt.LazyModuleInfo * IncrementalOptimizationEnv

val IlxgenEnvInit : TcConfig * TcImports * TcGlobals * ccu -> ilxGenEnv
val GenerateIlxCode : bool * bool * TcGlobals * TcConfig * ImportMap * TypeChecker.topAttribs * TypedAssembly * ccu * string * ilxGenEnv -> CodegenResults

// Used during static linking
val NormalizeAssemblyRefs : TcImports -> (AbstractIL.IL.ILScopeRef -> AbstractIL.IL.ILScopeRef)

// Miscellany
val ignoreFailureOnMono1_1_16 : (unit -> unit) -> unit
val mutable enableConsoleColoring : bool
val DoWithErrorColor : bool -> (unit -> 'a) -> 'a
val ReportTime : TcConfig -> string -> unit
val abbrevFlagSet : TcConfigBuilder -> bool -> string Set
val PostProcessCompilerArgs : string Set -> string [] -> string list
