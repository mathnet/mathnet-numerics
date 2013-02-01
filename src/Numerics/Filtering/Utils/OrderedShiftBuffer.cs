// <copyright file="OrderedShiftBuffer.cs" company="Math.NET">
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

namespace MathNet.Numerics.Filtering.Utils
{
    using System.Collections.Generic;

    /// <summary>
    /// A fixed-sized sorted buffer that behaves like a shift register,
    /// hence the item added first is also removed first.
    /// </summary>
    public class OrderedShiftBuffer
    {
        readonly int _len, _mid;
        bool _initialized;
        LinkedList<double> _ordered;
        LinkedList<LinkedListNode<double>> _shift;

        /// <summary>
        /// Create an ordered shift buffer.
        /// </summary>
        public
        OrderedShiftBuffer(
            int length
            )
        {
            _len = length;
            _mid = length >> 1; // 4 items -> 3rd item; 5 items -> 3rd item; 6 items -> 4th item etc.
            _ordered = new LinkedList<double>();
            _shift = new LinkedList<LinkedListNode<double>>();
        }

        /// <summary>
        /// The number of samples currently loaded in the buffer.
        /// </summary>
        public int ActualCount
        {
            get { return _shift.Count; }
        }

        /// <summary>
        /// True if the buffer is filled completely and thus in normal operation.
        /// </summary>
        public bool IsInitialized
        {
            get { return _initialized; }
        }

        /// <summary>
        /// The number of samples required for this buffer to operate normally.
        /// </summary>
        public int InitializedCount
        {
            get { return _len; }
        }

        /// <summary>
        /// Append a single sample to the buffer.
        /// </summary>
        public
        void
        Append(
            double value
            )
        {
            LinkedListNode<double> node = new LinkedListNode<double>(value);
            _shift.AddFirst(node);
            if(_initialized)
            {
                _ordered.Remove(_shift.Last.Value);
                _shift.RemoveLast();
            }
            else if(_shift.Count == _len)
                _initialized = true;

            LinkedListNode<double> next = _ordered.First;
            while(next != null)
            {
                if(value > next.Value)
                {
                    next = next.Next;
                    continue;
                }

                _ordered.AddBefore(next, node);
                return;
            }

            _ordered.AddLast(node);
        }

        /// <summary>
        /// Remove all samples from the buffer.
        /// </summary>
        public
        void
        Clear()
        {
            _initialized = false;
            _shift.Clear();
            _ordered.Clear();
        }

        /// <summary>
        /// The current median of all samples currently in the buffer.
        /// </summary>
        public double Median
        {
            get
            {
                int mid = _initialized ? _mid : (_ordered.Count >> 1);
                LinkedListNode<double> next = _ordered.First;
                for(int i = 0; i < mid; i++)
                {
                    next = next.Next;
                }
                return next.Value;
            }
        }

        /// <summary>
        /// Iterate over all samples, ordered by value.
        /// </summary>
        public IEnumerable<double> ByValueOrder
        {
            get
            {
                LinkedListNode<double> item = _ordered.First;
                while(item != null)
                {
                    yield return item.Value;
                    item = item.Next;
                }
            }
        }

        /// <summary>
        /// Iterate over all samples, ordered by insertion order.
        /// </summary>
        public IEnumerable<double> ByInsertOrder
        {
            get
            {
                LinkedListNode<LinkedListNode<double>> item = _shift.First;
                while(item != null)
                {
                    yield return item.Value.Value;
                    item = item.Next;
                }
            }
        }
    }
}
