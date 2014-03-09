// <copyright file="Pareto.cs" company="Math.NET">
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
    /// Continuous Univariate Pareto distribution.
    /// The Pareto distribution is a power law probability distribution that coincides with social,
    /// scientific, geophysical, actuarial, and many other types of observable phenomena.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Pareto_distribution">Wikipedia - Pareto distribution</a>.
    /// </summary>
    public class Pareto : IContinuousDistribution
    {
        System.Random _random;

        double _scale;
        double _shape;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pareto"/> class.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <exception cref="ArgumentException">If <paramref name="scale"/> or <paramref name="shape"/> are negative.</exception>
        public Pareto(double scale, double shape)
        {
            _random = SystemRandomSource.Default;
            SetParameters(scale, shape);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pareto"/> class.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentException">If <paramref name="scale"/> or <paramref name="shape"/> are negative.</exception>
        public Pareto(double scale, double shape, System.Random randomSource)
        {
            _random = randomSource ?? SystemRandomSource.Default;
            SetParameters(scale, shape);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Pareto(xm = " + _scale + ", α = " + _shape + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double scale, double shape)
        {
            if (scale <= 0.0 || shape <= 0.0 || Double.IsNaN(scale) || Double.IsNaN(shape))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _scale = scale;
            _shape = shape;
        }

        /// <summary>
        /// Gets or sets the scale (xm) of the distribution. Range: xm > 0.
        /// </summary>
        public double Scale
        {
            get { return _scale; }
            set { SetParameters(value, _shape); }
        }

        /// <summary>
        /// Gets or sets the shape (α) of the distribution. Range: α > 0.
        /// </summary>
        public double Shape
        {
            get { return _shape; }
            set { SetParameters(_scale, value); }
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
            get
            {
                if (_shape <= 1)
                {
                    throw new NotSupportedException();
                }

                return _shape*_scale/(_shape - 1.0);
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_shape <= 2.0)
                {
                    return double.PositiveInfinity;
                }

                return _scale*_scale*_shape/((_shape - 1.0)*(_shape - 1.0)*(_shape - 2.0));
            }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return (_scale*Math.Sqrt(_shape))/(Math.Abs(_shape - 1.0)*Math.Sqrt(_shape - 2.0)); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return Math.Log(_shape/_scale) - (1.0/_shape) - 1.0; }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return (2.0*(_shape + 1.0)/(_shape - 3.0))*Math.Sqrt((_shape - 2.0)/_shape); }
        }

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
            get { return _scale*Math.Pow(2.0, 1.0/_shape); }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return _scale; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return _shape*Math.Pow(_scale, _shape)/Math.Pow(x, _shape + 1.0);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return Math.Log(_shape) + _shape*Math.Log(_scale) - (_shape + 1.0)*Math.Log(x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return 1.0 - Math.Pow(_scale/x, _shape);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InvCDF"/>
        public double InverseCumulativeDistribution(double p)
        {
            return _scale*Math.Pow(1.0 - p, -1.0/_shape);
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return _scale*Math.Pow(_random.NextDouble(), -1.0/_shape);
        }

        /// <summary>
        /// Generates a sequence of samples from the Pareto distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            var power = -1.0/_shape;
            while (true)
            {
                yield return _scale*Math.Pow(_random.NextDouble(), power);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double scale, double shape, double x)
        {
            if (scale <= 0.0 || shape <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return shape*Math.Pow(scale, shape)/Math.Pow(x, shape + 1.0);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double scale, double shape, double x)
        {
            if (scale <= 0.0 || shape <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return Math.Log(shape) + shape*Math.Log(scale) - (shape + 1.0)*Math.Log(x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double scale, double shape, double x)
        {
            if (scale <= 0.0 || shape <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return 1.0 - Math.Pow(scale/x, shape);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public static double InvCDF(double scale, double shape, double p)
        {
            if (scale <= 0.0 || shape <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return scale*Math.Pow(1.0 - p, -1.0/shape);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double scale, double shape)
        {
            if (scale <= 0.0 || shape <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return scale*Math.Pow(rnd.NextDouble(), -1.0/shape);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double scale, double shape)
        {
            if (scale <= 0.0 || shape <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            var power = -1.0 / shape;
            while (true)
            {
                yield return scale*Math.Pow(rnd.NextDouble(), power);
            }
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double scale, double shape)
        {
            return Sample(SystemRandomSource.Default, scale, shape);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double scale, double shape)
        {
            return Samples(SystemRandomSource.Default, scale, shape);
        }
    }
}
