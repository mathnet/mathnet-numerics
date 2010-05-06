// <copyright file="CorrelationTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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
    using System.Linq;
    using System.Collections.Generic;
    using MbUnit.Framework;
    using Statistics;

	[TestFixture]
    public class CorrelationTests
    {
        private readonly IDictionary<string, StatTestData> mData = new Dictionary<string, StatTestData>();

        public CorrelationTests()
        {
            StatTestData lottery = new StatTestData("./data/NIST/Lottery.dat");
            mData.Add("lottery", lottery);
            StatTestData lew = new StatTestData("./data/NIST/Lew.dat");
            mData.Add("lew", lew);
        }

        [Test]
        public void PearsonCorrelationTest()
        {
            var dataA = mData["lottery"].Data.Take(200);
            var dataB = mData["lew"].Data.Take(200);

            double corr = Correlation.Pearson(dataA, dataB);
            AssertHelpers.AlmostEqual(corr, -0.029470861580726, 13);
        }

        [Test]
        public void PearsonCorrelationTest_Fail()
        {
            var dataA = mData["lottery"].Data;
            var dataB = mData["lew"].Data;

            Assert.Throws<ArgumentOutOfRangeException>(() => Correlation.Pearson(dataA, dataB));
        }
    }
}
