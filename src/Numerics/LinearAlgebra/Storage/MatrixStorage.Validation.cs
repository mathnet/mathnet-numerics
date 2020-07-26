// <copyright file="MatrixStorage.Validation.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    // ReSharper disable UnusedParameter.Local

    public partial class MatrixStorage<T>
    {
        void ValidateRange(int row, int column)
        {
            if ((uint)row >= (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if ((uint)column >= (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }
        }

        void ValidateSubMatrixRange<TU>(MatrixStorage<TU> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if (rowCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount), "Value must be positive.");
            }

            if (columnCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount), "Value must be positive.");
            }

            // Verify Source

            if ((uint)sourceRowIndex >= (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceRowIndex));
            }

            if ((uint)sourceColumnIndex >= (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceColumnIndex));
            }

            var sourceRowMax = sourceRowIndex + rowCount;
            var sourceColumnMax = sourceColumnIndex + columnCount;

            if (sourceRowMax > RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount));
            }

            if (sourceColumnMax > ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount));
            }

            // Verify Target

            if ((uint)targetRowIndex >= (uint)target.RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(targetRowIndex));
            }

            if ((uint)targetColumnIndex >= (uint)target.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(targetColumnIndex));
            }

            var targetRowMax = targetRowIndex + rowCount;
            var targetColumnMax = targetColumnIndex + columnCount;

            if (targetRowMax > target.RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount));
            }

            if (targetColumnMax > target.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount));
            }
        }

        void ValidateRowRange<TU>(VectorStorage<TU> target, int rowIndex)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if ((uint)rowIndex >= (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            if (ColumnCount != target.Length)
            {
                throw new ArgumentException("Matrix row dimensions must agree.", nameof(target));
            }
        }

        void ValidateColumnRange<TU>(VectorStorage<TU> target, int columnIndex)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if ((uint)columnIndex >= (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            if (RowCount != target.Length)
            {
                throw new ArgumentException("Matrix column dimensions must agree.", nameof(target));
            }
        }

        void ValidateSubRowRange<TU>(VectorStorage<TU> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if (columnCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount), "Value must be positive.");
            }

            // Verify Source

            if ((uint)rowIndex >= (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            if ((uint)sourceColumnIndex >= (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceColumnIndex));
            }

            if (sourceColumnIndex + columnCount > ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount));
            }

            // Verify Target

            if ((uint)targetColumnIndex >= (uint)target.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(targetColumnIndex));
            }

            if (targetColumnIndex + columnCount > target.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount));
            }
        }

        void ValidateSubColumnRange<TU>(VectorStorage<TU> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if (rowCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount), "Value must be positive.");
            }

            // Verify Source

            if ((uint)columnIndex >= (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            if ((uint)sourceRowIndex >= (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceRowIndex));
            }

            if (sourceRowIndex + rowCount > RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount));
            }

            // Verify Target

            if ((uint)targetRowIndex >= (uint)target.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(targetRowIndex));
            }

            if (targetRowIndex + rowCount > target.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount));
            }
        }
    }

    // ReSharper restore UnusedParameter.Local
}
