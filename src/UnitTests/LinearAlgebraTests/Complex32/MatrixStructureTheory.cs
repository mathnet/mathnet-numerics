// <copyright file="MatrixStructureTheory.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using Numerics;

    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<Complex32>
    {
        [Datapoints]
        Matrix<Complex32>[] _matrices = new Matrix<Complex32>[]
        {
            Matrix<Complex32>.Build.DenseOfArray(new[,] { { 1f, new Complex32(1.1f, -4f), 2f }, { 1f, 1f, 2f }, { 1f, new Complex32(1f, 2f), 2f } }),
            Matrix<Complex32>.Build.DenseOfArray(new[,] { { -1.1f, -2.2f, -3.3f }, { 0f, 1.1f, new Complex32(2.2f, -1.2f) }, { -4.4f, 5.5f, 6.6f } }),
            Matrix<Complex32>.Build.DenseOfArray(new[,] { { new Complex32(-1.1f, -2f), -2.2f, -3.3f, -4.4f }, { 0f, 1.1f, 2.2f, 3.3f }, { 1f, 2.1f, 6.2f, 4.3f }, { -4.4f, 5.5f, 6.6f, -7.7f } }),
            Matrix<Complex32>.Build.DenseOfArray(new[,] { { -1.1f, new Complex32(-2.2f, 3.4f), -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f } }),
            Matrix<Complex32>.Build.DenseOfArray(new[,] { { -1.1f, -2.2f }, { Complex32.Zero, 1.1f }, { -4.4f, 5.5f } }),
            Matrix<Complex32>.Build.DenseOfArray(new[,] { { -1.1f, -2.2f, -3.3f }, { 0f, new Complex32(1.1f, 0.1f), 2.2f } }),
            Matrix<Complex32>.Build.DenseOfArray(new[,] { { 1f, 2f, 3f }, { 2f, new Complex32(2f, 2f), 0f }, { 3f, Complex32.Zero, 3f } }),

            Matrix<Complex32>.Build.SparseOfArray(new[,] { { 7f, 1f, 2f }, { 1f, 1f, 2f }, { 1f, 1f + Complex32.ImaginaryOne, 2f } }),
            Matrix<Complex32>.Build.SparseOfArray(new[,] { { 7f, 1f, 2f }, { new Complex32(1f, 2f), 0f, Complex32.Zero }, { -2f, 0f, 0f } }),
            Matrix<Complex32>.Build.SparseOfArray(new[,] { { -1.1f, 0f, 0f }, { 0f, new Complex32(1.1f, 2f), 2.2f } }),

            Matrix<Complex32>.Build.Diagonal(3, 3, new[] { new Complex32(1f, 1f), -2f, 1.5f }),
            Matrix<Complex32>.Build.Diagonal(3, 3, new[] { new Complex32(1f, 2f), 0f, -1.5f }),

            new UserDefinedMatrix(new[,] { { 0f, 1f, 2f }, { -1f, 7.7f, 0f }, { -2f, Complex32.Zero, 0f } })
        };

        [Datapoints]
        Complex32[] scalars = new[] { new Complex32(2f, 0f), new Complex32(-1.5f, 3.5f), Complex32.Zero };
    }
}
