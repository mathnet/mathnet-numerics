// <copyright file="MlkBiCgStabTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.Iterative
{
    /// <summary>
    /// Tests for Multiple-Lanczos Bi-Conjugate Gradient stabilized iterative matrix solver.
    /// </summary>
    [TestFixture, Category("LASolver")]
    public class MlkBiCgStabTest
    {
        /// <summary>
        /// Convergence boundary.
        /// </summary>
        const double ConvergenceBoundary = 1e-10;

        /// <summary>
        /// Maximum iterations.
        /// </summary>
        const int MaximumIterations = 1000;

        /// <summary>
        /// Solve wide matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SolveWideMatrixThrowsArgumentException()
        {
            var matrix = new SparseMatrix(2, 3);
            var input = new DenseVector(2);

            var solver = new MlkBiCgStab();
            Assert.That(() => matrix.SolveIterative(input, solver), Throws.ArgumentException);
        }

        /// <summary>
        /// Solve long matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SolveLongMatrixThrowsArgumentException()
        {
            var matrix = new SparseMatrix(3, 2);
            var input = new DenseVector(3);

            var solver = new MlkBiCgStab();
            Assert.That(() => matrix.SolveIterative(input, solver), Throws.ArgumentException);
        }

        /// <summary>
        /// Solve unit matrix and back multiply.
        /// </summary>
        [Test]
        public void SolveUnitMatrixAndBackMultiply()
        {
            // Create the identity matrix
            var matrix = Matrix<double>.Build.SparseIdentity(100);

            // Create the y vector
            var y = Vector<double>.Build.Dense(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator<double>(
                new IterationCountStopCriterion<double>(MaximumIterations),
                new ResidualStopCriterion<double>(ConvergenceBoundary),
                new DivergenceStopCriterion<double>(),
                new FailureStopCriterion<double>());

            var solver = new MlkBiCgStab();

            // Solve equation Ax = y
            var x = matrix.SolveIterative(y, solver, monitor);

            // Now compare the results
            Assert.IsNotNull(x, "#02");
            Assert.AreEqual(y.Count, x.Count, "#03");

            // Back multiply the vector
            var z = matrix.Multiply(x);

            // Check that the solution converged
            Assert.IsTrue(monitor.Status == IterationStatus.Converged, "#04");

            // Now compare the vectors
            for (var i = 0; i < y.Count; i++)
            {
                Assert.GreaterOrEqual(ConvergenceBoundary, Math.Abs(y[i] - z[i]), "#05-" + i);
            }
        }

        /// <summary>
        /// Solve scaled unit matrix and back multiply.
        /// </summary>
        [Test]
        public void SolveScaledUnitMatrixAndBackMultiply()
        {
            // Create the identity matrix
            var matrix = Matrix<double>.Build.SparseIdentity(100);

            // Scale it with a funny number
            matrix.Multiply(Math.PI, matrix);

            // Create the y vector
            var y = Vector<double>.Build.Dense(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator<double>(
                new IterationCountStopCriterion<double>(MaximumIterations),
                new ResidualStopCriterion<double>(ConvergenceBoundary),
                new DivergenceStopCriterion<double>(),
                new FailureStopCriterion<double>());

            var solver = new MlkBiCgStab();

            // Solve equation Ax = y
            var x = matrix.SolveIterative(y, solver, monitor);

            // Now compare the results
            Assert.IsNotNull(x, "#02");
            Assert.AreEqual(y.Count, x.Count, "#03");

            // Back multiply the vector
            var z = matrix.Multiply(x);

            // Check that the solution converged
            Assert.IsTrue(monitor.Status == IterationStatus.Converged, "#04");

            // Now compare the vectors
            for (var i = 0; i < y.Count; i++)
            {
                Assert.GreaterOrEqual(ConvergenceBoundary, Math.Abs(y[i] - z[i]), "#05-" + i);
            }
        }

        /// <summary>
        /// Solve poisson matrix and back multiply.
        /// </summary>
        [Test]
        public void SolvePoissonMatrixAndBackMultiply()
        {
            // Create the matrix
            var matrix = Matrix<double>.Build.Sparse(100, 100);

            // Assemble the matrix. We assume we're solving the Poisson equation
            // on a rectangular 10 x 10 grid
            const int GridSize = 10;

            // The pattern is:
            // 0 .... 0 -1 0 0 0 0 0 0 0 0 -1 4 -1 0 0 0 0 0 0 0 0 -1 0 0 ... 0
            for (var i = 0; i < matrix.RowCount; i++)
            {
                // Insert the first set of -1's
                if (i > (GridSize - 1))
                {
                    matrix[i, i - GridSize] = -1;
                }

                // Insert the second set of -1's
                if (i > 0)
                {
                    matrix[i, i - 1] = -1;
                }

                // Insert the centerline values
                matrix[i, i] = 4;

                // Insert the first trailing set of -1's
                if (i < matrix.RowCount - 1)
                {
                    matrix[i, i + 1] = -1;
                }

                // Insert the second trailing set of -1's
                if (i < matrix.RowCount - GridSize)
                {
                    matrix[i, i + GridSize] = -1;
                }
            }

            // Create the y vector
            var y = Vector<double>.Build.Dense(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator<double>(
                new IterationCountStopCriterion<double>(MaximumIterations),
                new ResidualStopCriterion<double>(ConvergenceBoundary),
                new DivergenceStopCriterion<double>(),
                new FailureStopCriterion<double>());

            var solver = new MlkBiCgStab();

            // Solve equation Ax = y
            var x = matrix.SolveIterative(y, solver, monitor);

            // Now compare the results
            Assert.IsNotNull(x, "#02");
            Assert.AreEqual(y.Count, x.Count, "#03");

            // Back multiply the vector
            var z = matrix.Multiply(x);

            // Check that the solution converged
            Assert.IsTrue(monitor.Status == IterationStatus.Converged, "#04");

            // Now compare the vectors
            for (var i = 0; i < y.Count; i++)
            {
                Assert.GreaterOrEqual(ConvergenceBoundary, Math.Abs(y[i] - z[i]), "#05-" + i);
            }
        }

        /// <summary>
        /// Can solve for a random vector.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(10)]
        public void CanSolveForRandomVector(int order)
        {
            var matrixA = Matrix<double>.Build.Random(order, order, 1);
            var vectorb = Vector<double>.Build.Random(order, 1);

            var monitor = new Iterator<double>(
                new IterationCountStopCriterion<double>(1000),
                new ResidualStopCriterion<double>(1e-10));

            var solver = new MlkBiCgStab();

            var resultx = matrixA.SolveIterative(vectorb, solver, monitor);
            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var matrixBReconstruct = matrixA*resultx;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                Assert.AreEqual(vectorb[i], matrixBReconstruct[i], 1e-7);
            }
        }

        /// <summary>
        /// Can solve for random matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(10)]
        public void CanSolveForRandomMatrix(int order)
        {
            var matrixA = Matrix<double>.Build.Random(order, order, 1);
            var matrixB = Matrix<double>.Build.Random(order, order, 1);

            var monitor = new Iterator<double>(
                new IterationCountStopCriterion<double>(1000),
                new ResidualStopCriterion<double>(1e-10));

            var solver = new MlkBiCgStab();
            var matrixX = matrixA.SolveIterative(matrixB, solver, monitor);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA*matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1.0e-7);
                }
            }
        }
    }
}
