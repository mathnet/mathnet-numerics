// (c) Microsoft Corporation 2005-2009.  

namespace Microsoft.FSharp.Math

open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Primitives.Basics
open Microsoft.FSharp.Math
open System
open System.Globalization

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

type IIEEE<'a> =
    inherit IFloating<'a>
    abstract PositiveInfinity : 'a
    abstract NegativeInfinity : 'a
    abstract NaN              : 'a
    abstract EpsilonOne          : 'a
    abstract IsNaN: 'a -> bool 
    abstract IsInfinite : 'a -> bool 

type INormFloat<'a> =
    abstract Norm : 'a -> float
 
module Instances = 
  let Int32Numerics = 
    { new IIntegral<int32> with 
         member dict.Zero = 0
         member dict.One = 1
         member dict.Add(a,b) = a + b
         member dict.Subtract(a,b) = a - b
         member dict.Multiply(a,b) = a * b
         member dict.Negate(a) = - a 
         member dict.Abs(a) = a
         member dict.ToBigInt(a) = new BigInt(a)
         member dict.OfBigInt(a) = 
#if FX_ATLEAST_40
             (BigInt.op_Explicit a : int32)
#else
             BigInt.ToInt32 a
#endif
         member dict.Sign(a) = Math.Sign(a)
         member dict.Modulus(a,b) = a % b
         member dict.Divide(a,b) = a / b
         member dict.DivRem(a,b) = (a / b, a % b)
         member dict.ToString((x:int32),fmt,fmtprovider) = 
                x.ToString(fmt,fmtprovider) 
         member dict.Parse(s,numstyle,fmtprovider) = 
                System.Int32.Parse(s,numstyle,fmtprovider)
      interface INormFloat<int32> with  
         member dict.Norm(x) = float (abs x)
    }
  let Int64Numerics = 
    { new IIntegral<int64> with 
         member dict.Zero =0L
         member dict.One = 1L
         member dict.Add(a,b) = a + b
         member dict.Subtract(a,b) = a - b
         member dict.Multiply(a,b) = a * b
         member dict.Negate(a) = - a 
         member dict.Abs(a) = Math.Abs(a)
         member dict.ToBigInt(a) = new BigInt(a)
         member dict.OfBigInt(a) = 
#if FX_ATLEAST_40
             (BigInt.op_Explicit a : int64)
#else
             BigInt.ToInt64 a
#endif
         member dict.Sign(a) = Math.Sign(a)
         member dict.Modulus(a,b) = a % b
         member dict.Divide(a,b) = a / b
         member dict.DivRem(a,b) = (a / b, a % b)
         member dict.ToString((x:int64),fmt,fmtprovider) = x.ToString(fmt,fmtprovider) 
         member dict.Parse(s,numstyle,fmtprovider) = System.Int64.Parse(s,numstyle,fmtprovider)
      interface INormFloat<int64> with
         member dict.Norm(x) = float (Math.Abs x)
    }
  let FloatNumerics = 
    { new IIEEE<float> with 
         member dict.Zero = 0.0
         member dict.One =  1.0
         member dict.Add(a,b) =  a + b
         member dict.Subtract(a,b) = a - b
         member dict.Multiply(a,b) = a * b
         member dict.PositiveInfinity = Double.PositiveInfinity
         member dict.NegativeInfinity = Double.NegativeInfinity
         member dict.NaN = Double.NaN
         member dict.EpsilonOne = 0x3CB0000000000000LF
         member dict.IsInfinite(a) = Double.IsInfinity(a)
         member dict.IsNaN(a) = Double.IsNaN(a)
         member dict.Pi = Math.PI
         member dict.Reciprocal(a) = 1.0/a
         member dict.Abs(a) = Math.Abs(a)
         member dict.Sign(a) = Math.Sign(a)
         member dict.Asin(a) = Math.Asin(a)
         member dict.Acos(a) = Math.Acos(a)
         member dict.Atan(a) = Math.Atan(a)
         member dict.Atan2(a,b) = Math.Atan2(a,b)
         member dict.Tanh(a) = Math.Tanh(a)
         member dict.Tan(a) = Math.Tan(a)
         member dict.Sqrt(a) = Math.Sqrt(a)
         member dict.Sinh(a) = Math.Sinh(a)
         member dict.Cosh(a) = Math.Cosh(a)
         member dict.Sin(a) = Math.Sin(a)
         member dict.Cos(a) = Math.Cos(a)
         member dict.LogN(a,n) = 
#if FX_NO_LOGN
             raise (System.NotSupportedException("this operation is not supported on this platform"))
#else
             Math.Log(a,n)
#endif
         member dict.Log(a) = Math.Log(a)
         member dict.Exp(a) = Math.Exp(a)
         member dict.Negate(a) = -a 
         member dict.Divide(a,b) = a / b
         member dict.ToString((x:float),fmt,fmtprovider) = x.ToString(fmt,fmtprovider) 
         member dict.Parse(s,numstyle,fmtprovider) = System.Double.Parse(s,numstyle,fmtprovider)
      interface INormFloat<float> with
          member dict.Norm(x) = float (Math.Abs x)
    }
  let Float32Numerics = 
    { new IFractional<float32> with
           member dict.Zero = 0.0f
           member dict.One =  1.0f
           member dict.Add(a,b) = a + b
           member dict.Subtract(a,b) = a - b
           member dict.Multiply(a,b) = a * b
           member dict.Negate(a) = -a 
           member dict.Reciprocal(a) = 1.0f/a
           member dict.Sign(a) = Math.Sign(a)
           member dict.Abs(a) = Math.Abs(a)
           member dict.Divide(a,b) = a / b
           member dict.ToString((x:float32),fmt,fmtprovider) = x.ToString(fmt,fmtprovider) 
           member dict.Parse(s,numstyle,fmtprovider) = System.Single.Parse(s,numstyle,fmtprovider)
       interface INormFloat<float32> with  
           member dict.Norm(x) = float (Math.Abs x)
    }

  let BigNumNumerics = 
    { new IFractional<bignum> with 
         member dict.Zero = BigNum.Zero
         member dict.One = BigNum.One
         member dict.Add(a,b)      = a + b
         member dict.Subtract(a,b) = a - b
         member dict.Multiply(a,b) = a * b
         member dict.Divide(a,b)   = a / b
         member dict.Abs(a) = BigNum.Abs a
         member dict.Sign(a) = a.Sign
         member dict.Negate(a) = - a 
         member dict.Reciprocal(a) = BigNum.One / a 
         // Note, this ignores fmt, fmtprovider
         member dict.ToString((x:bignum),fmt,fmtprovider) = x.ToString()
         // Note, this ignroes numstyle, fmtprovider
         member dict.Parse(s,numstyle,fmtprovider) = BigNum.Parse(s)

      interface INormFloat<bignum> with
         member dict.Norm(x) = float (BigNum.Abs x)
    }       

  let BigIntNumerics = 
    let ZeroI = new BigInt(0)
    { new IIntegral<_> with 
         member dict.Zero = BigInt.Zero
         member dict.One =  BigInt.One
         member dict.Add(a,b) = a + b
         member dict.Subtract(a,b) = a - b
         member dict.Multiply(a,b) = a * b
         member dict.Divide(a,b) = a / b
         member dict.Negate(a) = -a 
         member dict.Modulus(a,b) = a % b
         member dict.DivRem(a,b) = BigInt.DivRem (a,b)
         member dict.Sign(a) = a.Sign
         member dict.Abs(a) = abs a
         member dict.ToBigInt(a) = a 
         member dict.OfBigInt(a) = a 
         
         member dict.ToString(x,fmt,fmtprovider) = 
#if FX_ATLEAST_40
             x.ToString(fmt,fmtprovider) 
#else
             // Note: this ignores fmt and fmtprovider
             x.ToString() 
#endif
         // Note: this ignores fmt and fmtprovider
         member dict.Parse(s,numstyle,fmtprovider) = 
#if FX_ATLEAST_40
             BigInt.Parse(s,numstyle,fmtprovider)
#else
             BigInt.Parse(s)
#endif

      interface INormFloat<bigint> with  
         member dict.Norm(x) = 
#if FX_ATLEAST_40
             (BigInt.op_Explicit (BigInt.Abs x) : float)
#else
             float (abs x)
#endif
    }       

