// <copyright file="TruncatedPareto.cs" company="Math.NET">
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
    public class TruncatedPareto : IContinuousDistribution
    {
        System.Random _random;

        /// <summary>
        /// Initializes a new instance of the TruncatedPareto class.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentException">If <paramref name="scale"/> or <paramref name="shape"/> are non-positive or if T ≤ xm.</exception>
        public TruncatedPareto(double scale, double shape, double truncation, System.Random randomSource = null)
        {
            if (!IsValidParameterSet(scale, shape, truncation))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            _random = randomSource ?? SystemRandomSource.Default;
            Scale = scale;
            Shape = shape;
            Truncation = truncation;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Truncated Pareto(Scale = {Scale}, Shape = {Shape}, Truncation = {Truncation})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        public static bool IsValidParameterSet(double scale, double shape, double truncation)
        {
            var allFinite = scale.IsFinite() && shape.IsFinite() && truncation.IsFinite();
            return allFinite && scale > 0.0 && shape > 0.0 && truncation > scale;
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
        /// Gets the scale (xm) of the distribution. Range: xm > 0.
        /// </summary>
        public double Scale { get; }

        /// <summary>
        /// Gets the shape (α) of the distribution. Range: α > 0.
        /// </summary>
        public double Shape { get; }

        /// <summary>
        /// Gets the truncation (T) of the distribution. Range: T > 0.
        /// </summary>
        public double Truncation { get; }

        /// <summary>
        /// Gets the n-th raw moment of the distribution.
        /// </summary>
        /// <param name="n">The order (n) of the moment. Range: n ≥ 1.</param>
        /// <returns>the n-th moment of the distribution.</returns>
        public double GetMoment(int n)
        {
            double moment;
            if (Shape.AlmostEqual(n))
            {
                moment = ((Shape * Math.Pow(Scale, n)) / (1 - Math.Pow(Scale / Truncation, Shape))) * Math.Log(Truncation / Scale);
            }
            else
            {
                moment = ((Shape * Math.Pow(Scale, n)) / (Shape - n)) * ((1 - Math.Pow((Scale / Truncation), (Shape - n))) / (1 - Math.Pow(Scale / Truncation, Shape)));
            }

            return moment;
        }

        /// <summary>
        /// Gets the mean of the truncated Pareto distribution.
        /// </summary>
        public double Mean => GetMoment(1);

        /// <summary>
        /// Gets the variance of the truncated Pareto distribution.
        /// </summary>
        public double Variance => GetMoment(2) - Math.Pow(GetMoment(1), 2);

        /// <summary>
        /// Gets the standard deviation of the truncated Pareto distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(Variance);

        /// <summary>
        /// Gets the mode of the truncated Pareto distribution (not supported).
        /// </summary>
        public double Mode => throw new NotSupportedException();

        /// <summary>
        /// Gets the minimum of the truncated Pareto distribution.
        /// </summary>
        public double Minimum => Scale;

        /// <summary>
        /// Gets the maximum of the truncated Pareto distribution.
        /// </summary>
        public double Maximum => Truncation;

        /// <summary>
        /// Gets the entropy of the truncated Pareto distribution (not supported).
        /// </summary>
        public double Entropy => throw new NotSupportedException();

        /// <summary>
        /// Gets the skewness of the truncated Pareto distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                var mean = Mean;
                var variance = Variance;
                var std = StdDev;
                return (GetMoment(3) - 3.0 * mean * variance - mean * mean * mean) / (std * std * std);
            }
        }

        /// <summary>
        /// Gets the median of the truncated Pareto distribution.
        /// </summary>
        public double Median => Scale * Math.Pow(1.0 - (1.0 / 2.0) * (1.0 - Math.Pow(Scale / Truncation, Shape)), -(1.0 / Shape));

        /// <summary>
        /// Generates a sample from the truncated Pareto distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, Scale, Shape, Truncation);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, Scale, Shape, Truncation);
        }

        /// <summary>
        /// Generates a sequence of samples from the truncated Pareto distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, Scale, Shape, Truncation);
        }

        /// <summary>
        /// Generates a sample from the truncated Pareto distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double scale, double shape, double truncation)
        {
            if (!IsValidParameterSet(scale, shape, truncation))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return SampleUnchecked(rnd, scale, shape, truncation);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        public static void Samples(System.Random rnd, double[] values, double scale, double shape, double truncation)
        {
            if (!IsValidParameterSet(scale, shape, truncation))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            SamplesUnchecked(rnd, values, scale, shape, truncation);
        }

        /// <summary>
        /// Generates a sequence of samples from the truncated Pareto distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double scale, double shape, double truncation)
        {
            if (!IsValidParameterSet(scale, shape, truncation))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return SamplesUnchecked(rnd, scale, shape, truncation);
        }

        internal static double SampleUnchecked(System.Random rnd, double scale, double shape, double truncation)
        {
            double uniform = rnd.NextDouble();
            return InvCDFUncheckedImpl(scale, shape, truncation, uniform);
        }

        internal static void SamplesUnchecked(System.Random rnd, double[] values, double scale, double shape, double truncation)
        {
            if (values.Length == 0)
            {
                return;
            }
            double[] uniforms = rnd.NextDoubles(values.Length);
            for (var j = 0; j < values.Length; ++j)
            {
                values[j] = InvCDFUncheckedImpl(scale, shape, truncation, uniforms[j]);
            }
        }

        internal static IEnumerable<double> SamplesUnchecked(System.Random rnd, double scale, double shape, double truncation)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, scale, shape, truncation);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return DensityImpl(Scale, Shape, Truncation, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return DensityLnImpl(Scale, Shape, Truncation, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CumulativeDistributionImpl(Scale, Shape, Truncation, x);
        }

        /// <summary>
        /// Computes the inverse cumulative distribution (CDF) of the distribution at p, i.e. solving for P(X ≤ x) = p.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative distribution function.</param>
        /// <returns>the inverse cumulative distribution at location <paramref name="p"/>.</returns>
        public double InvCDF(double p)
        {
            return InvCDFUncheckedImpl(Scale, Shape, Truncation, p);
        }

        /// <summary>
        /// Computes the inverse cumulative distribution (CDF) of the distribution at p, i.e. solving for P(X ≤ x) = p.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        /// <param name="p">The location at which to compute the inverse cumulative distribution function.</param>
        ///  <returns>the inverse cumulative distribution at location <paramref name="p"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double InvCDF(double scale, double shape, double truncation, double p)
        {
            if (!IsValidParameterSet(scale, shape, truncation))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return InvCDFUncheckedImpl(scale, shape, truncation, p);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double scale, double shape, double truncation, double x)
        {
            if (!IsValidParameterSet(scale, shape, truncation))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return DensityImpl(scale, shape, truncation, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double scale, double shape, double truncation, double x)
        {
            if (!IsValidParameterSet(scale, shape, truncation))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return DensityLnImpl(scale, shape, truncation, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="scale">The scale (xm) of the distribution. Range: xm > 0.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="truncation">The truncation (T) of the distribution. Range: T > xm.</param>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double scale, double shape, double truncation, double x)
        {
            if (!IsValidParameterSet(scale, shape, truncation))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return CumulativeDistributionImpl(scale, shape, truncation, x);
        }

        static double DensityImpl(double scale, double shape, double truncation, double x)
        {
            if (x < scale || x > truncation)
                return 0;
            else
                return (shape * Math.Pow(scale, shape) * Math.Pow(x, -shape - 1)) / (1 - Math.Pow(scale / truncation, shape));
        }

        static double DensityLnImpl(double scale, double shape, double truncation, double x)
        {
            return Math.Log(DensityImpl(scale, shape, truncation, x));
        }

        static double CumulativeDistributionImpl(double scale, double shape, double truncation, double x)
        {
            if (x <= scale)
                return 0;
            else if (x >= truncation)
                return 1;
            else
                return (1 - Math.Pow(scale, shape) * Math.Pow(x, -shape)) / (1 - Math.Pow(scale / truncation, shape));
        }

        static double InvCDFUncheckedImpl(double scale, double shape, double truncation, double p)
        {
            var numerator = p * Math.Pow(truncation, shape) - p * Math.Pow(scale, shape) - Math.Pow(truncation, shape);
            var denominator = Math.Pow(truncation, shape) * Math.Pow(scale, shape);
            return Math.Pow(-numerator / denominator, -(1 / shape));
        }
    }
}
