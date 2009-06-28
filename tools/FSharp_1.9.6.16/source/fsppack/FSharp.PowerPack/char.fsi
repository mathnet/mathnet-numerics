//==========================================================================
// (c) Microsoft Corporation 2005-2009.  
//==========================================================================

#if INTERNALIZED_POWER_PACK
namespace Internal.Utilities.OCaml
#else
namespace Microsoft.FSharp.Compatibility
#endif

/// Unicode characters, i.e. the <c>System.Char</c> type.  see also the operations
/// in <c>System.Char</c> and the <c>System.Text.Encoding</c> interfaces if necessary.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<OCamlCompatibility("Consider using the F# overloaded operators such as 'char' and 'int' to convert basic types")>]
[<RequireQualifiedAccess>]
module Char = 

    /// Converts the value of the specified 32-bit signed integer to its equivalent Unicode character
    val chr: i:int -> char
    /// Converts the value of the specified Unicode character to the equivalent 32-bit signed integer
    val code: c:char -> int
    /// Compares a and b and returns 1 if a &gt; b, -1 if b &lt; a and 0 if a = b
    val compare: a:char -> b:char -> int
    /// Converts the value of a Unicode character to its lowercase equivalent
    val lowercase: char -> char
    /// Converts the value of a Unicode character to its uppercase equivalent
    val uppercase: char -> char 