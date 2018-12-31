using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.Subproblems
{
    internal class QuadraticSubproblem : ITrustRegionSubproblem
    {
        public Vector<double> Pstep { get; private set; }

        public double PredictedReduction { get; private set; }

        public bool HitBoundary { get; private set; }

        public void Solve(IObjectiveModel objective, double delta)
        {
            var Jacobian = objective.Jacobian;
            var Gradient = objective.Gradient;
            var Hessian = objective.Hessian;
            var RSS = objective.Residue;

            // newton point
            // the Gauss–Newton step by solving the normal equations
            var Pgn = Hessian.PseudoInverse() * Gradient; // Hessian.Solve(Gradient) fails so many times...

            // cauchy point
            // steepest descent direction is given by
            var alpha = Gradient.DotProduct(Gradient) / (Hessian * Gradient).DotProduct(Gradient);
            var Psd = alpha * Gradient;

            // update step and prectted reduction
            if (Pgn.L2Norm() <= delta)
            {
                // Pgn is inside trust region radius
                HitBoundary = false;
                Pstep = Pgn;
                PredictedReduction = RSS;
            }
            else if (alpha * Psd.L2Norm() >= delta)
            {
                // Psd is outside trust region radius
                HitBoundary = true;
                Pstep = delta / Psd.L2Norm() * Psd;
                PredictedReduction = delta * (2.0 * (alpha * Gradient).L2Norm() - delta) / 2.0 / alpha;
            }
            else
            {
                // Pstep is intersection of the trust region boundary
                HitBoundary = true;
                var beta = Util.FindBeta(alpha, Psd, Pgn, delta).Item2;
                Pstep = alpha * Psd + beta * (Pgn - alpha * Psd);
                PredictedReduction = 0.5 * alpha * (1 - beta) * (1 - beta) * Gradient.DotProduct(Gradient) + beta * (2 - beta) * RSS;
            }
        }
    }
}
