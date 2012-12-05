namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra.Generic
open MathNet.Numerics.LinearAlgebra.Double

/// Unit tests for the sparse matrix type.
module SparseMatrixTests =

    /// A small uniform vector.
    let smallM = DenseMatrix.init 4 4 (fun i j -> if i = 1 && j = 2 then 1.0 else 0.0) :> Matrix<float>

    [<Test>]
    let ``SparseMatrix.ofList`` () =
        (SparseMatrix.ofList 4 4 [(1,2,1.0)] :> Matrix<float>) |> should equal smallM

    [<Test>]
    let ``SparseMatrix.ofSeq`` () =
        (SparseMatrix.ofSeq 4 4 (Seq.ofList [(1,2,1.0)]) :> Matrix<float>) |> should equal smallM

    [<Test>]
    let ``SparseMatrix.constDiag`` () =
        SparseMatrix.constDiag 100 2.0 |> should equal (2.0 * (SparseMatrix.Identity 100))

    [<Test>]
    let ``SparseMatrix.diag`` () =
        SparseMatrix.diag (new DenseVector(100, 2.0)) |> should equal (2.0 * (SparseMatrix.Identity 100))
