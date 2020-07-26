// <copyright file="Stable.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Stable distribution.
    /// A random variable is said to be stable (or to have a stable distribution) if it has
    /// the property that a linear combination of two independent copies of the variable has
    /// the same distribution, up to location and scale parameters.
    /// For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Stable_distribution">Wikipedia - Stable distribution</a>.
    /// </summary>
    public class Stable : IContinuousDistribution
    {
        System.Random _random;

        readonly double _alpha;
        readonly double _beta;
        readonly double _scale;
        readonly double _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stable"/> class.
        /// </summary>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        public Stable(double alpha, double beta, double scale, double location)
        {
            if (!IsValidParameterSet(alpha, beta, scale, location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _alpha = alpha;
            _beta = beta;
            _scale = scale;
            _location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stable"/> class.
        /// </summary>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Stable(double alpha, double beta, double scale, double location, System.Random randomSource)
        {
            if (!IsValidParameterSet(alpha, beta, scale, location))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _alpha = alpha;
            _beta = beta;
            _scale = scale;
            _location = location;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Stable(α = {_alpha}, β = {_beta}, c = {_scale}, μ = {_location})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        public static bool IsValidParameterSet(double alpha, double beta, double scale, double location)
        {
            return alpha > 0.0 && alpha <= 2.0 && beta >= -1.0 && beta <= 1.0 && scale > 0.0 && !double.IsNaN(location);
        }

        /// <summary>
        /// Gets the stability (α) of the distribution. Range: 2 ≥ α > 0.
        /// </summary>
        public double Alpha => _alpha;

        /// <summary>
        /// Gets The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.
        /// </summary>
        public double Beta => _beta;

        /// <summary>
        /// Gets the scale (c) of the distribution. Range: c > 0.
        /// </summary>
        public double Scale => _scale;

        /// <summary>
        /// Gets the location (μ) of the distribution.
        /// </summary>
        public double Location => _location;

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
                if (_alpha <= 1d)
                {
                    throw new NotSupportedException();
                }

                return _location;
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_alpha == 2d)
                {
                    return 2.0*_scale*_scale;
                }

                return double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get
            {
                if (_alpha == 2d)
                {
                    return Constants.Sqrt2*_scale;
                }

                return double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Gets he entropy of the distribution.
        /// </summary>
        /// <remarks>Always throws a not supported exception.</remarks>
        public double Entropy => throw new NotSupportedException();

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        /// <remarks>Throws a not supported exception of <c>Alpha</c> != 2.</remarks>
        public double Skewness
        {
            get
            {
                if (_alpha != 2d)
                {
                    throw new NotSupportedException();
                }

                return 0.0;
            }
        }

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        /// <remarks>Throws a not supported exception if <c>Beta != 0</c>.</remarks>
        public double Mode
        {
            get
            {
                if (_beta != 0d)
                {
                    throw new NotSupportedException();
                }

                return _location;
            }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <remarks>Throws a not supported exception if <c>Beta != 0</c>.</remarks>
        public double Median
        {
            get
            {
                if (_beta != 0)
                {
                    throw new NotSupportedException();
                }

                return _location;
            }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get
            {
                if (Math.Abs(_beta) == 1)
                {
                    return 0.0;
                }

                return double.NegativeInfinity;
            }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum => double.PositiveInfinity;

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            return PDF(_alpha, _beta, _scale, _location, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return PDFLn(_alpha, _beta, _scale, _location, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <remarks>Throws a not supported exception if <c>Alpha != 2</c>, <c>(Alpha != 1 and Beta !=0)</c>, or <c>(Alpha != 0.5 and Beta != 1)</c></remarks>
        public double CumulativeDistribution(double x)
        {
            return CDF(_alpha, _beta, _scale, _location, x);
        }

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <returns>a random number from the distribution.</returns>
        static double SampleUnchecked(System.Random rnd, double alpha, double beta, double scale, double location)
        {
            var randTheta = ContinuousUniform.Sample(rnd, -Constants.PiOver2, Constants.PiOver2);
            var randW = Exponential.Sample(rnd, 1.0);

            if (!1.0.AlmostEqual(alpha))
            {
                var theta = (1.0/alpha)*Math.Atan(beta*Math.Tan(Constants.PiOver2*alpha));
                var angle = alpha*(randTheta + theta);
                var part1 = beta*Math.Tan(Constants.PiOver2*alpha);

                var factor = Math.Pow(1.0 + (part1*part1), 1.0/(2.0*alpha));
                var factor1 = Math.Sin(angle)/Math.Pow(Math.Cos(randTheta), 1.0/alpha);
                var factor2 = Math.Pow(Math.Cos(randTheta - angle)/randW, (1 - alpha)/alpha);

                return location + scale*(factor*factor1*factor2);
            }
            else
            {
                var part1 = Constants.PiOver2 + (beta*randTheta);
                var summand = part1*Math.Tan(randTheta);
                var subtrahend = beta*Math.Log(Constants.PiOver2*randW*Math.Cos(randTheta)/part1);

                return location + scale*Constants.TwoInvPi*(summand - subtrahend);
            }
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double alpha, double beta, double scale, double location)
        {
            var randThetas = new double[values.Length];
            var randWs = new double[values.Length];
            ContinuousUniform.SamplesUnchecked(rnd, randThetas, -Constants.PiOver2, Constants.PiOver2);
            Exponential.SamplesUnchecked(rnd, randWs, 1.0);

            if (!1.0.AlmostEqual(alpha))
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var randTheta = randThetas[i];

                    var theta = (1.0/alpha)*Math.Atan(beta*Math.Tan(Constants.PiOver2*alpha));
                    var angle = alpha*(randTheta + theta);
                    var part1 = beta*Math.Tan(Constants.PiOver2*alpha);

                    var factor = Math.Pow(1.0 + (part1*part1), 1.0/(2.0*alpha));
                    var factor1 = Math.Sin(angle)/Math.Pow(Math.Cos(randTheta), 1.0/alpha);
                    var factor2 = Math.Pow(Math.Cos(randTheta - angle)/randWs[i], (1 - alpha)/alpha);

                    values[i] = location + scale*(factor*factor1*factor2);
                }
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var randTheta = randThetas[i];

                    var part1 = Constants.PiOver2 + (beta*randTheta);
                    var summand = part1*Math.Tan(randTheta);
                    var subtrahend = beta*Math.Log(Constants.PiOver2*randWs[i]*Math.Cos(randTheta)/part1);

                    values[i] = location + scale*Constants.TwoInvPi*(summand - subtrahend);
                }
            }
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double alpha, double beta, double scale, double location)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, alpha, beta, scale, location);
            }
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _alpha, _beta, _scale, _location);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _alpha, _beta, _scale, _location);
        }

        /// <summary>
        /// Generates a sequence of samples from the Stable distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _alpha, _beta, _scale, _location);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double alpha, double beta, double scale, double location, double x)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (alpha == 2d)
            {
                return Normal.PDF(location, Constants.Sqrt2*scale, x);
            }

            if (alpha == 1d && beta == 0d)
            {
                return Cauchy.PDF(location, scale, x);
            }

            if (alpha == 0.5d && beta == 1d && x >= location)
            {
                return (Math.Sqrt(scale/Constants.Pi2)*Math.Exp(-scale/(2*(x - location))))/Math.Pow(x - location, 1.5);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double alpha, double beta, double scale, double location, double x)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (alpha == 2d)
            {
                return Normal.PDFLn(location, Constants.Sqrt2*scale, x);
            }

            if (alpha == 1d && beta == 0d)
            {
                return Cauchy.PDFLn(location, scale, x);
            }

            if (alpha == 0.5d && beta == 1d && x >= location)
            {
                return Math.Log(scale/Constants.Pi2)/2 - scale/(2*(x - location)) - 1.5*Math.Log(x - location);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double alpha, double beta, double scale, double location, double x)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (alpha == 2d)
            {
                return Normal.CDF(location, Constants.Sqrt2*scale, x);
            }

            if (alpha == 1d && beta == 0d)
            {
                return Cauchy.CDF(location, scale, x);
            }

            if (alpha == 0.5d && beta == 1d)
            {
                return SpecialFunctions.Erfc(Math.Sqrt(scale/(2*(x - location))));
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double alpha, double beta, double scale, double location)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, alpha, beta, scale, location);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double alpha, double beta, double scale, double location)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, alpha, beta, scale, location);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double alpha, double beta, double scale, double location)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, alpha, beta, scale, location);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double alpha, double beta, double scale, double location)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, alpha, beta, scale, location);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double alpha, double beta, double scale, double location)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, alpha, beta, scale, location);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="alpha">The stability (α) of the distribution. Range: 2 ≥ α > 0.</param>
        /// <param name="beta">The skewness (β) of the distribution. Range: 1 ≥ β ≥ -1.</param>
        /// <param name="scale">The scale (c) of the distribution. Range: c > 0.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double alpha, double beta, double scale, double location)
        {
            if (alpha <= 0.0 || alpha > 2.0 || beta < -1.0 || beta > 1.0 || scale <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, alpha, beta, scale, location);
        }
    }
}
