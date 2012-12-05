// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/math/q.fsi
// (c) Microsoft Corporation 2005-2009.

namespace MathNet.Numerics

    open System
    open System.Numerics

    /// The type of arbitrary-sized rational numbers
    [<Sealed>]
    type BigRational =
        /// Return the sum of two rational numbers
        static member ( + ) : BigRational * BigRational -> BigRational
        /// Return the product of two rational numbers
        static member ( * ) : BigRational * BigRational -> BigRational
        /// Return the difference of two rational numbers
        static member ( - ) : BigRational * BigRational -> BigRational
        /// Return the ratio of two rational numbers
        static member ( / ) : BigRational * BigRational -> BigRational
        /// Return the negation of a rational number
        static member ( ~- ): BigRational          -> BigRational
        /// Return the given rational number
        static member ( ~+ ): BigRational          -> BigRational

        override ToString: unit -> string
        override GetHashCode: unit -> int
        interface System.IComparable

        /// Get zero as a rational number
        static member Zero : BigRational
        /// Get one as a rational number
        static member One : BigRational
        /// This operator is for use from other .NET languages
        static member op_Equality : BigRational * BigRational -> bool
        /// This operator is for use from other .NET languages
        static member op_Inequality : BigRational * BigRational -> bool
        /// This operator is for use from other .NET languages
        static member op_LessThan: BigRational * BigRational -> bool
        /// This operator is for use from other .NET languages
        static member op_GreaterThan: BigRational * BigRational -> bool
        /// This operator is for use from other .NET languages
        static member op_LessThanOrEqual: BigRational * BigRational -> bool
        /// This operator is for use from other .NET languages
        static member op_GreaterThanOrEqual: BigRational * BigRational -> bool

        /// Return a boolean indicating if this rational number is strictly negative
        member IsNegative: bool
        /// Return a boolean indicating if this rational number is strictly positive
        member IsPositive: bool

        /// Return the numerator of the normalized rational number
        member Numerator: BigInteger
        /// Return the denominator of the normalized rational number
        member Denominator: BigInteger

        member StructuredDisplayString : string

        /// Return the absolute value of a rational number
        static member Abs : BigRational -> BigRational
        /// Return the sign of a rational number; 0, +1 or -1
        member Sign : int
        /// Return the result of raising the given rational number to the given power
        static member PowN : BigRational * int -> BigRational
        /// Return the result of converting the given integer to a rational number
        static member FromInt : int         -> BigRational
        /// Return the result of converting the given big integer to a rational number
        static member FromBigInt : BigInteger      -> BigRational
        /// Return the result of converting the given rational number to a floating point number
        static member ToDouble: BigRational -> float
        /// Return the result of converting the given rational number to a big integer
        static member ToBigInt: BigRational -> BigInteger
        /// Return the result of converting the given rational number to an integer
        static member ToInt32 : BigRational -> int
        /// Return the result of converting the given rational number to a floating point number
        static member op_Explicit : BigRational -> float
        /// Return the result of converting the given rational number to a big integer
        static member op_Explicit : BigRational -> BigInteger
        /// Return the result of converting the given rational number to an integer
        static member op_Explicit : BigRational -> int
        /// Return the result of converting the string to a rational number
        static member Parse: string -> BigRational

    type BigNum = BigRational

    type bignum = BigRational

    [<RequireQualifiedAccess>]
    module NumericLiteralN =
        val FromZero : unit -> BigRational
        val FromOne : unit -> BigRational
        val FromInt32 : int32 -> BigRational
        val FromInt64 : int64 -> BigRational
        val FromString : string -> BigRational
