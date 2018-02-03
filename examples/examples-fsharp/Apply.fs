module Apply

open System.Numerics
open MathNet.Numerics
open MathNet.Numerics.Distributions
open MathNet.Numerics.Random
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double

/// The size of the vector we want to map things for.
let N = 1000000

/// The number of times we repeat a call.
let T = 10

/// The list of all functions we want to test.
let FunctionList : (string * (float -> float)) [] =
    [| ("Cosine", cos);
       ("Sine", sin);
       ("Tangent", tan);
       ("Inverse Cosine", acos);
       ("Inverse Sine", asin);
       ("Inverse Tangent", atan);
       ("Hyperbolic Cosine", cosh);
       ("Hyperbolic Sine", sinh);
       ("Hyperbolic Tangent", tanh);
       ("Abs", abs);
       ("Exp", exp);
       ("Log", log);
       ("Sqrt", sqrt);
       ("Error Function", SpecialFunctions.Erf);
       ("Error Function Complement", SpecialFunctions.Erfc);
       ("Inverse Error Function", SpecialFunctions.ErfInv);
       ("Inverse Error Function Complement", SpecialFunctions.ErfcInv) |]

/// A vector with random entries.
let w =
    let dist = Normal(1.0, 10.0, Random.mersenneTwister ())
    DenseVector.random N dist

/// A stopwatch to time the execution.
let sw = System.Diagnostics.Stopwatch()


printfn "%d-dimensional vector for %d iterations:" N T

for (name, f) in FunctionList do

    let v = w.Clone()

    sw.Restart()
    for t in 1 .. T do Vector.mapInPlace f v
    sw.Stop()

    printfn "%s:\t\t%d ms" name sw.ElapsedMilliseconds
