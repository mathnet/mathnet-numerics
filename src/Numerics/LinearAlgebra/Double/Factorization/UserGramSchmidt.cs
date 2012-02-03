// <copyright file="UserGramSchmidt.cs" company="Math.NET">
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
    using Generic;
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition Modified Gram-Schmidt Orthogonalization.</para>
    /// <para>Any real square matrix A may be decomposed as A = QR where Q is an orthogonal mxn matrix and R is an nxn upper triangular matrix.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by modified Gram-Schmidt Orthogonalization.
    /// </remarks>
    public class UserGramSchmidt : GramSchmidt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserGramSchmidt"/> class. This object creates an orthogonal matrix 
        /// using the modified Gram-Schmidt method.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> row count is less then column count</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is rank deficient</exception>
        public UserGramSchmidt(Matrix<double> matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount < matrix.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix);
            }

            MatrixQ = matrix.Clone();
            MatrixR = matrix.CreateMatrix(matrix.ColumnCount, matrix.ColumnCount);

            for (var k = 0; k < MatrixQ.ColumnCount; k++)
            {
                var norm = MatrixQ.Column(k).Norm(2);
                if (norm == 0.0)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixNotRankDeficient);
                }

                MatrixR.At(k, k, norm);
                for (var i = 0; i < MatrixQ.RowCount; i++)
                {
                    MatrixQ.At(i, k, MatrixQ.At(i, k) / norm);
                }

                for (var j = k + 1; j < MatrixQ.ColumnCount; j++)
                {
                    var dot = MatrixQ.Column(k).DotProduct(MatrixQ.Column(j));
                    MatrixR.At(k, j, dot);
                    for (var i = 0; i < MatrixQ.RowCount; i++)
                    {
                        var value = MatrixQ.At(i, j) - (MatrixQ.At(i, k) * dot);
                        MatrixQ.At(i, j, value);
                    }
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<double> input, Matrix<double> result)
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
            if (MatrixQ.RowCount != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            // The solution X row dimension is equal to the column dimension of A
            if (MatrixQ.ColumnCount != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            var inputCopy = input.Clone();
            
            // Compute Y = transpose(Q)*B
            var column = new double[MatrixQ.RowCount];
            for (var j = 0; j < input.ColumnCount; j++)
            {
                for (var k = 0; k < MatrixQ.RowCount; k++)
                {
                    column[k] = inputCopy.At(k, j);
                }

                for (var i = 0; i < MatrixQ.ColumnCount; i++)
                {
                    double s = 0;
                    for (var k = 0; k < MatrixQ.RowCount; k++)
                    {
                        s += MatrixQ.At(k, i) * column[k];
                    }

                    inputCopy.At(i, j, s);
                }
            }

            // Solve R*X = Y;
            for (var k = MatrixQ.ColumnCount - 1; k >= 0; k--)
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
        public override void Solve(Vector<double> input, Vector<double> result)
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
            if (MatrixQ.RowCount != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            // Check that x is a column vector with n entries
            if (MatrixQ.ColumnCount != result.Count)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(MatrixQ, result);
            }

            var inputCopy = input.Clone();

            // Compute Y = transpose(Q)*B
            var column = new double[MatrixQ.RowCount];
            for (var k = 0; k < MatrixQ.RowCount; k++)
            {
                column[k] = inputCopy[k];
            }

            for (var i = 0; i < MatrixQ.ColumnCount; i++)
            {
                double s = 0;
                for (var k = 0; k < MatrixQ.RowCount; k++)
                {
                    s += MatrixQ.At(k, i) * column[k];
                }

                inputCopy[i] = s;
            }

            // Solve R*X = Y;
            for (var k = MatrixQ.ColumnCount - 1; k >= 0; k--)
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
