// <copyright file="LaplaceTests.cs" company="Math.NET">
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
    public class LaplaceTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        public void CanCreateLaplace()
        {
            var n = new Laplace();
            Assert.AreEqual<double>(0.0, n.Location);
            Assert.AreEqual<double>(1.0, n.Scale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.1)]
        [Row(-1.0, 0.1)]
        [Row(5.0, 0.1)]
        [Row(-5.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.NegativeInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(1.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(5.0, 1.0)]
        [Row(-5.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(-1.0, Double.PositiveInfinity)]
        [Row(5.0, Double.PositiveInfinity)]
        [Row(-5.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void CanCreateLaplace(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(location, n.Location);
            Assert.AreEqual<double>(scale, n.Scale);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Laplace(-1.0, 2.0);
            Assert.AreEqual<string>("Laplace(Location = -1, Scale = 2)", n.ToString());
        }

        [Test]
        [Row(0.0)]
        [Row(1.0)]
        [Row(-1.0)]
        [Row(5.0)]
        [Row(-5.0)]
        [Row(Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity)]
        public void CanSetLocation(double location)
        {
            var n = new Laplace();
            n.Location = location;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        public void SetLocationFailsWithNegativeLocation(double location)
        {
            var n = new Laplace();
            n.Location = location;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            var n = new Laplace();
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0)]
        [Row(-1.0)]
        [Row(-5.0)]
        [Row(Double.NegativeInfinity)]
        [Row(Double.NaN)]
        public void SetScaleFailsWithNegativeScale(double scale)
        {
            var n = new Laplace();
            n.Scale = scale;
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.1)]
        [Row(-1.0, 0.1)]
        [Row(5.0, 0.1)]
        [Row(-5.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.NegativeInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(1.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(5.0, 1.0)]
        [Row(-5.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(-1.0, Double.PositiveInfinity)]
        [Row(5.0, Double.PositiveInfinity)]
        [Row(-5.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void ValidateMean(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(location, n.Mean);
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.1)]
        [Row(-1.0, 0.1)]
        [Row(5.0, 0.1)]
        [Row(-5.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.NegativeInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(1.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(5.0, 1.0)]
        [Row(-5.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(-1.0, Double.PositiveInfinity)]
        [Row(5.0, Double.PositiveInfinity)]
        [Row(-5.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void ValidateVariance(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(2.0 * scale * scale, n.Variance);
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.1)]
        [Row(-1.0, 0.1)]
        [Row(5.0, 0.1)]
        [Row(-5.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.NegativeInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(1.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(5.0, 1.0)]
        [Row(-5.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(-1.0, Double.PositiveInfinity)]
        [Row(5.0, Double.PositiveInfinity)]
        [Row(-5.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void ValidateStdDev(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(Math.Sqrt(2.0) * scale, n.StdDev);
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.1)]
        [Row(-1.0, 0.1)]
        [Row(5.0, 0.1)]
        [Row(-5.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.NegativeInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(1.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(5.0, 1.0)]
        [Row(-5.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(-1.0, Double.PositiveInfinity)]
        [Row(5.0, Double.PositiveInfinity)]
        [Row(-5.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void ValidateEntropy(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(Math.Log(2.0 * Constants.E * scale), n.Entropy);
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.1)]
        [Row(-1.0, 0.1)]
        [Row(5.0, 0.1)]
        [Row(-5.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.NegativeInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(1.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(5.0, 1.0)]
        [Row(-5.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(-1.0, Double.PositiveInfinity)]
        [Row(5.0, Double.PositiveInfinity)]
        [Row(-5.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void ValidateSkewness(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(0.0, n.Skewness);
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.1)]
        [Row(-1.0, 0.1)]
        [Row(5.0, 0.1)]
        [Row(-5.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.NegativeInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(1.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(5.0, 1.0)]
        [Row(-5.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(-1.0, Double.PositiveInfinity)]
        [Row(5.0, Double.PositiveInfinity)]
        [Row(-5.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void ValidateMode(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(location, n.Mode);
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.1)]
        [Row(-1.0, 0.1)]
        [Row(5.0, 0.1)]
        [Row(-5.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.NegativeInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(1.0, 1.0)]
        [Row(-1.0, 1.0)]
        [Row(5.0, 1.0)]
        [Row(-5.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.NegativeInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(-1.0, Double.PositiveInfinity)]
        [Row(5.0, Double.PositiveInfinity)]
        [Row(-5.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void ValidateMedian(double location, double scale)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(location, n.Median);
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new Laplace();
            Assert.AreEqual<double>(Double.NegativeInfinity, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new Laplace();
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0.0, 0.1, 1.5)]
        [Row(1.0, 0.1, 2.8)]
        [Row(-1.0, 0.1, -5.4)]
        [Row(5.0, 0.1, -4.9)]
        [Row(-5.0, 0.1, 2.0)]
        [Row(Double.PositiveInfinity, 0.1, 5.5)]
        [Row(Double.NegativeInfinity, 0.1, -0.0)]
        [Row(0.0, 1.0, Double.PositiveInfinity)]
        [Row(1.0, 1.0, 5.0)]
        [Row(-1.0, 1.0, -1.0)]
        [Row(5.0, 1.0, -1.0)]
        [Row(-5.0, 1.0, 2.5)]
        [Row(Double.PositiveInfinity, 1.0, 2.0)]
        [Row(Double.NegativeInfinity, 1.0, 15.0)]
        [Row(0.0, Double.PositiveInfinity, 89.3)]
        [Row(1.0, Double.PositiveInfinity, -0.1)]
        [Row(-1.0, Double.PositiveInfinity, 0.1)]
        [Row(5.0, Double.PositiveInfinity, -6.1)]
        [Row(-5.0, Double.PositiveInfinity, -10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, 2.0)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity, -5.1)]
        public void ValidateDensity(double location, double scale, double x)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(Math.Exp(-Math.Abs(x - location) / scale) / (2.0 * scale), n.Density(x));
        }

        [Test]
        [Row(0.0, 0.1, 1.5)]
        [Row(1.0, 0.1, 2.8)]
        [Row(-1.0, 0.1, -5.4)]
        [Row(5.0, 0.1, -4.9)]
        [Row(-5.0, 0.1, 2.0)]
        [Row(Double.PositiveInfinity, 0.1, 5.5)]
        [Row(Double.NegativeInfinity, 0.1, -0.0)]
        [Row(0.0, 1.0, Double.PositiveInfinity)]
        [Row(1.0, 1.0, 5.0)]
        [Row(-1.0, 1.0, -1.0)]
        [Row(5.0, 1.0, -1.0)]
        [Row(-5.0, 1.0, 2.5)]
        [Row(Double.PositiveInfinity, 1.0, 2.0)]
        [Row(Double.NegativeInfinity, 1.0, 15.0)]
        [Row(0.0, Double.PositiveInfinity, 89.3)]
        [Row(1.0, Double.PositiveInfinity, -0.1)]
        [Row(-1.0, Double.PositiveInfinity, 0.1)]
        [Row(5.0, Double.PositiveInfinity, -6.1)]
        [Row(-5.0, Double.PositiveInfinity, -10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, 2.0)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity, -5.1)]
        public void ValidateDensityLn(double location, double scale, double x)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(-Math.Log(2.0 * scale) - Math.Abs(x - location) / scale, n.DensityLn(x));
        }

        [Test]
        public void CanSample()
        {
            var n = new Laplace();
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Laplace();
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.0, 0.1, 1.5)]
        [Row(1.0, 0.1, 2.8)]
        [Row(-1.0, 0.1, -5.4)]
        [Row(5.0, 0.1, -4.9)]
        [Row(-5.0, 0.1, 2.0)]
        [Row(Double.PositiveInfinity, 0.1, 5.5)]
        [Row(Double.NegativeInfinity, 0.1, -0.0)]
        [Row(0.0, 1.0, Double.PositiveInfinity)]
        [Row(1.0, 1.0, 5.0)]
        [Row(-1.0, 1.0, -1.0)]
        [Row(5.0, 1.0, -1.0)]
        [Row(-5.0, 1.0, 2.5)]
        [Row(Double.PositiveInfinity, 1.0, 2.0)]
        [Row(Double.NegativeInfinity, 1.0, 15.0)]
        [Row(0.0, Double.PositiveInfinity, 89.3)]
        [Row(1.0, Double.PositiveInfinity, -0.1)]
        [Row(-1.0, Double.PositiveInfinity, 0.1)]
        [Row(5.0, Double.PositiveInfinity, -6.1)]
        [Row(-5.0, Double.PositiveInfinity, -10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, 2.0)]
        [Row(Double.NegativeInfinity, Double.PositiveInfinity, -5.1)]
        public void ValidateCumulativeDistribution(double location, double scale, double x)
        {
            var n = new Laplace(location, scale);
            Assert.AreEqual<double>(0.5 * (1.0 + Math.Sign(x - location) * (1.0 - Math.Exp(-Math.Abs(x - location) / scale))), n.CumulativeDistribution(x));
        }
    }
}
