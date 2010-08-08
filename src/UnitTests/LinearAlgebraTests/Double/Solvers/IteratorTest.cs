namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers
{
    using System;
    using System.Collections.Generic;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Solvers;
    using LinearAlgebra.Double.Solvers.Status;
    using LinearAlgebra.Double.Solvers.StopCriterium;
    using MbUnit.Framework;

    [TestFixture]
    public class IteratorTest
    {
        [Test]
        [MultipleAsserts]
        public void CreateWithNullCollection()
        {
            var iterator = new Iterator(null);
            Assert.IsNotNull(iterator, "Should have an iterator");
            Assert.AreEqual(0, iterator.NumberOfCriteria, "There shouldn't be any criteria");
        }

        [Test]
        [MultipleAsserts]
        public void CreateWithEmptyCollection()
        {
            var iterator = new Iterator(new IIterationStopCriterium[] { });
            Assert.IsNotNull(iterator, "Should have an iterator");
            Assert.AreEqual(0, iterator.NumberOfCriteria, "There shouldn't be any criteria");
        }

        [Test]
        [MultipleAsserts]
        public void CreateWithCollectionWithNulls()
        {
            var iterator = new Iterator(new IIterationStopCriterium[] { null, null });
            Assert.IsNotNull(iterator, "Should have an iterator");
            Assert.AreEqual(0, iterator.NumberOfCriteria, "There shouldn't be any criteria");
        }

        [Test]
        [ExpectedArgumentException]
        public void CreateWithDuplicates()
        {
            new Iterator(new IIterationStopCriterium[]
                         {
                             new FailureStopCriterium(),
                             new FailureStopCriterium()
                         });
        }

        [Test]
        [MultipleAsserts]
        public void CreateWithCollection()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            var iterator = new Iterator(criteria);
            Assert.IsNotNull(iterator, "Should have an iterator");

            // Check that we have all the criteria
            Assert.AreEqual(criteria.Count, iterator.NumberOfCriteria, "Incorrect criterium count");
            var enumerator = iterator.StoredStopCriteria;
            while (enumerator.MoveNext())
            {
                var criterium = enumerator.Current;
                Assert.IsTrue(criteria.Exists( c => ReferenceEquals(c, criterium)), "Criterium missing");
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void AddWithNullStopCriterium()
        {
            var iterator = new Iterator();
            iterator.Add(null);
        }

        [Test]
        [ExpectedArgumentException]
        public void AddWithExistingStopCriterium()
        {
            var iterator = new Iterator();
            iterator.Add(new FailureStopCriterium());
            Assert.AreEqual(1, iterator.NumberOfCriteria, "Incorrect criterium count");

            iterator.Add(new FailureStopCriterium());
        }

        [Test]
        [MultipleAsserts]
        public void Add()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            var iterator = new Iterator();
            Assert.AreEqual(0, iterator.NumberOfCriteria, "Incorrect criterium count");

            foreach (var criterium in criteria)
            {
                iterator.Add(criterium);
                Assert.IsTrue(iterator.Contains(criterium), "Missing criterium");
            }

            // Check that we have all the criteria
            Assert.AreEqual(criteria.Count, iterator.NumberOfCriteria, "Incorrect criterium count");
            var enumerator = iterator.StoredStopCriteria;
            while (enumerator.MoveNext())
            {
                var criterium = enumerator.Current;
                Assert.IsTrue(criteria.Exists( c => ReferenceEquals(c, criterium)), "Criterium missing");
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void RemoveWithNullStopCriterium()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            var iterator = new Iterator(criteria);
            Assert.AreEqual(criteria.Count, iterator.NumberOfCriteria, "Incorrect criterium count");

            iterator.Remove(null);
        }

        [Test]
        [MultipleAsserts]
        public void RemoveWithNonExistingStopCriterium()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                           };
            var iterator = new Iterator(criteria);
            Assert.AreEqual(criteria.Count, iterator.NumberOfCriteria, "Incorrect criterium count");

            iterator.Remove(new ResidualStopCriterium());
            Assert.AreEqual(criteria.Count, iterator.NumberOfCriteria, "Incorrect criterium count");
        }

        [Test]
        [MultipleAsserts]
        public void Remove()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            var iterator = new Iterator(criteria);
            Assert.AreEqual(criteria.Count, iterator.NumberOfCriteria, "Incorrect criterium count");

            foreach (var criterium in criteria)
            {
                iterator.Remove(criterium);
                Assert.IsFalse(iterator.Contains(criterium), "Did not remove the criterium");
            }
        }

        [Test]
        [ExpectedArgumentException]
        public void DetermineStatusWithoutStopCriteria()
        {
            var iterator = new Iterator();
            iterator.DetermineStatus(0,
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 5),
                                     new DenseVector(3, 6));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void DetermineStatusWithNegativeIterationNumber()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            var iterator = new Iterator(criteria);

            iterator.DetermineStatus(-1,
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 5),
                                     new DenseVector(3, 6));
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullSolutionVector()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            var iterator = new Iterator(criteria);

            iterator.DetermineStatus(1,
                                     null,
                                     new DenseVector(3, 5),
                                     new DenseVector(3, 6));
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullSourceVector()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            var iterator = new Iterator(criteria);

            iterator.DetermineStatus(1,
                                     new DenseVector(3, 5),
                                     null,
                                     new DenseVector(3, 6));
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DetermineStatusWithNullResidualVector()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            var iterator = new Iterator(criteria);

            iterator.DetermineStatus(1,
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 5),
                                     null);
        }

        [Test]
        [MultipleAsserts]
        public void DetermineStatus()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(1)
                           };

            var iterator = new Iterator(criteria);

            // First step, nothing should happen.
            iterator.DetermineStatus(0,
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 4));
            Assert.IsInstanceOfType(typeof(CalculationRunning), iterator.Status, "Incorrect status");

            // Second step, should run out of iterations.
            iterator.DetermineStatus(1,
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 4));
            Assert.IsInstanceOfType(typeof(CalculationStoppedWithoutConvergence), iterator.Status, "Incorrect status");
        }

        [Test]
        [MultipleAsserts]
        public void ResetToPrecalculationState()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(1)
                           };

            var iterator = new Iterator(criteria);

            // First step, nothing should happen.
            iterator.DetermineStatus(0,
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 4),
                                     new DenseVector(3, 4));
            Assert.IsInstanceOfType(typeof(CalculationRunning), iterator.Status, "Incorrect status");

            iterator.ResetToPrecalculationState();
            Assert.IsInstanceOfType(typeof(CalculationIndetermined), iterator.Status, "Incorrect status");
            Assert.IsInstanceOfType(typeof(CalculationIndetermined), criteria[0].Status, "Incorrect status");
            Assert.IsInstanceOfType(typeof(CalculationIndetermined), criteria[1].Status, "Incorrect status");
            Assert.IsInstanceOfType(typeof(CalculationIndetermined), criteria[2].Status, "Incorrect status");
        }

        [Test]
        [MultipleAsserts]
        public void Clone()
        {
            var criteria = new List<IIterationStopCriterium>
                           {
                               new FailureStopCriterium(),
                               new DivergenceStopCriterium(),
                               new IterationCountStopCriterium(),
                               new ResidualStopCriterium()
                           };
            
            var iterator = new Iterator(criteria);

            var clonedIterator = iterator.Clone();
            Assert.IsInstanceOfType(typeof(Iterator), clonedIterator, "Incorrect type");

            var clone = clonedIterator as Iterator;
            Assert.IsNotNull(clone);
            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(iterator.NumberOfCriteria, clone.NumberOfCriteria, "Incorrect criterium count");
            // ReSharper restore PossibleNullReferenceException

            var enumerator = clone.StoredStopCriteria;
            while (enumerator.MoveNext())
            {
                var criterium = enumerator.Current;
                Assert.IsTrue(criteria.Exists(c => c.GetType().Equals(criterium.GetType())), "Criterium missing");
            }
        }
    }
}
