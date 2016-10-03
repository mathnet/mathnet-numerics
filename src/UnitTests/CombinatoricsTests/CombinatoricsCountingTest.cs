// <copyright file="CombinatoricsCountingTest.cs" company="Math.NET">
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

using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.CombinatoricsTests
{

    /// <summary>
    /// Combinatorics counting tests.
    /// </summary>
    [TestFixture]
    public class CombinatoricsCountingTest
    {
        /// <summary>
        /// Can count variations.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 1)]
        [TestCase(10, 0, 1)]
        [TestCase(10, 2, 90)]
        [TestCase(10, 4, 5040)]
        [TestCase(10, 6, 151200)]
        [TestCase(10, 9, 3628800)]
        [TestCase(10, 10, 3628800)]
        public void CanCountVariations(int n, int k, long expected)
        {
            Assert.AreEqual(
                expected,
                Combinatorics.Variations(n, k),
                "Count the number of variations without repetition");
        }

        /// <summary>
        /// Out of range variations count to zero.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        [TestCase(0, 1)]
        [TestCase(10, 11)]
        [TestCase(0, -1)]
        [TestCase(1, -1)]
        [TestCase(-1, 0)]
        [TestCase(-1, 1)]
        public void OutOfRangeVariationsCountToZero(int n, int k)
        {
            Assert.AreEqual(
                0,
                Combinatorics.Variations(n, k),
                "The number of variations without repetition but out of the range must be 0.");
        }

        /// <summary>
        /// Can count variations with repetition.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 1)]
        [TestCase(10, 0, 1)]
        [TestCase(10, 2, 100)]
        [TestCase(10, 4, 10000)]
        [TestCase(10, 6, 1000000)]
        [TestCase(10, 9, 1000000000)]
        [TestCase(10, 10, 10000000000)]
        [TestCase(10, 11, 100000000000)]
        public void CanCountVariationsWithRepetition(int n, int k, long expected)
        {
            Assert.AreEqual(
                expected,
                Combinatorics.VariationsWithRepetition(n, k),
                "Count the number of variations with repetition");
        }

        /// <summary>
        /// Out of range variations withR repetition count to zero.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        [TestCase(0, 1)]
        [TestCase(0, -1)]
        [TestCase(1, -1)]
        [TestCase(-1, 0)]
        [TestCase(-1, 1)]
        public void OutOfRangeVariationsWithRepetitionCountToZero(int n, int k)
        {
            Assert.AreEqual(
                0,
                Combinatorics.VariationsWithRepetition(n, k),
                "The number of variations with repetition but out of the range must be 0.");
        }

        /// <summary>
        /// Can count combinations.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 1)]
        [TestCase(10, 0, 1)]
        [TestCase(10, 2, 45)]
        [TestCase(10, 4, 210)]
        [TestCase(10, 6, 210)]
        [TestCase(10, 9, 10)]
        [TestCase(10, 10, 1)]
        public void CanCountCombinations(int n, int k, long expected)
        {
            Assert.AreEqual(
                expected,
                Combinatorics.Combinations(n, k),
                "Count the number of combinations without repetition");
        }

        /// <summary>
        /// Out of range combinations count to zero.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        [TestCase(0, 1)]
        [TestCase(10, 11)]
        [TestCase(0, -1)]
        [TestCase(1, -1)]
        [TestCase(-1, 0)]
        [TestCase(-1, 1)]
        public void OutOfRangeCombinationsCountToZero(int n, int k)
        {
            Assert.AreEqual(
                0,
                Combinatorics.Combinations(n, k),
                "The number of combinations without repetition but out of the range must be 0.");
        }

        /// <summary>
        /// Can count combinations with repetition.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 1)]
        [TestCase(10, 0, 1)]
        [TestCase(10, 2, 55)]
        [TestCase(10, 4, 715)]
        [TestCase(10, 6, 5005)]
        [TestCase(10, 9, 48620)]
        [TestCase(10, 10, 92378)]
        [TestCase(10, 11, 167960)]
        public void CanCountCombinationsWithRepetition(int n, int k, long expected)
        {
            Assert.AreEqual(
                expected,
                Combinatorics.CombinationsWithRepetition(n, k),
                "Count the number of combinations with repetition");
        }

        /// <summary>
        /// Out of range combinations with repetition count to zero.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        [TestCase(0, 1)]
        [TestCase(0, -1)]
        [TestCase(1, -1)]
        [TestCase(-1, 0)]
        [TestCase(-1, 1)]
        public void OutOfRangeCombinationsWithRepetitionCountToZero(int n, int k)
        {
            Assert.AreEqual(
                0,
                Combinatorics.CombinationsWithRepetition(n, k),
                "The number of combinations with repetition but out of the range must be 0.");
        }

        /// <summary>
        /// Can count permutations.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(8, 40320)]
        [TestCase(15, 1307674368000)]
        public void CanCountPermutations(int n, long expected)
        {
            Assert.AreEqual(
                expected,
                Combinatorics.Permutations(n),
                "Count the number of permutations");
        }
    }
}
