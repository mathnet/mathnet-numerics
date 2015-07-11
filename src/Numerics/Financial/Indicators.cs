// <copyright file="Indicators.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.Financial
{
    /// <summary>
    /// Extension methods to calculate basic technical indicators
    /// </summary>
    public static class Indicators
    {
        private static IEnumerable<double> TrueRange(this IEnumerable<Bar> samples)
        {
            using (var enumerator = samples.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }

                Bar last = enumerator.Current;

                //yield return double.NaN;
                yield return Math.Abs(last.High - last.Low);

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    var hl = Math.Abs(current.High - current.Low);
                    var pdch = Math.Abs(last.Close - current.High);
                    var pdcl = Math.Abs(last.Close - current.Low);
                    last = current;

                    yield return Math.Max(hl, Math.Max(pdch, pdcl));
                }
            }
        }

        /// <summary>
        /// Calculate the Simple Moving Average (SMA) based on Close. Online/Streaming.
        /// </summary>
        /// <param name="samples">Input samples</param>
        /// <param name="period">Period of calculation</param>
        public static IEnumerable<double> SMA(this IEnumerable<Bar> samples, int period)
        {
            return samples.Select(bar => bar.Close).MovingAverage(period).Select(x => Math.Round(x, 2));
        }

        /// <summary>
        /// Calculate the True Range (TR). Online/Streaming.
        /// </summary>
        /// <param name="samples">Input samples</param>
        public static IEnumerable<double> TR(this IEnumerable<Bar> samples)
        {
            return samples.TrueRange().Select(x => Math.Round(x, 2));
        }
        /// <summary>
        /// Calculate the Average True Range (ATR). Online/Streaming.
        /// </summary>
        /// <param name="samples">Input samples</param>
        /// <param name="period">Period of calculation</param>
        public static IEnumerable<double> ATR(this IEnumerable<Bar> samples, int period)
        {
            return samples.TrueRange().MovingAverage(period).Select(x => Math.Round(x, 2));
        }
    }
}
