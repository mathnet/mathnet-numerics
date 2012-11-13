// <copyright file="AcmlLinearAlgebraProvider.Complex.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2011 Math.NET
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

using MathNet.Numerics.LinearAlgebra.Generic.Factorization;

namespace MathNet.Numerics.Algorithms.LinearAlgebra.Acml
{
    using System;
    using System.Numerics;
    using System.Security;
    using Properties;

    /// <summary>
    /// AMD Core Math Library (ACML) linear algebra provider.
    /// </summary>
    public partial class AcmlLinearAlgebraProvider : ManagedLinearAlgebraProvider
    {
        /// <summary>
        /// Computes the dot product of x and y.
        /// </summary>
        /// <param name="x">The vector x.</param>
        /// <param name="y">The vector y.</param>
        /// <returns>The dot product of x and y.</returns>
        /// <remarks>This is equivalent to the DOT BLAS routine.</remarks>
        [SecuritySafeCritical]
        public override Complex DotProduct(Complex[] x, Complex[] y)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (x.Length != y.Length)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength);
            }

            return SafeNativeMethods.z_dot_product(x.Length, x, y);
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
        public override void AddVectorToScaledVector(Complex[] y, Complex alpha, Complex[] x, Complex[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (y.Length != x.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (!ReferenceEquals(y, result))
            {
                Array.Copy(y, 0, result, 0, y.Length);
            }

            if (alpha == Complex.Zero)
            {
                return;
            }

            SafeNativeMethods.z_axpy(y.Length, alpha, x, result);
        }

        /// <summary>
        /// Scales an array. Can be used to scale a vector and a matrix.
        /// </summary>
        /// <param name="alpha">The scalar.</param>
        /// <param name="x">The values to scale.</param>
        /// <param name="result">This result of the scaling.</param>
        /// <remarks>This is similar to the SCAL BLAS routine.</remarks>
        [SecuritySafeCritical]
        public override void ScaleArray(Complex alpha, Complex[] x, Complex[] result)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (!ReferenceEquals(x, result))
            {
                Array.Copy(x, 0, result, 0, x.Length);
            }

            if (alpha == Complex.One)
            {
                return;
            }

            SafeNativeMethods.z_scale(x.Length, alpha, result);
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
        /// set to Complex.One and beta set to Complex.Zero, and x and y are not transposed.</remarks>
        public override void MatrixMultiply(Complex[] x, int rowsX, int columnsX, Complex[] y, int rowsY, int columnsY, Complex[] result)
        {
            MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, Complex.One, x, rowsX, columnsX, y, rowsY, columnsY, Complex.Zero, result);
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
        public override void MatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, Complex alpha, Complex[] a, int rowsA, int columnsA, Complex[] b, int rowsB, int columnsB, Complex beta, Complex[] c)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (c == null)
            {
                throw new ArgumentNullException("c");
            }

            var m = transposeA == Transpose.DontTranspose ? rowsA : columnsA;
            var n = transposeB == Transpose.DontTranspose ? columnsB : rowsB;
            var k = transposeA == Transpose.DontTranspose ? columnsA : rowsA;
            var l = transposeB == Transpose.DontTranspose ? rowsB : columnsB;

            if (c.Length != m * n)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (k != l)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            SafeNativeMethods.z_matrix_multiply(transposeA, transposeB, m, n, k, alpha, a, b, beta, c);
        }

        /// <summary>
        /// Computes the LUP factorization of A. P*A = L*U.
        /// </summary>
        /// <param name="data">An <paramref name="order"/> by <paramref name="order"/> matrix. The matrix is overwritten with the
        /// the LU factorization on exit. The lower triangular factor L is stored in under the diagonal of <paramref name="data"/> (the diagonal is always Complex.One
        /// for the L factor). The upper triangular factor U is stored on and above the diagonal of <paramref name="data"/>.</param>
        /// <param name="order">The order of the square matrix <paramref name="data"/>.</param>
        /// <param name="ipiv">On exit, it contains the pivot indices. The size of the array must be <paramref name="order"/>.</param>
        /// <remarks>This is equivalent to the GETRF LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public override void LUFactor(Complex[] data, int order, int[] ipiv)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (ipiv == null)
            {
                throw new ArgumentNullException("ipiv");
            }

            if (data.Length != order * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "data");
            }

            if (ipiv.Length != order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "ipiv");
            }

            SafeNativeMethods.z_lu_factor(order, data, ipiv);
        }

        /// <summary>
        /// Computes the inverse of matrix using LU factorization.
        /// </summary>
        /// <param name="a">The N by N matrix to invert. Contains the inverse On exit.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <remarks>This is equivalent to the GETRF and GETRI LAPACK routines.</remarks>
        [SecuritySafeCritical]
        public override void LUInverse(Complex[] a, int order)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (a.Length != order * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            var work = new Complex[order];
            SafeNativeMethods.z_lu_inverse(order, a, work, work.Length);
        }

        /// <summary>
        /// Computes the inverse of a previously factored matrix.
        /// </summary>
        /// <param name="a">The LU factored N by N matrix.  Contains the inverse On exit.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <remarks>This is equivalent to the GETRI LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public override void LUInverseFactored(Complex[] a, int order, int[] ipiv)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (ipiv == null)
            {
                throw new ArgumentNullException("ipiv");
            }

            if (a.Length != order * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            if (ipiv.Length != order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "ipiv");
            }

            var work = new Complex[order];
            SafeNativeMethods.z_lu_inverse_factored(order, a, ipiv, work, order);
        }

        /// <summary>
        /// Computes the inverse of matrix using LU factorization.
        /// </summary>
        /// <param name="a">The N by N matrix to invert. Contains the inverse On exit.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>This is equivalent to the GETRF and GETRI LAPACK routines.</remarks>
        [SecuritySafeCritical]
        public override void LUInverse(Complex[] a, int order, Complex[] work)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (a.Length != order * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            if (work == null)
            {
                throw new ArgumentNullException("work");
            }

            if (work.Length < order)
            {
                throw new ArgumentException(Resources.WorkArrayTooSmall, "work");
            }

            SafeNativeMethods.z_lu_inverse(order, a, work, work.Length);
        }

        /// <summary>
        /// Computes the inverse of a previously factored matrix.
        /// </summary>
        /// <param name="a">The LU factored N by N matrix.  Contains the inverse On exit.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>This is equivalent to the GETRI LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public override void LUInverseFactored(Complex[] a, int order, int[] ipiv, Complex[] work)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (ipiv == null)
            {
                throw new ArgumentNullException("ipiv");
            }

            if (a.Length != order * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            if (ipiv.Length != order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "ipiv");
            }

            if (work == null)
            {
                throw new ArgumentNullException("work");
            }

            if (work.Length < order)
            {
                throw new ArgumentException(Resources.WorkArrayTooSmall, "work");
            }

            SafeNativeMethods.z_lu_inverse_factored(order, a, ipiv, work, order);
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
        public override void LUSolve(int columnsOfB, Complex[] a, int order, Complex[] b)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (a.Length != order * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            if (b.Length != columnsOfB * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException(Resources.ArgumentReferenceDifferent);
            }

            SafeNativeMethods.z_lu_solve(order, columnsOfB, a, b);
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
        public override void LUSolveFactored(int columnsOfB, Complex[] a, int order, int[] ipiv, Complex[] b)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (ipiv == null)
            {
                throw new ArgumentNullException("ipiv");
            }

            if (a.Length != order * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            if (ipiv.Length != order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "ipiv");
            }

            if (b.Length != columnsOfB * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException(Resources.ArgumentReferenceDifferent);
            }

            SafeNativeMethods.z_lu_solve_factored(order, columnsOfB, a, ipiv, b);
        }

        /// <summary>
        /// Computes the Cholesky factorization of A.
        /// </summary>
        /// <param name="a">On entry, a square, positive definite matrix. On exit, the matrix is overwritten with the
        /// the Cholesky factorization.</param>
        /// <param name="order">The number of rows or columns in the matrix.</param>
        /// <remarks>This is equivalent to the POTRF LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public override void CholeskyFactor(Complex[] a, int order)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (order < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "order");
            }

            if (a.Length != order * order)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            var info = SafeNativeMethods.z_cholesky_factor(order, a);

            if (info > 0)
            {
                throw new ArgumentException(Resources.ArgumentMatrixPositiveDefinite);
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
        public override void CholeskySolve(Complex[] a, int orderA, Complex[] b, int columnsB)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (b.Length != orderA * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException(Resources.ArgumentReferenceDifferent);
            }

            SafeNativeMethods.z_cholesky_solve(orderA, columnsB, a, b);
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
        public override void CholeskySolveFactored(Complex[] a, int orderA, Complex[] b, int columnsB)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (b.Length != orderA * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException(Resources.ArgumentReferenceDifferent);
            }

            SafeNativeMethods.z_cholesky_solve_factored(orderA, columnsB, a, b);
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
        public override void QRFactor(Complex[] r, int rowsR, int columnsR, Complex[] q, Complex[] tau)
        {
            if (r == null)
            {
                throw new ArgumentNullException("r");
            }

            if (q == null)
            {
                throw new ArgumentNullException("q");
            }

            if (r.Length != rowsR * columnsR)
            {
                throw new ArgumentException(string.Format(Resources.ArgumentArrayWrongLength, "rowsR * columnsR"), "r");
            }

            if (tau.Length < Math.Min(rowsR, columnsR))
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, "min(m,n)"), "tau");
            }

            if (q.Length != rowsR * rowsR)
            {
                throw new ArgumentException(string.Format(Resources.ArgumentArrayWrongLength, "rowsR * rowsR"), "q");
            }

            var work = new Complex[columnsR * Control.BlockSize];
            SafeNativeMethods.z_qr_factor(rowsR, columnsR, r, tau, q, work, work.Length);
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
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>This is similar to the GEQRF and ORGQR LAPACK routines.</remarks>
        [SecuritySafeCritical]
        public override void QRFactor(Complex[] r, int rowsR, int columnsR, Complex[] q, Complex[] tau, Complex[] work)
        {
            if (r == null)
            {
                throw new ArgumentNullException("r");
            }

            if (q == null)
            {
                throw new ArgumentNullException("q");
            }

            if (work == null)
            {
                throw new ArgumentNullException("work");
            }

            if (r.Length != rowsR * columnsR)
            {
                throw new ArgumentException(string.Format(Resources.ArgumentArrayWrongLength, "rowsR * columnsR"), "r");
            }

            if (tau.Length < Math.Min(rowsR, columnsR))
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, "min(m,n)"), "tau");
            }

            if (q.Length != rowsR * rowsR)
            {
                throw new ArgumentException(string.Format(Resources.ArgumentArrayWrongLength, "rowsR * rowsR"), "q");
            }

            if (work.Length < columnsR * Control.BlockSize)
            {
                work[0] = columnsR * Control.BlockSize;
                throw new ArgumentException(Resources.WorkArrayTooSmall, "work");
            }

            SafeNativeMethods.z_qr_factor(rowsR, columnsR, r, tau, q, work, work.Length);
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
        /// <remarks>Rows must be greater or equal to columns.</remarks>
        public override void QRSolve(Complex[] a, int rows, int columns, Complex[] b, int columnsB, Complex[] x)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (a.Length != rows * columns)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            if (b.Length != rows * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (x.Length != columns * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "x");
            }

            if (rows < columns)
            {
                throw new ArgumentException(Resources.RowsLessThanColumns);
            }

            var work = new Complex[columns * Control.BlockSize];
            QRSolve(a, rows, columns, b, columnsB, x, work);
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
        /// <param name="work">The work array. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>Rows must be greater or equal to columns.</remarks>
        public override void QRSolve(Complex[] a, int rows, int columns, Complex[] b, int columnsB, Complex[] x, Complex[] work)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (work == null)
            {
                throw new ArgumentNullException("work");
            }

            if (a.Length != rows * columns)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "a");
            }

            if (b.Length != rows * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (x.Length != columns * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "x");
            }

            if (rows < columns)
            {
                throw new ArgumentException(Resources.RowsLessThanColumns);
            }

            if (work.Length < 1)
            {
                work[0] = rows * Control.BlockSize;
                throw new ArgumentException(Resources.WorkArrayTooSmall, "work");
            }

            SafeNativeMethods.z_qr_solve(rows, columns, columnsB, a, b, x, work, work.Length);
        }

        /// <summary>
        /// Solves A*X=B for X using a previously QR factored matrix.
        /// </summary>
        /// <param name="q">The Q matrix obtained by calling <see cref="QRFactor(Complex[],int,int,Complex[],Complex[])"/>.</param>
        /// <param name="r">The R matrix obtained by calling <see cref="QRFactor(Complex[],int,int,Complex[],Complex[])"/>. </param>
        /// <param name="rowsR">The number of rows in the A matrix.</param>
        /// <param name="columnsR">The number of columns in the A matrix.</param>
        /// <param name="tau">Contains additional information on Q. Only used for the native solver
        /// and can be <c>null</c> for the managed provider.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="columnsB">The number of columns of B.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        /// <remarks>Rows must be greater or equal to columns.</remarks>
        [SecuritySafeCritical]
        public override void QRSolveFactored(Complex[] q, Complex[] r, int rowsR, int columnsR, Complex[] tau, Complex[] b, int columnsB, Complex[] x)
        {
            if (r == null)
            {
                throw new ArgumentNullException("r");
            }

            if (q == null)
            {
                throw new ArgumentNullException("q");
            }

            if (b == null)
            {
                throw new ArgumentNullException("q");
            }

            if (x == null)
            {
                throw new ArgumentNullException("q");
            }

            if (r.Length != rowsR * columnsR)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "r");
            }

            if (q.Length != rowsR * rowsR)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "q");
            }

            if (b.Length != rowsR * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (x.Length != columnsR * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "x");
            }

            if (rowsR < columnsR)
            {
                throw new ArgumentException(Resources.RowsLessThanColumns);
            }

            var work = new Complex[columnsR * Control.BlockSize];
            QRSolveFactored(q, r, rowsR, columnsR, tau, b, columnsB, x, work);
        }

        /// <summary>
        /// Solves A*X=B for X using a previously QR factored matrix.
        /// </summary>
        /// <param name="q">The Q matrix obtained by QR factor. This is only used for the managed provider and can be
        /// <c>null</c> for the native provider. The native provider uses the Q portion stored in the R matrix.</param>
        /// <param name="r">The R matrix obtained by calling <see cref="QRFactor(Complex[],int,int,Complex[],Complex[])"/>. </param>
        /// <param name="rowsR">The number of rows in the A matrix.</param>
        /// <param name="columnsR">The number of columns in the A matrix.</param>
        /// <param name="tau">Contains additional information on Q. Only used for the native solver
        /// and can be <c>null</c> for the managed provider.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <param name="columnsB">The number of columns of B.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        /// <param name="work">The work array - only used in the native provider. The array must have a length of at least N,
        /// but should be N*blocksize. The blocksize is machine dependent. On exit, work[0] contains the optimal
        /// work size value.</param>
        /// <remarks>Rows must be greater or equal to columns.</remarks>
        public override void QRSolveFactored(Complex[] q, Complex[] r, int rowsR, int columnsR, Complex[] tau, Complex[] b, int columnsB, Complex[] x, Complex[] work)
        {
            if (r == null)
            {
                throw new ArgumentNullException("r");
            }

            if (q == null)
            {
                throw new ArgumentNullException("q");
            }

            if (b == null)
            {
                throw new ArgumentNullException("q");
            }

            if (x == null)
            {
                throw new ArgumentNullException("q");
            }

            if (work == null)
            {
                throw new ArgumentNullException("work");
            }

            if (r.Length != rowsR * columnsR)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "r");
            }

            if (q.Length != rowsR * rowsR)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "q");
            }

            if (b.Length != rowsR * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (x.Length != columnsR * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "x");
            }

            if (rowsR < columnsR)
            {
                throw new ArgumentException(Resources.RowsLessThanColumns);
            }

            if (work.Length < 1)
            {
                work[0] = rowsR * Control.BlockSize;
                throw new ArgumentException(Resources.WorkArrayTooSmall, "work");
            }

            SafeNativeMethods.z_qr_solve_factored(rowsR, columnsR, columnsB, r, b, tau, x, work, work.Length);
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
        public override void SingularValueDecomposition(bool computeVectors, Complex[] a, int rowsA, int columnsA, Complex[] s, Complex[] u, Complex[] vt)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (u == null)
            {
                throw new ArgumentNullException("u");
            }

            if (vt == null)
            {
                throw new ArgumentNullException("vt");
            }

            if (u.Length != rowsA * rowsA)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "u");
            }

            if (vt.Length != columnsA * columnsA)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "vt");
            }

            if (s.Length != Math.Min(rowsA, columnsA))
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "s");
            }

            var work = new Complex[(2 * Math.Min(rowsA, columnsA)) + Math.Max(rowsA, columnsA)];
            SingularValueDecomposition(computeVectors, a, rowsA, columnsA, s, u, vt, work);
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
        public override void SvdSolve(Complex[] a, int rowsA, int columnsA, Complex[] b, int columnsB, Complex[] x)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (b.Length != rowsA * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            if (x.Length != columnsA * columnsB)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "b");
            }

            var work = new Complex[(2 * Math.Min(rowsA, columnsA)) + Math.Max(rowsA, columnsA)];
            var s = new Complex[Math.Min(rowsA, columnsA)];
            var u = new Complex[rowsA * rowsA];
            var vt = new Complex[columnsA * columnsA];

            var clone = new Complex[a.Length];
            a.Copy(clone);
            SingularValueDecomposition(true, clone, rowsA, columnsA, s, u, vt, work);
            SvdSolveFactored(rowsA, columnsA, s, u, vt, b, columnsB, x);
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
        /// <param name="work">The work array. For real matrices, the work array should be at least
        /// Max(3*Min(M, N) + Max(M, N), 5*Min(M,N)). For complex matrices, 2*Min(M, N) + Max(M, N).
        /// On exit, work[0] contains the optimal work size value.</param>
        /// <remarks>This is equivalent to the GESVD LAPACK routine.</remarks>
        [SecuritySafeCritical]
        public override void SingularValueDecomposition(bool computeVectors, Complex[] a, int rowsA, int columnsA, Complex[] s, Complex[] u, Complex[] vt, Complex[] work)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (u == null)
            {
                throw new ArgumentNullException("u");
            }

            if (vt == null)
            {
                throw new ArgumentNullException("vt");
            }

            if (work == null)
            {
                throw new ArgumentNullException("work");
            }

            if (u.Length != rowsA * rowsA)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "u");
            }

            if (vt.Length != columnsA * columnsA)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "vt");
            }

            if (s.Length != Math.Min(rowsA, columnsA))
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "s");
            }

            if (work.Length == 0)
            {
                throw new ArgumentException(Resources.ArgumentSingleDimensionArray, "work");
            }

            if (work.Length < (2 * Math.Min(rowsA, columnsA)) + Math.Max(rowsA, columnsA))
            {
                work[0] = (2 * Math.Min(rowsA, columnsA)) + Math.Max(rowsA, columnsA);
                throw new ArgumentException(Resources.WorkArrayTooSmall, "work");
            }

            SafeNativeMethods.z_svd_factor(computeVectors, rowsA, columnsA, a, s, u, vt, work, work.Length);
        }
    }
}
