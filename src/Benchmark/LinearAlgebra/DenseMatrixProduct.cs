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
        readonly Dictionary<int, Matrix<double>> _dataMathNet = new Dictionary<int, Matrix<double>>();
        private ILinearAlgebraProvider _managed;
        private ILinearAlgebraProvider _mkl;
        //private ILinearAlgebraProvider _safeProvider;
        private ILinearAlgebraProvider _unsafeProvider;
        private ILinearAlgebraProvider _experimentalProvider;

        [Params(8, 64, 128)]
        public int N { get; set; }

        public DenseMatrixProduct()
        {
            foreach (var n in new[] {8, 64, 128})
            {
                _dataMathNet[n] = Matrix<double>.Build.Random(n, n);
            }

            Control.NativeProviderPath = @"..\..\..\..\out\MKL\Windows\";
            _managed = new ManagedLinearAlgebraProvider();
            _mkl = new MklLinearAlgebraProvider();
            //_safeProvider = new SafeProvider();
            _unsafeProvider = new UnsafeProvider();
            _experimentalProvider = new ExperimentalProvider();

            _managed.InitializeVerify();
            _mkl.InitializeVerify();
            //_safeProvider.InitializeVerify();
            _unsafeProvider.InitializeVerify();
            _experimentalProvider.InitializeVerify();

            N = 8;
            var resultMkl = MathNet().ToRowArrays();
            var resultManaged = MathNetManaged().ToRowArrays();
            var resultUnsafe = MathNetExtraUnsafe().ToRowArrays();
            var resultExperimental = MathNetExtraExperimental().ToRowArrays();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!resultMkl[i][j].AlmostEqual(resultManaged[i][j], 1e-14))
                    {
                        throw new Exception(string.Format("Managed [{0}][{1}] {2} != {3}", i, j, resultManaged[i][j], resultMkl[i][j]));
                    }
                    if (!resultMkl[i][j].AlmostEqual(resultUnsafe[i][j], 1e-14))
                    {
                        throw new Exception(string.Format("Unsafe [{0}][{1}] {2} != {3}", i, j, resultUnsafe[i][j], resultMkl[i][j]));
                    }
                    if (!resultMkl[i][j].AlmostEqual(resultExperimental[i][j], 1e-14))
                    {
                        throw new Exception(string.Format("Experimental [{0}][{1}] {2} != {3}", i, j, resultExperimental[i][j], resultMkl[i][j]));
                    }
                }
            }
            Console.WriteLine("Results OK");
        }

        [Setup]
        public void Setup()
        {
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNet()
        {
            Control.LinearAlgebraProvider = _mkl;
            return _dataMathNet[N]*_dataMathNet[N];
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetManaged()
        {
            Control.LinearAlgebraProvider = _managed;
            return _dataMathNet[N]*_dataMathNet[N];
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        //public Matrix<double> MathNetExtraSafe()
        //{
        //    Control.LinearAlgebraProvider = _safeProvider;
        //    return _dataMathNet[N] * _dataMathNet[N];
        //}

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetExtraUnsafe()
        {
            Control.LinearAlgebraProvider = _unsafeProvider;
            return _dataMathNet[N] * _dataMathNet[N];
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetExtraExperimental()
        {
            Control.LinearAlgebraProvider = _experimentalProvider;
            return _dataMathNet[N] * _dataMathNet[N];
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

                if (transposeA != Transpose.DontTranspose)
                {
                    Swap(ref rowsA, ref columnsA);
                }

                if (transposeB != Transpose.DontTranspose)
                {
                    Swap(ref rowsB, ref columnsB);
                }

                if (columnsA != rowsB)
                {
                    throw new ArgumentOutOfRangeException(String.Format("columnsA ({0}) != rowsB ({1})", columnsA, rowsB));
                }

                if (rowsA*columnsA != a.Length)
                {
                    throw new ArgumentOutOfRangeException(String.Format(
                        "rowsA ({0}) * columnsA ({1}) != a.Length ({2})",
                        rowsA, columnsA, a.Length));
                }

                if (rowsB*columnsB != b.Length)
                {
                    throw new ArgumentOutOfRangeException(String.Format(
                        "rowsB ({0}) * columnsB ({1}) != b.Length ({2})",
                        rowsB, columnsB, b.Length));
                }

                if (rowsA*columnsB != c.Length)
                {
                    throw new ArgumentOutOfRangeException(String.Format(
                        "rowsA ({0}) * columnsB ({1}) != c.Length ({2})",
                        rowsA, columnsB, c.Length));
                }

                // handle the degenerate cases
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
                    columnDataB[i] = GetColumn(transposeB, i, rowsB, columnsB, b);
                }

                var shouldNotParallelize = rowsA + columnsB + columnsA < Control.ParallelizeOrder ||
                                           Control.MaxDegreeOfParallelism < 2;
                if (shouldNotParallelize)
                {
                    for (int i = 0; i < rowsA; i++)
                    {
                        var row = GetRow(transposeA, i, rowsA, columnsA, a);
                        for (int j = 0; j < columnsB; j++)
                        {
                            var col = columnDataB[j];
                            double sum = 0;
                            for (int ii = 0; ii < row.Length; ii++)
                            {
                                sum += row[ii]*col[ii];
                            }

                            c[j*rowsA + i] += alpha*sum;
                        }
                    }
                }
                else
                {
                    CommonParallel.For(0, rowsA, 1, (u, v) =>
                    {
                        for (int i = u; i < v; i++)
                        {
                            // for each row in a
                            var row = GetRow(transposeA, i, rowsA, columnsA, a);
                            for (int j = 0; j < columnsB; j++)
                            {
                                var column = columnDataB[j];
                                double sum = 0;
                                for (int ii = 0; ii < row.Length; ii++)
                                {
                                    sum += row[ii]*column[ii];
                                }

                                c[j*rowsA + i] += alpha*sum;
                            }
                        }
                    });
                }
            }

            static void Swap(ref int first, ref int second)
            {
                var prior = first;
                first = second;
                second = prior;
            }

            /// <summary>
            /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
            /// </summary>
            static double[] GetRow(Transpose transpose, int rowindx, int numRows, int numCols, double[] matrix)
            {
                var ret = new double[numCols];
                if (transpose == Transpose.DontTranspose)
                {
                    for (int i = 0; i < numCols; i++)
                    {
                        ret[i] = matrix[(i*numRows) + rowindx];
                    }
                }
                else
                {
                    Array.Copy(matrix, rowindx*numCols, ret, 0, numCols);
                }

                return ret;
            }

            /// <summary>
            /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
            /// </summary>
            static double[] GetColumn(Transpose transpose, int colindx, int numRows, int numCols, double[] matrix)
            {
                var ret = new double[numRows];
                if (transpose == Transpose.DontTranspose)
                {
                    Array.Copy(matrix, colindx*numRows, ret, 0, numRows);
                }
                else
                {
                    for (int i = 0; i < numRows; i++)
                    {
                        ret[i] = matrix[(i*numCols) + colindx];
                    }
                }

                return ret;
            }
        }
    }
}
