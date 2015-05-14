using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class MinimizationOutput
    {
        public enum ExitCondition { None, RelativeGradient, LackOfProgress, AbsoluteGradient, WeakWolfeCriteria, BoundTolerance, StrongWolfeCriteria, LackOfFunctionImprovement }

        public Vector<double> MinimizingPoint { get { return FunctionInfoAtMinimum.Point; } }
        public IObjectiveFunction FunctionInfoAtMinimum { get; private set; }
        public int Iterations { get; private set; }
        public ExitCondition ReasonForExit { get; private set; }

        public MinimizationOutput(IObjectiveFunction functionInfo, int iterations, ExitCondition reasonForExit)
        {
            FunctionInfoAtMinimum = functionInfo;
            Iterations = iterations;
            ReasonForExit = reasonForExit;
        }
    }
}
