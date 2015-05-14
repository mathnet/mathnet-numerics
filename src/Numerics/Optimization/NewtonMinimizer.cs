using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.Implementation;
using LU = MathNet.Numerics.LinearAlgebra.Factorization.LU<double>;

namespace MathNet.Numerics.Optimization
{
    public class NewtonMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }
        public bool UseLineSearch { get; set; }

        public NewtonMinimizer(double gradientTolerance, int maximumIterations, bool useLineSearch = false)
        {
            GradientTolerance = gradientTolerance;
            MaximumIterations = maximumIterations;
            UseLineSearch = useLineSearch;
        }

        public MinimizationOutput FindMinimum(IEvaluation objective, Vector<double> initialGuess)
        {
            if (!objective.GradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for Newton minimization.");

            if (!objective.HessianSupported)
                throw new IncompatibleObjectiveException("Hessian not supported in objective function, but required for Newton minimization.");

            if (!(objective is CheckedEvaluation))
                objective = new CheckedEvaluation(objective, ValidateObjective, ValidateGradient, ValidateHessian);

            IEvaluation initialEval = objective.CreateNew();
            initialEval.Point = initialGuess;

            // Check that we're not already done
            if (ExitCriteriaSatisfied(initialGuess, initialEval.Gradient))
                return new MinimizationOutput(initialEval, 0, MinimizationOutput.ExitCondition.AbsoluteGradient);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.9, 1e-4, maxIterations: 1000);

            // Declare state variables
            IEvaluation candidatePoint = initialEval;

            // Subsequent steps
            int iterations = 0;
            int totalLineSearchSteps = 0;
            int iterationsWithNontrivialLineSearch = 0;
            bool tmpLineSearch = false;
            while (!ExitCriteriaSatisfied(candidatePoint.Point, candidatePoint.Gradient) && iterations < MaximumIterations)
            {
                var searchDirection = candidatePoint.Hessian.LU().Solve(-candidatePoint.Gradient);
                if (searchDirection * candidatePoint.Gradient >= 0)
                {
                    searchDirection = -candidatePoint.Gradient;
                    tmpLineSearch = true;
                }

                if (UseLineSearch || tmpLineSearch)
                {
                    LineSearchOutput result;
                    try
                    {
                        result = lineSearcher.FindConformingStep(objective, candidatePoint, searchDirection, 1.0);
                    }
                    catch (Exception e)
                    {
                        throw new InnerOptimizationException("Line search failed.", e);
                    }
                    iterationsWithNontrivialLineSearch += result.Iterations > 0 ? 1 : 0;
                    totalLineSearchSteps += result.Iterations;
                    candidatePoint = result.FunctionInfoAtMinimum;
                }
                else
                {
                    candidatePoint.Point = candidatePoint.Point + searchDirection;
                }

                tmpLineSearch = false;

                iterations += 1;
            }

            if (iterations == MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));

            return new MinimizationWithLineSearchOutput(candidatePoint, iterations, MinimizationOutput.ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        private bool ExitCriteriaSatisfied(Vector<double> candidatePoint, Vector<double> gradient)
        {
            return gradient.Norm(2.0) < GradientTolerance;
        }

        private void ValidateGradient(IEvaluation eval)
        {
            foreach (var x in eval.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.", eval);
            }
        }

        private void ValidateObjective(IEvaluation eval)
        {
            if (Double.IsNaN(eval.Value) || Double.IsInfinity(eval.Value))
                throw new EvaluationException("Non-finite objective function returned.", eval);
        }

        private void ValidateHessian(IEvaluation eval)
        {
            for (int ii = 0; ii < eval.Hessian.RowCount; ++ii)
            {
                for (int jj = 0; jj < eval.Hessian.ColumnCount; ++jj)
                {
                    if (Double.IsNaN(eval.Hessian[ii, jj]) || Double.IsInfinity(eval.Hessian[ii, jj]))
                        throw new EvaluationException("Non-finite Hessian returned.", eval);
                }
            }
        }
    }
}
