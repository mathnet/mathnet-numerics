// <copyright file="Binomial.cs" company="Math.NET">
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
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Discrete Univariate Binomial distribution.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Binomial_distribution">Wikipedia - Binomial distribution</a>.
    /// </summary>
    /// <remarks>
    /// The distribution is parameterized by a probability (between 0.0 and 1.0).
    /// </remarks>
    public class Binomial : IDiscreteDistribution
    {
        System.Random _random;

        readonly double _p;
        readonly int _trials;

        /// <summary>
        /// Initializes a new instance of the Binomial class.
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="p"/> is not in the interval [0.0,1.0].</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        public Binomial(double p, int n)
        {
            if (!IsValidParameterSet(p, n))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _p = p;
            _trials = n;
        }

        /// <summary>
        /// Initializes a new instance of the Binomial class.
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="p"/> is not in the interval [0.0,1.0].</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        public Binomial(double p, int n, System.Random randomSource)
        {
            if (!IsValidParameterSet(p, n))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _p = p;
            _trials = n;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Binomial(p = {_p}, n = {_trials})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        public static bool IsValidParameterSet(double p, int n)
        {
            return p >= 0.0 && p <= 1.0 && n >= 0;
        }

        /// <summary>
        /// Gets the success probability in each trial. Range: 0 ≤ p ≤ 1.
        /// </summary>
        public double P => _p;

        /// <summary>
        /// Gets the number of trials. Range: n ≥ 0.
        /// </summary>
        public int N => _trials;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean => _p*_trials;

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(_p*(1.0 - _p)*_trials);

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => _p*(1.0 - _p)*_trials;

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                if (_p == 0.0 || _p == 1.0)
                {
                    return 0.0;
                }

                var e = 0.0;
                for (var i = 0; i <= _trials; i++)
                {
                    var p = Probability(i);
                    e -= p*Math.Log(p);
                }

                return e;
            }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => (1.0 - (2.0*_p))/Math.Sqrt(_trials*_p*(1.0 - _p));

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum => 0;

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum => _trials;

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode
        {
            get
            {
                if (_p == 1.0)
                {
                    return _trials;
                }

                if (_p == 0.0)
                {
                    return 0;
                }

                return (int)Math.Floor((_trials + 1)*_p);
            }
        }

        /// <summary>
        /// Gets all modes of the distribution.
        /// </summary>
        public int[] Modes
        {
            get
            {
                if (_p == 1.0)
                {
                    return new[] { _trials };
                }

                if (_p == 0.0)
                {
                    return new[] { 0 };
                }

                double td = (_trials + 1)*_p;
                int t = (int)Math.Floor(td);
                return t != td ? new[] { t } : new[] { t, t - 1 };
            }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => Math.Floor(_p*_trials);

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            return PMF(_p, _trials, k);
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            return PMFLn(_p, _trials, k);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return CDF(_p, _trials, x);
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public static double PMF(double p, int n, int k)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (k < 0 || k > n)
            {
                return 0.0;
            }

            if (p == 0.0)
            {
                return k == 0 ? 1.0 : 0.0;
            }

            if (p == 1.0)
            {
                return k == n ? 1.0 : 0.0;
            }

            return Math.Exp(SpecialFunctions.BinomialLn(n, k) + (k*Math.Log(p)) + ((n - k)*Math.Log(1.0 - p)));
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public static double PMFLn(double p, int n, int k)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (k < 0 || k > n)
            {
                return double.NegativeInfinity;
            }

            if (p == 0.0)
            {
                return k == 0 ? 0.0 : double.NegativeInfinity;
            }

            if (p == 1.0)
            {
                return k == n ? 0.0 : double.NegativeInfinity;
            }

            return SpecialFunctions.BinomialLn(n, k) + (k*Math.Log(p)) + ((n - k)*Math.Log(1.0 - p));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double p, int n, double x)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (x < 0.0)
            {
                return 0.0;
            }

            if (x > n)
            {
                return 1.0;
            }

            double k = Math.Floor(x);
            return SpecialFunctions.BetaRegularized(n - k, k + 1, 1 - p);
        }

        /// <summary>
        /// Generates a sample from the Binomial distribution without doing parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>The number of successful trials.</returns>
        internal static int SampleUnchecked(System.Random rnd, double p, int n)
        {
            var k = 0;
            for (var i = 0; i < n; i++)
            {
                k += rnd.NextDouble() < p ? 1 : 0;
            }

            return k;
        }

        static void SamplesUnchecked(System.Random rnd, int[] values, double p, int n)
        {
            var uniform = rnd.NextDoubles(values.Length*n);
            CommonParallel.For(0, values.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    int k = i*n;
                    int sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        sum += uniform[k + j] < p ? 1 : 0;
                    }

                    values[i] = sum;
                }
            });
        }

        static IEnumerable<int> SamplesUnchecked(System.Random rnd, double p, int n)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, p, n);
            }
        }

        /// <summary>
        /// Samples a Binomially distributed random variable.
        /// </summary>
        /// <returns>The number of successes in N trials.</returns>
        public int Sample()
        {
            return SampleUnchecked(_random, _p, _trials);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(int[] values)
        {
            SamplesUnchecked(_random, values, _p, _trials);
        }

        /// <summary>
        /// Samples an array of Binomially distributed random variables.
        /// </summary>
        /// <returns>a sequence of successes in N trials.</returns>
        public IEnumerable<int> Samples()
        {
            return SamplesUnchecked(_random, _p, _trials);
        }

        /// <summary>
        /// Samples a binomially distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>The number of successes in <paramref name="n"/> trials.</returns>
        public static int Sample(System.Random rnd, double p, int n)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, p, n);
        }

        /// <summary>
        /// Samples a sequence of binomially distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>a sequence of successes in <paramref name="n"/> trials.</returns>
        public static IEnumerable<int> Samples(System.Random rnd, double p, int n)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, p, n);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>a sequence of successes in <paramref name="n"/> trials.</returns>
        public static void Samples(System.Random rnd, int[] values, double p, int n)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, p, n);
        }

        /// <summary>
        /// Samples a binomially distributed random variable.
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>The number of successes in <paramref name="n"/> trials.</returns>
        public static int Sample(double p, int n)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, p, n);
        }

        /// <summary>
        /// Samples a sequence of binomially distributed random variable.
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>a sequence of successes in <paramref name="n"/> trials.</returns>
        public static IEnumerable<int> Samples(double p, int n)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, p, n);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>a sequence of successes in <paramref name="n"/> trials.</returns>
        public static void Samples(int[] values, double p, int n)
        {
            if (!(p >= 0.0 && p <= 1.0 && n >= 0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, p, n);
        }
    }
}
