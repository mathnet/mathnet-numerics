// <copyright file="UserCholesky.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex.Factorization
{
    using System;
    using System.Numerics;
    using Generic;
    using Generic.Factorization;
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization for user matrices.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    public class UserCholesky : Cholesky<Complex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCholesky"/> class. This object will compute the
        /// Cholesky factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        public UserCholesky(Matrix<Complex> matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            // Create a new matrix for the Cholesky factor, then perform factorization (while overwriting).
            CholeskyFactor = matrix.Clone();
            for (var i = 0; i < CholeskyFactor.RowCount; i++)
            {
                var d = Complex.Zero;
                for (var j = 0; j < i; j++)
                {
                    var s = Complex.Zero;
                    for (var k = 0; k < j; k++)
                    {
                        s += CholeskyFactor.At(i, k) * CholeskyFactor.At(j, k).Conjugate();
                    }

                    s = (matrix.At(i, j) - s) / CholeskyFactor.At(j, j);
                    CholeskyFactor.At(i, j, s);
                    d += s * s.Conjugate();
                }

                d = matrix.At(i, i) - d;
                if (d.Real <= 0)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixPositiveDefinite);
                }

                CholeskyFactor.At(i, i, d.SquareRoot());
                for (var k = i + 1; k < CholeskyFactor.RowCount; k++)
                {
                    CholeskyFactor.At(i, k, 0.0);
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<Complex> input, Matrix<Complex> result)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // Check for proper dimensions.
            if (result.RowCount != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            if (result.ColumnCount != input.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            if (input.RowCount != CholeskyFactor.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            input.CopyTo(result);
            var order = CholeskyFactor.RowCount;

            for (var c = 0; c < result.ColumnCount; c++)
            {
                // Solve L*Y = B;
                Complex sum;
                for (var i = 0; i < order; i++)
                {
                    sum = result.At(i, c);
                    for (var k = i - 1; k >= 0; k--)
                    {
                        sum -= CholeskyFactor.At(i, k) * result.At(k, c);
                    }

                    result.At(i, c, sum / CholeskyFactor.At(i, i));
                }

                // Solve L'*X = Y;
                for (var i = order - 1; i >= 0; i--)
                {
                    sum = result.At(i, c);
                    for (var k = i + 1; k < order; k++)
                    {
                        sum -= CholeskyFactor.At(k, i).Conjugate() * result.At(k, c);
                    }

                    result.At(i, c, sum / CholeskyFactor.At(i, i));
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<Complex> input, Vector<Complex> result)
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

            // Check for proper dimensions.
            if (input.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (input.Count != CholeskyFactor.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            input.CopyTo(result);
            var order = CholeskyFactor.RowCount;

            // Solve L*Y = B;
            Complex sum;
            for (var i = 0; i < order; i++)
            {
                sum = result[i];
                for (var k = i - 1; k >= 0; k--)
                {
                    sum -= CholeskyFactor.At(i, k) * result[k];
                }

                result[i] = sum / CholeskyFactor.At(i, i);
            }

            // Solve L'*X = Y;
            for (var i = order - 1; i >= 0; i--)
            {
                sum = result[i];
                for (var k = i + 1; k < order; k++)
                {
                    sum -= CholeskyFactor.At(k, i).Conjugate() * result[k];
                }

                result[i] = sum / CholeskyFactor.At(i, i);
            }
        }

        #region Simple arithmetic of type T
        /// <summary>
        /// Add two values T+T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of addition</returns>
        protected sealed override Complex AddT(Complex val1, Complex val2)
        {
            return val1 + val2;
        }

        /// <summary>
        /// Multiply two values T*T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of multiplication</returns>
        protected sealed override Complex MultiplyT(Complex val1, Complex val2)
        {
            return val1 * val2;
        }

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified number.
        /// </summary>
        /// <param name="val1"> A number whose logarithm is to be found</param>
        /// <returns>Natural (base e) logarithm </returns>
        protected sealed override Complex LogT(Complex val1)
        {
            return val1.NaturalLogarithm();
        }
        #endregion
    }
}
