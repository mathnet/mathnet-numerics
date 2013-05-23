// <copyright file="Vector.BCL.cs" company="Math.NET">
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Generic
{
    [DebuggerDisplay("Vector {Count}")]
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
        public override sealed bool Equals(object obj)
        {
            var other = obj as Vector<T>;
            return other != null && Storage.Equals(other.Storage);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override sealed int GetHashCode()
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

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

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
            // Do NOT convert this loop to LINQ (since LINQ would redirect to this very method)!
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
                throw new ArgumentNullException("array");
            }

            Storage.CopySubVectorTo(new DenseVectorStorage<T>(array.Length, array), 0, arrayIndex, Count);
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        bool IList.IsFixedSize
        {
            get { return true; }
        }

        object IList.this[int index]
        {
            get { return Storage[index]; }
            set { Storage[index] = (T)value; }
        }

        int IList.IndexOf(object value)
        {
            if (!(value is T))
            {
                return -1;
            }

            return ((IList<T>)this).IndexOf((T)value);
        }

        bool IList.Contains(object value)
        {
            if (!(value is T))
            {
                return false;
            }

            return ((ICollection<T>)this).Contains((T)value);
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

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return null; }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(Resources.ArgumentSingleDimensionArray, "array");
            }

            Storage.CopySubVectorTo(new DenseVectorStorage<T>(array.Length, (T[])array), 0, index, Count);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Storage.Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that describes the type, dimensions and shape of this vector.
        /// </summary>
        public virtual string ToTypeString()
        {
            return string.Format("{0} {1}-{2}", GetType().Name, Count, typeof(T).Name);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the content of this vector, row by row.
        /// </summary>
        public string ToVectorString(int maxLines, int maxPerLine, IFormatProvider provider = null)
        {
            return ToVectorString(maxLines, maxPerLine, 12, "G6", provider);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the content of this vector, row by row.
        /// </summary>
        public string ToVectorString(int maxLines, int maxPerLine, int padding, string format = null, IFormatProvider provider = null)
        {
            int fullLines = Count/maxPerLine;
            int lastLine = Count%maxPerLine;
            bool incomplete = false;

            if (fullLines > maxLines || fullLines == maxLines && lastLine > 0)
            {
                fullLines = maxLines - 1;
                lastLine = maxPerLine - 1;
                incomplete = true;
            }

            const string separator = " ";
            string pdots = "...".PadLeft(padding);

            if (format == null)
            {
                format = "G8";
            }

            var stringBuilder = new StringBuilder();

            var iterator = GetEnumerator();
            for (var line = 0; line < fullLines; line++)
            {
                if (line > 0)
                {
                    stringBuilder.Append(Environment.NewLine);
                }
                iterator.MoveNext();
                stringBuilder.Append(iterator.Current.ToString(format, provider).PadLeft(padding));
                for (var column = 1; column < maxPerLine; column++)
                {
                    stringBuilder.Append(separator);
                    iterator.MoveNext();
                    stringBuilder.Append(iterator.Current.ToString(format, provider).PadLeft(padding));
                }
            }

            if (lastLine > 0)
            {
                if (fullLines > 0)
                {
                    stringBuilder.Append(Environment.NewLine);
                }
                iterator.MoveNext();
                stringBuilder.Append(iterator.Current.ToString(format, provider).PadLeft(padding));
                for (var column = 1; column < lastLine; column++)
                {
                    stringBuilder.Append(separator);
                    iterator.MoveNext();
                    stringBuilder.Append(iterator.Current.ToString(format, provider).PadLeft(padding));
                }
                if (incomplete)
                {
                    stringBuilder.Append(separator);
                    stringBuilder.Append(pdots);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that summarizes this vector.
        /// </summary>
        public string ToString(int maxLines, int maxPerLine, IFormatProvider provider = null)
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToVectorString(maxLines, maxPerLine, provider));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that summarizes this vector.
        /// The maximum number of cells can be configured in the <see cref="Control"/> class.
        /// </summary>
        public override sealed string ToString()
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToVectorString(Control.MaxToStringRows, Control.MaxToStringColumns, null));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that summarizes this vector.
        /// The maximum number of cells can be configured in the <see cref="Control"/> class.
        /// The format string is ignored.
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Concat(ToTypeString(), Environment.NewLine, ToVectorString(Control.MaxToStringRows, Control.MaxToStringColumns, formatProvider));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that summarizes this vector.
        /// </summary>
        [Obsolete("Scheduled for removal in v3.0.")]
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }
    }
}
