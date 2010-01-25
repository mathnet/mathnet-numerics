// <copyright file="Matrix.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using System;
    using System.Text;
    using Properties;
    using Threading;

    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    public abstract partial class Matrix
    {
        /// <summary>
        /// Adds another matrix to this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public virtual void Add(Matrix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            Parallel.For(
                0, 
                RowCount, 
                i =>
                {
                    for (int j = 0; j < ColumnCount; j++)
                    {
                        At(i, j, At(i, j) + other.At(i, j));
                    }
                });
        }

        /// <summary>
        /// Subtracts another matrix from this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public virtual void Subtract(Matrix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            Parallel.For(
                0,
                RowCount,
                i =>
                {
                    for (int j = 0; j < ColumnCount; j++)
                    {
                        At(i, j, At(i, j) - other.At(i, j));
                    }
                });
        }
        /// <summary>
        /// Multiplies each element of this matrix with a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to multiply with.</param>
        public virtual void Multiply(double scalar)
        {
            if (Precision.AlmostEqualInDecimalPlaces(1.0, scalar, 15))
            {
                return;
            }

            Parallel.For(
                0,
                RowCount,
                i =>
                {
                    for (int j = 0; j < ColumnCount; j++)
                    {
                        At(i, j, At(i, j) * scalar);
                    }
                });
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to multiply.</param>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void Multiply(double scalar, Matrix result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount)
            {
                throw new ArgumentException("result", Resources.ArgumentMatrixSameRowDimension);
            }

            if (result.ColumnCount != ColumnCount)
            {
                throw new ArgumentException("result", Resources.ArgumentMatrixSameColumnDimension);
            }

            CopyTo(result);
            result.Multiply(scalar);
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="NotConformableException">If <strong>this.Columns != other.Rows</strong>.</exception>
        /// <exception cref="NotConformableException">If the result matrix's dimensions are not the this.Rows x other.Columns.</exception>
        public virtual void Multiply(Matrix other, Matrix result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (result.RowCount != RowCount || result.ColumnCount != other.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                Matrix tmp = result.CreateMatrix(result.RowCount, result.ColumnCount);
                Multiply(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                Parallel.For(0, this.RowCount, j =>
                {
                    for (int i = 0; i != other.ColumnCount; i++)
                    {
                        double s = 0;
                        for (int l = 0; l < this.ColumnCount; l++)
                        {
                            s += this.At(j, l) * other.At(l, i);
                        }
                        result.At(j, i, s);
                    }
                });
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and returns the result.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="NotConformableException">If <strong>this.Columns != other.Rows</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        public virtual Matrix Multiply(Matrix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (ColumnCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            Matrix result = CreateMatrix(RowCount, other.ColumnCount);
            Multiply(other, result);
            return result;
        }

        /// <summary>
        /// Adds two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to add.</param>
        /// <param name="rightSide">The right matrix to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix operator +(Matrix leftSide, Matrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.RowCount != rightSide.RowCount || leftSide.ColumnCount != rightSide.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            Matrix ret = leftSide.Clone();
            ret.Add(rightSide);
            return ret;
        }

        /// <summary>
        /// Subtracts two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to subtract.</param>
        /// <param name="rightSide">The right matrix to subtract.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix operator -(Matrix leftSide, Matrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.RowCount != rightSide.RowCount || leftSide.ColumnCount != rightSide.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            Matrix ret = leftSide.Clone();
            ret.Subtract(rightSide);
            return ret;
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Matrix operator *(Matrix leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            Matrix ret = leftSide.Clone();
            ret.Multiply(rightSide);
            return ret;
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix operator *(double leftSide, Matrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            Matrix ret = rightSide.Clone();
            ret.Multiply(leftSide);
            return ret;
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to multiply.</param>
        /// <param name="rightSide">The right matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="NotConformableException">If the dimensions of <paramref name="leftSide"/> or <paramref name="rightSide"/> don't conform.</exception>
        public static Matrix operator *(Matrix leftSide, Matrix rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide.ColumnCount != rightSide.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            return leftSide.Multiply(rightSide);
        }
    }
}