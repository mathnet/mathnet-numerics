// <copyright file="BetaScaled.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
    public class BetaScaled : IContinuousDistribution
    {
        System.Random _random;

        readonly double _shapeA;
        readonly double _shapeB;
        readonly double _location;
        readonly double _scale;

        /// <summary>
        /// Initializes a new instance of the BetaScaled class.
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        public BetaScaled(double a, double b, double location, double scale)
        {
            if (!IsValidParameterSet(a, b, location, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _shapeA = a;
            _shapeB = b;
            _location = location;
            _scale = scale;
        }

        /// <summary>
        /// Initializes a new instance of the BetaScaled class.
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public BetaScaled(double a, double b, double location, double scale, System.Random randomSource)
        {
            if (!IsValidParameterSet(a, b, location, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _shapeA = a;
            _shapeB = b;
            _location = location;
            _scale = scale;
        }

        /// <summary>
        /// Create a Beta PERT distribution, used in risk analysis and other domains where an expert forecast
        /// is used to construct an underlying beta distribution.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="likely">The most likely value (mode).</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <returns>The Beta distribution derived from the PERT parameters.</returns>
        public static BetaScaled PERT(double min, double max, double likely, System.Random randomSource = null)
        {
            if (min > max || likely > max || likely < min)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            // specified to make the formulas match the literature;
            // traditionally set to 4 so that the range between min and max
            // represents six standard deviations (sometimes called
            // "the six-sigma assumption").
            const double lambda = 4;

            // calculate the mean
            double mean = (min + max + lambda * likely) / (lambda + 2);

            // derive the shape parameters a and b
            double a;

            // special case where mean and mode are identical
            if (mean == likely)
            {
                a = (lambda / 2) + 1;
            }
            else
            {
                a = ((mean - min) * (2 * likely - min - max)) / ((likely - mean) * (max - min));
            }

            double b = (a * (max - mean)) / (mean - min);

            return new BetaScaled(a, b, min, max - min, randomSource);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>A string representation of the BetaScaled distribution.</returns>
        public override string ToString()
        {
            return $"BetaScaled(α = {_shapeA}, β = {_shapeB}, μ = {_location}, σ = {_scale})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        public static bool IsValidParameterSet(double a, double b, double location, double scale)
        {
            return a > 0.0 && b > 0.0 && scale > 0.0 && !double.IsNaN(location);
        }

        /// <summary>
        /// Gets the α shape parameter of the BetaScaled distribution. Range: α > 0.
        /// </summary>
        public double A => _shapeA;

        /// <summary>
        /// Gets the β shape parameter of the BetaScaled distribution. Range: β > 0.
        /// </summary>
        public double B => _shapeB;

        /// <summary>
        /// Gets the location (μ) of the BetaScaled distribution.
        /// </summary>
        public double Location => _location;

        /// <summary>
        /// Gets the scale (σ) of the BetaScaled distribution. Range: σ > 0.
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
        /// Gets the mean of the BetaScaled distribution.
        /// </summary>
        public double Mean
        {
            get
            {
                if (double.IsPositiveInfinity(_shapeA) && double.IsPositiveInfinity(_shapeB))
                {
                    return _location + 0.5 * _scale;
                }

                if (double.IsPositiveInfinity(_shapeA))
                {
                    return _location + _scale;
                }

                if (double.IsPositiveInfinity(_shapeB))
                {
                    return _location;
                }

                return (_shapeB*_location + _shapeA*(_location + _scale))/(_shapeA + _shapeB);
            }
        }

        /// <summary>
        /// Gets the variance of the BetaScaled distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                double sum = _shapeA + _shapeB;
                return (_shapeA*_shapeB*_scale*_scale)/(sum*sum*(1.0 + sum));
            }
        }

        /// <summary>
        /// Gets the standard deviation of the BetaScaled distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(Variance);

        /// <summary>
        /// Gets the entropy of the BetaScaled distribution.
        /// </summary>
        public double Entropy => throw new NotSupportedException();

        /// <summary>
        /// Gets the skewness of the BetaScaled distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (double.IsPositiveInfinity(_shapeA) && double.IsPositiveInfinity(_shapeB))
                {
                    return 0.0;
                }

                if (double.IsPositiveInfinity(_shapeA))
                {
                    return -2.0*_scale/Math.Sqrt(_shapeB*_scale*_scale);
                }

                if (double.IsPositiveInfinity(_shapeB))
                {
                    return 2.0*_scale/Math.Sqrt(_shapeA*_scale*_scale);
                }

                double sum = _shapeA + _shapeB;
                double variance = (_shapeA * _shapeB * _scale * _scale) / (sum * sum * (1.0 + sum));
                return 2.0*(_shapeB - _shapeA)*_scale/(sum*(2.0 + sum)*Math.Sqrt(variance));
            }
        }

        /// <summary>
        /// Gets the mode of the BetaScaled distribution; when there are multiple answers, this routine will return 0.5.
        /// </summary>
        public double Mode
        {
            get
            {
                if (double.IsPositiveInfinity(_shapeA) && double.IsPositiveInfinity(_shapeB))
                {
                    return _location + 0.5 * _scale;
                }

                if (double.IsPositiveInfinity(_shapeA))
                {
                    return _location + _scale;
                }

                if (double.IsPositiveInfinity(_shapeB))
                {
                    return _location;
                }

                if (_shapeA == 1.0 && _shapeB == 1.0)
                {
                    return _location + 0.5 * _scale;
                }

                return ((_shapeA - 1)/(_shapeA + _shapeB - 2))*_scale + _location;
            }
        }

        /// <summary>
        /// Gets the median of the BetaScaled distribution.
        /// </summary>
        public double Median => throw new NotSupportedException();

        /// <summary>
        /// Gets the minimum of the BetaScaled distribution.
        /// </summary>
        public double Minimum => _location;

        /// <summary>
        /// Gets the maximum of the BetaScaled distribution.
        /// </summary>
        public double Maximum => _location + _scale;

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return PDF(_shapeA, _shapeB, _location, _scale, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return PDFLn(_shapeA, _shapeB, _location, _scale, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CDF(_shapeA, _shapeB, _location, _scale, x);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InvCDF"/>
        /// <remarks>WARNING: currently not an explicit implementation, hence slow and unreliable.</remarks>
        public double InverseCumulativeDistribution(double p)
        {
            return InvCDF(_shapeA, _shapeB, _location, _scale, p);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _shapeA, _shapeB, _location, _scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _shapeA, _shapeB, _location, _scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _shapeA, _shapeB, _location, _scale);
        }

        static double SampleUnchecked(System.Random rnd, double a, double b, double location, double scale)
        {
            return Beta.SampleUnchecked(rnd, a, b)*scale + location;
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double a, double b, double location, double scale)
        {
            Beta.SamplesUnchecked(rnd, values, a, b);
            CommonParallel.For(0, values.Length, 4096, (aa, bb) =>
            {
                for (int i = aa; i < bb; i++)
                {
                    values[i] = values[i]*scale + location;
                }
            });
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double a, double b, double location, double scale)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, a, b, location, scale);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double a, double b, double location, double scale, double x)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Beta.PDF(a, b, (x - location)/scale)/Math.Abs(scale);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double a, double b, double location, double scale, double x)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Beta.PDFLn(a, b, (x - location)/scale) - Math.Log(Math.Abs(scale));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double a, double b, double location, double scale, double x)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Beta.CDF(a, b, (x - location) / scale);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        /// <remarks>WARNING: currently not an explicit implementation, hence slow and unreliable.</remarks>
        public static double InvCDF(double a, double b, double location, double scale, double p)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Beta.InvCDF(a, b, p)*scale + location;
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double a, double b, double location, double scale)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, a, b, location, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double a, double b, double location, double scale)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, a, b, location, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double a, double b, double location, double scale)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, a, b, location, scale);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double a, double b, double location, double scale)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, a, b, location, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double a, double b, double location, double scale)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, a, b, location, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="a">The α shape parameter of the BetaScaled distribution. Range: α > 0.</param>
        /// <param name="b">The β shape parameter of the BetaScaled distribution. Range: β > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double a, double b, double location, double scale)
        {
            if (!(a > 0.0 && b > 0.0 && scale > 0.0) || double.IsNaN(location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, a, b, location, scale);
        }
    }
}
