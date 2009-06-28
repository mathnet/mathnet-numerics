// (c) Microsoft Corporation. All rights reserved

module (* internal *) Microsoft.FSharp.Compiler.ConstraintSolver

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Import
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Infos


val internal new_compgen_inference_var : TyparKind * TyparRigidity * Ast.TyparStaticReq * TyparDynamicReq * bool -> Typar
val internal new_anon_inference_var : TyparKind * range * TyparRigidity * TyparStaticReq * TyparDynamicReq -> Typar
val internal new_inference_measurevar : unit -> Typar
val internal new_error_tyvar : unit -> Typar
val internal new_error_measurevar : unit -> Typar
val internal new_inference_typ : unit -> typ
val internal new_error_typ : unit -> typ
val internal new_error_measure : unit -> measure
val internal new_inference_typs : 'a list -> typ list

val internal freshen_and_fixup_typars : range -> TyparRigidity -> typars -> typ list -> typars -> typars * TyparInst * typ list
val internal new_tinst : range -> typars -> typars * TyparInst * typ list
val internal freshen_tps : range -> typars -> typ list
val internal FreshenMethInfo : range -> MethInfo -> typ list

exception internal ConstraintSolverTupleDiffLengths              of DisplayEnv * typ list * typ list * range * range
exception internal ConstraintSolverInfiniteTypes                 of DisplayEnv * typ * typ * range * range
exception internal ConstraintSolverTypesNotInEqualityRelation    of DisplayEnv * typ * typ * range * range
exception internal ConstraintSolverTypesNotInSubsumptionRelation of DisplayEnv * typ * typ * range * range
exception internal ConstraintSolverMissingConstraint             of DisplayEnv * Typar * TyparConstraint * range * range
exception internal ConstraintSolverError                         of string * range * range
exception internal ConstraintSolverRelatedInformation            of string option * range * exn
exception internal ErrorFromApplyingDefault                      of TcGlobals * DisplayEnv * Typar * typ * error * range
exception internal ErrorFromAddingTypeEquation                   of TcGlobals * DisplayEnv * typ * typ * error * range
exception internal ErrorsFromAddingSubsumptionConstraint         of TcGlobals * DisplayEnv * typ * typ * error * range
exception internal ErrorFromAddingConstraint                     of DisplayEnv * error * range
exception internal UnresolvedOverloading                         of DisplayEnv * error list * error list * error list * string * range
exception internal PossibleOverload                              of DisplayEnv * string * range
//exception internal PossibleBestOverload                              of DisplayEnv * string * range
exception internal NonRigidTypar                                 of DisplayEnv * string option * range * typ * typ * range

type ConstraintSolverState =
  {css_g: TcGlobals;
   css_amap: ImportMap;
   css_InfoReader : InfoReader;
   mutable css_cxs: Internal.Utilities.Hashtbl.t<stamp,(TraitConstraintInfo * range)> ;}

type ConstraintSolverEnv 

val internal BakedInTraitConstraintNames : string list

val internal MakeConstraintSolverEnv : ConstraintSolverState -> range -> DisplayEnv -> ConstraintSolverEnv

type trace = Trace of (unit -> unit) list ref

type OptionalTrace =
  | NoTrace
  | WithTrace of trace

val internal SimplifyMeasuresInTypeScheme                       : TcGlobals -> bool -> typars -> typ -> typars
val internal SolveTyparEqualsTyp                      : ConstraintSolverEnv -> int -> range -> OptionalTrace -> typ -> typ -> unit OperationResult
val internal SolveTypEqualsTypKeepAbbrevs             : ConstraintSolverEnv -> int -> range -> OptionalTrace -> typ -> typ -> unit OperationResult
val internal CanonicalizeRelevantMemberConstraints    : ConstraintSolverEnv -> int -> OptionalTrace -> typars -> unit OperationResult
val internal ResolveOverloading                       : ConstraintSolverEnv -> OptionalTrace -> string -> int * int -> AccessorDomain -> expr Typrelns.CalledMeth list ->  bool -> (typ * expr) option -> expr Typrelns.CalledMeth option * unit OperationResult
val internal UnifyUniqueOverloading                   : ConstraintSolverEnv -> int * int -> string -> AccessorDomain -> expr Typrelns.CalledMeth list -> bool OperationResult 
val internal EliminateConstraintsForGeneralizedTypars : ConstraintSolverEnv -> OptionalTrace -> typars -> unit 
val internal IsArrayTypeWithIndexer : Env.TcGlobals -> typ -> bool

val internal AddConstraint                             : ConstraintSolverEnv -> int -> Range.range -> OptionalTrace -> Typar -> TyparConstraint -> unit OperationResult
val internal AddCxTypeEqualsType                       : DisplayEnv -> ConstraintSolverState -> range -> typ -> typ -> unit
val internal AddCxTypeEqualsTypeUndoIfFailed           : DisplayEnv -> ConstraintSolverState -> range -> typ -> typ -> bool
val internal AddCxTypeMustSubsumeType                  : DisplayEnv -> ConstraintSolverState -> range -> OptionalTrace -> typ -> typ -> unit
val internal AddCxTypeMustSubsumeTypeUndoIfFailed      : DisplayEnv -> ConstraintSolverState -> range -> typ -> typ -> bool
val internal AddCxMethodConstraint                     : DisplayEnv -> ConstraintSolverState -> range -> OptionalTrace -> TraitConstraintInfo -> unit
val internal AddCxTypeMustSupportNull                  : DisplayEnv -> ConstraintSolverState -> range -> OptionalTrace -> typ -> unit
val internal AddCxTypeMustSupportDefaultCtor           : DisplayEnv -> ConstraintSolverState -> range -> OptionalTrace -> typ -> unit
val internal AddCxTypeIsReferenceType                  : DisplayEnv -> ConstraintSolverState -> range -> OptionalTrace -> typ -> unit
val internal AddCxTypeIsValueType                      : DisplayEnv -> ConstraintSolverState -> range -> OptionalTrace -> typ -> unit
val internal AddCxTypeIsEnum                           : DisplayEnv -> ConstraintSolverState -> range -> OptionalTrace -> typ -> typ -> unit
val internal AddCxTypeIsDelegate                       : DisplayEnv -> ConstraintSolverState -> range -> OptionalTrace -> typ -> typ -> typ -> unit

val internal CodegenWitnessThatTypSupportsTraitConstraint : TcGlobals -> ImportMap -> range -> TraitConstraintInfo -> OperationResult<(MethInfo * tinst) option>

val internal ChooseTyparSolutionAndSolve : ConstraintSolverState -> DisplayEnv -> Typar -> unit
