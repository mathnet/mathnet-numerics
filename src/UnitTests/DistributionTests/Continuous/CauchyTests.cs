// <copyright file="CauchyTests.cs" company="Math.NET">
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
    public class CauchyTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        public void CanCreateCauchy()
        {
            var n = new Cauchy();
            Assert.AreEqual<double>(0.0, n.Location);
            Assert.AreEqual<double>(1.0, n.Scale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateCauchy(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual<double>(location, n.Location);
            Assert.AreEqual<double>(scale, n.Scale);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1.0)]
        [Row(1.0, Double.NaN)]
        [Row(Double.NaN, Double.NaN)]
        [Row(1.0, 0.0)]
        public void CauchyCreateFailsWithBadParameters(double location, double scale)
        {
            var n = new Cauchy(location, scale);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Cauchy(1.0, 2.0);
            Assert.AreEqual<string>("Cauchy(Location = 1, Scale = 2)", n.ToString());
        }

        [Test]
        [Row(-10.0)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        public void CanSetLocation(double location)
        {
            var n = new Cauchy();
            n.Location = location;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetLocationFail()
        {
            var n = new Cauchy();
            n.Location = Double.NaN;
        }

        [Test]
        [Row(1.0)]
        [Row(2.0)]
        [Row(12.0)]
        public void CanSetScale(double scale)
        {
            var n = new Cauchy();
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetScaleFail()
        {
            var n = new Cauchy();
            n.Scale = -1.0;
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        public void ValidateEntropy(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual<double>(Math.Log(4.0 * Constants.Pi * scale), n.Entropy);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateSkewness(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual<double>(0.0, n.Skewness);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateMode(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual<double>(location, n.Mode);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateMedian(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual<double>(location, n.Median);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateMinimum(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual<double>(Double.NegativeInfinity, n.Minimum);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateMaximum(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        public void ValidateDensity(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            for (int i = 0; i < 11; i++)
            {
                double x = i - 5.0;
                Assert.AreEqual<double>(1.0 / (Constants.Pi * scale * (1.0 + ((x - location) / scale) * ((x - location) / scale))), n.Density(x));
            }
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        public void ValidateDensityLn(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            for (int i = 0; i < 11; i++)
            {
                double x = i - 5.0;
                Assert.AreEqual<double>(-Math.Log(Constants.Pi * scale * (1.0 + ((x - location) / scale) * ((x - location) / scale))), n.DensityLn(x));
           }
        }

        [Test]
        public void CanSample()
        {
            var n = new Cauchy();
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Cauchy();
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double location, double scale)
        {
            var n = new Cauchy(location, scale);
            for (int i = 0; i < 11; i++)
            {
                double x = i - 5.0;
                Assert.AreEqual<double>((1.0 / Constants.Pi) * Math.Atan((x - location) / scale) + 0.5, n.CumulativeDistribution(x));
            }
        }
    }
}
