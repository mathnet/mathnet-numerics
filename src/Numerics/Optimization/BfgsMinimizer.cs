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
        public int MaximumIterations { get; set; }

        public BfgsMinimizer(double gradient_tolerance, int maximum_iterations)
        {
            this.GradientTolerance = gradient_tolerance;
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
            if (this.ExitCriteriaSatisfied(initial_guess, initial_eval.Gradient))
                return new MinimizationOutput(initial_eval, 0);
            
            // Set up line search algorithm
            var line_searcher = new WeakWolfeLineSearch(1e-4, 0.9, 1000);

            // Declare state variables
            IEvaluation candidate_point;
            double step_size;
            Vector<double> gradient, previous_gradient, step, search_direction;
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

            candidate_point = result.FunctionInfoAtMinimum;
            gradient = candidate_point.Gradient;
            previous_gradient = initial_eval.Gradient;
            step = candidate_point.Point - initial_guess;
            step_size = result.FinalStep;

            // Subsequent steps
            int iterations = 1;
            int total_line_search_steps = result.Iterations;
            int iterations_with_nontrivial_line_search = result.Iterations > 0 ? 0 : 1;
            int steepest_descent_resets = 0;
            while (!this.ExitCriteriaSatisfied(candidate_point.Point, candidate_point.Gradient) && iterations < this.MaximumIterations)
            {
                var y = candidate_point.Gradient - previous_gradient;

                double sy = step * y;
                inverse_pseudo_hessian = inverse_pseudo_hessian + ((sy + y * inverse_pseudo_hessian * y) / Math.Pow(sy, 2.0)) * step.OuterProduct(step) - ( (inverse_pseudo_hessian * y.ToColumnMatrix())*step.ToRowMatrix() + step.ToColumnMatrix()*(y.ToRowMatrix() * inverse_pseudo_hessian)) * (1.0 / sy);

               search_direction = -inverse_pseudo_hessian * candidate_point.Gradient;

                if (search_direction * candidate_point.Gradient >= 0)
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
                previous_gradient = candidate_point.Gradient;
                candidate_point = result.FunctionInfoAtMinimum;

                iterations += 1;
            }

            if (iterations == this.MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", this.MaximumIterations));

            return new MinimizationWithLineSearchOutput(candidate_point, iterations, total_line_search_steps, iterations_with_nontrivial_line_search);
        }

        private bool ExitCriteriaSatisfied(Vector<double> candidate_point, Vector<double> gradient)
        {
            return gradient.Norm(2.0) < this.GradientTolerance;
        }

        private void ValidateGradient(Vector<double> gradient, Vector<double> input)
        {
            foreach (var x in gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.");
            }
        }

        private void ValidateObjective(double objective, Vector<double> input)
        {
            if (Double.IsNaN(objective) || Double.IsInfinity(objective))
                throw new EvaluationException("Non-finite objective function returned.");
        }
    }
}
