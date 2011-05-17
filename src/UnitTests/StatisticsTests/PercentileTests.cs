// <copyright file="PercentileTests.cs" company="Math.NET">
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
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Percentile tests.
    /// </summary>
    [TestFixture]
    public class PercentileTests
    {
        /// <summary>
        /// Data array
        /// </summary>
        private static readonly double[] Data = { 95.1772, 95.1567, 95.1937, 95.1959, 95.1442, 95.061, 95.1591, 95.1195, 95.1065, 95.0925, 95.199, 95.1682 };

        /// <summary>
        /// Can compute percentile using NIST method.
        /// </summary>
        [Test]
        public void CanComputePercentileUsingNistMethod()
        {
            var percentile = new Percentile(Data)
                             {
                                 Method = PercentileMethod.Nist
                             };
            Assert.AreEqual(95.19807, percentile.Compute(.9));
        }

        /// <summary>
        /// Can compute percentile using excel method.
        /// </summary>
        [Test]
        public void CanComputePercentileUsingExcelMethod()
        {
            var percentile = new Percentile(Data)
                             {
                                 Method = PercentileMethod.Excel
                             };
            Assert.AreEqual(95.19568, percentile.Compute(.9));
        }

        /// <summary>
        /// Can compute percentile using nearest method.
        /// </summary>
        [Test]
        public void CanComputePercentileUsingNearestMethod()
        {
            var percentile = new Percentile(Data)
                             {
                                 Method = PercentileMethod.Nearest
                             };
            Assert.AreEqual(95.1959, percentile.Compute(.9));
        }

        /// <summary>
        /// Can compute percentile using interpolation method
        /// </summary>
        [Test]
        public void CanComputePercentileUsingInterpolationMethod()
        {
            var data = new double[] { 1, 2, 3, 4, 5 };
            var percentile = new Percentile(data)
                             {
                                 Method = PercentileMethod.Interpolation
                             };
            var values = new[] { .25, .5, .75 };
            var percentiles = percentile.Compute(values);
            Assert.AreEqual(1.75, percentiles[0]);
            Assert.AreEqual(3.0, percentiles[1]);
            Assert.AreEqual(4.25, percentiles[2]);
        }

        /// <summary>
        /// Small dataset throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SmallDataSetThrowArgumentException()
        {
            var data = new double[] { };
            var data1 = data;
            Assert.Throws<ArgumentException>(() => new Percentile(data1));

            data = new double[] { 1 };
            var data2 = data;
            Assert.Throws<ArgumentException>(() => new Percentile(data2));

            data = new double[] { 1, 2 };
            Assert.Throws<ArgumentException>(() => new Percentile(data));
        }

        /// <summary>
        /// Invalid percentile values throw <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void InvalidPercentileValuesThrowArgumentException()
        {
            var percentile = new Percentile(Data);
            Assert.Throws<ArgumentException>(() => percentile.Compute(-0.1));
            Assert.Throws<ArgumentException>(() => percentile.Compute(100.1));
        }
    }
}
