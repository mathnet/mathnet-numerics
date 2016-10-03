﻿// <copyright file="ChiSquare.cs" company="Math.NET">
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
using MathNet.Numerics.Properties;
using MathNet.Numerics.Random;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Chi-Squared distribution.
    /// This distribution is a sum of the squares of k independent standard normal random variables.
    /// <a href="http://en.wikipedia.org/wiki/Chi-square_distribution">Wikipedia - ChiSquare distribution</a>.
    /// </summary>
    public class ChiSquared : IContinuousDistribution
    {
        System.Random _random;

        readonly double _freedom;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChiSquared"/> class.
        /// </summary>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        public ChiSquared(double freedom)
        {
            if (!IsValidParameterSet(freedom))
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            _random = SystemRandomSource.Default;
            _freedom = freedom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChiSquared"/> class.
        /// </summary>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public ChiSquared(double freedom, System.Random randomSource)
        {
            if (!IsValidParameterSet(freedom))
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _freedom = freedom;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "ChiSquared(k = " + _freedom + ")";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        public static bool IsValidParameterSet(double freedom)
        {
            return freedom > 0.0;
        }

        /// <summary>
        /// Gets the degrees of freedom (k) of the Chi-Squared distribution. Range: k > 0.
        /// </summary>
        public double DegreesOfFreedom
        {
            get { return _freedom; }
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
            get { return _freedom; }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return 2.0*_freedom; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(2.0*_freedom); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return (_freedom/2.0) + Math.Log(2.0*SpecialFunctions.Gamma(_freedom/2.0)) + ((1.0 - (_freedom/2.0))*SpecialFunctions.DiGamma(_freedom/2.0)); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return Math.Sqrt(8.0/_freedom); }
        }

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get { return _freedom - 2.0; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median
        {
            get { return _freedom - (2.0/3.0); }
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
            get { return double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return PDF(_freedom, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return PDFLn(_freedom, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CDF(_freedom, x);
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
            return InvCDF(_freedom, p);
        }

        /// <summary>
        /// Generates a sample from the <c>ChiSquare</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _freedom);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _freedom);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>ChiSquare</c> distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _freedom);
        }

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <returns>a random number from the distribution.</returns>
        static double SampleUnchecked(System.Random rnd, double freedom)
        {
            // Use the simple method if the degrees of freedom is an integer anyway
            if (Math.Floor(freedom) == freedom && freedom < int.MaxValue)
            {
                double sum = 0;
                var n = (int)freedom;
                for (var i = 0; i < n; i++)
                {
                    sum += Math.Pow(Normal.Sample(rnd, 0.0, 1.0), 2);
                }

                return sum;
            }

            // Call the gamma function (see http://en.wikipedia.org/wiki/Gamma_distribution#Specializations
            // for a justification)
            return Gamma.SampleUnchecked(rnd, freedom/2.0, .5);
        }

        internal static void SamplesUnchecked(System.Random rnd, double[] values, double freedom)
        {
            // Use the simple method if the degrees of freedom is an integer anyway
            if (Math.Floor(freedom) == freedom && freedom < int.MaxValue)
            {
                var n = (int)freedom;
                var standard = new double[values.Length*n];
                Normal.SamplesUnchecked(rnd, standard, 0.0, 1.0);
                CommonParallel.For(0, values.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        int k = i*n;
                        double sum = 0;
                        for (int j = 0; j < n; j++)
                        {
                            sum += standard[k + j]*standard[k + j];
                        }

                        values[i] = sum;
                    }
                });
                return;
            }

            // Call the gamma function (see http://en.wikipedia.org/wiki/Gamma_distribution#Specializations
            // for a justification)
            Gamma.SamplesUnchecked(rnd, values, freedom/2.0, .5);
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double freedom)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, freedom);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double freedom, double x)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            if (double.IsPositiveInfinity(freedom) || double.IsPositiveInfinity(x) || x == 0.0)
            {
                return 0.0;
            }

            if (freedom > 160.0)
            {
                return Math.Exp(PDFLn(freedom, x));
            }

            return (Math.Pow(x, (freedom/2.0) - 1.0)*Math.Exp(-x/2.0))/(Math.Pow(2.0, freedom/2.0)*SpecialFunctions.Gamma(freedom/2.0));
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double freedom, double x)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            if (double.IsPositiveInfinity(freedom) || double.IsPositiveInfinity(x) || x == 0.0)
            {
                return double.NegativeInfinity;
            }

            return (-x/2.0) + (((freedom/2.0) - 1.0)*Math.Log(x)) - ((freedom/2.0)*Math.Log(2)) - SpecialFunctions.GammaLn(freedom/2.0);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double freedom, double x)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            if (double.IsPositiveInfinity(x))
            {
                return 1.0;
            }

            if (double.IsPositiveInfinity(freedom))
            {
                return 1.0;
            }

            return SpecialFunctions.GammaLowerRegularized(freedom/2.0, x/2.0);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        public static double InvCDF(double freedom, double p)
        {
            if(!IsValidParameterSet(freedom))
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SpecialFunctions.GammaLowerRegularizedInv(freedom / 2.0, p) / 0.5;
        }

        /// <summary>
        /// Generates a sample from the <c>ChiSquare</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <returns>a sample from the distribution. </returns>
        public static double Sample(System.Random rnd, double freedom)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, freedom);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <returns>a sample from the distribution. </returns>
        public static IEnumerable<double> Samples(System.Random rnd, double freedom)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SamplesUnchecked(rnd, freedom);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <returns>a sample from the distribution. </returns>
        public static void Samples(System.Random rnd, double[] values, double freedom)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            SamplesUnchecked(rnd, values, freedom);
        }

        /// <summary>
        /// Generates a sample from the <c>ChiSquare</c> distribution.
        /// </summary>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <returns>a sample from the distribution. </returns>
        public static double Sample(double freedom)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(SystemRandomSource.Default, freedom);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <returns>a sample from the distribution. </returns>
        public static IEnumerable<double> Samples(double freedom)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            return SamplesUnchecked(SystemRandomSource.Default, freedom);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="freedom">The degrees of freedom (k) of the distribution. Range: k > 0.</param>
        /// <returns>a sample from the distribution. </returns>
        public static void Samples(double[] values, double freedom)
        {
            if (freedom <= 0.0)
            {
                throw new ArgumentException(Resources.InvalidDistributionParameters);
            }

            SamplesUnchecked(SystemRandomSource.Default, values, freedom);
        }
    }
}
