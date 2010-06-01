//Hany committing
// <copyright file="SortingTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests
{
    using System;
    using System.Collections.Generic;
    using MbUnit.Framework;
    using Numerics;

    [TestFixture]
    public class SortingTests
    {
        [Test]
        public void TestRandomTupleArraySorting()
        {
            const int Len = 0x1 << 10;
            var random = new Random();

            int[] keys = new int[Len];
            int[] items = new int[Len];
            int[] keysCopy = new int[Len];

            for(int i = 0; i < keys.Length; i++)
            {
                keys[i] = random.Next();
                keysCopy[i] = keys[i];
                items[i] = -keys[i];
            }

            Sorting.Sort(keys, items);

            for(int i = 1; i < keys.Length; i++)
            {
                Assert.IsTrue(keys[i] >= keys[i - 1], "Sort Order - " + i.ToString());
                Assert.AreEqual(items[i], -keys[i], "Items Permutation - " + i.ToString());
            }

            for(int i = 0; i < keysCopy.Length; i++)
            {
                Assert.IsTrue(Array.IndexOf(keys, keysCopy[i]) >= 0, "All keys still there - " + i.ToString());
            }
        }

        [Test]
        public void TestRandomTupleListSorting()
        {
            const int Len = 0x1 << 10;
            var random = new Random();

            List<int> keys = new List<int>(Len);
            List<int> items = new List<int>(Len);
            int[] keysCopy = new int[Len];

            for(int i = 0; i < Len; i++)
            {
                int value = random.Next();
                keys.Add(value);
                keysCopy[i] = value;
                items.Add(-value);
            }

            Sorting.Sort(keys, items);

            for(int i = 1; i < Len; i++)
            {
                Assert.IsTrue(keys[i] >= keys[i - 1], "Sort Order - " + i.ToString());
                Assert.AreEqual(items[i], -keys[i], "Items Permutation - " + i.ToString());
            }

            for(int i = 0; i < keysCopy.Length; i++)
            {
                Assert.IsTrue(keys.IndexOf(keysCopy[i]) >= 0, "All keys still there - " + i.ToString());
            }
        }

        [Test]
        public void TestRandomTripleArraySorting()
        {
            const int Len = 0x1 << 10;
            var random = new Random();

            int[] keys = new int[Len];
            int[] items1 = new int[Len];
            int[] items2 = new int[Len];
            int[] keysCopy = new int[Len];

            for(int i = 0; i < keys.Length; i++)
            {
                keys[i] = random.Next();
                keysCopy[i] = keys[i];
                items1[i] = -keys[i];
                items2[i] = keys[i] >> 2;
            }

            Sorting.Sort(keys, items1, items2);

            for(int i = 1; i < keys.Length; i++)
            {
                Assert.IsTrue(keys[i] >= keys[i - 1], "Sort Order - " + i.ToString());
                Assert.AreEqual(items1[i], -keys[i], "Items1 Permutation - " + i.ToString());
                Assert.AreEqual(items2[i], keys[i] >> 2, "Items2 Permutation - " + i.ToString());
            }

            for(int i = 0; i < keysCopy.Length; i++)
            {
                Assert.IsTrue(Array.IndexOf(keys, keysCopy[i]) >= 0, "All keys still there - " + i.ToString());
            }
        }

        [Test]
        public void TestAppliedListSorting()
        {
            const int Len = 0x1 << 10;
            var random = new Random();

            List<int> list = new List<int>();

            for(int i = 0; i < Len; i++)
            {
                list.Add(random.Next());
            }

            // default sorting (Ascending)
            list.Sort();

            // just check that the order is as expected, not that the items are correct
            for(int i = 1; i < list.Count; i++)
            {
                Assert.IsTrue(list[i] >= list[i - 1], "Sort Order - " + i.ToString());
            }
        }
    }
}
