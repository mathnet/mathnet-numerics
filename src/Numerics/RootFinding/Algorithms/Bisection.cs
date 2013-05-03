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

namespace MathNet.Numerics.RootFinding.Algorithms
{
    public static class Bisection
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        public static double FindRoot(Func<double, double> f, double lowerBound, double upperBound, double fTolerance = 1e-5, double xTolerance = 1e-5, double lowerExpansionFactor = -1.0, double upperExpansionFactor = -1.0, int maxExpansionSteps = 10)
        {
            double fmin = f(lowerBound);
            double fmax = f(upperBound);

            if (fmin == 0.0)
                return lowerBound;
            if (fmax == 0.0)
                return upperBound;

            ValidateEvaluation(fmin, lowerBound);
            ValidateEvaluation(fmax, upperBound);

            if (Math.Sign(fmin) == Math.Sign(fmax) && lowerExpansionFactor <= 1.0 && upperExpansionFactor <= 1.0)
                throw new Exception("Bounds do not necessarily span a root, and StepExpansionFactor is not set to expand the interval in this case.");

            int expansionSteps = 0;
            while (Math.Sign(fmin) == Math.Sign(fmax) && expansionSteps < maxExpansionSteps)
            {
                double range = upperBound - lowerBound;
                if (upperExpansionFactor <= 0.0 || (lowerExpansionFactor > 0.0 && Math.Abs(fmin) < Math.Abs(fmax)))
                {
                    lowerBound = upperBound - lowerExpansionFactor * range;
                    fmin = f(lowerBound);
                    ValidateEvaluation(fmin, lowerBound);
                }
                else
                {
                    upperBound = lowerBound + upperExpansionFactor * range;
                    fmax = f(upperBound);
                    ValidateEvaluation(fmax, upperBound);
                }
                expansionSteps += 1;
            }

            if (expansionSteps == maxExpansionSteps)
                throw new NonConvergenceException();

            while (Math.Abs(fmax - fmin) > 0.5 * fTolerance || Math.Abs(upperBound - lowerBound) > 0.5 * xTolerance)
            {
                double midpoint = 0.5*(upperBound + lowerBound);
                double midval = f(midpoint);
                ValidateEvaluation(midval, midpoint);

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

        static void ValidateEvaluation(double output, double input)
        {
            if (Double.IsInfinity(output) || Double.IsInfinity(output))
            {
                throw new Exception(String.Format("Objective function returned non-finite result: f({0}) = {1}", input, output));
            }
        }
    }
}
