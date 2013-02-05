// <copyright file="OnlineIirFilter.cs" company="Math.NET">
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

namespace MathNet.Numerics.Filtering.IIR
{
    using System;
    using MathNet.Numerics.Properties;

    /// <summary>
    /// Infinite Impulse Response (FIR) Filters need much
    /// less coefficients (and are thus much faster) than
    /// comparable FIR Filters, but are potentially unstable.
    /// IIR Filters are always online and causal. This IIR
    /// Filter implements the canonic Direct Form II structure.
    /// </summary>
    /// <remarks>
    /// System Descripton: H(z) = (a0 + a1*z^-1 + a2*z^-2) / (1 + b1*z^-1 + b2*z^-2)
    /// </remarks>
    public class OnlineIirFilter : OnlineFilter
    {
        double[] _leftCoefficients, _rightCoefficients;
        double[] _buffer;
        readonly int _size, _halfSize;
        int _offset;

        /// <summary>
        /// Infinite Impulse Response (IIR) Filter.
        /// </summary>
        public
        OnlineIirFilter(
            double[] coefficients
            )
        {
            if(null == coefficients)
                throw new ArgumentNullException("coefficients");
            if((coefficients.Length & 1) != 0)
                throw new ArgumentException(Resources.ArgumentEvenNumberOfCoefficients, "coefficients");

            _size = coefficients.Length;
            _halfSize = _size >> 1;
            _leftCoefficients = new double[_size];
            _rightCoefficients = new double[_size];
            for(int i = 0; i < _halfSize; i++)
            {
                _leftCoefficients[i] = _leftCoefficients[_halfSize + i] = coefficients[i];
                _rightCoefficients[i] = _rightCoefficients[_halfSize + i] = coefficients[_halfSize + i];
            }
            _buffer = new double[_size];
        }

        /// <summary>
        /// Process a single sample.
        /// </summary>
        public override
        double
        ProcessSample(
            double sample
            )
        {
            double un = _leftCoefficients[0] * sample;
            for(int i = 0, j = _halfSize - _offset + 1; i < _halfSize - 1; i++, j++)
            {
                un = _buffer[i] * _leftCoefficients[j];
            }

            _offset = (_offset != 0) ? _offset - 1 : _halfSize - 1;
            _buffer[_offset] = un - _buffer[_offset] * _leftCoefficients[1];

            double yn = 0d;
            for(int i = 0, j = _halfSize - _offset; i < _halfSize; i++, j++)
            {
                yn += _buffer[i] * _rightCoefficients[j];
            }

            return yn;
        }

        /// <summary>
        /// Reset internal state (not coefficients!).
        /// </summary>
        public override
        void
        Reset()
        {
            for(int i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = 0d;
            }
        }
    }
}
