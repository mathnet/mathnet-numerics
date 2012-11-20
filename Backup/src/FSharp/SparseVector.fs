// <copyright file="SparseVector.fs" company="Math.NET">
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

/// A module which implements functional sparse vector operations.
module SparseVector =
    
    /// Create a sparse vector with a given dimension from a list of entry, value pairs.
    let inline ofList (dim: int) (fl: list<int * float>) =
        let v = new Double.SparseVector(dim)
        fl |> List.iter (fun (i, f) -> v.[i] <- f)
        v
    
    /// Create a sparse vector with a given dimension from a sequence of entry, value pairs.
    let inline ofSeq (dim: int) (fs: #seq<int * float>) =
        let v = new Double.SparseVector(dim)
        fs |> Seq.iter (fun (i, f) -> v.[i] <- f)
        v