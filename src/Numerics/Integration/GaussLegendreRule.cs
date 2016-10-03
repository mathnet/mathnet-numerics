﻿// <copyright file="GaussLegendreRule.cs" company="Math.NET">
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
using MathNet.Numerics.Integration.GaussRule;

namespace MathNet.Numerics.Integration
{
    /// <summary>
    /// Approximates a definite integral using an Nth order Gauss-Legendre rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calulcated on the fly.
    /// </summary>
    public class GaussLegendreRule
    {
        private readonly GaussPoint _gaussLegendrePoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussLegendreRule"/> class.
        /// </summary>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calulcated on the fly.</param>
        public GaussLegendreRule(double intervalBegin, double intervalEnd, int order)
        {
            _gaussLegendrePoint = GaussLegendrePointFactory.GetGaussPoint(intervalBegin, intervalEnd, order);
        }

        /// <summary>
        /// Gettter for the ith abscissa.
        /// </summary>
        /// <param name="index">Index of the ith abscissa.</param>
        /// <returns>The ith abscissa.</returns>
        public double GetAbscissa(int index)
        {
            return _gaussLegendrePoint.Abscissas[index];
        }

        /// <summary>
        /// Getter that returns a clone of the array containing the abscissas.
        /// </summary>
        public double[] Abscissas
        {
            get
            {
                return _gaussLegendrePoint.Abscissas.Clone() as double[];
            }
        }

        /// <summary>
        /// Getter for the ith weight.
        /// </summary>
        /// <param name="index">Index of the ith weight.</param>
        /// <returns>The ith weight.</returns>
        public double GetWeight(int index)
        {
            return _gaussLegendrePoint.Weights[index];
        }

        /// <summary>
        /// Getter that returns a clone of the array containing the weights.
        /// </summary>
        public double[] Weights
        {
            get
            {
                return _gaussLegendrePoint.Weights.Clone() as double[];
            }
        }

        /// <summary>
        /// Getter for the order.
        /// </summary>
        public int Order
        {
            get
            {
                return _gaussLegendrePoint.Order;
            }
        }

        /// <summary>
        /// Getter for the InvervalBegin.
        /// </summary>
        public double IntervalBegin
        {
            get
            {
                return _gaussLegendrePoint.IntervalBegin;
            }
        }

        /// <summary>
        /// Getter for the InvervalEnd.
        /// </summary>
        public double IntervalEnd
        {
            get
            {
                return _gaussLegendrePoint.IntervalEnd;
            }
        }

        /// <summary>
        /// Approximates a definite integral using an Nth order Gauss-Legendre rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="invervalBegin">Where the interval starts, exclusive and finite.</param>
        /// <param name="invervalEnd">Where the interval ends, exclusive and finite.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calulcated on the fly.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double Integrate(Func<double, double> f, double invervalBegin, double invervalEnd, int order)
        {
            GaussPoint gaussLegendrePoint = GaussLegendrePointFactory.GetGaussPoint(order);

            double sum, ax;
            int i;
            int m = (order + 1) >> 1;

            double a = 0.5*(invervalEnd - invervalBegin);
            double b = 0.5*(invervalEnd + invervalBegin);

            if (order.IsOdd())
            {
                sum = gaussLegendrePoint.Weights[0]*f(b);
                for (i = 1; i < m; i++)
                {
                    ax = a*gaussLegendrePoint.Abscissas[i];
                    sum += gaussLegendrePoint.Weights[i]*(f(b + ax) + f(b - ax));
                }
            }
            else
            {
                sum = 0.0;
                for (i = 0; i < m; i++)
                {
                    ax = a*gaussLegendrePoint.Abscissas[i];
                    sum += gaussLegendrePoint.Weights[i]*(f(b + ax) + f(b - ax));
                }
            }

            return a*sum;
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
        public static double Integrate(Func<double, double, double> f, double invervalBeginA, double invervalEndA, double invervalBeginB, double invervalEndB, int order)
        {
            GaussPoint gaussLegendrePoint = GaussLegendrePointFactory.GetGaussPoint(order);

            double ax, cy, sum;
            int i, j;
            int m = (order + 1) >> 1;

            double a = 0.5*(invervalEndA - invervalBeginA);
            double b = 0.5*(invervalEndA + invervalBeginA);
            double c = 0.5*(invervalEndB - invervalBeginB);
            double d = 0.5*(invervalEndB + invervalBeginB);

            if (order.IsOdd())
            {
                sum = gaussLegendrePoint.Weights[0]*gaussLegendrePoint.Weights[0]*f(b, d);

                double t;
                for (j = 1, t = 0.0; j < m; j++)
                {
                    cy = c*gaussLegendrePoint.Abscissas[j];
                    t += gaussLegendrePoint.Weights[j]*(f(b, d + cy) + f(b, d - cy));
                }

                sum += gaussLegendrePoint.Weights[0]*t;

                for (i = 1, t = 0.0; i < m; i++)
                {
                    ax = a*gaussLegendrePoint.Abscissas[i];
                    t += gaussLegendrePoint.Weights[i]*(f(b + ax, d) + f(b - ax, d));
                }

                sum += gaussLegendrePoint.Weights[0]*t;

                for (i = 1; i < m; i++)
                {
                    ax = a*gaussLegendrePoint.Abscissas[i];
                    for (j = 1; j < m; j++)
                    {
                        cy = c*gaussLegendrePoint.Abscissas[j];
                        sum += gaussLegendrePoint.Weights[i]*gaussLegendrePoint.Weights[j]*(f(b + ax, d + cy) + f(ax + b, d - cy) + f(b - ax, d + cy) + f(b - ax, d - cy));
                    }
                }
            }
            else
            {
                sum = 0.0;
                for (i = 0; i < m; i++)
                {
                    ax = a*gaussLegendrePoint.Abscissas[i];
                    for (j = 0; j < m; j++)
                    {
                        cy = c*gaussLegendrePoint.Abscissas[j];
                        sum += gaussLegendrePoint.Weights[i]*gaussLegendrePoint.Weights[j]*(f(b + ax, d + cy) + f(ax + b, d - cy) + f(b - ax, d + cy) + f(b - ax, d - cy));
                    }
                }
            }

            return c*a*sum;
        }
    }
}
