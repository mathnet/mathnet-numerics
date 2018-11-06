using System.Numerics;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Bessel functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Returns the Bessel function of the first kind.
        /// <para>BesselJ(n, z) is a solution to the Bessel differential equation.</para>
        /// <para>BesselJ(n, z, Scale.Exponential) returns Exp(-Abs(z.Imaginary)) * BesselJ(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The Bessel function of the first kind.</returns>
        public static Complex BesselJ(double n, Complex z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbesj(n, z) : Amos.Cbesj(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Bessel function of the first kind.
        /// <para>ScaledBesselJ(n, z) is given by Exp(-Abs(z.Imaginary)) * BesselJ(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The exponentially scaled Bessel function of the first kind.</returns>
        public static Complex ScaledBesselJ(double n, Complex z)
        {
            return Amos.ScaledCbesj(n, z);
        }

        /// <summary>
        /// Returns the Bessel function of the first kind.
        /// <para>BesselJ(n, z) is a solution to the Bessel differential equation.</para>
        /// <para>BesselJ(n, z, Scale.Exponential) returns Exp(-Abs(z.Imaginary)) * J(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The Bessel function of the first kind.</returns>
        public static double BesselJ(double n, double z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbesj(n, z) : Amos.Cbesj(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Bessel function of the first kind.
        /// <para>ScaledBesselJ(n, z) is given by Exp(-Abs(z.Imaginary)) * BesselJ(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The exponentially scaled Bessel function of the first kind.</returns>
        public static double ScaledBesselJ(double n, double z)
        {
            return Amos.ScaledCbesj(n, z);
        }

        /// <summary>
        /// Returns the Bessel function of the second kind.
        /// <para>BesselY(n, z) is a solution to the Bessel differential equation.</para>
        /// <para>BesselY(n, z, Scale.Exponential) returns Exp(-Abs(z.Imaginary)) * BesselY(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The Bessel function of the second kind.</returns>
        public static Complex BesselY(double n, Complex z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbesy(n, z) : Amos.Cbesy(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Bessel function of the second kind.
        /// <para>ScaledBesselY(n, z) is given by Exp(-Abs(z.Imaginary)) * Y(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The exponentially scaled Bessel function of the second kind.</returns>
        public static Complex ScaledBesselY(double n, Complex z)
        {
            return Amos.ScaledCbesy(n, z);
        }

        /// <summary>
        /// Returns the Bessel function of the second kind.
        /// <para>BesselY(n, z) is a solution to the Bessel differential equation.</para>
        /// <para>BesselY(n, z, Scale.Exponential) returns Exp(-Abs(z.Imaginary)) * BesselY(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The Bessel function of the second kind.</returns>
        public static double BesselY(double n, double z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbesy(n, z) : Amos.Cbesy(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Bessel function of the second kind.
        /// <para>ScaledBesselY(n, z) is given by Exp(-Abs(z.Imaginary)) * BesselY(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The exponentially scaled Bessel function of the second kind.</returns>
        public static double ScaledBesselY(double n, double z)
        {
            return Amos.ScaledCbesy(n, z);
        }

        /// <summary>
        /// Returns the modified Bessel function of the first kind.
        /// <para>BesselI(n, z) is a solution to the modified Bessel differential equation.</para>
        /// <para>BesselI(n, z, Scale.Exponential) returns Exp(-Abs(z.Real)) * BesselI(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The modified Bessel function of the first kind.</returns>
        public static Complex BesselI(double n, Complex z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbesi(n, z) : Amos.Cbesi(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled modified Bessel function of the first kind.
        /// <para>ScaledBesselI(n, z) is given by Exp(-Abs(z.Real)) * BesselI(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The exponentially scaled modified Bessel function of the first kind.</returns>
        public static Complex ScaledBesselI(double n, Complex z)
        {
            return Amos.ScaledCbesi(n, z);
        }

        /// <summary>
        /// Returns the modified Bessel function of the first kind.
        /// <para>BesselI(n, z) is a solution to the modified Bessel differential equation.</para>
        /// <para>BesselI(n, z, Scale.Exponential) returns Exp(-Abs(z.Real)) * BesselI(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The modified Bessel function of the first kind.</returns>
        public static double BesselI(double n, double z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbesi(n, z) : BesselI(n, new Complex(z, 0), scale).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled modified Bessel function of the first kind.
        /// <para>ScaledBesselI(n, z) is given by Exp(-Abs(z.Real)) * BesselI(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The exponentially scaled modified Bessel function of the first kind.</returns>
        public static double ScaledBesselI(double n, double z)
        {
            return Amos.ScaledCbesi(n, z);
        }

        /// <summary>
        /// Returns the modified Bessel function of the second kind.
        /// <para>BesselK(n, z) is a solution to the modified Bessel differential equation.</para>
        /// <para>BesselK(n, z, Scale.Exponential) returns Exp(z) * BesselK(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The modified Bessel function of the second kind.</returns>
        public static Complex BesselK(double n, Complex z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbesk(n, z) : Amos.Cbesk(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled modified Bessel function of the second kind.
        /// <para>ScaledBesselK(n, z) is given by Exp(z) * BesselK(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The exponentially scaled modified Bessel function of the second kind.</returns>
        public static Complex ScaledBesselK(double n, Complex z)
        {
            return Amos.ScaledCbesk(n, z);
        }

        /// <summary>
        /// Returns the modified Bessel function of the second kind.
        /// <para>BesselK(n, z) is a solution to the modified Bessel differential equation.</para>
        /// <para>BesselK(n, z, Scale.Exponential) returns Exp(z) * BesselK(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The modified Bessel function of the second kind.</returns>
        public static double BesselK(double n, double z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbesk(n, z) : Amos.Cbesk(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled modified Bessel function of the second kind.
        /// <para>ScaledBesselK(n, z) is given by Exp(z) * BesselK(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The exponentially scaled modified Bessel function of the second kind.</returns>
        public static double ScaledBesselK(double n, double z)
        {
            return Amos.ScaledCbesk(n, z);
        }
    }
}
