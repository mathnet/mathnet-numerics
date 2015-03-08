﻿// <copyright file="LU.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Integer.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of an LU factorization.</para>
    /// <para>For a matrix A, the LU factorization is a pair of lower triangular matrix L and
    /// upper triangular matrix U so that A = L*U.</para>
    /// <para>In the Math.Net implementation we also store a set of pivot elements for increased 
    /// numerical stability. The pivot elements encode a permutation matrix P such that P*A = L*U.</para>
    /// </summary>
    /// <exception cref="NotSupportedException">at construction: Not Supported For Integer Matrices</exception>
    /// <remarks>
    /// The computation of the LU factorization is done at construction time.
    /// </remarks>
    internal abstract class LU : LU<int>
    {
        protected LU(Matrix<int> factors, int[] pivots)
            : base(factors, pivots)
        {
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the LU factorization was computed.
        /// </summary>
        public override int Determinant
        {
            get
            {
                // Shouldn't be possible as this cannot be constructed
                throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
            }
        }
    }
}
