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
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void CanCreateHypergeometric(int size, int m, int n)
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
        [TestCase(2, 3, 2)]
        [TestCase(10, 5, 20)]
        [TestCase(-2, 1, 1)]
        [TestCase(0, 1, 1)]
        public void HypergeometricCreateFailsWithBadParameters(int size, int m, int n)
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
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        public void CanSetSize(int size)
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
        [TestCase(-1)]
        [TestCase(0)]
        public void SetSizeFails(int size)
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.PopulationSize = size);
        }

        /// <summary>
        /// Can set M.
        /// </summary>
        /// <param name="m">M parameter.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CanSetm(int m)
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
        [TestCase(11)]
        [TestCase(-1)]
        public void SetmFails(int m)
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.M = m);
        }

        /// <summary>
        /// Can set N.
        /// </summary>
        /// <param name="n">N parameter.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CanSetn(int n)
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
        [TestCase(11)]
        [TestCase(-1)]
        public void SetnFails(int n)
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
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void ValidateSkewness(int size, int m, int n)
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
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void ValidateMode(int size, int m, int n)
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
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void ValidateMinimum(int size, int m, int n)
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
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void ValidateMaximum(int size, int m, int n)
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
        [TestCase(0, 0, 0, 0)]
        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 1, 1, 0)]
        [TestCase(2, 1, 1, 1)]
        [TestCase(2, 2, 2, 2)]
        [TestCase(10, 1, 1, 0)]
        [TestCase(10, 1, 1, 1)]
        [TestCase(10, 5, 3, 1)]
        [TestCase(10, 5, 3, 3)]
        public void ValidateProbability(int size, int m, int n, int x)
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
        [TestCase(0, 0, 0, 0)]
        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 1, 1, 0)]
        [TestCase(2, 1, 1, 1)]
        [TestCase(2, 2, 2, 2)]
        [TestCase(10, 1, 1, 0)]
        [TestCase(10, 1, 1, 1)]
        [TestCase(10, 5, 3, 1)]
        [TestCase(10, 5, 3, 3)]
        public void ValidateProbabilityLn(int size, int m, int n, int x)
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
        [TestCase(0, 0, 0, 0.5, 1.0)]
        [TestCase(1, 1, 1, 1.1, 1.0)]
        [TestCase(2, 1, 1, 0.3, 0.5)]
        [TestCase(2, 1, 1, 1.2, 1.0)]
        [TestCase(2, 2, 2, 2.4, 1.0)]
        [TestCase(10, 1, 1, 0.3, 0.9)]
        [TestCase(10, 1, 1, 1.2, 1.0)]
        [TestCase(10, 5, 3, 1.1, 0.5)]
        [TestCase(10, 5, 3, 3.0, 0.916666666666667)]
        public void ValidateCumulativeDistribution(int size, int m, int n, double x, double cdf)
        {
            var d = new Hypergeometric(size, m, n);
            AssertHelpers.AlmostEqual(cdf, d.CumulativeDistribution(x), 14);
        }
    }
}
