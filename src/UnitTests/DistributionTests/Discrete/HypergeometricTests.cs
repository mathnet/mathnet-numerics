// <copyright file="HypergeometricTests.cs" company="Math.NET">
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
    /// Hypergeometric tests.
    /// </summary>
    [TestFixture]
    public class HypergeometricTests
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
        /// Can create Hypergeometric.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        [Test, Sequential]
        public void CanCreateHypergeometric([Values(0, 1, 2, 2, 10, 10)] int size, [Values(0, 1, 1, 2, 1, 5)] int m, [Values(0, 1, 1, 2, 1, 3)] int n)
        {
            var d = new Hypergeometric(size, m, n);
            Assert.AreEqual(size, d.PopulationSize);
            Assert.AreEqual(m, d.M);
            Assert.AreEqual(n, d.N);
        }

        /// <summary>
        /// Hypergeometric create fails with bad parameters.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        [Test, Sequential]
        public void HypergeometricCreateFailsWithBadParameters([Values(2, 10, -2, 0)] int size, [Values(3, 5, 1, 1)] int m, [Values(2, 20, 1, 1)] int n)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Hypergeometric(size, m, n));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.AreEqual("Hypergeometric(N = 10, m = 1, n = 1)", d.ToString());
        }

        /// <summary>
        /// Can set size.
        /// </summary>
        /// <param name="size">Population size.</param>
        [Test]
        public void CanSetSize([Values(5, 10, 20)] int size)
        {
            new Hypergeometric(10, 1, 1)
            {
                PopulationSize = size
            };
        }

        /// <summary>
        /// Set size fails with bad values.
        /// </summary>
        /// <param name="size">Population size.</param>
        [Test]
        public void SetSizeFails([Values(-1, 0)] int size)
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.PopulationSize = size);
        }

        /// <summary>
        /// Can set M.
        /// </summary>
        /// <param name="m">M parameter.</param>
        [Test]
        public void CanSetm([Values(0, 1, 2, 5)] int m)
        {
            new Hypergeometric(10, 1, 1)
            {
                M = m
            };
        }

        /// <summary>
        /// Set M fails with bad values.
        /// </summary>
        /// <param name="m">M parameter.</param>
        [Test]
        public void SetmFails([Values(11, -1)] int m)
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.M = m);
        }

        /// <summary>
        /// Can set N.
        /// </summary>
        /// <param name="n">N parameter.</param>
        [Test]
        public void CanSetn([Values(0, 1, 2, 5)] int n)
        {
            new Hypergeometric(10, 1, 1)
            {
                N = n
            };
        }

        /// <summary>
        /// Set N fails with bad values.
        /// </summary>
        /// <param name="n">N parameter.</param>
        [Test]
        public void SetnFails([Values(11, -1)] int n)
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.N = n);
        }

        /// <summary>
        /// Validate entropy throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateEntropyThrowsNotSupportedException()
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<NotSupportedException>(() => { var e = d.Entropy; });
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        [Test, Sequential]
        public void ValidateSkewness([Values(0, 1, 2, 2, 10, 10)] int size, [Values(0, 1, 1, 2, 1, 5)] int m, [Values(0, 1, 1, 2, 1, 3)] int n)
        {
            var d = new Hypergeometric(size, m, n);
            Assert.AreEqual((Math.Sqrt(size - 1.0) * (size - (2 * n)) * (size - (2 * m))) / (Math.Sqrt(n * m * (size - m) * (size - n)) * (size - 2.0)), d.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        [Test, Sequential]
        public void ValidateMode([Values(0, 1, 2, 2, 10, 10)] int size, [Values(0, 1, 1, 2, 1, 5)] int m, [Values(0, 1, 1, 2, 1, 3)] int n)
        {
            var d = new Hypergeometric(size, m, n);
            Assert.AreEqual((n + 1) * (m + 1) / (size + 2), d.Mode);
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<NotSupportedException>(() => { var m = d.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        [Test, Sequential]
        public void ValidateMinimum([Values(0, 1, 2, 2, 10, 10)] int size, [Values(0, 1, 1, 2, 1, 5)] int m, [Values(0, 1, 1, 2, 1, 3)] int n)
        {
            var d = new Hypergeometric(size, m, n);
            Assert.AreEqual(Math.Max(0, n + m - size), d.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        [Test, Sequential]
        public void ValidateMaximum([Values(0, 1, 2, 2, 10, 10)] int size, [Values(0, 1, 1, 2, 1, 5)] int m, [Values(0, 1, 1, 2, 1, 3)] int n)
        {
            var d = new Hypergeometric(size, m, n);
            Assert.AreEqual(Math.Min(m, n), d.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        /// <param name="x">Input X value.</param>
        [Test, Sequential]
        public void ValidateProbability(
            [Values(0, 1, 2, 2, 2, 10, 10, 10, 10)] int size, 
            [Values(0, 1, 1, 1, 2, 1, 1, 5, 5)] int m, 
            [Values(0, 1, 1, 1, 2, 1, 1, 3, 3)] int n, 
            [Values(0, 1, 0, 1, 2, 0, 1, 1, 3)] int x)
        {
            var d = new Hypergeometric(size, m, n);
            Assert.AreEqual(SpecialFunctions.Binomial(m, x) * SpecialFunctions.Binomial(size - m, n - x) / SpecialFunctions.Binomial(size, n), d.Probability(x));
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        /// <param name="x">Input X value.</param>
        [Test, Sequential]
        public void ValidateProbabilityLn(
            [Values(0, 1, 2, 2, 2, 10, 10, 10, 10)] int size, 
            [Values(0, 1, 1, 1, 2, 1, 1, 5, 5)] int m, 
            [Values(0, 1, 1, 1, 2, 1, 1, 3, 3)] int n, 
            [Values(0, 1, 0, 1, 2, 0, 1, 1, 3)] int x)
        {
            var d = new Hypergeometric(size, m, n);
            Assert.AreEqual(Math.Log(d.Probability(x)), d.ProbabilityLn(x));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new Hypergeometric(10, 1, 1);
            d.Sample();
        }

        /// <summary>
        /// Can sample sequence
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var d = new Hypergeometric(10, 1, 1);
            var ied = d.Samples();
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="size">Population size.</param>
        /// <param name="m">M parameter.</param>
        /// <param name="n">N parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(0, 1, 2, 2, 2, 10, 10, 10, 10)] int size, 
            [Values(0, 1, 1, 1, 2, 1, 1, 5, 5)] int m, 
            [Values(0, 1, 1, 1, 2, 1, 1, 3, 3)] int n, 
            [Values(0.5, 1.1, 0.3, 1.2, 2.4, 0.3, 1.2, 1.1, 3.0)] double x, 
            [Values(1.0, 1.0, 0.5, 1.0, 1.0, 0.9, 1.0, 0.5, 0.916666666666667)] double cdf)
        {
            var d = new Hypergeometric(size, m, n);
            AssertHelpers.AlmostEqual(cdf, d.CumulativeDistribution(x), 14);
        }
    }
}
