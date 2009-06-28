// (c) Microsoft Corporation. All rights reserved

module (* internal *) Microsoft.FSharp.Compiler.Patcompile

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Range

/// What should the decision tree contain for any incomplete match? 
type (* internal *) ActionOnFailure = 
  | Incomplete 
  | Throw 
  | Rethrow 
  | FailFilter

[<StructuralEquality(false); StructuralComparison(false)>]
type (* internal *) pat =
  | TPat_const of Constant * range
  | TPat_wild of range
  | TPat_as of  pat * pbind * range
  | TPat_disjs of  pat list * range
  | TPat_conjs of  pat list * range
  | TPat_query of (expr * typ list * ValRef option * int * ActivePatternInfo) * pat * range
  | TPat_unioncase of UnionCaseRef * tinst * pat list * range
  | TPat_exnconstr of TyconRef * pat list * range
  | TPat_tuple of  pat list * typ list * range
  | TPat_array of  pat list * typ * range
  | TPat_recd of TyconRef * tinst * (tinst * pat) list * range
  | TPat_range of char * char * range
  | TPat_null of range
  | TPat_isinst of typ * typ * pbind option * range
and (* internal *) pbind = PBind of Val * TypeScheme

and (* internal *) tclause =  TClause of pat * expr option * DecisionTreeTarget * range

val internal RangeOfPat : pat -> range

val internal CompilePattern : 
    Env.TcGlobals ->
    Tastops.DisplayEnv ->
    Import.ImportMap -> 
    range ->  (* range of the expression we are matching on *)
    range ->  (* range of the whole match clause on *)
    // warn on unused? 
    bool ->  
    ActionOnFailure -> 
    // the value being matched against, perhaps polymorphic 
    Val * typars -> 
    // input type-checked syntax of pattern matching
    tclause list -> 
    typ -> 
      // produce TAST nodes
      DecisionTree * DecisionTreeTarget list
	

exception internal MatchIncomplete of bool * (string * bool) option * range
exception internal RuleNeverMatched of range
