//==========================================================================
// (c) Microsoft Corporation 2005-2008. The interface to the module 
// is similar to that found in versions of other ML implementations, 
// but is not an exact match. The type signatures in this interface
// are an edited version of those generated automatically by running 
// "bin\fsc.exe -i" on the implementation file.
//===========================================================================
///Pervasives: Additional OCaml-compatible bindings 
#if INTERNALIZED_POWER_PACK
module (* internal *) Internal.Utilities.Pervasives
#else
module Microsoft.FSharp.Compatibility.OCaml.Pervasives
#endif

#nowarn "62" // compatibility warnings
#nowarn "35"  // 'deprecated' warning about redefining '<' etc.
#nowarn "86"  // 'deprecated' warning about redefining '<' etc.

open System
open System.IO
open System.Collections.Generic
open Microsoft.FSharp.Core

//--------------------------------------------------------------------------
//Pointer (physical) equality and hashing.

///Reference/physical equality. 
///True if boxed versions of the inputs are reference-equal, OR if
///both are value types and the implementation of Object.Equals for the type
///of the first argument returns true on the boxed versions of the inputs. 
///
///In normal use on reference types or non-mutable value types this function 
///has the following properties:
///   - returns 'true' for two F# values where mutation of data
///     in mutable fields of one affects mutation of data in the other
///   - will return 'true' if (=) returns true
///   - hashq will return equal hashes if (==) returns 'true'
///
///The use on mutable value types is not recommended.
[<OCamlCompatibility("Using the physical equality operator '==' is not recommended except in cross-compiled code. Consider using generic structural equality 'x = y' or 'LanguagePrimitives.PhysicalEquality x y'")>]
val inline (==): 'a -> 'a -> bool
/// Negation of the '==' operator, see also Obj.eq
[<OCamlCompatibility("Using the physical inequality operator '!=' is not recommended except in cross-compiled code. Consider using generic structual inequality 'x <> y' or 'not(LanguagePrimitives.PhysicalEquality x y)'")>]
val inline (!=): 'a -> 'a -> bool
[<OCamlCompatibility("Consider using the overloaded operator 'x % y' instead of 'x mod y'. The precedence of these operators differs, so you may need to add parentheses")>]
val inline (mod): int -> int -> int 
[<OCamlCompatibility("Consider using the overloaded operator 'x &&& y' instead of 'x land y'. The precedence of these operators differs, so you may need to add parentheses")>]
val inline (land): int -> int -> int 
[<OCamlCompatibility("Consider using the overloaded operator 'x ||| y' instead of 'x lor y'. The precedence of these operators differs, so you may need to add parentheses")>]
val inline (lor) : int -> int -> int 
[<OCamlCompatibility("Consider using the overloaded operator 'x ^^^ y' instead of 'x lxor y'. The precedence of these operators differs, so you may need to add parentheses")>]
val inline (lxor): int -> int -> int
[<OCamlCompatibility("Consider using the overloaded operator '~~~x' instead of 'lnot x'")>]
val inline lnot  : int -> int
[<OCamlCompatibility("Consider using the overloaded operator 'x <<< y' instead of 'x lsl y'. The precedence of these operators differs, so you may need to add parentheses")>]
val inline (lsl): int -> int -> int
[<OCamlCompatibility("Consider using the overloaded operator 'x >>> y' on an unsigned type instead of 'x lsr y'. The precedence of these operators differs, so you may need to add parentheses")>]
val inline (lsr): int -> int -> int
[<OCamlCompatibility("Consider using the overloaded operator 'x >>> y' instead of 'x asr y'. The precedence of these operators differs, so you may need to add parentheses")>]
val inline (asr): int -> int -> int

/// 1D Array element get-accessor ('getter')
[<OCamlCompatibility("Consider using 'arr.[idx]' instead")>]
val inline ( .() ) : 'a array -> int -> 'a
/// 1D Array element set-accessor ('setter')
[<OCamlCompatibility("Consider using 'arr.[idx] <- v' instead")>]
val inline ( .()<- ) : 'a array -> int -> 'a -> unit

//--------------------------------------------------------------------------
//Integer-specific arithmetic

/// n-1 (no overflow checking)
[<OCamlCompatibility>]
val pred: int -> int
/// n+1 (no overflow checking)
[<OCamlCompatibility>]
val succ: int -> int

/// The lowest representable value in the 'int' type
[<OCamlCompatibility("Consider using 'System.Int32.MinValue' instead")>]
val min_int : int
/// The highest representable value in the 'int' type
[<OCamlCompatibility("Consider using 'System.Int32.MaxValue' instead")>]
val max_int : int

