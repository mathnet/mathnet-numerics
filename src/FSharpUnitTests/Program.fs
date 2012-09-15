open FsUnit
open MathNet.Numerics.FSharp
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic

/// Unit tests for the dense vector type.
let DenseVectorTests =

    /// A small uniform vector.
    let smallv = new DenseVector(5, 0.3 )

    /// A large vector with increasingly large entries
    let largev = new DenseVector( Array.init 100 (fun i -> float i / 100.0) )
    
    specs "DenseVector" [
        spec "DenseVector.init"
            (DenseVector.init 100 (fun i -> float i / 100.0) |> should equal largev)
        spec "DenseVector.ofList"
            (DenseVector.ofList [ for i in 0 .. 99 -> float i / 100.0 ] |> should equal largev)
        spec "DenseVector.ofSeq"
            (DenseVector.ofSeq (seq { for i in 0 .. 99 -> float i / 100.0 }) |> should equal largev)
        spec "DenseVector.rangef"
            (DenseVector.rangef 0.0 0.01 0.99 |> should equal (new DenseVector( [| for i in 0 .. 99 -> 0.01 * float i |] ) ))
        spec "DenseVector.range"
            (DenseVector.range 0 99 |> should equal (new DenseVector( [| for i in 0 .. 99 -> float i |] ) ))
    ]


/// Unit tests for the sparse vector type.
let SparseVectorTests =

    /// A small uniform vector.
    let smallv = new DenseVector( [|0.0;0.3;0.0;0.0;0.0|] ) :> Vector<float>
    
    specs "SparseVector" [
        spec "SparseVector.ofList"
            ((SparseVector.ofList 5 [ (1,0.3) ] :> Vector<float>) |> should equal smallv)
        spec "SparseVector.ofSeq"
            ((SparseVector.ofSeq 5 (List.toSeq [ (1,0.3) ]) :> Vector<float>) |> should equal smallv)
    ]


/// Unit tests for the vector type.
let VectorTests =

    /// A small uniform vector.
    let smallv = new DenseVector( [|0.3;0.3;0.3;0.3;0.3|] ) :> Vector<float>

    /// A large vector with increasingly large entries
    let largev = new DenseVector( Array.init 100 (fun i -> float i / 100.0) ) :> Vector<float>
    
    specs "Vector" [
        spec "Vector.toArray"
            (Vector.toArray smallv |> should array_equal [|0.3;0.3;0.3;0.3;0.3|])
        spec "Vector.toList"
            (Vector.toList smallv |> should equal [0.3;0.3;0.3;0.3;0.3])
        spec "Vector.mapInPlace"
            ( let w = smallv.Clone()
              Vector.mapInPlace (fun x -> 2.0 * x) w
              w |> should equal (2.0 * smallv))
        spec "Vector.mapiInPlace"
            ( let w = largev.Clone()
              Vector.mapiInPlace (fun i x -> float i / 100.0) w
              w |> should equal (largev))
        spec "Vector.addInPlace"
            ( let w = largev.Clone()
              Vector.addInPlace w largev
              w |> should equal (2.0 * largev))
        spec "Vector.subInPlace"
            ( let w = largev.Clone()
              Vector.subInPlace w largev
              w |> should equal (0.0 * largev))
        spec "Vector.map"
            (Vector.map (fun x -> 2.0 * x) largev |> should equal (2.0 * largev))
        spec "Vector.mapi"
            (Vector.mapi (fun i x -> float i / 100.0) largev |> should equal largev)
        spec "Vector.fold"
            (Vector.fold (fun a b -> a - b) 0.0 smallv |> should equal -1.5)
        spec "Vector.foldBack"
            (Vector.foldBack (fun a b -> a - b) 0.0 smallv |> should equal 0.0)
        spec "Vector.foldi"
            (Vector.foldi (fun i a b -> a + b) 0.0 smallv |> should equal 1.5)
        spec "Vector.forall"
            (Vector.forall (fun x -> x = 0.3) smallv |> should equal true)
        spec "Vector.exists"
            (Vector.exists (fun x -> x = 0.3) smallv |> should equal true)
        spec "Vector.foralli"
            (Vector.foralli (fun i x -> x = 0.3 && i < 5) smallv |> should equal true)
        spec "Vector.existsi"
            (Vector.existsi (fun i x -> x = 0.3 && i = 2) smallv |> should equal true)
        spec "Vector.scan"
            (Vector.scan (fun acc x -> acc + x) smallv |> should approximately_vector_equal 14 (new DenseVector( [|0.3;0.6;0.9;1.2;1.5|] ) :> Vector<float>) )
        spec "Vector.scanBack"
            (Vector.scanBack (fun x acc -> acc + x) smallv |> should approximately_vector_equal 14 (new DenseVector( [|1.5;1.2;0.9;0.6;0.3|] ) :> Vector<float>) )
        spec "Vector.reduce"
            (Vector.reduce (fun acc x -> acc ** x) smallv |> should approximately_equal 14 0.990295218585507)
        spec "Vector.reduceBack"
            (Vector.reduceBack (fun x acc -> x ** acc) smallv |> should approximately_equal 14 0.488911287726319)
        spec "Vector.insert"
            (Vector.insert 2 0.5 smallv |> should approximately_vector_equal 14 (new DenseVector ( [|0.3;0.3;0.5;0.3;0.3;0.3|] ) :> Vector<float>) )
    ]


