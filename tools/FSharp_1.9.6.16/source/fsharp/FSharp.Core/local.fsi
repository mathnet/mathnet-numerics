//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

/// Definitions internal for this library.
namespace Microsoft.FSharp.Primitives.Basics 

open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections

module internal List =
    val init : int -> (int -> 'a) -> 'a list
    val iter : ('a -> unit) -> 'a list -> unit
    val filter : predicate:('a -> bool) -> 'a list -> 'a list
    val collect : ('a -> 'b list) -> 'a list -> 'b list
    val partition : predicate:('a -> bool) -> 'a list -> 'a list * 'a list
    val map : mapping:('a -> 'b) -> 'a list -> 'b list
    val map2 : mapping:('a -> 'b -> 'c) -> 'a list -> 'b list -> 'c list
    val mapi : (int -> 'a -> 'b) -> 'a list -> 'b list
    val forall : predicate:('a -> bool) -> 'a list -> bool
    val exists : predicate:('a -> bool) -> 'a list -> bool
    val rev: 'a list -> 'a list
    val concat : seq<'a list> -> 'a list
    val iteri : action:(int -> 'a -> unit) -> 'a list -> unit
    val unzip : ('a * 'b) list -> 'a list * 'b list
    val unzip3 : ('a * 'b * 'c) list -> 'a list * 'b list * 'c list
    val zip : 'a list -> 'b list -> ('a * 'b) list
    val zip3 : 'a list -> 'b list -> 'c list -> ('a * 'b * 'c) list
    val of_array : 'a[] -> 'a list
    val to_array : 'a list -> 'a[]
    val sortWith : ('a -> 'a -> int) -> 'a list -> 'a list

module internal Array =
    val inline zeroCreate : int -> 'a[]
    val init : int -> (int -> 'a) -> 'a[]
    val permute : indexMap:(int -> int) -> 'a[] -> 'a[]


