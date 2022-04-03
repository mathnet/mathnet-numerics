// <copyright file="Vector.BCL.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.LinearAlgebra
{
    [DebuggerDisplay("Vector {" + nameof(Count) + "}")]
    public abstract partial class Vector<T>
    {
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///    <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Vector<T> other)
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
        public sealed override bool Equals(object obj)
        {
            return obj is Vector<T> other && Storage.Equals(other.Storage);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public sealed override int GetHashCode()
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

        int IList<T>.IndexOf(T item)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (At(i).Equals(item))
                    return i;
            }
            return -1;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var x in this)
            {
                if (x.Equals(item))
                    return true;
            }
            return false;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            Storage.CopySubVectorTo(new DenseVectorStorage<T>(array.Length, array), 0, arrayIndex, Count);
        }

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => true;

        object IList.this[int index]
        {
            get => Storage[index];
            set => Storage[index] = (T) value;
        }

        int IList.IndexOf(object value)
        {
            if (!(value is T))
            {
                return -1;
            }

            return ((IList<T>) this).IndexOf((T) value);
        }

        bool IList.Contains(object value)
        {
            if (!(value is T))
            {
                return false;
            }

            return ((ICollection<T>) this).Contains((T) value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => Storage;

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException("Array must have exactly one dimension (and not be null).", nameof(array));
            }

            Storage.CopySubVectorTo(new DenseVectorStorage<T>(array.Length, (T[]) array), 0, index, Count);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Returns a string that describes the type, dimensions and shape of this vector.
        /// </summary>
        public virtual string ToTypeString()
        {
            return FormattableString.Invariant($"{GetType().Name} {Count}-{typeof(T).Name}");
        }

        public string[,] ToVectorStringArray(int maxPerColumn, int maxCharactersWidth, int padding, string ellipsis, Func<T, string> formatValue)
        {
            // enforce minima to avoid pathetic cases
            maxPerColumn = Math.Max(maxPerColumn, 3);
            maxCharactersWidth = Math.Max(maxCharactersWidth, 16);

            var columns = new List<Tuple<int, string[]>>();
            int chars = 0;
            int offset = 0;
            while (offset < Count)
            {
                // full column
                int height = Math.Min(maxPerColumn, Count - offset);
                var candidate = FormatCompleteColumn(offset, height, formatValue);
                chars += candidate.Item1 + padding;
                if (chars > maxCharactersWidth && offset > 0)
                {
                    break;
                }
                columns.Add(candidate);
                offset += height;
            }
            if (offset < Count)
            {
                // we're not done yet, but adding the last column has failed
                // --> make the last column partial
                var last = columns[columns.Count - 1];
                var c = last.Item2;
                c[c.Length - 2] = ellipsis;
                c[c.Length - 1] = formatValue(At(Count - 1));
            }

            int rows = columns[0].Item2.Length;
            int cols = columns.Count;
            var array = new string[rows, cols];
            int colIndex = 0;
            foreach (var column in columns)
            {
                var columnItem2 = column.Item2;
                for (int k = 0; k < column.Item2.Length; k++)
                {
                    array[k, colIndex] = columnItem2[k];
                }
                for (int k = column.Item2.Length; k < rows; k++)
                {
                    array[k, colIndex] = "";
                }
                colIndex++;
            }
            return array;
        }

        static string FormatStringArrayToString(string[,] array, string columnSeparator, string rowSeparator)
        {
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
            for (int i = 0; i < rows; i++)
            {
                sb.Append(array[i, 0].PadLeft(widths[0]));
                for (int j = 1; j < cols; j++)
                {
                    sb.Append(columnSeparator);
                    sb.Append(array[i, j].PadLeft(widths[j]));
                }
                sb.Append(rowSeparator);
            }
            return sb.ToString();
        }

        Tuple<int, string[]> FormatCompleteColumn(int offset, int height, Func<T, string> formatValue)
        {
            var c = new string[height];
            int index = 0;
            for (var k = 0; k < height; k++)
            {
                c[index++] = formatValue(At(offset + k));
            }
            int w = c.Max(x => x.Length);
            return new Tuple<int, string[]>(w, c);
        }

        /// <summary>
        /// Returns a string that represents the content of this vector, column by column.
        /// </summary>
        /// <param name="maxPerColumn">Maximum number of entries and thus lines per column. Typical value: 12; Minimum: 3.</param>
        /// <param name="maxCharactersWidth">Maximum number of characters per line over all columns. Typical value: 80; Minimum: 16.</param>
        /// <param name="ellipsis">Character to use to print if there is not enough space to print all entries. Typical value: "..".</param>
        /// <param name="columnSeparator">Character to use to separate two columns on a line. Typical value: "  " (2 spaces).</param>
        /// <param name="rowSeparator">Character to use to separate two rows/lines. Typical value: Environment.NewLine.</param>
        /// <param name="formatValue">Function to provide a string for any given entry value.</param>
        public string ToVectorString(int maxPerColumn, int maxCharactersWidth, string ellipsis, string columnSeparator, string rowSeparator, Func<T, string> formatValue)
        {
            return FormatStringArrayToString(
                ToVectorStringArray(maxPerColumn, maxCharactersWidth, columnSeparator.Length, ellipsis, formatValue),
                columnSeparator, rowSeparator);
        }

        /// <summary>
        /// Returns a string that represents the content of this vector, column by column.
        /// </summary>
        /// <param name="maxPerColumn">Maximum number of entries and thus lines per column. Typical value: 12; Minimum: 3.</param>
        /// <param name="maxCharactersWidth">Maximum number of characters per line over all columns. Typical value: 80; Minimum: 16.</param>
        /// <param name="format">Floating point format string. Can be null. Default value: G6.</param>
        /// <param name="provider">Format provider or culture. Can be null.</param>
        public string ToVectorString(int maxPerColumn, int maxCharactersWidth, string format = null, IFormatProvider provider = null)
        {
            if (format == null)
            {
                format = "G6";
            }

            return ToVectorString(maxPerColumn, maxCharactersWidth, "..", "  ", Environment.NewLine, x => x.ToString(format, provider));
        }

        /// <summary>
        /// Returns a string that represents the content of this vector, column by column.
        /// </summary>
        /// <param name="format">Floating point format string. Can be null. Default value: G6.</param>
        /// <param name="provider">Format provider or culture. Can be null.</param>
        public string ToVectorString(string format = null, IFormatProvider provider = null)
        {
            if (format == null)
            {
                format = "G6";
            }

            return ToVectorString(12, 80, "..", "  ", Environment.NewLine, x => x.ToString(format, provider));
        }

        /// <summary>
        /// Returns a string that summarizes this vector, column by column and with a type header.
        /// </summary>
        /// <param name="maxPerColumn">Maximum number of entries and thus lines per column. Typical value: 12; Minimum: 3.</param>
        /// <param name="maxCharactersWidth">Maximum number of characters per line over all columns. Typical value: 80; Minimum: 16.</param>
        /// <param name="format">Floating point format string. Can be null. Default value: G6.</param>
        /// <param name="provider">Format provider or culture. Can be null.</param>
        public string ToString(int maxPerColumn, int maxCharactersWidth, string format = null, IFormatProvider provider = null)
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToVectorString(maxPerColumn, maxCharactersWidth, format, provider));
        }

        /// <summary>
        /// Returns a string that summarizes this vector.
        /// The maximum number of cells can be configured in the <see cref="Control"/> class.
        /// </summary>
        public sealed override string ToString()
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToVectorString());
        }

        /// <summary>
        /// Returns a string that summarizes this vector.
        /// The maximum number of cells can be configured in the <see cref="Control"/> class.
        /// The format string is ignored.
        /// </summary>
        public string ToString(string format = null, IFormatProvider formatProvider = null)
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToVectorString(format, formatProvider));
        }
    }
}
