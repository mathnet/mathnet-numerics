// <copyright file="BernoulliTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using System;
using System.Linq;
using MathNet.Numerics.Distributions;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DistributionTests.Discrete
{
    /// <summary>
    /// Bernoulli distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class BernoulliTests
    {
        /// <summary>
        /// Can create Bernoulli.
        /// </summary>
        /// <param name="p">Probability of one.</param>
        [TestCase(0.0)]
        [TestCase(0.3)]
        [TestCase(1.0)]
        public void CanCreateBernoulli(double p)
        {
            var bernoulli = new Bernoulli(p);
            Assert.AreEqual(p, bernoulli.P);
        }

        /// <summary>
        /// Bernoulli create fails with bad parameters.
        /// </summary>
        /// <param name="p">Probability of one.</param>
        [TestCase(Double.NaN)]
        [TestCase(-1.0)]
        [TestCase(2.0)]
        public void BernoulliCreateFailsWithBadParameters(double p)
        {
            Assert.That(() => new Bernoulli(p), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var b = new Bernoulli(0.3);
            Assert.AreEqual("Bernoulli(p = 0.3)", b.ToString());
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="p">Probability of one.</param>
        [TestCase(0.0)]
        [TestCase(0.3)]
        [TestCase(1.0)]
        public void ValidateEntropy(double p)
        {
            var b = new Bernoulli(p);
            AssertHelpers.AlmostEqualRelative(-((1.0 - p) * Math.Log(1.0 - p)) - (p * Math.Log(p)), b.Entropy, 14);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="p">Probability of one.</param>
        [TestCase(0.0)]
        [TestCase(0.3)]
        [TestCase(1.0)]
        public void ValidateSkewness(double p)
        {
            var b = new Bernoulli(p);
            Assert.AreEqual((1.0 - (2.0 * p)) / Math.Sqrt(p * (1.0 - p)), b.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="p">Probability of one.</param>
        /// <param name="m">Expected value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.3, 0.0)]
        [TestCase(1.0, 1.0)]
        public void ValidateMode(double p, double m)
        {
            var b = new Bernoulli(p);
            Assert.AreEqual(m, b.Mode);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(0.4, 0.0)]
        [TestCase(0.5, 0.5)]
        [TestCase(0.6, 1.0)]
        [TestCase(1.0, 1.0)]
        public void ValidateMedian(double p, double expected)
        {
            Assert.That(new Bernoulli(p).Median, Is.EqualTo(expected));
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var b = new Bernoulli(0.3);
            Assert.AreEqual(0.0, b.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var b = new Bernoulli(0.3);
            Assert.AreEqual(1.0, b.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="p">Probability of one.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="d">Expected value.</param>
        [TestCase(0.0, -1, 0.0)]
        [TestCase(0.0, 0, 1.0)]
        [TestCase(0.0, 1, 0.0)]
        [TestCase(0.0, 2, 0.0)]
        [TestCase(0.3, -1, 0.0)]
        [TestCase(0.3, 0, 0.7)]
        [TestCase(0.3, 1, 0.3)]
        [TestCase(0.3, 2, 0.0)]
        [TestCase(1.0, -1, 0.0)]
        [TestCase(1.0, 0, 0.0)]
        [TestCase(1.0, 1, 1.0)]
        [TestCase(1.0, 2, 0.0)]
        public void ValidateProbability(double p, int x, double d)
        {
            var b = new Bernoulli(p);
            Assert.AreEqual(d, b.Probability(x));
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="p">Probability of one.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="dln">Expected value.</param>
        [TestCase(0.0, -1, Double.NegativeInfinity)]
        [TestCase(0.0, 0, 0.0)]
        [TestCase(0.0, 1, Double.NegativeInfinity)]
        [TestCase(0.0, 2, Double.NegativeInfinity)]
        [TestCase(0.3, -1, Double.NegativeInfinity)]
        [TestCase(0.3, 0, -0.35667494393873244235395440410727451457180907089949815)]
        [TestCase(0.3, 1, -1.2039728043259360296301803719337238685164245381839102)]
        [TestCase(0.3, 2, Double.NegativeInfinity)]
        [TestCase(1.0, -1, Double.NegativeInfinity)]
        [TestCase(1.0, 0, Double.NegativeInfinity)]
        [TestCase(1.0, 1, 0.0)]
        [TestCase(1.0, 2, Double.NegativeInfinity)]
        public void ValidateProbabilityLn(double p, int x, double dln)
        {
            var b = new Bernoulli(p);
            Assert.AreEqual(dln, b.ProbabilityLn(x));
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Bernoulli.Sample(new System.Random(0), 0.3);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Bernoulli.Samples(new System.Random(0), 0.3);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad values.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => Bernoulli.Sample(new System.Random(0), -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad values.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => Bernoulli.Samples(new System.Random(0), -1.0).First(), Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Bernoulli(0.3);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Bernoulli(0.3);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="p">Probability of one.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [TestCase(0.0, -1.0, 0.0)]
        [TestCase(0.0, 0.0, 1.0)]
        [TestCase(0.0, 0.5, 1.0)]
        [TestCase(0.0, 1.0, 1.0)]
        [TestCase(0.0, 2.0, 1.0)]
        [TestCase(0.3, -1.0, 0.0)]
        [TestCase(0.3, 0.0, 0.7)]
        [TestCase(0.3, 0.5, 0.7)]
        [TestCase(0.3, 1.0, 1.0)]
        [TestCase(0.3, 2.0, 1.0)]
        [TestCase(1.0, -1.0, 0.0)]
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(1.0, 0.5, 0.0)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(1.0, 2.0, 1.0)]
        public void ValidateCumulativeDistribution(double p, double x, double cdf)
        {
            var b = new Bernoulli(p);
            Assert.AreEqual(cdf, b.CumulativeDistribution(x));
        }
    }
}
