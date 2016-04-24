// <copyright file="ContinuousUniformTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using System;
using System.Linq;
using MathNet.Numerics.Distributions;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using Random = System.Random;

    /// <summary>
    /// Continuous uniform tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class ContinuousUniformTests
    {
        /// <summary>
        /// Can create continuous uniform.
        /// </summary>
        [Test]
        public void CanCreateContinuousUniform()
        {
            var n = new ContinuousUniform();
            Assert.AreEqual(0.0, n.LowerBound);
            Assert.AreEqual(1.0, n.UpperBound);
        }

        /// <summary>
        /// Can create continuous uniform.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(-5.0, 11.0)]
        [TestCase(-5.0, 100.0)]
        [TestCase(Double.NegativeInfinity, Double.PositiveInfinity)]
        public void CanCreateContinuousUniform(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual(lower, n.LowerBound);
            Assert.AreEqual(upper, n.UpperBound);
        }

        /// <summary>
        /// Continuous uniform create fails with bad parameters.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(Double.NaN, 1.0)]
        [TestCase(1.0, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN)]
        [TestCase(1.0, 0.0)]
        public void ContinuousUniformCreateFailsWithBadParameters(double lower, double upper)
        {
            Assert.That(() => new ContinuousUniform(lower, upper), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new ContinuousUniform(1.0, 2.0);
            Assert.AreEqual("ContinuousUniform(Lower = 1, Upper = 2)", n.ToString());
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateEntropy(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual(Math.Log(upper - lower), n.Entropy);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateSkewness(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual(0.0, n.Skewness);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateMode(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual((lower + upper) / 2.0, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateMedian(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual((lower + upper) / 2.0, n.Median);
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateMinimum(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual(lower, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(-0.0, 2.0)]
        [TestCase(0.0, 2.0)]
        [TestCase(0.1, 4.0)]
        [TestCase(1.0, 10.0)]
        [TestCase(10.0, 11.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateMaximum(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual(upper, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(-5.0, 100.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateDensity(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                if (x >= lower && x <= upper)
                {
                    Assert.AreEqual(1.0/(upper - lower), n.Density(x));
                    Assert.AreEqual(1.0/(upper - lower), ContinuousUniform.PDF(lower, upper, x));
                }
                else
                {
                    Assert.AreEqual(0.0, n.Density(x));
                    Assert.AreEqual(0.0, ContinuousUniform.PDF(lower, upper, x));
                }
            }
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(-5.0, 100.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                if (x >= lower && x <= upper)
                {
                    Assert.AreEqual(-Math.Log(upper - lower), n.DensityLn(x));
                    Assert.AreEqual(-Math.Log(upper - lower), ContinuousUniform.PDFLn(lower, upper, x));
                }
                else
                {
                    Assert.AreEqual(double.NegativeInfinity, n.DensityLn(x));
                    Assert.AreEqual(double.NegativeInfinity, ContinuousUniform.PDFLn(lower, upper, x));
                }
            }
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            ContinuousUniform.Sample(new Random(0), 0.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = ContinuousUniform.Samples(new Random(0), 0.0, 1.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => ContinuousUniform.Sample(new Random(0), 0.0, -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => ContinuousUniform.Samples(new Random(0), 0.0, -1.0).First(), Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new ContinuousUniform();
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new ContinuousUniform();
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(-5.0, 100.0)]
        [TestCase(0.0, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                if (x <= lower)
                {
                    Assert.AreEqual(0.0, n.CumulativeDistribution(x));
                    Assert.AreEqual(0.0, ContinuousUniform.CDF(lower, upper, x));
                }
                else if (x >= upper)
                {
                    Assert.AreEqual(1.0, n.CumulativeDistribution(x));
                    Assert.AreEqual(1.0, ContinuousUniform.CDF(lower, upper, x));
                }
                else
                {
                    Assert.AreEqual((x - lower)/(upper - lower), n.CumulativeDistribution(x));
                    Assert.AreEqual((x - lower)/(upper - lower), ContinuousUniform.CDF(lower, upper, x));
                }
            }
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(0.0, 0.1)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.0, 10.0)]
        [TestCase(-5.0, 100.0)]
        public void ValidateInverseCumulativeDistribution(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            for (var i = 0; i < 11; i++)
            {
                var x = i - 5.0;
                if (x <= lower)
                {
                    Assert.AreEqual(lower, n.InverseCumulativeDistribution(0.0), 1e-12);
                    Assert.AreEqual(lower, ContinuousUniform.InvCDF(lower, upper, 0.0), 1e-12);
                }
                else if (x >= upper)
                {
                    Assert.AreEqual(upper, n.InverseCumulativeDistribution(1.0), 1e-12);
                    Assert.AreEqual(upper, ContinuousUniform.InvCDF(lower, upper, 1.0), 1e-12);
                }
                else
                {
                    Assert.AreEqual(x, n.InverseCumulativeDistribution((x - lower)/(upper - lower)), 1e-12);
                    Assert.AreEqual(x, ContinuousUniform.InvCDF(lower, upper, (x - lower)/(upper - lower)), 1e-12);
                }
            }
        }
    }
}
