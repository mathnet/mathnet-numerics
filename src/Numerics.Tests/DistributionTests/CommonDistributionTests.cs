// <copyright file="CommonDistributionTests.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DistributionTests
{
    /// <summary>
    /// This class will perform various tests on discrete and continuous univariate distributions.
    /// The multivariate distributions will implement these respective tests in their local unit
    /// test classes as they do not adhere to the same interfaces.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class CommonDistributionTests
    {
        public const int NumberOfTestSamples = 3500000;
        public const int NumberOfHistogramBuckets = 100;
        public const double ErrorTolerance = 0.01;
        public const double ErrorProbability = 0.001;

        readonly List<IDiscreteDistribution> _discreteDistributions =
            new List<IDiscreteDistribution>
            {
                new Bernoulli(0.6),
                new Binomial(0.7, 10),
                new Categorical(new[] { 0.7, 0.3 }),
                //new ConwayMaxwellPoisson(0.2, 0.4),
                new DiscreteUniform(1, 10),
                new Geometric(0.2),
                new Hypergeometric(20, 3, 5),
                //new NegativeBinomial(4, 0.6),
                //new Poisson(0.4),
                new Zipf(3.0, 10),
            };

        readonly List<IContinuousDistribution> _continuousDistributions =
            new List<IContinuousDistribution>
            {
                new Beta(1.0, 1.0),
                new BetaScaled(1.0, 1.5, 0.5, 2.0),
                new Cauchy(1.0, 1.0),
                new Chi(3.0),
                new ChiSquared(3.0),
                new ContinuousUniform(0.0, 1.0),
                new Erlang(3, 0.4),
                new Exponential(0.4),
                new FisherSnedecor(0.3, 0.4),
                new Gamma(1.0, 1.0),
                new InverseGamma(1.0, 1.0),
                new Laplace(1.0, 0.5),
                new LogNormal(1.0, 1.0),
                new Normal(0.0, 1.0),
                new Pareto(1.0, 0.5),
                new Rayleigh(0.8),
                new Stable(0.5, 1.0, 0.5, 1.0),
                new StudentT(0.0, 1.0, 5.0),
                new Triangular(0, 1, 0.7),
                new Weibull(1.0, 1.0),
            };

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

        [Test]
        public void CanSetRandomSource()
        {
            foreach (var dd in _discreteDistributions)
            {
                dd.RandomSource = MersenneTwister.Default;
            }

            foreach (var cd in _continuousDistributions)
            {
                cd.RandomSource = MersenneTwister.Default;
            }
        }

        [Test]
        public void HasRandomSourceEvenAfterSetToNull()
        {
            foreach (var dd in _discreteDistributions)
            {
                Assert.DoesNotThrow(() => dd.RandomSource = null);
                Assert.IsNotNull(dd.RandomSource);
            }

            foreach (var cd in _continuousDistributions)
            {
                Assert.DoesNotThrow(() => cd.RandomSource = null);
                Assert.IsNotNull(cd.RandomSource);
            }
        }

        [Test, Category("LongRunning")]
        public void DiscreteSampleIsDistributedCorrectly()
        {
            foreach (var dist in _discreteDistributions)
            {
                dist.RandomSource = new SystemRandomSource(1, false);
                var samples = new int[NumberOfTestSamples];
                for (var i = 0; i < NumberOfTestSamples; i++)
                {
                    samples[i] = dist.Sample();
                }
                DiscreteVapnikChervonenkisTest(ErrorTolerance, ErrorProbability, samples, dist);
            }
        }

        [Test, Category("LongRunning")]
        public void DiscreteSampleSequenceIsDistributedCorrectly()
        {
            foreach (var dist in _discreteDistributions)
            {
                dist.RandomSource = new SystemRandomSource(1, false);
                var samples = dist.Samples().Take(NumberOfTestSamples).ToArray();
                DiscreteVapnikChervonenkisTest(ErrorTolerance, ErrorProbability, samples, dist);
            }
        }

        [Test, Category("LongRunning")]
        public void ContinuousSampleIsDistributedCorrectly()
        {
            foreach (var dist in _continuousDistributions)
            {
                dist.RandomSource = new SystemRandomSource(1, false);
                var samples = new double[NumberOfTestSamples];
                for (var i = 0; i < NumberOfTestSamples; i++)
                {
                    samples[i] = dist.Sample();
                }
                ContinuousVapnikChervonenkisTest(ErrorTolerance, ErrorProbability, samples, dist);
            }
        }

        [Test, Category("LongRunning")]
        public void ContinuousSampleSequenceIsDistributedCorrectly()
        {
            foreach (var dist in _continuousDistributions)
            {
                dist.RandomSource = new SystemRandomSource(1, false);
                var samples = dist.Samples().Take(NumberOfTestSamples).ToArray();
                ContinuousVapnikChervonenkisTest(ErrorTolerance, ErrorProbability, samples, dist);
            }
        }

        /// <summary>
        /// Vapnik Chervonenkis test.
        /// </summary>
        /// <param name="epsilon">The error we are willing to tolerate.</param>
        /// <param name="delta">The error probability we are willing to tolerate.</param>
        /// <param name="s">The samples to use for testing.</param>
        /// <param name="dist">The distribution we are testing.</param>
        public static void ContinuousVapnikChervonenkisTest(double epsilon, double delta, double[] s, IContinuousDistribution dist)
        {
            // Using VC-dimension, we can bound the probability of making an error when estimating empirical probability
            // distributions. We are using Theorem 2.41 in "All Of Nonparametric Statistics".
            // http://books.google.com/books?id=MRFlzQfRg7UC&lpg=PP1&dq=all%20of%20nonparametric%20statistics&pg=PA22#v=onepage&q=%22shatter%20coe%EF%AC%83cients%20do%20not%22&f=false .</para>
            // For intervals on the real line the VC-dimension is 2.
            Assert.Greater(s.Length, Math.Ceiling(32.0 * Math.Log(16.0 / delta) / epsilon / epsilon));

            var histogram = new Histogram(s, NumberOfHistogramBuckets);
            for (var i = 0; i < NumberOfHistogramBuckets; i++)
            {
                var p = dist.CumulativeDistribution(histogram[i].UpperBound) - dist.CumulativeDistribution(histogram[i].LowerBound);
                var pe = histogram[i].Count/(double)s.Length;
                Assert.Less(Math.Abs(p - pe), epsilon, dist.ToString());
            }
        }

        /// <summary>
        /// Vapnik Chervonenkis test.
        /// </summary>
        /// <param name="epsilon">The error we are willing to tolerate.</param>
        /// <param name="delta">The error probability we are willing to tolerate.</param>
        /// <param name="s">The samples to use for testing.</param>
        /// <param name="dist">The distribution we are testing.</param>
        public static void DiscreteVapnikChervonenkisTest(double epsilon, double delta, int[] s, IDiscreteDistribution dist)
        {
            // Using VC-dimension, we can bound the probability of making an error when estimating empirical probability
            // distributions. We are using Theorem 2.41 in "All Of Nonparametric Statistics".
            // http://books.google.com/books?id=MRFlzQfRg7UC&lpg=PP1&dq=all%20of%20nonparametric%20statistics&pg=PA22#v=onepage&q=%22shatter%20coe%EF%AC%83cients%20do%20not%22&f=false .</para>
            // For intervals on the real line the VC-dimension is 2.
            Assert.Greater(s.Length, Math.Ceiling(32.0 * Math.Log(16.0 / delta) / epsilon / epsilon));

            var min = s.Min();
            var max = s.Max();

            var histogram = new int[max - min + 1];
            for (int i = 0; i < s.Length; i++)
            {
                histogram[s[i] - min]++;
            }

            for (int i = 0; i < histogram.Length; i++)
            {
                var p = dist.CumulativeDistribution(i + min) - dist.CumulativeDistribution(i + min - 1.0);
                var pe = histogram[i]/(double)s.Length;
                Assert.Less(Math.Abs(p - pe), epsilon, dist.ToString());
            }
        }
    }
}
