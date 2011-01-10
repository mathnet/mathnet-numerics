// <copyright file="ParetoTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
    using Distributions;
    using NUnit.Framework;

    /// <summary>
    /// Pareto distribution tests.
    /// </summary>
    [TestFixture]
    public class ParetoTests
    {
        /// <summary>
        /// Set-up parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create Pareto distribution.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Combinatorial]
        public void CanCreatePareto([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(scale, n.Scale);
            Assert.AreEqual(shape, n.Shape);
        }

        /// <summary>
        /// Pareto create fails with bad parameters.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Sequential]
        public void ParetoCreateFailsWithBadParameters(
            [Values(Double.NaN, 1.0, Double.NaN, 1.0, -1.0, -1.0, 0.0, 0.0, 1.0)] double scale, 
            [Values(1.0, Double.NaN, Double.NaN, -1.0, 1.0, -1.0, 0.0, 1.0, 0.0)] double shape)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { var n = new Pareto(scale, shape); });
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Pareto(1.0, 2.0);
            Assert.AreEqual("Pareto(Scale = 1, Shape = 2)", n.ToString());
        }

        /// <summary>
        /// Can set scale.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [Test]
        public void CanSetScale([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale)
        {
            new Pareto(1.0, 1.0)
            {
                Scale = scale
            };
        }

        /// <summary>
        /// Set scale fails with negative scale.
        /// </summary>
        [Test]
        public void SetScaleFailsWithNegativeScale()
        {
            var n = new Pareto(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Scale = -1.0);
        }

        /// <summary>
        /// Can set shape.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        [Test]
        public void CanSetShape([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            new Pareto(1.0, 1.0)
            {
                Shape = shape
            };
        }

        /// <summary>
        /// Set shape fails with negative shape.
        /// </summary>
        [Test]
        public void SetShapeFailsWithNegativeShape()
        {
            var n = new Pareto(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Shape = -1.0);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Combinatorial]
        public void ValidateMean([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            var n = new Pareto(scale, shape);
            if (shape > 1)
            {
                Assert.AreEqual(shape * scale / (shape - 1.0), n.Mean);
            }
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Combinatorial]
        public void ValidateVariance([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            var n = new Pareto(scale, shape);
            if (shape <= 2.0)
            {
                Assert.AreEqual(double.PositiveInfinity, n.Variance);
            }
            else
            {
                Assert.AreEqual(scale * scale * shape / ((shape - 1.0) * (shape - 1.0) * (shape - 2.0)), n.Variance);
            }
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Combinatorial]
        public void ValidateStdDev([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual((scale * Math.Sqrt(shape)) / ((shape - 1.0) * Math.Sqrt(shape - 2.0)), n.StdDev);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Combinatorial]
        public void ValidateEntropy([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(Math.Log(shape / scale) - (1.0 / shape) - 1.0, n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Combinatorial]
        public void ValidateSkewness([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual((2.0 * (shape + 1.0) / (shape - 3.0)) * Math.Sqrt((shape - 2.0) / shape), n.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Combinatorial]
        public void ValidateMode([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(scale, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        [Test, Combinatorial]
        public void ValidateMedian(
            [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, 
            [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(scale * Math.Pow(2.0, 1.0 / shape), n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [Test]
        public void ValidateMinimum([Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale)
        {
            var n = new Pareto(scale, 1.0);
            Assert.AreEqual(scale, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Pareto(1.0, 1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        /// <param name="x">Input X value.</param>
        [Test, Combinatorial]
        public void ValidateDensity(
            [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, 
            [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape, 
            [Values(0.1, 1.0, 2.0, 2.1, 10.0, 12.0, Double.PositiveInfinity)] double x)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(shape * Math.Pow(scale, shape) / Math.Pow(x, shape + 1.0), n.Density(x));
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        /// <param name="x">Input X value.</param>
        [Test, Combinatorial]
        public void ValidateDensityLn(
            [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, 
            [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape, 
            [Values(0.1, 1.0, 2.0, 2.1, 10.0, 12.0, Double.PositiveInfinity)] double x)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(Math.Log(n.Density(x)), n.DensityLn(x));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Pareto(1.0, 1.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Pareto(1.0, 1.0);
            var ied = n.Samples();
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        /// <param name="shape">Shape value.</param>
        /// <param name="x">Input X value.</param>
        [Test, Combinatorial]
        public void ValidateCumulativeDistribution(
            [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale, 
            [Values(0.1, 1.0, 10.0, Double.PositiveInfinity)] double shape, 
            [Values(0.1, 1.0, 2.0, 2.1, 10.0, 12.0, Double.PositiveInfinity)] double x)
        {
            var n = new Pareto(scale, shape);
            Assert.AreEqual(1.0 - Math.Pow(scale / x, shape), n.CumulativeDistribution(x));
        }
    }
}
