// <copyright file="LinearAlgebra.Double.Vector.fs" company="Math.NET">
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

/// A module which implements functional vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Vector =

    /// Transform a vector into an array.
    let inline toArray (v: #Vector<float>) = v.ToArray()

    /// Transform a vector into a list.
    let inline toList (v: #Vector<float>) = List.init v.Count v.At

    /// In-place mutation by applying a function to every element of the vector.
    let inline mapInPlace (f: float -> float) (v: #Vector<float>) =
        v.MapInplace((fun x -> f x), true)

    /// In-place mutation by applying a function to every element of the vector.
    let inline mapiInPlace (f: int -> float -> float) (v: #Vector<float>) =
        v.MapIndexedInplace((fun i x -> f i x), true)

    /// In-place mutation by applying a function to every element of the vector.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapnzInPlace (f: float -> float) (v: #Vector<float>) =
        v.MapInplace((fun x -> f x), false)

    /// In-place mutation by applying a function to every element of the vector.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapinzInPlace (f: int -> float -> float) (v: #Vector<float>) =
        v.MapIndexedInplace((fun i x -> f i x), false)

    /// Maps a vector to a new vector by applying a function to every element.
    let inline map f (v: #Vector<float>) =
        let w = v.Clone()
        w.MapInplace((fun x -> f x), true)
        w

    /// Maps a vector to a new vector by applying a function to every element.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapnz f (v: #Vector<float>) =
        let w = v.Clone()
        w.MapInplace((fun x -> f x), false)
        w

    /// Maps a vector to a new vector by applying a function to every element.
    let inline mapi (f: int -> float -> float) (v: #Vector<float>) =
        let w = v.Clone()
        w.MapIndexedInplace((fun i x -> f i x), true)
        w

    /// Maps a vector to a new vector by applying a function to every element.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapinz (f: int -> float -> float) (v: #Vector<float>) =
        let w = v.Clone()
        w.MapIndexedInplace((fun i x -> f i x), false)
        w

    /// In-place vector addition.
    let inline addInPlace (v: #Vector<float>) (w: #Vector<float>) = v.Add(w, v)

    /// In place vector subtraction.
    let inline subInPlace (v: #Vector<float>) (w: #Vector<float>) = v.Subtract(w, v)

    /// Applies a function to all elements of the vector.
    let inline iter (f: float -> unit) (v: #Vector<float>) =
        for i=0 to v.Count-1 do
            f (v.At i)

    /// Applies a function to all elements of the vector.
    let inline iteri (f: int -> float -> unit) (v: #Vector<float>) =
        for i=0 to v.Count-1 do
            f i (v.At i)


    /// Fold all entries of a vector.
    let inline fold (f: 'a -> float -> 'a) (acc0: 'a) (v: #Vector<float>) =
        let mutable acc = acc0
        for i=0 to v.Count-1 do
            acc <- f acc (v.At i)
        acc

    /// Fold all entries of a vector in reverse order.
    let inline foldBack (f: float -> 'a -> 'a) (acc0: 'a) (v: #Vector<float>) =
        let mutable acc = acc0
        for i=2 to v.Count do
            acc <- f (v.At (v.Count - i)) acc
        acc

    /// Fold all entries of a vector using a position dependent folding function.
    let inline foldi (f: int -> 'a -> float -> 'a) (acc0: 'a) (v: #Vector<float>) =
        let mutable acc = acc0
        for i=0 to v.Count-1 do
            acc <- f i acc (v.At i)
        acc

    /// Checks whether a predicate is satisfied for every element in the vector.
    let inline forall (p: float -> bool) (v: #Vector<float>) =
        let mutable b = true
        let mutable i = 0
        while b && i < v.Count do
            b <- b && (p (v.At i))
            i <- i+1
        b

    /// Checks whether there is an entry in the vector that satisfies a given predicate.
    let inline exists (p: float -> bool) (v: #Vector<float>) =
        let mutable b = false
        let mutable i = 0
        while not(b) && i < v.Count do
            b <- b || (p (v.At i))
            i <- i+1
        b

    /// Checks whether a predicate is true for all entries in a vector.
    let inline foralli (p: int -> float -> bool) (v: #Vector<float>) =
        let mutable b = true
        let mutable i = 0
        while b && i < v.Count do
            b <- b && (p i (v.At i))
            i <- i+1
        b

    /// Checks whether there is an entry in the vector that satisfies a given position dependent predicate.
    let inline existsi (p: int -> float -> bool) (v: #Vector<float>) =
        let mutable b = false
        let mutable i = 0
        while not(b) && i < v.Count do
            b <- b || (p i (v.At i))
            i <- i+1
        b

    /// Scans a vector; like fold but returns the intermediate result.
    let inline scan (f: float -> float -> float) (v: #Vector<float>) =
        let w = v.Clone()
        let mutable p = v.Item(0)
        for i=1 to v.Count-1 do
            p <- f p (v.At i)
            w.At(i, p)
        w

    /// Scans a vector in reverse order; like foldBack but returns the intermediate result.
    let inline scanBack (f: float -> float -> float) (v: #Vector<float>) =
        let w = v.Clone()
        let mutable p = v.At (v.Count-1)
        for i=2 to v.Count do
            p <- f (v.At (v.Count - i)) p
            w.At(v.Count - i, p)
        w

    /// Reduces a vector: the result of this function will be f(...f(f(v[0],v[1]), v[2]),..., v[n]).
    let inline reduce (f: float -> float -> float) (v: #Vector<float>) =
        let mutable p = v.Item(0)
        for i=1 to v.Count-1 do
            p <- f p (v.At i)
        p

    /// Reduces a vector in reverse order: the result of this function will be f(v[1], ..., f(v[n-2], f(v[n-1],v[n]))...).
    let inline reduceBack (f: float -> float -> float) (v: #Vector<float>) =
        let mutable p = v.Item(v.Count-1)
        for i=2 to v.Count do
            p <- f (v.At (v.Count - i)) p
        p

    /// Creates a new vector and inserts the given value at the given index.
    let inline insert index value (v: #Vector<float>) =
        let newV = new DenseVector(v.Count + 1)
        for i = 0 to index - 1 do
            newV.At(i, v.At i)
        newV.At(index, value)
        for i = index + 1 to v.Count do
            newV.At(i, v.At (i - 1))
        newV

/// A module which implements functional dense vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DenseVector =

    /// Create a vector that directly binds to a raw storage array, without copying.
    let inline raw (raw: float[]) = DenseVector(raw)

    /// Initialize an all-zero vector with the given dimension.
    let inline zeroCreate (n: int) = DenseVector(n)

    /// Initialize a random vector with the given dimension and distribution.
    let inline randomCreate (n: int) dist = DenseVector.CreateRandom(n, dist)

    /// Initialize an x-valued vector with the given dimension.
    let inline create (n: int) x = DenseVector.Create(n, fun i -> x)

    /// Initialize a vector by calling a construction function for every element.
    let inline init (n: int) (f: int -> float) = DenseVector.Create(n, fun i -> f i)

    /// Create a vector from a float list.
    let inline ofList (fl: float list) = DenseVector(Array.ofList fl)

    /// Create a vector from a sequences.
    let inline ofSeq (fs: #seq<float>) = DenseVector.OfEnumerable(fs)

    /// Create a vector with a given dimension from an indexed list of index, value pairs.
    let inline ofListi (n: int) (fl: list<int * float>) = DenseVector.OfIndexedEnumerable(n, Seq.ofList fl)

    /// Create a vector with a given dimension from an indexed sequences of index, value pairs.
    let inline ofSeqi (n: int) (fs: #seq<int * float>) = DenseVector.OfIndexedEnumerable(n, fs)

    /// Create a vector with evenly spaced entries: e.g. rangef -1.0 0.5 1.0 = [-1.0 -0.5 0.0 0.5 1.0]
    let inline rangef (start: float) (step: float) (stop: float) =
        let n = (int ((stop - start) / step)) + 1
        let v = new DenseVector(n)
        for i=0 to n-1 do
            v.At(i, (float i) * step + start)
        v

    /// Create a vector with integer entries in the given range.
    let inline range (start: int) (stop: int) =
        new DenseVector([| for i in [start .. stop] -> float i |])

/// A module which implements functional sparse vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SparseVector =

    /// Initialize an all-zero vector with the given dimension.
    let inline zeroCreate (n: int) = SparseVector(n)

    /// Initialize a vector by calling a construction function for every element.
    let inline init (n: int) (f: int -> float) = SparseVector.Create(n, fun i -> f i)

    /// Create a sparse vector with a given dimension from a list of index, value pairs.
    [<System.ObsoleteAttribute("Use ofListi instead. Will be changed to expect a non-indexed list in a future version.")>]
    let inline ofList (n: int) (fl: list<int * float>) = SparseVector.OfIndexedEnumerable(n, Seq.ofList fl)

    /// Create a sparse vector with a given dimension from a sequence of index, value pairs.
    [<System.ObsoleteAttribute("Use ofSeqi instead. Will be changed to expect a non-indexed seq in a future version.")>]
    let inline ofSeq (n: int) (fs: #seq<int * float>) = SparseVector.OfIndexedEnumerable(n, fs)

    /// Create a sparse vector with a given dimension from an indexed list of index, value pairs.
    let inline ofListi (n: int) (fl: list<int * float>) = SparseVector.OfIndexedEnumerable(n, Seq.ofList fl)

    /// Create a sparse vector with a given dimension from an indexed sequence of index, value pairs.
    let inline ofSeqi (n: int) (fs: #seq<int * float>) = SparseVector.OfIndexedEnumerable(n, fs)