/// Unit tests for the matrix type.
let MatrixTests =

    /// A small uniform vector.
    let smallM = new DenseMatrix( Array2D.create 2 2 0.3 )
    let failingFoldBackM = DenseMatrix.init 2 3 (fun i j -> 1.0)


    /// A large vector with increasingly large entries
    let largeM = new DenseMatrix( Array2D.init 100 100 (fun i j -> float i * 100.0 + float j) )
    
    specs "Matrix" [
        spec "Matrix.fold"
            (Matrix.fold (fun a b -> a - b) 0.0 smallM |> should equal -1.2)
        spec "Matrix.foldBack"
            (Matrix.foldBack (fun a b -> a - b) 0.0 smallM |> should equal 0.0)
        spec "Matrix.foldBackSummation"
            (Matrix.foldBack( fun a b -> a + b) 0.0 failingFoldBackM |> should equal 6.0)
        spec "Matrix.foldi"
            (Matrix.foldi (fun i j acc x -> acc + x + float (i+j)) 0.0 smallM |> should equal 5.2)
        spec "Matrix.toArray2"
            (Matrix.toArray2 smallM |> should array2_equal (Array2D.create 2 2 0.3))
        spec "Matrix.forall"
            (Matrix.forall (fun x -> x = 0.3) smallM |> should equal true)
        spec "Matrix.exists"
            (Matrix.exists (fun x -> x = 0.5) smallM |> should equal false)
        spec "Matrix.foralli"
            (Matrix.foralli (fun i j x -> x = float i * 100.0 + float j) largeM |> should equal true)
        spec "Matrix.existsi"
            (Matrix.existsi (fun i j x -> x = float i * 100.0 + float j) largeM |> should equal true)
        spec "Matrix.map"
            (Matrix.map (fun x -> 2.0 * x) smallM |> should equal (2.0 * smallM))
        spec "Matrix.mapi"
            (Matrix.mapi (fun i j x -> float i * 100.0 + float j + x) largeM |> should equal (2.0 * largeM))
        spec "Matrix.mapCols"
            (Matrix.mapCols (fun j col -> col.Add(float j)) smallM |> should approximately_matrix_equal 14 (matrix [[0.3;1.3];[0.3;1.3]]))
        spec "Matrix.mapRows"
            (Matrix.mapRows (fun i row -> row.Add(float i)) smallM |> should approximately_matrix_equal 14 (matrix [[0.3;0.3];[1.3;1.3]]))
        spec "Matrix.inplaceAssign"
            ( let N = smallM.Clone()
              Matrix.inplaceAssign (fun i j -> 0.0) N
              N |> should equal (0.0 * smallM))
        spec "Matrix.inplaceMapi"
            ( let N = largeM.Clone()
              Matrix.inplaceMapi (fun i j x -> 2.0 * (float i * 100.0 + float j) + x) N
              N |> should equal (3.0 * largeM))
        spec "Matrix.nonZeroEntries"
            (Seq.length (Matrix.nonZeroEntries smallM) |> should equal 4)
        spec "Matrix.sum"
            (Matrix.sum smallM |> should equal 1.2)
        spec "Matrix.sumColsBy"
            (Matrix.sumColsBy (fun j col -> col.[0] * col.[1]) (matrix [[1.0; 2.0]; [3.0; 4.0]]) |> should equal 11.0)
        spec "Matrix.sumRowsBy"
            (Matrix.sumRowsBy (fun i row -> row.[0] * row.[1]) (matrix [[1.0; 2.0]; [3.0; 4.0]]) |> should equal 14.0)
        spec "Matrix.foldCol"
            (Matrix.foldCol (+) 0.0 largeM 0 |> should equal 495000.0)
        spec "Matrix.foldRow"
            (Matrix.foldRow (+) 0.0 largeM 0 |> should equal 4950.0)
        spec "Matrix.foldByCol"
            (Matrix.foldByCol (+) 0.0 smallM |> should equal (DenseVector.ofList [0.6;0.6] :> Vector<float>))
        spec "Matrix.foldByRow"
            (Matrix.foldByRow (+) 0.0 smallM |> should equal (DenseVector.ofList [0.6;0.6] :> Vector<float>))
    ]


