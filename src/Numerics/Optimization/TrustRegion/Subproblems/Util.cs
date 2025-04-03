using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.TrustRegion.Subproblems
{
    internal static class Util
    {
        /// <summary>
        /// Finds the two intersection points of a line with the trust region boundary.
        /// </summary>
        /// <param name="alpha">Scaling factor for steepest descent direction</param>
        /// <param name="sd">Steepest descent direction vector</param>
        /// <param name="gn">Gauss-Newton direction vector</param>
        /// <param name="delta">Trust region radius</param>
        /// <returns>A tuple containing two beta values, sorted from low to high</returns>
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

        /// <summary>
        /// Calculates the value of the quadratic model at a given point.
        /// The quadratic model is defined as:
        ///     m(p) = g^T * p + 0.5 * p^T * H * p
        /// where g is the gradient and H is the Hessian at the current point.
        /// </summary>
        /// <param name="gradient">The gradient vector</param>
        /// <param name="hessian">The Hessian matrix</param>
        /// <param name="p">The point at which to evaluate the quadratic model</param>
        /// <returns>The value of the quadratic model at point p</returns>
        public static double CalculateQuadraticModel(Vector<double> gradient, Matrix<double> hessian, Vector<double> p)
        {
            // Quadratic model: m(p) = g^T * p + 0.5 * p^T * H * p
            var linearTerm = gradient.DotProduct(p);
            var quadraticTerm = 0.5 * p.DotProduct(hessian * p);

            return linearTerm + quadraticTerm;
        }
    }
}
