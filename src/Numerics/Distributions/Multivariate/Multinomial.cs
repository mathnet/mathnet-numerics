// <copyright file="Bernoulli.cs" company="Math.NET">
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
    /// to false, all parameter checks can be turned off.</para></remarks>
    public class Multinomial
    {
        /// <summary>
        /// Stores the normalized multinomial probabilities.
        /// </summary>
        private double[] _p;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the Multinomial class.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <exception cref="ArgumentException">If any of the probabilities are negative or do not sum to one.</exception>
        public Multinomial(double[] p)
        {
            SetParameters(p);
            RandomSource = new System.Random();
        }

        /* TODO
        /// <summary>
        /// Generate a multinomial distribution from histogram <paramref name="h"/>. The distribution will
        /// not be automatically updated when the histogram changes.
        /// </summary>
        public Multinomial(Histogram h)
        {
            // The probability distribution vector.
            _p = new double[h.BinCount];

            // Fill in the distribution vector.
            for (int i = 0; i < h.BinCount; i++)
            {
                _p[i] = h[i];
            }

            RandomNumberGenerator = new System.Random();
        }*/

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        public override string ToString()
        {
            return "Multinomial(Dimension = " + _p.Length + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>If any of the probabilities are negative returns false, or if the sum of parameters is 0.0; otherwise true</returns>
        private static bool IsValidParameterSet(double[] p)
        {
            double sum = 0.0;
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] < 0.0 || Double.IsNaN(p[i]))
                {
                    return false;
                }
                else
                {
                    sum += p[i];
                }
            }

            if (sum == 0.0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        private void SetParameters(double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _p = (double[])p.Clone();
        }

        /// <summary>
        /// Gets or sets the probability of generating a one.
        /// </summary>
        public double[] P
        {
            get
            {
                return (double[]) _p.Clone();
            }

            set
            {
                SetParameters(value);
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
        /// Samples one multinomial distributed random variable; also known as the Discrete distribution.
        /// </summary>
        /// <returns>One random integer between 0 and the size of the multinomial (exclusive).</returns>
        public int Sample()
        {
            return Sample(RandomSource, _p);
        }

        /// <summary>
        /// Samples a multinomially distributed random variable.
        /// </summary>
        /// <param name="n">The number of variables needed.</param>
        /// <returns><paramref name="n"/> random integers between 0 and the size of the multinomial (exclusive).</returns>
        public int[] Sample(int n)
        {
            return Sample(RandomSource, n, _p);
        }

        /// <summary>
        /// Samples one multinomial distributed random variable; also known as the Discrete distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>One random integer between 0 and the size of the multinomial (exclusive).</returns>
        public static int Sample(System.Random rnd, double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // The cumulative density of p.
            double[] cp = UnnormalizedCDF(p);

            double u = rnd.NextDouble()*cp[cp.Length - 1];
            int idx = 0;
            while (u > cp[idx])
            {
                idx++;
            }
            return idx;
        }

        /// <summary>
        /// Samples a multinomially distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="n">The number of variables needed.</param>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns><paramref name="n"/> random integers between 0 and the size of the multinomial (exclusive).</returns>
        public static int[] Sample(System.Random rnd, int n, double[] p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            // The cumulative density of p.
            double[] cp = UnnormalizedCDF(p);

            int[] arr = new int[n];
            for (int i = 0; i < n; i++)
            {
                double u = rnd.NextDouble()*cp[cp.Length - 1];
                int idx = 0;
                while (u > cp[idx])
                {
                    idx++;
                }
                arr[i] = idx;
            }

            return arr;
        }

        /// <summary>
        /// Computes the unnormalized cumulative distribution function. This method performs no
        /// parameter checking.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios: this array does not need to be normalized 
        /// as this is often impossible using floating point arithmetic.</param>
        /// <returns>An array representing the unnormalized cumulative distribution function.</returns>
        private static double[] UnnormalizedCDF(double[] p)
        {
            double[] cp = (double[]) p.Clone();

            for (int i = 1; i < p.Length; i++)
            {
                cp[i] += cp[i - 1];
            }

            return cp;
        }
    }
}