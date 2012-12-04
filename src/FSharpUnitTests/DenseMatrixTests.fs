namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra.Generic
open MathNet.Numerics.LinearAlgebra.Double

/// Unit tests for the dense matrix type.
module DenseMatrixTests =

    /// A small uniform vector.
    let smallM = new DenseMatrix( Array2D.create 2 2 0.3 )

    /// A large vector with increasingly large entries
    let largeM = new DenseMatrix( Array2D.init 100 100 (fun i j -> float i * 100.0 + float j) )

    [<Test>]
    let ``DenseMatrix.init`` () =
        DenseMatrix.init 100 100 (fun i j -> float i * 100.0 + float j) |> should equal largeM

    [<Test>]
    let ``DenseMatrix.ofList`` () =
        DenseMatrix.ofList [[0.3;0.3];[0.3;0.3]] |> should equal smallM

    [<Test>]
    let ``DenseMatrix.ofSeq`` () =
        DenseMatrix.ofSeq (Seq.ofList [[0.3;0.3];[0.3;0.3]]) |> should equal smallM

    [<Test>]
    let ``DenseMatrix.ofArray2`` () =
        DenseMatrix.ofArray2 (Array2D.create 2 2 0.3) |> should equal smallM

    [<Test>]
    let ``DenseMatrix.initDense`` () =
        DenseMatrix.initDense 100 100 (seq { for i in 0 .. 99 do
                                             for j in 0 .. 99 -> (i,j, float i * 100.0 + float j)}) |> should equal largeM
    [<Test>]
    let ``DenseMatrix.constDiag`` () =
        DenseMatrix.constDiag 100 2.0 |> should equal (2.0 * (DenseMatrix.Identity 100))

    [<Test>]
    let ``DenseMatrix.diag`` () =
        DenseMatrix.diag (new DenseVector(100, 2.0)) |> should equal (2.0 * (DenseMatrix.Identity 100))

    [<Test>]
    let ``DenseMatrix.init_row`` () =
        DenseMatrix.initRow 100 100 (fun i -> (DenseVector.init 100 (fun j -> float i * 100.0 + float j))) |> should equal largeM

    [<Test>]
    let ``DenseMatrix.init_col`` () =
        DenseMatrix.initCol 100 100 (fun j -> (DenseVector.init 100 (fun i -> float i * 100.0 + float j))) |> should equal largeM
