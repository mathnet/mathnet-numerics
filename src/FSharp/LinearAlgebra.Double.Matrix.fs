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

open MathNet.Numerics.LinearAlgebra.Generic

/// A module which implements functional matrix operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Matrix =

    /// Transform a vector into a 2D array.
    let inline toArray2 (A: #Matrix<float>) = A.ToArray()

    /// In-place map of every matrix element using a function.
    let inline mapInPlace (f: float -> float) (A: #Matrix<float>) =
        A.MapInplace((fun x -> f x), true)

    /// In-place map of every matrix element using a position dependent function.
    let inline mapiInPlace (f: int -> int -> float -> float) (A: #Matrix<float>) =
        A.MapIndexedInplace((fun i j x -> f i j x), true)

    /// In-place map of every matrix element using a function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapnzInPlace (f: float -> float) (A: #Matrix<float>) =
        A.MapInplace((fun x -> f x), false)

    /// In-place map of every matrix element using a position dependent function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapinzInPlace (f: int -> int -> float -> float) (A: #Matrix<float>) =
        A.MapIndexedInplace((fun i j x -> f i j x), false)

    /// In-place map every matrix column using the given position dependent function.
    let inline mapColsInPlace (f: int -> Vector<float> -> Vector<float>) (A: #Matrix<float>) =
        for j = 0 to A.ColumnCount-1 do
            A.SetColumn(j, f j (A.Column(j)))

    /// In-place map every matrix row using the given position dependent function.
    let inline mapRowsInPlace (f: int -> Vector<float> -> Vector<float>) (A: #Matrix<float>) =
        for i = 0 to A.RowCount-1 do
            A.SetRow(i, f i (A.Row(i)))

    [<System.ObsoleteAttribute("Use mapiInPlace instead. Scheduled for removal in v3.0.")>]
    let inplaceMapi = mapiInPlace
    [<System.ObsoleteAttribute("Use mapColsInPlace instead. Scheduled for removal in v3.0.")>]
    let inplaceMapCols = mapColsInPlace
    [<System.ObsoleteAttribute("Use mapRowsInPlace instead. Scheduled for removal in v3.0.")>]
    let inplaceMapRows = mapRowsInPlace

    /// Map every matrix element using the given function.
    let inline map (f: float -> float) (A: #Matrix<float>) =
        let A = A.Clone()
        A.MapInplace((fun x -> f x), true)
        A

    /// Map every matrix element using the given function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapnz (f: float -> float) (A: #Matrix<float>) =
        let A = A.Clone()
        A.MapInplace((fun x -> f x), false)
        A

    /// Map every matrix element using the given position dependent function.
    let inline mapi (f: int -> int -> float -> float) (A: #Matrix<float>) =
        let A = A.Clone()
        A.MapIndexedInplace((fun i j x -> f i j x), true)
        A

    /// Map every matrix element using the given position dependent function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapinz (f: int -> int -> float -> float) (A: #Matrix<float>) =
        let A = A.Clone()
        A.MapIndexedInplace((fun i j x -> f i j x), false)
        A

    /// Map every matrix column using the given position dependent function.
    let inline mapCols (f: int -> Vector<float> -> Vector<float>) (A: #Matrix<float>) =
        let A = A.Clone()
        mapColsInPlace f A
        A

    /// Map every matrix row using the given position dependent function.
    let inline mapRows (f: int -> Vector<float> -> Vector<float>) (A: #Matrix<float>) =
        let A = A.Clone()
        mapRowsInPlace f A
        A

    /// Fold a function over all matrix elements.
    let inline fold (f: 'a -> float -> 'a) (acc0: 'a) (A: #Matrix<float>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i=0 to n-1 do
            for j=0 to m-1 do
                acc <- f acc (A.At(i,j))
        acc

    /// Fold a function over all matrix elements in reverse order.
    let inline foldBack (f: float -> 'a -> 'a) (acc0: 'a) (A: #Matrix<float>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i in n-1 .. -1 .. 0 do
            for j in m-1 .. -1 .. 0 do
                acc <- f (A.At(i,j)) acc
        acc

    /// Fold a matrix by applying a given function to all matrix elements.
    let inline foldi (f: int -> int -> 'a -> float -> 'a) (acc0: 'a) (A: #Matrix<float>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i=0 to n-1 do
            for j=0 to m-1 do
                acc <- f i j acc (A.At(i,j))
        acc

    /// Checks whether a predicate holds for all elements of a matrix.
    let inline forall (p: float -> bool) (A: #Matrix<float>) =
        let mutable b = true
        let mutable i = 0
        let mutable j = 0
        while b && i < A.RowCount do
            b <- b && (p (A.At(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Chechks whether a predicate holds for at least one element of a matrix.
    let inline exists (p: float -> bool) (A: #Matrix<float>) =
        let mutable b = false
        let mutable i = 0
        let mutable j = 0
        while not(b) && i < A.RowCount do
            b <- b || (p (A.At(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Checks whether a position dependent predicate holds for all elements of a matrix.
    let inline foralli (p: int -> int -> float -> bool) (A: #Matrix<float>) =
        let mutable b = true
        let mutable i = 0
        let mutable j = 0
        while b && i < A.RowCount do
            b <- b && (p i j (A.At(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Checks whether a position dependent predicate holds for at least one element of a matrix.
    let inline existsi (p: int -> int -> float -> bool) (A: #Matrix<float>) =
        let mutable b = false
        let mutable i = 0
        let mutable j = 0
        while not(b) && i < A.RowCount do
            b <- b || (p i j (A.At(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// In-place assignment.
    let inline inplaceAssign (f: int -> int -> float) (A: #Matrix<float>) =
        A.MapIndexedInplace((fun i j x -> f i j), true)

    /// Creates a sequence that iterates the non-zero entries in the matrix.
    let inline nonZeroEntries (A: #Matrix<float>) =
        seq { for i in 0 .. A.RowCount-1 do
                for j in 0 .. A.ColumnCount-1 do
                  if A.At(i,j) <> 0.0 then yield (i, j, A.At(i,j)) }

    /// Returns the sum of all elements of a matrix.
    let inline sum (A: #Matrix<float>) =
        let mutable f = 0.0
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                f <- f + A.At(i,j)
        f

    /// Returns the sum of the results generated by applying a position dependent function to each column of the matrix.
    let inline sumColsBy (f: int -> Vector<float> -> 'a) (A: #Matrix<float>) =
        A.ColumnEnumerator() |> Seq.map (fun (j,col) -> f j col) |> Seq.reduce (+)

    /// Returns the sum of the results generated by applying a position dependent function to each row of the matrix.
    let inline sumRowsBy (f: int -> Vector<float> -> 'a) (A: #Matrix<float>) =
        A.RowEnumerator() |> Seq.map (fun (i,row) -> f i row)  |> Seq.reduce (+)

    /// Iterates over all elements of a matrix.
    let inline iter (f: float -> unit) (A: #Matrix<float>) =
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                f (A.At(i,j))

    /// Iterates over all elements of a matrix using the element indices.
    let inline iteri (f: int -> int -> float -> unit) (A: #Matrix<float>) =
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                f i j (A.At(i,j))

    /// Fold one column.
    let inline foldCol (f: 'a -> float -> 'a) acc (A: #Matrix<float>) k =
        let mutable macc = acc
        for i=0 to A.RowCount-1 do
            macc <- f macc (A.Item(i,k))
        macc

    /// Fold one row.
    let inline foldRow (f: 'a -> float -> 'a) acc (A: #Matrix<float>) k =
        let mutable macc = acc
        for i=0 to A.ColumnCount-1 do
            macc <- f macc (A.Item(k,i))
        macc

    /// Fold all columns into one row vector.
    let inline foldByCol (f: float -> float -> float) acc (A: #Matrix<float>) =
        let v = new DenseVector(A.ColumnCount)
        for k=0 to A.ColumnCount-1 do
            let mutable macc = acc
            for i=0 to A.RowCount-1 do
                macc <- f macc (A.At(i,k))
            v.At(k, macc)
        v :> Vector<float>

    /// Fold all rows into one column vector.
    let inline foldByRow (f: float -> float -> float) acc (A: #Matrix<float>) =
        let v = new DenseVector(A.RowCount)
        for k=0 to A.RowCount-1 do
            let mutable macc = acc
            for i=0 to A.ColumnCount-1 do
                macc <- f macc (A.At(k,i))
            v.At(k, macc)
        v :> Vector<float>

/// A module which implements functional dense vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DenseMatrix =

    /// Create a matrix that directly binds to a raw storage array in column-major (column by column) format, without copying.
    let inline raw (rows: int) (cols: int) (columnMajor: float[]) = DenseMatrix(rows, cols, columnMajor)

    /// Create an all-zero matrix with the given dimension.
    let inline zeroCreate (rows: int) (cols: int) = DenseMatrix(rows, cols)

    /// Create a random matrix with the given dimension and value distribution.
    let inline randomCreate (rows: int) (cols: int) dist = DenseMatrix.CreateRandom(rows, cols, dist)

    /// Create a matrix with the given dimension and set all values to x.
    let inline create (rows: int) (cols: int) x = DenseMatrix.Create(rows, cols, fun i j -> x)

    /// Initialize a matrix by calling a construction function for every element.
    let inline init (rows: int) (cols: int) (f: int -> int -> float) = DenseMatrix.Create(rows, cols, fun i j -> f i j)

    /// Create a matrix from a 2D array of floating point numbers.
    let inline ofArray2 array = DenseMatrix.OfArray(array)

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    /// If the dimensions are known, consider to use ofRowSeq instead to avoid multiple enumeration.
    let inline ofSeq (fss: #seq<#seq<float>>) =
        let n = Seq.length fss
        let m = Seq.length (Seq.head fss)
        DenseMatrix.OfRowsCovariant(n, m, fss)

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    /// If the dimensions are known, consider to use ofRowList instead to avoid multiple enumeration.
    let inline ofList (fll: float list list) =
        let n = List.length fll
        let m = List.length (List.head fll)
        DenseMatrix.OfRowsCovariant(n, m, fll)

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofRows (rows: int) (cols: int) (fss: #seq<#seq<float>>) = DenseMatrix.OfRowsCovariant(rows, cols, fss)

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofRowsList (rows: int) (cols: int) (fll: float list list) = DenseMatrix.OfRowsCovariant(rows, cols, fll)

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a column.
    let inline ofColumns (rows: int) (cols: int) (fss: #seq<#seq<float>>) = DenseMatrix.OfColumnsCovariant(rows, cols, fss)

    /// Create a matrix from a list of float lists. Every list in the master list specifies a column.
    let inline ofColumnsList (rows: int) (cols: int) (fll: float list list) = DenseMatrix.OfColumnsCovariant(rows, cols, fll)

    /// Create a matrix with a given dimension from an indexed sequences of row, column, value tuples.
    let inline ofSeqi (rows: int) (cols: int) (fs: #seq<int * int * float>) = DenseMatrix.OfIndexed(rows, cols, fs)

    /// Create a matrix with a given dimension from an indexed list of row, column, value tuples.
    let inline ofListi (rows: int) (cols: int) (fl: list<int * int * float>) = DenseMatrix.OfIndexed(rows, cols, Seq.ofList fl)

    /// Create a matrix with the given entries.
    [<System.ObsoleteAttribute("Use ofSeqi instead. Scheduled for removal in v3.0.")>]
    let inline initDense (rows: int) (cols: int) (es: #seq<int * int * float>) = ofSeqi rows cols es

    /// Create a square matrix with constant diagonal entries.
    let inline constDiag (n: int) (f: float) =
        let A = new DenseMatrix(n,n)
        for i=0 to n-1 do
            A.At(i,i,f)
        A

    /// Create a square matrix with the vector elements on the diagonal.
    let inline diag (v: #Vector<float>) =
        let n = v.Count
        let A = new DenseMatrix(n,n)
        A.SetDiagonal(v)
        A

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRow (rows: int) (cols: int) (f: int -> #Vector<float>) =
        let A = new DenseMatrix(rows,cols)
        for i=0 to rows-1 do A.SetRow(i, f i)
        A

    /// Initialize a matrix by calling a construction function for every column.
    let inline initCol (rows: int) (cols: int) (f: int -> #Vector<float>) =
        let A = new DenseMatrix(rows,cols)
        for i=0 to cols-1 do A.SetColumn(i, f i)
        A

/// A module which implements functional sparse vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SparseMatrix =

    /// Create an all-zero matrix with the given dimension.
    let inline zeroCreate (rows: int) (cols: int) = SparseMatrix(rows, cols)

    /// Initialize a matrix by calling a construction function for every element.
    let inline init (rows: int) (cols: int) (f: int -> int -> float) = SparseMatrix.Create(rows, cols, fun n m -> f n m)

    /// Create a matrix from a 2D array of floating point numbers.
    let inline ofArray2 array = SparseMatrix.OfArray(array)

    /// Create a matrix with a given dimension from an indexed sequences of row, column, value tuples.
    [<System.ObsoleteAttribute("Use ofSeqi instead. Will be changed to expect a non-indexed seq in a future version.")>]
    let inline ofSeq (rows: int) (cols: int) (fs: #seq<int * int * float>) = SparseMatrix.OfIndexed(rows, cols, fs)

    /// Create a matrix with a given dimension from an indexed list of row, column, value tuples.
    [<System.ObsoleteAttribute("Use ofListi instead. Will be changed to expect a non-indexed list in a future version.")>]
    let inline ofList (rows: int) (cols: int) (fl: list<int * int * float>) = SparseMatrix.OfIndexed(rows, cols, Seq.ofList fl)

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofRows (rows: int) (cols: int) (fss: #seq<#seq<float>>) = SparseMatrix.OfRowsCovariant(rows, cols, fss)

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofRowsList (rows: int) (cols: int) (fll: float list list) = SparseMatrix.OfRowsCovariant(rows, cols, fll)

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a column.
    let inline ofColumns (rows: int) (cols: int) (fss: #seq<#seq<float>>) = SparseMatrix.OfColumnsCovariant(rows, cols, fss)

    /// Create a matrix from a list of float lists. Every list in the master list specifies a column.
    let inline ofColumnsList (rows: int) (cols: int) (fll: float list list) = SparseMatrix.OfColumnsCovariant(rows, cols, fll)

    /// Create a matrix with a given dimension from an indexed sequences of row, column, value tuples.
    let inline ofSeqi (rows: int) (cols: int) (fs: #seq<int * int * float>) = SparseMatrix.OfIndexed(rows, cols, fs)

    /// Create a matrix with a given dimension from an indexed list of row, column, value tuples.
    let inline ofListi (rows: int) (cols: int) (fl: list<int * int * float>) = SparseMatrix.OfIndexed(rows, cols, Seq.ofList fl)

    /// Create a square matrix with constant diagonal entries.
    let inline constDiag (n: int) (f: float) =
        let A = new SparseMatrix(n,n)
        for i=0 to n-1 do
            A.At(i,i,f)
        A

    /// Create a square matrix with the vector elements on the diagonal.
    let inline diag (v: #Vector<float>) =
        let n = v.Count
        let A = new SparseMatrix(n,n)
        A.SetDiagonal(v)
        A

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRow (rows: int) (cols: int) (f: int -> #Vector<float>) =
        let A = new SparseMatrix(rows,cols)
        for i=0 to rows-1 do A.SetRow(i, f i)
        A

    /// Initialize a matrix by calling a construction function for every column.
    let inline initCol (rows: int) (cols: int) (f: int -> #Vector<float>) =
        let A = new SparseMatrix(rows,cols)
        for i=0 to cols-1 do A.SetColumn(i, f i)
        A
