using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public class LineSearchOutput : MinimizationOutput
    {
        public double FinalStep { get; private set; }

        public LineSearchOutput(IEvaluation function_info, int iterations, double final_step)
            : base(function_info, iterations)
        {
            this.FinalStep = final_step;
        }
    }
}
