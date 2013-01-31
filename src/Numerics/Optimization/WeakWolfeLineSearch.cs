using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class WeakWolfeLineSearch
    {
        public double C1 { get; set; }
        public double C2 { get; set; }
		public int MaximumIterations { get; set; }

        public WeakWolfeLineSearch(double c1, double c2, int max_iterations=10)
        {
            this.C1 = c1;
            this.C2 = c2;
			this.MaximumIterations = max_iterations;
        }

        // Implemented following http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf
        public LineSearchOutput FindConformingStep(IObjectiveFunction objective, IEvaluation starting_point, Vector<double> search_direction, double initial_step)
        {
            double lower_bound = 0.0;
            double upper_bound = Double.PositiveInfinity;
            double step = initial_step;

            double initial_value = starting_point.Value;
            Vector<double> initial_gradient = starting_point.Gradient;
            
            double initial_dd = search_direction*initial_gradient;

            int ii;
            IEvaluation candidate_eval = null;
            for (ii = 0; ii < this.MaximumIterations; ++ii)
            {
                candidate_eval = objective.Evaluate(starting_point.Point + search_direction * step);

                double step_dd = search_direction * candidate_eval.Gradient;

                if (candidate_eval.Value > initial_value + this.C1 * step * initial_dd)
                {
                    upper_bound = step;
                    step = 0.5 * (lower_bound + upper_bound);
                } 
                else if (step_dd < this.C2*initial_dd)
                {
                    lower_bound = step;
                    step = Double.IsPositiveInfinity(upper_bound) ? 2 * lower_bound : 0.5 * (lower_bound + upper_bound);
                }
                else 
                {
                    break;
                }
            }
                       
            if (ii == this.MaximumIterations)
                throw new Exception("Line search failed with max iterations.  Function is likely unbounded in search direction.");
            else
                return new LineSearchOutput(candidate_eval, ii, step);
        }

        private bool Conforms(IEvaluation starting_point, Vector<double> search_direction, double step, IEvaluation ending_point)
        {

            bool sufficient_decrease = ending_point.Value <= starting_point.Value + this.C1 * step * (starting_point.Gradient * search_direction);
            bool not_too_steep = ending_point.Gradient * search_direction >= this.C2 * starting_point.Gradient * search_direction;

            return step > 0 && sufficient_decrease && not_too_steep;
        }

    }
}
