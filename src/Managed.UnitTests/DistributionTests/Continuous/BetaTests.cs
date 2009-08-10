// <copyright file="BetaTests.cs" company="Math.NET">
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
    public class BetaTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.0)]
        [Row(1.0, 1.0)]
        [Row(9.0, 1.0)]
        [Row(5.0, 100.0)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.0)]
        public void CanCreateBeta(double a, double b)
        {
            var n = new Beta(a, b);
            AssertEx.AreEqual<double>(a, n.A);
            AssertEx.AreEqual<double>(b, n.B);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1.0)]
        [Row(1.0, Double.NaN)]
        [Row(Double.NaN, Double.NaN)]
        [Row(1.0, -1.0)]
        [Row(-1.0, 1.0)]
        [Row(-1.0, -1.0)]
        public void BetaCreateFailsWithBadParameters(double a, double b)
        {
            var n = new Beta(a, b);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Beta(1.0, 2.0);
            AssertEx.AreEqual<string>("Beta(A = 1, B = 2)", n.ToString());
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetShapeA(double a)
        {
            var n = new Beta(1.0, 1.0);
            n.A = a;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetShapeAFailsWithNegativeA()
        {
            var n = new Beta(1.0, 1.0);
            n.A = -1.0;
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetShapeB(double b)
        {
            var n = new Beta(1.0, 1.0);
            n.B = b;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetShapeBFailsWithNegativeB()
        {
            var n = new Beta(1.0, 1.0);
            n.B = -1.0;
        }

        [Test]
        [Row(0.0, 0.0, 0.5)]
        [Row(0.0, 0.1, 0.0)]
        [Row(1.0, 0.0, 1.0)]
        [Row(1.0, 1.0, 0.5)]
        [Row(9.0, 1.0, 0.9)]
        [Row(5.0, 100.0, 0.047619047619047619047616)]
        [Row(1.0, Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 1.0, 1.0)]
        [Row(0.0, Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 0.0, 1.0)]
        public void ValidateMean(double a, double b, double mean)
        {
            var n = new Beta(a, b);
            AssertEx.AreEqual<double>(mean, n.Mean);
        }

        [Test, Ignore("Depending on Special Functions")]
        [Row(0.0, 0.0, 0.5)]
        [Row(0.0, 0.1, 0.1)]
        [Row(1.0, 0.0, 1.0)]
        [Row(1.0, 1.0, 0.0)]
        [Row(9.0, 1.0, -1.3083356884473304939016015849561625204060922267565917)]
        [Row(5.0, 100.0, -2.5201623187602743679459255108827601222133603091753153)]
        [Row(1.0, Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 1.0, 0.0)]
        [Row(0.0, Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 0.0, 0.0)]
        public void ValidateEntropy(double a, double b, double entropy)
        {
            var n = new Beta(a, b);
            AssertEx.AreEqual<double>(entropy, n.Entropy);
        }

        [Test]
        [Row(0.0, 0.0, 0.0)]
        [Row(0.0, 0.1, 2.0)]
        [Row(1.0, 0.0, -2.0)]
        [Row(1.0, 1.0, 0.0)]
        [Row(9.0, 1.0, -1.4740554623801777107177478829647496373009282424841579)]
        [Row(5.0, 100.0, 0.81759410927553430354583159143895018978562196953345572)]
        [Row(1.0, Double.PositiveInfinity, 2.0)]
        [Row(Double.PositiveInfinity, 1.0, -2.0)]
        [Row(0.0, Double.PositiveInfinity, 2.0)]
        [Row(Double.PositiveInfinity, 0.0, -2.0)]
        public void ValidateSkewness(double a, double b, double skewness)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(skewness, n.Skewness, 15);
        }

        [Test]
        [Row(0.0, 0.0, 0.5)]
        [Row(0.0, 0.1, 0.0)]
        [Row(1.0, 0.0, 1.0)]
        [Row(1.0, 1.0, 0.5)]
        [Row(9.0, 1.0, 1.0)]
        [Row(5.0, 100.0, 0.038834951456310676243255386452801758423447608947753906)]
        [Row(1.0, Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 1.0, 1.0)]
        [Row(0.0, Double.PositiveInfinity, 0.0)]
        [Row(Double.PositiveInfinity, 0.0, 1.0)]
        public void ValidateMode(double a, double b, double mode)
        {
            var n = new Beta(a, b);
            AssertEx.AreEqual<double>(mode, n.Mode);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(1.0, 0.0)]
        [Row(1.0, 1.0)]
        [Row(9.0, 1.0)]
        [Row(5.0, 100.0)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(0.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 0.0)]
        public void ValidateMedian(double a, double b)
        {
            var n = new Beta(a, 1.0);
            var m = n.Median;
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new Beta(1.0, 1.0);
            AssertEx.AreEqual<double>(0.0, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new Beta(1.0, 1.0);
            AssertEx.AreEqual<double>(1.0, n.Maximum);
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = Beta.Sample(new Random(), 2.0, 3.0);
        }

        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Beta.Samples(new Random(), 2.0, 3.0);
            var arr = ied.Take(5).ToArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = Beta.Sample(new Random(), 1.0, -1.0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleSequenceStatic()
        {
            var ied = Beta.Samples(new Random(), 1.0, -1.0).First();
        }

        [Test]
        public void CanSample()
        {
            var n = new Beta(2.0, 3.0);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Beta(2.0, 3.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.0, 0.0, 0.0, Double.PositiveInfinity)]
        [Row(0.0, 0.0, 0.5, 0.0)]
        [Row(0.0, 0.0, 1.0, Double.PositiveInfinity)]
        [Row(0.0, 0.1, 0.0, Double.PositiveInfinity)]
        [Row(0.0, 0.1, 0.5, 0.0)]
        [Row(0.0, 0.1, 1.0, 0.0)]
        [Row(1.0, 0.0, 0.0, 0.0)]
        [Row(1.0, 0.0, 0.5, 0.0)]
        [Row(1.0, 0.0, 1.0, Double.PositiveInfinity)]
        [Row(1.0, 1.0, 0.0, 1.0)]
        [Row(1.0, 1.0, 0.5, 1.0)]
        [Row(1.0, 1.0, 1.0, 1.0)]
        [Row(9.0, 1.0, 0.0, 0.0)]
        [Row(9.0, 1.0, 0.5, 0.035155378090821160189479427593561667617370600927556366)]
        [Row(9.0, 1.0, 1.0, 8.9997767912502170085067334639517869100468738374544298)]
        [Row(5.0, 100.0, 0.0, 0.0)]
        [Row(5.0, 100.0, 0.5, 1.0881845516040810386311829462908430145307026037926335e-21)]
        [Row(5.0, 100.0, 1.0, 0.0)]
        [Row(1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity, 0.5, 0.0)]
        [Row(1.0, Double.PositiveInfinity, 1.0, 0.0)]
        [Row(Double.PositiveInfinity, 1.0, 0.0, 0.0)]
        [Row(Double.PositiveInfinity, 1.0, 0.5, 0.0)]
        [Row(Double.PositiveInfinity, 1.0, 1.0, Double.PositiveInfinity)]
        [Row(0.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [Row(0.0, Double.PositiveInfinity, 0.5, 0.0)]
        [Row(0.0, Double.PositiveInfinity, 1.0, 0.0)]
        [Row(Double.PositiveInfinity, 0.0, 0.0, 0.0)]
        [Row(Double.PositiveInfinity, 0.0, 0.5, 0.0)]
        [Row(Double.PositiveInfinity, 0.0, 1.0, Double.PositiveInfinity)]
        public void ValidateDensity(double a, double b, double x, double pdf)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(pdf, n.Density(x), 14);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, Double.PositiveInfinity)]
        [Row(0.0, 0.0, 0.5, Double.NegativeInfinity)]
        [Row(0.0, 0.0, 1.0, Double.PositiveInfinity)]
        [Row(0.0, 0.1, 0.0, Double.PositiveInfinity)]
        [Row(0.0, 0.1, 0.5, Double.NegativeInfinity)]
        [Row(0.0, 0.1, 1.0, Double.NegativeInfinity)]
        [Row(1.0, 0.0, 0.0, Double.NegativeInfinity)]
        [Row(1.0, 0.0, 0.5, Double.NegativeInfinity)]
        [Row(1.0, 0.0, 1.0, Double.PositiveInfinity)]
        [Row(1.0, 1.0, 0.0, 0.0)]
        [Row(1.0, 1.0, 0.5, 0.0)]
        [Row(1.0, 1.0, 1.0, 0.0)]
        [Row(9.0, 1.0, 0.0, Double.NegativeInfinity)]
        [Row(9.0, 1.0, 0.5, -3.3479528671433430925473664978203611353090199592365404)]
        [Row(9.0, 1.0, 1.0, 2.197199776056471990627201569939718235188380979206923)]
        [Row(5.0, 100.0, 0.0, Double.NegativeInfinity)]
        [Row(5.0, 100.0, 0.5, -51.447830024537682154565870837960406410586196074573801)]
        [Row(5.0, 100.0, 1.0, Double.NegativeInfinity)]
        [Row(1.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity, 0.5, Double.NegativeInfinity)]
        [Row(1.0, Double.PositiveInfinity, 1.0, Double.NegativeInfinity)]
        [Row(Double.PositiveInfinity, 1.0, 0.0, Double.NegativeInfinity)]
        [Row(Double.PositiveInfinity, 1.0, 0.5, Double.NegativeInfinity)]
        [Row(Double.PositiveInfinity, 1.0, 1.0, Double.PositiveInfinity)]
        [Row(0.0, Double.PositiveInfinity, 0.0, Double.PositiveInfinity)]
        [Row(0.0, Double.PositiveInfinity, 0.5, Double.NegativeInfinity)]
        [Row(0.0, Double.PositiveInfinity, 1.0, Double.NegativeInfinity)]
        [Row(Double.PositiveInfinity, 0.0, 0.0, Double.NegativeInfinity)]
        [Row(Double.PositiveInfinity, 0.0, 0.5, Double.NegativeInfinity)]
        [Row(Double.PositiveInfinity, 0.0, 1.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(double a, double b, double x, double pdfln)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(pdfln, n.DensityLn(x), 14);
        }

        [Test, Ignore("Depending on Special Functions")]
        [Row(0.0, 0.0, 0.0, 0.5)]
        [Row(0.0, 0.0, 0.5, 0.5)]
        [Row(0.0, 0.0, 1.0, 1.0)]
        [Row(0.0, 0.1, 0.0, 1.0)]
        [Row(0.0, 0.1, 0.5, 1.0)]
        [Row(0.0, 0.1, 1.0, 1.0)]
        [Row(1.0, 0.0, 0.0, 0.0)]
        [Row(1.0, 0.0, 0.5, 0.0)]
        [Row(1.0, 0.0, 1.0, 1.0)]
        [Row(1.0, 1.0, 0.0, 0.0)]
        [Row(1.0, 1.0, 0.5, 0.5)]
        [Row(1.0, 1.0, 1.0, 1.0)]
        [Row(9.0, 1.0, 0.0, 0.0)]
        [Row(9.0, 1.0, 0.5, 0.00195313)]
        [Row(9.0, 1.0, 1.0, 1.0)]
        [Row(5.0, 100.0, 0.0, 0.0)]
        [Row(5.0, 100.0, 0.5, 1.0)]
        [Row(5.0, 100.0, 1.0, 1.0)]
        [Row(1.0, Double.PositiveInfinity, 0.0, 1.0)]
        [Row(1.0, Double.PositiveInfinity, 0.5, 1.0)]
        [Row(1.0, Double.PositiveInfinity, 1.0, 1.0)]
        [Row(Double.PositiveInfinity, 1.0, 0.0, 0.0)]
        [Row(Double.PositiveInfinity, 1.0, 0.5, 0.0)]
        [Row(Double.PositiveInfinity, 1.0, 1.0, 1.0)]
        [Row(0.0, Double.PositiveInfinity, 0.0, 1.0)]
        [Row(0.0, Double.PositiveInfinity, 0.5, 1.0)]
        [Row(0.0, Double.PositiveInfinity, 1.0, 1.0)]
        [Row(Double.PositiveInfinity, 0.0, 0.0, 0.0)]
        [Row(Double.PositiveInfinity, 0.0, 0.5, 0.0)]
        [Row(Double.PositiveInfinity, 0.0, 1.0, 1.0)]
        public void ValidateCumulativeDistribution(double a, double b, double x, double cdf)
        {
            var n = new Beta(a, b);
            AssertHelpers.AlmostEqual(cdf, n.CumulativeDistribution(x), 15);
        }
    }
}
