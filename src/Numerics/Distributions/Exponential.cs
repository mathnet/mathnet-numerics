// <copyright file="Exponential.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Random;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Exponential distribution.
    /// The exponential distribution is a distribution over the real numbers parameterized by one non-negative parameter.
    /// <a href="http://en.wikipedia.org/wiki/Exponential_distribution">Wikipedia - exponential distribution</a>.
    /// </summary>
    public class Exponential : IContinuousDistribution
    {
        System.Random _random;

        readonly double _rate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Exponential"/> class.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        public Exponential(double rate)
        {
            if (!IsValidParameterSet(rate))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _rate = rate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exponential"/> class.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Exponential(double rate, System.Random randomSource)
        {
            if (!IsValidParameterSet(rate))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _rate = rate;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Exponential(λ = {_rate})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        public static bool IsValidParameterSet(double rate)
        {
            return rate >= 0.0;
        }

        /// <summary>
        /// Gets the rate (λ) parameter of the distribution. Range: λ ≥ 0.
        /// </summary>
        public double Rate => _rate;

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
        public double Mean => 1.0/_rate;

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => 1.0/(_rate*_rate);

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => 1.0/_rate;

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy => 1.0 - Math.Log(_rate);

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => 2.0;

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode => 0.0;

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => Math.Log(2.0)/_rate;

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
            return x < 0.0 ? 0.0 : _rate*Math.Exp(-_rate*x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return Math.Log(_rate) - (_rate*x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return x < 0.0 ? 0.0 : 1.0 - Math.Exp(-_rate*x);
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
            return p >= 1.0 ? double.PositiveInfinity : -Math.Log(1 - p)/_rate;
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Exponential distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _rate);
        }

        static double SampleUnchecked(System.Random rnd, double rate)
        {
            var r = rnd.NextDouble();
            while (r == 0.0)
            {
                r = rnd.NextDouble();
            }

            return -Math.Log(r)/rate;
        }

        internal static void SamplesUnchecked(System.Random rnd, double[] values, double rate)
        {
            rnd.NextDoubles(values);
            CommonParallel.For(0, values.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    // this happens very rarely
                    var r = values[i];
                    while (r == 0.0)
                    {
                        r = rnd.NextDouble();
                    }

                    values[i] = -Math.Log(r)/rate;
                }
            });
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double rate)
        {
            return rnd.NextDoubleSequence().Where(r => r != 0.0).Select(r => -Math.Log(r)/rate);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double rate, double x)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return x < 0.0 ? 0.0 : rate*Math.Exp(-rate*x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double rate, double x)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Math.Log(rate) - (rate*x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double rate, double x)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return x < 0.0 ? 0.0 : 1.0 - Math.Exp(-rate*x);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public static double InvCDF(double rate, double p)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return p >= 1.0 ? double.PositiveInfinity : -Math.Log(1 - p)/rate;
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <returns>A random number from this distribution.</returns>
        public static double Sample(System.Random rnd, double rate)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double rate)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Exponential distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double rate)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, rate);
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <returns>A random number from this distribution.</returns>
        public static double Sample(double rate)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double rate)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Exponential distribution.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double rate)
        {
            if (rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, rate);
        }
    }
}
