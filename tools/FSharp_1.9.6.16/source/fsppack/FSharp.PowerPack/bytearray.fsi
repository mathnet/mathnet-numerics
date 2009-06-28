//==========================================================================
// (c) Microsoft Corporation 2005-2008.  The interface to the module 
// is similar to that found in versions of other ML implementations, 
// but is not an exact match.  The type signatures in this interface
// are an edited version of those generated automatically by running 
// "bin\fsc.exe -i" on the implementation file.
//===========================================================================


/// Byte arrays. Arrays of bytes type-compatible with the C# byte[] type
#if INTERNALIZED_POWER_PACK
module internal Internal.Utilities.Bytearray
#else
[<System.Obsolete("This module will be removed in a future release of the F# PowerPack. Use the Array module instead")>]
module Microsoft.FSharp.Compatibility.Bytearray
#endif

type bytearray = byte[]

val append: byte[] -> byte[] -> byte[]
val blit: byte[] -> int -> byte[] -> int -> int -> unit
val compare: byte[] -> byte[] -> int
val concat: byte[] list -> byte[]
val copy: byte[] -> byte[]
val create: int -> byte[]
val fill: byte[] -> int -> int -> byte -> unit

/// Apply a function to each element of the collection, threading an accumulator argument
/// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c> 
/// then computes <c>f (... (f s i0)...) iN</c>
val fold_left: ('State -> byte -> 'State) -> 'State -> byte[] -> 'State

/// Apply a function to each element of the collection, threading an accumulator argument
/// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c> 
/// then computes <c>f i0 (...(f iN s))</c>.
val fold_right: (byte -> 'State -> 'State) -> byte[] -> 'State -> 'State
val get: byte[] -> int -> byte
val init: int -> (int -> byte) -> byte[]

/// Apply the given function to each element of the collection. 
val iter: (byte -> unit) -> byte[] -> unit

/// Apply the given function to each element of the collection. The integer passed to the
/// function indicates the index of element.
val iteri: (int -> byte -> unit) -> byte[] -> unit

val length: byte[] -> int
val make: int -> byte[]
/// Build a new collection whose elements are the results of applying the given function
/// to each of the elements of the collection.
val map: (byte -> byte) -> byte[] -> byte[]

/// Build a new collection whose elements are the results of applying the given function
/// to each of the elements of the collection. The integer index passed to the
/// function indicates the index of element being transformed.
val mapi: (int -> byte -> byte) -> byte[] -> byte[]

///Build a collection from the given list
val of_list: byte list -> byte[]
val set: byte[] -> int -> byte -> unit
val sub: byte[] -> int -> int -> byte[]

///Build a list from the given collection
val to_list: byte[] -> byte list
val zero_create: int -> byte[]

// --------------------------------------------------------------------
// Text encodings: convert between Unicode and AsciiTables encodings of
// textual data.  Other encodings can be accessed very easily,
// e.g. the functions below are defined as
//
// let ascii_to_string (b:byte[]) = System.Text.Encoding.ASCII.GetString(b)
// let string_to_ascii (s:string) = System.Text.Encoding.ASCII.GetBytes(s)
// -------------------------------------------------------------------- 
type encoding = System.Text.Encoding

#if FX_NO_ASCII_ENCODING
#else
val ascii_to_string: byte[] -> string
val string_to_ascii: string -> byte[]
#endif
