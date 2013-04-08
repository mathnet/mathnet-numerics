// <copyright file="GpBiCg.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex.Solvers.Iterative
{
    using System;
    using System.Numerics;
    using Generic.Solvers.Status;
    using Preconditioners;
    using Properties;

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
    public sealed class GpBiCg : IIterativeSolver
    {
        /// <summary>
        /// The status used if there is no status, i.e. the solver hasn't run yet and there is no
        /// iterator.
        /// </summary>
        static readonly ICalculationStatus DefaultStatus = new CalculationIndetermined();

        /// <summary>
        /// The preconditioner that will be used. Can be set to <c>null</c>, in which case the default
        /// pre-conditioner will be used.
        /// </summary>
        IPreConditioner _preconditioner;

        /// <summary>
        /// The iterative process controller.
        /// </summary>
        IIterator _iterator;

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
        /// Indicates if the user has stopped the solver.
        /// </summary>
        bool _hasBeenStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="GpBiCg"/> class.
        /// </summary>
        /// <remarks>
        /// When using this constructor the solver will use the <see cref="IIterator"/> with
        /// the standard settings and a default preconditioner.
        /// </remarks>
        public GpBiCg()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpBiCg"/> class.
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
        /// <param name="iterator">The <see cref="IIterator"/> that will be used to monitor the iterative process.</param>
        public GpBiCg(IIterator iterator)
            : this(null, iterator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpBiCg"/> class.
        /// </summary>
        /// <remarks>
        /// When using this constructor the solver will use the <see cref="IIterator"/> with
        /// the standard settings.
        /// </remarks>
        /// <param name="preconditioner">The <see cref="IPreConditioner"/> that will be used to precondition the matrix equation.</param>
        public GpBiCg(IPreConditioner preconditioner)
            : this(preconditioner, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpBiCg"/> class.
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
        /// <param name="preconditioner">The <see cref="IPreConditioner"/> that will be used to precondition the matrix equation.</param>
        /// <param name="iterator">The <see cref="IIterator"/> that will be used to monitor the iterative process.</param>
        public GpBiCg(IPreConditioner preconditioner, IIterator iterator)
        {
            _iterator = iterator;
            _preconditioner = preconditioner;
        }

        /// <summary>
        /// Gets or sets the number of steps taken with the <c>BiCgStab</c> algorithm
        /// before switching over to the <c>GPBiCG</c> algorithm.
        /// </summary>
        public int NumberOfBiCgStabSteps
        {
            get { return _numberOfBiCgStabSteps; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
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
            get { return _numberOfGpbiCgSteps; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _numberOfGpbiCgSteps = value;
            }
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
            get { return (_iterator != null) ? _iterator.Status : DefaultStatus; }
        }

        /// <summary>
        /// Stops the solve process. 
        /// </summary>
        /// <remarks>
        /// Note that it may take an indetermined amount of time for the solver to actually
        /// stop the process.
        /// </remarks>
        public void StopSolve()
        {
            _hasBeenStopped = true;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="vector">The solution vector, <c>b</c>.</param>
        /// <returns>The result vector, <c>x</c>.</returns>
        public Vector Solve(Matrix matrix, Vector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException();
            }

            Vector result = new DenseVector(matrix.RowCount);
            Solve(matrix, vector, result);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="input">The solution vector, <c>b</c></param>
        /// <param name="result">The result vector, <c>x</c></param>
        public void Solve(Matrix matrix, Vector input, Vector result)
        {
            // If we were stopped before, we are no longer
            // We're doing this at the start of the method to ensure
            // that we can use these fields immediately.
            _hasBeenStopped = false;

            // Error checks
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

            if (input.Count != matrix.RowCount || result.Count != input.Count)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix, input, result);
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

            // x_0 is initial guess
            // Take x_0 = 0
            Vector xtemp = new DenseVector(input.Count);

            // r_0 = b - Ax_0
            // This is basically a SAXPY so it could be made a lot faster
            Vector residuals = new DenseVector(matrix.RowCount);
            CalculateTrueResidual(matrix, residuals, xtemp, input);

            // Define the temporary scalars
            Complex beta = 0;

            // Define the temporary vectors
            // rDash_0 = r_0
            Vector rdash = DenseVector.OfVector(residuals);

            // t_-1 = 0
            Vector t = new DenseVector(residuals.Count);
            Vector t0 = new DenseVector(residuals.Count);

            // w_-1 = 0
            Vector w = new DenseVector(residuals.Count);

            // Define the remaining temporary vectors
            Vector c = new DenseVector(residuals.Count);
            Vector p = new DenseVector(residuals.Count);
            Vector s = new DenseVector(residuals.Count);
            Vector u = new DenseVector(residuals.Count);
            Vector y = new DenseVector(residuals.Count);
            Vector z = new DenseVector(residuals.Count);

            Vector temp = new DenseVector(residuals.Count);
            Vector temp2 = new DenseVector(residuals.Count);
            Vector temp3 = new DenseVector(residuals.Count);

            // for (k = 0, 1, .... )
            var iterationNumber = 0;
            while (ShouldContinue(iterationNumber, xtemp, input, residuals))
            {
                // p_k = r_k + beta_(k-1) * (p_(k-1) - u_(k-1))
                p.Subtract(u, temp);

                temp.Multiply(beta, temp2);
                residuals.Add(temp2, p);

                // Solve M b_k = p_k
                _preconditioner.Approximate(p, temp);

                // s_k = A b_k
                matrix.Multiply(temp, s);

                // alpha_k = (r*_0 * r_k) / (r*_0 * s_k)
                var alpha = rdash.DotProduct(residuals)/rdash.DotProduct(s);

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
                _preconditioner.Approximate(t, temp);

                // c_k = A d_k
                matrix.Multiply(temp, c);
                var cdot = c.DotProduct(c);

                // cDot can only be zero if c is a zero vector
                // We'll set cDot to 1 if it is zero to prevent NaN's
                // Note that the calculation should continue fine because
                // c.DotProduct(t) will be zero and so will c.DotProduct(y)
                if (cdot.Real.AlmostEqual(0, 1) && cdot.Imaginary.AlmostEqual(0, 1))
                {
                    cdot = 1.0;
                }

                // Even if we don't want to do any BiCGStab steps we'll still have
                // to do at least one at the start to initialize the
                // system, but we'll only have to take special measures
                // if we don't do any so ...
                var ctdot = c.DotProduct(t);
                Complex eta;
                Complex sigma;
                if (((_numberOfBiCgStabSteps == 0) && (iterationNumber == 0)) || ShouldRunBiCgStabSteps(iterationNumber))
                {
                    // sigma_k = (c_k * t_k) / (c_k * c_k)
                    sigma = ctdot/cdot;

                    // eta_k = 0
                    eta = 0;
                }
                else
                {
                    var ydot = y.DotProduct(y);

                    // yDot can only be zero if y is a zero vector
                    // We'll set yDot to 1 if it is zero to prevent NaN's
                    // Note that the calculation should continue fine because
                    // y.DotProduct(t) will be zero and so will c.DotProduct(y)
                    if (ydot.Real.AlmostEqual(0, 1) && ydot.Imaginary.AlmostEqual(0, 1))
                    {
                        ydot = 1.0;
                    }

                    var ytdot = y.DotProduct(t);
                    var cydot = c.DotProduct(y);

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
                beta = (!sigma.Real.AlmostEqual(0, 1) || !sigma.Imaginary.AlmostEqual(0, 1)) ? alpha/sigma*rdash.DotProduct(residuals)/rdash.DotProduct(t0) : 0;

                // w_k = c_k + beta_k s_k
                s.Multiply(beta, temp2);
                c.Add(temp2, w);

                // Get the real value
                _preconditioner.Approximate(xtemp, result);

                // Now check for convergence
                if (!ShouldContinue(iterationNumber, result, input, residuals))
                {
                    // Recalculate the residuals and go round again. This is done to ensure that
                    // we have the proper residuals.
                    CalculateTrueResidual(matrix, residuals, result, input);
                }

                // Next iteration.
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
        static void CalculateTrueResidual(Matrix matrix, Vector residual, Vector x, Vector b)
        {
            // -Ax = residual
            matrix.Multiply(x, residual);
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
        bool ShouldContinue(int iterationNumber, Vector result, Vector source, Vector residuals)
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
        /// Decide if to do steps with BiCgStab
        /// </summary>
        /// <param name="iterationNumber">Number of iteration</param>
        /// <returns><c>true</c> if yes, otherwise <c>false</c></returns>
        bool ShouldRunBiCgStabSteps(int iterationNumber)
        {
            // Run the first steps as BiCGStab
            // The number of steps past a whole iteration set
            var difference = iterationNumber%(_numberOfBiCgStabSteps + _numberOfGpbiCgSteps);

            // Do steps with BiCGStab if:
            // - The difference is zero or more (i.e. we have done zero or more complete cycles)
            // - The difference is less than the number of BiCGStab steps that should be taken
            return (difference >= 0) && (difference < _numberOfBiCgStabSteps);
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix, B is the
        /// solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="input">The solution matrix, <c>B</c>.</param>
        /// <returns>The result matrix, <c>X</c>.</returns>
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

            var result = (Matrix) matrix.CreateMatrix(input.RowCount, input.ColumnCount);
            Solve(matrix, input, result);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix, B is the
        /// solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="input">The solution matrix, <c>B</c>.</param>
        /// <param name="result">The result matrix, <c>X</c></param>
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
                var solution = Solve(matrix, (Vector) input.Column(column));
                foreach (var element in solution.GetIndexedEnumerator())
                {
                    result.At(element.Item1, column, element.Item2);
                }
            }
        }
    }
}
