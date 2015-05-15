﻿// <copyright file="BfgsSolver.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.RootFinding
{
    /// <summary>
    /// Broyden-Fletcher-Goldfarb-Shanno solver for finding function minima
    /// See http://en.wikipedia.org/wiki/Broyden%E2%80%93Fletcher%E2%80%93Goldfarb%E2%80%93Shanno_algorithm
    /// Inspired by implementation: https://github.com/PatWie/CppNumericalSolvers/blob/master/src/BfgsSolver.cpp
    /// </summary>
    public static class BfgsSolver
    {
        private const double gradientTolerance = 1e-5;
        private const int maxIterations = 100000;

        /// <summary>
        /// Finds a minimum of a function by the BFGS quasi-Newton method
        /// This uses the function and it's gradient (partial derivatives in each direction) and approximates the Hessian
        /// </summary>
        /// <param name="initialGuess">An initial guess</param>
        /// <param name="functionValue">Evaluates the function at a point</param>
        /// <param name="functionGradient">Evaluates the gradient of the function at a point</param>
        /// <returns>The minimum found</returns>
        public static Vector<double> Solve(Vector initialGuess, Func<Vector<double>, double> functionValue, Func<Vector<double>, Vector<double>> functionGradient)
        {
            int dim = initialGuess.Count;
            int iter = 0;
            // H represents the approximation of the inverse hessian matrix
            // it is updated via the Sherman–Morrison formula (http://en.wikipedia.org/wiki/Sherman%E2%80%93Morrison_formula)
            Matrix<double> H = DenseMatrix.CreateIdentity(dim);
            
            Vector<double> x = initialGuess;
            Vector<double> x_old = x;
            Vector<double> grad;

            do
            {
                // search along the direction of the gradient
                grad = functionGradient(x);
                Vector<double> p = -1 * H * grad;
                double rate = WolfeRule.LineSearch(x, p, functionValue, functionGradient);
                x = x + rate * p;
                Vector<double> grad_old = grad;

                // update the gradient
                grad = functionGradient(x);

                Vector<double> s = x - x_old;
                Vector<double> y = grad - grad_old;

                double rho = 1.0 / (y * s);
                if (iter == 0)
                {
                    // set up an initial hessian
                    H = (y * s) / (y * y) * DenseMatrix.CreateIdentity(dim);
                }

                var sM = s.ToColumnMatrix();
                var yM = y.ToColumnMatrix();

                // Update the estimate of the hessian
                H = H 
                    - rho * (sM * (yM.TransposeThisAndMultiply(H)) + (H * yM).TransposeAndMultiply(sM))
                    + rho * rho * (y.DotProduct(H * y) + 1.0 / rho) * (sM.TransposeAndMultiply(sM));
                x_old = x;
                iter++;
            }
            while ((grad.InfinityNorm() > gradientTolerance) && (iter < maxIterations));

            return x;
        }
    }
}
