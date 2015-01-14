using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class MinimizationOutput
    {
        public enum ExitCondition { None, RelativeGradient, LackOfProgress, AbsoluteGradient, WeakWolfeCriteria, BoundTolerance, StrongWolfeCriteria, LackOfFunctionImprovement }

        public Vector<double> MinimizingPoint { get { return FunctionInfoAtMinimum.Point; } }
        public IEvaluation FunctionInfoAtMinimum { get; private set; }
        public int Iterations { get; private set; }
        public ExitCondition ReasonForExit { get; private set; }

        public MinimizationOutput(IEvaluation function_info, int iterations, ExitCondition reason_for_exit)
        {
            this.FunctionInfoAtMinimum = function_info;
            this.Iterations = iterations;
            this.ReasonForExit = reason_for_exit;
        }
    }
}
