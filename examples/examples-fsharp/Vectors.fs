module Vectors

open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.Distributions

// Create a dense vector of length 3 directly from an array (by reference, without copy)
let a1 = DenseVector [| 1.0; 2.0; 3.0 |]
let a2 = DenseVector.raw [| 1.0; 2.0; 3.0 |]

// Create a vector of length 100 with a given number for each value
let b1 : float Vector = DenseVector.zero 100
let b2 = DenseVector.create 100 20.5
let b3 : float Vector = SparseVector.zero 100

// Create a vector of length 100 with random values sampled from a distribution
let c : float Vector = DenseVector.random 100 (Normal.WithMeanStdDev(2.0, 0.5))

// Create a vector of length 100 with each value initialized by a lambda function
let d1 = DenseVector.init 100 (fun i -> float i / 100.0)
let d2 = SparseVector.init 100 (fun i -> if i%5 = 0 then float i / 100.0 else 0.0)

// Vectors can also be constructed from sequences
let e = DenseVector.ofSeq (seq { for i in 1 .. 100 do yield float i })

// Or from F# lists
let f = DenseVector.ofList [ for i in 1 .. 100 -> float i ]

// Or from indexed lists or sequences where all other values are zero (useful mostly for sparse data)
let g1 = DenseVector.ofListi 100 [(4,20.0); (18,3.0); (2,100.0)]
let g2 = SparseVector.ofListi 100 [(4,20.0); (18,3.0); (2,100.0)]
let g3 = SparseVector.ofSeqi 100 (Seq.ofList [(4,20.0); (18,3.0); (2,100.0)])

// Another way to create a 100 dimensional dense vector is using the vector function.
let h = vector (List.init 100 (fun i -> float i ** 2.0))

// We can now add two vectors together ...
let z = a1 + a2

// ... or scale them in the process.
let x = d2 + 3.0 * e - g2

// We can create a vector from an integer range (in this case, 5 and 10 inclusive) ...
let u = DenseVector.range 5 10

// ... or we can create a vector from a double range with a particular step size.
let v = DenseVector.rangef 0.0 0.1 10.0

// "pretty" printing (configurable with the Control class):
printfn "%s" (v.ToString())
// "printfn "%A" v" doesn't work yet because FSI treats it as sequence
