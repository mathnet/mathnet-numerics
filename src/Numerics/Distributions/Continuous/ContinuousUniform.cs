// <copyright file="ContinuousUniform.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

    /// <summary>
    /// The continuous uniform distribution is a distribution over real numbers. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Uniform_distribution_%28continuous%29">Wikipedia - Continuous uniform distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class ContinuousUniform : IContinuousDistribution
    {
        /// <summary>
        /// The distribution's lower bound.
        /// </summary>
        double _lower;

        /// <summary>
        /// The distribution's upper bound.
        /// </summary>
        double _upper;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the ContinuousUniform class with lower bound 0 and upper bound 1.
        /// </summary>
        public ContinuousUniform()
            : this(0.0, 1.0)
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
            RandomSource = new Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "ContinuousUniform(Lower = " + _lower + ", Upper = " + _upper + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="lower">Lower bound.</param>
        /// <param name="upper">Upper bound; must be at least as large as <paramref name="lower"/>.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double lower, double upper)
        {
            if (upper < lower)
            {
                return false;
            }

            if (Double.IsNaN(upper) || Double.IsNaN(lower))
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
        void SetParameters(double lower, double upper)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lower, upper))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _lower = lower;
            _upper = upper;
        }

        /// <summary>
        /// Gets or sets the lower bound of the distribution.
        /// </summary>
        public double Lower
        {
            get { return _lower; }

            set { SetParameters(value, _upper); }
        }

        /// <summary>
        /// Gets or sets the upper bound of the distribution.
        /// </summary>
        public double Upper
        {
            get { return _upper; }

            set { SetParameters(_lower, value); }
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
            get { return (_lower + _upper) / 2.0; }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return (_upper - _lower) * (_upper - _lower) / 12.0; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return (_upper - _lower) / Math.Sqrt(12.0); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        /// <value></value>
        public double Entropy
        {
            get { return Math.Log(_upper - _lower); }
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
            get { return (_lower + _upper) / 2.0; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <value></value>
        public double Median
        {
            get { return (_lower + _upper) / 2.0; }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return _lower; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return _upper; }
        }

        /// <summary>
        /// Computes the density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            if (x >= _lower && x <= _upper)
            {
                return 1.0 / (_upper - _lower);
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
            if (x >= _lower && x <= _upper)
            {
                return -Math.Log(_upper - _lower);
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
            if (x <= _lower)
            {
                return 0.0;
            }

            if (x >= _upper)
            {
                return 1.0;
            }

            return (x - _lower) / (_upper - _lower);
        }

        #endregion

        /// <summary>
        /// Generates one sample from the <c>ContinuousUniform</c> distribution without parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">The lower bound of the uniform random variable.</param>
        /// <param name="upper">The upper bound of the uniform random variable.</param>
        /// <returns>a uniformly distributed random number.</returns>
        internal static double SampleUnchecked(Random rnd, double lower, double upper)
        {
            return lower + (rnd.NextDouble() * (upper - lower));
        }

        /// <summary>
        /// Generates a sample from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _lower, _upper);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _lower, _upper);
            }
        }

        /// <summary>
        /// Generates a sample from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">The lower bound of the uniform random variable.</param>
        /// <param name="upper">The upper bound of the uniform random variable.</param>
        /// <returns>a uniformly distributed sample.</returns>
        public static double Sample(Random rnd, double lower, double upper)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lower, upper))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, lower, upper);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">The lower bound of the uniform random variable.</param>
        /// <param name="upper">The upper bound of the uniform random variable.</param>
        /// <returns>a sequence of uniformly distributed samples.</returns>
        public static IEnumerable<double> Samples(Random rnd, double lower, double upper)
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
    }
}