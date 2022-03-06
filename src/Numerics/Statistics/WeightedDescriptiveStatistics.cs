// <copyright file="DescriptiveStatistics.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
#if NET5_0_OR_GREATER
using System.Text.Json.Serialization;
#endif

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Computes the basic statistics of data set. The class meets the
    /// NIST standard of accuracy for mean, variance, and standard deviation
    /// (the only statistics they provide exact values for) and exceeds them
    /// in increased accuracy mode.
    /// Recommendation: consider to use RunningWeightedStatistics instead.
    /// </summary>
    /// <remarks>
    /// This type declares a DataContract for out of the box ephemeral serialization
    /// with engines like DataContractSerializer, Protocol Buffers and FsPickler,
    /// but does not guarantee any compatibility between versions.
    /// It is not recommended to rely on this mechanism for durable persistence.
    /// </remarks>
    [DataContract(Namespace = "urn:MathNet/Numerics")]
    public class WeightedDescriptiveStatistics
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="WeightedDescriptiveStatistics"/> class.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <param name="increasedAccuracy">
        /// If set to <c>true</c>, increased accuracy mode used.
        /// Increased accuracy mode uses <see cref="decimal"/> types for internal calculations.
        /// </param>
        /// <remarks>
        /// Don't use increased accuracy for data sets containing large values (in absolute value).
        /// This may cause the calculations to overflow.
        /// </remarks>
        public WeightedDescriptiveStatistics(IEnumerable<Tuple<double, double>> data, bool increasedAccuracy = false)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var data2 = data.Select(x => (x.Item1, x.Item2));
            if (increasedAccuracy)
                ComputeDecimal(data2);
            else
                Compute(data2);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeightedDescriptiveStatistics"/> class.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <param name="increasedAccuracy">
        /// If set to <c>true</c>, increased accuracy mode used.
        /// Increased accuracy mode uses <see cref="decimal"/> types for internal calculations.
        /// </param>
        /// <remarks>
        /// Don't use increased accuracy for data sets containing large values (in absolute value).
        /// This may cause the calculations to overflow.
        /// </remarks>
        public WeightedDescriptiveStatistics(IEnumerable<(double, double)> data, bool increasedAccuracy = false)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (increasedAccuracy)
                ComputeDecimal(data);
            else
                Compute(data);
        }
#if NET5_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptiveStatistics"/> class.
        /// </summary>
        /// <remarks>
        /// Used for Json serialization
        /// </remarks>
        public WeightedDescriptiveStatistics()
        {
        }
