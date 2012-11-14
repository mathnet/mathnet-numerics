﻿using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    internal class SparseCompressedRowMatrixStorage<T> : MatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        readonly T _zero;

        /// <summary>
        /// The array containing the row indices of the existing rows. Element "j" of the array gives the index of the 
        /// element in the <see cref="Values"/> array that is first non-zero element in a row "j"
        /// </summary>
        public readonly int[] RowPointers;

        /// <summary>
        /// An array containing the column indices of the non-zero values. Element "I" of the array 
        /// is the number of the column in matrix that contains the I-th value in the <see cref="_nonZeroValues"/> array.
        /// </summary>
        public int[] ColumnIndices;

        /// <summary>
        /// Array that contains the non-zero elements of matrix. Values of the non-zero elements of matrix are mapped into the values 
        /// array using the row-major storage mapping described in a compressed sparse row (CSR) format.
        /// </summary>
        public T[] Values;

        /// <summary>
        /// Gets the number of non zero elements in the matrix.
        /// </summary>
        /// <value>The number of non zero elements.</value>
        public int ValueCount;

        internal SparseCompressedRowMatrixStorage(int rows, int columns, T zero)
            : base(rows, columns)
        {
            _zero = zero;
            RowPointers = new int[rows];
            ColumnIndices = new int[0];
            Values = new T[0];
            ValueCount = 0;
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <returns>
        /// The requested element.
        /// </returns>
        /// <remarks>Not range-checked.</remarks>
        public override T At(int row, int column)
        {
            var index = FindItem(row, column);
            return index >= 0 ? Values[index] : _zero;
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        /// <param name="row"> The row of the element. </param>
        /// <param name="column"> The column of the element. </param>
        /// <param name="value"> The value to set the element to. </param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks.</remarks>
        public override void At(int row, int column, T value)
        {
            var index = FindItem(row, column);
            if (index >= 0)
            {
                // Non-zero item found in matrix
                if (_zero.Equals(value))
                {
                    // Delete existing item
                    DeleteItemByIndex(index, row);
                }
                else
                {
                    // Update item
                    Values[index] = value;
                }
            }
            else
            {
                // Item not found. Add new value
                if (_zero.Equals(value))
                {
                    return;
                }

                index = ~index;

                // Check if the storage needs to be increased
                if ((ValueCount == Values.Length) && (ValueCount < ((long)RowCount * ColumnCount)))
                {
                    // Value array is completely full so we increase the size
                    // Determine the increase in size. We will not grow beyond the size of the matrix
                    var size = Math.Min(Values.Length + GrowthSize(), (long)RowCount * ColumnCount);
                    if (size > int.MaxValue)
                    {
                        throw new NotSupportedException(Resources.TooManyElements);
                    }

                    Array.Resize(ref Values, (int)size);
                    Array.Resize(ref ColumnIndices, (int)size);
                }

                // Move all values (with an position larger than index) in the value array to the next position
                // move all values (with an position larger than index) in the columIndices array to the next position
                for (var i = ValueCount - 1; i > index - 1; i--)
                {
                    Values[i + 1] = Values[i];
                    ColumnIndices[i + 1] = ColumnIndices[i];
                }

                // Add the value and the column index
                Values[index] = value;
                ColumnIndices[index] = column;

                // increase the number of non-zero numbers by one
                ValueCount += 1;

                // add 1 to all the row indices for rows bigger than rowIndex
                // so that they point to the correct part of the value array again.
                for (var i = row + 1; i < RowPointers.Length; i++)
                {
                    RowPointers[i] += 1;
                }
            }
        }

        public override void Clear()
        {
            ValueCount = 0;
            Array.Clear(RowPointers, 0, RowPointers.Length);
        }

        /// <summary>
        /// Delete value from internal storage
        /// </summary>
        /// <param name="itemIndex">Index of value in nonZeroValues array</param>
        /// <param name="row">Row number of matrix</param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks</remarks>
        void DeleteItemByIndex(int itemIndex, int row)
        {
            // Move all values (with an position larger than index) in the value array to the previous position
            // move all values (with an position larger than index) in the columIndices array to the previous position
            for (var i = itemIndex + 1; i < ValueCount; i++)
            {
                Values[i - 1] = Values[i];
                ColumnIndices[i - 1] = ColumnIndices[i];
            }

            // Decrease value in Row
            for (var i = row + 1; i < RowPointers.Length; i++)
            {
                RowPointers[i] -= 1;
            }

            ValueCount -= 1;

            // Check if the storage needs to be shrink. This is reasonable to do if 
            // there are a lot of non-zero elements and storage is two times bigger
            if ((ValueCount > 1024) && (ValueCount < Values.Length / 2))
            {
                Array.Resize(ref Values, ValueCount);
                Array.Resize(ref ColumnIndices, ValueCount);
            }
        }

        /// <summary>
        /// Find item Index in nonZeroValues array
        /// </summary>
        /// <param name="row">Matrix row index</param>
        /// <param name="column">Matrix column index</param>
        /// <returns>Item index</returns>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks</remarks>
        public int FindItem(int row, int column)
        {
            // Determin bounds in columnIndices array where this item should be searched (using rowIndex)
            var startIndex = RowPointers[row];
            var endIndex = row < RowPointers.Length - 1 ? RowPointers[row + 1] : ValueCount;
            return Array.BinarySearch(ColumnIndices, startIndex, endIndex - startIndex, column);
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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(MatrixStorage<T> other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other == null)
            {
                return false;
            }
            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var sparse = other as SparseCompressedRowMatrixStorage<T>;
            if (sparse == null)
            {
                return base.Equals(other);
            }

            if (ValueCount != sparse.ValueCount)
            {
                // TODO: this is not always correct
                return false;
            }

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < ValueCount; index++)
            {
                // TODO: AlmostEquals
                if (!Values[index].Equals(sparse.Values[index]) || ColumnIndices[index] != sparse.ColumnIndices[index])
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

        /// <remarks>Parameters assumed to be validated already.</remarks>
        public override void CopyTo(MatrixStorage<T> target, bool skipClearing = false)
        {
            var sparseTarget = target as SparseCompressedRowMatrixStorage<T>;
            if (sparseTarget != null)
            {
                CopyTo(sparseTarget);
                return;
            }

            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                CopyTo(denseTarget, skipClearing);
                return;
            }

            // FALL BACK

            if (!skipClearing)
            {
                target.Clear();
            }

            if (ValueCount != 0)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    var startIndex = RowPointers[row];
                    var endIndex = row < RowPointers.Length - 1 ? RowPointers[row + 1] : ValueCount;
                    for (var j = startIndex; j < endIndex; j++)
                    {
                        target.At(row, ColumnIndices[j], Values[j]);
                    }
                }
            }
        }

        public void CopyTo(SparseCompressedRowMatrixStorage<T> target)
        {
            if (ReferenceEquals(this, target))
            {
                return;
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (RowCount != target.RowCount || ColumnCount != target.ColumnCount)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, RowCount + "x" + ColumnCount, target.RowCount + "x" + target.ColumnCount);
                throw new ArgumentException(message, "target");
            }

            target.ValueCount = ValueCount;
            target.Values = new T[ValueCount];
            target.ColumnIndices = new int[ValueCount];

            if (ValueCount != 0)
            {
                Array.Copy(Values, target.Values, ValueCount);
                Buffer.BlockCopy(ColumnIndices, 0, target.ColumnIndices, 0, ValueCount * Constants.SizeOfInt);
                Buffer.BlockCopy(RowPointers, 0, target.RowPointers, 0, RowCount * Constants.SizeOfInt);
            }
        }

        public void CopyTo(DenseColumnMajorMatrixStorage<T> target, bool skipClearing = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (RowCount != target.RowCount || ColumnCount != target.ColumnCount)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, RowCount + "x" + ColumnCount, target.RowCount + "x" + target.ColumnCount);
                throw new ArgumentException(message, "target");
            }

            if (!skipClearing)
            {
                target.Clear();
            }

            if (ValueCount != 0)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    var startIndex = RowPointers[row];
                    var endIndex = row < RowPointers.Length - 1 ? RowPointers[row + 1] : ValueCount;
                    for (var j = startIndex; j < endIndex; j++)
                    {
                        target.At(row, ColumnIndices[j], Values[j]);
                    }
                }
            }
        }
    }
}
