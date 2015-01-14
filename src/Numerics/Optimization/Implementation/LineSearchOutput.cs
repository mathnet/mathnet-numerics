using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization.Implementation
{
    public class LineSearchOutput : MinimizationOutput
    {
        public double FinalStep { get; private set; }

        public LineSearchOutput(IEvaluation function_info, int iterations, double final_step, ExitCondition reason_for_exit)
            : base(function_info, iterations, reason_for_exit)
        {
            this.FinalStep = final_step;
        }
    }
}
