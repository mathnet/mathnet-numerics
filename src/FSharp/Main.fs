// <copyright file="Main.fs" company="Math.NET">
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

namespace MathNet.Numerics

open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic
/// A module which implements some F# utility functions.
module FSharp =

    /// Construct a dense matrix from a list of floating point numbers.
    let inline matrix (lst: list<list<float>>) = DenseMatrix.ofList lst :> Matrix<float>

    /// Construct a dense vector from a list of floating point numbers.
    let inline vector (lst: list<float>) = DenseVector.ofList lst :> Vector<float>

    type DenseVector with

        /// Supports the slicing syntax 'v.[idx1..idx2]'
        member v.GetSlice(start, finish) = 
            let start = match start with None -> 0 | Some i -> i
            let finish = match finish with None -> v.Count - 1 | Some i -> i
            v.SubVector(start, finish - start + 1)

        /// Supports the slicing syntax 'v.[idx1..idx2] <- v2'
        member v.SetSlice(start, finish, vs:Vector<_>) = 
            let start = match start with None -> 0 | Some i -> i 
            let finish = match finish with None -> v.Count - 1 | Some i -> i
            assert (vs.Count = finish - start + 1)
            for i = start to finish do 
                v.[i] <- vs.[i-start]

    type DenseMatrix with

        /// Supports the slicing syntax 'A.[idx1..idx2,idx1..idx2]'
        member m.GetSlice(start1, finish1, start2, finish2) = 
            let start1 = match start1 with None -> 0 | Some v -> v 
            let finish1 = match finish1 with None -> m.RowCount - 1 | Some v -> v 
            let start2 = match start2 with None -> 0 | Some v -> v 
            let finish2 = match finish2 with None -> m.ColumnCount - 1 | Some v -> v 
            m.SubMatrix(start1, finish1 - start1 + 1, start2, finish2 - start2 + 1)

        /// Supports the slicing syntax 'A.[idx1..idx2,idx1..idx2] <- B'
        member m.SetSlice (start1, finish1, start2, finish2, vs:Matrix<_>) = 
            let start1 = match start1 with None -> 0 | Some i -> i 
            let finish1 = match finish1 with None -> m.RowCount - 1 | Some i -> i 
            let start2 = match start2 with None -> 0 | Some i -> i 
            let finish2 = match finish2 with None -> m.ColumnCount - 1 | Some i -> i 
            m.SetSubMatrix(start1, finish1 - start1 + 1, start2, finish2 - start2 + 1, vs)