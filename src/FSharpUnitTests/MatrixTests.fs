namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra.Generic
open MathNet.Numerics.LinearAlgebra.Double

/// Unit tests for the matrix type.
module MatrixTests =

    /// A small uniform vector.
    let smallM = DenseMatrix.OfArray( Array2D.create 2 2 0.3 )
    let failingFoldBackM = DenseMatrix.init 2 3 (fun i j -> 1.0)

    /// A small sparse matrix.
    let sparseM = SparseMatrix.ofListi 2 3 [(1,0,0.3)]

    /// A large vector with increasingly large entries
    let largeM = DenseMatrix.OfArray( Array2D.init 100 100 (fun i j -> float i * 100.0 + float j) )

    [<Test>]
    let ``Matrix.GetSlice`` () =
        largeM.[*,*] |> should equal largeM
        largeM.[0..99,0..99]  |> should equal largeM
        largeM.[1..2,1..2]  |> should equal (DenseMatrix(2,2,[|101.;201.;102.;202.|]))
        largeM.[98..,98..]  |> should equal (DenseMatrix(2,2,[|9898.;9998.;9899.;9999.|]))
        largeM.[..1,..1]  |> should equal (DenseMatrix(2,2,[|0.;100.;1.;101.|]))

    [<Test>]
    let ``Matrix.SetSlice`` () =
        let m = DenseMatrix.init 2 2 (fun i j -> float i * 100.0 + float j) in
            m.[*,*] <- DenseMatrix(2,2,[|5.;6.;7.;8.|]);
            m |> should equal (DenseMatrix(2,2,[|5.;6.;7.;8.|]))
        let m = DenseMatrix.init 2 2 (fun i j -> float i * 100.0 + float j) in
            m.[0..1,0..1] <- DenseMatrix(2,2,[|5.;6.;7.;8.|]);
            m |> should equal (DenseMatrix(2,2,[|5.;6.;7.;8.|]))
        let m = DenseMatrix.init 4 4 (fun i j -> float i * 100.0 + float j) in
            m.[1..2,1..2] <- DenseMatrix(2,2,[|5.;6.;7.;8.|]);
            m |> should equal (DenseMatrix(4,4,[|0.;100.;200.;300.;1.;5.;6.;301.;2.;7.;8.;302.;3.;103.;203.;303.|]))
        let m = DenseMatrix.init 4 4 (fun i j -> float i * 100.0 + float j) in
            m.[2..,..1] <- DenseMatrix(2,2,[|5.;6.;7.;8.|]);
            m |> should equal (DenseMatrix(4,4,[|0.;100.;5.;6.;1.;101.;7.;8.;2.;102.;202.;302.;3.;103.;203.;303.|]))
        let m = DenseMatrix.init 4 4 (fun i j -> float i * 100.0 + float j) in
            m.[..1,2..] <- DenseMatrix(2,2,[|5.;6.;7.;8.|]);
            m |> should equal (DenseMatrix(4,4,[|0.;100.;200.;300.;1.;101.;201.;301.;5.;6.;202.;302.;7.;8.;203.;303.|]))

    [<Test>]
    let ``Matrix.toArray2`` () =
        Matrix.toArray2 smallM |> should array2_equal (Array2D.create 2 2 0.3)

    [<Test>]
    let ``Matrix.mapInPlace.Dense`` () =
        let M = largeM.Clone()
        M |> Matrix.mapInPlace (fun x -> 3.0 * x)
        M |> should equal (3.0 * largeM)

    [<Test>]
    let ``Matrix.mapInPlace.Sparse`` () =
        let M = sparseM.Clone()
        M |> Matrix.mapInPlace (fun x -> 3.0 * x)
        M |> should equal (3.0 * sparseM)

    [<Test>]
    let ``Matrix.mapnzInPlace.Sparse`` () =
        let M = sparseM.Clone()
        M |> Matrix.mapnzInPlace (fun x -> 3.0 * x)
        M |> should equal (3.0 * sparseM)

    [<Test>]
    let ``Matrix.mapiInPlace.Dense`` () =
        let M = largeM.Clone()
        M |> Matrix.mapiInPlace (fun i j x -> 2.0 * (float i * 100.0 + float j) + x)
        M |> should equal (3.0 * largeM)

    [<Test>]
    let ``Matrix.mapiInPlace.Sparse`` () =
        let M = sparseM.Clone()
        M |> Matrix.mapiInPlace (fun i j x -> if i=j then 2.0*x+1.0 else 2.0*x)
        M |> should equal (2.0 * sparseM + SparseMatrix.init 2 3 (fun i j -> if i=j then 1.0 else 0.0))

    [<Test>]
    let ``Matrix.mapinzInPlace.Sparse`` () =
        let M = sparseM.Clone()
        M |> Matrix.mapinzInPlace (fun i j x -> 2.0*x)
        M |> should equal (2.0 * sparseM)

    [<Test>]
    let ``Matrix.map`` () =
        Matrix.map (fun x -> 2.0 * x) smallM |> should equal (2.0 * smallM)

    [<Test>]
    let ``Matrix.mapnz`` () =
        Matrix.mapnz (fun x -> 2.0 * x) smallM |> should equal (2.0 * smallM)

    [<Test>]
    let ``Matrix.mapi`` () =
        Matrix.mapi (fun i j x -> float i * 100.0 + float j + x) largeM |> should equal (2.0 * largeM)

    [<Test>]
    let ``Matrix.mapinz`` () =
        Matrix.mapinz (fun i j x -> float i * 100.0 + float j + x) largeM |> should equal (2.0 * largeM)

    [<Test>]
    let ``Matrix.mapCols`` () =
        Matrix.mapCols (fun j col -> col.Add(float j)) smallM |> should (approximately_matrix_equal 14) (matrix [[0.3;1.3];[0.3;1.3]])

    [<Test>]
    let ``Matrix.mapRows`` () =
        Matrix.mapRows (fun i row -> row.Add(float i)) smallM |> should (approximately_matrix_equal 14) (matrix [[0.3;0.3];[1.3;1.3]])

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
    let ``Matrix.inplaceAssign`` () =
        let N = smallM.Clone()
        N |> Matrix.inplaceAssign (fun i j -> 0.0)
        N |> should equal (0.0 * smallM)

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
