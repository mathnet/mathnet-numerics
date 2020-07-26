// <copyright file="BiCgStab.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2013 Math.NET
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

using System;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace MathNet.Numerics.LinearAlgebra.Complex.Solvers
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// A Bi-Conjugate Gradient stabilized iterative matrix solver.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Bi-Conjugate Gradient Stabilized (BiCGStab) solver is an 'improvement'
    /// of the standard Conjugate Gradient (CG) solver. Unlike the CG solver the
    /// BiCGStab can be used on non-symmetric matrices. <br/>
    /// Note that much of the success of the solver depends on the selection of the
    /// proper preconditioner.
    /// </para>
    /// <para>
    /// The Bi-CGSTAB algorithm was taken from: <br/>
    /// Templates for the solution of linear systems: Building blocks
    /// for iterative methods
    /// <br/>
    /// Richard Barrett, Michael Berry, Tony F. Chan, James Demmel,
    /// June M. Donato, Jack Dongarra, Victor Eijkhout, Roldan Pozo,
    /// Charles Romine and Henk van der Vorst
    /// <br/>
    /// Url: <a href="http://www.netlib.org/templates/Templates.html">http://www.netlib.org/templates/Templates.html</a>
    /// <br/>
    /// Algorithm is described in Chapter 2, section 2.3.8, page 27
    /// </para>
    /// <para>
    /// The example code below provides an indication of the possible use of the
    /// solver.
    /// </para>
    /// </remarks>
    public sealed class BiCgStab : IIterativeSolver<Complex>
    {
        /// <summary>
        /// Calculates the <c>true</c> residual of the matrix equation Ax = b according to: residual = b - Ax
        /// </summary>
        /// <param name="matrix">Instance of the <see cref="Matrix"/> A.</param>
        /// <param name="residual">Residual values in <see cref="Vector"/>.</param>
        /// <param name="x">Instance of the <see cref="Vector"/> x.</param>
        /// <param name="b">Instance of the <see cref="Vector"/> b.</param>
        static void CalculateTrueResidual(Matrix<Complex> matrix, Vector<Complex> residual, Vector<Complex> x, Vector<Complex> b)
        {
            // -Ax = residual
            matrix.Multiply(x, residual);

            // Do not use residual = residual.Negate() because it creates another object
            residual.Multiply(-1, residual);

            // residual + b
            residual.Add(b, residual);
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient <see cref="Matrix"/>, <c>A</c>.</param>
        /// <param name="input">The solution <see cref="Vector"/>, <c>b</c>.</param>
        /// <param name="result">The result <see cref="Vector"/>, <c>x</c>.</param>
        /// <param name="iterator">The iterator to use to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        public void Solve(Matrix<Complex> matrix, Vector<Complex> input, Vector<Complex> result, Iterator<Complex> iterator, IPreconditioner<Complex> preconditioner)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.", nameof(matrix));
            }

            if (result.Count != input.Count)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (input.Count != matrix.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, result);
            }

            if (iterator == null)
            {
                iterator = new Iterator<Complex>();
            }

            if (preconditioner == null)
            {
                preconditioner = new UnitPreconditioner<Complex>();
            }

            preconditioner.Initialize(matrix);

            // Compute r_0 = b - Ax_0 for some initial guess x_0
            // In this case we take x_0 = vector
            // This is basically a SAXPY so it could be made a lot faster
            var residuals = new DenseVector(matrix.RowCount);
            CalculateTrueResidual(matrix, residuals, result, input);

            // Choose r~ (for example, r~ = r_0)
            var tempResiduals = residuals.Clone();

            // create seven temporary vectors needed to hold temporary
            // coefficients. All vectors are mangled in each iteration.
            // These are defined here to prevent stressing the garbage collector
            var vecP = new DenseVector(residuals.Count);
            var vecPdash = new DenseVector(residuals.Count);
            var nu = new DenseVector(residuals.Count);
            var vecS = new DenseVector(residuals.Count);
            var vecSdash = new DenseVector(residuals.Count);
            var temp = new DenseVector(residuals.Count);
            var temp2 = new DenseVector(residuals.Count);

            // create some temporary double variables that are needed
            // to hold values in between iterations
            Complex currentRho = 0;
            Complex alpha = 0;
            Complex omega = 0;

            var iterationNumber = 0;
            while (iterator.DetermineStatus(iterationNumber, result, input, residuals) == IterationStatus.Continue)
            {
                // rho_(i-1) = r~^T r_(i-1) // dotproduct r~ and r_(i-1)
                var oldRho = currentRho;
                currentRho = tempResiduals.ConjugateDotProduct(residuals);

                // if (rho_(i-1) == 0) // METHOD FAILS
                // If rho is only 1 ULP from zero then we fail.
                if (currentRho.Real.AlmostEqualNumbersBetween(0, 1) && currentRho.Imaginary.AlmostEqualNumbersBetween(0, 1))
                {
                    // Rho-type breakdown
                    throw new NumericalBreakdownException();
                }

                if (iterationNumber != 0)
                {
                    // beta_(i-1) = (rho_(i-1)/rho_(i-2))(alpha_(i-1)/omega(i-1))
                    var beta = (currentRho/oldRho)*(alpha/omega);

                    // p_i = r_(i-1) + beta_(i-1)(p_(i-1) - omega_(i-1) * nu_(i-1))
                    nu.Multiply(-omega, temp);
                    vecP.Add(temp, temp2);
                    temp2.CopyTo(vecP);

                    vecP.Multiply(beta, vecP);
                    vecP.Add(residuals, temp2);
                    temp2.CopyTo(vecP);
                }
                else
                {
                    // p_i = r_(i-1)
                    residuals.CopyTo(vecP);
                }

                // SOLVE Mp~ = p_i // M = preconditioner
                preconditioner.Approximate(vecP, vecPdash);

                // nu_i = Ap~
                matrix.Multiply(vecPdash, nu);

                // alpha_i = rho_(i-1)/ (r~^T nu_i) = rho / dotproduct(r~ and nu_i)
                alpha = currentRho*1/tempResiduals.ConjugateDotProduct(nu);

                // s = r_(i-1) - alpha_i nu_i
                nu.Multiply(-alpha, temp);
                residuals.Add(temp, vecS);

                // Check if we're converged. If so then stop. Otherwise continue;
                // Calculate the temporary result.
                // Be careful not to change any of the temp vectors, except for
                // temp. Others will be used in the calculation later on.
                // x_i = x_(i-1) + alpha_i * p^_i + s^_i
                vecPdash.Multiply(alpha, temp);
                temp.Add(vecSdash, temp2);
                temp2.CopyTo(temp);
                temp.Add(result, temp2);
                temp2.CopyTo(temp);

                // Check convergence and stop if we are converged.
                if (iterator.DetermineStatus(iterationNumber, temp, input, vecS) != IterationStatus.Continue)
                {
                    temp.CopyTo(result);

                    // Calculate the true residual
                    CalculateTrueResidual(matrix, residuals, result, input);

                    // Now recheck the convergence
                    if (iterator.DetermineStatus(iterationNumber, result, input, residuals) != IterationStatus.Continue)
                    {
                        // We're all good now.
                        return;
                    }

                    // Continue the calculation
                    iterationNumber++;
                    continue;
                }

                // SOLVE Ms~ = s
                preconditioner.Approximate(vecS, vecSdash);

                // temp = As~
                matrix.Multiply(vecSdash, temp);

                // omega_i = temp^T s / temp^T temp
                omega = temp.ConjugateDotProduct(vecS)/temp.ConjugateDotProduct(temp);

                // x_i = x_(i-1) + alpha_i p^ + omega_i s^
                temp.Multiply(-omega, residuals);
                residuals.Add(vecS, temp2);
                temp2.CopyTo(residuals);

                vecSdash.Multiply(omega, temp);
                result.Add(temp, temp2);
                temp2.CopyTo(result);

                vecPdash.Multiply(alpha, temp);
                result.Add(temp, temp2);
                temp2.CopyTo(result);

                // for continuation it is necessary that omega_i != 0.0
                // If omega is only 1 ULP from zero then we fail.
                if (omega.Real.AlmostEqualNumbersBetween(0, 1) && omega.Imaginary.AlmostEqualNumbersBetween(0, 1))
                {
                    // Omega-type breakdown
                    throw new NumericalBreakdownException();
                }

                if (iterator.DetermineStatus(iterationNumber, result, input, residuals) != IterationStatus.Continue)
                {
                    // Recalculate the residuals and go round again. This is done to ensure that
                    // we have the proper residuals.
                    // The residual calculation based on omega_i * s can be off by a factor 10. So here
                    // we calculate the real residual (which can be expensive) but we only do it if we're
                    // sufficiently close to the finish.
                    CalculateTrueResidual(matrix, residuals, result, input);
                }

                iterationNumber++;
            }
        }
    }
}
