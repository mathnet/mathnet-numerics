using System;

namespace MathNet.Numerics.Integration.GaussRule
{
    /// <summary>
    /// Creates a Gauss-Kronrod point.
    /// </summary>
    internal static class GaussKronrodPointFactory
    {
        [ThreadStatic]
        private static GaussPoints gaussKronrodPoint;

        /// <summary>
        /// Getter for the GaussKronrodPoint.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Kronrod rule. Precomputed Gauss-Kronrod abscissas/weights for orders 15, 21, 31, 41, 51, 61 are used, otherwise they're calculated on the fly.</param>
        /// <param name="targetAbsoluteTolerance">Required precision to compute the abscissas/weights. 1e-10 is usually fine.</param>
        /// <returns>Object containing the non-negative abscissas/weights, and order.</returns>
        public static GaussPoints GetGaussPoint(int order, double targetAbsoluteTolerance = 1E-10)
        {
            // Try to get the GaussKronrodPoint from the cached static field.
            bool gaussKronrodPointIsCached = gaussKronrodPoint != null && gaussKronrodPoint.Order == order;
            if (!gaussKronrodPointIsCached)
            {
                // Try to find the GaussKronrodPoint in the precomputed dictionary. 
                if (!GaussKronrodPoint.PreComputed.TryGetValue(order, out gaussKronrodPoint))
                {
                    //Not yet supported!
                    gaussKronrodPoint = GaussKronrodPoint.Generate(order, targetAbsoluteTolerance);
                }
            }

            return gaussKronrodPoint;
        }
    }
}
