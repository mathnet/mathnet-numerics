
// (c) Microsoft Corporation. All rights reserved

#light

/// Code to pickle out quotations in the quotation binary format.
module (* internal *) Microsoft.FSharp.Compiler.Sreflect

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.Lib
open Bytes

type TypeData 
type TypeVarData =  { tvName: string }
type NamedTypeData = { tcName: string; tcAssembly:  string }

val mkVarTy : int -> TypeData 
val mkFunTy : (TypeData * TypeData) -> TypeData
val mkArrayTy : (int * TypeData ) -> TypeData 
val mkNamedTy : (NamedTypeData * TypeData list) -> TypeData 

type ExprData

type VarData 

type CtorData = 
    { ctorParent: NamedTypeData; 
      ctorArgTypes: TypeData list; }

type MethodData = 
    { methParent: NamedTypeData;
      methName: string;
      methArgTypes: TypeData list;
      methRetType: TypeData; 
      numGenericArgs: int }

type ModuleDefnData = 
    { Module: NamedTypeData;
      Name: string;
      IsProperty: bool }

type MethodBaseData = 
    | ModuleDefn of ModuleDefnData
    | Method     of MethodData
    | Ctor       of CtorData

type FieldData     = NamedTypeData * string
type RecdFieldData = NamedTypeData * string
type PropInfoData  = NamedTypeData * string * TypeData * TypeData list

val mkVar    : int -> ExprData 
val mkHole   : TypeData * int -> ExprData 
val mkApp    : ExprData * ExprData -> ExprData 
val mkLambda : VarData * ExprData -> ExprData 
val mkQuote  : ExprData -> ExprData 
val mkCond   : ExprData * ExprData * ExprData -> ExprData 
val mkModuleValueApp : NamedTypeData * string * bool * TypeData list * ExprData list list -> ExprData 
val mkLetRec : (VarData * ExprData) list * ExprData -> ExprData 
val mkLet : (VarData * ExprData) * ExprData -> ExprData
val mkRecdMk : NamedTypeData  * TypeData list * ExprData list -> ExprData
val mkRecdGet : RecdFieldData   * TypeData list * ExprData list -> ExprData 
val mkRecdSet :  RecdFieldData * TypeData list * ExprData list -> ExprData 
val mkSum : (NamedTypeData * string) * TypeData list * ExprData list -> ExprData 
val mkSumFieldGet : (NamedTypeData * string * int) * TypeData list * ExprData -> ExprData  
val mkSumTagTest : (NamedTypeData * string)   * TypeData list * ExprData -> ExprData  
val mkTuple : TypeData * ExprData list -> ExprData 
val mkTupleGet : TypeData * int * ExprData -> ExprData
val mkCoerce : TypeData * ExprData -> ExprData 
val mkNewArray : TypeData * ExprData list -> ExprData 
val mkTypeTest : TypeData * ExprData -> ExprData 
val mkAddressSet : ExprData * ExprData -> ExprData 
val mkUnit : unit -> ExprData 
val mkNull : TypeData -> ExprData 
val mkBool : bool -> ExprData 
val mkString : string -> ExprData 
val mkSingle : float32 -> ExprData 
val mkDouble : float -> ExprData 
val mkChar : char -> ExprData 
val mkSByte : sbyte -> ExprData 
val mkByte : byte -> ExprData 
val mkInt16 : int16 -> ExprData 
val mkUInt16 : uint16 -> ExprData 
val mkInt32 : int32 -> ExprData 
val mkUInt32 : uint32 -> ExprData 
val mkInt64 : int64 -> ExprData 
val mkUInt64 : uint64 -> ExprData 
val mkAddressOf : ExprData -> ExprData
val mkSequential : ExprData * ExprData -> ExprData 
val mkForLoop : ExprData * ExprData * ExprData -> ExprData 
val mkWhileLoop : ExprData * ExprData -> ExprData 
val mkTryFinally : ExprData * ExprData -> ExprData 
val mkTryWith : ExprData * VarData * ExprData * VarData * ExprData -> ExprData 
val mkDelegate : TypeData * ExprData -> ExprData 
val mkPropGet : PropInfoData   * TypeData list * ExprData list -> ExprData   
val mkPropSet : PropInfoData   * TypeData list * ExprData list -> ExprData   
val mkFieldGet : FieldData   * TypeData list * ExprData list -> ExprData  
val mkFieldSet : FieldData   * TypeData list * ExprData list -> ExprData  
val mkCtorCall : CtorData * TypeData list * ExprData list -> ExprData 
val mkMethodCall : MethodData * TypeData list * ExprData list -> ExprData 
val mkAttributedExpression : ExprData * ExprData -> ExprData 
val pickle : (ExprData -> byte[]) 

    
val PickleDefns : ((MethodBaseData * ExprData) list -> byte[]) 
val pickledDefinitionsResourceNameBase : string
val freshVar : string * TypeData -> VarData

