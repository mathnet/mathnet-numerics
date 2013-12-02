// <copyright file="LinearAlgebra.Vector.fs" company="Math.NET">
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


/// A module which implements functional vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Vector =

    /// Transform a vector into an array.
    let inline toArray (v: #Vector<_>) = v.ToArray()

    /// Transform a vector into a list.
    let inline toList (v: #Vector<_>) = List.init v.Count v.At


    /// Transform a vector into a sequence.
    let inline toSeq (v: #Vector<_>) = v.Enumerate()

    /// Transform a vector into an indexed sequence.
    let inline toSeqi (v: #Vector<_>) = v.EnumerateIndexed()

    /// Transform a vector into a sequence where zero-values are skipped. Skipping zeros is efficient on sparse data.
    let inline toSeqSkipZeros (v: #Vector<_>) = v.EnumerateNonZero()

    /// Transform a vector into an indexed sequence where zero-values are skipped. Skipping zeros is efficient on sparse data.
    let inline toSeqiSkipZeros (v: #Vector<_>) = v.EnumerateNonZeroIndexed()


    /// Applies a function to all elements of the vector.
    let inline iter f (v: #Vector<_>) = v.Enumerate() |> Seq.iter f

    /// Applies a function to all indexed elements of the vector.
    let inline iteri f (v: #Vector<_>) = v.Enumerate() |> Seq.iteri f

    /// Applies a function to all non-zero elements of the vector. Skipping zeros is efficient on sparse data.
    let inline iterSkipZeros f (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.iter f

    /// Applies a function to all non-zero indexed elements of the vector. Skipping zeros is efficient on sparse data.
    let inline iteriSkipZeros f (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.iter (fun (i,x) -> f i x)


    /// Fold all entries of a vector.
    let inline fold f state (v: #Vector<_>) = v.Enumerate() |> Seq.fold f state

    /// Fold all entries of a vector using a position dependent folding function.
    let inline foldi f state (v: #Vector<_>) = v.EnumerateIndexed() |> Seq.fold (fun s (i,x) -> f i s x) state

    /// Fold all non-zero entries of a vector. Skipping zeros is efficient on sparse data.
    let inline foldSkipZeros f state (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.fold f state

    /// Fold all non-zero entries of a vector using a position dependent folding function. Skipping zeros is efficient on sparse data.
    let inline foldiSkipZeros f state (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.fold (fun s (i,x) -> f i s x) state


    /// Scan all entries of a vector.
    let inline scan f state (v: #Vector<_>) = v.Enumerate() |> Seq.scan f state

    /// Scan all entries of a vector using a position dependent folding function.
    let inline scani f state (v: #Vector<_>) = v.EnumerateIndexed() |> Seq.scan (fun s (i,x) -> f i s x) state

    /// Scan all non-zero entries of a vector. Skipping zeros is efficient on sparse data.
    let inline scanSkipZeros f state (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.scan f state

    /// Scan all non-zero entries of a vector using a position dependent folding function. Skipping zeros is efficient on sparse data.
    let inline scaniSkipZeros f state (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.scan (fun s (i,x) -> f i s x) state


    /// Reduce all entries of a vector.
    let inline reduce f (v: #Vector<_>) = v.Enumerate() |> Seq.reduce f

    /// Reduce all non-zero entries of a vector. Skipping zeros is efficient on sparse data.
    let inline reduceSkipZeros f (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.reduce f


    /// Checks whether there is an entry in the vector that satisfies a predicate.
    let inline exists p (v: #Vector<_>) = v.Enumerate() |> Seq.exists p

    /// Checks whether there is an entry in the vector that satisfies a position dependent predicate.
    let inline existsi p (v: #Vector<_>) = v.EnumerateIndexed() |> Seq.exists (fun (i,x) -> p i x)

    /// Checks whether there is a non-zero entry in the vector that satisfies a predicate. Skipping zeros is efficient on sparse data.
    let inline existsSkipZeros p (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.exists p

    /// Checks whether there is a non-zero entry in the vector that satisfies a position dependent predicate. Skipping zeros is efficient on sparse data.
    let inline existsiSkipZeros p (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.exists (fun (i,x) -> p i x)


    /// Checks whether all entries in the vector that satisfies a given predicate.
    let inline forall p (v: #Vector<_>) = v.Enumerate() |> Seq.forall p

    /// Checks whether all entries in the vector that satisfies a given position dependent predicate.
    let inline foralli p (v: #Vector<_>) = v.EnumerateIndexed() |> Seq.forall (fun (i,x) -> p i x)

    /// Checks whether all non-zero entries in the vector that satisfies a given predicate. Skipping zeros is efficient on sparse data.
    let inline forallSkipZeros p (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.forall p

    /// Checks whether all non-zero entries in the vector that satisfies a given position dependent predicate. Skipping zeros is efficient on sparse data.
    let inline foralliSkipZeros p (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.forall (fun (i,x) -> p i x)



    /// In-place mutation by applying a function to every element of the vector.
    let inline mapInPlace f (v: #Vector<_>) =
        v.MapInplace((fun x -> f x), true)

    /// In-place mutation by applying a function to every element of the vector.
    let inline mapiInPlace f (v: #Vector<_>) =
        v.MapIndexedInplace((fun i x -> f i x), true)

    /// In-place mutation by applying a function to every element of the vector.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapSkipZerosInPlace f (v: #Vector<_>) =
        v.MapInplace((fun x -> f x), false)

    /// In-place mutation by applying a function to every element of the vector.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapiSkipZerosInPlace (f: int -> float -> float) (v: #Vector<float>) =
        v.MapIndexedInplace((fun i x -> f i x), false)


    /// Maps a vector to a new vector by applying a function to every element.
    let inline map f (v: #Vector<_>) =
        let w = v.Clone()
        w.MapInplace((fun x -> f x), true)
        w

    /// Maps a vector to a new vector by applying a function to every element.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapSkipZeros f (v: #Vector<_>) =
        let w = v.Clone()
        w.MapInplace((fun x -> f x), false)
        w

    /// Maps a vector to a new vector by applying a function to every element.
    let inline mapi f (v: #Vector<_>) =
        let w = v.Clone()
        w.MapIndexedInplace((fun i x -> f i x), true)
        w

    /// Maps a vector to a new vector by applying a function to every element.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapiSkipZeros f (v: #Vector<_>) =
        let w = v.Clone()
        w.MapIndexedInplace((fun i x -> f i x), false)
        w



    /// Fold all entries of a vector in reverse order.
    let inline foldBack f state (v: #Vector<_>) =
        let mutable acc = state
        for i=2 to v.Count do
            acc <- f (v.At (v.Count - i)) acc
        acc

    /// Reduces a vector in reverse order: the result of this function will be f(v[1], ..., f(v[n-2], f(v[n-1],v[n]))...).
    let inline reduceBack f (v: #Vector<_>) =
        let mutable p = v.Item(v.Count-1)
        for i=2 to v.Count do
            p <- f (v.At (v.Count - i)) p
        p

    /// Scans a vector in reverse order; like foldBack but returns the intermediate result.
    let inline scanBack f state (v: #Vector<_>) =
        seq {
            let rstate = ref state
            yield !rstate
            for i in v.Count-1..-1..0 do
                rstate := f (v.At(i)) !rstate
                yield !rstate
        }

    /// Creates a new vector and inserts the given value at the given index.
    let inline insert index value (v: #Vector<'T>) =
        let newV = Vector<'T>.Build.SameAs(v, v.Count + 1)
        v.CopySubVectorTo(newV, 0, 0, index)
        v.CopySubVectorTo(newV, index, index+1, v.Count - index)
        newV.At(index, value)
        newV



    /// In-place vector addition.
    let inline addInPlace (v: #Vector<_>) (w: #Vector<_>) = v.Add(w, v)

    /// In place vector subtraction.
    let inline subInPlace (v: #Vector<_>) (w: #Vector<_>) = v.Subtract(w, v)



/// A module which helps constructing generic dense vectors.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DenseVector =

    /// Create a vector that directly binds to a storage object.
    let inline ofStorage (storage: Storage.DenseVectorStorage<'T>) = Vector<'T>.Build.Dense(storage)

    /// Create a vector that directly binds to a raw storage array, without copying.
    let inline raw (raw: 'T[]) = Vector<'T>.Build.Dense(raw)

    /// Initialize an all-zero vector with the given dimension.
    let inline zero (n: int) = Vector<'T>.Build.Dense(n)

    /// Initialize a random vector with the given dimension and distribution.
    let inline random (n: int) dist = Vector<'T>.Build.Random(n, dist)

    /// Initialize an x-valued vector with the given dimension.
    let inline create (n: int) (x: 'T) = Vector<'T>.Build.Dense(n, x)

    /// Initialize a vector by calling a construction function for every element.
    let inline init (n: int) (f: int -> 'T) = Vector<'T>.Build.Dense(n, f)

    /// Create a vector from a float array (by copying - use raw instead if no copy is needed).
    let inline ofArray (fa: 'T array) = Vector<'T>.Build.Dense(Array.copy fa)

    /// Create a vector from a float list.
    let inline ofList (fl: 'T list) = Vector<'T>.Build.Dense(Array.ofList fl)

    /// Create a vector from a float sequence.
    let inline ofSeq (fs: #seq<'T>) = Vector<'T>.Build.DenseOfEnumerable(fs)

    /// Create a vector with a given dimension from an indexed list of index, value pairs.
    let inline ofListi (n: int) (fl: list<int * 'T>) = Vector<'T>.Build.DenseOfIndexed(n, Seq.ofList fl)

    /// Create a vector with a given dimension from an indexed sequences of index, value pairs.
    let inline ofSeqi (n: int) (fs: #seq<int * 'T>) = Vector<'T>.Build.DenseOfIndexed(n, fs)

    /// Create a vector with integer entries in the given range.
    let inline range (start: int) (step: int) (stop: int) = raw [| for i in start..step..stop -> float i |]

    /// Create a vector with evenly spaced entries: e.g. rangef -1.0 0.5 1.0 = [-1.0 -0.5 0.0 0.5 1.0]
    let inline rangef (start: float) (step: float) (stop: float) = raw [| start..step..stop |]


/// A module which helps constructing generic sparse vectors.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SparseVector =

    /// Create a vector that directly binds to a storage object.
    let inline ofStorage (storage: Storage.SparseVectorStorage<'T>) = Vector<'T>.Build.Sparse(storage)

    /// Initialize an all-zero vector with the given dimension.
    let inline zero (n: int) = Vector<'T>.Build.Sparse(n)

    /// Initialize an x-valued vector with the given dimension.
    let inline create (n: int) (x: 'T) = Vector<'T>.Build.Sparse(n, x)

    /// Initialize a vector by calling a construction function for every element.
    let inline init (n: int) (f: int -> 'T) = Vector<'T>.Build.Sparse(n, f)

    /// Create a sparse vector from a float array.
    let inline ofArray (fa: 'T array) = Vector<'T>.Build.SparseOfArray(fa)

    /// Create a sparse vector from a float list.
    let inline ofList (fl: 'T list) = Vector<'T>.Build.SparseOfEnumerable(Seq.ofList fl)

    /// Create a sparse vector from a float sequence.
    let inline ofSeq (fs: #seq<'T>) = Vector<'T>.Build.SparseOfEnumerable(fs)

    /// Create a sparse vector with a given dimension from an indexed list of index, value pairs.
    let inline ofListi (n: int) (fl: list<int * 'T>) = Vector<'T>.Build.SparseOfIndexed(n, Seq.ofList fl)

    /// Create a sparse vector with a given dimension from an indexed sequence of index, value pairs.
    let inline ofSeqi (n: int) (fs: #seq<int * 'T>) = Vector<'T>.Build.SparseOfIndexed(n, fs)


/// Module that contains implementation of useful F#-specific extension members for generic vectors
[<AutoOpen>]
module VectorExtensions =

    /// Construct a dense vector from a list of floating point numbers.
    let inline vector (lst: list<'T>) = DenseVector.ofList lst

    // A type extension for the generic vector type that
    // adds the 'GetSlice' method to allow vec.[a .. b] syntax
    type MathNet.Numerics.LinearAlgebra.
        Vector<'T when 'T : struct and 'T : (new : unit -> 'T)
                    and 'T :> System.IEquatable<'T> and 'T :> System.IFormattable
                    and 'T :> System.ValueType> with

        /// Gets a slice of a vector starting at a specified index
        /// and ending at a specified index (both indices are optional)
        /// This method can be used via the x.[start .. finish] syntax
        member x.GetSlice(start, finish) =
            let start = defaultArg start 0
            let finish = defaultArg finish (x.Count - 1)
            x.SubVector(start, finish - start + 1)

        /// Sets a slice of a vector starting at a specified index
        /// and ending at a specified index (both indices are optional)
        /// This method can be used via the x.[start .. finish] <- v syntax
        member x.SetSlice(start, finish, values) =
            let start = defaultArg start 0
            let finish = defaultArg finish (x.Count - 1)
            x.SetSubVector(start, finish - start + 1, values)
