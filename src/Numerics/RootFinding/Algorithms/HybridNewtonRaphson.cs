// <copyright file="NewtonRaphson.cs" company="Math.NET">
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

namespace MathNet.Numerics.RootFinding.Algorithms
{
    public static class HybridNewtonRaphson
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="df">The first derivative of the function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Example: 1e-14.</param>
        /// <param name="maxIterations">Maximum number of iterations. Example: 100.</param>
        /// <param name="subdivision">How many parts an interval should be split into for zero crossing scanning in case of lacking bracketing. Example: 20.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <remarks>Hybrid Newton-Raphson that falls back to bisection when overshooting or converging too slow, or to subdivision on lacking bracketing.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRoot(Func<double, double> f, Func<double, double> df, double lowerBound, double upperBound, double accuracy, int maxIterations, int subdivision)
        {
            double root;
            if (TryFindRoot(f, df, lowerBound, upperBound, accuracy, maxIterations, subdivision, out root))
            {
                return root;
            }
            throw new NonConvergenceException("The algorithm has exceeded the number of iterations allowed");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="df">The first derivative of the function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Example: 1e-14.</param>
        /// <param name="maxIterations">Maximum number of iterations. Example: 100.</param>
        /// <param name="subdivision">How many parts an interval should be split into for zero crossing scanning in case of lacking bracketing. Example: 20.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        /// <remarks>Hybrid Newton-Raphson that falls back to bisection when overshooting or converging too slow, or to subdivision on lacking bracketing.</remarks>
        public static bool TryFindRoot(Func<double, double> f, Func<double, double> df, double lowerBound, double upperBound, double accuracy, int maxIterations, int subdivision, out double root)
        {
            double fmin = f(lowerBound);
            double fmax = f(upperBound);

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
            double fx = f(root);
            double lastStep = Math.Abs(upperBound - lowerBound);
            for (int i = 0; i < maxIterations; i++)
            {
                double dfx = df(root);

                // Netwon-Raphson step
                double step = fx/dfx;
                root -= step;

                if (Math.Abs(step) < accuracy && Math.Abs(fx) < accuracy)
                {
                    return true;
                }

                bool overshoot = root > upperBound, undershoot = root < lowerBound;
                if (overshoot || undershoot || Math.Abs(2*fx) > Math.Abs(lastStep*dfx))
                {
                    // Newton-Raphson step failed

                    // If same signs, try subdivision to scan for zero crossing intervals
                    if (Math.Sign(fmin) == Math.Sign(fmax) && TryScanForCrossingsWithRoots(f, df, lowerBound, upperBound, accuracy, maxIterations - i - 1, subdivision, out root))
                    {
                        return true;
                    }

                    // Bisection
                    root = 0.5*(upperBound + lowerBound);
                    fx = f(root);
                    lastStep = 0.5*Math.Abs(upperBound - lowerBound);
                    if (Math.Sign(fx) == Math.Sign(fmin))
                    {
                        lowerBound = root;
                        fmin = fx;
                        if (overshoot)
                        {
                            root = upperBound;
                            fx = fmax;
                        }
                    }
                    else
                    {
                        upperBound = root;
                        fmax = fx;
                        if (undershoot)
                        {
                            root = lowerBound;
                            fx = fmin;
                        }
                    }
                    continue;
                }

                // Evaluation
                fx = f(root);
                lastStep = step;

                // Update bounds
                if (Math.Sign(fx) != Math.Sign(fmin))
                {
                    upperBound = root;
                    fmax = fx;
                }
                else if (Math.Sign(fx) != Math.Sign(fmax))
                {
                    lowerBound = root;
                    fmin = fx;
                }
                else if (Math.Sign(fmin) != Math.Sign(fmax) && Math.Abs(fx) < accuracy)
                {
                    return true;
                }
            }

            return false;
        }

        static bool TryScanForCrossingsWithRoots(Func<double, double> f, Func<double, double> df, double lowerBound, double upperBound, double accuracy, int maxIterations, int subdivision, out double root)
        {
            var zeroCrossings = ZeroCrossingBracketing.FindIntervalsWithin(f, lowerBound, upperBound, subdivision);
            foreach (Tuple<double, double> bounds in zeroCrossings)
            {
                if (TryFindRoot(f, df, bounds.Item1, bounds.Item2, accuracy, maxIterations, subdivision, out root))
                {
                    return true;
                }
            }

            root = double.NaN;
            return false;
        }
    }
}
