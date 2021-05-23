using System;
using System.Numerics;

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the modified Bessel function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Returns the Kelvin function of the first kind.
        /// <para>KelvinBe(nu, x) is given by BesselJ(0, j * sqrt(j) * x) where j = sqrt(-1).</para>
        /// <para>KelvinBer(nu, x) and KelvinBei(nu, x) are the real and imaginary parts of the KelvinBe(nu, x)</para>
        /// </summary>
        /// <param name="nu">the order of the the Kelvin function.</param>
        /// <param name="x">The value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function of the first kind.</returns>
        public static Complex KelvinBe(double nu, double x)
        {
            Complex ISqrtI = new Complex(-Constants.Sqrt1Over2, Constants.Sqrt1Over2); // j * sqrt(j) = (-1)^(3/4) = (-1 + j)/sqrt(2)
            return BesselJ(nu, ISqrtI * x);
        }

        /// <summary>
        /// Returns the Kelvin function ber.
        /// <para>KelvinBer(nu, x) is given by the real part of BesselJ(nu, j * sqrt(j) * x) where j = sqrt(-1).</para>
        /// </summary>
        /// <param name="nu">the order of the the Kelvin function.</param>
        /// <param name="x">The value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function ber.</returns>
        public static double KelvinBer(double nu, double x)
        {
            return KelvinBe(nu, x).Real;
        }

        /// <summary>
        /// Returns the Kelvin function ber.
        /// <para>KelvinBer(x) is given by the real part of BesselJ(0, j * sqrt(j) * x) where j = sqrt(-1).</para>
        /// <para>KelvinBer(x) is equivalent to KelvinBer(0, x).</para>
        /// </summary>
        /// <param name="x">The value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function ber.</returns>
        public static double KelvinBer(double x)
        {
            return KelvinBe(0, x).Real;
        }

        /// <summary>
        /// Returns the Kelvin function bei.
        /// <para>KelvinBei(nu, x) is given by the imaginary part of BesselJ(nu, j * sqrt(j) * x) where j = sqrt(-1).</para>
        /// </summary>
        /// <param name="nu">the order of the the Kelvin function.</param>
        /// <param name="x">The value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function bei.</returns>
        public static double KelvinBei(double nu, double x)
        {
            return KelvinBe(nu, x).Imaginary;
        }

        /// <summary>
        /// Returns the Kelvin function bei.
        /// <para>KelvinBei(x) is given by the imaginary part of BesselJ(0, j * sqrt(j) * x) where j = sqrt(-1).</para>
        /// <para>KelvinBei(x) is equivalent to KelvinBei(0, x).</para>
        /// </summary>
        /// <param name="x">The value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function bei.</returns>
        public static double KelvinBei(double x)
        {
            return KelvinBe(0, x).Imaginary;
        }

        /// <summary>
        /// Returns the derivative of the Kelvin function ber.
        /// </summary>
        /// <param name="nu">The order of the Kelvin function.</param>
        /// <param name="x">The value to compute the derivative of the Kelvin function of.</param>
        /// <returns>the derivative of the Kelvin function ber</returns>
        public static double KelvinBerPrime(double nu, double x)
        {
            const double inv2Sqrt2 = 0.35355339059327376220042218105242451964241796884424; // 1/(2 * sqrt(2))
            return inv2Sqrt2 * (-KelvinBer(nu - 1, x) + KelvinBer(nu + 1, x) - KelvinBei(nu - 1, x) + KelvinBei(nu + 1, x));
        }

        /// <summary>
        /// Returns the derivative of the Kelvin function ber.
        /// </summary>
        /// <param name="x">The value to compute the derivative of the Kelvin function of.</param>
        /// <returns>The derivative of the Kelvin function ber.</returns>
        public static double KelvinBerPrime(double x)
        {
            return KelvinBerPrime(0, x);
        }

        /// <summary>
        /// Returns the derivative of the Kelvin function bei.
        /// </summary>
        /// <param name="nu">The order of the Kelvin function.</param>
        /// <param name="x">The value to compute the derivative of the Kelvin function of.</param>
        /// <returns>the derivative of the Kelvin function bei.</returns>
        public static double KelvinBeiPrime(double nu, double x)
        {
            const double inv2Sqrt2 = 0.35355339059327376220042218105242451964241796884424; // 1/(2 * sqrt(2))
            return inv2Sqrt2 * (KelvinBer(nu - 1, x) - KelvinBer(nu + 1, x) - KelvinBei(nu - 1, x) + KelvinBei(nu + 1, x));
        }

        /// <summary>
        /// Returns the derivative of the Kelvin function bei.
        /// </summary>
        /// <param name="x">The value to compute the derivative of the Kelvin function of.</param>
        /// <returns>The derivative of the Kelvin function bei.</returns>
        public static double KelvinBeiPrime(double x)
        {
            return KelvinBeiPrime(0, x);
        }

        /// <summary>
        /// Returns the Kelvin function of the second kind
        /// <para>KelvinKe(nu, x) is given by Exp(-nu * pi * j / 2) * BesselK(nu, x * sqrt(j)) where j = sqrt(-1).</para>
        /// <para>KelvinKer(nu, x) and KelvinKei(nu, x) are the real and imaginary parts of the KelvinBe(nu, x)</para>
        /// </summary>
        /// <param name="nu">The order of the Kelvin function.</param>
        /// <param name="x">The value to calculate the kelvin function of,</param>
        /// <returns></returns>
        public static Complex KelvinKe(double nu, double x)
        {
            Complex PiIOver2 = new Complex(0.0, Constants.PiOver2); // pi * I / 2
            Complex SqrtI = new Complex(Constants.Sqrt1Over2, Constants.Sqrt1Over2); // sqrt(j) = (-1)^(1/4) = (1 + j)/sqrt(2)
            return Complex.Exp(-nu * PiIOver2) * BesselK(nu, SqrtI * x);
        }

        /// <summary>
        /// Returns the Kelvin function ker.
        /// <para>KelvinKer(nu, x) is given by the real part of Exp(-nu * pi * j / 2) * BesselK(nu, sqrt(j) * x) where j = sqrt(-1).</para>
        /// </summary>
        /// <param name="nu">the order of the the Kelvin function.</param>
        /// <param name="x">The non-negative real value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function ker.</returns>
        public static double KelvinKer(double nu, double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }

            return KelvinKe(nu, x).Real;
        }

        /// <summary>
        /// Returns the Kelvin function ker.
        /// <para>KelvinKer(x) is given by the real part of Exp(-nu * pi * j / 2) * BesselK(0, sqrt(j) * x) where j = sqrt(-1).</para>
        /// <para>KelvinKer(x) is equivalent to KelvinKer(0, x).</para>
        /// </summary>
        /// <param name="x">The non-negative real value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function ker.</returns>
        public static double KelvinKer(double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }

            return KelvinKe(0, x).Real;
        }

        /// <summary>
        /// Returns the Kelvin function kei.
        /// <para>KelvinKei(nu, x) is given by the imaginary part of Exp(-nu * pi * j / 2) * BesselK(nu, sqrt(j) * x) where j = sqrt(-1).</para>
        /// </summary>
        /// <param name="nu">the order of the the Kelvin function.</param>
        /// <param name="x">The non-negative real value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function kei.</returns>
        public static double KelvinKei(double nu, double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }

            return KelvinKe(nu, x).Imaginary;
        }

        /// <summary>
        /// Returns the Kelvin function kei.
        /// <para>KelvinKei(x) is given by the imaginary part of Exp(-nu * pi * j / 2) * BesselK(0, sqrt(j) * x) where j = sqrt(-1).</para>
        /// <para>KelvinKei(x) is equivalent to KelvinKei(0, x).</para>
        /// </summary>
        /// <param name="x">The non-negative real value to compute the Kelvin function of.</param>
        /// <returns>The Kelvin function kei.</returns>
        public static double KelvinKei(double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }

            return KelvinKe(0, x).Imaginary;
        }

        /// <summary>
        /// Returns the derivative of the Kelvin function ker.
        /// </summary>
        /// <param name="nu">The order of the Kelvin function.</param>
        /// <param name="x">The non-negative real value to compute the derivative of the Kelvin function of.</param>
        /// <returns>The derivative of the Kelvin function ker.</returns>
        public static double KelvinKerPrime(double nu, double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }

            const double inv2Sqrt2 = 0.35355339059327376220042218105242451964241796884424; // 1/(2 * sqrt(2))
            return inv2Sqrt2 * (-KelvinKer(nu - 1, x) + KelvinKer(nu + 1, x) - KelvinKei(nu - 1, x) + KelvinKei(nu + 1, x));
        }

        /// <summary>
        /// Returns the derivative of the Kelvin function ker.
        /// </summary>
        /// <param name="x">The value to compute the derivative of the Kelvin function of.</param>
        /// <returns>The derivative of the Kelvin function ker.</returns>
        public static double KelvinKerPrime(double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }

            return KelvinKerPrime(0, x);
        }

        /// <summary>
        /// Returns the derivative of the Kelvin function kei.
        /// </summary>
        /// <param name="nu">The order of the Kelvin function.</param>
        /// <param name="x">The value to compute the derivative of the Kelvin function of.</param>
        /// <returns>The derivative of the Kelvin function kei.</returns>
        public static double KelvinKeiPrime(double nu, double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }

            const double inv2Sqrt2 = 0.35355339059327376220042218105242451964241796884424; // 1/(2 * sqrt(2))
            return inv2Sqrt2 * (KelvinKer(nu - 1, x) - KelvinKer(nu + 1, x) - KelvinKei(nu - 1, x) + KelvinKei(nu + 1, x));
        }

        /// <summary>
        /// Returns the derivative of the Kelvin function kei.
        /// </summary>
        /// <param name="x">The value to compute the derivative of the Kelvin function of.</param>
        /// <returns>The derivative of the Kelvin function kei.</returns>
        public static double KelvinKeiPrime(double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }

            return KelvinKeiPrime(0, x);
        }
    }
}
