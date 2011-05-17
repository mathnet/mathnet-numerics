// <copyright file="BinomialTests.cs" company="Math.NET">
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
    /// Binomial distribution tests.
    /// </summary>
    [TestFixture]
    public class BinomialTests
    {
        /// <summary>
        /// Set-up parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create binomial.
        /// </summary>
        /// <param name="p">Success probability.</param>
        /// <param name="n">Number of trials.</param>
        [TestCase(0.0, 4)]
        [TestCase(0.3, 3)]
        [TestCase(1.0, 2)]
        public void CanCreateBinomial(double p, int n)
        {
            var bernoulli = new Binomial(p, n);
            Assert.AreEqual(p, bernoulli.P);
        }

        /// <summary>
        /// Binomial create fails with bad parameters.
        /// </summary>
        /// <param name="p">Success probability.</param>
        /// <param name="n">Number of trials.</param>
        [TestCase(Double.NaN, 1)]
        [TestCase(-1.0, 1)]
        [TestCase(2.0, 1)]
        [TestCase(0.3, -2)]
        public void BinomialCreateFailsWithBadParameters(double p, int n)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Binomial(p, n));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var b = new Binomial(0.3, 2);
            Assert.AreEqual(String.Format("Binomial(Success Probability = {0}, Number of Trials = {1})", b.P, b.N), b.ToString());
        }

        /// <summary>
        /// Can set success probability.
        /// </summary>
        /// <param name="p">Success probability.</param>
        /// <param name="n">Number of trials.</param>
        [TestCase(0.0, 4)]
        [TestCase(0.3, 3)]
        [TestCase(1.0, 2)]
        public void CanSetSuccessProbability(double p, int n)
        {
            new Binomial(0.3, n)
            {
                P = p
            };
        }

        /// <summary>
        /// Set success probability fails with bad values.
        /// </summary>
        /// <param name="p">Success probability.</param>
        [TestCase(Double.NaN)]
        [TestCase(-1.0)]
        [TestCase(2.0)]
        public void SetProbabilityOfOneFails(double p)
        {
            var b = new Binomial(0.3, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => b.P = p);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="p">Success probability.</param>
        /// <param name="n">Number of trials.</param>
        /// <param name="e">Expected value.</param>
        [TestCase(0.0, 4, 0.0)]
        [TestCase(0.3, 3, 1.1404671643037712668976423399228972051669206536461)]
        [TestCase(1.0, 2, 0.0)]
        public void ValidateEntropy(double p, int n, double e)
        {
            var b = new Binomial(p, n);
            AssertHelpers.AlmostEqual(e, b.Entropy, 14);
        }

        /// <summary>
        /// Validate skewness
        /// </summary>
        /// <param name="p">Success probability.</param>
        /// <param name="n">Number of trials.</param>
        [TestCase(0.0, 4)]
        [TestCase(0.3, 3)]
        [TestCase(1.0, 2)]
        public void ValidateSkewness(double p, int n)
        {
            var b = new Binomial(p, n);
            Assert.AreEqual((1.0 - (2.0 * p)) / Math.Sqrt(n * p * (1.0 - p)), b.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="p">Success probability.</param>
        /// <param name="n">Number of trials.</param>
        /// <param name="m">Expected value.</param>
        [TestCase(0.0, 4, 0)]
        [TestCase(0.3, 3, 1)]
        [TestCase(1.0, 2, 2)]
        public void ValidateMode(double p, int n, int m)
        {
            var b = new Binomial(p, n);
            Assert.AreEqual(m, b.Mode);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var b = new Binomial(0.3, 10);
            Assert.AreEqual(0, b.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var b = new Binomial(0.3, 10);
            Assert.AreEqual(10, b.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="p">Success probability.</param>
        /// <param name="n">Number of trials.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="d">Expected value.</param>
        [TestCase(0.000000, 1, 0, 1.0)]
        [TestCase(0.000000, 1, 1, 0.0)]
        [TestCase(0.000000, 3, 0, 1.0)]
        [TestCase(0.000000, 3, 1, 0.0)]
        [TestCase(0.000000, 3, 3, 0.0)]
        [TestCase(0.000000, 10, 0, 1.0)]
        [TestCase(0.000000, 10, 1, 0.0)]
        [TestCase(0.000000, 10, 10, 0.0)]
        [TestCase(0.300000, 1, 0, 0.69999999999999995559107901499373838305473327636719)]
        [TestCase(0.300000, 1, 1, 0.2999999999999999888977697537484345957636833190918)]
        [TestCase(0.300000, 3, 0, 0.34299999999999993471888615204079956461021032657166)]
        [TestCase(0.300000, 3, 1, 0.44099999999999992772448109690231306411849135972008)]
        [TestCase(0.300000, 3, 3, 0.026999999999999997002397833512077451789759292859569)]
        [TestCase(0.300000, 10, 0, 0.02824752489999998207939855277004937778546385011091)]
        [TestCase(0.300000, 10, 1, 0.12106082099999992639752977030555903089040470780077)]
        [TestCase(0.300000, 10, 10, 0.0000059048999999999978147480206303047454017251032868501)]
        [TestCase(1.000000, 1, 0, 0.0)]
        [TestCase(1.000000, 1, 1, 1.0)]
        [TestCase(1.000000, 3, 0, 0.0)]
        [TestCase(1.000000, 3, 1, 0.0)]
        [TestCase(1.000000, 3, 3, 1.0)]
        [TestCase(1.000000, 10, 0, 0.0)]
        [TestCase(1.000000, 10, 1, 0.0)]
        [TestCase(1.000000, 10, 10, 1.0)]
        public void ValidateProbability(double p, int n, int x, double d)
        {
            var b = new Binomial(p, n);
            AssertHelpers.AlmostEqual(d, b.Probability(x), 14);
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="p">Success probability.</param>
        /// <param name="n">Number of trials.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="dln">Expected value.</param>
        [TestCase(0.000000, 1, 0, 0.0)]
        [TestCase(0.000000, 1, 1, Double.NegativeInfinity)]
        [TestCase(0.000000, 3, 0, 0.0)]
        [TestCase(0.000000, 3, 1, Double.NegativeInfinity)]
        [TestCase(0.000000, 3, 3, Double.NegativeInfinity)]
        [TestCase(0.000000, 10, 0, 0.0)]
        [TestCase(0.000000, 10, 1, Double.NegativeInfinity)]
        [TestCase(0.000000, 10, 10, Double.NegativeInfinity)]
        [TestCase(0.300000, 1, 0, -0.3566749439387324423539544041072745145718090708995)]
        [TestCase(0.300000, 1, 1, -1.2039728043259360296301803719337238685164245381839)]
        [TestCase(0.300000, 3, 0, -1.0700248318161973270618632123218235437154272126985)]
        [TestCase(0.300000, 3, 1, -0.81871040353529122294284394322574719301255212216016)]
        [TestCase(0.300000, 3, 3, -3.6119184129778080888905411158011716055492736145517)]
        [TestCase(0.300000, 10, 0, -3.566749439387324423539544041072745145718090708995)]
        [TestCase(0.300000, 10, 1, -2.1114622067804823267977785542148302920616046876506)]
        [TestCase(0.300000, 10, 10, -12.039728043259360296301803719337238685164245381839)]
        [TestCase(1.000000, 1, 0, Double.NegativeInfinity)]
        [TestCase(1.000000, 1, 1, 0.0)]
        [TestCase(1.000000, 3, 0, Double.NegativeInfinity)]
        [TestCase(1.000000, 3, 1, Double.NegativeInfinity)]
        [TestCase(1.000000, 3, 3, 0.0)]
        [TestCase(1.000000, 10, 0, Double.NegativeInfinity)]
        [TestCase(1.000000, 10, 1, Double.NegativeInfinity)]
        [TestCase(1.000000, 10, 10, 0.0)]
        public void ValidateProbabilityLn(double p, int n, int x, double dln)
        {
            var b = new Binomial(p, n);
            AssertHelpers.AlmostEqual(dln, b.ProbabilityLn(x), 14);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Binomial.Sample(new Random(), 0.3, 5);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Binomial.Samples(new Random(), 0.3, 5);
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Binomial.Sample(new Random(), -1.0, 5));
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Binomial.Samples(new Random(), -1.0, 5).First());
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Binomial(0.3, 5);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Binomial(0.3, 5);
            var ied = n.Samples();
            ied.Take(5).ToArray();
        }
    }
}
