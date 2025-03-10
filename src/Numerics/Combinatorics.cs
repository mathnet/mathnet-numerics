// <copyright file="Combinatorics.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Random;
using System.Numerics;

namespace MathNet.Numerics
{
    /// <summary>
    /// Enumerative Combinatorics and Counting.
    /// </summary>
    public static class Combinatorics
    {
        /// <summary>
        /// Count the number of possible variations without repetition.
        /// The order matters and each object can be chosen only once.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen at most once.</param>
        /// <returns>Maximum number of distinct variations.</returns>
        public static double Variations(int n, int k)
        {
            if (k < 0 || n < 0 || k > n)
            {
                return 0;
            }

            return Math.Floor(
                0.5 + Math.Exp(
                    SpecialFunctions.FactorialLn(n)
                    - SpecialFunctions.FactorialLn(n - k)));
        }

        /// <summary>
        /// Count the number of possible variations with repetition.
        /// The order matters and each object can be chosen more than once.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen 0, 1 or multiple times.</param>
        /// <returns>Maximum number of distinct variations with repetition.</returns>
        public static double VariationsWithRepetition(int n, int k)
        {
            if (k < 0 || n < 0)
            {
                return 0;
            }

            return Math.Pow(n, k);
        }

        /// <summary>
        /// Count the number of possible combinations without repetition.
        /// The order does not matter and each object can be chosen only once.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen at most once.</param>
        /// <returns>Maximum number of combinations.</returns>
        public static double Combinations(int n, int k)
        {
            return SpecialFunctions.Binomial(n, k);
        }

        /// <summary>
        /// Count the number of possible combinations with repetition.
        /// The order does not matter and an object can be chosen more than once.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen 0, 1 or multiple times.</param>
        /// <returns>Maximum number of combinations with repetition.</returns>
        public static double CombinationsWithRepetition(int n, int k)
        {
            if (k < 0 || n < 0 || (n == 0 && k > 0))
            {
                return 0;
            }

            if (n == 0 && k == 0)
            {
                return 1;
            }

            return Math.Floor(
                0.5 + Math.Exp(
                    SpecialFunctions.FactorialLn(n + k - 1)
                    - SpecialFunctions.FactorialLn(k)
                    - SpecialFunctions.FactorialLn(n - 1)));
        }

        /// <summary>
        /// Count the number of possible permutations (without repetition).
        /// </summary>
        /// <param name="n">Number of (distinguishable) elements in the set.</param>
        /// <returns>Maximum number of permutations without repetition.</returns>
        public static double Permutations(int n)
        {
            return SpecialFunctions.Factorial(n);
        }

        /// <summary>
        /// Generate a random permutation, without repetition, by generating the index numbers 0 to N-1 and shuffle them randomly.
        /// Implemented using Fisher-Yates Shuffling.
        /// </summary>
        /// <returns>An array of length <c>N</c> that contains (in any order) the integers of the interval <c>[0, N)</c>.</returns>
        /// <param name="n">Number of (distinguishable) elements in the set.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static int[] GeneratePermutation(int n, System.Random randomSource = null)
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Value must not be negative (zero is ok).");
            return Enumerable.Range(0, n).SelectPermutation(randomSource).ToArray();
        }

        /// <summary>
        /// Select a random permutation, without repetition, from a data array by reordering the provided array in-place.
        /// Implemented using Fisher-Yates Shuffling. The provided data array will be modified.
        /// </summary>
        /// <param name="data">The data array to be reordered. The array will be modified by this routine.</param>
        /// <param name="startIndex">Elements before this index won't be reordered</param>
        /// <param name="endIndex">Elements after this index won't be reordered</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static void SelectPermutationInplace<T>(this IList<T> data, int startIndex, int endIndex, System.Random randomSource = null)
        {
            var random = randomSource ?? SystemRandomSource.Default;

            // Fisher-Yates Shuffling
            for (int i = endIndex; i > startIndex; i--)
            {
                int swapIndex = random.Next(startIndex, i + 1);
                (data[i], data[swapIndex]) = (data[swapIndex], data[i]);
            }
        }

