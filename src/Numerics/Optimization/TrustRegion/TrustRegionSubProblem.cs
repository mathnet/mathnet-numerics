using MathNet.Numerics.Optimization.TrustRegion.Subproblems;

namespace MathNet.Numerics.Optimization.TrustRegion
{
    public static class TrustRegionSubproblem
    {
        public static ITrustRegionSubproblem DogLeg()
        {
            return new DogLegSubproblem();
        }

        public static ITrustRegionSubproblem NewtonCG()
        {
            return new NewtonCGSubproblem();
        }
    }
}
