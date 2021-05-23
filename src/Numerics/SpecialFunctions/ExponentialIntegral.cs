// <copyright file="ExponentialIntegral.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2014 Math.NET
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

// <contribution>
//    Ashley Messer
// </contribution>

using System;

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Computes the generalized Exponential Integral function (En).
        /// </summary>
        /// <param name="x">The argument of the Exponential Integral function.</param>
        /// <param name="n">Integer power of the denominator term. Generalization index.</param>
        /// <returns>The value of the Exponential Integral function.</returns>
        /// <remarks>
        /// <para>This implementation of the computation of the Exponential Integral function follows the derivation in
        ///     "Handbook of Mathematical Functions, Applied Mathematics Series, Volume 55", Abramowitz, M., and Stegun, I.A. 1964,  reprinted 1968 by
        ///     Dover Publications, New York), Chapters 6, 7, and 26.
        ///     AND
        ///     "Advanced mathematical methods for scientists and engineers", Bender, Carl M.; Steven A. Orszag (1978). page 253
        /// </para>
        /// <para>
        ///     for x &gt; 1  uses continued fraction approach that is often used to compute incomplete gamma.
        ///     for 0 &lt; x &lt;= 1 uses Taylor series expansion
        /// </para>
        /// <para>Our unit tests suggest that the accuracy of the Exponential Integral function is correct up to 13 floating point digits.</para>
        /// </remarks>
        public static double ExponentialIntegral(double x, int n)
        {
            //parameter validation
            if (n < 0 || x < 0.0)
            {
                throw new ArgumentOutOfRangeException(FormattableString.Invariant($"x and n must be positive: x={x}, n={n}"));
            }

            const double epsilon = 0.00000000000000001;
            int maxIterations = 100;
            int i, ii;
            double ndbl = n;
            double result;
            double nearDoubleMin = 1e-100; //needs a very small value that is not quite as small as the lowest value double can take
            double factorial = 1.0d;
            double del;
            double psi;
            double a, b, c, d, h; //variables for continued fraction

            //special cases
            if (n == 0)
            {
                return Math.Exp(-1.0d*x)/x;
            }
            else if (x == 0.0d)
            {
                return 1.0d/(ndbl - 1.0d);
            }
            //general cases
            //continued fraction for large x
            if (x > 1.0d)
            {
                b = x + n;
                c = 1.0d/nearDoubleMin;
                d = 1.0d/b;
                h = d;
                for (i = 1; i <= maxIterations; i++)
                {
                    a = -1.0d*i*((ndbl - 1.0d) + i);
                    b += 2.0d;
                    d = 1.0d/(a*d + b);
                    c = b + a/c;
                    del = c*d;
                    h = h*del;
                    if (Math.Abs(del - 1.0d) < epsilon)
                    {
                        return h*Math.Exp(-x);
                    }
                }
                throw new ArithmeticException(FormattableString.Invariant($"Continued fraction failed to converge for x={x}, n={n})"));
            }
            //series computation for small x
            else
            {
                result = ((ndbl - 1.0d) != 0 ? 1.0/(ndbl - 1.0d) : (-1.0d*Math.Log(x) - Constants.EulerMascheroni)); //Set first term.
                for (i = 1; i <= maxIterations; i++)
                {
                    factorial *= (-1.0d*x/i);
                    if (i != (ndbl - 1.0d))
                    {
                        del = -factorial/(i - (ndbl - 1.0d));
                    }
                    else
                    {
                        psi = -1.0d*Constants.EulerMascheroni;
                        for (ii = 1; ii <= (ndbl - 1.0d); ii++)
                        {
                            psi += (1.0d/ii);
                        }
                        del = factorial*(-1.0d*Math.Log(x) + psi);
                    }
                    result += del;
                    if (Math.Abs(del) < Math.Abs(result)*epsilon)
                    {
                        return result;
                    }
                }
                throw new ArithmeticException(FormattableString.Invariant($"Series failed to converge for x={x}, n={n})"));
            }
        }
    }
}
