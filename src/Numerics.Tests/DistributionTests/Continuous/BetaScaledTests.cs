// <copyright file="BetaTests.cs" company="Math.NET">
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
    /// BetaScaled distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class BetaScaledTests
    {
        /// <summary>
        /// Can create BetaScaled distribution.
        /// </summary>
        [TestCase(1.0, 1.0, -1.0, 1.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0)]
        [TestCase(5.0, 100.0, 0.0, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, Double.PositiveInfinity, 1.0)]
        public void CanCreateBetaScaled(double a, double b, double location, double scale)
        {
            var n = new BetaScaled(a, b, location, scale);
            Assert.AreEqual(a, n.A);
            Assert.AreEqual(b, n.B);
            Assert.AreEqual(location, n.Location);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// BetaScaled create fails with bad parameters.
        /// </summary>
        [Test]
        public void BetaScaledCreateFailsWithBadParameters()
        {
            Assert.That(() => new BetaScaled(Double.NaN, 1.0, 0.0, 1.0), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(1.0, Double.NaN, 0.0, 1.0), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(Double.NaN, Double.NaN, 0.0, 1.0), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(1.0, 1.0, Double.NaN, 1.0), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(1.0, 1.0, 1.0, Double.NaN), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(1.0, 0.0, 0.0, 1.0), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(0.0, 1.0, 0.0, 1.0), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(-1.0, -1.0, 0.0, 1.0), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(1.0, 1.0, 1.0, 0.0), Throws.ArgumentException);
            Assert.That(() => new BetaScaled(1.0, 1.0, 1.0, -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate to string.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new BetaScaled(1d, 2d, 0.0, 1.0);
            Assert.AreEqual("BetaScaled(α = 1, β = 2, μ = 0, σ = 1)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.5)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.9)]
        [TestCase(5.0, 100.0, 0.0, 1.0, 0.047619047619047619047616)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 1.0)]
        public void ValidateMean(double a, double b, double location, double scale, double mean)
        {
            var n = new BetaScaled(a, b, location, scale);
            Assert.AreEqual(mean, n.Mean);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, -1.4740554623801777107177478829647496373009282424841579)]
        [TestCase(5.0, 100.0, 0.0, 1.0, 0.81759410927553430354583159143895018978562196953345572)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 2.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, -2.0)]
        public void ValidateSkewness(double a, double b, double location, double scale, double skewness)
        {
            var n = new BetaScaled(a, b, location, scale);
            AssertHelpers.AlmostEqualRelative(skewness, n.Skewness, 14);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.5)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 1.0)]
        [TestCase(5.0, 100.0, 0.0, 1.0, 0.038834951456310676243255386452801758423447608947753906)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 1.0)]
        public void ValidateMode(double a, double b, double location, double scale, double mode)
        {
            var n = new BetaScaled(a, b, location, scale);
            Assert.AreEqual(mode, n.Mode);
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var n = new BetaScaled(1.0, 1.0, 0.0, 1.0);
            Assert.Throws<NotSupportedException>(() => { var m = n.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new BetaScaled(1.0, 1.0, 0.0, 1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new BetaScaled(1.0, 1.0, 0.0, 1.0);
            Assert.AreEqual(1.0, n.Maximum);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            BetaScaled.Sample(new Random(0), 2.0, 3.0, 0.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = BetaScaled.Samples(new Random(0), 2.0, 3.0, 0.0, 1.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with wrong parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => BetaScaled.Sample(new Random(0), 1.0, -1.0, 0.0, 1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with wrong parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => BetaScaled.Samples(new Random(0), 1.0, -1.0, 0.0, 1.0).First(), Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new BetaScaled(2.0, 3.0, 0.0, 1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new BetaScaled(2.0, 3.0, 0.0, 1.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <remarks>Reference: N[PDF[TransformedDistribution[l + s d, d \[Distributed] BetaDistribution[a, b]], x], 20]</remarks>
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.0, 1.0)]
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.5, 1.0)]
        [TestCase(1.0, 1.0, 0.0, 1.0, 1.0, 1.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.5, 0.03515625)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 1.0, 9.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, -1.0, 0.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 2.0, 0.0)]
        [TestCase(9.0, 1.0, -2.0, 2.0, -0.5, 0.450508)]
        [TestCase(5.0, 100, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(5.0, 100, 0.0, 1.0, 0.5, 1.0881845516040810386311829462908430145307026037926335e-21)]
        [TestCase(5.0, 100, 0.0, 1.0, 1.0, 0.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 0.0, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 0.5, 0.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 1.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 0.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 1.0, Double.PositiveInfinity)]
        public void ValidateDensity(double a, double b, double location, double scale, double x, double pdf)
        {
            var n = new BetaScaled(a, b, location, scale);
            AssertHelpers.AlmostEqualRelative(pdf, n.Density(x), 5);
            AssertHelpers.AlmostEqualRelative(pdf, BetaScaled.PDF(a, b, location, scale, x), 5);
        }

        /// <remarks>Reference: N[Log[PDF[TransformedDistribution[l + s d, d \[Distributed] BetaDistribution[a, b]], x]], 20]</remarks>
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.5, 0.0)]
        [TestCase(1.0, 1.0, 0.0, 1.0, 1.0, 0.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.5, -3.3479528671433430925473664978203611353090199592365458)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 1.0, 2.1972245773362193827904904738450514092949811156454996)]
        [TestCase(9.0, 1.0, 0.0, 1.0, -1.0, Double.NegativeInfinity)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 2.0, Double.NegativeInfinity)]
        [TestCase(9.0, 1.0, -2.0, 2.0, -0.5, -0.797379)]
        [TestCase(5.0, 100, 0.0, 1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(5.0, 100, 0.0, 1.0, 0.5, -51.447830024537682154565870837960406410586196074573801)]
        [TestCase(5.0, 100, 0.0, 1.0, 1.0, Double.NegativeInfinity)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 0.0, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 0.5, Double.NegativeInfinity)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 1.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 0.5, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 1.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(double a, double b, double location, double scale, double x, double pdfln)
        {
            var n = new BetaScaled(a, b, location, scale);
            AssertHelpers.AlmostEqualRelative(pdfln, n.DensityLn(x), 5);
            AssertHelpers.AlmostEqualRelative(pdfln, BetaScaled.PDFLn(a, b, location, scale, x), 5);
        }

        /// <remarks>Reference: N[CDF[TransformedDistribution[l + s d, d \[Distributed] BetaDistribution[a, b]], x], 20]</remarks>
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(1.0, 1.0, 0.0, 1.0, 0.5, 0.5)]
        [TestCase(1.0, 1.0, 0.0, 1.0, 1.0, 1.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.5, 0.001953125)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 1.0, 1.0)]
        [TestCase(9.0, 1.0, -2.0, 2.0, -0.5, 0.0750847)]
        [TestCase(5.0, 100, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(5.0, 100, 0.0, 1.0, 0.5, 1.0)]
        [TestCase(5.0, 100, 0.0, 1.0, 1.0, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 0.0, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 0.5, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0, 1.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 0.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 1.0, 1.0, 1.0)]
        public void ValidateCumulativeDistribution(double a, double b, double location, double scale, double x, double p)
        {
            var dist = new BetaScaled(a, b, location, scale);
            Assert.That(dist.CumulativeDistribution(x), Is.EqualTo(p).Within(1e-5));
            Assert.That(BetaScaled.CDF(a, b, location, scale, x), Is.EqualTo(p).Within(1e-5));
        }

        [TestCase(1.0, 1.0, 0.0, 1.0, 1.0, 1.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.0, 0.0)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 0.5, 0.001953125)]
        [TestCase(9.0, 1.0, 0.0, 1.0, 1.0, 1.0)]
        [TestCase(5.0, 100, 0.0, 1.0, 0.0, 0.0)]
        public void ValidateInverseCumulativeDistribution(double a, double b, double location, double scale, double x, double p)
        {
            var dist = new BetaScaled(a, b, location, scale);
            Assert.That(dist.InverseCumulativeDistribution(p), Is.EqualTo(x).Within(1e-6));
            Assert.That(BetaScaled.InvCDF(a, b, location, scale, p), Is.EqualTo(x).Within(1e-6));
        }
    }
}
