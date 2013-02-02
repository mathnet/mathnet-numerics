using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    // ReSharper disable UnusedParameter.Local
    public partial class MatrixStorage<T>
    {
        void ValidateRange(int row, int column)
        {
            if (row < 0 || row >= RowCount)
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if (column < 0 || column >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("column");
            }
        }

        void ValidateSubMatrixRange(MatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            if (rowCount < 1)
            {
                throw new ArgumentOutOfRangeException("rowCount", Resources.ArgumentMustBePositive);
            }

            if (columnCount < 1)
            {
                throw new ArgumentOutOfRangeException("columnCount", Resources.ArgumentMustBePositive);
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
        }

        void ValidateSubRowRange(VectorStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            if (columnCount < 1)
            {
                throw new ArgumentOutOfRangeException("columnCount", Resources.ArgumentMustBePositive);
            }

            // Verify Source

            if (rowIndex >= RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (sourceColumnIndex >= ColumnCount || sourceColumnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("sourceColumnIndex");
            }

            if (sourceColumnIndex + columnCount > ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnCount");
            }

            // Verify Target

            if (targetColumnIndex >= target.Length || targetColumnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("targetColumnIndex");
            }

            if (targetColumnIndex + columnCount > target.Length)
            {
                throw new ArgumentOutOfRangeException("columnCount");
            }
        }

        void ValidateSubColumnRange(VectorStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount)
        {
            if (rowCount < 1)
            {
                throw new ArgumentOutOfRangeException("rowCount", Resources.ArgumentMustBePositive);
            }

            // Verify Source

            if (columnIndex >= ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (sourceRowIndex >= RowCount || sourceRowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("sourceRowIndex");
            }

            if (sourceRowIndex + rowCount > RowCount)
            {
                throw new ArgumentOutOfRangeException("rowCount");
            }

            // Verify Target

            if (targetRowIndex >= target.Length || targetRowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("targetRowIndex");
            }

            if (targetRowIndex + rowCount > target.Length)
            {
                throw new ArgumentOutOfRangeException("rowCount");
            }
        }
    }
    // ReSharper restore UnusedParameter.Local
}
