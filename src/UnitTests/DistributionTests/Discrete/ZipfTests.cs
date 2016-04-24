// <copyright file="ZipfTests.cs" company="Math.NET">
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
    /// Zipf law tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class ZipfTests
    {
        /// <summary>
        /// Can create zipf.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        [TestCase(0.1, 1)]
        [TestCase(1, 20)]
        [TestCase(1, 50)]
        public void CanCreateZipf(double s, int n)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual(s, d.S);
            Assert.AreEqual(n, d.N);
        }

        /// <summary>
        /// Zipf create fails with bad parameters.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        [TestCase(0.0, -10)]
        [TestCase(0.0, 0)]
        public void ZipfCreateFailsWithBadParameters(double s, int n)
        {
            Assert.That(() => new Zipf(s, n), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new Zipf(1.0, 5);
            Assert.AreEqual("Zipf(S = 1, N = 5)", d.ToString());
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        /// <param name="e">Expected value.</param>
        [TestCase(0.1, 1, 0.0)]
        [TestCase(0.1, 20, 2.9924075515295949)]
        [TestCase(0.1, 50, 3.9078245132371388)]
        [TestCase(1.0, 1, 0.0)]
        [TestCase(1.0, 20, 2.5279968533953743)]
        [TestCase(1.0, 50, 3.1971263138845916)]
        public void ValidateEntropy(double s, int n, double e)
        {
            var d = new Zipf(s, n);
            AssertHelpers.AlmostEqualRelative(e, d.Entropy, 15);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        [TestCase(5.0, 1)]
        [TestCase(10.0, 20)]
        [TestCase(10.0, 50)]
        public void ValidateSkewness(double s, int n)
        {
            var d = new Zipf(s, n);
            if (s > 4)
            {
                Assert.AreEqual(((SpecialFunctions.GeneralHarmonic(n, s - 3) * Math.Pow(SpecialFunctions.GeneralHarmonic(n, s), 2)) - (SpecialFunctions.GeneralHarmonic(n, s - 1) * ((3 * SpecialFunctions.GeneralHarmonic(n, s - 2) * SpecialFunctions.GeneralHarmonic(n, s)) - Math.Pow(SpecialFunctions.GeneralHarmonic(n, s - 1), 2)))) / Math.Pow((SpecialFunctions.GeneralHarmonic(n, s - 2) * SpecialFunctions.GeneralHarmonic(n, s)) - Math.Pow(SpecialFunctions.GeneralHarmonic(n, s - 1), 2), 1.5), d.Skewness);
            }
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        [TestCase(0.1, 1)]
        [TestCase(1, 20)]
        [TestCase(1, 50)]
        public void ValidateMode(double s, int n)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual(1, d.Mode);
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var d = new Zipf(1.0, 5);
            Assert.Throws<NotSupportedException>(() => { var m = d.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var d = new Zipf(1.0, 5);
            Assert.AreEqual(1, d.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        [TestCase(0.1, 1)]
        [TestCase(1, 20)]
        [TestCase(1, 50)]
        public void ValidateMaximum(double s, int n)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual(n, d.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.1, 1, 1)]
        [TestCase(1, 20, 15)]
        [TestCase(1, 50, 20)]
        public void ValidateProbability(double s, int n, int x)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual((1.0 / Math.Pow(x, s)) / SpecialFunctions.GeneralHarmonic(n, s), d.Probability(x));
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.1, 1, 1)]
        [TestCase(1, 20, 15)]
        [TestCase(1, 50, 20)]
        public void ValidateProbabilityLn(double s, int n, int x)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual(Math.Log(d.Probability(x)), d.ProbabilityLn(x));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new Zipf(0.7, 5);
            var s = d.Sample();
            Assert.LessOrEqual(s, 5);
            Assert.GreaterOrEqual(s, 0);
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var d = new Zipf(0.7, 5);
            var ied = d.Samples();
            var e = ied.Take(1000).ToArray();
            foreach (var i in e)
            {
                Assert.LessOrEqual(i, 5);
                Assert.GreaterOrEqual(i, 0);
            }
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="s">S parameter.</param>
        /// <param name="n">N parameter.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.1, 1, 2)]
        [TestCase(1, 20, 15)]
        [TestCase(1, 50, 20)]
        public void ValidateCumulativeDistribution(double s, int n, int x)
        {
            var d = new Zipf(s, n);
            var cdf = SpecialFunctions.GeneralHarmonic(x, s) / SpecialFunctions.GeneralHarmonic(n, s);
            AssertHelpers.AlmostEqualRelative(cdf, d.CumulativeDistribution(x), 14);
        }
    }
}
