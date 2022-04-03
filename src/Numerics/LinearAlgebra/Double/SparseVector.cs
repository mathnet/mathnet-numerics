// <copyright file="SparseVector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Providers.LinearAlgebra;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Double
{
    /// <summary>
    /// A vector with sparse storage, intended for very large vectors where most of the cells are zero.
    /// </summary>
    /// <remarks>The sparse vector is not thread safe.</remarks>
    [Serializable]
    [DebuggerDisplay("SparseVector {Count}-Double {NonZerosCount}-NonZero")]
    public class SparseVector : Vector
    {
        readonly SparseVectorStorage<double> _storage;

        /// <summary>
        /// Gets the number of non zero elements in the vector.
        /// </summary>
        /// <value>The number of non zero elements.</value>
        public int NonZerosCount => _storage.ValueCount;

        /// <summary>
        /// Create a new sparse vector straight from an initialized vector storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public SparseVector(SparseVectorStorage<double> storage)
            : base(storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Create a new sparse vector with the given length.
        /// All cells of the vector will be initialized to zero.
        /// </summary>
        /// <exception cref="ArgumentException">If length is less than one.</exception>
        public SparseVector(int length)
            : this(new SparseVectorStorage<double>(length))
        {
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static SparseVector OfVector(Vector<double> vector)
        {
            return new SparseVector(SparseVectorStorage<double>.OfVector(vector.Storage));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static SparseVector OfEnumerable(IEnumerable<double> enumerable)
        {
            return new SparseVector(SparseVectorStorage<double>.OfEnumerable(enumerable));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static SparseVector OfIndexedEnumerable(int length, IEnumerable<Tuple<int, double>> enumerable)
        {
            return new SparseVector(SparseVectorStorage<double>.OfIndexedEnumerable(length, enumerable));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static SparseVector OfIndexedEnumerable(int length, IEnumerable<(int, double)> enumerable)
        {
            return new SparseVector(SparseVectorStorage<double>.OfIndexedEnumerable(length, enumerable));
        }

        /// <summary>
        /// Create a new sparse vector and initialize each value using the provided value.
        /// </summary>
        public static SparseVector Create(int length, double value)
        {
            return new SparseVector(SparseVectorStorage<double>.OfValue(length, value));
        }

        /// <summary>
        /// Create a new sparse vector and initialize each value using the provided init function.
        /// </summary>
        public static SparseVector Create(int length, Func<int, double> init)
        {
            return new SparseVector(SparseVectorStorage<double>.OfInit(length, init));
        }

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
                // populate a new vector with the scalar
                var vnonZeroValues = new double[Count];
                var vnonZeroIndices = new int[Count];
                for (int index = 0; index < Count; index++)
                {
                    vnonZeroIndices[index] = index;
                    vnonZeroValues[index] = scalar;
                }

                // populate the non zero values from this
                var indices = _storage.Indices;
                var values = _storage.Values;
                for (int j = 0; j < _storage.ValueCount; j++)
                {
                    vnonZeroValues[indices[j]] = values[j] + scalar;
                }

                // assign this vectors array to the new arrays.
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
            if (other is SparseVector otherSparse && result is SparseVector resultSparse)
            {
                // TODO (ruegg, 2011-10-11): Options to optimize?

                var otherStorage = otherSparse._storage;
                var otherStorageIndices = otherStorage.Indices;
                var otherStorageValues = otherStorage.Values;

                if (ReferenceEquals(this, resultSparse))
                {
                    int i = 0, j = 0;
                    while (j < otherStorage.ValueCount)
                    {
                        if (i >= _storage.ValueCount || _storage.Indices[i] > otherStorageIndices[j])
                        {
                            var otherValue = otherStorageValues[j];
                            if (otherValue != 0.0)
                            {
                                _storage.InsertAtIndexUnchecked(i++, otherStorageIndices[j], otherValue);
                            }

                            j++;
                        }
                        else if (_storage.Indices[i] == otherStorageIndices[j])
                        {
                            // TODO: result can be zero, remove?
                            _storage.Values[i++] += otherStorageValues[j++];
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
                        if (j >= otherStorage.ValueCount || i < _storage.ValueCount && _storage.Indices[i] <= otherStorageIndices[j])
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
                            var next = otherStorageIndices[j];
                            if (next != last)
                            {
                                last = next;
                                result.At(next, At(next) + otherStorageValues[j]);
                            }

                            j++;
                        }
                    }
                }
            }
            else
            {
                base.DoAdd(other, result);
            }
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

            if (other is SparseVector otherSparse && result is SparseVector resultSparse)
            {
                // TODO (ruegg, 2011-10-11): Options to optimize?

                var otherStorage = otherSparse._storage;
                var otherStorageIndices = otherStorage.Indices;
                var otherStorageValues = otherStorage.Values;

                if (ReferenceEquals(this, resultSparse))
                {
                    int i = 0, j = 0;
                    while (j < otherStorage.ValueCount)
                    {
                        if (i >= _storage.ValueCount || _storage.Indices[i] > otherStorageIndices[j])
                        {
                            var otherValue = otherStorageValues[j];
                            if (otherValue != 0.0)
                            {
                                _storage.InsertAtIndexUnchecked(i++, otherStorageIndices[j], -otherValue);
                            }

                            j++;
                        }
                        else if (_storage.Indices[i] == otherStorageIndices[j])
                        {
                            // TODO: result can be zero, remove?
                            _storage.Values[i++] -= otherStorageValues[j++];
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
                        if (j >= otherStorage.ValueCount || i < _storage.ValueCount && _storage.Indices[i] <= otherStorageIndices[j])
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
                            var next = otherStorageIndices[j];
                            if (next != last)
                            {
                                last = next;
                                result.At(next, At(next) - otherStorageValues[j]);
                            }

                            j++;
                        }
                    }
                }
            }
            else
            {
                base.DoSubtract(other, result);
            }
        }

        /// <summary>
        /// Negates vector and saves result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoNegate(Vector<double> result)
        {
            if (result is SparseVector sparseResult)
            {
                if (!ReferenceEquals(this, result))
                {
                    sparseResult._storage.ValueCount = _storage.ValueCount;
                    sparseResult._storage.Indices = new int[_storage.ValueCount];
                    Buffer.BlockCopy(_storage.Indices, 0, sparseResult._storage.Indices, 0, _storage.ValueCount * Constants.SizeOfInt);
                    sparseResult._storage.Values = new double[_storage.ValueCount];
                    Array.Copy(_storage.Values, 0, sparseResult._storage.Values, 0, _storage.ValueCount);
                }

                LinearAlgebraControl.Provider.ScaleArray(-1.0d, sparseResult._storage.Values, sparseResult._storage.Values);
            }
            else
            {
                var storageIndices = _storage.Indices;
                var storageValues = _storage.Values;

                result.Clear();
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    result.At(storageIndices[index], -storageValues[index]);
                }
            }
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
            if (result is SparseVector sparseResult)
            {
                if (!ReferenceEquals(this, result))
                {
                    sparseResult._storage.ValueCount = _storage.ValueCount;
                    sparseResult._storage.Indices = new int[_storage.ValueCount];
                    Buffer.BlockCopy(_storage.Indices, 0, sparseResult._storage.Indices, 0, _storage.ValueCount * Constants.SizeOfInt);
                    sparseResult._storage.Values = new double[_storage.ValueCount];
                    Array.Copy(_storage.Values, 0, sparseResult._storage.Values, 0, _storage.ValueCount);
                }

                LinearAlgebraControl.Provider.ScaleArray(scalar, sparseResult._storage.Values, sparseResult._storage.Values);
            }
            else
            {
                var storageIndices = _storage.Indices;
                var storageValues = _storage.Values;

                result.Clear();
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    result.At(storageIndices[index], scalar * storageValues[index]);
                }
            }
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The sum of a[i]*b[i] for all i.</returns>
        protected override double DoDotProduct(Vector<double> other)
        {
            var storageIndices = _storage.Indices;
            var storageValues = _storage.Values;

            var result = 0d;
            if (ReferenceEquals(this, other))
            {
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    result += storageValues[i] * storageValues[i];
                }
            }
            else
            {
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    result += storageValues[i] * other.At(storageIndices[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoModulus(double divisor, Vector<double> result)
        {
            var storageIndices = _storage.Indices;
            var storageValues = _storage.Values;

            if (ReferenceEquals(this, result))
            {
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    storageValues[index] = Euclid.Modulus(storageValues[index], divisor);
                }
            }
            else
            {
                result.Clear();
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    result.At(storageIndices[index], Euclid.Modulus(storageValues[index], divisor));
                }
            }
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoRemainder(double divisor, Vector<double> result)
        {
            var storageIndices = _storage.Indices;
            var storageValues = _storage.Values;

            if (ReferenceEquals(this, result))
            {
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    storageValues[index] %= divisor;
                }
            }
            else
            {
                result.Clear();
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    result.At(storageIndices[index], storageValues[index]%divisor);
                }
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
        public static SparseVector operator +(SparseVector leftSide, SparseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException(nameof(leftSide));
            }

            return (SparseVector)leftSide.Add(rightSide);
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
                throw new ArgumentNullException(nameof(rightSide));
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
            if (leftSide == null)
            {
                throw new ArgumentNullException(nameof(leftSide));
            }

            return (SparseVector)leftSide.Subtract(rightSide);
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
                throw new ArgumentNullException(nameof(leftSide));
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
                throw new ArgumentNullException(nameof(rightSide));
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
            if (leftSide == null)
            {
                throw new ArgumentNullException(nameof(leftSide));
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
                throw new ArgumentNullException(nameof(leftSide));
            }

            return (SparseVector)leftSide.Divide(rightSide);
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// of each element of the vector of the given divisor.
        /// </summary>
        /// <param name="leftSide">The vector whose elements we want to compute the modulus of.</param>
        /// <param name="rightSide">The divisor to use,</param>
        /// <returns>The result of the calculation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator %(SparseVector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException(nameof(leftSide));
            }

            return (SparseVector)leftSide.Remainder(rightSide);
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

            var storageValues = _storage.Values;

            var index = 0;
            var min = Math.Abs(storageValues[index]);
            for (var i = 1; i < _storage.ValueCount; i++)
            {
                var test = Math.Abs(storageValues[i]);
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return _storage.Indices[index];
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>
        public override int AbsoluteMaximumIndex()
        {
            if (_storage.ValueCount == 0)
            {
                // No non-zero elements. Return 0
                return 0;
            }

            var storageValues = _storage.Values;

            var index = 0;
            var max = Math.Abs(storageValues[index]);
            for (var i = 1; i < _storage.ValueCount; i++)
            {
                var test = Math.Abs(storageValues[i]);
                if (test > max)
                {
                    index = i;
                    max = test;
                }
            }

            return _storage.Indices[index];
        }

        /// <summary>
        /// Returns the index of the maximum element.
        /// </summary>
        /// <returns>The index of maximum element.</returns>
        public override int MaximumIndex()
        {
            if (_storage.ValueCount == 0)
            {
                return 0;
            }

            var storageValues = _storage.Values;

            var index = 0;
            var max = storageValues[0];
            for (var i = 1; i < _storage.ValueCount; i++)
            {
                if (max < storageValues[i])
                {
                    index = i;
                    max = storageValues[i];
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

            var storageValues = _storage.Values;

            var index = 0;
            var min = storageValues[0];
            for (var i = 1; i < _storage.ValueCount; i++)
            {
                if (min > storageValues[i])
                {
                    index = i;
                    min = storageValues[i];
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
            var storageValues = _storage.Values;

            double result = 0;
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                result += storageValues[i];
            }
            return result;
        }

        /// <summary>
        /// Calculates the L1 norm of the vector, also known as Manhattan norm.
        /// </summary>
        /// <returns>The sum of the absolute values.</returns>
        public override double L1Norm()
        {
            var storageValues = _storage.Values;

            var result = 0d;
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                result += Math.Abs(storageValues[i]);
            }
            return result;
        }

        /// <summary>
        /// Calculates the infinity norm of the vector.
        /// </summary>
        /// <returns>The maximum absolute value.</returns>
        public override double InfinityNorm()
        {
            return CommonParallel.Aggregate(0, _storage.ValueCount, i => Math.Abs(_storage.Values[i]), Math.Max, 0d);
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>Scalar <c>ret = ( ∑|this[i]|^p )^(1/p)</c></returns>
        public override double Norm(double p)
        {
            if (p < 0d) throw new ArgumentOutOfRangeException(nameof(p));

            if (_storage.ValueCount == 0)
            {
                return 0d;
            }

            if (p == 1d) return L1Norm();
            if (p == 2d) return L2Norm();
            if (double.IsPositiveInfinity(p)) return InfinityNorm();

            var storageValues = _storage.Values;

            var sum = 0d;
            for (var index = 0; index < _storage.ValueCount; index++)
            {
                sum += Math.Pow(Math.Abs(storageValues[index]), p);
            }
            return Math.Pow(sum, 1.0 / p);
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Vector<double> other, Vector<double> result)
        {
            if (ReferenceEquals(this, other) && ReferenceEquals(this, result))
            {
                var storageValues = _storage.Values;
                for (var i = 0; i < _storage.ValueCount; i++)
                {
                    storageValues[i] *= storageValues[i];
                }
            }
            else
            {
                base.DoPointwiseMultiply(other, result);
            }
        }

        #region Parse Functions

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
        public static SparseVector Parse(string value, IFormatProvider formatProvider = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
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
            var tokens = value.Split(new[] { formatProvider.GetTextInfo().ListSeparator, " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            var data = tokens.Select(t => double.Parse(t, NumberStyles.Any, formatProvider)).ToList();
            if (data.Count == 0) throw new FormatException();
            return new SparseVector(SparseVectorStorage<double>.OfEnumerable(data));
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

        public override string ToTypeString()
        {
            return FormattableString.Invariant($"SparseVector {Count}-Double {NonZerosCount / (double) Count:P2} Filled");
        }
    }
}
