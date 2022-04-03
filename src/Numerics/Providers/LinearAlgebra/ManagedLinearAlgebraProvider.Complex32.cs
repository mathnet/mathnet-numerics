// <copyright file="ManagedLinearAlgebraProvider.Complex32.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2021 Math.NET
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
using MathNet.Numerics.Threading;
using Complex = System.Numerics.Complex;
using QRMethod = MathNet.Numerics.LinearAlgebra.Factorization.QRMethod;
using static System.FormattableString;

namespace MathNet.Numerics.Providers.LinearAlgebra
{
    /// <summary>
    /// The managed linear algebra provider.
    /// </summary>
    public partial class ManagedLinearAlgebraProvider
    {
        /// <summary>
        /// Adds a scaled vector to another: <c>result = y + alpha*x</c>.
        /// </summary>
        /// <param name="y">The vector to update.</param>
        /// <param name="alpha">The value to scale <paramref name="x"/> by.</param>
        /// <param name="x">The vector to add to <paramref name="y"/>.</param>
        /// <param name="result">The result of the addition.</param>
        /// <remarks>This is similar to the AXPY BLAS routine.</remarks>
        public void AddVectorToScaledVector(Complex32[] y, Complex32 alpha, Complex32[] x, Complex32[] result)
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

            if (alpha.IsZero())
            {
                y.Copy(result);
            }
            else if (alpha.IsOne())
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = y[i] + x[i];
                }
            }
            else
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = y[i] + (alpha * x[i]);
                }
            }

        }

        /// <summary>
        /// Scales an array. Can be used to scale a vector and a matrix.
        /// </summary>
        /// <param name="alpha">The scalar.</param>
        /// <param name="x">The values to scale.</param>
        /// <param name="result">This result of the scaling.</param>
        /// <remarks>This is similar to the SCAL BLAS routine.</remarks>
        public void ScaleArray(Complex32 alpha, Complex32[] x, Complex32[] result)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (alpha.IsZero())
            {
                Array.Clear(result, 0, result.Length);
            }
            else if (alpha.IsOne())
            {
                x.Copy(result);
            }
            else
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = alpha * x[i];
                }
            }
        }

        /// <summary>
        /// Conjugates an array. Can be used to conjugate a vector and a matrix.
        /// </summary>
        /// <param name="x">The values to conjugate.</param>
        /// <param name="result">This result of the conjugation.</param>
        public void ConjugateArray(Complex32[] x, Complex32[] result)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i].Conjugate();
            }
        }

        /// <summary>
        /// Computes the dot product of x and y.
        /// </summary>
        /// <param name="x">The vector x.</param>
        /// <param name="y">The vector y.</param>
        /// <returns>The dot product of x and y.</returns>
        /// <remarks>This is equivalent to the DOT BLAS routine.</remarks>
        public Complex32 DotProduct(Complex32[] x, Complex32[] y)
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

            Complex32 d = new Complex32(0.0F, 0.0F);
            for (var i = 0; i < y.Length; i++)
            {
                d += y[i]*x[i];
            }

            return d;
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
        public void AddArrays(Complex32[] x, Complex32[] y, Complex32[] result)
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
        public void SubtractArrays(Complex32[] x, Complex32[] y, Complex32[] result)
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
        public void PointWiseMultiplyArrays(Complex32[] x, Complex32[] y, Complex32[] result)
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
        public void PointWiseDivideArrays(Complex32[] x, Complex32[] y, Complex32[] result)
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

            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    result[i] = x[i] / y[i];
                }
            });
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
        public void PointWisePowerArrays(Complex32[] x, Complex32[] y, Complex32[] result)
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

            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    result[i] = Complex32.Pow(x[i], y[i]);
                }
            });
        }

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <returns>The requested <see cref="Norm"/> of the matrix.</returns>
        public double MatrixNorm(Norm norm, int rows, int columns, Complex32[] matrix)
        {
            switch (norm)
            {
                case Norm.OneNorm:
                    var norm1 = 0d;
                    for (var j = 0; j < columns; j++)
                    {
                        var s = 0d;
                        for (var i = 0; i < rows; i++)
                        {
                            s += matrix[(j*rows) + i].Magnitude;
                        }

                        norm1 = Math.Max(norm1, s);
                    }
                    return norm1;
                case Norm.LargestAbsoluteValue:
                    var normMax = 0d;
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            normMax = Math.Max(matrix[(j * rows) + i].Magnitude, normMax);
                        }
                    }
                    return normMax;
                case Norm.InfinityNorm:
                    var r = new double[rows];
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            r[i] += matrix[(j * rows) + i].Magnitude;
                        }
                    }
                    // TODO: reuse
                    var max = r[0];
                    for (int i = 0; i < r.Length; i++)
                    {
                        if (r[i] > max)
                        {
                            max = r[i];
                        }
                    }
                    return max;
                case Norm.FrobeniusNorm:
                    var aat = new Complex32[rows*rows];
                    MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.ConjugateTranspose, 1.0f, matrix, rows, columns, matrix, rows, columns, 0.0f, aat);
                    var normF = 0d;
                    for (var i = 0; i < rows; i++)
                    {
                        normF += aat[(i * rows) + i].Magnitude;
                    }
                    return Math.Sqrt(normF);
                default:
                    throw new NotSupportedException();
            }
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
        /// set to 1.0 and beta set to 0.0, and x and y are not transposed.</remarks>
        public void MatrixMultiply(Complex32[] x, int rowsX, int columnsX, Complex32[] y, int rowsY, int columnsY, Complex32[] result)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (columnsX != rowsY)
            {
                throw new ArgumentOutOfRangeException(Invariant($"columnsA ({columnsX}) != rowsB ({rowsY})"));
            }

            if (rowsX * columnsX != x.Length)
            {
                throw new ArgumentOutOfRangeException(Invariant($"rowsA ({rowsX}) * columnsA ({columnsX}) != a.Length ({x.Length})"));
            }

            if (rowsY * columnsY != y.Length)
            {
                throw new ArgumentOutOfRangeException(Invariant($"rowsB ({rowsY}) * columnsB ({columnsY}) != b.Length ({y.Length})"));
            }

            if (rowsX * columnsY != result.Length)
            {
                throw new ArgumentOutOfRangeException(Invariant($"rowsA ({rowsX}) * columnsB ({columnsY}) != c.Length ({result.Length})"));
            }

            // handle degenerate cases
            Array.Clear(result, 0, result.Length);

            // Extract column arrays
            var columnDataB = new Complex32[columnsY][];
            for (int i = 0; i < columnDataB.Length; i++)
            {
                var column = new Complex32[rowsY];
                GetColumn(Transpose.DontTranspose, i, rowsY, columnsY, y, column);
                columnDataB[i] = column;
            }

            var shouldNotParallelize = rowsX + columnsY + columnsX < Control.ParallelizeOrder || Control.MaxDegreeOfParallelism < 2;
            if (shouldNotParallelize)
            {
                var row = new Complex32[columnsX];
                for (int i = 0; i < rowsX; i++)
                {
                    GetRow(Transpose.DontTranspose, i, rowsX, columnsX, x, row);
                    for (int j = 0; j < columnsY; j++)
                    {
                        var col = columnDataB[j];
                        Complex32 sum = Complex32.Zero;
                        for (int ii = 0; ii < row.Length; ii++)
                        {
                            sum += row[ii] * col[ii];
                        }

                        result[j * rowsX + i] += Complex32.One * sum;
                    }
                }
            }
            else
            {
                CommonParallel.For(0, rowsX, 1, (u, v) =>
                {
                    var row = new Complex32[columnsX];
                    for (int i = u; i < v; i++)
                    {
                        GetRow(Transpose.DontTranspose, i, rowsX, columnsX, x, row);
                        for (int j = 0; j < columnsY; j++)
                        {
                            var column = columnDataB[j];
                            Complex32 sum = Complex32.Zero;
                            for (int ii = 0; ii < row.Length; ii++)
                            {
                                sum += row[ii] * column[ii];
                            }

                            result[j * rowsX + i] += Complex32.One * sum;
                        }
                    }
                });
            }
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
        public void MatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, Complex32 alpha, Complex32[] a, int rowsA, int columnsA, Complex32[] b, int rowsB, int columnsB, Complex32 beta, Complex32[] c)
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

            if (transposeA != Transpose.DontTranspose)
            {
                (rowsA, columnsA) = (columnsA, rowsA);
            }

            if (transposeB != Transpose.DontTranspose)
            {
                (rowsB, columnsB) = (columnsB, rowsB);
            }

            if (columnsA != rowsB)
            {
                throw new ArgumentOutOfRangeException(Invariant($"columnsA ({columnsA}) != rowsB ({rowsB})"));
            }

            if (rowsA * columnsA != a.Length)
            {
                throw new ArgumentOutOfRangeException(Invariant($"rowsA ({rowsA}) * columnsA ({columnsA}) != a.Length ({a.Length})"));
            }

            if (rowsB * columnsB != b.Length)
            {
                throw new ArgumentOutOfRangeException(Invariant($"rowsB ({rowsB}) * columnsB ({columnsB}) != b.Length ({b.Length})"));
            }

            if (rowsA * columnsB != c.Length)
            {
                throw new ArgumentOutOfRangeException(Invariant($"rowsA ({rowsA}) * columnsB ({columnsB}) != c.Length ({c.Length})"));
            }

            // handle degenerate cases
            if (beta == Complex32.Zero)
            {
                Array.Clear(c, 0, c.Length);
            }
            else if (beta != Complex32.One)
            {
                ScaleArray(beta, c, c);
            }

            if (alpha == Complex32.Zero)
            {
                return;
            }

            // Extract column arrays
            var columnDataB = new Complex32[columnsB][];
            for (int i = 0; i < columnDataB.Length; i++)
            {
                var column = new Complex32[rowsB];
                GetColumn(transposeB, i, rowsB, columnsB, b, column);
                columnDataB[i] = column;
            }

            var shouldNotParallelize = rowsA + columnsB + columnsA < Control.ParallelizeOrder || Control.MaxDegreeOfParallelism < 2;
            if (shouldNotParallelize)
            {
                var row = new Complex32[columnsA];
                for (int i = 0; i < rowsA; i++)
                {
                    GetRow(transposeA, i, rowsA, columnsA, a, row);
                    for (int j = 0; j < columnsB; j++)
                    {
                        var col = columnDataB[j];
                        Complex32 sum = Complex32.Zero;
                        for (int ii = 0; ii < row.Length; ii++)
                        {
                            sum += row[ii] * col[ii];
                        }

                        c[j * rowsA + i] += alpha * sum;
                    }
                }
            }
            else
            {
                CommonParallel.For(0, rowsA, 1, (u, v) =>
                {
                    var row = new Complex32[columnsA];
                    for (int i = u; i < v; i++)
                    {
                        GetRow(transposeA, i, rowsA, columnsA, a, row);
                        for (int j = 0; j < columnsB; j++)
                        {
                            var column = columnDataB[j];
                            Complex32 sum = Complex32.Zero;
                            for (int ii = 0; ii < row.Length; ii++)
                            {
                                sum += row[ii] * column[ii];
                            }

                            c[j * rowsA + i] += alpha * sum;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Computes the LUP factorization of A. P*A = L*U.
        /// </summary>
        /// <param name="data">An <paramref name="order"/> by <paramref name="order"/> matrix. The matrix is overwritten with the
        /// the LU factorization on exit. The lower triangular factor L is stored in under the diagonal of <paramref name="data"/> (the diagonal is always 1.0
        /// for the L factor). The upper triangular factor U is stored on and above the diagonal of <paramref name="data"/>.</param>
        /// <param name="order">The order of the square matrix <paramref name="data"/>.</param>
        /// <param name="ipiv">On exit, it contains the pivot indices. The size of the array must be <paramref name="order"/>.</param>
        /// <remarks>This is equivalent to the GETRF LAPACK routine.</remarks>
        public void LUFactor(Complex32[] data, int order, int[] ipiv)
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

            // Initialize the pivot matrix to the identity permutation.
            for (var i = 0; i < order; i++)
            {
                ipiv[i] = i;
            }

            var vecLUcolj = new Complex32[order];

            // Outer loop.
            for (var j = 0; j < order; j++)
            {
                var indexj = j*order;
                var indexjj = indexj + j;

                // Make a copy of the j-th column to localize references.
                for (var i = 0; i < order; i++)
                {
                    vecLUcolj[i] = data[indexj + i];
                }

                // Apply previous transformations.
                for (var i = 0; i < order; i++)
                {
                    // Most of the time is spent in the following dot product.
                    var kmax = Math.Min(i, j);
                    var s = Complex32.Zero;
                    for (var k = 0; k < kmax; k++)
                    {
                        s += data[(k*order) + i]*vecLUcolj[k];
                    }

                    data[indexj + i] = vecLUcolj[i] -= s;
                }

                // Find pivot and exchange if necessary.
                var p = j;
                for (var i = j + 1; i < order; i++)
                {
                    if (vecLUcolj[i].Magnitude > vecLUcolj[p].Magnitude)
                    {
                        p = i;
                    }
                }

                if (p != j)
                {
                    for (var k = 0; k < order; k++)
                    {
                        var indexk = k*order;
                        var indexkp = indexk + p;
                        var indexkj = indexk + j;
                        (data[indexkp], data[indexkj]) = (data[indexkj], data[indexkp]);
                    }

                    ipiv[j] = p;
                }

                // Compute multipliers.
                if (j < order & data[indexjj] != 0.0f)
                {
                    for (var i = j + 1; i < order; i++)
                    {
                        data[indexj + i] /= data[indexjj];
                    }
                }
            }
        }

        /// <summary>
        /// Computes the inverse of matrix using LU factorization.
        /// </summary>
        /// <param name="a">The N by N matrix to invert. Contains the inverse On exit.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <remarks>This is equivalent to the GETRF and GETRI LAPACK routines.</remarks>
        public void LUInverse(Complex32[] a, int order)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (a.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            var ipiv = new int[order];
            LUFactor(a, order, ipiv);
            LUInverseFactored(a, order, ipiv);
        }

        /// <summary>
        /// Computes the inverse of a previously factored matrix.
        /// </summary>
        /// <param name="a">The LU factored N by N matrix.  Contains the inverse On exit.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
        /// <remarks>This is equivalent to the GETRI LAPACK routine.</remarks>
        public void LUInverseFactored(Complex32[] a, int order, int[] ipiv)
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

            var inverse = new Complex32[a.Length];
            for (var i = 0; i < order; i++)
            {
                inverse[i + (order*i)] = Complex32.One;
            }

            LUSolveFactored(order, a, order, ipiv, inverse);
            inverse.Copy(a);
        }

        /// <summary>
        /// Solves A*X=B for X using LU factorization.
        /// </summary>
        /// <param name="columnsOfB">The number of columns of B.</param>
        /// <param name="a">The square matrix A.</param>
        /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <remarks>This is equivalent to the GETRF and GETRS LAPACK routines.</remarks>
        public void LUSolve(int columnsOfB, Complex32[] a, int order, Complex32[] b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            if (b.Length != order*columnsOfB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException("Arguments must be different objects.");
            }

            var ipiv = new int[order];
            var clone = new Complex32[a.Length];
            a.Copy(clone);
            LUFactor(clone, order, ipiv);
            LUSolveFactored(columnsOfB, clone, order, ipiv, b);
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
        public void LUSolveFactored(int columnsOfB, Complex32[] a, int order, int[] ipiv, Complex32[] b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (ipiv == null)
            {
                throw new ArgumentNullException(nameof(ipiv));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Length != order*order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            if (ipiv.Length != order)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
            }

            if (b.Length != order*columnsOfB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (ReferenceEquals(a, b))
            {
                throw new ArgumentException("Arguments must be different objects.");
            }

            // Compute the column vector  P*B
            for (var i = 0; i < ipiv.Length; i++)
            {
                if (ipiv[i] == i)
                {
                    continue;
                }

                var p = ipiv[i];
                for (var j = 0; j < columnsOfB; j++)
                {
                    var indexk = j*order;
                    var indexkp = indexk + p;
                    var indexkj = indexk + i;
                    (b[indexkp], b[indexkj]) = (b[indexkj], b[indexkp]);
                }
            }

            // Solve L*Y = P*B
            for (var k = 0; k < order; k++)
            {
                var korder = k*order;
                for (var i = k + 1; i < order; i++)
                {
                    for (var j = 0; j < columnsOfB; j++)
                    {
                        var index = j*order;
                        b[i + index] -= b[k + index]*a[i + korder];
                    }
                }
            }

            // Solve U*X = Y;
            for (var k = order - 1; k >= 0; k--)
            {
                var korder = k + (k*order);
                for (var j = 0; j < columnsOfB; j++)
                {
                    b[k + (j*order)] /= a[korder];
                }

                korder = k*order;
                for (var i = 0; i < k; i++)
                {
                    for (var j = 0; j < columnsOfB; j++)
                    {
                        var index = j*order;
                        b[i + index] -= b[k + index]*a[i + korder];
                    }
                }
            }
        }

        /// <summary>
        /// Computes the Cholesky factorization of A.
        /// </summary>
        /// <param name="a">On entry, a square, positive definite matrix. On exit, the matrix is overwritten with the
        /// the Cholesky factorization.</param>
        /// <param name="order">The number of rows or columns in the matrix.</param>
        /// <remarks>This is equivalent to the POTRF LAPACK routine.</remarks>
        public void CholeskyFactor(Complex32[] a, int order)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            var tmpColumn = new Complex32[order];

            // Main loop - along the diagonal
            for (var ij = 0; ij < order; ij++)
            {
                // "Pivot" element
                var tmpVal = a[(ij*order) + ij];

                if (tmpVal.Real > 0.0)
                {
                    tmpVal = tmpVal.SquareRoot();
                    a[(ij*order) + ij] = tmpVal;
                    tmpColumn[ij] = tmpVal;

                    // Calculate multipliers and copy to local column
                    // Current column, below the diagonal
                    for (var i = ij + 1; i < order; i++)
                    {
                        a[(ij*order) + i] /= tmpVal;
                        tmpColumn[i] = a[(ij*order) + i];
                    }

                    // Remaining columns, below the diagonal
                    DoCholeskyStep(a, order, ij + 1, order, tmpColumn, Control.MaxDegreeOfParallelism);
                }
                else
                {
                    throw new ArgumentException("Matrix must be positive definite.");
                }

                for (var i = ij + 1; i < order; i++)
                {
                    a[(i*order) + ij] = 0.0f;
                }
            }
        }

        /// <summary>
        /// Calculate Cholesky step
        /// </summary>
        /// <param name="data">Factor matrix</param>
        /// <param name="rowDim">Number of rows</param>
        /// <param name="firstCol">Column start</param>
        /// <param name="colLimit">Total columns</param>
        /// <param name="multipliers">Multipliers calculated previously</param>
        /// <param name="availableCores">Number of available processors</param>
        static void DoCholeskyStep(Complex32[] data, int rowDim, int firstCol, int colLimit, Complex32[] multipliers, int availableCores)
        {
            var tmpColCount = colLimit - firstCol;

            if ((availableCores > 1) && (tmpColCount > Control.ParallelizeElements))
            {
                var tmpSplit = firstCol + (tmpColCount/3);
                var tmpCores = availableCores/2;

                CommonParallel.Invoke(
                    () => DoCholeskyStep(data, rowDim, firstCol, tmpSplit, multipliers, tmpCores),
                    () => DoCholeskyStep(data, rowDim, tmpSplit, colLimit, multipliers, tmpCores));
            }
            else
            {
                for (var j = firstCol; j < colLimit; j++)
                {
                    var tmpVal = multipliers[j];
                    for (var i = j; i < rowDim; i++)
                    {
                        data[(j*rowDim) + i] -= multipliers[i]*tmpVal.Conjugate();
                    }
                }
            }
        }

        /// <summary>
        /// Solves A*X=B for X using Cholesky factorization.
        /// </summary>
        /// <param name="a">The square, positive definite matrix A.</param>
        /// <param name="orderA">The number of rows and columns in A.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <param name="columnsB">The number of columns in the B matrix.</param>
        /// <remarks>This is equivalent to the POTRF add POTRS LAPACK routines.</remarks>
        public void CholeskySolve(Complex32[] a, int orderA, Complex32[] b, int columnsB)
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

            var clone = new Complex32[a.Length];
            a.Copy(clone);
            CholeskyFactor(clone, orderA);
            CholeskySolveFactored(clone, orderA, b, columnsB);
        }

        /// <summary>
        /// Solves A*X=B for X using a previously factored A matrix.
        /// </summary>
        /// <param name="a">The square, positive definite matrix A.</param>
        /// <param name="orderA">The number of rows and columns in A.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <param name="columnsB">The number of columns in the B matrix.</param>
        /// <remarks>This is equivalent to the POTRS LAPACK routine.</remarks>
        public void CholeskySolveFactored(Complex32[] a, int orderA, Complex32[] b, int columnsB)
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

            CommonParallel.For(0, columnsB, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        DoCholeskySolve(a, orderA, b, i);
                    }
                });
        }

        /// <summary>
        /// Solves A*X=B for X using a previously factored A matrix.
        /// </summary>
        /// <param name="a">The square, positive definite matrix A. Has to be different than <paramref name="b"/>.</param>
        /// <param name="orderA">The number of rows and columns in A.</param>
        /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
        /// <param name="index">The column to solve for.</param>
        static void DoCholeskySolve(Complex32[] a, int orderA, Complex32[] b, int index)
        {
            var cindex = index*orderA;

            // Solve L*Y = B;
            Complex32 sum;
            for (var i = 0; i < orderA; i++)
            {
                sum = b[cindex + i];
                for (var k = i - 1; k >= 0; k--)
                {
                    sum -= a[(k*orderA) + i]*b[cindex + k];
                }

                b[cindex + i] = sum/a[(i*orderA) + i];
            }

            // Solve L'*X = Y;
            for (var i = orderA - 1; i >= 0; i--)
            {
                sum = b[cindex + i];
                var iindex = i*orderA;
                for (var k = i + 1; k < orderA; k++)
                {
                    sum -= a[iindex + k].Conjugate()*b[cindex + k];
                }

                b[cindex + i] = sum/a[iindex + i];
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
        public void QRFactor(Complex32[] r, int rowsR, int columnsR, Complex32[] q, Complex32[] tau)
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

            var work = columnsR > rowsR ? new Complex32[rowsR*rowsR] : new Complex32[rowsR*columnsR];

            CommonParallel.For(0, rowsR, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        q[(i*rowsR) + i] = Complex32.One;
                    }
                });

            var minmn = Math.Min(rowsR, columnsR);
            for (var i = 0; i < minmn; i++)
            {
                GenerateColumn(work, r, rowsR, i, i);
                ComputeQR(work, i, r, i, rowsR, i + 1, columnsR, Control.MaxDegreeOfParallelism);
            }

            for (var i = minmn - 1; i >= 0; i--)
            {
                ComputeQR(work, i, q, i, rowsR, i, rowsR, Control.MaxDegreeOfParallelism);
            }
        }

        /// <summary>
        /// Computes the QR factorization of A.
        /// </summary>
        /// <param name="a">On entry, it is the M by N A matrix to factor. On exit,
        /// it is overwritten with the Q matrix of the QR factorization.</param>
        /// <param name="rowsA">The number of rows in the A matrix.</param>
        /// <param name="columnsA">The number of columns in the A matrix.</param>
        /// <param name="r">On exit, A N by N matrix that holds the R matrix of the
        /// QR factorization.</param>
        /// <param name="tau">A min(m,n) vector. On exit, contains additional information
        /// to be used by the QR solve routine.</param>
        /// <remarks>This is similar to the GEQRF and ORGQR LAPACK routines.</remarks>
        public void ThinQRFactor(Complex32[] a, int rowsA, int columnsA, Complex32[] r, Complex32[] tau)
        {
            if (r == null)
            {
                throw new ArgumentNullException(nameof(r));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (a.Length != rowsA*columnsA)
            {
                throw new ArgumentException("The given array has the wrong length. Should be rowsR * columnsR.", nameof(a));
            }

            if (tau.Length < Math.Min(rowsA, columnsA))
            {
                throw new ArgumentException("The given array is too small. It must be at least min(m,n) long.", nameof(tau));
            }

            if (r.Length != columnsA*columnsA)
            {
                throw new ArgumentException("The given array has the wrong length. Should be columnsA * columnsA.", nameof(r));
            }

            var work = new Complex32[rowsA*columnsA];

            var minmn = Math.Min(rowsA, columnsA);
            for (var i = 0; i < minmn; i++)
            {
                GenerateColumn(work, a, rowsA, i, i);
                ComputeQR(work, i, a, i, rowsA, i + 1, columnsA, Control.MaxDegreeOfParallelism);
            }

            //copy R
            for (var j = 0; j < columnsA; j++)
            {
                var rIndex = j*columnsA;
                var aIndex = j*rowsA;
                for (var i = 0; i < columnsA; i++)
                {
                    r[rIndex + i] = a[aIndex + i];
                }
            }

            //clear A and set diagonals to 1
            Array.Clear(a, 0, a.Length);
            for (var i = 0; i < columnsA; i++)
            {
                a[i*rowsA + i] = Complex32.One;
            }

            for (var i = minmn - 1; i >= 0; i--)
            {
                ComputeQR(work, i, a, i, rowsA, i, columnsA, Control.MaxDegreeOfParallelism);
            }
        }


        #region QR Factor Helper functions

        /// <summary>
        /// Perform calculation of Q or R
        /// </summary>
        /// <param name="work">Work array</param>
        /// <param name="workIndex">Index of column in work array</param>
        /// <param name="a">Q or R matrices</param>
        /// <param name="rowStart">The first row in </param>
        /// <param name="rowCount">The last row</param>
        /// <param name="columnStart">The first column</param>
        /// <param name="columnCount">The last column</param>
        /// <param name="availableCores">Number of available CPUs</param>
        static void ComputeQR(Complex32[] work, int workIndex, Complex32[] a, int rowStart, int rowCount, int columnStart, int columnCount, int availableCores)
        {
            if (rowStart > rowCount || columnStart > columnCount)
            {
                return;
            }

            var tmpColCount = columnCount - columnStart;

            if ((availableCores > 1) && (tmpColCount > 200))
            {
                var tmpSplit = columnStart + (tmpColCount/2);
                var tmpCores = availableCores/2;

                CommonParallel.Invoke(
                    () => ComputeQR(work, workIndex, a, rowStart, rowCount, columnStart, tmpSplit, tmpCores),
                    () => ComputeQR(work, workIndex, a, rowStart, rowCount, tmpSplit, columnCount, tmpCores));
            }
            else
            {
                for (var j = columnStart; j < columnCount; j++)
                {
                    var scale = Complex32.Zero;
                    for (var i = rowStart; i < rowCount; i++)
                    {
                        scale += work[(workIndex*rowCount) + i - rowStart]*a[(j*rowCount) + i];
                    }

                    for (var i = rowStart; i < rowCount; i++)
                    {
                        a[(j*rowCount) + i] -= work[(workIndex*rowCount) + i - rowStart].Conjugate()*scale;
                    }
                }
            }
        }

        /// <summary>
        /// Generate column from initial matrix to work array
        /// </summary>
        /// <param name="work">Work array</param>
        /// <param name="a">Initial matrix</param>
        /// <param name="rowCount">The number of rows in matrix</param>
        /// <param name="row">The first row</param>
        /// <param name="column">Column index</param>
        static void GenerateColumn(Complex32[] work, Complex32[] a, int rowCount, int row, int column)
        {
            var tmp = column*rowCount;
            var index = tmp + row;

            CommonParallel.For(row, rowCount, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        var iIndex = tmp + i;
                        work[iIndex - row] = a[iIndex];
                        a[iIndex] = Complex32.Zero;
                    }
                });

            var norm = Complex32.Zero;
            for (var i = 0; i < rowCount - row; ++i)
            {
                var index1 = tmp + i;
                norm += work[index1].Magnitude*work[index1].Magnitude;
            }

            norm = norm.SquareRoot();
            if (row == rowCount - 1 || norm.Magnitude == 0)
            {
                a[index] = -work[tmp];
                work[tmp] = new Complex32(2.0f, 0).SquareRoot();
                return;
            }

            if (work[tmp].Magnitude != 0.0f)
            {
                norm = norm.Magnitude*(work[tmp]/work[tmp].Magnitude);
            }

            a[index] = -norm;
            CommonParallel.For(0, rowCount - row, 4096, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        work[tmp + i] /= norm;
                    }
                });
            work[tmp] += 1.0f;

            var s = (1.0f/work[tmp]).SquareRoot();
            CommonParallel.For(0, rowCount - row, 4096, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        work[tmp + i] = work[tmp + i].Conjugate()*s;
                    }
                });
        }

        #endregion

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
        public void QRSolve(Complex32[] a, int rows, int columns, Complex32[] b, int columnsB, Complex32[] x, QRMethod method = QRMethod.Full)
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

            if (a.Length != rows*columns)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(a));
            }

            if (b.Length != rows*columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (x.Length != columns*columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(x));
            }

            if (rows < columns)
            {
                throw new ArgumentException("The number of rows must greater than or equal to the number of columns.");
            }

            var work = new Complex32[rows * columns];

            var clone = new Complex32[a.Length];
            a.Copy(clone);

            if (method == QRMethod.Full)
            {
                var q = new Complex32[rows*rows];
                QRFactor(clone, rows, columns, q, work);
                QRSolveFactored(q, clone, rows, columns, null, b, columnsB, x, method);
            }
            else
            {
                var r = new Complex32[columns*columns];
                ThinQRFactor(clone, rows, columns, r, work);
                QRSolveFactored(clone, r, rows, columns, null, b, columnsB, x, method);
            }
        }

        /// <summary>
        /// Solves A*X=B for X using a previously QR factored matrix.
        /// </summary>
        /// <param name="q">The Q matrix obtained by calling <see cref="QRFactor(Complex32[],int,int,Complex32[],Complex32[])"/>.</param>
        /// <param name="r">The R matrix obtained by calling <see cref="QRFactor(Complex32[],int,int,Complex32[],Complex32[])"/>. </param>
        /// <param name="rowsA">The number of rows in the A matrix.</param>
        /// <param name="columnsA">The number of columns in the A matrix.</param>
        /// <param name="tau">Contains additional information on Q. Only used for the native solver
        /// and can be <c>null</c> for the managed provider.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="columnsB">The number of columns of B.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        /// <param name="method">The type of QR factorization to perform. <seealso cref="QRMethod"/></param>
        /// <remarks>Rows must be greater or equal to columns.</remarks>
        public void QRSolveFactored(Complex32[] q, Complex32[] r, int rowsA, int columnsA, Complex32[] tau, Complex32[] b, int columnsB, Complex32[] x, QRMethod method = QRMethod.Full)
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
                throw new ArgumentNullException(nameof(b));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (rowsA < columnsA)
            {
                throw new ArgumentException("The number of rows must greater than or equal to the number of columns.");
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

            if (r.Length != rowsR*columnsR)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {rowsR * columnsR}.", nameof(r));
            }

            if (q.Length != rowsQ*columnsQ)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {rowsQ * columnsQ}.", nameof(q));
            }

            if (b.Length != rowsA*columnsB)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {rowsA * columnsB}.", nameof(b));
            }

            if (x.Length != columnsA*columnsB)
            {
                throw new ArgumentException($"The given array has the wrong length. Should be {columnsA * columnsB}.", nameof(x));
            }

            var sol = new Complex32[b.Length];

            // Copy B matrix to "sol", so B data will not be changed
            Array.Copy(b, 0, sol, 0, b.Length);

            // Compute Y = transpose(Q)*B
            var column = new Complex32[rowsA];
            for (var j = 0; j < columnsB; j++)
            {
                var jm = j*rowsA;
                Array.Copy(sol, jm, column, 0, rowsA);
                CommonParallel.For(0, columnsA, (u, v) =>
                    {
                        for (int i = u; i < v; i++)
                        {
                            var im = i*rowsA;

                            var sum = Complex32.Zero;
                            for (var k = 0; k < rowsA; k++)
                            {
                                sum += q[im + k].Conjugate()*column[k];
                            }

                            sol[jm + i] = sum;
                        }
                    });
            }

            // Solve R*X = Y;
            for (var k = columnsA - 1; k >= 0; k--)
            {
                var km = k*rowsR;
                for (var j = 0; j < columnsB; j++)
                {
                    sol[(j*rowsA) + k] /= r[km + k];
                }

                for (var i = 0; i < k; i++)
                {
                    for (var j = 0; j < columnsB; j++)
                    {
                        var jm = j*rowsA;
                        sol[jm + i] -= sol[jm + k]*r[km + i];
                    }
                }
            }

            // Fill result matrix
            for (var col = 0; col < columnsB; col++)
            {
                Array.Copy(sol, col*rowsA, x, col*columnsA, columnsR);
            }
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
        public void SingularValueDecomposition(bool computeVectors, Complex32[] a, int rowsA, int columnsA, Complex32[] s, Complex32[] u, Complex32[] vt)
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

            var work = new Complex32[rowsA];

            const int maxiter = 1000;

            var e = new Complex32[columnsA];
            var v = new Complex32[vt.Length];
            var stemp = new Complex32[Math.Min(rowsA + 1, columnsA)];

            int i, j, l, lp1;

            Complex32 t;

            var ncu = rowsA;

            // Reduce matrix to bidiagonal form, storing the diagonal elements
            // in "s" and the super-diagonal elements in "e".
            var nct = Math.Min(rowsA - 1, columnsA);
            var nrt = Math.Max(0, Math.Min(columnsA - 2, rowsA));
            var lu = Math.Max(nct, nrt);

            for (l = 0; l < lu; l++)
            {
                lp1 = l + 1;
                if (l < nct)
                {
                    // Compute the transformation for the l-th column and
                    // place the l-th diagonal in vector s[l].
                    var sum = 0.0f;
                    for (i = l; i < rowsA; i++)
                    {
                        sum += a[(l*rowsA) + i].Magnitude*a[(l*rowsA) + i].Magnitude;
                    }

                    stemp[l] = (float) Math.Sqrt(sum);
                    if (stemp[l] != 0.0f)
                    {
                        if (a[(l*rowsA) + l] != 0.0f)
                        {
                            stemp[l] = stemp[l].Magnitude*(a[(l*rowsA) + l]/a[(l*rowsA) + l].Magnitude);
                        }

                        // A part of column "l" of Matrix A from row "l" to end multiply by 1.0f / s[l]
                        for (i = l; i < rowsA; i++)
                        {
                            a[(l*rowsA) + i] = a[(l*rowsA) + i]*(1.0f/stemp[l]);
                        }

                        a[(l*rowsA) + l] = 1.0f + a[(l*rowsA) + l];
                    }

                    stemp[l] = -stemp[l];
                }

                for (j = lp1; j < columnsA; j++)
                {
                    if (l < nct)
                    {
                        if (stemp[l] != 0.0f)
                        {
                            // Apply the transformation.
                            t = 0.0f;
                            for (i = l; i < rowsA; i++)
                            {
                                t += a[(l*rowsA) + i].Conjugate()*a[(j*rowsA) + i];
                            }

                            t = -t/a[(l*rowsA) + l];

                            for (var ii = l; ii < rowsA; ii++)
                            {
                                a[(j*rowsA) + ii] += t*a[(l*rowsA) + ii];
                            }
                        }
                    }

                    // Place the l-th row of matrix into "e" for the
                    // subsequent calculation of the row transformation.
                    e[j] = a[(j*rowsA) + l].Conjugate();
                }

                if (computeVectors && l < nct)
                {
                    // Place the transformation in "u" for subsequent back multiplication.
                    for (i = l; i < rowsA; i++)
                    {
                        u[(l*rowsA) + i] = a[(l*rowsA) + i];
                    }
                }

                if (l >= nrt)
                {
                    continue;
                }

                // Compute the l-th row transformation and place the l-th super-diagonal in e(l).
                var enorm = 0.0f;
                for (i = lp1; i < e.Length; i++)
                {
                    enorm += e[i].Magnitude*e[i].Magnitude;
                }

                e[l] = (float) Math.Sqrt(enorm);
                if (e[l] != 0.0f)
                {
                    if (e[lp1] != 0.0f)
                    {
                        e[l] = e[l].Magnitude*(e[lp1]/e[lp1].Magnitude);
                    }

                    // Scale vector "e" from "lp1" by 1.0f / e[l]
                    for (i = lp1; i < e.Length; i++)
                    {
                        e[i] = e[i]*(1.0f/e[l]);
                    }

                    e[lp1] = 1.0f + e[lp1];
                }

                e[l] = -e[l].Conjugate();

                if (lp1 < rowsA && e[l] != 0.0f)
                {
                    // Apply the transformation.
                    for (i = lp1; i < rowsA; i++)
                    {
                        work[i] = 0.0f;
                    }

                    for (j = lp1; j < columnsA; j++)
                    {
                        for (var ii = lp1; ii < rowsA; ii++)
                        {
                            work[ii] += e[j]*a[(j*rowsA) + ii];
                        }
                    }

                    for (j = lp1; j < columnsA; j++)
                    {
                        var ww = (-e[j]/e[lp1]).Conjugate();
                        for (var ii = lp1; ii < rowsA; ii++)
                        {
                            a[(j*rowsA) + ii] += ww*work[ii];
                        }
                    }
                }

                if (!computeVectors)
                {
                    continue;
                }

                // Place the transformation in v for subsequent back multiplication.
                for (i = lp1; i < columnsA; i++)
                {
                    v[(l*columnsA) + i] = e[i];
                }
            }

            // Set up the final bidiagonal matrix or order m.
            var m = Math.Min(columnsA, rowsA + 1);
            var nctp1 = nct + 1;
            var nrtp1 = nrt + 1;
            if (nct < columnsA)
            {
                stemp[nctp1 - 1] = a[((nctp1 - 1)*rowsA) + (nctp1 - 1)];
            }

            if (rowsA < m)
            {
                stemp[m - 1] = 0.0f;
            }

            if (nrtp1 < m)
            {
                e[nrtp1 - 1] = a[((m - 1)*rowsA) + (nrtp1 - 1)];
            }

            e[m - 1] = 0.0f;

            // If required, generate "u".
            if (computeVectors)
            {
                for (j = nctp1 - 1; j < ncu; j++)
                {
                    for (i = 0; i < rowsA; i++)
                    {
                        u[(j*rowsA) + i] = 0.0f;
                    }

                    u[(j*rowsA) + j] = 1.0f;
                }

                for (l = nct - 1; l >= 0; l--)
                {
                    if (stemp[l] != 0.0f)
                    {
                        for (j = l + 1; j < ncu; j++)
                        {
                            t = 0.0f;
                            for (i = l; i < rowsA; i++)
                            {
                                t += u[(l*rowsA) + i].Conjugate()*u[(j*rowsA) + i];
                            }

                            t = -t/u[(l*rowsA) + l];
                            for (var ii = l; ii < rowsA; ii++)
                            {
                                u[(j*rowsA) + ii] += t*u[(l*rowsA) + ii];
                            }
                        }

                        // A part of column "l" of matrix A from row "l" to end multiply by -1.0f
                        for (i = l; i < rowsA; i++)
                        {
                            u[(l*rowsA) + i] = u[(l*rowsA) + i]*-1.0f;
                        }

                        u[(l*rowsA) + l] = 1.0f + u[(l*rowsA) + l];
                        for (i = 0; i < l; i++)
                        {
                            u[(l*rowsA) + i] = 0.0f;
                        }
                    }
                    else
                    {
                        for (i = 0; i < rowsA; i++)
                        {
                            u[(l*rowsA) + i] = 0.0f;
                        }

                        u[(l*rowsA) + l] = 1.0f;
                    }
                }
            }

            // If it is required, generate v.
            if (computeVectors)
            {
                for (l = columnsA - 1; l >= 0; l--)
                {
                    lp1 = l + 1;
                    if (l < nrt)
                    {
                        if (e[l] != 0.0f)
                        {
                            for (j = lp1; j < columnsA; j++)
                            {
                                t = 0.0f;
                                for (i = lp1; i < columnsA; i++)
                                {
                                    t += v[(l*columnsA) + i].Conjugate()*v[(j*columnsA) + i];
                                }

                                t = -t/v[(l*columnsA) + lp1];
                                for (var ii = l; ii < columnsA; ii++)
                                {
                                    v[(j*columnsA) + ii] += t*v[(l*columnsA) + ii];
                                }
                            }
                        }
                    }

                    for (i = 0; i < columnsA; i++)
                    {
                        v[(l*columnsA) + i] = 0.0f;
                    }

                    v[(l*columnsA) + l] = 1.0f;
                }
            }

            // Transform "s" and "e" so that they are float
            for (i = 0; i < m; i++)
            {
                Complex32 r;
                if (stemp[i] != 0.0f)
                {
                    t = stemp[i].Magnitude;
                    r = stemp[i]/t;
                    stemp[i] = t;
                    if (i < m - 1)
                    {
                        e[i] = e[i]/r;
                    }

                    if (computeVectors)
                    {
                        // A part of column "i" of matrix U from row 0 to end multiply by r
                        for (j = 0; j < rowsA; j++)
                        {
                            u[(i*rowsA) + j] = u[(i*rowsA) + j]*r;
                        }
                    }
                }

                // Exit
                if (i == m - 1)
                {
                    break;
                }

                if (e[i] == 0.0f)
                {
                    continue;
                }

                t = e[i].Magnitude;
                r = t/e[i];
                e[i] = t;
                stemp[i + 1] = stemp[i + 1]*r;
                if (!computeVectors)
                {
                    continue;
                }

                // A part of column "i+1" of matrix VT from row 0 to end multiply by r
                for (j = 0; j < columnsA; j++)
                {
                    v[((i + 1)*columnsA) + j] = v[((i + 1)*columnsA) + j]*r;
                }
            }

            // Main iteration loop for the singular values.
            var mn = m;
            var iter = 0;

            while (m > 0)
            {
                // Quit if all the singular values have been found.
                // If too many iterations have been performed throw exception.
                if (iter >= maxiter)
                {
                    throw new NonConvergenceException();
                }

                // This section of the program inspects for negligible elements in the s and e arrays,
                // on completion the variables case and l are set as follows:
                // case = 1: if mS[m] and e[l-1] are negligible and l < m
                // case = 2: if mS[l] is negligible and l < m
                // case = 3: if e[l-1] is negligible, l < m, and mS[l, ..., mS[m] are not negligible (qr step).
                // case = 4: if e[m-1] is negligible (convergence).
                float ztest;
                float test;
                for (l = m - 2; l >= 0; l--)
                {
                    test = stemp[l].Magnitude + stemp[l + 1].Magnitude;
                    ztest = test + e[l].Magnitude;
                    if (ztest.AlmostEqualRelative(test, 7))
                    {
                        e[l] = 0.0f;
                        break;
                    }
                }

                int kase;
                if (l == m - 2)
                {
                    kase = 4;
                }
                else
                {
                    int ls;
                    for (ls = m - 1; ls > l; ls--)
                    {
                        test = 0.0f;
                        if (ls != m - 1)
                        {
                            test = test + e[ls].Magnitude;
                        }

                        if (ls != l + 1)
                        {
                            test = test + e[ls - 1].Magnitude;
                        }

                        ztest = test + stemp[ls].Magnitude;
                        if (ztest.AlmostEqualRelative(test, 7))
                        {
                            stemp[ls] = 0.0f;
                            break;
                        }
                    }

                    if (ls == l)
                    {
                        kase = 3;
                    }
                    else if (ls == m - 1)
                    {
                        kase = 1;
                    }
                    else
                    {
                        kase = 2;
                        l = ls;
                    }
                }

                l = l + 1;

                // Perform the task indicated by case.
                int k;
                float f;
                float sn;
                float cs;
                switch (kase)
                {
                        // Deflate negligible s[m].
                    case 1:
                        f = e[m - 2].Real;
                        e[m - 2] = 0.0f;
                        float t1;
                        for (var kk = l; kk < m - 1; kk++)
                        {
                            k = m - 2 - kk + l;
                            t1 = stemp[k].Real;
                            Drotg(ref t1, ref f, out cs, out sn);
                            stemp[k] = t1;
                            if (k != l)
                            {
                                f = -sn*e[k - 1].Real;
                                e[k - 1] = cs*e[k - 1];
                            }

                            if (computeVectors)
                            {
                                // Rotate
                                for (i = 0; i < columnsA; i++)
                                {
                                    var z = (cs*v[(k*columnsA) + i]) + (sn*v[((m - 1)*columnsA) + i]);
                                    v[((m - 1)*columnsA) + i] = (cs*v[((m - 1)*columnsA) + i]) - (sn*v[(k*columnsA) + i]);
                                    v[(k*columnsA) + i] = z;
                                }
                            }
                        }

                        break;

                        // Split at negligible s[l].
                    case 2:
                        f = e[l - 1].Real;
                        e[l - 1] = 0.0f;
                        for (k = l; k < m; k++)
                        {
                            t1 = stemp[k].Real;
                            Drotg(ref t1, ref f, out cs, out sn);
                            stemp[k] = t1;
                            f = -sn*e[k].Real;
                            e[k] = cs*e[k];
                            if (computeVectors)
                            {
                                // Rotate
                                for (i = 0; i < rowsA; i++)
                                {
                                    var z = (cs*u[(k*rowsA) + i]) + (sn*u[((l - 1)*rowsA) + i]);
                                    u[((l - 1)*rowsA) + i] = (cs*u[((l - 1)*rowsA) + i]) - (sn*u[(k*rowsA) + i]);
                                    u[(k*rowsA) + i] = z;
                                }
                            }
                        }

                        break;

                        // Perform one qr step.
                    case 3:
                        // calculate the shift.
                        var scale = 0.0f;
                        scale = Math.Max(scale, stemp[m - 1].Magnitude);
                        scale = Math.Max(scale, stemp[m - 2].Magnitude);
                        scale = Math.Max(scale, e[m - 2].Magnitude);
                        scale = Math.Max(scale, stemp[l].Magnitude);
                        scale = Math.Max(scale, e[l].Magnitude);
                        var sm = stemp[m - 1].Real/scale;
                        var smm1 = stemp[m - 2].Real/scale;
                        var emm1 = e[m - 2].Real/scale;
                        var sl = stemp[l].Real/scale;
                        var el = e[l].Real/scale;
                        var b = (((smm1 + sm)*(smm1 - sm)) + (emm1*emm1))/2.0f;
                        var c = (sm*emm1)*(sm*emm1);
                        var shift = 0.0f;
                        if (b != 0.0f || c != 0.0f)
                        {
                            shift = (float) Math.Sqrt((b*b) + c);
                            if (b < 0.0f)
                            {
                                shift = -shift;
                            }

                            shift = c/(b + shift);
                        }

                        f = ((sl + sm)*(sl - sm)) + shift;
                        var g = sl*el;

                        // Chase zeros
                        for (k = l; k < m - 1; k++)
                        {
                            Drotg(ref f, ref g, out cs, out sn);
                            if (k != l)
                            {
                                e[k - 1] = f;
                            }

                            f = (cs*stemp[k].Real) + (sn*e[k].Real);
                            e[k] = (cs*e[k]) - (sn*stemp[k]);
                            g = sn*stemp[k + 1].Real;
                            stemp[k + 1] = cs*stemp[k + 1];
                            if (computeVectors)
                            {
                                for (i = 0; i < columnsA; i++)
                                {
                                    var z = (cs*v[(k*columnsA) + i]) + (sn*v[((k + 1)*columnsA) + i]);
                                    v[((k + 1)*columnsA) + i] = (cs*v[((k + 1)*columnsA) + i]) - (sn*v[(k*columnsA) + i]);
                                    v[(k*columnsA) + i] = z;
                                }
                            }

                            Drotg(ref f, ref g, out cs, out sn);
                            stemp[k] = f;
                            f = (cs*e[k].Real) + (sn*stemp[k + 1].Real);
                            stemp[k + 1] = -(sn*e[k]) + (cs*stemp[k + 1]);
                            g = sn*e[k + 1].Real;
                            e[k + 1] = cs*e[k + 1];
                            if (computeVectors && k < rowsA)
                            {
                                for (i = 0; i < rowsA; i++)
                                {
                                    var z = (cs*u[(k*rowsA) + i]) + (sn*u[((k + 1)*rowsA) + i]);
                                    u[((k + 1)*rowsA) + i] = (cs*u[((k + 1)*rowsA) + i]) - (sn*u[(k*rowsA) + i]);
                                    u[(k*rowsA) + i] = z;
                                }
                            }
                        }

                        e[m - 2] = f;
                        iter = iter + 1;
                        break;

                        // Convergence
                    case 4:

                        // Make the singular value  positive
                        if (stemp[l].Real < 0.0f)
                        {
                            stemp[l] = -stemp[l];
                            if (computeVectors)
                            {
                                // A part of column "l" of matrix VT from row 0 to end multiply by -1
                                for (i = 0; i < columnsA; i++)
                                {
                                    v[(l*columnsA) + i] = v[(l*columnsA) + i]*-1.0f;
                                }
                            }
                        }

                        // Order the singular value.
                        while (l != mn - 1)
                        {
                            if (stemp[l].Real >= stemp[l + 1].Real)
                            {
                                break;
                            }

                            t = stemp[l];
                            stemp[l] = stemp[l + 1];
                            stemp[l + 1] = t;
                            if (computeVectors && l < columnsA)
                            {
                                // Swap columns l, l + 1
                                for (i = 0; i < columnsA; i++)
                                {
                                    (v[(l*columnsA) + i], v[((l + 1)*columnsA) + i]) = (v[((l + 1)*columnsA) + i], v[(l*columnsA) + i]);
                                }
                            }

                            if (computeVectors && l < rowsA)
                            {
                                // Swap columns l, l + 1
                                for (i = 0; i < rowsA; i++)
                                {
                                    (u[(l*rowsA) + i], u[((l + 1)*rowsA) + i]) = (u[((l + 1)*rowsA) + i], u[(l*rowsA) + i]);
                                }
                            }

                            l = l + 1;
                        }

                        iter = 0;
                        m = m - 1;
                        break;
                }
            }

            if (computeVectors)
            {
                // Finally transpose "v" to get "vt" matrix
                for (i = 0; i < columnsA; i++)
                {
                    for (j = 0; j < columnsA; j++)
                    {
                        vt[(j*columnsA) + i] = v[(i*columnsA) + j].Conjugate();
                    }
                }
            }

            // Copy stemp to s with size adjustment. We are using ported copy of linpack's svd code and it uses
            // a singular vector of length rows+1 when rows < columns. The last element is not used and needs to be removed.
            // We should port lapack's svd routine to remove this problem.
            Array.Copy(stemp, 0, s, 0, Math.Min(rowsA, columnsA));
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
        public void SvdSolve(Complex32[] a, int rowsA, int columnsA, Complex32[] b, int columnsB, Complex32[] x)
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

            var s = new Complex32[Math.Min(rowsA, columnsA)];
            var u = new Complex32[rowsA*rowsA];
            var vt = new Complex32[columnsA*columnsA];

            var clone = new Complex32[a.Length];
            a.Copy(clone);
            SingularValueDecomposition(true, clone, rowsA, columnsA, s, u, vt);
            SvdSolveFactored(rowsA, columnsA, s, u, vt, b, columnsB, x);
        }

        /// <summary>
        /// Solves A*X=B for X using a previously SVD decomposed matrix.
        /// </summary>
        /// <param name="rowsA">The number of rows in the A matrix.</param>
        /// <param name="columnsA">The number of columns in the A matrix.</param>
        /// <param name="s">The s values returned by <see cref="SingularValueDecomposition(bool,Complex32[],int,int,Complex32[],Complex32[],Complex32[])"/>.</param>
        /// <param name="u">The left singular vectors returned by  <see cref="SingularValueDecomposition(bool,Complex32[],int,int,Complex32[],Complex32[],Complex32[])"/>.</param>
        /// <param name="vt">The right singular  vectors returned by  <see cref="SingularValueDecomposition(bool,Complex32[],int,int,Complex32[],Complex32[],Complex32[])"/>.</param>
        /// <param name="b">The B matrix.</param>
        /// <param name="columnsB">The number of columns of B.</param>
        /// <param name="x">On exit, the solution matrix.</param>
        public void SvdSolveFactored(int rowsA, int columnsA, Complex32[] s, Complex32[] u, Complex32[] vt, Complex32[] b, int columnsB, Complex32[] x)
        {
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

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
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

            if (b.Length != rowsA*columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            if (x.Length != columnsA*columnsB)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(b));
            }

            var mn = Math.Min(rowsA, columnsA);
            var tmp = new Complex32[columnsA];

            for (var k = 0; k < columnsB; k++)
            {
                for (var j = 0; j < columnsA; j++)
                {
                    var value = Complex32.Zero;
                    if (j < mn)
                    {
                        for (var i = 0; i < rowsA; i++)
                        {
                            value += u[(j*rowsA) + i].Conjugate()*b[(k*rowsA) + i];
                        }

                        value /= s[j];
                    }

                    tmp[j] = value;
                }

                for (var j = 0; j < columnsA; j++)
                {
                    var value = Complex32.Zero;
                    for (var i = 0; i < columnsA; i++)
                    {
                        value += vt[(j*columnsA) + i].Conjugate()*tmp[i];
                    }

                    x[(k*columnsA) + j] = value;
                }
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
        public void EigenDecomp(bool isSymmetric, int order, Complex32[] matrix, Complex32[] matrixEv, Complex[] vectorEv, Complex32[] matrixD)
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

            var matrixCopy = new Complex32[matrix.Length];
            Array.Copy(matrix, 0, matrixCopy, 0, matrix.Length);
            if (isSymmetric)
            {
                var tau = new Complex32[order];
                var d = new float[order];
                var e = new float[order];

                SymmetricTridiagonalize(matrixCopy, d, e, tau, order);
                SymmetricDiagonalize(matrixEv, d, e, order);
                SymmetricUntridiagonalize(matrixEv, matrixCopy, tau, order);

                for (var i = 0; i < order; i++)
                {
                    vectorEv[i] = new Complex(d[i], e[i]);
                    matrixD[i*order + i] = new Complex32(d[i], e[i]);
                }
            }
            else
            {
                var v = new Complex32[order];

                NonsymmetricReduceToHessenberg(matrixEv, matrixCopy, order);
                NonsymmetricReduceHessenberToRealSchur(v, matrixEv, matrixCopy, order);

                for (var i = 0; i < order; i++)
                {
                    vectorEv[i] = new Complex(v[i].Real, v[i].Imaginary);
                    matrixD[i*order + i] = v[i];
                }
            }
        }

        /// <summary>
        /// Reduces a complex Hermitian matrix to a real symmetric tridiagonal matrix using unitary similarity transformations.
        /// </summary>
        /// <param name="matrixA">Source matrix to reduce</param>
        /// <param name="d">Output: Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Output: Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="tau">Output: Arrays that contains further information about the transformations.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures HTRIDI by
        /// Smith, Boyle, Dongarra, Garbow, Ikebe, Klema, Moler, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        internal static void SymmetricTridiagonalize(Complex32[] matrixA, float[] d, float[] e, Complex32[] tau, int order)
        {
            float hh;
            tau[order - 1] = Complex32.One;

            for (var i = 0; i < order; i++)
            {
                d[i] = matrixA[i*order + i].Real;
            }

            // Householder reduction to tridiagonal form.
            for (var i = order - 1; i > 0; i--)
            {
                // Scale to avoid under/overflow.
                var scale = 0.0f;
                var h = 0.0f;

                for (var k = 0; k < i; k++)
                {
                    scale = scale + Math.Abs(matrixA[k*order + i].Real) + Math.Abs(matrixA[k*order + i].Imaginary);
                }

                if (scale == 0.0f)
                {
                    tau[i - 1] = Complex32.One;
                    e[i] = 0.0f;
                }
                else
                {
                    for (var k = 0; k < i; k++)
                    {
                        matrixA[k*order + i] /= scale;
                        h += matrixA[k*order + i].MagnitudeSquared;
                    }

                    Complex32 g = (float) Math.Sqrt(h);
                    e[i] = scale*g.Real;

                    Complex32 temp;
                    var im1Oi = (i - 1)*order + i;
                    var f = matrixA[im1Oi];
                    if (f.Magnitude != 0.0f)
                    {
                        temp = -(matrixA[im1Oi].Conjugate()*tau[i].Conjugate())/f.Magnitude;
                        h += f.Magnitude*g.Real;
                        g = 1.0f + (g/f.Magnitude);
                        matrixA[im1Oi] *= g;
                    }
                    else
                    {
                        temp = -tau[i].Conjugate();
                        matrixA[im1Oi] = g;
                    }

                    if ((f.Magnitude == 0.0f) || (i != 1))
                    {
                        f = Complex32.Zero;
                        for (var j = 0; j < i; j++)
                        {
                            var tmp = Complex32.Zero;
                            var jO = j*order;
                            // Form element of A*U.
                            for (var k = 0; k <= j; k++)
                            {
                                tmp += matrixA[k*order + j]*matrixA[k*order + i].Conjugate();
                            }

                            for (var k = j + 1; k <= i - 1; k++)
                            {
                                tmp += matrixA[jO + k].Conjugate()*matrixA[k*order + i].Conjugate();
                            }

                            // Form element of P
                            tau[j] = tmp/h;
                            f += (tmp/h)*matrixA[jO + i];
                        }

                        hh = f.Real/(h + h);

                        // Form the reduced A.
                        for (var j = 0; j < i; j++)
                        {
                            f = matrixA[j*order + i].Conjugate();
                            g = tau[j] - (hh*f);
                            tau[j] = g.Conjugate();

                            for (var k = 0; k <= j; k++)
                            {
                                matrixA[k*order + j] -= (f*tau[k]) + (g*matrixA[k*order + i]);
                            }
                        }
                    }

                    for (var k = 0; k < i; k++)
                    {
                        matrixA[k*order + i] *= scale;
                    }

                    tau[i - 1] = temp.Conjugate();
                }

                hh = d[i];
                d[i] = matrixA[i*order + i].Real;
                matrixA[i*order + i] = new Complex32(hh, scale*(float) Math.Sqrt(h));
            }

            hh = d[0];
            d[0] = matrixA[0].Real;
            matrixA[0] = hh;
            e[0] = 0.0f;
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        /// <param name="dataEv">Data array of matrix V (eigenvectors)</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tql2, by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        internal static void SymmetricDiagonalize(Complex32[] dataEv, float[] d, float[] e, int order)
        {
            const int maxiter = 1000;

            for (var i = 1; i < order; i++)
            {
                e[i - 1] = e[i];
            }

            e[order - 1] = 0.0f;

            var f = 0.0f;
            var tst1 = 0.0f;
            var eps = Precision.DoublePrecision;
            for (var l = 0; l < order; l++)
            {
                // Find small subdiagonal element
                tst1 = Math.Max(tst1, Math.Abs(d[l]) + Math.Abs(e[l]));
                var m = l;
                while (m < order)
                {
                    if (Math.Abs(e[m]) <= eps*tst1)
                    {
                        break;
                    }

                    m++;
                }

                // If m == l, d[l] is an eigenvalue,
                // otherwise, iterate.
                if (m > l)
                {
                    var iter = 0;
                    do
                    {
                        iter = iter + 1; // (Could check iteration count here.)

                        // Compute implicit shift
                        var g = d[l];
                        var p = (d[l + 1] - g)/(2.0f*e[l]);
                        var r = SpecialFunctions.Hypotenuse(p, 1.0f);
                        if (p < 0)
                        {
                            r = -r;
                        }

                        d[l] = e[l]/(p + r);
                        d[l + 1] = e[l]*(p + r);

                        var dl1 = d[l + 1];
                        var h = g - d[l];
                        for (var i = l + 2; i < order; i++)
                        {
                            d[i] -= h;
                        }

                        f = f + h;

                        // Implicit QL transformation.
                        p = d[m];
                        var c = 1.0f;
                        var c2 = c;
                        var c3 = c;
                        var el1 = e[l + 1];
                        var s = 0.0f;
                        var s2 = 0.0f;
                        for (var i = m - 1; i >= l; i--)
                        {
                            c3 = c2;
                            c2 = c;
                            s2 = s;
                            g = c*e[i];
                            h = c*p;
                            r = SpecialFunctions.Hypotenuse(p, e[i]);
                            e[i + 1] = s*r;
                            s = e[i]/r;
                            c = p/r;
                            p = (c*d[i]) - (s*g);
                            d[i + 1] = h + (s*((c*g) + (s*d[i])));

                            // Accumulate transformation.
                            for (var k = 0; k < order; k++)
                            {
                                h = dataEv[((i + 1)*order) + k].Real;
                                dataEv[((i + 1)*order) + k] = (s*dataEv[(i*order) + k].Real) + (c*h);
                                dataEv[(i*order) + k] = (c*dataEv[(i*order) + k].Real) - (s*h);
                            }
                        }

                        p = (-s)*s2*c3*el1*e[l]/dl1;
                        e[l] = s*p;
                        d[l] = c*p;

                        // Check for convergence. If too many iterations have been performed,
                        // throw exception that Convergence Failed
                        if (iter >= maxiter)
                        {
                            throw new NonConvergenceException();
                        }
                    } while (Math.Abs(e[l]) > eps*tst1);
                }

                d[l] = d[l] + f;
                e[l] = 0.0f;
            }

            // Sort eigenvalues and corresponding vectors.
            for (var i = 0; i < order - 1; i++)
            {
                var k = i;
                var p = d[i];
                for (var j = i + 1; j < order; j++)
                {
                    if (d[j] < p)
                    {
                        k = j;
                        p = d[j];
                    }
                }

                if (k != i)
                {
                    d[k] = d[i];
                    d[i] = p;
                    for (var j = 0; j < order; j++)
                    {
                        p = dataEv[(i*order) + j].Real;
                        dataEv[(i*order) + j] = dataEv[(k*order) + j];
                        dataEv[(k*order) + j] = p;
                    }
                }
            }
        }

        /// <summary>
        /// Determines eigenvectors by undoing the symmetric tridiagonalize transformation
        /// </summary>
        /// <param name="dataEv">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixA">Previously tridiagonalized matrix by SymmetricTridiagonalize.</param>
        /// <param name="tau">Contains further information about the transformations</param>
        /// <param name="order">Input matrix order</param>
        /// <remarks>This is derived from the Algol procedures HTRIBK, by
        /// by Smith, Boyle, Dongarra, Garbow, Ikebe, Klema, Moler, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        internal static void SymmetricUntridiagonalize(Complex32[] dataEv, Complex32[] matrixA, Complex32[] tau, int order)
        {
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    dataEv[(j*order) + i] = dataEv[(j*order) + i].Real*tau[i].Conjugate();
                }
            }

            // Recover and apply the Householder matrices.
            for (var i = 1; i < order; i++)
            {
                var h = matrixA[i*order + i].Imaginary;
                if (h != 0)
                {
                    for (var j = 0; j < order; j++)
                    {
                        var s = Complex32.Zero;
                        for (var k = 0; k < i; k++)
                        {
                            s += dataEv[(j*order) + k]*matrixA[k*order + i];
                        }

                        s = (s/h)/h;

                        for (var k = 0; k < i; k++)
                        {
                            dataEv[(j*order) + k] -= s*matrixA[k*order + i].Conjugate();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction to Hessenberg form.
        /// </summary>
        /// <param name="dataEv">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures orthes and ortran,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutines in EISPACK.</remarks>
        internal static void NonsymmetricReduceToHessenberg(Complex32[] dataEv, Complex32[] matrixH, int order)
        {
            var ort = new Complex32[order];

            for (var m = 1; m < order - 1; m++)
            {
                // Scale column.
                var scale = 0.0f;
                var mm1O = (m - 1)*order;
                for (var i = m; i < order; i++)
                {
                    scale += Math.Abs(matrixH[mm1O + i].Real) + Math.Abs(matrixH[mm1O + i].Imaginary);
                }

                if (scale != 0.0f)
                {
                    // Compute Householder transformation.
                    var h = 0.0f;
                    for (var i = order - 1; i >= m; i--)
                    {
                        ort[i] = matrixH[mm1O + i]/scale;
                        h += ort[i].MagnitudeSquared;
                    }

                    var g = (float) Math.Sqrt(h);
                    if (ort[m].Magnitude != 0)
                    {
                        h = h + (ort[m].Magnitude*g);
                        g /= ort[m].Magnitude;
                        ort[m] = (1.0f + g)*ort[m];
                    }
                    else
                    {
                        ort[m] = g;
                        matrixH[mm1O + m] = scale;
                    }

                    // Apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)
                    for (var j = m; j < order; j++)
                    {
                        var f = Complex32.Zero;
                        var jO = j*order;
                        for (var i = order - 1; i >= m; i--)
                        {
                            f += ort[i].Conjugate()*matrixH[jO + i];
                        }

                        f = f/h;
                        for (var i = m; i < order; i++)
                        {
                            matrixH[jO + i] -= f*ort[i];
                        }
                    }

                    for (var i = 0; i < order; i++)
                    {
                        var f = Complex32.Zero;
                        for (var j = order - 1; j >= m; j--)
                        {
                            f += ort[j]*matrixH[j*order + i];
                        }

                        f = f/h;
                        for (var j = m; j < order; j++)
                        {
                            matrixH[j*order + i] -= f*ort[j].Conjugate();
                        }
                    }

                    ort[m] = scale*ort[m];
                    matrixH[mm1O + m] *= -g;
                }
            }

            // Accumulate transformations (Algol's ortran).
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    dataEv[(j*order) + i] = i == j ? Complex32.One : Complex32.Zero;
                }
            }

            for (var m = order - 2; m >= 1; m--)
            {
                var mm1O = (m - 1)*order;
                var mm1Om = mm1O + m;
                if (matrixH[mm1Om] != Complex32.Zero && ort[m] != Complex32.Zero)
                {
                    var norm = (matrixH[mm1Om].Real*ort[m].Real) + (matrixH[mm1Om].Imaginary*ort[m].Imaginary);

                    for (var i = m + 1; i < order; i++)
                    {
                        ort[i] = matrixH[mm1O + i];
                    }

                    for (var j = m; j < order; j++)
                    {
                        var g = Complex32.Zero;
                        for (var i = m; i < order; i++)
                        {
                            g += ort[i].Conjugate()*dataEv[(j*order) + i];
                        }

                        // Double division avoids possible underflow
                        g /= norm;
                        for (var i = m; i < order; i++)
                        {
                            dataEv[(j*order) + i] += g*ort[i];
                        }
                    }
                }
            }

            // Create real subdiagonal elements.
            for (var i = 1; i < order; i++)
            {
                var im1 = i - 1;
                var im1O = im1*order;
                var im1Oi = im1O + i;
                var iO = i*order;
                if (matrixH[im1Oi].Imaginary != 0.0f)
                {
                    var y = matrixH[im1Oi]/matrixH[im1Oi].Magnitude;
                    matrixH[im1Oi] = matrixH[im1Oi].Magnitude;
                    for (var j = i; j < order; j++)
                    {
                        matrixH[j*order + i] *= y.Conjugate();
                    }

                    for (var j = 0; j <= Math.Min(i + 1, order - 1); j++)
                    {
                        matrixH[iO + j] *= y;
                    }

                    for (var j = 0; j < order; j++)
                    {
                        dataEv[(i*order) + j] *= y;
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction from Hessenberg to real Schur form.
        /// </summary>
        /// <param name="vectorV">Data array of the eigenvectors</param>
        /// <param name="dataEv">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedure hqr2,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        internal static void NonsymmetricReduceHessenberToRealSchur(Complex32[] vectorV, Complex32[] dataEv, Complex32[] matrixH, int order)
        {
            // Initialize
            var n = order - 1;
            var eps = (float) Precision.SinglePrecision;

            float norm;
            Complex32 x, y, z, exshift = Complex32.Zero;

            // Outer loop over eigenvalue index
            var iter = 0;
            while (n >= 0)
            {
                // Look for single small sub-diagonal element
                var l = n;
                while (l > 0)
                {
                    var lm1 = l - 1;
                    var lm1O = lm1*order;
                    var lO = l*order;
                    var tst1 = Math.Abs(matrixH[lm1O + lm1].Real) + Math.Abs(matrixH[lm1O + lm1].Imaginary) + Math.Abs(matrixH[lO + l].Real) + Math.Abs(matrixH[lO + l].Imaginary);
                    if (Math.Abs(matrixH[lm1O + l].Real) < eps*tst1)
                    {
                        break;
                    }

                    l--;
                }

                var nm1 = n - 1;
                var nm1O = nm1*order;
                var nO = n*order;
                var nOn = nO + n;
                // Check for convergence
                // One root found
                if (l == n)
                {
                    matrixH[nOn] += exshift;
                    vectorV[n] = matrixH[nOn];
                    n--;
                    iter = 0;
                }
                else
                {
                    // Form shift
                    Complex32 s;
                    if (iter != 10 && iter != 20)
                    {
                        s = matrixH[nOn];
                        x = matrixH[nO + nm1]*matrixH[nm1O + n].Real;

                        if (x.Real != 0.0f || x.Imaginary != 0.0f)
                        {
                            y = (matrixH[nm1O + nm1] - s)/2.0f;
                            z = ((y*y) + x).SquareRoot();
                            if ((y.Real*z.Real) + (y.Imaginary*z.Imaginary) < 0.0)
                            {
                                z *= -1.0f;
                            }

                            x /= y + z;
                            s = s - x;
                        }
                    }
                    else
                    {
                        // Form exceptional shift
                        s = Math.Abs(matrixH[nm1O + n].Real) + Math.Abs(matrixH[(n - 2)*order + nm1].Real);
                    }

                    for (var i = 0; i <= n; i++)
                    {
                        matrixH[i*order + i] -= s;
                    }

                    exshift += s;
                    iter++;

                    // Reduce to triangle (rows)
                    for (var i = l + 1; i <= n; i++)
                    {
                        var im1 = i - 1;
                        var im1O = im1*order;
                        var im1Oim1 = im1O + im1;
                        s = matrixH[im1O + i].Real;
                        norm = SpecialFunctions.Hypotenuse(matrixH[im1Oim1].Magnitude, s.Real);
                        x = matrixH[im1Oim1]/norm;
                        vectorV[i - 1] = x;
                        matrixH[im1Oim1] = norm;
                        matrixH[im1O + i] = new Complex32(0.0f, s.Real/norm);

                        for (var j = i; j < order; j++)
                        {
                            var jO = j*order;
                            y = matrixH[jO + im1];
                            z = matrixH[jO + i];
                            matrixH[jO + im1] = (x.Conjugate()*y) + (matrixH[im1O + i].Imaginary*z);
                            matrixH[jO + i] = (x*z) - (matrixH[im1O + i].Imaginary*y);
                        }
                    }

                    s = matrixH[nOn];
                    if (s.Imaginary != 0.0f)
                    {
                        s /= matrixH[nOn].Magnitude;
                        matrixH[nOn] = matrixH[nOn].Magnitude;

                        for (var j = n + 1; j < order; j++)
                        {
                            matrixH[j*order + n] *= s.Conjugate();
                        }
                    }

                    // Inverse operation (columns).
                    for (var j = l + 1; j <= n; j++)
                    {
                        x = vectorV[j - 1];
                        var jO = j*order;
                        var jm1 = j - 1;
                        var jm1O = jm1*order;
                        var jm1Oj = jm1O + j;
                        for (var i = 0; i <= j; i++)
                        {
                            var jm1Oi = jm1O + i;
                            z = matrixH[jO + i];
                            if (i != j)
                            {
                                y = matrixH[jm1Oi];
                                matrixH[jm1Oi] = (x*y) + (matrixH[jm1O + j].Imaginary*z);
                            }
                            else
                            {
                                y = matrixH[jm1Oi].Real;
                                matrixH[jm1Oi] = new Complex32((x.Real*y.Real) - (x.Imaginary*y.Imaginary) + (matrixH[jm1O + j].Imaginary*z.Real), matrixH[jm1Oi].Imaginary);
                            }

                            matrixH[jO + i] = (x.Conjugate()*z) - (matrixH[jm1O + j].Imaginary*y);
                        }

                        for (var i = 0; i < order; i++)
                        {
                            y = dataEv[((j - 1)*order) + i];
                            z = dataEv[(j*order) + i];
                            dataEv[jm1O + i] = (x*y) + (matrixH[jm1Oj].Imaginary*z);
                            dataEv[jO + i] = (x.Conjugate()*z) - (matrixH[jm1Oj].Imaginary*y);
                        }
                    }

                    if (s.Imaginary != 0.0f)
                    {
                        for (var i = 0; i <= n; i++)
                        {
                            matrixH[nO + i] *= s;
                        }

                        for (var i = 0; i < order; i++)
                        {
                            dataEv[nO + i] *= s;
                        }
                    }
                }
            }

            // All roots found.
            // Backsubstitute to find vectors of upper triangular form
            norm = 0.0f;
            for (var i = 0; i < order; i++)
            {
                for (var j = i; j < order; j++)
                {
                    norm = Math.Max(norm, Math.Abs(matrixH[j*order + i].Real) + Math.Abs(matrixH[j*order + i].Imaginary));
                }
            }

            if (order == 1)
            {
                return;
            }

            if (norm == 0.0)
            {
                return;
            }

            for (n = order - 1; n > 0; n--)
            {
                var nO = n*order;
                var nOn = nO + n;
                x = vectorV[n];
                matrixH[nOn] = 1.0f;

                for (var i = n - 1; i >= 0; i--)
                {
                    z = 0.0f;
                    for (var j = i + 1; j <= n; j++)
                    {
                        z += matrixH[j*order + i]*matrixH[nO + j];
                    }

                    y = x - vectorV[i];
                    if (y.Real == 0.0f && y.Imaginary == 0.0f)
                    {
                        y = eps*norm;
                    }

                    matrixH[nO + i] = z/y;

                    // Overflow control
                    var tr = Math.Abs(matrixH[nO + i].Real) + Math.Abs(matrixH[nO + i].Imaginary);
                    if ((eps*tr)*tr > 1)
                    {
                        for (var j = i; j <= n; j++)
                        {
                            matrixH[nO + j] = matrixH[nO + j]/tr;
                        }
                    }
                }
            }

            // Back transformation to get eigenvectors of original matrix
            for (var j = order - 1; j > 0; j--)
            {
                var jO = j*order;
                for (var i = 0; i < order; i++)
                {
                    z = Complex32.Zero;
                    for (var k = 0; k <= j; k++)
                    {
                        z += dataEv[(k*order) + i]*matrixH[jO + k];
                    }

                    dataEv[jO + i] = z;
                }
            }
        }

        /// <summary>
        /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
        /// </summary>
        static void GetRow(Transpose transpose, int rowindx, int numRows, int numCols, Complex32[] matrix, Complex32[] row)
        {
            if (transpose == Transpose.DontTranspose)
            {
                for (int i = 0; i < numCols; i++)
                {
                    row[i] = matrix[(i*numRows) + rowindx];
                }
            }
            else if (transpose == Transpose.ConjugateTranspose)
            {
                int offset = rowindx*numCols;
                for (int i = 0; i < row.Length; i++)
                {
                    row[i] = matrix[i + offset].Conjugate();
                }
            }
            else
            {
                Array.Copy(matrix, rowindx*numCols, row, 0, numCols);
            }
        }

        /// <summary>
        /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
        /// </summary>
        static void GetColumn(Transpose transpose, int colindx, int numRows, int numCols, Complex32[] matrix, Complex32[] column)
        {
            if (transpose == Transpose.DontTranspose)
            {
                Array.Copy(matrix, colindx*numRows, column, 0, numRows);
            }
            else if (transpose == Transpose.ConjugateTranspose)
            {
                for (int i = 0; i < numRows; i++)
                {
                    column[i] = matrix[(i*numCols) + colindx].Conjugate();
                }
            }
            else
            {
                for (int i = 0; i < numRows; i++)
                {
                    column[i] = matrix[(i*numCols) + colindx];
                }
            }
        }
    }
}
