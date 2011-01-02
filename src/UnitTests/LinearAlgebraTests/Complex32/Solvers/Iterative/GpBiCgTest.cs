namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.Iterative
{
    using System;
    using Numerics;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.Solvers;
    using LinearAlgebra.Complex32.Solvers.Iterative;
    using LinearAlgebra.Complex32.Solvers.StopCriterium;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Status;
    using LinearAlgebra.Generic.Solvers.StopCriterium;
    using MbUnit.Framework;

    [TestFixture]
    public class GpBiCgTest
    {
        private const float ConvergenceBoundary = 1e-5f;
        private const int MaximumIterations = 1000;

        [Test]
        [ExpectedArgumentException]
        public void SolveWideMatrix()
        {
            var matrix = new SparseMatrix(2, 3);
            Vector input = new DenseVector(2);

            var solver = new GpBiCg();
            solver.Solve(matrix, input);
        }

        [Test]
        [ExpectedArgumentException]
        public void SolveLongMatrix()
        {
            var matrix = new SparseMatrix(3, 2);
            Vector input = new DenseVector(3);

            var solver = new GpBiCg();
            solver.Solve(matrix, input);
        }

        [Test]
        [MultipleAsserts]
        public void SolveUnitMatrixAndBackMultiply()
        {
            // Create the identity matrix
            Matrix matrix = SparseMatrix.Identity(100);

            // Create the y vector
            Vector y = new DenseVector(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator(new IIterationStopCriterium[]
                                       {
                                           new IterationCountStopCriterium(MaximumIterations),
                                           new ResidualStopCriterium(ConvergenceBoundary),
                                           new DivergenceStopCriterium(),
                                           new FailureStopCriterium()
                                       });
            var solver = new GpBiCg(monitor);

            // Solve equation Ax = y
            var x = solver.Solve(matrix, y);

            // Now compare the results
            Assert.IsNotNull(x, "#02");
            Assert.AreEqual(y.Count, x.Count, "#03");

            // Back multiply the vector
            var z = matrix.Multiply(x);

            // Check that the solution converged
            Assert.IsTrue(monitor.Status is CalculationConverged, "#04");

            // Now compare the vectors
            for (var i = 0; i < y.Count; i++)
            {
                Assert.IsTrue((y[i] - z[i]).Magnitude.IsSmaller(ConvergenceBoundary, 1), "#05-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void SolveScaledUnitMatrixAndBackMultiply()
        {
            // Create the identity matrix
            Matrix matrix = SparseMatrix.Identity(100);

            // Scale it with a funny number
            matrix.Multiply((float)Math.PI, matrix);

            // Create the y vector
            Vector y = new DenseVector(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator(new IIterationStopCriterium[]
                                       {
                                           new IterationCountStopCriterium(MaximumIterations),
                                           new ResidualStopCriterium(ConvergenceBoundary),
                                           new DivergenceStopCriterium(),
                                           new FailureStopCriterium()
                                       });
            
            var solver = new GpBiCg(monitor);

            // Solve equation Ax = y
            var x = solver.Solve(matrix, y);

            // Now compare the results
            Assert.IsNotNull(x, "#02");
            Assert.AreEqual(y.Count, x.Count, "#03");

            // Back multiply the vector
            var z = matrix.Multiply(x);

            // Check that the solution converged
            Assert.IsTrue(monitor.Status is CalculationConverged, "#04");

            // Now compare the vectors
            for (var i = 0; i < y.Count; i++)
            {
                Assert.IsTrue((y[i] - z[i]).Magnitude.IsSmaller(ConvergenceBoundary, 1), "#05-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void SolvePoissonMatrixAndBackMultiply()
        {
            // Create the matrix
            var matrix = new SparseMatrix(25);

            // Assemble the matrix. We assume we're solving the Poisson equation
            // on a rectangular 5 x 5 grid
            const int GridSize = 5;

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
            Vector y = new DenseVector(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator(new IIterationStopCriterium[]
                                       {
                                           new IterationCountStopCriterium(MaximumIterations),
                                           new ResidualStopCriterium(ConvergenceBoundary),
                                           new DivergenceStopCriterium(),
                                           new FailureStopCriterium()
                                       });
            
            var solver = new GpBiCg(monitor);

            // Solve equation Ax = y
            var x = solver.Solve(matrix, y);

            // Now compare the results
            Assert.IsNotNull(x, "#02");
            Assert.AreEqual(y.Count, x.Count, "#03");

            // Back multiply the vector
            var z = matrix.Multiply(x);

            // Check that the solution converged
            Assert.IsTrue(monitor.Status is CalculationConverged, "#04");

            // Now compare the vectors
            for (var i = 0; i < y.Count; i++)
            {
                Assert.IsTrue((y[i] - z[i]).Magnitude.IsSmaller(ConvergenceBoundary, 1), "#05-" + i);
            }
        }

        [Test]
        [Row(5)]
        [MultipleAsserts]
        public void CanSolveForRandomVector(int order)
        {
            for (var iteration = 5; iteration > 3; iteration--)
            {
                var matrixA = (Matrix)MatrixLoader.GenerateRandomDenseMatrix(order, order);
                var vectorb = (Vector)MatrixLoader.GenerateRandomDenseVector(order);

                var monitor = new Iterator(new IIterationStopCriterium[]
                                           {
                                               new IterationCountStopCriterium(1000),
                                               new ResidualStopCriterium((float)Math.Pow(1.0/10.0, iteration)),
                                           });
                var solver = new GpBiCg(monitor);

                var resultx = solver.Solve(matrixA, vectorb);

                if (!(monitor.Status is CalculationConverged))
                {
                    // Solution was not found, try again downgrading convergence boundary
                    continue;
                }

                Assert.AreEqual(matrixA.ColumnCount, resultx.Count);
                var bReconstruct = matrixA * resultx;

                // Check the reconstruction.
                for (var i = 0; i < order; i++)
                {
                    Assert.AreApproximatelyEqual(vectorb[i].Real, bReconstruct[i].Real, (float)Math.Pow(1.0 / 10.0, iteration - 3));
                    Assert.AreApproximatelyEqual(vectorb[i].Imaginary, bReconstruct[i].Imaginary, (float)Math.Pow(1.0 / 10.0, iteration - 3));
                }
                return;
            }

            Assert.Fail("Solution was not found in 3 tries");
        }

        [Test]
        [Row(5)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrix(int order)
        {
            for (var iteration = 5; iteration > 3; iteration--)
            {
                var matrixA = (Matrix)MatrixLoader.GenerateRandomDenseMatrix(order, order);
                var matrixB = (Matrix)MatrixLoader.GenerateRandomDenseMatrix(order, order);

                var monitor = new Iterator(new IIterationStopCriterium[]
                                           {
                                               new IterationCountStopCriterium(1000),
                                               new ResidualStopCriterium((float)Math.Pow(1.0 / 10.0, iteration))
                                           });
                var solver = new GpBiCg(monitor);
                var matrixX = solver.Solve(matrixA, matrixB);

                if (!(monitor.Status is CalculationConverged))
                {
                    // Solution was not found, try again downgrading convergence boundary
                    continue;
                }

                // The solution X row dimension is equal to the column dimension of A
                Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);
                // The solution X has the same number of columns as B
                Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

                var matrixBReconstruct = matrixA * matrixX;

                // Check the reconstruction.
                for (var i = 0; i < matrixB.RowCount; i++)
                {
                    for (var j = 0; j < matrixB.ColumnCount; j++)
                    {
                        Assert.AreApproximatelyEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, (float)Math.Pow(1.0 / 10.0, iteration - 3));
                        Assert.AreApproximatelyEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, (float)Math.Pow(1.0 / 10.0, iteration - 3));
                    }
                }
                return;
            }

            Assert.Fail("Solution was not found in 3 tries");
        }
    }
}