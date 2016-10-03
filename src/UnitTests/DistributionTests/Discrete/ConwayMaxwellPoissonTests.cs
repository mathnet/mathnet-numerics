// <copyright file="ConwayMaxwellPoissonTests.cs" company="Math.NET">
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
    /// Conway-Maxwell-Poisson tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class ConwayMaxwellPoissonTests
    {
        /// <summary>
        /// Can create <c>ConwayMaxwellPoisson</c>.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        [TestCase(0.1, 0.0)]
        [TestCase(1.0, 2.5)]
        [TestCase(2.5, 3.0)]
        [TestCase(10.0, 3.5)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreateConwayMaxwellPoisson(double lambda, double nu)
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
            Assert.That(() => new ConwayMaxwellPoisson(-1.0, -2.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var d = new ConwayMaxwellPoisson(1d, 2d);
            Assert.AreEqual("ConwayMaxwellPoisson(λ = 1, ν = 2)", d.ToString());
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
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="mean">Expected value.</param>
        [TestCase(1, 1, 1.0)]
        [TestCase(2, 1, 2.0)]
        [TestCase(10, 1, 10.0)]
        [TestCase(20, 1, 20.0)]
        [TestCase(1, 2, 0.697774657964008)]
        [TestCase(2, 2, 1.12635723962342)]
        public void ValidateMean(int lambda, int nu, double mean)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqualRelative(mean, d.Mean, 10);
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
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(1.0, 1.0, 1, 0.367879441171442)]
        [TestCase(1.0, 1.0, 2, 0.183939720585721)]
        [TestCase(2.0, 1.0, 1, 0.270670566473225)]
        [TestCase(2.0, 1.0, 2, 0.270670566473225)]
        [TestCase(2.0, 2.0, 1, 0.470328074204904)]
        [TestCase(2.0, 2.0, 3, 0.052258674911656)]
        public void ValidateProbability(double lambda, double nu, int x, double p)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqualRelative(p, d.Probability(x), 12);
        }

        /// <summary>
        /// Validate probability log.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pln">Expected value.</param>
        [TestCase(1.0, 1.0, 1, -1.0)]
        [TestCase(1.0, 1.0, 2, -1.69314718055995)]
        [TestCase(2.0, 1.0, 1, -1.30685281944005)]
        [TestCase(2.0, 1.0, 2, -1.30685281944005)]
        [TestCase(2.0, 2.0, 1, -0.754324797564617)]
        [TestCase(2.0, 2.0, 3, -2.95154937490084)]
        public void ValidateProbabilityLn(double lambda, double nu, int x, double pln)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqualRelative(pln, d.ProbabilityLn(x), 12);
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
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="lambda">Lambda value.</param>
        /// <param name="nu">DegreesOfFreedom parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [TestCase(1.0, 1.0, 1, 0.735758882342885)]
        [TestCase(1.0, 1.0, 2, 0.919698602928606)]
        [TestCase(2.0, 1.0, 1, 0.406005849709838)]
        [TestCase(2.0, 1.0, 2, 0.676676416183064)]
        [TestCase(2.0, 2.0, 1, 0.705492111307356)]
        [TestCase(2.0, 2.0, 3, 0.992914823321464)]
        public void ValidateCumulativeDistribution(double lambda, double nu, int x, double cdf)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqualRelative(cdf, d.CumulativeDistribution(x), 12);
        }
    }
}
