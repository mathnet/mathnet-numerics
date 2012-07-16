using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    public abstract class MatrixStorage<T> where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        public readonly int RowCount;
        public readonly int ColumnCount;

        protected MatrixStorage(int rowCount, int columnCount)
        {
            if (rowCount <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.MatrixRowsMustBePositive);
            }

            if (columnCount <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.MatrixColumnsMustBePositive);
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
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
        public abstract T At(int row, int column);

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        /// <param name="row"> The row of the element. </param>
        /// <param name="column"> The column of the element. </param>
        /// <param name="value"> The value to set the element to. </param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks.</remarks>
        public abstract void At(int row, int column, T value);

        public abstract void Clear();
    }
}
