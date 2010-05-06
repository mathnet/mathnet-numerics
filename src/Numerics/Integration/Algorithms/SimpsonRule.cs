// <copyright file="SimpsonRule.cs" company="Math.NET">
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

namespace MathNet.Numerics.Integration.Algorithms
{
    using System;
    using NumberTheory;
    using Properties;

    /// <summary>
    /// Approximation algorithm for definite integrals by Simpson's rule.
    /// </summary>
    public static class SimpsonRule
    {
        /// <summary>
        /// Direct 3-point approximation of the definite integral in the provided interval by Simpson's rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double IntegrateThreePoint(
            Func<double, double> f,
            double intervalBegin,
            double intervalEnd)
        {
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }

            double midpoint = (intervalEnd + intervalBegin) / 2;
            return (intervalEnd - intervalBegin) / 6 * (f(intervalBegin) + f(intervalEnd) + (4 * f(midpoint)));
        }

        /// <summary>
        /// Composite N-point approximation of the definite integral in the provided interval by Simpson's rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="numberOfPartitions">Even number of composite subdivision partitions.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double IntegrateComposite(
            Func<double, double> f,
            double intervalBegin,
            double intervalEnd,
            int numberOfPartitions)
        {
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }

            if (numberOfPartitions <= 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitions", Resources.ArgumentPositive);
            }

            if (numberOfPartitions.IsOdd())
            {
                throw new ArgumentException(Resources.ArgumentEven, "numberOfPartitions");
            }

            double step = (intervalEnd - intervalBegin) / numberOfPartitions;
            double factor = step / 3;

            double offset = step;
            int m = 4;
            double sum = f(intervalBegin) + f(intervalEnd);
            for (int i = 0; i < numberOfPartitions - 1; i++)
            {
                // NOTE (cdrnet, 2009-01-07): Do not combine intervalBegin and offset (numerical stability)
                sum += m * f(intervalBegin + offset);
                m = 6 - m;
                offset += step;
            }

            return factor * sum;
        }
    }
}