        /// <summary>
        /// Select a random permutation, without repetition, from a data array by reordering the provided array in-place.
        /// Implemented using Fisher-Yates Shuffling. The provided data array will be modified.
        /// </summary>
        /// <param name="data">The data array to be reordered. The array will be modified by this routine.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static void SelectPermutationInplace<T>(this IList<T> data, System.Random randomSource = null)
        {
            data.SelectPermutationInplace(0, data.Count - 1, randomSource);
        }
#if NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Select a random permutation, without repetition, from a data span by reordering the provided span in-place.
        /// Implemented using Fisher-Yates Shuffling. The provided data span will be modified.
        /// </summary>
        /// <param name="data">The data span to be reordered. The span will be modified by this routine.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static void SelectPermutationInplace<T>(this Span<T> data, System.Random randomSource = null)
        {
            var random = randomSource ?? SystemRandomSource.Default;

            // Fisher-Yates Shuffling
            for (int i = data.Length - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (data[i], data[swapIndex]) = (data[swapIndex], data[i]);
            }
        }
#endif
        /// <summary>
        /// Select a random permutation from a data sequence by returning the provided data in random order.
        /// Implemented using Fisher-Yates Shuffling. The provided data array will be modified but not copied.
        /// </summary>
        /// <param name="data">The data elements to be reordered.</param>
        /// <param name="startIndex">Elements before this index won't be reordered</param>
        /// <param name="endIndex">Elements after this index won't be reordered</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static IEnumerable<T> SelectPermutationWithoutCopy<T>(this IList<T> data, int startIndex, int endIndex, System.Random randomSource = null)
        {
            var random = randomSource ?? SystemRandomSource.Default;

            // Fisher-Yates Shuffling
            for (int i = endIndex; i >= startIndex; i--)
            {
                int k = random.Next(startIndex, i + 1);
                yield return data[k];
                data[k] = data[i];
            }
        }
#if NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Select a random permutation from a data sequence by returning the provided data in random order.
        /// Implemented using Fisher-Yates Shuffling. The provided data memory will be modified but not copied.
        /// </summary>
        /// <param name="data">The data elements to be reordered.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static IEnumerable<T> SelectPermutationWithoutCopy<T>(this Memory<T> data, System.Random randomSource = null)
        {
            var random = randomSource ?? SystemRandomSource.Default;

            // Fisher-Yates Shuffling
            for (int i = data.Length - 1; i >= 0; i--)
            {
                int k = random.Next(i + 1);
                yield return data.Span[k];
                data.Span[k] = data.Span[i];
            }
        }
#endif
        /// <summary>
        /// Select a random permutation from a data sequence by returning the provided data in random order.
        /// Implemented using Fisher-Yates Shuffling. The provided data array will be copied but not modified.
        /// </summary>
        /// <param name="data">The data elements to be reordered.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static IEnumerable<T> SelectPermutation<T>(this IEnumerable<T> data, System.Random randomSource = null)
        {
            return data.ToArray().SelectPermutationWithoutCopy(0, data.Count() - 1, randomSource);
        }

        /// <summary>
        /// Generate a random combination, without repetition, by randomly selecting some of N elements.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>Boolean mask array of length <c>N</c>, for each item true if it is selected.</returns>
        public static bool[] GenerateCombination(int n, System.Random randomSource = null)
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Value must not be negative (zero is ok).");

            var random = randomSource ?? SystemRandomSource.Default;

