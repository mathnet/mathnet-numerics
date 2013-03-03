// <copyright file="MCMCDiagonistics.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Statistics.Mcmc.Diagnostics
{
    /// <summary>
    /// Provides utilities to analysis the convergence of a set of samples from
    /// a <seealso cref="McmcSampler{T}"/>.
    /// </summary>
    static public class MCMCDiagnostics
    {
        /// <summary>
        /// Computes the auto correlations of a series evaluated by a function f.
        /// </summary>
        /// <param name="series">The series for computing the auto correlation.</param>
        /// <param name="lag">The lag in the series</param>
        /// <param name="f">The function used to evaluate the series.</param>
        /// <returns>The auto correlation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if lag is zero or if lag is
        /// greater than or equal to the length of Series.</exception>
        static public double ACF<T>(IEnumerable<T> series, int lag, Func<T,double> f)
        {
            if (lag < 0)
            {
                throw new ArgumentOutOfRangeException("Lag must be positive");
            }

            int length = series.Count();
            if (lag >= length)
            {
                throw new ArgumentOutOfRangeException("Lag must be smaller than the sample size");
            }

            var transformedSeries = series.Select(f);

            var enumerable = transformedSeries as double[] ?? transformedSeries.ToArray();
            var firstSeries = enumerable.Take(length-lag);
            var secondSeries = enumerable.Skip(lag);

            return Correlation.Pearson(firstSeries, secondSeries);
        }

        /// <summary>
        /// Computes the effective size of the sample when evaluated by a function f.
        /// </summary>
        /// <param name="series">The samples.</param>
        /// <param name="f">The function use for evaluating the series.</param>
        /// <returns>The effective size when auto correlation is taken into account.</returns>
        static public double EffectiveSize<T>(IEnumerable<T> series, Func<T,double> f)
        {
            int length = series.Count();
            double rho = ACF(series, 1, f);
            return ((1 - rho) / (1 + rho)) * length;
        }
    }
}
