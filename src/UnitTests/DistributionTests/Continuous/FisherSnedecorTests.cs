// <copyright file="FisherSnedecorTests.cs" company="Math.NET">
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
    /// Fisher-Snedecor distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class FisherSnedecorTests
    {
        /// <summary>
        /// Can create fisher snedecor.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(Double.PositiveInfinity, 0.1)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreateFisherSnedecor(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            Assert.AreEqual(d1, n.DegreesOfFreedom1);
            Assert.AreEqual(d2, n.DegreesOfFreedom2);
        }

        /// <summary>
        /// <c>FisherSnedecor</c> create fails with bad parameters.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        [TestCase(Double.NaN, Double.NaN)]
        [TestCase(0.0, Double.NaN)]
        [TestCase(-1.0, Double.NaN)]
        [TestCase(-10.0, Double.NaN)]
        [TestCase(Double.NaN, 0.0)]
        [TestCase(0.0, 0.0)]
        [TestCase(-1.0, 0.0)]
        [TestCase(-10.0, 0.0)]
        [TestCase(Double.NaN, -1.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, -1.0)]
        [TestCase(-10.0, -1.0)]
        [TestCase(Double.NaN, -10.0)]
        [TestCase(0.0, -10.0)]
        [TestCase(-1.0, -10.0)]
        [TestCase(-10.0, -10.0)]
        public void FisherSnedecorCreateFailsWithBadParameters(double d1, double d2)
        {
            Assert.That(() => new FisherSnedecor(d1, d2), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new FisherSnedecor(2d, 1d);
            Assert.AreEqual("FisherSnedecor(d1 = 2, d2 = 1)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(Double.PositiveInfinity, 0.1)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMean(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d2 > 2)
            {
                Assert.AreEqual(d2 / (d2 - 2.0), n.Mean);
            }
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(Double.PositiveInfinity, 0.1)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateVariance(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d2 > 4)
            {
                Assert.AreEqual((2.0 * d2 * d2 * (d1 + d2 - 2.0)) / (d1 * (d2 - 2.0) * (d2 - 2.0) * (d2 - 4.0)), n.Variance);
            }
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(Double.PositiveInfinity, 0.1)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateStdDev(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d2 > 4)
            {
                Assert.AreEqual(Math.Sqrt(n.Variance), n.StdDev);
            }
        }

        /// <summary>
        /// Validate entropy throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateEntropyThrowsNotSupportedException()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            Assert.Throws<NotSupportedException>(() => { var ent = n.Entropy; });
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(Double.PositiveInfinity, 0.1)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.1, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(10.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateSkewness(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d2 > 6)
            {
                Assert.AreEqual((((2.0 * d1) + d2 - 2.0) * Math.Sqrt(8.0 * (d2 - 4.0))) / ((d2 - 6.0) * Math.Sqrt(d1 * (d1 + d2 - 2.0))), n.Skewness);
            }
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 0.1)]
        [TestCase(100.0, 0.1)]
        [TestCase(0.1, 1.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(100.0, 1.0)]
        [TestCase(0.1, 100.0)]
        [TestCase(1.0, 100.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(100.0, 100.0)]
        public void ValidateMode(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d1 > 2)
            {
                Assert.AreEqual((d2 * (d1 - 2.0)) / (d1 * (d2 + 2.0)), n.Mode);
            }
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            Assert.Throws<NotSupportedException>(() => { var m = n.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        /// <param name="x">Input X value</param>
        [TestCase(0.1, 0.1, 1.0)]
        [TestCase(1.0, 0.1, 1.0)]
        [TestCase(10.0, 0.1, 1.0)]
        [TestCase(100.0, 0.1, 1.0)]
        [TestCase(0.1, 1.0, 1.0)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 1.0, 1.0)]
        [TestCase(100.0, 1.0, 1.0)]
        [TestCase(0.1, 100.0, 1.0)]
        [TestCase(1.0, 100.0, 1.0)]
        [TestCase(10.0, 100.0, 1.0)]
        [TestCase(100.0, 100.0, 1.0)]
        [TestCase(0.1, 0.1, 10.0)]
        [TestCase(1.0, 0.1, 10.0)]
        [TestCase(10.0, 0.1, 10.0)]
        [TestCase(100.0, 0.1, 10.0)]
        [TestCase(0.1, 1.0, 10.0)]
        [TestCase(1.0, 1.0, 10.0)]
        [TestCase(10.0, 1.0, 10.0)]
        [TestCase(100.0, 1.0, 10.0)]
        [TestCase(0.1, 100.0, 10.0)]
        [TestCase(1.0, 100.0, 10.0)]
        [TestCase(10.0, 100.0, 10.0)]
        [TestCase(100.0, 100.0, 10.0)]
        public void ValidateDensity(double d1, double d2, double x)
        {
            var n = new FisherSnedecor(d1, d2);
            double expected = Math.Sqrt(Math.Pow(d1*x, d1)*Math.Pow(d2, d2)/Math.Pow((d1*x) + d2, d1 + d2))/(x*SpecialFunctions.Beta(d1/2.0, d2/2.0));
            Assert.AreEqual(expected, n.Density(x));
            Assert.AreEqual(expected, FisherSnedecor.PDF(d1, d2, x));
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="d1">Degrees of freedom 1</param>
        /// <param name="d2">Degrees of freedom 2</param>
        /// <param name="x">Input X value</param>
        [TestCase(0.1, 0.1, 1.0)]
        [TestCase(1.0, 0.1, 1.0)]
        [TestCase(10.0, 0.1, 1.0)]
        [TestCase(100.0, 0.1, 1.0)]
        [TestCase(0.1, 1.0, 1.0)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 1.0, 1.0)]
        [TestCase(100.0, 1.0, 1.0)]
        [TestCase(0.1, 100.0, 1.0)]
        [TestCase(1.0, 100.0, 1.0)]
        [TestCase(10.0, 100.0, 1.0)]
        [TestCase(100.0, 100.0, 1.0)]
        [TestCase(0.1, 0.1, 10.0)]
        [TestCase(1.0, 0.1, 10.0)]
        [TestCase(10.0, 0.1, 10.0)]
        [TestCase(100.0, 0.1, 10.0)]
        [TestCase(0.1, 1.0, 10.0)]
        [TestCase(1.0, 1.0, 10.0)]
        [TestCase(10.0, 1.0, 10.0)]
        [TestCase(100.0, 1.0, 10.0)]
        [TestCase(0.1, 100.0, 10.0)]
        [TestCase(1.0, 100.0, 10.0)]
        [TestCase(10.0, 100.0, 10.0)]
        [TestCase(100.0, 100.0, 10.0)]
        public void ValidateDensityLn(double d1, double d2, double x)
        {
            var n = new FisherSnedecor(d1, d2);
            double expected = Math.Log(Math.Sqrt(Math.Pow(d1*x, d1)*Math.Pow(d2, d2)/Math.Pow((d1*x) + d2, d1 + d2))/(x*SpecialFunctions.Beta(d1/2.0, d2/2.0)));
            Assert.AreEqual(expected, n.DensityLn(x));
            Assert.AreEqual(expected, FisherSnedecor.PDFLn(d1, d2, x));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        [TestCase(0.1, 0.1, 1.0)]
        [TestCase(1.0, 0.1, 1.0)]
        [TestCase(10.0, 0.1, 1.0)]
        [TestCase(0.1, 1.0, 1.0)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 1.0, 1.0)]
        [TestCase(0.1, 0.1, 10.0)]
        [TestCase(1.0, 0.1, 10.0)]
        [TestCase(10.0, 0.1, 10.0)]
        [TestCase(0.1, 1.0, 10.0)]
        [TestCase(1.0, 1.0, 10.0)]
        [TestCase(10.0, 1.0, 10.0)]
        public void ValidateCumulativeDistribution(double d1, double d2, double x)
        {
            var n = new FisherSnedecor(d1, d2);
            double expected = SpecialFunctions.BetaRegularized(d1/2.0, d2/2.0, d1*x/(d2 + (x*d1)));
            Assert.That(n.CumulativeDistribution(x), Is.EqualTo(expected));
            Assert.That(FisherSnedecor.CDF(d1, d2, x), Is.EqualTo(expected));
        }

        [TestCase(0.1, 0.1, 1.0)]
        [TestCase(1.0, 0.1, 1.0)]
        [TestCase(10.0, 0.1, 1.0)]
        [TestCase(0.1, 1.0, 1.0)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 1.0, 1.0)]
        [TestCase(0.1, 0.1, 10.0)]
        [TestCase(1.0, 0.1, 10.0)]
        [TestCase(10.0, 0.1, 10.0)]
        [TestCase(0.1, 1.0, 10.0)]
        [TestCase(1.0, 1.0, 10.0)]
        [TestCase(10.0, 1.0, 10.0)]
        public void ValidateInverseCumulativeDistribution(double d1, double d2, double x)
        {
            var n = new FisherSnedecor(d1, d2);
            double p = SpecialFunctions.BetaRegularized(d1/2.0, d2/2.0, d1*x/(d2 + (x*d1)));
            Assert.That(n.InverseCumulativeDistribution(p), Is.EqualTo(x).Within(1e-8));
            Assert.That(FisherSnedecor.InvCDF(d1, d2, p), Is.EqualTo(x).Within(1e-8));
        }
    }
}
