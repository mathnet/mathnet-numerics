namespace MathNet.Numerics.Optimization.Implementation
{
    public class LineSearchOutput : MinimizationOutput
    {
        public double FinalStep { get; private set; }

        public LineSearchOutput(IEvaluation functionInfo, int iterations, double finalStep, ExitCondition reasonForExit)
            : base(functionInfo, iterations, reasonForExit)
        {
            FinalStep = finalStep;
        }
    }
}
