using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.TrustRegion.Subproblems
{
    /// <summary>
    /// Implements the Newton Conjugate Gradient method for solving trust region subproblems.
    /// </summary>
    /// <remarks>
    /// The Newton-CG method iteratively improves the solution using conjugate gradient iterations
    /// while ensuring the solution stays within the trust region boundary. This method is
    /// particularly effective for large-scale problems and can handle cases where the Hessian
    /// is indefinite by detecting directions of negative curvature.
    /// </remarks>
    internal class NewtonCGSubproblem : ITrustRegionSubproblem
    {
        /// <inheritdoc/>
        public Vector<double> Pstep { get; private set; }

        /// <inheritdoc/>
        public bool HitBoundary { get; private set; }

        /// <inheritdoc/>
        public void Solve(Vector<double> gradient, Matrix<double> hessian, double delta)
        {
            // define tolerance
            var gnorm = gradient.L2Norm();
            var tolerance = Math.Min(0.5, Math.Sqrt(gnorm)) * gnorm;

            // initialize internal variables
            var z = Vector<double>.Build.Dense(hessian.RowCount);
            var r = gradient;
            var d = -r;

            while (true)
            {
                var Bd = hessian * d;
                var dBd = d.DotProduct(Bd);

                if (dBd <= 0)
                {
                    // Direction of negative curvature found
                    // Calculate two boundary points and choose the one with lower model value
                    var t = Util.FindBeta(1, z, d, delta);
                    var pa = z + t.Item1 * d;
                    var pb = z + t.Item2 * d;

                    // Evaluate quadratic model at both points
                    var valueA = Util.CalculateQuadraticModel(gradient, hessian, pa);
                    var valueB = Util.CalculateQuadraticModel(gradient, hessian, pb);

                    // Choose the point with the lower model value
                    Pstep = valueA < valueB ? pa : pb;
                    HitBoundary = true;
                    return;
                }

                var r_sq = r.DotProduct(r);
                var alpha = r_sq / dBd;
                var znext = z + alpha * d;
                if (znext.L2Norm() >= delta)
                {
                    var t = Util.FindBeta(1, z, d, delta);
                    Pstep = z + t.Item2 * d;
                    HitBoundary = true;
                    return;
                }

                var rnext = r + alpha * Bd;
                var rnext_sq = rnext.DotProduct(rnext);
                if (Math.Sqrt(rnext_sq) < tolerance)
                {
                    Pstep = znext;
                    HitBoundary = false;
                    return;
                }

                z = znext;
                r = rnext;
                d = -rnext + rnext_sq / r_sq * d;
            }
        }
    }
}
