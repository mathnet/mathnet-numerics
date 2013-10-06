// <copyright file="Binomial.cs" company="Math.NET">
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

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Discrete Univariate Binomial distribution.
    /// For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Binomial_distribution">Wikipedia - Binomial distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution is parameterized by a probability (between 0.0 and 1.0).</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Binomial : IDiscreteDistribution
    {
        System.Random _random;

        double _p;
        int _trials;

        /// <summary>
        /// Initializes a new instance of the Binomial class.
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="p"/> is not in the interval [0.0,1.0].</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        public Binomial(double p, int n)
        {
            _random = new System.Random(Random.RandomSeed.Guid());
            SetParameters(p, n);
        }

        /// <summary>
        /// Initializes a new instance of the Binomial class.
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="p"/> is not in the interval [0.0,1.0].</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        public Binomial(double p, int n, System.Random randomSource)
        {
            _random = randomSource ?? new System.Random(Random.RandomSeed.Guid());
            SetParameters(p, n);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Binomial(p = " + _p + ", n = " + _trials + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns><c>false</c> <paramref name="p"/> is not in the interval [0.0,1.0] or <paramref name="n"/> is negative, <c>true</c> otherwise.</returns>
        static bool IsValidParameterSet(double p, int n)
        {
            return p >= 0.0 && p <= 1.0 && n >= 0;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="p"/> is not in the interval [0.0,1.0].</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        void SetParameters(double p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _p = p;
            _trials = n;
        }

        /// <summary>
        /// Gets or sets the success probability in each trial. Range: 0 ≤ p ≤ 1.
        /// </summary>
        public double P
        {
            get { return _p; }
            set { SetParameters(value, _trials); }
        }

        /// <summary>
        /// Gets or sets the number of trials. Range: n ≥ 0.
        /// </summary>
        public int N
        {
            get { return _trials; }
            set { SetParameters(_p, value); }
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
            get { return _p*_trials; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(_p*(1.0 - _p)*_trials); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _p*(1.0 - _p)*_trials; }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                if (_p == 0.0 || _p == 1.0) return 0.0;

                var e = 0.0;
                for (var i = 0; i <= _trials; i++)
                {
                    var p = Probability(i);
                    e -= p*Math.Log(p);
                }

                return e;
            }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return (1.0 - (2.0*_p))/Math.Sqrt(_trials*_p*(1.0 - _p)); }
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
            get { return _trials; }
        }

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode
        {
            get
            {
                if (_p == 1.0) return _trials;
                if (_p == 0.0) return 0;

                return (int) Math.Floor((_trials + 1)*_p);
            }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public int Median
        {
            get { return (int) Math.Floor(_p*_trials); }
        }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            if (k < 0) return 0.0;
            if (k > _trials) return 0.0;
            if (_p == 0.0 && k == 0) return 1.0;
            if (_p == 0.0) return 0.0;
            if (_p == 1.0 && k == _trials) return 1.0;
            if (_p == 1.0) return 0.0;

            return SpecialFunctions.Binomial(_trials, k)*Math.Pow(_p, k)*Math.Pow(1.0 - _p, _trials - k);
        }

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            if (k < 0) return Double.NegativeInfinity;
            if (k > _trials) return Double.NegativeInfinity;
            if (_p == 0.0 && k == 0) return 0.0;
            if (_p == 0.0) return Double.NegativeInfinity;
            if (_p == 1.0 && k == _trials) return 0.0;
            if (_p == 1.0) return Double.NegativeInfinity;

            return SpecialFunctions.BinomialLn(_trials, k) + (k*Math.Log(_p)) + ((_trials - k)*Math.Log(1.0 - _p));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < 0.0) return 0.0;
            if (x > _trials) return 1.0;

            var cdf = 0.0;
            for (var i = 0; i <= (int) Math.Floor(x); i++)
            {
                cdf += Combinatorics.Combinations(_trials, i)*Math.Pow(_p, i)*Math.Pow(1.0 - _p, _trials - i);
            }

            return cdf;
        }

        /// <summary>
        /// Generates a sample from the Binomial distribution without doing parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>The number of successful trials.</returns>
        static int SampleUnchecked(System.Random rnd, double p, int n)
        {
            var k = 0;
            for (var i = 0; i < n; i++)
            {
                k += rnd.NextDouble() < p ? 1 : 0;
            }

            return k;
        }

        /// <summary>
        /// Samples a Binomially distributed random variable.
        /// </summary>
        /// <returns>The number of successes in N trials.</returns>
        public int Sample()
        {
            return SampleUnchecked(_random, _p, _trials);
        }

        /// <summary>
        /// Samples an array of Binomially distributed random variables.
        /// </summary>
        /// <returns>a sequence of successes in N trials.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(_random, _p, _trials);
            }
        }

        /// <summary>
        /// Samples a binomially distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>The number of successes in <paramref name="n"/> trials.</returns>
        public static int Sample(System.Random rnd, double p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, p, n);
        }

        /// <summary>
        /// Samples a sequence of binomially distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability (p) in each trial. Range: 0 ≤ p ≤ 1.</param>
        /// <param name="n">The number of trials (n). Range: n ≥ 0.</param>
        /// <returns>a sequence of successes in <paramref name="n"/> trials.</returns>
        public static IEnumerable<int> Samples(System.Random rnd, double p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, p, n);
            }
        }
    }
}
