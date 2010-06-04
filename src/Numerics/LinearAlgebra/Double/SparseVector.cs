// <copyright file="SparseVector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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
    using NumberTheory;
    using Properties;
    using Threading;

    public class SparseVector : Vector
    {
        /// <summary>
        ///  Gets the vector's internal data. The array containing the actual values; only the non-zero values are stored.
        /// </summary>
        private double[] NonZeroValues = new double[0];

        /// <summary>
        /// The indices of the non-zero entries.
        /// </summary>
        private int[] NonZeroIndices = new int[0];
        /// <summary>
        /// Returns the number of non zero elements in the vector.
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
        public SparseVector(int size) : base(size) { }

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
            if (value == 0.0) //Skip adding values 
                return;

            // We already know that this vector is "full", let's allocate all needed memory
            NonZeroValues = new double[size];
            NonZeroIndices = new int[size];
            NonZerosCount = size;

            CommonParallel.For(
                0,
                this.Count,
                index =>
                {
                    NonZeroValues[index] = value;
                    NonZeroIndices[index] = index;
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
                for (int i = 0; i < other.Count; i++ )
                    this[i] = other[i]; 
            }
            else
            {
                NonZeroValues = new double[vector.NonZerosCount];
                NonZeroIndices = new int[vector.NonZerosCount];
                NonZerosCount = vector.NonZerosCount;

                // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
                Buffer.BlockCopy(vector.NonZeroValues, 0, this.NonZeroValues, 0, vector.NonZerosCount * Constants.SizeOfDouble);
                Buffer.BlockCopy(vector.NonZeroIndices, 0, this.NonZeroIndices, 0, vector.NonZerosCount * Constants.SizeOfInt);
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
            NonZeroValues = new double[other.NonZerosCount];
            NonZeroIndices = new int[other.NonZerosCount];
            NonZerosCount = other.NonZerosCount;

            Buffer.BlockCopy(other.NonZeroValues, 0, this.NonZeroValues, 0, other.NonZerosCount * Constants.SizeOfDouble);
            Buffer.BlockCopy(other.NonZeroIndices, 0, this.NonZeroIndices, 0, other.NonZerosCount * Constants.SizeOfInt);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVector"/> class for an array.
        /// </summary>
        /// <param name="array">The array to create this vector from.</param>
        /// <remarks>The vector copy the array. Any changes to the vector will NOT change the array.</remarks>
        public SparseVector(double[] array) : this(array.Length)
        {
            for (int i = 0; i < array.Length; i++ )
                this[i] = array[i]; 
        }
        #endregion

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>This vector as a column matrix.</returns>
        public override Matrix ToColumnMatrix()
        {
            throw new NotImplementedException();
            //var matrix = new SparseMatrix(this.Count, 1);
            //CommonParallel.For(
            //         0,
            //         this.Count,
            //         index => matrix[i, 0] = vector[index]); 
            //return matrix;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>This vector as a row matrix.</returns>
        public override Matrix ToRowMatrix()
        {
            throw new NotImplementedException();
            //var matrix = new SparseMatrix(1, this.Count);
            //CommonParallel.For(
            //         0,
            //         this.Count,
            //         index => matrix[0, i] = vector[index]); 
            //return matrix;
        }

        private readonly object lockObject = new object();
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

                lock (lockObject)
                {
                    // Search if item idex exists in NonZeroIndices array in range "0 - real nonzero values count"
                    int itemIndex = Array.BinarySearch(NonZeroIndices, 0, NonZerosCount, index);
                    if (itemIndex >= 0)
                        return NonZeroValues[itemIndex];
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

                lock (lockObject)
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
            throw new NotImplementedException();
            //return new SparseMatrix(rows, columns);
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

            if (this.Count != target.Count)
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
                    this.Count,
                    index => target[index] = this[index]);
            }
            else
            {
                // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
                otherVector.NonZeroValues = new double[this.NonZerosCount];
                otherVector.NonZeroIndices = new int[this.NonZerosCount];
                otherVector.NonZerosCount = this.NonZerosCount;

                Buffer.BlockCopy(this.NonZeroValues, 0, otherVector.NonZeroValues, 0, this.NonZerosCount * Constants.SizeOfDouble);
                Buffer.BlockCopy(this.NonZeroIndices, 0, otherVector.NonZeroIndices, 0, this.NonZerosCount * Constants.SizeOfInt);
            }
        }

        #region Operators and supplementary functions
        
        // NOTE: There are no operators as:
        // public static implicit operator SparseVector(double[] array)
        // and
        // public static implicit operator double[](SparseVector vector)
        // as it is in DenseVector. Because when creating vector from double[] values are copied to internal storage and if user wants
        // to get double[] he should call SparseVector.ToArray(), then double[] will be generated and returned to a user.\

        // In DenseVector implementation reference to double[] is assigned to interanl storage when casting from double[] and returned 
        // when casting to double[] 

        /// <summary>
        /// Adds a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        public override void Add(double scalar)
        {
            if (scalar == 0.0)
            {
                return;
            }
            for (int i = 0; i < this.Count; i++ )
                this[i] += scalar;
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

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            this.CopyTo(result);
            result.Add(scalar);
        }

        /// <summary>
        /// Adds another vector to this vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public override void Add(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var sparseVector = other as SparseVector;

            if (sparseVector == null)
            {
                base.Add(other);
            }
            else
            {
                this.AddScaledSparceVector(1.0, sparseVector);
            }
        }

        private void AddScaledSparceVector(double alpha, SparseVector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
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
                    for (int i = 0; i < this.NonZerosCount; i++)
                    {
                        this.NonZeroValues[i] += this.NonZeroValues[i];
                    }
                }
                else if (alpha == -1.0)
                {
                    NonZerosCount = 0; // Vector is subtracted from itself
                }
                else
                {
                    for (int i = 0; i < this.NonZerosCount; i++)
                    {
                        this[other.NonZeroIndices[i]] += alpha * this.NonZeroValues[i];
                    }
                }
            }
            else
            {
                // "this" and "other" are different objects, so by modifying "this" the "other" object will not be changed
                if (alpha == 1.0)
                {
                    for (int i = 0; i < other.NonZerosCount; i++)
                    {
                        this[other.NonZeroIndices[i]] += other.NonZeroValues[i];
                    }
                }
                else
                {
                    for (int i = 0; i < other.NonZerosCount; i++)
                    {
                        this[other.NonZeroIndices[i]] += alpha * other.NonZeroValues[i];
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

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateVector(result.Count);
                Add(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                this.CopyTo(result);
                result.Add(other);
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the same values of rightSide. 
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
        public override void Subtract(double scalar)
        {
            if (scalar == 0.0)
            {
                return;
            }
            for (int i = 0; i < this.Count; i++)
                this[i] -= scalar;
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

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            this.CopyTo(result);
            result.Subtract(scalar);
        }

        /// <summary>
        /// Subtracts another vector from this vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public override void Subtract(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var sparseVector = other as SparseVector;

            if (sparseVector == null)
            {
                base.Subtract(other);
            }
            else
            {
                this.AddScaledSparceVector(-1.0, sparseVector);
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

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateVector(result.Count);
                Subtract(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                this.CopyTo(result);
                result.Subtract(other);
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the negated values of rightSide. 
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
            var result = new SparseVector(this.Count)
                         {
                             NonZeroValues = new double[this.NonZerosCount],
                             NonZeroIndices = new int[this.NonZerosCount],
                             NonZerosCount = this.NonZerosCount
                         };

            Buffer.BlockCopy(this.NonZeroIndices, 0, result.NonZeroIndices, 0, this.NonZerosCount * Constants.SizeOfInt);

            CommonParallel.For(
                0,
                this.NonZerosCount,
                index => result.NonZeroValues[index] = -this.NonZeroValues[index]);

            return result;
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        public override void Multiply(double scalar)
        {
            if (scalar == 1.0)
            {
                return;
            }
            if (scalar == 0)
            {
                NonZerosCount = 0; // Set array empty
            }
            Control.LinearAlgebraProvider.ScaleArray(scalar, this.NonZeroValues);
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

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            double result = 0;

            // base implementation iterates though all elements, but we need only take non-zeros 
            for (var i = 0; i < this.NonZerosCount; i++)
            {
                result += this.NonZeroValues[i] * other[this.NonZeroIndices[i]];
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
        #endregion

        #region Vector Norms

        /// <summary>
        /// Euclidean Norm also known as 2-Norm.
        /// </summary>
        /// <returns>Scalar ret = sqrt(sum(this[i]^2))</returns>
        public override double Norm()
        {
            var sum = 0.0;

            for (var i = 0; i < this.Count; i++)
            {
                sum = SpecialFunctions.Hypotenuse(sum, this[i]);
            }

            return sum;
        }

        /// <summary>
        /// 1-Norm also known as Manhattan Norm or Taxicab Norm.
        /// </summary>
        /// <returns>Scalar ret = sum(abs(this[i]))</returns>
        public override double Norm1()
        {
            return CommonParallel.Aggregate(
                0,
                this.NonZerosCount,
                index => Math.Abs(this.NonZeroValues[index]));
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>Scalar ret = (sum(abs(this[i])^p))^(1/p)</returns>
        public override double NormP(int p)
        {
            if (1 > p)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            if (1 == p)
            {
                return this.Norm1();
            }

            if (2 == p)
            {
                return this.Norm();
            }

            var sum = CommonParallel.Aggregate(
                0,
                this.NonZerosCount,
                index => Math.Pow(Math.Abs(this.NonZeroValues[index]), p));

            return Math.Pow(sum, 1.0 / p);
        }

        /// <summary>
        /// Infinity Norm.
        /// </summary>
        /// <returns>Scalar ret = max(abs(this[i]))</returns>
        public override double NormInfinity()
        {
            return CommonParallel.Select(
                0,
                this.NonZerosCount,
                (index, localData) => localData = Math.Max(localData, Math.Abs(this.NonZeroValues[index])), Math.Max);
        }
        #endregion

        #region Parse Functions

        /// <summary>
        /// Creates a double sparse vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n,n,..', '(n,n,..)', '[n,n,...]', where n is a double.
        /// </summary>
        /// <returns>
        /// A double sparce vector containing the values specified by the given string.
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
        /// A double sparce vector containing the values specified by the given string.
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

        #region Implementation
        /// <summary>
        /// Delete, Add or Update the value in NonZeroValues and NonZeroIndices
        /// </summary>
        /// <param name="index">Value real index in array</param>
        /// <param name="value">Value</param>
        /// <remarks>This method assume that index is between 0 and Array Size</remarks>
        private void SetValue(int index, double value)
        {
            // Search if "index" already exists in range "0 - real nonzero values count"
            int itemIndex = Array.BinarySearch(NonZeroIndices, 0, NonZerosCount, index);

            if (itemIndex >= 0)
            {
                // Item already exist at itemIndex
                if (value == 0.0)
                {
                    // Value is zero. Let's delete it from Values and Indices array
                    for (int i = itemIndex + 1; i < NonZerosCount; i++)
                    {
                        NonZeroValues[i - 1] = NonZeroValues[i];
                        NonZeroIndices[i - 1] = NonZeroIndices[i];
                    }
                    NonZerosCount -= 1;
        
                    // Check if the storage needs to be shrink. This is reasonable to do if 
                    // there are a lot of non-zero elements and storage is two times bigger
                    if ((NonZerosCount > 1024) && (NonZerosCount < NonZeroIndices.Length / 2))
                    {
                        Array.Resize(ref NonZeroValues, NonZerosCount);
                        Array.Resize(ref NonZeroIndices, NonZerosCount);
                    }

                }
                else
                {
                    NonZeroValues[itemIndex] = value;
                }
            }
            else
            {
                itemIndex = ~itemIndex; //Index where to put new value

                // Check if the storage needs to be increased
                if (NonZerosCount == NonZeroValues.Length)
                {
                    // Value and Indices arrays are completely full so we increase the size
                    int size = Math.Min(NonZeroValues.Length + GrowthSize(), Count);
                    Array.Resize(ref NonZeroValues, size);
                    Array.Resize(ref NonZeroIndices, size);
                }

                // Move all values (with an position larger than index) in the value array 
                // to the next position
                // move all values (with an position larger than index) in the columIndices 
                // array to the next position
                for (int i = NonZerosCount - 1; i > itemIndex - 1; i--)
                {
                    NonZeroValues[i + 1] = NonZeroValues[i];
                    NonZeroIndices[i + 1] = NonZeroIndices[i];
                }

                // Add the value and the column index
                NonZeroValues[itemIndex] = value;
                NonZeroIndices[itemIndex] = index;

                // increase the number of non-zero numbers by one
                NonZerosCount += 1;
            }
        }
        /// <summary>
        /// Calculates the amount with which to grow the storage array's if they need to be
        /// increased in size.
        /// </summary>
        private int GrowthSize()
        {
            int delta;
            if (NonZeroValues.Length > 1024)
            {
                delta = NonZeroValues.Length / 4;
            }
            else
            {
                if (NonZeroValues.Length > 256)
                {
                    delta = 512;
                }
                else
                {
                    delta = NonZeroValues.Length > 64 ? 128 : 32;
                }
            }
            return delta;
        }

        #endregion

        #region System.Object override
        /// <summary>
        /// Check equality. If this is regular vector, then chek by base implementation. If Sparse - use own equition
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var sparseVector = obj as SparseVector;

            if (sparseVector == null)
                return base.Equals(obj);

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, sparseVector))
            {
                return true;
            }

            if ((this.Count != sparseVector.Count) || (this.NonZerosCount != sparseVector.NonZerosCount))
            {
                return false;
            }

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < this.NonZerosCount; index++)
            {
                if (!this.NonZeroValues[index].AlmostEqual(sparseVector.NonZeroValues[index]) || (this.NonZeroIndices[index] != sparseVector.NonZeroIndices[index]))
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
            var hashNum = Math.Min(this.NonZerosCount, 20);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
#if SILVERLIGHT
                hash ^= Precision.DoubleToInt64Bits(this.NonZeroValues[i]);
#else
                hash ^= BitConverter.DoubleToInt64Bits(this.NonZeroValues[i]);
#endif
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }
#endregion
    }
}
