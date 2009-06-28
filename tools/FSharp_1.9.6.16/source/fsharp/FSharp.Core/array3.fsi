//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    open System
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.Operators

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    /// Basic operations on rank 3 arrays.  
    module Array3D =

        /// Create an array whose elements are all initially the given value
        val create: length1:int -> length2:int -> length3:int -> initial:'T -> 'T[,,]

        /// Create an array given the dimensions and a generator function to compute the elements.
        val init: length1:int -> length2:int -> length3:int  -> initializer:(int -> int -> int -> 'T) -> 'T[,,]

        /// Fetch an element from a 3D array.  You can also use the syntax 'array.[index1,index2,index3]'
        val get: array:'T[,,] -> index1:int -> index2:int -> index3:int -> 'T

        /// Apply the given function to each element of the array. 
        val iter: action:('T -> unit) -> array:'T[,,] -> unit

        /// Apply the given function to each element of the array.  The integer indicies passed to the
        /// function indicates the index of element.
        val iteri: action:(int -> int -> int -> 'T -> unit) -> array:'T[,,] -> unit

        /// Return the length of an array in the first dimension  
        val length1: array:'T[,,] -> int

        /// Return the length of an array in the second dimension  
        val length2: array:'T[,,] -> int

        /// Return the length of an array in the third dimension  
        val length3: array:'T[,,] -> int

        /// Build a new array whose elements are the results of applying the given function
        /// to each of the elements of the array.
        ///
        /// For non-zero-based arrays the basing on an input array will be propogated to the output
        /// array.
        val map: mapping:('T -> 'U) -> array:'T[,,] -> 'U[,,]

        /// Build a new array whose elements are the results of applying the given function
        /// to each of the elements of the array. The integer indices passed to the
        /// function indicates the element being transformed.
        ///
        /// For non-zero-based arrays the basing on an input array will be propogated to the output
        /// array.
        val mapi: mapping:(int -> int -> int -> 'T -> 'U) -> array:'T[,,] -> 'U[,,]

        /// Set the value of an element in an array.  You can also 
        /// use the syntax 'array.[index1,index2,index3] &lt;- value'.
        val set: array:'T[,,] -> index1:int -> index2:int -> index3:int -> value:'T -> unit

        /// Create an array where the entries are initially the "default" value. 
        val zeroCreate: length1:int -> length2:int -> length3:int  -> 'T[,,]

#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This F# library function has been renamed. Use 'zeroCreate' instead")>]
        val zero_create: length1:int -> length2:int -> length3:int  -> 'T[,,]
#endif


    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    /// Basic operations on rank 4 arrays.  
    module Array4D =

        /// Create an array whose elements are all initially the given value
        val create: length1:int -> length2:int -> length3:int -> length4:int -> initial:'T -> 'T[,,,]

        /// Create an array given the dimensions and a generator function to compute the elements.
        val init: length1:int -> length2:int -> length3:int  -> length4:int  -> initializer:(int -> int -> int -> int -> 'T) -> 'T[,,,]

        /// Return the length of an array in the first dimension  
        val length1: array:'T[,,,] -> int

        /// Return the length of an array in the second dimension  
        val length2: array:'T[,,,] -> int

        /// Return the length of an array in the third dimension  
        val length3: array:'T[,,,] -> int

        /// Return the length of an array in the fourth dimension  
        val length4: array:'T[,,,] -> int

        /// Create an array where the entries are initially the "default" value. 
        val zeroCreate: length1:int -> length2:int -> length3:int  -> length4:int  -> 'T[,,,]

        /// Fetch an element from a 4D array.  You can also use the syntax 'array.[index1,index2,index3,index4]'
        val get: array:'T[,,,] -> index1:int -> index2:int -> index3:int -> index4:int -> 'T

        /// Set the value of an element in an array.  You can also 
        /// use the syntax 'array.[index1,index2,index3,index4] &lt;- value'.
        val set: array:'T[,,,] -> index1:int -> index2:int -> index3:int -> index4:int -> value:'T -> unit

