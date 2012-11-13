// <copyright file="GramSchmidt.cs" company="Math.NET">
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
    /// <para>A class which encapsulates the functionality of the QR decomposition Modified Gram-Schmidt Orthogonalization.</para>
    /// <para>Any real square matrix A may be decomposed as A = QR where Q is an orthogonal mxn matrix and R is an nxn upper triangular matrix.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by modified Gram-Schmidt Orthogonalization.
    /// </remarks>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class GramSchmidt<T> : QR<T>
    where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Internal method which routes the call to perform the QR factorization to the appropriate class.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>A QR factorization object.</returns>
        internal static GramSchmidt<T> Create(Matrix<T> matrix)
        {
            if (typeof(T) == typeof(double))
            {
                var dense = matrix as LinearAlgebra.Double.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Double.Factorization.DenseGramSchmidt(dense) as GramSchmidt<T>;
                }

                return new LinearAlgebra.Double.Factorization.UserGramSchmidt(matrix as Matrix<double>) as GramSchmidt<T>;
            }

            if (typeof(T) == typeof(float))
            {
                var dense = matrix as LinearAlgebra.Single.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Single.Factorization.DenseGramSchmidt(dense) as GramSchmidt<T>;
                }

                return new LinearAlgebra.Single.Factorization.UserGramSchmidt(matrix as Matrix<float>) as GramSchmidt<T>;
            }

            if (typeof(T) == typeof(Complex))
            {
                var dense = matrix as LinearAlgebra.Complex.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Complex.Factorization.DenseGramSchmidt(dense) as GramSchmidt<T>;
                }

                return new LinearAlgebra.Complex.Factorization.UserGramSchmidt(matrix as Matrix<Complex>) as GramSchmidt<T>;
            }

            if (typeof(T) == typeof(Complex32))
            {
                var dense = matrix as LinearAlgebra.Complex32.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Complex32.Factorization.DenseGramSchmidt(dense) as GramSchmidt<T>;
                }

                return new LinearAlgebra.Complex32.Factorization.UserGramSchmidt(matrix as Matrix<Complex32>) as GramSchmidt<T>;
            }

            throw new NotSupportedException();
        }
    }
}
