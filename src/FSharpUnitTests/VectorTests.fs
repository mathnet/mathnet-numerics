namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra.Generic
open MathNet.Numerics.LinearAlgebra.Double

/// Unit tests for the vector type.
module VectorTests =

    /// A small uniform vector.
    let smallv = new DenseVector([|0.3;0.3;0.3;0.3;0.3|]) :> Vector<float>

    /// A large vector with increasingly large entries
    let largev = new DenseVector(Array.init 100 (fun i -> float i / 100.0)) :> Vector<float>

    [<Test>]
    let ``Vector.toArray`` () =
        Vector.toArray smallv |> should array_equal [|0.3;0.3;0.3;0.3;0.3|]

    [<Test>]
    let ``Vector.toList`` () =
        Vector.toList smallv |> should equal [0.3;0.3;0.3;0.3;0.3]

    [<Test>]
    let ``Vector.mapInPlace`` () =
        let w = smallv.Clone()
        Vector.mapInPlace (fun x -> 2.0 * x) w
        w |> should equal (2.0 * smallv)

    [<Test>]
    let ``Vector.mapiInPlace`` () =
        let w = largev.Clone()
        Vector.mapiInPlace (fun i x -> float i / 100.0) w
        w |> should equal (largev)

    [<Test>]
    let ``Vector.addInPlace`` () =
        let w = largev.Clone()
        Vector.addInPlace w largev
        w |> should equal (2.0 * largev)

    [<Test>]
    let ``Vector.subInPlace`` () =
        let w = largev.Clone()
        Vector.subInPlace w largev
        w |> should equal (0.0 * largev)

    [<Test>]
    let ``Vector.map`` () =
        Vector.map (fun x -> 2.0 * x) largev |> should equal (2.0 * largev)

    [<Test>]
    let ``Vector.mapi`` () =
        Vector.mapi (fun i x -> float i / 100.0) largev |> should equal largev

    [<Test>]
    let ``Vector.fold`` () =
        Vector.fold (fun a b -> a - b) 0.0 smallv |> should equal -1.5

    [<Test>]
    let ``Vector.foldBack`` () =
        Vector.foldBack (fun a b -> a - b) 0.0 smallv |> should equal 0.0

    [<Test>]
    let ``Vector.foldi`` () =
        Vector.foldi (fun i a b -> a + b) 0.0 smallv |> should equal 1.5

    [<Test>]
    let ``Vector.forall`` () =
        Vector.forall (fun x -> x = 0.3) smallv |> should equal true

    [<Test>]
    let ``Vector.exists`` () =
        Vector.exists (fun x -> x = 0.3) smallv |> should equal true

    [<Test>]
    let ``Vector.foralli`` () =
        Vector.foralli (fun i x -> x = 0.3 && i < 5) smallv |> should equal true

    [<Test>]
    let ``Vector.existsi`` () =
        Vector.existsi (fun i x -> x = 0.3 && i = 2) smallv |> should equal true

    [<Test>]
    let ``Vector.scan`` () =
        Vector.scan (fun acc x -> acc + x) smallv |> should (approximately_vector_equal 14) (new DenseVector( [|0.3;0.6;0.9;1.2;1.5|] ) :> Vector<float>)

    [<Test>]
    let ``Vector.scanBack`` () =
        Vector.scanBack (fun x acc -> acc + x) smallv |> should (approximately_vector_equal 14) (new DenseVector( [|1.5;1.2;0.9;0.6;0.3|] ) :> Vector<float>)

    [<Test>]
    let ``Vector.reduce`` () =
        Vector.reduce (fun acc x -> acc ** x) smallv |> should (approximately_equal 14) 0.990295218585507

    [<Test>]
    let ``Vector.reduceBack`` () =
        Vector.reduceBack (fun x acc -> x ** acc) smallv |> should (approximately_equal 14) 0.488911287726319

    [<Test>]
    let ``Vector.insert`` () =
        Vector.insert 2 0.5 smallv |> should (approximately_vector_equal 14) (new DenseVector ( [|0.3;0.3;0.5;0.3;0.3;0.3|] ) :> Vector<float>)
