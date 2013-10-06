// <copyright file="FisherSnedecor.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

using System;
using System.Collections.Generic;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Continuous Univariate F-distribution, also known as Fisher-Snedecor distribution.
    /// For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/F-distribution">Wikipedia - FisherSnedecor distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class FisherSnedecor : IContinuousDistribution
    {
        System.Random _random;

        double _freedom1;
        double _freedom2;

        /// <summary>
        /// Initializes a new instance of the <see cref="FisherSnedecor"/> class. 
        /// </summary>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        public FisherSnedecor(double d1, double d2)
        {
            _random = new System.Random(Random.RandomSeed.Guid());
            SetParameters(d1, d2);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FisherSnedecor"/> class. 
        /// </summary>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public FisherSnedecor(double d1, double d2, System.Random randomSource)
        {
            _random = randomSource ?? new System.Random(Random.RandomSeed.Guid());
            SetParameters(d1, d2);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "FisherSnedecor(d1 = " + _freedom1 + ", d2 = " + _freedom2 + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double d1, double d2)
        {
            if (d1 <= 0.0 || d2 <= 0.0 || Double.IsNaN(d1) || Double.IsNaN(d2))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _freedom1 = d1;
            _freedom2 = d2;
        }

        /// <summary>
        /// Gets or sets the first degree of freedom (d1) of the distribution. Range: d1 > 0.
        /// </summary>
        public double DegreesOfFreedom1
        {
            get { return _freedom1; }
            set { SetParameters(value, _freedom2); }
        }

        /// <summary>
        /// Gets or sets the second degree of freedom (d2) of the distribution. Range: d2 > 0.
        /// </summary>
        public double DegreesOfFreedom2
        {
            get { return _freedom2; }
            set { SetParameters(_freedom1, value); }
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get { return _random; }
            set { _random = value ?? new System.Random(Random.RandomSeed.Guid()); }
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        public double Mean
        {
            get
            {
                if (_freedom2 <= 2)
                {
                    throw new NotSupportedException();
                }

                return _freedom2/(_freedom2 - 2.0);
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_freedom2 <= 4)
                {
                    throw new NotSupportedException();
                }

                return (2.0*_freedom2*_freedom2*(_freedom1 + _freedom2 - 2.0))/(_freedom1*(_freedom2 - 2.0)*(_freedom2 - 2.0)*(_freedom2 - 4.0));
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
                if (_freedom2 <= 6)
                {
                    throw new NotSupportedException();
                }

                return (((2.0*_freedom1) + _freedom2 - 2.0)*Math.Sqrt(8.0*(_freedom2 - 4.0)))/((_freedom2 - 6.0)*Math.Sqrt(_freedom1*(_freedom1 + _freedom2 - 2.0)));
            }
        }

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        public double Mode
        {
            get
            {
                if (_freedom1 <= 2)
                {
                    throw new NotSupportedException();
                }

                return (_freedom2*(_freedom1 - 2.0))/(_freedom1*(_freedom2 + 2.0));
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
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return Math.Sqrt(Math.Pow(_freedom1*x, _freedom1)*Math.Pow(_freedom2, _freedom2)/Math.Pow((_freedom1*x) + _freedom2, _freedom1 + _freedom2))/(x*SpecialFunctions.Beta(_freedom1/2.0, _freedom2/2.0));
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return Math.Log(Density(x));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return SpecialFunctions.BetaRegularized(_freedom1/2.0, _freedom2/2.0, _freedom1*x/((_freedom1*x) + _freedom2));
        }

        /// <summary>
        /// Generates a sample from the <c>FisherSnedecor</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, _freedom1, _freedom2);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>FisherSnedecor</c> distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            while (true)
            {
                yield return SampleUnchecked(_random, _freedom1, _freedom2);
            }
        }

        /// <summary>
        /// Generates one sample from the <c>FisherSnedecor</c> distribution without parameter checking.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        /// <returns>a <c>FisherSnedecor</c> distributed random number.</returns>
        static double SampleUnchecked(System.Random rnd, double d1, double d2)
        {
            return (ChiSquared.Sample(rnd, d1) / d1) / (ChiSquared.Sample(rnd, d2) / d2);
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double d1, double d2, double x)
        {
            if (d1 <= 0.0 || d2 <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return Math.Sqrt(Math.Pow(d1*x, d1)*Math.Pow(d2, d2)/Math.Pow((d1*x) + d2, d1 + d2))/(x*SpecialFunctions.Beta(d1/2.0, d2/2.0));
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double d1, double d2, double x)
        {
            return Math.Log(PDF(d1, d2, x));
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double d1, double d2, double x)
        {
            if (d1 <= 0.0 || d2 <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return SpecialFunctions.BetaRegularized(d1/2.0, d2/2.0, d1*x/((d1*x) + d2));
        }

        /// <summary>
        /// Generates a sample from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double d1, double d2)
        {
            if (d1 <= 0.0 || d2 <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            return SampleUnchecked(rnd, d1, d2);
        }

        /// <summary>
        /// Generates a sequence of samples from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="d1">The first degree of freedom (d1) of the distribution. Range: d1 > 0.</param>
        /// <param name="d2">The second degree of freedom (d2) of the distribution. Range: d2 > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double d1, double d2)
        {
            if (d1 <= 0.0 || d2 <= 0.0) throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);

            while (true)
            {
                yield return SampleUnchecked(rnd, d1, d2);
            }
        }
    }
}
