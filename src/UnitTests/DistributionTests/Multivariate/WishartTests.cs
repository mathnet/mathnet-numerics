// <copyright file="WishartTests.cs" company="Math.NET">
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
    /// <c>Wishart</c> distribution tests.
    /// </summary>
    [TestFixture]
    public class WishartTests
    {
        /// <summary>
        /// Set-up parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create wishart.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void CanCreateWishart(double nu, int order)
        {
            var matrix = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);

            var d = new Wishart(nu, matrix);

            Assert.AreEqual(nu, d.Nu);
            for (var i = 0; i < d.S.RowCount; i++)
            {
                for (var j = 0; j < d.S.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], d.S[i, j]);
                }
            }
        }

        /// <summary>
        /// Fail create Wishart with bad parameters.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.0, 2)]
        [TestCase(0.1, 5)]
        [TestCase(1.0, 2)]
        [TestCase(5.0, 5)]
        public void FailSCreateWishart(double nu, int order)
        {
            var matrix = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            matrix[0, 0] = 0.0;

            Assert.Throws<ArgumentOutOfRangeException>(() => new Wishart(nu, matrix));
        }

        /// <summary>
        /// Fail create Wishart with bad parameters.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(-1.0, 2)]
        [TestCase(Double.NaN, 5)]
        public void FailNuCreateWishart(double nu, int order)
        {
            var matrix = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            Assert.Throws<ArgumentOutOfRangeException>(() => new InverseWishart(nu, matrix));
        }

        /// <summary>
        /// Has random source.
        /// </summary>
        [Test]
        public void HasRandomSource()
        {
            var d = new Wishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            Assert.IsNotNull(d.RandomSource);
        }

        /// <summary>
        /// Can set random source.
        /// </summary>
        [Test]
        public void CanSetRandomSource()
        {
            new Wishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2))
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
            var d = new Wishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            Assert.Throws<ArgumentNullException>(() => d.RandomSource = null);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new Wishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            Assert.AreEqual("Wishart(Nu = 1, Rows = 2, Columns = 2)", d.ToString());
        }

        /// <summary>
        /// Can get Nu.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(5.0)]
        public void CanGetNu(double nu)
        {
            var d = new Wishart(nu, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            Assert.AreEqual(nu, d.Nu);
        }

        /// <summary>
        /// Can set Nu.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(5.0)]
        public void CanSetNu(double nu)
        {
            new Wishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2))
            {
                Nu = nu
            };
        }

        /// <summary>
        /// Can get scale matrix.
        /// </summary>
        [Test]
        public void CanGetS()
        {
            const int Order = 2;
            var matrix = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(Order);
            var d = new Wishart(1.0, matrix);

            for (var i = 0; i < Order; i++)
            {
                for (var j = 0; j < Order; j++)
                {
                    Assert.AreEqual(matrix[i, j], d.S[i, j]);
                }
            }
        }

        /// <summary>
        /// Can set scale matrix.
        /// </summary>
        [Test]
        public void CanSetS()
        {
            new Wishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2))
            {
                S = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2)
            };
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void ValidateMean(double nu, int order)
        {
            var d = new Wishart(nu, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order));

            var mean = d.Mean;
            for (var i = 0; i < d.S.RowCount; i++)
            {
                for (var j = 0; j < d.S.ColumnCount; j++)
                {
                    Assert.AreEqual(nu * d.S[i, j], mean[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void ValidateMode(double nu, int order)
        {
            var d = new Wishart(nu, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order));

            var mode = d.Mode;
            for (var i = 0; i < d.S.RowCount; i++)
            {
                for (var j = 0; j < d.S.ColumnCount; j++)
                {
                    Assert.AreEqual((nu - d.S.RowCount - 1.0) * d.S[i, j], mode[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void ValidateVariance(double nu, int order)
        {
            var d = new Wishart(nu, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order));

            var variance = d.Variance;
            for (var i = 0; i < d.S.RowCount; i++)
            {
                for (var j = 0; j < d.S.ColumnCount; j++)
                {
                    Assert.AreEqual(nu * ((d.S[i, j] * d.S[i, j]) + (d.S[i, i] * d.S[j, j])), variance[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="density">Expected value.</param>
        [TestCase(1.0, 0.014644982561926487)]
        [TestCase(2.0, 0.041042499311949421)]
        [TestCase(5.0, 0.12204152134938706)]
        public void ValidateDensity(double nu, double density)
        {
            const int Order = 1;
            var matrix = new DenseMatrix(Order);
            matrix[0, 0] = 1;

            var x = new DenseMatrix(Order);
            x[0, 0] = 5;

            var d = new Wishart(nu, matrix);
            AssertHelpers.AlmostEqual(density, d.Density(x), 16);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new Wishart(1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
            d.Sample();
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Wishart.Sample(new Random(), 1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2));
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Wishart.Sample(new Random(), -1.0, MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(2)));
        }
    }
}
