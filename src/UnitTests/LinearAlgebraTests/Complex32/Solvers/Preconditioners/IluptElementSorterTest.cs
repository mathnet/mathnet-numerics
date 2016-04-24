// <copyright file="IluptElementSorterTest.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra.Complex32;
using MathNet.Numerics.LinearAlgebra.Complex32.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.Preconditioners
{
    /// <summary>
    /// Test for element sort algorithm of Ilupt class.
    /// </summary>
    [TestFixture, Category("LASolver")]
    public sealed class IluptElementSorterTest
    {
        /// <summary>
        /// Heap sort with increasing integer array.
        /// </summary>
        [Test]
        public void HeapSortWithIncreasingIntegerArray()
        {
            var sortedIndices = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            ILUTPElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        /// <summary>
        /// Heap sort with decreasing integer array.
        /// </summary>
        [Test]
        public void HeapSortWithDecreasingIntegerArray()
        {
            var sortedIndices = new[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0};
            ILUTPElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        /// <summary>
        /// Heap sort with random integer array.
        /// </summary>
        [Test]
        public void HeapSortWithRandomIntegerArray()
        {
            var sortedIndices = new[] {5, 2, 8, 6, 0, 4, 1, 7, 3, 9};
            ILUTPElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        /// <summary>
        /// Heap sort with duplicate entries.
        /// </summary>
        [Test]
        public void HeapSortWithDuplicateEntries()
        {
            var sortedIndices = new[] {1, 1, 1, 1, 2, 2, 2, 2, 3, 4};
            ILUTPElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(4, sortedIndices[i], "#01-" + i);
                }
                else
                {
                    if (i == 1)
                    {
                        Assert.AreEqual(3, sortedIndices[i], "#01-" + i);
                    }
                    else
                    {
                        if (i < 6)
                        {
                            if (sortedIndices[i] != 2)
                            {
                                Assert.Fail("#01-" + i);
                            }
                        }
                        else
                        {
                            if (sortedIndices[i] != 1)
                            {
                                Assert.Fail("#01-" + i);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Heap sort with special constructed integer array.
        /// </summary>
        [Test]
        public void HeapSortWithSpecialConstructedIntegerArray()
        {
            var sortedIndices = new[] {0, 0, 0, 0, 0, 1, 0, 0, 0, 0};
            ILUTPElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1, sortedIndices[i], "#01-" + i);
                    break;
                }
            }

            sortedIndices = new[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            ILUTPElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1, sortedIndices[i], "#02-" + i);
                    break;
                }
            }

            sortedIndices = new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 1};
            ILUTPElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1, sortedIndices[i], "#03-" + i);
                    break;
                }
            }

            sortedIndices = new[] {1, 1, 1, 0, 1, 1, 1, 1, 1, 1};
            ILUTPElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 9)
                {
                    Assert.AreEqual(0, sortedIndices[i], "#04-" + i);
                    break;
                }
            }
        }

        /// <summary>
        /// Heap sort with increasing double array.
        /// </summary>
        [Test]
        public void HeapSortWithIncreasingDoubleArray()
        {
            var sortedIndices = new int[10];
            var values = new DenseVector(10);
            values[0] = 0;
            values[1] = 1;
            values[2] = 2;
            values[3] = 3;
            values[4] = 4;
            values[5] = 5;
            values[6] = 6;
            values[7] = 7;
            values[8] = 8;
            values[9] = 9;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        /// <summary>
        /// Heap sort with decreasing doubleArray
        /// </summary>
        [Test]
        public void HeapSortWithDecreasingDoubleArray()
        {
            var sortedIndices = new int[10];
            var values = new DenseVector(10);
            values[0] = 9;
            values[1] = 8;
            values[2] = 7;
            values[3] = 6;
            values[4] = 5;
            values[5] = 4;
            values[6] = 3;
            values[7] = 2;
            values[8] = 1;
            values[9] = 0;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(i, sortedIndices[i], "#01-" + i);
            }
        }

        /// <summary>
        /// Heap sort with random double array.
        /// </summary>
        [Test]
        public void HeapSortWithRandomDoubleArray()
        {
            var sortedIndices = new int[10];
            var values = new DenseVector(10);
            values[0] = 5;
            values[1] = 2;
            values[2] = 8;
            values[3] = 6;
            values[4] = 0;
            values[5] = 4;
            values[6] = 1;
            values[7] = 7;
            values[8] = 3;
            values[9] = 9;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(9, sortedIndices[i], "#01-" + i);
                        break;
                    case 1:
                        Assert.AreEqual(2, sortedIndices[i], "#01-" + i);
                        break;
                    case 2:
                        Assert.AreEqual(7, sortedIndices[i], "#01-" + i);
                        break;
                    case 3:
                        Assert.AreEqual(3, sortedIndices[i], "#01-" + i);
                        break;
                    case 4:
                        Assert.AreEqual(0, sortedIndices[i], "#01-" + i);
                        break;
                    case 5:
                        Assert.AreEqual(5, sortedIndices[i], "#01-" + i);
                        break;
                    case 6:
                        Assert.AreEqual(8, sortedIndices[i], "#01-" + i);
                        break;
                    case 7:
                        Assert.AreEqual(1, sortedIndices[i], "#01-" + i);
                        break;
                    case 8:
                        Assert.AreEqual(6, sortedIndices[i], "#01-" + i);
                        break;
                    case 9:
                        Assert.AreEqual(4, sortedIndices[i], "#01-" + i);
                        break;
                }
            }
        }

        /// <summary>
        /// Heap sort with duplicate double entries.
        /// </summary>
        [Test]
        public void HeapSortWithDuplicateDoubleEntries()
        {
            var sortedIndices = new int[10];
            var values = new DenseVector(10);
            values[0] = 1;
            values[1] = 1;
            values[2] = 1;
            values[3] = 1;
            values[4] = 2;
            values[5] = 2;
            values[6] = 2;
            values[7] = 2;
            values[8] = 3;
            values[9] = 4;

            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(9, sortedIndices[i], "#01-" + i);
                }
                else
                {
                    if (i == 1)
                    {
                        Assert.AreEqual(8, sortedIndices[i], "#01-" + i);
                    }
                    else
                    {
                        if (i < 6)
                        {
                            if ((sortedIndices[i] != 4) &&
                                (sortedIndices[i] != 5) &&
                                (sortedIndices[i] != 6) &&
                                (sortedIndices[i] != 7))
                            {
                                Assert.Fail("#01-" + i);
                            }
                        }
                        else
                        {
                            if ((sortedIndices[i] != 0) &&
                                (sortedIndices[i] != 1) &&
                                (sortedIndices[i] != 2) &&
                                (sortedIndices[i] != 3))
                            {
                                Assert.Fail("#01-" + i);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Heap sort with special constructed double array.
        /// </summary>
        [Test]
        public void HeapSortWithSpecialConstructedDoubleArray()
        {
            var sortedIndices = new int[10];
            var values = new DenseVector(10);
            values[0] = 0;
            values[1] = 0;
            values[2] = 0;
            values[3] = 0;
            values[4] = 0;
            values[5] = 1;
            values[6] = 0;
            values[7] = 0;
            values[8] = 0;
            values[9] = 0;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(5, sortedIndices[i], "#01-" + i);
                    break;
                }
            }

            values[0] = 1;
            values[1] = 0;
            values[2] = 0;
            values[3] = 0;
            values[4] = 0;
            values[5] = 0;
            values[6] = 0;
            values[7] = 0;
            values[8] = 0;
            values[9] = 0;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(0, sortedIndices[i], "#02-" + i);
                    break;
                }
            }

            values[0] = 0;
            values[1] = 0;
            values[2] = 0;
            values[3] = 0;
            values[4] = 0;
            values[5] = 0;
            values[6] = 0;
            values[7] = 0;
            values[8] = 0;
            values[9] = 1;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(9, sortedIndices[i], "#03-" + i);
                    break;
                }
            }

            values[0] = 1;
            values[1] = 1;
            values[2] = 1;
            values[3] = 0;
            values[4] = 1;
            values[5] = 1;
            values[6] = 1;
            values[7] = 1;
            values[8] = 1;
            values[9] = 1;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 9)
                {
                    Assert.AreEqual(3, sortedIndices[i], "#04-" + i);
                    break;
                }
            }
        }

        /// <summary>
        /// Heap sort with increasing double array with lower bound
        /// </summary>
        [Test]
        public void HeapSortWithIncreasingDoubleArrayWithLowerBound()
        {
            var sortedIndices = new int[10];
            var values = new DenseVector(10);
            values[0] = 0;
            values[1] = 1;
            values[2] = 2;
            values[3] = 3;
            values[4] = 4;
            values[5] = 5;
            values[6] = 6;
            values[7] = 7;
            values[8] = 8;
            values[9] = 9;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(4, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length - 4; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        /// <summary>
        /// Heap sort with increasing double array with upper bound.
        /// </summary>
        [Test]
        public void HeapSortWithIncreasingDoubleArrayWithUpperBound()
        {
            var sortedIndices = new int[10];
            var values = new DenseVector(10);
            values[0] = 0;
            values[1] = 1;
            values[2] = 2;
            values[3] = 3;
            values[4] = 4;
            values[5] = 5;
            values[6] = 6;
            values[7] = 7;
            values[8] = 8;
            values[9] = 9;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 5, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length - 5; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 5 - i, sortedIndices[i], "#01-" + i);
            }
        }

        /// <summary>
        /// Heap sort with increasing double array with lower and upper bound.
        /// </summary>
        [Test]
        public void HeapSortWithIncreasingDoubleArrayWithLowerAndUpperBound()
        {
            var sortedIndices = new int[10];
            var values = new DenseVector(10);
            values[0] = 0;
            values[1] = 1;
            values[2] = 2;
            values[3] = 3;
            values[4] = 4;
            values[5] = 5;
            values[6] = 6;
            values[7] = 7;
            values[8] = 8;
            values[9] = 9;
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = i;
            }

            ILUTPElementSorter.SortDoubleIndicesDecreasing(2, sortedIndices.Length - 3, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length - 4; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 3 - i, sortedIndices[i], "#01-" + i);
            }
        }
    }
}
