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

    /// Transform a vector into a 2D array.
    let inline toArray2 (A: #Matrix<_>) = A.ToArray()

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

    /// Fold a function over all matrix elements.
    let inline fold f acc0 (A: #Matrix<_>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i=0 to n-1 do
            for j=0 to m-1 do
                acc <- f acc (A.At(i,j))
        acc

    /// Fold a function over all matrix elements in reverse order.
    let inline foldBack f acc0 (A: #Matrix<_>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i in n-1 .. -1 .. 0 do
            for j in m-1 .. -1 .. 0 do
                acc <- f (A.At(i,j)) acc
        acc

    /// Fold a matrix by applying a given function to all matrix elements.
    let inline foldi f acc0 (A: #Matrix<_>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i=0 to n-1 do
            for j=0 to m-1 do
                acc <- f i j acc (A.At(i,j))
        acc

    /// Checks whether a predicate holds for all elements of a matrix.
    let inline forall p (A: #Matrix<_>) =
        let mutable b = true
        let mutable i = 0
        let mutable j = 0
        while b && i < A.RowCount do
            b <- b && (p (A.At(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Chechks whether a predicate holds for at least one element of a matrix.
    let inline exists p (A: #Matrix<_>) =
        let mutable b = false
        let mutable i = 0
        let mutable j = 0
        while not(b) && i < A.RowCount do
            b <- b || (p (A.At(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Checks whether a position dependent predicate holds for all elements of a matrix.
    let inline foralli p (A: #Matrix<_>) =
        let mutable b = true
        let mutable i = 0
        let mutable j = 0
        while b && i < A.RowCount do
            b <- b && (p i j (A.At(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Checks whether a position dependent predicate holds for at least one element of a matrix.
    let inline existsi p (A: #Matrix<_>) =
        let mutable b = false
        let mutable i = 0
        let mutable j = 0
        while not(b) && i < A.RowCount do
            b <- b || (p i j (A.At(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// In-place assignment.
    let inline inplaceAssign f (A: #Matrix<_>) =
        A.MapIndexedInplace((fun i j x -> f i j), true)

    /// Iterates over all elements of a matrix.
    let inline iter f (A: #Matrix<_>) =
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                f (A.At(i,j))

    /// Iterates over all elements of a matrix using the element indices.
    let inline iteri f (A: #Matrix<_>) =
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                f i j (A.At(i,j))

    /// Fold one column.
    let inline foldCol f acc (A: #Matrix<_>) k =
        let mutable macc = acc
        for i=0 to A.RowCount-1 do
            macc <- f macc (A.Item(i,k))
        macc

    /// Fold one row.
    let inline foldRow f acc (A: #Matrix<_>) k =
        let mutable macc = acc
        for i=0 to A.ColumnCount-1 do
            macc <- f macc (A.Item(k,i))
        macc

    /// Returns the sum of the results generated by applying a position dependent function to each column of the matrix.
    let inline sumColsBy f (A: #Matrix<_>) =
        A.EnumerateColumnsIndexed() |> Seq.map (fun (j,col) -> f j col) |> Seq.reduce (+)

    /// Returns the sum of the results generated by applying a position dependent function to each row of the matrix.
    let inline sumRowsBy f (A: #Matrix<_>) =
        A.EnumerateRowsIndexed() |> Seq.map (fun (i,row) -> f i row)  |> Seq.reduce (+)

    /// Creates a sequence that iterates the non-zero entries in the matrix.
    let nonZeroEntries (A: #Matrix<_>) = A.EnumerateNonZero()
