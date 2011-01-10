// <copyright file="PoissonTests.cs" company="Math.NET">
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
    /// Poisson distribution tests.
    /// </summary>
    [TestFixture]
    public class PoissonTests
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
        /// Can create Poisson.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void CanCreatePoisson([Values(1.5, 5.4, 10.8)] double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual(lambda, d.Lambda);
        }

        /// <summary>
        /// Poisson create fails with bad parameters.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void PoissonCreateFailsWithBadParameters([Values(Double.NaN, -1.5, 0.0)] double lambda)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Poisson(lambda));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new Poisson(0.3);
            Assert.AreEqual(String.Format("Poisson(λ = {0})", 0.3), d.ToString());
        }

        /// <summary>
        /// Can set probability of one.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void CanSetProbabilityOfOne([Values(1.5, 5.4, 10.8)] double lambda)
        {
            new Poisson(0.3)
            {
                Lambda = lambda
            };
        }

        /// <summary>
        /// Set probability of one fails with bad value.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void SetProbabilityOfOneFails([Values(Double.NaN, -1.5, 0.0)] double lambda)
        {
            var d = new Poisson(0.3);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.Lambda = lambda);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void ValidateEntropy([Values(1.5, 5.4, 10.8)] double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual((0.5 * Math.Log(2 * Math.PI * Math.E * lambda)) - (1.0 / (12.0 * lambda)) - (1.0 / (24.0 * lambda * lambda)) - (19.0 / (360.0 * lambda * lambda * lambda)), d.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void ValidateSkewness([Values(1.5, 5.4, 10.8)] double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual(1.0 / Math.Sqrt(lambda), d.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void ValidateMode([Values(1.5, 5.4, 10.8)] double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual((int)Math.Floor(lambda), d.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void ValidateMedian([Values(1.5, 5.4, 10.8)] double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual((int)Math.Floor(lambda + (1.0 / 3.0) - (0.02 / lambda)), d.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var d = new Poisson(0.3);
            Assert.AreEqual(0, d.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var d = new Poisson(0.3);
            Assert.AreEqual(int.MaxValue, d.Maximum);
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="result">Expected value.</param>
        [Test, Sequential]
        public void ValidateProbability(
            [Values(1.5, 1.5, 1.5, 5.4, 5.4, 5.4, 10.8, 10.8, 10.8)] double lambda, 
            [Values(1, 10, 20, 1, 10, 20, 1, 10, 20)] int x, 
            [Values(0.334695240222645000000000000000, 0.000003545747740570180000000000, 0.000000000000000304971208961018, 0.024389537090108400000000000000, 0.026241240591792300000000000000, 0.000000825202200316548000000000, 0.000220314636840657000000000000, 0.121365183659420000000000000000, 0.003908139778574110000000000000)] double result)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual(d.Probability(x), result, 1e-12);
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="result">Expected value.</param>
        [Test, Sequential]
        public void ValidateProbabilityLn(
            [Values(1.5, 1.5, 1.5, 5.4, 5.4, 5.4, 10.8, 10.8, 10.8)] double lambda, 
            [Values(1, 10, 20, 1, 10, 20, 1, 10, 20)] int x, 
            [Values(0.334695240222645000000000000000, 0.000003545747740570180000000000, 0.000000000000000304971208961018, 0.024389537090108400000000000000, 0.026241240591792300000000000000, 0.000000825202200316548000000000, 0.000220314636840657000000000000, 0.121365183659420000000000000000, 0.003908139778574110000000000000)] double result)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual(d.ProbabilityLn(x), Math.Log(result), 1e-12);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new Poisson(0.3);
            d.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var d = new Poisson(0.3);
            var ied = d.Samples();
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="result">Expected value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(1.5, 1.5, 1.5, 5.4, 5.4, 5.4, 10.8, 10.8, 10.8)] double lambda, 
            [Values(1, 10, 20, 1, 10, 20, 1, 10, 20)] int x, 
            [Values(0.5578254003710750000000, 0.9999994482467640000000, 1.0000000000000000000000, 0.0289061180327211000000, 0.9774863006897650000000, 0.9999997199928290000000, 0.0002407141402518290000, 0.4839692359955690000000, 0.9961800769608090000000)] double result)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual(d.CumulativeDistribution(x), result, 1e-12);
        }
    }
}
