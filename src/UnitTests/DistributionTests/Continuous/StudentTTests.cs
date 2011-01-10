// <copyright file="StudentTTests.cs" company="Math.NET">
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
    /// <c>StudentT</c> distribution tests.
    /// </summary>
    [TestFixture]
    public class StudentTTests
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
        [Test, Combinatorial]
        public void CanCreateStudentT([Values(0.0, -5.0, 10.0)] double location, [Values(0.1, 1.0, 10.0)] double scale, [Values(1.0, 3.0, Double.PositiveInfinity)] double dof)
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
        [Test, Sequential]
        public void StudentTCreateFailsWithBadParameters(
            [Values(Double.NaN, 0.0, 0.0, 0.0, 0.0)] double location, 
            [Values(1.0, Double.NaN, 1.0, -10.0, 10.0)] double scale, 
            [Values(1.0, 1.0, Double.NaN, 1.0, -1.0)] double dof)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new StudentT(location, scale, dof));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new StudentT(1.0, 2.0, 1.0);
            Assert.AreEqual("StudentT(Location = 1, Scale = 2, DoF = 1)", n.ToString());
        }

        /// <summary>
        /// Can set location.
        /// </summary>
        /// <param name="loc">Location value.</param>
        [Test]
        public void CanSetLocation([Values(Double.NegativeInfinity, -5.0, -0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double loc)
        {
            new StudentT
            {
                Location = loc
            };
        }

        /// <summary>
        /// Can set scale.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [Test]
        public void CanSetScale([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale)
        {
            new StudentT
            {
                Scale = scale
            };
        }

        /// <summary>
        /// Set scale fails with non-positive scale.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [Test]
        public void SetScaleFailsWithNonPositiveScale([Values(-1.0, -0.0, 0.0)] double scale)
        {
            var n = new StudentT();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Scale = scale);
        }

        /// <summary>
        /// Can set degrees of freedom.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [Test]
        public void CanSetDoF([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double dof)
        {
            new StudentT
            {
                DegreesOfFreedom = dof
            };
        }

        /// <summary>
        /// Set degrees of freedom fails with non-positive value.
        /// </summary>
        /// <param name="dof">Degrees of freedom.</param>
        [Test]
        public void SetDofFailsWithNonPositiveDoF([Values(-1.0, -0.0, 0.0)] double dof)
        {
            var n = new StudentT();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.DegreesOfFreedom = dof);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="mean">Expected value.</param>
        [Test, Sequential]
        public void ValidateMean(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10.0, -5.0, 0.0)] double location, 
            [Values(1.0, 0.1, 1.0, 10.0, 10.0, 10.0, 1.0, 100.0, Double.PositiveInfinity)] double scale, 
            [Values(1.0, 1.0, 3.0, 1.0, 2.0, Double.PositiveInfinity, 1.0, 1.5, 1.0)] double dof, 
            [Values(Double.NaN, Double.NaN, 0.0, Double.NaN, 0.0, 0.0, Double.NaN, -5.0, Double.NaN)] double mean)
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
        [Test, Sequential]
        public void ValidateVariance(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10.0, 10.0, -5.0, 0.0)] double location, 
            [Values(1.0, 0.1, 1.0, 10.0, 10.0, 10.0, 10.0, 1.0, 1.0, 100.0, Double.PositiveInfinity)] double scale, 
            [Values(1.0, 1.0, 3.0, 1.0, 2.0, 2.5, Double.PositiveInfinity, 1.0, 2.5, 1.5, 1.0)] double dof, 
            [Values(Double.NaN, Double.NaN, 3.0, Double.NaN, Double.PositiveInfinity, 500.0, 100.0, Double.NaN, 5.0, Double.PositiveInfinity, Double.NaN)] double var)
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
        [Test, Sequential]
        public void ValidateStdDev(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10.0, 10.0, -5.0, 0.0)] double location, 
            [Values(1.0, 0.1, 1.0, 10.0, 10.0, 10.0, 10.0, 1.0, 1.0, 100.0, Double.PositiveInfinity)] double scale, 
            [Values(1.0, 1.0, 3.0, 1.0, 2.0, 2.5, Double.PositiveInfinity, 1.0, 2.5, 1.5, 1.0)] double dof, 
            [Values(Double.NaN, Double.NaN, 1.7320508075688772935274463415059, Double.NaN, Double.PositiveInfinity, 22.360679774997896964091736687313, 10.0, Double.NaN, 2.2360679774997896964091736687313, Double.PositiveInfinity, Double.NaN)] double sdev)
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
        [Test, Sequential]
        public void ValidateMode(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10.0, 10.0, -5.0, 0.0)] double location, 
            [Values(1.0, 0.1, 1.0, 10.0, 10.0, 10.0, 10.0, 1.0, 1.0, 100.0, Double.PositiveInfinity)] double scale, 
            [Values(1.0, 1.0, 3.0, 1.0, 2.0, 2.5, Double.PositiveInfinity, 1.0, 2.5, 1.5, 1.0)] double dof)
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
        [Test, Sequential]
        public void ValidateMedian(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10.0, 10.0, -5.0, 0.0)] double location, 
            [Values(1.0, 0.1, 1.0, 10.0, 10.0, 10.0, 10.0, 1.0, 1.0, 100.0, Double.PositiveInfinity)] double scale, 
            [Values(1.0, 1.0, 3.0, 1.0, 2.0, 2.5, Double.PositiveInfinity, 1.0, 2.5, 1.5, 1.0)] double dof)
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
        [Test, Sequential]
        public void ValidateDensity(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)] double location, 
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0)] double scale, 
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 2.0, 2.0, 2.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double dof, 
            [Values(0.0, 1.0, -1.0, 2.0, -2.0, 0.0, 1.0, -1.0, 2.0, -2.0, 0.0, 1.0, 2.0)] double x, 
            [Values(0.318309886183791, 0.159154943091895, 0.159154943091895, 0.063661977236758, 0.063661977236758, 0.353553390593274, 0.192450089729875, 0.192450089729875, 0.068041381743977, 0.068041381743977, 0.398942280401433, 0.241970724519143, 0.053990966513188)] double p)
        {
            var n = new StudentT(location, scale, dof);
            AssertHelpers.AlmostEqual(p, n.Density(x), 13);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        [Test, Sequential]
        public void ValidateDensityLn(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)] double location, 
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0)] double scale, 
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 2.0, 2.0, 2.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double dof, 
            [Values(0.0, 1.0, -1.0, 2.0, -2.0, 0.0, 1.0, -1.0, 2.0, -2.0, 0.0, 1.0, 2.0)] double x, 
            [Values(-1.144729885849399, -1.837877066409348, -1.837877066409348, -2.754167798283503, -2.754167798283503, -1.039720770839917, -1.647918433002166, -1.647918433002166, -2.687639203842085, -2.687639203842085, -0.918938533204672, -1.418938533204674, -2.918938533204674)] double p)
        {
            var n = new StudentT(location, scale, dof);
            AssertHelpers.AlmostEqual(p, n.DensityLn(x), 13);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            StudentT.Sample(new Random(), 0.0, 1.0, 3.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = StudentT.Samples(new Random(), 0.0, 1.0, 3.0);
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => StudentT.Sample(new Random(), Double.NaN, 1.0, Double.NaN));
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            var ied = StudentT.Samples(new Random(), 0.0, 1.0, Double.NaN);
            Assert.Throws<ArgumentOutOfRangeException>(() => ied.Take(5).ToArray());
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
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="location">Location value.</param>
        /// <param name="scale">Scale value.</param>
        /// <param name="dof">Degrees of freedom.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="c">Expected value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)] double location, 
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0)] double scale, 
            [Values(1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 2.0, 2.0, 2.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double dof, 
            [Values(0.0, 1.0, -1.0, 2.0, -2.0, 0.0, 1.0, -1.0, 2.0, -2.0, 0.0, 1.0, 2.0)] double x, 
            [Values(0.5, 0.75, 0.25, 0.852416382349567, 0.147583617650433, 0.5, 0.788675134594813, 0.211324865405187, 0.908248290463863, 0.091751709536137, 0.5, 0.841344746068543, 0.977249868051821)] double c)
        {
            var n = new StudentT(location, scale, dof);
            AssertHelpers.AlmostEqual(c, n.CumulativeDistribution(x), 13);
        }
    }
}
