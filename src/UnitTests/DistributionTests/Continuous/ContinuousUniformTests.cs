// <copyright file="ContinuousUniformTests.cs" company="Math.NET">
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
    /// Continuous uniform tests.
    /// </summary>
    [TestFixture]
    public class ContinuousUniformTests
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
        /// Can create continuous uniform.
        /// </summary>
        [Test]
        public void CanCreateContinuousUniform()
        {
            var n = new ContinuousUniform();
            Assert.AreEqual(0.0, n.Lower);
            Assert.AreEqual(1.0, n.Upper);
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
            Assert.AreEqual(lower, n.Lower);
            Assert.AreEqual(upper, n.Upper);
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
            Assert.Throws<ArgumentOutOfRangeException>(() => new ContinuousUniform(lower, upper));
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
        /// Can set lower bound.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        [TestCase(-10.0)]
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        public void CanSetLower(double lower)
        {
            new ContinuousUniform
            {
                Lower = lower
            };
        }

        /// <summary>
        /// Set bad lower bound fails.
        /// </summary>
        [Test]
        public void SetBadLowerFails()
        {
            var n = new ContinuousUniform();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Lower = 3.0);
        }

        /// <summary>
        /// Can set upper bound.
        /// </summary>
        /// <param name="upper">Upper bound.</param>
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(12.0)]
        public void CanSetUpper(double upper)
        {
            new ContinuousUniform
            {
                Upper = upper
            };
        }

        /// <summary>
        /// Set bad upper fails.
        /// </summary>
        [Test]
        public void SetBadUpperFails()
        {
            var n = new ContinuousUniform();
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Upper = -1.0);
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
                    Assert.AreEqual(1.0 / (upper - lower), n.Density(x));
                }
                else
                {
                    Assert.AreEqual(0.0, n.Density(x));
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
                }
                else
                {
                    Assert.AreEqual(double.NegativeInfinity, n.DensityLn(x));
                }
            }
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            ContinuousUniform.Sample(new Random(), 0.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = ContinuousUniform.Samples(new Random(), 0.0, 1.0);
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Fail sample static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ContinuousUniform.Sample(new Random(), 0.0, -1.0));
        }

        /// <summary>
        /// Fail sample sequence static with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ContinuousUniform.Samples(new Random(), 0.0, -1.0).First());
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
            ied.Take(5).ToArray();
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
                }
                else if (x >= upper)
                {
                    Assert.AreEqual(1.0, n.CumulativeDistribution(x));
                }
                else
                {
                    Assert.AreEqual((x - lower) / (upper - lower), n.CumulativeDistribution(x));
                }
            }
        }
    }
}
