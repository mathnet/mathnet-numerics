// (c) Microsoft Corporation. All rights reserved

#light

module (* internal *) Microsoft.FSharp.Compiler.Nameres

open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Infos
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Import
open Microsoft.FSharp.Compiler.Outcome
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.PrettyNaming

/// A NameResolver primarily holds an InfoReader
type NameResolver =
    new : g:TcGlobals * amap:ImportMap * infoReader:InfoReader * instantiationGenerator:(range -> typars -> tinst) -> NameResolver
    member InfoReader : InfoReader
    member amap : ImportMap
    member g : TcGlobals
    member instantiationGenerator : (range -> typars -> tinst)

//---------------------------------------------------------------------------
// 
//------------------------------------------------------------------------- 

[<StructuralEquality(false); StructuralComparison(false)>]
type NamedItem = 
  // These exist in the "eUnqualifiedItems" List.map in the type environment. 
  | Item_val of  ValRef
  | Item_ucase of UnionCaseInfo
  | Item_apres of ActivePatternInfo * typ * int 
  | Item_apelem of ActivePatternElemRef 
  | Item_ecref of TyconRef 
  | Item_recdfield of RecdFieldInfo

  // The following are never in the items table but are valid results of binding 
  // an identitifer in different circumstances. 
  | Item_newdef of ident
  | Item_il_field of ILFieldInfo
  | Item_event of EventInfo
  | Item_property of string * PropInfo list
  | Item_meth_group of string * MethInfo list
  | Item_ctor_group of string * MethInfo list
  | Item_fake_intf_ctor of Tast.typ
  | Item_delegate_ctor of Tast.typ
  | Item_typs of string * Tast.typ list
  | Item_modrefs of Tast.ModuleOrNamespaceRef list
  /// Represents the resolution of a source identifier to an implicit use of an infix operator
  | Item_implicit_op of ident
  /// Represents the resolution of a source identifier to a named argument
  | Item_param_name of ident 
  | Item_prop_name of ident

type ExtensionMember = 
   | FSExtMem of ValRef
   | ILExtMem of ILTypeRef * ILMethodDef

[<StructuralEqualityAttribute (false); StructuralComparisonAttribute (false)>]
type NameResolutionEnv =
    {eDisplayEnv: DisplayEnv;
     eUnqualifiedItems: NamedItem NameMap;
     ePatItems: NamedItem NameMap;
     eModulesAndNamespaces: ModuleOrNamespaceRef list NameMap;
     eFullyQualifiedModulesAndNamespaces: ModuleOrNamespaceRef list NameMap;
     eFieldLabels: (RecdFieldRef * bool) NameMultiMap;
     eTyconsByAccessNames: TyconRef NameMultiMap;
     eTyconsByDemangledNameAndArity: Map<NameArityPair,TyconRef>;
     eExtensionMembers: TcrefMultiMap<ExtensionMember>;
     eTypars: Typar NameMap;}
    static member Empty : g:TcGlobals -> NameResolutionEnv
    member DisplayEnv : DisplayEnv
    member UnqualifiedItems : NamedItem NameMap


val public DisplayNameOfItem : TcGlobals -> NamedItem -> string

val internal AddFakeNamedValRefToNameEnv : string -> ValRef -> NameResolutionEnv -> NameResolutionEnv
val internal AddFakeNameToNameEnv : string -> NamedItem -> NameResolutionEnv -> NameResolutionEnv

val internal AddValRefToNameEnv                    : ValRef -> NameResolutionEnv -> NameResolutionEnv
val internal AddActivePatternResultTagsToNameEnv   : ActivePatternInfo -> typ -> NameResolutionEnv -> NameResolutionEnv
val internal AddTyconRefsToNameEnv                 : TcGlobals -> ImportMap -> range -> TyconRef list -> NameResolutionEnv -> NameResolutionEnv
val internal AddExceptionDeclsToNameEnv            : TyconRef -> NameResolutionEnv -> NameResolutionEnv
val internal AddModuleAbbrevToNameEnv              : ident -> ModuleOrNamespaceRef list -> NameResolutionEnv -> NameResolutionEnv
val internal AddModrefsToNameEnv                   : TcGlobals -> ImportMap -> range -> bool -> AccessorDomain -> Map<string,TyconRef> -> NameResolutionEnv -> NameResolutionEnv
val internal AddModrefToNameEnv                    : TcGlobals -> ImportMap -> range -> bool -> AccessorDomain -> ModuleOrNamespaceRef -> NameResolutionEnv -> NameResolutionEnv
val internal AddModuleOrNamespaceContentsToNameEnv : TcGlobals -> ImportMap -> AccessorDomain -> range -> ModuleOrNamespaceRef -> NameResolutionEnv -> NameResolutionEnv

type CheckForDuplicateTyparFlag =
  | CheckForDuplicateTypars
  | NoCheckForDuplicateTypars

