// <copyright file="IterationCountStopCriteriumTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.StopCriterium
{
    using System;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Solvers.StopCriterium;
    using LinearAlgebra.Generic.Solvers.Status;
    using NUnit.Framework;

    /// <summary>
    /// Iteration count stop criterium tests.
    /// </summary>
    [TestFixture]
    public sealed class IterationCountStopCriteriumTest
    {
        /// <summary>
        /// Create with illegal minimum iterations throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithIllegalMinimumIterationsThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new IterationCountStopCriterium(-1));
        }

        /// <summary>
        /// Can create.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");
        }

        /// <summary>
        /// Can reset maximum iterations.
        /// </summary>
        [Test]
        public void ResetMaximumIterations()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");
            Assert.AreEqual(10, criterium.MaximumNumberOfIterations, "Incorrect maximum number of iterations");

            criterium.ResetMaximumNumberOfIterationsToDefault();
            Assert.AreNotEqual(10, criterium.MaximumNumberOfIterations, "Should have reset");
            Assert.AreEqual(IterationCountStopCriterium.DefaultMaximumNumberOfIterations, criterium.MaximumNumberOfIterations, "Reset to the wrong value");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");

            Assert.Throws<ArgumentOutOfRangeException>(() => criterium.DetermineStatus(-1, DenseVector.Create(3, i => 1), DenseVector.Create(3, i => 2), DenseVector.Create(3, i => 3)));
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
        public void DetermineStatus()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");

            criterium.DetermineStatus(5, DenseVector.Create(3, i => 1), DenseVector.Create(3, i => 2), DenseVector.Create(3, i => 3));
            Assert.IsInstanceOf(typeof (CalculationRunning), criterium.Status, "Should be running");

            criterium.DetermineStatus(10, DenseVector.Create(3, i => 1), DenseVector.Create(3, i => 2), DenseVector.Create(3, i => 3));
            Assert.IsInstanceOf(typeof (CalculationStoppedWithoutConvergence), criterium.Status, "Should be finished");
        }

        /// <summary>
        /// Can reset calculation state.
        /// </summary>
        [Test]
        public void ResetCalculationState()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");

            criterium.DetermineStatus(5, DenseVector.Create(3, i => 1), DenseVector.Create(3, i => 2), DenseVector.Create(3, i => 3));
            Assert.IsInstanceOf(typeof (CalculationRunning), criterium.Status, "Should be running");

            criterium.ResetToPrecalculationState();
            Assert.IsInstanceOf(typeof (CalculationIndetermined), criterium.Status, "Should not have started");
        }

        /// <summary>
        /// Can clone a stop criterium.
        /// </summary>
        [Test]
        public void Clone()
        {
            var criterium = new IterationCountStopCriterium(10);
            Assert.IsNotNull(criterium, "A criterium should have been created");
            Assert.AreEqual(10, criterium.MaximumNumberOfIterations, "Incorrect maximum");

            var clone = criterium.Clone();
            Assert.IsInstanceOf(typeof (IterationCountStopCriterium), clone, "Wrong criterium type");

            var clonedCriterium = clone as IterationCountStopCriterium;
            Assert.IsNotNull(clonedCriterium);

            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(criterium.MaximumNumberOfIterations, clonedCriterium.MaximumNumberOfIterations, "Clone failed");

            // ReSharper restore PossibleNullReferenceException
        }
    }
}
