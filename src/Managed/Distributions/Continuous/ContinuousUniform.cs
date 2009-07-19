// <copyright file="ContinuousUniform.cs" company="Math.NET">
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
    using MathNet.Numerics.Properties;

    /// <summary>
    /// The continuous uniform distribution is a distribution over real numbers. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Uniform_distribution_%28continuous%29">Wikipedia - Continuous uniform distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to false, all parameter checks can be turned off.</para></remarks>
    public class ContinuousUniform : IContinuousDistribution
    {
        /// <summary>
        /// The distribution's lower bound.
        /// </summary>
        private double mLower;

        /// <summary>
        /// The distribution's upper bound.
        /// </summary>
        private double mUpper;

        /// <summary>
        /// Initializes a new instance of the ContinuousUniform class with lower bound 0 and upper bound 1.
        /// </summary>
        public ContinuousUniform() : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ContinuousUniform class with given lower and upper bounds.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound; must be at least as large as <paramref name="lower"/>.</param>
        /// <exception cref="ArgumentException">If the upper bound is smaller than the lower bound.</exception>
        public ContinuousUniform(double lower, double upper)
        {
            SetParameters(lower, upper);
            RandomSource = new System.Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "ContinuousUniform(Lower = " + mLower + ", Upper = " + mUpper + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound; must be at least as large as <paramref name="lower"/>.</param>
        /// <returns>True when the parameters are valid, false otherwise.</returns>
        private static bool IsValidParameterSet(double lower, double upper)
        {
            if (upper < lower)
            {
                return false;
            }
            else if(Double.IsNaN(upper) || Double.IsNaN(lower))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound; must be at least as large as <paramref name="lower"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        private void SetParameters(double lower, double upper)
        {
            if (!Control.CheckDistributionParameters || IsValidParameterSet(lower, upper))
            {
                mLower = lower;
                mUpper = upper;
            }
            else
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }
        }

        /// <summary>
        /// Gets or sets the lower bound of the distribution.
        /// </summary>
        public double Lower
        {
            get
            {
                return mLower;
            }

            set
            {
                SetParameters(value, mUpper);
            }
        }

        /// <summary>
        /// Gets or sets the upper bound of the distribution.
        /// </summary>
        public double Upper
        {
            get
            {
                return mUpper;
            }

            set
            {
                SetParameters(mLower, value);
            }
        }

        #region IDistribution Members

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public Random RandomSource { get; set; }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean
        {
            get { return (mLower + mUpper)/2.0; }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return (mUpper - mLower)*(mUpper - mLower)/12.0; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return (mUpper - mLower) / System.Math.Sqrt(12.0); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        /// <value></value>
        public double Entropy
        {
            get { return System.Math.Log(mUpper - mLower); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return 0.0; }
        }
        #endregion

        #region IContinuousDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        /// <value></value>
        public double Mode
        {
            get { return (mLower + mUpper)/2.0; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <value></value>
        public double Median
        {
            get { return (mLower + mUpper)/2.0; }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return mLower; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return mUpper; }
        }

        /// <summary>
        /// Computes the density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            if (x >= mLower && x <= mUpper)
            {
                return 1.0/(mUpper - mLower);
            }

            return 0.0;
        }

        /// <summary>
        /// Computes the log density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            if (x >= mLower && x <= mUpper)
            {
                return -Math.Log(mUpper - mLower);
            }

            return Double.NegativeInfinity;
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if(x <= mLower)
            {
                return 0.0;
            }
            else if(x >= mUpper)
            {
                return 1.0;
            }

            return (x - mLower)/(mUpper - mLower);
        }

        /// <summary>
        /// Generates a sample from the ContinuousUniform distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return DoSample(RandomSource, mLower, mUpper);
        }

        /// <summary>
        /// Generates a sequence of samples from the ContinuousUniform distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while(true)
            {
                yield return DoSample(RandomSource, mLower, mUpper);
            }
        }

        #endregion

        /// <summary>
        /// Generates a sample from the ContinuousUniform distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">The lower bound of the uniform random variable.</param>
        /// <param name="upper">The upper bound of the uniform random variable.</param>
        /// <returns>a uniformly distributed sample.</returns>
        public static double Sample(System.Random rnd, double lower, double upper)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lower, upper))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return DoSample(rnd, lower, upper);
        }

        /// <summary>
        /// Generates a sequence of samples from the ContinuousUniform distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">The lower bound of the uniform random variable.</param>
        /// <param name="upper">The upper bound of the uniform random variable.</param>
        /// <returns>a sequence of uniformly distributed samples.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double lower, double upper)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lower, upper))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while(true)
            {
                yield return DoSample(rnd, lower, upper);
            }
        }

        /// <summary>
        /// Generates one sample from the ContinuousUniform distribution without parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">The lower bound of the uniform random variable.</param>
        /// <param name="upper">The upper bound of the uniform random variable.</param>
        /// <returns>a uniformly distributed random number.</returns>
        private static double DoSample(System.Random rnd, double lower, double upper)
        {
            return lower + (rnd.NextDouble()*(upper - lower));
        }
    }
}