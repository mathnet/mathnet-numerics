// <copyright file="NegativeBinomial.cs" company="Math.NET">
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
    /// Discrete Univariate Negative Binomial distribution.
    /// The negative binomial is a distribution over the natural numbers with two parameters r, p. For the special
    /// case that r is an integer one can interpret the distribution as the number of failures before the r'th success
    /// when the probability of success is p.
    /// <a href="http://en.wikipedia.org/wiki/Negative_binomial_distribution">Wikipedia - NegativeBinomial distribution</a>.
    /// </summary>
    public class NegativeBinomial : IDiscreteDistribution
    {
        System.Random _random;

        readonly double _r;
        readonly double _p;

        /// <summary>
        /// Initializes a new instance of the <see cref="NegativeBinomial"/> class.
        /// </summary>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        public NegativeBinomial(double r, double p)
        {
            if (!IsValidParameterSet(r, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _p = p;
            _r = r;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NegativeBinomial"/> class.
        /// </summary>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public NegativeBinomial(double r, double p, System.Random randomSource)
        {
            if (!IsValidParameterSet(r, p))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _p = p;
            _r = r;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"NegativeBinomial(R = {_r}, P = {_p})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        public static bool IsValidParameterSet(double r, double p)
        {
            return r >= 0.0 && p >= 0.0 && p <= 1.0;
        }

        /// <summary>
        /// Gets the number of successes. Range: r ≥ 0.
        /// </summary>
        public double R => _r;

        /// <summary>
        /// Gets the probability of success. Range: 0 ≤ p ≤ 1.
        /// </summary>
        public double P => _p;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean => _r*(1.0 - _p)/_p;

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance => _r*(1.0 - _p)/(_p*_p);

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(_r*(1.0 - _p))/_p;

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy => throw new NotSupportedException();

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness => (2.0 - _p)/Math.Sqrt(_r*(1.0 - _p));

        /// <summary>
        /// Gets the mode of the distribution
        /// </summary>
        public int Mode => _r > 1.0 ? (int)Math.Floor((_r - 1.0)*(1.0 - _p)/_p) : 0;

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median => throw new NotSupportedException();

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
            return PMF(_r, _p, k);
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            return PMFLn(_r, _p, k);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return CDF(_r, _p, x);
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public static double PMF(double r, double p, int k)
        {
            return Math.Exp(PMFLn(r, p, k));
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public static double PMFLn(double r, double p, int k)
        {
            if (!(r >= 0.0 && p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SpecialFunctions.GammaLn(r + k)
                   - SpecialFunctions.GammaLn(r)
                   - SpecialFunctions.GammaLn(k + 1.0)
                   + (r*Math.Log(p))
                   + (k*Math.Log(1.0 - p));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double r, double p, double x)
        {
            if (!(r >= 0.0 && p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return 1 - SpecialFunctions.BetaRegularized(x + 1, r, 1 - p);
        }

        /// <summary>
        /// Samples a negative binomial distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        /// <returns>a sample from the distribution.</returns>
        static int SampleUnchecked(System.Random rnd, double r, double p)
        {
            var lambda = Gamma.SampleUnchecked(rnd, r, p);
            var c = Math.Exp(-lambda);
            var p1 = 1.0;
            var k = 0;
            do
            {
                k = k + 1;
                p1 = p1*rnd.NextDouble();
            }
            while (p1 >= c);
            return k - 1;
        }

        static void SamplesUnchecked(System.Random rnd, int[] values, double r, double p)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = SampleUnchecked(rnd, r, p);
            }
        }

        static IEnumerable<int> SamplesUnchecked(System.Random rnd, double r, double p)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, r, p);
            }
        }

        /// <summary>
        /// Samples a <c>NegativeBinomial</c> distributed random variable.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public int Sample()
        {
            return SampleUnchecked(_random, _r, _p);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        public void Samples(int[] values)
        {
            SamplesUnchecked(_random, values, _r, _p);
        }

        /// <summary>
        /// Samples an array of <c>NegativeBinomial</c> distributed random variables.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples()
        {
            return SamplesUnchecked(_random, _r, _p);
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        public static int Sample(System.Random rnd, double r, double p)
        {
            if (!(r >= 0.0 && p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(rnd, r, p);
        }

        /// <summary>
        /// Samples a sequence of this random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        public static IEnumerable<int> Samples(System.Random rnd, double r, double p)
        {
            if (!(r >= 0.0 && p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(rnd, r, p);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        public static void Samples(System.Random rnd, int[] values, double r, double p)
        {
            if (!(r >= 0.0 && p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(rnd, values, r, p);
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        public static int Sample(double r, double p)
        {
            if (!(r >= 0.0 && p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SampleUnchecked(SystemRandomSource.Default, r, p);
        }

        /// <summary>
        /// Samples a sequence of this random variable.
        /// </summary>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        public static IEnumerable<int> Samples(double r, double p)
        {
            if (!(r >= 0.0 && p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return SamplesUnchecked(SystemRandomSource.Default, r, p);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="r">The number of successes (r) required to stop the experiment. Range: r ≥ 0.</param>
        /// <param name="p">The probability (p) of a trial resulting in success. Range: 0 ≤ p ≤ 1.</param>
        public static void Samples(int[] values, double r, double p)
        {
            if (!(r >= 0.0 && p >= 0.0 && p <= 1.0))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            SamplesUnchecked(SystemRandomSource.Default, values, r, p);
        }
    }
}
