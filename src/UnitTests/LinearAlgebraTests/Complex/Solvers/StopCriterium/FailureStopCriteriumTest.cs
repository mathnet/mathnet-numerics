namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Solvers.StopCriterium
{
    using System.Numerics;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Complex.Solvers.StopCriterium;
    using LinearAlgebra.Generic.Solvers.Status;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class FailureStopCriteriumTest
    {
        [Test]
        [MultipleAsserts] 
        public void Create()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "Should have a criterium now");
        }
        
        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void DetermineStatusWithIllegalIterationNumber()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(-1, new DenseVector(3, 4), new DenseVector(3, 5), new DenseVector(3, 6));
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullSolutionVector()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1, null, new DenseVector(3, 6), new DenseVector(4, 4));
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullResidualVector()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1, new DenseVector(3, 4), new DenseVector(3, 6), null);
        }

        [Test]
        [ExpectedArgumentException]
        public void DetermineStatusWithNonMatchingVectors()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.DetermineStatus(1, new DenseVector(3, 4), new DenseVector(3, 6), new DenseVector(4, 4));
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithResidualNaN()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] { new Complex(1.0, 0), new Complex(1.0, 0), new Complex(2.0, 0) });
            var source = new DenseVector(new[] { new Complex(1001.0, 0), Complex.Zero, new Complex(2003.0, 0) });
            var residual = new DenseVector(new[] { new Complex(1000, 0), new Complex(double.NaN, 0), new Complex(2001, 0) });

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationFailure), criterium.Status, "Should be failed");
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatusWithSolutionNaN()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] { new Complex(1.0, 0), new Complex(1.0, 0), new Complex(double.NaN, 0) });
            var source = new DenseVector(new[] { new Complex(1001.0, 0), Complex.Zero, new Complex(2003.0, 0) });
            var residual = new DenseVector(new[] { new Complex(1000, 0), new Complex(1000, 0), new Complex(2001, 0) });

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationFailure), criterium.Status, "Should be failed");
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatus()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] { new Complex(3.0, 0), new Complex(2.0, 0), new Complex(1, 0) });
            var source = new DenseVector(new[] { new Complex(1001.0, 0), Complex.Zero, new Complex(2003.0, 0) });
            var residual = new DenseVector(new[] { new Complex(1.0, 0), new Complex(2.0, 0), new Complex(3, 0) });

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Should be running");
        }

        [Test]
        [MultipleAsserts]
        public void ResetCalculationState()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] { new Complex(1.0, 0), new Complex(1.0, 0), new Complex(2, 0) });
            var source = new DenseVector(new[] { new Complex(1001.0, 0), Complex.Zero, new Complex(2003.0, 0) });
            var residual = new DenseVector(new[] { new Complex(1000, 0), new Complex(1000, 0), new Complex(2001, 0) });

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOfType(typeof(CalculationRunning), criterium.Status, "Should be running");

            criterium.ResetToPrecalculationState();
            Assert.IsInstanceOfType(typeof(CalculationIndetermined), criterium.Status, "Should not have started");
        }

        [Test]
        [MultipleAsserts]
        public void Clone()
        {
            var criterium = new FailureStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var clone = criterium.Clone();
            Assert.IsInstanceOfType(typeof(FailureStopCriterium), clone, "Wrong criterium type");
        }
    }
}
