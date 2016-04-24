// <copyright file="DiscreteUniformTests.cs" company="Math.NET">
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
    /// Discrete uniform tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class DiscreteUniformTests
    {
        /// <summary>
        /// Can create discrete uniform.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        [TestCase(-10, 10)]
        [TestCase(0, 4)]
        [TestCase(10, 20)]
        [TestCase(20, 20)]
        public void CanCreateDiscreteUniform(int l, int u)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(l, du.LowerBound);
            Assert.AreEqual(u, du.UpperBound);
        }

        /// <summary>
        /// Discrete Uniform create fails with bad parameters.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        [TestCase(-1, -2)]
        [TestCase(6, 5)]
        public void DiscreteUniformCreateFailsWithBadParameters(int l, int u)
        {
            Assert.That(() => new DiscreteUniform(l, u), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var b = new DiscreteUniform(0, 10);
            Assert.AreEqual("DiscreteUniform(Lower = 0, Upper = 10)", b.ToString());
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="e">Expected value.</param>
        [TestCase(-10, 10, 3.0445224377234229965005979803657054342845752874046093)]
        [TestCase(0, 4, 1.6094379124341003746007593332261876395256013542685181)]
        [TestCase(10, 20, 2.3978952727983705440619435779651292998217068539374197)]
        [TestCase(20, 20, 0.0)]
        public void ValidateEntropy(int l, int u, double e)
        {
            var du = new DiscreteUniform(l, u);
            AssertHelpers.AlmostEqualRelative(e, du.Entropy, 14);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        [TestCase(-10, 10)]
        [TestCase(0, 4)]
        [TestCase(10, 20)]
        [TestCase(20, 20)]
        public void ValidateSkewness(int l, int u)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(0.0, du.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="m">Expected value.</param>
        [TestCase(-10, 10, 0)]
        [TestCase(0, 4, 2)]
        [TestCase(10, 20, 15)]
        [TestCase(20, 20, 20)]
        public void ValidateMode(int l, int u, int m)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(m, du.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="m">Expected value.</param>
        [TestCase(-10, 10, 0)]
        [TestCase(0, 4, 2)]
        [TestCase(10, 20, 15)]
        [TestCase(20, 20, 20)]
        public void ValidateMedian(int l, int u, int m)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(m, du.Median);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="m">Expected value.</param>
        [TestCase(-10, 10, 0)]
        [TestCase(0, 4, 2)]
        [TestCase(10, 20, 15)]
        [TestCase(20, 20, 20)]
        public void ValidateMean(int l, int u, int m)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(m, du.Mean);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var b = new DiscreteUniform(-10, 10);
            Assert.AreEqual(-10, b.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var b = new DiscreteUniform(-10, 10);
            Assert.AreEqual(10, b.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(-10, 10, -5, 1 / 21.0)]
        [TestCase(-10, 10, 1, 1 / 21.0)]
        [TestCase(-10, 10, 10, 1 / 21.0)]
        [TestCase(-10, -10, 0, 0.0)]
        [TestCase(-10, -10, -10, 1.0)]
        public void ValidateProbability(int l, int u, int x, double p)
        {
            var b = new DiscreteUniform(l, u);
            Assert.AreEqual(p, b.Probability(x));
        }

        /// <summary>
        /// Validate porbability log.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="dln">Expected value.</param>
        [TestCase(-10, 10, -5, -3.0445224377234229965005979803657054342845752874046093)]
        [TestCase(-10, 10, 1, -3.0445224377234229965005979803657054342845752874046093)]
        [TestCase(-10, 10, 10, -3.0445224377234229965005979803657054342845752874046093)]
        [TestCase(-10, -10, 0, Double.NegativeInfinity)]
        [TestCase(-10, -10, -10, 0.0)]
        public void ValidateProbabilityLn(int l, int u, int x, double dln)
        {
            var b = new DiscreteUniform(l, u);
            Assert.AreEqual(dln, b.ProbabilityLn(x));
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            DiscreteUniform.Sample(new System.Random(0), 0, 10);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = DiscreteUniform.Samples(new System.Random(0), 0, 10);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => DiscreteUniform.Sample(new System.Random(0), 20, 10), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => DiscreteUniform.Samples(new System.Random(0), 20, 10).First(), Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new DiscreteUniform(0, 10);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new DiscreteUniform(0, 10);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [TestCase(-10, 10, -5, 6.0 / 21.0)]
        [TestCase(-10, 10, 1, 12.0 / 21.0)]
        [TestCase(-10, 10, 10, 1.0)]
        [TestCase(-10, -10, 0, 1.0)]
        [TestCase(-10, -10, -10, 1.0)]
        [TestCase(-10, -10, -11, 0.0)]
        public void ValidateCumulativeDistribution(int l, int u, double x, double cdf)
        {
            var b = new DiscreteUniform(l, u);
            Assert.AreEqual(cdf, b.CumulativeDistribution(x));
        }
    }
}
