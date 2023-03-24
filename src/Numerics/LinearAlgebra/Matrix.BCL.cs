// <copyright file="Matrix.BCL.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra
{
    [DebuggerDisplay("Matrix {RowCount}x{ColumnCount}")]

    public abstract partial class Matrix<T>
    {
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Matrix<T> other)
        {
            return other != null && Storage.Equals(other.Storage);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Matrix<T> other && Storage.Equals(other.Storage);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Storage.GetHashCode();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Returns a string that describes the type, dimensions and shape of this matrix.
        /// </summary>
        public virtual string ToTypeString()
        {
            return FormattableString.Invariant($"{GetType().Name} {RowCount}x{ColumnCount}-{typeof(T).Name}");
        }

        /// <summary>
        /// Returns a string 2D array that summarizes the content of this matrix.
        /// </summary>
        public string[,] ToMatrixStringArray(int upperRows, int lowerRows, int leftColumns, int rightColumns,
            string horizontalEllipsis, string verticalEllipsis, string diagonalEllipsis, Func<T, string> formatValue)
        {
            upperRows = Math.Max(upperRows, 1);
            lowerRows = Math.Max(lowerRows, 0);
            leftColumns = Math.Max(leftColumns, 1);
            rightColumns = Math.Max(rightColumns, 0);

            int upper = RowCount <= upperRows ? RowCount : upperRows;
            int lower = RowCount <= upperRows ? 0 : RowCount <= upperRows + lowerRows ? RowCount - upperRows : lowerRows;
            bool rowEllipsis = RowCount > upper + lower;
            int rows = rowEllipsis ? upper + lower + 1 : upper + lower;

            int left = ColumnCount <= leftColumns ? ColumnCount : leftColumns;
            int right = ColumnCount <= leftColumns ? 0 : ColumnCount <= leftColumns + rightColumns ? ColumnCount - leftColumns : rightColumns;
            bool colEllipsis = ColumnCount > left + right;
            int cols = colEllipsis ? left + right + 1 : left + right;

            var array = new string[rows, cols];
            for (int i = 0; i < upper; i++)
            {
                for (int j = 0; j < left; j++)
                {
                    array[i, j] = formatValue(At(i, j));
                }
                int colOffset = left;
                if (colEllipsis)
                {
                    array[i, left] = horizontalEllipsis;
                    colOffset++;
                }
                for (int j = 0; j < right; j++)
                {
                    array[i, colOffset + j] = formatValue(At(i, ColumnCount - right + j));
                }
            }
            int rowOffset = upper;
            if (rowEllipsis)
            {
                for (int j = 0; j < left; j++)
                {
                    array[upper, j] = verticalEllipsis;
                }
                int colOffset = left;
                if (colEllipsis)
                {
                    array[upper, left] = diagonalEllipsis;
                    colOffset++;
                }
                for (int j = 0; j < right; j++)
                {
                    array[upper, colOffset + j] = verticalEllipsis;
                }
                rowOffset++;
            }
            for (int i = 0; i < lower; i++)
            {
                for (int j = 0; j < left; j++)
                {
                    array[rowOffset + i, j] = formatValue(At(RowCount - lower + i, j));
                }
                int colOffset = left;
                if (colEllipsis)
                {
                    array[rowOffset + i, left] = horizontalEllipsis;
                    colOffset++;
                }
                for (int j = 0; j < right; j++)
                {
                    array[rowOffset + i, colOffset + j] = formatValue(At(RowCount - lower + i, ColumnCount - right + j));
                }
            }
            return array;
        }

        /// <summary>
        /// Returns a string 2D array that summarizes the content of this matrix.
        /// </summary>
        public string[,] ToMatrixStringArray(int upperRows, int lowerRows, int minLeftColumns, int rightColumns, int maxWidth, int padding,
            string horizontalEllipsis, string verticalEllipsis, string diagonalEllipsis, Func<T, string> formatValue)
        {
            upperRows = Math.Max(upperRows, 1);
            lowerRows = Math.Max(lowerRows, 0);
            minLeftColumns = Math.Max(minLeftColumns, 1);
            maxWidth = Math.Max(maxWidth, 12);

            int upper = RowCount <= upperRows ? RowCount : upperRows;
            int lower = RowCount <= upperRows ? 0 : RowCount <= upperRows + lowerRows ? RowCount - upperRows : lowerRows;
            bool rowEllipsis = RowCount > upper + lower;
            int rows = rowEllipsis ? upper + lower + 1 : upper + lower;

            int left = ColumnCount <= minLeftColumns ? ColumnCount : minLeftColumns;
            int right = ColumnCount <= minLeftColumns ? 0 : ColumnCount <= minLeftColumns + rightColumns ? ColumnCount - minLeftColumns : rightColumns;

            var columnsLeft = new List<Tuple<int, string[]>>();
            for (int j = 0; j < left; j++)
            {
                columnsLeft.Add(FormatColumn(j, rows, upper, lower, rowEllipsis, verticalEllipsis, formatValue));
            }

            var columnsRight = new List<Tuple<int, string[]>>();
            for (int j = 0; j < right; j++)
            {
                columnsRight.Add(FormatColumn(ColumnCount - right + j, rows, upper, lower, rowEllipsis, verticalEllipsis, formatValue));
            }

            int chars = columnsLeft.Sum(t => t.Item1 + padding) + columnsRight.Sum(t => t.Item1 + padding);
            for (int j = left; j < ColumnCount - right; j++)
            {
                var candidate = FormatColumn(j, rows, upper, lower, rowEllipsis, verticalEllipsis, formatValue);
                chars += candidate.Item1 + padding;
                if (chars > maxWidth)
                {
                    break;
                }
                columnsLeft.Add(candidate);
            }

            int cols = columnsLeft.Count + columnsRight.Count;
            bool colEllipsis = ColumnCount > cols;
            if (colEllipsis)
            {
                cols++;
            }

            var array = new string[rows, cols];
            int colIndex = 0;
            foreach (var column in columnsLeft)
            {
                var columnItem2 = column.Item2;
                for (int i = 0; i < column.Item2.Length; i++)
                {
                    array[i, colIndex] = columnItem2[i];
                }
                colIndex++;
            }
            if (colEllipsis)
            {
                int rowIndex = 0;
                for (var row = 0; row < upper; row++)
                {
                    array[rowIndex++, colIndex] = horizontalEllipsis;
                }
                if (rowEllipsis)
                {
                    array[rowIndex++, colIndex] = diagonalEllipsis;
                }
                for (var row = RowCount - lower; row < RowCount; row++)
                {
                    array[rowIndex++, colIndex] = horizontalEllipsis;
                }
                colIndex++;
            }
            foreach (var column in columnsRight)
            {
                var columnItem2 = column.Item2;
                for (int i = 0; i < column.Item2.Length; i++)
                {
                    array[i, colIndex] = columnItem2[i];
                }
                colIndex++;
            }
            return array;
        }

        Tuple<int, string[]> FormatColumn(int column, int height, int upper, int lower, bool withEllipsis, string ellipsis, Func<T, string> formatValue)
        {
            var c = new string[height];
            int index = 0;
            for (var row = 0; row < upper; row++)
            {
                c[index++] = formatValue(At(row, column));
            }
            if (withEllipsis)
            {
                c[index++] = "";
            }
            for (var row = RowCount - lower; row < RowCount; row++)
            {
                c[index++] = formatValue(At(row, column));
            }

            int w = height != 0
                ? c.Max(x => x.Length)
                : 0;
            if (withEllipsis)
            {
                c[upper] = ellipsis;
            }
            return new Tuple<int, string[]>(w, c);
        }

        static string FormatStringArrayToString(string[,] array, string columnSeparator, string rowSeparator)
        {
            const string emptyString = "[empty]";
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);

            var widths = new int[cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    widths[j] = Math.Max(widths[j], array[i, j].Length);
                }
            }

            var sb = new StringBuilder();
            if (rows > 0)
            {
                for (int i = 0; i < rows; i++)
                {
                    if (cols > 0)
                    {
                        sb.Append(array[i, 0].PadLeft(widths[0]));
                        for (int j = 1; j < cols; j++)
                        {
                            sb.Append(columnSeparator);
                            sb.Append(array[i, j].PadLeft(widths[j]));
                        }
                    }
                    else
                    {
                        sb.Append(emptyString);
                    }

                    sb.Append(rowSeparator);
                }
            }
            else
            {
                if (cols > 0)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        sb.Append(emptyString);
                        if (j != cols - 1)
                        {
                            sb.Append(columnSeparator);
                        }
                    }
                }
                else
                {
                    sb.Append(emptyString);
                }
            }

            return sb.ToString();
        }

        public string ToMatrixString(int upperRows, int lowerRows, int leftColumns, int rightColumns,
            string horizontalEllipsis, string verticalEllipsis, string diagonalEllipsis,
            string columnSeparator, string rowSeparator, Func<T, string> formatValue)
        {
            var array = ToMatrixStringArray(
                upperRows,
                lowerRows,
                leftColumns,
                rightColumns,
                horizontalEllipsis,
                verticalEllipsis,
                diagonalEllipsis,
                formatValue);

            return FormatStringArrayToString(
                array,
                columnSeparator,
                rowSeparator);
        }

        public string ToMatrixString(int upperRows, int lowerRows, int minLeftColumns, int rightColumns, int maxWidth,
            string horizontalEllipsis, string verticalEllipsis, string diagonalEllipsis,
            string columnSeparator, string rowSeparator, Func<T, string> formatValue)
        {
            var array = ToMatrixStringArray(
                upperRows,
                lowerRows,
                minLeftColumns,
                rightColumns,
                maxWidth,
                columnSeparator.Length,
                horizontalEllipsis,
                verticalEllipsis,
                diagonalEllipsis,
                formatValue);

            return FormatStringArrayToString(
                array,
                columnSeparator,
                rowSeparator);
        }

        /// <summary>
        /// Returns a string that summarizes the content of this matrix.
        /// </summary>
        public string ToMatrixString(int maxRows, int maxColumns, string format = null, IFormatProvider provider = null)
        {
            if (format == null)
            {
                format = "G6";
            }

            int bottom = maxRows > 4 ? 2 : 0;
            int right = maxColumns > 4 ? 2 : 0;
            return ToMatrixString(maxRows - bottom, bottom, maxColumns - right, right, "..", "..", "..", "  ", Environment.NewLine, x => x.ToString(format, provider));
        }

        /// <summary>
        /// Returns a string that summarizes the content of this matrix.
        /// </summary>
        public string ToMatrixString(string format = null, IFormatProvider provider = null)
        {
            if (format == null)
            {
                format = "G6";
            }

            return ToMatrixString(8, 4, 5, 2, 76, "..", "..", "..", "  ", Environment.NewLine, x => x.ToString(format, provider));
        }

        /// <summary>
        /// Returns a string that summarizes this matrix.
        /// </summary>
        public string ToString(int maxRows, int maxColumns, string format = null, IFormatProvider formatProvider = null)
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToMatrixString(maxRows, maxColumns, format, formatProvider));
        }

        /// <summary>
        /// Returns a string that summarizes this matrix.
        /// The maximum number of cells can be configured in the <see cref="Control"/> class.
        /// </summary>
        public sealed override string ToString()
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToMatrixString());
        }

        /// <summary>
        /// Returns a string that summarizes this matrix.
        /// The maximum number of cells can be configured in the <see cref="Control"/> class.
        /// The format string is ignored.
        /// </summary>
        public string ToString(string format = null, IFormatProvider formatProvider = null)
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToMatrixString(format, formatProvider));
        }
    }
}
