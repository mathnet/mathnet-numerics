using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.LineSearch;

namespace MathNet.Numerics.Optimization
{
    public class BfgsMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public BfgsMinimizer(double gradientTolerance, int maximumIterations)
        {
            GradientTolerance = gradientTolerance;
            MaximumIterations = maximumIterations;
        }

        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (!objective.IsGradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for BFGS minimization.");

            objective.EvaluateAt(initialGuess);

            ValidateGradient(objective);

            // Check that we're not already done
            if (ExitCriteriaSatisfied(objective.Point, objective.Gradient))
                return new MinimizationResult(objective, 0, MinimizationResult.ExitCondition.AbsoluteGradient);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.9, 1000);

            // First step
            var inversePseudoHessian = CreateMatrix.DenseIdentity<double>(initialGuess.Count);
            var searchDirection = -objective.Gradient;
            var stepSize = 100 * GradientTolerance / (searchDirection * searchDirection);

            var previousGradient = objective.Gradient;

            LineSearchResult result;
            try
            {
                result = lineSearcher.FindConformingStep(objective, searchDirection, stepSize);
            }
            catch (Exception e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

            objective = result.FunctionInfoAtMinimum;
            ValidateGradient(objective);

            var gradient = objective.Gradient;
            var step = objective.Point - initialGuess;
            stepSize = result.FinalStep;

            // Subsequent steps
            int iterations = 1;
            int totalLineSearchSteps = result.Iterations;
            int iterationsWithNontrivialLineSearch = result.Iterations > 0 ? 0 : 1;
            while (!ExitCriteriaSatisfied(objective.Point, objective.Gradient) && iterations < MaximumIterations)
            {
                var y = objective.Gradient - previousGradient;

                double sy = step * y;
                inversePseudoHessian = inversePseudoHessian + ((sy + y * inversePseudoHessian * y) / Math.Pow(sy, 2.0)) * step.OuterProduct(step) - ( (inversePseudoHessian * y.ToColumnMatrix())*step.ToRowMatrix() + step.ToColumnMatrix()*(y.ToRowMatrix() * inversePseudoHessian)) * (1.0 / sy);

                searchDirection = -inversePseudoHessian * objective.Gradient;

                if (searchDirection * objective.Gradient >= 0)
                {
                    searchDirection = -objective.Gradient;
                    inversePseudoHessian = CreateMatrix.DenseIdentity<double>(initialGuess.Count);
                }

                previousGradient = objective.Gradient;
                var previousPoint = objective.Point;

                try
                {
                    result = lineSearcher.FindConformingStep(objective, searchDirection, 1.0);
                }
                catch (Exception e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }

                iterationsWithNontrivialLineSearch += result.Iterations > 0 ? 1 : 0;
                totalLineSearchSteps += result.Iterations;
                stepSize = result.FinalStep;
                step = result.FunctionInfoAtMinimum.Point - previousPoint;
                objective = result.FunctionInfoAtMinimum;

                iterations += 1;
            }

            if (iterations == this.MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));

            return new MinimizationWithLineSearchResult(objective, iterations, MinimizationResult.ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        private bool ExitCriteriaSatisfied(Vector<double> candidatePoint, Vector<double> gradient)
        {
            return gradient.Norm(2.0) < this.GradientTolerance;
        }

        private void ValidateGradient(IObjectiveFunction objective)
        {
            foreach (var x in objective.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.", objective);
            }
        }

        private void ValidateObjective(IObjectiveFunction objective)
        {
            if (Double.IsNaN(objective.Value) || Double.IsInfinity(objective.Value))
                throw new EvaluationException("Non-finite objective function returned.", objective);
        }
    }
}
