// <copyright file="DenseVector.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

namespace MathNet.Numerics.LinearAlgebra.Double

open MathNet.Numerics.LinearAlgebra

/// A module which implements functional dense vector operations.
module DenseVector =

    /// Initialize a vector by calling a construction function for every element.
    let inline init (n: int) f =
        let v = new Double.DenseVector(n)
        for i=0 to n-1 do
            v.[i] <- f i
        v

    /// Create a vector from a float list.
    let inline ofList (fl: float list) =
        let n = List.length fl
        let v = Double.DenseVector(n)
        fl |> List.iteri (fun i f -> v.[i] <- f)
        v
    
    /// Create a vector from a sequences.
    let inline ofSeq (fs: #seq<float>) =
        let n = Seq.length fs
        let v = DenseVector(n)
        fs |> Seq.iteri (fun i f -> v.[i] <- f)
        v
    
    /// Create a vector with evenly spaced entries: e.g. rangef -1.0 0.5 1.0 = [-1.0 -0.5 0.0 0.5 1.0]
    let inline rangef (start: float) (step: float) (stop: float) =
        let n = (int ((stop - start) / step)) + 1
        let v = new DenseVector(n)
        for i=0 to n-1 do
            v.[i] <- (float i) * step + start
        v
    
    /// Create a vector with integer entries in the given range.
    let inline range (start: int) (stop: int) =
        new DenseVector([| for i in [start .. stop] -> float i |])