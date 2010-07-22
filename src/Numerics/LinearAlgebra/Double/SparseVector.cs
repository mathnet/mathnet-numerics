// <copyright file="SparseVector.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Distributions;
    using NumberTheory;
    using Properties;
    using Threading;

    /// <summary>
    /// A vector with sparse storage.
    /// </summary>
    public class SparseVector : Vector
    {
        /// <summary>
        /// Lock object for the indexer.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        ///  Gets the vector's internal data. The array containing the actual values; only the non-zero values are stored.
        /// </summary>
        private double[] _nonZeroValues = new double[0];

        /// <summary>
        /// The indices of the non-zero entries.
        /// </summary>
        private int[] _nonZeroIndices = new int[0];

        /// <summary>
        /// Gets the number of non zero elements in the vector.
        /// </summary>
        /// <value>The number of non zero elements.</value>
        public int NonZerosCount
        {
            get;
            private set;
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
        public SparseVector(int size) : base(size)
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
        public SparseVector(int size, double value) : this(size)
        {
            if (value == 0.0)
            {
                // Skip adding values 
                return;
            }

            // We already know that this vector is "full", let's allocate all needed memory
            _nonZeroValues = new double[size];
            _nonZeroIndices = new int[size];
            NonZerosCount = size;

            CommonParallel.For(
                0, 
                Count, 
                index =>
                {
                    _nonZeroValues[index] = value;
                    _nonZeroIndices[index] = index;
                });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector"/> class by
        /// copying the values from another.
        /// </summary>
        /// <param name="other">
        /// The vector to create the new vector from.
        /// </param>
        public SparseVector(Vector other) : this(other.Count)
        {
            var vector = other as SparseVector;
            if (vector == null)
            {
                for (var i = 0; i < other.Count; i++)
                {
                    this[i] = other[i];
                }
            }
            else
            {
                _nonZeroValues = new double[vector.NonZerosCount];
                _nonZeroIndices = new int[vector.NonZerosCount];
                NonZerosCount = vector.NonZerosCount;

                // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
                Buffer.BlockCopy(vector._nonZeroValues, 0, _nonZeroValues, 0, vector.NonZerosCount * Constants.SizeOfDouble);
                Buffer.BlockCopy(vector._nonZeroIndices, 0, _nonZeroIndices, 0, vector.NonZerosCount * Constants.SizeOfInt);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector"/> class by
        /// copying the values from another.
        /// </summary>
        /// <param name="other">
        /// The vector to create the new vector from.
        /// </param>
        public SparseVector(SparseVector other) : this(other.Count)
        {
            // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
            _nonZeroValues = new double[other.NonZerosCount];
            _nonZeroIndices = new int[other.NonZerosCount];
            NonZerosCount = other.NonZerosCount;

            Buffer.BlockCopy(other._nonZeroValues, 0, _nonZeroValues, 0, other.NonZerosCount * Constants.SizeOfDouble);
            Buffer.BlockCopy(other._nonZeroIndices, 0, _nonZeroIndices, 0, other.NonZerosCount * Constants.SizeOfInt);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector"/> class for an array.
        /// </summary>
        /// <param name="array">The array to create this vector from.</param>
        /// <remarks>The vector copy the array. Any changes to the vector will NOT change the array.</remarks>
        public SparseVector(double[] array) : this(array.Length)
        {
            for (var i = 0; i < array.Length; i++)
            {
                this[i] = array[i];
            }
        }

        #endregion

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>This vector as a column matrix.</returns>
        public override Matrix ToColumnMatrix()
        {
            var matrix = new SparseMatrix(Count, 1);
            for (var i = 0; i < NonZerosCount; i++)
            {
                matrix[_nonZeroIndices[i], 0] = _nonZeroValues[i];
            }

            return matrix;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>This vector as a row matrix.</returns>
        public override Matrix ToRowMatrix()
        {
            var matrix = new SparseMatrix(1, Count);
            for (var i = 0; i < NonZerosCount; i++)
            {
                matrix[0, _nonZeroIndices[i]] = _nonZeroValues[i];
            }

            return matrix;
        }

        /// <summary>Gets or sets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns> 
        /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is negative or 
        /// greater than the size of the vector.</exception>
        public override double this[int index]
        {
            get
            {
                // If index is out of bounds
                if ((index < 0) || (index >= Count))
                {
                    throw new IndexOutOfRangeException();
                }

                lock (_lockObject)
                {
                    // Search if item idex exists in NonZeroIndices array in range "0 - real nonzero values count"
                    var itemIndex = Array.BinarySearch(_nonZeroIndices, 0, NonZerosCount, index);
                    if (itemIndex >= 0)
                    {
                        return _nonZeroValues[itemIndex];
                    }
                }

                return 0.0;
            }

            set
            {
                // If index is out of bounds
                if ((index < 0) || (index >= Count))
                {
                    throw new IndexOutOfRangeException();
                }

                lock (_lockObject)
                {
                    SetValue(index, value);
                }
            }
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
        public override Matrix CreateMatrix(int rows, int columns)
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
        public override Vector CreateVector(int size)
        {
            return new SparseVector(size);
        }

        /// <summary>
        /// Resets all values to zero.
        /// </summary>
        public override void Clear()
        {
            NonZerosCount = 0;
        }

        /// <summary>
        /// Copies the values of this vector into the target vector.
        /// </summary>
        /// <param name="target">
        /// The vector to copy elements into.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="target"/> is not the same size as this vector.
        /// </exception>
        public override void CopyTo(Vector target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (Count != target.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "target");
            }

            if (ReferenceEquals(this, target))
            {
                return;
            }

            var otherVector = target as SparseVector;
            if (otherVector == null)
            {
                CommonParallel.For(
                    0, 
                    Count, 
                    index => target[index] = this[index]);
            }
            else
            {
                // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
                otherVector._nonZeroValues = new double[NonZerosCount];
                otherVector._nonZeroIndices = new int[NonZerosCount];
                otherVector.NonZerosCount = NonZerosCount;

                Buffer.BlockCopy(_nonZeroValues, 0, otherVector._nonZeroValues, 0, NonZerosCount * Constants.SizeOfDouble);
                Buffer.BlockCopy(_nonZeroIndices, 0, otherVector._nonZeroIndices, 0, NonZerosCount * Constants.SizeOfInt);
            }
        }

        #region Operators and supplementary functions

        /// <summary>
        /// Adds a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <returns>A copy of the vector with the scalar added.</returns>
        public override Vector Add(double scalar)
        {
            if (scalar == 0.0)
            {
                return this.Clone();
            }

            var copy = (SparseVector)this.Clone();
            for (var i = 0; i < Count; i++)
            {
                copy[i] += scalar;
            }
            return copy;
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void Add(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            CopyTo(result);
            result.Add(scalar);
        }

        /// <summary>
        /// Adds another vector to this vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <returns>A new vector containing the sum of both vectors.</returns>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public override Vector Add(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var sparseVector = other as SparseVector;

            if (sparseVector == null)
            {
                return base.Add(other);
            }
            else
            {
                var copy = (SparseVector)this.Clone();
                copy.AddScaledSparseVector(1.0, sparseVector);
                return copy;
            }
        }

        /// <summary>
        /// Adds the scaled sparse vector.
        /// </summary>
        /// <param name="alpha">The alpha.</param>
        /// <param name="other">The other.</param>
        private void AddScaledSparseVector(double alpha, SparseVector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (alpha == 0.0)
            {
                return;
            }

            // I don't use ILinearAlgebraProvider because we will get no benefit due to "lock" in this[index]
            // Possible fucniton in ILinearAlgebraProvider may be AddSparseVectorToScaledSparseVector(T[] y, int[] yIndices, T alpha, T[] x, int[] xIndices); 
            // But it require to develop value setting algorithm and due to "lock" it will be even more greedy then implemented below
            if (ReferenceEquals(this, other))
            {
                // Adding the same instance of sparse vector. That means if we modify "this" then "other" will be modified too.
                // To avoid such problem lets change values in internal storage of "this"
                if (alpha == 1.0)
                {
                    for (var i = 0; i < NonZerosCount; i++)
                    {
                        _nonZeroValues[i] += _nonZeroValues[i];
                    }
                }
                else if (alpha == -1.0)
                {
                    Clear(); // Vector is subtracted from itself
                    return;
                }
                else
                {
                    for (var i = 0; i < NonZerosCount; i++)
                    {
                        _nonZeroValues[i] += alpha * _nonZeroValues[i];
                    }
                }
            }
            else
            {
                // "this" and "other" are different objects, so by modifying "this" the "other" object will not be changed
                if (alpha == 1.0)
                {
                    for (var i = 0; i < other.NonZerosCount; i++)
                    {
                        this[other._nonZeroIndices[i]] += other._nonZeroValues[i];
                    }
                }
                else
                {
                    for (var i = 0; i < other.NonZerosCount; i++)
                    {
                        this[other._nonZeroIndices[i]] += alpha * other._nonZeroValues[i];
                    }
                }
            }
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void Add(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = this.Add(other);
                tmp.CopyTo(result);
            }
            else
            {
                var sparse = result as SparseVector;
                if (sparse == null)
                {
                    base.Add(other, result);
                }
                else
                {
                    var sparseother = other as SparseVector;
                    if (sparseother == null)
                    {
                        sparse.AddScaledSparseVector(1.0, sparseother);
                    }
                    else
                    {
                        base.Add(other, result);
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
        public static Vector operator +(SparseVector rightSide)
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
        public static Vector operator +(SparseVector leftSide, SparseVector rightSide)
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

            var ret = leftSide.Clone();
            ret.Add(rightSide);
            return ret;
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <returns>A new vector containing the subtraction of this vector and the scalar.</returns>
        public override Vector Subtract(double scalar)
        {
            if (scalar == 0.0)
            {
                return this.Clone();
            }

            var copy = (SparseVector)this.Clone();
            for (var i = 0; i < Count; i++)
            {
                copy[i] -= scalar;
            }
            return copy;
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void Subtract(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            CopyTo(result);
            result.Subtract(scalar);
        }

        /// <summary>
        /// Subtracts another vector from this vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <returns>A new vector containing the subtraction of the the two vectors.</returns>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public override Vector Subtract(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var sparseVector = other as SparseVector;

            if (sparseVector == null)
            {
                return base.Subtract(other);
            }
            else
            {
                var copy = (SparseVector)this.Clone();
                copy.AddScaledSparseVector(-1.0, sparseVector);
                return copy;
            }
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void Subtract(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = this.Subtract(other);
                tmp.CopyTo(result);
            }
            else
            {
                var sparse = result as SparseVector;
                if (sparse == null)
                {
                    base.Subtract(other, result);
                }
                else
                {
                    var sparseother = other as SparseVector;
                    if (sparseother == null)
                    {
                        sparse.AddScaledSparseVector(-1.0, sparseother);
                    }
                    else
                    {
                        base.Subtract(other, result);
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
        public static Vector operator -(SparseVector rightSide)
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
        public static Vector operator -(SparseVector leftSide, SparseVector rightSide)
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

            var ret = leftSide.Clone();
            ret.Subtract(rightSide);
            return ret;
        }

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        /// <returns>The negated vector.</returns>
        /// <remarks>Added as an alternative to the unary negation operator.</remarks>
        public override Vector Negate()
        {
            var result = new SparseVector(Count)
                         {
                             _nonZeroValues = new double[NonZerosCount], 
                             _nonZeroIndices = new int[NonZerosCount], 
                             NonZerosCount = NonZerosCount
                         };

            Buffer.BlockCopy(_nonZeroIndices, 0, result._nonZeroIndices, 0, NonZerosCount * Constants.SizeOfInt);

            CommonParallel.For(
                0, 
                NonZerosCount, 
                index => result._nonZeroValues[index] = -_nonZeroValues[index]);

            return result;
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <returns>A new vector that is the multiplication of the vector and the scalar.</returns>
        public override Vector Multiply(double scalar)
        {
            if (scalar == 1.0)
            {
                return this.Clone();
            }
            else if (scalar == 0)
            {
                var copy = this.Clone();
                copy.Clear(); // Set array empty
                return copy;
            }
            else
            {

                var copy = (SparseVector)this.Clone();
                Control.LinearAlgebraProvider.ScaleArray(scalar, copy._nonZeroValues);
                return copy;
            }
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentException">If <paramref name="other"/> is not of the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="other"/> is <see langword="null" />.</exception>
        public override double DotProduct(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            double result = 0;

            // base implementation iterates though all elements, but we need only take non-zeros 
            for (var i = 0; i < NonZerosCount; i++)
            {
                result += _nonZeroValues[i] * other[_nonZeroIndices[i]];
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

            var ret = (SparseVector)leftSide.Clone();
            ret.Multiply(rightSide);
            return ret;
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

            var ret = (SparseVector)rightSide.Clone();
            ret.Multiply(leftSide);
            return ret;
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

            var ret = (SparseVector)leftSide.Clone();
            ret.Multiply(1.0 / rightSide);
            return ret;
        }

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>   
        public override int AbsoluteMinimumIndex()
        {
            if (NonZerosCount == 0)
            {
// No non-zero elements. Return 0
                return 0;
            }

            var index = 0;
            var min = Math.Abs(_nonZeroValues[index]);
            for (var i = 1; i < NonZerosCount; i++)
            {
                var test = Math.Abs(_nonZeroValues[i]);
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return _nonZeroIndices[index];
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
        public override Vector SubVector(int index, int length)
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
                result[i - index] = this[i];
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
                this[i] = values[i];
            }
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>          
        public override int MaximumIndex()
        {
            if (NonZerosCount == 0)
            {
                return 0;
            }

            var index = 0;
            var max = _nonZeroValues[0];
            for (var i = 1; i < NonZerosCount; i++)
            {
                if (max < _nonZeroValues[i])
                {
                    index = i;
                    max = _nonZeroValues[i];
                }
            }

            return _nonZeroIndices[index];
        }

        /// <summary>
        /// Returns the index of the minimum element.
        /// </summary>
        /// <returns>The index of minimum element.</returns>  
        public override int MinimumIndex()
        {
            if (NonZerosCount == 0)
            {
                return 0;
            }

            var index = 0;
            var min = _nonZeroValues[0];
            for (var i = 1; i < NonZerosCount; i++)
            {
                if (min > _nonZeroValues[i])
                {
                    index = i;
                    min = _nonZeroValues[i];
                }
            }

            return _nonZeroIndices[index];
        }

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public override double Sum()
        {
            double result = 0;
            for (var i = 0; i < NonZerosCount; i++)
            {
                result += _nonZeroValues[i];
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
            for (var i = 0; i < NonZerosCount; i++)
            {
                result += Math.Abs(_nonZeroValues[i]);
            }

            return result;
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <returns>A new vector which is the pointwise multiplication of the two vectors.</returns>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public override Vector PointwiseMultiply(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            // We cannot iterate using NonZeroCount because the value may be changed (if multiply by 0)
            var copy = (SparseVector)this.Clone();
            for (var i = 0; i < Count; i++)
            {
                copy[i] *= other[i];
            }
            return copy;
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void PointwiseMultiply(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateVector(result.Count);
                PointwiseMultiply(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                CopyTo(result);
                result.PointwiseMultiply(other);
            }
        }

        /// <summary>
        /// Pointwise divide this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <returns>A new vector which is the pointwise division of the two vectors.</returns>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public override Vector PointwiseDivide(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            // base implementation iterates though all elements, but we need only take non-zeros
            var copy = (SparseVector)this.Clone();
            for (var i = 0; i < NonZerosCount; i++)
            {
                copy[_nonZeroIndices[i]] /= other[_nonZeroIndices[i]];
            }
            return copy;
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void PointwiseDivide(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateVector(result.Count);
                PointwiseDivide(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                CopyTo(result);
                result.PointwiseDivide(other);
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
        public static Matrix /*SparseMatrix*/ OuterProduct(SparseVector u, SparseVector v)
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
            for (var i = 0; i < u.NonZerosCount; i++)
            {
                for (var j = 0; j < v.NonZerosCount; j++)
                {
                    if (u._nonZeroIndices[i] == v._nonZeroIndices[j])
                    {
                        matrix.At(i, j, u._nonZeroValues[i] * v._nonZeroValues[j]);
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// Generates a vector with random elements
        /// </summary>
        /// <param name="length">Number of elements in the vector.</param>
        /// <param name="randomDistribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// A vector with n-random elements distributed according
        /// to the specified random distribution.
        /// </returns>
        /// <exception cref="ArgumentNullException">If the length vector is non positive<see langword="null" />.</exception> 
        public override Vector Random(int length, IContinuousDistribution randomDistribution)
        {
            if (length < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            var v = (SparseVector)CreateVector(length);
            for (var index = 0; index < v.Count; index++)
            {
                v[index] = randomDistribution.Sample();
            }

            return v;
        }

        /// <summary>
        /// Generates a vector with random elements
        /// </summary>
        /// <param name="length">Number of elements in the vector.</param>
        /// <param name="randomDistribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// A vector with n-random elements distributed according
        /// to the specified random distribution.
        /// </returns>
        /// <exception cref="ArgumentNullException">If the n vector is non positive<see langword="null" />.</exception> 
        public override Vector Random(int length, IDiscreteDistribution randomDistribution)
        {
            if (length < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            var v = (SparseVector)CreateVector(length);
            for (var index = 0; index < v.Count; index++)
            {
                v[index] = randomDistribution.Sample();
            }

            return v;
        }

        /// <summary>
        /// Tensor Product (Outer) of this and another vector.
        /// </summary>
        /// <param name="v">The vector to operate on.</param>
        /// <returns>
        /// Matrix M[i,j] = this[i] * v[j].
        /// </returns>
        /// <seealso cref="OuterProduct"/>
        public Matrix TensorMultiply(SparseVector v)
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
            else if (Double.IsPositiveInfinity(p))
            {
                return CommonParallel.Select(0, NonZerosCount, (index, localData) => localData = Math.Max(localData, Math.Abs(_nonZeroValues[index])), Math.Max);
            }
            else
            {
                var sum = CommonParallel.Aggregate(
                    0,
                    NonZerosCount,
                    index => Math.Pow(Math.Abs(_nonZeroValues[index]), p));

                return Math.Pow(sum, 1.0 / p);
            }
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
                throw new ArgumentNullException(value);
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

        /// <summary>
        /// Delete, Add or Update the value in NonZeroValues and NonZeroIndices
        /// </summary>
        /// <param name="index">Value real index in array</param>
        /// <param name="value">The value to set.</param>
        /// <remarks>This method assume that index is between 0 and Array Size</remarks>
        private void SetValue(int index, double value)
        {
            // Search if "index" already exists in range "0 - real nonzero values count"
            var itemIndex = Array.BinarySearch(_nonZeroIndices, 0, NonZerosCount, index);

            if (itemIndex >= 0)
            {
                // Item already exist at itemIndex
                if (value == 0.0)
                {
                    // Value is zero. Let's delete it from Values and Indices array
                    for (var i = itemIndex + 1; i < NonZerosCount; i++)
                    {
                        _nonZeroValues[i - 1] = _nonZeroValues[i];
                        _nonZeroIndices[i - 1] = _nonZeroIndices[i];
                    }

                    NonZerosCount -= 1;

                    // Check if the storage needs to be shrink. This is reasonable to do if 
                    // there are a lot of non-zero elements and storage is two times bigger
                    if ((NonZerosCount > 1024) && (NonZerosCount < _nonZeroIndices.Length / 2))
                    {
                        Array.Resize(ref _nonZeroValues, NonZerosCount);
                        Array.Resize(ref _nonZeroIndices, NonZerosCount);
                    }
                }
                else
                {
                    _nonZeroValues[itemIndex] = value;
                }
            }
            else
            {
                if (value == 0.0)
                {
                    return;
                }

                itemIndex = ~itemIndex; // Index where to put new value

                // Check if the storage needs to be increased
                if ((NonZerosCount == _nonZeroValues.Length) && (NonZerosCount < Count))
                {
                    // Value and Indices arrays are completely full so we increase the size
                    var size = Math.Min(_nonZeroValues.Length + GrowthSize(), Count);
                    Array.Resize(ref _nonZeroValues, size);
                    Array.Resize(ref _nonZeroIndices, size);
                }

                // Move all values (with an position larger than index) in the value array 
                // to the next position
                // move all values (with an position larger than index) in the columIndices 
                // array to the next position
                for (var i = NonZerosCount - 1; i > itemIndex - 1; i--)
                {
                    _nonZeroValues[i + 1] = _nonZeroValues[i];
                    _nonZeroIndices[i + 1] = _nonZeroIndices[i];
                }

                // Add the value and the column index
                _nonZeroValues[itemIndex] = value;
                _nonZeroIndices[itemIndex] = index;

                // increase the number of non-zero numbers by one
                NonZerosCount += 1;
            }
        }

        /// <summary>
        /// Calculates the amount with which to grow the storage array's if they need to be
        /// increased in size.
        /// </summary>
        /// <returns>The amount grown.</returns>
        private int GrowthSize()
        {
            int delta;
            if (_nonZeroValues.Length > 1024)
            {
                delta = _nonZeroValues.Length / 4;
            }
            else
            {
                if (_nonZeroValues.Length > 256)
                {
                    delta = 512;
                }
                else
                {
                    delta = _nonZeroValues.Length > 64 ? 128 : 32;
                }
            }

            return delta;
        }

        #region System.Object override

        /// <summary>
        /// Check equality. If this is regular vector, then check by base implementation. If Sparse - use own method.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var sparseVector = obj as SparseVector;

            if (sparseVector == null)
            {
                return base.Equals(obj);
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, sparseVector))
            {
                return true;
            }

            if ((Count != sparseVector.Count) || (NonZerosCount != sparseVector.NonZerosCount))
            {
                return false;
            }

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < NonZerosCount; index++)
            {
                if (!_nonZeroValues[index].AlmostEqual(sparseVector._nonZeroValues[index]) || (_nonZeroIndices[index] != sparseVector._nonZeroIndices[index]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(NonZerosCount, 20);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
#if SILVERLIGHT
                hash ^= Precision.DoubleToInt64Bits(this._nonZeroValues[i]);
#else
                hash ^= BitConverter.DoubleToInt64Bits(_nonZeroValues[i]);
#endif
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        #endregion
    }
}
