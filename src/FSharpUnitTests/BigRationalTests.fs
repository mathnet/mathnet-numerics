// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack.Unittests/BigRationalTests.fs

namespace MathNet.Numerics.Tests

#if NOSYSNUMERICS
#else

open MathNet.Numerics
open NUnit.Framework
open System
open System.Collections
open System.Collections.Generic
open System.Numerics


[<TestFixture>]
type public BigRationalTests() =

    // BigRational Tests
    // =================

    // Notes: What cases to consider?
    //   For (p,q) cases q=0, q=1, q<>1. [UPDATE: remove (x,0)]
    //   For (p,q) when q=1 there could be 2 internal representations, either Z or Q.
    //   For (p,0) this value can be signed, corresponds to +/- infinity point. [Update: remove it]
    // Hashes on (p,1) for both representations must agree.
    // For binary operators, try for result with and without HCF (normalisation).
    // Also: 0/0 is an acceptable representation. See normalisation code. [Update: remove it].

    // Overrides to test:
    // .ToString()
    // .GetHashCode()
    // .Equals()
    // IComparable.CompareTo()

    // Misc construction.
    let natA  n   = BigRational.FromInt n       // internally Z
    let natB  n   = (natA n / natA 7) * natA 7  // internally Q
    let ratio p q = BigRational.FromInt p / BigRational.FromInt q
    let (/%)  b c = BigRational.FromBigInt b / BigRational.FromBigInt c

    // Misc test values
    let q0 = natA 0
    let q1 = natA 1
    let q2 = natA 2
    let q3 = natA 3
    let q4 = natA 4
    let q5 = natA 5
    let minIntI = bigint System.Int32.MinValue
    let maxIntI = bigint System.Int32.MaxValue
    let ran = System.Random()
    let nextZ n = bigint (ran.Next(n))

    // A selection of test points.
    let points =
        // A selection of integer and reciprical points
        let points =
            [for i in -13I .. 13I -> i,1I] @
            [for i in -13I .. 13I -> 1I,i]
        // Exclude x/0
        let points = [for p,q in points do if q <> 0I then yield p,q ] // PROPOSE: (q,0) never a valid Q value, filter them out of tests...
        // Scale by various values, including into BigInt range
        let scale (kp,kq) (p,q) = (p*kp,q*kq)
        let scales k pqs = List.map (scale k) pqs
        let points = List.concat [points;
                                  scales (10000I,1I) points;
                                  scales (1I,10000I) points;
                                  scales (maxIntI,1I) points;
                                  scales (1I,maxIntI) points;
                                 ]
        points
    let pointsNonZero = [for p,q in points do if p<>0I then yield p,q] // non zero points

    let makeQs p q =
        if q = 1I && minIntI <= p && p <= maxIntI then
            // (p,1) where p is int32
            let p32 = int32 p
            [natA p32;natB p32;BigRational.FromBigInt p]   // two reprs for int32
        else
            [BigRational.FromBigInt p / BigRational.FromBigInt q]

    let miscQs = [for p,q in points do yield! makeQs p q]

    let product xs ys = [for x in xs do for y in ys do yield x,y]
    let vector1s = [for z in points -> z]
    let vector2s = product points points

    [<Test>]
    member this.BasicTests1() =
        check "generic format h"  "1N" (sprintf "%A" 1N)
        check "generic format q"  "-1N" (sprintf "%A" (-1N))

        test "vliwe98"   (id -2N = - 2N)
        test "d3oc002" (LanguagePrimitives.GenericZero<bignum> = 0N)
        test "d3oc112w" (LanguagePrimitives.GenericOne<bignum> = 1N)

        check "weioj3h" (sprintf "%O" 3N) "3"
        check "weioj3k" (sprintf "%O" (3N / 4N)) "3/4"
        check "weioj3k" (sprintf "%O" (3N / 400000000N)) "3/400000000"
        check "weioj3l" (sprintf "%O" (3N / 3N))  "1"
        check "weioj3q" (sprintf "%O" (-3N))  "-3"
        //check "weioj3w" (sprintf "%O" -3N) "-3"
        check "weioj3e" (sprintf "%O" (-3N / -3N)) "1"

        // The reason why we do not use hardcoded values is the the representation may change based on the NetFx we are targeting.
        // For example, when targeting NetFx4.0, the result is "-3E+61" instead of "-3000....0N"
        let v = -30000000000000000000000000000000000000000000000000000000000000N
        check "weioj3r" (sprintf "%O" v) ((box v).ToString())


    [<Test>]
    member this.BasicTests2() =


        // Test arithmetic ops: tests
        let test2One name f check ((p,q),(pp,qq)) =
            // There may be several ways to construct the test rationals
            let zs        = makeQs p  q
            let zzs       = makeQs pp qq
            let results   = [for z in zs do for zz in zzs do yield f (z,zz)]
            let refP,refQ = check (p,q) (pp,qq)
            let refResult = BigRational.FromBigInt refP / BigRational.FromBigInt refQ
            let resOK (result:BigRational) =
                result.Numerator * refQ = refP * result.Denominator &&
                BigRational.Equals(refResult,result)
            match List.tryFind (fun result -> not (resOK result)) results with
            | None        -> () // ok
            | Some result -> printf "Test failed. %s (%A,%A) (%A,%A). Expected %A. Observed %A\n" name p q pp qq refResult result
                             reportFailure "cejkew09"

        let test2All name f check vectors = List.iter (test2One name f check) vectors

        // Test arithmetic ops: call
        test2All "add" (BigRational.(+))  (fun (p,q) (pp,qq) -> (p*qq + q*pp,q*qq)) vector2s
        test2All "sub" (BigRational.(-))  (fun (p,q) (pp,qq) -> (p*qq - q*pp,q*qq)) vector2s
        test2All "mul" (BigRational.(*))  (fun (p,q) (pp,qq) -> (p*pp,q*qq))        vector2s // *) <-- for EMACS
        test2All "div" (BigRational.(/))  (fun (p,q) (pp,qq) -> (p*qq,q*pp))        (product points pointsNonZero)



    [<Test>]
    member this.RangeTests() =
        // Test x0 .. dx .. x1
        let checkRange3 (x0:BigRational) dx x1 k =
            let f (x:BigRational) = x * BigRational.FromBigInt k |> BigRational.ToBigInt
            let rangeA = {x0 .. dx .. x1} |> Seq.map f
            let rangeB = {f x0 .. f dx .. f x1}
            //printf "Length=%d\n" (Seq.length rangeA)
            let same = Seq.forall2 (=) rangeA rangeB
            check (sprintf "Range3 %A .. %A .. %A scaled to %A" x0 dx x1 k) same true

        checkRange3 (0I /% 1I)  (1I /% 7I) (100I /% 1I)  (7I*1I)
        checkRange3 (0I /% 1I)  (1I /% 7I) (100I /% 11I) (7I*11I)
        checkRange3 (1I /% 13I) (1I /% 7I) (100I /% 11I) (7I*11I*13I)
        for i = 0 to 1000 do
            let m = 1000 // max steps is -m to m in steps of 1/m i.e. 2.m^2
            let p0,q0 = nextZ m     ,nextZ m + 1I
            let p1,q1 = nextZ m     ,nextZ m + 1I
            let pd,qd = nextZ m + 1I,nextZ m + 1I
            checkRange3 (p0 /% q0) (pd /% qd) (p1 /% q1) (q0 * q1 * qd)


        // Test x0 .. x1
        let checkRange2 (x0:BigRational) x1 =
            let z0  = BigRational.ToBigInt x0
            let z01 = BigRational.ToBigInt (x1 - x0)
            let f (x:BigRational) = x |> BigRational.ToBigInt
            let rangeA = [x0 .. x1] |> List.map f       // range with each item rounded down
            let rangeB = [z0 .. z0 + z01]               // range of same length from the round down start point
            check (sprintf "Range2: %A .. %A" x0 x1) rangeA rangeB

        checkRange2 (0I /% 1I)  (100I /% 1I)
        checkRange2 (0I /% 1I)  (100I /% 11I)
        checkRange2 (1I /% 13I) (100I /% 11I)
        for i = 0 to 1000 do
            let m = 10000 // max steps is -m to m in steps of 1 i.e. 2.m
            let p0,q0 = nextZ m     ,nextZ m + 1I
            let p1,q1 = nextZ m     ,nextZ m + 1I
            checkRange2 (p0 /% q0) (p1 /% q1) //(q0 * q1 * qd)

        // ToString()
        // Cases: integer, computed integer, rational<1, rational>1, +/-infinity, nan
        (natA 1).ToString()     |> check "ToString" "1"
        (natA 0).ToString()     |> check "ToString"  "0"
        (natA (-12)).ToString() |> check "ToString" "-12"
        (natB 1).ToString()     |> check "ToString" "1"
        (natB 0).ToString()     |> check "ToString" "0"
        (natB (-12)).ToString() |> check "ToString" "-12"
        (1I /% 3I).ToString()   |> check "ToString" "1/3"
        (12I /% 5I).ToString()  |> check "ToString" "12/5"
        //(13I /% 0I).ToString()  |> check "ToString" "1/0"     // + 1/0. Plan to make this invalid value
        //(-13I /% 0I).ToString() |> check "ToString" "1/0"     // - 1/0. Plan to make this invalid value
        //(0I /% 0I).ToString()   |> check "ToString" "0/0"     //   0/0. Plan to make this invalid value

        // GetHashCode
        // Cases: zero, integer, computed integer, computed by multiple routes.
        let checkSameHashGeneric a b                      = check (sprintf "GenericHash     %A %A" a b) (a.GetHashCode()) (b.GetHashCode())
        let checkSameHash (a:BigRational) (b:BigRational) = check (sprintf "BigRationalHash %A %A" a b)  (a.GetHashCode()) (b.GetHashCode()); checkSameHashGeneric a b

        List.iter (fun n -> checkSameHash (natA n) (natB n)) [-10 .. 10]
        List.iter (fun n -> checkSameHash n ((n * q3 + n * q2) / q5)) miscQs

        // bug 3488: should non-finite values be supported?
        //let x = BigRational.FromBigInt (-1I) / BigRational.FromBigInt 0I
        //let q2,q3,q5 = BigRational.FromInt 2,BigRational.FromInt 3,BigRational.FromInt 5
        //let x2 = (x * q2 + x * q3) / q5
        //x,x2,x = x2

        // Test: Zero,One?
        check "ZeroA" BigRational.Zero (natA 0)
        check "ZeroA" BigRational.Zero (natA 0)
        check "OneA"  BigRational.One  (natB 1)
        check "OneB"  BigRational.One  (natB 1)

    [<Test>]
    member this.BinaryAndUnaryOperators() =
        // Test: generic bop
        let testR2One name f check ((p,q),(pp,qq)) =
            // There may be several ways to construct the test rationals
            let zs        = makeQs p  q
            let zzs       = makeQs pp qq
            let resultRef = check (p,q) (pp,qq) // : bool
            let args      = [for z in zs do for zz in zzs do yield (z,zz)]
            match List.tryFind (fun (z,zz) -> resultRef <> f (z,zz)) args with
            | None        -> () // ok
            | Some (z,zz) -> printf "Test failed. %s (%A,%A) (%A,%A) = %s %A %A. Expected %A.\n" name p q pp qq name z zz resultRef
                             reportFailure "cknwe9"

        // Test: generic uop
        let testR1One name f check (p,q) =
            // There may be several ways to construct the test rationals
            let zs        = makeQs p  q
            let resultRef = check (p,q) //: bool
            match List.tryFind (fun z -> resultRef <> f z) zs with
            | None   -> () // ok
            | Some z -> printf "Test failed. %s (%A,%A) = %s %A. Expected %A.\n" name p q name z resultRef
                        reportFailure "vekjkrejvre0"

        let testR2All name f check vectors = List.iter (testR2One name f check) vectors
        let testR1All name f check vectors = List.iter (testR1One name f check) vectors

        // Test: relations
        let sign (i:BigInteger) = BigInteger(i.Sign)
        testR2All "="  BigRational.(=)           (fun (p,q) (pp,qq) -> (p*qq =  q*pp)) vector2s
        testR2All "="  BigRational.op_Equality   (fun (p,q) (pp,qq) -> (p*qq =  q*pp)) vector2s
        testR2All "!=" BigRational.op_Inequality (fun (p,q) (pp,qq) -> (p*qq <> q*pp)) vector2s
        //     p/q < pp/qq
        // iff (p * sign q) / (q  * sign q)  < (pp * sign qq) / (qq * sign qq)
        // iff (p * sign q) * (qq * sign qq) < (pp * sign qq) * (q  * sign q)           since q*sign q is always +ve.
        testR2All "<"  BigRational.(<)  (fun (p,q) (pp,qq) -> (p * sign q) * (qq * sign qq) < (pp * sign qq) * (q * sign q)) vector2s
        testR2All ">"  BigRational.(>)  (fun (p,q) (pp,qq) -> (p * sign q) * (qq * sign qq) > (pp * sign qq) * (q * sign q)) vector2s
        testR2All "<=" BigRational.(<=) (fun (p,q) (pp,qq) -> (p * sign q) * (qq * sign qq) <= (pp * sign qq) * (q * sign q)) vector2s
        testR2All ">=" BigRational.(>=) (fun (p,q) (pp,qq) -> (p * sign q) * (qq * sign qq) >= (pp * sign qq) * (q * sign q)) vector2s

        // System.IComparable tests
        let BigRationalCompareTo (p:BigRational,q:BigRational) = (p :> System.IComparable).CompareTo(q)
        testR2All "IComparable.CompareTo" BigRationalCompareTo (fun (p,q) (pp,qq) -> compare ((p * sign q) * (qq * sign qq)) ((pp * sign qq) * (q * sign q))) vector2s

        // Test: is negative, is positive
        testR1All "IsNegative" (fun (x:BigRational) -> x.IsNegative) (fun (p,q) -> sign p * sign q = -1I) vector1s
        testR1All "IsPositive" (fun (x:BigRational) -> x.IsPositive) (fun (p,q) -> sign p * sign q =  1I) vector1s
        testR1All "IsZero"     (fun (x:BigRational) -> x = q0)       (fun (p,q) -> sign p = 0I)           vector1s


        let test1One name f check (p,q) =
            // There may be several ways to construct the test rationals
            let zs        = makeQs p  q
            let results   = [for z in zs -> f z]
            let refP,refQ = check (p,q)
            let refResult = BigRational.FromBigInt refP / BigRational.FromBigInt refQ
            let resOK (result:BigRational) =
               result.Numerator * refQ = refP * result.Denominator &&
               BigRational.Equals(refResult,result)
            match List.tryFind (fun result -> not (resOK result)) results with
            | None        -> () // ok
            | Some result -> printf "Test failed. %s (%A,%A). Expected %A. Observed %A\n" name p q refResult result
                             reportFailure "klcwe09wek"

        let test1All name f check vectors = List.iter (test1One name f check) vectors

        test1All "neg" (BigRational.(~-)) (fun (p,q)         -> (-p,q))             vector1s
        test1All "pos" (BigRational.(~+)) (fun (p,q)         -> (p,q))              vector1s // why have ~+ ???

        // Test: Abs,Sign
        test1All  "Abs"         (BigRational.Abs)     (fun (p,q) -> (abs p,abs q)) vector1s
        testR1All "Sign"        (fun (x:BigRational) -> x.Sign)    (fun (p,q) -> check "NonZeroDenom" (sign q <> 0I) true; (sign p * sign q) |> int32) vector1s

        // Test: Pow
        test1All  "Pow(x,2)"   (fun x -> BigRational.Pow(x,2))   (fun (p,q) -> (p*p,q*q)) vector1s
        test1All  "Pow(x,1)"   (fun x -> BigRational.Pow(x,1))   (fun (p,q) -> (p,q)) vector1s
        test1All  "Pow(x,0)"   (fun x -> BigRational.Pow(x,0))   (fun (p,q) -> (1I,1I)) vector1s
        test1All  "Pow(x,-1)"   (fun x -> BigRational.Pow(x,-1))   (fun (p,q) -> (q,p)) (vector1s |> List.filter (fun (p,_) -> p <> 0I))
        test1All  "Pow(x,-2)"   (fun x -> BigRational.Pow(x,-2))   (fun (p,q) -> (q*q,p*p)) (vector1s |> List.filter (fun (p,_) -> p <> 0I))

        testR1One  "Pow(0,-1)"  (fun x -> throws (fun () -> BigRational.PowN(x,-1))) (fun (p,q) -> true) (0I, -1I)
        testR1One  "Pow(0,-2)"  (fun x -> throws (fun () -> BigRational.PowN(x,-2))) (fun (p,q) -> true) (0I, -2I)

        // MatteoT: moved to numbersVS2008\test.ml
        //test1All  "PowN(x,200)" (fun x -> BigRational.PowN(x,200)) (fun (p,q) -> (BigInteger.Pow(p,200I),BigInteger.Pow(q,200I))) vector1s

        // MatteoT: moved to numbersVS2008\test.ml
        //let powers = [0I .. 100I]
        //powers |> List.iter (fun i -> test1All  "PowN(x,i)" (fun x -> BigRational.PowN(x,int i)) (fun (p,q) -> (BigInteger.Pow(p,i),BigInteger.Pow(q,i))) vector1s)



