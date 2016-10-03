﻿// <copyright file="Bisection.cs" company="Math.NET">
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
using MathNet.Numerics.Properties;

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
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <param name="expandFactor">Factor at which to expand the bounds, if needed. Default 1.6.</param>
        /// <param name="maxExpandIteratons">Maximum number of expand iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRootExpand(Func<double, double> f, double guessLowerBound, double guessUpperBound, double accuracy = 1e-8, int maxIterations = 100, double expandFactor = 1.6, int maxExpandIteratons = 100)
        {
            ZeroCrossingBracketing.Expand(f, ref guessLowerBound, ref guessUpperBound, expandFactor, maxExpandIteratons);
            return FindRoot(f, guessLowerBound, guessUpperBound, accuracy, maxIterations);
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            double root;
            if (TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out root))
            {
                return root;
            }

            throw new NonConvergenceException(Resources.RootFindingFailed);
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy, int maxIterations, out double root)
        {
            double fmin = f(lowerBound);
            double fmax = f(upperBound);

            // already there?
            if (Math.Abs(fmin) < accuracy)
            {
                root = lowerBound;
                return true;
            }

            if (Math.Abs(fmax) < accuracy)
            {
                root = upperBound;
                return true;
            }

            root = 0.5*(lowerBound + upperBound);

            // bad bracketing?
            if (Math.Sign(fmin) == Math.Sign(fmax))
            {
                return false;
            }

            for (int i = 0; i <= maxIterations; i++)
            {
                if (Math.Abs(fmax - fmin) < 0.5*accuracy && upperBound.AlmostEqualRelative(lowerBound))
                {
                    return true;
                }

                if ((lowerBound == root) || (upperBound == root))
                {
                    // accuracy not sufficient, but cannot be improved further
                    return false;
                }

                double midval = f(root);

                if (Math.Sign(midval) == Math.Sign(fmin))
                {
                    lowerBound = root;
                    fmin = midval;
                }
                else if (Math.Sign(midval) == Math.Sign(fmax))
                {
                    upperBound = root;
                    fmax = midval;
                }
                else
                {
                    return true;
                }

                root = 0.5*(lowerBound + upperBound);
            }

            return false;
        }
    }
}
