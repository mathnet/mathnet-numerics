using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.TrustRegion.Subproblems
{
    internal static class Util
    {
        public static (double, double) FindBeta(double alpha, Vector<double> sd, Vector<double> gn, double delta)
        {
            // Pstep is intersection of the trust region boundary
            // Pstep = α*Psd + β*(Pgn - α*Psd)
            // find r so that ||Pstep|| = Δ
            // z = α*Psd, d = (Pgn - z)
            // (d^2)β^2 + (2*z*d)β + (z^2 - Δ^2) = 0
            //
            // positive β is used for the quadratic formula

            var z = alpha * sd;
            var d = gn - z;

            var a = d.DotProduct(d);
            var b = 2.0 * z.DotProduct(d);
            var c = z.DotProduct(z) - delta * delta;

            var aux = b + ((b >= 0) ? 1.0 : -1.0) * Math.Sqrt(b * b - 4.0 * a * c);
            var beta1 = -aux / 2.0 / a;
            var beta2 = -2.0 * c / aux;

            // return sorted beta
            return beta1 < beta2 ? (beta1, beta2) : (beta2, beta1);
        }
    }
}
