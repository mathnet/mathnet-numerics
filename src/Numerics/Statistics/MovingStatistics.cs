// <copyright file="MovingStatistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Running statistics over a window of data, allows updating by adding values.
    /// </summary>
    public class MovingStatistics
    {
        readonly double[] _oldValues;
        readonly int _windowSize;

        long _count;
        long _totalCountOffset;
        int _lastIndex;
        int _lastNaNTimeToLive;
        int _lastPosInfTimeToLive;
        int _lastNegInfTimeToLive;

        double _m1;
        double _m2;
        double _max = double.NegativeInfinity;
        double _min = double.PositiveInfinity;

        public MovingStatistics(int windowSize)
        {
            if (windowSize < 1)
            {
                throw new ArgumentException("Value must be positive.", nameof(windowSize));
            }
            _windowSize = windowSize;
            _oldValues = new double[_windowSize];
        }

        public MovingStatistics(int windowSize, IEnumerable<double> values)
            : this(windowSize)
        {
            PushRange(values);
        }

        public int WindowSize => _windowSize;

        /// <summary>
        /// Gets the total number of samples.
        /// </summary>
        public long Count => _totalCountOffset + _count;

        /// <summary>
        /// Returns the minimum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Minimum
        {
            get
            {
                if (_lastNaNTimeToLive > 0)
                {
                    return double.NaN;
                }

                if (_lastNegInfTimeToLive > 0)
                {
                    return double.NegativeInfinity;
                }

                return (_count > 0 || _lastPosInfTimeToLive > 0) ? _min : double.NaN;
            }
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Maximum
        {
            get
            {
                if (_lastNaNTimeToLive > 0)
                {
                    return double.NaN;
                }

                if (_lastPosInfTimeToLive > 0)
                {
                    return double.PositiveInfinity;
                }

                return (_count > 0 || _lastNegInfTimeToLive > 0) ? _max : double.NaN;
            }
        }

        /// <summary>
        /// Evaluates the sample mean, an estimate of the population mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Mean
        {
            get
            {
                if (_lastNaNTimeToLive > 0 || (_lastPosInfTimeToLive > 0 && _lastNegInfTimeToLive > 0))
                {
                    return double.NaN;
                }

                if (_lastPosInfTimeToLive > 0)
                {
                    return double.PositiveInfinity;
                }

                if (_lastNegInfTimeToLive > 0)
                {
                    return double.NegativeInfinity;
                }

                return _count == 0 ? double.NaN : _m1;
            }
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        public double Variance
        {
            get
            {
                if (_lastNaNTimeToLive > 0 || _lastNegInfTimeToLive > 0)
                {
                    return double.NaN;
                }

                if (_lastPosInfTimeToLive > 0)
                {
                    return double.PositiveInfinity;
                }

                return _count < 2 ? double.NaN : _m2 / (_count - 1);
            }
        }

        /// <summary>
        /// Evaluates the variance from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double PopulationVariance
        {
            get
            {
                if (_lastNaNTimeToLive > 0 || _lastNegInfTimeToLive > 0)
                {
                    return double.NaN;
                }

                if (_lastPosInfTimeToLive > 0)
                {
                    return double.PositiveInfinity;
                }

                return _count < 2 ? double.NaN : _m2 / _count;
            }
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        public double StandardDeviation
        {
            get
            {
                if (_lastNaNTimeToLive > 0 || _lastNegInfTimeToLive > 0)
                {
                    return double.NaN;
                }

                if (_lastPosInfTimeToLive > 0)
                {
                    return double.PositiveInfinity;
                }

                return _count < 2 ? double.NaN : Math.Sqrt(_m2 / (_count - 1));
            }
        }

        /// <summary>
        /// Evaluates the standard deviation from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double PopulationStandardDeviation
        {
            get
            {
                if (_lastNaNTimeToLive > 0 || _lastNegInfTimeToLive > 0)
                {
                    return double.NaN;
                }

                if (_lastPosInfTimeToLive > 0)
                {
                    return double.PositiveInfinity;
                }

                return _count < 2 ? double.NaN : Math.Sqrt(_m2 / _count);
            }
        }

        /// <summary>
        /// Update the running statistics by adding another observed sample (in-place).
        /// </summary>
        public void Push(double value)
        {
            DecrementTimeToLive();

            if (double.IsNaN(value))
            {
                _lastNaNTimeToLive = _windowSize;
                Reset(double.PositiveInfinity, double.NegativeInfinity);
                return;
            }

            if (double.IsPositiveInfinity(value))
            {
                _lastPosInfTimeToLive = _windowSize;
                Reset(_min, double.NegativeInfinity);
                return;
            }

            if (double.IsNegativeInfinity(value))
            {
                _lastNegInfTimeToLive = _windowSize;
                Reset(double.PositiveInfinity, _max);
                return;
            }

            if (_count < _windowSize)
            {
                _oldValues[_count] = value;
                _count++;
                var d = value - _m1;
                var s = d / _count;
                var t = d * s * (_count - 1);

                _m1 += s;
                _m2 += t;

                if (value < _min)
                {
                    _min = value;
                }

                if (value > _max)
                {
                    _max = value;
                }
            }
            else
            {
                var oldValue = _oldValues[_lastIndex];
                var d = value - oldValue;
                var s = d / _count;
                var oldM1 = _m1;
                _m1 += s;

                var x = (value - _m1 + oldValue - oldM1);
                var t = d * x;
                _m2 += t;

                _oldValues[_lastIndex] = value;
                _lastIndex++;
                if (_lastIndex == WindowSize)
                {
                    _lastIndex = 0;
                }
                _max = value > _max ? value : _oldValues.Maximum();
                _min = value < _min ? value : _oldValues.Minimum();
            }
        }

        /// <summary>
        /// Update the running statistics by adding a sequence of observed sample (in-place).
        /// </summary>
        public void PushRange(IEnumerable<double> values)
        {
            foreach (var value in values)
            {
                Push(value);
            }
        }

        void DecrementTimeToLive()
        {
            if (_lastNaNTimeToLive > 0)
            {
                _lastNaNTimeToLive--;
            }

            if (_lastPosInfTimeToLive > 0)
            {
                _lastPosInfTimeToLive--;
            }

            if (_lastNegInfTimeToLive > 0)
            {
                _lastNegInfTimeToLive--;
            }
        }

        void Reset(double min, double max)
        {
            _totalCountOffset += _count + 1;
            _count = 0;
            _m1 = 0;
            _max = max;
            _min = min;
        }
    }
}
