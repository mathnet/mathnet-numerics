// <copyright file="NegativeBinomialTests.cs" company="Math.NET">
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
    /// Negative Binomial distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class NegativeBinomialTests
    {
        /// <summary>
        /// Can create Negative Binomial.
        /// </summary>
        /// <param name="r">Number of trials.</param>
        /// <param name="p">Probability of success.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.1, 0.3)]
        [TestCase(1.0, 1.0)]
        public void CanCreateNegativeBinomial(double r, double p)
        {
            var d = new NegativeBinomial(r, p);
            Assert.AreEqual(r, d.R);
            Assert.AreEqual(p, d.P);
        }

        /// <summary>
        /// <c>NegativeBinomial</c> create fails with bad parameters.
        /// </summary>
        /// <param name="r">Number of trials.</param>
        /// <param name="p">Probability of success.</param>
        [TestCase(0.0, Double.NaN)]
        [TestCase(0.0, -1.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(Double.NegativeInfinity, 0.0)]
        [TestCase(-1.0, 0.3)]
        [TestCase(Double.NaN, 1.0)]
        [TestCase(Double.NegativeInfinity, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN)]
        public void NegativeBinomialCreateFailsWithBadParameters(double r, double p)
        {
            Assert.That(() => new NegativeBinomial(r, p), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new NegativeBinomial(1.0, 0.3);
            Assert.AreEqual(String.Format("NegativeBinomial(R = {0}, P = {1})", d.R, d.P), d.ToString());
        }

        /// <summary>
        /// Validate entropy throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateEntropyThrowsNotSupportedException()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            Assert.Throws<NotSupportedException>(() => { var e = d.Entropy; });
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="r">Number of trials.</param>
        /// <param name="p">Probability of success.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.1, 0.3)]
        [TestCase(1.0, 1.0)]
        public void ValidateSkewness(double r, double p)
        {
            var b = new NegativeBinomial(r, p);
            Assert.AreEqual((2.0 - p) / Math.Sqrt(r * (1.0 - p)), b.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="r">Number of trials.</param>
        /// <param name="p">Probability of success.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.3, 0.0)]
        [TestCase(1.0, 1.0)]
        public void ValidateMode(double r, double p)
        {
            var d = new NegativeBinomial(r, p);
            if (r > 1)
            {
                Assert.AreEqual((int)Math.Floor((r - 1.0) * (1.0 - p) / p), d.Mode);
            }
            else
            {
                Assert.AreEqual(0.0, d.Mode);
            }
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            Assert.Throws<NotSupportedException>(() => { var m = d.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            Assert.AreEqual(0, d.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var d = new NegativeBinomial(1.0, 0.3);
            Assert.AreEqual(int.MaxValue, d.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="r">Number of trials.</param>
        /// <param name="p">Probability of success.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.0, 0)]
        [TestCase(0.1, 0.3, 1)]
        [TestCase(1.0, 1.0, 2)]
        [TestCase(1.0, 1.0, 3)]
        [TestCase(1.0, 1.0, 5)]
        public void ValidateProbability(double r, double p, int x)
        {
            var d = new NegativeBinomial(r, p);
            Assert.AreEqual(Math.Exp(SpecialFunctions.GammaLn(r + x) - SpecialFunctions.GammaLn(r) - SpecialFunctions.GammaLn(x + 1.0) + (r * Math.Log(p)) + (x * Math.Log(1.0 - p))), d.Probability(x));
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="r">Number of trials.</param>
        /// <param name="p">Probability of success.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.0, 0)]
        [TestCase(0.1, 0.3, 1)]
        [TestCase(1.0, 1.0, 2)]
        [TestCase(1.0, 1.0, 3)]
        [TestCase(1.0, 1.0, 5)]
        public void ValidateProbabilityLn(double r, double p, int x)
        {
            var d = new NegativeBinomial(r, p);
            Assert.AreEqual(SpecialFunctions.GammaLn(r + x) - SpecialFunctions.GammaLn(r) - SpecialFunctions.GammaLn(x + 1.0) + (r * Math.Log(p)) + (x * Math.Log(1.0 - p)), d.ProbabilityLn(x));
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="r">Number of trials.</param>
        /// <param name="p">Probability of success.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, 0.0, 0)]
        [TestCase(0.1, 0.3, 1)]
        [TestCase(1.0, 1.0, 2)]
        [TestCase(1.0, 1.0, 3)]
        [TestCase(1.0, 1.0, 5)]
        public void ValidateCumulativeDistribution(double r, double p, int x)
        {
            var d = new NegativeBinomial(r, p);
            Assert.AreEqual(SpecialFunctions.BetaRegularized(r, x + 1.0, p), d.CumulativeDistribution(x), 1e-12);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            d.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            var ied = d.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }
    }
}
