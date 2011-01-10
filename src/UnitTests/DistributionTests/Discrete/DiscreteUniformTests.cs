// <copyright file="DiscreteUniformTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
    using Distributions;
    using NUnit.Framework;

    /// <summary>
    /// Discrete uniform tests.
    /// </summary>
    [TestFixture]
    public class DiscreteUniformTests
    {
        /// <summary>
        /// Set-up tests parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create discrete uniform.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        [Test, Sequential]
        public void CanCreateDiscreteUniform([Values(-10, 0, 10, 20)] int l, [Values(10, 4, 20, 20)] int u)
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
        [Test, Sequential]
        public void DiscreteUniformCreateFailsWithBadParameters([Values(-1, 6)] int l, [Values(-2, 5)] int u)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DiscreteUniform(l, u));
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
        /// Can set lower bound.
        /// </summary>
        /// <param name="p">Lower bound.</param>
        [Test]
        public void CanSetLowerBound([Values(0, 3, 10)] int p)
        {
            new DiscreteUniform(0, 10)
            {
                LowerBound = p
            };
        }

        /// <summary>
        /// Can set upper bound.
        /// </summary>
        /// <param name="p">Upper bound.</param>
        [Test]
        public void CanSetUpperBound([Values(0, 3, 10)] int p)
        {
            new DiscreteUniform(0, 10)
            {
                UpperBound = p
            };
        }

        /// <summary>
        /// Set lower bound with bad values fails.
        /// </summary>
        /// <param name="p">Lower bound.</param>
        [Test]
        public void SetLowerBoundFails([Values(11, 20)] int p)
        {
            var b = new DiscreteUniform(0, 10);
            Assert.Throws<ArgumentOutOfRangeException>(() => b.LowerBound = p);
        }

        /// <summary>
        /// Set upper bound with bad values fails
        /// </summary>
        /// <param name="p">Upper bound.</param>
        [Test]
        public void SetUpperBoundFails([Values(-11, -20)] int p)
        {
            var b = new DiscreteUniform(0, 10);
            Assert.Throws<ArgumentOutOfRangeException>(() => b.UpperBound = p);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="e">Expceted value.</param>
        [Test, Sequential]
        public void ValidateEntropy([Values(-10, 0, 10, 20)] int l, [Values(10, 4, 20, 20)] int u, [Values(3.0445224377234229965005979803657054342845752874046093, 1.6094379124341003746007593332261876395256013542685181, 2.3978952727983705440619435779651292998217068539374197, 0.0)] double e)
        {
            var du = new DiscreteUniform(l, u);
            AssertHelpers.AlmostEqual(e, du.Entropy, 14);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        [Test, Sequential]
        public void ValidateSkewness([Values(-10, 0, 10, 20)] int l, [Values(10, 4, 20, 20)] int u)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(0.0, du.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="m">Expceted value.</param>
        [Test, Sequential]
        public void ValidateMode([Values(-10, 0, 10, 20)] int l, [Values(10, 4, 20, 20)] int u, [Values(0, 2, 15, 20)] int m)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(m, du.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="m">Expceted value.</param>
        [Test, Sequential]
        public void ValidateMedian([Values(-10, 0, 10, 20)] int l, [Values(10, 4, 20, 20)] int u, [Values(0, 2, 15, 20)] int m)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(m, du.Median);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="m">Expceted value.</param>
        [Test, Sequential]
        public void ValidateMean([Values(-10, 0, 10, 20)] int l, [Values(10, 4, 20, 20)] int u, [Values(0, 2, 15, 20)] int m)
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
        /// <param name="p">Expceted value.</param>
        [Test, Sequential]
        public void ValidateProbability([Values(-10, -10, -10, -10, -10)] int l, [Values(10, 10, 10, -10, -10)] int u, [Values(-5, 1, 10, 0, -10)] int x, [Values(1 / 21.0, 1 / 21.0, 1 / 21.0, 0.0, 1.0)] double p)
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
        /// <param name="dln">Expceted value.</param>
        [Test, Sequential]
        public void ValidateProbabilityLn(
            [Values(-10, -10, -10, -10, -10)] int l, 
            [Values(10, 10, 10, -10, -10)] int u, 
            [Values(-5, 1, 10, 0, -10)] int x, 
            [Values(-3.0445224377234229965005979803657054342845752874046093, -3.0445224377234229965005979803657054342845752874046093, -3.0445224377234229965005979803657054342845752874046093, Double.NegativeInfinity, 0.0)] double dln)
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
            DiscreteUniform.Sample(new Random(), 0, 10);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = DiscreteUniform.Samples(new Random(), 0, 10);
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiscreteUniform.Sample(new Random(), 20, 10));
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiscreteUniform.Samples(new Random(), 20, 10).First());
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
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="l">Lower bound.</param>
        /// <param name="u">Upper bound.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expceted value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(-10, -10, -10, -10, -10, -10)] int l, 
            [Values(10, 10, 10, -10, -10, -10)] int u, 
            [Values(-5, 1, 10, 0, -10, -11)] double x, 
            [Values(6.0 / 21.0, 12.0 / 21.0, 1.0, 1.0, 1.0, 0.0)] double cdf)
        {
            var b = new DiscreteUniform(l, u);
            Assert.AreEqual(cdf, b.CumulativeDistribution(x));
        }
    }
}
