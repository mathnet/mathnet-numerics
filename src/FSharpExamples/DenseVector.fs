open MathNet.Numerics.FSharp
open MathNet.Numerics.LinearAlgebra

/// Create a new 100 dimensional dense vector.
let v = Double.DenseVector.init 100 (fun i -> float i / 100.0)

/// Another way to create a 100 dimensional dense vector is as follows.
let w = vector (List.init 100 (fun i -> float i ** 2.0))