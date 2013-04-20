// <copyright file="ILinearAlgebraProvider.cs" company="Math.NET">
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

// INITIAL DRAFT MISSING EXCEPTION SPECIFICATIONS
namespace MathNet.Numerics.Algorithms.LinearAlgebra
{
    using System.Numerics;

    /// <summary>
    /// Interface to linear algebra algorithms that work off 1-D arrays.
    /// </summary>
    public interface ILinearAlgebraProvider : ILinearAlgebraProvider<double>, ILinearAlgebraProvider<float>, ILinearAlgebraProvider<Complex>, ILinearAlgebraProvider<Complex32>
    {
        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        float MatrixNorm(Norm norm, int rows, int columns, float[] matrix, float[] work);

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        double MatrixNorm(Norm norm, int rows, int columns, double[] matrix, double[] work);

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        Complex32 MatrixNorm(Norm norm, int rows, int columns, Complex32[] matrix, float[] work);

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        Complex MatrixNorm(Norm norm, int rows, int columns, Complex[] matrix, double[] work);

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of a matrix.
        /// </summary>
        /// <param name="isSymmetric">Wether the matrix is symmetric or not.</param>
        /// <param name="order">The order of the matrix.</param>
        /// <param name="matrix">The matrix to decompose. The lenth of the array must be order * order.</param>
        /// <param name="matrixEv">On output, the matrix contains the eigen vectors. The lenth of the array must be order * order.</param>
        /// <param name="vectorEv">On output, the eigen values (λ) of matrix in ascending value. The length of the arry must <paramref name="order"/>.</param>
        /// <param name="matrixD">On output, the block diagonal eigenvalue matrix. The lenth of the array must be order * order.</param>
        void EigenDecomp(bool isSymmetric, int order, float[] matrix, float[] matrixEv, Complex[] vectorEv, float[] matrixD);

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of a matrix.
        /// </summary>
        /// <param name="isSymmetric">Wether the matrix is symmetric or not.</param>
        /// <param name="order">The order of the matrix.</param>
        /// <param name="matrix">The matrix to decompose. The lenth of the array must be order * order.</param>
        /// <param name="matrixEv">On output, the matrix contains the eigen vectors. The lenth of the array must be order * order.</param>
        /// <param name="vectorEv">On output, the eigen values (λ) of matrix in ascending value. The length of the arry must <paramref name="order"/>.</param>
        /// <param name="matrixD">On output, the block diagonal eigenvalue matrix. The lenth of the array must be order * order.</param>
        void EigenDecomp(bool isSymmetric, int order, double[] matrix, double[] matrixEv, Complex[] vectorEv, double[] matrixD);

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of a matrix.
        /// </summary>
        /// <param name="isSymmetric">Wether the matrix is symmetric or not.</param>
        /// <param name="order">The order of the matrix.</param>
        /// <param name="matrix">The matrix to decompose. The lenth of the array must be order * order.</param>
        /// <param name="matrixEv">On output, the matrix contains the eigen vectors. The lenth of the array must be order * order.</param>
        /// <param name="vectorEv">On output, the eigen values (λ) of matrix in ascending value. The length of the arry must <paramref name="order"/>.</param>
        /// <param name="matrixD">On output, the block diagonal eigenvalue matrix. The lenth of the array must be order * order.</param>
        void EigenDecomp(bool isSymmetric, int order, Complex32[] matrix, Complex32[] matrixEv, Complex[] vectorEv, Complex32[] matrixD);

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of a matrix.
        /// </summary>
        /// <param name="isSymmetric">Wether the matrix is symmetric or not.</param>
        /// <param name="order">The order of the matrix.</param>
        /// <param name="matrix">The matrix to decompose. The lenth of the array must be order * order.</param>
        /// <param name="matrixEv">On output, the matrix contains the eigen vectors. The lenth of the array must be order * order.</param>
        /// <param name="vectorEv">On output, the eigen values (λ) of matrix in ascending value. The length of the arry must <paramref name="order"/>.</param>
        /// <param name="matrixD">On output, the block diagonal eigenvalue matrix. The lenth of the array must be order * order.</param>
        void EigenDecomp(bool isSymmetric, int order, Complex[] matrix, Complex[] matrixEv, Complex[] vectorEv, Complex[] matrixD);
    }
}
