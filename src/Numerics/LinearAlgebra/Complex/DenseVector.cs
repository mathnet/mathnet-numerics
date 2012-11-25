// <copyright file="DenseVector.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using Distributions;
    using Generic;
    using NumberTheory;
    using Properties;
    using Storage;
    using Threading;

    /// <summary>
    /// A vector using dense storage.
    /// </summary>
    [Serializable]
    public class DenseVector : Vector
    {
        /// <summary>
        /// Number of elements
        /// </summary>
        readonly int _length;

        /// <summary>
        /// Gets the vector's data.
        /// </summary>
        readonly Complex[] _values;

        internal DenseVector(DenseVectorStorage<Complex> storage)
            : base(storage)
        {
            _length = storage.Length;
            _values = storage.Data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector"/> class with a given size.
        /// </summary>
        /// <param name="size">
        /// the size of the vector.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="size"/> is less than one.
        /// </exception>
        public DenseVector(int size)
            : this(new DenseVectorStorage<Complex>(size))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector"/> class with a given size
        /// and each element set to the given value;
        /// </summary>
        /// <param name="size">
        /// the size of the vector.
        /// </param>
        /// <param name="value">
        /// the value to set each element to.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="size"/> is less than one.
        /// </exception>
        public DenseVector(int size, Complex value)
            : this(size)
        {
            for (var index = 0; index < _values.Length; index++)
            {
                _values[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector"/> class by
        /// copying the values from another.
        /// </summary>
        /// <param name="other">
        /// The vector to create the new vector from.
        /// </param>
        public DenseVector(Vector<Complex> other)
            : this(other.Count)
        {
            other.Storage.CopyTo(Storage, skipClearing: true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector"/> class for an array.
        /// </summary>
        /// <param name="array">The array to create this vector from.</param>
        /// <remarks>The vector does not copy the array, but keeps a reference to it. Any 
        /// changes to the vector will also change the array.</remarks>
        public DenseVector(Complex[] array)
            : this(new DenseVectorStorage<Complex>(array.Length, array))
        {
        }

        /// <summary>
        /// Create a new dense vector with values sampled from the provided random distribution.
        /// </summary>
        public static DenseVector CreateRandom(int size, IContinuousDistribution distribution)
        {
            var storage = new DenseVectorStorage<Complex>(size);
            for (var i = 0; i < storage.Data.Length; i++)
            {
                storage.Data[i] = new Complex(distribution.Sample(), distribution.Sample());
            }
            return new DenseVector(storage);
        }

        /// <summary>
        /// Gets the vector's data.
        /// </summary>
        /// <value>The vector's data.</value>
        public Complex[] Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Returns a reference to the internal data structure.
        /// </summary>
        /// <param name="vector">The <c>DenseVector</c> whose internal data we are
        /// returning.</param>
        /// <returns>
        /// A reference to the internal date of the given vector.
        /// </returns>
        public static explicit operator Complex[](DenseVector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException();
            }

            return vector.Values;
        }

        /// <summary>
        /// Returns a vector bound directly to a reference of the provided array.
        /// </summary>
        /// <param name="array">The array to bind to the <c>DenseVector</c> object.</param>
        /// <returns>
        /// A <c>DenseVector</c> whose values are bound to the given array.
        /// </returns>
        public static implicit operator DenseVector(Complex[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            return new DenseVector(array);
        }

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>This vector as a column matrix.</returns>
        public override Matrix<Complex> ToColumnMatrix()
        {
            var matrix = new DenseMatrix(_length, 1);
            for (var i = 0; i < _values.Length; i++)
            {
                matrix.At(i, 0, _values[i]);
            }

            return matrix;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>This vector as a row matrix.</returns>
        public override Matrix<Complex> ToRowMatrix()
        {
            var matrix = new DenseMatrix(1, _length);
            for (var i = 0; i < _values.Length; i++)
            {
                matrix.At(0, i, _values[i]);
            }

            return matrix;
        }

        /// <summary>
        /// Creates a matrix with the given dimensions using the same storage type
        /// as this vector.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <returns>
        /// A matrix with the given dimensions.
        /// </returns>
        public override Matrix<Complex> CreateMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        /// <summary>
        /// Creates a <strong>Vector</strong> of the given size using the same storage type
        /// as this vector.
        /// </summary>
        /// <param name="size">
        /// The size of the <strong>Vector</strong> to create.
        /// </param>
        /// <returns>
        /// The new <c>Vector</c>.
        /// </returns>
        public override Vector<Complex> CreateVector(int size)
        {
            return new DenseVector(size);
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        protected override void DoAdd(Complex scalar, Vector<Complex> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoAdd(scalar, result);
            }
            else
            {
                CommonParallel.For(
                    0,
                    _values.Length,
                    index => dense._values[index] = _values[index] + scalar);
            }
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        protected override void DoAdd(Vector<Complex> other, Vector<Complex> result)
        {
            var rdense = result as DenseVector;
            var odense = other as DenseVector;
            if (rdense != null && odense != null)
            {
                Control.LinearAlgebraProvider.AddVectorToScaledVector(_values, Complex.One, odense.Values, rdense.Values);
            }
            else
            {
                base.DoAdd(other, result);
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the same values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <remarks>This method is included for completeness.</remarks>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing a the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<Complex> operator +(DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Plus();
        }

        /// <summary>
        /// Adds two <strong>Vectors</strong> together and returns the results.
        /// </summary>
        /// <param name="leftSide">One of the vectors to add.</param>
        /// <param name="rightSide">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<Complex> operator +(DenseVector leftSide, DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            return leftSide.Add(rightSide);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected override void DoSubtract(Complex scalar, Vector<Complex> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoSubtract(scalar, result);
            }
            else
            {
                CommonParallel.For(
                    0,
                    _values.Length,
                    index => dense._values[index] = _values[index] - scalar);
            }
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected override void DoSubtract(Vector<Complex> other, Vector<Complex> result)
        {
            var rdense = result as DenseVector;
            var odense = other as DenseVector;
            if (rdense != null && odense != null)
            {
                Control.LinearAlgebraProvider.AddVectorToScaledVector(_values, -1.0, odense.Values, rdense.Values);
            }
            else
            {
                base.DoSubtract(other, result);
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the negated values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the negated values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<Complex> operator -(DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Negate();
        }

        /// <summary>
        /// Subtracts two <strong>Vectors</strong> and returns the results.
        /// </summary>
        /// <param name="leftSide">The vector to subtract from.</param>
        /// <param name="rightSide">The vector to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<Complex> operator -(DenseVector leftSide, DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            return leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        /// <returns>The negated vector.</returns>
        /// <remarks>Added as an alternative to the unary negation operator.</remarks>
        public override Vector<Complex> Negate()
        {
            var result = new DenseVector(_length);
            CommonParallel.For(
                0, 
                _values.Length,
                index => result[index] = -_values[index]);

            return result;
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="result">The vector to store the result of the multiplication.</param>
        /// <remarks></remarks>
        protected override void DoMultiply(Complex scalar, Vector<Complex> result)
        {
            var denseResult = result as DenseVector;
            if (denseResult == null)
            {
                base.DoMultiply(scalar, result);
            }
            else
            {
                Control.LinearAlgebraProvider.ScaleArray(scalar, _values, denseResult.Values);
            }
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector to add.</param>
        /// <returns>s
        /// The result of the addition.</returns>
        protected override Complex DoDotProduct(Vector<Complex> other)
        {
            var denseVector = other as DenseVector;

            return denseVector == null ? base.DoDotProduct(other) : Control.LinearAlgebraProvider.DotProduct(_values, denseVector.Values);
        }

        /// <summary>
        /// Multiplies a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The vector to scale.</param>
        /// <param name="rightSide">The Complex value.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator *(DenseVector leftSide, Complex rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The Complex value.</param>
        /// <param name="rightSide">The vector to scale.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator *(Complex leftSide, DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseVector)rightSide.Multiply(leftSide);
        }

        /// <summary>
        /// Computes the dot product between two <strong>Vectors</strong>.
        /// </summary>
        /// <param name="leftSide">The left row vector.</param>
        /// <param name="rightSide">The right column vector.</param>
        /// <returns>The dot product between the two vectors.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Complex operator *(DenseVector leftSide, DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            return Control.LinearAlgebraProvider.DotProduct(leftSide.Values, rightSide.Values);
        }

        /// <summary>
        /// Divides a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The vector to divide.</param>
        /// <param name="rightSide">The Complex value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator /(DenseVector leftSide, Complex rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Multiply(Complex.One / rightSide);
        }

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>   
        public override int AbsoluteMinimumIndex()
        {
            var index = 0;
            var min = _values[index].Magnitude;
            for (var i = 1; i < _length; i++)
            {
                var test = _values[i].Magnitude;
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the value of the absolute minimum element.
        /// </summary>
        /// <returns>The value of the absolute minimum element.</returns>
        public override Complex AbsoluteMinimum()
        {
            return _values[AbsoluteMinimumIndex()].Magnitude;
        }

        /// <summary>
        /// Returns the value of the absolute maximum element.
        /// </summary>
        /// <returns>The value of the absolute maximum element.</returns>
        public override Complex AbsoluteMaximum()
        {
            return _values[AbsoluteMaximumIndex()].Magnitude;
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>   
        public override int AbsoluteMaximumIndex()
        {
            var index = 0;
            var max = _values[index].Magnitude;
            for (var i = 1; i < _length; i++)
            {
                var test = _values[i].Magnitude;
                if (test > max)
                {
                    index = i;
                    max = test;
                }
            }

            return index;
        }

        /// <summary>
        /// Creates a vector containing specified elements.
        /// </summary>
        /// <param name="index">The first element to begin copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A vector containing a copy of the specified elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><list><item>If <paramref name="index"/> is not positive or
        /// greater than or equal to the size of the vector.</item>
        /// <item>If <paramref name="index"/> + <paramref name="length"/> is greater than or equal to the size of the vector.</item>
        /// </list></exception>
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        public override Vector<Complex> SubVector(int index, int length)
        {
            if (index < 0 || index >= _length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (index + length > _length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            var result = new DenseVector(length);

            CommonParallel.For(
                index, 
                index + length,
                i => result._values[i - index] = _values[i]);
            return result;
        }

        /// <summary>
        /// Set the values of this vector to the given values.
        /// </summary>
        /// <param name="values">The array containing the values to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="values"/> is not the same size as this vector.</exception>
        public override void SetValues(Complex[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length != _length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "values");
            }

            CommonParallel.For(
                0, 
                values.Length, 
                i => _values[i] = values[i]);
        }

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public override Complex Sum()
        {
            var sum = Complex.Zero;

            for (var i = 0; i < _length; i++)
            {
                sum += _values[i];
            }

            return sum;
        }

        /// <summary>
        /// Computes the sum of the absolute value of the vector's elements.
        /// </summary>
        /// <returns>The sum of the absolute value of the vector's elements.</returns>
        public override Complex SumMagnitudes()
        {
            var sum = Complex.Zero;

            for (var i = 0; i < _length; i++)
            {
                sum += _values[i].Magnitude;
            }

            return sum;
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        protected override void DoPointwiseMultiply(Vector<Complex> other, Vector<Complex> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoPointwiseMultiply(other, result);
            }
            else
            {
                CommonParallel.For(
                    0,
                    _values.Length,
                    index => dense._values[index] = _values[index] * other[index]);
            }
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        /// <remarks></remarks>
        protected override void DoPointwiseDivide(Vector<Complex> other, Vector<Complex> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoPointwiseDivide(other, result);
            }
            else
            {
                CommonParallel.For(
                    0,
                    _values.Length,
                    index => dense._values[index] = _values[index] / other[index]);
            }
        }

        /// <summary>
        /// Outer product of two vectors
        /// </summary>
        /// <param name="u">First vector</param>
        /// <param name="v">Second vector</param>
        /// <returns>Matrix M[i,j] = u[i]*v[j] </returns>
        /// <exception cref="ArgumentNullException">If the u vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the v vector is <see langword="null" />.</exception> 
        public static DenseMatrix OuterProduct(DenseVector u, DenseVector v)
        {
            if (u == null)
            {
                throw new ArgumentNullException("u");
            }

            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            var matrix = new DenseMatrix(u.Count, v.Count);
            CommonParallel.For(
                0, 
                u.Count, 
                i =>
                {
                    for (var j = 0; j < v.Count; j++)
                    {
                        matrix.At(i, j, u._values[i] * v._values[j]);
                    }
                });
            return matrix;
        }

        /// <summary>
        /// Outer product of this and another vector.
        /// </summary>
        /// <param name="v">The vector to operate on.</param>
        /// <returns>
        /// Matrix M[i,j] = this[i] * v[j].
        /// </returns>
        /// <seealso cref="OuterProduct(DenseVector, DenseVector)"/>
        public Matrix<Complex> OuterProduct(DenseVector v)
        {
            return OuterProduct(this, v);
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>Scalar <c>ret = (sum(abs(this[i])^p))^(1/p)</c></returns>
        public override Complex Norm(double p)
        {
            if (p < 0.0)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            if (1.0 == p)
            {
                return SumMagnitudes();
            }

            if (2.0 == p)
            {
                return _values.Aggregate(Complex.Zero, SpecialFunctions.Hypotenuse).Magnitude;
            }

            if (Double.IsPositiveInfinity(p))
            {
                return CommonParallel.Aggregate(_values, (i, v) => v.Magnitude, Math.Max, 0d);
            }

            var sum = 0.0;

            for (var i = 0; i < _length; i++)
            {
                sum += Math.Pow(_values[i].Magnitude, p);
            }

            return Math.Pow(sum, 1.0 / p);
        }

        #region Parse Functions

        /// <summary>
        /// Creates a Complex dense vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n;n;..', '(n;n;..)', '[n;n;...]', where n is a Complex.
        /// </summary>
        /// <returns>
        /// A Complex dense vector containing the values specified by the given string.
        /// </returns>
        /// <param name="value">
        /// The string to parse.
        /// </param>
        public static DenseVector Parse(string value)
        {
            return Parse(value, null);
        }

        /// <summary>
        /// Creates a Complex dense vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n;n;..', '(n;n;..)', '[n;n;...]', where n is a double.
        /// </summary>
        /// <returns>
        /// A Complex dense vector containing the values specified by the given string.
        /// </returns>
        /// <param name="value">
        /// the string to parse.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        public static DenseVector Parse(string value, IFormatProvider formatProvider)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                throw new FormatException();
            }

            // strip out parens
            if (value.StartsWith("(", StringComparison.Ordinal))
            {
                if (!value.EndsWith(")", StringComparison.Ordinal))
                {
                    throw new FormatException();
                }

                value = value.Substring(1, value.Length - 2).Trim();
            }

            if (value.StartsWith("[", StringComparison.Ordinal))
            {
                if (!value.EndsWith("]", StringComparison.Ordinal))
                {
                    throw new FormatException();
                }

                value = value.Substring(1, value.Length - 2).Trim();
            }

            // keywords
            var textInfo = formatProvider.GetTextInfo();
            var keywords = new[] { textInfo.ListSeparator };

            // lexing
            var tokens = new LinkedList<string>();
            GlobalizationHelper.Tokenize(tokens.AddFirst(value), keywords, 0);
            var token = tokens.First;

            if (token == null || tokens.Count.IsEven())
            {
                throw new FormatException();
            }

            // parsing
            var data = new Complex[(tokens.Count + 1) >> 1];
            for (var i = 0; i < data.Length; i++)
            {
                if (token == null || token.Value == textInfo.ListSeparator)
                {
                    throw new FormatException();
                }

                data[i] = token.Value.ToComplex(formatProvider);

                token = token.Next;
                if (token != null)
                {
                    token = token.Next;
                }
            }

            return new DenseVector(data);
        }

        /// <summary>
        /// Converts the string representation of a complex dense vector to double-precision dense vector equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">
        /// A string containing a complex vector to convert.
        /// </param>
        /// <param name="result">
        /// The parsed value.
        /// </param>
        /// <returns>
        /// If the conversion succeeds, the result will contain a complex number equivalent to value.
        /// Otherwise the result will be <c>null</c>.
        /// </returns>
        public static bool TryParse(string value, out DenseVector result)
        {
            return TryParse(value, null, out result);
        }

        /// <summary>
        /// Converts the string representation of a complex dense vector to double-precision dense vector equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">
        /// A string containing a complex vector to convert.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information about value.
        /// </param>
        /// <param name="result">
        /// The parsed value.
        /// </param>
        /// <returns>
        /// If the conversion succeeds, the result will contain a complex number equivalent to value.
        /// Otherwise the result will be <c>null</c>.
        /// </returns>
        public static bool TryParse(string value, IFormatProvider formatProvider, out DenseVector result)
        {
            bool ret;
            try
            {
                result = Parse(value, formatProvider);
                ret = true;
            }
            catch (ArgumentNullException)
            {
                result = null;
                ret = false;
            }
            catch (FormatException)
            {
                result = null;
                ret = false;
            }

            return ret;
        }

        #endregion

        /// <summary>
        /// Conjugates vector and save result to <paramref name="target"/>
        /// </summary>
        /// <param name="target">Target vector</param>
        protected override void DoConjugate(Vector<Complex> target)
        {
            var denseTarget = target as DenseVector;

            if (denseTarget == null)
            {
                base.DoConjugate(target);
            }
            else
            {
                CommonParallel.For(
                    0,
                    _length,
                    index => denseTarget._values[index] = _values[index].Conjugate());
            }
        }
    }
}
