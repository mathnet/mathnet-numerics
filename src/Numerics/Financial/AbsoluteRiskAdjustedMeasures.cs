// <copyright file="AbsoluteRiskAdjustedMeasures.cs" company="Math.NET">
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

namespace MathNet.Numerics.Financial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MathNet.Numerics.Statistics;

    public static class AbsoluteRiskAdjustedMeasures
    {
        /// <summary>
        /// The greater a portfolio's Sharpe ratio, the better its risk-adjusted performance has been. 
        /// A negative Sharpe ratio indicates that a risk-less asset would perform better than the security being analyzed.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="periodRiskFreeReturn">Some benchmark's rate of return over the same period.</param>
        /// <returns></returns>
        /// <seealso cref="http://www.investopedia.com/terms/s/sharperatio.asp"/>
        /// <seealso cref="http://www.edge-fund.com/Lo02.pdf"/>
        public static double SharpRatio(this IEnumerable<double> data, double periodRiskFreeReturn)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            //SR(t) = (sample mean[0, T] - Rf) / sample std dev[0, T]
            return (data.Mean() - periodRiskFreeReturn) / data.StandardDeviation();
        }

        /// <summary>
        /// A comparison of the average annual compounded rate of return and the maximum drawdown risk
        /// The lower the Calmar Ratio, the worse the investment performed on a risk-adjusted basis 
        /// over the specified time period; the higher the Calmar Ratio, the better it performed. 
        /// Generally speaking, the time period used is three years, but this can be higher or lower 
        /// based on the investment in question.
        /// http://www.investopedia.com/terms/c/calmarratio.asp
        /// </summary>
        /// <param name="data">Monthly return data ordered by date ascending.</param>
        /// <returns>A double representing the Calmar Ratio</returns>
        /// <seealso cref="<seealso cref="http://alumnus.caltech.edu/~amir/mdd-risk.pdf"/>"/>
        public static double CalmarRatio(this IEnumerable<double> data)
        {
            //Calmar(t) = Return over[0,T] / MDD over [0,T]
            return data.CompoundMonthlyReturn() / data.MaximumDrawdown();
        }

        /// <summary>
        /// A ratio used mainly in the context of hedge funds. This risk-reward measure determines 
        /// which hedge funds have the highest returns while enduring the least amount of volatility. 
        /// Just like the Calmar ratio, a higher Sterling ratio is generally better because it means 
        /// that the investment(s) are receiving a higher return relative to risk.
        /// http://www.investopedia.com/terms/s/sterlingratio.asp
        /// </summary>
        /// <param name="data">Monthly return data ordered by date ascending.</param>
        /// <returns>A double representing the Sterling Ratio</returns>
        /// <seealso cref="http://alumnus.caltech.edu/~amir/mdd-risk.pdf"/>
        public static double SterlingRatio(this IEnumerable<double> data)
        {
            //Sterling(T) = Return over[0,T] / MDD over[0, T] - 10%
            return data.CompoundMonthlyReturn() / (data.MaximumDrawdown() - 0.1);
        }

        /// <summary>
        /// The Sortino ratio is similar to the Sharpe ratio, except it uses downside deviation 
        /// for the denominator instead of standard deviation, the use of which doesn't 
        /// discriminate between up and down volatility.
        /// http://www.investopedia.com/terms/s/sortinoratio.asp
        /// It also substitutes minimum acceptable return for risk free rate.
        /// </summary>
        /// <param name="data">Monthly return data ordered by date ascending.</param>
        /// <param name="minimumAcceptableReturn">The minimum acceptable return. Between 0.0 and 1.0</param>
        /// <returns>A double representing the Sortino Ratio</returns>
        public static double SortinoRatio(this IEnumerable<double> data, double minimumAcceptableReturn)
        {
            return (data.CompoundMonthlyReturn() - minimumAcceptableReturn) / data.DownsideDeviation(minimumAcceptableReturn);
        }

        //Omega
    }
}
