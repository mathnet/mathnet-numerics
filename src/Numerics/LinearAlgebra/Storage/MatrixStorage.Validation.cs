// <copyright file="MatrixStorage.Validation.cs" company="Math.NET">
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

        void ValidateRowRange(VectorStorage<T> target, int rowIndex)
        {
            if (rowIndex >= RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (ColumnCount != target.Length)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "target");
            }
        }

        void ValidateColumnRange(VectorStorage<T> target, int columnIndex)
        {
            if (columnIndex >= ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (RowCount != target.Length)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension, "target");
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
