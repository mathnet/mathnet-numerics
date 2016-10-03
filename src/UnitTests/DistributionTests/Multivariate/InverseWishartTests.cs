// <copyright file="InverseWishartTests.cs" company="Math.NET">
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
    /// Inverse Wishart tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class InverseWishartTests
    {
        /// <summary>
        /// Set-up test parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create inverse Wishart.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void CanCreateInverseWishart(double nu, int order)
        {
            var matrix = Matrix<double>.Build.RandomPositiveDefinite(order, 1);
            var d = new InverseWishart(nu, matrix);

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
        /// Fail create inverse Wishart with bad parameters.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(0.1, 2)]
        [TestCase(1.0, 5)]
        [TestCase(5.0, 5)]
        public void FailSCreateInverseWishart(double nu, int order)
        {
            var matrix = Matrix<double>.Build.RandomPositiveDefinite(order, 1);
            matrix[0, 0] = 0.0;

            Assert.That(() => new InverseWishart(nu, matrix), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail create inverse Wishart with bad parameters.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="order">Scale matrix order.</param>
        [TestCase(-1.0, 2)]
        [TestCase(Double.NaN, 5)]
        public void FailNuCreateInverseWishart(double nu, int order)
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
            var d = new InverseWishart(1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
            Assert.IsNotNull(d.RandomSource);
        }

        /// <summary>
        /// Can set random source.
        /// </summary>
        [Test]
        public void CanSetRandomSource()
        {
            GC.KeepAlive(new InverseWishart(1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1))
            {
                RandomSource = new System.Random(0)
            });
        }

        [Test]
        public void HasRandomSourceEvenAfterSetToNull()
        {
            var d = new InverseWishart(1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
            Assert.DoesNotThrow(() => d.RandomSource = null);
            Assert.IsNotNull(d.RandomSource);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new InverseWishart(1d, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
            Assert.AreEqual("InverseWishart(ν = 1, Rows = 2, Columns = 2)", d.ToString());
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
            var d = new InverseWishart(nu, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
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
            var d = new InverseWishart(1.0, matrix);

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
            var d = new InverseWishart(nu, Matrix<double>.Build.RandomPositiveDefinite(order, 1));

            var mean = d.Mean;
            for (var i = 0; i < d.Scale.RowCount; i++)
            {
                for (var j = 0; j < d.Scale.ColumnCount; j++)
                {
                    Assert.AreEqual(d.Scale[i, j] * (1.0 / (nu - d.Scale.RowCount - 1.0)), mean[i, j]);
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
            var d = new InverseWishart(nu, Matrix<double>.Build.RandomPositiveDefinite(order, 1));

            var mode = d.Mode;
            for (var i = 0; i < d.Scale.RowCount; i++)
            {
                for (var j = 0; j < d.Scale.ColumnCount; j++)
                {
                    Assert.AreEqual(d.Scale[i, j] * (1.0 / (nu + d.Scale.RowCount + 1.0)), mode[i, j]);
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
            var d = new InverseWishart(nu, Matrix<double>.Build.RandomPositiveDefinite(order, 1));

            var variance = d.Variance;
            for (var i = 0; i < d.Scale.RowCount; i++)
            {
                for (var j = 0; j < d.Scale.ColumnCount; j++)
                {
                    var num1 = ((nu - d.Scale.RowCount + 1) * d.Scale[i, j] * d.Scale[i, j]) + ((nu - d.Scale.RowCount - 1) * d.Scale[i, i] * d.Scale[j, j]);
                    var num2 = (nu - d.Scale.RowCount) * (nu - d.Scale.RowCount - 1) * (nu - d.Scale.RowCount - 1) * (nu - d.Scale.RowCount - 3);
                    Assert.AreEqual(num1 / num2, variance[i, j]);
                }
            }
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="density">Expected value.</param>
        [TestCase(1.0, 0.03228684517430723)]
        [TestCase(2.0, 0.018096748360719193)]
        [TestCase(5.0, 0.00043049126899076171)]
        public void ValidateDensity(double nu, double density)
        {
            var matrix = Matrix<double>.Build.Dense(1, 1, 1.0);
            var x = Matrix<double>.Build.Dense(1, 1, 5.0);

            var d = new InverseWishart(nu, matrix);
            AssertHelpers.AlmostEqualRelative(density, d.Density(x), 16);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new InverseWishart(1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
            d.Sample();
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            InverseWishart.Sample(new System.Random(0), 1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1));
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => InverseWishart.Sample(new System.Random(0), -1.0, Matrix<double>.Build.RandomPositiveDefinite(2, 1)), Throws.ArgumentException);
        }
    }
}
