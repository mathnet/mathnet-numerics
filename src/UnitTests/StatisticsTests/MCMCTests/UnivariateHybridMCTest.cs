// <copyright file="UnivariateHybridMCTest.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using NUnit.Framework;
using MathNet.Numerics.Statistics.Mcmc;
using MathNet.Numerics.Statistics.Mcmc.Diagnostics;
using MathNet.Numerics.Statistics;


namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{

    /// <summary>
    /// Test for the UnivariateHybridMC.
    /// </summary>
    [TestFixture]
    public class UnivariateHybridMCTest
    {
        /// <summary>
        /// Use for testing the constructor.
        /// </summary>
        private Normal normal = new Normal(0.0, 1.0);


        /// <summary>
        /// Testing the constructor to make sure that RandomSource is 
        /// assigned.
        /// </summary>
        [Test]
        public void UnivariateHybridMCConstructor()
        {

            UnivariateHybridMC Hybrid = new UnivariateHybridMC(0, normal.DensityLn, 10, 0.1);

            Assert.IsNotNull(Hybrid.RandomSource);

            Hybrid.RandomSource = new System.Random();
            Assert.IsNotNull(Hybrid.RandomSource);
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
                => new UnivariateHybridMC(0, x => normal.DensityLn(x), 0, 0.1));
        }

        /// <summary>
        /// Test the range of StepSize. Sets StepSize
        /// to negative or zero throws <c>ArgumentOutOfRangeException</c>. 
        /// </summary>
        [Test]
        public void StepSizeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new UnivariateHybridMC(0, x => normal.DensityLn(x), 1, 0));
        }

        /// <summary>
        /// Test the range of BurnInterval. Sets BurnInterval
        /// to negative throws <c>ArgumentOutOfRangeException</c>. 
        /// </summary>
        [Test]
        public void BurnIntervalTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new UnivariateHybridMC(0, x => normal.DensityLn(x), 10, 0.1, -1));
        }

        /// <summary>
        /// Test the range of MomentumStdDev. Sets MomentumStdDev
        /// to negative throws <c>ArgumentNullException</c>. 
        /// </summary>
        [Test]
        public void MomentumStdDevNegativeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new UnivariateHybridMC(0, x => normal.DensityLn(x), 10, 0.1, 0, -1));
        }

        #endregion


        /// <summary>
        /// Test the sampler using normal distribution with randomly selected mean and standard deviation.
        /// It is a statistical test and may not pass all the time. 
        /// </summary>
        [TestCase(-4.987, 3.3, 1000)]
        [TestCase(3.987, 1.2, 1000)]
        [TestCase(-2.987, 4.3, 1000)]
        public void SampleTest(double Mean, double Sdv, int seed)
        {

            Normal dis = new Normal(Mean, Sdv);
            UnivariateHybridMC Hybrid = new UnivariateHybridMC(0, dis.DensityLn, 10, 0.1, 1000, 1, new System.Random(seed));
            Hybrid.BurnInterval = 0;
            double[] Sample = Hybrid.Sample(10000);

            double Effective = MCMCDiagnostics.EffectiveSize(Sample, x => x);

            DescriptiveStatistics Stats = new DescriptiveStatistics(Sample);

            //Testing the mean using the normal distribution. 
            double MeanConvergence = 3 * Sdv / Math.Sqrt(Effective);

            Assert.AreEqual(Stats.Mean, Mean, MeanConvergence, "Mean");

            double DeviationRation = Math.Pow(Stats.StandardDeviation / Sdv, 2);

            //Approximating chi-square with normal distribution. (Degree of freedom is large)
            double DeviationConvergence = 3 * Math.Sqrt(2) / Math.Sqrt(Effective);
            Assert.AreEqual(DeviationRation, 1, DeviationConvergence, "Standard Deivation");
        }
    }
}
