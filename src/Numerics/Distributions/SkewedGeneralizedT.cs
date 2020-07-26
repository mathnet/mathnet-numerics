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

using System;
using System.Collections.Generic;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{

    /// <summary>
    /// Continuous Univariate Skewed Generalized T-distribution.
    /// Implements the univariate Skewed Generalized t-distribution. For details about this
    /// distribution, see
    /// <a href="https://en.wikipedia.org/wiki/Skewed_generalized_t_distribution">
    /// Wikipedia - Skewed generalized t-distribution</a>.
    /// The skewed generalized t-distribution contains many different distributions within it
    /// as special cases based on the parameterization chosen.
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
    public class SkewedGeneralizedT : IContinuousDistribution
    {
        System.Random _random;

        // If the given parameterization is one of the recognized special cases, then
        // this variable is non-null and the special case is used for all functions.
        // Else this value is null and the full formulation of the generalized distribution is used.
        IContinuousDistribution _d;

        readonly double _skewness;

        /// <summary>
        /// Initializes a new instance of the SkewedGeneralizedT class. This is a skewed generalized t-distribution
        /// with location=0.0, scale=1.0, skew=0.0, p=2.0 and q=Inf (a standard normal distribution).
        /// </summary>
        public SkewedGeneralizedT()
        {
            _random = SystemRandomSource.Default;
            Location = 0.0;
            Scale = 1.0;
            Skew = 0.0;
            P = 2.0;
            Q = double.PositiveInfinity;

            _d = new Normal(Location, Scale, _random);
        }

        /// <summary>
        /// Initializes a new instance of the SkewedGeneralizedT class with a particular location, scale, skew
        /// and kurtosis parameters. Different parameterizations result in different distributions.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        public SkewedGeneralizedT(double location, double scale, double skew, double p, double q)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            Location = location;
            Scale = scale;
            Skew = skew;
            P = p;
            Q = q;

            _d = FindSpecializedDistribution(location, scale, skew, p, q);

            if (_d == null)
            {
                _skewness = CalculateSkewness();
            }
        }

        /// <summary>
        /// Given a parameter set, returns the distribution that matches this parameterization.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>Null if no known distribution matches the parameterization, else the distribution.</returns>
        public static IContinuousDistribution FindSpecializedDistribution(double location, double scale, double skew, double p, double q)
        {
            if (p == double.PositiveInfinity)
            {
                scale *= Math.Sqrt(3.0);
                return new ContinuousUniform(location - scale, location + scale);
            }

            if (q == double.PositiveInfinity)
                return new SkewedGeneralizedError(location, scale, skew, p);

            return null;
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
            return $"SkewedGeneralizedT(μ = {Location}, σ = {Scale}, λ = {Skew}, p = {P}, q = {Q})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        public static bool IsValidParameterSet(double location, double scale, double skew, double p, double q)
        {
            return scale > 0.0 && skew > -1.0 && skew < 1.0 && p > 0.0 && q > 0.0 && p*q> 2.0 && !double.IsNaN(location);
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
        /// Gets the first parameter that controls the kurtosis of the distribution. Range: p > 0.
        /// </summary>
        public double P { get; }

        /// <summary>
        /// Gets the second parameter that controls the kurtosis of the distribution. Range: q > 0.
        /// </summary>
        public double Q { get; }

        // No skew implies Median=Mode=Mean
        public double Mode => _d?.Mode ?? (Skew == 0 ? Mean : Mean - AdjustAddend(AdjustScale(Scale, Skew, P, Q), Skew, P, Q));

        public double Minimum => _d?.Minimum ?? double.NegativeInfinity;

        public double Maximum => _d?.Maximum ?? double.PositiveInfinity;

        // Mean=Location due to our adjustments made
        public double Mean => _d?.Mean ?? Location;

        // Variance=Scale*Scale due to our adjustments made
        public double Variance => _d?.Variance ?? Scale * Scale;

        public double StdDev => _d?.StdDev ?? Scale;

        public double Entropy => _d?.Entropy ?? throw new NotImplementedException();

        public double Skewness => _d?.Skewness ?? _skewness;

        // No skew implies Median=Mode=Mean
        // Else find it via the point where CDF gives 0.5
        public double Median => _d?.Median ?? (Skew == 0 ? Mean : InverseCumulativeDistribution(0.5));

        double CalculateSkewness()
        {
            if (P * Q <= 3 || Skew == 0)
            {
                return 0.0;
            }

            var scale = AdjustScale(Scale, Skew, P, Q);
            var b1 = SpecialFunctions.Beta(1.0 / P, Q);
            var b2 = SpecialFunctions.Beta(2.0 / P, Q - 1.0 / P);
            var b3 = SpecialFunctions.Beta(3.0 / P, Q - 2.0 / P);
            var b4 = SpecialFunctions.Beta(4.0 / P, Q - 3.0 / P);

            var t1 = (2.0 * Math.Pow(Q, 3.0 / P) * Skew * Math.Pow(scale, 3.0)) / Math.Pow(b1, 3.0);
            var t2 = 8.0 * Skew * Skew * Math.Pow(b2, 3.0);
            var t3 = 3.0 * (1.0 + 3.0 * Skew * Skew) * b1;
            var t4 = b2 * b3;
            var t5 = 2.0 * (1.0 + Skew * Skew) * Math.Pow(b1, 2.0) * b4;

            return t1 * (t2 - t3 * t4 + t5);
        }

        static double AdjustScale(double scale, double skew, double p, double q)
        {
            var b1 = SpecialFunctions.Beta(3.0 / p, q - 2.0 / p);
            var b2 = SpecialFunctions.Beta(1.0 / p, q);
            var b3 = SpecialFunctions.Beta(2.0 / p, q - 1.0 / p);
            var b4 = SpecialFunctions.Beta(1.0 / p, q);

            return scale / (Math.Pow(q, 1.0 / p) * Math.Sqrt((3.0 * skew * skew + 1.0) * b1 / b2 - 4.0 * skew * skew * ((b3 / b4) * (b3 / b4))));
        }

        // Note: Scale is assumed to be adjusted already when calling this function.
        static double AdjustX(double x, double scale, double skew, double p, double q)
        {
            return x + AdjustAddend(scale, skew, p, q);
        }

        // Note: Scale is assumed to be adjusted already when calling this function.
        static double AdjustAddend(double scale, double skew, double p, double q)
        {
            var b1 = SpecialFunctions.Beta(2.0 / p, q - 1.0 / p);
            var b2 = SpecialFunctions.Beta(1.0 / p, q);

            return (2.0 * scale * skew * Math.Pow(q, 1.0 / p) * b1) / b2;
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double location, double scale, double skew, double p, double q, double x)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var fn = PDFunc(location, scale, skew, p, q, false);
            return fn(x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDFLn(double location, double scale, double skew, double p, double q, double x)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var fn = PDFunc(location, scale, skew, p, q, true);
            return fn(x);
        }

        static double PDFull(double location, double scale, double skew, double p, double q, double x)
        {
            scale = AdjustScale(scale, skew, p, q);
            x = AdjustX(x, scale, skew, p, q);

            var b = SpecialFunctions.Beta(1.0 / p, q);
            var skewSign = Math.Sign(x - location);
            var d1 = Math.Pow(Math.Abs(x - location), p);
            var d2 = q * Math.Pow(scale, p) * Math.Pow(skew * skewSign + 1.0, p);

            var denominator = 2.0 * scale * Math.Pow(q, 1.0 / p) * b * Math.Pow(d1 / d2 + 1.0, 1.0 / p + q);
            return p / denominator;
        }

        static double PDFullLn(double location, double scale, double skew, double p, double q, double x)
        {
            scale = AdjustScale(scale, skew, p, q);
            x = AdjustX(x, scale, skew, p, q);

            var bLn = SpecialFunctions.BetaLn(1.0 / p, q);
            return Math.Log(p) - Math.Log(2.0) - Math.Log(scale) - Math.Log(q) / p - bLn - (1.0 / p + q) *
                Math.Log(1.0 + Math.Pow(Math.Abs(x - location), p) /
                (q * Math.Pow(scale, p) * Math.Pow(1.0 + skew * Math.Sign(x - location), p)));
        }

        // For known parameterizations we just use the existing distributions as visualized
        // by Hansen, McDonald and Newey (2010).
        // Note that, for all cases where skew is required to be 0, if skew is non-zero, this
        // simply gives the corresponding skewed version of the distribution.
        static Func<double, double> PDFunc(double location, double scale, double skew, double p, double q, bool ln)
        {
            if (p == double.PositiveInfinity)
            {
                scale *= Math.Sqrt(3.0);
                return x => ln ? ContinuousUniform.PDFLn(location - scale, location + scale, x) :
                    ContinuousUniform.PDF(-1.0 * (Math.Sqrt(3.0) * scale + location), Math.Sqrt(3.0) * scale + location, x);
            }
            if (q == double.PositiveInfinity)
                return x => ln ? SkewedGeneralizedError.PDFLn(location, scale, skew, p, x) :
                    SkewedGeneralizedError.PDF(location, scale, skew, p, x);

            return x => ln ? PDFullLn(location, scale, skew, p, q, x) :
                PDFull(location, scale, skew, p, q, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double location, double scale, double skew, double p, double q, double x)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            // Note: Adapted from the R package,
            // based on a transformation of the cumulative probability density function that uses the
            // incomplete beta function or incomplete gamma function.

            scale = AdjustScale(scale, skew, p, q);
            x = AdjustX(x, scale, skew, p, q) - location;

            var flip = x > 0;
            if (flip)
            {
                skew = -skew;
                x = -x;
            }

            var res = (1.0 - skew) / 2.0 + (skew - 1.0) / 2.0 * Beta.CDF(1.0 / p, q, 1.0 / (1.0 + q * Math.Pow(scale * (1.0 - skew) / -x, p)));
            return flip ? 1.0 - res : res;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="pr">The location at which to compute the inverse cumulative density.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public static double InvCDF(double location, double scale, double skew, double p, double q, double pr)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            // If parameters represent a specialized distribution, then we use that distribution to avoid
            // problems with infinite p or q parameters.
            var d = FindSpecializedDistribution(location, scale, skew, p, q);
            // InverseCumulativeDistribution is not a part of the interface, so resort to type-checking.
            if (d != null)
            {
                switch (d)
                {
                    case SkewedGeneralizedError sge:
                        return sge.InverseCumulativeDistribution(pr);
                    case ContinuousUniform u:
                        return u.InverseCumulativeDistribution(pr);
                }
            }

            // Note: Adapted from the R package,
            // solving for the inverse of the CDF that uses the inverse of the incomplete beta function or
            // incomplete gamma function

            scale = AdjustScale(scale, skew, p, q);

            var flip = pr > (1.0 - skew) / 2.0;
            var lambda = skew;
            if (flip)
            {
                pr = 1.0 - pr;
                lambda = -lambda;
            }

            var res = scale * (lambda - 1.0) * Math.Pow(1.0 / (q * Beta.InvCDF(1.0 / p, q, 1.0 - 2.0 * pr / (1.0 - lambda))) - 1.0 / q, -1.0 / p);

            if (flip)
                res = -res;
            res += location;
            return res - AdjustAddend(scale, skew, p, q);
        }

        public double CumulativeDistribution(double x)
        {
            return _d?.CumulativeDistribution(x) ?? CDF(Location, Scale, Skew, P, Q, x);
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
            // InverseCumulativeDistribution is not a part of the interface, so resort to type-checking.
            if (_d != null)
            {
                switch (_d)
                {
                    case SkewedGeneralizedError sge:
                        return sge.InverseCumulativeDistribution(p);
                    case ContinuousUniform u:
                        return u.InverseCumulativeDistribution(p);
                }
            }

            return InvCDF(Location, Scale, Skew, P, Q, p);
        }

        public double Density(double x)
        {
            return _d?.Density(x) ?? PDF(Location, Scale, Skew, P, Q, x);
        }

        public double DensityLn(double x)
        {
            return _d?.DensityLn(x) ?? PDFLn(Location, Scale, Skew, P, Q, x);
        }

        public double Sample()
        {
            return SampleUnchecked(_random, Location, Scale, Skew, P, Q);
        }

        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, Location, Scale, Skew, P, Q);
        }

        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, Location, Scale, Skew, P, Q);
        }

        static double SampleUnchecked(System.Random rnd, double location, double scale, double skew, double p, double q)
        {
            var u = ContinuousUniform.Sample(rnd, 0, 1);
            return InvCDF(location, scale, skew, p, q, u);
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double location, double scale, double skew, double p, double q)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = SampleUnchecked(rnd, location, scale, skew, p, q);
            }
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double location, double scale, double skew, double p, double q)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, location, scale, skew, p, q);
            }
        }

        /// <summary>
        /// Generates a sample from the Skew Generalized t-distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double location, double scale, double skew, double p, double q)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, location, scale, skew, p, q);
        }

        /// <summary>
        /// Generates a sequence of samples from the Skew Generalized t-distribution using inverse transform.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double location, double scale, double skew, double p, double q)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, location, scale, skew, p, q);
        }

        /// <summary>
        /// Fills an array with samples from the Skew Generalized t-distribution using inverse transform.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double location, double scale, double skew, double p, double q)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, location, scale, skew, p, q);
        }

        /// <summary>
        /// Generates a sample from the Skew Generalized t-distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double location, double scale, double skew, double p, double q)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, location, scale, skew, p, q);
        }

        /// <summary>
        /// Generates a sequence of samples from the Skew Generalized t-distribution using inverse transform.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double location, double scale, double skew, double p, double q)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, location, scale, skew, p, q);
        }

        /// <summary>
        /// Fills an array with samples from the Skew Generalized t-distribution using inverse transform.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="skew">The skew, 1 > λ > -1</param>
        /// <param name="p">First parameter that controls kurtosis. Range: p > 0</param>
        /// <param name="q">Second parameter that controls kurtosis. Range: q > 0</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double location, double scale, double skew, double p, double q)
        {
            if (!IsValidParameterSet(location, scale, skew, p, q))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, location, scale, skew, p, q);
        }
    }
}
