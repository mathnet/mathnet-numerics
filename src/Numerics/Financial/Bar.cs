// <copyright file="Bar.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

namespace MathNet.Numerics.Financial
{
    /// <summary>
    /// Representation of a stock bar
    /// </summary>
    public struct Bar
    {
        readonly double _high;
        readonly double _low;
        readonly double _open;
        readonly double _close;

        /// <summary>
        /// High of the bar
        /// </summary>
        public double High
        {
            get { return _high; }
        }

        /// <summary>
        /// Low of the bar
        /// </summary>
        public double Low
        {
            get { return _low; }
        }

        /// <summary>
        /// Open of the bar
        /// </summary>
        public double Open
        {
            get { return _open; }
        }

        /// <summary>
        /// Close of the bar
        /// </summary>
        public double Close
        {
            get { return _close; }
        }

        /// <param name="high"> High of the bar</param>
        /// <param name="low"> Low of the bar</param>
        /// <param name="open"> Open of the bar</param>
        /// <param name="close"> Close of the bar</param>
        public Bar(double high, double low, double open, double close)
        {
            _high = high;
            _low = low;
            _open = open;
            _close = close;
        }
    }
}
