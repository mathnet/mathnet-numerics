// <copyright file="Cholesky.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Generic.Factorization
{
    using System;
    using System.Numerics;
    using Generic;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class Cholesky<T> : ISolver<T>
    where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Internal method which routes the call to perform the Cholesky factorization to the appropriate class.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>A cholesky factorization object.</returns>
        internal static Cholesky<T> Create(Matrix<T> matrix)
        {
            if (typeof(T) == typeof(double))
            {
                var dense = matrix as LinearAlgebra.Double.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Double.Factorization.DenseCholesky(dense) as Cholesky<T>;
                }

                return new LinearAlgebra.Double.Factorization.UserCholesky(matrix as Matrix<double>) as Cholesky<T>;
            }

            if (typeof(T) == typeof(Complex))
            {
                var dense = matrix as LinearAlgebra.Complex.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Complex.Factorization.DenseCholesky(dense) as Cholesky<T>;
                }

                return new LinearAlgebra.Complex.Factorization.UserCholesky(matrix as Matrix<Complex>) as Cholesky<T>;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the lower triangular form of the Cholesky matrix
        /// </summary>
        protected Matrix<T> CholeskyFactor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the lower triangular form of the Cholesky matrix.
        /// </summary>
        public virtual Matrix<T> Factor
        {
            get
            {
                return CholeskyFactor.Clone();
            }
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public virtual T Determinant
        {
            get
            {
                var det = OneValueT;
                for (var j = 0; j < CholeskyFactor.RowCount; j++)
                {
                    det = MultiplyT(det, MultiplyT(CholeskyFactor[j, j], CholeskyFactor[j, j]));
                }

                return det;
            }
        }

        /// <summary>
        /// Gets the log determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public virtual T DeterminantLn
        {
            get
            {
                var det = default(T);
                for (var j = 0; j < CholeskyFactor.RowCount; j++)
                {
                    // det += 2.0 * CholeskyFactor[j, j].NaturalLogarithm();
                    det = AddT(det, MultiplyT(AddT(OneValueT, OneValueT), LogT(CholeskyFactor[j, j])));
                }

                return det;
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A Cholesky factorized.
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
        /// Solves a system of linear equations, <b>AX = B</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix<T> input, Matrix<T> result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A Cholesky factorized.
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
        /// Solves a system of linear equations, <b>Ax = b</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public abstract void Solve(Vector<T> input, Vector<T> result);

        #region Simple arithmetic of type T

        /// <summary>
        /// Add two values T+T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of addition</returns>
        protected abstract T AddT(T val1, T val2);

        /// <summary>
        /// Multiply two values T*T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of multiplication</returns>
        protected abstract T MultiplyT(T val1, T val2);

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified number.
        /// </summary>
        /// <param name="val1"> A number whose logarithm is to be found</param>
        /// <returns>Natural (base e) logarithm </returns>
        protected abstract T LogT(T val1);

        /// <summary>
        /// Gets value of type T equal to one
        /// </summary>
        /// <returns>One value</returns>
        protected abstract T OneValueT
        {
            get;
        }
        #endregion
    }
}
