// <copyright file="DiscreteUniformTests.cs" company="Math.NET">
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
    public class DiscreteUniformTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        [Row(-10, 10)]
        [Row(0, 4)]
        [Row(10, 20)]
        [Row(20, 20)]
        public void CanCreateDiscreteUniform(int l, int u)
        {
            var du = new DiscreteUniform(l, u);
            AssertEx.AreEqual(l, du.LowerBound);
            AssertEx.AreEqual(u, du.UpperBound);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1, -2)]
        [Row(6, 5)]
        public void DiscreteUniformCreateFailsWithBadParameters(int l, int u)
        {
            var du = new DiscreteUniform(l, u);
        }

        [Test]
        public void ValidateToString()
        {
            var b = new DiscreteUniform(0, 10);
            AssertEx.AreEqual<string>("DiscreteUniform(Lower = 0, Upper = 10)", b.ToString());
        }

        [Test]
        [Row(0)]
        [Row(3)]
        [Row(10)]
        public void CanSetLowerBound(int p)
        {
            var b = new DiscreteUniform(0, 10);
            b.LowerBound = p;
        }

        [Test]
        [Row(0)]
        [Row(3)]
        [Row(10)]
        public void CanSetUpperBound(int p)
        {
            var b = new DiscreteUniform(0, 10);
            b.UpperBound = p;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(11)]
        [Row(20.0)]
        public void SetLowerBoundFails(int p)
        {
            var b = new DiscreteUniform(0, 10);
            b.LowerBound = p;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-11)]
        [Row(-20)]
        public void SetUpperBoundFails(int p)
        {
            var b = new DiscreteUniform(0, 10);
            b.UpperBound = p;
        }

        [Test]
        [Row(-10, 10, 3.0445224377234229965005979803657054342845752874046093)]
        [Row(0, 4, 1.6094379124341003746007593332261876395256013542685181)]
        [Row(10, 20, 2.3978952727983705440619435779651292998217068539374197)]
        [Row(20, 20, 0.0)]
        public void ValidateEntropy(int l, int u, double e)
        {
            var du = new DiscreteUniform(l, u);
            AssertHelpers.AlmostEqual(e, du.Entropy, 14);
        }

        [Test]
        [Row(-10, 10)]
        [Row(0, 4)]
        [Row(10, 20)]
        [Row(20, 20)]
        public void ValidateSkewness(int l, int u)
        {
            var du = new DiscreteUniform(l, u);
            AssertEx.AreEqual<double>(0.0, du.Skewness);
        }

        [Test]
        [Row(-10, 10, 0)]
        [Row(0, 4, 2)]
        [Row(10, 20, 15)]
        [Row(20, 20, 20)]
        public void ValidateMode(int l, int u, int m)
        {
            var du = new DiscreteUniform(l, u);
            AssertEx.AreEqual<double>(m, du.Mode);
        }

        [Test]
        [Row(-10, 10, 0)]
        [Row(0, 4, 2)]
        [Row(10, 20, 15)]
        [Row(20, 20, 20)]
        public void ValidateMedian(int l, int u, int m)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(m, du.Median);
        }

        [Test]
        [Row(-10, 10, 0.0)]
        [Row(0, 4, 2.0)]
        [Row(10, 20, 15.0)]
        [Row(20, 20, 20.0)]
        public void ValidateMean(int l, int u, double m)
        {
            var du = new DiscreteUniform(l, u);
            Assert.AreEqual(m, du.Mean);
        }

        [Test]
        public void ValidateMinimum()
        {
            var b = new DiscreteUniform(-10, 10);
            AssertEx.AreEqual<double>(-10, b.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var b = new DiscreteUniform(-10, 10);
            AssertEx.AreEqual<double>(10, b.Maximum);
        }

        [Test]
        [Row(-10, 10, -5, 1/21.0)]
        [Row(-10, 10, 1, 1 / 21.0)]
        [Row(-10, 10, 10, 1 / 21.0)]
        [Row(-10, -10, 0, 0.0)]
        [Row(-10, -10, -10, 1.0)]
        public void ValidateProbability(int l, int u, int x, double p)
        {
            var b = new DiscreteUniform(l, u);
            AssertEx.AreEqual(p, b.Probability(x));
        }

        [Test]
        [Row(-10, 10, -5, -3.0445224377234229965005979803657054342845752874046093)]
        [Row(-10, 10, 1, -3.0445224377234229965005979803657054342845752874046093)]
        [Row(-10, 10, 10, -3.0445224377234229965005979803657054342845752874046093)]
        [Row(-10, -10, 0, Double.NegativeInfinity)]
        [Row(-10, -10, -10, 0.0)]
        public void ValidateProbabilityLn(int l, int u, int x, double dln)
        {
            var b = new DiscreteUniform(l, u);
            AssertEx.AreEqual(dln, b.ProbabilityLn(x));
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = DiscreteUniform.Sample(new Random(), 0, 10);
        }

        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = DiscreteUniform.Samples(new Random(), 0, 10);
            var arr = ied.Take(5).ToArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = DiscreteUniform.Sample(new Random(), 20, 10);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleSequenceStatic()
        {
            var ied = DiscreteUniform.Samples(new Random(), 20, 10).First();
        }

        [Test]
        public void CanSample()
        {
            var n = new DiscreteUniform(0, 10);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new DiscreteUniform(0, 10);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(-10, 10, -5, 6.0 / 21.0)]
        [Row(-10, 10, 1, 12.0 / 21.0)]
        [Row(-10, 10, 10, 1.0)]
        [Row(-10, -10, 0, 1.0)]
        [Row(-10, -10, -10, 1.0)]
        [Row(-10, -10, -11, 0.0)]
        public void ValidateCumulativeDistribution(int l, int u, double x, double cdf)
        {
            var b = new DiscreteUniform(l, u);
            AssertEx.AreEqual(cdf, b.CumulativeDistribution(x));
        }
    }
}