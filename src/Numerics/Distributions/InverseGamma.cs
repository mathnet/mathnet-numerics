// <copyright file="InverseGamma.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate Inverse Gamma distribution.
    /// The inverse Gamma distribution is a distribution over the positive real numbers parameterized by
    /// two positive parameters.
    /// <a href="http://en.wikipedia.org/wiki/Inverse-gamma_distribution">Wikipedia - InverseGamma distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class InverseGamma : IContinuousDistribution
    {
        System.Random _random;

        double _shape;
        double _scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="InverseGamma"/> class. 
        /// </summary>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="scale">The scale (β) of the distribution. Range: β > 0.</param>
        public InverseGamma(double shape, double scale)
        {
            _random = new System.Random(Random.RandomSeed.Guid());
            SetParameters(shape, scale);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InverseGamma"/> class. 
        /// </summary>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="scale">The scale (β) of the distribution. Range: β > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public InverseGamma(double shape, double scale, System.Random randomSource)
        {
            _random = randomSource ?? new System.Random(Random.RandomSeed.Guid());
            SetParameters(shape, scale);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "InverseGamma(α = " + _shape + ", β = " + _scale + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="scale">The scale (β) of the distribution. Range: β > 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double shape, double scale)
        {
            if (shape <= 0.0 || scale <= 0.0 || Double.IsNaN(shape) || Double.IsNaN(scale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _shape = shape;
            _scale = scale;
        }

        /// <summary>
        /// Gets or sets the shape (α) parameter. Range: α > 0.
        /// </summary>
        public double Shape
        {
            get { return _shape; }
            set { SetParameters(value, _scale); }
        }

        /// <summary>
        /// Gets or sets The scale (β) parameter. Range: β > 0.
        /// </summary>
        public double Scale
        {
            get { return _scale; }
            set { SetParameters(_shape, value); }
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get { return _random; }
            set { _random = value ?? new System.Random(Random.RandomSeed.Guid()); }
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean
        {
            get
            {
                if (_shape <= 1)
                {
                    throw new NotSupportedException();
                }

                return _scale/(_shape - 1.0);
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_shape <= 2)
                {
                    throw new NotSupportedException();
                }

                return _scale*_scale/((_shape - 1.0)*(_shape - 1.0)*(_shape - 2.0));
            }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return _scale/(Math.Abs(_shape - 1.0)*Math.Sqrt(_shape - 2.0)); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return _shape + Math.Log(_scale) + SpecialFunctions.GammaLn(_shape) - ((1 + _shape)*SpecialFunctions.DiGamma(_shape)); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (_shape <= 3)
                {
                    throw new NotSupportedException();
                }

                return (4*Math.Sqrt(_shape - 2))/(_shape - 3);
            }
        }

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get { return _scale/(_shape + 1.0); }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <remarks>Throws <see cref="NotSupportedException"/>.</remarks>
        public double Median
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return x < 0.0 ? 0.0 : Math.Pow(_scale, _shape)*Math.Pow(x, -_shape - 1.0)*Math.Exp(-_scale/x)/SpecialFunctions.Gamma(_shape);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return Math.Log(Density(x));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return SpecialFunctions.GammaUpperRegularized(_shape, _scale/x);
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return 1.0/Gamma.Sample(_random, _shape, _scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Cauchy distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return Gamma.Samples(_random, _shape, _scale).Select(z => 1.0/z);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="scale">The scale (β) of the distribution. Range: β > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double shape, double scale, double x)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return x < 0.0 ? 0.0 : Math.Pow(scale, shape)*Math.Pow(x, -shape - 1.0)*Math.Exp(-scale/x)/SpecialFunctions.Gamma(shape);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="scale">The scale (β) of the distribution. Range: β > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double shape, double scale, double x)
        {
            return Math.Log(PDF(shape, scale, x));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="scale">The scale (β) of the distribution. Range: β > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double shape, double scale, double x)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return SpecialFunctions.GammaUpperRegularized(shape, scale/x);
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="scale">The scale (β) of the distribution. Range: β > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double shape, double scale)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return 1.0/Gamma.Sample(rnd, shape, scale);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape (α) of the distribution. Range: α > 0.</param>
        /// <param name="scale">The scale (β) of the distribution. Range: β > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double shape, double scale)
        {
            if (shape <= 0.0 || scale <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return Gamma.Samples(rnd, shape, scale).Select(z => 1.0/z);
        }
    }
}
