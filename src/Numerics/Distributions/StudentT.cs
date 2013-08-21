// <copyright file="StudentT.cs" company="Math.NET">
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

        double _location;
        double _scale;
        double _freedom;

        /// <summary>
        /// Initializes a new instance of the StudentT class. This is a Student t-distribution with location 0.0
        /// scale 1.0 and degrees of freedom 1. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        public StudentT()
            : this(0.0, 1.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the StudentT class with a particular location, scale and degrees of
        /// freedom. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="location">The location of the Student t-distribution.</param>
        /// <param name="scale">The scale of the Student t-distribution.</param>
        /// <param name="dof">The degrees of freedom for the Student t-distribution.</param>
        public StudentT(double location, double scale, double dof)
        {
            _random = new System.Random();
            SetParameters(location, scale, dof);
        }

        /// <summary>
        /// Initializes a new instance of the StudentT class with a particular location, scale and degrees of
        /// freedom. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="location">The location of the Student t-distribution.</param>
        /// <param name="scale">The scale of the Student t-distribution.</param>
        /// <param name="dof">The degrees of freedom for the Student t-distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public StudentT(double location, double scale, double dof, System.Random randomSource)
        {
            _random = randomSource ?? new System.Random();
            SetParameters(location, scale, dof);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "StudentT(Location = " + _location + ", Scale = " + _scale + ", DoF = " + _freedom + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="location">The location of the Student t-distribution.</param>
        /// <param name="scale">The scale of the Student t-distribution.</param>
        /// <param name="dof">The degrees of freedom for the Student t-distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double location, double scale, double dof)
        {
            return scale > 0.0 && dof > 0.0 && !Double.IsNaN(location);
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="location">The location of the Student t-distribution.</param>
        /// <param name="scale">The scale of the Student t-distribution.</param>
        /// <param name="dof">The degrees of freedom for the Student t-distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double location, double scale, double dof)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(location, scale, dof))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _location = location;
            _scale = scale;
            _freedom = dof;
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get { return _random; }
            set { _random = value ?? new System.Random(); }
        }

        /// <summary>
        /// Gets or sets the location of the Student t-distribution.
        /// </summary>
        public double Location
        {
            get { return _location; }
            set { SetParameters(value, _scale, _freedom); }
        }

        /// <summary>
        /// Gets or sets the scale of the Student t-distribution.
        /// </summary>
        public double Scale
        {
            get { return _scale; }
            set { SetParameters(_location, value, _freedom); }
        }

        /// <summary>
        /// Gets or sets the degrees of freedom of the Student t-distribution.
        /// </summary>
        public double DegreesOfFreedom
        {
            get { return _freedom; }
            set { SetParameters(_location, _scale, value); }
        }

        /// <summary>
        /// Gets the mean of the Student t-distribution.
        /// </summary>
        public double Mean
        {
            get { return _freedom > 1.0 ? _location : Double.NaN; }
        }

        /// <summary>
        /// Gets the variance of the Student t-distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (Double.IsPositiveInfinity(_freedom))
                {
                    return _scale*_scale;
                }

                if (_freedom > 2.0)
                {
                    return _freedom*_scale*_scale/(_freedom - 2.0);
                }

                return _freedom > 1.0 ? Double.PositiveInfinity : Double.NaN;
            }
        }

        /// <summary>
        /// Gets the standard deviation of the Student t-distribution.
        /// </summary>
        public double StdDev
        {
            get
            {
                if (Double.IsPositiveInfinity(_freedom))
                {
                    return Math.Sqrt(_scale*_scale);
                }

                if (_freedom > 2.0)
                {
                    return Math.Sqrt(_freedom*_scale*_scale/(_freedom - 2.0));
                }

                return _freedom > 1.0 ? Double.PositiveInfinity : Double.NaN;
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

                return (((_freedom + 1.0)/2.0)*(SpecialFunctions.DiGamma((1.0 + _freedom)/2.0) - SpecialFunctions.DiGamma(_freedom/2.0))) + Math.Log(Math.Sqrt(_freedom)*SpecialFunctions.Beta(_freedom/2.0, 1.0/2.0));
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
        public double Mode
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the median of the Student t-distribution.
        /// </summary>
        public double Median
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the minimum of the Student t-distribution.
        /// </summary>
        public double Minimum
        {
            get { return Double.NegativeInfinity; }
        }

        /// <summary>
        /// Gets the maximum of the Student t-distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the density of the distribution (PDF), i.e. dP(X &lt;= x)/dx.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            // TODO JVG we can probably do a better job for Cauchy special case
            if (_freedom >= 1e+8d)
            {
                return Normal.Density(_location, _scale, x);
            }

            var d = (x - _location)/_scale;
            return Math.Exp(SpecialFunctions.GammaLn((_freedom + 1.0)/2.0) - SpecialFunctions.GammaLn(_freedom/2.0))
                   *Math.Pow(1.0 + (d*d/_freedom), -0.5*(_freedom + 1.0))
                   /Math.Sqrt(_freedom*Math.PI)
                   /_scale;
        }

        /// <summary>
        /// Computes the log density of the distribution (lnPDF), i.e. ln(dP(X &lt;= x)/dx).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            // TODO JVG we can probably do a better job for Cauchy special case
            if (_freedom >= 1e+8d)
            {
                return Normal.DensityLn(_location, _scale, x);
            }

            var d = (x - _location)/_scale;
            return SpecialFunctions.GammaLn((_freedom + 1.0)/2.0)
                   - (0.5*((_freedom + 1.0)*Math.Log(1.0 + (d*d/_freedom))))
                   - SpecialFunctions.GammaLn(_freedom/2.0)
                   - (0.5*Math.Log(_freedom*Math.PI)) - Math.Log(_scale);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution, i.e. P(X &lt;= x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            // TODO JVG we can probably do a better job for Cauchy special case
            if (Double.IsPositiveInfinity(_freedom))
            {
                return Normal.CumulativeDistribution(_location, _scale, x);
            }

            var k = (x - _location)/_scale;
            var h = _freedom/(_freedom + (k*k));
            var ib = 0.5*SpecialFunctions.BetaRegularized(_freedom/2.0, 0.5, h);
            return x <= _location ? ib : 1.0 - ib;
        }

        /// <summary>
        /// Samples student-t distributed random variables.
        /// </summary>
        /// <remarks>The algorithm is method 2 in section 5, chapter 9 
        /// in L. Devroye's "Non-Uniform Random Variate Generation"</remarks>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="location">The location of the Student t-distribution.</param>
        /// <param name="scale">The scale of the Student t-distribution.</param>
        /// <param name="dof">The degrees of freedom for the standard student-t distribution.</param>
        /// <returns>a random number from the standard student-t distribution.</returns>
        internal static double SampleUnchecked(System.Random rnd, double location, double scale, double dof)
        {
            var n = Normal.SampleUncheckedBoxMuller(rnd).Item1;
            var g = Gamma.SampleUnchecked(rnd, 0.5*dof, 0.5);
            return location + (scale*n*Math.Sqrt(dof/g));
        }

        /// <summary>
        /// Generates a sample from the Student t-distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _location, _scale, _freedom);
        }

        /// <summary>
        /// Generates a sequence of samples from the Student t-distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _location, _scale, _freedom);
            }
        }

        /// <summary>
        /// Generates a sample from the Student t-distribution.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        /// <param name="location">The location of the Student t-distribution.</param>
        /// <param name="scale">The scale of the Student t-distribution.</param>
        /// <param name="dof">The degrees of freedom for the Student t-distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rng, double location, double scale, double dof)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(location, scale, dof))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rng, location, scale, dof);
        }

        /// <summary>
        /// Generates a sequence of samples from the Student t-distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        /// <param name="location">The location of the Student t-distribution.</param>
        /// <param name="scale">The scale of the Student t-distribution.</param>
        /// <param name="dof">The degrees of freedom for the Student t-distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rng, double location, double scale, double dof)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(location, scale, dof))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rng, location, scale, dof);
            }
        }
    }
}
