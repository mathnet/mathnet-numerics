open FsUnit
open MathNet.Numerics.FSharp
open MathNet.Numerics.LinearAlgebra

/// Unit tests for the dense vector type.
let DenseVectorTests =

    /// A small uniform vector.
    let smallv = new Double.DenseVector(5, 0.3 )

    (*/// A large vector with increasingly large entries
    let largev = new Double.DenseVector( Array.init 100 (fun i -> float i / 100.0) )
    
    specs "DenseVector" [
        spec "DenseVector.init"
            (Double.DenseVector.init 100 (fun i -> float i / 100.0) |> should equal largev)
        spec "DenseVector.of_list"
            (Double.DenseVector.of_list [ for i in 0 .. 99 -> float i / 100.0 ] |> should equal largev)
    ]*)
    ()