// <copyright file="ShiftBuffer.cs" company="Math.NET">
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
    using System;

    /// <summary>
    /// ShiftBuffer is a circular buffer behaving like a shift register (el. engineering)
    /// </summary>
    public class ShiftBuffer
    {
        int _offset;
        readonly int _size;
        double[] _buffer;

        /// <summary>
        /// Shift Buffer for discrete convolutions.
        /// </summary>
        public
        ShiftBuffer(
            int size
            )
        {
            _size = size;
            _buffer = new double[size];
        }
        /// <summary>
        /// Shift Buffer for discrete convolutions.
        /// </summary>
        public
        ShiftBuffer(
            double[] buffer
            )
        {
            if(null == buffer)
                throw new ArgumentNullException("buffer");

            _size = buffer.Length;
            _buffer = buffer;
        }

        /// <summary>
        /// Add a new sample on top of the buffer and discard the oldest entry (tail).
        /// </summary>
        public
        void
        ShiftAddHead(
            double sample
            )
        {
            _offset = (_offset != 0) ? _offset - 1 : _size - 1;
            _buffer[_offset] = sample;
        }

        /// <summary>
        /// Buffer Indexer. The newest (top) item has index 0.
        /// </summary>
        public double this[int index]
        {
            get { return _buffer[(_offset + index) % _size]; }
            set { _buffer[(_offset + index) % _size] = value; }
        }

        /// <summary>
        /// Discrete Convolution. Evaluates the classic MAC operation to another buffer/vector (looped).
        /// </summary>
        /// <returns>The sum of the memberwiese sample products, sum(a[i]*b[i],i=0..size)</returns>
        public
        double
        MultiplyAccumulate(
            double[] samples
            )
        {
            if(null == samples)
                throw new ArgumentNullException("samples");

            int len = Math.Min(samples.Length, _size);
            double sum = 0;
            int j = _offset;
            for(int i = 0; i < len; i++)
            {
                sum += samples[i] * _buffer[j];
                j = (j + 1) % _size;
            }
            return sum;
        }

        /// <summary>
        /// Discrete Convolution. Evaluates the classic MAC operation to another buffer/vector (looped).
        /// </summary>
        /// <returns>The sum of the memberwiese sample products, sum(a[i]*b[i],i=0..size)</returns>
        public
        double
        MultiplyAccumulate(
            double[] samples,
            int sampleOffset,
            int count
            )
        {
            if(null == samples)
                throw new ArgumentNullException("samples");

            int end = Math.Min(
                Math.Min(samples.Length, _size + sampleOffset),
                count + sampleOffset
                );

            double sum = 0;
            int j = _offset;
            for(int i = sampleOffset; i < end; i++)
            {
                sum += samples[i] * _buffer[j];
                j = (j + 1) % _size;
            }
            return sum;
        }

        /// <summary>
        /// Sets all buffer items to zero.
        /// </summary>
        public
        void
        Reset()
        {
            for(int i = 0; i < _size; i++)
            {
                _buffer[i] = 0;
            }
        }
    }
}
