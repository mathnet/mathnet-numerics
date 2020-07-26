// <copyright file="GpBiCg.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex32.Solvers
{
    /// <summary>
    /// A Generalized Product Bi-Conjugate Gradient iterative matrix solver.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Generalized Product Bi-Conjugate Gradient (GPBiCG) solver is an
    /// alternative version of the Bi-Conjugate Gradient stabilized (CG) solver.
    /// Unlike the CG solver the GPBiCG solver can be used on
    /// non-symmetric matrices. <br/>
    /// Note that much of the success of the solver depends on the selection of the
    /// proper preconditioner.
    /// </para>
    /// <para>
    /// The GPBiCG algorithm was taken from: <br/>
    /// GPBiCG(m,l): A hybrid of BiCGSTAB and GPBiCG methods with
    /// efficiency and robustness
    /// <br/>
    /// S. Fujino
    /// <br/>
    /// Applied Numerical Mathematics, Volume 41, 2002, pp 107 - 117
    /// <br/>
    /// </para>
    /// <para>
    /// The example code below provides an indication of the possible use of the
    /// solver.
    /// </para>
    /// </remarks>
    public sealed class GpBiCg : IIterativeSolver<Numerics.Complex32>
    {
        /// <summary>
        /// Indicates the number of <c>BiCGStab</c> steps should be taken
        /// before switching.
        /// </summary>
        int _numberOfBiCgStabSteps = 1;

        /// <summary>
        /// Indicates the number of <c>GPBiCG</c> steps should be taken
        /// before switching.
        /// </summary>
        int _numberOfGpbiCgSteps = 4;

        /// <summary>
        /// Gets or sets the number of steps taken with the <c>BiCgStab</c> algorithm
        /// before switching over to the <c>GPBiCG</c> algorithm.
        /// </summary>
        public int NumberOfBiCgStabSteps
        {
            get => _numberOfBiCgStabSteps;

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _numberOfBiCgStabSteps = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of steps taken with the <c>GPBiCG</c> algorithm
        /// before switching over to the <c>BiCgStab</c> algorithm.
        /// </summary>
        public int NumberOfGpBiCgSteps
        {
            get => _numberOfGpbiCgSteps;

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _numberOfGpbiCgSteps = value;
            }
        }

        /// <summary>
        /// Calculates the <c>true</c> residual of the matrix equation Ax = b according to: residual = b - Ax
        /// </summary>
        /// <param name="matrix">Instance of the <see cref="Matrix"/> A.</param>
        /// <param name="residual">Residual values in <see cref="Vector"/>.</param>
        /// <param name="x">Instance of the <see cref="Vector"/> x.</param>
        /// <param name="b">Instance of the <see cref="Vector"/> b.</param>
        static void CalculateTrueResidual(Matrix<Numerics.Complex32> matrix, Vector<Numerics.Complex32> residual, Vector<Numerics.Complex32> x, Vector<Numerics.Complex32> b)
        {
            // -Ax = residual
            matrix.Multiply(x, residual);
            residual.Multiply(-1, residual);

            // residual + b
            residual.Add(b, residual);
        }

        /// <summary>
        /// Decide if to do steps with BiCgStab
        /// </summary>
        /// <param name="iterationNumber">Number of iteration</param>
        /// <returns><c>true</c> if yes, otherwise <c>false</c></returns>
        bool ShouldRunBiCgStabSteps(int iterationNumber)
        {
            // Run the first steps as BiCGStab
            // The number of steps past a whole iteration set
            var difference = iterationNumber % (_numberOfBiCgStabSteps + _numberOfGpbiCgSteps);

            // Do steps with BiCGStab if:
            // - The difference is zero or more (i.e. we have done zero or more complete cycles)
            // - The difference is less than the number of BiCGStab steps that should be taken
            return (difference >= 0) && (difference < _numberOfBiCgStabSteps);
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="input">The solution vector, <c>b</c></param>
        /// <param name="result">The result vector, <c>x</c></param>
        /// <param name="iterator">The iterator to use to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        public void Solve(Matrix<Numerics.Complex32> matrix, Vector<Numerics.Complex32> input, Vector<Numerics.Complex32> result, Iterator<Numerics.Complex32> iterator, IPreconditioner<Numerics.Complex32> preconditioner)
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
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            if (iterator == null)
            {
                iterator = new Iterator<Numerics.Complex32>();
            }

            if (preconditioner == null)
            {
                preconditioner = new UnitPreconditioner<Numerics.Complex32>();
            }

            preconditioner.Initialize(matrix);

            // x_0 is initial guess
            // Take x_0 = 0
            var xtemp = new DenseVector(input.Count);

            // r_0 = b - Ax_0
            // This is basically a SAXPY so it could be made a lot faster
            var residuals = new DenseVector(matrix.RowCount);
            CalculateTrueResidual(matrix, residuals, xtemp, input);

            // Define the temporary scalars
            Numerics.Complex32 beta = 0;

            // Define the temporary vectors
            // rDash_0 = r_0
            var rdash = DenseVector.OfVector(residuals);

            // t_-1 = 0
            var t = new DenseVector(residuals.Count);
            var t0 = new DenseVector(residuals.Count);

            // w_-1 = 0
            var w = new DenseVector(residuals.Count);

            // Define the remaining temporary vectors
            var c = new DenseVector(residuals.Count);
            var p = new DenseVector(residuals.Count);
            var s = new DenseVector(residuals.Count);
            var u = new DenseVector(residuals.Count);
            var y = new DenseVector(residuals.Count);
            var z = new DenseVector(residuals.Count);

            var temp = new DenseVector(residuals.Count);
            var temp2 = new DenseVector(residuals.Count);
            var temp3 = new DenseVector(residuals.Count);

            // for (k = 0, 1, .... )
            var iterationNumber = 0;
            while (iterator.DetermineStatus(iterationNumber, xtemp, input, residuals) == IterationStatus.Continue)
            {
                // p_k = r_k + beta_(k-1) * (p_(k-1) - u_(k-1))
                p.Subtract(u, temp);

                temp.Multiply(beta, temp2);
                residuals.Add(temp2, p);

                // Solve M b_k = p_k
                preconditioner.Approximate(p, temp);

                // s_k = A b_k
                matrix.Multiply(temp, s);

                // alpha_k = (r*_0 * r_k) / (r*_0 * s_k)
                var alpha = rdash.ConjugateDotProduct(residuals)/rdash.ConjugateDotProduct(s);

                // y_k = t_(k-1) - r_k - alpha_k * w_(k-1) + alpha_k s_k
                s.Subtract(w, temp);
                t.Subtract(residuals, y);

                temp.Multiply(alpha, temp2);
                y.Add(temp2, temp3);
                temp3.CopyTo(y);

                // Store the old value of t in t0
                t.CopyTo(t0);

                // t_k = r_k - alpha_k s_k
                s.Multiply(-alpha, temp2);
                residuals.Add(temp2, t);

                // Solve M d_k = t_k
                preconditioner.Approximate(t, temp);

                // c_k = A d_k
                matrix.Multiply(temp, c);
                var cdot = c.ConjugateDotProduct(c);

                // cDot can only be zero if c is a zero vector
                // We'll set cDot to 1 if it is zero to prevent NaN's
                // Note that the calculation should continue fine because
                // c.DotProduct(t) will be zero and so will c.DotProduct(y)
                if (cdot.Real.AlmostEqualNumbersBetween(0, 1) && cdot.Imaginary.AlmostEqualNumbersBetween(0, 1))
                {
                    cdot = 1.0f;
                }

                // Even if we don't want to do any BiCGStab steps we'll still have
                // to do at least one at the start to initialize the
                // system, but we'll only have to take special measures
                // if we don't do any so ...
                var ctdot = c.ConjugateDotProduct(t);
                Numerics.Complex32 eta;
                Numerics.Complex32 sigma;
                if (((_numberOfBiCgStabSteps == 0) && (iterationNumber == 0)) || ShouldRunBiCgStabSteps(iterationNumber))
                {
                    // sigma_k = (c_k * t_k) / (c_k * c_k)
                    sigma = ctdot/cdot;

                    // eta_k = 0
                    eta = 0;
                }
                else
                {
                    var ydot = y.ConjugateDotProduct(y);

                    // yDot can only be zero if y is a zero vector
                    // We'll set yDot to 1 if it is zero to prevent NaN's
                    // Note that the calculation should continue fine because
                    // y.DotProduct(t) will be zero and so will c.DotProduct(y)
                    if (ydot.Real.AlmostEqualNumbersBetween(0, 1) && ydot.Imaginary.AlmostEqualNumbersBetween(0, 1))
                    {
                        ydot = 1.0f;
                    }

                    var ytdot = y.ConjugateDotProduct(t);
                    var cydot = c.ConjugateDotProduct(y);

                    var denom = (cdot*ydot) - (cydot*cydot);

                    // sigma_k = ((y_k * y_k)(c_k * t_k) - (y_k * t_k)(c_k * y_k)) / ((c_k * c_k)(y_k * y_k) - (y_k * c_k)(c_k * y_k))
                    sigma = ((ydot*ctdot) - (ytdot*cydot))/denom;

                    // eta_k = ((c_k * c_k)(y_k * t_k) - (y_k * c_k)(c_k * t_k)) / ((c_k * c_k)(y_k * y_k) - (y_k * c_k)(c_k * y_k))
                    eta = ((cdot*ytdot) - (cydot*ctdot))/denom;
                }

                // u_k = sigma_k s_k + eta_k (t_(k-1) - r_k + beta_(k-1) u_(k-1))
                u.Multiply(beta, temp2);
                t0.Add(temp2, temp);

                temp.Subtract(residuals, temp3);
                temp3.CopyTo(temp);
                temp.Multiply(eta, temp);

                s.Multiply(sigma, temp2);
                temp.Add(temp2, u);

                // z_k = sigma_k r_k +_ eta_k z_(k-1) - alpha_k u_k
                z.Multiply(eta, z);
                u.Multiply(-alpha, temp2);
                z.Add(temp2, temp3);
                temp3.CopyTo(z);

                residuals.Multiply(sigma, temp2);
                z.Add(temp2, temp3);
                temp3.CopyTo(z);

                // x_(k+1) = x_k + alpha_k p_k + z_k
                p.Multiply(alpha, temp2);
                xtemp.Add(temp2, temp3);
                temp3.CopyTo(xtemp);

                xtemp.Add(z, temp3);
                temp3.CopyTo(xtemp);

                // r_(k+1) = t_k - eta_k y_k - sigma_k c_k
                // Copy the old residuals to a temp vector because we'll
                // need those in the next step
                residuals.CopyTo(t0);

                y.Multiply(-eta, temp2);
                t.Add(temp2, residuals);

                c.Multiply(-sigma, temp2);
                residuals.Add(temp2, temp3);
                temp3.CopyTo(residuals);

                // beta_k = alpha_k / sigma_k * (r*_0 * r_(k+1)) / (r*_0 * r_k)
                // But first we check if there is a possible NaN. If so just reset beta to zero.
                beta = (!sigma.Real.AlmostEqualNumbersBetween(0, 1) || !sigma.Imaginary.AlmostEqualNumbersBetween(0, 1)) ? alpha/sigma*rdash.ConjugateDotProduct(residuals)/rdash.ConjugateDotProduct(t0) : 0;

                // w_k = c_k + beta_k s_k
                s.Multiply(beta, temp2);
                c.Add(temp2, w);

                // Get the real value
                preconditioner.Approximate(xtemp, result);

                // Now check for convergence
                if (iterator.DetermineStatus(iterationNumber, result, input, residuals) != IterationStatus.Continue)
                {
                    // Recalculate the residuals and go round again. This is done to ensure that
                    // we have the proper residuals.
                    CalculateTrueResidual(matrix, residuals, result, input);
                }

                // Next iteration.
                iterationNumber++;
            }
        }
    }
}