            bool[] mask = new bool[n];
            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] = random.NextBoolean();
            }

            return mask;
        }

        /// <summary>
        /// Generate a random combination, without repetition, by randomly selecting k of N elements.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen at most once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>Boolean mask array of length <c>N</c>, for each item true if it is selected.</returns>
        public static bool[] GenerateCombination(int n, int k, System.Random randomSource = null)
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Value must not be negative (zero is ok).");
            if (k < 0) throw new ArgumentOutOfRangeException(nameof(k), "Value must not be negative (zero is ok).");
            if (k > n) throw new ArgumentOutOfRangeException(nameof(k), $"{"k"} must be smaller than or equal to {"n"}.");

            var random = randomSource ?? SystemRandomSource.Default;

            bool[] mask = new bool[n];
            if (k*3 < n)
            {
                // just pick and try
                int selectionCount = 0;
                while (selectionCount < k)
                {
                    int index = random.Next(n);
                    if (!mask[index])
                    {
                        mask[index] = true;
                        selectionCount++;
                    }
                }

                return mask;
            }

            // based on permutation
            int[] permutation = GeneratePermutation(n, random);
            for (int i = 0; i < k; i++)
            {
                mask[permutation[i]] = true;
            }

            return mask;
        }

        /// <summary>
        /// Select a random combination, without repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Each element is chosen at most once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen combination, in the original order.</returns>
        public static IEnumerable<T> SelectCombination<T>(this IEnumerable<T> data, int elementsToChoose, System.Random randomSource = null)
        {
            IReadOnlyList<T> array = data as IReadOnlyList<T> ?? data.ToArray();

            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");
            if (elementsToChoose > array.Count) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), $"elementsToChoose must be smaller than or equal to data.Count.");

            bool[] mask = GenerateCombination(array.Count, elementsToChoose, randomSource);

            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    yield return array[i];
                }
            }
        }

        /// <summary>
        /// Select a random combination, without repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="startIndex">Elements before this index won't be chosen.</param>
        /// <param name="segmentLength">Number of elements in the data segment to be chosen from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Each element is chosen at most once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen combination, in the original order.</returns>
        public static IEnumerable<T> SelectCombination<T>(this IReadOnlyList<T> data, int startIndex, int segmentLength, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");
            if (elementsToChoose > segmentLength) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), $"elementsToChoose must be smaller than or equal to segmentLength.");

            bool[] mask = GenerateCombination(segmentLength, elementsToChoose, randomSource);

            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    yield return data[startIndex + i];
                }
            }
        }
#if NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Select a random combination, without repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Each element is chosen at most once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen combination, in the original order.</returns>
        public static IEnumerable<T> SelectCombination<T>(this ReadOnlyMemory<T> data, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");
            if (elementsToChoose > data.Length) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), $"elementsToChoose must be smaller than or equal to data.Length.");

            bool[] mask = GenerateCombination(data.Length, elementsToChoose, randomSource);

            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    yield return data.Span[i];
                }
            }
        }

        /// <summary>
        /// Select a random combination, without repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="output">The chosen combination, in the original order. Number of elements to choose is determined by its <c>Length</c></param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static void SelectCombination<T>(this ReadOnlySpan<T> data, Span<T> output, System.Random randomSource = null)
        {
            if (output.Length > data.Length) throw new ArgumentOutOfRangeException("output.Length", $"output.Length must be smaller than or equal to data.Length.");

            bool[] mask = GenerateCombination(data.Length, output.Length, randomSource);
            int startIndex = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    output[startIndex++] = data[i];
                }
            }
        }

        /// <summary>
        /// Select a random combination, without repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Each element is chosen at most once.</param>
        /// <param name="output">The chosen combination, in the original order.</param>
        /// <param name="startIndex">Chosen elements will be copied to output starting from this index.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static void SelectCombination<T>(this ReadOnlySpan<T> data, int elementsToChoose, IList<T> output, int startIndex = 0, System.Random randomSource = null)
        {
            if (elementsToChoose > data.Length) throw new ArgumentOutOfRangeException("elementsToChoose", $"elementsToChoose must be smaller than or equal to data.Length.");

            bool[] mask = GenerateCombination(data.Length, elementsToChoose, randomSource);
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    output[startIndex++] = data[i];
                }
            }
        }
