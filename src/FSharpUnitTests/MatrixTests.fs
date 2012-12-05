namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra.Generic
open MathNet.Numerics.LinearAlgebra.Double

/// Unit tests for the matrix type.
module MatrixTests =

    /// A small uniform vector.
    let smallM = new DenseMatrix( Array2D.create 2 2 0.3 )
    let failingFoldBackM = DenseMatrix.init 2 3 (fun i j -> 1.0)

    /// A large vector with increasingly large entries
    let largeM = new DenseMatrix( Array2D.init 100 100 (fun i j -> float i * 100.0 + float j) )

    [<Test>]
    let ``Matrix.fold`` () =
        Matrix.fold (fun a b -> a - b) 0.0 smallM |> should equal -1.2

    [<Test>]
    let ``Matrix.foldBack`` () =
        Matrix.foldBack (fun a b -> a - b) 0.0 smallM |> should equal 0.0

    [<Test>]
    let ``Matrix.foldBackSummation`` () =
        Matrix.foldBack( fun a b -> a + b) 0.0 failingFoldBackM |> should equal 6.0

    [<Test>]
    let ``Matrix.foldi`` () =
        Matrix.foldi (fun i j acc x -> acc + x + float (i+j)) 0.0 smallM |> should equal 5.2

    [<Test>]
    let ``Matrix.toArray2`` () =
        Matrix.toArray2 smallM |> should array2_equal (Array2D.create 2 2 0.3)

    [<Test>]
    let ``Matrix.forall`` () =
        Matrix.forall (fun x -> x = 0.3) smallM |> should equal true

    [<Test>]
    let ``Matrix.exists`` () =
        Matrix.exists (fun x -> x = 0.5) smallM |> should equal false

    [<Test>]
    let ``Matrix.foralli`` () =
        Matrix.foralli (fun i j x -> x = float i * 100.0 + float j) largeM |> should equal true

    [<Test>]
    let ``Matrix.existsi`` () =
        Matrix.existsi (fun i j x -> x = float i * 100.0 + float j) largeM |> should equal true

    [<Test>]
    let ``Matrix.map`` () =
        Matrix.map (fun x -> 2.0 * x) smallM |> should equal (2.0 * smallM)

    [<Test>]
    let ``Matrix.mapi`` () =
        Matrix.mapi (fun i j x -> float i * 100.0 + float j + x) largeM |> should equal (2.0 * largeM)

    [<Test>]
    let ``Matrix.mapCols`` () =
        Matrix.mapCols (fun j col -> col.Add(float j)) smallM |> should (approximately_matrix_equal 14) (matrix [[0.3;1.3];[0.3;1.3]])

    [<Test>]
    let ``Matrix.mapRows`` () =
        Matrix.mapRows (fun i row -> row.Add(float i)) smallM |> should (approximately_matrix_equal 14) (matrix [[0.3;0.3];[1.3;1.3]])

    [<Test>]
    let ``Matrix.inplaceAssign`` () =
        let N = smallM.Clone()
        Matrix.inplaceAssign (fun i j -> 0.0) N
        N |> should equal (0.0 * smallM)

    [<Test>]
    let ``Matrix.inplaceMapi`` () =
        let N = largeM.Clone()
        Matrix.inplaceMapi (fun i j x -> 2.0 * (float i * 100.0 + float j) + x) N
        N |> should equal (3.0 * largeM)

    [<Test>]
    let ``Matrix.nonZeroEntries`` () =
        Seq.length (Matrix.nonZeroEntries smallM) |> should equal 4

    [<Test>]
    let ``Matrix.sum`` () =
        Matrix.sum smallM |> should equal 1.2

    [<Test>]
    let ``Matrix.sumColsBy`` () =
        Matrix.sumColsBy (fun j col -> col.[0] * col.[1]) (matrix [[1.0; 2.0]; [3.0; 4.0]]) |> should equal 11.0

    [<Test>]
    let ``Matrix.sumRowsBy`` () =
        Matrix.sumRowsBy (fun i row -> row.[0] * row.[1]) (matrix [[1.0; 2.0]; [3.0; 4.0]]) |> should equal 14.0

    [<Test>]
    let ``Matrix.foldCol`` () =
        Matrix.foldCol (+) 0.0 largeM 0 |> should equal 495000.0

    [<Test>]
    let ``Matrix.foldRow`` () =
        Matrix.foldRow (+) 0.0 largeM 0 |> should equal 4950.0

    [<Test>]
    let ``Matrix.foldByCol`` () =
        Matrix.foldByCol (+) 0.0 smallM |> should equal (DenseVector.ofList [0.6;0.6] :> Vector<float>)

    [<Test>]
    let ``Matrix.foldByRow`` () =
        Matrix.foldByRow (+) 0.0 smallM |> should equal (DenseVector.ofList [0.6;0.6] :> Vector<float>)
