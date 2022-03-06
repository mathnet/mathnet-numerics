// <copyright file="Bessel.cs" company="Math.NET">
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
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Bessel functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Returns the Bessel function of the first kind.
        /// <para>BesselJ(n, z) is a solution to the Bessel differential equation.</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The Bessel function of the first kind.</returns>
        public static Complex BesselJ(double n, Complex z)
        {
            return Amos.Cbesj(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Bessel function of the first kind.
        /// <para>ScaledBesselJ(n, z) is given by Exp(-Abs(z.Imaginary)) * BesselJ(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The exponentially scaled Bessel function of the first kind.</returns>
        public static Complex BesselJScaled(double n, Complex z)
        {
            return Amos.ScaledCbesj(n, z);
        }

        /// <summary>
        /// Returns the Bessel function of the first kind.
        /// <para>BesselJ(n, z) is a solution to the Bessel differential equation.</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The Bessel function of the first kind.</returns>
        public static double BesselJ(double n, double z)
        {
            return Amos.Cbesj(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Bessel function of the first kind.
        /// <para>ScaledBesselJ(n, z) is given by Exp(-Abs(z.Imaginary)) * BesselJ(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The exponentially scaled Bessel function of the first kind.</returns>
        public static double BesselJScaled(double n, double z)
        {
            return Amos.ScaledCbesj(n, z);
        }

        /// <summary>
        /// Returns the Bessel function of the second kind.
        /// <para>BesselY(n, z) is a solution to the Bessel differential equation.</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The Bessel function of the second kind.</returns>
        public static Complex BesselY(double n, Complex z)
        {
            return Amos.Cbesy(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Bessel function of the second kind.
        /// <para>ScaledBesselY(n, z) is given by Exp(-Abs(z.Imaginary)) * Y(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The exponentially scaled Bessel function of the second kind.</returns>
        public static Complex BesselYScaled(double n, Complex z)
        {
            return Amos.ScaledCbesy(n, z);
        }

        /// <summary>
        /// Returns the Bessel function of the second kind.
        /// <para>BesselY(n, z) is a solution to the Bessel differential equation.</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The Bessel function of the second kind.</returns>
        public static double BesselY(double n, double z)
        {
            return Amos.Cbesy(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled Bessel function of the second kind.
        /// <para>ScaledBesselY(n, z) is given by Exp(-Abs(z.Imaginary)) * BesselY(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the Bessel function.</param>
        /// <param name="z">The value to compute the Bessel function of.</param>
        /// <returns>The exponentially scaled Bessel function of the second kind.</returns>
        public static double BesselYScaled(double n, double z)
        {
            return Amos.ScaledCbesy(n, z);
        }

        /// <summary>
        /// Returns the modified Bessel function of the first kind.
        /// <para>BesselI(n, z) is a solution to the modified Bessel differential equation.</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The modified Bessel function of the first kind.</returns>
        public static Complex BesselI(double n, Complex z)
        {
            return Amos.Cbesi(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled modified Bessel function of the first kind.
        /// <para>ScaledBesselI(n, z) is given by Exp(-Abs(z.Real)) * BesselI(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The exponentially scaled modified Bessel function of the first kind.</returns>
        public static Complex BesselIScaled(double n, Complex z)
        {
            return Amos.ScaledCbesi(n, z);
        }

        /// <summary>
        /// Returns the modified Bessel function of the first kind.
        /// <para>BesselI(n, z) is a solution to the modified Bessel differential equation.</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The modified Bessel function of the first kind.</returns>
        public static double BesselI(double n, double z)
        {
            return BesselI(n, new Complex(z, 0)).Real;
        }

        /// <summary>
        /// Returns the exponentially scaled modified Bessel function of the first kind.
        /// <para>ScaledBesselI(n, z) is given by Exp(-Abs(z.Real)) * BesselI(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The exponentially scaled modified Bessel function of the first kind.</returns>
        public static double BesselIScaled(double n, double z)
        {
            return Amos.ScaledCbesi(n, z);
        }

        /// <summary>
        /// Returns the modified Bessel function of the second kind.
        /// <para>BesselK(n, z) is a solution to the modified Bessel differential equation.</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The modified Bessel function of the second kind.</returns>
        public static Complex BesselK(double n, Complex z)
        {
            return Amos.Cbesk(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled modified Bessel function of the second kind.
        /// <para>ScaledBesselK(n, z) is given by Exp(z) * BesselK(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The exponentially scaled modified Bessel function of the second kind.</returns>
        public static Complex BesselKScaled(double n, Complex z)
        {
            return Amos.ScaledCbesk(n, z);
        }

        /// <summary>
        /// Returns the modified Bessel function of the second kind.
        /// <para>BesselK(n, z) is a solution to the modified Bessel differential equation.</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The modified Bessel function of the second kind.</returns>
        public static double BesselK(double n, double z)
        {
            return Amos.Cbesk(n, z);
        }

        /// <summary>
        /// Returns the exponentially scaled modified Bessel function of the second kind.
        /// <para>ScaledBesselK(n, z) is given by Exp(z) * BesselK(n, z).</para>
        /// </summary>
        /// <param name="n">The order of the modified Bessel function.</param>
        /// <param name="z">The value to compute the modified Bessel function of.</param>
        /// <returns>The exponentially scaled modified Bessel function of the second kind.</returns>
        public static double BesselKScaled(double n, double z)
        {
            return Amos.ScaledCbesk(n, z);
        }
    }
}
