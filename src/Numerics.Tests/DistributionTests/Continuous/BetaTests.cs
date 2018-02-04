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
    /// Beta distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class BetaTests
    {
        /// <summary>
        /// Can create Beta distribution.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(1.0, 0.0)]
        [TestCase(1.0, 1.0)]
        [TestCase(9.0, 1.0)]
        [TestCase(5.0, 100.0)]
        [TestCase(1.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        [TestCase(Double.PositiveInfinity, 0.0)]
        public void CanCreateBeta(double a, double b)
        {
            var n = new Beta(a, b);
            Assert.AreEqual(a, n.A);
            Assert.AreEqual(b, n.B);
        }

        /// <summary>
        /// Beta create fails with bad parameters.
        /// </summary>
        [Test]
        public void BetaCreateFailsWithBadParameters()
        {
            Assert.That(() => new Beta(Double.NaN, 1.0), Throws.ArgumentException);
            Assert.That(() => new Beta(1.0, Double.NaN), Throws.ArgumentException);
            Assert.That(() => new Beta(Double.NaN, Double.NaN), Throws.ArgumentException);
            Assert.That(() => new Beta(1.0, -1.0), Throws.ArgumentException);
            Assert.That(() => new Beta(-1.0, 1.0), Throws.ArgumentException);
            Assert.That(() => new Beta(-1.0, -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate to string.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Beta(1d, 2d);
            Assert.AreEqual("Beta(α = 1, β = 2)", n.ToString());
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="mean">Mean value.</param>
        [TestCase(0.0, 0.0, 0.5)]
        [TestCase(0.0, 0.1, 0.0)]
        [TestCase(1.0, 0.0, 1.0)]
        [TestCase(1.0, 1.0, 0.5)]
        [TestCase(9.0, 1.0, 0.9)]
        [TestCase(5.0, 100.0, 0.047619047619047619047616)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 1.0)]
        public void ValidateMean(double a, double b, double mean)
        {
            var n = new Beta(a, b);
            Assert.AreEqual(mean, n.Mean);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="entropy">Entropy value.</param>
        [TestCase(0.0, 0.0, 0.693147180559945309417232121458176568075500134360255)]
        [TestCase(0.0, 0.1, 0.0)]
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(1.0, 1.0, 0.0)]
        [TestCase(9.0, 1.0, -1.3083356884473304939016015849561625204060922267565917)]
        [TestCase(5.0, 100.0, -2.5201623187602743679459255108827601222133603091753153)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(0.0, Double.PositiveInfinity, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.0)]
        public void ValidateEntropy(double a, double b, double entropy)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqualRelative(entropy, n.Entropy, 13);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="skewness">Skewness value.</param>
        [TestCase(0.0, 0.0, 0.0)]
        [TestCase(0.0, 0.1, 2.0)]
        [TestCase(1.0, 0.0, -2.0)]
        [TestCase(1.0, 1.0, 0.0)]
        [TestCase(9.0, 1.0, -1.4740554623801777107177478829647496373009282424841579)]
        [TestCase(5.0, 100.0, 0.81759410927553430354583159143895018978562196953345572)]
        [TestCase(1.0, Double.PositiveInfinity, 2.0)]
        [TestCase(Double.PositiveInfinity, 1.0, -2.0)]
        [TestCase(0.0, Double.PositiveInfinity, 2.0)]
        [TestCase(Double.PositiveInfinity, 0.0, -2.0)]
        public void ValidateSkewness(double a, double b, double skewness)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqualRelative(skewness, n.Skewness, 14);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="mode">Mode value.</param>
        [TestCase(0.0, 0.0, 0.5)]
        [TestCase(0.0, 0.1, 0.0)]
        [TestCase(1.0, 0.0, 1.0)]
        [TestCase(1.0, 1.0, 0.5)]
        [TestCase(9.0, 1.0, 1.0)]
        [TestCase(5.0, 100.0, 0.038834951456310676243255386452801758423447608947753906)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 1.0)]
        public void ValidateMode(double a, double b, double mode)
        {
            var n = new Beta(a, b);
            Assert.AreEqual(mode, n.Mode);
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var n = new Beta(0.0, 1.0);
            Assert.Throws<NotSupportedException>(() => { var m = n.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Beta(1.0, 1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Beta(1.0, 1.0);
            Assert.AreEqual(1.0, n.Maximum);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Beta.Sample(new Random(0), 2.0, 3.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Beta.Samples(new Random(0), 2.0, 3.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with wrong parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => Beta.Sample(new Random(0), 1.0, -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with wrong parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => Beta.Samples(new Random(0), 1.0, -1.0).First(), Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Beta(2.0, 3.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Beta(2.0, 3.0);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="x">Input value X.</param>
        /// <param name="pdf">Density value.</param>
        [TestCase(0.0, 0.0, 0.0, Double.PositiveInfinity)]
        [TestCase(0.0, 0.0, 0.5, 0.0)]
        [TestCase(0.0, 0.0, 1.0, Double.PositiveInfinity)]
        [TestCase(0.0, 0.1, 0.0, Double.PositiveInfinity)]
        [TestCase(0.0, 0.1, 0.5, 0.0)]
        [TestCase(0.0, 0.1, 1.0, 0.0)]
        [TestCase(1.0, 0.0, 0.0, 0.0)]
        [TestCase(1.0, 0.0, 0.5, 0.0)]
        [TestCase(1.0, 0.0, 1.0, Double.PositiveInfinity)]
        [TestCase(1.0, 1.0, 0.0, 1.0)]
        [TestCase(1.0, 1.0, 0.5, 1.0)]
        [TestCase(1.0, 1.0, 1.0, 1.0)]
        [TestCase(9.0, 1.0, 0.0, 0.0)]
        [TestCase(9.0, 1.0, 0.5, 0.03515625)]
        [TestCase(9.0, 1.0, 1.0, 9.0)]
        [TestCase(9.0, 1.0, -1.0, 0.0)]
        [TestCase(9.0, 1.0, 2.0, 0.0)]
        [TestCase(5.0, 100, 0.0, 0.0)]
        [TestCase(5.0, 100, 0.5, 1.0881845516040810386311829462908430145307026037926335e-21)]
        [TestCase(5.0, 100, 1.0, 0.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity, 0.5, 0.0)]
        [TestCase(1.0, Double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 1.0, Double.PositiveInfinity)]
        [TestCase(0.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [TestCase(0.0, Double.PositiveInfinity, 0.5, 0.0)]
        [TestCase(0.0, Double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 1.0, Double.PositiveInfinity)]
        public void ValidateDensity(double a, double b, double x, double pdf)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqualRelative(pdf, n.Density(x), 12);
            AssertHelpers.AlmostEqualRelative(pdf, Beta.PDF(a, b, x), 12);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="x">Input value X.</param>
        /// <param name="pdfln">Density log value.</param>
        [TestCase(0.0, 0.0, 0.0, Double.PositiveInfinity)]
        [TestCase(0.0, 0.0, 0.5, Double.NegativeInfinity)]
        [TestCase(0.0, 0.0, 1.0, Double.PositiveInfinity)]
        [TestCase(0.0, 0.1, 0.0, Double.PositiveInfinity)]
        [TestCase(0.0, 0.1, 0.5, Double.NegativeInfinity)]
        [TestCase(0.0, 0.1, 1.0, Double.NegativeInfinity)]
        [TestCase(1.0, 0.0, 0.0, Double.NegativeInfinity)]
        [TestCase(1.0, 0.0, 0.5, Double.NegativeInfinity)]
        [TestCase(1.0, 0.0, 1.0, Double.PositiveInfinity)]
        [TestCase(1.0, 1.0, 0.0, 0.0)]
        [TestCase(1.0, 1.0, 0.5, 0.0)]
        [TestCase(1.0, 1.0, 1.0, 0.0)]
        [TestCase(9.0, 1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(9.0, 1.0, 0.5, -3.3479528671433430925473664978203611353090199592365458)]
        [TestCase(9.0, 1.0, 1.0, 2.1972245773362193827904904738450514092949811156454996)]
        [TestCase(9.0, 1.0, -1.0, Double.NegativeInfinity)]
        [TestCase(9.0, 1.0, 2.0, Double.NegativeInfinity)]
        [TestCase(5.0, 100, 0.0, Double.NegativeInfinity)]
        [TestCase(5.0, 100, 0.5, -51.447830024537682154565870837960406410586196074573801)]
        [TestCase(5.0, 100, 1.0, Double.NegativeInfinity)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [TestCase(1.0, Double.PositiveInfinity, 0.5, Double.NegativeInfinity)]
        [TestCase(1.0, Double.PositiveInfinity, 1.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.5, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 1.0, 1.0, Double.PositiveInfinity)]
        [TestCase(0.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [TestCase(0.0, Double.PositiveInfinity, 0.5, Double.NegativeInfinity)]
        [TestCase(0.0, Double.PositiveInfinity, 1.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.0, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.5, Double.NegativeInfinity)]
        [TestCase(Double.PositiveInfinity, 0.0, 1.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(double a, double b, double x, double pdfln)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqualRelative(pdfln, n.DensityLn(x), 13);
            AssertHelpers.AlmostEqualRelative(pdfln, Beta.PDFLn(a, b, x), 13);
        }

        [TestCase(0.0, 0.0, 0.0, 0.5)]
        [TestCase(0.0, 0.0, 0.5, 0.5)]
        [TestCase(0.0, 0.0, 1.0, 1.0)]
        [TestCase(0.0, 0.1, 0.0, 1.0)]
        [TestCase(0.0, 0.1, 0.5, 1.0)]
        [TestCase(0.0, 0.1, 1.0, 1.0)]
        [TestCase(1.0, 0.0, 0.0, 0.0)]
        [TestCase(1.0, 0.0, 0.5, 0.0)]
        [TestCase(1.0, 0.0, 1.0, 1.0)]
        [TestCase(1.0, 1.0, 0.0, 0.0)]
        [TestCase(1.0, 1.0, 0.5, 0.5)]
        [TestCase(1.0, 1.0, 1.0, 1.0)]
        [TestCase(9.0, 1.0, 0.0, 0.0)]
        [TestCase(9.0, 1.0, 0.5, 0.001953125)]
        [TestCase(9.0, 1.0, 1.0, 1.0)]
        [TestCase(5.0, 100, 0.0, 0.0)]
        [TestCase(5.0, 100, 0.5, 1.0)]
        [TestCase(5.0, 100, 1.0, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.0, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 0.5, 1.0)]
        [TestCase(1.0, Double.PositiveInfinity, 1.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 0.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 1.0, 1.0, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity, 0.0, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity, 0.5, 1.0)]
        [TestCase(0.0, Double.PositiveInfinity, 1.0, 1.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.0, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 0.5, 0.0)]
        [TestCase(Double.PositiveInfinity, 0.0, 1.0, 1.0)]
        public void ValidateCumulativeDistribution(double a, double b, double x, double p)
        {
            var dist = new Beta(a, b);
            Assert.That(dist.CumulativeDistribution(x), Is.EqualTo(p).Within(1e-13));
            Assert.That(Beta.CDF(a, b, x), Is.EqualTo(p).Within(1e-13));
        }

        [TestCase(1.0, 1.0, 1.0, 1.0)]
        [TestCase(9.0, 1.0, 0.0, 0.0)]
        [TestCase(9.0, 1.0, 0.5, 0.001953125)]
        [TestCase(9.0, 1.0, 1.0, 1.0)]
        [TestCase(5.0, 100, 0.0, 0.0)]
        public void ValidateInverseCumulativeDistribution(double a, double b, double x, double p)
        {
            var dist = new Beta(a, b);
            Assert.That(dist.InverseCumulativeDistribution(p), Is.EqualTo(x).Within(1e-6));
            Assert.That(Beta.InvCDF(a, b, p), Is.EqualTo(x).Within(1e-6));
        }
    }
}
