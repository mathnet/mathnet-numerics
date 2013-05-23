// <copyright file="Stable.cs" company="Math.NET">
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
    /// A random variable is said to be stable (or to have a stable distribution) if it has 
    /// the property that a linear combination of two independent copies of the variable has 
    /// the same distribution, up to location and scale parameters.
    /// For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Stable_distribution">Wikipedia - Stable distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default.`
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Stable : IContinuousDistribution
    {
        /// <summary>
        /// The stability parameter of the distribution.
        /// </summary>
        double _alpha;

        /// <summary>
        /// The skewness parameter of the distribution.
        /// </summary>
        double _beta;

        /// <summary>
        /// The scale parameter of the distribution.
        /// </summary>
        double _scale;

        /// <summary>
        /// The location parameter of the distribution.
        /// </summary>
        double _location;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stable"/> class. 
        /// </summary>
        /// <param name="alpha">
        /// The stability parameter of the distribution.
        /// </param>
        /// <param name="beta">
        /// The skewness parameter of the distribution.
        /// </param>
        /// <param name="scale">
        /// The scale parameter of the distribution.
        /// </param>
        /// <param name="location">
        /// The location parameter of the distribution.
        /// </param>
        public Stable(double alpha, double beta, double scale, double location)
        {
            SetParameters(alpha, beta, scale, location);
            RandomSource = new Random();
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="alpha">The stability parameter of the distribution.</param>
        /// <param name="beta">The skewness parameter of the distribution.</param>
        /// <param name="scale">The scale parameter of the distribution.</param>
        /// <param name="location">The location parameter of the distribution.</param>
        void SetParameters(double alpha, double beta, double scale, double location)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(alpha, beta, scale, location))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _alpha = alpha;
            _beta = beta;
            _scale = scale;
            _location = location;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="alpha">The stability parameter of the distribution.</param>
        /// <param name="beta">The skewness parameter of the distribution.</param>
        /// <param name="scale">The scale parameter of the distribution.</param>
        /// <param name="location">The location parameter of the distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        static bool IsValidParameterSet(double alpha, double beta, double scale, double location)
        {
            if (alpha <= 0 || alpha > 2)
            {
                return false;
            }

            if (beta < -1 || beta > 1)
            {
                return false;
            }

            if (scale <= 0)
            {
                return false;
            }

            if (Double.IsNaN(alpha) || Double.IsNaN(beta) || Double.IsNaN(scale) || Double.IsNaN(location))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the stability parameter of the distribution.
        /// </summary>
        public double Alpha
        {
            get { return _alpha; }

            set { SetParameters(value, _beta, _scale, _location); }
        }

        /// <summary>
        /// Gets or sets The skewness parameter of the distribution.
        /// </summary>
        public double Beta
        {
            get { return _beta; }

            set { SetParameters(_alpha, value, _scale, _location); }
        }

        /// <summary>
        /// Gets or sets the scale parameter of the distribution.
        /// </summary>
        public double Scale
        {
            get { return _scale; }

            set { SetParameters(_alpha, _beta, value, _location); }
        }

        /// <summary>
        /// Gets or sets the location parameter of the distribution.
        /// </summary>
        public double Location
        {
            get { return _location; }

            set { SetParameters(_alpha, _beta, _scale, value); }
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "Stable(" + "Stability = " + _alpha + ", Skewness = " + _beta + ", Scale = " + _scale + ", Location = " + _location + ")";
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
            get
            {
                if (_alpha <= 1)
                {
                    throw new NotSupportedException();
                }

                return _location;
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_alpha == 2)
                {
                    return 2.0 * _scale * _scale;
                }

                return Double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        public double StdDev
        {
            get
            {
                if (_alpha == 2)
                {
                    return Math.Sqrt(2.0) * _scale;
                }

                return Double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Gets he entropy of the distribution.
        /// </summary>
        /// <remarks>Always throws a not supported exception.</remarks>
        public double Entropy
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        /// <remarks>Throws a not supported exception of <c>Alpha</c> != 2.</remarks>
        public double Skewness
        {
            get
            {
                if (_alpha != 2)
                {
                    throw new NotSupportedException();
                }

                return 0.0;
            }
        }

        /// <summary>
        /// Computes the cumulative distribution function of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>the cumulative density at <paramref name="x"/>.</returns>
        /// <remarks>Throws a not supported exception if <c>Alpha != 2</c>, <c>(Alpha != 1 and Beta !=0)</c>, or <c>(Alpha != 0.5 and Beta != 1)</c></remarks>
        public double CumulativeDistribution(double x)
        {
            if (_alpha == 2)
            {
                return (new Normal(_location, StdDev)).CumulativeDistribution(x);
            }

            if (_alpha == 1 && _beta == 0)
            {
                return (new Cauchy(_location, _scale)).CumulativeDistribution(x);
            }

            if (_alpha == 0.5 && _beta == 1)
            {
                return LevyCumulativeDistribution(_scale, _location, x);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the cumulative distribution function of the Levy distribution.
        /// </summary>
        /// <param name="scale">The scale parameter.</param>
        /// <param name="location">The location parameter.</param>
        /// <param name="x">The location at which to compute the cumulative density.</param>
        /// <returns>
        /// the cumulative density at <paramref name="x"/>.
        /// </returns>
        static double LevyCumulativeDistribution(double scale, double location, double x)
        {
            // The parameters scale and location must be correct
            return SpecialFunctions.Erfc(Math.Sqrt(scale / (2 * (x - location))));
        }

        #endregion

        #region IContinuousDistribution Members

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        /// <remarks>Throws a not supported exception if <c>Beta != 0</c>.</remarks>
        public double Mode
        {
            get
            {
                if (_beta != 0)
                {
                    throw new NotSupportedException();
                }

                return _location;
            }
        }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        /// <remarks>Throws a not supported exception if <c>Beta != 0</c>.</remarks>
        public double Median
        {
            get
            {
                if (_beta != 0)
                {
                    throw new NotSupportedException();
                }

                return _location;
            }
        }

        /// <summary>
        /// Gets the minimum of the distribution.
        /// </summary>
        public double Minimum
        {
            get
            {
                if (Math.Abs(_beta) == 1)
                {
                    return 0.0;
                }

                return Double.NegativeInfinity;
            }
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
            if (_alpha == 2)
            {
                return (new Normal(_location, StdDev)).Density(x);
            }

            if (_alpha == 1 && _beta == 0)
            {
                return (new Cauchy(_location, _scale)).Density(x);
            }

            if (_alpha == 0.5 && _beta == 1)
            {
                return LevyDensity(_scale, _location, x);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the density of the Levy distribution.
        /// </summary>
        /// <param name="scale">The scale parameter of the distribution.</param>
        /// <param name="location">The location parameter of the distribution.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        static double LevyDensity(double scale, double location, double x)
        {
            // The parameters scale and location must be correct
            if (x < location)
            {
                throw new NotSupportedException();
            }

            return (Math.Sqrt(scale / Constants.Pi2) * Math.Exp(-scale / (2 * (x - location)))) / Math.Pow(x - location, 1.5);
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
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The stability parameter of the distribution.</param>
        /// <param name="beta">The skewness parameter of the distribution.</param>
        /// <param name="scale">The scale parameter of the distribution.</param>
        /// <param name="location">The location parameter of the distribution.</param>
        /// <returns>a random number from the distribution.</returns>
        internal static double SampleUnchecked(Random rnd, double alpha, double beta, double scale, double location)
        {
            var randTheta = ContinuousUniform.Sample(rnd, -Constants.PiOver2, Constants.PiOver2);
            var randW = Exponential.Sample(rnd, 1.0);

            if (!1.0.AlmostEqual(alpha))
            {
                var theta = (1.0 / alpha) * Math.Atan(beta * Math.Tan(Constants.PiOver2 * alpha));
                var angle = alpha * (randTheta + theta);
                var part1 = beta * Math.Tan(Constants.PiOver2 * alpha);

                var factor = Math.Pow(1.0 + (part1 * part1), 1.0 / (2.0 * alpha));
                var factor1 = Math.Sin(angle) / Math.Pow(Math.Cos(randTheta), (1.0 / alpha));
                var factor2 = Math.Pow(Math.Cos(randTheta - angle) / randW, (1 - alpha) / alpha);

                return location + scale * (factor * factor1 * factor2);
            }
            else
            {
                var part1 = Constants.PiOver2 + (beta * randTheta);
                var summand = part1 * Math.Tan(randTheta);
                var subtrahend = beta * Math.Log(Constants.PiOver2 * randW * Math.Cos(randTheta) / part1);

                return location + scale * ((2.0 / Math.PI) * (summand - subtrahend));
            }
        }

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(RandomSource, _alpha, _beta, _scale, _location);
        }

        /// <summary>
        /// Generates a sequence of samples from the Stable distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(RandomSource, _alpha, _beta, _scale, _location);
            }
        }


        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The stability parameter of the distribution.</param>
        /// <param name="beta">The skewness parameter of the distribution.</param>
        /// <param name="scale">The scale parameter of the distribution.</param>
        /// <param name="location">The location parameter of the distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(Random rnd, double alpha, double beta, double scale, double location)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(alpha, beta, scale, location))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            return SampleUnchecked(rnd, alpha, beta, scale, location);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The stability parameter of the distribution.</param>
        /// <param name="beta">The skewness parameter of the distribution.</param>
        /// <param name="scale">The scale parameter of the distribution.</param>
        /// <param name="location">The location parameter of the distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(Random rnd, double alpha, double beta, double scale, double location)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(location, scale, scale, location))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            while (true)
            {
                yield return SampleUnchecked(rnd, alpha, beta, scale, location);
            }
        }
    }
}
