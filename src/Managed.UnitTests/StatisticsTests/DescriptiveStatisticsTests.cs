// <copyright file="DescriptiveStatisticsTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.Statistics
{
    using System;
    using System.Collections.Generic;
    using MathNet.Numerics.Statistics;
    using MbUnit.Framework;

    [TestFixture]
    public class DescriptiveStatisticsTests
    {
        private readonly IDictionary<string, StatTestData> mData = new Dictionary<string, StatTestData>();

        public DescriptiveStatisticsTests()
        {
            StatTestData lottery = new StatTestData("./data/NIST/Lottery.dat");
            mData.Add("lottery", lottery);
            StatTestData lew = new StatTestData("./data/NIST/Lew.dat");
            mData.Add("lew", lew);
            StatTestData mavro = new StatTestData("./data/NIST/Mavro.dat");
            mData.Add("mavro", mavro);
            StatTestData michelso = new StatTestData("./data/NIST/Michelso.dat");
            mData.Add("michelso", michelso);
            StatTestData numacc1 = new StatTestData("./data/NIST/NumAcc1.dat");
            mData.Add("numacc1", numacc1);
            StatTestData numacc2 = new StatTestData("./data/NIST/NumAcc2.dat");
            mData.Add("numacc2", numacc2);
            StatTestData numacc3 = new StatTestData("./data/NIST/NumAcc3.dat");
            mData.Add("numacc3", numacc3);
            StatTestData numacc4 = new StatTestData("./data/NIST/NumAcc4.dat");
            mData.Add("numacc4", numacc4);
        }

        [Test]
        public void Constructor_ThrowArgumentNullException()
        {
            const IEnumerable<double> data = null;
            const IEnumerable<double?> nullableData = null;

            Assert.Throws<ArgumentNullException>(() => new DescriptiveStatistics(data));
            Assert.Throws<ArgumentNullException>(() => new DescriptiveStatistics(data, true));
            Assert.Throws<ArgumentNullException>(() => new DescriptiveStatistics(nullableData));
            Assert.Throws<ArgumentNullException>(() => new DescriptiveStatistics(nullableData, true));
        }

        [Test]
        [Row("lottery", 15, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [Row("lew", 15, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [Row("mavro", 12, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [Row("michelso", 12, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [Row("numacc1", 15, 0, 0, 10000002, 10000001, 10000003, 3)]
        [Row("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [Row("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [Row("numacc4", 8, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableDouble(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            StatTestData data = mData[dataSet];
            DescriptiveStatistics stats = new DescriptiveStatistics(data.Data);

            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        [Test]
        [Row("lottery", -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [Row("lew", -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [Row("mavro", 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [Row("michelso", -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [Row("numacc1", 0, 0, 10000002, 10000001, 10000003, 3)]
        [Row("numacc2", 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [Row("numacc3", 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [Row("numacc4", 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableDoubleHighAccuracy(string dataSet, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            StatTestData data = mData[dataSet];
            DescriptiveStatistics stats = new DescriptiveStatistics(data.Data, true);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, 15);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 9);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 9);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        [Test]
        [Row("lottery", 15, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [Row("lew", 15, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [Row("mavro", 12, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [Row("michelso", 12, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [Row("numacc1", 15, 0, 0, 10000002, 10000001, 10000003, 3)]
        [Row("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [Row("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [Row("numacc4", 8, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableDoubleLowAccuracy(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            StatTestData data = mData[dataSet];
            DescriptiveStatistics stats = new DescriptiveStatistics(data.Data, false);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        [Test]
        [Row("lottery", 15, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [Row("lew", 15, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [Row("mavro", 12, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [Row("michelso", 12, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [Row("numacc1", 15, 0, 0, 10000002, 10000001, 10000003, 3)]
        [Row("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [Row("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [Row("numacc4", 8, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableNullableDouble(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            StatTestData data = mData[dataSet];
            DescriptiveStatistics stats = new DescriptiveStatistics(data.DataWithNulls);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 7);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 7);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        [Test]
        [Row("lottery", -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [Row("lew", -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [Row("mavro", 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [Row("michelso", -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [Row("numacc1", 0, 0, 10000002, 10000001, 10000003, 3)]
        [Row("numacc2", 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [Row("numacc3", 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [Row("numacc4", 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableNullableDoubleHighAccuracy(string dataSet, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            StatTestData data = mData[dataSet];
            DescriptiveStatistics stats = new DescriptiveStatistics(data.DataWithNulls, true);
            AssertHelpers.AlmostEqual(data.Mean, stats.Mean, 15);
            AssertHelpers.AlmostEqual(data.StandardDeviation, stats.StandardDeviation, 15);
            AssertHelpers.AlmostEqual(skewness, stats.Skewness, 9);
            AssertHelpers.AlmostEqual(kurtosis, stats.Kurtosis, 9);
            AssertHelpers.AlmostEqual(median, stats.Median, 15);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        [Test]
        [Row("lottery", 15, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [Row("lew", 15, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [Row("mavro", 12, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [Row("michelso", 12, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [Row("numacc1", 15, 0, 0, 10000002, 10000001, 10000003, 3)]
        [Row("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [Row("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [Row("numacc4", 8, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        public void IEnumerableNullableDoubleLowAccuracy(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            StatTestData data = mData[dataSet];
            DescriptiveStatistics stats = new DescriptiveStatistics(data.DataWithNulls, false);
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