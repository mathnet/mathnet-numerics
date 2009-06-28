// (c) Microsoft Corporation. All rights reserved

#light

module (* internal *) Microsoft.FSharp.Compiler.Import

open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.AbstractIL.IL

type AssemblyLoader = 
    | AssemblyLoader of (range * ILAssemblyRef -> CcuResolutionResult)
    member LoadAssembly : range * ILAssemblyRef -> CcuResolutionResult


[<SealedAttribute ()>]
type ImportMap =
    new : g:Env.TcGlobals * assemMap:AssemblyLoader -> ImportMap
    member assemMap : AssemblyLoader
    member g : Env.TcGlobals

val internal ImportILTypeRef : ImportMap -> range -> ILTypeRef -> TyconRef
val internal ImportILType : ImportMap -> range -> typ list -> ILType -> typ
val internal ImportIlTypars : (unit -> ImportMap) -> range -> ILScopeRef -> typ list -> ILGenericParameterDef list -> Typar list
val internal ImportIlAssembly : (unit -> ImportMap) * range * (ILScopeRef -> ILModuleDef) * ILScopeRef * sourceDir:string * filename: string option * ILModuleDef -> CcuThunk
