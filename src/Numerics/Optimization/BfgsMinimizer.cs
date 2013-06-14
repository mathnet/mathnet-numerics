using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{

    public class BfgsMinimizer
    {
        public double GradientTolerance { get; set; }
		public double ParameterTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public BfgsMinimizer(double gradient_tolerance, double parameter_tolerance, int maximum_iterations)
        {
            this.GradientTolerance = gradient_tolerance;
			this.ParameterTolerance = parameter_tolerance;
            this.MaximumIterations = maximum_iterations;
        }

        public MinimizationOutput FindMinimum(IObjectiveFunction objective, Vector<double> initial_guess)
        {
            if (!objective.GradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for BFGS minimization.");

            if (!(objective is ObjectiveChecker))
                objective = new ObjectiveChecker(objective, this.ValidateObjective, this.ValidateGradient, null);

            IEvaluation initial_eval = objective.Evaluate(initial_guess);            
            
            // Check that we're not already done
			ExitCondition current_exit_condition = this.ExitCriteriaSatisfied(initial_eval, null);
            if (current_exit_condition != ExitCondition.None)
                return new MinimizationOutput(initial_eval, 0, current_exit_condition);
            
            // Set up line search algorithm
            var line_searcher = new WeakWolfeLineSearch(1e-4, 0.9,this.ParameterTolerance, max_iterations:1000);

            // Declare state variables
            IEvaluation candidate_point, previous_point;
            double step_size;
            Vector<double> gradient, step, search_direction;
            Matrix<double> inverse_pseudo_hessian;

            // First step
            inverse_pseudo_hessian = Matrix<double>.Build.DiagonalIdentity(initial_guess.Count);
            search_direction = -initial_eval.Gradient;
            step_size = 100 * this.GradientTolerance / (search_direction * search_direction);
                        
            LineSearchOutput result;
            try 
            {
                result = line_searcher.FindConformingStep(objective, initial_eval, search_direction, step_size);
            } 
            catch (Exception e) 
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

			previous_point = initial_eval;
            candidate_point = result.FunctionInfoAtMinimum;
            gradient = candidate_point.Gradient;            
            step = candidate_point.Point - initial_guess;
            step_size = result.FinalStep;

            // Subsequent steps
            int iterations;
            int total_line_search_steps = result.Iterations;
            int iterations_with_nontrivial_line_search = result.Iterations > 0 ? 0 : 1;
            int steepest_descent_resets = 0;
			for (iterations = 1; iterations < this.MaximumIterations; ++iterations)
            {
                var y = candidate_point.Gradient - previous_point.Gradient;

                double sy = step * y;
                inverse_pseudo_hessian = inverse_pseudo_hessian + ((sy + y * inverse_pseudo_hessian * y) / Math.Pow(sy, 2.0)) * step.OuterProduct(step) - ( (inverse_pseudo_hessian * y.ToColumnMatrix())*step.ToRowMatrix() + step.ToColumnMatrix()*(y.ToRowMatrix() * inverse_pseudo_hessian)) * (1.0 / sy);

                search_direction = -inverse_pseudo_hessian * candidate_point.Gradient;

                if (search_direction * candidate_point.Gradient >= -this.GradientTolerance*this.GradientTolerance)
                {
                    search_direction = -candidate_point.Gradient;
                    inverse_pseudo_hessian = Matrix<double>.Build.DiagonalIdentity(initial_guess.Count);
                    steepest_descent_resets += 1;
                }

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

                step_size = result.FinalStep;
                step = result.FunctionInfoAtMinimum.Point - candidate_point.Point;
				previous_point = candidate_point;
                candidate_point = result.FunctionInfoAtMinimum;

				current_exit_condition = this.ExitCriteriaSatisfied(candidate_point, previous_point);
				if (current_exit_condition != ExitCondition.None)
					break;
            }

            if (iterations == this.MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", this.MaximumIterations));

			return new MinimizationWithLineSearchOutput(candidate_point, iterations, current_exit_condition, total_line_search_steps, iterations_with_nontrivial_line_search);
        }

        private ExitCondition ExitCriteriaSatisfied(IEvaluation candidate_point, IEvaluation last_point)
        {
			Vector<double> rel_grad = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(candidate_point.Point.Count);
			double relative_gradient = 0.0;
			double normalizer = Math.Max(Math.Abs(candidate_point.Value),1.0);
			for (int ii = 0; ii < rel_grad.Count; ++ii)
			{
				double tmp = candidate_point.Gradient[ii]*Math.Max(Math.Abs(candidate_point.Point[ii]), 1.0) / normalizer;
				relative_gradient = Math.Max(relative_gradient, Math.Abs(tmp));
			}
			if (relative_gradient < this.GradientTolerance)
			{
				return ExitCondition.RelativeGradient;
			}

			if (last_point != null)
			{
				double most_progress = 0.0;
				for (int ii = 0; ii < candidate_point.Point.Count; ++ii)
				{
					var tmp = Math.Abs(candidate_point.Point[ii] - last_point.Point[ii])/Math.Max(Math.Abs(last_point.Point[ii]),1.0);
					most_progress = Math.Max(most_progress, tmp);
				}
				if ( most_progress < this.ParameterTolerance )
				{
					return ExitCondition.LackOfProgress;
				}
			}

			return ExitCondition.None;
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
    }
}
