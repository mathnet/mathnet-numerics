using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Optimization
{
    public abstract class TrustRegionMinimizerBase
    {
        public static ITrustRegionSubproblem Subproblem;

        /// <summary>
        /// The stopping threshold for infinity norm of the gradient.
        /// </summary>
        public static double GradientTolerance { get; set; }

        /// <summary>
        /// The stopping threshold for L2 norm of the change of the parameters.
        /// </summary>
        public static double StepTolerance { get; set; }

        /// <summary>
        /// The stopping threshold for the function value or L2 norm of the residuals.
        /// </summary>
        public static double FunctionTolerance { get; set; }

        /// <summary>
        /// The stopping threshold for the trust region radius.
        /// </summary>
        public static double RadiusTolerance { get; set; }

        /// <summary>
        /// The maximum number of iterations.
        /// </summary>
        public int MaximumIterations { get; set; }
        
        public TrustRegionMinimizerBase(ITrustRegionSubproblem subproblem,
            double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-8, int maximumIterations = -1)
        {
            if (subproblem == null)
                throw new ArgumentNullException("subproblem");

            Subproblem = subproblem;
            FunctionTolerance = functionTolerance;
            GradientTolerance = gradientTolerance;
            StepTolerance = stepTolerance;
            RadiusTolerance = radiusTolerance;
            MaximumIterations = maximumIterations;            
        }

        public NonlinearMinimizationResult FindMinimum(IObjectiveModel objective, Vector<double> initialGuess,
            Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null)
        {
            if (objective == null)
                throw new ArgumentNullException("objective");
            if (initialGuess == null)
                throw new ArgumentNullException("initialGuess");

            return Minimum(Subproblem, objective, initialGuess, lowerBound, upperBound, scales, isFixed,
                GradientTolerance, StepTolerance, FunctionTolerance, RadiusTolerance, MaximumIterations);
        }

        public NonlinearMinimizationResult FindMinimum(IObjectiveModel objective, double[] initialGuess,
            double[] lowerBound = null, double[] upperBound = null, double[] scales = null, bool[] isFixed = null)
        {
            if (objective == null)
                throw new ArgumentNullException("objective");
            if (initialGuess == null)
                throw new ArgumentNullException("initialGuess");

            var lb = (lowerBound == null) ? null : CreateVector.Dense<double>(lowerBound);
            var ub = (upperBound == null) ? null : CreateVector.Dense<double>(upperBound);
            var sc = (scales == null) ? null : CreateVector.Dense<double>(scales);
            var fx = (isFixed == null) ? null : isFixed.ToList();

            return Minimum(Subproblem, objective, CreateVector.DenseOfArray<double>(initialGuess), lb, ub, sc, fx,
                GradientTolerance, StepTolerance, FunctionTolerance, RadiusTolerance, MaximumIterations);
        }

        /// <summary>
        /// Non-linear least square fitting by the trust-region algorithm.
        /// </summary>
        /// <param name="objective">The objective model, including function, jacobian, observations, and parameter bounds.</param>
        /// <param name="subproblem">The subproblem</param> 
        /// <param name="initialGuess">The initial guess values.</param>
        /// <param name="functionTolerance">The stopping threshold for L2 norm of the residuals.</param>
        /// <param name="gradientTolerance">The stopping threshold for infinity norm of the gradient vector.</param>
        /// <param name="stepTolerance">The stopping threshold for L2 norm of the change of parameters.</param>
        /// <param name="radiusTolerance">The stopping threshold for trust region radius</param>
        /// <param name="maximumIterations">The max iterations.</param>
        /// <returns></returns>
        public static NonlinearMinimizationResult Minimum(ITrustRegionSubproblem subproblem, IObjectiveModel objective, Vector<double> initialGuess,
            Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null,
            double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-18, int maximumIterations = -1)
        {
            // Non-linear least square fitting by the trust-region algorithm.
            //
            // For given datum pair (x, y), uncertainties σ (or weighting W  =  1 / σ^2) and model function f = f(x; p),
            // let's find the parameters of the model so that the sum of the quares of the deviations is minimized.
            //
            //    F(p) = 1/2 * ∑{ Wi * (yi - f(xi; p))^2 }
            //    pbest = argmin F(p)
            //
            // Here, we will use the following terms:
            //    Weighting W is the diagonal matrix and can be decomposed as LL', so L = 1/σ
            //    Residuals, R = L(y - f(x; p))
            //    Residual sum of squares, RSS = ||R||^2 = R.DotProduct(R)
            //    Jacobian J = df(x; p)/dp
            //    Gradient g = -J'W(y − f(x; p)) = -J'LR
            //    Approximated Hessian H = J'WJ
            //
            // The trust region algorithm is summarized as follows:
            //    initially set trust-region radius, Δ
            //    repeat
            //       solve subproblem
            //       update Δ:
            //          let ρ = (RSS - RSSnew) / predRed
            //          if ρ > 0.75, Δ = 2Δ
            //          if ρ < 0.25, Δ = Δ/4
            //          if ρ > eta, P = P + ΔP
            //
            // References:
            // [1]. Madsen, K., H. B. Nielsen, and O. Tingleff.
            //    "Methods for Non-Linear Least Squares Problems. Technical University of Denmark, 2004. Lecture notes." (2004).
            //    Available Online from: http://orbit.dtu.dk/files/2721358/imm3215.pdf
            // [2]. Nocedal, Jorge, and Stephen J. Wright.
            //    Numerical optimization (2006): 101-134.
            // [3]. SciPy
            //    Available Online from: https://github.com/scipy/scipy/blob/master/scipy/optimize/_trustregion.py

            double maxDelta = 1000;
            double eta = 0;

            if (objective == null)
                throw new ArgumentNullException("objective");
            if (initialGuess == null)
                throw new ArgumentNullException("initialGuess");

            objective.SetParameters(initialGuess, lowerBound, upperBound, scales, isFixed);

            ExitCondition exitCondition = ExitCondition.None;

            // Initialize objective
            objective.FunctionEvaluations = 0;
            objective.JacobianEvaluations = 0;
            objective.IsFinished = false;

            // First, calculate function values and setup variables
            objective.EvaluateAt(initialGuess);
            var P = objective.Point; // current parameters
            var RSS = objective.Value; // Residual Sum of Squares = R'R
            var RSSinit = RSS; // RSS at initial gussing parameters

            if (maximumIterations < 0)
            {
                maximumIterations = 200 * (initialGuess.Count + 1);
            }

            // if RSS == NaN, stop
            if (double.IsNaN(RSS))
            {
                exitCondition = ExitCondition.InvalidValues;
                return new NonlinearMinimizationResult(objective, -1, exitCondition);
            }

            // When only function evaluation is needed, set maximumIterations to zero, 
            if (maximumIterations == 0)
            {
                exitCondition = ExitCondition.ManuallyStopped;
            }

            // if ||R||^2 <= fTol, stop
            if (RSS <= functionTolerance)
            {
                exitCondition = ExitCondition.Converged; // SmallRSS
            }

            // evaluate projected gradient and Hessian
            var Gradient = objective.Gradient;
            var Hessian = objective.Hessian;            

            // if ||g||_oo <= gtol, found and stop
            if (Gradient.InfinityNorm() <= gradientTolerance)
            {
                exitCondition = ExitCondition.RelativeGradient; // SmallGradient
            }

            if (exitCondition != ExitCondition.None)
            {
                return new NonlinearMinimizationResult(objective, -1, exitCondition);
            }

            // initialize trust-region radius, Δ
            double delta = Gradient.DotProduct(Gradient) / (Hessian * Gradient).DotProduct(Gradient);
            delta = Math.Max(1.0, Math.Min(delta, maxDelta));

            int iterations = 0;
            while (iterations < maximumIterations && exitCondition == ExitCondition.None)
            {
                iterations++;

                // solve the subproblem
                subproblem.Solve(objective, delta);
                var Pstep = subproblem.Pstep;
                var hitBoundary = subproblem.HitBoundary;
                // predicted reduction = L(0) - L(Δp) = -Δp'g - 1/2 * Δp'HΔp
                var predictedReduction = -Gradient.DotProduct(Pstep) - 0.5 * Pstep.DotProduct(Hessian * Pstep);

                if (Pstep.L2Norm() <= stepTolerance * (stepTolerance + P.L2Norm()))
                {
                    exitCondition = ExitCondition.RelativePoints; // SmallRelativeParameters
                    break;
                }

                var Pnew = P + Pstep; // parameters to test

                objective.EvaluateAt(Pnew);
                var RSSnew = objective.Value;

                // if RSS == NaN, stop
                if (double.IsNaN(RSSnew))
                {
                    exitCondition = ExitCondition.InvalidValues;
                    break;
                }

                // calculate the ratio of the actual to the predicted reduction.
                double rho = (predictedReduction != 0)
                        ? (RSS - RSSnew) / predictedReduction
                        : 0.0;

                if (rho > 0.75 && hitBoundary)
                {
                    delta = Math.Min(2.0 * delta, maxDelta);
                }
                else if (rho < 0.25)
                {
                    delta = delta * 0.25;
                    if (delta <= radiusTolerance * (radiusTolerance + P.DotProduct(P)))
                    {
                        exitCondition = ExitCondition.RelativePoints; // SmallRelativeParameters
                        break;
                    }
                }

                if (rho > eta)
                {
                    // accepted
                    Pnew.CopyTo(P);
                    RSS = RSSnew;

                    // evaluate projected gradient and Hessian
                    Gradient = objective.Gradient;
                    Hessian = objective.Hessian;

                    // if ||g||_oo <= gtol, found and stop
                    if (Gradient.InfinityNorm() <= gradientTolerance)
                    {
                        exitCondition = ExitCondition.RelativeGradient;
                    }

                    // if ||R||^2 < fTol, found and stop
                    if (RSS <= functionTolerance)
                    {
                        exitCondition = ExitCondition.Converged; // SmallRSS
                    }
                }
            }

            if (iterations >= maximumIterations)
            {
                exitCondition = ExitCondition.ExceedIterations;
            }

            return new NonlinearMinimizationResult(objective, iterations, exitCondition);
        }
    }
}
