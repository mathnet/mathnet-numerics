using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class ConjugateGradientMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public ConjugateGradientMinimizer(double gradient_tolerance, int maximum_iterations)
        {
            this.GradientTolerance = gradient_tolerance;
            this.MaximumIterations = maximum_iterations;
        }

        public MinimizationOutput FindMinimum(IObjectiveFunction objective, Vector<double> initial_guess)
        {
            if (!objective.GradientSupported)
                throw new Exception("Gradient not supported in objective function, but required for ConjugateGradient minimization.");

            IEvaluation initial_eval = objective.Evaluate(initial_guess);
            var gradient = initial_eval.Gradient;
            this.ValidateGradient(gradient, initial_guess);
            
            // Check that we're not already done
            if (this.ExitCriteriaSatisfied(initial_guess, gradient))
                return new MinimizationOutput(initial_eval, 0);
            
            // Set up line search algorithm
            var line_searcher = new WeakWolfeLineSearch(1e-4, 0.1,1000);

            // Declare state variables
            IEvaluation candidate_point;
            Vector<double> steepest_direction, previous_steepest_direction, search_direction;

            // First step
            steepest_direction = -gradient;
            search_direction = steepest_direction;
            double initial_step_size = 100 * this.GradientTolerance / (gradient * gradient);
            var result = line_searcher.FindConformingStep(objective, initial_eval, search_direction, initial_step_size);
            candidate_point = result.FunctionInfoAtMinimum;
            this.ValidateGradient(candidate_point.Gradient, candidate_point.Point);
            
			double step_size = (candidate_point.Point - initial_guess).Norm(2.0);
            // Subsequent steps
            int iterations = 1;
            int total_line_search_steps = result.Iterations;
            int no_line_search_iterations = result.Iterations > 0 ? 0 : 1;
            int steepest_descent_resets = 0;
            while (!this.ExitCriteriaSatisfied(candidate_point.Point, candidate_point.Gradient) && iterations < this.MaximumIterations)
            {
                previous_steepest_direction = steepest_direction;
                steepest_direction = -candidate_point.Gradient;
                var search_direction_adjuster = Math.Max(0,steepest_direction * (steepest_direction - previous_steepest_direction) / (previous_steepest_direction * previous_steepest_direction));
                
                //double prev_grad_mag = previous_steepest_direction*previous_steepest_direction;
                //double grad_overlap = steepest_direction*previous_steepest_direction;
                //double search_grad_overlap = candidate_point.Gradient*search_direction;

                //if (iterations % initial_guess.Count == 0 || (Math.Abs(grad_overlap) >= 0.2 * prev_grad_mag) || (-2 * prev_grad_mag >= search_grad_overlap) || (search_grad_overlap >= -0.2 * prev_grad_mag))
                //    search_direction = steepest_direction;
                //else 
                //    search_direction = steepest_direction + search_direction_adjuster * search_direction;

                search_direction = steepest_direction + search_direction_adjuster * search_direction;
                if (search_direction * candidate_point.Gradient >= 0)
                {
                    search_direction = steepest_direction;
                    steepest_descent_resets += 1;
                }
                
                result = line_searcher.FindConformingStep(objective, candidate_point, search_direction, step_size);

                no_line_search_iterations += result.Iterations == 0 ? 1 : 0;
                total_line_search_steps += result.Iterations;

				step_size = (result.FunctionInfoAtMinimum.Point - candidate_point.Point).Norm (2.0);
                candidate_point = result.FunctionInfoAtMinimum;

                iterations += 1;
            }

            return new MinimizationWithLineSearchOutput(candidate_point, iterations, total_line_search_steps, no_line_search_iterations);
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
