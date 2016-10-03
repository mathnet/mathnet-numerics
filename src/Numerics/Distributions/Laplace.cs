﻿// <copyright file="Laplace.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Laplace distribution.
    /// The Laplace distribution is a distribution over the real numbers parameterized by a mean and
    /// scale parameter. The PDF is:
    ///     p(x) = \frac{1}{2 * scale} \exp{- |x - mean| / scale}.
    /// <a href="http://en.wikipedia.org/wiki/Laplace_distribution">Wikipedia - Laplace distribution</a>.
    /// </summary>
    public class Laplace : IContinuousDistribution
    {
        System.Random _random;

        readonly double _location;
        readonly double _scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="Laplace"/> class (location = 0, scale = 1).
        /// </summary>
        public Laplace()
            : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Laplace"/> class.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <exception cref="ArgumentException">If <paramref name="scale"/> is negative.</exception>
        public Laplace(double location, double scale)
        {
            if (!IsValidParameterSet(location, scale))
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            _random = SystemRandomSource.Default;
            _location = location;
            _scale = scale;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Laplace"/> class.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentException">If <paramref name="scale"/> is negative.</exception>
        public Laplace(double location, double scale, System.Random randomSource)
        {
            if (!IsValidParameterSet(location, scale))
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _location = location;
            _scale = scale;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Laplace(μ = " + _location + ", b = " + _scale + ")";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        public static bool IsValidParameterSet(double location, double scale)
        {
            return scale > 0.0 && !double.IsNaN(location);
        }

        /// <summary>
        /// Gets the location (μ) of the Laplace distribution.
        /// </summary>
        public double Location
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the scale (b) of the Laplace distribution. Range: b > 0.
        /// </summary>
        public double Scale
        {
            get { return _scale; }
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
            get { return _location; }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return 2.0*_scale*_scale; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Constants.Sqrt2*_scale; }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return Math.Log(2.0*Constants.E*_scale); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return double.NegativeInfinity; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return Math.Exp(-Math.Abs(x - _location)/_scale)/(2.0*_scale);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return -Math.Abs(x - _location)/_scale - Math.Log(2.0*_scale);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return 0.5*(1.0 + (Math.Sign(x - _location)*(1.0 - Math.Exp(-Math.Abs(x - _location)/_scale))));
        }

        /// <summary>
        /// Samples a Laplace distributed random variable.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _location, _scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _location, _scale);
        }

        /// <summary>
        /// Generates a sample from the Laplace distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _location, _scale);
        }

        static double SampleUnchecked(System.Random rnd, double location, double scale)
        {
            var u = rnd.NextDouble() - 0.5;
            return location - (scale*Math.Sign(u)*Math.Log(1.0 - (2.0*Math.Abs(u))));
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double location, double scale)
        {
            rnd.NextDoubles(values);
            CommonParallel.For(0, values.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    var u = values[i] - 0.5;
                    values[i] = location - (scale*Math.Sign(u)*Math.Log(1.0 - (2.0*Math.Abs(u))));
                }
            });
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double location, double scale)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, location, scale);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double location, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return Math.Exp(-Math.Abs(x - location)/scale)/(2.0*scale);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double location, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return -Math.Abs(x - location)/scale - Math.Log(2.0*scale);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double location, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return 0.5*(1.0 + (Math.Sign(x - location)*(1.0 - Math.Exp(-Math.Abs(x - location)/scale))));
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, location, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SamplesUnchecked(rnd, location, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            SamplesUnchecked(rnd, values, location, scale);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(SystemRandomSource.Default, location, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SamplesUnchecked(SystemRandomSource.Default, location, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (b) of the distribution. Range: b > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            SamplesUnchecked(SystemRandomSource.Default, values, location, scale);
        }
    }
}
