// <copyright file="BiCgStab.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.Double.Solvers.Iterative
{
    using System;
    using Generic.Solvers.Status;
    using Preconditioners;
    using Properties;

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
    public sealed class BiCgStab : IIterativeSolver
    {
        /// <summary>
        /// The status used if there is no status, i.e. the solver hasn't run yet and there is no
        /// iterator.
        /// </summary>
        private static readonly ICalculationStatus DefaultStatus = new CalculationIndetermined();

        /// <summary>
        /// The preconditioner that will be used. Can be set to <see langword="null" />, in which case the default
        /// pre-conditioner will be used.
        /// </summary>
        private IPreConditioner _preconditioner;

        /// <summary>
        /// The iterative process controller.
        /// </summary>
        private IIterator _iterator;

        /// <summary>
        /// Indicates if the user has stopped the solver.
        /// </summary>
        private bool _hasBeenStopped;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BiCgStab"/> class.
        /// </summary>
        /// <remarks>
        /// When using this constructor the solver will use the <see cref="IIterator"/> with
        /// the standard settings and a default preconditioner.
        /// </remarks>
        public BiCgStab() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiCgStab"/> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When using this constructor the solver will use a default preconditioner.
        /// </para>
        /// <para>
        /// The main advantages of using a user defined <see cref="IIterator"/> are:
        /// <list type="number">
        /// <item>It is possible to set the desired convergence limits.</item>
        /// <item>
        /// It is possible to check the reason for which the solver finished 
        /// the iterative procedure by calling the <see cref="IIterator.Status"/> property.
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="iterator">The <see cref="IIterator"/> that will be used to monitor the iterative process. </param>
        public BiCgStab(IIterator iterator) : this(null, iterator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiCgStab"/> class.
        /// </summary>
        /// <remarks>
        /// When using this constructor the solver will use the <see cref="IIterator"/> with
        /// the standard settings.
        /// </remarks>
        /// <param name="preconditioner">The <see cref="IPreConditioner"/> that will be used to precondition the matrix equation.</param>
        public BiCgStab(IPreConditioner preconditioner) : this(preconditioner, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiCgStab"/> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The main advantages of using a user defined <see cref="IIterator"/> are:
        /// <list type="number">
        /// <item>It is possible to set the desired convergence limits.</item>
        /// <item>
        /// It is possible to check the reason for which the solver finished 
        /// the iterative procedure by calling the <see cref="IIterator.Status"/> property.
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="preconditioner">The <see cref="IPreConditioner"/> that will be used to precondition the matrix equation. </param>
        /// <param name="iterator">The <see cref="IIterator"/> that will be used to monitor the iterative process. </param>
        public BiCgStab(IPreConditioner preconditioner, IIterator iterator)
        {
            _iterator = iterator;
            _preconditioner = preconditioner;
        }

        /// <summary>
        /// Sets the <see cref="IPreConditioner"/> that will be used to precondition the iterative process.
        /// </summary>
        /// <param name="preconditioner">The preconditioner.</param>
        public void SetPreconditioner(IPreConditioner preconditioner)
        {
            _preconditioner = preconditioner;
        }

        /// <summary>
        /// Sets the <see cref="IIterator"/> that will be used to track the iterative process.
        /// </summary>
        /// <param name="iterator">The iterator.</param>
        public void SetIterator(IIterator iterator)
        {
            _iterator = iterator;
        }

        /// <summary>
        /// Gets the status of the iteration once the calculation is finished.
        /// </summary>
        public ICalculationStatus IterationResult
        {
            get 
            { 
                return (_iterator != null) ? _iterator.Status : DefaultStatus; 
            }
        }

        /// <summary>
        /// Stops the solve process. 
        /// </summary>
        /// <remarks>
        /// It may take an indetermined amount of time for the solver to actually stop the process.
        /// </remarks>
        public void StopSolve()
        {
            _hasBeenStopped = true;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient <see cref="Matrix"/>, <c>A</c>.</param>
        /// <param name="vector">The solution <see cref="Vector"/>, <c>b</c>.</param>
        /// <returns>The result <see cref="Vector"/>, <c>x</c>.</returns>
        public Vector Solve(Matrix matrix, Vector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException();
            }

            var result = new DenseVector(matrix.RowCount);
            Solve(matrix, vector, result);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient <see cref="Matrix"/>, <c>A</c>.</param>
        /// <param name="input">The solution <see cref="Vector"/>, <c>b</c>.</param>
        /// <param name="result">The result <see cref="Vector"/>, <c>x</c>.</param>
        public void Solve(Matrix matrix, Vector input, Vector result)
        {
            // If we were stopped before, we are no longer
            // We're doing this at the start of the method to ensure
            // that we can use these fields immediately.
            _hasBeenStopped = false;

            // Parameters checks
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, "matrix");
            }

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.Count != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (input.Count != matrix.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            // Initialize the solver fields
            // Set the convergence monitor
            if (_iterator == null)
            {
                _iterator = Iterator.CreateDefault();
            }

            if (_preconditioner == null)
            {
                _preconditioner = new UnitPreconditioner();
            }
            
            _preconditioner.Initialize(matrix);
            
            // Compute r_0 = b - Ax_0 for some initial guess x_0
            // In this case we take x_0 = vector
            // This is basically a SAXPY so it could be made a lot faster
            Vector residuals = new DenseVector(matrix.RowCount);
            CalculateTrueResidual(matrix, residuals, result, input);

            // Choose r~ (for example, r~ = r_0)
            var tempResiduals = residuals.Clone();
            
            // create seven temporary vectors needed to hold temporary
            // coefficients. All vectors are mangled in each iteration.
            // These are defined here to prevent stressing the garbage collector
            Vector vecP = new DenseVector(residuals.Count);
            Vector vecPdash = new DenseVector(residuals.Count);
            Vector nu = new DenseVector(residuals.Count);
            Vector vecS = new DenseVector(residuals.Count);
            Vector vecSdash = new DenseVector(residuals.Count);
            Vector temp = new DenseVector(residuals.Count);
            Vector temp2 = new DenseVector(residuals.Count);

            // create some temporary double variables that are needed
            // to hold values in between iterations
            double currentRho = 0;
            double alpha = 0;
            double omega = 0;

            var iterationNumber = 0;
            while (ShouldContinue(iterationNumber, result, input, residuals))
            {
                // rho_(i-1) = r~^T r_(i-1) // dotproduct r~ and r_(i-1)
                var oldRho = currentRho;
                currentRho = tempResiduals.DotProduct(residuals);

                // if (rho_(i-1) == 0) // METHOD FAILS
                // If rho is only 1 ULP from zero then we fail.
                if (currentRho.AlmostEqual(0, 1))
                {
                    // Rho-type breakdown
                    throw new Exception("Iterative solver experience a numerical break down");
                }

                if (iterationNumber != 0)
                {
                    // beta_(i-1) = (rho_(i-1)/rho_(i-2))(alpha_(i-1)/omega(i-1))
                    var beta = (currentRho / oldRho) * (alpha / omega);

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
                _preconditioner.Approximate(vecP, vecPdash);
                
                // nu_i = Ap~
                matrix.Multiply(vecPdash, nu);

                // alpha_i = rho_(i-1)/ (r~^T nu_i) = rho / dotproduct(r~ and nu_i)
                alpha = currentRho * 1 / tempResiduals.DotProduct(nu);

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
                if (!ShouldContinue(iterationNumber, temp, input, vecS))
                {
                    temp.CopyTo(result);

                    // Calculate the true residual
                    CalculateTrueResidual(matrix, residuals, result, input);

                    // Now recheck the convergence
                    if (!ShouldContinue(iterationNumber, result, input, residuals))
                    {
                        // We're all good now.
                        return;
                    }

                    // Continue the calculation
                    iterationNumber++;
                    continue;
                }

                // SOLVE Ms~ = s
                _preconditioner.Approximate(vecS, vecSdash);

                // temp = As~
                matrix.Multiply(vecSdash, temp);

                // omega_i = temp^T s / temp^T temp
                omega = temp.DotProduct(vecS) / temp.DotProduct(temp);

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
                if (omega.AlmostEqual(0, 1))
                {
                    // Omega-type breakdown
                    throw new Exception("Iterative solver experience a numerical break down");
                }

                if (!ShouldContinue(iterationNumber, result, input, residuals))
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

        /// <summary>
        /// Calculates the <c>true</c> residual of the matrix equation Ax = b according to: residual = b - Ax
        /// </summary>
        /// <param name="matrix">Instance of the <see cref="Matrix"/> A.</param>
        /// <param name="residual">Residual values in <see cref="Vector"/>.</param>
        /// <param name="x">Instance of the <see cref="Vector"/> x.</param>
        /// <param name="b">Instance of the <see cref="Vector"/> b.</param>
        private static void CalculateTrueResidual(Matrix matrix, Vector residual, Vector x, Vector b)
        {
            // -Ax = residual
            matrix.Multiply(x, residual);
            
            // Do not use residual = residual.Negate() because it creates another object
            residual.Multiply(-1, residual);

            // residual + b
            residual.Add(b, residual);
        }

        /// <summary>
        /// Determine if calculation should continue
        /// </summary>
        /// <param name="iterationNumber">Number of iterations passed</param>
        /// <param name="result">Result <see cref="Vector"/>.</param>
        /// <param name="source">Source <see cref="Vector"/>.</param>
        /// <param name="residuals">Residual <see cref="Vector"/>.</param>
        /// <returns><c>true</c> if continue, otherwise <c>false</c></returns>
        private bool ShouldContinue(int iterationNumber, Vector result, Vector source, Vector residuals)
        {
            if (_hasBeenStopped)
            {
                _iterator.IterationCancelled();
                return true;
            }

            _iterator.DetermineStatus(iterationNumber, result, source, residuals);
            var status = _iterator.Status;

            // We stop if either:
            // - the user has stopped the calculation
            // - the calculation needs to be stopped from a numerical point of view (divergence, convergence etc.)
            return (!status.TerminatesCalculation) && (!_hasBeenStopped);
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix, B is the
        /// solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="matrix">The coefficient <see cref="Matrix"/>, <c>A</c>.</param>
        /// <param name="input">The solution <see cref="Matrix"/>, <c>B</c>.</param>
        /// <returns>The result <see cref="Matrix"/>, <c>X</c>.</returns>
        public Matrix Solve(Matrix matrix, Matrix input)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var result = (Matrix)matrix.CreateMatrix(input.RowCount, input.ColumnCount);
            Solve(matrix, input, result);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix, B is the
        /// solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="matrix">The coefficient <see cref="Matrix"/>, <c>A</c>.</param>
        /// <param name="input">The solution <see cref="Matrix"/>, <c>B</c>.</param>
        /// <param name="result">The result <see cref="Matrix"/>, <c>X</c></param>
        public void Solve(Matrix matrix, Matrix input, Matrix result)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (matrix.RowCount != input.RowCount || input.RowCount != result.RowCount || input.ColumnCount != result.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix, input, result);
            }

            for (var column = 0; column < input.ColumnCount; column++)
            {
                var solution = Solve(matrix, (Vector)input.Column(column));
                foreach (var element in solution.GetIndexedEnumerator())
                {
                    result.At(element.Item1, column, element.Item2);
                }
            }
        }
    }
}