val internal AddDeclaredTyparsToNameEnv : CheckForDuplicateTyparFlag -> Typar list -> NameResolutionEnv -> NameResolutionEnv
val internal OneResult : 'a outcome -> 'a list outcome
val internal AtMostOneResult : range -> 'a list outcome -> 'a outcome
val internal LookupTypeNameInEnvNoArity : string -> NameResolutionEnv -> TyconRef list

type TypeNameInExprOrPatFlag =
  | ResolveTypeNamesToCtors
  | ResolveTypeNamesToTypeRefs

type TypeNameResInfo = TypeNameInExprOrPatFlag * int option

val internal DefaultTypeNameResInfo : TypeNameInExprOrPatFlag * 'a option

type internal ItemOccurence = 
  | Binding = 0
  | Use = 1
  | Pattern = 2
  
type ITypecheckResultsSink =
    abstract NotifyEnvWithScope   : range * NameResolutionEnv * AccessorDomain -> unit
    abstract NotifyExprHasType    : pos * typ * DisplayEnv * NameResolutionEnv * AccessorDomain * range -> unit
    abstract NotifyNameResolution : pos * NamedItem * ItemOccurence * DisplayEnv * NameResolutionEnv * AccessorDomain * range -> unit

val internal GlobalTypecheckResultsSink : ITypecheckResultsSink option ref 
val internal CallEnvSink                : range * NameResolutionEnv * AccessorDomain -> unit
val internal CallNameResolutionSink     : range * NameResolutionEnv * NamedItem * ItemOccurence * DisplayEnv * AccessorDomain -> unit
val internal CallExprHasTypeSink        : range * NameResolutionEnv * Tast.typ * DisplayEnv * AccessorDomain -> unit

val internal AllPropInfosOfTypeInScope : InfoReader -> TcrefMultiMap<ExtensionMember> -> string option * AccessorDomain -> FindMemberFlag -> range -> typ -> PropInfo list
val internal AllMethInfosOfTypeInScope : InfoReader -> TcrefMultiMap<ExtensionMember> -> string option * AccessorDomain -> FindMemberFlag -> range -> typ -> MethInfo list

exception internal IndeterminateType of range
exception internal UpperCaseIdentifierInPattern of range
exception internal DeprecatedClassFieldInference of range

val FreshenRecdFieldRef :NameResolver -> Range.range -> Tast.RecdFieldRef -> NamedItem

type FullyQualifiedFlag =
  | FullyQualified
  | OpenQualified

type LookupKind =
  | RecdField
  | Pattern
  | Expr
  | Type
  | Ctor


type WarnOnUpperFlag =
  | WarnOnUpperCase
  | AllIdsOK

val internal ResolveLongIndentAsModuleOrNamespace   : FullyQualifiedFlag -> NameResolutionEnv -> AccessorDomain -> ident list -> (int * ModuleOrNamespaceRef * ModuleOrNamespaceType) list outcome
val internal ResolveObjectConstructor               : NameResolver -> DisplayEnv -> range -> AccessorDomain -> typ -> (NamedItem * 'a list) outcome
val internal ResolveLongIdentInType                 : NameResolver -> NameResolutionEnv -> LookupKind -> range -> AccessorDomain -> ident list -> FindMemberFlag -> TypeNameInExprOrPatFlag * int option -> typ -> NamedItem * ident list
val internal ResolvePatternLongIdent                : NameResolver -> WarnOnUpperFlag -> bool -> range -> AccessorDomain -> NameResolutionEnv -> TypeNameInExprOrPatFlag * int option -> ident list -> NamedItem
val internal ResolveTypeLongIdentInType             : NameResolver -> NameResolutionEnv -> TypeNameInExprOrPatFlag * int option -> AccessorDomain -> range -> ModuleOrNamespaceRef -> ident list -> TyconRef 
val internal ResolveTypeLongIdent                   : NameResolver -> ItemOccurence -> FullyQualifiedFlag -> NameResolutionEnv -> AccessorDomain -> ident list -> int -> TyconRef outcome
val internal ResolveField                           : NameResolver -> NameResolutionEnv -> AccessorDomain -> typ -> ident list * ident -> (RecdFieldRef * bool) list
val internal ResolveExprLongIdent                   : NameResolver -> range -> AccessorDomain -> NameResolutionEnv -> TypeNameInExprOrPatFlag * int option -> ident list -> NamedItem * ident list
val internal ResolveLongIdentAsExprAndComputeRange  : NameResolver -> range -> AccessorDomain -> NameResolutionEnv -> TypeNameInExprOrPatFlag * int option -> ident list -> NamedItem * range * ident list
val internal ResolveExprDotLongIdentAndComputeRange : NameResolver -> range -> AccessorDomain -> NameResolutionEnv -> typ -> ident list -> FindMemberFlag -> NamedItem * range * ident list

val FakeInstantiationGenerator : range -> Typar list -> typ list
val ResolvePartialLongIdent : NameResolver -> NameResolutionEnv -> range -> AccessorDomain -> string list -> bool -> NamedItem list
val ResolveCompletionsInType       : NameResolver -> NameResolutionEnv -> Range.range -> Infos.AccessorDomain -> bool -> Tast.typ -> NamedItem list
