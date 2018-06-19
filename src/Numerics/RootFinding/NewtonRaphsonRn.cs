// <copyright file="NewtonRaphson.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using System.Linq;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.RootFinding
{
    /// <summary>
    /// Pure Newton-Raphson root-finding algorithm without any recovery measures in cases it behaves badly.
    /// The algorithm aborts immediately if the root leaves the bound interval.
    /// </summary>
    public static class NewtonRaphsonRn
    {
        /// <summary>Find a solution of a system of equations fi(x)=0.</summary>
        /// <param name="f">The functions to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be. Aborts if it leaves the interval.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be. Aborts if it leaves the interval.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double[] FindRoot(Func<double[], double>[] f, double lowerBound = double.MinValue, double upperBound = double.MaxValue, double accuracy = 1e-8, int maxIterations = 100)
        {
            var intialGuess = new double[f.Length];
            for (var i = 0; i < f.Length; i++)
            {
                intialGuess[i] = 0.5 * (lowerBound + upperBound);
            }

            return FindRootNearGuess(f, intialGuess, lowerBound, upperBound, accuracy, maxIterations);
        }

        /// <summary>Find a solution of a system of equations fi(x)=0.</summary>
        /// <param name="f">The functions to find roots from.</param>
        /// <param name="initialGuess">Initial guess of the root.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be. Aborts if it leaves the interval. Default MinValue.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be. Aborts if it leaves the interval. Default MaxValue.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double[] FindRootNearGuess(Func<double[], double>[] f, double[] initialGuess, double lowerBound = double.MinValue, double upperBound = double.MaxValue, double accuracy = 1e-8, int maxIterations = 100)
        {
            var n = f.Length;
            var numericalJacobian = new NumericalJacobian();

            var fx = Vector<double>.Build.Dense(n);

            var root = initialGuess;
            for (int i = 0; i < maxIterations && root.All(x => x >= lowerBound && x <= upperBound); i++)
            {
                // Evaluation
                for (var j = 0; j < n; j++)
                {
                    fx[j] = f[j](root);
                }

                // Newton-Raphson step
                var jacobian = numericalJacobian.Evaluate(f, root);
                var jacobianMatrix = Matrix<double>.Build.DenseOfArray(jacobian);
                var step = jacobianMatrix.Solve(fx);
                for (var j = 0; j < n; j++)
                {
                    root[j] -= step[j];
                }

                var errSize = Math.Abs(fx.L2Norm());
                if (errSize < accuracy)
                {
                    return root;
                }
            }

            throw new NonConvergenceException();
        }
    }
}
