// <copyright file="CommonDistributionTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using MbUnit.Framework;
    using MathNet.Numerics.Random;
    using MathNet.Numerics.Statistics;
    using MathNet.Numerics.Distributions;

    [TestFixture]
    public class CommonDistributionTests
    {
        // The number of samples we want.
        private int numberOfTestSamples = 100000;
        // The accuracy of the histograms.
        private double sampleAccuracy = 0.01;
        // The number of buckets to use to test against the cdf.
        private int numberOfBuckets = 100;
        // The list of discrete distributions which we test.
        private List<IDiscreteDistribution> discreteDistributions;
        // The list of continuous distributions which we test.
        private List<IContinuousDistribution> continuousDistributions;

        [SetUp]
        public void SetupDistributions()
        {
            discreteDistributions = new List<IDiscreteDistribution>();
            discreteDistributions.Add(new Bernoulli(0.6));
            discreteDistributions.Add(new Binomial(0.7, 10));
            discreteDistributions.Add(new Categorical(new double[] { 0.7, 0.3 }));
            discreteDistributions.Add(new DiscreteUniform(1, 10));

            continuousDistributions = new List<IContinuousDistribution>();
            continuousDistributions.Add(new Beta(1.0, 1.0));
            continuousDistributions.Add(new ContinuousUniform(0.0, 1.0));
            continuousDistributions.Add(new Gamma(1.0, 1.0));
            continuousDistributions.Add(new Normal(0.0, 1.0));
            continuousDistributions.Add(new Weibull(1.0, 1.0));
            continuousDistributions.Add(new LogNormal(1.0, 1.0));
            //continuousDistributions.Add(new StudentT(0.0, 1.0, 3.0));
        }

        [Test]
        [MultipleAsserts]
        public void ValidateThatUnivariateDistributionsHaveRandomSource(int i)
        {
            foreach(var dd in discreteDistributions)
            {
                Assert.IsNotNull(dd.RandomSource);
            }

            foreach(var cd in continuousDistributions)
            {
                Assert.IsNotNull(cd.RandomSource);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSetRandomSource(int i)
        {
            foreach(var dd in discreteDistributions)
            {
                dd.RandomSource = new Random();
            }

            foreach(var cd in continuousDistributions)
            {
                cd.RandomSource = new Random();
            }
        }

        [Test]
        [MultipleAsserts]
        public void FailSetRandomSourceWithNullReference(int i)
        {
            foreach(var dd in discreteDistributions)
            {
                Assert.Throws<ArgumentNullException>(() => dd.RandomSource = null);
            }

            foreach(var cd in continuousDistributions)
            {
                Assert.Throws<ArgumentNullException>(() => cd.RandomSource = null);
            }
        }

        [Test]
        [MultipleAsserts]
        public void SampleFollowsCorrectDistribution()
        {
            Random rnd = new MersenneTwister();

            // The test samples from the distributions, builds a histogram and checks
            // whether the histogram follows the CDF.
            foreach (var dd in discreteDistributions)
            {
                dd.RandomSource = rnd;

                double[] samples = new double[numberOfTestSamples];
                for (int i = 0; i < numberOfTestSamples; i++)
                {
                    samples[i] = (double) dd.Sample();
                }

                var histogram = new Histogram(samples, numberOfBuckets);
                for (int i = 0; i < numberOfBuckets; i++)
                {
                    var bucket = histogram[i];
                    double empiricalProbability = bucket.Count / (double)numberOfTestSamples;
                    double realProbability = dd.CumulativeDistribution(bucket.UpperBound)
                        - dd.CumulativeDistribution(bucket.LowerBound);
                    Assert.LessThan(Math.Abs(empiricalProbability - realProbability), sampleAccuracy, dd.ToString());
                }
            }

            foreach (var cd in continuousDistributions)
            {
                cd.RandomSource = rnd;
                double[] samples = new double[numberOfTestSamples];
                for (int i = 0; i < numberOfTestSamples; i++)
                {
                    samples[i] = cd.Sample();
                }

                var histogram = new Histogram(samples, numberOfBuckets);
                for (int i = 0; i < numberOfBuckets; i++)
                {
                    var bucket = histogram[i];
                    double empiricalProbability = bucket.Count / (double)numberOfTestSamples;
                    double realProbability = cd.CumulativeDistribution(bucket.UpperBound)
                        - cd.CumulativeDistribution(bucket.LowerBound);
                    Assert.LessThan(Math.Abs(empiricalProbability - realProbability), sampleAccuracy, cd.ToString());
                }
            }
        }

        [Test]
        [MultipleAsserts]
        public void SamplesFollowsCorrectDistribution()
        {
            Random rnd = new MersenneTwister();

            // The test samples from the distributions, builds a histogram and checks
            // whether the histogram follows the CDF.
            foreach (var dd in discreteDistributions)
            {
                dd.RandomSource = rnd;
                var samples = dd.Samples().Take(numberOfTestSamples).Select(x => (double)x);

                var histogram = new Histogram(samples, numberOfBuckets);
                for (int i = 0; i < numberOfBuckets; i++)
                {
                    var bucket = histogram[i];
                    double empiricalProbability = bucket.Count / (double)numberOfTestSamples;
                    double realProbability = dd.CumulativeDistribution(bucket.UpperBound)
                        - dd.CumulativeDistribution(bucket.LowerBound);
                    Assert.LessThan(Math.Abs(empiricalProbability - realProbability), sampleAccuracy, dd.ToString());
                }
            }

            foreach (var cd in continuousDistributions)
            {
                cd.RandomSource = rnd;
                var samples = cd.Samples().Take(numberOfTestSamples);

                var histogram = new Histogram(samples, numberOfBuckets);
                for (int i = 0; i < numberOfBuckets; i++)
                {
                    var bucket = histogram[i];
                    double empiricalProbability = bucket.Count / (double)numberOfTestSamples;
                    double realProbability = cd.CumulativeDistribution(bucket.UpperBound)
                        - cd.CumulativeDistribution(bucket.LowerBound);
                    Assert.LessThan(Math.Abs(empiricalProbability - realProbability), sampleAccuracy, cd.ToString());
                }
            }
        }
    }
}
