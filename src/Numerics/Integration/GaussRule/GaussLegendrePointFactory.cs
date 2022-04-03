// <copyright file="GaussLegendreRule.cs" company="Math.NET">
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

namespace MathNet.Numerics.Integration.GaussRule
{
    /// <summary>
    /// Creates and maps a Gauss-Legendre point.
    /// </summary>
    internal static class GaussLegendrePointFactory
    {
        [ThreadStatic]
        static GaussPoint _gaussLegendrePoint;

        /// <summary>
        /// Getter for the GaussPoint.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calculated on the fly.</param>
        /// <returns>Object containing the non-negative abscissas/weights, order, and intervalBegin/intervalEnd. The non-negative abscissas/weights are generated over the interval [-1,1] for the given order.</returns>
        public static GaussPoint GetGaussPoint(int order)
        {
            // Try to get the GaussPoint from the cached static field.
            bool gaussLegendrePointIsCached = _gaussLegendrePoint != null && _gaussLegendrePoint.Order == order;
            if (!gaussLegendrePointIsCached)
            {
                // Try to find the GaussPoint in the precomputed dictionary.
                if (!GaussLegendrePoint.PreComputed.TryGetValue(order, out _gaussLegendrePoint))
                {
                    _gaussLegendrePoint = GaussLegendrePoint.Generate(order, 1e-10); // Generate the GaussPoint on the fly.
                }
            }

            return _gaussLegendrePoint;
        }

        /// <summary>
        /// Getter for the GaussPoint.
        /// </summary>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calculated on the fly.</param>
        /// <returns>Object containing the abscissas/weights, order, and intervalBegin/intervalEnd.</returns>
        public static GaussPoint GetGaussPoint(double intervalBegin, double intervalEnd, int order)
        {
            return Map(intervalBegin, intervalEnd, GetGaussPoint(order));
        }

        /// <summary>
        /// Maps the non-negative abscissas/weights from the interval [-1, 1] to the interval [intervalBegin, intervalEnd].
        /// </summary>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="gaussPoint">Object containing the non-negative abscissas/weights, order, and intervalBegin/intervalEnd. The non-negative abscissas/weights are generated over the interval [-1,1] for the given order.</param>
        /// <returns>Object containing the abscissas/weights, order, and intervalBegin/intervalEnd.</returns>
        static GaussPoint Map(double intervalBegin, double intervalEnd, GaussPoint gaussPoint)
        {
            double[] abscissas = new double[gaussPoint.Order];
            double[] weights = new double[gaussPoint.Order];

            double a = 0.5 * (intervalEnd - intervalBegin);
            double b = 0.5 * (intervalEnd + intervalBegin);

            int m = (gaussPoint.Order + 1) >> 1;

            var gaussPointAbscissas = gaussPoint.Abscissas;
            var gaussPointWeights = gaussPoint.Weights;
            for (int i = 1; i <= m; i++)
            {
                int index1 = gaussPoint.Order - i;
                int index2 = i - 1;
                int index3 = m - i;

                abscissas[index1] = gaussPointAbscissas[index3] * a + b;
                abscissas[index2] = -gaussPointAbscissas[index3] * a + b;

                weights[index1] = gaussPointWeights[index3] * a;
                weights[index2] = gaussPointWeights[index3] * a;
            }

            return new GaussPoint(intervalBegin, intervalEnd, gaussPoint.Order, abscissas, weights);
        }
    }
}
