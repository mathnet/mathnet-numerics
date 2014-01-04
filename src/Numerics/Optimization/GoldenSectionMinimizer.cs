// <copyright file="GoldenSectionMinimizer.cs" company="Math.NET">
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
    public class GoldenSectionMinimizer
    {
        public double XTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public GoldenSectionMinimizer(double xTolerance = 1e-5, int maxIterations = 1000)
        {
            XTolerance = xTolerance;
            MaximumIterations = maxIterations;
        }

        public MinimizationOutput1D FindMinimum(IObjectiveFunction1D objective, double lowerBound, double upperBound)
        {
            if (!(objective is ObjectiveChecker1D))
                objective = new ObjectiveChecker1D(objective, ValueChecker, null, null);

            double middlePointX = lowerBound + (upperBound - lowerBound)/(1 + GoldenRatio);
            IEvaluation1D lower = objective.Evaluate(lowerBound);
            IEvaluation1D middle = objective.Evaluate(middlePointX);
            IEvaluation1D upper = objective.Evaluate(upperBound);

            if (upperBound <= lowerBound)
                throw new OptimizationException("Lower bound must be lower than upper bound.");

            if (upper.Value < middle.Value || lower.Value < middle.Value)
                throw new OptimizationException("Lower and upper bounds do not necessarily bound a minimum.");

            int iterations = 0;
            while (Math.Abs(upper.Point - lower.Point) > XTolerance && iterations < MaximumIterations)
            {
                double testX = lower.Point + (upper.Point - middle.Point);
                var test = objective.Evaluate(testX);

                if (test.Point < middle.Point)
                {
                    if (test.Value > middle.Value)
                    {
                        lower = test;
                    }
                    else
                    {
                        upper = middle;
                        middle = test;
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
                        middle = test;
                    }
                }

                iterations += 1;
            }

            if (iterations == MaximumIterations)
                throw new MaximumIterationsException("Max iterations reached.");

            return new MinimizationOutput1D(middle, iterations, ExitCondition.BoundTolerance);
        }

        void ValueChecker(IEvaluation1D eval)
        {
            if (Double.IsNaN(eval.Value) || Double.IsInfinity(eval.Value))
                throw new EvaluationException("Objective function returned non-finite value.", eval);
        }

        static readonly double GoldenRatio = (1.0 + Math.Sqrt(5))/2.0;
    }
}
