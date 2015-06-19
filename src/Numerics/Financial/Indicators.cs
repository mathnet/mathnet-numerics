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
using System.Text;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.Financial
{
    /// <summary>
    /// Extension methods to calculate basic technical indicators
    /// </summary>
    public static class Indicators
    {
        /// <summary>
        /// Evaluates the sample mean over a moving window, for each samples.
        /// Returns NaN if no data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="samples">The sample stream to calculate the mean of.</param>
        /// <param name="period">The number of last samples to consider.</param>
        /// <returns>The SMA for samples</returns>
        public static IEnumerable<double> SMA(this IEnumerable<double> samples, int period)
        {
            return samples.MovingAverage(period);
        }
        /// <summary>
        /// Calculate the Average True Range (ATR)
        /// </summary>
        /// <param name="samples">Input samples</param>
        /// <param name="period">Period of calculation</param>
        /// <returns></returns>
        public static IEnumerable<double> ATR(this IEnumerable<Bar> samples, int period)
        {
            if (period<=0)
                throw new ArgumentException("period should be greater than 0","period");
            if(samples==null)
                throw new ArgumentNullException("samples", "samples should not be null");
            if (period > (samples.Count() +1))
                throw new ArgumentException("samples", "samples should be greater than period");

            var trList = new List<double>();
            var enumerator = samples.GetEnumerator();
            enumerator.MoveNext();
            var lastBar = enumerator.Current;

            trList.Add(double.NaN);

            while (enumerator.MoveNext())
            {
                var currentBar = enumerator.Current;

                var hl = Math.Round(currentBar.High - currentBar.Low,10);
                hl = Math.Abs(hl);
                var pdch = Math.Round(lastBar.Close - currentBar.High,10);
                pdch = Math.Abs(pdch);
                var pdcl = Math.Round(lastBar.Close - currentBar.Low,10);
                pdcl = Math.Abs(pdcl);
                double tr = Math.Max(hl, Math.Max(pdch, pdcl));

                trList.Add(tr);

                lastBar = currentBar;
            }

            var atrList = new List<double>();

            for (int i = 0; i < period; i++)
                atrList.Add(Double.NaN);

            //remove first tr, this is not valid
            trList.RemoveAt(0);

            while (trList.Count>=period)
            {
                var mean = trList.Take(period).Mean();
                var meanRounded = Math.Round(mean, 2);
                atrList.Add(meanRounded);
                trList.RemoveAt(0);
            }

            return atrList;
        }

    }
}
