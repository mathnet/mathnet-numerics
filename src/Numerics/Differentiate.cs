// <copyright file="Differentiate.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using MathNet.Numerics.Differentiation;

namespace MathNet.Numerics
{
    /// <summary>
    /// Numerical Derivative.
    /// </summary>
    public static class Differentiate
    {
        /// <summary>
        /// Initialized a NumericalDerivative with the given points and center.
        /// </summary>
        public static NumericalDerivative Points(int points, int center)
        {
            return new NumericalDerivative(points, center);
        }

        /// <summary>
        /// Initialized a NumericalDerivative with the default points and center for the given order.
        /// </summary>
        public static NumericalDerivative Order(int order)
        {
            var points = order + (order.IsEven() ? 1 : 2);
            return new NumericalDerivative(points, points/2);
        }

        /// <summary>
        /// Evaluates the derivative of a scalar univariate function.
        /// </summary>
        /// <param name="f">Univariate function handle.</param>
        /// <param name="x">Point at which to evaluate the derivative.</param>
        /// <param name="order">Derivative order.</param>
        public static double Derivative(Func<double, double> f, double x, int order)
        {
            return Order(order).EvaluateDerivative(f, x, order);
        }

        /// <summary>
        /// Creates a function handle for the derivative of a scalar univariate function.
        /// </summary>
        /// <param name="f">Univariate function handle.</param>
        /// <param name="order">Derivative order.</param>
        public static Func<double, double> DerivativeFunc(Func<double, double> f, int order)
        {
            return Order(order).CreateDerivativeFunctionHandle(f, order);
        }

        /// <summary>
        /// Evaluates the first derivative of a scalar univariate function.
        /// </summary>
        /// <param name="f">Univariate function handle.</param>
        /// <param name="x">Point at which to evaluate the derivative.</param>
        public static double FirstDerivative(Func<double, double> f, double x)
        {
            return Order(1).EvaluateDerivative(f, x, 1);
        }

        /// <summary>
        /// Creates a function handle for the first derivative of a scalar univariate function.
        /// </summary>
        /// <param name="f">Univariate function handle.</param>
        public static Func<double, double> FirstDerivativeFunc(Func<double, double> f)
        {
            return Order(1).CreateDerivativeFunctionHandle(f, 1);
        }

        /// <summary>
        /// Evaluates the second derivative of a scalar univariate function.
        /// </summary>
        /// <param name="f">Univariate function handle.</param>
        /// <param name="x">Point at which to evaluate the derivative.</param>
        public static double SecondDerivative(Func<double, double> f, double x)
        {
            return Order(2).EvaluateDerivative(f, x, 2);
        }

        /// <summary>
        /// Creates a function handle for the second derivative of a scalar univariate function.
        /// </summary>
        /// <param name="f">Univariate function handle.</param>
        public static Func<double, double> SecondDerivativeFunc(Func<double, double> f)
        {
            return Order(2).CreateDerivativeFunctionHandle(f, 2);
        }

        /// <summary>
        /// Evaluates the partial derivative of a multivariate function.
        /// </summary>
        /// <param name="f">Multivariate function handle.</param>
        /// <param name="x">Vector at which to evaluate the derivative.</param>
        /// <param name="parameterIndex">Index of independent variable for partial derivative.</param>
        /// <param name="order">Derivative order.</param>
        public static double PartialDerivative(Func<double[], double> f, double[] x, int parameterIndex, int order)
        {
            return Order(order).EvaluatePartialDerivative(f, x, parameterIndex, order);
        }

        /// <summary>
        /// Creates a function handle for the partial derivative of a multivariate function.
        /// </summary>
        /// <param name="f">Multivariate function handle.</param>
        /// <param name="parameterIndex">Index of independent variable for partial derivative.</param>
        /// <param name="order">Derivative order.</param>
        public static Func<double[], double> PartialDerivativeFunc(Func<double[], double> f, int parameterIndex, int order)
        {
            return Order(order).CreatePartialDerivativeFunctionHandle(f, parameterIndex, order);
        }

        /// <summary>
        /// Evaluates the first partial derivative of a multivariate function.
        /// </summary>
        /// <param name="f">Multivariate function handle.</param>
        /// <param name="x">Vector at which to evaluate the derivative.</param>
        /// <param name="parameterIndex">Index of independent variable for partial derivative.</param>
        public static double FirstPartialDerivative(Func<double[], double> f, double[] x, int parameterIndex)
        {
            return PartialDerivative(f, x, parameterIndex, 1);
        }

        /// <summary>
        /// Creates a function handle for the first partial derivative of a multivariate function.
        /// </summary>
        /// <param name="f">Multivariate function handle.</param>
        /// <param name="parameterIndex">Index of independent variable for partial derivative.</param>
        public static Func<double[], double> FirstPartialDerivativeFunc(Func<double[], double> f, int parameterIndex)
        {
            return PartialDerivativeFunc(f, parameterIndex, 1);
        }

        /// <summary>
        /// Evaluates the partial derivative of a bivariate function.
        /// </summary>
        /// <param name="f">Bivariate function handle.</param>
        /// <param name="x">First argument at which to evaluate the derivative.</param>
        /// <param name="y">Second argument at which to evaluate the derivative.</param>
        /// <param name="parameterIndex">Index of independent variable for partial derivative.</param>
        /// <param name="order">Derivative order.</param>
        public static double PartialDerivative2(Func<double, double, double> f, double x, double y, int parameterIndex, int order)
        {
            return Order(order).EvaluatePartialDerivative(array => f(array[0], array[1]), new[] { x, y }, parameterIndex, order);
        }

        /// <summary>
        /// Creates a function handle for the partial derivative of a bivariate function.
        /// </summary>
        /// <param name="f">Bivariate function handle.</param>
        /// <param name="parameterIndex">Index of independent variable for partial derivative.</param>
        /// <param name="order">Derivative order.</param>
        public static Func<double, double, double> PartialDerivative2Func(Func<double, double, double> f, int parameterIndex, int order)
        {
            var handle = Order(order).CreatePartialDerivativeFunctionHandle(array => f(array[0], array[1]), parameterIndex, order);
            return (x, y) => handle(new[] { x, y });
        }

        /// <summary>
        /// Evaluates the first partial derivative of a bivariate function.
        /// </summary>
        /// <param name="f">Bivariate function handle.</param>
        /// <param name="x">First argument at which to evaluate the derivative.</param>
        /// <param name="y">Second argument at which to evaluate the derivative.</param>
        /// <param name="parameterIndex">Index of independent variable for partial derivative.</param>
        public static double FirstPartialDerivative2(Func<double, double, double> f, double x, double y, int parameterIndex)
        {
            return PartialDerivative2(f, x, y, parameterIndex, 1);
        }

        /// <summary>
        /// Creates a function handle for the first partial derivative of a bivariate function.
        /// </summary>
        /// <param name="f">Bivariate function handle.</param>
        /// <param name="parameterIndex">Index of independent variable for partial derivative.</param>
        public static Func<double, double, double> FirstPartialDerivative2Func(Func<double, double, double> f, int parameterIndex)
        {
            return PartialDerivative2Func(f, parameterIndex, 1);
        }
    }
}
