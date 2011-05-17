// <copyright file="CauchyTests.cs" company="Math.NET">
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
    /// Cauchy distribution tests.
    /// </summary>
    [TestFixture]
    public class CauchyTests
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
        /// Can create Cauchy.
        /// </summary>
        [Test]
        public void CanCreateCauchy()
        {
            var n = new Cauchy();
            Assert.AreEqual(0.0, n.Location);
            Assert.AreEqual(1.0, n.Scale);
        }

        /// <summary>
        /// Can create Cauchy.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(-5.0, 100.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void CanCreateCauchy(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual(location, n.Location);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// Cauchy create fails with bad parameters.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NaN, 1.0)]
        [TestCase(1.0, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN)]
        [TestCase(1.0, 0.0)]
        public void CauchyCreateFailsWithBadParameters(double location, double scale)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Cauchy(location, scale));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Cauchy(1.0, 2.0);
            Assert.AreEqual("Cauchy(Location = 1, Scale = 2)", n.ToString());
        }

        /// <summary>
        /// Can set location.
        /// </summary>
        /// <param name="location">Location value.</param>
        [TestCase(-10.0)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        public void CanSetLocation(double location)
        {
            new Cauchy
            {
                Location = location
            };
        }

        /// <summary>
        /// Set bad location fails.
        /// </summary>
        [Test]
        public void SetBadLocationFail()
        {
            var n = new Cauchy();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Location = Double.NaN);
        }

        /// <summary>
        /// Can set scale.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(12.0)]
        public void CanSetScale(double scale)
        {
            new Cauchy
            {
                Scale = scale
            };
        }

        /// <summary>
        /// Set bad scale fails.
        /// </summary>
        [Test]
        public void SetBadScaleFail()
        {
            var n = new Cauchy();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Scale = -1.0);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        public void ValidateEntropy(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual(Math.Log(4.0 * Constants.Pi * scale), n.Entropy);
        }

        /// <summary>
        /// Validate skewness throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateSkewnessThrowsNotSupportedException()
        {
            var n = new Cauchy(-0.0, 2.0);
            Assert.Throws<NotSupportedException>(() => { var s = n.Skewness; });
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateMode(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual(location, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateMedian(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual(location, n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateMinimum(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual(Double.NegativeInfinity, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateMaximum(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(-5.0, 100.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        public void ValidateDensity(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                Assert.AreEqual(1.0 / ((Constants.Pi * scale) * (1.0 + (((x - location) / scale) * ((x - location) / scale)))), n.Density(x));
            }
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(-5.0, 100.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        public void ValidateDensityLn(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                Assert.AreEqual(-Math.Log((Constants.Pi * scale) * (1.0 + (((x - location) / scale) * ((x - location) / scale)))), n.DensityLn(x));
            }
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Cauchy();
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Cauchy();
            var ied = n.Samples();
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(-5.0, 100.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                Assert.AreEqual(((1.0 / Constants.Pi) * Math.Atan((x - location) / scale)) + 0.5, n.CumulativeDistribution(x));
            }
        }
    }
}
