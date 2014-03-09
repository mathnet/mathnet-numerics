// <copyright file="Erlang.cs" company="Math.NET">
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
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Erlang distribution.
    /// This distribution is is a continuous probability distribution with wide applicability primarily due to its
    /// relation to the exponential and Gamma distributions.
    /// <a href="http://en.wikipedia.org/wiki/Erlang_distribution">Wikipedia - Erlang distribution</a>.
    /// </summary>
    public class Erlang : IContinuousDistribution
    {
        System.Random _random;

        double _shape;
        double _rate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Erlang"/> class.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        public Erlang(int shape, double rate)
        {
            _random = SystemRandomSource.Default;
            SetParameters(shape, rate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Erlang"/> class.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Erlang(int shape, double rate, System.Random randomSource)
        {
            _random = randomSource ?? SystemRandomSource.Default;
            SetParameters(shape, rate);
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
            return "Erlang(k = " + _shape + ", λ = " + _rate + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0 || Double.IsNaN(shape) || Double.IsNaN(rate))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _shape = shape;
            _rate = rate;
        }

        /// <summary>
        /// Gets or sets the shape (k) of the Erlang distribution. Range: k ≥ 0.
        /// </summary>
        public int Shape
        {
            get { return (int)_shape; }
            set { SetParameters(value, _rate); }
        }

        /// <summary>
        /// Gets or sets the rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.
        /// </summary>
        public double Rate
        {
            get { return _rate; }
            set { SetParameters(_shape, value); }
        }

        /// <summary>
        /// Gets or sets the scale of the Erlang distribution.
        /// </summary>
        public double Scale
        {
            get { return 1.0 / _rate; }
            set
            {
                var invScale = 1.0 / value;
                if (Double.IsNegativeInfinity(invScale))
                {
                    invScale = -invScale;
                }
                SetParameters(_shape, invScale);
            }
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
            get
            {
                if (Double.IsPositiveInfinity(_rate))
                {
                    return _shape;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
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
                if (Double.IsPositiveInfinity(_rate))
                {
                    return 0.0;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
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
                if (Double.IsPositiveInfinity(_rate))
                {
                    return 0.0;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
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
                if (Double.IsPositiveInfinity(_rate))
                {
                    return 0.0;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
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
                if (Double.IsPositiveInfinity(_rate))
                {
                    return 0.0;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
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

                if (Double.IsPositiveInfinity(_rate))
                {
                    return _shape;
                }

                if (_rate == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
                }

                return (_shape - 1.0)/_rate;
            }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        public double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the Maximum value.
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
        /// Generates a sample from the Erlang distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _shape, _rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the Erlang distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(_random, _shape, _rate);
            }
        }

        /// <summary>
        /// <para>Sampling implementation based on:
        /// "A Simple Method for Generating Erlang Variables" - Marsaglia &amp; Tsang
        /// ACM Transactions on Mathematical Software, Vol. 26, No. 3, September 2000, Pages 363–372.</para>
        /// <para>This method performs no parameter checks.</para>
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>A sample from a Erlang distributed random variable.</returns>
        static double SampleUnchecked(System.Random rnd, double shape, double rate)
        {
            if (Double.IsPositiveInfinity(rate))
            {
                return shape;
            }

            var a = shape;
            var alphafix = 1.0;

            // Fix when alpha is less than one.
            if (shape < 1.0)
            {
                a = shape + 1.0;
                alphafix = Math.Pow(rnd.NextDouble(), 1.0 / shape);
            }

            var d = a - (1.0 / 3.0);
            var c = 1.0 / Math.Sqrt(9.0 * d);
            while (true)
            {
                var x = Normal.Sample(rnd, 0.0, 1.0);
                var v = 1.0 + (c * x);
                while (v <= 0.0)
                {
                    x = Normal.Sample(rnd, 0.0, 1.0);
                    v = 1.0 + (c * x);
                }

                v = v * v * v;
                var u = rnd.NextDouble();
                x = x * x;
                if (u < 1.0 - (0.0331 * x * x))
                {
                    return alphafix * d * v / rate;
                }

                if (Math.Log(u) < (0.5 * x) + (d * (1.0 - v + Math.Log(v))))
                {
                    return alphafix * d * v / rate;
                }
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
        public static double PDF(double shape, double rate, double x)
        {
            if (shape < 0.0 || rate < 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            if (Double.IsPositiveInfinity(rate)) return x == shape ? Double.PositiveInfinity : 0.0;
            if (shape == 0.0 && rate == 0.0) return 0.0;
            if (shape == 1.0) return rate*Math.Exp(-rate*x);

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
        public static double PDFLn(double shape, double rate, double x)
        {
            if (shape < 0.0 || rate < 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            if (Double.IsPositiveInfinity(rate)) return x == shape ? Double.PositiveInfinity : Double.NegativeInfinity;
            if (shape == 0.0 && rate == 0.0) return Double.NegativeInfinity;
            if (shape == 1.0) return Math.Log(rate) - (rate*x);

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
        public static double CDF(double shape, double rate, double x)
        {
            if (shape < 0.0 || rate < 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            if (Double.IsPositiveInfinity(rate)) return x >= shape ? 1.0 : 0.0;
            if (shape == 0.0 && rate == 0.0) return 0.0;

            return SpecialFunctions.GammaLowerRegularized(shape, x*rate);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return SampleUnchecked(rnd, shape, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double shape, double rate)
        {
            if (shape < 0.0 || rate < 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            while (true)
            {
                yield return SampleUnchecked(rnd, shape, rate);
            }
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double shape, double rate)
        {
            return Sample(SystemRandomSource.Default, shape, rate);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="shape">The shape (k) of the Erlang distribution. Range: k ≥ 0.</param>
        /// <param name="rate">The rate or inverse scale (λ) of the Erlang distribution. Range: λ ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double shape, double rate)
        {
            return Samples(SystemRandomSource.Default, shape, rate);
        }
    }
}
