// <copyright file="UserGramSchmidt.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex.Factorization
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition Modified Gram-Schmidt Orthogonalization.</para>
    /// <para>Any complex square matrix A may be decomposed as A = QR where Q is an unitary mxn matrix and R is an nxn upper triangular matrix.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by modified Gram-Schmidt Orthogonalization.
    /// </remarks>
    internal sealed class UserGramSchmidt : GramSchmidt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserGramSchmidt"/> class. This object creates an unitary matrix
        /// using the modified Gram-Schmidt method.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> row count is less then column count</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is rank deficient</exception>
        public static UserGramSchmidt Create(Matrix<Complex> matrix)
        {
            if (matrix.RowCount < matrix.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix);
            }

            var q = matrix.Clone();
            var r = Matrix<Complex>.Build.SameAs(matrix, matrix.ColumnCount, matrix.ColumnCount, fullyMutable: true);

            for (var k = 0; k < q.ColumnCount; k++)
            {
                var norm = q.Column(k).L2Norm();
                if (norm == 0.0)
                {
                    throw new ArgumentException("Matrix must not be rank deficient.");
                }

                r.At(k, k, norm);
                for (var i = 0; i < q.RowCount; i++)
                {
                    q.At(i, k, q.At(i, k) / norm);
                }

                for (var j = k + 1; j < q.ColumnCount; j++)
                {
                    var dot = Complex.Zero;
                    for (int i = 0; i < q.RowCount; i++)
                    {
                        dot += q.Column(k)[i].Conjugate() * q.Column(j)[i];
                    }

                    r.At(k, j, dot);
                    for (var i = 0; i < q.RowCount; i++)
                    {
                        var value = q.At(i, j) - (q.At(i, k) * dot);
                        q.At(i, j, value);
                    }
                }
            }

            return new UserGramSchmidt(q, r);
        }

        UserGramSchmidt(Matrix<Complex> q, Matrix<Complex> rFull)
            : base(q, rFull)
        {
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<Complex> input, Matrix<Complex> result)
        {
            // The solution X should have the same number of columns as B
            if (input.ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            // The dimension compatibility conditions for X = A\B require the two matrices A and B to have the same number of rows
            if (Q.RowCount != input.RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            // The solution X row dimension is equal to the column dimension of A
            if (Q.ColumnCount != result.RowCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            var inputCopy = input.Clone();

            // Compute Y = transpose(Q)*B
            var column = new Complex[Q.RowCount];
            for (var j = 0; j < input.ColumnCount; j++)
            {
                for (var k = 0; k < Q.RowCount; k++)
                {
                    column[k] = inputCopy.At(k, j);
                }

                for (var i = 0; i < Q.ColumnCount; i++)
                {
                    var s = Complex.Zero;
                    for (var k = 0; k < Q.RowCount; k++)
                    {
                        s += Q.At(k, i).Conjugate() * column[k];
                    }

                    inputCopy.At(i, j, s);
                }
            }

            // Solve R*X = Y;
            for (var k = Q.ColumnCount - 1; k >= 0; k--)
            {
                for (var j = 0; j < input.ColumnCount; j++)
                {
                    inputCopy.At(k, j, inputCopy.At(k, j) / FullR.At(k, k));
                }

                for (var i = 0; i < k; i++)
                {
                    for (var j = 0; j < input.ColumnCount; j++)
                    {
                        inputCopy.At(i, j, inputCopy.At(i, j) - (inputCopy.At(k, j) * FullR.At(i, k)));
                    }
                }
            }

            for (var i = 0; i < FullR.ColumnCount; i++)
            {
                for (var j = 0; j < input.ColumnCount; j++)
                {
                    result.At(i, j, inputCopy.At(i, j));
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<Complex> input, Vector<Complex> result)
        {
            // Ax=b where A is an m x n matrix
            // Check that b is a column vector with m entries
            if (Q.RowCount != input.Count)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            // Check that x is a column vector with n entries
            if (Q.ColumnCount != result.Count)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(Q, result);
            }

            var inputCopy = input.Clone();

            // Compute Y = transpose(Q)*B
            var column = new Complex[Q.RowCount];
            for (var k = 0; k < Q.RowCount; k++)
            {
                column[k] = inputCopy[k];
            }

            for (var i = 0; i < Q.ColumnCount; i++)
            {
                var s = Complex.Zero;
                for (var k = 0; k < Q.RowCount; k++)
                {
                    s += Q.At(k, i).Conjugate() * column[k];
                }

                inputCopy[i] = s;
            }

            // Solve R*X = Y;
            for (var k = Q.ColumnCount - 1; k >= 0; k--)
            {
                inputCopy[k] /= FullR.At(k, k);
                for (var i = 0; i < k; i++)
                {
                    inputCopy[i] -= inputCopy[k] * FullR.At(i, k);
                }
            }

            for (var i = 0; i < FullR.ColumnCount; i++)
            {
                result[i] = inputCopy[i];
            }
        }
    }
}
