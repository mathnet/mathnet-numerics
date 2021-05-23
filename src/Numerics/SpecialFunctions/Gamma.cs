// <copyright file="Gamma.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
        /// The order of the <see cref="GammaLn"/> approximation.
        /// </summary>
        const int GammaN = 10;

        /// <summary>
        /// Auxiliary variable when evaluating the <see cref="GammaLn"/> function.
        /// </summary>
        const double GammaR = 10.900511;

        /// <summary>
        /// Polynomial coefficients for the <see cref="GammaLn"/> approximation.
        /// </summary>
        static readonly double[] GammaDk =
        {
            2.48574089138753565546e-5,
            1.05142378581721974210,
            -3.45687097222016235469,
            4.51227709466894823700,
            -2.98285225323576655721,
            1.05639711577126713077,
            -1.95428773191645869583e-1,
            1.70970543404441224307e-2,
            -5.71926117404305781283e-4,
            4.63399473359905636708e-6,
            -2.71994908488607703910e-9
        };

        /// <summary>
        /// Computes the logarithm of the Gamma function.
        /// </summary>
        /// <param name="z">The argument of the gamma function.</param>
        /// <returns>The logarithm of the gamma function.</returns>
        /// <remarks>
        /// <para>This implementation of the computation of the gamma and logarithm of the gamma function follows the derivation in
        ///     "An Analysis Of The Lanczos Gamma Approximation", Glendon Ralph Pugh, 2004.
        /// We use the implementation listed on p. 116 which achieves an accuracy of 16 floating point digits. Although 16 digit accuracy
        /// should be sufficient for double values, improving accuracy is possible (see p. 126 in Pugh).</para>
        /// <para>Our unit tests suggest that the accuracy of the Gamma function is correct up to 14 floating point digits.</para>
        /// </remarks>
        public static double GammaLn(double z)
        {
            if (z < 0.5)
            {
                double s = GammaDk[0];
                for (int i = 1; i <= GammaN; i++)
                {
                    s += GammaDk[i]/(i - z);
                }

                return Constants.LnPi
                       - Math.Log(Math.Sin(Math.PI*z))
                       - Math.Log(s)
                       - Constants.LogTwoSqrtEOverPi
                       - ((0.5 - z)*Math.Log((0.5 - z + GammaR)/Math.E));
            }
            else
            {
                double s = GammaDk[0];
                for (int i = 1; i <= GammaN; i++)
                {
                    s += GammaDk[i]/(z + i - 1.0);
                }

                return Math.Log(s)
                       + Constants.LogTwoSqrtEOverPi
                       + ((z - 0.5)*Math.Log((z - 0.5 + GammaR)/Math.E));
            }
        }

        /// <summary>
        /// Computes the Gamma function.
        /// </summary>
        /// <param name="z">The argument of the gamma function.</param>
        /// <returns>The logarithm of the gamma function.</returns>
        /// <remarks>
        /// <para>
        /// This implementation of the computation of the gamma and logarithm of the gamma function follows the derivation in
        ///     "An Analysis Of The Lanczos Gamma Approximation", Glendon Ralph Pugh, 2004.
        /// We use the implementation listed on p. 116 which should achieve an accuracy of 16 floating point digits. Although 16 digit accuracy
        /// should be sufficient for double values, improving accuracy is possible (see p. 126 in Pugh).
        /// </para>
        /// <para>Our unit tests suggest that the accuracy of the Gamma function is correct up to 13 floating point digits.</para>
        /// </remarks>
        public static double Gamma(double z)
        {
            if (z < 0.5)
            {
                double s = GammaDk[0];
                for (int i = 1; i <= GammaN; i++)
                {
                    s += GammaDk[i]/(i - z);
                }

                return Math.PI/(Math.Sin(Math.PI*z)
                                *s
                                *Constants.TwoSqrtEOverPi
                                *Math.Pow((0.5 - z + GammaR)/Math.E, 0.5 - z));
            }
            else
            {
                double s = GammaDk[0];
                for (int i = 1; i <= GammaN; i++)
                {
                    s += GammaDk[i]/(z + i - 1.0);
                }

                return s*Constants.TwoSqrtEOverPi*Math.Pow((z - 0.5 + GammaR)/Math.E, z - 0.5);
            }
        }

        /// <summary>
        /// Returns the upper incomplete regularized gamma function
        /// Q(a,x) = 1/Gamma(a) * int(exp(-t)t^(a-1),t=0..x) for real a &gt; 0, x &gt; 0.
        /// </summary>
        /// <param name="a">The argument for the gamma function.</param>
        /// <param name="x">The lower integral limit.</param>
        /// <returns>The upper incomplete regularized gamma function.</returns>
        public static double GammaUpperRegularized(double a, double x)
        {
            const double epsilon = 0.000000000000001;
            const double big = 4503599627370496.0;
            const double bigInv = 2.22044604925031308085e-16;


            if (x < 1d || x <= a)
            {
                return 1d - GammaLowerRegularized(a, x);
            }

            double ax = a*Math.Log(x) - x - GammaLn(a);
            if (ax < -709.78271289338399)
            {
                return a < x ? 0d : 1d;
            }

            ax = Math.Exp(ax);
            double t;
            double y = 1 - a;
            double z = x + y + 1;
            double c = 0;
            double pkm2 = 1;
            double qkm2 = x;
            double pkm1 = x + 1;
            double qkm1 = z*x;
            double ans = pkm1/qkm1;
            do
            {
                c = c + 1;
                y = y + 1;
                z = z + 2;
                double yc = y*c;
                double pk = pkm1*z - pkm2*yc;
                double qk = qkm1*z - qkm2*yc;
                if (qk != 0)
                {
                    double r = pk/qk;
                    t = Math.Abs((ans - r)/r);
                    ans = r;
                }
                else
                {
                    t = 1;
                }

                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                if (Math.Abs(pk) > big)
                {
                    pkm2 = pkm2*bigInv;
                    pkm1 = pkm1*bigInv;
                    qkm2 = qkm2*bigInv;
                    qkm1 = qkm1*bigInv;
                }
            }
            while (t > epsilon);

            return ans*ax;
        }

        /// <summary>
        /// Returns the upper incomplete gamma function
        /// Gamma(a,x) = int(exp(-t)t^(a-1),t=0..x) for real a &gt; 0, x &gt; 0.
        /// </summary>
        /// <param name="a">The argument for the gamma function.</param>
        /// <param name="x">The lower integral limit.</param>
        /// <returns>The upper incomplete gamma function.</returns>
        public static double GammaUpperIncomplete(double a, double x)
        {
            return GammaUpperRegularized(a, x)*Gamma(a);
        }

        /// <summary>
        /// Returns the lower incomplete gamma function
        /// gamma(a,x) = int(exp(-t)t^(a-1),t=0..x) for real a &gt; 0, x &gt; 0.
        /// </summary>
        /// <param name="a">The argument for the gamma function.</param>
        /// <param name="x">The upper integral limit.</param>
        /// <returns>The lower incomplete gamma function.</returns>
        public static double GammaLowerIncomplete(double a, double x)
        {
            return GammaLowerRegularized(a, x)*Gamma(a);
        }

        /// <summary>
        /// Returns the lower incomplete regularized gamma function
        /// P(a,x) = 1/Gamma(a) * int(exp(-t)t^(a-1),t=0..x) for real a &gt; 0, x &gt; 0.
        /// </summary>
        /// <param name="a">The argument for the gamma function.</param>
        /// <param name="x">The upper integral limit.</param>
        /// <returns>The lower incomplete gamma function.</returns>
        public static double GammaLowerRegularized(double a, double x)
        {
            const double epsilon = 0.000000000000001;
            const double big = 4503599627370496.0;
            const double bigInv = 2.22044604925031308085e-16;

            if (a < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(a), "Value must not be negative (zero is ok).");
            }

            if (x < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Value must not be negative (zero is ok).");
            }

            if (a.AlmostEqual(0.0))
            {
                if (x.AlmostEqual(0.0))
                {
                    //use right hand limit value because so that regularized upper/lower gamma definition holds.
                    return 1d;
                }

                return 1d;
            }

            if (x.AlmostEqual(0.0))
            {
                return 0d;
            }

            double ax = (a*Math.Log(x)) - x - GammaLn(a);
            if (ax < -709.78271289338399)
            {
                return a < x ? 1d : 0d;
            }

            if (x <= 1 || x <= a)
            {
                double r2 = a;
                double c2 = 1;
                double ans2 = 1;

                do
                {
                    r2 = r2 + 1;
                    c2 = c2*x/r2;
                    ans2 += c2;
                }
                while ((c2/ans2) > epsilon);

                return Math.Exp(ax)*ans2/a;
            }

            int c = 0;
            double y = 1 - a;
            double z = x + y + 1;

            double p3 = 1;
            double q3 = x;
            double p2 = x + 1;
            double q2 = z*x;
            double ans = p2/q2;

            double error;

            do
            {
                c++;
                y += 1;
                z += 2;
                double yc = y*c;

                double p = (p2*z) - (p3*yc);
                double q = (q2*z) - (q3*yc);

                if (q != 0)
                {
                    double nextans = p/q;
                    error = Math.Abs((ans - nextans)/nextans);
                    ans = nextans;
                }
                else
                {
                    // zero div, skip
                    error = 1;
                }

                // shift
                p3 = p2;
                p2 = p;
                q3 = q2;
                q2 = q;

                // normalize fraction when the numerator becomes large
                if (Math.Abs(p) > big)
                {
                    p3 *= bigInv;
                    p2 *= bigInv;
                    q3 *= bigInv;
                    q2 *= bigInv;
                }
            }
            while (error > epsilon);

            return 1d - (Math.Exp(ax)*ans);
        }

        /// <summary>
        /// Returns the inverse P^(-1) of the regularized lower incomplete gamma function
        /// P(a,x) = 1/Gamma(a) * int(exp(-t)t^(a-1),t=0..x) for real a &gt; 0, x &gt; 0,
        /// such that P^(-1)(a,P(a,x)) == x.
        /// </summary>
        public static double GammaLowerRegularizedInv(double a, double y0)
        {
            const double epsilon = 0.000000000000001;
            const double big = 4503599627370496.0;
            const double threshold = 5*epsilon;

            if (double.IsNaN(a) || double.IsNaN(y0))
            {
                return double.NaN;
            }

            if (a < 0 || a.AlmostEqual(0.0))
            {
                throw new ArgumentOutOfRangeException(nameof(a));
            }

            if (y0 < 0 || y0 > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(y0));
            }

            if (y0.AlmostEqual(0.0))
            {
                return 0d;
            }

            if (y0.AlmostEqual(1.0))
            {
                return double.PositiveInfinity;
            }

            y0 = 1 - y0;

            double xUpper = big;
            double xLower = 0;
            double yUpper = 1;
            double yLower = 0;

            // Initial Guess
            double d = 1/(9*a);
            double y = 1 - d - (0.98*Constants.Sqrt2*ErfInv((2.0*y0) - 1.0)*Math.Sqrt(d));
            double x = a*y*y*y;
            double lgm = GammaLn(a);

            for (int i = 0; i < 20; i++)
            {
                if (x < xLower || x > xUpper)
                {
                    d = 0.0625;
                    break;
                }

                y = 1 - GammaLowerRegularized(a, x);
                if (y < yLower || y > yUpper)
                {
                    d = 0.0625;
                    break;
                }

                if (y < y0)
                {
                    xUpper = x;
                    yLower = y;
                }
                else
                {
                    xLower = x;
                    yUpper = y;
                }

                d = ((a - 1)*Math.Log(x)) - x - lgm;
                if (d < -709.78271289338399)
                {
                    d = 0.0625;
                    break;
                }

                d = -Math.Exp(d);
                d = (y - y0)/d;
                if (Math.Abs(d/x) < epsilon)
                {
                    return x;
                }

                if ((d > (x/4)) && (y0 < 0.05))
                {
                    // Naive heuristics for cases near the singularity
                    d = x/10;
                }

                x -= d;
            }

            if (xUpper == big)
            {
                if (x <= 0)
                {
                    x = 1;
                }

                while (xUpper == big)
                {
                    x = (1 + d)*x;
                    y = 1 - GammaLowerRegularized(a, x);
                    if (y < y0)
                    {
                        xUpper = x;
                        yLower = y;
                        break;
                    }

                    d = d + d;
                }
            }

            int dir = 0;
            d = 0.5;
            for (int i = 0; i < 400; i++)
            {
                x = xLower + (d*(xUpper - xLower));
                y = 1 - GammaLowerRegularized(a, x);
                lgm = (xUpper - xLower)/(xLower + xUpper);
                if (Math.Abs(lgm) < threshold)
                {
                    return x;
                }

                lgm = (y - y0)/y0;
                if (Math.Abs(lgm) < threshold)
                {
                    return x;
                }

                if (x <= 0d)
                {
                    return 0d;
                }

                if (y >= y0)
                {
                    xLower = x;
                    yUpper = y;
                    if (dir < 0)
                    {
                        dir = 0;
                        d = 0.5;
                    }
                    else
                    {
                        if (dir > 1)
                        {
                            d = (0.5*d) + 0.5;
                        }
                        else
                        {
                            d = (y0 - yLower)/(yUpper - yLower);
                        }
                    }

                    dir = dir + 1;
                }
                else
                {
                    xUpper = x;
                    yLower = y;
                    if (dir > 0)
                    {
                        dir = 0;
                        d = 0.5;
                    }
                    else
                    {
                        if (dir < -1)
                        {
                            d = 0.5*d;
                        }
                        else
                        {
                            d = (y0 - yLower)/(yUpper - yLower);
                        }
                    }

                    dir = dir - 1;
                }
            }

            return x;
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
            const double c = 12.0;
            const double d1 = -0.57721566490153286;
            const double d2 = 1.6449340668482264365;
            const double s = 1e-6;
            const double s3 = 1.0/12.0;
            const double s4 = 1.0/120.0;
            const double s5 = 1.0/252.0;
            const double s6 = 1.0/240.0;
            const double s7 = 1.0/132.0;

            if (double.IsNegativeInfinity(x) || double.IsNaN(x))
            {
                return double.NaN;
            }

            // Handle special cases.
            if (x <= 0 && Math.Floor(x) == x)
            {
                return double.NegativeInfinity;
            }

            // Use inversion formula for negative numbers.
            if (x < 0)
            {
                return DiGamma(1.0 - x) + (Math.PI/Math.Tan(-Math.PI*x));
            }

            if (x <= s)
            {
                return d1 - (1/x) + (d2*x);
            }

            double result = 0;
            while (x < c)
            {
                result -= 1/x;
                x++;
            }

            if (x >= c)
            {
                var r = 1/x;
                result += Math.Log(x) - (0.5*r);
                r *= r;

                result -= r*(s3 - (r*(s4 - (r*(s5 - (r*(s6 - (r*s7))))))));
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
            if (double.IsNaN(p))
            {
                return double.NaN;
            }

            if (double.IsNegativeInfinity(p))
            {
                return 0.0;
            }

            if (double.IsPositiveInfinity(p))
            {
                return double.PositiveInfinity;
            }

            var x = Math.Exp(p);
            for (var d = 1.0; d > 1.0e-15; d /= 2.0)
            {
                x += d*Math.Sign(p - DiGamma(x));
            }

            return x;
        }
    }
}
