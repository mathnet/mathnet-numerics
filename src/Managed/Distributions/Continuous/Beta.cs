// <copyright file="Beta.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.Distributions
{
    using System;
    using System.Collections.Generic;
    using Properties;

    /// <summary>
    /// Implements the Beta distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Beta_distribution">Wikipedia - Beta distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to false, all parameter checks can be turned off.</para></remarks>
    public class Beta : IContinuousDistribution
    {
        /// <summary>
        /// Beta shape parameter a.
        /// </summary>
        private double _shapeA;

        /// <summary>
        /// Beta shape parameter b.
        /// </summary>
        private double _shapeB;

        /// <summary>
        /// Initializes a new instance of the Beta distribution.
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
        public override string ToString()
        {
            return "Beta(A = " + _shapeA + ", B = " + _shapeB + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="a">The a shape parameter of the Beta distribution.</param>
        /// <param name="b">The b shape parameter of the Beta distribution.</param>
        /// <returns>True when the parameters are valid, false otherwise.</returns>
        private static bool IsValidParameterSet(double a, double b)
        {
            if (a < 0.0 || b < 0.0)
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
        private void SetParameters(double a, double b)
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
        public Random RandomSource { get; set; }

        /// <summary>
        /// Gets the mean of the Beta distribution.
        /// </summary>
        public double Mean
        {
            get { return _shapeA / (_shapeA + _shapeB); }
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
                return SpecialFunctions.BetaLn(_shapeA, _shapeB)
                       - (_shapeA - 1.0) * SpecialFunctions.DiGamma(_shapeA)
                       - (_shapeB - 1.0) * SpecialFunctions.DiGamma(_shapeB)
                       + (_shapeA + _shapeB - 2.0) * SpecialFunctions.DiGamma(_shapeA + _shapeB);
            }
        }

        /// <summary>
        /// Gets the skewness of the Beta distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                return 2.0 * (_shapeB - _shapeA) * Math.Sqrt(_shapeA + _shapeB + 1.0) 
                    / ((_shapeA + _shapeB + 2.0) * Math.Sqrt(_shapeA * _shapeB));
            }
        }
        #endregion

        #region IContinuousDistribution implementation

        /// <summary>
        /// Gets the mode of the Beta distribution.
        /// </summary>
        public double Mode
        {
            get { return (_shapeA - 1) / (_shapeA + _shapeB - 2); }
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
            double b = SpecialFunctions.Gamma(_shapeA + _shapeB) / (SpecialFunctions.Gamma(_shapeA) * SpecialFunctions.Gamma(_shapeB));
            return b * Math.Pow(x, _shapeA - 1.0) * Math.Pow(1.0 - x, _shapeB - 1.0);
        }

        /// <summary>
        /// Computes the log density of the Beta distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            double b = SpecialFunctions.GammaLn(_shapeA + _shapeB) - SpecialFunctions.GammaLn(_shapeA) - SpecialFunctions.GammaLn(_shapeB);
            return b + (_shapeA - 1.0)*Math.Log(x) + (_shapeB - 1.0)*Math.Log(1.0 - x);
        }

        /// <summary>
        /// Computes the cumulative distribution function of the Beta distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return SpecialFunctions.BetaRegularized(_shapeA, _shapeB, x);
        }

        /// <summary>
        /// Generates a sample from the Beta distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleBeta(RandomSource, _shapeA, _shapeB);
        }

        /// <summary>
        /// Generates a sequence of samples from the Beta distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleBeta(RandomSource, _shapeA, _shapeB);
            }
        }
        #endregion

        /// <summary>
        /// Generates a sample from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        /// <param name="a">The a shape parameter of the Beta distribution.</param>
        /// <param name="b">The b shape parameter of the Beta distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rng, double a, double b)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(a, b))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleBeta(rng, a, b);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        /// <param name="a">The a shape parameter of the Beta distribution.</param>
        /// <param name="b">The b shape parameter of the Beta distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rng, double a, double b)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(a, b))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleBeta(rng, a, b);
            }
        }

        /// <summary>
        /// Samples Beta distributed random variables by sampling two Gamma variables and normalizing.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The A shape parameter.</param>
        /// <param name="b">The B shape parameter.</param>
        /// <returns>a random number from the Beta distribution.</returns>
        internal static double SampleBeta(Random rnd, double a, double b)
        {
            double x = Gamma.SampleGamma(rnd, a, 1.0);
            double y = Gamma.SampleGamma(rnd, b, 1.0);
            return x / (x + y);
        }
    }
}
