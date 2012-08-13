// <copyright file="Diagonal.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex.Solvers.Preconditioners
{
    using System;
    using System.Numerics;
    using Properties;

    /// <summary>
    /// A diagonal preconditioner. The preconditioner uses the inverse
    /// of the matrix diagonal as preconditioning values.
    /// </summary>
    public sealed class Diagonal : IPreConditioner
    {
        /// <summary>
        /// The inverse of the matrix diagonal.
        /// </summary>
        private Complex[] _inverseDiagonals;

        /// <summary>
        /// Returns the decomposed matrix diagonal.
        /// </summary>
        /// <returns>The matrix diagonal.</returns>
        internal DiagonalMatrix DiagonalEntries()
        {
            var result = new DiagonalMatrix(_inverseDiagonals.Length);
            for (var i = 0; i < _inverseDiagonals.Length; i++)
            {
                result.At(i, i, 1 / _inverseDiagonals[i]);
            }

            return result;
        }

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">
        /// The <see cref="Matrix"/> upon which this preconditioner is based.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <see langword="null" />. </exception>
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

            _inverseDiagonals = new Complex[matrix.RowCount];
            for (var i = 0; i < matrix.RowCount; i++)
            {
                _inverseDiagonals[i] = 1 / matrix.At(i, i);
            }
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>The left hand side vector.</returns>
        public Vector Approximate(Vector rhs)
        {
            if (rhs == null)
            {
                throw new ArgumentNullException("rhs");
            }

            if (_inverseDiagonals == null)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDoesNotExist);
            }

            if (rhs.Count != _inverseDiagonals.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rhs");
            }

            Vector result = new DenseVector(rhs.Count);
            Approximate(rhs, result);
            return result;
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="lhs">The left hand side vector. Also known as the result vector.</param>
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

            if (_inverseDiagonals == null)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDoesNotExist);
            }

            if ((lhs.Count != rhs.Count) || (lhs.Count != _inverseDiagonals.Length))
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rhs");
            }

            for (var i = 0; i < _inverseDiagonals.Length; i++)
            {
                lhs[i] = rhs[i] * _inverseDiagonals[i];
            }
        }
    }
}
