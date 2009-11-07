// <copyright file="UnivariateSliceSamplerTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{
    using System;
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Statistics.Mcmc;
    using MbUnit.Framework;

    [TestFixture]
    public class UnivariateSliceSamplerTests
    {
        [Test]
        public void ConstructorTest()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5 * x * x, 5, 1.0);
        }

        [Test]
        public void SampleTest()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5 * x * x, 5, 1.0);
            double sample = ss.Sample();
        }

        [Test]
        public void SampleArrayTest()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5 * x * x, 5, 1.0);
            double[] sample = ss.Sample(5);
        }

        [Test]
        public void RNGTest()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5 * x * x, 5, 1.0);

            Assert.IsNotNull(ss.RandomSource);
            ss.RandomSource = new System.Random();
            Assert.IsNotNull(ss.RandomSource);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidScale()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5 * x * x, 5, -1.0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidBurn()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5 * x * x, -5, 1.0);
        }
    }
}
