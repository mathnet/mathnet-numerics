namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Solvers.Iterative
{
    using System;
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.Solvers;
    using LinearAlgebra.Single.Solvers.Iterative;
    using LinearAlgebra.Single.Solvers.StopCriterium;
    using LinearAlgebra.Generic.Solvers.Status;
    using MbUnit.Framework;

    [TestFixture]
    public class TFQMRTest
    {
        private const float ConvergenceBoundary = 1e-5f;
        private const int MaximumIterations = 1000;

        [Test]
        [ExpectedArgumentException]
        public void SolveWideMatrix()
        {
            var matrix = new SparseMatrix(2, 3);
            Vector input = new DenseVector(2);

            var solver = new TFQMR();
            solver.Solve(matrix, input);
        }

        [Test]
        [ExpectedArgumentException]
        public void SolveLongMatrix()
        {
            var matrix = new SparseMatrix(3, 2);
            Vector input = new DenseVector(3);

            var solver = new TFQMR();
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
            
            var solver = new TFQMR(monitor);

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
                Assert.IsTrue((y[i] - z[i]).IsSmaller(ConvergenceBoundary, 1), "#05-" + i);
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
            var solver = new TFQMR(monitor);

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
                Assert.IsTrue((y[i] - z[i]).IsSmaller(ConvergenceBoundary, 1), "#05-" + i);
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
            var solver = new TFQMR(monitor);

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
                Assert.IsTrue(Math.Abs(y[i] - z[i]).IsSmaller(ConvergenceBoundary, 1), "#05-" + i);
            }
        }

        [Test]
        [Row(5)]
        [MultipleAsserts]
        public void CanSolveForRandomVector(int order)
        {
            // Due to datatype "float" it can happen that solution will not converge for specific random matrix
            // That's why we will do 3 tries and downgrade stop criterium each time
            for (var iteration = 6; iteration > 3; iteration--)
            {
                var matrixA = (Matrix)MatrixLoader.GenerateRandomDenseMatrix(order, order);
                var vectorb = (Vector)MatrixLoader.GenerateRandomDenseVector(order);

                var monitor = new Iterator(new IIterationStopCriterium[]
                                           {
                                               new IterationCountStopCriterium(MaximumIterations),
                                               new ResidualStopCriterium((float)Math.Pow(1.0/10.0, iteration)),
                                           });
                var solver = new TFQMR(monitor);
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
                    Assert.AreApproximatelyEqual(vectorb[i], bReconstruct[i], (float)Math.Pow(1.0 / 10.0, iteration - 3));
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
            // Due to datatype "float" it can happen that solution will not converge for specific random matrix
            // That's why we will do 4 tries and downgrade stop criterium each time
            for (var iteration = 6; iteration > 3; iteration--)
            {
                var matrixA = (Matrix)MatrixLoader.GenerateRandomDenseMatrix(order, order);
                var matrixB = (Matrix)MatrixLoader.GenerateRandomDenseMatrix(order, order);

                var monitor = new Iterator(new IIterationStopCriterium[]
                                           {
                                               new IterationCountStopCriterium(MaximumIterations),
                                               new ResidualStopCriterium((float)Math.Pow(1.0/10.0, iteration))
                                           });
                var solver = new TFQMR(monitor);
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
                        Assert.AreApproximatelyEqual(matrixB[i, j], matrixBReconstruct[i, j], (float)Math.Pow(1.0 / 10.0, iteration - 3));
                    }
                }

                return;
            }

            Assert.Fail("Solution was not found in 3 tries");
        }
    }
}
