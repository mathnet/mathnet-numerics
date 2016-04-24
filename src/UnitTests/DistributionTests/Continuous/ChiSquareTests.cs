// <copyright file="ChiSquareTests.cs" company="Math.NET">
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
    using Random = System.Random;

    /// <summary>
    /// Chi square distribution test
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class ChiSquareTests
    {
        /// <summary>
        /// Can create chi square.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [TestCase(1.0)]
        [TestCase(3.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanCreateChiSquare(double dof)
        {
            var n = new ChiSquared(dof);
            Assert.AreEqual(dof, n.DegreesOfFreedom);
        }

        /// <summary>
        /// Chi square create fails with bad parameters.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [TestCase(0.0)]
        [TestCase(-1.0)]
        [TestCase(-100.0)]
        [TestCase(Double.NegativeInfinity)]
        [TestCase(Double.NaN)]
        public void ChiSquareCreateFailsWithBadParameters(double dof)
        {
            Assert.That(() => new ChiSquared(dof), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new ChiSquared(1.0);
            Assert.AreEqual("ChiSquared(k = 1)", n.ToString());
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
            var n = new ChiSquared(dof);
            Assert.AreEqual(dof, n.Mean);
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
            var n = new ChiSquared(dof);
            Assert.AreEqual(2 * dof, n.Variance);
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
            var n = new ChiSquared(dof);
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
            var n = new ChiSquared(dof);
            Assert.AreEqual(dof - 2, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="dof">Degrees of freedom</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(2.5)]
        [TestCase(3.0)]
        [TestCase(Double.PositiveInfinity)]
        public void ValidateMedian(double dof)
        {
            var n = new ChiSquared(dof);
            Assert.AreEqual(dof - (2.0 / 3.0), n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new ChiSquared(1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new ChiSquared(1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <remarks>Reference: N[PDF[ChiSquaredDistribution[dof],x],20]</remarks>
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(1.0, 0.1, 1.2000389484301359798)]
        [TestCase(1.0, 1.0, 0.24197072451914334980)]
        [TestCase(1.0, 5.5, 0.010874740337283141714)]
        [TestCase(1.0, 110.1, 4.7000792147504127122e-26)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0)]
        [TestCase(2.0, 0.0, 0.0)]
        [TestCase(2.0, 0.1, 0.47561471225035700455)]
        [TestCase(2.0, 1.0, 0.30326532985631671180)]
        [TestCase(2.0, 5.5, 0.031963930603353786351)]
        [TestCase(2.0, 110.1, 6.1810004550085248492e-25)]
        [TestCase(2.0, Double.PositiveInfinity, 0.0)]
        [TestCase(2.5, 0.0, 0.0)]
        [TestCase(2.5, 0.1, 0.24812852712543073541)]
        [TestCase(2.5, 1.0, 0.28134822576318228131)]
        [TestCase(2.5, 5.5, 0.045412171451573920401)]
        [TestCase(2.5, 110.1, 1.8574923023527248767e-24)]
        [TestCase(2.5, Double.PositiveInfinity, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.1, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 5.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 110.1, 0.0)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, 0.0)]
        public void ValidateDensity(double dof, double x, double expected)
        {
            var chiSquared = new ChiSquared(dof);
            Assert.That(chiSquared.Density(x), Is.EqualTo(expected).Within(13));
            Assert.That(ChiSquared.PDF(dof, x), Is.EqualTo(expected).Within(13));
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <remarks>Reference: N[Ln[PDF[ChiSquaredDistribution[dof],x]],20]</remarks>
        [TestCase(1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(1.0, 0.1, 0.18235401329235010023)]
        [TestCase(1.0, 1.0, -1.4189385332046727418)]
        [TestCase(1.0, 5.5, -4.5213125793238853591)]
        [TestCase(1.0, 110.1, -58.319633055068989881)]
        [TestCase(1.0, Double.PositiveInfinity, Double.NegativeInfinity)]
        [TestCase(2.0, 0.0, Double.NegativeInfinity)]
        [TestCase(2.0, 0.1, -0.74314718055994530942)]
        [TestCase(2.0, 1.0, -1.1931471805599453094)]
        [TestCase(2.0, 5.5, -3.4431471805599453094)]
        [TestCase(2.0, 110.1, -55.743147180559945309)]
        [TestCase(2.0, Double.PositiveInfinity, Double.NegativeInfinity)]
        [TestCase(2.5, 0.0, Double.NegativeInfinity)]
        [TestCase(2.5, 0.1, -1.3938084125266298963)]
        [TestCase(2.5, 1.0, -1.2681621392781184753)]
        [TestCase(2.5, 5.5, -3.0919751162185121666)]
        [TestCase(2.5, 110.1, -54.642814878345959906)]
        [TestCase(2.5, Double.PositiveInfinity, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 0.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 0.1, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 5.5, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 110.1, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, Double.PositiveInfinity, Double.NegativeInfinity)]
        public void ValidateDensityLn(double dof, double x, double expected)
        {
            var chiSquared = new ChiSquared(dof);
            Assert.That(chiSquared.DensityLn(x), Is.EqualTo(expected).Within(13));
            Assert.That(ChiSquared.PDFLn(dof, x), Is.EqualTo(expected).Within(13));
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            ChiSquared.Sample(new Random(0), 2.0);
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => ChiSquared.Sample(new Random(0), -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new ChiSquared(1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new ChiSquared(1.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="expected">N[CDF[ChiSquaredDistribution[dof],x],20]</param>
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(1.0, 0.1, 0.24817036595415071751)]
        [TestCase(1.0, 1.0, 0.68268949213708589717)]
        [TestCase(1.0, 5.5, 0.98098352632769945624)]
        [TestCase(1.0, 110.1, 1.0)]
        [TestCase(2.0, 0.0, 0.0)]
        [TestCase(2.0, 0.1, 0.048770575499285990909)]
        [TestCase(2.0, 1.0, 0.39346934028736657640)]
        [TestCase(2.0, 5.5, 0.93607213879329242730)]
        [TestCase(2.0, 110.1, 1.0)]
        [TestCase(2.5, 0.0, 0.0)]
        [TestCase(2.5, 0.1, 0.020298266579604156571)]
        [TestCase(2.5, 1.0, 0.28378995266531297417)]
        [TestCase(2.5, 5.5, 0.90239512593899828629)]
        [TestCase(2.5, 110.1, 1.0)]
        [TestCase(10000.0, 1.0, 0.0)]
        [TestCase(10000.0, 7500.0, 3.3640453687878842514e-84)]
        [TestCase(20000.0, 1.0, 0.0)]
        public void ValidateCumulativeDistribution(double dof, double x, double expected)
        {
            var chiSquared = new ChiSquared(dof);
            Assert.That(chiSquared.CumulativeDistribution(x), Is.EqualTo(expected).Within(1e-14));
            Assert.That(ChiSquared.CDF(dof, x), Is.EqualTo(expected).Within(1e-14));
        }

        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(1.0, 0.24817036595415071751, 0.1)]
        [TestCase(1.0, 0.68268949213708589717, 1.0)]
        [TestCase(1.0, 0.98098352632769945624, 5.5)]
        [TestCase(1.0, 1.0, Double.PositiveInfinity)]
        [TestCase(2.0, 0.0, 0.0)]
        [TestCase(2.0, 0.048770575499285990909, 0.1)]
        [TestCase(2.0, 0.39346934028736657640, 1.0)]
        [TestCase(2.0, 0.93607213879329242730, 5.5)]
        [TestCase(2.0, 1.0, Double.PositiveInfinity)]
        [TestCase(2.5, 0.0, 0.0)]
        [TestCase(2.5, 0.020298266579604156571, 0.1)]
        [TestCase(2.5, 0.28378995266531297417, 1.0)]
        [TestCase(2.5, 0.90239512593899828629, 5.5)]
        [TestCase(2.5, 1.0, Double.PositiveInfinity)]
        [TestCase(10000.0, 0.0, 0.0)]
        [TestCase(10000.0, 0.5, 9999.3333412343982)]
        [TestCase(20000.0, 0.0, 0.0)]
        [TestCase(100000, 0.1, 99427.302671875732)]
        public void ValidateInverseCumulativeDistribution(double dof, double x, double expected)
        {
            var chiSquared = new ChiSquared(dof);
            Assert.That(chiSquared.InverseCumulativeDistribution(x), Is.EqualTo(expected).Within(1e-14));
            Assert.That(ChiSquared.InvCDF(dof, x), Is.EqualTo(expected).Within(1e-14));
        }
    }
}
