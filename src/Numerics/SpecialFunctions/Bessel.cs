using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Bessel functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Bessel function of the first kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static Complex BesselJ(double n, Complex z)
        {
            var amos = new AmosWrapper();
            return amos.Cbesj(n, z);
        }

        /// <summary>
        /// Bessel function of the first kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="x">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static double BesselJ(double n, double x)
        {
            return BesselJ(n, new Complex(x, 0)).Real;
        }

        /// <summary>
        /// Bessel function of the second kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static Complex BesselY(double n, Complex z)
        {
            var amos = new AmosWrapper();
            return amos.Cbesy(n, z);
        }

        /// <summary>
        /// Bessel function of the second kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="x">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static double BesselY(double n, double x)
        {
            var amos = new AmosWrapper();
            return amos.CbesyReal(n, x);
        }

        /// <summary>
        /// Modified Bessel function of the first kind, of order n 
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static Complex BesselI(double n, Complex z)
        {
            var amos = new AmosWrapper();
            return amos.Cbesi(n, z);
        }

        /// <summary>
        /// Modified Bessel function of the first kind, of order n  
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="x">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static double BesselI(double n, double x)
        {
            return BesselI(n, new Complex(x, 0)).Real;
        }

        /// <summary>
        /// Modified Bessel function of the second kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static Complex BesselK(double n, Complex z)
        {
            var amos = new AmosWrapper();
            return amos.Cbesk(n, z);
        }

        /// <summary>
        /// Modified Bessel function of the second kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="x">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static double BesselK(double n, double x)
        {
            var amos = new AmosWrapper();
            return amos.CbeskReal(n, x);
        }
    }
}
