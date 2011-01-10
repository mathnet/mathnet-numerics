// <copyright file="ChiTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using System;
    using System.Linq;
    using Distributions;
    using NUnit.Framework;

    /// <summary>
    /// Chi distribution test
    /// </summary>
    [TestFixture]
    public class ChiTests
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
        /// Can create chi.
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [Test]
        public void CanCreateChi([Values(1.0, 3.0, Double.PositiveInfinity)] double dof)
        {
            var n = new Chi(dof);
            Assert.AreEqual(dof, n.DegreesOfFreedom);
        }

        /// <summary>
        /// Chi create fails with bad parameters.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [Test]
        public void ChiCreateFailsWithBadParameters([Values(0.0, -1.0, -100.0, Double.NegativeInfinity, Double.NaN)] double dof)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Chi(dof));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Chi(1.0);
            Assert.AreEqual("Chi(DoF = 1)", n.ToString());
        }

        /// <summary>
        /// Can set degrees of freedom.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [Test]
        public void CanSetDoF([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double dof)
        {
            new Chi(1.0)
            {
                DegreesOfFreedom = dof
            };
        }

        /// <summary>
        /// Set Degrees of freedom fails with non-positive value.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [Test]
        public void SetDofFailsWithNonPositiveDoF([Values(-1.0, -0.0, 0.0)] double dof)
        {
            var n = new Chi(1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.DegreesOfFreedom = dof);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [Test]
        public void ValidateMean([Values(1.0, 2.0, 2.5, 5.0, Double.PositiveInfinity)] double dof)
        {
            var n = new Chi(dof);
            Assert.AreEqual(Math.Sqrt(2) * (SpecialFunctions.Gamma((dof + 1.0) / 2.0) / SpecialFunctions.Gamma(dof / 2.0)), n.Mean);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [Test]
        public void ValidateVariance([Values(1.0, 2.0, 2.5, 3.0, Double.PositiveInfinity)] double dof)
        {
            var n = new Chi(dof);
            Assert.AreEqual(dof - (n.Mean * n.Mean), n.Variance);
        }

        /// <summary>
        /// Validate standard deviation
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [Test]
        public void ValidateStdDev([Values(1.0, 2.0, 2.5, 3.0, Double.PositiveInfinity)] double dof)
        {
            var n = new Chi(dof);
            Assert.AreEqual(Math.Sqrt(n.Variance), n.StdDev);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [Test]
        public void ValidateMode([Values(1.0, 2.0, 2.5, 3.0, Double.PositiveInfinity)] double dof)
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
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="x">Input X value.</param>
        [Test, Sequential]
        public void ValidateDensity(
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 2.0, 2.0, 2.0, 2.0, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double dof, 
            [Values(0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity)] double x)
        {
            var n = new Chi(dof);
            Assert.AreEqual((Math.Pow(2.0, 1.0 - (dof / 2.0)) * Math.Pow(x, dof - 1.0) * Math.Exp(-x * (x / 2.0))) / SpecialFunctions.Gamma(dof / 2.0), n.Density(x));
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="x">Input X value.</param>
        [Test, Sequential]
        public void ValidateDensityLn(
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 2.0, 2.0, 2.0, 2.0, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double dof, 
            [Values(0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity)] double x)
        {
            var n = new Chi(dof);
            Assert.AreEqual(((1.0 - (dof / 2.0)) * Math.Log(2.0)) + ((dof - 1.0) * Math.Log(x)) - (x * (x / 2.0)) - SpecialFunctions.GammaLn(dof / 2.0), n.DensityLn(x));
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
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="x">Input X value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 2.0, 2.0, 2.0, 2.0, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double dof, 
            [Values(0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity, 0.0, 0.1, 1.0, 5.5, 110.1, Double.PositiveInfinity)] double x)
        {
            var n = new Chi(dof);
            Assert.AreEqual(SpecialFunctions.GammaLowerIncomplete(dof / 2.0, x * x / 2.0) / SpecialFunctions.Gamma(dof / 2.0), n.CumulativeDistribution(x));
        }
    }
}
