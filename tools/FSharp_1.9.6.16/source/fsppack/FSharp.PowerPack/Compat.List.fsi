// (c) Microsoft Corporation 2005-2009.  

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities
#else
namespace Microsoft.FSharp.Compatibility
#endif

open System

/// Compatibility operations on lists.  
module List = 

    /// Like reduce_left, but return both the intermediary and final results
    val scanReduce : reduction:('T -> 'T -> 'T) -> 'T list -> 'T list

    /// Like reduce_right, but return both the intermediary and final results
    val scanReduceBack : reduction:('T -> 'T -> 'T) -> 'T list -> 'T list

    /// Is an element in the list. Elements are compared using generic equality.
    val contains: 'T -> 'T list -> bool

    /// Is an element in the list. Elements are compared using generic equality.
    [<OCamlCompatibility("The F# name for this function is 'contains'")>]
    val mem: 'T -> 'T list -> bool

    /// Lookup key's data in association list, uses (=) equality.
    /// Raise <c>System.IndexOutOfRangeException</c> exception if key not found, in which case you should typically use <c>try_assoc</c> instead.
    [<OCamlCompatibility>]
    val assoc: 'Key -> ('Key * 'T) list -> 'T

    /// Lookup key's data in association list, uses (=) equality,
    /// returning "Some data" or "None".  
    [<OCamlCompatibility>]
    val try_assoc: 'Key -> ('Key * 'T) list -> 'T option

    /// Does the key have pair in the association list?
    [<OCamlCompatibility>]
    val mem_assoc: 'Key -> ('Key * 'T) list -> bool

    /// Remove pair for key from the association list (if it's there).
    [<OCamlCompatibility>]
    val remove_assoc: 'Key -> ('Key * 'T) list -> ('Key * 'T) list

    /// See <c>assoc</c>, but uses the physical equality operator (==) for equality tests
    [<OCamlCompatibility>]
    val assq: 'Key -> ('Key * 'T) list -> 'T
      
    /// See <c>try_assoc</c>, but uses the physical equality operator (==) for equality tests.    
    [<OCamlCompatibility>]
    val try_assq: 'Key -> ('Key * 'T) list -> 'T option

    /// See <c>mem_assoc</c>, but uses the physical equality operator (==) for equality tests.      
    [<OCamlCompatibility>]
    val mem_assq: 'Key -> ('Key * 'T) list -> bool

    /// See <c>remove_assoc</c>, but uses the physical equality operator (==) for equality tests.        
    [<OCamlCompatibility>]
    val remove_assq: 'Key -> ('Key * 'T) list -> ('Key * 'T) list

    /// See <c>mem</c>, but uses the physical equality operator (==) for equality tests.        
    [<OCamlCompatibility>]
    val memq: 'Key -> 'Key list -> bool

    /// Return true if the list is not empty.
    [<Obsolete("This function will be removed. Use 'not List.isEmpty' instead")>]
    val nonempty: 'Key list -> bool

    /// "rev_map f l1" evaluates to "map f (rev l1)"
    [<OCamlCompatibility>]
    val rev_map: mapping:('T -> 'U) -> 'T list -> 'U list

    /// "rev_map2 f l1 l2" evaluates to "map2 f (rev l1) (rev l2)"
    [<OCamlCompatibility>]
    val rev_map2: mapping:('T1 -> 'T2 -> 'U) -> 'T1 list -> 'T2 list -> 'U list

    /// "rev_append l1 l2" evaluates to "append (rev l1) l2"
    [<OCamlCompatibility>]
    val rev_append: 'T list -> 'T list -> 'T list

    [<Obsolete("This function has been renamed to 'scanReduce'")>]
    val scan1_left : reduction:('T -> 'T -> 'T) -> 'T list -> 'T list

    [<Obsolete("This function has been renamed to 'scanReduceBack'")>]
    val scan1_right : reduction:('T -> 'T -> 'T) -> 'T list -> 'T list

    [<Obsolete("This function will be removed in a future release")>]
    val tryfind_indexi: predicate:(int -> 'T -> bool) -> list:'T list -> int option

    [<Obsolete("This function will be removed in a future release")>]
    val find_indexi: predicate:(int -> 'T -> bool) -> list:'T list -> int 


    

