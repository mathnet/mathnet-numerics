//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    open System
    open System.Collections.Generic
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Collections

    /// Immutable sets based on binary trees, where comparison is the
    /// F# structural comparison function, potentially using implementations
    /// of the IComparable interface on key values.
    ///
    /// See the Set module for further operations on sets.
    ///
    /// These sets can be used with elements of any type, but you should check that
    /// structural hashing and equality on the element type are correct for your type.  
    [<Sealed>]
    type Set<'T> =

        /// Create a set containing elements drawn from the given sequence.
        new : elements:seq<'T> -> Set<'T> 

        /// The empty set for the type 'T.
        static member Empty : Set<'T>
        
        /// A useful shortcut for Set.add.  Note this operation produces a new set
        /// and does not mutate the original set.  The new set will share many storage
        /// nodes with the original.  See the Set module for further operations on sets.
        member Add : value:'T -> Set<'T>
        
        /// A useful shortcut for Set.remove.  Note this operation produces a new set
        /// and does not mutate the original set.  The new set will share many storage
        /// nodes with the original.  See the Set module for further operations on sets.
        member Remove : value:'T -> Set<'T>
        
        /// The number of elements in the set
        member Count : int
        
        /// A useful shortcut for Set.contains.  See the Set module for further operations on sets.
        member Contains : value:'T -> bool
        
        /// A useful shortcut for Set.isEmpty.  See the Set module for further operations on sets.
        member IsEmpty  : bool

        /// Return a new set with the elements of the second set removed from the first.
        static member Subtract : set1:Set<'T> * set2:Set<'T> -> Set<'T> 

        /// Return a new set with the elements of the second set removed from the first.
        static member (-) : set1:Set<'T> * set2:Set<'T> -> Set<'T> 

        /// Compute the union of the two sets.
        static member (+) : set1:Set<'T> * set2:Set<'T> -> Set<'T> 


        /// Evaluates to "true" if all elements of the second set are in the first
        member IsSubsetOf: set:Set<'T> -> bool

        /// Evaluates to "true" if all elements of the first set are in the second
        member IsSupersetOf: set:Set<'T> -> bool


        /// Returns the lowest element in the set according to the ordering being used for the set
        member MinimumElement: 'T

        /// Returns the highest element in the set according to the ordering being used for the set
        member MaximumElement: 'T

#if DONT_INCLUDE_DEPRECATED
#else
        /// Returns the least element in the set that is greater than the given key 
        /// according to the ordering being used for the set
        [<Obsolete("This member is being removed. Consider searching the elements explicitly instead")>]
        member GetNextElement: value:'T -> 'T option

        /// Returns the greatest element in the set that is less than the given key 
        /// according to the ordering being used for the set
        [<Obsolete("This member is being removed. Consider searching the elements explicitly instead")>]
        member GetPreviousElement: value:'T -> 'T option
#endif

        interface ICollection<'T> 
        interface IEnumerable<'T> 
        interface System.Collections.IEnumerable 

        interface System.IComparable
        override Equals : obj -> bool


