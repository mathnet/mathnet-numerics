using MathNet.Numerics.Combinations;
using NUnit.Framework;
using System;

namespace NUnitTests
{
    /// <summary>
    /// Permutation tests.
    /// </summary>
    [TestFixture()]
    public class CombinatoricsTest
    {
        /// <summary>
        /// Can create permutation from int array.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { -1 })]
        [TestCase(new[] { 0 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 4 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase(new[] { 0, 4, 3, 2, 1, 5 })]
        [TestCase(new[] { 0, 3, 2, 1, 4, 5 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 7 })]
        public void CanCreateIntegerPermutation(int[] idx)
        {
            GC.KeepAlive(new Permutations<int>(idx));
        }

        /// <summary>
        /// Can create permutation from char array.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { 'A' })]
        [TestCase(new[] { 'A', 'A', 'B', 'C', 'D', 'E' })]
        [TestCase(new[] { 'A', 'B', 'C', 'D', 'E', 'F' })]
        [TestCase(new[] { 'D', 'E', 'F', 'A', 'B', 'C' })]
        public void CanCreateCharPermutation(char[] idx)
        {
            GC.KeepAlive(new Permutations<char>(idx));
        }

        /// <summary>
        /// Can create permutation from int array.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { -1 })]
        [TestCase(new[] { 0 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 4 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase(new[] { 0, 4, 3, 2, 1, 5 })]
        [TestCase(new[] { 0, 3, 2, 1, 4, 5 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 7 })]
        public void CanCreateIntegerCombination(int[] idx)
        {
            GC.KeepAlive(new Combinations<int>(idx, 3));
        }

        /// <summary>
        /// Can create permutation from char array.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { 'A' })]
        [TestCase(new[] { 'A', 'A', 'B', 'C', 'D', 'E' })]
        [TestCase(new[] { 'A', 'B', 'C', 'D', 'E', 'F' })]
        [TestCase(new[] { 'D', 'E', 'F', 'A', 'B', 'C' })]
        public void CanCreateCharCombination(char[] idx)
        {
            GC.KeepAlive(new Combinations<char>(idx, 3));
        }

        /// <summary>
        /// Can create permutation from int array.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { -1 })]
        [TestCase(new[] { 0 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 4 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase(new[] { 0, 4, 3, 2, 1, 5 })]
        [TestCase(new[] { 0, 3, 2, 1, 4, 5 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 7 })]
        public void CanCreateIntegerVariation(int[] idx)
        {
            GC.KeepAlive(new Variations<int>(idx, 3));
        }

        /// <summary>
        /// Can create permutation from char array.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { 'A' })]
        [TestCase(new[] { 'A', 'A', 'B', 'C', 'D', 'E' })]
        [TestCase(new[] { 'A', 'B', 'C', 'D', 'E', 'F' })]
        [TestCase(new[] { 'D', 'E', 'F', 'A', 'B', 'C' })]
        public void CanCreateCharVariation(char[] idx)
        {
            GC.KeepAlive(new Variations<char>(idx, 3));
        }
    }
}