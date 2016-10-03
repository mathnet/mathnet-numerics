// <copyright file="GeometricTests.cs" company="Math.NET">
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
    /// Geometric distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class GeometricTests
    {
        /// <summary>
        /// Can create Geometric.
        /// </summary>
        /// <param name="p">Probability of generating a one.</param>
        [TestCase(0.0)]
        [TestCase(0.3)]
        [TestCase(1.0)]
        public void CanCreateGeometric(double p)
        {
            var d = new Geometric(p);
            Assert.AreEqual(p, d.P);
        }

        /// <summary>
        /// Geometric create fails with bad parameters.
        /// </summary>
        /// <param name="p">Probability of generating a one.</param>
        [TestCase(Double.NaN)]
        [TestCase(-1.0)]
        [TestCase(2.0)]
        public void GeometricCreateFailsWithBadParameters(double p)
        {
            Assert.That(() => new Geometric(p), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new Geometric(0.3);
            Assert.AreEqual("Geometric(p = 0.3)", d.ToString());
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="p">Probability of generating a one.</param>
        [TestCase(0.0)]
        [TestCase(0.3)]
        [TestCase(1.0)]
        public void ValidateEntropy(double p)
        {
            var d = new Geometric(p);
            Assert.AreEqual(((-p * Math.Log(p, 2.0)) - ((1.0 - p) * Math.Log(1.0 - p, 2.0))) / p, d.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="p">Probability of generating a one.</param>
        [TestCase(0.0)]
        [TestCase(0.3)]
        [TestCase(1.0)]
        public void ValidateSkewness(double p)
        {
            var d = new Geometric(p);
            Assert.AreEqual((2.0 - p) / Math.Sqrt(1.0 - p), d.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="p">Probability of generating a one.</param>
        [TestCase(0.0)]
        [TestCase(0.3)]
        [TestCase(1.0)]
        public void ValidateMode(double p)
        {
            var d = new Geometric(p);
            Assert.AreEqual(1, d.Mode);
        }

        [TestCase(0.0, double.PositiveInfinity)]
        [TestCase(0.0001, 6932.0)]
        [TestCase(0.1, 7.0)]
        [TestCase(0.3, 2.0)]
        [TestCase(0.9, 1.0)]
        [TestCase(1.0, 1.0)]
        public void ValidateMedian(double p, double expected)
        {
            Assert.That(new Geometric(p).Median, Is.EqualTo(expected));
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var d = new Geometric(0.3);
            Assert.AreEqual(1.0, d.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var d = new Geometric(0.3);
            Assert.AreEqual(int.MaxValue, d.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="p">Probability of generating a one.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, -1)]
        [TestCase(0.3, 0)]
        [TestCase(1.0, 1)]
        [TestCase(1.0, 2)]
        public void ValidateProbability(double p, int x)
        {
            var d = new Geometric(p);
            if (x > 0)
            {
                Assert.AreEqual(Math.Pow(1.0 - p, x - 1) * p, d.Probability(x));
            }
            else
            {
                Assert.AreEqual(0.0, d.Probability(x));
            }
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="p">Probability of generating a one.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pln">Expected value.</param>
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
        public void ValidateProbabilityLn(double p, int x, double pln)
        {
            var d = new Geometric(p);
            if (x > 0)
            {
                Assert.AreEqual(((x - 1) * Math.Log(1.0 - p)) + Math.Log(p), d.ProbabilityLn(x));
            }
            else
            {
                Assert.AreEqual(Double.NegativeInfinity, d.ProbabilityLn(x));
            }
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new Geometric(0.3);
            d.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var d = new Geometric(0.3);
            var ied = d.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="p">Probability of generating a one.</param>
        /// <param name="x">Input X value.</param>
        [TestCase(0.0, -1)]
        [TestCase(0.3, 0)]
        [TestCase(1.0, 1)]
        [TestCase(1.0, 2)]
        public void ValidateCumulativeDistribution(double p, int x)
        {
            var d = new Geometric(p);
            Assert.AreEqual(1.0 - Math.Pow(1.0 - p, x), d.CumulativeDistribution(x));
        }
    }
}
