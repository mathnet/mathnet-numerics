using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the log1p function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Computes ln(1+x) with good relative precision when |x| is small
        /// </summary>
        /// <param name="x">The parameter for which to compute the log1p function. Range: x > 0.</param>
        public static double Log1p(double x)
        {
            double y0 = Math.Log(1.0 + x);

            if ((-0.2928 < x) && (x < 0.4142))
            {
                double y = y0;

                if (y == 0.0)
                {
                    y = 1.0;
                }
                else if ((y < -0.69) || (y > 0.4))
                {
                    y = (Math.Exp(y) - 1.0) / y;
                }
                else
                {
                    double t = y / 2.0;
                    y = Math.Exp(t) * Math.Sinh(t) / t;
                }

                double s = y0 * y;
                double r = (s - x) / (s + 1.0);
                y0 = y0 - r * (6 - r) / (6 - 4 * r);
            }

            return y0;
        }
    }
}
