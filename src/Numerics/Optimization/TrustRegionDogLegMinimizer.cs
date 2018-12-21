using MathNet.Numerics.LinearAlgebra;
using System;

namespace MathNet.Numerics.Optimization
{
    public sealed class TrustRegionDogLegMinimizer
    {
        #region Tolerances and options

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

        #endregion Tolerances and options

        public TrustRegionDogLegMinimizer(double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-8, int maximumIterations = -1)
        {
            FunctionTolerance = functionTolerance;
            GradientTolerance = gradientTolerance;
            StepTolerance = stepTolerance;
            RadiusTolerance = radiusTolerance;
            MaximumIterations = maximumIterations;
        }

        public ModelMinimizationResult FindMinimum(IObjectiveModel objective, Vector<double> initialGuess)
        {
            if (objective == null)
                throw new ArgumentNullException("objective");
            if (initialGuess == null)
                throw new ArgumentNullException("initialGuess");

            return Minimum(objective, initialGuess, GradientTolerance, StepTolerance, FunctionTolerance, RadiusTolerance, MaximumIterations);
        }

        public ModelMinimizationResult FindMinimum(IObjectiveModel objective, double[] initialGuess)
        {
            if (objective == null)
                throw new ArgumentNullException("objective");
            if (initialGuess == null)
                throw new ArgumentNullException("initialGuess");

            return Minimum(objective, CreateVector.DenseOfArray<double>(initialGuess), GradientTolerance, StepTolerance, FunctionTolerance, RadiusTolerance, MaximumIterations);
        }

