// <copyright file="Categorical.cs" company="Math.NET">
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

namespace MathNet.Numerics.Distributions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Properties;
    using Statistics;

    /// <summary>
    /// Implements the categorical distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Categorical_distribution">Wikipedia - Categorical distribution</a>. This
    /// distribution is sometimes called the Discrete distribution.
    /// </summary>
    /// <remarks><para>The distribution is parameterized by a vector of ratios: in other words, the parameter
    /// does not have to be normalized and sum to 1. The reason is that some vectors can't be exactly normalized
    /// to sum to 1 in floating point representation.</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Categorical : IDiscreteDistribution
    {
        Random _random;
        double[] _pmfNormalized;
        double[] _cdfUnnormalized;

        /// <summary>
        /// Initializes a new instance of the Categorical class.
        /// </summary>
        /// <param name="probabilityMass">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <exception cref="ArgumentException">If any of the probabilities are negative or do not sum to one.</exception>
        public Categorical(double[] probabilityMass)
        {
            SetParameters(probabilityMass);
            RandomSource = new Random();
        }

        /// <summary>
        /// Initializes a new instance of the Categorical class from a <paramref name="histogram"/>. The distribution 
        /// will not be automatically updated when the histogram changes. The categorical distribution will have
        /// one value for each bucket and a probability for that value proportional to the bucket count.
        /// </summary>
        /// <param name="histogram">The histogram from which to create the categorical variable.</param>
        public Categorical(Histogram histogram)
        {
            if (histogram == null)
            {
                throw new ArgumentNullException("histogram");
            }

            // The probability distribution vector.
            var p = new double[histogram.BucketCount];

            // Fill in the distribution vector.
            for (var i = 0; i < histogram.BucketCount; i++)
            {
                p[i] = histogram[i].Count;
            }

            SetParameters(p);
            RandomSource = new Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Categorical(Dimension = " + _pmfNormalized.Length + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized as this is often impossible using floating point arithmetic.</param>
        /// <returns>If any of the probabilities are negative returns <c>false</c>, or if the sum of parameters is 0.0; otherwise <c>true</c></returns>
        static bool IsValidProbabilityMass(double[] p)
        {
            var sum = 0.0;
            for (int i = 0; i < p.Length; i++)
            {
                double t = p[i];
                if (t < 0.0 || Double.IsNaN(t))
                {
                    return false;
                }

                sum += t;
            }

            return sum > 0.0;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="cdf">An array of nonnegative ratios: this array does not need to be normalized as this is often impossible using floating point arithmetic.</param>
        /// <returns>If any of the probabilities are negative returns <c>false</c>, or if the sum of parameters is 0.0; otherwise <c>true</c></returns>
        static bool IsValidCumulativeDistribution(double[] cdf)
        {
            var last = 0.0;
            for (int i = 0; i < cdf.Length; i++)
            {
                double t = cdf[i];
                if (t < 0.0 || Double.IsNaN(t) || t < last)
                {
                    return false;
                }

                last = t;
            }

            return last > 0.0;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidProbabilityMass"/> function.</exception>
        void SetParameters(double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidProbabilityMass(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // Extract unnormalized cumulative distribution
            _cdfUnnormalized = new double[p.Length];
            _cdfUnnormalized[0] = p[0];
            for (int i = 1; i < p.Length; i++)
            {
                _cdfUnnormalized[i] = _cdfUnnormalized[i - 1] + p[i];
            }

            // Extract normalized probability mass
            var sum = _cdfUnnormalized[_cdfUnnormalized.Length - 1];
            _pmfNormalized = new double[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                _pmfNormalized[i] = p[i]/sum;
            }

        }

        /// <summary>
        /// Gets or sets the normalized probability vector of the multinomial.
        /// </summary>
        /// <remarks>Sometimes the normalized probability vector cannot be represented
        /// exactly in a floating point representation.</remarks>
        public double[] P
        {
            get { return (double[])_pmfNormalized.Clone(); }
            set { SetParameters(value); }
        }

        #region IDistribution Members

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public Random RandomSource
        {
            get { return _random; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _random = value;
            }
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean
        {
            get { return _pmfNormalized.Mean(); }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return _pmfNormalized.StandardDeviation(); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _pmfNormalized.Variance(); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return _pmfNormalized.Sum(p => p * Math.Log(p)); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        /// <remarks>Throws a <see cref="NotSupportedException"/>.</remarks>
        public double Skewness
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum
        {
            get { return _pmfNormalized.Length - 1; }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the Binomial distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < 0.0)
            {
                return 0.0;
            }

            if (x >= _cdfUnnormalized.Length)
            {
                return 1.0;
            }

            return _cdfUnnormalized[(int) Math.Floor(x)]/_cdfUnnormalized[_cdfUnnormalized.Length - 1];
        }

        #endregion

        #region IDiscreteDistribution Members

        /// <summary>
        /// Gets he mode of the distribution.
        /// </summary>
        /// <remarks>Throws a <see cref="NotSupportedException"/>.</remarks>
        public int Mode
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public int Median
        {
            get { return (int)_pmfNormalized.Median(); }
        }

        /// <summary>
        /// Computes values of the probability mass function.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            if (k < 0)
            {
                return 0.0;
            }

            if (k >= _pmfNormalized.Length)
            {
                return 0.0;
            }

            return _pmfNormalized[k];
        }

        /// <summary>
        /// Computes values of the log probability mass function.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            if (k < 0)
            {
                return 0.0;
            }

            if (k >= _pmfNormalized.Length)
            {
                return 0.0;
            }

            return Math.Log(_pmfNormalized[k]);
        }

        #endregion

        /// <summary>
        /// Computes the cumulative distribution function. This method performs no parameter checking.
        /// If the probability mass was normalized, the resulting cumulative distribution is normalized as well (up to numerical errors).
        /// </summary>
        /// <param name="pmfUnnormalized">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>An array representing the unnormalized cumulative distribution function.</returns>
        internal static double[] ProbabilityMassToCumulativeDistribution(double[] pmfUnnormalized)
        {
            var cdfUnnormalized = new double[pmfUnnormalized.Length];
            cdfUnnormalized[0] = pmfUnnormalized[0];
            for (int i = 1; i < pmfUnnormalized.Length; i++)
            {
                cdfUnnormalized[i] = cdfUnnormalized[i - 1] + pmfUnnormalized[i];
            }

            return cdfUnnormalized;
        }

        /// <summary>
        /// Returns one trials from the categorical distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="cdfUnnormalized">The (unnormalized) cumulative distribution of the probability distribution.</param>
        /// <returns>One sample from the categorical distribution implied by <paramref name="cdfUnnormalized"/>.</returns>
        internal static int SampleUnchecked(Random rnd, double[] cdfUnnormalized)
        {
            // TODO : use binary search to speed up this procedure.
            var u = rnd.NextDouble() * cdfUnnormalized[cdfUnnormalized.Length - 1];

            var idx = 0;
            while (u > cdfUnnormalized[idx])
            {
                idx++;
            }

            return idx;
        }

        /// <summary>
        /// Samples a Binomially distributed random variable.
        /// </summary>
        /// <returns>The number of successful trials.</returns>
        public int Sample()
        {
            return SampleUnchecked(RandomSource, _cdfUnnormalized);
        }

        /// <summary>
        /// Samples an array of Bernoulli distributed random variables.
        /// </summary>
        /// <returns>a sequence of successful trial counts.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _cdfUnnormalized);
            }
        }

        /// <summary>
        /// Samples one categorical distributed random variable; also known as the Discrete distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="pmfUnnormalized">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>One random integer between 0 and the size of the categorical (exclusive).</returns>
        [Obsolete("Use SampleWithProbabilityMass instead (or SampleWithCumulativeDistribution which is faster). Scheduled for removal in v3.0.")]
        public static int Sample(Random rnd, double[] pmfUnnormalized)
        {
            return SampleWithProbabilityMass(rnd, pmfUnnormalized);
        }

        /// <summary>
        /// Samples one categorical distributed random variable; also known as the Discrete distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="cdfUnnormalized">An array of the cumulative distribution. Not assumed to be normalized.</param>
        /// <returns>One random integer between 0 and the size of the categorical (exclusive).</returns>
        public static int SampleWithCumulativeDistribution(Random rnd, double[] cdfUnnormalized)
        {
            if (Control.CheckDistributionParameters && !IsValidCumulativeDistribution(cdfUnnormalized))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, cdfUnnormalized);
        }

        /// <summary>
        /// Samples one categorical distributed random variable; also known as the Discrete distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="pmfUnnormalized">An array of nonnegative ratios. Not assumed to be normalized.</param>
        /// <returns>One random integer between 0 and the size of the categorical (exclusive).</returns>
        public static int SampleWithProbabilityMass(Random rnd, double[] pmfUnnormalized)
        {
            if (Control.CheckDistributionParameters && !IsValidProbabilityMass(pmfUnnormalized))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            var cdf = ProbabilityMassToCumulativeDistribution(pmfUnnormalized);
            return SampleUnchecked(rnd, cdf);
        }

        /// <summary>
        /// Samples a categorically distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>random integers between 0 and the size of the categorical (exclusive).</returns>
        [Obsolete("Use SamplesWithProbabilityMass instead (or SamplesWithCumulativeDistribution which is faster). Scheduled for removal in v3.0.")]
        public static IEnumerable<int> Samples(Random rnd, double[] p)
        {
            return SamplesWithProbabilityMass(rnd, p);
        }

        /// <summary>
        /// Samples a categorically distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="cdfUnnormalized">An array of the cumulative distribution. Not assumed to be normalized.</param>
        /// <returns>random integers between 0 and the size of the categorical (exclusive).</returns>
        public static IEnumerable<int> SamplesWithCumulativeDistribution(Random rnd, double[] cdfUnnormalized)
        {
            if (Control.CheckDistributionParameters && !IsValidCumulativeDistribution(cdfUnnormalized))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, cdfUnnormalized);
            }
        }

        /// <summary>
        /// Samples a categorically distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="pmfUnnormalized">An array of nonnegative ratios. Not assumed to be normalized.</param>
        /// <returns>random integers between 0 and the size of the categorical (exclusive).</returns>
        public static IEnumerable<int> SamplesWithProbabilityMass(Random rnd, double[] pmfUnnormalized)
        {
            if (Control.CheckDistributionParameters && !IsValidProbabilityMass(pmfUnnormalized))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            var cdf = ProbabilityMassToCumulativeDistribution(pmfUnnormalized);
            while (true)
            {
                yield return SampleUnchecked(rnd, cdf);
            }
        }

        /// <summary>
        /// Returns the inverse of the distribution function for the categorical distribution
        /// specified by the given normalized CDF, for the given probability.
        /// </summary>
        /// <param name="cdfUnnormalized">An array corresponding to a CDF for a categorical distribution. Not assumed to be normalized.</param>
        /// <param name="probability">A real number between 0 and 1.</param>
        /// <returns>An integer between 0 and the size of the categorical (exclusive),
        /// that corresponds to the inverse CDF for the given probability.</returns>
        public static int InverseCumulativeDistribution(double[] cdfUnnormalized, double probability)
        {
            if (Control.CheckDistributionParameters && !IsValidCumulativeDistribution(cdfUnnormalized))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            if (probability < 0.0 || probability > 1.0 || Double.IsNaN(probability))
            {
                throw new ArgumentOutOfRangeException("probability");
            }

            var denormalizedProbability = probability*cdfUnnormalized[cdfUnnormalized.Length - 1];
            int idx = Array.BinarySearch(cdfUnnormalized, denormalizedProbability);
            if (idx < 0)
            {
                idx = ~idx;
            }

            return idx;
        }
    }
}
