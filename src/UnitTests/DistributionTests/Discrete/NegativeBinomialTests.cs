// <copyright file="NegativeBinomialTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Discrete
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using Distributions;

    [TestFixture]
    public class NegativeBinomialTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.3)]
        [Row(0.0, 1.0)]
        [Row(0.1, 0.0)]
        [Row(0.1, 0.3)]
        [Row(0.1, 1.0)]
        [Row(1.0, 0.0)]
        [Row(1.0, 0.3)]
        [Row(1.0, 1.0)]
        public void CanCreateNegativeBinomial(double r, double p)
        {
            var d = new NegativeBinomial(r, p);
            Assert.AreEqual<double>(r, d.R);
            Assert.AreEqual<double>(p, d.P);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0, Double.NaN)]
        [Row(0.0, -1.0)]
        [Row(0.0, 2.0)]
        [Row(Double.NegativeInfinity, 0.0)]
        [Row(-1.0, 0.3)]
        [Row(Double.NaN, 1.0)]
        [Row(Double.NegativeInfinity, Double.NaN)]
        [Row(Double.NaN, Double.NaN)]
        public void NegativeBinomialCreateFailsWithBadParameters(double r, double p)
        {
            var d = new NegativeBinomial(r, p);
        }

        [Test]
        public void ValidateToString()
        {
            var d = new NegativeBinomial(1.0, 0.3);
            Assert.AreEqual(String.Format("NegativeBinomial(R = {0}, P = {1})", d.R, d.P), d.ToString());
        }

        [Test]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        public void CanSetR(double r)
        {
            var d = new NegativeBinomial(1.0, 0.5);
            d.R = r;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.0)]
        [Row(Double.NegativeInfinity)]
        public void SetRFails(double r)
        {
            var d = new NegativeBinomial(1.0, 0.5);
            d.R = r;
        }

        [Test]
        [Row(0.0)]
        [Row(0.3)]
        [Row(1.0)]
        public void CanSetProbabilityOfOne(double p)
        {
            var d = new NegativeBinomial(1.0, 0.5);
            d.P = p;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.0)]
        [Row(2.0)]
        public void SetProbabilityOfOneFails(double p)
        {
            var d = new NegativeBinomial(1.0, 0.5);
            d.P = p;
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ValidateEntropy()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            var e = d.Entropy;
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.3)]
        [Row(0.0, 1.0)]
        [Row(0.1, 0.0)]
        [Row(0.1, 0.3)]
        [Row(0.1, 1.0)]
        [Row(1.0, 0.0)]
        [Row(1.0, 0.3)]
        [Row(1.0, 1.0)]
        public void ValidateSkewness(double r, double p)
        {
            var b = new NegativeBinomial(r, p);
            Assert.AreEqual<double>((2.0 - p) / Math.Sqrt(r * (1.0 - p)), b.Skewness);
        }

        [Test]
        [Row(0.0, 0)]
        [Row(0.3, 0)]
        [Row(1.0, 1)]
        public void ValidateMode(double r, double p)
        {
            var d = new NegativeBinomial(r, p);
            if (r > 1)
            {
                Assert.AreEqual<double>((int)Math.Floor((r - 1.0) * (1.0 - p) / p), d.Mode);
            }
            else
            {
                Assert.AreEqual<double>(0.0, d.Mode);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ValidateMedian()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            int m = d.Median;
        }

        [Test]
        public void ValidateMinimum()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            Assert.AreEqual(0, d.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var d = new NegativeBinomial(1.0, 0.3);
            Assert.AreEqual(int.MaxValue, d.Maximum);
        }

        [Test]
        [Row(0.0, 0.0, 5)]
        [Row(0.0, 0.3, 3)]
        [Row(0.0, 1.0, 0)]
        [Row(0.1, 0.0, 2)]
        [Row(0.1, 0.3, 1)]
        [Row(0.1, 1.0, 2)]
        [Row(1.0, 0.0, 2)]
        [Row(1.0, 0.3, 10)]
        [Row(1.0, 1.0, 5)]
        public void ValidateProbability(double r, double p, int x)
        {
            var d = new NegativeBinomial(r, p);
            Assert.AreEqual(Math.Exp(SpecialFunctions.GammaLn(r + x) - SpecialFunctions.GammaLn(r) - SpecialFunctions.GammaLn(x + 1.0) + r * Math.Log(p) + x * Math.Log(1.0 - p)), d.Probability(x));
        }

        [Test]
        [Row(0.0, 0.0, 5)]
        [Row(0.0, 0.3, 3)]
        [Row(0.0, 1.0, 0)]
        [Row(0.1, 0.0, 2)]
        [Row(0.1, 0.3, 1)]
        [Row(0.1, 1.0, 2)]
        [Row(1.0, 0.0, 2)]
        [Row(1.0, 0.3, 10)]
        [Row(1.0, 1.0, 5)]
        public void ValidateProbabilityLn(double r, double p, int x)
        {
            var d = new NegativeBinomial(r, p);
            Assert.AreEqual(SpecialFunctions.GammaLn(r + x) - SpecialFunctions.GammaLn(r) - SpecialFunctions.GammaLn(x + 1.0) + r * Math.Log(p) + x * Math.Log(1.0 - p), d.ProbabilityLn(x));
        }

        [Test]
        [Row(0.0, 0.0, 5)]
        [Row(0.0, 0.3, 3)]
        [Row(0.0, 1.0, 0)]
        [Row(0.1, 0.0, 2)]
        [Row(0.1, 0.3, 1)]
        [Row(0.1, 1.0, 2)]
        [Row(1.0, 0.0, 2)]
        [Row(1.0, 0.3, 10)]
        [Row(1.0, 1.0, 5)]
        public void ValidateCumulativeDistribution(double r, double p, int x)
        {
            var d = new NegativeBinomial(r, p);
            Assert.AreApproximatelyEqual(SpecialFunctions.BetaRegularized(r, x + 1.0, p), d.CumulativeDistribution(x), 1e-12);
        }

        [Test]
        public void CanSample()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            var s = d.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var d = new NegativeBinomial(1.0, 0.5);
            var ied = d.Samples();
            var e = ied.Take(5).ToArray();
        }
    }
}