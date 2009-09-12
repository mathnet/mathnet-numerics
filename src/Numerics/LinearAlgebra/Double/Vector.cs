// <copyright file="Vector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    using Properties;
    using Threading;

    /// <summary>
    /// Defines the base class for <c>Vector</c> classes.
    /// </summary>
    [Serializable]
    public abstract class Vector : IFormattable, IEnumerable<double>, ICloneable, IEquatable<Vector>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector"/> class. 
        /// Constructs a <strong>Vector</strong> with the given size.
        /// </summary>
        /// <param name="size">
        /// The size of the <strong>Vector</strong> to construct.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="size"/> is less than one.
        /// </exception>
        protected Vector(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "size");
            }

            Count = size;
        }

        /// <summary>
        /// Gets he number of elements in the vector.
        /// </summary>
        public int Count
        {
            get;
            private set;
        }

        /// <summary>Gets or sets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns> 
        /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is negative or 
        /// greater than the size of the vector.</exception>
        public abstract double this[int index]
        {
            get;
            set;
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
        public abstract Matrix CreateMatrix(int rows, int columns);

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
        public abstract Vector CreateVector(int size);

        #region Elementary operations
        /// <summary>
        /// Adds a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        public virtual void Add(double scalar)
        {
            if (scalar == 0.0)
            {
                return;
            }

            Parallel.For(0, Count, i => this[i] += scalar);
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void Add(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (!ReferenceEquals(this, result))
            {
                CopyTo(result);
            }

            result.Add(scalar);
        }

        /// <summary>
        /// Returns this vector.
        /// </summary>
        /// <returns>This vector.</returns>
        /// <remarks>Added as an alternative to the unary addition operator.</remarks>
        public virtual Vector Plus()
        {
            return this;
        }

        /// <summary>
        /// Adds another vector to this vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public virtual void Add(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            Parallel.For(0, Count, i => this[i] += other[i]);
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
        public virtual void Add(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
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
                CopyTo(result);
                result.Add(other);
            }
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        public virtual void Subtract(double scalar)
        {
            if (scalar == 0.0)
            {
                return;
            }

            Parallel.For(0, Count, i => this[i] -= scalar);
        }

        /// <summary>
        ///  Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void Subtract(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (!ReferenceEquals(this, result))
            {
                CopyTo(result);
            }

            result.Subtract(scalar);
        }

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        /// <returns>The negated vector.</returns>
        /// <remarks>Added as an alternative to the unary negation operator.</remarks>
        public virtual Vector Negate()
        {
            return this * -1;
        }

        /// <summary>
        /// Subtracts another vector from this vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public virtual void Subtract(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            Parallel.For(0, Count, i => this[i] -= other[i]);
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void Subtract(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
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
                CopyTo(result);
                result.Subtract(other);
            }
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        public virtual void Multiply(double scalar)
        {
            if (scalar.AlmostEqual(1.0))
            {
                return;
            }

            Parallel.For(0, Count, index => this[index] *= scalar);
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="result">The vector to store the result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void Multiply(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }
        
            if (!ReferenceEquals(this, result))
            {
                CopyTo(result);
            }

            result.Multiply(scalar);
        }

        /// <summary>
        /// Divides each element of the vector by a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to divide with.</param>
        public virtual void Divide(double scalar)
        {
            if (scalar.AlmostEqual(1.0))
            {
                return;
            }

            Multiply(1.0 / scalar);
        }

        /// <summary>
        ///  Divides each element of the vector by a scalar and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to divide with.</param>
        /// <param name="result">The vector to store the result of the division.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void Divide(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (!ReferenceEquals(this, result))
            {
                CopyTo(result);
            }

            result.Multiply(1.0 / scalar);
        }
        #endregion

        #region Arithmetic Operator Overloading
        /// <summary>
        /// Returns a <strong>Vector</strong> containing the same values of rightSide. 
        /// </summary>
        /// <remarks>This method is included for completeness.</remarks>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator +(Vector rightSide)
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
        public static Vector operator +(Vector leftSide, Vector rightSide)
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
        /// Returns a <strong>Vector</strong> containing the negated values of rightSide. 
        /// </summary>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the negated values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator -(Vector rightSide)
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
        public static Vector operator -(Vector leftSide, Vector rightSide)
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
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to scale.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Vector operator *(Vector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            var ret = leftSide.Clone();
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
        public static Vector operator *(double leftSide, Vector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            var ret = rightSide.Clone();
            ret.Multiply(leftSide);
            return ret;
        }

        /// <summary>
        /// Divides a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to divide.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Vector operator /(Vector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            var ret = leftSide.Clone();
            ret.Multiply(1.0 / rightSide);
            return ret;
        }

        #endregion

        #region Vector Norms

        /// <summary>
        /// Euclidean Norm also known as 2-Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = sqrt(sum(this[i]^2))
        /// </returns>
        public virtual double Norm()
        {
            return NormP(2);
        }

        /// <summary>
        /// Squared Euclidean 2-Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = sum(this[i]^2)
        /// </returns>
        public virtual double SquaredNorm()
        {
            var norm = Norm();
            return norm * norm;
        }

        /// <summary>
        /// 1-Norm also known as Manhattan Norm or Taxicab Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = sum(abs(this[i]))
        /// </returns>
        public virtual double Norm1()
        {
            return NormP(1);
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>Scalar ret = (sum(abs(this[i])^p))^(1/p)</returns>
        public virtual double NormP(int p)
        {
            if (1 > p)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            var sum = 0.0;
            var syncLock = new object();
            Parallel.For(
                0, 
                Count, 
                () => 0.0,
                (index, localData) =>
                {
                    localData += Math.Pow(Math.Abs(this[index]), p);
                    return localData;
                },
                localResult =>
                {
                    lock (syncLock)
                    {
                        sum += localResult;
                    }
                });

            return Math.Pow(sum, 1.0 / p);
        }

        /// <summary>
        /// Infinity Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = max(abs(this[i]))
        /// </returns>
        public virtual double NormInfinity()
        {
            var max = 0.0;
            var syncLock = new object();
            Parallel.For(
                0, 
                Count,
                () => 0.0,
                (index, localData) =>
                {
                    localData = Math.Max(localData, Math.Abs(this[index]));
                    return localData;
                },
                localResult =>
                {
                    lock (syncLock)
                    {
                        max = Math.Max(localResult, max);
                    }
                });

            return max;
        }

        /// <summary>
        /// Normalizes this vector to a unit vector with respect to the Eucliden 2-Norm.
        /// </summary>
        /// <returns>This vector normalized to a unit vector with respect to the Eucliden 2-Norm.</returns>
        public virtual Vector Normalize()
        {
            var norm = Norm();
            var clone = Clone();
            if (norm == 0.0)
            {
                return clone;
            }

            clone.Multiply(1.0 / norm);
            return clone;
        }

        #endregion

        #region Coping and Conversion
        /// <summary>
        /// Returns a deep-copy clone of the vector.
        /// </summary>
        /// <returns>
        /// A deep-copy clone of the vector.
        /// </returns>
        public Vector Clone()
        {
            var retrunVector = CreateVector(Count);
            CopyTo(retrunVector);
            return retrunVector;
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
        public virtual void CopyTo(Vector target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (Count != target.Count)
            {
                throw new ArgumentException("target", Resources.ArgumentVectorsSameLength);
            }

            if (ReferenceEquals(this, target))
            {
                return;
            }

            Parallel.For(0, Count, index => target[index] = this[index]);
        }

        /// <summary>
        /// Copies the requested elements from this vector to another.
        /// </summary>
        /// <param name="destination">
        /// The vector to copy the elements to.
        /// </param>
        /// <param name="offset">
        /// The element to start copying from.
        /// </param>
        /// <param name="destinationOffset">
        /// The element to start copying to.
        /// </param>
        /// <param name="count">
        /// The number of elements to copy.
        /// </param>
        public virtual void CopyTo(Vector destination, int offset, int destinationOffset, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            if (offset >= Count)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (offset + count > Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (destinationOffset >= destination.Count)
            {
                throw new ArgumentOutOfRangeException("destinationOffset");
            }

            if (destinationOffset + count > destination.Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (ReferenceEquals(this, destination))
            {
                var tmpVector = destination.CreateVector(destination.Count);
                CopyTo(tmpVector, offset, destinationOffset, count);
                tmpVector.CopyTo(destination);
            }
            else
            {
               Parallel.For(0, count, index => destination[destinationOffset + index] = this[offset + index]);
            }
        }

        #endregion

        #region Implemented Interfaces

        #region ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable<double>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<double> GetEnumerator()
        {
            for (var index = 0; index < Count; index++)
            {
                yield return this[index];
            }
        }

        #endregion

        #region IEquatable<Vector>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Vector other)
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

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < Count; index++)
            {
                if (!this[index].AlmostEqual(other[index]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IFormattable

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < Count; index++)
            {
                stringBuilder.Append(this[index].ToString(format, formatProvider));
                if (index != Count - 1)
                {
                    stringBuilder.Append(formatProvider.GetTextInfo().ListSeparator);
                }
            }

            return stringBuilder.ToString();
        }

        #endregion

        #endregion

        #region System.Object overrides

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Vector);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(Count, 20);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
                hash ^= BitConverter.DoubleToInt64Bits(this[i]);
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(null, null);
        }
        #endregion
    }
}