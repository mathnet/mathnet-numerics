// <copyright file="DescriptiveStatisticsTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Descriptive statistics tests.
    /// </summary>
    [TestFixture]
    public class DescriptiveStatisticsTests
    {
        /// <summary>
        /// Statistics data.
        /// </summary>
        private readonly IDictionary<string, StatTestData> _data = new Dictionary<string, StatTestData>();

        /// <summary>
        /// Initializes a new instance of the DescriptiveStatisticsTests class.
        /// </summary>
        public DescriptiveStatisticsTests()
        {
            var lottery = new StatTestData("./data/NIST/Lottery.dat");
            _data.Add("lottery", lottery);
            var lew = new StatTestData("./data/NIST/Lew.dat");
            _data.Add("lew", lew);
            var mavro = new StatTestData("./data/NIST/Mavro.dat");
            _data.Add("mavro", mavro);
            var michelso = new StatTestData("./data/NIST/Michelso.dat");
            _data.Add("michelso", michelso);
            var numacc1 = new StatTestData("./data/NIST/NumAcc1.dat");
            _data.Add("numacc1", numacc1);
            var numacc2 = new StatTestData("./data/NIST/NumAcc2.dat");
            _data.Add("numacc2", numacc2);
            var numacc3 = new StatTestData("./data/NIST/NumAcc3.dat");
            _data.Add("numacc3", numacc3);
            var numacc4 = new StatTestData("./data/NIST/NumAcc4.dat");
            _data.Add("numacc4", numacc4);
        }

        /// <summary>
        /// Constructor with <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void ConstructorThrowArgumentNullException()
        {
            const IEnumerable<double> Data = null;
            const IEnumerable<double?> NullableData = null;

            Assert.Throws<ArgumentNullException>(() => new DescriptiveStatistics(Data));
            Assert.Throws<ArgumentNullException>(() => new DescriptiveStatistics(Data, true));
            Assert.Throws<ArgumentNullException>(() => new DescriptiveStatistics(NullableData));
            Assert.Throws<ArgumentNullException>(() => new DescriptiveStatistics(NullableData, true));
        }

        /// <summary>
        /// <c>IEnumerable</c> Double.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="digits">Digits count.</param>
        /// <param name="skewness">Skewness value.</param>
        /// <param name="kurtosis">Kurtosis value.</param>
        /// <param name="median">Median value.</param>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max value.</param>
        /// <param name="count">Count value.</param>
        [Test, Sequential]
        public void IEnumerableDouble(
            [Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet, 
            [Values(15, 15, 12, 12, 15, 13, 9, 8)] int digits, 
            [Values(-0.09333165310779, -0.050606638756334, 0.64492948110824, -0.0185388637725746, 0, 0, 0, 0)] double skewness, 
            [Values(-1.19256091074856, -1.49604979214447, -0.82052379677456, 0.33968459842539, 0, -2.003003003003, -2.003003003003, -2.00300300299913)] double kurtosis, 
            [Values(522.5, -162, 2.0018, 299.85, 10000002, 1.2, 1000000.2, 10000000.2)] double median, 
            [Values(4, -579, 2.0013, 299.62, 10000001, 1.1, 1000000.1, 10000000.1)] double min, 
            [Values(999, 300, 2.0027, 300.07, 10000003, 1.3, 1000000.3, 10000000.3)] double max, 
            [Values(218, 200, 50, 100, 3, 1001, 1001, 1001)] int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.Data);

            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        /// <summary>
        /// <c>IEnumerable</c> Double high accuracy.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="skewness">Skewness value.</param>
        /// <param name="kurtosis">Kurtosis value.</param>
        /// <param name="median">Median value.</param>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max value.</param>
        /// <param name="count">Count value.</param>
        [Test, Sequential]
        public void IEnumerableDoubleHighAccuracy(
            [Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet, 
            [Values(-0.09333165310779, -0.050606638756334, 0.64492948110824, -0.0185388637725746, 0, 0, 0, 0)] double skewness, 
            [Values(-1.19256091074856, -1.49604979214447, -0.82052379677456, 0.33968459842539, 0, -2.003003003003, -2.003003003003, -2.00300300299913)] double kurtosis, 
            [Values(522.5, -162, 2.0018, 299.85, 10000002, 1.2, 1000000.2, 10000000.2)] double median, 
            [Values(4, -579, 2.0013, 299.62, 10000001, 1.1, 1000000.1, 10000000.1)] double min, 
            [Values(999, 300, 2.0027, 300.07, 10000003, 1.3, 1000000.3, 10000000.3)] double max, 
            [Values(218, 200, 50, 100, 3, 1001, 1001, 1001)] int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.Data, true);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, 15);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 9);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 9);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        /// <summary>
        /// <c>IEnumerable</c> double low accuracy.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="digits">Digits count.</param>
        /// <param name="skewness">Skewness value.</param>
        /// <param name="kurtosis">Kurtosis value.</param>
        /// <param name="median">Median value.</param>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max value.</param>
        /// <param name="count">Count value.</param>
        [Test, Sequential]
        public void IEnumerableDoubleLowAccuracy(
            [Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet, 
            [Values(15, 15, 12, 12, 15, 13, 9, 8)] int digits, 
            [Values(-0.09333165310779, -0.050606638756334, 0.64492948110824, -0.0185388637725746, 0, 0, 0, 0)] double skewness, 
            [Values(-1.19256091074856, -1.49604979214447, -0.82052379677456, 0.33968459842539, 0, -2.003003003003, -2.003003003003, -2.00300300299913)] double kurtosis, 
            [Values(522.5, -162, 2.0018, 299.85, 10000002, 1.2, 1000000.2, 10000000.2)] double median, 
            [Values(4, -579, 2.0013, 299.62, 10000001, 1.1, 1000000.1, 10000000.1)] double min, 
            [Values(999, 300, 2.0027, 300.07, 10000003, 1.3, 1000000.3, 10000000.3)] double max, 
            [Values(218, 200, 50, 100, 3, 1001, 1001, 1001)] int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.Data, false);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        /// <summary>
        /// <c>IEnumerable</c> <c>Nullable</c> double.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="digits">Digits count.</param>
        /// <param name="skewness">Skewness value.</param>
        /// <param name="kurtosis">Kurtosis value.</param>
        /// <param name="median">Median value.</param>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max value.</param>
        /// <param name="count">Count value.</param>
        [Test, Sequential]
        public void IEnumerableNullableDouble(
            [Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet, 
            [Values(15, 15, 12, 12, 15, 13, 9, 8)] int digits, 
            [Values(-0.09333165310779, -0.050606638756334, 0.64492948110824, -0.0185388637725746, 0, 0, 0, 0)] double skewness, 
            [Values(-1.19256091074856, -1.49604979214447, -0.82052379677456, 0.33968459842539, 0, -2.003003003003, -2.003003003003, -2.00300300299913)] double kurtosis, 
            [Values(522.5, -162, 2.0018, 299.85, 10000002, 1.2, 1000000.2, 10000000.2)] double median, 
            [Values(4, -579, 2.0013, 299.62, 10000001, 1.1, 1000000.1, 10000000.1)] double min, 
            [Values(999, 300, 2.0027, 300.07, 10000003, 1.3, 1000000.3, 10000000.3)] double max, 
            [Values(218, 200, 50, 100, 3, 1001, 1001, 1001)] int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.DataWithNulls);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        /// <summary>
        /// <c>IEnumerable</c> <c>Nullable</c> double high accuracy.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="skewness">Skewness value.</param>
        /// <param name="kurtosis">Kurtosis value.</param>
        /// <param name="median">Median value.</param>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max value.</param>
        /// <param name="count">Count value.</param>
        [Test, Sequential]
        public void IEnumerableNullableDoubleHighAccuracy(
            [Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet, 
            [Values(-0.09333165310779, -0.050606638756334, 0.64492948110824, -0.0185388637725746, 0, 0, 0, 0)] double skewness, 
            [Values(-1.19256091074856, -1.49604979214447, -0.82052379677456, 0.33968459842539, 0, -2.003003003003, -2.003003003003, -2.00300300299913)] double kurtosis, 
            [Values(522.5, -162, 2.0018, 299.85, 10000002, 1.2, 1000000.2, 10000000.2)] double median, 
            [Values(4, -579, 2.0013, 299.62, 10000001, 1.1, 1000000.1, 10000000.1)] double min, 
            [Values(999, 300, 2.0027, 300.07, 10000003, 1.3, 1000000.3, 10000000.3)] double max, 
            [Values(218, 200, 50, 100, 3, 1001, 1001, 1001)] int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.DataWithNulls, true);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, 15);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 9);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 9);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        /// <summary>
        /// <c>IEnumerable</c> <c>Nullable</c> Double Low Accuracy.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="digits">Digits count.</param>
        /// <param name="skewness">Skewness value.</param>
        /// <param name="kurtosis">Kurtosis value.</param>
        /// <param name="median">Median value.</param>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max value.</param>
        /// <param name="count">Count value.</param>
        [Test, Sequential]
        public void IEnumerableNullableDoubleLowAccuracy(
            [Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet, 
            [Values(15, 15, 12, 12, 15, 13, 9, 8)] int digits, 
            [Values(-0.09333165310779, -0.050606638756334, 0.64492948110824, -0.0185388637725746, 0, 0, 0, 0)] double skewness, 
            [Values(-1.19256091074856, -1.49604979214447, -0.82052379677456, 0.33968459842539, 0, -2.003003003003, -2.003003003003, -2.00300300299913)] double kurtosis, 
            [Values(522.5, -162, 2.0018, 299.85, 10000002, 1.2, 1000000.2, 10000000.2)] double median, 
            [Values(4, -579, 2.0013, 299.62, 10000001, 1.1, 1000000.1, 10000000.1)] double min, 
            [Values(999, 300, 2.0027, 300.07, 10000003, 1.3, 1000000.3, 10000000.3)] double max, 
            [Values(218, 200, 50, 100, 3, 1001, 1001, 1001)] int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.DataWithNulls, false);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }
    }
}
