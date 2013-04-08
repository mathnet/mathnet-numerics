// <copyright file="TFQMR.cs" company="Math.NET">
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
    /// A Transpose Free Quasi-Minimal Residual (TFQMR) iterative matrix solver.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The TFQMR algorithm was taken from: <br/>
    /// Iterative methods for sparse linear systems.
    /// <br/>
    /// Yousef Saad
    /// <br/>
    /// Algorithm is described in Chapter 7, section 7.4.3, page 219
    /// </para>
    /// <para>
    /// The example code below provides an indication of the possible use of the
    /// solver.
    /// </para>
    /// </remarks>
    public sealed class TFQMR : IIterativeSolver
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
        /// Initializes a new instance of the <see cref="TFQMR"/> class.
        /// </summary>
        /// <remarks>
        /// When using this constructor the solver will use the <see cref="IIterator"/> with
        /// the standard settings and a default preconditioner.
        /// </remarks>
        public TFQMR() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TFQMR"/> class.
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
        public TFQMR(IIterator iterator) : this(null, iterator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TFQMR"/> class.
        /// </summary>
        /// <remarks>
        /// When using this constructor the solver will use the <see cref="IIterator"/> with
        /// the standard settings.
        /// </remarks>
        /// <param name="preconditioner">The <see cref="IPreConditioner"/> that will be used to precondition the matrix equation.</param>
        public TFQMR(IPreConditioner preconditioner) : this(preconditioner, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TFQMR"/> class.
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
        public TFQMR(IPreConditioner preconditioner, IIterator iterator)
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
        /// Note that it may take an indetermined amount of time for the solver to actually stop the process.
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

            var d = new DenseVector(input.Count);
            var r = DenseVector.OfVector(input);

            var uodd = new DenseVector(input.Count);
            var ueven = new DenseVector(input.Count);

            var v = new DenseVector(input.Count);
            var pseudoResiduals = DenseVector.OfVector(input);

            var x = new DenseVector(input.Count);
            var yodd = new DenseVector(input.Count);
            var yeven = DenseVector.OfVector(input);

            // Temp vectors
            var temp = new DenseVector(input.Count);
            var temp1 = new DenseVector(input.Count);
            var temp2 = new DenseVector(input.Count);
           
            // Initialize
            var startNorm = input.Norm(2);

            // Define the scalars
            double alpha = 0;
            double eta = 0;
            double theta = 0;

            var tau = startNorm;
            var rho = tau * tau;

            // Calculate the initial values for v
            // M temp = yEven
            _preconditioner.Approximate(yeven, temp);
            
            // v = A temp
            matrix.Multiply(temp, v);

            // Set uOdd
            v.CopyTo(ueven);

            // Start the iteration
            var iterationNumber = 0;
            while (ShouldContinue(iterationNumber, result, input, pseudoResiduals))
            {
                // First part of the step, the even bit
                if (IsEven(iterationNumber))
                {
                    // sigma = (v, r)
                    var sigma = v.DotProduct(r);
                    if (sigma.AlmostEqual(0, 1))
                    {
                        // FAIL HERE
                        _iterator.IterationCancelled();
                        break;
                    }

                    // alpha = rho / sigma
                    alpha = rho / sigma;

                    // yOdd = yEven - alpha * v
                    v.Multiply(-alpha, temp1);
                    yeven.Add(temp1, yodd);

                    // Solve M temp = yOdd
                    _preconditioner.Approximate(yodd, temp);

                    // uOdd = A temp
                    matrix.Multiply(temp, uodd);
                }

                // The intermediate step which is equal for both even and
                // odd iteration steps.
                // Select the correct vector
                var uinternal = IsEven(iterationNumber) ? ueven : uodd;
                var yinternal = IsEven(iterationNumber) ? yeven : yodd;

                // pseudoResiduals = pseudoResiduals - alpha * uOdd
                uinternal.Multiply(-alpha, temp1);
                pseudoResiduals.Add(temp1, temp2);
                temp2.CopyTo(pseudoResiduals);

                // d = yOdd + theta * theta * eta / alpha * d
                d.Multiply(theta * theta * eta / alpha, temp);
                yinternal.Add(temp, d);

                // theta = ||pseudoResiduals||_2 / tau
                theta = pseudoResiduals.Norm(2) / tau;
                var c = 1 / Math.Sqrt(1 + (theta * theta));

                // tau = tau * theta * c
                tau *= theta * c;

                // eta = c^2 * alpha
                eta = c * c * alpha;

                // x = x + eta * d
                d.Multiply(eta, temp1);
                x.Add(temp1, temp2);
                temp2.CopyTo(x);

                // Check convergence and see if we can bail
                if (!ShouldContinue(iterationNumber, result, input, pseudoResiduals))
                {
                    // Calculate the real values
                    _preconditioner.Approximate(x, result);

                    // Calculate the true residual. Use the temp vector for that
                    // so that we don't pollute the pseudoResidual vector for no
                    // good reason.
                    CalculateTrueResidual(matrix, temp, result, input);

                    // Now recheck the convergence
                    if (!ShouldContinue(iterationNumber, result, input, temp))
                    {
                        // We're all good now.
                        return;
                    }
                }

                // The odd step
                if (!IsEven(iterationNumber))
                {
                    if (rho.AlmostEqual(0, 1))
                    {
                        // FAIL HERE
                        _iterator.IterationCancelled();
                        break;
                    }

                    var rhoNew = pseudoResiduals.DotProduct(r);
                    var beta = rhoNew / rho;

                    // Update rho for the next loop
                    rho = rhoNew;

                    // yOdd = pseudoResiduals + beta * yOdd
                    yodd.Multiply(beta, temp1);
                    pseudoResiduals.Add(temp1, yeven);

                    // Solve M temp = yOdd
                    _preconditioner.Approximate(yeven, temp);

                    // uOdd = A temp
                    matrix.Multiply(temp, ueven);

                    // v = uEven + beta * (uOdd + beta * v)
                    v.Multiply(beta, temp1);
                    uodd.Add(temp1, temp);

                    temp.Multiply(beta, temp1);
                    ueven.Add(temp1, v);
                }

                // Calculate the real values
                _preconditioner.Approximate(x, result);

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
        /// Is <paramref name="number"/> even?
        /// </summary>
        /// <param name="number">Number to check</param>
        /// <returns><c>true</c> if <paramref name="number"/> even, otherwise <c>false</c></returns>
        private static bool IsEven(int number)
        {
            return number % 2 == 0;
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

            var result = (Matrix)matrix.CreateMatrix(input.RowCount, input.ColumnCount);
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
                var solution = Solve(matrix, (Vector)input.Column(column));
                foreach (var element in solution.GetIndexedEnumerator())
                {
                    result.At(element.Item1, column, element.Item2);
                }
            }
        }
    }
}
