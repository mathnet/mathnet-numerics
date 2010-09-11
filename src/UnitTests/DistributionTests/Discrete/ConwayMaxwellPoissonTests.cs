// <copyright file="ConwayMaxwellPoissonTests.cs" company="Math.NET">
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
    public class ConwayMaxwellPoissonTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        [Row(0.1, 0.0)]
        [Row(1.0, 0.0)]
        [Row(2.5, 0.0)]
        [Row(10.0, 0.0)]
        [Row(Double.PositiveInfinity, 0.0)]
        [Row(0.1, 2.5)]
        [Row(1.0, 2.5)]
        [Row(2.5, 2.5)]
        [Row(10.0, 2.5)]
        [Row(Double.PositiveInfinity, 2.5)]
        [Row(0.1, Double.PositiveInfinity)]
        [Row(1.0, Double.PositiveInfinity)]
        [Row(2.5, Double.PositiveInfinity)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreateConwayMaxwellPoisson(double lambda, double nu)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            Assert.AreEqual(lambda, d.Lambda);
            Assert.AreEqual(nu, d.Nu);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-1.0, -2.0)]
        [Row(0.0, 0.0)]
        [Row(0.0, 1.0)]
        public void ConwayMaxwellPoissonCreateFailsWithBadParameters(double lambda, double nu)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
        }

        [Test]
        public void ValidateToString()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.AreEqual<string>("ConwayMaxwellPoisson(Lambda = 1, Nu = 2)", d.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(3.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetLambda(double lambda)
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            d.Lambda = lambda;
        }

        [Test]
        [Row(0.0)]
        [Row(3.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetNu(double nu)
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            d.Nu = nu;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(0.0)]
        [Row(-0.0)]
        [Row(-1.0)]
        [Row(Double.NegativeInfinity)]
        public void SetLambdaFails(double lambda)
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            d.Lambda = lambda;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-0.1)]
        [Row(-1.0)]
        [Row(-10.0)]
        [Row(Double.NegativeInfinity)]
        public void SetNuFails(double nu)
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            d.Nu = nu;
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ValidateEntropy()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            var e = d.Entropy;
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ValidateSkewness()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            var s = d.Skewness;
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ValidateMode()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            var m = d.Mode;
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ValidateMedian()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            var m = d.Median;
        }

        [Test]
        [Row(1.0, 1.0, 1.0)]
        [Row(2.0, 1.0, 2.0)]
        [Row(10.0, 1.0, 10.0)]
        [Row(20.0, 1.0, 20.0)]
        [Row(1.0, 2.0, 0.697774657964008)]
        [Row(2.0, 2.0, 1.12635723962342)]
        public void ValidateMean(int lambda, int nu, double mean)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqual(mean, d.Mean, 10);
        }

        [Test]
        public void ValidateMinimum()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            Assert.AreEqual<double>(0.0, d.Minimum);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ValidateMaximum()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            var max = d.Maximum;
        }

        [Test]
        [Row(1.0, 1.0, 1, 0.367879441171442)]
        [Row(1.0, 1.0, 2, 0.183939720585721)]
        [Row(2.0, 1.0, 1, 0.270670566473225)]
        [Row(2.0, 1.0, 2, 0.270670566473225)]
        [Row(2.0, 2.0, 1, 0.470328074204904)]
        [Row(2.0, 2.0, 3, 0.052258674911656)]
        public void ValidateProbability(double lambda, double nu, int x, double p)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqual(p, d.Probability(x), 13);
        }

        [Test]
        [Row(1.0, 1.0, 1, -1.0)]
        [Row(1.0, 1.0, 2, -1.69314718055995)]
        [Row(2.0, 1.0, 1, -1.30685281944005)]
        [Row(2.0, 1.0, 2, -1.30685281944005)]
        [Row(2.0, 2.0, 1, -0.754324797564617)]
        [Row(2.0, 2.0, 3, -2.95154937490084)]
        public void ValidateProbabilityLn(double lambda, double nu, int x, double pln)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqual(pln, d.ProbabilityLn(x), 13);
        }

        [Test]
        public void CanSample()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            var s = d.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var d = new ConwayMaxwellPoisson(1.0, 2.0);
            var ied = d.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(1.0, 1.0, 1, 0.735758882342885)]
        [Row(1.0, 1.0 ,2 , 0.919698602928606)]
        [Row(2.0, 1.0, 1, 0.406005849709838)]
        [Row(2.0, 1.0, 2, 0.676676416183064)]
        [Row(2.0, 2.0, 1, 0.705492111307356)]
        [Row(2.0, 2.0, 3, 0.992914823321464)]
        public void ValidateCumulativeDistribution(double lambda, double nu, double x, double cdf)
        {
            var d = new ConwayMaxwellPoisson(lambda, nu);
            AssertHelpers.AlmostEqual(cdf, d.CumulativeDistribution(x), 13);
        }
    }
}