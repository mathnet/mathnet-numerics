// <copyright file="ZipfTests.cs" company="Math.NET">
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
    public class ZipfTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        [Row(0.1, 1)]
        [Row(0.1, 20)]
        [Row(0.1, 50)]
        [Row(1.0, 1)]
        [Row(1.0, 20)]
        [Row(1.0, 50)]
        public void CanCreateZipf(double s, int n)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual<double>(s, d.S);
            Assert.AreEqual<int>(n, d.N);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0, 1)]
        [Row(0.0, 20)]
        [Row(0.0, 50)]
        [Row(1.0, 0)]
        [Row(1.0, 0)]
        [Row(1.0, 0)]
        [Row(1.0, -10)]
        [Row(1.0, -10)]
        [Row(1.0, -10)]
        public void ZipfCreateFailsWithBadParameters(double s, int n)
        {
            var d = new Zipf(s, n);
        }

        [Test]
        public void ValidateToString()
        {
            var d = new Zipf(1.0, 5);
            Assert.AreEqual<string>("Zipf(S = 1, N = 5)", d.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(5.0)]
        public void CanSetS(double s)
        {
            var d = new Zipf(1.0, 5);
            d.S = s;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.0)]
        [Row(Double.NegativeInfinity)]
        public void SetSFails(double s)
        {
            var d = new Zipf(1.0, 5);
            d.S = s;
        }

        [Test]
        [Row(1)]
        [Row(20)]
        [Row(50)]
        public void CanSetN(int n)
        {
            var d = new Zipf(1.0, 5);
            d.N = n;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1)]
        [Row(0)]
        public void SetNFails(int n)
        {
            var d = new Zipf(1.0, 5);
            d.N = n;
        }

        [Test]
        [Row(0.1, 1, 0.0)]
        [Row(0.1, 20, 2.9924075515295949)]
        [Row(0.1, 50, 3.9078245132371388)]
        [Row(1.0, 1, 0.0)]
        [Row(1.0, 20, 2.5279968533953743)]
        [Row(1.0, 50, 3.1971263138845916)]
        public void ValidateEntropy(double s, int n, double e)
        {
            var d = new Zipf(s, n);
            AssertHelpers.AlmostEqual(e, d.Entropy, 15);
        }

        [Test]
        [Row(5.0, 1)]
        [Row(5.0, 20)]
        [Row(5.0, 50)]
        [Row(10.0, 1)]
        [Row(10.0, 20)]
        [Row(10.0, 50)]
        public void ValidateSkewness(double s, int n)
        {
            var d = new Zipf(s, n);
            if (s > 4)
            {
                Assert.AreEqual<double>((SpecialFunctions.GeneralHarmonic(n, s - 3) * Math.Pow(SpecialFunctions.GeneralHarmonic(n, s), 2) - SpecialFunctions.GeneralHarmonic(n, s - 1) * (3 * SpecialFunctions.GeneralHarmonic(n, s - 2) * SpecialFunctions.GeneralHarmonic(n, s) - Math.Pow(SpecialFunctions.GeneralHarmonic(n, s - 1), 2))) / Math.Pow(SpecialFunctions.GeneralHarmonic(n, s - 2) * SpecialFunctions.GeneralHarmonic(n, s) - Math.Pow(SpecialFunctions.GeneralHarmonic(n, s - 1), 2), 1.5), d.Skewness);
            }
        }

        [Test]
        [Row(0.1, 1)]
        [Row(0.1, 20)]
        [Row(0.1, 50)]
        [Row(1.0, 1)]
        [Row(1.0, 20)]
        [Row(1.0, 50)]
        public void ValidateMode(double s, int n)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual(1, d.Mode);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateMedian()
        {
            var d = new Zipf(1.0, 5);
            int m = d.Median;
        }

        [Test]
        public void ValidateMinimum()
        {
            var d = new Zipf(1.0, 5);
            Assert.AreEqual(1, d.Minimum);
        }

        [Test]
        [Row(0.1, 1)]
        [Row(0.1, 20)]
        [Row(0.1, 50)]
        [Row(1.0, 1)]
        [Row(1.0, 20)]
        [Row(1.0, 50)]
        public void ValidateMaximum(double s, int n)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual(n, d.Maximum);
        }

        [Test]
        [Row(0.1, 1, 1)]
        [Row(0.1, 20, 1)]
        [Row(0.1, 50, 1)]
        [Row(1.0, 1, 1)]
        [Row(1.0, 20, 1)]
        [Row(1.0, 50, 1)]
        [Row(0.1, 20, 15)]
        [Row(0.1, 50, 15)]
        [Row(1.0, 20, 15)]
        [Row(1.0, 50, 15)]
        public void ValidateProbability(double s, int n, int x)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual((1.0 / Math.Pow(x, s)) / SpecialFunctions.GeneralHarmonic(n, s), d.Probability(x));
        }

        [Test]
        [Row(0.1, 1, 1)]
        [Row(0.1, 20, 1)]
        [Row(0.1, 50, 1)]
        [Row(1.0, 1, 1)]
        [Row(1.0, 20, 1)]
        [Row(1.0, 50, 1)]
        [Row(0.1, 20, 15)]
        [Row(0.1, 50, 15)]
        [Row(1.0, 20, 15)]
        [Row(1.0, 50, 15)]
        public void ValidateProbabilityLn(double s, int n, int x)
        {
            var d = new Zipf(s, n);
            Assert.AreEqual(Math.Log(d.Probability(x)), d.ProbabilityLn(x));
        }

        [Test]
        public void CanSample()
        {
            var d = new Zipf(0.7, 5);
            var s = d.Sample();
            Assert.Between(s, 0, 5);
        }

        [Test]
        public void CanSampleSequence()
        {
            var d = new Zipf(0.7, 5);
            var ied = d.Samples();
            var e = ied.Take(1000).ToArray();
            foreach (var i in e)
            {
                Assert.Between(i, 0, 5);
            }
        }

        [Test]
        [Row(0.1, 1, 2)]
        [Row(0.1, 20, 2)]
        [Row(0.1, 50, 2)]
        [Row(1.0, 1, 2)]
        [Row(1.0, 20, 2)]
        [Row(1.0, 50, 2)]
        [Row(0.1, 20, 15)]
        [Row(0.1, 50, 15)]
        [Row(1.0, 20, 15)]
        [Row(1.0, 50, 15)]
        public void ValidateCumulativeDistribution(double s, int n, int x)
        {
            var d = new Zipf(s, n);
            var cdf = SpecialFunctions.GeneralHarmonic(x, s) / SpecialFunctions.GeneralHarmonic(n, s);
            AssertHelpers.AlmostEqual(cdf, d.CumulativeDistribution(x), 14);
        }
    }
}