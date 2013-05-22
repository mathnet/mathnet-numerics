// <copyright file="DenseVector.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using Distributions;
    using Generic;
    using Properties;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Threading;

    /// <summary>
    /// A vector using dense storage.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("DenseVector {Count}-Double")]
    public class DenseVector : Vector
    {
        /// <summary>
        /// Number of elements
        /// </summary>
        readonly int _length;

        /// <summary>
        /// Gets the vector's data.
        /// </summary>
        readonly double[] _values;

        /// <summary>
        /// Create a new dense vector straight from an initialized vector storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public DenseVector(DenseVectorStorage<double> storage)
            : base(storage)
        {
            _length = storage.Length;
            _values = storage.Data;
        }

        /// <summary>
        /// Create a new dense vector with the given length.
        /// All cells of the vector will be initialized to zero.
        /// Zero-length vectors are not supported.
        /// </summary>
        /// <exception cref="ArgumentException">If length is less than one.</exception>
        public DenseVector(int length)
            : this(new DenseVectorStorage<double>(length))
        {
        }

        /// <summary>
        /// Create a new dense vector directly binding to a raw array.
        /// The array is used directly without copying.
        /// Very efficient, but changes to the array and the vector will affect each other.
        /// </summary>
        public DenseVector(double[] storage)
            : this(new DenseVectorStorage<double>(storage.Length, storage))
        {
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static DenseVector OfVector(Vector<double> vector)
        {
            return new DenseVector(DenseVectorStorage<double>.OfVector(vector.Storage));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static DenseVector OfEnumerable(IEnumerable<double> enumerable)
        {
            return new DenseVector(DenseVectorStorage<double>.OfEnumerable(enumerable));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static DenseVector OfIndexedEnumerable(int length, IEnumerable<Tuple<int,double>> enumerable)
        {
            return new DenseVector(DenseVectorStorage<double>.OfIndexedEnumerable(length, enumerable));
        }

        /// <summary>
        /// Create a new dense vector and initialize each value using the provided init function.
        /// </summary>
        public static DenseVector Create(int length, Func<int, double> init)
        {
            return new DenseVector(DenseVectorStorage<double>.OfInit(length, init));
        }

        /// <summary>
        /// Create a new dense vector with values sampled from the provided random distribution.
        /// </summary>
        public static DenseVector CreateRandom(int length, IContinuousDistribution distribution)
        {
            return new DenseVector(DenseVectorStorage<double>.OfInit(length,
                i => distribution.Sample()));
        }

        /// <summary>
        /// Create a new dense vector with the given length.
        /// All cells of the vector will be initialized with the provided value.
        /// Zero-length vectors are not supported.
        /// </summary>
        /// <exception cref="ArgumentException">If length is less than one.</exception>
        [Obsolete("Use DenseVector.Create instead. Scheduled for removal in v3.0.")]
        public DenseVector(int length, double value)
            : this(DenseVectorStorage<double>.OfInit(length, i => value))
        {
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        [Obsolete("Use DenseVector.OfVector instead. Scheduled for removal in v3.0.")]
        public DenseVector(Vector<double> other)
            : this(DenseVectorStorage<double>.OfVector(other.Storage))
        {
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        [Obsolete("Use DenseVector.OfEnumerable instead. Scheduled for removal in v3.0.")]
        public DenseVector(IEnumerable<double> other)
            : this(DenseVectorStorage<double>.OfEnumerable(other))
        {
        }

        /// <summary>
        /// Gets the vector's data.
        /// </summary>
        /// <value>The vector's data.</value>
        public double[] Values
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
        public static explicit operator double[](DenseVector vector)
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
        public static implicit operator DenseVector(double[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            return new DenseVector(array);
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
        public override Matrix<double> CreateMatrix(int rows, int columns)
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
        public override Vector<double> CreateVector(int size)
        {
            return new DenseVector(size);
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        protected override void DoAdd(double scalar, Vector<double> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoAdd(scalar, result);
            }
            else
            {
                CommonParallel.For(0, _values.Length, 4096, (a, b) =>
                    {
                        for (int i = a; i < b; i++)
                        {
                            dense._values[i] = _values[i] + scalar;
                        }
                    });
            }
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        protected override void DoAdd(Vector<double> other, Vector<double> result)
        {
            var otherDense = other as DenseVector;
            var resultDense = result as DenseVector;

            if (otherDense == null || resultDense == null)
            {
                base.DoAdd(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.AddArrays(_values, otherDense._values, resultDense._values);
            }
        }

        /// <summary>
        /// Adds two <strong>Vectors</strong> together and returns the results.
        /// </summary>
        /// <param name="leftSide">One of the vectors to add.</param>
        /// <param name="rightSide">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator +(DenseVector leftSide, DenseVector rightSide)
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

            return (DenseVector)leftSide.Add(rightSide);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected override void DoSubtract(double scalar, Vector<double> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoSubtract(scalar, result);
            }
            else
            {
                CommonParallel.For(0, _values.Length, 4096, (a, b) =>
                    {
                        for (int i = a; i < b; i++)
                        {
                            dense._values[i] = _values[i] - scalar;
                        }
                    });
            }
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected override void DoSubtract(Vector<double> other, Vector<double> result)
        {
            var otherDense = other as DenseVector;
            var resultDense = result as DenseVector;

            if (otherDense == null || resultDense == null)
            {
                base.DoSubtract(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.SubtractArrays(_values, otherDense._values, resultDense._values);
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the negated values of <paramref name="rightSide"/>.
        /// </summary>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the negated values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator -(DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseVector)rightSide.Negate();
        }

        /// <summary>
        /// Subtracts two <strong>Vectors</strong> and returns the results.
        /// </summary>
        /// <param name="leftSide">The vector to subtract from.</param>
        /// <param name="rightSide">The vector to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator -(DenseVector leftSide, DenseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Negates vector and saves result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoNegate(Vector<double> result)
        {
            var denseResult = result as DenseVector;
            if (denseResult == null)
            {
                base.DoNegate(result);
            }
            else
            {
                Control.LinearAlgebraProvider.ScaleArray(-1.0d, _values, denseResult.Values);
            }
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="result">The vector to store the result of the multiplication.</param>
        /// <remarks></remarks>
        protected override void DoMultiply(double scalar, Vector<double> result)
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
        protected override double DoDotProduct(Vector<double> other)
        {
            var denseVector = other as DenseVector;

            return denseVector == null ? base.DoDotProduct(other) : Control.LinearAlgebraProvider.DotProduct(_values, denseVector.Values);
        }

        /// <summary>
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to scale.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator *(DenseVector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The scalar value.</param>
        /// <param name="rightSide">The vector to scale.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator *(double leftSide, DenseVector rightSide)
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
        public static double operator *(DenseVector leftSide, DenseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.DotProduct(rightSide);
        }

        /// <summary>
        /// Divides a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to divide.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator /(DenseVector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Divide(rightSide);
        }

        /// <summary>
        /// Computes the modulus for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoModulus(double divisor, Vector<double> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoModulus(divisor, result);
            }
            else
            {
                CommonParallel.For(0, _length, 4096, (a, b) =>
                    {
                        for (int i = a; i < b; i++)
                        {
                            dense._values[i] = _values[i]%divisor;
                        }
                    });
            }
        }

        /// <summary>
        /// Computes the modulus of each element of the vector of the given divisor.
        /// </summary>
        /// <param name="leftSide">The vector whose elements we want to compute the modulus of.</param>
        /// <param name="rightSide">The divisor to use,</param>
        /// <returns>The result of the calculation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator %(DenseVector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Modulus(rightSide);
        }

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>
        public override int AbsoluteMinimumIndex()
        {
            var index = 0;
            var min = Math.Abs(_values[index]);
            for (var i = 1; i < _length; i++)
            {
                var test = Math.Abs(_values[i]);
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
        public override double AbsoluteMinimum()
        {
            return Math.Abs(_values[AbsoluteMinimumIndex()]);
        }

        /// <summary>
        /// Returns the value of the absolute maximum element.
        /// </summary>
        /// <returns>The value of the absolute maximum element.</returns>
        public override double AbsoluteMaximum()
        {
            return Math.Abs(_values[AbsoluteMaximumIndex()]);
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>
        public override int AbsoluteMaximumIndex()
        {
            var index = 0;
            var max = Math.Abs(_values[index]);
            for (var i = 1; i < _length; i++)
            {
                var test = Math.Abs(_values[i]);
                if (test > max)
                {
                    index = i;
                    max = test;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>
        public override int MaximumIndex()
        {
            var index = 0;
            var max = _values[0];
            for (var i = 1; i < _length; i++)
            {
                if (max < _values[i])
                {
                    index = i;
                    max = _values[i];
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the index of the minimum element.
        /// </summary>
        /// <returns>The index of minimum element.</returns>
        public override int MinimumIndex()
        {
            var index = 0;
            var min = _values[0];
            for (var i = 1; i < _length; i++)
            {
                if (min > _values[i])
                {
                    index = i;
                    min = _values[i];
                }
            }

            return index;
        }

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public override double Sum()
        {
            var sum = 0.0;

            for (var index = 0; index < _length; index++)
            {
                sum += _values[index];
            }

            return sum;
        }

        /// <summary>
        /// Computes the sum of the absolute value of the vector's elements.
        /// </summary>
        /// <returns>The sum of the absolute value of the vector's elements.</returns>
        public override double SumMagnitudes()
        {
            var sum = 0.0;

            for (var index = 0; index < _length; index++)
            {
                sum += Math.Abs(_values[index]);
            }

            return sum;
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        protected override void DoPointwiseMultiply(Vector<double> other, Vector<double> result)
        {
            var denseOther = other as DenseVector;
            var denseResult = result as DenseVector;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseMultiplyArrays(_values, denseOther._values, denseResult._values);
            }
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        /// <remarks></remarks>
        protected override void DoPointwiseDivide(Vector<double> other, Vector<double> result)
        {
            var denseOther = other as DenseVector;
            var denseResult = result as DenseVector;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseDivide(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseDivideArrays(_values, denseOther._values, denseResult._values);
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
            CommonParallel.For(0, u.Count, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        for (var j = 0; j < v.Count; j++)
                        {
                            matrix.At(i, j, u._values[i]*v._values[j]);
                        }
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
        public Matrix<double> OuterProduct(DenseVector v)
        {
            return OuterProduct(this, v);
        }

        #region Vector Norms

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>Scalar <c>ret = (sum(abs(this[i])^p))^(1/p)</c></returns>
        public override double Norm(double p)
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
                return _values.Aggregate(0.0, SpecialFunctions.Hypotenuse);
            }

            if (Double.IsPositiveInfinity(p))
            {
                return CommonParallel.Aggregate(_values, (i, v) => Math.Abs(v), Math.Max, 0d);
            }

            var sum = 0.0;
            for (var index = 0; index < _length; index++)
            {
                sum += Math.Pow(Math.Abs(_values[index]), p);
            }

            return Math.Pow(sum, 1.0 / p);
        }

        #endregion

        #region Parse Functions

        /// <summary>
        /// Creates a double dense vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n,n,..', '(n,n,..)', '[n,n,...]', where n is a double.
        /// </summary>
        /// <returns>
        /// A double dense vector containing the values specified by the given string.
        /// </returns>
        /// <param name="value">
        /// The string to parse.
        /// </param>
        public static DenseVector Parse(string value)
        {
            return Parse(value, null);
        }

        /// <summary>
        /// Creates a double dense vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n,n,..', '(n,n,..)', '[n,n,...]', where n is a double.
        /// </summary>
        /// <returns>
        /// A double dense vector containing the values specified by the given string.
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

            // parsing
            var tokens = value.Split(new[] {formatProvider.GetTextInfo().ListSeparator, " ", "\t"}, StringSplitOptions.RemoveEmptyEntries);
            var data = tokens.Select(t => Double.Parse(t, NumberStyles.Any, formatProvider)).ToArray();
            if (data.Length == 0) throw new FormatException();
            return new DenseVector(data);
        }

        /// <summary>
        /// Converts the string representation of a real dense vector to double-precision dense vector equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">
        /// A string containing a real vector to convert.
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
        /// Converts the string representation of a real dense vector to double-precision dense vector equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">
        /// A string containing a real vector to convert.
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
    }
}
