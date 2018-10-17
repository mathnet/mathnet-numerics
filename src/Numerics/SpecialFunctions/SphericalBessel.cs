using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the spherical Bessel functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Spherical Bessel function of the first kind
        /// </summary>
        /// <param name="n">The order of the spherical Bessel function</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <returns></returns>
        public static Complex SphericalBesselJ(double n, Complex z)
        {
            const double rthpi = 1.2533141373155002512; //sqrt(pi/2)

            return rthpi * BesselJ(n + 0.5, z) / Complex.Sqrt(z);
        }

        /// <summary>
        /// Spherical Bessel function of the first kind
        /// </summary>
        /// <param name="n">The order of the spherical Bessel function</param>
        /// <param name="x">The value to compute the spherical Bessel function of.</param>
        /// <returns></returns>
        public static double SphericalBesselJ(double n, double x)
        {
            const double rthpi = 1.2533141373155002512; //sqrt(pi/2)

            return rthpi * BesselJ(n + 0.5, x) / Math.Sqrt(x);
        }

        /// <summary>
        /// Spherical Bessel function of the second kind
        /// </summary>
        /// <param name="n">The order of the spherical Bessel function</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <returns></returns>
        public static Complex SphericalBesselY(double n, Complex z)
        {
            const double rthpi = 1.2533141373155002512; //sqrt(pi/2)

            return rthpi * BesselY(n + 0.5, z) / Complex.Sqrt(z);
        }

        /// <summary>
        /// Spherical Bessel function of the second kind
        /// </summary>
        /// <param name="n">The order of the spherical Bessel function</param>
        /// <param name="x">The value to compute the spherical Bessel function of.</param>
        /// <returns></returns>
        public static double SphericalBesselY(double n, double x)
        {
            const double rthpi = 1.2533141373155002512; //sqrt(pi/2)

            return rthpi * BesselY(n + 0.5, x) / Math.Sqrt(x);
        }
    }
}
