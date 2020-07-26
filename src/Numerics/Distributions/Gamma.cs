// <copyright file="Gamma.cs" company="Math.NET">
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
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Gamma distribution.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Gamma_distribution">Wikipedia - Gamma distribution</a>.
    /// </summary>
    /// <remarks>
    /// The Gamma distribution is parametrized by a shape and inverse scale parameter. When we want
    /// to specify a Gamma distribution which is a point distribution we set the shape parameter to be the
    /// location of the point distribution and the inverse scale as positive infinity. The distribution
    /// with shape and inverse scale both zero is undefined.
    ///
    /// Random number generation for the Gamma distribution is based on the algorithm in:
    /// "A Simple Method for Generating Gamma Variables" - Marsaglia &amp; Tsang
    /// ACM Transactions on Mathematical Software, Vol. 26, No. 3, September 2000, Pages 363–372.
    /// </remarks>
    public class Gamma : IContinuousDistribution
    {
        System.Random _random;

        readonly double _shape;
        readonly double _rate;

        /// <summary>
        /// Initializes a new instance of the Gamma class.
        /// </summary>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        public Gamma(double shape, double rate)
        {
            if (!IsValidParameterSet(shape, rate))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _shape = shape;
            _rate = rate;
        }

        /// <summary>
        /// Initializes a new instance of the Gamma class.
        /// </summary>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Gamma(double shape, double rate, System.Random randomSource)
        {
            if (!IsValidParameterSet(shape, rate))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _shape = shape;
            _rate = rate;
        }

        /// <summary>
        /// Constructs a Gamma distribution from a shape and scale parameter. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="shape">The shape (k) of the Gamma distribution. Range: k ≥ 0.</param>
        /// <param name="scale">The scale (θ) of the Gamma distribution. Range: θ ≥ 0</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        public static Gamma WithShapeScale(double shape, double scale, System.Random randomSource = null)
        {
            return new Gamma(shape, 1.0/scale, randomSource);
        }

        /// <summary>
        /// Constructs a Gamma distribution from a shape and inverse scale parameter. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        public static Gamma WithShapeRate(double shape, double rate, System.Random randomSource = null)
        {
            return new Gamma(shape, rate, randomSource);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Gamma(α = {_shape}, β = {_rate})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        public static bool IsValidParameterSet(double shape, double rate)
        {
            return shape >= 0.0 && rate >= 0.0;
        }

        /// <summary>
        /// Gets or sets the shape (k, α) of the Gamma distribution. Range: α ≥ 0.
        /// </summary>
        public double Shape => _shape;

        /// <summary>
        /// Gets or sets the rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.
        /// </summary>
        public double Rate => _rate;

        /// <summary>
        /// Gets or sets the scale (θ) of the Gamma distribution.
        /// </summary>
        public double Scale => 1.0/_rate;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the Gamma distribution.
        /// </summary>
        public double Mean
        {
            get
            {
                if (double.IsPositiveInfinity(_rate))
                {
                    return _shape;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return double.NaN;
                }

                return _shape/_rate;
            }
        }

        /// <summary>
        /// Gets the variance of the Gamma distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (double.IsPositiveInfinity(_rate))
                {
                    return 0.0;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return double.NaN;
                }

                return _shape/(_rate*_rate);
            }
        }

        /// <summary>
        /// Gets the standard deviation of the Gamma distribution.
        /// </summary>
        public double StdDev
        {
            get
            {
                if (double.IsPositiveInfinity(_rate))
                {
                    return 0.0;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return double.NaN;
                }

                return Math.Sqrt(_shape/(_rate*_rate));
            }
        }

        /// <summary>
        /// Gets the entropy of the Gamma distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                if (double.IsPositiveInfinity(_rate))
                {
                    return 0.0;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return double.NaN;
                }

                return _shape - Math.Log(_rate) + SpecialFunctions.GammaLn(_shape) + ((1.0 - _shape)*SpecialFunctions.DiGamma(_shape));
            }
        }

        /// <summary>
        /// Gets the skewness of the Gamma distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (double.IsPositiveInfinity(_rate))
                {
                    return 0.0;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return double.NaN;
                }

                return 2.0/Math.Sqrt(_shape);
            }
        }

        /// <summary>
        /// Gets the mode of the Gamma distribution.
        /// </summary>
        public double Mode
        {
            get
            {
                if (double.IsPositiveInfinity(_rate))
                {
                    return _shape;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return double.NaN;
                }

                return (_shape - 1.0)/_rate;
            }
        }

        /// <summary>
        /// Gets the median of the Gamma distribution.
        /// </summary>
        public double Median => throw new NotSupportedException();

        /// <summary>
        /// Gets the minimum of the Gamma distribution.
        /// </summary>
        public double Minimum => 0.0;

        /// <summary>
        /// Gets the maximum of the Gamma distribution.
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
            return PDF(_shape, _rate, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return PDFLn(_shape, _rate, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CDF(_shape, _rate, x);
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
            return InvCDF(_shape, _rate, p);
        }

        /// <summary>
        /// Generates a sample from the Gamma distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _shape, _rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _shape, _rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Gamma distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _shape, _rate);
        }

        /// <summary>
        /// <para>Sampling implementation based on:
        /// "A Simple Method for Generating Gamma Variables" - Marsaglia &amp; Tsang
        /// ACM Transactions on Mathematical Software, Vol. 26, No. 3, September 2000, Pages 363–372.</para>
        /// <para>This method performs no parameter checks.</para>
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>A sample from a Gamma distributed random variable.</returns>
        internal static double SampleUnchecked(System.Random rnd, double shape, double rate)
        {
            if (double.IsPositiveInfinity(rate))
            {
                return shape;
            }

            var a = shape;
            var alphafix = 1.0;

            // Fix when alpha is less than one.
            if (shape < 1.0)
            {
                a = shape + 1.0;
                alphafix = Math.Pow(rnd.NextDouble(), 1.0/shape);
            }

            var d = a - (1.0/3.0);
            var c = 1.0/Math.Sqrt(9.0*d);
            while (true)
            {
                var x = Normal.Sample(rnd, 0.0, 1.0);
                var v = 1.0 + (c*x);
                while (v <= 0.0)
                {
                    x = Normal.Sample(rnd, 0.0, 1.0);
                    v = 1.0 + (c*x);
                }

                v = v*v*v;
                var u = rnd.NextDouble();
                x = x*x;
                if (u < 1.0 - (0.0331*x*x))
                {
                    return alphafix*d*v/rate;
                }

                if (Math.Log(u) < (0.5*x) + (d*(1.0 - v + Math.Log(v))))
                {
                    return alphafix*d*v/rate;
                }
            }
        }

        internal static void SamplesUnchecked(System.Random rnd, double[] values, double shape, double rate)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = SampleUnchecked(rnd, shape, rate);
            }
        }

        internal static IEnumerable<double> SamplesUnchecked(System.Random rnd, double location, double scale)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, location, scale);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double shape, double rate, double x)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (double.IsPositiveInfinity(rate))
            {
                return x == shape ? double.PositiveInfinity : 0.0;
            }

            if (shape == 0.0 && rate == 0.0)
            {
                return 0.0;
            }

            if (shape == 1.0)
            {
                return rate*Math.Exp(-rate*x);
            }

            if (shape > 160.0)
            {
                return Math.Exp(PDFLn(shape, rate, x));
            }

            return Math.Pow(rate, shape)*Math.Pow(x, shape - 1.0)*Math.Exp(-rate*x)/SpecialFunctions.Gamma(shape);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double shape, double rate, double x)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (double.IsPositiveInfinity(rate))
            {
                return x == shape ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (shape == 0.0 && rate == 0.0)
            {
                return double.NegativeInfinity;
            }

            if (shape == 1.0)
            {
                return Math.Log(rate) - (rate*x);
            }

            return (shape*Math.Log(rate)) + ((shape - 1.0)*Math.Log(x)) - (rate*x) - SpecialFunctions.GammaLn(shape);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double shape, double rate, double x)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (double.IsPositiveInfinity(rate))
            {
                return x >= shape ? 1.0 : 0.0;
            }

            if (shape == 0.0 && rate == 0.0)
            {
                return 0.0;
            }

            return SpecialFunctions.GammaLowerRegularized(shape, x*rate);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public static double InvCDF(double shape, double rate, double p)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SpecialFunctions.GammaLowerRegularizedInv(shape, p)/rate;
        }

        /// <summary>
        /// Generates a sample from the Gamma distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, shape, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Gamma distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, shape, rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, shape, rate);
        }

        /// <summary>
        /// Generates a sample from the Gamma distribution.
        /// </summary>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, shape, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Gamma distribution.
        /// </summary>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, shape, rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="shape">The shape (k, α) of the Gamma distribution. Range: α ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (β) of the Gamma distribution. Range: β ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, shape, rate);
        }
    }
}
