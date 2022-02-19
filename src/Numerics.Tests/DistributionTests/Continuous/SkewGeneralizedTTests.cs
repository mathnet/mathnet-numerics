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

namespace MathNet.Numerics.Tests.DistributionTests.Continuous
{
    /// <summary>
    /// <c>SkewedGeneralizedT</c> distribution tests.
    /// Reference values are from the R package sgt 2.0 (run on Microsoft R Open v3.5.2)
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class SkewedGeneralizedTTests
    {
        [Test]
        public void CanCreateStandardSkewedGeneralizedT()
        {
            var n = new SkewedGeneralizedT();
            Assert.AreEqual(0.0, n.Location);
            Assert.AreEqual(1.0, n.Scale);
            Assert.AreEqual(0.0, n.Skew);
            Assert.AreEqual(2.0, n.P);
            Assert.AreEqual(double.PositiveInfinity, n.Q);
        }

        [TestCase(0.0, 1.0, 0.0, 2.0, double.PositiveInfinity)] // Standard Normal distribution
        [TestCase(5.0, 1.0, 0.0, 2.0, double.PositiveInfinity)] // Mean shifted Normal distribution
        [TestCase(0.0, 2.0, 0.0, 2.0, double.PositiveInfinity)] // Scaled Normal distribution
        [TestCase(0.0, 1.0, 0.9, 2.0, double.PositiveInfinity)] // Skewed Normal distribution
        [TestCase(1.0, 1.5, 0.9, 2.0, double.PositiveInfinity)] // Mean shifted and scaled Skewed Normal distribution
        [TestCase(0.0, 1.0, 0.9, 2.0, 1.1)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, 0.0, 2.0, 1.1)] // Student T distribution
        [TestCase(0.0, 1.0, -0.3, 2.2, double.PositiveInfinity)] // Skewed Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, 2.2, double.PositiveInfinity)] // Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0)] // Continuous Uniform
        public void CanCreateSkewedGeneralizedT(double location, double scale, double skew, double p, double q)
        {
            var n = new SkewedGeneralizedT(location, scale, skew, p, q);
            Assert.AreEqual(location, n.Location);
            Assert.AreEqual(scale, n.Scale);
            Assert.AreEqual(skew, n.Skew);
            Assert.AreEqual(p, n.P);
            Assert.AreEqual(q, n.Q);
        }

        [TestCase(0.0, 1.0, 0.0, 2.0, 1.0)] // pq <= 2
        [TestCase(0.0, 1.0, 0.0, -2.0, -1.0)] // pq <= 2 and negative values
        [TestCase(5.0, -1.0, 0.0, 2.0, double.PositiveInfinity)] // Negative scale
        [TestCase(0.0, 2.0, 1.1, 2.0, double.PositiveInfinity)] // Invalid skew, too large
        [TestCase(0.0, 1.0, -1.1, 2.0, double.PositiveInfinity)] // Invalid skew, too small
        public void SkewedGeneralizedTCreateFailsWithBadParameters(double location, double scale, double skew, double p, double q)
        {
            Assert.That(() => new SkewedGeneralizedT(location, scale, skew, p, q), Throws.ArgumentException);
        }

