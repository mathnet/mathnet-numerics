// <copyright file="ManagedLinearAlgebraProvider.Single.cs" company="Math.NET">
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

            if (alpha == 0.0)
            {
                y.Copy(result);
            }
            else if (alpha == 1.0)
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
        public void ScaleArray(float alpha, float[] x, float[] result)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (alpha == 0.0)
            {
                Array.Clear(result, 0, result.Length);
            }
            else if (alpha == 1.0)
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

            if (y.Length != x.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            float sum = 0.0f;
            for (var index = 0; index < y.Length; index++)
            {
                sum += y[index]*x[index];
            }

            return sum;
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

            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    result[i] = (float)Math.Pow(x[i], y[i]);
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
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        public double MatrixNorm(Norm norm, int rows, int columns, float[] matrix)
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
                            s += Math.Abs(matrix[(j*rows) + i]);
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
                            normMax = Math.Max(Math.Abs(matrix[(j * rows) + i]), normMax);
                        }
                    }
                    return normMax;
                case Norm.InfinityNorm:
                    var r = new double[rows];
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            r[i] += Math.Abs(matrix[(j * rows) + i]);
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
                    var aat = new float[rows*rows];
                    MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.Transpose, 1.0f, matrix, rows, columns, matrix, rows, columns, 0.0f, aat);
                    var normF = 0d;
                    for (var i = 0; i < rows; i++)
                    {
                        normF += Math.Abs(aat[(i * rows) + i]);
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
        public void MatrixMultiply(float[] x, int rowsX, int columnsX, float[] y, int rowsY, int columnsY, float[] result)
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
            var columnDataB = new float[columnsY][];
            for (int i = 0; i < columnDataB.Length; i++)
            {
                var column = new float[rowsY];
                GetColumn(Transpose.DontTranspose, i, rowsY, columnsY, y, column);
                columnDataB[i] = column;
            }

            var shouldNotParallelize = rowsX + columnsY + columnsX < Control.ParallelizeOrder || Control.MaxDegreeOfParallelism < 2;
            if (shouldNotParallelize)
            {
                var row = new float[columnsX];
                for (int i = 0; i < rowsX; i++)
                {
                    GetRow(Transpose.DontTranspose, i, rowsX, columnsX, x, row);
                    for (int j = 0; j < columnsY; j++)
                    {
                        var col = columnDataB[j];
                        float sum = 0;
                        for (int ii = 0; ii < row.Length; ii++)
                        {
                            sum += row[ii] * col[ii];
                        }

                        result[j * rowsX + i] += 1.0f * sum;
                    }
                }
            }
            else
            {
                CommonParallel.For(0, rowsX, 1, (u, v) =>
                {
                    var row = new float[columnsX];
                    for (int i = u; i < v; i++)
                    {
                        GetRow(Transpose.DontTranspose, i, rowsX, columnsX, x, row);
                        for (int j = 0; j < columnsY; j++)
                        {
                            var column = columnDataB[j];
                            float sum = 0;
                            for (int ii = 0; ii < row.Length; ii++)
                            {
                                sum += row[ii] * column[ii];
                            }

                            result[j * rowsX + i] += 1.0f * sum;
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
            if (beta == 0.0)
            {
                Array.Clear(c, 0, c.Length);
            }
            else if (beta != 1.0)
            {
                ScaleArray(beta, c, c);
            }

            if (alpha == 0.0)
            {
                return;
            }

            // Extract column arrays
            var columnDataB = new float[columnsB][];
            for (int i = 0; i < columnDataB.Length; i++)
            {
                var column = new float[rowsB];
                GetColumn(transposeB, i, rowsB, columnsB, b, column);
                columnDataB[i] = column;
            }

            var shouldNotParallelize = rowsA + columnsB + columnsA < Control.ParallelizeOrder || Control.MaxDegreeOfParallelism < 2;
            if (shouldNotParallelize)
            {
                var row = new float[columnsA];
                for (int i = 0; i < rowsA; i++)
                {
                    GetRow(transposeA, i, rowsA, columnsA, a, row);
                    for (int j = 0; j < columnsB; j++)
                    {
                        var col = columnDataB[j];
                        float sum = 0;
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
                    var row = new float[columnsA];
                    for (int i = u; i < v; i++)
                    {
                        GetRow(transposeA, i, rowsA, columnsA, a, row);
                        for (int j = 0; j < columnsB; j++)
                        {
                            var column = columnDataB[j];
                            float sum = 0;
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

            // Initialize the pivot matrix to the identity permutation.
            for (var i = 0; i < order; i++)
            {
                ipiv[i] = i;
            }

            var vecLUcolj = new float[order];

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
                    var s = 0.0f;
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
                    if (Math.Abs(vecLUcolj[i]) > Math.Abs(vecLUcolj[p]))
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
                if (j < order & data[indexjj] != 0.0)
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

            var inverse = new float[a.Length];
            for (var i = 0; i < order; i++)
            {
                inverse[i + (order*i)] = 1.0f;
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
        public void LUSolve(int columnsOfB, float[] a, int order, float[] b)
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
            var clone = new float[a.Length];
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
        public void CholeskyFactor(float[] a, int order)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            var tmpColumn = new float[order];

            // Main loop - along the diagonal
            for (var ij = 0; ij < order; ij++)
            {
                // "Pivot" element
                var tmpVal = a[(ij*order) + ij];

                if (tmpVal > 0.0)
                {
                    tmpVal = (float) Math.Sqrt(tmpVal);
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

                for (int i = ij + 1; i < order; i++)
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
        static void DoCholeskyStep(float[] data, int rowDim, int firstCol, int colLimit, float[] multipliers, int availableCores)
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
                        data[(j*rowDim) + i] -= multipliers[i]*tmpVal;
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

            var clone = new float[a.Length];
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
        static void DoCholeskySolve(float[] a, int orderA, float[] b, int index)
        {
            var cindex = index*orderA;

            // Solve L*Y = B;
            float sum;
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
                    sum -= a[iindex + k]*b[cindex + k];
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

            var work = columnsR > rowsR ? new float[rowsR*rowsR] : new float[rowsR*columnsR];

            CommonParallel.For(0, rowsR, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        q[(i*rowsR) + i] = 1.0f;
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
        public void ThinQRFactor(float[] a, int rowsA, int columnsA, float[] r, float[] tau)
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

            var work = new float[rowsA*columnsA];

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
                a[i*rowsA + i] = 1.0f;
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
        static void ComputeQR(float[] work, int workIndex, float[] a, int rowStart, int rowCount, int columnStart, int columnCount, int availableCores)
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
                    var scale = 0.0f;
                    for (var i = rowStart; i < rowCount; i++)
                    {
                        scale += work[(workIndex*rowCount) + i - rowStart]*a[(j*rowCount) + i];
                    }

                    for (var i = rowStart; i < rowCount; i++)
                    {
                        a[(j*rowCount) + i] -= work[(workIndex*rowCount) + i - rowStart]*scale;
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
        static void GenerateColumn(float[] work, float[] a, int rowCount, int row, int column)
        {
            var tmp = column*rowCount;
            var index = tmp + row;

            CommonParallel.For(row, rowCount, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        var iIndex = tmp + i;
                        work[iIndex - row] = a[iIndex];
                        a[iIndex] = 0.0f;
                    }
                });

            var norm = 0.0;
            for (var i = 0; i < rowCount - row; ++i)
            {
                var iindex = tmp + i;
                norm += work[iindex]*work[iindex];
            }

            norm = Math.Sqrt(norm);
            if (row == rowCount - 1 || norm == 0)
            {
                a[index] = -work[tmp];
                work[tmp] = (float) Constants.Sqrt2;
                return;
            }

            var scale = 1.0f/(float) norm;
            if (work[tmp] < 0.0)
            {
                scale *= -1.0f;
            }

            a[index] = -1.0f/scale;
            CommonParallel.For(0, rowCount - row, 4096, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        work[tmp + i] *= scale;
                    }
                });
            work[tmp] += 1.0f;

            var s = (float) Math.Sqrt(1.0/work[tmp]);
            CommonParallel.For(0, rowCount - row, 4096, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        work[tmp + i] *= s;
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

            var work = new float[rows * columns];

            var clone = new float[a.Length];
            a.Copy(clone);

            if (method == QRMethod.Full)
            {
                var q = new float[rows*rows];
                QRFactor(clone, rows, columns, q, work);
                QRSolveFactored(q, clone, rows, columns, null, b, columnsB, x, method);
            }
            else
            {
                var r = new float[columns*columns];
                ThinQRFactor(clone, rows, columns, r, work);
                QRSolveFactored(clone, r, rows, columns, null, b, columnsB, x, method);
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

            var sol = new float[b.Length];

            // Copy B matrix to "sol", so B data will not be changed
            Buffer.BlockCopy(b, 0, sol, 0, b.Length*Constants.SizeOfFloat);

            // Compute Y = transpose(Q)*B
            var column = new float[rowsA];
            for (var j = 0; j < columnsB; j++)
            {
                var jm = j*rowsA;
                Array.Copy(sol, jm, column, 0, rowsA);
                CommonParallel.For(0, columnsA, (u, v) =>
                    {
                        for (int i = u; i < v; i++)
                        {
                            var im = i*rowsA;

                            var sum = 0.0f;
                            for (var k = 0; k < rowsA; k++)
                            {
                                sum += q[im + k]*column[k];
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

            var work = new float[rowsA];

            const int maxiter = 1000;

            var e = new float[columnsA];
            var v = new float[vt.Length];
            var stemp = new float[Math.Min(rowsA + 1, columnsA)];

            int i, j, l, lp1;

            float t;

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
                    var l1 = l;

                    var sum = 0.0f;
                    for (var i1 = l; i1 < rowsA; i1++)
                    {
                        sum += a[(l1*rowsA) + i1]*a[(l1*rowsA) + i1];
                    }

                    stemp[l] = (float) Math.Sqrt(sum);

                    if (stemp[l] != 0.0)
                    {
                        if (a[(l*rowsA) + l] != 0.0)
                        {
                            stemp[l] = Math.Abs(stemp[l])*(a[(l*rowsA) + l]/Math.Abs(a[(l*rowsA) + l]));
                        }

                        // A part of column "l" of Matrix A from row "l" to end multiply by 1.0 / s[l]
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
                        if (stemp[l] != 0.0)
                        {
                            // Apply the transformation.
                            t = 0.0f;
                            for (i = l; i < rowsA; i++)
                            {
                                t += a[(j*rowsA) + i]*a[(l*rowsA) + i];
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
                    e[j] = a[(j*rowsA) + l];
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
                var enorm = 0.0;
                for (i = lp1; i < e.Length; i++)
                {
                    enorm += e[i]*e[i];
                }

                e[l] = (float) Math.Sqrt(enorm);
                if (e[l] != 0.0)
                {
                    if (e[lp1] != 0.0)
                    {
                        e[l] = Math.Abs(e[l])*(e[lp1]/Math.Abs(e[lp1]));
                    }

                    // Scale vector "e" from "lp1" by 1.0 / e[l]
                    for (i = lp1; i < e.Length; i++)
                    {
                        e[i] = e[i]*(1.0f/e[l]);
                    }

                    e[lp1] = 1.0f + e[lp1];
                }

                e[l] = -e[l];

                if (lp1 < rowsA && e[l] != 0.0)
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
                        var ww = -e[j]/e[lp1];
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
                    if (stemp[l] != 0.0)
                    {
                        for (j = l + 1; j < ncu; j++)
                        {
                            t = 0.0f;
                            for (i = l; i < rowsA; i++)
                            {
                                t += u[(j*rowsA) + i]*u[(l*rowsA) + i];
                            }

                            t = -t/u[(l*rowsA) + l];

                            for (var ii = l; ii < rowsA; ii++)
                            {
                                u[(j*rowsA) + ii] += t*u[(l*rowsA) + ii];
                            }
                        }

                        // A part of column "l" of matrix A from row "l" to end multiply by -1.0
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
                        if (e[l] != 0.0)
                        {
                            for (j = lp1; j < columnsA; j++)
                            {
                                t = 0.0f;
                                for (i = lp1; i < columnsA; i++)
                                {
                                    t += v[(j*columnsA) + i]*v[(l*columnsA) + i];
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

            // Transform "s" and "e" so that they are double
            for (i = 0; i < m; i++)
            {
                float r;
                if (stemp[i] != 0.0)
                {
                    t = stemp[i];
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

                if (e[i] == 0.0)
                {
                    continue;
                }

                t = e[i];
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
                double ztest;
                double test;
                for (l = m - 2; l >= 0; l--)
                {
                    test = Math.Abs(stemp[l]) + Math.Abs(stemp[l + 1]);
                    ztest = test + Math.Abs(e[l]);
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
                        test = 0.0;
                        if (ls != m - 1)
                        {
                            test = test + Math.Abs(e[ls]);
                        }

                        if (ls != l + 1)
                        {
                            test = test + Math.Abs(e[ls - 1]);
                        }

                        ztest = test + Math.Abs(stemp[ls]);
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
                float cs;
                float sn;
                switch (kase)
                {
                        // Deflate negligible s[m].
                    case 1:
                        f = e[m - 2];
                        e[m - 2] = 0.0f;
                        float t1;
                        for (var kk = l; kk < m - 1; kk++)
                        {
                            k = m - 2 - kk + l;
                            t1 = stemp[k];

                            Drotg(ref t1, ref f, out cs, out sn);
                            stemp[k] = t1;
                            if (k != l)
                            {
                                f = -sn*e[k - 1];
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
                        f = e[l - 1];
                        e[l - 1] = 0.0f;
                        for (k = l; k < m; k++)
                        {
                            t1 = stemp[k];
                            Drotg(ref t1, ref f, out cs, out sn);
                            stemp[k] = t1;
                            f = -sn*e[k];
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
                        scale = Math.Max(scale, Math.Abs(stemp[m - 1]));
                        scale = Math.Max(scale, Math.Abs(stemp[m - 2]));
                        scale = Math.Max(scale, Math.Abs(e[m - 2]));
                        scale = Math.Max(scale, Math.Abs(stemp[l]));
                        scale = Math.Max(scale, Math.Abs(e[l]));
                        var sm = stemp[m - 1]/scale;
                        var smm1 = stemp[m - 2]/scale;
                        var emm1 = e[m - 2]/scale;
                        var sl = stemp[l]/scale;
                        var el = e[l]/scale;
                        var b = (((smm1 + sm)*(smm1 - sm)) + (emm1*emm1))/2.0f;
                        var c = (sm*emm1)*(sm*emm1);
                        var shift = 0.0f;
                        if (b != 0.0 || c != 0.0)
                        {
                            shift = (float) Math.Sqrt((b*b) + c);
                            if (b < 0.0)
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

                            f = (cs*stemp[k]) + (sn*e[k]);
                            e[k] = (cs*e[k]) - (sn*stemp[k]);
                            g = sn*stemp[k + 1];
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
                            f = (cs*e[k]) + (sn*stemp[k + 1]);
                            stemp[k + 1] = -(sn*e[k]) + (cs*stemp[k + 1]);
                            g = sn*e[k + 1];
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
                        if (stemp[l] < 0.0)
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
                            if (stemp[l] >= stemp[l + 1])
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
                        vt[(j*columnsA) + i] = v[(i*columnsA) + j];
                    }
                }
            }

            // Copy stemp to s with size adjustment. We are using ported copy of linpack's svd code and it uses
            // a singular vector of length rows+1 when rows < columns. The last element is not used and needs to be removed.
            // We should port lapack's svd routine to remove this problem.
            Buffer.BlockCopy(stemp, 0, s, 0, Math.Min(rowsA, columnsA)*Constants.SizeOfFloat);
        }

        /// <summary>
        /// Given the Cartesian coordinates (da, db) of a point p, these function return the parameters da, db, c, and s
        /// associated with the Givens rotation that zeros the y-coordinate of the point.
        /// </summary>
        /// <param name="da">Provides the x-coordinate of the point p. On exit contains the parameter r associated with the Givens rotation</param>
        /// <param name="db">Provides the y-coordinate of the point p. On exit contains the parameter z associated with the Givens rotation</param>
        /// <param name="c">Contains the parameter c associated with the Givens rotation</param>
        /// <param name="s">Contains the parameter s associated with the Givens rotation</param>
        /// <remarks>This is equivalent to the DROTG LAPACK routine.</remarks>
        static void Drotg(ref float da, ref float db, out float c, out float s)
        {
            float r, z;

            var roe = db;
            var absda = Math.Abs(da);
            var absdb = Math.Abs(db);
            if (absda > absdb)
            {
                roe = da;
            }

            var scale = absda + absdb;
            if (scale == 0.0)
            {
                c = 1.0f;
                s = 0.0f;
                r = 0.0f;
                z = 0.0f;
            }
            else
            {
                var sda = da/scale;
                var sdb = db/scale;
                r = scale*(float) Math.Sqrt((sda*sda) + (sdb*sdb));
                if (roe < 0.0)
                {
                    r = -r;
                }

                c = da/r;
                s = db/r;
                z = 1.0f;
                if (absda > absdb)
                {
                    z = s;
                }

                if (absdb >= absda && c != 0.0)
                {
                    z = 1.0f/c;
                }
            }

            da = r;
            db = z;
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
            Buffer.BlockCopy(a, 0, clone, 0, a.Length*Constants.SizeOfFloat);
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
            var tmp = new float[columnsA];

            for (var k = 0; k < columnsB; k++)
            {
                for (var j = 0; j < columnsA; j++)
                {
                    float value = 0;
                    if (j < mn)
                    {
                        for (var i = 0; i < rowsA; i++)
                        {
                            value += u[(j*rowsA) + i]*b[(k*rowsA) + i];
                        }

                        value /= s[j];
                    }

                    tmp[j] = value;
                }

                for (var j = 0; j < columnsA; j++)
                {
                    float value = 0;
                    for (var i = 0; i < columnsA; i++)
                    {
                        value += vt[(j*columnsA) + i]*tmp[i];
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

            var d = new float[order];
            var e = new float[order];

            if (isSymmetric)
            {
                Buffer.BlockCopy(matrix, 0, matrixEv, 0, matrix.Length*Constants.SizeOfFloat);
                var om1 = order - 1;
                for (var i = 0; i < order; i++)
                {
                    d[i] = matrixEv[i*order + om1];
                }

                SymmetricTridiagonalize(matrixEv, d, e, order);
                SymmetricDiagonalize(matrixEv, d, e, order);
            }
            else
            {
                var matrixH = new float[matrix.Length];
                Buffer.BlockCopy(matrix, 0, matrixH, 0, matrix.Length*Constants.SizeOfFloat);
                NonsymmetricReduceToHessenberg(matrixEv, matrixH, order);
                NonsymmetricReduceHessenberToRealSchur(matrixEv, matrixH, d, e, order);
            }

            for (var i = 0; i < order; i++)
            {
                vectorEv[i] = new Complex(d[i], e[i]);

                var io = i*order;
                matrixD[io + i] = d[i];

                if (e[i] > 0)
                {
                    matrixD[io + order + i] = e[i];
                }
                else if (e[i] < 0)
                {
                    matrixD[io - order + i] = e[i];
                }
            }
        }

        /// <summary>
        /// Symmetric Householder reduction to tridiagonal form.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tred2 by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        internal static void SymmetricTridiagonalize(float[] a, float[] d, float[] e, int order)
        {
            // Householder reduction to tridiagonal form.
            for (var i = order - 1; i > 0; i--)
            {
                // Scale to avoid under/overflow.
                var scale = 0.0f;
                var h = 0.0f;

                for (var k = 0; k < i; k++)
                {
                    scale = scale + Math.Abs(d[k]);
                }

                if (scale == 0.0f)
                {
                    e[i] = d[i - 1];
                    for (var j = 0; j < i; j++)
                    {
                        d[j] = a[(j*order) + i - 1];
                        a[(j*order) + i] = 0.0f;
                        a[(i*order) + j] = 0.0f;
                    }
                }
                else
                {
                    // Generate Householder vector.
                    for (var k = 0; k < i; k++)
                    {
                        d[k] /= scale;
                        h += d[k]*d[k];
                    }

                    var f = d[i - 1];
                    var g = (float) Math.Sqrt(h);
                    if (f > 0)
                    {
                        g = -g;
                    }

                    e[i] = scale*g;
                    h = h - (f*g);
                    d[i - 1] = f - g;

                    for (var j = 0; j < i; j++)
                    {
                        e[j] = 0.0f;
                    }

                    // Apply similarity transformation to remaining columns.
                    for (var j = 0; j < i; j++)
                    {
                        f = d[j];
                        a[(i*order) + j] = f;
                        g = e[j] + (a[(j*order) + j]*f);

                        for (var k = j + 1; k <= i - 1; k++)
                        {
                            g += a[(j*order) + k]*d[k];
                            e[k] += a[(j*order) + k]*f;
                        }

                        e[j] = g;
                    }

                    f = 0.0f;

                    for (var j = 0; j < i; j++)
                    {
                        e[j] /= h;
                        f += e[j]*d[j];
                    }

                    var hh = f/(h + h);

                    for (var j = 0; j < i; j++)
                    {
                        e[j] -= hh*d[j];
                    }

                    for (var j = 0; j < i; j++)
                    {
                        f = d[j];
                        g = e[j];

                        for (var k = j; k <= i - 1; k++)
                        {
                            a[(j*order) + k] -= (f*e[k]) + (g*d[k]);
                        }

                        d[j] = a[(j*order) + i - 1];
                        a[(j*order) + i] = 0.0f;
                    }
                }

                d[i] = h;
            }

            // Accumulate transformations.
            for (var i = 0; i < order - 1; i++)
            {
                a[(i*order) + order - 1] = a[(i*order) + i];
                a[(i*order) + i] = 1.0f;
                var h = d[i + 1];
                if (h != 0.0f)
                {
                    for (var k = 0; k <= i; k++)
                    {
                        d[k] = a[((i + 1)*order) + k]/h;
                    }

                    for (var j = 0; j <= i; j++)
                    {
                        var g = 0.0f;
                        for (var k = 0; k <= i; k++)
                        {
                            g += a[((i + 1)*order) + k]*a[(j*order) + k];
                        }

                        for (var k = 0; k <= i; k++)
                        {
                            a[(j*order) + k] -= g*d[k];
                        }
                    }
                }

                for (var k = 0; k <= i; k++)
                {
                    a[((i + 1)*order) + k] = 0.0f;
                }
            }

            for (var j = 0; j < order; j++)
            {
                d[j] = a[(j*order) + order - 1];
                a[(j*order) + order - 1] = 0.0f;
            }

            a[(order*order) - 1] = 1.0f;
            e[0] = 0.0f;
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tql2, by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        internal static void SymmetricDiagonalize(float[] a, float[] d, float[] e, int order)
        {
            const int maxiter = 1000;

            for (var i = 1; i < order; i++)
            {
                e[i - 1] = e[i];
            }

            e[order - 1] = 0.0f;

            var f = 0.0f;
            var tst1 = 0.0f;
            var eps = Precision.SinglePrecision;
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
                                h = a[((i + 1)*order) + k];
                                a[((i + 1)*order) + k] = (s*a[(i*order) + k]) + (c*h);
                                a[(i*order) + k] = (c*a[(i*order) + k]) - (s*h);
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
                        p = a[(i*order) + j];
                        a[(i*order) + j] = a[(k*order) + j];
                        a[(k*order) + j] = p;
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction to Hessenberg form.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures orthes and ortran,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutines in EISPACK.</remarks>
        internal static void NonsymmetricReduceToHessenberg(float[] a, float[] matrixH, int order)
        {
            var ort = new float[order];
            var high = order - 1;
            for (var m = 1; m <= high - 1; m++)
            {
                var mm1 = m - 1;
                var mm1O = mm1*order;
                // Scale column.
                var scale = 0.0f;
                for (var i = m; i <= high; i++)
                {
                    scale += Math.Abs(matrixH[mm1O + i]);
                }

                if (scale != 0.0f)
                {
                    // Compute Householder transformation.
                    var h = 0.0f;
                    for (var i = high; i >= m; i--)
                    {
                        ort[i] = matrixH[mm1O + i]/scale;
                        h += ort[i]*ort[i];
                    }

                    var g = (float) Math.Sqrt(h);
                    if (ort[m] > 0)
                    {
                        g = -g;
                    }

                    h = h - (ort[m]*g);
                    ort[m] = ort[m] - g;

                    // Apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)
                    for (var j = m; j < order; j++)
                    {
                        var jO = j*order;
                        var f = 0.0f;
                        for (var i = order - 1; i >= m; i--)
                        {
                            f += ort[i]*matrixH[jO + i];
                        }

                        f = f/h;

                        for (var i = m; i <= high; i++)
                        {
                            matrixH[jO + i] -= f*ort[i];
                        }
                    }

                    for (var i = 0; i <= high; i++)
                    {
                        var f = 0.0f;
                        for (var j = high; j >= m; j--)
                        {
                            f += ort[j]*matrixH[j*order + i];
                        }
                        f = f/h;

                        for (var j = m; j <= high; j++)
                        {
                            matrixH[j*order + i] -= f*ort[j];
                        }
                    }

                    ort[m] = scale*ort[m];
                    matrixH[mm1O + m] = scale*g;
                }
            }

            // Accumulate transformations (Algol's ortran).
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    a[(j*order) + i] = i == j ? 1.0f : 0.0f;
                }
            }

            for (var m = high - 1; m >= 1; m--)
            {
                var mm1 = m - 1;
                var mm1O = mm1*order;
                var mm1Om = mm1O + m;
                if (matrixH[mm1Om] != 0.0)
                {
                    for (var i = m + 1; i <= high; i++)
                    {
                        ort[i] = matrixH[mm1O + i];
                    }

                    for (var j = m; j <= high; j++)
                    {
                        var g = 0.0f;
                        var jO = j*order;
                        for (var i = m; i <= high; i++)
                        {
                            g += ort[i]*a[jO + i];
                        }

                        // Double division avoids possible underflow
                        g = (g/ort[m])/matrixH[mm1Om];

                        for (var i = m; i <= high; i++)
                        {
                            a[jO + i] += g*ort[i];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction from Hessenberg to real Schur form.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedure hqr2,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        internal static void NonsymmetricReduceHessenberToRealSchur(float[] a, float[] matrixH, float[] d, float[] e, int order)
        {
            // Initialize
            var n = order - 1;
            var eps = (float) Precision.SinglePrecision;
            var exshift = 0.0f;
            float p = 0, q = 0, r = 0, s = 0, z = 0;
            float w, x, y;

            // Store roots isolated by balanc and compute matrix norm
            var norm = 0.0f;
            for (var i = 0; i < order; i++)
            {
                for (var j = Math.Max(i - 1, 0); j < order; j++)
                {
                    norm = norm + Math.Abs(matrixH[j*order + i]);
                }
            }

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
                    s = Math.Abs(matrixH[lm1O + lm1]) + Math.Abs(matrixH[l*order + l]);

                    if (s == 0.0)
                    {
                        s = norm;
                    }

                    if (Math.Abs(matrixH[lm1O + l]) < eps*s)
                    {
                        break;
                    }

                    l--;
                }

                // Check for convergence
                // One root found
                if (l == n)
                {
                    var index = n*order + n;
                    matrixH[index] += exshift;
                    d[n] = matrixH[index];
                    e[n] = 0.0f;
                    n--;
                    iter = 0;

                    // Two roots found
                }
                else if (l == n - 1)
                {
                    var nO = n*order;
                    var nm1 = n - 1;
                    var nm1O = nm1*order;
                    var nOn = nO + n;

                    w = matrixH[nm1O + n]*matrixH[nO + nm1];
                    p = (matrixH[nm1O + nm1] - matrixH[nOn])/2.0f;
                    q = (p*p) + w;
                    z = (float) Math.Sqrt(Math.Abs(q));

                    matrixH[nOn] += exshift;
                    matrixH[nm1O + nm1] += exshift;
                    x = matrixH[nOn];

                    // Real pair
                    if (q >= 0)
                    {
                        if (p >= 0)
                        {
                            z = p + z;
                        }
                        else
                        {
                            z = p - z;
                        }

                        d[nm1] = x + z;

                        d[n] = d[nm1];
                        if (z != 0.0)
                        {
                            d[n] = x - (w/z);
                        }

                        e[n - 1] = 0.0f;
                        e[n] = 0.0f;
                        x = matrixH[nm1O + n];
                        s = Math.Abs(x) + Math.Abs(z);
                        p = x/s;
                        q = z/s;
                        r = (float) Math.Sqrt((p*p) + (q*q));
                        p = p/r;
                        q = q/r;

                        // Row modification
                        for (var j = n - 1; j < order; j++)
                        {
                            var jO = j*order;
                            var jOn = jO + n;
                            z = matrixH[jO + nm1];
                            matrixH[jO + nm1] = (q*z) + (p*matrixH[jOn]);
                            matrixH[jOn] = (q*matrixH[jOn]) - (p*z);
                        }

                        // Column modification
                        for (var i = 0; i <= n; i++)
                        {
                            var nOi = nO + i;
                            z = matrixH[nm1O + i];
                            matrixH[nm1O + i] = (q*z) + (p*matrixH[nOi]);
                            matrixH[nOi] = (q*matrixH[nOi]) - (p*z);
                        }

                        // Accumulate transformations
                        for (var i = 0; i < order; i++)
                        {
                            var nOi = nO + i;
                            z = a[nm1O + i];
                            a[nm1O + i] = (q*z) + (p*a[nOi]);
                            a[nOi] = (q*a[nOi]) - (p*z);
                        }

                        // Complex pair
                    }
                    else
                    {
                        d[n - 1] = x + p;
                        d[n] = x + p;
                        e[n - 1] = z;
                        e[n] = -z;
                    }

                    n = n - 2;
                    iter = 0;

                    // No convergence yet
                }
                else
                {
                    var nO = n*order;
                    var nm1 = n - 1;
                    var nm1O = nm1*order;
                    var nOn = nO + n;

                    // Form shift
                    x = matrixH[nOn];
                    y = 0.0f;
                    w = 0.0f;
                    if (l < n)
                    {
                        y = matrixH[nm1O + nm1];
                        w = matrixH[nm1O + n]*matrixH[nO + nm1];
                    }

                    // Wilkinson's original ad hoc shift
                    if (iter == 10)
                    {
                        exshift += x;
                        for (var i = 0; i <= n; i++)
                        {
                            matrixH[i*order + i] -= x;
                        }

                        s = Math.Abs(matrixH[nm1O + n]) + Math.Abs(matrixH[(n - 2)*order + nm1]);
                        x = y = 0.75f*s;
                        w = (-0.4375f)*s*s;
                    }

                    // MATLAB's new ad hoc shift
                    if (iter == 30)
                    {
                        s = (y - x)/2.0f;
                        s = (s*s) + w;
                        if (s > 0)
                        {
                            s = (float) Math.Sqrt(s);
                            if (y < x)
                            {
                                s = -s;
                            }

                            s = x - (w/(((y - x)/2.0f) + s));
                            for (var i = 0; i <= n; i++)
                            {
                                matrixH[i*order + i] -= s;
                            }

                            exshift += s;
                            x = y = w = 0.964f;
                        }
                    }

                    iter = iter + 1;
                    if (iter >= 30*order)
                    {
                        throw new NonConvergenceException();
                    }

                    // Look for two consecutive small sub-diagonal elements
                    var m = n - 2;
                    while (m >= l)
                    {
                        var mp1 = m + 1;
                        var mm1 = m - 1;
                        var mO = m*order;
                        var mp1O = mp1*order;
                        var mm1O = mm1*order;

                        z = matrixH[mO + m];
                        r = x - z;
                        s = y - z;
                        p = (((r*s) - w)/matrixH[mO + mp1]) + matrixH[mp1O + m];
                        q = matrixH[mp1O + mp1] - z - r - s;
                        r = matrixH[mp1O + (m + 2)];
                        s = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                        p = p/s;
                        q = q/s;
                        r = r/s;

                        if (m == l)
                        {
                            break;
                        }

                        if (Math.Abs(matrixH[mm1O + m])*(Math.Abs(q) + Math.Abs(r)) < eps*(Math.Abs(p)*(Math.Abs(matrixH[mm1O + mm1]) + Math.Abs(z) + Math.Abs(matrixH[mp1O + mp1]))))
                        {
                            break;
                        }

                        m--;
                    }

                    var mp2 = m + 2;
                    for (var i = mp2; i <= n; i++)
                    {
                        matrixH[(i - 2)*order + i] = 0.0f;
                        if (i > mp2)
                        {
                            matrixH[(i - 3)*order + i] = 0.0f;
                        }
                    }

                    // Double QR step involving rows l:n and columns m:n
                    for (var k = m; k <= n - 1; k++)
                    {
                        var notlast = k != n - 1;
                        var kO = k*order;
                        var km1 = k - 1;
                        var kp1 = k + 1;
                        var kp2 = k + 2;
                        var kp1O = kp1*order;
                        var kp2O = kp2*order;
                        var km1O = km1*order;
                        if (k != m)
                        {
                            p = matrixH[km1O + k];
                            q = matrixH[km1O + kp1];
                            r = notlast ? matrixH[km1O + kp2] : 0.0f;
                            x = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                            if (x == 0.0f)
                            {
                                continue;
                            }

                            p = p/x;
                            q = q/x;
                            r = r/x;
                        }

                        s = (float) Math.Sqrt((p*p) + (q*q) + (r*r));

                        if (p < 0)
                        {
                            s = -s;
                        }

                        if (s != 0.0f)
                        {
                            if (k != m)
                            {
                                matrixH[km1O + k] = (-s)*x;
                            }
                            else if (l != m)
                            {
                                matrixH[km1O + k] = -matrixH[km1O + k];
                            }

                            p = p + s;
                            x = p/s;
                            y = q/s;
                            z = r/s;
                            q = q/p;
                            r = r/p;

                            // Row modification
                            for (var j = k; j < order; j++)
                            {
                                var jO = j*order;
                                var jOk = jO + k;
                                var jOkp1 = jO + kp1;
                                var jOkp2 = jO + kp2;
                                p = matrixH[jOk] + (q*matrixH[jOkp1]);
                                if (notlast)
                                {
                                    p = p + (r*matrixH[jOkp2]);
                                    matrixH[jOkp2] -= (p*z);
                                }

                                matrixH[jOk] -= (p*x);
                                matrixH[jOkp1] -= (p*y);
                            }

                            // Column modification
                            for (var i = 0; i <= Math.Min(n, k + 3); i++)
                            {
                                p = (x*matrixH[kO + i]) + (y*matrixH[kp1O + i]);

                                if (notlast)
                                {
                                    p = p + (z*matrixH[kp2O + i]);
                                    matrixH[kp2O + i] -= (p*r);
                                }

                                matrixH[kO + i] -= p;
                                matrixH[kp1O + i] -= (p*q);
                            }

                            // Accumulate transformations
                            for (var i = 0; i < order; i++)
                            {
                                p = (x*a[kO + i]) + (y*a[kp1O + i]);

                                if (notlast)
                                {
                                    p = p + (z*a[kp2O + i]);
                                    a[kp2O + i] -= p*r;
                                }

                                a[kO + i] -= p;
                                a[kp1O + i] -= p*q;
                            }
                        } // (s != 0)
                    } // k loop
                } // check convergence
            } // while (n >= low)

            // Backsubstitute to find vectors of upper triangular form
            if (norm == 0.0f)
            {
                return;
            }

            for (n = order - 1; n >= 0; n--)
            {
                var nO = n*order;
                var nm1 = n - 1;
                var nm1O = nm1*order;

                p = d[n];
                q = e[n];


                // Real vector
                float t;
                if (q == 0.0f)
                {
                    var l = n;
                    matrixH[nO + n] = 1.0f;
                    for (var i = n - 1; i >= 0; i--)
                    {
                        var ip1 = i + 1;
                        var iO = i*order;
                        var ip1O = ip1*order;

                        w = matrixH[iO + i] - p;
                        r = 0.0f;
                        for (var j = l; j <= n; j++)
                        {
                            r = r + (matrixH[j*order + i]*matrixH[nO + j]);
                        }

                        if (e[i] < 0.0)
                        {
                            z = w;
                            s = r;
                        }
                        else
                        {
                            l = i;
                            if (e[i] == 0.0f)
                            {
                                if (w != 0.0f)
                                {
                                    matrixH[nO + i] = (-r)/w;
                                }
                                else
                                {
                                    matrixH[nO + i] = (-r)/(eps*norm);
                                }

                                // Solve real equations
                            }
                            else
                            {
                                x = matrixH[ip1O + i];
                                y = matrixH[iO + ip1];
                                q = ((d[i] - p)*(d[i] - p)) + (e[i]*e[i]);
                                t = ((x*s) - (z*r))/q;
                                matrixH[nO + i] = t;
                                if (Math.Abs(x) > Math.Abs(z))
                                {
                                    matrixH[nO + ip1] = (-r - (w*t))/x;
                                }
                                else
                                {
                                    matrixH[nO + ip1] = (-s - (y*t))/z;
                                }
                            }

                            // Overflow control
                            t = Math.Abs(matrixH[nO + i]);
                            if ((eps*t)*t > 1)
                            {
                                for (var j = i; j <= n; j++)
                                {
                                    matrixH[nO + j] /= t;
                                }
                            }
                        }
                    }

                    // Complex vector
                }
                else if (q < 0)
                {
                    var l = n - 1;

                    // Last vector component imaginary so matrix is triangular
                    if (Math.Abs(matrixH[nm1O + n]) > Math.Abs(matrixH[nO + nm1]))
                    {
                        matrixH[nm1O + nm1] = q/matrixH[nm1O + n];
                        matrixH[nO + nm1] = (-(matrixH[nO + n] - p))/matrixH[nm1O + n];
                    }
                    else
                    {
                        var res = Cdiv(0.0f, -matrixH[nO + nm1], matrixH[nm1O + nm1] - p, q);
                        matrixH[nm1O + nm1] = res.Real;
                        matrixH[nO + nm1] = res.Imaginary;
                    }

                    matrixH[nm1O + n] = 0.0f;
                    matrixH[nO + n] = 1.0f;
                    for (var i = n - 2; i >= 0; i--)
                    {
                        var ip1 = i + 1;
                        var iO = i*order;
                        var ip1O = ip1*order;
                        var ra = 0.0f;
                        var sa = 0.0f;
                        for (var j = l; j <= n; j++)
                        {
                            var jO = j*order;
                            var jOi = jO + i;
                            ra = ra + (matrixH[jOi]*matrixH[nm1O + j]);
                            sa = sa + (matrixH[jOi]*matrixH[nO + j]);
                        }

                        w = matrixH[iO + i] - p;

                        if (e[i] < 0.0)
                        {
                            z = w;
                            r = ra;
                            s = sa;
                        }
                        else
                        {
                            l = i;
                            if (e[i] == 0.0)
                            {
                                var res = Cdiv(-ra, -sa, w, q);
                                matrixH[nm1O + i] = res.Real;
                                matrixH[nO + i] = res.Imaginary;
                            }
                            else
                            {
                                // Solve complex equations
                                x = matrixH[ip1O + i];
                                y = matrixH[iO + ip1];

                                var vr = ((d[i] - p)*(d[i] - p)) + (e[i]*e[i]) - (q*q);
                                var vi = (d[i] - p)*2.0f*q;
                                if ((vr == 0.0f) && (vi == 0.0f))
                                {
                                    vr = eps*norm*(Math.Abs(w) + Math.Abs(q) + Math.Abs(x) + Math.Abs(y) + Math.Abs(z));
                                }

                                var res = Cdiv((x*r) - (z*ra) + (q*sa), (x*s) - (z*sa) - (q*ra), vr, vi);
                                matrixH[nm1O + i] = res.Real;
                                matrixH[nO + i] = res.Imaginary;
                                if (Math.Abs(x) > (Math.Abs(z) + Math.Abs(q)))
                                {
                                    matrixH[nm1O + ip1] = (-ra - (w*matrixH[nm1O + i]) + (q*matrixH[nO + i]))/x;
                                    matrixH[nO + ip1] = (-sa - (w*matrixH[nO + i]) - (q*matrixH[nm1O + i]))/x;
                                }
                                else
                                {
                                    res = Cdiv(-r - (y*matrixH[nm1O + i]), -s - (y*matrixH[nO + i]), z, q);
                                    matrixH[nm1O + ip1] = res.Real;
                                    matrixH[nO + ip1] = res.Imaginary;
                                }
                            }

                            // Overflow control
                            t = Math.Max(Math.Abs(matrixH[nm1O + i]), Math.Abs(matrixH[nO + i]));
                            if ((eps*t)*t > 1)
                            {
                                for (var j = i; j <= n; j++)
                                {
                                    matrixH[nm1O + j] /= t;
                                    matrixH[nO + j] /= t;
                                }
                            }
                        }
                    }
                }
            }

            // Back transformation to get eigenvectors of original matrix
            for (var j = order - 1; j >= 0; j--)
            {
                var jO = j*order;
                for (var i = 0; i < order; i++)
                {
                    z = 0.0f;
                    for (var k = 0; k <= j; k++)
                    {
                        z = z + (a[k*order + i]*matrixH[jO + k]);
                    }

                    a[jO + i] = z;
                }
            }
        }

        /// <summary>
        /// Complex scalar division X/Y.
        /// </summary>
        /// <param name="xreal">Real part of X</param>
        /// <param name="ximag">Imaginary part of X</param>
        /// <param name="yreal">Real part of Y</param>
        /// <param name="yimag">Imaginary part of Y</param>
        /// <returns>Division result as a <see cref="Complex"/> number.</returns>
        static Complex32 Cdiv(float xreal, float ximag, float yreal, float yimag)
        {
            if (Math.Abs(yimag) < Math.Abs(yreal))
            {
                return new Complex32((xreal + (ximag*(yimag/yreal)))/(yreal + (yimag*(yimag/yreal))), (ximag - (xreal*(yimag/yreal)))/(yreal + (yimag*(yimag/yreal))));
            }

            return new Complex32((ximag + (xreal*(yreal/yimag)))/(yimag + (yreal*(yreal/yimag))), (-xreal + (ximag*(yreal/yimag)))/(yimag + (yreal*(yreal/yimag))));
        }
    }
}
