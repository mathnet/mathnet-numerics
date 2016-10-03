// <copyright file="StatisticsTests.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using MathNet.Numerics.TestData;
using NUnit.Framework;

// ReSharper disable InvokeAsExtensionMethod

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    using Statistics;

    /// <summary>
    /// Statistics Tests
    /// </summary>
    [TestFixture, Category("Statistics")]
    public class StatisticsTests
    {
        readonly IDictionary<string, StatTestData> _data = new Dictionary<string, StatTestData>
        {
            { "lottery", new StatTestData("NIST.Lottery.dat") },
            { "lew", new StatTestData("NIST.Lew.dat") },
            { "mavro", new StatTestData("NIST.Mavro.dat") },
            { "michelso", new StatTestData("NIST.Michelso.dat") },
            { "numacc1", new StatTestData("NIST.NumAcc1.dat") },
            { "numacc2", new StatTestData("NIST.NumAcc2.dat") },
            { "numacc3", new StatTestData("NIST.NumAcc3.dat") },
            { "numacc4", new StatTestData("NIST.NumAcc4.dat") },
            { "meixner", new StatTestData("NIST.Meixner.dat") }
        };

        [Test]
        public void ThrowsOnNullData()
        {
            double[] data = null;

// ReSharper disable ExpressionIsAlwaysNull
            Assert.That(() => Statistics.Minimum(data), Throws.Exception);
            Assert.That(() => Statistics.Maximum(data), Throws.Exception);
            Assert.That(() => Statistics.Mean(data), Throws.Exception);
            Assert.That(() => Statistics.HarmonicMean(data), Throws.Exception);
            Assert.That(() => Statistics.GeometricMean(data), Throws.Exception);
            Assert.That(() => Statistics.Median(data), Throws.Exception);
            Assert.That(() => Statistics.Quantile(data, 0.3), Throws.Exception);
            Assert.That(() => Statistics.Variance(data), Throws.Exception);
            Assert.That(() => Statistics.StandardDeviation(data), Throws.Exception);
            Assert.That(() => Statistics.PopulationVariance(data), Throws.Exception);
            Assert.That(() => Statistics.PopulationStandardDeviation(data), Throws.Exception);
            Assert.That(() => Statistics.Covariance(data, data), Throws.Exception);
            Assert.That(() => Statistics.PopulationCovariance(data, data), Throws.Exception);
            Assert.That(() => Statistics.RootMeanSquare(data), Throws.Exception);

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
            Assert.That(() => ArrayStatistics.HarmonicMean(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.GeometricMean(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.Variance(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.StandardDeviation(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.PopulationVariance(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.PopulationStandardDeviation(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.Covariance(data, data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.PopulationCovariance(data, data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.RootMeanSquare(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.MedianInplace(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => ArrayStatistics.QuantileInplace(data, 0.3), Throws.Exception.TypeOf<NullReferenceException>());

            Assert.That(() => StreamingStatistics.Minimum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Maximum(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Mean(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.HarmonicMean(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.GeometricMean(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Variance(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.StandardDeviation(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.PopulationVariance(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.PopulationStandardDeviation(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Covariance(data, data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.PopulationCovariance(data, data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.RootMeanSquare(data), Throws.Exception.TypeOf<NullReferenceException>());
            Assert.That(() => StreamingStatistics.Entropy(data), Throws.Exception.TypeOf<NullReferenceException>());

            Assert.That(() => new RunningStatistics(data), Throws.Exception);
            Assert.That(() => new RunningStatistics().PushRange(data), Throws.Exception);
// ReSharper restore ExpressionIsAlwaysNull
        }

        [Test]
        public void DoesNotThrowOnEmptyData()
        {
            double[] data = new double[0];

            Assert.DoesNotThrow(() => Statistics.Minimum(data));
            Assert.DoesNotThrow(() => Statistics.Maximum(data));
            Assert.DoesNotThrow(() => Statistics.Mean(data));
            Assert.DoesNotThrow(() => Statistics.HarmonicMean(data));
            Assert.DoesNotThrow(() => Statistics.GeometricMean(data));
            Assert.DoesNotThrow(() => Statistics.Median(data));
            Assert.DoesNotThrow(() => Statistics.Quantile(data, 0.3));
            Assert.DoesNotThrow(() => Statistics.Variance(data));
            Assert.DoesNotThrow(() => Statistics.StandardDeviation(data));
            Assert.DoesNotThrow(() => Statistics.PopulationVariance(data));
            Assert.DoesNotThrow(() => Statistics.PopulationStandardDeviation(data));
            Assert.DoesNotThrow(() => Statistics.Covariance(data, data));
            Assert.DoesNotThrow(() => Statistics.PopulationCovariance(data, data));
            Assert.DoesNotThrow(() => Statistics.RootMeanSquare(data));

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
            Assert.DoesNotThrow(() => ArrayStatistics.RootMeanSquare(data));
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
            Assert.DoesNotThrow(() => StreamingStatistics.RootMeanSquare(data));
            Assert.DoesNotThrow(() => StreamingStatistics.Entropy(data));

            Assert.That(() => new RunningStatistics(data), Throws.Nothing);
            Assert.That(() => new RunningStatistics().PushRange(data), Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).Minimum, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).Maximum, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).Mean, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).Variance, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).StandardDeviation, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).Skewness, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).Kurtosis, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).PopulationVariance, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).PopulationStandardDeviation, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).PopulationSkewness, Throws.Nothing);
            Assert.That(() => new RunningStatistics(data).PopulationKurtosis, Throws.Nothing);
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
            AssertHelpers.AlmostEqualRelative(data.Mean, new RunningStatistics(data.Data).Mean, 14);
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
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, new RunningStatistics(data.Data).StandardDeviation, digits);
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
            var samples = new[] { -1.0, 5, 0, -3, 10, -0.5, 4 };
            Assert.That(Statistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(Statistics.Maximum(samples), Is.EqualTo(10), "Max");
            Assert.That(ArrayStatistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(ArrayStatistics.Maximum(samples), Is.EqualTo(10), "Max");
            Assert.That(StreamingStatistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(StreamingStatistics.Maximum(samples), Is.EqualTo(10), "Max");
            Assert.That(new RunningStatistics(samples).Minimum, Is.EqualTo(-3), "Min");
            Assert.That(new RunningStatistics(samples).Maximum, Is.EqualTo(10), "Max");

            Array.Sort(samples);
            Assert.That(SortedArrayStatistics.Minimum(samples), Is.EqualTo(-3), "Min");
            Assert.That(SortedArrayStatistics.Maximum(samples), Is.EqualTo(10), "Max");
        }

        [Test]
        public void MinimumMaximumOnShortSequence32()
        {
            var samples = new[] { -1.0f, 5f, 0f, -3f, 10f, -0.5f, 4f };
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
        public void QuantileR1EmpiricalInvCDFOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=1)
            // Mathematica: Quantile[{-1,5,0,-3,10,-1/2,4,1/5,1,6},{0,1,1/2,1/5,7/10,1/100,99/100,13/25,13/40},{{0,0},{1,0}}]

            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

            Assert.AreEqual(expected, Statistics.EmpiricalInvCDF(samples, tau), 1e-14);
            Assert.AreEqual(expected, Statistics.EmpiricalInvCDFFunc(samples)(tau), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.EmpiricalInvCDF), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.EmpiricalInvCDF)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.EmpiricalInvCDF), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 0d, 0d, 1d, 0d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.EmpiricalInvCDF), 1e-14);
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
        public void QuantileR2EmpiricalInvCDFAverageOnShortSequence(double tau, double expected)
        {
            // R: quantile(c(-1,5,0,-3,10,-0.5,4,0.2,1,6),probs=c(0,1,0.5,0.2,0.7,0.01,0.99,0.52,0.325),type=2)
            // Mathematica: Not Supported

            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R2), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R2)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.EmpiricalInvCDFAverage), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.EmpiricalInvCDFAverage), 1e-14);
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

            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

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

            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

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

            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

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

            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

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

            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

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
            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

            Assert.AreEqual(expected, Statistics.Quantile(samples, tau), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R8), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R8)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileInplace(samples, tau), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.Median), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 1/3d, 1/3d, 0d, 1d), 1e-14);

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
            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };

            Assert.AreEqual(expected, Statistics.QuantileCustom(samples, tau, QuantileDefinition.R9), 1e-14);
            Assert.AreEqual(expected, Statistics.QuantileCustomFunc(samples, QuantileDefinition.R9)(tau), 1e-14);

            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, QuantileDefinition.Normal), 1e-14);
            Assert.AreEqual(expected, ArrayStatistics.QuantileCustomInplace(samples, tau, 3/8d, 1/4d, 0d, 1d), 1e-14);

            Array.Sort(samples);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, QuantileDefinition.Normal), 1e-14);
            Assert.AreEqual(expected, SortedArrayStatistics.QuantileCustom(samples, tau, 3/8d, 1/4d, 0d, 1d), 1e-14);
        }

        [Test]
        public void RanksSortedArray()
        {
            var distinct = new double[] { 1, 2, 4, 7, 8, 9, 10, 12 };
            var ties = new double[] { 1, 2, 2, 7, 9, 9, 10, 12 };

            // R: rank(sort(data), ties.method="average")
            Assert.That(
                SortedArrayStatistics.Ranks(distinct, RankDefinition.Average),
                Is.EqualTo(new[] { 1.0, 2, 3, 4, 5, 6, 7, 8 }).AsCollection.Within(1e-8));
            Assert.That(
                SortedArrayStatistics.Ranks(ties, RankDefinition.Average),
                Is.EqualTo(new[] { 1, 2.5, 2.5, 4, 5.5, 5.5, 7, 8 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="min")
            Assert.That(
                SortedArrayStatistics.Ranks(distinct, RankDefinition.Min),
                Is.EqualTo(new[] { 1.0, 2, 3, 4, 5, 6, 7, 8 }).AsCollection.Within(1e-8));
            Assert.That(
                SortedArrayStatistics.Ranks(ties, RankDefinition.Min),
                Is.EqualTo(new[] { 1.0, 2, 2, 4, 5, 5, 7, 8 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="max")
            Assert.That(
                SortedArrayStatistics.Ranks(distinct, RankDefinition.Max),
                Is.EqualTo(new[] { 1.0, 2, 3, 4, 5, 6, 7, 8 }).AsCollection.Within(1e-8));
            Assert.That(
                SortedArrayStatistics.Ranks(ties, RankDefinition.Max),
                Is.EqualTo(new[] { 1.0, 3, 3, 4, 6, 6, 7, 8 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="first")
            Assert.That(
                SortedArrayStatistics.Ranks(distinct, RankDefinition.First),
                Is.EqualTo(new[] { 1.0, 2, 3, 4, 5, 6, 7, 8 }).AsCollection.Within(1e-8));
            Assert.That(
                SortedArrayStatistics.Ranks(ties, RankDefinition.First),
                Is.EqualTo(new[] { 1.0, 2, 3, 4, 5, 6, 7, 8 }).AsCollection.Within(1e-8));
        }

        [Test]
        public void RanksArray()
        {
            var distinct = new double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            var ties = new double[] { 1, 9, 12, 7, 2, 9, 10, 2 };

            // R: rank(data, ties.method="average")
            Assert.That(
                ArrayStatistics.RanksInplace((double[])distinct.Clone(), RankDefinition.Average),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
            Assert.That(
                ArrayStatistics.RanksInplace((double[])ties.Clone(), RankDefinition.Average),
                Is.EqualTo(new[] { 1, 5.5, 8, 4, 2.5, 5.5, 7, 2.5 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="min")
            Assert.That(
                ArrayStatistics.RanksInplace((double[])distinct.Clone(), RankDefinition.Min),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
            Assert.That(
                ArrayStatistics.RanksInplace((double[])ties.Clone(), RankDefinition.Min),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 5, 7, 2 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="max")
            Assert.That(
                ArrayStatistics.RanksInplace((double[])distinct.Clone(), RankDefinition.Max),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
            Assert.That(
                ArrayStatistics.RanksInplace((double[])ties.Clone(), RankDefinition.Max),
                Is.EqualTo(new[] { 1.0, 6, 8, 4, 3, 6, 7, 3 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="first")
            Assert.That(
                ArrayStatistics.RanksInplace((double[])distinct.Clone(), RankDefinition.First),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
            Assert.That(
                ArrayStatistics.RanksInplace((double[])ties.Clone(), RankDefinition.First),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
        }

        [Test]
        public void Ranks()
        {
            var distinct = new double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            var ties = new double[] { 1, 9, 12, 7, 2, 9, 10, 2 };

            // R: rank(data, ties.method="average")
            Assert.That(
                Statistics.Ranks(distinct, RankDefinition.Average),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
            Assert.That(
                Statistics.Ranks(ties, RankDefinition.Average),
                Is.EqualTo(new[] { 1, 5.5, 8, 4, 2.5, 5.5, 7, 2.5 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="min")
            Assert.That(
                Statistics.Ranks(distinct, RankDefinition.Min),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
            Assert.That(
                Statistics.Ranks(ties, RankDefinition.Min),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 5, 7, 2 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="max")
            Assert.That(
                Statistics.Ranks(distinct, RankDefinition.Max),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
            Assert.That(
                Statistics.Ranks(ties, RankDefinition.Max),
                Is.EqualTo(new[] { 1.0, 6, 8, 4, 3, 6, 7, 3 }).AsCollection.Within(1e-8));

            // R: rank(data, ties.method="first")
            Assert.That(
                Statistics.Ranks(distinct, RankDefinition.First),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
            Assert.That(
                Statistics.Ranks(ties, RankDefinition.First),
                Is.EqualTo(new[] { 1.0, 5, 8, 4, 2, 6, 7, 3 }).AsCollection.Within(1e-8));
        }

        [Test]
        public void EmpiricalCDF()
        {
            // R: ecdf(data)(x)
            var ties = new double[] { 1, 9, 12, 7, 2, 9, 10, 2 };

            Assert.That(Statistics.EmpiricalCDF(ties, -1.0), Is.EqualTo(0.0).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 0.0), Is.EqualTo(0.0).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 1.0), Is.EqualTo(0.125).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 2.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 3.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 4.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 5.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 6.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 7.0), Is.EqualTo(0.5).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 8.0), Is.EqualTo(0.5).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 9.0), Is.EqualTo(0.75).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 10.0), Is.EqualTo(0.875).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 11.0), Is.EqualTo(0.875).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 12.0), Is.EqualTo(1.0).Within(1e-8));
            Assert.That(Statistics.EmpiricalCDF(ties, 13.0), Is.EqualTo(1.0).Within(1e-8));
        }

        [Test]
        public void EmpiricalCDFSortedArray()
        {
            // R: ecdf(data)(x)
            var ties = new double[] { 1, 9, 12, 7, 2, 9, 10, 2 };
            Array.Sort(ties);

            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, -1.0), Is.EqualTo(0.0).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 0.0), Is.EqualTo(0.0).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 1.0), Is.EqualTo(0.125).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 2.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 3.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 4.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 5.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 6.0), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 7.0), Is.EqualTo(0.5).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 8.0), Is.EqualTo(0.5).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 9.0), Is.EqualTo(0.75).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 10.0), Is.EqualTo(0.875).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 11.0), Is.EqualTo(0.875).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 12.0), Is.EqualTo(1.0).Within(1e-8));
            Assert.That(SortedArrayStatistics.EmpiricalCDF(ties, 13.0), Is.EqualTo(1.0).Within(1e-8));

            Assert.That(SortedArrayStatistics.QuantileRank(ties, -1.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.0).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 0.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.0).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 1.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.125).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 2.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 3.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 4.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 5.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 6.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.375).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 7.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.5).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 8.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.5).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 9.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.75).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 10.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.875).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 11.0, RankDefinition.EmpiricalCDF), Is.EqualTo(0.875).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 12.0, RankDefinition.EmpiricalCDF), Is.EqualTo(1.0).Within(1e-8));
            Assert.That(SortedArrayStatistics.QuantileRank(ties, 13.0, RankDefinition.EmpiricalCDF), Is.EqualTo(1.0).Within(1e-8));
        }

        [Test]
        public void MedianOnShortSequence()
        {
            // R: median(c(-1,5,0,-3,10,-0.5,4,0.2,1,6))
            // Mathematica: Median[{-1,5,0,-3,10,-1/2,4,1/5,1,6}]
            var even = new[] { -1, 5, 0, -3, 10, -0.5, 4, 0.2, 1, 6 };
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

        [Test]
        public void MedianOnLongConstantSequence()
        {
            var even = Generate.Repeat(100000, 2.0);
            Assert.AreEqual(2.0,SortedArrayStatistics.Median(even), 1e-14);

            var odd = Generate.Repeat(100001, 2.0);
            Assert.AreEqual(2.0, SortedArrayStatistics.Median(odd), 1e-14);
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
            AssertHelpers.AlmostEqualRelative(1e+9, Statistics.RootMeanSquare(gaussian.Samples().Take(10000)), 10);

            AssertHelpers.AlmostEqualRelative(1e+9, ArrayStatistics.Mean(gaussian.Samples().Take(10000).ToArray()), 10);
            AssertHelpers.AlmostEqualRelative(4d, ArrayStatistics.Variance(gaussian.Samples().Take(10000).ToArray()), 0);
            AssertHelpers.AlmostEqualRelative(2d, ArrayStatistics.StandardDeviation(gaussian.Samples().Take(10000).ToArray()), 1);
            AssertHelpers.AlmostEqualRelative(1e+9, ArrayStatistics.RootMeanSquare(gaussian.Samples().Take(10000).ToArray()), 10);

            AssertHelpers.AlmostEqualRelative(1e+9, StreamingStatistics.Mean(gaussian.Samples().Take(10000)), 10);
            AssertHelpers.AlmostEqualRelative(4d, StreamingStatistics.Variance(gaussian.Samples().Take(10000)), 0);
            AssertHelpers.AlmostEqualRelative(2d, StreamingStatistics.StandardDeviation(gaussian.Samples().Take(10000)), 1);
            AssertHelpers.AlmostEqualRelative(1e+9, StreamingStatistics.RootMeanSquare(gaussian.Samples().Take(10000)), 10);

            AssertHelpers.AlmostEqualRelative(1e+9, new RunningStatistics(gaussian.Samples().Take(10000)).Mean, 10);
            AssertHelpers.AlmostEqualRelative(4d, new RunningStatistics(gaussian.Samples().Take(10000)).Variance, 0);
            AssertHelpers.AlmostEqualRelative(2d, new RunningStatistics(gaussian.Samples().Take(10000)).StandardDeviation, 1);
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
            var dataA = _data["lottery"].Data.Take(200).ToArray();
            var dataB = _data["lew"].Data.Take(200).ToArray();

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
        [TestCase("numacc2")]
        [TestCase("meixner")]
        public void ArrayStatisticsConsistentWithStreamimgStatistics(string dataSet)
        {
            var data = _data[dataSet];
            Assert.That(ArrayStatistics.Mean(data.Data), Is.EqualTo(StreamingStatistics.Mean(data.Data)).Within(1e-15), "Mean");
            Assert.That(ArrayStatistics.Variance(data.Data), Is.EqualTo(StreamingStatistics.Variance(data.Data)).Within(1e-15), "Variance");
            Assert.That(ArrayStatistics.StandardDeviation(data.Data), Is.EqualTo(StreamingStatistics.StandardDeviation(data.Data)).Within(1e-15), "StandardDeviation");
            Assert.That(ArrayStatistics.PopulationVariance(data.Data), Is.EqualTo(StreamingStatistics.PopulationVariance(data.Data)).Within(1e-15), "PopulationVariance");
            Assert.That(ArrayStatistics.PopulationStandardDeviation(data.Data), Is.EqualTo(StreamingStatistics.PopulationStandardDeviation(data.Data)).Within(1e-15), "PopulationStandardDeviation");
            Assert.That(ArrayStatistics.Covariance(data.Data, data.Data), Is.EqualTo(StreamingStatistics.Covariance(data.Data, data.Data)).Within(1e-10), "Covariance");
            Assert.That(ArrayStatistics.PopulationCovariance(data.Data, data.Data), Is.EqualTo(StreamingStatistics.PopulationCovariance(data.Data, data.Data)).Within(1e-10), "PopulationCovariance");
            Assert.That(ArrayStatistics.RootMeanSquare(data.Data), Is.EqualTo(StreamingStatistics.RootMeanSquare(data.Data)).Within(1e-15), "RootMeanSquare");
        }

        [TestCase("lottery")]
        [TestCase("lew")]
        [TestCase("mavro")]
        [TestCase("michelso")]
        [TestCase("numacc1")]
        [TestCase("numacc2")]
        [TestCase("meixner")]
        public void RunningStatisticsConsistentWithDescriptiveStatistics(string dataSet)
        {
            var data = _data[dataSet];
            var running = new RunningStatistics(data.Data);
            var descriptive = new DescriptiveStatistics(data.Data);
            Assert.That(running.Minimum, Is.EqualTo(descriptive.Minimum), "Minimum");
            Assert.That(running.Maximum, Is.EqualTo(descriptive.Maximum), "Maximum");
            Assert.That(running.Mean, Is.EqualTo(descriptive.Mean).Within(1e-15), "Mean");
            Assert.That(running.Variance, Is.EqualTo(descriptive.Variance).Within(1e-15), "Variance");
            Assert.That(running.StandardDeviation, Is.EqualTo(descriptive.StandardDeviation).Within(1e-15), "StandardDeviation");
            Assert.That(running.Skewness, Is.EqualTo(descriptive.Skewness).Within(1e-15), "Skewness");
            Assert.That(running.Kurtosis, Is.EqualTo(descriptive.Kurtosis).Within(1e-14), "Kurtosis");
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
            Assert.That(StreamingStatistics.Minimum(new[] { 2d }), Is.Not.NaN);
            Assert.That(new RunningStatistics(new double[0]).Minimum, Is.NaN);
            Assert.That(new RunningStatistics(new[] { 2d }).Minimum, Is.Not.NaN);
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
            Assert.That(new RunningStatistics(new double[0]).Maximum, Is.NaN);
            Assert.That(new RunningStatistics(new[] { 2d }).Maximum, Is.Not.NaN);
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
            Assert.That(new RunningStatistics(new double[0]).Mean, Is.NaN);
            Assert.That(new RunningStatistics(new[] { 2d }).Mean, Is.Not.NaN);
        }

        [Test]
        public void RootMeanSquareOfEmptyMustBeNaN()
        {
            Assert.That(Statistics.RootMeanSquare(new double[0]), Is.NaN);
            Assert.That(Statistics.RootMeanSquare(new[] { 2d }), Is.Not.NaN);
            Assert.That(ArrayStatistics.RootMeanSquare(new double[0]), Is.NaN);
            Assert.That(ArrayStatistics.RootMeanSquare(new[] { 2d }), Is.Not.NaN);
            Assert.That(StreamingStatistics.RootMeanSquare(new double[0]), Is.NaN);
            Assert.That(StreamingStatistics.RootMeanSquare(new[] { 2d }), Is.Not.NaN);
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
            Assert.That(new RunningStatistics(new[] { 2d }).Variance, Is.NaN);
            Assert.That(new RunningStatistics(new[] { 2d, 3d }).Variance, Is.Not.NaN);
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
            Assert.That(new RunningStatistics(new[] { 2d }).PopulationVariance, Is.NaN);
            Assert.That(new RunningStatistics(new[] { 2d, 3d }).PopulationVariance, Is.Not.NaN);
        }

        /// <summary>
        /// URL http://mathnetnumerics.codeplex.com/workitem/5667
        /// </summary>
        [Test]
        public void Median_CodeplexIssue5667()
        {
            var seq = Data.ReadAllLines("Codeplex-5667.csv").Select(double.Parse);
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

            Assert.AreEqual(21.578697, new RunningStatistics(a).Variance, 1e-5);
            Assert.AreEqual(21.578231, new RunningStatistics(a).PopulationVariance, 1e-5);
        }

        [Test]
        public void MedianIsRobustOnCloseInfinities()
        {
            Assert.That(Statistics.Median(new[] { 2.0, double.NegativeInfinity, double.PositiveInfinity }), Is.EqualTo(2.0));
            Assert.That(Statistics.Median(new[] { 2.0, double.NegativeInfinity, 3.0, double.PositiveInfinity }), Is.EqualTo(2.5));
            Assert.That(ArrayStatistics.MedianInplace(new[] { 2.0, double.NegativeInfinity, double.PositiveInfinity }), Is.EqualTo(2.0));
            Assert.That(ArrayStatistics.MedianInplace(new[] { double.NegativeInfinity, 2.0, double.PositiveInfinity }), Is.EqualTo(2.0));
            Assert.That(ArrayStatistics.MedianInplace(new[] { double.NegativeInfinity, double.PositiveInfinity, 2.0 }), Is.EqualTo(2.0));
            Assert.That(ArrayStatistics.MedianInplace(new[] { double.NegativeInfinity, 2.0, 3.0, double.PositiveInfinity }), Is.EqualTo(2.5));
            Assert.That(ArrayStatistics.MedianInplace(new[] { double.NegativeInfinity, 2.0, double.PositiveInfinity, 3.0 }), Is.EqualTo(2.5));
            Assert.That(SortedArrayStatistics.Median(new[] { double.NegativeInfinity, 2.0, double.PositiveInfinity }), Is.EqualTo(2.0));
            Assert.That(SortedArrayStatistics.Median(new[] { double.NegativeInfinity, 2.0, 3.0, double.PositiveInfinity }), Is.EqualTo(2.5));
        }

        [Test]
        public void RobustOnLargeSampleSets()
        {
            // 0, 0.25, 0.5, 0.75, 0, 0.25, 0.5, 0.75, ...
            var shorter = Generate.Periodic(4*4096, 4, 1);
            var longer = Generate.Periodic(4*32768, 4, 1);

            Assert.That(Statistics.Mean(shorter), Is.EqualTo(0.375).Within(1e-14), "Statistics.Mean: shorter");
            Assert.That(Statistics.Mean(longer), Is.EqualTo(0.375).Within(1e-14), "Statistics.Mean: longer");
            Assert.That(new DescriptiveStatistics(shorter).Mean, Is.EqualTo(0.375).Within(1e-14), "DescriptiveStatistics.Mean: shorter");
            Assert.That(new DescriptiveStatistics(longer).Mean, Is.EqualTo(00.375).Within(1e-14), "DescriptiveStatistics.Mean: longer");

            Assert.That(Statistics.RootMeanSquare(shorter), Is.EqualTo(Math.Sqrt(0.21875)).Within(1e-14), "Statistics.RootMeanSquare: shorter");
            Assert.That(Statistics.RootMeanSquare(longer), Is.EqualTo(Math.Sqrt(0.21875)).Within(1e-14), "Statistics.RootMeanSquare: longer");

            Assert.That(Statistics.Skewness(shorter), Is.EqualTo(0.0).Within(1e-12), "Statistics.Skewness: shorter");
            Assert.That(Statistics.Skewness(longer), Is.EqualTo(0.0).Within(1e-12), "Statistics.Skewness: longer");
            Assert.That(new DescriptiveStatistics(shorter).Skewness, Is.EqualTo(0.0).Within(1e-12), "DescriptiveStatistics.Skewness: shorter");
            Assert.That(new DescriptiveStatistics(longer).Skewness, Is.EqualTo(0.0).Within(1e-12), "DescriptiveStatistics.Skewness: longer");

            Assert.That(Statistics.Kurtosis(shorter), Is.EqualTo(-1.36).Within(1e-4), "Statistics.Kurtosis: shorter");
            Assert.That(Statistics.Kurtosis(longer), Is.EqualTo(-1.36).Within(1e-4), "Statistics.Kurtosis: longer");
            Assert.That(new DescriptiveStatistics(shorter).Kurtosis, Is.EqualTo(-1.36).Within(1e-4), "DescriptiveStatistics.Kurtosis: shorter");
            Assert.That(new DescriptiveStatistics(longer).Kurtosis, Is.EqualTo(-1.36).Within(1e-4), "DescriptiveStatistics.Kurtosis: longer");
        }

        [Test]
        public void RootMeanSquareOfSinusoidal()
        {
            var data = Generate.Sinusoidal(128, 64, 16, 2.0);
            Assert.That(Statistics.RootMeanSquare(data), Is.EqualTo(2.0/Constants.Sqrt2).Within(1e-12));
        }

        [Test]
        public void EntropyIsMinimum()
        {
            var data1 = new double[] { 1, 1, 1, 1, 1 };
            Assert.That(StreamingStatistics.Entropy(data1) == 0);

            var data2 = new double[] { 0, 0 };
            Assert.That(StreamingStatistics.Entropy(data2) == 0);
        }

        [Test]
        public void EntropyIsMaximum()
        {
            var data1 = new double[] { 1, 2  };
            Assert.That(StreamingStatistics.Entropy(data1) == 1.0);

            var data2 = new double[] { 1, 2, 3, 4 };
            Assert.That(StreamingStatistics.Entropy(data2) == 2.0);
        }

        [Test]
        public void EntropyOfNaNIsNaN()
        {
            var data = new double[] { 1, 2, double.NaN };
            Assert.That(double.IsNaN(StreamingStatistics.Entropy(data)));
        }

        [Test]
        public void MinimumMagnitudePhase()
        {
            var a = new[] { new Complex32(1.0f, 2.0f), new Complex32(float.PositiveInfinity, float.NegativeInfinity), new Complex32(-2.0f, 4.0f) };
            Assert.That(ArrayStatistics.MinimumMagnitudePhase(a), Is.EqualTo(a[0]));
        }

        [Test]
        public void MinimumMagnitudePhaseOfNaNIsNaN()
        {
            var a = new[] { new Complex32(1.0f, 2.0f), new Complex32(float.NaN, float.NegativeInfinity), new Complex32(-2.0f, 4.0f) };
            Assert.That(ArrayStatistics.MinimumMagnitudePhase(a).IsNaN(), Is.True);
        }

        [Test]
        public void MaximumMagnitudePhase()
        {
            var a = new[] { new Complex32(1.0f, 2.0f), new Complex32(float.PositiveInfinity, float.NegativeInfinity), new Complex32(-2.0f, 4.0f) };
            Assert.That(ArrayStatistics.MaximumMagnitudePhase(a), Is.EqualTo(a[1]));
        }

        [Test]
        public void MaximumMagnitudePhaseOfNaNIsNaN()
        {
            var a = new[] { new Complex32(1.0f, 2.0f), new Complex32(float.NaN, float.NegativeInfinity), new Complex32(-2.0f, 4.0f) };
            Assert.That(ArrayStatistics.MaximumMagnitudePhase(a).IsNaN(), Is.True);
        }
    }
}

// ReSharper restore InvokeAsExtensionMethod
