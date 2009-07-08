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
    /// Implements the univariate Normal (or Gaussian) distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Normal_distribution">Wikipedia - Normal distribution</a>.
    /// </summary>
    public class Normal : IContinuousDistribution
    {
        // Keeps track of the mean of the normal distribution.
        private double mMean;
        // Keeps track of the standard deviation of the normal distribution.
        private double mStdDev;

        /// <summary>
        /// Constructs a standard normal distribution. This is a normal distribution with mean 0.0
        /// and standard deviation 1.0. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        public Normal() : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Construct a normal distribution with a particular mean and standard deviation. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        public Normal(double mean, double stddev)
        {
            SetParameters(mean, stddev);
            RandomSource = new Random();
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and standard deviation. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The standard deviation of the normal distribution.</param>
        public static Normal WithMeanStdDev(double mean, double stddev)
        {
            return new Normal(mean, stddev);
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and variance. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stddev">The variance of the normal distribution.</param>
        public static Normal WithMeanVariance(double mean, double var)
        {
            return new Normal(mean, System.Math.Sqrt(var));
        }

        /// <summary>
        /// Constructs a normal distribution from a mean and precision. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
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
        
        /// <summary>
        /// The precision of the normal distribution.
        /// </summary>
        public double Precision
        {
            get { return 1.0 / (mStdDev * mStdDev); }
            set { SetParameters(mMean, 1.0/Math.Sqrt(value)); }
        }

        #region IDistribution implementation

        /// <summary>
        /// The random number generator which is used to draw random samples.
        /// </summary>
        public Random RandomSource { get; set; }

        /// <summary>
        /// The mean of the normal distribution.
        /// </summary>
        public double Mean
        {
            get { return mMean; }
            set { SetParameters(value, mStdDev); }
        }

        /// <summary>
        /// The variance of the normal distribution.
        /// </summary>
        public double Variance
        {
            get { return mStdDev * mStdDev; }
            set { SetParameters(mMean, value); }
        }

        /// <summary>
        /// The standard deviation of the normal distribution.
        /// </summary>
        public double StdDev
        {
            get { return mStdDev; }
            set { SetParameters(mMean, value); }
        }

        /// <summary>
        /// The entropy of the normal distribution.
        /// </summary>
        public double Entropy { get { return Math.Log(mStdDev) + Constants.LogSqrt2PiE; } }

        /// <summary>
        /// The skewness of the normal distribution.
        /// </summary>
        public double Skewness { get { return 0.0; } }
        #endregion

        #region IContinuousDistribution implementation

        /// <summary>
        /// The mode of the normal distribution.
        /// </summary>
        public double Mode { get { return mMean; } }

        /// <summary>
        /// The median of the normal distribution.
        /// </summary>
        public double Median { get { return mMean; } }

        /// <summary>
        /// The minimum of the normal distribution.
        /// </summary>
        public double Minimum { get { return System.Double.NegativeInfinity; } }

        /// <summary>
        /// The maximum of the normal distribution.
        /// </summary>
        public double Maximum { get { return System.Double.PositiveInfinity; } }

        /// <summary>
        /// Computes the density of the normal distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        public double Density(double x)
        {
            double d = (x - mMean) / mStdDev;
            return Math.Exp(-0.5*d*d) / (Constants.Sqrt2Pi*mStdDev);
        }

        /// <summary>
        /// Computes the log density of the normal distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        public double DensityLn(double x)
        {
            double d = (x - mMean) / mStdDev;
            return -0.5 * d * d - Math.Log(mStdDev) - Constants.LogSqrt2Pi;
        }

        public double CumulativeDistribution(double x) { throw new NotImplementedException(); }

        /// <summary>
        /// Generates a sample from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        public double Sample()
        {
            double r2;
            return mMean + mStdDev * SampleBoxMuller(rng, r2);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        public IEnumerable<double> Samples()
        {
            double r2;

            while (true)
            {
                double r1 = SampleBoxMuller(rng, r2);
                yield return mean + stddev * r1;
                yield return mean + stddev * r2;
            }
        }
        #endregion

        public double InverseCumulativeDistribution(double p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates a sample from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        /// <param name="mean">The mean of the normal distribution from which to generate samples.</param>
        /// <param name="stddev">The standard deviation of the normal distribution from which to generate samples.</param>
        public static double Sample(System.Random rng, double mean, double stddev)
        {
            double r2;
            return mean + stddev * SampleBoxMuller(rng, r2);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        /// <param name="mean">The mean of the normal distribution from which to generate samples.</param>
        /// <param name="stddev">The standard deviation of the normal distribution from which to generate samples.</param>
        public static IEnumerable<double> Samples(System.Random rng, double mean, double stddev)
        {
            double r2;

            while(true)
            {
                double r1 = SampleBoxMuller(rng, r2);
                yield return mean + stddev * r1;
                yield return mean + stddev * r2;
            }
        }

        /// <summary>
        /// Samples a pair of standard normal distributed random variables using the <i>Box-Muller</i> algorithm.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="r2">The second random number.</param>
        internal static double SampleBoxMuller(System.Random rnd, out double r2)
        {
            double v1 = 2.0 * rnd.NextDouble() - 1.0;
            double v2 = 2.0 * rnd.NextDouble() - 1.0;
            double r = v1 * v1 + v2 * v2;
            while (r >= 1.0 || r == 0.0)
            {
                v1 = 2.0 * rnd.NextDouble() - 1.0;
                v2 = 2.0 * rnd.NextDouble() - 1.0;
                r = v1 * v1 + v2 * v2;
            }
            double fac = System.Math.Sqrt(-2.0 * System.Math.Log(r) / r);
            r2 = v2 * fac;
            return v1 * fac;
        }
    }
}
