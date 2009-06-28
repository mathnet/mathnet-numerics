// (c) Microsoft Corporation 2005-2009. 

namespace Microsoft.FSharp.Collections

#nowarn "0057";; // active patterns

open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Control
open Microsoft.FSharp.Collections
open System.Collections.Generic

/// LazyLists are possibly-infinite, cached sequences.  See also IEnumerable/Seq for
/// uncached sequences. Calling "get" on the same lazy list value you will keep 
/// getting the same (cached) result.  LazyLists normally involve delayed computations
/// without side-effects, and calling "get" may cause these computations to be executed.  The results 
/// of these computations are cached - evaluations will be performed 
/// only once for each element of the lazy list.  This is different to IEnumerable/Seq where
/// recomputation happens each time an enumerator is created and the sequence traversed.
///
/// LazyLists can represent cached potentially-infinite computations.  Because they are cached they may cause 
/// memory leaks if some part of your code maintains a live reference to
/// the head of an infinite or very large lazy list while iterating it, or if a reference is
/// maintained after the list is no longer required.
///
/// Although lazy lists are an abstract type you may pattern match against them using the
/// LazyList.Cons and LazyList.Nil active patterns. These may force the computation of elements
/// of the list.

// abstract type: implementation details hidden.
//[<System.Obsolete("This type has been deprecated. Consider using the 'seq<_>' type instead to implement on-demand sequences of results, and 'Seq.cache' to cache results of an on-demand computation")>]
[<Sealed>]
type LazyList<'T> =
    interface IEnumerable<'T>
    interface System.Collections.IEnumerable
    

//[<System.Obsolete("This module has been deprecated. Consider using the 'seq<_>' type instead to implement on-demand sequences of results, and 'Seq.cache' to cache results of an on-demand computation")>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module LazyList =

    [<System.Obsolete("Use 'LazyList<_>' instead")>]
    type 'T t      = LazyList<'T>     
    [<System.Obsolete("Use 'LazyList<_>' instead")>]
    type 'T llist  = LazyList<'T>     

    /// Test if a stream contains at least one element.  Forces the evaluation of
    /// the first element of the stream if it is not already evaluated.
    val nonempty : LazyList<'T> -> bool

    /// Return the first element of the stream.  Raise 'Invalid_argument "hd"' if the
    /// stream is empty. Forces the evaluation of
    /// the first cell of the stream if it is not already evaluated.
    val hd       : LazyList<'T> -> 'T

    /// Return the stream corresponding to the remaining items in the sequence.  
    /// Raise 'Invalid_argument "tl"' if the stream is empty. Forces the evaluation of
    /// the first cell of the stream if it is not already evaluated.
    val tl       : LazyList<'T> -> LazyList<'T>

    /// Get the first cell of the stream.
    val get      : LazyList<'T> -> ('T * LazyList<'T>) option  

    /// Return the stream which on consumption will consist of at most 'n' elements of 
    /// the given stream.  Does not force the evaluation of any cells in the stream.
    val take     : int -> LazyList<'T> -> LazyList<'T>

    /// Return the stream without the first 'n' elements of the given stream.  Does
    /// not force the evaluation of any cells in the stream.
    val drop     : int -> LazyList<'T> -> LazyList<'T>

    /// Apply the given function to successive elements of the list, returning the first
    /// result where function returns <c>Some(x)</c> for some x. If the function never returns
    /// true, 'None' is returned.
    val first    : predicate:('T -> bool) -> LazyList<'T> -> 'T option

    /// Return the first element for which the given function returns <c>true</c>.
    /// Raise <c>KeyNotFoundException</c> if no such element exists.
    val find     : predicate:('T -> bool) -> LazyList<'T> -> 'T 

    /// Evaluates to the stream that contains no items
    val empty    : unit -> LazyList<'T>

    /// Return a new stream which contains on demand the given item followed by the
    /// given stream.
    val cons     : 'T -> LazyList<'T>               -> LazyList<'T>

    /// Return a new stream which contains on demand the given item followed by the
    /// stream returned by the given computation.  The computation is
    /// not executed until the elements of the stream are consumed.  The
    /// computation is only executed once.
    val consf    : 'T -> (unit -> LazyList<'T>)     -> LazyList<'T>

    /// Return the stream which on consumption will consist of an infinite sequence of the given item
    val repeat   : 'T -> LazyList<'T>

    /// Return a stream that is in effect the stream returned by the given computation.
    /// The given computation is not executed until the first element on the stream is
    /// consumed.
    val delayed  : (unit -> LazyList<'T>)           -> LazyList<'T>

    /// Return a stream that contains the elements returned by the given computation.
    /// The given computation is not executed until the first element on the stream is
    /// consumed.  The given argument is passed to the computation.  Subsequent elements
    /// in the stream are generated by again applying the residual 'b to the computation.
    val unfold   : ('State -> ('T * 'State) option) -> 'State -> LazyList<'T>

    /// Return the stream which contains on demand the elements of the first stream followed
    /// by the elements of the second list
    val append   : LazyList<'T> -> LazyList<'T> -> LazyList<'T>

    /// Return the stream which contains on demand the pair of elements of the first and second list
    val combine  : LazyList<'T1> -> LazyList<'T2> -> LazyList<'T1 * 'T2>

    /// Return the stream which contains on demand the list of elements of the list of lazy lists.
    val concat   : LazyList< LazyList<'T>> -> LazyList<'T>

    /// Return a new collection which on consumption will consist of only the elements of the collection
    /// for which the given predicate returns "true"
    val filter   : predicate:('T -> bool) -> LazyList<'T> -> LazyList<'T>

    /// Return a new stream consisting of the results of applying the given accumulating function
    /// to successive elements of the stream
    val folds    : folder:('State -> 'T -> 'State) -> 'State -> LazyList<'T> -> LazyList<'State>  

    /// Build a new collection whose elements are the results of applying the given function
    /// to each of the elements of the collection.
    val map      : mapping:('T -> 'U) -> LazyList<'T> -> LazyList<'U>

    /// Build a new collection whose elements are the results of applying the given function
    /// to the corresponding elements of the two collections pairwise.
    val map2     : mapping:('T1 -> 'T2 -> 'U) -> LazyList<'T1> -> LazyList<'T2> -> LazyList<'U>

    /// Build a collection from the given array. This function will eagerly evaluate all of the 
    /// stream (and thus may not terminate). 
    val of_array : 'T array -> LazyList<'T>

    /// Build an array from the given collection
    val to_array : LazyList<'T> -> 'T array

    /// Build a collection from the given list. This function will eagerly evaluate all of the 
    /// stream (and thus may not terminate). 
    val of_list  : list<'T> -> LazyList<'T>

    /// Build a list from the given collection This function will eagerly evaluate all of the 
    /// stream (and thus may not terminate). 
    val to_list  : LazyList<'T> -> list<'T>

    /// Return a view of the collection as an enumerable object
    val to_seq: LazyList<'T> -> seq<'T>

    /// Build a new collection from the given enumerable object
    val of_seq: seq<'T> -> LazyList<'T>

    //--------------------------------------------------------------------------
    // Lazy list active patterns

    val (|Cons|Nil|) : LazyList<'T> -> Choice<('T * LazyList<'T>),unit>

