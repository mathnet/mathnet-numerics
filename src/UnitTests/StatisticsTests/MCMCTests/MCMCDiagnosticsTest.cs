// <copyright file="MCMCDiagonistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Statistics.Mcmc;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.StatisticsTests.McmcTests
{
    /// <summary>
    /// MCMCDiagonistics testing.
    /// </summary>
    [TestFixture, Category("Statistics")]
    public class MCMCDiagnosticsTest
    {
        /// <summary>
        /// For generation of a random series to test the methods.
        /// </summary>
        private readonly System.Random _rnd = new System.Random(0);
        /// <summary>
        /// Distribution to sample the entries of the random series from.
        /// </summary>
        private readonly Normal _dis = new Normal(0, 1);


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
                const int length = 10000;
                var firstSeries = new double[length - lag];
                var secondSeries = new double[length - lag];

                var series = new double[length];

                for (int i = 0; i < length; i++)
                { series[i] = RandomSeries(); }

                var transformedSeries = new double[length];
                for (int i = 0; i < length; i++)
                { transformedSeries[i] = series[i] * series[i]; }

                Array.Copy(transformedSeries, firstSeries, length - lag);
                Array.Copy(transformedSeries, lag, secondSeries, 0, length - lag);

                double result = MCMCDiagnostics.ACF(series, lag, x=>x*x);
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
            const int length = 10;
            var series = new double[length];
            Assert.That(() => MCMCDiagnostics.ACF(series, 11, x=>x), Throws.TypeOf<ArgumentOutOfRangeException>());

        }
        /// <summary>
        /// Set lag to be negative throws a <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void LagNegative()
        {
            Assert.That(() => MCMCDiagnostics.ACF(new double[10], -1, x=>x), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Generating a random number used for the entry of the series.
        /// </summary>
        /// <returns>A random number.</returns>
        private double RandomSeries()
        { return _rnd.NextDouble() + _rnd.NextDouble() * (_dis.Sample()); }

        /// <summary>
        /// Testing the effective size using a random series.
        /// </summary>
        [Test]
        public void EffectiveSizeTest()
        {
            const int length = 10;
            var series = new double[length];
            for (int i = 0; i < length; i++)
            { series[i] = RandomSeries(); }

            double rho = MCMCDiagnostics.ACF(series, 1,x=>x*x);
            double ess = (1 - rho) / (1 + rho) * length;
            Assert.AreEqual(ess, MCMCDiagnostics.EffectiveSize(series,x=>x*x), 10e-13);
        }
    }
}
