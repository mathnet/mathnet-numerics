// (c) Microsoft Corporation. All rights reserved

#light

module  Microsoft.FSharp.Compiler.Opt

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 

type OptimizationSettings = 
    { abstractBigTargets : bool
      jitOptUser : bool option
      localOptUser : bool option
      crossModuleOptUser : bool option
      bigTargetSize : int
      veryBigExprSize : int 
      lambdaInlineThreshold : int
      reportingPhase : bool;
      reportNoNeedToTailcall: bool
      reportFunctionSizes : bool
      reportHasEffect : bool
      reportTotalSizes : bool }

    member jitOpt : unit -> bool 
    member localOpt : unit -> bool 
    static member Defaults : OptimizationSettings

/// Optimization information 
type ModuleInfo
type LazyModuleInfo = ModuleInfo Lazy.t

type IncrementalOptimizationEnv
val internal empty_env : IncrementalOptimizationEnv

/// For building optimization environments incrementally 
val internal BindCcu : Tast.ccu -> LazyModuleInfo -> IncrementalOptimizationEnv -> IncrementalOptimizationEnv

/// The entry point. Boolean indicates 'incremental extension' in FSI 
val internal OptimizeImplFile : OptimizationSettings *  Tast.ccu (* scope *) * Env.TcGlobals * Import.ImportMap * IncrementalOptimizationEnv * bool * Tast.TypedImplFile -> IncrementalOptimizationEnv * Tast.TypedImplFile * LazyModuleInfo

/// Displaying optimization data
val internal moduleInfoL : LazyModuleInfo -> Layout.layout

/// Saving and re-reading optimization information 
val p_lazy_modul_info : LazyModuleInfo -> Pickle.WriterState -> unit 
val internal u_lazy_modul_info : Pickle.ReaderState -> LazyModuleInfo

/// Rewrite the modul info using the export remapping 
val RemapLazyModulInfo : Env.TcGlobals -> Tastops.Remap -> (LazyModuleInfo -> LazyModuleInfo)
val AbstractLazyModulInfoToEssentials : (LazyModuleInfo -> LazyModuleInfo)
val UnionModuleInfos: LazyModuleInfo list -> LazyModuleInfo