#endif
        /// <summary>
        /// Generates a random combination, with repetition, by randomly selecting k of N elements.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Elements can be chosen more than once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>Integer mask array of length <c>N</c>, for each item the number of times it was selected.</returns>
        public static int[] GenerateCombinationWithRepetition(int n, int k, System.Random randomSource = null)
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Value must not be negative (zero is ok).");
            if (k < 0) throw new ArgumentOutOfRangeException(nameof(k), "Value must not be negative (zero is ok).");

            var random = randomSource ?? SystemRandomSource.Default;

            int[] mask = new int[n];
            for (int i = 0; i < k; i++)
            {
                mask[random.Next(n)]++;
            }

            return mask;
        }

        /// <summary>
        /// Select a random combination, with repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Elements can be chosen more than once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen combination with repetition, in the original order.</returns>
        public static IEnumerable<T> SelectCombinationWithRepetition<T>(this IEnumerable<T> data, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");

            IReadOnlyList<T> array = data as IReadOnlyList<T> ?? data.ToArray();
            int[] mask = GenerateCombinationWithRepetition(array.Count, elementsToChoose, randomSource);

            for (int i = 0; i < mask.Length; i++)
            {
                for (int j = 0; j < mask[i]; j++)
                {
                    yield return array[i];
                }
            }
        }

        /// <summary>
        /// Select a random combination, with repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="startIndex">Elements before this index won't be chosen.</param>
        /// <param name="segmentLength">Number of elements in the data segment to be chosen from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Elements can be chosen more than once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen combination with repetition, in the original order.</returns>
        public static IEnumerable<T> SelectCombinationWithRepetition<T>(this IReadOnlyList<T> data, int startIndex, int segmentLength, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");

            int[] mask = GenerateCombinationWithRepetition(segmentLength, elementsToChoose, randomSource);

            for (int i = 0; i < mask.Length; i++)
            {
                for (int j = 0; j < mask[i]; j++)
                {
                    yield return data[startIndex + i];
                }
            }
        }
#if NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Select a random combination, with repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Elements can be chosen more than once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen combination with repetition, in the original order.</returns>
        public static IEnumerable<T> SelectCombinationWithRepetition<T>(this ReadOnlyMemory<T> data, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");

            int[] mask = GenerateCombinationWithRepetition(data.Length, elementsToChoose, randomSource);

            for (int i = 0; i < mask.Length; i++)
            {
                for (int j = 0; j < mask[i]; j++)
                {
                    yield return data.Span[i];
                }
            }
        }

        /// <summary>
        /// Select a random combination, with repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="output">The chosen combination, in the original order. Number of elements to choose is determined by its <c>Length</c></param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen combination with repetition, in the original order.</returns>
        public static void SelectCombinationWithRepetition<T>(this ReadOnlySpan<T> data, Span<T> output, System.Random randomSource = null)
        {
            int[] mask = GenerateCombinationWithRepetition(data.Length, output.Length, randomSource);
            int startIndex = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                for (int j = 0; j < mask[i]; j++)
                {
                    output[startIndex++] = data[i];
                }
            }
        }

        /// <summary>
        /// Select a random combination, with repetition, from a data sequence by selecting k elements in original order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set.</param>
        /// <param name="output">The chosen combination, in the original order.</param>
        /// <param name="startIndex">Chosen elements will be copied to output starting from this index.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen combination with repetition, in the original order.</returns>
        public static void SelectCombinationWithRepetition<T>(this ReadOnlySpan<T> data, int elementsToChoose, IList<T> output, int startIndex = 0, System.Random randomSource = null)
        {
            int[] mask = GenerateCombinationWithRepetition(data.Length, elementsToChoose, randomSource);
            for (int i = 0; i < mask.Length; i++)
            {
                for (int j = 0; j < mask[i]; j++)
                {
                    output[startIndex++] = data[i];
                }
            }
        }
