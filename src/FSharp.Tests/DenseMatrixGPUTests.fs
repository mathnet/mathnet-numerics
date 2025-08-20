namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open FsUnitTyped

open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.Distributions
open MathNet.Numerics.Statistics
open Brahma.FSharp


/// Unit tests for the dense matrix type.
module DenseMatrixGPUTests =

    /// A small uniform matrix.
    let smallM = DenseMatrix.raw 3 2 [|0.3;0.3;0.3;0.3;0.3;0.3|]

    /// A large matrix with increasingly large entries
    let largeM =
        Array.init (100*120) (fun k -> let i, j = (k%100),(k/100) in float i * 100.0 + float j)
        |> DenseMatrix.raw 100 120

    [<Test>]
    let ``DenseMatrixGPU.iter`` () =
        GPU.Matrix.iter <@ fun x -> x |> ignore @> largeM |> ignore
        Assert.True true

    [<Test>]
    let ``DenseMatrixGPU.map2`` () =
        GPU.Matrix.map2 id largeM largeM |> shouldEqual largeM

    [<Test>]
    let ``DenseMatrixGPU.mXm`` () =
        GPU.Matrix.mXm largeM largeM |> shouldEqual largeM