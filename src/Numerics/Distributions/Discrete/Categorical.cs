// <copyright file="Categorical.cs" company="Math.NET">
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
        /// <summary>
        /// Stores the unnormalized categorical probabilities.
        /// </summary>
        double[] _p;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the Categorical class.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <exception cref="ArgumentException">If any of the probabilities are negative or do not sum to one.</exception>
        public Categorical(double[] p)
        {
            SetParameters(p);
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
            return "Categorical(Dimension = " + _p.Length + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>If any of the probabilities are negative returns <c>false</c>, or if the sum of parameters is 0.0; otherwise <c>true</c></returns>
        static bool IsValidParameterSet(IEnumerable<double> p)
        {
            var sum = 0.0;
            foreach (double t in p)
            {
                if (t < 0.0 || Double.IsNaN(t))
                {
                    return false;
                }

                sum += t;
            }

            return sum != 0.0;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _p = (double[])p.Clone();
        }

        /// <summary>
        /// Gets or sets the normalized probability vector of the multinomial.
        /// </summary>
        /// <remarks>Sometimes the normalized probability vector cannot be represented
        /// exactly in a floating point representation.</remarks>
        public double[] P
        {
            get
            {
                var p = (double[])_p.Clone();

                var sum = p.Sum();

                for (var i = 0; i < p.Length; i++)
                {
                    p[i] /= sum;
                }

                return p;
            }

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
            get { return _p.Mean(); }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return _p.StandardDeviation(); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _p.Variance(); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return _p.Sum(p => p * Math.Log(p)); }
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
            get { return _p.Length - 1; }
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

            if (x >= _p.Length)
            {
                return 1.0;
            }

            var cdf = UnnormalizedCdf(_p);
            return cdf[(int)Math.Floor(x)] / cdf[_p.Length - 1];
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
            get { return (int)_p.Median(); }
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

            if (k >= _p.Length)
            {
                return 0.0;
            }

            return _p[k];
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

            if (k >= _p.Length)
            {
                return 0.0;
            }

            return Math.Log(_p[k]);
        }

        #endregion

        /// <summary>
        /// Computes the unnormalized cumulative distribution function. This method performs no
        /// parameter checking.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>An array representing the unnormalized cumulative distribution function.</returns>
        internal static double[] UnnormalizedCdf(double[] p)
        {
            var cp = (double[])p.Clone();

            for (var i = 1; i < p.Length; i++)
            {
                cp[i] += cp[i - 1];
            }

            return cp;
        }

        /// <summary>
        /// Returns one trials from the categorical distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="cdf">The cumulative distribution of the probability distribution.</param>
        /// <returns>One sample from the categorical distribution implied by <paramref name="cdf"/>.</returns>
        internal static int SampleUnchecked(Random rnd, double[] cdf)
        {
            // TODO : use binary search to speed up this procedure.
            var u = rnd.NextDouble() * cdf[cdf.Length - 1];

            var idx = 0;
            while (u > cdf[idx])
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
            return Sample(RandomSource, _p);
        }

        /// <summary>
        /// Samples an array of Bernoulli distributed random variables.
        /// </summary>
        /// <returns>a sequence of successful trial counts.</returns>
        public IEnumerable<int> Samples()
        {
            return Samples(RandomSource, _p);
        }

        /// <summary>
        /// Samples one categorical distributed random variable; also known as the Discrete distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>One random integer between 0 and the size of the categorical (exclusive).</returns>
        public static int Sample(Random rnd, double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // The cumulative density of p.
            var cp = UnnormalizedCdf(p);

            return SampleUnchecked(rnd, cp);
        }

        /// <summary>
        /// Samples a categorically distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>random integers between 0 and the size of the categorical (exclusive).</returns>
        public static IEnumerable<int> Samples(Random rnd, double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // The cumulative density of p.
            var cp = UnnormalizedCdf(p);

            while (true)
            {
                yield return SampleUnchecked(rnd, cp);
            }
        }
    }
}
