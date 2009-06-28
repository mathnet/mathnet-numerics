// (c) Microsoft Corporation 2005-2009. 

   
namespace Microsoft.FSharp.Compatibility

    open System

    /// Compatibility operations on arrays.  
    [<RequireQualifiedAccess>]
    module Array = 

        /// Create a jagged 2 dimensional array.
        ///
        ///  This member is primarily provided for compatibility with implementations 
        ///  of ML. F# also supports non-jagged 2D arrays - see the Array2D module and 
        ///  types such as "int[,]".
        val createJaggedMatrix: int -> int -> 'T -> 'T array array
        /// Is an element in the array, uses (=) equality.
        val inline contains: 'T -> 'T array -> bool

        /// Like reduce, but return both the intermediary and final results
        val scanReduce : reduction:('T -> 'T -> 'T) -> 'T array -> 'T array

        /// Like reduceBack, but return both the intermediary and final results
        val scanReduceBack : reduction:('T -> 'T -> 'T) -> 'T array -> 'T array


        /// Pin the given array for the duration of a single call to the given function.  A native pointer to
        /// the first element in the array is passed to the given function.  Cleanup the GCHandle associated with the 
        /// pin when the function completes, even if an exception is raised.
        [<Unverifiable>]
        [<NoDynamicInvocation>]
        val inline pin: 'T[] -> (nativeptr<'T> -> 'b) -> 'b

        /// As for Array.pin, except that the caller is responsible for calling Free on the returned GCHandle in order
        /// to release the pin.
        [<Unverifiable>]
        [<NoDynamicInvocation>]
        val inline pinUnscoped: 'T[] -> nativeptr<'T> *  System.Runtime.InteropServices.GCHandle



        [<OCamlCompatibility("The F# name for this function is 'createJaggedMatrix'")>]
        val create_matrix: int -> int -> 'T -> 'T array array

        /// Create a jagged 2 dimensional array.  Synonym for create.
        ///
        ///  This member is primarily provided for compatibility with implementations 
        ///  of ML. F# also supports non-jagged 2D arrays - see the Array2D module and 
        ///  types such as "int[,]".
        [<Obsolete("Use Array.create_matrix, or 2-dimensional array types from the Array2D module")>]
        val make_matrix: int -> int -> 'T -> 'T array array

        [<OCamlCompatibility("The F# name for this function is 'contains'")>]
        val inline mem: 'T -> 'T array -> bool

        [<Obsolete("This function has been renamed to 'scanReduceBack'")>]
        val scan1_left : reduction:('T -> 'T -> 'T) -> 'T array -> 'T array

        [<Obsolete("This function has been renamed to 'scanReduceBack'")>]
        val scan1_right : reduction:('T -> 'T -> 'T) -> 'T array -> 'T array
            
        [<Unverifiable>]
        [<NoDynamicInvocation>]
        [<Obsolete("This function has been renamed to 'pinUnscoped'")>]
        val inline pin_unscoped: 'T[] -> nativeptr<'T> *  System.Runtime.InteropServices.GCHandle


        /// Return true if the list is not empty.
        [<Obsolete("This function will be removed. Use 'not Array.isEmpty' instead")>]
        val nonempty: 'T[] -> bool

