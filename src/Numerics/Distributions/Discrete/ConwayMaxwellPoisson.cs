// <copyright file="ConwayMaxwellPoisson.cs" company="Math.NET">
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
        /// <summary>
        /// Since many properties of the distribution can only be computed approximately, the tolerance
        /// level specifies how much error we accept.
        /// </summary>
        const double Tolerance = 1e-12;

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
        /// The lambda parameter.
        /// </summary>
        double _lambda;

        /// <summary>
        /// The nu parameter.
        /// </summary>
        double _nu;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConwayMaxwellPoisson"/> class. 
        /// </summary>
        /// <param name="lambda">
        /// The lambda parameter.
        /// </param>
        /// <param name="nu">
        /// The nu parameter.
        /// </param>
        public ConwayMaxwellPoisson(double lambda, double nu)
        {
            SetParameters(lambda, nu);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="lambda">The lambda parameter.</param>
        /// <param name="nu">The nu parameter.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
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
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="lambda">The lambda parameter.</param>
        /// <param name="nu">The nu parameter.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double lambda, double nu)
        {
            if (lambda <= 0.0)
            {
                return false;
            }

            if (nu < 0.0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the lambda parameter.
        /// </summary>
        /// <value>The value of the lambda parameter.</value>
        public double Lambda
        {
            get { return _lambda; }

            set { SetParameters(value, _nu); }
        }

        /// <summary>
        /// Gets or sets the Nu parameter.
        /// </summary>
        /// <value>The value of the Nu parameter.</value>
        public double Nu
        {
            get { return _nu; }

            set { SetParameters(_lambda, value); }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "ConwayMaxwellPoisson(Lambda = " + _lambda + ", Nu = " + _nu + ")";
        }

        #region IDistribution Members

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
                var a1 = _lambda * _lambda / Math.Pow(2, _nu);

                // The unnormalized mean.
                var zx = _lambda;

                // The contribution of the next term to the mean.
                var ax1 = 2 * a1;

                for (var i = 3; i < 1000; i++)
                {
                    var e = _lambda / Math.Pow(i, _nu);
                    var ex = _lambda / Math.Pow(i, _nu - 1) / (i - 1);
                    var a2 = a1 * e;
                    var ax2 = ax1 * ex;

                    var m = zx / z;
                    var upper = (zx + (ax1 / (1 - (ax2 / ax1)))) / z;
                    var lower = zx / (z + (a1 / (1 - (a2 / a1))));

                    if ((ax2 < ax1) && (a2 < a1))
                    {
                        var r = (upper - lower) / m;
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

                _mean = zx / z;
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
                var a1 = _lambda * _lambda / Math.Pow(2, _nu);

                // The unnormalized second moment.
                var zxx = _lambda;

                // The contribution of the next term to the second moment.
                var axx1 = 4 * a1;

                for (var i = 3; i < 1000; i++)
                {
                    var e = _lambda / Math.Pow(i, _nu);
                    var exx = _lambda / Math.Pow(i, _nu - 2) / (i - 1) / (i - 1);
                    var a2 = a1 * e;
                    var axx2 = axx1 * exx;

                    var m = zxx / z;
                    var upper = (zxx + (axx1 / (1 - (axx2 / axx1)))) / z;
                    var lower = zxx / (z + (a1 / (1 - (a2 / a1))));

                    if ((axx2 < axx1) && (a2 < a1))
                    {
                        var r = (upper - lower) / m;
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
                _variance = (zxx / z) - (mean * mean);
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
        /// Computes the cumulative distribution function of the <c>ConwayMaxwellPoisson</c> distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            double sum = 0;
            for (var i = 0; i < x + 1; i++)
            {
                sum += Probability(i);
            }

            return sum;
        }

        #endregion

        #region IDiscreteDistribution Members

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
        /// Computes the probability of a specific value.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>
        /// the probability mass at location <paramref name="k"/>.
        /// </returns>
        public double Probability(int k)
        {
            return Math.Pow(_lambda, k) / Math.Pow(SpecialFunctions.Factorial(k), _nu) / Z;
        }

        /// <summary>
        /// Computes the log probability of a specific value.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>
        /// the log probability mass at location <paramref name="k"/>.
        /// </returns>
        public double ProbabilityLn(int k)
        {
            return Math.Log(Probability(k));
        }

        #endregion

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
        /// <param name="lambda">The lambda parameter for the CMP distribution.</param>
        /// <param name="nu">The nu parameter for the CMP distribution.</param>
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
                var e = lambda / Math.Pow(i, nu);

                // The new term.
                t = t * e;

                // The updated normalization constant.
                z = z + t;

                // The stopping criterion.
                if (e < 1)
                {
                    if (t / (1 - e) / z < Tolerance)
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
        /// <param name="lambda">The lambda parameter</param>
        /// <param name="nu">The nu parameter.</param>
        /// <param name="z">The z parameter.</param>
        /// <returns>
        /// One sample from the distribution implied by <paramref name="lambda"/>, <paramref name="nu"/>, and <paramref name="z"/>.
        /// </returns>
        internal static int SampleUnchecked(Random rnd, double lambda, double nu, double z)
        {
            var u = rnd.NextDouble();
            var p = 1.0 / z;
            var cdf = p;
            var i = 0;

            while (u > cdf)
            {
                i++;
                p = p * lambda / Math.Pow(i, nu);
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
            return SampleUnchecked(RandomSource, _lambda, _nu, Z);
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
                yield return SampleUnchecked(RandomSource, _lambda, _nu, Z);
            }
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lambda">The lambda parameter</param>
        /// <param name="nu">The nu parameter.</param>
        public static int Sample(Random rnd, double lambda, double nu)
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
        /// <param name="lambda">The lambda parameter</param>
        /// <param name="nu">The nu parameter.</param>
        public static IEnumerable<int> Samples(Random rnd, double lambda, double nu)
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
