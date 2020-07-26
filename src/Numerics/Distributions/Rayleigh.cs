// <copyright file="Rayleigh.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Random;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Rayleigh distribution.
    /// The Rayleigh distribution (pronounced /ˈreɪli/) is a continuous probability distribution. As an
    /// example of how it arises, the wind speed will have a Rayleigh distribution if the components of
    /// the two-dimensional wind velocity vector are uncorrelated and normally distributed with equal variance.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Rayleigh_distribution">Wikipedia - Rayleigh distribution</a>.
    /// </summary>
    public class Rayleigh : IContinuousDistribution
    {
        System.Random _random;

        readonly double _scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rayleigh"/> class.
        /// </summary>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <exception cref="ArgumentException">If <paramref name="scale"/> is negative.</exception>
        public Rayleigh(double scale)
        {
            if (!IsValidParameterSet(scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _scale = scale;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rayleigh"/> class.
        /// </summary>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentException">If <paramref name="scale"/> is negative.</exception>
        public Rayleigh(double scale, System.Random randomSource)
        {
            if (!IsValidParameterSet(scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _scale = scale;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Rayleigh(σ = {_scale})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        public static bool IsValidParameterSet(double scale)
        {
            return scale > 0.0;
        }

        /// <summary>
        /// Gets the scale (σ) of the distribution. Range: σ > 0.
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
        public double Mean => _scale*Math.Sqrt(Constants.PiOver2);

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => (2.0 - Constants.PiOver2)*_scale*_scale;

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(2.0 - Constants.PiOver2)*_scale;

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy => 1.0 + Math.Log(_scale/Constants.Sqrt2) + (Constants.EulerMascheroni/2.0);

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => (2.0*Math.Sqrt(Constants.Pi)*(Constants.Pi - 3.0))/Math.Pow(4.0 - Constants.Pi, 1.5);

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode => _scale;

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => _scale*Math.Sqrt(Math.Log(4.0));

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum => 0.0;

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
            return (x/(_scale*_scale))*Math.Exp(-x*x/(2.0*_scale*_scale));
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return Math.Log(x/(_scale*_scale)) - (x*x/(2.0*_scale*_scale));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return 1.0 - Math.Exp(-x*x/(2.0*_scale*_scale));
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
            return _scale*Math.Sqrt(-2*Math.Log(1 - p));
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Rayleigh distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _scale);
        }

        static double SampleUnchecked(System.Random rnd, double scale)
        {
            return scale*Math.Sqrt(-2.0*Math.Log(rnd.NextDouble()));
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double scale)
        {
            return rnd.NextDoubleSequence().Select(x => scale*Math.Sqrt(-2.0*Math.Log(x)));
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double scale)
        {
            rnd.NextDoubles(values);
            CommonParallel.For(0, values.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    values[i] = scale*Math.Sqrt(-2.0*Math.Log(values[i]));
                }
            });
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return (x/(scale*scale))*Math.Exp(-x*x/(2.0*scale*scale));
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Math.Log(x/(scale*scale)) - (x*x/(2.0*scale*scale));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return 1.0 - Math.Exp(-x*x/(2.0*scale*scale));
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public static double InvCDF(double scale, double p)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return scale*Math.Sqrt(-2*Math.Log(1 - p));
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, scale);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, scale);
        }
    }
}
