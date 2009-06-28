// (c) Microsoft Corporation 2005-2009. 

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities.Collections.Tagged
#else
namespace Microsoft.FSharp.Collections.Tagged
#endif

    open System
    open System.Collections.Generic

    /// HashMultiMap, but where a constraint tag tracks information about the hash/equality functions used
    /// for the hashing. When the tag is Tags.StructuralHash this is identical to HashMultiMap.
    [<Sealed>]
    type HashMultiMap<'key,'a,'hashTag> when 'hashTag :> IEqualityComparer<'key> =
        /// Create a new empty mutable hash table with an internal bucket array of the given approximate size
        /// and with the given key hash/equality functions
        static member Create: 'hashTag * int             -> HashMultiMap<'key,'a,'hashTag>

        /// Make a shallow copy of the collection
        member Copy    : unit    -> HashMultiMap<'key,'a,'hashTag>

        /// Add a binding for the element to the table
        member Add     : 'key * 'a -> unit

        /// Clear all elements from the collection
        member Clear   : unit    -> unit

        /// Test if the collection contains any bindings for the given element
        [<System.Obsolete("This member has been renamed to ContainsKey")>]
        member Contains: 'key      -> bool

        /// Test if the collection contains any bindings for the given element
        member ContainsKey: 'key      -> bool

        /// Remove the latest binding (if any) for the given element from the table
        member Remove  : 'key      -> unit

        /// Replace the latest binding (if any) for the given element.
        member Replace : 'key * 'a -> unit

        /// Lookup or set the given element in the table.  Raise <c>KeyNotFoundException</c> if the element is not found.
        member Item : 'key -> 'a with get,set

        /// Lookup the given element in the table, returning the result as an Option
        member TryFind : 'key      -> 'a option
        /// Find all bindings for the given element in the table, if any
        member FindAll : 'key      -> 'a list

        /// Apply the given function to each element in the collection threading the accumulating parameter
        /// through the sequence of function applications
        member Fold    : ('key -> 'a -> 'c -> 'c) -> 'c -> 'c

        /// The number of bindings in the hash table
        member Count   : int

        /// Apply the given function to each binding in the hash table 
        member Iterate : ('key -> 'a -> unit) -> unit

    type HashMultiMap<'key,'a> = HashMultiMap<'key,'a, IEqualityComparer<'key>>    

    /// Mutable hash sets based on F# structural "hash" and (=) functions. Implemented via a hash table and/or Dictionary.
    
    /// Mutable hash sets where a constraint tag tracks information about the hash/equality functions used
    /// for the hashing. When the tag is Tags.StructuralHash this is identical to HashSet.
    [<Sealed>]
    type HashSet<'a,'hashTag> when 'hashTag :> IEqualityComparer<'a>  =
        /// Create a new empty mutable hash set with an internal bucket array of the given approximate size
        /// and with the given key hash/equality functions 
        static member Create: 'hashTag * int             -> HashSet<'a,'hashTag>

        /// Make a shallow copy of the set
        member Copy    : unit -> HashSet<'a,'hashTag>
        /// Add an element to the collection
        member Add     : 'a   -> unit
        /// Clear all elements from the set
        member Clear   : unit -> unit
        /// Test if the set contains the given element
        member Contains: 'a   -> bool
        /// Remove the given element from the set
        member Remove  : 'a   -> unit
        /// Apply the given function to the set threading the accumulating parameter
        /// through the sequence of function applications
        member Fold    : ('a -> 'b -> 'b) -> 'b -> 'b
        
        /// The number of elements in the set
        member Count   : int

        /// Apply the given function to each binding in the hash table 
        member Iterate : ('a -> unit) -> unit

        interface IEnumerable<'a> 
        interface System.Collections.IEnumerable 

    type HashSet<'a> = HashSet<'a, IEqualityComparer<'a>>    
