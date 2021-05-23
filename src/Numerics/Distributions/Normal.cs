// <copyright file="Normal.cs" company="Math.NET">
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
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Normal distribution, also known as Gaussian distribution.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Normal_distribution">Wikipedia - Normal distribution</a>.
    /// </summary>
    public class Normal : IContinuousDistribution
    {
        System.Random _random;

        readonly double _mean;
        readonly double _stdDev;

        /// <summary>
        /// Initializes a new instance of the Normal class. This is a normal distribution with mean 0.0
        /// and standard deviation 1.0. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        public Normal()
            : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Normal class. This is a normal distribution with mean 0.0
        /// and standard deviation 1.0. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Normal(System.Random randomSource)
            : this(0.0, 1.0, randomSource)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Normal class with a particular mean and standard deviation. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        public Normal(double mean, double stddev)
        {
            if (!IsValidParameterSet(mean, stddev))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _mean = mean;
            _stdDev = stddev;
        }

        /// <summary>
        /// Initializes a new instance of the Normal class with a particular mean and standard deviation. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Normal(double mean, double stddev, System.Random randomSource)
        {
            if (!IsValidParameterSet(mean, stddev))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _mean = mean;
            _stdDev = stddev;
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and standard deviation.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>a normal distribution.</returns>
        public static Normal WithMeanStdDev(double mean, double stddev, System.Random randomSource = null)
        {
            return new Normal(mean, stddev, randomSource);
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and variance.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="var">The variance (σ^2) of the normal distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>A normal distribution.</returns>
        public static Normal WithMeanVariance(double mean, double var, System.Random randomSource = null)
        {
            return new Normal(mean, Math.Sqrt(var), randomSource);
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and precision.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="precision">The precision of the normal distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>A normal distribution.</returns>
        public static Normal WithMeanPrecision(double mean, double precision, System.Random randomSource = null)
        {
            return new Normal(mean, 1.0/Math.Sqrt(precision), randomSource);
        }

        /// <summary>
        /// Estimates the normal distribution parameters from sample data with maximum-likelihood.
        /// </summary>
        /// <param name="samples">The samples to estimate the distribution parameters from.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>A normal distribution.</returns>
        /// <remarks>MATLAB: normfit</remarks>
        public static Normal Estimate(IEnumerable<double> samples, System.Random randomSource = null)
        {
            var meanStdDev = samples.MeanStandardDeviation();
            return new Normal(meanStdDev.Item1, meanStdDev.Item2, randomSource);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Normal(μ = {_mean}, σ = {_stdDev})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        public static bool IsValidParameterSet(double mean, double stddev)
        {
            return stddev >= 0.0 && !double.IsNaN(mean);
        }

        /// <summary>
        /// Gets the mean (μ) of the normal distribution.
        /// </summary>
        public double Mean => _mean;

        /// <summary>
        /// Gets the standard deviation (σ) of the normal distribution. Range: σ ≥ 0.
        /// </summary>
        public double StdDev => _stdDev;

        /// <summary>
        /// Gets the variance of the normal distribution.
        /// </summary>
        public double Variance => _stdDev*_stdDev;

        /// <summary>
        /// Gets the precision of the normal distribution.
        /// </summary>
        public double Precision => 1.0/(_stdDev*_stdDev);

        /// <summary>
        /// Gets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the entropy of the normal distribution.
        /// </summary>
        public double Entropy => Math.Log(_stdDev) + Constants.LogSqrt2PiE;

        /// <summary>
        /// Gets the skewness of the normal distribution.
        /// </summary>
        public double Skewness => 0.0;

        /// <summary>
        /// Gets the mode of the normal distribution.
        /// </summary>
        public double Mode => _mean;

        /// <summary>
        /// Gets the median of the normal distribution.
        /// </summary>
        public double Median => _mean;

        /// <summary>
        /// Gets the minimum of the normal distribution.
        /// </summary>
        public double Minimum => double.NegativeInfinity;

        /// <summary>
        /// Gets the maximum of the normal distribution.
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
            var d = (x - _mean)/_stdDev;
            return Math.Exp(-0.5*d*d)/(Constants.Sqrt2Pi*_stdDev);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            var d = (x - _mean)/_stdDev;
            return (-0.5*d*d) - Math.Log(_stdDev) - Constants.LogSqrt2Pi;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return 0.5*SpecialFunctions.Erfc((_mean - x)/(_stdDev*Constants.Sqrt2));
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
            return _mean - (_stdDev*Constants.Sqrt2*SpecialFunctions.ErfcInv(2.0*p));
        }

        /// <summary>
        /// Generates a sample from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _mean, _stdDev);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _mean, _stdDev);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _mean, _stdDev);
        }

        internal static double SampleUnchecked(System.Random rnd, double mean, double stddev)
        {
            double x;
            while (!PolarTransform(rnd.NextDouble(), rnd.NextDouble(), out x, out _))
            {
            }

            return mean + (stddev*x);
        }

        internal static IEnumerable<double> SamplesUnchecked(System.Random rnd, double mean, double stddev)
        {
            while (true)
            {
                if (!PolarTransform(rnd.NextDouble(), rnd.NextDouble(), out var x, out var y))
                {
                    continue;
                }

                yield return mean + (stddev*x);
                yield return mean + (stddev*y);
            }
        }

        internal static void SamplesUnchecked(System.Random rnd, double[] values, double mean, double stddev)
        {
            if (values.Length == 0)
            {
                return;
            }

            // Since we only accept points within the unit circle
            // we need to generate roughly 4/pi=1.27 times the numbers needed.
            int n = (int)Math.Ceiling(values.Length*4*Constants.InvPi);
            if (n.IsOdd())
            {
                n++;
            }

            var uniform = rnd.NextDoubles(n);

            // Polar transform
            double x, y;
            int index = 0;
            for (int i = 0; i < uniform.Length && index < values.Length; i += 2)
            {
                if (!PolarTransform(uniform[i], uniform[i + 1], out x, out y))
                {
                    continue;
                }

                values[index++] = mean + stddev*x;
                if (index == values.Length)
                {
                    return;
                }

                values[index++] = mean + stddev*y;
                if (index == values.Length)
                {
                    return;
                }
            }

            // remaining, if any
            while (index < values.Length)
            {
                if (!PolarTransform(rnd.NextDouble(), rnd.NextDouble(), out x, out y))
                {
                    continue;
                }

                values[index++] = mean + stddev*x;
                if (index == values.Length)
                {
                    return;
                }

                values[index++] = mean + stddev*y;
                if (index == values.Length)
                {
                    return;
                }
            }
        }

        static bool PolarTransform(double a, double b, out double x, out double y)
        {
            var v1 = (2.0*a) - 1.0;
            var v2 = (2.0*b) - 1.0;
            var r = (v1*v1) + (v2*v2);
            if (r >= 1.0 || r == 0.0)
            {
                x = 0;
                y = 0;
                return false;
            }

            var fac = Math.Sqrt(-2.0*Math.Log(r)/r);
            x = v1*fac;
            y = v2*fac;
            return true;
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        /// <remarks>MATLAB: normpdf</remarks>
        public static double PDF(double mean, double stddev, double x)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var d = (x - mean)/stddev;
            return Math.Exp(-0.5*d*d)/(Constants.Sqrt2Pi*stddev);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double mean, double stddev, double x)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var d = (x - mean)/stddev;
            return (-0.5*d*d) - Math.Log(stddev) - Constants.LogSqrt2Pi;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        /// <remarks>MATLAB: normcdf</remarks>
        public static double CDF(double mean, double stddev, double x)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return 0.5*SpecialFunctions.Erfc((mean - x)/(stddev*Constants.Sqrt2));
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        /// <remarks>MATLAB: norminv</remarks>
        public static double InvCDF(double mean, double stddev, double p)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return mean - (stddev*Constants.Sqrt2*SpecialFunctions.ErfcInv(2.0*p));
        }

        /// <summary>
        /// Generates a sample from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double mean, double stddev)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, mean, stddev);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double mean, double stddev)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, mean, stddev);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double mean, double stddev)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, mean, stddev);
        }

        /// <summary>
        /// Generates a sample from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double mean, double stddev)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, mean, stddev);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double mean, double stddev)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, mean, stddev);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double mean, double stddev)
        {
            if (stddev < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, mean, stddev);
        }
    }
}
