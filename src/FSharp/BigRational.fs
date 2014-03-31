// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/math/q.fs
// (c) Microsoft Corporation. All rights reserved

#nowarn "44"  // OK to use the "compiler only" function RangeGeneric
#nowarn "52"  // The value has been copied to ensure the original is not mutated by this operation

namespace MathNet.Numerics

#if NOSYSNUMERICS
#else

open System
open System.Numerics
open System.Globalization


// invariants: (p,q) in lowest form, q >= 0
[<Sealed>]
type BigRationalLarge (p : BigInteger, q : BigInteger) =
    //
    member __.IsNegative =
        sign p < 0

    //
    member __.IsPositive =
        sign p > 0

    //
    member __.Numerator = p

    //
    member __.Denominator = q

    //
    member __.Sign =
        sign p

    override __.GetHashCode () =
        // This hash code must be identical to the hash for BigInteger when the numbers coincide.
        if q.IsOne then p.GetHashCode ()
        else (p.GetHashCode () <<< 3) + q.GetHashCode ()

    override __.ToString () =
        if q.IsOne then
            p.ToString ()
        else
            p.ToString () + "/" + q.ToString ()

    //
    static member Equals (x : BigRationalLarge, y : BigRationalLarge) =
        // normal form, so structural equality
        x.Numerator = y.Numerator && x.Denominator = y.Denominator

    //
    static member Compare (x : BigRationalLarge, y : BigRationalLarge) =
        compare (x.Numerator * y.Denominator) (y.Numerator * x.Denominator)

    //
    static member ToDouble (num : BigRationalLarge) =
        float num.Numerator / float num.Denominator

    //
    static member Normalize (p : BigInteger, q : BigInteger) =
        if q.IsZero then
            (* throw for any x/0 *)
            raise <| System.DivideByZeroException ()
        elif q.IsOne then
            BigRationalLarge (p, q)
        else
            let k = BigInteger.GreatestCommonDivisor (p, q)
            let p = p / k
            let q = q / k
            if sign q < 0 then
                BigRationalLarge (-p, -q)
            else
                BigRationalLarge (p, q)

    //
    static member Create (p : int, q : int) =
        BigRationalLarge.Normalize (bigint p, bigint q)

    //
    static member Create (p, q) =
        BigRationalLarge.Normalize (p, q)

    /// Return the given rational number
    static member (~+) (n1 : BigRationalLarge) = n1

    /// Return the negation of a rational number
    static member (~-) (num : BigRationalLarge) =
        // still coprime, bq >= 0
        BigRationalLarge (-num.Numerator, num.Denominator)

    /// Return the sum of two rational numbers
    static member (+) (x : BigRationalLarge, y : BigRationalLarge) =
        BigRationalLarge.Normalize ((x.Numerator * y.Denominator) + (y.Numerator * x.Denominator), x.Denominator * y.Denominator)

    /// Return the difference of two rational numbers
    static member (-) (x : BigRationalLarge, y : BigRationalLarge) =
        BigRationalLarge.Normalize ((x.Numerator * y.Denominator) - (y.Numerator * x.Denominator), x.Denominator * y.Denominator)

    /// Return the product of two rational numbers
    static member (*) (x : BigRationalLarge, y : BigRationalLarge) =
        BigRationalLarge.Normalize (x.Numerator * y.Numerator, x.Denominator * y.Denominator)

    /// Return the ratio of two rational numbers
    static member (/) (x : BigRationalLarge, y : BigRationalLarge) =
        BigRationalLarge.Normalize (x.Numerator * y.Denominator, x.Denominator * y.Numerator)

    //
    static member Reciprocal (num : BigRationalLarge) =
        BigRationalLarge.Normalize (num.Denominator, num.Numerator)

    //
    static member Pow (num : BigRationalLarge, n : int) =
        // p,q powers still coprime
        if n < 0 then BigRationalLarge.Normalize (BigInteger.Pow (num.Denominator, -n), BigInteger.Pow (num.Numerator, -n))
        else BigRationalLarge (BigInteger.Pow (num.Numerator, n), BigInteger.Pow (num.Denominator, n))

    //
    static member FromBigInteger z =
        BigRationalLarge.Create (z, BigInteger.One)

    //
    static member FromInt32 n =
        BigRationalLarge.Create (n, 1)

    /// Returns the integer part of a rational number.
    static member ToBigInteger (num : BigRationalLarge) =
        // have p = d.q + r, |r| < |q|
        let d, r = BigInteger.DivRem (num.Numerator, num.Denominator)

        if r < BigInteger.Zero then
            // p = (d-1).q + (r+q)
            d - BigInteger.One
        else
            // p = d.q + r
            d

    //
    static member Parse (str : string) =
        let len = str.Length
        if len = 0 then
            invalidArg "str" "empty string"

        let j = str.IndexOf '/'
        if j >= 0 then
            let p = BigInteger.Parse (str.Substring (0, j))
            let q = BigInteger.Parse (str.Substring (j + 1, len - j - 1))
            BigRationalLarge.Create (p, q)
        else
            let p = BigInteger.Parse str
            BigRationalLarge.Create (p, BigInteger.One)

    override this.Equals (that : obj) =
        match that with
        | :? BigRationalLarge as that ->
            BigRationalLarge.Equals (this, that)
        | _ -> false

    interface System.IComparable with
        member this.CompareTo (obj : obj) =
            match obj with
            | :? BigRationalLarge as other ->
                BigRationalLarge.Compare (this, other)
            | _ ->
                invalidArg "obj" "the object does not have the correct type"

    interface System.IComparable<BigRationalLarge> with
        member this.CompareTo other =
            BigRationalLarge.Compare (this, other)


