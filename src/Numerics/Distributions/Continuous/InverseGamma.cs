// <copyright file="InverseGamma.cs" company="Math.NET">
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
    /// The inverse Gamma distribution is a distribution over the positive real numbers parameterized by
    /// two positive parameters.
    /// <a href="http://en.wikipedia.org/wiki/inverse-gamma_distribution">Wikipedia - InverseGamma distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class InverseGamma : IContinuousDistribution
    {
        /// <summary>
        /// Inverse Gamma shape parameter. 
        /// </summary>
        double _shape;

        /// <summary>
        /// Inverse Gamma scale parameter scale. 
        /// </summary>
        double _scale;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="InverseGamma"/> class. 
        /// </summary>
        /// <param name="shape">
        /// The shape (alpha) parameter of the inverse Gamma distribution.
        /// </param>
        /// <param name="scale">
        /// The scale (beta) parameter of the inverse Gamma distribution.
        /// </param>
        public InverseGamma(double shape, double scale)
        {
            SetParameters(shape, scale);
            _random = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="shape">
        /// The shape (alpha) parameter of the inverse Gamma distribution.
        /// </param>
        /// <param name="scale">
        /// The scale (beta) parameter of the inverse Gamma distribution.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double shape, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(shape, scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _shape = shape;
            _scale = scale;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="shape">
        /// The shape (alpha) parameter of the inverse Gamma distribution.
        /// </param>
        /// <param name="scale">
        /// The scale (beta) parameter of the inverse Gamma distribution.
        /// </param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double shape, double scale)
        {
            if (shape <= 0 || scale <= 0)
            {
                return false;
            }

            if (Double.IsNaN(shape) || Double.IsNaN(scale))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the shape (alpha) parameter.
        /// </summary>
        public double Shape
        {
            get { return _shape; }

            set { SetParameters(value, _scale); }
        }

        /// <summary>
        /// Gets or sets The scale (beta) parameter.
        /// </summary>
        public double Scale
        {
            get { return _scale; }

            set { SetParameters(_shape, value); }
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "InverseGamma(Shape = " + _shape + ", Inverse Scale = " + _scale + ")";
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
            get
            {
                if (_shape <= 1)
                {
                    throw new NotSupportedException();
                }

                return _scale / (_shape - 1.0);
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_shape <= 2)
                {
                    throw new NotSupportedException();
                }

                return _scale * _scale / ((_shape - 1.0) * (_shape - 1.0) * (_shape - 2.0));
            }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return _scale / (Math.Abs(_shape - 1.0) * Math.Sqrt(_shape - 2.0)); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return _shape + Math.Log(_scale) + SpecialFunctions.GammaLn(_shape) - ((1 + _shape) * SpecialFunctions.DiGamma(_shape)); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (_shape <= 3)
                {
                    throw new NotSupportedException();
                }

                return (4 * Math.Sqrt(_shape - 2)) / (_shape - 3);
            }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return SpecialFunctions.GammaUpperRegularized(_shape, _scale / x);
        }

        #endregion

        #region IContinuousDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get { return _scale / (_shape + 1.0); }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <remarks>Throws <see cref="NotSupportedException"/>.</remarks>
        public double Median
        {
            get { throw new NotSupportedException(); }
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
            if (x >= 0.0)
            {
                return Math.Pow(_scale, _shape) * Math.Pow(x, -_shape - 1.0) * Math.Exp(-_scale / x) / SpecialFunctions.Gamma(_shape);
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
            return Math.Log(Density(x));
        }

        #endregion

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (alpha) parameter of the inverse Gamma distribution.</param>
        /// <param name="scale">The scale (beta) parameter of the inverse Gamma distribution.</param>
        /// <returns>a random number from the distribution.</returns>
        internal static double SampleUnchecked(Random rnd, double shape, double scale)
        {
            return 1.0 / Gamma.Sample(rnd, shape, scale);
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _shape, _scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Cauchy distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _shape, _scale);
            }
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (alpha) parameter of the inverse Gamma distribution.</param>
        /// <param name="scale">The scale (beta) parameter of the inverse Gamma distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rnd, double shape, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(shape, scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, shape, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (alpha) parameter of the inverse Gamma distribution.</param>
        /// <param name="scale">The scale (beta) parameter of the inverse Gamma distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rnd, double shape, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(shape, scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, shape, scale);
            }
        }
    }
}
