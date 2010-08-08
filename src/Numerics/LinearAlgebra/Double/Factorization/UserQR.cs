// <copyright file="UserQR.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.Double.Factorization
{
    using System;
    using System.Linq;
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A may be decomposed as A = QR where Q is an orthogonal matrix 
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R is an upper triangular matrix 
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// </remarks>
    public class UserQR : QR
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserQR"/> class. This object will compute the
        /// QR factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        public UserQR(Matrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount < matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            MatrixR = matrix.Clone();
            MatrixQ = matrix.CreateMatrix(matrix.RowCount, matrix.RowCount);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                MatrixQ.At(i, i, 1.0);
            }

            var minmn = Math.Min(matrix.RowCount, matrix.ColumnCount);
            var u = new double[minmn][];
            for (var i = 0; i < minmn; i++)
            {
                u[i] = GenerateColumn(MatrixR, i, matrix.RowCount - 1, i);
                ComputeQR(u[i], MatrixR, i, matrix.RowCount - 1, i + 1, matrix.ColumnCount - 1);
            }

            for (var i = minmn - 1; i >= 0; i--)
            {
                ComputeQR(u[i], MatrixQ, i, matrix.RowCount - 1, i, matrix.RowCount - 1);
            }
        }

        /// <summary>
        /// Generate column from initial matrix to work array
        /// </summary>
        /// <param name="a">Initial matrix</param>
        /// <param name="rowStart">The firts row</param>
        /// <param name="rowEnd">The last row</param>
        /// <param name="column">Column index</param>
        /// <returns>Generated vector</returns>
        private static double[] GenerateColumn(Matrix a, int rowStart, int rowEnd, int column)
        {
            var ru = rowEnd - rowStart + 1;
            var u = new double[ru];

            for (var i = rowStart; i <= rowEnd; i++)
            {
                u[i - rowStart] = a.At(i, rowStart);
                a.At(i, rowStart, 0.0);
            }

            var norm = u.Sum(t => t * t);
            norm = Math.Sqrt(norm);

            if (rowStart == rowEnd || norm == 0)
            {
                a.At(rowStart, column, -u[0]);
                u[0] = Math.Sqrt(2.0);
                return u;
            }

            var scale = 1.0 / norm;
            if (u[0] < 0.0)
            {
                scale *= -1.0;
            }

            a.At(rowStart, column, -1.0 / scale);

            for (var i = 0; i < ru; i++)
            {
                u[i] *= scale;
            }

            u[0] += 1.0;
            var s = Math.Sqrt(1.0 / u[0]);

            for (var i = 0; i < ru; i++)
            {
                u[i] *= s;
            }

            return u;
        }

        /// <summary>
        /// Perform calculation of Q or R
        /// </summary>
        /// <param name="u">Work array</param>
        /// <param name="a">Q or R matrices</param>
        /// <param name="rowStart">The first row</param>
        /// <param name="rowEnd">The last row</param>
        /// <param name="columnStart">The first column</param>
        /// <param name="columnEnd">The last column</param>
        private static void ComputeQR(double[] u, Matrix a, int rowStart, int rowEnd, int columnStart, int columnEnd)
        {
            if (rowEnd < rowStart || columnEnd < columnStart)
            {
                return;
            }

            var v = new double[columnEnd - columnStart + 1];
            for (var j = columnStart; j <= columnEnd; j++)
            {
                v[j - columnStart] = 0.0;
            }

            for (var i = rowStart; i <= rowEnd; i++)
            {
                for (var j = columnStart; j <= columnEnd; j++)
                {
                    v[j - columnStart] = v[j - columnStart] + (u[i - rowStart] * a.At(i, j));
                }
            }

            for (var i = rowStart; i <= rowEnd; i++)
            {
                for (var j = columnStart; j <= columnEnd; j++)
                {
                    a.At(i, j, a.At(i, j) - (u[i - rowStart] * v[j - columnStart]));
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>X</b>.</param>
        public override void Solve(Matrix input, Matrix result)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // The solution X should have the same number of columns as B
            if (input.ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            // The dimension compatibility conditions for X = A\B require the two matrices A and B to have the same number of rows
            if (MatrixR.RowCount != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            // The solution X row dimension is equal to the column dimension of A
            if (MatrixR.ColumnCount != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            var inputCopy = input.Clone();

            // Compute Y = transpose(Q)*B
            var column = new double[MatrixR.RowCount];
            for (var j = 0; j < input.ColumnCount; j++)
            {
                for (var k = 0; k < MatrixR.RowCount; k++)
                {
                    column[k] = inputCopy.At(k, j);
                }

                for (var i = 0; i < MatrixR.RowCount; i++)
                {
                    double s = 0;
                    for (var k = 0; k < MatrixR.RowCount; k++)
                    {
                        s += MatrixQ.At(k, i) * column[k];
                    }

                    inputCopy.At(i, j, s);
                }
            }

            // Solve R*X = Y;
            for (var k = MatrixR.ColumnCount - 1; k >= 0; k--)
            {
                for (var j = 0; j < input.ColumnCount; j++)
                {
                    inputCopy.At(k, j, inputCopy.At(k, j) / MatrixR.At(k, k));
                }

                for (var i = 0; i < k; i++)
                {
                    for (var j = 0; j < input.ColumnCount; j++)
                    {
                        inputCopy.At(i, j, inputCopy.At(i, j) - (inputCopy.At(k, j) * MatrixR.At(i, k)));
                    }
                }
            }

            for (var i = 0; i < MatrixR.ColumnCount; i++)
            {
                for (var j = 0; j < inputCopy.ColumnCount; j++)
                {
                    result.At(i, j, inputCopy.At(i, j));
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>x</b>.</param>
        public override void Solve(Vector input, Vector result)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // Ax=b where A is an m x n matrix
            // Check that b is a column vector with m entries
            if (MatrixR.RowCount != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            // Check that x is a column vector with n entries
            if (MatrixR.ColumnCount != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var inputCopy = input.Clone();

            // Compute Y = transpose(Q)*B
            var column = new double[MatrixR.RowCount];
            for (var k = 0; k < MatrixR.RowCount; k++)
            {
                column[k] = inputCopy[k];
            }

            for (var i = 0; i < MatrixR.RowCount; i++)
            {
                double s = 0;
                for (var k = 0; k < MatrixR.RowCount; k++)
                {
                    s += MatrixQ.At(k, i) * column[k];
                }

                inputCopy[i] = s;
            }

            // Solve R*X = Y;
            for (var k = MatrixR.ColumnCount - 1; k >= 0; k--)
            {
                inputCopy[k] /= MatrixR.At(k, k);
                for (var i = 0; i < k; i++)
                {
                    inputCopy[i] -= inputCopy[k] * MatrixR.At(i, k);
                }
            }

            for (var i = 0; i < MatrixR.ColumnCount; i++)
            {
                result[i] = inputCopy[i];
            }
        }
    }
}
