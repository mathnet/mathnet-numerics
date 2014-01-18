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


[<AutoOpen>]
module private BigRationalLargeImpl =
    let ZeroI = BigInteger (0)
    let OneI = BigInteger (1)
    let bigint (x : int) = BigInteger (x)
    let ToDoubleI (x : BigInteger) = float x
    let ToInt32I (x : BigInteger) = int32 x


[<CustomEquality; CustomComparison>]
type BigRationalLarge =
    // invariants: (p,q) in lowest form, q >= 0
    | Q of BigInteger * BigInteger

    member x.IsNegative =
        let (Q (ap, _)) = x
        sign ap < 0

    member x.IsPositive =
        let (Q (ap, _)) = x
        sign ap > 0

    member x.Numerator =
        let (Q (p, _)) = x in p

    member x.Denominator =
        let (Q (_, q)) = x in q

    member x.Sign =
        let (Q (p,_) ) = x
        sign p

    override this.GetHashCode () =
        BigRationalLarge.Hash this

    override this.ToString () =
        let (Q (p, q)) = this
        if q.IsOne then
            p.ToString()
        else
            p.ToString() + "/" + q.ToString()

    static member Hash (Q (ap, aq)) =
        // This hash code must be identical to the hash for BigInteger when the numbers coincide.
        if aq.IsOne then ap.GetHashCode ()
        else (ap.GetHashCode () <<< 3) + aq.GetHashCode ()

    static member Equals(Q (ap, aq), Q (bp, bq)) =
        // normal form, so structural equality
        BigInteger.(=) (ap, bp) && BigInteger.(=) (aq, bq)

    static member LessThan (Q (ap, aq), Q (bp, bq)) =
        BigInteger.(<) (ap * bq, bp * aq)

    // TODO: performance improvement possible here
    static member Compare (p, q) =
        if BigRationalLarge.LessThan (p, q) then -1
        elif BigRationalLarge.LessThan (q, p)then 1
        else 0

    static member ToDouble (Q (p, q)) =
        ToDoubleI p / ToDoubleI q

    static member Normalize (p : BigInteger, q : BigInteger) =
        if q.IsZero then
            (* throw for any x/0 *)
            raise <| System.DivideByZeroException ()
        elif q.IsOne then
            Q (p, q)
        else
            let k = BigInteger.GreatestCommonDivisor (p, q)
            let p = p / k
            let q = q / k
            if sign q < 0 then
                Q (-p, -q)
            else Q (p, q)

    static member Rational (p : int, q : int) =
        BigRationalLarge.Normalize (bigint p, bigint q)

    // TODO : Rename to Rational? It doesn't seem like we need to force the overload resolution here with a separate name...
    static member RationalZ (p, q) =
        BigRationalLarge.Normalize (p, q)

    /// Return the negation of a rational number
    static member (~-) (Q (bp, bq)) =
        // still coprime, bq >= 0
        Q(-bp, bq)
    
    /// Return the sum of two rational numbers
    static member (+) (Q (ap, aq), Q (bp, bq)) =
        BigRationalLarge.Normalize ((ap * bq) + (bp * aq), aq * bq)
    
    /// Return the difference of two rational numbers
    static member (-) (Q (ap, aq), Q (bp, bq)) =
        BigRationalLarge.Normalize ((ap * bq) - (bp * aq), aq * bq)
    
    /// Return the product of two rational numbers
    static member (*) (Q (ap, aq), Q (bp, bq)) =
        BigRationalLarge.Normalize (ap * bp, aq * bq)
    
    /// Return the ratio of two rational numbers
    static member (/) (Q (ap, aq), Q (bp, bq)) =
        BigRationalLarge.Normalize (ap * bq, aq * bp)

    /// Return the given rational number
    static member ( ~+ ) (n1 : BigRationalLarge) = n1

    //
    static member Parse (str : string) =
        let len = str.Length
        if len=0 then invalidArg "str" "empty string";
        let j = str.IndexOf '/'
        if j >= 0 then
            let p = BigInteger.Parse (str.Substring(0,j))
            let q = BigInteger.Parse (str.Substring(j+1,len-j-1))
            BigRationalLarge.RationalZ (p,q)
        else
            let p = BigInteger.Parse str
            BigRationalLarge.RationalZ (p,OneI)

    override this.Equals(that : obj) =
        match that with
        | :? BigRationalLarge as that ->
            BigRationalLarge.Equals(this,that)
        | _ -> false

    interface System.IComparable with
        member this.CompareTo (obj : obj) =
            match obj with
            | :? BigRationalLarge as other ->
                BigRationalLarge.Compare (this, other)
            | _ ->
                invalidArg "obj" "the object does not have the correct type"


