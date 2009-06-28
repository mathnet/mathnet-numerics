// (c) Microsoft Corporation. All rights reserved

/// A set of "IL rewrites" ("morphs").  These map each sub-construct
/// of particular ILTypeDefs.  The morphing functions are passed
/// some details about the context in which the item being
/// morphed occurs, e.g. the module being morphed itself, the
/// ILTypeDef (possibly nested) where the item occurs, 
/// the ILMethodDef (if any) where the item occurs. etc.
module Microsoft.FSharp.Compiler.AbstractIL.Morphs 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types 
open Microsoft.FSharp.Compiler.AbstractIL.IL 

type 'a morph = 'a -> 'a

/// Morph each scope reference inside a type signature 
val tref_scoref2scoref: ILScopeRef morph -> ILTypeRef -> ILTypeRef 

val mdefs_mdef2mdef: ILMethodDef morph -> ILMethodDefs -> ILMethodDefs
/// nb. does not do nested tdefs
val tdefs_tdef2tdef: ILTypeDef morph -> ILTypeDefs -> ILTypeDefs 

val tdefs_tdef2tdefs: (ILTypeDef -> ILTypeDef list) -> ILTypeDefs -> ILTypeDefs

/// Morph all tables of ILTypeDefs in "ILModuleDef"
val module_tdefs2tdefs: ILTypeDefs morph -> ILModuleDef -> ILModuleDef

/// Morph all type references throughout an entire module.
val module_tref2tref_memoized:  ILTypeRef morph ->  ILModuleDef ->  ILModuleDef

val module_scoref2scoref_memoized:  ILScopeRef morph ->  ILModuleDef ->  ILModuleDef

/// Morph all type definitions throughout an entire module, including nested type 
/// definitions.
val module_tdef2tdef: (ILTypeDef list -> ILTypeDef morph) -> ILModuleDef -> ILModuleDef

val mbody_ilmbody2ilmbody: ILMethodBody morph -> LazyMethodBody -> LazyMethodBody
val cloinfo_ilmbody2ilmbody: ILMethodBody morph -> IlxClosureInfo ->  IlxClosureInfo
val topcode_instr2instrs: (ILInstr -> ILInstr list) -> ILCode -> ILCode
val topcode_instr2code: (ILCodeLabel -> ILCodeLabel -> ILInstr -> Choice<ILInstr list, ILCode>) -> ILCode -> ILCode
