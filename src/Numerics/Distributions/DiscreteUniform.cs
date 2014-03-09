// <copyright file="DiscreteUniform.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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
using MathNet.Numerics.Properties;
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

        int _lower;
        int _upper;

        /// <summary>
        /// Initializes a new instance of the DiscreteUniform class.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        public DiscreteUniform(int lower, int upper)
        {
            _random = SystemRandomSource.Default;
            SetParameters(lower, upper);
        }

        /// <summary>
        /// Initializes a new instance of the DiscreteUniform class.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public DiscreteUniform(int lower, int upper, System.Random randomSource)
        {
            _random = randomSource ?? SystemRandomSource.Default;
            SetParameters(lower, upper);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "DiscreteUniform(Lower = " + _lower + ", Upper = " + _upper + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(int lower, int upper)
        {
            return lower <= upper;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(int lower, int upper)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lower, upper))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _lower = lower;
            _upper = upper;
        }

        /// <summary>
        /// Gets or sets the lower bound of the probability distribution.
        /// </summary>
        public int LowerBound
        {
            get { return _lower; }
            set { SetParameters(value, _upper); }
        }

        /// <summary>
        /// Gets or sets the upper bound of the probability distribution.
        /// </summary>
        public int UpperBound
        {
            get { return _upper; }
            set { SetParameters(_lower, value); }
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get { return _random; }
            set { _random = value ?? SystemRandomSource.Default; }
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean
        {
            get { return (_lower + _upper)/2.0; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt((((_upper - _lower + 1.0)*(_upper - _lower + 1.0)) - 1.0)/12.0); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return (((_upper - _lower + 1.0)*(_upper - _lower + 1.0)) - 1.0)/12.0; }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return Math.Log(_upper - _lower + 1.0); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum
        {
            get { return _lower; }
        }

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum
        {
            get { return _upper; }
        }

        /// <summary>
        /// Gets the mode of the distribution; since every element in the domain has the same probability this method returns the middle one.
        /// </summary>
        public int Mode
        {
            get { return (int) Math.Floor((_lower + _upper)/2.0); }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public int Median
        {
            get { return (int) Math.Floor((_lower + _upper)/2.0); }
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            if (k >= _lower && k <= _upper)
            {
                return 1.0/(_upper - _lower + 1);
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
            if (k >= _lower && k <= _upper)
            {
                return -Math.Log(_upper - _lower + 1);
            }

            return Double.NegativeInfinity;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < _lower)
            {
                return 0.0;
            }

            if (x >= _upper)
            {
                return 1.0;
            }

            return Math.Min(1.0, (Math.Floor(x) - _lower + 1)/(_upper - _lower + 1));
        }

        /// <summary>
        /// Generates one sample from the discrete uniform distribution. This method does not do any parameter checking.
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>A random sample from the discrete uniform distribution.</returns>
        static int SampleUnchecked(System.Random rnd, int lower, int upper)
        {
            return (rnd.Next()%(upper - lower + 1)) + lower;
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
        /// Samples an array of uniformly distributed random variables.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(_random, _lower, _upper);
            }
        }

        /// <summary>
        /// Samples a uniformly distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>A sample from the discrete uniform distribution.</returns>
        public static int Sample(System.Random rnd, int lower, int upper)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lower, upper))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, lower, upper);
        }

        /// <summary>
        /// Samples a sequence of uniformly distributed random variables.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>a sequence of samples from the discrete uniform distribution.</returns>
        public static IEnumerable<int> Samples(System.Random rnd, int lower, int upper)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lower, upper))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, lower, upper);
            }
        }

        /// <summary>
        /// Samples a uniformly distributed random variable.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>A sample from the discrete uniform distribution.</returns>
        public static int Sample(int lower, int upper)
        {
            return Sample(SystemRandomSource.Default, lower, upper);
        }

        /// <summary>
        /// Samples a sequence of uniformly distributed random variables.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>a sequence of samples from the discrete uniform distribution.</returns>
        public static IEnumerable<int> Samples(int lower, int upper)
        {
            return Samples(SystemRandomSource.Default, lower, upper);
        }
    }
}
