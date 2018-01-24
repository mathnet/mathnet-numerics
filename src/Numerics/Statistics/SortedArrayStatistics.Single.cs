// <copyright file="SortedArrayStatistics.Single.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

namespace MathNet.Numerics.Statistics
{
    public static partial class SortedArrayStatistics
    {
        /// <summary>
        /// Returns the smallest value from the sorted data array (ascending).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static float Minimum(float[] data)
        {
            if (data.Length == 0)
            {
                return float.NaN;
            }

            return data[0];
        }

        /// <summary>
        /// Returns the largest value from the sorted data array (ascending).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static float Maximum(float[] data)
        {
            if (data.Length == 0)
            {
                return float.NaN;
            }

            return data[data.Length - 1];
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the sorted data array (ascending).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="order">One-based order of the statistic, must be between 1 and N (inclusive).</param>
        public static float OrderStatistic(float[] data, int order)
        {
            if (order < 1 || order > data.Length)
            {
                return float.NaN;
            }

            return data[order - 1];
        }

        /// <summary>
        /// Estimates the median value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static float Median(float[] data)
        {
            if (data.Length == 0)
            {
                return float.NaN;
            }

            var k = data.Length/2;
            return data.Length.IsOdd()
                ? data[k]
                : (data[k - 1] + data[k])/2.0f;
        }

        /// <summary>
        /// Estimates the p-Percentile value from the sorted data array (ascending).
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="p">Percentile selector, between 0 and 100 (inclusive).</param>
        public static float Percentile(float[] data, int p)
        {
            return Quantile(data, p/100d);
        }

        /// <summary>
        /// Estimates the first quartile value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static float LowerQuartile(float[] data)
        {
            return Quantile(data, 0.25d);
        }

        /// <summary>
        /// Estimates the third quartile value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static float UpperQuartile(float[] data)
        {
            return Quantile(data, 0.75d);
        }

        /// <summary>
        /// Estimates the inter-quartile range from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static float InterquartileRange(float[] data)
        {
            return Quantile(data, 0.75d) - Quantile(data, 0.25d);
        }

        /// <summary>
        /// Estimates {min, lower-quantile, median, upper-quantile, max} from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        public static float[] FiveNumberSummary(float[] data)
        {
            if (data.Length == 0)
            {
                return new[] { float.NaN, float.NaN, float.NaN, float.NaN, float.NaN };
            }

            return new[] { data[0], Quantile(data, 0.25), Median(data), Quantile(data, 0.75), data[data.Length - 1] };
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
        public static float Quantile(float[] data, double tau)
        {
            if (tau < 0d || tau > 1d || data.Length == 0)
            {
                return float.NaN;
            }

            if (tau == 0d || data.Length == 1)
            {
                return data[0];
            }

            if (tau == 1d)
            {
                return data[data.Length - 1];
            }

            double h = (data.Length + 1/3d)*tau + 1/3d;
            var hf = (int)h;
            return hf < 1
                ? data[0]
                : hf >= data.Length
                    ? data[data.Length - 1]
                    : (float)(data[hf - 1] + (h - hf)*(data[hf] - data[hf - 1]));
        }

        /// <summary>
        /// Estimates the tau-th quantile from the sorted data array (ascending).
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified
        /// by 4 parameters a, b, c and d, consistent with Mathematica.
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <param name="a">a-parameter</param>
        /// <param name="b">b-parameter</param>
        /// <param name="c">c-parameter</param>
        /// <param name="d">d-parameter</param>
        public static float QuantileCustom(float[] data, double tau, double a, double b, double c, double d)
        {
            if (tau < 0d || tau > 1d || data.Length == 0)
            {
                return float.NaN;
            }

            var x = a + (data.Length + b)*tau - 1;
            var ip = Math.Truncate(x);
            var fp = x - ip;

            if (Math.Abs(fp) < 1e-9)
            {
                return data[Math.Min(Math.Max((int)ip, 0), data.Length - 1)];
            }

            var lower = data[Math.Max((int)Math.Floor(x), 0)];
            var upper = data[Math.Min((int)Math.Ceiling(x), data.Length - 1)];
            return (float)(lower + (upper - lower)*(c + d*fp));
        }

        /// <summary>
        /// Estimates the tau-th quantile from the sorted data array (ascending).
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static float QuantileCustom(float[] data, double tau, QuantileDefinition definition)
        {
            if (tau < 0d || tau > 1d || data.Length == 0)
            {
                return float.NaN;
            }

            if (tau == 0d || data.Length == 1)
            {
                return data[0];
            }

            if (tau == 1d)
            {
                return data[data.Length - 1];
            }

            switch (definition)
            {
                case QuantileDefinition.R1:
                {
                    double h = data.Length*tau + 0.5d;
                    return data[(int)Math.Ceiling(h - 0.5d) - 1];
                }

                case QuantileDefinition.R2:
                {
                    double h = data.Length*tau + 0.5d;
                    return (data[(int)Math.Ceiling(h - 0.5d) - 1] + data[(int)(h + 0.5d) - 1])*0.5f;
                }

                case QuantileDefinition.R3:
                {
                    double h = data.Length*tau;
                    return data[Math.Max((int)Math.Round(h) - 1, 0)];
                }

                case QuantileDefinition.R4:
                {
                    double h = data.Length*tau;
                    var hf = (int)h;
                    var lower = data[Math.Max(hf - 1, 0)];
                    var upper = data[Math.Min(hf, data.Length - 1)];
                    return (float)(lower + (h - hf)*(upper - lower));
                }

                case QuantileDefinition.R5:
                {
                    double h = data.Length*tau + 0.5d;
                    var hf = (int)h;
                    var lower = data[Math.Max(hf - 1, 0)];
                    var upper = data[Math.Min(hf, data.Length - 1)];
                    return (float)(lower + (h - hf)*(upper - lower));
                }

                case QuantileDefinition.R6:
                {
                    double h = (data.Length + 1)*tau;
                    var hf = (int)h;
                    var lower = data[Math.Max(hf - 1, 0)];
                    var upper = data[Math.Min(hf, data.Length - 1)];
                    return (float)(lower + (h - hf)*(upper - lower));
                }

                case QuantileDefinition.R7:
                {
                    double h = (data.Length - 1)*tau + 1d;
                    var hf = (int)h;
                    var lower = data[Math.Max(hf - 1, 0)];
                    var upper = data[Math.Min(hf, data.Length - 1)];
                    return (float)(lower + (h - hf)*(upper - lower));
                }

                case QuantileDefinition.R8:
                {
                    double h = (data.Length + 1/3d)*tau + 1/3d;
                    var hf = (int)h;
                    var lower = data[Math.Max(hf - 1, 0)];
                    var upper = data[Math.Min(hf, data.Length - 1)];
                    return (float)(lower + (h - hf)*(upper - lower));
                }

                case QuantileDefinition.R9:
                {
                    double h = (data.Length + 0.25d)*tau + 0.375d;
                    var hf = (int)h;
                    var lower = data[Math.Max(hf - 1, 0)];
                    var upper = data[Math.Min(hf, data.Length - 1)];
                    return (float)(lower + (h - hf)*(upper - lower));
                }

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Estimates the empirical cumulative distribution function (CDF) at x from the sorted data array (ascending).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="x">The value where to estimate the CDF at.</param>
        public static double EmpiricalCDF(float[] data, float x)
        {
            if (x < data[0])
            {
                return 0.0;
            }

            if (x >= data[data.Length - 1])
            {
                return 1.0;
            }

            int right = Array.BinarySearch(data, x);
            if (right >= 0)
            {
                while (right < data.Length - 1 && data[right + 1] == data[right])
                {
                    right++;
                }

                return (right + 1)/(double)data.Length;
            }

            return (~right)/(double)data.Length;
        }

        /// <summary>
        /// Estimates the quantile tau from the sorted data array (ascending).
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="x">Quantile value.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static double QuantileRank(float[] data, float x, RankDefinition definition = RankDefinition.Default)
        {
            if (x < data[0])
            {
                return 0.0;
            }

            if (x >= data[data.Length - 1])
            {
                return 1.0;
            }

            int right = Array.BinarySearch(data, x);
            if (right >= 0)
            {
                int left = right;

                while (left > 0 && data[left - 1] == data[left])
                {
                    left--;
                }

                while (right < data.Length - 1 && data[right + 1] == data[right])
                {
                    right++;
                }

                switch (definition)
                {
                    case RankDefinition.EmpiricalCDF:
                        return (right + 1)/(double)data.Length;

                    case RankDefinition.Max:
                        return right/(double)(data.Length - 1);

                    case RankDefinition.Min:
                        return left/(double)(data.Length - 1);

                    case RankDefinition.Average:
                        return (left/(double)(data.Length - 1) + right/(double)(data.Length - 1))/2;

                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                right = ~right;
                int left = right - 1;

                switch (definition)
                {
                    case RankDefinition.EmpiricalCDF:
                        return (left + 1)/(double)data.Length;

                    default:
                    {
                        var a = left/(double)(data.Length - 1);
                        var b = right/(double)(data.Length - 1);
                        return ((data[right] - x)*a + (x - data[left])*b)/(data[right] - data[left]);
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates the rank of each entry of the sorted data array (ascending).
        /// The rank definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        public static double[] Ranks(float[] data, RankDefinition definition = RankDefinition.Default)
        {
            var ranks = new double[data.Length];

            if (definition == RankDefinition.First)
            {
                for (int i = 0; i < ranks.Length; i++)
                {
                    ranks[i] = i + 1;
                }

                return ranks;
            }

            int previousIndex = 0;
            for (int i = 1; i < data.Length; i++)
            {
                if (Math.Abs(data[i] - data[previousIndex]) <= 0d)
                {
                    continue;
                }

                if (i == previousIndex + 1)
                {
                    ranks[previousIndex] = i;
                }
                else
                {
                    RanksTies(ranks, previousIndex, i, definition);
                }

                previousIndex = i;
            }

            RanksTies(ranks, previousIndex, data.Length, definition);
            return ranks;
        }
    }
}
