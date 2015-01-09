namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra

/// Unit tests for the sparse matrix type.
module SparseMatrixTests =

    /// A small uniform matrix.
    let smallM = DenseMatrix.init 4 6 (fun i j -> if i = 1 && j = 2 then 1.0 else 0.0)

    [<Test>]
    let ``SparseMatrix.zero`` () =
        (SparseMatrix.zero 4 6) + smallM |> should equal smallM

    [<Test>]
    let ``SparseMatrix.init`` () =
        SparseMatrix.init 4 6 (fun i j -> if i = 1 && j = 2 then 1.0 else 0.0) |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofArray2`` () =
        SparseMatrix.ofArray2 (array2D [[0.;0.;0.;0.;0.;0.];[0.;0.;1.;0.;0.;0.];[0.;0.;0.;0.;0.;0.];[0.;0.;0.;0.;0.;0.]]) |> should equal smallM
        SparseMatrix.ofArray2 (Array2D.init 4 6 (fun i j -> if i = 1 && j = 2 then 1.0 else 0.0)) |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofMatrixArray2`` () =
        let a = SparseMatrix.ofMatrixArray2 (array2D [[smallM;smallM];[smallM;smallM];[smallM;smallM]])
        a.[0..3,0..5] |> should equal smallM
        a.[4..7,6..11] |> should equal smallM
        a.[8..11,0..5] |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofMatrixList2`` () =
        let a = SparseMatrix.ofMatrixList2 [[smallM; smallM]; [smallM; smallM]; [smallM; smallM]]
        a.[0..3,0..5] |> should equal smallM
        a.[4..7,6..11] |> should equal smallM
        a.[8..11,0..5] |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofRowSeq`` () =
        SparseMatrix.ofRowSeq (Seq.ofList [[0.;0.;0.;0.;0.;0.];[0.;0.;1.;0.;0.;0.];[0.;0.;0.;0.;0.;0.];[0.;0.;0.;0.;0.;0.]]) |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofRowList`` () =
        SparseMatrix.ofRowList [[0.;0.;0.;0.;0.;0.];[0.;0.;1.;0.;0.;0.];[0.;0.;0.;0.;0.;0.];[0.;0.;0.;0.;0.;0.]] |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofColumnSeq`` () =
        SparseMatrix.ofColumnSeq (Seq.ofList [[0.;0.;0.;0.];[0.;0.;0.;0.];[0.;1.;0.;0.];[0.;0.;0.;0.];[0.;0.;0.;0.];[0.;0.;0.;0.]]) |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofColumnList`` () =
        SparseMatrix.ofColumnList [[0.;0.;0.;0.];[0.;0.;0.;0.];[0.;1.;0.;0.];[0.;0.;0.;0.];[0.;0.;0.;0.];[0.;0.;0.;0.]] |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofSeqi`` () =
        SparseMatrix.ofSeqi 4 6 (Seq.ofList [(1,2,1.0)]) |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofListi`` () =
        SparseMatrix.ofListi 4 6 [(1,2,1.0)] |> should equal smallM

    [<Test>]
    let ``SparseMatrix.diag`` () =
        SparseMatrix.diag 100 2.0 |> should equal (2.0 * (SparseMatrix.identity 100))

    [<Test>]
    let ``SparseMatrix.diag2`` () =
        SparseMatrix.diag2 100 120 2.0 |> should equal (2.0 * (SparseMatrix.identity2 100 120))

    [<Test>]
    let ``SparseMatrix.ofDiag`` () =
        SparseMatrix.ofDiag (DenseVector.init 100 (fun i -> 2.0)) |> should equal (2.0 * (SparseMatrix.identity 100))

    [<Test>]
    let ``SparseMatrix.init_row`` () =
        SparseMatrix.initRows 4 (fun i -> if i=1 then DenseVector.raw [|0.;0.;1.;0.;0.;0.|] else DenseVector.zero 6) |> should equal smallM

    [<Test>]
    let ``SparseMatrix.init_col`` () =
        SparseMatrix.initColumns 6 (fun j -> if j=2 then DenseVector.raw [|0.;1.;0.;0.|] else DenseVector.zero 4) |> should equal smallM
