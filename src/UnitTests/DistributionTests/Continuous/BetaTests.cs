// <copyright file="BetaTests.cs" company="Math.NET">
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
    /// Beta distribution tests.
    /// </summary>
    [TestFixture]
    public class BetaTests
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
        /// Can create Beta distribution.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        [Test, Sequential]
        public void CanCreateBeta([Values(0.0, 0.0, 1.0, 1.0, 9.0, 5.0, 1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)] double a, [Values(0.0, 0.1, 0.0, 1.0, 1.0, 100.0, Double.PositiveInfinity, 1.0, Double.PositiveInfinity, 0.0)] double b)
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
            Assert.Throws<ArgumentOutOfRangeException>(() => new Beta(Double.NaN, 1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Beta(1.0, Double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Beta(Double.NaN, Double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Beta(1.0, -1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Beta(-1.0, 1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Beta(-1.0, -1.0));
        }

        /// <summary>
        /// Validate to string.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Beta(1.0, 2.0);
            Assert.AreEqual("Beta(A = 1, B = 2)", n.ToString());
        }

        /// <summary>
        /// Can Set Shape A
        /// </summary>
        /// <param name="a">New A value.</param>
        [Test]
        public void CanSetShapeA([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double a)
        {
            new Beta(1.0, 1.0)
            {
                A = a
            };
        }

        /// <summary>
        /// Set A fails with negative A.
        /// </summary>
        [Test]
        public void SetShapeAFailsWithNegativeA()
        {
            var n = new Beta(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.A = -1.0);
        }

        /// <summary>
        /// Can set shape B.
        /// </summary>
        /// <param name="b">New B value.</param>
        [Test]
        public void CanSetShapeB([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double b)
        {
            new Beta(1.0, 1.0)
            {
                B = b
            };
        }

        /// <summary>
        /// Set shape B fails with negative B.
        /// </summary>
        [Test, Sequential]
        public void SetShapeBFailsWithNegativeB()
        {
            var n = new Beta(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.B = -1.0);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="mean">Mean value.</param>
        [Test, Sequential]
        public void ValidateMean([Values(0.0, 0.0, 1.0, 1.0, 9.0, 5.0, 1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)] double a, [Values(0.0, 0.1, 0.0, 1.0, 1.0, 100.0, Double.PositiveInfinity, 1.0, Double.PositiveInfinity, 0.0)] double b, [Values(0.5, 0.0, 1.0, 0.5, 0.9, 0.047619047619047619047616, 0.0, 1.0, 0.0, 1.0)] double mean)
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
        [Test, Sequential]
        public void ValidateEntropy([Values(0.0, 0.0, 1.0, 1.0, 9.0, 5.0, 1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)] double a, [Values(0.0, 0.1, 0.0, 1.0, 1.0, 100.0, Double.PositiveInfinity, 1.0, Double.PositiveInfinity, 0.0)] double b, [Values(0.693147180559945309417232121458176568075500134360255, 0.0, 0.0, 0.0, -1.3083356884473304939016015849561625204060922267565917, -2.5201623187602743679459255108827601222133603091753153, 0.0, 0.0, 0.0, 0.0)] double entropy)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(entropy, n.Entropy, 14);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="skewness">Skewness value.</param>
        [Test, Sequential]
        public void ValidateSkewness([Values(0.0, 0.0, 1.0, 1.0, 9.0, 5.0, 1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)] double a, [Values(0.0, 0.1, 0.0, 1.0, 1.0, 100.0, Double.PositiveInfinity, 1.0, Double.PositiveInfinity, 0.0)] double b, [Values(0.0, 2.0, -2.0, 0.0, -1.4740554623801777107177478829647496373009282424841579, 0.81759410927553430354583159143895018978562196953345572, 2.0, -2.0, 2.0, -2.0)] double skewness)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(skewness, n.Skewness, 15);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="mode">Mode value.</param>
        [Test, Sequential]
        public void ValidateMode([Values(0.0, 0.0, 1.0, 1.0, 9.0, 5.0, 1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)] double a, [Values(0.0, 0.1, 0.0, 1.0, 1.0, 100.0, Double.PositiveInfinity, 1.0, Double.PositiveInfinity, 0.0)] double b, [Values(0.5, 0.0, 1.0, 0.5, 1.0, 0.038834951456310676243255386452801758423447608947753906, 0.0, 1.0, 0.0, 1.0)] double mode)
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
            Beta.Sample(new Random(), 2.0, 3.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Beta.Samples(new Random(), 2.0, 3.0);
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Fail sample static with wrong parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Beta.Sample(new Random(), 1.0, -1.0));
        }

        /// <summary>
        /// Fail sample sequence static with wrong parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Beta.Samples(new Random(), 1.0, -1.0).First());
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
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="x">Input value X.</param>
        /// <param name="pdf">Density value.</param>
        [Test, Sequential]
        public void ValidateDensity(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 9.0, 9.0, 9.0, 9.0, 9.0, 5.0, 5.0, 5.0, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double a, 
            [Values(0.0, 0.0, 0.0, 0.1, 0.1, 0.1, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 100, 100, 100, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0)] double b, 
            [Values(0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, -1.0, 2.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0)] double x, 
            [Values(Double.PositiveInfinity, 0.0, Double.PositiveInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0, 0.0, Double.PositiveInfinity, 1.0, 1.0, 1.0, 0.0, 0.03515625, 9.0, 0.0, 0.0, 0.0, 1.0881845516040810386311829462908430145307026037926335e-21, 0.0, Double.PositiveInfinity, 0.0, 0.0, 0.0, 0.0, Double.PositiveInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0, 0.0, Double.PositiveInfinity)] double pdf)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(pdf, n.Density(x), 13);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="x">Input value X.</param>
        /// <param name="pdfln">Density log value.</param>
        [Test, Sequential]
        public void ValidateDensityLn(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 9.0, 9.0, 9.0, 9.0, 9.0, 5.0, 5.0, 5.0, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double a, 
            [Values(0.0, 0.0, 0.0, 0.1, 0.1, 0.1, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 100, 100, 100, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0)] double b, 
            [Values(0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, -1.0, 2.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0)] double x, 
            [Values(Double.PositiveInfinity, Double.NegativeInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0, Double.NegativeInfinity, -3.3479528671433430925473664978203611353090199592365458, 2.1972245773362193827904904738450514092949811156454996, Double.NegativeInfinity, Double.NegativeInfinity, Double.NegativeInfinity, -51.447830024537682154565870837960406410586196074573801, Double.NegativeInfinity, Double.PositiveInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.NegativeInfinity, Double.PositiveInfinity)] double pdfln)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(pdfln, n.DensityLn(x), 14);
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="a">Parameter A.</param>
        /// <param name="b">Parameter B.</param>
        /// <param name="x">Input value X.</param>
        /// <param name="cdf">Cumulative distribution value.</param>
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 9.0, 9.0, 9.0, 5.0, 5.0, 5.0, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double a, 
            [Values(0.0, 0.0, 0.0, 0.1, 0.1, 0.1, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 100, 100, 100, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, 0.0, 0.0, 0.0)] double b, 
            [Values(0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0)] double x, 
            [Values(0.5, 0.5, 1.0, 1.0, 1.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.5, 1.0, 0.0, 0.001953125, 1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 0.0, 0.0, 1.0)] double cdf)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(cdf, n.CumulativeDistribution(x), 13);
        }
    }
}
