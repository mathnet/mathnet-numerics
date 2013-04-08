// <copyright file="SparseVector.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex32
{
    using Generic;
    using Numerics;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Threading;

    /// <summary>
    /// A vector with sparse storage, intended for very large vectors where most of the cells are zero.
    /// </summary>
    /// <remarks>The sparse vector is not thread safe.</remarks>
    [Serializable]
    [DebuggerDisplay("SparseVector {Count}-Complex32 {NonZerosCount}-NonZero")]
    public class SparseVector : Vector
    {
        readonly SparseVectorStorage<Complex32> _storage;

        /// <summary>
        /// Gets the number of non zero elements in the vector.
        /// </summary>
        /// <value>The number of non zero elements.</value>
        public int NonZerosCount
        {
            get { return _storage.ValueCount; }
        }

        /// <summary>
        /// Create a new sparse vector straight from an initialized vector storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public SparseVector(SparseVectorStorage<Complex32> storage)
            : base(storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Create a new sparse vector with the given length.
        /// All cells of the vector will be initialized to zero.
        /// Zero-length vectors are not supported.
        /// </summary>
        /// <exception cref="ArgumentException">If length is less than one.</exception>
        public SparseVector(int length)
            : this(new SparseVectorStorage<Complex32>(length))
        {
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static SparseVector OfVector(Vector<Complex32> vector)
        {
            return new SparseVector(SparseVectorStorage<Complex32>.OfVector(vector.Storage));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static SparseVector OfEnumerable(IEnumerable<Complex32> enumerable)
        {
            return new SparseVector(SparseVectorStorage<Complex32>.OfEnumerable(enumerable));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static SparseVector OfIndexedEnumerable(int length, IEnumerable<Tuple<int, Complex32>> enumerable)
        {
            return new SparseVector(SparseVectorStorage<Complex32>.OfIndexedEnumerable(length, enumerable));
        }

        /// <summary>
        /// Create a new sparse vector and initialize each value using the provided init function.
        /// </summary>
        public static SparseVector Create(int length, Func<int, Complex32> init)
        {
            return new SparseVector(SparseVectorStorage<Complex32>.OfInit(length, init));
        }

        /// <summary>
        /// Create a new sparse vector with the given length.
        /// All cells of the vector will be initialized with the provided value.
        /// Zero-length vectors are not supported.
        /// </summary>
        /// <exception cref="ArgumentException">If length is less than one.</exception>
        [Obsolete("Use a dense vector instead. Scheduled for removal in v3.0.")]
        public SparseVector(int length, Complex32 value)
            : this(SparseVectorStorage<Complex32>.OfInit(length, i => value))
        {
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        [Obsolete("Use SparseVector.OfVector instead. Scheduled for removal in v3.0.")]
        public SparseVector(Vector<Complex32> other)
            : this(SparseVectorStorage<Complex32>.OfVector(other.Storage))
        {
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        [Obsolete("Use SparseVector.OfEnumerable instead. Scheduled for removal in v3.0.")]
        public SparseVector(IEnumerable<Complex32> other)
            : this(SparseVectorStorage<Complex32>.OfEnumerable(other))
        {
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
        public override Matrix<Complex32> CreateMatrix(int rows, int columns)
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
        public override Vector<Complex32> CreateVector(int size)
        {
            return new SparseVector(size);
        }

        /// <summary>
        /// Conjugates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoConjugate(Vector<Complex32> result)
        {
            if (ReferenceEquals(this, result))
            {
                var tmp = CreateVector(Count);
                DoConjugate(tmp);
                tmp.CopyTo(result);
            }

            var targetSparse = result as SparseVector;
            if (targetSparse == null)
            {
                base.DoConjugate(result);
                return;
            }

            // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
            targetSparse._storage.Values = new Complex32[_storage.ValueCount];
            targetSparse._storage.Indices = new int[_storage.ValueCount];
            targetSparse._storage.ValueCount = _storage.ValueCount;

            if (_storage.ValueCount != 0)
            {
                CommonParallel.For(0, _storage.ValueCount, (a, b) =>
                    {
                        for (int i = a; i < b; i++)
                        {
                            targetSparse._storage.Values[i] = _storage.Values[i].Conjugate();
                        }
                    });
                Buffer.BlockCopy(_storage.Indices, 0, targetSparse._storage.Indices, 0, _storage.ValueCount*Constants.SizeOfInt);
            }
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
        protected override void DoAdd(Complex32 scalar, Vector<Complex32> result)
        {
            if (scalar == Complex32.Zero)
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
                var vnonZeroValues = new Complex32[Count];
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
        protected override void DoAdd(Vector<Complex32> other, Vector<Complex32> result)
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
                        if (!Complex32.Zero.Equals(otherValue))
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
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to subtract.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the subtraction.
        /// </param>
        protected override void DoSubtract(Complex32 scalar, Vector<Complex32> result)
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
        protected override void DoSubtract(Vector<Complex32> other, Vector<Complex32> result)
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
                        if (!Complex32.Zero.Equals(otherValue))
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
        /// Negates vector and saves result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoNegate(Vector<Complex32> result)
        {
            var sparseResult = result as SparseVector;
            if (sparseResult == null)
            {
                result.Clear();
                for (var index = 0; index < _storage.ValueCount; index++)
                {
                    result.At(_storage.Indices[index], -_storage.Values[index]);
                }
            }
            else
            {
                if (!ReferenceEquals(this, result))
                {
                    sparseResult._storage.ValueCount = _storage.ValueCount;
                    sparseResult._storage.Indices = new int[_storage.ValueCount];
                    Buffer.BlockCopy(_storage.Indices, 0, sparseResult._storage.Indices, 0, _storage.ValueCount * Constants.SizeOfInt);
                    sparseResult._storage.Values = new Complex32[_storage.ValueCount];
                    Array.Copy(_storage.Values, sparseResult._storage.Values, _storage.ValueCount);
                }

                Control.LinearAlgebraProvider.ScaleArray(-Complex32.One, sparseResult._storage.Values, sparseResult._storage.Values);
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
        protected override void DoMultiply(Complex32 scalar, Vector<Complex32> result)
        {
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
                    sparseResult._storage.Values = new Complex32[_storage.ValueCount];
                    Array.Copy(_storage.Values, sparseResult._storage.Values, _storage.ValueCount);
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
        protected override Complex32 DoDotProduct(Vector<Complex32> other)
        {
            var result = Complex32.Zero;

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
                throw new ArgumentNullException("leftSide");
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
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseVector)leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Multiplies a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The vector to scale.</param>
        /// <param name="rightSide">The complex value.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator *(SparseVector leftSide, Complex32 rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseVector)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The complex value.</param>
        /// <param name="rightSide">The vector to scale.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator *(Complex32 leftSide, SparseVector rightSide)
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
        public static Complex32 operator *(SparseVector leftSide, SparseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.DotProduct(rightSide);
        }

        /// <summary>
        /// Divides a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The vector to divide.</param>
        /// <param name="rightSide">The complex value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator /(SparseVector leftSide, Complex32 rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseVector)leftSide.Divide(rightSide);
        }

        /// <summary>
        /// Computes the modulus of each element of the vector of the given divisor.
        /// </summary>
        /// <param name="leftSide">The vector whose elements we want to compute the modulus of.</param>
        /// <param name="rightSide">The divisor to use,</param>
        /// <returns>The result of the calculation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator %(SparseVector leftSide, Complex32 rightSide)
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
            var min = _storage.Values[index].Magnitude;
            for (var i = 1; i < _storage.ValueCount; i++)
            {
                var test = _storage.Values[i].Magnitude;
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return _storage.Indices[index];
        }

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public override Complex32 Sum()
        {
            var result = Complex32.Zero;
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
        public override Complex32 SumMagnitudes()
        {
            var result = 0.0f;
            for (var i = 0; i < _storage.ValueCount; i++)
            {
                result += _storage.Values[i].Magnitude;
            }

            return result;
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Vector<Complex32> other, Vector<Complex32> result)
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
        protected override void DoPointwiseDivide(Vector<Complex32> other, Vector<Complex32> result)
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
        public static Matrix<Complex32> /*SparseMatrix*/ OuterProduct(SparseVector u, SparseVector v)
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
                    if (u._storage.Indices[i] == v._storage.Indices[j])
                    {
                        matrix.At(i, j, u._storage.Values[i] * v._storage.Values[j]);
                    }
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
        public Matrix<Complex32> OuterProduct(SparseVector v)
        {
            return OuterProduct(this, v);
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>Scalar <c>ret = (sum(abs(this[i])^p))^(1/p)</c></returns>
        public override Complex32 Norm(double p)
        {
            if (1 > p)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            if (_storage.ValueCount == 0)
            {
                return Complex32.Zero;
            }

            if (2.0 == p)
            {
                return _storage.Values.Aggregate(Complex32.Zero, SpecialFunctions.Hypotenuse).Magnitude;
            }

            if (Double.IsPositiveInfinity(p))
            {
                return CommonParallel.Aggregate(0, _storage.ValueCount, i => _storage.Values[i].Magnitude, Math.Max, 0f);
            }

            var sum = 0.0;
            for (var index = 0; index < _storage.ValueCount; index++)
            {
                sum += Math.Pow(_storage.Values[index].Magnitude, p);
            }

            return (float)Math.Pow(sum, 1.0 / p);
        }

        #region Parse Functions

        /// <summary>
        /// Creates a double sparse vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n,n,..', '(n,n,..)', '[n,n,...]', where n is a Complex32.
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
        /// quotes): 'n', 'n;n;..', '(n;n;..)', '[n;n;...]', where n is a Complex32.
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

            // parsing
            var strongTokens = value.Split(new[] { formatProvider.GetTextInfo().ListSeparator }, StringSplitOptions.RemoveEmptyEntries);
            var data = new List<Complex32>();
            foreach (string strongToken in strongTokens)
            {
                var weakTokens = strongToken.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string current = string.Empty;
                for (int i = 0; i < weakTokens.Length; i++)
                {
                    current += weakTokens[i];
                    if (current.EndsWith("+") || current.EndsWith("-") || current.StartsWith("(") && !current.EndsWith(")"))
                    {
                        continue;
                    }
                    var ahead = i < weakTokens.Length - 1 ? weakTokens[i + 1] : string.Empty;
                    if (ahead.StartsWith("+") || ahead.StartsWith("-"))
                    {
                        continue;
                    }
                    data.Add(current.ToComplex32(formatProvider));
                    current = string.Empty;
                }
                if (current != string.Empty)
                {
                    throw new FormatException();
                }
            }
            if (data.Count == 0)
            {
                throw new FormatException();
            }
            return OfEnumerable(data);
        }

        /// <summary>
        /// Converts the string representation of a complex sparse vector to double-precision sparse vector equivalent.
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
        public static bool TryParse(string value, out SparseVector result)
        {
            return TryParse(value, null, out result);
        }

        /// <summary>
        /// Converts the string representation of a complex sparse vector to double-precision sparse vector equivalent.
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
            return string.Format("SparseVector {0}-Complex32 {1:P2} Filled", Count, NonZerosCount / (double)Count);
        }
    }
}