/// Negation on integers of the 'int' type
[<OCamlCompatibility("Consider using the overloaded operator '-x' instead of 'int_neg x'")>]
val int_neg : int -> int

//--------------------------------------------------------------------------
//Exceptions

[<OCamlCompatibility("Consider using System.IO.EndOfStreamException instead")>]
exception End_of_file = System.IO.EndOfStreamException
[<OCamlCompatibility("Consider using System.OutOfMemoryException instead")>]
exception Out_of_memory = System.OutOfMemoryException
[<OCamlCompatibility("Consider using System.DivideByZeroException instead")>]
exception Division_by_zero = System.DivideByZeroException
[<OCamlCompatibility("Consider using System.StackOverflowException instead")>]
exception Stack_overflow = System.StackOverflowException 

[<OCamlCompatibility("This is a synonym for 'System.Collections.Generic.KeyNotFoundException'")>]
val Not_found<'a> : exn
[<OCamlCompatibility("This is a synonym for 'System.Collections.Generic.KeyNotFoundException'")>]
val (|Not_found|_|) : exn -> unit option

[<OCamlCompatibility("This exception is raised by the 'exit' function should exiting fail")>]
exception Exit 

///  Non-exhaustive match failures will raise Match failures
/// A future release of F# may map this exception to a corresponding .NET exception.
[<OCamlCompatibility("Consider using 'MatchFailure' instead")>]
exception Match_failure = Microsoft.FSharp.Core.MatchFailure

/// The exception thrown by 'assert' failures.
/// A future release of F# may map this exception to a corresponding .NET exception.
[<OCamlCompatibility>]
exception Assert_failure of string * int * int 

/// The exception thrown by <c>invalid_arg</c> and misues of F# library functions
[<OCamlCompatibility("Consider using 'System.ArgumentException' instead")>]
val Invalid_argument : string -> exn
[<OCamlCompatibility("Consider matching against 'System.ArgumentException' instead")>]
val (|Invalid_argument|_|) : exn -> string option

//--------------------------------------------------------------------------
//Floating point.
//
//The following operators only manipulate 'float64' numbers. The operators  '+' etc. may also be used.

/// This value is present primarily for compatibility with other versions of ML
[<OCamlCompatibility("Consider using the overloaded operator 'x * y' instead of 'x *. y'")>]
val ( *. ): float -> float -> float
/// This value is present primarily for compatibility with other versions of ML. In F#
/// the overloaded operators may be used.
[<OCamlCompatibility("Consider using the overloaded operator 'x + y' instead of 'x +. y'")>]
val ( +. ): float -> float -> float
/// This value is present primarily for compatibility with other versions of ML. In F#
/// the overloaded operators may be used.
[<OCamlCompatibility("Consider using the overloaded operator 'x - y' instead of 'x -. y'")>]
val ( -. ): float -> float -> float
/// This value is present primarily for compatibility with other versions of ML. In F#
/// the overloaded operators may be used.
[<OCamlCompatibility("Consider using the overloaded operator '-x' instead of '-. x'")>]
val ( ~-. ): float -> float
/// This value is present primarily for compatibility with other versions of ML. In F#
/// the overloaded operators may be used.
[<OCamlCompatibility("Consider using the overloaded operator '+x' instead of '+. x'")>]
val ( ~+. ): float -> float
/// This value is present primarily for compatibility with other versions of ML. In F#
/// the overloaded operators may be used.
[<OCamlCompatibility("Consider using the overloaded operator 'x / y' instead of 'x /. y'")>]
val ( /. ): float -> float -> float

[<OCamlCompatibility("Consider using the overloaded F# library function 'abs' instead")>]
val abs_float: float -> float

/// This value is present primarily for compatibility with other versions of ML
/// The highest representable positive value in the 'float' type
[<OCamlCompatibility("Consider using 'System.Double.MaxValue' instead")>]
val max_float: float

/// This value is present primarily for compatibility with other versions of ML
/// The lowest non-denormalized positive IEEE64 float
[<OCamlCompatibility>]
val min_float: float

/// This value is present primarily for compatibility with other versions of ML
/// The smallest value that when added to 1.0 gives a different value to 1.0
[<OCamlCompatibility>]
val epsilon_float: float

/// This value is present primarily for compatibility with other versions of ML
[<OCamlCompatibility("Consider using the '%' operator instead")>]
val mod_float: float -> float -> float

/// This value is present primarily for compatibility with other versions of ML
[<OCamlCompatibility>]
val modf: float -> float * float

/// This value is present primarily for compatibility with other versions of ML
[<OCamlCompatibility("Consider using '-infinity' instead")>]
val neg_infinity: float

