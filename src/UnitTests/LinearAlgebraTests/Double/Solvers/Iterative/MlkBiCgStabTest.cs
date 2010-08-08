namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.Iterative
{
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Solvers;
    using LinearAlgebra.Double.Solvers.Iterative;
    using LinearAlgebra.Double.Solvers.Status;
    using LinearAlgebra.Double.Solvers.StopCriterium;
    using MbUnit.Framework;

    [TestFixture]
    public class MlkBiCgStabTest
    {
        private const double ConvergenceBoundary = 1e-10;
        private const int MaximumIterations = 1000;

        [Test]
        [ExpectedArgumentException]
        public void SolveWideMatrix()
        {
            var matrix = new SparseMatrix(2, 3);
            Vector input = new DenseVector(2);

            var solver = new MlkBiCgStab();
            solver.Solve(matrix, input);
        }

        [Test]
        [ExpectedArgumentException]
        public void SolveLongMatrix()
        {
            var matrix = new SparseMatrix(3, 2);
            Vector input = new DenseVector(3);

            var solver = new MlkBiCgStab();
            solver.Solve(matrix, input);
        }

        [Test]
        [MultipleAsserts]
        public void SolveUnitMatrixAndBackMultiply()
        {
            // Create the identity matrix
            Matrix matrix = new SparseMatrix(100);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                matrix[i, i] = 1.0;
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

            var solver = new MlkBiCgStab(monitor);

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
            Matrix matrix = new SparseMatrix(100);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                matrix[i, i] = 1.0;
            }

            // Scale it with a funny number
            matrix.Multiply(System.Math.PI);

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
            var solver = new MlkBiCgStab(monitor);

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
            Vector y = new DenseVector(matrix.RowCount, 1);

            // Create an iteration monitor which will keep track of iterative convergence
            var monitor = new Iterator(new IIterationStopCriterium[]
                                       {
                                           new IterationCountStopCriterium(MaximumIterations),
                                           new ResidualStopCriterium(ConvergenceBoundary),
                                           new DivergenceStopCriterium(),
                                           new FailureStopCriterium()
                                       });
            var solver = new MlkBiCgStab(monitor);

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
    }
}
