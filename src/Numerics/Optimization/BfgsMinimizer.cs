using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.LineSearch;

namespace MathNet.Numerics.Optimization
{
    public class BfgsMinimizer
    {
        public double GradientTolerance { get; set; }
		public double ParameterTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public BfgsMinimizer(double gradientTolerance, double parameterTolerance, int maximumIterations=1000)
        {
            GradientTolerance = gradientTolerance;
			ParameterTolerance = parameterTolerance;
            MaximumIterations = maximumIterations;
        }

        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (!objective.IsGradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for BFGS minimization.");

            objective.EvaluateAt(initialGuess);
            ValidateGradient(objective);

            var initial = objective.Fork();

            // Check that we're not already done
            MinimizationResult.ExitCondition currentExitCondition = ExitCriteriaSatisfied(objective, null);
            if (currentExitCondition != MinimizationResult.ExitCondition.None)
                return new MinimizationResult(objective, 0, currentExitCondition);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.9, Math.Max(ParameterTolerance, 1e-10), 1000);

            // First step
            var inversePseudoHessian = CreateMatrix.DenseIdentity<double>(initialGuess.Count);
            var searchDirection = -objective.Gradient;
            var stepSize = 100 * GradientTolerance / (searchDirection * searchDirection);

            var previousPoint = objective.Point;
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
            Matrix<double> I = CreateMatrix.DiagonalIdentity<double>(initialGuess.Count);
            int iterations;
            int totalLineSearchSteps = result.Iterations;
            int iterationsWithNontrivialLineSearch = result.Iterations > 0 ? 0 : 1;
			for (iterations = 1; iterations < MaximumIterations; ++iterations)
            {
                var y = objective.Gradient - previousGradient;

                double sy = step * y;
                inversePseudoHessian = inversePseudoHessian + ((sy + y * inversePseudoHessian * y) / Math.Pow(sy, 2.0)) * step.OuterProduct(step) - ( (inversePseudoHessian * y.ToColumnMatrix())*step.ToRowMatrix() + step.ToColumnMatrix()*(y.ToRowMatrix() * inversePseudoHessian)) * (1.0 / sy);
                searchDirection = -inversePseudoHessian * objective.Gradient;

                if (searchDirection * objective.Gradient >= 0.0)
                {
                    searchDirection = -objective.Gradient;
                    inversePseudoHessian = CreateMatrix.DenseIdentity<double>(initialGuess.Count);
                }

                previousGradient = objective.Gradient;
                previousPoint = objective.Point;

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

                currentExitCondition = ExitCriteriaSatisfied(objective, previousPoint);
                if (currentExitCondition != MinimizationResult.ExitCondition.None)
                    break;
            }

            if (iterations == MaximumIterations && currentExitCondition == MinimizationResult.ExitCondition.None)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));

            return new MinimizationWithLineSearchResult(objective, iterations, MinimizationResult.ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        private MinimizationResult.ExitCondition ExitCriteriaSatisfied(IObjectiveFunction candidatePoint, Vector<double> lastPoint)
        {
            Vector<double> relGrad = new LinearAlgebra.Double.DenseVector(candidatePoint.Point.Count);
			double relativeGradient = 0.0;
            double normalizer = Math.Max(Math.Abs(candidatePoint.Value), 1.0);
			for (int ii = 0; ii < relGrad.Count; ++ii)
			{
                double tmp = candidatePoint.Gradient[ii]*Math.Max(Math.Abs(candidatePoint.Point[ii]), 1.0) / normalizer;
				relativeGradient = Math.Max(relativeGradient, Math.Abs(tmp));
			}
			if (relativeGradient < GradientTolerance)
			{
				return MinimizationResult.ExitCondition.RelativeGradient;
			}

            if (lastPoint != null)
			{
				double mostProgress = 0.0;
                for (int ii = 0; ii < candidatePoint.Point.Count; ++ii)
				{
                    var tmp = Math.Abs(candidatePoint.Point[ii] - lastPoint[ii])/Math.Max(Math.Abs(lastPoint[ii]), 1.0);
					mostProgress = Math.Max(mostProgress, tmp);
				}
				if ( mostProgress < ParameterTolerance )
				{
					return MinimizationResult.ExitCondition.LackOfProgress;
				}
			}

			return MinimizationResult.ExitCondition.None;
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
