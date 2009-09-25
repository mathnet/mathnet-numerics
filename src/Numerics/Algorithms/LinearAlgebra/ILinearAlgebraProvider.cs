// <copyright file="ILinearAlgebraProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
// Copyright (c) 2009 Math.NET
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

// INITIAL DRAFT MISSING EXCEPTION SPECIFICATIONS
namespace MathNet.Numerics.Algorithms.LinearAlgebra
{
    /// <summary>
    /// How to transpose a matrix.
    /// </summary>
    public enum Transpose
    {
        /// <summary>
        /// Don't transpose a matrix.
        /// </summary>
        DontTranspose = 111,

        /// <summary>
        /// Transpose a matrix.
        /// </summary>
        Transpose = 112,

        /// <summary>
        /// Conjugate transpose a complex matrix.
        /// </summary>
        /// <remarks>If a conjugate transpose is used with a real matrix, then the matrix is just transposed.</remarks>
        ConjugateTranspose = 113
    }

    /// <summary>
    /// Types of matrix norms.
    /// </summary>
    public enum Norm : byte
    {
        /// <summary>
        /// The 1-norm.
        /// </summary>
        OneNorm = (byte)'1',

        /// <summary>
        /// The Frobenius norm.
        /// </summary>
        FrobeniusNorm = (byte)'f',

        /// <summary>
        /// The infinity norm.
        /// </summary>
        InfinityNorm = (byte)'i',

        /// <summary>
        /// The largest absolute value norm.
        /// </summary>
        LargestAbsoluteValue = (byte)'m'
    }

    /// <summary>
    /// Interface to linear algebra algorithms that work off 1-D arrays.
    /// </summary>
    public interface ILinearAlgebraProvider
    {
        /// <summary>
        /// Queries the provider for the optimal, workspace block size
        /// for the given routine.
        /// </summary>
        /// <param name="methodName">Name of the method to query.</param>
        /// <returns>-1 if the provider cannot compute the workspace size; otherwise
        /// the suggested block size.</returns>
        int QueryWorkspaceBlockSize(string methodName);
        
        /// <summary>
        /// Adds a scaled vector to another: <c>y += alpha*x</c>.
        /// </summary>
        /// <param name="y">The vector to update.</param>
        /// <param name="alpha">The value to scale <paramref name="x"/> by.</param>
        /// <param name="x">The vector to add to <paramref name="y"/>.</param>
        /// <remarks>This is equivalent to the AXPY BLAS routine.</remarks>
        void AddVectorToScaledVector(double[] y, double alpha, double[] x);

        /// <summary>
        /// Scales an array. Can be used to scale a vector and a matrix.
        /// </summary>
        /// <param name="alpha">The scalar.</param>
        /// <param name="x">The values to scale.</param>
        /// <remarks>This is equivalent to the SCAL BLAS routine.</remarks>
        void ScaleArray(double alpha, double[] x);

        /// <summary>
        /// Computes the dot product of x and y.
        /// </summary>
        /// <param name="x">The vector x.</param>
        /// <param name="y">The vector y.</param>
        /// <returns>The dot product of x and y.</returns>
        /// <remarks>This is equivalent to the DOT BLAS routine.</remarks>
        double DotProduct(double[] x, double[] y);

        /// <summary>
        /// Does a point wise add of two arrays <c>z = x + y</c>. This can be used 
        /// to add vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the addition.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        void AddArrays(double[] x, double[] y, double[] result);

        /// <summary>
        /// Does a point wise subtraction of two arrays <c>z = x - y</c>. This can be used 
        /// to subtract vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the subtraction.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        void SubtractArrays(double[] x, double[] y, double[] result);

        /// <summary>
        /// Does a point wise multiplication of two arrays <c>z = x * y</c>. This can be used
        /// to multiple elements of vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the point wise multiplication.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        void PointWiseMultiplyArrays(double[] x, double[] y, double[] result);

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        double MatrixNorm(Norm norm, double[] matrix);

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        double MatrixNorm(Norm norm, double[] matrix, double[] work);

