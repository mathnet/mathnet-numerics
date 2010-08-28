// <copyright file="SparseMatrix.fs" company="Math.NET">
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
open MathNet.Numerics.LinearAlgebra.Generic
/// A module which implements functional sparse vector operations.
module SparseMatrix =
    
    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofList (rows: int) (cols: int) (fll: list<int * int * float>) =
        let A = new Double.SparseMatrix(rows, cols)
        fll |> List.iter (fun (i, j, x) -> A.[i,j] <- x)
        A
    
    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofSeq (rows: int) (cols: int) (fss: #seq<int * int * float>) =
        let A = new Double.SparseMatrix(rows, cols)
        fss |> Seq.iter (fun (i, j, x) -> A.[i,j] <- x)
        A
    
    /// Create a square matrix with constant diagonal entries.
    let inline constDiag (n: int) (f: float) =
        let A = new Double.SparseMatrix(n,n)
        for i=0 to n-1 do
            A.[i,i] <- f
        A
    
    /// Create a square matrix with the vector elements on the diagonal.
    let inline diag (v: #Vector<float>) =
        let n = v.Count
        let A = new Double.SparseMatrix(n,n)
        for i=0 to n-1 do
            A.[i,i] <- v.Item(i)
        A

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRow (n: int) (m: int) (f: int -> #Vector<float>) =
        let A = new Double.SparseMatrix(n,m)
        for i=0 to n-1 do A.SetRow(i, f i)
        A

    /// Initialize a matrix by calling a construction function for every column.
    let inline initCol (n: int) (m: int) (f: int -> #Vector<float>) =
        let A = new Double.SparseMatrix(n,m)
        for i=0 to n-1 do A.SetColumn(i, f i)
        A