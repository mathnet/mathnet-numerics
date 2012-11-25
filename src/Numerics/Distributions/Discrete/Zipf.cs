// <copyright file="Zipf.cs" company="Math.NET">
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
    /// Zipf's law, an empirical law formulated using mathematical statistics, refers to the fact 
    /// that many types of data studied in the physical and social sciences can be approximated with 
    /// a Zipfian distribution, one of a family of related discrete power law probability distributions.
    /// For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Zipf%27s_law">Wikipedia - Zipf distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Zipf : IDiscreteDistribution
    {
        /// <summary>
        /// The s parameter of the distribution.
        /// </summary>
        double _s;

        /// <summary>
        /// The n parameter of the distribution.
        /// </summary>
        int _n;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Zipf"/> class. 
        /// </summary>
        /// <param name="s">
        /// The s parameter of the distribution.
        /// </param>
        /// <param name="n">
        /// The n parameter of the distribution.
        /// </param>
        public Zipf(double s, int n)
        {
            SetParameters(s, n);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="s">The s parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        void SetParameters(double s, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(s, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _s = s;
            _n = n;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="s">The s parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double s, int n)
        {
            if (n <= 0 || s <= 0 || Double.IsNaN(s))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the s parameter of the distribution.
        /// </summary>
        public double S
        {
            get { return _s; }

            set { SetParameters(value, _n); }
        }

        /// <summary>
        /// Gets or sets the n parameter of the distribution.
        /// </summary>
        public int N
        {
            get { return _n; }

            set { SetParameters(_s, value); }
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Zipf(S = " + _s + ", N = " + _n + ")";
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
            get { return SpecialFunctions.GeneralHarmonic(_n, _s - 1.0) / SpecialFunctions.GeneralHarmonic(_n, _s); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_s <= 3)
                {
                    throw new NotSupportedException();
                }

                var generalHarmonicsNS = SpecialFunctions.GeneralHarmonic(_n, _s);
                return (SpecialFunctions.GeneralHarmonic(_n, _s - 2) * SpecialFunctions.GeneralHarmonic(_n, _s)) - (Math.Pow(SpecialFunctions.GeneralHarmonic(_n, _s - 1), 2) / (generalHarmonicsNS * generalHarmonicsNS));
            }
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
            get
            {
                double sum = 0;
                for (var i = 0; i < _n; i++)
                {
                    sum += Math.Log(i + 1) / Math.Pow(i + 1, _s);
                }

                return ((_s / SpecialFunctions.GeneralHarmonic(_n, _s)) * sum) + Math.Log(SpecialFunctions.GeneralHarmonic(_n, _s));
            }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (_s <= 4)
                {
                    throw new NotSupportedException();
                }

                return ((SpecialFunctions.GeneralHarmonic(_n, _s - 3) * Math.Pow(SpecialFunctions.GeneralHarmonic(_n, _s), 2)) - (SpecialFunctions.GeneralHarmonic(_n, _s - 1) * ((3 * SpecialFunctions.GeneralHarmonic(_n, _s - 2) * SpecialFunctions.GeneralHarmonic(_n, _s)) - Math.Pow(SpecialFunctions.GeneralHarmonic(_n, _s - 1), 2)))) / Math.Pow((SpecialFunctions.GeneralHarmonic(_n, _s - 2) * SpecialFunctions.GeneralHarmonic(_n, _s)) - Math.Pow(SpecialFunctions.GeneralHarmonic(_n, _s - 1), 2), 1.5);
            }
        }

        /// <summary> 
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The integer location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            if (x <= 1)
            {
                return 0.0;
            }

            return SpecialFunctions.GeneralHarmonic((int)x, _s) / SpecialFunctions.GeneralHarmonic(_n, _s);
        }

        #endregion

        #region IDiscreteDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public int Mode
        {
            get { return 1; }
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
            get { return 1; }
        }

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by an integer.
        /// </summary>
        public int Maximum
        {
            get { return _n; }
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
            return (1.0 / Math.Pow(k, _s)) / SpecialFunctions.GeneralHarmonic(_n, _s);
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
        /// Generates a sample from the Zipf distribution without doing parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="s">The s parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        /// <returns>a random number from the Zipf distribution.</returns>
        internal static int SampleUnchecked(Random rnd, double s, int n)
        {
            var r = 0.0;
            while (r == 0.0)
            {
                r = rnd.NextDouble();
            }

            var p = 1.0 / SpecialFunctions.GeneralHarmonic(n, s);
            int i;
            var sum = 0.0;
            for (i = 1; i <= n; i++)
            {
                sum += p / Math.Pow(i, s);
                if (sum >= r)
                {
                    break;
                }
            }

            return i;
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public int Sample()
        {
            return SampleUnchecked(RandomSource, _s, _n);
        }

        /// <summary>
        /// Samples an array of zipf distributed random variables.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<int> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _s, _n);
            }
        }

        /// <summary>
        /// Samples a random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="s">The s parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        public static int Sample(Random rnd, double s, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(s, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, s, n);
        }

        /// <summary>
        /// Samples a sequence of this random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="s">The s parameter of the distribution.</param>
        /// <param name="n">The n parameter of the distribution.</param>
        public static IEnumerable<int> Samples(Random rnd, double s, int n)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(s, n))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, s, n);
            }
        }
    }
}
