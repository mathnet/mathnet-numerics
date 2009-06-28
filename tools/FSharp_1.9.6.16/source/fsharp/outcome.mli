// (c) Microsoft Corporation 2005-2009. 

module (* internal *) Microsoft.FSharp.Compiler.Outcome

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library

type 'a outcome = ResultOrException<'a>
val success : 'a -> 'a outcome
val raze : exn -> 'a outcome
val trappable_outcome_cases : mapping:('a -> 'b) -> (exn -> 'b) -> 'a outcome -> 'b
val trappable_option : exn -> 'a option -> 'a outcome
val trappable_trywith : 'a outcome -> (exn -> 'a outcome) -> 'a outcome
val trappable_outcome_first : exn -> ('a -> 'b outcome) -> 'a list -> 'b outcome
val ( ||?> ) : 'a outcome -> ('a -> 'b outcome) -> 'b outcome
val ( |?> ) : 'a outcome -> ('a -> 'b) -> 'b outcome
val razewith : string -> 'a outcome
val ForceRaise : 'a outcome -> 'a
val trappable_map : ('a -> 'b outcome) -> 'a list -> 'b list outcome
val trappable_map2 : ('a -> 'b -> 'c outcome) -> 'a list -> 'b list -> 'c list outcome
val otherwise : (unit -> 'a outcome) -> 'a outcome -> 'a outcome
