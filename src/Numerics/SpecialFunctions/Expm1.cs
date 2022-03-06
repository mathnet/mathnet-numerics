// <copyright file="Expm1.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2022 Math.NET
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

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    public partial class SpecialFunctions
    {
        /// <summary>
        /// Numerically stable exponential minus one, i.e. <code>x -> exp(x)-1</code>
        /// </summary>
        /// <param name="power">A number specifying a power.</param>
        /// <returns>Returns <code>exp(power)-1</code>.</returns>
        public static double Expm1(double power)
        {
            double x = Math.Abs(power);
            if (x > 0.1)
            {
                return Math.Exp(power) - 1.0;
            }

            if (x < x.PositiveEpsilonOf())
            {
                return x;
            }

            // Series Expansion to x^k / k!
            int k = 0;
            double term = 1.0;
            return Series.Evaluate(
                () =>
                {
                    k++;
                    term *= power;
                    term /= k;
                    return term;
                });
        }

        /// <summary>
        /// Numerically stable exponential minus one, i.e. <code>x -> exp(x)-1</code>
        /// </summary>
        /// <param name="power">A number specifying a power.</param>
        /// <returns>Returns <code>exp(power)-1</code>.</returns>
        [Obsolete("Use Expm1 instead")]
        public static double ExponentialMinusOne(double power)
        {
            return Expm1(power);
        }
    }
}