[<OCamlCompatibility>]
val ldexp: float -> int -> float

[<OCamlCompatibility>]
type fpclass = 
  | FP_normal
  | FP_zero
  | FP_infinite
  | FP_nan

[<OCamlCompatibility>]
val classify_float: float -> fpclass

//--------------------------------------------------------------------------
//Common conversions. See also conversions such as
//Float32.to_int etc.


[<OCamlCompatibility>]
val bool_of_string: string -> bool

[<OCamlCompatibility("Consider using the overloaded operator 'char' instead")>]
val char_of_int: int -> char

[<OCamlCompatibility("Consider using the overloaded operator 'int' instead")>]
val int_of_char: char -> int

[<OCamlCompatibility("Consider using the overloaded operator 'int' instead")>]
val int_of_string: string -> int

[<OCamlCompatibility("Consider using the overloaded operator 'int' instead")>]
val int_of_float: float -> int

[<OCamlCompatibility("Consider using the overloaded operator 'string' instead")>]
val string_of_bool: bool -> string

[<OCamlCompatibility("Consider using the overloaded operator 'string' instead")>]
val string_of_float: float -> string

[<OCamlCompatibility("Consider using the overloaded operator 'string' instead")>]
val string_of_int: int -> string

[<OCamlCompatibility("Consider using the overloaded conversion function 'float' instead")>]
val float_of_int: int -> float

[<OCamlCompatibility("Consider using the overloaded conversion function 'float' instead")>]
val float_of_string: string -> float


//--------------------------------------------------------------------------
//I/O
//
//Caveat: These functions do not have precisely the same behaviour as 
//corresponding functions in other ML implementations, e.g. OCaml. 
//For example they may raise .NET exceptions rather than Sys_error.

  
/// This type is present primarily for compatibility with other versions of ML. When
/// not cross-compiling we recommend using the .NET I/O libraries
[<OCamlCompatibility("For advanced I/O consider using the System.IO namespace")>]
type open_flag = 
  | Open_rdonly
  | Open_wronly
  | Open_append
  | Open_creat
  | Open_trunc
  | Open_excl
  | Open_binary
  | Open_text
#if FX_NO_NONBLOCK_IO
#else
  | Open_nonblock
#endif
  | Open_encoding of System.Text.Encoding

//--------------------------------------------------------------------------


/// A pseudo-abstraction over binary and textual input channels.
/// OCaml-compatible channels conflate binary and text IO, and for this reasons their
/// use from F# is somewhat deprecated (direct use of System.IO StreamReader, TextReader and 
/// BinaryReader objects is preferred, e.g. see System.IO.File.OpenText). 
/// Well-written OCaml-compatible code that simply opens either a channel in text or binary 
/// mode and then does text or binary I/O using the OCaml-compatible functions below
/// will work, though care must be taken with regard to end-of-line characters (see 
/// input_char below).
///
/// This library pretends that an in_channel is just a System.IO.TextReader. Channel values
/// created using open_in_bin maintain a private System.IO.BinaryReader, which will be used whenever
/// you do I/O using this channel. 
///
/// InChannel.of_BinaryReader and InChannel.of_StreamReader allow you to build input 
/// channels out of the corresponding .NET abstractions.
[<OCamlCompatibility("Consider using one of the types System.IO.TextReader, System.IO.BinaryReader or System.IO.StreamReader instead")>]
type in_channel = System.IO.TextReader
    

/// Open the given file to read. 
///
///In the absence of an explicit encoding (e.g. using Open_encoding) open_in
///uses the default text encoding (System.Text.Encoding.Default). If you want to read a file
///regardless of encoding then you should use binary modes. Note that .NET's 
///"new StreamReader" function defaults to use a utf8 encoding, and also attempts
///to determine an automatic encoding by looking for "byteorder-marks" at the head
///of a text file. This function does not do this.
///
/// No CR-LF translation is done on input.
[<OCamlCompatibility("Consider using 'System.IO.File.OpenText(path)' instead")>]
val open_in: path:string -> in_channel

/// Open the given file to read in text-mode using the UTF8 encoding
[<Obsolete("This value is deprecated. Use 'System.IO.File.OpenText(path)' instead")>]
val open_in_utf8: path:string -> in_channel

/// Open the given file to read in binary-mode 
[<OCamlCompatibility("Consider using 'new System.IO.BinaryReader(System.IO.File.OpenRead(path))' and changing your type to be a BinaryReader instead")>]
val open_in_bin: path:string -> in_channel

/// Open the given file in the mode specified by the given flags
[<OCamlCompatibility("For advanced I/O consider using the System.IO namespace instead")>]
val open_in_gen: flags: open_flag list -> int -> path:string -> in_channel

