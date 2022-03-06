// <copyright file="BurrTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2019 Math.NET
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

using MathNet.Numerics.Distributions;
using NUnit.Framework;
using System;
using System.Linq;

namespace MathNet.Numerics.Tests.DistributionTests.Continuous
{
    /// <summary>
    /// <c>Burr</c> distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class BurrTests
    {
        private const int highPrecision = 12;
        private const int lowPrecision = 8;

        /// <summary>
        /// Can create <c>Burr</c>.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        [TestCase(1.0, 1.0, 0.5)]
        [TestCase(10.0, 0.1, 0.2)]
        [TestCase(5.0, 1.0, 3)]
        [TestCase(2.0, 10.0, 2.0)]
        [TestCase(10.0, 100.0, 1.0)]
        public void CanCreateBurr(double a, double c, double k)
        {
            var n = new Burr(a, c, k);
            Assert.AreEqual(a, n.A);
            Assert.AreEqual(c, n.C);
            Assert.AreEqual(k, n.K);
        }

        /// <summary>
        /// Can create <c>Burr</c> with random source.
        /// </summary>
        [Test]
        public void CanCreateBurrWithRandomSource()
        {
            var randomSource = new Numerics.Random.MersenneTwister(100);
            var n = new Burr(1.0, 1.0, 0.5, randomSource);
            Assert.AreEqual(randomSource, n.RandomSource);
        }

        /// <summary>
        /// <c>Burr</c> create fails with bad parameters.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        [TestCase(5.0, 1.0, -1.0)]
        [TestCase(-1.0, 1.0, 2.05)]
        [TestCase(1.0, -1.0, 2.05)]
        [TestCase(double.NaN, double.NaN, double.NaN)]
        [TestCase(double.NaN, 1.0, 2.4)]
        [TestCase(1.0, 1.0, double.PositiveInfinity)]
        [TestCase(1.0, -1.0, double.NegativeInfinity)]
        public void BurrCreateFailsWithBadParameters(double a, double c, double k)
        {
            Assert.That(() => new Burr(a, c, k), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate IsValidParameterSet.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="validity">exoected validity of paremeter set.</param>
        [TestCase(1.0, 1.0, 0.5, true)]
        [TestCase(10.0, 0.1, 0.2, true)]
        [TestCase(5.0, 1.0, -1.0, false)]
        [TestCase(1.0, 1.0, double.PositiveInfinity, false)]
        public void ValidateIsValidParameterSet(double a, double c, double k, bool validity)
        {
            Assert.AreEqual(Burr.IsValidParameterSet(a, c, k), validity);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var n = new Burr(1d, 2d, 3d);
            Assert.AreEqual("Burr(a = 1, c = 2, k = 3)", n.ToString());
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="mode">Expected value.</param>
        [TestCase(2.000000, 2.00000, 1.0, 1.154700538379251)]
        [TestCase(1.000000, 1.00000, 0.5, 0.0)]
        [TestCase(3.000000, 4.00000, 1.0, 2.640335210380180)]
        public void ValidateMode(double a, double c, double k, double mode)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(mode, n.Mode, highPrecision);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="median">Expected value.</param>
        [TestCase(1.000000, 1.00000, 0.5, 3.0)]
        [TestCase(1.000000, 0.50000, 0.5, 9.0)]
        [TestCase(1.000000, 1.00000, 5.0, 0.148698354997035)]
        public void ValidateMedian(double a, double c, double k, double median)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(median, n.Median, highPrecision);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="mean">Expected value.</param>
        [TestCase(1.000000, 1.00000, 1.5, 2)]
        [TestCase(4.000000, 5.00000, 0.5, 6.198785110989412)]
        [TestCase(4.000000, 5.00000, 5.0, 2.729694550490384)]
        public void ValidateMean(double a, double c, double k, double mean)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(mean, n.Mean, highPrecision);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="variance">Expected value.</param>
        [TestCase(4.0, 5.0, 2.0, 0.983559126843161)]
        [TestCase(2.0, 3.5, 4.0, 0.207489170174404)]
        public void ValidateVariance(double a, double c, double k, double variance)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(variance, n.Variance, highPrecision);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="std">Expected value.</param>
        [TestCase(4.0, 5.0, 2.0, 0.991745494995143)]
        [TestCase(2.0, 3.5, 4.0, 0.455509791524182)]
        public void ValidateStandardDeviation(double a, double c, double k, double std)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(std, n.StdDev, highPrecision);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Burr(1.0, 2.0, 2.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Burr(1.0, 2.0, 1.0);
            Assert.AreEqual(double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate entropy throws not supported exception
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        [TestCase(1.0, 1.0, 0.5)]
        public void ValidateEntropyFailsWithNotSupported(double a, double c, double k)
        {
            var n = new Burr(a, c, k);
            Assert.Throws<NotSupportedException>(() => { var x = n.Entropy; });
        }

        /// <summary>
        /// Validate GetMoments.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="n"> order of the moment.</param>
        /// <param name="value">Expected value.</param>
        [TestCase(1.000000, 1.00000, 1.5, 1, 2)]
        [TestCase(4.000000, 5.00000, 0.5, 1, 6.198785110989412)]
        [TestCase(4.000000, 5.00000, 5.0, 1, 2.729694550490384)]
        [TestCase(4.0, 5.0, 2.0, 3, 50.738165747621750)]
        [TestCase(2.0, 3.5, 4.0, 3, 2.895046685294824)]
        public void ValidateMoments(double a, double c, double k, int order, double value)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(value, n.GetMoment(order), lowPrecision);
        }

        /// <summary>
        /// Validate GetMoments throws when given bad parameters
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="order">the order of the moment.</param>
        [TestCase(1.0, 1.0, 0.5, 1)]
        [TestCase(10.0, 1.0, 1.5, 2)]
        [TestCase(5.0, 2, 1.4, 3)]
        public void ValidateGetMomentsFailsWithBadParameters(double a, double c, double k, double order)
        {
            var n = new Burr(a, c, k);
            Assert.That(() => n.GetMoment(order), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="skewness">Expected value.</param>
        [TestCase(4.0, 5.0, 2.0, 0.635277200891842)]
        [TestCase(2.0, 3.5, 4.0, 0.483073007212360)]
        public void ValidateSkewness(double a, double c, double k, double skewness)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(skewness, n.Skewness, lowPrecision);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(1.00000, 1.000000, 0.5, 0.100000, 0.433392086020724)]
        [TestCase(1.000000, 1.000000, 5.0, 0.500000, 0.438957475994512)]
        public void ValidateDensity(double a, double c, double k, double x, double p)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(p, n.Density(x), highPrecision);
            AssertHelpers.AlmostEqualRelative(p, Burr.PDF(a, c, k, x), highPrecision);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(1.000000, 1.000000, 0.5, 0.500000, -1.301344842722192)]
        [TestCase(1.000000, 1.000000, 5.0, 0.500000, -0.823352736214886)]
        [TestCase(4.000000, 5.000000, 1.0, 5.000000, -1.682583872895781)]
        public void ValidateDensityLn(double a, double c, double k, double x, double p)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(p, n.DensityLn(x), highPrecision);
            AssertHelpers.AlmostEqualRelative(p, Burr.PDFLn(a, c, k, x), highPrecision);
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="a">a parameter.</param>
        /// <param name="c">c parameter.</param>
        /// <param name="k">k parameter.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <param name="f">Expected value.</param>
        [TestCase(1.000000, 1.000000, 0.5, 0.500000, 0.183503419072274)]
        [TestCase(4.000000, 5.000000, 0.5, 0.500000, 0.00001525843981653452)]
        [TestCase(4.000000, 5.000000, 0.5, 5.00000, 0.503203804978536)]
        [TestCase(4.000000, 5.000000, 5.0, 5.00000, 0.999084238019496)]
        public void ValidateCumulativeDistribution(double a, double c, double k, double x, double f)
        {
            var n = new Burr(a, c, k);
            AssertHelpers.AlmostEqualRelative(f, n.CumulativeDistribution(x), lowPrecision);
            AssertHelpers.AlmostEqualRelative(f, Burr.CDF(a, c, k, x), lowPrecision);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Burr.Sample(new Numerics.Random.MersenneTwister(100), 1.0, 2.0, 2.0);
        }

        /// <summary>
        /// Can fill array with samples static.
        /// </summary>
        [Test]
        public void CanFillSampleArrayStatic()
        {
            double[] samples = new double[100];
            Burr.Samples(new Numerics.Random.MersenneTwister(100), samples, 1.0, 2.0, 2.0);
            Assert.IsTrue(!samples.Any(x => x == 0));
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Burr.Samples(new Numerics.Random.MersenneTwister(100), 1.0, 2.0, 2.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => { var d = Burr.Sample(new Numerics.Random.MersenneTwister(100), 1.0, -1.0, 2.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Fail filling array with samples with bad parameters static.
        /// </summary>
        [Test]
        public void FailFillingSampleArrayStatic()
        {
            double[] samples = new double[100];
            Assert.That(() => { Burr.Samples(new Numerics.Random.MersenneTwister(100), samples, -1.0, 2.0, 2.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => { var ied = Burr.Samples(new Numerics.Random.MersenneTwister(100), 1.0, -1.0, 2.0).First(); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Burr(1.0, 2.0, 2.0);
            n.Sample();
        }

        /// <summary>
        /// Can fill array with samples.
        /// </summary>
        [Test]
        public void CanFillSampleArray()
        {
            double[] samples = new double[100];
            var n = new Burr(1.0, 2.0, 2.0, new Numerics.Random.MersenneTwister(100));
            n.Samples(samples);
            Assert.IsTrue(!samples.Any(x => x == 0));
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Burr(1.0, 2.0, 2.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }
    }
}
