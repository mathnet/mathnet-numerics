// <copyright file="Sorting.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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

namespace MathNet.Numerics
{
    /// <summary>
    /// Sorting algorithms for single, tuple and triple lists.
    /// </summary>
    public static class Sorting
    {
        /// <summary>
        /// Sort a list of keys, in place using the quick sort algorithm using the introsort algorithm.
        /// </summary>
        /// <typeparam name="T">The type of elements in the key list.</typeparam>
        /// <param name="keys">List to sort.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static void Sort<T>(IList<T> keys, IComparer<T> comparer = null)
        {
            int count = keys.Count;

            if (null == comparer)
            {
                comparer = Comparer<T>.Default;
            }

            // array case
            if (keys is T[] keysArray)
            {
                Array.Sort(keysArray, comparer);
                return;
            }

            // generic list case
            if (keys is List<T> keysList)
            {
                keysList.Sort(comparer);
                return;
            }

            // local sort implementation
            IntroSort<T>(keys, comparer, 0, count - 1, (int a, int b) => Swap(keys, a, b));
        }

        /// <summary>
        /// Sort a list of keys and items with respect to the keys, in place using the introsort algorithm.
        /// </summary>
        /// <typeparam name="TKey">The type of elements in the key list.</typeparam>
        /// <typeparam name="TItem">The type of elements in the item list.</typeparam>
        /// <param name="keys">List to sort.</param>
        /// <param name="items">List to permute the same way as the key list.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static void Sort<TKey, TItem>(IList<TKey> keys, IList<TItem> items, IComparer<TKey> comparer = null)
        {
            int count = keys.Count;

            if (null == comparer)
            {
                comparer = Comparer<TKey>.Default;
            }

            // array case
            if (keys is TKey[] keysArray && items is TItem[] itemsArray)
            {
                Array.Sort(keysArray, itemsArray, comparer);
                return;
            }

            // local sort implementation
            Action<int, int> swap = (int a, int b) =>
            {
                Swap<TKey>(keys, a, b);
                Swap<TItem>(items, a, b);
            };
            IntroSort(keys, comparer, 0, count - 1, swap);
        }

        /// <summary>
        /// Sort a list of keys, items1 and items2 with respect to the keys, in place using the quick sort algorithm.
        /// </summary>
        /// <typeparam name="TKey">The type of elements in the key list.</typeparam>
        /// <typeparam name="TItem1">The type of elements in the first item list.</typeparam>
        /// <typeparam name="TItem2">The type of elements in the second item list.</typeparam>
        /// <param name="keys">List to sort.</param>
        /// <param name="items1">First list to permute the same way as the key list.</param>
        /// <param name="items2">Second list to permute the same way as the key list.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static void Sort<TKey, TItem1, TItem2>(IList<TKey> keys, IList<TItem1> items1, IList<TItem2> items2, IComparer<TKey> comparer = null)
        {
            int count = keys.Count;

            if (null == comparer)
            {
                comparer = Comparer<TKey>.Default;
            }

            // local sort implementation
            Action<int, int> swap = (int a, int b) =>
            {
                Swap<TKey>(keys, a, b);
                Swap<TItem1>(items1, a, b);
                Swap<TItem2>(items2, a, b);
            };
            IntroSort(keys, comparer, 0, count - 1, swap);
        }

        /// <summary>
        /// Sort a range of a list of keys, in place using the quick sort algorithm.
        /// </summary>
        /// <typeparam name="T">The type of element in the list.</typeparam>
        /// <param name="keys">List to sort.</param>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="count">The length of the range to sort.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static void Sort<T>(IList<T> keys, int index, int count, IComparer<T> comparer = null)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || index + count > keys.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (null == comparer)
            {
                comparer = Comparer<T>.Default;
            }

            // array case
            if (keys is T[] keysArray)
            {
                Array.Sort(keysArray, index, count, comparer);
                return;
            }

            // generic list case
            if (keys is List<T> keysList)
            {
                keysList.Sort(index, count, comparer);
                return;
            }

            // fall back: local sort implementation
            IntroSort(keys, comparer, index, index + count, (int a, int b) => Swap(keys, a, b));
        }

        /// <summary>
        /// Sort a list of keys and items with respect to the keys, in place using the quick sort algorithm.
        /// </summary>
        /// <typeparam name="TKey">The type of elements in the key list.</typeparam>
        /// <typeparam name="TItem">The type of elements in the item list.</typeparam>
        /// <param name="keys">List to sort.</param>
        /// <param name="items">List to permute the same way as the key list.</param>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="count">The length of the range to sort.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static void Sort<TKey, TItem>(IList<TKey> keys, IList<TItem> items, int index, int count, IComparer<TKey> comparer = null)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || index + count > keys.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (null == comparer)
            {
                comparer = Comparer<TKey>.Default;
            }

