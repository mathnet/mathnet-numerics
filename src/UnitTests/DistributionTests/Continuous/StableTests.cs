// <copyright file="StableTests.cs" company="Math.NET">
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
    public class StableTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0.1, -1.0, 0.1, Double.NegativeInfinity)]
        [Row(0.1, -1.0, 0.1, -1.0)]
        [Row(0.1, -1.0, 0.1, 0.0)]
        [Row(0.1, -1.0, 0.1, 1.0)]
        [Row(0.1, -1.0, 0.1, Double.PositiveInfinity)]
        [Row(0.1, -1.0, 1.0, Double.NegativeInfinity)]
        [Row(0.1, -1.0, 1.0, -1.0)]
        [Row(0.1, -1.0, 1.0, 0.0)]
        [Row(0.1, -1.0, 1.0, 1.0)]
        [Row(0.1, -1.0, 1.0, Double.PositiveInfinity)]
        [Row(0.1, -1.0, Double.PositiveInfinity, Double.NegativeInfinity)]
        [Row(0.1, -1.0, Double.PositiveInfinity, -1.0)]
        [Row(0.1, -1.0, Double.PositiveInfinity, 0.0)]
        [Row(0.1, -1.0, Double.PositiveInfinity, 1.0)]
        [Row(0.1, -1.0, Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(0.1, 1.0, 0.1, Double.NegativeInfinity)]
        [Row(0.1, 1.0, 0.1, -1.0)]
        [Row(0.1, 1.0, 0.1, 0.0)]
        [Row(0.1, 1.0, 0.1, 1.0)]
        [Row(0.1, 1.0, 0.1, Double.PositiveInfinity)]
        [Row(2.0, 1.0, 0.1, Double.NegativeInfinity)]
        [Row(2.0, 1.0, 0.1, -1.0)]
        [Row(2.0, 1.0, 0.1, 0.0)]
        [Row(2.0, 1.0, 0.1, 1.0)]
        [Row(2.0, 1.0, 0.1, Double.PositiveInfinity)]
        public void CanCreateStable(double alpha, double beta, double scale, double location)
        {
            var n = new Stable(alpha, beta, scale, location);
            Assert.AreEqual<double>(alpha, n.Alpha);
            Assert.AreEqual<double>(beta, n.Beta);
            Assert.AreEqual<double>(scale, n.Scale);
            Assert.AreEqual<double>(location, n.Location);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, Double.NaN, Double.NaN, Double.NaN)]
        [Row(1.0, Double.NaN, Double.NaN, Double.NaN)]
        [Row(Double.NaN, 1.0, Double.NaN, Double.NaN)]
        [Row(Double.NaN, Double.NaN, 1.0, Double.NaN)]
        [Row(Double.NaN, Double.NaN, Double.NaN, 1.0)]
        [Row(1.0, 1.0, Double.NaN, Double.NaN)]
        [Row(1.0, Double.NaN, 1.0, Double.NaN)]
        [Row(1.0, Double.NaN, Double.NaN, 1.0)]
        [Row(Double.NaN, 1.0, 1.0, Double.NaN)]
        [Row(1.0, 1.0, 1.0, Double.NaN)]
        [Row(1.0, 1.0, Double.NaN, 1.0)]
        [Row(1.0, Double.NaN, 1.0, 1.0)]
        [Row(Double.NaN, 1.0, 1.0, 1.0)]
        [Row(1.0, 1.0, 0.0, 1.0)]
        [Row(1.0, -1.1, 1.0, 1.0)]
        [Row(1.0, 1.1, 1.0, 1.0)]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(2.1, 1.0, 1.0, 1.0)]
        public void StableCreateFailsWithBadParameters(double alpha, double beta, double location, double scale)
        {
            var n = new Stable(alpha, beta, location, scale);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Stable(1.2, 0.3, 1.0, 2.0);
            Assert.AreEqual<string>("Stable(Stability = 1.2, Skewness = 0.3, Scale = 1, Location = 2)", n.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(2.0)]
        public void CanSetAlpha(double alpha)
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Alpha = alpha;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(2.1)]
        [Row(Double.NegativeInfinity)]
        [Row(Double.PositiveInfinity)]
        public void SetAlphaFail(double alpha)
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Alpha = alpha;
        }

        [Test]
        [Row(-1.0)]
        [Row(-0.1)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        public void CanSetBeta(double beta)
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Beta = beta;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.1)]
        [Row(1.1)]
        [Row(Double.NegativeInfinity)]
        [Row(Double.PositiveInfinity)]
        public void SetBetaFail(double beta)
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Beta = beta;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(0.0)]
        public void SetScaleFail(double scale)
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Scale = scale;
        }

        [Test]
        [Row(Double.NegativeInfinity)]
        [Row(-10.0)]
        [Row(-1.0)]
        [Row(-0.1)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetLocation(double location)
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Location = location;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetLocationFail()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            n.Location = Double.NaN;
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateEntropy()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            var e = n.Entropy;
        }

        [Test]
        public void ValidateSkewness()
        {
            var n = new Stable(2.0, 1.0, 1.0, 1.0);
            if (n.Alpha == 2)
            {
                Assert.AreEqual<double>(0.0, n.Skewness);
            }
        }

        [Test]
        [Row(Double.NegativeInfinity)]
        [Row(-10.0)]
        [Row(-1.0)]
        [Row(-0.1)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMode(double location)
        {
            var n = new Stable(1.0, 0.0, 1.0, location);
            if (n.Beta == 0)
            {
                Assert.AreEqual<double>(location, n.Mode);
            }
        }

        [Test]
        [Row(Double.NegativeInfinity)]
        [Row(-10.0)]
        [Row(-1.0)]
        [Row(-0.1)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMedian(double location)
        {
            var n = new Stable(1.0, 0.0, 1.0, location);
            if (n.Beta == 0)
            {
                Assert.AreEqual<double>(location, n.Mode);
            }
        }

        [Test]
        [Row(-1.0)]
        [Row(-0.1)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        public void ValidateMinimum(double beta)
        {
            var n = new Stable(1.0, beta, 1.0, 1.0);
            if (Math.Abs(beta) != 1)
            {
                Assert.AreEqual<double>(Double.NegativeInfinity, n.Minimum);
            }
            else
            {
                Assert.AreEqual<double>(0.0, n.Minimum);
            }
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(2.0, -1.0, 1.0, 0.0, 1.5, 0.16073276729880184)]
        [Row(2.0, -1.0, 1.0, 0.0, 3.0, 0.029732572305907354)]
        [Row(2.0, -1.0, 1.0, 0.0, 5.0, 0.00054457105758817781)]
        [Row(1.0, 0.0, 1.0, 0.0, 1.5, 0.097941503441166353)]
        [Row(1.0, 0.0, 1.0, 0.0, 3.0, 0.031830988618379068)]
        [Row(1.0, 0.0, 1.0, 0.0, 5.0, 0.012242687930145794)]
        [Row(0.5, 1.0, 1.0, 0.0, 1.5, 0.15559955475708653)]
        [Row(0.5, 1.0, 1.0, 0.0, 3.0, 0.064989885240913717)]
        [Row(0.5, 1.0, 1.0, 0.0, 5.0, 0.032286845174307237)]
        public void ValidateDensity(double alpha, double beta, double scale, double location, double x, double d)
        {
            var n = new Stable(alpha, beta, scale, location);
            AssertHelpers.AlmostEqual(d, n.Density(x), 15);
        }

        [Test]
        [Row(2.0, -1.0, 1.0, 0.0, 1.5, -1.8280121234846454)]
        [Row(2.0, -1.0, 1.0, 0.0, 3.0, -3.5155121234846449)]
        [Row(2.0, -1.0, 1.0, 0.0, 5.0, -7.5155121234846449)]
        [Row(1.0, 0.0, 1.0, 0.0, 1.5, -2.3233848821910463)]
        [Row(1.0, 0.0, 1.0, 0.0, 3.0, -3.4473149788434458)]
        [Row(1.0, 0.0, 1.0, 0.0, 5.0, -4.4028264238708825)]
        [Row(0.5, 1.0, 1.0, 0.0, 1.5, -1.8604695287002526)]
        [Row(0.5, 1.0, 1.0, 0.0, 3.0, -2.7335236328735038)]
        [Row(0.5, 1.0, 1.0, 0.0, 5.0, -3.4330954018558235)]
        public void ValidateDensityLn(double alpha, double beta, double scale, double location, double x, double dln)
        {
            var n = new Stable(alpha, beta, scale, location);
            AssertHelpers.AlmostEqual(dln, n.DensityLn(x), 15);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void CanSample()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            var d = n.Sample();
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void CanSampleSequence()
        {
            var n = new Stable(1.0, 1.0, 1.0, 1.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(2.0, -1.0, 1.0, 0.0, 1.5, 0.8555778168267576)]
        [Row(2.0, -1.0, 1.0, 0.0, 3.0, 0.98305257323765538)]
        [Row(2.0, -1.0, 1.0, 0.0, 5.0, 0.9997965239912775)]
        [Row(1.0, 0.0, 1.0, 0.0, 1.5, 0.81283295818900125)]
        [Row(1.0, 0.0, 1.0, 0.0, 3.0, 0.89758361765043326)]
        [Row(1.0, 0.0, 1.0, 0.0, 5.0, 0.93716704181099886)]
        [Row(0.5, 1.0, 1.0, 0.0, 1.5, 0.41421617824252516)]
        [Row(0.5, 1.0, 1.0, 0.0, 3.0, 0.563702861650773)]
        [Row(0.5, 1.0, 1.0, 0.0, 5.0, 0.65472084601857694)]
        public void ValidateCumulativeDistribution(double alpha, double beta, double scale, double location, double x, double cdf)
        {
            var n = new Stable(alpha, beta, scale, location);
            AssertHelpers.AlmostEqual(cdf, n.CumulativeDistribution(x), 15);
        }
    }
}
