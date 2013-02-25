// <copyright file="MovingAverage.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2013 Math.NET
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
using System.Collections.ObjectModel;
using System.Linq;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Class for calculating and updating a moving average.
    /// </summary>
    public class MovingAverage
    {
        readonly object _lock = new object();
        readonly IList<double> _result = new List<double>();
        readonly double[] _window;
        readonly int _windowSize;
        int _position;
        double _sum;

        /// <summary>
        /// Creates a moving average object and computes the initial
        /// moving average.
        /// </summary>
        /// <param name="data">The data to compute the moving average from.</param>
        /// <param name="windowSize">The size of moving average window; the number of items
        /// to include in the average.</param>
        public MovingAverage(IEnumerable<double> data, int windowSize)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            _window = new double[windowSize];
            _windowSize = windowSize;
            Iterate(data);
        }

        /// <summary>
        /// Creates a moving average object and computes the initial
        /// moving average.
        /// </summary>
        /// <param name="data">The data to compute the moving average from.</param>
        /// <param name="windowSize">The size of moving average window; the number of items
        /// to include in the average.</param>
        /// <remarks>null values are ignored.</remarks>
        public MovingAverage(IEnumerable<double?> data, int windowSize)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            _window = new double[windowSize];
            _windowSize = windowSize;
            Iterate(data);
        }

        /// <summary>
        /// The computed averages as a read-only list.
        /// </summary>
        public IList<double> Averages
        {
            get { return new ReadOnlyCollection<double>(_result); }
        }

        /// <summary>
        /// Updates/extends the moving average.
        /// </summary>
        /// <param name="data">New data to extend the moving average with.</param>
        public void Update(IEnumerable<double> data)
        {
            lock (_lock)
            {
                Iterate(data);
            }
        }

        /// <summary>
        /// Updates/extends the moving average.
        /// </summary>
        /// <param name="data">New data to extend the moving average with.</param>
        /// <remarks>null values are ignored.</remarks>
        public void Update(IEnumerable<double?> data)
        {
            lock (_lock)
            {
                Iterate(data);
            }
        }

        /// <summary>
        /// Does the actual computation of the data.
        /// </summary>
        /// <param name="data">The data to create the moving average with.</param>
        void Iterate(IEnumerable<double> data)
        {
            foreach (var current in data)
            {
                _sum += current;
                if (_position < _windowSize)
                {
                    _window[_position] = current;
                    if (_position == _windowSize - 1)
                    {
                        _result.Add(_sum/_windowSize);
                    }
                    _position++;
                }
                else
                {
                    var index = _position%_windowSize;
                    var last = _window[index];
                    _sum -= last;
                    _result.Add(_sum/_windowSize);
                    _window[index] = current;
                    _position++;
                }
            }
        }
        /// <summary>
        /// Does the actual computation of the data.
        /// </summary>
        /// <param name="data">The data to create the moving average with.</param>
        void Iterate(IEnumerable<double?> data)
        {
            foreach (var current in from value in data where value.HasValue select value.Value)
            {
                _sum += current;
                if (_position < _windowSize)
                {
                    _window[_position] = current;
                    if (_position == _windowSize - 1)
                    {
                        _result.Add(_sum/_windowSize);
                    }
                    _position++;
                }
                else
                {
                    var index = _position%_windowSize;
                    var last = _window[index];
                    _sum -= last;
                    _result.Add(_sum/_windowSize);
                    _window[index] = current;
                    _position++;
                }
            }
        }
    }
}
