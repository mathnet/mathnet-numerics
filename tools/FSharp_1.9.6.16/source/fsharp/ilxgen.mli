// (c) Microsoft Corporation. All rights reserved

module (* internal *) Microsoft.FSharp.Compiler.Ilxgen

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open System
open System.IO  
open System.Reflection
open Microsoft.FSharp.Compiler 

val generatePublicAsInternal : bool ref

[<StructuralEquality(false); StructuralComparison(false)>]
type internal cenv = 
 { g: Env.TcGlobals;
   viewCcu: Tast.ccu;
   fragName: string;
   generateFilterBlocks : bool;
   workAroundReflectionEmitBugs: bool;
   emitConstantArraysUsingStaticDataBlobs: bool;
   amap: Import.ImportMap;
   mainMethodInfo: Tast.Attribs option;
   localOptimizationsAreOn: bool;
   debug: bool;
   emptyProgramOk: bool; }

type public  ilxGenEnv 
val internal GetEmptyIlxGenEnv : Tast.ccu -> ilxGenEnv 
val public AddExternalCcusToIlxGenEnv : Env.TcGlobals -> ilxGenEnv -> Tast.ccu list -> ilxGenEnv 
val public AddIncrementalLocalAssmblyFragmentToIlxGenEnv : bool -> Env.TcGlobals -> Tast.ccu -> string -> ilxGenEnv -> Tast.TypedAssembly -> ilxGenEnv 

type public CodegenResults = 
    { ilTypeDefs             : ILTypeDef list;
      ilAssemAttrs           : ILAttribute list;
      ilNetModuleAttrs       : ILAttribute list;
      quotationResourceBytes : byte[]  list }

val internal GenerateCode : 
   cenv 
   -> ilxGenEnv 
   -> Tast.TypedAssembly 
   -> Tast.Attribs * Tast.Attribs 
   -> CodegenResults


val report : TextWriter -> unit
  
val lookupGeneratedValue : ((ILFieldRef -> FieldInfo) *
                            (ILMethodRef -> MethodInfo) *
                            (ILTypeRef -> Type) *
                            (ILType -> Type)) -> Env.TcGlobals -> ilxGenEnv -> Tast.Val -> (obj * System.Type) option

val lookupGeneratedInfo  : ((ILFieldRef -> FieldInfo) *
                            (ILMethodRef -> MethodInfo) *
                            (ILTypeRef -> Type) *
                            (ILType -> Type)) -> Env.TcGlobals -> ilxGenEnv -> Tast.Val -> System.Reflection.MemberInfo option
