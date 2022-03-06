// <copyright file="Airy.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2022 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using Complex = System.Numerics.Complex;

// ReSharper disable once CheckNamespace
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
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The Airy function Ai.</returns>
        public static Complex AiryAi(Complex z)
        {
            return Amos.Cairy(z);
        }

        /// <summary>
        /// Returns the exponentially scaled Airy function Ai.
        /// <para>ScaledAiryAi(z) is given by Exp(zta) * AiryAi(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The exponentially scaled Airy function Ai.</returns>
        public static Complex AiryAiScaled(Complex z)
        {
            return Amos.ScaledCairy(z);
        }

        /// <summary>
        /// Returns the Airy function Ai.
        /// <para>AiryAi(z) is a solution to the Airy equation, y'' - y * z = 0.</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The Airy function Ai.</returns>
        public static double AiryAi(double z)
        {
            return AiryAi(new Complex(z, 0)).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled Airy function Ai.
        /// <para>ScaledAiryAi(z) is given by Exp(zta) * AiryAi(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The exponentially scaled Airy function Ai.</returns>
        public static double AiryAiScaled(double z)
        {
            return Amos.ScaledCairy(z);
        }

        /// <summary>
        /// Returns the derivative of the Airy function Ai.
        /// <para>AiryAiPrime(z) is defined as d/dz AiryAi(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The derivative of the Airy function Ai.</returns>
        public static Complex AiryAiPrime(Complex z)
        {
            return Amos.CairyPrime(z);
        }

        /// <summary>
        /// Returns the exponentially scaled derivative of Airy function Ai
        /// <para>ScaledAiryAiPrime(z) is given by Exp(zta) * AiryAiPrime(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The exponentially scaled derivative of Airy function Ai.</returns>
        public static Complex AiryAiPrimeScaled(Complex z)
        {
            return Amos.ScaledCairyPrime(z);
        }

        /// <summary>
        /// Returns the derivative of the Airy function Ai.
        /// <para>AiryAiPrime(z) is defined as d/dz AiryAi(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The derivative of the Airy function Ai.</returns>
        public static double AiryAiPrime(double z)
        {
            return AiryAiPrime(new Complex(z, 0)).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled derivative of the Airy function Ai.
        /// <para>ScaledAiryAiPrime(z) is given by Exp(zta) * AiryAiPrime(z), where zta = (2/3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The exponentially scaled derivative of the Airy function Ai.</returns>
        public static double AiryAiPrimeScaled(double z)
        {
            return Amos.ScaledCairyPrime(z);
        }

        /// <summary>
        /// Returns the Airy function Bi.
        /// <para>AiryBi(z) is a solution to the Airy equation, y'' - y * z = 0.</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The Airy function Bi.</returns>
        public static Complex AiryBi(Complex z)
        {
            return Amos.Cbiry(z);
        }

        /// <summary>
        /// Returns the exponentially scaled Airy function Bi.
        /// <para>ScaledAiryBi(z) is given by Exp(-Abs(zta.Real)) * AiryBi(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The exponentially scaled Airy function Bi(z).</returns>
        public static Complex AiryBiScaled(Complex z)
        {
            return Amos.ScaledCbiry(z);
        }

        /// <summary>
        /// Returns the Airy function Bi.
        /// <para>AiryBi(z) is a solution to the Airy equation, y'' - y * z = 0.</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The Airy function Bi.</returns>
        public static double AiryBi(double z)
        {
            return AiryBi(new Complex(z, 0)).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled Airy function Bi.
        /// <para>ScaledAiryBi(z) is given by Exp(-Abs(zta.Real)) * AiryBi(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the Airy function of.</param>
        /// <returns>The exponentially scaled Airy function Bi.</returns>
        public static double AiryBiScaled(double z)
        {
            return AiryBiScaled(new Complex(z, 0)).Real;
        }

        /// <summary>
        /// Returns the derivative of the Airy function Bi.
        /// <para>AiryBiPrime(z) is defined as d/dz AiryBi(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The derivative of the Airy function Bi.</returns>
        public static Complex AiryBiPrime(Complex z)
        {
            return Amos.CbiryPrime(z);
        }

        /// <summary>
        /// Returns the exponentially scaled derivative of the Airy function Bi.
        /// <para>ScaledAiryBiPrime(z) is given by Exp(-Abs(zta.Real)) * AiryBiPrime(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The exponentially scaled derivative of the Airy function Bi.</returns>
        public static Complex AiryBiPrimeScaled(Complex z)
        {
            return Amos.ScaledCbiryPrime(z);
        }

        /// <summary>
        /// Returns the derivative of the Airy function Bi.
        /// <para>AiryBiPrime(z) is defined as d/dz AiryBi(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The derivative of the Airy function Bi.</returns>
        public static double AiryBiPrime(double z)
        {
            return AiryBiPrime(new Complex(z, 0)).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled derivative of the Airy function Bi.
        /// <para>ScaledAiryBiPrime(z) is given by Exp(-Abs(zta.Real)) * AiryBiPrime(z) where zta = (2 / 3) * z * Sqrt(z).</para>
        /// </summary>
        /// <param name="z">The value to compute the derivative of the Airy function of.</param>
        /// <returns>The exponentially scaled derivative of the Airy function Bi.</returns>
        public static double AiryBiPrimeScaled(double z)
        {
            return AiryBiPrimeScaled(new Complex(z, 0)).Real;
        }
    }
}
