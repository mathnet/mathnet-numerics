namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.StopCriterium
{
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.Solvers.StopCriterium;
    using LinearAlgebra.Generic.Solvers.Status;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class IterationCountStopCriteriumTest
    {
        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void CreateWithIllegalMinimumIterations()
        {
            new IterationCountStopCriterium(-1);
        }

        [Test]
        [MultipleAsserts]
        public void Create()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");
        }

        [Test]
        [MultipleAsserts]
        public void ResetMaximumIterations()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");
            Assert.AreEqual(10, criterium.MaximumNumberOfIterations, "Incorrect maximum number of iterations");

            criterium.ResetMaximumNumberOfIterationsToDefault();
            Assert.AreNotEqual(10, criterium.MaximumNumberOfIterations, "Should have reset");
            Assert.AreEqual(IterationCountStopCriterium.DefaultMaximumNumberOfIterations, criterium.MaximumNumberOfIterations, "Reset to the wrong value");
        }

        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void DetermineStatusWithIllegalIterationNumber()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");

            criterium.DetermineStatus(-1, new DenseVector(3, 1), new DenseVector(3, 2), new DenseVector(3,3));
            Assert.Fail();
        }
        
        [Test]
        [MultipleAsserts]
        public void DetermineStatus()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");

            criterium.DetermineStatus(5, new DenseVector(3, 1), new DenseVector(3, 2), new DenseVector(3, 3));
            Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Should be running");

            criterium.DetermineStatus(10, new DenseVector(3, 1), new DenseVector(3, 2), new DenseVector(3, 3));
            Assert.IsInstanceOfType(typeof(CalculationStoppedWithoutConvergence), criterium.Status, "Should be finished");
        }

        [Test]
        [MultipleAsserts]
        public void ResetCalculationState()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");

            criterium.DetermineStatus(5, new DenseVector(3, 1), new DenseVector(3, 2), new DenseVector(3, 3));
            Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Should be running");

            criterium.ResetToPrecalculationState();
            Assert.IsInstanceOfType(typeof(CalculationIndetermined), criterium.Status, "Should not have started");
        }

        [Test]
        [MultipleAsserts]
        public void Clone()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");
            Assert.AreEqual(10, criterium.MaximumNumberOfIterations, "Incorrect maximum");

            var clone = criterium.Clone();
            Assert.IsInstanceOfType(typeof(IterationCountStopCriterium), clone, "Wrong criterium type");

            var clonedCriterium = clone as IterationCountStopCriterium;
            Assert.IsNotNull(clonedCriterium);
            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(criterium.MaximumNumberOfIterations, clonedCriterium.MaximumNumberOfIterations, "Clone failed");
            // ReSharper restore PossibleNullReferenceException
        }
    }
}
