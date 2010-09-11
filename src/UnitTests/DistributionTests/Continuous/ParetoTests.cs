// <copyright file="ParetoTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using Distributions;

    [TestFixture]
    public class ParetoTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, 10.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void CanCreatePareto(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>(scale, n.Scale);
            Assert.AreEqual<double>(shape, n.Shape);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1.0)]
        [Row(1.0, Double.NaN)]
        [Row(Double.NaN, Double.NaN)]
        [Row(1.0, -1.0)]
        [Row(-1.0, 1.0)]
        [Row(-1.0, -1.0)]
        [Row(0.0, 0.0)]
        [Row(0.0, 1.0)]
        [Row(1.0, 0.0)]
        public void ParetoCreateFailsWithBadParameters(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Pareto(1.0, 2.0);
            Assert.AreEqual<string>("Pareto(Scale = 1, Shape = 2)", n.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            var n = new Pareto(1.0, 1.0);
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(-1.0)]
        public void SetScaleFailsWithNegativeScale(double scale)
        {
            var n = new Pareto(1.0, 1.0);
            n.Scale = scale;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetShape(double shape)
        {
            var n = new Pareto(1.0, 1.0);
            n.Shape = shape;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(-1.0)]
        public void SetShapeFailsWithNegativeShape(double shape)
        {
            var n = new Pareto(1.0, 1.0);
            n.Shape = shape;
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, 10.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMean(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            if (shape > 1)
            {
                Assert.AreEqual(shape * scale / (shape - 1.0), n.Mean);
            }
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, 10.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateVariance(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>(scale * scale * shape / ((shape - 1.0) * (shape - 1.0) * (scale - 2.0)), n.Variance);
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, 10.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateStdDev(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>((scale * Math.Sqrt(shape)) / ((shape - 1.0) * Math.Sqrt(shape - 2.0)), n.StdDev);
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, 10.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateEntropy(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>(Math.Log(shape / scale) - 1.0 / shape - 1.0, n.Entropy);
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, 10.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateSkewness(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>((2.0 * (shape + 1.0) / (shape - 3.0)) * Math.Sqrt((shape - 2.0) / shape), n.Skewness);
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, 10.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMode(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>(scale, n.Mode);
        }

        [Test]
        [Row(0.1, 0.1)]
        [Row(0.1, 1.0)]
        [Row(0.1, 10.0)]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 1.0)]
        [Row(Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity)]
        public void ValidateMedian(double scale, double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>(scale * Math.Pow(2.0, 1.0 / shape), n.Median);
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void ValidateMinimum(double scale)
        {
            var n = new Pareto(scale, 1.0);
            Assert.AreEqual<double>(scale, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new Pareto(1.0, 1.0);
            Assert.AreEqual<double>(Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0.1, 0.1, 0.1)]
        [Row(0.1, 1.0, 0.1)]
        [Row(0.1, 10.0, 0.1)]
        [Row(1.0, 0.1, 1.0)]
        [Row(1.0, 1.0, 1.0)]
        [Row(1.0, 10.0, 1.0)]
        [Row(10.0, 1.0, 10.0)]
        [Row(10.0, 10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, 1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(0.1, 0.1, 2.1)]
        [Row(0.1, 1.0, 2.1)]
        [Row(0.1, 10.0, 2.1)]
        [Row(1.0, 0.1, 2.0)]
        [Row(1.0, 1.0, 2.0)]
        [Row(1.0, 10.0, 2.0)]
        [Row(10.0, 1.0, 12.0)]
        [Row(10.0, 10.0, 12.0)]
        [Row(10.0, Double.PositiveInfinity, 12.0)]
        public void ValidateDensity(double scale, double shape, double x)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>(shape * Math.Pow(scale, shape) / Math.Pow(x, shape + 1.0), n.Density(x));
        }

        [Test]
        [Row(0.1, 0.1, 0.1)]
        [Row(0.1, 1.0, 0.1)]
        [Row(0.1, 10.0, 0.1)]
        [Row(1.0, 0.1, 1.0)]
        [Row(1.0, 1.0, 1.0)]
        [Row(1.0, 10.0, 1.0)]
        [Row(10.0, 1.0, 10.0)]
        [Row(10.0, 10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, 1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(0.1, 0.1, 2.1)]
        [Row(0.1, 1.0, 2.1)]
        [Row(0.1, 10.0, 2.1)]
        [Row(1.0, 0.1, 2.0)]
        [Row(1.0, 1.0, 2.0)]
        [Row(1.0, 10.0, 2.0)]
        [Row(10.0, 1.0, 12.0)]
        [Row(10.0, 10.0, 12.0)]
        [Row(10.0, Double.PositiveInfinity, 12.0)]
        public void ValidateDensityLn(double scale, double shape, double x)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>(Math.Log(n.Density(x)), n.DensityLn(x));
        }

        [Test]
        public void CanSample()
        {
            var n = new Pareto(1.0, 1.0);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Pareto(1.0, 1.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.1, 0.1, 0.1)]
        [Row(0.1, 1.0, 0.1)]
        [Row(0.1, 10.0, 0.1)]
        [Row(1.0, 0.1, 1.0)]
        [Row(1.0, 1.0, 1.0)]
        [Row(1.0, 10.0, 1.0)]
        [Row(10.0, 1.0, 10.0)]
        [Row(10.0, 10.0, 10.0)]
        [Row(10.0, Double.PositiveInfinity, 10.0)]
        [Row(Double.PositiveInfinity, 1.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, 10.0, Double.PositiveInfinity)]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)]
        [Row(0.1, 0.1, 2.1)]
        [Row(0.1, 1.0, 2.1)]
        [Row(0.1, 10.0, 2.1)]
        [Row(1.0, 0.1, 2.0)]
        [Row(1.0, 1.0, 2.0)]
        [Row(1.0, 10.0, 2.0)]
        [Row(10.0, 1.0, 12.0)]
        [Row(10.0, 10.0, 12.0)]
        [Row(10.0, Double.PositiveInfinity, 12.0)]
        public void ValidateCumulativeDistribution(double scale, double shape, double x)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual<double>(1.0 - Math.Pow(scale / x, shape), n.CumulativeDistribution(x));
        }
    }
}
