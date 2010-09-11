// <copyright file="ChiSquareTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using Distributions;

    [TestFixture]
    public class ChiSquareTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(1.0)]
        [Row(3.0)]
        [Row(Double.PositiveInfinity)]
        public void CanCreateChiSquare(double dof)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>(dof, n.DegreesOfFreedom);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0)]
        [Row(-1.0)]
        [Row(-100.0)]
        [Row(Double.NegativeInfinity)]
        [Row(Double.NaN)]
        public void ChiSquareCreateFailsWithBadParameters(double dof)
        {
            var n = new ChiSquare(dof);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new ChiSquare(1.0);
            Assert.AreEqual<string>("ChiSquare(DoF = 1)", n.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetDoF(double dof)
        {
            var n = new ChiSquare(1.0);
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
                var n = new ChiSquare(1.0);
                n.DegreesOfFreedom = dof;
            }
        }

        [Test]
        [Row(1.0)]
        [Row(2.0)]
        [Row(2.5)]
        [Row(5.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMean(double dof)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>(dof, n.Mean);
        }

        [Test]
        [Row(1.0)]
        [Row(2.0)]
        [Row(2.5)]
        [Row(3.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateVariance(double dof)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>(2 * dof, n.Variance);
        }

        [Test]
        [Row(1.0)]
        [Row(2.0)]
        [Row(2.5)]
        [Row(3.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateStdDev(double dof)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>(Math.Sqrt(n.Variance), n.StdDev);
        }

        [Test]
        [Row(1.0)]
        [Row(1.5)]
        [Row(2.5)]
        [Row(3.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMode(double dof)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>(dof - 2, n.Mode);
        }

        [Test]
        [Row(1.0)]
        [Row(1.5)]
        [Row(2.5)]
        [Row(3.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMedian(double dof)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>(dof - 2.0 / 3.0, n.Median);
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new ChiSquare(1.0);
            Assert.AreEqual<double>(0.0, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new ChiSquare(1.0);
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(1.0, 0.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 5.5)]
        [Row(1.0, 110.1)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(2.0, 0.0)]
        [Row(2.0, 0.1)]
        [Row(2.0, 1.0)]
        [Row(2.0, 5.5)]
        [Row(2.0, 110.1)]
        [Row(2.0, Double.PositiveInfinity)]
        [Row(2.5, 0.0)]
        [Row(2.5, 0.1)]
        [Row(2.5, 1.0)]
        [Row(2.5, 5.5)]
        [Row(2.5, 110.1)]
        [Row(2.5, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 5.5)]
        [Row(Double.PositiveInfinity, 110.1)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensity(double dof, double x)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>((Math.Pow(x, dof / 2.0 - 1.0) * Math.Exp(-x / 2.0)) / (Math.Pow(2.0, dof / 2.0) * SpecialFunctions.Gamma(dof / 2.0)), n.Density(x));
        }

        [Test]
        [Row(1.0, 0.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 5.5)]
        [Row(1.0, 110.1)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(2.0, 0.0)]
        [Row(2.0, 0.1)]
        [Row(2.0, 1.0)]
        [Row(2.0, 5.5)]
        [Row(2.0, 110.1)]
        [Row(2.0, Double.PositiveInfinity)]
        [Row(2.5, 0.0)]
        [Row(2.5, 0.1)]
        [Row(2.5, 1.0)]
        [Row(2.5, 5.5)]
        [Row(2.5, 110.1)]
        [Row(2.5, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 5.5)]
        [Row(Double.PositiveInfinity, 110.1)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateDensityLn(double dof, double x)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>(-x / 2.0 + (dof / 2.0 - 1.0) * Math.Log(x) - (dof / 2.0) * Math.Log(2) - SpecialFunctions.GammaLn(dof / 2.0), n.DensityLn(x));
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = ChiSquare.Sample(new Random(), 2.0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = ChiSquare.Sample(new Random(), -1.0);
        }

        [Test]
        public void CanSample()
        {
            var n = new ChiSquare(1.0);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new ChiSquare(1.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(1.0, 0.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 5.5)]
        [Row(1.0, 110.1)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(2.0, 0.0)]
        [Row(2.0, 0.1)]
        [Row(2.0, 1.0)]
        [Row(2.0, 5.5)]
        [Row(2.0, 110.1)]
        [Row(2.0, Double.PositiveInfinity)]
        [Row(2.5, 0.0)]
        [Row(2.5, 0.1)]
        [Row(2.5, 1.0)]
        [Row(2.5, 5.5)]
        [Row(2.5, 110.1)]
        [Row(2.5, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 0.1)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 5.5)]
        [Row(Double.PositiveInfinity, 110.1)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double dof, double x)
        {
            var n = new ChiSquare(dof);
            Assert.AreEqual<double>(SpecialFunctions.GammaUpperIncomplete(dof / 2.0, x / 2.0) / SpecialFunctions.Gamma(dof / 2.0), n.CumulativeDistribution(x));
        }
    }
}