namespace Microsoft.FSharp.Collections
        
    open System
    open System.Collections.Generic
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Collections

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]

    /// Functional programming operators related to the <c>Set&lt;_&gt;</c> type.
    module Set = 

        /// The empty set for the type 'T .
        [<GeneralizableValue>]
        val empty<'T> : Set<'T>

        /// The set containing the given one element.
        val singleton: value:'T -> Set<'T>

        /// Return a new set with an element added to the set.  No exception is raised if
        /// the set already contains the given element.
        val add: value:'T -> set:Set<'T> -> Set<'T>

        ///Evaluates to "true" if the given element is in the given set
        val contains: element:'T -> set:Set<'T> -> bool

        ///Return the number of elements in the set.  Same as <c>size</c>
        val count: set:Set<'T> -> int

        ///Test if any element of the collection satisfies the given predicate.
        ///If the input function is <c>f</c> and the elements are <c>i0...iN</c> 
        ///then computes <c>p i0 or ... or p iN</c>.
        val exists: predicate:('T -> bool) -> set:Set<'T> -> bool

        /// Return a new collection containing only the elements of the collection
        /// for which the given predicate returns <c>true</c>
        val filter: predicate:('T -> bool) -> set:Set<'T> -> Set<'T>

        /// Return a new collection containing the results of applying the
        /// given function to each element of the input set
        val map: mapping:('T -> 'U) -> set:Set<'T> -> Set<'U>

        /// Apply the given accumulating function to all the elements of the set
        val fold: folder:('State -> 'T -> 'State) -> state:'State -> set:Set<'T> -> 'State

        /// Apply the given accumulating function to all the elements of the set
        val foldBack: folder:('T -> 'State -> 'State) -> set:Set<'T> -> state:'State -> 'State

        /// Test if all elements of the collection satisfy the given predicate.
        /// If the input function is <c>f</c> and the elements are <c>i0...iN</c> and "j0...jN"
        /// then computes <c>p i0 &amp;&amp; ... &amp;&amp; p iN</c>.
        val forall: predicate:('T -> bool) -> set:Set<'T> -> bool

        /// Compute the intersection of the two sets.
        val intersect: set1:Set<'T> -> set2:Set<'T> -> Set<'T>

        ///Compute the intersection of a sequence of sets. The sequence must be non-empty
        val intersectMany: sets:seq<Set<'T>> -> Set<'T>

        ///Compute the union of the two sets.
        val union: set1:Set<'T> -> set2:Set<'T> -> Set<'T>

        ///Compute the union of a sequence of sets.
        val unionMany: sets:seq<Set<'T>> -> Set<'T>

        ///Return "true" if the set is empty
        val isEmpty: set:Set<'T> -> bool

        ///Apply the given function to each element of the set, in order according
        ///to the comparison function
        val iter: action:('T -> unit) -> set:Set<'T> -> unit

        ///Split the set into two sets containing the elements for which the given predicate
        ///returns true and false respectively
        val partition: predicate:('T -> bool) -> set:Set<'T> -> (Set<'T> * Set<'T>)

        ///Return a new set with the given element removed.  No exception is raised in 
        ///the set doesn't contain the given element.
        val remove: value: 'T -> set:Set<'T> -> Set<'T>

        /// Build a set that contains the same elements as the given list
        val of_list: elements:'T list -> Set<'T>

        /// Build a list that contains the elements of the set in order
        val to_list: set:Set<'T> -> 'T list

        /// Build a set that contains the same elements as the given array
        val of_array: array:'T array -> Set<'T>

        /// Build an array that contains the elements of the set in order
        val to_array: set:Set<'T> -> 'T array

        /// Return a view of the collection as an enumerable object
        val to_seq: set:Set<'T> -> seq<'T>

        /// Build a new collection from the given enumerable object
        val of_seq: elements:seq<'T> -> Set<'T>

#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This F# library function has been renamed. Use 'unionMany' instead")>]
        val union_all: seq<Set<'T>> -> Set<'T>

        [<Obsolete("Use the property notation 'set.MinumumElement' instead")>]
        val min_elt: set:Set<'T> -> 'T

        [<Obsolete("Use the property notation 'set.MaximumElement' instead")>]
        val max_elt: set:Set<'T> -> 'T

        [<Obsolete("This function is being removed. Consider using a different data structure, or iterating the elements of the set explicitly")>]
        val next_elt: value:'T -> set:Set<'T> -> 'T option

        [<Obsolete("This function is being removed. Consider using a different data structure, or iterating the elements of the set explicitly")>]
        val prev_elt: value:'T -> set:Set<'T> -> 'T option

        [<Obsolete("This function is being removed. Use 'Set.count' instead, or the property notation 'set.Size'")>]
        val size: set:Set<'T> -> int

        [<Obsolete("This function is being removed. Use 'Operators.compare' on the two sets")>]
        val compare: set1:Set<'T> -> set2:Set<'T> -> int

        ///Evaluates to "true" if the given element is in the given set
        [<Obsolete("This F# library function has been renamed. Use 'contains' instead")>]
        val mem: element:'T -> set:Set<'T> -> bool

        /// Returns the minimum element of the set
        [<Obsolete("This function is being removed. Use 'set.MinimumElement' instead")>]
        val choose: set:Set<'T> -> 'T

        /// Evaluates to "true" if all elements of the second set are in the first        
        [<Obsolete("This function is being removed. Use 'set.IsSubsetOf' instead")>]
        val subset: set1:Set<'T> -> set2:Set<'T> -> bool

        /// Return a new set with the elements of the second set removed from the first.
        [<Obsolete("This function is being removed. Use the subtraction operator instead, e.g. a - b")>]
        val diff: set1:Set<'T> -> set2:Set<'T> -> Set<'T>

        [<Obsolete("This F# library function has been renamed. Use 'intersectMany' instead")>]
        val intersect_all: seq<Set<'T>> -> Set<'T>

        [<OCamlCompatibility("This F# library function has been renamed. Use 'isEmpty' instead")>]
        val is_empty: set:Set<'T> -> bool

        [<OCamlCompatibility("This F# library function has been renamed. Use 'forall' instead")>]
        val for_all: predicate:('T -> bool) -> set:Set<'T> -> bool

        [<Obsolete("This F# library function has been renamed. Use 'fold' instead")>]
        val fold_left: folder:('State -> 'T -> 'State) -> state:'State -> set:Set<'T> -> 'State

        [<Obsolete("This F# library function has been renamed. Use 'foldBack' instead")>]
        val fold_right: folder:('T -> 'State -> 'State) -> set:Set<'T> -> state:'State -> 'State

#endif
