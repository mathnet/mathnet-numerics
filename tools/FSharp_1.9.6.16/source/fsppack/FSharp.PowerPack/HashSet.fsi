// (c) Microsoft Corporation 2005-2009. 

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities
#else
namespace Microsoft.FSharp.Collections
#endif


open System
open System.Collections.Generic

/// Mutable hash sets based by default on F# structural "hash" and (=) functions. Implemented via a hash table and/or Dictionary.
[<Sealed>]
type HashSet<'a>  =

    /// Create a new empty mutable hash set 
    /// with key hash/equality based on the F# structural "hash" and (=) functions
    new : unit -> HashSet<'a>

    /// Create a new empty mutable hash set with an internal bucket array of the given approximate size
    /// and with key hash/equality based on the F# structural "hash" and (=) functions
    [<OverloadID("new_size")>]
    new : size:int -> HashSet<'a>

    /// Create a new empty mutable hash set with an internal bucket array of the given approximate size
    /// and with the given key hash/equality functions 
    new : size:int * comparer:IEqualityComparer<'a> -> HashSet<'a>

    /// Create a new mutable hash set containing elements drawn from the given sequence
    [<OverloadID("new_seq")>]
    new : elements:seq<'a> -> HashSet<'a>
    
    /// Create a new empty mutable hash set 
    /// with key hash/equality based on the F# structural "hash" and (=) functions
    [<Obsolete("Use 'new HashSet<_>(...)' instead")>]
    static member Create : unit -> HashSet<'a>

    /// Create a new empty mutable hash set with an internal bucket array of the given approximate size
    /// and with key hash/equality based on the F# structural "hash" and (=) functions
    [<OverloadID("Create_size")>]
    [<Obsolete("Use 'new HashSet<_>(...)' instead")>]
    static member Create : size:int -> HashSet<'a>

    /// Create a new empty mutable hash set with an internal bucket array of the given approximate size
    /// and with the given key hash/equality functions 
    [<Obsolete("Use 'new HashSet<_>(...)' instead")>]
    static member Create : size:int * comparer:IEqualityComparer<'a> -> HashSet<'a>

    /// Create a new mutable hash set containing elements drawn from the given sequence
    [<OverloadID("Create_seq")>]
    [<Obsolete("Use 'new HashSet<_>(...)' instead")>]
    static member Create : elements:seq<'a> -> HashSet<'a>
    
    /// Make a shallow copy of the set
    member Copy    : unit -> HashSet<'a>
    
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

    /// The total number of elements in the set
    member Count   : int

    /// Apply the given function to each binding in the hash table 
    member Iterate : ('a -> unit) -> unit

    interface IEnumerable<'a> 
    interface System.Collections.IEnumerable 
