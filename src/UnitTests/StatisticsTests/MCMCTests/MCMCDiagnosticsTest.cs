// <copyright file="MCMCDiagonistics.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics.Mcmc.Diagnostics;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{
    /// <summary>
    /// MCMCDiagonistics testing.
    /// </summary>
    [TestFixture]
    public class MCMCDiagnosticsTest
    {
        /// <summary>
        /// For generation of a random series to test the methods.
        /// </summary>
        private System.Random rnd = new System.Random();
        /// <summary>
        /// Distribution to sample the entries of the random series from.
        /// </summary>
        private Normal dis = new Normal(0, 1);


        /// <summary>
        /// Testing the ACF function using a randomly generated series with a range
        /// of lags.
        /// </summary>
        /// <param name="startlag">Minimum value of lag in the test.</param>
        /// <param name="endlag">Maximum value of lag in the test.</param>
        [TestCase(0, 10)]
        [TestCase(11, 20)]
        [TestCase(21, 30)]
        [TestCase(31, 40)]
        public void TestACF(int startlag, int endlag)
        {
            for (int lag = startlag; lag < endlag; lag++)
            {
                int Length = 10000;
                double[] firstSeries = new double[Length - lag];
                double[] secondSeries = new double[Length - lag];

                double[] Series = new double[Length];

                for (int i = 0; i < Length; i++)
                { Series[i] = RandomSeries(); }

                double[] TransformedSeries = new double[Length];
                for (int i = 0; i < Length; i++)
                { TransformedSeries[i] = Series[i] * Series[i]; }

                Array.Copy(TransformedSeries, firstSeries, Length - lag);
                Array.Copy(TransformedSeries, lag, secondSeries, 0, Length - lag);

                double result = MCMCDiagnostics.ACF(Series, lag, x=>x*x);
                double correlation = Correlation.Pearson(firstSeries, secondSeries);
                Assert.AreEqual(result, correlation, 10e-13);

            }
        }

        /// <summary>
        /// Set lag to be greater than the length of the series throws a 
        /// <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void LagOutOfRange()
        {
            int Length = 10;
            double[] Series = new double[Length];
            Assert.Throws<ArgumentOutOfRangeException>(() => MCMCDiagnostics.ACF(Series, 11, x=>x));

        }
        /// <summary>
        /// Set lag to be negative throws a <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void LagNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MCMCDiagnostics.ACF(new double[10], -1, x=>x));
        }

        /// <summary>
        /// Generating a random number used for the entry of the series.
        /// </summary>
        /// <returns>A random number.</returns>
        private double RandomSeries()
        { return rnd.NextDouble() + rnd.NextDouble() * (dis.Sample()); }

        /// <summary>
        /// Testing the effective size using a random series. 
        /// </summary>
        [Test]
        public void EffectiveSizeTest()
        {
            int Length = 10;
            double[] Series = new double[Length];
            for (int i = 0; i < Length; i++)
            { Series[i] = RandomSeries(); }

            double rho = MCMCDiagnostics.ACF(Series, 1,x=>x*x);
            double ESS = (1 - rho) / (1 + rho) * Length;
            Assert.AreEqual(ESS, MCMCDiagnostics.EffectiveSize(Series,x=>x*x), 10e-13);
        }
    }
}
