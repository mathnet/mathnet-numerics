﻿using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    public abstract partial class VectorStorage<T> : IEquatable<VectorStorage<T>>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        public readonly int Length;

        protected VectorStorage(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMustBePositive);
            }

            Length = length;
        }

        /// <summary>
        /// Gets or sets the value at the given index, with range checking.
        /// </summary>
        /// <param name="index">
        /// The index of the element.
        /// </param>
        /// <value>The value to get or set.</value>
        /// <remarks>This method is ranged checked. <see cref="At(int)"/> and <see cref="At(int,T)"/>
        /// to get and set values without range checking.</remarks>
        public T this[int index]
        {
            get
            {
                ValidateRange(index);
                return At(index);
            }

            set
            {
                ValidateRange(index);
                At(index, value);
            }
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The requested element.</returns>
        /// <remarks>Not range-checked.</remarks>
        public abstract T At(int index);

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <param name="value">The value to set the element to. </param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks.</remarks>
        public abstract void At(int index, T value);

        /// <summary>
        /// True if all fields of this vector can be set to any value.
        /// False if some fields are fixed.
        /// </summary>
        public virtual bool IsFullyMutable
        {
            get { return true; }
        }

        /// <summary>
        /// True if the specified field can be set to any value.
        /// False if the field is fixed.
        /// </summary>
        public virtual bool IsMutable(int index)
        {
            return true;
        }

        public virtual void Clear()
        {
            for (var i = 0; i < Length; i++)
            {
                At(i, default(T));
            }
        }

        public virtual void Clear(int index, int count)
        {
            for (var i = index; i < index + count; i++)
            {
                At(i, default(T));
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Equals(VectorStorage<T> other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other == null)
            {
                return false;
            }
            if (Length != other.Length)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < Length; index++)
            {
                if (!At(index).Equals(other.At(index)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param>
        public override sealed bool Equals(object obj)
        {
            return Equals(obj as MatrixStorage<T>);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(Length, 25);
            int hash = 17;
            unchecked
            {
                for (var i = 0; i < hashNum; i++)
                {
                    hash = hash*31 + At(i).GetHashCode();
                }
            }
            return hash;
        }

        /// <remarks>Parameters assumed to be validated already.</remarks>
        public virtual void CopyTo(VectorStorage<T> target, bool skipClearing = false)
        {
            for (int i = 0; i < Length; i++)
            {
                target.At(i, At(i));
            }
        }

        public virtual void CopySubVectorTo(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count,
            bool skipClearing = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (ReferenceEquals(this, target))
            {
                throw new NotSupportedException();
            }

            ValidateSubVectorRange(target, sourceIndex, targetIndex, count);

            for (int i = sourceIndex, ii = targetIndex; i < sourceIndex + count; i++, ii++)
            {
                target.At(ii, At(i));
            }
        }
    }
}
