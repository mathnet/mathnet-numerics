using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MathNet.Numerics.Spatial.Internal
{
    /// <summary>
    /// An internal implementation of ImmutableList
    /// </summary>
    /// <typeparam name="T">A type for the list</typeparam>
    internal sealed class ImmutableList<T> : IEnumerable<T>
    {
        /// <summary>
        /// An empty list
        /// </summary>
        internal static readonly ImmutableList<T> Empty = new ImmutableList<T>(new T[0]);

        /// <summary>
        /// The list data
        /// </summary>
        private readonly T[] data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableList{T}"/> class.
        /// </summary>
        /// <param name="data">The data to initialize the list with</param>
        private ImmutableList(T[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// Gets the number of items in the list
        /// </summary>
        internal int Count => this.data.Length;

        /// <summary>
        /// An 0 based index into the list
        /// </summary>
        /// <param name="index">the index</param>
        /// <returns>A list item</returns>
        internal T this[int index] => this.data[index];

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => ((IList<T>)this.data).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this.data.GetEnumerator();

        /// <summary>
        /// Adds an item to the list
        /// </summary>
        /// <param name="value">An item</param>
        /// <returns>A new list with the item added</returns>
        [Pure]
        internal ImmutableList<T> Add(T value)
        {
            var newData = new T[this.data.Length + 1];
            Array.Copy(this.data, newData, this.data.Length);
            newData[this.data.Length] = value;
            return new ImmutableList<T>(newData);
        }

        /// <summary>
        /// Adds a range of items to the list
        /// </summary>
        /// <param name="values">The items to add</param>
        /// <returns>A new list with the items added</returns>
        [Pure]
        internal ImmutableList<T> AddRange(ICollection<T> values)
        {
            var newData = new T[this.data.Length + values.Count];
            Array.Copy(this.data, newData, this.data.Length);
            values.CopyTo(newData, this.data.Length);
            return new ImmutableList<T>(newData);
        }

        /// <summary>
        /// Removes an item from the list
        /// </summary>
        /// <param name="value">The item to remove</param>
        /// <returns>A new list with the item removed</returns>
        [Pure]
        internal ImmutableList<T> Remove(T value)
        {
            var i = Array.IndexOf(this.data, value);
            if (i < 0)
            {
                return this;
            }

            var length = this.data.Length;
            if (length == 1)
            {
                return Empty;
            }

            var newData = new T[length - 1];

            Array.Copy(this.data, 0, newData, 0, i);
            Array.Copy(this.data, i + 1, newData, i, length - i - 1);

            return new ImmutableList<T>(newData);
        }

        /// <summary>
        /// An internal method to access the underlying data.  To be used with care.
        /// </summary>
        /// <returns>The backing data array</returns>
        internal T[] GetRawData() => this.data;
    }
}
