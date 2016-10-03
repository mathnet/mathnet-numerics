// <copyright file="UnivariateHybridMCTest.cs" company="Math.NET">
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
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Statistics.Mcmc;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{
    /// <summary>
    /// Test for the UnivariateHybridMC.
    /// </summary>
    [TestFixture, Category("Statistics")]
    public class UnivariateHybridMCTest
    {
        /// <summary>
        /// Use for testing the constructor.
        /// </summary>
        readonly Normal _normal = new Normal(0.0, 1.0);

        /// <summary>
        /// Testing the constructor to make sure that RandomSource is
        /// assigned.
        /// </summary>
        [Test]
        public void UnivariateHybridMCConstructor()
        {
            var hybrid = new UnivariateHybridMC(0, _normal.DensityLn, 10, 0.1);
            Assert.IsNotNull(hybrid.RandomSource);

            hybrid.RandomSource = new System.Random(0);
            Assert.IsNotNull(hybrid.RandomSource);
        }


        #region Tests for the argument checking logic

        /// <summary>
        /// Test the range of FrogLeapSteps. Sets FrogLeapSteps
        /// to negative or zero throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void FrogLeapTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new UnivariateHybridMC(0, x => _normal.DensityLn(x), 0, 0.1));
        }

        /// <summary>
        /// Test the range of StepSize. Sets StepSize
        /// to negative or zero throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void StepSizeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new UnivariateHybridMC(0, x => _normal.DensityLn(x), 1, 0));
        }

        /// <summary>
        /// Test the range of BurnInterval. Sets BurnInterval
        /// to negative throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void BurnIntervalTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new UnivariateHybridMC(0, x => _normal.DensityLn(x), 10, 0.1, -1));
        }

        /// <summary>
        /// Test the range of MomentumStdDev. Sets MomentumStdDev
        /// to negative throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void MomentumStdDevNegativeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new UnivariateHybridMC(0, x => _normal.DensityLn(x), 10, 0.1, 0, -1));
        }

        #endregion


        /// <summary>
        /// Test the sampler using normal distribution with randomly selected mean and standard deviation.
        /// It is a statistical test and may not pass all the time.
        /// </summary>
        [TestCase(-4.987, 3.3, 1000)]
        [TestCase(3.987, 1.2, 1000)]
        [TestCase(-2.987, 4.3, 1000)]
        public void SampleTest(double mean, double sdv, int seed)
        {

            var dis = new Normal(mean, sdv);
            var hybrid = new UnivariateHybridMC(0, dis.DensityLn, 10, 0.1, 1000, 1, new System.Random(seed))
                {
                    BurnInterval = 0
                };
            double[] sample = hybrid.Sample(10000);

            double effective = MCMCDiagnostics.EffectiveSize(sample, x => x);

            var stats = new DescriptiveStatistics(sample);

            //Testing the mean using the normal distribution.
            double meanConvergence = 3*sdv/Math.Sqrt(effective);

            Assert.AreEqual(stats.Mean, mean, meanConvergence, "Mean");

            double deviationRation = Math.Pow(stats.StandardDeviation/sdv, 2);

            //Approximating chi-square with normal distribution. (Degree of freedom is large)
            double deviationConvergence = 3*Constants.Sqrt2/Math.Sqrt(effective);
            Assert.AreEqual(deviationRation, 1, deviationConvergence, "Standard Deviation");
        }
    }
}
