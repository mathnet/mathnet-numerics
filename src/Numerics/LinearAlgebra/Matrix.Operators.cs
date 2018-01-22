// <copyright file="Matrix.Arithmetic.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2013 Math.NET
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
using System.Runtime.CompilerServices;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    public abstract partial class Matrix<T>
    {
        /// <summary>
        /// Returns a <strong>Matrix</strong> containing the same values of <paramref name="rightSide"/>.
        /// </summary>
        /// <param name="rightSide">The matrix to get the values from.</param>
        /// <returns>A matrix containing a the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator +(Matrix<T> rightSide)
        {
            return rightSide.Clone();
        }

        /// <summary>
        /// Negates each element of the matrix.
        /// </summary>
        /// <param name="rightSide">The matrix to negate.</param>
        /// <returns>A matrix containing the negated values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator -(Matrix<T> rightSide)
        {
            return rightSide.Negate();
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
        public static Matrix<T> operator +(Matrix<T> leftSide, Matrix<T> rightSide)
        {
            return leftSide.Add(rightSide);
        }

        /// <summary>
        /// Adds a scalar to each element of the matrix.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of the provided matrix.</remarks>
        /// <param name="leftSide">The left matrix to add.</param>
        /// <param name="rightSide">The scalar value to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator +(Matrix<T> leftSide, T rightSide)
        {
            return leftSide.Add(rightSide);
        }

        /// <summary>
        /// Adds a scalar to each element of the matrix.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of the provided matrix.</remarks>
        /// <param name="leftSide">The scalar value to add.</param>
        /// <param name="rightSide">The right matrix to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator +(T leftSide, Matrix<T> rightSide)
        {
            return rightSide.Add(leftSide);
        }

        /// <summary>
        /// Subtracts two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to subtract.</param>
        /// <param name="rightSide">The right matrix to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator -(Matrix<T> leftSide, Matrix<T> rightSide)
        {
            return leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Subtracts a scalar from each element of a matrix.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of the provided matrix.</remarks>
        /// <param name="leftSide">The left matrix to subtract.</param>
        /// <param name="rightSide">The scalar value to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator -(Matrix<T> leftSide, T rightSide)
        {
            return leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Subtracts each element of a matrix from a scalar.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of the provided matrix.</remarks>
        /// <param name="leftSide">The scalar value to subtract.</param>
        /// <param name="rightSide">The right matrix to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator -(T leftSide, Matrix<T> rightSide)
        {
            return rightSide.SubtractFrom(leftSide);
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
            return rightSide.LeftMultiply(leftSide);
        }

        /// <summary>
        /// Divides a scalar with a matrix.
        /// </summary>
        /// <param name="dividend">The scalar to divide.</param>
        /// <param name="divisor">The matrix.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="divisor"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator /(T dividend, Matrix<T> divisor)
        {
            return divisor.DivideByThis(dividend);
        }

        /// <summary>
        /// Divides a matrix with a scalar.
        /// </summary>
        /// <param name="dividend">The matrix to divide.</param>
        /// <param name="divisor">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="dividend"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator /(Matrix<T> dividend, T divisor)
        {
            return dividend.Divide(divisor);
        }

        /// <summary>
        /// Computes the pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of each element of the matrix of the given divisor.
        /// </summary>
        /// <param name="dividend">The matrix whose elements we want to compute the modulus of.</param>
        /// <param name="divisor">The divisor to use.</param>
        /// <returns>The result of the calculation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="dividend"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator %(Matrix<T> dividend, T divisor)
        {
            return dividend.Remainder(divisor);
        }

        /// <summary>
        /// Computes the pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of the given dividend of each element of the matrix.
        /// </summary>
        /// <param name="dividend">The dividend we want to compute the modulus of.</param>
        /// <param name="divisor">The matrix whose elements we want to use as divisor.</param>
        /// <returns>The result of the calculation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="divisor"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator %(T dividend, Matrix<T> divisor)
        {
            return divisor.RemainderByThis(dividend);
        }

        /// <summary>
        /// Computes the pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of each element of two matrices.
        /// </summary>
        /// <param name="dividend">The matrix whose elements we want to compute the remainder of.</param>
        /// <param name="divisor">The divisor to use.</param>
        /// <exception cref="ArgumentException">If <paramref name="dividend"/> and <paramref name="divisor"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="dividend"/> is <see langword="null" />.</exception>
        public static Matrix<T> operator %(Matrix<T> dividend, Matrix<T> divisor)
        {
            return dividend.PointwiseRemainder(divisor);
        }

        [SpecialName]
        public static Matrix<T> op_DotMultiply(Matrix<T> x, Matrix<T> y)
        {
            return x.PointwiseMultiply(y);
        }

        [SpecialName]
        public static Matrix<T> op_DotDivide(Matrix<T> dividend, Matrix<T> divisor)
        {
            return dividend.PointwiseDivide(divisor);
        }

        [SpecialName]
        public static Matrix<T> op_DotPercent(Matrix<T> dividend, Matrix<T> divisor)
        {
            return dividend.PointwiseRemainder(divisor);
        }

        [SpecialName]
        public static Matrix<T> op_DotHat(Matrix<T> matrix, Matrix<T> exponent)
        {
            return matrix.PointwisePower(exponent);
        }

        [SpecialName]
        public static Matrix<T> op_DotHat(Matrix<T> matrix, T exponent)
        {
            return matrix.PointwisePower(exponent);
        }

        /// <summary>
        /// Computes the sqrt of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Sqrt(Matrix<T> x)
        {
            return x.PointwiseSqrt();
        }

        /// <summary>
        /// Computes the exponential of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Exp(Matrix<T> x)
        {
            return x.PointwiseExp();
        }

        /// <summary>
        /// Computes the log of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Log(Matrix<T> x)
        {
            return x.PointwiseLog();
        }

        /// <summary>
        /// Computes the log10 of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Log10(Matrix<T> x)
        {
            return x.PointwiseLog10();
        }

        /// <summary>
        /// Computes the sin of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Sin(Matrix<T> x)
        {
            return x.PointwiseSin();
        }

        /// <summary>
        /// Computes the cos of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Cos(Matrix<T> x)
        {
            return x.PointwiseCos();
        }

        /// <summary>
        /// Computes the tan of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Tan(Matrix<T> x)
        {
            return x.PointwiseTan();
        }

        /// <summary>
        /// Computes the asin of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Asin(Matrix<T> x)
        {
            return x.PointwiseAsin();
        }

        /// <summary>
        /// Computes the acos of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Acos(Matrix<T> x)
        {
            return x.PointwiseAcos();
        }

        /// <summary>
        /// Computes the atan of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Atan(Matrix<T> x)
        {
            return x.PointwiseAtan();
        }

        /// <summary>
        /// Computes the sinh of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Sinh(Matrix<T> x)
        {
            return x.PointwiseSinh();
        }

        /// <summary>
        /// Computes the cosh of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Cosh(Matrix<T> x)
        {
            return x.PointwiseCosh();
        }

        /// <summary>
        /// Computes the tanh of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Tanh(Matrix<T> x)
        {
            return x.PointwiseTanh();
        }

        /// <summary>
        /// Computes the absolute value of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Abs(Matrix<T> x)
        {
            return x.PointwiseAbs();
        }

        /// <summary>
        /// Computes the floor of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Floor(Matrix<T> x)
        {
            return x.PointwiseFloor();
        }

        /// <summary>
        /// Computes the ceiling of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Ceiling(Matrix<T> x)
        {
            return x.PointwiseCeiling();
        }

        /// <summary>
        /// Computes the rounded value of a matrix pointwise
        /// </summary>
        /// <param name="x">The input matrix</param>
        /// <returns></returns>
        public static Matrix<T> Round(Matrix<T> x)
        {
            return x.PointwiseRound();
        }
    }
}
