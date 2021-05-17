// <copyright file="ManagedLinearAlgebraProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2021 Math.NET
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

namespace MathNet.Numerics.Providers.LinearAlgebra
{
    /// <summary>
    /// The managed linear algebra provider.
    /// </summary>
    public sealed partial class ManagedLinearAlgebraProvider : ILinearAlgebraProvider
    {
        public static ManagedLinearAlgebraProvider Instance { get; } = new ManagedLinearAlgebraProvider();

        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public bool IsAvailable()
        {
            return true;
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        public void InitializeVerify()
        {
        }

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        public void FreeResources()
        {
        }

        public override string ToString()
        {
            return "Managed";
        }

        /// <summary>
        /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
        /// </summary>
        static void GetRow<T>(Transpose transpose, int rowindx, int numRows, int numCols, T[] matrix, T[] row)
        {
            if (transpose == Transpose.DontTranspose)
            {
                for (int i = 0; i < numCols; i++)
                {
                    row[i] = matrix[(i * numRows) + rowindx];
                }
            }
            else
            {
                Array.Copy(matrix, rowindx * numCols, row, 0, numCols);
            }
        }

        /// <summary>
        /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
        /// </summary>
        static void GetColumn<T>(Transpose transpose, int colindx, int numRows, int numCols, T[] matrix, T[] column)
        {
            if (transpose == Transpose.DontTranspose)
            {
                Array.Copy(matrix, colindx * numRows, column, 0, numRows);
            }
            else
            {
                for (int i = 0; i < numRows; i++)
                {
                    column[i] = matrix[(i * numCols) + colindx];
                }
            }
        }
    }
}
