using MathNet.Numerics.Optimization.TrustRegion.Subproblems;

namespace MathNet.Numerics.Optimization.TrustRegion
{
    /// <summary>
    /// Provides factory methods for creating instances of trust region subproblems.
    /// </summary>
    public static class TrustRegionSubproblem
    {
        /// <summary>
        /// Creates an instance of the trust region subproblem using the dogleg algorithm.
        /// </summary>
        /// <returns>An implementation of <see cref="ITrustRegionSubproblem"/> based on the dogleg method.</returns>
        public static ITrustRegionSubproblem DogLeg()
        {
            return new DogLegSubproblem();
        }

        /// <summary>
        /// Creates an instance of the trust region subproblem using the Newton-Conjugate-Gradient algorithm.
        /// </summary>
        /// <returns>An implementation of <see cref="ITrustRegionSubproblem"/> based on the Newton-CG method.</returns>
        public static ITrustRegionSubproblem NewtonCG()
        {
            return new NewtonCGSubproblem();
        }

        /// <summary>
        /// Creates an instance of the trust region subproblem using the Exact algorithm with eigenvalue decomposition.
        /// </summary>
        /// <returns>An implementation of <see cref="ITrustRegionSubproblem"/> based on the Exact method.</returns>
        public static ITrustRegionSubproblem Exact()
        {
            return new ExactSubproblem();
        }
    }
}
