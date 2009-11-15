// <copyright file="Categorical.cs" company="Math.NET">
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

namespace MathNet.Numerics.Distributions
{
    using System;
    using System.Collections.Generic;
    using Properties;
    using MathNet.Numerics.Statistics;


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
    /// to false, all parameter checks can be turned off.</para></remarks>
    public class Categorical : IDiscreteDistribution
    {
        /// <summary>
        /// Stores the unnormalized categorical probabilities.
        /// </summary>
        private double[] _p;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the Categorical class.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <exception cref="ArgumentException">If any of the probabilities are negative or do not sum to one.</exception>
        public Categorical(double[] p)
        {
            SetParameters(p);
            RandomSource = new System.Random();
        }

        /// <summary>
        /// Initializes a new instance of the Categorical class from a histogram <paramref name="h"/>. The distribution 
        /// will not be automatically updated when the histogram changes. The categorical distribution will have
        /// one value for each bucket and a probability for that value proportional to the bucket count.
        /// </summary>
        /// <param name="h">The histogram from which to create the categorical variable.</param>
        public Categorical(Histogram histogram)
        {
            if (histogram == null)
            {
                throw new ArgumentNullException("Cannot create a categorical variable from a null histogram.");
            }

            // The probability distribution vector.
            double[] p = new double[histogram.BucketCount];

            // Fill in the distribution vector.
            for (int i = 0; i < histogram.BucketCount; i++)
            {
                p[i] = histogram[i].Count;
            }

            SetParameters(p);
            RandomSource = new System.Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        public override string ToString()
        {
            return "Categorical(Dimension = " + _p.Length + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>If any of the probabilities are negative returns false, or if the sum of parameters is 0.0; otherwise true</returns>
        private static bool IsValidParameterSet(double[] p)
        {
            double sum = 0.0;
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] < 0.0 || Double.IsNaN(p[i]))
                {
                    return false;
                }
                else
                {
                    sum += p[i];
                }
            }

            if (sum == 0.0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        private void SetParameters(double[] p)
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
        /// <remarks>Note that sometimes the normalized probability vector cannot be represented
        /// exactly in a floating point representation.</remarks>
        public double[] P
        {
            get
            {
                double[] p = (double[]) _p.Clone();

                double sum = 0.0;
                for (int i = 0; i < p.Length; i++)
                {
                    sum += p[i];
                }

                for (int i = 0; i < p.Length; i++)
                {
                    p[i] /= sum;
                }

                return p;
            }

            set
            {
                SetParameters(value);
            }
        }

        #region IDistribution Members

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public Random RandomSource
        {
            get
            {
                return _random;
            }

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
            get {
                double E = 0.0;
                for (int i = 0; i < _p.Length; i++)
                {
                    double p = _p[i];
                    E += p * Math.Log(p);
                }
                return E;
            }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum { get { return 0; } }

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum { get { return _p.Length-1; } }

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
            else if (x >= _p.Length)
            {
                return 1.0;
            }

            var cdf = UnnormalizedCDF(_p);
            return cdf[(int) Math.Floor(x)] / cdf[_p.Length - 1];
        }

        #endregion

        #region IDiscreteDistribution Members

        /// <summary>
        /// The mode of the distribution.
        /// </summary>
        public int Mode
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The median of the distribution.
        /// </summary>
        public int Median
        {
            get { return (int) _p.Median(); }
        }

        /// <summary>
        /// Computes the probability of a specific value.
        /// </summary>
        public double Probability(int val)
        {
            if (val < 0)
            {
                return 0.0;
            }

            if (val >= _p.Length)
            {
                return 0.0;
            }

            return _p[val];
        }

        /// <summary>
        /// Computes the probability of a specific value.
        /// </summary>
        public double ProbabilityLn(int val)
        {
            if (val < 0)
            {
                return 0.0;
            }

            if (val >= _p.Length)
            {
                return 0.0;
            }

            return Math.Log(_p[val]);
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

        #endregion

        /// <summary>
        /// Samples one categorical distributed random variable; also known as the Discrete distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>One random integer between 0 and the size of the categorical (exclusive).</returns>
        public static int Sample(System.Random rnd, double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // The cumulative density of p.
            double[] cp = UnnormalizedCDF(p);

            return DoSample(rnd, cp);
        }

        /// <summary>
        /// Samples a categorically distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns><paramref name="n"/> random integers between 0 and the size of the categorical (exclusive).</returns>
        public static IEnumerable<int> Samples(System.Random rnd, double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // The cumulative density of p.
            double[] cp = UnnormalizedCDF(p);

            while (true)
            {
                yield return DoSample(rnd, cp);
            }
        }

        /// <summary>
        /// Computes the unnormalized cumulative distribution function. This method performs no
        /// parameter checking.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>An array representing the unnormalized cumulative distribution function.</returns>
        internal static double[] UnnormalizedCDF(double[] p)
        {
            double[] cp = (double[]) p.Clone();

            for (int i = 1; i < p.Length; i++)
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
        /// <returns>One sample from the categorical distribution implied by <see cref="cdf"/>.</returns>
        internal static int DoSample(System.Random rnd, double[] cdf)
        {
            // TODO : use binary search to speed up this procedure.
            double u = rnd.NextDouble() * cdf[cdf.Length - 1];
            int idx = 0;
            while (u > cdf[idx])
            {
                idx++;
            }
            return idx;
        }
    }
}