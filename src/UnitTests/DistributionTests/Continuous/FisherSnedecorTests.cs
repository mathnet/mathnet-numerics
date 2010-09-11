// <copyright file="FisherSnedecorTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using Distributions;

    [TestFixture]
    public class FisherSnedecorTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreateFisherSnedecor(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            Assert.AreEqual<double>(d1, n.DegreeOfFreedom1);
            Assert.AreEqual<double>(d2, n.DegreeOfFreedom2);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, Double.NaN)]
        [Row(0.0, Double.NaN)]
        [Row(-1.0, Double.NaN)]
        [Row(-10.0, Double.NaN)]
        [Row(Double.NaN, 0.0)]
        [Row(0.0, 0.0)]
        [Row(-1.0, 0.0)]
        [Row(-10.0, 0.0)]
        [Row(Double.NaN, -1.0)]
        [Row(0.0, -1.0)]
        [Row(-1.0, -1.0)]
        [Row(-10.0, -1.0)]
        [Row(Double.NaN, -10.0)]
        [Row(0.0, -10.0)]
        [Row(-1.0, -10.0)]
        [Row(-10.0, -10.0)]
        public void FisherSnedecorCreateFailsWithBadParameters(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new FisherSnedecor(2.0, 1.0);
            Assert.AreEqual<string>("FisherSnedecor(DegreeOfFreedom1 = 2, DegreeOfFreedom2 = 1)", n.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetDegreeOfFreedom1(double d1)
        {
            var n = new FisherSnedecor(1.0, 2.0);
            n.DegreeOfFreedom1 = d1;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDegreeOfFreedom1FailsWithNegativeDegreeOfFreedom()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            n.DegreeOfFreedom1 = -1.0;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetDegreeOfFreedom2(double d2)
        {
            var n = new FisherSnedecor(1.0, 2.0);
            n.DegreeOfFreedom2 = d2;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDegreeOfFreedom2FailsWithNegativeDegreeOfFreedom()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            n.DegreeOfFreedom2 = -1.0;
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMean(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d2 > 2)
            {
                Assert.AreEqual<double>(d2 / (d2 - 2.0), n.Mean);
            }
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateVariance(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d2 > 4)
            {
                Assert.AreEqual<double>((2.0 * d2 * d2 * (d1 + d2 - 2.0)) / (d1 * (d2 - 2.0) * (d2 - 2.0) * (d2 - 4.0)), n.Variance);
            }
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateStdDev(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d2 > 4)
            {
                Assert.AreEqual<double>(Math.Sqrt(n.Variance), n.StdDev);
            }
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateEntropy()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            var ent = n.Entropy;
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateSkewness(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d2 > 6)
            {
                Assert.AreEqual<double>(((2.0 * d1 + d2 - 2.0) * Math.Sqrt(8.0 * (d2 - 4.0))) / ((d2 - 6.0) * Math.Sqrt(d1 * (d1 + d2 - 2.0))), n.Skewness);
            }
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(100.0, 0.1)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(100.0, 1.0)]
        [Row(0.1, 100.0)]
        [Row(1.0, 100.0)]
        [Row(10.0, 100.0)]
        [Row(100.0, 100.0)]
        public void ValidateMode(double d1, double d2)
        {
            var n = new FisherSnedecor(d1, d2);
            if (d1 > 2)
            {
                Assert.AreEqual<double>((d2 * (d1 - 2.0)) / (d1 * (d2 + 2.0)), n.Mode);
            }
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateMedian()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            var m = n.Median;
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            Assert.AreEqual<double>(0.0, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0.1, 0.1, 1.0)]
        [Row(1.0, 0.1, 1.0)]
        [Row(10.0, 0.1, 1.0)]
        [Row(100.0, 0.1, 1.0)]
        [Row(0.1, 1.0, 1.0)]
        [Row(1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 1.0)]
        [Row(100.0, 1.0, 1.0)]
        [Row(0.1, 100.0, 1.0)]
        [Row(1.0, 100.0, 1.0)]
        [Row(10.0, 100.0, 1.0)]
        [Row(100.0, 100.0, 1.0)]
        [Row(0.1, 0.1, 10.0)]
        [Row(1.0, 0.1, 10.0)]
        [Row(10.0, 0.1, 10.0)]
        [Row(100.0, 0.1, 10.0)]
        [Row(0.1, 1.0, 10.0)]
        [Row(1.0, 1.0, 10.0)]
        [Row(10.0, 1.0, 10.0)]
        [Row(100.0, 1.0, 10.0)]
        [Row(0.1, 100.0, 10.0)]
        [Row(1.0, 100.0, 10.0)]
        [Row(10.0, 100.0, 10.0)]
        [Row(100.0, 100.0, 10.0)]
        public void ValidateDensity(double d1, double d2, double x)
        {
            var n = new FisherSnedecor(d1, d2);
            Assert.AreEqual<double>(Math.Sqrt(Math.Pow(d1 * x, d1) * Math.Pow(d2, d2) / (Math.Pow(d1 * x + d2, d1 + d2))) / (x * SpecialFunctions.Beta(d1 / 2.0, d2 / 2.0)), n.Density(x));
        }

        [Test]
        [Row(0.1, 0.1, 1.0)]
        [Row(1.0, 0.1, 1.0)]
        [Row(10.0, 0.1, 1.0)]
        [Row(100.0, 0.1, 1.0)]
        [Row(0.1, 1.0, 1.0)]
        [Row(1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 1.0)]
        [Row(100.0, 1.0, 1.0)]
        [Row(0.1, 100.0, 1.0)]
        [Row(1.0, 100.0, 1.0)]
        [Row(10.0, 100.0, 1.0)]
        [Row(100.0, 100.0, 1.0)]
        [Row(0.1, 0.1, 10.0)]
        [Row(1.0, 0.1, 10.0)]
        [Row(10.0, 0.1, 10.0)]
        [Row(100.0, 0.1, 10.0)]
        [Row(0.1, 1.0, 10.0)]
        [Row(1.0, 1.0, 10.0)]
        [Row(10.0, 1.0, 10.0)]
        [Row(100.0, 1.0, 10.0)]
        [Row(0.1, 100.0, 10.0)]
        [Row(1.0, 100.0, 10.0)]
        [Row(10.0, 100.0, 10.0)]
        [Row(100.0, 100.0, 10.0)]
        public void ValidateDensityLn(double d1, double d2, double x)
        {
            var n = new FisherSnedecor(d1, d2);
            Assert.AreEqual<double>(Math.Log(n.Density(x)), n.DensityLn(x));
        }

        [Test]
        public void CanSample()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new FisherSnedecor(1.0, 2.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.1, 0.1, 1.0)]
        [Row(1.0, 0.1, 1.0)]
        [Row(10.0, 0.1, 1.0)]
        [Row(0.1, 1.0, 1.0)]
        [Row(1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 1.0)]
        [Row(0.1, 0.1, 10.0)]
        [Row(1.0, 0.1, 10.0)]
        [Row(10.0, 0.1, 10.0)]
        [Row(0.1, 1.0, 10.0)]
        [Row(1.0, 1.0, 10.0)]
        [Row(10.0, 1.0, 10.0)]
        public void ValidateCumulativeDistribution(double d1, double d2, double x)
        {
            var n = new FisherSnedecor(d1, d2);
            Assert.AreEqual<double>(SpecialFunctions.BetaRegularized(d1 / 2.0, d2 / 2.0, d1 * d2 / (d1 + d1 * d2)), n.CumulativeDistribution(x));
        }
    }
}
