// <copyright file="BisectionRootFinder.cs" company="Math.NET">
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

namespace MathNet.Numerics.Optimization
{
    public class BisectionRootFinder
    {
        public double ObjectiveTolerance { get; set; }
        public double XTolerance { get; set; }
        public double LowerExpansionFactor { get; set; }
        public double UpperExpansionFactor { get; set; }
        public int MaxExpansionSteps { get; set; }

        public BisectionRootFinder(double objectiveTolerance = 1e-5, double xTolerance = 1e-5, double lowerExpansionFactor = -1.0, double upperExpansionFactor = -1.0, int maxExpansionSteps = 10)
        {
            ObjectiveTolerance = objectiveTolerance;
            XTolerance = xTolerance;
            LowerExpansionFactor = lowerExpansionFactor;
            UpperExpansionFactor = upperExpansionFactor;
            MaxExpansionSteps = maxExpansionSteps;
        }

        public double FindRoot(Func<double, double> objectiveFunction, double lowerBound, double upperBound)
        {
            double lowerVal = objectiveFunction(lowerBound);
            double upperVal = objectiveFunction(upperBound);

            if (lowerVal == 0.0)
                return lowerBound;
            if (upperVal == 0.0)
                return upperBound;

            ValidateEvaluation(lowerVal, lowerBound);
            ValidateEvaluation(upperVal, upperBound);

            if (Math.Sign(lowerVal) == Math.Sign(upperVal) && LowerExpansionFactor <= 1.0 && UpperExpansionFactor <= 1.0)
                throw new Exception("Bounds do not necessarily span a root, and StepExpansionFactor is not set to expand the interval in this case.");

            int expansionSteps = 0;
            while (Math.Sign(lowerVal) == Math.Sign(upperVal) && expansionSteps < MaxExpansionSteps)
            {
                double midpoint = 0.5*(upperBound + lowerBound);
                double range = upperBound - lowerBound;
                if (UpperExpansionFactor <= 0.0 || (LowerExpansionFactor > 0.0 && Math.Abs(lowerVal) < Math.Abs(upperVal)))
                {
                    lowerBound = upperBound - LowerExpansionFactor*range;
                    lowerVal = objectiveFunction(lowerBound);
                    ValidateEvaluation(lowerVal, lowerBound);
                }
                else
                {
                    upperBound = lowerBound + UpperExpansionFactor*range;
                    upperVal = objectiveFunction(upperBound);
                    ValidateEvaluation(upperVal, upperBound);
                }
                expansionSteps += 1;
            }

            if (Math.Sign(lowerVal) == Math.Sign(upperVal) && expansionSteps == MaxExpansionSteps)
                throw new MaximumIterationsException("Could not bound root in maximum expansion iterations.");

            while (Math.Abs(upperVal - lowerVal) > 0.5*ObjectiveTolerance || Math.Abs(upperBound - lowerBound) > 0.5*XTolerance)
            {
                double midpoint = 0.5*(upperBound + lowerBound);
                double midval = objectiveFunction(midpoint);
                ValidateEvaluation(midval, midpoint);

                if (Math.Sign(midval) == Math.Sign(lowerVal))
                {
                    lowerBound = midpoint;
                    lowerVal = midval;
                }
                else if (Math.Sign(midval) == Math.Sign(upperVal))
                {
                    upperBound = midpoint;
                    upperVal = midval;
                }
                else
                {
                    return midpoint;
                }
            }

            return 0.5*(lowerBound + upperBound);
        }

        void ValidateEvaluation(double output, double input)
        {
            if (!IsFinite(output))
                throw new Exception(String.Format("Objective function returned non-finite result: f({0}) = {1}", input, output));
        }

        static bool IsFinite(double x)
        {
            return !(Double.IsInfinity(x) || Double.IsNaN(x));
        }
    }
}
