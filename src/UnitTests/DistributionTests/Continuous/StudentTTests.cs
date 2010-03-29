// <copyright file="StudentTTests.cs" company="Math.NET">
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
    public class StudentTTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        public void CanCreateStandardStudentT()
        {
            var n = new StudentT();
            AssertEx.AreEqual<double>(0.0, n.Location);
            AssertEx.AreEqual<double>(1.0, n.Scale);
            AssertEx.AreEqual<double>(1.0, n.DegreesOfFreedom);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0)]
        [Row(0.0, 0.1, 1.0)]
        [Row(0.0, 1.0, 3.0)]
        [Row(0.0, 10.0, 1.0)]
        [Row(0.0, 10.0, Double.PositiveInfinity)]
        [Row(10.0, 1.0, 1.0)]
        [Row(-5.0, 100.0, 1.0)]
        [Row(0.0, Double.PositiveInfinity, 1.0)]
        public void CanCreateStudentT(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(location, n.Location);
            AssertEx.AreEqual<double>(scale, n.Scale);
            AssertEx.AreEqual<double>(dof, n.DegreesOfFreedom);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1.0, 1.0)]
        [Row(0.0, Double.NaN, 1.0)]
        [Row(0.0, 1.0, Double.NaN)]
        [Row(0.0, -10.0, 1.0)]
        [Row(0.0, 10.0, -1.0)]
        public void StudentTCreateFailsWithBadParameters(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new StudentT(1.0, 2.0, 1.0);
            AssertEx.AreEqual<string>("StudentT(Location = 1, Scale = 2, DoF = 1)", n.ToString());
        }

        [Test]
        [Row(Double.NegativeInfinity)]
        [Row(-5.0)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetLocation(double loc)
        {
            var n = new StudentT();
            n.Location = loc;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            var n = new StudentT();
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0)]
        [Row(-0.0)]
        [Row(0.0)]
        public void SetScaleFailsWithNonPositiveScale(double scale)
        {
            {
                var n = new StudentT();
                n.Scale = scale;
            }
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetDoF(double dof)
        {
            var n = new StudentT();
            n.DegreesOfFreedom = dof;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0)]
        [Row(-0.0)]
        [Row(0.0)]
        public void SetDofFailsWithNonPositiveDoF(double dof)
        {
            {
                var n = new StudentT();
                n.DegreesOfFreedom = dof;
            }
        }

        [Test]
        [Row(0.0, 1.0, 1.0, Double.NaN)]
        [Row(0.0, 0.1, 1.0, Double.NaN)]
        [Row(0.0, 1.0, 3.0, 0.0)]
        [Row(0.0, 10.0, 1.0, Double.NaN)]
        [Row(0.0, 10.0, 2.0, 0.0)]
        [Row(0.0, 10.0, Double.PositiveInfinity, 0.0)]
        [Row(10.0, 1.0, 1.0, Double.NaN)]
        [Row(-5.0, 100.0, 1.5, -5.0)]
        [Row(0.0, Double.PositiveInfinity, 1.0)]
        public void ValidateMean(double location, double scale, double dof, double mean)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(n.Mean, mean);
        }
/*
        [Test]
        [Row(0.0, 1.0, 1.0)]
        [Row(0.0, 0.1, 1.0)]
        [Row(0.0, 1.0, 3.0)]
        [Row(0.0, 10.0, 1.0)]
        [Row(0.0, 10.0, 2.0)]
        [Row(0.0, 10.0, 3.0)]
        [Row(0.0, 10.0, Double.PositiveInfinity)]
        [Row(10.0, 1.0, 1.0)]
        [Row(-5.0, 100.0, 1.0)]
        [Row(0.0, Double.PositiveInfinity, 1.0)]
        public void ValidateVariance(double location, double scale, double dof, double var)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(n.Variance, location);
        }

        

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void Entropy(double sdev)
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
        }*/
    }
}
