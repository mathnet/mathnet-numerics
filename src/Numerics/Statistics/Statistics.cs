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
        /// Returns NaN if data is empty or if any entry is NaN.
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
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
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
        /// Returns NaN if data is empty or if any entry is NaN.
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
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
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
        /// Returns NaN if data is empty or if any entry is NaN.
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
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return StreamingStatistics.Mean(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer.
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Variance(this IEnumerable<double> samples)
        {
            var array = samples as double[];
            return array != null
                ? ArrayStatistics.Variance(array)
                : StreamingStatistics.Variance(samples);
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer.
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Variance(this IEnumerable<double?> samples)
        {
            if (samples == null) throw new ArgumentNullException("samples");
            return StreamingStatistics.Variance(samples.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Evaluates the biased population variance from the provided full population.
        /// On a dataset of size N will use an N normalizer.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationVariance(this IEnumerable<double> population)
        {
            var array = population as double[];
            return array != null
                ? ArrayStatistics.PopulationVariance(array)
                : StreamingStatistics.PopulationVariance(population);
        }

        /// <summary>
        /// Evaluates the biased population variance from the provided full population.
        /// On a dataset of size N will use an N normalizer.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationVariance(this IEnumerable<double?> population)
        {
            if (population == null) throw new ArgumentNullException("population");
            return StreamingStatistics.PopulationVariance(population.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer.
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double StandardDeviation(this IEnumerable<double> samples)
        {
            var array = samples as double[];
            return array != null
                ? ArrayStatistics.StandardDeviation(array)
                : StreamingStatistics.StandardDeviation(samples);
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer.
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double StandardDeviation(this IEnumerable<double?> samples)
        {
            if (samples == null) throw new ArgumentNullException("samples");
            return StreamingStatistics.StandardDeviation(samples.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Evaluates the biased population standard deviation from the provided full population.
        /// On a dataset of size N will use an N normalizer.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationStandardDeviation(this IEnumerable<double> population)
        {
            var array = population as double[];
            return array != null
                ? ArrayStatistics.PopulationStandardDeviation(array)
                : StreamingStatistics.PopulationStandardDeviation(population);
        }

        /// <summary>
        /// Evaluates the biased population standard deviation from the provided full population.
        /// On a dataset of size N will use an N normalizer.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationStandardDeviation(this IEnumerable<double?> population)
        {
            if (population == null) throw new ArgumentNullException("population");
            return StreamingStatistics.PopulationStandardDeviation(population.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the sample median from the provided samples (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double Median(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.MedianInplace(array);
        }

        /// <summary>
        /// Estimates the sample median from the provided samples (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double Median(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.MedianInplace(array);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double Quantile(this IEnumerable<double> data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.QuantileInplace(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double Quantile(this IEnumerable<double?> data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.QuantileInplace(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double,double> QuantileFunc(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.Quantile(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> QuantileFunc(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.Quantile(array, tau);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double InverseCDF(this IEnumerable<double> data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, QuantileDefinition.InverseCDF);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double InverseCDF(this IEnumerable<double?> data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, QuantileDefinition.InverseCDF);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> InverseCDFFunc(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, QuantileDefinition.InverseCDF);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> InverseCDFFunc(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, QuantileDefinition.InverseCDF);
        }

        /// <summary>
        /// stimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specificed to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static double QuantileCustom(this IEnumerable<double> data, double tau, QuantileDefinition definition)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, definition);
        }

        /// <summary>
        /// stimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specificed to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static double QuantileCustom(this IEnumerable<double?> data, double tau, QuantileDefinition definition)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, definition);
        }

        /// <summary>
        /// stimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specificed to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static Func<double, double> QuantileCustomFunc(this IEnumerable<double> data, QuantileDefinition definition)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, definition);
        }

        /// <summary>
        /// stimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specificed to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static Func<double, double> QuantileCustomFunc(this IEnumerable<double?> data, QuantileDefinition definition)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, definition);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="p">Percentile selector, between 0 and 100 (inclusive).</param>
        public static double Percentile(this IEnumerable<double> data, int p)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.PercentileInplace(array, p);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="p">Percentile selector, between 0 and 100 (inclusive).</param>
        public static double Percentile(this IEnumerable<double?> data, int p)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.PercentileInplace(array, p);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<int, double> PercentileFunc(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            Array.Sort(array);
            return p => SortedArrayStatistics.Percentile(array, p);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<int, double> PercentileFunc(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            Array.Sort(array);
            return p => SortedArrayStatistics.Percentile(array, p);
        }

        /// <summary>
        /// Estimates the first quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double LowerQuartile(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.LowerQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the first quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double LowerQuartile(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.LowerQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the third quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double UpperQuartile(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.UpperQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the third quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double UpperQuartile(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.UpperQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the inter-quartile range from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double InterquartileRange(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.InterquartileRangeInplace(array);
        }

        /// <summary>
        /// Estimates the inter-quartile range from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double InterquartileRange(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.InterquartileRangeInplace(array);
        }

        /// <summary>
        /// Estimates {min, lower-quantile, median, upper-quantile, max} from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double[] FiveNumberSummary(this IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.FiveNumberSummaryInplace(array);
        }

        /// <summary>
        /// Estimates {min, lower-quantile, median, upper-quantile, max} from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double[] FiveNumberSummary(this IEnumerable<double?> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.FiveNumberSummaryInplace(array);
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="order">One-based order of the statistic, must be between 1 and N (inclusive).</param>
        public static double OrderStatistic(IEnumerable<double> data, int order)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            return ArrayStatistics.OrderStatisticInplace(array, order);
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<int, double> OrderStatisticFunc(IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var array = data.ToArray();
            Array.Sort(array);
            return order => SortedArrayStatistics.OrderStatistic(array, order);
        }
    }
}
