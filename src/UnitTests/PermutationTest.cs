// <copyright file="PermutationTest.cs" company="Math.NET">
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

using System;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests
{

    /// <summary>
    /// Permutation tests.
    /// </summary>
    [TestFixture]
    public class PermutationTest
    {
        /// <summary>
        /// Can create permutation.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { 0 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase(new[] { 0, 4, 3, 2, 1, 5 })]
        [TestCase(new[] { 0, 3, 2, 1, 4, 5 })]
        public void CanCreatePermutation(int[] idx)
        {
            GC.KeepAlive(new Permutation(idx));
        }

        /// <summary>
        /// Create permutation fails when given bad index set.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { -1 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 4 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 7 })]
        public void CreatePermutationFailsWhenGivenBadIndexSet(int[] idx)
        {
            Assert.That(() => new Permutation(idx), Throws.ArgumentException);
        }

        /// <summary>
        /// Can invert permutation.
        /// </summary>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { 0 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase(new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase(new[] { 0, 4, 3, 2, 1, 5 })]
        [TestCase(new[] { 0, 3, 2, 1, 4, 5 })]
        public void CanInvertPermutation([Values(new[] { 0 }, new[] { 0, 1, 2, 3, 4, 5 }, new[] { 5, 4, 3, 2, 1, 0 }, new[] { 0, 4, 3, 2, 1, 5 }, new[] { 0, 3, 2, 1, 4, 5 })] int[] idx)
        {
            var p = new Permutation(idx);
            var pinv = p.Inverse();

            Assert.AreEqual(p.Dimension, pinv.Dimension);
            for (var i = 0; i < p.Dimension; i++)
            {
                Assert.AreEqual(i, pinv[p[i]]);
                Assert.AreEqual(i, p[pinv[i]]);
            }
        }

        /// <summary>
        /// Can create permutation from inversions.
        /// </summary>
        /// <param name="inv">Inverse permutations index set.</param>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { 0 }, new[] { 0 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5 }, new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase(new[] { 5, 4, 3, 3, 4, 5 }, new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase(new[] { 0, 4, 3, 3, 4, 5 }, new[] { 0, 4, 3, 2, 1, 5 })]
        [TestCase(new[] { 0, 3, 2, 3, 4, 5 }, new[] { 0, 3, 2, 1, 4, 5 })]
        [TestCase(new[] { 2, 2, 2, 4, 4 }, new[] { 1, 2, 0, 4, 3 })]
        public void CanCreatePermutationFromInversions(int[] inv, int[] idx)
        {
            var p = Permutation.FromInversions(inv);
            var q = new Permutation(idx);

            Assert.AreEqual(q.Dimension, p.Dimension);
            for (var i = 0; i < q.Dimension; i++)
            {
                Assert.AreEqual(q[i], p[i]);
            }
        }

        /// <summary>
        /// Can create inversions from permutation.
        /// </summary>
        /// <param name="inv">Inverse permutations index set.</param>
        /// <param name="idx">Permutations index set.</param>
        [TestCase(new[] { 0 }, new[] { 0 })]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5 }, new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase(new[] { 5, 4, 3, 3, 4, 5 }, new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase(new[] { 0, 4, 3, 3, 4, 5 }, new[] { 0, 4, 3, 2, 1, 5 })]
        [TestCase(new[] { 0, 3, 2, 3, 4, 5 }, new[] { 0, 3, 2, 1, 4, 5 })]
        [TestCase(new[] { 2, 2, 2, 4, 4 }, new[] { 1, 2, 0, 4, 3 })]
        public void CanCreateInversionsFromPermutation(int[] inv, int[] idx)
        {
            var q = new Permutation(idx);
            var p = q.ToInversions();
            Assert.AreEqual(inv.Length, p.Length);
            for (var i = 0; i < q.Dimension; i++)
            {
                Assert.AreEqual(inv[i], p[i]);
            }
        }
    }
}
