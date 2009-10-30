// <copyright file="Binomial.cs" company="Math.NET">
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
    /// Implements the binomial distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Binomial_distribution">Wikipedia - Binomial distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution is parameterized by a probability (between 0.0 and 1.0).</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to false, all parameter checks can be turned off.</para></remarks>
    public class Binomial : IDiscreteDistribution
    {
        /// <summary>
        /// Stores the normalized binomial probability.
        /// </summary>
        private double _p;

        /// <summary>
        /// The number of trials.
        /// </summary>
        private int _n;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the Binomial class.
        /// </summary>
        /// <param name="p">The success probability of a trial.</param>
        /// <param name="n">The number of trials.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="p"/> is not in the interval [0.0,1.0].</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        public Binomial(double p, int n)
        {
            SetParameters(p, n);
            RandomSource = new System.Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        public override string ToString()
        {
            return "Binomial(Success Probability = " + _p + ", Number of Trials = " + _n + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="p">The success probability of a trial.</param>
        /// <param name="n">The number of trials.</param>
        /// <returns>false <paramref name="p"/> is not in the interval [0.0,1.0] or <paramref name="n"/> is negative, true otherwise.</exception>
        private static bool IsValidParameterSet(double p, int n)
        {
            if(p < 0.0 || p > 1.0)
            {
                return false;
            }

            if(n < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="p">The success probability of a trial.</param>
        /// <param name="n">The number of trials.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="p"/> is not in the interval [0.0,1.0].</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        private void SetParameters(double p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _p = p;
            _n = n;
        }

        /// <summary>
        /// Gets or sets the success probability.
        /// </summary>
        public double P
        {
            get
            {
                return _p;
            }

            set
            {
                SetParameters(value, _n);
            }
        }

        /// <summary>
        /// Gets or sets the number of trials.
        /// </summary>
        public int N
        {
            get
            {
                return _n;
            }

            set
            {
                SetParameters(_p, value);
            }
        }

        #region IDistribution Members

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public Random RandomSource
        {
            get
            {
                return _random;
            }

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
            get { return _p * _n; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(_p * (1.0 - _p) * _n); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _p * (1.0 - _p) * _n; }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                double E = 0.0;
                for(int i = 0; i < _n; i++)
                {
                    double p = Probability(i);
                    E += p * Math.Log(p);
                }
                return E;
            }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return (1.0 - 2.0 * _p) / Math.Sqrt(_n * _p * (1.0 - _p)); }
        }

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Minimum { get { return 0; } }

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum { get { return _n; } }

        /// <summary>
        /// Computes the cumulative distribution function of the Binomial distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < 0.0)
            {
                return 0.0;
            }
            else if (x > _n)
            {
                return 1.0;
            }

            int k = (int) Math.Floor(x);
            return (_n - k) * Combinatorics.Combinations(_n,k) * SpecialFunctions.BetaRegularized(_n - k, 1 + k, 1-_p);
        }

        #endregion

        #region IDiscreteDistribution Members

        /// <summary>
        /// The mode of the distribution.
        /// </summary>
        public int Mode
        {
            get { return (int) Math.Floor((_n + 1) * _p); }
        }

        /// <summary>
        /// The median of the distribution.
        /// </summary>
        public int Median
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Computes the probability of a specific value.
        /// </summary>
        public double Probability(int val)
        {
            if (val < 0)
            {
                return 0.0;
            }

            if (val > _n)
            {
                return 0.0;
            }

            return SpecialFunctions.Binomial(_n, val) * Math.Pow(_p, val) * Math.Pow(1.0 - _p, _n - val);
        }

        /// <summary>
        /// Computes the probability of a specific value.
        /// </summary>
        public double ProbabilityLn(int val)
        {
            if (val < 0)
            {
                return 0.0;
            }

            if (val > _n)
            {
                return 0.0;
            }

            return SpecialFunctions.BinomialLn(_n, val) + val * Math.Log(_p) + (_n - val) * Math.Log(1.0 - _p);
        }

        /// <summary>
        /// Samples a Binomially distributed random variable.
        /// </summary>
        /// <returns>The number of successful trials.</returns>
        public int Sample()
        {
            return DoSample(RandomSource, _p, _n);
        }

        /// <summary>
        /// Samples an array of Bernoulli distributed random variables.
        /// </summary>
        /// <returns>a sequence of successful trial counts.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return DoSample(RandomSource, _p, _n);
            }
        }

        #endregion

        /// <summary>
        /// Samples a binomially distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability of a trial; must be in the interval [0.0, 1.0].</param>
        /// <param name="n">The number of trials; must be positive.</param>
        /// <returns>The number of successes in <see cref="N"/> trials.</returns>
        public static int Sample(System.Random rnd, double p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return DoSample(rnd, p, n);
        }

        /// <summary>
        /// Samples a sequence of binomially distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability of a trial; must be in the interval [0.0, 1.0].</param>
        /// <param name="n">The number of trials; must be positive.</param>
        /// <returns>a sequence of successful trial counts.</returns>
        public static IEnumerable<int> Samples(System.Random rnd, double p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return DoSample(rnd, p, n);
            }
        }

        /// <summary>
        /// Generates a sample from the Binomial distribution without doing parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The success probability of a trial; must be in the interval [0.0, 1.0].</param>
        /// <param name="n">The number of trials; must be positive.</param>
        /// <returns>The number of successful trials.</returns>
        private static int DoSample(System.Random rnd, double p, int n)
        {
            int k = 0;
            for (int i = 0; i < n; i++)
            {
                k += (rnd.NextDouble() < p ? 1 : 0);
            }

            return k;
        }
    }
}