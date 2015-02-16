// <copyright file="Ilutp.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Integer.Solvers
{
    /// <summary>
    /// This class performs an Incomplete LU factorization with drop tolerance
    /// and partial pivoting. The drop tolerance indicates which additional entries
    /// will be dropped from the factorized LU matrices.
    /// </summary>
    /// <remarks>
    /// The ILUTP-Mem algorithm was taken from: <br/>
    /// ILUTP_Mem: a Space-Efficient Incomplete LU Preconditioner
    /// <br/>
    /// Tzu-Yi Chen, Department of Mathematics and Computer Science, <br/>
    /// Pomona College, Claremont CA 91711, USA <br/>
    /// Published in: <br/>
    /// Lecture Notes in Computer Science <br/>
    /// Volume 3046 / 2004 <br/>
    /// pp. 20 - 28 <br/>
    /// Algorithm is described in Section 2, page 22
    /// </remarks>
    public sealed class ILUTPPreconditioner : IPreconditioner<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ILUTPPreconditioner"/> class with the default settings.
        /// </summary>
        public ILUTPPreconditioner()
        {
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ILUTPPreconditioner"/> class with the specified settings.
        /// </summary>
        /// <param name="fillLevel">
        /// The amount of fill that is allowed in the matrix. The value is a fraction of 
        /// the number of non-zero entries in the original matrix. Values should be positive.
        /// </param>
        /// <param name="dropTolerance">
        /// The absolute drop tolerance which indicates below what absolute value an entry 
        /// will be dropped from the matrix. A drop tolerance of 0.0 means that no values
        /// will be dropped. Values should always be positive.
        /// </param>
        /// <param name="pivotTolerance">
        /// The pivot tolerance which indicates at what level pivoting will take place. A
        /// value of 0.0 means that no pivoting will take place.
        /// </param>
        public ILUTPPreconditioner(double fillLevel, double dropTolerance, double pivotTolerance)
        {
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        public void Initialize(Matrix<int> matrix)
        {
            // Not possible since this cannot be constructed.
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        public void Approximate(Vector<int> rhs, Vector<int> lhs)
        {
            // Not possible since this cannot be constructed.
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }
    }

    /// <summary>
    /// An element sort algorithm for the <see cref="ILUTPPreconditioner"/> class.
    /// </summary>
    /// <remarks>
    /// This sort algorithm is used to sort the columns in a sparse matrix based on
    /// the value of the element on the diagonal of the matrix.
    /// </remarks>
    internal static class ILUTPElementSorter
    {
        /// <summary>
        /// Sorts the elements of the <paramref name="values"/> vector in decreasing
        /// fashion. The vector itself is not affected.
        /// </summary>
        /// <param name="lowerBound">The starting index.</param>
        /// <param name="upperBound">The stopping index.</param>
        /// <param name="sortedIndices">An array that will contain the sorted indices once the algorithm finishes.</param>
        /// <param name="values">The <see cref="Vector"/> that contains the values that need to be sorted.</param>
        public static void SortDoubleIndicesDecreasing(int lowerBound, int upperBound, int[] sortedIndices, Vector<int> values)
        {
            // Move all the indices that we're interested in to the beginning of the
            // array. Ignore the rest of the indices.
            if (lowerBound > 0)
            {
                for (var i = 0; i < (upperBound - lowerBound + 1); i++)
                {
                    Exchange(sortedIndices, i, i + lowerBound);
                }

                upperBound -= lowerBound;
                lowerBound = 0;
            }

            HeapSortDoublesIndices(lowerBound, upperBound, sortedIndices, values);
        }

        /// <summary>
        /// Sorts the elements of the <paramref name="values"/> vector in decreasing
        /// fashion using heap sort algorithm. The vector itself is not affected.
        /// </summary>
        /// <param name="lowerBound">The starting index.</param>
        /// <param name="upperBound">The stopping index.</param>
        /// <param name="sortedIndices">An array that will contain the sorted indices once the algorithm finishes.</param>
        /// <param name="values">The <see cref="Vector"/> that contains the values that need to be sorted.</param>
        private static void HeapSortDoublesIndices(int lowerBound, int upperBound, int[] sortedIndices, Vector<int> values)
        {
            var start = ((upperBound - lowerBound + 1) / 2) - 1 + lowerBound;
            var end = (upperBound - lowerBound + 1) - 1 + lowerBound;

            BuildDoubleIndexHeap(start, upperBound - lowerBound + 1, sortedIndices, values);

            while (end >= lowerBound)
            {
                Exchange(sortedIndices, end, lowerBound);
                SiftDoubleIndices(sortedIndices, values, lowerBound, end);
                end -= 1;
            }
        }

        /// <summary>
        /// Build heap for double indicies
        /// </summary>
        /// <param name="start">Root position</param>
        /// <param name="count">Length of <paramref name="values"/></param>
        /// <param name="sortedIndices">Indicies of <paramref name="values"/></param>
        /// <param name="values">Target <see cref="Vector"/></param>
        private static void BuildDoubleIndexHeap(int start, int count, int[] sortedIndices, Vector<int> values)
        {
            while (start >= 0)
            {
                SiftDoubleIndices(sortedIndices, values, start, count);
                start -= 1;
            }
        }

        /// <summary>
        /// Sift double indicies
        /// </summary>
        /// <param name="sortedIndices">Indicies of <paramref name="values"/></param>
        /// <param name="values">Target <see cref="Vector"/></param>
        /// <param name="begin">Root position</param>
        /// <param name="count">Length of <paramref name="values"/></param>
        private static void SiftDoubleIndices(int[] sortedIndices, Vector<int> values, int begin, int count)
        {
            var root = begin;

            while (root * 2 < count)
            {
                var child = root * 2;
                if ((child < count - 1) && (values[sortedIndices[child]] > values[sortedIndices[child + 1]]))
                {
                    child += 1;
                }

                if (values[sortedIndices[root]] <= values[sortedIndices[child]])
                {
                    return;
                }

                Exchange(sortedIndices, root, child);
                root = child;
            }
        }

        /// <summary>
        /// Sorts the given integers in a decreasing fashion.
        /// </summary>
        /// <param name="values">The values.</param>
        public static void SortIntegersDecreasing(int[] values)
        {
            HeapSortIntegers(values, values.Length);
        }

        /// <summary>
        /// Sort the given integers in a decreasing fashion using heapsort algorithm 
        /// </summary>
        /// <param name="values">Array of values to sort</param>
        /// <param name="count">Length of <paramref name="values"/></param>
        private static void HeapSortIntegers(int[] values, int count)
        {
            var start = (count / 2) - 1;
            var end = count - 1;

            BuildHeap(values, start, count);

            while (end >= 0)
            {
                Exchange(values, end, 0);
                Sift(values, 0, end);
                end -= 1;
            }
        }

        /// <summary>
        /// Build heap
        /// </summary>
        /// <param name="values">Target values array</param>
        /// <param name="start">Root position</param>
        /// <param name="count">Length of <paramref name="values"/></param>
        private static void BuildHeap(int[] values, int start, int count)
        {
            while (start >= 0)
            {
                Sift(values, start, count);
                start -= 1;
            }
        }

        /// <summary>
        /// Sift values
        /// </summary>
        /// <param name="values">Target value array</param>
        /// <param name="start">Root position</param>
        /// <param name="count">Length of <paramref name="values"/></param>
        private static void Sift(int[] values, int start, int count)
        {
            var root = start;

            while (root * 2 < count)
            {
                var child = root * 2;
                if ((child < count - 1) && (values[child] > values[child + 1]))
                {
                    child += 1;
                }

                if (values[root] > values[child])
                {
                    Exchange(values, root, child);
                    root = child;
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Exchange values in array
        /// </summary>
        /// <param name="values">Target values array</param>
        /// <param name="first">First value to exchange</param>
        /// <param name="second">Second value to exchange</param>
        private static void Exchange(int[] values, int first, int second)
        {
            var t = values[first];
            values[first] = values[second];
            values[second] = t;
        }
    }
}
