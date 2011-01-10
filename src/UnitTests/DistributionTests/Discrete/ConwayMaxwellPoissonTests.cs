// <copyright file="ConwayMaxwellPoissonTests.cs" company="Math.NET">
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
    /// Conway-Maxwell-Poisson tests.
    /// </summary>
    [TestFixture]
    public class ConwayMaxwellPoissonTests
    {
        /// <summary>
        /// Set-up test parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create <c>ConwayMaxwellPoisson</c>.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="nu">Nu parameter.</param>
        [Test, Combinatorial]
        public void CanCreateConwayMaxwellPoisson([Values(0.1, 1.0, 2.5, 10.0, Double.PositiveInfinity)] double lambda, [Values(0.0, 2.5, Double.PositiveInfinity)] double nu)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            Assert.AreEqual(lambda, d.Lambda);
            Assert.AreEqual(nu, d.Nu);
        }

        /// <summary>
        /// <c>ConwayMaxwellPoisson</c> create fails with bad parameters.
        /// </summary>
        [Test]
        public void ConwayMaxwellPoissonCreateFailsWithBadParameters()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConwayMaxwellPoisson(-1.0, -2.0));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.AreEqual("ConwayMaxwellPoisson(Lambda = 1, Nu = 2)", d.ToString());
        }

        /// <summary>
        /// Can set lambda.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void CanSetLambda([Values(0.1, 3.0, 10.0, Double.PositiveInfinity)] double lambda)
        {
            new ConwayMaxwellPoisson(1.0, 2.0)
            {
                Lambda = lambda
            };
        }

        /// <summary>
        /// Can set Nu.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        [Test]
        public void CanSetNu([Values(0.0, 3.0, 10.0, Double.PositiveInfinity)] double nu)
        {
            new ConwayMaxwellPoisson(1.0, 2.0)
            {
                Nu = nu
            };
        }

        /// <summary>
        /// Set lambda with bad values fails.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        [Test]
        public void SetLambdaFails([Values(0.0, -0.0, -1.0, Double.NegativeInfinity)] double lambda)
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.Lambda = lambda);
        }

        /// <summary>
        /// Set Nu with bad values fails.
        /// </summary>
        /// <param name="nu">Nu parameter.</param>
        [Test]
        public void SetNuFails([Values(-0.1, -1.0, -10.0, Double.NegativeInfinity)] double nu)
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.Nu = nu);
        }

        /// <summary>
        /// Validate entropy throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateEntropyThrowsNotSupportedException()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.Throws<NotSupportedException>(() => { var e = d.Entropy; });
        }

        /// <summary>
        /// Validate skewness throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateSkewnessThrowsNotSupportedException()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.Throws<NotSupportedException>(() => { var s = d.Skewness; });
        }

        /// <summary>
        /// Validate mode throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateModeThrowsNotSupportedException()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.Throws<NotSupportedException>(() => { var m = d.Mode; });
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.Throws<NotSupportedException>(() => { var m = d.Median; });
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="mean">Expected value.</param>
        [Test, Sequential]
        public void ValidateMean(
            [Values(1, 2, 10, 20, 1, 2)] int lambda, 
            [Values(1, 1, 1, 1, 2, 2)] int nu, 
            [Values(1.0, 2.0, 10.0, 20.0, 0.697774657964008, 1.12635723962342)] double mean)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqual(mean, d.Mean, 10);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.AreEqual(0.0, d.Minimum);
        }

        /// <summary>
        /// Validate maximum throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMaximumThrowsNotSupportedException()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.Throws<NotSupportedException>(() => { var max = d.Maximum; });
        }

        /// <summary>
        /// Validate probability.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [Test, Sequential]
        public void ValidateProbability(
            [Values(1.0, 1.0, 2.0, 2.0, 2.0, 2.0)] double lambda, 
            [Values(1.0, 1.0, 1.0, 1.0, 2.0, 2.0)] double nu, 
            [Values(1, 2, 1, 2, 1, 3)] int x, 
            [Values(0.367879441171442, 0.183939720585721, 0.270670566473225, 0.270670566473225, 0.470328074204904, 0.052258674911656)] double p)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqual(p, d.Probability(x), 13);
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pln">Expected value.</param>
        [Test, Sequential]
        public void ValidateProbabilityLn(
            [Values(1.0, 1.0, 2.0, 2.0, 2.0, 2.0)] double lambda, 
            [Values(1.0, 1.0, 1.0, 1.0, 2.0, 2.0)] double nu, 
            [Values(1, 2, 1, 2, 1, 3)] int x, 
            [Values(-1.0, -1.69314718055995, -1.30685281944005, -1.30685281944005, -0.754324797564617, -2.95154937490084)] double pln)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqual(pln, d.ProbabilityLn(x), 13);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            d.Sample();
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            var ied = d.Samples();
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="nu">Nu parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(1.0, 1.0, 2.0, 2.0, 2.0, 2.0)] double lambda, 
            [Values(1.0, 1.0, 1.0, 1.0, 2.0, 2.0)] double nu, 
            [Values(1, 2, 1, 2, 1, 3)] int x, 
            [Values(0.735758882342885, 0.919698602928606, 0.406005849709838, 0.676676416183064, 0.705492111307356, 0.992914823321464)] double cdf)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqual(cdf, d.CumulativeDistribution(x), 13);
        }
    }
}
