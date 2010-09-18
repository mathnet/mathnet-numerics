// <copyright file="RejectionSamplerTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{
    using System;
    using Numerics.Random;
    using Distributions;
    using Statistics.Mcmc;
    using MbUnit.Framework;

    [TestFixture]
    public class RejectionSamplerTests
    {
        [Test]
        public void RejectTest()
        {
            var uniform = new ContinuousUniform(0.0, 1.0);
            uniform.RandomSource = new MersenneTwister();

            var rs = new RejectionSampler<double>(x => System.Math.Pow(x, 1.7) * System.Math.Pow(1.0 - x, 5.3),
                                                  x => 0.021,
                                                  uniform.Sample);
            Assert.IsNotNull(rs.RandomSource);

            rs.RandomSource = uniform.RandomSource;
            Assert.IsNotNull(rs.RandomSource);
        }

        [Test]
        public void SampleTest()
        {
            var uniform = new ContinuousUniform(0.0, 1.0);
            uniform.RandomSource = new MersenneTwister();

            var rs = new RejectionSampler<double>(x => System.Math.Pow(x, 1.7) * System.Math.Pow(1.0 - x, 5.3),
                                                  x => 0.021,
                                                  uniform.Sample);
            rs.RandomSource = uniform.RandomSource;

            double sample = rs.Sample();
        }

        [Test]
        public void SampleArrayTest()
        {
            var uniform = new ContinuousUniform(0.0, 1.0);
            uniform.RandomSource = new MersenneTwister();

            var rs = new RejectionSampler<double>(x => System.Math.Pow(x, 1.7) * System.Math.Pow(1.0 - x, 5.3),
                                                  x => 0.021,
                                                  uniform.Sample);
            rs.RandomSource = uniform.RandomSource;

            double[] sample = rs.Sample(5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void NoUpperBound()
        {
            var uniform = new ContinuousUniform(0.0, 1.0);
            uniform.RandomSource = new MersenneTwister();

            var rs = new RejectionSampler<double>(x => System.Math.Pow(x, 1.7) * System.Math.Pow(1.0 - x, 5.3),
                                                  x => System.Double.NegativeInfinity,
                                                  uniform.Sample);
            double s = rs.Sample();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullRandomNumberGenerator()
        {
            var uniform = new ContinuousUniform(0.0, 1.0);
            uniform.RandomSource = new MersenneTwister();

            var rs = new RejectionSampler<double>(x => System.Math.Pow(x, 1.7) * System.Math.Pow(1.0 - x, 5.3),
                                                  x => System.Double.NegativeInfinity,
                                                  uniform.Sample);
            rs.RandomSource = null;
        }
    }
}
