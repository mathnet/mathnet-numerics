// <copyright file="MlkBiCgStab.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Single.Solvers.Iterative
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Distributions;
    using Generic.Solvers.Status;
    using Preconditioners;
    using Properties;

    /// <summary>
    /// A Multiple-Lanczos Bi-Conjugate Gradient stabilized iterative matrix solver.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Multiple-Lanczos Bi-Conjugate Gradient stabilized (ML(k)-BiCGStab) solver is an 'improvement'
    /// of the standard BiCgStab solver. 
    /// </para>
    /// <para>
    /// The algorithm was taken from: <br/>
    /// ML(k)BiCGSTAB: A BiCGSTAB variant based on multiple Lanczos starting vectors
    /// <br/>
    /// Man-chung Yeung and Tony F. Chan
    /// <br/>
    /// SIAM Journal of Scientific Computing
    /// <br/>
    /// Volume 21, Number 4, pp. 1263 - 1290
    /// </para>
    /// <para>
    /// The example code below provides an indication of the possible use of the
    /// solver.
    /// </para>
    /// </remarks>
    public sealed class MlkBiCgStab : IIterativeSolver
    {
        /// <summary>
        /// The default number of starting vectors.
        /// </summary>
        private const int DefaultNumberOfStartingVectors = 50;
        
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
        /// The collection of starting vectors which are used as the basis for the Krylov sub-space.
        /// </summary>
        private IList<Vector> _startingVectors;

        /// <summary>
        /// The number of starting vectors used by the algorithm
        /// </summary>
        private int _numberOfStartingVectors = DefaultNumberOfStartingVectors;

        /// <summary>
        /// Indicates if the user has stopped the solver.
        /// </summary>
        private bool _hasBeenStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="MlkBiCgStab"/> class.
        /// </summary>
        /// <remarks>
        /// When using this constructor the solver will use the <see cref="IIterator"/> with
        /// the standard settings and a default preconditioner.
        /// </remarks>
        public MlkBiCgStab() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MlkBiCgStab"/> class.
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
        public MlkBiCgStab(IIterator iterator) : this(null, iterator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MlkBiCgStab"/> class.
        /// </summary>
        /// <remarks>
        /// When using this constructor the solver will use the <see cref="IIterator"/> with
        /// the standard settings.
        /// </remarks>
        /// <param name="preconditioner">The <see cref="IPreConditioner"/> that will be used to precondition the matrix equation.</param>
        public MlkBiCgStab(IPreConditioner preconditioner) : this(preconditioner, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MlkBiCgStab"/> class.
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
        public MlkBiCgStab(IPreConditioner preconditioner, IIterator iterator)
        {
            _iterator = iterator;
            _preconditioner = preconditioner;
        }

        /// <summary>
        /// Gets or sets the number of starting vectors.
        /// </summary>
        /// <remarks>
        /// Must be larger than 1 and smaller than the number of variables in the matrix that 
        /// for which this solver will be used.
        /// </remarks>
        public int NumberOfStartingVectors
        {
            [DebuggerStepThrough]
            get
            {
                return _numberOfStartingVectors;
            }

            [DebuggerStepThrough]
            set
            {
                if (value <= 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _numberOfStartingVectors = value;
            }
        }

        /// <summary>
        /// Resets the number of starting vectors to the default value.
        /// </summary>
        public void ResetNumberOfStartingVectors()
        {
            _numberOfStartingVectors = DefaultNumberOfStartingVectors;
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
        /// Gets or sets a series of orthonormal vectors which will be used as basis for the 
        /// Krylov sub-space.
        /// </summary>
        public IList<Vector> StartingVectors
        {
            [DebuggerStepThrough]
            get
            {
                return _startingVectors;
            }

            [DebuggerStepThrough]
            set
            {
                if ((value == null) || (value.Count == 0))
                {
                    _startingVectors = null;
                }
                else
                {
                    _startingVectors = value;
                }
            }
        }

        /// <summary>
        /// Gets the status of the iteration once the calculation is finished.
        /// </summary>
        public ICalculationStatus IterationResult
        {
            [DebuggerStepThrough]
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

            // Choose an initial guess x_0
            // Take x_0 = 0
            Vector xtemp = new DenseVector(input.Count);

            // Choose k vectors q_1, q_2, ..., q_k
            // Build a new set if:
            // a) the stored set doesn't exist (i.e. == null)
            // b) Is of an incorrect length (i.e. too long)
            // c) The vectors are of an incorrect length (i.e. too long or too short)
            var useOld = false;
            if (_startingVectors != null)
            {
                // We don't accept collections with zero starting vectors so ...
                if (_startingVectors.Count <= NumberOfStartingVectorsToCreate(_numberOfStartingVectors, input.Count))
                {
                    // Only check the first vector for sizing. If that matches we assume the 
                    // other vectors match too. If they don't the process will crash
                    if (_startingVectors[0].Count == input.Count)
                    {
                        useOld = true;
                    }
                }
            }

            _startingVectors = useOld ? _startingVectors : CreateStartingVectors(_numberOfStartingVectors, input.Count);

            // Store the number of starting vectors. Not really necessary but easier to type :)
            var k = _startingVectors.Count;

            // r_0 = b - Ax_0
            // This is basically a SAXPY so it could be made a lot faster
            Vector residuals = new DenseVector(matrix.RowCount);
            CalculateTrueResidual(matrix, residuals, xtemp, input);

            // Define the temporary scalars
            var c = new float[k];

            // Define the temporary vectors
            Vector gtemp = new DenseVector(residuals.Count);

            Vector u = new DenseVector(residuals.Count);
            Vector utemp = new DenseVector(residuals.Count);
            Vector temp = new DenseVector(residuals.Count);
            Vector temp1 = new DenseVector(residuals.Count);
            Vector temp2 = new DenseVector(residuals.Count);

            Vector zd = new DenseVector(residuals.Count);
            Vector zg = new DenseVector(residuals.Count);
            Vector zw = new DenseVector(residuals.Count);

            var d = CreateVectorArray(_startingVectors.Count, residuals.Count);
            
            // g_0 = r_0
            var g = CreateVectorArray(_startingVectors.Count, residuals.Count);
            residuals.CopyTo(g[k - 1]);

            var w = CreateVectorArray(_startingVectors.Count, residuals.Count);

            // FOR (j = 0, 1, 2 ....)
            var iterationNumber = 0;
            while (ShouldContinue(iterationNumber, xtemp, input, residuals))
            {
                // SOLVE M g~_((j-1)k+k) = g_((j-1)k+k)
                _preconditioner.Approximate(g[k - 1], gtemp);

                // w_((j-1)k+k) = A g~_((j-1)k+k)
                matrix.Multiply(gtemp, w[k - 1]);

                // c_((j-1)k+k) = q^T_1 w_((j-1)k+k)
                c[k - 1] = _startingVectors[0].DotProduct(w[k - 1]);
                if (c[k - 1].AlmostEqual(0, 1))
                {
                    throw new Exception("Iterative solver experience a numerical break down");
                }

                // alpha_(jk+1) = q^T_1 r_((j-1)k+k) / c_((j-1)k+k)
                var alpha = _startingVectors[0].DotProduct(residuals) / c[k - 1];

                // u_(jk+1) = r_((j-1)k+k) - alpha_(jk+1) w_((j-1)k+k)
                w[k - 1].Multiply(-alpha, temp);
                residuals.Add(temp, u);

                // SOLVE M u~_(jk+1) = u_(jk+1)
                _preconditioner.Approximate(u, temp1);
                temp1.CopyTo(utemp);

                // rho_(j+1) = -u^t_(jk+1) A u~_(jk+1) / ||A u~_(jk+1)||^2
                matrix.Multiply(temp1, temp);
                var rho = temp.DotProduct(temp);

                // If rho is zero then temp is a zero vector and we're probably
                // about to have zero residuals (i.e. an exact solution).
                // So set rho to 1.0 because in the next step it will turn to zero.
                if (rho.AlmostEqual(0, 1))
                {
                    rho = 1.0f;
                }

                rho = -u.DotProduct(temp) / rho;

                // r_(jk+1) = rho_(j+1) A u~_(jk+1) + u_(jk+1)
                u.CopyTo(residuals);

                // Reuse temp
                temp.Multiply(rho, temp);
                residuals.Add(temp, temp2);
                temp2.CopyTo(residuals);

                // x_(jk+1) = x_((j-1)k_k) - rho_(j+1) u~_(jk+1) + alpha_(jk+1) g~_((j-1)k+k)
                utemp.Multiply(-rho, temp);
                xtemp.Add(temp, temp2);
                temp2.CopyTo(xtemp);

                gtemp.Multiply(alpha, gtemp);
                xtemp.Add(gtemp, temp2);
                temp2.CopyTo(xtemp);

                // Check convergence and stop if we are converged.
                if (!ShouldContinue(iterationNumber, xtemp, input, residuals))
                {
                    // Calculate the true residual
                    CalculateTrueResidual(matrix, residuals, xtemp, input);

                    // Now recheck the convergence
                    if (!ShouldContinue(iterationNumber, xtemp, input, residuals))
                    {
                        // We're all good now.
                        // Exit from the while loop.
                        break;
                    }
                }

                // FOR (i = 1,2, ...., k)
                for (var i = 0; i < k; i++)
                {
                    // z_d = u_(jk+1)
                    u.CopyTo(zd);

                    // z_g = r_(jk+i)
                    residuals.CopyTo(zg);

                    // z_w = 0
                    zw.Clear();

                    // FOR (s = i, ...., k-1) AND j >= 1
                    float beta;
                    if (iterationNumber >= 1)
                    {
                        for (var s = i; s < k - 1; s++)
                        {
                            // beta^(jk+i)_((j-1)k+s) = -q^t_(s+1) z_d / c_((j-1)k+s)
                            beta = -_startingVectors[s + 1].DotProduct(zd) / c[s];

                            // z_d = z_d + beta^(jk+i)_((j-1)k+s) d_((j-1)k+s)
                            d[s].Multiply(beta, temp);
                            zd.Add(temp, temp2);
                            temp2.CopyTo(zd);

                            // z_g = z_g + beta^(jk+i)_((j-1)k+s) g_((j-1)k+s)
                            g[s].Multiply(beta, temp);
                            zg.Add(temp, temp2);
                            temp2.CopyTo(zg);

                            // z_w = z_w + beta^(jk+i)_((j-1)k+s) w_((j-1)k+s)
                            w[s].Multiply(beta, temp);
                            zw.Add(temp, temp2);
                            temp2.CopyTo(zw);
                        }
                    }

                    beta = rho * c[k - 1];
                    if (beta.AlmostEqual(0, 1))
                    {
                        throw new Exception("Iterative solver experience a numerical break down");
                    }

                    // beta^(jk+i)_((j-1)k+k) = -(q^T_1 (r_(jk+1) + rho_(j+1) z_w)) / (rho_(j+1) c_((j-1)k+k))
                    zw.Multiply(rho, temp2);
                    residuals.Add(temp2, temp);
                    beta = -_startingVectors[0].DotProduct(temp) / beta;

                    // z_g = z_g + beta^(jk+i)_((j-1)k+k) g_((j-1)k+k)
                    g[k - 1].Multiply(beta, temp);
                    zg.Add(temp, temp2);
                    temp2.CopyTo(zg);

                    // z_w = rho_(j+1) (z_w + beta^(jk+i)_((j-1)k+k) w_((j-1)k+k))
                    w[k - 1].Multiply(beta, temp);
                    zw.Add(temp, temp2);
                    temp2.CopyTo(zw);
                    zw.Multiply(rho, zw);

                    // z_d = r_(jk+i) + z_w
                    residuals.Add(zw, zd);

                    // FOR (s = 1, ... i - 1)
                    for (var s = 0; s < i - 1; s++)
                    {
                        // beta^(jk+i)_(jk+s) = -q^T_s+1 z_d / c_(jk+s)
                        beta = -_startingVectors[s + 1].DotProduct(zd) / c[s];

                        // z_d = z_d + beta^(jk+i)_(jk+s) * d_(jk+s)
                        d[s].Multiply(beta, temp);
                        zd.Add(temp, temp2);
                        temp2.CopyTo(zd);

                        // z_g = z_g + beta^(jk+i)_(jk+s) * g_(jk+s)
                        g[s].Multiply(beta, temp);
                        zg.Add(temp, temp2);
                        temp2.CopyTo(zg);
                    }

                    // d_(jk+i) = z_d - u_(jk+i)
                    zd.Subtract(u, d[i]);

                    // g_(jk+i) = z_g + z_w
                    zg.Add(zw, g[i]);

                    // IF (i < k - 1)
                    if (i < k - 1)
                    {
                        // c_(jk+1) = q^T_i+1 d_(jk+i)
                        c[i] = _startingVectors[i + 1].DotProduct(d[i]);
                        if (c[i].AlmostEqual(0, 1))
                        {
                            throw new Exception("Iterative solver experience a numerical break down");
                        }

                        // alpha_(jk+i+1) = q^T_(i+1) u_(jk+i) / c_(jk+i)
                        alpha = _startingVectors[i + 1].DotProduct(u) / c[i];

                        // u_(jk+i+1) = u_(jk+i) - alpha_(jk+i+1) d_(jk+i)
                        d[i].Multiply(-alpha, temp);
                        u.Add(temp, temp2);
                        temp2.CopyTo(u);

                        // SOLVE M g~_(jk+i) = g_(jk+i)
                        _preconditioner.Approximate(g[i], gtemp);

                        // x_(jk+i+1) = x_(jk+i) + rho_(j+1) alpha_(jk+i+1) g~_(jk+i)
                        gtemp.Multiply(rho * alpha, temp);
                        xtemp.Add(temp, temp2);
                        temp2.CopyTo(xtemp);

                        // w_(jk+i) = A g~_(jk+i)
                        matrix.Multiply(gtemp, w[i]);

                        // r_(jk+i+1) = r_(jk+i) - rho_(j+1) alpha_(jk+i+1) w_(jk+i)
                        w[i].Multiply(-rho * alpha, temp);
                        residuals.Add(temp, temp2);
                        temp2.CopyTo(residuals);

                        // We can check the residuals here if they're close
                        if (!ShouldContinue(iterationNumber, xtemp, input, residuals))
                        {
                            // Recalculate the residuals and go round again. This is done to ensure that
                            // we have the proper residuals.
                            CalculateTrueResidual(matrix, residuals, xtemp, input);
                        }
                    }
                } // END ITERATION OVER i

                iterationNumber++;
            }

            // copy the temporary result to the real result vector
            xtemp.CopyTo(result);
        }

        /// <summary>
        /// Gets the number of starting vectors to create
        /// </summary>
        /// <param name="maximumNumberOfStartingVectors">Maximum number</param>
        /// <param name="numberOfVariables">Number of variables</param>
        /// <returns>Number of starting vectors to create</returns>
        private static int NumberOfStartingVectorsToCreate(int maximumNumberOfStartingVectors, int numberOfVariables)
        {
            // Create no more starting vectors than the size of the problem - 1
            return Math.Min(maximumNumberOfStartingVectors, (numberOfVariables - 1));
        }

        /// <summary>
        /// Returns an array of starting vectors.
        /// </summary>
        /// <param name="maximumNumberOfStartingVectors">The maximum number of starting vectors that should be created.</param>
        /// <param name="numberOfVariables">The number of variables.</param>
        /// <returns>
        ///  An array with starting vectors. The array will never be larger than the
        ///  <paramref name="maximumNumberOfStartingVectors"/> but it may be smaller if
        ///  the <paramref name="numberOfVariables"/> is smaller than 
        ///  the <paramref name="maximumNumberOfStartingVectors"/>.
        /// </returns>
        private static IList<Vector> CreateStartingVectors(int maximumNumberOfStartingVectors, int numberOfVariables)
        {
            // Create no more starting vectors than the size of the problem - 1
            // Get random values and then orthogonalize them with
            // modified Gramm - Schmidt
            var count = NumberOfStartingVectorsToCreate(maximumNumberOfStartingVectors, numberOfVariables);

            // Get a random set of samples based on the standard normal distribution with
            // mean = 0 and sd = 1
            var distribution = new Normal();

            Matrix matrix = new DenseMatrix(numberOfVariables, count);
            for (var i = 0; i < matrix.ColumnCount; i++)
            {
                var samples = new float[matrix.RowCount];
                for (var j = 0; j < matrix.RowCount; j++)
                {
                    samples[j] = (float)distribution.Sample();
                }
                
                // Set the column
                matrix.SetColumn(i, samples);
            }

            // Compute the orthogonalization.
            var gs = matrix.GramSchmidt();
            var orthogonalMatrix = gs.Q;

            // Now transfer this to vectors
            var result = new List<Vector>();
            for (var i = 0; i < orthogonalMatrix.ColumnCount; i++)
            {
                result.Add((Vector)orthogonalMatrix.Column(i));
                
                // Normalize the result vector
                result[i].Multiply(1 / result[i].Norm(2), result[i]);
            }

            return result;
        }

        /// <summary>
        /// Create random vectors array
        /// </summary>
        /// <param name="arraySize">Number of vectors</param>
        /// <param name="vectorSize">Size of each vector</param>
        /// <returns>Array of random vectors</returns>
        private static Vector[] CreateVectorArray(int arraySize, int vectorSize)
        {
            var result = new Vector[arraySize];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = new DenseVector(vectorSize);
            }

            return result;
        }

        /// <summary>
        /// Calculates the <c>true</c> residual of the matrix equation Ax = b according to: residual = b - Ax
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix"/>A.</param>
        /// <param name="residual">Residual <see cref="Vector"/> data.</param>
        /// <param name="x">x <see cref="Vector"/> data.</param>
        /// <param name="b">b <see cref="Vector"/> data.</param>
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
