using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.TrustRegion
{
    /// <summary>
    /// Defines the interface for solving a trust region subproblem in nonlinear least squares minimization.
    /// This interface provides properties to retrieve the computed step and a flag indicating whether the solution hit the trust region boundary,
    /// as well as a method to solve the subproblem given an objective model and a trust region radius.
    /// </summary>
    public interface ITrustRegionSubproblem
    {
        /// <summary>
        /// Gets the computed parameter step vector after solving the subproblem.
        /// </summary>
        Vector<double> Pstep { get; }

        /// <summary>
        /// Gets a value indicating whether the solution of the subproblem hit the trust region boundary.
        /// </summary>
        bool HitBoundary { get; }

        /// <summary>
        /// Solves the trust region subproblem using the provided gradient and Hessian.
        /// </summary>
        /// <param name="gradient">The scaled gradient vector</param>
        /// <param name="hessian">The scaled Hessian matrix</param>
        /// <param name="delta">Trust region radius</param>
        void Solve(Vector<double> gradient, Matrix<double> hessian, double delta);
    }
}
