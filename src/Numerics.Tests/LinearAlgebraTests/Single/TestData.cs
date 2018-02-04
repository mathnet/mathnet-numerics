// <copyright file="TestData.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    public static class TestData
    {
        static readonly MatrixBuilder<float> M = Matrix<float>.Build;
        static readonly VectorBuilder<float> V = Vector<float>.Build;

        public static Matrix<float> Matrix(TestMatrix matrix)
        {
            switch (matrix)
            {
                case TestMatrix.DenseSquare3x3: return M.DenseOfArray(new[,] { { 1f, 1f, 2f }, { 1f, 1f, 2f }, { 1f, 1f, 2f } });
                case TestMatrix.DenseSquare3x3b: return M.DenseOfArray(new[,] { { -1.1f, -2.2f, -3.3f }, { 0f, 1.1f, 2.2f }, { -4.4f, 5.5f, 6.6f } });
                case TestMatrix.DenseSquare3x3c: return M.DenseOfArray(new[,] { { 1f, 2f, 3f }, { 2f, 2f, 0f }, { 3f, 0f, 3f } });
                case TestMatrix.DenseSquare4x4: return M.DenseOfArray(new[,] { { -1.1f, -2.2f, -3.3f, -4.4f }, { 0f, 1.1f, 2.2f, 3.3f }, { 1f, 2.1f, 6.2f, 4.3f }, { -4.4f, 5.5f, 6.6f, -7.7f } });
                case TestMatrix.DenseSquare4x4b: return M.DenseOfArray(new[,] { { -1.1f, -2.2f, -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f } });
                case TestMatrix.DenseTall3x2: return M.DenseOfArray(new[,] { { -1.1f, -2.2f }, { 0f, 1.1f }, { -4.4f, 5.5f } });
                case TestMatrix.DenseWide2x3: return M.DenseOfArray(new[,] { { -1.1f, -2.2f, -3.3f }, { 0f, 1.1f, 2.2f } });

                case TestMatrix.SparseSquare3x3: return M.SparseOfArray(new[,] { { 7f, 1f, 2f }, { 1f, 1f, 2f }, { 1f, 1f, 2f } });
                case TestMatrix.SparseSquare3x3b: return M.SparseOfArray(new[,] { { 7f, 1f, 2f }, { 1f, 0f, 0f }, { -2f, 0f, 0f } });
                case TestMatrix.SparseWide2x3: return M.SparseOfArray(new[,] { { -1.1f, 0f, 0f }, { 0f, 1.1f, 2.2f } });

                case TestMatrix.DiagonalSquare3x3: return M.Diagonal(3, 3, new[] { 1f, -2f, 1.5f });
                case TestMatrix.DiagonalSquare3x3b: return M.Diagonal(3, 3, new[] { 1f, 0f, -1.5f });

                case TestMatrix.UserSquare3x3: return new UserDefinedMatrix(new[,]  { { 0f, 1f, 2f }, { -1f, 7.7f, 0f }, { -2f, 0f, 0f } });

                default: throw new NotSupportedException();
            }
        }

        public static Vector<float> Vector(TestVector vector)
        {
            switch (vector)
            {
                case TestVector.Dense5: return V.Dense(new float[] { 1, 2, 3, 4, 5 });
                case TestVector.Dense5WithZeros: return V.Dense(new float[] { 2, 0, 0, -5, 0 });

                case TestVector.Sparse5: return V.SparseOfEnumerable(new float[] { 1, 2, 3, 4, 5 });
                case TestVector.Sparse5WithZeros: return V.SparseOfEnumerable(new float[] { 2, 0, 0, -5, 0 });
                case TestVector.Sparse5AllZeros: return V.Sparse(5);
                case TestVector.SparseMaxLengthAllZeros: return V.Sparse(int.MaxValue);

                default: throw new NotSupportedException();
            }
        }
    }
}
