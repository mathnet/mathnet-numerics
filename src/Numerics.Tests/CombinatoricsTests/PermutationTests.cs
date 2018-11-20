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
