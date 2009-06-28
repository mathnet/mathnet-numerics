//==========================================================================
// ResizeArray
// 
// (c) Microsoft Corporation 2005-2008.  
//===========================================================================

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities
#else
namespace Microsoft.FSharp.Collections
#endif


open System
open System.Collections.Generic
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
/// Generic operations on the type System.Collections.Generic.List, which is called ResizeArray in the F# libraries.
module ResizeArray =

    /// Return the length of the collection.  You can also use property <c>arr.Length</c>.
    val length: ResizeArray<'a> -> int

    /// Fetch an element from the collection.  You can also use the syntax <c>arr.[idx]</c>.
    val get: ResizeArray<'a> -> int -> 'a

    /// Set the value of an element in the collection.  You can also use the syntax <c>arr.[idx] &lt;- e</c>.
    val set: ResizeArray<'a> -> int -> 'a -> unit

    /// Create an array whose elements are all initially the given value.
    val create: int -> 'a -> ResizeArray<'a>
     
    /// Create an array by calling the given generator on each index.
    val init: int -> (int -> 'a) -> ResizeArray<'a>

    ///Build a new array that contains the elements of the first array followed by the elements of the second array
    val append: ResizeArray<'a> -> ResizeArray<'a> -> ResizeArray<'a>

    ///Build a new array that contains the elements of each of the given list of arrays
    val concat: ResizeArray<'a> list -> ResizeArray<'a>

    ///Build a new array that contains the given subrange specified by
    ///starting index and length.
    val sub: ResizeArray<'a> -> int -> int -> ResizeArray<'a>

    ///Build a new array that contains the elements of the given array
    val copy: ResizeArray<'a> -> ResizeArray<'a>

    ///Fill a range of the collection with the given element
    val fill: ResizeArray<'a> -> int -> int -> 'a -> unit

    ///Read a range of elements from the first array and write them into the second.
    val blit: ResizeArray<'a> -> int -> ResizeArray<'a> -> int -> int -> unit

    ///Build a list from the given array
    val to_list: ResizeArray<'a> -> 'a list

    ///Build an array from the given list
    val of_list: 'a list -> ResizeArray<'a>

    /// Apply a function to each element of the collection, threading an accumulator argument
    /// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c> 
    /// then computes <c>f (... (f s i0)...) iN</c>
    val fold_left: ('a -> 'b -> 'a) -> 'a -> ResizeArray<'b> -> 'a

    /// Apply a function to each element of the array, threading an accumulator argument
    /// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c> then 
    /// computes <c>f i0 (...(f iN s))</c>.
    val fold_right: ('a -> 'b -> 'b) -> ResizeArray<'a> -> 'b -> 'b

    ///Apply the given function to each element of the array. 
    val iter: ('a -> unit) -> ResizeArray<'a> -> unit

    ///Build a new array whose elements are the results of applying the given function
    ///to each of the elements of the array.
    val map: ('a -> 'b) -> ResizeArray<'a> -> ResizeArray<'b>

    ///Apply the given function to two arrays simultaneously. The
    ///two arrays must have the same lengths, otherwise an Invalid_argument exception is
    ///raised.
    val iter2: ('a -> 'b -> unit) -> ResizeArray<'a> -> ResizeArray<'b> -> unit

    ///Build a new collection whose elements are the results of applying the given function
    ///to the corresponding elements of the two collections pairwise.  The two input
    ///arrays must have the same lengths.
    val map2: ('a -> 'b -> 'c) -> ResizeArray<'a> -> ResizeArray<'b> -> ResizeArray<'c>

    ///Apply the given function to each element of the array.  The integer passed to the
    ///function indicates the index of element.
    val iteri: (int -> 'a -> unit) -> ResizeArray<'a> -> unit

    ///Build a new array whose elements are the results of applying the given function
    ///to each of the elements of the array. The integer index passed to the
    ///function indicates the index of element being transformed.
    val mapi: (int -> 'a -> 'b) -> ResizeArray<'a> -> ResizeArray<'b>

    /// Test if any element of the array satisfies the given predicate.
    /// If the input function is <c>f</c> and the elements are <c>i0...iN</c> 
    /// then computes <c>p i0 or ... or p iN</c>.
    val exists: ('a -> bool) -> ResizeArray<'a> -> bool

    /// Test if all elements of the array satisfy the given predicate.
    /// If the input function is <c>f</c> and the elements are <c>i0...iN</c> and "j0...jN"
    /// then computes <c>p i0 &amp;&amp; ... &amp;&amp; p iN</c>.
    val for_all: ('a -> bool) -> ResizeArray<'a> -> bool

    ///Return a new collection containing only the elements of the collection
    ///for which the given predicate returns <c>true</c>
    val filter: ('a -> bool) -> ResizeArray<'a> -> ResizeArray<'a>

    ///Split the collection into two collections, containing the 
    ///elements for which the given predicate returns <c>true</c> and <c>false</c>
    ///respectively 
    val partition: ('a -> bool) -> ResizeArray<'a> -> ResizeArray<'a> * ResizeArray<'a>

    ///Apply the given function to each element of the array. Return
    ///the array comprised of the results "x" for each element where
    ///the function returns Some(x)
    val choose: ('a -> 'b option) -> ResizeArray<'a> -> ResizeArray<'b>

    ///Return the first element for which the given function returns <c>true</c>.
    ///Raise <c>KeyNotFoundException</c> if no such element exists.
    val find: ('a -> bool) -> ResizeArray<'a> -> 'a

    ///Return the first element for which the given function returns <c>true</c>.
    ///Return None if no such element exists.
    val tryfind: ('a -> bool) -> ResizeArray<'a> -> 'a option

    ///Apply the given function to successive elements, returning the first
    ///result where function returns "Some(x)" for some x.
    val first: ('a -> 'b option) -> ResizeArray<'a> -> 'b option

    ///Combine the two arrays into an array of pairs. The two arrays must have equal lengths.
    val combine: ResizeArray<'a> -> ResizeArray<'b> -> ResizeArray<('a * 'b)>

    ///Split a list of pairs into two lists
    val split: ResizeArray<('a * 'b)> -> (ResizeArray<'a> * ResizeArray<'b>)

    ///Return a new array with the elements in reverse order
    val rev: ResizeArray<'a> -> ResizeArray<'a>

    /// Sort the elements using the given comparison function
    val sort: ('a -> 'a -> int) -> ResizeArray<'a> -> unit

    /// Sort the elements using the key extractor and generic comparison on the keys
    val sort_by: ('a -> 'key) -> ResizeArray<'a> -> unit


    /// Return a view of the array as an enumerable object
    val to_seq : ResizeArray<'a> -> seq<'a>

    /// Test elements of the two arrays pairwise to see if any pair of element satisfies the given predicate.
    /// Raise ArgumentException if the arrays have different lengths.
    val exists2 : ('a -> 'b -> bool) -> ResizeArray<'a> -> ResizeArray<'b> -> bool

    /// Return the index of the first element in the array
    /// that satisfies the given predicate. Raise <c>KeyNotFoundException</c> if 
    /// none of the elements satisy the predicate.
    val find_index : ('a -> bool) -> ResizeArray<'a> -> int

    /// Return the index of the first element in the array
    /// that satisfies the given predicate. Raise <c>KeyNotFoundException</c> if 
    /// none of the elements satisy the predicate.
    val find_indexi : (int -> 'a -> bool) -> ResizeArray<'a> -> int

    /// Apply a function to each element of the array, threading an accumulator argument
    /// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c> 
    /// then computes <c>f (... (f i0 i1)...) iN</c>. Raises ArgumentException if the array has size zero.
    val reduce_left : ('a -> 'a -> 'a) -> ResizeArray<'a> -> 'a

    /// Apply a function to each element of the array, threading an accumulator argument
    /// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c> then 
    /// computes <c>f i0 (...(f iN-1 iN))</c>. Raises ArgumentException if the array has size zero.
    val reduce_right : ('a -> 'a -> 'a) -> ResizeArray<'a> -> 'a

    /// Apply a function to pairs of elements drawn from the two collections, 
    /// left-to-right, threading an accumulator argument
    /// through the computation.  The two input
    /// arrays must have the same lengths, otherwise an <c>ArgumentException</c> is
    /// raised.
    val fold_left2: ('state -> 'b1 -> 'b2 -> 'state) -> 'state -> ResizeArray<'b1> -> ResizeArray<'b2> -> 'state

    /// Apply a function to pairs of elements drawn from the two collections, right-to-left, 
    /// threading an accumulator argument through the computation.  The two input
    /// arrays must have the same lengths, otherwise an <c>ArgumentException</c> is
    /// raised.
    val fold_right2 : ('a1 -> 'a2 -> 'b -> 'b) -> ResizeArray<'a1> -> ResizeArray<'a2> -> 'b -> 'b

    /// Test elements of the two arrays pairwise to see if all pairs of elements satisfy the given predicate.
    /// Raise ArgumentException if the arrays have different lengths.
    val for_all2 : ('a -> 'b -> bool) -> ResizeArray<'a> -> ResizeArray<'b> -> bool

    /// Return true if the given array is empty, otherwise false
    val is_empty : ResizeArray<'a> -> bool

    /// Apply the given function to pair of elements drawn from matching indices in two arrays,
    /// also passing the index of the elements. The two arrays must have the same lengths, 
    /// otherwise an <c>ArgumentException</c> is raised.
    val iteri2 : (int -> 'a -> 'b -> unit) -> ResizeArray<'a> -> ResizeArray<'b> -> unit

    /// Build a new collection whose elements are the results of applying the given function
    /// to the corresponding elements of the two collections pairwise.  The two input
    /// arrays must have the same lengths, otherwise an <c>ArgumentException</c> is
    /// raised.
    val mapi2 : (int -> 'a -> 'b -> 'c) -> ResizeArray<'a> -> ResizeArray<'b> -> ResizeArray<'c>

    /// Like <c>fold_left</c>, but return the intermediary and final results
    val scan_left : ('b -> 'a -> 'b) -> 'b -> ResizeArray<'a> -> ResizeArray<'b>

    /// Like <c>fold_right</c>, but return both the intermediary and final results
    val scan_right : ('a -> 'c -> 'c) -> ResizeArray<'a> -> 'c -> ResizeArray<'c>

    /// Return an array containing the given element
    val singleton : 'a -> ResizeArray<'a>
    
    /// Return the index of the first element in the array
    /// that satisfies the given predicate.
    val tryfind_index : ('a -> bool) -> ResizeArray<'a> -> int option

    /// Return the index of the first element in the array
    /// that satisfies the given predicate.
    val tryfind_indexi : (int -> 'a -> bool) -> ResizeArray<'a> -> int option

    /// Combine the two arrays into an array of pairs. The two arrays must have equal lengths, otherwise an <c>ArgumentException</c> is
    /// raised..
    val zip : ResizeArray<'a> -> ResizeArray<'b> -> ResizeArray<'a * 'b>

    /// Split an array of pairs into two arrays
    val unzip : ResizeArray<'a * 'b> -> ResizeArray<'a> * ResizeArray<'b>
