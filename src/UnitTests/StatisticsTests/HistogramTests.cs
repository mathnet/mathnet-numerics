// <copyright file="HistogramTests.cs" company="Math.NET">
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
    public class HistogramTests
    {
        private double[] smallDataset = { 0.5, 1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5, 8.5, 9.5 };

        #region BucketTests
        [Test]
        public void CanCreateEmptyBucket()
        {
            var b = new Bucket(0.0, 1.0);
        }

        [Test]
        public void CanCreateFilledBucket()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
        }

        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void EmptyBucketWithBadBoundsFails()
        {
            var b = new Bucket(1.0, 0.5);
        }

        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void EmptyBucketWithBadCountFails()
        {
            var b = new Bucket(1.0, 0.5, -1.0);
        }

        [Test]
        public void CanGetBucketWidth()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
            Assert.AreEqual(1.0, b.Width);
        }

        [Test]
        public void CanGetBucketCount()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
            Assert.AreEqual(10.0, b.Count);
        }

        [Test]
        public void CanGetBucketLowerBound()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
            Assert.AreEqual(0.0, b.LowerBound);
        }

        [Test]
        public void CanGetBucketUpperBound()
        {
            var b = new Bucket(0.0, 1.0, 10.0);
            Assert.AreEqual(1.0, b.UpperBound);
        }

        [Test]
        [Row(1.0, 0)]
        [Row(2.0, 1)]
        [Row(-1.0, -1)]
        public void ValidateContains(double x, int r)
        {
            var b = new Bucket(0.0, 1.5, 10.0);
            Assert.AreEqual(r, b.Contains(x));
        }

        #endregion



        #region
        [Test]
        public void CanCreateEmptyHistogram()
        {
            var h = new Histogram();
        }

        [Test]
        public void CanAddBucket()
        {
            var h = new Histogram();
            h.AddBucket(new Bucket(0.0, 1.0));
        }

        [Test]
        [Row(0.0, 0)]
        [Row(0.5, 0)]
        [Row(1.0, 1)]
        [Row(10.0, 3)]
        [Row(10000.0, 4)]
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

        [Test]
        [MultipleAsserts]
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

        [Test]
        public void CanCreateEqualSpacedHistogram()
        {
            var h = new Histogram(new double[] { 1.0, 5.0, 10.0 }, 2);
        }

        [Test]
        [ExpectedArgumentException]
        public void FailCreateEqualSpacedHistogramWithNoData()
        {
            var h = new Histogram(new List<double>(), 10);
        }

        [Test]
        public void CanCreateEqualSpacedHistogramWithGivenLowerAndUpperBound()
        {
            var h = new Histogram(new double[] { 1.0, 5.0, 10.0 }, 2, 0.0, 20.0);
        }

        [Test]
        public void CanAddDataSingle()
        {
            var h = new Histogram(new double[] { 1.0, 5.0, 10.0 }, 2);
            h.AddData(7.0);
            Assert.AreEqual(2, h[1].Count);
        }

        [Test]
        public void CanAddDataList()
        {
            var h = new Histogram(new double[] { 1.0, 5.0, 10.0 }, 2);
            h.AddData(new double[] { 7.0, 8.0} );
            Assert.AreEqual(3, h[1].Count);
        }

        [Test]
        public void AddDataIncreasesUpperBound()
        {
            var h = new Histogram(new double[] { 1.0, 5.0, 10.0 }, 2);
            h.AddData(20.0);
            Assert.AreEqual(2, h[1].Count);
        }

        [Test]
        public void AddDataDecreasesLowerBound()
        {
            var h = new Histogram(new double[] { 1.0, 5.0, 10.0 }, 2);
            h.AddData(0.0);
            Assert.AreEqual(3, h[0].Count);
        }

        [Test]
        public void SmallDatasetHistogramWithoutBounds()
        {
            Histogram hist = new Histogram(smallDataset, 9);

            Assert.AreEqual(9, hist.BucketCount);

            Console.WriteLine("{0}", hist);

            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine("{0} : {1}", i, hist[i].Count);
                Assert.AreEqual(1.0, hist[i].Count);
            }
            Assert.AreEqual(2.0, hist[8].Count);

            Assert.AreEqual(0.5, hist.LowerBound);
            Assert.AreEqual(9.5.Increment(), hist.UpperBound);
        }

        [Test]
        public void SmallDatasetHistogramWithBounds()
        {
            Histogram hist = new Histogram(smallDataset, 10, 0.0, 10.0);

            Assert.AreEqual(10, hist.BucketCount);

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(1.0, hist[i].Count);
            }

            Assert.AreEqual(0.0, hist.LowerBound);
            Assert.AreEqual(10.0, hist.UpperBound);
        }

        #endregion
    }
}