        /// <summary>
        /// Multiples two matrices. <c>result = x * y</c>
        /// </summary>
        /// <param name="x">The x matrix.</param>
        /// <param name="y">The y matrix.</param>
        /// <param name="result">Where to store the result of the multiplication.</param>
        /// <remarks>This is a simplified version of the BLAS GEMM routine with alpha
        /// set to 1.0 and beta set to 0.0, and x and y are not transposed.</remarks>
        void MatrixMultiply(double[] x, double[] y, double[] result);

        /// <summary>
        /// Multiplies two matrices and updates another with the result. <c>c = alpha*op(a)*op(b) + beta*c</c>
        /// </summary>
        /// <param name="transposeA">How to transpose the <paramref name="a"/> matrix.</param>
        /// <param name="transposeB">How to transpose the <paramref name="b"/> matrix.</param>
        /// <param name="alpha">The value to scale <paramref name="a"/> matrix.</param>
        /// <param name="a">The a matrix.</param>
        /// <param name="b">The b matrix</param>
        /// <param name="beta">The value to scale the <paramref name="c"/> matrix.</param>
        /// <param name="c">The c matrix.</param>
        void MatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, double alpha, double[] a, double[] b, double beta, double[] c);

        /// <summary>
        /// Computes the LU factorization of A.
        /// </summary>
        /// <param name="a">An m by n matrix. The matrix is overwritten with the
        /// the LU factorization On exit.</param>
        /// <param name="ipiv">On exit, it contains the pivot indices. The size
        /// of the array must be min(m,n).</param>
        /// <remarks>This is equivalent to the GETRF LAPACK routine.</remarks>
        void LUFactor(double[] a, int[] ipiv);

        /// <summary>
        /// Computes the inverse of matrix using LU factorization.
        /// </summary>
        /// <param name="a">The N by N matrix to invert. Contains the inverse On exit.</param>
        /// <remarks>This is equivalent to the GETRF and GETRI LAPACK routines.</remarks>
        void LUInverse(double[] a);

        /// <summary>
        /// Computes the inverse of a previously factored matrix.
        /// </summary>
        /// <param name="a">The LU factored N by N matrix.  Contains the inverse On exit.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <remarks>This is equivalent to the GETRI LAPACK routine.</remarks>
        void LUInverseFactored(double[] a, int[] ipiv);

        /// <summary>
        /// Computes the inverse of matrix using LU factorization.
        /// </summary>
        /// <param name="a">The N by N matrix to invert. Contains the inverse On exit.</param>
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. Use <see cref="QueryWorkspaceBlockSize"/>
        /// to determine the optimal size of the work array. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>This is equivalent to the GETRF and GETRI LAPACK routines.</remarks>
        void LUInverse(double[] a, double[] work);

        /// <summary>
        /// Computes the inverse of a previously factored matrix.
        /// </summary>
        /// <param name="a">The LU factored N by N matrix.  Contains the inverse On exit.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. Use <see cref="QueryWorkspaceBlockSize"/>
        /// to determine the optimal size of the work array. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>This is equivalent to the GETRI LAPACK routine.</remarks>
        void LUInverseFactored(double[] a, int[] ipiv, double[] work);

        /// <summary>
        /// Solves A*X=B for X using LU factorization.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The square matrix A.</param>
        /// <param name="b">The B matrix.</param>
        /// <remarks>This is equivalent to the GETRF and GETRS LAPACK routines.</remarks>
        void LUSolve(int columnsOfB, double[] a, double[] b);

        /// <summary>
        /// Solves A*X=B for X using a previously factored A matrix.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The factored A matrix.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <param name="b">The B matrix.</param>
        /// <remarks>This is equivalent to the GETRS LAPACK routine.</remarks>
        void LUSolveFactored(int columnsOfB, double[] a, int ipiv, double[] b);

