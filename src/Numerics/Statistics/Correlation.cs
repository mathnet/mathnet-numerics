// <copyright file="Correlation.cs" company="Math.NET">
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

namespace MathNet.Numerics.Statistics
{
    using System;
    using System.Collections.Generic;
    using Properties;

    /// <summary>
    /// A class with correlation measures between two datasets.
    /// </summary>
    public static class Correlation
    {
        /// <summary>
        /// Computes the Pearson product-moment correlation coefficient.
        /// </summary>
        /// <param name="dataA">Sample data A.</param>
        /// <param name="dataB">Sample data B.</param>
        /// <returns>The Pearson product-moment correlation coefficient.</returns>
        public static double Pearson(IEnumerable<double> dataA, IEnumerable<double> dataB)
        {
            int n = 0;
            double r = 0.0;
            double meanA = dataA.Mean();
            double meanB = dataB.Mean();
            double sdevA = dataA.StandardDeviation();
            double sdevB = dataB.StandardDeviation();

            IEnumerator<double> ieA = dataA.GetEnumerator();
            IEnumerator<double> ieB = dataB.GetEnumerator();

            while (ieA.MoveNext())
            {
                if (ieB.MoveNext() == false)
                {
                    throw new ArgumentOutOfRangeException("Datasets dataA and dataB need to have the same length.");
                }

                n++;
                r += (ieA.Current - meanA) * (ieB.Current - meanB) / (sdevA * sdevB);
            }
            if (ieB.MoveNext() == true)
            {
                throw new ArgumentOutOfRangeException("Datasets dataA and dataB need to have the same length.");
            }

            return r / (n - 1);
        }
    }
}
