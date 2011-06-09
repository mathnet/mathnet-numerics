﻿// <copyright file="Matrix.Arithmetic.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Generic
{
    using System;
    using Distributions;
    using Factorization;
    using Properties;

    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    public abstract partial class Matrix<T>
    {
        /// <summary>
        /// The value of 1.0.
        /// </summary>
        private static readonly T One = Common.SetOne<T>();

        /// <summary>
        /// The value of 0.0.
        /// </summary>
        private static readonly T Zero = default(T);

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public virtual Matrix<T> Add(Matrix<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            var result = CreateMatrix(RowCount, ColumnCount);
            DoAdd(other, result);
            return result;
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public virtual void Add(Matrix<T> other, Matrix<T> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException("other", Resources.ArgumentMatrixDimensions);
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException("result", Resources.ArgumentMatrixDimensions);
            }

            DoAdd(other, result);
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected abstract void DoAdd(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public virtual Matrix<T> Subtract(Matrix<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            var result = CreateMatrix(RowCount, ColumnCount);
            DoSubtract(other, result);
            return result;
        }

        /// <summary>
        /// Subtracts another matrix from this matrix. 
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public virtual void Subtract(Matrix<T> other, Matrix<T> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            DoSubtract(other, result);
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        protected abstract void DoSubtract(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Multiplies each element of this matrix with a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        public virtual Matrix<T> Multiply(T scalar)
        {
            if (scalar.Equals(One))
            {
                return Clone();
            }

            if (scalar.Equals(Zero))
            {
                return CreateMatrix(RowCount, ColumnCount);
            }

            var result = CreateMatrix(RowCount, ColumnCount);
            DoMultiply(scalar, result);
            return result;
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void Multiply(T scalar, Matrix<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "result");
            }

            if (result.ColumnCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension, "result");
            }
            
            if (scalar.Equals(One))
            {
                CopyTo(result);
                return;
            }
            
            if (scalar.Equals(Zero))
            {
                result.Clear();
                return;
            }

            DoMultiply(scalar, result);
        }

        /// <summary>
        /// Divides each element of this matrix with a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to divide with.</param>
        /// <returns>The result of the division.</returns>
        public virtual Matrix<T> Divide(T scalar)
        {
            if (scalar.Equals(One))
            {
                return Clone();
            }

            if (scalar.Equals(Zero))
            {
                throw new DivideByZeroException();
            }

            var result = CreateMatrix(RowCount, ColumnCount);
            DoDivide(scalar, result);
            return result;
        }

        /// <summary>
        /// Divides each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to divide the matrix with.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void Divide(T scalar, Matrix<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "result");
            }

            if (result.ColumnCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension, "result");
            }

            if (scalar.Equals(One))
            {
                CopyTo(result);
                return;
            }

            if (scalar.Equals(Zero))
            {
                throw new DivideByZeroException();
            }

            DoDivide(scalar, result);
        }

        /// <summary>
        /// Divides each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to divide the matrix with.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        protected abstract void DoDivide(T scalar, Matrix<T> result);
        
        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        protected abstract void DoMultiply(T scalar, Matrix<T> result);

        /// <summary>
        /// Multiplies this matrix by a vector and returns the result.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <c>this.ColumnCount != rightSide.Count</c>.</exception>
        public virtual Vector<T> Multiply(Vector<T> rightSide)
        {
            var ret = CreateVector(RowCount);
            Multiply(rightSide, ret);
            return ret;
        }

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>result.Count != this.RowCount</strong>.</exception>
        /// <exception cref="ArgumentException">If <strong>this.ColumnCount != <paramref name="rightSide"/>.Count</strong>.</exception>
        public virtual void Multiply(Vector<T> rightSide, Vector<T> result)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (ColumnCount != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "rightSide");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (RowCount != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ReferenceEquals(rightSide, result))
            {
                var tmp = result.CreateVector(result.Count);
                Multiply(rightSide, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoMultiply(rightSide, result);
            }
        }

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoMultiply(Vector<T> rightSide, Vector<T> result);
       
        /// <summary>
        /// Left multiply a matrix with a vector ( = vector * matrix ).
        /// </summary>
        /// <param name="leftSide">The vector to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>this.RowCount != <paramref name="leftSide"/>.Count</strong>.</exception>
        public virtual Vector<T> LeftMultiply(Vector<T> leftSide)
        {
            var ret = CreateVector(ColumnCount);
            LeftMultiply(leftSide, ret);
            return ret;
        }

        /// <summary>
        /// Left multiply a matrix with a vector ( = vector * matrix ) and place the result in the result vector.
        /// </summary>
        /// <param name="leftSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>result.Count != this.ColumnCount</strong>.</exception>
        /// <exception cref="ArgumentException">If <strong>this.RowCount != <paramref name="leftSide"/>.Count</strong>.</exception>
        public virtual void LeftMultiply(Vector<T> leftSide, Vector<T> result)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (RowCount != leftSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "leftSide");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ReferenceEquals(leftSide, result))
            {
                var tmp = result.CreateVector(result.Count);
                LeftMultiply(leftSide, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoLeftMultiply(leftSide, result);
            }
        }

        /// <summary>
        /// Left multiply a matrix with a vector ( = vector * matrix ) and place the result in the result vector.
        /// </summary>
        /// <param name="leftSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected void DoLeftMultiply(Vector<T> leftSide, Vector<T> result)
        {
            DoTransposeThisAndMultiply(leftSide, result);
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.Rows x other.Columns.</exception>
        public virtual void Multiply(Matrix<T> other, Matrix<T> result)
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
                var tmp = result.CreateMatrix(result.RowCount, result.ColumnCount);
                Multiply(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoMultiply(other, result);
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <returns>The result of the multiplication.</returns>
        public virtual Matrix<T> Multiply(Matrix<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (ColumnCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var result = CreateMatrix(RowCount, other.ColumnCount);
            Multiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoMultiply(Matrix<T> other, Matrix<T> result);
  
        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.ColumnCount</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.RowCount x other.RowCount.</exception>
        public virtual void TransposeAndMultiply(Matrix<T> other, Matrix<T> result)
               {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != other.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if ((result.RowCount != RowCount) || (result.ColumnCount != other.RowCount))
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateMatrix(result.RowCount, result.ColumnCount);
                TransposeAndMultiply(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoTransposeAndMultiply(other, result);
            }
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.ColumnCount</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <returns>The result of the multiplication.</returns>
        public virtual Matrix<T> TransposeAndMultiply(Matrix<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (ColumnCount != other.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var result = CreateMatrix(RowCount, other.RowCount);
            TransposeAndMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoTransposeAndMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Multiplies the transpose of this matrix by a vector and returns the result.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <c>this.RowCount != rightSide.Count</c>.</exception>
        public virtual Vector<T> TransposeThisAndMultiply(Vector<T> rightSide)
        {
            var ret = CreateVector(RowCount);
            TransposeThisAndMultiply(rightSide, ret);
            return ret;
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>result.Count != this.ColumnCount</strong>.</exception>
        /// <exception cref="ArgumentException">If <strong>this.RowCount != <paramref name="rightSide"/>.Count</strong>.</exception>
        public virtual void TransposeThisAndMultiply(Vector<T> rightSide, Vector<T> result)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (RowCount != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "rightSide");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ReferenceEquals(rightSide, result))
            {
                var tmp = result.CreateVector(result.Count);
                TransposeThisAndMultiply(rightSide, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoTransposeThisAndMultiply(rightSide, result);
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoTransposeThisAndMultiply(Vector<T> rightSide, Vector<T> result);

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix. 
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>this.Rows != other.RowCount</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.ColumnCount x other.ColumnCount.</exception>
        public virtual void TransposeThisAndMultiply(Matrix<T> other, Matrix<T> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (RowCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if ((result.RowCount != ColumnCount) || (result.ColumnCount != other.ColumnCount))
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateMatrix(result.RowCount, result.ColumnCount);
                TransposeThisAndMultiply(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoTransposeThisAndMultiply(other, result);
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Rows != other.RowCount</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <returns>The result of the multiplication.</returns>
        public virtual Matrix<T> TransposeThisAndMultiply(Matrix<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (RowCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var result = CreateMatrix(ColumnCount, other.ColumnCount);
            TransposeThisAndMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoTransposeThisAndMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Negate each element of this matrix.
        /// </summary>
        /// <returns>A matrix containing the negated values.</returns>
        public virtual Matrix<T> Negate()
        {
            var result = CreateMatrix(RowCount, ColumnCount);
            DoNegate(result);
            return result;
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">if the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void Negate(Matrix<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            DoNegate(result);
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected abstract void DoNegate(Matrix<T> result);

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
        public static Matrix<T> operator +(Matrix<T> leftSide, Matrix<T> rightSide)
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

            return leftSide.Add(rightSide);
        }

        /// <summary>
        /// Returns a <strong>Matrix</strong> containing the same values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <param name="rightSide">The matrix to get the values from.</param>
        /// <returns>A matrix containing a the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator +(Matrix<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Clone();
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
        public static Matrix<T> operator -(Matrix<T> leftSide, Matrix<T> rightSide)
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

            return leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Negates each element of the matrix.
        /// </summary>
        /// <param name="rightSide">The matrix to negate.</param>
        /// <returns>A matrix containing the negated values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator -(Matrix<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Negate();
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator *(Matrix<T> leftSide, T rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator *(T leftSide, Matrix<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Multiply(leftSide);
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
        /// <exception cref="ArgumentException">If the dimensions of <paramref name="leftSide"/> or <paramref name="rightSide"/> don't conform.</exception>
        public static Matrix<T> operator *(Matrix<T> leftSide, Matrix<T> rightSide)
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

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> and a Vector.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The vector to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator *(Matrix<T> leftSide, Vector<T> rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a Vector and a <strong>Matrix</strong>.
        /// </summary>
        /// <param name="leftSide">The vector to multiply.</param>
        /// <param name="rightSide">The matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator *(Vector<T> leftSide, Matrix<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.LeftMultiply(leftSide);
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new matrix that is the pointwise multiplication of this matrix and <paramref name="other"/>.</returns>
        public virtual Matrix<T> PointwiseMultiply(Matrix<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "other");
            }

            var result = CreateMatrix(RowCount, ColumnCount);
            PointwiseMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public virtual void PointwiseMultiply(Matrix<T> other, Matrix<T> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            DoPointwiseMultiply(other, result);
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise subtract this one by.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new matrix that is the pointwise division of this matrix and <paramref name="other"/>.</returns>
        public virtual Matrix<T> PointwiseDivide(Matrix<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "other");
            }

            var result = CreateMatrix(RowCount, ColumnCount);
            PointwiseDivide(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected abstract void DoPointwiseMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public virtual void PointwiseDivide(Matrix<T> other, Matrix<T> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            DoPointwiseDivide(other, result);
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected abstract void DoPointwiseDivide(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Computes the modulus for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <returns>A matrix containing the results.</returns>
        public Matrix<T> Modulus(T divisor)
        {
            var result = CreateMatrix(RowCount, ColumnCount);
            DoModulus(divisor, result);
            return result;
        }

        /// <summary>
        /// Computes the modulus for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        public void Modulus(T divisor, Matrix<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            DoModulus(divisor, result);
        }

        /// <summary>
        /// Computes the modulus for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected abstract void DoModulus(T divisor, Matrix<T> result);

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator %(Matrix<T> leftSide, T rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.Modulus(rightSide);
        }

        /// <summary>
        /// Generates a matrix with random elements.
        /// </summary>
        /// <param name="numberOfRows">Number of rows.</param>
        /// <param name="numberOfColumns">Number of columns.</param>
        /// <param name="distribution">Continuous Random Distribution to generate elements from.</param>
        /// <returns>
        /// An <c>numberOfRows</c>-by-<c>numberOfColumns</c> matrix with elements distributed according to the provided distribution.
        /// </returns>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfRows"/> is not positive.</exception>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfColumns"/> is not positive.</exception>
        public virtual Matrix<T> Random(int numberOfRows, int numberOfColumns, IContinuousDistribution distribution)
        {
            if (numberOfRows < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfRows");
            }

            if (numberOfColumns < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfColumns");
            }

            var matrix = CreateMatrix(numberOfRows, numberOfColumns);
            DoRandom(matrix, distribution);
            return matrix;
        }

        /// <summary>
        /// Populates a matrix with random elements.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="distribution">Continuous Random Distribution to generate elements from.</param>
       protected abstract void DoRandom(Matrix<T> matrix, IContinuousDistribution distribution);

        /// <summary>
        /// Generates a matrix with random elements.
        /// </summary>
        /// <param name="numberOfRows">Number of rows.</param>
        /// <param name="numberOfColumns">Number of columns.</param>
        /// <param name="distribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// An <c>numberOfRows</c>-by-<c>numberOfColumns</c> matrix with elements distributed according to the provided distribution.
        /// </returns>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfRows"/> is not positive.</exception>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfColumns"/> is not positive.</exception>
        public virtual Matrix<T> Random(int numberOfRows, int numberOfColumns, IDiscreteDistribution distribution)
        {
            if (numberOfRows < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfRows");
            }

            if (numberOfColumns < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfColumns");
            }

            var matrix = CreateMatrix(numberOfRows, numberOfColumns);
            DoRandom(matrix, distribution);
            return matrix;
        }

        /// <summary>
        /// Populates a matrix with random elements.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="distribution">Continuous Random Distribution to generate elements from.</param>
        protected abstract void DoRandom(Matrix<T> matrix, IDiscreteDistribution distribution);

        /// <summary>
        /// Computes the trace of this matrix.
        /// </summary>
        /// <returns>The trace of this matrix</returns>
        /// <exception cref="ArgumentException">If the matrix is not square</exception>
        public abstract T Trace();

        /// <summary>
        /// Calculates the rank of the matrix
        /// </summary>
        /// <returns>effective numerical rank, obtained from SVD</returns>
        public virtual int Rank()
        {
            return Svd<T>.Create(this, false).Rank;
        }

        /// <summary>Calculates the condition number of this matrix.</summary>
        /// <returns>The condition number of the matrix.</returns>
        /// <remarks>The condition number is calculated using singular value decomposition.</remarks>
        public virtual T ConditionNumber()
        {
            return Svd<T>.Create(this, false).ConditionNumber;
        }

        /// <summary>Computes the determinant of this matrix.</summary>
        /// <returns>The determinant of this matrix.</returns>
        public virtual T Determinant()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            return LU<T>.Create(this).Determinant;
        }

        /// <summary>Computes the inverse of this matrix.</summary>
        /// <returns>The inverse of this matrix.</returns>
        public virtual Matrix<T> Inverse()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            return LU<T>.Create(this).Inverse();
        }

        /// <summary>
        /// Computes the Kronecker product of this matrix with the given matrix. The new matrix is M-by-N
        /// with M = this.Rows * lower.Rows and N = this.Columns * lower.Columns.
        /// </summary>
        /// <param name="other">The other matrix.</param>
        /// <exception cref="ArgumentNullException">If other is <see langword="null" />.</exception>
        /// <returns>The kronecker product of the two matrices.</returns>
        public virtual Matrix<T> KroneckerProduct(Matrix<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            var result = CreateMatrix(RowCount * other.RowCount, ColumnCount * other.ColumnCount);
            KroneckerProduct(other, result);
            return result;
        }

        /// <summary>
        /// Computes the Kronecker product of this matrix with the given matrix. The new matrix is M-by-N
        /// with M = this.Rows * lower.Rows and N = this.Columns * lower.Columns.
        /// </summary>
        /// <param name="other">The other matrix.</param>
        /// <param name="result">The kronecker product of the two matrices.</param>
        /// <exception cref="ArgumentNullException">If other is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not (this.Rows * lower.rows) x (this.Columns * lower.Columns).</exception>
        public virtual void KroneckerProduct(Matrix<T> other, Matrix<T> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != (RowCount * other.RowCount) || result.ColumnCount != (ColumnCount * other.ColumnCount))
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            for (var j = 0; j < ColumnCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    result.SetSubMatrix(i * other.RowCount, other.RowCount, j * other.ColumnCount, other.ColumnCount, At(i, j) * other);
                }
            }
        }

        /// <summary>
        /// Normalizes the columns of a matrix.
        /// </summary>
        /// <param name="p">The norm under which to normalize the columns under.</param>
        /// <returns>A normalized version of the matrix.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the parameter p is not positive.</exception>
        public virtual Matrix<T> NormalizeColumns(int p)
        {
            if (p < 1)
            {
                throw new ArgumentOutOfRangeException("p", Resources.ArgumentMustBePositive);
            }

            var ret = Clone();

            for (var index = 0; index < ColumnCount; index++)
            {
                ret.SetColumn(index, Column(index).Normalize(p));
            }

            return ret;
        }

        /// <summary>
        /// Normalizes the rows of a matrix.
        /// </summary>
        /// <param name="p">The norm under which to normalize the rows under.</param>
        /// <returns>A normalized version of the matrix.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the parameter p is not positive.</exception>
        public virtual Matrix<T> NormalizeRows(int p)
        {
            if (p < 1)
            {
                throw new ArgumentOutOfRangeException("p", Resources.ArgumentMustBePositive);
            }

            var ret = Clone();

            for (var index = 0; index < RowCount; index++)
            {
                ret.SetRow(index, Row(index).Normalize(p));
            }

            return ret;
        }
    }
}
