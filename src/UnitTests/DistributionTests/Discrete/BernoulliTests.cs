// <copyright file="BernoulliTests.cs" company="Math.NET">
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
    public class BernoulliTests
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
        public void CanCreateBernoulli(double p)
        {
            var bernoulli = new Bernoulli(p);
            AssertEx.AreEqual<double>(p, bernoulli.P);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.0)]
        [Row(2.0)]
        public void BernoulliCreateFailsWithBadParameters(double p)
        {
            var bernoulli = new Bernoulli(p);
        }

        [Test]
        public void ValidateToString()
        {
            var b = new Bernoulli(0.3);
            AssertEx.AreEqual<string>("Bernoulli(P = 0.3)", b.ToString());
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void CanSetProbabilityOfOne(double p)
        {
            var b = new Bernoulli(0.3);
            b.P = p;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.0)]
        [Row(2.0)]
        public void SetProbabilityOfOneFails(double p)
        {
            var b = new Bernoulli(0.3);
            b.P = p;
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void ValidateEntropy(double p)
        {
            var b = new Bernoulli(p);
            AssertHelpers.AlmostEqual(-(1.0 - p) * Math.Log(1.0 - p) - p * Math.Log(p), b.Entropy, 14);
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void ValidateSkewness(double p)
        {
            var b = new Bernoulli(p);
            AssertEx.AreEqual<double>((1.0 - 2.0 * p) / Math.Sqrt(p * (1.0 - p)), b.Skewness);
        }

        [Test]
        [Row(0.0, 0)]
        [Row(0.3, 0)]
        [Row(1.0, 1)]
        public void ValidateMode(double p, double m)
        {
            var b = new Bernoulli(p);
            AssertEx.AreEqual<double>(m, b.Mode);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ValidateMedian()
        {
            var b = new Bernoulli(0.3);
            double m = b.Median;
        }

        [Test]
        public void ValidateMinimum()
        {
            var b = new Bernoulli(0.3);
            AssertEx.AreEqual<double>(0.0, b.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var b = new Bernoulli(0.3);
            AssertEx.AreEqual<double>(1.0, b.Maximum);
        }

        [Test]
        [Row(0.0, -1, 0.0)]
        [Row(0.0, 0, 1.0)]
        [Row(0.0, 1, 0.0)]
        [Row(0.0, 2, 0.0)]
        [Row(0.3, -1, 0.0)]
        [Row(0.3, 0, 0.7)]
        [Row(0.3, 1, 0.3)]
        [Row(0.3, 2, 0.0)]
        [Row(1.0, -1, 0.0)]
        [Row(1.0, 0, 0.0)]
        [Row(1.0, 1, 1.0)]
        [Row(1.0, 2, 0.0)]
        public void ValidateProbability(double p, int x, double d)
        {
            var b = new Bernoulli(p);
            AssertEx.AreEqual(d, b.Probability(x));
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
        public void ValidateProbabilityLn(double p, int x, double dln)
        {
            var b = new Bernoulli(p);
            AssertEx.AreEqual(dln, b.ProbabilityLn(x));
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = Bernoulli.Sample(new Random(), 0.3);
        }

        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Bernoulli.Samples(new Random(), 0.3);
            var arr = ied.Take(5).ToArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = Bernoulli.Sample(new Random(), -1.0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleSequenceStatic()
        {
            var ied = Bernoulli.Samples(new Random(), -1.0).First();
        }

        [Test]
        public void CanSample()
        {
            var n = new Bernoulli(0.3);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Bernoulli(0.3);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.0, -1.0, 0.0)]
        [Row(0.0, 0.0, 1.0)]
        [Row(0.0, 0.5, 1.0)]
        [Row(0.0, 1.0, 1.0)]
        [Row(0.0, 2.0, 1.0)]
        [Row(0.3, -1.0, 0.0)]
        [Row(0.3, 0.0, 0.7)]
        [Row(0.3, 0.5, 0.7)]
        [Row(0.3, 1.0, 1.0)]
        [Row(0.3, 2.0, 1.0)]
        [Row(1.0, -1.0, 0.0)]
        [Row(1.0, 0.0, 0.0)]
        [Row(1.0, 0.5, 0.0)]
        [Row(1.0, 1.0, 1.0)]
        [Row(1.0, 2.0, 1.0)]
        public void ValidateCumulativeDistribution(double p, double x, double cdf)
        {
            var b = new Bernoulli(p);
            AssertEx.AreEqual(cdf, b.CumulativeDistribution(x));
        }
    }
}