open MathNet.Numerics.FSharp
open MathNet.Numerics.LinearAlgebra

// Create a new 100 dimensional dense vector.
let v = Double.DenseVector.init 100 (fun i -> float i / 100.0)

// Another way to create a 100 dimensional dense vector is using the vector function.
let w = vector (List.init 100 (fun i -> float i ** 2.0))

// Vectors can also be constructed from sequences.
let t = Double.DenseVector.of_seq (seq { for i in 1 .. 100 do yield float i })

// We can now add two vectors together ...
let z = v + w

// ... or scale them in the process.
let x = v + 3.0 * t



// We can create a vector from an integer range (in this case, 5 and 10 inclusive) ...
let s = Double.DenseVector.range 5 10

// ... or we can create a vector from a double range with a particular step size.
let r = Double.DenseVector.rangef 0.0 0.1 10.0