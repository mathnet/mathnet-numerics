//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    open System
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    /// Basic operations on 2-dimensional arrays. 
    ///
    /// F# and .NET multi-dimensional arrays are typically zero-based. 
    /// However, .NET multi-dimensional arrays used in conjunction with external
    /// libraries (e.g. libraries associated with Visual Basic) be 
    /// non-zero based, using a potentially different base for each dimension.
    /// The operations in this module will accept such arrays, and
    /// the basing on an input array will be propogated to a matching output
    /// array on the <c>Array2D.map</c> and <c>Array2D.mapi</c> operations.
    /// Non-zero-based arrays can also be created using <c>Array2D.zero_create_based</c>, 
    /// <c>Array2D.create_based</c> and <c>Array2D.init_based</c>.
    module Array2D = 

        /// Fetch the base-index for the first dimension of the array.
        ///
        /// See notes on the Array2D module re. zero-basing.
        val base1: array:'T[,] -> int

        /// Fetch the base-index for the second dimension of the array.
        ///
        /// See notes on the Array2D module re. zero-basing.
        val base2: array:'T[,] -> int

        /// Build a new array whose elements are the same as the input array.
        ///
        /// For non-zero-based arrays the basing on an input array will be propogated to the output
        /// array.
        val copy: array:'T[,] -> 'T[,]

        /// Read a range of elements from the first array and write them into the second.
        val blit: source:'T[,] -> sourceIndex1:int -> sourceIndex2:int -> target:'T[,] -> targetIndex1:int -> targetIndex2:int -> length1:int -> length2:int -> unit

        /// Create an array given the dimensions and a generator function to compute the elements.
        val init: length1:int -> length2:int -> initializer:(int -> int -> 'T) -> 'T[,]

        /// Create an array whose elements are all initially the given value
        val create: length1:int -> length2:int -> value:'T -> 'T[,]

        /// Create an array where the entries are initially Unchecked.defaultof&lt;'T&gt;. 
        val zeroCreate : length1:int -> length2:int -> 'T[,]

#if FX_NO_BASED_ARRAYS
#else
        /// Create a based array given the dimensions and a generator function to compute the elements.
        val initBased: base1:int -> base2:int -> length1:int -> length2:int -> initializer:(int -> int -> 'T) -> 'T[,]

        /// Create a based array whose elements are all initially the given value
        val createBased: base1:int -> base2:int -> length1:int -> length2:int -> initial: 'T -> 'T[,]

        /// Create a based array where the entries are initially Unchecked.defaultof&lt;'T&gt;. 
        val zeroCreateBased : base1:int -> base2:int -> length1:int -> length2:int -> 'T[,]
#endif

        /// Apply the given function to each element of the array. 
        val iter: action:('T -> unit) -> array:'T[,] -> unit

        /// Apply the given function to each element of the array.  The integer indicies passed to the
        /// function indicates the index of element.
        val iteri: action:(int -> int -> 'T -> unit) -> array:'T[,] -> unit

        /// Return the length of an array in the first dimension  
        val length1: array:'T[,] -> int

        /// Return the length of an array in the second dimension  
        val length2: array:'T[,] -> int
        /// Build a new array whose elements are the results of applying the given function
        /// to each of the elements of the array.
        ///
        /// For non-zero-based arrays the basing on an input array will be propogated to the output
        /// array.
        val map: mapping:('T -> 'U) -> array:'T[,] -> 'U[,]

        /// Build a new array whose elements are the results of applying the given function
        /// to each of the elements of the array. The integer indices passed to the
        /// function indicates the element being transformed.
        ///
        /// For non-zero-based arrays the basing on an input array will be propogated to the output
        /// array.
        val mapi: mapping:(int -> int -> 'T -> 'U) -> array:'T[,] -> 'U[,]


        /// Build a new array whose elements are the same as the input array but
        /// where a non-zero-based input array generates a corresponding zero-based 
        /// output array.
        val rebase: array:'T[,] -> 'T[,]

        /// Set the value of an element in an array. You can also use the syntax 'array.[index1,index2] &lt;- value' 
        val set: array:'T[,] -> index1:int -> index2:int -> value:'T -> unit

        /// Fetch an element from a 2D array. You can also use the syntax 'array.[index1,index2]' 
        val get: array:'T[,] -> index1:int -> index2:int -> 'T

#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This F# library function has been renamed. Use 'initBased' instead")>]
        val init_based: base1:int -> base2:int -> length1:int -> length2:int -> initializer:(int -> int -> 'T) -> 'T[,]

        [<Obsolete("This F# library function has been renamed. Use 'createBased' instead")>]
        val create_based: base1:int -> base2:int -> length1:int -> length2:int -> initial: 'T -> 'T[,]

        [<Obsolete("This F# library function has been renamed. Use 'zeroCreate' instead")>]
        val zero_create : length1:int -> length2:int -> 'T[,]

        [<Obsolete("This F# library function has been renamed. Use 'zeroCreateBased' instead")>]
        val zero_create_based : base1:int -> base2:int -> length1:int -> length2:int -> 'T[,]

        [<Obsolete("Use arr2.[startIndex1..endIndex1, startIndex2..endIndex2] instead")>]
        val sub: source:'T[,] -> sourceIndex1:int -> sourceIndex2:int -> count1:int -> count2:int -> 'T[,]
#endif
