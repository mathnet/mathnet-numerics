namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.StopCriterium
{
    using Numerics;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.Solvers.StopCriterium;
    using LinearAlgebra.Generic.Solvers.Status;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class ResidualStopCriteriumTest
    {
        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void CreateWithNegativeMaximum()
        {
            new ResidualStopCriterium(-0.1f);
            Assert.Fail();
        }   

        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void CreateWithIllegalMinimumIterations()
        {
            new ResidualStopCriterium(-1);
            Assert.Fail();
        }

        [Test]
        [MultipleAsserts]
        public void Create()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.AreEqual(1e-6f, criterium.Maximum, "Incorrect maximum");
            Assert.AreEqual(50, criterium.MinimumIterationsBelowMaximum, "Incorrect iteration count");
        }

        [Test]
        [MultipleAsserts]
        public void ResetMaximum()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.ResetMaximumResidualToDefault();
            Assert.AreEqual(ResidualStopCriterium.DefaultMaximumResidual, criterium.Maximum, "Incorrect maximum");
        }

        [Test]
        [MultipleAsserts]
        public void ResetMinimumIterationsBelowMaximum()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.ResetMinimumIterationsBelowMaximumToDefault();
            Assert.AreEqual(ResidualStopCriterium.DefaultMinimumIterationsBelowMaximum, criterium.MinimumIterationsBelowMaximum, "Incorrect iteration count");
        }

        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void DetermineStatusWithIllegalIterationNumber()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(-1, 
                                      new DenseVector(3, 4), 
                                      new DenseVector(3, 5),
                                      new DenseVector(3, 6));
            Assert.Fail();
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullSolutionVector()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1, 
                                      null, 
                                      new DenseVector(3, 5),
                                      new DenseVector(3, 6));
            Assert.Fail();
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullSourceVector()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1,
                                      new DenseVector(3, 4),
                                      null,
                                      new DenseVector(3, 6));
            Assert.Fail();
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullResidualVector()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1, 
                                      new DenseVector(3, 4),
                                      new DenseVector(3, 5),
                                      null);
        }

        [Test]
        [ExpectedArgumentException]
        public void DetermineStatusWithNonMatchingSolutionVector()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1,
                                      new DenseVector(4, 4),
                                      new DenseVector(3, 4),
                                      new DenseVector(3, 4));
        }

        [Test]
        [ExpectedArgumentException]
        public void DetermineStatusWithNonMatchingSourceVector()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1,
                                      new DenseVector(3, 4),
                                      new DenseVector(4, 4),
                                      new DenseVector(3, 4));
        }

        [Test]
        [ExpectedArgumentException]
        public void DetermineStatusWithNonMatchingResidualVector()
        {
            var criterium = new ResidualStopCriterium(1e-6f, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1, 
                                      new DenseVector(3, 4),
                                      new DenseVector(3, 4),
                                      new DenseVector(4, 4));
        }
        
        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithSourceNaN()
        {
            var criterium = new ResidualStopCriterium(1e-3f, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] { new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1) });
            var source = new DenseVector(new[] { new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(float.NaN, 1) });
            var residual = new DenseVector(new[] { new Complex32(1000.0f, 1), new Complex32(1000.0f, 1), new Complex32(2001.0f, 1) });

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationDiverged), criterium.Status, "Should be diverged");
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithResidualNaN()
        {
            var criterium = new ResidualStopCriterium(1e-3f, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] { new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1) });
            var source = new DenseVector(new[] { new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1) });
            var residual = new DenseVector(new[] { new Complex32(1000.0f, 1), new Complex32(float.NaN, 1), new Complex32(2001.0f, 1) });

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationDiverged), criterium.Status, "Should be diverged");
        }

        // Bugfix: The unit tests for the BiCgStab solver run with a super simple matrix equation
        //         which converges at the first iteration. The default settings for the
        //         residual stop criterium should be able to handle this.
        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithConvergenceAtFirstIteration()
        {
            var criterium = new ResidualStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] { Complex32.One, Complex32.One, Complex32.One });
            var source = new DenseVector(new[] { Complex32.One, Complex32.One, Complex32.One });
            var residual = new DenseVector(new[] { Complex32.Zero, Complex32.Zero, Complex32.Zero });

            criterium.DetermineStatus(0, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationConverged), criterium.Status, "Should be done");
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatus()
        {
            var criterium = new ResidualStopCriterium(1e-3f, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            // Note that the solution vector isn't actually being used so ...
            var solution = new DenseVector(new[] { new Complex32(float.NaN, float.NaN), new Complex32(float.NaN, float.NaN), new Complex32(float.NaN, float.NaN) });
            
            // Set the source values
            var source = new DenseVector(new[] { new Complex32(1.000f, 1),  new Complex32(1.000f, 1),  new Complex32(2.001f, 1) });
            
            // Set the residual values
            var residual = new DenseVector(new[] {  new Complex32(0.001f, 0),  new Complex32(0.001f, 0),  new Complex32(0.002f, 0) });

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Should still be running");

            criterium.DetermineStatus(16, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationConverged), criterium.Status, "Should be done");
        }

        [Test]
        [MultipleAsserts]
        public void ResetCalculationState()
        {
            var criterium = new ResidualStopCriterium(1e-3f, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {  new Complex32(0.001f, 1),  new Complex32(0.001f, 1),  new Complex32(0.002f, 1) });
            var source = new DenseVector(new[] { new Complex32(0.001f, 1), new Complex32(0.001f, 1), new Complex32(0.002f, 1) });
            var residual = new DenseVector(new[] { new Complex32(1.000f, 0), new Complex32(1.000f, 0), new Complex32(2.001f, 0) });

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Should be running");

            criterium.ResetToPrecalculationState();
            Assert.IsInstanceOfType(typeof(CalculationIndetermined), criterium.Status, "Should not have started");
        }

        [Test]
        [MultipleAsserts]
        public void Clone()
        {
            var criterium = new ResidualStopCriterium(1e-3f, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var clone = criterium.Clone();
            Assert.IsInstanceOfType(typeof(ResidualStopCriterium), clone, "Wrong criterium type");

            var clonedCriterium = clone as ResidualStopCriterium;
            Assert.IsNotNull(clonedCriterium);
            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(criterium.Maximum, clonedCriterium.Maximum, "Clone failed");
            Assert.AreEqual(criterium.MinimumIterationsBelowMaximum, clonedCriterium.MinimumIterationsBelowMaximum, "Clone failed");
            // ReSharper restore PossibleNullReferenceException
        }
    }
}
