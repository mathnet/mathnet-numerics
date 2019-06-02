using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.TrustRegion.Subproblems
{
    internal class DogLegSubproblem : ITrustRegionSubproblem
    {
        public Vector<double> Pstep { get; private set; }

        public bool HitBoundary { get; private set; }

        public void Solve(IObjectiveModel objective, double delta)
        {
            var Gradient = objective.Gradient;
            var Hessian = objective.Hessian;

            // newton point, the Gauss–Newton step by solving the normal equations
            var Pgn = -Hessian.PseudoInverse() * Gradient; // Hessian.Solve(Gradient) fails so many times...

            // cauchy point, steepest descent direction is given by
            var alpha = Gradient.DotProduct(Gradient) / (Hessian * Gradient).DotProduct(Gradient);
            var Psd = -alpha * Gradient;

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
