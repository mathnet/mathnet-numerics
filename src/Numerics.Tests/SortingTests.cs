// <copyright file="SortingTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests
{

    /// <summary>
    /// Sorting tests.
    /// </summary>
    [TestFixture]
    public class SortingTests
    {
        /// <summary>
        /// Test random tuple array sorting.
        /// </summary>
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(7)]
        [TestCase(24)]
        [TestCase(1024)]
        public void TestRandomTupleArraySorting(int length)
        {
            var random = new System.Random(0);

            var keys = new int[length];
            var items = new int[length];
            var keysCopy = new int[length];

            for (var i = 0; i < keys.Length; i++)
            {
                keys[i] = random.Next();
                keysCopy[i] = keys[i];
                items[i] = -keys[i];
            }

            Sorting.Sort(keys, items);

            for (var i = 1; i < keys.Length; i++)
            {
                Assert.IsTrue(keys[i] >= keys[i - 1], "Sort Order - " + i);
                Assert.AreEqual(items[i], -keys[i], "Items Permutation - " + i);
            }

            for (var i = 0; i < keysCopy.Length; i++)
            {
                Assert.IsTrue(Array.IndexOf(keys, keysCopy[i]) >= 0, "All keys still there - " + i);
            }
        }

        /// <summary>
        /// Test random tuple list sorting.
        /// </summary>
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(7)]
        [TestCase(24)]
        [TestCase(1024)]
        public void TestRandomTupleListSorting(int length)
        {
            var random = new System.Random(0);

            var keys = new List<int>(length);
            var items = new List<int>(length);
            var keysCopy = new int[length];

            for (var i = 0; i < length; i++)
            {
                var value = random.Next();
                keys.Add(value);
                keysCopy[i] = value;
                items.Add(-value);
            }

            Sorting.Sort(keys, items);

            for (var i = 1; i < length; i++)
            {
                Assert.IsTrue(keys[i] >= keys[i - 1], "Sort Order - " + i);
                Assert.AreEqual(items[i], -keys[i], "Items Permutation - " + i);
            }

            for (var i = 0; i < keysCopy.Length; i++)
            {
                Assert.IsTrue(keys.IndexOf(keysCopy[i]) >= 0, "All keys still there - " + i);
            }
        }

        /// <summary>
        /// Test random triple array sorting.
        /// </summary>
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(7)]
        [TestCase(24)]
        [TestCase(1024)]
        public void TestRandomTripleArraySorting(int length)
        {
            var random = new System.Random(0);

            var keys = new int[length];
            var items1 = new int[length];
            var items2 = new int[length];
            var keysCopy = new int[length];

            for (var i = 0; i < keys.Length; i++)
            {
                keys[i] = random.Next();
                keysCopy[i] = keys[i];
                items1[i] = -keys[i];
                items2[i] = keys[i] >> 2;
            }

            Sorting.Sort(keys, items1, items2);

            for (var i = 1; i < keys.Length; i++)
            {
                Assert.IsTrue(keys[i] >= keys[i - 1], "Sort Order - " + i);
                Assert.AreEqual(items1[i], -keys[i], "Items1 Permutation - " + i);
                Assert.AreEqual(items2[i], keys[i] >> 2, "Items2 Permutation - " + i);
            }

            for (var i = 0; i < keysCopy.Length; i++)
            {
                Assert.IsTrue(Array.IndexOf(keys, keysCopy[i]) >= 0, "All keys still there - " + i);
            }
        }

        /// <summary>
        /// Test applied list sorting.
        /// </summary>
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(7)]
        [TestCase(24)]
        [TestCase(1024)]
        public void TestAppliedListSorting(int length)
        {
            var random = new System.Random(0);

            var list = new List<int>();

            for (var i = 0; i < length; i++)
            {
                list.Add(random.Next());
            }

            // default sorting (Ascending)
            list.Sort();

            // just check that the order is as expected, not that the items are correct
            for (var i = 1; i < list.Count; i++)
            {
                Assert.IsTrue(list[i] >= list[i - 1], "Sort Order - " + i);
            }
        }
    }
}
