// <copyright file="Matrix.Arithmetic.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using System.Linq;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    public abstract partial class Matrix<T>
    {
        /// <summary>
        /// The value of 1.0.
        /// </summary>
        public static readonly T One = BuilderInstance<T>.Matrix.One;

        /// <summary>
        /// The value of 0.0.
        /// </summary>
        public static readonly T Zero = BuilderInstance<T>.Matrix.Zero;

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected abstract void DoNegate(Matrix<T> result);

        /// <summary>
        /// Complex conjugates each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the conjugation.</param>
        protected abstract void DoConjugate(Matrix<T> result);

        /// <summary>
        /// Add a scalar to each element of the matrix and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        protected abstract void DoAdd(T scalar, Matrix<T> result);

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected abstract void DoAdd(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Subtracts a scalar from each element of the matrix and stores the result in the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        protected abstract void DoSubtract(T scalar, Matrix<T> result);

        /// <summary>
        /// Subtracts each element of the matrix from a scalar and stores the result in the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to subtract from.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        protected void DoSubtractFrom(T scalar, Matrix<T> result)
        {
            DoNegate(result);
            result.DoAdd(scalar, result);
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        protected abstract void DoSubtract(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        protected abstract void DoMultiply(T scalar, Matrix<T> result);

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoMultiply(Vector<T> rightSide, Vector<T> result);

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Multiplies this matrix with the transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoTransposeAndMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Multiplies this matrix with the conjugate transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoConjugateTransposeAndMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoTransposeThisAndMultiply(Vector<T> rightSide, Vector<T> result);

        /// <summary>
        /// Multiplies the conjugate transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoConjugateTransposeThisAndMultiply(Vector<T> rightSide, Vector<T> result);

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoTransposeThisAndMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected abstract void DoConjugateTransposeThisAndMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Divides each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        protected abstract void DoDivide(T divisor, Matrix<T> result);

        /// <summary>
        /// Divides a scalar by each element of the matrix and stores the result in the result matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        protected abstract void DoDivideByThis(T dividend, Matrix<T> result);

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given divisor each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected abstract void DoModulus(T divisor, Matrix<T> result);

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given dividend for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected abstract void DoModulusByThis(T dividend, Matrix<T> result);

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for the given divisor each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected abstract void DoRemainder(T divisor, Matrix<T> result);

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for the given dividend for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected abstract void DoRemainderByThis(T dividend, Matrix<T> result);

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected abstract void DoPointwiseMultiply(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected abstract void DoPointwiseDivide(Matrix<T> divisor, Matrix<T> result);

        /// <summary>
        /// Pointwise raise this matrix to an exponent and store the result into the result matrix.
        /// </summary>
        /// <param name="exponent">The exponent to raise this matrix values to.</param>
        /// <param name="result">The matrix to store the result of the pointwise power.</param>
        protected abstract void DoPointwisePower(T exponent, Matrix<T> result);

        /// <summary>
        /// Pointwise raise this matrix to an exponent matrix and store the result into the result matrix.
        /// </summary>
        /// <param name="exponent">The exponent matrix to raise this matrix values to.</param>
        /// <param name="result">The matrix to store the result of the pointwise power.</param>
        protected abstract void DoPointwisePower(Matrix<T> exponent, Matrix<T> result);

        /// <summary>
        /// Pointwise canonical modulus, where the result has the sign of the divisor,
        /// of this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use</param>
        /// <param name="result">The result of the modulus.</param>
        protected abstract void DoPointwiseModulus(Matrix<T> divisor, Matrix<T> result);

        /// <summary>
        /// Pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use</param>
        /// <param name="result">The result of the modulus.</param>
        protected abstract void DoPointwiseRemainder(Matrix<T> divisor, Matrix<T> result);

        /// <summary>
        /// Pointwise applies the exponential function to each value and stores the result into the result matrix.
        /// </summary>
        /// <param name="result">The matrix to store the result.</param>
        protected abstract void DoPointwiseExp(Matrix<T> result);

        /// <summary>
        /// Pointwise applies the natural logarithm function to each value and stores the result into the result matrix.
        /// </summary>
        /// <param name="result">The matrix to store the result.</param>
        protected abstract void DoPointwiseLog(Matrix<T> result);

        protected abstract void DoPointwiseAbs(Matrix<T> result);
        protected abstract void DoPointwiseAcos(Matrix<T> result);
        protected abstract void DoPointwiseAsin(Matrix<T> result);
        protected abstract void DoPointwiseAtan(Matrix<T> result);
        protected abstract void DoPointwiseCeiling(Matrix<T> result);
        protected abstract void DoPointwiseCos(Matrix<T> result);
        protected abstract void DoPointwiseCosh(Matrix<T> result);
        protected abstract void DoPointwiseFloor(Matrix<T> result);
        protected abstract void DoPointwiseLog10(Matrix<T> result);
        protected abstract void DoPointwiseRound(Matrix<T> result);
        protected abstract void DoPointwiseSign(Matrix<T> result);
        protected abstract void DoPointwiseSin(Matrix<T> result);
        protected abstract void DoPointwiseSinh(Matrix<T> result);
        protected abstract void DoPointwiseSqrt(Matrix<T> result);
        protected abstract void DoPointwiseTan(Matrix<T> result);
        protected abstract void DoPointwiseTanh(Matrix<T> result);
        protected abstract void DoPointwiseAtan2(Matrix<T> other, Matrix<T> result);
        protected abstract void DoPointwiseMinimum(T scalar, Matrix<T> result);
        protected abstract void DoPointwiseMinimum(Matrix<T> other, Matrix<T> result);
        protected abstract void DoPointwiseMaximum(T scalar, Matrix<T> result);
        protected abstract void DoPointwiseMaximum(Matrix<T> other, Matrix<T> result);
        protected abstract void DoPointwiseAbsoluteMinimum(T scalar, Matrix<T> result);
        protected abstract void DoPointwiseAbsoluteMinimum(Matrix<T> other, Matrix<T> result);
        protected abstract void DoPointwiseAbsoluteMaximum(T scalar, Matrix<T> result);
        protected abstract void DoPointwiseAbsoluteMaximum(Matrix<T> other, Matrix<T> result);

        /// <summary>
        /// Adds a scalar to each element of the matrix.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public Matrix<T> Add(T scalar)
        {
            if (scalar.Equals(Zero))
            {
                return Clone();
            }

            var result = Build.SameAs(this);
            DoAdd(scalar, result);
            return result;
        }

        /// <summary>
        /// Adds a scalar to each element of the matrix and stores the result in the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public void Add(T scalar, Matrix<T> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, result, "result");
            }

            if (scalar.Equals(Zero))
            {
                CopyTo(result);
                return;
            }

            DoAdd(scalar, result);
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public Matrix<T> Add(Matrix<T> other)
        {
            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, other);
            }

            var result = Build.SameAs(this, other, RowCount, ColumnCount);
            DoAdd(other, result);
            return result;
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public void Add(Matrix<T> other, Matrix<T> result)
        {
            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, other, "other");
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, result, "result");
            }

            DoAdd(other, result);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the matrix.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <returns>A new matrix containing the subtraction of this matrix and the scalar.</returns>
        public Matrix<T> Subtract(T scalar)
        {
            if (scalar.Equals(Zero))
            {
                return Clone();
            }

            var result = Build.SameAs(this);
            DoSubtract(scalar, result);
            return result;
        }

        /// <summary>
        /// Subtracts a scalar from each element of the matrix and stores the result in the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void Subtract(T scalar, Matrix<T> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, result, "result");
            }

            if (scalar.Equals(Zero))
            {
                CopyTo(result);
                return;
            }

            DoSubtract(scalar, result);
        }

        /// <summary>
        /// Subtracts each element of the matrix from a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to subtract from.</param>
        /// <returns>A new matrix containing the subtraction of the scalar and this matrix.</returns>
        public Matrix<T> SubtractFrom(T scalar)
        {
            var result = Build.SameAs(this);
            DoSubtractFrom(scalar, result);
            return result;
        }

        /// <summary>
        /// Subtracts each element of the matrix from a scalar and stores the result in the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to subtract from.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void SubtractFrom(T scalar, Matrix<T> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, result, "result");
            }

            DoSubtractFrom(scalar, result);
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public Matrix<T> Subtract(Matrix<T> other)
        {
            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, other);
            }

            var result = Build.SameAs(this, other, RowCount, ColumnCount);
            DoSubtract(other, result);
            return result;
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public void Subtract(Matrix<T> other, Matrix<T> result)
        {
            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, other, "other");
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, result, "result");
            }

            DoSubtract(other, result);
        }

        /// <summary>
        /// Multiplies each element of this matrix with a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        public Matrix<T> Multiply(T scalar)
        {
            if (scalar.Equals(One))
            {
                return Clone();
            }

            if (scalar.Equals(Zero))
            {
                return Build.SameAs(this);
            }

            var result = Build.SameAs(this);
            DoMultiply(scalar, result);
            return result;
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public void Multiply(T scalar, Matrix<T> result)
        {
            if (result.RowCount != RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.", nameof(result));
            }

            if (result.ColumnCount != ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.", nameof(result));
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
        public Matrix<T> Divide(T scalar)
        {
            if (scalar.Equals(One))
            {
                return Clone();
            }

            if (scalar.Equals(Zero))
            {
                throw new DivideByZeroException();
            }

            var result = Build.SameAs(this);
            DoDivide(scalar, result);
            return result;
        }

        /// <summary>
        /// Divides each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to divide the matrix with.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public void Divide(T scalar, Matrix<T> result)
        {
            if (result.RowCount != RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.", nameof(result));
            }

            if (result.ColumnCount != ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.", nameof(result));
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
        /// Divides a scalar by each element of the matrix.
        /// </summary>
        /// <param name="scalar">The scalar to divide.</param>
        /// <returns>The result of the division.</returns>
        public Matrix<T> DivideByThis(T scalar)
        {
            var result = Build.SameAs(this);
            DoDivideByThis(scalar, result);
            return result;
        }

        /// <summary>
        /// Divides a scalar by each element of the matrix and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to divide.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public void DivideByThis(T scalar, Matrix<T> result)
        {
            if (result.RowCount != RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.", nameof(result));
            }

            if (result.ColumnCount != ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.", nameof(result));
            }

            DoDivideByThis(scalar, result);
        }

        /// <summary>
        /// Multiplies this matrix by a vector and returns the result.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentException">If <c>this.ColumnCount != rightSide.Count</c>.</exception>
        public Vector<T> Multiply(Vector<T> rightSide)
        {
            if (ColumnCount != rightSide.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, rightSide, "rightSide");
            }

            var ret = Vector<T>.Build.SameAs(this, rightSide, RowCount);
            DoMultiply(rightSide, ret);
            return ret;
        }

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentException">If <strong>result.Count != this.RowCount</strong>.</exception>
        /// <exception cref="ArgumentException">If <strong>this.ColumnCount != <paramref name="rightSide"/>.Count</strong>.</exception>
        public void Multiply(Vector<T> rightSide, Vector<T> result)
        {
            if (ColumnCount != rightSide.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, rightSide, "rightSide");
            }

            if (RowCount != result.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(rightSide, result))
            {
                var tmp = Vector<T>.Build.SameAs(result);
                DoMultiply(rightSide, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoMultiply(rightSide, result);
            }
        }

        /// <summary>
        /// Left multiply a matrix with a vector ( = vector * matrix ).
        /// </summary>
        /// <param name="leftSide">The vector to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentException">If <strong>this.RowCount != <paramref name="leftSide"/>.Count</strong>.</exception>
        public Vector<T> LeftMultiply(Vector<T> leftSide)
        {
            if (RowCount != leftSide.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, leftSide, "leftSide");
            }

            var ret = Vector<T>.Build.SameAs(this, leftSide, ColumnCount);
            DoLeftMultiply(leftSide, ret);
            return ret;
        }

        /// <summary>
        /// Left multiply a matrix with a vector ( = vector * matrix ) and place the result in the result vector.
        /// </summary>
        /// <param name="leftSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentException">If <strong>result.Count != this.ColumnCount</strong>.</exception>
        /// <exception cref="ArgumentException">If <strong>this.RowCount != <paramref name="leftSide"/>.Count</strong>.</exception>
        public void LeftMultiply(Vector<T> leftSide, Vector<T> result)
        {
            if (RowCount != leftSide.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, leftSide, "leftSide");
            }

            if (ColumnCount != result.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(leftSide, result))
            {
                var tmp = Vector<T>.Build.SameAs(result);
                DoLeftMultiply(leftSide, tmp);
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
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.Rows x other.Columns.</exception>
        public void Multiply(Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != other.RowCount || result.RowCount != RowCount || result.ColumnCount != other.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = Build.SameAs(result);
                DoMultiply(other, tmp);
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
        /// <returns>The result of the multiplication.</returns>
        public Matrix<T> Multiply(Matrix<T> other)
        {
            if (ColumnCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other);
            }

            var result = Build.SameAs(this, other, RowCount, other.ColumnCount);
            DoMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.ColumnCount</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.RowCount x other.RowCount.</exception>
        public void TransposeAndMultiply(Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != other.ColumnCount || result.RowCount != RowCount || result.ColumnCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = Build.SameAs(result);
                DoTransposeAndMultiply(other, tmp);
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
        /// <returns>The result of the multiplication.</returns>
        public Matrix<T> TransposeAndMultiply(Matrix<T> other)
        {
            if (ColumnCount != other.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other);
            }

            var result = Build.SameAs(this, other, RowCount, other.RowCount);
            DoTransposeAndMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies the transpose of this matrix by a vector and returns the result.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentException">If <c>this.RowCount != rightSide.Count</c>.</exception>
        public Vector<T> TransposeThisAndMultiply(Vector<T> rightSide)
        {
            if (RowCount != rightSide.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, rightSide, "rightSide");
            }

            var result = Vector<T>.Build.SameAs(this, rightSide, ColumnCount);
            DoTransposeThisAndMultiply(rightSide, result);
            return result;
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentException">If <strong>result.Count != this.ColumnCount</strong>.</exception>
        /// <exception cref="ArgumentException">If <strong>this.RowCount != <paramref name="rightSide"/>.Count</strong>.</exception>
        public void TransposeThisAndMultiply(Vector<T> rightSide, Vector<T> result)
        {
            if (RowCount != rightSide.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, rightSide, "rightSide");
            }

            if (ColumnCount != result.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(rightSide, result))
            {
                var tmp = Vector<T>.Build.SameAs(result);
                DoTransposeThisAndMultiply(rightSide, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoTransposeThisAndMultiply(rightSide, result);
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentException">If <strong>this.Rows != other.RowCount</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.ColumnCount x other.ColumnCount.</exception>
        public void TransposeThisAndMultiply(Matrix<T> other, Matrix<T> result)
        {
            if (RowCount != other.RowCount || result.RowCount != ColumnCount || result.ColumnCount != other.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = Build.SameAs(result);
                DoTransposeThisAndMultiply(other, tmp);
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
        /// <returns>The result of the multiplication.</returns>
        public Matrix<T> TransposeThisAndMultiply(Matrix<T> other)
        {
            if (RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other);
            }

            var result = Build.SameAs(this, other, ColumnCount, other.ColumnCount);
            DoTransposeThisAndMultiply(other, result);
            return result;
        }



        /// <summary>
        /// Multiplies this matrix with the conjugate transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.ColumnCount</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.RowCount x other.RowCount.</exception>
        public void ConjugateTransposeAndMultiply(Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != other.ColumnCount || result.RowCount != RowCount || result.ColumnCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = Build.SameAs(result);
                DoConjugateTransposeAndMultiply(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoConjugateTransposeAndMultiply(other, result);
            }
        }

        /// <summary>
        /// Multiplies this matrix with the conjugate transpose of another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.ColumnCount</strong>.</exception>
        /// <returns>The result of the multiplication.</returns>
        public Matrix<T> ConjugateTransposeAndMultiply(Matrix<T> other)
        {
            if (ColumnCount != other.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other);
            }

            var result = Build.SameAs(this, other, RowCount, other.RowCount);
            DoConjugateTransposeAndMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies the conjugate transpose of this matrix by a vector and returns the result.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentException">If <c>this.RowCount != rightSide.Count</c>.</exception>
        public Vector<T> ConjugateTransposeThisAndMultiply(Vector<T> rightSide)
        {
            if (RowCount != rightSide.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, rightSide, "rightSide");
            }

            var result = Vector<T>.Build.SameAs(this, rightSide, ColumnCount);
            DoConjugateTransposeThisAndMultiply(rightSide, result);
            return result;
        }

        /// <summary>
        /// Multiplies the conjugate transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentException">If <strong>result.Count != this.ColumnCount</strong>.</exception>
        /// <exception cref="ArgumentException">If <strong>this.RowCount != <paramref name="rightSide"/>.Count</strong>.</exception>
        public void ConjugateTransposeThisAndMultiply(Vector<T> rightSide, Vector<T> result)
        {
            if (RowCount != rightSide.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, rightSide, "rightSide");
            }

            if (ColumnCount != result.Count)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(rightSide, result))
            {
                var tmp = Vector<T>.Build.SameAs(result);
                DoConjugateTransposeThisAndMultiply(rightSide, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoConjugateTransposeThisAndMultiply(rightSide, result);
            }
        }

        /// <summary>
        /// Multiplies the conjugate transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentException">If <strong>this.Rows != other.RowCount</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.ColumnCount x other.ColumnCount.</exception>
        public void ConjugateTransposeThisAndMultiply(Matrix<T> other, Matrix<T> result)
        {
            if (RowCount != other.RowCount || result.RowCount != ColumnCount || result.ColumnCount != other.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = Build.SameAs(result);
                DoConjugateTransposeThisAndMultiply(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                DoConjugateTransposeThisAndMultiply(other, result);
            }
        }

        /// <summary>
        /// Multiplies the conjugate transpose of this matrix with another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Rows != other.RowCount</strong>.</exception>
        /// <returns>The result of the multiplication.</returns>
        public Matrix<T> ConjugateTransposeThisAndMultiply(Matrix<T> other)
        {
            if (RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other);
            }

            var result = Build.SameAs(this, other, ColumnCount, other.ColumnCount);
            DoConjugateTransposeThisAndMultiply(other, result);
            return result;
        }

        static Matrix<T> IntPower(int exponent, Matrix<T> x, Matrix<T> y, Matrix<T> work)
        {
            // We try to be smart about not allocating more matrices than needed
            // and to minimize the number of multiplications (not optimal on either though)

            // TODO: For large or non-integer exponents we could diagonalize the matrix with
            // a similarity transform (eigenvalue decomposition)

            // return y*x
            if (exponent == 1)
            {
                // return x
                if (y == null)
                {
                    return x;
                }

                if (work == null) work = y.Multiply(x); else y.Multiply(x, work);
                return work;
            }

            // return y*x^2
            if (exponent == 2)
            {
                if (work == null) work = x.Multiply(x); else x.Multiply(x, work);

                // return x^2
                if (y == null)
                {
                    return work;
                }

                y.Multiply(work, x);
                return x;
            }

            // recursive n <-- n/2, y <-- y, x <-- x^2
            if (exponent.IsEven())
            {
                // we store the new x in work, keep the y as is and reuse the old x as new work matrix.
                if (work == null) work = x.Multiply(x); else x.Multiply(x, work);
                return IntPower(exponent/2, work, y, x);
            }

            // recursive n <-- (n-1)/2, y <-- x, x <-- x^2
            if (y == null)
            {
                // we store the new x in work, directly use the old x as y. no work matrix.
                if (work == null) work = x.Multiply(x); else x.Multiply(x, work);
                return IntPower((exponent - 1)/2, work, x, null);
            }

            // recursive n <-- (n-1)/2, y <-- y*x, x <-- x^2
            // we store the new y in work, the new x in y, and reuse the old x as work
            if (work == null) work = y.Multiply(x); else y.Multiply(x, work);
            x.Multiply(x, y);
            return IntPower((exponent - 1)/2, y, work, x);
        }

        /// <summary>
        /// Raises this square matrix to a positive integer exponent and places the results into the result matrix.
        /// </summary>
        /// <param name="exponent">The positive integer exponent to raise the matrix to.</param>
        /// <param name="result">The result of the power.</param>
        public void Power(int exponent, Matrix<T> result)
        {
            if (RowCount != ColumnCount || result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }
            if (exponent < 0)
            {
                throw new ArgumentException("Value must not be negative (zero is ok).");
            }
            if (exponent == 0)
            {
                Build.DiagonalIdentity(RowCount, ColumnCount).CopyTo(result);
                return;
            }
            if (exponent == 1)
            {
                CopyTo(result);
                return;
            }
            if (exponent == 2)
            {
                Multiply(this, result);
                return;
            }

            var res = IntPower(exponent, Clone(), null, result);
            if (!ReferenceEquals(res, result))
            {
                res.CopyTo(result);
            }
        }

        /// <summary>
        /// Multiplies this square matrix with another matrix and returns the result.
        /// </summary>
        /// <param name="exponent">The positive integer exponent to raise the matrix to.</param>
        public Matrix<T> Power(int exponent)
        {
            if (RowCount != ColumnCount) throw new ArgumentException("Matrix must be square.");
            if (exponent < 0) throw new ArgumentException("Value must not be negative (zero is ok).");

            if (exponent == 0) return Build.DiagonalIdentity(RowCount, ColumnCount);
            if (exponent == 1) return this;
            if (exponent == 2) return Multiply(this);

            return IntPower(exponent, Clone(), null, null);
        }

        /// <summary>
        /// Negate each element of this matrix.
        /// </summary>
        /// <returns>A matrix containing the negated values.</returns>
        public Matrix<T> Negate()
        {
            var result = Build.SameAs(this);
            DoNegate(result);
            return result;
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        /// <exception cref="ArgumentException">if the result matrix's dimensions are not the same as this matrix.</exception>
        public void Negate(Matrix<T> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoNegate(result);
        }

        /// <summary>
        /// Complex conjugate each element of this matrix.
        /// </summary>
        /// <returns>A matrix containing the conjugated values.</returns>
        public Matrix<T> Conjugate()
        {
            var result = Build.SameAs(this);
            DoConjugate(result);
            return result;
        }

        /// <summary>
        /// Complex conjugate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the conjugation.</param>
        /// <exception cref="ArgumentException">if the result matrix's dimensions are not the same as this matrix.</exception>
        public void Conjugate(Matrix<T> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoConjugate(result);
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <returns>A matrix containing the results.</returns>
        public Matrix<T> Modulus(T divisor)
        {
            var result = Build.SameAs(this);
            DoModulus(divisor, result);
            return result;
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        public void Modulus(T divisor, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoModulus(divisor, result);
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <returns>A matrix containing the results.</returns>
        public Matrix<T> ModulusByThis(T dividend)
        {
            var result = Build.SameAs(this);
            DoModulusByThis(dividend, result);
            return result;
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        public void ModulusByThis(T dividend, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoModulusByThis(dividend, result);
        }

        /// <summary>
        /// Computes the remainder (matrix % divisor), where the result has the sign of the dividend,
        /// for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <returns>A matrix containing the results.</returns>
        public Matrix<T> Remainder(T divisor)
        {
            var result = Build.SameAs(this);
            DoRemainder(divisor, result);
            return result;
        }

        /// <summary>
        /// Computes the remainder (matrix % divisor), where the result has the sign of the dividend,
        /// for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        public void Remainder(T divisor, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoRemainder(divisor, result);
        }

        /// <summary>
        /// Computes the remainder (dividend % matrix), where the result has the sign of the dividend,
        /// for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <returns>A matrix containing the results.</returns>
        public Matrix<T> RemainderByThis(T dividend)
        {
            var result = Build.SameAs(this);
            DoRemainderByThis(dividend, result);
            return result;
        }

        /// <summary>
        /// Computes the remainder (dividend % matrix), where the result has the sign of the dividend,
        /// for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        public void RemainderByThis(T dividend, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoRemainderByThis(dividend, result);
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new matrix that is the pointwise multiplication of this matrix and <paramref name="other"/>.</returns>
        public Matrix<T> PointwiseMultiply(Matrix<T> other)
        {
            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, "other");
            }

            var result = Build.SameAs(this, other);
            DoPointwiseMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseMultiply(Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            DoPointwiseMultiply(other, result);
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="divisor"/> are not the same size.</exception>
        /// <returns>A new matrix that is the pointwise division of this matrix and <paramref name="divisor"/>.</returns>
        public Matrix<T> PointwiseDivide(Matrix<T> divisor)
        {
            if (ColumnCount != divisor.ColumnCount || RowCount != divisor.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, divisor);
            }

            var result = Build.SameAs(this, divisor);
            DoPointwiseDivide(divisor, result);
            return result;
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="divisor"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseDivide(Matrix<T> divisor, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != divisor.ColumnCount || RowCount != divisor.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, divisor, result);
            }

            DoPointwiseDivide(divisor, result);
        }

        /// <summary>
        /// Pointwise raise this matrix to an exponent and store the result into the result matrix.
        /// </summary>
        /// <param name="exponent">The exponent to raise this matrix values to.</param>
        public Matrix<T> PointwisePower(T exponent)
        {
            var result = Build.SameAs(this);
            DoPointwisePower(exponent, result);
            return result;
        }

        /// <summary>
        /// Pointwise raise this matrix to an exponent.
        /// </summary>
        /// <param name="exponent">The exponent to raise this matrix values to.</param>
        /// <param name="result">The matrix to store the result into.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwisePower(T exponent, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoPointwisePower(exponent, result);
        }

        /// <summary>
        /// Pointwise raise this matrix to an exponent and store the result into the result matrix.
        /// </summary>
        /// <param name="exponent">The exponent to raise this matrix values to.</param>
        public Matrix<T> PointwisePower(Matrix<T> exponent)
        {
            if (ColumnCount != exponent.ColumnCount || RowCount != exponent.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, exponent);
            }

            var result = Build.SameAs(this);
            DoPointwisePower(exponent, result);
            return result;
        }

        /// <summary>
        /// Pointwise raise this matrix to an exponent.
        /// </summary>
        /// <param name="exponent">The exponent to raise this matrix values to.</param>
        /// <param name="result">The matrix to store the result into.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwisePower(Matrix<T> exponent, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != exponent.ColumnCount || RowCount != exponent.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, exponent, result);
            }

            DoPointwisePower(exponent, result);
        }

        /// <summary>
        /// Pointwise canonical modulus, where the result has the sign of the divisor,
        /// of this matrix by another matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="divisor"/> are not the same size.</exception>
        public Matrix<T> PointwiseModulus(Matrix<T> divisor)
        {
            if (ColumnCount != divisor.ColumnCount || RowCount != divisor.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, divisor);
            }

            var result = Build.SameAs(this, divisor);
            DoPointwiseModulus(divisor, result);
            return result;
        }

        /// <summary>
        /// Pointwise canonical modulus, where the result has the sign of the divisor,
        /// of this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use.</param>
        /// <param name="result">The matrix to store the result of the pointwise modulus.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="divisor"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseModulus(Matrix<T> divisor, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != divisor.ColumnCount || RowCount != divisor.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, divisor, result);
            }

            DoPointwiseModulus(divisor, result);
        }

        /// <summary>
        /// Pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of this matrix by another matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="divisor"/> are not the same size.</exception>
        public Matrix<T> PointwiseRemainder(Matrix<T> divisor)
        {
            if (ColumnCount != divisor.ColumnCount || RowCount != divisor.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, divisor);
            }

            var result = Build.SameAs(this, divisor);
            DoPointwiseRemainder(divisor, result);
            return result;
        }

        /// <summary>
        /// Pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use.</param>
        /// <param name="result">The matrix to store the result of the pointwise remainder.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="divisor"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseRemainder(Matrix<T> divisor, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != divisor.ColumnCount || RowCount != divisor.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, divisor, result);
            }

            DoPointwiseRemainder(divisor, result);
        }

        /// <summary>
        /// Helper function to apply a unary function to a matrix. The function
        /// f modifies the matrix given to it in place.  Before its
        /// called, a copy of the 'this' matrix is first created, then passed to
        /// f.  The copy is then returned as the result
        /// </summary>
        /// <param name="f">Function which takes a matrix, modifies it in place and returns void</param>
        /// <returns>New instance of matrix which is the result</returns>
        protected Matrix<T> PointwiseUnary(Action<Matrix<T>> f)
        {
            var result = Build.SameAs(this);
            f(result);
            return result;
        }

        /// <summary>
        /// Helper function to apply a unary function which modifies a matrix
        /// in place.
        /// </summary>
        /// <param name="f">Function which takes a matrix, modifies it in place and returns void</param>
        /// <param name="result">The matrix to be passed to f and where the result is to be stored</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        protected void PointwiseUnary(Action<Matrix<T>> f, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }
            f(result);
        }

        /// <summary>
        /// Helper function to apply a binary function which takes two matrices
        /// and modifies the latter in place.  A copy of the "this" matrix is
        /// first made and then passed to f together with the other matrix. The
        /// copy is then returned as the result
        /// </summary>
        /// <param name="f">Function which takes two matrices, modifies the second in place and returns void</param>
        /// <param name="other">The other matrix to be passed to the function as argument. It is not modified</param>
        /// <returns>The resulting matrix</returns>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same dimension.</exception>
        protected Matrix<T> PointwiseBinary(Action<Matrix<T>, Matrix<T>> f, Matrix<T> other)
        {
            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other);
            }

            var result = Build.SameAs(this, other);
            f(other, result);
            return result;
        }

        /// <summary>
        /// Helper function to apply a binary function which takes two matrices
        /// and modifies the second one in place
        /// </summary>
        /// <param name="f">Function which takes two matrices, modifies the second in place and returns void</param>
        /// <param name="other">The other matrix to be passed to the function as argument. It is not modified</param>
        /// <param name="result">The matrix to store the result.</param>
        /// <returns>The resulting matrix</returns>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same dimension.</exception>
        protected void PointwiseBinary(Action<Matrix<T>,Matrix<T>> f, Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            f(other, result);
        }

        /// <summary>
        /// Pointwise applies the exponent function to each value.
        /// </summary>
        public Matrix<T> PointwiseExp()
        {
            return PointwiseUnary(DoPointwiseExp);
        }

        /// <summary>
        /// Pointwise applies the exponent function to each value.
        /// </summary>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseExp(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseExp, result);
        }

        /// <summary>
        /// Pointwise applies the natural logarithm function to each value.
        /// </summary>
        public Matrix<T> PointwiseLog()
        {
            return PointwiseUnary(DoPointwiseLog);
        }

        /// <summary>
        /// Pointwise applies the natural logarithm function to each value.
        /// </summary>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseLog(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseLog, result);
        }

        /// <summary>
        /// Pointwise applies the abs function to each value
        /// </summary>
        public Matrix<T> PointwiseAbs()
        {
            return PointwiseUnary(DoPointwiseAbs);
        }

        /// <summary>
        /// Pointwise applies the abs function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseAbs(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseAbs, result);
        }

        /// <summary>
        /// Pointwise applies the acos function to each value
        /// </summary>
        public Matrix<T> PointwiseAcos()
        {
            return PointwiseUnary(DoPointwiseAcos);
        }

        /// <summary>
        /// Pointwise applies the acos function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseAcos(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseAcos, result);
        }

        /// <summary>
        /// Pointwise applies the asin function to each value
        /// </summary>
        public Matrix<T> PointwiseAsin()
        {
            return PointwiseUnary(DoPointwiseAsin);
        }

        /// <summary>
        /// Pointwise applies the asin function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseAsin(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseAsin, result);
        }

        /// <summary>
        /// Pointwise applies the atan function to each value
        /// </summary>
        public Matrix<T> PointwiseAtan()
        {
            return PointwiseUnary(DoPointwiseAtan);
        }

        /// <summary>
        /// Pointwise applies the atan function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseAtan(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseAtan, result);
        }

        /// <summary>
        /// Pointwise applies the atan2 function to each value of the current
        /// matrix and a given other matrix being the 'x' of atan2 and the
        /// 'this' matrix being the 'y'
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Matrix<T> PointwiseAtan2(Matrix<T> other)
        {
            return PointwiseBinary(DoPointwiseAtan2, other);
        }

        /// <summary>
        /// Pointwise applies the atan2 function to each value of the current
        /// matrix and a given other matrix being the 'x' of atan2 and the
        /// 'this' matrix being the 'y'
        /// </summary>
        /// <param name="other">The other matrix 'y'</param>
        /// <param name="result">The matrix with the result and 'x'</param>
        /// <returns></returns>
        public void PointwiseAtan2(Matrix<T> other, Matrix<T> result)
        {
            PointwiseBinary(DoPointwiseAtan2, other, result);
        }

        /// <summary>
        /// Pointwise applies the ceiling function to each value
        /// </summary>
        public Matrix<T> PointwiseCeiling()
        {
            return PointwiseUnary(DoPointwiseCeiling);
        }

        /// <summary>
        /// Pointwise applies the ceiling function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseCeiling(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseCeiling, result);
        }

        /// <summary>
        /// Pointwise applies the cos function to each value
        /// </summary>
        public Matrix<T> PointwiseCos()
        {
            return PointwiseUnary(DoPointwiseCos);
        }

        /// <summary>
        /// Pointwise applies the cos function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseCos(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseCos, result);
        }

        /// <summary>
        /// Pointwise applies the cosh function to each value
        /// </summary>
        public Matrix<T> PointwiseCosh()
        {
            return PointwiseUnary(DoPointwiseCosh);
        }

        /// <summary>
        /// Pointwise applies the cosh function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseCosh(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseCosh, result);
        }

        /// <summary>
        /// Pointwise applies the floor function to each value
        /// </summary>
        public Matrix<T> PointwiseFloor()
        {
            return PointwiseUnary(DoPointwiseFloor);
        }

        /// <summary>
        /// Pointwise applies the floor function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseFloor(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseFloor, result);
        }

        /// <summary>
        /// Pointwise applies the log10 function to each value
        /// </summary>
        public Matrix<T> PointwiseLog10()
        {
            return PointwiseUnary(DoPointwiseLog10);
        }

        /// <summary>
        /// Pointwise applies the log10 function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseLog10(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseLog10, result);
        }

        /// <summary>
        /// Pointwise applies the round function to each value
        /// </summary>
        public Matrix<T> PointwiseRound()
        {
            return PointwiseUnary(DoPointwiseRound);
        }

        /// <summary>
        /// Pointwise applies the round function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseRound(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseRound, result);
        }

        /// <summary>
        /// Pointwise applies the sign function to each value
        /// </summary>
        public Matrix<T> PointwiseSign()
        {
            return PointwiseUnary(DoPointwiseSign);
        }

        /// <summary>
        /// Pointwise applies the sign function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseSign(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseSign, result);
        }

        /// <summary>
        /// Pointwise applies the sin function to each value
        /// </summary>
        public Matrix<T> PointwiseSin()
        {
            return PointwiseUnary(DoPointwiseSin);
        }

        /// <summary>
        /// Pointwise applies the sin function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseSin(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseSin, result);
        }

        /// <summary>
        /// Pointwise applies the sinh function to each value
        /// </summary>
        public Matrix<T> PointwiseSinh()
        {
            return PointwiseUnary(DoPointwiseSinh);
        }

        /// <summary>
        /// Pointwise applies the sinh function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseSinh(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseSinh, result);
        }

        /// <summary>
        /// Pointwise applies the sqrt function to each value
        /// </summary>
        public Matrix<T> PointwiseSqrt()
        {
            return PointwiseUnary(DoPointwiseSqrt);
        }

        /// <summary>
        /// Pointwise applies the sqrt function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseSqrt(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseSqrt, result);
        }

        /// <summary>
        /// Pointwise applies the tan function to each value
        /// </summary>
        public Matrix<T> PointwiseTan()
        {
            return PointwiseUnary(DoPointwiseTan);
        }

        /// <summary>
        /// Pointwise applies the tan function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseTan(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseTan, result);
        }

        /// <summary>
        /// Pointwise applies the tanh function to each value
        /// </summary>
        public Matrix<T> PointwiseTanh()
        {
            return PointwiseUnary(DoPointwiseTanh);
        }

        /// <summary>
        /// Pointwise applies the tanh function to each value
        /// </summary>
        /// <param name="result">The vector to store the result</param>
        public void PointwiseTanh(Matrix<T> result)
        {
            PointwiseUnary(DoPointwiseTanh, result);
        }

        /// <summary>
        /// Computes the trace of this matrix.
        /// </summary>
        /// <returns>The trace of this matrix</returns>
        /// <exception cref="ArgumentException">If the matrix is not square</exception>
        public abstract T Trace();

        /// <summary>
        /// Calculates the rank of the matrix.
        /// </summary>
        /// <returns>effective numerical rank, obtained from SVD</returns>
        public virtual int Rank()
        {
            return Svd(false).Rank;
        }

        /// <summary>
        /// Calculates the nullity of the matrix.
        /// </summary>
        /// <returns>effective numerical nullity, obtained from SVD</returns>
        public int Nullity()
        {
            return ColumnCount - Rank();
        }

        /// <summary>Calculates the condition number of this matrix.</summary>
        /// <returns>The condition number of the matrix.</returns>
        /// <remarks>The condition number is calculated using singular value decomposition.</remarks>
        public virtual T ConditionNumber()
        {
            return Svd(false).ConditionNumber;
        }

        /// <summary>Computes the determinant of this matrix.</summary>
        /// <returns>The determinant of this matrix.</returns>
        public virtual T Determinant()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            return LU().Determinant;
        }

        /// <summary>
        /// Computes an orthonormal basis for the null space of this matrix,
        /// also known as the kernel of the corresponding matrix transformation.
        /// </summary>
        public virtual Vector<T>[] Kernel()
        {
            var svd = Svd(true);
            return svd.VT.EnumerateRows(svd.Rank, ColumnCount - svd.Rank).ToArray();
        }

        /// <summary>
        /// Computes an orthonormal basis for the column space of this matrix,
        /// also known as the range or image of the corresponding matrix transformation.
        /// </summary>
        public virtual Vector<T>[] Range()
        {
            var svd = Svd(true);
            return svd.U.EnumerateColumns(0, svd.Rank).ToArray();
        }

        /// <summary>Computes the inverse of this matrix.</summary>
        /// <returns>The inverse of this matrix.</returns>
        public virtual Matrix<T> Inverse()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            return LU().Inverse();
        }

        /// <summary>Computes the Moore-Penrose Pseudo-Inverse of this matrix.</summary>
        public abstract Matrix<T> PseudoInverse();

        /// <summary>
        /// Computes the Kronecker product of this matrix with the given matrix. The new matrix is M-by-N
        /// with M = this.Rows * lower.Rows and N = this.Columns * lower.Columns.
        /// </summary>
        /// <param name="other">The other matrix.</param>
        /// <returns>The Kronecker product of the two matrices.</returns>
        public Matrix<T> KroneckerProduct(Matrix<T> other)
        {
            var result = Build.SameAs(this, other, RowCount*other.RowCount, ColumnCount*other.ColumnCount);
            KroneckerProduct(other, result);
            return result;
        }

        /// <summary>
        /// Computes the Kronecker product of this matrix with the given matrix. The new matrix is M-by-N
        /// with M = this.Rows * lower.Rows and N = this.Columns * lower.Columns.
        /// </summary>
        /// <param name="other">The other matrix.</param>
        /// <param name="result">The Kronecker product of the two matrices.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not (this.Rows * lower.rows) x (this.Columns * lower.Columns).</exception>
        public virtual void KroneckerProduct(Matrix<T> other, Matrix<T> result)
        {
            if (result.RowCount != (RowCount*other.RowCount) || result.ColumnCount != (ColumnCount*other.ColumnCount))
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, other, result);
            }

            for (var j = 0; j < ColumnCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    result.SetSubMatrix(i*other.RowCount, other.RowCount, j*other.ColumnCount, other.ColumnCount, At(i, j)*other);
                }
            }
        }

        /// <summary>
        /// Pointwise applies the minimum with a scalar to each value.
        /// </summary>
        /// <param name="scalar">The scalar value to compare to.</param>
        public Matrix<T> PointwiseMinimum(T scalar)
        {
            var result = Build.SameAs(this);
            DoPointwiseMinimum(scalar, result);
            return result;
        }

        /// <summary>
        /// Pointwise applies the minimum with a scalar to each value.
        /// </summary>
        /// <param name="scalar">The scalar value to compare to.</param>
        /// <param name="result">The vector to store the result.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseMinimum(T scalar, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoPointwiseMinimum(scalar, result);
        }

        /// <summary>
        /// Pointwise applies the maximum with a scalar to each value.
        /// </summary>
        /// <param name="scalar">The scalar value to compare to.</param>
        public Matrix<T> PointwiseMaximum(T scalar)
        {
            var result = Build.SameAs(this);
            DoPointwiseMaximum(scalar, result);
            return result;
        }

        /// <summary>
        /// Pointwise applies the maximum with a scalar to each value.
        /// </summary>
        /// <param name="scalar">The scalar value to compare to.</param>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseMaximum(T scalar, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoPointwiseMaximum(scalar, result);
        }

        /// <summary>
        /// Pointwise applies the absolute minimum with a scalar to each value.
        /// </summary>
        /// <param name="scalar">The scalar value to compare to.</param>
        public Matrix<T> PointwiseAbsoluteMinimum(T scalar)
        {
            var result = Build.SameAs(this);
            DoPointwiseAbsoluteMinimum(scalar, result);
            return result;
        }

        /// <summary>
        /// Pointwise applies the absolute minimum with a scalar to each value.
        /// </summary>
        /// <param name="scalar">The scalar value to compare to.</param>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseAbsoluteMinimum(T scalar, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoPointwiseAbsoluteMinimum(scalar, result);
        }

        /// <summary>
        /// Pointwise applies the absolute maximum with a scalar to each value.
        /// </summary>
        /// <param name="scalar">The scalar value to compare to.</param>
        public Matrix<T> PointwiseAbsoluteMaximum(T scalar)
        {
            var result = Build.SameAs(this);
            DoPointwiseAbsoluteMaximum(scalar, result);
            return result;
        }

        /// <summary>
        /// Pointwise applies the absolute maximum with a scalar to each value.
        /// </summary>
        /// <param name="scalar">The scalar value to compare to.</param>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseAbsoluteMaximum(T scalar, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result);
            }

            DoPointwiseAbsoluteMaximum(scalar, result);
        }

        /// <summary>
        /// Pointwise applies the minimum with the values of another matrix to each value.
        /// </summary>
        /// <param name="other">The matrix with the values to compare to.</param>
        public Matrix<T> PointwiseMinimum(Matrix<T> other)
        {
            var result = Build.SameAs(this);
            DoPointwiseMinimum(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise applies the minimum with the values of another matrix to each value.
        /// </summary>
        /// <param name="other">The matrix with the values to compare to.</param>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseMinimum(Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            DoPointwiseMinimum(other, result);
        }

        /// <summary>
        /// Pointwise applies the maximum with the values of another matrix to each value.
        /// </summary>
        /// <param name="other">The matrix with the values to compare to.</param>
        public Matrix<T> PointwiseMaximum(Matrix<T> other)
        {
            var result = Build.SameAs(this);
            DoPointwiseMaximum(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise applies the maximum with the values of another matrix to each value.
        /// </summary>
        /// <param name="other">The matrix with the values to compare to.</param>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseMaximum(Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            DoPointwiseMaximum(other, result);
        }

        /// <summary>
        /// Pointwise applies the absolute minimum with the values of another matrix to each value.
        /// </summary>
        /// <param name="other">The matrix with the values to compare to.</param>
        public Matrix<T> PointwiseAbsoluteMinimum(Matrix<T> other)
        {
            var result = Build.SameAs(this);
            DoPointwiseAbsoluteMinimum(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise applies the absolute minimum with the values of another matrix to each value.
        /// </summary>
        /// <param name="other">The matrix with the values to compare to.</param>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseAbsoluteMinimum(Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            DoPointwiseAbsoluteMinimum(other, result);
        }

        /// <summary>
        /// Pointwise applies the absolute maximum with the values of another matrix to each value.
        /// </summary>
        /// <param name="other">The matrix with the values to compare to.</param>
        public Matrix<T> PointwiseAbsoluteMaximum(Matrix<T> other)
        {
            var result = Build.SameAs(this);
            DoPointwiseAbsoluteMaximum(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise applies the absolute maximum with the values of another matrix to each value.
        /// </summary>
        /// <param name="other">The matrix with the values to compare to.</param>
        /// <param name="result">The matrix to store the result.</param>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseAbsoluteMaximum(Matrix<T> other, Matrix<T> result)
        {
            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount || ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, result);
            }

            DoPointwiseAbsoluteMaximum(other, result);
        }

        /// <summary>Calculates the induced L1 norm of this matrix.</summary>
        /// <returns>The maximum absolute column sum of the matrix.</returns>
        public abstract double L1Norm();

        /// <summary>Calculates the induced L2 norm of the matrix.</summary>
        /// <returns>The largest singular value of the matrix.</returns>
        /// <remarks>
        /// For sparse matrices, the L2 norm is computed using a dense implementation of singular value decomposition.
        /// In a later release, it will be replaced with a sparse implementation.
        /// </remarks>
        public virtual double L2Norm()
        {
            return Svd(false).L2Norm;
        }

        /// <summary>Calculates the induced infinity norm of this matrix.</summary>
        /// <returns>The maximum absolute row sum of the matrix.</returns>
        public abstract double InfinityNorm();

        /// <summary>Calculates the entry-wise Frobenius norm of this matrix.</summary>
        /// <returns>The square root of the sum of the squared values.</returns>
        public abstract double FrobeniusNorm();

        /// <summary>
        /// Calculates the p-norms of all row vectors.
        /// Typical values for p are 1.0 (L1, Manhattan norm), 2.0 (L2, Euclidean norm) and positive infinity (infinity norm)
        /// </summary>
        public abstract Vector<double> RowNorms(double norm);

        /// <summary>
        /// Calculates the p-norms of all column vectors.
        /// Typical values for p are 1.0 (L1, Manhattan norm), 2.0 (L2, Euclidean norm) and positive infinity (infinity norm)
        /// </summary>
        public abstract Vector<double> ColumnNorms(double norm);

        /// <summary>
        /// Normalizes all row vectors to a unit p-norm.
        /// Typical values for p are 1.0 (L1, Manhattan norm), 2.0 (L2, Euclidean norm) and positive infinity (infinity norm)
        /// </summary>
        public abstract Matrix<T> NormalizeRows(double norm);

        /// <summary>
        /// Normalizes all column vectors to a unit p-norm.
        /// Typical values for p are 1.0 (L1, Manhattan norm), 2.0 (L2, Euclidean norm) and positive infinity (infinity norm)
        /// </summary>
        public abstract Matrix<T> NormalizeColumns(double norm);

        /// <summary>
        /// Calculates the value sum of each row vector.
        /// </summary>
        public abstract Vector<T> RowSums();

        /// <summary>
        /// Calculates the value sum of each column vector.
        /// </summary>
        public abstract Vector<T> ColumnSums();

        /// <summary>
        /// Calculates the absolute value sum of each row vector.
        /// </summary>
        public abstract Vector<T> RowAbsoluteSums();

        /// <summary>
        /// Calculates the absolute value sum of each column vector.
        /// </summary>
        public abstract Vector<T> ColumnAbsoluteSums();

        #region Exceptions - possibly move elsewhere?

        internal static Exception DimensionsDontMatch<TException>(Matrix<T> left, Matrix<T> right, Matrix<T> result, string paramName = null)
            where TException : Exception
        {
            var message = $"Matrix dimensions must agree: op1 is {left.RowCount}x{left.ColumnCount}, op2 is {right.RowCount}x{right.ColumnCount}, op3 is {result.RowCount}x{result.ColumnCount}.";
            return CreateException<TException>(message, paramName);
        }

        internal static Exception DimensionsDontMatch<TException>(Matrix<T> left, Matrix<T> right, string paramName = null)
            where TException : Exception
        {
            var message = $"Matrix dimensions must agree: op1 is {left.RowCount}x{left.ColumnCount}, op2 is {right.RowCount}x{right.ColumnCount}.";
            return CreateException<TException>(message, paramName);
        }

        internal static Exception DimensionsDontMatch<TException>(Matrix<T> matrix)
            where TException : Exception
        {
            var message = $"Matrix dimensions must agree: {matrix.RowCount}x{matrix.ColumnCount}.";
            return CreateException<TException>(message);
        }

        internal static Exception DimensionsDontMatch<TException>(Matrix<T> left, Vector<T> right, Vector<T> result, string paramName = null)
            where TException : Exception
        {
            return DimensionsDontMatch<TException>(left, right.ToColumnMatrix(), result.ToColumnMatrix(), paramName);
        }

        internal static Exception DimensionsDontMatch<TException>(Matrix<T> left, Vector<T> right, string paramName = null)
            where TException : Exception
        {
            return DimensionsDontMatch<TException>(left, right.ToColumnMatrix(), paramName);
        }

        internal static Exception DimensionsDontMatch<TException>(Vector<T> left, Matrix<T> right, string paramName = null)
            where TException : Exception
        {
            return DimensionsDontMatch<TException>(left.ToColumnMatrix(), right, paramName);
        }

        internal static Exception DimensionsDontMatch<TException>(Vector<T> left, Vector<T> right, string paramName = null)
            where TException : Exception
        {
            return DimensionsDontMatch<TException>(left.ToColumnMatrix(), right.ToColumnMatrix(), paramName);
        }

        static Exception CreateException<TException>(string message, string paramName = null)
            where TException : Exception
        {
            if (typeof (TException) == typeof (ArgumentException))
            {
                return new ArgumentException(message, paramName);
            }
            if (typeof (TException) == typeof (ArgumentOutOfRangeException))
            {
                return new ArgumentOutOfRangeException(paramName, message);
            }
            return new Exception(message);
        }

        #endregion
    }
}
