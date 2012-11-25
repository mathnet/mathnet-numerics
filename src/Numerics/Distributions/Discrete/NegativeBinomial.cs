// <copyright file="NegativeBinomial.cs" company="Math.NET">
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
    /// The negative binomial is a distribution over the natural numbers with two parameters r,p. For the special
    /// case that r is an integer one can interpret the distribution as the number of tails before the r'th head
    /// when the probability of head is p.
    /// <a href="http://en.wikipedia.org/wiki/Negative_binomial_distribution">Wikipedia - NegativeBinomial distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class NegativeBinomial : IDiscreteDistribution
    {
        /// <summary>
        /// The r parameter of the distribution.
        /// </summary>
        double _r;

        /// <summary>
        /// The p parameter of the distribution.
        /// </summary>
        double _p;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Gets or sets the number of trials.
        /// </summary>
        public double R
        {
            get { return _r; }

            set { SetParameters(value, _p); }
        }

        /// <summary>
        /// Gets or sets the probability of success.
        /// </summary>
        public double P
        {
            get { return _p; }

            set { SetParameters(_r, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NegativeBinomial"/> class. 
        /// </summary>
        /// <param name="r">The number of trials.</param>
        /// <param name="p">The probability of a trial resulting in success.</param>
        public NegativeBinomial(double r, double p)
        {
            SetParameters(r, p);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="r">The number of trials.</param>
        /// <param name="p">The probability of a trial resulting in success.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double r, double p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(r, p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _p = p;
            _r = r;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="r">The number of trials.</param>
        /// <param name="p">The probability of a trial resulting in success.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>        
        static bool IsValidParameterSet(double r, double p)
        {
            if (r < 0.0 || Double.IsNaN(r))
            {
                return false;
            }

            if (p < 0.0 || p > 1.0 || Double.IsNaN(p))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "NegativeBinomial(R = " + _r + ", P = " + _p + ")";
        }

        #region IDistribution Members

        /// <summary>
        /// Gets or sets the distribution's random number generator.
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
            get { return _r * (1.0 - _p) / _p; }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _r * (1.0 - _p) / (_p * _p); }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(_r * (1.0 - _p)) / _p; }
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
            get { return (2.0 - _p) / Math.Sqrt(_r * (1.0 - _p)); }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the <c>NegativeBinomial</c> distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return 1 - SpecialFunctions.BetaRegularized(x + 1, _r, 1 - _p);
        }

        #endregion

        #region IDiscreteDistribution Members

        /// <summary>
        /// Gets the mode of the distribution
        /// </summary>
        public int Mode
        {
            get { return _r > 1.0 ? (int)Math.Floor((_r - 1.0) * (1.0 - _p) / _p) : 0; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public int Median
        {
            get { throw new NotSupportedException(); }
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
        /// Computes values of the probability mass function.
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>
        /// the probability mass at location <paramref name="k"/>.
        /// </returns>
        public double Probability(int k)
        {
            var ln = SpecialFunctions.GammaLn(_r + k)
                - SpecialFunctions.GammaLn(_r)
                - SpecialFunctions.GammaLn(k + 1.0)
                + (_r * Math.Log(_p))
                + (k * Math.Log(1.0 - _p));
            return Math.Exp(ln);
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
            var ln = SpecialFunctions.GammaLn(_r + k)
                - SpecialFunctions.GammaLn(_r)
                - SpecialFunctions.GammaLn(k + 1.0)
                + (_r * Math.Log(_p))
                + (k * Math.Log(1.0 - _p));
            return ln;
        }

        #endregion

        /// <summary>
        /// Samples a negative binomial distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="r">The r parameter.</param>
        /// <param name="p">The p parameter.</param>
        /// <returns>a sample from the distribution.</returns>
        internal static int SampleUnchecked(Random rnd, double r, double p)
        {
            var lambda = Gamma.SampleUnchecked(rnd, r, p);
            var c = Math.Exp(-lambda);
            var p1 = 1.0;
            var k = 0;
            do
            {
                k = k + 1;
                p1 = p1 * rnd.NextDouble();
            }
            while (p1 >= c);
            return k - 1;
        }

        /// <summary>
        /// Samples a <c>NegativeBinomial</c> distributed random variable.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public int Sample()
        {
            return SampleUnchecked(RandomSource, _r, _p);
        }

        /// <summary>
        /// Samples an array of <c>NegativeBinomial</c> distributed random variables.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _r, _p);
            }
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="r">The r parameter.</param>
        /// <param name="p">The p parameter.</param>
        public static int Sample(Random rnd, double r, double p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(r, p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, r, p);
        }

        /// <summary>
        /// Samples a sequence of this random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="r">The r parameter.</param>
        /// <param name="p">The p parameter.</param>
        public static IEnumerable<int> Samples(Random rnd, double r, double p)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(r, p))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, r, p);
            }
        }
    }
}
