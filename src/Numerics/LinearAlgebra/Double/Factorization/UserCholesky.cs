// <copyright file="UserCholesky.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using System;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Double.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization for user matrices.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    internal sealed class UserCholesky : Cholesky
    {
        /// <summary>
        /// Computes the Cholesky factorization in-place.
        /// </summary>
        /// <param name="factor">On entry, the matrix to factor. On exit, the Cholesky factor matrix</param>
        /// <exception cref="ArgumentNullException">If <paramref name="factor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="factor"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="factor"/> is not positive definite.</exception>
        static void DoCholesky(Matrix<double> factor)
        {
            if (factor.RowCount != factor.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            var tmpColumn = new double[factor.RowCount];

            // Main loop - along the diagonal
            for (var ij = 0; ij < factor.RowCount; ij++)
            {
                // "Pivot" element
                var tmpVal = factor.At(ij, ij);

                if (tmpVal > 0.0)
                {
                    tmpVal = Math.Sqrt(tmpVal);
                    factor.At(ij, ij, tmpVal);
                    tmpColumn[ij] = tmpVal;

                    // Calculate multipliers and copy to local column
                    // Current column, below the diagonal
                    for (var i = ij + 1; i < factor.RowCount; i++)
                    {
                        factor.At(i, ij, factor.At(i, ij)/tmpVal);
                        tmpColumn[i] = factor.At(i, ij);
                    }

                    // Remaining columns, below the diagonal
                    DoCholeskyStep(factor, factor.RowCount, ij + 1, factor.RowCount, tmpColumn, Control.MaxDegreeOfParallelism);
                }
                else
                {
                    throw new ArgumentException("Matrix must be positive definite.");
                }

                for (var i = ij + 1; i < factor.RowCount; i++)
                {
                    factor.At(ij, i, 0.0);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCholesky"/> class. This object will compute the
        /// Cholesky factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        public static UserCholesky Create(Matrix<double> matrix)
        {
            // Create a new matrix for the Cholesky factor, then perform factorization (while overwriting).
            var factor = matrix.Clone();
            DoCholesky(factor);
            return new UserCholesky(factor);
        }

        /// <summary>
        /// Calculates the Cholesky factorization of the input matrix.
        /// </summary>
        /// <param name="matrix">The matrix to be factorized<see cref="Matrix{T}"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="matrix"/> does not have the same dimensions as the existing factor.</exception>
        public override void Factorize(Matrix<double> matrix)
        {
            if (matrix.RowCount != Factor.RowCount || matrix.ColumnCount != Factor.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix, Factor);
            }

            matrix.CopyTo(Factor);
            DoCholesky(Factor);
        }

        UserCholesky(Matrix<double> factor)
            : base(factor)
        {
        }

        /// <summary>
        /// Calculate Cholesky step
        /// </summary>
        /// <param name="data">Factor matrix</param>
        /// <param name="rowDim">Number of rows</param>
        /// <param name="firstCol">Column start</param>
        /// <param name="colLimit">Total columns</param>
        /// <param name="multipliers">Multipliers calculated previously</param>
        /// <param name="availableCores">Number of available processors</param>
        static void DoCholeskyStep(Matrix<double> data, int rowDim, int firstCol, int colLimit, double[] multipliers, int availableCores)
        {
            var tmpColCount = colLimit - firstCol;

            if ((availableCores > 1) && (tmpColCount > 200))
            {
                var tmpSplit = firstCol + (tmpColCount/3);
                var tmpCores = availableCores/2;

                CommonParallel.Invoke(
                    () => DoCholeskyStep(data, rowDim, firstCol, tmpSplit, multipliers, tmpCores),
                    () => DoCholeskyStep(data, rowDim, tmpSplit, colLimit, multipliers, tmpCores));
            }
            else
            {
                for (var j = firstCol; j < colLimit; j++)
                {
                    var tmpVal = multipliers[j];
                    for (var i = j; i < rowDim; i++)
                    {
                        data.At(i, j, data.At(i, j) - (multipliers[i]*tmpVal));
                    }
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<double> input, Matrix<double> result)
        {
            if (result.RowCount != input.RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            if (result.ColumnCount != input.ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            if (input.RowCount != Factor.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factor);
            }

            input.CopyTo(result);
            var order = Factor.RowCount;

            for (var c = 0; c < result.ColumnCount; c++)
            {
                // Solve L*Y = B;
                double sum;
                for (var i = 0; i < order; i++)
                {
                    sum = result.At(i, c);
                    for (var k = i - 1; k >= 0; k--)
                    {
                        sum -= Factor.At(i, k)*result.At(k, c);
                    }

                    result.At(i, c, sum/Factor.At(i, i));
                }

                // Solve L'*X = Y;
                for (var i = order - 1; i >= 0; i--)
                {
                    sum = result.At(i, c);
                    for (var k = i + 1; k < order; k++)
                    {
                        sum -= Factor.At(k, i)*result.At(k, c);
                    }

                    result.At(i, c, sum/Factor.At(i, i));
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<double> input, Vector<double> result)
        {
            if (input.Count != result.Count)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (input.Count != Factor.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factor);
            }

            input.CopyTo(result);
            var order = Factor.RowCount;

            // Solve L*Y = B;
            double sum;
            for (var i = 0; i < order; i++)
            {
                sum = result[i];
                for (var k = i - 1; k >= 0; k--)
                {
                    sum -= Factor.At(i, k)*result[k];
                }

                result[i] = sum/Factor.At(i, i);
            }

            // Solve L'*X = Y;
            for (var i = order - 1; i >= 0; i--)
            {
                sum = result[i];
                for (var k = i + 1; k < order; k++)
                {
                    sum -= Factor.At(k, i)*result[k];
                }

                result[i] = sum/Factor.At(i, i);
            }
        }
    }
}
