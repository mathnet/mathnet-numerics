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
    /// Recommendation: consider to use RunningStatistics instead.
    /// </summary>
    /// <remarks>
    /// This type declares a DataContract for out of the box ephemeral serialization
    /// with engines like DataContractSerializer, Protocol Buffers and FsPickler,
    /// but does not guarantee any compatibility between versions.
    /// It is not recommended to rely on this mechanism for durable persistence.
    /// </remarks>
    [DataContract(Namespace = "urn:MathNet/Numerics")]
    public class DescriptiveStatistics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptiveStatistics"/> class.
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
        public DescriptiveStatistics(IEnumerable<double> data, bool increasedAccuracy = false)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (increasedAccuracy)
            {
                ComputeDecimal(data);
            }
            else
            {
                Compute(data);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptiveStatistics"/> class.
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
        public DescriptiveStatistics(IEnumerable<double?> data, bool increasedAccuracy = false)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (increasedAccuracy)
            {
                ComputeDecimal(data);
            }
            else
            {
                Compute(data);
            }
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptiveStatistics"/> class.
        /// </summary>
        /// <remarks>
        /// Used for Json serialization
        /// </remarks>
        public DescriptiveStatistics()
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
        /// Gets the sample skewness.
        /// </summary>
        /// <value>The sample skewness.</value>
        /// <remarks>Returns zero if <see cref="Count"/> is less than three. </remarks>
        [DataMember(Order = 5)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
        public double Skewness { get; private set; }

        /// <summary>
        /// Gets the sample kurtosis.
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
        /// Computes descriptive statistics from a stream of data values.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        void Compute(IEnumerable<double> data)
        {
            double mean = 0;
            double variance = 0;
            double skewness = 0;
            double kurtosis = 0;
            double minimum = double.PositiveInfinity;
            double maximum = double.NegativeInfinity;
            long n = 0;

            foreach (var xi in data)
            {
                double delta = xi - mean;
                double scaleDelta = delta/++n;
                double scaleDeltaSqr = scaleDelta*scaleDelta;
                double tmpDelta = delta*(n - 1);

                mean += scaleDelta;

                kurtosis += tmpDelta*scaleDelta*scaleDeltaSqr*(n*n - 3*n + 3)
                            + 6*scaleDeltaSqr*variance - 4*scaleDelta*skewness;

                skewness += tmpDelta*scaleDeltaSqr*(n - 2) - 3*scaleDelta*variance;
                variance += tmpDelta*scaleDelta;

                if (minimum > xi)
                {
                    minimum = xi;
                }

                if (maximum < xi)
                {
                    maximum = xi;
                }
            }

            SetStatistics(mean, variance, skewness, kurtosis, minimum, maximum, n);
        }

        /// <summary>
        /// Computes descriptive statistics from a stream of nullable data values.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        void Compute(IEnumerable<double?> data)
        {
            double mean = 0;
            double variance = 0;
            double skewness = 0;
            double kurtosis = 0;
            double minimum = double.PositiveInfinity;
            double maximum = double.NegativeInfinity;
            long n = 0;

            foreach (var xi in data)
            {
                if (xi.HasValue)
                {
                    double delta = xi.Value - mean;
                    double scaleDelta = delta/++n;
                    double scaleDeltaSqr = scaleDelta*scaleDelta;
                    double tmpDelta = delta*(n - 1);

                    mean += scaleDelta;

                    kurtosis += tmpDelta*scaleDelta*scaleDeltaSqr*(n*n - 3*n + 3)
                                + 6*scaleDeltaSqr*variance - 4*scaleDelta*skewness;

                    skewness += tmpDelta*scaleDeltaSqr*(n - 2) - 3*scaleDelta*variance;
                    variance += tmpDelta*scaleDelta;

                    if (minimum > xi)
                    {
                        minimum = xi.Value;
                    }

                    if (maximum < xi)
                    {
                        maximum = xi.Value;
                    }
                }
            }

            SetStatistics(mean, variance, skewness, kurtosis, minimum, maximum, n);
        }

        /// <summary>
        /// Computes descriptive statistics from a stream of data values.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        void ComputeDecimal(IEnumerable<double> data)
        {
            decimal mean = 0;
            decimal variance = 0;
            decimal skewness = 0;
            decimal kurtosis = 0;
            decimal minimum = decimal.MaxValue;
            decimal maximum = decimal.MinValue;
            long n = 0;

            foreach (double x in data)
            {
                decimal xi = (decimal)x;
                decimal delta = xi - mean;
                decimal scaleDelta = delta/++n;
                decimal scaleDelta2 = scaleDelta*scaleDelta;
                decimal tmpDelta = delta*(n - 1);

                mean += scaleDelta;

                kurtosis += tmpDelta*scaleDelta*scaleDelta2*(n*n - 3*n + 3)
                            + 6*scaleDelta2*variance - 4*scaleDelta*skewness;

                skewness += tmpDelta*scaleDelta2*(n - 2) - 3*scaleDelta*variance;
                variance += tmpDelta*scaleDelta;

                if (minimum > xi)
                {
                    minimum = xi;
                }

                if (maximum < xi)
                {
                    maximum = xi;
                }
            }

            SetStatistics((double)mean, (double)variance, (double)skewness, (double)kurtosis, (double)minimum, (double)maximum, n);
        }

        /// <summary>
        /// Computes descriptive statistics from a stream of nullable data values.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        void ComputeDecimal(IEnumerable<double?> data)
        {
            decimal mean = 0;
            decimal variance = 0;
            decimal skewness = 0;
            decimal kurtosis = 0;
            decimal minimum = decimal.MaxValue;
            decimal maximum = decimal.MinValue;
            long n = 0;

            foreach (double? x in data)
            {
                if (x.HasValue)
                {
                    decimal xi = (decimal)x.Value;
                    decimal delta = xi - mean;
                    decimal scaleDelta = delta/++n;
                    decimal scaleDeltaSquared = scaleDelta*scaleDelta;
                    decimal tmpDelta = delta*(n - 1);

                    mean += scaleDelta;

                    kurtosis += tmpDelta*scaleDelta*scaleDeltaSquared*(n*n - 3*n + 3)
                                + 6*scaleDeltaSquared*variance - 4*scaleDelta*skewness;

                    skewness += tmpDelta*scaleDeltaSquared*(n - 2) - 3*scaleDelta*variance;
                    variance += tmpDelta*scaleDelta;

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

            SetStatistics((double)mean, (double)variance, (double)skewness, (double)kurtosis, (double)minimum, (double)maximum, n);
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
        void SetStatistics(double mean, double variance, double skewness, double kurtosis, double minimum, double maximum, long n)
        {
            Mean = mean;
            Count = n;

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
                    Variance = variance/(n - 1);
                    StandardDeviation = Math.Sqrt(Variance);
                }

                if (Variance != 0)
                {
                    if (n > 2)
                    {
                        Skewness = (double)n/((n - 1)*(n - 2))*(skewness/(Variance*StandardDeviation));
                    }

                    if (n > 3)
                    {
                        Kurtosis = ((double)n*n - 1)/((n - 2)*(n - 3))
                                   *(n*kurtosis/(variance*variance) - 3 + 6.0/(n + 1));
                    }
                }
            }
        }
    }
}
