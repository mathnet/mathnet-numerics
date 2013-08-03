// <copyright file="Vector.Arithmetic.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Generic
{
    public abstract partial class Vector<T>
    {
        /// <summary>
        /// Negates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected abstract void DoNegate(Vector<T> result);

        /// <summary>
        /// Complex conjugates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected abstract void DoConjugate(Vector<T> result);

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        protected abstract void DoAdd(T scalar, Vector<T> result);

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        protected abstract void DoAdd(Vector<T> other, Vector<T> result);

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected abstract void DoSubtract(T scalar, Vector<T> result);

        /// <summary>
        /// Subtracts each element of the vector from a scalar and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract from.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected virtual void DoSubtractFrom(T scalar, Vector<T> result)
        {
            DoNegate(result);
            result.DoAdd(scalar, result);
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected abstract void DoSubtract(Vector<T> other, Vector<T> result);

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="result">The vector to store the result of the multiplication.</param>
        protected abstract void DoMultiply(T scalar, Vector<T> result);

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        protected abstract T DoDotProduct(Vector<T> other);

        /// <summary>
        /// Divides each element of the vector by a scalar and stores the result in the result vector.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">The vector to store the result of the division.</param>
        protected abstract void DoDivide(T divisor, Vector<T> result);

        /// <summary>
        /// Divides a scalar by each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">The vector to store the result of the division.</param>
        protected abstract void DoDivideByThis(T dividend, Vector<T> result);

        /// <summary>
        /// Computes the modulus for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected abstract void DoModulus(T divisor, Vector<T> result);

        /// <summary>
        /// Computes the modulus for the given dividend for each element of the vector.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected abstract void DoModulusByThis(T dividend, Vector<T> result);

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected abstract void DoPointwiseMultiply(Vector<T> other, Vector<T> result);

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <param name="result">The result of the division.</param>
        protected abstract void DoPointwiseDivide(Vector<T> divisor, Vector<T> result);

        /// <summary>
        /// Pointwise modulus this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <param name="result">The result of the modulus.</param>
        protected abstract void DoPointwiseModulus(Vector<T> divisor, Vector<T> result);

        /// <summary>
        /// Adds a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <returns>A copy of the vector with the scalar added.</returns>
        public Vector<T> Add(T scalar)
        {
            if (scalar.Equals(Zero))
            {
                return Clone();
            }

            var result = CreateVector(Count);
            DoAdd(scalar, result);
            return result;
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void Add(T scalar, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (scalar.Equals(Zero))
            {
                CopyTo(result);
                return;
            }

            DoAdd(scalar, result);
        }

        /// <summary>
        /// Adds another vector to this vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <returns>A new vector containing the sum of both vectors.</returns>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public Vector<T> Add(Vector<T> other)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = CreateVector(Count);
            DoAdd(other, result);
            return result;
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void Add(Vector<T> other, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoAdd(other, result);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <returns>A new vector containing the subtraction of this vector and the scalar.</returns>
        public Vector<T> Subtract(T scalar)
        {
            if (scalar.Equals(Zero))
            {
                return Clone();
            }

            var result = CreateVector(Count);
            DoSubtract(scalar, result);
            return result;
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void Subtract(T scalar, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (scalar.Equals(Zero))
            {
                CopyTo(result);
                return;
            }

            DoSubtract(scalar, result);
        }

        /// <summary>
        /// Subtracts each element of the vector from a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to subtract from.</param>
        /// <returns>A new vector containing the subtraction of the scalar and this vector.</returns>
        public Vector<T> SubtractFrom(T scalar)
        {
            var result = CreateVector(Count);
            DoSubtractFrom(scalar, result);
            return result;
        }

        /// <summary>
        /// Subtracts each element of the vector from a scalar and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract from.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void SubtractFrom(T scalar, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoSubtractFrom(scalar, result);
        }

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        /// <returns>The negated vector.</returns>
        /// <remarks>Added as an alternative to the unary negation operator.</remarks>
        public Vector<T> Negate()
        {
            var retrunVector = CreateVector(Count);
            DoNegate(retrunVector);
            return retrunVector;
        }

        /// <summary>
        /// Negates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        public void Negate(Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoNegate(result);
        }

        /// <summary>
        /// Subtracts another vector from this vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <returns>A new vector containing the subtraction of the the two vectors.</returns>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public Vector<T> Subtract(Vector<T> other)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = CreateVector(Count);
            DoSubtract(other, result);
            return result;
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void Subtract(Vector<T> other, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoSubtract(other, result);
        }

        /// <summary>
        /// Return vector with complex conjugate values of the source vector
        /// </summary>
        /// <returns>Conjugated vector</returns>
        public Vector<T> Conjugate()
        {
            var retrunVector = CreateVector(Count);
            DoConjugate(retrunVector);
            return retrunVector;
        }

        /// <summary>
        /// Complex conjugates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        public void Conjugate(Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoConjugate(result);
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <returns>A new vector that is the multiplication of the vector and the scalar.</returns>
        public Vector<T> Multiply(T scalar)
        {
            if (scalar.Equals(One))
            {
                return Clone();
            }

            if (scalar.Equals(Zero))
            {
                return CreateVector(Count);
            }

            var result = CreateVector(Count);
            DoMultiply(scalar, result);
            return result;
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="result">The vector to store the result of the multiplication.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void Multiply(T scalar, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
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
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentException">If <paramref name="other"/> is not of the same size.</exception>
        public T DotProduct(Vector<T> other)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            return DoDotProduct(other);
        }

        /// <summary>
        /// Divides each element of the vector by a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to divide with.</param>
        /// <returns>A new vector that is the division of the vector and the scalar.</returns>
        public Vector<T> Divide(T scalar)
        {
            if (scalar.Equals(One))
            {
                return Clone();
            }

            var result = CreateVector(Count);
            DoDivide(scalar, result);
            return result;
        }

        /// <summary>
        /// Divides each element of the vector by a scalar and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to divide with.</param>
        /// <param name="result">The vector to store the result of the division.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void Divide(T scalar, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (scalar.Equals(One))
            {
                CopyTo(result);
                return;
            }

            DoDivide(scalar, result);
        }

        /// <summary>
        /// Divides a scalar by each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to divide.</param>
        /// <returns>A new vector that is the division of the vector and the scalar.</returns>
        public Vector<T> DevideByThis(T scalar)
        {
            var result = CreateVector(Count);
            DoDivideByThis(scalar, result);
            return result;
        }

        /// <summary>
        /// Divides a scalar by each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to divide.</param>
        /// <param name="result">The vector to store the result of the division.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void DivideByThis(T scalar, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoDivideByThis(scalar, result);
        }

        /// <summary>
        /// Computes the modulus (vector % divisor) for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <returns>A vector containing the result.</returns>
        public Vector<T> Modulus(T divisor)
        {
            var result = CreateVector(Count);
            DoModulus(divisor, result);
            return result;
        }

        /// <summary>
        /// Computes the modulus (vector % divisor) for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        public void Modulus(T divisor, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoModulus(divisor, result);
        }

        /// <summary>
        /// Computes the modulus (dividend % vector) for the given dividend for each element of the vector.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <returns>A vector containing the result.</returns>
        public Vector<T> ModulusByThis(T dividend)
        {
            var result = CreateVector(Count);
            DoModulusByThis(dividend, result);
            return result;
        }

        /// <summary>
        /// Computes the modulus (dividend % vector) for the given dividend for each element of the vector.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        public void ModulusByThis(T dividend, Vector<T> result)
        {
            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoModulusByThis(dividend, result);
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <returns>A new vector which is the pointwise multiplication of the two vectors.</returns>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public Vector<T> PointwiseMultiply(Vector<T> other)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = CreateVector(Count);
            DoPointwiseMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseMultiply(Vector<T> other, Vector<T> result)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoPointwiseMultiply(other, result);
        }

        /// <summary>
        /// Pointwise divide this vector with another vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <returns>A new vector which is the pointwise division of the two vectors.</returns>
        /// <exception cref="ArgumentException">If this vector and <paramref name="divisor"/> are not the same size.</exception>
        public Vector<T> PointwiseDivide(Vector<T> divisor)
        {
            if (Count != divisor.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "divisor");
            }

            var result = CreateVector(Count);
            DoPointwiseDivide(divisor, result);
            return result;
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="divisor"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseDivide(Vector<T> divisor, Vector<T> result)
        {
            if (Count != divisor.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "divisor");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoPointwiseDivide(divisor, result);
        }

        /// <summary>
        /// Pointwise modulus this vector with another vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <returns>A new vector which is the pointwise modulus of the two vectors.</returns>
        /// <exception cref="ArgumentException">If this vector and <paramref name="divisor"/> are not the same size.</exception>
        public Vector<T> PointwiseModulus(Vector<T> divisor)
        {
            if (Count != divisor.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "divisor");
            }

            var result = CreateVector(Count);
            DoPointwiseModulus(divisor, result);
            return result;
        }

        /// <summary>
        /// Pointwise modulus this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <param name="result">The vector to store the result of the pointwise modulus.</param>
        /// <exception cref="ArgumentException">If this vector and <paramref name="divisor"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public void PointwiseModulus(Vector<T> divisor, Vector<T> result)
        {
            if (Count != divisor.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "divisor");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoPointwiseModulus(divisor, result);
        }

        /// <summary>
        /// Outer product of two vectors
        /// </summary>
        /// <param name="u">First vector</param>
        /// <param name="v">Second vector</param>
        /// <returns>Matrix M[i,j] = u[i]*v[j] </returns>
        public static Matrix<T> OuterProduct(Vector<T> u, Vector<T> v)
        {
            var matrix = u.CreateMatrix(u.Count, v.Count);

            for (var i = 0; i < u.Count; i++)
            {
                matrix.SetRow(i, v.Multiply(u.At(i)));
            }

            return matrix;
        }

        /// <summary>
        /// Outer product of this and another vector.
        /// </summary>
        /// <param name="v">The vector to operate on.</param>
        /// <returns>
        /// Matrix M[i,j] = this[i] * v[j].
        /// </returns>
        /// <seealso cref="OuterProduct(Vector{T}, Vector{T})"/>
        public Matrix<T> OuterProduct(Vector<T> v)
        {
            return OuterProduct(this, v);
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns><c>Scalar ret = (sum(abs(this[i])^p))^(1/p)</c></returns>
        public abstract T Norm(double p);

        /// <summary>
        /// Normalizes this vector to a unit vector with respect to the p-norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>This vector normalized to a unit vector with respect to the p-norm.</returns>
        public abstract Vector<T> Normalize(double p);

        /// <summary>
        /// Returns the value of the absolute minimum element.
        /// </summary>
        /// <returns>The value of the absolute minimum element.</returns>
        public abstract T AbsoluteMinimum();

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>
        public abstract int AbsoluteMinimumIndex();

        /// <summary>
        /// Returns the value of the absolute maximum element.
        /// </summary>
        /// <returns>The value of the absolute maximum element.</returns>
        public abstract T AbsoluteMaximum();

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>
        public abstract int AbsoluteMaximumIndex();

        /// <summary>
        /// Returns the value of maximum element.
        /// </summary>
        /// <returns>The value of maximum element.</returns>
        public T Maximum()
        {
            return At(MaximumIndex());
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>
        public abstract int MaximumIndex();

        /// <summary>
        /// Returns the value of the minimum element.
        /// </summary>
        /// <returns>The value of the minimum element.</returns>
        public T Minimum()
        {
            return At(MinimumIndex());
        }

        /// <summary>
        /// Returns the index of the minimum element.
        /// </summary>
        /// <returns>The index of minimum element.</returns>
        public abstract int MinimumIndex();

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public abstract T Sum();

        /// <summary>
        /// Computes the sum of the absolute value of the vector's elements.
        /// </summary>
        /// <returns>The sum of the absolute value of the vector's elements.</returns>
        public abstract T SumMagnitudes();
    }
}
