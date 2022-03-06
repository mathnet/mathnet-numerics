// <copyright file="ConjugateGradientMinimizer.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.LineSearch;

namespace MathNet.Numerics.Optimization
{
    public class ConjugateGradientMinimizer : IUnconstrainedMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public ConjugateGradientMinimizer(double gradientTolerance, int maximumIterations)
        {
            GradientTolerance = gradientTolerance;
            MaximumIterations = maximumIterations;
        }

        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            return Minimum(objective, initialGuess, GradientTolerance, MaximumIterations);
        }

        public static MinimizationResult Minimum(IObjectiveFunction objective, Vector<double> initialGuess, double gradientTolerance=1e-8, int maxIterations=1000)
        {
            if (!objective.IsGradientSupported)
            {
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for ConjugateGradient minimization.");
            }

            objective.EvaluateAt(initialGuess);
            var gradient = objective.Gradient;
            ValidateGradient(objective);

            // Check that we're not already done
            if (gradient.Norm(2.0) < gradientTolerance)
            {
                return new MinimizationResult(objective, 0, ExitCondition.AbsoluteGradient);
            }

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.1, 1e-4, 1000);

            // First step
            var steepestDirection = -gradient;
            var searchDirection = steepestDirection;
            double initialStepSize = 100 * gradientTolerance / (gradient * gradient);

            LineSearchResult result;
            try
            {
                result = lineSearcher.FindConformingStep(objective, searchDirection, initialStepSize);
            }
            catch (Exception e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

            objective = result.FunctionInfoAtMinimum;
            ValidateGradient(objective);

            double stepSize = result.FinalStep;

            // Subsequent steps
            int iterations = 1;
            int totalLineSearchSteps = result.Iterations;
            int iterationsWithNontrivialLineSearch = result.Iterations > 0 ? 0 : 1;
            int steepestDescentResets = 0;
            while (objective.Gradient.Norm(2.0) >= gradientTolerance && iterations < maxIterations)
            {
                var previousSteepestDirection = steepestDirection;
                steepestDirection = -objective.Gradient;
                var searchDirectionAdjuster = Math.Max(0, steepestDirection*(steepestDirection - previousSteepestDirection)/(previousSteepestDirection*previousSteepestDirection));
                searchDirection = steepestDirection + searchDirectionAdjuster * searchDirection;
                if (searchDirection * objective.Gradient >= 0)
                {
                    searchDirection = steepestDirection;
                    steepestDescentResets += 1;
                }

                try
                {
                    result = lineSearcher.FindConformingStep(objective, searchDirection, stepSize);
                }
                catch (Exception e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }

                iterationsWithNontrivialLineSearch += result.Iterations == 0 ? 1 : 0;
                totalLineSearchSteps += result.Iterations;
                stepSize = result.FinalStep;
                objective = result.FunctionInfoAtMinimum;
                iterations += 1;
            }

            if (iterations == maxIterations)
            {
                throw new MaximumIterationsException(FormattableString.Invariant($"Maximum iterations ({maxIterations}) reached."));
            }

            return new MinimizationWithLineSearchResult(objective, iterations, ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        static void ValidateGradient(IObjectiveFunctionEvaluation objective)
        {
            foreach (var x in objective.Gradient)
            {
                if (double.IsNaN(x) || double.IsInfinity(x))
                {
                    throw new EvaluationException("Non-finite gradient returned.", objective);
                }
            }
        }

        static void ValidateObjective(IObjectiveFunctionEvaluation objective)
        {
            if (double.IsNaN(objective.Value) || double.IsInfinity(objective.Value))
            {
                throw new EvaluationException("Non-finite objective function returned.", objective);
            }
        }
    }
}
