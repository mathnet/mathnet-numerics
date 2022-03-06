// <copyright file="LogisticTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.Tests.DistributionTests.Continuous
{
    using Random = System.Random;

    /// <summary>
    /// Logistic distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class LogisticTests
    {
        /// <summary>
        /// Can create standard logistic.
        /// </summary>
        [Test]
        public void CanCreateStandardLogistic()
        {
            var l = new Logistic();
            Assert.AreEqual(0.0, l.Mean);
            Assert.AreEqual(1.0, l.Scale);
        }

        /// <summary>
        /// Can create logistic.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="scale">Scale parameter value.</param>
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, double.PositiveInfinity)]
        public void CanCreateLogistic(double mean, double scale)
        {
            var n = new Logistic(mean, scale);
            Assert.AreEqual(mean, n.Mean);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// Logistic create fails with bad parameters.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="scale">Scale parameter value.</param>
        [TestCase(double.NaN, 1.0)]
        [TestCase(1.0, double.NaN)]
        [TestCase(double.NaN, double.NaN)]
        [TestCase(1.0, -1.0)]
        public void LogisticCreateFailsWithBadParameters(double mean, double scale)
        {
            Assert.That(() => new Logistic(mean, scale), Throws.ArgumentException);
        }

            /// <summary>
        /// Can create logistic from mean and scale parameter value.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="scale">Scale parameter value.</param>
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, double.PositiveInfinity)]
        public void CanCreateLogisticFromMeanAndScale(double mean, double scale)
        {
            var n = Logistic.WithMeanScale(mean, scale);
            Assert.AreEqual(mean, n.Mean);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// Can create logistic from mean and standard deviation.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="sdev">Standard deviation value.</param>
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, double.PositiveInfinity)]
        public void CanCreateLogisticFromMeanAndStdDev(double mean, double sdev)
        {
            var n = Logistic.WithMeanStdDev(mean, sdev);
            Assert.AreEqual(mean, n.Mean);
            Assert.AreEqual(sdev, n.StdDev);
        }

        /// <summary>
        /// Can create logistic from mean and variance.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="var">Variance value.</param>
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, double.PositiveInfinity)]
        public void CanCreateLogisticFromMeanAndVariance(double mean, double var)
        {
            var n = Logistic.WithMeanVariance(mean, var);
            AssertHelpers.AlmostEqualRelative(mean, n.Mean, 15);
            AssertHelpers.AlmostEqualRelative(var, n.Variance, 15);
        }

        /// <summary>
        /// Can create logistic from mean and precision.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="prec">Precision value.</param>
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        public void CanCreateLogisticFromMeanAndPrecision(double mean, double prec)
        {
            var n = Logistic.WithMeanPrecision(mean, prec);
            AssertHelpers.AlmostEqualRelative(mean, n.Mean, 15);
            AssertHelpers.AlmostEqualRelative(prec, n.Precision, 15);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var n = new Logistic(1d, 2d);
            Assert.AreEqual("Logistic(μ = 1, s = 2)", n.ToString());
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="scale">Scale parameter value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(double.PositiveInfinity)]
        public void ValidateEntropy(double scale)
        {
            var n = new Logistic(1.0, scale);
            Assert.AreEqual(Math.Log(scale) + 2, n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="scale">Scale parameter value.</param>
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(double.PositiveInfinity)]
        public void ValidateSkewness(double scale)
        {
            var n = new Logistic(1.0, scale);
            Assert.AreEqual(0.0, n.Skewness);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        [TestCase(double.NegativeInfinity)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(double.PositiveInfinity)]
        public void ValidateMode(double mean)
        {
            var n = new Logistic(mean, 1.0);
            Assert.AreEqual(mean, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        [TestCase(double.NegativeInfinity)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(double.PositiveInfinity)]
        public void ValidateMedian(double mean)
        {
            var n = new Logistic(mean, 1.0);
            Assert.AreEqual(mean, n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Logistic();
            Assert.AreEqual(double.NegativeInfinity, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Logistic();
            Assert.AreEqual(double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Logistic.Sample(new Random(0), 0.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Logistic.Samples(new Random(0), 0.0, 1.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => { var d = Logistic.Sample(new Random(0), 0.0, -1.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => { var ied = Logistic.Samples(new Random(0), 0.0, -1.0).First(); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Logistic();
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Logistic();
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="d">Expected value.</param>
        [TestCase(double.NegativeInfinity, double.NaN)]
        [TestCase(-5.0, 0.00332402833539508)]
        [TestCase(-2.0, 0.01422651193986778)]
        [TestCase(0.0, 0.03505185827255409)]
        [TestCase(4.0, 0.11750185610079725)]
        [TestCase(5.0, 0.12500000000000000)]
        [TestCase(6.0, 0.11750185610079725)]
        [TestCase(10.0, 0.03505185827255409)]
        [TestCase(double.PositiveInfinity, 0)]
        public void ValidateDensity(double x, double d)
        {
            var n = Logistic.WithMeanScale(5.0, 2.0);
            AssertHelpers.AlmostEqualRelative(d, n.Density(x), 9);
            AssertHelpers.AlmostEqualRelative(d, Logistic.PDF(5.0, 2.0, x), 9);
        }

                /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="d">Expected value.</param>
        [TestCase(double.NegativeInfinity, double.NaN)]
        [TestCase(-5.0, -5.70657787753818)]
        [TestCase(-2.0, -4.25264801710519)]
        [TestCase(0.0, -3.35092664914504)]
        [TestCase(4.0, -2.14130114892016)]
        [TestCase(5.0, -2.07944154167984)]
        [TestCase(6.0, -2.14130114892016)]
        [TestCase(10.0, -3.35092664914504)]
        [TestCase(double.PositiveInfinity, double.NegativeInfinity)]
        public void ValidateLogDensity(double x, double d)
        {
            var n = Logistic.WithMeanScale(5.0, 2.0);
            AssertHelpers.AlmostEqualRelative(d, n.DensityLn(x), 9);
            AssertHelpers.AlmostEqualRelative(d, Logistic.PDFLn(5.0, 2.0, x), 9);
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(double.NegativeInfinity, 0.0)]
        [TestCase(-5.0, 0.00669285092428486)]
        [TestCase(-2.0, 0.0293122307513563)]
        [TestCase(0.0, 0.0758581800212435)]
        [TestCase(4.0, 0.377540668798145)]
        [TestCase(5.0, 0.5)]
        [TestCase(6.0, 0.622459331201855)]
        [TestCase(10.0, 0.924141819978757)]
        [TestCase(double.PositiveInfinity, 1.0)]
        public void ValidateCumulativeDistribution(double x, double p)
        {
            var n = Logistic.WithMeanScale(5.0, 2.0);
            AssertHelpers.AlmostEqualRelative(p, n.CumulativeDistribution(x), 9);
            AssertHelpers.AlmostEqualRelative(p, Logistic.CDF(5.0, 2.0, x), 9);
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(double.NegativeInfinity, 0.0)]
        [TestCase(-5.0, 0.00669285092428486)]
        [TestCase(-2.0, 0.0293122307513563)]
        [TestCase(0.0, 0.0758581800212435)]
        [TestCase(4.0, 0.377540668798145)]
        [TestCase(5.0, 0.5)]
        [TestCase(6.0, 0.622459331201855)]
        [TestCase(10.0, 0.924141819978757)]
        [TestCase(double.PositiveInfinity, 1.0)]
        public void ValidateInverseCumulativeDistribution(double x, double p)
        {
            var n = Logistic.WithMeanScale(5.0, 2.0);
            AssertHelpers.AlmostEqualRelative(x, n.InverseCumulativeDistribution(p), 14);
            AssertHelpers.AlmostEqualRelative(x, Logistic.InvCDF(5.0, 2.0, p), 14);
        }

    }
}
