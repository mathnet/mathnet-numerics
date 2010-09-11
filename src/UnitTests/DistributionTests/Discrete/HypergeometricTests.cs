// <copyright file="HypergeometricTests.cs" company="Math.NET">
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
    public class HypergeometricTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        [Row(0, 0, 0)]
        [Row(1, 1, 1)]
        [Row(2, 1, 1)]
        [Row(2, 2, 2)]
        [Row(10, 1, 1)]
        [Row(10, 5, 3)]
        public void CanCreateHypergeometric(int N, int m, int n)
        {
            var d = new Hypergeometric(N, m, n);
            Assert.AreEqual<double>(N, d.PopulationSize);
            Assert.AreEqual<double>(m, d.M);
            Assert.AreEqual<double>(n, d.N);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(2, 3, 2)]
        [Row(10, 5, 20)]
        [Row(-2, 1, 1)]
        [Row(0, 1, 1)]
        public void HypergeometricCreateFailsWithBadParameters(int N, int m, int n)
        {
            var d = new Hypergeometric(N, m, n);
        }

        [Test]
        public void ValidateToString()
        {
            var d = new Hypergeometric(10, 1, 1);
            Assert.AreEqual<string>("Hypergeometric(N = 10, m = 1, n = 1)", d.ToString());
        }

        [Test]
        [Row(5)]
        [Row(10)]
        [Row(20)]
        public void CanSetSize(int N)
        {
            var d = new Hypergeometric(10, 1, 1);
            d.PopulationSize = N;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0)]
        [Row(0.0)]
        public void SetSizeFails(int N)
        {
            var d = new Hypergeometric(10, 1, 1);
            d.PopulationSize = N;
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2)]
        [Row(5)]
        public void CanSetm(int m)
        {
            var d = new Hypergeometric(10, 1, 1);
            d.M = m;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(11)]
        [Row(-1)]
        public void SetmFails(int m)
        {
            var d = new Hypergeometric(10, 1, 1);
            d.M = m;
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2)]
        [Row(5)]
        public void CanSetn(int n)
        {
            var d = new Hypergeometric(10, 1, 1);
            d.N = n;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(11)]
        [Row(-1)]
        public void SetnFails(int n)
        {
            var d = new Hypergeometric(10, 1, 1);
            d.N = n;
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateEntropy()
        {
            var d = new Hypergeometric(10, 1, 1);
            var e = d.Entropy;
        }

        [Test]
        [Row(0, 0, 0)]
        [Row(1, 1, 1)]
        [Row(2, 1, 1)]
        [Row(2, 2, 2)]
        [Row(10, 1, 1)]
        [Row(10, 5, 3)]
        public void ValidateSkewness(int N, int m, int n)
        {
            var d = new Hypergeometric(N, m, n);
            Assert.AreEqual<double>((Math.Sqrt(N - 1.0) * (N - 2 * n) * (N - 2 * m)) / (Math.Sqrt(n * m * (N - m) * (N - n)) * (N - 2.0)), d.Skewness);
        }

        [Test]
        [Row(0, 0, 0)]
        [Row(1, 1, 1)]
        [Row(2, 1, 1)]
        [Row(2, 2, 2)]
        [Row(10, 1, 1)]
        [Row(10, 5, 3)]
        public void ValidateMode(int N, int m, int n)
        {
            var d = new Hypergeometric(N, m, n);
            Assert.AreEqual<double>((n + 1) * (m + 1) / (N + 2), d.Mode);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ValidateMedian()
        {
            var d = new Hypergeometric(10, 1, 1);
            var m = d.Median;
        }

        [Test]
        [Row(0, 0, 0)]
        [Row(1, 1, 1)]
        [Row(2, 1, 1)]
        [Row(2, 2, 2)]
        [Row(10, 1, 1)]
        [Row(10, 5, 3)]
        public void ValidateMinimum(int N, int m, int n)
        {
            var d = new Hypergeometric(N, m, n);
            Assert.AreEqual(Math.Max(0, n + m - N), d.Minimum);
        }

        [Test]
        [Row(0, 0, 0)]
        [Row(1, 1, 1)]
        [Row(2, 1, 1)]
        [Row(2, 2, 2)]
        [Row(10, 1, 1)]
        [Row(10, 5, 3)]
        public void ValidateMaximum(int N, int m, int n)
        {
            var d = new Hypergeometric(N, m, n);
            Assert.AreEqual(Math.Min(m, n), d.Maximum);
        }

        [Test]
        [Row(0, 0, 0, 0)]
        [Row(1, 1, 1, 1)]
        [Row(2, 1, 1, 0)]
        [Row(2, 1, 1, 1)]
        [Row(2, 2, 2, 2)]
        [Row(10, 1, 1, 0)]
        [Row(10, 1, 1, 1)]
        [Row(10, 5, 3, 1)]
        [Row(10, 5, 3, 3)]
        public void ValidateProbability(int N, int m, int n, int x)
        {
            var d = new Hypergeometric(N, m, n);
            Assert.AreEqual<double>(SpecialFunctions.Binomial(m, x) * SpecialFunctions.Binomial(N - m, n - x) / SpecialFunctions.Binomial(N, n), d.Probability(x));
        }

        [Test]
        [Row(0, 0, 0, 0)]
        [Row(1, 1, 1, 1)]
        [Row(2, 1, 1, 0)]
        [Row(2, 1, 1, 1)]
        [Row(2, 2, 2, 2)]
        [Row(10, 1, 1, 0)]
        [Row(10, 1, 1, 1)]
        [Row(10, 5, 3, 1)]
        [Row(10, 5, 3, 3)]
        public void ValidateProbabilityLn(int N, int m, int n, int x)
        {
            var d = new Hypergeometric(N, m, n);
            Assert.AreEqual(Math.Log(d.Probability(x)), d.ProbabilityLn(x));
        }

        [Test]
        public void CanSample()
        {
            var d = new Hypergeometric(10, 1, 1);
            var s = d.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var d = new Hypergeometric(10, 1, 1);
            var ied = d.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0, 0, 0, 0.5, 1.0)]
        [Row(1, 1, 1, 1.1, 1.0)]
        [Row(2, 1, 1, 0.3, 0.5)]
        [Row(2, 1, 1, 1.2, 1.0)]
        [Row(2, 2, 2, 2.4, 1.0)]
        [Row(10, 1, 1, 0.3, 0.9)]
        [Row(10, 1, 1, 1.2, 1.0)]
        [Row(10, 5, 3, 1.1, 0.5)]
        [Row(10, 5, 3, 3.0, 0.916666666666667)]
        public void ValidateCumulativeDistribution(int N, int m, int n, double x, double cdf)
        {
            var d = new Hypergeometric(N, m, n);
            AssertHelpers.AlmostEqual(cdf, d.CumulativeDistribution(x), 14);
        }
    }
}