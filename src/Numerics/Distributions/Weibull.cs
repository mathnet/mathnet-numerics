// <copyright file="Weibull.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Weibull distribution.
    /// For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Weibull_distribution">Wikipedia - Weibull distribution</a>.
    /// </summary>
    /// <remarks>
    /// <para>The Weibull distribution is parametrized by a shape and scale parameter.</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
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
        /// <param name="shape">The shape (k) of the Weibull distribution.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution.</param>
        public Weibull(double shape, double scale)
        {
            _random = new System.Random();
            SetParameters(shape, scale);
        }

        /// <summary>
        /// Initializes a new instance of the Weibull class.
        /// </summary>
        /// <param name="shape">The shape (k) of the Weibull distribution.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Weibull(double shape, double scale, System.Random randomSource)
        {
            _random = randomSource ?? new System.Random();
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
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="shape">The shape (k) of the Weibull distribution.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution.</param>
        /// <returns><c>true</c> when the parameters positive valid floating point numbers, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double shape, double scale)
        {
            return shape > 0.0 && scale > 0.0;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="shape">The shape (k) of the Weibull distribution.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double shape, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(shape, scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _shape = shape;
            _scale = scale;
            _scalePowShapeInv = Math.Pow(scale, -shape);
        }

        /// <summary>
        /// Gets or sets the shape (k) of the Weibull distribution.
        /// </summary>
        public double Shape
        {
            get { return _shape; }
            set { SetParameters(value, _scale); }
        }

        /// <summary>
        /// Gets or sets the scale (λ) of the Weibull distribution.
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
            set { _random = value ?? new System.Random(); }
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
        /// Computes the probability density of the distribution (PDF) at x, i.e. dP(X &lt;= x)/dx.
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
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(dP(X &lt;= x)/dx).
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
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X &lt;= x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < 0.0)
            {
                return 0.0;
            }

            return -SpecialFunctions.ExponentialMinusOne(-Math.Pow(x, _shape)*_scalePowShapeInv);
        }

        /// <summary>
        /// Generates one sample from the Weibull distribution. This method doesn't perform
        /// any parameter checks.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Weibull distribution.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution.</param>
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
        /// Generates a sample from the Weibull distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Weibull distribution.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double shape, double scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(shape, scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, shape, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Weibull distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Weibull distribution.</param>
        /// <param name="scale">The scale (λ) of the Weibull distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double shape, double scale)
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
