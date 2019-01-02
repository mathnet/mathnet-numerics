using MathNet.Numerics.LinearAlgebra;
using System;

namespace MathNet.Numerics.Optimization
{
    public sealed class TrustRegionDogLegMinimizer : TrustRegionMinimizerBase
    {
        public TrustRegionDogLegMinimizer(double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-8, int maximumIterations = -1)
            : base(TrustRegionSubproblem.DogLeg(), gradientTolerance, stepTolerance, functionTolerance, radiusTolerance, maximumIterations)
        { }
    }
}
