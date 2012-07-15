using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    internal class DenseColumnMajorMatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        public readonly int RowCount;
        public readonly int ColumnCount;

        public readonly T[] Data;

        internal DenseColumnMajorMatrixStorage(int rows, int columns)
        {
            RowCount = rows;
            ColumnCount = columns;

            Data = new T[rows * columns];
        }

        internal DenseColumnMajorMatrixStorage(int rows, int columns, T[] data)
        {
            RowCount = rows;
            ColumnCount = columns;

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
            return Data[(column * RowCount) + row];
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public void At(int row, int column, T value)
        {
            Data[(column * RowCount) + row] = value;
        }

        public void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public void CopyTo(DenseColumnMajorMatrixStorage<T> target)
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

        public void CopySubMatrixTo(DenseColumnMajorMatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            if (rowCount < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rowCount");
            }

            if (columnCount < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columnCount");
            }

            // Verify Source

            if (sourceRowIndex >= RowCount || sourceRowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("sourceRowIndex");
            }

            if (sourceColumnIndex >= ColumnCount || sourceColumnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("sourceColumnIndex");
            }

            var sourceRowMax = sourceRowIndex + rowCount;
            var sourceColumnMax = sourceColumnIndex + columnCount;

            if (sourceRowMax > RowCount)
            {
                throw new ArgumentOutOfRangeException("rowCount");
            }

            if (sourceColumnMax > ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnCount");
            }

            // Verify Target

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (ReferenceEquals(this, target))
            {
                throw new NotSupportedException();
            }

            if (targetRowIndex >= target.RowCount || targetRowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("targetRowIndex");
            }

            if (targetColumnIndex >= target.ColumnCount || targetColumnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("targetColumnIndex");
            }

            var targetRowMax = targetRowIndex + rowCount;
            var targetColumnMax = targetColumnIndex + columnCount;

            if (targetRowMax > target.RowCount)
            {
                throw new ArgumentOutOfRangeException("rowCount");
            }

            if (targetColumnMax > target.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnCount");
            }

            // Copy

            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnMax; j++, jj++)
            {
                //Buffer.BlockCopy(Data, j*RowCount + sourceRowIndex, target.Data, jj*target.RowCount + targetRowIndex, rowCount * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
                Array.Copy(Data, j*RowCount + sourceRowIndex, target.Data, jj*target.RowCount + targetRowIndex, rowCount);
            }
        }
    }
}