        [TestCase(0.0, 1.0, 0.0, 2.0, double.PositiveInfinity, 0.9123, 0.2631360242)] // Standard Normal distribution
        [TestCase(5.0, 1.0, 0.0, 2.0, double.PositiveInfinity, 4.9123, 0.3974110362)] // Mean shifted Normal distribution
        [TestCase(0.0, 2.0, 0.0, 2.0, double.PositiveInfinity, 0.9123, 0.1797618922)] // Scaled Normal distribution
        [TestCase(0.0, 1.0, 0.9, 2.0, double.PositiveInfinity, 0.9123, 0.1958872375)] // Skewed Normal distribution
        [TestCase(1.0, 1.5, 0.9, 2.0, double.PositiveInfinity, 0.9123, 0.2400040926)] // Mean shifted and scaled Skewed Normal distribution
        [TestCase(0.0, 1.0, -0.9, 2.0, 5.0, 0.9123, 0.523647666)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, -0.9, 2.0, 5.0, -0.9123, 0.1799965988)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, 0.0, 2.0, 5.0, 0.9123, 0.2524160191)] // Student T distribution
        [TestCase(0.0, 1.0, -0.3, 2.2, double.PositiveInfinity, 0.9123, 0.3323206895)] // Skewed Generalized Error Distribution
        [TestCase(-1.5, 1.0, -0.3, 2.2, double.PositiveInfinity, 0.9123, 0.006297697854)] // Mean shifted Skewed Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, 2.2, double.PositiveInfinity, 0.9123, 0.2701962342)] // Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0, 0.5123, 0.2886751346)] // Continuous Uniform
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0, 0.6123, 0.2886751346)] // Continuous Uniform
        [TestCase(0.0, 1.0, 0.77, 1.0, double.PositiveInfinity, 0.6123, 0.2016342715)] // Skewed Laplace
        [TestCase(0.0, 1.0, 0.0, 1.0, double.PositiveInfinity, 0.6123, 0.2974537422)] // Laplace
        [TestCase(0.9, 1.0, 0.0, 1.0, double.PositiveInfinity, 0.6123, 0.4707430703)] // Mean shifted Laplace
        public void ValidateDensity(double location, double scale, double skew, double p, double q, double x, double d)
        {
            var n = new SkewedGeneralizedT(location, scale, skew, p, q);
            var density = n.Density(x);
            AssertHelpers.AlmostEqualRelative(d, density, 8);
        }

        [TestCase(0.0, 1.0, 0.0, 2.0, double.PositiveInfinity, 0.9123, -1.335084178)] // Standard Normal distribution
        [TestCase(5.0, 1.0, 0.0, 2.0, double.PositiveInfinity, 4.9123, -0.9227841782)] // Mean shifted Normal distribution
        [TestCase(0.0, 2.0, 0.0, 2.0, double.PositiveInfinity, 0.9123, -1.716122125)] // Scaled Normal distribution
        [TestCase(0.0, 1.0, 0.9, 2.0, double.PositiveInfinity, 0.9123, -1.630216104)] // Skewed Normal distribution
        [TestCase(1.0, 1.5, 0.9, 2.0, double.PositiveInfinity, 0.9123, -1.427099303)] // Mean shifted and scaled Skewed Normal distribution
        [TestCase(0.0, 1.0, -0.9, 2.0, 5.0, 0.9123, -0.646936214)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, -0.9, 2.0, 5.0, -0.9123, -1.714817324)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, 0.0, 2.0, 5.0, 0.9123, -1.376676683)] // Student T distribution
        [TestCase(0.0, 1.0, -0.3, 2.2, double.PositiveInfinity, 0.9123, -1.101654844)] // Skewed Generalized Error Distribution
        [TestCase(-1.5, 1.0, -0.3, 2.2, double.PositiveInfinity, 0.9123, -5.067571132)] // Mean shifted Skewed Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, 2.2, double.PositiveInfinity, 0.9123, -1.308606791)] // Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0, -0.5123, -1.242453325)] // Continuous Uniform
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0, -0.6123, -1.242453325)] // Continuous Uniform
        [TestCase(0.0, 1.0, 0.77, 1.0, double.PositiveInfinity, 0.6123, -1.60129976)] // Skewed Laplace
        [TestCase(0.0, 1.0, 0.0, 1.0, double.PositiveInfinity, 0.6123, -1.212496555)] // Laplace
        [TestCase(0.9, 1.0, 0.0, 1.0, double.PositiveInfinity, 0.6123, -0.7534428322)] // Mean shifted Laplace
        public void ValidateDensityLn(double location, double scale, double skew, double p, double q, double x, double d)
        {
            var n = new SkewedGeneralizedT(location, scale, skew, p, q);
            var density = n.DensityLn(x);
            AssertHelpers.AlmostEqualRelative(d, density, 8);
        }

        [TestCase(0.0, 1.0, 0.0, 2.0, double.PositiveInfinity, 0.9123, 0.8191945928)] // Standard Normal distribution
        [TestCase(5.0, 1.0, 0.0, 2.0, double.PositiveInfinity, 4.123, 0.190243319)] // Mean shifted Normal distribution
        [TestCase(0.0, 2.0, 0.0, 2.0, double.PositiveInfinity, 0.9123, 0.6758589413)] // Scaled Normal distribution
        [TestCase(0.0, 1.0, 0.9, 2.0, double.PositiveInfinity, 0.9123, 0.8216671619)] // Skewed Normal distribution
        [TestCase(1.0, 1.5, 0.9, 2.0, double.PositiveInfinity, 0.9123, 0.5519973476)] // Mean shifted and scaled Skewed Normal distribution
        [TestCase(0.0, 1.0, -0.9, 2.0, 5.0, 0.9123, 0.8297526431)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, -0.9, 2.0, 5.0, -0.9123, 0.1624618933)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, 0.0, 2.0, 5.0, 0.9123, 0.8341106883)] // Student T distribution
        [TestCase(0.0, 1.0, -0.3, 2.2, double.PositiveInfinity, 0.9123, 0.8118701776)] // Skewed Generalized Error Distribution
        [TestCase(-1.5, 1.0, -0.3, 2.2, double.PositiveInfinity, 0.9123, 0.9987893207)] // Mean shifted Skewed Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, 2.2, double.PositiveInfinity, 0.9123, 0.8140902875)] // Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0, 0.5123, 0.6478882715)] // Continuous Uniform
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0, 0.6123, 0.6767557849)] // Continuous Uniform
        [TestCase(0.0, 1.0, 0.77, 1.0, double.PositiveInfinity, 0.6123, 0.8000467981)] // Skewed Laplace
        [TestCase(0.0, 1.0, 0.0, 1.0, double.PositiveInfinity, 0.6123, 0.7896684418)] // Laplace
        [TestCase(0.9, 1.0, 0.0, 1.0, double.PositiveInfinity, 0.6123, 0.3328656172)] // Mean shifted Laplace
        public void ValidateCDF(double location, double scale, double skew, double p, double q, double x, double pr)
        {
            var n = new SkewedGeneralizedT(location, scale, skew, p, q);
            var cpr = n.CumulativeDistribution(x);
            AssertHelpers.AlmostEqualRelative(pr, cpr, 8);
        }

        [TestCase(0.0, 1.0, 0.0, 2.0, double.PositiveInfinity, 0.9123, 1.355055108)] // Standard Normal distribution
        [TestCase(5.0, 1.0, 0.0, 2.0, double.PositiveInfinity, 0.4123, 4.778367525)] // Mean shifted Normal distribution
        [TestCase(0.0, 2.0, 0.0, 2.0, double.PositiveInfinity, 0.9123, 2.710110216)] // Scaled Normal distribution
        [TestCase(0.0, 1.0, 0.9, 2.0, double.PositiveInfinity, 0.9123, 1.506912119)] // Skewed Normal distribution
        [TestCase(1.0, 1.5, 0.9, 2.0, double.PositiveInfinity, 0.9123, 3.260368178)] // Mean shifted and scaled Skewed Normal distribution
        [TestCase(0.0, 1.0, -0.9, 2.0, 5.0, 0.9123, 1.068716999)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, -0.9, 2.0, 5.0, 0.123, -1.159154003)] // Skewed Student T distribution
        [TestCase(0.0, 1.0, 0.0, 2.0, 5.0, 0.9123, 1.304471125)] // Student T distribution
        [TestCase(0.0, 1.0, -0.3, 2.2, double.PositiveInfinity, 0.9123, 1.275451275)] // Skewed Generalized Error Distribution
        [TestCase(-1.5, 1.0, -0.3, 2.2, double.PositiveInfinity, 0.9123, -0.2245487247)] // Mean shifted Skewed Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, 2.2, double.PositiveInfinity, 0.9123, 1.363253899)] // Generalized Error Distribution
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0, 0.5123, 0.04260844987)] // Continuous Uniform
        [TestCase(0.0, 1.0, 0.0, double.PositiveInfinity, 1.0, 0.6123, 0.3890186114)] // Continuous Uniform
        [TestCase(0.0, 1.0, 0.77, 1.0, double.PositiveInfinity, 0.6123, -0.04432801722)] // Skewed Laplace
        [TestCase(0.0, 1.0, 0.0, 1.0, double.PositiveInfinity, 0.6123, 0.179871174)] // Laplace
        [TestCase(0.9, 1.0, 0.0, 1.0, double.PositiveInfinity, 0.6123, 1.079871174)] // Mean shifted Laplace
        public void ValidateInvCDF(double location, double scale, double skew, double p, double q, double quantile, double x)
        {
            var n = new SkewedGeneralizedT(location, scale, skew, p, q);
            var xq = n.InverseCumulativeDistribution(quantile);
            AssertHelpers.AlmostEqualRelative(x, xq, 8);

            AssertHelpers.AlmostEqualRelative(quantile, n.CumulativeDistribution(xq), 8);
        }

        [TestCase(0, 1.0, 0.4123)]
        [TestCase(1.5, 2.5, 0.5123)]
        [TestCase(-0.5, 5, 0.6123)]
        public void ValidateLaplaceDensityEquivalence(double location, double scale, double x)
        {
            var n = new SkewedGeneralizedT(location, scale, 0, 1, double.PositiveInfinity);

            var b = scale / Math.Sqrt(2.0);
            var l = new Laplace(location, b);

            AssertHelpers.AlmostEqualRelative(l.Density(x), n.Density(x), 8);
            AssertHelpers.AlmostEqualRelative(l.DensityLn(x), n.DensityLn(x), 8);
        }

        [TestCase(0, 1.0, 0.4123)]
        [TestCase(1.5, 2.5, 0.5123)]
        [TestCase(-0.5, 5, 0.6123)]
        public void ValidateNormalDensityEquivalence(double location, double scale, double x)
        {
            var sgt = new SkewedGeneralizedT(location, scale, 0, 2, double.PositiveInfinity);
            var n = new Normal(location, scale);

            AssertHelpers.AlmostEqualRelative(n.Density(x), sgt.Density(x), 8);
            AssertHelpers.AlmostEqualRelative(n.DensityLn(x), sgt.DensityLn(x), 8);
        }

        [TestCase(0, 1, -0.1, 0.5123)]
        [TestCase(0, 1, 0.1, 0.6123)]
        public void ValidateSkewedNormalDistribution(double location, double scale, double skew, double x)
        {
            var sn = new SkewedGeneralizedT(location, scale, skew, 2, double.PositiveInfinity);
            var n = new Normal(location, scale);

            var sp = sn.CumulativeDistribution(x);
            var p = n.CumulativeDistribution(x);

            if (skew > 0)
                Assert.IsTrue(sp > p);
            else
                Assert.IsTrue(sp < p);
        }

        [TestCase(0, 1, -0.1, 0.5123)]
        [TestCase(0, 1, 0.1, 0.6123)]
        public void ValidateModeOfSkewedNormalDistribution(double location, double scale, double skew, double x)
        {
            var sn = new SkewedGeneralizedT(location, scale, skew, 2, double.PositiveInfinity);
            var n = new Normal(location, scale);

            var sm = sn.Mode;
            var m = n.Mode;

            if (skew < 0)
                Assert.IsTrue(sm > m);
            else
                Assert.IsTrue(sm < m);
        }

        [TestCase(0, 1, -0.1, 3, 5, 0.5123)]
        [TestCase(0, 1, 0.1, 3, 5, 0.6123)]
        public void ValidateModeOfSkewedGeneralizedTDistribution(double location, double scale, double skew, double p, double q, double x)
        {
            var sn = new SkewedGeneralizedT(location, scale, skew, 2, double.PositiveInfinity);
            var n = new Normal(location, scale);

            var sm = sn.Mode;
            var m = n.Mode;

            if (skew < 0)
                Assert.IsTrue(sm > m);
            else
                Assert.IsTrue(sm < m);
        }

        [TestCase(-0.5)]
        [TestCase(0.0)]
        [TestCase(0.5)]
        public void ValidateSkewnessOfNormalDistribution(double skew)
        {
            var sn = new SkewedGeneralizedT(0.0, 1.0, skew, 2.0, double.PositiveInfinity);

            if (skew > 0)
                Assert.IsTrue(sn.Skewness > 0.0);
            else if (skew < 0)
                Assert.IsTrue(sn.Skewness < 0.0);
            else
                Assert.AreEqual(skew, sn.Skewness);
        }

        [TestCase(-0.5)]
        [TestCase(0.0)]
        [TestCase(0.5)]
        public void ValidateSkewnessOfSkewGeneralizedTDistribution(double skew)
        {
            var sn = new SkewedGeneralizedT(0.0, 1.0, skew, 3.0, 4.0);

            if (skew > 0)
                Assert.IsTrue(sn.Skewness > 0.0);
            else if (skew < 0)
                Assert.IsTrue(sn.Skewness < 0.0);
            else
                Assert.AreEqual(skew, sn.Skewness);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            SkewedGeneralizedT.Sample(0.0, 1.0, 0.3, 2.2, 5.6);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = SkewedGeneralizedT.Samples(0.0, 1.0, 0.3, 2.2, 5.6);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new SkewedGeneralizedT();
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new SkewedGeneralizedT();
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }
    }
}
