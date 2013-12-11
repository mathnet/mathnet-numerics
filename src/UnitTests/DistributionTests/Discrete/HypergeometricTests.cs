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
    [TestFixture, Category("Distributions")]
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
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="draws">N parameter.</param>
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void CanCreateHypergeometric(int population, int success, int draws)
        {
            var d = new Hypergeometric(population, success, draws);
            Assert.AreEqual(population, d.Population);
            Assert.AreEqual(success, d.Success);
            Assert.AreEqual(draws, d.Draws);
        }

        /// <summary>
        /// Hypergeometric create fails with bad parameters.
        /// </summary>
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="n">N parameter.</param>
        [TestCase(2, 3, 2)]
        [TestCase(10, 5, 20)]
        [TestCase(-2, 1, 1)]
        [TestCase(0, 1, 1)]
        public void HypergeometricCreateFailsWithBadParameters(int population, int success, int n)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Hypergeometric(population, success, n));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.AreEqual("Hypergeometric(N = 10, M = 1, n = 1)", d.ToString());
        }

        /// <summary>
        /// Can set size.
        /// </summary>
        /// <param name="population">Population size.</param>
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        public void CanSetPopulation(int population)
        {
            new Hypergeometric(10, 1, 1)
                {
                    Population = population
                };
        }

        /// <summary>
        /// Set size fails with bad values.
        /// </summary>
        /// <param name="population">Population size.</param>
        [TestCase(-1)]
        [TestCase(0)]
        public void SetPopulationFails(int population)
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.Population = population);
        }

        /// <summary>
        /// Can set M.
        /// </summary>
        /// <param name="success">M parameter.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CanSetSuccess(int success)
        {
            new Hypergeometric(10, 1, 1)
                {
                    Success = success
                };
        }

        /// <summary>
        /// Set M fails with bad values.
        /// </summary>
        /// <param name="success">M parameter.</param>
        [TestCase(11)]
        [TestCase(-1)]
        public void SetSuccessFails(int success)
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.Success = success);
        }

        /// <summary>
        /// Can set N.
        /// </summary>
        /// <param name="draws">N parameter.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CanSetDraws(int draws)
        {
            new Hypergeometric(10, 1, 1)
                {
                    Draws = draws
                };
        }

        /// <summary>
        /// Set N fails with bad values.
        /// </summary>
        /// <param name="draws">N parameter.</param>
        [TestCase(11)]
        [TestCase(-1)]
        public void SetDrawsFails(int draws)
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.Draws = draws);
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
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="draws">N parameter.</param>
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void ValidateSkewness(int population, int success, int draws)
        {
            var d = new Hypergeometric(population, success, draws);
            Assert.AreEqual((Math.Sqrt(population - 1.0)*(population - (2*draws))*(population - (2*success)))/(Math.Sqrt(draws*success*(population - success)*(population - draws))*(population - 2.0)), d.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="draws">N parameter.</param>
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void ValidateMode(int population, int success, int draws)
        {
            var d = new Hypergeometric(population, success, draws);
            Assert.AreEqual((draws + 1)*(success + 1)/(population + 2), d.Mode);
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
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="draws">N parameter.</param>
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void ValidateMinimum(int population, int success, int draws)
        {
            var d = new Hypergeometric(population, success, draws);
            Assert.AreEqual(Math.Max(0, draws + success - population), d.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="draws">N parameter.</param>
        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(10, 1, 1)]
        [TestCase(10, 5, 3)]
        public void ValidateMaximum(int population, int success, int draws)
        {
            var d = new Hypergeometric(population, success, draws);
            Assert.AreEqual(Math.Min(success, draws), d.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="draws">N parameter.</param>
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
        public void ValidateProbability(int population, int success, int draws, int x)
        {
            var d = new Hypergeometric(population, success, draws);
            Assert.AreEqual(SpecialFunctions.Binomial(success, x)*SpecialFunctions.Binomial(population - success, draws - x)/SpecialFunctions.Binomial(population, draws), d.Probability(x));
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="draws">N parameter.</param>
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
        public void ValidateProbabilityLn(int population, int success, int draws, int x)
        {
            var d = new Hypergeometric(population, success, draws);
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
        /// <param name="population">Population size.</param>
        /// <param name="success">M parameter.</param>
        /// <param name="draws">N parameter.</param>
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
        [TestCase(10, 5, 3, 2.0, 11.0/12.0)]
        [TestCase(10, 5, 3, 3.0, 1.0)]
        [TestCase(10000, 2, 9800, 0.0, 199.0/499950.0)]
        [TestCase(10000, 2, 9800, 0.5, 199.0/499950.0)]
        [TestCase(10000, 2, 9800, 1.5, 19799.0/499950.0)]
        public void ValidateCumulativeDistribution(int population, int success, int draws, double x, double cdf)
        {
            var d = new Hypergeometric(population, success, draws);
            AssertHelpers.AlmostEqualRelative(cdf, d.CumulativeDistribution(x), 9);
        }

        [Test]
        public void CumulativeDistributionMustNotOverflow_CodePlexIssue5729()
        {
            var d = new Hypergeometric(10000, 2, 9800);
            Assert.That(d.CumulativeDistribution(0.0), Is.Not.NaN);
            Assert.That(d.CumulativeDistribution(0.1), Is.Not.NaN);
        }
    }
}
