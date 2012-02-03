// <copyright file="UserLU.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Single.Factorization
{
    using System;
    using Generic;
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of an LU factorization.</para>
    /// <para>For a matrix A, the LU factorization is a pair of lower triangular matrix L and
    /// upper triangular matrix U so that A = L*U.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the LU factorization is done at construction time.
    /// </remarks>
    public class UserLU : LU
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLU"/> class. This object will compute the
        /// LU factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public UserLU(Matrix<float> matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            // Create an array for the pivot indices.
            var order = matrix.RowCount;
            Factors = matrix.Clone();
            Pivots = new int[order];
            
            // Initialize the pivot matrix to the identity permutation.
            for (var i = 0; i < order; i++)
            {
                Pivots[i] = i;
            }

            var vectorLUcolj = new float[order];
            for (var j = 0; j < order; j++)
            {
                // Make a copy of the j-th column to localize references.
                for (var i = 0; i < order; i++)
                {
                    vectorLUcolj[i] = Factors.At(i, j);
                }

                // Apply previous transformations.
                for (var i = 0; i < order; i++)
                {
                    var kmax = Math.Min(i, j);
                    var s = 0.0f;
                    for (var k = 0; k < kmax; k++)
                    {
                        s += Factors.At(i, k) * vectorLUcolj[k];
                    }

                    vectorLUcolj[i] -= s;
                    Factors.At(i, j, vectorLUcolj[i]);
                }

                // Find pivot and exchange if necessary.
                var p = j;
                for (var i = j + 1; i < order; i++)
                {
                    if (Math.Abs(vectorLUcolj[i]) > Math.Abs(vectorLUcolj[p]))
                    {
                        p = i;
                    }
                }

                if (p != j)
                {
                    for (var k = 0; k < order; k++)
                    {
                        var temp = Factors.At(p, k);
                        Factors.At(p, k, Factors.At(j, k));
                        Factors.At(j, k, temp);
                    }

                    Pivots[j] = p;
                }

                // Compute multipliers.
                if (j < order & Factors.At(j, j) != 0.0)
                {
                    for (var i = j + 1; i < order; i++)
                    {
                        Factors.At(i, j, (Factors.At(i, j) / Factors.At(j, j)));
                    }
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <c>AX = B</c>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <c>B</c>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <c>X</c>.</param>
        public override void Solve(Matrix<float> input, Matrix<float> result)
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
            if (result.RowCount != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            if (result.ColumnCount != input.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            if (input.RowCount != Factors.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factors);
            }

            // Copy the contents of input to result.
            input.CopyTo(result);
            for (var i = 0; i < Pivots.Length; i++)
            {
                if (Pivots[i] == i)
                {
                    continue;
                }

                var p = Pivots[i];
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    var temp = result.At(p, j);
                    result.At(p, j, result.At(i, j));
                    result.At(i, j, temp);
                }
            }

            var order = Factors.RowCount;
            
            // Solve L*Y = P*B
            for (var k = 0; k < order; k++)
            {
                for (var i = k + 1; i < order; i++)
                {
                    for (var j = 0; j < result.ColumnCount; j++)
                    {
                        var temp = result.At(k, j) * Factors.At(i, k);
                        result.At(i, j, result.At(i, j) - temp);
                    }
                }
            }

            // Solve U*X = Y;
            for (var k = order - 1; k >= 0; k--)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    result.At(k, j, (result.At(k, j) / Factors.At(k, k)));
                }

                for (var i = 0; i < k; i++)
                {
                    for (var j = 0; j < result.ColumnCount; j++)
                    {
                        var temp = result.At(k, j) * Factors.At(i, k);
                        result.At(i, j, result.At(i, j) - temp);
                    }
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <c>Ax = b</c>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <c>b</c>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <c>x</c>.</param>
        public override void Solve(Vector<float> input, Vector<float> result)
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

            if (input.Count != Factors.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factors);
            }

            // Copy the contents of input to result.
            input.CopyTo(result);
            for (var i = 0; i < Pivots.Length; i++)
            {
                if (Pivots[i] == i)
                {
                    continue;
                }

                var p = Pivots[i];
                var temp = result[p];
                result[p] = result[i];
                result[i] = temp;
            }
            
            var order = Factors.RowCount;

            // Solve L*Y = P*B
            for (var k = 0; k < order; k++)
            {
                for (var i = k + 1; i < order; i++)
                {
                    result[i] -= result[k] * Factors.At(i, k);
                }
            }

            // Solve U*X = Y;
            for (var k = order - 1; k >= 0; k--)
            {
                result[k] /= Factors.At(k, k);
                for (var i = 0; i < k; i++)
                {
                    result[i] -= result[k] * Factors.At(i, k);
                }
            }
        }

        /// <summary>
        /// Returns the inverse of this matrix. The inverse is calculated using LU decomposition.
        /// </summary>
        /// <returns>The inverse of this matrix.</returns>
        public override Matrix<float> Inverse()
        {
            var order = Factors.RowCount;
            var inverse = Factors.CreateMatrix(order, order);
            for (var i = 0; i < order; i++)
            {
                inverse.At(i, i, 1.0f);
            }

            return Solve(inverse);
        }
    }
}
