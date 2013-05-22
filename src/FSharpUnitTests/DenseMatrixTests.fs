namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra.Generic
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.Distributions
open MathNet.Numerics.Statistics

/// Unit tests for the dense matrix type.
module DenseMatrixTests =

    /// A small uniform matrix.
    let smallM = DenseMatrix.raw 3 2 [|0.3;0.3;0.3;0.3;0.3;0.3|]

    /// A large matrix with increasingly large entries
    let largeM =
        Array.init (100*120) (fun k -> let (j,i) = System.Math.DivRem(k,100) in float i * 100.0 + float j)
        |> DenseMatrix.raw 100 120

    [<Test>]
    let ``DenseMatrix.zeroCreate`` () =
        (DenseMatrix.zeroCreate 100 120) + largeM |> should equal largeM

    [<Test>]
    let ``DenseMatrix.randomCreate`` () =
        let m = DenseMatrix.randomCreate 100 120 (Normal.WithMeanStdDev(100.0,0.1))
        m.Values |> ArrayStatistics.Mean |> should (equalWithin 10.0) 100.0
        m.RowCount |> should equal 100
        m.ColumnCount |> should equal 120

    [<Test>]
    let ``DenseMatrix.create`` () =
        DenseMatrix.create 3 2 0.3 |> should equal smallM

    [<Test>]
    let ``DenseMatrix.init`` () =
        DenseMatrix.init 3 2 (fun i j -> 0.3) |> should equal smallM
        DenseMatrix.init 100 120 (fun i j -> float i * 100.0 + float j) |> should equal largeM

    [<Test>]
    let ``DenseMatrix.ofArray2`` () =
        DenseMatrix.ofArray2 (array2D [[0.3;0.3];[0.3;0.3];[0.3;0.3]]) |> should equal smallM
        DenseMatrix.ofArray2 (Array2D.create 3 2 0.3) |> should equal smallM
        DenseMatrix.ofArray2 (Array2D.init 100 120 (fun i j -> float i * 100.0 + float j)) |> should equal largeM

    [<Test>]
    let ``DenseMatrix.ofSeq`` () =
        DenseMatrix.ofSeq (Seq.ofList [[0.3;0.3];[0.3;0.3];[0.3;0.3]]) |> should equal smallM

    [<Test>]
    let ``DenseMatrix.ofList`` () =
        DenseMatrix.ofList [[0.3;0.3];[0.3;0.3];[0.3;0.3]] |> should equal smallM

    [<Test>]
    let ``DenseMatrix.ofRows`` () =
        DenseMatrix.ofRows 3 2 (Seq.ofList [[0.3;0.3];[0.3;0.3];[0.3;0.3]]) |> should equal smallM

    [<Test>]
    let ``DenseMatrix.ofRowsList`` () =
        DenseMatrix.ofRowsList 3 2 [[0.3;0.3];[0.3;0.3];[0.3;0.3]] |> should equal smallM

    [<Test>]
    let ``DenseMatrix.ofColumn`` () =
        DenseMatrix.ofColumns 3 2 (Seq.ofList [[0.3;0.3;0.3];[0.3;0.3;0.3]]) |> should equal smallM

    [<Test>]
    let ``DenseMatrix.ofColumnsList`` () =
        DenseMatrix.ofColumnsList 3 2 [[0.3;0.3;0.3];[0.3;0.3;0.3]] |> should equal smallM

    [<Test>]
    let ``DenseMatrix.ofSeqi`` () =
        seq { for i in 0 .. 99 do for j in 0 .. 119 -> (i,j, float i * 100.0 + float j)}
        |> DenseMatrix.ofSeqi 100 120 |> should equal largeM

    [<Test>]
    let ``DenseMatrix.ofListi`` () =
        [ for i in 0 .. 99 do for j in 0 .. 119 -> (i,j, float i * 100.0 + float j) ]
        |> DenseMatrix.ofListi 100 120 |> should equal largeM

    [<Test>]
    let ``DenseMatrix.constDiag`` () =
        DenseMatrix.constDiag 100 2.0 |> should equal (2.0 * (DenseMatrix.Identity 100))

    [<Test>]
    let ``DenseMatrix.diag`` () =
        DenseMatrix.diag (DenseVector.Create(100, fun i -> 2.0)) |> should equal (2.0 * (DenseMatrix.Identity 100))

    [<Test>]
    let ``DenseMatrix.init_row`` () =
        DenseMatrix.initRow 100 120 (fun i -> (DenseVector.init 120 (fun j -> float i * 100.0 + float j))) |> should equal largeM

    [<Test>]
    let ``DenseMatrix.init_col`` () =
        DenseMatrix.initCol 100 120 (fun j -> (DenseVector.init 100 (fun i -> float i * 100.0 + float j))) |> should equal largeM
