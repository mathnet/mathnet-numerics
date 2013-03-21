// <copyright file="StreamingStatistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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
    public static class StreamingStatistics
    {
        /// <summary>
        /// Returns the smallest value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Minimum(IEnumerable<double> stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var min = double.PositiveInfinity;
            bool any = false;
            foreach (var d in stream)
            {
                if (d < min || double.IsNaN(d))
                {
                    min = d;
                }
                any = true;
            }
            return any ? min : double.NaN;
        }

        /// <summary>
        /// Returns the largest value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Maximum(IEnumerable<double> stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var max = double.NegativeInfinity;
            bool any = false;
            foreach (var d in stream)
            {
                if (d > max || double.IsNaN(d))
                {
                    max = d;
                }
                any = true;
            }
            return any ? max : double.NaN;
        }

        /// <summary>
        /// Estimates the arithmetic sample mean from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Mean(IEnumerable<double> stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            double mean = 0;
            ulong m = 0;
            bool any = false;
            foreach (var d in stream)
            {
                mean += (d - mean)/++m;
                any = true;
            }
            return any ? mean : double.NaN;
        }

        /// <summary>
        /// Estimates the unbiased population or sample variance from the enumerable, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Variance(IEnumerable<double> stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            double variance = 0;
            double t = 0;
            ulong j = 0;
            using (var iterator = stream.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    j++;
                    t = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    j++;
                    double xi = iterator.Current;
                    t += xi;
                    double diff = (j*xi) - t;
                    variance += (diff*diff)/(j*(j - 1));
                }
            }
            return j > 1 ? variance/(j - 1) : double.NaN;
        }

        /// <summary>
        /// Estimates the biased population variance from the enumerable, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double PopulationVariance(IEnumerable<double> stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            double variance = 0;
            double t = 0;
            ulong j = 0;
            using (var iterator = stream.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    j++;
                    t = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    j++;
                    double xi = iterator.Current;
                    t += xi;
                    double diff = (j*xi) - t;
                    variance += (diff*diff)/(j*(j - 1));
                }
            }
            return variance/j;
        }
    }
}