/// Close the channel
[<OCamlCompatibility("Consider using 'channel.Close()' instead")>]
val close_in: channel:in_channel -> unit

/// Return the length of the input channel
[<OCamlCompatibility("Consider using 'channel.BaseStream.Length' instead")>]
val in_channel_length: channel:in_channel -> int

/// Attempt to input the given number of bytes from the channel, writing them into the
/// buffer at the given start position. Does not block if the bytes are not available.
///
/// The use of this function with a channel performing byte-to-character translation (e.g. one
/// created with open_in, open_in_utf8 or open_in_encoded, or one 
/// or built from a StreamReader or TextReader) is not recommended.
/// Instead, open the channel using open_in_bin or InChannel.of_BinaryReader.
///
/// If used with a StreamReader channel, i.e. one created using 
/// open_in, open_in_utf8 or open_in_encoded, or one 
/// or built from a StreamReader, this function reads bytes directly from the underlying
/// BaseStream. This may not be appropriate if any other input techniques are being
/// used on the channel.
///
/// If used with a TextReader channel (e.g. stdin), this function reads characters from the
/// stream and then fills some of the byte array with the decoding of these into 
/// bytes, where the decoding is performed using the System.Text.Encoding.Default encoding
///
/// Raise End_of_file (= System.IO.EndOfStreamException) if end of file reached.
[<OCamlCompatibility("Consider using 'channel.Read(buffer,index,count)' instead")>]
val input: channel:in_channel -> buffer:byte[] -> index:int -> count:int -> int

/// Attempt to input characters from a channel. Does not block if inpout is not available.
/// Raise End_of_file (= System.IO.EndOfStreamException) if end of file reached.
///
/// No CRLF translation is done on input, even in text mode. That is, if an input file
/// has '\r\n' (CRLF) line terminators both characters will be seen in the input.
[<OCamlCompatibility("Consider using 'channel.Read(buffer,index,count)' instead")>]
val input_chars: channel:in_channel -> buffer:char[] -> index:int -> count:int -> int

/// Input a binary integer from a binary channel. Compatible with output_binary_int.
[<OCamlCompatibility("Consider using 'channel.ReadInt32()' on a BinaryReader instead")>]
val input_binary_int: channel:in_channel -> int

/// Input a single byte. 
/// For text channels this only accepts characters with a UTF16 encoding that fits in a byte, e.g. ASCII.
/// Raise End_of_file (= System.IO.EndOfStreamException) if end of file reached.
[<OCamlCompatibility("Consider using the 'Read()' method on a 'BinaryReader' instead, which returns -1 if no byte is available")>]
val input_byte: channel:in_channel -> int

/// Input a single character. Raise End_of_file (= System.IO.EndOfStreamException) if end of file reached.
[<OCamlCompatibility("Consider using the 'channel.Read()' method instead, which returns -1 if no character is available")>]
val input_char: channel:in_channel -> char

/// Input a single line. Raise End_of_file (= System.IO.EndOfStreamException) if end of file reached.
[<OCamlCompatibility("Consider using the 'channel.ReadLine()' method instead")>]
val input_line: channel:in_channel -> string

#if FX_NO_BINARY_SERIALIZATION
#else
/// Input a single serialized value from a binary stream. Raise End_of_file (= System.IO.EndOfStreamException) if end of file reached.
[<OCamlCompatibility("Consider deserializing using an object of type 'System.Runtime.Serialization.Formatters.Binary.BinaryFormatter' method instead")>]
val input_value: channel:in_channel -> 'a
#endif
/// Report the current position in the input channel
[<OCamlCompatibility("Consider using 'channel.BaseStream.Position' property instead")>]
val pos_in: channel:in_channel -> int

/// Reads bytes from the channel. Blocks if the bytes are not available.
/// See 'input' for treatment of text channels.
/// Raise End_of_file (= System.IO.EndOfStreamException) if end of file reached.
[<OCamlCompatibility>]
val really_input: channel:in_channel -> buffer:byte[] -> index:int -> count:int -> unit

/// Reads bytes from the channel. Blocks if the bytes are not available.
/// For text channels this only accepts UTF-16 bytes with an encoding less than 256.
/// Raise End_of_file (= System.IO.EndOfStreamException) if end of file reached.
[<OCamlCompatibility("Consider using 'channel.BaseStream.Seek' method instead, or using a 'System.IO.BinaryReader' and related types for binary I/O")>]
val seek_in: channel:in_channel -> int -> unit

