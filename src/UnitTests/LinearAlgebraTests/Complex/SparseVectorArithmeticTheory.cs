// <copyright file="SparseVectorArithmeticTheory.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
    using System.Numerics;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class SparseVectorArithmeticTheory : VectorArithmeticTheory
    {
        [Datapoints]
        Vector<Complex>[] denseVectors = new Vector<Complex>[]
            {
                SparseVector.OfEnumerable(new[] {new Complex(1, 1), new Complex(2, 1), new Complex(3, 1), new Complex(4, 1), new Complex(5, 1)}),
                SparseVector.OfEnumerable(new[] {new Complex(2, -1), new Complex(0, 0), new Complex(0, 2), new Complex(-5, 1), new Complex(0, 0)}),
                new SparseVector(5),
                new SparseVector(int.MaxValue)
            };

        [Datapoints]
        Complex[] scalars = new[] {new Complex(2d, -1d)};
    }
}
