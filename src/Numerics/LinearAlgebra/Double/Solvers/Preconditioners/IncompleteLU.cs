// <copyright file="IncompleteLU.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Double.Solvers.Preconditioners
{
    using System;
    using Properties;

    /// <summary>
    /// An incomplete, level 0, LU factorization preconditioner.
    /// </summary>
    /// <remarks>
    /// The ILU(0) algorithm was taken from: <br/>
    /// Iterative methods for sparse linear systems <br/>
    /// Yousef Saad <br/>
    /// Algorithm is described in Chapter 10, section 10.3.2, page 275 <br/>
    /// </remarks>
    public sealed class IncompleteLU : IPreConditioner
    {
        /// <summary>
        /// The matrix holding the lower (L) and upper (U) matrices. The
        /// decomposition matrices are combined to reduce storage.
        /// </summary>
        private SparseMatrix _decompositionLU;

        /// <summary>
        /// Returns the upper triagonal matrix that was created during the LU decomposition.
        /// </summary>
        /// <returns>A new matrix containing the upper triagonal elements.</returns>
        internal Matrix UpperTriangle()
        {
            var result = new SparseMatrix(_decompositionLU.RowCount);
            for (var i = 0; i < _decompositionLU.RowCount; i++)
            {
                for (var j = i; j < _decompositionLU.ColumnCount; j++)
                {
                    result[i, j] = _decompositionLU[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the lower triagonal matrix that was created during the LU decomposition.
        /// </summary>
        /// <returns>A new matrix containing the lower triagonal elements.</returns>
        internal Matrix LowerTriangle()
        {
            var result = new SparseMatrix(_decompositionLU.RowCount);
            for (var i = 0; i < _decompositionLU.RowCount; i++)
            {
                for (var j = 0; j <= i; j++)
                {
                    if (i == j)
                    {
                        result[i, j] = 1.0;
                    }
                    else 
                    {
                        result[i, j] = _decompositionLU[i, j];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">The matrix upon which the preconditioner is based. </param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <see langword="null" />.</exception>
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

            _decompositionLU = SparseMatrix.OfMatrix(matrix);

            // M == A
            // for i = 2, ... , n do
            //     for k = 1, .... , i - 1 do
            //         if (i,k) == NZ(Z) then
            //             compute z(i,k) = z(i,k) / z(k,k);
            //             for j = k + 1, ...., n do
            //                 if (i,j) == NZ(Z) then
            //                     compute z(i,j) = z(i,j) - z(i,k) * z(k,j)
            //                 end
            //             end
            //         end
            //     end
            // end
            for (var i = 0; i < _decompositionLU.RowCount; i++)
            {
                for (var k = 0; k < i; k++)
                {
                    if (_decompositionLU[i, k] != 0.0)
                    {
                        var t = _decompositionLU[i, k] / _decompositionLU[k, k];
                        _decompositionLU[i, k] = t;
                        if (_decompositionLU[k, i] != 0.0)
                        {
                            _decompositionLU[i, i] = _decompositionLU[i, i] - (t * _decompositionLU[k, i]);
                        }

                        for (var j = k + 1; j < _decompositionLU.RowCount; j++)
                        {
                            if (j == i)
                            {
                                continue;
                            }

                            if (_decompositionLU[i, j] != 0.0)
                            {
                                _decompositionLU[i, j] = _decompositionLU[i, j] - (t * _decompositionLU[k, j]);
                            }
                        }
                    }
                }
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

            if (_decompositionLU == null)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDoesNotExist);
            }

            if (rhs.Count != _decompositionLU.ColumnCount)
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

            if (_decompositionLU == null)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDoesNotExist);
            }

            if ((lhs.Count != rhs.Count) || (lhs.Count != _decompositionLU.RowCount))
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            // Solve:
            // Lz = y
            // Which gives
            // for (int i = 1; i < matrix.RowLength; i++)
            // {
            //     z_i = l_ii^-1 * (y_i - SUM_(j<i) l_ij * z_j)
            // }
            // NOTE: l_ii should be 1 because u_ii has to be the value
            Vector rowValues = new DenseVector(_decompositionLU.RowCount);
            for (var i = 0; i < _decompositionLU.RowCount; i++)
            {
                // Clear the rowValues 
                rowValues.Clear();
                _decompositionLU.Row(i, rowValues);

                var sum = 0.0;
                for (var j = 0; j < i; j++)
                {
                    sum += rowValues[j] * lhs[j];
                }

                lhs[i] = rhs[i] - sum;
            }

            // Solve:
            // Ux = z
            // Which gives
            // for (int i = matrix.RowLength - 1; i > -1; i--)
            // {
            //     x_i = u_ii^-1 * (z_i - SUM_(j > i) u_ij * x_j)
            // }
            for (var i = _decompositionLU.RowCount - 1; i > -1; i--)
            {
                _decompositionLU.Row(i, rowValues);

                var sum = 0.0;
                for (var j = _decompositionLU.RowCount - 1; j > i; j--)
                {
                    sum += rowValues[j] * lhs[j];
                }

                lhs[i] = 1 / rowValues[i] * (lhs[i] - sum);
            }
        }
    }
}
