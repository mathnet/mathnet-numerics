// <copyright file="SpecialFunctions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2011 Math.NET
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
//    Cephes Math Library, Stephen L. Moshier
//    ALGLIB, Sergey Bochkanov
// </contribution>

namespace MathNet.Numerics
{
    using System;
    using Properties;

    /// <summary>
    /// This class implements a collection of special function evaluations for double precision. This class 
    /// has a static constructor which will precompute a small number of values for faster runtime computations.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Initializes static members of the SpecialFunctions class.
        /// </summary>
        static SpecialFunctions()
        {
            InitializeFactorial();
        }

        /// <summary>
        /// Computes the <paramref name="t"/>'th Harmonic number.
        /// </summary>
        /// <param name="t">The Harmonic number which needs to be computed.</param>
        /// <returns>The t'th Harmonic number.</returns>
        public static double Harmonic(int t)
        {
            return Constants.EulerMascheroni + DiGamma(t + 1.0);
        }

        /// <summary>
        /// Compute the generalized harmonic number of order n of m. (1 + 1/2^m + 1/3^m + ... + 1/n^m)
        /// </summary>
        /// <param name="n">The order parameter.</param>
        /// <param name="m">The power parameter.</param>
        /// <returns>General Harmonic number.</returns>
        public static double GeneralHarmonic(int n, double m)
        {
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += Math.Pow(i + 1, -m);
            }
            return sum;
        }

        /// <summary>
        /// Computes the Digamma function which is mathematically defined as the derivative of the logarithm of the gamma function.
        /// This implementation is based on
        ///     Jose Bernardo
        ///     Algorithm AS 103:
        ///     Psi ( Digamma ) Function,
        ///     Applied Statistics,
        ///     Volume 25, Number 3, 1976, pages 315-317.
        /// Using the modifications as in Tom Minka's lightspeed toolbox.
        /// </summary>
        /// <param name="x">The argument of the digamma function.</param>
        /// <returns>The value of the DiGamma function at <paramref name="x"/>.</returns>
        public static double DiGamma(double x)
        {
            const double C = 12.0;
            const double D1 = -0.57721566490153286;
            const double D2 = 1.6449340668482264365;
            const double S = 1e-6;
            const double S3 = 1.0 / 12.0;
            const double S4 = 1.0 / 120.0;
            const double S5 = 1.0 / 252.0;
            const double S6 = 1.0 / 240.0;
            const double S7 = 1.0 / 132.0;

            if (Double.IsNegativeInfinity(x) || Double.IsNaN(x))
            {
                return Double.NaN;
            }

            // Handle special cases.
            if (x <= 0 && Math.Floor(x) == x)
            {
                return Double.NegativeInfinity;
            }

            // Use inversion formula for negative numbers.
            if (x < 0)
            {
                return DiGamma(1.0 - x) + (Math.PI / Math.Tan(-Math.PI * x));
            }

            if (x <= S)
            {
                return D1 - (1 / x) + (D2 * x);
            }

            double result = 0;
            while (x < C)
            {
                result -= 1 / x;
                x++;
            }

            if (x >= C)
            {
                var r = 1 / x;
                result += Math.Log(x) - (0.5 * r);
                r *= r;

                result -= r * (S3 - (r * (S4 - (r * (S5 - (r * (S6 - (r * S7))))))));
            }

            return result;
        }

        /// <summary>
        /// <para>Computes the inverse Digamma function: this is the inverse of the logarithm of the gamma function. This function will
        /// only return solutions that are positive.</para>
        /// <para>This implementation is based on the bisection method.</para>
        /// </summary>
        /// <param name="p">The argument of the inverse digamma function.</param>
        /// <returns>The positive solution to the inverse DiGamma function at <paramref name="p"/>.</returns>
        public static double DiGammaInv(double p)
        {
            if (Double.IsNaN(p))
            {
                return Double.NaN;
            }

            if (Double.IsNegativeInfinity(p))
            {
                return 0.0;
            }

            if (Double.IsPositiveInfinity(p))
            {
                return Double.PositiveInfinity;
            }

            var x = Math.Exp(p);
            for (var d = 1.0; d > 1.0e-15; d /= 2.0)
            {
                x += d * Math.Sign(p - DiGamma(x));
            }

            return x;
        }

        /// <summary>
        /// Computes the logit function. see: http://en.wikipedia.org/wiki/Logit
        /// </summary>
        /// <param name="p">The parameter for which to compute the logit function. This number should be
        /// between 0 and 1.</param>
        /// <returns>The logarithm of <paramref name="p"/> divided by 1.0 - <paramref name="p"/>.</returns>
        public static double Logit(double p)
        {
            if (p < 0.0 || p > 1.0)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentBetween0And1);
            }

            return Math.Log(p / (1.0 - p));
        }

        /// <summary>
        /// Computes the logistic function. see: http://en.wikipedia.org/wiki/Logistic
        /// </summary>
        /// <param name="p">The parameter for which to compute the logistic function.</param>
        /// <returns>The logistic function of <paramref name="p"/>.</returns>
        public static double Logistic(double p)
        {
            return 1.0 / (Math.Exp(-p) + 1.0);
        }
    }
}
