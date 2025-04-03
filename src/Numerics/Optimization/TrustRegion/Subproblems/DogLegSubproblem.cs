using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.TrustRegion.Subproblems
{
    /// <summary>
    /// Implements the Dog-Leg method for solving trust region subproblems.
    /// </summary>
    /// <remarks>
    /// The Dog-Leg method combines the Gauss-Newton step and the steepest descent step
    /// to find an approximate solution to the trust region subproblem. It provides a good
    /// compromise between the computational efficiency of steepest descent and the fast
    /// convergence of Gauss-Newton near the solution.
    /// </remarks>
    internal class DogLegSubproblem : ITrustRegionSubproblem
    {
        /// <inheritdoc/>
        public Vector<double> Pstep { get; private set; }

        /// <inheritdoc/>
        public bool HitBoundary { get; private set; }

        /// <inheritdoc/>
        public void Solve(Vector<double> gradient, Matrix<double> hessian, double delta)
        {
            // newton point, the Gauss–Newton step by solving the normal equations
            var Pgn = -hessian.PseudoInverse() * gradient; // hessian.Solve(gradient) fails so many times...

            // cauchy point, steepest descent direction is given by
            var alpha = gradient.DotProduct(gradient) / (hessian * gradient).DotProduct(gradient);
            var Psd = -alpha * gradient;

            // update step and prectted reduction
            if (Pgn.L2Norm() <= delta)
            {
                // Pgn is inside trust region radius
                HitBoundary = false;
                Pstep = Pgn;
            }
            else if (alpha * Psd.L2Norm() >= delta)
            {
                // Psd is outside trust region radius
                HitBoundary = true;
                Pstep = delta / Psd.L2Norm() * Psd;
            }
            else
            {
                // Pstep is intersection of the trust region boundary
                HitBoundary = true;
                var beta = Util.FindBeta(alpha, Psd, Pgn, delta).Item2;
                Pstep = alpha * Psd + beta * (Pgn - alpha * Psd);
            }
        }
    }
}
