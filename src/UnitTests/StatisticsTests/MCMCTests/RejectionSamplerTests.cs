// <copyright file="RejectionSamplerTests.cs" company="Math.NET">
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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using MathNet.Numerics.Statistics.Mcmc;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{
    /// <summary>
    /// Rejection sampler tests.
    /// </summary>
    [TestFixture, Category("Statistics")]
    public class RejectionSamplerTests
    {
        /// <summary>
        /// Rejection test.
        /// </summary>
        [Test]
        public void RejectTest()
        {
            var uniform = new ContinuousUniform(0.0, 1.0, new SystemRandomSource(1));
            var rs = new RejectionSampler<double>(x => Math.Pow(x, 1.7)*Math.Pow(1.0 - x, 5.3), x => 0.021, uniform.Sample);
            Assert.IsNotNull(rs.RandomSource);

            rs.RandomSource = uniform.RandomSource;
            Assert.IsNotNull(rs.RandomSource);
        }

        /// <summary>
        /// Sample test.
        /// </summary>
        [Test]
        public void SampleTest()
        {
            var uniform = new ContinuousUniform(0.0, 1.0, new SystemRandomSource(1));

            var rs = new RejectionSampler<double>(x => Math.Pow(x, 1.7)*Math.Pow(1.0 - x, 5.3), x => 0.021, uniform.Sample)
                {
                    RandomSource = uniform.RandomSource
                };

            rs.Sample();
        }

        /// <summary>
        /// Sample array test.
        /// </summary>
        [Test]
        public void SampleArrayTest()
        {
            var uniform = new ContinuousUniform(0.0, 1.0, new SystemRandomSource(1));

            var rs = new RejectionSampler<double>(x => Math.Pow(x, 1.7)*Math.Pow(1.0 - x, 5.3), x => 0.021, uniform.Sample)
                {
                    RandomSource = uniform.RandomSource
                };

            rs.Sample(5);
        }

        /// <summary>
        /// No upper bound.
        /// </summary>
        [Test]
        public void NoUpperBound()
        {
            var uniform = new ContinuousUniform(0.0, 1.0, new SystemRandomSource(1));
            var rs = new RejectionSampler<double>(x => Math.Pow(x, 1.7)*Math.Pow(1.0 - x, 5.3), x => Double.NegativeInfinity, uniform.Sample);
            Assert.That(() => rs.Sample(), Throws.ArgumentException);
        }

        /// <summary>
        /// Set <c>null</c> RNG throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void NullRandomNumberGenerator()
        {
            var uniform = new ContinuousUniform(0.0, 1.0, new SystemRandomSource(1));
            var rs = new RejectionSampler<double>(x => Math.Pow(x, 1.7)*Math.Pow(1.0 - x, 5.3), x => Double.NegativeInfinity, uniform.Sample);
            Assert.That(() => rs.RandomSource = null, Throws.TypeOf<ArgumentNullException>());
        }
    }
}
