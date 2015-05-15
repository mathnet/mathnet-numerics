// <copyright file="WolfeRule.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.RootFinding
{
    /// <summary>
    /// Performs an inexact line search. This is used as a part of quasi-Newton optimization methods to figure
    /// out how far to move along a certain gradient.
    /// See http://en.wikipedia.org/wiki/Wolfe_conditions
    /// Inspired by implementation: https://github.com/PatWie/CppNumericalSolvers/blob/master/src/linesearch/WolfeRule.h
    /// </summary>
    internal static class WolfeRule
    {
        /// <summary>
        /// Searches along a line to satisfy the Wolfe conditions (inexact search for minimum)
        /// </summary>
        /// <param name="x0">Starting point of search</param>
        /// <param name="z">Search direction</param>
        /// <param name="functionValue">Evaluates the function being minimized</param>
        /// <param name="functionGradient">Evaluates the gradient of the function</param>
        /// <param name="alphaInit">Initial value for the coefficient of z (distance to travel in z direction)</param>
        /// <returns></returns>
        public static double LineSearch(
            Vector<double> x0,
            Vector<double> z,
            Func<Vector<double>, double> functionValue,
            Func<Vector<double>, Vector<double>> functionGradient,
            float alphaInit = 1)
        {
            Vector<double> x = x0;

            // evaluate phi(0)
            double phi0 = functionValue(x0);

            // evaluate phi'(0)
            Vector<double> grad = functionGradient(x);
            double phi0_dash = z * grad;

            double alpha = alphaInit;

            bool decrease_direction = true;

            // 200 guesses max
            for (int iter = 0; iter < 200; ++iter) {

                // new guess for phi(alpha)
                Vector<double> x_candidate = x + alpha * z;
                double phi = functionValue(x_candidate);

                // decrease condition invalid --> shrink interval
                if (phi > phi0 + 0.0001 * alpha * phi0_dash)
                {
                    alpha *= 0.5;
                    decrease_direction = false;
                }
                else
                {
                    // valid decrease --> test strong wolfe condition
                    Vector<double> grad2 = functionGradient(x_candidate);
                    double phi_dash = z * grad2;

                    // curvature condition invalid ?
                    if ((phi_dash < 0.9 * phi0_dash) || !decrease_direction) { 
                        // increase interval
                        alpha *= 4.0;
                    }
                    else {
                        // both condition are valid --> we are happy
                        x = x_candidate;
                        grad = grad2;
                        phi0 = phi;
                        return alpha;
                    }
                }
            }


            return alpha;
        }
    }
}
