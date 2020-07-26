// <copyright file="ContinuousUniform.cs" company="Math.NET">
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
    /// Continuous Univariate Uniform distribution.
    /// The continuous uniform distribution is a distribution over real numbers. For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Uniform_distribution_%28continuous%29">Wikipedia - Continuous uniform distribution</a>.
    /// </summary>
    public class ContinuousUniform : IContinuousDistribution
    {
        System.Random _random;

        readonly double _lower;
        readonly double _upper;

        /// <summary>
        /// Initializes a new instance of the ContinuousUniform class with lower bound 0 and upper bound 1.
        /// </summary>
        public ContinuousUniform() : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ContinuousUniform class with given lower and upper bounds.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <exception cref="ArgumentException">If the upper bound is smaller than the lower bound.</exception>
        public ContinuousUniform(double lower, double upper)
        {
            if (!IsValidParameterSet(lower, upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _lower = lower;
            _upper = upper;
        }

        /// <summary>
        /// Initializes a new instance of the ContinuousUniform class with given lower and upper bounds.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentException">If the upper bound is smaller than the lower bound.</exception>
        public ContinuousUniform(double lower, double upper, System.Random randomSource)
        {
            if (!IsValidParameterSet(lower, upper))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _lower = lower;
            _upper = upper;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"ContinuousUniform(Lower = {_lower}, Upper = {_upper})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        public static bool IsValidParameterSet(double lower, double upper)
        {
            return lower <= upper;
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
        public double Mean => (_lower + _upper)/2.0;

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => (_upper - _lower)*(_upper - _lower)/12.0;

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => (_upper - _lower)/Math.Sqrt(12.0);

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        /// <value></value>
        public double Entropy => Math.Log(_upper - _lower);

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => 0.0;

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        /// <value></value>
        public double Mode => (_lower + _upper)/2.0;

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <value></value>
        public double Median => (_lower + _upper)/2.0;

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
            return x < _lower || x > _upper ? 0.0 : 1.0/(_upper - _lower);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return x < _lower || x > _upper ? double.NegativeInfinity : -Math.Log(_upper - _lower);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return x <= _lower ? 0.0 : x >= _upper ? 1.0 : (x - _lower)/(_upper - _lower);
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
            return p <= 0.0 ? _lower : p >= 1.0 ? _upper : _lower*(1.0 - p) + _upper*p;
        }

        /// <summary>
        /// Generates a sample from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _lower, _upper);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _lower, _upper);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _lower, _upper);
        }

        static double SampleUnchecked(System.Random rnd, double lower, double upper)
        {
            return lower + rnd.NextDouble()*(upper - lower);
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double lower, double upper)
        {
            double difference = upper - lower;
            while (true)
            {
                yield return lower + rnd.NextDouble()*difference;
            }
        }

        internal static void SamplesUnchecked(System.Random rnd, double[] values, double lower, double upper)
        {
            rnd.NextDoubles(values);
            var difference = upper - lower;
            CommonParallel.For(0, values.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    values[i] = lower + values[i]*difference;
                }
            });
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double lower, double upper, double x)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return x < lower || x > upper ? 0.0 : 1.0/(upper - lower);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double lower, double upper, double x)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return x < lower || x > upper ? double.NegativeInfinity : -Math.Log(upper - lower);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double lower, double upper, double x)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return x <= lower ? 0.0 : x >= upper ? 1.0 : (x - lower)/(upper - lower);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public static double InvCDF(double lower, double upper, double p)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return p <= 0.0 ? lower : p >= 1.0 ? upper : lower*(1.0 - p) + upper*p;
        }

        /// <summary>
        /// Generates a sample from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>a uniformly distributed sample.</returns>
        public static double Sample(System.Random rnd, double lower, double upper)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, lower, upper);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>a sequence of uniformly distributed samples.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double lower, double upper)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, lower, upper);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double lower, double upper)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, lower, upper);
        }

        /// <summary>
        /// Generates a sample from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>a uniformly distributed sample.</returns>
        public static double Sample(double lower, double upper)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, lower, upper);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>a sequence of uniformly distributed samples.</returns>
        public static IEnumerable<double> Samples(double lower, double upper)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, lower, upper);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="lower">Lower bound. Range: lower ≤ upper.</param>
        /// <param name="upper">Upper bound. Range: lower ≤ upper.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double lower, double upper)
        {
            if (upper < lower)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, lower, upper);
        }
    }
}
