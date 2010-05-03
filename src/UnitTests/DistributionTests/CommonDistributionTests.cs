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

    /// <summary>
    /// This class will perform various tests on discrete and continuous univariate distributions. The multivariate distributions
    /// will implement these respective tests in their local unit test classes as they do not adhere to the same interfaces.
    /// </summary>
    [TestFixture]
    public class CommonDistributionTests
    {
        // The number of samples we want.
        public static int NumberOfTestSamples = 3500000;
        // The number of buckets in the histogram for the sampling function tests.
        public static int NumberOfBuckets = 100;
        // The error we want to tolerate for sampling functions.
        public static double Error = 0.01;
        // The error probability we want to tolerate for sampling functions.
        public static double ErrorProbability = 0.001;
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
            continuousDistributions.Add(new StudentT(0.0, 1.0, 5.0));
        }

        [Test]
        [MultipleAsserts]
        public void ValidateThatUnivariateDistributionsHaveRandomSource()
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
        public void CanSetRandomSource()
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
        public void FailSetRandomSourceWithNullReference()
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

        /// <summary>
        /// Test the method which samples only one variable at a time.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void SampleFollowsCorrectDistribution()
        {
            Random rnd = new MersenneTwister(1);

            foreach (var dd in discreteDistributions)
            {
                dd.RandomSource = rnd;
                double[] samples = new double[NumberOfTestSamples];
                for (int i = 0; i < NumberOfTestSamples; i++)
                {
                    samples[i] = (double)dd.Sample();
                }
                VapnikChervonenkisTest(Error, ErrorProbability, samples, dd);
            }

            foreach (var cd in continuousDistributions)
            {
                cd.RandomSource = rnd;
                double[] samples = new double[NumberOfTestSamples];
                for (int i = 0; i < NumberOfTestSamples; i++)
                {
                    samples[i] = cd.Sample();
                }
                VapnikChervonenkisTest(Error, ErrorProbability, samples, cd);
            }
        }

        /// <summary>
        /// Test the method which samples a sequence of variables.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void SamplesFollowsCorrectDistribution()
        {
            Random rnd = new MersenneTwister(1);

            foreach (var dd in discreteDistributions)
            {
                dd.RandomSource = rnd;
                VapnikChervonenkisTest(Error, ErrorProbability, dd.Samples().Select(x => (double) x).Take(NumberOfTestSamples), dd);
            }

            foreach (var cd in continuousDistributions)
            {
                cd.RandomSource = rnd;
                VapnikChervonenkisTest(Error, ErrorProbability, cd.Samples().Take(NumberOfTestSamples), cd);
            }
        }

        /// <summary>
        /// <para>Using VC-dimension, we can bound the probability of making an error when estimating empirical probability
        /// distributions. We are using Theorem 2.41 in "All Of Nonparametric Statistics".
        /// http://books.google.com/books?id=MRFlzQfRg7UC&lpg=PP1&dq=all%20of%20nonparametric%20statistics&pg=PA22#v=onepage&q=%22shatter%20coe%EF%AC%83cients%20do%20not%22&f=false .</para>
        /// <para>Note that for intervals on the real line the VC-dimension is 2.</para>
        /// </summary>
        /// <param name="epsilon">The error we are willing to tolerate.</param>
        /// <param name="delta">The error probability we are willing to tolerate.</param>
        /// <param name="s">The samples to use for testing.</param>
        /// <param name="dist">The distribution we are testing.</param>
        public static void VapnikChervonenkisTest(double epsilon, double delta, IEnumerable<double> s, IDistribution dist)
        {
            double N = (double) s.Count();
            Assert.GreaterThan(N, Math.Ceiling(32.0 * Math.Log(16.0 / delta) / epsilon / epsilon));

            var histogram = new Histogram(s, NumberOfBuckets);

            for (int i = 0; i < NumberOfBuckets; i++)
            {
                double p = dist.CumulativeDistribution(histogram[i].UpperBound) - dist.CumulativeDistribution(histogram[i].LowerBound);
                double pe = histogram[i].Count / N;
                Assert.LessThan(Math.Abs(p - pe), epsilon, dist.ToString());
            }
        }
    }
}
