// (c) Microsoft Corporation. All rights reserved

#light

/// Blobs of bytes, cross-compiling 
module Microsoft.FSharp.Compiler.AbstractIL.Internal.Bytes 

open Internal.Utilities
open Internal.Utilities.Pervasives

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 

val length: byte[] -> int
/// returned int will be 0 <= x <= 255
val get: byte[] -> int -> int    
val make: (int -> int) -> int -> byte[]
val zero_create: int -> byte[]
/// each int must be 0 <= x <= 255 
val of_intarray: int array ->  byte[] 
/// each int will be 0 <= x <= 255 

val to_intarray: byte[] -> int array 
val sub: byte[] -> int -> int -> byte[]
val set: byte[] -> int -> int -> unit
val blit: byte[] -> int -> byte[] -> int -> int -> unit
val append: byte[] -> byte[] -> byte[]
val compare: byte[] -> byte[] -> int


/// Read/write byte[] off a binary stream 
val really_input: in_channel -> int -> byte[]
val maybe_input: in_channel -> int -> byte[]
val output: out_channel -> byte[] -> unit

(* Bytes are commonly used for unicode strings *)
val string_as_unicode_bytes: string -> byte[]
val string_as_utf8_bytes: string -> byte[]
val unicode_bytes_as_string: byte[] -> string
val utf8_bytes_as_string: byte[] -> string

/// included mainly for legacy reasons
val string_as_unicode_bytes_null_terminated: string -> byte[]
val string_as_utf8_bytes_null_terminated: string -> byte[]


/// Imperative buffers and streams of byte[]
module Bytebuf =

    type t 
    val create : int -> t
    val emit_int_as_byte : t -> int -> unit
    val emit_intarray_as_bytes : t -> int array -> unit
    val close : t -> byte[]
    val emit_byte : t -> byte -> unit
    val emit_bool_as_byte : t -> bool  -> unit
    val emit_u16 : t -> uint16 -> unit
    val emit_bytes : t -> byte[] -> unit
    val emit_i32 : t -> int32 -> unit
    val emit_i64 : t -> int64 -> unit
    val emit_i32_as_u16 : t -> int32 -> unit
    val length : t -> int
    val position : t -> int
    val fixup_i32 : t -> position:int -> value:int32 -> unit


module Bytestream =
    type t
    val of_bytes: byte[] -> int -> int -> t
    val read_byte: t -> int
    val position : t -> int
    val clone_and_seek: t -> int -> t
    val skip : t -> int -> unit
    val read_bytes: t -> int -> byte[]
    val read_utf8_bytes_as_string: t -> int -> string
