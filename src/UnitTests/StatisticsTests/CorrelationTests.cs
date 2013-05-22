// <copyright file="CorrelationTests.cs" company="Math.NET">
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
    using System.Linq;
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Correlation tests
    /// </summary>
    /// <remarks>NOTE: this class is not included into Silverlight version, because it uses data from local files. 
    /// In Silverlight access to local files is forbidden, except several cases.</remarks>
    [TestFixture]
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
            var lottery = new StatTestData("./data/NIST/Lottery.dat");
            _data.Add("lottery", lottery);
            var lew = new StatTestData("./data/NIST/Lew.dat");
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
            AssertHelpers.AlmostEqual(-0.029470861580726, corr, 13);
        }

        /// <summary>
        /// Pearson correlation test fail.
        /// </summary>
        [Test]
        public void PearsonCorrelationTestFail()
        {
            var dataA = _data["lottery"].Data;
            var dataB = _data["lew"].Data;
            Assert.Throws<ArgumentOutOfRangeException>(() => Correlation.Pearson(dataA, dataB));
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
            AssertHelpers.AlmostEqual(-0.0382856977898528, corr, 13);
        }

        /// <summary>
        /// Spearman correlation test fail.
        /// </summary>
        [Test]
        public void SpearmanCorrelationTestFail()
        {
            var dataA = _data["lottery"].Data;
            var dataB = _data["lew"].Data;
            Assert.Throws<ArgumentOutOfRangeException>(() => Correlation.Spearman(dataA, dataB));
        }
    }
#endif
}
