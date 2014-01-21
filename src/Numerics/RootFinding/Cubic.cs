using System;

#if !NOSYSNUMERICS
    using Complex = System.Numerics.Complex;
#endif

namespace MathNet.Numerics.RootFinding
{
    /// <summary>
    /// Finds roots to the cubic equation x^3 + a2*x^2 + a1*x + a0 = 0
    /// Implements the cubic formula in http://mathworld.wolfram.com/CubicFormula.html
    /// </summary>
    public static class Cubic
    {
        // D = Q^3 + R^2 is the polynomial discriminant.
        // D > 0, 1 real root
        // D = 0, 3 real roots, at least two are equal
        // D < 0, 3 real and unequal roots

        /// <summary>
        /// Q and R are transformed variables.
        /// </summary>
        private static void QR(double a2, double a1, double a0, ref double Q, ref double R)
		{
			Q = (3 * a1 - a2 * a2)/9.0;
			R = (9.0 * a2 * a1 - 27 * a0 - 2 * a2 * a2 * a2)/54.0;           
		}

        /// <summary>
        /// n^(1/3) - work around a negative double raised to (1/3)
        /// </summary>
        private static double PowThird(double n)
        {
            return Math.Pow(Math.Abs(n), 1d / 3d) * Math.Sign(n);
        }

        public static Tuple<double, double, double> RealRoots(double a2, double a1, double a0)
        {
            var Q = double.NaN;
            var R = double.NaN;
            QR(a2, a1, a0, ref Q, ref R);

            var Q3 = Q * Q * Q;
            var D = Q3 + R * R;
            var shift = -a2 / 3d;

            double x1 = double.NaN;
            double x2 = double.NaN;
            double x3 = double.NaN;

            // when D >= 0, use eqn (54)-(56) where S and T are real
            if (D >= 0)
            {
                double sqrtD = Math.Pow(D, 0.5);
                double S = PowThird(R + sqrtD);
                double T = PowThird(R - sqrtD);
                x1 = shift + (S + T);
                if (D == 0)
                    x2 = shift - S;
            }
            // 3 real roots, use eqn (70)-(73) to calculate the real roots  
            else
            {
                double theta = Math.Acos(R / Math.Sqrt(-Q3));
                x1 = 2d * Math.Sqrt(-Q) * Math.Cos(theta / 3.0) + shift;
                x2 = 2d * Math.Sqrt(-Q) * Math.Cos((theta + 2.0 * Constants.Pi) / 3d) + shift;
                x3 = 2d * Math.Sqrt(-Q) * Math.Cos((theta - 2.0 * Constants.Pi) / 3d) + shift;
            }
            return Tuple.Create(x1, x2, x3);
        }

        public static Tuple<Complex, Complex, Complex> Roots(double a2, double a1, double a0)
        {
            // use eqn (54)-(56)
            var Q = double.NaN;
            var R = double.NaN;
            QR(a2, a1, a0, ref Q, ref R);

            var D = Q * Q * Q + R * R;

            var rootD = Complex.Sqrt(D);
            var S = Complex.Pow(R + rootD, 1d / 3d);
            var T = Complex.Pow(R - rootD, 1d / 3d);
            var shift = -a2 / 3d;
            var sharedI = 0.5 * Complex.ImaginaryOne * Math.Sqrt(3) * (S - T);

            var x1 = shift + (S + T);
            var x2 = shift - 0.5 * (S + T);
            var x3 = x2;
            x2 += sharedI;
            x3 -= sharedI;

            return Tuple.Create(x1, x2, x3);
        }
    }
}
