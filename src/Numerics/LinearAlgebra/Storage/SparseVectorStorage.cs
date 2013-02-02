using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    public class SparseVectorStorage<T> : VectorStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        readonly T _zero;

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

        internal SparseVectorStorage(int length, T zero = default(T))
            : base(length)
        {
            _zero = zero;
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
            return itemIndex >= 0 ? Values[itemIndex] : _zero;
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
                if (_zero.Equals(value))
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
                if (!_zero.Equals(value))
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
    }
}
