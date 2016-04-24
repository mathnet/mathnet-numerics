// <copyright file="CauchyTests.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Distributions;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    /// <summary>
    /// Cauchy distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class CauchyTests
    {
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
            Assert.That(() => new Cauchy(location, scale), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Cauchy(1d, 2d);
            Assert.AreEqual("Cauchy(x0 = 1, γ = 2)", n.ToString());
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
                double expected = 1.0 / ((Constants.Pi * scale) * (1.0 + (((x - location) / scale) * ((x - location) / scale))));
                Assert.AreEqual(expected, n.Density(x));
                Assert.AreEqual(expected, Cauchy.PDF(location, scale, x));
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
                double expected = -Math.Log((Constants.Pi * scale) * (1.0 + (((x - location) / scale) * ((x - location) / scale))));
                Assert.AreEqual(expected, n.DensityLn(x));
                Assert.AreEqual(expected, Cauchy.PDFLn(location, scale, x));
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
            GC.KeepAlive(ied.Take(5).ToArray());
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
                double expected = (Math.Atan((x - location)/scale))/Math.PI + 0.5;
                Assert.AreEqual(expected, n.CumulativeDistribution(x), 1e-12);
                Assert.AreEqual(expected, Cauchy.CDF(location, scale, x), 1e-12);
            }
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(-5.0, 100.0)]
        public void ValidateInverseCumulativeDistribution(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                double expected = (Math.Atan((x - location)/scale))/Math.PI + 0.5;
                Assert.AreEqual(x, n.InverseCumulativeDistribution(expected), 1e-12);
                Assert.AreEqual(x, Cauchy.InvCDF(location, scale, expected), 1e-12);
            }
        }
    }
}
