// <copyright file="QR.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Factorization
{
    /// <summary>
    /// The type of QR factorization go perform.
    /// </summary>
    public enum QRMethod
    {
        /// <summary>
        /// Compute the full QR factorization of a matrix.
        /// </summary>
        Full = 0,

        /// <summary>
        /// Compute the thin QR factorization of a matrix.
        /// </summary>
        Thin = 1
    }

    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A (m x n) may be decomposed as A = QR where Q is an orthogonal matrix
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R is an upper triangular matrix
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// If a <seealso cref="QRMethod.Full"/> factorization is performed, the resulting Q matrix is an m x m matrix
    /// and the R matrix is an m x n matrix. If a <seealso cref="QRMethod.Thin"/> factorization is performed, the
    /// resulting Q matrix is an m x n matrix and the R matrix is an n x n matrix.
    /// </remarks>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class QR<T> : ISolver<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        readonly Lazy<Matrix<T>> _lazyR;

        protected readonly Matrix<T> FullR;
        protected readonly QRMethod Method;

        protected QR(Matrix<T> q, Matrix<T> rFull, QRMethod method)
        {
            Q = q;
            FullR = rFull;
            Method = method;

            _lazyR = new Lazy<Matrix<T>>(FullR.UpperTriangle);
        }

        /// <summary>
        /// Gets or sets orthogonal Q matrix
        /// </summary>
        public Matrix<T> Q { get; }

        /// <summary>
        /// Gets the upper triangular factor R.
        /// </summary>
        public Matrix<T> R => _lazyR.Value;

        /// <summary>
        /// Gets the absolute determinant value of the matrix for which the QR matrix was computed.
        /// </summary>
        public abstract T Determinant { get; }

        /// <summary>
        /// Gets a value indicating whether the matrix is full rank or not.
        /// </summary>
        /// <value><c>true</c> if the matrix is full rank; otherwise <c>false</c>.</value>
        public abstract bool IsFullRank { get; }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</returns>
        public virtual Matrix<T> Solve(Matrix<T> input)
        {
            var x = Matrix<T>.Build.SameAs(input, FullR.ColumnCount, input.ColumnCount, fullyMutable: true);
            Solve(input, x);
            return x;
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
            var x = Vector<T>.Build.SameAs(input, FullR.ColumnCount);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public abstract void Solve(Vector<T> input, Vector<T> result);
    }
}
