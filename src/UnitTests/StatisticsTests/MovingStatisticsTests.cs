// <copyright file="MovingStatisticsTests.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    [TestFixture, Category("Statistics")]
    public class MovingStatisticsTests
    {
        [Test]
        public void PositiveInfinityTest()
        {
            var ms = new MovingStatistics(3);

            ms.Push(1.0);
            ms.Push(2.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.Not.EqualTo(double.PositiveInfinity));

            ms.Push(double.PositiveInfinity);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Maximum, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.EqualTo(double.PositiveInfinity));

            ms.Push(1.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Maximum, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.EqualTo(double.PositiveInfinity));

            ms.Push(double.PositiveInfinity);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Maximum, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.EqualTo(double.PositiveInfinity));

            ms.Push(2.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Maximum, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.EqualTo(double.PositiveInfinity));

            ms.Push(3.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Maximum, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.EqualTo(double.PositiveInfinity));

            ms.Push(4.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.Not.EqualTo(double.PositiveInfinity));
        }

        [Test]
        public void NegativeInfinityTest()
        {
            var ms = new MovingStatistics(3);

            ms.Push(1.0);
            ms.Push(2.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.Not.EqualTo(double.NegativeInfinity));

            ms.Push(double.NegativeInfinity);
            Assert.That(ms.Minimum, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(1.0);
            Assert.That(ms.Minimum, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(double.NegativeInfinity);
            Assert.That(ms.Minimum, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(2.0);
            Assert.That(ms.Minimum, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(3.0);
            Assert.That(ms.Minimum, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(4.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.Not.NaN);
        }

        [Test]
        public void MixedInfinityTest()
        {
            var ms = new MovingStatistics(3);

            ms.Push(1.0);
            ms.Push(2.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.Not.EqualTo(double.PositiveInfinity));

            ms.Push(double.NegativeInfinity);
            Assert.That(ms.Minimum, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(1.0);
            Assert.That(ms.Minimum, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(double.PositiveInfinity);
            Assert.That(ms.Minimum, Is.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.NaN);
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(2.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.EqualTo(double.PositiveInfinity));

            ms.Push(3.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.EqualTo(double.PositiveInfinity));

            ms.Push(4.0);
            Assert.That(ms.Minimum, Is.Not.EqualTo(double.NegativeInfinity));
            Assert.That(ms.Maximum, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.Mean, Is.Not.EqualTo(double.PositiveInfinity));
            Assert.That(ms.StandardDeviation, Is.Not.EqualTo(double.PositiveInfinity));
        }

        [Test]
        public void StabilityTest()
        {
            var data = new double[1000000];
            Normal.Samples(new SystemRandomSource(0), data, 50, 10);

            var ms = new MovingStatistics(5, data);
            ms.PushRange(new[] { 11.11, 22.22, 33.33, 44.44, 55.55 });

            Assert.AreEqual(5, ms.Count);
            Assert.AreEqual(11.11, ms.Minimum);
            Assert.AreEqual(55.55, ms.Maximum);

            Assert.AreEqual(33.33, ms.Mean, 1e-11);
            Assert.AreEqual(308.58025, ms.Variance, 1e-10);
            Assert.AreEqual(17.5664524022354, ms.StandardDeviation, 1e-11);
            Assert.AreEqual(246.8642, ms.PopulationVariance, 1e-10);
            Assert.AreEqual(15.7119126779651, ms.PopulationStandardDeviation, 1e-10);
        }

        [Test]
        public void NaNTest()
        {
            var ms = new MovingStatistics(3);
            Assert.That(ms.Minimum, Is.NaN);
            Assert.That(ms.Maximum, Is.NaN);
            Assert.That(ms.Mean, Is.NaN);
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(1.0);
            Assert.That(ms.Minimum, Is.Not.NaN);
            Assert.That(ms.Maximum, Is.Not.NaN);
            Assert.That(ms.Mean, Is.Not.NaN);
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(2.0);
            Assert.That(ms.Minimum, Is.Not.NaN);
            Assert.That(ms.Maximum, Is.Not.NaN);
            Assert.That(ms.Mean, Is.Not.NaN);
            Assert.That(ms.StandardDeviation, Is.Not.NaN);

            ms.Push(double.NaN);
            Assert.That(ms.Minimum, Is.NaN);
            Assert.That(ms.Maximum, Is.NaN);
            Assert.That(ms.Mean, Is.NaN);
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(1.0);
            Assert.That(ms.Minimum, Is.NaN);
            Assert.That(ms.Maximum, Is.NaN);
            Assert.That(ms.Mean, Is.NaN);
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(2.0);
            Assert.That(ms.Minimum, Is.NaN);
            Assert.That(ms.Maximum, Is.NaN);
            Assert.That(ms.Mean, Is.NaN);
            Assert.That(ms.StandardDeviation, Is.NaN);

            ms.Push(3.0);
            Assert.That(ms.Minimum, Is.Not.NaN);
            Assert.That(ms.Maximum, Is.Not.NaN);
            Assert.That(ms.Mean, Is.Not.NaN);
            Assert.That(ms.StandardDeviation, Is.Not.NaN);
        }

        [Test]
        public void MovingAverageEmptyData()
        {
            var average = new double[0].MovingAverage(10).ToArray();
            Assert.AreEqual(0, average.Length);
        }

        [Test]
        public void MovingAverage()
        {
            var average = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 }.MovingAverage(2).ToArray();
            Assert.AreEqual(average.Length, 10);
            Assert.AreEqual(1.0, average[0]);
            Assert.AreEqual(1.5, average[1]);
            Assert.AreEqual(2.5, average[2]);
            Assert.AreEqual(3.5, average[3]);
            Assert.AreEqual(4.5, average[4]);
            Assert.AreEqual(5.5, average[5]);
            Assert.AreEqual(6.5, average[6]);
            Assert.AreEqual(7.5, average[7]);
            Assert.AreEqual(8.5, average[8]);
            Assert.AreEqual(9.5, average[9]);
        }

        [Test]
        public void MovingAverageMissingData()
        {
            var average = new[] { 1.0, double.NaN, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, double.NaN, 10.0 }.MovingAverage(2).ToArray();
            Assert.AreEqual(average.Length, 10);
            Assert.AreEqual(1.0, average[0]);
            Assert.IsNaN(average[1]);
            Assert.IsNaN(average[2]);
            Assert.AreEqual(3.5, average[3]);
            Assert.AreEqual(4.5, average[4]);
            Assert.AreEqual(5.5, average[5]);
            Assert.AreEqual(6.5, average[6]);
            Assert.AreEqual(7.5, average[7]);
            Assert.IsNaN(average[8]);
            Assert.IsNaN(average[9]);
        }
    }
}