        /// <summary>
        /// Solves A*X=B for X using LU factorization.
        /// </summary>
        /// <param name="transposeA">How to transpose the <paramref name="a"/> matrix.</param>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The square matrix A.</param>
        /// <param name="b">The B matrix.</param>
        /// <remarks>This is equivalent to the GETRF and GETRS LAPACK routines.</remarks>
        void LUSolve(Transpose transposeA, int columnsOfB, double[] a, double[] b);

        /// <summary>
        /// Solves A*X=B for X using a previously factored A matrix.
        /// </summary>
        /// <param name="transposeA">How to transpose the <paramref name="a"/> matrix.</param>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The factored A matrix.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <param name="b">The B matrix.</param>
        /// <remarks>This is equivalent to the GETRS LAPACK routine.</remarks>
        void LUSolveFactored(Transpose transposeA, int columnsOfB, double[] a, int ipiv, double[] b);

        /// <summary>
        /// Computes the Cholesky factorization of A.
        /// </summary>
        /// <param name="a">On entry, a square, positive definite matrix. On exit, the matrix is overwritten with the
        /// the Cholesky factorization.</param>
        /// <remarks>This is equivalent to the POTRF LAPACK routine.</remarks>
        void CholeskyFactor(double[] a);

        /// <summary>
        /// Solves A*X=B for X using Cholesky factorization.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The square, positive definite matrix A.</param>
        /// <param name="b">The B matrix.</param>
        /// <remarks>This is equivalent to the POTRF add POTRS LAPACK routines.</remarks>
        void CholeskySolve(int columnsOfB, double[] a, double[] b);

        /// <summary>
        /// Solves A*X=B for X using a previously factored A matrix.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The factored A matrix.</param>
        /// <param name="b">The B matrix.</param>
        /// <remarks>This is equivalent to the POTRS LAPACK routine.</remarks>
        void CholeskySolveFactored(int columnsOfB, double[] a, double[] b);

        /// <summary>
        /// Computes the QR factorization of A.
        /// </summary>
        /// <param name="r">On entry, it is the M by N A matrix to factor. On exit,
        /// it is overwritten with the R matrix of the QR factorization. </param>
        /// <param name="q">On exit, A M by M matrix that holds the Q matrix of the
        /// QR factorization.</param>
        /// <remarks>This is similar to the GEQRF and ORGQR LAPACK routines.</remarks>
        void QRFactor(double[] r, double[] q);

        /// <summary>
        /// Computes the QR factorization of A.
        /// </summary>
        /// <param name="r">On entry, it is the M by N A matrix to factor. On exit,
        /// it is overwritten with the R matrix of the QR factorization. </param>
        /// <param name="q">On exit, A M by M matrix that holds the Q matrix of the 
        /// QR factorization.</param>
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. Use <see cref="QueryWorkspaceBlockSize"/>
        /// to determine the optimal size of the work array. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>This is similar to the GEQRF and ORGQR LAPACK routines.</remarks>
        void QRFactor(double[] r, double[] q, double[] work);

        /// <summary>
        /// Solves A*X=B for X using QR factorization of A.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="r">On entry, it is the M by N A matrix to factor. On exit,
        /// it is overwritten with the R matrix of the QR factorization. </param>
        /// <param name="q">On exit, A M by M matrix that holds the Q matrix of the 
        /// QR factorization.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        void QRSolve(int columnsOfB, double[] r, double[] q, double[] b, double[] x);

        /// <summary>
        /// Solves A*X=B for X using QR factorization of A.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="r">On entry, it is the M by N A matrix to factor. On exit,
        /// it is overwritten with the R matrix of the QR factorization. </param>
        /// <param name="q">On exit, A M by M matrix that holds the Q matrix of the 
        /// QR factorization.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. Use <see cref="QueryWorkspaceBlockSize"/>
        /// to determine the optimal size of the work array. On exit, work[0] contains the optimal
        /// work size value.</param>
        void QRSolve(int columnsOfB, double[] r, double[] q, double[] b, double[] x, double[] work);

