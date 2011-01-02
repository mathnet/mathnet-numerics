// <copyright file="UnitPreconditioner.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Single.Solvers.Preconditioners
{
    using System;
    using Properties;

    /// <summary>
    /// A unit preconditioner. This preconditioner does not actually do anything
    /// it is only used when running an <see cref="IIterativeSolver"/> without
    /// a preconditioner.
    /// </summary>
    internal sealed class UnitPreconditioner : IPreConditioner
    {
        /// <summary>
        /// The coefficient matrix on which this preconditioner operates.
        /// Is used to check dimensions on the different vectors that are processed.
        /// </summary>
        private int _size;
        
        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">
        /// The matrix upon which the preconditioner is based.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <see langword="null"/>. </exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public void Initialize(Matrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, "matrix");
            }

            _size = matrix.RowCount;
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="lhs">The left hand side vector. Also known as the result vector.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="rhs"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="lhs"/> is <see langword="null"/>. </exception>
        /// <exception cref="ArgumentException">
        ///   <para>
        ///     If <paramref name="rhs"/> and <paramref name="lhs"/> do not have the same size.
        ///   </para>
        ///   <para>
        ///     - or -
        ///   </para>
        ///   <para>
        ///     If the size of <paramref name="rhs"/> is different the number of rows of the coefficient matrix.
        ///   </para>
        /// </exception>
        public void Approximate(Vector rhs, Vector lhs)
        {
            if (rhs == null)
            {
                throw new ArgumentNullException("rhs");
            }

            if (lhs == null)
            {
                throw new ArgumentNullException("lhs");
            }

            if ((lhs.Count != rhs.Count) || (lhs.Count != _size))
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            rhs.CopyTo(lhs);
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>The left hand side vector.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rhs"/> is <see langword="null"/>. </exception>
        /// <exception cref="ArgumentException">
        /// If the size of <paramref name="rhs"/> is different the number of rows of the coefficient matrix.
        /// </exception>
        public Vector Approximate(Vector rhs)
        {
            if (rhs == null)
            {
                throw new ArgumentNullException("rhs");
            }

            if (rhs.Count != _size)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            Vector result = new DenseVector(rhs.Count);
            Approximate(rhs, result);
            return result;
        }
    }
}
