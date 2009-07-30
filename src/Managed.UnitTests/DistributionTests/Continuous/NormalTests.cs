// <copyright file="NormalTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.DistributionTests
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using MathNet.Numerics.Distributions;

    [TestFixture]
    public class NormalTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        public void CanCreateStandardNormal()
        {
            var n = new Normal();
            AssertEx.AreEqual<double>(0.0, n.Mean);
            AssertEx.AreEqual<double>(1.0, n.StdDev);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateNormal(double mean, double sdev)
        {
            var n = new Normal(mean, sdev);
            AssertEx.AreEqual<double>(mean, n.Mean);
            AssertEx.AreEqual<double>(sdev, n.StdDev);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1.0)]
        [Row(1.0, Double.NaN)]
        [Row(Double.NaN, Double.NaN)]
        [Row(1.0, -1.0)]
        public void NormalCreateFailsWithBadParameters(double mean, double sdev)
        {
            var n = new Normal(mean, sdev);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndStdDev(double mean, double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            AssertEx.AreEqual<double>(mean, n.Mean);
            AssertEx.AreEqual<double>(sdev, n.StdDev);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndVariance(double mean, double var)
        {
            var n = Normal.WithMeanVariance(mean, var);
            AssertHelpers.AlmostEqual(mean, n.Mean, 16);
            AssertHelpers.AlmostEqual(var, n.Variance, 16);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndPrecision(double mean, double prec)
        {
            var n = Normal.WithMeanAndPrecision(mean, prec);
            AssertHelpers.AlmostEqual(mean, n.Mean, 15);
            AssertHelpers.AlmostEqual(prec, n.Precision, 15);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Normal(1.0, 2.0);
            AssertEx.AreEqual<string>("Normal(Mean = 1, StdDev = 2)", n.ToString());
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetPrecision(double prec)
        {
            var n = new Normal();
            n.Precision = prec;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetPrecisionFailsWithNegativePrecision()
        {
            var n = new Normal();
            n.Precision = -1.0;
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetVariance(double var)
        {
            var n = new Normal();
            n.Variance = var;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetVarianceFailsWithNegativeVariance()
        {
            var n = new Normal();
            n.Variance = -1.0;
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetStdDev(double sdev)
        {
            var n = new Normal();
            n.StdDev = sdev;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetStdDevFailsWithNegativeStdDev()
        {
            var n = new Normal();
            n.StdDev = -1.0;
        }

        [Test]
        [Row(Double.NegativeInfinity)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetMean(double mean)
        {
            var n = new Normal();
            n.Mean = mean;
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateEntropy(double sdev)
        {
            var n = new Normal(1.0, sdev);
            AssertEx.AreEqual<double>(MathNet.Numerics.Constants.LogSqrt2PiE + Math.Log(n.StdDev), n.Entropy);
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateSkewness(double sdev)
        {
            var n = new Normal(1.0, sdev);
            AssertEx.AreEqual<double>(0.0, n.Skewness);
        }

        [Test]
        [Row(Double.NegativeInfinity)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMode(double mean)
        {
            var n = new Normal(mean, 1.0);
            AssertEx.AreEqual<double>(mean, n.Mode);
        }

        [Test]
        [Row(Double.NegativeInfinity)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMedian(double mean)
        {
            var n = new Normal(mean, 1.0);
            AssertEx.AreEqual<double>(mean, n.Median);
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new Normal();
            AssertEx.AreEqual<double>(System.Double.NegativeInfinity, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new Normal();
            AssertEx.AreEqual<double>(System.Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateDensity(double mean, double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            for(int i = 0; i < 11; i++)
            {
                double x = i - 5.0;
                double d = (mean - x)/sdev;
                double pdf = Math.Exp(-0.5*d*d)/(sdev*Constants.Sqrt2Pi);
                AssertEx.AreEqual<double>(pdf, n.Density(x));
            }
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(double mean, double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            for (int i = 0; i < 11; i++)
            {
                double x = i - 5.0;
                double d = (mean - x) / sdev;
                double pdfln = -0.5 * d * d - Math.Log(sdev) - Constants.LogSqrt2Pi;
                AssertEx.AreEqual<double>(pdfln, n.DensityLn(x));
            }
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = Normal.Sample(new Random(), 0.0, 1.0);
        }

        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Normal.Samples(new Random(), 0.0, 1.0);
            var arr = ied.Take(5).ToArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = Normal.Sample(new Random(), 0.0, -1.0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleSequenceStatic()
        {
            var ied = Normal.Samples(new Random(), 0.0, -1.0).First();
        }

        [Test]
        public void CanSample()
        {
            var n = new Normal();
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Normal();
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(Double.NegativeInfinity, 0.0)]
        [Row(-5.0, 0.00000028665157187919391167375233287464535385442301361187883)]
        [Row(-2.0, 0.0002326290790355250363499258867279847735487493358890356)]
        [Row(-0.0, 0.0062096653257761351669781045741922211278977469230927036)]
        [Row(0.0, 0.0062096653257761351669781045741922211278977469230927036)]
        [Row(4.0, 0.30853753872598689636229538939166226011639782444542207)]
        [Row(5.0, 0.5)]
        [Row(6.0, 0.69146246127401310363770461060833773988360217555457859)]
        [Row(10.0, 0.9937903346742238648330218954258077788721022530769078)]
        [Row(Double.PositiveInfinity, 1.0)]
        public void ValidateCumulativeDistribution(double x, double f)
        {
            var n = Normal.WithMeanStdDev(5.0, 2.0);
            AssertHelpers.AlmostEqual(f, n.CumulativeDistribution(x), 10);
        }

        [Test]
        [Row(Double.NegativeInfinity, 0.0)]
        [Row(-5.0, 0.00000028665157187919391167375233287464535385442301361187883)]
        [Row(-2.0, 0.0002326290790355250363499258867279847735487493358890356)]
        [Row(-0.0, 0.0062096653257761351669781045741922211278977469230927036)]
        [Row(0.0, 0.0062096653257761351669781045741922211278977469230927036)]
        [Row(4.0, 0.30853753872598689636229538939166226011639782444542207)]
        [Row(5.0, 0.5)]
        [Row(6.0, 0.69146246127401310363770461060833773988360217555457859)]
        [Row(10.0, 0.9937903346742238648330218954258077788721022530769078)]
        [Row(Double.PositiveInfinity, 1.0)]
        public void ValidateInverseCumulativeDistribution(double x, double f)
        {
            var n = Normal.WithMeanStdDev(5.0, 2.0);
            AssertHelpers.AlmostEqual(x, n.InverseCumulativeDistribution(f), 15);
        }
    }
}
