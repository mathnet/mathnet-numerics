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

/// Module that contains implementation of useful F#-specific extension members for generic vectors
[<AutoOpen>]
module VectorExtensions =

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
    let inline toSeqnz (v: #Vector<_>) = v.EnumerateNonZero()

    /// Transform a vector into an indexed sequence where zero-values are skipped. Skipping zeros is efficient on sparse data.
    let inline toSeqinz (v: #Vector<_>) = v.EnumerateNonZeroIndexed()


    /// Applies a function to all elements of the vector.
    let inline iter f (v: #Vector<_>) = v.Enumerate() |> Seq.iter f

    /// Applies a function to all indexed elements of the vector.
    let inline iteri f (v: #Vector<_>) = v.Enumerate() |> Seq.iteri f

    /// Applies a function to all non-zero elements of the vector. Skipping zeros is efficient on sparse data.
    let inline iternz f (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.iter f

    /// Applies a function to all non-zero indexed elements of the vector. Skipping zeros is efficient on sparse data.
    let inline iterinz f (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.iter (fun (i,x) -> f i x)


    /// Fold all entries of a vector.
    let inline fold f state (v: #Vector<_>) = v.Enumerate() |> Seq.fold f state

    /// Fold all entries of a vector using a position dependent folding function.
    let inline foldi f state (v: #Vector<_>) = v.EnumerateIndexed() |> Seq.fold (fun s (i,x) -> f i s x) state

    /// Fold all non-zero entries of a vector. Skipping zeros is efficient on sparse data.
    let inline foldnz f state (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.fold f state

    /// Fold all non-zero entries of a vector using a position dependent folding function. Skipping zeros is efficient on sparse data.
    let inline foldinz f state (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.fold (fun s (i,x) -> f i s x) state


    /// Scan all entries of a vector.
    let inline scan f state (v: #Vector<_>) = v.Enumerate() |> Seq.scan f state

    /// Scan all entries of a vector using a position dependent folding function.
    let inline scani f state (v: #Vector<_>) = v.EnumerateIndexed() |> Seq.scan (fun s (i,x) -> f i s x) state

    /// Scan all non-zero entries of a vector. Skipping zeros is efficient on sparse data.
    let inline scannz f state (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.scan f state

    /// Scan all non-zero entries of a vector using a position dependent folding function. Skipping zeros is efficient on sparse data.
    let inline scaninz f state (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.scan (fun s (i,x) -> f i s x) state


    /// Reduce all entries of a vector.
    let inline reduce f (v: #Vector<_>) = v.Enumerate() |> Seq.reduce f

    /// Reduce all non-zero entries of a vector. Skipping zeros is efficient on sparse data.
    let inline reducenz f (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.reduce f


    /// Checks whether there is an entry in the vector that satisfies a predicate.
    let inline exists p (v: #Vector<_>) = v.Enumerate() |> Seq.exists p

    /// Checks whether there is an entry in the vector that satisfies a position dependent predicate.
    let inline existsi p (v: #Vector<_>) = v.EnumerateIndexed() |> Seq.exists (fun (i,x) -> p i x)

    /// Checks whether there is a non-zero entry in the vector that satisfies a predicate. Skipping zeros is efficient on sparse data.
    let inline existsnz p (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.exists p

    /// Checks whether there is a non-zero entry in the vector that satisfies a position dependent predicate. Skipping zeros is efficient on sparse data.
    let inline existsinz p (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.exists (fun (i,x) -> p i x)


    /// Checks whether all entries in the vector that satisfies a given predicate.
    let inline forall p (v: #Vector<_>) = v.Enumerate() |> Seq.forall p

    /// Checks whether all entries in the vector that satisfies a given position dependent predicate.
    let inline foralli p (v: #Vector<_>) = v.EnumerateIndexed() |> Seq.forall (fun (i,x) -> p i x)

    /// Checks whether all non-zero entries in the vector that satisfies a given predicate. Skipping zeros is efficient on sparse data.
    let inline forallnz p (v: #Vector<_>) = v.EnumerateNonZero() |> Seq.forall p

    /// Checks whether all non-zero entries in the vector that satisfies a given position dependent predicate. Skipping zeros is efficient on sparse data.
    let inline forallinz p (v: #Vector<_>) = v.EnumerateNonZeroIndexed() |> Seq.forall (fun (i,x) -> p i x)



    /// In-place mutation by applying a function to every element of the vector.
    let inline mapInPlace f (v: #Vector<_>) =
        v.MapInplace((fun x -> f x), true)

    /// In-place mutation by applying a function to every element of the vector.
    let inline mapiInPlace f (v: #Vector<_>) =
        v.MapIndexedInplace((fun i x -> f i x), true)

    /// In-place mutation by applying a function to every element of the vector.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapnzInPlace f (v: #Vector<_>) =
        v.MapInplace((fun x -> f x), false)

    /// In-place mutation by applying a function to every element of the vector.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapinzInPlace (f: int -> float -> float) (v: #Vector<float>) =
        v.MapIndexedInplace((fun i x -> f i x), false)


    /// Maps a vector to a new vector by applying a function to every element.
    let inline map f (v: #Vector<_>) =
        let w = v.Clone()
        w.MapInplace((fun x -> f x), true)
        w

    /// Maps a vector to a new vector by applying a function to every element.
    /// Zero-values may be skipped (relevant mostly for sparse vectors).
    let inline mapnz f (v: #Vector<_>) =
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
    let inline mapinz f (v: #Vector<_>) =
        let w = v.Clone()
        w.MapIndexedInplace((fun i x -> f i x), false)
        w



    /// In-place vector addition.
    let inline addInPlace (v: #Vector<_>) (w: #Vector<_>) = v.Add(w, v)

    /// In place vector subtraction.
    let inline subInPlace (v: #Vector<_>) (w: #Vector<_>) = v.Subtract(w, v)

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
