namespace MathNet.Numerics.Optimization.TrustRegion
{
    /// <summary>
    /// Implements nonlinear least squares fitting using the trust region dogleg algorithm.
    /// This class inherits from <see cref="TrustRegionMinimizerBase"/>.
    /// </summary>
    public sealed class TrustRegionDogLegMinimizer : TrustRegionMinimizerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrustRegionDogLegMinimizer"/> class using the trust region dogleg algorithm.
        /// </summary>
        /// <param name="gradientTolerance">The tolerance for the infinity norm of the gradient. Default is 1E-8.</param>
        /// <param name="stepTolerance">The tolerance for the parameter update step size. Default is 1E-8.</param>
        /// <param name="functionTolerance">The tolerance for the function value (residual sum of squares). Default is 1E-8.</param>
        /// <param name="radiusTolerance">The tolerance for the trust region radius. Default is 1E-8.</param>
        /// <param name="maximumIterations">The maximum number of iterations. Default is -1 (unlimited).</param>
        public TrustRegionDogLegMinimizer(double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-8, int maximumIterations = -1)
            : base(TrustRegionSubproblem.DogLeg(), gradientTolerance, stepTolerance, functionTolerance, radiusTolerance, maximumIterations)
        { }
    }
}
