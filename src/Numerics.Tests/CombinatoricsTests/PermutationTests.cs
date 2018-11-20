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

    }
}
