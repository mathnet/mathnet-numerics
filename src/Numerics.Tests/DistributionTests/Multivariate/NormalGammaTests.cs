// <copyright file="NormalGammaTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Multivariate
{
    /// <summary>
    /// <c>NormalGamma</c> distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class NormalGammaTests
    {
        /// <summary>
        /// Can create <c>NormalGamma</c>.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(10.0, 2.0, 2.0, 2.0)]
        public void CanCreateNormalGamma(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);

            Assert.AreEqual(meanLocation, ng.MeanLocation);
            Assert.AreEqual(meanScale, ng.MeanScale);
            Assert.AreEqual(precShape, ng.PrecisionShape);
            Assert.AreEqual(precInvScale, ng.PrecisionInverseScale);
        }

        /// <summary>
        /// Can get density and density log.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(10.0, 1.0, 2.0, 2.0)]
        public void CanGetDensityAndDensityLn(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual(ng.DensityLn(meanLocation, precShape), Math.Log(ng.Density(meanLocation, precShape)), 1e-14);
        }

        /// <summary>
        /// <c>NormalGamma</c> constructor fails with invalid params.
        /// </summary>
        [Test]
        public void NormalGammaConstructorFailsWithInvalidParams()
        {
            Assert.That(() => new NormalGamma(1.0, -1.3, 2.0, 2.0), Throws.ArgumentException);
            Assert.That(() => new NormalGamma(1.0, 1.0, -1.0, 1.0), Throws.ArgumentException);
            Assert.That(() => new NormalGamma(1.0, 1.0, 1.0, -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Can get mean location.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(10.0, 2.0, 2.0, 2.0)]
        public void CanGetMeanLocation(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual(meanLocation, ng.MeanLocation);
        }

        /// <summary>
        /// Can get mean scale.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(10.0, 2.0, 2.0, 2.0)]
        public void CanGetMeanScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual(meanScale, ng.MeanScale);
        }

        /// <summary>
        /// Can get precision shape.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(10.0, 2.0, 2.0, 2.0)]
        public void CanGetPrecisionShape(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual(precShape, ng.PrecisionShape);
        }

        /// <summary>
        /// Can get precision inverse scale.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(10.0, 2.0, 2.0, 2.0)]
        public void CanGetPrecisionInverseScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual(precInvScale, ng.PrecisionInverseScale);
        }

        /// <summary>
        /// Can get mean marginals.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        /// <param name="meanMarginalMean">Mean marginal mean.</param>
        /// <param name="meanMarginalScale">Mean marginal scale.</param>
        /// <param name="meanMarginalDoF">Mean marginal degrees of freedom.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0, 0.0, 1.0, 2.0)]
        [TestCase(10.0, 1.0, 2.0, 2.0, 10.0, 1.0, 4.0)]
        [TestCase(10.0, 1.0, 2.0, Double.PositiveInfinity, 10.0, 0.5, Double.PositiveInfinity)]
        public void CanGetMeanMarginal(double meanLocation, double meanScale, double precShape, double precInvScale, double meanMarginalMean, double meanMarginalScale, double meanMarginalDoF)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            var mm = ng.MeanMarginal();
            Assert.AreEqual(meanMarginalMean, mm.Location);
            Assert.AreEqual(meanMarginalScale, mm.Scale);
            Assert.AreEqual(meanMarginalDoF, mm.DegreesOfFreedom);
        }

        /// <summary>
        /// Can get precision marginal.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(10.0, 2.0, 2.0, 2.0)]
        [TestCase(10.0, 2.0, 2.0, Double.PositiveInfinity)]
        public void CanGetPrecisionMarginal(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            var pm = ng.PrecisionMarginal();
            Assert.AreEqual(precShape, pm.Shape);
            Assert.AreEqual(precInvScale, pm.Rate);
        }

        /// <summary>
        /// Can get mean.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        /// <param name="meanMean">Mean value.</param>
        /// <param name="meanPrecision">Mean precision.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0, 0.0, 1.0)]
        [TestCase(10.0, 1.0, 2.0, 2.0, 10.0, 1.0)]
        [TestCase(10.0, 1.0, 2.0, Double.PositiveInfinity, 10.0, 2.0)]
        public void CanGetMean(double meanLocation, double meanScale, double precShape, double precInvScale, double meanMean, double meanPrecision)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            Assert.AreEqual(meanMean, ng.Mean.Mean);
            Assert.AreEqual(meanPrecision, ng.Mean.Precision);
        }

        /// <summary>
        /// Has random source.
        /// </summary>
        [Test]
        public void HasRandomSource()
        {
            var ng = new NormalGamma(0.0, 1.0, 1.0, 1.0);
            Assert.IsNotNull(ng.RandomSource);
        }

        /// <summary>
        /// Can set random source.
        /// </summary>
        [Test]
        public void CanSetRandomSource()
        {
            GC.KeepAlive(new NormalGamma(0.0, 1.0, 1.0, 1.0)
            {
                RandomSource = new System.Random(0)
            });
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="meanLocation">Mean location.</param>
        /// <param name="meanScale">Mean scale.</param>
        /// <param name="precShape">Precision shape.</param>
        /// <param name="precInvScale">Precision inverse scale.</param>
        [TestCase(0.0, 1.0, 1.0, 1.0)]
        [TestCase(10.0, 2.0, 2.0, 2.0)]
        [TestCase(10.9, 2.0, 2.0, Double.PositiveInfinity)]
        public void ValidateVariance(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            var x = precInvScale / (meanScale * (precShape - 1));
            var t = precShape / Math.Sqrt(precInvScale);
            Assert.AreEqual(x, ng.Variance.Mean);
            Assert.AreEqual(t, ng.Variance.Precision);
        }

        /// <summary>
        /// Test the method which samples one variable at a time.
        /// </summary>
        [Test]
        public void SampleFollowsCorrectDistribution()
        {
            var cd = new NormalGamma(1.0, 4.0, 7.0, 3.5);

            // Sample from the distribution.
            var samples = new MeanPrecisionPair[CommonDistributionTests.NumberOfTestSamples];
            for (var i = 0; i < CommonDistributionTests.NumberOfTestSamples; i++)
            {
                samples[i] = cd.Sample();
            }

            // Extract the mean and precisions.
            var means = samples.Select(mp => mp.Mean).ToArray();
            var precs = samples.Select(mp => mp.Precision).ToArray();
            var meanMarginal = cd.MeanMarginal();
            var precMarginal = cd.PrecisionMarginal();

            // Check the precision distribution.
            CommonDistributionTests.ContinuousVapnikChervonenkisTest(
                CommonDistributionTests.ErrorTolerance,
                CommonDistributionTests.ErrorProbability,
                precs,
                precMarginal);

            // Check the mean distribution.
            CommonDistributionTests.ContinuousVapnikChervonenkisTest(
                CommonDistributionTests.ErrorTolerance,
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
            var cd = new NormalGamma(1.0, 4.0, 3.0, 3.5);

            // Sample from the distribution.
            var samples = cd.Samples().Take(CommonDistributionTests.NumberOfTestSamples).ToArray();

            // Extract the mean and precisions.
            var means = samples.Select(mp => mp.Mean).ToArray();
            var precs = samples.Select(mp => mp.Precision).ToArray();
            var meanMarginal = cd.MeanMarginal();
            var precMarginal = cd.PrecisionMarginal();

            // Check the precision distribution.
            CommonDistributionTests.ContinuousVapnikChervonenkisTest(
                CommonDistributionTests.ErrorTolerance,
                CommonDistributionTests.ErrorProbability,
                precs,
                precMarginal);

            // Check the mean distribution.
            CommonDistributionTests.ContinuousVapnikChervonenkisTest(
                CommonDistributionTests.ErrorTolerance,
                CommonDistributionTests.ErrorProbability,
                means,
                meanMarginal);
        }
    }
}
