// <copyright file="NormalGammaTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Multivariate
{
	using System;
	using System.Linq;
	using MbUnit.Framework;
	using Numerics.Random;
	using Distributions;

	[TestFixture]
    public class NormalGammaTests
    {
        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanCreateNormalGamma(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);

            Assert.AreEqual<double>(meanLocation, ng.MeanLocation);
            Assert.AreEqual<double>(meanScale, ng.MeanScale);
            Assert.AreEqual<double>(precShape, ng.PrecisionShape);
            Assert.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test]
        [Row(1.0, -1.3, 2.0, 2.0)]
        [Row(1.0, 1.0, -1.0, 1.0)]
        [Row(1.0, 1.0, 1.0, -1.0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NormalGammaConstructorFailsWithInvalidParams(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var nb = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanGetMeanLocation(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual<double>(meanLocation, ng.MeanLocation);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanSetMeanLocation(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            ng.MeanLocation = -5.0;

            Assert.AreEqual<double>(-5.0, ng.MeanLocation);
            Assert.AreEqual<double>(meanScale, ng.MeanScale);
            Assert.AreEqual<double>(precShape, ng.PrecisionShape);
            Assert.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanGetMeanScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual<double>(meanScale, ng.MeanScale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanSetMeanScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            ng.MeanScale = 5.0;
            Assert.AreEqual<double>(meanLocation, ng.MeanLocation);
            Assert.AreEqual<double>(5.0, ng.MeanScale);
            Assert.AreEqual<double>(precShape, ng.PrecisionShape);
            Assert.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanGetPrecisionShape(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual<double>(precShape, ng.PrecisionShape);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanSetPrecisionShape(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            ng.PrecisionShape = 5.0;
            Assert.AreEqual<double>(meanLocation, ng.MeanLocation);
            Assert.AreEqual<double>(meanScale, ng.MeanScale);
            Assert.AreEqual<double>(5.0, ng.PrecisionShape);
            Assert.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanGetPrecisionInverseScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanSetPrecisionPrecisionInverseScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            ng.PrecisionInverseScale = 5.0;
            Assert.AreEqual<double>(meanLocation, ng.MeanLocation);
            Assert.AreEqual<double>(meanScale, ng.MeanScale);
            Assert.AreEqual<double>(precShape, ng.PrecisionShape);
            Assert.AreEqual<double>(5.0, ng.PrecisionInverseScale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0, 0.0, 1.0, 2.0)]
        [Row(10.0, 1.0, 2.0, 2.0, 10.0, 1.0, 4.0)]
        [Row(10.0, 1.0, 2.0, Double.PositiveInfinity, 10.0, 0.5, Double.PositiveInfinity)]
        public void CanGetMeanMarginal(double meanLocation, double meanScale, double precShape, double precInvScale,
            double meanMarginalMean, double meanMarginalScale, double meanMarginalDoF)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            var mm = ng.MeanMarginal();
            Assert.AreEqual<double>(meanMarginalMean, mm.Location);
            Assert.AreEqual<double>(meanMarginalScale, mm.Scale);
            Assert.AreEqual<double>(meanMarginalDoF, mm.DegreesOfFreedom);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        [Row(10.0, 1.0, 2.0, Double.PositiveInfinity)]
        public void CanGetPrecisionMarginal(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            var pm = ng.PrecisionMarginal();
            Assert.AreEqual<double>(precShape, pm.Shape);
            Assert.AreEqual<double>(precInvScale, pm.InvScale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0, 0.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0, 10.0, 1.0)]
        [Row(10.0, 1.0, 2.0, Double.PositiveInfinity, 10.0, 2.0)]
        public void CanGetMean(double meanLocation, double meanScale, double precShape, double precInvScale,
            double meanMean, double meanPrecision)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual<double>(meanMean, ng.Mean.Mean);
            Assert.AreEqual<double>(meanPrecision, ng.Mean.Precision);
        }

        [Test]
        public void HasRandomSource()
        {
            NormalGamma ng = new NormalGamma(0.0, 1.0, 1.0, 1.0);
            Assert.IsNotNull(ng.RandomSource);
        }

        [Test]
        public void CanSetRandomSource()
        {
            NormalGamma ng = new NormalGamma(0.0, 1.0, 1.0, 1.0);
            ng.RandomSource = new Random();
        }

        /// <summary>
        /// Test the method which samples one variable at a time.
        /// </summary>
        [Test]
        public void SampleFollowsCorrectDistribution()
        {
            Random rnd = new MersenneTwister();
            //var cd = new NormalGamma(1.0, 4.0, 3.0, 3.5);
            var cd = new NormalGamma(1.0, 4.0, 7.0, 3.5);

            // Sample from the distribution.
            MeanPrecisionPair[] samples = new MeanPrecisionPair[CommonDistributionTests.NumberOfTestSamples];
            for (int i = 0; i < CommonDistributionTests.NumberOfTestSamples; i++)
            {
                samples[i] = cd.Sample();
            }

            // Extract the mean and precisions.
            var means = samples.Select(mp => mp.Mean);
            var precs = samples.Select(mp => mp.Precision);
            var meanMarginal = cd.MeanMarginal();
            var precMarginal = cd.PrecisionMarginal();

            // Check the precision distribution.
            CommonDistributionTests.VapnikChervonenkisTest(
                CommonDistributionTests.Error,
                CommonDistributionTests.ErrorProbability,
                precs,
                precMarginal);

            // Check the mean distribution.
            CommonDistributionTests.VapnikChervonenkisTest(
                CommonDistributionTests.Error,
                CommonDistributionTests.ErrorProbability, 
                means,
                meanMarginal);
        }

        /// <summary>
        /// Test the method which samples a sequence of variables.
        /// </summary>
        [Test]
        public void SamplesFollowsCorrectDistribution()
        {
            Random rnd = new MersenneTwister();
            var cd = new NormalGamma(1.0, 4.0, 3.0, 3.5);

            // Sample from the distribution.
            var samples = cd.Samples().Take(CommonDistributionTests.NumberOfTestSamples).ToArray();

            // Extract the mean and precisions.
            var means = samples.Select(mp => mp.Mean);
            var precs = samples.Select(mp => mp.Precision);
            var meanMarginal = cd.MeanMarginal();
            var precMarginal = cd.PrecisionMarginal();

            // Check the precision distribution.
            CommonDistributionTests.VapnikChervonenkisTest(
                CommonDistributionTests.Error,
                CommonDistributionTests.ErrorProbability,
                precs,
                precMarginal);

            // Check the mean distribution.
            CommonDistributionTests.VapnikChervonenkisTest(
                CommonDistributionTests.Error,
                CommonDistributionTests.ErrorProbability, 
                means,
                meanMarginal);
        }
    }
}