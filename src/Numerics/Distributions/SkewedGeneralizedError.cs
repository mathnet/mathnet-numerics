// <copyright file="StudentT.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
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
    /// <summary>
    /// Continuous Univariate Skewed Generalized Error Distribution (SGED).
    /// Implements the univariate Skewed Generalized Error Distribution. For details about this
    /// distribution, see
    /// <a href="https://en.wikipedia.org/wiki/Generalized_normal_distribution">
    /// Wikipedia - Generalized Error Distribution</a>.
    /// It includes Laplace, Normal and Student-t distributions.
    /// This is the <see cref="SkewedGeneralizedT"/> distribution with q=Inf.
    /// </summary>
    /// <remarks><para>This implementation is based on the R package dsgt and corresponding viginette, see
    /// <a href="">https://cran.r-project.org/web/packages/sgt/vignettes/sgt.pdf</a>. Compared to that
    /// implementation, the options for mean adjustment and variance adjustment are always true.
    /// The location (μ) is the mean of the distribution.
    /// The scale (σ) squared is the variance of the distribution.
    /// </para>
    /// <para>The distribution will use the <see cref="System.Random"/> by
    /// default.  Users can get/set the random number generator by using the
    /// <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters
    /// whether they are in the allowed range.</para></remarks>
    public class SkewedGeneralizedError : IContinuousDistribution
    {
        System.Random _random;

        readonly double _skewness;

        /// <summary>
        /// Initializes a new instance of the SkewedGeneralizedError class. This is a generalized error distribution
        /// with location=0.0, scale=1.0, skew=0.0 and p=2.0 (a standard normal distribution).
        /// </summary>
        public SkewedGeneralizedError()
        {
            _random = SystemRandomSource.Default;
            Location = 0.0;
            Scale = 1.0;
            Skew = 0.0;
            P = 2.0;
        }

        /// <summary>
        /// Initializes a new instance of the SkewedGeneralizedT class with a particular location, scale, skew
        /// and kurtosis parameters. Different parameterizations result in different distributions.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">Parameter that controls kurtosis. Range: p > 0</param>
        public SkewedGeneralizedError(double location, double scale, double skew, double p)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            Location = location;
            Scale = scale;
            Skew = skew;
            P = p;

            _skewness = CalculateSkewness();
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"SkewedGeneralizedError(μ = {Location}, σ = {Scale}, λ = { Skew }, p = {P}";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">Parameter that controls kurtosis. Range: p > 0</param>
        public static bool IsValidParameterSet(double location, double scale, double skew, double p)
        {
            return scale > 0.0 && skew > -1.0 && skew < 1.0 && p > 0.0 && !double.IsNaN(location);
        }

        /// <summary>
        /// Gets the location (μ) of the Skewed Generalized t-distribution.
        /// </summary>
        public double Location { get; }

        /// <summary>
        /// Gets the scale (σ) of the Skewed Generalized t-distribution. Range: σ > 0.
        /// </summary>
        public double Scale { get; }

        /// <summary>
        /// Gets the skew (λ) of the Skewed Generalized t-distribution. Range: 1 > λ > -1.
        /// </summary>
        public double Skew { get; }

        /// <summary>
        /// Gets the parameter that controls the kurtosis of the distribution. Range: p > 0.
        /// </summary>
        public double P { get; }

        // No skew implies Median=Mode=Mean
        public double Mode =>
            Skew == 0 ? Mean : Mean - AdjustAddend(AdjustScale(Scale, Skew, P), Skew, P);

        public double Minimum => double.NegativeInfinity;

        public double Maximum => double.PositiveInfinity;

        // Mean=Location due to our adjustments made
        public double Mean => Location;

        // Variance=Scale*Scale due to our adjustments made
        public double Variance => Scale * Scale;

        public double StdDev => Scale;

        public double Entropy => throw new NotImplementedException();

        public double Skewness => _skewness;

        // No skew implies Median=Mode=Mean
        // Else find it via the point where CDF gives 0.5
        public double Median =>
            Skew == 0 ? Mean : InverseCumulativeDistribution(0.5);

        double CalculateSkewness()
        {
            if (Skew == 0)
            {
                return 0.0;
            }

            var piPow = Math.Pow(Constants.Pi, 3.0 / 2.0);
            var g1 = SpecialFunctions.Gamma(1.0 / P);
            var g2 = SpecialFunctions.Gamma(0.5 + 1.0 / P);
            var g3 = SpecialFunctions.Gamma(3.0 / P);
            var g4 = SpecialFunctions.Gamma(4.0 / P);

            var t1 = Skew * Scale * Scale * Scale / (piPow * g1);
            var t2 = Math.Pow(2.0, (6.0 + P) / P) * Skew * Skew * Math.Pow(g2, 3.0) * g1;
            var t3 = 3.0 * Math.Pow(4.0, 1.0 / P) * Constants.Pi * (1.0 + 3.0 * Skew * Skew) * g2 * g3;
            var t4 = 4.0 * piPow * (1.0 + Skew * Skew) * g4;

            return t1 * (t2 - t3 + t4);
        }

        static double AdjustScale(double scale, double skew, double p)
        {
            var g1 = SpecialFunctions.Gamma(3.0 / p);
            var g2 = SpecialFunctions.Gamma(0.5 + 1.0 / p);
            var g3 = SpecialFunctions.Gamma(1.0 / p);
            var g4 = SpecialFunctions.Gamma(1.0 / p);
            var n1 = Constants.Pi * (1.0 + 3.0 * skew * skew) * g1;
            var n2 = Math.Pow(16.0, 1.0 / p) * skew * skew * Math.Pow(g2, 2) * g3;
            var d = Constants.Pi * g4;
            return scale / Math.Sqrt((n1 - n2) / d);
        }

        static double AdjustX(double x, double scale, double skew, double p)
        {
            return x + AdjustAddend(scale, skew, p);
        }

        static double AdjustAddend(double scale, double skew, double p)
        {
            return (Math.Pow(2.0, 2.0 / p) * scale * skew * SpecialFunctions.Gamma(1.0 / 2.0 + 1.0 / p)) /
                Constants.SqrtPi;
        }

        public static double PDF(double location, double scale, double skew, double p, double x)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            scale = AdjustScale(scale, skew, p);
            x = AdjustX(x, scale, skew, p);

           // p / (2 * sigma * gamma(1 / p) * exp((abs(x - mu) / (sigma * (1 + lambda * sgn(x - mu)))) ^ p))
            var d1 = Math.Abs(x - location);
            var d2 = scale * (1.0 + skew * Math.Sign(x - location));
            var d3 = 2.0 * scale * SpecialFunctions.Gamma(1.0 / p);
            return p / (Math.Exp(Math.Pow(d1 / d2, p)) * d3);
        }

        public static double PDFLn(double location, double scale, double skew, double p, double x)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            scale = AdjustScale(scale, skew, p);
            x = AdjustX(x, scale, skew, p);

            return Math.Log(p) - Math.Log(2.0) - Math.Log(scale) - SpecialFunctions.GammaLn(1.0 / p) -
                Math.Pow(Math.Abs(x - location) / (scale * (1.0 + skew * Math.Sign(x - location))), p);
        }

        public static double CDF(double location, double scale, double skew, double p, double x)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            scale = AdjustScale(scale, skew, p);
            x = AdjustX(x, scale, skew, p) - location;

            var flip = x < 0;
            if (flip)
            {
                skew = -skew;
                x = -x;
            }

            var res = (1.0 - skew) / 2.0 + (1.0 + skew) / 2.0 * Gamma.CDF(1.0 / p, 1.0, Math.Pow(x / (scale * (1.0 + skew)), p));
            return flip ? 1.0 - res : res;
        }

        public static double InvCDF(double location, double scale, double skew, double p, double pr)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            scale = AdjustScale(scale, skew, p);

            var flip = pr < (1.0 - skew) / 2.0;
            var lambda = skew;
            if (flip)
            {
                pr = 1.0 - pr;
                lambda = -lambda;
            }

            var res = scale * (1.0 + lambda) * Math.Pow(Gamma.InvCDF(1.0 / p, 1.0, 2 * pr / (1.0 + lambda) + (lambda - 1.0) / (lambda + 1.0)), 1.0 / p);

            if (flip)
                res = -res;
            res += location;
            return res - AdjustAddend(scale, skew, p);
        }

        public double InverseCumulativeDistribution(double p)
        {
            return InvCDF(Location, Scale, Skew, P, p);
        }

        public double CumulativeDistribution(double x)
        {
            return CDF(Location, Scale, Skew, P, x);
        }

        public double Density(double x)
        {
            return PDF(Location, Scale, Skew, P, x);
        }

        public double DensityLn(double x)
        {
            return PDFLn(Location, Scale, Skew, P, x);
        }

        public double Sample()
        {
            return SampleUnchecked(_random, Location, Scale, Skew, P);
        }

        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, Location, Scale, Skew, P);
        }

        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, Location, Scale, Skew, P);
        }

        static double SampleUnchecked(System.Random rnd, double location, double scale, double skew, double p)
        {
            var u = ContinuousUniform.Sample(rnd, 0, 1);
            return InvCDF(location, scale, skew, p, u);
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double location, double scale, double skew, double p)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = SampleUnchecked(rnd, location, scale, skew, p);
            }
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double location, double scale, double skew, double p)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, location, scale, skew, p);
            }
        }

        /// <summary>
        /// Generates a sample from the Skew Generalized Error distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">Parameter that controls kurtosis. Range: p > 0</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double location, double scale, double skew, double p)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, location, scale, skew, p);
        }

        /// <summary>
        /// Generates a sequence of samples from the Skew Generalized Error distribution using inverse transform.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">Parameter that controls kurtosis. Range: p > 0</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double location, double scale, double skew, double p)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, location, scale, skew, p);
        }

        /// <summary>
        /// Fills an array with samples from the Skew Generalized Error distribution using inverse transform.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">Parameter that controls kurtosis. Range: p > 0</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double location, double scale, double skew, double p)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, location, scale, skew, p);
        }

        /// <summary>
        /// Generates a sample from the Skew Generalized Error distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">Parameter that controls kurtosis. Range: p > 0</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double location, double scale, double skew, double p)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, location, scale, skew, p);
        }

        /// <summary>
        /// Generates a sequence of samples from the Skew Generalized Error distribution using inverse transform.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">Parameter that controls kurtosis. Range: p > 0</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double location, double scale, double skew, double p)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, location, scale, skew, p);
        }

        /// <summary>
        /// Fills an array with samples from the Skew Generalized Error distribution using inverse transform.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">Parameter that controls kurtosis. Range: p > 0</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double location, double scale, double skew, double p)
        {
            if (!IsValidParameterSet(location, scale, skew, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, location, scale, skew, p);
        }
    }
}