/// Unit tests for the dense matrix type.
let DenseMatrixTests =

    /// A small uniform vector.
    let smallM = new DenseMatrix( Array2D.create 2 2 0.3 )

    /// A large vector with increasingly large entries
    let largeM = new DenseMatrix( Array2D.init 100 100 (fun i j -> float i * 100.0 + float j) )
    
    specs "DenseMatrix" [
        spec "DenseMatrix.init"
            (DenseMatrix.init 100 100 (fun i j -> float i * 100.0 + float j) |> should equal largeM)
        spec "DenseMatrix.ofList"
            (DenseMatrix.ofList [[0.3;0.3];[0.3;0.3]] |> should equal smallM)
        spec "DenseMatrix.ofSeq"
            (DenseMatrix.ofSeq (Seq.ofList [[0.3;0.3];[0.3;0.3]]) |> should equal smallM)
        spec "DenseMatrix.ofArray2"
            (DenseMatrix.ofArray2 (Array2D.create 2 2 0.3) |> should equal smallM)
        spec "DenseMatrix.initDense"
            (DenseMatrix.initDense 100 100 (seq { for i in 0 .. 99 do
                                                   for j in 0 .. 99 -> (i,j, float i * 100.0 + float j)}) |> should equal largeM)
        spec "DenseMatrix.constDiag"
            (DenseMatrix.constDiag 100 2.0 |> should equal (2.0 * (DenseMatrix.Identity 100)))
        spec "DenseMatrix.diag"
            (DenseMatrix.diag (new DenseVector(100, 2.0)) |> should equal (2.0 * (DenseMatrix.Identity 100)))
        spec "DenseMatrix.init_row"
            (DenseMatrix.initRow 100 100 (fun i -> (DenseVector.init 100 (fun j -> float i * 100.0 + float j))) |> should equal largeM)
        spec "DenseMatrix.init_col"
            (DenseMatrix.initCol 100 100 (fun j -> (DenseVector.init 100 (fun i -> float i * 100.0 + float j))) |> should equal largeM)
    ]
    

/// Unit tests for the sparse matrix type.
let SparseMatrixTests =

    /// A small uniform vector.
    let smallM = DenseMatrix.init 4 4 (fun i j -> if i = 1 && j = 2 then 1.0 else 0.0) :> Matrix<float>
    
    specs "SparseMatrix" [
        spec "SparseMatrix.ofList"
            ((SparseMatrix.ofList 4 4 [(1,2,1.0)] :> Matrix<float>) |> should equal smallM)
        spec "SparseMatrix.ofSeq"
            ((SparseMatrix.ofSeq 4 4 (Seq.ofList [(1,2,1.0)]) :> Matrix<float>) |> should equal smallM)
        spec "SparseMatrix.constDiag"
            (SparseMatrix.constDiag 100 2.0 |> should equal (2.0 * (SparseMatrix.Identity 100)))
        spec "SparseMatrix.diag"
            (SparseMatrix.diag (new DenseVector(100, 2.0)) |> should equal (2.0 * (SparseMatrix.Identity 100)))
    ]

    
/// Report on errors and success and exit.
printfn "F# Test Results:"
printfn "%s" (Results.summary())
    
let code = if Results.erredCount() > 0 || Results.failedCount() > 0 then -1 else 0;;
exit code;;