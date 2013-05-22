// <copyright file="Matrix.BCL.cs" company="Math.NET">
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
using System.Diagnostics;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra.Generic
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
            var other = obj as Matrix<T>;
            return other != null && Storage.Equals(other.Storage);
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

#if !PORTABLE

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

#endif

        /// <summary>
        /// Returns a <see cref="System.String"/> that describes the type, dimensions and shape of this matrix.
        /// </summary>
        public virtual string ToTypeString()
        {
            return string.Format("{0} {1}x{2}-{3}", GetType().Name, RowCount, ColumnCount, typeof(T).Name);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the content of this matrix.
        /// </summary>
        public string ToMatrixString(int maxRows, int maxColumns, IFormatProvider provider = null)
        {
            return ToMatrixString(maxRows, maxColumns, 12, "G6", provider);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the content of this matrix.
        /// </summary>
        public string ToMatrixString(int maxRows, int maxColumns, int padding, string format = null, IFormatProvider provider = null)
        {
            int rowN = RowCount <= maxRows ? RowCount : maxRows < 3 ? maxRows : maxRows - 1;
            bool rowDots = maxRows < RowCount;
            bool rowLast = rowDots && maxRows > 2;

            int colN = ColumnCount <= maxColumns ? ColumnCount : maxColumns < 3 ? maxColumns : maxColumns - 1;
            bool colDots = maxColumns < ColumnCount;
            bool colLast = colDots && maxColumns > 2;

            const string separator = " ";
            const string dots = "...";
            string pdots = "...".PadLeft(padding);

            if (format == null)
            {
                format = "G8";
            }

            var stringBuilder = new StringBuilder();

            for (var row = 0; row < rowN; row++)
            {
                if (row > 0)
                {
                    stringBuilder.Append(Environment.NewLine);
                }
                stringBuilder.Append(At(row, 0).ToString(format, provider).PadLeft(padding));
                for (var column = 1; column < colN; column++)
                {
                    stringBuilder.Append(separator);
                    stringBuilder.Append(At(row, column).ToString(format, provider).PadLeft(padding));
                }
                if (colDots)
                {
                    stringBuilder.Append(separator);
                    stringBuilder.Append(dots);
                    if (colLast)
                    {
                        stringBuilder.Append(separator);
                        stringBuilder.Append(At(row, ColumnCount - 1).ToString(format, provider).PadLeft(12));
                    }
                }
            }

            if (rowDots)
            {
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(pdots);
                for (var column = 1; column < colN; column++)
                {
                    stringBuilder.Append(separator);
                    stringBuilder.Append(pdots);
                }
                if (colDots)
                {
                    stringBuilder.Append(separator);
                    stringBuilder.Append(dots);
                    if (colLast)
                    {
                        stringBuilder.Append(separator);
                        stringBuilder.Append(pdots);
                    }
                }
            }

            if (rowLast)
            {
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(At(RowCount - 1, 0).ToString(format, provider).PadLeft(padding));
                for (var column = 1; column < colN; column++)
                {
                    stringBuilder.Append(separator);
                    stringBuilder.Append(At(RowCount - 1, column).ToString(format, provider).PadLeft(padding));
                }
                if (colDots)
                {
                    stringBuilder.Append(separator);
                    stringBuilder.Append(dots);
                    if (colLast)
                    {
                        stringBuilder.Append(separator);
                        stringBuilder.Append(At(RowCount - 1, ColumnCount - 1).ToString(format, provider).PadLeft(padding));
                    }
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that summarizes this matrix.
        /// </summary>
        public string ToString(int maxRows, int maxColumns, IFormatProvider provider = null)
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToMatrixString(maxRows, maxColumns, provider));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that summarizes this matrix.
        /// The maximum number of cells can be configured in the <see cref="Control"/> class.
        /// </summary>
        public override sealed string ToString()
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToMatrixString(Control.MaxToStringRows, Control.MaxToStringColumns, null));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that summarizes this matrix.
        /// The maximum number of cells can be configured in the <see cref="Control"/> class.
        /// The format string is ignored.
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToMatrixString(Control.MaxToStringRows, Control.MaxToStringColumns, formatProvider));
        }
    }
}
