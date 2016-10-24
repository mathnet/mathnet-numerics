using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra.Mkl;
using MathNet.Numerics.Threading;

namespace Benchmark.LinearAlgebra
{
    public class DenseMatrixProduct
    {
        readonly Dictionary<string, Matrix<double>> _data = new Dictionary<string, Matrix<double>>();

        readonly ILinearAlgebraProvider _managed;
        readonly ILinearAlgebraProvider _managedExperimental;
        readonly ILinearAlgebraProvider _mkl;
        readonly ILinearAlgebraProvider _experimental;

        [Params(8, 64, 128)]
        public int M { get; set; }

        [Params(8, 64, 128)]
        public int N { get; set; }

        static string Key(int m, int n)
        {
            return $"{m}x{n}";
        }

        public DenseMatrixProduct()
        {
            foreach (var m in new[] {8, 64, 128})
            foreach (var n in new[] {8, 64, 128})
            {
                var key = Key(m, n);
                _data[key] = Matrix<double>.Build.Random(m, n);
            }

            Control.NativeProviderPath = @"..\..\..\..\out\MKL\Windows\";
            _managed = new ManagedLinearAlgebraProvider();
            _managedExperimental = new ManagedLinearAlgebraProvider(Variation.Experimental);
            _mkl = new MklLinearAlgebraProvider();
            _experimental = new ExperimentalProvider();

            _managed.InitializeVerify();
            _managedExperimental.InitializeVerify();
            _mkl.InitializeVerify();
            _experimental.InitializeVerify();

            //Verify();
        }

