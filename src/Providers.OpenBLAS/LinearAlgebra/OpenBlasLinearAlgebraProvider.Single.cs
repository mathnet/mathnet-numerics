// <copyright file="OpenBlasLinearAlgebraProvider.Single.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using System.Security;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Providers.LinearAlgebra;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Providers.OpenBLAS.LinearAlgebra
{
    /// <summary>
    /// OpenBLAS linear algebra provider.
    /// </summary>
    internal partial class OpenBlasLinearAlgebraProvider
    {
        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public double MatrixNorm(Norm norm, int rows, int columns, float[] matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            if (rows <= 0)
            {
                throw new ArgumentException("Value must be positive.", nameof(rows));
            }

            if (columns <= 0)
            {
                throw new ArgumentException("Value must be positive.", nameof(columns));
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException($"The given array is too small. It must be at least {rows * columns} long.", nameof(matrix));
            }

            return SafeNativeMethods.s_matrix_norm((byte)norm, rows, columns, matrix);
        }

        /// <summary>
        /// Conjugates an array. Can be used to conjugate a vector and a matrix.
        /// </summary>
        /// <param name="x">The values to conjugate.</param>
        /// <param name="result">This result of the conjugation.</param>
        public void ConjugateArray(float[] x, float[] result)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (!ReferenceEquals(x, result))
            {
                x.CopyTo(result, 0);
            }
        }

        /// <summary>
        /// Computes the dot product of x and y.
        /// </summary>
        /// <param name="x">The vector x.</param>
        /// <param name="y">The vector y.</param>
        /// <returns>The dot product of x and y.</returns>
        /// <remarks>This is equivalent to the DOT BLAS routine.</remarks>
        [SecuritySafeCritical]
        public float DotProduct(float[] x, float[] y)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (x.Length != y.Length)
            {
                throw new ArgumentException("The array arguments must have the same length.");
            }

            return SafeNativeMethods.s_dot_product(x.Length, x, y);
        }

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
        public void AddArrays(float[] x, float[] y, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i] + y[i];
            }
        }

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
        public void SubtractArrays(float[] x, float[] y, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i] - y[i];
            }
        }

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
        public void PointWiseMultiplyArrays(float[] x, float[] y, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i] * y[i];
            }
        }

        /// <summary>
        /// Does a point wise division of two arrays <c>z = x / y</c>. This can be used
        /// to divide elements of vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the point wise division.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void PointWiseDivideArrays(float[] x, float[] y, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i] / y[i];
            }
        }

        /// <summary>
        /// Does a point wise power of two arrays <c>z = x ^ y</c>. This can be used
        /// to raise elements of vectors or matrices to the powers of another vector or matrix.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the point wise power.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void PointWisePowerArrays(float[] x, float[] y, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (float)Math.Pow(x[i], y[i]);
            }
        }

        /// <summary>
        /// Adds a scaled vector to another: <c>result = y + alpha*x</c>.
        /// </summary>
        /// <param name="y">The vector to update.</param>
        /// <param name="alpha">The value to scale <paramref name="x"/> by.</param>
        /// <param name="x">The vector to add to <paramref name="y"/>.</param>
        /// <param name="result">The result of the addition.</param>
        /// <remarks>This is similar to the AXPY BLAS routine.</remarks>
        [SecuritySafeCritical]
        public void AddVectorToScaledVector(float[] y, float alpha, float[] x, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y.Length != x.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (!ReferenceEquals(y, result))
            {
                Array.Copy(y, 0, result, 0, y.Length);
            }

            if (alpha == 0.0f)
            {
                return;
            }

            SafeNativeMethods.s_axpy(y.Length, alpha, x, result);
        }

        /// <summary>
        /// Scales an array. Can be used to scale a vector and a matrix.
        /// </summary>
        /// <param name="alpha">The scalar.</param>
        /// <param name="x">The values to scale.</param>
        /// <param name="result">This result of the scaling.</param>
        /// <remarks>This is similar to the SCAL BLAS routine.</remarks>
        [SecuritySafeCritical]
        public void ScaleArray(float alpha, float[] x, float[] result)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (!ReferenceEquals(x, result))
            {
                Array.Copy(x, 0, result, 0, x.Length);
            }

            if (alpha == 1.0f)
            {
                return;
            }

            SafeNativeMethods.s_scale(x.Length, alpha, result);
        }

        /// <summary>
        /// Multiples two matrices. <c>result = x * y</c>
        /// </summary>
        /// <param name="x">The x matrix.</param>
        /// <param name="rowsX">The number of rows in the x matrix.</param>
        /// <param name="columnsX">The number of columns in the x matrix.</param>
        /// <param name="y">The y matrix.</param>
        /// <param name="rowsY">The number of rows in the y matrix.</param>
        /// <param name="columnsY">The number of columns in the y matrix.</param>
        /// <param name="result">Where to store the result of the multiplication.</param>
        /// <remarks>This is a simplified version of the BLAS GEMM routine with alpha
        /// set to 1.0f and beta set to 0.0f, and x and y are not transposed.</remarks>
        public void MatrixMultiply(float[] x, int rowsX, int columnsX, float[] y, int rowsY, int columnsY, float[] result)
        {
            MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 1.0f, x, rowsX, columnsX, y, rowsY, columnsY, 0.0f, result);
        }

        /// <summary>
        /// Multiplies two matrices and updates another with the result. <c>c = alpha*op(a)*op(b) + beta*c</c>
        /// </summary>
        /// <param name="transposeA">How to transpose the <paramref name="a"/> matrix.</param>
        /// <param name="transposeB">How to transpose the <paramref name="b"/> matrix.</param>
        /// <param name="alpha">The value to scale <paramref name="a"/> matrix.</param>
        /// <param name="a">The a matrix.</param>
        /// <param name="rowsA">The number of rows in the <paramref name="a"/> matrix.</param>
        /// <param name="columnsA">The number of columns in the <paramref name="a"/> matrix.</param>
        /// <param name="b">The b matrix</param>
        /// <param name="rowsB">The number of rows in the <paramref name="b"/> matrix.</param>
        /// <param name="columnsB">The number of columns in the <paramref name="b"/> matrix.</param>
        /// <param name="beta">The value to scale the <paramref name="c"/> matrix.</param>
        /// <param name="c">The c matrix.</param>
        [SecuritySafeCritical]
        public void MatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, float alpha, float[] a, int rowsA, int columnsA, float[] b, int rowsB, int columnsB, float beta, float[] c)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (c == null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            var m = transposeA == Transpose.DontTranspose ? rowsA : columnsA;
            var n = transposeB == Transpose.DontTranspose ? columnsB : rowsB;
            var k = transposeA == Transpose.DontTranspose ? columnsA : rowsA;
            var l = transposeB == Transpose.DontTranspose ? rowsB : columnsB;

            if (c.Length != m*n)
            {
                throw new ArgumentException("Matrix dimensions must agree.");
            }

            if (k != l)
            {
                throw new ArgumentException("Matrix dimensions must agree.");
            }

            SafeNativeMethods.s_matrix_multiply(transposeA, transposeB, m, n, k, alpha, a, b, beta, c);
        }

        /// <summary>
        /// Computes the LUP factorization of A. P*A = L*U.
        /// </summary>
        /// <param name="data">An <paramref name="order"/> by <paramref name="order"/> matrix. The matrix is overwritten with the
        /// the LU factorization on exit. The lower triangular factor L is stored in under the diagonal of <paramref name="data"/> (the diagonal is always 1.0f
        /// for the L factor). The upper triangular factor U is stored on and above the diagonal of <paramref name="data"/>.</param>
        /// <param name="order">The order of the square matrix <paramref name="data"/>.</param>
        /// <param name="ipiv">On exit, it contains the pivot indices. The size of the array must be <paramref name="order"/>.</param>
        /// <remarks>This is equivalent to the GETRF LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public void LUFactor(float[] data, int order, int[] ipiv)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (ipiv == null)
            {
                throw new ArgumentNullException(nameof(ipiv));
            }

            if (data.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(data));
            }

            if (ipiv.Length != order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
            }

            var info = SafeNativeMethods.s_lu_factor(order, data, ipiv);

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }
        }

        /// <summary>
        /// Computes the inverse of matrix using LU factorization.
        /// </summary>
        /// <param name="a">The N by N matrix to invert. Contains the inverse On exit.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <remarks>This is equivalent to the GETRF and GETRI LAPACK routines.</remarks>
        [SecuritySafeCritical]
        public void LUInverse(float[] a, int order)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (a.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            var info = SafeNativeMethods.s_lu_inverse(order, a);

            if (info == (int)NativeError.MemoryAllocation)
            {
                throw new MemoryAllocationException();
            }

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }

            if (info > 0)
            {
                throw new SingularUMatrixException(info);
            }
        }

        /// <summary>
        /// Computes the inverse of a previously factored matrix.
        /// </summary>
        /// <param name="a">The LU factored N by N matrix.  Contains the inverse On exit.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <remarks>This is equivalent to the GETRI LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public void LUInverseFactored(float[] a, int order, int[] ipiv)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (ipiv == null)
            {
                throw new ArgumentNullException(nameof(ipiv));
            }

            if (a.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            if (ipiv.Length != order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
            }

            var info = SafeNativeMethods.s_lu_inverse_factored(order, a, ipiv);

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }

            if (info > 0)
            {
                throw new SingularUMatrixException(info);
            }
        }

        /// <summary>
        /// Solves A*X=B for X using LU factorization.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The square matrix A.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <remarks>This is equivalent to the GETRF and GETRS LAPACK routines.</remarks>
        [SecuritySafeCritical]
        public void LUSolve(int columnsOfB, float[] a, int order, float[] b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (a.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            if (b.Length != columnsOfB*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException("Arguments must be different objects.");
            }

            var info = SafeNativeMethods.s_lu_solve(order, columnsOfB, a, b);

            if (info == (int)NativeError.MemoryAllocation)
            {
                throw new MemoryAllocationException();
            }

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }
        }

        /// <summary>
        /// Solves A*X=B for X using a previously factored A matrix.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The factored A matrix.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <remarks>This is equivalent to the GETRS LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public void LUSolveFactored(int columnsOfB, float[] a, int order, int[] ipiv, float[] b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (ipiv == null)
            {
                throw new ArgumentNullException(nameof(ipiv));
            }

            if (a.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            if (ipiv.Length != order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
            }

            if (b.Length != columnsOfB*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException("Arguments must be different objects.");
            }

            var info = SafeNativeMethods.s_lu_solve_factored(order, columnsOfB, a, ipiv, b);

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }
        }

        /// <summary>
        /// Computes the Cholesky factorization of A.
        /// </summary>
        /// <param name="a">On entry, a square, positive definite matrix. On exit, the matrix is overwritten with the
        /// the Cholesky factorization.</param>
        /// <param name="order">The number of rows or columns in the matrix.</param>
        /// <remarks>This is equivalent to the POTRF LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public void CholeskyFactor(float[] a, int order)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (order < 1)
            {
                throw new ArgumentException("Value must be positive.", nameof(order));
            }

            if (a.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            var info = SafeNativeMethods.s_cholesky_factor(order, a);

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }

            if (info > 0)
            {
                throw new ArgumentException("Matrix must be positive definite.");
            }
        }

        /// <summary>
        /// Solves A*X=B for X using Cholesky factorization.
        /// </summary>
        /// <param name="a">The square, positive definite matrix A.</param>
        /// <param name="orderA">The number of rows and columns in A.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <param name="columnsB">The number of columns in the B matrix.</param>
        /// <remarks>This is equivalent to the POTRF add POTRS LAPACK routines.
        /// </remarks>
        [SecuritySafeCritical]
        public void CholeskySolve(float[] a, int orderA, float[] b, int columnsB)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (b.Length != orderA*columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException("Arguments must be different objects.");
            }

            var info = SafeNativeMethods.s_cholesky_solve(orderA, columnsB, a, b);

            if (info == (int)NativeError.MemoryAllocation)
            {
                throw new MemoryAllocationException();
            }

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }
        }

        /// <summary>
        /// Solves A*X=B for X using a previously factored A matrix.
        /// </summary>
        /// <param name="a">The square, positive definite matrix A.</param>
        /// <param name="orderA">The number of rows and columns in A.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <param name="columnsB">The number of columns in the B matrix.</param>
        /// <remarks>This is equivalent to the POTRS LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public void CholeskySolveFactored(float[] a, int orderA, float[] b, int columnsB)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (b.Length != orderA*columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException("Arguments must be different objects.");
            }

            var info = SafeNativeMethods.s_cholesky_solve_factored(orderA, columnsB, a, b);

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }
        }

        /// <summary>
        /// Computes the QR factorization of A.
        /// </summary>
        /// <param name="r">On entry, it is the M by N A matrix to factor. On exit,
        /// it is overwritten with the R matrix of the QR factorization. </param>
        /// <param name="rowsR">The number of rows in the A matrix.</param>
        /// <param name="columnsR">The number of columns in the A matrix.</param>
        /// <param name="q">On exit, A M by M matrix that holds the Q matrix of the
        /// QR factorization.</param>
        /// <param name="tau">A min(m,n) vector. On exit, contains additional information
        /// to be used by the QR solve routine.</param>
        /// <remarks>This is similar to the GEQRF and ORGQR LAPACK routines.</remarks>
        [SecuritySafeCritical]
        public void QRFactor(float[] r, int rowsR, int columnsR, float[] q, float[] tau)
        {
            if (r == null)
            {
                throw new ArgumentNullException(nameof(r));
            }

            if (q == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            if (r.Length != rowsR*columnsR)
            {
                throw new ArgumentException("The given array has the wrong length. Should be rowsR * columnsR.", nameof(r));
            }

            if (tau.Length < Math.Min(rowsR, columnsR))
            {
                throw new ArgumentException("The given array is too small. It must be at least min(m,n) long.", nameof(tau));
            }

            if (q.Length != rowsR*rowsR)
            {
                throw new ArgumentException("The given array has the wrong length. Should be rowsR * rowsR.", nameof(q));
            }

            var info = SafeNativeMethods.s_qr_factor(rowsR, columnsR, r, tau, q);

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }
        }

        /// <summary>
        /// Computes the thin QR factorization of A where M &gt; N.
        /// </summary>
        /// <param name="q">On entry, it is the M by N A matrix to factor. On exit,
        /// it is overwritten with the Q matrix of the QR factorization.</param>
        /// <param name="rowsA">The number of rows in the A matrix.</param>
        /// <param name="columnsA">The number of columns in the A matrix.</param>
        /// <param name="r">On exit, A N by N matrix that holds the R matrix of the
        /// QR factorization.</param>
        /// <param name="tau">A min(m,n) vector. On exit, contains additional information
        /// to be used by the QR solve routine.</param>
        /// <remarks>This is similar to the GEQRF and ORGQR LAPACK routines.</remarks>
        [SecuritySafeCritical]
        public void ThinQRFactor(float[] q, int rowsA, int columnsA, float[] r, float[] tau)
        {
            if (r == null)
            {
                throw new ArgumentNullException(nameof(r));
            }

            if (q == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            if (q.Length != rowsA * columnsA)
            {
                throw new ArgumentException("The given array has the wrong length. Should be rowsR * columnsR.", nameof(q));
            }

            if (tau.Length < Math.Min(rowsA, columnsA))
            {
                throw new ArgumentException("The given array is too small. It must be at least min(m,n) long.", nameof(tau));
            }

            if (r.Length != columnsA * columnsA)
            {
                throw new ArgumentException("The given array has the wrong length. Should be columnsA * columnsA.", nameof(r));
            }

            var info = SafeNativeMethods.s_qr_thin_factor(rowsA, columnsA, q, tau, r);

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }
        }

        /// <summary>
        /// Solves A*X=B for X using QR factorization of A.
        /// </summary>
        /// <param name="a">The A matrix.</param>
        /// <param name="rows">The number of rows in the A matrix.</param>
        /// <param name="columns">The number of columns in the A matrix.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="columnsB">The number of columns of B.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        /// <param name="method">The type of QR factorization to perform. <seealso cref="QRMethod"/></param>
        /// <remarks>Rows must be greater or equal to columns.</remarks>
        [SecuritySafeCritical]
        public void QRSolve(float[] a, int rows, int columns, float[] b, int columnsB, float[] x, QRMethod method = QRMethod.Full)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (a.Length != rows * columns)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            if (b.Length != rows * columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (x.Length != columns * columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(x));
            }

            if (rows < columns)
            {
                throw new ArgumentException("The number of rows must greater than or equal to the number of columns.");
            }

            var info = SafeNativeMethods.s_qr_solve(rows, columns, columnsB, a, b, x);

            if (info == (int)NativeError.MemoryAllocation)
            {
                throw new MemoryAllocationException();
            }

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }

            if (info > 0)
            {
                throw new ArgumentException("Matrix must not be rank deficient.", nameof(a));
            }
        }

        /// <summary>
        /// Solves A*X=B for X using a previously QR factored matrix.
        /// </summary>
        /// <param name="q">The Q matrix obtained by calling <see cref="QRFactor(float[],int,int,float[],float[])"/>.</param>
        /// <param name="r">The R matrix obtained by calling <see cref="QRFactor(float[],int,int,float[],float[])"/>. </param>
        /// <param name="rowsA">The number of rows in the A matrix.</param>
        /// <param name="columnsA">The number of columns in the A matrix.</param>
        /// <param name="tau">Contains additional information on Q. Only used for the native solver
        /// and can be <c>null</c> for the managed provider.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="columnsB">The number of columns of B.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        /// <param name="method">The type of QR factorization to perform. <seealso cref="QRMethod"/></param>
        /// <remarks>Rows must be greater or equal to columns.</remarks>
        [SecuritySafeCritical]
        public void QRSolveFactored(float[] q, float[] r, int rowsA, int columnsA, float[] tau, float[] b, int columnsB, float[] x, QRMethod method = QRMethod.Full)
        {
            if (r == null)
            {
                throw new ArgumentNullException(nameof(r));
            }

            if (q == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            int rowsQ, columnsQ, rowsR, columnsR;
            if (method == QRMethod.Full)
            {
                rowsQ = columnsQ = rowsR = rowsA;
                columnsR = columnsA;
            }
            else
            {
                rowsQ = rowsA;
                columnsQ = rowsR = columnsR = columnsA;
            }

            if (r.Length != rowsR * columnsR)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {rowsR * columnsR}.", nameof(r));
            }

            if (q.Length != rowsQ * columnsQ)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {rowsQ * columnsQ}.", nameof(q));
            }

            if (b.Length != rowsA * columnsB)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {rowsA * columnsB}.", nameof(b));
            }

            if (x.Length != columnsA * columnsB)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {columnsA * columnsB}.", nameof(x));
            }

            if (method == QRMethod.Full)
            {
                var info = SafeNativeMethods.s_qr_solve_factored(rowsA, columnsA, columnsB, r, b, tau, x);

                if (info == (int)NativeError.MemoryAllocation)
                {
                    throw new MemoryAllocationException();
                }

                if (info < 0)
                {
                    throw new InvalidParameterException(Math.Abs(info));
                }
            }
            else
            {
                // we don't have access to the raw Q matrix any more(it is stored in R in the full QR), need to think about this.
                // let just call the managed version in the meantime. The heavy lifting has already been done. -marcus
                ManagedLinearAlgebraProvider.Instance.QRSolveFactored(q, r, rowsA, columnsA, tau, b, columnsB, x, QRMethod.Thin);
            }
        }

        /// <summary>
        /// Solves A*X=B for X using the singular value decomposition of A.
        /// </summary>
        /// <param name="a">On entry, the M by N matrix to decompose.</param>
        /// <param name="rowsA">The number of rows in the A matrix.</param>
        /// <param name="columnsA">The number of columns in the A matrix.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="columnsB">The number of columns of B.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        public void SvdSolve(float[] a, int rowsA, int columnsA, float[] b, int columnsB, float[] x)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (b.Length != rowsA*columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (x.Length != columnsA*columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            var s = new float[Math.Min(rowsA, columnsA)];
            var u = new float[rowsA*rowsA];
            var vt = new float[columnsA*columnsA];

            var clone = new float[a.Length];
            a.Copy(clone);
            SingularValueDecomposition(true, clone, rowsA, columnsA, s, u, vt);
            SvdSolveFactored(rowsA, columnsA, s, u, vt, b, columnsB, x);
        }

        /// <summary>
        /// Solves A*X=B for X using a previously SVD decomposed matrix.
        /// </summary>
        /// <param name="rowsA">The number of rows in the A matrix.</param>
        /// <param name="columnsA">The number of columns in the A matrix.</param>
        /// <param name="s">The s values returned by <see cref="SingularValueDecomposition(bool,float[],int,int,float[],float[],float[])"/>.</param>
        /// <param name="u">The left singular vectors returned by  <see cref="SingularValueDecomposition(bool,float[],int,int,float[],float[],float[])"/>.</param>
        /// <param name="vt">The right singular  vectors returned by  <see cref="SingularValueDecomposition(bool,float[],int,int,float[],float[],float[])"/>.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="columnsB">The number of columns of B.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        public void SvdSolveFactored(int rowsA, int columnsA, float[] s, float[] u, float[] vt, float[] b, int columnsB, float[] x)
        {
            ManagedLinearAlgebraProvider.Instance.SvdSolveFactored(rowsA, columnsA, s, u, vt, b, columnsB, x);
        }

        /// <summary>
        /// Computes the singular value decomposition of A.
        /// </summary>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <param name="a">On entry, the M by N matrix to decompose. On exit, A may be overwritten.</param>
        /// <param name="rowsA">The number of rows in the A matrix.</param>
        /// <param name="columnsA">The number of columns in the A matrix.</param>
        /// <param name="s">The singular values of A in ascending value.</param>
        /// <param name="u">If <paramref name="computeVectors"/> is <c>true</c>, on exit U contains the left
        /// singular vectors.</param>
        /// <param name="vt">If <paramref name="computeVectors"/> is <c>true</c>, on exit VT contains the transposed
        /// right singular vectors.</param>
        /// <remarks>This is equivalent to the GESVD LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public void SingularValueDecomposition(bool computeVectors, float[] a, int rowsA, int columnsA, float[] s, float[] u, float[] vt)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (u == null)
            {
                throw new ArgumentNullException(nameof(u));
            }

            if (vt == null)
            {
                throw new ArgumentNullException(nameof(vt));
            }

            if (u.Length != rowsA*rowsA)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(u));
            }

            if (vt.Length != columnsA*columnsA)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(vt));
            }

            if (s.Length != Math.Min(rowsA, columnsA))
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(s));
            }

            var info = SafeNativeMethods.s_svd_factor(computeVectors, rowsA, columnsA, a, s, u, vt);

            if (info == (int)NativeError.MemoryAllocation)
            {
                throw new MemoryAllocationException();
            }

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }

            if (info > 0)
            {
                throw new NonConvergenceException();
            }
        }

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of a matrix.
        /// </summary>
        /// <param name="isSymmetric">Whether the matrix is symmetric or not.</param>
        /// <param name="order">The order of the matrix.</param>
        /// <param name="matrix">The matrix to decompose. The length of the array must be order * order.</param>
        /// <param name="matrixEv">On output, the matrix contains the eigen vectors. The length of the array must be order * order.</param>
        /// <param name="vectorEv">On output, the eigen values (λ) of matrix in ascending value. The length of the array must <paramref name="order"/>.</param>
        /// <param name="matrixD">On output, the block diagonal eigenvalue matrix. The length of the array must be order * order.</param>
        public void EigenDecomp(bool isSymmetric, int order, float[] matrix, float[] matrixEv, Complex[] vectorEv, float[] matrixD)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            if (matrix.Length != order*order)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {order * order}.", nameof(matrix));
            }

            if (matrixEv == null)
            {
                throw new ArgumentNullException(nameof(matrixEv));
            }

            if (matrixEv.Length != order*order)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {order * order}.", nameof(matrixEv));
            }

            if (vectorEv == null)
            {
                throw new ArgumentNullException(nameof(vectorEv));
            }

            if (vectorEv.Length != order)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {order}.", nameof(vectorEv));
            }

            if (matrixD == null)
            {
                throw new ArgumentNullException(nameof(matrixD));
            }

            if (matrixD.Length != order*order)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {order * order}.", nameof(matrixD));
            }

            var info = SafeNativeMethods.s_eigen(isSymmetric, order, matrix, matrixEv, vectorEv, matrixD);

            if (info == (int)NativeError.MemoryAllocation)
            {
                throw new MemoryAllocationException();
            }

            if (info < 0)
            {
                throw new InvalidParameterException(Math.Abs(info));
            }

            if (info > 0)
            {
                throw new NonConvergenceException();
            }
        }
    }
}
