// <copyright file="MlkBiCgStab.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace MathNet.Numerics.LinearAlgebra.Complex32.Solvers
{
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
    /// Man-Chung Yeung and Tony F. Chan
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
    public sealed class MlkBiCgStab : IIterativeSolver<Numerics.Complex32>
    {
        /// <summary>
        /// The default number of starting vectors.
        /// </summary>
        const int DefaultNumberOfStartingVectors = 50;

        /// <summary>
        /// The collection of starting vectors which are used as the basis for the Krylov sub-space.
        /// </summary>
        IList<Vector<Numerics.Complex32>> _startingVectors;

        /// <summary>
        /// The number of starting vectors used by the algorithm
        /// </summary>
        int _numberOfStartingVectors = DefaultNumberOfStartingVectors;

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
            get => _numberOfStartingVectors;

            [DebuggerStepThrough]
            set
            {
                if (value <= 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
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
        /// Gets or sets a series of orthonormal vectors which will be used as basis for the
        /// Krylov sub-space.
        /// </summary>
        public IList<Vector<Numerics.Complex32>> StartingVectors
        {
            [DebuggerStepThrough]
            get => _startingVectors;

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
        /// Gets the number of starting vectors to create
        /// </summary>
        /// <param name="maximumNumberOfStartingVectors">Maximum number</param>
        /// <param name="numberOfVariables">Number of variables</param>
        /// <returns>Number of starting vectors to create</returns>
        static int NumberOfStartingVectorsToCreate(int maximumNumberOfStartingVectors, int numberOfVariables)
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
        static IList<Vector<Numerics.Complex32>> CreateStartingVectors(int maximumNumberOfStartingVectors, int numberOfVariables)
        {
            // Create no more starting vectors than the size of the problem - 1
            // Get random values and then orthogonalize them with
            // modified Gramm - Schmidt
            var count = NumberOfStartingVectorsToCreate(maximumNumberOfStartingVectors, numberOfVariables);

            // Get a random set of samples based on the standard normal distribution with
            // mean = 0 and sd = 1
            var distribution = new Normal();

            var matrix = new DenseMatrix(numberOfVariables, count);
            for (var i = 0; i < matrix.ColumnCount; i++)
            {
                var samples = new Numerics.Complex32[matrix.RowCount];
                var samplesRe = distribution.Samples().Take(matrix.RowCount).ToArray();
                var samplesIm = distribution.Samples().Take(matrix.RowCount).ToArray();
                for (int j = 0; j < matrix.RowCount; j++)
                {
                    samples[j] = new Numerics.Complex32((float)samplesRe[j], (float)samplesIm[j]);
                }

                // Set the column
                matrix.SetColumn(i, samples);
            }

            // Compute the orthogonalization.
            var gs = matrix.GramSchmidt();
            var orthogonalMatrix = gs.Q;

            // Now transfer this to vectors
            var result = new List<Vector<Numerics.Complex32>>(orthogonalMatrix.ColumnCount);
            for (var i = 0; i < orthogonalMatrix.ColumnCount; i++)
            {
                result.Add(orthogonalMatrix.Column(i));

                // Normalize the result vector
                result[i].Multiply(1/(float) result[i].L2Norm(), result[i]);
            }

            return result;
        }

        /// <summary>
        /// Create random vectors array
        /// </summary>
        /// <param name="arraySize">Number of vectors</param>
        /// <param name="vectorSize">Size of each vector</param>
        /// <returns>Array of random vectors</returns>
        static Vector<Numerics.Complex32>[] CreateVectorArray(int arraySize, int vectorSize)
        {
            var result = new Vector<Numerics.Complex32>[arraySize];
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
        static void CalculateTrueResidual(Matrix<Numerics.Complex32> matrix, Vector<Numerics.Complex32> residual, Vector<Numerics.Complex32> x, Vector<Numerics.Complex32> b)
        {
            // -Ax = residual
            matrix.Multiply(x, residual);
            residual.Multiply(-1, residual);

            // residual + b
            residual.Add(b, residual);
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

            // Choose an initial guess x_0
            // Take x_0 = 0
            var xtemp = new DenseVector(input.Count);

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
            var residuals = new DenseVector(matrix.RowCount);
            CalculateTrueResidual(matrix, residuals, xtemp, input);

            // Define the temporary values
            var c = new Numerics.Complex32[k];

            // Define the temporary vectors
            var gtemp = new DenseVector(residuals.Count);

            var u = new DenseVector(residuals.Count);
            var utemp = new DenseVector(residuals.Count);
            var temp = new DenseVector(residuals.Count);
            var temp1 = new DenseVector(residuals.Count);
            var temp2 = new DenseVector(residuals.Count);

            var zd = new DenseVector(residuals.Count);
            var zg = new DenseVector(residuals.Count);
            var zw = new DenseVector(residuals.Count);

            var d = CreateVectorArray(_startingVectors.Count, residuals.Count);

            // g_0 = r_0
            var g = CreateVectorArray(_startingVectors.Count, residuals.Count);
            residuals.CopyTo(g[k - 1]);

            var w = CreateVectorArray(_startingVectors.Count, residuals.Count);

            // FOR (j = 0, 1, 2 ....)
            var iterationNumber = 0;
            while (iterator.DetermineStatus(iterationNumber, xtemp, input, residuals) == IterationStatus.Continue)
            {
                // SOLVE M g~_((j-1)k+k) = g_((j-1)k+k)
                preconditioner.Approximate(g[k - 1], gtemp);

                // w_((j-1)k+k) = A g~_((j-1)k+k)
                matrix.Multiply(gtemp, w[k - 1]);

                // c_((j-1)k+k) = q^T_1 w_((j-1)k+k)
                c[k - 1] = _startingVectors[0].ConjugateDotProduct(w[k - 1]);
                if (c[k - 1].Real.AlmostEqualNumbersBetween(0, 1) && c[k - 1].Imaginary.AlmostEqualNumbersBetween(0, 1))
                {
                    throw new NumericalBreakdownException();
                }

                // alpha_(jk+1) = q^T_1 r_((j-1)k+k) / c_((j-1)k+k)
                var alpha = _startingVectors[0].ConjugateDotProduct(residuals)/c[k - 1];

                // u_(jk+1) = r_((j-1)k+k) - alpha_(jk+1) w_((j-1)k+k)
                w[k - 1].Multiply(-alpha, temp);
                residuals.Add(temp, u);

                // SOLVE M u~_(jk+1) = u_(jk+1)
                preconditioner.Approximate(u, temp1);
                temp1.CopyTo(utemp);

                // rho_(j+1) = -u^t_(jk+1) A u~_(jk+1) / ||A u~_(jk+1)||^2
                matrix.Multiply(temp1, temp);
                var rho = temp.ConjugateDotProduct(temp);

                // If rho is zero then temp is a zero vector and we're probably
                // about to have zero residuals (i.e. an exact solution).
                // So set rho to 1.0 because in the next step it will turn to zero.
                if (rho.Real.AlmostEqualNumbersBetween(0, 1) && rho.Imaginary.AlmostEqualNumbersBetween(0, 1))
                {
                    rho = 1.0f;
                }

                rho = -u.ConjugateDotProduct(temp)/rho;

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
                if (iterator.DetermineStatus(iterationNumber, xtemp, input, residuals) != IterationStatus.Continue)
                {
                    // Calculate the true residual
                    CalculateTrueResidual(matrix, residuals, xtemp, input);

                    // Now recheck the convergence
                    if (iterator.DetermineStatus(iterationNumber, xtemp, input, residuals) != IterationStatus.Continue)
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
                    Numerics.Complex32 beta;
                    if (iterationNumber >= 1)
                    {
                        for (var s = i; s < k - 1; s++)
                        {
                            // beta^(jk+i)_((j-1)k+s) = -q^t_(s+1) z_d / c_((j-1)k+s)
                            beta = -_startingVectors[s + 1].ConjugateDotProduct(zd)/c[s];

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

                    beta = rho*c[k - 1];
                    if (beta.Real.AlmostEqualNumbersBetween(0, 1) && beta.Imaginary.AlmostEqualNumbersBetween(0, 1))
                    {
                        throw new NumericalBreakdownException();
                    }

                    // beta^(jk+i)_((j-1)k+k) = -(q^T_1 (r_(jk+1) + rho_(j+1) z_w)) / (rho_(j+1) c_((j-1)k+k))
                    zw.Multiply(rho, temp2);
                    residuals.Add(temp2, temp);
                    beta = -_startingVectors[0].ConjugateDotProduct(temp)/beta;

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
                        beta = -_startingVectors[s + 1].ConjugateDotProduct(zd)/c[s];

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
                        c[i] = _startingVectors[i + 1].ConjugateDotProduct(d[i]);
                        if (c[i].Real.AlmostEqualNumbersBetween(0, 1) && c[i].Imaginary.AlmostEqualNumbersBetween(0, 1))
                        {
                            throw new NumericalBreakdownException();
                        }

                        // alpha_(jk+i+1) = q^T_(i+1) u_(jk+i) / c_(jk+i)
                        alpha = _startingVectors[i + 1].ConjugateDotProduct(u)/c[i];

                        // u_(jk+i+1) = u_(jk+i) - alpha_(jk+i+1) d_(jk+i)
                        d[i].Multiply(-alpha, temp);
                        u.Add(temp, temp2);
                        temp2.CopyTo(u);

                        // SOLVE M g~_(jk+i) = g_(jk+i)
                        preconditioner.Approximate(g[i], gtemp);

                        // x_(jk+i+1) = x_(jk+i) + rho_(j+1) alpha_(jk+i+1) g~_(jk+i)
                        gtemp.Multiply(rho*alpha, temp);
                        xtemp.Add(temp, temp2);
                        temp2.CopyTo(xtemp);

                        // w_(jk+i) = A g~_(jk+i)
                        matrix.Multiply(gtemp, w[i]);

                        // r_(jk+i+1) = r_(jk+i) - rho_(j+1) alpha_(jk+i+1) w_(jk+i)
                        w[i].Multiply(-rho*alpha, temp);
                        residuals.Add(temp, temp2);
                        temp2.CopyTo(residuals);

                        // We can check the residuals here if they're close
                        if (iterator.DetermineStatus(iterationNumber, xtemp, input, residuals) != IterationStatus.Continue)
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
    }
}
