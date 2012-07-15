using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    internal class SparseDiagonalMatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        public readonly int RowCount;
        public readonly int ColumnCount;
        readonly T _zero;

        public readonly T[] Data;

        internal SparseDiagonalMatrixStorage(int rows, int columns, T zero)
        {
            RowCount = rows;
            ColumnCount = columns;
            _zero = zero;

            Data = new T[Math.Min(rows, columns)];
        }

        internal SparseDiagonalMatrixStorage(int rows, int columns, T zero, T[] data)
        {
            RowCount = rows;
            ColumnCount = columns;
            _zero = zero;

            Data = data;
        }

        /// <summary>
        /// Gets or sets the value at the given row and column, with range checking.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <value>The value to get or set.</value>
        /// <remarks>This method is ranged checked. <see cref="At(int,int)"/> and <see cref="At(int,int,T)"/>
        /// to get and set values without range checking.</remarks>
        public T this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= RowCount)
                {
                    throw new ArgumentOutOfRangeException("row");
                }

                if (column < 0 || column >= ColumnCount)
                {
                    throw new ArgumentOutOfRangeException("column");
                }

                return At(row, column);
            }

            set
            {
                if (row < 0 || row >= RowCount)
                {
                    throw new ArgumentOutOfRangeException("row");
                }

                if (column < 0 || column >= ColumnCount)
                {
                    throw new ArgumentOutOfRangeException("column");
                }

                At(row, column, value);
            }
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        public T At(int row, int column)
        {
            return row == column ? Data[row] : _zero;
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public void At(int row, int column, T value)
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

        public void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public void CopyTo(SparseDiagonalMatrixStorage<T> target)
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

        public void CopyTo(DenseColumnMajorMatrixStorage<T> target, bool targetKnownClear = false)
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

            if (!targetKnownClear)
            {
                target.Clear();
            }

            for (int i = 0; i < Data.Length; i++)
            {
                target.Data[i*(target.RowCount + 1)] = Data[i];
            }
        }
    }
}
