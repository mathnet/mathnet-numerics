namespace MathNet.Numerics.Optimization
{
    public class MinimizationWithLineSearchOutput : MinimizationOutput
    {
        public int TotalLineSearchIterations { get; private set; }
        public int IterationsWithNonTrivialLineSearch { get; private set; }

        public MinimizationWithLineSearchOutput(IEvaluation functionInfo, int iterations, ExitCondition reasonForExit, int totalLineSearchIterations, int iterationsWithNonTrivialLineSearch)
            : base(functionInfo, iterations, reasonForExit)
        {
            TotalLineSearchIterations = totalLineSearchIterations;
            IterationsWithNonTrivialLineSearch = iterationsWithNonTrivialLineSearch;
        }
    }
}
