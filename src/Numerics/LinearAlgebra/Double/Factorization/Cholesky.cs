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

namespace MathNet.Numerics.LinearAlgebra.Double.Decomposition
{
    using System;
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky decomposition.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky decomposition
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky decomposition is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    public abstract class Cholesky
    {
        /// <summary>
        /// Internal method which routes the call to perform the Cholesky factorization to the appropriate class.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>A cholesky factorization object.</returns>
        internal static Cholesky Create(Matrix matrix)
        {
            var dense = matrix as DenseMatrix;
            if (dense != null)
            {
                return new DenseCholesky(dense);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// A reference to the lower triangular form of the Cholesky matrix.
        /// </summary>
        public abstract Matrix Factor { get; }

        /// <summary>
        /// The determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public abstract double Determinant { get; }

        /// <summary>
        /// The log determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public abstract double DeterminantLn { get; }
    }
}
