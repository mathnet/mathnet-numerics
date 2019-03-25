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
        /// Can create permutation.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [Test(new[] { 0 })]
        [Test(new[] { 0, 1, 2, 3, 4, 5 })]
        [Test(new[] { 5, 4, 3, 2, 1, 0 })]
        [Test(new[] { 0, 4, 3, 2, 1, 5 })]
        [Test(new[] { 0, 3, 2, 1, 4, 5 })]
        public void CanCreatePermutation(int[] idx)
        {
            GC.KeepAlive(new Permutations(idx));
        }

        /// <summary>
        /// Create permutation fails when given bad index set.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [Test(new[] { -1 })]
        [Test(new[] { 0, 1, 2, 3, 4, 4 })]
        [Test(new[] { 5, 4, 3, 2, 1, 7 })]
        public void CreatePermutationFailsWhenGivenBadIndexSet(int[] idx)
        {
            Assert.That(() => new Permutations(idx), Throws.ArgumentException);

        }
