// <copyright file="CombinatoricsCountingTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.CombinatoricsTests
{
    using NUnit.Framework;

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
        [Test, Sequential]
        public void CanCountVariations([Values(0, 1, 10, 10, 10, 10, 10, 10)] int n, [Values(0, 0, 0, 2, 4, 6, 9, 10)] int k, [Values(1, 1, 1, 90, 5040, 151200, 3628800, 3628800)] long expected)
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
        [Test, Sequential]
        public void OutOfRangeVariationsCountToZero([Values(0, 10, 0, 1, -1, -1)] int n, [Values(1, 11, -1, -1, 0, 1)] int k)
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
        [Test, Sequential]
        public void CanCountVariationsWithRepetition([Values(0, 1, 10, 10, 10, 10, 10, 10, 10)] int n, [Values(0, 0, 0, 2, 4, 6, 9, 10, 11)] int k, [Values(1, 1, 1, 100, 10000, 1000000, 1000000000, 10000000000, 100000000000)] long expected)
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
        [Test, Sequential]
        public void OutOfRangeVariationsWithRepetitionCountToZero([Values(0, 0, 1, -1, -1)] int n, [Values(1, -1, -1, 0, 1)] int k)
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
        [Test, Sequential]
        public void CanCountCombinations([Values(0, 1, 10, 10, 10, 10, 10, 10)] int n, [Values(0, 0, 0, 2, 4, 6, 9, 10)] int k, [Values(1, 1, 1, 45, 210, 210, 10, 1)] long expected)
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
        [Test, Sequential]
        public void OutOfRangeCombinationsCountToZero([Values(0, 10, 0, 1, -1, -1)] int n, [Values(1, 11, -1, -1, 0, 1)] int k)
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
        [Test, Sequential]
        public void CanCountCombinationsWithRepetition([Values(0, 1, 10, 10, 10, 10, 10, 10, 10)] int n, [Values(0, 0, 0, 2, 4, 6, 9, 10, 11)] int k, [Values(1, 1, 1, 55, 715, 5005, 48620, 92378, 167960)] long expected)
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
        [Test, Sequential]
        public void OutOfRangeCombinationsWithRepetitionCountToZero([Values(0, 0, 1, -1, -1)] int n, [Values(1, -1, -1, 0, 1)] int k)
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
        [Test, Sequential]
        public void CanCountPermutations([Values(0, 1, 2, 8, 15)] int n, [Values(1, 1, 2, 40320, 1307674368000)] long expected)
        {
            Assert.AreEqual(
                expected, 
                Combinatorics.Permutations(n), 
                "Count the number of permutations");
        }
    }
}