//
[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module private BigRationalLarge =
    //
    let inv (Q (ap, aq)) =
        BigRationalLarge.Normalize (aq, ap)

    //
    let pown (Q (p, q)) (n:int) =
        // p,q powers still coprime
        Q (BigInteger.Pow (p, n), BigInteger.Pow (q, n))

    //
    let equal (Q (ap, aq)) (Q (bp, bq)) =
        // normal form, so structural equality
        ap = bp && aq = bq
    
    //
    let lt a b =
        BigRationalLarge.LessThan (a, b)
    
    //
    let gt a b =
        BigRationalLarge.LessThan (b, a)
    
    //
    let lte (Q(ap, aq)) (Q(bp, bq)) =
        BigInteger.(<=) (ap * bq,bp * aq)
    
    //
    let gte (Q(ap, aq)) (Q(bp, bq)) =
        BigInteger.(>=) (ap * bq, bp * aq)

    //
    let of_bigint z =
        BigRationalLarge.RationalZ(z,OneI)

    //
    let of_int n =
        BigRationalLarge.Rational(n,1)

    // integer part
    let integer (Q (p, q)) =
        let mutable r = BigInteger(0)

        // have p = d.q + r, |r| < |q|
        let d = BigInteger.DivRem (p, q, &r)
        if r < ZeroI then
            // p = (d-1).q + (r+q)
            d - OneI
        else
            // p = d.q + r
            d


/// The type of arbitrary-sized rational numbers.
[<CustomEquality; CustomComparison>]
[<StructuredFormatDisplay("{StructuredDisplayString}N")>]
type BigRational =
    private
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
        | Z _ -> OneI
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
            z.ToString()
        | Q q ->
            q.ToString()

    member this.StructuredDisplayString =
        this.ToString ()

    /// Return the result of converting the string to a rational number
    static member Parse (str : string) =
        Q (BigRationalLarge.Parse str)

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
            Q (BigRationalLarge.of_bigint z + qq)
        | Q q, Z zz ->
            Q (q  + BigRationalLarge.of_bigint zz)

    /// Return the difference of two rational numbers
    static member ( - ) (n1, n2) =
        match n1, n2 with
        | Z z, Z zz ->
            Z (z - zz)
        | Q q, Q qq ->
            Q (q - qq)
        | Z z, Q qq ->
            Q (BigRationalLarge.of_bigint z - qq)
        | Q q, Z zz ->
            Q (q  - BigRationalLarge.of_bigint zz)

    /// Return the product of two rational numbers
    static member ( * ) (n1, n2) =
        match n1,n2 with
        | Z z, Z zz ->
            Z (z * zz)
        | Q q, Q qq ->
            Q (q * qq)
        | Z z, Q qq ->
            Q (BigRationalLarge.of_bigint z * qq)
        | Q q, Z zz ->
            Q (q  * BigRationalLarge.of_bigint zz)

    /// Return the ratio of two rational numbers
    static member ( / ) (n1, n2) =
        match n1, n2 with
        | Z z, Z zz ->
            Q (BigRationalLarge.RationalZ(z,zz))
        | Q q, Q qq ->
            Q (q / qq)
        | Z z, Q qq ->
            Q (BigRationalLarge.of_bigint z / qq)
        | Q q, Z zz ->
            Q (q  / BigRationalLarge.of_bigint zz)

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
            (BigRationalLarge.equal q qq)
        | Z z, Q qq ->
            (BigRationalLarge.equal (BigRationalLarge.of_bigint z) qq)
        | Q q, Z zz ->
            (BigRationalLarge.equal q (BigRationalLarge.of_bigint zz))

    /// This operator is for use from other .NET languages
    static member op_Inequality (n, nn) =
        not <| BigRational.op_Equality (n, nn)

    /// This operator is for use from other .NET languages
    static member op_LessThan (n, nn) =
        match n, nn with
        | Z z, Z zz ->
            BigInteger.(<) (z,zz)
        | Q q, Q qq ->
            (BigRationalLarge.lt q qq)
        | Z z, Q qq ->
            (BigRationalLarge.lt (BigRationalLarge.of_bigint z) qq)
        | Q q, Z zz ->
            (BigRationalLarge.lt q (BigRationalLarge.of_bigint zz))

    /// This operator is for use from other .NET languages
    static member op_LessThanOrEqual (n, nn) =
        match n, nn with
        | Z z, Z zz ->
            BigInteger.(<=) (z,zz)
        | Q q, Q qq ->
            (BigRationalLarge.lte q qq)
        | Z z, Q qq ->
            (BigRationalLarge.lte (BigRationalLarge.of_bigint z) qq)
        | Q q, Z zz ->
            (BigRationalLarge.lte q (BigRationalLarge.of_bigint zz))

    /// This operator is for use from other .NET languages
    static member op_GreaterThan (n, nn) =
        match n, nn with
        | Z z, Z zz ->
            BigInteger.(>) (z,zz)
        | Q q, Q qq ->
            (BigRationalLarge.gt q qq)
        | Z z, Q qq ->
            (BigRationalLarge.gt (BigRationalLarge.of_bigint z) qq)
        | Q q, Z zz ->
            (BigRationalLarge.gt q (BigRationalLarge.of_bigint zz))

    /// This operator is for use from other .NET languages
    static member op_GreaterThanOrEqual (n, nn) =
        match n, nn with
        | Z z, Z zz ->
            BigInteger.(>=) (z,zz)
        | Q q, Q qq ->
            (BigRationalLarge.gte q qq)
        | Z z, Q qq ->
            (BigRationalLarge.gte (BigRationalLarge.of_bigint z) qq)
        | Q q, Z zz ->
            (BigRationalLarge.gte q (BigRationalLarge.of_bigint zz))

    /// Return the absolute value of a rational number
    static member Abs (n : BigRational) =
        if n.IsNegative then -n else n

    /// Return the result of raising the given rational number to the given power
    static member PowN (n, i : int) =
        match n with
        | Z z ->
            Z (BigInteger.Pow (z, i))
        | Q q ->
            Q (BigRationalLarge.pown q i)

    /// Return the result of converting the given rational number to a floating point number
    static member ToDouble (n : BigRational) =
        match n with
        | Z z ->
            ToDoubleI z
        | Q q ->
            BigRationalLarge.ToDouble q

    /// Return the result of converting the given rational number to a big integer
    static member ToBigInt (n : BigRational) =
        match n with
        | Z z -> z
        | Q q ->
            BigRationalLarge.integer q

    /// Return the result of converting the given rational number to an integer
    static member ToInt32 (n : BigRational) =
        match n with
        | Z z ->
            ToInt32I z
        | Q q ->
            ToInt32I (BigRationalLarge.integer q)

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
