// <copyright file="Matrix.Factorization.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

using MathNet.Numerics.LinearAlgebra.Factorization;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    public abstract partial class Matrix<T>
    {
        /// <summary>
        /// Computes the Cholesky decomposition for a matrix.
        /// </summary>
        /// <returns>The Cholesky decomposition object.</returns>
        public abstract Cholesky<T> Cholesky();

        /// <summary>
        /// Computes the LU decomposition for a matrix.
        /// </summary>
        /// <returns>The LU decomposition object.</returns>
        public abstract LU<T> LU();

        /// <summary>
        /// Computes the QR decomposition for a matrix.
        /// </summary>
        /// <param name="method">The type of QR factorization to perform.</param>
        /// <returns>The QR decomposition object.</returns>
        public abstract QR<T> QR(QRMethod method = QRMethod.Thin);

        /// <summary>
        /// Computes the QR decomposition for a matrix using Modified Gram-Schmidt Orthogonalization.
        /// </summary>
        /// <returns>The QR decomposition object.</returns>
        public abstract GramSchmidt<T> GramSchmidt();

        /// <summary>
        /// Computes the SVD decomposition for a matrix.
        /// </summary>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <returns>The SVD decomposition object.</returns>
        public abstract Svd<T> Svd(bool computeVectors);

        /// <summary>
        /// Computes the EVD decomposition for a matrix.
        /// </summary>
        /// <returns>The EVD decomposition object.</returns>
        public abstract Evd<T> Evd();
    }
}
