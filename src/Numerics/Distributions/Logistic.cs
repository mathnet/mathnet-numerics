// <copyright file="Logistic.cs" company="Math.NET">
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

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Logistic distribution.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Logistic_distribution">Wikipedia - Logistic distribution</a>.
    /// </summary>
    public class Logistic : IContinuousDistribution
    {
        System.Random _random;

        readonly double _mean;
        readonly double _scale;

        /// <summary>
        /// Initializes a new instance of the Logistic class. This is a logistic distribution with mean 0.0
        /// and scale 1.0. The distribution will be initialized with the default <seealso cref="System.Random"/>
        /// random number generator.
        /// </summary>
        public Logistic()
            : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Logistic class. This is a logistic distribution with mean 0.0
        /// and scale 1.0. The distribution will be initialized with the default <seealso cref="System.Random"/>
        /// random number generator.
        /// </summary>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Logistic(System.Random randomSource)
            : this(0.0, 1.0, randomSource)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Logistic class with a particular mean and scale parameter. The
        /// distribution will be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        public Logistic(double mean, double scale)
        {
            if (!IsValidParameterSet(mean, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _mean = mean;
            _scale = scale;
        }

        /// <summary>
        /// Initializes a new instance of the Logistic class with a particular mean and standard deviation. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Logistic(double mean, double scale, System.Random randomSource)
        {
            if (!IsValidParameterSet(mean, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _mean = mean;
            _scale = scale;
        }

        /// <summary>
        /// Constructs a logistic distribution from a mean and scale parameter.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>a logistic distribution.</returns>
        public static Logistic WithMeanScale(double mean, double scale, System.Random randomSource = null)
        {
            return new Logistic(mean, scale, randomSource);
        }

        /// <summary>
        /// Constructs a logistic distribution from a mean and standard deviation.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the logistic distribution. Range: σ > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>a logistic distribution.</returns>
        public static Logistic WithMeanStdDev(double mean, double stddev, System.Random randomSource = null)
        {
            var scale = Math.Sqrt(3) * stddev / Math.PI;
            return new Logistic(mean, scale, randomSource);
        }

        /// <summary>
        /// Constructs a logistic distribution from a mean and variance.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="var">The variance (σ^2) of the logistic distribution. Range: (σ^2) > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>A logistic distribution.</returns>
        public static Logistic WithMeanVariance(double mean, double var, System.Random randomSource = null)
        {
            return WithMeanStdDev(mean, Math.Sqrt(var), randomSource);
        }

        /// <summary>
        /// Constructs a logistic distribution from a mean and precision.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="precision">The precision of the logistic distribution.  Range: precision > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>A logistic distribution.</returns>
        public static Logistic WithMeanPrecision(double mean, double precision, System.Random randomSource = null)
        {
            return WithMeanVariance(mean, 1 / precision, randomSource);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Logistic(μ = {_mean}, s = {_scale})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        public static bool IsValidParameterSet(double mean, double scale)
        {
            return scale > 0.0 && !double.IsNaN(mean);
        }

        /// <summary>
        /// Gets the scale parameter of the Logistic distribution. Range: s > 0.
        /// </summary>
        public double Scale => _scale;

        /// <summary>
        /// Gets the mean (μ) of the logistic distribution.
        /// </summary>
        public double Mean => _mean;

        /// <summary>
        /// Gets the standard deviation (σ) of the logistic distribution. Range: σ > 0.
        /// </summary>
        public double StdDev => Math.Sqrt(Variance);

        /// <summary>
        /// Gets the variance of the logistic distribution.
        /// </summary>
        public double Variance => (Math.Pow(_scale, 2) * Math.Pow(Math.PI,2))/3;

        /// <summary>
        /// Gets the precision of the logistic distribution.
        /// </summary>
        public double Precision => 1.0/Variance;

        /// <summary>
        /// Gets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the entropy of the logistic distribution.
        /// </summary>
        public double Entropy => Math.Log(_scale) + 2;

        /// <summary>
        /// Gets the skewness of the logistic distribution.
        /// </summary>
        public double Skewness => 0.0;

        /// <summary>
        /// Gets the mode of the logistic distribution.
        /// </summary>
        public double Mode => _mean;

        /// <summary>
        /// Gets the median of the logistic distribution.
        /// </summary>
        public double Median => _mean;

        /// <summary>
        /// Gets the minimum of the logistic distribution.
        /// </summary>
        public double Minimum => double.NegativeInfinity;

        /// <summary>
        /// Gets the maximum of the logistic distribution.
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
            return PDF(_mean, _scale, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return PDFLn(_mean, _scale, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CDF(_mean, _scale, x);
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
            return InvCDF(_mean, _scale, p);
        }

        /// <summary>
        /// Generates a sample from the logistic distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _mean, _scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _mean, _scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the logistic distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _mean, _scale);
        }

        static double SampleUnchecked(System.Random rnd, double mean, double scale)
        {
            return InvCDF(mean, scale, rnd.NextDouble());
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double mean, double scale)
        {
            while (true)
            {
                yield return InvCDF(mean, scale, rnd.NextDouble());
            }
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double mean, double scale)
        {
            if (values.Length == 0)
            {
                return;
            }

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = SampleUnchecked(rnd, mean, scale);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double mean, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var z = (x - mean)/scale;
            return Math.Exp(-z) / (scale * Math.Pow(1.0 + Math.Exp(-z), 2));
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double mean, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var z = (x - mean)/scale;
            return -z - Math.Log(scale) - (2 * Math.Log(1+Math.Exp(-z)));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        /// <remarks>MATLAB: normcdf</remarks>
        public static double CDF(double mean, double scale, double x)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            var z = (x - mean)/scale;
            return 1 / (1 + Math.Exp(-z));
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        /// <remarks>MATLAB: norminv</remarks>
        public static double InvCDF(double mean, double scale, double p)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return mean + (scale*Math.Log(p / (1-p)));
        }

        /// <summary>
        /// Generates a sample from the logistic distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double mean, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, mean, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the logistic distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double mean, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, mean, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double mean, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, mean, scale);
        }

        /// <summary>
        /// Generates a sample from the logistic distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double mean, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, mean, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the logistic distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double mean, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, mean, scale);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="mean">The mean (μ) of the logistic distribution.</param>
        /// <param name="scale">The scale (s) of the logistic distribution. Range: s > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double mean, double scale)
        {
            if (scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, mean, scale);
        }
    }
}
