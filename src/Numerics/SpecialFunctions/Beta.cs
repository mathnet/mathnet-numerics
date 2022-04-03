// <copyright file="Beta.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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
//    ALGLIB 2.0.1, Sergey Bochkanov
// </contribution>

using System;

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{

    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Computes the logarithm of the Euler Beta function.
        /// </summary>
        /// <param name="z">The first Beta parameter, a positive real number.</param>
        /// <param name="w">The second Beta parameter, a positive real number.</param>
        /// <returns>The logarithm of the Euler Beta function evaluated at z,w.</returns>
        /// <exception cref="ArgumentException">If <paramref name="z"/> or <paramref name="w"/> are not positive.</exception>
        public static double BetaLn(double z, double w)
        {
            if (z <= 0.0)
            {
                throw new ArgumentException("Value must be positive.", nameof(z));
            }

            if (w <= 0.0)
            {
                throw new ArgumentException("Value must be positive.", nameof(w));
            }

            return GammaLn(z) + GammaLn(w) - GammaLn(z + w);
        }

        /// <summary>
        /// Computes the Euler Beta function.
        /// </summary>
        /// <param name="z">The first Beta parameter, a positive real number.</param>
        /// <param name="w">The second Beta parameter, a positive real number.</param>
        /// <returns>The Euler Beta function evaluated at z,w.</returns>
        /// <exception cref="ArgumentException">If <paramref name="z"/> or <paramref name="w"/> are not positive.</exception>
        public static double Beta(double z, double w)
        {
            return Math.Exp(BetaLn(z, w));
        }

        /// <summary>
        /// Returns the lower incomplete (unregularized) beta function
        /// B(a,b,x) = int(t^(a-1)*(1-t)^(b-1),t=0..x) for real a &gt; 0, b &gt; 0, 1 &gt;= x &gt;= 0.
        /// </summary>
        /// <param name="a">The first Beta parameter, a positive real number.</param>
        /// <param name="b">The second Beta parameter, a positive real number.</param>
        /// <param name="x">The upper limit of the integral.</param>
        /// <returns>The lower incomplete (unregularized) beta function.</returns>
        public static double BetaIncomplete(double a, double b, double x)
        {
            return BetaRegularized(a, b, x)*Beta(a, b);
        }

        /// <summary>
        /// Returns the regularized lower incomplete beta function
        /// I_x(a,b) = 1/Beta(a,b) * int(t^(a-1)*(1-t)^(b-1),t=0..x) for real a &gt; 0, b &gt; 0, 1 &gt;= x &gt;= 0.
        /// </summary>
        /// <param name="a">The first Beta parameter, a positive real number.</param>
        /// <param name="b">The second Beta parameter, a positive real number.</param>
        /// <param name="x">The upper limit of the integral.</param>
        /// <returns>The regularized lower incomplete beta function.</returns>
        public static double BetaRegularized(double a, double b, double x)
        {
            if (a < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(a), "Value must not be negative (zero is ok).");
            }

            if (b < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(b), "Value must not be negative (zero is ok).");
            }

            if (x < 0.0 || x > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"Value is expected to be between 0.0 and 1.0 (including 0.0 and 1.0).");
            }

            var bt = (x == 0.0 || x == 1.0)
                ? 0.0
                : Math.Exp(GammaLn(a + b) - GammaLn(a) - GammaLn(b) + (a*Math.Log(x)) + (b*Math.Log(1.0 - x)));

            var symmetryTransformation = x >= (a + 1.0)/(a + b + 2.0);

            /* Continued fraction representation */
            var eps = Precision.DoublePrecision;
            var fpmin = 0.0.Increment()/eps;

            if (symmetryTransformation)
            {
                x = 1.0 - x;
                (a, b) = (b, a);
            }

            var qab = a + b;
            var qap = a + 1.0;
            var qam = a - 1.0;
            var c = 1.0;
            var d = 1.0 - (qab*x/qap);

            if (Math.Abs(d) < fpmin)
            {
                d = fpmin;
            }

            d = 1.0/d;
            var h = d;

            for (int m = 1, m2 = 2; m <= 50000; m++, m2 += 2)
            {
                var aa = m*(b - m)*x/((qam + m2)*(a + m2));
                d = 1.0 + (aa*d);

                if (Math.Abs(d) < fpmin)
                {
                    d = fpmin;
                }

                c = 1.0 + (aa/c);
                if (Math.Abs(c) < fpmin)
                {
                    c = fpmin;
                }

                d = 1.0/d;
                h *= d*c;
                aa = -(a + m)*(qab + m)*x/((a + m2)*(qap + m2));
                d = 1.0 + (aa*d);

                if (Math.Abs(d) < fpmin)
                {
                    d = fpmin;
                }

                c = 1.0 + (aa/c);

                if (Math.Abs(c) < fpmin)
                {
                    c = fpmin;
                }

                d = 1.0/d;
                var del = d*c;
                h *= del;

                if (Math.Abs(del - 1.0) <= eps)
                {
                    return symmetryTransformation ? 1.0 - (bt*h/a) : bt*h/a;
                }
            }

            return symmetryTransformation ? 1.0 - (bt*h/a) : bt*h/a;
        }
    }
}
