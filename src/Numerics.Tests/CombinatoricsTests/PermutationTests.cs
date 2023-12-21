using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.CombinatoricsTests
{
    [TestFixture]
    public class PermutationTests
    {
        private IEqualityComparer<int[]> _comparer = new ArrayEqualityComparer();
        class ArrayEqualityComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] x, int[] y)
            {
                return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
            }

            public int GetHashCode(int[] obj)
            {
                return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void NumberOfPermutationsGeneratedMatchesNFactorial(int n)
        {
            // See the xml comment on lazy enumeration danger.
            var permutations = Combinatorics.GenerateAllPermutations(n).Select(pi => pi.AsEnumerable().ToArray()).ToArray();
            var count = permutations.Distinct(_comparer).Count();
            var expected = Combinatorics.Permutations(n);
            Assert.AreEqual(expected,count);
        }

        [TestCase(1,1)]
        [TestCase(2,1)]
        [TestCase(3,3)]
        [TestCase(4,2)]
        [TestCase(4,3)]
        [TestCase(7,3)]
        public void NumberOfCombinationsGeneratedMatchesNChooseK(int n,int k)
        {
            // See the xml comment on lazy enumeration danger.
            var combinations = Combinatorics.GenerateAllCombinations(n,k).Select(pi => pi.AsEnumerable().ToArray()).ToArray();
            var count = combinations.Distinct(_comparer).Count();
            var expected = Combinatorics.Combinations(n,k);
            Assert.AreEqual(expected, count);
        }


        [TestCase(2,5,0,1,4,3)]
        public void InversePermutationInverts(params int[] permutation)
        {
            var identityPermutation = Enumerable.Range(0, permutation.Length).ToArray();
            var permuted = permutation.Select(p => identityPermutation[p]).ToArray();
            var inversePermutation = Combinatorics.InvertPermutation(permutation);
            var shouldBeIdentity = inversePermutation.Select(p => permuted[p]).ToArray();

            bool shouldBeTrue = _comparer.Equals(identityPermutation, shouldBeIdentity);

            Assert.IsTrue(shouldBeTrue);
        }
    }
}
