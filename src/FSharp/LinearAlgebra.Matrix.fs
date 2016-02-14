// <copyright file="LinearAlgebra.Matrix.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra

open System
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra


/// A module which implements functional matrix operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Matrix =

    /// Transform a matrix into a 2D array.
    let inline toArray2 (A: #Matrix<_>) = A.ToArray()

    /// Transform a matrix into an array of column arrays.
    let inline toColArrays (m: #Matrix<_>) = m.ToColumnArrays()

    /// Transform a matrix into an array of row arrays.
    let inline toRowArrays (m: #Matrix<_>) = m.ToRowArrays()


    /// Transform a matrix into a sequence.
    let inline toSeq (m: #Matrix<_>) = m.Enumerate(Zeros.Include)

    /// Transform a matrix into an indexed sequence.
    let inline toSeqi (m: #Matrix<_>) = m.EnumerateIndexed(Zeros.Include) |> properTuple3Seq

    /// Transform a matrix into a sequence where zero-values are skipped. Skipping zeros is efficient on sparse data.
    let inline toSeqSkipZeros (m: #Matrix<_>) = m.Enumerate(Zeros.AllowSkip)

    /// Transform a matrix into an indexed sequence where zero-values are skipped. Skipping zeros is efficient on sparse data.
    let inline toSeqiSkipZeros (m: #Matrix<_>) = m.EnumerateIndexed(Zeros.AllowSkip) |> properTuple3Seq

    /// Transform a matrix into a column sequence.
    let inline toColSeq (m: #Matrix<_>) = m.EnumerateColumns()

    /// Transform a matrix into an indexed column sequence.
    let inline toColSeqi (m: #Matrix<_>) = m.EnumerateColumnsIndexed() |> properTuple2Seq

    /// Transform a matrix into a row sequence.
    let inline toRowSeq (m: #Matrix<_>) = m.EnumerateRows()

    /// Transform a matrix into an indexed row sequence.
    let inline toRowSeqi (m: #Matrix<_>) = m.EnumerateRowsIndexed() |> properTuple2Seq


    /// Applies a function to all elements of the matrix.
    let inline iter f (m: #Matrix<_>) = m |> toSeq |> Seq.iter f

    /// Applies a function to all indexed elements of the matrix.
    let inline iteri f (m: #Matrix<_>) = m |> toSeqi |> Seq.iter (fun (i, j, x) -> f i j x)

    /// Applies a function to all non-zero elements of the matrix. Skipping zeros is efficient on sparse data.
    let inline iterSkipZerosnz f (m: #Matrix<_>) = m |> toSeqSkipZeros |> Seq.iter f

    /// Applies a function to all non-zero indexed elements of the matrix. Skipping zeros is efficient on sparse data.
    let inline iteriSkipZeros f (m: #Matrix<_>) = m |> toSeqiSkipZeros |> Seq.iter (fun (i, j, x) -> f i j x)

    /// Applies a function to all columns of the matrix.
    let inline iterCols f (m: #Matrix<_>) = m |> toColSeq |> Seq.iter f

    /// Applies a function to all indexed columns of the matrix.
    let inline iteriCols f (m: #Matrix<_>) = m |> toColSeq |> Seq.iteri f

    /// Applies a function to all rows of the matrix.
    let inline iterRows f (m: #Matrix<_>) = m |> toRowSeq |> Seq.iter f

    /// Applies a function to all indexed rows of the matrix.
    let inline iteriRows f (m: #Matrix<_>) = m |> toRowSeq |> Seq.iteri f


    /// Fold all entries of a matrix.
    let inline fold f state (m: #Matrix<_>) = m |> toSeq |> Seq.fold f state

    /// Fold all entries of a matrix with an indexed folding function.
    let inline foldi f state (m: #Matrix<_>) = m |> toSeqi |> Seq.fold (fun s (i,j,x) -> f i j s x) state

    /// Fold all non-zero entries of a matrix. Skipping zeros is efficient on sparse data.
    let inline foldSkipZeros f state (m: #Matrix<_>) = m |> toSeqSkipZeros |> Seq.fold f state

    /// Fold all non-zero entries of a matrix with an indexed folding function. Skipping zeros is efficient on sparse data.
    let inline foldiSkipZeros f state (m: #Matrix<_>) = m |> toSeqiSkipZeros |> Seq.fold (fun s (i,j,x) -> f i j s x) state

    /// Fold all columns of a matrix.
    let inline foldCols f state (m: #Matrix<_>) = m |> toColSeq |> Seq.fold f state

    /// Fold all columns of a matrix with an indexed folding function.
    let inline foldiCols f state (m: #Matrix<_>) = m |> toColSeqi |> Seq.fold (fun s (j,x) -> f j s x) state

    /// Fold all rows of a matrix.
    let inline foldRows f state (m: #Matrix<_>) = m |> toRowSeq |> Seq.fold f state

    /// Fold all rows of a matrix with an indexed folding function.
    let inline foldiRows f state (m: #Matrix<_>) = m |> toRowSeqi |> Seq.fold (fun s (i,x) -> f i s x) state


    /// Scan all entries of a matrix.
    let inline scan f state (m: #Matrix<_>) = m |> toSeq |> Seq.scan f state

    /// Scan all entries of a matrix with an indexed folding function.
    let inline scani f state (m: #Matrix<_>) = m |> toSeqi |> Seq.scan (fun s (i,j,x) -> f i j s x) state

    /// Scan all non-zero entries of a matrix. Skipping zeros is efficient on sparse data.
    let inline scanSkipZeros f state (m: #Matrix<_>) = m |> toSeqSkipZeros |> Seq.scan f state

    /// Scan all non-zero entries of a matrix with an indexed folding function. Skipping zeros is efficient on sparse data.
    let inline scaniSkipZeros f state (m: #Matrix<_>) = m |> toSeqiSkipZeros |> Seq.scan (fun s (i,j,x) -> f i j s x) state

    /// Scan all columns of a matrix.
    let inline scanCols f state (m: #Matrix<_>) = m |> toColSeq |> Seq.scan f state

    /// Scan all columns of a matrix with an indexed folding function.
    let inline scaniCols f state (m: #Matrix<_>) = m |> toColSeqi |> Seq.scan (fun s (j,x) -> f j s x) state

    /// Scan all rows of a matrix.
    let inline scanRows f state (m: #Matrix<_>) = m |> toRowSeq |> Seq.scan f state

    /// Scan all rows of a matrix with an indexed folding function.
    let inline scaniRows f state (m: #Matrix<_>) = m |> toRowSeqi |> Seq.scan (fun s (i,x) -> f i s x) state


    /// Reduce all entries of a matrix.
    let inline reduce f (m: #Matrix<_>) = m |> toSeq |> Seq.reduce f

    /// Reduce all non-zero entries of a matrix. Skipping zeros is efficient on sparse data.
    let inline reduceSkipZeros f (m: #Matrix<_>) = m |> toSeqSkipZeros |> Seq.reduce f

    /// Reduce all columns of a matrix.
    let inline reduceCols f (m: #Matrix<_>) = m |> toColSeq |> Seq.reduce f

    /// Reduce all rows of a matrix.
    let inline reduceRows f (m: #Matrix<_>) = m |> toColSeq |> Seq.reduce f


    /// Checks whether there is an entry in the matrix that satisfies a predicate.
    let inline exists p (m: #Matrix<_>) = m |> toSeq |> Seq.exists p

    /// Checks whether there is an entry in the matrix that satisfies a position dependent predicate.
    let inline existsi p (m: #Matrix<_>) = m |> toSeqi |> Seq.exists (fun (i,j,x) -> p i j x)

    /// Checks whether there is a non-zero entry in the matrix that satisfies a predicate. Skipping zeros is efficient on sparse data.
    let inline existsSkipZeros p (m: #Matrix<_>) = m |> toSeqSkipZeros |> Seq.exists p

    /// Checks whether there is a non-zero entry in the matrix that satisfies a position dependent predicate. Skipping zeros is efficient on sparse data.
    let inline existsiSkipZeros p (m: #Matrix<_>) = m |> toSeqiSkipZeros |> Seq.exists (fun (i,j,x) -> p i j x)

    /// Checks whether there is a column in the matrix that satisfies a predicate.
    let inline existsCol p (m: #Matrix<_>) = m |> toColSeq |> Seq.exists p

    /// Checks whether there is a column in the matrix that satisfies a position dependent predicate.
    let inline existsiCol p (m: #Matrix<_>) = m |> toColSeqi |> Seq.exists (fun (j,x) -> p j x)

    /// Checks whether there is a row in the matrix that satisfies a predicate.
    let inline existsRow p (m: #Matrix<_>) = m |> toRowSeq |> Seq.exists p

    /// Checks whether there is a row in the matrix that satisfies a position dependent predicate.
    let inline existsiRow p (m: #Matrix<_>) = m |> toRowSeqi |> Seq.exists (fun (i,x) -> p i x)


    /// Checks whether all entries in the matrix that satisfies a given predicate.
    let inline forall p (m: #Matrix<_>) = m |> toSeq |> Seq.forall p

    /// Checks whether all entries in the matrix that satisfies a given position dependent predicate.
    let inline foralli p (m: #Matrix<_>) = m |> toSeqi |> Seq.forall (fun (i,j,x) -> p i j x)

    /// Checks whether all non-zero entries in the matrix that satisfies a given predicate. Skipping zeros is efficient on sparse data.
    let inline forallSkipZeros p (m: #Matrix<_>) = m |> toSeqSkipZeros |> Seq.forall p

    /// Checks whether all non-zero entries in the matrix that satisfies a given position dependent predicate. Skipping zeros is efficient on sparse data.
    let inline foralliSkipZeros p (m: #Matrix<_>) = m |> toSeqiSkipZeros |> Seq.forall (fun (i,j,x) -> p i j x)

    /// Checks whether all columns in the matrix that satisfy a predicate.
    let inline forallCols p (m: #Matrix<_>) = m |> toColSeq |> Seq.forall p

    /// Checks whether all columns in the matrix that satisfy a position dependent predicate.
    let inline foralliCols p (m: #Matrix<_>) = m |> toColSeqi |> Seq.forall (fun (j,x) -> p j x)

    /// Checks whether all rows in the matrix that satisfy a predicate.
    let inline forallRows p (m: #Matrix<_>) = m |> toRowSeq |> Seq.forall p

    /// Checks whether all rows in the matrix that satisfy a position dependent predicate.
    let inline foralliRows p (m: #Matrix<_>) = m |> toRowSeqi |> Seq.forall (fun (i,x) -> p i x)



    /// In-place map of every matrix element using a function.
    let inline mapInPlace f (A: #Matrix<_>) = A.MapInplace((fun x -> f x), Zeros.Include)

    /// In-place map of every matrix element using a function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapSkipZerosInPlace f (A: #Matrix<_>) = A.MapInplace((fun x -> f x), Zeros.AllowSkip)

    /// In-place map of every matrix element using a position dependent function.
    let inline mapiInPlace f (A: #Matrix<_>) = A.MapIndexedInplace((fun i j x -> f i j x), Zeros.Include)

    /// In-place map of every matrix element using a position dependent function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapiSkipZerosInPlace f (A: #Matrix<_>) = A.MapIndexedInplace((fun i j x -> f i j x), Zeros.AllowSkip)

    /// In-place map every matrix column using the given position dependent function.
    let inline mapColsInPlace (f: int -> Vector<'a> -> Vector<'a>) (A: #Matrix<_>) =
        for j = 0 to A.ColumnCount-1 do
            A.SetColumn(j, f j (A.Column(j)))

    /// In-place map every matrix row using the given position dependent function.
    let inline mapRowsInPlace (f: int -> Vector<'a> -> Vector<'a>) (A: #Matrix<_>) =
        for i = 0 to A.RowCount-1 do
            A.SetRow(i, f i (A.Row(i)))


    /// Map every matrix element using the given function.
    let inline map f (A: #Matrix<_>) = A.Map((fun x -> f x), Zeros.Include)

    /// Map every matrix element using the given function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapSkipZeros f (A: #Matrix<_>) = A.Map((fun x -> f x), Zeros.AllowSkip)

    /// Map every matrix element using the given position dependent function.
    let inline mapi f (A: #Matrix<_>) = A.MapIndexed((fun i j x -> f i j x), Zeros.Include)

    /// Map every matrix element using the given position dependent function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapiSkipZeros f (A: #Matrix<_>) = A.MapIndexed((fun i j x -> f i j x), Zeros.AllowSkip)

    /// Map every matrix column using the given position dependent function.
    let inline mapCols (f: int -> Vector<'a> -> Vector<'a>) (A: #Matrix<_>) =
        let A = A.Clone()
        mapColsInPlace f A
        A

    /// Map every matrix row using the given position dependent function.
    let inline mapRows (f: int -> Vector<'a> -> Vector<'a>) (A: #Matrix<_>) =
        let A = A.Clone()
        mapRowsInPlace f A
        A



    /// Fold one column.
    let inline foldCol f state (A: #Matrix<_>) k =
        let mutable acc = state
        for i=0 to A.RowCount-1 do
            acc <- f acc (A.Item(i,k))
        acc

    /// Fold one row.
    let inline foldRow f state (A: #Matrix<_>) k =
        let mutable acc = state
        for i=0 to A.ColumnCount-1 do
            acc <- f acc (A.Item(k,i))
        acc

    /// Fold a function over all matrix elements in reverse order.
    let inline foldBack f state (A: #Matrix<_>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = state
        for i in n-1 .. -1 .. 0 do
            for j in m-1 .. -1 .. 0 do
                acc <- f (A.At(i,j)) acc
        acc

    /// In-place assignment.
    let inline inplaceAssign f (A: #Matrix<_>) =
        A.MapIndexedInplace((fun i j x -> f i j), Zeros.Include)

    /// Fold all columns into one row vector.
    let inline foldByCol f acc (A: #Matrix<'T>) =
        let v = Vector<'T>.Build.SameAs(A, A.ColumnCount)
        for k=0 to A.ColumnCount-1 do
            let mutable macc = acc
            for i=0 to A.RowCount-1 do
                macc <- f macc (A.At(i,k))
            v.At(k, macc)
        v :> _ Vector

    /// Fold all rows into one column vector.
    let inline foldByRow f acc (A: #Matrix<'T>) =
        let v = Vector<'T>.Build.SameAs(A, A.RowCount)
        for k=0 to A.RowCount-1 do
            let mutable macc = acc
            for i=0 to A.ColumnCount-1 do
                macc <- f macc (A.At(k,i))
            v.At(k, macc)
        v :> _ Vector


    let inline insertRow rowIndex (rowVector: #Vector<_>) (matrix: #Matrix<_>) = matrix.InsertRow(rowIndex, rowVector)
    let inline insertCol columnIndex (columnVector: #Vector<_>) (matrix: #Matrix<_>) = matrix.InsertColumn(columnIndex, columnVector)

    let inline appendRow (rowVector: #Vector<_>) (matrix: #Matrix<_>) = matrix.InsertRow(matrix.RowCount, rowVector)
    let inline appendCol (columnVector: #Vector<_>) (matrix: #Matrix<_>) = matrix.InsertColumn(matrix.ColumnCount, columnVector)

    let inline prependRow (rowVector: #Vector<_>) (matrix: #Matrix<_>) = matrix.InsertRow(0, rowVector)
    let inline prependCol (columnVector: #Vector<_>) (matrix: #Matrix<_>) = matrix.InsertColumn(0, columnVector)


    /// In-place matrix addition.
    let inline addInPlace (v: #Matrix<_>) (w: #Matrix<_>) = v.Add(w, v)

    /// In place matrix subtraction.
    let inline subInPlace (v: #Matrix<_>) (w: #Matrix<_>) = v.Subtract(w, v)


    /// Returns the sum of all elements of a matrix.
    let inline sum (A: #Matrix<'a>) = A |> foldSkipZeros (+) Matrix<'a>.Zero

    let inline sumRows (A: #Matrix<_>) = A.RowSums()
    let inline sumCols (A: #Matrix<_>) = A.ColumnSums()

    let inline sumAbsRows (A: #Matrix<_>) = A.RowAbsoluteSums()
    let inline sumAbsCols (A: #Matrix<_>) = A.ColumnAbsoluteSums()

    /// Returns the sum of the results generated by applying a position dependent function to each column of the matrix.
    let inline sumColsBy f (A: #Matrix<_>) =
        A |> toColSeqi |> Seq.map (fun (j,col) -> f j col) |> Seq.reduce (+)

    /// Returns the sum of the results generated by applying a position dependent function to each row of the matrix.
    let inline sumRowsBy f (A: #Matrix<_>) =
        A |> toRowSeqi |> Seq.map (fun (i,row) -> f i row)  |> Seq.reduce (+)


    let inline rowCount (A: #Matrix<_>) = A.RowCount
    let inline columnCount (A: #Matrix<_>) = A.ColumnCount

    let inline transpose (A: #Matrix<_>) = A.Transpose()
    let inline conjugate (A: #Matrix<_>) = A.Conjugate()
    let inline conjugateTranspose (A: #Matrix<_>) = A.ConjugateTranspose()
    let inline inverse (A: #Matrix<_>) = A.Inverse()

    let inline norm (A: #Matrix<_>) = A.L2Norm()
    let inline normRows (A: #Matrix<_>) = A.RowNorms 2.0
    let inline normCols (A: #Matrix<_>) = A.ColumnNorms 2.0

    let inline rank (A: #Matrix<_>) = A.Rank()
    let inline trace (A: #Matrix<_>) = A.Trace()
    let inline determinant (A: #Matrix<_>) = A.Determinant()
    let inline condition (A: #Matrix<_>) = A.ConditionNumber()
    let inline nullity (A: #Matrix<_>) = A.Nullity()
    let inline kernel (A: #Matrix<_>) = A.Kernel()
    let inline range (A: #Matrix<_>) = A.Range()
    let inline symmetric (A: #Matrix<_>) = A.IsSymmetric()
    let inline hermitian (A: #Matrix<_>) = A.IsHermitian()

    let inline cholesky (A: #Matrix<_>) = A.Cholesky()
    let inline lu (A: #Matrix<_>) = A.LU()
    let inline qr (A: #Matrix<_>) = A.QR()
    let inline svd (A: #Matrix<_>) = A.Svd()
    let inline eigen (A: #Matrix<_>) = A.Evd()


/// A module which helps constructing generic dense matrices.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DenseMatrix =

    open MathNet.Numerics.Distributions

    /// Create a matrix that directly binds to a storage object.
    let inline ofStorage storage = Matrix<'T>.Build.Dense(storage)

    /// Create a matrix that directly binds to a raw storage array in column-major (column by column) format, without copying.
    let inline raw (rows: int) (cols: int) (columnMajor: 'T[]) = Matrix<'T>.Build.Dense(rows, cols, columnMajor)

    /// Create an all-zero matrix with the given dimension.
    let inline zero<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (rows: int) (cols: int) = Matrix<'T>.Build.Dense(rows, cols)

    /// Create a random matrix with the given dimension and value distribution.
    let inline random<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (rows: int) (cols: int) (dist: IContinuousDistribution) = Matrix<'T>.Build.Random(rows, cols, dist)

    /// Create a random matrix with the given dimension and standard distributed values.
    let inline randomStandard<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
         (rows: int) (cols: int) = Matrix<'T>.Build.Random(rows, cols)

    /// Create a random matrix with the given dimension and standard distributed values using the provided seed.
    let inline randomSeed<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (rows: int) (cols: int) (seed: int) = Matrix<'T>.Build.Random(rows, cols, seed)

    /// Create a matrix with the given dimension and set all values to x.
    let inline create (rows: int) (cols: int) (x: 'T) = Matrix<'T>.Build.Dense(rows, cols, x)

    /// Create a matrix with the given dimension and set all diagonal values to x. All other values are zero.
    let inline diag (order: int) (x: 'T) = Matrix<'T>.Build.DenseDiagonal(order, x)

    /// Create a matrix with the given dimension and set all diagonal values to x. All other values are zero.
    let inline diag2 (rows: int) (cols: int) (x: 'T) = Matrix<'T>.Build.DenseDiagonal(rows, cols, x)

    /// Create an identity matrix with the given dimension.
    let inline identity<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (order: int) = Matrix<'T>.Build.DenseIdentity(order)

    /// Create an identity matrix with the given dimension.
    let inline identity2<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (rows: int) (cols: int) = Matrix<'T>.Build.DenseIdentity(rows, cols)

    /// Initialize a matrix by calling a construction function for every element.
    let inline init (rows: int) (cols: int) (f: int -> int -> 'T) = Matrix<'T>.Build.Dense(rows, cols, fun i j -> f i j)

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRows (rows: int) (f: int -> Vector<'T>) = Matrix<'T>.Build.DenseOfRowVectors(Array.init rows f)

    /// Initialize a matrix by calling a construction function for every column.
    let inline initColumns (cols: int) (f: int -> Vector<'T>) = Matrix<'T>.Build.DenseOfColumnVectors(Array.init cols f)

    /// Initialize a matrix by calling a construction function for every diagonal element. All other values are zero.
    let inline initDiag (rows: int) (cols: int) (f: int -> 'T) = Matrix<'T>.Build.DenseDiagonal(rows, cols, f)

    /// Create a matrix from a 2D array of floating point numbers.
    let inline ofArray2 array = Matrix<'T>.Build.DenseOfArray(array)

    /// Create a matrix from a 2D array of matrices.
    let inline ofMatrixArray2 array = Matrix<'T>.Build.DenseOfMatrixArray(array)

    /// Create a matrix from a list of matrix lists forming a 2D grid.
    let inline ofMatrixList2 (matrices: Matrix<'T> list list) = Matrix<'T>.Build.DenseOfMatrixArray(array2D matrices)

    /// Create a matrix from a list of row vectors.
    let inline ofRows (rows: Vector<'T> list) = Matrix<'T>.Build.DenseOfRowVectors(Array.ofList rows)

    /// Create a matrix from a list of row arrays.
    let inline ofRowArrays (rows: 'T[][]) = Matrix<'T>.Build.DenseOfRowArrays(rows :> seq<'T[]>) // workaround params issue - impl detects it's actually an array

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofRowList (rows: 'T list list) = rows |> List.map List.toArray |> List.toArray |> ofRowArrays

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofRowSeq (rows: #seq<#seq<'T>>) = rows |> Seq.map Seq.toArray |> Seq.toArray |> ofRowArrays

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofRowSeq2 (rows: int) (cols: int) (seqOfRows: #seq<seq<'T>>) = Matrix<'T>.Build.DenseOfRows(rows, cols, seqOfRows)

    /// Create a matrix from a list of column vectors.
    let inline ofColumns (columns: Vector<'T> list) = Matrix<'T>.Build.DenseOfColumnVectors(Array.ofList columns)

    /// Create a matrix from a list of column arrays.
    let inline ofColumnArrays (columns: 'T[][]) = Matrix<'T>.Build.DenseOfColumnArrays(columns :> seq<'T[]>) // workaround params issue - impl detects it's actually an array

    /// Create a matrix from a list of float lists. Every list in the master list specifies a column.
    let inline ofColumnList (columns: 'T list list) = columns |> List.map List.toArray |> List.toArray |> ofColumnArrays

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a column.
    let inline ofColumnSeq (columns: #seq<#seq<'T>>) = columns |> Seq.map Seq.toArray |> Seq.toArray |> ofColumnArrays

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a column.
    let inline ofColumnSeq2 (rows: int) (cols: int) (seqOfCols: #seq<seq<'T>>) = Matrix<'T>.Build.DenseOfColumns(rows, cols, seqOfCols)

    /// Create a matrix with a given dimension from an indexed list of row, column, value tuples.
    let inline ofListi (rows: int) (cols: int) (indexed: list<int * int * 'T>) = Matrix<'T>.Build.DenseOfIndexed(rows, cols, Seq.ofList indexed |> internalTuple3Seq)

    /// Create a matrix with a given dimension from an indexed sequences of row, column, value tuples.
    let inline ofSeqi (rows: int) (cols: int) (indexed: #seq<int * int * 'T>) = Matrix<'T>.Build.DenseOfIndexed(rows, cols, indexed |> internalTuple3Seq)

    /// Create a square matrix with the vector elements on the diagonal.
    let inline ofDiag (v: Vector<'T>) = Matrix<'T>.Build.DenseOfDiagonalVector(v)

    /// Create a matrix with the vector elements on the diagonal.
    let inline ofDiag2 (rows: int) (cols: int) (v: Vector<'T>) = Matrix<'T>.Build.DenseOfDiagonalVector(rows, cols, v)

    /// Create a square matrix with the array elements on the diagonal.
    let inline ofDiagArray (array: 'T array) = Matrix<'T>.Build.DenseOfDiagonalArray(array)

    /// Create a matrix with the array elements on the diagonal.
    let inline ofDiagArray2 (rows: int) (cols: int) (array: 'T array) = Matrix<'T>.Build.DenseOfDiagonalArray(rows, cols, array)

    /// Create a matrix by appending a list of matrices horizontally, the first matrix on the left.
    let inline append matrices = ofMatrixList2 [matrices]

    /// Create a matrix by stacking a list of matrices vertically, the first matrix on the top.
    let inline stack matrices = matrices |> List.map (fun x -> [x]) |> ofMatrixList2


/// A module which helps constructing generic sparse matrices.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SparseMatrix =

    /// Create a matrix that directly binds to a storage object.
    let inline ofStorage storage = Matrix<'T>.Build.Sparse(storage)

    /// Create an all-zero matrix with the given dimension.
    let inline zero<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (rows: int) (cols: int) = Matrix<'T>.Build.Sparse(rows, cols)

    /// Create a matrix with the given dimension and set all values to x. Note that a dense matrix would likely be more appropriate.
    let inline create (rows: int) (cols: int) (x: 'T) = Matrix<'T>.Build.Sparse(rows, cols, x)

    /// Create a matrix with the given dimension and set all diagonal values to x. All other values are zero.
    let inline diag (order: int) (x: 'T) = Matrix<'T>.Build.SparseDiagonal(order, x)

    /// Create a matrix with the given dimension and set all diagonal values to x. All other values are zero.
    let inline diag2 (rows: int) (cols: int) (x: 'T) = Matrix<'T>.Build.SparseDiagonal(rows, cols, x)

    /// Create an identity matrix with the given dimension.
    let inline identity<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (order: int) = Matrix<'T>.Build.SparseIdentity(order)

    /// Create an identity matrix with the given dimension.
    let inline identity2<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (rows: int) (cols: int) = Matrix<'T>.Build.SparseIdentity(rows, cols)

    /// Initialize a matrix by calling a construction function for every element.
    let inline init (rows: int) (cols: int) (f: int -> int -> 'T) = Matrix<'T>.Build.Sparse(rows, cols, fun n m -> f n m)

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRows (rows: int) (f: int -> Vector<'T>) = Matrix<'T>.Build.SparseOfRowVectors(Array.init rows f)

    /// Initialize a matrix by calling a construction function for every column.
    let inline initColumns (cols: int) (f: int -> Vector<'T>) = Matrix<'T>.Build.SparseOfColumnVectors(Array.init cols f)

    /// Initialize a matrix by calling a construction function for every diagonal element. All other values are zero.
    let inline initDiag (rows: int) (cols: int) (f: int -> 'T) = Matrix<'T>.Build.SparseDiagonal(rows, cols, f)

    /// Create a matrix from a 2D array of floating point numbers.
    let inline ofArray2 array = Matrix<'T>.Build.SparseOfArray(array)

    /// Create a matrix from a 2D array of matrices.
    let inline ofMatrixArray2 array = Matrix<'T>.Build.SparseOfMatrixArray(array)

    /// Create a matrix from a list of matrix lists forming a 2D grid.
    let inline ofMatrixList2 (matrices: Matrix<'T> list list) = Matrix<'T>.Build.SparseOfMatrixArray(array2D matrices)

    /// Create a matrix from a list of row vectors.
    let inline ofRows (rows: Vector<'T> list) = Matrix<'T>.Build.SparseOfRowVectors(Array.ofList rows)

    /// Create a matrix from a list of row arrays.
    let inline ofRowArrays (rows: 'T[][]) = Matrix<'T>.Build.SparseOfRowArrays(rows :> seq<'T[]>) // workaround params issue - impl detects it's actually an array

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofRowList (rows: 'T list list) = rows |> List.map List.toArray |> List.toArray |> ofRowArrays

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofRowSeq (rows: #seq<#seq<'T>>) = rows |> Seq.map Seq.toArray |> Seq.toArray |> ofRowArrays

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofRowSeq2 (rows: int) (cols: int) (seqOfRows: #seq<seq<'T>>) = Matrix<'T>.Build.SparseOfRows(rows, cols, seqOfRows)

    /// Create a matrix from a list of column vectors.
    let inline ofColumns (columns: Vector<'T> list) = Matrix<'T>.Build.SparseOfColumnVectors(Array.ofList columns)

    /// Create a matrix from a list of column arrays.
    let inline ofColumnArrays (columns: 'T[][]) = Matrix<'T>.Build.SparseOfColumnArrays(columns :> seq<'T[]>) // workaround params issue - impl detects it's actually an array

    /// Create a matrix from a list of float lists. Every list in the master list specifies a column.
    let inline ofColumnList (columns: 'T list list) = columns |> List.map List.toArray |> List.toArray |> ofColumnArrays

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a column.
    let inline ofColumnSeq (columns: #seq<#seq<'T>>) = columns |> Seq.map Seq.toArray |> Seq.toArray |> ofColumnArrays

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a column.
    let inline ofColumnSeq2 (rows: int) (cols: int) (seqOfCols: #seq<seq<'T>>) = Matrix<'T>.Build.SparseOfColumns(rows, cols, seqOfCols)

    /// Create a matrix with a given dimension from an indexed list of row, column, value tuples.
    let inline ofListi (rows: int) (cols: int) (indexed: list<int * int * 'T>) = Matrix<'T>.Build.SparseOfIndexed(rows, cols, Seq.ofList indexed |> internalTuple3Seq)

    /// Create a matrix with a given dimension from an indexed sequences of row, column, value tuples.
    let inline ofSeqi (rows: int) (cols: int) (indexed: #seq<int * int * 'T>) = Matrix<'T>.Build.SparseOfIndexed(rows, cols, indexed |> internalTuple3Seq)

    /// Create a square matrix with the vector elements on the diagonal.
    let inline ofDiag (v: Vector<'T>) = Matrix<'T>.Build.SparseOfDiagonalVector(v)

    /// Create a matrix with the vector elements on the diagonal.
    let inline ofDiag2 (rows: int) (cols: int) (v: Vector<'T>) = Matrix<'T>.Build.SparseOfDiagonalVector(rows, cols, v)

    /// Create a square matrix with the array elements on the diagonal.
    let inline ofDiagArray (array: 'T array) = Matrix<'T>.Build.SparseOfDiagonalArray(array)

    /// Create a matrix with the array elements on the diagonal.
    let inline ofDiagArray2 (rows: int) (cols: int) (array: 'T array) = Matrix<'T>.Build.SparseOfDiagonalArray(rows, cols, array)

    /// Create a matrix by appending a list of matrices horizontally, the first matrix on the left.
    let inline append matrices = ofMatrixList2 [matrices]

    /// Create a matrix by stacking a list of matrices vertically, the first matrix on the top.
    let inline stack matrices = matrices |> List.map (fun x -> [x]) |> ofMatrixList2


/// A module which helps constructing generic diagonal matrices.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DiagonalMatrix =

    /// Create a matrix that directly binds to a storage object.
    let inline ofStorage (storage:Storage.DiagonalMatrixStorage<_>) = Matrix<'T>.Build.Diagonal(storage)

    /// Create a square matrix that directly binds to a raw storage array that represents the diagonal, without copying.
    let inline raw (diagonal: 'T[]) = Matrix<'T>.Build.Diagonal(diagonal)

    /// Create a matrix that directly binds to a raw storage array that represents the diagonal, without copying.
    let inline raw2 (rows: int) (cols: int) (diagonal: 'T[]) = Matrix<'T>.Build.Diagonal(rows, cols, diagonal)

    /// Create an all-zero matrix with the given dimension.
    let inline zero<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (rows: int) (cols: int) = Matrix<'T>.Build.Diagonal(rows, cols)

    /// Create a square matrix with the given order and set all diagonal values to x.
    let inline create (order: int) (x: 'T) = Matrix<'T>.Build.Diagonal(order, order, x)

    /// Create a matrix with the given dimension and set all diagonal values to x.
    let inline create2 (rows: int) (cols: int) (x: 'T) = Matrix<'T>.Build.Diagonal(rows, cols, x)

    /// Create an identity matrix with the given dimension.
    let inline identity<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (order: int) = Matrix<'T>.Build.DiagonalIdentity(order)

    /// Create an identity matrix with the given dimension.
    let inline identity2<'T when 'T:struct and 'T :> ValueType and 'T: (new: unit ->'T) and 'T :> IEquatable<'T> and 'T :> IFormattable>
        (rows: int) (cols: int) = Matrix<'T>.Build.DiagonalIdentity(rows, cols)

    /// Initialize a square matrix by calling a construction function for every element.
    let inline init (order: int) (f: int -> 'T) = Matrix<'T>.Build.Diagonal(order, order, fun k -> f k)

    /// Initialize a matrix by calling a construction function for every element.
    let inline init2 (rows: int) (cols: int) (f: int -> 'T) = Matrix<'T>.Build.Diagonal(rows, cols, fun k -> f k)

    /// Create a square matrix with the vector elements on the diagonal.
    let inline ofDiag (v: Vector<'T>) = Matrix<'T>.Build.DiagonalOfDiagonalVector(v)

    /// Create a matrix with the vector elements on the diagonal.
    let inline ofDiag2 (rows: int) (cols: int) (v: Vector<'T>) = Matrix<'T>.Build.DiagonalOfDiagonalVector(rows, cols, v)

    /// Create a square matrix with the array elements on the diagonal.
    let inline ofDiagArray (array: 'T array) = Matrix<'T>.Build.DiagonalOfDiagonalArray(array)

    /// Create a matrix with the array elements on the diagonal.
    let inline ofDiagArray2 (rows: int) (cols: int) (array: 'T array) = Matrix<'T>.Build.DiagonalOfDiagonalArray(rows, cols, array)


/// Module that contains implementation of useful F#-specific extension members for generic matrices
[<AutoOpen>]
module MatrixExtensions =

    /// Construct a dense matrix from a nested list of numbers.
    let inline matrix (lst: list<list<'T>>) = DenseMatrix.ofRowList lst

    // A type extension for the generic matrix type that
    // adds the 'GetSlice' method to allow m.[r1 .. r2, c1 .. c2] syntax
    type MathNet.Numerics.LinearAlgebra.
        Matrix<'T when 'T : struct and 'T : (new : unit -> 'T)
                   and 'T :> System.IEquatable<'T> and 'T :> System.IFormattable
                   and 'T :> System.ValueType> with

        /// Gets a submatrix using a specified column range and
        /// row range (all indices are optional)
        /// This method can be used via the x.[r1 .. r2, c1 .. c2 ] syntax
        member x.GetSlice(rstart, rfinish, cstart, cfinish) =
          let cstart = defaultArg cstart 0
          let rstart = defaultArg rstart 0
          let cfinish = defaultArg cfinish (x.ColumnCount - 1)
          let rfinish = defaultArg rfinish (x.RowCount - 1)
          x.SubMatrix(rstart, rfinish - rstart + 1, cstart, cfinish - cstart + 1)

        /// Sets a submatrix using a specified column range and
        /// row range (all indices are optional)
        /// This method can be used via the x.[r1 .. r2, c1 .. c2 ] <- m syntax
        member x.SetSlice(rstart, rfinish, cstart, cfinish, values) =
          let cstart = defaultArg cstart 0
          let rstart = defaultArg rstart 0
          let cfinish = defaultArg cfinish (x.ColumnCount - 1)
          let rfinish = defaultArg rfinish (x.RowCount - 1)
          x.SetSubMatrix(rstart, rfinish - rstart + 1, cstart, cfinish - cstart + 1, values)

        /// Gets a row subvector using a specified row index and column range.
        /// This method can be used via the x.[r, c1 .. c2] syntax (F#3.1)
        member x.GetSlice(r, cstart, cfinish) =
          let cstart = defaultArg cstart 0
          let cfinish = defaultArg cfinish (x.ColumnCount - 1)
          x.Row(r, cstart, cfinish - cstart + 1)

        /// Gets a column subvector using a specified row index and column range.
        /// This method can be used via the x.[r1 .. r2, c] syntax (F#3.1)
        member x.GetSlice(rstart, rfinish, c) =
          let rstart = defaultArg rstart 0
          let rfinish = defaultArg rfinish (x.RowCount - 1)
          x.Column(c, rstart, rfinish - rstart + 1)

        /// Sets a row subvector using a specified row index and column range.
        /// This method can be used via the x.[r, c1 .. c2] <- v syntax (F#3.1)
        member x.SetSlice(r, cstart, cfinish, values) =
          let cstart = defaultArg cstart 0
          let cfinish = defaultArg cfinish (x.ColumnCount - 1)
          x.SetRow(r, cstart, cfinish - cstart + 1, values)

        /// Sets a column subvector using a specified row index and column range.
        /// This method can be used via the x.[r1 .. r2, c] <- v syntax (F#3.1)
        member x.SetSlice(rstart, rfinish, c, values) =
          let rstart = defaultArg rstart 0
          let rfinish = defaultArg rfinish (x.RowCount - 1)
          x.SetColumn(c, rstart, rfinish - rstart + 1, values)
