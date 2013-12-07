// <copyright file="ConwayMaxwellPoisson.cs" company="Math.NET">
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
    /// Discrete Univariate Conway-Maxwell-Poisson distribution.
    /// <para>The Conway-Maxwell-Poisson distribution is a generalization of the Poisson, Geometric and Bernoulli
    /// distributions. It is parameterized by two real numbers "lambda" and "nu". For
    /// <list>    
    ///     <item>nu = 0 the distribution reverts to a Geometric distribution</item>
    ///     <item>nu = 1 the distribution reverts to the Poisson distribution</item>
    ///     <item>nu -> infinity the distribution converges to a Bernoulli distribution</item>
    /// </list></para>
    /// This implementation will cache the value of the normalization constant.
    /// <a href="http://en.wikipedia.org/wiki/Conway%E2%80%93Maxwell%E2%80%93Poisson_distribution">Wikipedia - ConwayMaxwellPoisson distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class ConwayMaxwellPoisson : IDiscreteDistribution
    {
        System.Random _random;

        double _lambda;
        double _nu;

        /// <summary>
        /// The mean of the distribution.
        /// </summary>
        double _mean = double.MinValue;

        /// <summary>
        ///  The variance of the distribution.
        /// </summary>
        double _variance = double.MinValue;

        /// <summary>
        /// Caches the value of the normalization constant.
        /// </summary>
        double _z = double.MinValue;

        /// <summary>
        /// Since many properties of the distribution can only be computed approximately, the tolerance
        /// level specifies how much error we accept.
        /// </summary>
        const double Tolerance = 1e-12;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConwayMaxwellPoisson"/> class. 
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter. Range: λ > 0.</param>
        /// <param name="nu">The rate of decay (ν) parameter. Range: ν ≥ 0.</param>
        public ConwayMaxwellPoisson(double lambda, double nu)
        {
            _random = MersenneTwister.Default;
            SetParameters(lambda, nu);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConwayMaxwellPoisson"/> class. 
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter. Range: λ > 0.</param>
        /// <param name="nu">The rate of decay (ν) parameter. Range: ν ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public ConwayMaxwellPoisson(double lambda, double nu, System.Random randomSource)
        {
            _random = randomSource ?? MersenneTwister.Default;
            SetParameters(lambda, nu);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return "ConwayMaxwellPoisson(λ = " + _lambda + ", ν = " + _nu + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter. Range: λ > 0.</param>
        /// <param name="nu">The rate of decay (ν) parameter. Range: ν ≥ 0.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double lambda, double nu)
        {
            return lambda > 0.0 && nu >= 0.0;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter. Range: λ > 0.</param>
        /// <param name="nu">The rate of decay (ν) parameter. Range: ν ≥ 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double lambda, double nu)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lambda, nu))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _lambda = lambda;
            _nu = nu;
        }

        /// <summary>
        /// Gets or sets the lambda (λ) parameter. Range: λ > 0.
        /// </summary>
        public double Lambda
        {
            get { return _lambda; }
            set { SetParameters(value, _nu); }
        }

        /// <summary>
        /// Gets or sets the rate of decay (ν) parameter. Range: ν ≥ 0.
        /// </summary>
        public double Nu
        {
            get { return _nu; }
            set { SetParameters(_lambda, value); }
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get { return _random; }
            set { _random = value ?? MersenneTwister.Default; }
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean
        {
            get
            {
                // Special case requiring no computation.
                if (_lambda == 0)
                {
                    return 0.0;
                }

                if (_mean != double.MinValue)
                {
                    return _mean;
                }

                // The normalization constant for the distribution.
                var z = 1 + _lambda;

                // The probability of the next term.
                var a1 = _lambda*_lambda/Math.Pow(2, _nu);

                // The unnormalized mean.
                var zx = _lambda;

                // The contribution of the next term to the mean.
                var ax1 = 2*a1;

                for (var i = 3; i < 1000; i++)
                {
                    var e = _lambda/Math.Pow(i, _nu);
                    var ex = _lambda/Math.Pow(i, _nu - 1)/(i - 1);
                    var a2 = a1*e;
                    var ax2 = ax1*ex;

                    var m = zx/z;
                    var upper = (zx + (ax1/(1 - (ax2/ax1))))/z;
                    var lower = zx/(z + (a1/(1 - (a2/a1))));

                    if ((ax2 < ax1) && (a2 < a1))
                    {
                        var r = (upper - lower)/m;
                        if (r < Tolerance)
                        {
                            break;
                        }
                    }

                    z = z + a1;
                    zx = zx + ax1;
                    a1 = a2;
                    ax1 = ax2;
                }

                _mean = zx/z;
                return _mean;
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                // Special case requiring no computation.
                if (_lambda == 0)
                {
                    return 0.0;
                }

                if (_variance != double.MinValue)
                {
                    return _variance;
                }

                // The normalization constant for the distribution.
                var z = 1 + _lambda;

                // The probability of the next term.
                var a1 = _lambda*_lambda/Math.Pow(2, _nu);

                // The unnormalized second moment.
                var zxx = _lambda;

                // The contribution of the next term to the second moment.
                var axx1 = 4*a1;

                for (var i = 3; i < 1000; i++)
                {
                    var e = _lambda/Math.Pow(i, _nu);
                    var exx = _lambda/Math.Pow(i, _nu - 2)/(i - 1)/(i - 1);
                    var a2 = a1*e;
                    var axx2 = axx1*exx;

                    var m = zxx/z;
                    var upper = (zxx + (axx1/(1 - (axx2/axx1))))/z;
                    var lower = zxx/(z + (a1/(1 - (a2/a1))));

                    if ((axx2 < axx1) && (a2 < a1))
                    {
                        var r = (upper - lower)/m;
                        if (r < Tolerance)
                        {
                            break;
                        }
                    }

                    z = z + a1;
                    zxx = zxx + axx1;
                    a1 = a2;
                    axx1 = axx2;
                }

                var mean = Mean;
                _variance = (zxx/z) - (mean*mean);
                return _variance;
            }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(Variance); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the mode of the distribution
        /// </summary>
        public int Mode
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public int Median
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            return Math.Pow(_lambda, k)/Math.Pow(SpecialFunctions.Factorial(k), _nu)/Z;
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            return Math.Log(Probability(k));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            double sum = 0;
            for (var i = 0; i < x + 1; i++)
            {
                sum += Probability(i);
            }

            return sum;
        }

        /// <summary>
        /// Gets the normalization constant of the Conway-Maxwell-Poisson distribution.
        /// </summary>
        double Z
        {
            get
            {
                if (_z != double.MinValue)
                {
                    return _z;
                }

                _z = Normalization(_lambda, _nu);
                return _z;
            }
        }

        /// <summary>
        /// Computes an approximate normalization constant for the CMP distribution.
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter for the CMP distribution.</param>
        /// <param name="nu">The rate of decay (ν) parameter for the CMP distribution.</param>
        /// <returns>
        /// an approximate normalization constant for the CMP distribution.
        /// </returns>
        static double Normalization(double lambda, double nu)
        {
            // Initialize Z with the first two terms.
            var z = 1.0 + lambda;

            // Remembers the last term added.
            var t = lambda;

            // Start adding more terms until convergence.
            for (var i = 2; i < 1000; i++)
            {
                // The new addition for term i.
                var e = lambda/Math.Pow(i, nu);

                // The new term.
                t = t*e;

                // The updated normalization constant.
                z = z + t;

                // The stopping criterion.
                if (e < 1)
                {
                    if (t/(1 - e)/z < Tolerance)
                    {
                        break;
                    }
                }
            }

            return z;
        }

        /// <summary>
        /// Returns one trials from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lambda">The lambda (λ) parameter. Range: λ > 0.</param>
        /// <param name="nu">The rate of decay (ν) parameter. Range: ν ≥ 0.</param>
        /// <param name="z">The z parameter.</param>
        /// <returns>
        /// One sample from the distribution implied by <paramref name="lambda"/>, <paramref name="nu"/>, and <paramref name="z"/>.
        /// </returns>
        static int SampleUnchecked(System.Random rnd, double lambda, double nu, double z)
        {
            var u = rnd.NextDouble();
            var p = 1.0/z;
            var cdf = p;
            var i = 0;

            while (u > cdf)
            {
                i++;
                p = p*lambda/Math.Pow(i, nu);
                cdf += p;
            }

            return i;
        }

        /// <summary>
        /// Samples a Conway-Maxwell-Poisson distributed random variable.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public int Sample()
        {
            return SampleUnchecked(_random, _lambda, _nu, Z);
        }

        /// <summary>
        /// Samples a sequence of a Conway-Maxwell-Poisson distributed random variables.
        /// </summary>
        /// <returns>
        /// a sequence of samples from a Conway-Maxwell-Poisson distribution.
        /// </returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(_random, _lambda, _nu, Z);
            }
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lambda">The lambda (λ) parameter. Range: λ > 0.</param>
        /// <param name="nu">The rate of decay (ν) parameter. Range: ν ≥ 0.</param>
        public static int Sample(System.Random rnd, double lambda, double nu)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lambda, nu))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            var z = Normalization(lambda, nu);
            return SampleUnchecked(rnd, lambda, nu, z);
        }

        /// <summary>
        /// Samples a sequence of this random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lambda">The lambda (λ) parameter. Range: λ > 0.</param>
        /// <param name="nu">The rate of decay (ν) parameter. Range: ν ≥ 0.</param>
        public static IEnumerable<int> Samples(System.Random rnd, double lambda, double nu)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lambda, nu))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            var z = Normalization(lambda, nu);
            while (true)
            {
                yield return SampleUnchecked(rnd, lambda, nu, z);
            }
        }
    }
}