#endif
        /// <summary>
        /// Generate a random variation, without repetition, by randomly selecting k of n elements with order.
        /// Implemented using partial Fisher-Yates Shuffling.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen at most once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>An array of length <c>K</c> that contains the indices of the selections as integers of the interval <c>[0, N)</c>.</returns>
        public static int[] GenerateVariation(int n, int k, System.Random randomSource = null)
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Value must not be negative (zero is ok).");
            if (k < 0) throw new ArgumentOutOfRangeException(nameof(k), "Value must not be negative (zero is ok).");
            if (k > n) throw new ArgumentOutOfRangeException(nameof(k), $"k must be smaller than or equal to n.");

            return Enumerable.Range(0,n).SelectPermutation(randomSource).Take(k).ToArray();
        }

        /// <summary>
        /// Generate a random variation, without repetition, by randomly selecting k of n elements with order. This is an O(k) space-complexity implementation optimized for very large N.<br/>
        /// The space complexity of Fisher-Yates Shuffling is O(n+k). When N is very large, the algorithm will be unexecutable in limited memory, and a more memory-efficient algorithm is needed.<br/>
        /// You can explicitly cast N to <see cref="BigInteger"/> if N is out of range of <see cref="int"/> or memory, so that this special implementation is called. However, this implementation is slower than Fisher-Yates Shuffling: don't call it if time is more critical than space.<br/>
        /// The K of type <see cref="BigInteger"/> seems impossible, because the returned array is of size K and must all be stored in memory.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen at most once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>An array of length <c>K</c> that contains the indices of the selections as integers of the interval <c>[0, N)</c>.</returns>
        public static BigInteger[] GenerateVariation(BigInteger n, int k, System.Random randomSource = null)
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Value must not be negative (zero is ok).");
            if (k < 0) throw new ArgumentOutOfRangeException(nameof(k), "Value must not be negative (zero is ok).");
            if (k > n) throw new ArgumentOutOfRangeException(nameof(k), $"k must be smaller than or equal to n.");

            var random = randomSource ?? SystemRandomSource.Default;

            BigInteger[] selection = new BigInteger[k];
            if (n == 0 || k == 0)
            {
                return selection;
            }

            selection[0] = random.NextBigIntegerSequence(BigInteger.Zero, n).First();
            bool[] compareCache;
            bool keepLooping;
            BigInteger randomNumber;

            for (int a = 1; a < k; a++)
            {
                randomNumber = random.NextBigIntegerSequence(BigInteger.Zero, n - a).First();
                compareCache = Generate.Repeat(a, true);
                do
                {
                    keepLooping = false;
                    for (int b = 0; b < a; ++b)
                        if (compareCache[b] && randomNumber >= selection[b])
                        {
                            compareCache[b] = false;
                            keepLooping = true;
                            randomNumber++;
                        }
                } while (keepLooping);
                selection[a] = randomNumber;
            }

            return selection;
        }

        /// <summary>
        /// Select a random variation, without repetition, from a data sequence by randomly selecting k elements in random order.
        /// Implemented using partial Fisher-Yates Shuffling.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the set. Each element is chosen at most once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen variation, in random order.</returns>
        public static IEnumerable<T> SelectVariation<T>(this IEnumerable<T> data, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");
            if (elementsToChoose > data.Count()) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "elementsToChoose must be smaller than or equal to data.Count().");

            return data.SelectPermutation(randomSource).Take(elementsToChoose);
        }

        /// <summary>
        /// Generate a random variation, with repetition, by randomly selecting k of n elements with order.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Elements can be chosen more than once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>An array of length <c>K</c> that contains the indices of the selections as integers of the interval <c>[0, N)</c>.</returns>
        public static int[] GenerateVariationWithRepetition(int n, int k, System.Random randomSource = null)
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Value must not be negative (zero is ok).");
            if (k < 0) throw new ArgumentOutOfRangeException(nameof(k), "Value must not be negative (zero is ok).");

            var random = randomSource ?? SystemRandomSource.Default;

            int[] ret = new int[k];
            random.NextInt32s(ret, 0, n);
            return ret;
        }

        /// <summary>
        /// Select a random variation, with repetition, from a data sequence by randomly selecting k elements in random order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Elements can be chosen more than once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen variation with repetition, in random order.</returns>
        public static IEnumerable<T> SelectVariationWithRepetition<T>(this IEnumerable<T> data, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");

            IReadOnlyList<T> array = data as IReadOnlyList<T> ?? data.ToArray();
            int[] indices = GenerateVariationWithRepetition(array.Count, elementsToChoose, randomSource);

            for (int i = 0; i < indices.Length; i++)
            {
                yield return array[indices[i]];
            }
        }

        /// <summary>
        /// Select a random variation, with repetition, from a data sequence by randomly selecting k elements in random order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="startIndex">Elements before this index won't be chosen.</param>
        /// <param name="segmentLength">Number of elements in the data segment to be chosen from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Elements can be chosen more than once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen variation with repetition, in random order.</returns>
        public static IEnumerable<T> SelectVariationWithRepetition<T>(this IReadOnlyList<T> data, int startIndex, int segmentLength, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");

            int[] indices = GenerateVariationWithRepetition(segmentLength, elementsToChoose, randomSource);

            for (int i = 0; i < indices.Length; i++)
            {
                yield return data[startIndex + indices[i]];
            }
        }
