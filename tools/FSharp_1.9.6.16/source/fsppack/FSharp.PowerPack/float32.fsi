//==========================================================================
// (c) Microsoft Corporation 2005-2008.  
//===========================================================================

namespace Microsoft.FSharp.Compatibility

open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Core.Operators

/// ML-like operations on 32-bit System.Single floating point numbers.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<System.Obsolete("Consider using the F# overloaded operators such as 'int' and 'float' to convert numbers")>]
module Float32 = 

    /// Returns the sum of a and b
    val add: a:float32 -> b:float32 -> float32
    /// Returns a divided by b 
    val div: a:float32 -> b:float32 -> float32
    /// Returns a multiplied by b 
    val mul: a:float32 -> b:float32 -> float32
    /// Returns -a
    val neg: a:float32 -> float32
    /// Returns a minus b 
    val sub: a:float32 -> b:float32 -> float32
    /// Compares a and b and returns 1 if a &gt; b, -1 if b &lt; a and 0 if a = b
    val compare: a:float32 -> b:float32 -> int

    /// Converts a 32-bit integer to a 32-bit float
    val of_int: int -> float32
    /// Converts a 32-bit float to a 32-bit integer
    val to_int: float32 -> int

    /// Converts a 32-bit integer to a 32-bit float
    val of_int32: int32 -> float32
    /// Converts a 32-bit float to a 32-bit integer
    val to_int32: float32 -> int32

    /// Converts a 64-bit integer to a 32-bit float
    val of_int64: int64 -> float32
    /// Converts a 32-bit float to a 64-bit integer 
    val to_int64: float32 -> int64

    /// Converts a 64-bit float to a 32-bit float
    val of_float: float -> float32
    /// Converts a 32-bit float to a 64-bit float
    val to_float: float32 -> float

    /// Converts a string to a 32-bit float
    val of_string: string -> float32
    /// Converts a 32-bit float to a string
    val to_string: float32 -> string

    /// Converts a raw 32-bit representation to a 32-bit float
    val of_bits: int32 -> float32
    /// Converts a 32-bit float to raw 32-bit representation 
    val to_bits: float32 -> int32

