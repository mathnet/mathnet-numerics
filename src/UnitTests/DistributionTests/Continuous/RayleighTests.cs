// <copyright file="RayleighTests.cs" company="Math.NET">
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
    public class RayleighTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanCreateRayleigh(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>(scale, n.Scale);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(Double.NegativeInfinity)]
        [Row(-1.0)]
        [Row(0.0)]
        public void RayleighCreateFailsWithBadParameters(double scale)
        {
            var n = new Rayleigh(scale);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Rayleigh(2.0);
            Assert.AreEqual<string>("Rayleigh(Scale = 2)", n.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            var n = new Rayleigh(1.0);
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(-1.0)]
        [Row(Double.NegativeInfinity)]
        [Row(Double.NaN)]
        public void SetScaleFailsWithNegativeScale(double scale)
        {
            var n = new Rayleigh(scale);
            n.Scale = scale;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMean(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual(scale * Math.Sqrt(Constants.PiOver2), n.Mean);
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateVariance(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>((2.0 - Constants.PiOver2) * scale * scale, n.Variance);
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateStdDev(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>(Math.Sqrt(2.0 - Constants.PiOver2) * scale, n.StdDev);
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateEntropy(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>(1.0 + Math.Log(scale / Math.Sqrt(2)) + Constants.EulerMascheroni / 2.0, n.Entropy);
        }

        [Test]
        [Row(0.1, 0.63111065781893638)]
        [Row(1.0, 0.63111065781893638)]
        [Row(10.0, 0.63111065781893638)]
        [Row(Double.PositiveInfinity, 0.63111065781893638)]
        public void ValidateSkewness(double scale, double skn)
        {
            var n = new Rayleigh(scale);
            AssertHelpers.AlmostEqual(skn, n.Skewness, 17);
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMode(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>(scale, n.Mode);
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMedian(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>(scale * Math.Sqrt(Math.Log(4.0)), n.Median);
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMinimum(double scale)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>(0.0, n.Minimum);
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMaximum(double scale)
        {
            var n = new Rayleigh(1.0);
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
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
        [Row(0.1, 10.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 10.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensity(double scale, double x)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>((x / (scale * scale)) * Math.Exp(-x * x / (2.0 * scale * scale)), n.Density(x));
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
        [Row(0.1, 10.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 10.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensityLn(double scale, double x)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>(Math.Log(x / (scale * scale)) - x * x / (2.0 * scale * scale), n.DensityLn(x));
        }

        [Test]
        public void CanSample()
        {
            var n = new Rayleigh(1.0);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Rayleigh(1.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
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
        [Row(0.1, 10.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 10.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double scale, double x)
        {
            var n = new Rayleigh(scale);
            Assert.AreEqual<double>(1.0 - Math.Exp(-x * x / (2.0 * scale * scale)), n.CumulativeDistribution(x));
        }
    }
}
