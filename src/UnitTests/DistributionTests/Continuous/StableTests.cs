// <copyright file="StableTests.cs" company="Math.NET">
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
    /// Stable distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class StableTests
    {
        /// <summary>
        /// Can create stable.
        /// </summary>
        /// <param name="alpha">Alpha value.</param>
        /// <param name="beta">Beta value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="location">Location value.</param>
        [TestCase(0.1, -1.0, 0.1, Double.NegativeInfinity)]
        [TestCase(2.0, 1.0, Double.PositiveInfinity, -1.0)]
        [TestCase(0.1, -1.0, 0.1, 0.0)]
        [TestCase(2.0, 1.0, Double.PositiveInfinity, 1.0)]
        [TestCase(0.1, -1.0, 0.1, Double.PositiveInfinity)]
        public void CanCreateStable(double alpha, double beta, double scale, double location)
        {
            var n = new Stable(alpha, beta, scale, location);
            Assert.AreEqual(alpha, n.Alpha);
            Assert.AreEqual(beta, n.Beta);
            Assert.AreEqual(scale, n.Scale);
            Assert.AreEqual(location, n.Location);
        }

        /// <summary>
        /// Stable create fails with bad parameters.
        /// </summary>
        /// <param name="alpha">Alpha value.</param>
        /// <param name="beta">Beta value.</param>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NaN, Double.NaN, Double.NaN, Double.NaN)]
        [TestCase(1.0, Double.NaN, Double.NaN, Double.NaN)]
        [TestCase(Double.NaN, 1.0, Double.NaN, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN, 1.0, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN, Double.NaN, 1.0)]
        [TestCase(1.0, 1.0, Double.NaN, Double.NaN)]
        [TestCase(1.0, Double.NaN, 1.0, Double.NaN)]
        [TestCase(1.0, Double.NaN, Double.NaN, 1.0)]
        [TestCase(Double.NaN, 1.0, 1.0, Double.NaN)]
        [TestCase(1.0, 1.0, 1.0, Double.NaN)]
        [TestCase(1.0, 1.0, Double.NaN, 1.0)]
        [TestCase(1.0, Double.NaN, 1.0, 1.0)]
        [TestCase(Double.NaN, 1.0, 1.0, 1.0)]
        [TestCase(1.0, 1.0, 0.0, 1.0)]
        [TestCase(1.0, -1.1, 1.0, 1.0)]
        [TestCase(1.0, 1.1, 1.0, 1.0)]
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(2.1, 1.0, 1.0, 1.0)]
        public void StableCreateFailsWithBadParameters(double alpha, double beta, double location, double scale)
        {
            Assert.That(() => new Stable(alpha, beta, location, scale), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Stable(1.2d, 0.3d, 1d, 2d);
            Assert.AreEqual("Stable(α = 1.2, β = 0.3, c = 1, μ = 2)", n.ToString());
        }

        /// <summary>
        /// Validate entropy throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateEntropyThrowsNotSupportedException()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            Assert.Throws<NotSupportedException>(() => { var e = n.Entropy; });
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        [Test]
        public void ValidateSkewness()
        {
            var n = new Stable(2.0, 1.0, 1.0, 1.0);
            if (n.Alpha == 2)
            {
                Assert.AreEqual(0.0, n.Skewness);
            }
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="location">Location value.</param>
        [TestCase(Double.NegativeInfinity)]
        [TestCase(-10.0)]
        [TestCase(-1.0)]
        [TestCase(-0.1)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMode(double location)
        {
            var n = new Stable(1.0, 0.0, 1.0, location);
            if (n.Beta == 0)
            {
                Assert.AreEqual(location, n.Mode);
            }
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="location">Location value.</param>
        [TestCase(Double.NegativeInfinity)]
        [TestCase(-10.0)]
        [TestCase(-1.0)]
        [TestCase(-0.1)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMedian(double location)
        {
            var n = new Stable(1.0, 0.0, 1.0, location);
            if (n.Beta == 0)
            {
                Assert.AreEqual(location, n.Mode);
            }
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        /// <param name="beta">Beta value.</param>
        [TestCase(-1.0)]
        [TestCase(-0.1)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        public void ValidateMinimum(double beta)
        {
            var n = new Stable(1.0, beta, 1.0, 1.0);
            Assert.AreEqual(Math.Abs(beta) != 1 ? Double.NegativeInfinity : 0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="alpha">Alpha value.</param>
        /// <param name="beta">Beta value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="location">Location value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="d">Expected value.</param>
        [TestCase(2.0, -1.0, 1.0, 0.0, 1.5, 0.16073276729880184)]
        [TestCase(2.0, -1.0, 1.0, 0.0, 3.0, 0.029732572305907354)]
        [TestCase(2.0, -1.0, 1.0, 0.0, 5.0, 0.00054457105758817781)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 1.5, 0.097941503441166353)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 3.0, 0.031830988618379068)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 5.0, 0.012242687930145794)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 1.5, 0.15559955475708653)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 3.0, 0.064989885240913717)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 5.0, 0.032286845174307237)]
        public void ValidateDensity(double alpha, double beta, double scale, double location, double x, double d)
        {
            var n = new Stable(alpha, beta, scale, location);
            AssertHelpers.AlmostEqualRelative(d, n.Density(x), 15);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="alpha">Alpha value.</param>
        /// <param name="beta">Beta value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="location">Location value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="dln">Expected value.</param>
        [TestCase(2.0, -1.0, 1.0, 0.0, 1.5, -1.8280121234846454)]
        [TestCase(2.0, -1.0, 1.0, 0.0, 3.0, -3.5155121234846449)]
        [TestCase(2.0, -1.0, 1.0, 0.0, 5.0, -7.5155121234846449)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 1.5, -2.3233848821910463)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 3.0, -3.4473149788434458)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 5.0, -4.4028264238708825)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 1.5, -1.8604695287002526)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 3.0, -2.7335236328735038)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 5.0, -3.4330954018558235)]
        public void ValidateDensityLn(double alpha, double beta, double scale, double location, double x, double dln)
        {
            var n = new Stable(alpha, beta, scale, location);
            AssertHelpers.AlmostEqualRelative(dln, n.DensityLn(x), 15);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="alpha">Alpha value.</param>
        /// <param name="beta">Beta value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="location">Location value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [TestCase(2.0, -1.0, 1.0, 0.0, 1.5, 0.8555778168267576)]
        [TestCase(2.0, -1.0, 1.0, 0.0, 3.0, 0.98305257323765538)]
        [TestCase(2.0, -1.0, 1.0, 0.0, 5.0, 0.9997965239912775)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 1.5, 0.81283295818900125)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 3.0, 0.89758361765043326)]
        [TestCase(1.0, 0.0, 1.0, 0.0, 5.0, 0.93716704181099886)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 1.5, 0.41421617824252516)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 3.0, 0.563702861650773)]
        [TestCase(0.5, 1.0, 1.0, 0.0, 5.0, 0.65472084601857694)]
        public void ValidateCumulativeDistribution(double alpha, double beta, double scale, double location, double x, double cdf)
        {
            var n = new Stable(alpha, beta, scale, location);
            AssertHelpers.AlmostEqualRelative(cdf, n.CumulativeDistribution(x), 15);
        }
    }
}
