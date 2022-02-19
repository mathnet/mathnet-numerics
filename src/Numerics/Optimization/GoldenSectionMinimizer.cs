// <copyright file="GoldenSectionMinimizer.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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
    public class GoldenSectionMinimizer
    {
        public double XTolerance { get; set; }
        public int MaximumIterations { get; set; }
        public int MaximumExpansionSteps { get; set; }
        public double LowerExpansionFactor { get; set; }
        public double UpperExpansionFactor { get; set; }

        public GoldenSectionMinimizer(double xTolerance=1e-5, int maxIterations=1000, int maxExpansionSteps=10, double lowerExpansionFactor=2.0, double upperExpansionFactor=2.0)
        {
            XTolerance = xTolerance;
            MaximumIterations = maxIterations;
            MaximumExpansionSteps = maxExpansionSteps;
            LowerExpansionFactor = lowerExpansionFactor;
            UpperExpansionFactor = upperExpansionFactor;
        }

        public ScalarMinimizationResult FindMinimum(IScalarObjectiveFunction objective, double lowerBound, double upperBound)
        {
            return Minimum(objective, lowerBound, upperBound, XTolerance, MaximumIterations, MaximumExpansionSteps, LowerExpansionFactor, UpperExpansionFactor);
        }

        public static ScalarMinimizationResult Minimum(IScalarObjectiveFunction objective, double lowerBound, double upperBound, double xTolerance=1e-5, int maxIterations=1000, int maxExpansionSteps=10, double lowerExpansionFactor=2.0, double upperExpansionFactor=2.0)
        {
            if (upperBound <= lowerBound)
            {
                throw new OptimizationException("Lower bound must be lower than upper bound.");
            }

            double middlePointX = lowerBound + (upperBound - lowerBound)/(1 + Constants.GoldenRatio);
            IScalarObjectiveFunctionEvaluation lower = objective.Evaluate(lowerBound);
            IScalarObjectiveFunctionEvaluation middle = objective.Evaluate(middlePointX);
            IScalarObjectiveFunctionEvaluation upper = objective.Evaluate(upperBound);

            ValueChecker(lower.Value);
            ValueChecker(middle.Value);
            ValueChecker(upper.Value);

            int expansionSteps = 0;
            while ((expansionSteps < maxExpansionSteps) && (upper.Value < middle.Value || lower.Value < middle.Value))
            {
                if (lower.Value < middle.Value)
                {
                    lowerBound = 0.5*(upperBound + lowerBound) - lowerExpansionFactor*0.5*(upperBound - lowerBound);
                    lower = objective.Evaluate(lowerBound);
                }

                if (upper.Value < middle.Value)
                {
                    upperBound = 0.5*(upperBound + lowerBound) + upperExpansionFactor*0.5*(upperBound - lowerBound);
                    upper = objective.Evaluate(upperBound);
                }

                middlePointX = lowerBound + (upperBound - lowerBound)/(1 + Constants.GoldenRatio);
                middle = objective.Evaluate(middlePointX);

                expansionSteps += 1;
            }

            if (upper.Value < middle.Value || lower.Value < middle.Value)
            {
                throw new OptimizationException("Lower and upper bounds do not necessarily bound a minimum.");
            }

            int iterations = 0;
            while (Math.Abs(upper.Point - lower.Point) > xTolerance && iterations < maxIterations)
            {
                // Recompute middle point on each iteration to avoid loss of precision
                middlePointX = lower.Point + (upper.Point - lower.Point)/(1 + Constants.GoldenRatio);
                middle = objective.Evaluate(middlePointX);
                ValueChecker(middle.Value);

                double testX = lower.Point + (upper.Point - middle.Point);
                var test = objective.Evaluate(testX);
                ValueChecker(test.Value);

                if (test.Point < middle.Point)
                {
                    if (test.Value > middle.Value)
                    {
                        lower = test;
                    }
                    else
                    {
                        upper = middle;
                    }
                }
                else
                {
                    if (test.Value > middle.Value)
                    {
                        upper = test;
                    }
                    else
                    {
                        lower = middle;
                    }
                }

                iterations += 1;
            }

            if (iterations == maxIterations)
            {
                throw new MaximumIterationsException("Max iterations reached.");
            }

            return new ScalarMinimizationResult(middle, iterations, ExitCondition.BoundTolerance);
        }

        static void ValueChecker(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new Exception("Objective function returned non-finite value.");
            }
        }
    }
}
