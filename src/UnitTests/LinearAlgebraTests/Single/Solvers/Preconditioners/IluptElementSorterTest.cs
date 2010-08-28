// NOTE: This class file Build Action is not set to Compile by default. Because IlutpElementSorter class is internal. If you want 
// NOTE: to test IlutpElementSorter you should make it public, set Build Action=Compile of this file (in properties) and run tets.
// NOTE: After all tests passed please do all actions vice versa. IlutpElementSorter class is only for internal usage.

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Solvers.Preconditioners
{
    using LinearAlgebra.Generic;
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class IluptElementSorterTest
    {
        [Test]
        [MultipleAsserts]
        public void HeapSortWithIncreasingIntergerArray()
        {
            var sortedIndices = new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            IlutpElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithDecreasingIntegerArray()
        {
            var sortedIndices = new [] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
            IlutpElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithRandomIntegerArray()
        {
            var sortedIndices = new []{ 5, 2, 8, 6, 0, 4, 1, 7, 3, 9 };
            IlutpElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithDuplicateEntries()
        {
            var sortedIndices = new []{ 1, 1, 1, 1, 2, 2, 2, 2, 3, 4 };
            IlutpElementSorter.SortIntegersDecreasing(sortedIndices);
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

        [Test]
        [MultipleAsserts]
        public void HeapSortWithSpecialConstructedIntegerArray()
        {
            var sortedIndices = new []{ 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 };
            IlutpElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1, sortedIndices[i], "#01-" + i);
                    break;
                }
            }

            sortedIndices = new []{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            IlutpElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1, sortedIndices[i], "#02-" + i);
                    break;
                }
            }

            sortedIndices = new []{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
            IlutpElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1, sortedIndices[i], "#03-" + i);
                    break;
                }
            }

            sortedIndices = new []{ 1, 1, 1, 0, 1, 1, 1, 1, 1, 1 };
            IlutpElementSorter.SortIntegersDecreasing(sortedIndices);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 9)
                {
                    Assert.AreEqual(0, sortedIndices[i], "#04-" + i);
                    break;
                }
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithIncreasingDoubleArray()
        {
            var sortedIndices = new int[10];
            Vector<float> values = new DenseVector(10);
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

            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithDecreasingDoubleArray()
        {
            var sortedIndices = new int[10];
            Vector<float>  values = new DenseVector(10);
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

            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                Assert.AreEqual(i, sortedIndices[i], "#01-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithRandomDoubleArray()
        {
            var sortedIndices = new int[10];
            Vector<float>  values = new DenseVector(10);
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

            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
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

        [Test]
        [MultipleAsserts]
        public void HeapSortWithDuplicateDoubleEntries()
        {
            var sortedIndices = new int[10];
            Vector<float>  values = new DenseVector(10);
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
            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
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

        [Test]
        [MultipleAsserts]
        public void HeapSortWithSpecialConstructedDoubleArray()
        {
            var sortedIndices = new int[10];
            Vector<float>  values = new DenseVector(10);
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
            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
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
            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
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
            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
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
            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length; i++)
            {
                if (i == 9)
                {
                    Assert.AreEqual(3, sortedIndices[i], "#04-" + i);
                    break;
                }
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithIncreasingDoubleArrayWithLowerBound()
        {
            var sortedIndices = new int[10];
            Vector<float>  values = new DenseVector(10);
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

            IlutpElementSorter.SortDoubleIndicesDecreasing(4, sortedIndices.Length - 1, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length - 4; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 1 - i, sortedIndices[i], "#01-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithIncreasingDoubleArrayWithUpperBound()
        {
            var sortedIndices = new int[10];
            Vector<float>  values = new DenseVector(10);
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

            IlutpElementSorter.SortDoubleIndicesDecreasing(0, sortedIndices.Length - 5, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length - 5; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 5 - i, sortedIndices[i], "#01-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void HeapSortWithIncreasingDoubleArrayWithLowerAndUpperBound()
        {
            var sortedIndices = new int[10];
            Vector<float>  values = new DenseVector(10);
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

            IlutpElementSorter.SortDoubleIndicesDecreasing(2, sortedIndices.Length - 3, sortedIndices, values);
            for (var i = 0; i < sortedIndices.Length - 4; i++)
            {
                Assert.AreEqual(sortedIndices.Length - 3 - i, sortedIndices[i], "#01-" + i);
            }
        }
    }
}
