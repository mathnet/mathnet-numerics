// <copyright file="HybridMCTest.cs" company="Math.NET">
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

using MathNet.Numerics.Random;
using NUnit.Framework;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Statistics.Mcmc;
using MathNet.Numerics.Statistics.Mcmc.Diagnostics;

namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{

    /// <summary>
    /// Tests for the HybridMC class.
    /// </summary>
    [TestFixture]
    public class HybridMCTest
    {
        /// <summary>
        ///Random number generator to be used in the test.
        /// </summary>
        private MersenneTwister rnd = new MersenneTwister();

        private Distributions.Normal normal = new Distributions.Normal(0, 1);

        /// <summary>
        /// Testing the constructor to make sure that RandomSource is 
        /// assigned.
        /// </summary>
        [Test]
        public void RandomSourceTest()
        {

            HybridMC Hybrid = new HybridMC(new double[1] { 0 }, x => normal.DensityLn(x[0]), 10, 0.1);

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
                => new HybridMC(new double[1] { 0 }, x => normal.DensityLn(x[0]), 0, 0.1));
        }

        /// <summary>
        /// Test the range of StepSize. Sets StepSize
        /// to negative or zero throws <c>ArgumentOutOfRangeException</c>. 
        /// </summary>
        [Test]
        public void StepSizeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new HybridMC(new double[1] { 0 }, x => normal.DensityLn(x[0]), 1, 0));
        }

        /// <summary>
        /// Test the range of BurnInterval. Sets BurnInterval
        /// to negative throws <c>ArgumentOutOfRangeException</c>. 
        /// </summary>
        [Test]
        public void BurnIntervalTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new HybridMC(new double[1] { 0 }, x => normal.DensityLn(x[0]), 10, 0.1, -1));
        }

        
        /// <summary>
        /// Test the range of MomentumStdDev. Sets MomentumStdDev
        /// to negative throws <c>ArgumentNullException</c>. 
        /// </summary>
        [Test]
        public void MomentumStdDevNegativeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new HybridMC(new double[1] { 0 }, x => normal.DensityLn(x[0]), 10, 0.1, 0, new double[1] { -1 }));
        }

        /// <summary>
        /// Test the length of MomentumStdDev. Sets MomentumStdDev 
        /// to a length different from the length of samples throws <c>ArgumentOutOfRangeException</c>. 
        /// </summary>
        [Test]
        public void MomentumStdDevLengthTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => new HybridMC(new double[1] { 0 }, x => normal.DensityLn(x[0]), 10, 0.1, 0, new double[2]));
        }

        #endregion

        /// <summary>
        /// Test the sampler using  a bivariate normal distribution with randomly selected mean and standard deviation.
        /// It is a statistical test and may not pass all the time. 
        /// </summary>
        [Test]
        public void SampleTest()
        {

            //Variances and Means
            double[] Sdv = new double[2];
            double[] Mean = new double[2];
            for (int i = 0; i < 2; i++)
            {
                Sdv[i] = RandomVar();
                Mean[i] = 5 * rnd.NextDouble();

            }

            //Cross Variance
            double rho = 1.0 - RandomVar();

            DensityLn<double[]> pdfLn = new DensityLn<double[]>(x => LogDen(x, Sdv, Mean, rho));


            HybridMC Hybrid = new HybridMC(new double[2] { 0, 0 }, pdfLn, 10, 0.1, 1000);

            Hybrid.BurnInterval = 0;

            int SampleSize = 10000;
            double[][] Sample = Hybrid.Sample(SampleSize);
            double[][] NewSamples = ArrangeSamples(SampleSize, Sample);

            double[] Convergence = new double[2];
            double[] SampleMean = new double[2];
            double[] SampleSdv = new double[2];


            for (int i = 0; i < 2; i++)
            {
                Convergence[i] = 1 / Math.Sqrt(MCMCDiagnostics.EffectiveSize(Sample,x=>x[i]));
                DescriptiveStatistics Stats = new DescriptiveStatistics(NewSamples[i]);
                SampleMean[i] = Stats.Mean;
                SampleSdv[i] = Stats.StandardDeviation;

            }
            double SampleRho = Correlation.Pearson(NewSamples[0], NewSamples[1]);

            for (int i = 0; i < 2; i++)
            {
                string index = i.ToString();
                Assert.AreEqual(Mean[i], SampleMean[i], 10 * Convergence[i], index + "Mean");
                Assert.AreEqual(SampleSdv[i] * SampleSdv[i], Sdv[i] * Sdv[i], 10 * Convergence[i], index + "Standard Deviation");
            }

            double ConvergenceRho=1/Math.Sqrt(MCMCDiagnostics.EffectiveSize(Sample,x=>(x[0]-SampleMean[0])*(x[1]-SampleMean[1])));

            Assert.AreEqual(SampleRho*SampleSdv[0]*SampleSdv[1], rho*Sdv[0]*Sdv[1], 10 * ConvergenceRho, "Rho");

        }

        #region Helper Methods
        /// <summary>
        /// Generator for standard deviations and means used in SampleTest.
        /// </summary>
        /// <returns>A random number between 0.1 and 1.</returns>
        private double RandomVar()
        {
            double Var = 0;
            while (Var <= 0.1)
            { Var = rnd.NextDouble(); }
            return Var;
        }

        /// <summary>
        /// The log density of the bivariate normal distribution used in SampleTest.
        /// </summary>
        /// <param name="x">Location to evaluate the density.</param>
        /// <param name="Sdv">Standard deviation.</param>
        /// <param name="Mean">Mean.</param>
        /// <param name="rho">Correlation of the two variables.</param>
        /// <returns>Value of the log density.</returns>
        private double LogDen(double[] x, double[] Sdv, double[] Mean, double rho)
        {
            if (x.Length != 2 || Sdv.Length != 2 || Mean.Length != 2)
                throw new ArgumentOutOfRangeException("LogDen must take a 2 dimensional array");
            double xDiv = x[0] - Mean[0];
            double yDiv = x[1] - Mean[1];
            double xVar = Sdv[0] * Sdv[0];
            double yVar = Sdv[1] * Sdv[1];


            return (-(0.5 / (1 - rho * rho)) * ((xDiv * xDiv) / xVar + (yDiv * yDiv) / yVar - 2 * rho * xDiv * yDiv / (Sdv[0] * Sdv[1])));


        }

        /// <summary>
        /// Method to rearrange the array of samples in to separated arrays.
        /// </summary>
        /// <param name="SampleSize">Size of the sample.</param>
        /// <param name="Sample">Sample from the HybridMC.</param>
        /// <returns>An array whose first entry is the samples in the first variable and 
        /// second entry is the samples in the second variable.</returns>
        private double[][] ArrangeSamples(int SampleSize, double[][] Sample)
        {

            double[] xSample = new double[SampleSize];
            double[] ySample = new double[SampleSize];

            for (int i = 0; i < SampleSize; i++)
            {
                xSample[i] = Sample[i][0];
                ySample[i] = Sample[i][1];
            }

            return new double[2][] { xSample, ySample };
        }
        #endregion
    }
}
