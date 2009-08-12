// <copyright file="StatisticsTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MbUnit.Framework;
    using MathNet.Numerics.Statistics;

    [TestFixture]
    public class StatisticsTests
    {
        private readonly IDictionary<string, StatTestData> mData = new Dictionary<string, StatTestData>();
        public StatisticsTests()
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
        [Row("lottery")]
        [Row("lew")]
        [Row("mavro")]
        [Row("michelso")]
        [Row("numacc1")]
        [Row("numacc2")]
        [Row("numacc3")]
        [Row("numacc4")]
        public void Mean(string dataSet)
        {
            StatTestData data = mData[dataSet];
            AssertHelpers.AlmostEqual(data.Mean, data.Data.Mean(), 15);
        }

        [Test]
        [Row("lottery")]
        [Row("lew")]
        [Row("mavro")]
        [Row("michelso")]
        [Row("numacc1")]
        [Row("numacc2")]
        [Row("numacc3")]
        [Row("numacc4")]
        public void NullableMean(string dataSet)
        {
            StatTestData data = mData[dataSet];
            AssertHelpers.AlmostEqual(data.Mean, data.DataWithNulls.Mean(), 15);
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Mean_ThrowsArgumentNullException()
        {
            double[] data = null;
            MathNet.Numerics.Statistics.Statistics.Mean(data);
        }


        [Test]
        [Row("lottery", 15)]
        [Row("lew", 15)]
        [Row("mavro", 12)]
        [Row("michelso", 12)]
        [Row("numacc1", 15)]
        [Row("numacc2", 14)]
        [Row("numacc3", 9)]
        [Row("numacc4", 8)]
        public void StandardDeviation(string dataSet, int digits)
        {
            StatTestData data = mData[dataSet];
            AssertHelpers.AlmostEqual(data.StandardDeviation, data.Data.StandardDeviation(), digits);
        }

        [Test]
        [Row("lottery", 15)]
        [Row("lew", 15)]
        [Row("mavro", 12)]
        [Row("michelso", 12)]
        [Row("numacc1", 15)]
        [Row("numacc2", 14)]
        [Row("numacc3", 9)]
        [Row("numacc4", 8)]
        public void NullableStandardDeviation(string dataSet, int digits)
        {
            StatTestData data = mData[dataSet];
            AssertHelpers.AlmostEqual(data.StandardDeviation, data.DataWithNulls.StandardDeviation(), digits);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StandardDeviation_ThrowsArgumentNullException()
        {
            double[] data = null;
            MathNet.Numerics.Statistics.Statistics.StandardDeviation(data);
        }
    }
}
