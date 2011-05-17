// <copyright file="InverseGammaTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using System;
    using System.Linq;
    using Distributions;
    using NUnit.Framework;

    /// <summary>
    /// Inverse gamma distribution tests.
    /// </summary>
    [TestFixture]
    public class InverseGammaTests
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
        /// Can create inverse gamma.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreateInverseGamma(double a, double b)
        {
            var n = new InverseGamma(a, b);
            Assert.AreEqual(a, n.Shape);
            Assert.AreEqual(b, n.Scale);
        }

        /// <summary>
        /// Inverse gamma create fails with bad parameters.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        [TestCase(0.0, 1.0)]
        [TestCase(-1.0, 1.0)]
        [TestCase(-100.0, 1.0)]
        [TestCase(Double.NegativeInfinity, 1.0)]
        [TestCase(Double.NaN, 1.0)]
        [TestCase(1.0, 0.0)]
        [TestCase(1.0, -1.0)]
        [TestCase(1.0, -100.0)]
        [TestCase(1.0, Double.NegativeInfinity)]
        [TestCase(1.0, Double.NaN)]
        public void InverseGammaCreateFailsWithBadParameters(double a, double b)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new InverseGamma(a, b));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new InverseGamma(1.1, 2.1);
            Assert.AreEqual(String.Format("InverseGamma(Shape = {0}, Inverse Scale = {1})", n.Shape, n.Scale), n.ToString());
        }

        /// <summary>
        /// Can set A.
        /// </summary>
        /// <param name="a">A parameter.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanSetA(double a)
        {
            new InverseGamma(1.0, 1.0)
            {
                Shape = a
            };
        }

        /// <summary>
        /// Set A fails with non-positive value.
        /// </summary>
        /// <param name="a">A parameter.</param>
        [TestCase(-1.0)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        public void SetAFailsWithNonPositiveA(double a)
        {
            var n = new InverseGamma(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Shape = a);
        }

        /// <summary>
        /// Can set B.
        /// </summary>
        /// <param name="b">B parameter.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanSetB(double b)
        {
            new InverseGamma(1.0, 1.0)
            {
                Scale = b
            };
        }

        /// <summary>
        /// Set B fails with non-positive value.
        /// </summary>
        /// <param name="b">B parameter.</param>
        [TestCase(-1.0)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        public void SetBFailsWithNonPositiveB(double b)
        {
            var n = new InverseGamma(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Scale = b);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMean(double a, double b)
        {
            var n = new InverseGamma(a, b);
            if (a > 1)
            {
                Assert.AreEqual(b / (a - 1.0), n.Mean);
            }
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateVariance(double a, double b)
        {
            var n = new InverseGamma(a, b);
            if (a > 2)
            {
                Assert.AreEqual(b * b / ((a - 1.0) * (a - 1.0) * (a - 2.0)), n.Variance);
            }
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateStdDev(double a, double b)
        {
            var n = new InverseGamma(a, b);
            if (a > 2)
            {
                Assert.AreEqual(b / ((a - 1.0) * Math.Sqrt(a - 2.0)), n.StdDev);
            }
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMode(double a, double b)
        {
            var n = new InverseGamma(a, b);
            Assert.AreEqual(b / (a + 1.0), n.Mode);
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var n = new InverseGamma(1.0, 1.0);
            Assert.Throws<NotSupportedException>(() => { var median = n.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new InverseGamma(1.0, 1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new InverseGamma(1.0, 1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        /// <param name="x">Input X valuer.</param>
        [TestCase(0.1, 0.1, 1.2)]
        [TestCase(0.1, 1.0, 2.0)]
        [TestCase(0.1, Double.PositiveInfinity, 1.1)]
        [TestCase(1.0, 0.1, 1.5)]
        [TestCase(1.0, 1.0, 1.2)]
        [TestCase(1.0, Double.PositiveInfinity, 1.5)]
        [TestCase(Double.PositiveInfinity, 0.1, 5.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 2.5)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 1.0)]
        public void ValidateDensity(double a, double b, double x)
        {
            var n = new InverseGamma(a, b);
            if (x >= 0)
            {
                Assert.AreEqual(Math.Pow(b, a) * Math.Pow(x, -a - 1.0) * Math.Exp(-b / x) / SpecialFunctions.Gamma(a), n.Density(x));
            }
            else
            {
                Assert.AreEqual(0.0, n.Density(x));
            }
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        /// <param name="x">Input X valuer.</param>
        [TestCase(0.1, 0.1, 1.2)]
        [TestCase(0.1, 1.0, 2.0)]
        [TestCase(0.1, Double.PositiveInfinity, 1.1)]
        [TestCase(1.0, 0.1, 1.5)]
        [TestCase(1.0, 1.0, 1.2)]
        [TestCase(1.0, Double.PositiveInfinity, 1.5)]
        [TestCase(Double.PositiveInfinity, 0.1, 5.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 2.5)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 1.0)]
        public void ValidateDensityLn(double a, double b, double x)
        {
            var n = new InverseGamma(a, b);
            Assert.AreEqual(Math.Log(n.Density(x)), n.DensityLn(x));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new InverseGamma(1.0, 1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new InverseGamma(1.0, 1.0);
            var ied = n.Samples();
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="a">A parameter.</param>
        /// <param name="b">B parameter.</param>
        /// <param name="x">Input X valuer.</param>
        [TestCase(0.1, 0.1, 1.2)]
        [TestCase(0.1, 1.0, 2.0)]
        [TestCase(0.1, Double.PositiveInfinity, 1.1)]
        [TestCase(1.0, 0.1, 1.5)]
        [TestCase(1.0, 1.0, 1.2)]
        [TestCase(1.0, Double.PositiveInfinity, 1.5)]
        [TestCase(Double.PositiveInfinity, 0.1, 5.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 2.5)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 1.0)]
        public void ValidateCumulativeDistribution(double a, double b, double x)
        {
            var n = new InverseGamma(a, b);
            Assert.AreEqual(SpecialFunctions.GammaUpperRegularized(a, b / x), n.CumulativeDistribution(x));
        }
    }
}
