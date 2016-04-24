// <copyright file="WeibullTests.cs" company="Math.NET">
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
    using Random = System.Random;

    /// <summary>
    /// Weibull distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class WeibullTests
    {
        /// <summary>
        /// Can create Weibull.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(1.0, 0.1)]
        [TestCase(10.0, 1.0)]
        [TestCase(11.0, 10.0)]
        [TestCase(12.0, Double.PositiveInfinity)]
        public void CanCreateWeibull(double shape, double scale)
        {
            var n = new Weibull(shape, scale);
            Assert.AreEqual(shape, n.Shape);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// Weibull create fails with bad parameters.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(Double.NaN, 1.0)]
        [TestCase(1.0, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN)]
        [TestCase(1.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        [TestCase(-1.0, -1.0)]
        [TestCase(0.0, 0.0)]
        [TestCase(0.0, 1.0)]
        [TestCase(1.0, 0.0)]
        public void WeibullCreateFailsWithBadParameters(double shape, double scale)
        {
            Assert.That(() => new Weibull(shape, scale), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Weibull(1d, 2d);
            Assert.AreEqual("Weibull(k = 1, λ = 2)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="mean">Expected value.</param>
        [TestCase(1.0, 0.1, 0.1)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 10.0, 9.5135076986687318362924871772654021925505786260884)]
        [TestCase(10.0, 1.0, 0.95135076986687318362924871772654021925505786260884)]
        public void ValidateMean(double shape, double scale, double mean)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqualRelative(mean, n.Mean, 13);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="var">Expected value.</param>
        [TestCase(1.0, 0.1, 0.01)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 10.0, 1.3100455073468309147154581687505295026863354547057)]
        [TestCase(10.0, 1.0, 0.013100455073468309147154581687505295026863354547057)]
        public void ValidateVariance(double shape, double scale, double var)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqualRelative(var, n.Variance, 12);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="sdev">Expected value.</param>
        [TestCase(1.0, 0.1, 0.1)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 10.0, 1.1445721940300799194124723631014002560036613065794)]
        [TestCase(10.0, 1.0, 0.11445721940300799194124723631014002560036613065794)]
        public void ValidateStdDev(double shape, double scale, double sdev)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqualRelative(sdev, n.StdDev, 12);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="skewness">Expected value.</param>
        [TestCase(1.0, 0.1, 2.0)]
        [TestCase(1.0, 1.0, 2.0)]
        [TestCase(10.0, 10.0, -0.63763713390314440916597757156663888653981696212127)]
        [TestCase(10.0, 1.0, -0.63763713390314440916597757156663888653981696212127)]
        public void ValidateSkewness(double shape, double scale, double skewness)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqualRelative(skewness, n.Skewness, 10);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="mode">Expected value.</param>
        [TestCase(1.0, 0.1, 0.0)]
        [TestCase(1.0, 1.0, 0.0)]
        [TestCase(10.0, 10.0, 9.8951925820621439264623017041980483215553841533709)]
        [TestCase(10.0, 1.0, 0.98951925820621439264623017041980483215553841533709)]
        public void ValidateMode(double shape, double scale, double mode)
        {
            var n = new Weibull(shape, scale);
            Assert.AreEqual(mode, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="median">Expected value.</param>
        [TestCase(1.0, 0.1, 0.069314718055994530941723212145817656807550013436026)]
        [TestCase(1.0, 1.0, 0.69314718055994530941723212145817656807550013436026)]
        [TestCase(10.0, 10.0, 9.6401223546778973665856033763604752124634905617583)]
        [TestCase(10.0, 1.0, 0.96401223546778973665856033763604752124634905617583)]
        public void ValidateMedian(double shape, double scale, double median)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqualRelative(median, n.Median, 13);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Weibull(1.0, 1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Weibull(1.0, 1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pdf">Expected value.</param>
        [TestCase(1.0, 0.1, 0.0, 10.0)]
        [TestCase(1.0, 0.1, 1.0, 0.00045399929762484851535591515560550610237918088866565)]
        [TestCase(1.0, 0.1, 10.0, 3.7200759760208359629596958038631183373588922923768e-43)]
        [TestCase(1.0, 1.0, 0.0, 1.0)]
        [TestCase(1.0, 1.0, 1.0, 0.36787944117144232159552377016146086744581113103177)]
        [TestCase(1.0, 1.0, 10.0, 0.000045399929762484851535591515560550610237918088866565)]
        [TestCase(10.0, 10.0, 0.0, 0.0)]
        [TestCase(10.0, 10.0, 1.0, 9.9999999990000000000499999999983333333333750000000e-10)]
        [TestCase(10.0, 10.0, 10.0, 0.36787944117144232159552377016146086744581113103177)]
        [TestCase(10.0, 1.0, 0.0, 0.0)]
        [TestCase(10.0, 1.0, 1.0, 3.6787944117144232159552377016146086744581113103177)]
        [TestCase(10.0, 1.0, 10.0, 0.0)]
        public void ValidateDensity(double shape, double scale, double x, double pdf)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqualRelative(pdf, n.Density(x), 13);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pdfln">Expected value.</param>
        [TestCase(1.0, 0.1, 0.0, 2.3025850929940456840179914546843642076011014886288)]
        [TestCase(1.0, 0.1, 1.0, -7.6974149070059543159820085453156357923988985113712)]
        [TestCase(1.0, 0.1, 10.0, -97.697414907005954315982008545315635792398898511371)]
        [TestCase(1.0, 1.0, 0.0, 0.0)]
        [TestCase(1.0, 1.0, 1.0, -1.0)]
        [TestCase(1.0, 1.0, 10.0, -10.0)]
        [TestCase(10.0, 10.0, 0.0, Double.NegativeInfinity)]
        [TestCase(10.0, 10.0, 1.0, -20.723265837046411156161923092159277868409913397659)]
        [TestCase(10.0, 10.0, 10.0, -1.0)]
        [TestCase(10.0, 1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(10.0, 1.0, 1.0, 1.3025850929940456840179914546843642076011014886288)]
        [TestCase(10.0, 1.0, 10.0, -9.999999976974149070059543159820085453156357923988985113712e9)]
        public void ValidateDensityLn(double shape, double scale, double x, double pdfln)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqualRelative(pdfln, n.DensityLn(x), 14);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Weibull.Sample(new Random(0), 1.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Weibull.Samples(new Random(0), 1.0, 1.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => Normal.Sample(new Random(0), 1.0, -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => Normal.Samples(new Random(0), 1.0, -1.0).First(), Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Normal();
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Normal();
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [TestCase(1.0, 0.1, 0.0, 0.0)]
        [TestCase(1.0, 0.1, 1.0, 0.99995460007023751514846440848443944938976208191113)]
        [TestCase(1.0, 0.1, 10.0, 0.99999999999999999999999999999999999999999996279924)]
        [TestCase(1.0, 1.0, 0.0, 0.0)]
        [TestCase(1.0, 1.0, 1.0, 0.63212055882855767840447622983853913255418886896823)]
        [TestCase(1.0, 1.0, 10.0, 0.99995460007023751514846440848443944938976208191113)]
        [TestCase(10.0, 10.0, 0.0, 0.0)]
        [TestCase(10.0, 10.0, 1.0, 9.9999999995000000000166666666662500000000083333333e-11)]
        [TestCase(10.0, 10.0, 10.0, 0.63212055882855767840447622983853913255418886896823)]
        [TestCase(10.0, 1.0, 0.0, 0.0)]
        [TestCase(10.0, 1.0, 1.0, 0.63212055882855767840447622983853913255418886896823)]
        [TestCase(10.0, 1.0, 10.0, 1.0)]
        public void ValidateCumulativeDistribution(double shape, double scale, double x, double cdf)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqualRelative(cdf, n.CumulativeDistribution(x), 14);
        }

        /// <summary>
        /// Can estimate distribution parameters.
        /// </summary>
        [TestCase(1.0, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(10.0, 50.0)]
        public void CanEstimateParameters(double shape, double scale)
        {
            var original = new Weibull(shape, scale, new Random(100));
            var estimated = Weibull.Estimate(original.Samples().Take(10000));

            AssertHelpers.AlmostEqualRelative(shape, estimated.Shape, 1);
            AssertHelpers.AlmostEqualRelative(scale, estimated.Scale, 1);
        }
    }
}
