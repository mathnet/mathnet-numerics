// <copyright file="Cauchy.cs" company="Math.NET">
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
    /// The Cauchy distribution is a symmetric continuous probability distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/cauchy_distribution">Wikipedia - Cauchy distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Cauchy : IContinuousDistribution
    {
        /// <summary>
        /// The scale of the Cauchy distribution.
        /// </summary>
        double _scale;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cauchy"/> class with the location parameter set to 0 and the scale parameter set to 1
        /// </summary>
        public Cauchy()
            : this(0, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cauchy"/> class. 
        /// </summary>
        /// <param name="location">
        /// The location parameter for the distribution.
        /// </param>
        /// <param name="scale">
        /// The scale parameter for the distribution.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="scale"/> is negative.
        /// </exception>
        public Cauchy(double location, double scale)
        {
            SetParameters(location, scale);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="location">Location parameter.</param>
        /// <param name="scale">Scale parameter. Must be greater than 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double location, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(location, scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            Median = location;
            _scale = scale;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="location">Location parameter.</param>
        /// <param name="scale">Scale parameter. Must be greater than 0.</param>
        /// <returns>True when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double location, double scale)
        {
            if (scale <= 0)
            {
                return false;
            }

            if (Double.IsNaN(location) || Double.IsNaN(scale))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the location parameter of the distribution.
        /// </summary>
        public double Location
        {
            get { return Median; }

            set { SetParameters(value, _scale); }
        }

        /// <summary>
        /// Gets or sets the scale parameter of the distribution.
        /// </summary>
        public double Scale
        {
            get { return _scale; }

            set { SetParameters(Median, value); }
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Cauchy(Location = " + Median + ", Scale = " + _scale + ")";
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
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return Math.Log(4.0 * Constants.Pi * _scale); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return ((1.0 / Constants.Pi) * Math.Atan((x - Median) / _scale)) + 0.5;
        }

        #endregion

        #region IContinuousDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get { return Median; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median { get; private set; }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return Double.NegativeInfinity; }
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
            return 1.0 / (Constants.Pi * _scale * (1.0 + (((x - Median) / _scale) * ((x - Median) / _scale))));
        }

        /// <summary>
        /// Computes the log density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return -Math.Log(Constants.Pi * _scale * (1.0 + (((x - Median) / _scale) * ((x - Median) / _scale))));
        }

        #endregion

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location shape parameter.</param>
        /// <param name="scale">The scale parameter.</param>
        /// <returns>a random number from the distribution.</returns>
        internal static double SampleUnchecked(Random rnd, double location, double scale)
        {
            var u = rnd.NextDouble();
            return location + (scale * Math.Tan(Constants.Pi * (u - 0.5)));
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, Median, _scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Cauchy distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, Median, _scale);
            }
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location shape parameter.</param>
        /// <param name="scale">The scale parameter.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rnd, double location, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(location, scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, location, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location shape parameter.</param>
        /// <param name="scale">The scale parameter.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rnd, double location, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(location, scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, location, scale);
            }
        }
    }
}
