// <copyright file="NewtonRaphson.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
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

namespace MathNet.Numerics.RootFinding
{
    /// <summary>
    /// Pure Newton-Raphson root-finding algorithm without any recovery measures in cases it behaves badly.
    /// The algorithm aborts immediately if the root leaves the bound interval.
    /// </summary>
    /// <seealso cref="RobustNewtonRaphson"/>
    public static class NewtonRaphson
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="df">The first derivative of the function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be. Aborts if it leaves the interval.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be. Aborts if it leaves the interval.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRoot(Func<double, double> f, Func<double, double> df, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            if (TryFindRoot(f, df, 0.5 * (lowerBound + upperBound), lowerBound, upperBound, accuracy, maxIterations, out var root))
            {
                return root;
            }

            throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds. Consider to use RobustNewtonRaphson instead.");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="df">The first derivative of the function to find roots from.</param>
        /// <param name="initialGuess">Initial guess of the root.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be. Aborts if it leaves the interval. Default MinValue.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be. Aborts if it leaves the interval. Default MaxValue.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRootNearGuess(Func<double, double> f, Func<double, double> df, double initialGuess, double lowerBound = double.MinValue, double upperBound = double.MaxValue, double accuracy = 1e-8, int maxIterations = 100)
        {
            if (TryFindRoot(f, df, initialGuess, lowerBound, upperBound, accuracy, maxIterations, out var root))
            {
                return root;
            }

            throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds. Consider to use RobustNewtonRaphson instead.");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="df">The first derivative of the function to find roots from.</param>
        /// <param name="initialGuess">Initial guess of the root.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be. Aborts if it leaves the interval.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be. Aborts if it leaves the interval.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Example: 1e-14. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Example: 100.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRoot(Func<double, double> f, Func<double, double> df, double initialGuess, double lowerBound, double upperBound, double accuracy, int maxIterations, out double root)
        {
            if (accuracy <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than zero.");
            }

            root = initialGuess;
            for (int i = 0; i < maxIterations && root >= lowerBound && root <= upperBound; i++)
            {
                // Evaluation
                double fx = f(root);
                if (fx == 0.0)
                {
                    return true;
                }

                double dfx = df(root);

                // Netwon-Raphson step
                double step = fx/dfx;
                root -= step;

                if (Math.Abs(step) < accuracy && Math.Abs(fx) < accuracy)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
