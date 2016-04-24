// <copyright file="UnivariateSliceSamplerTests.cs" company="Math.NET">
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
using NUnit.Framework;
using MathNet.Numerics.Statistics.Mcmc;

namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{
    /// <summary>
    /// Univariate slice sampler tests.
    /// </summary>
    [TestFixture, Category("Statistics")]
    public class UnivariateSliceSamplerTests
    {
        /// <summary>
        /// Constructor tests.
        /// </summary>
        [Test]
        public void ConstructorTest()
        {
            GC.KeepAlive(new UnivariateSliceSampler(0.1, x => -0.5*x*x, 5, 1.0));
        }

        /// <summary>
        /// Sample test.
        /// </summary>
        [Test]
        public void SampleTest()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5*x*x, 5, 1.0);
            ss.Sample();
        }

        /// <summary>
        /// Sample array tests.
        /// </summary>
        [Test]
        public void SampleArrayTest()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5*x*x, 5, 1.0);
            ss.Sample(5);
        }

        /// <summary>
        /// Random number generator tests.
        /// </summary>
        [Test]
        public void RandomNumberGeneratorTest()
        {
            var ss = new UnivariateSliceSampler(0.1, x => -0.5*x*x, 5, 1.0);

            Assert.IsNotNull(ss.RandomSource);
            ss.RandomSource = new System.Random(0);
            Assert.IsNotNull(ss.RandomSource);
        }

        /// <summary>
        /// Constructor with invalid scale throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void InvalidScale()
        {
            Assert.That(() => new UnivariateSliceSampler(0.1, x => -0.5*x*x, 5, -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Constructor with invalid burn throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void InvalidBurn()
        {
            Assert.That(() => new UnivariateSliceSampler(0.1, x => -0.5*x*x, -5, 1.0), Throws.ArgumentException);
        }
    }
}
