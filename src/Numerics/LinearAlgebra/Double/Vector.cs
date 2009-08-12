// <copyright file="Vector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
// Copyright (c) 2009 Math.NET
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using MathNet.Numerics.Properties;

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
                throw new ArgumentException("target", Resources.ArgumentVectorsSameLengths);
            }

            if (ReferenceEquals(this, target))
            {
                return;
            }

            for (var index = 0; index < Count; index++)
            {
                target[index] = this[index];
            }
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
                for (var index = 0; index < count; index++)
                {
                    destination[destinationOffset + index] = this[offset + index];
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

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> that contains the position and value of the element.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> over this vector that contains the position and value of each
        /// non-zero element.
        /// </returns>
        /// <remarks>
        /// The enumerator returns a 
        /// <seealso cref="KeyValuePair{T,K}"/>
        /// with the key being the element index and the value 
        /// being the value of the element at that index. For sparse vectors, the enumerator will exclude all elements
        /// with a zero value.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", 
            Justification = "Needed to support sparse vectors.")]
        public virtual IEnumerable<KeyValuePair<int, double>> GetIndexedEnumerator()
        {
            for (var index = 0; index < Count; index++)
            {
                yield return new KeyValuePair<int, double>(index, this[index]);
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> over the specified elements.
        /// </summary>
        /// <param name="startIndex">
        /// The element to start copying from.
        /// </param>
        /// <param name="length">
        /// The number of elements to enumerate over.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> over a range of this vector.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="startIndex"/> or <paramref name="startIndex"/> + <paramref name="length"/>
        /// is greater than the vector's length. 
        /// </exception>
        /// <remarks>
        /// The enumerator returns a 
        /// <seealso cref="KeyValuePair{T,K}"/>
        /// with the key being the element index and the value 
        /// being the value of the element at that index.
        /// </remarks>
        /// <seealso cref="KeyValuePair{T,K}"/>
        /// <seealso cref="IEnumerable{T}"/>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", 
            Justification = "Needed to support sparse vectors.")]
        public virtual IEnumerable<KeyValuePair<int, double>> GetIndexedEnumerator(int startIndex, int length)
        {
            if (startIndex > Count)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            if (startIndex + length > Count)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            for (var index = startIndex; index < length; index++)
            {
                yield return new KeyValuePair<int, double>(index, this[index]);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ToString(null, null);
        }

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
            return this.Clone();
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
        /// <remarks>
        /// For sparse vectors, <see cref="GetIndexedEnumerator()"/> will perform better.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", 
            Justification = "Needed to support sparse vectors.")]
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
                if (this[index] != other[index])
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
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="formatProvider">
        /// The format provider to use.
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
                    stringBuilder.Append(",");
                }
            }

            return stringBuilder.ToString();
        }

        #endregion

        #endregion
    }
}