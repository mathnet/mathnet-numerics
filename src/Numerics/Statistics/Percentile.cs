// <copyright file="Percentile.cs" company="Math.NET">
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
    using System.Linq;

    /// <summary>
    /// Methods to calculate the percentiles.
    /// </summary>
    public enum PercentileMethod
    {
        /// <summary>
        /// Using the method recommened my NIST,
        /// http://www.itl.nist.gov/div898/handbook/prc/section2/prc252.htm
        /// </summary>
        Nist = 0, 

        /// <summary>
        /// Using the nearest rank, http://en.wikipedia.org/wiki/Percentile#Nearest_Rank
        /// </summary>
        Nearest, 

        /// <summary>
        /// Using the same method as Excel does, 
        /// http://www.itl.nist.gov/div898/handbook/prc/section2/prc252.htm
        /// </summary>
        Excel, 

        /// <summary>
        /// Use linear interpolation between the two nearest ranks,
        /// http://en.wikipedia.org/wiki/Percentile#Linear_Interpolation_Between_Closest_Ranks
        /// </summary>
        Interpolation
    }

    /// <summary>
    /// Class to calculate percentiles.
    /// </summary>
    [Obsolete("Use Statistics.Quantile or .QuantileFunc or one of the custom variants instead. Scheduled for removal in v3.0.")]
    public class Percentile
    {
        /// <summary>
        /// Holds the data.
        /// </summary>
        private readonly double[] _data;

        /// <summary>
        /// Gets or sets the method used to calculate the percentiles.
        /// </summary>
        /// <value>The calculation method.</value>
        /// <remarks>defaults to <see cref="PercentileMethod.Nist"/>.</remarks>
        public PercentileMethod Method
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Percentile"/> class.
        /// </summary>
        /// <param name="data">The data to calculate the percentiles of.</param>
        public Percentile(IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            _data = data.ToArray();
            Array.Sort(_data);
        }

        /// <summary>
        /// Computes the percentile.
        /// </summary>
        /// <param name="percentile">The percentile, must be between 0.0 and 1.0 (inclusive).</param>
        /// <returns>the requested percentile.</returns>
        public double Compute(double percentile)
        {
            switch (Method)
            {
                case PercentileMethod.Nist:
                    return SortedArrayStatistics.QuantileCustom(_data, percentile, QuantileDefinition.Nist);
                case PercentileMethod.Nearest:
                    return SortedArrayStatistics.QuantileCustom(_data, percentile, QuantileDefinition.R3);
                case PercentileMethod.Interpolation:
                    return SortedArrayStatistics.QuantileCustom(_data, percentile, QuantileDefinition.R5);
                case PercentileMethod.Excel:
                    return SortedArrayStatistics.QuantileCustom(_data, percentile, QuantileDefinition.Excel);
                default:
                    return SortedArrayStatistics.Quantile(_data, percentile);
            }
        }

        /// <summary>
        /// Computes the percentiles for the given list.
        /// </summary>
        /// <param name="percentiles">The percentiles, must be between 0.0 and 1.0 (inclusive)</param>
        /// <returns>the values that correspond to the given percentiles.</returns>
        public IList<double> Compute(IEnumerable<double> percentiles)
        {
            if (percentiles == null)
            {
                throw new ArgumentNullException("percentiles");
            }

            return percentiles.Select(Compute).ToList();
        }
    }
}
