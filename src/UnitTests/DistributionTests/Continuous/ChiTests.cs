// <copyright file="ChiTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    /// <summary>
    /// Chi distribution test
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class ChiTests
    {
        /// <summary>
        /// Can create chi.
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [TestCase(1.0)]
        [TestCase(3.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanCreateChi(double dof)
        {
            var n = new Chi(dof);
            Assert.AreEqual(dof, n.DegreesOfFreedom);
        }

        /// <summary>
        /// Chi create fails with bad parameters.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [TestCase(0.0)]
        [TestCase(-1.0)]
        [TestCase(-100.0)]
        [TestCase(Double.NegativeInfinity)]
        [TestCase(Double.NaN)]
        public void ChiCreateFailsWithBadParameters(double dof)
        {
            Assert.That(() => new Chi(dof), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Chi(1.0);
            Assert.AreEqual("Chi(k = 1)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(2.5)]
        [TestCase(5.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMean(double dof)
        {
            var n = new Chi(dof);
            Assert.AreEqual(Constants.Sqrt2 * (SpecialFunctions.Gamma((dof + 1.0) / 2.0) / SpecialFunctions.Gamma(dof / 2.0)), n.Mean);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(2.5)]
        [TestCase(3.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateVariance(double dof)
        {
            var n = new Chi(dof);
            Assert.AreEqual(dof - (n.Mean * n.Mean), n.Variance);
        }

        /// <summary>
        /// Validate standard deviation
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(2.5)]
        [TestCase(3.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateStdDev(double dof)
        {
            var n = new Chi(dof);
            Assert.AreEqual(Math.Sqrt(n.Variance), n.StdDev);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(2.5)]
        [TestCase(3.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMode(double dof)
        {
            var n = new Chi(dof);
            if (dof >= 1)
            {
                Assert.AreEqual(Math.Sqrt(dof - 1), n.Mode);
            }
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var n = new Chi(1.0);
            Assert.Throws<NotSupportedException>(() => { var median = n.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Chi(1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Chi(1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <remarks>Reference: N[PDF[ChiDistribution[dof],x],20]</remarks>
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(1.0, 0.1, 0.79390509495402353102)]
        [TestCase(1.0, 1.0, 0.48394144903828669960)]
        [TestCase(1.0, 5.5, 2.1539520085086552718e-7)]
        [TestCase(1.0, 110.1, 4.3743524642224403027e-2633)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0)]
        [TestCase(2.0, 0.0, 0.0)]
        [TestCase(2.0, 0.1, 0.099501247919268231335)]
        [TestCase(2.0, 1.0, 0.60653065971263342360)]
        [TestCase(2.0, 5.5, 1.4847681768496578863e-6)]
        [TestCase(2.0, 110.1, 6.0361640012969793703e-2631)]
        [TestCase(2.0, Double.PositiveInfinity, 0.0)]
        [TestCase(2.5, 0.0, 0.0)]
        [TestCase(2.5, 0.1, 0.029191065334961657461)]
        [TestCase(2.5, 1.0, 0.56269645152636456261)]
        [TestCase(2.5, 5.5, 3.2304380188895211768e-6)]
        [TestCase(2.5, 110.1, 5.8759231594821958799e-2630)]
        [TestCase(2.5, Double.PositiveInfinity, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.1, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 5.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 110.1, 0.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 0.0)]
        public void ValidateDensity(double dof, double x, double expected)
        {
            var chi = new Chi(dof);
            Assert.That(chi.Density(x), Is.EqualTo(expected).Within(13));
            Assert.That(Chi.PDF(dof, x), Is.EqualTo(expected).Within(13));
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <remarks>Reference: N[Ln[PDF[ChiDistribution[dof],x]],20]</remarks>
        [TestCase(1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(1.0, 0.1, -0.23079135264472743236)]
        [TestCase(1.0, 1.0, -0.72579135264472743236)]
        [TestCase(1.0, 5.5, -15.350791352644727432)]
        [TestCase(1.0, 110.1, -6061.2307913526447274)]
        [TestCase(1.0, Double.PositiveInfinity, Double.NegativeInfinity)]
        [TestCase(2.0, 0.0, Double.NegativeInfinity)]
        [TestCase(2.0, 0.1, -2.3075850929940456840)]
        [TestCase(2.0, 1.0, -0.5)]
        [TestCase(2.0, 5.5, -13.420251907761574765)]
        [TestCase(2.0, 110.1, -6056.3036109562713657)]
        [TestCase(2.0, Double.PositiveInfinity, Double.NegativeInfinity)]
        [TestCase(2.5, 0.0, Double.NegativeInfinity)]
        [TestCase(2.5, 0.1, -3.5338925982092416919)]
        [TestCase(2.5, 1.0, -0.57501495871817316589)]
        [TestCase(2.5, 5.5, -12.642892820360535314)]
        [TestCase(2.5, 110.1, -6054.0279313931252217)]
        [TestCase(2.5, Double.PositiveInfinity, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 0.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 0.1, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 5.5, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 110.1, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, Double.NegativeInfinity)]
        public void ValidateDensityLn(double dof, double x, double expected)
        {
            var chi = new Chi(dof);
            Assert.That(chi.DensityLn(x), Is.EqualTo(expected).Within(13));
            Assert.That(Chi.PDFLn(dof, x), Is.EqualTo(expected).Within(13));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Chi(1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Chi(1.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <remarks>Reference: N[CDF[ChiDistribution[dof],x],20]</remarks>
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(1.0, 0.1, 0.079655674554057962931)]
        [TestCase(1.0, 1.0, 0.68268949213708589717)]
        [TestCase(1.0, 5.5, 0.99999996202087506822)]
        [TestCase(1.0, 110.1, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 1.0)]
        [TestCase(2.0, 0.0, 0.0)]
        [TestCase(2.0, 0.1, 0.0049875208073176866474)]
        [TestCase(2.0, 1.0, 0.39346934028736657640)]
        [TestCase(2.0, 5.5, 0.99999973004214966370)]
        [TestCase(2.0, 110.1, 1.0)]
        [TestCase(2.0, Double.PositiveInfinity, 1.0)]
        [TestCase(2.5, 0.0, 0.0)]
        [TestCase(2.5, 0.1, 0.0011702413714030096290)]
        [TestCase(2.5, 1.0, 0.28378995266531297417)]
        [TestCase(2.5, 5.5, 0.99999940337322804750)]
        [TestCase(2.5, 110.1, 1.0)]
        [TestCase(2.5, Double.PositiveInfinity, 1.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.1, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 5.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 110.1, 0.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 1.0)]
        public void ValidateCumulativeDistribution(double dof, double x, double expected)
        {
            var chi = new Chi(dof);
            Assert.That(chi.CumulativeDistribution(x), Is.EqualTo(expected).Within(13));
            Assert.That(Chi.CDF(dof, x), Is.EqualTo(expected).Within(13));
            //double expected = SpecialFunctions.GammaLowerIncomplete(dof / 2.0, x * x / 2.0) / SpecialFunctions.Gamma(dof / 2.0);
            //Assert.AreEqual(expected, n.CumulativeDistribution(x));
            //Assert.AreEqual(expected, Chi.CDF(dof, x));
        }
    }
}
