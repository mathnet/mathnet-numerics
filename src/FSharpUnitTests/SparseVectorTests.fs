namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra.Generic
open MathNet.Numerics.LinearAlgebra.Double

/// Unit tests for the sparse vector type.
module SparseVectorTests =

    /// A small uniform vector.
    let smallv = new DenseVector( [|0.0;0.3;0.0;0.0;0.0|] ) :> Vector<float>

    [<Test>]
    let ``SparseVector.ofList`` () =
        (SparseVector.ofList 5 [ (1,0.3) ] :> Vector<float>) |> should equal smallv

    [<Test>]
    let ``SparseVector.ofSeq`` () =
        (SparseVector.ofSeq 5 (List.toSeq [ (1,0.3) ]) :> Vector<float>) |> should equal smallv

