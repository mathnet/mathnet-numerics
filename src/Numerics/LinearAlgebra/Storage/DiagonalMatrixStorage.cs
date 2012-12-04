using System;
using System.Linq;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    public class DiagonalMatrixStorage<T> : MatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        readonly T _zero;
        public readonly T[] Data;

        internal DiagonalMatrixStorage(int rows, int columns, T zero)
            : base(rows, columns)
        {
            _zero = zero;
            Data = new T[Math.Min(rows, columns)];
        }

        internal DiagonalMatrixStorage(int rows, int columns, T zero, T[] data)
            : base(rows, columns)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length != Math.Min(rows, columns))
            {
                throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, Math.Min(rows, columns)));
            }

            _zero = zero;
            Data = data;
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        public override T At(int row, int column)
        {
            return row == column ? Data[row] : _zero;
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public override void At(int row, int column, T value)
        {
            if (row == column)
            {
                Data[row] = value;
            }
            else if (!_zero.Equals(value))
            {
                throw new IndexOutOfRangeException("Cannot set an off-diagonal element in a diagonal matrix.");
            }
        }

        public override bool IsFullyMutable
        {
            get { return false; }
        }

        public override bool IsMutable(int row, int column)
        {
            return row == column;
        }

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public override void Clear(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            var beginInclusive = Math.Max(rowIndex, columnIndex);
            var endExclusive = Math.Min(rowIndex + rowCount, columnIndex + columnCount);
            if (endExclusive > beginInclusive)
            {
                Array.Clear(Data, beginInclusive, endExclusive - beginInclusive);
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
        public override bool Equals(MatrixStorage<T> other)
        {
            var diagonal = other as DiagonalMatrixStorage<T>;
            if (diagonal == null)
            {
                return base.Equals(other);
            }

            // Reject equality when the argument is null or has a different shape.
            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (diagonal.Data.Length != Data.Length)
            {
                return false;
            }

            // If all else fails, perform element wise comparison.
            return !Data.Where((t, i) => !t.Equals(diagonal.Data[i])).Any();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(Data.Length, 25);
            int hash = 17;
            unchecked
            {
                for (var i = 0; i < hashNum; i++)
                {
                    hash = hash * 31 + Data[i].GetHashCode();
                }
            }
            return hash;
        }

        /// <remarks>Parameters assumed to be validated already.</remarks>
        public override void CopyTo(MatrixStorage<T> target, bool skipClearing = false)
        {
            var diagonalTarget = target as DiagonalMatrixStorage<T>;
            if (diagonalTarget != null)
            {
                CopyTo(diagonalTarget);
                return;
            }

            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                CopyTo(denseTarget, skipClearing);
                return;
            }

            var sparseTarget = target as SparseCompressedRowMatrixStorage<T>;
            if (sparseTarget != null)
            {
                CopyTo(sparseTarget, skipClearing);
                return;
            }

            // FALL BACK

            if (!skipClearing)
            {
                target.Clear();
            }

            for (int i = 0; i < Data.Length; i++)
            {
                target.At(i, i, Data[i]);
            }
        }

        void CopyTo(DiagonalMatrixStorage<T> target)
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

            //Buffer.BlockCopy(Data, 0, target.Data, 0, Data.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            Array.Copy(Data, 0, target.Data, 0, Data.Length);
        }

        void CopyTo(SparseCompressedRowMatrixStorage<T> target, bool skipClearing)
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

            for (int i = 0; i < Data.Length; i++)
            {
                target.At(i, i, Data[i]);
            }
        }

        void CopyTo(DenseColumnMajorMatrixStorage<T> target, bool skipClearing)
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

            for (int i = 0; i < Data.Length; i++)
            {
                target.Data[i*(target.RowCount + 1)] = Data[i];
            }
        }

        public override void CopySubMatrixTo(MatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing = false)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                CopySubMatrixTo(denseTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
                return;
            }

            var diagonalTarget = target as DiagonalMatrixStorage<T>;
            if (diagonalTarget != null)
            {
                CopySubMatrixTo(diagonalTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount);
                return;
            }

            var sparseTarget = target as SparseCompressedRowMatrixStorage<T>;
            if (sparseTarget != null)
            {
                CopySubMatrixTo(sparseTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
                return;
            }

            // FALL BACK

            base.CopySubMatrixTo(target, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
        }

        void CopySubMatrixTo(DiagonalMatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (ReferenceEquals(this, target))
            {
                throw new NotSupportedException();
            }

            if (sourceRowIndex - sourceColumnIndex != targetRowIndex - targetColumnIndex)
            {
                if (Data.Any(x => !_zero.Equals(x)))
                {
                    throw new NotSupportedException();
                }

                target.Clear(targetRowIndex, rowCount, targetColumnIndex, columnCount);
                return;
            }

            ValidateSubMatrixRange(target,
                sourceRowIndex, targetRowIndex, rowCount,
                sourceColumnIndex, targetColumnIndex, columnCount);

            var beginInclusive = Math.Max(sourceRowIndex, sourceColumnIndex);
            var endExclusive = Math.Min(sourceRowIndex + rowCount, sourceColumnIndex + columnCount);
            if (endExclusive > beginInclusive)
            {
                var beginTarget = Math.Max(targetRowIndex, targetColumnIndex);
                Array.Copy(Data, beginInclusive, target.Data, beginTarget, endExclusive - beginInclusive);
            }
        }

        void CopySubMatrixTo(DenseColumnMajorMatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            ValidateSubMatrixRange(target,
                sourceRowIndex, targetRowIndex, rowCount,
                sourceColumnIndex, targetColumnIndex, columnCount);

            if (!skipClearing)
            {
                target.Clear(targetRowIndex, rowCount, targetColumnIndex, columnCount);
            }

            if (sourceRowIndex > sourceColumnIndex && sourceColumnIndex + columnCount > sourceRowIndex)
            {
                // column by column, but skip resulting zero columns at the beginning

                int columnInit = sourceRowIndex - sourceColumnIndex;
                int offset = (columnInit + targetColumnIndex) * target.RowCount + targetRowIndex;
                int step = target.RowCount + 1;
                int end = Math.Min(columnCount - columnInit, rowCount) + sourceRowIndex;

                for (int i = sourceRowIndex, j = offset; i < end; i++, j += step)
                {
                    target.Data[j] = Data[i];
                }
            }
            else if (sourceRowIndex < sourceColumnIndex && sourceRowIndex + rowCount > sourceColumnIndex)
            {
                // row by row, but skip resulting zero rows at the beginning

                int rowInit = sourceColumnIndex - sourceRowIndex;
                int offset = targetColumnIndex*target.RowCount + rowInit + targetRowIndex;
                int step = target.RowCount + 1;
                int end = Math.Min(columnCount, rowCount - rowInit) + sourceColumnIndex;

                for (int i = sourceColumnIndex, j = offset; i < end; i++, j += step)
                {
                    target.Data[j] = Data[i];
                }
            }
            else
            {
                int offset = targetColumnIndex*target.RowCount + targetRowIndex;
                int step = target.RowCount + 1;
                var end = Math.Min(columnCount, rowCount) + sourceRowIndex;

                for (int i = sourceRowIndex, j = offset; i < end; i++, j += step)
                {
                    target.Data[j] = Data[i];
                }
            }
        }

        void CopySubMatrixTo(SparseCompressedRowMatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            ValidateSubMatrixRange(target,
                sourceRowIndex, targetRowIndex, rowCount,
                sourceColumnIndex, targetColumnIndex, columnCount);

            if (!skipClearing)
            {
                target.Clear(targetRowIndex, rowCount, targetColumnIndex, columnCount);
            }

            if (sourceRowIndex == sourceColumnIndex)
            {
                for (var i = 0; i < Math.Min(columnCount, rowCount); i++)
                {
                    target.At(i + targetRowIndex, i + targetColumnIndex, Data[sourceRowIndex + i]);
                }
            }
            else if (sourceRowIndex > sourceColumnIndex && sourceColumnIndex + columnCount > sourceRowIndex)
            {
                // column by column, but skip resulting zero columns at the beginning
                int columnInit = sourceRowIndex - sourceColumnIndex;
                for (var i = 0; i < Math.Min(columnCount - columnInit, rowCount); i++)
                {
                    target.At(i + targetRowIndex, columnInit + i + targetColumnIndex, Data[sourceRowIndex + i]);
                }
            }
            else if (sourceRowIndex < sourceColumnIndex && sourceRowIndex + rowCount > sourceColumnIndex)
            {
                // row by row, but skip resulting zero rows at the beginning
                int rowInit = sourceColumnIndex - sourceRowIndex;
                for (var i = 0; i < Math.Min(columnCount, rowCount - rowInit); i++)
                {
                    target.At(rowInit + i + targetRowIndex, i + targetColumnIndex, Data[sourceColumnIndex + i]);
                }
            }

            // else: all zero, nop
        }
    }
}
