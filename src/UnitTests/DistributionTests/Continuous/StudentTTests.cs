// <copyright file="StudentTTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.DistributionTests
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using MathNet.Numerics.Distributions;

    [TestFixture]
    public class StudentTTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        public void CanCreateStandardStudentT()
        {
            var n = new StudentT();
            AssertEx.AreEqual<double>(0.0, n.Location);
            AssertEx.AreEqual<double>(1.0, n.Scale);
            AssertEx.AreEqual<double>(1.0, n.DegreesOfFreedom);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0)]
        [Row(0.0, 0.1, 1.0)]
        [Row(0.0, 1.0, 3.0)]
        [Row(0.0, 10.0, 1.0)]
        [Row(0.0, 10.0, Double.PositiveInfinity)]
        [Row(10.0, 1.0, 1.0)]
        [Row(-5.0, 100.0, 1.0)]
        [Row(0.0, Double.PositiveInfinity, 1.0)]
        public void CanCreateStudentT(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(location, n.Location);
            AssertEx.AreEqual<double>(scale, n.Scale);
            AssertEx.AreEqual<double>(dof, n.DegreesOfFreedom);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1.0, 1.0)]
        [Row(0.0, Double.NaN, 1.0)]
        [Row(0.0, 1.0, Double.NaN)]
        [Row(0.0, -10.0, 1.0)]
        [Row(0.0, 10.0, -1.0)]
        public void StudentTCreateFailsWithBadParameters(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new StudentT(1.0, 2.0, 1.0);
            AssertEx.AreEqual<string>("StudentT(Location = 1, Scale = 2, DoF = 1)", n.ToString());
        }

        [Test]
        [Row(Double.NegativeInfinity)]
        [Row(-5.0)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetLocation(double loc)
        {
            var n = new StudentT();
            n.Location = loc;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            var n = new StudentT();
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0)]
        [Row(-0.0)]
        [Row(0.0)]
        public void SetScaleFailsWithNonPositiveScale(double scale)
        {
            {
                var n = new StudentT();
                n.Scale = scale;
            }
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetDoF(double dof)
        {
            var n = new StudentT();
            n.DegreesOfFreedom = dof;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0)]
        [Row(-0.0)]
        [Row(0.0)]
        public void SetDofFailsWithNonPositiveDoF(double dof)
        {
            {
                var n = new StudentT();
                n.DegreesOfFreedom = dof;
            }
        }

        [Test]
        [Row(0.0, 1.0, 1.0, Double.NaN)]
        [Row(0.0, 0.1, 1.0, Double.NaN)]
        [Row(0.0, 1.0, 3.0, 0.0)]
        [Row(0.0, 10.0, 1.0, Double.NaN)]
        [Row(0.0, 10.0, 2.0, 0.0)]
        [Row(0.0, 10.0, Double.PositiveInfinity, 0.0)]
        [Row(10.0, 1.0, 1.0, Double.NaN)]
        [Row(-5.0, 100.0, 1.5, -5.0)]
        [Row(0.0, Double.PositiveInfinity, 1.0, Double.NaN)]
        public void ValidateMean(double location, double scale, double dof, double mean)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(mean, n.Mean);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, Double.NaN)]
        [Row(0.0, 0.1, 1.0, Double.NaN)]
        [Row(0.0, 1.0, 3.0, 3.0)]
        [Row(0.0, 10.0, 1.0, Double.NaN)]
        [Row(0.0, 10.0, 2.0, Double.PositiveInfinity)]
        [Row(0.0, 10.0, 2.5, 50.0)]
        [Row(0.0, 10.0, Double.PositiveInfinity, 10.0)]
        [Row(10.0, 1.0, 1.0, Double.NaN)]
        [Row(10.0, 1.0, 2.5, 5.0)]
        [Row(-5.0, 100.0, 1.5, Double.PositiveInfinity)]
        [Row(0.0, Double.PositiveInfinity, 1.0, Double.NaN)]
        public void ValidateVariance(double location, double scale, double dof, double var)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(var, n.Variance);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, Double.NaN)]
        [Row(0.0, 0.1, 1.0, Double.NaN)]
        [Row(0.0, 1.0, 3.0, 1.7320508075688772935274463415059)]
        [Row(0.0, 10.0, 1.0, Double.NaN)]
        [Row(0.0, 10.0, 2.0, Double.PositiveInfinity)]
        [Row(0.0, 10.0, 2.5, 7.0710678118654752440084436210485)]
        [Row(0.0, 10.0, Double.PositiveInfinity, 3.1622776601683793319988935444327)]
        [Row(10.0, 1.0, 1.0, Double.NaN)]
        [Row(10.0, 1.0, 2.5, 2.2360679774997896964091736687313)]
        [Row(-5.0, 100.0, 1.5, Double.PositiveInfinity)]
        [Row(0.0, Double.PositiveInfinity, 1.0, Double.NaN)]
        public void ValidateStdDev(double location, double scale, double dof, double sdev)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(sdev, n.StdDev);
        }

        [Test]
        [Row(0.0, 1.0, 1.0)]
        [Row(0.0, 0.1, 1.0)]
        [Row(0.0, 1.0, 3.0)]
        [Row(0.0, 10.0, 1.0)]
        [Row(0.0, 10.0, 2.0)]
        [Row(0.0, 10.0, 2.5)]
        [Row(0.0, 10.0, Double.PositiveInfinity)]
        [Row(10.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.5)]
        [Row(-5.0, 100.0, 1.5)]
        [Row(0.0, Double.PositiveInfinity, 1.0)]
        public void ValidateMode(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(location, n.Mode);
        }

        [Test]
        [Row(0.0, 1.0, 1.0)]
        [Row(0.0, 0.1, 1.0)]
        [Row(0.0, 1.0, 3.0)]
        [Row(0.0, 10.0, 1.0)]
        [Row(0.0, 10.0, 2.0)]
        [Row(0.0, 10.0, 2.5)]
        [Row(0.0, 10.0, Double.PositiveInfinity)]
        [Row(10.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.5)]
        [Row(-5.0, 100.0, 1.5)]
        [Row(0.0, Double.PositiveInfinity, 1.0)]
        public void ValidateMedian(double location, double scale, double dof)
        {
            var n = new StudentT(location, scale, dof);
            AssertEx.AreEqual<double>(location, n.Median);
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new StudentT();
            AssertEx.AreEqual<double>(System.Double.NegativeInfinity, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new StudentT();
            AssertEx.AreEqual<double>(System.Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 0.0, 0.318309886183791)]
        [Row(0.0, 1.0, 1.0, 1.0, 0.159154943091895)]
        [Row(0.0, 1.0, 1.0, -1.0, 0.159154943091895)]
        [Row(0.0, 1.0, 1.0, 2.0, 0.063661977236758)]
        [Row(0.0, 1.0, 1.0, -2.0, 0.063661977236758)]
        [Row(0.0, 1.0, 2.0, 0.0, 0.353553390593274)]
        [Row(0.0, 1.0, 2.0, 1.0, 0.192450089729875)]
        [Row(0.0, 1.0, 2.0, -1.0, 0.192450089729875)]
        [Row(0.0, 1.0, 2.0, 2.0, 0.068041381743977)]
        [Row(0.0, 1.0, 2.0, -2.0, 0.068041381743977)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 0.0, 0.398942280401433)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 1.0, 0.241970724519143)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 2.0, 0.053990966513188)]
        public void ValidateDensity(double location, double scale, double dof, double x, double p)
        {
            var n = new StudentT(location, scale, dof);
            AssertHelpers.AlmostEqual(p, n.Density(x), 13);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 0.0, -1.144729885849399)]
        [Row(0.0, 1.0, 1.0, 1.0, -1.837877066409348)]
        [Row(0.0, 1.0, 1.0, -1.0, -1.837877066409348)]
        [Row(0.0, 1.0, 1.0, 2.0, -2.754167798283503)]
        [Row(0.0, 1.0, 1.0, -2.0, -2.754167798283503)]
        [Row(0.0, 1.0, 2.0, 0.0, -1.039720770839917)]
        [Row(0.0, 1.0, 2.0, 1.0, -1.647918433002166)]
        [Row(0.0, 1.0, 2.0, -1.0, -1.647918433002166)]
        [Row(0.0, 1.0, 2.0, 2.0, -2.687639203842085)]
        [Row(0.0, 1.0, 2.0, -2.0, -2.687639203842085)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 0.0, -0.918938533204672)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 1.0, -1.418938533204674)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 2.0, -2.918938533204674)]
        public void ValidateDensityLn(double location, double scale, double dof, double x, double p)
        {
            var n = new StudentT(location, scale, dof);
            AssertHelpers.AlmostEqual(p, n.DensityLn(x), 13);
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = StudentT.Sample(new Random(), 0.0, 1.0, 3.0);
        }

        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = StudentT.Samples(new Random(), 0.0, 1.0, 3.0);
            var arr = ied.Take(5).ToArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0, Double.NaN, 1.0)]
        [Row(0.0, 1.0, Double.NaN)]
        [Row(0.0, -1.0, 1.0)]
        [Row(0.0, 1.0, -1.0)]
        [Row(Double.NaN, 1.0, Double.NaN)]
        public void FailSampleStatic(double location, double scale, double dof)
        {
            var d = StudentT.Sample(new Random(), location, scale, dof);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0, Double.NaN, 1.0)]
        [Row(0.0, 1.0, Double.NaN)]
        [Row(0.0, -1.0, 1.0)]
        [Row(0.0, 1.0, -1.0)]
        [Row(Double.NaN, 1.0, 1.0)]
        public void FailSampleSequenceStatic(double location, double scale, double dof)
        {
            var ied = StudentT.Samples(new Random(), location, scale, dof);
            var e = ied.Take(5).ToArray();
        }

        [Test]
        public void CanSample()
        {
            var n = new StudentT();
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new StudentT();
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 0.0, 0.5)]
        [Row(0.0, 1.0, 1.0, 1.0, 0.75)]
        [Row(0.0, 1.0, 1.0, -1.0, 0.25)]
        [Row(0.0, 1.0, 1.0, 2.0, 0.852416382349567)]
        [Row(0.0, 1.0, 1.0, -2.0, 0.147583617650433)]
        [Row(0.0, 1.0, 2.0, 0.0, 0.5)]
        [Row(0.0, 1.0, 2.0, 1.0, 0.788675134594813)]
        [Row(0.0, 1.0, 2.0, -1.0, 0.211324865405187)]
        [Row(0.0, 1.0, 2.0, 2.0, 0.908248290463863)]
        [Row(0.0, 1.0, 2.0, -2.0, 0.091751709536137)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 0.0, 0.5)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 1.0, 0.841344746068543)]
        [Row(0.0, 1.0, Double.PositiveInfinity, 2.0, 0.977249868051821)]
        public void ValidateCumulativeDistribution(double location, double scale, double dof, double x, double c)
        {
            var n = new StudentT(location, scale, dof);
            AssertHelpers.AlmostEqual(c, n.CumulativeDistribution(x), 13);
        }
    }
}
