// <copyright file="Statistics.cs" company="Math.NET">
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

namespace MathNet.Numerics.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Properties;

    /// <summary>
    /// Extension methods to return basic statistics on set of data.
    /// </summary>
    public static class Statistics
    {
        /// <summary>
        /// Calculates the sample mean.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double mean = 0;
            ulong m = 0;
            foreach (var item in data)
            {
                mean += (item - mean) / ++m;
            }

            return mean;
        }

        /// <summary>
        /// Calculates the sample mean.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double mean = 0;
            ulong m = 0;
            foreach (var item in data)
            {
                if (item.HasValue)
                {
                    mean += (item.Value - mean) / ++m;
                }
            }

            return mean;
        }

        /// <summary>
        /// Calculates the unbiased population variance estimator (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the variance of.</param>
        /// <returns>The unbiased population variance of the sample.</returns>
        public static double Variance(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double variance = 0;
            double t = 0;
            ulong j = 0;

            using (IEnumerator<double> iterator = data.GetEnumerator())
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
                    double diff = (j * xi) - t;
                    variance += (diff * diff) / (j * (j - 1));
                }
            }

            return variance / (j - 1);
        }

        /// <summary>
        /// Computes the unbiased population variance estimator (on a dataset of size N will use an N-1 normalizer) for nullable data.
        /// </summary>
        /// <param name="data">The data to calculate the variance of.</param>
        /// <returns>The population variance of the sample.</returns>
        public static double Variance(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double variance = 0;
            double t = 0;
            ulong j = 0;

            using (IEnumerator<double?> iterator = data.GetEnumerator())
            {
                while (true)
                {
                    bool hasNext = iterator.MoveNext();
                    if (!hasNext)
                    {
                        break;
                    }

                    if (iterator.Current.HasValue)
                    {
                        j++;
                        t = iterator.Current.Value;
                        break;
                    }
                }

                while (iterator.MoveNext())
                {
                    if (iterator.Current.HasValue)
                    {
                        j++;
                        double xi = iterator.Current.Value;
                        t += xi;
                        double diff = (j * xi) - t;
                        variance += (diff * diff) / (j * (j - 1));
                    }
                }
            }

            return variance / (j - 1);
        }

        /// <summary>
        /// Calculates the biased population variance estimator (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the variance of.</param>
        /// <returns>The biased population variance of the sample.</returns>
        public static double PopulationVariance(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double variance = 0;
            double t = 0;
            ulong j = 0;

            using (IEnumerator<double> iterator = data.GetEnumerator())
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
                    double diff = (j * xi) - t;
                    variance += (diff * diff) / (j * (j - 1));
                }
            }

            return variance / j;
        }

        /// <summary>
        /// Computes the biased population variance estimator (on a dataset of size N will use an N normalizer) for nullable data.
        /// </summary>
        /// <param name="data">The data to calculate the variance of.</param>
        /// <returns>The population variance of the sample.</returns>
        public static double PopulationVariance(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double variance = 0;
            double t = 0;
            ulong j = 0;

            using (IEnumerator<double?> iterator = data.GetEnumerator())
            {
                while (true)
                {
                    bool hasNext = iterator.MoveNext();
                    if (!hasNext)
                    {
                        break;
                    }

                    if (iterator.Current.HasValue)
                    {
                        j++;
                        t = iterator.Current.Value;
                        break;
                    }
                }

                while (iterator.MoveNext())
                {
                    if (iterator.Current.HasValue)
                    {
                        j++;
                        double xi = iterator.Current.Value;
                        t += xi;
                        double diff = (j * xi) - t;
                        variance += (diff * diff) / (j * (j - 1));
                    }
                }
            }

            return variance / j;
        }

        /// <summary>
        /// Calculates the unbiased sample standard deviation (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double StandardDeviation(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Math.Sqrt(Variance(data));
        }

        /// <summary>
        /// Calculates the unbiased sample standard deviation (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double StandardDeviation(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Math.Sqrt(Variance(data));
        }

        /// <summary>
        /// Calculates the biased sample standard deviation (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double PopulationStandardDeviation(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Math.Sqrt(PopulationVariance(data));
        }

        /// <summary>
        /// Calculates the biased sample standard deviation (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double PopulationStandardDeviation(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Math.Sqrt(PopulationVariance(data));
        }

        /// <summary>
        /// Returns the minimum value in the sample data.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double Minimum(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double min = double.MaxValue;
            ulong count = 0;
            foreach (double? d in data)
            {
                if (d.HasValue)
                {
                    min = Math.Min(min, d.Value);
                    count++;
                }
            }

            if (count == 0)
            {
                throw new ArgumentException(Resources.CollectionEmpty, "data");
            }

            return min;
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double Maximum(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double max = double.MinValue;
            ulong count = 0;
            foreach (double? d in data)
            {
                if (d.HasValue)
                {
                    max = Math.Max(max, d.Value);
                    count++;
                }
            }

            if (count == 0)
            {
                throw new ArgumentException(Resources.CollectionEmpty, "data");
            }

            return max;
        }

        /// <summary>
        /// Returns the minimum value in the sample data.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double Minimum(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double min = double.MaxValue;
            ulong count = 0;
            foreach (double d in data)
            {
                min = Math.Min(min, d);
                count++;
            }

            if (count == 0)
            {
                throw new ArgumentException(Resources.CollectionEmpty, "data");
            }

            return min;
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double Maximum(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            double max = double.MinValue;
            ulong count = 0;
            foreach (double d in data)
            {
                max = Math.Max(max, d);
                count++;
            }

            if (count == 0)
            {
                throw new ArgumentException(Resources.CollectionEmpty, "data");
            }

            return max;
        }

        /// <summary>
        /// Calculates the sample median.
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <returns>The median of the sample.</returns>
        public static double Median(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var dataArray = new List<double>(data);
            int index = (dataArray.Count / 2) + 1;
            if (dataArray.Count % 2 == 0)
            {
                double lower = OrderSelect(dataArray, 0, dataArray.Count - 1, index - 1);
                double upper = dataArray.Skip(index - 1).Minimum();
                return (lower + upper) / 2.0;
            }

            return OrderSelect(dataArray, 0, dataArray.Count - 1, index);
        }

        /// <summary>
        /// Calculates the sample median.
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <returns>The median of the sample.</returns>
        public static double Median(this IEnumerable<double?> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var nonNull = new List<double>();
            foreach (double? value in data)
            {
                if (value.HasValue)
                {
                    nonNull.Add(value.Value);
                }
            }

            if (nonNull.Count == 0)
            {
                throw new ArgumentException(Resources.CollectionEmpty, "data");
            }

            return nonNull.Median();
        }

        /// <summary>
        /// Evaluate the i-order (1..N) statistic of the provided samples.
        /// </summary>
        /// <param name="samples">The sample data.</param>
        /// <param name="order">Order of the statistic to evaluate.</param>
        /// <returns>The i'th order statistic in the sample data.</returns>
        public static double OrderStatistic(IEnumerable<double> samples, int order)
        {
            if (order == 1)
            {
                // Can be done in linear time by Min()
                return Minimum(samples);
            }

            var list = new List<double>(samples);
            if (order < 1 || order > list.Count)
            {
                throw new ArgumentOutOfRangeException("order", Resources.ArgumentInIntervalXYInclusive);
            }

            if (order == list.Count)
            {
                // Can be done in linear time by Max()
                return Maximum(list);
            }

            return OrderSelect(list, 0, list.Count - 1, order);
        }

        /// <summary>
        /// Implementation of the order statistics finding algorithm based on the algorithm in
        /// "Introduction to Algorithms", Cormen et al. section 7.1.
        /// </summary>
        /// <param name="samples">The sample data.</param>
        /// <param name="left">The left bound in which to order select.</param>
        /// <param name="right">The right bound in which to order select.</param>
        /// <param name="order">The order we are trying to find.</param>
        /// <returns>The <paramref name="order"/> order statistic.</returns>
        private static double OrderSelect(IList<double> samples, int left, int right, int order)
        {
            while (true)
            {
                System.Diagnostics.Debug.Assert(order > 0, "Order must always be positive.");
                System.Diagnostics.Debug.Assert(left >= 0 && left <= right, "Left side must always be positive and smaller than right side.");
                System.Diagnostics.Debug.Assert(right < samples.Count, "Right side must always be smaller than number of elements in list.");
                System.Diagnostics.Debug.Assert(right - left + 1 >= order, "Make sure there are at least order items in the segment [left, right].");

                if (left == right)
                {
                    return samples[left];
                }

                
                // The pivot point. Choose median of left, right and center
                //to be the pivot and arrange so that
                //samples[left]<=samples[right]<=samples[center]
                int center = (left + right) / 2;
                if (samples[center] < samples[left])
                    Sorting.Swap(samples, left, center);
                if (samples[center] < samples[right])
                    Sorting.Swap(samples, right, center);
                if (samples[right] < samples[left])
                    Sorting.Swap(samples, right, left);

                double pivot = samples[right];

                // The partioning code.
                int i = left;
                for (int j = left+1; j <= right - 1; j++)
                {
                    if (samples[j] <= pivot)
                    {
                        i++;
                        Sorting.Swap(samples, i, j);
                    }
                }

                Sorting.Swap(samples, i + 1, right);

                // Recursive order finding algorithm.
                if (order == (i - left) + 2)
                {
                    return pivot;
                }

                if (order < (i - left) + 2)
                {
                    right = i;
                }
                else
                {
                    order = order - i + left - 2;
                    left = i + 2;
                }
            }
        }
    }
}