        private void Verify()
        {
            M = 8;
            N = 8;
            var resultMkl = MathNet().ToRowArrays();
            var resultManaged = MathNetManaged().ToRowArrays();
            var resultManagedExperimental = MathNetManagedExperimental().ToRowArrays();
            var resultExperimental = MathNetExperimental().ToRowArrays();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!resultMkl[i][j].AlmostEqual(resultManaged[i][j], 1e-14))
                    {
                        throw new Exception($"Managed [{i}][{j}] {resultManaged[i][j]} != {resultMkl[i][j]}");
                    }
                    if (!resultMkl[i][j].AlmostEqual(resultManagedExperimental[i][j], 1e-14))
                    {
                        throw new Exception($"ManagedExperimental [{i}][{j}] {resultManagedExperimental[i][j]} != {resultMkl[i][j]}");
                    }
                    if (!resultMkl[i][j].AlmostEqual(resultExperimental[i][j], 1e-14))
                    {
                        throw new Exception($"Experimental [{i}][{j}] {resultExperimental[i][j]} != {resultMkl[i][j]}");
                    }
                }
            }
        }

        [Setup]
        public void Setup()
        {
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNet()
        {
            Control.LinearAlgebraProvider = _mkl;
            return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetManaged()
        {
            Control.LinearAlgebraProvider = _managed;
            return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetManagedExperimental()
        {
            Control.LinearAlgebraProvider = _managedExperimental;
            return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetExperimental()
        {
            Control.LinearAlgebraProvider = _experimental;
            return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
        }

        public class SafeProvider : ManagedLinearAlgebraProvider
        {
            public override void MatrixMultiply(
                double[] x, int rowsX, int columnsX, double[] y, int rowsY, int columnsY, double[] result)
            {
                if (rowsX + columnsY <= Control.MaxDegreeOfParallelism)
                {
                    for (int i = 0; i < rowsX; ++i)
                    {
                        for (int j = 0; j < columnsY; ++j)
                        {
                            var jrowsY = j*rowsY;
                            double sum = 0.0;
                            for (int k = 0; k < columnsX; ++k)
                            {
                                sum += x[k*rowsX + i]*y[jrowsY + k];
                            }
                            result[j*rowsX + i] = sum;
                        }
                    }

                    return;
                }

                double[] xdata;
                if (ReferenceEquals(x, result))
                {
                    xdata = (double[]) x.Clone();
                }
                else
                {
                    xdata = x;
                }

                double[] ydata;
                if (ReferenceEquals(y, result))
                {
                    ydata = (double[]) y.Clone();
                }
                else
                {
                    ydata = y;
                }

                Array.Clear(result, 0, result.Length);

                CacheObliviousMatrixMultiply(xdata, 0, 0, ydata, 0, 0, result, 0, 0, rowsX, columnsY, columnsX, rowsX,
                    columnsY, columnsX, 0);
            }

            public override void MatrixMultiplyWithUpdate(
                Transpose transposeA, Transpose transposeB, double alpha, double[] a, int rowsA, int columnsA,
                double[] b,
                int rowsB, int columnsB, double beta, double[] c)
            {
                if (transposeA == Transpose.DontTranspose && transposeB == Transpose.DontTranspose && alpha == 1.0 &&
                    beta == 0.0)
                {
                    MatrixMultiply(a, rowsA, columnsA, b, rowsB, columnsB, c);
                    return;
                }

                base.MatrixMultiplyWithUpdate(transposeA, transposeB, alpha, a, rowsA, columnsA, b, rowsB, columnsB,
                    beta, c);
            }

            static void CacheObliviousMatrixMultiply(
                double[] matrixA, int shiftArow, int shiftAcol, double[] matrixB, int shiftBrow, int shiftBcol,
                double[] result, int shiftCrow, int shiftCcol, int m, int n, int k, int constM, int constN, int constK,
                int level)
            {
                if (m + n <= Control.MaxDegreeOfParallelism)
                {
                    for (var m1 = 0; m1 < m; m1++)
                    {
                        var matArowPos = m1 + shiftArow;
                        var matCrowPos = m1 + shiftCrow;
                        for (var n1 = 0; n1 < n; ++n1)
                        {
                            var boffset = ((n1 + shiftBcol)*constK) + shiftBrow;
                            double sum = 0;
                            for (var k1 = 0; k1 < k; ++k1)
                            {
                                sum += matrixA[((k1 + shiftAcol)*constM) + matArowPos]*matrixB[boffset + k1];
                            }

                            result[((n1 + shiftCcol)*constM) + matCrowPos] += sum;
                        }
                    }

                    return;
                }

                // divide and conquer
                int m2 = m/2, n2 = n/2, k2 = k/2;

                level++;
                if (level <= 2)
                {
                    CommonParallel.Invoke(
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol, matrixB, shiftBrow, shiftBcol,
                                result, shiftCrow, shiftCcol, m2, n2, k2, constM, constN, constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol, matrixB, shiftBrow,
                                shiftBcol + n2,
                                result, shiftCrow, shiftCcol + n2, m2, n - n2, k2, constM, constN, constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol, matrixB, shiftBrow,
                                shiftBcol,
                                result, shiftCrow + m2, shiftCcol, m - m2, n2, k2, constM, constN, constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol, matrixB, shiftBrow,
                                shiftBcol + n2, result, shiftCrow + m2, shiftCcol + n2, m - m2, n - n2, k2, constM,
                                constN,
                                constK, level));

                    CommonParallel.Invoke(
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol + k2, matrixB, shiftBrow + k2,
                                shiftBcol, result, shiftCrow, shiftCcol, m2, n2, k - k2, constM, constN, constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol + k2, matrixB, shiftBrow + k2,
                                shiftBcol + n2, result, shiftCrow, shiftCcol + n2, m2, n - n2, k - k2, constM, constN,
                                constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol + k2, matrixB,
                                shiftBrow + k2,
                                shiftBcol, result, shiftCrow + m2, shiftCcol, m - m2, n2, k - k2, constM, constN, constK,
                                level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol + k2, matrixB,
                                shiftBrow + k2,
                                shiftBcol + n2, result, shiftCrow + m2, shiftCcol + n2, m - m2, n - n2, k - k2, constM,
                                constN, constK, level));
                }
                else
                {
                    CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol, matrixB, shiftBrow, shiftBcol, result,
                        shiftCrow, shiftCcol, m2, n2, k2, constM, constN, constK, level);
                    CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol, matrixB, shiftBrow, shiftBcol + n2,
                        result,
                        shiftCrow, shiftCcol + n2, m2, n - n2, k2, constM, constN, constK, level);

                    CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol + k2, matrixB, shiftBrow + k2, shiftBcol,
                        result, shiftCrow, shiftCcol, m2, n2, k - k2, constM, constN, constK, level);
                    CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol + k2, matrixB, shiftBrow + k2,
                        shiftBcol + n2,
                        result, shiftCrow, shiftCcol + n2, m2, n - n2, k - k2, constM, constN, constK, level);

                    CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol, matrixB, shiftBrow, shiftBcol,
                        result,
                        shiftCrow + m2, shiftCcol, m - m2, n2, k2, constM, constN, constK, level);
                    CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol, matrixB, shiftBrow, shiftBcol + n2,
                        result, shiftCrow + m2, shiftCcol + n2, m - m2, n - n2, k2, constM, constN, constK, level);

                    CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol + k2, matrixB, shiftBrow + k2,
                        shiftBcol,
                        result, shiftCrow + m2, shiftCcol, m - m2, n2, k - k2, constM, constN, constK, level);
                    CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol + k2, matrixB, shiftBrow + k2,
                        shiftBcol + n2, result, shiftCrow + m2, shiftCcol + n2, m - m2, n - n2, k - k2, constM, constN,
                        constK, level);
                }
            }
        }

        public unsafe class UnsafeProvider : ManagedLinearAlgebraProvider
        {
            public override void MatrixMultiply(
                double[] x, int rowsX, int columnsX, double[] y, int rowsY, int columnsY, double[] result)
            {
                if (rowsX + columnsY <= Control.ParallelizeOrder)
                {
                    fixed (double* resultPtr = &result[0])
                    fixed (double* xPtr = &x[0])
                    fixed (double* yPtr = &y[0])
                    {
                        double* a = xPtr;
                        double* c = resultPtr;
                        for (int i = 0; i < rowsX; ++i)
                        {
                            double* b = yPtr;
                            double* cj = c;
                            for (int j = 0; j < columnsY; ++j)
                            {
                                double sum = 0.0;
                                for (int k = 0; k < columnsX; ++k)
                                {
                                    sum += a[k*rowsX]*b[k];
                                }
                                *cj = sum;
                                cj += rowsX;
                                b += rowsY;
                            }
                            a++;
                            c++;
                        }
                    }

                    return;
                }

                double[] xdata;
                if (ReferenceEquals(x, result))
                {
                    xdata = (double[]) x.Clone();
                }
                else
                {
                    xdata = x;
                }

                double[] ydata;
                if (ReferenceEquals(y, result))
                {
                    ydata = (double[]) y.Clone();
                }
                else
                {
                    ydata = y;
                }

                Array.Clear(result, 0, result.Length);

                CacheObliviousMatrixMultiply(xdata, 0, 0, ydata, 0, 0, result, 0, 0, rowsX, columnsY, columnsX, rowsX,
                    columnsY, columnsX, 0);
            }

            public override void MatrixMultiplyWithUpdate(
                Transpose transposeA, Transpose transposeB, double alpha, double[] a, int rowsA, int columnsA,
                double[] b,
                int rowsB, int columnsB, double beta, double[] c)
            {
                if (transposeA == Transpose.DontTranspose && transposeB == Transpose.DontTranspose && alpha == 1.0 &&
                    beta == 0.0)
                {
                    MatrixMultiply(a, rowsA, columnsA, b, rowsB, columnsB, c);
                    return;
                }

                base.MatrixMultiplyWithUpdate(transposeA, transposeB, alpha, a, rowsA, columnsA, b, rowsB, columnsB,
                    beta, c);
            }

            static void CacheObliviousMatrixMultiply(
                double[] matrixA, int shiftArow, int shiftAcol, double[] matrixB, int shiftBrow, int shiftBcol,
                double[] result, int shiftCrow, int shiftCcol, int m, int n, int k, int constM, int constN, int constK,
                int level)
            {
                if (m + n <= Control.ParallelizeOrder)
                {
                    fixed (double* resultPtr = &result[0])
                    fixed (double* aPtr = &matrixA[0])
                    fixed (double* bPtr = &matrixB[0])
                    {
                        double* a = aPtr + shiftArow;
                        double* c = resultPtr + shiftCrow;
                        for (var m1 = 0; m1 < m; m1++)
                        {
                            for (var n1 = 0; n1 < n; ++n1)
                            {
                                double* b = bPtr + (n1 + shiftBcol)*constK + shiftBrow;
                                double sum = 0;
                                for (var k1 = 0; k1 < k; ++k1)
                                {
                                    sum += a[((k1 + shiftAcol)*constM)]*b[k1];
                                }

                                c[((n1 + shiftCcol)*constM)] += sum;
                            }
                            a++;
                            c++;
                        }
                    }

                    return;
                }

                // divide and conquer
                int m2 = m/2, n2 = n/2, k2 = k/2;

                level++;
                if (level <= 2)
                {
                    CommonParallel.Invoke(
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol, matrixB, shiftBrow, shiftBcol,
                                result, shiftCrow, shiftCcol, m2, n2, k2, constM, constN, constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol, matrixB, shiftBrow,
                                shiftBcol + n2,
                                result, shiftCrow, shiftCcol + n2, m2, n - n2, k2, constM, constN, constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol, matrixB, shiftBrow,
                                shiftBcol,
                                result, shiftCrow + m2, shiftCcol, m - m2, n2, k2, constM, constN, constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol, matrixB, shiftBrow,
                                shiftBcol + n2, result, shiftCrow + m2, shiftCcol + n2, m - m2, n - n2, k2, constM,
                                constN,
                                constK, level));

                    CommonParallel.Invoke(
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol + k2, matrixB, shiftBrow + k2,
                                shiftBcol, result, shiftCrow, shiftCcol, m2, n2, k - k2, constM, constN, constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol + k2, matrixB, shiftBrow + k2,
                                shiftBcol + n2, result, shiftCrow, shiftCcol + n2, m2, n - n2, k - k2, constM, constN,
                                constK, level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol + k2, matrixB,
                                shiftBrow + k2,
                                shiftBcol, result, shiftCrow + m2, shiftCcol, m - m2, n2, k - k2, constM, constN, constK,
                                level),
                        () =>
                            CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol + k2, matrixB,
                                shiftBrow + k2,
                                shiftBcol + n2, result, shiftCrow + m2, shiftCcol + n2, m - m2, n - n2, k - k2, constM,
                                constN, constK, level));
                }
                else
                {
                    CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol, matrixB, shiftBrow, shiftBcol, result,
                        shiftCrow, shiftCcol, m2, n2, k2, constM, constN, constK, level);
                    CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol, matrixB, shiftBrow, shiftBcol + n2,
                        result,
                        shiftCrow, shiftCcol + n2, m2, n - n2, k2, constM, constN, constK, level);

                    CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol + k2, matrixB, shiftBrow + k2, shiftBcol,
                        result, shiftCrow, shiftCcol, m2, n2, k - k2, constM, constN, constK, level);
                    CacheObliviousMatrixMultiply(matrixA, shiftArow, shiftAcol + k2, matrixB, shiftBrow + k2,
                        shiftBcol + n2,
                        result, shiftCrow, shiftCcol + n2, m2, n - n2, k - k2, constM, constN, constK, level);

                    CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol, matrixB, shiftBrow, shiftBcol,
                        result,
                        shiftCrow + m2, shiftCcol, m - m2, n2, k2, constM, constN, constK, level);
                    CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol, matrixB, shiftBrow, shiftBcol + n2,
                        result, shiftCrow + m2, shiftCcol + n2, m - m2, n - n2, k2, constM, constN, constK, level);

                    CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol + k2, matrixB, shiftBrow + k2,
                        shiftBcol,
                        result, shiftCrow + m2, shiftCcol, m - m2, n2, k - k2, constM, constN, constK, level);
                    CacheObliviousMatrixMultiply(matrixA, shiftArow + m2, shiftAcol + k2, matrixB, shiftBrow + k2,
                        shiftBcol + n2, result, shiftCrow + m2, shiftCcol + n2, m - m2, n - n2, k - k2, constM, constN,
                        constK, level);
                }
            }
        }

        public class ExperimentalProvider : ManagedLinearAlgebraProvider
        {
            public override void MatrixMultiply(
                double[] x, int rowsX, int columnsX, double[] y, int rowsY, int columnsY, double[] result)
            {
                MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 1.0, x, rowsX, columnsX, y,
                    rowsY,
                    columnsY, 0.0, result);
            }

            public override void MatrixMultiplyWithUpdate(
                Transpose transposeA, Transpose transposeB, double alpha, double[] a, int rowsA, int columnsA,
                double[] b,
                int rowsB, int columnsB, double beta, double[] c)
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
                    var swap = rowsA;
                    rowsA = columnsA;
                    columnsA = swap;
                }

                if (transposeB != Transpose.DontTranspose)
                {
                    var swap = rowsB;
                    rowsB = columnsB;
                    columnsB = swap;
                }

                if (columnsA != rowsB)
                {
                    throw new ArgumentOutOfRangeException($"columnsA ({columnsA}) != rowsB ({rowsB})");
                }

                if (rowsA * columnsA != a.Length)
                {
                    throw new ArgumentOutOfRangeException($"rowsA ({rowsA}) * columnsA ({columnsA}) != a.Length ({a.Length})");
                }

                if (rowsB * columnsB != b.Length)
                {
                    throw new ArgumentOutOfRangeException($"rowsB ({rowsB}) * columnsB ({columnsB}) != b.Length ({b.Length})");
                }

                if (rowsA * columnsB != c.Length)
                {
                    throw new ArgumentOutOfRangeException($"rowsA ({rowsA}) * columnsB ({columnsB}) != c.Length ({c.Length})");
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
                var columnDataB = new double[columnsB][];
                for (int i = 0; i < columnDataB.Length; i++)
                {
                    var column = new double[rowsB];
                    GetColumn(transposeB, i, rowsB, columnsB, b, column);
                    columnDataB[i] = column;
                }

                var shouldNotParallelize = rowsA + columnsB + columnsA < Control.ParallelizeOrder || Control.MaxDegreeOfParallelism < 2;
                if (shouldNotParallelize)
                {
                    var row = new double[columnsA];
                    for (int i = 0; i < rowsA; i++)
                    {
                        GetRow(transposeA, i, rowsA, columnsA, a, row);
                        for (int j = 0; j < columnsB; j++)
                        {
                            var col = columnDataB[j];
                            double sum = 0;
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
                        var row = new double[columnsA];
                        for (int i = u; i < v; i++)
                        {
                            GetRow(transposeA, i, rowsA, columnsA, a, row);
                            for (int j = 0; j < columnsB; j++)
                            {
                                var column = columnDataB[j];
                                double sum = 0;
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
            /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
            /// </summary>
            static void GetRow(Transpose transpose, int rowindx, int numRows, int numCols, double[] matrix, double[] row)
            {
                if (transpose == Transpose.DontTranspose)
                {
                    for (int i = 0; i < numCols; i++)
                    {
                        row[i] = matrix[(i * numRows) + rowindx];
                    }
                }
                else
                {
                    Array.Copy(matrix, rowindx * numCols, row, 0, numCols);
                }
            }

            /// <summary>
            /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
            /// </summary>
            static void GetColumn(Transpose transpose, int colindx, int numRows, int numCols, double[] matrix, double[] column)
            {
                if (transpose == Transpose.DontTranspose)
                {
                    Array.Copy(matrix, colindx * numRows, column, 0, numRows);
                }
                else
                {
                    for (int i = 0; i < numRows; i++)
                    {
                        column[i] = matrix[(i * numCols) + colindx];
                    }
                }
            }
        }
    }
}
