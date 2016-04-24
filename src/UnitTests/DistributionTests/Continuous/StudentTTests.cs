// <copyright file="StudentTTests.cs" company="Math.NET">
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
    /// <c>StudentT</c> distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class StudentTTests
    {
        /// <summary>
        /// Can create standard <c>StudentT</c>.
        /// </summary>
        [Test]
        public void CanCreateStandardStudentT()
        {
            var n = new StudentT();
            Assert.AreEqual(0.0, n.Location);
            Assert.AreEqual(1.0, n.Scale);
            Assert.AreEqual(1.0, n.DegreesOfFreedom);
        }

        /// <summary>
        /// Can create <c>StudentT</c>.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        [TestCase(0.0, 0.1, 1.0)]
        [TestCase(-5.0, 1.0, 3.0)]
        [TestCase(10.0, 10.0, Double.PositiveInfinity)]
        public void CanCreateStudentT(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
            Assert.AreEqual(location, n.Location);
            Assert.AreEqual(scale, n.Scale);
            Assert.AreEqual(dof, n.DegreesOfFreedom);
        }

        /// <summary>
        /// <c>StudentT</c> create fails with bad parameters.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        [TestCase(Double.NaN, 1.0, 1.0)]
        [TestCase(0.0, Double.NaN, 1.0)]
        [TestCase(0.0, 1.0, Double.NaN)]
        [TestCase(0.0, -10.0, 1.0)]
        [TestCase(0.0, 10.0, -1.0)]
        public void StudentTCreateFailsWithBadParameters(double location, double scale, double dof)
        {
            Assert.That(() => new StudentT(location, scale, dof), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new StudentT(1.0, 2.0, 1.0);
            Assert.AreEqual("StudentT(μ = 1, σ = 2, ν = 1)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="mean">Expected value.</param>
        [TestCase(0.0, 1.0, 1.0, Double.NaN)]
        [TestCase(0.0, 0.1, 1.0, Double.NaN)]
        [TestCase(0.0, 1.0, 3.0, 0.0)]
        [TestCase(0.0, 10.0, 1.0, Double.NaN)]
        [TestCase(0.0, 10.0, 2.0, 0.0)]
        [TestCase(0.0, 10.0, Double.PositiveInfinity, 0.0)]
        [TestCase(10.0, 1.0, 1.0, Double.NaN)]
        [TestCase(-5.0, 100.0, 1.5, -5.0)]
        [TestCase(0.0, Double.PositiveInfinity, 1.0, Double.NaN)]
        public void ValidateMean(double location, double scale, double dof, double mean)
        {
            var n = new StudentT(location, scale, dof);
            Assert.AreEqual(mean, n.Mean);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="var">Expected value.</param>
        [TestCase(0.0, 1.0, 1.0, Double.NaN)]
        [TestCase(0.0, 0.1, 1.0, Double.NaN)]
        [TestCase(0.0, 1.0, 3.0, 3.0)]
        [TestCase(0.0, 10.0, 1.0, Double.NaN)]
        [TestCase(0.0, 10.0, 2.0, Double.PositiveInfinity)]
        [TestCase(0.0, 10.0, 2.5, 500.0)]
        [TestCase(0.0, 10.0, Double.PositiveInfinity, 100.0)]
        [TestCase(10.0, 1.0, 1.0, Double.NaN)]
        [TestCase(10.0, 1.0, 2.5, 5.0)]
        [TestCase(-5.0, 100.0, 1.5, Double.PositiveInfinity)]
        [TestCase(0.0, Double.PositiveInfinity, 1.0, Double.NaN)]
        public void ValidateVariance(double location, double scale, double dof, double var)
        {
            var n = new StudentT(location, scale, dof);
            Assert.AreEqual(var, n.Variance);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="sdev">Expected value.</param>
        [TestCase(0.0, 1.0, 1.0, Double.NaN)]
        [TestCase(0.0, 0.1, 1.0, Double.NaN)]
        [TestCase(0.0, 1.0, 3.0, 1.7320508075688772935274463415059)]
        [TestCase(0.0, 10.0, 1.0, Double.NaN)]
        [TestCase(0.0, 10.0, 2.0, Double.PositiveInfinity)]
        [TestCase(0.0, 10.0, 2.5, 22.360679774997896964091736687313)]
        [TestCase(0.0, 10.0, Double.PositiveInfinity, 10.0)]
        [TestCase(10.0, 1.0, 1.0, Double.NaN)]
        [TestCase(10.0, 1.0, 2.5, 2.2360679774997896964091736687313)]
        [TestCase(-5.0, 100.0, 1.5, Double.PositiveInfinity)]
        [TestCase(0.0, Double.PositiveInfinity, 1.0, Double.NaN)]
        public void ValidateStdDev(double location, double scale, double dof, double sdev)
        {
            var n = new StudentT(location, scale, dof);
            Assert.AreEqual(sdev, n.StdDev);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        [TestCase(0.0, 1.0, 1.0)]
        [TestCase(0.0, 0.1, 1.0)]
        [TestCase(0.0, 1.0, 3.0)]
        [TestCase(0.0, 10.0, 1.0)]
        [TestCase(0.0, 10.0, 2.0)]
        [TestCase(0.0, 10.0, 2.5)]
        [TestCase(0.0, 10.0, Double.PositiveInfinity)]
        [TestCase(10.0, 1.0, 1.0)]
        [TestCase(10.0, 1.0, 2.5)]
        [TestCase(-5.0, 100.0, 1.5)]
        [TestCase(0.0, Double.PositiveInfinity, 1.0)]
        public void ValidateMode(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
            Assert.AreEqual(location, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        [TestCase(0.0, 1.0, 1.0)]
        [TestCase(0.0, 0.1, 1.0)]
        [TestCase(0.0, 1.0, 3.0)]
        [TestCase(0.0, 10.0, 1.0)]
        [TestCase(0.0, 10.0, 2.0)]
        [TestCase(0.0, 10.0, 2.5)]
        [TestCase(0.0, 10.0, Double.PositiveInfinity)]
        [TestCase(10.0, 1.0, 1.0)]
        [TestCase(10.0, 1.0, 2.5)]
        [TestCase(-5.0, 100.0, 1.5)]
        [TestCase(0.0, Double.PositiveInfinity, 1.0)]
        public void ValidateMedian(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
            Assert.AreEqual(location, n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new StudentT();
            Assert.AreEqual(Double.NegativeInfinity, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new StudentT();
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(0.0, 1.0, 1.0, 0.0, 0.318309886183791)]
        [TestCase(0.0, 1.0, 1.0, 1.0, 0.159154943091895)]
        [TestCase(0.0, 1.0, 1.0, -1.0, 0.159154943091895)]
        [TestCase(0.0, 1.0, 1.0, 2.0, 0.063661977236758)]
        [TestCase(0.0, 1.0, 1.0, -2.0, 0.063661977236758)]
        [TestCase(0.0, 1.0, 2.0, 0.0, 0.353553390593274)]
        [TestCase(0.0, 1.0, 2.0, 1.0, 0.192450089729875)]
        [TestCase(0.0, 1.0, 2.0, -1.0, 0.192450089729875)]
        [TestCase(0.0, 1.0, 2.0, 2.0, 0.068041381743977)]
        [TestCase(0.0, 1.0, 2.0, -2.0, 0.068041381743977)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 0.0, 0.398942280401433)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 1.0, 0.241970724519143)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 2.0, 0.053990966513188)]
        public void ValidateDensity(double location, double scale, double dof, double x, double p)
        {
            var n = new StudentT(location, scale, dof);
            AssertHelpers.AlmostEqualRelative(p, n.Density(x), 13);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [TestCase(0.0, 1.0, 1.0, 0.0, -1.144729885849399)]
        [TestCase(0.0, 1.0, 1.0, 1.0, -1.837877066409348)]
        [TestCase(0.0, 1.0, 1.0, -1.0, -1.837877066409348)]
        [TestCase(0.0, 1.0, 1.0, 2.0, -2.754167798283503)]
        [TestCase(0.0, 1.0, 1.0, -2.0, -2.754167798283503)]
        [TestCase(0.0, 1.0, 2.0, 0.0, -1.039720770839917)]
        [TestCase(0.0, 1.0, 2.0, 1.0, -1.647918433002166)]
        [TestCase(0.0, 1.0, 2.0, -1.0, -1.647918433002166)]
        [TestCase(0.0, 1.0, 2.0, 2.0, -2.687639203842085)]
        [TestCase(0.0, 1.0, 2.0, -2.0, -2.687639203842085)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 0.0, -0.918938533204672)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 1.0, -1.418938533204674)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 2.0, -2.918938533204674)]
        public void ValidateDensityLn(double location, double scale, double dof, double x, double p)
        {
            var n = new StudentT(location, scale, dof);
            AssertHelpers.AlmostEqualRelative(p, n.DensityLn(x), 13);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            StudentT.Sample(new Random(0), 0.0, 1.0, 3.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = StudentT.Samples(new Random(0), 0.0, 1.0, 3.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new StudentT();
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new StudentT();
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        [TestCase(0.0, 1.0, 1.0, 0.0, 0.5)]
        [TestCase(0.0, 1.0, 1.0, 1.0, 0.75)]
        [TestCase(0.0, 1.0, 1.0, -1.0, 0.25)]
        [TestCase(0.0, 1.0, 1.0, 2.0, 0.852416382349567)]
        [TestCase(0.0, 1.0, 1.0, -2.0, 0.147583617650433)]
        [TestCase(0.0, 1.0, 2.0, 0.0, 0.5)]
        [TestCase(0.0, 1.0, 2.0, 1.0, 0.788675134594813)]
        [TestCase(0.0, 1.0, 2.0, -1.0, 0.211324865405187)]
        [TestCase(0.0, 1.0, 2.0, 2.0, 0.908248290463863)]
        [TestCase(0.0, 1.0, 2.0, -2.0, 0.091751709536137)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 0.0, 0.5)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 1.0, 0.841344746068543)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 2.0, 0.977249868051821)]
        public void ValidateCumulativeDistribution(double location, double scale, double dof, double x, double p)
        {
            var dist = new StudentT(location, scale, dof);
            Assert.That(dist.CumulativeDistribution(x), Is.EqualTo(p).Within(1e-13));
            Assert.That(StudentT.CDF(location, scale, dof, x), Is.EqualTo(p).Within(1e-13));
        }

        [TestCase(0.0, 1.0, 1.0, 0.0, 0.5)]
        [TestCase(0.0, 1.0, 1.0, 1.0, 0.75)]
        [TestCase(0.0, 1.0, 1.0, -1.0, 0.25)]
        [TestCase(0.0, 1.0, 1.0, 2.0, 0.852416382349567)]
        [TestCase(0.0, 1.0, 1.0, -2.0, 0.147583617650433)]
        [TestCase(0.0, 1.0, 2.0, 0.0, 0.5)]
        [TestCase(0.0, 1.0, 2.0, 1.0, 0.788675134594813)]
        [TestCase(0.0, 1.0, 2.0, -1.0, 0.211324865405187)]
        [TestCase(0.0, 1.0, 2.0, 2.0, 0.908248290463863)]
        [TestCase(0.0, 1.0, 2.0, -2.0, 0.091751709536137)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 0.0, 0.5)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 1.0, 0.841344746068543)]
        [TestCase(0.0, 1.0, Double.PositiveInfinity, 2.0, 0.977249868051821)]
        public void ValidateInverseCumulativeDistribution(double location, double scale, double dof, double x, double p)
        {
            var dist = new StudentT(location, scale, dof);
            Assert.That(dist.InverseCumulativeDistribution(p), Is.EqualTo(x).Within(1e-6));
            Assert.That(StudentT.InvCDF(location, scale, dof, p), Is.EqualTo(x).Within(1e-6));
        }
    }
}
