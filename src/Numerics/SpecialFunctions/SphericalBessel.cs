// <copyright file="SphericalBessel.cs" company="Math.NET">
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

using System;
using Complex = System.Numerics.Complex;

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the spherical Bessel functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Returns the spherical Bessel function of the first kind.
        /// <para>SphericalBesselJ(n, z) is given by Sqrt(pi/2) / Sqrt(z) * BesselJ(n + 1/2, z).</para>
        /// </summary>
        /// <param name="n">The order of the spherical Bessel function.</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <returns>The spherical Bessel function of the first kind.</returns>
        public static Complex SphericalBesselJ(double n, Complex z)
        {
            if (double.IsNaN(n) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
            {
                return new Complex(double.NaN, double.NaN);
            }

            if (double.IsInfinity(z.Real))
            {
                return (z.Imaginary == 0) ? Complex.Zero : new Complex(double.PositiveInfinity, double.PositiveInfinity);
            }

            if (z.Real == 0 && z.Imaginary == 0)
            {
                return (n == 0) ? 1 : 0;
            }

            return Constants.SqrtPiOver2 * BesselJ(n + 0.5, z) / Complex.Sqrt(z);
        }

        /// <summary>
        /// Returns the spherical Bessel function of the first kind.
        /// <para>SphericalBesselJ(n, z) is given by Sqrt(pi/2) / Sqrt(z) * BesselJ(n + 1/2, z).</para>
        /// </summary>
        /// <param name="n">The order of the spherical Bessel function.</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <returns>The spherical Bessel function of the first kind.</returns>
        public static double SphericalBesselJ(double n, double z)
        {
            if (double.IsNaN(n) || double.IsNaN(z))
            {
                return double.NaN;
            }

            if (n < 0)
            {
                return double.NaN;
            }

            if (double.IsInfinity(z))
            {
                return 0;
            }

            if (z == 0)
            {
                return (n == 0) ? 1 : 0;
            }

            return Constants.SqrtPiOver2 * BesselJ(n + 0.5, z) / Math.Sqrt(z);
        }

        /// <summary>
        /// Returns the spherical Bessel function of the second kind.
        /// <para>SphericalBesselY(n, z) is given by Sqrt(pi/2) / Sqrt(z) * BesselY(n + 1/2, z).</para>
        /// </summary>
        /// <param name="n">The order of the spherical Bessel function.</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <returns>The spherical Bessel function of the second kind.</returns>
        public static Complex SphericalBesselY(double n, Complex z)
        {
            if (double.IsNaN(n) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
            {
                return new Complex(double.NaN, double.NaN);
            }

            if (double.IsInfinity(z.Real))
            {
                return (z.Imaginary == 0) ? Complex.Zero : new Complex(double.PositiveInfinity, double.PositiveInfinity);
            }

            if (z.Real == 0 && z.Imaginary == 0)
            {
                return new Complex(double.NaN, double.NaN);
            }

            return Constants.SqrtPiOver2 * BesselY(n + 0.5, z) / Complex.Sqrt(z);
        }

        /// <summary>
        /// Returns the spherical Bessel function of the second kind.
        /// <para>SphericalBesselY(n, z) is given by Sqrt(pi/2) / Sqrt(z) * BesselY(n + 1/2, z).</para>
        /// </summary>
        /// <param name="n">The order of the spherical Bessel function.</param>
        /// <param name="z">The value to compute the spherical Bessel function of.</param>
        /// <returns>The spherical Bessel function of the second kind.</returns>
        public static double SphericalBesselY(double n, double z)
        {
            if (double.IsNaN(n) || double.IsNaN(z))
            {
                return double.NaN;
            }

            if (n < 0)
            {
                return double.NaN;
            }

            if (double.IsInfinity(z))
            {
                return 0;
            }

            if (z == 0)
            {
                return double.NegativeInfinity;
            }

            return Constants.SqrtPiOver2 * BesselY(n + 0.5, z) / Math.Sqrt(z);
        }
    }
}
