// <copyright file="ExponentialTests.cs" company="Math.NET">
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
    /// Exponential distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class ExponentialTests
    {
        /// <summary>
        /// Can create exponential.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanCreateExponential(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(lambda, n.Rate);
        }

        /// <summary>
        /// Exponential create fails with bad parameter.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(Double.NaN)]
        [TestCase(-1.0)]
        [TestCase(-10.0)]
        public void ExponentialCreateFailsWithBadParameters(double lambda)
        {
            Assert.That(() => new Exponential(lambda), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Exponential(2d);
            Assert.AreEqual("Exponential(λ = 2)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMean(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(1.0 / lambda, n.Mean);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateVariance(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(1.0 / (lambda * lambda), n.Variance);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateStdDev(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(1.0 / lambda, n.StdDev);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateEntropy(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(1.0 - Math.Log(lambda), n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateSkewness(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(2.0, n.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMode(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(0.0, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMedian(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(Math.Log(2.0) / lambda, n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Exponential(1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Exponential(1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.1, 0.0)]
        [TestCase(1.0, 0.0)]
        [TestCase(10.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(Double.PositiveInfinity, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensity(double lambda, double x)
        {
            var n = new Exponential(lambda);
            if (x >= 0)
            {
                Assert.AreEqual(lambda*Math.Exp(-lambda*x), n.Density(x));
                Assert.AreEqual(lambda*Math.Exp(-lambda*x), Exponential.PDF(lambda, x));
            }
            else
            {
                Assert.AreEqual(0.0, n.Density(lambda));
                Assert.AreEqual(0.0, Exponential.PDF(lambda, lambda));
            }
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.1, 0.0)]
        [TestCase(1.0, 0.0)]
        [TestCase(10.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(Double.PositiveInfinity, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensityLn(double lambda, double x)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(Math.Log(lambda) - (lambda*x), n.DensityLn(x));
            Assert.AreEqual(Math.Log(lambda) - (lambda*x), Exponential.PDFLn(lambda, x));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Exponential(1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Exponential(1.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.1, 0.0)]
        [TestCase(1.0, 0.0)]
        [TestCase(10.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(Double.PositiveInfinity, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double lambda, double x)
        {
            var n = new Exponential(lambda);
            if (x >= 0.0)
            {
                Assert.AreEqual(1.0 - Math.Exp(-lambda*x), n.CumulativeDistribution(x));
                Assert.AreEqual(1.0 - Math.Exp(-lambda*x), Exponential.CDF(lambda, x));
            }
            else
            {
                Assert.AreEqual(0.0, n.CumulativeDistribution(x));
                Assert.AreEqual(0.0, Exponential.CDF(lambda, x));
            }
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.1, 0.0)]
        [TestCase(1.0, 0.0)]
        [TestCase(10.0, 0.0)]
        [TestCase(10.0, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        public void ValidateInverseCumulativeDistribution(double lambda, double x)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual(x, n.InverseCumulativeDistribution(1.0 - Math.Exp(-lambda*x)));
            Assert.AreEqual(x, Exponential.InvCDF(lambda, 1.0 - Math.Exp(-lambda*x)));
        }
    }
}
