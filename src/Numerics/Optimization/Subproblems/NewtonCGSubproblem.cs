using MathNet.Numerics.LinearAlgebra;
using System;

namespace MathNet.Numerics.Optimization.Subproblems
{
    internal class NewtonCGSubproblem : ITrustRegionSubproblem
    {
        public Vector<double> Pstep { get; private set; }

        public bool HitBoundary { get; private set; }

        public void Solve(IObjectiveModel objective, double delta)
        {
            var Jacobian = objective.Jacobian;
            var Gradient = objective.Gradient;
            var Hessian = objective.Hessian;

            // define tolerance
            var tolerance = Math.Min(0.5, Math.Sqrt(Gradient.L2Norm())) * Gradient.L2Norm();

            // initialize internal variables
            var z = Vector<double>.Build.Dense(Hessian.RowCount);
            var r = -Gradient;
            var d = -r;

            while (true)
            {
                var Bd = Hessian * d;
                var dBd = d.DotProduct(Bd);

                if (dBd <= 0)
                {
                    var t = Util.FindBeta(1, z, d, delta);
                    Pstep = z + t.Item1 * d;
                    HitBoundary = true;
                    return;
                }

                var r_sq = r.DotProduct(r);
                var alpha = r_sq / dBd;
                var znext = z + alpha * d;
                if(znext.L2Norm() >= delta)
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
                d = -rnext + rnext_sq / r_sq * d; ;
            }
        }
    }
}
