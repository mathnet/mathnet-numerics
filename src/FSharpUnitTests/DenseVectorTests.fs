namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra.Generic
open MathNet.Numerics.LinearAlgebra.Double

/// Unit tests for the dense vector type.
module DenseVectorTests =

    /// A small uniform vector.
    let smallv = new DenseVector(5, 0.3 )

    /// A large vector with increasingly large entries
    let largev = new DenseVector( Array.init 100 (fun i -> float i / 100.0) )

    [<Test>]
    let ``DenseVector.init`` () =
        DenseVector.init 100 (fun i -> float i / 100.0) |> should equal largev

    [<Test>]
    let ``DenseVector.ofList`` () =
        DenseVector.ofList [ for i in 0 .. 99 -> float i / 100.0 ] |> should equal largev

    [<Test>]
    let ``DenseVector.ofSeq`` () =
        DenseVector.ofSeq (seq { for i in 0 .. 99 -> float i / 100.0 }) |> should equal largev

    [<Test>]
    let ``DenseVector.rangef`` () =
        DenseVector.rangef 0.0 0.01 0.99 |> should equal (new DenseVector( [| for i in 0 .. 99 -> 0.01 * float i |] ) )

    [<Test>]
    let ``DenseVector.range`` () =
        DenseVector.range 0 99 |> should equal (new DenseVector( [| for i in 0 .. 99 -> float i |] ) )