        /// <summary>
        /// Solves A*X=B for X using a previously QR factored matrix.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="q">The Q matrix obtained by calling <see cref="QRFactor(double[],double[])"/>.</param>
        /// <param name="r">The R matrix obtained by calling <see cref="QRFactor(double[],double[])"/>. </param>
        /// <param name="b">The B matrix.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        void QRSolveFactored(int columnsOfB, double[] q, double[] r, double[] b, double[] x);

        /// <summary>
        /// Computes the singular value decomposition of A.
        /// </summary>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <param name="a">On entry, the M by N matrix to decompose. On exit, A may be overwritten.</param>
        /// <param name="s">The singular values of A in ascending value. </param>
        /// <param name="u">If <paramref name="computeVectors"/> is true, on exit U contains the left
        /// singular vectors.</param>
        /// <param name="vt">If <paramref name="computeVectors"/> is true, on exit VT contains the transposed
        /// right singular vectors.</param>
        /// <remarks>This is equivalent to the GESVD LAPACK routine.</remarks>
        void SinguarValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt);

        /// <summary>
        /// Computes the singular value decomposition of A.
        /// </summary>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <param name="a">On entry, the M by N matrix to decompose. On exit, A may be overwritten.</param>
        /// <param name="s">The singular values of A in ascending value. </param>
        /// <param name="u">If <paramref name="computeVectors"/> is true, on exit U contains the left
        /// singular vectors.</param>
        /// <param name="vt">If <paramref name="computeVectors"/> is true, on exit VT contains the transposed
        /// right singular vectors.</param>
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. Use <see cref="QueryWorkspaceBlockSize"/>
        /// to determine the optimal size of the work array. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>This is equivalent to the GESVD LAPACK routine.</remarks>
        void SingularValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt, double[] work);

        /// <summary>
        /// Solves A*X=B for X using the singular value decomposition of A.
        /// </summary>
        /// <param name="a">On entry, the M by N matrix to decompose. On exit, A may be overwritten.</param>
        /// <param name="s">The singular values of A in ascending value. </param>
        /// <param name="u">On exit U contains the left singular vectors.</param>
        /// <param name="vt">On exit VT contains the transposed right singular vectors.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        void SvdSolve(double[] a, double[] s, double[] u, double[] vt, double[] b, double[] x);

        /// <summary>
        /// Solves A*X=B for X using the singular value decomposition of A.
        /// </summary>
        /// <param name="a">On entry, the M by N matrix to decompose. On exit, A may be overwritten.</param>
        /// <param name="s">The singular values of A in ascending value. </param>
        /// <param name="u">On exit U contains the left singular vectors.</param>
        /// <param name="vt">On exit VT contains the transposed right singular vectors.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. Use <see cref="QueryWorkspaceBlockSize"/>
        /// to determine the optimal size of the work array. On exit, work[0] contains the optimal
        /// work size value.</param>
        void SvdSolve(double[] a, double[] s, double[] u, double[] vt, double[] b, double[] x, double[] work);

        /// <summary>
        /// Solves A*X=B for X using a previously SVD decomposed matrix.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="s">The s values returned by <see cref="SinguarValueDecomposition(bool,double[],double[],double[],double[])"/>.</param>
        /// <param name="u">The left singular vectors returned by  <see cref="SinguarValueDecomposition(bool,double[],double[],double[],double[])"/>.</param>
        /// <param name="vt">The right singular  vectors returned by  <see cref="SinguarValueDecomposition(bool,double[],double[],double[],double[])"/>.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        void SvdSolveFactored(int columnsOfB, double[] s, double[] u, double[] vt, double[] b, double[] x);
    }
}
