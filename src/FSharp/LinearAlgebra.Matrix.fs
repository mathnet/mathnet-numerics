// <copyright file="LinearAlgebra.Matrix.fs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra

/// Module that contains implementation of useful F#-specific extension members for generic matrices
[<AutoOpen>]
module MatrixExtensions =

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


/// A module which implements functional matrix operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Matrix =

    /// Transform a matrix into a 2D array.
    let inline toArray2 (A: #Matrix<_>) = A.ToArray()


    /// Transform a matrix into a sequence.
    let inline toSeq (m: #Matrix<_>) = m.Enumerate()

    /// Transform a matrix into an indexed sequence.
    let inline toSeqi (m: #Matrix<_>) = m.EnumerateIndexed()

    /// Transform a matrix into a sequence where zero-values are skipped. Skipping zeros is efficient on sparse data.
    let inline toSeqnz (m: #Matrix<_>) = m.EnumerateNonZero()

    /// Transform a matrix into an indexed sequence where zero-values are skipped. Skipping zeros is efficient on sparse data.
    let inline toSeqinz (m: #Matrix<_>) = m.EnumerateNonZeroIndexed()

    /// Transform a matrix into a column sequence.
    let inline toColSeq (m: #Matrix<_>) = m.EnumerateColumns()

    /// Transform a matrix into an indexed column sequence.
    let inline toColSeqi (m: #Matrix<_>) = m.EnumerateColumnsIndexed()

    /// Transform a matrix into a row sequence.
    let inline toRowSeq (m: #Matrix<_>) = m.EnumerateRows()

    /// Transform a matrix into an indexed row sequence.
    let inline toRowSeqi (m: #Matrix<_>) = m.EnumerateRowsIndexed()


    /// Applies a function to all elements of the matrix.
    let inline iter f (m: #Matrix<_>) = m.Enumerate() |> Seq.iter f

    /// Applies a function to all indexed elements of the matrix.
    let inline iteri f (m: #Matrix<_>) = m.EnumerateIndexed() |> Seq.iter (fun (i, j, x) -> f i j x)

    /// Applies a function to all non-zero elements of the matrix. Skipping zeros is efficient on sparse data.
    let inline iternz f (m: #Matrix<_>) = m.EnumerateNonZero() |> Seq.iter f

    /// Applies a function to all non-zero indexed elements of the matrix. Skipping zeros is efficient on sparse data.
    let inline iterinz f (m: #Matrix<_>) = m.EnumerateNonZeroIndexed() |> Seq.iter (fun (i, j, x) -> f i j x)

    /// Applies a function to all columns of the matrix.
    let inline iterCols f (m: #Matrix<_>) = m.EnumerateColumns() |> Seq.iter f

    /// Applies a function to all indexed columns of the matrix.
    let inline iteriCols f (m: #Matrix<_>) = m.EnumerateColumns() |> Seq.iteri f

    /// Applies a function to all rows of the matrix.
    let inline iterRows f (m: #Matrix<_>) = m.EnumerateRows() |> Seq.iter f

    /// Applies a function to all indexed rows of the matrix.
    let inline iteriRows f (m: #Matrix<_>) = m.EnumerateRows() |> Seq.iteri f


    /// Fold all entries of a matrix.
    let inline fold f state (m: #Matrix<_>) = m.Enumerate() |> Seq.fold f state

    /// Fold all entries of a matrix with an indexed folding function.
    let inline foldi f state (m: #Matrix<_>) = m.EnumerateIndexed() |> Seq.fold (fun s (i,j,x) -> f i j s x) state

    /// Fold all non-zero entries of a matrix. Skipping zeros is efficient on sparse data.
    let inline foldnz f state (m: #Matrix<_>) = m.EnumerateNonZero() |> Seq.fold f state

    /// Fold all non-zero entries of a matrix with an indexed folding function. Skipping zeros is efficient on sparse data.
    let inline foldinz f state (m: #Matrix<_>) = m.EnumerateNonZeroIndexed() |> Seq.fold (fun s (i,j,x) -> f i j s x) state

    /// Fold all columns of a matrix.
    let inline foldCols f state (m: #Matrix<_>) = m.EnumerateColumns() |> Seq.fold f state

    /// Fold all columns of a matrix with an indexed folding function.
    let inline foldiCols f state (m: #Matrix<_>) = m.EnumerateColumnsIndexed() |> Seq.fold (fun s (j,x) -> f j s x) state

    /// Fold all rows of a matrix.
    let inline foldRows f state (m: #Matrix<_>) = m.EnumerateRows() |> Seq.fold f state

    /// Fold all rows of a matrix with an indexed folding function.
    let inline foldiRows f state (m: #Matrix<_>) = m.EnumerateRowsIndexed() |> Seq.fold (fun s (i,x) -> f i s x) state


    /// Scan all entries of a matrix.
    let inline scan f state (m: #Matrix<_>) = m.Enumerate() |> Seq.scan f state

    /// Scan all entries of a matrix with an indexed folding function.
    let inline scani f state (m: #Matrix<_>) = m.EnumerateIndexed() |> Seq.scan (fun s (i,j,x) -> f i j s x) state

    /// Scan all non-zero entries of a matrix. Skipping zeros is efficient on sparse data.
    let inline scannz f state (m: #Matrix<_>) = m.EnumerateNonZero() |> Seq.scan f state

    /// Scan all non-zero entries of a matrix with an indexed folding function. Skipping zeros is efficient on sparse data.
    let inline scaninz f state (m: #Matrix<_>) = m.EnumerateNonZeroIndexed() |> Seq.scan (fun s (i,j,x) -> f i j s x) state

    /// Scan all columns of a matrix.
    let inline scanCols f state (m: #Matrix<_>) = m.EnumerateColumns() |> Seq.scan f state

    /// Scan all columns of a matrix with an indexed folding function.
    let inline scaniCols f state (m: #Matrix<_>) = m.EnumerateColumnsIndexed() |> Seq.scan (fun s (j,x) -> f j s x) state

    /// Scan all rows of a matrix.
    let inline scanRows f state (m: #Matrix<_>) = m.EnumerateRows() |> Seq.scan f state

    /// Scan all rows of a matrix with an indexed folding function.
    let inline scaniRows f state (m: #Matrix<_>) = m.EnumerateRowsIndexed() |> Seq.scan (fun s (i,x) -> f i s x) state


    /// Reduce all entries of a matrix.
    let inline reduce f (m: #Matrix<_>) = m.Enumerate() |> Seq.reduce f

    /// Reduce all non-zero entries of a matrix. Skipping zeros is efficient on sparse data.
    let inline reducenz f (m: #Matrix<_>) = m.EnumerateNonZero() |> Seq.reduce f

    /// Reduce all columns of a matrix.
    let inline reduceCols f (m: #Matrix<_>) = m.EnumerateColumns() |> Seq.reduce f

    /// Reduce all rows of a matrix.
    let inline reduceRows f (m: #Matrix<_>) = m.EnumerateColumns() |> Seq.reduce f


    /// Checks whether there is an entry in the matrix that satisfies a predicate.
    let inline exists p (m: #Matrix<_>) = m.Enumerate() |> Seq.exists p

    /// Checks whether there is an entry in the matrix that satisfies a position dependent predicate.
    let inline existsi p (m: #Matrix<_>) = m.EnumerateIndexed() |> Seq.exists (fun (i,j,x) -> p i j x)

    /// Checks whether there is a non-zero entry in the matrix that satisfies a predicate. Skipping zeros is efficient on sparse data.
    let inline existsnz p (m: #Matrix<_>) = m.EnumerateNonZero() |> Seq.exists p

    /// Checks whether there is a non-zero entry in the matrix that satisfies a position dependent predicate. Skipping zeros is efficient on sparse data.
    let inline existsinz p (m: #Matrix<_>) = m.EnumerateNonZeroIndexed() |> Seq.exists (fun (i,j,x) -> p i j x)

    /// Checks whether there is a column in the matrix that satisfies a predicate.
    let inline existsCol p (m: #Matrix<_>) = m.EnumerateColumns() |> Seq.exists p
    
    /// Checks whether there is a column in the matrix that satisfies a position dependent predicate.
    let inline existsiCol p (m: #Matrix<_>) = m.EnumerateColumnsIndexed() |> Seq.exists (fun (j,x) -> p j x)
    
    /// Checks whether there is a row in the matrix that satisfies a predicate.
    let inline existsRow p (m: #Matrix<_>) = m.EnumerateRows() |> Seq.exists p
    
    /// Checks whether there is a row in the matrix that satisfies a position dependent predicate.
    let inline existsiRow p (m: #Matrix<_>) = m.EnumerateRowsIndexed() |> Seq.exists (fun (i,x) -> p i x)


    /// Checks whether all entries in the matrix that satisfies a given predicate.
    let inline forall p (m: #Matrix<_>) = m.Enumerate() |> Seq.forall p

    /// Checks whether all entries in the matrix that satisfies a given position dependent predicate.
    let inline foralli p (m: #Matrix<_>) = m.EnumerateIndexed() |> Seq.forall (fun (i,j,x) -> p i j x)

    /// Checks whether all non-zero entries in the matrix that satisfies a given predicate. Skipping zeros is efficient on sparse data.
    let inline forallnz p (m: #Matrix<_>) = m.EnumerateNonZero() |> Seq.forall p

    /// Checks whether all non-zero entries in the matrix that satisfies a given position dependent predicate. Skipping zeros is efficient on sparse data.
    let inline forallinz p (m: #Matrix<_>) = m.EnumerateNonZeroIndexed() |> Seq.forall (fun (i,j,x) -> p i j x)

    /// Checks whether all columns in the matrix that satisfy a predicate.
    let inline forallCols p (m: #Matrix<_>) = m.EnumerateColumns() |> Seq.forall p
    
    /// Checks whether all columns in the matrix that satisfy a position dependent predicate.
    let inline foralliCols p (m: #Matrix<_>) = m.EnumerateColumnsIndexed() |> Seq.forall (fun (j,x) -> p j x)
    
    /// Checks whether all rows in the matrix that satisfy a predicate.
    let inline forallRows p (m: #Matrix<_>) = m.EnumerateRows() |> Seq.forall p
    
    /// Checks whether all rows in the matrix that satisfy a position dependent predicate.
    let inline foralliRows p (m: #Matrix<_>) = m.EnumerateRowsIndexed() |> Seq.forall (fun (i,x) -> p i x)



    /// In-place map of every matrix element using a function.
    let inline mapInPlace f (A: #Matrix<_>) =
        A.MapInplace((fun x -> f x), true)

    /// In-place map of every matrix element using a position dependent function.
    let inline mapiInPlace f (A: #Matrix<_>) =
        A.MapIndexedInplace((fun i j x -> f i j x), true)

    /// In-place map of every matrix element using a function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapnzInPlace f (A: #Matrix<_>) =
        A.MapInplace((fun x -> f x), false)

    /// In-place map of every matrix element using a position dependent function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapinzInPlace f (A: #Matrix<_>) =
        A.MapIndexedInplace((fun i j x -> f i j x), false)

    /// In-place map every matrix column using the given position dependent function.
    let inline mapColsInPlace (f: int -> Vector<'a> -> Vector<'a>) (A: #Matrix<_>) =
        for j = 0 to A.ColumnCount-1 do
            A.SetColumn(j, f j (A.Column(j)))

    /// In-place map every matrix row using the given position dependent function.
    let inline mapRowsInPlace (f: int -> Vector<'a> -> Vector<'a>) (A: #Matrix<_>) =
        for i = 0 to A.RowCount-1 do
            A.SetRow(i, f i (A.Row(i)))


    /// Map every matrix element using the given function.
    let inline map f (A: #Matrix<_>) =
        let A = A.Clone()
        A.MapInplace((fun x -> f x), true)
        A

    /// Map every matrix element using the given function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapnz f (A: #Matrix<_>) =
        let A = A.Clone()
        A.MapInplace((fun x -> f x), false)
        A

    /// Map every matrix element using the given position dependent function.
    let inline mapi f (A: #Matrix<_>) =
        let A = A.Clone()
        A.MapIndexedInplace((fun i j x -> f i j x), true)
        A

    /// Map every matrix element using the given position dependent function.
    /// Zero-values may be skipped (relevant mostly for sparse matrices).
    let inline mapinz f (A: #Matrix<_>) =
        let A = A.Clone()
        A.MapIndexedInplace((fun i j x -> f i j x), false)
        A

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
        A.MapIndexedInplace((fun i j x -> f i j), true)

    /// Returns the sum of the results generated by applying a position dependent function to each column of the matrix.
    let inline sumColsBy f (A: #Matrix<_>) =
        A.EnumerateColumnsIndexed() |> Seq.map (fun (j,col) -> f j col) |> Seq.reduce (+)

    /// Returns the sum of the results generated by applying a position dependent function to each row of the matrix.
    let inline sumRowsBy f (A: #Matrix<_>) =
        A.EnumerateRowsIndexed() |> Seq.map (fun (i,row) -> f i row)  |> Seq.reduce (+)
