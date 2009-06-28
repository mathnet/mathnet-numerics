// (c) Microsoft Corporation 2005-2009. 

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities.Collections.Tagged
#else
/// This namespace contains FSharp.PowerPack extensions for the F# collection types
namespace Microsoft.FSharp.Collections.Tagged
#endif

    open System
    open System.Collections.Generic


    /// Immutable sets based on binary trees, default tag
    type Set<'a> = Set<'a, IComparer<'a>>    

    /// Immutable sets where a constraint tag carries information about the class of key-comparer being used.  
    and 
      [<Sealed>]
      Set<'a,'comparerTag> when 'comparerTag :> IComparer<'a> =

        /// A useful shortcut for Set.add.  Note this operation prodcues a new set
        /// and does not mutate the original set.  The new set will share many storage
        /// nodes with the original.  See the Set module for further operations on sets.
        member Add : 'a -> Set<'a,'comparerTag>
        
        /// A useful shortcut for Set.remove.  Note this operation produces a new set
        /// and does not mutate the original set.  The new set will share many storage
        /// nodes with the original.  See the Set module for further operations on sets.
        member Remove : 'a -> Set<'a,'comparerTag>
        
        /// Return the number of elements in the set
        member Count : int
        
        /// A useful shortcut for Set.mem.  See the Set module for further operations on sets.
        member Contains : 'a -> bool
        
        /// A useful shortcut for Set.is_empty.  See the Set module for further operations on sets.
        member IsEmpty  : bool

        /// Apply the given function to each binding in the collection
        member Iterate : ('a -> unit) -> unit

        /// Apply the given accumulating function to all the elements of the set
        member Fold    : ('a -> 'b -> 'b) -> 'b -> 'b

        /// Build two new sets, one containing the elements for which the given predicate returns 'true',
        /// and the other the remaining elements.
        member Partition: predicate:('a -> bool) -> Set<'a,'comparerTag> * Set<'a,'comparerTag>

        /// Return a new collection containing only the elements of the collection
        /// for which the given predicate returns "true"
        member Filter: predicate:('a -> bool) -> Set<'a,'comparerTag> 

        /// Test if any element of the collection satisfies the given predicate.
        /// If the input function is <c>f</c> and the elements are <c>i0...iN</c> then computes 
        /// <c>p i0 or ... or p iN</c>.
        member Exists: predicate:('a -> bool) -> bool

        /// Test if all elements of the collection satisfy the given predicate.
        /// If the input function is <c>f</c> and the elements are <c>i0...iN</c> and <c>j0...jN</c> then 
        /// computes <c>p i0 &amp;&amp; ... &amp;&amp; p iN</c>.
        member ForAll: predicate:('a -> bool) -> bool

        /// A set based on the given comparer containing the given initial elements
        static member Create: 'comparerTag * seq<'a> -> Set<'a,'comparerTag> 
        
        /// The empty set based on the given comparer
        static member Empty: 'comparerTag -> Set<'a,'comparerTag> 
        
        /// A singleton set based on the given comparison operator
        static member Singleton: 'comparerTag * 'a -> Set<'a,'comparerTag> 
        
        /// Compares two sets and returns true if they are equal or false otherwise
        static member Equality : Set<'a,'comparerTag> * Set<'a,'comparerTag> -> bool
        
        /// Compares a and b and returns 1 if a &gt; b, -1 if b &lt; a and 0 if a = b        
        static member Compare : a:Set<'a,'comparerTag> * b:Set<'a,'comparerTag> -> int

        /// Return a new set with the elements of the second set removed from the first.
        static member (-) : Set<'a,'comparerTag> * Set<'a,'comparerTag> -> Set<'a,'comparerTag> 

        /// Compute the union of the two sets.
        static member (+) : Set<'a,'comparerTag> * Set<'a,'comparerTag> -> Set<'a,'comparerTag> 

        /// Compute the intersection of the two sets.
        static member Intersection : Set<'a,'comparerTag> * Set<'a,'comparerTag> -> Set<'a,'comparerTag> 

        /// Compute the union of the two sets.
        static member Union : Set<'a,'comparerTag> * Set<'a,'comparerTag> -> Set<'a,'comparerTag>

        /// Return a new set with the elements of the second set removed from the first.
        static member Difference: Set<'a,'comparerTag> * Set<'a,'comparerTag> -> Set<'a,'comparerTag> 

        /// The number of elements in the set
        member Choose : 'a 

        /// Returns the lowest element in the set according to the ordering being used for the set
        member MinimumElement: 'a

        /// Returns the highest element in the set according to the ordering being used for the set
        member MaximumElement: 'a

        /// Evaluates to "true" if all elements of the second set are in the first
        member IsSubsetOf: Set<'a,'comparerTag> -> bool

        /// Evaluates to "true" if all elements of the first set are in the second
        member IsSupersetOf: Set<'a,'comparerTag> -> bool

        /// The elements of the set as a list.
        member ToList : unit -> 'a list
        
        /// The elements of the set as an array.
        member ToArray: unit -> 'a array 

        interface ICollection<'a> 
        interface IEnumerable<'a> 
        interface System.Collections.IEnumerable

        interface System.IComparable
        override Equals : obj -> bool


    /// Immutable maps.  Keys are ordered by construction function specified
    /// when creating empty maps or by F# structural comparison if no
    /// construction function is specified.
    ///
    /// <performance> 
    ///   Maps based on structural comparison are  
    ///   efficient for small keys. They are not a suitable choice if keys are recursive data structures 
    ///   or require non-structural comparison semantics.
    /// </performance>
    type Map<'key,'a> = Map<'key, 'a, IComparer<'key>>    

    /// Immutable maps.  A constraint tag carries information about the class of key-comparers being used.  
    and  
      [<Sealed>]
      Map<'key,'a,'comparerTag>  when 'comparerTag :> IComparer<'key> =
        /// Return a new map with the binding added to the given map.
        member Add: 'key * 'a -> Map<'key,'a,'comparerTag>

        /// Return true if there are no bindings in the map.
        member IsEmpty: bool

        /// The empty map, and use the given comparer comparison function for all operations associated
        /// with any maps built from this map.
        static member Empty: 'comparerTag -> Map<'key,'a,'comparerTag>

        static member FromList : 'comparerTag * ('key * 'a) list -> Map<'key,'a,'comparerTag>

        /// Build a map that contains the bindings of the given IEnumerable
        /// and where comparison of elements is based on the given comparison function
        static member Create: 'comparerTag * seq<'key * 'a> -> Map<'key,'a,'comparerTag> 

        /// Test is an element is in the domain of the map
        member ContainsKey: 'key -> bool

        /// The number of bindings in the map
        member Count: int

        /// Lookup an element in the map. Raise <c>KeyNotFoundException</c> if no binding
        /// exists in the map.
        member Item : 'key -> 'a with get

        /// Search the map looking for the first element where the given function returns a <c>Some</c> value
        member First: ('key -> 'a -> 'b option) -> 'b option

        /// Return true if the given predicate returns true for all of the
        /// bindings in the map. Always returns true if the map is empty.
        member ForAll: ('key -> 'a -> bool) -> bool

        /// Return true if the given predicate returns true for one of the
        /// bindings in the map. Always returns false if the map is empty.
        member Exists: ('key -> 'a -> bool) -> bool

        /// Build a new map containing the bindings for which the given predicate returns 'true'.
        member Filter: ('key -> 'a -> bool) -> Map<'key,'a,'comparerTag> 

        /// Fold over the bindings in the map.  
        member Fold: folder:('key -> 'a -> 'state -> 'state) -> 'state -> 'state

        /// Given the start and end points of a key range,
        /// Fold over the bindings in the map that are in the range,
        /// and the end points are included if present (the range is considered a closed interval).
        member FoldSection: 'key -> 'key -> ('key -> 'a -> 'state -> 'state) -> 'state -> 'state

        /// Fold over the bindings in the map.  
        member FoldAndMap: ('key -> 'a -> 'state -> 'b * 'state) -> 'state -> Map<'key,'b,'comparerTag> * 'state

        /// Apply the given function to each binding in the dictionary
        member Iterate: action:('key -> 'a -> unit) -> unit

        /// Build a new collection whose elements are the results of applying the given function
        /// to each of the elements of the collection. The index passed to the
        /// function indicates the index of element being transformed.
        member Map: mapping:('key -> 'a -> 'b) -> Map<'key,'b,'comparerTag>

        /// Build a new collection whose elements are the results of applying the given function
        /// to each of the elements of the collection.
        member MapRange: mapping:('a -> 'b) -> Map<'key,'b,'comparerTag>

        /// Build two new maps, one containing the bindings for which the given predicate returns 'true',
        /// and the other the remaining bindings.
        member Partition: ('key -> 'a -> bool) -> Map<'key,'a,'comparerTag> * Map<'key,'a,'comparerTag>

        /// Remove an element from the domain of the map.  No exception is raised if the element is not present.
        member Remove: 'key -> Map<'key,'a,'comparerTag>

        /// Lookup an element in the map, returning a <c>Some</c> value if the element is in the domain 
        /// of the map and <c>None</c> if not.
        member TryFind: 'key -> 'a option

        /// The elements of the set as a list.
        member ToList : unit -> ('key * 'a) list
    
        /// The elements of the set as an array
        member ToArray: unit -> ('key * 'a) array 

        interface IEnumerable<KeyValuePair<'key, 'a>>
        
        interface System.Collections.IEnumerable 
        interface System.IComparable
        override Equals : obj -> bool

  
    

