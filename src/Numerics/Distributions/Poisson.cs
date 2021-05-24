// <copyright file="Poisson.cs" company="Math.NET">
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

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Discrete Univariate Poisson distribution.
    /// </summary>
    /// <remarks>
    /// <para>Distribution is described at <a href="http://en.wikipedia.org/wiki/Poisson_distribution"> Wikipedia - Poisson distribution</a>.</para>
    /// <para>Knuth's method is used to generate Poisson distributed random variables.</para>
    /// <para>f(x) = exp(-λ)*λ^x/x!;</para>
    /// </remarks>
    public class Poisson : IDiscreteDistribution
    {
        System.Random _random;

        readonly double _lambda;

        /// <summary>
        /// Initializes a new instance of the <see cref="Poisson"/> class.
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">If <paramref name="lambda"/> is equal or less then 0.0.</exception>
        public Poisson(double lambda)
        {
            if (!IsValidParameterSet(lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _lambda = lambda;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Poisson"/> class.
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">If <paramref name="lambda"/> is equal or less then 0.0.</exception>
        public Poisson(double lambda, System.Random randomSource)
        {
            if (!IsValidParameterSet(lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _lambda = lambda;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Poisson(λ = {_lambda})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        public static bool IsValidParameterSet(double lambda)
        {
            return lambda > 0.0;
        }

        /// <summary>
        /// Gets the Poisson distribution parameter λ. Range: λ > 0.
        /// </summary>
        public double Lambda => _lambda;

        /// <summary>
        /// Gets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean => _lambda;

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => _lambda;

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(_lambda);

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        /// <remarks>Approximation, see Wikipedia <a href="http://en.wikipedia.org/wiki/Poisson_distribution">Poisson distribution</a></remarks>
        public double Entropy => (0.5*Math.Log(Constants.Pi2*Constants.E*_lambda)) - (1.0/(12.0*_lambda)) - (1.0/(24.0*_lambda*_lambda)) - (19.0/(360.0*_lambda*_lambda*_lambda));

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => 1.0/Math.Sqrt(_lambda);

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum => 0;

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum => int.MaxValue;

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode => (int)Math.Floor(_lambda);

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <remarks>Approximation, see Wikipedia <a href="http://en.wikipedia.org/wiki/Poisson_distribution">Poisson distribution</a></remarks>
        public double Median => Math.Floor(_lambda + (1.0/3.0) - (0.02/_lambda));

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            return Math.Exp(-_lambda + (k*Math.Log(_lambda)) - SpecialFunctions.FactorialLn(k));
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            return -_lambda + (k*Math.Log(_lambda)) - SpecialFunctions.FactorialLn(k);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return 1.0 - SpecialFunctions.GammaLowerRegularized(x + 1, _lambda);
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public static double PMF(double lambda, int k)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return Math.Exp(-lambda + (k*Math.Log(lambda)) - SpecialFunctions.FactorialLn(k));
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public static double PMFLn(double lambda, int k)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return -lambda + (k*Math.Log(lambda)) - SpecialFunctions.FactorialLn(k);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double lambda, double x)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return 1.0 - SpecialFunctions.GammaLowerRegularized(x + 1, lambda);
        }

        /// <summary>
        /// Generates one sample from the Poisson distribution.
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>A random sample from the Poisson distribution.</returns>
        static int SampleUnchecked(System.Random rnd, double lambda)
        {
            return (lambda < 30.0) ? DoSampleShort(rnd, lambda) : DoSampleLarge(rnd, lambda);
        }

        static void SamplesUnchecked(System.Random rnd, int[] values, double lambda)
        {
            if (lambda < 30.0)
            {
                var limit = Math.Exp(-lambda);
                for (int i = 0; i < values.Length; i++)
                {
                    var count = 0;
                    for (var product = rnd.NextDouble(); product >= limit; product *= rnd.NextDouble())
                    {
                        count++;
                    }

                    values[i] = count;
                }
            }
            else
            {
                var c = 0.767 - (3.36/lambda);
                var beta = Math.PI/Math.Sqrt(3.0*lambda);
                var alpha = beta*lambda;
                var k = Math.Log(c) - lambda - Math.Log(beta);
                for (int i = 0; i < values.Length; i++)
                {
                    for (;;)
                    {
                        var u = rnd.NextDouble();
                        var x = (alpha - Math.Log((1.0 - u)/u))/beta;
                        var n = (int)Math.Floor(x + 0.5);
                        if (n < 0)
                        {
                            continue;
                        }

                        var v = rnd.NextDouble();
                        var y = alpha - (beta*x);
                        var temp = 1.0 + Math.Exp(y);
                        var lhs = y + Math.Log(v/(temp*temp));
                        var rhs = k + (n*Math.Log(lambda)) - SpecialFunctions.FactorialLn(n);
                        if (lhs <= rhs)
                        {
                            values[i] = n;
                            break;
                        }
                    }
                }
            }
        }

        static IEnumerable<int> SamplesUnchecked(System.Random rnd, double lambda)
        {
            if (lambda < 30.0)
            {
                while (true)
                {
                    yield return DoSampleShort(rnd, lambda);
                }
            }
            else
            {
                while (true)
                {
                    yield return DoSampleLarge(rnd, lambda);
                }
            }
        }

        /// <summary>
        /// Generates one sample from the Poisson distribution by Knuth's method.
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>A random sample from the Poisson distribution.</returns>
        static int DoSampleShort(System.Random rnd, double lambda)
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
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>A random sample from the Poisson distribution.</returns>
        /// <remarks>"Rejection method PA" from "The Computer Generation of Poisson Random Variables" by A. C. Atkinson,
        /// Journal of the Royal Statistical Society Series C (Applied Statistics) Vol. 28, No. 1. (1979)
        /// The article is on pages 29-35. The algorithm given here is on page 32. </remarks>
        static int DoSampleLarge(System.Random rnd, double lambda)
        {
            var c = 0.767 - (3.36/lambda);
            var beta = Math.PI/Math.Sqrt(3.0*lambda);
            var alpha = beta*lambda;
            var k = Math.Log(c) - lambda - Math.Log(beta);

            for (;;)
            {
                var u = rnd.NextDouble();
                var x = (alpha - Math.Log((1.0 - u)/u))/beta;
                var n = (int)Math.Floor(x + 0.5);
                if (n < 0)
                {
                    continue;
                }

                var v = rnd.NextDouble();
                var y = alpha - (beta*x);
                var temp = 1.0 + Math.Exp(y);
                var lhs = y + Math.Log(v/(temp*temp));
                var rhs = k + (n*Math.Log(lambda)) - SpecialFunctions.FactorialLn(n);
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
            return SampleUnchecked(_random, _lambda);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(int[] values)
        {
            SamplesUnchecked(_random, values, _lambda);
        }

        /// <summary>
        /// Samples an array of Poisson distributed random variables.
        /// </summary>
        /// <returns>a sequence of successes in N trials.</returns>
        public IEnumerable<int> Samples()
        {
            return SamplesUnchecked(_random, _lambda);
        }

        /// <summary>
        /// Samples a Poisson distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>A sample from the Poisson distribution.</returns>
        public static int Sample(System.Random rnd, double lambda)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, lambda);
        }

        /// <summary>
        /// Samples a sequence of Poisson distributed random variables.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<int> Samples(System.Random rnd, double lambda)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, lambda);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(System.Random rnd, int[] values, double lambda)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, lambda);
        }

        /// <summary>
        /// Samples a Poisson distributed random variable.
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>A sample from the Poisson distribution.</returns>
        public static int Sample(double lambda)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, lambda);
        }

        /// <summary>
        /// Samples a sequence of Poisson distributed random variables.
        /// </summary>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<int> Samples(double lambda)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, lambda);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="lambda">The lambda (λ) parameter of the Poisson distribution. Range: λ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static void Samples(int[] values, double lambda)
        {
            if (!(lambda > 0.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, lambda);
        }
    }
}
