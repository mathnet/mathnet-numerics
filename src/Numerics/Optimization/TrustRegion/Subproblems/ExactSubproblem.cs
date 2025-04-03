// <copyright file="ExactSubproblem.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using MathNet.Numerics.LinearAlgebra;
using System;
using System.Linq;

namespace MathNet.Numerics.Optimization.TrustRegion.Subproblems
{
    /// <summary>
    /// Implements the Exact method for solving trust region subproblems.
    /// </summary>
    /// <remarks>
    /// The Exact method uses eigenvalue decomposition to find the exact solution to the trust region subproblem.
    /// It is more computationally expensive than other methods but can provide more accurate solutions,
    /// especially when the Hessian is indefinite or when high accuracy is required.
    /// </remarks>
    internal class ExactSubproblem : ITrustRegionSubproblem
    {
        /// <inheritdoc/>
        public Vector<double> Pstep { get; private set; }

        /// <inheritdoc/>
        public bool HitBoundary { get; private set; }

        /// <inheritdoc/>
        public void Solve(Vector<double> gradient, Matrix<double> hessian, double delta)
        {
            // Get the dimension of the problem
            var n = gradient.Count;

            try
            {
                // Perform eigenvalue decomposition of the Hessian
                var evd = hessian.Evd();
                var Q = evd.EigenVectors;
                var eigvals = evd.EigenValues.Real();

                // Transform the gradient to the eigenvector basis
                var gHat = Q.Transpose() * gradient;

                // Try the interior solution first
                var isInteriorValid = true;
                var pInterior = Vector<double>.Build.Dense(n);

                // Tolerance for numerical stability
                const double epsilon = 1e-12;

                for (var i = 0; i < n; i++)
                {
                    if (Math.Abs(eigvals[i]) < epsilon)
                    {
                        // Handle zero eigenvalues
                        if (Math.Abs(gHat[i]) > epsilon)
                        {
                            // If gHat[i] != 0 when λ_i = 0, Newton step goes to infinity
                            isInteriorValid = false;
                            break;
                        }
                        pInterior[i] = 0;
                    }
                    else if (eigvals[i] < 0)
                    {
                        // Negative eigenvalue implies the Hessian is not positive definite
                        isInteriorValid = false;
                        break;
                    }
                    else
                    {
                        // Compute the Newton step component
                        pInterior[i] = -gHat[i] / eigvals[i];
                    }
                }

                if (isInteriorValid)
                {
                    // Transform back to original coordinates
                    var p = Q * pInterior;

                    // Check if the solution is within the trust region
                    if (p.L2Norm() <= delta)
                    {
                        Pstep = p;
                        HitBoundary = false;
                        return;
                    }
                }

                // If the Newton step is outside the trust region or invalid,
                // solve the constrained problem with the solution on the boundary

                // Find the optimal Lagrange multiplier lambda using a root-finding approach
                var lambda = FindOptimalLambda(eigvals, gHat, delta);

                // Compute the solution with this lambda
                var pBoundary = Vector<double>.Build.Dense(n);
                for (var i = 0; i < n; i++)
                {
                    var denominator = eigvals[i] + lambda;
                    if (Math.Abs(denominator) < epsilon)
                    {
                        // Handle potential division by near-zero
                        pBoundary[i] = gHat[i] > 0 ? -delta : delta;
                    }
                    else
                    {
                        pBoundary[i] = -gHat[i] / denominator;
                    }
                }

                // Transform back to original coordinates
                Pstep = Q * pBoundary;
                HitBoundary = true;
            }
            catch (Exception)
            {
                // Fallback to a more robust method if eigenvalue decomposition fails
                // For example, use a steepest descent step
                var alpha = gradient.DotProduct(gradient) / (hessian * gradient).DotProduct(gradient);
                Pstep = -Math.Min(alpha, delta / gradient.L2Norm()) * gradient;
                HitBoundary = alpha * gradient.L2Norm() >= delta;
            }
        }

        /// <summary>
        /// Finds the optimal Lagrange multiplier to ensure the solution lies on the trust region boundary.
        /// </summary>
        /// <param name="eigvals">Eigenvalues of the Hessian</param>
        /// <param name="gHat">Gradient in the eigenvector basis</param>
        /// <param name="delta">Trust region radius</param>
        /// <returns>The optimal Lagrange multiplier</returns>
        private static double FindOptimalLambda(Vector<double> eigvals, Vector<double> gHat, double delta)
        {
            // Tolerance for numerical stability
            const double epsilon = 1e-12;
            const int maxIterations = 100;
            const double tolerance = 1e-10;

            // Function to evaluate the norm of the solution for a given lambda
            double PhiFunction(double lambda)
            {
                var sum = 0.0;
                for (var i = 0; i < eigvals.Count; i++)
                {
                    var denominator = eigvals[i] + lambda;
                    if (Math.Abs(denominator) > epsilon)
                    {
                        var term = gHat[i] / denominator;
                        sum += term * term;
                    }
                    else if (Math.Abs(gHat[i]) > epsilon)
                    {
                        // If denominator is near zero but gHat[i] is not, 
                        // this would result in a very large term
                        return double.PositiveInfinity;
                    }
                }
                return Math.Sqrt(sum) - delta;
            }

            // Find the minimum eigenvalue to establish a lower bound for lambda
            var lambdaMin = -eigvals.Min();

            // Start with lambdaMin plus a small value to ensure positive definiteness
            var lambdaLower = Math.Max(0.0, lambdaMin + epsilon);
            var lambdaUpper = lambdaLower + 1.0;

            // Expand the upper bound until phi(lambdaUpper) < 0
            var expandCount = 0;
            while (PhiFunction(lambdaUpper) > 0 && expandCount < 30)
            {
                lambdaLower = lambdaUpper;
                lambdaUpper *= 10;
                expandCount++;
            }

            if (expandCount >= 30)
            {
                // If we can't find a suitable upper bound, use a fallback value
                return lambdaUpper;
            }

            // Use bisection method to find the root of the phi function
            var lambdaMid = 0.0;
            double phiMid;
            for (var i = 0; i < maxIterations; i++)
            {
                lambdaMid = (lambdaLower + lambdaUpper) / 2.0;
                phiMid = PhiFunction(lambdaMid);

                if (Math.Abs(phiMid) < tolerance)
                {
                    break;
                }

                if (phiMid > 0)
                {
                    lambdaLower = lambdaMid;
                }
                else
                {
                    lambdaUpper = lambdaMid;
                }
            }

            return lambdaMid;
        }
    }
}
