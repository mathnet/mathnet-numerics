// <copyright file="Gamma.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.Distributions
{
    using System;
    using System.Collections.Generic;
    using Properties;

    /// <summary>
    /// Implements the univariate Gamma distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Gamma_distribution">Wikipedia - Gamma distribution</a>.
    /// </summary>
    /// <remarks>
    /// <para>The Gamma distribution is parametrized by a shape and inverse scale parameter. When we want
    /// to specify a Gamma distribution which is a point distribution we set the shape parameter to be the
    /// location of the point distribution and the inverse scale as positive infinity. The distribution
    /// with shape and inverse scale both zero is undefined.</para>
    /// <para> Random number generation for the Gamma distribution is based on the algorithm in:
    /// "A Simple Method for Generating Gamma Variables" - Marsaglia &amp; Tsang
    /// ACM Transactions on Mathematical Software, Vol. 26, No. 3, September 2000, Pages 363–372.</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Gamma : IContinuousDistribution
    {
        /// <summary>
        /// Gamma shape parameter.
        /// </summary>
        double _shape;

        /// <summary>
        /// Gamma inverse scale parameter.
        /// </summary>
        double _invScale;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the Gamma class.
        /// </summary>
        /// <param name="shape">The shape of the Gamma distribution.</param>
        /// <param name="invScale">The inverse scale of the Gamma distribution.</param>
        public Gamma(double shape, double invScale)
        {
            SetParameters(shape, invScale);
            RandomSource = new Random();
        }

        /// <summary>
        /// Constructs a Gamma distribution from a shape and scale parameter. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="shape">The shape of the Gamma distribution.</param>
        /// <param name="scale">The scale of the Gamma distribution.</param>
        /// <returns>a normal distribution.</returns>
        public static Gamma WithShapeScale(double shape, double scale)
        {
            return new Gamma(shape, 1.0 / scale);
        }

        /// <summary>
        /// Constructs a Gamma distribution from a shape and inverse scale parameter. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="shape">The shape of the Gamma distribution.</param>
        /// <param name="invScale">The inverse scale of the Gamma distribution.</param>
        /// <returns>a normal distribution.</returns>
        public static Gamma WithShapeInvScale(double shape, double invScale)
        {
            return new Gamma(shape, invScale);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Gamma(Shape = " + _shape + ", Inverse Scale = " + _invScale + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="shape">The shape of the Gamma distribution.</param>
        /// <param name="invScale">The inverse scale of the Gamma distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double shape, double invScale)
        {
            if (shape < 0.0 || invScale < 0.0 || Double.IsNaN(shape) || Double.IsNaN(invScale))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="shape">The shape of the Gamma distribution.</param>
        /// <param name="invScale">The inverse scale of the Gamma distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double shape, double invScale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(shape, invScale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _shape = shape;
            _invScale = invScale;
        }

        /// <summary>
        /// Gets or sets the shape of the Gamma distribution.
        /// </summary>
        public double Shape
        {
            get { return _shape; }

            set { SetParameters(value, _invScale); }
        }

        /// <summary>
        /// Gets or sets the scale of the Gamma distribution.
        /// </summary>
        public double Scale
        {
            get { return 1.0 / _invScale; }

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
        /// Gets or sets the inverse scale of the Gamma distribution.
        /// </summary>
        public double InvScale
        {
            get { return _invScale; }

            set { SetParameters(_shape, value); }
        }

        #region IDistribution implementation

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public Random RandomSource
        {
            get { return _random; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _random = value;
            }
        }

        /// <summary>
        /// Gets the mean of the Gamma distribution.
        /// </summary>
        public double Mean
        {
            get
            {
                if (Double.IsPositiveInfinity(_invScale))
                {
                    return _shape;
                }

                if (_invScale == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
                }

                return _shape / _invScale;
            }
        }

        /// <summary>
        /// Gets the variance of the Gamma distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (Double.IsPositiveInfinity(_invScale))
                {
                    return 0.0;
                }

                if (_invScale == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
                }

                return _shape / (_invScale * _invScale);
            }
        }

        /// <summary>
        /// Gets the standard deviation of the Gamma distribution.
        /// </summary>
        public double StdDev
        {
            get
            {
                if (Double.IsPositiveInfinity(_invScale))
                {
                    return 0.0;
                }

                if (_invScale == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
                }

                return Math.Sqrt(_shape / (_invScale * _invScale));
            }
        }

        /// <summary>
        /// Gets the entropy of the Gamma distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                if (Double.IsPositiveInfinity(_invScale))
                {
                    return 0.0;
                }

                if (_invScale == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
                }

                return _shape - Math.Log(_invScale) + SpecialFunctions.GammaLn(_shape) + ((1.0 - _shape) * SpecialFunctions.DiGamma(_shape));
            }
        }

        /// <summary>
        /// Gets the skewness of the Gamma distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (Double.IsPositiveInfinity(_invScale))
                {
                    return 0.0;
                }

                if (_invScale == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
                }

                return 2.0 / Math.Sqrt(_shape);
            }
        }

        #endregion

        #region IContinuousDistribution implementation

        /// <summary>
        /// Gets the mode of the Gamma distribution.
        /// </summary>
        public double Mode
        {
            get
            {
                if (Double.IsPositiveInfinity(_invScale))
                {
                    return _shape;
                }

                if (_invScale == 0.0 && _shape == 0.0)
                {
                    return Double.NaN;
                }

                return (_shape - 1.0) / _invScale;
            }
        }

        /// <summary>
        /// Gets the median of the Gamma distribution.
        /// </summary>
        public double Median
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the minimum of the Gamma distribution.
        /// </summary>
        public double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the maximum of the Gamma distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the density of the Gamma distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            if (Double.IsPositiveInfinity(_invScale))
            {
                return x == _shape ? Double.PositiveInfinity : 0.0;
            }

            if (_shape == 0.0 && _invScale == 0.0)
            {
                return 0.0;
            }

            if (_shape == 1.0)
            {
                return _invScale * Math.Exp(-_invScale * x);
            }

            return Math.Pow(_invScale, _shape) * Math.Pow(x, _shape - 1.0) * Math.Exp(-_invScale * x) / SpecialFunctions.Gamma(_shape);
        }

        /// <summary>
        /// Computes the log density of the Gamma distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            if (Double.IsPositiveInfinity(_invScale))
            {
                return x == _shape ? Double.PositiveInfinity : Double.NegativeInfinity;
            }

            if (_shape == 0.0 && _invScale == 0.0)
            {
                return Double.NegativeInfinity;
            }

            if (_shape == 1.0)
            {
                return Math.Log(_invScale) - (_invScale * x);
            }

            return (_shape * Math.Log(_invScale)) + ((_shape - 1.0) * Math.Log(x)) - (_invScale * x) - SpecialFunctions.GammaLn(_shape);
        }

        /// <summary>
        /// Computes the cumulative distribution function of the Gamma distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (Double.IsPositiveInfinity(_invScale))
            {
                return x >= _shape ? 1.0 : 0.0;
            }

            if (_shape == 0.0 && _invScale == 0.0)
            {
                return 0.0;
            }

            return SpecialFunctions.GammaLowerRegularized(_shape, x * _invScale);
        }

        #endregion

        /// <summary>
        /// <para>Sampling implementation based on:
        /// "A Simple Method for Generating Gamma Variables" - Marsaglia &amp; Tsang
        /// ACM Transactions on Mathematical Software, Vol. 26, No. 3, September 2000, Pages 363–372.</para>
        /// <para>This method performs no parameter checks.</para>
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="shape">The shape of the Gamma distribution.</param>
        /// <param name="invScale">The inverse scale of the Gamma distribution.</param>
        /// <returns>A sample from a Gamma distributed random variable.</returns>
        internal static double SampleUnchecked(Random rnd, double shape, double invScale)
        {
            if (Double.IsPositiveInfinity(invScale))
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
                    return alphafix * d * v / invScale;
                }

                if (Math.Log(u) < (0.5 * x) + (d * (1.0 - v + Math.Log(v))))
                {
                    return alphafix * d * v / invScale;
                }
            }
        }

        /// <summary>
        /// Generates a sample from the Gamma distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _shape, _invScale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Gamma distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _shape, _invScale);
            }
        }

        /// <summary>
        /// Generates a sample from the Gamma distribution.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        /// <param name="shape">The shape of the Gamma distribution from which to generate samples.</param>
        /// <param name="invScale">The inverse scale of the Gamma distribution from which to generate samples.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rng, double shape, double invScale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(shape, invScale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rng, shape, invScale);
        }

        /// <summary>
        /// Generates a sequence of samples from the Gamma distribution.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        /// <param name="shape">The shape of the Gamma distribution from which to generate samples.</param>
        /// <param name="invScale">The inverse scale of the Gamma distribution from which to generate samples.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rng, double shape, double invScale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(shape, invScale))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rng, shape, invScale);
            }
        }
    }
}
