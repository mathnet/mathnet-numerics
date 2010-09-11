// <copyright file="MatrixNormalTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Multivariate
{
    using System;
    using System.Linq;
    using LinearAlgebra.Generic;
    using MbUnit.Framework;
    using Random;
    using Distributions;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.LinearAlgebra.Double.Factorization;
    using MathNet.Numerics.UnitTests.LinearAlgebraTests.Double;

    [TestFixture]
    public class MatrixNormalTests
    {
        [Test, MultipleAsserts]
        [Row(1, 1)]
        [Row(3, 1)]
        [Row(10, 1)]
        [Row(1, 3)]
        [Row(3, 3)]
        [Row(10, 3)]
        [Row(1, 10)]
        [Row(3, 10)]
        [Row(10, 10)]
        public void CanCreateMatrixNormal(int n ,int p)
        {
            var M = MatrixLoader.GenerateRandomDenseMatrix(n, p);
            var V = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n);
            var K = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p);
            MatrixNormal d = new MatrixNormal(M, V, K);

            for (int i = 0; i < M.RowCount; i++)
            {
                for (int j = 0; j < M.ColumnCount; j++)
                {
                    Assert.AreEqual<double>(M[i, j], d.Mean[i, j]);
                }
            }

            for (int i = 0; i < V.RowCount; i++)
            {
                for (int j = 0; j < V.ColumnCount; j++)
                {
                    Assert.AreEqual<double>(V[i, j], d.RowCovariance[i, j]);
                }
            }

            for (int i = 0; i < K.RowCount; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    Assert.AreEqual<double>(K[i, j], d.ColumnCovariance[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        //  (n, p, n, n, p, p)
        [Row(2, 2, 3, 2, 2, 2)]
        [Row(2, 2, 2, 3, 2, 2)]
        [Row(2, 2, 2, 2, 3, 2)]
        [Row(2, 2, 2, 2, 2, 3)]
        [Row(5, 2, 6, 5, 2, 2)]
        [Row(5, 2, 5, 6, 2, 2)]
        [Row(5, 2, 5, 5, 3, 2)]
        [Row(5, 2, 5, 5, 2, 3)]
        public void FailCreateMatrixNormal(int mRows, int mCols, int vRows, int vCols, int kRows, int kCols)
        {
            var M = MatrixLoader.GenerateRandomDenseMatrix(mRows, mCols);
            var V = MatrixLoader.GenerateRandomDenseMatrix(vRows, vCols);
            var K = MatrixLoader.GenerateRandomDenseMatrix(kRows, kCols);
            
            MatrixNormal d = new MatrixNormal(M, V, K);
        }

        [Test]
        public void HasRandomSource()
        {
            int n = 2;
            int p = 3;
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            Assert.IsNotNull(d.RandomSource);
        }

        [Test]
        public void CanSetRandomSource()
        {
            int n = 2;
            int p = 3;
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            d.RandomSource = new Random();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FailSetRandomSourceWithNullReference()
        {
            int n = 2;
            int p = 3;
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            d.RandomSource = null;
        }

        [Test]
        public void ValidateToString()
        {
            int n = 2;
            int p = 5;
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            Assert.AreEqual<string>("MatrixNormal(Rows = 2, Columns = 5)", d.ToString());
        }

        [Test, MultipleAsserts]
        [Row(1, 1)]
        [Row(3, 1)]
        [Row(10, 1)]
        [Row(1, 3)]
        [Row(3, 3)]
        [Row(10, 3)]
        [Row(1, 10)]
        [Row(3, 10)]
        [Row(10, 10)]
        public void CanGetM(int n, int p)
        {
            var M = MatrixLoader.GenerateRandomDenseMatrix(n, p);
            var d = new MatrixNormal(M, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            for (int i = 0; i < M.RowCount; i++)
            {
                for (int j = 0; j < M.ColumnCount; j++)
                {
                    Assert.AreEqual<double>(M[i, j], d.Mean[i, j]);
                }
            }
        }

        [Test]
        [Row(1, 1)]
        [Row(3, 1)]
        [Row(10, 1)]
        [Row(1, 3)]
        [Row(3, 3)]
        [Row(10, 3)]
        [Row(1, 10)]
        [Row(3, 10)]
        [Row(10, 10)]
        public void CanSetM(int n, int p)
        {
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            d.Mean = MatrixLoader.GenerateRandomDenseMatrix(n, p);
        }

        [Test, MultipleAsserts]
        [Row(1, 1)]
        [Row(3, 1)]
        [Row(10, 1)]
        [Row(1, 3)]
        [Row(3, 3)]
        [Row(10, 3)]
        [Row(1, 10)]
        [Row(3, 10)]
        [Row(10, 10)]
        public void CanGetV(int n, int p)
        {
            var V = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n);
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), V, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            for (int i = 0; i < V.RowCount; i++)
            {
                for (int j = 0; j < V.ColumnCount; j++)
                {
                    Assert.AreEqual<double>(V[i, j], d.RowCovariance[i, j]);
                }
            }
        }

        [Test]
        [Row(1, 1)]
        [Row(3, 1)]
        [Row(10, 1)]
        [Row(1, 3)]
        [Row(3, 3)]
        [Row(10, 3)]
        [Row(1, 10)]
        [Row(3, 10)]
        [Row(10, 10)]
        public void CanSetV(int n, int p)
        {
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            d.RowCovariance = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n);
        }

        [Test, MultipleAsserts]
        [Row(1, 1)]
        [Row(3, 1)]
        [Row(10, 1)]
        [Row(1, 3)]
        [Row(3, 3)]
        [Row(10, 3)]
        [Row(1, 10)]
        [Row(3, 10)]
        [Row(10, 10)]
        public void CanGetK(int n, int p)
        {
            var K = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p);
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), K);
            for (int i = 0; i < K.RowCount; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    Assert.AreEqual<double>(K[i, j], d.ColumnCovariance[i, j]);
                }
            }
        }

        [Test]
        [Row(1, 1)]
        [Row(3, 1)]
        [Row(10, 1)]
        [Row(1, 3)]
        [Row(3, 3)]
        [Row(10, 3)]
        [Row(1, 10)]
        [Row(3, 10)]
        [Row(10, 10)]
        public void CanSetK(int n, int p)
        {
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            d.ColumnCovariance = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p);
        }

        [Test]
        public void ValidateDensity()
        {
            int rows = 2;
            int cols = 2;
            var M = new DenseMatrix(rows, cols);
            M[0, 0] = 0.156065579983862;
            M[0, 1] = -0.568039841576594;
            M[1, 0] = -0.806288628097313;
            M[1, 1] = -1.20004405005077;

            var V = new DenseMatrix(rows, rows);
            V[0, 0] = 0.674457817054746;
            V[0, 1] = 0.878930403442185;
            V[1, 0] = 0.878930403442185;
            V[1, 1] = 1.76277498368061;

            var K = new DenseMatrix(cols, cols);
            K[0, 0] = 0.674457817054746;
            K[0, 1] = 0.878930403442185;
            K[1, 0] = 0.878930403442185;
            K[1, 1] = 1.76277498368061;
            MatrixNormal d = new MatrixNormal(M, V, K);

            var X = new DenseMatrix(rows, cols);
            X[0, 0] = 2;
            X[0, 1] = 2;

            AssertHelpers.AlmostEqual(0.00015682927366491211, d.Density(X), 16);
        }

        [Test]
        [Row(1, 1)]
        [Row(3, 3)]
        [Row(10, 10)]
        public void CanSample(int n, int p)
        {
            var d = new MatrixNormal(MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
            var s = d.Sample();
        }

        [Test]
        [Row(1, 1)]
        [Row(3, 3)]
        [Row(10, 10)]
        public void CanSampleStatic(int n, int p)
        {
            var s = MatrixNormal.Sample(new Random(), MatrixLoader.GenerateRandomDenseMatrix(n, p), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(n), MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(p));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(2, 2, 3, 2, 2, 2)]
        [Row(2, 2, 2, 3, 2, 2)]
        [Row(2, 2, 2, 2, 3, 2)]
        [Row(2, 2, 2, 2, 2, 3)]
        //[Row(5, 2, 6, 5, 2, 2)]
        //[Row(5, 2, 5, 6, 2, 2)]
        //[Row(5, 2, 5, 5, 3, 2)]
        //[Row(5, 2, 5, 5, 2, 3)]
        public void FailSampleStatic(int mRows, int mCols, int vRows, int vCols, int kRows, int kCols)
        {
            var s = MatrixNormal.Sample(new Random(), MatrixLoader.GenerateRandomDenseMatrix(mRows, mCols), MatrixLoader.GenerateRandomDenseMatrix(vRows, vCols), MatrixLoader.GenerateRandomDenseMatrix(kRows, kCols));
        }
    }
}