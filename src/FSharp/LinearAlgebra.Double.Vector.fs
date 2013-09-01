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

open MathNet.Numerics.LinearAlgebra

/// A module which implements functional dense vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DenseVector =

    /// Create a vector that directly binds to a raw storage array, without copying.
    let inline raw (raw: float[]) = DenseVector(raw) :> _ Vector

    /// Initialize an all-zero vector with the given dimension.
    let inline zeroCreate (n: int) = DenseVector(n) :> _ Vector

    /// Initialize a random vector with the given dimension and distribution.
    let inline randomCreate (n: int) dist = DenseVector.CreateRandom(n, dist) :> _ Vector

    /// Initialize an x-valued vector with the given dimension.
    let inline create (n: int) (x: float) = DenseVector.Create(n, x) :> _ Vector

    /// Initialize a vector by calling a construction function for every element.
    let inline init (n: int) (f: int -> float) = DenseVector.Create(n, f) :> _ Vector

    /// Create a vector from a float array (by copying - use raw instead if no copy is needed).
    let inline ofArray (fl: float array) = DenseVector(Array.copy fl) :> _ Vector

    /// Create a vector from a float list.
    let inline ofList (fl: float list) = DenseVector(Array.ofList fl) :> _ Vector

    /// Create a vector from a float sequence.
    let inline ofSeq (fs: #seq<float>) = DenseVector.OfEnumerable(fs) :> _ Vector

    /// Create a vector with a given dimension from an indexed list of index, value pairs.
    let inline ofListi (n: int) (fl: list<int * float>) = DenseVector.OfIndexedEnumerable(n, Seq.ofList fl) :> _ Vector

    /// Create a vector with a given dimension from an indexed sequences of index, value pairs.
    let inline ofSeqi (n: int) (fs: #seq<int * float>) = DenseVector.OfIndexedEnumerable(n, fs) :> _ Vector

    /// Create a vector with integer entries in the given range.
    let inline range (start: int) (step: int) (stop: int) = raw [| for i in start..step..stop -> float i |]

    /// Create a vector with evenly spaced entries: e.g. rangef -1.0 0.5 1.0 = [-1.0 -0.5 0.0 0.5 1.0]
    let inline rangef (start: float) (step: float) (stop: float) = raw [| start..step..stop |]


/// A module which implements functional sparse vector operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SparseVector =

    /// Initialize an all-zero vector with the given dimension.
    let inline zeroCreate (n: int) = SparseVector(n) :> _ Vector

    /// Initialize an x-valued vector with the given dimension.
    let inline create (n: int) (x: float) = SparseVector.Create(n, x) :> _ Vector

    /// Initialize a vector by calling a construction function for every element.
    let inline init (n: int) (f: int -> float) = SparseVector.Create(n, f) :> _ Vector

    /// Create a sparse vector from a float array.
    let inline ofArray (fl: float array) = SparseVector.OfEnumerable(Seq.ofArray fl) :> _ Vector

    /// Create a sparse vector from a float list.
    let inline ofList (fl: float list) = SparseVector.OfEnumerable(Seq.ofList fl) :> _ Vector

    /// Create a sparse vector from a float sequence.
    let inline ofSeq (fs: #seq<float>) = SparseVector.OfEnumerable(fs) :> _ Vector

    /// Create a sparse vector with a given dimension from an indexed list of index, value pairs.
    let inline ofListi (n: int) (fl: list<int * float>) = SparseVector.OfIndexedEnumerable(n, Seq.ofList fl) :> _ Vector

    /// Create a sparse vector with a given dimension from an indexed sequence of index, value pairs.
    let inline ofSeqi (n: int) (fs: #seq<int * float>) = SparseVector.OfIndexedEnumerable(n, fs) :> _ Vector


/// A module which implements some F# utility functions.
[<AutoOpen>]
module VectorUtility =

    /// Construct a dense vector from a list of floating point numbers.
    let inline vector (lst: list<float>) = DenseVector.ofList lst
