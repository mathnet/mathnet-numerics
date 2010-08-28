// <copyright file="QR.cs" company="Math.NET">
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
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A (m x n) may be decomposed as A = QR where Q is an orthogonal matrix (m x m)
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R (m x n) is an upper triangular matrix 
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// </remarks>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class QR<T> : ISolver<T>
    where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Gets or sets orthogonal Q matrix
        /// </summary>
        protected Matrix<T> MatrixQ
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets upper triangular factor R
        /// </summary>
        protected Matrix<T> MatrixR
        {
            get;
            set;
        }

        /// <summary>
        /// Internal method which routes the call to perform the QR factorization to the appropriate class.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>A QR factorization object.</returns>
        internal static QR<T> Create(Matrix<T> matrix)
        {
            if (typeof(T) == typeof(double))
            {
                var dense = matrix as LinearAlgebra.Double.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Double.Factorization.DenseQR(dense) as QR<T>;
                }

                return new LinearAlgebra.Double.Factorization.UserQR(matrix as Matrix<double>) as QR<T>;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets orthogonal Q matrix
        /// </summary>
        public virtual Matrix<T> Q 
        {
            get
            {
                return MatrixQ.Clone();
            }
        }

        /// <summary>
        /// Gets the upper triangular factor R.
        /// </summary>
        public virtual Matrix<T> R
        {
            get
            {
                return MatrixR.UpperTriangle();
            }
        }

        /// <summary>
        /// Gets the absolute determinant value of the matrix for which the QR matrix was computed.
        /// </summary>
        public virtual double Determinant
        {
            get
            {
                if (MatrixR.RowCount != MatrixR.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixSquare);
                }

                var det = OneValueT;
                for (var i = 0; i < MatrixR.ColumnCount; i++)
                {
                    det = MultiplyT(det, MatrixR.At(i, i));
                    if (AbsoluteT(MatrixR.At(i, i)).AlmostEqualInDecimalPlaces(0.0, 15))
                    {
                        return 0;
                    }
                }

                return AbsoluteT(det);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix is full rank or not.
        /// </summary>
        /// <value><c>true</c> if the matrix is full rank; otherwise <c>false</c>.</value>
        public virtual bool IsFullRank
        {
            get
            {
                for (var i = 0; i < MatrixR.ColumnCount; i++)
                {
                    if (AbsoluteT(MatrixR.At(i, i)).AlmostEqualInDecimalPlaces(0.0, 15))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
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

            var matrixX = input.CreateMatrix(MatrixR.ColumnCount, input.ColumnCount);
            Solve(input, matrixX);
            return matrixX;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix<T> input, Matrix<T> result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
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

            var x = input.CreateVector(MatrixR.ColumnCount);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public abstract void Solve(Vector<T> input, Vector<T> result);

        #region Simple arithmetic of type T
        /// <summary>
        /// Multiply two values T*T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of multiplication</returns>
        protected abstract T MultiplyT(T val1, T val2);

        /// <summary>
        /// Take absolute value
        /// </summary>
        /// <param name="val">Source alue</param>
        /// <returns>True if one; otherwise false</returns>
        protected abstract double AbsoluteT(T val);

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