        /// <summary>
        /// Non-linear least square fitting by the trust-region dogleg algorithm.
        /// </summary>
        /// <param name="objective">The objective model, including function, jacobian, observations, and parameter bounds.</param>
        /// <param name="initialGuess">The initial guess values.</param>
        /// <param name="functionTolerance">The stopping threshold for L2 norm of the residuals.</param>
        /// <param name="gradientTolerance">The stopping threshold for infinity norm of the gradient vector.</param>
        /// <param name="stepTolerance">The stopping threshold for L2 norm of the change of parameters.</param>
        /// <param name="radiusTolerance">The stopping threshold for trust region radius</param>
        /// <param name="maximumIterations">The max iterations.</param>
        /// <returns></returns>
        public static ModelMinimizationResult Minimum(IObjectiveModel objective, Vector<double> initialGuess, double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-18, int maximumIterations = -1)
        {
            // Non-linear least square fitting by the trust-region dogleg algorithm.
            //
            // The Powell's dogleg method is finding the minimum of a function F(p) that is a sum of squares of nonlinear functions.
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
            //    Gradient g = J'W(y − f(x; p)) = J'LR
            //    Approximated Hessian H = J'WJ
            //
            // The Powell's dogleg algorithm is summarized as follows:
            //    initially set trust-region radius, Δ
            //    repeat
            //       solve quadratic subproblem
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
            // [2]. SciPy
            //    Available Online from: https://github.com/scipy/scipy/blob/master/scipy/optimize/_trustregion_dogleg.py

            double maxDelta = 1000;
            double eta = 0;

            if (objective == null)
                throw new ArgumentNullException("objective");
            if (initialGuess == null)
                throw new ArgumentNullException("initialGuess");

            ExitCondition exitCondition = ExitCondition.None;

            // First, calculate function values and setup variables
            objective.EvaluateFunction(initialGuess);
            var P = objective.Parameters; // current parameters
            var RSS = objective.Residue; // Residual Sum of Squares = R'R
            var RSSinit = RSS; // RSS at initial gussing parameters

            if (maximumIterations < 0)
            {
                maximumIterations = 200 * (initialGuess.Count + 1);
            }

            // if R == NaN, stop
            if (double.IsNaN(RSS))
            {
                exitCondition = ExitCondition.InvalidValues;
                return new ModelMinimizationResult(objective, -1, exitCondition);
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

            // Evaluate projected Hessian, and gradient
            objective.EvaluateJacobian(P);
            var Hessian = objective.Hessian;
            var Gradient = objective.Gradient;

            // if ||g||_oo <= gtol, found and stop
            if (Gradient.InfinityNorm() <= gradientTolerance)
            {
                exitCondition = ExitCondition.RelativeGradient; // SmallGradient
            }

            if (exitCondition != ExitCondition.None)
            {
                // finalize
                objective.EvaluateCovariance(P);
                return new ModelMinimizationResult(objective, -1, exitCondition);
            }

            // initialize trust-region radius, Δ
            double delta = Gradient.DotProduct(Gradient) / (Hessian * Gradient).DotProduct(Gradient);
            delta = Math.Max(1.0, Math.Min(delta, maxDelta));

            int iterations = 0;
            while (iterations < maximumIterations && exitCondition == ExitCondition.None)
            {
                iterations++;

                // solve the subproblem
                var subprogram = SolveQuadraticSubproblem(objective, delta);
                var Pstep = subprogram.Item1;
                var predictedReduction = subprogram.Item2;
                var hitBoundary = subprogram.Item3;
                
                if (Pstep.L2Norm() <= stepTolerance * (stepTolerance + P.L2Norm()))
                {
                    exitCondition = ExitCondition.RelativePoints; // SmallRelativeParameters
                    break;
                }

                var Pnew = P + Pstep; // parameters to test

                objective.EvaluateFunction(Pnew);
                var RSSnew = objective.Residue;

                // calculate the ratio of the actual to the predicted reduction.
                double rho = (predictedReduction != 0)
                        ? (RSS - RSSnew) / predictedReduction
                        : 0;

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

                    // update Jacobian, Hessian, and gradient
                    objective.EvaluateJacobian(P);
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

            // finalize
            objective.EvaluateCovariance(P);

            return new ModelMinimizationResult(objective, iterations, exitCondition);
        }

        private static Tuple<Vector<double>, double, bool> SolveQuadraticSubproblem(IObjectiveModel objective, double delta)
        {
            Vector<double> Pstep;
            double predictedReduction;
            bool hitBoundary = false;

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
                hitBoundary = false;
                Pstep = Pgn;
                predictedReduction = RSS;
            }
            else if (alpha * Psd.L2Norm() >= delta)
            {
                // Psd is outside trust region radius
                hitBoundary = true;
                Pstep = delta / Psd.L2Norm() * Psd;
                predictedReduction = delta * (2.0 * (alpha * Gradient).L2Norm() - delta) / 2.0 / alpha;
            }
            else
            {
                // Pstep is intersection of the trust region boundary
                hitBoundary = true;
                var beta = FindBeta(alpha, Psd, Pgn, delta);
                Pstep = alpha * Psd + beta * (Pgn - alpha * Psd);
                predictedReduction = 0.5 * alpha * (1 - beta) * (1 - beta) * Gradient.DotProduct(Gradient) + beta * (2 - beta) * RSS;
            }

            return new Tuple<Vector<double>, double, bool>(Pstep, predictedReduction, hitBoundary);
        }

        private static double FindBeta(double alpha, Vector<double> sd, Vector<double> gn, double delta)
        {
            // Pstep is intersection of the trust region boundary
            // Pstep = α*Psd + β*(Pgn - α*Psd) 
            // find r so that ||Pstep|| = Δ
            // z = α*Psd, d = (Pgn - z) 
            // (d^2)β^2 + (2*z*d)β + (z^2 - Δ^2) = 0
            // get positive β by using the quadratic formula

            var z = alpha * sd;
            var d = gn - z;

            var a = d.DotProduct(d);
            var b = 2.0 * z.DotProduct(d);
            var c = z.DotProduct(z) - delta * delta;

            var aux = b + ((b >= 0) ? 1.0 : -1.0) * Math.Sqrt(b * b - 4.0 * a * c);
            var beta = Math.Max(-aux / 2.0 / a, -2.0 * c / aux);

            return beta;
        }
    }
}
