// <copyright file="SparseVectorStorage.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    public class SparseVectorStorage<T> : VectorStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        /// <summary>
        /// Array that contains the indices of the non-zero values.
        /// </summary>
        public int[] Indices;

        /// <summary>
        /// Array that contains the non-zero elements of the vector.
        /// </summary>
        public T[] Values;

        /// <summary>
        /// Gets the number of non-zero elements in the vector.
        /// </summary>
        public int ValueCount;

        internal SparseVectorStorage(int length)
            : base(length)
        {
            Indices = new int[0];
            Values = new T[0];
            ValueCount = 0;
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        public override T At(int index)
        {
            // Search if item idex exists in NonZeroIndices array in range "0 - nonzero values count"
            var itemIndex = Array.BinarySearch(Indices, 0, ValueCount, index);
            return itemIndex >= 0 ? Values[itemIndex] : Zero;
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public override void At(int index, T value)
        {
            // Search if "index" already exists in range "0 - nonzero values count"
            var itemIndex = Array.BinarySearch(Indices, 0, ValueCount, index);
            if (itemIndex >= 0)
            {
                // Non-zero item found in matrix
                if (Zero.Equals(value))
                {
                    // Delete existing item
                    RemoveAtIndexUnchecked(itemIndex);
                }
                else
                {
                    // Update item
                    Values[itemIndex] = value;
                }
            }
            else
            {
                // Item not found. Add new value
                if (!Zero.Equals(value))
                {
                    InsertAtIndexUnchecked(~itemIndex, index, value);
                }
            }
        }

        internal void InsertAtIndexUnchecked(int itemIndex, int index, T value)
        {
            // Check if the storage needs to be increased
            if ((ValueCount == Values.Length) && (ValueCount < Length))
            {
                // Value and Indices arrays are completely full so we increase the size
                var size = Math.Min(Values.Length + GrowthSize(), Length);
                Array.Resize(ref Values, size);
                Array.Resize(ref Indices, size);
            }

            // Move all values (with a position larger than index) in the value array to the next position
            // Move all values (with a position larger than index) in the columIndices array to the next position
            Array.Copy(Values, itemIndex, Values, itemIndex + 1, ValueCount - itemIndex);
            Array.Copy(Indices, itemIndex, Indices, itemIndex + 1, ValueCount - itemIndex);

            // Add the value and the column index
            Values[itemIndex] = value;
            Indices[itemIndex] = index;

            // increase the number of non-zero numbers by one
            ValueCount += 1;
        }

        internal void RemoveAtIndexUnchecked(int itemIndex)
        {
            // Value is zero. Let's delete it from Values and Indices array
            Array.Copy(Values, itemIndex + 1, Values, itemIndex, ValueCount - itemIndex - 1);
            Array.Copy(Indices, itemIndex + 1, Indices, itemIndex, ValueCount - itemIndex - 1);

            ValueCount -= 1;

            // Check whether we need to shrink the arrays. This is reasonable to do if
            // there are a lot of non-zero elements and storage is two times bigger
            if ((ValueCount > 1024) && (ValueCount < Indices.Length / 2))
            {
                Array.Resize(ref Values, ValueCount);
                Array.Resize(ref Indices, ValueCount);
            }
        }

        /// <summary>
        /// Calculates the amount with which to grow the storage array's if they need to be
        /// increased in size.
        /// </summary>
        /// <returns>The amount grown.</returns>
        int GrowthSize()
        {
            int delta;
            if (Values.Length > 1024)
            {
                delta = Values.Length / 4;
            }
            else
            {
                if (Values.Length > 256)
                {
                    delta = 512;
                }
                else
                {
                    delta = Values.Length > 64 ? 128 : 32;
                }
            }

            return delta;
        }

        public override void Clear()
        {
            ValueCount = 0;
        }

        public override void Clear(int index, int count)
        {
            if (index == 0 && count == Length)
            {
                Clear();
                return;
            }

            var first = Array.BinarySearch(Indices, 0, ValueCount, index);
            var last = Array.BinarySearch(Indices, 0, ValueCount, index + count - 1);
            if (first < 0) first = ~first;
            if (last < 0) last = ~last - 1;
            int itemCount = last - first + 1;

            if (itemCount > 0)
            {
                Array.Copy(Values, first + count, Values, first, ValueCount - first - count);
                Array.Copy(Indices, first + count, Indices, first, ValueCount - first - count);

                ValueCount -= count;
            }

            // Check whether we need to shrink the arrays. This is reasonable to do if
            // there are a lot of non-zero elements and storage is two times bigger
            if ((ValueCount > 1024) && (ValueCount < Indices.Length / 2))
            {
                Array.Resize(ref Values, ValueCount);
                Array.Resize(ref Indices, ValueCount);
            }
        }

        public override bool Equals(VectorStorage<T> other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other == null || Length != other.Length)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var otherSparse = other as SparseVectorStorage<T>;
            if (otherSparse == null)
            {
                return base.Equals(other);
            }

            int i = 0, j = 0;
            while (i < ValueCount || j < otherSparse.ValueCount)
            {
                if (j >= otherSparse.ValueCount || i < ValueCount && Indices[i] < otherSparse.Indices[j])
                {
                    if (!Zero.Equals(Values[i++]))
                    {
                        return false;
                    }
                    continue;
                }

                if (i >= ValueCount || j < otherSparse.ValueCount && otherSparse.Indices[j] < Indices[i])
                {
                    if (!Zero.Equals(otherSparse.Values[j++]))
                    {
                        return false;
                    }
                    continue;
                }

                if (!Values[i].Equals(otherSparse.Values[j]))
                {
                    return false;
                }

                i++;
                j++;
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
            var values = Values;
            var hashNum = Math.Min(ValueCount, 25);
            int hash = 17;
            unchecked
            {
                for (var i = 0; i < hashNum; i++)
                {
                    hash = hash * 31 + values[i].GetHashCode();
                }
            }
            return hash;
        }

        // INITIALIZATION

        public static SparseVectorStorage<T> OfVector(VectorStorage<T> vector)
        {
            var storage = new SparseVectorStorage<T>(vector.Length);
            vector.CopyToUnchecked(storage, skipClearing: true);
            return storage;
        }

        public static SparseVectorStorage<T> OfInit(int length, Func<int, T> init)
        {
            if (length < 1)
            {
                throw new ArgumentOutOfRangeException("length", string.Format(Resources.ArgumentLessThanOne, length));
            }

            var indices = new List<int>();
            var values = new List<T>();
            for (int i = 0; i < length; i++)
            {
                var item = init(i);
                if (!Zero.Equals(item))
                {
                    values.Add(item);
                    indices.Add(i);
                }
            }
            return new SparseVectorStorage<T>(length)
                {
                    Indices = indices.ToArray(),
                    Values = values.ToArray(),
                    ValueCount = values.Count
                };
        }

        public static SparseVectorStorage<T> OfEnumerable(IEnumerable<T> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var indices = new List<int>();
            var values = new List<T>();
            int index = 0;

            foreach (T item in data)
            {
                if (!Zero.Equals(item))
                {
                    values.Add(item);
                    indices.Add(index);
                }
                index++;
            }

            return new SparseVectorStorage<T>(index)
                {
                    Indices = indices.ToArray(),
                    Values = values.ToArray(),
                    ValueCount = values.Count
                };
        }

        public static SparseVectorStorage<T> OfIndexedEnumerable(int length, IEnumerable<Tuple<int, T>> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var indices = new List<int>();
            var values = new List<T>();
            foreach (var item in data)
            {
                if (!Zero.Equals(item.Item2))
                {
                    values.Add(item.Item2);
                    indices.Add(item.Item1);
                }
            }

            var indicesArray = indices.ToArray();
            var valuesArray = values.ToArray();
            Sorting.Sort(indicesArray, valuesArray);

            return new SparseVectorStorage<T>(length)
                {
                    Indices = indicesArray,
                    Values = valuesArray,
                    ValueCount = values.Count
                };
        }

        // ENUMERATION

        public override IEnumerable<T> Enumerate()
        {
            int k = 0;
            for (int i = 0; i < Length; i++)
            {
                yield return k < ValueCount && Indices[k] == i
                    ? Values[k++]
                    : Zero;
            }
        }

        public override IEnumerable<Tuple<int, T>> EnumerateNonZero()
        {
            for (var i = 0; i < ValueCount; i++)
            {
                if (!Zero.Equals(Values[i]))
                {
                    yield return new Tuple<int, T>(Indices[i], Values[i]);
                }
            }
        }

        // VECTOR COPY

        internal override void CopyToUnchecked(VectorStorage<T> target, bool skipClearing = false)
        {
            var sparseTarget = target as SparseVectorStorage<T>;
            if (sparseTarget != null)
            {
                CopyToUnchecked(sparseTarget);
                return;
            }

            // FALL BACK

            if (!skipClearing)
            {
                target.Clear();
            }

            if (ValueCount != 0)
            {
                for (int i = 0; i < ValueCount; i++)
                {
                    target.At(Indices[i], Values[i]);
                }
            }
        }

        void CopyToUnchecked(SparseVectorStorage<T> target)
        {
            if (ReferenceEquals(this, target))
            {
                return;
            }

            if (Length != target.Length)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, Length, target.Length);
                throw new ArgumentException(message, "target");
            }

            target.ValueCount = ValueCount;
            target.Values = new T[ValueCount];
            target.Indices = new int[ValueCount];

            if (ValueCount != 0)
            {
                Array.Copy(Values, target.Values, ValueCount);
                Buffer.BlockCopy(Indices, 0, target.Indices, 0, ValueCount * Constants.SizeOfInt);
            }
        }

        // Row COPY

        internal override void CopyToRowUnchecked(MatrixStorage<T> target, int rowIndex, bool skipClearing = false)
        {
            if (!skipClearing)
            {
                target.Clear(rowIndex, 1, 0, Length);
            }

            if (ValueCount == 0)
            {
                return;
            }

            for (int i = 0; i < ValueCount; i++)
            {
                target.At(rowIndex, Indices[i], Values[i]);
            }
        }

        // COLUMN COPY

        internal override void CopyToColumnUnchecked(MatrixStorage<T> target, int columnIndex, bool skipClearing = false)
        {
            if (!skipClearing)
            {
                target.Clear(0, Length, columnIndex, 1);
            }

            if (ValueCount == 0)
            {
                return;
            }

            for (int i = 0; i < ValueCount; i++)
            {
                target.At(Indices[i], columnIndex, Values[i]);
            }
        }

        // SUB-VECTOR COPY

        internal override void CopySubVectorToUnchecked(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count,
            bool skipClearing = false)
        {
            var sparseTarget = target as SparseVectorStorage<T>;
            if (sparseTarget != null)
            {
                CopySubVectorToUnchecked(sparseTarget, sourceIndex, targetIndex, count, skipClearing);
                return;
            }

            // FALL BACK

            var offset = targetIndex - sourceIndex;

            var sourceFirst = Array.BinarySearch(Indices, 0, ValueCount, sourceIndex);
            var sourceLast = Array.BinarySearch(Indices, 0, ValueCount, sourceIndex + count - 1);
            if (sourceFirst < 0) sourceFirst = ~sourceFirst;
            if (sourceLast < 0) sourceLast = ~sourceLast - 1;

            if (!skipClearing)
            {
                target.Clear(targetIndex, count);
            }

            for (int i = sourceFirst; i <= sourceLast; i++)
            {
                target.At(Indices[i] + offset, Values[i]);
            }
        }

        void CopySubVectorToUnchecked(SparseVectorStorage<T> target,
            int sourceIndex, int targetIndex, int count,
            bool skipClearing)
        {
            var offset = targetIndex - sourceIndex;

            var sourceFirst = Array.BinarySearch(Indices, 0, ValueCount, sourceIndex);
            var sourceLast = Array.BinarySearch(Indices, 0, ValueCount, sourceIndex + count - 1);
            if (sourceFirst < 0) sourceFirst = ~sourceFirst;
            if (sourceLast < 0) sourceLast = ~sourceLast - 1;
            int sourceCount = sourceLast - sourceFirst + 1;

            // special case when copying to itself
            if (ReferenceEquals(this, target))
            {
                var values = new T[sourceCount];
                var indices = new int[sourceCount];

                Array.Copy(Values, sourceFirst, values, 0, sourceCount);
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = Indices[i + sourceFirst];
                }

                if (!skipClearing)
                {
                    Clear(targetIndex, count);
                }

                for (int i = sourceFirst; i <= sourceLast; i++)
                {
                    At(indices[i] + offset, values[i]);
                }

                return;
            }

            // special case for empty target - much faster
            if (target.ValueCount == 0)
            {
                var values = new T[sourceCount];
                var indices = new int[sourceCount];

                Array.Copy(Values, sourceFirst, values, 0, sourceCount);
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = Indices[i + sourceFirst] + offset;
                }

                target.ValueCount = sourceCount;
                target.Values = values;
                target.Indices = indices;

                return;
            }

            if (!skipClearing)
            {
                target.Clear(targetIndex, count);
            }

            for (int i = sourceFirst; i <= sourceLast; i++)
            {
                target.At(Indices[i] + offset, Values[i]);
            }
        }

        // FUNCTIONAL COMBINATORS

        public override void MapInplace(Func<T, T> f, bool forceMapZeros = false)
        {
            var indices = new List<int>();
            var values = new List<T>();
            if (forceMapZeros || !Zero.Equals(f(Zero)))
            {
                int k = 0;
                for (int i = 0; i < Length; i++)
                {
                    var item = k < ValueCount && (Indices[k]) == i ? f(Values[k++]) : f(Zero);
                    if (!Zero.Equals(item))
                    {
                        values.Add(item);
                        indices.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < ValueCount; i++)
                {
                    var item = f(Values[i]);
                    if (!Zero.Equals(item))
                    {
                        values.Add(item);
                        indices.Add(Indices[i]);
                    }
                }
            }
            Indices = indices.ToArray();
            Values = values.ToArray();
            ValueCount = values.Count;
        }

        public override void MapIndexedInplace(Func<int, T, T> f, bool forceMapZeros = false)
        {
            var indices = new List<int>();
            var values = new List<T>();
            if (forceMapZeros || !Zero.Equals(f(0, Zero)))
            {
                int k = 0;
                for (int i = 0; i < Length; i++)
                {
                    var item = k < ValueCount && (Indices[k]) == i ? f(i, Values[k++]) : f(i, Zero);
                    if (!Zero.Equals(item))
                    {
                        values.Add(item);
                        indices.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < ValueCount; i++)
                {
                    var item = f(Indices[i], Values[i]);
                    if (!Zero.Equals(item))
                    {
                        values.Add(item);
                        indices.Add(Indices[i]);
                    }
                }
            }
            Indices = indices.ToArray();
            Values = values.ToArray();
            ValueCount = values.Count;
        }
    }
}
