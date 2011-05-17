// <copyright file="LaplaceTests.cs" company="Math.NET">
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
    /// Laplace distribution tests.
    /// </summary>
    [TestFixture]
    public class LaplaceTests
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
        /// Can create Laplace.
        /// </summary>
        [Test]
        public void CanCreateLaplace()
        {
            var n = new Laplace();
            Assert.AreEqual(0.0, n.Location);
            Assert.AreEqual(1.0, n.Scale);
        }

        /// <summary>
        /// Can create Laplace.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NegativeInfinity, 0.1)]
        [TestCase(-5.0 - 1.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(1.0, 7.0)]
        [TestCase(5.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreateLaplace(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(location, n.Location);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Laplace(-1.0, 2.0);
            Assert.AreEqual("Laplace(Location = -1, Scale = 2)", n.ToString());
        }

        /// <summary>
        /// Can set location.
        /// </summary>
        /// <param name="location">Location value.</param>
        [TestCase(Double.NegativeInfinity)]
        [TestCase(-5.0 - 1.0)]
        [TestCase(0.0)]
        [TestCase(1.0)]
        [TestCase(5.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanSetLocation(double location)
        {
            new Laplace
            {
                Location = location
            };
        }

        /// <summary>
        /// Set location fails with negative value.
        /// </summary>
        [Test]
        public void SetLocationFailsWithNegativeLocation()
        {
            var n = new Laplace();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Location = Double.NaN);
        }

        /// <summary>
        /// Can set scale.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            new Laplace
            {
                Scale = scale
            };
        }

        /// <summary>
        /// Set scale fails with negative value.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.0)]
        [TestCase(-1.0)]
        [TestCase(-5.0)]
        [TestCase(Double.NegativeInfinity)]
        [TestCase(Double.NaN)]
        public void SetScaleFailsWithNegativeScale(double scale)
        {
            var n = new Laplace();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Scale = scale);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NegativeInfinity, 0.1)]
        [TestCase(-5.0 - 1.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(1.0, 7.0)]
        [TestCase(5.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMean(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(location, n.Mean);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NegativeInfinity, 0.1)]
        [TestCase(-5.0 - 1.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(1.0, 7.0)]
        [TestCase(5.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateVariance(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(2.0 * scale * scale, n.Variance);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NegativeInfinity, 0.1)]
        [TestCase(-5.0 - 1.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(1.0, 7.0)]
        [TestCase(5.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateStdDev(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(Math.Sqrt(2.0) * scale, n.StdDev);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NegativeInfinity, 0.1)]
        [TestCase(-5.0 - 1.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(1.0, 7.0)]
        [TestCase(5.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateEntropy(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(Math.Log(2.0 * Constants.E * scale), n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NegativeInfinity, 0.1)]
        [TestCase(-5.0 - 1.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(1.0, 7.0)]
        [TestCase(5.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateSkewness(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(0.0, n.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NegativeInfinity, 0.1)]
        [TestCase(-5.0 - 1.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(1.0, 7.0)]
        [TestCase(5.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMode(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(location, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NegativeInfinity, 0.1)]
        [TestCase(-5.0 - 1.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(1.0, 7.0)]
        [TestCase(5.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMedian(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(location, n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Laplace();
            Assert.AreEqual(Double.NegativeInfinity, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Laplace();
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.1, 1.5)]
        [TestCase(1.0, 0.1, 2.8)]
        [TestCase(-1.0, 0.1, -5.4)]
        [TestCase(5.0, 0.1, -4.9)]
        [TestCase(-5.0, 0.1, 2.0)]
        [TestCase(Double.PositiveInfinity, 0.1, 5.5)]
        [TestCase(Double.NegativeInfinity, 0.1, -0.0)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity)]
        [TestCase(1.0, 1.0, 5.0)]
        [TestCase(-1.0, 1.0, -1.0)]
        [TestCase(5.0, 1.0, -1.0)]
        [TestCase(-5.0, 1.0, 2.5)]
        [TestCase(Double.PositiveInfinity, 1.0, 2.0)]
        [TestCase(Double.NegativeInfinity, 1.0, 15.0)]
        [TestCase(0.0, Double.PositiveInfinity, 89.3)]
        [TestCase(1.0, Double.PositiveInfinity, -0.1)]
        [TestCase(-1.0, Double.PositiveInfinity, 0.1)]
        [TestCase(5.0, Double.PositiveInfinity, -6.1)]
        [TestCase(-5.0, Double.PositiveInfinity, -10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 2.0)]
        [TestCase(Double.NegativeInfinity, Double.PositiveInfinity, -5.1)]
        public void ValidateDensity(double location, double scale, double x)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(Math.Exp(-Math.Abs(x - location) / scale) / (2.0 * scale), n.Density(x));
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.1, 1.5)]
        [TestCase(1.0, 0.1, 2.8)]
        [TestCase(-1.0, 0.1, -5.4)]
        [TestCase(5.0, 0.1, -4.9)]
        [TestCase(-5.0, 0.1, 2.0)]
        [TestCase(Double.PositiveInfinity, 0.1, 5.5)]
        [TestCase(Double.NegativeInfinity, 0.1, -0.0)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity)]
        [TestCase(1.0, 1.0, 5.0)]
        [TestCase(-1.0, 1.0, -1.0)]
        [TestCase(5.0, 1.0, -1.0)]
        [TestCase(-5.0, 1.0, 2.5)]
        [TestCase(Double.PositiveInfinity, 1.0, 2.0)]
        [TestCase(Double.NegativeInfinity, 1.0, 15.0)]
        [TestCase(0.0, Double.PositiveInfinity, 89.3)]
        [TestCase(1.0, Double.PositiveInfinity, -0.1)]
        [TestCase(-1.0, Double.PositiveInfinity, 0.1)]
        [TestCase(5.0, Double.PositiveInfinity, -6.1)]
        [TestCase(-5.0, Double.PositiveInfinity, -10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 2.0)]
        [TestCase(Double.NegativeInfinity, Double.PositiveInfinity, -5.1)]
        public void ValidateDensityLn(double location, double scale, double x)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(-Math.Log(2.0 * scale) - (Math.Abs(x - location) / scale), n.DensityLn(x));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Laplace();
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Laplace();
            var ied = n.Samples();
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.1, 1.5)]
        [TestCase(1.0, 0.1, 2.8)]
        [TestCase(-1.0, 0.1, -5.4)]
        [TestCase(5.0, 0.1, -4.9)]
        [TestCase(-5.0, 0.1, 2.0)]
        [TestCase(Double.PositiveInfinity, 0.1, 5.5)]
        [TestCase(Double.NegativeInfinity, 0.1, -0.0)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity)]
        [TestCase(1.0, 1.0, 5.0)]
        [TestCase(-1.0, 1.0, -1.0)]
        [TestCase(5.0, 1.0, -1.0)]
        [TestCase(-5.0, 1.0, 2.5)]
        [TestCase(Double.PositiveInfinity, 1.0, 2.0)]
        [TestCase(Double.NegativeInfinity, 1.0, 15.0)]
        [TestCase(0.0, Double.PositiveInfinity, 89.3)]
        [TestCase(1.0, Double.PositiveInfinity, -0.1)]
        [TestCase(-1.0, Double.PositiveInfinity, 0.1)]
        [TestCase(5.0, Double.PositiveInfinity, -6.1)]
        [TestCase(-5.0, Double.PositiveInfinity, -10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 2.0)]
        [TestCase(Double.NegativeInfinity, Double.PositiveInfinity, -5.1)]
        public void ValidateCumulativeDistribution(double location, double scale, double x)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual(0.5 * (1.0 + (Math.Sign(x - location) * (1.0 - Math.Exp(-Math.Abs(x - location) / scale)))), n.CumulativeDistribution(x));
        }
    }
}
