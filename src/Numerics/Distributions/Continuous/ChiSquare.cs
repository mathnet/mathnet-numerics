// <copyright file="ChiSquare.cs" company="Math.NET">
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
    /// This class implements functionality for the ChiSquare distribution. This distribution is
    /// a sum of the squares of k independent standard normal random variables.
    /// <a href="http://en.wikipedia.org/wiki/Chi-square_distribution">Wikipedia - ChiSquare distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class ChiSquare : IContinuousDistribution
    {
        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChiSquare"/> class. 
        /// </summary>
        /// <param name="dof">
        /// The degrees of freedom for the ChiSquare distribution.
        /// </param>
        public ChiSquare(double dof)
        {
            SetParameters(dof);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="dof">The degrees of freedom for the <c>ChiSquare</c> distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        void SetParameters(double dof)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(dof))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            Mean = dof;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="dof">The degrees of freedom for the <c>ChiSquare</c> distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double dof)
        {
            return dof > 0 && !Double.IsNaN(dof);
        }

        /// <summary>
        /// Gets or sets the degrees of freedom of the <c>ChiSquare</c> distribution.
        /// </summary>
        public double DegreesOfFreedom
        {
            get { return Mean; }

            set { SetParameters(value); }
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "ChiSquare(DoF = " + Mean + ")";
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
        public double Mean { get; private set; }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get { return 2.0 * Mean; }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get { return Math.Sqrt(2.0 * Mean); }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get { return (Mean / 2.0) + Math.Log(2.0 * SpecialFunctions.Gamma(Mean / 2.0)) + ((1.0 - (Mean / 2.0)) * SpecialFunctions.DiGamma(Mean / 2.0)); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        public double Skewness
        {
            get { return Math.Sqrt(8.0 / Mean); }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return SpecialFunctions.GammaLowerIncomplete(Mean / 2.0, x / 2.0) / SpecialFunctions.Gamma(Mean / 2.0);
        }

        #endregion

        #region IContinuousDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get { return Mean - 2.0; }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        public double Median
        {
            get { return Mean - (2.0 / 3.0); }
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
            return (Math.Pow(x, (Mean / 2.0) - 1.0) * Math.Exp(-x / 2.0)) / (Math.Pow(2.0, Mean / 2.0) * SpecialFunctions.Gamma(Mean / 2.0));
        }

        /// <summary>
        /// Computes the log density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        public double DensityLn(double x)
        {
            return (-x / 2.0) + (((Mean / 2.0) - 1.0) * Math.Log(x)) - ((Mean / 2.0) * Math.Log(2)) - SpecialFunctions.GammaLn(Mean / 2.0);
        }

        #endregion

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="dof">The degrees of freedom.</param>
        /// <returns>a random number from the distribution.</returns>
        internal static double SampleUnchecked(Random rnd, double dof)
        {
            //Use the simple method if the dof is an integer anyway
            if (Math.Floor(dof) == dof && dof < Int32.MaxValue)
            {
                double sum = 0;
                var n = (int)dof;
                for (var i = 0; i < n; i++)
                {
                    sum += Math.Pow(Normal.Sample(rnd, 0.0, 1.0), 2);
                }
                return sum;
            }
            //Call the gamma function (see http://en.wikipedia.org/wiki/Gamma_distribution#Specializations
            //for a justification)
            return Gamma.SampleUnchecked(rnd, dof / 2.0, .5);
        }

        /// <summary>
        /// Generates a sample from the <c>ChiSquare</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, Mean);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>ChiSquare</c> distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, Mean);
            }
        }

        /// <summary>
        /// Generates a sample from the <c>ChiSquare</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="dof">The degrees of freedom.</param>
        /// <returns>a sample from the distribution. </returns>
        public static double Sample(Random rnd, double dof)
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
        /// <param name="dof">The degrees of freedom.</param>
        /// <returns>a sample from the distribution. </returns>
        public static IEnumerable<double> Samples(Random rnd, double dof)
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
