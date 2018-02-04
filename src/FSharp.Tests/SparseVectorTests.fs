namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnitTyped

open MathNet.Numerics.LinearAlgebra

/// Unit tests for the sparse vector type.
module SparseVectorTests =

    /// A small uniform vector.
    let smallv = DenseVector.raw [|0.0;0.3;0.0;0.0;0.0|]

    [<Test>]
    let ``SparseVector.ofListi`` () = SparseVector.ofListi 5 [ (1,0.3) ] |> shouldEqual smallv

    [<Test>]
    let ``SparseVector.ofSeqi`` () = SparseVector.ofSeqi 5 (List.toSeq [ (1,0.3) ]) |> shouldEqual smallv
