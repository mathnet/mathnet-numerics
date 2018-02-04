namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open FsUnitTyped

open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.Distributions
open MathNet.Numerics.Statistics

/// Unit tests for the dense vector type.
module DenseVectorTests =

    /// A small uniform vector.
    let smallv = Double.DenseVector.Create(5, fun i -> 0.3) :> Vector<float>

    /// A large vector with increasingly large entries
    let largev = new Double.DenseVector( Array.init 100 (fun i -> float i / 100.0) ) :> Vector<float>

    [<Test>]
    let ``DenseVector.zero`` () =
        (DenseVector.zero 100) + largev |> shouldEqual largev

    [<Test>]
    let ``DenseVector.random`` () =
        let m = DenseVector.random 100 (Normal.WithMeanStdDev(100.0,0.1))
        (m :?> Double.DenseVector).Values |> ArrayStatistics.Mean |> should (equalWithin 10.0) 100.0
        m.Count |> shouldEqual 100

    [<Test>]
    let ``DenseVector.create`` () =
        DenseVector.create 5 0.3 |> shouldEqual smallv

    [<Test>]
    let ``DenseVector.init`` () =
        DenseVector.init 5 (fun i -> 0.3) |> shouldEqual smallv
        DenseVector.init 100 (fun i -> float i / 100.0) |> shouldEqual largev

    [<Test>]
    let ``DenseVector.ofList`` () =
        DenseVector.ofList [ for i in 0 .. 99 -> float i / 100.0 ] |> shouldEqual largev

    [<Test>]
    let ``DenseVector.ofListi`` () =
        DenseVector.ofListi 100 [ for i in 0 .. 99 -> i, float i / 100.0 ] |> shouldEqual largev

    [<Test>]
    let ``DenseVector.ofSeq`` () =
        DenseVector.ofSeq (seq { for i in 0 .. 99 -> float i / 100.0 }) |> shouldEqual largev

    [<Test>]
    let ``DenseVector.ofSeqi`` () =
        DenseVector.ofSeqi 100 (seq { for i in 99 .. -1 .. 0 -> i, float i / 100.0 }) |> shouldEqual largev

    [<Test>]
    let ``DenseVector.rangef`` () =
        DenseVector.rangef 0.0 0.01 0.99 |> shouldEqual (DenseVector.raw [| for i in 0 .. 99 -> 0.01 * float i |])

    [<Test>]
    let ``DenseVector.range`` () =
        DenseVector.range 0 1 99 |> shouldEqual (DenseVector.raw [| for i in 0 .. 99 -> float i |])
