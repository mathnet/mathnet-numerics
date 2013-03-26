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
    /// <summary>
    /// Statistics operating on arrays assumed to be unsorted.
    /// WARNING: Methods with the Inplace-suffix may modify the data array by reordering its entries.
    /// </summary>
    /// <seealso cref="SortedArrayStatistics"/>
    /// <seealso cref="StreamingStatistics"/>
    /// <seealso cref="Statistics"/>
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
        /// Estimates the unbiased population variance from the provided samples as unsorted array.
        /// On a dataset of size N will use an N-1 normalizer.
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample array, no sorting is assumed.</param>
        public static double Variance(double[] samples)
        {
            if (samples == null) throw new ArgumentNullException("samples");
            if (samples.Length <= 1) return double.NaN;

            double variance = 0;
            double t = samples[0];
            for (int i = 1; i < samples.Length; i++)
            {
                t += samples[i];
                double diff = ((i + 1)*samples[i]) - t;
                variance += (diff*diff)/((i + 1)*i);
            }
            return variance/(samples.Length - 1);
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples as unsorted array.
        /// On a dataset of size N will use an N-1 normalizer.
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample array, no sorting is assumed.</param>
        public static double StandardDeviation(double[] samples)
        {
            return Math.Sqrt(Variance(samples));
        }

        /// <summary>
        /// Evaluates the biased population variance from the provided full population as unsorted array.
        /// On a dataset of size N will use an N normalizer.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample array, no sorting is assumed.</param>
        public static double PopulationVariance(double[] population)
        {
            if (population == null) throw new ArgumentNullException("population");
            if (population.Length == 0) return double.NaN;

            double variance = 0;
            double t = population[0];
            for (int i = 1; i < population.Length; i++)
            {
                t += population[i];
                double diff = ((i + 1)*population[i]) - t;
                variance += (diff*diff)/((i + 1)*i);
            }
            return variance/population.Length;
        }

        /// <summary>
        /// Evaluates the biased population standard deviation from the provided full population as unsorted array.
        /// On a dataset of size N will use an N normalizer.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample array, no sorting is assumed.</param>
        public static double PopulationStandardDeviation(double[] population)
        {
            return Math.Sqrt(PopulationVariance(population));
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the unsorted data array.
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        /// <param name="order">One-based order of the statistic, must be between 1 and N (inclusive).</param>
        public static double OrderStatisticInplace(double[] data, int order)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (order < 1 || order > data.Length) return double.NaN;

            if (order == 1) return Minimum(data);
            if (order == data.Length) return Maximum(data);
            return SelectInplace(data, order - 1);
        }

        /// <summary>
        /// Estimates the median value from the unsorted data array.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        public static double MedianInplace(double[] data)
        {
            return QuantileInplace(data, 0.5d);
        }


        /// <summary>
        /// Estimates the p-Percentile value from the unsorted data array.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        /// <param name="p">Percentile selector, between 0 and 100 (inclusive).</param>
        public static double PercentileInplace(double[] data, int p)
        {
            return QuantileInplace(data, p / 100d);
        }

        /// <summary>
        /// Estimates the first quartile value from the unsorted data array.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        public static double LowerQuartileInplace(double[] data)
        {
            return QuantileInplace(data, 0.25d);
        }

        /// <summary>
        /// Estimates the third quartile value from the unsorted data array.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        public static double UpperQuartileInplace(double[] data)
        {
            return QuantileInplace(data, 0.75d);
        }

        /// <summary>
        /// Estimates the inter-quartile range from the unsorted data array.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        public static double InterquartileRangeInplace(double[] data)
        {
            return QuantileInplace(data, 0.75d) - QuantileInplace(data, 0.25d);
        }

        /// <summary>
        /// Estimates {min, lower-quantile, median, upper-quantile, max} from the unsorted data array.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        public static double[] FiveNumberSummaryInplace(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return new[] { double.NaN, double.NaN, double.NaN, double.NaN, double.NaN };

            // TODO: Benchmark: is this still faster than sorting the array then using SortedArrayStatistics instead?
            return new[] { Minimum(data), QuantileInplace(data, 0.25), QuantileInplace(data, 0.50), QuantileInplace(data, 0.75), Maximum(data) };
        }

        /// <summary>
        /// Estimates the tau-th quantile from the unsorted data array.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <remarks>
        /// R-8, SciPy-(1/3,1/3):
        /// Linear interpolation of the approximate medians for order statistics.
        /// When tau &lt; (2/3) / (N + 1/3), use x1. When tau &gt;= (N - 1/3) / (N + 1/3), use xN.
        /// </remarks>
        public static double QuantileInplace(double[] data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (tau < 0d || tau > 1d || data.Length == 0) return double.NaN;

            double h = (data.Length + 1d/3d)*tau + 1d/3d;
            var hf = (int) h;

            if (hf <= 0 || tau == 0d)
            {
                return Minimum(data);
            }
            if (hf >= data.Length || tau == 1d)
            {
                return Maximum(data);
            }

            var a = SelectInplace(data, hf - 1);
            var b = SelectInplace(data, hf);
            return a + (h - hf)*(b - a);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the unsorted data array.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile defintion can be specified
        /// by 4 parameters a, b, c and d, consistent with Mathematica.
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive)</param>
        public static double QuantileCustomInplace(double[] data, double tau, double a, double b, double c, double d)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (tau < 0d || tau > 1d || data.Length == 0) return double.NaN;

            var x = a + (data.Length + b) * tau - 1;
