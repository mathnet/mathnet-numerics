// <copyright file="Permutation.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics
{
    using System;
    using Properties;

    /// <summary>
    /// Class to represent a permutation for a subset of the natural numbers.
    /// </summary>
    [Serializable]
    public class Permutation
    {
        #region fields

        /// <summary>
        /// Entry _indices[i] represents the location to which i is permuted to.
        /// </summary>
        private readonly int[] _indices;

        #endregion fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Permutation class.
        /// </summary>
        /// <param name="indices">An array which represents where each integer is permuted too: indices[i] represents that integer i
        /// is permuted to location indices[i].</param>
        public Permutation(int[] indices)
        {
            if (!CheckForProperPermutation(indices))
            {
                throw new ArgumentException(Resources.PermutationAsIntArrayInvalid, "indices");
            }

            _indices = (int[])indices.Clone();
        }

        #endregion

        /// <summary>
        /// Gets the number of elements this permutation is over.
        /// </summary>
        public int Dimension
        {
            get { return _indices.Length; }
        }

        /// <summary>
        /// Computes where <paramref name="idx"/> permutes too.
        /// </summary>
        /// <param name="idx">The index to permute from.</param>
        /// <returns>The index which is permuted to.</returns>
        public int this[int idx]
        {
            get
            {
                return _indices[idx];
            }
        }

        /// <summary>
        /// Computes the inverse of the permutation.
        /// </summary>
        /// <returns>The inverse of the permutation.</returns>
        public Permutation Inverse()
        {
            var invIdx = new int[Dimension];
            for (int i = 0; i < invIdx.Length; i++)
            {
                invIdx[_indices[i]] = i;
            }

            return new Permutation(invIdx);
        }

        /// <summary>
        /// Construct an array from a sequence of inversions.
        /// </summary>
        /// <example>
        /// From wikipedia: the permutation 12043 has the inversions (0,2), (1,2) and (3,4). This would be
        /// encoded using the array [22244].
        /// </example>
        /// <param name="inv">The set of inversions to construct the permutation from.</param>
        /// <returns>A permutation generated from a sequence of inversions.</returns>
        public static Permutation FromInversions(int[] inv)
        {
            var idx = new int[inv.Length];
            for (int i = 0; i < inv.Length; i++)
            {
                idx[i] = i;
            }

            for (int i = inv.Length - 1; i >= 0; i--)
            {
                if (idx[i] != inv[i])
                {
                    int t = idx[i];
                    idx[i] = idx[inv[i]];
                    idx[inv[i]] = t;
                }
            }

            return new Permutation(idx);
        }

        /// <summary>
        /// Construct a sequence of inversions from the permutation.
        /// </summary>
        /// <example>
        /// From wikipedia: the permutation 12043 has the inversions (0,2), (1,2) and (3,4). This would be
        /// encoded using the array [22244].
        /// </example>
        /// <returns>A sequence of inversions.</returns>
        public int[] ToInversions()
        {
            var idx = (int[])_indices.Clone();

            for (int i = 0; i < idx.Length; i++)
            {
                if (idx[i] != i)
                {
#if !PORTABLE
                    int q = Array.FindIndex(idx, i + 1, x => x == i);
#else
                    int q = -1;
                    for(int j = i+1; j < Dimension; j++)
                    {
                        if(idx[j] == i)
                        {
                            q = j;
                            break;
                        }
                    }
#endif
                    var t = idx[i];
                    idx[i] = q;
                    idx[q] = t;
                }
            }

            return idx;
        }

        /// <summary>
        /// Checks whether the <paramref name="indices"/> array represents a proper permutation.
        /// </summary>
        /// <param name="indices">An array which represents where each integer is permuted too: indices[i] represents that integer i
        /// is permuted to location indices[i].</param>
        /// <returns>True if <paramref name="indices"/> represents a proper permutation, <c>false</c> otherwise.</returns>
        static private bool CheckForProperPermutation(int[] indices)
        {
            var idxCheck = new bool[indices.Length];

            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] >= indices.Length || indices[i] < 0)
                {
                    return false;
                }

                idxCheck[indices[i]] = true;
            }

            for (int i = 0; i < indices.Length; i++)
            {
                if (idxCheck[i] == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}