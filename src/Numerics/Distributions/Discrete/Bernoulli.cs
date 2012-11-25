// <copyright file="Bernoulli.cs" company="Math.NET">
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
    /// The Bernoulli distribution is a distribution over bits. The parameter
    /// p specifies the probability that a 1 is generated.
    /// <a href="http://en.wikipedia.org/wiki/Bernoulli_distribution">Wikipedia - Bernoulli distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Bernoulli : IDiscreteDistribution
    {
        /// <summary>
        /// The probability of generating a one.
        /// </summary>
        double _p;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the Bernoulli class.
        /// </summary>
        /// <param name="p">The probability of generating one.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the Bernoulli parameter is not in the range [0,1].</exception>
        public Bernoulli(double p)
        {
            SetParameters(p);
            RandomSource = new Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Bernoulli(P = " + _p + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="p">The probability of generating a one.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double p)
        {
            if (p >= 0.0 && p <= 1.0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="p">The probability of generating a one.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _p = p;
        }

        /// <summary>
        /// Gets or sets the probability of generating a one.
        /// </summary>
        public double P
        {
            get { return _p; }

            set { SetParameters(value); }
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
            get { return _p; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(_p * (1.0 - _p)); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _p * (1.0 - _p); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return -(_p * Math.Log(_p)) - ((1.0 - _p) * Math.Log(1.0 - _p)); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return (1.0 - (2.0 * _p)) / Math.Sqrt(_p * (1.0 - _p)); }
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
            get { return 1; }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the Bernoulli distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x < 0.0)
            {
                return 0.0;
            }

            if (x < 1.0)
            {
                return 1.0 - _p;
            }

            return 1.0;
        }

        #endregion

        #region IDiscreteDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode
        {
            get { return _p > 0.5 ? 1 : 0; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public int Median
        {
            get { throw new NotSupportedException("The median of the Bernoulli distribution is undefined."); }
        }

        /// <summary>
        /// Computes values of the probability mass function.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        public double Probability(int k)
        {
            if (k == 0)
            {
                return 1.0 - _p;
            }

            if (k == 1)
            {
                return _p;
            }

            return 0.0;
        }

        /// <summary>
        /// Computes values of the log probability mass function.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        public double ProbabilityLn(int k)
        {
            if (k == 0)
            {
                return Math.Log(1.0 - _p);
            }

            return k == 1 ? Math.Log(_p) : Double.NegativeInfinity;
        }

        #endregion

        /// <summary>
        /// Generates one sample from the Bernoulli distribution.
        /// </summary>
        /// <param name="rnd">The random source to use.</param>
        /// <param name="p">The probability of generating a one.</param>
        /// <returns>A random sample from the Bernoulli distribution.</returns>
        internal static int SampleUnchecked(Random rnd, double p)
        {
            if (rnd.NextDouble() < p)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Samples a Bernoulli distributed random variable.
        /// </summary>
        /// <returns>A sample from the Bernoulli distribution.</returns>
        public int Sample()
        {
            return SampleUnchecked(RandomSource, _p);
        }

        /// <summary>
        /// Samples an array of Bernoulli distributed random variables.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _p);
            }
        }

        /// <summary>
        /// Samples a Bernoulli distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The probability of generating a 1.</param>
        /// <returns>A sample from the Bernoulli distribution.</returns>
        public static int Sample(Random rnd, double p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, p);
        }

        /// <summary>
        /// Samples a sequence of Bernoulli distributed random variables.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="p">The probability of generating a 1.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<int> Samples(Random rnd, double p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, p);
            }
        }

    }
}
