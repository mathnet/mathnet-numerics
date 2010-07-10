// <copyright file="MatlabFile.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Double.IO.Matlab
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a Matlab file
    /// </summary>
    internal class MatlabFile
    {
        /// <summary>
        /// Matrices in a matlab file stored as 1-D arrays
        /// </summary>
        private readonly IDictionary<string, Matrix> _matrices = new SortedList<string, Matrix>();
       
        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>The header text.</value>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the first name of the matrix.
        /// </summary>
        /// <value>The first name of the matrix.</value>
        public string FirstMatrixName { get; set; }

        /// <summary>
        /// Gets the first matrix.
        /// </summary>
        /// <value>The first matrix.</value>
        public Matrix FirstMatrix
        {
            get
            {
                if (string.IsNullOrEmpty(FirstMatrixName) || !_matrices.ContainsKey(FirstMatrixName))
                {
                    return null;
                }

                return _matrices[FirstMatrixName];
            }
        }

        /// <summary>
        /// Gets the matrices.
        /// </summary>
        /// <value>The matrices.</value>
        public IDictionary<string, Matrix> Matrices
        {
            get { return _matrices; }
        }
    }

    /*
    /// <summary>
    /// An 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    internal class ListDictionary<T, K> : IDictionary<T, K>
    {
        private readonly IList<T> _keys = new List<T>();
        private readonly IList<K> _values = new List<K>();

        public void Add(T key, K value)
        {
            _keys.Add(key);
            _values.Add(value);
        }

        public bool ContainsKey(T key)
        {
            return _keys.Contains(key);
        }

        public ICollection<T> Keys
        {
            get { return _keys; }
        }

        public bool Remove(T key)
        {
            if (_keys.Contains(key))
            {
                int pos = _keys.IndexOf(key);
                _values.RemoveAt(pos);
                _values.RemoveAt(pos);
                return true;
            }
            return false;
        }

        public bool TryGetValue(T key, out K value)
        {
            if (_keys.Contains(key))
            {
                value = _values[_keys.IndexOf(key)];
                return true;
            }
            value = default(K);
            return false;
        }

        public ICollection<K> Values
        {
            get { return _values; }
        }

        public K this[T key]
        {
            get
            {
                if (_keys.Contains(key))
                {
                    return _values[_keys.IndexOf(key)];
                }
                throw new KeyNotFoundException();
            }
            set
            {
                if (_keys.Contains(key))
                {
                    _values[_keys.IndexOf(key)] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public void Add(KeyValuePair<T, K> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
        }

        public bool Contains(KeyValuePair<T, K> item)
        {
            return _keys.Contains(item.Key) && _values[_keys.IndexOf(item.Key)].Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<T, K>[] array, int arrayIndex)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                array[arrayIndex + i] = new KeyValuePair<T, K>(_keys[i], _values[i]);
            }
        }

        public int Count
        {
            get { return _keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<T, K> item)
        {
            if (Contains(item))
            {
                Remove(item.Key);
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<T, K>> GetEnumerator()
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                yield return new KeyValuePair<T, K>(_keys[i], _values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    
    }*/
}