/// Set the binary mode to true or false. If the binary mode is changed from "true" to 
/// "false" then a StreamReader is created to read the binary stream. The StreamReader uses 
/// the default text encoding System.Text.Encoding.Default
[<OCamlCompatibility("Consider using 'System.IO.BinaryReader' and related types for binary I/O")>]
val set_binary_mode_in: channel:in_channel -> bool -> unit

[<Obsolete("For F# code unsafe_really_input is identical to really_input");
  OCamlCompatibility>]
val unsafe_really_input: channel:in_channel -> byte[] -> int -> int -> unit

//--------------------------------------------------------------------------
//Output channels (out_channel). 

/// An pseudo-abstraction over binary and textual output channels.
/// OCaml-compatible channels conflate binary and text IO, and for this reasons their
/// use from F# is somewhat deprecated The direct use of System.IO StreamWriter, TextWriter and 
/// BinaryWriter objects is preferred, e.g. see System.IO.File.CreateText). Well-written OCaml code 
/// that simply opens either a channel in text or binary mode and then does text 
/// or binary I/O using the OCaml functions will work, though care must 
/// be taken with regard to end-of-line characters (see output_char below).
///
/// This library pretends that an out_channel is just a System.IO.TextWriter. Channels
/// created using open_out_bin maintain a private System.IO.BinaryWriter, which will be used whenever
/// do I/O using this channel. 
[<OCamlCompatibility("Consider using one of the types 'System.IO.TextWriter', 'System.IO.StreamWriter' or 'System.IO.BinaryWriter' instead")>]
type out_channel  = System.IO.TextWriter

/// Open the given file to write in text-mode using the
/// System.Text.Encoding.Default encoding
///
/// See output_char for a description of CR-LF translation
/// done on output.
[<OCamlCompatibility("Consider using 'System.IO.File.CreateText(path)' instead")>]
val open_out: path:string -> out_channel

/// Open the given file to write in text-mode using the given encoding
[<Obsolete("Consider using the 'System.IO.StreamWriter' type instead")>]
val open_out_encoded:  encoding: System.Text.Encoding -> path:string -> out_channel

/// Open the given file to write in text-mode using the UTF8 encoding
[<Obsolete("Consider using 'System.IO.File.CreateText(path)' function instead")>]
val open_out_utf8: path:string -> out_channel

/// Open the given file to write in binary-mode 
[<OCamlCompatibility("Consider using 'new System.IO.BinaryWriter(System.IO.File.Create(path))' and changing your type to be a BinaryWriter instead")>]
val open_out_bin: path:string -> out_channel

/// Open the given file to write in the mode according to the specified flags
[<OCamlCompatibility("For advanced I/O consider using the System.IO namespace")>]
val open_out_gen: open_flag list -> int -> path:string -> out_channel

/// Close the given output channel
[<OCamlCompatibility("Consider using 'channel.Close()' instead, or create the channel via a 'use' binding to ensure automatic cleanup")>]
val close_out: channel:out_channel -> unit

/// Return the length of the output channel. 
/// Raise an exception if not an app
[<OCamlCompatibility("Consider using 'channel.BaseStream.Length' instead")>]
val out_channel_length: channel:out_channel -> int

/// Write the given range of bytes to the output channel. 
[<OCamlCompatibility("Consider using 'channel.Write(buffer,index,count)' instead")>]
val output: channel:out_channel -> bytes:byte[] -> index:int -> count:int -> unit

/// Write the given integer to the output channel in binary format.
/// Only valid on binary channels.
[<OCamlCompatibility("Consider using 'channel.Write(int)' instead")>]
val output_binary_int: channel:out_channel -> int:int -> unit

/// Write the given byte to the output channel. No CRLF translation is
/// performed.
[<OCamlCompatibility("Consider using 'channel.Write(byte)' instead")>]
val output_byte: channel:out_channel -> byte:int -> unit

/// Write all the given bytes to the output channel. No CRLF translation is
/// performed.
[<OCamlCompatibility("Consider using 'channel.Write(bytes,0,bytes.Length)' instead")>]
val output_bytearray: channel:out_channel -> bytes:byte[] -> unit

/// Write the given Unicode character to the output channel. 
///
/// If the output channel is a binary stream and the UTF-16 value of the Unicode character is greater
/// than 255 then ArgumentException is thrown.
///
/// No CRLF translation is done on output. That is, if the output character is
/// '\n' (LF) characters they will not be written as '\r\n' (CRLF) characters, regardless
/// of whether the underlying operating system or output stream uses CRLF as the default
/// line-feed character.
[<OCamlCompatibility("Consider using 'channel.Write(char)' instead")>]
val output_char: channel:out_channel -> char -> unit

