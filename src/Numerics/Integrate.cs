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
using System.Numerics;
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
        /// <param name="invervalEndB">Where the interval ends for the second (outside) integral, exclusive and finite.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calculated on the fly.</param>
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
        /// <param name="invervalEndB">Where the interval ends for the second (outside) integral, exclusive and finite.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double OnRectangle(Func<double, double, double> f, double invervalBeginA, double invervalEndA, double invervalBeginB, double invervalEndB)
        {
            return GaussLegendreRule.Integrate(f, invervalBeginA, invervalEndA, invervalBeginB, invervalEndB, 32);
        }

        /// <summary>
        /// Approximates a 3-dimensional definite integral using an Nth order Gauss-Legendre rule over the cuboid or rectangular prism [a1,a2] x [b1,b2] x [c1,c2].
        /// </summary>
        /// <param name="f">The 3-dimensional analytic smooth function to integrate.</param>
        /// <param name="invervalBeginA">Where the interval starts for the first integral, exclusive and finite.</param>
        /// <param name="invervalEndA">Where the interval ends for the first integral, exclusive and finite.</param>
        /// <param name="invervalBeginB">Where the interval starts for the second integral, exclusive and finite.</param>
        /// <param name="invervalEndB">Where the interval ends for the second integral, exclusive and finite.</param>
        /// <param name="invervalBeginC">Where the interval starts for the third integral, exclusive and finite.</param>
        /// <param name="invervalEndC">Where the interval ends for the third integral, exclusive and finite.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calculated on the fly.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double OnCuboid(Func<double, double, double, double> f, double invervalBeginA, double invervalEndA, double invervalBeginB, double invervalEndB, double invervalBeginC, double invervalEndC, int order = 32)
        {
            return GaussLegendreRule.Integrate(f, invervalBeginA, invervalEndA, invervalBeginB, invervalEndB, invervalBeginC, invervalEndC, order);
        }

        /// <summary>
        /// Approximation of the definite integral of an analytic smooth function by double-exponential quadrature. When either or both limits are infinite, the integrand is assumed rapidly decayed to zero as x -> infinity.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts.</param>
        /// <param name="intervalEnd">Where the interval stops.</param>
        /// <param name="targetAbsoluteError">The expected relative accuracy of the approximation.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double DoubleExponential(Func<double, double> f, double intervalBegin, double intervalEnd, double targetAbsoluteError = 1E-8)
        {
            // Reference:
            // Formula used for variable subsitution from
            // 1. Shampine, L. F. (2008). Vectorized adaptive quadrature in MATLAB. Journal of Computational and Applied Mathematics, 211(2), 131-140.
            // 2. quadgk.m, GNU Octave

            if (intervalBegin > intervalEnd)
            {
                return -DoubleExponential(f, intervalEnd, intervalBegin, targetAbsoluteError);
            }

            // (-oo, oo) => [-1, 1]
            //
            // integral_(-oo)^(oo) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = t / (1 - t^2)
            // g'(t) = (1 + t^2) / (1 - t^2)^2
            if (double.IsInfinity(intervalBegin) && double.IsInfinity(intervalEnd))
            {
                Func<double, double> u = (t) =>
                {
                    return f(t / (1 - t * t)) * (1 + t * t) / ((1 - t * t) * (1 - t * t));
                };
                return DoubleExponentialTransformation.Integrate(u, -1, 1, targetAbsoluteError);
            }
            // [a, oo) => [0, 1]
            //
            // integral_(a)^(oo) f(x) dx = integral_(0)^(oo) f(a + t^2) 2 t dt
            //                           = integral_(0)^(1) f(a + g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 - s)
            // g'(s) = 1 / (1 - s)^2
            else if (double.IsInfinity(intervalEnd))
            {
                Func<double, double> u = (s) =>
                {
                    return 2 * s * f(intervalBegin + (s / (1 - s)) * (s / (1 - s))) / ((1 - s) * (1 - s) * (1 - s));
                };
                return DoubleExponentialTransformation.Integrate(u, 0, 1, targetAbsoluteError);
            }
            // (-oo, b] => [-1, 0]
            //
            // integral_(-oo)^(b) f(x) dx = -integral_(-oo)^(0) f(b - t^2) 2 t dt
            //                            = -integral_(-1)^(0) f(b - g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 + s)
            // g'(s) = 1 / (1 + s)^2
            else if (double.IsInfinity(intervalBegin))
            {
                Func<double, double> u = (s) =>
                {
                    return -2 * s * f(intervalEnd - s / (1 + s) * (s / (1 + s))) / ((1 + s) * (1 + s) * (1 + s));
                };
                return DoubleExponentialTransformation.Integrate(u, -1, 0, targetAbsoluteError);
            }
            else
            {
                return DoubleExponentialTransformation.Integrate(f, intervalBegin, intervalEnd, targetAbsoluteError);
            }
        }

        /// <summary>
        /// Approximation of the definite integral of an analytic smooth function by Gauss-Legendre quadrature. When either or both limits are infinite, the integrand is assumed rapidly decayed to zero as x -> infinity.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts.</param>
        /// <param name="intervalEnd">Where the interval stops.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calculated on the fly.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double GaussLegendre(Func<double, double> f, double intervalBegin, double intervalEnd, int order = 128)
        {
            // Reference:
            // Formula used for variable subsitution from
            // 1. Shampine, L. F. (2008). Vectorized adaptive quadrature in MATLAB. Journal of Computational and Applied Mathematics, 211(2), 131-140.
            // 2. quadgk.m, GNU Octave

            if (intervalBegin > intervalEnd)
            {
                return -GaussLegendre(f, intervalEnd, intervalBegin, order);
            }

            // (-oo, oo) => [-1, 1]
            //
            // integral_(-oo)^(oo) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = t / (1 - t^2)
            // g'(t) = (1 + t^2) / (1 - t^2)^2
            if (double.IsInfinity(intervalBegin) && double.IsInfinity(intervalEnd))
            {
                Func<double, double> u = (t) =>
                {
                    return f(t / (1 - t * t)) * (1 + t * t) / ((1 - t * t) * (1 - t * t));
                };
                return GaussLegendreRule.Integrate(u, -1, 1, order);
            }
            // [a, oo) => [0, 1]
            //
            // integral_(a)^(oo) f(x) dx = integral_(0)^(oo) f(a + t^2) 2 t dt
            //                           = integral_(0)^(1) f(a + g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 - s)
            // g'(s) = 1 / (1 - s)^2
            else if (double.IsInfinity(intervalEnd))
            {
                Func<double, double> u = (s) =>
                {
                    return 2 * s * f(intervalBegin + (s / (1 - s)) * (s / (1 - s))) / ((1 - s) * (1 - s) * (1 - s));
                };
                return GaussLegendreRule.Integrate(u, 0, 1, order);
            }
            // (-oo, b] => [-1, 0]
            //
            // integral_(-oo)^(b) f(x) dx = -integral_(-oo)^(0) f(b - t^2) 2 t dt
            //                            = -integral_(-1)^(0) f(b - g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 + s)
            // g'(s) = 1 / (1 + s)^2
            else if (double.IsInfinity(intervalBegin))
            {
                Func<double, double> u = (s) =>
                {
                    return -2 * s * f(intervalEnd - s / (1 + s) * (s / (1 + s))) / ((1 + s) * (1 + s) * (1 + s));
                };
                return GaussLegendreRule.Integrate(u, -1, 0, order);
            }
            // [a, b] => [-1, 1]
            //
            // integral_(a)^(b) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = (b - a) * t * (3 - t^2) / 4 + (b + a) / 2
            // g'(t) = 3 / 4 * (b - a) * (1 - t^2)
            else
            {
                Func<double, double> u = (t) =>
                {
                    return f((intervalEnd - intervalBegin) / 4 * t * (3 - t * t) + (intervalEnd + intervalBegin) / 2) * 3 * (intervalEnd - intervalBegin) / 4 * (1 - t * t);
                };
                return GaussLegendreRule.Integrate(u, -1, 1, order);
            }
        }

        /// <summary>
        /// Approximation of the definite integral of an analytic smooth function by Gauss-Kronrod quadrature. When either or both limits are infinite, the integrand is assumed rapidly decayed to zero as x -> infinity.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts.</param>
        /// <param name="intervalEnd">Where the interval stops.</param>
        /// <param name="targetRelativeError">The expected relative accuracy of the approximation.</param>
        /// <param name="maximumDepth">The maximum number of interval splittings permitted before stopping.</param>
        /// <param name="order">The number of Gauss-Kronrod points. Pre-computed for 15, 21, 31, 41, 51 and 61 points.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double GaussKronrod(Func<double, double> f, double intervalBegin, double intervalEnd, double targetRelativeError = 1E-8, int maximumDepth = 15, int order = 15)
        {
            return GaussKronrodRule.Integrate(f, intervalBegin, intervalEnd, out _, out _, targetRelativeError: targetRelativeError, maximumDepth: maximumDepth, order: order);
        }

        /// <summary>
        /// Approximation of the definite integral of an analytic smooth function by Gauss-Kronrod quadrature. When either or both limits are infinite, the integrand is assumed rapidly decayed to zero as x -> infinity.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts.</param>
        /// <param name="intervalEnd">Where the interval stops.</param>
        /// <param name="error">The difference between the (N-1)/2 point Gauss approximation and the N-point Gauss-Kronrod approximation</param>
        /// <param name="L1Norm">The L1 norm of the result, if there is a significant difference between this and the returned value, then the result is likely to be ill-conditioned.</param>
        /// <param name="targetRelativeError">The expected relative accuracy of the approximation.</param>
        /// <param name="maximumDepth">The maximum number of interval splittings permitted before stopping</param>
        /// <param name="order">The number of Gauss-Kronrod points. Pre-computed for 15, 21, 31, 41, 51 and 61 points</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double GaussKronrod(Func<double, double> f, double intervalBegin, double intervalEnd, out double error, out double L1Norm, double targetRelativeError = 1E-8, int maximumDepth = 15, int order = 15)
        {
            return GaussKronrodRule.Integrate(f, intervalBegin, intervalEnd, out error, out L1Norm, targetRelativeError: targetRelativeError, maximumDepth: maximumDepth, order: order);
        }
    }

    /// <summary>
    /// Numerical Contour Integration of a complex-valued function over a real variable,.
    /// </summary>
    public static class ContourIntegrate
    {
        /// <summary>
        /// Approximation of the definite integral of an analytic smooth complex function by double-exponential quadrature. When either or both limits are infinite, the integrand is assumed rapidly decayed to zero as x -> infinity.
        /// </summary>
        /// <param name="f">The analytic smooth complex function to integrate, defined on the real domain.</param>
        /// <param name="intervalBegin">Where the interval starts.</param>
        /// <param name="intervalEnd">Where the interval stops.</param>
        /// <param name="targetAbsoluteError">The expected relative accuracy of the approximation.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static Complex DoubleExponential(Func<double, Complex> f, double intervalBegin, double intervalEnd, double targetAbsoluteError = 1E-8)
        {
            // Reference:
            // Formula used for variable subsitution from
            // 1. Shampine, L. F. (2008). Vectorized adaptive quadrature in MATLAB. Journal of Computational and Applied Mathematics, 211(2), 131-140.
            // 2. quadgk.m, GNU Octave

            if (intervalBegin > intervalEnd)
            {
                return -DoubleExponential(f, intervalEnd, intervalBegin, targetAbsoluteError);
            }

            // (-oo, oo) => [-1, 1]
            //
            // integral_(-oo)^(oo) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = t / (1 - t^2)
            // g'(t) = (1 + t^2) / (1 - t^2)^2
            if (double.IsInfinity(intervalBegin) && double.IsInfinity(intervalEnd))
            {
                Func<double, Complex> u = (t) =>
                {
                    return f(t / (1 - t * t)) * (1 + t * t) / ((1 - t * t) * (1 - t * t));
                };
                return DoubleExponentialTransformation.ContourIntegrate(u, -1, 1, targetAbsoluteError);
            }
            // [a, oo) => [0, 1]
            //
            // integral_(a)^(oo) f(x) dx = integral_(0)^(oo) f(a + t^2) 2 t dt
            //                           = integral_(0)^(1) f(a + g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 - s)
            // g'(s) = 1 / (1 - s)^2
            else if (double.IsInfinity(intervalEnd))
            {
                Func<double, Complex> u = (s) =>
                {
                    return 2 * s * f(intervalBegin + (s / (1 - s)) * (s / (1 - s))) / ((1 - s) * (1 - s) * (1 - s));
                };
                return DoubleExponentialTransformation.ContourIntegrate(u, 0, 1, targetAbsoluteError);
            }
            // (-oo, b] => [-1, 0]
            //
            // integral_(-oo)^(b) f(x) dx = -integral_(-oo)^(0) f(b - t^2) 2 t dt
            //                            = -integral_(-1)^(0) f(b - g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 + s)
            // g'(s) = 1 / (1 + s)^2
            else if (double.IsInfinity(intervalBegin))
            {
                Func<double, Complex> u = (s) =>
                {
                    return -2 * s * f(intervalEnd - s / (1 + s) * (s / (1 + s))) / ((1 + s) * (1 + s) * (1 + s));
                };
                return DoubleExponentialTransformation.ContourIntegrate(u, -1, 0, targetAbsoluteError);
            }
            else
            {
                return DoubleExponentialTransformation.ContourIntegrate(f, intervalBegin, intervalEnd, targetAbsoluteError);
            }
        }

        /// <summary>
        /// Approximation of the definite integral of an analytic smooth complex function by double-exponential quadrature. When either or both limits are infinite, the integrand is assumed rapidly decayed to zero as x -> infinity.
        /// </summary>
        /// <param name="f">The analytic smooth complex function to integrate, defined on the real domain.</param>
        /// <param name="intervalBegin">Where the interval starts.</param>
        /// <param name="intervalEnd">Where the interval stops.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calculated on the fly.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static Complex GaussLegendre(Func<double, Complex> f, double intervalBegin, double intervalEnd, int order = 128)
        {
            // Reference:
            // Formula used for variable subsitution from
            // 1. Shampine, L. F. (2008). Vectorized adaptive quadrature in MATLAB. Journal of Computational and Applied Mathematics, 211(2), 131-140.
            // 2. quadgk.m, GNU Octave

            if (intervalBegin > intervalEnd)
            {
                return -GaussLegendre(f, intervalEnd, intervalBegin, order);
            }

            // (-oo, oo) => [-1, 1]
            //
            // integral_(-oo)^(oo) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = t / (1 - t^2)
            // g'(t) = (1 + t^2) / (1 - t^2)^2
            if (double.IsInfinity(intervalBegin) && double.IsInfinity(intervalEnd))
            {
                Func<double, Complex> u = (t) =>
                {
                    return f(t / (1 - t * t)) * (1 + t * t) / ((1 - t * t) * (1 - t * t));
                };
                return GaussLegendreRule.ContourIntegrate(u, -1, 1, order);
            }
            // [a, oo) => [0, 1]
            //
            // integral_(a)^(oo) f(x) dx = integral_(0)^(oo) f(a + t^2) 2 t dt
            //                           = integral_(0)^(1) f(a + g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 - s)
            // g'(s) = 1 / (1 - s)^2
            else if (double.IsInfinity(intervalEnd))
            {
                Func<double, Complex> u = (s) =>
                {
                    return 2 * s * f(intervalBegin + (s / (1 - s)) * (s / (1 - s))) / ((1 - s) * (1 - s) * (1 - s));
                };
                return GaussLegendreRule.ContourIntegrate(u, 0, 1, order);
            }
            // (-oo, b] => [-1, 0]
            //
            // integral_(-oo)^(b) f(x) dx = -integral_(-oo)^(0) f(b - t^2) 2 t dt
            //                            = -integral_(-1)^(0) f(b - g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 + s)
            // g'(s) = 1 / (1 + s)^2
            else if (double.IsInfinity(intervalBegin))
            {
                Func<double, Complex> u = (s) =>
                {
                    return -2 * s * f(intervalEnd - s / (1 + s) * (s / (1 + s))) / ((1 + s) * (1 + s) * (1 + s));
                };
                return GaussLegendreRule.ContourIntegrate(u, -1, 0, order);
            }
            // [a, b] => [-1, 1]
            //
            // integral_(a)^(b) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = (b - a) * t * (3 - t^2) / 4 + (b + a) / 2
            // g'(t) = 3 / 4 * (b - a) * (1 - t^2)
            else
            {
                Func<double, Complex> u = (t) =>
                {
                    return f((intervalEnd - intervalBegin) / 4 * t * (3 - t * t) + (intervalEnd + intervalBegin) / 2) * 3 * (intervalEnd - intervalBegin) / 4 * (1 - t * t);
                };
                return GaussLegendreRule.ContourIntegrate(u, -1, 1, order);
            }
        }

        /// <summary>
        /// Approximation of the definite integral of an analytic smooth function by Gauss-Kronrod quadrature. When either or both limits are infinite, the integrand is assumed rapidly decayed to zero as x -> infinity.
        /// </summary>
        /// <param name="f">The analytic smooth complex function to integrate, defined on the real domain.</param>
        /// <param name="intervalBegin">Where the interval starts.</param>
        /// <param name="intervalEnd">Where the interval stops.</param>
        /// <param name="targetRelativeError">The expected relative accuracy of the approximation.</param>
        /// <param name="maximumDepth">The maximum number of interval splittings permitted before stopping</param>
        /// <param name="order">The number of Gauss-Kronrod points. Pre-computed for 15, 21, 31, 41, 51 and 61 points</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static Complex GaussKronrod(Func<double, Complex> f, double intervalBegin, double intervalEnd, double targetRelativeError = 1E-8, int maximumDepth = 15, int order = 15)
        {
            return GaussKronrodRule.ContourIntegrate(f, intervalBegin, intervalEnd, out _, out _, targetRelativeError: targetRelativeError, maximumDepth: maximumDepth, order: order);
        }

        /// <summary>
        /// Approximation of the definite integral of an analytic smooth function by Gauss-Kronrod quadrature. When either or both limits are infinite, the integrand is assumed rapidly decayed to zero as x -> infinity.
        /// </summary>
        /// <param name="f">The analytic smooth complex function to integrate, defined on the real domain.</param>
        /// <param name="intervalBegin">Where the interval starts.</param>
        /// <param name="intervalEnd">Where the interval stops.</param>
        /// <param name="error">The difference between the (N-1)/2 point Gauss approximation and the N-point Gauss-Kronrod approximation</param>
        /// <param name="L1Norm">The L1 norm of the result, if there is a significant difference between this and the returned value, then the result is likely to be ill-conditioned.</param>
        /// <param name="targetRelativeError">The expected relative accuracy of the approximation.</param>
        /// <param name="maximumDepth">The maximum number of interval splittings permitted before stopping</param>
        /// <param name="order">The number of Gauss-Kronrod points. Pre-computed for 15, 21, 31, 41, 51 and 61 points</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static Complex GaussKronrod(Func<double, Complex> f, double intervalBegin, double intervalEnd, out double error, out double L1Norm, double targetRelativeError = 1E-8, int maximumDepth = 15, int order = 15)
        {
            return GaussKronrodRule.ContourIntegrate(f, intervalBegin, intervalEnd, out error, out L1Norm, targetRelativeError: targetRelativeError, maximumDepth: maximumDepth, order: order);
        }
    }
}
