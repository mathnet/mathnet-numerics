// <copyright file="MovingAverageTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2013 Math.NET
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
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Moving average tests.
    /// </summary>
    [TestFixture]
    public class MovingAverageTests
    {
        /// <summary>
        /// The moving average of an empty data set is also empty.
        /// </summary>
        [Test]
        public void MovingAverageEmptyData()
        {
            var ma = new MovingAverage(new double[0], 10);
            var average = ma.Averages;
            Assert.AreEqual(0, average.Count);
        }

        /// <summary>
        /// The moving average of an empty data set is also empty.
        /// </summary>
        [Test]
        public void MovingAverageNullableEmptyData()
        {
            var ma = new MovingAverage(new double?[0], 10);
            var average = ma.Averages;
            Assert.AreEqual(0, average.Count);
        }

        /// <summary>
        /// The moving average of data set with insufficient number of items is empty.
        /// </summary>
        [Test]
        public void MovingAverageInsufficientData()
        {
            var ma = new MovingAverage(new double[9], 10);
            var average = ma.Averages;
            Assert.AreEqual(0, average.Count);

            ma = new MovingAverage(new double[10], 10);
            average = ma.Averages;
            Assert.AreEqual(1, average.Count);
        }

        /// <summary>
        /// The moving average of data set with insufficient number of items is empty.
        /// </summary>
        [Test]
        public void MovingAverageInsufficientNullableEmptyData()
        {
            var ma = new MovingAverage(new double?[9], 10);
            var average = ma.Averages;
            Assert.AreEqual(0, average.Count);

            ma = new MovingAverage(new double?[10], 10);
            average = ma.Averages;
            Assert.AreEqual(0, average.Count);
        }

        /// <summary>
        /// Can calculate a the moving average of an array with a window size of 2.
        /// </summary>
        [Test]
        public void MovingAverage()
        {
            var ma = new MovingAverage(new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 }, 2);
            var average = ma.Averages;
            Assert.AreEqual(average.Count, 9);
            Assert.AreEqual(1.5, average[0]);
            Assert.AreEqual(2.5, average[1]);
            Assert.AreEqual(3.5, average[2]);
            Assert.AreEqual(4.5, average[3]);
            Assert.AreEqual(5.5, average[4]);
            Assert.AreEqual(6.5, average[5]);
            Assert.AreEqual(7.5, average[6]);
            Assert.AreEqual(8.5, average[7]);
            Assert.AreEqual(9.5, average[8]);

            ma.Update(new [] { 11.0, 12.0 });
            Assert.AreEqual(average.Count, 11);
            Assert.AreEqual(10.5, average[9]);
            Assert.AreEqual(11.5, average[10]);
        }

        /// <summary>
        /// Can calculate a the moving average of an array with a window size of 2.
        /// </summary>
        [Test]
        public void MovingAverageNullableData()
        {
            var ma = new MovingAverage(new double?[] { 1.0, null, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, null, 10.0 }, 2);
            var average = ma.Averages;
            Assert.AreEqual(average.Count, 7);
            Assert.AreEqual(2.0, average[0]);
            Assert.AreEqual(3.5, average[1]);
            Assert.AreEqual(4.5, average[2]);
            Assert.AreEqual(5.5, average[3]);
            Assert.AreEqual(6.5, average[4]);
            Assert.AreEqual(7.5, average[5]);
            Assert.AreEqual(9.0, average[6]);

            ma.Update(new double?[] { 11.0, null, 12.0 });
            Assert.AreEqual(average.Count, 9);
            Assert.AreEqual(10.5, average[7]);
            Assert.AreEqual(11.5, average[8]);

        }
    }
}
