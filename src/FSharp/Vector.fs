// <copyright file="Vector.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009 Math.NET
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
open MathNet.Numerics.LinearAlgebra.Generic
/// A module which implements functional vector operations.
module Vector =
        
    /// Transform a vector into an array.
    let inline toArray (v: #Vector<float>) =
        let n = v.Count
        Array.init n (fun i -> v.Item(i))
        
    /// Transform a vector into an array.
    let inline toList (v: #Vector<float>) =
        let n = v.Count
        List.init n (fun i -> v.Item(i))

    /// In-place mutation by applying a function to every element of the vector.
    let inline mapInPlace (f: float -> float) (v: #Vector<float>) =
        for i=0 to v.Count-1 do
            v.Item(i) <- f (v.Item(i))
        ()

    /// In-place mutation by applying a function to every element of the vector.
    let inline mapiInPlace (f: int -> float -> float) (v: #Vector<float>) =
        for i=0 to v.Count-1 do
            v.Item(i) <- f i (v.Item(i))
        ()
        
    /// In-place vector addition.
    let inline addInPlace (v: #Vector<float>) (w: #Vector<float>) = v.Add(w, v)
        
    /// In place vector subtraction.
    let inline subInPlace (v: #Vector<float>) (w: #Vector<float>) = v.Subtract(w, v)
    
    /// Functional map operator for vectors.
    /// <include file='../../../../FSharpExamples/DenseVector.xml' path='example'/> 
    let inline map f (v: #Vector<float>) =
        let w = v.Clone()
        mapInPlace (fun x -> f x) w
        w
        
    /// Applies a function to all elements of the vector.
    let inline iter (f: float -> unit) (v: #Vector<float>) =
        for i=0 to v.Count-1 do
            f (v.Item i)
        
    /// Applies a function to all elements of the vector.
    let inline iteri (f: int -> float -> unit) (v: #Vector<float>) =
        for i=0 to v.Count-1 do
            f i (v.Item i)
            
    /// Maps a vector to a new vector by applying a function to every element.
    let inline mapi (f: int -> float -> float) (v: #Vector<float>) =
        let w = v.Clone()
        mapiInPlace f w
        w

    /// Fold all entries of a vector.
    let inline fold (f: 'a -> float -> 'a) (acc0: 'a) (v: #Vector<float>) =
        let mutable acc = acc0
        for i=0 to v.Count-1 do
            acc <- f acc (v.Item(i))
        acc

    /// Fold all entries of a vector in reverse order.
    let inline foldBack (f: float -> 'a -> 'a) (acc0: 'a) (v: #Vector<float>) =
        let mutable acc = acc0
        for i=2 to v.Count do
            acc <- f (v.Item(v.Count - i)) acc
        acc

    /// Fold all entries of a vector using a position dependent folding function.
    let inline foldi (f: int -> 'a -> float -> 'a) (acc0: 'a) (v: #Vector<float>) =
        let mutable acc = acc0
        for i=0 to v.Count-1 do
            acc <- f i acc (v.Item(i))
        acc
        
    /// Checks whether a predicate is satisfied for every element in the vector.
    let inline forall (p: float -> bool) (v: #Vector<float>) =
        let mutable b = true
        let mutable i = 0
        while b && i < v.Count do
            b <- b && (p (v.Item(i)))
            i <- i+1
        b
    
    /// Checks whether there is an entry in the vector that satisfies a given predicate.
    let inline exists (p: float -> bool) (v: #Vector<float>) =
        let mutable b = false
        let mutable i = 0
        while not(b) && i < v.Count do
            b <- b || (p (v.Item(i)))
            i <- i+1
        b
    
    /// Checks whether a predicate is true for all entries in a vector.
    let inline foralli (p: int -> float -> bool) (v: #Vector<float>) =
        let mutable b = true
        let mutable i = 0
        while b && i < v.Count do
            b <- b && (p i (v.Item(i)))
            i <- i+1
        b
    
    /// Checks whether there is an entry in the vector that satisfies a given position dependent predicate.
    let inline existsi (p: int -> float -> bool) (v: #Vector<float>) =
        let mutable b = false
        let mutable i = 0
        while not(b) && i < v.Count do
            b <- b || (p i (v.Item(i)))
            i <- i+1
        b

    /// Scans a vector; like fold but returns the intermediate result.
    let inline scan (f: float -> float -> float) (v: #Vector<float>) =
        let w = v.Clone()
        let mutable p = v.Item(0)
        for i=1 to v.Count-1 do
            p <- f p (v.Item(i))
            w.[i] <- p
        w

    /// Scans a vector in reverse order; like foldBack but returns the intermediate result.
    let inline scanBack (f: float -> float -> float) (v: #Vector<float>) =
        let w = v.Clone()
        let mutable p = v.Item(v.Count-1)
        for i=2 to v.Count do
            p <- f (v.Item(v.Count - i)) p
            w.[v.Count - i] <- p
        w

    /// Reduces a vector: the result of this function will be f(...f(f(v[0],v[1]), v[2]),..., v[n]).
    let inline reduce (f: float -> float -> float) (v: #Vector<float>) =
        let mutable p = v.Item(0)
        for i=1 to v.Count-1 do
            p <- f p (v.Item(i))
        p

    /// Reduces a vector in reverse order: the result of this function will be f(v[1], ..., f(v[n-2], f(v[n-1],v[n]))...).
    let inline reduceBack (f: float -> float -> float) (v: #Vector<float>) =
        let mutable p = v.Item(v.Count-1)
        for i=2 to v.Count do
            p <- f (v.Item(v.Count - i)) p
        p