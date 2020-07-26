// <copyright file="Hypergeometric.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Discrete Univariate Hypergeometric distribution.
    /// This distribution is a discrete probability distribution that describes the number of successes in a sequence
    /// of n draws from a finite population without replacement, just as the binomial distribution
    /// describes the number of successes for draws with replacement
    /// <a href="http://en.wikipedia.org/wiki/Hypergeometric_distribution">Wikipedia - Hypergeometric distribution</a>.
    /// </summary>
    public class Hypergeometric : IDiscreteDistribution
    {
        System.Random _random;

        readonly int _population;
        readonly int _success;
        readonly int _draws;

        /// <summary>
        /// Initializes a new instance of the Hypergeometric class.
        /// </summary>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        public Hypergeometric(int population, int success, int draws)
        {
            if (!IsValidParameterSet(population, success, draws))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _population = population;
            _success = success;
            _draws = draws;
        }

        /// <summary>
        /// Initializes a new instance of the Hypergeometric class.
        /// </summary>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Hypergeometric(int population, int success, int draws, System.Random randomSource)
        {
            if (!IsValidParameterSet(population, success, draws))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _population = population;
            _success = success;
            _draws = draws;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Hypergeometric(N = {_population}, M = {_success}, n = {_draws})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        public static bool IsValidParameterSet(int population, int success, int draws)
        {
            return population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population;
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the size of the population (N).
        /// </summary>
        public int Population => _population;

        /// <summary>
        /// Gets the number of draws without replacement (n).
        /// </summary>
        public int Draws => _draws;

        /// <summary>
        /// Gets the number successes within the population (K, M).
        /// </summary>
        public int Success => _success;

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean => (double)_success*_draws/_population;

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => _draws*_success*(_population - _draws)*(_population - _success)/(_population*_population*(_population - 1.0));

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(Variance);

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy => throw new NotSupportedException();

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => (Math.Sqrt(_population - 1.0)*(_population - (2*_draws))*(_population - (2*_success)))/(Math.Sqrt(_draws*_success*(_population - _success)*(_population - _draws))*(_population - 2.0));

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode => (_draws + 1)*(_success + 1)/(_population + 2);

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => throw new NotSupportedException();

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public int Minimum => Math.Max(0, _draws + _success - _population);

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public int Maximum => Math.Min(_success, _draws);

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            return SpecialFunctions.Binomial(_success, k)*SpecialFunctions.Binomial(_population - _success, _draws - k)/SpecialFunctions.Binomial(_population, _draws);
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            return SpecialFunctions.BinomialLn(_success, k) + SpecialFunctions.BinomialLn(_population - _success, _draws - k) - SpecialFunctions.BinomialLn(_population, _draws);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return CDF(_population, _success, _draws, x);
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public static double PMF(int population, int success, int draws, int k)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SpecialFunctions.Binomial(success, k)*SpecialFunctions.Binomial(population - success, draws - k)/SpecialFunctions.Binomial(population, draws);
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public static double PMFLn(int population, int success, int draws, int k)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SpecialFunctions.BinomialLn(success, k) + SpecialFunctions.BinomialLn(population - success, draws - k) - SpecialFunctions.BinomialLn(population, draws);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(int population, int success, int draws, double x)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (x < Math.Max(0, draws + success - population))
            {
                return 0.0;
            }

            if (x >= Math.Min(success, draws))
            {
                return 1.0;
            }

            var k = (int)Math.Floor(x);
            var denominatorLn = SpecialFunctions.BinomialLn(population, draws);
            var sum = 0.0;
            for (var i = 0; i <= k; i++)
            {
                sum += Math.Exp(SpecialFunctions.BinomialLn(success, i) + SpecialFunctions.BinomialLn(population - success, draws - i) - denominatorLn);
            }

            return Math.Min(sum, 1.0);
        }

        /// <summary>
        /// Generates a sample from the Hypergeometric distribution without doing parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The n parameter of the distribution.</param>
        /// <returns>a random number from the Hypergeometric distribution.</returns>
        static int SampleUnchecked(System.Random rnd, int population, int success, int draws)
        {
            var x = 0;

            do
            {
                var p = (double)success/population;
                var r = rnd.NextDouble();
                if (r < p)
                {
                    x++;
                    success--;
                }

                population--;
                draws--;
            }
            while (0 < draws);

            return x;
        }

        static void SamplesUnchecked(System.Random rnd, int[] values, int population, int success, int draws)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = SampleUnchecked(rnd, population, success, draws);
            }
        }

        static IEnumerable<int> SamplesUnchecked(System.Random rnd, int population, int success, int draws)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, population, success, draws);
            }
        }

        /// <summary>
        /// Samples a Hypergeometric distributed random variable.
        /// </summary>
        /// <returns>The number of successes in n trials.</returns>
        public int Sample()
        {
            return SampleUnchecked(_random, _population, _success, _draws);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(int[] values)
        {
            SamplesUnchecked(_random, values, _population, _success, _draws);
        }

        /// <summary>
        /// Samples an array of Hypergeometric distributed random variables.
        /// </summary>
        /// <returns>a sequence of successes in n trials.</returns>
        public IEnumerable<int> Samples()
        {
            return SamplesUnchecked(_random, _population, _success, _draws);
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        public static int Sample(System.Random rnd, int population, int success, int draws)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, population, success, draws);
        }

        /// <summary>
        /// Samples a sequence of this random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        public static IEnumerable<int> Samples(System.Random rnd, int population, int success, int draws)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, population, success, draws);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        public static void Samples(System.Random rnd, int[] values, int population, int success, int draws)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, population, success, draws);
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        public static int Sample(int population, int success, int draws)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, population, success, draws);
        }

        /// <summary>
        /// Samples a sequence of this random variable.
        /// </summary>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        public static IEnumerable<int> Samples(int population, int success, int draws)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, population, success, draws);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="population">The size of the population (N).</param>
        /// <param name="success">The number successes within the population (K, M).</param>
        /// <param name="draws">The number of draws without replacement (n).</param>
        public static void Samples(int[] values, int population, int success, int draws)
        {
            if (!(population >= 0 && success >= 0 && draws >= 0 && success <= population && draws <= population))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, population, success, draws);
        }
    }
}
