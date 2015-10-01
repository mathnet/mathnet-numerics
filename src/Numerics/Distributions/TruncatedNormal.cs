// <copyright file="TruncatedNormal.cs" company="Math.NET">
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
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions {

	/// <summary>
	/// Truncated Normal Distribution.
	/// For more details about this distribution, see
	/// <a href="https://en.wikipedia.org/wiki/Truncated_normal_distribution">Wikipedia - Truncated normal distribution</a>
	/// </summary>
	public class TruncatedNormal : IContinuousDistribution {

		System.Random _random;

		readonly double _mean;
		readonly double _stdDev;
		readonly double _lowerBound;
		readonly double _upperBound;
		readonly Normal _uncorrectedNormal;
		/// <summary>
		/// The total density of the uncorrected normal distribution which is within the lower and upper bounds.
		/// Referred to as "Z" in the wikipedia equations. Z = Φ(UpperBound) - Φ(LowerBound).
		/// </summary>
		readonly double _cumulativeDensityWithinBounds;

		/// <summary>
		/// Initializes a new instance of the TruncatedNormal class with a particular mean, standard deviation, lower bound, and upper bound. The distribution will
		/// be initialized with the default <seealso cref="System.Random"/> random number generator. The mean and standard deviation are that of the untruncated
		/// normal distribution.
		/// </summary>
		/// <param name="mean">The mean (μ) of the untruncated distribution.</param>
		/// <param name="stddev">The standard deviation (σ) of the untruncated distribution. Range: σ ≥ 0.</param>
		/// <param name="lowerBound">The inclusive lower bound of the truncated distribution. Default is double.NegativeInfinity.</param>
		/// <param name="upperBound">The inclusive upper bound of the truncated distribution. Must be larger than <paramref name="lowerBound"/>.
		/// Default is double.PositiveInfinity.</param>
		public TruncatedNormal(double mean, double stddev, double lowerBound = double.NegativeInfinity, double upperBound = double.PositiveInfinity) 
			:this(mean, stddev, SystemRandomSource.Default, lowerBound, upperBound) 
		{

		}

		/// <summary>
		/// Initializes a new instance of the Normal class with a particular mean and standard deviation. The distribution will
		/// be initialized with the default <seealso cref="System.Random"/> random number generator.
		/// </summary>
		/// <param name="mean">The mean (μ) of the normal distribution.</param>
		/// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
		/// <param name="randomSource">The random number generator which is used to draw random samples.</param>
		public TruncatedNormal(double mean, double stddev, System.Random randomSource, double lowerBound = double.NegativeInfinity, double upperBound = double.PositiveInfinity) 
		{
			if (!IsValidParameterSet(mean, stddev, lowerBound, upperBound)) 
			{
				throw new ArgumentException(Resources.InvalidDistributionParameters);
			}

			_random = randomSource ?? SystemRandomSource.Default;
			_mean = mean;
			_stdDev = stddev;
			_lowerBound = lowerBound;
			_upperBound = upperBound;
			_uncorrectedNormal = Normal.WithMeanStdDev(_mean, _stdDev);
			_cumulativeDensityWithinBounds = _uncorrectedNormal.CumulativeDistribution(_upperBound) - _uncorrectedNormal.CumulativeDistribution(_lowerBound);
		}

		/// <summary>
		/// Tests whether the provided values are valid parameters for this distribution.
		/// </summary>
		/// <param name="mean">The mean (μ) of the normal distribution.</param>
		/// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
		public static bool IsValidParameterSet(double mean, double stddev, double lowerBound, double upperBound) 
		{
			bool normalRequirements = Normal.IsValidParameterSet(mean, stddev);
			bool boundsAreOrdered = lowerBound < upperBound;
			return normalRequirements && boundsAreOrdered;
		}

		public override string ToString() {
			return "TruncatedNormal(μ = " + _mean + ", σ = " + _stdDev +", LowerBound = " + _lowerBound + ", UpperBound = " + _upperBound + ")";
		}

		/// <summary>
		/// Gets the mode of the normal distribution.
		/// </summary>
		public double Mode 
		{
			get 
			{
				if (_mean < _lowerBound)
					return _lowerBound;
				if (_mean > _upperBound)
					return _upperBound;
				return _mean;
			}
		}

		/// <summary>
		/// Gets the minimum of the truncated normal distribution.
		/// </summary>
		public double Minimum 
		{
			get { return _lowerBound; }
		}

		/// <summary>
		/// Gets the maximum of the truncated normal distribution.
		/// </summary>
		public double Maximum 
		{
			get { return _upperBound; }
		}

		/// <summary>
		/// Gets the mean (μ) of the truncated normal distribution.
		/// </summary>
		public double Mean 
		{
			get 
			{
				var pdfDifference = _uncorrectedNormal.Density(_lowerBound) - _uncorrectedNormal.Density(_upperBound);
				var diffFromUncorrected = pdfDifference * _stdDev / _cumulativeDensityWithinBounds;
				return _mean + diffFromUncorrected;
			}
		}

		/// <summary>
		/// Gets the variance of the truncated normal distribution. 
		/// </summary>
		public double Variance 
		{
			get 
			{
				//TODO might need special handling for cases where either or both bounds are infinity

				//Second term
				var secondNumerator = _lowerBound * _uncorrectedNormal.Density(_lowerBound) - _upperBound * _uncorrectedNormal.Density(_upperBound);
				var secordTerm = secondNumerator / _cumulativeDensityWithinBounds;

				//Third term
				var thirdNumerator = _uncorrectedNormal.Density(_lowerBound) - _uncorrectedNormal.Density(_upperBound);
				var thirdTerm = (thirdNumerator / _cumulativeDensityWithinBounds) * (thirdNumerator / _cumulativeDensityWithinBounds);

				var sumOfTerms = 1 + secordTerm + thirdTerm;

				return _stdDev * _stdDev * sumOfTerms;
			}
		}

		/// <summary>
		/// Gets the standard deviation (σ) of the truncated normal distribution. Range: σ ≥ 0.
		/// </summary>
		public double StdDev 
		{
			get { return Math.Sqrt(Variance); }
		}

		/// <summary>
		/// Gets the entropy of the truncated normal distribution.
		/// </summary>
		public double Entropy 
		{
			get 
			{
				var firstTerm = Constants.LogSqrt2PiE + Math.Log(_stdDev + _cumulativeDensityWithinBounds);

				var secondNumerator = _lowerBound * _uncorrectedNormal.Density(_lowerBound) - _upperBound * _uncorrectedNormal.Density(_upperBound);
				var secondTerm = secondNumerator / (2 * _cumulativeDensityWithinBounds);

				return firstTerm + secondTerm;
			}
		}

		public double Skewness
		{
			get 
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the median of the truncated distribution.
		/// </summary>
		public double Median 
		{
			get 
			{
				return InverseCumulativeDistribution(0.5);
			}
		}

		/// <summary>
		/// Gets or sets the random number generator which is used to draw random samples.
		/// </summary>
		public System.Random RandomSource 
		{
			get { return _random; }
			set { _random = value ?? SystemRandomSource.Default; }
		}

		/// <summary>
		/// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
		/// </summary>
		/// <param name="x">The location at which to compute the density.</param>
		/// <returns>the density at <paramref name="x"/>.</returns>
		/// <seealso cref="PDF"/>
		public double Density(double x) 
		{
			if (x < _lowerBound || _upperBound < x)
				return 0d;

			return _uncorrectedNormal.Density(x) / (_stdDev * _cumulativeDensityWithinBounds);
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

		//TODO: implement sampling, use method described by Mazet here: http://miv.u-strasbg.fr/mazet/rtnorm/
		// see implmentations listed on that page for examples.

		public double Sample() 
		{
			throw new NotImplementedException();
		}

		public void Samples(double[] values) 
		{
			throw new NotImplementedException();
		}

		public IEnumerable<double> Samples() 
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
		/// </summary>
		/// <param name="x">The location at which to compute the cumulative distribution function.</param>
		/// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
		/// <seealso cref="CDF"/>
		public double CumulativeDistribution(double x) 
		{
			if (x < _lowerBound)
				return 0d;
			if (x > _upperBound)
				return 1d;

			double cumulative = _uncorrectedNormal.CumulativeDistribution(x) - _uncorrectedNormal.CumulativeDistribution(_lowerBound);
			return cumulative / _cumulativeDensityWithinBounds;
		}

		/// <summary>
		/// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
		/// at the given probability. This is also known as the quantile or percent point function.
		/// </summary>
		/// <param name="p">The location at which to compute the inverse cumulative density.</param>
		/// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
		/// <seealso cref="InvCDF"/>
		public double InverseCumulativeDistribution(double p) 
		{
			//TODO check that this is correct with someone.
			var pUntruncated = p * _cumulativeDensityWithinBounds + _uncorrectedNormal.CumulativeDistribution(_lowerBound);

			return _uncorrectedNormal.InverseCumulativeDistribution(pUntruncated);
		}

	}
}
