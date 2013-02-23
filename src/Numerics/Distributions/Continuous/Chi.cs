// <copyright file="Chi.cs" company="Math.NET">
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
    /// This class implements functionality for the Chi distribution. This distribution is
    /// a continuous probability distribution. The distribution usually arises when a k-dimensional vector's orthogonal 
    /// components are independent and each follow a standard normal distribution. The length of the vector will 
    /// then have a chi distribution.
    /// <a href="http://en.wikipedia.org/wiki/Chi_distribution">Wikipedia - Chi distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Chi : IContinuousDistribution
    {
        /// <summary>
        /// Keeps track of the degrees of freedom for the Chi distribution.
        /// </summary>
        double _dof;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Chi"/> class. 
        /// </summary>
        /// <param name="dof">
        /// The degrees of freedom for the Chi distribution.
        /// </param>
        public Chi(double dof)
        {
            SetParameters(dof);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="dof">The degrees of freedom for the Chi distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double dof)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(dof))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _dof = dof;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid.
        /// </summary>
        /// <param name="dof">The degrees of freedom for the Chi distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double dof)
        {
            if (dof <= 0 || Double.IsNaN(dof))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the degrees of freedom of the Chi distribution.
        /// </summary>
        public double DegreesOfFreedom
        {
            get { return _dof; }

            set { SetParameters(value); }
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Chi(DoF = " + _dof + ")";
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
            get { return Math.Sqrt(2) * (SpecialFunctions.Gamma((_dof + 1.0) / 2.0) / SpecialFunctions.Gamma(_dof / 2.0)); }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return _dof - (Mean * Mean); }
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
            get { return SpecialFunctions.GammaLn(_dof / 2.0) + ((_dof - Math.Log(2) - ((_dof - 1.0) * SpecialFunctions.DiGamma(_dof / 2.0))) / 2.0); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                var sigma = StdDev;
                return (Mean * (1.0 - (2.0 * (sigma * sigma)))) / (sigma * sigma * sigma);
            }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return SpecialFunctions.GammaLowerIncomplete(_dof / 2.0, x * x / 2.0) / SpecialFunctions.Gamma(_dof / 2.0);
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
                if (_dof < 1)
                {
                    throw new NotSupportedException();
                }

                return Math.Sqrt(_dof - 1.0);
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
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(double x)
        {
            return (Math.Pow(2.0, 1.0 - (_dof / 2.0)) * Math.Pow(x, _dof - 1.0) * Math.Exp(-x * x / 2.0)) / SpecialFunctions.Gamma(_dof / 2.0);
        }

        /// <summary>
        /// Computes the log density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return ((1.0 - (_dof / 2.0)) * Math.Log(2.0)) + ((_dof - 1.0) * Math.Log(x)) - (x * x / 2.0) - SpecialFunctions.GammaLn(_dof / 2.0);
        }

        #endregion

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="dof">Degrees of Freedom</param>
        /// <returns>a random number from the distribution.</returns>
        internal static double SampleUnchecked(Random rnd, int dof)
        {
            double sum = 0;
            for (var i = 0; i < dof; i++)
            {
                sum += Math.Pow(Normal.Sample(rnd, 0.0, 1.0), 2);
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Generates a sample from the Chi distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, (int)_dof);
        }

        /// <summary>
        /// Generates a sequence of samples from the Chi distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            var dof = (int)_dof;
            while (true)
            {
                yield return SampleUnchecked(RandomSource, dof);
            }
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="dof">Degrees of Freedom</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rnd, int dof)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(dof))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, dof);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="dof">Degrees of Freedom</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rnd, int dof)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(dof))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, dof);
            }
        }
    }
}
