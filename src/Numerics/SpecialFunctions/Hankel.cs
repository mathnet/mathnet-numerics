using Complex = System.Numerics.Complex;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Hankel function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Hankel function of the first kind, H1(n, z).
        /// <p/>
        /// If expScaled is true, returns Exp(-z * j) * H1(n, z) where j = Sqrt(-1).
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Hankel function</param>
        /// <returns></returns>
        public static Complex HankelH1(double n, Complex z, bool expScaled = false)
        {
            return (expScaled) ? Amos.ScaledCbesh1(n, z) : Amos.Cbesh1(n, z);
        }

        /// <summary>
        /// Hankel function of the first kind, H1(n, z).
        /// <p/>
        /// If expScaled is true, returns Exp(-z * j) * H1(n, z) where j = Sqrt(-1).
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Hankel function</param>
        /// <returns></returns>
        public static double HankelH1(double n, double z, bool expScaled = false)
        {
            return HankelH1(n, new Complex(z, 0), expScaled).Real;
        }

        /// <summary>
        /// Hankel function of the second kind, H2(n, z).
        /// <p/>
        /// If expScaled is true, returns Exp(z * j) * H2(n, z) where j = Sqrt(-1).
        /// </summary>
        /// <param name="n">The order of the Hankel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Hankel function</param>
        /// <returns></returns>
        public static Complex HankelH2(double n, Complex z, bool expScaled = false)
        {
            return (expScaled) ? Amos.ScaledCbesh2(n, z) : Amos.Cbesh2(n, z);
        }

        /// <summary>
        /// Hankel function of the second kind, H2(n, z).
        /// <p/>
        /// If expScaled is true, returns Exp(z * j) * H2(n, z) where j = Sqrt(-1).
        /// </summary>
        /// <param name="n">The order of the Bessel function</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Hankel function</param>
        /// <returns></returns>
        public static double HankelH2(double n, double z, bool expScaled = false)
        {
            return HankelH2(n, new Complex(z, 0), expScaled).Real;
        }
    }
}
