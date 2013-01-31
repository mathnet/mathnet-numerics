using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class MinimizationOutput
    {
        public Vector<double> MinimizingPoint { get { return FunctionInfoAtMinimum.Point; } }
        public IEvaluation FunctionInfoAtMinimum { get; private set; }
        public int Iterations { get; private set; }

        public MinimizationOutput(IEvaluation function_info, int iterations)
        {
            this.FunctionInfoAtMinimum = function_info;
            this.Iterations = iterations;
        }
    }
}
