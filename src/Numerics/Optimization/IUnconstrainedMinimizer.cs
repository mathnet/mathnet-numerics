using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public interface IUnconstrainedMinimizer
    {
        MinimizationOutput FindMinimum(IEvaluation objective, Vector<double> initialGuess);
    }
}
