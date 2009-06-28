(* (c) Microsoft Corporation. All rights reserved  *)

/// Internal use only.  Erase closures
module Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Pubclo

open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX 

module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 
module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types

open Il
open Ilx

val ConvModule: ILGlobals -> ILModuleDef -> ILModuleDef (* string is name of module fragment if converting multiple module fragments *)

type cenv
val typ_Func1 : cenv -> ILType -> ILType -> ILType
val typ_TyFunc : cenv -> ILGenericParameterDef -> ILType -> ILType
val new_cenv : ILGlobals -> cenv
val typ_of_lambdas: cenv -> IlxClosureLambdas -> ILType
