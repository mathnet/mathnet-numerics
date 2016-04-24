// <copyright file="ParetoTests.cs" company="Math.NET">
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
    /// Pareto distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class ParetoTests
    {
        /// <summary>
        /// Can create Pareto distribution.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreatePareto(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(scale, n.Scale);
            Assert.AreEqual(shape, n.Shape);
        }

        /// <summary>
        /// Pareto create fails with bad parameters.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(Double.NaN, 1.0)]
        [TestCase(1.0, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN)]
        [TestCase(1.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        [TestCase(-1.0, -1.0)]
        [TestCase(0.0, 0.0)]
        [TestCase(0.0, 1.0)]
        [TestCase(1.0, 0.0)]
        public void ParetoCreateFailsWithBadParameters(double scale, double shape)
        {
            Assert.That(() => { var n = new Pareto(scale, shape); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Pareto(1d, 2d);
            Assert.AreEqual("Pareto(xm = 1, α = 2)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMean(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            if (shape > 1)
            {
                Assert.AreEqual(shape * scale / (shape - 1.0), n.Mean);
            }
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateVariance(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            if (shape <= 2.0)
            {
                Assert.AreEqual(double.PositiveInfinity, n.Variance);
            }
            else
            {
                Assert.AreEqual(scale * scale * shape / ((shape - 1.0) * (shape - 1.0) * (shape - 2.0)), n.Variance);
            }
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateStdDev(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual((scale * Math.Sqrt(shape)) / ((shape - 1.0) * Math.Sqrt(shape - 2.0)), n.StdDev);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateEntropy(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(Math.Log(shape / scale) - (1.0 / shape) - 1.0, n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateSkewness(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual((2.0 * (shape + 1.0) / (shape - 3.0)) * Math.Sqrt((shape - 2.0) / shape), n.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMode(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(scale, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMedian(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(scale * Math.Pow(2.0, 1.0 / shape), n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMinimum(double scale)
        {
            var n = new Pareto(scale, 1.0);
            Assert.AreEqual(scale, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Pareto(1.0, 1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        [TestCase(1, 1, 1, 1)]
        [TestCase(1, 1, 1.5, 4/9.0)]
        [TestCase(1, 1, 5, 1/25.0)]
        [TestCase(1, 1, 50, 1/2500.0)]
        [TestCase(1, 4, 1, 4)]
        [TestCase(1, 4, 1.5, 128/243.0)]
        [TestCase(1, 4, 50, 1/78125000.0)]
        [TestCase(3, 2, 3, 2/3.0)]
        [TestCase(3, 2, 5, 18/125.0)]
        [TestCase(25, 100, 50, 1.5777218104420236e-30)]
        [TestCase(100, 25, 150, 6.6003546737276816e-6)]
        public void ValidateDensity(double scale, double shape, double x, double expected)
        {
            var dist = new Pareto(scale, shape);

            Assert.AreEqual(expected, dist.Density(x), 1e-12);
            Assert.AreEqual(expected, Pareto.PDF(scale, shape, x), 1e-12);

            Assert.AreEqual(Math.Log(expected), dist.DensityLn(x), 1e-12);
            Assert.AreEqual(Math.Log(expected), Pareto.PDFLn(scale, shape, x), 1e-12);
        }

        [TestCase(0.1, 0.1, 0.1)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(5.0, 5.0, 2.0)]
        [TestCase(7.0, 7.0, 10.0)]
        [TestCase(10.0, 10.0, 12.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double scale, double shape, double x)
        {
            var n = new Pareto(scale, shape);
            double expected = 1.0 - Math.Pow(scale/x, shape);
            Assert.AreEqual(expected, n.CumulativeDistribution(x));
            Assert.AreEqual(expected, Pareto.CDF(scale, shape, x));
        }

        [TestCase(0.1, 0.1, 0.1)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(5.0, 5.0, 2.0)]
        [TestCase(7.0, 7.0, 10.0)]
        [TestCase(10.0, 10.0, 12.0)]
        public void ValidateInverseCumulativeDistribution(double scale, double shape, double x)
        {
            var n = new Pareto(scale, shape);
            double cdf = 1.0 - Math.Pow(scale / x, shape);
            Assert.AreEqual(x, n.InverseCumulativeDistribution(cdf), 1e-12);
            Assert.AreEqual(x, Pareto.InvCDF(scale, shape, cdf), 1e-12);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Pareto(1.0, 1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Pareto(1.0, 1.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }
    }
}
