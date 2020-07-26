// <copyright file="Bernoulli.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Random;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Discrete Univariate Bernoulli distribution.
    /// The Bernoulli distribution is a distribution over bits. The parameter
    /// p specifies the probability that a 1 is generated.
    /// <a href="http://en.wikipedia.org/wiki/Bernoulli_distribution">Wikipedia - Bernoulli distribution</a>.
    /// </summary>
    public class Bernoulli : IDiscreteDistribution
    {
        System.Random _random;

        readonly double _p;

        /// <summary>
        /// Initializes a new instance of the Bernoulli class.
        /// </summary>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the Bernoulli parameter is not in the range [0,1].</exception>
        public Bernoulli(double p)
        {
            if (!IsValidParameterSet(p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _p = p;
        }

        /// <summary>
        /// Initializes a new instance of the Bernoulli class.
        /// </summary>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the Bernoulli parameter is not in the range [0,1].</exception>
        public Bernoulli(double p, System.Random randomSource)
        {
            if (!IsValidParameterSet(p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _p = p;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Bernoulli(p = {_p})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        public static bool IsValidParameterSet(double p)
        {
            return p >= 0.0 && p <= 1.0;
        }

        /// <summary>
        /// Gets the probability of generating a one. Range: 0 ≤ p ≤ 1.
        /// </summary>
        public double P => _p;

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
        public double Mean => _p;

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(_p*(1.0 - _p));

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => _p*(1.0 - _p);

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy => -(_p*Math.Log(_p)) - ((1.0 - _p)*Math.Log(1.0 - _p));

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => (1.0 - (2.0*_p))/Math.Sqrt(_p*(1.0 - _p));

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum => 0;

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum => 1;

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode => _p > 0.5 ? 1 : 0;

        /// <summary>
        /// Gets all modes of the distribution.
        /// </summary>
        public int[] Modes
        {
            get { return _p < 0.5 ? new[] { 0 } : P > 0.5 ? new[] { 1 } : new[] { 0, 1 }; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => _p < 0.5 ? 0.0 : _p > 0.5 ? 1.0 : 0.5;

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            if (k == 0)
            {
                return 1.0 - _p;
            }

            if (k == 1)
            {
                return _p;
            }

            return 0.0;
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            if (k == 0)
            {
                return Math.Log(1.0 - _p);
            }

            return k == 1 ? Math.Log(_p) : double.NegativeInfinity;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < 0.0)
            {
                return 0.0;
            }

            if (x >= 1.0)
            {
                return 1.0;
            }

            return 1.0 - _p;
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public static double PMF(double p, int k)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (k == 0)
            {
                return 1.0 - p;
            }

            if (k == 1)
            {
                return p;
            }

            return 0.0;
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public static double PMFLn(double p, int k)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (k == 0)
            {
                return Math.Log(1.0 - p);
            }

            return k == 1 ? Math.Log(p) : double.NegativeInfinity;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double p, double x)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (x < 0.0)
            {
                return 0.0;
            }

            if (x >= 1.0)
            {
                return 1.0;
            }

            return 1.0 - p;
        }

        /// <summary>
        /// Generates one sample from the Bernoulli distribution.
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>A random sample from the Bernoulli distribution.</returns>
        static int SampleUnchecked(System.Random rnd, double p)
        {
            if (rnd.NextDouble() < p)
            {
                return 1;
            }

            return 0;
        }

        static void SamplesUnchecked(System.Random rnd, int[] values, double p)
        {
            var uniform = rnd.NextDoubles(values.Length);
            CommonParallel.For(0, values.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    values[i] = uniform[i] < p ? 1 : 0;
                }
            });
        }

        static IEnumerable<int> SamplesUnchecked(System.Random rnd, double p)
        {
            return rnd.NextDoubleSequence().Select(r => r < p ? 1 : 0);
        }

        /// <summary>
        /// Samples a Bernoulli distributed random variable.
        /// </summary>
        /// <returns>A sample from the Bernoulli distribution.</returns>
        public int Sample()
        {
            return SampleUnchecked(_random, _p);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(int[] values)
        {
            SamplesUnchecked(_random, values, _p);
        }

        /// <summary>
        /// Samples an array of Bernoulli distributed random variables.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(_random, _p);
            }
        }

        /// <summary>
        /// Samples a Bernoulli distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>A sample from the Bernoulli distribution.</returns>
        public static int Sample(System.Random rnd, double p)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, p);
        }

        /// <summary>
        /// Samples a sequence of Bernoulli distributed random variables.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<int> Samples(System.Random rnd, double p)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, p);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, int[] values, double p)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, p);
        }

        /// <summary>
        /// Samples a Bernoulli distributed random variable.
        /// </summary>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>A sample from the Bernoulli distribution.</returns>
        public static int Sample(double p)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, p);
        }

        /// <summary>
        /// Samples a sequence of Bernoulli distributed random variables.
        /// </summary>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<int> Samples(double p)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, p);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="p">The probability (p) of generating one. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(int[] values, double p)
        {
            if (!(p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, p);
        }
    }
}
