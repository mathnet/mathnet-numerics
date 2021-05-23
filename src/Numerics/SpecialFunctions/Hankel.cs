using Complex = System.Numerics.Complex;

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Hankel function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Returns the Hankel function of the first kind.
        /// <para>HankelH1(n, z) is defined as BesselJ(n, z) + j * BesselY(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Hankel function.</param>
        /// <param name="z">The value to compute the Hankel function of.</param>
        /// <returns>The Hankel function of the first kind.</returns>
        public static Complex HankelH1(double n, Complex z)
        {
            return Amos.Cbesh1(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Hankel function of the first kind.
        /// <para>ScaledHankelH1(n, z) is given by Exp(-z * j) * HankelH1(n, z) where j = Sqrt(-1).</para>
        /// </summary>
        /// <param name="n">The order of the Hankel function.</param>
        /// <param name="z">The value to compute the Hankel function of.</param>
        /// <returns>The exponentially scaled Hankel function of the first kind.</returns>
        public static Complex HankelH1Scaled(double n, Complex z)
        {
            return Amos.ScaledCbesh1(n, z);
        }

        /// <summary>
        /// Returns the Hankel function of the second kind.
        /// <para>HankelH2(n, z) is defined as BesselJ(n, z) - j * BesselY(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Hankel function.</param>
        /// <param name="z">The value to compute the Hankel function of.</param>
        /// <returns>The Hankel function of the second kind.</returns>
        public static Complex HankelH2(double n, Complex z)
        {
            return Amos.Cbesh2(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Hankel function of the second kind.
        /// <para>ScaledHankelH2(n, z) is given by Exp(z * j) * HankelH2(n, z) where j = Sqrt(-1).</para>
        /// </summary>
        /// <param name="n">The order of the Hankel function.</param>
        /// <param name="z">The value to compute the Hankel function of.</param>
        /// <returns>The exponentially scaled Hankel function of the second kind.</returns>
        public static Complex HankelH2Scaled(double n, Complex z)
        {
            return Amos.ScaledCbesh2(n, z);
        }
    }
}
