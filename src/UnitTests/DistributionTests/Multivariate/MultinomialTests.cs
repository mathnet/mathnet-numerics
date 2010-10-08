// <copyright file="MultinomialTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Multivariate
{
	using System;
	using System.Linq;
	using MbUnit.Framework;
	using Distributions;
	using Statistics;

    [TestFixture]
    public class MultinomialTests
    {
        double[] badP;
        double[] badP2;
        double[] smallP;
        double[] largeP;

        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
            badP = new double[] { -1.0, 1.0 };
            badP2 = new double[] { 0.0, 0.0 };
            smallP = new double[] {1.0, 1.0, 1.0};
            largeP = new double[] {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0};
        }

        [Test]
        public void CanCreateMultinomial()
        {
            var m = new Multinomial(largeP, 4);
            Assert.AreEqual<double[]>(largeP, m.P);
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateMultinomialFromHistogram()
        {
            double[] smallDataset = { 0.5, 1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5, 8.5, 9.5 };
            var hist = new Histogram(smallDataset, 10, 0.0, 10.0);
            var m = new Multinomial(hist, 7);

            foreach (var t in m.P)
            {
                Assert.AreEqual(1.0, t);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultinomialCreateFailsWithNullHistogram()
        {
            Histogram h = null;
            var m = new Categorical(h);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MultinomialCreateFailsWithNegativeRatios()
        {
            var m = new Multinomial(badP, 4);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MultinomialCreateFailsWithAllZeroRatios()
        {
            var m = new Multinomial(badP2, 4);
        }

        [Test]
        public void ValidateToString()
        {
            var b = new Multinomial(smallP, 4);
            Assert.AreEqual<string>("Multinomial(Dimension = 3, Number of Trails = 4)", b.ToString());
        }

        [Test]
        [Row(new[] { 0.3, 0.7 }, 5, new[] { 0.390360029179413, -0.390360029179413 })]
        [Row(new[] { 0.1, 0.3, 0.6 }, 10, new[] { 0.843274042711568, 0.276026223736942, -0.129099444873581 })]
        [Row(new[] { 0.15, 0.35, 0.3, 0.2 }, 20, new[] { 0.438357003759605, 0.140642169281549, 0.195180014589707, 0.335410196624968 })]
        public void ValidateSkewness(double[] p, int n, double[] res)
        {
            var b = new Multinomial(p, n);
            for (var i = 0; i < b.P.Length; i++)
            {
                AssertHelpers.AlmostEqual(res[i], b.Skewness[i], 12);
            }
        }

        [Test]
        [Row(new[] { 0.3, 0.7 }, 5, new[] { 1.05, 1.05 })]
        [Row(new[] { 0.1, 0.3, 0.6 }, 10, new[] { 0.9, 2.1, 2.4 })]
        [Row(new[] { 0.15, 0.35, 0.3, 0.2 }, 20, new[] { 2.55, 4.55, 4.2, 3.2 })]
        public void ValidateVariance(double[] p, int n, double[] res)
        {
            var b = new Multinomial(p, n);
            for (var i = 0; i < b.P.Length; i++)
            {
                AssertHelpers.AlmostEqual(res[i], b.Variance[i], 12);
            }
        }

        [Test]
        [Row(new[] { 0.3, 0.7 }, 5, new[] { 1.5, 3.5 })]
        [Row(new[] { 0.1, 0.3, 0.6 }, 10, new[] { 1.0, 3.0, 6.0 })]
        [Row(new[] { 0.15, 0.35, 0.3, 0.2 }, 20, new[] {3.0, 7.0, 6.0, 4.0 })]
        public void ValidateMean(double[] p, int n, double[] res)
        {
            var b = new Multinomial(p, n);
            for (var i = 0; i < b.P.Length; i++)
            {
                AssertHelpers.AlmostEqual(res[i], b.Mean[i], 12);
            }
        }

        [Test]
        [Row(new[] { 0.3, 0.7 }, new[] { 1, 9 }, 0.121060821)]
        [Row(new[] { 0.1, 0.3, 0.6 }, new[] { 1, 3, 6 }, 0.105815808)]
        [Row(new[] { 0.15, 0.35, 0.3, 0.2 }, new[] { 1, 1, 1, 7 }, 0.000145152)]
        public void ValidateProbability(double[] p, int[] x, double res)
        {
            var b = new Multinomial(p, x.Sum());
            AssertHelpers.AlmostEqual(b.Probability(x), res, 12);
        }

        [Test]
        [Row(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [Row(new[] { 1, 1, 1, 2, 2, 2, 3, 3, 3 })]
        [Row(new[] { 5, 6, 7, 8, 7, 6, 5, 4, 3 })]
        public void ValidateProbabilityLn(int[] x)
        {
            var b = new Multinomial(largeP, x.Sum());
            AssertHelpers.AlmostEqual(b.ProbabilityLn(x), Math.Log(b.Probability(x)), 12);
        }

        [Test]
        public void CanSetProbability()
        {
            var b = new Multinomial(largeP, 4);
            b.P = smallP;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetProbabilityFails()
        {
            var b = new Multinomial(largeP, 4);
            b.P = badP;
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = Multinomial.Sample(new Random(), largeP, 4);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = Multinomial.Sample(new Random(), badP, 4);
        }

        [Test]
        public void CanSample()
        {
            var n = new Multinomial(largeP, 4);
            var d = n.Sample();
        }
    }
}