// <copyright file="StudentT.cs" company="Math.NET">
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
using MathNet.Numerics.RootFinding;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Student's T-distribution.
    /// Implements the univariate Student t-distribution. For details about this
    /// distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Student%27s_t-distribution">
    /// Wikipedia - Student's t-distribution</a>.
    /// </summary>
    /// <remarks><para>We use a slightly generalized version (compared to
    /// Wikipedia) of the Student t-distribution. Namely, one which also
    /// parameterizes the location and scale. See the book "Bayesian Data
    /// Analysis" by Gelman et al. for more details.</para>
    /// <para>The density of the Student t-distribution  p(x|mu,scale,dof) =
    /// Gamma((dof+1)/2) (1 + (x - mu)^2 / (scale * scale * dof))^(-(dof+1)/2) /
    /// (Gamma(dof/2)*Sqrt(dof*pi*scale)).</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by
    /// default.  Users can get/set the random number generator by using the
    /// <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters
    /// whether they are in the allowed range. This might involve heavy
    /// computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class StudentT : IContinuousDistribution
    {
        System.Random _random;

        readonly double _location;
        readonly double _scale;
        readonly double _freedom;

        /// <summary>
        /// Initializes a new instance of the StudentT class. This is a Student t-distribution with location 0.0
        /// scale 1.0 and degrees of freedom 1.
        /// </summary>
        public StudentT()
        {
            _random = SystemRandomSource.Default;
            _location = 0.0;
            _scale = 1.0;
            _freedom = 1.0;
        }

        /// <summary>
        /// Initializes a new instance of the StudentT class with a particular location, scale and degrees of
        /// freedom.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        public StudentT(double location, double scale, double freedom)
        {
            if (!IsValidParameterSet(location, scale, freedom))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _location = location;
            _scale = scale;
            _freedom = freedom;
        }

        /// <summary>
        /// Initializes a new instance of the StudentT class with a particular location, scale and degrees of
        /// freedom.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public StudentT(double location, double scale, double freedom, System.Random randomSource)
        {
            if (!IsValidParameterSet(location, scale, freedom))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _location = location;
            _scale = scale;
            _freedom = freedom;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"StudentT(μ = {_location}, σ = {_scale}, ν = {_freedom})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        public static bool IsValidParameterSet(double location, double scale, double freedom)
        {
            return scale > 0.0 && freedom > 0.0 && !double.IsNaN(location);
        }

        /// <summary>
        /// Gets the location (μ) of the Student t-distribution.
        /// </summary>
        public double Location => _location;

        /// <summary>
        /// Gets the scale (σ) of the Student t-distribution. Range: σ > 0.
        /// </summary>
        public double Scale => _scale;

        /// <summary>
        /// Gets the degrees of freedom (ν) of the Student t-distribution. Range: ν > 0.
        /// </summary>
        public double DegreesOfFreedom => _freedom;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the Student t-distribution.
        /// </summary>
        public double Mean => _freedom > 1.0 ? _location : double.NaN;

        /// <summary>
        /// Gets the variance of the Student t-distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (double.IsPositiveInfinity(_freedom))
                {
                    return _scale*_scale;
                }

                if (_freedom > 2.0)
                {
                    return _freedom*_scale*_scale/(_freedom - 2.0);
                }

                return _freedom > 1.0 ? double.PositiveInfinity : double.NaN;
            }
        }

        /// <summary>
        /// Gets the standard deviation of the Student t-distribution.
        /// </summary>
        public double StdDev
        {
            get
            {
                if (double.IsPositiveInfinity(_freedom))
                {
                    return Math.Sqrt(_scale*_scale);
                }

                if (_freedom > 2.0)
                {
                    return Math.Sqrt(_freedom*_scale*_scale/(_freedom - 2.0));
                }

                return _freedom > 1.0 ? double.PositiveInfinity : double.NaN;
            }
        }

        /// <summary>
        /// Gets the entropy of the Student t-distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                if (_location != 0 || _scale != 1.0)
                {
                    throw new NotSupportedException();
                }

                return (((_freedom + 1.0)/2.0)*(SpecialFunctions.DiGamma((1.0 + _freedom)/2.0) - SpecialFunctions.DiGamma(_freedom/2.0)))
                       + Math.Log(Math.Sqrt(_freedom)*SpecialFunctions.Beta(_freedom/2.0, 1.0/2.0));
            }
        }

        /// <summary>
        /// Gets the skewness of the Student t-distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (_freedom <= 3)
                {
                    throw new NotSupportedException();
                }

                return 0.0;
            }
        }

        /// <summary>
        /// Gets the mode of the Student t-distribution.
        /// </summary>
        public double Mode => _location;

        /// <summary>
        /// Gets the median of the Student t-distribution.
        /// </summary>
        public double Median => _location;

        /// <summary>
        /// Gets the minimum of the Student t-distribution.
        /// </summary>
        public double Minimum => double.NegativeInfinity;

        /// <summary>
        /// Gets the maximum of the Student t-distribution.
        /// </summary>
        public double Maximum => double.PositiveInfinity;

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            return PDF(_location, _scale, _freedom, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return PDFLn(_location, _scale, _freedom, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return CDF(_location, _scale, _freedom, x);
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
            return InvCDF(_location, _scale, _freedom, p);
        }

        /// <summary>
        /// Samples student-t distributed random variables.
        /// </summary>
        /// <remarks>The algorithm is method 2 in section 5, chapter 9
        /// in L. Devroye's "Non-Uniform Random Variate Generation"</remarks>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>a random number from the standard student-t distribution.</returns>
        static double SampleUnchecked(System.Random rnd, double location, double scale, double freedom)
        {
            var gamma = Gamma.SampleUnchecked(rnd, 0.5*freedom, 0.5);
            return Normal.Sample(rnd, location, scale*Math.Sqrt(freedom/gamma));
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double location, double scale, double freedom)
        {
            Gamma.SamplesUnchecked(rnd, values, 0.5*freedom, 0.5);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Normal.Sample(rnd, location, scale*Math.Sqrt(freedom/values[i]));
            }
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double location, double scale, double freedom)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, location, scale, freedom);
            }
        }

        /// <summary>
        /// Generates a sample from the Student t-distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _location, _scale, _freedom);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, _location, _scale, _freedom);
        }

        /// <summary>
        /// Generates a sequence of samples from the Student t-distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, _location, _scale, _freedom);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double location, double scale, double freedom, double x)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            // TODO JVG we can probably do a better job for Cauchy special case
            if (freedom >= 1e+8d)
            {
                return Normal.PDF(location, scale, x);
            }

            var d = (x - location)/scale;
            return Math.Exp(SpecialFunctions.GammaLn((freedom + 1.0)/2.0) - SpecialFunctions.GammaLn(freedom/2.0))
                   *Math.Pow(1.0 + (d*d/freedom), -0.5*(freedom + 1.0))
                   /Math.Sqrt(freedom*Math.PI)
                   /scale;
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double location, double scale, double freedom, double x)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            // TODO JVG we can probably do a better job for Cauchy special case
            if (freedom >= 1e+8d)
            {
                return Normal.PDFLn(location, scale, x);
            }

            var d = (x - location)/scale;
            return SpecialFunctions.GammaLn((freedom + 1.0)/2.0)
                   - (0.5*((freedom + 1.0)*Math.Log(1.0 + (d*d/freedom))))
                   - SpecialFunctions.GammaLn(freedom/2.0)
                   - (0.5*Math.Log(freedom*Math.PI)) - Math.Log(scale);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double location, double scale, double freedom, double x)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            // TODO JVG we can probably do a better job for Cauchy special case
            if (double.IsPositiveInfinity(freedom))
            {
                return Normal.CDF(location, scale, x);
            }

            var k = (x - location)/scale;
            var h = freedom/(freedom + (k*k));
            var ib = 0.5*SpecialFunctions.BetaRegularized(freedom/2.0, 0.5, h);
            return x <= location ? ib : 1.0 - ib;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        /// <remarks>WARNING: currently not an explicit implementation, hence slow and unreliable.</remarks>
        public static double InvCDF(double location, double scale, double freedom, double p)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            // TODO JVG we can probably do a better job for Cauchy special case
            if (double.IsPositiveInfinity(freedom))
            {
                return Normal.InvCDF(location, scale, p);
            }

            if (p == 0.5d)
            {
                return location;
            }

            // TODO PERF: We must implement this explicitly instead of solving for CDF^-1
            return Brent.FindRoot(x =>
            {
                var k = (x - location)/scale;
                var h = freedom/(freedom + (k*k));
                var ib = 0.5*SpecialFunctions.BetaRegularized(freedom/2.0, 0.5, h);
                return x <= location ? ib - p : 1.0 - ib - p;
            }, -800, 800, accuracy: 1e-12);
        }

        /// <summary>
        /// Generates a sample from the Student t-distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double location, double scale, double freedom)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, location, scale, freedom);
        }

        /// <summary>
        /// Generates a sequence of samples from the Student t-distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double location, double scale, double freedom)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, location, scale, freedom);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, double[] values, double location, double scale, double freedom)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, location, scale, freedom);
        }

        /// <summary>
        /// Generates a sample from the Student t-distribution.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(double location, double scale, double freedom)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, location, scale, freedom);
        }

        /// <summary>
        /// Generates a sequence of samples from the Student t-distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(double location, double scale, double freedom)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, location, scale, freedom);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(double[] values, double location, double scale, double freedom)
        {
            if (scale <= 0.0 || freedom <= 0.0)
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, location, scale, freedom);
        }
    }
}
