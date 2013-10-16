namespace MathNet.Numerics.Optimization
{
    public class MinimizationResult1D
    {
        public double MinimizingPoint { get { return FunctionInfoAtMinimum.Point; } }
        public IEvaluation1D FunctionInfoAtMinimum { get; private set; }
        public int Iterations { get; private set; }
        public MinimizationResult.ExitCondition ReasonForExit { get; private set; }

        public MinimizationResult1D(IEvaluation1D functionInfo, int iterations, MinimizationResult.ExitCondition reasonForExit)
        {
            FunctionInfoAtMinimum = functionInfo;
            Iterations = iterations;
            ReasonForExit = reasonForExit;
        }
    }
}
