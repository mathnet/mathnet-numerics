namespace MathNet.Numerics.Optimization.TrustRegion
{
    public sealed class TrustRegionDogLegMinimizer : TrustRegionMinimizerBase
    {
        /// <summary>
        /// Non-linear least square fitting by the trust region dogleg algorithm.
        /// </summary>
        public TrustRegionDogLegMinimizer(double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-8, int maximumIterations = -1)
            : base(TrustRegionSubproblem.DogLeg(), gradientTolerance, stepTolerance, functionTolerance, radiusTolerance, maximumIterations)
        { }
    }
}
