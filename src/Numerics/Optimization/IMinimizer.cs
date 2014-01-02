// <copyright file="IMinimizer.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Interface implemented by a class that minimizes f(p) where p is a vector of model parameters.
    /// A class implenting this interface can then be used to solve curve fitting problems.
    /// </summary>
    public interface IMinimizer
    {
        double[] Minimize(Func<double[], double> function, double[] pInitialGuess);
    }

    public static class NonLinearCurveFitExtensions
    {
        /// <summary>
        /// Non-Linear Least-Squares fitting the points (x,y) to a specified function of y : x -> f(x, p), p being a vector of parameters.
        /// returning its best fitting parameters p.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="f"></param>
        /// <param name="pStart">Initial guess of parameters, p.</param>
        /// <returns></returns>
        public static double[] CurveFit(this IMinimizer minimizer, double[] x, double[] y, Func<double, double[], double> f,
            double[] pStart)
        {
            // Need to minimize sum of squares of residuals; create this function:
            Func<double[], double> function = p =>
            {
                double sum = 0;
                for (int i = 0; i < x.Length; ++i)
                {
                    double temp = y[i] - f(x[i], p);
                    sum += temp*temp;
                }
                return sum;
            };
            return minimizer.Minimize(function, pStart);
        }
    }
}
