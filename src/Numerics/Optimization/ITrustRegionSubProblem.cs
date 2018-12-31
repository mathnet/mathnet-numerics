using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public interface ITrustRegionSubproblem
    {
        Vector<double> Pstep { get; }
        double PredictedReduction { get; }
        bool HitBoundary { get; }

        void Solve(IObjectiveModel objective, double radius);
    }
}