#if PORTABLE
            var ip = (int)x;
#else
            var ip = Math.Truncate(x);
#endif
            var fp = x - ip;

            if (Math.Abs(fp) < 1e-9)
            {
                return SelectInplace(data, (int) ip);
            }

            var lower = SelectInplace(data, (int) Math.Floor(x));
            var upper = SelectInplace(data, (int) Math.Ceiling(x));
            return lower + (upper - lower) * (c + d * fp);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the unsorted data array.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specificed to be compatible
        /// with an existing system.
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive)</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static double QuantileCustomInplace(double[] data, double tau, QuantileDefinition definition)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (tau < 0d || tau > 1d || data.Length == 0) return double.NaN;
            if (tau == 0d || data.Length == 1) return Minimum(data);
            if (tau == 1d) return Maximum(data);

            switch (definition)
            {
                case QuantileDefinition.R1:
                    {
                        double h = data.Length * tau + 0.5d;
                        return SelectInplace(data, (int)Math.Ceiling(h - 0.5d) - 1);
                    }
                case QuantileDefinition.R2:
                    {
                        double h = data.Length * tau + 0.5d;
                        return (SelectInplace(data, (int) Math.Ceiling(h - 0.5d) - 1) + SelectInplace(data, (int) (h + 0.5d) - 1))*0.5d;
                    }
                case QuantileDefinition.R3:
                    {
                        double h = data.Length * tau;
                        return SelectInplace(data, (int)Math.Round(h) - 1);
                    }
                case QuantileDefinition.R4:
                    {
                        double h = data.Length * tau;
                        var hf = (int)h;
                        var lower = SelectInplace(data, hf - 1);
                        var upper = SelectInplace(data, hf);
                        return lower + (h - hf) * (upper - lower);
                    }
                case QuantileDefinition.R5:
                    {
                        double h = data.Length * tau + 0.5d;
                        var hf = (int)h;
                        var lower = SelectInplace(data, hf - 1);
                        var upper = SelectInplace(data, hf);
                        return lower + (h - hf) * (upper - lower);
                    }
                case QuantileDefinition.R6:
                    {
                        double h = (data.Length + 1) * tau;
                        var hf = (int)h;
                        var lower = SelectInplace(data, hf - 1);
                        var upper = SelectInplace(data, hf);
                        return lower + (h - hf) * (upper - lower);
                    }
                case QuantileDefinition.R7:
                    {
                        double h = (data.Length - 1) * tau + 1d;
                        var hf = (int)h;
                        var lower = SelectInplace(data, hf - 1);
                        var upper = SelectInplace(data, hf);
                        return lower + (h - hf) * (upper - lower);
                    }
                case QuantileDefinition.R8:
                    {
                        double h = (data.Length + 1 / 3d) * tau + 1 / 3d;
                        var hf = (int)h;
                        var lower = SelectInplace(data, hf - 1);
                        var upper = SelectInplace(data, hf);
                        return lower + (h - hf) * (upper - lower);
                    }
                case QuantileDefinition.R9:
                    {
                        double h = (data.Length + 0.25d) * tau + 0.375d;
                        var hf = (int)h;
                        var lower = SelectInplace(data, hf - 1);
                        var upper = SelectInplace(data, hf);
                        return lower + (h - hf) * (upper - lower);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        static double SelectInplace(double[] workingData, int rank)
        {
            // Numerical Recipes: select
            // http://en.wikipedia.org/wiki/Selection_algorithm

            if (rank <= 0) return Minimum(workingData);
            if (rank >= workingData.Length - 1) return Maximum(workingData);

            var a = workingData;
            int low = 0;
            int high = a.Length - 1;

            while (true)
            {
                if (high <= low + 1)
                {
                    if (high == low + 1 && a[high] < a[low])
                    {
                        var tmp = a[low];
                        a[low] = a[high];
                        a[high] = tmp;
                    }

                    return a[rank];
                }

                int middle = (low + high) >> 1;

                var tmp1 = a[middle];
                a[middle] = a[low + 1];
                a[low + 1] = tmp1;

                if (a[low] > a[high])
                {
                    var tmp = a[low];
                    a[low] = a[high];
                    a[high] = tmp;
                }
                if (a[low + 1] > a[high])
                {
                    var tmp = a[low + 1];
                    a[low + 1] = a[high];
                    a[high] = tmp;
                }
                if (a[low] > a[low + 1])
                {
                    var tmp = a[low];
                    a[low] = a[low + 1];
                    a[low + 1] = tmp;
                }

                int begin = low + 1;
                int end = high;
                double pivot = a[begin];

                while (true)
                {
                    do begin++; while (a[begin] < pivot);
                    do end--; while (a[end] > pivot);

                    if (end < begin) break;

                    var tmp = a[begin];
                    a[begin] = a[end];
                    a[end] = tmp;
                }

                a[low + 1] = a[end];
                a[end] = pivot;

                if (end >= rank) high = end - 1;
                if (end <= rank) low = begin;
            }
        }
    }
}