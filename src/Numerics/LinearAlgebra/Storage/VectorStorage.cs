// <copyright file="VectorStorage.cs" company="Math.NET">
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
using System.Runtime.Serialization;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/LinearAlgebra")]
    public abstract partial class VectorStorage<T> : IEquatable<VectorStorage<T>>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        protected static readonly T Zero = BuilderInstance<T>.Vector.Zero;

        [DataMember(Order = 1)]
        public readonly int Length;

        protected VectorStorage(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Value must not be negative (zero is ok).");
            }

            Length = length;
        }

        /// <summary>
        /// True if the vector storage format is dense.
        /// </summary>
        public abstract bool IsDense { get; }

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
        public sealed override bool Equals(object obj)
        {
            return Equals(obj as VectorStorage<T>);
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

        // CLEARING

        public virtual void Clear()
        {
            for (var i = 0; i < Length; i++)
            {
                At(i, Zero);
            }
        }

        public virtual void Clear(int index, int count)
        {
            for (var i = index; i < index + count; i++)
            {
                At(i, Zero);
            }
        }

        // VECTOR COPY

        public void CopyTo(VectorStorage<T> target, ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (ReferenceEquals(this, target))
            {
                return;
            }

            if (Length != target.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(target));
            }

            CopyToUnchecked(target, existingData);
        }

        internal virtual void CopyToUnchecked(VectorStorage<T> target, ExistingData existingData)
        {
            for (int i = 0; i < Length; i++)
            {
                target.At(i, At(i));
            }
        }

        // ROW COPY

        public void CopyToRow(MatrixStorage<T> target, int rowIndex, ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (Length != target.ColumnCount)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(target));
            }

            ValidateRowRange(target, rowIndex);
            CopyToRowUnchecked(target, rowIndex, existingData);
        }

        internal virtual void CopyToRowUnchecked(MatrixStorage<T> target, int rowIndex, ExistingData existingData)
        {
            for (int j = 0; j < Length; j++)
            {
                target.At(rowIndex, j, At(j));
            }
        }

        // COLUMN COPY

        public void CopyToColumn(MatrixStorage<T> target, int columnIndex, ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (Length != target.RowCount)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(target));
            }

            ValidateColumnRange(target, columnIndex);
            CopyToColumnUnchecked(target, columnIndex, existingData);
        }

        internal virtual void CopyToColumnUnchecked(MatrixStorage<T> target, int columnIndex, ExistingData existingData)
        {
            for (int i = 0; i < Length; i++)
            {
                target.At(i, columnIndex, At(i));
            }
        }

        // SUB-VECTOR COPY

        public void CopySubVectorTo(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count,
            ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (count == 0)
            {
                return;
            }

            ValidateSubVectorRange(target, sourceIndex, targetIndex, count);
            CopySubVectorToUnchecked(target, sourceIndex, targetIndex, count, existingData);
        }

        internal virtual void CopySubVectorToUnchecked(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count, ExistingData existingData)
        {
            if (ReferenceEquals(this, target))
            {
                var tmp = new T[count];
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = At(i + sourceIndex);
                }
                for (int i = 0; i < tmp.Length; i++)
                {
                    At(i + targetIndex, tmp[i]);
                }

                return;
            }

            for (int i = sourceIndex, ii = targetIndex; i < sourceIndex + count; i++, ii++)
            {
                target.At(ii, At(i));
            }
        }

        // SUB-ROW COPY

        public void CopyToSubRow(MatrixStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (columnCount == 0)
            {
                return;
            }

            ValidateSubRowRange(target, rowIndex, sourceColumnIndex, targetColumnIndex, columnCount);
            CopyToSubRowUnchecked(target, rowIndex, sourceColumnIndex, targetColumnIndex, columnCount, existingData);
        }

        internal virtual void CopyToSubRowUnchecked(MatrixStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount, ExistingData existingData)
        {
            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                target.At(rowIndex, jj, At(j));
            }
        }

        // SUB-COLUMN COPY

        public void CopyToSubColumn(MatrixStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (rowCount == 0)
            {
                return;
            }

            ValidateSubColumnRange(target, columnIndex, sourceRowIndex, targetRowIndex, rowCount);
            CopyToSubColumnUnchecked(target, columnIndex, sourceRowIndex, targetRowIndex, rowCount, existingData);
        }

        internal virtual void CopyToSubColumnUnchecked(MatrixStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount, ExistingData existingData)
        {
            for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
            {
                target.At(ii, columnIndex, At(i));
            }
        }

        // EXTRACT

        public virtual T[] ToArray()
        {
            var ret = new T[Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = At(i);
            }
            return ret;
        }

        public virtual T[] AsArray()
        {
            return null;
        }

        // ENUMERATION

        public virtual IEnumerable<T> Enumerate()
        {
            for (var i = 0; i < Length; i++)
            {
                yield return At(i);
            }
        }

        public virtual IEnumerable<(int, T)> EnumerateIndexed()
        {
            for (var i = 0; i < Length; i++)
            {
                yield return (i, At(i));
            }
        }

        public virtual IEnumerable<T> EnumerateNonZero()
        {
            for (var i = 0; i < Length; i++)
            {
                var x = At(i);
                if (!Zero.Equals(x))
                {
                    yield return x;
                }
            }
        }

        public virtual IEnumerable<(int, T)> EnumerateNonZeroIndexed()
        {
            for (var i = 0; i < Length; i++)
            {
                var x = At(i);
                if (!Zero.Equals(x))
                {
                    yield return (i, x);
                }
            }
        }

        // FIND

        public virtual Tuple<int, T> Find(Func<T, bool> predicate, Zeros zeros)
        {
            for (int i = 0; i < Length; i++)
            {
                var item = At(i);
                if (predicate(item))
                {
                    return new Tuple<int, T>(i, item);
                }
            }
            return null;
        }

        public Tuple<int, T, TOther> Find2<TOther>(VectorStorage<TOther> other, Func<T, TOther, bool> predicate, Zeros zeros)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (Length != other.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(other));
            }

            return Find2Unchecked(other, predicate, zeros);
        }

        internal virtual Tuple<int, T, TOther> Find2Unchecked<TOther>(VectorStorage<TOther> other, Func<T, TOther, bool> predicate, Zeros zeros)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            for (int i = 0; i < Length; i++)
            {
                var item = At(i);
                var otherItem = other.At(i);
                if (predicate(item, otherItem))
                {
                    return new Tuple<int, T, TOther>(i, item, otherItem);
                }
            }
            return null;
        }

        // FUNCTIONAL COMBINATORS: MAP

        public virtual void MapInplace(Func<T, T> f, Zeros zeros)
        {
            for (int i = 0; i < Length; i++)
            {
                At(i, f(At(i)));
            }
        }

        public virtual void MapIndexedInplace(Func<int, T, T> f, Zeros zeros)
        {
            for (int i = 0; i < Length; i++)
            {
                At(i, f(i, At(i)));
            }
        }

        public void MapTo<TU>(VectorStorage<TU> target, Func<T, TU> f, Zeros zeros, ExistingData existingData)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (Length != target.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(target));
            }

            MapToUnchecked(target, f, zeros, existingData);
        }

        internal virtual void MapToUnchecked<TU>(VectorStorage<TU> target, Func<T, TU> f, Zeros zeros, ExistingData existingData)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            for (int i = 0; i < Length; i++)
            {
                target.At(i, f(At(i)));
            }
        }

        public void MapIndexedTo<TU>(VectorStorage<TU> target, Func<int, T, TU> f, Zeros zeros, ExistingData existingData)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (Length != target.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(target));
            }

            MapIndexedToUnchecked(target, f, zeros, existingData);
        }

        internal virtual void MapIndexedToUnchecked<TU>(VectorStorage<TU> target, Func<int, T, TU> f, Zeros zeros, ExistingData existingData)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            for (int i = 0; i < Length; i++)
            {
                target.At(i, f(i, At(i)));
            }
        }

        public void Map2To(VectorStorage<T> target, VectorStorage<T> other, Func<T, T, T> f, Zeros zeros, ExistingData existingData)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (Length != target.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(target));
            }

            if (Length != other.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(other));
            }

            Map2ToUnchecked(target, other, f, zeros, existingData);
        }

        internal virtual void Map2ToUnchecked(VectorStorage<T> target, VectorStorage<T> other, Func<T, T, T> f, Zeros zeros, ExistingData existingData)
        {
            for (int i = 0; i < Length; i++)
            {
                target.At(i, f(At(i), other.At(i)));
            }
        }

        // FUNCTIONAL COMBINATORS: FOLD

        public TState Fold2<TOther, TState>(VectorStorage<TOther> other, Func<TState, T, TOther, TState> f, TState state, Zeros zeros)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (Length != other.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(other));
            }

            return Fold2Unchecked(other, f, state, zeros);
        }

        internal virtual TState Fold2Unchecked<TOther, TState>(VectorStorage<TOther> other, Func<TState, T, TOther, TState> f, TState state, Zeros zeros)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            for (int i = 0; i < Length; i++)
            {
                state = f(state, At(i), other.At(i));
            }

            return state;
        }
    }
}