            // fall back: local sort implementation
            Action<int, int> swap = (int a, int b) =>
            {
                Swap<TKey>(keys, a, b);
                Swap<TItem>(items, a, b);
            };
            IntroSort(keys, comparer, index, index + count, swap);
        }

        /// <summary>
        /// Sort a list of keys, items1 and items2 with respect to the keys, in place using the quick sort algorithm.
        /// </summary>
        /// <typeparam name="TKey">The type of elements in the key list.</typeparam>
        /// <typeparam name="TItem1">The type of elements in the first item list.</typeparam>
        /// <typeparam name="TItem2">The type of elements in the second item list.</typeparam>
        /// <param name="keys">List to sort.</param>
        /// <param name="items1">First list to permute the same way as the key list.</param>
        /// <param name="items2">Second list to permute the same way as the key list.</param>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="count">The length of the range to sort.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static void Sort<TKey, TItem1, TItem2>(IList<TKey> keys, IList<TItem1> items1, IList<TItem2> items2, int index, int count, IComparer<TKey> comparer = null)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || index + count > keys.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (null == comparer)
            {
                comparer = Comparer<TKey>.Default;
            }

            // fall back: local sort implementation
            Action<int, int> swap = (int a, int b) =>
            {
                Swap<TKey>(keys, a, b);
                Swap<TItem1>(items1, a, b);
                Swap<TItem2>(items2, a, b);
            };
            IntroSort(keys, comparer, index, index + count, swap);
        }

        /// <summary>
        /// Sort a list of keys and items with respect to the keys, in place using the quick sort algorithm.
        /// </summary>
        /// <typeparam name="T1">The type of elements in the primary list.</typeparam>
        /// <typeparam name="T2">The type of elements in the secondary list.</typeparam>
        /// <param name="primary">List to sort.</param>
        /// <param name="secondary">List to sort on duplicate primary items, and permute the same way as the key list.</param>
        /// <param name="primaryComparer">Comparison, defining the primary sort order.</param>
        /// <param name="secondaryComparer">Comparison, defining the secondary sort order.</param>
        public static void SortAll<T1, T2>(IList<T1> primary, IList<T2> secondary, IComparer<T1> primaryComparer = null, IComparer<T2> secondaryComparer = null)
        {
            if (null == primaryComparer)
            {
                primaryComparer = Comparer<T1>.Default;
            }

            if (null == secondaryComparer)
            {
                secondaryComparer = Comparer<T2>.Default;
            }

            // local sort implementation
            QuickSortAll(primary, secondary, primaryComparer, secondaryComparer, 0, primary.Count - 1);
        }

        /// <summary>
        /// Recursively reorders the given list to satisfy the max heap property.
        /// </summary>
        /// <typeparam name="TKey">The type of elements in the key list.</typeparam>
        /// <param name="keys">The list which is turned into a heap.</param>
        /// <param name="comparer">The method with which to compare two elements of the heap.</param>
        /// <param name="i">The index of the heap to heapify.</param>
        /// <param name="left">The left boundary of the heapify.</param>
        /// <param name="right">The right boundary of the heapify.</param>
        /// <param name="swapper">An Action which takes the given indexes and swaps the key and satellite data.</param>
        static void MaxHeapify<TKey>(IList<TKey> keys, IComparer<TKey> comparer, int i, int left, int right, Action<int, int> swapper)
        {
            i -= left;
            int leftChild = 2 * i + 1;
            int rightChild = 2 * i + 2;

            int largest = i;

            if (leftChild + left >= left && leftChild + left <= right && comparer.Compare(keys[leftChild + left], keys[largest + left]) > 0)
            {
                largest = leftChild;
            }

            if (rightChild + left >= left && rightChild + left <= right && comparer.Compare(keys[rightChild + left], keys[largest + left]) > 0)
            {
                largest = rightChild;
            }

            if (largest != i)
            {
                swapper(largest + left, i + left);
                MaxHeapify(keys, comparer, largest + left, left, right, swapper);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey">The type of elements in the key list.</typeparam>
        /// <param name="keys">The list which is turned into a heap.</param>
        /// <param name="comparer">The method with which to compare two elements of the heap.</param>
        /// <param name="left">The left boundary of the heap.</param>
        /// <param name="right">The right boundary of the heap.</param>
        /// <param name="swapper">An Action which takes the given indexes and swaps the key and satellite data.</param>

        static void BuildMaxHeap<TKey>(IList<TKey> keys, IComparer<TKey> comparer, int left, int right, Action<int, int> swapper)
        {
            for (int i = (right - left) / 2; i >= 0; i--)
            {
                MaxHeapify(keys, comparer, i + left, left, right, swapper);
            }
        }

        /// <summary>
        /// Recursive implementation for an in place introspective sort on a list.
        /// </summary>
        /// <typeparam name="TKey">The type of elements in the key list.</typeparam>
        /// <param name="keys">The list which is sorted using intro sort.</param>
        /// <param name="comparer">The method with which to compare two elements of the intro sort.</param>
        /// <param name="left">The left boundary of the intro sort.</param>
        /// <param name="right">The right boundary of the intro sort.</param>
        /// <param name="swapper">An Action which takes the given indexes and swaps the key and satellite data.</param>
        /// <param name="recursions">Tracks the number of recursions entered.</param>
        static void IntroSort<TKey>(IList<TKey> keys, IComparer<TKey> comparer, int left, int right, Action<int, int> swapper, int recursions = 0)
        {
            const double ln2 = 0.69314718056; // Natural Logarithm of 2
            if (left >= right)
            {
                return;
            }

            Random.CryptoRandomSource rand = new MathNet.Numerics.Random.CryptoRandomSource();
            double max_recursion_depth = 2 * Math.Log(keys.Count) / ln2; // This is the cap on recursion depth used by the GNU STL

            if (right - left < 16) // Insertion Sort is faster on very small sequences, 16 is the number that Array.Sort uses
            {
                for (int i = left + 1; i <= right; i++)
                {
                    int j = i;
                    while (j > 0 && comparer.Compare(keys[j - 1], keys[j]) > 0)
                    {
                        swapper(j, j - 1);
                        j--;
                    }
                }
            }
            else if (recursions > max_recursion_depth) // Heapsort is guaranteed O(n log n) 
            {
                BuildMaxHeap(keys, comparer, left, right, swapper);

                int heapBoundary = right;

                for (int i = right - left; i > 0; i--)
                {
                    swapper(left, left + i);
                    MaxHeapify(keys, comparer, left, left, --heapBoundary, swapper);
                }
            }
            else // Quicksort
            {
                int pivot_index = (rand.Next() % (right - left)) + left; // Don't need to worry about negatives because rand.Next() returns a non-negative number
                swapper(pivot_index, right);

                TKey pivot = keys[right];

                int i = left - 1;
                for (int j = left; j < right; j++)
                {
                    if (comparer.Compare(keys[j], pivot) <= 0)
                    {
                        i++;
                        swapper(i, j);
                    }
                }

                i++;
                swapper(i, right);

                recursions++;
                IntroSort(keys, comparer, left, i - 1, swapper, recursions);
                IntroSort(keys, comparer, i + 1, right, swapper, recursions);
            }

        }

        /// <summary>
        /// Recursive implementation for an in place quick sort on a list.
        /// </summary>
        /// <typeparam name="T">The type of the list on which the quick sort is performed.</typeparam>
        /// <param name="keys">The list which is sorted using quick sort.</param>
        /// <param name="comparer">The method with which to compare two elements of the quick sort.</param>
        /// <param name="left">The left boundary of the quick sort.</param>
        /// <param name="right">The right boundary of the quick sort.</param>
        static void QuickSort<T>(IList<T> keys, IComparer<T> comparer, int left, int right)
        {
            do
            {
                // Pivoting
                int a = left;
                int b = right;
                int p = a + ((b - a) >> 1); // midpoint

                if (comparer.Compare(keys[a], keys[p]) > 0)
                {
                    Swap(keys, a, p);
                }

                if (comparer.Compare(keys[a], keys[b]) > 0)
                {
                    Swap(keys, a, b);
                }

                if (comparer.Compare(keys[p], keys[b]) > 0)
                {
                    Swap(keys, p, b);
                }

                T pivot = keys[p];

                // Hoare Partitioning
                do
                {
                    while (comparer.Compare(keys[a], pivot) < 0)
                    {
                        a++;
                    }

                    while (comparer.Compare(pivot, keys[b]) < 0)
                    {
                        b--;
                    }

                    if (a > b)
                    {
                        break;
                    }

                    if (a < b)
                    {
                        Swap(keys, a, b);
                    }

                    a++;
                    b--;
                } while (a <= b);

                // In order to limit the recursion depth to log(n), we sort the
                // shorter partition recursively and the longer partition iteratively.
                if ((b - left) <= (right - a))
                {
                    if (left < b)
                    {
                        QuickSort(keys, comparer, left, b);
                    }

                    left = a;
                }
                else
                {
                    if (a < right)
                    {
                        QuickSort(keys, comparer, a, right);
                    }

                    right = b;
                }
            } while (left < right);
        }

        /// <summary>
        /// Recursive implementation for an in place quick sort on a list while reordering one other list accordingly.
        /// </summary>
        /// <typeparam name="T">The type of the list on which the quick sort is performed.</typeparam>
        /// <typeparam name="TItems">The type of the list which is automatically reordered accordingly.</typeparam>
        /// <param name="keys">The list which is sorted using quick sort.</param>
        /// <param name="items">The list which is automatically reordered accordingly.</param>
        /// <param name="comparer">The method with which to compare two elements of the quick sort.</param>
        /// <param name="left">The left boundary of the quick sort.</param>
        /// <param name="right">The right boundary of the quick sort.</param>
        static void QuickSort<T, TItems>(IList<T> keys, IList<TItems> items, IComparer<T> comparer, int left, int right)
        {
            do
            {
                // Pivoting
                int a = left;
                int b = right;
                int p = a + ((b - a) >> 1); // midpoint

                if (comparer.Compare(keys[a], keys[p]) > 0)
                {
                    Swap(keys, a, p);
                    Swap(items, a, p);
                }

                if (comparer.Compare(keys[a], keys[b]) > 0)
                {
                    Swap(keys, a, b);
                    Swap(items, a, b);
                }

                if (comparer.Compare(keys[p], keys[b]) > 0)
                {
                    Swap(keys, p, b);
                    Swap(items, p, b);
                }

                T pivot = keys[p];

                // Hoare Partitioning
                do
                {
                    while (comparer.Compare(keys[a], pivot) < 0)
                    {
                        a++;
                    }

                    while (comparer.Compare(pivot, keys[b]) < 0)
                    {
                        b--;
                    }

                    if (a > b)
                    {
                        break;
                    }

                    if (a < b)
                    {
                        Swap(keys, a, b);
                        Swap(items, a, b);
                    }

                    a++;
                    b--;
                } while (a <= b);

                // In order to limit the recursion depth to log(n), we sort the
                // shorter partition recursively and the longer partition iteratively.
                if ((b - left) <= (right - a))
                {
                    if (left < b)
                    {
                        QuickSort(keys, items, comparer, left, b);
                    }

                    left = a;
                }
                else
                {
                    if (a < right)
                    {
                        QuickSort(keys, items, comparer, a, right);
                    }

                    right = b;
                }
            } while (left < right);
        }

        /// <summary>
        /// Recursive implementation for an in place quick sort on one list while reordering two other lists accordingly.
        /// </summary>
        /// <typeparam name="T">The type of the list on which the quick sort is performed.</typeparam>
        /// <typeparam name="TItems1">The type of the first list which is automatically reordered accordingly.</typeparam>
        /// <typeparam name="TItems2">The type of the second list which is automatically reordered accordingly.</typeparam>
        /// <param name="keys">The list which is sorted using quick sort.</param>
        /// <param name="items1">The first list which is automatically reordered accordingly.</param>
        /// <param name="items2">The second list which is automatically reordered accordingly.</param>
        /// <param name="comparer">The method with which to compare two elements of the quick sort.</param>
        /// <param name="left">The left boundary of the quick sort.</param>
        /// <param name="right">The right boundary of the quick sort.</param>
        static void QuickSort<T, TItems1, TItems2>(
            IList<T> keys, IList<TItems1> items1, IList<TItems2> items2,
            IComparer<T> comparer,
            int left, int right)
        {
            do
            {
                // Pivoting
                int a = left;
                int b = right;
                int p = a + ((b - a) >> 1); // midpoint

                if (comparer.Compare(keys[a], keys[p]) > 0)
                {
                    Swap(keys, a, p);
                    Swap(items1, a, p);
                    Swap(items2, a, p);
                }

                if (comparer.Compare(keys[a], keys[b]) > 0)
                {
                    Swap(keys, a, b);
                    Swap(items1, a, b);
                    Swap(items2, a, b);
                }

                if (comparer.Compare(keys[p], keys[b]) > 0)
                {
                    Swap(keys, p, b);
                    Swap(items1, p, b);
                    Swap(items2, p, b);
                }

                T pivot = keys[p];

                // Hoare Partitioning
                do
                {
                    while (comparer.Compare(keys[a], pivot) < 0)
                    {
                        a++;
                    }

                    while (comparer.Compare(pivot, keys[b]) < 0)
                    {
                        b--;
                    }

                    if (a > b)
                    {
                        break;
                    }

                    if (a < b)
                    {
                        Swap(keys, a, b);
                        Swap(items1, a, b);
                        Swap(items2, a, b);
                    }

                    a++;
                    b--;
                } while (a <= b);

                // In order to limit the recursion depth to log(n), we sort the
                // shorter partition recursively and the longer partition iteratively.
                if ((b - left) <= (right - a))
                {
                    if (left < b)
                    {
                        QuickSort(keys, items1, items2, comparer, left, b);
                    }

                    left = a;
                }
                else
                {
                    if (a < right)
                    {
                        QuickSort(keys, items1, items2, comparer, a, right);
                    }

                    right = b;
                }
            } while (left < right);
        }

        /// <summary>
        /// Recursive implementation for an in place quick sort on the primary and then by the secondary list while reordering one secondary list accordingly.
        /// </summary>
        /// <typeparam name="T1">The type of the primary list.</typeparam>
        /// <typeparam name="T2">The type of the secondary list.</typeparam>
        /// <param name="primary">The list which is sorted using quick sort.</param>
        /// <param name="secondary">The list which is sorted secondarily (on primary duplicates) and automatically reordered accordingly.</param>
        /// <param name="primaryComparer">The method with which to compare two elements of the primary list.</param>
        /// <param name="secondaryComparer">The method with which to compare two elements of the secondary list.</param>
        /// <param name="left">The left boundary of the quick sort.</param>
        /// <param name="right">The right boundary of the quick sort.</param>
        static void QuickSortAll<T1, T2>(
            IList<T1> primary, IList<T2> secondary,
            IComparer<T1> primaryComparer, IComparer<T2> secondaryComparer,
            int left, int right)
        {
            do
            {
                // Pivoting
                int a = left;
                int b = right;
                int p = a + ((b - a) >> 1); // midpoint

                int ap = primaryComparer.Compare(primary[a], primary[p]);
                if (ap > 0 || ap == 0 && secondaryComparer.Compare(secondary[a], secondary[p]) > 0)
                {
                    Swap(primary, a, p);
                    Swap(secondary, a, p);
                }

                int ab = primaryComparer.Compare(primary[a], primary[b]);
                if (ab > 0 || ab == 0 && secondaryComparer.Compare(secondary[a], secondary[b]) > 0)
                {
                    Swap(primary, a, b);
                    Swap(secondary, a, b);
                }

                int pb = primaryComparer.Compare(primary[p], primary[b]);
                if (pb > 0 || pb == 0 && secondaryComparer.Compare(secondary[p], secondary[b]) > 0)
                {
                    Swap(primary, p, b);
                    Swap(secondary, p, b);
                }

                T1 pivot1 = primary[p];
                T2 pivot2 = secondary[p];

                // Hoare Partitioning
                do
                {
                    int ax;
                    while ((ax = primaryComparer.Compare(primary[a], pivot1)) < 0 || ax == 0 && secondaryComparer.Compare(secondary[a], pivot2) < 0)
                    {
                        a++;
                    }

                    int xb;
                    while ((xb = primaryComparer.Compare(pivot1, primary[b])) < 0 || xb == 0 && secondaryComparer.Compare(pivot2, secondary[b]) < 0)
                    {
                        b--;
                    }

                    if (a > b)
                    {
                        break;
                    }

                    if (a < b)
                    {
                        Swap(primary, a, b);
                        Swap(secondary, a, b);
                    }

                    a++;
                    b--;
                } while (a <= b);

                // In order to limit the recursion depth to log(n), we sort the
                // shorter partition recursively and the longer partition iteratively.
                if ((b - left) <= (right - a))
                {
                    if (left < b)
                    {
                        QuickSortAll(primary, secondary, primaryComparer, secondaryComparer, left, b);
                    }

                    left = a;
                }
                else
                {
                    if (a < right)
                    {
                        QuickSortAll(primary, secondary, primaryComparer, secondaryComparer, a, right);
                    }

                    right = b;
                }
            } while (left < right);
        }

        /// <summary>
        /// Performs an in place swap of two elements in a list.
        /// </summary>
        /// <typeparam name="T">The type of elements stored in the list.</typeparam>
        /// <param name="keys">The list in which the elements are stored.</param>
        /// <param name="a">The index of the first element of the swap.</param>
        /// <param name="b">The index of the second element of the swap.</param>
        static void Swap<T>(IList<T> keys, int a, int b)
        {
            if (a == b)
            {
                return;
            }

            T local = keys[a];
            keys[a] = keys[b];
            keys[b] = local;
        }
    }
}
