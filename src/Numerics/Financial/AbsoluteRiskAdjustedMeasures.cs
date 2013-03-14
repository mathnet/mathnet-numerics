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
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="periodRiskFreeReturn"></param>
        /// <returns></returns>
        public static double SharpRatio(this IEnumerable<double> data, double periodRiskFreeReturn)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            //might need to be annualized mean and std dev
            return (data.Mean() - periodRiskFreeReturn) / data.StandardDeviation();
        }

        public static double CalmarRatio(this IEnumerable<double> data)
        {
            //CR = AnnualizedROR(last 3 years) / MaximumDrawdown(last 3 years)
            throw new NotImplementedException();
        }

        public static double SterlingRatio(this IEnumerable<double> data)
        {
            //SR = AnnualizedROR(last 3 years) / ABS( AvgDrawdown - .10);
            throw new NotImplementedException();
        }

        public static double SortinoRatio(this IEnumerable<double> data, double minimumAcceptableReturn)
        {
            return (data.CompoundMonthlyReturn() - minimumAcceptableReturn) / data.DownsideDeviation(minimumAcceptableReturn);
        }

        //Omega
    }
}
