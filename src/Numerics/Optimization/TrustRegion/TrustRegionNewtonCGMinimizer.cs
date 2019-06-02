namespace MathNet.Numerics.Optimization.TrustRegion
{
    public sealed class TrustRegionNewtonCGMinimizer : TrustRegionMinimizerBase
    {
        /// <summary>
        /// Non-linear least square fitting by the trust region Newton-Conjugate-Gradient algorithm.
        /// </summary>
        public TrustRegionNewtonCGMinimizer(double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-8, int maximumIterations = -1)
            : base(TrustRegionSubproblem.NewtonCG(), gradientTolerance, stepTolerance, functionTolerance, radiusTolerance, maximumIterations)
        { }
    }
}
