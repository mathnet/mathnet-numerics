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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    using Statistics;

    /// <summary>
    /// Statistics Tests
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

            // ReSharper disable InvokeAsExtensionMethod
            Assert.That(() => Statistics.Minimum(data), Throws.Exception);
            Assert.That(() => Statistics.Maximum(data), Throws.Exception);
            Assert.That(() => Statistics.Mean(data), Throws.Exception);
            Assert.That(() => Statistics.Median(data), Throws.Exception);
            Assert.That(() => Statistics.Quantile(data, 0.3), Throws.Exception);
            Assert.That(() => Statistics.Variance(data), Throws.Exception);
            Assert.That(() => Statistics.StandardDeviation(data), Throws.Exception);
            Assert.That(() => Statistics.PopulationVariance(data), Throws.Exception);
            Assert.That(() => Statistics.PopulationStandardDeviation(data), Throws.Exception);
            Assert.That(() => Statistics.Covariance(data, data), Throws.Exception);
            Assert.That(() => Statistics.PopulationCovariance(data, data), Throws.Exception);
            // ReSharper restore InvokeAsExtensionMethod

            Assert.That(() => SortedArrayStatistics.Minimum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.Minimum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.Maximum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.OrderStatistic(data, 1), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.Median(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.LowerQuartile(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.UpperQuartile(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.Percentile(data, 30), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.Quantile(data, 0.3), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.QuantileCustom(data, 0.3, 0, 0, 1, 0), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.QuantileCustom(data, 0.3, QuantileDefinition.Nearest), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.InterquartileRange(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => SortedArrayStatistics.FiveNumberSummary(data), Throws.Exception.TypeOf<NullReferenceException>());

            Assert.That(() => ArrayStatistics.Minimum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.Maximum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.OrderStatisticInplace(data, 1), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.Mean(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.Variance(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.StandardDeviation(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.PopulationVariance(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.PopulationStandardDeviation(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.Covariance(data, data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.PopulationCovariance(data, data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.MedianInplace(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.QuantileInplace(data, 0.3), Throws.Exception.TypeOf<NullReferenceException>());

            Assert.That(() => StreamingStatistics.Minimum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Maximum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Mean(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Variance(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.StandardDeviation(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.PopulationVariance(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.PopulationStandardDeviation(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Covariance(data, data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.PopulationCovariance(data, data), Throws.Exception.TypeOf<NullReferenceException>());
        }

        [Test]
        public void DoesNotThrowOnEmptyData()
        {
            double[] data = new double[0];

            Assert.DoesNotThrow(() => Statistics.Minimum(data));
            Assert.DoesNotThrow(() => Statistics.Maximum(data));
            Assert.DoesNotThrow(() => Statistics.Mean(data));
            Assert.DoesNotThrow(() => Statistics.Median(data));
            Assert.DoesNotThrow(() => Statistics.Quantile(data, 0.3));
            Assert.DoesNotThrow(() => Statistics.Variance(data));
            Assert.DoesNotThrow(() => Statistics.StandardDeviation(data));
            Assert.DoesNotThrow(() => Statistics.PopulationVariance(data));
            Assert.DoesNotThrow(() => Statistics.PopulationStandardDeviation(data));
            Assert.DoesNotThrow(() => Statistics.Covariance(data, data));
            Assert.DoesNotThrow(() => Statistics.PopulationCovariance(data, data));

            Assert.DoesNotThrow(() => SortedArrayStatistics.Minimum(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.Maximum(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.OrderStatistic(data, 1));
            Assert.DoesNotThrow(() => SortedArrayStatistics.Median(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.LowerQuartile(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.UpperQuartile(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.Percentile(data, 30));
            Assert.DoesNotThrow(() => SortedArrayStatistics.Quantile(data, 0.3));
            Assert.DoesNotThrow(() => SortedArrayStatistics.QuantileCustom(data, 0.3, 0, 0, 1, 0));
            Assert.DoesNotThrow(() => SortedArrayStatistics.QuantileCustom(data, 0.3, QuantileDefinition.Nearest));
            Assert.DoesNotThrow(() => SortedArrayStatistics.InterquartileRange(data));
            Assert.DoesNotThrow(() => SortedArrayStatistics.FiveNumberSummary(data));

            Assert.DoesNotThrow(() => ArrayStatistics.Minimum(data));
            Assert.DoesNotThrow(() => ArrayStatistics.Maximum(data));
            Assert.DoesNotThrow(() => ArrayStatistics.OrderStatisticInplace(data, 1));
            Assert.DoesNotThrow(() => ArrayStatistics.Mean(data));
            Assert.DoesNotThrow(() => ArrayStatistics.Variance(data));
            Assert.DoesNotThrow(() => ArrayStatistics.StandardDeviation(data));
            Assert.DoesNotThrow(() => ArrayStatistics.PopulationVariance(data));
            Assert.DoesNotThrow(() => ArrayStatistics.PopulationStandardDeviation(data));
            Assert.DoesNotThrow(() => ArrayStatistics.Covariance(data, data));
            Assert.DoesNotThrow(() => ArrayStatistics.PopulationCovariance(data, data));
            Assert.DoesNotThrow(() => ArrayStatistics.MedianInplace(data));
            Assert.DoesNotThrow(() => ArrayStatistics.QuantileInplace(data, 0.3));

            Assert.DoesNotThrow(() => StreamingStatistics.Minimum(data));
            Assert.DoesNotThrow(() => StreamingStatistics.Maximum(data));
            Assert.DoesNotThrow(() => StreamingStatistics.Mean(data));
            Assert.DoesNotThrow(() => StreamingStatistics.Variance(data));
            Assert.DoesNotThrow(() => StreamingStatistics.StandardDeviation(data));
            Assert.DoesNotThrow(() => StreamingStatistics.PopulationVariance(data));
            Assert.DoesNotThrow(() => StreamingStatistics.PopulationStandardDeviation(data));
            Assert.DoesNotThrow(() => StreamingStatistics.Covariance(data, data));
            Assert.DoesNotThrow(() => StreamingStatistics.PopulationCovariance(data, data));
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
            AssertHelpers.AlmostEqualRelative(data.Mean, Statistics.Mean(data.Data), 14);
            AssertHelpers.AlmostEqualRelative(data.Mean, ArrayStatistics.Mean(data.Data), 14);
            AssertHelpers.AlmostEqualRelative(data.Mean, StreamingStatistics.Mean(data.Data), 14);
            AssertHelpers.AlmostEqualRelative(data.Mean, Statistics.MeanVariance(data.Data).Item1, 14);
            AssertHelpers.AlmostEqualRelative(data.Mean, ArrayStatistics.MeanVariance(data.Data).Item1, 14);
            AssertHelpers.AlmostEqualRelative(data.Mean, StreamingStatistics.MeanVariance(data.Data).Item1, 14);
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
            AssertHelpers.AlmostEqualRelative(data.Mean, Statistics.Mean(data.DataWithNulls), 14);
        }

        [TestCase("lottery", 14)]
        [TestCase("lew", 14)]
        [TestCase("mavro", 11)]
        [TestCase("michelso", 11)]
        [TestCase("numacc1", 15)]
        [TestCase("numacc2", 13)]
        [TestCase("numacc3", 9)]
        [TestCase("numacc4", 7)]
        public void StandardDeviationConsistentWithNistData(string dataSet, int digits)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, Statistics.StandardDeviation(data.Data), digits);
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, ArrayStatistics.StandardDeviation(data.Data), digits);
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, StreamingStatistics.StandardDeviation(data.Data), digits);
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, Math.Sqrt(Statistics.MeanVariance(data.Data).Item2), digits);
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, Math.Sqrt(ArrayStatistics.MeanVariance(data.Data).Item2), digits);
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, Math.Sqrt(StreamingStatistics.MeanVariance(data.Data).Item2), digits);
        }

        [TestCase("lottery", 14)]
        [TestCase("lew", 14)]
        [TestCase("mavro", 11)]
        [TestCase("michelso", 11)]
        [TestCase("numacc1", 15)]
        [TestCase("numacc2", 13)]
        [TestCase("numacc3", 9)]
        [TestCase("numacc4", 7)]
        public void NullableStandardDeviationConsistentWithNistData(string dataSet, int digits)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, Statistics.StandardDeviation(data.DataWithNulls), digits);
        }

        [Test]
        public void MinimumMaximumOnShortSequence()
        {
            var samples = new[] {-1.0, 5, 0, -3, 10, -0.5, 4};
            Assert.That(Statistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(Statistics.Maximum(samples), Is.EqualTo(10), "Max");
            Assert.That(ArrayStatistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(ArrayStatistics.Maximum(samples), Is.EqualTo(10), "Max");
            Assert.That(StreamingStatistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(StreamingStatistics.Maximum(samples), Is.EqualTo(10), "Max");

            Array.Sort(samples);
            Assert.That(SortedArrayStatistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(SortedArrayStatistics.Maximum(samples), Is.EqualTo(10), "Max");
        }

        [Test]
        public void OrderStatisticsOnShortSequence()
        {
            // -3 -1 -0.5 0  1  4 5 6 10
            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 1, 6 };

            var f = Statistics.OrderStatisticFunc(samples);
            Assert.That(f(0), Is.NaN, "Order-0 (bad)");
            Assert.That(f(1), Is.EqualTo(-3), "Order-1");
            Assert.That(f(2), Is.EqualTo(-1), "Order-2");
            Assert.That(f(3), Is.EqualTo(-0.5), "Order-3");
            Assert.That(f(7), Is.EqualTo(5), "Order-7");
            Assert.That(f(8), Is.EqualTo(6), "Order-8");
            Assert.That(f(9), Is.EqualTo(10), "Order-9");
            Assert.That(f(10), Is.NaN, "Order-10 (bad)");

            Assert.That(Statistics.OrderStatistic(samples, 0), Is.NaN, "Order-0 (bad)");
            Assert.That(Statistics.OrderStatistic(samples, 1), Is.EqualTo(-3), "Order-1");
            Assert.That(Statistics.OrderStatistic(samples, 2), Is.EqualTo(-1), "Order-2");
            Assert.That(Statistics.OrderStatistic(samples, 3), Is.EqualTo(-0.5), "Order-3");
            Assert.That(Statistics.OrderStatistic(samples, 7), Is.EqualTo(5), "Order-7");
            Assert.That(Statistics.OrderStatistic(samples, 8), Is.EqualTo(6), "Order-8");
            Assert.That(Statistics.OrderStatistic(samples, 9), Is.EqualTo(10), "Order-9");
            Assert.That(Statistics.OrderStatistic(samples, 10), Is.NaN, "Order-10 (bad)");

            Assert.That(ArrayStatistics.OrderStatisticInplace(samples, 0), Is.NaN, "Order-0 (bad)");
            Assert.That(ArrayStatistics.OrderStatisticInplace(samples, 1), Is.EqualTo(-3), "Order-1");
            Assert.That(ArrayStatistics.OrderStatisticInplace(samples, 2), Is.EqualTo(-1), "Order-2");
            Assert.That(ArrayStatistics.OrderStatisticInplace(samples, 3), Is.EqualTo(-0.5), "Order-3");
            Assert.That(ArrayStatistics.OrderStatisticInplace(samples, 7), Is.EqualTo(5), "Order-7");
            Assert.That(ArrayStatistics.OrderStatisticInplace(samples, 8), Is.EqualTo(6), "Order-8");
            Assert.That(ArrayStatistics.OrderStatisticInplace(samples, 9), Is.EqualTo(10), "Order-9");
            Assert.That(ArrayStatistics.OrderStatisticInplace(samples, 10), Is.NaN, "Order-10 (bad)");

            Array.Sort(samples);
            Assert.That(SortedArrayStatistics.OrderStatistic(samples, 0), Is.NaN, "Order-0 (bad)");
            Assert.That(SortedArrayStatistics.OrderStatistic(samples, 1), Is.EqualTo(-3), "Order-1");
            Assert.That(SortedArrayStatistics.OrderStatistic(samples, 2), Is.EqualTo(-1), "Order-2");
            Assert.That(SortedArrayStatistics.OrderStatistic(samples, 3), Is.EqualTo(-0.5), "Order-3");
            Assert.That(SortedArrayStatistics.OrderStatistic(samples, 7), Is.EqualTo(5), "Order-7");
            Assert.That(SortedArrayStatistics.OrderStatistic(samples, 8), Is.EqualTo(6), "Order-8");
            Assert.That(SortedArrayStatistics.OrderStatistic(samples, 9), Is.EqualTo(10), "Order-9");
            Assert.That(SortedArrayStatistics.OrderStatistic(samples, 10), Is.NaN, "Order-10 (bad)");
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 1/5d)]
        [TestCase(0.2d, -1d)]
        [TestCase(0.7d, 4d)]
        [TestCase(0.01d, -3d)]
        [TestCase(0.99d, 10d)]
        [TestCase(0.52d, 1d)]
        [TestCase(0.325d, 0d)]
        public void QuantileR1InverseCDFOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=1)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{0,0},{1,0}}]

            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.InverseCDF(samples, tau), 1e-14);
            Assert.AreEqual(expected, Statistics.InverseCDFFunc(samples)(tau), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.InverseCDF), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.InverseCDF)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.InverseCDF), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 0d, 0d, 1d, 0d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.InverseCDF), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 0d, 0d, 1d, 0d), 1e-14);
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 3/5d)]
        [TestCase(0.2d, -3/4d)]
        [TestCase(0.7d, 9/2d)]
        [TestCase(0.01d, -3d)]
        [TestCase(0.99d, 10d)]
        [TestCase(0.52d, 1d)]
        [TestCase(0.325d, 0d)]
        public void QuantileR2InverseCDFAverageOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=2)
            // Mathematica: Not Supported

            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R2), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R2)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.InverseCDFAverage), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.InverseCDFAverage), 1e-14);
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 1/5d)]
        [TestCase(0.2d, -1d)]
        [TestCase(0.7d, 4d)]
        [TestCase(0.01d, -3d)]
        [TestCase(0.99d, 10d)]
        [TestCase(0.52d, 1/5d)]
        [TestCase(0.325d, -1/2d)]
        public void QuantileR3NearestOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=3)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{1/2,0},{0,0}}]

            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R3), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R3)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.Nearest), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 0.5d, 0d, 0d, 0d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.Nearest), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 0.5d, 0d, 0d, 0d), 1e-14);
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 1/5d)]
        [TestCase(0.2d, -1d)]
        [TestCase(0.7d, 4d)]
        [TestCase(0.01d, -3d)]
        [TestCase(0.99d, 48/5d)]
        [TestCase(0.52d, 9/25d)]
        [TestCase(0.325d, -3/8d)]
        public void QuantileR4CaliforniaOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=4)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{0,0},{0,1}}]

            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R4), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R4)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.California), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 0d, 0d, 0d, 1d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.California), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 0d, 0d, 0d, 1d), 1e-14);
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 3/5d)]
        [TestCase(0.2d, -3/4d)]
        [TestCase(0.7d, 9/2d)]
        [TestCase(0.01d, -3d)]
        [TestCase(0.99d, 10d)]
        [TestCase(0.52d, 19/25d)]
        [TestCase(0.325d, -1/8d)]
        public void QuantileR5HydrologyOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=5)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{1/2,0},{0,1}}]

            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R5), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R5)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.Hydrology), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 0.5d, 0d, 0d, 1d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.Hydrology), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 0.5d, 0d, 0d, 1d), 1e-14);
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 3/5d)]
        [TestCase(0.2d, -9/10d)]
        [TestCase(0.7d, 47/10d)]
        [TestCase(0.01d, -3d)]
        [TestCase(0.99d, 10d)]
        [TestCase(0.52d, 97/125d)]
        [TestCase(0.325d, -17/80d)]
        public void QuantileR6WeibullOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=6)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{0,1},{0,1}}]

            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R6), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R6)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.Weibull), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 0d, 1d, 0d, 1d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.Weibull), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 0d, 1d, 0d, 1d), 1e-14);
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 3/5d)]
        [TestCase(0.2d, -3/5d)]
        [TestCase(0.7d, 43/10d)]
        [TestCase(0.01d, -141/50d)]
        [TestCase(0.99d, 241/25d)]
        [TestCase(0.52d, 93/125d)]
        [TestCase(0.325d, -3/80d)]
        public void QuantileR7ExcelOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=7)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{1,-1},{0,1}}]

            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R7), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R7)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.Excel), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 1d, -1d, 0d, 1d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.Excel), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 1d, -1d, 0d, 1d), 1e-14);
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 3/5d)]
        [TestCase(0.2d, -4/5d)]
        [TestCase(0.7d, 137/30d)]
        [TestCase(0.01d, -3d)]
        [TestCase(0.99d, 10d)]
        [TestCase(0.52d, 287/375d)]
        [TestCase(0.325d, -37/240d)]
        public void QuantileR8MedianOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=8)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{1/3,1/3},{0,1}}]
            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.Quantile(samples, tau), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R8), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R8)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileInplace(samples, tau), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.Median), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 1 / 3d, 1 / 3d, 0d, 1d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.Quantile(samples, tau), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.Median), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 1/3d, 1/3d, 0d, 1d), 1e-14);
        }

        [TestCase(0d, -3d)]
        [TestCase(1d, 10d)]
        [TestCase(0.5d, 3/5d)]
        [TestCase(0.2d, -63/80d)]
        [TestCase(0.7d, 91/20d)]
        [TestCase(0.01d, -3d)]
        [TestCase(0.99d, 10d)]
        [TestCase(0.52d, 191/250d)]
        [TestCase(0.325d, -47/320d)]
        public void QuantileR9NormalOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=9)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{3/8,1/4},{0,1}}]
            var samples = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R9), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R9)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.Normal), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 3/8d, 1/4d, 0d, 1d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.Normal), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 3/8d, 1/4d, 0d, 1d), 1e-14);
        }

        [Test]
        public void MedianOnShortSequence()
        {
            // R: median(c(-1,5,0,-3,10,-0.5,4,0.2,1,6))
            // Mathematica: Median[{-1,5,0,-3,10,-1/2,4,1/5,1,6}]
            var even = new[] {-1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6};
            Assert.AreEqual(0.6d, Statistics.Median(even), 1e-14);
            Assert.AreEqual(0.6d, ArrayStatistics.MedianInplace(even), 1e-14);
            Array.Sort(even);
            Assert.AreEqual(0.6d, SortedArrayStatistics.Median(even), 1e-14);

            // R: median(c(-1,5,0,-3,10,-0.5,4,0.2,1))
            // Mathematica: Median[{-1,5,0,-3,10,-1/2,4,1/5,1}]
            var odd = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1 };
            Assert.AreEqual(0.2d, Statistics.Median(odd), 1e-14);
            Assert.AreEqual(0.2d, ArrayStatistics.MedianInplace(odd), 1e-14);
            Array.Sort(even);
            Assert.AreEqual(0.2d, SortedArrayStatistics.Median(odd), 1e-14);
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
            var gaussian = new Normal(1e+9, 2, new MersenneTwister(100));

            AssertHelpers.AlmostEqualRelative(1e+9, Statistics.Mean(gaussian.Samples().Take(10000)), 10);
            AssertHelpers.AlmostEqualRelative(4d, Statistics.Variance(gaussian.Samples().Take(10000)), 0);
            AssertHelpers.AlmostEqualRelative(2d, Statistics.StandardDeviation(gaussian.Samples().Take(10000)), 1);

            AssertHelpers.AlmostEqualRelative(1e+9, ArrayStatistics.Mean(gaussian.Samples().Take(10000).ToArray()), 10);
            AssertHelpers.AlmostEqualRelative(4d, ArrayStatistics.Variance(gaussian.Samples().Take(10000).ToArray()), 0);
            AssertHelpers.AlmostEqualRelative(2d, ArrayStatistics.StandardDeviation(gaussian.Samples().Take(10000).ToArray()), 1);

            AssertHelpers.AlmostEqualRelative(1e+9, StreamingStatistics.Mean(gaussian.Samples().Take(10000)), 10);
            AssertHelpers.AlmostEqualRelative(4d, StreamingStatistics.Variance(gaussian.Samples().Take(10000)), 0);
            AssertHelpers.AlmostEqualRelative(2d, StreamingStatistics.StandardDeviation(gaussian.Samples().Take(10000)), 1);
        }

        [TestCase("lottery")]
        [TestCase("lew")]
        [TestCase("mavro")]
        [TestCase("michelso")]
        [TestCase("numacc1")]
        public void CovarianceConsistentWithVariance(string dataSet)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqualRelative(Statistics.Variance(data.Data), Statistics.Covariance(data.Data, data.Data), 10);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.Variance(data.Data), ArrayStatistics.Covariance(data.Data, data.Data), 10);
            AssertHelpers.AlmostEqualRelative(StreamingStatistics.Variance(data.Data), StreamingStatistics.Covariance(data.Data, data.Data), 10);
        }

        [TestCase("lottery")]
        [TestCase("lew")]
        [TestCase("mavro")]
        [TestCase("michelso")]
        [TestCase("numacc1")]
        public void PopulationCovarianceConsistentWithPopulationVariance(string dataSet)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqualRelative(Statistics.PopulationVariance(data.Data), Statistics.PopulationCovariance(data.Data, data.Data), 10);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.PopulationVariance(data.Data), ArrayStatistics.PopulationCovariance(data.Data, data.Data), 10);
            AssertHelpers.AlmostEqualRelative(StreamingStatistics.PopulationVariance(data.Data), StreamingStatistics.PopulationCovariance(data.Data, data.Data), 10);
        }

        [Test]
        public void CovarianceIsSymmetric()
        {
            var dataA = _data["lottery"].Data.Take(200);
            var dataB = _data["lew"].Data.Take(200);

            AssertHelpers.AlmostEqualRelative(Statistics.Covariance(dataA, dataB), Statistics.Covariance(dataB, dataA), 12);
            AssertHelpers.AlmostEqualRelative(StreamingStatistics.Covariance(dataA, dataB), StreamingStatistics.Covariance(dataB, dataA), 12);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.Covariance(dataA.ToArray(), dataB.ToArray()), ArrayStatistics.Covariance(dataB.ToArray(), dataA.ToArray()), 12);

            AssertHelpers.AlmostEqualRelative(Statistics.PopulationCovariance(dataA, dataB), Statistics.PopulationCovariance(dataB, dataA), 12);
            AssertHelpers.AlmostEqualRelative(StreamingStatistics.PopulationCovariance(dataA, dataB), StreamingStatistics.PopulationCovariance(dataB, dataA), 12);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.PopulationCovariance(dataA.ToArray(), dataB.ToArray()), ArrayStatistics.PopulationCovariance(dataB.ToArray(), dataA.ToArray()), 12);
        }

        [TestCase("lottery")]
        [TestCase("lew")]
        [TestCase("mavro")]
        [TestCase("michelso")]
        [TestCase("numacc1")]
        public void ArrayStatisticsConsistentWithStreamimgStatistics(string dataSet)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.Mean(data.Data), StreamingStatistics.Mean(data.Data), 10);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.Variance(data.Data), StreamingStatistics.Variance(data.Data), 10);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.StandardDeviation(data.Data), StreamingStatistics.StandardDeviation(data.Data), 10);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.PopulationVariance(data.Data), StreamingStatistics.PopulationVariance(data.Data), 10);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.PopulationStandardDeviation(data.Data), StreamingStatistics.PopulationStandardDeviation(data.Data), 10);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.Covariance(data.Data, data.Data), StreamingStatistics.Covariance(data.Data, data.Data), 10);
            AssertHelpers.AlmostEqualRelative(ArrayStatistics.PopulationCovariance(data.Data, data.Data), StreamingStatistics.PopulationCovariance(data.Data, data.Data), 10);
        }

        [Test]
        public void MinimumOfEmptyMustBeNaN()
        {
            Assert.That(Statistics.Minimum(new double[0]), Is.NaN);
            Assert.That(Statistics.Minimum(new[] { 2d }), Is.Not.NaN);
            Assert.That(ArrayStatistics.Minimum(new double[0]), Is.NaN);
            Assert.That(ArrayStatistics.Minimum(new[] { 2d }), Is.Not.NaN);
            Assert.That(SortedArrayStatistics.Minimum(new double[0]), Is.NaN);
            Assert.That(SortedArrayStatistics.Minimum(new[] { 2d }), Is.Not.NaN);
            Assert.That(StreamingStatistics.Minimum(new double[0]), Is.NaN);
            Assert.That(StreamingStatistics.Minimum(new[] {2d }), Is.Not.NaN);
        }

        [Test]
        public void MaximumOfEmptyMustBeNaN()
        {
            Assert.That(Statistics.Maximum(new double[0]), Is.NaN);
            Assert.That(Statistics.Maximum(new[] { 2d }), Is.Not.NaN);
            Assert.That(ArrayStatistics.Maximum(new double[0]), Is.NaN);
            Assert.That(ArrayStatistics.Maximum(new[] { 2d }), Is.Not.NaN);
            Assert.That(SortedArrayStatistics.Maximum(new double[0]), Is.NaN);
            Assert.That(SortedArrayStatistics.Maximum(new[] { 2d }), Is.Not.NaN);
            Assert.That(StreamingStatistics.Maximum(new double[0]), Is.NaN);
            Assert.That(StreamingStatistics.Maximum(new[] { 2d }), Is.Not.NaN);
        }

        [Test]
        public void MeanOfEmptyMustBeNaN()
        {
            Assert.That(Statistics.Mean(new double[0]), Is.NaN);
            Assert.That(Statistics.Mean(new[] { 2d }), Is.Not.NaN);
            Assert.That(ArrayStatistics.Mean(new double[0]), Is.NaN);
            Assert.That(ArrayStatistics.Mean(new[] { 2d }), Is.Not.NaN);
            Assert.That(StreamingStatistics.Mean(new double[0]), Is.NaN);
            Assert.That(StreamingStatistics.Mean(new[] { 2d }), Is.Not.NaN);
        }

        [Test]
        public void SampleVarianceOfEmptyAndSingleMustBeNaN()
        {
            Assert.That(Statistics.Variance(new double[0]), Is.NaN);
            Assert.That(Statistics.Variance(new[] { 2d }), Is.NaN);
            Assert.That(Statistics.Variance(new[] { 2d, 3d }), Is.Not.NaN);
            Assert.That(ArrayStatistics.Variance(new double[0]), Is.NaN);
            Assert.That(ArrayStatistics.Variance(new[] { 2d }), Is.NaN);
            Assert.That(ArrayStatistics.Variance(new[] { 2d, 3d }), Is.Not.NaN);
            Assert.That(StreamingStatistics.Variance(new double[0]), Is.NaN);
            Assert.That(StreamingStatistics.Variance(new[] { 2d }), Is.NaN);
            Assert.That(StreamingStatistics.Variance(new[] { 2d, 3d }), Is.Not.NaN);
        }

        [Test]
        public void PopulationVarianceOfEmptyMustBeNaN()
        {
            Assert.That(Statistics.PopulationVariance(new double[0]), Is.NaN);
            Assert.That(Statistics.PopulationVariance(new[] { 2d }), Is.Not.NaN);
            Assert.That(Statistics.PopulationVariance(new[] { 2d, 3d }), Is.Not.NaN);
            Assert.That(ArrayStatistics.PopulationVariance(new double[0]), Is.NaN);
            Assert.That(ArrayStatistics.PopulationVariance(new[] { 2d }), Is.Not.NaN);
            Assert.That(ArrayStatistics.PopulationVariance(new[] { 2d, 3d }), Is.Not.NaN);
            Assert.That(StreamingStatistics.PopulationVariance(new double[0]), Is.NaN);
            Assert.That(StreamingStatistics.PopulationVariance(new[] { 2d }), Is.Not.NaN);
            Assert.That(StreamingStatistics.PopulationVariance(new[] { 2d, 3d }), Is.Not.NaN);
        }

        /// <summary>
        /// URL http://mathnetnumerics.codeplex.com/workitem/5667
        /// </summary>
        [Test]
        public void Median_CodeplexIssue5667()
        {
            var seq = File.ReadAllLines("./data/Codeplex-5667.csv").Select(double.Parse);
            Assert.AreEqual(1.0, Statistics.Median(seq));

            var array = seq.ToArray();
            Assert.AreEqual(1.0, ArrayStatistics.MedianInplace(array));

            Array.Sort(array);
            Assert.AreEqual(1.0, SortedArrayStatistics.Median(array));
        }

        [Test]
        public void VarianceDenominatorMustNotOverflow_GitHubIssue137()
        {
            var a = new double[46342];
            a[a.Length - 1] = 1000d;

            Assert.AreEqual(21.578697, a.Variance(), 1e-5);
            Assert.AreEqual(21.578231, a.PopulationVariance(), 1e-5);
        }
    }
}