/// Write the given Unicode string to the output channel. See output_char for the treatment of
/// '\n' characters within the string.
[<OCamlCompatibility("Consider using 'channel.Write(string)' instead")>]
val output_string: channel:out_channel -> string -> unit

#if FX_NO_BINARY_SERIALIZATION
#else
/// Serialize the given value to the output channel.
[<OCamlCompatibility("Consider serializing using an object of type 'System.Runtime.Serialization.Formatters.Binary.BinaryFormatter' instead")>]
val output_value: channel:out_channel -> 'a -> unit
#endif
/// Return the current position in the output channel, measured from the
/// start of the channel. Not valid on all channels.
[<OCamlCompatibility("Consider using 'channel.BaseStream.Position' on a TextWriter or '.Position' on a Stream instead")>]
val pos_out: channel:out_channel -> int

/// Set the current position in the output channel, measured from the
/// start of the channel.
[<OCamlCompatibility("Consider using 'channel.BaseStream.Seek' on a TextReader or 'channel.Seek' on a Stream instead")>]
val seek_out: channel:out_channel -> int -> unit

/// Set the binary mode. If the binary mode is changed from "true" to 
/// "false" then a StreamWriter is created to write the binary stream. The StreamWriter uses 
/// the default text encoding System.Text.Encoding.Default.
[<OCamlCompatibility("For advanced I/O consider using the System.IO namespace")>]
val set_binary_mode_out: channel:out_channel -> bool -> unit

/// Flush all pending output on the channel to the physical
/// output device.
[<OCamlCompatibility("Consider using 'channel.Flush()' instead")>]
val flush: channel:out_channel -> unit

//--------------------------------------------------------------------------
//Printing data to stdout/stderr


/// Print a character to the stderr stream
[<OCamlCompatibility("Consider using 'System.Console.Error.Write(char)' instead")>]
val prerr_char: char -> unit
[<OCamlCompatibility("Consider using 'System.Console.Error.WriteLine(string)' instead")>]
val prerr_endline: string -> unit
[<OCamlCompatibility("Consider using 'System.Console.Error.Write(double)' instead")>]
val prerr_float: float -> unit
[<OCamlCompatibility("Consider using 'System.Console.Error.Write(int)' instead")>]
val prerr_int: int -> unit
[<OCamlCompatibility("Consider using 'System.Console.Error.WriteLine()' instead")>]
val prerr_newline: unit -> unit
[<OCamlCompatibility("Consider using 'System.Console.Error.Write(string)' instead")>]
val prerr_string: string -> unit

[<OCamlCompatibility("Consider using 'System.Console.Write(char)' instead")>]
val print_char: char -> unit
//[<OCamlCompatibility("Consider using 'System.Console.WriteLine(string)' instead")>]
val print_endline: string -> unit
[<OCamlCompatibility("Consider using 'System.Console.Write(double)' instead")>]
val print_float: float -> unit
[<OCamlCompatibility("Consider using 'System.Console.Write(int)' instead")>]
val print_int: int -> unit

[<OCamlCompatibility("Consider using 'System.Console.WriteLine()' instead")>]
val print_newline: unit -> unit
[<OCamlCompatibility("Consider using 'System.Console.Write(string)' instead")>]
val print_string: string -> unit

//--------------------------------------------------------------------------
//Reading data from the console.


///Read a floating point number from the console.
[<OCamlCompatibility("Consider using 'System.Console.ReadLine() |> float' instead")>]
val read_float: unit -> float

///Read an integer from the console.
[<OCamlCompatibility("Consider using 'System.Console.ReadLine() |> int' instead")>]
val read_int: unit -> int

///Read a line from the console, without the end-of-line character.
[<OCamlCompatibility("Consider using 'System.Console.ReadLine()' instead")>]
val read_line: unit -> string


//--------------------------------------------------------------------------


[<OCamlCompatibility>]
module InChannel =
    ///Link .NET IO with the out_channel/in_channel model
    [<OCamlCompatibility>]
    val to_Stream: in_channel -> System.IO.Stream

    /// Access the underlying stream-based objects for the channel
    [<OCamlCompatibility>]
    val to_TextReader: in_channel -> System.IO.TextReader

    /// Access the underlying stream-based objects for the channel
    [<OCamlCompatibility>]
    val to_StreamReader: in_channel -> System.IO.StreamReader
    /// Access the underlying stream-based objects for the channel
    [<OCamlCompatibility>]
    val to_BinaryReader: in_channel -> System.IO.BinaryReader

    ///Link .NET IO with the out_channel/in_channel model
    [<OCamlCompatibility>]
    val of_BinaryReader: System.IO.BinaryReader -> in_channel
    ///Link .NET IO with the out_channel/in_channel model
    [<OCamlCompatibility>]
    val of_StreamReader: System.IO.StreamReader -> in_channel
    ///Link .NET IO with the out_channel/in_channel model
    [<OCamlCompatibility>]
    val of_TextReader: System.IO.TextReader -> in_channel
    /// Wrap a stream by creating a StreamReader for the 
    /// stream and then wrapping is as an input channel.
    /// A text encoding must be given, e.g. System.Text.Encoding.UTF8
    [<OCamlCompatibility>]
    val of_Stream:  System.Text.Encoding -> System.IO.Stream -> in_channel

    
