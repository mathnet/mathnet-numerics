// <copyright file="IteratorTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Solvers
{
    using System;
    using System.Collections.Generic;
    using LinearAlgebra.Generic.Solvers.Status;
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.Solvers;
    using LinearAlgebra.Single.Solvers.StopCriterium;
    using NUnit.Framework;

    /// <summary>
    /// Iterator tests
    /// </summary>
    [TestFixture]
    public class IteratorTest
    {
        /// <summary>
        /// Can create with <c>null</c> collection.
        /// </summary>
        [Test]
        public void CreateWithNullCollection()
        {
            var iterator = new Iterator(null);
            Assert.IsNotNull(iterator, "Should have an iterator");
            Assert.AreEqual(0, iterator.NumberOfCriteria, "There shouldn't be any criteria");
        }

        /// <summary>
        /// Can create with empty collection.
        /// </summary>
        [Test]
        public void CreateWithEmptyCollection()
        {
            var iterator = new Iterator(new IIterationStopCriterium[] {});
            Assert.IsNotNull(iterator, "Should have an iterator");
            Assert.AreEqual(0, iterator.NumberOfCriteria, "There shouldn't be any criteria");
        }

        /// <summary>
        /// Can create with collection with <c>nulls</c>.
        /// </summary>
        [Test]
        public void CreateWithCollectionWithNulls()
        {
            var iterator = new Iterator(new IIterationStopCriterium[] {null, null});
            Assert.IsNotNull(iterator, "Should have an iterator");
            Assert.AreEqual(0, iterator.NumberOfCriteria, "There shouldn't be any criteria");
        }

        /// <summary>
        /// Create with duplicates throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void CreateWithDuplicatesThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Iterator(new IIterationStopCriterium[]
                {
                    new FailureStopCriterium(),
                    new FailureStopCriterium()
                }));
        }

        /// <summary>
        /// Can create with collection.
        /// </summary>
        [Test]
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
            foreach (var criterium in iterator.StoredStopCriteria)
            {
                Assert.IsTrue(criteria.Exists(c => ReferenceEquals(c, criterium)), "Criterium missing");
            }
        }

        /// <summary>
        /// Add with <c>null</c> stop criterium throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void AddWithNullStopCriteriumThrowsArgumentNullException()
        {
            var iterator = new Iterator();
            Assert.Throws<ArgumentNullException>(() => iterator.Add(null));
        }

        /// <summary>
        /// Add with existing stop criterium throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void AddWithExistingStopCriteriumThrowsArgumentException()
        {
            var iterator = new Iterator();
            iterator.Add(new FailureStopCriterium());
            Assert.AreEqual(1, iterator.NumberOfCriteria, "Incorrect criterium count");

            Assert.Throws<ArgumentException>(() => iterator.Add(new FailureStopCriterium()));
        }

        /// <summary>
        /// Can add criterium.
        /// </summary>
        [Test]
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
            foreach (var criterium in iterator.StoredStopCriteria)
            {
                Assert.IsTrue(criteria.Exists(c => ReferenceEquals(c, criterium)), "Criterium missing");
            }
        }

        /// <summary>
        /// Remove with <c>null</c> stop criterium throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void RemoveWithNullStopCriteriumThrowsArgumentNullException()
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

            Assert.Throws<ArgumentNullException>(() => iterator.Remove(null));
        }

        /// <summary>
        /// Can remove with non-existing stop criterium.
        /// </summary>
        [Test]
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

        /// <summary>
        /// Can remove.
        /// </summary>
        [Test]
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

        /// <summary>
        /// Determine status without stop criteria throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithoutStopCriteriaThrowsArgumentException()
        {
            var iterator = new Iterator();
            Assert.Throws<ArgumentException>(() => iterator.DetermineStatus(
                0,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with negative iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNegativeIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criteria = new List<IIterationStopCriterium>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium(),
                    new ResidualStopCriterium()
                };
            var iterator = new Iterator(criteria);

            Assert.Throws<ArgumentOutOfRangeException>(() => iterator.DetermineStatus(
                -1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with <c>null</c> solution vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNullSolutionVectorThrowsArgumentNullException()
        {
            var criteria = new List<IIterationStopCriterium>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium(),
                    new ResidualStopCriterium()
                };
            var iterator = new Iterator(criteria);

            Assert.Throws<ArgumentNullException>(() => iterator.DetermineStatus(
                1,
                null,
                DenseVector.Create(3, i => 5),
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with <c>null</c> source vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNullSourceVectorThrowsArgumentNullException()
        {
            var criteria = new List<IIterationStopCriterium>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium(),
                    new ResidualStopCriterium()
                };
            var iterator = new Iterator(criteria);

            Assert.Throws<ArgumentNullException>(() => iterator.DetermineStatus(
                1,
                DenseVector.Create(3, i => 5),
                null,
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with <c>null</c> residual vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNullResidualVectorThrowsArgumentNullException()
        {
            var criteria = new List<IIterationStopCriterium>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium(),
                    new ResidualStopCriterium()
                };
            var iterator = new Iterator(criteria);

            Assert.Throws<ArgumentNullException>(() => iterator.DetermineStatus(
                1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                null));
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
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
            iterator.DetermineStatus(
                0,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4));
            Assert.IsInstanceOf(typeof (CalculationRunning), iterator.Status, "Incorrect status");

            // Second step, should run out of iterations.
            iterator.DetermineStatus(
                1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4));
            Assert.IsInstanceOf(typeof (CalculationStoppedWithoutConvergence), iterator.Status, "Incorrect status");
        }

        /// <summary>
        /// Can reset to precalculation state.
        /// </summary>
        [Test]
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
            iterator.DetermineStatus(
                0,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4));
            Assert.IsInstanceOf(typeof (CalculationRunning), iterator.Status, "Incorrect status");

            iterator.ResetToPrecalculationState();
            Assert.IsInstanceOf(typeof (CalculationIndetermined), iterator.Status, "Incorrect status");
            Assert.IsInstanceOf(typeof (CalculationIndetermined), criteria[0].Status, "Incorrect status");
            Assert.IsInstanceOf(typeof (CalculationIndetermined), criteria[1].Status, "Incorrect status");
            Assert.IsInstanceOf(typeof (CalculationIndetermined), criteria[2].Status, "Incorrect status");
        }

        /// <summary>
        /// Can clone.
        /// </summary>
        [Test]
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
            Assert.IsInstanceOf(typeof (Iterator), clonedIterator, "Incorrect type");

            var clone = clonedIterator as Iterator;
            Assert.IsNotNull(clone);

            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(iterator.NumberOfCriteria, clone.NumberOfCriteria, "Incorrect criterium count");

            // ReSharper restore PossibleNullReferenceException
            foreach (var criterium in clone.StoredStopCriteria)
            {
                Assert.IsTrue(criteria.Exists(c => c.GetType() == criterium.GetType()), "Criterium missing");
            }
        }
    }
}
