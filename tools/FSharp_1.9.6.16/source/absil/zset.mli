(* (c) Microsoft Corporation. All rights reserved  *)

module Microsoft.FSharp.Compiler.AbstractIL.Internal.Zset 

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

type 'a order = 'a -> 'a -> int

/// Sets with a specific comparison function
type 'a set = Internal.Utilities.Collections.Tagged.Set<'a>
type 'a t = 'a set

val empty     : 'a order -> 'a t
val is_empty  : 'a t -> bool
val mem       : 'a -> 'a t -> bool
val mem_of    : 'a t -> 'a -> bool
val add       : 'a -> 'a t -> 'a t
val addList   : 'a list -> 'a t -> 'a t
val addFlatList : FlatList<'a> -> 'a t -> 'a t
val singleton : 'a order -> 'a -> 'a t
val remove    : 'a -> 'a t -> 'a t

val count     : 'a t -> int
val union     : 'a t -> 'a t -> 'a t
val inter     : 'a t -> 'a t -> 'a t
val diff      : 'a t -> 'a t -> 'a t
val equal     : 'a t -> 'a t -> bool
val subset    : 'a t -> 'a t -> bool
val for_all   : predicate:('a -> bool) -> 'a t -> bool
val forall    : predicate:('a -> bool) -> 'a t -> bool
val exists    : predicate:('a -> bool) -> 'a t -> bool
val filter    : predicate:('a -> bool) -> 'a t -> 'a  t   

val fold      : ('a -> 'b -> 'b) -> 'a t -> 'b -> 'b
val iter      : ('a -> unit) -> 'a t -> unit

val elements  : 'a t -> 'a list



