// <copyright file="TruncatedNormalTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2014 Math.NET
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

using MathNet.Numerics.Distributions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous 
{
	[TestFixture, Category("Distributions")]
	public class TruncatedNormalTests 
	{

		/// <summary>
		/// Can create a truncated normal without bounds.
		/// </summary>
		[TestCase(0.0, 0.0)]
		[TestCase(10.0, 0.1)]
		[TestCase(-5.0, 1.0)]
		[TestCase(0.0, 10.0)]
		[TestCase(10.0, 100.0)]
		[TestCase(-5.0, Double.PositiveInfinity)]
		public void CanCreateWithoutBounds(double mean, double stdDev) 
		{
			var truncatedNormal = new TruncatedNormal(mean, stdDev);
			Assert.IsTrue(double.IsNegativeInfinity(truncatedNormal.Minimum));
			Assert.IsTrue(double.IsPositiveInfinity(truncatedNormal.Maximum));
			Assert.AreEqual(mean, truncatedNormal.Mean);
			Assert.AreEqual(stdDev, truncatedNormal.StdDev);
		}

		/// <summary>
		/// Constructor fails with negative stdDev or incorrectly ordered bounds.
		/// </summary>
		/// <param name="mean"></param>
		/// <param name="stdDev"></param>
		/// <param name="lower"></param>
		/// <param name="upper"></param>
		[TestCase(0.0, -1.0,-10d, 10d)]
		[TestCase(0.0, 1.0, 10d, 9d)]
		public void TruncatedNormalCreateFailsWithBadParameters(double mean, double stdDev, double lower, double upper) 
		{
			Assert.That(() => new TruncatedNormal(mean, stdDev, lower, upper), Throws.ArgumentException);
		}

		[TestCase(0.0, 1.0, double.NegativeInfinity, double.PositiveInfinity)]
		[TestCase(0.0, 1.0, -5.0, 5.0)]
		[TestCase(-6.0, 1.0, -5.0, 5.0)]
		[TestCase(8.0, 1.0, -5.0, 5.0)]
		[TestCase(15, 20.0, -20.0, 0.0)]
		public void ValidateMode(double mean, double stdDev, double lower, double upper) 
		{
			double mode;
			if(mean < lower) {
				mode = lower;
			} else if(mean <= upper) {
				mode = mean;
			} else {
				mode = upper;
			}
			var truncatedNormal = new TruncatedNormal(mean, stdDev, lower, upper);
			Assert.AreEqual(mode, truncatedNormal.Mode);
		}

		/// <summary>
		/// Validate cumulative distribution. Uses the same test cases as for the normal distribution
		/// as they should be equivalent.
		/// </summary>
		/// <param name="x">Input X value.</param>
		/// <param name="p">Expected value.</param>
		[TestCase(Double.NegativeInfinity, 0.0)]
		[TestCase(-5.0, 0.00000028665157187919391167375233287464535385442301361187883)]
		[TestCase(-2.0, 0.0002326290790355250363499258867279847735487493358890356)]
		[TestCase(-0.0, 0.0062096653257761351669781045741922211278977469230927036)]
		[TestCase(0.0, .0062096653257761351669781045741922211278977469230927036)]
		[TestCase(4.0, .30853753872598689636229538939166226011639782444542207)]
		[TestCase(5.0, .5)]
		[TestCase(6.0, .69146246127401310363770461060833773988360217555457859)]
		[TestCase(10.0, 0.9937903346742238648330218954258077788721022530769078)]
		[TestCase(Double.PositiveInfinity, 1.0)]
		public void ValidateCumulativeNoBounds(double x, double p) {
			var truncatedNormal = new TruncatedNormal(5.0, 2.0);
			AssertHelpers.AlmostEqualRelative(p, truncatedNormal.CumulativeDistribution(x), 14);
		}

		/// <summary>
		/// Validate inverse cumulative distribution. Uses the same test cases as for the normal distribution
		/// as they should be equivalent.
		/// </summary>
		/// <param name="x">Expected value</param>
		/// <param name="p">Input quantile</param>
		[TestCase(Double.NegativeInfinity, 0.0)]
		[TestCase(-5.0, 0.00000028665157187919391167375233287464535385442301361187883)]
		[TestCase(-2.0, 0.0002326290790355250363499258867279847735487493358890356)]
		[TestCase(-0.0, 0.0062096653257761351669781045741922211278977469230927036)]
		[TestCase(0.0,  0.0062096653257761351669781045741922211278977469230927036)]
		[TestCase(4.0, .30853753872598689636229538939166226011639782444542207)]
		[TestCase(5.0, .5)]
		[TestCase(6.0, .69146246127401310363770461060833773988360217555457859)]
		[TestCase(10.0, 0.9937903346742238648330218954258077788721022530769078)]
		[TestCase(Double.PositiveInfinity, 1.0)]
		public void ValidateInverseCumulativeNoBounds(double x, double p) 
		{
			var truncatedNormal = new TruncatedNormal(5.0, 2.0);
			AssertHelpers.AlmostEqualRelative(x, truncatedNormal.InverseCumulativeDistribution(p), 14);
		}

		/// <summary>
		/// Validate density when no bounds are specified. Uses same
		/// test cases as the Normal distribution as should be equivalent in this case.
		/// </summary>
		/// <param name="mean">Mean value.</param>
		/// <param name="sdev">Standard deviation value.</param>
		[TestCase(10.0, 0.1)]
		[TestCase(-5.0, 1.0)]
		[TestCase(0.0, 10.0)]
		[TestCase(10.0, 100.0)]
		[TestCase(-5.0, Double.PositiveInfinity)]
		public void ValidateDensityNoBounds(double mean, double sdev) {
			var n = new TruncatedNormal(mean, sdev);
			for (var i = 0; i < 11; i++) {
				var x = i - 5.0;
				var d = (mean - x) / sdev;
				var pdf = Math.Exp(-0.5 * d * d) / (sdev * Constants.Sqrt2Pi);
				AssertHelpers.AlmostEqualRelative(pdf, n.Density(x), 14);
			}
		}

		/// <summary>
		/// Validate density log when no bounds are specified. Uses same 
		/// test cases as the Normal distribution as should be equivalent in this case.
		/// </summary>
		/// <param name="mean">Mean value.</param>
		/// <param name="sdev">Standard deviation value.</param>
		[TestCase(10.0, 0.1)]
		[TestCase(-5.0, 1.0)]
		[TestCase(0.0, 10.0)]
		[TestCase(10.0, 100.0)]
		[TestCase(-5.0, Double.PositiveInfinity)]
		public void ValidateDensityLnNoBounds(double mean, double sdev) {
			var n = new TruncatedNormal(mean, sdev);
			for (var i = 0; i < 11; i++) {
				var x = i - 5.0;
				var d = (mean - x) / sdev;
				var pdfln = (-0.5 * (d * d)) - Math.Log(sdev) - Constants.LogSqrt2Pi;
				AssertHelpers.AlmostEqualRelative(pdfln, n.DensityLn(x), 14);
			}
		}

		[Test]
		public void CanSample() 
		{
			var truncatedNormal = new TruncatedNormal(5.0, 2.0, -10, 10.0);
			truncatedNormal.Sample();
		}


		/// <summary>
		/// Can sample sequence.
		/// </summary>
		[Test]
		public void CanSampleSequence() {
			var truncatedNormal = new TruncatedNormal(5.0, 2.0, -10, 10.0);
			var ied = truncatedNormal.Samples();
			GC.KeepAlive(ied.Take(5).ToArray());
		}
	}
}
