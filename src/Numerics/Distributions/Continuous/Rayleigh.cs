// <copyright file="Rayleigh.cs" company="Math.NET">
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
    /// The Rayleigh distribution (pronounced /ˈreɪli/) is a continuous probability distribution. As an 
    /// example of how it arises, the wind speed will have a Rayleigh distribution if the components of 
    /// the two-dimensional wind velocity vector are uncorrelated and normally distributed with equal variance.
    /// For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Rayleigh_distribution">Wikipedia - Rayleigh distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Rayleigh : IContinuousDistribution
    {
        /// <summary>
        /// The scale parameter of the distribution.
        /// </summary>
        double _scale;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rayleigh"/> class. 
        /// </summary>
        /// <param name="scale">
        /// The scale parameter of the distribution.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="scale"/> is negative.
        /// </exception>
        public Rayleigh(double scale)
        {
            SetParameters(scale);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="scale">The scale parameter of the distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _scale = scale;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="scale">The scale parameter of the distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double scale)
        {
            if (scale <= 0)
            {
                return false;
            }

            if (Double.IsNaN(scale))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the scale parameter of the distribution.
        /// </summary>
        public double Scale
        {
            get { return _scale; }

            set { SetParameters(value); }
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Rayleigh(Scale = " + _scale + ")";
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
            get { return _scale * Math.Sqrt(Constants.PiOver2); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return (2.0 - Constants.PiOver2) * _scale * _scale; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(2.0 - Constants.PiOver2) * _scale; }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return 1.0 + Math.Log(_scale / Math.Sqrt(2)) + (Constants.EulerMascheroni / 2.0); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return (2.0 * Math.Sqrt(Constants.Pi) * (Constants.Pi - 3.0)) / Math.Pow(4.0 - Constants.Pi, 1.5); }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return 1.0 - Math.Exp(-x * x / (2.0 * _scale * _scale));
        }

        #endregion

        #region IContinuousDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get { return _scale; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median
        {
            get { return _scale * Math.Sqrt(Math.Log(4.0)); }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            return (x / (_scale * _scale)) * Math.Exp(-x * x / (2.0 * _scale * _scale));
        }

        /// <summary>
        /// Computes the log density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return Math.Log(x / (_scale * _scale)) - (x * x / (2.0 * _scale * _scale));
        }

        #endregion

        /// <summary>
        /// Generates a sample from the Rayleigh distribution without doing parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale parameter.</param>
        /// <returns>a random number from the Rayleigh distribution.</returns>
        internal static double SampleUnchecked(Random rnd, double scale)
        {
            return scale * Math.Sqrt(-2.0 * Math.Log(rnd.NextDouble()));
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Rayleigh distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _scale);
            }
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale parameter.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rnd, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale parameter.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rnd, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, scale);
            }
        }
    }
}
