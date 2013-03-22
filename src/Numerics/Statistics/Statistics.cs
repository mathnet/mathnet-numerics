// <copyright file="Statistics.cs" company="Math.NET">
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

namespace MathNet.Numerics.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods to return basic statistics on set of data.
    /// </summary>
    public static class Statistics
    {
        /// <summary>
        /// Returns the minimum value in the sample data.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double Minimum(this IEnumerable<double> data)
        {
            var array = data as double[];
            return array != null
                ? ArrayStatistics.Minimum(array)
                : StreamingStatistics.Minimum(data);
        }
        /// <summary>
        /// Returns the minimum value in the sample data.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double Minimum(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return StreamingStatistics.Minimum(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double Maximum(this IEnumerable<double> data)
        {
            var array = data as double[];
            return array != null
                ? ArrayStatistics.Maximum(array)
                : StreamingStatistics.Maximum(data);
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double Maximum(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return StreamingStatistics.Maximum(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the sample mean.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double> data)
        {
            var array = data as double[];
            return array != null
                ? ArrayStatistics.Mean(array)
                : StreamingStatistics.Mean(data);
        }

        /// <summary>
        /// Estimates the sample mean.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return StreamingStatistics.Mean(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the unbiased population (sample) variance estimator (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the variance of.</param>
        /// <returns>The unbiased population variance of the sample.</returns>
        public static double Variance(this IEnumerable<double> data)
        {
            var array = data as double[];
            return array != null
                ? ArrayStatistics.Variance(array)
                : StreamingStatistics.Variance(data);
        }

        /// <summary>
        /// Estimates the unbiased population (sample) variance estimator (on a dataset of size N will use an N-1 normalizer) for nullable data.
        /// </summary>
        /// <param name="data">The data to calculate the variance of.</param>
        /// <returns>The population variance of the sample.</returns>
        public static double Variance(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return StreamingStatistics.Variance(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the biased population variance estimator (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the variance of.</param>
        /// <returns>The biased population variance of the sample.</returns>
        public static double PopulationVariance(this IEnumerable<double> data)
        {
            var array = data as double[];
            return array != null
                ? ArrayStatistics.PopulationVariance(array)
                : StreamingStatistics.PopulationVariance(data);
        }

        /// <summary>
        /// Estimates the biased population variance estimator (on a dataset of size N will use an N normalizer) for nullable data.
        /// </summary>
        /// <param name="data">The data to calculate the variance of.</param>
        /// <returns>The population variance of the sample.</returns>
        public static double PopulationVariance(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return StreamingStatistics.PopulationVariance(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the unbiased sample standard deviation (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double StandardDeviation(this IEnumerable<double> data)
        {
            var array = data as double[];
            return array != null
                ? ArrayStatistics.StandardDeviation(array)
                : StreamingStatistics.StandardDeviation(data);
        }

        /// <summary>
        /// Estimates the unbiased sample standard deviation (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double StandardDeviation(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return StreamingStatistics.StandardDeviation(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the biased sample standard deviation (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double PopulationStandardDeviation(this IEnumerable<double> data)
        {
            var array = data as double[];
            return array != null
                ? ArrayStatistics.PopulationStandardDeviation(array)
                : StreamingStatistics.PopulationStandardDeviation(data);
        }

        /// <summary>
        /// Estimates the biased sample standard deviation (on a dataset of size N will use an N normalizer).
        /// </summary>
        /// <param name="data">The data to calculate the standard deviation of.</param>
        /// <returns>The standard deviation of the sample.</returns>
        public static double PopulationStandardDeviation(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return StreamingStatistics.PopulationStandardDeviation(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the sample median.
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <returns>The median of the sample.</returns>
        public static double Median(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.MedianInplace(array);
        }

        /// <summary>
        /// Estimates the sample median.
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <returns>The median of the sample.</returns>
        public static double Median(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.MedianInplace(array);
        }

        /// <summary>
        /// Estimates the sample tau-quantile.
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <returns>The median of the sample.</returns>
        public static double Quantile(this IEnumerable<double> data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.QuantileInplace(array, tau);
        }

        /// <summary>
        /// Estimates the sample tau-quantile.
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <returns>The median of the sample.</returns>
        public static double Quantile(this IEnumerable<double?> data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.QuantileInplace(array, tau);
        }

        /// <summary>
        /// Estimates the empiric inverse CDF at tau (tau-quantile).
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <returns>The median of the sample.</returns>
        public static double InverseCDF(this IEnumerable<double> data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, QuantileDefinition.InverseCDF);
        }

        /// <summary>
        /// Estimates the empiric inverse CDF at tau (tau-quantile).
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <returns>The median of the sample.</returns>
        public static double InverseCDF(this IEnumerable<double?> data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, QuantileDefinition.InverseCDF);
        }

        /// <summary>
        /// Estimates the sample tau-quantile.
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <returns>The median of the sample.</returns>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static double QuantileCustom(this IEnumerable<double> data, double tau, QuantileDefinition definition)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, definition);
        }

        /// <summary>
        /// Estimates the sample tau-quantile.
        /// </summary>
        /// <param name="data">The data to calculate the median of.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <returns>The median of the sample.</returns>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static double QuantileCustom(this IEnumerable<double?> data, double tau, QuantileDefinition definition)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, definition);
        }

        /// <summary>
        /// Returns the i-order (1..N) statistic of the provided samples.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <param name="order">Order of the statistic to evaluate.</param>
        /// <returns>The i'th order statistic in the sample data.</returns>
        public static double OrderStatistic(IEnumerable<double> data, int order)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.OrderStatisticInplace(array, order);
        }
    }
}
