// <copyright file="Cauchy.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using MathNet.Numerics.Random;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Cauchy distribution.
    /// The Cauchy distribution is a symmetric continuous probability distribution. For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Cauchy_distribution">Wikipedia - Cauchy distribution</a>.
    /// </summary>
    public class Cauchy : IContinuousDistribution
    {
        System.Random _random;

        readonly double _location;
        readonly double _scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cauchy"/> class with the location parameter set to 0 and the scale parameter set to 1
        /// </summary>
        public Cauchy() : this(0, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cauchy"/> class.
        /// </summary>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        public Cauchy(double location, double scale)
        {
            if (!IsValidParameterSet(location, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _location = location;
            _scale = scale;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cauchy"/> class.
        /// </summary>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Cauchy(double location, double scale, System.Random randomSource)
        {
            if (!IsValidParameterSet(location, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
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
            return $"Cauchy(x0 = {_location}, γ = {_scale})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        public static bool IsValidParameterSet(double location, double scale)
        {
            return scale > 0.0 && !double.IsNaN(location);
        }

        /// <summary>
        /// Gets the location  (x0) of the distribution.
        /// </summary>
        public double Location => _location;

        /// <summary>
        /// Gets the scale (γ) of the distribution. Range: γ > 0.
        /// </summary>
        public double Scale => _scale;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean => throw new NotSupportedException();

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => throw new NotSupportedException();

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => throw new NotSupportedException();

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy => Math.Log(4.0*Constants.Pi*_scale);

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => throw new NotSupportedException();

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode => _location;

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => _location;

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum => double.NegativeInfinity;

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum => double.PositiveInfinity;

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            var z = (x - _location)/_scale;
            return 1.0/(Constants.Pi*_scale*(1.0 + z * z));
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            var z = (x - _location)/_scale;
            return -Math.Log(Constants.Pi*_scale*(1.0 +  z * z));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return Constants.InvPi*Math.Atan((x - _location)/_scale) + 0.5;
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
            return p <= 0.0 ? double.NegativeInfinity : p >= 1.0 ? double.PositiveInfinity
                : _location + _scale*Math.Tan((p - 0.5)*Constants.Pi);
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
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
        /// Generates a sequence of samples from the Cauchy distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _location, _scale);
        }

        static double SampleUnchecked(System.Random rnd, double location, double scale)
        {
            return location + scale*Math.Tan(Constants.Pi*(rnd.NextDouble() - 0.5));
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double location, double scale)
        {
            rnd.NextDoubles(values);
            CommonParallel.For(0, values.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    values[i] = location + scale*Math.Tan(Constants.Pi*(values[i] - 0.5));
                }
            });
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double location, double scale)
        {
            while (true)
            {
                yield return location + scale*Math.Tan(Constants.Pi*(rnd.NextDouble() - 0.5));
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double location, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var z = (x - location)/scale;
            return 1.0/(Constants.Pi*scale*(1.0 + z * z));
        }
        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double location, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var z = (x - location)/scale;
            return -Math.Log(Constants.Pi*scale*(1.0 + z * z ));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double location, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Math.Atan((x - location)/scale)/Constants.Pi + 0.5;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public static double InvCDF(double location, double scale, double p)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return p <= 0.0 ? double.NegativeInfinity : p >= 1.0 ? double.PositiveInfinity
                : location + scale*Math.Tan((p - 0.5)*Constants.Pi);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, location, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, location, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, location, scale);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, location, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, location, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (x0) of the distribution.</param>
        /// <param name="scale">The scale (γ) of the distribution. Range: γ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double location, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, location, scale);
        }
    }
}
