// <copyright file="Series.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2021 Math.NET
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

namespace MathNet.Numerics
{
    public static class Series
    {
        /// <summary>
        /// Numerically stable series summation (stops automatically).
        /// </summary>
        /// <param name="nextSummand">provides the summands sequentially</param>
        /// <returns>Sum</returns>
        public static double Evaluate(Func<double> nextSummand)
        {
            double compensation = 0.0;
            double current;
            const double factor = 1 << 16;

            double sum = nextSummand();

            do
            {
                // Kahan Summation
                // NOTE (ruegg): do NOT optimize. Now, how to tell that the compiler?
                current = nextSummand();
                double y = current - compensation;
                double t = sum + y;
                compensation = t - sum;
                compensation -= y;
                sum = t;
            }
            while (Math.Abs(sum) < Math.Abs(factor*current));

            return sum;
        }

        /// <summary>
        /// Numerically stable series summation (stops automatically).
        /// </summary>
        /// <param name="infiniteSummands">provides the summands sequentially</param>
        /// <returns>Sum</returns>
        public static double Evaluate(IEnumerable<double> infiniteSummands)
        {
            double compensation = 0.0;
            double current, sum;
            const double factor = 1 << 16;

            using (var enumerator = infiniteSummands.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return 0.0;
                }

                sum = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    return sum;
                }

                do
                {
                    // Kahan Summation
                    // NOTE (ruegg): do NOT optimize. Now, how to tell that the compiler?
                    current = enumerator.Current;
                    double y = current - compensation;
                    double t = sum + y;
                    compensation = t - sum;
                    compensation -= y;
                    sum = t;
                } while (Math.Abs(sum) < Math.Abs(factor * current) && enumerator.MoveNext());
            }

            return sum;
        }
    }
}
