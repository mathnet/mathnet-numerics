// <copyright file="DescriptiveStatisticsTests.cs" company="Math.NET">
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

using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    /// <summary>
    /// Running statistics tests.
    /// </summary>
    /// <remarks>NOTE: this class is not included into Silverlight version, because it uses data from local files.
    /// In Silverlight access to local files is forbidden, except several cases.</remarks>
    [TestFixture, Category("Statistics")]
    public class RunningStatisticsTests
    {
        /// <summary>
        /// Statistics data.
        /// </summary>
        readonly IDictionary<string, StatTestData> _data = new Dictionary<string, StatTestData>();

        /// <summary>
        /// Initializes a new instance of the DescriptiveStatisticsTests class.
        /// </summary>
        public RunningStatisticsTests()
        {
            _data.Add("lottery", new StatTestData("NIST.Lottery.dat"));
            _data.Add("lew", new StatTestData("NIST.Lew.dat"));
            _data.Add("mavro", new StatTestData("NIST.Mavro.dat"));
            _data.Add("michelso", new StatTestData("NIST.Michelso.dat"));
            _data.Add("numacc1", new StatTestData("NIST.NumAcc1.dat"));
            _data.Add("numacc2", new StatTestData("NIST.NumAcc2.dat"));
            _data.Add("numacc3", new StatTestData("NIST.NumAcc3.dat"));
            _data.Add("numacc4", new StatTestData("NIST.NumAcc4.dat"));
            _data.Add("meixner", new StatTestData("NIST.Meixner.dat"));
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
        [TestCase("lottery", 14, -0.09333165310779, -1.19256091074856, 522.5, 4, 999, 218)]
        [TestCase("lew", 14, -0.050606638756334, -1.49604979214447, -162, -579, 300, 200)]
        [TestCase("mavro", 11, 0.64492948110824, -0.82052379677456, 2.0018, 2.0013, 2.0027, 50)]
        [TestCase("michelso", 11, -0.0185388637725746, 0.33968459842539, 299.85, 299.62, 300.07, 100)]
        [TestCase("numacc1", 15, 0, double.NaN, 10000002, 10000001, 10000003, 3)]
        [TestCase("numacc2", 13, 0, -2.003003003003, 1.2, 1.1, 1.3, 1001)]
        [TestCase("numacc3", 9, 0, -2.003003003003, 1000000.2, 1000000.1, 1000000.3, 1001)]
        [TestCase("numacc4", 7, 0, -2.00300300299913, 10000000.2, 10000000.1, 10000000.3, 1001)]
        [TestCase("meixner", 8, -0.016649617280859657, 0.8171318629552635, -0.002042931016531602, -4.825626912281697, 5.3018298664184913, 10000)]
        public void ConsistentWithNist(string dataSet, int digits, double skewness, double kurtosis, double median, double min, double max, int count)
        {
            var data = _data[dataSet];
            var stats = new RunningStatistics(data.Data);

            AssertHelpers.AlmostEqualRelative(data.Mean, stats.Mean, 10);
            AssertHelpers.AlmostEqualRelative(data.StandardDeviation, stats.StandardDeviation, digits);
            AssertHelpers.AlmostEqualRelative(skewness, stats.Skewness, 8);
            AssertHelpers.AlmostEqualRelative(kurtosis, stats.Kurtosis, 8);
            Assert.AreEqual(stats.Minimum, min);
            Assert.AreEqual(stats.Maximum, max);
            Assert.AreEqual(stats.Count, count);
        }

        [TestCase("lottery", 1e-8, -0.09268823, -0.09333165)]
        [TestCase("lew", 1e-8, -0.0502263, -0.05060664)]
        [TestCase("mavro", 1e-6, 0.6254181, 0.6449295)]
        [TestCase("michelso", 1e-8, -0.01825961, -0.01853886)]
        [TestCase("numacc1", 1e-8, 0, 0)]
        //[TestCase("numacc2", 1e-20, 3.254232e-15, 3.259118e-15)] TODO: accuracy
        //[TestCase("numacc3", 1e-14, 1.747103e-09, 1.749726e-09)] TODO: accuracy
        //[TestCase("numacc4", 1e-13, 2.795364e-08, 2.799561e-08)] TODO: accuracy
        [TestCase("meixner", 1e-8, -0.01664712, -0.01664962)]
        public void SkewnessConsistentWithR_e1071(string dataSet, double delta, double skewnessType1, double skewnessType2)
        {
            var data = _data[dataSet];
            var stats = new RunningStatistics(data.Data);

            Assert.That(stats.Skewness, Is.EqualTo(skewnessType2).Within(delta), "Skewness");
            Assert.That(stats.PopulationSkewness, Is.EqualTo(skewnessType1).Within(delta), "PopulationSkewness");
        }

        [TestCase("lottery", -1.192781, -1.192561)]
        [TestCase("lew", -1.48876, -1.49605)]
        [TestCase("mavro", -0.858384, -0.8205238)]
        [TestCase("michelso", 0.2635305, 0.3396846)]
        [TestCase("numacc1", -1.5, double.NaN)]
        [TestCase("numacc2", -1.999, -2.003003)]
        [TestCase("numacc3", -1.999, -2.003003)]
        [TestCase("numacc4", -1.999, -2.003003)]
        [TestCase("meixner", 0.8161234, 0.8171319)]
        public void KurtosisConsistentWithR_e1071(string dataSet, double kurtosisType1, double kurtosisType2)
        {
            var data = _data[dataSet];
            var stats = new RunningStatistics(data.Data);

            Assert.That(stats.Kurtosis, Is.EqualTo(kurtosisType2).Within(1e-6), "Kurtosis");
            Assert.That(stats.PopulationKurtosis, Is.EqualTo(kurtosisType1).Within(1e-6), "PopulationKurtosis");
        }

        [Test]
        public void ShortSequences()
        {
            var stats0 = new RunningStatistics(new double[0]);
            Assert.That(stats0.Skewness, Is.NaN);
            Assert.That(stats0.Kurtosis, Is.NaN);

            var stats1 = new RunningStatistics(new[] { 1.0 });
            Assert.That(stats1.Skewness, Is.NaN);
            Assert.That(stats1.Kurtosis, Is.NaN);

            var stats2 = new RunningStatistics(new[] { 1.0, 2.0 });
            Assert.That(stats2.Skewness, Is.NaN);
            Assert.That(stats2.Kurtosis, Is.NaN);

            var stats3 = new RunningStatistics(new[] { 1.0, 2.0, -3.0 });
            Assert.That(stats3.Skewness, Is.Not.NaN);
            Assert.That(stats3.Kurtosis, Is.NaN);

            var stats4 = new RunningStatistics(new[] { 1.0, 2.0, -3.0, -4.0 });
            Assert.That(stats4.Skewness, Is.Not.NaN);
            Assert.That(stats4.Kurtosis, Is.Not.NaN);
        }

        [Test]
        public void ZeroVarianceSequence()
        {
            var stats = new RunningStatistics(new[] { 2.0, 2.0, 2.0, 2.0 });
            Assert.That(stats.Skewness, Is.NaN);
            Assert.That(stats.Kurtosis, Is.NaN);
        }

        [Test]
        public void Combine()
        {
            var rnd = new SystemRandomSource(10);
            var a = Generate.Random(200, new Erlang(2, 0.2, rnd));
            var b = Generate.Random(100, new Beta(1.2, 1.4, rnd));
            var c = Generate.Random(150, new Rayleigh(0.8, rnd));

            var d = a.Concat(b).Concat(c).ToArray();

            var x = new RunningStatistics(d);

            var y = new RunningStatistics(a);
            y.PushRange(b);
            y.PushRange(c);

            var za  = new RunningStatistics(a);
            var zb  = new RunningStatistics(b);
            var zc  = new RunningStatistics(c);
            var z = za + zb + zc;

            Assert.That(x.Mean, Is.EqualTo(d.Mean()).Within(1e-12), "Mean Reference");
            Assert.That(y.Mean, Is.EqualTo(x.Mean).Within(1e-12), "Mean y");
            Assert.That(z.Mean, Is.EqualTo(x.Mean).Within(1e-12), "Mean z");

            Assert.That(x.Variance, Is.EqualTo(d.Variance()).Within(1e-12), "Variance Reference");
            Assert.That(y.Variance, Is.EqualTo(x.Variance).Within(1e-12), "Variance y");
            Assert.That(z.Variance, Is.EqualTo(x.Variance).Within(1e-12), "Variance z");

            Assert.That(x.PopulationVariance, Is.EqualTo(d.PopulationVariance()).Within(1e-12), "PopulationVariance Reference");
            Assert.That(y.PopulationVariance, Is.EqualTo(x.PopulationVariance).Within(1e-12), "PopulationVariance y");
            Assert.That(z.PopulationVariance, Is.EqualTo(x.PopulationVariance).Within(1e-12), "PopulationVariance z");

            Assert.That(x.StandardDeviation, Is.EqualTo(d.StandardDeviation()).Within(1e-12), "StandardDeviation Reference");
            Assert.That(y.StandardDeviation, Is.EqualTo(x.StandardDeviation).Within(1e-12), "StandardDeviation y");
            Assert.That(z.StandardDeviation, Is.EqualTo(x.StandardDeviation).Within(1e-12), "StandardDeviation z");

            Assert.That(x.PopulationStandardDeviation, Is.EqualTo(d.PopulationStandardDeviation()).Within(1e-12), "PopulationStandardDeviation Reference");
            Assert.That(y.PopulationStandardDeviation, Is.EqualTo(x.PopulationStandardDeviation).Within(1e-12), "PopulationStandardDeviation y");
            Assert.That(z.PopulationStandardDeviation, Is.EqualTo(x.PopulationStandardDeviation).Within(1e-12), "PopulationStandardDeviation z");

            Assert.That(x.Skewness, Is.EqualTo(d.Skewness()).Within(1e-12), "Skewness Reference (not independent!)");
            Assert.That(y.Skewness, Is.EqualTo(x.Skewness).Within(1e-12), "Skewness y");
            Assert.That(z.Skewness, Is.EqualTo(x.Skewness).Within(1e-12), "Skewness z");

            Assert.That(x.PopulationSkewness, Is.EqualTo(d.PopulationSkewness()).Within(1e-12), "PopulationSkewness Reference (not independent!)");
            Assert.That(y.PopulationSkewness, Is.EqualTo(x.PopulationSkewness).Within(1e-12), "PopulationSkewness y");
            Assert.That(z.PopulationSkewness, Is.EqualTo(x.PopulationSkewness).Within(1e-12), "PopulationSkewness z");

            Assert.That(x.Kurtosis, Is.EqualTo(d.Kurtosis()).Within(1e-12), "Kurtosis Reference (not independent!)");
            Assert.That(y.Kurtosis, Is.EqualTo(x.Kurtosis).Within(1e-12), "Kurtosis y");
            Assert.That(z.Kurtosis, Is.EqualTo(x.Kurtosis).Within(1e-12), "Kurtosis z");

            Assert.That(x.PopulationKurtosis, Is.EqualTo(d.PopulationKurtosis()).Within(1e-12), "PopulationKurtosis Reference (not independent!)");
            Assert.That(y.PopulationKurtosis, Is.EqualTo(x.PopulationKurtosis).Within(1e-12), "PopulationKurtosis y");
            Assert.That(z.PopulationKurtosis, Is.EqualTo(x.PopulationKurtosis).Within(1e-12), "PopulationKurtosis z");
        }
    }
}
