// <copyright file="DescriptiveStatisticsTests.cs" company="Math.NET">
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
#if !PORTABLE
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Descriptive statistics tests.
    /// </summary>
    /// <remarks>NOTE: this class is not included into Silverlight version, because it uses data from local files. 
    /// In Silverlight access to local files is forbidden, except several cases.</remarks>
    [TestFixture]
    public class DescriptiveStatisticsTests
    {
        /// <summary>
        /// Statistics data.
        /// </summary>
        readonly IDictionary<string, StatTestData> _data = new Dictionary<string, StatTestData>();

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
            var meixner = new StatTestData("./data/NIST/Meixner.dat");
            _data.Add("meixner", meixner);
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
        [TestCase("lottery", 15, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [TestCase("lew", 15, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [TestCase("mavro", 12, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [TestCase("michelso", 12, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [TestCase("numacc1", 15, 0, 0, 10000002, 10000001, 10000003, 3)]
        [TestCase("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [TestCase("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [TestCase("numacc4", 8, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        [TestCase("meixner", 8, -0.016649617280859657, 0.8171318629552635, -0.002042931016531602, -4.825626912281697, 5.3018298664184913, 10000)]
        public void IEnumerableDouble(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.Data);

            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
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
        [TestCase("lottery", -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [TestCase("lew", -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [TestCase("mavro", 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [TestCase("michelso", -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [TestCase("numacc1", 0, 0, 10000002, 10000001, 10000003, 3)]
        [TestCase("numacc2", 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [TestCase("numacc3", 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [TestCase("numacc4", 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableDoubleHighAccuracy(string dataSet, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.Data, true);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, 15);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 9);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 9);
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
        [TestCase("lottery", 15, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [TestCase("lew", 15, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [TestCase("mavro", 12, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [TestCase("michelso", 12, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [TestCase("numacc1", 15, 0, 0, 10000002, 10000001, 10000003, 3)]
        [TestCase("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [TestCase("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [TestCase("numacc4", 8, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableDoubleLowAccuracy(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.Data, false);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
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
        [TestCase("lottery", 15, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [TestCase("lew", 15, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [TestCase("mavro", 12, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [TestCase("michelso", 12, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [TestCase("numacc1", 15, 0, 0, 10000002, 10000001, 10000003, 3)]
        [TestCase("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [TestCase("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [TestCase("numacc4", 8, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableNullableDouble(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.DataWithNulls);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
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
        [TestCase("lottery", -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [TestCase("lew", -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [TestCase("mavro", 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [TestCase("michelso", -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [TestCase("numacc1", 0, 0, 10000002, 10000001, 10000003, 3)]
        [TestCase("numacc2", 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [TestCase("numacc3", 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [TestCase("numacc4", 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableNullableDoubleHighAccuracy(string dataSet, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.DataWithNulls, true);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, 15);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 9);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 9);
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
        [TestCase("lottery", 15, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [TestCase("lew", 15, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [TestCase("mavro", 12, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [TestCase("michelso", 12, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [TestCase("numacc1", 15, 0, 0, 10000002, 10000001, 10000003, 3)]
        [TestCase("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [TestCase("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [TestCase("numacc4", 8, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableNullableDoubleLowAccuracy(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            var data = _data[dataSet];
            var stats = new DescriptiveStatistics(data.DataWithNulls, false);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }
    }
#endif
}
