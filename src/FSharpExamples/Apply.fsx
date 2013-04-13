// <copyright file="Apply.fsx" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

#r "../../out/lib/Net40/MathNet.Numerics.dll"
#r "../../out/lib/Net40/MathNet.Numerics.FSharp.dll"

open System.Numerics
open MathNet.Numerics
open MathNet.Numerics.Distributions
open MathNet.Numerics.Random
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic

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
    let dist = Normal(1.0, 10.0) |> withRandom (Random.mersenneTwister ())
    DenseVector.randomCreate N dist

/// A stopwatch to time the execution.
let sw = System.Diagnostics.Stopwatch()


printfn "%d-dimensional vector for %d iterations:" N T

for (name, f) in FunctionList do
    
    let v = w.Clone()

    sw.Restart()
    for t in 1 .. T do Vector.mapInPlace f v
    sw.Stop()

    printfn "%s:\t\t%d ms" name sw.ElapsedMilliseconds
