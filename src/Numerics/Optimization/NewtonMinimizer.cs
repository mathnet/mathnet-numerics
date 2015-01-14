using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using LU = MathNet.Numerics.LinearAlgebra.Factorization.LU<double>;
using MathNet.Numerics.Optimization.Implementation;

namespace MathNet.Numerics.Optimization
{
    public class NewtonMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }
        public bool UseLineSearch { get; set; }

        public NewtonMinimizer(double gradient_tolerance, int maximum_iterations, bool use_line_search = false)
        {
            this.GradientTolerance = gradient_tolerance;
            this.MaximumIterations = maximum_iterations;
            this.UseLineSearch = use_line_search;
        }

        public MinimizationOutput FindMinimum(IObjectiveFunction objective, Vector<double> initial_guess)
        {
            if (!objective.GradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for Newton minimization.");

            if (!objective.HessianSupported)
                throw new IncompatibleObjectiveException("Hessian not supported in objective function, but required for Newton minimization.");

            if (!(objective is ObjectiveChecker))
                objective = new ObjectiveChecker(objective, this.ValidateObjective, this.ValidateGradient, this.ValidateHessian);

            IEvaluation initial_eval = objective.CreateEvaluationObject();
            objective.Evaluate(initial_guess, initial_eval);

            // Check that we're not already done
            if (this.ExitCriteriaSatisfied(initial_guess, initial_eval.Gradient))
                return new MinimizationOutput(initial_eval, 0, MinimizationOutput.ExitCondition.AbsoluteGradient);

            // Set up line search algorithm            
            var line_searcher = new WeakWolfeLineSearch(1e-4, 0.9, 1e-4, max_iterations: 1000);

            // Declare state variables
            IEvaluation candidate_point = initial_eval;
            Vector<double> search_direction;
            LineSearchOutput result;

            // Subsequent steps
            int iterations = 0;
            int total_line_search_steps = 0;
            int iterations_with_nontrivial_line_search = 0;
            int steepest_descent_resets = 0;
            bool tmp_line_search = false;
            while (!this.ExitCriteriaSatisfied(candidate_point.Point, candidate_point.Gradient) && iterations < this.MaximumIterations)
            {
                
                search_direction = candidate_point.Hessian.LU().Solve(-candidate_point.Gradient);

                if (search_direction * candidate_point.Gradient >= 0)
                {
                    search_direction = -candidate_point.Gradient;
                    steepest_descent_resets += 1;
                    tmp_line_search = true;
                }

                if (this.UseLineSearch || tmp_line_search)
                {
                    try
                    {
                        result = line_searcher.FindConformingStep(objective, candidate_point, search_direction, 1.0);
                    }
                    catch (Exception e)
                    {
                        throw new InnerOptimizationException("Line search failed.", e);
                    }
                    iterations_with_nontrivial_line_search += result.Iterations > 0 ? 1 : 0;
                    total_line_search_steps += result.Iterations;
                    candidate_point = result.FunctionInfoAtMinimum;
                }
                else
                {
                    objective.Evaluate(candidate_point.Point + search_direction, candidate_point);
                }

                tmp_line_search = false;

                iterations += 1;
            }

            if (iterations == this.MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", this.MaximumIterations));

            return new MinimizationWithLineSearchOutput(candidate_point, iterations, MinimizationOutput.ExitCondition.AbsoluteGradient, total_line_search_steps, iterations_with_nontrivial_line_search);
        }

        private bool ExitCriteriaSatisfied(Vector<double> candidate_point, Vector<double> gradient)
        {
            return gradient.Norm(2.0) < this.GradientTolerance;
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
