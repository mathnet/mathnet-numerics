// <copyright file="Exponential.cs" company="Math.NET">
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

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Exponential distribution.
    /// The exponential distribution is a distribution over the real numbers parameterized by one non-negative parameter.
    /// <a href="http://en.wikipedia.org/wiki/Exponential_distribution">Wikipedia - exponential distribution</a>.
    /// </summary>
    /// <remarks>The distribution will use the <see cref="System.Random"/> by default. 
    /// <para>Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Exponential : IContinuousDistribution
    {
        System.Random _random;

        double _rate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Exponential"/> class.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the Exponential distribution.</param>
        public Exponential(double rate)
        {
            _random = new System.Random();
            SetParameters(rate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exponential"/> class.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the Exponential distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Exponential(double rate, System.Random randomSource)
        {
            _random = randomSource ?? new System.Random();
            SetParameters(rate);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Exponential(λ = " + _rate + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the Exponential distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double rate)
        {
            return rate >= 0.0;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="rate">The rate (λ) parameter of the Exponential distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double rate)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(rate))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _rate = rate;
        }

        /// <summary>
        /// Gets or sets the rate (λ) parameter of the distribution.
        /// </summary>
        public double Rate
        {
            get { return _rate; }
            set { SetParameters(value); }
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
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean
        {
            get { return 1.0/_rate; }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return 1.0/(_rate*_rate); }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return 1.0/_rate; }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return 1.0 - Math.Log(_rate); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return 2.0; }
        }

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median
        {
            get { return Math.Log(2.0)/_rate; }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the density of the distribution (PDF), i.e. dP(X &lt;= x)/dx.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            if (x >= 0.0)
            {
                return _rate*Math.Exp(-_rate*x);
            }

            return 0.0;
        }

        /// <summary>
        /// Computes the log density of the distribution (lnPDF), i.e. ln(dP(X &lt;= x)/dx).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return Math.Log(_rate) - (_rate*x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution, i.e. P(X &lt;= x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x >= 0.0)
            {
                return 1.0 - Math.Exp(-_rate*x);
            }

            return 0.0;
        }

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="rate">The rate (λ) parameter of the Exponential distribution.</param>
        /// <returns>a random number from the distribution.</returns>
        internal static double SampleUnchecked(System.Random rnd, double rate)
        {
            var r = rnd.NextDouble();
            while (r == 0.0)
            {
                r = rnd.NextDouble();
            }

            return -Math.Log(r)/rate;
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Exponential distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _rate);
            }
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="rate">The rate (λ) parameter of the Exponential distribution.</param>
        /// <returns>A random number from this distribution.</returns>
        public static double Sample(System.Random rnd, double rate)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(rate))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Exponential distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="rate">The rate (λ) parameter of the Exponential distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double rate)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(rate))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, rate);
            }
        }
    }
}
