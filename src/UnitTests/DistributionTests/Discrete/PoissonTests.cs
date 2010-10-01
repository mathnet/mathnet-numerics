// <copyright file="PoissonTests.cs" company="Math.NET">
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
    public class PoissonTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        [Row(1.5)]
        [Row(5.4)]
        [Row(10.8)]
        public void CanCreatePoisson(double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual(lambda, d.Lambda);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.5)]
        [Row(0.0)]
        public void PoissonCreateFailsWithBadParameters(double lambda)
        {
            var d = new Poisson(lambda);
        }

        [Test]
        public void ValidateToString()
        {
            var d = new Poisson(0.3);
            Assert.AreEqual(String.Format("Poisson(λ = {0})", 0.3), d.ToString());
        }

        [Test]
        [Row(1.5)]
        [Row(5.4)]
        [Row(10.8)]
        public void CanSetProbabilityOfOne(double lambda)
        {
            var d = new Poisson(0.3)
                    {
                        Lambda = lambda
                    };
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN)]
        [Row(-1.5)]
        [Row(0.0)]
        public void SetProbabilityOfOneFails(double lambda)
        {
            var d = new Poisson(0.3)
                    {
                        Lambda = lambda
                    };
        }

        [Test]
        [Row(1.5)]
        [Row(5.4)]
        [Row(10.8)]
        public void ValidateEntropy(double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual((0.5 * Math.Log(2 * Math.PI * Math.E * lambda)) - (1.0 / (12.0 * lambda)) - 1.0 / (24.0 * lambda * lambda) - (19.0 / (360.0 * lambda * lambda * lambda)), d.Entropy);
        }

        [Test]
        [Row(1.5)]
        [Row(5.4)]
        [Row(10.8)]
        public void ValidateSkewness(double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual(1.0 / Math.Sqrt(lambda), d.Skewness);
        }

        [Test]
        [Row(1.5)]
        [Row(5.4)]
        [Row(10.8)]
        public void ValidateMode(double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual((int)Math.Floor(lambda), d.Mode);
        }

        [Test]
        [Row(1.5)]
        [Row(5.4)]
        [Row(10.8)]
        public void ValidateMedian(double lambda)
        {
            var d = new Poisson(lambda);
            Assert.AreEqual((int)Math.Floor(lambda + (1.0 / 3.0) - (0.02 / lambda)), d.Median);
        }

        [Test]
        public void ValidateMinimum()
        {
            var d = new Poisson(0.3);
            Assert.AreEqual(0, d.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var d = new Poisson(0.3);
            Assert.AreEqual(int.MaxValue, d.Maximum);
        }

        [Test]
        [Row(1.5, 1,   0.334695240222645000000000000000)]
        [Row(1.5, 10,  0.000003545747740570180000000000)]
        [Row(1.5, 20,  0.000000000000000304971208961018)]
        [Row(5.4, 1,   0.024389537090108400000000000000)]
        [Row(5.4, 10,  0.026241240591792300000000000000)]
        [Row(5.4, 20,  0.000000825202200316548000000000)]
        [Row(10.8, 1,  0.000220314636840657000000000000)]
        [Row(10.8, 10, 0.121365183659420000000000000000)]
        [Row(10.8, 20, 0.003908139778574110000000000000)]
        public void ValidateProbability(double lambda, int x, double result)
        {
            var d = new Poisson(lambda);
            Assert.AreApproximatelyEqual(d.Probability(x), result, 1e-12);
        }

        [Test]
        [Row(1.5, 1,   0.334695240222645000000000000000)]
        [Row(1.5, 10,  0.000003545747740570180000000000)]
        [Row(1.5, 20,  0.000000000000000304971208961018)]
        [Row(5.4, 1,   0.024389537090108400000000000000)]
        [Row(5.4, 10,  0.026241240591792300000000000000)]
        [Row(5.4, 20,  0.000000825202200316548000000000)]
        [Row(10.8, 1,  0.000220314636840657000000000000)]
        [Row(10.8, 10, 0.121365183659420000000000000000)]
        [Row(10.8, 20, 0.003908139778574110000000000000)]
        public void ValidateProbabilityLn(double lambda, int x, double result)
        {
            var d = new Poisson(lambda);
            Assert.AreApproximatelyEqual(d.ProbabilityLn(x), Math.Log(result), 1e-12);
        }

        [Test]
        public void CanSample()
        {
            var d = new Poisson(0.3);
            var s = d.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var d = new Poisson(0.3);
            var ied = d.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(1.5, 1,   0.5578254003710750000000)]
        [Row(1.5, 10,  0.9999994482467640000000)]
        [Row(1.5, 20,  1.0000000000000000000000)]
        [Row(5.4, 1,   0.0289061180327211000000)]
        [Row(5.4, 10,  0.9774863006897650000000)]
        [Row(5.4, 20,  0.9999997199928290000000)]
        [Row(10.8, 1,  0.0002407141402518290000)]
        [Row(10.8, 10, 0.4839692359955690000000)]
        [Row(10.8, 20, 0.9961800769608090000000)]
        [Row(20.1, 1,  0.0000000393516882521484)]
        [Row(20.1, 10, 0.0102444128791257000000)]
        [Row(20.1, 20, 0.5502097908860160000000)]
        public void ValidateCumulativeDistribution(double lambda, int x, double result)
        {
            var d = new Poisson(lambda);
            Assert.AreApproximatelyEqual(d.CumulativeDistribution(x), result, 1e-12);
        }
    }
}