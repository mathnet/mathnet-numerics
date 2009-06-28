(* (c) Microsoft Corporation. All rights reserved  *)

module Microsoft.FSharp.Compiler.AbstractIL.Internal.Zmap 

open Internal.Utilities.Collections.Tagged
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

type 'a order = 'a -> 'a -> int
  
/// Maps with a specific comparison function
type ('key,'a) map = Internal.Utilities.Collections.Tagged.Map<'key,'a> 
type ('key,'a) t = ('key,'a) map


val empty    : 'key order -> ('key,'a) t
val is_empty : ('key,'a) t -> bool
    
val add      : 'key -> 'a -> ('key,'a) t -> ('key,'a) t
val remove   : 'key -> ('key,'a) t -> ('key,'a) t
val mem      : 'key -> ('key,'a) t -> bool
val mem_of   :  ('key,'a) t -> 'key -> bool
val tryfind  : 'key -> ('key,'a) t -> 'a option
val find     : 'key -> ('key,'a) t -> 'a          (* or raises Not_found *)

val map      : mapping:('a -> 'b) -> ('key,'a) t -> ('key,'b) t
val mapi     : ('key -> 'a -> 'b) -> ('key,'a) t -> ('key,'b) t
val fold     : ('key -> 'a -> 'b -> 'b) -> ('key,'a) t -> 'b -> 'b
val fmap     : ('z -> 'key -> 'a -> 'z * 'b) -> 'z -> ('key,'a) t -> 'z * ('key,'b) t
val iter     : action:('a -> 'b -> unit) -> ('a, 'b) map -> unit

val fold_section: 'key -> 'key -> ('key -> 'a -> 'b -> 'b) -> ('key,'a) t -> 'b -> 'b  

val first    : ('key -> 'a -> bool) -> ('key,'a) t -> ('key * 'a) option
val exists   : ('key -> 'a -> bool) -> ('key,'a) t -> bool
val forall   : ('key -> 'a -> bool) -> ('key,'a) t -> bool

val choose   : ('key -> 'a -> 'b option) -> ('key,'a) t -> 'b option
val chooseL  : ('key -> 'a -> 'b option) -> ('key,'a) t -> 'b list

val to_list   : ('key,'a) t -> ('key * 'a) list
val of_list   : 'key order -> ('key * 'a) list -> ('key,'a) t  
val of_FlatList : 'key order -> FlatList<'key * 'a> -> ('key,'a) t  

val keys     : ('key,'a) t -> 'key list
val values   : ('key,'a) t -> 'a   list
