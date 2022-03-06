// <copyright file="Hankel.cs" company="Math.NET">
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
