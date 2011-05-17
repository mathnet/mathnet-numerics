// <copyright file="MatrixNormalTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Multivariate
{
    using System;
    using Distributions;
    using LinearAlgebra.Double;
    using LinearAlgebraTests.Double;
    using NUnit.Framework;

    /// <summary>
    /// Matrix Normal tests.
    /// </summary>
    [TestFixture]
    public class MatrixNormalTests
    {
        /// <summary>
        /// Can create matrix normal.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanCreateMatrixNormal(int n, int p)
        {
            var matrixM = MatrixLoader.GenerateRandomDenseMatrix(n, p);
            var matrixV = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n);
            var matrixK = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p);
            var d = new MatrixNormal(matrixM, matrixV, matrixK);

            for (var i = 0; i < matrixM.RowCount; i++)
            {
                for (var j = 0; j < matrixM.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixM[i, j], d.Mean[i, j]);
                }
            }

            for (var i = 0; i < matrixV.RowCount; i++)
            {
                for (var j = 0; j < matrixV.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixV[i, j], d.RowCovariance[i, j]);
                }
            }

            for (var i = 0; i < matrixK.RowCount; i++)
            {
                for (var j = 0; j < matrixK.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixK[i, j], d.ColumnCovariance[i, j]);
                }
            }
        }

        /// <summary>
        /// Fail create <c>MatrixNormal</c> with bad parameters.
        /// </summary>
        /// <param name="rowsOfM">Mean matrix rows.</param>
        /// <param name="columnsOfM">Mean matrix columns.</param>
        /// <param name="rowsOfV">Covariance matrix rows.</param>
        /// <param name="columnsOfV">Covariance matrix columns.</param>
        /// <param name="rowsOfK">Covariance matrix rows (for columns)</param>
        /// <param name="columnsOfK">Covariance matrix columns (for columns)</param>
        [TestCase(2, 2, 3, 2, 2, 2)]
        [TestCase(2, 2, 2, 3, 2, 2)]
        [TestCase(2, 2, 2, 2, 3, 2)]
        [TestCase(2, 2, 2, 2, 2, 3)]
        [TestCase(5, 2, 6, 5, 2, 2)]
        [TestCase(5, 2, 5, 6, 2, 2)]
        [TestCase(5, 2, 5, 5, 3, 2)]
        [TestCase(5, 2, 5, 5, 2, 3)]
        public void FailCreateMatrixNormal(int rowsOfM, int columnsOfM, int rowsOfV, int columnsOfV, int rowsOfK, int columnsOfK)
        {
            var matrixM = MatrixLoader.GenerateRandomDenseMatrix(rowsOfM, columnsOfM);
            var matrixV = MatrixLoader.GenerateRandomDenseMatrix(rowsOfV, columnsOfV);
            var matrixK = MatrixLoader.GenerateRandomDenseMatrix(rowsOfK, columnsOfK);

            Assert.Throws<ArgumentOutOfRangeException>(() => new MatrixNormal(matrixM, matrixV, matrixK));
        }

        /// <summary>
        /// Has random source.
        /// </summary>
        [Test]
        public void HasRandomSource()
        {
            const int N = 2;
            const int P = 3;
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(N, P), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(N), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(P));
            Assert.IsNotNull(d.RandomSource);
        }

        /// <summary>
        /// Can set random source.
        /// </summary>
        [Test]
        public void CanSetRandomSource()
        {
            const int N = 2;
            const int P = 3;
            new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(N, P), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(N), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(P))
            {
                RandomSource = new Random()
            };
        }

        /// <summary>
        /// Fail set random source with <c>null</c> reference.
        /// </summary>
        [Test]
        public void FailSetRandomSourceWithNullReference()
        {
            const int N = 2;
            const int P = 3;
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(N, P), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(N), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(P));
            Assert.Throws<ArgumentNullException>(() => d.RandomSource = null);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            const int N = 2;
            const int P = 5;
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(N, P), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(N), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(P));
            Assert.AreEqual("MatrixNormal(Rows = 2, Columns = 5)", d.ToString());
        }

        /// <summary>
        /// Can get M.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanGetM(int n, int p)
        {
            var matrixM = MatrixLoader.GenerateRandomDenseMatrix(n, p);
            var d = new MatrixNormal(matrixM, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            for (var i = 0; i < matrixM.RowCount; i++)
            {
                for (var j = 0; j < matrixM.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixM[i, j], d.Mean[i, j]);
                }
            }
        }

        /// <summary>
        /// Can set M.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanSetM(int n, int p)
        {
            new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p))
            {
                Mean = MatrixLoader.GenerateRandomDenseMatrix(n, p)
            };
        }

        /// <summary>
        /// Can get V matrix.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanGetV(int n, int p)
        {
            var matrixV = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n);
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), matrixV, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            for (var i = 0; i < matrixV.RowCount; i++)
            {
                for (var j = 0; j < matrixV.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixV[i, j], d.RowCovariance[i, j]);
                }
            }
        }

        /// <summary>
        /// Can set V matrix.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanSetV(int n, int p)
        {
            new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p))
            {
                RowCovariance = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n)
            };
        }

        /// <summary>
        /// Can get K matrix.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanGetK(int n, int p)
        {
            var matrixK = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p);
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), matrixK);
            for (var i = 0; i < matrixK.RowCount; i++)
            {
                for (var j = 0; j < matrixK.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixK[i, j], d.ColumnCovariance[i, j]);
                }
            }
        }

        /// <summary>
        /// Can set K matrix.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanSetK(int n, int p)
        {
            new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p))
            {
                ColumnCovariance = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p)
            };
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        [Test]
        public void ValidateDensity()
        {
            const int Rows = 2;
            const int Cols = 2;
            var m = new DenseMatrix(Rows, Cols);
            m[0, 0] = 0.156065579983862;
            m[0, 1] = -0.568039841576594;
            m[1, 0] = -0.806288628097313;
            m[1, 1] = -1.20004405005077;

            var v = new DenseMatrix(Rows, Rows);
            v[0, 0] = 0.674457817054746;
            v[0, 1] = 0.878930403442185;
            v[1, 0] = 0.878930403442185;
            v[1, 1] = 1.76277498368061;

            var k = new DenseMatrix(Cols, Cols);
            k[0, 0] = 0.674457817054746;
            k[0, 1] = 0.878930403442185;
            k[1, 0] = 0.878930403442185;
            k[1, 1] = 1.76277498368061;
            var d = new MatrixNormal(m, v, k);

            var x = new DenseMatrix(Rows, Cols);
            x[0, 0] = 2;
            x[0, 1] = 2;

            AssertHelpers.AlmostEqual(0.00015682927366491211, d.Density(x), 16);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanSample(int n, int p)
        {
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            d.Sample();
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        /// <param name="n">Matrix rows count.</param>
        /// <param name="p">Matrix columns count.</param>
        [TestCase(1, 1)]
        [TestCase(3, 3)]
        [TestCase(10, 10)]
        public void CanSampleStatic(int n, int p)
        {
            MatrixNormal.Sample(new Random(), MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        /// <param name="rowsOfM">Mean matrix rows.</param>
        /// <param name="columnsOfM">Mean matrix columns.</param>
        /// <param name="rowsOfV">Covariance matrix rows.</param>
        /// <param name="columnsOfV">Covariance matrix columns.</param>
        /// <param name="rowsOfK">Covariance matrix rows (for columns)</param>
        /// <param name="columnsOfK">Covariance matrix columns (for columns)</param>
        [TestCase(2, 2, 3, 2, 2, 2)]
        [TestCase(2, 2, 2, 3, 2, 2)]
        [TestCase(2, 2, 2, 2, 3, 2)]
        [TestCase(2, 2, 2, 2, 2, 3)]
        [TestCase(5, 2, 6, 5, 2, 2)]
        [TestCase(5, 2, 5, 6, 2, 2)]
        [TestCase(5, 2, 5, 5, 3, 2)]
        [TestCase(5, 2, 5, 5, 2, 3)]
        public void FailSampleStatic(int rowsOfM, int columnsOfM, int rowsOfV, int columnsOfV, int rowsOfK, int columnsOfK)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MatrixNormal.Sample(new Random(), MatrixLoader.GenerateRandomDenseMatrix(rowsOfM, columnsOfM), MatrixLoader.GenerateRandomDenseMatrix(rowsOfV, columnsOfV), MatrixLoader.GenerateRandomDenseMatrix(rowsOfK, columnsOfK)));
        }
    }
}
