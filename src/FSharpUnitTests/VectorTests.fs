namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open FsUnitTyped

open MathNet.Numerics.LinearAlgebra

/// Unit tests for the vector type.
module VectorTests =

    let approximately_equal tolerance = equalWithin (10.0 ** (float -tolerance))

    /// A small uniform vector.
    let smallv = DenseVector.raw [|0.3;0.3;0.3;0.3;0.3|]

    /// A small sparse vector.
    let sparsev = SparseVector.ofListi 5 [(1,0.3)]

    /// A large vector with increasingly large entries
    let largev = DenseVector.init 100 (fun i -> float i / 100.0)

    [<Test>]
    let ``Vector.GetSlice`` () =
        largev.[*] |> shouldEqual largev
        largev.[0..99]  |> shouldEqual largev
        largev.[1..3]  |> shouldEqual (DenseVector.raw [|0.01;0.02;0.03|])
        largev.[97..]  |> shouldEqual (DenseVector.raw [|0.97;0.98;0.99|])
        largev.[..4]  |> shouldEqual (DenseVector.raw [|0.00;0.01;0.02;0.03;0.04|])

#if NOFSSLICESET1D
#else
// Vector SetSlice does not work properly in VisualStudio 2013 RTM
    [<Test>]
    let ``Vector.SetSlice`` () =
        let v = smallv.Clone() in
            v.[*] <- vector [0.1;0.2;0.3;0.4;0.5];
            v |> shouldEqual (DenseVector.raw [|0.1;0.2;0.3;0.4;0.5|])
        let v = smallv.Clone() in
            v.[0..4] <- vector [0.1;0.2;0.3;0.4;0.5];
            v |> shouldEqual (DenseVector.raw [|0.1;0.2;0.3;0.4;0.5|])
        let v = smallv.Clone() in
            v.[1..3] <- vector [7.0;8.0;9.0];
            v |> shouldEqual (DenseVector.raw [|0.3;7.0;8.0;9.0;0.3|])
        let v = smallv.Clone() in
            v.[2..] <- vector [7.0;8.0;9.0];
            v |> shouldEqual (DenseVector.raw [|0.3;0.3;7.0;8.0;9.0|])
        let v = smallv.Clone() in
            v.[..2] <- vector [7.0;8.0;9.0];
            v |> shouldEqual (DenseVector.raw [|7.0;8.0;9.0;0.3;0.3|])
