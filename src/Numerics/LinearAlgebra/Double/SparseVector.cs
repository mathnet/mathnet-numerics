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
    using Threading;

    /// <summary>
    /// A vector with sparse storage.
    /// </summary>
    /// <remarks>The sparse vector is not thread safe.</remarks>
    [Serializable]
    public class SparseVector : Vector
    {
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
        public SparseVector(Vector<double> other) : this(other.Count)
        {
            var vector = other as SparseVector;
            if (vector == null)
            {
                for (var i = 0; i < other.Count; i++)
                {
                    this[i] = other.At(i);
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
        public SparseVector(IList<double> array) : this(array.Count)
        {
            for (var i = 0; i < array.Count; i++)
            {
                this[i] = array[i];
            }
        }

        #endregion

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>This vector as a column matrix.</returns>
        public override Matrix<double> ToColumnMatrix()
        {
            var matrix = new SparseMatrix(Count, 1);
            for (var i = 0; i < NonZerosCount; i++)
            {
                matrix.At(_nonZeroIndices[i], 0, _nonZeroValues[i]);
            }

            return matrix;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>This vector as a row matrix.</returns>
        public override Matrix<double> ToRowMatrix()
        {
            var matrix = new SparseMatrix(1, Count);
            for (var i = 0; i < NonZerosCount; i++)
            {
                matrix.At(0, _nonZeroIndices[i], _nonZeroValues[i]);
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
        public override void CopyTo(Vector<double> target)
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
                target.Clear();

                for (var index = 0; index < NonZerosCount; index++)
                {
                    target.At(_nonZeroIndices[index], _nonZeroValues[index]);
                }
            }
            else
            {
                // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
                otherVector._nonZeroValues = new double[NonZerosCount];
                otherVector._nonZeroIndices = new int[NonZerosCount];
                otherVector.NonZerosCount = NonZerosCount;

                if (NonZerosCount != 0)
                {
                    Buffer.BlockCopy(_nonZeroValues, 0, otherVector._nonZeroValues, 0, NonZerosCount*Constants.SizeOfDouble);
                    Buffer.BlockCopy(_nonZeroIndices, 0, otherVector._nonZeroIndices, 0, NonZerosCount*Constants.SizeOfInt);
                }
            }
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
                for (int j = 0; j < NonZerosCount; j++)
                {
                    vnonZeroValues[_nonZeroIndices[j]] = _nonZeroValues[j] + scalar;
                }

                //assign this vectors arrary to the new arrays. 
                _nonZeroValues = vnonZeroValues;
                _nonZeroIndices = vnonZeroIndices;
                NonZerosCount = Count;
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

            if (ReferenceEquals(this, resultSparse))
            {
                int i = 0, j = 0;
                while (i < NonZerosCount || j < otherSparse.NonZerosCount)
                {
                    if (i < NonZerosCount && j < otherSparse.NonZerosCount && _nonZeroIndices[i] == otherSparse._nonZeroIndices[j])
                    {
                        _nonZeroValues[i++] += otherSparse._nonZeroValues[j++];
                    }
                    else if (j >= otherSparse.NonZerosCount || i < NonZerosCount && _nonZeroIndices[i] < otherSparse._nonZeroIndices[j])
                    {
                        i++;
                    }
                    else
                    {
                        var otherValue = otherSparse._nonZeroValues[j];
                        if (otherValue != 0.0)
                        {
                            InsertAtUnchecked(i++, otherSparse._nonZeroIndices[j], otherValue);
                        }
                        j++;
                    }
                }
            }
            else
            {
                result.Clear();
                int i = 0, j = 0, last = -1;
                while (i < NonZerosCount || j < otherSparse.NonZerosCount)
                {
                    if (j >= otherSparse.NonZerosCount || i < NonZerosCount && _nonZeroIndices[i] <= otherSparse._nonZeroIndices[j])
                    {
                        var next = _nonZeroIndices[i];
                        if (next != last)
                        {
                            last = next;
                            result.At(next, _nonZeroValues[i] + otherSparse.At(next));
                        }
                        i++;
                    }
                    else
                    {
                        var next = otherSparse._nonZeroIndices[j];
                        if (next != last)
                        {
                            last = next;
                            result.At(next, At(next) + otherSparse._nonZeroValues[j]);
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

            if (ReferenceEquals(this, resultSparse))
            {
                int i = 0, j = 0;
                while (i < NonZerosCount || j < otherSparse.NonZerosCount)
                {
                    if (i < NonZerosCount && j < otherSparse.NonZerosCount && _nonZeroIndices[i] == otherSparse._nonZeroIndices[j])
                    {
                        _nonZeroValues[i++] -= otherSparse._nonZeroValues[j++];
                    }
                    else if (j >= otherSparse.NonZerosCount || i < NonZerosCount && _nonZeroIndices[i] < otherSparse._nonZeroIndices[j])
                    {
                        i++;
                    }
                    else
                    {
                        var otherValue = otherSparse._nonZeroValues[j];
                        if (otherValue != 0.0)
                        {
                            InsertAtUnchecked(i++, otherSparse._nonZeroIndices[j], -otherValue);
                        }
                        j++;
                    }
                }
            }
            else
            {
                result.Clear();
                int i = 0, j = 0, last = -1;
                while (i < NonZerosCount || j < otherSparse.NonZerosCount)
                {
                    if (j >= otherSparse.NonZerosCount || i < NonZerosCount && _nonZeroIndices[i] <= otherSparse._nonZeroIndices[j])
                    {
                        var next = _nonZeroIndices[i];
                        if (next != last)
                        {
                            last = next;
                            result.At(next, _nonZeroValues[i] - otherSparse.At(next));
                        }
                        i++;
                    }
                    else
                    {
                        var next = otherSparse._nonZeroIndices[j];
                        if (next != last)
                        {
                            last = next;
                            result.At(next, At(next) - otherSparse._nonZeroValues[j]);
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
            var result = new SparseVector(Count)
                         {
                             _nonZeroValues = new double[NonZerosCount], 
                             _nonZeroIndices = new int[NonZerosCount], 
                             NonZerosCount = NonZerosCount
                         };

            if (NonZerosCount != 0)
            {
                CommonParallel.For(
                    0,
                    NonZerosCount,
                    index => result._nonZeroValues[index] = -_nonZeroValues[index]);
                    
                Buffer.BlockCopy(_nonZeroIndices, 0, result._nonZeroIndices, 0, NonZerosCount * Constants.SizeOfInt);
            }

            return result;
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
            Control.LinearAlgebraProvider.ScaleArray(scalar, copy._nonZeroValues, copy._nonZeroValues);
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
                for (var index = 0; index < NonZerosCount; index++)
                {
                    result.At(_nonZeroIndices[index], scalar * _nonZeroValues[index]);
                }
            }
            else
            {
                if (!ReferenceEquals(this, result))
                {
                    sparseResult.NonZerosCount = NonZerosCount;
                    sparseResult._nonZeroIndices = new int[NonZerosCount];
                    Buffer.BlockCopy(_nonZeroIndices, 0, sparseResult._nonZeroIndices, 0, NonZerosCount * Constants.SizeOfInt);
                    sparseResult._nonZeroValues = new double[NonZerosCount];
                    Buffer.BlockCopy(_nonZeroValues, 0, sparseResult._nonZeroValues, 0, NonZerosCount * Constants.SizeOfDouble);
                }

                Control.LinearAlgebraProvider.ScaleArray(scalar, sparseResult._nonZeroValues, sparseResult._nonZeroValues);
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
                for (var i = 0; i < NonZerosCount; i++)
                {
                    result += _nonZeroValues[i] * _nonZeroValues[i];
                }
            }
            else
            {
                for (var i = 0; i < NonZerosCount; i++)
                {
                    result += _nonZeroValues[i] * other.At(_nonZeroIndices[i]);
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
                for (var index = 0; index < NonZerosCount; index++)
                {
                    _nonZeroValues[index] %= divisor;
                }
            }
            else
            {
                result.Clear();
                for (var index = 0; index < NonZerosCount; index++)
                {
                    result.At(_nonZeroIndices[index], _nonZeroValues[index] % divisor);
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
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Vector<double> other, Vector<double> result)
        {
            if (ReferenceEquals(this, other))
            {
                for (var i = 0; i < NonZerosCount; i++)
                {
                    _nonZeroValues[i] *= _nonZeroValues[i]; 
                }
            }
            else
            {
                for (var i = 0; i < NonZerosCount; i++)
                {
                    var index = _nonZeroIndices[i];
                    result.At(index, other.At(index) * _nonZeroValues[i]);
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
                for (var i = 0; i < NonZerosCount; i++)
                {
                    _nonZeroValues[i] /= _nonZeroValues[i];
                }
            }
            else
            {
                for (var i = 0; i < NonZerosCount; i++)
                {
                    var index = _nonZeroIndices[i];
                    result.At(index, _nonZeroValues[i] / other.At(index));
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
            for (var i = 0; i < u.NonZerosCount; i++)
            {
                for (var j = 0; j < v.NonZerosCount; j++)
                {
                    matrix.At(i, j, u._nonZeroValues[i] * v._nonZeroValues[j]);
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

            if (NonZerosCount == 0)
            {
                return 0.0;
            }

            if (2.0 == p)
            {
                return _nonZeroValues.Aggregate(0.0, SpecialFunctions.Hypotenuse);
            }

            if (Double.IsPositiveInfinity(p))
            {
                return CommonParallel.Aggregate(0, NonZerosCount, i => Math.Abs(_nonZeroValues[i]), Math.Max, 0d);
            }

            var sum = 0.0;
            for (var index = 0; index < NonZerosCount; index++)
            {
                sum += Math.Pow(Math.Abs(_nonZeroValues[index]), p);
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

        /// <summary>
        /// Gets the value at the given index.
        /// </summary>
        /// <param name="index">Value real index in array</param>
        /// <returns>The value at the given index.</returns>
        internal protected override double At(int index)
        {
            // Search if item idex exists in NonZeroIndices array in range "0 - real nonzero values count"
            var itemIndex = Array.BinarySearch(_nonZeroIndices, 0, NonZerosCount, index);
            return itemIndex >= 0 ? _nonZeroValues[itemIndex] : 0.0;
        }
        
        /// <summary>
        /// Delete, Add or Update the value in NonZeroValues and NonZeroIndices
        /// </summary>
        /// <param name="index">Value real index in array</param>
        /// <param name="value">The value to set.</param>
        /// <remarks>This method assume that index is between 0 and Array Size</remarks>
        internal protected override void At(int index, double value)
        {
            // Search if "index" already exists in range "0 - real nonzero values count"
            var itemIndex = Array.BinarySearch(_nonZeroIndices, 0, NonZerosCount, index);

            if (itemIndex >= 0)
            {
                // Item already exist at itemIndex
                if (value == 0.0)
                {
                    RemoveAtUnchecked(itemIndex);
                }
                else
                {
                    _nonZeroValues[itemIndex] = value;
                }
            }
            else
            {
                if (value != 0.0)
                {
                    InsertAtUnchecked(~itemIndex, index, value);
                }
            }
        }

        private void InsertAtUnchecked(int itemIndex, int index, double value)
        {
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
            // Move all values (with an position larger than index) in the columIndices
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

        private void RemoveAtUnchecked(int itemIndex)
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

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            if (Count > 20)
            {
                return String.Format("SparseVectorOfDouble({0},{1},{2})", Count, NonZerosCount, GetHashCode());
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
            var hashNum = Math.Min(NonZerosCount, 20);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
#if PORTABLE
                hash ^= Precision.DoubleToInt64Bits(this._nonZeroValues[i].GetHashCode());
#else
                hash ^= BitConverter.DoubleToInt64Bits(_nonZeroValues[i].GetHashCode());
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
            while (i < NonZerosCount || j < otherSparse.NonZerosCount)
            {
                if (j >= otherSparse.NonZerosCount || i < NonZerosCount && _nonZeroIndices[i] < otherSparse._nonZeroIndices[j])
                {
                    if (_nonZeroValues[i++] != 0d)
                    {
                        return false;
                    }
                    continue;
                }

                if (i >= NonZerosCount || j < otherSparse.NonZerosCount && otherSparse._nonZeroIndices[j] < _nonZeroIndices[i])
                {
                    if (otherSparse._nonZeroValues[j++] != 0d)
                    {
                        return false;
                    }
                    continue;
                }

                if (!_nonZeroValues[i].AlmostEqual(otherSparse._nonZeroValues[j]))
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
            for (var i = 0; i < NonZerosCount; i++)
            {
                yield return new Tuple<int, double>(_nonZeroIndices[i], _nonZeroValues[i]);
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
            for (var i = 0; i < NonZerosCount; i++)
            {
                ret[_nonZeroIndices[i]] = _nonZeroValues[i];
            }

            return ret;
        }
    }
}
