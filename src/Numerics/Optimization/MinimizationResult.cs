using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class MinimizationResult
    {
        public enum ExitCondition
        {
            None,
            RelativeGradient,
            LackOfProgress,
            AbsoluteGradient,
            WeakWolfeCriteria,
            BoundTolerance,
            StrongWolfeCriteria,
            LackOfFunctionImprovement,
            Converged
        }

        public Vector<double> MinimizingPoint { get { return FunctionInfoAtMinimum.Point; } }
        public IObjectiveFunction FunctionInfoAtMinimum { get; private set; }
        public int Iterations { get; private set; }
        public ExitCondition ReasonForExit { get; private set; }

        public MinimizationResult(IObjectiveFunction functionInfo, int iterations, ExitCondition reasonForExit)
        {
            FunctionInfoAtMinimum = functionInfo;
            Iterations = iterations;
            ReasonForExit = reasonForExit;
        }
    }
}
