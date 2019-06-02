using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.TrustRegion
{
    public interface ITrustRegionSubproblem
    {
        Vector<double> Pstep { get; }
        bool HitBoundary { get; }

        void Solve(IObjectiveModel objective, double radius);
    }
}
