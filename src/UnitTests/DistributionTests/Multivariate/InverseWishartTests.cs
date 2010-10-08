// <copyright file="InverseWishartTests.cs" company="Math.NET">
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
    using MbUnit.Framework;
    using Distributions;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.LinearAlgebra.Double.Factorization;
    using MathNet.Numerics.UnitTests.LinearAlgebraTests.Double;

    [TestFixture]
    public class InverseWishartTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0.1, 2)]
        [Row(1.0, 2)]
        [Row(5.0, 2)]
        [Row(0.1, 5)]
        [Row(1.0, 5)]
        [Row(5.0, 5)]
        public void CanCreateInverseWishart(double nu, int order)
        {
            var matrix = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);

            var d = new InverseWishart(nu, matrix);

            Assert.AreEqual<double>(nu, d.Nu);
            for (int i = 0; i < d.S.RowCount; i++)
            {
                for (int j = 0; j < d.S.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], d.S[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0, 2)]
        [Row(0.1, 2)]
        [Row(1.0, 2)]
        [Row(5.0, 2)]
        [Row(0.0, 5)]
        [Row(0.1, 5)]
        [Row(1.0, 5)]
        [Row(5.0, 5)]
        public void FailSCreateInverseWishart(double nu, int order)
        {
            var matrix = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            matrix[0, 0] = 0.0;

            var d = new InverseWishart(nu, matrix);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0, 2)]
        [Row(Double.NaN, 2)]
        [Row(-1.0, 5)]
        [Row(Double.NaN, 5)]
        public void FailNuCreateInverseWishart(double nu, int order)
        {
            var matrix = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);

            var d = new InverseWishart(nu, matrix);
        }

        [Test]
        public void HasRandomSource()
        {
            var d = new InverseWishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            Assert.IsNotNull(d.RandomSource);
        }

        [Test]
        public void CanSetRandomSource()
        {
            var d = new InverseWishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            d.RandomSource = new Random();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FailSetRandomSourceWithNullReference()
        {
            var d = new InverseWishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            d.RandomSource = null;
        }

        [Test]
        public void ValidateToString()
        {
            var d = new InverseWishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            Assert.AreEqual<string>("InverseWishart(Nu = 1, Rows = 2, Columns = 2)", d.ToString());
        }

        [Test]
        [Row(1.0)]
        [Row(2.0)]
        [Row(5.0)]
        public void CanGetNu(double nu)
        {
            var d = new InverseWishart(nu, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            Assert.AreEqual<double>(nu, d.Nu);
        }

        [Test]
        [Row(1.0)]
        [Row(2.0)]
        [Row(5.0)]
        public void CanSetNu(double nu)
        {
            var d = new InverseWishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            d.Nu = nu;
        }

        [Test, MultipleAsserts]
        public void CanGetS()
        {
            int order = 2;
            var matrix = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            var d = new InverseWishart(1.0, matrix);

            for (int i = 0; i < order; i++)
            {
                for (int j = 0; j < order; j++)
                {
                    Assert.AreEqual<double>(matrix[i, j], d.S[i, j]);
                }
            }
        }

        [Test]
        public void CanSetS()
        {
            var d = new InverseWishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            d.S = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2);
        }

        [Test, MultipleAsserts]
        [Row(1.0, 2)]
        [Row(2.0, 2)]
        [Row(5.0, 2)]
        [Row(1.0, 5)]
        [Row(2.0, 5)]
        [Row(5.0, 5)]
        public void ValidateMean(double nu, int order)
        {
            var d = new InverseWishart(nu, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order));

            var mean = d.Mean;
            for (int i = 0; i < d.S.RowCount; i++)
            {
                for (int j = 0; j < d.S.ColumnCount; j++)
                {
                    Assert.AreEqual(d.S[i,j] * (1.0 / (nu - d.S.RowCount - 1.0)), mean[i, j]);
                }
            }
        }

        [Test, MultipleAsserts]
        [Row(1.0, 2)]
        [Row(2.0, 2)]
        [Row(5.0, 2)]
        [Row(1.0, 5)]
        [Row(2.0, 5)]
        [Row(5.0, 5)]
        public void ValidateMode(double nu, int order)
        {
            var d = new InverseWishart(nu, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order));

            var mode = d.Mode;
            for (var i = 0; i < d.S.RowCount; i++)
            {
                for (var j = 0; j < d.S.ColumnCount; j++)
                {
                    Assert.AreEqual(d.S[i, j] * (1.0 / (nu + d.S.RowCount + 1.0)), mode[i, j]);
                }
            }
        }

        [Test, MultipleAsserts]
        [Row(1.0, 2)]
        [Row(2.0, 2)]
        [Row(5.0, 2)]
        [Row(1.0, 5)]
        [Row(2.0, 5)]
        [Row(5.0, 5)]
        public void ValidateVariance(double nu, int order)
        {
            var d = new InverseWishart(nu, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order));

            var variance = d.Variance;
            for (var i = 0; i < d.S.RowCount; i++)
            {
                for (var j = 0; j < d.S.ColumnCount; j++)
                {
                    var num1 = (nu - d.S.RowCount + 1) * d.S[i, j] * d.S[i, j] + (nu - d.S.RowCount - 1) * d.S[i, i] * d.S[j, j];
                    var num2 = (nu - d.S.RowCount) * (nu - d.S.RowCount - 1) * (nu - d.S.RowCount - 1) * (nu - d.S.RowCount - 3);
                    Assert.AreEqual(num1 / num2, variance[i, j]);
                }
            }
        }
        [Test]
        [Row(1.0, 0.03228684517430723)]
        [Row(2.0, 0.018096748360719193)]
        [Row(5.0, 0.00043049126899076171)]
        public void ValidateDensity(double nu, double density)
        {
            int order = 1;
            var matrix = new DenseMatrix(order);
            matrix[0, 0] = 1;

            var X = new DenseMatrix(order);
            X[0, 0] = 5;

            var d = new InverseWishart(nu, matrix);
            AssertHelpers.AlmostEqual(density, d.Density(X), 16);
        }

        [Test]
        public void CanSample()
        {
            var d = new InverseWishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            var s = d.Sample();
        }

        [Test]
        public void CanSampleStatic()
        {
            var s = InverseWishart.Sample(new Random(), 1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var s = InverseWishart.Sample(new Random(), -1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
        }
    }
}