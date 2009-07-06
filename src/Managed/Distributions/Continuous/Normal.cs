// <copyright file="Combinatorics.cs" company="Math.NET">
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

    /// <summary>
    /// Implements the univariate Normal (or Gaussian) distribution.
    /// </summary>
    public class Normal : IContinuousDistribution
    {
        // Keeps track of the mean of the normal distribution.
        private double mMean;
        // Keeps track of the standard deviation of the normal distribution.
        private double mStdDev;

        /// <summary>
        /// Constructs a standard normal distribution. This is a normal distribution with mean 0.0
        /// and standard deviation 1.0.
        /// </summary>
        public Normal() : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Construct a normal distribution with a particular mean and standard deviation.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        public Normal(double mean, double stddev)
        {
            SetParameters(mean, stddev);
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and standard deviation.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        public static Normal WithMeanStdDev(double mean, double stddev)
        {
            return new Normal(mean, stddev);
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and variance.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The variance of the normal distribution.</param>
        public static Normal WithMeanVariance(double mean, double var)
        {
            return new Normal(mean, System.Math.Sqrt(var));
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and precision.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The precision of the normal distribution.</param>
        public static Normal WithMeanAndPrecision(double mean, double prec)
        {
            return new Normal(mean, 1.0 / System.Math.Sqrt(prec));
        }


        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        public override string ToString()
        {
            return "Normal(Mean = " + mMean + ", StdDev = " + mStdDev + ")";
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        /// <returns>True when the parameters are valid, false otherwise.</returns>
        private static bool IsValidParameterSet(double mean, double stddev)
        {
            if (stddev < 0.0)
            {
                return false;
            }
            else if (System.Double.IsNaN(mean))
            {
                return false;
            }
            else if (System.Double.IsNaN(stddev))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        private void SetParameters(double mean, double stddev)
        {
            if (IsValidParameterSet(mean, stddev))
            {
                mMean = mean;
                mStdDev = stddev;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("Invalid parameterization for the normal distribution.");
            }
        }
        
        public double Precision
        {
            get { return 1.0 / (mStdDev * mStdDev); }
            set { throw new NotImplementedException(); }
        }

        #region IDistribution implementation
        public Random RandomNumberGenerator { get; set; }

        public double Mean
        {
            get { return mMean; }
            set { throw new NotImplementedException(); }
        }
        public double Variance
        {
            get { return mStdDev * mStdDev; }
            set { throw new NotImplementedException(); }
        }
        public double StdDev
        {
            get { return mStdDev; }
            set { throw new NotImplementedException(); }
        }
        public double Entropy { get { throw new NotImplementedException(); } }
        public double Skewness { get { throw new NotImplementedException(); } }
        #endregion

        #region IContinuousDistribution implementation
        public double Mode { get { throw new NotImplementedException(); } }
        public double Median { get { throw new NotImplementedException(); } }
        public double Minimum { get { throw new NotImplementedException(); } }
        public double Maximum { get { throw new NotImplementedException(); } }
        public double Density(double x) { throw new NotImplementedException(); }
        public double DensityLn(double x) { throw new NotImplementedException(); }
        public double CumulativeDistribution(double x) { throw new NotImplementedException(); }

        public double Sample() { throw new NotImplementedException(); }
        public IEnumerable<double> Samples() { throw new NotImplementedException(); }
        #endregion

        public double InverseCumulativeDistribution(double p)
        {
            throw new NotImplementedException();
        }

        public static double Sample(System.Random rng, double mean, double stddev) { throw new NotImplementedException(); }
        public static IEnumerable<double> Samples(System.Random rng, double mean, double stddev) { throw new NotImplementedException(); }
    }
}
