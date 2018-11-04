// <copyright file="AbsoluteRiskStatistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.Financial
{
    public static class AbsoluteRiskMeasures
    {
        // Note: The following statistics would be considered an absolute risk statistic in the finance realm as well.
        // Standard Deviation
        // Annualized Standard Deviation = Math.Sqrt(Monthly Standard Deviation x ( 12 ))
        // Skewness
        // Kurtosis

        /// <summary>
        /// Calculation is similar to Standard Deviation , except it calculates an average (mean) return only for periods with a gain
        /// and measures the variation of only the gain periods around the gain mean. Measures the volatility of upside performance.
        /// © Copyright 1996, 1999 Gary L.Gastineau. First Edition. © 1992 Swiss Bank Corporation.
        /// </summary>
        public static double GainStandardDeviation(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.Where(x => x >= 0).StandardDeviation();
        }

        /// <summary>
        /// Similar to standard deviation, except this statistic calculates an average (mean) return for only the periods with a loss and then
        /// measures the variation of only the losing periods around this loss mean. This statistic measures the volatility of downside performance.
        /// </summary>
        /// <remarks>http://www.offshore-library.com/kb/statistics.php</remarks>
        public static double LossStandardDeviation(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.Where(x => x < 0).StandardDeviation();
        }

        /// <summary>
        /// This measure is similar to the loss standard deviation except the downside deviation
        /// considers only returns that fall below a defined minimum acceptable return (MAR) rather than the arithmetic mean.
        /// For example, if the MAR is 7%, the downside deviation would measure the variation of each period that falls below
        /// 7%. (The loss standard deviation, on the other hand, would take only losing periods, calculate an average return for
        /// the losing periods, and then measure the variation between each losing return and the losing return average).
        /// </summary>
        public static double DownsideDeviation(this IEnumerable<double> data, double minimalAcceptableReturn)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.Where(x => x < minimalAcceptableReturn).StandardDeviation();
        }

        /// <summary>
        /// A measure of volatility in returns below the mean. It's similar to standard deviation, but it only
        /// looks at periods where the investment return was less than average return.
        /// </summary>
        public static double SemiDeviation(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var mean = data.Mean();
            var belowMeanData = data.Where(x => x < mean);
            return belowMeanData.StandardDeviation();
        }

        /// <summary>
        /// Measures a fund’s average gain in a gain period divided by the fund’s average loss in a losing
        /// period. Periods can be monthly or quarterly depending on the data frequency.
        /// </summary>
        public static double GainLossRatio(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var gains = data.Where(x => x >= 0);
            var losses = data.Where(x => x < 0);
            return Math.Abs(gains.Mean() / losses.Mean());
        }
    }
}
