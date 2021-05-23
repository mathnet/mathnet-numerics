// <copyright file="Burr.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2019 Math.NET
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

using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;

namespace MathNet.Numerics.Distributions
{
    public class Burr : IContinuousDistribution
    {
        System.Random _random;

        /// <summary>
        /// Gets the scale (a) of the distribution. Range: a > 0.
        /// </summary>
        public double A { get; }

        /// <summary>
        /// Gets the first shape parameter (c) of the distribution. Range: c > 0.
        /// </summary>
        public double C { get; }

        /// <summary>
        /// Gets the second shape parameter (k) of the distribution. Range: k > 0.
        /// </summary>
        public double K { get; }

        /// <summary>
        /// Initializes a new instance of the Burr Type XII class.
        /// </summary>
        /// <param name="a">The scale parameter a of the Burr distribution. Range: a > 0.</param>
        /// <param name="c">The first shape parameter c of the Burr distribution. Range: c > 0.</param>
        /// <param name="k">The second shape parameter k of the Burr distribution. Range: k > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        public Burr(double a, double c, double k, System.Random randomSource = null)
        {
            if (!IsValidParameterSet(a, c, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            _random = randomSource ?? SystemRandomSource.Default;
            A = a;
            C = c;
            K = k;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Burr(a = {A}, c = {C}, k = {K})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="a">The scale parameter a of the Burr distribution. Range: a > 0.</param>
        /// <param name="c">The first shape parameter c of the Burr distribution. Range: c > 0.</param>
        /// <param name="k">The second shape parameter k of the Burr distribution. Range: k > 0.</param>
        public static bool IsValidParameterSet(double a, double c, double k)
        {
            var allFinite = a.IsFinite() && c.IsFinite() && k.IsFinite();
            return allFinite && a > 0.0 && c > 0.0 && k > 0.0;
        }

        /// <summary>
        /// Gets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the Burr distribution.
        /// </summary>
        public double Mean => (1 / SpecialFunctions.Gamma(K)) * A * SpecialFunctions.Gamma(1 + 1 / C) * SpecialFunctions.Gamma(K - 1 / C);

        /// <summary>
        /// Gets the variance of the Burr distribution.
        /// </summary>
        public double Variance =>
            (1 / SpecialFunctions.Gamma(K)) * Math.Pow(A, 2) * SpecialFunctions.Gamma(1 + 2 / C) * SpecialFunctions.Gamma(K - 2 / C)
            - Math.Pow((1 / SpecialFunctions.Gamma(K)) * A * SpecialFunctions.Gamma(1 + 1 / C) * SpecialFunctions.Gamma(K - 1 / C), 2);

        /// <summary>
        /// Gets the standard deviation of the Burr distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(Variance);

        /// <summary>
        /// Gets the mode of the Burr distribution.
        /// </summary>
        public double Mode => A * Math.Pow((C - 1) / (C * K + 1), 1 / C);

        /// <summary>
        /// Gets the minimum of the Burr distribution.
        /// </summary>
        public double Minimum => 0.0;

        /// <summary>
        /// Gets the maximum of the Burr distribution.
        /// </summary>
        public double Maximum => double.PositiveInfinity;

        /// <summary>
        /// Gets the entropy of the Burr distribution (currently not supported).
        /// </summary>
        public double Entropy => throw new NotSupportedException();

        /// <summary>
        /// Gets the skewness of the Burr distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                var mean = Mean;
                var variance = Variance;
                var std = StdDev;
                return (GetMoment(3) - 3 * mean * variance - mean * mean * mean) / (std * std * std);
            }
        }

        /// <summary>
        /// Gets the median of the Burr distribution.
        /// </summary>
        public double Median => A * Math.Pow(Math.Pow(2, 1 / K) - 1, 1 / C);

        /// <summary>
        /// Generates a sample from the Burr distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, A, C, K);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, A, C, K);
        }

        /// <summary>
        /// Generates a sequence of samples from the Burr distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, A, C, K);
        }

