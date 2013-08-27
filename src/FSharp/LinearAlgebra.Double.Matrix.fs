// <copyright file="LinearAlgebra.Double.Matrix.fs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Double

open MathNet.Numerics.LinearAlgebra

/// A module which implements functional matrix operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Matrix =

    /// Returns the sum of all elements of a matrix.
    let inline sum (A: #Matrix<_>) =
        let mutable f = 0.0
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                f <- f + A.At(i,j)
        f

    /// Fold all columns into one row vector.
    let inline foldByCol (f: float -> float -> float) acc (A: #Matrix<float>) =
        let v = new DenseVector(A.ColumnCount)
        for k=0 to A.ColumnCount-1 do
            let mutable macc = acc
            for i=0 to A.RowCount-1 do
                macc <- f macc (A.At(i,k))
            v.At(k, macc)
        v :> _ Vector

    /// Fold all rows into one column vector.
    let inline foldByRow (f: float -> float -> float) acc (A: #Matrix<float>) =
        let v = new DenseVector(A.RowCount)
        for k=0 to A.RowCount-1 do
            let mutable macc = acc
            for i=0 to A.ColumnCount-1 do
                macc <- f macc (A.At(k,i))
            v.At(k, macc)
        v :> _ Vector

/// A module which implements functional dense vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DenseMatrix =

    /// Create a matrix that directly binds to a raw storage array in column-major (column by column) format, without copying.
    let inline raw (rows: int) (cols: int) (columnMajor: float[]) = DenseMatrix(rows, cols, columnMajor) :> _ Matrix

    /// Create an all-zero matrix with the given dimension.
    let inline zeroCreate (rows: int) (cols: int) = DenseMatrix(rows, cols) :> _ Matrix

    /// Create a random matrix with the given dimension and value distribution.
    let inline randomCreate (rows: int) (cols: int) dist = DenseMatrix.CreateRandom(rows, cols, dist) :> _ Matrix

    /// Create a matrix with the given dimension and set all values to x.
    let inline create (rows: int) (cols: int) x = DenseMatrix.Create(rows, cols, fun i j -> x) :> _ Matrix

    /// Initialize a matrix by calling a construction function for every element.
    let inline init (rows: int) (cols: int) (f: int -> int -> float) = DenseMatrix.Create(rows, cols, fun i j -> f i j) :> _ Matrix

    /// Create a matrix from a 2D array of floating point numbers.
    let inline ofArray2 array = DenseMatrix.OfArray(array) :> _ Matrix

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    /// If the dimensions are known, consider to use ofRowSeq instead to avoid multiple enumeration.
    let inline ofSeq (fss: #seq<#seq<float>>) =
        let n = Seq.length fss
        let m = Seq.length (Seq.head fss)
        DenseMatrix.OfRowsCovariant(n, m, fss) :> _ Matrix

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    /// If the dimensions are known, consider to use ofRowList instead to avoid multiple enumeration.
    let inline ofList (fll: float list list) =
        let n = List.length fll
        let m = List.length (List.head fll)
        DenseMatrix.OfRowsCovariant(n, m, fll) :> _ Matrix

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofRows (rows: int) (cols: int) (fss: #seq<#seq<float>>) = DenseMatrix.OfRowsCovariant(rows, cols, fss) :> _ Matrix

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofRowsList (rows: int) (cols: int) (fll: float list list) = DenseMatrix.OfRowsCovariant(rows, cols, fll) :> _ Matrix

    /// Create a matrix from a list of row vectors.
    let inline ofRowVectors (vectors: #Vector<float> list) = DenseMatrix.OfRowVectors(vectors |> Array.ofList |> box |> unbox) :> _ Matrix

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a column.
    let inline ofColumns (rows: int) (cols: int) (fss: #seq<#seq<float>>) = DenseMatrix.OfColumnsCovariant(rows, cols, fss) :> _ Matrix

    /// Create a matrix from a list of float lists. Every list in the master list specifies a column.
    let inline ofColumnsList (rows: int) (cols: int) (fll: float list list) = DenseMatrix.OfColumnsCovariant(rows, cols, fll) :> _ Matrix

    /// Create a matrix from a list of column vectors.
    let inline ofColumnVectors (vectors: #Vector<float> list) = DenseMatrix.OfColumnVectors(vectors |> Array.ofList |> box |> unbox) :> _ Matrix

    /// Create a matrix with a given dimension from an indexed sequences of row, column, value tuples.
    let inline ofSeqi (rows: int) (cols: int) (fs: #seq<int * int * float>) = DenseMatrix.OfIndexed(rows, cols, fs) :> _ Matrix

    /// Create a matrix with a given dimension from an indexed list of row, column, value tuples.
    let inline ofListi (rows: int) (cols: int) (fl: list<int * int * float>) = DenseMatrix.OfIndexed(rows, cols, Seq.ofList fl) :> _ Matrix

    /// Create a square matrix with constant diagonal entries.
    let inline constDiag (n: int) (f: float) =
        let A = DenseMatrix(n,n)
        for i=0 to n-1 do
            A.At(i,i,f)
        A :> _ Matrix

    /// Create a square matrix with the vector elements on the diagonal.
    let inline diag (v: #Vector<float>) =
        let n = v.Count
        let A = DenseMatrix(n,n)
        A.SetDiagonal(v)
        A :> _ Matrix

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRow (rows: int) (cols: int) (f: int -> #Vector<float>) =
        let A = DenseMatrix(rows,cols)
        for i=0 to rows-1 do A.SetRow(i, f i)
        A :> _ Matrix

    /// Initialize a matrix by calling a construction function for every column.
    let inline initCol (rows: int) (cols: int) (f: int -> #Vector<float>) =
        let A = DenseMatrix(rows,cols)
        for i=0 to cols-1 do A.SetColumn(i, f i)
        A :> _ Matrix

/// A module which implements functional sparse vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SparseMatrix =

    /// Create an all-zero matrix with the given dimension.
    let inline zeroCreate (rows: int) (cols: int) = SparseMatrix(rows, cols) :> _ Matrix

    /// Initialize a matrix by calling a construction function for every element.
    let inline init (rows: int) (cols: int) (f: int -> int -> float) = SparseMatrix.Create(rows, cols, fun n m -> f n m) :> _ Matrix

    /// Create a matrix from a 2D array of floating point numbers.
    let inline ofArray2 array = SparseMatrix.OfArray(array) :> _ Matrix

    /// Create a matrix with a given dimension from an indexed sequences of row, column, value tuples.
    [<System.ObsoleteAttribute("Use ofSeqi instead. Will be changed to expect a non-indexed seq in a future version.")>]
    let inline ofSeq (rows: int) (cols: int) (fs: #seq<int * int * float>) = SparseMatrix.OfIndexed(rows, cols, fs) :> _ Matrix

    /// Create a matrix with a given dimension from an indexed list of row, column, value tuples.
    [<System.ObsoleteAttribute("Use ofListi instead. Will be changed to expect a non-indexed list in a future version.")>]
    let inline ofList (rows: int) (cols: int) (fl: list<int * int * float>) = SparseMatrix.OfIndexed(rows, cols, Seq.ofList fl) :> _ Matrix

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofRows (rows: int) (cols: int) (fss: #seq<#seq<float>>) = SparseMatrix.OfRowsCovariant(rows, cols, fss) :> _ Matrix

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofRowsList (rows: int) (cols: int) (fll: float list list) = SparseMatrix.OfRowsCovariant(rows, cols, fll) :> _ Matrix

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a column.
    let inline ofColumns (rows: int) (cols: int) (fss: #seq<#seq<float>>) = SparseMatrix.OfColumnsCovariant(rows, cols, fss) :> _ Matrix

    /// Create a matrix from a list of float lists. Every list in the master list specifies a column.
    let inline ofColumnsList (rows: int) (cols: int) (fll: float list list) = SparseMatrix.OfColumnsCovariant(rows, cols, fll) :> _ Matrix

    /// Create a matrix with a given dimension from an indexed sequences of row, column, value tuples.
    let inline ofSeqi (rows: int) (cols: int) (fs: #seq<int * int * float>) = SparseMatrix.OfIndexed(rows, cols, fs) :> _ Matrix

    /// Create a matrix with a given dimension from an indexed list of row, column, value tuples.
    let inline ofListi (rows: int) (cols: int) (fl: list<int * int * float>) = SparseMatrix.OfIndexed(rows, cols, Seq.ofList fl) :> _ Matrix

    /// Create a square matrix with constant diagonal entries.
    let inline constDiag (n: int) (f: float) =
        let A = SparseMatrix(n,n)
        for i=0 to n-1 do
            A.At(i,i,f)
        A :> _ Matrix

    /// Create a square matrix with the vector elements on the diagonal.
    let inline diag (v: #Vector<float>) =
        let n = v.Count
        let A = SparseMatrix(n,n)
        A.SetDiagonal(v)
        A :> _ Matrix

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRow (rows: int) (cols: int) (f: int -> #Vector<float>) =
        let A = SparseMatrix(rows,cols)
        for i=0 to rows-1 do A.SetRow(i, f i)
        A :> _ Matrix

    /// Initialize a matrix by calling a construction function for every column.
    let inline initCol (rows: int) (cols: int) (f: int -> #Vector<float>) =
        let A = SparseMatrix(rows,cols)
        for i=0 to cols-1 do A.SetColumn(i, f i)
        A :> _ Matrix
