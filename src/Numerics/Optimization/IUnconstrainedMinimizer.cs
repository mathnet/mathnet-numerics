using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public interface IUnconstrainedMinimizer
    {
        MinimizationOutput FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess);
    }
}
