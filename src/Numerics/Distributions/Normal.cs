// <copyright file="Normal.cs" company="Math.NET">
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
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Normal distribution, also known as Gaussian distribution.
    /// For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Normal_distribution">Wikipedia - Normal distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Normal : IContinuousDistribution
    {
        System.Random _random;

        double _mean;
        double _stdDev;

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
            _random = new System.Random();
            SetParameters(mean, stddev);
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
            _random = randomSource ?? new System.Random();
            SetParameters(mean, stddev);
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
        public static Normal Estimate(IEnumerable<double> samples, System.Random randomSource = null)
        {
            var meanVariance = samples.MeanVariance();
            return new Normal(meanVariance.Item1, Math.Sqrt(meanVariance.Item2), randomSource);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Normal(μ = " + _mean + ", σ = " + _stdDev + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double mean, double stddev)
        {
            if (stddev < 0.0 || Double.IsNaN(mean) || Double.IsNaN(stddev))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _mean = mean;
            _stdDev = stddev;
        }

        /// <summary>
        /// Gets or sets the mean (μ) of the normal distribution.
        /// </summary>
        public double Mean
        {
            get { return _mean; }
            set { SetParameters(value, _stdDev); }
        }

        /// <summary>
        /// Gets or sets the standard deviation (σ) of the normal distribution. Range: σ ≥ 0.
        /// </summary>
        public double StdDev
        {
            get { return _stdDev; }
            set { SetParameters(_mean, value); }
        }

        /// <summary>
        /// Gets or sets the variance of the normal distribution.
        /// </summary>
        public double Variance
        {
            get { return _stdDev*_stdDev; }
            set { SetParameters(_mean, Math.Sqrt(value)); }
        }

        /// <summary>
        /// Gets or sets the precision of the normal distribution.
        /// </summary>
        public double Precision
        {
            get { return 1.0/(_stdDev*_stdDev); }
            set
            {
                var sdev = 1.0/Math.Sqrt(value);
                // Handle the case when the precision is -0.
                if (Double.IsInfinity(sdev))
                {
                    sdev = Double.PositiveInfinity;
                }
                SetParameters(_mean, sdev);
            }
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get { return _random; }
            set { _random = value ?? new System.Random(); }
        }

        /// <summary>
        /// Gets the entropy of the normal distribution.
        /// </summary>
        public double Entropy
        {
            get { return Math.Log(_stdDev) + Constants.LogSqrt2PiE; }
        }

        /// <summary>
        /// Gets the skewness of the normal distribution.
        /// </summary>
        public double Skewness
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the mode of the normal distribution.
        /// </summary>
        public double Mode
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the median of the normal distribution.
        /// </summary>
        public double Median
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the minimum of the normal distribution.
        /// </summary>
        public double Minimum
        {
            get { return Double.NegativeInfinity; }
        }

        /// <summary>
        /// Gets the maximum of the normal distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
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
        public double CumulativeDistribution(double x)
        {
            return 0.5*(1.0 + SpecialFunctions.Erf((x - _mean)/(_stdDev*Constants.Sqrt2)));
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
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
            return _mean + (_stdDev*SampleStandardBoxMuller(_random).Item1);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                var sample = SampleStandardBoxMuller(_random);
                yield return _mean + (_stdDev*sample.Item1);
                yield return _mean + (_stdDev*sample.Item2);
            }
        }

        /// <summary>
        /// Samples a pair of standard normal distributed random variables using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <returns>a pair of random numbers from the standard normal distribution.</returns>
        static Tuple<double, double> SampleStandardBoxMuller(System.Random rnd)
        {
            var v1 = (2.0 * rnd.NextDouble()) - 1.0;
            var v2 = (2.0 * rnd.NextDouble()) - 1.0;
            var r = (v1 * v1) + (v2 * v2);
            while (r >= 1.0 || r == 0.0)
            {
                v1 = (2.0 * rnd.NextDouble()) - 1.0;
                v2 = (2.0 * rnd.NextDouble()) - 1.0;
                r = (v1 * v1) + (v2 * v2);
            }

            var fac = Math.Sqrt(-2.0 * Math.Log(r) / r);
            return new Tuple<double, double>(v1 * fac, v2 * fac);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public static double PDF(double mean, double stddev, double x)
        {
            if (stddev < 0.0) throw new ArgumentOutOfRangeException("stddev", Resources.InvalidDistributionParameters);

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
        public static double PDFLn(double mean, double stddev, double x)
        {
            if (stddev < 0.0) throw new ArgumentOutOfRangeException("stddev", Resources.InvalidDistributionParameters);

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
        public static double CDF(double mean, double stddev, double x)
        {
            if (stddev < 0.0) throw new ArgumentOutOfRangeException("stddev", Resources.InvalidDistributionParameters);

            return 0.5*(1.0 + SpecialFunctions.Erf((x - mean)/(stddev*Constants.Sqrt2)));
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        public static double InvCDF(double mean, double stddev, double p)
        {
            if (stddev < 0.0) throw new ArgumentOutOfRangeException("stddev", Resources.InvalidDistributionParameters);

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
            if (stddev < 0.0) throw new ArgumentOutOfRangeException("stddev", Resources.InvalidDistributionParameters);

            return mean + (stddev*SampleStandardBoxMuller(rnd).Item1);
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
            if (stddev < 0.0) throw new ArgumentOutOfRangeException("stddev", Resources.InvalidDistributionParameters);

            while (true)
            {
                var sample = SampleStandardBoxMuller(rnd);
                yield return mean + (stddev*sample.Item1);
                yield return mean + (stddev*sample.Item2);
            }
        }
    }
}