/// The type of arbitrary-sized rational numbers.
[<CustomEquality; CustomComparison>]
[<StructuredFormatDisplay("{StructuredDisplayString}N")>]
type BigRational =
    //
    | Z of BigInteger
    //
    | Q of BigRationalLarge

    /// Return the numerator of the normalized rational number
    member this.Numerator =
        match this with
        | Z z -> z
        | Q q -> q.Numerator

    /// Return the denominator of the normalized rational number
    member this.Denominator =
        match this with
        | Z _ -> BigInteger.One
        | Q q -> q.Denominator

    /// Return a boolean indicating if this rational number is strictly negative
    member this.IsNegative =
        match this with
        | Z z -> sign z < 0
        | Q q -> q.IsNegative

    /// Return a boolean indicating if this rational number is strictly positive
    member this.IsPositive =
        match this with
        | Z z -> sign z > 0
        | Q q -> q.IsPositive

    /// Indicates whether this number is an integer; denominator is one
    member this.IsInteger =
        match this with
        | Z z -> true
        | Q q -> q.Denominator.IsOne

    /// Indicates whether this number is equal to zero.
    member this.IsZero =
        match this with
        | Z z -> z.IsZero
        | Q q -> q.Numerator.IsZero

    /// Indicates whether this number is equal to one.
    member this.IsOne =
        match this with
        | Z z -> z.IsOne
        | Q q -> q.Denominator.IsOne && q.Numerator.IsOne

    /// Return the sign of a rational number; 0, +1 or -1
    member this.Sign =
        if this.IsNegative then -1
        elif this.IsPositive then 1
        else 0

    override this.Equals (obj : obj) =
        match obj with
        | :? BigRational as other ->
            BigRational.(=)(this, other)
        | _ -> false

    override this.GetHashCode () =
        // nb. Q and Z hash codes must match up - see notes above
        match this with
        | Z z -> z.GetHashCode ()
        | Q q -> q.GetHashCode ()

    override this.ToString () =
        match this with
        | Z z ->
            z.ToString ()
        | Q q ->
            q.ToString ()

    member this.StructuredDisplayString =
        this.ToString ()

    /// Return the result of converting the string to a rational number
    static member Parse (str : string) =
        Q (BigRationalLarge.Parse str)

    // TODO : Optimize this by implementing a proper comparison function (so we only do one comparison instead of two).
    interface System.IComparable with
        member this.CompareTo (obj : obj) =
            match obj with
            | :? BigRational as other ->
                if BigRational.(<)(this, other) then -1
                elif BigRational.(=)(this, other) then 0
                else 1
            | _ ->
                invalidArg "obj" "The objects are not comparable."

    /// Return the result of converting the given integer to a rational number
    static member FromInt (x : int) =
        Z (bigint x)

    /// Return the result of converting the given big integer to a rational number
    static member FromBigInt x = Z x

    static member FromIntFraction (numerator: int, denominator: int) =
        Q (BigRationalLarge.Create (numerator, denominator))

    static member FromBigIntFraction (numerator: BigInteger, denominator: BigInteger) =
        Q (BigRationalLarge.Create (numerator, denominator))

    /// Get zero as a rational number
    static member Zero =
        BigRational.FromInt 0

    /// Get one as a rational number
    static member One =
        BigRational.FromInt 1

    /// Return the sum of two rational numbers
    static member ( + ) (n1, n2) =
        match n1, n2 with
        | Z z, Z zz ->
            Z (z + zz)
        | Q q, Q qq ->
            Q (q + qq)
        | Z z, Q qq ->
            Q (BigRationalLarge.FromBigInteger z + qq)
        | Q q, Z zz ->
            Q (q  + BigRationalLarge.FromBigInteger zz)

    /// Return the difference of two rational numbers
    static member ( - ) (n1, n2) =
        match n1, n2 with
        | Z z, Z zz ->
            Z (z - zz)
        | Q q, Q qq ->
            Q (q - qq)
        | Z z, Q qq ->
            Q (BigRationalLarge.FromBigInteger z - qq)
        | Q q, Z zz ->
            Q (q  - BigRationalLarge.FromBigInteger zz)

    /// Return the product of two rational numbers
    static member ( * ) (n1, n2) =
        match n1, n2 with
        | Z z, Z zz ->
            Z (z * zz)
        | Q q, Q qq ->
            Q (q * qq)
        | Z z, Q qq ->
            Q (BigRationalLarge.FromBigInteger z * qq)
        | Q q, Z zz ->
            Q (q  * BigRationalLarge.FromBigInteger zz)

    /// Return the ratio of two rational numbers
    static member ( / ) (n1, n2) =
        match n1, n2 with
        | Z z, Z zz ->
            Q (BigRationalLarge.Create (z, zz))
        | Q q, Q qq ->
            Q (q / qq)
        | Z z, Q qq ->
            Q (BigRationalLarge.FromBigInteger z / qq)
        | Q q, Z zz ->
            Q (q  / BigRationalLarge.FromBigInteger zz)

    /// Return the negation of a rational number
    static member ( ~- ) n =
        match n with
        | Z z -> Z (-z)
        | Q q -> Q (-q)

    /// Return the given rational number
    static member ( ~+ ) (n : BigRational) = n

    /// This operator is for use from other .NET languages
    static member op_Equality (n, nn) =
        match n,nn with
        | Z z, Z zz ->
            BigInteger.(=) (z,zz)
        | Q q, Q qq ->
            BigRationalLarge.Equals (q, qq)
        | Z z, Q qq ->
            BigRationalLarge.Equals (BigRationalLarge.FromBigInteger z, qq)
        | Q q, Z zz ->
            BigRationalLarge.Equals (q, BigRationalLarge.FromBigInteger zz)

    /// This operator is for use from other .NET languages
    static member op_Inequality (n, nn) =
        not <| BigRational.op_Equality (n, nn)

    /// This operator is for use from other .NET languages
    static member op_LessThan (n, nn) =
        match n, nn with
        | Z z, Z zz ->
            z < zz
        | Q q, Q qq ->
            q < qq
        | Z z, Q qq ->
            BigRationalLarge.FromBigInteger z < qq
        | Q q, Z zz ->
            q < BigRationalLarge.FromBigInteger zz

    /// This operator is for use from other .NET languages
    static member op_LessThanOrEqual (n, nn) =
        match n, nn with
        | Z z, Z zz ->
            z <= zz
        | Q q, Q qq ->
            q <= qq
        | Z z, Q qq ->
            BigRationalLarge.FromBigInteger z <= qq
        | Q q, Z zz ->
            q <= BigRationalLarge.FromBigInteger zz

    /// This operator is for use from other .NET languages
    static member op_GreaterThan (n, nn) =
        match n, nn with
        | Z z, Z zz ->
            z > zz
        | Q q, Q qq ->
            q > qq
        | Z z, Q qq ->
            BigRationalLarge.FromBigInteger z > qq
        | Q q, Z zz ->
            q > BigRationalLarge.FromBigInteger zz

    /// This operator is for use from other .NET languages
    static member op_GreaterThanOrEqual (n, nn) =
        match n, nn with
        | Z z, Z zz ->
            z >= zz
        | Q q, Q qq ->
            q >= qq
        | Z z, Q qq ->
            BigRationalLarge.FromBigInteger z >= qq
        | Q q, Z zz ->
            q >= BigRationalLarge.FromBigInteger zz

    /// Return the absolute value of a rational number
    static member Abs (n : BigRational) =
        if n.IsNegative then -n else n

    /// Returns the multiplicative inverse of a rational number
    static member Reciprocal (n) =
        match n with
        | Z z ->
            Q (BigRationalLarge.Create (BigInteger.One, z))
        | Q q ->
            Q (BigRationalLarge.Reciprocal q)

    /// Return the result of raising the given rational number to the given power
    static member Pow (n, i : int) =
        match n with
        | Z z when i > 0 ->
            Z (BigInteger.Pow (z, i))
        | Z z ->
            Q (BigRationalLarge.Pow (BigRationalLarge.FromBigInteger z, i))
        | Q q ->
            Q (BigRationalLarge.Pow (q, i))

    [<Obsolete("Use Pow instead, which is compatible with the ** operator. Will be removed in a future release.")>]
    static member PowN (n, i : int) = BigRational.Pow(n, i)


    /// Return the result of converting the given rational number to a floating point number
    static member ToDouble (n : BigRational) =
        match n with
        | Z z ->
            float z
        | Q q ->
            BigRationalLarge.ToDouble q

    /// Return the result of converting the given rational number to a big integer
    static member ToBigInt (n : BigRational) =
        match n with
        | Z z -> z
        | Q q ->
            BigRationalLarge.ToBigInteger q

    /// Return the result of converting the given rational number to an integer
    static member ToInt32 (n : BigRational) =
        match n with
        | Z z ->
            int z
        | Q q ->
            int (BigRationalLarge.ToBigInteger q)

    /// Return the result of converting the given rational number to an integer
    static member op_Explicit (n : BigRational) =
        BigRational.ToInt32 n

    /// Return the result of converting the given rational number to a big integer
    static member op_Explicit (n : BigRational) =
        BigRational.ToBigInt n

    /// Return the result of converting the given rational number to a floating point number
    static member op_Explicit (n : BigRational) =
        BigRational.ToDouble n

//
[<RequireQualifiedAccess>]
module NumericLiteralN =
    let private zero = BigRational.Zero
    let private one = BigRational.One

    //
    let FromZero () = zero

    //
    let FromOne () = one

    //
    let FromInt32 x =
        BigRational.FromInt x

    //
    let FromInt64 (x : int64) =
        BigInteger (x)
        |> BigRational.FromBigInt

    //
    let FromString str =
        BigRational.Parse str


//
type BigNum = BigRational
//
type bignum = BigRational

#endif
