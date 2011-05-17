// <copyright file="CommonDistributionTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
    using System.Collections.Generic;
    using System.Linq;
    using Distributions;
    using Numerics.Random;
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// This class will perform various tests on discrete and continuous univariate distributions. The multivariate distributions
    /// will implement these respective tests in their local unit test classes as they do not adhere to the same interfaces.
    /// </summary>
    [TestFixture]
    public class CommonDistributionTests
    {
        /// <summary>
        /// Gets or sets the number of samples we want.
        /// </summary>
        public static int NumberOfTestSamples
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of buckets in the histogram for the sampling function tests.
        /// </summary>
        public static int NumberOfBuckets
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error we want to tolerate for sampling functions.
        /// </summary>
        public static double Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error probability we want to tolerate for sampling functions.
        /// </summary>
        public static double ErrorProbability
        {
            get;
            set;
        }

        /// <summary>
        /// The list of discrete distributions which we test.
        /// </summary>
        private List<IDiscreteDistribution> _discreteDistributions;

        /// <summary>
        /// The list of continuous distributions which we test.
        /// </summary>
        private List<IContinuousDistribution> _continuousDistributions;

        /// <summary>
        /// Initializes static members of the CommonDistributionTests class.
        /// </summary>
        static CommonDistributionTests()
        {
            NumberOfTestSamples = 3500000;
            NumberOfBuckets = 100;
            Error = 0.01;
            ErrorProbability = 0.001;
        }

        /// <summary>
        /// Set-up test parameters.
        /// </summary>
        [SetUp]
        public void SetupDistributions()
        {
            _discreteDistributions = new List<IDiscreteDistribution>
                                     {
                                         new Bernoulli(0.6),
                                         new Binomial(0.7, 10),
                                         new Categorical(new[] { 0.7, 0.3 }),
                                         new DiscreteUniform(1, 10)
                                     };

            _continuousDistributions = new List<IContinuousDistribution>
                                       {
                                           new Beta(1.0, 1.0),
                                           new ContinuousUniform(0.0, 1.0),
                                           new Gamma(1.0, 1.0),
                                           new Normal(0.0, 1.0),
                                           new Weibull(1.0, 1.0),
                                           new LogNormal(1.0, 1.0),
                                           new StudentT(0.0, 1.0, 5.0)
                                       };
        }

        /// <summary>
        /// Validate that univariate distributions have random source.
        /// </summary>
        [Test]
        public void ValidateThatUnivariateDistributionsHaveRandomSource()
        {
            foreach (var dd in _discreteDistributions)
            {
                Assert.IsNotNull(dd.RandomSource);
            }

            foreach (var cd in _continuousDistributions)
            {
                Assert.IsNotNull(cd.RandomSource);
            }
        }

        /// <summary>
        /// Can set random source.
        /// </summary>
        [Test]
        public void CanSetRandomSource()
        {
            foreach (var dd in _discreteDistributions)
            {
                dd.RandomSource = new Random();
            }

            foreach (var cd in _continuousDistributions)
            {
                cd.RandomSource = new Random();
            }
        }

        /// <summary>
        /// Fail set random source with <c>null</c> reference.
        /// </summary>
        [Test]
        public void FailSetRandomSourceWithNullReference()
        {
            foreach (var dd in _discreteDistributions)
            {
                var dd1 = dd;
                Assert.Throws<ArgumentNullException>(() => dd1.RandomSource = null);
            }

            foreach (var cd in _continuousDistributions)
            {
                var cd1 = cd;
                Assert.Throws<ArgumentNullException>(() => cd1.RandomSource = null);
            }
        }

        /// <summary>
        /// Test the method which samples only one variable at a time.
        /// </summary>
        [Test]
        public void SampleFollowsCorrectDistribution()
        {
            Random rnd = new MersenneTwister(1);

            foreach (var dd in _discreteDistributions)
            {
                dd.RandomSource = rnd;
                var samples = new double[NumberOfTestSamples];
                for (var i = 0; i < NumberOfTestSamples; i++)
                {
                    samples[i] = dd.Sample();
                }

                VapnikChervonenkisTest(Error, ErrorProbability, samples, dd);
            }

            foreach (var cd in _continuousDistributions)
            {
                cd.RandomSource = rnd;
                var samples = new double[NumberOfTestSamples];
                for (var i = 0; i < NumberOfTestSamples; i++)
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
        public void SamplesFollowsCorrectDistribution()
        {
            Random rnd = new MersenneTwister(1);

            foreach (var dd in _discreteDistributions)
            {
                dd.RandomSource = rnd;
                VapnikChervonenkisTest(Error, ErrorProbability, dd.Samples().Select(x => (double)x).Take(NumberOfTestSamples), dd);
            }

            foreach (var cd in _continuousDistributions)
            {
                cd.RandomSource = rnd;
                VapnikChervonenkisTest(Error, ErrorProbability, cd.Samples().Take(NumberOfTestSamples), cd);
            }
        }

        /// <summary>
        /// Vapnik Chervonenkis test.
        /// </summary>
        /// <param name="epsilon">The error we are willing to tolerate.</param>
        /// <param name="delta">The error probability we are willing to tolerate.</param>
        /// <param name="s">The samples to use for testing.</param>
        /// <param name="dist">The distribution we are testing.</param>
        public static void VapnikChervonenkisTest(double epsilon, double delta, IEnumerable<double> s, IDistribution dist)
        {
            // Using VC-dimension, we can bound the probability of making an error when estimating empirical probability
            // distributions. We are using Theorem 2.41 in "All Of Nonparametric Statistics". 
            // http://books.google.com/books?id=MRFlzQfRg7UC&lpg=PP1&dq=all%20of%20nonparametric%20statistics&pg=PA22#v=onepage&q=%22shatter%20coe%EF%AC%83cients%20do%20not%22&f=false .</para>
            // For intervals on the real line the VC-dimension is 2.
            double n = s.Count();
            Assert.Greater(n, Math.Ceiling(32.0 * Math.Log(16.0 / delta) / epsilon / epsilon));

            var histogram = new Histogram(s, NumberOfBuckets);
            for (var i = 0; i < NumberOfBuckets; i++)
            {
                var p = dist.CumulativeDistribution(histogram[i].UpperBound) - dist.CumulativeDistribution(histogram[i].LowerBound);
                var pe = histogram[i].Count / n;
                Assert.Less(Math.Abs(p - pe), epsilon, dist.ToString());
            }
        }
    }
}
