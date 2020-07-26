// <copyright file="BetaBinomial.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
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

// <contribution>
//    Andrew J. Willshire
// </contribution>


using System;
using System.Collections.Generic;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Discrete Univariate Beta-Binomial distribution.
    /// The beta-binomial distribution is a family of discrete probability distributions on a finite support of non-negative integers arising
    /// when the probability of success in each of a fixed or known number of Bernoulli trials is either unknown or random.
    /// The beta-binomial distribution is the binomial distribution in which the probability of success at each of n trials is not fixed but randomly drawn from a beta distribution.
    /// It is frequently used in Bayesian statistics, empirical Bayes methods and classical statistics to capture overdispersion in binomial type distributed data.
    /// <a href="https://en.wikipedia.org/wiki/Beta-binomial_distribution">Wikipedia - Beta-Binomial distribution</a>.
    /// </summary>
    public class BetaBinomial : IDiscreteDistribution
    {
        System.Random _random;

        readonly int _n;
        readonly double _a;
        readonly double _b;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaBinomial"/> class.
        /// </summary>
        /// <param name="n">The number of Bernoulli trials n - n is a positive integer </param>
        /// <param name="a">Shape parameter alpha of the Beta distribution. Range: a > 0.</param>
        /// <param name="b">Shape parameter beta of the Beta distribution. Range: b > 0.</param>
        public BetaBinomial(int n, double a, double b)
        {
            if (!IsValidParameterSet(n, a, b))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _n = n;
            _a = a;
            _b = b;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaBinomial"/> class.
        /// </summary>
        /// <param name="n">The number of Bernoulli trials n - n is a positive integer </param>
        /// <param name="a">Shape parameter alpha of the Beta distribution. Range: a > 0.</param>
        /// <param name="b">Shape parameter beta of the Beta distribution. Range: b > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public BetaBinomial(int n, double a, double b, System.Random randomSource)
        {
            if (!IsValidParameterSet(n,a,b))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _n = n;
            _a = a;
            _b = b;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"BetaBinomial(n = {_n}, a = {_a}, b = {_b})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="n">The number of Bernoulli trials n - n is a positive integer </param>
        /// <param name="a">Shape parameter alpha of the Beta distribution. Range: a > 0.</param>
        /// <param name="b">Shape parameter beta of the Beta distribution. Range: b > 0.</param>
        public static bool IsValidParameterSet(int n, double a, double b)
        {
            return n >= 1.0 && a > 0.0 && b > 0.0;
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="n">The number of Bernoulli trials n - n is a positive integer </param>
        /// <param name="a">Shape parameter alpha of the Beta distribution. Range: a > 0.</param>
        /// <param name="b">Shape parameter beta of the Beta distribution. Range: b > 0.</param>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        public static bool IsValidParameterSet(int n, double a, double b, int k)
        {
            return n >= 1.0 && a > 0.0 && b > 0.0 && k >=0 && k <=n;
        }


        public int N => _n;
        public double A => _a;
        public double B => _b;

        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }
        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean => (_n * _a) / (_a + _b);

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => (_n*_a*_b*(_a+_b+_n))/(Math.Pow((_a+_b),2) * (_a+_b+1));

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt((_n * _a * _b * (_a + _b + _n)) / (Math.Pow((_a + _b), 2) * (_a + _b + 1)));

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        double IUnivariateDistribution.Entropy => throw new NotSupportedException();

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness =>
            (_a + _b + 2 * _n) * (_b - _a) / (_a + _b + 2) * Math.Sqrt((1 + _a + _b) / (_n * _a * _b * (_n + _a + _b)));

        /// <summary>
        /// Gets the mode of the distribution
        /// </summary>
        int IDiscreteDistribution.Mode => throw new NotSupportedException();

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        double IUnivariateDistribution.Median => throw new NotSupportedException();

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum => 0;

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum => int.MaxValue;

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            return PMF(_n, _a, _b, k);
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            return PMFLn(_n, _a, _b, k);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/></returns>
        public double CumulativeDistribution(double x)
        {
            return CDF(_n, _a, _b, (int)Math.Floor(x));
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="n">The number of Bernoulli trials n - n is a positive integer </param>
        /// <param name="a">Shape parameter alpha of the Beta distribution. Range: a > 0.</param>
        /// <param name="b">Shape parameter beta of the Beta distribution. Range: b > 0.</param>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public static double PMF(int n, double a, double b, int k)
        {
            if (!IsValidParameterSet(n, a, b, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            if (k > n)
            {
                return 0.0;
            }
            else
            {
                return Math.Exp(PMFLn(n, a, b, k));
            }
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="n">The number of Bernoulli trials n - n is a positive integer </param>
        /// <param name="a">Shape parameter alpha of the Beta distribution. Range: a > 0.</param>
        /// <param name="b">Shape parameter beta of the Beta distribution. Range: b > 0.</param>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public static double PMFLn(int n, double a, double b, int k)
        {
            if (!IsValidParameterSet(n, a, b, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SpecialFunctions.BinomialLn((n), k)
                + SpecialFunctions.BetaLn(k + a, n - k + b)
                - SpecialFunctions.BetaLn(a, b);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="n">The number of Bernoulli trials n - n is a positive integer </param>
        /// <param name="a">Shape parameter alpha of the Beta distribution. Range: a > 0.</param>
        /// <param name="b">Shape parameter beta of the Beta distribution. Range: b > 0.</param>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(int n, double a, double b, int x)
        {
            if (!IsValidParameterSet(n,a,b,x))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            double accumulator = 0;

            for (int i = 0; i<=x; i++)
            {
                accumulator += PMF(n, a, b, i);
            }

            return accumulator;
        }

        /// <summary>
        /// Samples BetaBinomial distributed random variables by sampling a Beta distribution then passing to a Binomial distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>a random number from the BetaBinomial distribution.</returns>
        static int SampleUnchecked(System.Random rnd, int n, double a, double b)
        {
            var p = Beta.SampleUnchecked(rnd, a, b);
            var x = Binomial.SampleUnchecked(rnd, p, n);
            return x;
        }

        static void SamplesUnchecked(System.Random rnd, int[] values, int n, double a, double b)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = SampleUnchecked(rnd, n, a, b);
            }
        }

        static IEnumerable<int> SamplesUnchecked(System.Random rnd, int n, double a, double b)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, n, a, b);
            }
        }


        /// <summary>
        /// Samples a <c>BetaBinomial</c> distributed random variable.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>

        public int Sample()
        {
            return SampleUnchecked(_random, _n, _a, _b);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(int[] values)
        {
            SamplesUnchecked(_random, values, _n, _a, _b);
        }

        /// <summary>
        /// Samples an array of <c>BetaBinomial</c> distributed random variables.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples()
        {
            return SamplesUnchecked(_random, _n, _a, _b);
        }

        /// <summary>
        /// Samples a <c>BetaBinomial</c> distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>a sample from the distribution.</returns>

        public int Sample(System.Random rnd, int n, double a, double b)
        {
            if (!IsValidParameterSet(n,a,b))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return SampleUnchecked(rnd, n, a, b);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        public void Samples(System.Random rnd, int[] values, int n, double a, double b)
        {
            if (!IsValidParameterSet(n, a, b))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, n, a, b);
        }

        /// <summary>
        /// Samples an array of <c>BetaBinomial</c> distributed random variables.
        /// </summary>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples(int n, double a, double b)
        {
            if (!IsValidParameterSet(n, a, b))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return SamplesUnchecked(_random, n, a, b);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="a">The α shape parameter of the Beta distribution. Range: α ≥ 0.</param>
        /// <param name="b">The β shape parameter of the Beta distribution. Range: β ≥ 0.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        public void Samples(int[] values, int n, double a, double b)
        {
            if (!IsValidParameterSet(n, a, b))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(_random, values, n, a, b);
        }
    }
}
