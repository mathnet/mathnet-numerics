// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/math/q.fsi
// (c) Microsoft Corporation 2005-2009.

(* NOTE : This signature file is necessary now _only_ to hide the case constructors for the BigRational type. *)

namespace MathNet.Numerics

#if NOSYSNUMERICS
#else

open System
open System.Numerics

[<Sealed>]
type BigRational =
    interface System.IComparable

    override ToString : unit -> string
    override GetHashCode : unit -> int

    member IsNegative: bool
    member IsPositive: bool
    member IsInteger: bool
    member IsZero: bool
    member IsOne: bool

    member Numerator : BigInteger
    member Denominator : BigInteger

    member Sign : int
    member StructuredDisplayString : string

    static member Zero : BigRational
    static member One : BigRational

    static member ( + ) : BigRational * BigRational -> BigRational
    static member ( * ) : BigRational * BigRational -> BigRational
    static member ( - ) : BigRational * BigRational -> BigRational
    static member ( / ) : BigRational * BigRational -> BigRational
    static member ( ~- ): BigRational -> BigRational
    static member ( ~+ ): BigRational -> BigRational

    static member op_Equality : BigRational * BigRational -> bool
    static member op_Inequality : BigRational * BigRational -> bool
    static member op_LessThan: BigRational * BigRational -> bool
    static member op_LessThanOrEqual: BigRational * BigRational -> bool
    static member op_GreaterThan: BigRational * BigRational -> bool
    static member op_GreaterThanOrEqual: BigRational * BigRational -> bool

    static member op_Explicit : BigRational -> BigInteger
    static member op_Explicit : BigRational -> int
    static member op_Explicit : BigRational -> float

    static member Abs : BigRational -> BigRational
    static member Reciprocal : BigRational -> BigRational
    static member Pow : BigRational * int -> BigRational
    static member PowN : BigRational * int -> BigRational

    static member Parse: string -> BigRational

    static member FromInt : int -> BigRational
    static member FromBigInt : BigInteger -> BigRational

    static member FromIntFraction : int * int -> BigRational
    static member FromBigIntFraction : BigInteger * BigInteger -> BigRational

    static member ToDouble: BigRational -> float
    static member ToBigInt: BigRational -> BigInteger
    static member ToInt32 : BigRational -> int

[<RequireQualifiedAccess>]
module NumericLiteralN =
    val FromZero : unit -> BigRational
    val FromOne : unit -> BigRational
    val FromInt32 : int32 -> BigRational
    val FromInt64 : int64 -> BigRational
    val FromString : string -> BigRational

type BigNum = BigRational
type bignum = BigRational

#endif
