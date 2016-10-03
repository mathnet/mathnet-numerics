// <copyright file="WishartTests.cs" company="Math.NET">
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
    /// <c>Wishart</c> distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
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
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void CanCreateWishart(double nu, int order)
        {
            var matrix = Matrix<double>.Build.RandomPositiveDefinite(order, 1);

            var d = new Wishart(nu, matrix);

            Assert.AreEqual(nu, d.DegreesOfFreedom);
            for (var i = 0; i < d.Scale.RowCount; i++)
            {
                for (var j = 0; j < d.Scale.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], d.Scale[i, j]);
                }
            }
        }

        /// <summary>
        /// Fail create Wishart with bad parameters.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.0, 2)]
        [TestCase(0.1, 5)]
        [TestCase(1.0, 2)]
        [TestCase(5.0, 5)]
        public void FailSCreateWishart(double nu, int order)
        {
            var matrix = Matrix<double>.Build.RandomPositiveDefinite(order, 1);
            matrix[0, 0] = 0.0;

            Assert.That(() => new Wishart(nu, matrix), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail create Wishart with bad parameters.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(-1.0, 2)]
        [TestCase(Double.NaN, 5)]
        public void FailNuCreateWishart(double nu, int order)
        {
            var matrix = Matrix<double>.Build.RandomPositiveDefinite(order, 1);
            Assert.That(() => new InverseWishart(nu, matrix), Throws.ArgumentException);
        }

        /// <summary>
        /// Has random source.
        /// </summary>
        [Test]
        public void HasRandomSource()
        {
            var d = new Wishart(1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
            Assert.IsNotNull(d.RandomSource);
        }

        /// <summary>
        /// Can set random source.
        /// </summary>
        [Test]
        public void CanSetRandomSource()
        {
            GC.KeepAlive(new Wishart(1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1))
            {
                RandomSource = new System.Random(0)
            });
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new Wishart(1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
            Assert.AreEqual("Wishart(DegreesOfFreedom = 1, Rows = 2, Columns = 2)", d.ToString());
        }

        /// <summary>
        /// Can get DegreesOfFreedom.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(5.0)]
        public void CanGetNu(double nu)
        {
            var d = new Wishart(nu, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
            Assert.AreEqual(nu, d.DegreesOfFreedom);
        }

        /// <summary>
        /// Can get scale matrix.
        /// </summary>
        [Test]
        public void CanGetS()
        {
            const int Order = 2;
            var matrix = Matrix<double>.Build.RandomPositiveDefinite(Order, 1);
            var d = new Wishart(1.0, matrix);

            for (var i = 0; i < Order; i++)
            {
                for (var j = 0; j < Order; j++)
                {
                    Assert.AreEqual(matrix[i, j], d.Scale[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void ValidateMean(double nu, int order)
        {
            var d = new Wishart(nu, Matrix<double>.Build.RandomPositiveDefinite(order, 1));

            var mean = d.Mean;
            for (var i = 0; i < d.Scale.RowCount; i++)
            {
                for (var j = 0; j < d.Scale.ColumnCount; j++)
                {
                    Assert.AreEqual(nu * d.Scale[i, j], mean[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void ValidateMode(double nu, int order)
        {
            var d = new Wishart(nu, Matrix<double>.Build.RandomPositiveDefinite(order, 1));

            var mode = d.Mode;
            for (var i = 0; i < d.Scale.RowCount; i++)
            {
                for (var j = 0; j < d.Scale.ColumnCount; j++)
                {
                    Assert.AreEqual((nu - d.Scale.RowCount - 1.0) * d.Scale[i, j], mode[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void ValidateVariance(double nu, int order)
        {
            var d = new Wishart(nu, Matrix<double>.Build.RandomPositiveDefinite(order, 1));

            var variance = d.Variance;
            for (var i = 0; i < d.Scale.RowCount; i++)
            {
                for (var j = 0; j < d.Scale.ColumnCount; j++)
                {
                    Assert.AreEqual(nu * ((d.Scale[i, j] * d.Scale[i, j]) + (d.Scale[i, i] * d.Scale[j, j])), variance[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="density">Expected value.</param>
        [TestCase(1.0, 0.014644982561926487)]
        [TestCase(2.0, 0.041042499311949421)]
        [TestCase(5.0, 0.12204152134938706)]
        public void ValidateDensity(double nu, double density)
        {
            var matrix = Matrix<double>.Build.Dense(1, 1, 1.0);
            var x = Matrix<double>.Build.Dense(1, 1, 5.0);

            var d = new Wishart(nu, matrix);
            AssertHelpers.AlmostEqualRelative(density, d.Density(x), 16);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new Wishart(1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
            d.Sample();
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Wishart.Sample(new System.Random(0), 1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => Wishart.Sample(new System.Random(0), -1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1)), Throws.ArgumentException);
        }
    }
}
