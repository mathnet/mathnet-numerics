using System.Numerics;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Airy functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Returns the Airy function Ai.
        /// <para>AiryAi(z) is a solution to the Airy equation, y'' - y * z = 0.</para>
        /// <para>AiryAi(z, Scale.Exponential) returns Exp(zta) * AiryAi(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The Airy function Ai.</returns>
        public static Complex AiryAi(Complex z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCairy(z) : Amos.Cairy(z);
        }

        /// <summary>
        /// Returns the exponentially scaled Airy function Ai.
        /// <para>ScaledAiryAi(z) is given by Exp(zta) * AiryAi(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The exponentially scaled Airy function Ai.</returns>
        public static Complex ScaledAiryAi(Complex z)
        {
            return Amos.ScaledCairy(z);
        }

        /// <summary>
        /// Returns the Airy function Ai.
        /// <para>AiryAi(z) is a solution to the Airy equation, y'' - y * z = 0.</para>
        /// <para>AiryAi(z, Scale.Exponential) returns Exp(zta) * AiryAi(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The Airy function Ai.</returns>
        public static double AiryAi(double z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCairy(z) : AiryAi(new Complex(z, 0), scale).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled Airy function Ai.
        /// <para>ScaledAiryAi(z) is given by Exp(zta) * AiryAi(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The exponentially scaled Airy function Ai.</returns>
        public static double ScaledAiryAi(double z)
        {
            return Amos.ScaledCairy(z);
        }

        /// <summary>
        /// Returns the derivative of the Airy function Ai.
        /// <para>AiryAiPrime(z) is defined as d/dz AiryAi(z).</para>
        /// <para>AiryAiPrime(z, Scale.Exponential) returns Exp(zta) * AiryAiPrime(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The derivative of the Airy function Ai.</returns>
        public static Complex AiryAiPrime(Complex z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCairyPrime(z) : Amos.CairyPrime(z);
        }

        /// <summary>
        /// Returns the exponentially scaled derivative of Airy function Ai
        /// <para>ScaledAiryAiPrime(z) is given by Exp(zta) * AiryAiPrime(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The exponentially scaled derivative of Airy function Ai.</returns>
        public static Complex ScaledAiryAiPrime(Complex z)
        {
            return Amos.ScaledCairyPrime(z);
        }

        /// <summary>
        /// Returns the derivative of the Airy function Ai.
        /// <para>AiryAiPrime(z) is defined as d/dz AiryAi(z).</para>
        /// <para>AiryAiPrime(z, Scale.Exponential) returns Exp(zta) * AiryAiPrime(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The derivative of the Airy function Ai.</returns>
        public static double AiryAiPrime(double z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCairyPrime(z) : AiryAiPrime(new Complex(z, 0), scale).Real;
        }

        /// <summary>
        /// Returns the expoenntially scaled derivative of the Airy function Ai.
        /// <para>ScaledAiryAiPrime(z) is given by Exp(zta) * AiryAiPrime(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The expoenntially scaled derivative of the Airy function Ai.</returns>
        public static double ScaledAiryAiPrime(double z)
        {
            return Amos.ScaledCairyPrime(z);
        }

        /// <summary>
        /// Returns the Airy function Bi.
        /// <para>AiryBi(z) is a solution to the Airy equation, y'' - y * z = 0.</para>
        /// <para>AiryBi(z, Scale.Exponential) returns Exp(-Abs(zta.Real)) * AiryBi(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The Airy function Bi.</returns>
        public static Complex AiryBi(Complex z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbiry(z) : Amos.Cbiry(z);
        }

        /// <summary>
        /// Returns the exponentially scaled Airy function Bi.
        /// <para>ScaledAiryBi(z) is given by Exp(-Abs(zta.Real)) * AiryBi(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The exponentially scaled Airy function Bi(z).</returns>
        public static Complex ScaledAiryBi(Complex z)
        {
            return Amos.ScaledCbiry(z);
        }

        /// <summary>
        /// Returns the Airy function Bi.
        /// <para>AiryBi(z) is a solution to the Airy equation, y'' - y * z = 0.</para>
        /// <para>AiryBi(z, Scale.Exponential) returns Exp(-Abs(zta.Real)) * AiryBi(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The Airy function Bi.</returns>
        public static double AiryBi(double z, Scale scale = Scale.Unity)
        {
            return AiryBi(new Complex(z, 0), scale).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled Airy function Bi.
        /// <para>ScaledAiryBi(z) is given by Exp(-Abs(zta.Real)) * AiryBi(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The exponentially scaled Airy function Bi.</returns>
        public static double ScaledAiryBi(double z)
        {
            return AiryBi(new Complex(z, 0), Scale.Exponential).Real;
        }

        /// <summary>
        /// Returns the derivative of the Airy function Bi.
        /// <para>AiryBiPrime(z) is defined as d/dz AiryBi(z).</para>
        /// <para>AiryBiPrime(z, Scale.Exponential) returns Exp(-Abs(zta.Real)) * AiryBiPrime(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The derivative of the Airy function Bi.</returns>
        public static Complex AiryBiPrime(Complex z, Scale scale = Scale.Unity)
        {
            return (scale == Scale.Exponential) ? Amos.ScaledCbiryPrime(z) : Amos.CbiryPrime(z);
        }

        /// <summary>
        /// Returns the exponentially scaled derivative of the Airy function Bi.
        /// <para>ScaledAiryBiPrime(z) is given by Exp(-Abs(zta.Real)) * AiryBiPrime(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The exponentially scaled derivative of the Airy function Bi.</returns>
        public static Complex ScaledAiryBiPrime(Complex z)
        {
            return Amos.ScaledCbiryPrime(z);
        }

        /// <summary>
        /// Returns the derivative of the Airy function Bi.
        /// <para>AiryBiPrime(z) is defined as d/dz AiryBi(z).</para>
        /// <para>AiryBiPrime(z, Scale.Exponential) returns Exp(-Abs(zta.Real)) * AiryBiPrime(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <param name="scale">The option to set the scaling factor.</param>
        /// <returns>The derivative of the Airy function Bi.</returns>
        public static double AiryBiPrime(double z, Scale scale = Scale.Unity)
        {
            return AiryBiPrime(new Complex(z, 0), scale).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled derivative of the Airy function Bi.
        /// <para>ScaledAiryBiPrime(z) is given by Exp(-Abs(zta.Real)) * AiryBiPrime(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The exponentially scaled derivative of the Airy function Bi.</returns>
        public static double ScaledAiryBiPrime(double z)
        {
            return AiryBiPrime(new Complex(z, 0), Scale.Exponential).Real;
        }
    }
}
