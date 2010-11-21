namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.Iterative
{
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Solvers;
    using LinearAlgebra.Double.Solvers.Iterative;
    using LinearAlgebra.Double.Solvers.StopCriterium;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Status;
    using LinearAlgebra.Generic.Solvers.StopCriterium;
    using MbUnit.Framework;

    [TestFixture]
    public class TFQMRTest
    {
        private const double ConvergenceBoundary = 1e-10;
        private const int MaximumIterations = 1000;

        [Test]
        [ExpectedArgumentException]
        public void SolveWideMatrix()
        {
            var matrix = new SparseMatrix(2, 3);
            Vector<double> input = new DenseVector(2);

            var solver = new TFQMR();
            solver.Solve(matrix, input);
        }

        [Test]
        [ExpectedArgumentException]
        public void SolveLongMatrix()
        {
            var matrix = new SparseMatrix(3, 2);
            Vector<double> input = new DenseVector(3);

            var solver = new TFQMR();
            solver.Solve(matrix, input);
        }

        [Test]
        [MultipleAsserts]
        public void SolveUnitMatrixAndBackMultiply()
        {
            // Create the identity matrix
            Matrix<double> matrix = SparseMatrix.Identity(100);

            // Create the y vector
            Vector<double> y = new DenseVector(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator(new IIterationStopCriterium<double>[]
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
            Matrix<double> matrix = SparseMatrix.Identity(100);

            // Scale it with a funny number
            matrix.Multiply(System.Math.PI, matrix);

            // Create the y vector
            Vector<double> y = new DenseVector(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator(new IIterationStopCriterium<double>[]
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
            var matrix = new SparseMatrix(100);
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
            Vector<double> y = new DenseVector(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator(new IIterationStopCriterium<double>[]
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
                Assert.IsTrue(System.Math.Abs(y[i] - z[i]).IsSmaller(ConvergenceBoundary, 1), "#05-" + i);
            }
        }

        [Test]
        [Row(4)]
        [Row(8)]
        [Row(10)]
        [MultipleAsserts]
        public void CanSolveForRandomVector(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var vectorb = MatrixLoader.GenerateRandomDenseVector(order);

            var monitor = new Iterator(new IIterationStopCriterium<double>[]
                                       {
                                           new IterationCountStopCriterium(1000),
                                           new ResidualStopCriterium(1e-10),
                                       });
            var solver = new TFQMR(monitor);

            var resultx = solver.Solve(matrixA, vectorb);
            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var bReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                Assert.AreApproximatelyEqual(vectorb[i], bReconstruct[i], 1e-7);
            }
        }

        [Test]
        [Row(4)]
        [Row(8)]
        [Row(10)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(order, order);

            var monitor = new Iterator(new IIterationStopCriterium<double>[]
                                       {
                                           new IterationCountStopCriterium(1000),
                                           new ResidualStopCriterium(1e-10)
                                       });
            var solver = new TFQMR(monitor);
            var matrixX = solver.Solve(matrixA, matrixB);

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
                    Assert.AreApproximatelyEqual(matrixB[i, j], matrixBReconstruct[i, j], 1.0e-7);
                }
            }
        }
    }
}
