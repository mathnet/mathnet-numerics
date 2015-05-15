namespace MathNet.Numerics.Optimization.Implementation
{
    public class LineSearchResult : MinimizationResult
    {
        public double FinalStep { get; private set; }

        public LineSearchResult(IObjectiveFunction functionInfo, int iterations, double finalStep, ExitCondition reasonForExit)
            : base(functionInfo, iterations, reasonForExit)
        {
            FinalStep = finalStep;
        }
    }
}
