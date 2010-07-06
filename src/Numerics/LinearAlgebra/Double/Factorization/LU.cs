// <copyright file="LU.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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

    /// <summary>
    /// <para>A class which encapsulates the functionality of an LU factorization.</para>
    /// <para>For a matrix A, the LU factorization is a pair of lower triangular matrix L and
    /// upper triangular matrix U so that A = L*U.</para>
    /// <para>In the Math.Net implementation we also store a set of pivot elements for increased 
    /// numerical stability. The pivot elements encode a permutation matrix P such that P*A = L*U.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the LU factorization is done at construction time.
    /// </remarks>
    public abstract class LU : ISolver
    {
        /// <summary>
        /// Gets or sets both the L and U factors in the same matrix.
        /// </summary>
        protected Matrix Factors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pivot indices of the LU factorization.
        /// </summary>
        protected int[] Pivots
        {
            get;
            set;
        }

        /// <summary>
        /// Internal method which routes the call to perform the LU factorization to the appropriate class.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>An LU factorization object.</returns>
        internal static LU Create(Matrix matrix)
        {
            var dense = matrix as DenseMatrix;
            if (dense != null)
            {
                return new DenseLU(dense);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the lower triangular factor.
        /// </summary>
        public virtual Matrix L
        {
            get
            {
                var result = Factors.LowerTriangle();
                for (var i = 0; i < result.RowCount; i++)
                {
                    result.At(i, i, 1);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the upper triangular factor.
        /// </summary>
        public virtual Matrix U
        {
            get
            {
                return Factors.UpperTriangle();
            }
        }

        /// <summary>
        /// Gets the permutation applied to LU factorization.
        /// </summary>
        public virtual Permutation P
        {
            get
            {
                return Permutation.FromInversions(Pivots);
            }
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the LU factorization was computed.
        /// </summary>
        public virtual double Determinant
        {
            get
            {
                var det = 1.0;
                for (var j = 0; j < Factors.RowCount; j++)
                {
                    if (Pivots[j] != j)
                    {
                        det = -det * Factors.At(j, j);
                    }
                    else
                    {
                        det *= Factors.At(j, j);
                    }
                }

                return det;
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix"/>, <b>X</b>.</returns>
        public virtual Matrix Solve(Matrix input)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var x = input.CreateMatrix(input.RowCount, input.ColumnCount);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix input, Matrix result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <returns>The left hand side <see cref="Vector"/>, <b>x</b>.</returns>
        public virtual Vector Solve(Vector input)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var x = input.CreateVector(input.Count);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>x</b>.</param>
        public abstract void Solve(Vector input, Vector result);

        /// <summary>
        /// Returns the inverse of this matrix. The inverse is calculated using LU decomposition.
        /// </summary>
        /// <returns>The inverse of this matrix.</returns>
        public abstract Matrix Inverse();
    }
}
