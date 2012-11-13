// <copyright file="ExtensionMethods.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex
{
    using System.Numerics;
    using Factorization;
    using Generic;
    using Generic.Factorization;

    /// <summary>
    /// Extension methods which return factorizations for the various matrix classes.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Computes the Cholesky decomposition for a matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>The Cholesky decomposition object.</returns>
        public static Cholesky Cholesky(this Matrix<Complex> matrix)
        {
            return (Cholesky)Cholesky<Complex>.Create(matrix);
        }

        /// <summary>
        /// Computes the LU decomposition for a matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>The LU decomposition object.</returns>
        public static LU LU(this Matrix<Complex> matrix)
        {
            return (LU)LU<Complex>.Create(matrix);
        }

        /// <summary>
        /// Computes the QR decomposition for a matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="method">The type of QR factorization to perform.</param>
        /// <returns>The QR decomposition object.</returns>
        public static QR QR(this Matrix<Complex> matrix, QRMethod method = QRMethod.Full)
        {
            return (QR)QR<Complex>.Create(matrix, method);
        }

        /// <summary>
        /// Computes the QR decomposition for a matrix using Modified Gram-Schmidt Orthogonalization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>The QR decomposition object.</returns>
        public static GramSchmidt GramSchmidt(this Matrix<Complex> matrix)
        {
            return (GramSchmidt)GramSchmidt<Complex>.Create(matrix);
        }

        /// <summary>
        /// Computes the SVD decomposition for a matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <returns>The SVD decomposition object.</returns>
        public static Svd Svd(this Matrix<Complex> matrix, bool computeVectors)
        {
            return (Svd)Svd<Complex>.Create(matrix, computeVectors);
        }

        /// <summary>
        /// Computes the EVD decomposition for a matrix.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>The EVD decomposition object.</returns>
        public static Evd Evd(this Matrix<Complex> matrix)
        {
            return (Evd)Evd<Complex>.Create(matrix);
        }
    }
}
