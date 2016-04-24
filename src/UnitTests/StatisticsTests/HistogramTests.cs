// <copyright file="HistogramTests.cs" company="Math.NET">
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
using NUnit.Framework;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    /// <summary>
    /// Histogram tests.
    /// </summary>
    [TestFixture, Category("Statistics")]
    public class HistogramTests
    {
        /// <summary>
        /// Datatset array.
        /// </summary>
        readonly double[] _smallDataset = {0.5, 1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5, 8.5, 9.5};

        /// <summary>
        /// Datatset array with small absolute values
        /// </summary>
        /// <remarks>
        /// These values are chosen to precisely match the upper bounds of 9 buckets,
        /// from 0.5e-22 to 9.5E-22
        /// </remarks>
        readonly double[] _smallValueDataset =
        {
            0.5e-22, 1.5E-22, 2.5E-22, 3.4999999999999996E-22, 4.4999999999999989E-22,
            5.4999999999999983E-22, 6.4999999999999986E-22, 7.4999999999999988E-22,
            8.4999999999999982E-22, 9.5E-22
        };

        /// <summary>
        /// Can create empty bucket.
        /// </summary>
        [Test]
        public void CanCreateEmptyBucket()
        {
            var b = new Bucket(0.0, 1.0);
        }

        /// <summary>
        /// Can create filled bucket.
        /// </summary>
        [Test]
        public void CanCreateFilledBucket()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
        }

        /// <summary>
        /// Empty bucket with bad bounds fails.
        /// </summary>
        [Test]
        public void EmptyBucketWithBadBoundsFails()
        {
            Assert.That(() => new Bucket(1.0, 0.5), Throws.ArgumentException);
        }

        /// <summary>
        /// Empty bucket with bad count fails.
        /// </summary>
        [Test]
        public void EmptyBucketWithBadCountFails()
        {
            Assert.That(() => new Bucket(1.0, 0.5, -1.0), Throws.ArgumentException);
        }

        /// <summary>
        /// Can get bucket width.
        /// </summary>
        [Test]
        public void CanGetBucketWidth()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
            Assert.AreEqual(1.0, b.Width);
        }

        /// <summary>
        /// Can get bucket count.
        /// </summary>
        [Test]
        public void CanGetBucketCount()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
            Assert.AreEqual(10.0, b.Count);
        }

        /// <summary>
        /// Can get bucket lower bound.
        /// </summary>
        [Test]
        public void CanGetBucketLowerBound()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
            Assert.AreEqual(0.0, b.LowerBound);
        }

        /// <summary>
        /// Can get bucket upper bound.
        /// </summary>
        [Test]
        public void CanGetBucketUpperBound()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
            Assert.AreEqual(1.0, b.UpperBound);
        }

        /// <summary>
        /// Validate contains.
        /// </summary>
        /// <param name="x">Point values.</param>
        /// <param name="r">Expected result.</param>
        [TestCase(0.0, -1)]
        [TestCase(1.0, 0)]
        [TestCase(1.05, 0)]
        [TestCase(2.0, 1)]
        [TestCase(-1.0, 0 - 1)]
        public void ValidateContains(double x, int r)
        {
            var b = new Bucket(0.0, 1.5, 10.0);
            Assert.AreEqual(r, b.Contains(x));
        }

        /// <summary>
        /// Can create empty histogram.
        /// </summary>
        [Test]
        public void CanCreateEmptyHistogram()
        {
            var h = new Histogram();
        }

        /// <summary>
        /// Can add bucket.
        /// </summary>
        [Test]
        public void CanAddBucket()
        {
            var h = new Histogram();
            h.AddBucket(new Bucket(0.0, 1.0));
        }

        /// <summary>
        /// Can get bucket index of.
        /// </summary>
        /// <param name="x">Point to check.</param>
        /// <param name="i">Bucket index.</param>
        [TestCase(0.5, 0)]
        [TestCase(1.0, 0)]
        [TestCase(10.0, 3)]
        [TestCase(10000.0, 4)]
        public void CanGetBucketIndexOf(double x, double i)
        {
            var h = new Histogram();
            h.AddBucket(new Bucket(0.0, 1.0));
            h.AddBucket(new Bucket(1.0, 2.0));
            h.AddBucket(new Bucket(2.0, 3.0));
            h.AddBucket(new Bucket(3.0, 20.0));
            h.AddBucket(new Bucket(20.0, Double.PositiveInfinity));
            Assert.AreEqual(i, h.GetBucketIndexOf(x));
        }

        /// <summary>
        /// Can get bucket index of fails when bucket doesn't exist.
        /// </summary>
        [Test]
        public void CanGetBucketIndexOfFailsWhenBucketDoesNotExist()
        {
            var h = new Histogram();
            h.AddBucket(new Bucket(0.0, 1.0));
            h.AddBucket(new Bucket(1.0, 2.0));
            h.AddBucket(new Bucket(2.0, 3.0));
            h.AddBucket(new Bucket(3.0, 20.0));
            h.AddBucket(new Bucket(20.0, Double.PositiveInfinity));
            Assert.That(() => { var i = h.GetBucketIndexOf(0.0); }, Throws.ArgumentException);
            Assert.That(() => { var i = h.GetBucketIndexOf(-1.0); }, Throws.ArgumentException);
        }

        /// <summary>
        /// Can get bucket of.
        /// </summary>
        [Test]
        public void CanGetBucketOf()
        {
            var h = new Histogram();
            var b = new Bucket(0.0, 1.0);
            h.AddBucket(b);
            h.AddBucket(new Bucket(1.0, 2.0));
            h.AddBucket(new Bucket(2.0, 3.0));
            h.AddBucket(new Bucket(3.0, 20.0));
            h.AddBucket(new Bucket(20.0, Double.PositiveInfinity));

            Assert.AreEqual(b, h.GetBucketOf(0.1));
        }

        /// <summary>
        /// Validate item.
        /// </summary>
        [Test]
        public void ValidateItem()
        {
            var h = new Histogram();
            var b = new Bucket(0.0, 1.0);
            var c = new Bucket(3.0, 20.0);
            h.AddBucket(b);
            h.AddBucket(c);
            h.AddBucket(new Bucket(1.0, 2.0));
            h.AddBucket(new Bucket(2.0, 3.0));
            h.AddBucket(new Bucket(20.0, Double.PositiveInfinity));

            Assert.AreEqual(b, h[0]);
            Assert.AreEqual(c, h[3]);
        }

        /// <summary>
        /// Can get bucket count in histogram.
        /// </summary>
        [Test]
        public void CanGetBucketCountInHistogram()
        {
            var h = new Histogram();
            h.AddBucket(new Bucket(0.0, 1.0));
            h.AddBucket(new Bucket(1.0, 2.0));
            h.AddBucket(new Bucket(2.0, 3.0));
            h.AddBucket(new Bucket(3.0, 20.0));
            h.AddBucket(new Bucket(20.0, Double.PositiveInfinity));

            Assert.AreEqual(5, h.BucketCount);
        }

        /// <summary>
        /// Can get total count.
        /// </summary>
        [Test]
        public void CanGetTotalCount()
        {
            var h = new Histogram();
            h.AddBucket(new Bucket(0.0, 1.0, 1));
            h.AddBucket(new Bucket(1.0, 2.0, 1));
            h.AddBucket(new Bucket(2.0, 3.0, 1));
            h.AddBucket(new Bucket(3.0, 20.0, 1));
            h.AddBucket(new Bucket(20.0, Double.PositiveInfinity, 1));

            Assert.AreEqual(5, h.DataCount);
        }

        /// <summary>
        /// Can create equal spaced histogram.
        /// </summary>
        [Test]
        public void CanCreateEqualSpacedHistogram()
        {
            var h = new Histogram(new[] {1.0, 5.0, 10.0}, 2);
        }

        /// <summary>
        /// Fail create equal spaced histogram with no data.
        /// </summary>
        [Test]
        public void FailCreateEqualSpacedHistogramWithNoData()
        {
            Assert.That(() => new Histogram(new List<double>(), 10), Throws.ArgumentException);
        }

        /// <summary>
        /// Can create equal spaced histogram with given lower and upper bounds.
        /// </summary>
        [Test]
        public void CanCreateEqualSpacedHistogramWithGivenLowerAndUpperBound()
        {
            var h = new Histogram(new[] {1.0, 5.0, 10.0}, 2, 0.0, 20.0);
        }

        /// <summary>
        /// Can add data single.
        /// </summary>
        [Test]
        public void CanAddDataSingle()
        {
            var h = new Histogram(new[] {1.0, 5.0, 10.0}, 2);
            h.AddData(7.0);
            Assert.AreEqual(2, h[1].Count);
        }

        /// <summary>
        /// Can add data list.
        /// </summary>
        [Test]
        public void CanAddDataList()
        {
            var h = new Histogram(new[] {1.0, 5.0, 10.0}, 2);
            h.AddData(new[] {7.0, 8.0});
            Assert.AreEqual(3, h[1].Count);
        }

        /// <summary>
        /// Add data increase upper bound.
        /// </summary>
        [Test]
        public void AddDataIncreasesUpperBound()
        {
            var h = new Histogram(new[] {1.0, 5.0, 10.0}, 2);
            h.AddData(20.0);
            Assert.AreEqual(2, h[1].Count);
        }

        /// <summary>
        /// Add data decrease lower bound.
        /// </summary>
        [Test]
        public void AddDataDecreasesLowerBound()
        {
            var h = new Histogram(new[] {1.0, 5.0, 10.0}, 2);
            h.AddData(0.0);
            Assert.AreEqual(3, h[0].Count);
        }

        /// <summary>
        /// Add data equal to the lower bound of a histogram.
        /// </summary>
        [Test]
        public void AddDataEqualToLowerBound()
        {
            var h = new Histogram(new[] { 1.0, 5.0, 10.0 }, 3, 0.0, 10.0);
            Assert.DoesNotThrow(() => h.AddData(0.0));

            Assert.AreEqual(2, h[0].Count);
        }

        /// <summary>
        /// Small dataset histogram without bounds.
        /// </summary>
        [Test]
        public void SmallDatasetHistogramWithoutBounds()
        {
            var hist = new Histogram(_smallDataset, 9);

            Assert.AreEqual(9, hist.BucketCount);

            for (var i = 1; i < 9; i++)
            {
                Assert.AreEqual(1.0, hist[i].Count);
            }

            Assert.AreEqual(2.0, hist[0].Count);

            Assert.AreEqual(0.5.Decrement(), hist.LowerBound);
            Assert.AreEqual(9.5, hist.UpperBound);
        }

        /// <summary>
        /// Small dataset histogram with bounds.
        /// </summary>
        [Test]
        public void SmallDatasetHistogramWithBounds()
        {
            var hist = new Histogram(_smallDataset, 10, 0.0, 10.0);

            Assert.AreEqual(10, hist.BucketCount);

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(1.0, hist[i].Count);
            }

            Assert.AreEqual(0.0, hist.LowerBound);
            Assert.AreEqual(10.0, hist.UpperBound);
        }



        /// <summary>
        /// Dataset of small values histogram without bounds.
        /// </summary>
        [Test]
        public void SmallValuesHistogramWithoutBounds()
        {
           var hist = new Histogram(_smallValueDataset, 9);

           Assert.AreEqual(9, hist.BucketCount);

           for (var i = 1; i < 9; i++)
           {
              Assert.AreEqual(1.0, hist[i].Count);
           }

           Assert.AreEqual(2.0, hist[0].Count);

           Assert.AreEqual(0.5e-22.Decrement(), hist.LowerBound);
           Assert.AreEqual(9.5e-22, hist.UpperBound);
        }

        /// <summary>
        /// Dataset of small values histogram with bounds.
        /// </summary>
        [Test]
        public void SmallValuesHistogramWithBounds()
        {
           var hist = new Histogram(_smallValueDataset, 10, 0.0, 10e-22);

           Assert.AreEqual(10, hist.BucketCount);

           for (var i = 0; i < 10; i++)
           {
              Assert.AreEqual(1.0, hist[i].Count);
           }

           Assert.AreEqual(0.0, hist.LowerBound);
           Assert.AreEqual(10.0e-22, hist.UpperBound);
        }

        /// <summary>
        /// Attempt to construct a dataset with small valued buckets
        /// </summary>
        [Test]
        public void SmallValuesManyBucketsHistogramWithBounds()
        {
           var hist = new Histogram(_smallValueDataset, 100, 0.0, 10e-22);

           Assert.AreEqual(100, hist.BucketCount);
           Assert.AreEqual(0.0, hist.LowerBound);
           Assert.AreEqual(10.0e-22, hist.UpperBound);
        }
    }
}
