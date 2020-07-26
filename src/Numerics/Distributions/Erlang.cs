// <copyright file="Erlang.cs" company="Math.NET">
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
    /// Continuous Univariate Erlang distribution.
    /// This distribution is a continuous probability distribution with wide applicability primarily due to its
    /// relation to the exponential and Gamma distributions.
    /// <a href="http://en.wikipedia.org/wiki/Erlang_distribution">Wikipedia - Erlang distribution</a>.
    /// </summary>
    public class Erlang : IContinuousDistribution
    {
        System.Random _random;

        readonly int _shape;
        readonly double _rate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Erlang"/> class.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        public Erlang(int shape, double rate)
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
        /// Initializes a new instance of the <see cref="Erlang"/> class.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Erlang(int shape, double rate, System.Random randomSource)
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
        /// Constructs a Erlang distribution from a shape and scale parameter. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="scale">The scale (μ) of the Erlang distribution. Range: μ ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        public static Erlang WithShapeScale(int shape, double scale, System.Random randomSource = null)
        {
            return new Erlang(shape, 1.0/scale, randomSource);
        }

        /// <summary>
        /// Constructs a Erlang distribution from a shape and inverse scale parameter. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        public static Erlang WithShapeRate(int shape, double rate, System.Random randomSource = null)
        {
            return new Erlang(shape, rate, randomSource);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Erlang(k = {_shape}, λ = {_rate})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        public static bool IsValidParameterSet(int shape, double rate)
        {
            return shape >= 0 && rate >= 0.0;
        }

        /// <summary>
        /// Gets the shape (k) of the Erlang distribution. Range: k ≥ 0.
        /// </summary>
        public int Shape => _shape;

        /// <summary>
        /// Gets the rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.
        /// </summary>
        public double Rate => _rate;

        /// <summary>
        /// Gets the scale of the Erlang distribution.
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
        /// Gets the mean of the distribution.
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
        /// Gets the variance of the distribution.
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
        /// Gets the standard deviation of the distribution.
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

                return Math.Sqrt(_shape)/_rate;
            }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
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
        /// Gets the skewness of the distribution.
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
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get
            {
                if (_shape < 1)
                {
                    throw new NotSupportedException();
                }

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
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => throw new NotSupportedException();

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        public double Minimum => 0.0;

        /// <summary>
        /// Gets the Maximum value.
        /// </summary>
        public double Maximum => double.PositiveInfinity;

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF(int, double, double)"/>
        public double Density(double x)
        {
            return PDF(_shape, _rate, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn(int, double, double)"/>
        public double DensityLn(double x)
        {
            return PDFLn(_shape, _rate, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF(int, double, double)"/>
        public double CumulativeDistribution(double x)
        {
            return CDF(_shape, _rate, x);
        }

        /// <summary>
        /// Generates a sample from the Erlang distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return Gamma.SampleUnchecked(_random, _shape, _rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            Gamma.SamplesUnchecked(_random, values, _shape, _rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Erlang distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return Gamma.SampleUnchecked(_random, _shape, _rate);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(int shape, double rate, double x)
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
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(int shape, double rate, double x)
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
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(int shape, double rate, double x)
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
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, int shape, double rate)
        {
            return Gamma.Sample(rnd, shape, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, int shape, double rate)
        {
            return Gamma.Samples(rnd, shape, rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, int shape, double rate)
        {
            Gamma.Samples(rnd, values, shape, rate);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(int shape, double rate)
        {
            return Gamma.Sample(shape, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(int shape, double rate)
        {
            return Gamma.Samples(shape, rate);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, int shape, double rate)
        {
            Gamma.Samples(values, shape, rate);
        }
    }
}
