// <copyright file="Poisson.cs" company="Math.NET">
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
    /// Pseudo-random generation of poisson distributed deviates.
    /// </summary>
    /// <remarks> 
    /// <para>Distribution is described at <a href="http://en.wikipedia.org/wiki/Poisson_distribution"> Wikipedia - Poisson distribution</a>.</para>
    /// <para>Knuth's method is used to generate Poisson distributed random variables.</para>
    /// <para>f(x) = exp(-λ)*λ^x/x!;</para>
    /// </remarks>
    public class Poisson : IDiscreteDistribution
    {
        /// <summary>
        /// The Poisson distribution parameter λ.
        /// </summary>
        double _lambda;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Gets or sets the Poisson distribution parameter λ.
        /// </summary>
        public double Lambda
        {
            get { return _lambda; }

            set { SetParameters(value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Poisson"/> class.
        /// </summary>
        /// <param name="lambda">The Poisson distribution parameter λ.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">If <paramref name="lambda"/> is equal or less then 0.0.</exception>
        public Poisson(double lambda)
        {
            SetParameters(lambda);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="lambda">The mean (λ) of the distribution.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double lambda)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lambda))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _lambda = lambda;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="lambda">The mean (λ) of the distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double lambda)
        {
            return lambda > 0.00;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Poisson(λ = " + _lambda + ")";
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
            get { return _lambda; }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _lambda; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(_lambda); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        /// <remarks>Approximation, see Wikipedia <a href="http://en.wikipedia.org/wiki/Poisson_distribution">Poisson distribution</a></remarks>
        public double Entropy
        {
            get { return (0.5 * Math.Log(2 * Constants.Pi * Constants.E * _lambda)) - (1.0 / (12.0 * _lambda)) - (1.0 / (24.0 * _lambda * _lambda)) - (19.0 / (360.0 * _lambda * _lambda * _lambda)); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return 1.0 / Math.Sqrt(_lambda); }
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
            get { return int.MaxValue; }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the Poisson distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return 1.0 - SpecialFunctions.GammaLowerRegularized(x + 1, _lambda);
        }

        #endregion

        #region IDiscreteDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode
        {
            get { return (int)Math.Floor(_lambda); }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <remarks>Approximation, see Wikipedia <a href="http://en.wikipedia.org/wiki/Poisson_distribution">Poisson distribution</a></remarks>
        public int Median
        {
            get { return (int)Math.Floor(_lambda + (1.0 / 3.0) - (0.02 / _lambda)); }
        }

        /// <summary>
        /// Computes values of the probability mass function.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            return Math.Exp(-_lambda + (k * Math.Log(_lambda)) - SpecialFunctions.FactorialLn(k));
        }

        /// <summary>
        /// Computes values of the log probability mass function.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            return -_lambda + (k * Math.Log(_lambda)) - SpecialFunctions.FactorialLn(k);
        }

        #endregion

        /// <summary>
        /// Generates one sample from the Poisson distribution.
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="lambda">The Poisson distribution parameter λ.</param>
        /// <returns>A random sample from the Poisson distribution.</returns>
        internal static int SampleUnchecked(Random rnd, double lambda)
        {
            return (lambda < 30.0) ? DoSampleShort(rnd, lambda) : DoSampleLarge(rnd, lambda);
        }

        /// <summary>
        /// Generates one sample from the Poisson distribution by Knuth's method.
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="lambda">The Poisson distribution parameter λ.</param>
        /// <returns>A random sample from the Poisson distribution.</returns>
        static int DoSampleShort(Random rnd, double lambda)
        {
            var limit = Math.Exp(-lambda);
            var count = 0;
            for (var product = rnd.NextDouble(); product >= limit; product *= rnd.NextDouble())
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// Generates one sample from the Poisson distribution by "Rejection method PA".
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="lambda">The Poisson distribution parameter λ.</param>
        /// <returns>A random sample from the Poisson distribution.</returns>
        /// <remarks>"Rejection method PA" from "The Computer Generation of Poisson Random Variables" by A. C. Atkinson,
        /// Journal of the Royal Statistical Society Series C (Applied Statistics) Vol. 28, No. 1. (1979)
        /// The article is on pages 29-35. The algorithm given here is on page 32. </remarks>
        static int DoSampleLarge(Random rnd, double lambda)
        {
            var c = 0.767 - (3.36 / lambda);
            var beta = Math.PI / Math.Sqrt(3.0 * lambda);
            var alpha = beta * lambda;
            var k = Math.Log(c) - lambda - Math.Log(beta);

            for (;;)
            {
                var u = rnd.NextDouble();
                var x = (alpha - Math.Log((1.0 - u) / u)) / beta;
                var n = (int)Math.Floor(x + 0.5);
                if (n < 0)
                {
                    continue;
                }

                var v = rnd.NextDouble();
                var y = alpha - (beta * x);
                var temp = 1.0 + Math.Exp(y);
                var lhs = y + Math.Log(v / (temp * temp));
                var rhs = k + (n * Math.Log(lambda)) - SpecialFunctions.FactorialLn(n);
                if (lhs <= rhs)
                {
                    return n;
                }
            }
        }

        /// <summary>
        /// Samples a Poisson distributed random variable.
        /// </summary>
        /// <returns>A sample from the Poisson distribution.</returns>
        public int Sample()
        {
            return SampleUnchecked(RandomSource, _lambda);
        }

        /// <summary>
        /// Samples an array of Poisson distributed random variables.
        /// </summary>
        /// <returns>a sequence of successes in N trials.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _lambda);
            }
        }

        /// <summary>
        /// Samples a Poisson distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lambda">The Poisson distribution parameter λ.</param>
        /// <returns>A sample from the Poisson distribution.</returns>
        public static int Sample(Random rnd, double lambda)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lambda))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, lambda);
        }

        /// <summary>
        /// Samples a sequence of Poisson distributed random variables.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lambda">The Poisson distribution parameter λ.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<int> Samples(Random rnd, double lambda)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(lambda))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, lambda);
            }
        }
    }
}
