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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    [TestFixture, Category("LA")]
    public class MatrixStructureTheory : MatrixStructureTheory<Complex>
    {
        [Datapoints]
        Matrix<Complex>[] _matrices =
        {
            Matrix<Complex>.Build.DenseOfArray(new[,] { { 1d, new Complex(1.1d, -4d), 2d }, { 1d, 1d, 2d }, { 1d, new Complex(1d, 2d), 2d } }),
            Matrix<Complex>.Build.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d }, { 0d, 1.1d, new Complex(2.2d, -1.2d) }, { -4.4d, 5.5d, 6.6d } }),
            Matrix<Complex>.Build.DenseOfArray(new[,] { { new Complex(-1.1d, -2d), -2.2d, -3.3d, -4.4d }, { 0d, 1.1d, 2.2d, 3.3d }, { 1d, 2.1d, 6.2d, 4.3d }, { -4.4d, 5.5d, 6.6d, -7.7d } }),
            Matrix<Complex>.Build.DenseOfArray(new[,] { { -1.1d, new Complex(-2.2d, 3.4d), -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d } }),
            Matrix<Complex>.Build.DenseOfArray(new[,] { { -1.1d, -2.2d }, { Complex.Zero, 1.1d }, { -4.4d, 5.5d } }),
            Matrix<Complex>.Build.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d }, { 0d, new Complex(1.1d, 0.1d), 2.2d } }),
            Matrix<Complex>.Build.DenseOfArray(new[,] { { 1d, 2d, 3d }, { 2d, new Complex(2d, 2d), 0d }, { 3d, Complex.Zero, 3d } }),

            Matrix<Complex>.Build.SparseOfArray(new[,] { { 7d, 1d, 2d }, { 1d, 1d, 2d }, { 1d, 1d + Complex.ImaginaryOne, 2d } }),
            Matrix<Complex>.Build.SparseOfArray(new[,] { { 7d, 1d, 2d }, { new Complex(1d, 2d), 0d, Complex.Zero }, { -2d, 0d, 0d } }),
            Matrix<Complex>.Build.SparseOfArray(new[,] { { -1.1d, 0d, 0d }, { 0d, new Complex(1.1d, 2d), 2.2d } }),

            Matrix<Complex>.Build.Diagonal(3, 3, new[] { new Complex(1d, 1d), -2d, 1.5d }),
            Matrix<Complex>.Build.Diagonal(3, 3, new[] { new Complex(1d, 2d), 0d, -1.5d }),

            new UserDefinedMatrix(new[,] { { 0d, 1d, 2d }, { -1d, 7.7d, 0d }, { -2d, Complex.Zero, 0d } })
        };

        [Datapoints]
        Complex[] _scalars = { new Complex(2d, 0d), new Complex(-1.5d, 3.5d), Complex.Zero };
    }
}
