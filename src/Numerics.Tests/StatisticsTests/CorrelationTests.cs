// <copyright file="CorrelationTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2018 Math.NET
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
using NUnit.Framework;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.TestData;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    /// <summary>
    /// Correlation tests
    /// </summary>
    /// <remarks>NOTE: this class is not included into Silverlight version, because it uses data from local files.
    /// In Silverlight access to local files is forbidden, except several cases.</remarks>
    [TestFixture, Category("Statistics")]
    public class CorrelationTests
    {
        /// <summary>
        /// Statistics data.
        /// </summary>
        readonly IDictionary<string, StatTestData> _data = new Dictionary<string, StatTestData>();

        /// <summary>
        /// Initializes a new instance of the CorrelationTests class.
        /// </summary>
        public CorrelationTests()
        {
            var lottery = new StatTestData("NIST.Lottery.dat");
            _data.Add("lottery", lottery);
            var lew = new StatTestData("NIST.Lew.dat");
            _data.Add("lew", lew);
        }

        [TestCase("numpy.CorrNumpyData_sqr.csv", 0.005)]
        [TestCase("numpy.CorrNumpyData_pwm.csv", 0.005)]
        [TestCase("numpy.CorrNumpyData_sin.csv", 0.005)]
        [TestCase("numpy.CorrNumpyData_rnd.csv", 0.005)]
        public void AutoCorrelationTest(string fName, double tol)
        {
            var data = Data.ReadAllLines(fName)
                .Select(line =>
                {
                    var vals = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new Tuple<string, string>(vals[0], vals[1]);
                }).ToArray();

            var series = data.Select(tuple => Double.Parse(tuple.Item1, CultureInfo.InvariantCulture)).ToArray();
            var resNumpy = data.Select(tuple => Double.Parse(tuple.Item2, CultureInfo.InvariantCulture)).ToArray();

            var resMathNet = Correlation.Auto(series);

            Assert.AreEqual(resNumpy.Length, resMathNet.Length);
            for (int i = 0; i < resMathNet.Length; i++)
            {
                Assert.AreEqual(resNumpy[i], resMathNet[i], tol);
            }
        }

        [TestCase("numpy.XCorrNumpyData.json", CorrelationMode.Valid, "result_valid")]
        [TestCase("numpy.XCorrNumpyData.json", CorrelationMode.Same, "result_same")]
        [TestCase("numpy.XCorrNumpyData.json", CorrelationMode.Full, "result_full")]
        [TestCase("numpy.XCorrNumpyDataShortY.json", CorrelationMode.Valid, "result_valid")]
        [TestCase("numpy.XCorrNumpyDataShortY.json", CorrelationMode.Same, "result_same")]
        [TestCase("numpy.XCorrNumpyDataShortY.json", CorrelationMode.Full, "result_full")]
        public void CrossCorrelationTest(string filename, CorrelationMode mode, string resultMemberName)
        {
            // read the test data
            var serializer = new DataContractJsonSerializer(typeof(XCorrTestdata));
            XCorrTestdata testdata;
            using (var stream = Data.ReadStream(filename))
            {
                testdata = serializer.ReadObject(stream) as XCorrTestdata;
            }
            Assume.That(testdata, Is.Not.Null);

            // select the test data for this test case
            var x = testdata.x;
            var y = testdata.y;
            var expectedResult = typeof(XCorrTestdata).GetField(resultMemberName).GetValue(testdata) as double[];
            Assume.That(expectedResult, Is.Not.Null);

            var actualResult = Correlation.CrossCorrelation(x, y, mode);
            Assert.That(actualResult, Is.EqualTo(expectedResult).Within(1e-10));
        }

        [TestCase(CorrelationMode.Valid)]
        [TestCase(CorrelationMode.Same)]
        [TestCase(CorrelationMode.Full)]
        public void CrossCorrelationInvalid(CorrelationMode mode)
        {
            // The first sequence must be equally long or longer than the second sequence
            Assert.Throws<ArgumentException>(() =>
            {
                Correlation.CrossCorrelation(new double[5], new double[6], mode);
            });
            Assert.DoesNotThrow(() =>
            {
                Correlation.CrossCorrelation(new double[5], new double[5], mode);
            });
        }

        [DataContract]
        public class XCorrTestdata
        {
            [DataMember]
            public double[] x;
            [DataMember]
            public double[] y;
            [DataMember]
            public double[] result_valid;
            [DataMember]
            public double[] result_same;
            [DataMember]
            public double[] result_full;
        }

        [TestCase()]
        public void AutoCorrelationTest2()
        {
            var tol = 1e-14;
            var n = 10;
            // make some dummy data
            var a = Generate.LinearSpacedMap(n, 0, 2*Constants.Pi, Math.Sin);

            var idxs = new int[] { 2, 1, 7 };

            var resFull = Correlation.Auto(a);
            var resLim = Correlation.Auto(a, 4);

            Assert.AreEqual(4+1, resLim.Length);
            for (int i = 0; i < resLim.Length; i++)
            {
                Assert.AreEqual(resFull[i], resLim[i], tol);
            }

            var resRange = Correlation.Auto(a, 3, 6);   // -> Order will be set in function anyways
            Assert.AreEqual(6-3+1, resRange.Length);
            for (int i = 0; i < resRange.Length; i++)
            {
                Assert.AreEqual(resFull[i], resLim[i], tol);
            }

            var resIdxs = Correlation.Auto(a, idxs);
            Assert.AreEqual(idxs.Length, resIdxs.Length);
            Assert.AreEqual(resFull[2], resIdxs[0], tol);
            Assert.AreEqual(resFull[1], resIdxs[1], tol);
            Assert.AreEqual(resFull[7], resIdxs[2], tol);

        }

        /// <summary>
        /// Pearson correlation test.
        /// </summary>
        [Test]
        public void PearsonCorrelationTest()
        {
            var dataA = _data["lottery"].Data.Take(200);
            var dataB = _data["lew"].Data.Take(200);

            var corr = Correlation.Pearson(dataA, dataB);
            AssertHelpers.AlmostEqual(-0.029470861580726, corr, 14);
        }

        /// <summary>
        /// Pearson correlation test.
        /// </summary>
        [Test]
        public void PearsonCorrelationConsistentWithCovariance()
        {
            var dataA = _data["lottery"].Data.Take(200).ToArray();
            var dataB = _data["lew"].Data.Take(200).ToArray();

            var direct = Correlation.Pearson(dataA, dataB);
            var covariance = dataA.Covariance(dataB)/(dataA.StandardDeviation()*dataB.StandardDeviation());

            AssertHelpers.AlmostEqual(covariance, direct, 14);
        }


        /// <summary>
        /// Constant-weighted Pearson correlation test.
        /// </summary>
        [Test]
        public void ConstantWeightedPearsonCorrelationTest()
        {
            var dataA = _data["lottery"].Data.Take(200);
            var dataB = _data["lew"].Data.Take(200);
            var weights = Generate.Repeat(200, 2.0);

            var corr = Correlation.Pearson(dataA, dataB);
            var corr2 = Correlation.WeightedPearson(dataA, dataB, weights);
            AssertHelpers.AlmostEqual(corr, corr2, 14);
        }

        /// <summary>
        /// Correlation between two identical data sets should always equal one,
        /// regardless of the weights used.
        /// </summary>
        [Test]
        public void WeightedPearsonCorrelationEqualsOneTest()
        {
            int n = 5;
            double maxWeight = 1e5;
            var dataA = Generate.LinearRange(1, n);
            var dataB = Generate.LinearRange(1, n);
            var weights1 = Generate.LinearRange(1, n);
            var weights2 = Generate.LogSpaced(n, 1, 5);
            var weights3 = Generate.LogSpaced(n, 5, 1);
            var weights4 = Generate.Repeat(n, maxWeight);
            var weights5 = Generate.Repeat(n, 1/maxWeight);

            var corr1 = Correlation.WeightedPearson(dataA, dataB, weights1);
            var corr2 = Correlation.WeightedPearson(dataA, dataB, weights2);
            var corr3 = Correlation.WeightedPearson(dataA, dataB, weights3);
            var corr4 = Correlation.WeightedPearson(dataA, dataB, weights4);
            var corr5 = Correlation.WeightedPearson(dataA, dataB, weights5);

            AssertHelpers.AlmostEqual(corr1, 1, 14);
            AssertHelpers.AlmostEqual(corr2, 1, 14);
            AssertHelpers.AlmostEqual(corr3, 1, 14);
            AssertHelpers.AlmostEqual(corr4, 1, 14);
            AssertHelpers.AlmostEqual(corr5, 1, 14);
        }

        /// <summary>
        /// Pearson correlation test fail.
        /// </summary>
        [Test]
        public void PearsonCorrelationTestFail()
        {
            var dataA = _data["lottery"].Data;
            var dataB = _data["lew"].Data;
            Assert.That(() => Correlation.Pearson(dataA, dataB), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Spearman correlation test.
        /// </summary>
        [Test]
        public void SpearmanCorrelationTest()
        {
            var dataA = _data["lottery"].Data.Take(200);
            var dataB = _data["lew"].Data.Take(200);

            var corr = Correlation.Spearman(dataA, dataB);
            AssertHelpers.AlmostEqual(-0.0382856977898528, corr, 14);
        }

        /// <summary>
        /// Spearman correlation test fail.
        /// </summary>
        [Test]
        public void SpearmanCorrelationTestFail()
        {
            var dataA = _data["lottery"].Data;
            var dataB = _data["lew"].Data;
            Assert.That(() => Correlation.Spearman(dataA, dataB), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
