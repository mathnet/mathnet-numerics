// <copyright file="AssertHelpers.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
#if NOSYSNUMERICS
    using Complex64 = Numerics.Complex;
#else
    using Complex64 = System.Numerics.Complex;
#endif

    /// <summary>
    /// Matrix utility functions to simplify tests.
    /// </summary>
    static public class MatrixHelpers
    {
        /// <summary>
        /// Forces a matrix elements to symmetric. Copies the lower triangle to the upper triangle.
        /// </summary>
        /// <typeparam name="T">The matrix type.</typeparam>
        /// <param name="matrix">The matrix to make symmetric.</param>
        static public void ForceSymmetric<T>(Matrix<T> matrix) where T : struct, IEquatable<T>, IFormattable
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("matrix must be square.", "matrix");
            }
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < row; column++)
                {
                    matrix.At(column, row, matrix.At(row, column));
                }
            }
        }

        /// <summary>
        /// Forces a matrix elements to hermitian (conjugate symmetric). Copies the conjugate of the values
        /// from the lower triangle to the upper triangle.
        /// </summary>
        /// <param name="matrix">The matrix to make conjugate symmetric.</param>
        static public void ForceHermitian(Matrix<Complex64> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("matrix must be square.", "matrix");
            }
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < row; column++)
                {
                    matrix.At(column, row, matrix.At(row, column).Conjugate());
                }
            }
        }

        /// <summary>
        /// Forces a matrix elements to conjugate symmetric. Copies the conjugate of the values
        /// from the lower triangle to the upper triangle.
        /// </summary>
        /// <param name="matrix">The matrix to make conjugate symmetric.</param>
        public static void ForceHermitian(Matrix<Numerics.Complex32> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("matrix must be square.", "matrix");
            }
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < row; column++)
                {
                    matrix.At(column, row, matrix.At(row, column).Conjugate());
                }
            }
        }
    }
}
