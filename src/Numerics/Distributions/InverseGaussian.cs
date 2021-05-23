// <copyright file="InverseGaussian.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2019 Math.NET
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

using MathNet.Numerics.Random;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Distributions
{
    public class InverseGaussian : IContinuousDistribution
    {
        System.Random _random;

        /// <summary>
        /// Gets the mean (μ) of the distribution. Range: μ > 0.
        /// </summary>
        public double Mu { get; }

        /// <summary>
        /// Gets the shape (λ) of the distribution. Range: λ > 0.
        /// </summary>
        public double Lambda { get; }

        /// <summary>
        /// Initializes a new instance of the InverseGaussian class.
        /// </summary>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public InverseGaussian(double mu, double lambda, System.Random randomSource = null)
        {
            if (!IsValidParameterSet(mu, lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            _random = randomSource ?? SystemRandomSource.Default;
            Mu = mu;
            Lambda = lambda;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"InverseGaussian(μ = {Mu}, λ = {Lambda})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        public static bool IsValidParameterSet(double mu, double lambda)
        {
            var allFinite = mu.IsFinite() && lambda.IsFinite();
            return allFinite && mu > 0.0 && lambda > 0.0;
        }

        /// <summary>
        /// Gets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the Inverse Gaussian distribution.
        /// </summary>
        public double Mean => Mu;

        /// <summary>
        /// Gets the variance of the Inverse Gaussian distribution.
        /// </summary>
        public double Variance => Math.Pow(Mu, 3) / Lambda;

        /// <summary>
        /// Gets the standard deviation of the Inverse Gaussian distribution.
        /// </summary>
        public double StdDev => Math.Sqrt(Variance);

        /// <summary>
        /// Gets the median of the Inverse Gaussian distribution.
        /// No closed form analytical expression exists, so this value is approximated numerically and can throw an exception.
        /// </summary>
        public double Median => InvCDF(0.5);

        /// <summary>
        /// Gets the minimum of the Inverse Gaussian distribution.
        /// </summary>
        public double Minimum => 0.0;

        /// <summary>
        /// Gets the maximum of the Inverse Gaussian distribution.
        /// </summary>
        public double Maximum => double.PositiveInfinity;

        /// <summary>
        /// Gets the skewness of the Inverse Gaussian distribution.
        /// </summary>
        public double Skewness => 3 * Math.Sqrt(Mu / Lambda);

        /// <summary>
        /// Gets the kurtosis of the Inverse Gaussian distribution.
        /// </summary>
        public double Kurtosis => 15 * Mu / Lambda;

        /// <summary>
        /// Gets the mode of the Inverse Gaussian distribution.
        /// </summary>
        public double Mode => Mu * (Math.Sqrt(1 + (9 * Mu * Mu) / (4 * Lambda * Lambda)) - (3 * Mu) / (2 * Lambda));

        /// <summary>
        /// Gets the entropy of the Inverse Gaussian distribution (currently not supported).
        /// </summary>
        public double Entropy => throw new NotSupportedException();

        /// <summary>
        /// Generates a sample from the inverse Gaussian distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(_random, Mu, Lambda);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="values">The array to fill with the samples.</param>
        public void Samples(double[] values)
        {
            SamplesUnchecked(_random, values, Mu, Lambda);
        }

        /// <summary>
        /// Generates a sequence of samples from the inverse Gaussian distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<double> Samples()
        {
            return SamplesUnchecked(_random, Mu, Lambda);
        }

        /// <summary>
        /// Generates a sample from the inverse Gaussian distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double Sample(System.Random rnd, double mu, double lambda)
        {
            if (!IsValidParameterSet(mu, lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return SampleUnchecked(rnd, mu, lambda);
        }

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="values">The array to fill with the samples.</param>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        public static void Samples(System.Random rnd, double[] values, double mu, double lambda)
        {
            if (!IsValidParameterSet(mu, lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            SamplesUnchecked(rnd, values, mu, lambda);
        }

        /// <summary>
        /// Generates a sequence of samples from the Burr distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<double> Samples(System.Random rnd, double mu, double lambda)
        {
            if (!IsValidParameterSet(mu, lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return SamplesUnchecked(rnd, mu, lambda);
        }

        static double SampleUnchecked(System.Random rnd, double mu, double lambda)
        {
            double v = Normal.Sample(rnd, 0, 1);
            double test = rnd.NextDouble();
            return InverseGaussianSampleImpl(mu, lambda, v, test);
        }

        static void SamplesUnchecked(System.Random rnd, double[] values, double mu, double lambda)
        {
            if (values.Length == 0)
            {
                return;
            }
            double[] v = new double[values.Length];
            Normal.Samples(rnd, v, 0, 1);
            double[] test = rnd.NextDoubles(values.Length);
            for (var j = 0; j < values.Length; ++j)
            {
                values[j] = InverseGaussianSampleImpl(mu, lambda, v[j], test[j]);
            }
        }

        static IEnumerable<double> SamplesUnchecked(System.Random rnd, double mu, double lambda)
        {
            while (true)
            {
                yield return SampleUnchecked(rnd, mu, lambda);
            }
        }

        static double InverseGaussianSampleImpl(double mu, double lambda, double normalSample, double uniformSample)
        {
            double y = normalSample * normalSample;
            double x = mu + (mu * mu * y) / (2 * lambda) - (mu / (2 * lambda)) * Math.Sqrt(4 * mu * lambda * y + mu * mu * y * y);
            if (uniformSample <= mu / (mu + x))
                return x;
            else
                return mu * mu / x;
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDF"/>
        public double Density(double x)
        {
            return DensityImpl(Mu, Lambda, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="PDFLn"/>
        public double DensityLn(double x)
        {
            return DensityLnImpl(Mu, Lambda, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CDF"/>
        public double CumulativeDistribution(double x)
        {
            return CumulativeDistributionImpl(Mu, Lambda, x);
        }

        /// <summary>
        /// Computes the inverse cumulative distribution (CDF) of the distribution at p, i.e. solving for P(X ≤ x) = p.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative distribution function.</param>
        /// <returns>the inverse cumulative distribution at location <paramref name="p"/>.</returns>
        public double InvCDF(double p)
        {
            double EquationToSolve(double x) => CumulativeDistribution(x) - p;
            if (!RootFinding.NewtonRaphson.TryFindRoot(EquationToSolve, Density, Mode, 0, double.PositiveInfinity, 1e-8, 100, out double quantile))
                throw new NonConvergenceException("Numerical estimation of the statistic has failed. The used solver did not succeed in finding a root.");
            return quantile;
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <seealso cref="Density"/>
        public static double PDF(double mu, double lambda, double x)
        {
            if (!IsValidParameterSet(mu, lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return DensityImpl(mu, lambda, x);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        /// <seealso cref="DensityLn"/>
        public static double PDFLn(double mu, double lambda, double x)
        {
            if (!IsValidParameterSet(mu, lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return DensityLnImpl(mu, lambda, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double mu, double lambda, double x)
        {
            if (!IsValidParameterSet(mu, lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            return CumulativeDistributionImpl(mu, lambda, x);
        }

        /// <summary>
        /// Computes the inverse cumulative distribution (CDF) of the distribution at p, i.e. solving for P(X ≤ x) = p.
        /// </summary>
        /// <param name="mu">The mean (μ) of the distribution. Range: μ > 0.</param>
        /// <param name="lambda">The shape (λ) of the distribution. Range: λ > 0.</param>
        /// <param name="p">The location at which to compute the inverse cumulative distribution function.</param>
        ///  <returns>the inverse cumulative distribution at location <paramref name="p"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double InvCDF(double mu, double lambda, double p)
        {
            if (!IsValidParameterSet(mu, lambda))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }
            var igDist = new InverseGaussian(mu, lambda);
            return igDist.InvCDF(p);
        }

        /// <summary>
        /// Estimates the Inverse Gaussian parameters from sample data with maximum-likelihood.
        /// </summary>
        /// <param name="samples">The samples to estimate the distribution parameters from.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples. Optional, can be null.</param>
        /// <returns>An Inverse Gaussian distribution.</returns>
        public static InverseGaussian Estimate(IEnumerable<double> samples, System.Random randomSource = null)
        {
            var samplesArray = samples.ToArray();
            var muHat = samplesArray.Mean();
            var lambdaHat = 1 / (1 / samplesArray.HarmonicMean() - 1 / muHat);
            return new InverseGaussian(muHat, lambdaHat, randomSource);
        }

        static double DensityImpl(double mu, double lambda, double x)
        {
            return Math.Sqrt(lambda / (2 * Math.PI * Math.Pow(x, 3))) * Math.Exp(-((lambda * Math.Pow(x - mu, 2)) / (2 * mu * mu * x)));
        }

        static double DensityLnImpl(double mu, double lambda, double x)
        {
            return Math.Log(Math.Sqrt(lambda / (2 * Math.PI * Math.Pow(x, 3)))) - ((lambda * Math.Pow(x - mu, 2)) / (2 * mu * mu * x));
        }

        static double CumulativeDistributionImpl(double mu, double lambda, double x)
        {
            return Normal.CDF(0, 1, Math.Sqrt(lambda / x) * (x / mu - 1)) + Math.Exp(2 * lambda / mu) * Normal.CDF(0, 1, -Math.Sqrt(lambda / x) * (x / mu + 1));
        }
    }
}
