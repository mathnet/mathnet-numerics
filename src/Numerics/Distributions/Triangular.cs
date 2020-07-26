// <copyright file="Triangular.cs" company="Math.NET">
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
    /// Triangular distribution.
    /// For details, see <a href="https://en.wikipedia.org/wiki/Triangular_distribution">Wikipedia - Triangular distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default.
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check whether all the incoming parameters are in the allowed range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Triangular : IContinuousDistribution
    {
        System.Random _random;

        readonly double _lower;
        readonly double _upper;
        readonly double _mode;

        /// <summary>
        /// Initializes a new instance of the Triangular class with the given lower bound, upper bound and mode.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <exception cref="ArgumentException">If the upper bound is smaller than the mode or if the mode is smaller than the lower bound.</exception>
        public Triangular(double lower, double upper, double mode)
        {
            if (!IsValidParameterSet(lower, upper, mode))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _lower = lower;
            _upper = upper;
            _mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the Triangular class with the given lower bound, upper bound and mode.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentException">If the upper bound is smaller than the mode or if the mode is smaller than the lower bound.</exception>
        public Triangular(double lower, double upper, double mode, System.Random randomSource)
        {
            if (!IsValidParameterSet(lower, upper, mode))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _lower = lower;
            _upper = upper;
            _mode = mode;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Triangular(Lower = {_lower}, Upper = {_upper}, Mode = {_mode})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        public static bool IsValidParameterSet(double lower, double upper, double mode)
        {
            return upper >= mode && mode >= lower && !double.IsInfinity(upper) && !double.IsInfinity(lower) && !double.IsInfinity(mode);
        }

        /// <summary>
        /// Gets the lower bound of the distribution.
        /// </summary>
        public double LowerBound => _lower;

        /// <summary>
        /// Gets the upper bound of the distribution.
        /// </summary>
        public double UpperBound => _upper;

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
        public double Mean => (_lower + _upper + _mode)/3.0;

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                var a = _lower;
                var b = _upper;
                var c = _mode;
                return (a*a + b*b + c*c - a*b - a*c - b*c)/18.0;
            }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(Variance);

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        /// <value></value>
        public double Entropy => 0.5 + Math.Log((_upper - _lower)/2);

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                var a = _lower;
                var b = _upper;
                var c = _mode;
                var q = Math.Sqrt(2)*(a + b - 2*c)*(2*a - b - c)*(a - 2*b + c);
                var d = 5*Math.Pow(a*a + b*b + c*c - a*b - a*c - b*c, 3.0/2);
                return q/d;
            }
        }

        /// <summary>
        /// Gets or sets the mode of the distribution.
        /// </summary>
        public double Mode => _mode;

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <value></value>
        public double Median
        {
            get
            {
                var a = _lower;
                var b = _upper;
                var c = _mode;
                return c >= (a + b)/2
                    ? a + Math.Sqrt((b - a)*(c - a)/2)
                    : b - Math.Sqrt((b - a)*(b - c)/2);
            }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum => _lower;

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum => _upper;

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return PDF(_lower, _upper, _mode, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return PDFLn(_lower, _upper, _mode, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CDF(_lower, _upper, _mode, x);
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
            return InvCDF(_lower, _upper, _mode, p);
        }

        /// <summary>
        /// Generates a sample from the <c>Triangular</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _lower, _upper, _mode);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _lower, _upper, _mode);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>Triangular</c> distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _lower, _upper, _mode);
        }

        static double SampleUnchecked(System.Random rnd, double lower, double upper, double mode)
        {
            var u = rnd.NextDouble();
            return u < (mode - lower)/(upper - lower)
                ? lower + Math.Sqrt(u*(upper - lower)*(mode - lower))
                : upper - Math.Sqrt((1 - u)*(upper - lower)*(upper - mode));
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double lower, double upper, double mode)
        {
            double ml = mode - lower, ul = upper - lower, um = upper - mode;
            double u = ml/ul, v = ul*ml, w = ul*um;

            return rnd.NextDoubleSequence().Select(x => x < u ? lower + Math.Sqrt(x*v) : upper - Math.Sqrt((1 - x)*w));
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double lower, double upper, double mode)
        {
            double ml = mode - lower, ul = upper - lower, um = upper - mode;
            double u = ml/ul, v = ul*ml, w = ul*um;

            rnd.NextDoubles(values);
            CommonParallel.For(0, values.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    values[i] = values[i] < u
                        ? lower + Math.Sqrt(values[i]*v)
                        : upper - Math.Sqrt((1 - values[i])*w);
                }
            });
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double lower, double upper, double mode, double x)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var a = lower;
            var b = upper;
            var c = mode;

            if (a <= x && x <= c)
            {
                return 2*(x - a)/((b - a)*(c - a));
            }

            if (c < x & x <= b)
            {
                return 2*(b - x)/((b - a)*(b - c));
            }

            return 0;
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double lower, double upper, double mode, double x)
        {
            return Math.Log(PDF(lower, upper, mode, x));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double lower, double upper, double mode, double x)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var a = lower;
            var b = upper;
            var c = mode;

            if (x < a)
            {
                return 0;
            }

            if (a <= x && x <= c)
            {
                return (x - a)*(x - a)/((b - a)*(c - a));
            }

            if (c < x & x <= b)
            {
                return 1 - (b - x)*(b - x)/((b - a)*(b - c));
            }

            return 1;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public static double InvCDF(double lower, double upper, double mode, double p)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var a = lower;
            var b = upper;
            var c = mode;

            if (p <= 0)
            {
                return lower;
            }

            // Taken from http://www.ntrand.com/triangular-distribution/
            if (p < (c - a)/(b - a))
            {
                return a + Math.Sqrt(p*(c - a)*(b - a));
            }

            if (p < 1)
            {
                return b - Math.Sqrt((1 - p)*(b - c)*(b - a));
            }

            return upper;
        }

        /// <summary>
        /// Generates a sample from the <c>Triangular</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double lower, double upper, double mode)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, lower, upper, mode);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>Triangular</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double lower, double upper, double mode)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, lower, upper, mode);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double lower, double upper, double mode)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, lower, upper, mode);
        }

        /// <summary>
        /// Generates a sample from the <c>Triangular</c> distribution.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double lower, double upper, double mode)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, lower, upper, mode);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>Triangular</c> distribution.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double lower, double upper, double mode)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, lower, upper, mode);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double lower, double upper, double mode)
        {
            if (!(upper >= mode && mode >= lower))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, lower, upper, mode);
        }
    }
}
