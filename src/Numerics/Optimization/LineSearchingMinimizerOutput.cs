using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public class MinimizationWithLineSearchOutput : MinimizationOutput
    {
        public int TotalLineSearchIterations { get; private set; }
        public int IterationsWithNonTrivialLineSearch { get; private set; }

        public MinimizationWithLineSearchOutput(IEvaluation function_info, int iterations, int total_line_search_iterations, int iterations_with_non_trivial_line_search)
            : base(function_info, iterations)
        {
            this.TotalLineSearchIterations = total_line_search_iterations;
            this.IterationsWithNonTrivialLineSearch = iterations_with_non_trivial_line_search;
        }
    }
}
