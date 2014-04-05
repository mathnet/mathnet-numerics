// <copyright file="Triangular.cs" company="Math.NET">
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

        double _lower;
        double _upper;
        double _mode;

        /// <summary>
        /// Initializes a new instance of the Triangular class with the given lower bound, upper bound and mode.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <exception cref="ArgumentException">If the upper bound is smaller than the mode or if the mode is smaller than the lower bound.</exception>
        public Triangular(double lower, double upper, double mode)
        {
            _random = SystemRandomSource.Default;
            SetParameters(lower, upper, mode);
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
            _random = randomSource ?? SystemRandomSource.Default;
            SetParameters(lower, upper, mode);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Triangular(Lower = " + _lower + ", Upper = " + _upper + ", Mode = " + _mode + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="upper">Upper bound. Range: lower ≤ mode ≤ upper</param>
        /// <param name="mode">Mode (most frequent value).  Range: lower ≤ mode ≤ upper</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double lower, double upper, double mode)
        {
            if (upper < mode || mode < lower || Double.IsNaN(upper) || Double.IsNaN(lower) || Double.IsNaN(mode)
                || Double.IsInfinity(upper) || Double.IsInfinity(lower) || Double.IsInfinity(mode)
                )
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            _lower = lower;
            _upper = upper;
            _mode = mode;
        }

        /// <summary>
        /// Gets or sets the lower bound of the distribution.
        /// </summary>
        public double LowerBound
        {
            get { return _lower; }
            set { SetParameters(value, _upper, _mode); }
        }

        /// <summary>
        /// Gets or sets the upper bound of the distribution.
        /// </summary>
        public double UpperBound
        {
            get { return _upper; }
            set { SetParameters(_lower, value, _mode); }
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
            get { return (_lower + _upper + _mode)/3.0; }
        }

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
        public double StdDev
        {
            get { return Math.Sqrt(Variance); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        /// <value></value>
        public double Entropy
        {
            get { return 0.5 + Math.Log((_upper - _lower)/2); }
        }

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
        public double Mode
        {
            get { return _mode; }
            set { SetParameters(_lower, _upper, value); }
        }

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
        public double Minimum
        {
            get { return _lower; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return _upper; }
        }

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
            return Sample(_random, _lower, _upper, _mode);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>Triangular</c> distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return Samples(_random, _lower, _upper, _mode);
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
            if (!(upper >= mode && mode >= lower)) throw new ArgumentException(Resources.InvalidDistributionParameters);

            var a = lower;
            var b = upper;
            var c = mode;

            if (a <= x && x <= c) return 2*(x - a)/((b - a)*(c - a));
            if (c < x & x <= b) return 2*(b - x)/((b - a)*(b - c));
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
            if (!(upper >= mode && mode >= lower)) throw new ArgumentException(Resources.InvalidDistributionParameters);

            var a = lower;
            var b = upper;
            var c = mode;

            if (x < a) return 0;
            if (a <= x && x <= c) return (x - a)*(x - a)/((b - a)*(c - a));
            if (c < x & x <= b) return 1 - (b - x)*(b - x)/((b - a)*(b - c));
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
            if (!(upper >= mode && mode >= lower)) throw new ArgumentException(Resources.InvalidDistributionParameters);

            var a = lower;
            var b = upper;
            var c = mode;

            if (p <= 0) return lower;
            // Taken from http://www.ntrand.com/triangular-distribution/
            if (p < (c - a)/(b - a)) return a + Math.Sqrt(p*(c - a)*(b - a));
            if (p < 1) return b - Math.Sqrt((1 - p)*(b - c)*(b - a));
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
            if (!(upper >= mode && mode >= lower)) throw new ArgumentException(Resources.InvalidDistributionParameters);

            var a = lower;
            var b = upper;
            var c = mode;
            var u = rnd.NextDouble();

            return u < (c - a)/(b - a)
                ? a + Math.Sqrt(u*(b - a)*(c - a))
                : b - Math.Sqrt((1 - u)*(b - a)*(b - c));
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
            if (!(upper >= mode && mode >= lower)) throw new ArgumentException(Resources.InvalidDistributionParameters);

            while (true)
            {
                yield return Sample(rnd, lower, upper, mode);
            }
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
            return Sample(SystemRandomSource.Default, lower, upper, mode);
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
            return Samples(SystemRandomSource.Default, lower, upper, mode);
        }
    }
}
