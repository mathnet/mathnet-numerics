// <copyright file="Weibull.cs" company="Math.NET">
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
    /// Continuous Univariate Weibull distribution.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Weibull_distribution">Wikipedia - Weibull distribution</a>.
    /// </summary>
    /// <remarks>
    /// The Weibull distribution is parametrized by a shape and scale parameter.
    /// </remarks>
    public class Weibull : IContinuousDistribution
    {
        System.Random _random;

        double _shape;
        double _scale;

        /// <summary>
        /// Reusable intermediate result 1 / (_scale ^ _shape)
        /// </summary>
        /// <remarks>
        /// By caching this parameter we can get slightly better numerics precision
        /// in certain constellations without any additional computations.
        /// </remarks>
        double _scalePowShapeInv;

        /// <summary>
        /// Initializes a new instance of the Weibull class.
        /// </summary>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        public Weibull(double shape, double scale)
        {
            _random = SystemRandomSource.Default;
            SetParameters(shape, scale);
        }

        /// <summary>
        /// Initializes a new instance of the Weibull class.
        /// </summary>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Weibull(double shape, double scale, System.Random randomSource)
        {
            _random = randomSource ?? SystemRandomSource.Default;
            SetParameters(shape, scale);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Weibull(k = " + _shape + ", λ = " + _scale + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double shape, double scale)
        {
            if (shape <= 0.0 || scale <= 0.0 || Double.IsNaN(shape) || Double.IsNaN(scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _shape = shape;
            _scale = scale;
            _scalePowShapeInv = Math.Pow(scale, -shape);
        }

        /// <summary>
        /// Gets or sets the shape (k) of the Weibull distribution. Range: k > 0.
        /// </summary>
        public double Shape
        {
            get { return _shape; }
            set { SetParameters(value, _scale); }
        }

        /// <summary>
        /// Gets or sets the scale (λ) of the Weibull distribution. Range: λ > 0.
        /// </summary>
        public double Scale
        {
            get { return _scale; }
            set { SetParameters(_shape, value); }
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
        /// Gets the mean of the Weibull distribution.
        /// </summary>
        public double Mean
        {
            get { return _scale*SpecialFunctions.Gamma(1.0 + (1.0/_shape)); }
        }

        /// <summary>
        /// Gets the variance of the Weibull distribution.
        /// </summary>
        public double Variance
        {
            get { return (_scale*_scale*SpecialFunctions.Gamma(1.0 + (2.0/_shape))) - (Mean*Mean); }
        }

        /// <summary>
        /// Gets the standard deviation of the Weibull distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(Variance); }
        }

        /// <summary>
        /// Gets the entropy of the Weibull distribution.
        /// </summary>
        public double Entropy
        {
            get { return (Constants.EulerMascheroni*(1.0 - (1.0/_shape))) + Math.Log(_scale/_shape) + 1.0; }
        }

        /// <summary>
        /// Gets the skewness of the Weibull distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                double mu = Mean;
                double sigma = StdDev;
                double sigma2 = sigma*sigma;
                double sigma3 = sigma2*sigma;
                return ((_scale*_scale*_scale*SpecialFunctions.Gamma(1.0 + (3.0/_shape))) - (3.0*sigma2*mu) - (mu*mu*mu))/sigma3;
            }
        }

        /// <summary>
        /// Gets the mode of the Weibull distribution.
        /// </summary>
        public double Mode
        {
            get
            {
                if (_shape <= 1.0)
                {
                    return 0.0;
                }

                return _scale*Math.Pow((_shape - 1.0)/_shape, 1.0/_shape);
            }
        }

        /// <summary>
        /// Gets the median of the Weibull distribution.
        /// </summary>
        public double Median
        {
            get { return _scale*Math.Pow(Constants.Ln2, 1.0/_shape); }
        }

        /// <summary>
        /// Gets the minimum of the Weibull distribution.
        /// </summary>
        public double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the maximum of the Weibull distribution.
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
        public double Density(double x)
        {
            if (x >= 0.0)
            {
                if (x == 0.0 && _shape == 1.0)
                {
                    return _shape/_scale;
                }

                return _shape*Math.Pow(x/_scale, _shape - 1.0)*Math.Exp(-Math.Pow(x, _shape)*_scalePowShapeInv)/_scale;
            }

            return 0.0;
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            if (x >= 0.0)
            {
                if (x == 0.0 && _shape == 1.0)
                {
                    return Math.Log(_shape) - Math.Log(_scale);
                }

                return Math.Log(_shape) + ((_shape - 1.0)*Math.Log(x/_scale)) - (Math.Pow(x, _shape)*_scalePowShapeInv) - Math.Log(_scale);
            }

            return double.NegativeInfinity;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < 0.0) return 0.0;

            return -SpecialFunctions.ExponentialMinusOne(-Math.Pow(x, _shape)*_scalePowShapeInv);
        }

        /// <summary>
        /// Generates one sample from the Weibull distribution. This method doesn't perform
        /// any parameter checks.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        /// <returns>A sample from a Weibull distributed random variable.</returns>
        static double SampleUnchecked(System.Random rnd, double shape, double scale)
        {
            var x = rnd.NextDouble();
            return scale*Math.Pow(-Math.Log(x), 1.0/shape);
        }

        /// <summary>
        /// Generates a sample from the Weibull distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _shape, _scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Weibull distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(_random, _shape, _scale);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double shape, double scale, double x)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            if (x >= 0.0)
            {
                if (x == 0.0 && shape == 1.0)
                {
                    return shape/scale;
                }

                return shape
                       *Math.Pow(x/scale, shape - 1.0)
                       *Math.Exp(-Math.Pow(x, shape)*Math.Pow(scale, -shape))
                       /scale;
            }

            return 0.0;
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double shape, double scale, double x)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            if (x >= 0.0)
            {
                if (x == 0.0 && shape == 1.0)
                {
                    return Math.Log(shape) - Math.Log(scale);
                }

                return Math.Log(shape)
                       + ((shape - 1.0)*Math.Log(x/scale))
                       - (Math.Pow(x, shape)*Math.Pow(scale, -shape))
                       - Math.Log(scale);
            }

            return double.NegativeInfinity;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double shape, double scale, double x)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            if (x < 0.0) return 0.0;

            return -SpecialFunctions.ExponentialMinusOne(-Math.Pow(x, shape)*Math.Pow(scale, -shape));
        }

        /// <summary>
        /// Generates a sample from the Weibull distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double shape, double scale)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return SampleUnchecked(rnd, shape, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Weibull distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Weibull distribution. Range: k > 0.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution. Range: λ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double shape, double scale)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            while (true)
            {
                yield return SampleUnchecked(rnd, shape, scale);
            }
        }
    }
}
