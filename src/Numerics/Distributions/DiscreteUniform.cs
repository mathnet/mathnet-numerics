// <copyright file="DiscreteUniform.cs" company="Math.NET">
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
    /// Discrete Univariate Uniform distribution.
    /// The discrete uniform distribution is a distribution over integers. The distribution
    /// is parameterized by a lower and upper bound (both inclusive).
    /// <a href="http://en.wikipedia.org/wiki/Uniform_distribution_%28discrete%29">Wikipedia - Discrete uniform distribution</a>.
    /// </summary>
    public class DiscreteUniform : IDiscreteDistribution
    {
        System.Random _random;

        readonly int _lower;
        readonly int _upper;

        /// <summary>
        /// Initializes a new instance of the DiscreteUniform class.
        /// </summary>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        public DiscreteUniform(int lower, int upper)
        {
            if (!IsValidParameterSet(lower, upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _lower = lower;
            _upper = upper;
        }

        /// <summary>
        /// Initializes a new instance of the DiscreteUniform class.
        /// </summary>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public DiscreteUniform(int lower, int upper, System.Random randomSource)
        {
            if (!IsValidParameterSet(lower, upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _lower = lower;
            _upper = upper;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"DiscreteUniform(Lower = {_lower}, Upper = {_upper})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        public static bool IsValidParameterSet(int lower, int upper)
        {
            return lower <= upper;
        }

        /// <summary>
        /// Gets the inclusive lower bound of the probability distribution.
        /// </summary>
        public int LowerBound => _lower;

        /// <summary>
        /// Gets the inclusive upper bound of the probability distribution.
        /// </summary>
        public int UpperBound => _upper;

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
        public double Mean => (_lower + _upper)/2.0;

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt((((_upper - _lower + 1.0)*(_upper - _lower + 1.0)) - 1.0)/12.0);

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => (((_upper - _lower + 1.0)*(_upper - _lower + 1.0)) - 1.0)/12.0;

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy => Math.Log(_upper - _lower + 1.0);

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => 0.0;

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum => _lower;

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum => _upper;

        /// <summary>
        /// Gets the mode of the distribution; since every element in the domain has the same probability this method returns the middle one.
        /// </summary>
        public int Mode => (int)Math.Floor((_lower + _upper)/2.0);

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => (_lower + _upper)/2.0;

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            return PMF(_lower, _upper, k);
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            return PMFLn(_lower, _upper, k);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return CDF(_lower, _upper, x);
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public static double PMF(int lower, int upper, int k)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return k >= lower && k <= upper ? 1.0/(upper - lower + 1) : 0.0;
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public static double PMFLn(int lower, int upper, int k)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return k >= lower && k <= upper ? -Math.Log(upper - lower + 1) : double.NegativeInfinity;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(int lower, int upper, double x)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (x < lower)
            {
                return 0.0;
            }

            if (x >= upper)
            {
                return 1.0;
            }

            return Math.Min(1.0, (Math.Floor(x) - lower + 1)/(upper - lower + 1));
        }

        /// <summary>
        /// Generates one sample from the discrete uniform distribution. This method does not do any parameter checking.
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>A random sample from the discrete uniform distribution.</returns>
        static int SampleUnchecked(System.Random rnd, int lower, int upper)
        {
            return rnd.Next(lower, upper + 1);
        }

        static void SamplesUnchecked(System.Random rnd, int[] values, int lower, int upper)
        {
            rnd.NextInt32s(values, lower, upper + 1);
        }

        static IEnumerable<int> SamplesUnchecked(System.Random rnd, int lower, int upper)
        {
            return rnd.NextInt32Sequence(lower, upper + 1);
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public int Sample()
        {
            return SampleUnchecked(_random, _lower, _upper);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(int[] values)
        {
            SamplesUnchecked(_random, values, _lower, _upper);
        }

        /// <summary>
        /// Samples an array of uniformly distributed random variables.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples()
        {
            return SamplesUnchecked(_random, _lower, _upper);
        }

        /// <summary>
        /// Samples a uniformly distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>A sample from the discrete uniform distribution.</returns>
        public static int Sample(System.Random rnd, int lower, int upper)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, lower, upper);
        }

        /// <summary>
        /// Samples a sequence of uniformly distributed random variables.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>a sequence of samples from the discrete uniform distribution.</returns>
        public static IEnumerable<int> Samples(System.Random rnd, int lower, int upper)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, lower, upper);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>a sequence of samples from the discrete uniform distribution.</returns>
        public static void Samples(System.Random rnd, int[] values, int lower, int upper)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, lower, upper);
        }

        /// <summary>
        /// Samples a uniformly distributed random variable.
        /// </summary>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>A sample from the discrete uniform distribution.</returns>
        public static int Sample(int lower, int upper)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, lower, upper);
        }

        /// <summary>
        /// Samples a sequence of uniformly distributed random variables.
        /// </summary>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>a sequence of samples from the discrete uniform distribution.</returns>
        public static IEnumerable<int> Samples(int lower, int upper)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, lower, upper);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="lower">Lower bound, inclusive. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound, inclusive. Range: lower ≤ upper.</param>
        /// <returns>a sequence of samples from the discrete uniform distribution.</returns>
        public static void Samples(int[] values, int lower, int upper)
        {
            if (!(lower <= upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, lower, upper);
        }
    }
}
