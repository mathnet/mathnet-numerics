// (c) Microsoft Corporation. All rights reserved

#light

module (* internal *) Microsoft.FSharp.Compiler.TypeChecker

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Infos

[<Sealed>]
type tcEnv =
    member DisplayEnv : DisplayEnv

(* Incremental construction of environments, e.g. for F# Interactive *)
val internal CreateInitialTcEnv        : Env.TcGlobals * Import.ImportMap * range * (ccu * string list * bool) list -> tcEnv 
val internal AddCcuToTcEnv             : Env.TcGlobals * Import.ImportMap * range * tcEnv * ccu * autoOpens: string list * bool -> tcEnv 
val internal AddLocalTopRootedModuleOrNamespace : Env.TcGlobals -> Import.ImportMap -> range -> tcEnv -> ModuleOrNamespaceType -> tcEnv
val internal TcOpenDecl         : Env.TcGlobals -> Import.ImportMap -> range -> range -> tcEnv -> Ast.LongIdent -> tcEnv 

type topAttribs =
    { mainMethodAttrs : Attribs;
      netModuleAttrs  : Attribs;
      assemblyAttrs   : Attribs  }

type conditionalDefines = 
    string list

val internal EmptyTopAttrs : topAttribs
val internal CombineTopAttrs : topAttribs -> topAttribs -> topAttribs

val internal TypecheckOneImplFile : 
      Env.TcGlobals * NiceNameGenerator * Import.ImportMap * ccu * (unit -> bool) * conditionalDefines
      -> tcEnv 
      -> Tast.ModuleOrNamespaceType option
      -> implFile
      -> Eventually<topAttribs * Tast.TypedImplFile * tcEnv>

val internal TypecheckOneSigFile : 
      Env.TcGlobals * NiceNameGenerator * Import.ImportMap * ccu  * (unit -> bool) * conditionalDefines
      -> tcEnv                             
      -> sigFile
      -> Eventually<tcEnv * tcEnv * ModuleOrNamespaceType >

(*-------------------------------------------------------------------------
 * exceptions arising from type checking 
 *------------------------------------------------------------------------- *)

exception internal BakedInMemberConstraintName of string * range
exception internal FunctionExpected of DisplayEnv * typ * range
exception internal NotAFunction of DisplayEnv * typ * range * range
exception internal Recursion of DisplayEnv * Ast.ident * typ * typ * range
exception internal RecursiveUseCheckedAtRuntime of DisplayEnv * ValRef * range
exception internal LetRecEvaluatedOutOfOrder of DisplayEnv * ValRef * ValRef * range
exception internal LetRecCheckedAtRuntime of range
exception internal LetRecUnsound of DisplayEnv * ValRef list * range
exception internal TyconBadArgs of DisplayEnv * TyconRef * int * range
exception internal UnionCaseWrongArguments of DisplayEnv * int * int * range
exception internal UnionCaseWrongNumberOfArgs of DisplayEnv * int * int * range
exception internal FieldsFromDifferentTypes of DisplayEnv * RecdFieldRef * RecdFieldRef * range
exception internal FieldGivenTwice of DisplayEnv * RecdFieldRef * range
exception internal MissingFields of string list * range
exception internal UnitTypeExpected of DisplayEnv * Tast.typ * bool * range
exception internal FunctionValueUnexpected of DisplayEnv * Tast.typ * range
exception internal UnionPatternsBindDifferentNames of range
exception internal VarBoundTwice of Ast.ident
exception internal ValueRestriction of DisplayEnv * bool * Val * Typar * range
exception internal FieldNotMutable of DisplayEnv * RecdFieldRef * range
exception internal ValNotMutable of DisplayEnv * ValRef * range
exception internal ValNotLocal of DisplayEnv * ValRef * range
exception internal InvalidRuntimeCoercion of DisplayEnv * typ * typ * range
exception internal IndeterminateRuntimeCoercion of DisplayEnv * typ * typ * range
exception internal IndeterminateStaticCoercion of DisplayEnv * typ * typ * range
exception internal StaticCoercionShouldUseBox of DisplayEnv * typ * typ * range
exception internal RuntimeCoercionSourceSealed of DisplayEnv * typ * range
exception internal CoercionTargetSealed of DisplayEnv * typ * range
exception internal UpcastUnnecessary of range
exception internal TypeTestUnnecessary of range
exception internal SelfRefObjCtor of bool * range
exception internal VirtualAugmentationOnNullValuedType of range
exception internal NonVirtualAugmentationOnNullValuedType of range
exception internal UseOfAddressOfOperator of range
exception internal DeprecatedThreadStaticBindingWarning of range
exception internal NotUpperCaseConstructor of range
exception internal IntfImplInIntrinsicAugmentation of range
exception internal IntfImplInExtrinsicAugmentation of range
exception internal OverrideInIntrinsicAugmentation of range
exception internal OverrideInExtrinsicAugmentation of range
exception internal NonUniqueInferredAbstractSlot of Env.TcGlobals * DisplayEnv * string * MethInfo * MethInfo * range
exception internal IndexOutOfRangeExceptionWarning of range
exception internal StandardOperatorRedefinitionWarning of string * range
exception internal ParameterlessStructCtor of range

val internal TcFieldInit : range -> ILFieldInit -> Tast.Constant
val public nenv_of_tenv : tcEnv -> Nameres.NameResolutionEnv
val public items_of_tenv : tcEnv -> Nameres.NamedItem NameMap

