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

namespace MathNet.Numerics.LinearAlgebra.Generic.Factorization
{
    using System;
    using System.Numerics;
    using Generic;
    using Numerics;

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
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class LU<T> : ISolver<T>
    where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Value of one for T.
        /// </summary>
        private static readonly T One = Common.OneOf<T>();

        /// <summary>
        /// Gets or sets both the L and U factors in the same matrix.
        /// </summary>
        protected Matrix<T> Factors
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
        internal static LU<T> Create(Matrix<T> matrix)
        {
            if (typeof(T) == typeof(double))
            {
                var dense = matrix as LinearAlgebra.Double.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Double.Factorization.DenseLU(dense) as LU<T>;
                }

                return new LinearAlgebra.Double.Factorization.UserLU(matrix as Matrix<double>) as LU<T>;
            }

            if (typeof(T) == typeof(float))
            {
                var dense = matrix as LinearAlgebra.Single.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Single.Factorization.DenseLU(dense) as LU<T>;
                }

                return new LinearAlgebra.Single.Factorization.UserLU(matrix as Matrix<float>) as LU<T>;
            }

            if (typeof(T) == typeof(Complex))
            {
                var dense = matrix as LinearAlgebra.Complex.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Complex.Factorization.DenseLU(dense) as LU<T>;
                }

                return new LinearAlgebra.Complex.Factorization.UserLU(matrix as Matrix<Complex>) as LU<T>;
            }

            if (typeof(T) == typeof(Complex32))
            {
                var dense = matrix as LinearAlgebra.Complex32.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Complex32.Factorization.DenseLU(dense) as LU<T>;
                }

                return new LinearAlgebra.Complex32.Factorization.UserLU(matrix as Matrix<Complex32>) as LU<T>;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the lower triangular factor.
        /// </summary>
        public virtual Matrix<T> L
        {
            get
            {
                var result = Factors.LowerTriangle();
                for (var i = 0; i < result.RowCount; i++)
                {
                    result.At(i, i, One);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the upper triangular factor.
        /// </summary>
        public virtual Matrix<T> U
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
        public abstract T Determinant
        {
            get;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</returns>
        public virtual Matrix<T> Solve(Matrix<T> input)
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
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix<T> input, Matrix<T> result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <returns>The left hand side <see cref="Vector{T}"/>, <b>x</b>.</returns>
        public virtual Vector<T> Solve(Vector<T> input)
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
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public abstract void Solve(Vector<T> input, Vector<T> result);

        /// <summary>
        /// Returns the inverse of this matrix. The inverse is calculated using LU decomposition.
        /// </summary>
        /// <returns>The inverse of this matrix.</returns>
        public abstract Matrix<T> Inverse();
    }
}
