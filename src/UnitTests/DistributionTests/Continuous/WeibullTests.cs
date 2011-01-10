// <copyright file="WeibullTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using System;
    using System.Linq;
    using Distributions;
    using NUnit.Framework;

    /// <summary>
    /// Weibull distribution tests.
    /// </summary>
    [TestFixture]
    public class WeibullTests
    {
        /// <summary>
        /// Set-up parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create Weibull.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        [Test, Combinatorial]
        public void CanCreateWeibull([Values(1.0, 10.0)] double shape, [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale)
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
        [Test, Sequential]
        public void WeibullCreateFailsWithBadParameters([Values(Double.NaN, 1.0, Double.NaN, 1.0, -1.0, -1.0, 0.0, 0.0, 1.0)] double shape, [Values(1.0, Double.NaN, Double.NaN, -1.0, 1.0, -1.0, 0.0, 1.0, 0.0)] double scale)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Weibull(shape, scale));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Weibull(1.0, 2.0);
            Assert.AreEqual("Weibull(Shape = 1, Scale = 2)", n.ToString());
        }

        /// <summary>
        /// Can set shape.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        [Test]
        public void CanSetShape([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            new Weibull(1.0, 1.0)
            {
                Shape = shape
            };
        }

        /// <summary>
        /// Set shape fails with negative shape.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        [Test]
        public void SetShapeFailsWithNegativeShape([Values(-1.0, -0.0, 0.0)] double shape)
        {
            var n = new Weibull(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Shape = shape);
        }

        /// <summary>
        /// Can set scale.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [Test]
        public void CanSetScale([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale)
        {
            new Weibull(1.0, 1.0)
            {
                Scale = scale
            };
        }

        /// <summary>
        /// Set scale fails with negative scale.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [Test]
        public void SetScaleFailsWithNegativeScale([Values(-1.0, -0.0, 0.0)] double scale)
        {
            var n = new Weibull(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Scale = scale);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="mean">Expected value.</param>
        [Test, Sequential]
        public void ValidateMean(
            [Values(1.0, 1.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 1.0, 10.0, 1.0)] double scale, 
            [Values(0.1, 1.0, 9.5135076986687318362924871772654021925505786260884, 0.95135076986687318362924871772654021925505786260884)] double mean)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(mean, n.Mean, 13);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="var">Expected value.</param>
        [Test, Sequential]
        public void ValidateVariance(
            [Values(1.0, 1.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 1.0, 10.0, 1.0)] double scale, 
            [Values(0.01, 1.0, 1.3100455073468309147154581687505295026863354547057, 0.013100455073468309147154581687505295026863354547057)] double var)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(var, n.Variance, 13);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="sdev">Expected value.</param>
        [Test, Sequential]
        public void ValidateStdDev(
            [Values(1.0, 1.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 1.0, 10.0, 1.0)] double scale, 
            [Values(0.1, 1.0, 1.1445721940300799194124723631014002560036613065794, 0.11445721940300799194124723631014002560036613065794)] double sdev)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(sdev, n.StdDev, 13);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="skewness">Expected value.</param>
        [Test, Sequential]
        public void ValidateSkewness(
            [Values(1.0, 1.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 1.0, 10.0, 1.0)] double scale, 
            [Values(2.0, 2.0, -0.63763713390314440916597757156663888653981696212127, -0.63763713390314440916597757156663888653981696212127)] double skewness)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(skewness, n.Skewness, 11);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="mode">Expected value.</param>
        [Test, Sequential]
        public void ValidateMode(
            [Values(1.0, 1.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 1.0, 10.0, 1.0)] double scale, 
            [Values(0.0, 0.0, 9.8951925820621439264623017041980483215553841533709, 0.98951925820621439264623017041980483215553841533709)] double mode)
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
        [Test, Sequential]
        public void ValidateMedian(
            [Values(1.0, 1.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 1.0, 10.0, 1.0)] double scale, 
            [Values(0.069314718055994530941723212145817656807550013436026, 0.69314718055994530941723212145817656807550013436026, 9.6401223546778973665856033763604752124634905617583, 0.96401223546778973665856033763604752124634905617583)] double median)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(median, n.Median, 13);
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
        [Test, Sequential]
        public void ValidateDensity(
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 0.1, 0.1, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 1.0, 1.0, 1.0)] double scale, 
            [Values(0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0)] double x, 
            [Values(10.0, 0.00045399929762484851535591515560550610237918088866565, 3.7200759760208359629596958038631183373588922923768e-43, 1.0, 0.36787944117144232159552377016146086744581113103177, 0.000045399929762484851535591515560550610237918088866565, 0.0, 9.9999999990000000000499999999983333333333750000000e-10, 0.36787944117144232159552377016146086744581113103177, 0.0, 3.6787944117144232159552377016146086744581113103177, 0.0)] double pdf)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(pdf, n.Density(x), 14);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pdfln">Expected value.</param>
        [Test, Sequential]
        public void ValidateDensityLn(
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 0.1, 0.1, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 1.0, 1.0, 1.0)] double scale, 
            [Values(0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0)] double x, 
            [Values(2.3025850929940456840179914546843642076011014886288, -7.6974149070059543159820085453156357923988985113712, -97.697414907005954315982008545315635792398898511371, 0.0, -1.0, -10.0, Double.NegativeInfinity, -20.723265837046411156161923092159277868409913397659, -1.0, Double.NegativeInfinity, 1.3025850929940456840179914546843642076011014886288, -9.999999976974149070059543159820085453156357923988985113712e9)] double pdfln)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(pdfln, n.DensityLn(x), 14);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Weibull.Sample(new Random(), 1.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Weibull.Samples(new Random(), 1.0, 1.0);
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Normal.Sample(new Random(), 1.0, -1.0));
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Normal.Samples(new Random(), 1.0, -1.0).First());
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
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0)] double shape, 
            [Values(0.1, 0.1, 0.1, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 1.0, 1.0, 1.0)] double scale, 
            [Values(0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0)] double x, 
            [Values(0.0, 0.99995460007023751514846440848443944938976208191113, 0.99999999999999999999999999999999999999999996279924, 0.0, 0.63212055882855767840447622983853913255418886896823, 0.99995460007023751514846440848443944938976208191113, 0.0, 9.9999999995000000000166666666662500000000083333333e-11, 0.63212055882855767840447622983853913255418886896823, 0.0, 0.63212055882855767840447622983853913255418886896823, 1.0)] double cdf)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(cdf, n.CumulativeDistribution(x), 15);
        }
    }
}
