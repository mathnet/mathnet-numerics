// <copyright file="InverseGammaTests.cs" company="Math.NET">
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
    public class InverseGammaTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreateInverseGamma(double a, double b)
        {
            var n = new InverseGamma(a, b);
            Assert.AreEqual<double>(a, n.Shape);
            Assert.AreEqual<double>(b, n.Scale);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(-100.0, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(Double.NaN, 1.0)]
        [Row(1.0, 0.0)]
        [Row(1.0, -1.0)]
        [Row(1.0, -100.0)]
        [Row(1.0, Double.NegativeInfinity)]
        [Row(1.0, Double.NaN)]
        public void InverseGammaCreateFailsWithBadParameters(double a, double b)
        {
            var n = new InverseGamma(a, b);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new InverseGamma(1.1, 2.1);
            Assert.AreEqual(String.Format("InverseGamma(Shape = {0}, Inverse Scale = {1})", n.Shape, n.Scale), n.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetA(double a)
        {
            var n = new InverseGamma(1.0, 1.0);
            n.Shape = a;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0)]
        [Row(-0.0)]
        [Row(0.0)]
        public void SetAFailsWithNonPositiveA(double a)
        {
            var n = new InverseGamma(1.0, 1.0);
            n.Shape = a;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetB(double b)
        {
            var n = new InverseGamma(1.0, 1.0);
            n.Scale = b;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0)]
        [Row(-0.0)]
        [Row(0.0)]
        public void SetBFailsWithNonPositiveB(double b)
        {
            var n = new InverseGamma(1.0, 1.0);
            n.Scale = b;
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMean(double a, double b)
        {
            var n = new InverseGamma(a, b);
            if (a > 1)
            {
                Assert.AreEqual<double>(b / (a - 1.0), n.Mean);
            }
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateVariance(double a, double b)
        {
            var n = new InverseGamma(a, b);
            if (a > 2)
            {
                Assert.AreEqual<double>(b * b / ((a - 1.0) * (a - 1.0) * (a - 2.0)), n.Variance);
            }
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateStdDev(double a, double b)
        {
            var n = new InverseGamma(a, b);
            if (a > 2)
            {
                Assert.AreEqual<double>(b / ((a - 1.0) * Math.Sqrt(a - 2.0)), n.StdDev);
            }
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMode(double a, double b)
        {
            var n = new InverseGamma(a, b);
            Assert.AreEqual<double>(b / (a + 1.0), n.Mode);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateMedian()
        {
            var n = new InverseGamma(1.0, 1.0);
            var median = n.Median;
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new InverseGamma(1.0, 1.0);
            Assert.AreEqual<double>(0.0, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new InverseGamma(1.0, 1.0);
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0.1, 0.1, 1.2)]
        [Row(0.1, 1.0, 2.0)]
        [Row(0.1, Double.PositiveInfinity, 1.1)]
        [Row(1.0, 0.1, 1.5)]
        [Row(1.0, 1.0, 1.2)]
        [Row(1.0, Double.PositiveInfinity, 1.5)]
        [Row(Double.PositiveInfinity, 0.1, 5.0)]
        [Row(Double.PositiveInfinity, 1.0, 2.5)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, 1.0)]
        public void ValidateDensity(double a, double b, double x)
        {
            var n = new InverseGamma(a, b);
            if (x >= 0)
            {
                Assert.AreEqual<double>(Math.Pow(b, a) * Math.Pow(x, -a - 1.0) * Math.Exp(-b / x) / SpecialFunctions.Gamma(a), n.Density(x));
            }
            else
            {
                Assert.AreEqual<double>(0.0, n.Density(x));
            }
        }

        [Test]
        [Row(0.1, 0.1, 1.2)]
        [Row(0.1, 1.0, 2.0)]
        [Row(0.1, Double.PositiveInfinity, 1.1)]
        [Row(1.0, 0.1, 1.5)]
        [Row(1.0, 1.0, 1.2)]
        [Row(1.0, Double.PositiveInfinity, 1.5)]
        [Row(Double.PositiveInfinity, 0.1, 5.0)]
        [Row(Double.PositiveInfinity, 1.0, 2.5)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, 1.0)]
        public void ValidateDensityLn(double a, double b, double x)
        {
            var n = new InverseGamma(a, b);
            Assert.AreEqual<double>(Math.Log(n.Density(x)), n.DensityLn(x));
        }

        [Test]
        public void CanSample()
        {
            var n = new InverseGamma(1.0, 1.0);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new InverseGamma(1.0, 1.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.1, 0.1, 1.2)]
        [Row(0.1, 1.0, 2.0)]
        [Row(0.1, Double.PositiveInfinity, 1.1)]
        [Row(1.0, 0.1, 1.5)]
        [Row(1.0, 1.0, 1.2)]
        [Row(1.0, Double.PositiveInfinity, 1.5)]
        [Row(Double.PositiveInfinity, 0.1, 5.0)]
        [Row(Double.PositiveInfinity, 1.0, 2.5)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, 1.0)]
        public void ValidateCumulativeDistribution(double a, double b, double x)
        {
            var n = new InverseGamma(a, b);
            Assert.AreEqual<double>(SpecialFunctions.GammaUpperRegularized(a, b / x), n.CumulativeDistribution(x));
        }
    }
}
