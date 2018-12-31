using MathNet.Numerics.Optimization.Subproblems;

namespace MathNet.Numerics.Optimization
{
    public static class TrustRegionSubproblem
    {
        public static ITrustRegionSubproblem Quadratic()
        {
            return new QuadraticSubproblem();
        }
    }
}
