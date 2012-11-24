// <copyright file="SparseVector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2011 Math.NET
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Generic;
    using NumberTheory;
    using Properties;
    using Storage;
    using Threading;

    /// <summary>
    /// A vector with sparse storage.
    /// </summary>
    /// <remarks>The sparse vector is not thread safe.</remarks>
    [Serializable]
    public class SparseVector : Vector
    {
        readonly SparseVectorStorage<double> _storage;

        internal SparseVector(SparseVectorStorage<double> storage)
            : base(storage)
        {
            _storage = storage;
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector"/> class with a given size.
        /// </summary>
        /// <param name="size">
        /// the size of the vector.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="size"/> is less than one.
        /// </exception>
        public SparseVector(int size)
            : this(new SparseVectorStorage<double>(size))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector"/> class with a given size
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
        [Obsolete("Use a dense vector instead.")]
        public SparseVector(int size, double value)
            : this(new SparseVectorStorage<double>(size))
        {
            if (value == 0.0)
            {
                return;
            }

            var valueCount = _storage.ValueCount = size;
            var indices = _storage.Indices = new int[valueCount];
            var values = _storage.Values = new double[valueCount];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = value;
                indices[i] = i;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector"/> class by
        /// copying the values from another.
        /// </summary>
        /// <param name="other">
        /// The vector to create the new vector from.
        /// </param>
        public SparseVector(Vector<double> other)
            : this(new SparseVectorStorage<double>(other.Count))
        {
            other.Storage.CopyTo(Storage, skipClearing: true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector"/> class for an array.
        /// </summary>
        /// <param name="array">The array to create this vector from.</param>
        /// <remarks>The vector copy the array. Any changes to the vector will NOT change the array.</remarks>
        public SparseVector(IList<double> array)
            : this(new SparseVectorStorage<double>(array.Count))
        {
            for (var i = 0; i < array.Count; i++)
            {
                Storage.At(i, array[i]);
            }
        }

        #endregion

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>This vector as a column matrix.</returns>
        public override Matrix<double> ToColumnMatrix()
        {
            var indices = _storage.Indices;
            var values = _storage.Values;

            var matrix = new SparseMatrix(Count, 1);
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                matrix.At(indices[i], 0, values[i]);
            }

            return matrix;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>This vector as a row matrix.</returns>
        public override Matrix<double> ToRowMatrix()
        {
            var indices = _storage.Indices;
            var values = _storage.Values;

            var matrix = new SparseMatrix(1, Count);
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                matrix.At(0, indices[i], values[i]);
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
        public override Matrix<double> CreateMatrix(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
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
            return new SparseVector(size);
        }

        #region Operators and supplementary functions

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// Warning, the new 'sparse vector' with a non-zero scalar added to it will be a 100% filled
        /// sparse vector and very inefficient. Would be better to work with a dense vector instead.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to add.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the addition.
        /// </param>
        protected override void DoAdd(double scalar, Vector<double> result)
        {
            if (scalar == 0.0)
            {
                if (!ReferenceEquals(this, result))
                {
                    CopyTo(result); 
                }

                return;
            }

            if (ReferenceEquals(this, result))
            {
                //populate a new vector with the scalar   
                var vnonZeroValues = new double[Count];
                var vnonZeroIndices = new int[Count];
                for (int index = 0; index < Count; index++)
                {
                    vnonZeroIndices[index] = index;
                    vnonZeroValues[index] = scalar;
                }

                //populate the non zero values from this
                var indices = _storage.Indices;
                var values = _storage.Values;
                for (int j = 0; j < _storage.ValueCount; j++)
                {
                    vnonZeroValues[indices[j]] = values[j] + scalar;
                }

                //assign this vectors arrary to the new arrays.
                _storage.Values = vnonZeroValues;
                _storage.Indices = vnonZeroIndices;
                _storage.ValueCount = Count;
            }
            else
            {
                for (var index = 0; index < Count; index++)
                {
                    result.At(index, At(index) + scalar);
                }
            }
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">
        /// The vector to add to this one.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the addition.
        /// </param>
        protected override void DoAdd(Vector<double> other, Vector<double> result)
        {
            var otherSparse = other as SparseVector;
            if (otherSparse == null)
            {
                base.DoAdd(other, result);
                return;
            }

            var resultSparse = result as SparseVector;
            if (resultSparse == null)
            {
                base.DoAdd(other, result);
                return;
            }

            // TODO (ruegg, 2011-10-11): Options to optimize?

            var otherStorage = otherSparse._storage;
            if (ReferenceEquals(this, resultSparse))
            {
                int i = 0, j = 0;
                while (j < otherStorage.ValueCount)
                {
                    if (i >= _storage.ValueCount || _storage.Indices[i] > otherStorage.Indices[j])
                    {
                        var otherValue = otherStorage.Values[j];
                        if (otherValue != 0.0)
                        {
                            _storage.InsertAtIndexUnchecked(i++, otherStorage.Indices[j], otherValue);
                        }
                        j++;
                    }
                    else if (_storage.Indices[i] == otherStorage.Indices[j])
                    {
                        // TODO: result can be zero, remove?
                        _storage.Values[i++] += otherStorage.Values[j++];
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            else
            {
                result.Clear();
                int i = 0, j = 0, last = -1;
                while (i < _storage.ValueCount || j < otherStorage.ValueCount)
                {
                    if (j >= otherStorage.ValueCount || i < _storage.ValueCount && _storage.Indices[i] <= otherStorage.Indices[j])
                    {
                        var next = _storage.Indices[i];
                        if (next != last)
                        {
                            last = next;
                            result.At(next, _storage.Values[i] + otherSparse.At(next));
                        }
                        i++;
                    }
                    else
                    {
                        var next = otherStorage.Indices[j];
                        if (next != last)
                        {
                            last = next;
                            result.At(next, At(next) + otherStorage.Values[j]);
                        }
                        j++;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the same values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <remarks>This method is included for completeness.</remarks>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing a the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator +(SparseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SparseVector)rightSide.Plus();
        }

        /// <summary>
        /// Adds two <strong>Vectors</strong> together and returns the results.
        /// </summary>
        /// <param name="leftSide">One of the vectors to add.</param>
        /// <param name="rightSide">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator +(SparseVector leftSide, SparseVector rightSide)
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

            return (SparseVector)leftSide.Add(rightSide);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to subtract.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the subtraction.
        /// </param>
        protected override void DoSubtract(double scalar, Vector<double> result)
        {
            DoAdd(-scalar, result);
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">
        /// The vector to subtract from this one.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the subtraction.
        /// </param>
        protected override void DoSubtract(Vector<double> other, Vector<double> result)
        {
            if (ReferenceEquals(this, other))
            {
                result.Clear();
                return;
            }

            var otherSparse = other as SparseVector;
            if (otherSparse == null)
            {
                base.DoSubtract(other, result);
                return;
            }

            var resultSparse = result as SparseVector;
            if (resultSparse == null)
            {
                base.DoSubtract(other, result);
                return;
            }

            // TODO (ruegg, 2011-10-11): Options to optimize?

            var otherStorage = otherSparse._storage;
            if (ReferenceEquals(this, resultSparse))
            {
                int i = 0, j = 0;
                while (j < otherStorage.ValueCount)
                {
                    if (i >= _storage.ValueCount || _storage.Indices[i] > otherStorage.Indices[j])
                    {
                        var otherValue = otherStorage.Values[j];
                        if (otherValue != 0.0)
                        {
                            _storage.InsertAtIndexUnchecked(i++, otherStorage.Indices[j], -otherValue);
                        }
                        j++;
                    }
                    else if (_storage.Indices[i] == otherStorage.Indices[j])
                    {
                        // TODO: result can be zero, remove?
                        _storage.Values[i++] -= otherStorage.Values[j++];
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            else
            {
                result.Clear();
                int i = 0, j = 0, last = -1;
                while (i < _storage.ValueCount || j < otherStorage.ValueCount)
                {
                    if (j >= otherStorage.ValueCount || i < _storage.ValueCount && _storage.Indices[i] <= otherStorage.Indices[j])
                    {
                        var next = _storage.Indices[i];
                        if (next != last)
                        {
                            last = next;
                            result.At(next, _storage.Values[i] - otherSparse.At(next));
                        }
                        i++;
                    }
                    else
                    {
                        var next = otherStorage.Indices[j];
                        if (next != last)
                        {
                            last = next;
                            result.At(next, At(next) - otherStorage.Values[j]);
                        }
                        j++;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the negated values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the negated values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator -(SparseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SparseVector)rightSide.Negate();
        }

        /// <summary>
        /// Subtracts two <strong>Vectors</strong> and returns the results.
        /// </summary>
        /// <param name="leftSide">The vector to subtract from.</param>
        /// <param name="rightSide">The vector to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator -(SparseVector leftSide, SparseVector rightSide)
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

            return (SparseVector)leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        /// <returns>The negated vector.</returns>
        /// <remarks>Added as an alternative to the unary negation operator.</remarks>
        public override Vector<double> Negate()
        {
            var result = new SparseVectorStorage<double>(Count);
            var valueCount = result.ValueCount = _storage.ValueCount;
            var indices = result.Indices = new int[valueCount];
            var values = result.Values = new double[valueCount];

            if (valueCount != 0)
            {
                CommonParallel.For(0, valueCount, index => values[index] = -_storage.Values[index]);
                Buffer.BlockCopy(_storage.Indices, 0, indices, 0, valueCount * Constants.SizeOfInt);
            }

            return new SparseVector(result);
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <returns>A new vector that is the multiplication of the vector and the scalar.</returns>
        public override Vector<double> Multiply(double scalar)
        {
            if (scalar == 1.0)
            {
                return Clone();
            }

            if (scalar == 0)
            {
                return new SparseVector(Count);
            }

            var copy = new SparseVector(this);
            Control.LinearAlgebraProvider.ScaleArray(scalar, copy._storage.Values, copy._storage.Values);
            return copy;
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to multiply.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the multiplication.
        /// </param>
        protected override void DoMultiply(double scalar, Vector<double> result)
        {
            if (scalar == 1.0)
            {
                if (!ReferenceEquals(this, result))
                {
                    CopyTo(result);
                }
                
                return;
            }

            if (scalar == 0)
            {
                result.Clear();
                return;
            }

            var sparseResult = result as SparseVector;
            if (sparseResult == null)
            {
                result.Clear();
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    result.At(_storage.Indices[index], scalar * _storage.Values[index]);
                }
            }
            else
            {
                if (!ReferenceEquals(this, result))
                {
                    sparseResult._storage.ValueCount = _storage.ValueCount;
                    sparseResult._storage.Indices = new int[_storage.ValueCount];
                    Buffer.BlockCopy(_storage.Indices, 0, sparseResult._storage.Indices, 0, _storage.ValueCount * Constants.SizeOfInt);
                    sparseResult._storage.Values = new double[_storage.ValueCount];
                    Buffer.BlockCopy(_storage.Values, 0, sparseResult._storage.Values, 0, _storage.ValueCount * Constants.SizeOfDouble);
                }

                Control.LinearAlgebraProvider.ScaleArray(scalar, sparseResult._storage.Values, sparseResult._storage.Values);
            }
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">
        /// The other vector to add.
        /// </param>
        /// <returns>s
        /// The result of the addition.
        /// </returns>
        protected override double DoDotProduct(Vector<double> other)
        {
            var result = 0.0;

            if (ReferenceEquals(this, other))
            {
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    result += _storage.Values[i] * _storage.Values[i];
                }
            }
            else
            {
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    result += _storage.Values[i] * other.At(_storage.Indices[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to scale.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator *(SparseVector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseVector)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The scalar value.</param>
        /// <param name="rightSide">The vector to scale.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator *(double leftSide, SparseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SparseVector)rightSide.Multiply(leftSide);
        }

        /// <summary>
        /// Computes the dot product between two <strong>Vectors</strong>.
        /// </summary>
        /// <param name="leftSide">The left row vector.</param>
        /// <param name="rightSide">The right column vector.</param>
        /// <returns>The dot product between the two vectors.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static double operator *(SparseVector leftSide, SparseVector rightSide)
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

            return leftSide.DotProduct(rightSide);
        }

        /// <summary>
        /// Divides a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to divide.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator /(SparseVector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseVector)leftSide.Multiply(1.0 / rightSide);
        }

        /// <summary>
        /// Computes the modulus for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoModulus(double divisor, Vector<double> result)
        {
            if (ReferenceEquals(this, result))
            {
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    _storage.Values[index] %= divisor;
                }
            }
            else
            {
                result.Clear();
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    result.At(_storage.Indices[index], _storage.Values[index] % divisor);
                }
            }
        }

        /// <summary>
        /// Computes the modulus of each element of the vector of the given divisor.
        /// </summary>
        /// <param name="leftSide">The vector whose elements we want to compute the modulus of.</param>
        /// <param name="rightSide">The divisor to use,</param>
        /// <returns>The result of the calculation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator %(SparseVector leftSide, float rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseVector)leftSide.Modulus(rightSide);
        }

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>   
        public override int AbsoluteMinimumIndex()
        {
            if (_storage.ValueCount == 0)
            {
                // No non-zero elements. Return 0
                return 0;
            }

            var index = 0;
            var min = Math.Abs(_storage.Values[index]);
            for (var i = 1; i < _storage.ValueCount; i++)
            {
                var test = Math.Abs(_storage.Values[i]);
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return _storage.Indices[index];
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
        public override Vector<double> SubVector(int index, int length)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (index + length > Count)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            var result = new SparseVector(length);
            for (var i = index; i < index + length; i++)
            {
                result.At(i - index, At(i));
            }

            return result;
        }

        /// <summary>
        /// Set the values of this vector to the given values.
        /// </summary>
        /// <param name="values">The array containing the values to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="values"/> is not the same size as this vector.</exception>
        public override void SetValues(double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length != Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "values");
            }

            for (var i = 0; i < values.Length; i++)
            {
                At(i, values[i]);
            }
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>          
        public override int MaximumIndex()
        {
            if (_storage.ValueCount == 0)
            {
                return 0;
            }

            var index = 0;
            var max = _storage.Values[0];
            for (var i = 1; i < _storage.ValueCount; i++)
            {
                if (max < _storage.Values[i])
                {
                    index = i;
                    max = _storage.Values[i];
                }
            }

            return _storage.Indices[index];
        }

        /// <summary>
        /// Returns the index of the minimum element.
        /// </summary>
        /// <returns>The index of minimum element.</returns>  
        public override int MinimumIndex()
        {
            if (_storage.ValueCount == 0)
            {
                return 0;
            }

            var index = 0;
            var min = _storage.Values[0];
            for (var i = 1; i < _storage.ValueCount; i++)
            {
                if (min > _storage.Values[i])
                {
                    index = i;
                    min = _storage.Values[i];
                }
            }

            return _storage.Indices[index];
        }

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public override double Sum()
        {
            double result = 0;
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                result += _storage.Values[i];
            }

            return result;
        }

        /// <summary>
        /// Computes the sum of the absolute value of the vector's elements.
        /// </summary>
        /// <returns>The sum of the absolute value of the vector's elements.</returns>
        public override double SumMagnitudes()
        {
            double result = 0;
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                result += Math.Abs(_storage.Values[i]);
            }

            return result;
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Vector<double> other, Vector<double> result)
        {
            if (ReferenceEquals(this, other))
            {
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    _storage.Values[i] *= _storage.Values[i]; 
                }
            }
            else
            {
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    var index = _storage.Indices[i];
                    result.At(index, other.At(index) * _storage.Values[i]);
                }
            }
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseDivide(Vector<double> other, Vector<double> result)
        {
            if (ReferenceEquals(this, other))
            {
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    _storage.Values[i] /= _storage.Values[i];
                }
            }
            else
            {
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    var index = _storage.Indices[i];
                    result.At(index, _storage.Values[i] / other.At(index));
                }
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
        public static Matrix<double> OuterProduct(SparseVector u, SparseVector v)
        {
            if (u == null)
            {
                throw new ArgumentNullException("u");
            }

            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            var matrix = new SparseMatrix(u.Count, v.Count);
            for (var i = 0; i < u._storage.ValueCount; i++)
            {
                for (var j = 0; j < v._storage.ValueCount; j++)
                {
                    matrix.At(i, j, u._storage.Values[i] * v._storage.Values[j]);
                }
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
        public Matrix<double> OuterProduct(SparseVector v)
        {
            return OuterProduct(this, v);
        }

        #endregion

        #region Vector Norms

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>Scalar <c>ret = (sum(abs(this[i])^p))^(1/p)</c></returns>
        public override double Norm(double p)
        {
            if (1 > p)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            if (_storage.ValueCount == 0)
            {
                return 0.0;
            }

            if (2.0 == p)
            {
                return _storage.Values.Aggregate(0.0, SpecialFunctions.Hypotenuse);
            }

            if (Double.IsPositiveInfinity(p))
            {
                return CommonParallel.Aggregate(0, _storage.ValueCount, i => Math.Abs(_storage.Values[i]), Math.Max, 0d);
            }

            var sum = 0.0;
            for (var index = 0; index < _storage.ValueCount; index++)
            {
                sum += Math.Pow(Math.Abs(_storage.Values[index]), p);
            }

            return Math.Pow(sum, 1.0 / p);
        }

        #endregion

        #region Parse Functions

        /// <summary>
        /// Creates a double sparse vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n,n,..', '(n,n,..)', '[n,n,...]', where n is a double.
        /// </summary>
        /// <returns>
        /// A double sparse vector containing the values specified by the given string.
        /// </returns>
        /// <param name="value">
        /// The string to parse.
        /// </param>
        public static SparseVector Parse(string value)
        {
            return Parse(value, null);
        }

        /// <summary>
        /// Creates a double sparse vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n,n,..', '(n,n,..)', '[n,n,...]', where n is a double.
        /// </summary>
        /// <returns>
        /// A double sparse vector containing the values specified by the given string.
        /// </returns>
        /// <param name="value">
        /// the string to parse.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        public static SparseVector Parse(string value, IFormatProvider formatProvider)
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
            var data = new double[(tokens.Count + 1) >> 1];
            for (var i = 0; i < data.Length; i++)
            {
                if (token == null || token.Value == textInfo.ListSeparator)
                {
                    throw new FormatException();
                }

                data[i] = Double.Parse(token.Value, NumberStyles.Any, formatProvider);

                token = token.Next;
                if (token != null)
                {
                    token = token.Next;
                }
            }

            return new SparseVector(data);
        }

        /// <summary>
        /// Converts the string representation of a real sparse vector to double-precision sparse vector equivalent.
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
        public static bool TryParse(string value, out SparseVector result)
        {
            return TryParse(value, null, out result);
        }

        /// <summary>
        /// Converts the string representation of a real sparse vector to double-precision sparse vector equivalent.
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
        public static bool TryParse(string value, IFormatProvider formatProvider, out SparseVector result)
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

        #region System.Object override

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            if (Count > 20)
            {
                return String.Format("SparseVectorOfDouble({0},{1},{2})", Count, _storage.ValueCount, GetHashCode());
            }

            return base.ToString(format, formatProvider);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(_storage.ValueCount, 20);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
#if PORTABLE
                hash ^= Precision.DoubleToInt64Bits(_storage.Values[i].GetHashCode());
#else
                hash ^= BitConverter.DoubleToInt64Bits(_storage.Values[i].GetHashCode());
#endif
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
       /// </returns>
        public override bool Equals(Vector<double> other)
        {
            // Reject equality when the argument is null or has a different length.
            if (other == null)
            {
                return false;
            }

            if (Count != other.Count)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var otherSparse = other as SparseVector;
            if (otherSparse == null)
            {
                return base.Equals(other);
            }

            int i = 0, j = 0;
            while (i < _storage.ValueCount || j < otherSparse._storage.ValueCount)
            {
                if (j >= otherSparse._storage.ValueCount || i < _storage.ValueCount && _storage.Indices[i] < otherSparse._storage.Indices[j])
                {
                    if (_storage.Values[i++] != 0d)
                    {
                        return false;
                    }
                    continue;
                }

                if (i >= _storage.ValueCount || j < otherSparse._storage.ValueCount && otherSparse._storage.Indices[j] < _storage.Indices[i])
                {
                    if (otherSparse._storage.Values[j++] != 0d)
                    {
                        return false;
                    }
                    continue;
                }

                if (!_storage.Values[i].AlmostEqual(otherSparse._storage.Values[j]))
                {
                    return false;
                }

                i++;
                j++;
            }

            return true;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that contains the position and value of the element.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> over this vector that contains the position and value of each
        /// element.
        /// </returns>
        /// <remarks>
        /// The enumerator returns a 
        /// <seealso cref="Tuple{T,K}"/>
        /// with the first value being the element index and the second value 
        /// being the value of the element at that index. For sparse vectors, the enumerator will exclude all elements
        /// with a zero value.
        /// </remarks>
        public override IEnumerable<Tuple<int, double>> GetIndexedEnumerator()
        {
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                yield return new Tuple<int, double>(_storage.Indices[i], _storage.Values[i]);
            }
        }

        /// <summary>
        /// Returns the data contained in the vector as an array.
        /// </summary>
        /// <returns>
        /// The vector's data as an array.
        /// </returns>
        public override double[] ToArray()
        {
            var ret = new double[Count];
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                ret[_storage.Indices[i]] = _storage.Values[i];
            }

            return ret;
        }
    }
}
