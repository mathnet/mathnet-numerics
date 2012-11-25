// <copyright file="Multinomial.cs" company="Math.NET">
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
    using System.Linq;
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using Properties;
    using Statistics;

    /// <summary>
    /// Implements the multinomial distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Multinomial_distribution">Wikipedia - Multinomial distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution is parameterized by a vector of ratios: in other words, the parameter
    /// does not have to be normalized and sum to 1. The reason is that some vectors can't be exactly normalized
    /// to sum to 1 in floating point representation.</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Multinomial
    {
        /// <summary>
        /// Stores the normalized multinomial probabilities.
        /// </summary>
        private double[] _p;

        /// <summary>
        /// The number of trials.
        /// </summary>
        private int _n;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the Multinomial class.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <param name="n">The number of trials.</param>
        /// <exception cref="ArgumentOutOfRangeException">If any of the probabilities are negative or do not sum to one.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        public Multinomial(double[] p, int n)
        {
            SetParameters(p, n);
            RandomSource = new Random();
        }

        /// <summary>
        /// Initializes a new instance of the Multinomial class from histogram <paramref name="h"/>. The distribution will
        /// not be automatically updated when the histogram changes.
        /// </summary>
        /// <param name="h">Histogram instance</param>
        /// <param name="n">The number of trials.</param>
                /// <exception cref="ArgumentOutOfRangeException">If any of the probabilities are negative or do not sum to one.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
        public Multinomial(Histogram h, int n)
        {
            if (h == null)
            {
                throw new ArgumentNullException("h");
            }

            // The probability distribution vector.
            var p = new double[h.BucketCount];

            // Fill in the distribution vector.
            for (var i = 0; i < h.BucketCount; i++)
            {
                p[i] = h[i].Count;
            }

            SetParameters(p, n);
            RandomSource = new Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Multinomial(Dimension = " + _p.Length + ", Number of Trails = " + _n + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <param name="n">The number of trials.</param>
        /// <returns>If any of the probabilities are negative returns <c>false</c>, 
        /// if the sum of parameters is 0.0, or if the number of trials is negative; otherwise <c>true</c>.</returns>
        private static bool IsValidParameterSet(IEnumerable<double> p, int n)
        {
            var sum = 0.0;
            foreach (var t in p)
            {
                if (t < 0.0 || Double.IsNaN(t))
                {
                    return false;
                }

                sum += t;
            }

            if (sum == 0.0)
            {
                return false;
            }

            return n >= 0;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <param name="n">The number of trials.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        private void SetParameters(double[] p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _p = (double[])p.Clone();
            _n = n;
        }

        /// <summary>
        /// Gets or sets the proportion of ratios.
        /// </summary>
        public double[] P
        {
            get
            {
                return (double[])_p.Clone();
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
        public Vector<double> Mean
        {
            get
            {
                return _n * (DenseVector)P;
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public Vector<double> Variance
        {
            get
            {
                // Do not use _p, because operations below will modify _p array. Use P or _p.Clone(). 
                var res = (DenseVector)P;
                for (var i = 0; i < res.Count; i++)
                {
                    res[i] *= _n * (1 - res[i]);
                }

                return res;
            }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public Vector<double> Skewness
        {
            get
            {
                // Do not use _p, because operations below will modify _p array. Use P or _p.Clone(). 
                var res = (DenseVector)P;
                for (var i = 0; i < res.Count; i++)
                {
                    res[i] = (1.0 - (2.0 * res[i])) / Math.Sqrt(_n * (1.0 - res[i]) * res[i]);
                }

                return res;
            }
        }

        /// <summary>
        /// Computes values of the probability mass function.
        /// </summary>
        /// <param name="x">Non-negative integers x1, ..., xk</param>
        /// <returns>The probability mass at location <paramref name="x"/>.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="x"/> is null.</exception>
        /// <exception cref="ArgumentException">When length of <paramref name="x"/> is not equal to event probabilities count.</exception>
        public double Probability(int[] x)
        {
            if (null == x)
            {
                throw new ArgumentNullException("x");
            }

            if (x.Length != _p.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "x");
            }

            if (x.Sum() == _n)
            {
                var coef = SpecialFunctions.Multinomial(_n, x);
                var num = 1.0;
                for (var i = 0; i < x.Length; i++)
                {
                    num *= Math.Pow(_p[i], x[i]);
                }

                return coef * num;
            }

            return 0.0;
        }

        /// <summary>
        /// Computes values of the log probability mass function.
        /// </summary>
        /// <param name="x">Non-negative integers x1, ..., xk</param>
        /// <returns>The log probability mass at location <paramref name="x"/>.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="x"/> is null.</exception>
        /// <exception cref="ArgumentException">When length of <paramref name="x"/> is not equal to event probabilities count.</exception>
        public double ProbabilityLn(int[] x)
        {
            if (null == x)
            {
                throw new ArgumentNullException("x");
            }

            if (x.Length != _p.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "x");
            }

            if (x.Sum() == _n)
            {
                var coef = Math.Log(SpecialFunctions.Multinomial(_n, x));
                var num = x.Select((t, i) => t * Math.Log(_p[i])).Sum();
                return coef + num;
            }

            return 0.0;
        }

        /// <summary>
        /// Samples one multinomial distributed random variable.
        /// </summary>
        /// <returns>the counts for each of the different possible values.</returns>
        public int[] Sample()
        {
            return Sample(RandomSource, _p, _n);
        }

        /// <summary>
        /// Samples a sequence multinomially distributed random variables.
        /// </summary>
        /// <returns>a sequence of counts for each of the different possible values.</returns>
        public IEnumerable<int[]> Samples()
        {
            while (true)
            {
                yield return Sample(RandomSource, _p, _n);
            }
        }

        /// <summary>
        /// Samples one multinomial distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <param name="n">The number of trials.</param>
        /// <returns>the counts for each of the different possible values.</returns>
        public static int[] Sample(Random rnd, double[] p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // The cumulative density of p.
            var cp = Categorical.UnnormalizedCdf(p);

            // The variable that stores the counts.
            var ret = new int[p.Length];

            for (var i = 0; i < n; i++)
            {
                ret[Categorical.SampleUnchecked(rnd, cp)]++;
            }

            return ret;
        }

        /// <summary>
        /// Samples a multinomially distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <param name="n">The number of variables needed.</param>
        /// <returns>a sequence of counts for each of the different possible values.</returns>
        public static IEnumerable<int[]> Samples(Random rnd, double[] p, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // The cumulative density of p.
            var cp = Categorical.UnnormalizedCdf(p);

            while (true)
            {
                // The variable that stores the counts.
                var ret = new int[p.Length];

                for (var i = 0; i < n; i++)
                {
                    ret[Categorical.SampleUnchecked(rnd, cp)]++;
                }

                yield return ret;
            }
        }
    }
}
