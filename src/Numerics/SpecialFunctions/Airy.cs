using Complex = System.Numerics.Complex;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Airy functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Airy function Ai(z).
        /// <p/>
        /// If expScaled is true, returns Exp(zta) * Ai(z), where zta = (2/3) * z * Sqrt(z).
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Airy function</param>
        /// <returns></returns>
        public static Complex AiryAi(Complex z, bool expScaled = false)
        {
            return (expScaled) ? Amos.ScaledCairy(z) : Amos.Cairy(z);
        }

        /// <summary>
        /// Airy function Ai(z).
        /// <p/>
        /// If expScaled is true, returns Exp(zta) * Ai(z), where zta = (2/3) * z * Sqrt(z).
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Airy function</param>
        /// <returns></returns>
        public static double AiryAi(double z, bool expScaled = false)
        {
            if (expScaled)
            {
                return Amos.ScaledCairy(z);
            }
            else
            {
                return AiryAi(new Complex(z, 0), expScaled).Real;
            }
        }

        /// <summary>
        /// Derivative of the Airy function Ai.
        /// <p/>
        /// If expScaled is true, returns Exp(zta) * d/dz Ai(z), where zta = (2/3) * z * Sqrt(z).
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Airy function</param>
        /// <returns></returns>
        public static Complex AiryAiPrime(Complex z, bool expScaled = false)
        {
            return (expScaled) ? Amos.ScaledCairyPrime(z) : Amos.CairyPrime(z);
        }

        /// <summary>
        /// Derivative of the Airy function Ai.
        /// <p/>
        /// If expScaled is true, returns Exp(zta) * d/dz Ai(z), where zta = (2/3) * z * Sqrt(z).
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Airy function</param>
        /// <returns></returns>
        public static double AiryAiPrime(double z, bool expScaled = false)
        {
            if (expScaled)
            {
                return Amos.ScaledCairyPrime(z);
            }
            else
            {
                return AiryAiPrime(new Complex(z, 0), expScaled).Real;
            }
        }

        /// <summary>
        /// Airy function Bi(z).
        /// <p/>
        /// If expScaled is true, returns Exp(-axzta) * Bi(z) where zta = (2 / 3) * z * Sqrt(z) and axzta = Abs(zta.Real).
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Airy function</param>
        /// <returns></returns>
        public static Complex AiryBi(Complex z, bool expScaled = false)
        {
            return (expScaled) ? Amos.ScaledCbiry(z) : Amos.Cbiry(z);
        }

        /// <summary>
        /// Airy function Bi(x).
        /// <p/>
        /// If expScaled is true, returns Exp(-axzta) * Bi(z) where zta = (2 / 3) * z * Sqrt(z) and axzta = Abs(zta.Real).
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Airy function</param>
        /// <returns></returns>
        public static double AiryBi(double z, bool expScaled = false)
        {
            return AiryBi(new Complex(z, 0), expScaled).Real;
        }

        /// <summary>
        /// Derivative of the Airy function Bi(z).
        /// <p/>
        /// If expScaled is true, returns Exp(-axzta) * d/dz Bi(z) where zta = (2 / 3) * z * Sqrt(z) and axzta = Abs(zta.Real).
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Airy function</param>
        /// <returns></returns>
        public static Complex AiryBiPrime(Complex z, bool expScaled = false)
        {
            return (expScaled) ? Amos.ScaledCbiryPrime(z) : Amos.CbiryPrime(z);
        }

        /// <summary>
        /// Derivative of the Airy function Bi(z).
        /// <p/>
        /// If expScaled is true, returns Exp(-axzta) * d/dz Bi(z) where zta = (2 / 3) * z * Sqrt(z) and axzta = Abs(zta.Real).
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <param name="expScaled">If true, returns exponentially-scaled Airy function</param>
        /// <returns></returns>
        public static double AiryBiPrime(double z, bool expScaled = false)
        {
            return AiryBiPrime(new Complex(z, 0), expScaled).Real;
        }
    }
}
