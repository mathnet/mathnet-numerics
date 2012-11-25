// <copyright file="Beta.cs" company="Math.NET">
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
    /// Implements the Beta distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Beta_distribution">Wikipedia - Beta distribution</a>.
    /// </summary>
    /// <remarks>
    /// <para>There are a few special cases for the parameterization of the Beta distribution. When both
    /// shape parameters are positive infinity, the Beta distribution degenerates to a point distribution
    /// at 0.5. When one of the shape parameters is positive infinity, the distribution degenerates to a point
    /// distribution at the positive infinity. When both shape parameters are 0.0, the Beta distribution 
    /// degenerates to a Bernoulli distribution with parameter 0.5. When one shape parameter is 0.0, the
    /// distribution degenerates to a point distribution at the non-zero shape parameter.</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Beta : IContinuousDistribution
    {
        /// <summary>
        /// Beta shape parameter a.
        /// </summary>
        double _shapeA;

        /// <summary>
        /// Beta shape parameter b.
        /// </summary>
        double _shapeB;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the Beta class.
        /// </summary>
        /// <param name="a">The a shape parameter of the Beta distribution.</param>
        /// <param name="b">The b shape parameter of the Beta distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">If any of the Beta parameters are negative.</exception>
        public Beta(double a, double b)
        {
            SetParameters(a, b);
            RandomSource = new Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>A string representation of the Beta distribution.</returns>
        public override string ToString()
        {
            return "Beta(A = " + _shapeA + ", B = " + _shapeB + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="a">The a shape parameter of the Beta distribution.</param>
        /// <param name="b">The b shape parameter of the Beta distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double a, double b)
        {
            if (a < 0.0 || b < 0.0 || Double.IsNaN(a) || Double.IsNaN(b))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="a">The a shape parameter of the Beta distribution.</param>
        /// <param name="b">The b shape parameter of the Beta distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double a, double b)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(a, b))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _shapeA = a;
            _shapeB = b;
        }

        /// <summary>
        /// Gets or sets the A shape parameter of the Beta distribution.
        /// </summary>
        public double A
        {
            get { return _shapeA; }
            set { SetParameters(value, _shapeB); }
        }

        /// <summary>
        /// Gets or sets the B shape parameter of the Beta distribution.
        /// </summary>
        public double B
        {
            get { return _shapeB; }
            set { SetParameters(_shapeA, value); }
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

                if (Double.IsPositiveInfinity(_shapeA) && Double.IsPositiveInfinity(_shapeB))
                {
                    return 0.5;
                }

                if (Double.IsPositiveInfinity(_shapeA))
                {
                    return 1.0;
                }

                if (Double.IsPositiveInfinity(_shapeB))
                {
                    return 0.0;
                }

                return _shapeA / (_shapeA + _shapeB);
            }
        }

        /// <summary>
        /// Gets the variance of the Beta distribution.
        /// </summary>
        public double Variance
        {
            get { return (_shapeA * _shapeB) / ((_shapeA + _shapeB) * (_shapeA + _shapeB) * (_shapeA + _shapeB + 1.0)); }
        }

        /// <summary>
        /// Gets the standard deviation of the Beta distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt((_shapeA * _shapeB) / ((_shapeA + _shapeB) * (_shapeA + _shapeB) * (_shapeA + _shapeB + 1.0))); }
        }

        /// <summary>
        /// Gets the entropy of the Beta distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                if (Double.IsPositiveInfinity(_shapeA) || Double.IsPositiveInfinity(_shapeB))
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
                    - ((_shapeA - 1.0) * SpecialFunctions.DiGamma(_shapeA))
                    - ((_shapeB - 1.0) * SpecialFunctions.DiGamma(_shapeB))
                    + ((_shapeA + _shapeB - 2.0) * SpecialFunctions.DiGamma(_shapeA + _shapeB));
            }
        }

        /// <summary>
        /// Gets the skewness of the Beta distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (Double.IsPositiveInfinity(_shapeA) && Double.IsPositiveInfinity(_shapeB))
                {
                    return 0.0;
                }

                if (Double.IsPositiveInfinity(_shapeA))
                {
                    return -2.0;
                }

                if (Double.IsPositiveInfinity(_shapeB))
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

                return 2.0 * (_shapeB - _shapeA) * Math.Sqrt(_shapeA + _shapeB + 1.0)
                    / ((_shapeA + _shapeB + 2.0) * Math.Sqrt(_shapeA * _shapeB));
            }
        }

        #endregion

        #region IContinuousDistribution implementation

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

                if (Double.IsPositiveInfinity(_shapeA) && Double.IsPositiveInfinity(_shapeB))
                {
                    return 0.5;
                }

                if (Double.IsPositiveInfinity(_shapeA))
                {
                    return 1.0;
                }

                if (Double.IsPositiveInfinity(_shapeB))
                {
                    return 0.0;
                }

                if (_shapeA == 1.0 && _shapeB == 1.0)
                {
                    return 0.5;
                }

                return (_shapeA - 1) / (_shapeA + _shapeB - 2);
            }
        }

        /// <summary>
        /// Gets the median of the Beta distribution.
        /// </summary>
        public double Median
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the minimum of the Beta distribution.
        /// </summary>
        public double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the maximum of the Beta distribution.
        /// </summary>
        public double Maximum
        {
            get { return 1.0; }
        }

        /// <summary>
        /// Computes the density of the Beta distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            if (x < 0.0 || x > 1.0)
            {
                return 0.0;
            }

            if (Double.IsPositiveInfinity(_shapeA) && Double.IsPositiveInfinity(_shapeB))
            {
                return x == 0.5 ? Double.PositiveInfinity : 0.0;
            }

            if (Double.IsPositiveInfinity(_shapeA))
            {
                return x == 1.0 ? Double.PositiveInfinity : 0.0;
            }

            if (Double.IsPositiveInfinity(_shapeB))
            {
                return x == 0.0 ? Double.PositiveInfinity : 0.0;
            }

            if (_shapeA == 0.0 && _shapeB == 0.0)
            {
                if (x == 0.0 || x == 1.0)
                {
                    return Double.PositiveInfinity;
                }

                return 0.0;
            }

            if (_shapeA == 0.0)
            {
                return x == 0.0 ? Double.PositiveInfinity : 0.0;
            }

            if (_shapeB == 0.0)
            {
                return x == 1.0 ? Double.PositiveInfinity : 0.0;
            }

            if (_shapeA == 1.0 && _shapeB == 1.0)
            {
                return 1.0;
            }

            var b = SpecialFunctions.Gamma(_shapeA + _shapeB) / (SpecialFunctions.Gamma(_shapeA) * SpecialFunctions.Gamma(_shapeB));
            return b * Math.Pow(x, _shapeA - 1.0) * Math.Pow(1.0 - x, _shapeB - 1.0);
        }

        /// <summary>
        /// Computes the log density of the Beta distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            if (x < 0.0 || x > 1.0)
            {
                return Double.NegativeInfinity;
            }

            if (Double.IsPositiveInfinity(_shapeA) && Double.IsPositiveInfinity(_shapeB))
            {
                return x == 0.5 ? Double.PositiveInfinity : Double.NegativeInfinity;
            }

            if (Double.IsPositiveInfinity(_shapeA))
            {
                return x == 1.0 ? Double.PositiveInfinity : Double.NegativeInfinity;
            }

            if (Double.IsPositiveInfinity(_shapeB))
            {
                return x == 0.0 ? Double.PositiveInfinity : Double.NegativeInfinity;
            }

            if (_shapeA == 0.0 && _shapeB == 0.0)
            {
                if (x == 0.0 || x == 1.0)
                {
                    return Double.PositiveInfinity;
                }

                return Double.NegativeInfinity;
            }

            if (_shapeA == 0.0)
            {
                return x == 0.0 ? Double.PositiveInfinity : Double.NegativeInfinity;
            }

            if (_shapeB == 0.0)
            {
                return x == 1.0 ? Double.PositiveInfinity : Double.NegativeInfinity;
            }

            if (_shapeA == 1.0 && _shapeB == 1.0)
            {
                return 0.0;
            }

            var a = SpecialFunctions.GammaLn(_shapeA + _shapeB) - SpecialFunctions.GammaLn(_shapeA) - SpecialFunctions.GammaLn(_shapeB);
            var b = x == 0.0 ? (_shapeA == 1.0 ? 0.0 : Double.NegativeInfinity) : (_shapeA - 1.0) * Math.Log(x);
            var c = x == 1.0 ? (_shapeB == 1.0 ? 0.0 : Double.NegativeInfinity) : (_shapeB - 1.0) * Math.Log(1.0 - x);

            return a + b + c;
        }

        /// <summary>
        /// Computes the cumulative distribution function of the Beta distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < 0.0)
            {
                return 0.0;
            }

            if (x >= 1.0)
            {
                return 1.0;
            }

            if (Double.IsPositiveInfinity(_shapeA) && Double.IsPositiveInfinity(_shapeB))
            {
                return x < 0.5 ? 0.0 : 1.0;
            }

            if (Double.IsPositiveInfinity(_shapeA))
            {
                return x < 1.0 ? 0.0 : 1.0;
            }

            if (Double.IsPositiveInfinity(_shapeB))
            {
                return x >= 0.0 ? 1.0 : 0.0;
            }

            if (_shapeA == 0.0 && _shapeB == 0.0)
            {
                if (x >= 0.0 && x < 1.0)
                {
                    return 0.5;
                }

                return 1.0;
            }

            if (_shapeA == 0.0)
            {
                return 1.0;
            }

            if (_shapeB == 0.0)
            {
                return x >= 1.0 ? 1.0 : 0.0;
            }

            if (_shapeA == 1.0 && _shapeB == 1.0)
            {
                return x;
            }

            return SpecialFunctions.BetaRegularized(_shapeA, _shapeB, x);
        }

        #endregion

        /// <summary>
        /// Samples Beta distributed random variables by sampling two Gamma variables and normalizing.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The A shape parameter.</param>
        /// <param name="b">The B shape parameter.</param>
        /// <returns>a random number from the Beta distribution.</returns>
        internal static double SampleUnchecked(Random rnd, double a, double b)
        {
            var x = Gamma.SampleUnchecked(rnd, a, 1.0);
            var y = Gamma.SampleUnchecked(rnd, b, 1.0);
            return x / (x + y);
        }

        /// <summary>
        /// Generates a sample from the Beta distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _shapeA, _shapeB);
        }

        /// <summary>
        /// Generates a sequence of samples from the Beta distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _shapeA, _shapeB);
            }
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The a shape parameter of the Beta distribution.</param>
        /// <param name="b">The b shape parameter of the Beta distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rnd, double a, double b)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(a, b))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, a, b);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The a shape parameter of the Beta distribution.</param>
        /// <param name="b">The b shape parameter of the Beta distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rnd, double a, double b)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(a, b))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, a, b);
            }
        }
    }
}
