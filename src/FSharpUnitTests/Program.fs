open FsUnit
open MathNet.Numerics.FSharp
open MathNet.Numerics.LinearAlgebra.Double

/// Unit tests for the dense vector type.
let DenseVectorTests =

    /// A small uniform vector.
    let smallv = new DenseVector(5, 0.3 )

    /// A large vector with increasingly large entries
    let largev = new DenseVector( Array.init 100 (fun i -> float i / 100.0) )
    
    specs "DenseVector" [
        spec "DenseVector.init"
            (DenseVector.init 100 (fun i -> float i / 100.0) |> should equal largev)
        spec "DenseVector.of_list"
            (DenseVector.of_list [ for i in 0 .. 99 -> float i / 100.0 ] |> should equal largev)
        spec "DenseVector.of_seq"
            (DenseVector.of_seq (seq { for i in 0 .. 99 -> float i / 100.0 }) |> should equal largev)
        spec "DenseVector.rangef"
            (DenseVector.rangef 0.0 0.01 0.99 |> should equal (new DenseVector( [| for i in 0 .. 99 -> 0.01 * float i |] ) ))
        spec "DenseVector.range"
            (DenseVector.range 0 99 |> should equal (new DenseVector( [| for i in 0 .. 99 -> float i |] ) ))
    ]


/// Unit tests for the vector type.
let VectorTests =

    /// A small uniform vector.
    let smallv = new DenseVector( [|0.3;0.3;0.3;0.3;0.3|] ) :> Vector

    /// A large vector with increasingly large entries
    let largev = new DenseVector( Array.init 100 (fun i -> float i / 100.0) ) :> Vector
    
    specs "Vector" [
        spec "Vector.to_array"
            (Vector.to_array smallv |> should array_equal [|0.3;0.3;0.3;0.3;0.3|])
        spec "Vector.to_list"
            (Vector.to_list smallv |> should equal [0.3;0.3;0.3;0.3;0.3])
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
            (Vector.fold (fun a b -> a + b) 0.0 smallv |> should equal 1.5)
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
            (Vector.scan (fun acc x -> acc + x) smallv |> should approximately_vector_equal 14 (new DenseVector( [|0.3;0.6;0.9;1.2;1.5|] ) :> Vector) )
        spec "Vector.scanBack"
            (Vector.scanBack (fun x acc -> acc + x) smallv |> should approximately_vector_equal 14 (new DenseVector( [|1.5;1.2;0.9;0.6;0.3|] ) :> Vector) )
        spec "Vector.reduce_left"
            (Vector.reduce (fun acc x -> acc ** x) smallv |> should approximately_equal 14 0.990295218585507)
        spec "Vector.reduce_right"
            (Vector.reduceBack (fun x acc -> x ** acc) smallv |> should approximately_equal 14 0.488911287726319)
    ]


/// Unit tests for the matrix type.
let MatrixTests =

    /// A small uniform vector.
    let smallM = new DenseMatrix( Array2D.create 2 2 0.3 )

    /// A large vector with increasingly large entries
    let largeM = new DenseMatrix( Array2D.init 100 100 (fun i j -> float i * 100.0 + float j) )
    
    specs "Matrix" [
        spec "Matrix.fold"
            (Matrix.fold (fun a b -> a + b) 0.0 smallM |> should equal 1.2)
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
        (*spec "Matrix.map"
            (Matrix.map (fun x -> 2.0 * x) smallM |> should equal (2.0 * smallM))
        spec "Matrix.mapi"
            (Matrix.mapi (fun i j x -> float i * 100.0 + float j + x) largeM |> should equal (2.0 * largeM))
        spec "Matrix.inplaceAssign"
            ( let N = smallM.Clone()
              Matrix.inplaceAssign (fun i j -> 0.0) N
              N |> should equal (0.0 * smallM))
        spec "Matrix.inplaceMapi"
            ( let N = largeM.Clone()
              Matrix.inplaceMapi (fun i j x -> 2.0 * (float i * 100.0 + float j) + x) N
              N |> should equal (3.0 * largeM))*)
        spec "Matrix.nonZeroEntries"
            (Seq.length (Matrix.nonZeroEntries smallM) |> should equal 4)
        spec "Matrix.sum"
            (Matrix.sum smallM |> should equal 1.2)
        spec "Matrix.foldCol"
            (Matrix.foldCol (+) 0.0 largeM 0 |> should equal 495000.0)
        spec "Matrix.foldRow"
            (Matrix.foldRow (+) 0.0 largeM 0 |> should equal 4950.0)
        spec "Matrix.foldByCol"
            (Matrix.foldByCol (+) 0.0 smallM |> should equal (DenseVector.of_list [0.6;0.6] :> Vector))
        spec "Matrix.foldByRow"
            (Matrix.foldByRow (+) 0.0 smallM |> should equal (DenseVector.of_list [0.6;0.6] :> Vector))
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
        spec "DenseMatrix.identity"
            (DenseMatrix.identity 10 |> should equal (DenseMatrix.init 10 10 (fun i j -> if i = j then 1.0 else 0.0)))
        spec "DenseMatrix.of_list"
            (DenseMatrix.of_list [[0.3;0.3];[0.3;0.3]] |> should equal smallM)
        spec "DenseMatrix.of_seq"
            (DenseMatrix.of_seq (Seq.of_list [[0.3;0.3];[0.3;0.3]]) |> should equal smallM)
        spec "DenseMatrix.of_array2"
            (DenseMatrix.of_array2 (Array2D.create 2 2 0.3) |> should equal smallM)
        spec "DenseMatrix.init_dense"
            (DenseMatrix.init_dense 100 100 (seq { for i in 0 .. 99 do
                                                   for j in 0 .. 99 -> (i,j, float i * 100.0 + float j)}) |> should equal largeM)
        (*spec "DenseMatrix.constDiag"
            (DenseMatrix.constDiag 100 2.0 |> should equal (2.0 * (DenseMatrix.identity 100)))
        spec "DenseMatrix.diag"
            (DenseMatrix.diag (new DenseVector(100, 2.0)) |> should equal (2.0 * (DenseMatrix.identity 100)))
        spec "DenseMatrix.init_row"
            (DenseMatrix.init_row 100 100 (fun i -> (DenseVector.init 100 (fun j -> float i * 100.0 + float j))) |> should equal largeM)
        spec "DenseMatrix.init_col"
            (DenseMatrix.init_col 100 100 (fun j -> (DenseVector.init 100 (fun i -> float i * 100.0 + float j))) |> should equal largeM)
        spec "DenseMatrix.of_rowvector"
            (DenseMatrix.of_rowvector (new DenseVector(10,3.0)) |> should equal ((new DenseMatrix(1,10,3.0))))
        spec "DenseMatrix.of_vector"
            (DenseMatrix.of_vector (new DenseVector(10,3.0)) |> should equal ((new DenseMatrix(10,1,3.0))))*)
    ]

    
/// Report on errors and success and exit.
printfn "F# Test Results:"
printfn "%s" (Results.summary())
    
let code = if Results.erredCount() > 0 || Results.failedCount() > 0 then -1 else 0;;
exit code;;