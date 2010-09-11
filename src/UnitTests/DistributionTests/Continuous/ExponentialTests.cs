// <copyright file="ExponentialTests.cs" company="Math.NET">
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
    public class ExponentialTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanCreateExponential(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(lambda, n.Lambda);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.0)]
        [Row(-10.0)]
        public void ExponentialCreateFailsWithBadParameters(double lambda)
        {
            var n = new Exponential(lambda);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Exponential(2.0);
            Assert.AreEqual<string>("Exponential(Lambda = 2)", n.ToString());
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetLambda(double lambda)
        {
            var n = new Exponential(1.0);
            n.Lambda = lambda;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetLambdaFailsWithNegativeLambda()
        {
            var n = new Exponential(1.0);
            n.Lambda = -1.0;
        }

        [Test]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMean(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(1.0 / lambda, n.Mean);
        }

        [Test]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateVariance(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(1.0 / (lambda * lambda), n.Variance);
        }

        [Test]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateStdDev(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(1.0 / lambda, n.StdDev);
        }

        [Test]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateEntropy(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(1.0 - Math.Log(lambda), n.Entropy);
        }

        [Test]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateSkewness(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(2.0, n.Skewness);
        }

        [Test]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMode(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(0.0, n.Mode);
        }

        [Test]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMedian(double lambda)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(Math.Log(2.0) / lambda, n.Median);
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new Exponential(1.0);
            Assert.AreEqual<double>(0.0, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new Exponential(1.0);
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.1, 0.0)]
        [Row(1.0, 0.0)]
        [Row(10.0, 0.0)]
        [Row(Double.PositiveInfinity, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensity(double lambda, double x)
        {
            var n = new Exponential(lambda);
            if (x >= 0)
            {
                Assert.AreEqual<double>(lambda * Math.Exp(-lambda * x), n.Density(x));
            }
            else
            {
                Assert.AreEqual<double>(0.0, n.Density(lambda));
            }
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.1, 0.0)]
        [Row(1.0, 0.0)]
        [Row(10.0, 0.0)]
        [Row(Double.PositiveInfinity, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensityLn(double lambda, double x)
        {
            var n = new Exponential(lambda);
            Assert.AreEqual<double>(Math.Log(lambda) - lambda * x, n.DensityLn(x));
        }

        [Test]
        public void CanSample()
        {
            var n = new Exponential(1.0);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Exponential(1.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.1, 0.0)]
        [Row(1.0, 0.0)]
        [Row(10.0, 0.0)]
        [Row(Double.PositiveInfinity, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.1, 0.1)]
        [Row(1.0, 0.1)]
        [Row(10.0, 0.1)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.1, 1.0)]
        [Row(1.0, 1.0)]
        [Row(10.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double lambda, double x)
        {
            var n = new Exponential(lambda);
            if (x >= 0.0)
            {
                Assert.AreEqual<double>(1.0 - Math.Exp(-lambda * x), n.CumulativeDistribution(x));
            }
            else
            {
                Assert.AreEqual<double>(0.0, n.CumulativeDistribution(x));
            }
        }
    }
}
