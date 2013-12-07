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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<double>
    {
        [Datapoints]
        Matrix<double>[] _matrices = new Matrix<double>[]
        {
            Matrix<double>.Build.DenseOfArray(new[,] { { 1d, 1d, 2d }, { 1d, 1d, 2d }, { 1d, 1d, 2d } }),
            Matrix<double>.Build.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d }, { 0d, 1.1d, 2.2d }, { -4.4d, 5.5d, 6.6d } }),
            Matrix<double>.Build.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d, -4.4d }, { 0d, 1.1d, 2.2d, 3.3d }, { 1d, 2.1d, 6.2d, 4.3d }, { -4.4d, 5.5d, 6.6d, -7.7d } }),
            Matrix<double>.Build.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d } }),
            Matrix<double>.Build.DenseOfArray(new[,] { { -1.1d, -2.2d }, { 0d, 1.1d }, { -4.4d, 5.5d } }),
            Matrix<double>.Build.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d }, { 0d, 1.1d, 2.2d } }),
            Matrix<double>.Build.DenseOfArray(new[,] { { 1d, 2d, 3d }, { 2d, 2d, 0d }, { 3d, 0d, 3d } }),

            Matrix<double>.Build.SparseOfArray(new[,] { { 7d, 1d, 2d }, { 1d, 1d, 2d }, { 1d, 1d, 2d } }),
            Matrix<double>.Build.SparseOfArray(new[,] { { 7d, 1d, 2d }, { 1d, 0d, 0d }, { -2d, 0d, 0d } }),
            Matrix<double>.Build.SparseOfArray(new[,] { { -1.1d, 0d, 0d }, { 0d, 1.1d, 2.2d } }),

            Matrix<double>.Build.Diagonal(3, 3, new[] { 1d, -2d, 1.5d }),
            Matrix<double>.Build.Diagonal(3, 3, new[] { 1d, 0d, -1.5d }),

            new UserDefinedMatrix(new[,] { { 0d, 1d, 2d }, { -1d, 7.7d, 0d }, { -2d, 0d, 0d } })
        };

        [Datapoints]
        double[] _scalars = new[] { 2d, -1.5d, 0d };
    }
}
