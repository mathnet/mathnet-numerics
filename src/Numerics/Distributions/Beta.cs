// <copyright file="Beta.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using MathNet.Numerics.RootFinding;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Beta distribution.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Beta_distribution">Wikipedia - Beta distribution</a>.
    /// </summary>
    /// <remarks>
    /// There are a few special cases for the parameterization of the Beta distribution. When both
    /// shape parameters are positive infinity, the Beta distribution degenerates to a point distribution
    /// at 0.5. When one of the shape parameters is positive infinity, the distribution degenerates to a point
    /// distribution at the positive infinity. When both shape parameters are 0.0, the Beta distribution
    /// degenerates to a Bernoulli distribution with parameter 0.5. When one shape parameter is 0.0, the
    /// distribution degenerates to a point distribution at the non-zero shape parameter.
    /// </remarks>
    public class Beta : IContinuousDistribution
    {
        System.Random _random;

        readonly double _shapeA;
        readonly double _shapeB;

        /// <summary>
        /// Initializes a new instance of the Beta class.
        /// </summary>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        public Beta(double a, double b)
        {
            if (!IsValidParameterSet(a, b))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _shapeA = a;
            _shapeB = b;
        }

        /// <summary>
        /// Initializes a new instance of the Beta class.
        /// </summary>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Beta(double a, double b, System.Random randomSource)
        {
            if (!IsValidParameterSet(a, b))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _shapeA = a;
            _shapeB = b;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>A string representation of the Beta distribution.</returns>
        public override string ToString()
        {
            return $"Beta(α = {_shapeA}, β = {_shapeB})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        public static bool IsValidParameterSet(double a, double b)
        {
            return a >= 0.0 && b >= 0.0;
        }

        /// <summary>
        /// Gets the α shape parameter of the Beta distribution. Range: α ≥ 0.
        /// </summary>
        public double A => _shapeA;

        /// <summary>
        /// Gets the β shape parameter of the Beta distribution. Range: β ≥ 0.
        /// </summary>
        public double B => _shapeB;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the Beta distribution.
        /// </summary>
        public double Mean
        {
            get
            {
                if (_shapeA == 0.0 && _shapeB == 0.0)
                {
                    return 0.5;
                }

                if (_shapeA == 0.0)
                {
                    return 0.0;
                }

                if (_shapeB == 0.0)
                {
                    return 1.0;
                }

                if (double.IsPositiveInfinity(_shapeA) && double.IsPositiveInfinity(_shapeB))
                {
                    return 0.5;
                }

                if (double.IsPositiveInfinity(_shapeA))
                {
                    return 1.0;
                }

                if (double.IsPositiveInfinity(_shapeB))
                {
                    return 0.0;
                }

                return _shapeA/(_shapeA + _shapeB);
            }
        }

        /// <summary>
        /// Gets the variance of the Beta distribution.
        /// </summary>
        public double Variance => (_shapeA*_shapeB)/((_shapeA + _shapeB)*(_shapeA + _shapeB)*(_shapeA + _shapeB + 1.0));

        /// <summary>
        /// Gets the standard deviation of the Beta distribution.
        /// </summary>
        public double StdDev => Math.Sqrt((_shapeA*_shapeB)/((_shapeA + _shapeB)*(_shapeA + _shapeB)*(_shapeA + _shapeB + 1.0)));

        /// <summary>
        /// Gets the entropy of the Beta distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                if (double.IsPositiveInfinity(_shapeA) || double.IsPositiveInfinity(_shapeB))
                {
                    return 0.0;
                }

                if (_shapeA == 0.0 && _shapeB == 0.0)
                {
                    return -Math.Log(0.5);
                }

                if (_shapeA == 0.0 || _shapeB == 0.0)
                {
                    return 0.0;
                }

                return SpecialFunctions.BetaLn(_shapeA, _shapeB)
                       - ((_shapeA - 1.0)*SpecialFunctions.DiGamma(_shapeA))
                       - ((_shapeB - 1.0)*SpecialFunctions.DiGamma(_shapeB))
                       + ((_shapeA + _shapeB - 2.0)*SpecialFunctions.DiGamma(_shapeA + _shapeB));
            }
        }

        /// <summary>
        /// Gets the skewness of the Beta distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (double.IsPositiveInfinity(_shapeA) && double.IsPositiveInfinity(_shapeB))
                {
                    return 0.0;
                }

                if (double.IsPositiveInfinity(_shapeA))
                {
                    return -2.0;
                }

                if (double.IsPositiveInfinity(_shapeB))
                {
                    return 2.0;
                }

                if (_shapeA == 0.0 && _shapeB == 0.0)
                {
                    return 0.0;
                }

                if (_shapeA == 0.0)
                {
                    return 2.0;
                }

                if (_shapeB == 0.0)
                {
                    return -2.0;
                }

                return 2.0*(_shapeB - _shapeA)*Math.Sqrt(_shapeA + _shapeB + 1.0)
                       /((_shapeA + _shapeB + 2.0)*Math.Sqrt(_shapeA*_shapeB));
            }
        }

        /// <summary>
        /// Gets the mode of the Beta distribution; when there are multiple answers, this routine will return 0.5.
        /// </summary>
        public double Mode
        {
            get
            {
                if (_shapeA == 0.0 && _shapeB == 0.0)
                {
                    return 0.5;
                }

                if (_shapeA == 0.0)
                {
                    return 0.0;
                }

                if (_shapeB == 0.0)
                {
                    return 1.0;
                }

                if (double.IsPositiveInfinity(_shapeA) && double.IsPositiveInfinity(_shapeB))
                {
                    return 0.5;
                }

                if (double.IsPositiveInfinity(_shapeA))
                {
                    return 1.0;
                }

                if (double.IsPositiveInfinity(_shapeB))
                {
                    return 0.0;
                }

                if (_shapeA == 1.0 && _shapeB == 1.0)
                {
                    return 0.5;
                }

                return (_shapeA - 1)/(_shapeA + _shapeB - 2);
            }
        }

        /// <summary>
        /// Gets the median of the Beta distribution.
        /// </summary>
        public double Median => throw new NotSupportedException();

        /// <summary>
        /// Gets the minimum of the Beta distribution.
        /// </summary>
        public double Minimum => 0.0;

        /// <summary>
        /// Gets the maximum of the Beta distribution.
        /// </summary>
        public double Maximum => 1.0;

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return PDF(_shapeA, _shapeB, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return PDFLn(_shapeA, _shapeB, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CDF(_shapeA, _shapeB, x);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InvCDF"/>
        /// <remarks>WARNING: currently not an explicit implementation, hence slow and unreliable.</remarks>
        public double InverseCumulativeDistribution(double p)
        {
            return InvCDF(_shapeA, _shapeB, p);
        }

        /// <summary>
        /// Generates a sample from the Beta distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _shapeA, _shapeB);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _shapeA, _shapeB);
        }

        /// <summary>
        /// Generates a sequence of samples from the Beta distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _shapeA, _shapeB);
        }

        /// <summary>
        /// Samples Beta distributed random variables by sampling two Gamma variables and normalizing.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>a random number from the Beta distribution.</returns>
        internal static double SampleUnchecked(System.Random rnd, double a, double b)
        {
            double x, y;
            if (a == b)
            {
                x = Gamma.SampleUnchecked(rnd, a, 1.0);
                y = Gamma.SampleUnchecked(rnd, b, 1.0);
                //When a==b (and possibly a==b==0), return value is equally possible to be 0 or 1
                if (x == 0 && y == 0)
                    return Bernoulli.Sample(0.5);//In particular, when a==b==0, Beta distribution degradates to Bernoulli distribution.
            }
            else
                do
                {
                    x = Gamma.SampleUnchecked(rnd, a, 1.0);
                    y = Gamma.SampleUnchecked(rnd, b, 1.0);
                } while (x == 0 && y == 0);//When a!=b, return value is not equally possible to be 0 or 1. Regenerate.
            return x / (x + y);
        }

        internal static void SamplesUnchecked(System.Random rnd, double[] values, double a, double b)
        {
            var y = new double[values.Length];
            Gamma.SamplesUnchecked(rnd, values, a, 1.0);
            Gamma.SamplesUnchecked(rnd, y, b, 1.0);
            CommonParallel.For(0, values.Length, 4096, (aa, bb) =>
            {
                for (int i = aa; i < bb; i++)
                {
                    values[i] = values[i]/(values[i] + y[i]);
                }
            });
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double a, double b)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, a, b);
            }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double a, double b, double x)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (x < 0.0 || x > 1.0)
            {
                return 0.0;
            }

            if (double.IsPositiveInfinity(a) && double.IsPositiveInfinity(b))
            {
                return x == 0.5 ? double.PositiveInfinity : 0.0;
            }

            if (double.IsPositiveInfinity(a))
            {
                return x == 1.0 ? double.PositiveInfinity : 0.0;
            }

            if (double.IsPositiveInfinity(b))
            {
                return x == 0.0 ? double.PositiveInfinity : 0.0;
            }

            if (a == 0.0 && b == 0.0)
            {
                if (x == 0.0 || x == 1.0)
                {
                    return double.PositiveInfinity;
                }

                return 0.0;
            }

            if (a == 0.0)
            {
                return x == 0.0 ? double.PositiveInfinity : 0.0;
            }

            if (b == 0.0)
            {
                return x == 1.0 ? double.PositiveInfinity : 0.0;
            }

            if (a == 1.0 && b == 1.0)
            {
                return 1.0;
            }

            if (a > 80.0 || b > 80.0)
            {
                return Math.Exp(PDFLn(a, b, x));
            }

            var bb = SpecialFunctions.Gamma(a + b)/(SpecialFunctions.Gamma(a)*SpecialFunctions.Gamma(b));
            return bb*Math.Pow(x, a - 1.0)*Math.Pow(1.0 - x, b - 1.0);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double a, double b, double x)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (x < 0.0 || x > 1.0)
            {
                return double.NegativeInfinity;
            }

            if (double.IsPositiveInfinity(a) && double.IsPositiveInfinity(b))
            {
                return x == 0.5 ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (double.IsPositiveInfinity(a))
            {
                return x == 1.0 ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (double.IsPositiveInfinity(b))
            {
                return x == 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (a == 0.0 && b == 0.0)
            {
                return x == 0.0 || x == 1.0 ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (a == 0.0)
            {
                return x == 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (b == 0.0)
            {
                return x == 1.0 ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (a == 1.0 && b == 1.0)
            {
                return 0.0;
            }

            var aa = SpecialFunctions.GammaLn(a + b) - SpecialFunctions.GammaLn(a) - SpecialFunctions.GammaLn(b);
            var bb = x == 0.0 ? (a == 1.0 ? 0.0 : double.NegativeInfinity) : (a - 1.0)*Math.Log(x);
            var cc = x == 1.0 ? (b == 1.0 ? 0.0 : double.NegativeInfinity) : (b - 1.0)*Math.Log(1.0 - x);

            return aa + bb + cc;
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double a, double b, double x)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (x < 0.0)
            {
                return 0.0;
            }

            if (x >= 1.0)
            {
                return 1.0;
            }

            if (double.IsPositiveInfinity(a) && double.IsPositiveInfinity(b))
            {
                return x < 0.5 ? 0.0 : 1.0;
            }

            if (double.IsPositiveInfinity(a))
            {
                return x < 1.0 ? 0.0 : 1.0;
            }

            if (double.IsPositiveInfinity(b))
            {
                return x >= 0.0 ? 1.0 : 0.0;
            }

            if (a == 0.0 && b == 0.0)
            {
                if (x >= 0.0 && x < 1.0)
                {
                    return 0.5;
                }

                return 1.0;
            }

            if (a == 0.0)
            {
                return 1.0;
            }

            if (b == 0.0)
            {
                return x >= 1.0 ? 1.0 : 0.0;
            }

            if (a == 1.0 && b == 1.0)
            {
                return x;
            }

            return SpecialFunctions.BetaRegularized(a, b, x);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        /// <remarks>WARNING: currently not an explicit implementation, hence slow and unreliable.</remarks>
        public static double InvCDF(double a, double b, double p)
        {
            if (a < 0.0 || b < 0.0 || p < 0.0 || p > 1.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Brent.FindRoot(x => SpecialFunctions.BetaRegularized(a, b, x) - p, 0.0, 1.0, accuracy: 1e-12);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double a, double b)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, a, b);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double a, double b)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, a, b);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double a, double b)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, a, b);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double a, double b)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, a, b);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double a, double b)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, a, b);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double a, double b)
        {
            if (a < 0.0 || b < 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, a, b);
        }
    }
}
