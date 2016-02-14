// <copyright file="Compatibility.fs" company="Math.NET">
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

namespace MathNet.Numerics

[<AutoOpen>]
module internal Compatibility =

#if NET35

    let inline internal properTuple2 (tuple: MathNet.Numerics.Tuple<'a,'b>) = tuple.Item1, tuple.Item2
    let inline internal properTuple3 (tuple: MathNet.Numerics.Tuple<'a,'b,'c>) = tuple.Item1, tuple.Item2, tuple.Item3
    let inline internal internalTuple2 ((a,b): ('a * 'b)) = MathNet.Numerics.Tuple<'a,'b>(a, b)
    let inline internal internalTuple3 ((a,b,c): ('a * 'b * 'c)) = MathNet.Numerics.Tuple<'a,'b,'c>(a, b, c)

    let inline internal properTuple2Seq x = x |> Seq.map properTuple2
    let inline internal properTuple3Seq x = x |> Seq.map properTuple3
    let inline internal internalTuple2Seq x = x |> Seq.map internalTuple2
    let inline internal internalTuple3Seq x = x |> Seq.map internalTuple3

#else

    let inline internal properTuple2 x = x
    let inline internal properTuple3 x = x
    let inline internal internalTuple2 x = x
    let inline internal internalTuple3 x = x

    let inline internal properTuple2Seq x = x
    let inline internal properTuple3Seq x = x
    let inline internal internalTuple2Seq x = x
    let inline internal internalTuple3Seq x = x

#endif
