// <copyright file="InverseGaussianTests.cs" company="Math.NET">
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
    /// <c>InverseGaussian</c> distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class InverseGaussianTests
    {
        private const int precision = 12;

        /// <summary>
        /// Can create <c>Inverse Gaussian</c>.
        /// </summary>
        /// <param name="mu">Mu value.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        public void CanCreateInverseGaussian(double mu, double lambda)
        {
            var n = new InverseGaussian(mu, lambda);
            Assert.AreEqual(mu, n.Mu);
            Assert.AreEqual(lambda, n.Lambda);
        }

        /// <summary>
        /// Can create <c>Inverse Gaussian</c> with random source.
        /// </summary>
        [Test]
        public void CanCreateInverseGaussianWithRandomSource()
        {
            var randomSource = new Numerics.Random.MersenneTwister(100);
            var n = new InverseGaussian(1.0, 1.0, randomSource);
            Assert.AreEqual(randomSource, n.RandomSource);
        }

        /// <summary>
        /// <c>InverseGaussian</c> create fails with bad parameters.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        [TestCase(double.NaN, 1.0)]
        [TestCase(1.0, double.NaN)]
        [TestCase(double.NaN, double.NaN)]
        [TestCase(-1.0, -1.0)]
        public void InverseGaussianCreateFailsWithBadParameters(double mu, double lambda)
        {
            Assert.That(() => new LogNormal(mu, lambda), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate IsValidParameterSet.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        /// <param name="validity">exoected validity of paremeter set.</param>
        [TestCase(1.0, 1.0, true)]
        [TestCase(10.0, 10.0, true)]
        [TestCase(-1.0, -1.0, false)]
        public void ValidateIsValidParameterSet(double mu, double lambda, bool validity)
        {
            Assert.AreEqual(InverseGaussian.IsValidParameterSet(mu, lambda), validity);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var n = new InverseGaussian(1d, 2d);
            Assert.AreEqual("InverseGaussian(μ = 1, λ = 2)", n.ToString());
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        /// <param name="mode">Expected value.</param>
        [TestCase(0.100000, 0.100000, 0.030277563773199)]
        [TestCase(1.500000, 5.500000, 1.007026953273370)]
        [TestCase(2.500000, 1.500000, 0.481456008918130)]
        [TestCase(5.500000, 2.500000, 0.815033614523337)]
        [TestCase(5.500000, 5.500000, 1.665266007525970)]
        public void ValidateMode(double mu, double lambda, double mode)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(mode, n.Mode, precision);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        /// <param name="median">Expected value.</param>
        [TestCase(0.100000, 0.100000, 0.067584130569524)]
        [TestCase(1.500000, 0.100000, 0.190369896722453)]
        [TestCase(2.500000, 0.100000, 0.201110669500521)]
        [TestCase(5.500000, 5.500000, 3.717127181323815)]
        public void ValidateMedian(double mu, double lambda, double median)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(median, n.Median, precision);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        /// <param name="mean">Expected value.</param>
        [TestCase(0.100000, 5.500000, 0.1)]
        [TestCase(1.500000, 0.100000, 1.5)]
        [TestCase(2.500000, 5.500000, 2.5)]
        [TestCase(5.500000, 0.100000, 5.5)]
        public void ValidateMean(double mu, double lambda, double mean)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(mean, n.Mean, precision);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        /// <param name="variance">Expected value.</param>
        [TestCase(0.100000, 5.500000, 1.818181818181819e-04)]
        [TestCase(1.500000, 0.100000, 33.75)]
        public void ValidateVariance(double mu, double lambda, double variance)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(variance, n.Variance, precision);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        /// <param name="std">Expected value.</param>
        [TestCase(2.500000, 5.500000, 1.685499656158105)]
        [TestCase(5.500000, 0.100000, 40.789091679026143)]
        public void ValidateStandardDeviation(double mu, double lambda, double std)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(std, n.StdDev, precision);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new InverseGaussian(1.0, 2.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new InverseGaussian(1.0, 2.0);
            Assert.AreEqual(double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate entropy throws not supported exception
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        [TestCase(1.0, 1.0)]
        public void ValidateEntropyFailsWithNotSupported(double mu, double lambda)
        {
            var n = new InverseGaussian(mu, lambda);
            Assert.Throws<NotSupportedException>(() => { var x = n.Entropy; });
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="´lambda">Lambda  parameter.</param>
        /// <param name="skewness">Expected value.</param>
        [TestCase(0.100000, 1.500000, 0.774596669241483)]
        [TestCase(1.500000, 2.500000, 2.323790007724450)]
        [TestCase(1.500000, 5.500000, 1.566698903601281)]
        [TestCase(2.500000, 0.100000, 15.0)]
        [TestCase(2.500000, 1.500000, 3.872983346207417)]
        [TestCase(5.500000, 2.500000, 4.449719092257398)]
        [TestCase(5.500000, 5.500000, 3.0)]
        public void ValidateSkewness(double mu, double lambda, double skewness)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(skewness, n.Skewness, precision);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Lambda  parameter.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(1.500000, 2.500000, 0.500000, 0.587321148416470)]
        [TestCase(1.500000, 2.500000, 0.800000, 0.627284170789435)]
        [TestCase(2.500000, 2.500000, 0.500000, 0.360208446721537)]
        [TestCase(2.500000, 2.500000, 0.800000, 0.428023216678204)]
        public void ValidateDensity(double mu, double lambda, double x, double p)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(p, n.Density(x), precision);
            AssertHelpers.AlmostEqualRelative(p, InverseGaussian.PDF(mu, lambda, x), precision);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Sigma value.</param>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(1.500000, 0.100000, 0.100000, 0.948091004233817)]
        [TestCase(1.500000, 1.500000, 0.800000, -0.585657318845943)]
        [TestCase(2.500000, 0.100000, 0.800000, -1.764415752730381)]
        [TestCase(2.500000, 1.500000, 0.100000, -4.174328339659523)]
        [TestCase(2.500000, 1.500000, 0.500000, -0.636485208310673)]
        public void ValidateDensityLn(double mu, double lambda, double x, double p)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(p, n.DensityLn(x), precision);
            AssertHelpers.AlmostEqualRelative(p, InverseGaussian.PDFLn(mu, lambda, x), precision);
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Sigma value.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <param name="f">Expected value.</param>
        [TestCase(1.500000, 2.500000, 0.100000, 0.000002882116410867029)]
        [TestCase(2.500000, 1.500000, 0.100000, 0.0001938001952257318)]
        [TestCase(2.500000, 1.500000, 0.500000, 0.145457623130791)]
        [TestCase(2.500000, 2.500000, 0.100000, 0.000001529605202470422)]
        [TestCase(2.500000, 2.500000, 0.800000, 0.187168922781367)]
        public void ValidateCumulativeDistribution(double mu, double lambda, double x, double f)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(f, n.CumulativeDistribution(x), precision);
            AssertHelpers.AlmostEqualRelative(f, InverseGaussian.CDF(mu, lambda, x), precision);
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="mu">Mu parameter.</param>
        /// <param name="lambda">Sigma value.</param>
        /// <param name="probability">The location at which to compute the inverse cumulative distribution function.</param>
        /// <param name="f">Expected value.</param>
        [TestCase(1.500000, 1.500000, 0.100000, 0.356437063090717)]
        [TestCase(1.500000, 1.500000, 0.500000, 1.013761958542859)]
        [TestCase(2.500000, 2.500000, 0.500000, 1.689603264238098)]
        [TestCase(2.500000, 2.500000, 0.800000, 3.619719792074686)]
        public void ValidateInverseCumulativeDistribution(double mu, double lambda, double probability, double f)
        {
            var n = new InverseGaussian(mu, lambda);
            AssertHelpers.AlmostEqualRelative(f, InverseGaussian.InvCDF(mu, lambda, probability), precision);
            AssertHelpers.AlmostEqualRelative(f, n.InvCDF(probability), precision);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            InverseGaussian.Sample(new Numerics.Random.MersenneTwister(100), 1.0, 1.0);
        }

        /// <summary>
        /// Can fill array with samples static.
        /// </summary>
        [Test]
        public void CanFillSampleArrayStatic()
        {
            double[] samples = new double[100];
            InverseGaussian.Samples(new Numerics.Random.MersenneTwister(100), samples, 1.0, 1.0);
            Assert.IsTrue(!samples.Any(x => x == 0));
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = InverseGaussian.Samples(new Numerics.Random.MersenneTwister(100), 1.0, 1.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => { var d = InverseGaussian.Sample(new Numerics.Random.MersenneTwister(100), 0.0, -1.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Fail filling array with samples with bad parameters static.
        /// </summary>
        [Test]
        public void FailFillingSampleArrayStatic()
        {
            double[] samples = new double[100];
            Assert.That(() => { InverseGaussian.Samples(new Numerics.Random.MersenneTwister(100), samples, -1.0, 1.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => { var ied = InverseGaussian.Samples(new Numerics.Random.MersenneTwister(100), 0.0, -1.0).First(); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new InverseGaussian(1.0, 2.0);
            n.Sample();
        }

        /// <summary>
        /// Can fill array with samples.
        /// </summary>
        [Test]
        public void CanFillSampleArray()
        {
            double[] samples = new double[100];
            var n = new InverseGaussian(1.0, 2.0, new Numerics.Random.MersenneTwister(100));
            n.Samples(samples);
            Assert.IsTrue(!samples.Any(x => x == 0));
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new InverseGaussian(1.0, 2.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Can estimate distribution parameters.
        /// </summary>
        [TestCase(10.0, 0.1)]
        [TestCase(1.5, 1.5)]
        [TestCase(2.5, 2.5)]
        [TestCase(2.5, 5.0)]
        [TestCase(10.0, 50.0)]
        public void CanEstimateParameters(double mu, double lambda)
        {
            var original = new InverseGaussian(mu, lambda, new Numerics.Random.MersenneTwister(100));
            var estimated = InverseGaussian.Estimate(original.Samples().Take(1000000));

            AssertHelpers.AlmostEqualRelative(mu, estimated.Mu, 1);
            AssertHelpers.AlmostEqualRelative(lambda, estimated.Lambda, 1);
        }
    }
}
