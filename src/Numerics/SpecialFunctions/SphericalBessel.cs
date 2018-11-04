using System;
using System.Numerics;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the spherical Bessel functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Spherical Bessel function of the first kind, j(v, z).
        /// <p/>
        /// If expScaled is true, returns Exp(-Abs(y)) * j(v, z) where y = z.Imaginary.
        /// </summary>
        /// <param name="v">The order of the spherical Bessel function</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled spherical Bessel function</param>
        /// <returns></returns>
        public static Complex SphericalBesselJ(double v, Complex z, bool expScaled = false)
        {
            const double rthpi = 1.2533141373155002512; //sqrt(pi/2)

            return rthpi * BesselJ(v + 0.5, z, expScaled) / Complex.Sqrt(z);
        }

        /// <summary>
        /// Spherical Bessel function of the first kind, j(v, z).
        /// <p/>
        /// If expScaled is true, returns Exp(-Abs(y)) * j(v, z) where y = z.Imaginary.
        /// </summary>
        /// <param name="v">The order of the spherical Bessel function</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled spherical Bessel function</param>
        /// <returns></returns>
        public static double SphericalBesselJ(double v, double z, bool expScaled = false)
        {
            const double rthpi = 1.2533141373155002512; //sqrt(pi/2)

            return rthpi * BesselJ(v + 0.5, z, expScaled) / Math.Sqrt(z);
        }

        /// <summary>
        /// Spherical Bessel function of the second kind, y(v, z).
        /// <p/>
        /// If expScaled is true, returns Exp(-Abs(y)) * y(v, z) where y = z.Imaginary.
        /// </summary>
        /// <param name="v">The order of the spherical Bessel function</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled spherical Bessel function</param>
        /// <returns></returns>
        public static Complex SphericalBesselY(double v, Complex z, bool expScaled = false)
        {
            const double rthpi = 1.2533141373155002512; //sqrt(pi/2)

            return rthpi * BesselY(v + 0.5, z, expScaled) / Complex.Sqrt(z);
        }

        /// <summary>
        /// Spherical Bessel function of the second kind, y(v, z).
        /// <p/>
        /// If expScaled is true, returns Exp(-Abs(y)) * y(v, z) where y = z.Imaginary.
        /// </summary>
        /// <param name="v">The order of the spherical Bessel function</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled spherical Bessel function</param>
        /// <returns></returns>
        public static double SphericalBesselY(double v, double z, bool expScaled = false)
        {
            const double rthpi = 1.2533141373155002512; //sqrt(pi/2)

            return rthpi * BesselY(v + 0.5, z, expScaled) / Math.Sqrt(z);
        }
    }
}
