// <copyright file="ConjugateGradientMinimizer.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class ConjugateGradientMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public ConjugateGradientMinimizer(double gradientTolerance, int maximumIterations)
        {
            GradientTolerance = gradientTolerance;
            MaximumIterations = maximumIterations;
        }

        public MinimizationOutput FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (!objective.GradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for ConjugateGradient minimization.");

            if (!(objective is ObjectiveChecker))
                objective = new ObjectiveChecker(objective, ValidateObjective, ValidateGradient, null);

            IEvaluation initialEval = objective.Evaluate(initialGuess);
            var gradient = initialEval.Gradient;

            // Check that we're not already done
            if (ExitCriteriaSatisfied(initialGuess, gradient))
                return new MinimizationOutput(initialEval, 0, ExitCondition.AbsoluteGradient);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.1, 1e-4, maxIterations: 1000);

            // Declare state variables

            // First step
            Vector<double> steepestDirection = -gradient;
            Vector<double> searchDirection = steepestDirection;
            double initialStepSize = 100*GradientTolerance/(gradient*gradient);
            LineSearchOutput result;
            try
            {
                result = lineSearcher.FindConformingStep(objective, initialEval, searchDirection, initialStepSize);
            }
            catch (Exception e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

            IEvaluation candidatePoint = result.FunctionInfoAtMinimum;

            double stepSize = result.FinalStep;

            // Subsequent steps
            int iterations = 1;
            int totalLineSearchSteps = result.Iterations;
            int iterationsWithNontrivialLineSearch = result.Iterations > 0 ? 0 : 1;
            while (!ExitCriteriaSatisfied(candidatePoint.Point, candidatePoint.Gradient) && iterations < MaximumIterations)
            {
                Vector<double> previousSteepestDirection = steepestDirection;
                steepestDirection = -candidatePoint.Gradient;
                var searchDirectionAdjuster = Math.Max(0, steepestDirection*(steepestDirection - previousSteepestDirection)/(previousSteepestDirection*previousSteepestDirection));

                //double prev_grad_mag = previous_steepest_direction*previous_steepest_direction;
                //double grad_overlap = steepest_direction*previous_steepest_direction;
                //double search_grad_overlap = candidate_point.Gradient*search_direction;

                //if (iterations % initial_guess.Count == 0 || (Math.Abs(grad_overlap) >= 0.2 * prev_grad_mag) || (-2 * prev_grad_mag >= search_grad_overlap) || (search_grad_overlap >= -0.2 * prev_grad_mag))
                //    search_direction = steepest_direction;
                //else
                //    search_direction = steepest_direction + search_direction_adjuster * search_direction;

                searchDirection = steepestDirection + searchDirectionAdjuster*searchDirection;
                if (searchDirection*candidatePoint.Gradient >= 0)
                {
                    searchDirection = steepestDirection;
                }

                try
                {
                    result = lineSearcher.FindConformingStep(objective, candidatePoint, searchDirection, stepSize);
                }
                catch (Exception e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }

                iterationsWithNontrivialLineSearch += result.Iterations > 0 ? 1 : 0;
                totalLineSearchSteps += result.Iterations;

                stepSize = result.FinalStep;
                candidatePoint = result.FunctionInfoAtMinimum;

                iterations += 1;
            }

            if (iterations == MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));

            return new MinimizationWithLineSearchOutput(candidatePoint, iterations, ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        bool ExitCriteriaSatisfied(Vector<double> candidatePoint, Vector<double> gradient)
        {
            return gradient.Norm(2.0) < GradientTolerance;
        }

        void ValidateGradient(IEvaluation eval)
        {
            foreach (var x in eval.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.", eval);
            }
        }

        void ValidateObjective(IEvaluation eval)
        {
            if (Double.IsNaN(eval.Value) || Double.IsInfinity(eval.Value))
                throw new EvaluationException("Non-finite objective function returned.", eval);
        }
    }
}
