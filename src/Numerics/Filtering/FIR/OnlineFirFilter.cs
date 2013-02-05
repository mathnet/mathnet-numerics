// <copyright file="OnlineFirFilter.cs" company="Math.NET">
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

namespace MathNet.Numerics.Filtering.FIR
{
    using System.Collections.Generic;

    /// <summary>
    /// Finite Impulse Response (FIR) Filters are based on
    /// fourier series and implemented using a discrete
    /// convolution equation. FIR Filters are always
    /// online, stable and causal. 
    /// </summary>
    /// <remarks>
    /// System Descripton: H(z) = a0 + a1*z^-1 + a2*z^-2 + ...
    /// </remarks>
    public class OnlineFirFilter : OnlineFilter
    {
        double[] _coefficients;
        double[] _buffer;
        int _offset;
        readonly int _size;

        /// <summary>
        /// Finite Impulse Response (FIR) Filter.
        /// </summary>
        public
        OnlineFirFilter(
            IList<double> coefficients
            )
        {
            _size = coefficients.Count;
            _buffer = new double[_size];
            _coefficients = new double[_size << 1];
            for(int i = 0; i < _size; i++)
            {
                _coefficients[i] = _coefficients[_size + i] = coefficients[i];
            }
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
            _offset = (_offset != 0) ? _offset - 1 : _size - 1;
            _buffer[_offset] = sample;

            double acc = 0;
            for(int i = 0, j = _size - _offset; i < _size; i++, j++)
            {
                acc += _buffer[i] * _coefficients[j];
            }

            return acc;
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
