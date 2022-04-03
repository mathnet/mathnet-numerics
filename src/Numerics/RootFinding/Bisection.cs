// <copyright file="Bisection.cs" company="Math.NET">
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
    /// Bisection root-finding algorithm.
    /// </summary>
    public static class Bisection
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="guessLowerBound">Guess for the low value of the range where the root is supposed to be. Will be expanded if needed.</param>
        /// <param name="guessUpperBound">Guess for the  high value of the range where the root is supposed to be. Will be expanded if needed.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <param name="expandFactor">Factor at which to expand the bounds, if needed. Default 1.6.</param>
        /// <param name="maxExpandIteratons">Maximum number of expand iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRootExpand(Func<double, double> f, double guessLowerBound, double guessUpperBound, double accuracy = 1e-8, int maxIterations = 100, double expandFactor = 1.6, int maxExpandIteratons = 100)
        {
            ZeroCrossingBracketing.ExpandReduce(f, ref guessLowerBound, ref guessUpperBound, expandFactor, maxExpandIteratons, maxExpandIteratons*10);
            return FindRoot(f, guessLowerBound, guessUpperBound, accuracy, maxIterations);
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-14, int maxIterations = 100)
        {
            double root;
            if (TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out root))
            {
                return root;
            }

            throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy for both the root and the function value at the root. The root will be refined until the accuracy or the maximum number of iterations is reached. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy, int maxIterations, out double root)
        {
            if (accuracy <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than zero.");
            }

            if (upperBound < lowerBound)
            {
                (upperBound, lowerBound) = (lowerBound, upperBound);
            }

            double fmin = f(lowerBound);
            if (Math.Sign(fmin) == 0)
            {
                root = lowerBound;
                return true;
            }

            double fmax = f(upperBound);
            if (Math.Sign(fmax) == 0)
            {
                root = upperBound;
                return true;
            }

            root = 0.5 * (lowerBound + upperBound);

            // bad bracketing?
            if (Math.Sign(fmin) == Math.Sign(fmax))
            {
                return false;
            }

            for (int i = 0; i <= maxIterations; i++)
            {
                double froot = f(root);

                if (upperBound - lowerBound <= 2*accuracy && Math.Abs(froot) <= accuracy)
                {
                    return true;
                }

                if ((lowerBound == root) || (upperBound == root))
                {
                    // accuracy not sufficient, but cannot be improved further
                    return false;
                }

                if (Math.Sign(froot) == Math.Sign(fmin))
                {
                    lowerBound = root;
                    fmin = froot;
                }
                else if (Math.Sign(froot) == Math.Sign(fmax))
                {
                    upperBound = root;
                    fmax = froot;
                }
                else // Math.Sign(froot) == 0
                {
                    return true;
                }

                root = 0.5*(lowerBound + upperBound);
            }

            return false;
        }
    }
}
