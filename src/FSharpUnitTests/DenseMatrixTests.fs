namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open FsUnitTyped

open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.Distributions
open MathNet.Numerics.Statistics

/// Unit tests for the dense matrix type.
module DenseMatrixTests =

    /// A small uniform matrix.
    let smallM = DenseMatrix.raw 3 2 [|0.3;0.3;0.3;0.3;0.3;0.3|]

    /// A large matrix with increasingly large entries
    let largeM =
        Array.init (100*120) (fun k -> let i, j = (k%100),(k/100) in float i * 100.0 + float j)
        |> DenseMatrix.raw 100 120

    [<Test>]
    let ``DenseMatrix.zero`` () =
        (DenseMatrix.zero 100 120) + largeM |> shouldEqual largeM

    [<Test>]
    let ``DenseMatrix.random`` () =
        let m = DenseMatrix.random 100 120 (Normal.WithMeanStdDev(100.0,0.1))
        (m :?> Double.DenseMatrix).Values |> ArrayStatistics.Mean |> should (equalWithin 10.0) 100.0
        m.RowCount |> shouldEqual 100
        m.ColumnCount |> shouldEqual 120

    [<Test>]
    let ``DenseMatrix.create`` () =
        DenseMatrix.create 3 2 0.3 |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.init`` () =
        DenseMatrix.init 3 2 (fun i j -> 0.3) |> shouldEqual smallM
        DenseMatrix.init 100 120 (fun i j -> float i * 100.0 + float j) |> shouldEqual largeM

    [<Test>]
    let ``DenseMatrix.ofArray2`` () =
        DenseMatrix.ofArray2 (array2D [[0.3;0.3];[0.3;0.3];[0.3;0.3]]) |> shouldEqual smallM
        DenseMatrix.ofArray2 (Array2D.create 3 2 0.3) |> shouldEqual smallM
        DenseMatrix.ofArray2 (Array2D.init 100 120 (fun i j -> float i * 100.0 + float j)) |> shouldEqual largeM

    [<Test>]
    let ``DenseMatrix.ofMatrixArray2`` () =
        let a = DenseMatrix.ofMatrixArray2 (array2D [[smallM;smallM];[smallM;smallM];[smallM;smallM]])
        a.[0..2,0..1] |> shouldEqual smallM
        a.[3..5,2..3] |> shouldEqual smallM
        a.[6..8,0..1] |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.ofMatrixList2`` () =
        let a = DenseMatrix.ofMatrixList2 [[smallM; smallM]; [smallM; smallM]; [smallM; smallM]]
        a.[0..2,0..1] |> shouldEqual smallM
        a.[3..5,2..3] |> shouldEqual smallM
        a.[6..8,0..1] |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.ofRowSeq`` () =
        DenseMatrix.ofRowSeq (Seq.ofList [[0.3;0.3];[0.3;0.3];[0.3;0.3]]) |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.ofRowList`` () =
        DenseMatrix.ofRowList [[0.3;0.3];[0.3;0.3];[0.3;0.3]] |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.ofRows`` () =
        DenseMatrix.ofRows [vector [0.3;0.3]; vector [0.3;0.3]; vector [0.3;0.3]] |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.ofColumnSeq`` () =
        DenseMatrix.ofColumnSeq (Seq.ofList [[0.3;0.3;0.3];[0.3;0.3;0.3]]) |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.ofColumnList`` () =
        DenseMatrix.ofColumnList [[0.3;0.3;0.3];[0.3;0.3;0.3]] |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.ofColumns`` () =
        DenseMatrix.ofColumns [vector [0.3;0.3;0.3]; vector [0.3;0.3;0.3]] |> shouldEqual smallM

    [<Test>]
    let ``DenseMatrix.ofSeqi`` () =
        seq { for i in 0 .. 99 do for j in 0 .. 119 -> (i,j, float i * 100.0 + float j)}
        |> DenseMatrix.ofSeqi 100 120 |> shouldEqual largeM

    [<Test>]
    let ``DenseMatrix.ofListi`` () =
        [ for i in 0 .. 99 do for j in 0 .. 119 -> (i,j, float i * 100.0 + float j) ]
        |> DenseMatrix.ofListi 100 120 |> shouldEqual largeM

    [<Test>]
    let ``DenseMatrix.diag`` () =
        DenseMatrix.diag 100 2.0 |> shouldEqual (2.0 * (DenseMatrix.identity 100))

    [<Test>]
    let ``DenseMatrix.diag2`` () =
        DenseMatrix.diag2 100 120 2.0 |> shouldEqual (2.0 * (DenseMatrix.identity2 100 120))

    [<Test>]
    let ``DenseMatrix.ofDiag`` () =
        DenseMatrix.ofDiag (DenseVector.init 100 (fun i -> 2.0)) |> shouldEqual (2.0 * (DenseMatrix.identity 100))

    [<Test>]
    let ``DenseMatrix.initRow`` () =
        DenseMatrix.initRows 100 (fun i -> (DenseVector.init 120 (fun j -> float i * 100.0 + float j))) |> shouldEqual largeM

    [<Test>]
    let ``DenseMatrix.initCol`` () =
        DenseMatrix.initColumns 120 (fun j -> (DenseVector.init 100 (fun i -> float i * 100.0 + float j))) |> shouldEqual largeM
