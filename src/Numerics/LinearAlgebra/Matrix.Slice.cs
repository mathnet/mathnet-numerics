using System;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// This class is used for slice Matrix by using System.Range.
    /// It is different from System.Range functions in the use of ".." syntax.
    /// Because the when use ".." in array,like double[],the sliced array won't contain the end index.
    /// However,in this Matrix<T>,it will contain the end index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract partial class Matrix<T>
    {
        /// <summary>
        /// Get or Set the SubMatrix by [Range,Range]
        /// For example:
        ///        Matrix<double> mat = Matrix<double>.Build.Dense(10,10);
        ///        mat[1..2,2..3];
        ///        mat[1..2,^9..^1];
        ///        mat[..,2..3];
        ///        mat[1..2,2..3] = a new Matrix has the same dimension with this submatrix;      
        /// </summary>
        /// <param name="rowRange">the Row Range,it must contain more than one Row</param>
        /// <param name="columnRange">the Column Range,it must contain more than one Column</param>
        /// <returns>A SubMatrix</returns>
        public Matrix<T> this[Range rowRange, Range columnRange]
        {
            get
            {
                int rowStartIndex = rowRange.Start.IsFromEnd ? RowCount - rowRange.Start.Value - 1 : rowRange.Start.Value;
                int rowEndIndex = rowRange.End.IsFromEnd ? RowCount - rowRange.End.Value - 1 : rowRange.End.Value;
                int rowCount = rowEndIndex - rowStartIndex + 1;

                if (rowCount <=1 || rowCount > RowCount)
                {
                    throw new ArgumentException($"The argument Row Range:{rowRange} is wrong!");
                }

                int columnStartIndex = columnRange.Start.IsFromEnd ? ColumnCount - columnRange.Start.Value - 1 : columnRange.Start.Value;
                int columnEndIndex = columnRange.End.IsFromEnd ? ColumnCount - columnRange.End.Value - 1 : columnRange.End.Value;
                int columnCount = columnEndIndex - columnStartIndex + 1;

                if (columnCount <= 1 || columnCount > ColumnCount)
                {
                    throw new ArgumentException($"The argument Column Range:{columnRange} is wrong!");
                }

                var result = Build.SameAs(this, rowCount, columnCount);
                Storage.CopySubMatrixTo(result.Storage, rowStartIndex, 0, rowCount, columnStartIndex, 0, columnCount, ExistingData.AssumeZeros);
                return result;
            }

            set
            {
                int rowStartIndex = rowRange.Start.IsFromEnd ? RowCount - rowRange.Start.Value - 1 : rowRange.Start.Value;
                int rowEndIndex = rowRange.End.IsFromEnd ? RowCount - rowRange.End.Value - 1 : rowRange.End.Value;
                int rowCount = rowEndIndex - rowStartIndex + 1;

                if (rowCount <= 1 || rowCount > RowCount)
                {
                    throw new ArgumentException($"The argument Row Range:{rowRange} is wrong!");
                }

                int columnStartIndex = columnRange.Start.IsFromEnd ? ColumnCount - columnRange.Start.Value - 1 : columnRange.Start.Value;
                int columnEndIndex = columnRange.End.IsFromEnd ? ColumnCount - columnRange.End.Value - 1 : columnRange.End.Value;
                int columnCount = columnEndIndex - columnStartIndex + 1;

                if (columnCount <= 1 || columnCount > ColumnCount)
                {
                    throw new ArgumentException($"The argument Column Range:{columnRange} is wrong!");
                }

                value.Storage.CopySubMatrixTo(Storage, 0, rowStartIndex, rowCount, 0, columnStartIndex, columnCount);
            }
        }

        /// <summary>
        /// Get or Set the Column SubVector of the Matrix 
        /// </summary>
        /// <param name="rowRange">the Row Range,it must contain more than one Row</param>
        /// <param name="columnIndex">the column index</param>
        /// <returns>a vector</returns>
        public Vector<T> this[Range rowRange, int columnIndex]
        {
            get
            {
                int rowStartIndex = rowRange.Start.IsFromEnd ? RowCount - rowRange.Start.Value - 1 : rowRange.Start.Value;
                int rowEndIndex = rowRange.End.IsFromEnd ? RowCount - rowRange.End.Value - 1 : rowRange.End.Value;
                int rowCount = rowEndIndex - rowStartIndex + 1;

                if (rowCount <= 1 || rowCount > RowCount)
                {
                    throw new ArgumentException($"The argument Row Range:{rowRange} is wrong!");
                }

                if ((uint)columnIndex >= (uint)ColumnCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(columnIndex));
                }

                var ret = Vector<T>.Build.SameAs(this, rowCount);
                Storage.CopySubColumnTo(ret.Storage, columnIndex, rowStartIndex, 0, rowCount);
                return ret;
            }

            set
            {
                int rowStartIndex = rowRange.Start.IsFromEnd ? RowCount - rowRange.Start.Value - 1 : rowRange.Start.Value;
                int rowEndIndex = rowRange.End.IsFromEnd ? RowCount - rowRange.End.Value - 1 : rowRange.End.Value;
                int rowCount = rowEndIndex - rowStartIndex + 1;

                if (rowCount <= 1 || rowCount > RowCount)
                {
                    throw new ArgumentException($"The argument Row Range:{rowRange} is wrong!");
                }

                if ((uint)columnIndex >= (uint)ColumnCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(columnIndex));
                }

                value.Storage.CopyToSubColumn(Storage, columnIndex, 0, rowStartIndex, rowCount);
            }
        }

        /// <summary>
        /// Get or Set the row SubVector of the Matrix 
        /// </summary>
        /// <param name="rowIndex">the column index</param>
        /// <param name="columnRange">the Column Range,it must contain more than one Column</param>
        /// <returns>a vector</returns>
        public Vector<T> this[int rowIndex, Range columnRange]
        {
            get
            {
                int columnStartIndex = columnRange.Start.IsFromEnd ? ColumnCount - columnRange.Start.Value - 1 : columnRange.Start.Value;
                int columnEndIndex = columnRange.End.IsFromEnd ? ColumnCount - columnRange.End.Value - 1 : columnRange.End.Value;
                int columnCount = columnEndIndex - columnStartIndex + 1;

                if (columnCount <= 1 || columnCount > ColumnCount)
                {
                    throw new ArgumentException($"The argument Column Range:{columnRange} is wrong!");
                }

                if ((uint)rowIndex >= (uint)RowCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(rowIndex));
                }

                var ret = Vector<T>.Build.SameAs(this, columnCount);
                Storage.CopySubRowTo(ret.Storage, rowIndex, columnStartIndex, 0, columnCount);
                return ret;
            }

            set
            {
                int columnStartIndex = columnRange.Start.IsFromEnd ? ColumnCount - columnRange.Start.Value - 1 : columnRange.Start.Value;
                int columnEndIndex = columnRange.End.IsFromEnd ? ColumnCount - columnRange.End.Value - 1 : columnRange.End.Value;
                int columnCount = columnEndIndex - columnStartIndex + 1;

                if (columnCount <= 1 || columnCount > ColumnCount)
                {
                    throw new ArgumentException($"The argument Column Range:{columnRange} is wrong!");
                }

                if ((uint)rowIndex >= (uint)RowCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(rowIndex));
                }

                value.Storage.CopyToSubRow(Storage, rowIndex, 0, columnStartIndex, columnCount);
            }
        }
    }
}
