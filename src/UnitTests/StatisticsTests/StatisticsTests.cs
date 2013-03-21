// <copyright file="StatisticsTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Statistics Tests.
    /// </summary>
    [TestFixture]
    public class StatisticsTests
    {
        readonly IDictionary<string, StatTestData> _data = new Dictionary<string, StatTestData>
            {
                {"lottery", new StatTestData("./data/NIST/Lottery.dat")},
                {"lew", new StatTestData("./data/NIST/Lew.dat")},
                {"mavro", new StatTestData("./data/NIST/Mavro.dat")},
                {"michelso", new StatTestData("./data/NIST/Michelso.dat")},
                {"numacc1", new StatTestData("./data/NIST/NumAcc1.dat")},
                {"numacc2", new StatTestData("./data/NIST/NumAcc2.dat")},
                {"numacc3", new StatTestData("./data/NIST/NumAcc3.dat")},
                {"numacc4", new StatTestData("./data/NIST/NumAcc4.dat")}
            };

        [Test]
        public void ThrowsOnNullData()
        {
            double[] data = null;

            Assert.Throws<ArgumentNullException>(() => Statistics.Minimum(data));
            Assert.Throws<ArgumentNullException>(() => Statistics.Maximum(data));
            Assert.Throws<ArgumentNullException>(() => Statistics.Mean(data));
            Assert.Throws<ArgumentNullException>(() => Statistics.Median(data));
            Assert.Throws<ArgumentNullException>(() => Statistics.Variance(data));
            Assert.Throws<ArgumentNullException>(() => Statistics.StandardDeviation(data));
            Assert.Throws<ArgumentNullException>(() => Statistics.PopulationVariance(data));
            Assert.Throws<ArgumentNullException>(() => Statistics.PopulationStandardDeviation(data));

            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.Minimum(data));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.Maximum(data));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.Median(data));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.LowerQuartile(data));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.UpperQuartile(data));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.Percentile(data, 30));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.Quantile(data, 0.3));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.QuantileCompatible(data, 0.3, QuantileCompatibility.Nearest));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.InterquartileRange(data));
            Assert.Throws<ArgumentNullException>(() => SortedArrayStatistics.FiveNumberSummary(data));

            Assert.Throws<ArgumentNullException>(() => ArrayStatistics.Minimum(data));
            Assert.Throws<ArgumentNullException>(() => ArrayStatistics.Maximum(data));
            Assert.Throws<ArgumentNullException>(() => ArrayStatistics.Mean(data));

            Assert.Throws<ArgumentNullException>(() => StreamingStatistics.Minimum(data));
            Assert.Throws<ArgumentNullException>(() => StreamingStatistics.Maximum(data));
        }

        [Test]
        public void DoesNotThrowOnEmptyData()
        {
            double[] data = new double[0];

            Assert.DoesNotThrow(() => Statistics.Minimum(data));
            Assert.DoesNotThrow(() => Statistics.Maximum(data));
            Assert.DoesNotThrow(() => Statistics.Mean(data));
            Assert.DoesNotThrow(() => Statistics.Median(data));
            Assert.DoesNotThrow(() => Statistics.Variance(data));
            Assert.DoesNotThrow(() => Statistics.StandardDeviation(data));
            Assert.DoesNotThrow(() => Statistics.PopulationVariance(data));
            Assert.DoesNotThrow(() => Statistics.PopulationStandardDeviation(data));

            Assert.DoesNotThrow(() => SortedArrayStatistics.Minimum(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.Maximum(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.Median(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.LowerQuartile(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.UpperQuartile(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.Percentile(data, 30));
            Assert.DoesNotThrow(() => SortedArrayStatistics.Quantile(data, 0.3));
            Assert.DoesNotThrow(() => SortedArrayStatistics.QuantileCompatible(data, 0.3, QuantileCompatibility.Nearest));
            Assert.DoesNotThrow(() => SortedArrayStatistics.InterquartileRange(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.FiveNumberSummary(data));

            Assert.DoesNotThrow(() => ArrayStatistics.Minimum(data));
            Assert.DoesNotThrow(() => ArrayStatistics.Maximum(data));
            Assert.DoesNotThrow(() => ArrayStatistics.Mean(data));

            Assert.DoesNotThrow(() => StreamingStatistics.Minimum(data));
            Assert.DoesNotThrow(() => StreamingStatistics.Maximum(data));
        }

        [TestCase("lottery")]
        [TestCase("lew")]
        [TestCase("mavro")]
        [TestCase("michelso")]
        [TestCase("numacc1")]
        [TestCase("numacc2")]
        [TestCase("numacc3")]
        [TestCase("numacc4")]
        public void MeanConsistentWithNistData(string dataSet)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqual(data.Mean, Statistics.Mean(data.Data), 15);
            AssertHelpers.AlmostEqual(data.Mean, ArrayStatistics.Mean(data.Data), 15);
        }

        [TestCase("lottery")]
        [TestCase("lew")]
        [TestCase("mavro")]
        [TestCase("michelso")]
        [TestCase("numacc1")]
        [TestCase("numacc2")]
        [TestCase("numacc3")]
        [TestCase("numacc4")]
        public void NullableMeanConsistentWithNistData(string dataSet)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqual(data.Mean, Statistics.Mean(data.DataWithNulls), 15);
        }

        /// <summary>
        /// Standard Deviation.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="digits">Digits count.</param>
        [TestCase("lottery", 15)]
        [TestCase("lew", 15)]
        [TestCase("mavro", 12)]
        [TestCase("michelso", 12)]
        [TestCase("numacc1", 15)]
        [TestCase("numacc2", 13)]
        [TestCase("numacc3", 9)]
        [TestCase("numacc4", 8)]
        public void StandardDeviation(string dataSet, int digits)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqual(data.StandardDeviation, Statistics.StandardDeviation(data.Data), digits);
        }

        /// <summary>
        /// <c>Nullable</c> Standard Deviation.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="digits">Digits count.</param>
        [TestCase("lottery", 15)]
        [TestCase("lew", 15)]
        [TestCase("mavro", 12)]
        [TestCase("michelso", 12)]
        [TestCase("numacc1", 15)]
        [TestCase("numacc2", 13)]
        [TestCase("numacc3", 9)]
        [TestCase("numacc4", 8)]
        public void NullableStandardDeviation(string dataSet, int digits)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqual(data.StandardDeviation, Statistics.StandardDeviation(data.DataWithNulls), digits);
        }

        /// <summary>
        /// Standard Deviation with <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void StandardDeviationThrowsArgumentNullException()
        {
            double[] data = null;
            Assert.Throws<ArgumentNullException>(() => Statistics.StandardDeviation(data));
        }

        /// <summary>
        /// Validate Min/Max on a short sequence.
        /// </summary>
        [Test]
        public void ShortMinMax()
        {
            var samples = new[] {-1.0, 5, 0, -3, 10, -0.5, 4};
            Assert.That(Statistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(Statistics.Maximum(samples), Is.EqualTo(10), "Max");
            Assert.That(ArrayStatistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(ArrayStatistics.Maximum(samples), Is.EqualTo(10), "Max");

            var sorted = new double[samples.Length];
            Array.Copy(samples, sorted, samples.Length);
            Array.Sort(sorted);
            Assert.That(SortedArrayStatistics.Minimum(sorted), Is.EqualTo(-3), "Min");
            Assert.That(SortedArrayStatistics.Maximum(sorted), Is.EqualTo(10), "Max");
        }

        /// <summary>
        /// Validate Order Statistics and Median on a short sequence.
        /// </summary>
        [Test]
        public void ShortOrderMedian()
        {
            // -3 -1 -0.5 0  1  4 5 6 10
            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 1, 6};
            Assert.That(Statistics.Median(samples), Is.EqualTo(1), "Median");
            Assert.That(Statistics.OrderStatistic(samples, 1), Is.EqualTo(-3), "Order-1");
            Assert.That(Statistics.OrderStatistic(samples, 3), Is.EqualTo(-0.5), "Order-3");
            Assert.That(Statistics.OrderStatistic(samples, 7), Is.EqualTo(5), "Order-7");
            Assert.That(Statistics.OrderStatistic(samples, 9), Is.EqualTo(10), "Order-9");

            var sorted = new double[samples.Length];
            Array.Copy(samples, sorted, samples.Length);
            Array.Sort(sorted);
            Assert.That(SortedArrayStatistics.Median(sorted), Is.EqualTo(1), "Median");
        }

        /// <summary>
        /// Validate Median/Variance/StdDev on a longer fixed-random sequence of a,
        /// large mean but only a very small variance, verifying the numerical stability.
        /// Naive summation algorithms generally fail this test.
        /// </summary>
        [Test]
        public void StabilityMeanVariance()
        {
            // Test around 10^9, potential stability issues
            var gaussian = new Distributions.Normal(1e+9, 2)
                {
                    RandomSource = new Numerics.Random.MersenneTwister(100)
                };

            AssertHelpers.AlmostEqual(1e+9, Statistics.Mean(gaussian.Samples().Take(10000)), 11);
            AssertHelpers.AlmostEqual(4d, Statistics.Variance(gaussian.Samples().Take(10000)), 1);
            AssertHelpers.AlmostEqual(2d, Statistics.StandardDeviation(gaussian.Samples().Take(10000)), 2);
        }

        [Test]
        public void MinimumOfEmptyMustBeNaN()
        {
            Assert.That(StreamingStatistics.Minimum(new double[0]), Is.NaN);
            Assert.That(StreamingStatistics.Minimum(new[] {2d }), Is.Not.NaN);
        }

        [Test]
        public void MaximumOfEmptyMustBeNaN()
        {
            Assert.That(StreamingStatistics.Maximum(new double[0]), Is.NaN);
            Assert.That(StreamingStatistics.Maximum(new[] { 2d }), Is.Not.NaN);
        }

        [Test]
        public void SampleVarianceOfEmptyAndSingleMustBeNaN()
        {
            Assert.That(Statistics.Variance(new double[0]), Is.NaN);
            Assert.That(Statistics.Variance(new[] { 2d }), Is.NaN);
            Assert.That(Statistics.Variance(new[] { 2d, 3d }), Is.Not.NaN);
        }

        [Test]
        public void PopulationVarianceOfEmptyMustBeNaN()
        {
            Assert.That(Statistics.PopulationVariance(new double[0]), Is.NaN);
            Assert.That(Statistics.PopulationVariance(new[] { 2d }), Is.Not.NaN);
            Assert.That(Statistics.PopulationVariance(new[] { 2d, 3d }), Is.Not.NaN);
        }

        /// <summary>
        /// URL http://mathnetnumerics.codeplex.com/workitem/5667
        /// </summary>
        [Test]
        public void Median_CodeplexIssue5667()
        {
            var seq = File.ReadLines("./data/Codeplex-5667.csv").Select(double.Parse);
            Assert.AreEqual(1.0, Statistics.Median(seq));

            var sorted = seq.ToArray();
            Array.Sort(sorted);
            Assert.AreEqual(1.0, SortedArrayStatistics.Median(sorted));
        }
    }
}
