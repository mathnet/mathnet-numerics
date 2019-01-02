using MathNet.Numerics.Optimization.Subproblems;

namespace MathNet.Numerics.Optimization
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
