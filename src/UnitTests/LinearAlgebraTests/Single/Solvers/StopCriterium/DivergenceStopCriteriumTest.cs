namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Solvers.StopCriterium
{
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.Solvers.StopCriterium;
    using LinearAlgebra.Generic.Solvers.Status;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class DivergenceStopCriteriumTest
    {
        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void CreateWithNegativeMaximumIncrease()
        {
            new DivergenceStopCriterium(-0.1);
        }

        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void CreateWithIllegalMinimumIterations()
        {
            new DivergenceStopCriterium(2);
        }

        [Test]
        [MultipleAsserts]
        public void Create()
        {
            var criterium = new DivergenceStopCriterium(0.1, 3);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.AreEqual(0.1, criterium.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(3, criterium.MinimumNumberOfIterations, "Incorrect iteration count");
        }

        [Test]
        [MultipleAsserts]
        public void ResetMaximumIncrease()
        {
            var criterium = new DivergenceStopCriterium(0.5, 3);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.AreEqual(0.5, criterium.MaximumRelativeIncrease, "Incorrect maximum");

            criterium.ResetMaximumRelativeIncreaseToDefault();
            Assert.AreEqual(DivergenceStopCriterium.DefaultMaximumRelativeIncrease, criterium.MaximumRelativeIncrease, "Incorrect value");
        }

        [Test]
        [MultipleAsserts]
        public void ResetMinimumIterationsBelowMaximum()
        {
            var criterium = new DivergenceStopCriterium(0.5, 15);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.AreEqual(15, criterium.MinimumNumberOfIterations, "Incorrect iteration count");

            criterium.ResetNumberOfIterationsToDefault();
            Assert.AreEqual(DivergenceStopCriterium.DefaultMinimumNumberOfIterations, criterium.MinimumNumberOfIterations, "Incorrect value");
        }

        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void DetermineStatusWithIllegalIterationNumber()
        {
            var criterium = new DivergenceStopCriterium(0.5, 15);
            criterium.DetermineStatus(-1,
                                      new DenseVector(3, 4),
                                      new DenseVector(3, 5),
                                      new DenseVector(3, 6));
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullResidualVector()
        {
            var criterium = new DivergenceStopCriterium(0.5, 15);
            criterium.DetermineStatus(1,
                                      new DenseVector(3, 4),
                                      new DenseVector(3, 5),
                                      null);
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithTooFewIterations()
        {
            const float Increase = 0.5f;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. We should not diverge because we'll have to few iterations
            for (var i = 0; i < Iterations - 1; i++)
            {
                criterium.DetermineStatus(i,
                                          new DenseVector(new [] { 1.0f }),
                                          new DenseVector(new [] { 1.0f }),
                                          new DenseVector(new [] { (i + 1) * (Increase + 0.1f) }));

                Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Status check fail.");
            }
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithNoDivergence()
        {
            const float Increase = 0.5f;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. We should not diverge because we won't have enough increase
            for (var i = 0; i < Iterations * 2; i++)
            {
                criterium.DetermineStatus(i,
                                          new DenseVector(new [] { 1.0f }),
                                          new DenseVector(new [] { 1.0f }),
                                          new DenseVector(new [] { (i + 1) * (Increase - 0.01f) }));

                Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Status check fail.");
            }
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithDivergenceThroughNaN()
        {
            const float Increase = 0.5f;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. We should not diverge because we'll have to few iterations
            for (var i = 0; i < Iterations - 5; i++)
            {
                criterium.DetermineStatus(i,
                                          new DenseVector(new [] { 1.0f }),
                                          new DenseVector(new [] { 1.0f }),
                                          new DenseVector(new [] { (i + 1) * (Increase - 0.01f) }));

                Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Status check fail.");
            }

            // Now make it fail by throwing in a NaN
            criterium.DetermineStatus(Iterations,
                                      new DenseVector(new [] { 1.0f }),
                                      new DenseVector(new [] { 1.0f }),
                                      new DenseVector(new [] { float.NaN }));

            Assert.IsInstanceOfType(typeof(CalculationDiverged), criterium.Status, "Status check fail.");
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithDivergence()
        {
            const float Increase = 0.5f;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. We should not diverge because we'll have one to few iterations
            float previous = 1;
            for (var i = 0; i < Iterations - 1; i++)
            {
                previous *= (1 + Increase + 0.01f);
                criterium.DetermineStatus(i,
                                          new DenseVector(new[] { 1.0f }),
                                          new DenseVector(new[] { 1.0f }),
                                          new DenseVector(new[] { previous }));

                Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Status check fail.");
            }

            // Add the final residual. Now we should have divergence
            previous *= (1 + Increase + 0.01f);
            criterium.DetermineStatus(Iterations - 1,
                                      new DenseVector(new[] { 1.0f }),
                                      new DenseVector(new[] { 1.0f }),
                                      new DenseVector(new[] { previous }));

            Assert.IsInstanceOfType(typeof(CalculationDiverged), criterium.Status, "Status check fail.");
        }

        [Test]
        [MultipleAsserts]
        public void ResetCalculationState()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. Blow it up instantly
            criterium.DetermineStatus(1,
                                      new DenseVector(new [] { 1.0f }),
                                      new DenseVector(new [] { 1.0f }),
                                      new DenseVector(new [] { float.NaN }));

            Assert.IsInstanceOfType(typeof(CalculationDiverged), criterium.Status, "Status check fail.");

            // Reset the state
            criterium.ResetToPrecalculationState();

            Assert.AreEqual(Increase, criterium.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(Iterations, criterium.MinimumNumberOfIterations, "Incorrect iteration count");
            Assert.IsInstanceOfType(typeof(CalculationIndetermined), criterium.Status, "Status check fail.");
        }

        [Test]
        [MultipleAsserts]
        public void Clone()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var clone = criterium.Clone();
            Assert.IsInstanceOfType(typeof(DivergenceStopCriterium), clone, "Wrong criterium type");

            var clonedCriterium = clone as DivergenceStopCriterium;
            Assert.IsNotNull(clonedCriterium);
            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(criterium.MaximumRelativeIncrease, clonedCriterium.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(criterium.MinimumNumberOfIterations, clonedCriterium.MinimumNumberOfIterations, "Incorrect iteration count");
            // ReSharper restore PossibleNullReferenceException
        }
    }
}
