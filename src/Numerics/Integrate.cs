// <copyright file="Integrate.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.Integration;

namespace MathNet.Numerics
{
    /// <summary>
    /// Numerical Integration (Quadrature).
    /// </summary>
    public static class Integrate
    {
        /// <summary>
        /// Approximation of the definite integral of an analytic smooth function on a closed interval.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="targetAbsoluteError">The expected relative accuracy of the approximation.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double OnClosedInterval(Func<double, double> f, double intervalBegin, double intervalEnd, double targetAbsoluteError)
        {
            return DoubleExponentialTransformation.Integrate(f, intervalBegin, intervalEnd, targetAbsoluteError);
        }

        /// <summary>
        /// Approximation of the definite integral of an analytic smooth function on a closed interval.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double OnClosedInterval(Func<double, double> f, double intervalBegin, double intervalEnd)
        {
            return DoubleExponentialTransformation.Integrate(f, intervalBegin, intervalEnd, 1e-8);
        }

        /// <summary>
        /// Approximates a 2-dimensional definite integral using an Nth order Gauss-Legendre rule over the rectangle [a,b] x [c,d].
        /// </summary>
        /// <param name="f">The 2-dimensional analytic smooth function to integrate.</param>
        /// <param name="invervalBeginA">Where the interval starts for the first (inside) integral, exclusive and finite.</param>
        /// <param name="invervalEndA">Where the interval ends for the first (inside) integral, exclusive and finite.</param>
        /// <param name="invervalBeginB">Where the interval starts for the second (outside) integral, exclusive and finite.</param>
        /// /// <param name="invervalEndB">Where the interval ends for the second (outside) integral, exclusive and finite.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calulcated on the fly.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double OnRectangle(Func<double, double, double> f, double invervalBeginA, double invervalEndA, double invervalBeginB, double invervalEndB, int order)
        {
            return GaussLegendreRule.Integrate(f, invervalBeginA, invervalEndA, invervalBeginB, invervalEndB, order);
        }

        /// <summary>
        /// Approximates a 2-dimensional definite integral using an Nth order Gauss-Legendre rule over the rectangle [a,b] x [c,d].
        /// </summary>
        /// <param name="f">The 2-dimensional analytic smooth function to integrate.</param>
        /// <param name="invervalBeginA">Where the interval starts for the first (inside) integral, exclusive and finite.</param>
        /// <param name="invervalEndA">Where the interval ends for the first (inside) integral, exclusive and finite.</param>
        /// <param name="invervalBeginB">Where the interval starts for the second (outside) integral, exclusive and finite.</param>
        /// /// <param name="invervalEndB">Where the interval ends for the second (outside) integral, exclusive and finite.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double OnRectangle(Func<double, double, double> f, double invervalBeginA, double invervalEndA, double invervalBeginB, double invervalEndB)
        {
            return GaussLegendreRule.Integrate(f, invervalBeginA, invervalEndA, invervalBeginB, invervalEndB, 32);
        }
    }
}
