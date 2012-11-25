// <copyright file="FisherSnedecor.cs" company="Math.NET">
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
    /// Implements the FisherSnedecor distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/F-distribution">Wikipedia - FisherSnedecor distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class FisherSnedecor : IContinuousDistribution
    {
        /// <summary>
        /// The first parameter - degree of freedom.
        /// </summary>
        double _d1;

        /// <summary>
        /// The second parameter - degree of freedom.
        /// </summary>
        double _d2;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="FisherSnedecor"/> class. 
        /// </summary>
        /// <param name="d1">
        /// The first parameter - degree of freedom.
        /// </param>
        /// <param name="d2">
        /// The second parameter - degree of freedom.
        /// </param>
        public FisherSnedecor(double d1, double d2)
        {
            SetParameters(d1, d2);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="d1">The first parameter - degree of freedom.</param>
        /// <param name="d2">The second parameter - degree of freedom.</param>
        void SetParameters(double d1, double d2)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(d1, d2))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _d1 = d1;
            _d2 = d2;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid.
        /// </summary>
        /// <param name="d1">The first parameter - degree of freedom.</param>
        /// <param name="d2">The second parameter - degree of freedom.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double d1, double d2)
        {
            if (d1 <= 0 || d2 <= 0)
            {
                return false;
            }

            if (Double.IsNaN(d1) || Double.IsNaN(d2))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the first parameter - degree of freedom.
        /// </summary>
        public double DegreeOfFreedom1
        {
            get { return _d1; }

            set { SetParameters(value, _d2); }
        }

        /// <summary>
        /// Gets or sets the second parameter - degree of freedom.
        /// </summary>
        public double DegreeOfFreedom2
        {
            get { return _d2; }

            set { SetParameters(_d1, value); }
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "FisherSnedecor(DegreeOfFreedom1 = " + _d1 + ", DegreeOfFreedom2 = " + _d2 + ")";
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
                    throw new ArgumentNullException(Resources.InvalidDistributionParameters);
                }

                _random = value;
            }
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean
        {
            get
            {
                if (_d2 <= 2)
                {
                    throw new NotSupportedException();
                }

                return _d2 / (_d2 - 2.0);
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_d2 <= 4)
                {
                    throw new NotSupportedException();
                }

                return (2.0 * _d2 * _d2 * (_d1 + _d2 - 2.0)) / (_d1 * (_d2 - 2.0) * (_d2 - 2.0) * (_d2 - 4.0));
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
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (_d2 <= 6)
                {
                    throw new NotSupportedException();
                }

                return (((2.0 * _d1) + _d2 - 2.0) * Math.Sqrt(8.0 * (_d2 - 4.0))) / ((_d2 - 6.0) * Math.Sqrt(_d1 * (_d1 + _d2 - 2.0)));
            }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return SpecialFunctions.BetaRegularized(_d1 / 2.0, _d2 / 2.0, _d1 * x / ((_d1 * x) + _d2));
        }

        #endregion

        #region IContinuousDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get
            {
                if (_d1 <= 2)
                {
                    throw new NotSupportedException();
                }

                return (_d2 * (_d1 - 2.0)) / (_d1 * (_d2 + 2.0));
            }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the maximum of the distribution.
        /// </summary>
        public double Maximum
        {
            get { return double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            return Math.Sqrt(Math.Pow(_d1 * x, _d1) * Math.Pow(_d2, _d2) / Math.Pow((_d1 * x) + _d2, _d1 + _d2)) / (x * SpecialFunctions.Beta(_d1 / 2.0, _d2 / 2.0));
        }

        /// <summary>
        /// Computes the log density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return Math.Log(Density(x));
        }

        #endregion

        /// <summary>
        /// Generates one sample from the <c>FisherSnedecor</c> distribution without parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="d1">The first parameter - degree of freedom.</param>
        /// <param name="d2">The second parameter - degree of freedom.</param>
        /// <returns>a <c>FisherSnedecor</c> distributed random number.</returns>
        internal static double SampleUnchecked(Random rnd, double d1, double d2)
        {
            return (ChiSquare.Sample(rnd, d1) / d1) / (ChiSquare.Sample(rnd, d2) / d2);
        }

        /// <summary>
        /// Generates a sample from the <c>FisherSnedecor</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _d1, _d2);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>FisherSnedecor</c> distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _d1, _d2);
            }
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="d1">The first parameter - degree of freedom.</param>
        /// <param name="d2">The second parameter - degree of freedom.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rnd, double d1, double d2)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(d1, d2))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, d1, d2);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="d1">The first parameter - degree of freedom.</param>
        /// <param name="d2">The second parameter - degree of freedom.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rnd, double d1, double d2)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(d1, d2))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, d1, d2);
            }
        }
    }
}
