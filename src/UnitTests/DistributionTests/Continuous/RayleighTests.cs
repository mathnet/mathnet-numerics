// <copyright file="RayleighTests.cs" company="Math.NET">
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
    /// Rayleigh distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class RayleighTests
    {
        /// <summary>
        /// Can create Rayleigh
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanCreateRayleigh(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// Rayleigh create fails with bad parameters.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NaN)]
        [TestCase(Double.NegativeInfinity)]
        [TestCase(-1.0)]
        [TestCase(0.0)]
        public void RayleighCreateFailsWithBadParameters(double scale)
        {
            Assert.That(() => new Rayleigh(scale), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Rayleigh(2d);
            Assert.AreEqual("Rayleigh(σ = 2)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMean(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual(scale * Math.Sqrt(Constants.PiOver2), n.Mean);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateVariance(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual((2.0 - Constants.PiOver2) * scale * scale, n.Variance);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateStdDev(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual(Math.Sqrt(2.0 - Constants.PiOver2) * scale, n.StdDev);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateEntropy(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual(1.0 + Math.Log(scale / Constants.Sqrt2) + (Constants.EulerMascheroni / 2.0), n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="skn">Expected value.</param>
        [TestCase(0.1, 0.63111065781893638)]
        [TestCase(1.0, 0.63111065781893638)]
        [TestCase(10.0, 0.63111065781893638)]
        [TestCase(Double.PositiveInfinity, 0.63111065781893638)]
        public void ValidateSkewness(double scale, double skn)
        {
            var n = new Rayleigh(scale);
            AssertHelpers.AlmostEqualRelative(skn, n.Skewness, 17);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMode(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual(scale, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMedian(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual(scale * Math.Sqrt(Math.Log(4.0)), n.Median);
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
            var n = new Rayleigh(scale);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMaximum(double scale)
        {
            var n = new Rayleigh(1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensity(double scale, double x)
        {
            var n = new Rayleigh(scale);
            double expected = (x/(scale*scale))*Math.Exp(-x*x/(2.0*scale*scale));
            Assert.AreEqual(expected, n.Density(x));
            Assert.AreEqual(expected, Rayleigh.PDF(scale, x));
        }

        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensityLn(double scale, double x)
        {
            var n = new Rayleigh(scale);
            double expected = Math.Log(x/(scale*scale)) - (x*(x/(2.0*(scale*scale))));
            Assert.AreEqual(expected, n.DensityLn(x));
            Assert.AreEqual(expected, Rayleigh.PDFLn(scale, x));
        }

        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        public void ValidateInverseCumulativeDistribution(double scale, double x)
        {
            var n = new Rayleigh(scale);
            double cdf = 1.0 - Math.Exp(-x*x/(2.0*scale*scale));
            Assert.AreEqual(x, n.InverseCumulativeDistribution(cdf));
            Assert.AreEqual(x, Rayleigh.InvCDF(scale, cdf));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Rayleigh(1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Rayleigh(1.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }
    }
}