#if NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Select a random variation, with repetition, from a data sequence by randomly selecting k elements in random order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Elements can be chosen more than once.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        /// <returns>The chosen variation with repetition, in random order.</returns>
        public static IEnumerable<T> SelectVariationWithRepetition<T>(this ReadOnlyMemory<T> data, int elementsToChoose, System.Random randomSource = null)
        {
            if (elementsToChoose < 0) throw new ArgumentOutOfRangeException(nameof(elementsToChoose), "Value must not be negative (zero is ok).");

            int[] indices = GenerateVariationWithRepetition(data.Length, elementsToChoose, randomSource);

            for (int i = 0; i < indices.Length; i++)
            {
                yield return data.Span[indices[i]];
            }
        }

        /// <summary>
        /// Select a random variation, with repetition, from a data sequence by randomly selecting k elements in random order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="output">The chosen variation with repetition, in random order. Number of elements to choose is determined by its <c>Length</c></param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static void SelectVariationWithRepetition<T>(this ReadOnlySpan<T> data, Span<T> output, System.Random randomSource = null)
        {
            int[] indices = GenerateVariationWithRepetition(data.Length, output.Length, randomSource);
            for (int i = 0; i < indices.Length; i++)
            {
                output[i] = data[indices[i]];
            }
        }

        /// <summary>
        /// Select a random variation, with repetition, from a data sequence by randomly selecting k elements in random order.
        /// </summary>
        /// <param name="data">The data source to choose from.</param>
        /// <param name="elementsToChoose">Number of elements (k) to choose from the data set. Elements can be chosen more than once.</param>
        /// <param name="output">The chosen variation with repetition, in random order.</param>
        /// <param name="startIndex">Chosen elements will be copied to output starting from this index.</param>
        /// <param name="randomSource">The random number generator to use. Optional; the default random source will be used if null.</param>
        public static void SelectVariationWithRepetition<T>(this ReadOnlySpan<T> data, int elementsToChoose, IList<T> output, int startIndex = 0, System.Random randomSource = null)
        {
            int[] indices = GenerateVariationWithRepetition(data.Length, elementsToChoose, randomSource);
            for (int i = 0; i < indices.Length; i++)
            {
                output[startIndex + i] = data[indices[i]];
            }
        }
#endif
    }
}