[<OCamlCompatibility>]
module OutChannel =
    ///Link .NET IO with the out_channel/in_channel model
    [<OCamlCompatibility>]
    val of_BinaryWriter: System.IO.BinaryWriter -> out_channel
    ///Link .NET IO with the out_channel/in_channel model
    [<OCamlCompatibility>]
    val of_StreamWriter: System.IO.StreamWriter -> out_channel
    ///Link .NET IO with the out_channel/in_channel model
    [<OCamlCompatibility>]
    val of_TextWriter: System.IO.TextWriter -> out_channel

    /// Wrap a stream by creating a StreamWriter for the 
    /// stream and then wrapping is as an output channel.
    /// A text encoding must be given, e.g. System.Text.Encoding.UTF8
    [<OCamlCompatibility>]
    val of_Stream: System.Text.Encoding -> System.IO.Stream -> out_channel

    /// Access the underlying stream-based objects for the channel
    [<OCamlCompatibility>]
    val to_Stream: out_channel -> System.IO.Stream
    /// Access the underlying stream-based objects for the channel
    [<OCamlCompatibility>]
    val to_TextWriter: out_channel -> System.IO.TextWriter
    /// Access the underlying stream-based objects for the channel
    [<OCamlCompatibility>]
    val to_StreamWriter: out_channel -> System.IO.StreamWriter
    /// Access the underlying stream-based objects for the channel
    [<OCamlCompatibility>]
    val to_BinaryWriter: out_channel -> System.IO.BinaryWriter

[<Obsolete("Consider using the corresponding InChannel.* member instead")>]
val binary_reader_to_in_channel: System.IO.BinaryReader -> in_channel
[<Obsolete("Consider using the corresponding OutChannel.* member instead")>]
val binary_writer_to_out_channel: System.IO.BinaryWriter -> out_channel
[<Obsolete("Consider using the corresponding InChannel.* member instead")>]
val stream_reader_to_in_channel: System.IO.StreamReader -> in_channel
[<Obsolete("Consider using the corresponding OutChannel.* member instead")>]
val stream_writer_to_out_channel: System.IO.StreamWriter -> out_channel
[<Obsolete("Consider using the corresponding InChannel.* member instead")>]
val text_reader_to_in_channel: System.IO.TextReader -> in_channel
[<Obsolete("Consider using the corresponding OutChannel.* member instead")>]
val text_writer_to_out_channel: System.IO.TextWriter -> out_channel
[<Obsolete("Consider using the corresponding InChannel.* member instead")>]
val stream_to_in_channel:  System.Text.Encoding -> System.IO.Stream -> in_channel
[<Obsolete("Consider using the corresponding OutChannel.* member instead")>]
val stream_to_out_channel: System.Text.Encoding -> System.IO.Stream -> out_channel
[<Obsolete("Consider using the corresponding InChannel.* member instead")>]
val in_channel_to_stream: in_channel -> System.IO.Stream
[<Obsolete("Consider using the corresponding InChannel.* member instead")>]
val in_channel_to_text_reader: in_channel -> System.IO.TextReader
[<Obsolete("Consider using the corresponding InChannel.* member instead")>]
val in_channel_to_stream_reader: in_channel -> System.IO.StreamReader
[<Obsolete("Consider using the corresponding InChannel.* member instead")>]
val in_channel_to_binary_reader: in_channel -> System.IO.BinaryReader
[<Obsolete("Consider using the corresponding OutChannel.* member instead")>]
val out_channel_to_stream: out_channel -> System.IO.Stream
[<Obsolete("Consider using the the corresponding OutChannel.* member instead")>]
val out_channel_to_text_writer: out_channel -> System.IO.TextWriter
[<Obsolete("Consider using the the corresponding OutChannel.* member instead")>]
val out_channel_to_stream_writer: out_channel -> System.IO.StreamWriter
[<Obsolete("Consider using the the corresponding OutChannel.* member instead")>]
val out_channel_to_binary_writer: out_channel -> System.IO.BinaryWriter


