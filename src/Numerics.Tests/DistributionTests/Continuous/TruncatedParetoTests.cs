// <copyright file="TruncatedParetoTests.cs" company="Math.NET">
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
    /// <c>TruncatedPareto</c> distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class TruncatedParetoTests
    {
        private const int highPrecision = 12;
        private const int lowPrecision = 6;
        private const double highTruncation = 1E8;

        /// <summary>
        /// Can create <c>TruncatedPareto</c>.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// /// <param name="truncation">Truncation parameter.</param>
        [TestCase(2.0, 2.0, 500.0)]
        [TestCase(10.0, 2.0, 1000.0)]
        [TestCase(100.0, 4.0, 10000.0)]
        [TestCase(3.0, 10.0, 5.0)]
        [TestCase(10.0, 100.0, 20.0)]
        public void CanCreateTruncatedPareto(double scale, double shape, double truncation)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            Assert.AreEqual(scale, n.Scale, highPrecision);
            Assert.AreEqual(shape, n.Shape, highPrecision);
            Assert.AreEqual(truncation, n.Truncation, highPrecision);
        }

        /// <summary>
        /// Can create <c>TruncatedPareto</c> with random source.
        /// </summary>
        [Test]
        public void CanCreateTruncatedParetoWithRandomSource()
        {
            var randomSource = new Numerics.Random.MersenneTwister(100);
            var n = new TruncatedPareto(10.0, 10.0, 1000, randomSource);
            Assert.AreEqual(randomSource, n.RandomSource);
        }

        /// <summary>
        /// <c>TruncatedPareto</c> create fails with bad parameters.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// /// <param name="truncation">Truncation parameter.</param>
        [TestCase(1.0, -1.0, 15.0)]
        [TestCase(1.0, 3.0, 0.5)]
        [TestCase(-1.0, 2.0, 15.0)]
        [TestCase(double.NaN, 1.0, 1.0)]
        [TestCase(1.0, double.PositiveInfinity, 0.0)]
        [TestCase(5.0, 2.0, double.PositiveInfinity)]
        public void TruncatedParetoCreateFailsWithBadParameters(double scale, double shape, double truncation)
        {
            Assert.That(() => new TruncatedPareto(scale, shape, truncation), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate IsValidParameterSet.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// /// <param name="truncation">Truncation parameter.</param>
        [TestCase(2.0, 2.0, 500, true)]
        [TestCase(10.0, 10.0, 1000, true)]
        [TestCase(5.0, 3.0, 1, false)]
        public void ValidateIsValidParameterSet(double scale, double shape, double truncation, bool validity)
        {
            Assert.AreEqual(TruncatedPareto.IsValidParameterSet(scale, shape, truncation), validity);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var n = new TruncatedPareto(1d, 2d, 100d);
            Assert.AreEqual("Truncated Pareto(Scale = 1, Shape = 2, Truncation = 100)", n.ToString());
        }

        /// <summary>
        /// Validate mode throws not supported exception
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        [TestCase(10.0, 10.0, highTruncation)]
        public void ValidateModeFailsWithNotSupported(double scale, double shape, double truncation)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            Assert.Throws<NotSupportedException>(() => { var x = n.Mode; });
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="median">Expected value.</param>
        [TestCase(10.0, 10.0, highTruncation, 10.717734625362931)]
        [TestCase(100, 3.5, 10000, 1.219013619375516e+02)]
        [TestCase(1000, 5.5, 100000, 1.134312522193400e+03)]
        public void ValidateMedian(double scale, double shape, double truncation, double median)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(median, n.Median, highPrecision);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="std">Expected value.</param>
        [TestCase(10.0, 10.0, highTruncation, 11.111111111111111)]
        [TestCase(100, 3.5, 10000, 1.399986139998614e+02)]
        public void ValidateMean(double scale, double shape, double truncation, double mean)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(mean, n.Mean, highPrecision);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="variance">Expected value.</param>
        [TestCase(10.0, 10.0, highTruncation, 1.543209876543210)]
        [TestCase(100, 3.5, 10000, 3.710390409118045e+03)]
        [TestCase(1000, 5.5, 100000, 7.760125676537910e+04)]
        public void ValidateVariance(double scale, double shape, double truncation, double variance)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(variance, n.Variance, highPrecision);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="std">Expected value.</param>
        [TestCase(10.0, 10.0, highTruncation, 1.242259987499883)]
        [TestCase(100, 3.5, 10000, 60.912974062329653)]
        [TestCase(1000, 5.5, 100000, 2.785700212969427e+02)]
        public void ValidateStandardDeviation(double scale, double shape, double truncation, double std)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(std, n.StdDev, highPrecision);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new TruncatedPareto(1.0, 2.0, 500.0);
            Assert.AreEqual(1.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new TruncatedPareto(1.0, 2.0, 500.0);
            Assert.AreEqual(500.0, n.Maximum);
        }

        /// <summary>
        /// Validate entropy throws not supported exception
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        [TestCase(100, 3.5, 10000)]
        public void ValidateEntropyFailsWithNotSupported(double scale, double shape, double truncation)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            Assert.Throws<NotSupportedException>(() => { var x = n.Entropy; });
        }

        /// <summary>
        /// Validate GetMoments.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="n"> order of the moment.</param>
        /// <param name="value">Expected value.</param>
        [TestCase(10.0, 10.0, highTruncation, 1, 11.111111111111111)]
        [TestCase(1.0, 6.0, 10000, 1, 1.2)]
        [TestCase(100, 3.5, 10000, 1, 1.399986139998614e+02)]
        [TestCase(100, 3.5, 10000, 2, 2.331000233100023e+04)]
        [TestCase(100, 3.5, 10000, 3, 6.300000630000063e+06)]
        [TestCase(1000, 5.5, 100000, 1, 1.222222221012222e+03)]
        [TestCase(1000, 5.5, 100000, 2, 1.571428414301428e+06)]
        [TestCase(1000, 5.5, 100000, 4, 3.663000000036630e+12)]
        [TestCase(1000, 2.0, 100000, 2, 9.211261498125996e+06)]
        [TestCase(1000, 3.0, 100000, 3, 1.381552437348865e+10)]
        public void ValidateMoments(double scale, double shape, double truncation, int order, double value)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(value, n.GetMoment(order), highPrecision);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="skewness">Expected value.</param>
        [TestCase(10.0, 10.0, highTruncation, 2.8110568859997356)]
        public void ValidateSkewness(double scale, double shape, double truncation, double skewness)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(skewness, n.Skewness, highPrecision);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(1, 1, highTruncation, 1, 1)]
        [TestCase(1, 1, highTruncation, 1.5, 4 / 9.0)]
        [TestCase(1, 1, highTruncation, 5, 1 / 25.0)]
        [TestCase(1, 1, highTruncation, 50, 1 / 2500.0)]
        [TestCase(1, 4, highTruncation, 1, 4)]
        [TestCase(1, 4, highTruncation, 1.5, 128 / 243.0)]
        [TestCase(1, 4, highTruncation, 50, 1 / 78125000.0)]
        [TestCase(3, 2, highTruncation, 3, 2 / 3.0)]
        [TestCase(3, 2, highTruncation, 5, 18 / 125.0)]
        [TestCase(25, 100, highTruncation, 50, 1.5777218104420236e-30)]
        [TestCase(100, 25, highTruncation, 150, 6.6003546737276816e-6)]
        [TestCase(100, 3.5, 10000, 1000, 1.106797291738662e-06)]
        [TestCase(100, 3.5, 10000, 2000, 4.891399189920708e-08)]
        [TestCase(1000, 5.5, 100000, 2000, 6.076698900882660e-05)]
        public void ValidateDensity(double scale, double shape, double truncation, double x, double p)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(p, n.Density(x), lowPrecision);
            AssertHelpers.AlmostEqualRelative(p, TruncatedPareto.PDF(scale, shape, truncation, x), lowPrecision);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(3, 2, highTruncation, 3, -0.405465108108164)]
        [TestCase(100, 3.5, 10000, 1000, -13.714040035965924)]
        [TestCase(100, 3.5, 10000, 2000, -16.833202348485678)]
        public void ValidateDensityLn(double scale, double shape, double truncation, double x, double p)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(p, n.DensityLn(x), lowPrecision);
            AssertHelpers.AlmostEqualRelative(p, TruncatedPareto.PDFLn(scale, shape, truncation, x), lowPrecision);
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(1.0, 1.0, highTruncation, 1.0, 0)]
        [TestCase(7.0, 7.0, highTruncation, 10.0, 0.9176457)]
        [TestCase(10.0, 10.0, highTruncation, 12.0, 0.83849441711015427)]
        [TestCase(100, 3.5, 10000, 102, 0.066961862452387)]
        [TestCase(100, 3.5, 10000, 1000, 0.999683872202370)]
        [TestCase(100, 3.5, 10000, 2000, 0.999972149147496)]
        [TestCase(1000, 5.5, 100000, 2000, 0.977902913097699)]
        public void ValidateCumulativeDistribution(double scale, double shape, double truncation, double x, double expected)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(expected, n.CumulativeDistribution(x), highPrecision);
            AssertHelpers.AlmostEqualRelative(expected, TruncatedPareto.CDF(scale, shape, truncation, x), highPrecision);
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="scale">Scale parameter.</param>
        /// <param name="shape">Shape parameter.</param>
        /// <param name="truncation">Truncation parameter.</param>
        /// <param name="p">The location at which to compute the inverse cumulative distribution function.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(1.0, 1.0, highTruncation, 0, 1.0)]
        [TestCase(7.0, 7.0, highTruncation, 0.9176457, 10.0)]
        [TestCase(10.0, 10.0, highTruncation, 0.83849441711015427, 12.0)]
        [TestCase(100, 3.5, 10000, 0.066961862452387, 102)]
        [TestCase(100, 3.5, 10000, 0.999683872202370, 1000)]
        [TestCase(100, 3.5, 10000, 0.999972149147496, 2000)]
        [TestCase(1000, 5.5, 100000, 0.977902913097699, 2000)]
        public void ValidateInverseCumulativeDistribution(double scale, double shape, double truncation, double p, double expected)
        {
            var n = new TruncatedPareto(scale, shape, truncation);
            AssertHelpers.AlmostEqualRelative(expected, n.InvCDF(p), lowPrecision);
            AssertHelpers.AlmostEqualRelative(expected, TruncatedPareto.InvCDF(scale, shape, truncation, p), lowPrecision);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            TruncatedPareto.Sample(new Numerics.Random.MersenneTwister(100), 10.0, 10.0, 1000.0);
        }

        /// <summary>
        /// Can fill array with samples static.
        /// </summary>
        [Test]
        public void CanFillSampleArrayStatic()
        {
            double[] samples = new double[100];
            TruncatedPareto.Samples(new Numerics.Random.MersenneTwister(100), samples, 10.0, 10.0, 1000.0);
            Assert.IsTrue(!samples.Any(x => x == 0));
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = TruncatedPareto.Samples(new Numerics.Random.MersenneTwister(100), 10.0, 10.0, 1000.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => { var d = TruncatedPareto.Sample(new Numerics.Random.MersenneTwister(100), 10.0, 10.0, 5.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Fail filling array with samples with bad parameters static.
        /// </summary>
        [Test]
        public void FailFillingSampleArrayStatic()
        {
            double[] samples = new double[100];
            Assert.That(() => { TruncatedPareto.Samples(new Numerics.Random.MersenneTwister(100), samples, 10.0, 10.0, 5.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => { var ied = TruncatedPareto.Samples(new Numerics.Random.MersenneTwister(100), 10.0, 10.0, 5.0).First(); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new TruncatedPareto(10.0, 10.0, 1000.0);
            n.Sample();
        }

        /// <summary>
        /// Can fill array with samples.
        /// </summary>
        [Test]
        public void CanFillSampleArray()
        {
            double[] samples = new double[100];
            var n = new TruncatedPareto(10.0, 10.0, 1000.0);
            n.Samples(samples);
            Assert.IsTrue(!samples.Any(x => x == 0));
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new TruncatedPareto(10.0, 10.0, 1000.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }
    }
}
