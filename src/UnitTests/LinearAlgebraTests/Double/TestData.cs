﻿// <copyright file="TestData.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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

using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    public static class TestData
    {
        static readonly MatrixBuilder<double> M = Matrix<double>.Build;
        static readonly VectorBuilder<double> V = Vector<double>.Build;

        public static Matrix<double> Matrix(TestMatrix matrix)
        {
            switch (matrix)
            {
                case TestMatrix.DenseSquare3x3: return M.DenseOfArray(new[,] { { 1d, 1d, 2d }, { 1d, 1d, 2d }, { 1d, 1d, 2d } });
                case TestMatrix.DenseSquare3x3b: return M.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d }, { 0d, 1.1d, 2.2d }, { -4.4d, 5.5d, 6.6d } });
                case TestMatrix.DenseSquare3x3c: return M.DenseOfArray(new[,] { { 1d, 2d, 3d }, { 2d, 2d, 0d }, { 3d, 0d, 3d } });
                case TestMatrix.DenseSquare4x4: return M.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d, -4.4d }, { 0d, 1.1d, 2.2d, 3.3d }, { 1d, 2.1d, 6.2d, 4.3d }, { -4.4d, 5.5d, 6.6d, -7.7d } });
                case TestMatrix.DenseSquare4x4b: return M.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d }, { -1.1d, -2.2d, -3.3d, -4.4d } });
                case TestMatrix.DenseTall3x2: return M.DenseOfArray(new[,] { { -1.1d, -2.2d }, { 0d, 1.1d }, { -4.4d, 5.5d } });
                case TestMatrix.DenseWide2x3: return M.DenseOfArray(new[,] { { -1.1d, -2.2d, -3.3d }, { 0d, 1.1d, 2.2d } });

                case TestMatrix.SparseSquare3x3: return M.SparseOfArray(new[,] { { 7d, 1d, 2d }, { 1d, 1d, 2d }, { 1d, 1d, 2d } });
                case TestMatrix.SparseSquare3x3b: return M.SparseOfArray(new[,] { { 7d, 1d, 2d }, { 1d, 0d, 0d }, { -2d, 0d, 0d } });
                case TestMatrix.SparseWide2x3: return M.SparseOfArray(new[,] { { -1.1d, 0d, 0d }, { 0d, 1.1d, 2.2d } });

                case TestMatrix.DiagonalSquare3x3: return M.Diagonal(3, 3, new[] { 1d, -2d, 1.5d });
                case TestMatrix.DiagonalSquare3x3b: return M.Diagonal(3, 3, new[] { 1d, 0d, -1.5d });

                case TestMatrix.UserSquare3x3: return new UserDefinedMatrix(new[,] { { 0d, 1d, 2d }, { -1d, 7.7d, 0d }, { -2d, 0d, 0d } });

                default: throw new NotSupportedException();
            }
        }

        public static Vector<double> Vector(TestVector vector)
        {
            switch (vector)
            {
                case TestVector.Dense5: return V.Dense(new double[] { 1, 2, 3, 4, 5 });
                case TestVector.Dense5WithZeros: return V.Dense(new double[] { 2, 0, 0, -5, 0 });

                case TestVector.Sparse5: return V.SparseOfEnumerable(new double[] { 1, 2, 3, 4, 5 });
                case TestVector.Sparse5WithZeros: return V.SparseOfEnumerable(new double[] { 2, 0, 0, -5, 0 });
                case TestVector.Sparse5AllZeros: return V.Sparse(5);
                case TestVector.SparseMaxLengthAllZeros: return V.Sparse(int.MaxValue);

                default: throw new NotSupportedException();
            }
        }
    }
}
