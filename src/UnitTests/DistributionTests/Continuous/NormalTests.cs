// <copyright file="NormalTests.cs" company="Math.NET">
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
    /// Normal distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class NormalTests
    {
        /// <summary>
        /// Can create standard normal.
        /// </summary>
        [Test]
        public void CanCreateStandardNormal()
        {
            var n = new Normal();
            Assert.AreEqual(0.0, n.Mean);
            Assert.AreEqual(1.0, n.StdDev);
        }

        /// <summary>
        /// Can create normal.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="sdev">Standard deviation value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, Double.PositiveInfinity)]
        public void CanCreateNormal(double mean, double sdev)
        {
            var n = new Normal(mean, sdev);
            Assert.AreEqual(mean, n.Mean);
            Assert.AreEqual(sdev, n.StdDev);
        }

        /// <summary>
        /// Normal create fails with bad parameters.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="sdev">Standard deviation value.</param>
        [TestCase(Double.NaN, 1.0)]
        [TestCase(1.0, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN)]
        [TestCase(1.0, -1.0)]
        public void NormalCreateFailsWithBadParameters(double mean, double sdev)
        {
            Assert.That(() => new Normal(mean, sdev), Throws.ArgumentException);
        }

        /// <summary>
        /// Can create normal from mean and standard deviation.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="sdev">Standard deviation value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndStdDev(double mean, double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            Assert.AreEqual(mean, n.Mean);
            Assert.AreEqual(sdev, n.StdDev);
        }

        /// <summary>
        /// Can create normal from mean and variance.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="var">Variance value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndVariance(double mean, double var)
        {
            var n = Normal.WithMeanVariance(mean, var);
            AssertHelpers.AlmostEqualRelative(mean, n.Mean, 15);
            AssertHelpers.AlmostEqualRelative(var, n.Variance, 15);
        }

        /// <summary>
        /// Can create normal from mean and precision.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="prec">Precision value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndPrecision(double mean, double prec)
        {
            var n = Normal.WithMeanPrecision(mean, prec);
            AssertHelpers.AlmostEqualRelative(mean, n.Mean, 15);
            AssertHelpers.AlmostEqualRelative(prec, n.Precision, 15);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Normal(1d, 2d);
            Assert.AreEqual("Normal(μ = 1, σ = 2)", n.ToString());
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="sdev">Standard deviation value.</param>
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateEntropy(double sdev)
        {
            var n = new Normal(1.0, sdev);
            Assert.AreEqual(Constants.LogSqrt2PiE + Math.Log(n.StdDev), n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="sdev">Standard deviation value.</param>
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateSkewness(double sdev)
        {
            var n = new Normal(1.0, sdev);
            Assert.AreEqual(0.0, n.Skewness);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        [TestCase(Double.NegativeInfinity)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMode(double mean)
        {
            var n = new Normal(mean, 1.0);
            Assert.AreEqual(mean, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        [TestCase(Double.NegativeInfinity)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMedian(double mean)
        {
            var n = new Normal(mean, 1.0);
            Assert.AreEqual(mean, n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Normal();
            Assert.AreEqual(Double.NegativeInfinity, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Normal();
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="sdev">Standard deviation value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, Double.PositiveInfinity)]
        public void ValidateDensity(double mean, double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                var d = (mean - x) / sdev;
                var pdf = Math.Exp(-0.5 * d * d) / (sdev * Constants.Sqrt2Pi);
                Assert.AreEqual(pdf, n.Density(x));
                Assert.AreEqual(pdf, Normal.PDF(mean, sdev, x));
            }
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="sdev">Standard deviation value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(10.0, 100.0)]
        [TestCase(-5.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(double mean, double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                var d = (mean - x) / sdev;
                var pdfln = (-0.5 * (d * d)) - Math.Log(sdev) - Constants.LogSqrt2Pi;
                Assert.AreEqual(pdfln, n.DensityLn(x));
                Assert.AreEqual(pdfln, Normal.PDFLn(mean, sdev, x));
            }
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Normal.Sample(new Random(0), 0.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Normal.Samples(new Random(0), 0.0, 1.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => { var d = Normal.Sample(new Random(0), 0.0, -1.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => { var ied = Normal.Samples(new Random(0), 0.0, -1.0).First(); }, Throws.ArgumentException);
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
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(Double.NegativeInfinity, 0.0)]
        [TestCase(-5.0, 0.00000028665157187919391167375233287464535385442301361187883)]
        [TestCase(-2.0, 0.0002326290790355250363499258867279847735487493358890356)]
        [TestCase(-0.0, 0.0062096653257761351669781045741922211278977469230927036)]
        [TestCase(0.0, 0.0062096653257761351669781045741922211278977469230927036)]
        [TestCase(4.0, 0.30853753872598689636229538939166226011639782444542207)]
        [TestCase(5.0, 0.5)]
        [TestCase(6.0, 0.69146246127401310363770461060833773988360217555457859)]
        [TestCase(10.0, 0.9937903346742238648330218954258077788721022530769078)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        public void ValidateCumulativeDistribution(double x, double p)
        {
            var n = Normal.WithMeanStdDev(5.0, 2.0);
            AssertHelpers.AlmostEqualRelative(p, n.CumulativeDistribution(x), 9);
            AssertHelpers.AlmostEqualRelative(p, Normal.CDF(5.0, 2.0, x), 9);
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(Double.NegativeInfinity, 0.0)]
        [TestCase(-5.0, 0.00000028665157187919391167375233287464535385442301361187883)]
        [TestCase(-2.0, 0.0002326290790355250363499258867279847735487493358890356)]
        [TestCase(-0.0, 0.0062096653257761351669781045741922211278977469230927036)]
        [TestCase(0.0, .0062096653257761351669781045741922211278977469230927036)]
        [TestCase(4.0, .30853753872598689636229538939166226011639782444542207)]
        [TestCase(5.0, .5)]
        [TestCase(6.0, .69146246127401310363770461060833773988360217555457859)]
        [TestCase(10.0, 0.9937903346742238648330218954258077788721022530769078)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        public void ValidateInverseCumulativeDistribution(double x, double p)
        {
            var n = Normal.WithMeanStdDev(5.0, 2.0);
            AssertHelpers.AlmostEqualRelative(x, n.InverseCumulativeDistribution(p), 14);
            AssertHelpers.AlmostEqualRelative(x, Normal.InvCDF(5.0, 2.0, p), 14);
        }

        /// <summary>
        /// Can estimate distribution parameters.
        /// </summary>
        [TestCase(0.0, 0.0)]
        [TestCase(10.0, 0.1)]
        [TestCase(-5.0, 1.0)]
        [TestCase(0.0, 5.0)]
        [TestCase(10.0, 50.0)]
        public void CanEstimateParameters(double mean, double stddev)
        {
            var original = new Normal(mean, stddev, new Random(100));
            var estimated = Normal.Estimate(original.Samples().Take(10000));

            AssertHelpers.AlmostEqualRelative(mean, estimated.Mean, 1);
            AssertHelpers.AlmostEqualRelative(stddev, estimated.StdDev, 1);
        }
    }
}
