// <copyright file="Hypergeometric.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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
    /// This class implements functionality for the Hypergeometric distribution. This distribution is
    /// a discrete probability distribution that describes the number of successes in a sequence 
    /// of n draws from a finite population without replacement, just as the binomial distribution 
    /// describes the number of successes for draws with replacement
    /// <a href="http://en.wikipedia.org/wiki/Hypergeometric_distribution">Wikipedia - Hypergeometric distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property</para>.
    /// <para>
    /// The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Hypergeometric : IDiscreteDistribution
    {
        /// <summary>
        /// The size of the population.
        /// </summary>
        int _populationSize;

        /// <summary>
        /// The m parameter of the distribution.
        /// </summary>
        int _m;

        /// <summary>
        /// The n parameter (number to draw) of the distribution.
        /// </summary>
        int _n;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the Hypergeometric class.
        /// </summary>
        /// <param name="populationSize">The population size.</param>
        /// <param name="m">The m parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        public Hypergeometric(int populationSize, int m, int n)
        {
            SetParameters(populationSize, m, n);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="total">The Total parameter of the distribution.</param>
        /// <param name="m">The m parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        void SetParameters(int total, int m, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(total, m, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _populationSize = total;
            _m = m;
            _n = n;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid.
        /// </summary>
        /// <param name="total">The Total parameter of the distribution.</param>
        /// <param name="m">The m parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(int total, int m, int n)
        {
            if (total < 0 || m < 0 || n < 0)
            {
                return false;
            }

            if (m > total || n > total)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the population size.
        /// </summary>
        public int PopulationSize
        {
            get { return _populationSize; }
            set { SetParameters(value, _m, _n); }
        }

        /// <summary>
        /// Gets or sets the n parameter of the distribution.
        /// </summary>
        public int N
        {
            get { return _n; }
            set { SetParameters(_populationSize, value, _n); }
        }

        /// <summary>
        /// Gets or sets the m parameter of the distribution.
        /// </summary>
        public int M
        {
            get { return _m; }
            set { SetParameters(_populationSize, _m, value); }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Hypergeometric(N = " + _populationSize + ", m = " + _m + ", n = " + _n + ")";
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
            get { return (double)_m * _n / _populationSize; }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _n * _m * (_populationSize - _n) * (_populationSize - _m) / (_populationSize * _populationSize * (_populationSize - 1.0)); }
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
            get { return (Math.Sqrt(_populationSize - 1.0) * (_populationSize - (2 * _n)) * (_populationSize - (2 * _m))) / (Math.Sqrt(_n * _m * (_populationSize - _m) * (_populationSize - _n)) * (_populationSize - 2.0)); }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            int alpha = Minimum;
            int beta = Maximum;
            if (x <= alpha)
            {
                return 0.0;
            }

            if (x > beta)
            {
                return 1.0;
            }

            var sum = 0.0;
            var k = (int)Math.Ceiling(x - alpha) - 1;
            for (var i = alpha; i <= alpha + k; i++)
            {
                sum += SpecialFunctions.Binomial(_m, i) * SpecialFunctions.Binomial(_populationSize - _m, _n - i);
            }

            return sum / SpecialFunctions.Binomial(_populationSize, _n);
        }

        #endregion

        #region IDiscreteDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode
        {
            get { return (_n + 1) * (_m + 1) / (_populationSize + 2); }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public int Median
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public int Minimum
        {
            get { return Math.Max(0, _n + _m - _populationSize); }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public int Maximum
        {
            get { return Math.Min(_m, _n); }
        }

        /// <summary>
        /// Computes values of the probability mass function.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>
        /// the probability mass at location <paramref name="k"/>.
        /// </returns>
        public double Probability(int k)
        {
            return SpecialFunctions.Binomial(_m, k) * SpecialFunctions.Binomial(_populationSize - _m, _n - k) / SpecialFunctions.Binomial(_populationSize, _n);
        }

        /// <summary>
        /// Computes values of the log probability mass function.
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
        /// Generates a sample from the Hypergeometric distribution without doing parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="size">The Total parameter of the distribution.</param>
        /// <param name="m">The m parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        /// <returns>a random number from the Hypergeometric distribution.</returns>
        internal static int SampleUnchecked(Random rnd, int size, int m, int n)
        {
            var x = 0;

            do
            {
                var p = (double)m / size;
                var r = rnd.NextDouble();
                if (r < p)
                {
                    x++;
                    m--;
                }

                size--;
                n--;
            }
            while (0 < n);

            return x;
        }

        /// <summary>
        /// Samples a Hypergeometric distributed random variable.
        /// </summary>
        /// <returns>The number of successes in n trials.</returns>
        public int Sample()
        {
            return SampleUnchecked(RandomSource, _populationSize, _m, _n);
        }

        /// <summary>
        /// Samples an array of Hypergeometric distributed random variables.
        /// </summary>
        /// <returns>a sequence of successes in n trials.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _populationSize, _m, _n);
            }
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="populationSize">The population size.</param>
        /// <param name="m">The m parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        public static int Sample(Random rnd, int populationSize, int m, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(populationSize, m, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, populationSize, m, n);
        }

        /// <summary>
        /// Samples a sequence of this random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="populationSize">The population size.</param>
        /// <param name="m">The m parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        public static IEnumerable<int> Samples(Random rnd, int populationSize, int m, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(populationSize, m, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, populationSize, m, n);
            }
        }
    }
}