[<OCamlCompatibility("Consider using Microsoft.FSharp.Text.Format<_,_,_,_> instead")>]
type ('a,'b,'c,'d) format4 = Microsoft.FSharp.Text.Format<'a,'b,'c,'d>
[<OCamlCompatibility("Consider using Microsoft.FSharp.Text.Format<_,_,_,_> instead")>]
type ('a,'b,'c) format = Microsoft.FSharp.Text.Format<'a,'b,'c,'c>

/// Throw an Invalid_argument exception
val invalid_arg: string -> 'a


//--------------------------------------------------------------------------
// OCaml path-lookup compatibility. All these constructs are in scope already
// for F# from Microsoft.FSharp.Operators and elsewhere. This module 
// is Microsoft.FSharp.Compatibility.OCaml.Pervasives.Pervasives and is only included 
// to resolve references in OCaml code written "compare" etc.
// We hide these away in the sub-module called "Pervasives" because we don't
// particularly want normal references such as "compare" to resolve to the 
// values in Pervasives.


[<OCamlCompatibility>]
module Pervasives = 
    //--------------------------------------------------------------------------
    // Comparison based on F# term structure and/or calls to System.IComparable

    ///Structural less-than comparison
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (<): 'a -> 'a -> bool
    ///Structural less-than-or-equal comparison
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (<=): 'a -> 'a -> bool
    ///Structural inequality
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (<>): 'a -> 'a -> bool
    ///Structural equality
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (=): 'a -> 'a -> bool
    ///Structural greater-than
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (>): 'a -> 'a -> bool
    ///Structural greater-than-or-equal
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (>=): 'a -> 'a -> bool

    ///Structural comparison
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val compare: 'a -> 'a -> int
    ///Maximum based on structural comparison
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val max: 'a -> 'a -> 'a
    ///Minimum based on structural comparison
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val min: 'a -> 'a -> 'a

    ///The "hash" function is a structural hash function. It is 
    ///designed to return equal hash values for items that are 
    ///equal according to the polymorphic equality 
    ///function Pervasives.(=) (i.e. the standard "=" operator).
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val hash: 'a -> int

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (+)  : int -> int -> int
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (-)  : int -> int -> int
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val ( * ): int -> int -> int
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (/)  : int -> int -> int

    ///Absolute value of the given integer
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val abs : int -> int

    ///Dereference a mutable reference cell
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (!) : 'a ref -> 'a

    ///Assign to a mutable reference cell
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (:=): 'a ref -> 'a -> unit

    ///Create a mutable reference cell
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val ref : 'a -> 'a ref

    /// Throw a 'Failure' exception
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val failwith: string -> 'a

    /// Throw an exception
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val raise: exn -> 'a

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val fst: ('a * 'b) -> 'a
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val snd: ('a * 'b) -> 'b

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val ignore: 'a -> unit
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val not: bool -> bool

    ///Decrement a mutable reference cell containing an integer
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val decr: int ref -> unit

    ///Increment a mutable reference cell containing an integer
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val incr: int ref -> unit
    
#if FX_NO_EXIT
#else
    ///Exit the current hardware isolated process, if security settings permit,
    ///otherwise raise an exception. Calls System.Environment.Exit.
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val exit: int -> 'a   
#endif
    /// Concatenate two strings. The overlaoded operator '+' may also be used.
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (^): string -> string -> string
    /// Concatenate two lists.
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val (@): 'a list -> 'a list -> 'a list

    /// The exception thrown by <c>failure</c> and many other F# functions
    /// A future release of F# may map this exception to a corresponding .NET exception.
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    exception Failure = Microsoft.FSharp.Core.Failure

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val float: int -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val acos: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val asin: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val atan: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val atan2: float -> float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val ceil: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val exp: float -> float

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val floor: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val log: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val log10: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val sqrt: float -> float

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val cos: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val cosh: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val sin: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val sinh: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val tan: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val tanh: float -> float
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val truncate: float -> int

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val ( **  ): float -> float -> float

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val nan: float

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    val infinity: float

    ///The type of pointers to mutable reference cells
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    type 'a ref = Microsoft.FSharp.Core.Ref<'a>

    ///The type of None/Some options
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    type 'a option = Microsoft.FSharp.Core.Option<'a>

    ///The type of simple immutable lists 
    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    type 'a list = Microsoft.FSharp.Collections.List<'a>

    [<OCamlCompatibility("Consider replacing uses of the functions accessible via Pervasives.* with their F# equivalents, usually by deleting 'Pervasives.'")>]
    type exn = System.Exception

