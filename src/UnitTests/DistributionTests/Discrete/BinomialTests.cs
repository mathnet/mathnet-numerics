// <copyright file="BinomialTests.cs" company="Math.NET">
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
    public class BinomialTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        [Row(0.0, 4)]
        [Row(0.3, 3)]
        [Row(1.0, 2)]
        public void CanCreateBinomial(double p, int n)
        {
            var bernoulli = new Binomial(p,n);
            AssertEx.AreEqual<double>(p, bernoulli.P);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1)]
        [Row(-1.0, 1)]
        [Row(2.0, 1)]
        [Row(0.3, -2)]
        public void BinomialCreateFailsWithBadParameters(double p, int n)
        {
            var bernoulli = new Binomial(p,n);
        }

        [Test]
        public void ValidateToString()
        {
            var b = new Binomial(0.3, 2);
            AssertEx.AreEqual<string>("Binomial(Success Probability = 0.3, Number of Trials = 2)", b.ToString());
        }

        [Test]
        [Row(0.0, 4)]
        [Row(0.3, 3)]
        [Row(1.0, 2)]
        public void CanSetSuccessProbability(double p, int n)
        {
            var b = new Binomial(0.3, n);
            b.P = p;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1)]
        [Row(-1.0, 1)]
        [Row(2.0, 1)]
        public void SetProbabilityOfOneFails(double p, int n)
        {
            var b = new Binomial(0.3, n);
            b.P = p;
        }

        [Test]
        [Row(0.0, 4, 0.0)]
        [Row(0.3, 3, 1.1404671643037712668976423399228972051669206536461)]
        [Row(1.0, 2, 0.0)]
        public void ValidateEntropy(double p, int n, double e)
        {
            var b = new Binomial(p,n);
            AssertHelpers.AlmostEqual(e, b.Entropy, 14);
        }

        [Test]
        [Row(0.0, 4)]
        [Row(0.3, 3)]
        [Row(1.0, 2)]
        public void ValidateSkewness(double p, int n)
        {
            var b = new Binomial(p,n);
            AssertEx.AreEqual<double>((1.0 - 2.0 * p) / Math.Sqrt(n * p * (1.0 - p)), b.Skewness);
        }

        [Test]
        [Row(0.0, 4, 0)]
        [Row(0.3, 3, 1)]
        [Row(1.0, 2, 2)]
        public void ValidateMode(double p, int n, int m)
        {
            var b = new Binomial(p,n);
            AssertEx.AreEqual<int>(m, b.Mode);
        }

        [Test]
        public void ValidateMinimum()
        {
            var b = new Binomial(0.3, 10);
            AssertEx.AreEqual<int>(0, b.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var b = new Binomial(0.3, 10);
            AssertEx.AreEqual<int>(10, b.Maximum);
        }

        [Test]
        [Row(0.000000, 1, 0, 1.0)]
        [Row(0.000000, 1, 1, 0.0)]
        [Row(0.000000, 3, 0, 1.0)]
        [Row(0.000000, 3, 1, 0.0)]
        [Row(0.000000, 3, 3, 0.0)]
        [Row(0.000000, 10, 0, 1.0)]
        [Row(0.000000, 10, 1, 0.0)]
        [Row(0.000000, 10, 10, 0.0)]
        [Row(0.300000, 1, 0, 0.69999999999999995559107901499373838305473327636719)]
        [Row(0.300000, 1, 1, 0.2999999999999999888977697537484345957636833190918)]
        [Row(0.300000, 3, 0, 0.34299999999999993471888615204079956461021032657166)]
        [Row(0.300000, 3, 1, 0.44099999999999992772448109690231306411849135972008)]
        [Row(0.300000, 3, 3, 0.026999999999999997002397833512077451789759292859569)]
        [Row(0.300000, 10, 0, 0.02824752489999998207939855277004937778546385011091)]
        [Row(0.300000, 10, 1, 0.12106082099999992639752977030555903089040470780077)]
        [Row(0.300000, 10, 10, 0.0000059048999999999978147480206303047454017251032868501)]
        [Row(1.000000, 1, 0, 0.0)]
        [Row(1.000000, 1, 1, 1.0)]
        [Row(1.000000, 3, 0, 0.0)]
        [Row(1.000000, 3, 1, 0.0)]
        [Row(1.000000, 3, 3, 1.0)]
        [Row(1.000000, 10, 0, 0.0)]
        [Row(1.000000, 10, 1, 0.0)]
        [Row(1.000000, 10, 10, 1.0)]
        public void ValidateProbability(double p, int n, int x, double d)
        {
            var b = new Binomial(p,n);
            AssertHelpers.AlmostEqual(d, b.Probability(x), 14);
        }

        [Test]
        [Row(0.000000, 1, 0, 0.0)]
        [Row(0.000000, 1, 1, Double.NegativeInfinity)]
        [Row(0.000000, 3, 0, 0.0)]
        [Row(0.000000, 3, 1, Double.NegativeInfinity)]
        [Row(0.000000, 3, 3, Double.NegativeInfinity)]
        [Row(0.000000, 10, 0, 0.0)]
        [Row(0.000000, 10, 1, Double.NegativeInfinity)]
        [Row(0.000000, 10, 10, Double.NegativeInfinity)]
        [Row(0.300000, 1, 0, -0.3566749439387324423539544041072745145718090708995)]
        [Row(0.300000, 1, 1, -1.2039728043259360296301803719337238685164245381839)]
        [Row(0.300000, 3, 0, -1.0700248318161973270618632123218235437154272126985)]
        [Row(0.300000, 3, 1, -0.81871040353529122294284394322574719301255212216016)]
        [Row(0.300000, 3, 3, -3.6119184129778080888905411158011716055492736145517)]
        [Row(0.300000, 10, 0, -3.566749439387324423539544041072745145718090708995)]
        [Row(0.300000, 10, 1, -2.1114622067804823267977785542148302920616046876506)]
        [Row(0.300000, 10, 10, -12.039728043259360296301803719337238685164245381839)]
        [Row(1.000000, 1, 0, Double.NegativeInfinity)]
        [Row(1.000000, 1, 1, 0.0)]
        [Row(1.000000, 3, 0, Double.NegativeInfinity)]
        [Row(1.000000, 3, 1, Double.NegativeInfinity)]
        [Row(1.000000, 3, 3, 0.0)]
        [Row(1.000000, 10, 0, Double.NegativeInfinity)]
        [Row(1.000000, 10, 1, Double.NegativeInfinity)]
        [Row(1.000000, 10, 10, 0.0)]
        public void ValidateProbabilityLn(double p, int n, int x, double dln)
        {
            var b = new Binomial(p,n);
            AssertHelpers.AlmostEqual(dln, b.ProbabilityLn(x), 14);
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = Binomial.Sample(new Random(), 0.3, 5);
        }

        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Binomial.Samples(new Random(), 0.3, 5);
            var arr = ied.Take(5).ToArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = Binomial.Sample(new Random(), -1.0, 5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleSequenceStatic()
        {
            var ied = Binomial.Samples(new Random(), -1.0, 5).First();
        }

        [Test]
        public void CanSample()
        {
            var n = new Binomial(0.3, 5);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Binomial(0.3, 5);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }
    }
}