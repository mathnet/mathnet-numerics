// <copyright file="StreamingStatistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2014 Math.NET
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Statistics operating on an IEnumerable in a single pass, without keeping the full data in memory.
    /// Can be used in a streaming way, e.g. on large datasets not fitting into memory.
    /// </summary>
    /// <seealso cref="SortedArrayStatistics"/>
    /// <seealso cref="StreamingStatistics"/>
    /// <seealso cref="Statistics"/>
    public static class StreamingStatistics
    {
        /// <summary>
        /// Returns the smallest value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Minimum(IEnumerable<double> stream)
        {
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
        /// Estimates the unbiased population variance from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static double Variance(IEnumerable<double> samples)
        {
            double variance = 0;
            double sum = 0;
            ulong count = 0;

            using (var iterator = samples.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    count++;
                    sum = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    count++;
                    double xi = iterator.Current;
                    sum += xi;
                    double diff = (count*xi) - sum;
                    variance += (diff*diff)/(count*(count - 1));
                }
            }

            return count > 1 ? variance/(count - 1) : double.NaN;
        }

        /// <summary>
        /// Evaluates the population variance from the full population provided as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample stream, no sorting is assumed.</param>
        public static double PopulationVariance(IEnumerable<double> population)
        {
            double variance = 0;
            double sum = 0;
            ulong count = 0;

            using (var iterator = population.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    count++;
                    sum = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    count++;
                    double xi = iterator.Current;
                    sum += xi;
                    double diff = (count*xi) - sum;
                    variance += (diff*diff)/(count*(count - 1));
                }
            }

            return variance/count;
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static double StandardDeviation(IEnumerable<double> samples)
        {
            return Math.Sqrt(Variance(samples));
        }

        /// <summary>
        /// Evaluates the population standard deviation from the full population provided as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample stream, no sorting is assumed.</param>
        public static double PopulationStandardDeviation(IEnumerable<double> population)
        {
            return Math.Sqrt(PopulationVariance(population));
        }

        /// <summary>
        /// Estimates the arithmetic sample mean and the unbiased population variance from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or any entry is NaN, and NaN for variance if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static Tuple<double, double> MeanVariance(IEnumerable<double> samples)
        {
            double mean = 0;
            double variance = 0;
            double sum = 0;
            ulong count = 0;

            using (var iterator = samples.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    count++;
                    sum = mean = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    count++;
                    double xi = iterator.Current;
                    sum += xi;
                    double diff = (count * xi) - sum;
                    variance += (diff * diff) / (count * (count - 1));
                    mean += (xi - mean) / count;
                }
            }

            return new Tuple<double, double>(
                count > 0 ? mean : double.NaN,
                count > 1 ? variance/(count - 1) : double.NaN);
        }

        /// <summary>
        /// Estimates the arithmetic sample mean and the unbiased population standard deviation from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or any entry is NaN, and NaN for standard deviation if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static Tuple<double, double> MeanStandardDeviation(IEnumerable<double> samples)
        {
            var meanVariance = MeanVariance(samples);
            return new Tuple<double, double>(meanVariance.Item1, Math.Sqrt(meanVariance.Item2));
        }

        /// <summary>
        /// Estimates the unbiased population covariance from the provided two sample enumerable sequences, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples1">First sample stream.</param>
        /// <param name="samples2">Second sample stream.</param>
        public static double Covariance(IEnumerable<double> samples1, IEnumerable<double> samples2)
        {
            // https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance
            var n = 0;
            var mean1 = 0.0;
            var mean2 = 0.0;
            var comoment = 0.0;

            using (var s1 = samples1.GetEnumerator())
            using (var s2 = samples2.GetEnumerator())
            {
                while (s1.MoveNext())
                {
                    if (!s2.MoveNext())
                    {
                        throw new ArgumentException(Resources.ArgumentVectorsSameLength);
                    }

                    var mean2Prev = mean2;
                    n++;
                    mean1 += (s1.Current - mean1)/n;
                    mean2 += (s2.Current - mean2)/n;
                    comoment += (s1.Current - mean1)*(s2.Current - mean2Prev);
                }

                if (s2.MoveNext())
                {
                    throw new ArgumentException(Resources.ArgumentVectorsSameLength);
                }
            }

            return n > 1 ? comoment/(n - 1) : double.NaN;
        }

        /// <summary>
        /// Evaluates the population covariance from the full population provided as two enumerable sequences, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population1">First population stream.</param>
        /// <param name="population2">Second population stream.</param>
        public static double PopulationCovariance(IEnumerable<double> population1, IEnumerable<double> population2)
        {
            // https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance
            var n = 0;
            var mean1 = 0.0;
            var mean2 = 0.0;
            var comoment = 0.0;

            using (var p1 = population1.GetEnumerator())
            using (var p2 = population2.GetEnumerator())
            {
                while (p1.MoveNext())
                {
                    if (!p2.MoveNext())
                    {
                        throw new ArgumentException(Resources.ArgumentVectorsSameLength);
                    }

                    var mean2Prev = mean2;
                    n++;
                    mean1 += (p1.Current - mean1) / n;
                    mean2 += (p2.Current - mean2) / n;
                    comoment += (p1.Current - mean1) * (p2.Current - mean2Prev);
                }

                if (p2.MoveNext())
                {
                    throw new ArgumentException(Resources.ArgumentVectorsSameLength);
                }
            }

            return comoment/n;
        }

        /// <summary>
        /// Estimates the root mean square (RMS) also known as quadratic mean from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double RootMeanSquare(IEnumerable<double> stream)
        {
            double mean = 0;
            ulong m = 0;
            bool any = false;

            foreach (var d in stream)
            {
                mean += (d*d - mean)/++m;
                any = true;
            }

            return any ? Math.Sqrt(mean) : double.NaN;
        }

        /// <summary>
        /// Calculates the entropy of a stream of double values.
        /// Returns NaN if any of the values in the stream are NaN.
        /// </summary>
        /// <param name="stream">The input stream to evaluate.</param>
        /// <returns></returns>
        public static double Entropy(IEnumerable<double> stream)
        {
            // http://en.wikipedia.org/wiki/Shannon_entropy

            var index = new Dictionary<double, double>();

            // count the number of occurrences of each item in the stream
            int totalCount = 0;
            foreach (double value in stream)
            {
                if (double.IsNaN(value))
                {
                    return double.NaN;
                }

                double currentValueCount;
                if (index.TryGetValue(value, out currentValueCount))
                {
                    index[value] = ++currentValueCount;
                }
                else
                {
                    index.Add(value, 1);
                }

                ++totalCount;
            }

            // calculate the entropy of the stream
            double entropy = 0;
            foreach (var item in index)
            {
                double p = item.Value / totalCount;
                entropy += p * Math.Log(p, 2);
            }

            return -entropy;
        }
    }
}
