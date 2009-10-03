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
            (Matrix.toArray2 smallM |> should equal (Array2D.create 2 2 0.3))
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
        spec "Matrix.foldCol"
            (Matrix.foldCol (+) 0.0 largeM 0 |> should equal 495000.0)
        spec "Matrix.foldRow"
            (Matrix.foldRow (+) 0.0 largeM 0 |> should equal 4950.0)
        spec "Matrix.foldByCol"
            (Matrix.foldByCol (+) 0.0 smallM |> should equal (DenseVector.of_list [0.6;0.6] :> Vector))
        spec "Matrix.foldByRow"
            (Matrix.foldByRow (+) 0.0 smallM |> should equal (DenseVector.of_list [0.6;0.6] :> Vector))
    ]
    
/// Report on errors and success and exit.
printfn "F# Test Results:"
printfn "%s" (Results.summary())
    
let code = if Results.erredCount() > 0 || Results.failedCount() > 0 then -1 else 0;;
exit code;;