#endif

        /// <summary>
        /// Gets the size of the sample.
        /// </summary>
        /// <value>The size of the sample.</value>
        [DataMember(Order = 1)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public long Count { get; private set; }

        /// <summary>
        /// Gets the sample mean.
        /// </summary>
        /// <value>The sample mean.</value>
        [DataMember(Order = 2)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double Mean { get; private set; }

        /// <summary>
        /// Gets the unbiased population variance estimator (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <value>The sample variance.</value>
        [DataMember(Order = 3)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double Variance { get; private set; }

        /// <summary>
        /// Gets the unbiased population standard deviation (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <value>The sample standard deviation.</value>
        [DataMember(Order = 4)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double StandardDeviation { get; private set; }

        /// <summary>
        /// Gets the unbiased estimator of the population skewness.
        /// </summary>
        /// <value>The sample skewness.</value>
        /// <remarks>Returns zero if <see cref="Count"/> is less than three. </remarks>
        [DataMember(Order = 5)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double Skewness { get; private set; }

        /// <summary>
        /// Gets the unbiased estimator of the population excess kurtosis using the G_2 estimator.
        /// </summary>
        /// <value>The sample kurtosis.</value>
        /// <remarks>Returns zero if <see cref="Count"/> is less than four. </remarks>
        [DataMember(Order = 6)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double Kurtosis { get; private set; }

        /// <summary>
        /// Gets the maximum sample value.
        /// </summary>
        /// <value>The maximum sample value.</value>
        [DataMember(Order = 7)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double Maximum { get; private set; }

        /// <summary>
        /// Gets the minimum sample value.
        /// </summary>
        /// <value>The minimum sample value.</value>
        [DataMember(Order = 8)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double Minimum { get; private set; }


        /// <summary>
        /// Gets the total weight. When used with unweighted data, returns the number of samples.
        /// </summary>
        /// <value>The total weight.</value>
        [DataMember(Order = 9)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double TotalWeight { get; private set; }

        /// <summary>
        /// The Kish's effective sample size https://en.wikipedia.org/wiki/Effective_sample_size
        /// </summary>
        /// <value>The Kish's effective sample size.</value>
        [DataMember(Order = 10)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double EffectiveSampleSize { get; private set; }

        /// <summary>
        /// Computes descriptive statistics from a stream of data values and reliability weights.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        void Compute(IEnumerable<(double, double)> data)
        {
            double mean = 0;
            double variance = 0;
            double skewness = 0;
            double kurtosis = 0;
            double minimum = double.PositiveInfinity;
            double maximum = double.NegativeInfinity;
            long n = 0;
            double totalWeight = 0;
            double den = 0;

            double V2 = 0;
            double V3 = 0;
            double V4 = 0;

            foreach (var (w, xi) in data)
            {
                if (w < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(data), w, "Expected non-negative weighting of sample");
                }
                else if (w > 0)
                {
                    ++n;
                    double delta = xi - mean;
                    double prevWeight = totalWeight;
                    totalWeight += w;
                    V2 += w * w;
                    V3 += w * w * w;
                    V4 += w * w * w * w;

                    den += w * (2.0 * prevWeight - den) / totalWeight;

                    double scaleDelta = delta * w / totalWeight;
                    double scaleDeltaSqr = scaleDelta * scaleDelta;
                    double tmpDelta = delta * scaleDelta * prevWeight;
                    double r = prevWeight / w;

                    mean += scaleDelta;

                    kurtosis += tmpDelta * scaleDeltaSqr * (r * r - r + 1.0)
                                + 6.0 * scaleDeltaSqr * variance - 4.0 * scaleDelta * skewness;

                    skewness += tmpDelta * scaleDelta * (r - 1.0) - 3.0 * scaleDelta * variance;
                    variance += tmpDelta;

                    if (minimum > xi)
                    {
                        minimum = xi;
                    }

                    if (maximum < xi)
                    {
                        maximum = xi;
                    }
                }
            }

            SetStatisticsWeighted(mean, variance, skewness, kurtosis, minimum, maximum, n, totalWeight, den, V2, V3, V4);
        }

        /// <summary>
        /// Computes descriptive statistics from a stream of data values and reliability weights.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        void ComputeDecimal(IEnumerable<(double, double)> data)
        {
            decimal mean = 0;
            decimal variance = 0;
            decimal skewness = 0;
            decimal kurtosis = 0;
            decimal minimum = decimal.MaxValue;
            decimal maximum = decimal.MinValue;
            decimal totalWeight = 0;
            long n = 0;
            decimal den = 0;

            decimal V2 = 0;
            decimal V3 = 0;
            decimal V4 = 0;

            foreach (var (w, x) in data)
            {
                if (w < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(data), w, "Expected non-negative weighting of sample");
                }
                else if (w > 0)
                {

                    decimal xi = (decimal)x;
                    decimal decW = (decimal)w;
                    ++n;
                    decimal delta = xi - mean;
                    decimal prevWeight = totalWeight;
                    totalWeight += decW;
                    V2 += decW * decW;
                    V3 += decW * decW * decW;
                    V4 += decW * decW * decW * decW;
                    den += decW * (2.0m * prevWeight - den) / totalWeight;
                    decimal scaleDelta = delta * decW / totalWeight;
                    decimal scaleDeltaSqr = scaleDelta * scaleDelta;
                    decimal tmpDelta = delta * scaleDelta * prevWeight;
                    decimal r = prevWeight / decW;

                    mean += scaleDelta;

                    kurtosis += tmpDelta * scaleDeltaSqr * (r * r - r + 1.0m)
                                + 6.0m * scaleDeltaSqr * variance - 4.0m * scaleDelta * skewness;

                    skewness += tmpDelta * scaleDelta * (r - 1.0m) - 3.0m * scaleDelta * variance;
                    variance += tmpDelta;

                    if (minimum > xi)
                    {
                        minimum = xi;
                    }

                    if (maximum < xi)
                    {
                        maximum = xi;
                    }
                }
            }

            SetStatisticsWeighted((double)mean, (double)variance, (double)skewness, (double)kurtosis, (double)minimum, (double)maximum, n, (double)totalWeight, (double)den, (double)V2, (double)V3, (double)V4);
        }

        /// <summary>
        /// Internal use. Method use for setting the statistics.
        /// </summary>
        /// <param name="mean">For setting Mean.</param>
        /// <param name="variance">For setting Variance.</param>
        /// <param name="skewness">For setting Skewness.</param>
        /// <param name="kurtosis">For setting Kurtosis.</param>
        /// <param name="minimum">For setting Minimum.</param>
        /// <param name="maximum">For setting Maximum.</param>
        /// <param name="n">For setting Count.</param>
        void SetStatisticsWeighted(double mean, double variance, double skewness, double kurtosis, double minimum, double maximum, long n, double w1, double den, double w2, double w3, double w4)
        {
            Mean = mean;
            Count = n;
            TotalWeight = w1;
            EffectiveSampleSize = w1 * w1 / w2;
            Minimum = double.NaN;
            Maximum = double.NaN;
            Variance = double.NaN;
            StandardDeviation = double.NaN;
            Skewness = double.NaN;
            Kurtosis = double.NaN;

            if (n > 0)
            {
                Minimum = minimum;
                Maximum = maximum;

                if (n > 1)
                {
                    Variance = variance / den;
                    StandardDeviation = Math.Sqrt(Variance);
                }

                if (Variance != 0)
                {
                    if (n > 2)
                    {
                        var skewDen = (w1 * (w1 * w1 - 3.0 * w2) + 2.0 * w3) / (w1 * w1);
                        Skewness = skewness / (skewDen * Variance * StandardDeviation);
                    }

                    if (n > 3)
                    {
                        double p2 = w1 * w1;
                        double p4 = p2 * p2;
                        double w2p2 = w2 * w2;
                        double poly = p4 - 6.0 * p2 * w2 + 8.0 * w1 * w3 + 3.0 * w2p2 - 6.0 * w4;
                        double a = p4 - 4.0 * w1 * w3 + 3.0 * w2p2;
                        double b = 3.0 * (p4 - 2.0 * p2 * w2 + 4.0 * w1 * w3 - 3.0 * w2p2);
                        Kurtosis = (a * w1 * kurtosis / (variance * variance) - b) * (den / (w1 * poly));
                    }
                }
            }
        }
    }
}