        /// <summary>
        /// Generates a sample from the Burr distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The scale parameter a of the Burr distribution. Range: a > 0.</param>
        /// <param name="c">The first shape parameter c of the Burr distribution. Range: c > 0.</param>
        /// <param name="k">The second shape parameter k of the Burr distribution. Range: k > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double a, double c, double k)
        {
            if (!IsValidParameterSet(a, c, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return SampleUnchecked(rnd, a, c, k);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="a">The scale parameter a of the Burr distribution. Range: a > 0.</param>
        /// <param name="c">The first shape parameter c of the Burr distribution. Range: c > 0.</param>
        /// <param name="k">The second shape parameter k of the Burr distribution. Range: k > 0.</param>
        public static void Samples(System.Random rnd, double[] values, double a, double c, double k)
        {
            if (!IsValidParameterSet(a, c, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            SamplesUnchecked(rnd, values, a, c, k);
        }

        /// <summary>
        /// Generates a sequence of samples from the Burr distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The scale parameter a of the Burr distribution. Range: a > 0.</param>
        /// <param name="c">The first shape parameter c of the Burr distribution. Range: c > 0.</param>
        /// <param name="k">The second shape parameter k of the Burr distribution. Range: k > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double a, double c, double k)
        {
            if (!IsValidParameterSet(a, c, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return SamplesUnchecked(rnd, a, c, k);
        }

        static double SampleUnchecked(System.Random rnd, double a, double c, double k)
        {
            var kInv = 1 / k;
            var cInv = 1 / c;
            double u = rnd.NextDouble();
            return a * Math.Pow(Math.Pow(1 - u, -kInv) - 1, cInv);
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double a, double c, double k)
        {
            if (values.Length == 0)
            {
                return;
            }
            var kInv = 1 / k;
            var cInv = 1 / c;
            double[] u = rnd.NextDoubles(values.Length);

            for (var j = 0; j < values.Length; ++j)
            {
                values[j] = a * Math.Pow(Math.Pow(1 - u[j], -kInv) - 1, cInv);
            }
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double a, double c, double k)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, a, c, k);
            }
        }

        /// <summary>
        /// Gets the n-th raw moment of the distribution.
        /// </summary>
        /// <param name="n">The order (n) of the moment. Range: n ≥ 1.</param>
        /// <returns>the n-th moment of the distribution.</returns>
        public double GetMoment(double n)
        {
            if (n > K * C)
            {
                throw new ArgumentException("The chosen parameter set is invalid (probably some value is out of range).");
            }
            var lambdaN = (n / C) * SpecialFunctions.Gamma(n / C) * SpecialFunctions.Gamma(K - n / C);
            return Math.Pow(A, n) * lambdaN / SpecialFunctions.Gamma(K);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return DensityImpl(A, C, K, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return DensityLnImpl(A, C, K, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CumulativeDistributionImpl(A, C, K, x);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="a">The scale parameter a of the Burr distribution. Range: a > 0.</param>
        /// <param name="c">The first shape parameter c of the Burr distribution. Range: c > 0.</param>
        /// <param name="k">The second shape parameter k of the Burr distribution. Range: k > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double a, double c, double k, double x)
        {
            if (!IsValidParameterSet(a, c, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return DensityImpl(a, c, k, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="a">The scale parameter a of the Burr distribution. Range: a > 0.</param>
        /// <param name="c">The first shape parameter c of the Burr distribution. Range: c > 0.</param>
        /// <param name="k">The second shape parameter k of the Burr distribution. Range: k > 0.</param>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double a, double c, double k, double x)
        {
            if (!IsValidParameterSet(a, c, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return DensityLnImpl(a, c, k, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="a">The scale parameter a of the Burr distribution. Range: a > 0.</param>
        /// <param name="c">The first shape parameter c of the Burr distribution. Range: c > 0.</param>
        /// <param name="k">The second shape parameter k of the Burr distribution. Range: k > 0.</param>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double a, double c, double k, double x)
        {
            if (!IsValidParameterSet(a, c, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return CumulativeDistributionImpl(a, c, k, x);
        }

        static double DensityImpl(double a, double c, double k, double x)
        {
            var numerator = (k * c / a) * Math.Pow(x / a, c - 1);
            var denominator = Math.Pow(1 + Math.Pow(x / a, c), k + 1);
            return numerator / denominator;
        }

        static double DensityLnImpl(double a, double c, double k, double x)
        {
            return Math.Log(DensityImpl(a, c, k, x));
        }

        static double CumulativeDistributionImpl(double a, double c, double k, double x)
        {
            var denominator = Math.Pow(1 + Math.Pow(x / a, c), k);
            return 1 - 1 / denominator;
        }
    }
}
