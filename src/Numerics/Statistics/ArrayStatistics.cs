// <copyright file="ArrayStatistics.cs" company="Math.NET">
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

namespace MathNet.Numerics.Statistics
{
    public static class ArrayStatistics
    {
        // TODO: Benchmark various options to find out the best approach (-> branch prediction)
        // TODO: consider leveraging MKL 

        /// <summary>
        /// Returns the smallest value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double Minimum(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return double.NaN;

            var min = double.PositiveInfinity;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < min || double.IsNaN(data[i]))
                {
                    min = data[i];
                }
            }
            return min;
        }

        /// <summary>
        /// Returns the smallest value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double Maximum(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return double.NaN;

            var max = double.NegativeInfinity;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > max || double.IsNaN(data[i]))
                {
                    max = data[i];
                }
            }
            return max;
        }

        /// <summary>
        /// Estimates the arithmetic sample mean from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double Mean(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return double.NaN;

            double mean = 0;
            ulong m = 0;
            for (int i = 0; i < data.Length; i++)
            {
                mean += (data[i] - mean)/++m;
            }
            return mean;
        }

        /// <summary>
        /// Estimates the unbiased population or sample variance from the unsorted data array.
        /// On a dataset of size N will use an N-1 normalizer
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double Variance(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length <= 1) return double.NaN;

            double variance = 0;
            double t = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                t += data[i];
                double diff = ((i + 1)*data[i]) - t;
                variance += (diff*diff)/((i + 1)*i);
            }
            return variance/(data.Length - 1);
        }

        /// <summary>
        /// Estimates the biased population variance from the unsorted data array.
        /// On a dataset of size N will use an N normalizer
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double PopulationVariance(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return double.NaN;

            double variance = 0;
            double t = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                t += data[i];
                double diff = ((i + 1)*data[i]) - t;
                variance += (diff*diff)/((i + 1)*i);
            }
            return variance/data.Length;
        }
    }
}