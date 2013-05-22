// <copyright file="Bisection.cs" company="Math.NET">
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.RootFinding.Algorithms
{
    public static class Bisection
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRootExpand(Func<double, double> f, double guessLowerBound, double guessUpperBound, double accuracy = 1e-8, double expandFactor = 1.6, int maxExpandIteratons = 100)
        {
            ZeroCrossingBracketing.Expand(f, ref guessLowerBound, ref guessUpperBound, expandFactor, maxExpandIteratons);
            return FindRoot(f, guessLowerBound, guessUpperBound, accuracy);
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8)
        {
            double fmin = f(lowerBound);
            double fmax = f(upperBound);

            if (Math.Abs(fmin) < accuracy)
            {
                return lowerBound;
            }
            if (Math.Abs(fmax) < accuracy)
            {
                return upperBound;
            }

            if (Math.Sign(fmin) == Math.Sign(fmax))
            {
                throw new NonConvergenceException(Resources.RootMustBeBracketedByBounds);
            }

            while (Math.Abs(fmax - fmin) > 0.5 * accuracy || Math.Abs(upperBound - lowerBound) > 0.5 * Precision.DoubleMachinePrecision)
            {
                double midpoint = 0.5*(upperBound + lowerBound);
                double midval = f(midpoint);

                if (Math.Sign(midval) == Math.Sign(fmin))
                {
                    lowerBound = midpoint;
                    fmin = midval;
                }
                else if (Math.Sign(midval) == Math.Sign(fmax))
                {
                    upperBound = midpoint;
                    fmax = midval;
                }
                else
                {
                    return midpoint;
                }
            }

            return 0.5*(lowerBound + upperBound);
        }
    }
}
