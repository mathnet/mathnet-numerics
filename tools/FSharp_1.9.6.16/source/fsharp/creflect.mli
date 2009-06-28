// (c) Microsoft Corporation. All rights reserved

#light

module internal Microsoft.FSharp.Compiler.Creflect
open Microsoft.FSharp.Compiler 

// Convert quoted TAST data structures to structures ready for pickling 

open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops

type cenv 
val mk_cenv : Env.TcGlobals * Import.ImportMap * ccu -> cenv

type env
val empty_env : env 
val BindTypars : env -> typars -> env 

exception InvalidQuotedTerm of exn
exception IgnoringPartOfQuotedTermWarning of string * Range.range

val ConvExprPublic : Env.TcGlobals * Import.ImportMap * ccu -> env -> expr -> Tast.typ list * Tast.expr list * Sreflect.ExprData 
val ConvMethodBase  : cenv -> env ->  Val  -> Sreflect.MethodBaseData

