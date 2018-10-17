using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Hankel function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Hankel function of the first kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static Complex HankelH1(double n, Complex z)
        {
            var amos = new AmosWrapper();
            return amos.Cbesh1(n, z);
        }

        /// <summary>
        /// Hankel function of the first kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="x">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static double HankelH1(double n, double x)
        {
            return HankelH1(n, new Complex(x, 0)).Real;
        }

        /// <summary>
        /// Hankel function of the second kind
        /// </summary>
        /// <param name="n">The order of the Hankel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static Complex HankelH2(double n, Complex z)
        {
            var amos = new AmosWrapper();
            return amos.Cbesh2(n, z);
        }

        /// <summary>
        /// Hankel function of the second kind
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="x">The value to compute the Bessel function of.</param>
        /// <returns></returns>
        public static double HankelH2(double n, double x)
        {
            return HankelH2(n, new Complex(x, 0)).Real;
        }
    }
}
