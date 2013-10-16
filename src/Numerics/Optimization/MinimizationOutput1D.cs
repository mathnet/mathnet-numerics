using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public class MinimizationOutput1D
    {
        public double MinimizingPoint { get { return FunctionInfoAtMinimum.Point; } }
        public IEvaluation1D FunctionInfoAtMinimum { get; private set; }
        public int Iterations { get; private set; }
        public ExitCondition ReasonForExit { get; private set; }

        public MinimizationOutput1D(IEvaluation1D function_info, int iterations, ExitCondition reason_for_exit)
        {
            this.FunctionInfoAtMinimum = function_info;
            this.Iterations = iterations;
            this.ReasonForExit = reason_for_exit;
        }
    }
}