#endif

    [<Test>]
    let ``Vector.toArray`` () =
        Vector.toArray smallv |> shouldEqual [|0.3;0.3;0.3;0.3;0.3|]

    [<Test>]
    let ``Vector.toList`` () =
        Vector.toList smallv |> shouldEqual [0.3;0.3;0.3;0.3;0.3]

    [<Test>]
    let ``Vector.mapInPlace.Dense`` () =
        let w = smallv.Clone()
        w |> Vector.mapInPlace (fun x -> 2.0 * x)
        w |> shouldEqual (2.0 * smallv)

    [<Test>]
    let ``Vector.mapInPlace.Sparse`` () =
        let w = sparsev.Clone()
        w |> Vector.mapInPlace (fun x -> 2.0 * x)
        w |> shouldEqual (2.0 * sparsev)

    [<Test>]
    let ``Vector.mapSkipZerosInPlace.Sparse`` () =
        let w = sparsev.Clone()
        w |> Vector.mapSkipZerosInPlace (fun x -> 2.0 * x)
        w |> shouldEqual (2.0 * sparsev)

    [<Test>]
    let ``Vector.mapiInPlace.Dense`` () =
        let w = largev.Clone()
        w |> Vector.mapiInPlace (fun i x -> float i / 100.0)
        w |> shouldEqual (largev)

    [<Test>]
    let ``Vector.mapiInPlace.Sparse`` () =
        let w = sparsev.Clone()
        w |> Vector.mapiInPlace (fun i x -> 2.0 * float i * x)
        w |> shouldEqual (2.0 * sparsev)

    [<Test>]
    let ``Vector.mapiSkipZerosInPlace.Sparse`` () =
        let w = sparsev.Clone()
        w |> Vector.mapiSkipZerosInPlace (fun i x -> 2.0 * float i * x)
        w |> shouldEqual (2.0 * sparsev)

    [<Test>]
    let ``Vector.addInPlace`` () =
        let w = largev.Clone()
        Vector.addInPlace w largev
        w |> shouldEqual (2.0 * largev)

    [<Test>]
    let ``Vector.subInPlace`` () =
        let w = largev.Clone()
        Vector.subInPlace w largev
        w |> shouldEqual (0.0 * largev)

    [<Test>]
    let ``Vector.map`` () =
        Vector.map (fun x -> 2.0 * x) largev |> shouldEqual (2.0 * largev)

    [<Test>]
    let ``Vector.mapSkipZeros`` () =
        Vector.mapSkipZeros (fun x -> 2.0 * x) largev |> shouldEqual (2.0 * largev)

    [<Test>]
    let ``Vector.mapi`` () =
        Vector.mapi (fun i x -> float i / 100.0) largev |> shouldEqual largev

    [<Test>]
    let ``Vector.mapiSkipZeros`` () =
        Vector.mapiSkipZeros (fun i x -> float i / 100.0) largev |> shouldEqual largev

    [<Test>]
    let ``Vector.fold`` () =
        Vector.fold (fun a b -> a - b) 0.0 smallv |> shouldEqual -1.5

    [<Test>]
    let ``Vector.foldBack`` () =
        Vector.foldBack (fun a b -> a - b) 0.0 smallv |> shouldEqual 0.0

    [<Test>]
    let ``Vector.foldi`` () =
        Vector.foldi (fun i a b -> a + b) 0.0 smallv |> shouldEqual 1.5

    [<Test>]
    let ``Vector.forall`` () =
        Vector.forall (fun x -> x = 0.3) smallv |> shouldEqual true

    [<Test>]
    let ``Vector.exists`` () =
        Vector.exists (fun x -> x = 0.3) smallv |> shouldEqual true

    [<Test>]
    let ``Vector.foralli`` () =
        Vector.foralli (fun i x -> x = 0.3 && i < 5) smallv |> shouldEqual true

    [<Test>]
    let ``Vector.existsi`` () =
        Vector.existsi (fun i x -> x = 0.3 && i = 2) smallv |> shouldEqual true

    [<Test>]
    let ``Vector.scan`` () =
        Vector.scan (fun acc x -> acc + x) 0.0 smallv |> should (approximately_equal 14) (DenseVector.raw [|0.0;0.3;0.6;0.9;1.2;1.5|])

    [<Test>]
    let ``Vector.scanBack`` () =
        Vector.scanBack (fun x acc -> acc + x) 0.0 smallv |> should (approximately_equal 14) (DenseVector.raw [|0.0;0.3;0.6;0.9;1.2;1.5|])

    [<Test>]
    let ``Vector.reduce`` () =
        Vector.reduce (fun acc x -> acc ** x) smallv |> should (approximately_equal 14) 0.990295218585507

    [<Test>]
    let ``Vector.reduceBack`` () =
        Vector.reduceBack (fun x acc -> x ** acc) smallv |> should (approximately_equal 14) 0.488911287726319

    [<Test>]
    let ``Vector.insert`` () =
        Vector.insert 2 0.5 smallv |> should (approximately_equal 14) (DenseVector.raw [|0.3;0.3;0.5;0.3;0.3;0.3|])

    [<Test>]
    let ``Pointwise Multiplication using .* Operator`` () =
        let z = largev .* largev
        z |> should (approximately_equal 14) (DenseVector.init 100 (fun i -> (float i / 100.0) ** 2.0))

    [<Test>]
    let ``Pointwise Division using ./ Operator`` () =
        let z = largev ./ DenseVector.create 100 2.0
        z |> should (approximately_equal 14) (largev * 0.5)

    [<Test>]
    let ``Pointwise Modulus using .% Operator`` () =
        let z = largev .% DenseVector.create 100 2.0
        z |> should (approximately_equal 14) (largev % 2.0)
