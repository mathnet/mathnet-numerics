// <copyright file="IncompleteLU.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Integer.Solvers
{
    /// <summary>
    /// An incomplete, level 0, LU factorization preconditioner.
    /// </summary>
    /// <remarks>
    /// The ILU(0) algorithm was taken from: <br/>
    /// Iterative methods for sparse linear systems <br/>
    /// Yousef Saad <br/>
    /// Algorithm is described in Chapter 10, section 10.3.2, page 275 <br/>
    /// </remarks>
    public sealed class ILU0Preconditioner : IPreconditioner<int>
    {
        public ILU0Preconditioner()
        {
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        /// <summary>
        /// Returns the upper triagonal matrix that was created during the LU decomposition.
        /// </summary>
        /// <returns>A new matrix containing the upper triagonal elements.</returns>
        internal Matrix<int> UpperTriangle()
        {
            // Shouldn't be possible as this cannot be constructed
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        /// <summary>
        /// Returns the lower triagonal matrix that was created during the LU decomposition.
        /// </summary>
        /// <returns>A new matrix containing the lower triagonal elements.</returns>
        internal Matrix<int> LowerTriangle()
        {
            // Shouldn't be possible as this cannot be constructed
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">The matrix upon which the preconditioner is based. </param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public void Initialize(Matrix<int> matrix)
        {
            // Shouldn't be possible as this cannot be constructed
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="lhs">The left hand side vector. Also known as the result vector.</param>
        public void Approximate(Vector<int> rhs, Vector<int> lhs)
        {
            // Shouldn't be possible as this cannot be constructed
            throw new NotSupportedException(Resources.NotSupportedForIntegerVectors);
        }
    }
}
