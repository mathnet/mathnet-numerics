// <copyright file="SortedArrayStatistics.cs" company="Math.NET">
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
    /// Statistics operating on an array already sorted ascendingly.
    /// </summary>
    /// <seealso cref="ArrayStatistics"/>
    /// <seealso cref="StreamingStatistics"/>
    /// <seealso cref="Statistics"/>
    public static class SortedArrayStatistics
    {
        /// <summary>
        /// Returns the smallest value from the sorted data array (ascending).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static double Minimum(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return double.NaN;

            return data[0];
        }

        /// <summary>
        /// Returns the largest value from the sorted data array (ascending).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static double Maximum(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return double.NaN;

            return data[data.Length - 1];
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the sorted data array (ascending).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="order">One-based order of the statistic, must be between 1 and N (inclusive).</param>
        public static double OrderStatistic(double[] data, int order)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (order < 1 || order > data.Length) return double.NaN;

            return data[order - 1];
        }

        /// <summary>
        /// Estimates the median value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static double Median(double[] data)
        {
            return Quantile(data, 0.5d);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the sorted data array (ascending).
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="p">Percentile selector, between 0 and 100 (inclusive).</param>
        public static double Percentile(double[] data, int p)
        {
            return Quantile(data, p/100d);
        }

        /// <summary>
        /// Estimates the first quartile value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static double LowerQuartile(double[] data)
        {
            return Quantile(data, 0.25d);
        }

        /// <summary>
        /// Estimates the third quartile value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static double UpperQuartile(double[] data)
        {
            return Quantile(data, 0.75d);
        }

        /// <summary>
        /// Estimates the inter-quartile range from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static double InterquartileRange(double[] data)
        {
            return Quantile(data, 0.75d) - Quantile(data, 0.25d);
        }

        /// <summary>
        /// Estimates {min, lower-quantile, median, upper-quantile, max} from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static double[] FiveNumberSummary(double[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return new[] {double.NaN, double.NaN, double.NaN, double.NaN, double.NaN};
            return new[] {data[0], Quantile(data, 0.25), Quantile(data, 0.50), Quantile(data, 0.75), data[data.Length - 1]};
        }

        /// <summary>
        /// Estimates the tau-th quantile from the sorted data array (ascending).
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <remarks>
        /// R-8, SciPy-(1/3,1/3):
        /// Linear interpolation of the approximate medians for order statistics.
        /// When tau &lt; (2/3) / (N + 1/3), use x1. When tau &gt;= (N - 1/3) / (N + 1/3), use xN.
        /// </remarks>
        public static double Quantile(double[] data, double tau)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (tau < 0d || tau > 1d || data.Length == 0) return double.NaN;
            if (tau == 0d || data.Length == 1) return data[0];
            if (tau == 1d) return data[data.Length - 1];

            double h = (data.Length + 1/3d)*tau + 1/3d;
            var hf = (int) h;
            return hf < 1 ? data[0]
                : hf >= data.Length ? data[data.Length - 1]
                : data[hf - 1] + (h - hf)*(data[hf] - data[hf - 1]);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the sorted data array (ascending).
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile defintion can be specified
        /// by 4 parameters a, b, c and d, consistent with Mathematica.
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double QuantileCustom(double[] data, double tau, double a, double b, double c, double d)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (tau < 0d || tau > 1d || data.Length == 0) return double.NaN;

            var x = a + (data.Length + b)*tau - 1;
#if PORTABLE
            var ip = (int) x;
#else
            var ip = Math.Truncate(x);
#endif
            var fp = x - ip;

            if (Math.Abs(fp) < 1e-9)
            {
                return data[Math.Min(Math.Max((int) ip, 0), data.Length - 1)];
            }

            var lower = data[Math.Max((int) Math.Floor(x), 0)];
            var upper = data[Math.Min((int) Math.Ceiling(x), data.Length - 1)];
            return lower + (upper - lower)*(c + d*fp);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the sorted data array (ascending).
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specificed to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static double QuantileCustom(double[] data, double tau, QuantileDefinition definition)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (tau < 0d || tau > 1d || data.Length == 0) return double.NaN;
            if (tau == 0d || data.Length == 1) return data[0];
            if (tau == 1d) return data[data.Length - 1];

            switch (definition)
            {
                case QuantileDefinition.R1:
                    {
                        double h = data.Length*tau + 0.5d;
                        return data[(int) Math.Ceiling(h - 0.5d) - 1];
                    }
                case QuantileDefinition.R2:
                    {
                        double h = data.Length*tau + 0.5d;
                        return (data[(int) Math.Ceiling(h - 0.5d) - 1] + data[(int) (h + 0.5d) - 1])*0.5d;
                    }
                case QuantileDefinition.R3:
                    {
                        double h = data.Length*tau;
                        return data[Math.Max((int) Math.Round(h) - 1, 0)];
                    }
                case QuantileDefinition.R4:
                    {
                        double h = data.Length*tau;
                        var hf = (int) h;
                        var lower = data[Math.Max(hf - 1, 0)];
                        var upper = data[Math.Min(hf, data.Length - 1)];
                        return lower + (h - hf)*(upper - lower);
                    }
                case QuantileDefinition.R5:
                    {
                        double h = data.Length*tau + 0.5d;
                        var hf = (int) h;
                        var lower = data[Math.Max(hf - 1, 0)];
                        var upper = data[Math.Min(hf, data.Length - 1)];
                        return lower + (h - hf)*(upper - lower);
                    }
                case QuantileDefinition.R6:
                    {
                        double h = (data.Length + 1)*tau;
                        var hf = (int) h;
                        var lower = data[Math.Max(hf - 1, 0)];
                        var upper = data[Math.Min(hf, data.Length - 1)];
                        return lower + (h - hf)*(upper - lower);
                    }
                case QuantileDefinition.R7:
                    {
                        double h = (data.Length - 1)*tau + 1d;
                        var hf = (int) h;
                        var lower = data[Math.Max(hf - 1, 0)];
                        var upper = data[Math.Min(hf, data.Length - 1)];
                        return lower + (h - hf)*(upper - lower);
                    }
                case QuantileDefinition.R8:
                    {
                        double h = (data.Length + 1/3d)*tau + 1/3d;
                        var hf = (int) h;
                        var lower = data[Math.Max(hf - 1, 0)];
                        var upper = data[Math.Min(hf, data.Length - 1)];
                        return lower + (h - hf)*(upper - lower);
                    }
                case QuantileDefinition.R9:
                    {
                        double h = (data.Length + 0.25d)*tau + 0.375d;
                        var hf = (int) h;
                        var lower = data[Math.Max(hf - 1, 0)];
                        var upper = data[Math.Min(hf, data.Length - 1)];
                        return lower + (h - hf)*(upper - lower);
                    }
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
