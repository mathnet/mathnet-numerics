//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================


namespace Microsoft.FSharp.NativeInterop

    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Collections

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    /// Contains operations on native pointers.  Use of these operators may
    /// result in the generation of unverifiable code.
    module NativePtr =

        [<Unverifiable>]
        [<NoDynamicInvocation>]
        /// Return a typed native pointer for a given machine address
        val inline of_nativeint : address:nativeint -> nativeptr<'T>

        [<Unverifiable>]
        [<NoDynamicInvocation>]
        /// Return a machine address for a given typed native pointer 
        val inline to_nativeint : address:nativeptr<'T> -> nativeint

        [<Unverifiable>]
        [<NoDynamicInvocation>]
        /// Return a typed native pointer by adding index * sizeof&lt;'T&gt; to the 
        /// given input pointer 
        val inline add : address:nativeptr<'T> -> index:int -> nativeptr<'T>

        [<Unverifiable>]
        [<NoDynamicInvocation>]
        /// Dereference the typed native pointer computed by adding index * sizeof&lt;'T&gt; to the 
        /// given input pointer 
        val inline get : address:nativeptr<'T> -> index:int -> 'T

        [<Unverifiable>]
        [<NoDynamicInvocation>]
        /// Dereference the given typed native pointer 
        val inline read : address:nativeptr<'T> -> 'T

        [<Unverifiable>]
        [<NoDynamicInvocation>]
        /// Assign the <c>value</c> into the memory location referenced by the given typed native pointer 
        val inline write : address:nativeptr<'T> -> value:'T -> unit

        [<Unverifiable>]
        [<NoDynamicInvocation>]
        /// Assign the <c>value</c> into the memory location referenced by the typed native 
        /// pointer computed by adding index * sizeof&lt;'T&gt; to the given input pointer 
        val inline set : address:nativeptr<'T> -> index:int -> value:'T -> unit

        /// Get the address of an element of a pinned array
        [<Unverifiable>]
        [<NoDynamicInvocation>]
        val inline of_array: array:'T[] -> index:int -> nativeptr<'T> 

        /// Get the address of an element of a pinned 2-dimensional array
        [<Unverifiable>]
        [<NoDynamicInvocation>]
        val inline of_array2 : array:'T[,] -> index1:int -> index2:int -> nativeptr<'T>
