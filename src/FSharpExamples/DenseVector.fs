// <copyright file="DenseVector.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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
module MathNet.Numerics.FSharp.Examples.DenseVector

open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double

// Create a new 100 dimensional dense vector.
let v = DenseVector.init 100 (fun i -> float i / 100.0)

// Another way to create a 100 dimensional dense vector is using the vector function.
let w = vector (List.init 100 (fun i -> float i ** 2.0))

// Vectors can also be constructed from sequences.
let t = DenseVector.ofSeq (seq { for i in 1 .. 100 do yield float i })

// We can now add two vectors together ...
let z = v + w

// ... or scale them in the process.
let x = v + 3.0 * t


// We can create a vector from an integer range (in this case, 5 and 10 inclusive) ...
let s = DenseVector.range 5 10

// ... or we can create a vector from a double range with a particular step size.
let r = DenseVector.rangef 0.0 0.1 10.0