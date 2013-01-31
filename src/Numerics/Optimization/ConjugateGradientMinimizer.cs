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
            this.GradientTolerance = gradientTolerance;
            this.MaximumIterations = maximumIterations;
        }

        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (!objective.IsGradientSupported)
                throw new Exception("Gradient not supported in objective function, but required for ConjugateGradient minimization.");

            objective.EvaluateAt(initialGuess);
            var gradient = objective.Gradient;
            ValidateGradient(gradient, initialGuess);

            // Check that we're not already done
            if (ExitCriteriaSatisfied(initialGuess, gradient))
                return new MinimizationResult(objective, 0, MinimizationResult.ExitCondition.AbsoluteGradient);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.1, 1e-4, 1000);

            // First step
            var steepestDirection = -gradient;
            var searchDirection = steepestDirection;
            double initialStepSize = 100 * GradientTolerance / (gradient * gradient);
            var result = lineSearcher.FindConformingStep(objective, searchDirection, initialStepSize);
            objective = result.FunctionInfoAtMinimum;
            ValidateGradient(objective.Gradient, objective.Point);

            double stepSize = (objective.Point - initialGuess).Norm(2.0);
            // Subsequent steps
            int iterations = 1;
            int totalLineSearchSteps = result.Iterations;
            int noLineSearchIterations = result.Iterations > 0 ? 0 : 1;
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


                result = lineSearcher.FindConformingStep(objective, searchDirection, stepSize);

                noLineSearchIterations += result.Iterations == 0 ? 1 : 0;
                totalLineSearchSteps += result.Iterations;
				stepSize = (result.FunctionInfoAtMinimum.Point - objective.Point).Norm(2.0);

                objective = result.FunctionInfoAtMinimum;
                iterations += 1;
            }

            return new MinimizationWithLineSearchResult(objective, iterations, MinimizationResult.ExitCondition.AbsoluteGradient, totalLineSearchSteps, noLineSearchIterations);
        }

        private bool ExitCriteriaSatisfied(Vector<double> candidatePoint, Vector<double> gradient)
        {
            return gradient.Norm(2.0) < GradientTolerance;
        }

        private void ValidateGradient(Vector<double> gradient, Vector<double> input)
        {
            foreach (var x in gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new Exception("Non-finite gradient returned.");
            }
        }

        private void ValidateObjective(double objective, Vector<double> input)
        {
            if (Double.IsNaN(objective) || Double.IsInfinity(objective))
                throw new Exception("Non-finite objective function returned.");
        }
    }
}
