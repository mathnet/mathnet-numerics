using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Optimization
{
    public class LevenbergMarquardtMinimizer : NonlinearMinimizerBase
    {
        /// <summary>
        /// The scale factor for initial mu
        /// </summary>
        public double InitialMu { get; set; }

        public LevenbergMarquardtMinimizer(double initialMu = 1E-3, double gradientTolerance = 1E-15, double stepTolerance = 1E-15, double functionTolerance = 1E-15, int maximumIterations = -1)
            : base(gradientTolerance, stepTolerance, functionTolerance, maximumIterations)
        {
            InitialMu = initialMu;
        }

        public NonlinearMinimizationResult FindMinimum(IObjectiveModel objective, Vector<double> initialGuess,
            Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null)
        {
            return Minimum(objective, initialGuess, lowerBound, upperBound, scales, isFixed, InitialMu, GradientTolerance, StepTolerance, FunctionTolerance, MaximumIterations);
        }

        public NonlinearMinimizationResult FindMinimum(IObjectiveModel objective, double[] initialGuess,
            double[] lowerBound = null, double[] upperBound = null, double[] scales = null, bool[] isFixed = null)
        {
            if (objective == null)
                throw new ArgumentNullException(nameof(objective));
            if (initialGuess == null)
                throw new ArgumentNullException(nameof(initialGuess));

            var lb = (lowerBound == null) ? null : CreateVector.Dense(lowerBound);
            var ub = (upperBound == null) ? null : CreateVector.Dense(upperBound);
            var sc = (scales == null) ? null : CreateVector.Dense(scales);
            var fx = isFixed?.ToList();

            return Minimum(objective, CreateVector.DenseOfArray(initialGuess), lb, ub, sc, fx, InitialMu, GradientTolerance, StepTolerance, FunctionTolerance, MaximumIterations);
        }

        /// <summary>
        /// Non-linear least square fitting by the Levenberg-Marduardt algorithm.
        /// </summary>
        /// <param name="objective">The objective function, including model, observations, and parameter bounds.</param>
        /// <param name="initialGuess">The initial guess values.</param>
        /// <param name="initialMu">The initial damping parameter of mu.</param>
        /// <param name="gradientTolerance">The stopping threshold for infinity norm of the gradient vector.</param>
        /// <param name="stepTolerance">The stopping threshold for L2 norm of the change of parameters.</param>
        /// <param name="functionTolerance">The stopping threshold for L2 norm of the residuals.</param>
        /// <param name="maximumIterations">The max iterations.</param>
        /// <returns>The result of the Levenberg-Marquardt minimization</returns>
        public NonlinearMinimizationResult Minimum(IObjectiveModel objective, Vector<double> initialGuess,
            Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null,
            double initialMu = 1E-3, double gradientTolerance = 1E-15, double stepTolerance = 1E-15, double functionTolerance = 1E-15, int maximumIterations = -1)
        {
            // Non-linear least square fitting by the Levenberg-Marduardt algorithm.
            //
            // Levenberg-Marquardt is finding the minimum of a function F(p) that is a sum of squares of nonlinear functions.
            //
            // For given datum pair (x, y), uncertainties σ (or weighting W  =  1 / σ^2) and model function f = f(x; p),
            // let's find the parameters of the model so that the sum of the quares of the deviations is minimized.
            //
            //    F(p) = 1/2 * ∑{ Wi * (yi - f(xi; p))^2 }
            //    pbest = argmin F(p)
            //
            // We will use the following terms:
            //    Weighting W is the diagonal matrix and can be decomposed as LL', so L = 1/σ
            //    Residuals, R = L(y - f(x; p))
            //    Residual sum of squares, RSS = ||R||^2 = R.DotProduct(R)
            //    Jacobian J = df(x; p)/dp
            //    Gradient g = -J'W(y − f(x; p)) = -J'LR
            //    Approximated Hessian H = J'WJ
            //
            // The Levenberg-Marquardt algorithm is summarized as follows:
            //    initially let μ = τ * max(diag(H)).
            //    repeat
            //       solve linear equations: (H + μI)ΔP = -g
            //       let ρ = (||R||^2 - ||Rnew||^2) / (Δp'(μΔp - g)).
            //       if ρ > ε, P = P + ΔP; μ = μ * max(1/3, 1 - (2ρ - 1)^3); ν = 2;
            //       otherwise μ = μ*ν; ν = 2*ν;
            //
            // References:
            // [1]. Madsen, K., H. B. Nielsen, and O. Tingleff.
            //    "Methods for Non-Linear Least Squares Problems. Technical University of Denmark, 2004. Lecture notes." (2004).
            //    Available Online from: http://orbit.dtu.dk/files/2721358/imm3215.pdf
            // [2]. Gavin, Henri.
            //    "The Levenberg-Marquardt method for nonlinear least squares curve-fitting problems."
            //    Department of Civil and Environmental Engineering, Duke University (2017): 1-19.
            //    Availble Online from: http://people.duke.edu/~hpgavin/ce281/lm.pdf

            if (objective == null)
                throw new ArgumentNullException(nameof(objective));

            ValidateBounds(initialGuess, lowerBound, upperBound, scales);

            objective.SetParameters(initialGuess, isFixed);

            ExitCondition exitCondition = ExitCondition.None;

            // First, calculate function values and setup variables
            var P = ProjectToInternalParameters(initialGuess); // current internal parameters
            Vector<double> Pstep; // the change of parameters
            var RSS = EvaluateFunction(objective, P);  // Residual Sum of Squares = R'R

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

            // if RSS <= fTol, stop
            if (RSS <= functionTolerance)
            {
                exitCondition = ExitCondition.Converged; // SmallRSS
            }

            // Evaluate gradient and Hessian
            var (Gradient, Hessian) = EvaluateJacobian(objective, P);
            var diagonalOfHessian = Hessian.Diagonal(); // diag(H)

            // if ||g||oo <= gtol, found and stop
            if (Gradient.InfinityNorm() <= gradientTolerance)
            {
                exitCondition = ExitCondition.RelativeGradient;
            }

            if (exitCondition != ExitCondition.None)
            {
                return new NonlinearMinimizationResult(objective, -1, exitCondition);
            }

            double mu = initialMu * diagonalOfHessian.Max(); // μ
            double nu = 2; //  ν
            int iterations = 0;
            while (iterations < maximumIterations && exitCondition == ExitCondition.None)
            {
                iterations++;

                while (true)
                {
                    Hessian.SetDiagonal(Hessian.Diagonal() + mu); // hessian[i, i] = hessian[i, i] + mu;

                    // solve normal equations
                    Pstep = Hessian.Solve(-Gradient);

                    // if ||ΔP|| <= xTol * (||P|| + xTol), found and stop
                    if (Pstep.L2Norm() <= stepTolerance * (stepTolerance + P.DotProduct(P)))
                    {
                        exitCondition = ExitCondition.RelativePoints;
                        break;
                    }

                    var Pnew = P + Pstep; // new parameters to test
                    // evaluate function at Pnew
                    var RSSnew = EvaluateFunction(objective, Pnew);

                    if (double.IsNaN(RSSnew))
                    {
                        exitCondition = ExitCondition.InvalidValues;
                        break;
                    }

                    // calculate the ratio of the actual to the predicted reduction.
                    // ρ = (RSS - RSSnew) / (Δp'(μΔp - g))
                    var predictedReduction = Pstep.DotProduct(mu * Pstep - Gradient);
                    var rho = (predictedReduction != 0)
                            ? (RSS - RSSnew) / predictedReduction
                            : 0;

                    if (rho > 0.0)
                    {
                        // accepted
                        Pnew.CopyTo(P);
                        RSS = RSSnew;

                        // update gradient and Hessian
                        (Gradient, Hessian) = EvaluateJacobian(objective, P);
                        diagonalOfHessian = Hessian.Diagonal();

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

                        mu = mu * Math.Max(1.0 / 3.0, 1.0 - Math.Pow(2.0 * rho - 1.0, 3));
                        nu = 2;

                        break;
                    }
                    else
                    {
                        // rejected, increased μ
                        mu = mu * nu;
                        nu = 2 * nu;

                        Hessian.SetDiagonal(diagonalOfHessian);
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
