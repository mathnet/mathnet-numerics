using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics
{
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Computes the Exponential Integral function.
        /// </summary>
        /// <param name="x">The argument of the Exponential Integral function.</param>
        /// <returns>The value of the Exponential Integral function.</returns>
        /// <remarks>
        /// <para>This implementation of the computation of the Exponential Integral function follows the derivation in
        ///     "Handbook of Mathematical Functions, Applied Mathematics Series, Volume 55", Abramowitz, M., and Stegun, I.A. 1964,  reprinted 1968 by
        ///     Dover Publications, New York), Chapters 6, 7, and 26.
        ///     AND
        ///     "Advanced mathematical methods for scientists and engineers", Bender, Carl M.; Steven A. Orszag (1978). page 253
        /// </para>
        /// <para>
        ///     for x > 1  uses continued fraction approach that is often used to compute incomplete gamma.
        ///     for 0 < x <= 1 uses taylor series expansion
        /// </para>
        /// <para>Our unit tests suggest that the accuracy of the Exponential Integral function is correct up to 13 floating point digits.</para>
        /// </remarks>
        public static double ExponentialIntegral(double x, int n)
        {
            //parameter validation
            if (n < 0 || x < 0.0 ) {
                throw new ArgumentOutOfRangeException(string.Format("x and n must be positive: x={0}, n={1}", x, n));                
            }

            const double epsilon = 0.00000000000000001;
            int maxIterations = 100;
            int i, ii;
            double ndbl = (double)n;
            double result = double.NaN;
            double nearDoubleMin = 1e-100; //needs a very small value that is not quite as small as the lowest value double can take
            double factorial = 1.0d;
            double del;
            double psi;
            double a, b, c, d, h; //variables for continued fraction

            //special cases
            if (n == 0)
            {
                result = Math.Exp( -1.0d * x ) / x;
                return result;
            }
            else if (x == 0.0d)
            {
                result = 1.0d / (ndbl - 1.0d);
                return result;
            }
            //general cases
            //continued fraction for large x
            if (x > 1.0d)                
            { 
                b = x + ((double)n);
                c = 1.0d / nearDoubleMin;
                d = 1.0d / b;
                h = d;
                for (i = 1; i <= maxIterations; i++)
                {
                    a = -1.0d * ((double)i) * ((ndbl - 1.0d) + (double)i);
                    b += 2.0d;
                    d = 1.0d / (a * d + b); 
                    c = b + a / c;
                    del = c * d;
                    h = h * del;
                    if (Math.Abs(del - 1.0d) < epsilon)
                    {
                        result = h * Math.Exp( -x );
                        return result;
                    }
                }
                throw new ArithmeticException(string.Format("continued fraction failed to converge for x={0}, n={1})", x, n));
            }
            //series computation for small x
            else
            {
                result = ((ndbl - 1.0d) != 0 ? 1.0 / (ndbl - 1.0d) : (-1.0d * Math.Log(x) - Constants.EulerMascheroni)); //Set first term.
                for (i = 1; i <= maxIterations; i++)
                {
                    factorial *= (-1.0d * x / ((double)i));
                    if (i != (ndbl - 1.0d)) { del = -factorial / (i - (ndbl - 1.0d)); }
                    else
                    {
                        psi = -1.0d * Constants.EulerMascheroni; 
                        for (ii = 1; ii <= (ndbl - 1.0d); ii++)
                        {
                            psi += (1.0d / ((double)ii));
                        }
                        del = factorial * (-1.0d * Math.Log(x) + psi);
                    }
                    result += del;
                    if (Math.Abs(del) < Math.Abs(result) * epsilon)
                    {
                        return result;
                    }
                }
                throw new ArithmeticException(string.Format("series failed to converge for x={0}, n={1})", x, n));
            }
        }
    }
}
