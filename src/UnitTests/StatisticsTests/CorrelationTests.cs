// <copyright file="CorrelationTests.cs" company="Math.NET">
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
using NUnit.Framework;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
#if !PORTABLE
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
#endif
}
