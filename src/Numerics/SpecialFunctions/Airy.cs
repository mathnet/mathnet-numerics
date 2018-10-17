using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Airy functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Airy function Ai
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns></returns>
        public static Complex AiryAi(Complex z)
        {
            var amos = new AmosWrapper();
            return amos.Cairy(z);
        }

        /// <summary>
        /// Airy function Ai
        /// </summary>
        /// <param name="x">The value to compute the Airy function of.</param>
        /// <returns></returns>
        public static double AiryAi(double x)
        {
            return AiryAi(new Complex(x, 0)).Real;
        }

        /// <summary>
        /// Derivative of the Airy function Ai
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns></returns>
        public static Complex AiryAiPrime(Complex z)
        {
            var amos = new AmosWrapper();
            return amos.CairyPrime(z);
        }

        /// <summary>
        /// Derivative of the Airy function Ai
        /// </summary>
        /// <param name="x">The value to compute the derivative of the Airy function of.</param>
        /// <returns></returns>
        public static double AiryAiPrime(double x)
        {
            return AiryAiPrime(new Complex(x, 0)).Real;
        }

        /// <summary>
        /// Airy function Bi(z)
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns></returns>
        public static Complex AiryBi(Complex z)
        {
            var amos = new AmosWrapper();
            return amos.Cbiry(z);
        }

        /// <summary>
        /// Airy function Bi(x)
        /// </summary>
        /// <param name="x">The value to compute the Airy function of.</param>
        /// <returns></returns>
        public static double AiryBi(double x)
        {
            return AiryBi(new Complex(x, 0)).Real;
        }

        /// <summary>
        /// Derivative of the Airy function Bi(z)
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns></returns>
        public static Complex AiryBiPrime(Complex z)
        {
            var amos = new AmosWrapper();
            return amos.CbiryPrime(z);
        }

        /// <summary>
        /// Derivative of the Airy function Bi(z)
        /// </summary>
        /// <param name="x">The value to compute the derivative of the Airy function of.</param>
        /// <returns></returns>
        public static double AiryBiPrime(double x)
        {
            return AiryBiPrime(new Complex(x, 0)).Real;
        }
    }
}
