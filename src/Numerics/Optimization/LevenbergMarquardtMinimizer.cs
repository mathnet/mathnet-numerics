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

            var objectiveModel = objective.CreateNew();

            ValidateBounds(initialGuess, lowerBound, upperBound, scales);

            objectiveModel.SetParameters(initialGuess, isFixed);

            var exitCondition = ExitCondition.None;

            // First, calculate function values and setup variables
            var P = ProjectToInternalParameters(initialGuess); // current internal parameters
            Vector<double> Pstep; // the change of parameters
            var RSS = EvaluateFunction(objectiveModel, P);  // Residual Sum of Squares = 1/2 R'R

            if (maximumIterations < 0)
            {
                maximumIterations = 200 * (initialGuess.Count + 1);
            }

            // if RSS == NaN, stop
            if (double.IsNaN(RSS))
            {
                exitCondition = ExitCondition.InvalidValues;
                return new NonlinearMinimizationResult(objectiveModel, -1, exitCondition);
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
            var (Gradient, Hessian) = EvaluateJacobian(objectiveModel, P);

            // if ||g||oo <= gtol, found and stop
            if (Gradient.InfinityNorm() <= gradientTolerance)
            {
                exitCondition = ExitCondition.RelativeGradient;
            }

            if (exitCondition != ExitCondition.None)
            {
                return new NonlinearMinimizationResult(objectiveModel, -1, exitCondition);
            }

            // Initialize trust region boundary delta and damping parameter mu
            var delta = initialMu * P.L2Norm(); // Trust region boundary
            if (delta == 0.0) delta = initialMu;
            var mu = initialMu * Hessian.Diagonal().Max(); // Damping parameter μ
            var nu = 2.0; // Multiplication factor ν for mu updates

            // Counters for successful and failed iterations
            var ncsuc = 0;  // number of consecutive successful iterations
            var ncfail = 0; // number of consecutive failed iterations

            // Flag for first iteration special handling
            var firstIteration = true;

            var iterations = 0;
            while (iterations < maximumIterations && exitCondition == ExitCondition.None)
            {
                iterations++;

                while (true)
                {
                    // Store current Hessian diagonal for restoration if step is rejected
                    var savedDiagonal = Hessian.Diagonal().Clone();

                    // Add damping to Hessian: H + μI
                    Hessian.SetDiagonal(Hessian.Diagonal() + mu); // hessian[i, i] = hessian[i, i] + mu;

                    // Solve normal equations: (H + μI)Δp = -g
                    Pstep = Hessian.Solve(-Gradient);

                    // Calculate step size (norm)
                    var pnorm = Pstep.L2Norm();

                    // On first iteration, adjust the initial step bound
                    if (firstIteration)
                    {
                        delta = Math.Min(delta, pnorm);
                        firstIteration = false;
                    }

                    // Check convergence on step size
                    if (pnorm <= stepTolerance * (P.L2Norm() + stepTolerance))
                    {
                        exitCondition = ExitCondition.RelativePoints;
                        break;
                    }

                    // New parameter vector
                    var Pnew = P + Pstep;

                    // Evaluate function at new point
                    var RSSnew = EvaluateFunction(objectiveModel, Pnew);

                    // Check for invalid results
                    if (double.IsNaN(RSSnew))
                    {
                        exitCondition = ExitCondition.InvalidValues;
                        break;
                    }

                    // Compute the scaled actual reduction
                    // actred = 1 - (fnorm1/fnorm)^2 if fnorm1 < fnorm, else -1
                    var actred = (RSSnew < RSS) ? 1.0 - Math.Pow(RSSnew / RSS, 2.0) : -1.0;

                    // Compute predicted reduction metrics
                    // In LMDER, wa3 = J'*(Q'*fvec), where J is the Jacobian at the current point
                    // and the predicted reduction is ||J*p||^2 + ||sqrt(par)*D*p||^2
                    var tempVec = Hessian.Multiply(Pstep);
                    var temp1 = Pstep.DotProduct(tempVec) / RSS;
                    var temp2 = (Math.Sqrt(mu) * pnorm) / Math.Sqrt(RSS);
                    var prered = temp1 + Math.Pow(temp2, 2.0) / 0.5;
                    var dirder = -(temp1 + Math.Pow(temp2, 2.0));

                    // Compute the ratio of actual to predicted reduction
                    var ratio = (prered != 0.0) ? actred / prered : 0.0;

                    // Update trust region based on reduction ratio
                    if (ratio < 0.0001)
                    {
                        // Failure: ratio too small
                        ncsuc = 0;
                        ncfail++;

                        mu = mu * nu;
                        nu = 2.0 * nu;

                        delta = 0.25 * delta;
                    }
                    else if (ratio < 0.25)
                    {
                        // Accept but shrink
                        ncfail = 0;
                        ncsuc = 0;

                        delta = 0.5 * delta;

                        var temp = 1.0 - Math.Pow((2.0 * ratio - 1.0), 3);
                        temp = Math.Max(temp, 1.0 / 3.0);
                        mu = mu * temp;
                    }
                    else if (ratio < 0.75)
                    {
                        // Accept + mild delta increase
                        ncsuc++;
                        ncfail = 0;

                        delta = Math.Max(delta, pnorm);

                        var temp = 1.0 - Math.Pow((2.0 * ratio - 1.0), 3);
                        temp = Math.Max(temp, 1.0 / 3.0);
                        mu = mu * temp;
                    }
                    else
                    {
                        // ratio >= 0.75
                        ncsuc++;
                        ncfail = 0;

                        delta = Math.Max(delta, 2.0 * pnorm);

                        var temp = 1.0 - Math.Pow((2.0 * ratio - 1.0), 3);
                        temp = Math.Max(temp, 1.0 / 3.0);
                        mu = mu * temp;
                    }

                    // Test for successful iteration
                    if (ratio >= 0.0001)
                    {
                        // Update parameters
                        Pnew.CopyTo(P);
                        RSS = RSSnew;

                        // Recalculate gradient and Hessian at new point
                        (Gradient, Hessian) = EvaluateJacobian(objectiveModel, P);

                        // Check convergence criteria
                        if (Gradient.InfinityNorm() <= gradientTolerance)
                        {
                            exitCondition = ExitCondition.RelativeGradient;
                        }

                        if (RSS <= functionTolerance)
                        {
                            exitCondition = ExitCondition.Converged;
                        }

                        break; // Exit inner loop, step accepted
                    }
                    else
                    {
                        // Step was rejected, restore original Hessian
                        Hessian.SetDiagonal(savedDiagonal);

                        // Update mu and nu
                        mu = mu * nu;
                        nu = 2.0 * nu;

                        // If we're making no progress, exit the inner loop
                        if (ncfail >= 2)
                        {
                            break;  // Exit inner loop, try a new Jacobian
                        }
                    }
                }
            }

            // Check if max iterations reached
            if (iterations >= maximumIterations)
            {
                exitCondition = ExitCondition.ExceedIterations;
            }

            return new NonlinearMinimizationResult(objectiveModel, iterations, exitCondition);
        }
    }
}
