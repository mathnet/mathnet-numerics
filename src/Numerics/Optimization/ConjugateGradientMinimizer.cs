using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.LineSearch;

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

        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (!objective.IsGradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for ConjugateGradient minimization.");

            objective.EvaluateAt(initialGuess);
            var gradient = objective.Gradient;
            ValidateGradient(objective);

            // Check that we're not already done
            if (ExitCriteriaSatisfied(initialGuess, gradient))
                return new MinimizationResult(objective, 0, MinimizationResult.ExitCondition.AbsoluteGradient);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.1, 1e-4, 1000);

            // First step
            var steepestDirection = -gradient;
            var searchDirection = steepestDirection;
            double initialStepSize = 100 * GradientTolerance / (gradient * gradient);

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
            while (!ExitCriteriaSatisfied(objective.Point, objective.Gradient) && iterations < MaximumIterations)
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

            if (iterations == MaximumIterations)
            {
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));
            }

            return new MinimizationWithLineSearchResult(objective, iterations, MinimizationResult.ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        bool ExitCriteriaSatisfied(Vector<double> candidatePoint, Vector<double> gradient)
        {
            return gradient.Norm(2.0) < GradientTolerance;
        }

        void ValidateGradient(IObjectiveFunction objective)
        {
            foreach (var x in objective.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.", objective);
            }
        }

        void ValidateObjective(IObjectiveFunction objective)
        {
            if (Double.IsNaN(objective.Value) || Double.IsInfinity(objective.Value))
                throw new EvaluationException("Non-finite objective function returned.", objective);
        }
    }
}
