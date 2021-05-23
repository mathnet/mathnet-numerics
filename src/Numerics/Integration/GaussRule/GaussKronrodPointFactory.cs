using System;

namespace MathNet.Numerics.Integration.GaussRule
{
    /// <summary>
    /// Creates a Gauss-Kronrod point.
    /// </summary>
    internal static class GaussKronrodPointFactory
    {
        [ThreadStatic]
        static GaussPointPair _gaussKronrodPoint;

        /// <summary>
        /// Getter for the GaussKronrodPoint.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Kronrod rule. Precomputed Gauss-Kronrod abscissas/weights for orders 15, 21, 31, 41, 51, 61 are used, otherwise they're calculated on the fly.</param>
        /// <returns>Object containing the non-negative abscissas/weights, and order.</returns>
        public static GaussPointPair GetGaussPoint(int order)
        {
            // Try to get the GaussKronrodPoint from the cached static field.
            bool gaussKronrodPointIsCached = _gaussKronrodPoint != null && _gaussKronrodPoint.Order == order;
            if (!gaussKronrodPointIsCached)
            {
                // Try to find the GaussKronrodPoint in the precomputed dictionary.
                if (!GaussKronrodPoint.PreComputed.TryGetValue(order, out _gaussKronrodPoint))
                {
                    _gaussKronrodPoint = GaussKronrodPoint.Generate(order, 1E-10);
                }
            }

            return _gaussKronrodPoint;
        }
    }
}
