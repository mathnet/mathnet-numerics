// <copyright file="AbsoluteReturnMeasures.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.Financial
{
    public static class AbsoluteReturnMeasures
    {
        /// <summary>
        /// Compound Monthly Return or Geometric Return or Annualized Return
        /// </summary>
        public static double CompoundReturn(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            int count = 0;
            double compoundReturn = 1.0;
            foreach (var item in data)
            {
                count++;
                compoundReturn *= 1 + item;
            }

            return count == 0 ? double.NaN : Math.Pow(compoundReturn, 1.0/count) - 1.0;
        }

        /// <summary>
        /// Average Gain or Gain Mean
        /// This is a simple average (arithmetic mean) of the periods with a gain. It is calculated by summing the returns for gain periods (return 0)
        /// and then dividing the total by the number of gain periods.
        /// </summary>
        /// <remarks>http://www.offshore-library.com/kb/statistics.php</remarks>
        public static double GainMean(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.Where(x => x >= 0).Mean();
        }

        /// <summary>
        /// Average Loss or LossMean
        /// This is a simple average (arithmetic mean) of the periods with a loss. It is calculated by summing the returns for loss periods (return &lt; 0)
        /// and then dividing the total by the number of loss periods.
        /// </summary>
        /// <remarks>http://www.offshore-library.com/kb/statistics.php</remarks>
        public static double LossMean(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.Where(x => x < 0).Mean();
        }
    }
}
