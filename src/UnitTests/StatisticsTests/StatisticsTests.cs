// <copyright file="StatisticsTests.cs" company="Math.NET">
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
#if !PORTABLE
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Statistics tests.
    /// </summary>
    /// <remarks>NOTE: this class is not included into Silverlight version, because it uses data from local files. 
    /// In Silverlight access to local files is forbidden, except several cases.</remarks>
    [TestFixture]
    public class StatisticsTests
    {
        /// <summary>
        /// Statistics data.
        /// </summary>
        private readonly IDictionary<string, StatTestData> _data = new Dictionary<string, StatTestData>();

        /// <summary>
        /// Initializes a new instance of the StatisticsTests class.
        /// </summary>
        public StatisticsTests()
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
        /// Validate mean.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        [TestCase("lottery")]
        [TestCase("lew")]
        [TestCase("mavro")]
        [TestCase("michelso")]
        [TestCase("numacc1")]
        [TestCase("numacc2")]
        [TestCase("numacc3")]
        [TestCase("numacc4")]
        public void Mean(string dataSet)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqual(data.Mean, data.Data.Mean(), 15);
        }

        /// <summary>
        /// <c>Nullable</c> mean.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        [TestCase("lottery")]
        [TestCase("lew")]
        [TestCase("mavro")]
        [TestCase("michelso")]
        [TestCase("numacc1")]
        [TestCase("numacc2")]
        [TestCase("numacc3")]
        [TestCase("numacc4")]
        public void NullableMean(string dataSet)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqual(data.Mean, data.DataWithNulls.Mean(), 15);
        }

        /// <summary>
        /// Mean with <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void MeanThrowsArgumentNullException()
        {
            double[] data = null;
            Assert.Throws<ArgumentNullException>(() => data.Mean());
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
            AssertHelpers.AlmostEqual(data.StandardDeviation, data.Data.StandardDeviation(), digits);
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
            AssertHelpers.AlmostEqual(data.StandardDeviation, data.DataWithNulls.StandardDeviation(), digits);
        }

        /// <summary>
        /// Standard Deviation with <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void StandardDeviationThrowsArgumentNullException()
        {
            double[] data = null;
            Assert.Throws<ArgumentNullException>(() => data.StandardDeviation());
        }

        /// <summary>
        /// Validate Min/Max on a short sequence.
        /// </summary>
        [Test]
        public void ShortMinMax()
        {
            var samples = new[] { -1.0, 5, 0, -3, 10, -0.5, 4 };
            Assert.That(samples.Minimum(), Is.EqualTo(-3), "Min");
            Assert.That(samples.Maximum(), Is.EqualTo(10), "Max");
        }

        /// <summary>
        /// Validate Order Statistics and Median on a short sequence.
        /// </summary>
        [Test]
        public void ShortOrderMedian()
        {
            // -3 -1 -0.5 0  1  4 5 6 10
            var samples = new[] { -1, 5, 0, -3, 10, -0.5, 4, 1, 6 };
            Assert.That(samples.Median(), Is.EqualTo(1), "Median");
            Assert.That(Statistics.OrderStatistic(samples, 1), Is.EqualTo(-3), "Order-1");
            Assert.That(Statistics.OrderStatistic(samples, 3), Is.EqualTo(-0.5), "Order-3");
            Assert.That(Statistics.OrderStatistic(samples, 7), Is.EqualTo(5), "Order-7");
            Assert.That(Statistics.OrderStatistic(samples, 9), Is.EqualTo(10), "Order-9");
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

            AssertHelpers.AlmostEqual(1e+9, gaussian.Samples().Take(10000).Mean(), 11);
            AssertHelpers.AlmostEqual(4d, gaussian.Samples().Take(10000).Variance(), 1);
            AssertHelpers.AlmostEqual(2d, gaussian.Samples().Take(10000).StandardDeviation(), 2);
        }

        /// <summary>
        /// URL http://mathnetnumerics.codeplex.com/workitem/5667
        /// </summary>
        [Test]
        public void Median_CodeplexIssue5667()
        {
            var seq = File.ReadLines("./data/Codeplex-5667.csv").Select(s => double.Parse(s));
            Assert.AreEqual(1.0, seq.Median());
        }
    }
#endif
}
