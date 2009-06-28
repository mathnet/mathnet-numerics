//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

/// This namespace contains numerical and mathematical types such as arbitrarily sized integers
namespace Microsoft.FSharp.Math

    open System
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core

#if FX_ATLEAST_40
    type BigInt = System.Numerics.BigInteger
    type bigint = System.Numerics.BigInteger
#else
    /// The type of arbitrary-sized integers
    [<Struct>]
    [<StructuralEquality(false); StructuralComparison(false)>]
    type BigInt =
        /// Return the sum of two big integers
        static member ( + )      : x:BigInt * y:BigInt -> BigInt
        /// Return the modulus of big integers
        static member ( % )      : x:BigInt * y:BigInt -> BigInt
        /// Return the product of big integers
        static member ( * )      : x:BigInt * y:BigInt -> BigInt
        /// Return the difference of big integers
        static member ( - )      : x:BigInt * y:BigInt -> BigInt
        /// Return the ratio of big integers
        static member ( / )      : x:BigInt * y:BigInt -> BigInt
        /// Return the negation of a big integer
        static member (~-)       : x:BigInt -> BigInt
        /// Return the given big integer
        static member (~+)       : x:BigInt -> BigInt
        /// Generate a range of big integers
        static member (..)       : start:BigInt * finish:BigInt -> seq<BigInt>
        /// Generate a range of big integers, with a step
        static member (.. ..)    : start:BigInt * step:BigInt * finish:BigInt -> seq<BigInt>
        /// Convert a big integer to a floating point number
        static member ToDouble   : x:BigInt -> float
        /// Convert a big integer to a 64-bit signed integer
        static member ToInt64    : x:BigInt -> int64
        /// Convert a big integer to a 32-bit signed integer
        static member ToInt32    : x:BigInt -> int32
        /// Parse a big integer from a string format
        static member Parse    : text:string -> BigInt
        /// Return the sign of a big integer: 0, +1 or -1
        member Sign    : int
        /// Compute the factorial function as a big integer
        static member Factorial : x:BigInt -> BigInt
        /// Compute the ratio and remainder of two big integers
        static member DivRem : x:BigInt * y:BigInt -> BigInt * BigInt

        /// This operator is for use from other .NET languages
        static member op_LessThan           : x:BigInt * y:BigInt -> bool
        /// This operator is for use from other .NET languages
        static member op_LessThanOrEqual    : x:BigInt * y:BigInt -> bool
        /// This operator is for use from other .NET languages
        static member op_GreaterThan        : x:BigInt * y:BigInt -> bool
        /// This operator is for use from other .NET languages
        static member op_GreaterThanOrEqual : x:BigInt * y:BigInt -> bool
        /// This operator is for use from other .NET languages
        static member op_Equality             : x:BigInt * y:BigInt -> bool
        /// This operator is for use from other .NET languages
        static member op_Inequality           : x:BigInt * y:BigInt -> bool

        /// Return the greatest common divisor of two big integers
        static member Gcd : x:BigInt * y:BigInt -> BigInt
        /// Return n^m for two big integers
        static member Pow    : x:BigInt * y:BigInt -> BigInt
        /// Compute the absolute value of a big integer 
        static member Abs    : x:BigInt -> BigInt
        /// Get the big integer for zero
        static member Zero    : BigInt 
        /// Get the big integer for one
        static member One     : BigInt 


        /// Return true if a big integer is 'zero'
        member IsZero : bool
        /// Return true if a big integer is 'one'
        member IsOne : bool
        interface System.IComparable
        override Equals : obj -> bool
        override GetHashCode : unit -> int
        override ToString : unit -> string

        [<OverloadID("new_int")>]
        /// Construct a BigInt value for the given integer
        new : x:int -> BigInt
        [<OverloadID("new_int64")>]
        /// Construct a BigInt value for the given 64-bit integer
        new : x:int64 -> BigInt

    type bigint = BigInt

namespace Microsoft.FSharp.Core

    open Microsoft.FSharp.Math
    
#if FX_ATLEAST_40
    [<System.Obsolete("The 'bigint' type abbreviation has been moved to 'Microsoft.FSharp.Math'. Please use 'open Microsoft.FSharp.Math' to use this abbreviation and ensure you reference 'System.dll'",true)>]
    type bigint 
#else
    /// An abbreviation for the type <c>Microsoft.FSharp.Math.BigInt</c>
    [<System.Obsolete("The 'bigint' type abbreviation has been moved to 'Microsoft.FSharp.Math'. Please use 'open Microsoft.FSharp.Math' to use this abbreviation")>]
    type bigint = Microsoft.FSharp.Math.BigInt
#endif

    [<AutoOpen>]
    /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
    module NumericLiterals =

        /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
        module NumericLiteralI = 
#if FX_ATLEAST_40
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val inline FromZero : value:unit -> 'T
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val inline FromOne : value:unit -> 'T
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val inline FromInt32 : value:int32 -> 'T
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val inline FromInt64 : value:int64 -> 'T
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val inline FromString : text:string -> 'T
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val FromInt64Dynamic : value:int64 -> obj
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val FromStringDynamic : text:string -> obj
#else            
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val FromZero : unit -> bigint
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val FromOne : unit -> bigint
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val FromInt32 : value:int32 -> bigint
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val FromInt64 : value:int64 -> bigint
            /// Provides a default implementations of F# numeric literal syntax  for literals fo the form 'dddI' 
            val FromString : text:string -> bigint
            

        /// Provide a default implementation of the F# numeric literal syntax 'dddN' 
        module NumericLiteralN = 
            [<System.Obsolete("Numeric literals of the form dddN are now only supported by an explicit definition of a NumericLiteralN module containing FromZero, FromOne, FromInt32, FromInt64 and FromString methods for an appropriate numeric type. One such implementation can be found in the FSharp.PowerPack.dll",true)>]
            val FromZero : unit -> 'a
            [<System.Obsolete("Numeric literals of the form dddN are now only supported by an explicit definition of a NumericLiteralN module containing FromZero, FromOne, FromInt32, FromInt64 and FromString methods for an appropriate numeric type. One such implementation can be found in the FSharp.PowerPack.dll",true)>]
            val FromOne : unit -> 'a
            [<System.Obsolete("Numeric literals of the form dddN are now only supported by an explicit definition of a NumericLiteralN module containing FromZero, FromOne, FromInt32, FromInt64 and FromString methods for an appropriate numeric type. One such implementation can be found in the FSharp.PowerPack.dll",true)>]
            val FromInt32 : int32 -> 'a
            [<System.Obsolete("Numeric literals of the form dddN are now only supported by an explicit definition of a NumericLiteralN module containing FromZero, FromOne, FromInt32, FromInt64 and FromString methods for an appropriate numeric type. One such implementation can be found in the FSharp.PowerPack.dll",true)>]
            val FromInt64 : int64 -> 'a
            [<System.Obsolete("Numeric literals of the form dddN are now only supported by an explicit definition of a NumericLiteralN module containing FromZero, FromOne, FromInt32, FromInt64 and FromString methods for an appropriate numeric type. One such implementation can be found in the FSharp.PowerPack.dll",true)>]
            val FromString : string -> 'a
#endif            

#endif
        
