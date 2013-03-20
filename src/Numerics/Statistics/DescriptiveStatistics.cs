// <copyright file="DescriptiveStatistics.cs" company="Math.NET">
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

    /// <summary>
    /// Computes the basic statistics of data set. The class meets the
    /// NIST standard of accuracy for mean, variance, and standard deviation
    /// (the only statistics they provide exact values for) and exceeds them 
    /// in increased accuracy mode.
    /// </summary>
    public class DescriptiveStatistics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptiveStatistics"/> class.
        /// </summary>
        /// <param name="data">The sample data.</param>
        public DescriptiveStatistics(IEnumerable<double> data)
            : this(data, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptiveStatistics"/> class.
        /// </summary>
        /// <param name="data">The sample data.</param>
        public DescriptiveStatistics(IEnumerable<double?> data)
            : this(data, false)
        {
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
        public DescriptiveStatistics(IEnumerable<double> data, bool increasedAccuracy)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (increasedAccuracy)
            {
                ComputeHA(data);
            }
            else
            {
                Compute(data);
            }

            _medianLazy = new Lazy<double>(() => data.Median());
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
        public DescriptiveStatistics(IEnumerable<double?> data, bool increasedAccuracy)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }


            if (increasedAccuracy)
            {
                ComputeHA(data);
            }
            else
            {
                Compute(data);
            }

            _medianLazy = new Lazy<double>(() => data.Median());
        }

        /// <summary>
        /// Gets the size of the sample.
        /// </summary>
        /// <value>The size of the sample.</value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the sample mean.
        /// </summary>
        /// <value>The sample mean.</value>
        public double Mean { get; private set; }

        /// <summary>
        /// Gets the unbiased population variance estimator (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <value>The sample variance.</value>
        public double Variance { get; private set; }

        /// <summary>
        /// Gets the unbiased population standard deviation (on a dataset of size N will use an N-1 normalizer).
        /// </summary>
        /// <value>The sample standard deviation.</value>
        public double StandardDeviation { get; private set; }

        /// <summary>
        /// Gets the sample skewness.
        /// </summary>
        /// <value>The sample skewness.</value>
        /// <remarks>Returns zero if <see cref="Count"/> is less than three. </remarks>
        public double Skewness { get; private set; }

        /// <summary>
        /// Gets the sample median.
        /// </summary>
        /// <value>The sample median.</value>
        [Obsolete("Please use Statistics.Median instead (performance). Scheduled for removal in v3.0.")]
        public double Median
        {
            get { return _medianLazy.Value; }
        }

        readonly Lazy<double> _medianLazy;

        /// <summary>
        /// Gets the sample kurtosis.
        /// </summary>
        /// <value>The sample kurtosis.</value>
        /// <remarks>Returns zero if <see cref="Count"/> is less than four. </remarks>
        public double Kurtosis { get; private set; }

        /// <summary>
        /// Gets the maximum sample value.
        /// </summary>
        /// <value>The maximum sample value.</value>
        public double Maximum { get; private set; }

        /// <summary>
        /// Gets the minimum sample value.
        /// </summary>
        /// <value>The minimum sample value.</value>
        public double Minimum { get; private set; }

        /// <summary>
        /// Computes descriptive statistics from a stream of data values.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        private void Compute(IEnumerable<double> data)
        {
            double mean = 0;
            double variance = 0;
            double skewness = 0;
            double kurtosis = 0;
            double minimum = Double.PositiveInfinity;
            double maximum = Double.NegativeInfinity;
            int n = 0;
            foreach (var xi in data)
            {
                double delta = xi - mean;
                double scaleDelta = delta / ++n;
                double scaleDeltaSQR = scaleDelta * scaleDelta;
                double tmpDelta = delta * (n - 1);

                mean += scaleDelta;

                kurtosis += tmpDelta * scaleDelta * scaleDeltaSQR * (n * n - 3 * n + 3)
                    + 6 * scaleDeltaSQR * variance - 4 * scaleDelta * skewness;

                skewness += tmpDelta * scaleDeltaSQR * (n - 2) - 3 * scaleDelta * variance;
                variance += tmpDelta * scaleDelta;

                if (minimum > xi) { minimum = xi; }
                if (maximum < xi) { maximum = xi; }
            }
            SetStatistics(mean, variance, skewness, kurtosis, minimum, maximum, n);
        }



        /// <summary>
        /// Computes descriptive statistics from a stream of nullable data values.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        private void Compute(IEnumerable<double?> data)
        {
            double mean = 0;
            double variance = 0;
            double skewness = 0;
            double kurtosis = 0;
            double minimum = Double.PositiveInfinity;
            double maximum = Double.NegativeInfinity;
            int n = 0;
            foreach (var xi in data)
            {
                if (xi.HasValue)
                {
                    double delta = xi.Value - mean;
                    double scaleDelta = delta / ++n;
                    double scaleDeltaSQR = scaleDelta * scaleDelta;
                    double tmpDelta = delta * (n - 1);

                    mean += scaleDelta;

                    kurtosis += tmpDelta * scaleDelta * scaleDeltaSQR * (n * n - 3 * n + 3)
                        + 6 * scaleDeltaSQR * variance - 4 * scaleDelta * skewness;

                    skewness += tmpDelta * scaleDeltaSQR * (n - 2) - 3 * scaleDelta * variance;
                    variance += tmpDelta * scaleDelta;
                    if (minimum > xi) { minimum = xi.Value; }
                    if (maximum < xi) { maximum = xi.Value; }
                }
            }

            SetStatistics(mean, variance, skewness, kurtosis, minimum, maximum, n);

        }

        /// <summary>
        /// Computes descriptive statistics from a stream of data values using high accuracy.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        private void ComputeHA(IEnumerable<double> data)
        {
            decimal mean = 0;
            decimal variance = 0;
            decimal skewness = 0;
            decimal kurtosis = 0;
            decimal minimum = Decimal.MaxValue;
            decimal maximum = Decimal.MinValue;
            int n = 0;
            foreach (decimal xi in data)
            {
                decimal delta = xi - mean;
                decimal scaleDelta = delta / ++n;
                decimal scaleDeltaSQR = scaleDelta * scaleDelta;
                decimal tmpDelta = delta * (n - 1);

                mean += scaleDelta;

                kurtosis += tmpDelta * scaleDelta * scaleDeltaSQR * (n * n - 3 * n + 3)
                    + 6 * scaleDeltaSQR * variance - 4 * scaleDelta * skewness;

                skewness += tmpDelta * scaleDeltaSQR * (n - 2) - 3 * scaleDelta * variance;
                variance += tmpDelta * scaleDelta;
                if (minimum > xi) { minimum = xi; }
                if (maximum < xi) { maximum = xi; }
            }

            SetStatistics((double)mean, (double)variance, (double)skewness, (double)kurtosis, (double)minimum, (double)maximum, n);

        }

        /// <summary>
        /// Computes descriptive statistics from a stream of nullable data values using high accuracy.
        /// </summary>
        /// <param name="data">A sequence of datapoints.</param>
        private void ComputeHA(IEnumerable<double?> data)
        {
            decimal mean = 0;
            decimal variance = 0;
            decimal skewness = 0;
            decimal kurtosis = 0;
            decimal minimum = Decimal.MaxValue;
            decimal maximum = Decimal.MinValue;
            int n = 0;
            foreach (decimal? xi in data)
            {
                if (xi.HasValue)
                {
                    decimal delta = xi.Value - mean;
                    decimal scaleDelta = delta / ++n;
                    decimal scaleDeltaSQR = scaleDelta * scaleDelta;
                    decimal tmpDelta = delta * (n - 1);

                    mean += scaleDelta;

                    kurtosis += tmpDelta * scaleDelta * scaleDeltaSQR * (n * n - 3 * n + 3)
                        + 6 * scaleDeltaSQR * variance - 4 * scaleDelta * skewness;

                    skewness += tmpDelta * scaleDeltaSQR * (n - 2) - 3 * scaleDelta * variance;
                    variance += tmpDelta * scaleDelta;
                    if (minimum > xi) { minimum = xi.Value; }
                    if (maximum < xi) { maximum = xi.Value; }
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
        private void SetStatistics(double mean, double variance, double skewness, double kurtosis, double minimum, double maximum, int n)
        {
            Mean = mean;
            Count = n;
            if (n > 0)
            {
                Minimum = minimum;
                Maximum = maximum;
                if (n > 1)
                {
                    Variance = variance / (n - 1);
                    StandardDeviation = Math.Sqrt(Variance);
                }
                if (Variance != 0)
                {
                    if (n > 2)
                    {
                        Skewness = (double)n / ((n - 1) * (n - 2)) * (skewness / (Variance * StandardDeviation));
                    }

                    if (n > 3)
                    {
                        Kurtosis = ((double)n * n - 1) / ((n - 2) * (n - 3))
                            * (n * kurtosis / (variance * variance) - 3 + 6.0 / (n + 1));
                    }
                }
            }
        }
    }

}
