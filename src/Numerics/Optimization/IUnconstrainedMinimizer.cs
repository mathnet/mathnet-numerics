using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public interface IUnconstrainedMinimizer
    {
        MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess);
    }
}
