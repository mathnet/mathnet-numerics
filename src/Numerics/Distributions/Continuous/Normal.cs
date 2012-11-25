// <copyright file="Normal.cs" company="Math.NET">
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
    using Properties;

    /// <summary>
    /// Implements the univariate Normal (or Gaussian) distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Normal_distribution">Wikipedia - Normal distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Normal : IContinuousDistribution
    {
        /// <summary>
        /// Keeps track of the mean of the normal distribution.
        /// </summary>
        double _mean;

        /// <summary>
        /// Keeps track of the standard deviation of the normal distribution.
        /// </summary>
        double _stdDev;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the Normal class. This is a normal distribution with mean 0.0
        /// and standard deviation 1.0. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        public Normal()
            : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Normal class with a particular mean and standard deviation. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        public Normal(double mean, double stddev)
        {
            SetParameters(mean, stddev);
            RandomSource = new Random();
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and standard deviation. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        /// <returns>a normal distribution.</returns>
        public static Normal WithMeanStdDev(double mean, double stddev)
        {
            return new Normal(mean, stddev);
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and variance. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="var">The variance of the normal distribution.</param>
        /// <returns>a normal distribution.</returns>
        public static Normal WithMeanVariance(double mean, double var)
        {
            return new Normal(mean, Math.Sqrt(var));
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and precision. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="precision">The precision of the normal distribution.</param>
        /// <returns>a normal distribution.</returns>
        public static Normal WithMeanPrecision(double mean, double precision)
        {
            return new Normal(mean, 1.0 / Math.Sqrt(precision));
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Normal(Mean = " + _mean + ", StdDev = " + _stdDev + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double mean, double stddev)
        {
            if (stddev < 0.0 || Double.IsNaN(mean) || Double.IsNaN(stddev))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double mean, double stddev)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(mean, stddev))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _mean = mean;
            _stdDev = stddev;
        }

        /// <summary>
        /// Gets or sets the precision of the normal distribution.
        /// </summary>
        public double Precision
        {
            get { return 1.0 / (_stdDev * _stdDev); }

            set
            {
                var sdev = 1.0 / Math.Sqrt(value);

                // Handle the case when the precision is -0.
                if (Double.IsInfinity(sdev))
                {
                    sdev = Double.PositiveInfinity;
                }

                SetParameters(_mean, sdev);
            }
        }

        #region IDistribution implementation

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
        /// Gets or sets the mean of the normal distribution.
        /// </summary>
        public double Mean
        {
            get { return _mean; }

            set { SetParameters(value, _stdDev); }
        }

        /// <summary>
        /// Gets or sets the variance of the normal distribution.
        /// </summary>
        public double Variance
        {
            get { return _stdDev * _stdDev; }

            set { SetParameters(_mean, Math.Sqrt(value)); }
        }

        /// <summary>
        /// Gets or sets the standard deviation of the normal distribution.
        /// </summary>
        public double StdDev
        {
            get { return _stdDev; }

            set { SetParameters(_mean, value); }
        }

        /// <summary>
        /// Gets the entropy of the normal distribution.
        /// </summary>
        public double Entropy
        {
            get { return Math.Log(_stdDev) + Constants.LogSqrt2PiE; }
        }

        /// <summary>
        /// Gets the skewness of the normal distribution.
        /// </summary>
        public double Skewness
        {
            get { return 0.0; }
        }

        #endregion

        #region IContinuousDistribution implementation

        /// <summary>
        /// Gets the mode of the normal distribution.
        /// </summary>
        public double Mode
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the median of the normal distribution.
        /// </summary>
        public double Median
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the minimum of the normal distribution.
        /// </summary>
        public double Minimum
        {
            get { return Double.NegativeInfinity; }
        }

        /// <summary>
        /// Gets the maximum of the normal distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the density of the normal distribution.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="sdev">The standard deviation of the normal distribution.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        internal static double Density(double mean, double sdev, double x)
        {
            var d = (x - mean) / sdev;
            return Math.Exp(-0.5 * d * d) / (Constants.Sqrt2Pi * sdev);
        }

        /// <summary>
        /// Computes the log density of the normal distribution.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="sdev">The standard deviation of the normal distribution.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        internal static double DensityLn(double mean, double sdev, double x)
        {
            var d = (x - mean) / sdev;
            return (-0.5 * d * d) - Math.Log(sdev) - Constants.LogSqrt2Pi;
        }

        /// <summary>
        /// Computes the density of the normal distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            return Density(_mean, _stdDev, x);
        }

        /// <summary>
        /// Computes the log density of the normal distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return DensityLn(_mean, _stdDev, x);
        }

        /// <summary>
        /// Computes the cumulative distribution function of the normal distribution.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="sdev">The standard deviation of the normal distribution.</param>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        internal static double CumulativeDistribution(double mean, double sdev, double x)
        {
            return 0.5 * (1.0 + SpecialFunctions.Erf((x - mean) / (sdev * Constants.Sqrt2)));
        }

        /// <summary>
        /// Computes the cumulative distribution function of the normal distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return CumulativeDistribution(_mean, _stdDev, x);
        }

        #endregion

        /// <summary>
        /// Computes the inverse cumulative distribution function of the normal distribution.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        public double InverseCumulativeDistribution(double p)
        {
            return _mean - (_stdDev * Math.Sqrt(2.0) * SpecialFunctions.ErfcInv(2.0 * p));
        }



        /// <summary>
        /// Samples a pair of standard normal distributed random variables using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <returns>a pair of random numbers from the standard normal distribution.</returns>
        internal static Tuple<double, double> SampleUncheckedBoxMuller(Random rnd)
        {
            var v1 = (2.0 * rnd.NextDouble()) - 1.0;
            var v2 = (2.0 * rnd.NextDouble()) - 1.0;
            var r = (v1 * v1) + (v2 * v2);
            while (r >= 1.0 || r == 0.0)
            {
                v1 = (2.0 * rnd.NextDouble()) - 1.0;
                v2 = (2.0 * rnd.NextDouble()) - 1.0;
                r = (v1 * v1) + (v2 * v2);
            }

            var fac = Math.Sqrt(-2.0 * Math.Log(r) / r);
            return new Tuple<double, double>(v1 * fac, v2 * fac);
        }

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean of the normal distribution from which to generate samples.</param>
        /// <param name="stddev">The standard deviation of the normal distribution from which to generate samples.</param>
        /// <returns>a random number from the distribution.</returns>
        internal static double SampleUnchecked(Random rnd, double mean, double stddev)
        {
            return mean + (stddev * SampleUncheckedBoxMuller(rnd).Item1);
        }

        /// <summary>
        /// Generates a sample from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _mean, _stdDev);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                var sample = SampleUncheckedBoxMuller(RandomSource);
                yield return _mean + (_stdDev * sample.Item1);
                yield return _mean + (_stdDev * sample.Item2);
            }
        }

        /// <summary>
        /// Generates a sample from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean of the normal distribution from which to generate samples.</param>
        /// <param name="stddev">The standard deviation of the normal distribution from which to generate samples.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rnd, double mean, double stddev)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(mean, stddev))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, mean, stddev);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean of the normal distribution from which to generate samples.</param>
        /// <param name="stddev">The standard deviation of the normal distribution from which to generate samples.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rnd, double mean, double stddev)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(mean, stddev))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                var sample = SampleUncheckedBoxMuller(rnd);
                yield return mean + (stddev * sample.Item1);
                yield return mean + (stddev * sample.Item2);
            }
        }
    }
}