[<TestFixture>]
type BigNumType() =
    let g_positive1 = 1000000000000000000000000000000000018N
    let g_positive2 = 1000000000000000000000000000000000000N
    let g_negative1 = -1000000000000000000000000000000000018N
    let g_negative2 = -1000000000000000000000000000000000000N
    let g_negative3 = -1000000000000000000000000000000000036N
    let g_zero      = 0N
    let g_normal    = 88N
    let g_bigintpositive    = 1000000000000000000000000000000000018I
    let g_bigintnegative    = -1000000000000000000000000000000000018I

    // Interfaces
    [<Test>]
    member this.IComparable() =
        // Legit IC
        let ic = g_positive1 :> IComparable
        Assert.AreEqual(ic.CompareTo(g_positive1),0)
        checkThrowsArgumentException( fun () -> ic.CompareTo(g_bigintpositive) |> ignore)

    // Base class methods
    [<Test>]
    member this.ObjectToString() =

        // Currently the CLR 4.0 and CLR 2.0 behavior of BigInt.ToString is different, causing this test to fail.

        Assert.AreEqual(g_positive1.ToString(),
                        "1000000000000000000000000000000000018")
        Assert.AreEqual(g_zero.ToString(),"0")
        Assert.AreEqual(g_normal.ToString(),"88")

    // Static methods
    [<Test>]
    member this.Abs() =
        Assert.AreEqual(bignum.Abs(g_negative1), g_positive1)
        Assert.AreEqual(bignum.Abs(g_negative2), g_positive2)
        Assert.AreEqual(bignum.Abs(g_positive1), g_positive1)
        Assert.AreEqual(bignum.Abs(g_normal),    g_normal)
        Assert.AreEqual(bignum.Abs(g_zero),      g_zero)
        ()

    [<Test>]
    member this.FromBigInt() =
        Assert.AreEqual(bignum.FromBigInt(g_bigintpositive),
                        g_positive1)
        Assert.AreEqual(bignum.FromBigInt(g_bigintnegative),
                        g_negative1)
        Assert.AreEqual(bignum.FromBigInt(0I),g_zero)
        Assert.AreEqual(bignum.FromBigInt(88I),g_normal)
        ()

    [<Test>]
    member this.FromInt() =
        Assert.AreEqual(bignum.FromInt(2147483647),   2147483647N)
        Assert.AreEqual(bignum.FromInt(-2147483648), -2147483648N)
        Assert.AreEqual(bignum.FromInt(0),   0N)
        Assert.AreEqual(bignum.FromInt(88), 88N)
        ()

    [<Test>]
    member this.Zero() =
        Assert.AreEqual(bignum.Zero,0N)
        Assert.IsTrue(bignum.Zero.IsZero)
        ()

    [<Test>]
    member this.One() =
        Assert.AreEqual(bignum.One,1N)
        Assert.IsTrue(bignum.One.IsOne)
        ()

    [<Test>]
    member this.Parse() =
        Assert.AreEqual(bignum.Parse("100"),   100N)
        Assert.AreEqual(bignum.Parse("-100"), -100N)
        Assert.AreEqual(bignum.Parse("0"),     g_zero)
        Assert.AreEqual(bignum.Parse("88"),    g_normal)
        ()

    [<Test>]
    member this.Pow() =
        Assert.AreEqual(bignum.Pow(100N,2), 10000N)
        Assert.AreEqual(bignum.Pow(-3N,3),  -27N)
        Assert.AreEqual(bignum.Pow(2N,-2), 1N/4N)
        Assert.AreEqual(bignum.Pow(2N/3N,-2), 9N/4N)
        Assert.AreEqual(bignum.Pow(g_zero,2147483647), 0N)
        Assert.AreEqual(bignum.Pow(g_normal,0), 1N)
        ()

    [<Test>]
    member this.PowN() =
        Assert.AreEqual(bignum.PowN(100N,2), 10000N)
        Assert.AreEqual(bignum.PowN(-3N,3),  -27N)
        Assert.AreEqual(bignum.PowN(g_zero,2147483647), 0N)
        Assert.AreEqual(bignum.PowN(g_normal,0),        1N)
        ()


    [<Test>]
    member this.Sign() =
        Assert.AreEqual(g_positive1.Sign,  1)
        Assert.AreEqual(g_negative1.Sign, -1)
        Assert.AreEqual(g_zero.Sign,   0)
        Assert.AreEqual(g_normal.Sign, 1)
        ()



    [<Test>]
    member this.ToBigInt() =
        Assert.AreEqual(bignum.ToBigInt(g_positive1), g_bigintpositive)
        Assert.AreEqual(bignum.ToBigInt(g_negative1), g_bigintnegative)
        Assert.AreEqual(bignum.ToBigInt(g_zero),   0I)
        Assert.AreEqual(bignum.ToBigInt(g_normal), 88I)
        ()



    [<Test>]
    member this.ToDouble() =
        Assert.AreEqual(bignum.ToDouble(179769N*1000000000000000N),   1.79769E+20)
        Assert.AreEqual(bignum.ToDouble(-179769N*1000000000000000N), -1.79769E+20)
        Assert.AreEqual(bignum.ToDouble(0N),0.0)
        Assert.AreEqual(bignum.ToDouble(88N),88.0)
        Assert.AreEqual(double(179769N*1000000000000000N),   1.79769E+20)
        Assert.AreEqual(double(-179769N*1000000000000000N), -1.79769E+20)
        Assert.AreEqual(double(0N),0.0)
        Assert.AreEqual(double(88N),88.0)
        ()


    [<Test>]
    member this.ToInt32() =
        Assert.AreEqual(bignum.ToInt32(2147483647N),   2147483647)
        Assert.AreEqual(bignum.ToInt32(-2147483648N), -2147483648)
        Assert.AreEqual(bignum.ToInt32(0N),  0)
        Assert.AreEqual(bignum.ToInt32(88N), 88)
        Assert.AreEqual(int32(2147483647N),   2147483647)
        Assert.AreEqual(int32(-2147483648N), -2147483648)
        Assert.AreEqual(int32(0N),  0)
        Assert.AreEqual(int32(88N), 88)




    // operator methods
    [<Test>]
    member this.test_op_Addition() =

        Assert.AreEqual(100N + 200N, 300N)
        Assert.AreEqual((-100N) + (-200N), -300N)
        Assert.AreEqual(g_positive1 + g_negative1, 0N)
        Assert.AreEqual(g_zero + g_zero,0N)
        Assert.AreEqual(g_normal + g_normal, 176N)
        Assert.AreEqual(g_normal + g_normal, 176N)
        ()



    [<Test>]
    member this.test_op_Division() =
        Assert.AreEqual(g_positive1 / g_positive1, 1N)
        Assert.AreEqual(-100N / 2N, -50N)
        Assert.AreEqual(g_zero / g_positive1, 0N)
        ()

    [<Test>]
    member this.test_op_Equality() =

        Assert.IsTrue((g_positive1 = g_positive1))
        Assert.IsTrue((g_negative1 = g_negative1))
        Assert.IsTrue((g_zero = g_zero))
        Assert.IsTrue((g_normal = g_normal))
        ()

    [<Test>]
    member this.test_op_GreaterThan() =
        Assert.AreEqual((g_positive1 > g_positive2), true)
        Assert.AreEqual((g_negative1 > g_negative2), false)
        Assert.AreEqual((g_zero > g_zero), false)
        Assert.AreEqual((g_normal > g_normal), false)


        ()
    [<Test>]
    member this.test_op_GreaterThanOrEqual() =
        Assert.AreEqual((g_positive1 >= g_positive2), true)
        Assert.AreEqual((g_positive2 >= g_positive1), false)
        Assert.AreEqual((g_negative1 >= g_negative1), true)
        Assert.AreEqual((0N >= g_zero), true)

        ()
    [<Test>]
    member this.test_op_LessThan() =
        Assert.AreEqual((g_positive1 < g_positive2), false)
        Assert.AreEqual((g_negative1 < g_negative3), false)
        Assert.AreEqual((0N < g_zero), false)

        ()
    [<Test>]
    member this.test_op_LessThanOrEqual() =
        Assert.AreEqual((g_positive1 <= g_positive2), false)
        Assert.AreEqual((g_positive2 <= g_positive1), true)
        Assert.AreEqual((g_negative1 <= g_negative1), true)
        Assert.AreEqual((0N <= g_zero), true)

        ()

    [<Test>]
    member this.test_op_Multiply() =
        Assert.AreEqual(3N * 5N, 15N)
        Assert.AreEqual((-3N) * (-5N), 15N)
        Assert.AreEqual((-3N) * 5N, -15N)
        Assert.AreEqual(0N * 5N, 0N)

        ()

    [<Test>]
    member this.test_op_Range() =
        let resultPos = [0N .. 2N]
        let seqPos    = [0N; 1N; 2N]
        verifySeqsEqual resultPos seqPos

        let resultNeg = [-2N .. 0N]
        let seqNeg =  [-2N; -1N; 0N]
        verifySeqsEqual resultNeg seqNeg

        let resultSmall = [0N ..5N]
        let seqSmall = [0N; 1N; 2N; 3N; 4N; 5N]
        verifySeqsEqual resultSmall seqSmall

        ()


    [<Test>]
    member this.test_op_RangeStep() =
        let resultPos = [0N .. 3N .. 6N]
        let seqPos    = [0N; 3N; 6N]
        verifySeqsEqual resultPos seqPos

        let resultNeg = [-6N .. 3N .. 0N]
        let seqNeg =  [-6N; -3N; 0N]
        verifySeqsEqual resultNeg seqNeg

        let resultSmall = [0N .. 3N .. 9N]
        let seqSmall = [0N; 3N; 6N; 9N]
        verifySeqsEqual resultSmall seqSmall

        ()

    [<Test>]
    member this.test_op_Subtraction() =
        Assert.AreEqual(g_positive1 - g_positive2,18N)
        Assert.AreEqual(g_negative1 - g_negative3,18N)
        Assert.AreEqual(0N-g_positive1, g_negative1)
        ()

    [<Test>]
    member this.test_op_UnaryNegation() =
        Assert.AreEqual(-g_positive1, g_negative1)
        Assert.AreEqual(-g_negative1, g_positive1)
        Assert.AreEqual(-0N,0N)

        ()

    [<Test>]
    member this.test_op_UnaryPlus() =
        Assert.AreEqual(+g_positive1,g_positive1)
        Assert.AreEqual(+g_negative1,g_negative1)
        Assert.AreEqual(+0N, 0N)

        ()

    // instance methods
    [<Test>]
    member this.Denominator() =
        Assert.AreEqual(g_positive1.Denominator, 1I)
        Assert.AreEqual(g_negative1.Denominator, 1I)
        Assert.AreEqual(0N.Denominator, 1I)

        ()

    [<Test>]
    member this.IsNegative() =
        Assert.IsFalse(g_positive1.IsNegative)
        Assert.IsTrue(g_negative1.IsNegative)

        Assert.IsFalse( 0N.IsNegative)
        Assert.IsFalse(-0N.IsNegative)

        ()


    [<Test>]
    member this.IsPositive() =

        Assert.IsTrue(g_positive1.IsPositive)
        Assert.IsFalse(g_negative1.IsPositive)

        Assert.IsFalse( 0N.IsPositive)
        Assert.IsFalse(-0N.IsPositive)

        ()

    [<Test>]
    member this.IsInteger() =

        Assert.IsTrue(0N.IsInteger)
        Assert.IsTrue(2N.IsInteger)
        Assert.IsTrue(-2N.IsInteger)
        Assert.IsTrue((2N-BigRational.FromInt(3)).IsInteger)
        Assert.IsTrue((1N/BigRational.FromInt(2)+BigRational.FromIntFraction(3,2)).IsInteger)

        Assert.IsFalse((1N/2N).IsInteger)
        Assert.IsFalse((1N/BigRational.FromInt(2)+BigRational.FromIntFraction(3,3)).IsInteger)

    [<Test>]
    member this.IsOne() =

        Assert.IsTrue(1N.IsOne)
        Assert.IsTrue((2N/BigRational.FromInt(2)).IsOne)
        Assert.IsTrue((2N-BigRational.FromInt(1)).IsOne)

        Assert.IsFalse(0N.IsOne)
        Assert.IsFalse(-1N.IsOne)
        Assert.IsFalse(-2N.IsOne)
        Assert.IsFalse(2N.IsOne)

    [<Test>]
    member this.IsZero() =

        Assert.IsTrue(0N.IsZero)
        Assert.IsTrue((2N-BigRational.FromInt(2)).IsZero)

        Assert.IsFalse(1N.IsZero)
        Assert.IsFalse(-1N.IsZero)
        Assert.IsFalse(-2N.IsZero)

    [<Test>]
    member this.Numerator() =
        Assert.AreEqual(g_positive1.Numerator, g_bigintpositive)
        Assert.AreEqual(g_negative1.Numerator, g_bigintnegative)
        Assert.AreEqual(0N.Numerator, 0I)

#endif
