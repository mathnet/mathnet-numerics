// <copyright file="Brent.cs" company="Math.NET">
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
    /// Algorithm by Brent, Van Wijngaarden, Dekker et al.
    /// Implementation inspired by Press, Teukolsky, Vetterling, and Flannery, "Numerical Recipes in C", 2nd edition, Cambridge University Press
    /// </summary>
    public static class Brent
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
        public static double FindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            if (TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out var root))
            {
                return root;
            }

            throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy, int maxIterations, out double root)
        {
            if (accuracy <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than zero.");
            }

            double fmin = f(lowerBound);
            double fmax = f(upperBound);
            double froot = fmax;
            double d = 0.0, e = 0.0;

            root = upperBound;
            double xMid = double.NaN;

            // Root must be bracketed.
            if (Math.Sign(fmin) == Math.Sign(fmax))
            {
                return false;
            }

            for (int i = 0; i <= maxIterations; i++)
            {
                // adjust bounds
                if (Math.Sign(froot) == Math.Sign(fmax))
                {
                    upperBound = lowerBound;
                    fmax = fmin;
                    e = d = root - lowerBound;
                }

                if (Math.Abs(fmax) < Math.Abs(froot))
                {
                    lowerBound = root;
                    root = upperBound;
                    upperBound = lowerBound;
                    fmin = froot;
                    froot = fmax;
                    fmax = fmin;
                }

                // convergence check
                double xAcc1 = Precision.PositiveDoublePrecision*Math.Abs(root) + 0.5*accuracy;
                double xMidOld = xMid;
                xMid = (upperBound - root)/2.0;

                if (Math.Abs(xMid) <= xAcc1 || froot.AlmostEqualNormRelative(0, froot, accuracy))
                {
                    return true;
                }

                if (xMid == xMidOld)
                {
                    // accuracy not sufficient, but cannot be improved further
                    return false;
                }

                if (Math.Abs(e) >= xAcc1 && Math.Abs(fmin) > Math.Abs(froot))
                {
                    // Attempt inverse quadratic interpolation
                    double s = froot/fmin;
                    double p;
                    double q;
                    if (lowerBound.AlmostEqualRelative(upperBound))
                    {
                        p = 2.0*xMid*s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = fmin/fmax;
                        double r = froot/fmax;
                        p = s*(2.0*xMid*q*(q - r) - (root - lowerBound)*(r - 1.0));
                        q = (q - 1.0)*(r - 1.0)*(s - 1.0);
                    }

                    if (p > 0.0)
                    {
                        // Check whether in bounds
                        q = -q;
                    }

                    p = Math.Abs(p);
                    if (2.0*p < Math.Min(3.0*xMid*q - Math.Abs(xAcc1*q), Math.Abs(e*q)))
                    {
                        // Accept interpolation
                        e = d;
                        d = p/q;
                    }
                    else
                    {
                        // Interpolation failed, use bisection
                        d = xMid;
                        e = d;
                    }
                }
                else
                {
                    // Bounds decreasing too slowly, use bisection
                    d = xMid;
                    e = d;
                }

                lowerBound = root;
                fmin = froot;
                if (Math.Abs(d) > xAcc1)
                {
                    root += d;
                }
                else
                {
                    root += Sign(xAcc1, xMid);
                }

                froot = f(root);
            }

            return false;
        }

        /// <summary>Helper method useful for preventing rounding errors.</summary>
        /// <returns>a*sign(b)</returns>
        static double Sign(double a, double b)
        {
            return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a);
        }
    }
}
