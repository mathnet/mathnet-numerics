// <copyright file="GeometricTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Discrete
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using Distributions;

    [TestFixture]
    public class GeometricTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void CanCreateGeometric(double p)
        {
            var d = new Geometric(p);
            Assert.AreEqual<double>(p, d.P);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.0)]
        [Row(2.0)]
        public void BernoulliCreateFailsWithBadParameters(double p)
        {
            var d = new Geometric(p);
        }

        [Test]
        public void ValidateToString()
        {
            var d = new Geometric(0.3);
            Assert.AreEqual<string>("Geometric(P = 0.3)", d.ToString());
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void CanSetProbabilityOfOne(double p)
        {
            var d = new Geometric(0.3);
            d.P = p;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.0)]
        [Row(2.0)]
        public void SetProbabilityOfOneFails(double p)
        {
            var d = new Geometric(0.3);
            d.P = p;
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void ValidateEntropy(double p)
        {
            var d = new Geometric(p);
            Assert.AreEqual<double>((-p * System.Math.Log(p, 2.0) - (1.0 - p) * System.Math.Log(1.0 - p, 2.0)) / p, d.Entropy);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateSkewness()
        {
            var d = new Geometric(0.3);
            double s = d.Skewness;
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void ValidateMode(double p)
        {
            var d = new Geometric(p);
            Assert.AreEqual<double>(1, d.Mode);
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void ValidateMedian(double p)
        {
            var d = new Geometric(p);
            Assert.AreEqual<double>((int)Math.Ceiling(-Math.Log(2.0) / System.Math.Log(1 - p)), d.Median);
        }

        [Test]
        public void ValidateMinimum()
        {
            var d = new Geometric(0.3);
            Assert.AreEqual<double>(1.0, d.Minimum);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateMaximum()
        {
            var d = new Geometric(0.3);
            int max = d.Maximum;
        }

        [Test]
        [Row(0.0, -1)]
        [Row(0.0, 0)]
        [Row(0.0, 1)]
        [Row(0.0, 2)]
        [Row(0.3, -1)]
        [Row(0.3, 0)]
        [Row(0.3, 1)]
        [Row(0.3, 2)]
        [Row(1.0, -1)]
        [Row(1.0, 0)]
        [Row(1.0, 1)]
        [Row(1.0, 2)]
        public void ValidateProbability(double p, int x)
        {
            var d = new Geometric(p);
            if (x > 0)
            {
                Assert.AreEqual<double>(Math.Pow(1.0 - p, x - 1) * p, d.Probability(x));
            }
            else
            {
                Assert.AreEqual<double>(0.0, d.Probability(x));
            }
        }

        [Test]
        [Row(0.0, -1, Double.NegativeInfinity)]
        [Row(0.0, 0, 0.0)]
        [Row(0.0, 1, Double.NegativeInfinity)]
        [Row(0.0, 2, Double.NegativeInfinity)]
        [Row(0.3, -1, Double.NegativeInfinity)]
        [Row(0.3, 0, -0.35667494393873244235395440410727451457180907089949815)]
        [Row(0.3, 1, -1.2039728043259360296301803719337238685164245381839102)]
        [Row(0.3, 2, Double.NegativeInfinity)]
        [Row(1.0, -1, Double.NegativeInfinity)]
        [Row(1.0, 0, Double.NegativeInfinity)]
        [Row(1.0, 1, 0.0)]
        [Row(1.0, 2, Double.NegativeInfinity)]
        public void ValidateProbabilityLn(double p, int x, double pln)
        {
            var d = new Geometric(p);
            if (x > 0)
            {
                Assert.AreEqual((x - 1) * Math.Log(1.0 - p) + Math.Log(p), d.ProbabilityLn(x));
            }
            else
            {
                Assert.AreEqual(Double.NegativeInfinity, d.ProbabilityLn(x));
            }
        }

        [Test]
        public void CanSample()
        {
            var d = new Geometric(0.3);
            var s = d.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var d = new Geometric(0.3);
            var ied = d.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.0, -1.0)]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.5)]
        [Row(0.0, 1.0)]
        [Row(0.0, 2.0)]
        [Row(0.3, -1.0)]
        [Row(0.3, 0.0)]
        [Row(0.3, 0.5)]
        [Row(0.3, 1.0)]
        [Row(0.3, 2.0)]
        [Row(1.0, -1.0)]
        [Row(1.0, 0.0)]
        [Row(1.0, 0.5)]
        [Row(1.0, 1.0)]
        [Row(1.0, 2.0)]
        public void ValidateCumulativeDistribution(double p, double x)
        {
            var d = new Geometric(p);
            Assert.AreEqual(1.0 - Math.Pow(1.0 - p, x), d.CumulativeDistribution(x));
        }
    }
}