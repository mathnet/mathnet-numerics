// <copyright file="OrderedShiftBufferTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.Filtering
{
    using System;
    using System.Collections.Generic;
    using MathNet.Numerics.Filtering.Utils;
    using NUnit.Framework;

    [TestFixture]
    public class OrderedShiftBufferTest
    {
        [Test]
        public void TestInitialize()
        {
            OrderedShiftBuffer b = new OrderedShiftBuffer(4);
            Assert.AreEqual(4, b.InitializedCount, "A1");
            Assert.AreEqual(0, b.ActualCount, "A2");
            Assert.AreEqual(false, b.IsInitialized, "A3");

            b.Append(1d);
            Assert.AreEqual(4, b.InitializedCount, "B1");
            Assert.AreEqual(1, b.ActualCount, "B2");
            Assert.AreEqual(false, b.IsInitialized, "B3");

            b.Append(2d);
            Assert.AreEqual(4, b.InitializedCount, "C1");
            Assert.AreEqual(2, b.ActualCount, "C2");
            Assert.AreEqual(false, b.IsInitialized, "C3");

            b.Append(3d);
            Assert.AreEqual(4, b.InitializedCount, "D1");
            Assert.AreEqual(3, b.ActualCount, "D2");
            Assert.AreEqual(false, b.IsInitialized, "D3");

            b.Append(4d);
            Assert.AreEqual(4, b.InitializedCount, "E1");
            Assert.AreEqual(4, b.ActualCount, "E2");
            Assert.AreEqual(true, b.IsInitialized, "E3");

            b.Append(5d);
            Assert.AreEqual(4, b.InitializedCount, "F1");
            Assert.AreEqual(4, b.ActualCount, "F2");
            Assert.AreEqual(true, b.IsInitialized, "F3");
        }

        [Test]
        public void TestMedian()
        {
            OrderedShiftBuffer b = new OrderedShiftBuffer(3);

            try
            {
                double d = b.Median;
                Assert.Fail("A1");
            }
            catch(NullReferenceException) { }

            b.Append(1.0);
            Assert.AreEqual(1.0, b.Median, "B1");
            b.Append(2.0);
            Assert.AreEqual(2.0, b.Median, "B2");
            b.Append(3.0);
            Assert.AreEqual(2.0, b.Median, "B3");
            b.Append(1.5);
            Assert.AreEqual(2.0, b.Median, "B4");
            b.Append(2.5);
            Assert.AreEqual(2.5, b.Median, "B5");
            b.Append(3.5);
            Assert.AreEqual(2.5, b.Median, "B6");
            b.Append(4.5);
            Assert.AreEqual(3.5, b.Median, "B7");
            b.Append(5.0);
            Assert.AreEqual(4.5, b.Median, "B8");
            b.Append(1.0);
            Assert.AreEqual(4.5, b.Median, "B9");
        }

        [Test]
        public void TestIterators()
        {
            OrderedShiftBuffer b = new OrderedShiftBuffer(3);
            b.Append(4.0);
            b.Append(1.0);
            b.Append(3.0);
            b.Append(2.0);

            IEnumerator<double> byInsert = b.ByInsertOrder.GetEnumerator();
            Assert.IsTrue(byInsert.MoveNext(),"A1");
            Assert.AreEqual(2.0, byInsert.Current, "A2");
            Assert.IsTrue(byInsert.MoveNext(), "B1");
            Assert.AreEqual(3.0, byInsert.Current, "B2");
            Assert.IsTrue(byInsert.MoveNext(), "C1");
            Assert.AreEqual(1.0, byInsert.Current, "C2");
            Assert.IsFalse(byInsert.MoveNext(), "D1");

            IEnumerator<double> byValue = b.ByValueOrder.GetEnumerator();
            Assert.IsTrue(byValue.MoveNext(), "E1");
            Assert.AreEqual(1.0, byValue.Current, "E2");
            Assert.IsTrue(byValue.MoveNext(), "F1");
            Assert.AreEqual(2.0, byValue.Current, "F2");
            Assert.IsTrue(byValue.MoveNext(), "G1");
            Assert.AreEqual(3.0, byValue.Current, "G2");
            Assert.IsFalse(byValue.MoveNext(), "H1");
        }
    }
}
