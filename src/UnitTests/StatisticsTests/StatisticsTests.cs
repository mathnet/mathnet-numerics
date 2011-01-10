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
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Statistics tests.
    /// </summary>
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
        [Test]
        public void Mean([Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqual(data.Mean, data.Data.Mean(), 15);
        }

        /// <summary>
        /// <c>Nullable</c> mean.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        [Test]
        public void NullableMean([Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet)
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
        [Test, Sequential]
        public void StandardDeviation(
            [Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet, 
            [Values(15, 15, 12, 12, 15, 13, 9, 8)] int digits)
        {
            var data = _data[dataSet];
            AssertHelpers.AlmostEqual(data.StandardDeviation, data.Data.StandardDeviation(), digits);
        }

        /// <summary>
        /// <c>Nullable</c> Standard Deviation.
        /// </summary>
        /// <param name="dataSet">Dataset name.</param>
        /// <param name="digits">Digits count.</param>
        [Test, Sequential]
        public void NullableStandardDeviation(
            [Values("lottery", "lew", "mavro", "michelso", "numacc1", "numacc2", "numacc3", "numacc4")] string dataSet, 
            [Values(15, 15, 12, 12, 15, 13, 9, 8)] int digits)
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
    }
}
