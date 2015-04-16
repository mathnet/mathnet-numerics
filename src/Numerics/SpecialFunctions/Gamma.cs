// <copyright file="Gamma.cs" company="Math.NET">
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

// <contribution>
//    Cephes Math Library, Stephen L. Moshier
//    ALGLIB 2.0.1, Sergey Bochkanov
// </contribution>

// ReSharper disable CheckNamespace
using System;

namespace MathNet.Numerics
// ReSharper restore CheckNamespace
{
  public static partial class SpecialFunctions
  {
    private const double OVERFLOW_INPUT = 171.624;
    private const double LOWER_BREAKPOINT = 0.004;
    private const double UPPER_BREAKPOINT = OVERFLOW_INPUT;
    /* The more precision your numerical type has, the further apart the lower and upper breakpoints
     * should be.  The ones above are for a C# double, circa 2014, which has a 52-bit mantissa.  By
     * setting UPPER_BREAKPOINT = OVERFLOW_INPUT, we are effectively using two ranges, not three.*/
    public static double Gamma
    (
      double x
    ) {
      if (x <= 0.0) {
        int offset = 0;
        double product = 1;
        while (x < 0.0) {
          product *= x;
          x++;
          offset++;
        }
        if (x == 0.0) {
          return Double.NaN;
        }
        double gammaPositiveArgument = SpecialFunctions.Gamma(x);
        double r = gammaPositiveArgument / product;
        return r;
      }

      // Split the function domain into three intervals:
      // (0, LOWER_BREAKPOINT), [LOWER_BREAKPOINT, UPPER_BREAKPOINT), and (UPPER_BREAKPOINT, infinity)
      // The upper breakpoint has historically been 12, not OVERFLOW_INPUT. But changing it
      // to OVERFLOW_INPUT produces much better results.  It may be that 12 was notify back when
      // machine epsilon was larger than it is today.

      ///////////////////////////////////////////////////////////////////////////
      // First interval: (0, LOWER_BREAKPOINT)
      //
      // For small x, 1/Gamma(x) has power series x + gamma x^2  - ...
      // So in this range, 1/Gamma(x) = x + gamma x^2 with error on the order of x^3.
      // The relative error over this interval is less than 6e-7.

      const double gamma = 0.577215664901532860606512090; // Euler's gamma constant
      const double coefX3 = -0.65587807152025388107701951514539048127976638047858; //(gamma ^ 2/2) - pi ^ 2/12
      const double coefX4 = -0.04200263503409523552900393487; // formula gets messy . . .
      const double coefX5 = 0.166538611382291489501700795102;

      if (x < LOWER_BREAKPOINT) {
        double inverseOfGamma = x * (1.0 +  x * (gamma + x * (coefX3 + x * (coefX4 + x * coefX5))));
        double r = 1.0 / inverseOfGamma;
        return r;
      }


      ///////////////////////////////////////////////////////////////////////////
      // Second interval: [LOWER_BREAKPOINT, UPPER_BREAKPOINT)

      if (x < UPPER_BREAKPOINT) {
        // The algorithm directly approximates gamma over (1,2) and uses
        // reduction identities to reduce other arguments to this interval.

        double y = x;
        int n = 0;
        bool arg_was_less_than_one = (y < 1.0);

        // Add or subtract integers as necessary to bring y into (1,2)
        // Will correct for this below
        if (arg_was_less_than_one) {
          y += 1.0;
        } else {
          n = (int) (Math.Floor(y)) - 1;  // will use n later
          y -= n;
        }

        // numerator coefficients for approximation over the interval (1,2)
        double[] p = {
          -1.71618513886549492533811E+0,
          2.47656508055759199108314E+1,
          -3.79804256470945635097577E+2,
          6.29331155312818442661052E+2,
          8.66966202790413211295064E+2,
          -3.14512729688483675254357E+4,
          -3.61444134186911729807069E+4,
          6.64561438202405440627855E+4
        };

        // denominator coefficients for approximation over the interval (1,2)
        double[] q =
          {
            -3.08402300119738975254353E+1,
            3.15350626979604161529144E+2,
            -1.01515636749021914166146E+3,
            -3.10777167157231109440444E+3,
            2.25381184209801510330112E+4,
            4.75584627752788110767815E+3,
            -1.34659959864969306392456E+5,
            -1.15132259675553483497211E+5
          };

        double num = 0.0;
        double den = 1.0;
        int i;

        double z = y - 1;
        for (i = 0; i < 8; i++) {
          num = (num + p[i]) * z;
          den = den * z + q[i];
        }
        double result = num / den + 1.0;

        // Apply correction if argument was not initially in (1,2)
        if (arg_was_less_than_one) {
          // Use identity gamma(z) = gamma(z+1)/z
          // The variable "result" now holds gamma of the original y + 1
          // Thus we use y-1 to get back the orginal y.
          result /= (y - 1.0);
        } else {
          // Use the identity gamma(z+n) = z*(z+1)* ... *(z+n-1)*gamma(z)
          for (i = 0; i < n; i++)
            result *= y++;
        }

        return result;
      }

      ///////////////////////////////////////////////////////////////////////////
      // Third interval: [UPPER_BREAKPOINT, infinity)

      if (x > OVERFLOW_INPUT) {
        // Correct answer too large to display. 
        return double.PositiveInfinity;
      }

      return Math.Exp(GammaLn(x));
    }
    /// <summary>Computes Sin(pi x).  
    private static double SinPiTimes(double xIn) {
      if (xIn < 0) {
        return -SinPiTimes(-xIn);
      }
      double x = xIn % 2;
      if (x > 1) {
        return -SinPiTimes(x - 1);
      }
      if (x > 0.5) {
        return SinPiTimes(1 - x);
      }
      // x is now known to be between 0 and 0.5.  This avoids the roundoff error
      // magnification that would happen if we were to compute something like
      // Math.Sin(Math.Pi * 1).
      return Math.Sin(Math.PI * x);
    }
    private static double LogPi = 1.1447298858494001741434273513;
    public static double GammaLn
    (
      double x
    ) {
      if (x <= 0.0) {
        // We use Euler's Reflection Formula:
        // Gamma(z) * Gamma(1-z) = pi / sin (pi z).
        // Taking logs gives us
        // LogGamma(z) + LogGamma(1-z) = Log(pi) - Log(sin(pi z)).
        // However, we need to be careful with the Sine function.  The problem is that if
        // we multiply by pi and use Math.Sin, we will get very bad answers when
        // z is close to an integer.
        double reflectedValue = GammaLn(1-x);
        double sinPiX = SpecialFunctions.SinPiTimes(x);
        if (sinPiX <= 0) {
          return double.NaN;
        }
        double r = SpecialFunctions.LogPi - Math.Log(sinPiX) - reflectedValue;
        return r;
      }

      if (x < UPPER_BREAKPOINT) {
        return Math.Log(Math.Abs(Gamma(x)));
      }

      // Abramowitz and Stegun 6.1.41
      // Asymptotic series should be good to at least 11 or 12 figures
      // For error analysis, see Whittiker and Watson
      // A Course in Modern Analysis (1927), page 252

      double[] c =
        {
          1.0/12.0,
          -1.0/360.0,
          1.0/1260.0,
          -1.0/1680.0,
          1.0/1188.0,
          -691.0/360360.0,
          1.0/156.0,
          -3617.0/122400.0
        };
      double z = 1.0 / (x * x);
      double sum = c[7];
      for (int i = 6; i >= 0; i--) {
        sum *= z;
        sum += c[i];
      }
      double series = sum / x;

      double halfLogTwoPi = 0.91893853320467274178032973640562;
      double logGamma = (x - 0.5) * Math.Log(x) - x + halfLogTwoPi + series;
      return logGamma;
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

            if (x <= 0d || a <= 0d)
            {
                return 1d;
            }

            if (x < 1d || x < a)
            {
                return 1d - GammaLowerRegularized(a, x);
            }

            double ax = a * Math.Log(x) - x - GammaLn(a);
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
            double qkm1 = z * x;
            double ans = pkm1 / qkm1;
            do
            {
                c = c + 1;
                y = y + 1;
                z = z + 2;
                double yc = y * c;
                double pk = pkm1 * z - pkm2 * yc;
                double qk = qkm1 * z - qkm2 * yc;
                if (qk != 0)
                {
                    double r = pk / qk;
                    t = Math.Abs((ans - r) / r);
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
                    pkm2 = pkm2 * bigInv;
                    pkm1 = pkm1 * bigInv;
                    qkm2 = qkm2 * bigInv;
                    qkm1 = qkm1 * bigInv;
                }
            }
            while (t > epsilon);

            return ans * ax;
        }

        /// <summary>
        /// Returns the upper incomplete gamma function
        /// Gamma(a,x) = 1/Gamma(a) * int(exp(-t)t^(a-1),t=0..x) for real a &gt; 0, x &gt; 0.
        /// </summary>
        /// <param name="a">The argument for the gamma function.</param>
        /// <param name="x">The lower integral limit.</param>
        /// <returns>The upper incomplete gamma function.</returns>
        public static double GammaUpperIncomplete(double a, double x)
        {
            return GammaUpperRegularized(a, x) * Gamma(a);
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
            return GammaLowerRegularized(a, x) * Gamma(a);
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
                throw new ArgumentOutOfRangeException("a", Properties.Resources.ArgumentNotNegative);
            }
            if (x < 0d)
            {
                throw new ArgumentOutOfRangeException("x", Properties.Resources.ArgumentNotNegative);
            }

            if (a.AlmostEqual(0.0))
            {
                if (x.AlmostEqual(0.0))
                {
                    // either 0 or 1, depending on the limit direction
                    return Double.NaN;
                }

                return 1d;
            }

            if (x.AlmostEqual(0.0))
            {
                return 0d;
            }

            double ax = (a * Math.Log(x)) - x - GammaLn(a);
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
                    c2 = c2 * x / r2;
                    ans2 += c2;
                }
                while ((c2 / ans2) > epsilon);

                return Math.Exp(ax) * ans2 / a;
            }

            int c = 0;
            double y = 1 - a;
            double z = x + y + 1;

            double p3 = 1;
            double q3 = x;
            double p2 = x + 1;
            double q2 = z * x;
            double ans = p2 / q2;

            double error;

            do
            {
                c++;
                y += 1;
                z += 2;
                double yc = y * c;

                double p = (p2 * z) - (p3 * yc);
                double q = (q2 * z) - (q3 * yc);

                if (q != 0)
                {
                    double nextans = p / q;
                    error = Math.Abs((ans - nextans) / nextans);
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

            return 1d - (Math.Exp(ax) * ans);
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
                throw new ArgumentOutOfRangeException("a");
            }
            if (y0 < 0 || y0 > 1)
            {
                throw new ArgumentOutOfRangeException("y0");
            }

            if (y0.AlmostEqual(0.0))
            {
                return 0d;
            }

            if (y0.AlmostEqual(1.0))
            {
                return Double.PositiveInfinity;
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

            for (int i = 0; i < 10; i++)
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
            const double s3 = 1.0 / 12.0;
            const double s4 = 1.0 / 120.0;
            const double s5 = 1.0 / 252.0;
            const double s6 = 1.0 / 240.0;
            const double s7 = 1.0 / 132.0;

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

            if (x <= s)
            {
                return d1 - (1 / x) + (d2 * x);
            }

            double result = 0;
            while (x < c)
            {
                result -= 1 / x;
                x++;
            }

            if (x >= c)
            {
                var r = 1 / x;
                result += Math.Log(x) - (0.5 * r);
                r *= r;

                result -= r * (s3 - (r * (s4 - (r * (s5 - (r * (s6 - (r * s7))))))));
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
    }
}