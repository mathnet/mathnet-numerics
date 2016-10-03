// <copyright file="MatrixNormalTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DistributionTests.Multivariate
{
    /// <summary>
    /// Matrix Normal tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
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
            var matrixM = Matrix<double>.Build.Random(n, p, 1);
            var matrixV = Matrix<double>.Build.RandomPositiveDefinite(n, 1);
            var matrixK = Matrix<double>.Build.RandomPositiveDefinite(p, 1);
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
            var matrixM = Matrix<double>.Build.Random(rowsOfM, columnsOfM, 1);
            var matrixV = Matrix<double>.Build.Random(rowsOfV, columnsOfV, 1);
            var matrixK = Matrix<double>.Build.Random(rowsOfK, columnsOfK, 1);

            Assert.That(() => new MatrixNormal(matrixM, matrixV, matrixK), Throws.ArgumentException);
        }

        /// <summary>
        /// Has random source.
        /// </summary>
        [Test]
        public void HasRandomSource()
        {
            const int N = 2;
            const int P = 3;
            var d = new MatrixNormal(Matrix<double>.Build.Random(N, P, 1), Matrix<double>.Build.RandomPositiveDefinite(N, 1), Matrix<double>.Build.RandomPositiveDefinite(P, 1));
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
            GC.KeepAlive(new MatrixNormal(Matrix<double>.Build.Random(N, P, 1), Matrix<double>.Build.RandomPositiveDefinite(N, 1), Matrix<double>.Build.RandomPositiveDefinite(P, 1))
            {
                RandomSource = new System.Random(0)
            });
        }

        [Test]
        public void HasRandomSourceEvenAfterSetToNull()
        {
            const int N = 2;
            const int P = 3;
            var d = new MatrixNormal(Matrix<double>.Build.Random(N, P, 1), Matrix<double>.Build.RandomPositiveDefinite(N, 1), Matrix<double>.Build.RandomPositiveDefinite(P, 1));
            Assert.DoesNotThrow(() => d.RandomSource = null);
            Assert.IsNotNull(d.RandomSource);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            const int N = 2;
            const int P = 5;
            var d = new MatrixNormal(Matrix<double>.Build.Random(N, P, 1), Matrix<double>.Build.RandomPositiveDefinite(N, 1), Matrix<double>.Build.RandomPositiveDefinite(P, 1));
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
            var matrixM = Matrix<double>.Build.Random(n, p, 1);
            var d = new MatrixNormal(matrixM, Matrix<double>.Build.RandomPositiveDefinite(n, 1), Matrix<double>.Build.RandomPositiveDefinite(p, 1));
            for (var i = 0; i < matrixM.RowCount; i++)
            {
                for (var j = 0; j < matrixM.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixM[i, j], d.Mean[i, j]);
                }
            }
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
            var matrixV = Matrix<double>.Build.RandomPositiveDefinite(n, 1);
            var d = new MatrixNormal(Matrix<double>.Build.Random(n, p, 1), matrixV, Matrix<double>.Build.RandomPositiveDefinite(p, 1));
            for (var i = 0; i < matrixV.RowCount; i++)
            {
                for (var j = 0; j < matrixV.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixV[i, j], d.RowCovariance[i, j]);
                }
            }
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
            var matrixK = Matrix<double>.Build.RandomPositiveDefinite(p, 1);
            var d = new MatrixNormal(Matrix<double>.Build.Random(n, p, 1), Matrix<double>.Build.RandomPositiveDefinite(n, 1), matrixK);
            for (var i = 0; i < matrixK.RowCount; i++)
            {
                for (var j = 0; j < matrixK.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixK[i, j], d.ColumnCovariance[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        [Test]
        public void ValidateDensity()
        {
            const int Rows = 2;
            const int Cols = 2;
            var m = Matrix<double>.Build.Dense(Rows, Cols);
            m[0, 0] = 0.156065579983862;
            m[0, 1] = -0.568039841576594;
            m[1, 0] = -0.806288628097313;
            m[1, 1] = -1.20004405005077;

            var v = Matrix<double>.Build.Dense(Rows, Rows);
            v[0, 0] = 0.674457817054746;
            v[0, 1] = 0.878930403442185;
            v[1, 0] = 0.878930403442185;
            v[1, 1] = 1.76277498368061;

            var k = Matrix<double>.Build.Dense(Cols, Cols);
            k[0, 0] = 0.674457817054746;
            k[0, 1] = 0.878930403442185;
            k[1, 0] = 0.878930403442185;
            k[1, 1] = 1.76277498368061;

            var d = new MatrixNormal(m, v, k);

            var x = Matrix<double>.Build.Dense(Rows, Cols);
            x[0, 0] = 2;
            x[0, 1] = 2;

            AssertHelpers.AlmostEqualRelative(0.00015682927366491211, d.Density(x), 16);
        }

        /// <summary>
        /// Validate density with non-square matrices.
        /// </summary>
        [Test]
        public void ValidateNonsquareDensity()
        {
            const int Rows = 2;
            const int Cols = 1;
            var m = Matrix<double>.Build.Dense(Rows, Cols);
            m[0, 0] = 0.156065579983862;
            m[1, 0] = -0.806288628097313;

            var v = Matrix<double>.Build.Dense(Rows, Rows);
            v[0, 0] = 0.674457817054746;
            v[0, 1] = 0.878930403442185;
            v[1, 0] = 0.878930403442185;
            v[1, 1] = 1.76277498368061;

            var k = Matrix<double>.Build.Dense(Cols, Cols);
            k[0, 0] = 0.674457817054746;

            var d = new MatrixNormal(m, v, k);

            var x = Matrix<double>.Build.Dense(Rows, Cols);
            x[0, 0] = 2;
            x[1, 0] = 1.5;

            AssertHelpers.AlmostEqualRelative(0.008613384131384546, d.Density(x), 12);
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
            var d = new MatrixNormal(Matrix<double>.Build.Random(n, p, 1), Matrix<double>.Build.RandomPositiveDefinite(n, 1), Matrix<double>.Build.RandomPositiveDefinite(p, 1));
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
            MatrixNormal.Sample(new System.Random(0), Matrix<double>.Build.Random(n, p, 1), Matrix<double>.Build.RandomPositiveDefinite(n, 1), Matrix<double>.Build.RandomPositiveDefinite(p, 1));
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
            Assert.That(() => MatrixNormal.Sample(new System.Random(0), Matrix<double>.Build.Random(rowsOfM, columnsOfM, 1), Matrix<double>.Build.Random(rowsOfV, columnsOfV, 1), Matrix<double>.Build.Random(rowsOfK, columnsOfK, 1)), Throws.ArgumentException);
        }
    }
}
