// <copyright file="NormalTests.cs" company="Math.NET">
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
    /// Normal distribution tests.
    /// </summary>
    [TestFixture]
    public class NormalTests
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
        [Test, Combinatorial]
        public void CanCreateNormal([Values(0.0, 10.0, -5.0)] double mean, [Values(0.0, 0.1, 1.0, 10.0, 100.0, Double.PositiveInfinity)] double sdev)
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
        [Test, Sequential]
        public void NormalCreateFailsWithBadParameters([Values(Double.NaN, 1.0, Double.NaN, 1.0)] double mean, [Values(1.0, Double.NaN, Double.NaN, -1.0)] double sdev)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Normal(mean, sdev));
        }

        /// <summary>
        /// Can create normal from mean and standard deviation.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="sdev">Standard deviation value.</param>
        [Test, Combinatorial]
        public void CanCreateNormalFromMeanAndStdDev([Values(0.0, 10.0, -5.0)] double mean, [Values(0.0, 0.1, 1.0, 10.0, 100.0, Double.PositiveInfinity)] double sdev)
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
        [Test, Combinatorial]
        public void CanCreateNormalFromMeanAndVariance([Values(0.0, 10.0, -5.0)] double mean, [Values(0.0, 0.1, 1.0, 10.0, 100.0, Double.PositiveInfinity)] double var)
        {
            var n = Normal.WithMeanVariance(mean, var);
            AssertHelpers.AlmostEqual(mean, n.Mean, 16);
            AssertHelpers.AlmostEqual(var, n.Variance, 16);
        }

        /// <summary>
        /// Can create normal from mean and precision.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="prec">Precision value.</param>
        [Test, Combinatorial]
        public void CanCreateNormalFromMeanAndPrecision([Values(0.0, 10.0, -5.0)] double mean, [Values(0.0, 0.1, 1.0, 10.0, 100.0, Double.PositiveInfinity)] double prec)
        {
            var n = Normal.WithMeanPrecision(mean, prec);
            AssertHelpers.AlmostEqual(mean, n.Mean, 15);
            AssertHelpers.AlmostEqual(prec, n.Precision, 15);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Normal(1.0, 2.0);
            Assert.AreEqual("Normal(Mean = 1, StdDev = 2)", n.ToString());
        }

        /// <summary>
        /// Can set precision.
        /// </summary>
        /// <param name="prec">Precision value.</param>
        [Test]
        public void CanSetPrecision([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double prec)
        {
            new Normal
            {
                Precision = prec
            };
        }

        /// <summary>
        /// Set precision fails with negative value.
        /// </summary>
        [Test]
        public void SetPrecisionFailsWithNegativePrecision()
        {
            var n = new Normal();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Precision = -1.0);
        }

        /// <summary>
        /// Can set variance.
        /// </summary>
        /// <param name="var">Variance value.</param>
        [Test]
        public void CanSetVariance([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double var)
        {
            new Normal
            {
                Variance = var
            };
        }

        /// <summary>
        /// Set variance fails with negative value.
        /// </summary>
        [Test]
        public void SetVarianceFailsWithNegativeVariance()
        {
            var n = new Normal();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Variance = -1.0);
        }

        /// <summary>
        /// Can set standard deviation.
        /// </summary>
        /// <param name="sdev">Standard deviation value.</param>
        [Test]
        public void CanSetStdDev([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double sdev)
        {
            new Normal
            {
                StdDev = sdev
            };
        }

        /// <summary>
        /// Set standard deviation fails with negative value.
        /// </summary>
        [Test]
        public void SetStdDevFailsWithNegativeStdDev()
        {
            var n = new Normal();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.StdDev = -1.0);
        }

        /// <summary>
        /// Can set mean.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        [Test]
        public void CanSetMean([Values(Double.NegativeInfinity, -0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double mean)
        {
            new Normal
            {
                Mean = mean
            };
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="sdev">Standard deviation value.</param>
        [Test]
        public void ValidateEntropy([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double sdev)
        {
            var n = new Normal(1.0, sdev);
            Assert.AreEqual(Constants.LogSqrt2PiE + Math.Log(n.StdDev), n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="sdev">Standard deviation value.</param>
        [Test]
        public void ValidateSkewness([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double sdev)
        {
            var n = new Normal(1.0, sdev);
            Assert.AreEqual(0.0, n.Skewness);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        [Test]
        public void ValidateMode([Values(Double.NegativeInfinity, -0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double mean)
        {
            var n = new Normal(mean, 1.0);
            Assert.AreEqual(mean, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        [Test]
        public void ValidateMedian([Values(Double.NegativeInfinity, -0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double mean)
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
        [Test, Combinatorial]
        public void ValidateDensity([Values(0.0, 10.0, -5.0)] double mean, [Values(0.0, 0.1, 1.0, 10.0, 100.0, Double.PositiveInfinity)] double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                var d = (mean - x) / sdev;
                var pdf = Math.Exp(-0.5 * d * d) / (sdev * Constants.Sqrt2Pi);
                Assert.AreEqual(pdf, n.Density(x));
            }
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="mean">Mean value.</param>
        /// <param name="sdev">Standard deviation value.</param>
        [Test, Combinatorial]
        public void ValidateDensityLn([Values(0.0, 10.0, -5.0)] double mean, [Values(0.0, 0.1, 1.0, 10.0, 100.0, Double.PositiveInfinity)] double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                var d = (mean - x) / sdev;
                var pdfln = (-0.5 * (d * d)) - Math.Log(sdev) - Constants.LogSqrt2Pi;
                Assert.AreEqual(pdfln, n.DensityLn(x));
            }
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Normal.Sample(new Random(), 0.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Normal.Samples(new Random(), 0.0, 1.0);
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { var d = Normal.Sample(new Random(), 0.0, -1.0); });
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { var ied = Normal.Samples(new Random(), 0.0, -1.0).First(); });
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
        /// <param name="x">Input X value.</param>
        /// <param name="f">Expected value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(Double.NegativeInfinity, -5.0, -2.0, -0.0, 0.0, 4.0, 5.0, 6.0, 10.0, Double.PositiveInfinity)] double x, 
            [Values(0.0, 0.00000028665157187919391167375233287464535385442301361187883, 0.0002326290790355250363499258867279847735487493358890356, 0.0062096653257761351669781045741922211278977469230927036, 0.0062096653257761351669781045741922211278977469230927036, 0.30853753872598689636229538939166226011639782444542207, 0.5, 0.69146246127401310363770461060833773988360217555457859, 0.9937903346742238648330218954258077788721022530769078, 1.0)] double f)
        {
            var n = Normal.WithMeanStdDev(5.0, 2.0);
            AssertHelpers.AlmostEqual(f, n.CumulativeDistribution(x), 10);
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="f">Expected value.</param>
        [Test, Sequential]
        public void ValidateInverseCumulativeDistribution(
            [Values(Double.NegativeInfinity, -5.0, -2.0, -0.0, 0.0, 4.0, 5.0, 6.0, 10.0, Double.PositiveInfinity)] double x, 
            [Values(0.0, 0.00000028665157187919391167375233287464535385442301361187883, 0.0002326290790355250363499258867279847735487493358890356, 0.0062096653257761351669781045741922211278977469230927036, .0062096653257761351669781045741922211278977469230927036, .30853753872598689636229538939166226011639782444542207, .5, .69146246127401310363770461060833773988360217555457859, 0.9937903346742238648330218954258077788721022530769078, 1.0)] double f)
        {
            var n = Normal.WithMeanStdDev(5.0, 2.0);
            AssertHelpers.AlmostEqual(x, n.InverseCumulativeDistribution(f), 15);
        }
    }
}
