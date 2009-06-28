// (c) Microsoft Corporation 2005-2009. 
namespace Microsoft.FSharp.Math

open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Microsoft.FSharp.Math
open System

// A type-class for numeric types
type INumeric<'a> =
    abstract Zero: 'a
    abstract One: 'a
    abstract Add: 'a * 'a -> 'a
    abstract Subtract: 'a * 'a -> 'a
    abstract Multiply : 'a * 'a -> 'a
    abstract Negate : 'a -> 'a
    abstract Sign : 'a -> int
    abstract Abs : 'a -> 'a    
    abstract ToString : 'a * string * System.IFormatProvider -> string
    abstract Parse : string * System.Globalization.NumberStyles * System.IFormatProvider -> 'a

type IIntegral<'a> =
    inherit INumeric<'a>
    abstract Modulus: 'a * 'a -> 'a
    abstract Divide : 'a * 'a -> 'a
    abstract DivRem : 'a * 'a -> 'a * 'a
    abstract ToBigInt : 'a -> bigint
    abstract OfBigInt : bigint -> 'a
  
type IFractional<'a> =
    inherit INumeric<'a>
    abstract Reciprocal : 'a -> 'a
    abstract Divide : 'a * 'a -> 'a

// Suggestion: IReal (since transcendentals are added here).
type IFloating<'a> =
    inherit IFractional<'a>
    abstract Pi : 'a
    abstract Exp : 'a -> 'a
    abstract Log : 'a -> 'a
    abstract Sqrt : 'a -> 'a
    abstract LogN : 'a * 'a -> 'a
    abstract Sin : 'a -> 'a
    abstract Cos : 'a -> 'a
    abstract Tan : 'a -> 'a
    abstract Asin : 'a -> 'a
    abstract Acos : 'a -> 'a
    abstract Atan : 'a -> 'a
    abstract Atan2 : 'a * 'a -> 'a
    abstract Sinh : 'a -> 'a
    abstract Cosh : 'a -> 'a
    abstract Tanh : 'a -> 'a

type INormFloat<'a> =
    abstract Norm : 'a -> float
  
// Direct access to IEEE encoding not easy on .NET
type IIEEE<'a> =
    inherit IFloating<'a>
    abstract PositiveInfinity : 'a
    abstract NegativeInfinity : 'a
    abstract NaN              : 'a
    abstract EpsilonOne       : 'a

    abstract IsNaN: 'a -> bool 
    abstract IsInfinite : 'a -> bool 
    //abstract IsDenormalized   : 'a -> bool 
    //abstract IsNegativeZero   : 'a -> bool 
    //abstract IsIEEE           : 'a -> bool 


module Instances =
    val Float32Numerics  : IFractional<float32> 
    val FloatNumerics    : IIEEE<float>
    val Int32Numerics    : IIntegral<int32>
    val Int64Numerics    : IIntegral<int64>
    val BigIntNumerics   : IIntegral<bigint>
    val BigNumNumerics   : IFractional<bignum>  



