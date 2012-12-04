// <copyright file="LinearAlgebra.Double.Matrix.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2012 Math.NET
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

    /// Fold a function over all matrix elements.
    let inline fold (f: 'a -> float -> 'a) (acc0: 'a) (A: #Matrix<float>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i=0 to n-1 do
            for j=0 to m-1 do
                acc <- f acc (A.Item(i,j))
        acc

    /// Fold a function over all matrix elements in reverse order.
    let inline foldBack (f: float -> 'a -> 'a) (acc0: 'a) (A: #Matrix<float>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i in n-1 .. -1 .. 0 do
            for j in m-1 .. -1 .. 0 do
                acc <- f (A.Item(i,j)) acc
        acc

    /// Fold a matrix by applying a given function to all matrix elements.
    let inline foldi (f: int -> int -> 'a -> float -> 'a) (acc0: 'a) (A: #Matrix<float>) =
        let n = A.RowCount
        let m = A.ColumnCount
        let mutable acc = acc0
        for i=0 to n-1 do
            for j=0 to m-1 do
                acc <- f i j acc (A.Item(i,j))
        acc

    /// Create a 2D array from a matrix.
    let inline toArray2 (A: #Matrix<float>) =
        let n = A.RowCount
        let m = A.ColumnCount
        Array2D.init n m (fun i j -> (A.Item(i,j)))

    /// Checks whether a predicate holds for all elements of a matrix.
    let inline forall (p: float -> bool) (A: #Matrix<float>) =
        let mutable b = true
        let mutable i = 0
        let mutable j = 0
        while b && i < A.RowCount do
            b <- b && (p (A.Item(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Chechks whether a predicate holds for at least one element of a matrix.
    let inline exists (p: float -> bool) (A: #Matrix<float>) =
        let mutable b = false
        let mutable i = 0
        let mutable j = 0
        while not(b) && i < A.RowCount do
            b <- b || (p (A.Item(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Checks whether a position dependent predicate holds for all elements of a matrix.
    let inline foralli (p: int -> int -> float -> bool) (A: #Matrix<float>) =
        let mutable b = true
        let mutable i = 0
        let mutable j = 0
        while b && i < A.RowCount do
            b <- b && (p i j (A.Item(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Checks whether a position dependent predicate holds for at least one element of a matrix.
    let inline existsi (p: int -> int -> float -> bool) (A: #Matrix<float>) =
        let mutable b = false
        let mutable i = 0
        let mutable j = 0
        while not(b) && i < A.RowCount do
            b <- b || (p i j (A.Item(i,j)))
            j <- j+1
            if j = A.ColumnCount then i <- i+1; j <- 0
        b

    /// Map every matrix element using the given function.
    let inline map (f: float -> float) (A: #Matrix<float>) =
        let N = A.RowCount
        let M = A.ColumnCount
        let C = A.Clone()
        for i=0 to N-1 do
            for j=0 to M-1 do
                C.[i,j] <- f (C.Item(i,j))
        C

    /// Map every matrix element using the given position dependent function.
    let inline mapi (f: int -> int -> float -> float) (A: #Matrix<float>) =
        let N = A.RowCount
        let M = A.ColumnCount
        let C = A.Clone()
        for i=0 to N-1 do
            for j=0 to M-1 do
                C.[i,j] <- f i j (C.Item(i,j))
        C

    /// In-place map every matrix column using the given position dependent function.
    let inline inplaceMapCols (f: int -> Vector<float> -> Vector<float>) (A: #Matrix<float>) =
        for j = 0 to A.ColumnCount-1 do
            A.SetColumn(j, f j (A.Column(j)))
        ()

    /// In-place map every matrix row using the given position dependent function.
    let inline inplaceMapRows (f: int -> Vector<float> -> Vector<float>) (A: #Matrix<float>) =
        for i = 0 to A.RowCount-1 do
            A.SetRow(i, f i (A.Row(i)))
        ()

    /// Map every matrix column using the given position dependent function.
    let inline mapCols (f: int -> Vector<float> -> Vector<float>) (A: #Matrix<float>) =
        let A = A.Clone()
        inplaceMapCols f A
        A

    /// Map every matrix row using the given position dependent function.
    let inline mapRows (f: int -> Vector<float> -> Vector<float>) (A: #Matrix<float>) =
        let A = A.Clone()
        inplaceMapRows f A
        A

    /// In-place assignment.
    let inline inplaceAssign (f: int -> int -> float) (A: #Matrix<float>) =
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                A.Item(i,j) <- f i j

    /// In-place map of every matrix element using a position dependent function.
    let inline inplaceMapi (f: int -> int -> float -> float) (A: #Matrix<float>) =
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                A.Item(i,j) <- f i j (A.Item(i,j))

    /// Creates a sequence that iterates the non-zero entries in the matrix.
    let inline nonZeroEntries (A: #Matrix<float>) =
        seq { for i in 0 .. A.RowCount-1 do
                for j in 0 .. A.ColumnCount-1 do
                  if A.Item(i,j) <> 0.0 then yield (i,j, A.Item(i,j)) }

    /// Returns the sum of all elements of a matrix.
    let inline sum (A: #Matrix<float>) =
        let mutable f = 0.0
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                f <- f + A.Item(i,j)
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
                f (A.Item(i,j))
        ()

    /// Iterates over all elements of a matrix using the element indices.
    let inline iteri (f: int -> int -> float -> unit) (A: #Matrix<float>) =
        for i=0 to A.RowCount-1 do
            for j=0 to A.ColumnCount-1 do
                f i j (A.Item(i,j))
        ()

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
                macc <- f macc (A.Item(i,k))
            v.[k] <- macc
        v :> Vector<float>

    /// Fold all rows into one column vector.
    let inline foldByRow (f: float -> float -> float) acc (A: #Matrix<float>) =
        let v = new DenseVector(A.RowCount)
        for k=0 to A.RowCount-1 do
            let mutable macc = acc
            for i=0 to A.ColumnCount-1 do
                macc <- f macc (A.Item(k,i))
            v.[k] <- macc
        v :> Vector<float>

/// A module which implements functional dense vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DenseMatrix =

    /// Initialize a matrix by calling a construction function for every element.
    let inline init (n: int) (m: int) f =
        let A = new DenseMatrix(n,m)
        for i=0 to n-1 do
            for j=0 to m-1 do
                A.[i,j] <- f i j
        A

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofList (fll: float list list) =
        let n = List.length fll
        let m = List.length (List.head fll)
        let A = DenseMatrix(n,m)
        fll |> List.iteri (fun i fl ->
                            if (List.length fl) <> m then failwith "Each subrow must be of the same length." else
                            List.iteri (fun j f -> A.[i,j] <- f) fl)
        A

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofSeq (fss: #seq<#seq<float>>) =
        let n = Seq.length fss
        let m = Seq.length (Seq.head fss)
        let A = DenseMatrix(n,m)
        fss |> Seq.iteri (fun i fs ->
                            if (Seq.length fs) <> m then failwith "Each subrow must be of the same length." else
                            Seq.iteri (fun j f -> A.[i,j] <- f) fs)
        A

    /// Create a matrix from a 2D array of floating point numbers.
    let inline ofArray2 (arr: float[,]) = new DenseMatrix(arr)

    /// Create a matrix with the given entries.
    let inline initDense (n: int) (m: int) (es: #seq<int * int * float>) =
        let A = new DenseMatrix(n,m)
        Seq.iter (fun (i,j,f) -> A.[i,j] <- f) es
        A

    /// Create a square matrix with constant diagonal entries.
    let inline constDiag (n: int) (f: float) =
        let A = new DenseMatrix(n,n)
        for i=0 to n-1 do
            A.[i,i] <- f
        A

    /// Create a square matrix with the vector elements on the diagonal.
    let inline diag (v: #Vector<float>) =
        let n = v.Count
        let A = new DenseMatrix(n,n)
        for i=0 to n-1 do
            A.[i,i] <- v.Item(i)
        A

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRow (n: int) (m: int) (f: int -> #Vector<float>) =
        let A = new DenseMatrix(n,m)
        for i=0 to n-1 do A.SetRow(i, f i)
        A

    /// Initialize a matrix by calling a construction function for every column.
    let inline initCol (n: int) (m: int) (f: int -> #Vector<float>) =
        let A = new DenseMatrix(n,m)
        for i=0 to m-1 do A.SetColumn(i, f i)
        A

/// A module which implements functional sparse vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SparseMatrix =

    /// Create a matrix from a list of float lists. Every list in the master list specifies a row.
    let inline ofList (rows: int) (cols: int) (fll: list<int * int * float>) =
        let A = new SparseMatrix(rows, cols)
        fll |> List.iter (fun (i, j, x) -> A.[i,j] <- x)
        A

    /// Create a matrix from a list of sequences. Every sequence in the master sequence specifies a row.
    let inline ofSeq (rows: int) (cols: int) (fss: #seq<int * int * float>) =
        let A = new SparseMatrix(rows, cols)
        fss |> Seq.iter (fun (i, j, x) -> A.[i,j] <- x)
        A

    /// Create a square matrix with constant diagonal entries.
    let inline constDiag (n: int) (f: float) =
        let A = new SparseMatrix(n,n)
        for i=0 to n-1 do
            A.[i,i] <- f
        A

    /// Create a square matrix with the vector elements on the diagonal.
    let inline diag (v: #Vector<float>) =
        let n = v.Count
        let A = new SparseMatrix(n,n)
        for i=0 to n-1 do
            A.[i,i] <- v.Item(i)
        A

    /// Initialize a matrix by calling a construction function for every row.
    let inline initRow (n: int) (m: int) (f: int -> #Vector<float>) =
        let A = new SparseMatrix(n,m)
        for i=0 to n-1 do A.SetRow(i, f i)
        A

    /// Initialize a matrix by calling a construction function for every column.
    let inline initCol (n: int) (m: int) (f: int -> #Vector<float>) =
        let A = new SparseMatrix(n,m)
        for i=0 to m-1 do A.SetColumn(i, f i)
        A